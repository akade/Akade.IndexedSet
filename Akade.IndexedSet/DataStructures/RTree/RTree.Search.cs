namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{
    public IEnumerable<TElement> IntersectWith(TEnvelope aabb)
    {
        List<TElement> results = [];

        Stack<Node> stack = new();
        stack.Push(_root);

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop();
            if (currentNode is ParentNode parentNode)
            {
                if (!parentNode.HasInitializedEnvelope)
                {
                    continue;
                }
                if (TEnvelopeMath.Intersects(parentNode.GetEnvelope(_getAABB), aabb))
                {
                    foreach (Node child in parentNode.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
            else if (currentNode is LeafNode leafNode)
            {
                if (TEnvelopeMath.Intersects(leafNode.GetEnvelope(_getAABB), aabb))
                {
                    results.Add(leafNode.Element);
                }
            }

        }
        return results;
    }

    public IEnumerable<(TElement element, TValue distance)> GetNearestNeighbours(TPoint position)
    {
        PriorityQueue<Node, TValue> queue = new();
        queue.Enqueue(_root, TValue.Zero);

        int count = 0;

        while (queue.TryDequeue(out Node? currentNode, out TValue distance))
        {
            count++;
            Console.WriteLine($"Processing node {count} with distance {distance}");
            if (currentNode is ParentNode parentNode)
            {
                foreach (Node child in parentNode.Children)
                {
                    TEnvelope childAABB = child.GetEnvelope(_getAABB);

                    TValue childDistance = TEnvelopeMath.DistanceToBoundary(childAABB, position);

                    queue.Enqueue(child, childDistance);
                }
            }
            else if (currentNode is LeafNode leafNode)
            {
                yield return (leafNode.Element, distance);
            }
        }
    }
}
