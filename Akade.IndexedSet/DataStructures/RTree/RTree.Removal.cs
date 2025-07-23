#if NET9_0_OR_GREATER


namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TValue>
{
    public bool Remove(TElement element)
    {
        AABB<TValue> aabb = _getAABB(element);

        Dictionary<Node<TElement, TValue>, ParentNode<TElement, TValue>> parentByNode = [];

        Stack<ParentNode<TElement, TValue>> stack = new();
        stack.Push(_root);

        Node<TElement, TValue>? targetNode = null;


        while (stack.TryPop(out ParentNode<TElement, TValue>? currentNode))
        {
            if (currentNode.GetAABB(_getAABB).Intersects(aabb))
            {
                foreach (Node<TElement, TValue> child in currentNode.Children)
                {
                    parentByNode[child] = currentNode;

                    if (child is ParentNode<TElement, TValue> childParentNode)
                    {
                        stack.Push(childParentNode);
                    }
                    else if (child is LeafNode<TElement, TValue> leafNode && (leafNode.Element?.Equals(element) ?? false))
                    {
                        targetNode = leafNode;
                        // We found the target node, no need to continue searching
                        stack.Clear();
                        break;
                    }
                }
            }
        }

        if (targetNode == null)
        {
            return false; // Element not found
        }

        Count--;

        ParentNode<TElement, TValue>? parentNode = parentByNode[targetNode];
        _ = parentNode.Children.Remove(targetNode);
        parentNode.RecalculateAABB(_getAABB);

        // We use a very simple remove strategy:
        // - Remove parent nodes up the path if any underflow occurs
        // - Reinsert all their children
        // There is some room for improvement here down the line
        List<LeafNode<TElement, TValue>> nodesForReinsertion = [];
        while (parentNode is not null && parentNode != _root)
        {
            if (parentNode.Children.Count < _settings.MinNodeEntries && parentNode != _root)
            {
                AddAllLeafNodes(parentNode, nodesForReinsertion);
                ParentNode<TElement, TValue> parentOfParentNode = parentByNode[parentNode];
                _ = parentOfParentNode.Children.Remove(parentNode);
                parentOfParentNode.RecalculateAABB(_getAABB);
                parentNode = parentOfParentNode;
            }
            else
            {
                parentNode = null;
            }
        }

        foreach (LeafNode<TElement, TValue> node in nodesForReinsertion)
        {
            Insert(node.Element);
        }


        return true;
    }

    private static void AddAllLeafNodes(ParentNode<TElement, TValue> parentNode, List<LeafNode<TElement, TValue>> nodesForReinsertion)
    {
        foreach (Node<TElement, TValue> child in parentNode.Children)
        {
            if (child is LeafNode<TElement, TValue> leafNode)
            {
                nodesForReinsertion.Add(leafNode);
            }
            else if (child is ParentNode<TElement, TValue> childParentNode)
            {
                RTree<TElement, TValue>.AddAllLeafNodes(childParentNode, nodesForReinsertion);
            }
        }
    }
}

#endif
