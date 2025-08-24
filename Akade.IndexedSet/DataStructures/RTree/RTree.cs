using System.Diagnostics;
using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

/// <summary>
/// Implemented based on https://github.com/georust/rstar/blob/master/rstar/src/algorithm/rstar.rs (MIT/Apache-2.0 License).
/// Many thanks for the legwork
/// </summary>

// TODO: explore on how to specialize with minimal code duplication for Vector2 & Vector3
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
    where TPoint : struct
    where TEnvelope : struct
    where TEnvelopeMath : struct, IEnvelopeMath<TPoint, TEnvelope, TValue>
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    private readonly Func<TElement, TEnvelope> _getAABB;
    private readonly int _dimensions;
    private readonly RTreeSettings _settings;
    private ParentNode _root = new();
    private readonly Comparison<Node>[] _axisComparers;

    internal RTree(Func<TElement, TEnvelope> getAABB, int dimensions, RTreeSettings settings)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dimensions, 1);
        settings.Validate();
        _getAABB = getAABB;
        _dimensions = dimensions;
        _settings = settings;

        // pre allocate axis comparers for each axis
        _axisComparers = Enumerable.Range(0, _dimensions).Select(axis => new Comparison<Node>((x, y) =>
        {
            TEnvelope aabbX = x.GetEnvelope(_getAABB);
            TEnvelope aabbY = y.GetEnvelope(_getAABB);

            TValue a = TEnvelopeMath.GetMin(aabbX, axis);
            TValue b = TEnvelopeMath.GetMin(aabbY, axis);

            return a.CompareTo(b);
        })).ToArray();
    }

    public int Count { get; private set; }

    internal void Clear()
    {
        _root = new ParentNode();
        Count = 0;
    }

    [Conditional("TEST")]
    internal void CheckForCorruption(bool isBulkLoaded)
    {
        Stack<ParentNode> stack = new();
        stack.Push(_root);

        int count = 0;

        while (stack.TryPop(out ParentNode? node))
        {
            if (!node.HasInitializedEnvelope)
            {
                throw new InvalidOperationException("Node has an uninitialized AABB, which is not allowed.");
            }
            //if ((_root != node && !isBulkLoaded && node.Children.Count < _settings.MinNodeEntries) || node.Children.Count > _settings.MaxNodeEntries)
            //{
            //    throw new InvalidOperationException($"Node has an invalid number of children: {node.Children.Count} [{_settings.MinNodeEntries} - {_settings.MaxNodeEntries}]");
            //}

            TEnvelope parent = node.GetEnvelope(_getAABB);

            foreach (Node child in node.Children)
            {
                TEnvelope aabb = child.GetEnvelope(_getAABB);

                if (!TEnvelopeMath.Contains(parent, aabb))
                {
                    throw new InvalidOperationException($"Child AABB {TEnvelopeMath.ToString(aabb)} is not contained in parent AABB {TEnvelopeMath.ToString(parent)}.");
                }

                if (child is ParentNode parentChild)
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

    public TValue OverlapArea(ParentNode node)
    {
        TValue totalOverlap = TValue.Zero;

        for (int i = 0; i < node.Children.Count; i++)
        {
            TEnvelope aabbA = node.Children[i].GetEnvelope(_getAABB);
            for (int j = i + 1; j < node.Children.Count; j++)
            {
                TEnvelope aabbB = node.Children[j].GetEnvelope(_getAABB);
                totalOverlap += TEnvelopeMath.IntersectionArea(aabbA, aabbB);
            }
        }
        return totalOverlap;
    }

    public TValue TotalOverlapArea()
    { 
        TValue totalOverlap = TValue.Zero;
        Stack<ParentNode> stack = new();
        stack.Push(_root);

        while (stack.TryPop(out ParentNode? node))
        {
            totalOverlap += OverlapArea(node);
            foreach (Node child in node.Children)
            {
                if (child is ParentNode parentChild)
                {
                    stack.Push(parentChild);
                }
            }
        }
        return totalOverlap;
    }
}
