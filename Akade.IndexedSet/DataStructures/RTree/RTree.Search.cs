#if NET9_0_OR_GREATER

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

}

#endif