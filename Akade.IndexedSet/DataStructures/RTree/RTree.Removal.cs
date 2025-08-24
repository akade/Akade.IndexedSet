namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{
    public bool Remove(TElement element)
    {
        TEnvelope aabb = _getAABB(element);

        Dictionary<Node, ParentNode> parentByNode = [];

        Stack<ParentNode> stack = new();
        stack.Push(_root);

        Node? targetNode = null;


        while (stack.TryPop(out ParentNode? currentNode))
        {
            if (TEnvelopeMath.Intersects(ref currentNode.Envelope, ref aabb))
            {
                foreach (Node child in currentNode.Children)
                {
                    parentByNode[child] = currentNode;

                    if (child is ParentNode childParentNode)
                    {
                        stack.Push(childParentNode);
                    }
                    else if (child is LeafNode leafNode && (leafNode.Element?.Equals(element) ?? false))
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

        ParentNode? parentNode = parentByNode[targetNode];
        _ = parentNode.Children.Remove(targetNode);
        parentNode.RecalculateAABB();

        // We use a very simple remove strategy:
        // - Remove parent nodes up the path if any underflow occurs
        // - Reinsert all their children
        // There is some room for improvement here down the line
        List<LeafNode> nodesForReinsertion = [];
        while (parentNode is not null && parentNode != _root)
        {
            if (parentNode.Children.Count < _settings.MinNodeEntries)
            {
                AddAllLeafNodes(parentNode, nodesForReinsertion);
                ParentNode parentOfParentNode = parentByNode[parentNode];
                _ = parentOfParentNode.Children.Remove(parentNode);
                parentOfParentNode.RecalculateAABB();
                parentNode = parentOfParentNode;
            }
            else
            {
                parentNode = null;
            }
        }
        Count -= nodesForReinsertion.Count;
        foreach (LeafNode node in nodesForReinsertion)
        {
            Insert(node.Element);
        }


        return true;
    }

    private static void AddAllLeafNodes(ParentNode parentNode, List<LeafNode> nodesForReinsertion)
    {
        foreach (Node child in parentNode.Children)
        {
            if (child is LeafNode leafNode)
            {
                nodesForReinsertion.Add(leafNode);
            }
            else if (child is ParentNode childParentNode)
            {
                AddAllLeafNodes(childParentNode, nodesForReinsertion);
            }
        }
    }
}
