#if NET9_0_OR_GREATER
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;

/// <summary>
/// Implemented based on https://github.com/georust/rstar/blob/master/rstar/src/algorithm/rstar.rs (MIT/Apache-2.0 License).
/// Many thanks for the legwork
/// </summary>

// TODO: explore on how to specialize with minimal code duplication for Vector2 & Vector3
internal sealed partial class RTree<TElement, TValue>
    where TElement : notnull
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    private readonly Func<TElement, AABB<TValue>> _getAABB;
    private readonly int _dimensions;
    private readonly RTreeSettings _settings;
    private ParentNode<TElement, TValue> _root = new();

    internal RTree(Func<TElement, AABB<TValue>> getAABB, int dimensions, RTreeSettings settings)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimensions, 1);
        settings.Validate();
        _getAABB = getAABB;
        _dimensions = dimensions;
        _settings = settings;
    }

    public int Count { get; private set; }


    [Conditional("Test")]
    internal void CheckForCorruption()
    {
        Span<TValue> buffer = stackalloc TValue[_dimensions * 2];

        Stack<ParentNode<TElement, TValue>> stack = new();
        stack.Push(_root);

        int count = 0;

        while (stack.TryPop(out ParentNode<TElement, TValue>? node))
        {
            if (node.IsEmptyAABB)
            {
                throw new InvalidOperationException("Node has an empty AABB, which is not allowed.");
            }
            if ((_root != node && node.Children.Count < _settings.MinNodeEntries) || node.Children.Count > _settings.MaxNodeEntries)
            {
                throw new InvalidOperationException($"Node has an invalid number of children: {node.Children.Count}");
            }

            AABB<TValue> parent = node.GetAABB(_getAABB);

            foreach (Node<TElement, TValue> child in node.Children)
            {
                AABB<TValue> aabb = child.GetAABB(_getAABB);

                if (!parent.Contains(aabb))
                {
                    throw new InvalidOperationException($"Child AABB {aabb.ToString()} is not contained in parent AABB {parent.ToString()}.");
                }

                if (child is ParentNode<TElement, TValue> parentChild)
                {
                    stack.Push(parentChild);
                }
                else
                {
                    count++;
                }
            }
        }

        if (count != Count)
        {
            throw new InvalidOperationException($"Count mismatch: Expected {Count}, but found {count} leaf nodes.");
        }
    }
}

#endif 
