#if NET9_0_OR_GREATER

using System.Numerics.Tensors;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TValue>
{
    public IEnumerable<TElement> IntersectWith(AABB<TValue> aabb)
    {
        List<TElement> results = [];

        Stack<Node<TElement, TValue>> stack = new();
        stack.Push(_root);

        while (stack.Count > 0)
        {
            Node<TElement, TValue> currentNode = stack.Pop();
            if (currentNode is ParentNode<TElement, TValue> parentNode)
            {
                if (parentNode.IsEmptyAABB)
                {
                    continue;
                }
                if (parentNode.GetAABB(_getAABB).Intersects(aabb))
                {
                    foreach (Node<TElement, TValue> child in parentNode.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
            else if (currentNode is LeafNode<TElement, TValue> leafNode)
            {
                if (leafNode.GetAABB(_getAABB).Intersects(aabb))
                {
                    results.Add(leafNode.Element);
                }
            }

        }
        return results;
    }

    public IEnumerable<(TElement element, TValue distance)> GetNearestNeighbours(Memory<TValue> position)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(position.Length, _dimensions, nameof(position));

        PriorityQueue<Node<TElement, TValue>, TValue> queue = new();
        queue.Enqueue(_root, TValue.Zero);

        int count = 0;

        while (queue.TryDequeue(out Node<TElement, TValue>? currentNode, out TValue distance))
        {
            count++;
            Console.WriteLine($"Processing node {count} with distance {distance}");
            if (currentNode is ParentNode<TElement, TValue> parentNode)
            {
                foreach (Node<TElement, TValue> child in parentNode.Children)
                {
                    AABB<TValue> childAABB = child.GetAABB(_getAABB);

                    TValue childDistance = childAABB.BoundaryDistance(position.Span);

                    queue.Enqueue(child, childDistance);
                }
            }
            else if (currentNode is LeafNode<TElement, TValue> leafNode)
            {
                yield return (leafNode.Element, distance);
            }
        }
    }
}

#endif