using Akade.IndexedSet.Utils;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{
    public IEnumerable<TElement> IntersectWith(TEnvelope aabb)
    {
        if (Count == 0 || !TEnvelopeMath.Intersects(ref _root.Envelope, ref aabb))
        {
            return [];
        }

        List<TElement> results = new(16);
        Stack<ParentNode> stack = ObjectPool<Stack<ParentNode>>.Instance.Rent();

        try
        {
            stack.Push(_root);

            while (stack.TryPop(out ParentNode? currentNode))
            {
                if (!currentNode.HasInitializedEnvelope)
                {
                    continue;
                }
                foreach (Node child in CollectionsMarshal.AsSpan(currentNode.Children))
                {
                    if (TEnvelopeMath.Intersects(ref child.Envelope, ref aabb))
                    {
                        if (child is ParentNode childParentNode)
                        {
                            stack.Push(childParentNode);
                        }
                        else if (child is LeafNode leafNode)
                        {
                            results.Add(leafNode.Element);
                        }
                    }
                }
            }
        }
        finally
        {
            stack.Clear();
            ObjectPool<Stack<ParentNode>>.Instance.Return(stack);
        }

        return results;
    }

    public IEnumerable<(TElement element, TValue distance)> GetNearestNeighbours(TPoint position)
    {
        PriorityQueue<Node, TValue> queue = ObjectPool<PriorityQueue<Node, TValue>>.Instance.Rent();
        try
        {
            queue.Enqueue(_root, TValue.Zero);

            int count = 0;

            while (queue.TryDequeue(out Node? currentNode, out TValue distance))
            {
                count++;
                // Console.WriteLine($"Processing node {count} with distance {distance}");
                if (currentNode is ParentNode parentNode)
                {
#if NET9_0_OR_GREATER
                    foreach (Node child in CollectionsMarshal.AsSpan(parentNode.Children))
#else
                    foreach (Node child in parentNode.Children)
#endif
                    {
                        TValue childDistance = TEnvelopeMath.DistanceToBoundary(ref child.Envelope, position);

                        queue.Enqueue(child, childDistance);
                    }
                }
                else if (currentNode is LeafNode leafNode)
                {
                    yield return (leafNode.Element, distance);
                }
            }
        }
        finally
        {
            queue.Clear();
            ObjectPool<PriorityQueue<Node, TValue>>.Instance.Return(queue);
        }
    }
}
