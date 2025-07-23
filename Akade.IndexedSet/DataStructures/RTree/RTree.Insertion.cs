#if NET9_0_OR_GREATER
using System.Diagnostics;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TValue>
{
    internal void Insert(TElement element)
    {
        Insert(new LeafNode<TElement, TValue>(element));
    }

    internal void Insert(LeafNode<TElement, TValue> leafToInsert)
    {
        Count++;

        InsertionResult first = RecursiveInsert(_root, leafToInsert, 0);
        int targetHeight = 0;
        Stack<InsertionAction<TElement, TValue>> insertion_stack = new();

        switch (first)
        {
            case InsertionResultComplete:
                return;
            case InsertionResultSplit<TElement, TValue> split:
                insertion_stack.Push(new InsertionAction<TElement, TValue>(InsertionActionType.Split, split.Node));

                break;
            case InsertionResultReinsert<TElement, TValue> reinsert:
                for (int i = 0; i < reinsert.Nodes.Count; i++)
                {
                    insertion_stack.Push(new InsertionAction<TElement, TValue>(InsertionActionType.Reinsert, reinsert.Nodes[i]));
                }
                targetHeight = reinsert.level;
                break;
            default:
                throw new InvalidOperationException("Should never happen: Bug?");
        }

        while (insertion_stack.TryPop(out InsertionAction<TElement, TValue> next))
        {
            switch (next)
            {
                case { ActionType: InsertionActionType.Split, Node: var node }:
                    ParentNode<TElement, TValue> newRoot = new();
                    ParentNode<TElement, TValue> oldRoot = _root;
                    _root = newRoot;
                    newRoot.Children.Add(oldRoot);
                    newRoot.Children.Add(node);
                    newRoot.RecalculateAABB(_getAABB);
                    targetHeight += 1;

                    break;
                case { ActionType: InsertionActionType.Reinsert, Node: var nodeToReinsert }:
                    InsertionResult reinsertResult = ForcedInsertion(_root, nodeToReinsert, targetHeight);

                    switch (reinsertResult)
                    {
                        case InsertionResultComplete:
                            break; ;
                        case InsertionResultSplit<TElement, TValue> split:
                            insertion_stack.Push(new InsertionAction<TElement, TValue>(InsertionActionType.Split, split.Node));
                            break;
                        case InsertionResultReinsert<TElement, TValue>:
                        default:
                            throw new InvalidOperationException("Should never happen: Bug?");
                    }

                    break;
            }
        }

        CheckForCorruption();
    }


    private InsertionResult ForcedInsertion(ParentNode<TElement, TValue> node, Node<TElement, TValue> nodeToReinsert, int targetHeight)
    {
        node.MergeAABB(nodeToReinsert.GetAABB(_getAABB));
        int expandIndex = ChooseSubTree(node, nodeToReinsert);

        if (targetHeight == 0 || node.Children.Count < expandIndex)
        {
            node.Children.Add(nodeToReinsert);
            return ResolveOverflowWithoutReinsertion(node);
        }

        var follow = (ParentNode<TElement, TValue>)node.Children[expandIndex];
        InsertionResult expand = ForcedInsertion(follow, nodeToReinsert, targetHeight - 1);

        return expand switch
        {
            InsertionResultSplit<TElement, TValue> { Node: var child } => SplitWithoutReinsertion(node, child),
            _ => expand
        };
    }

    private InsertionResult SplitWithoutReinsertion(ParentNode<TElement, TValue> node, Node<TElement, TValue> child)
    {
        node.MergeAABB(child.GetAABB(_getAABB));
        node.Children.Add(child);
        return ResolveOverflowWithoutReinsertion(node);
    }


    private InsertionResult RecursiveInsert(ParentNode<TElement, TValue> node, LeafNode<TElement, TValue> leafNode, int currentHeight)
    {
        node.MergeAABB(_getAABB(leafNode.Element));

        int expandIndex = ChooseSubTree(node, leafNode);

        if (node.Children.Count < expandIndex)
        {
            node.Children.Add(leafNode);
            return ResolveOverflow(node, currentHeight);
        }

        var follow = (ParentNode<TElement, TValue>)node.Children[expandIndex];
        InsertionResult expand = RecursiveInsert(follow, leafNode, currentHeight + 1);

        return expand switch
        {
            InsertionResultSplit<TElement, TValue> { Node: var child } => Split(node, child, currentHeight),
            InsertionResultReinsert<TElement, TValue> reinsert => Reinsert(node, reinsert),
            InsertionResultComplete => InsertionResultComplete.Instance,
            _ => throw new InvalidOperationException("Should never happen: Bug?")
        };
    }

    private InsertionResultReinsert<TElement, TValue> Reinsert(ParentNode<TElement, TValue> node, InsertionResultReinsert<TElement, TValue> reinsert)
    {
        node.RecalculateAABB(_getAABB);
        return reinsert;
    }


    private InsertionResult Split(ParentNode<TElement, TValue> node, Node<TElement, TValue> child, int currentHeight)
    {
        node.MergeAABB(child.GetAABB(_getAABB));
        node.Children.Add(child);
        return ResolveOverflow(node, currentHeight);
    }


    private int ChooseSubTree(ParentNode<TElement, TValue> node, Node<TElement, TValue> toInsert)
    {
        Node<TElement, TValue>? firstChild = node.Children.FirstOrDefault();
        if (firstChild is null or LeafNode<TElement, TValue>)
        {
            return int.MaxValue;
        }

        bool allLeaves = firstChild is ParentNode<TElement, TValue> { Children: [LeafNode<TElement, TValue>, ..] };

        AABB <TValue> insertionAABB = toInsert.GetAABB(_getAABB);

        int inclusionCount = 0;
        TValue minArea = TValue.MaxValue;
        int minIndex = -1;
        
        Span<Node<TElement, TValue>> children = CollectionsMarshal.AsSpan(node.Children);

        for (int index = 0; index < children.Length; index++)
        {
            Node<TElement, TValue> child = children[index];

            AABB<TValue> childAABB = child.GetAABB(_getAABB);

            if (childAABB.Contains(insertionAABB))
            {
                inclusionCount++;

                TValue childArea = childAABB.Area();
                if (childArea < minArea)
                {
                    minArea = childArea;
                    minIndex = index;
                }
            }
        }

        if (inclusionCount == 0) // check until here, one thing found
        {
            TValue minOverlapIncrease = TValue.Zero;
            TValue minAreaIncrease = TValue.Zero;
            minArea = TValue.Zero;

            Span<TValue> buffer = stackalloc TValue[_dimensions * 2];

            for (int index = 0; index < node.Children.Count; index++)
            {
                Node<TElement, TValue> child = node.Children[index];

                AABB<TValue> aabb = child.GetAABB(_getAABB);
                aabb.CopyTo(buffer);
                insertionAABB.MergeInto(buffer);

                var newAABB = AABB<TValue>.CreateFromCombinedBuffer(buffer);

                TValue overlapIncrease = TValue.Zero;

                if (allLeaves)
                {
                    TValue overlap = TValue.Zero;
                    TValue newOverlap = TValue.Zero;

                    foreach (Node<TElement, TValue> otherChild in node.Children)
                    {
                        if (otherChild == child)
                        {
                            continue;
                        }

                        AABB<TValue> otherAABB = otherChild.GetAABB(_getAABB);

                        overlap += aabb.IntersectionArea(otherAABB);
                        newOverlap += newAABB.IntersectionArea(otherAABB);
                    }

                    overlapIncrease = newOverlap - overlap;
                }

                TValue area = newAABB.Area();
                TValue areaIncrease = area - aabb.Area();

                if (index == 0 || (overlapIncrease < minOverlapIncrease && areaIncrease < minAreaIncrease && area < minArea))
                {
                    minOverlapIncrease = overlapIncrease;
                    minAreaIncrease = areaIncrease;
                    minArea = area;
                    minIndex = index;
                }
            }
        }

        return minIndex;
    }


    private InsertionResult ResolveOverflowWithoutReinsertion(ParentNode<TElement, TValue> node)
    {
        return node.Children.Count > _settings.MaxNodeEntries
            ? new InsertionResultSplit<TElement, TValue>(Split(node))
            : InsertionResultComplete.Instance;
    }

    private InsertionResult ResolveOverflow(ParentNode<TElement, TValue> node, int currentHeight)
    {
        if (_settings.ReinsertionCount == 0)
        {
            return ResolveOverflowWithoutReinsertion(node);
        }

        if (node.Children.Count > _settings.MaxNodeEntries)
        {
            List<Node<TElement, TValue>> reinsertionNodes = GetNodesForReinsertaion(node);
            return new InsertionResultReinsert<TElement, TValue>(reinsertionNodes, currentHeight);
        }
        else
        {
            return InsertionResultComplete.Instance;
        }
    }

    private Node<TElement, TValue> Split(ParentNode<TElement, TValue> node)
    {
        int axis = GetSplitAxis(node);

        Debug.Assert(node.Children.Count >= 2);

        node.Children.Sort((a, b) => a.GetAABB(_getAABB).Min[axis].CompareTo(b.GetAABB(_getAABB).Min[axis])); // TODO precreate a comparer for AABB<TValue> to avoid this allocation

        TValue bestOverlap = TValue.MaxValue;
        TValue bestArea = TValue.MaxValue;
        int minNodeEntries = _settings.MinNodeEntries;
        int bestIndex = minNodeEntries;

        Span<TValue> leftBuffer = stackalloc TValue[_dimensions * 2];
        Span<TValue> rightBuffer = stackalloc TValue[_dimensions * 2];
        Span<Node<TElement, TValue>> childSpan= CollectionsMarshal.AsSpan(node.Children);

        for (int k = minNodeEntries; k <= node.Children.Count - minNodeEntries; k++)
        {
            Node<TElement, TValue> firstChild = node.Children[k - 1];
            Node<TElement, TValue> secondChild = node.Children[k];
            AABB<TValue> firstAABB = firstChild.GetAABB(_getAABB);
            AABB<TValue> secondAABB = secondChild.GetAABB(_getAABB);
            firstAABB.CopyTo(leftBuffer);
            secondAABB.CopyTo(rightBuffer);

            foreach (Node<TElement, TValue> child in childSpan[..k])
            {
                child.GetAABB(_getAABB).MergeInto(leftBuffer);
            }

            foreach (Node<TElement, TValue> child in childSpan[k..])
            {
                child.GetAABB(_getAABB).MergeInto(rightBuffer);
            }

            var leftAABB = AABB<TValue>.CreateFromCombinedBuffer(leftBuffer);
            var rightAABB = AABB<TValue>.CreateFromCombinedBuffer(rightBuffer);

            TValue overlap = leftAABB.IntersectionArea(rightAABB);
            TValue area = leftAABB.Area() + rightAABB.Area();

            if (k == minNodeEntries || (overlap < bestOverlap && area < bestArea))
            {
                bestOverlap = overlap;
                bestArea = area;
                bestIndex = k;
            }
        }

        ParentNode<TElement, TValue> newNode = new(childSpan[bestIndex..], _getAABB);
        node.Children.RemoveRange(bestIndex, node.Children.Count - bestIndex);
        node.RecalculateAABB(_getAABB);
        return newNode;
    }

    private int GetSplitAxis(ParentNode<TElement, TValue> node)
    {
        TValue bestPerimeter = TValue.MaxValue;
        int bestAxis = 0;
        int minNodeEntries = _settings.MinNodeEntries;
        int until = node.Children.Count - minNodeEntries + 1;

        Span<TValue> left = stackalloc TValue[_dimensions * 2];
        Span<TValue> right = stackalloc TValue[_dimensions * 2];

        Span<TValue> leftModified = stackalloc TValue[_dimensions * 2];
        Span<TValue> rightModified = stackalloc TValue[_dimensions * 2];

        Span<Node<TElement, TValue>> childSpan = CollectionsMarshal.AsSpan(node.Children);

        for (int axis = 0; axis < _dimensions; axis++)
        {
            node.Children.Sort((a, b) => a.GetAABB(_getAABB).Min[axis].CompareTo(b.GetAABB(_getAABB).Min[axis]));

            // initialize the memory for left and right AABBs
            childSpan[0].GetAABB(_getAABB).CopyTo(left);
            childSpan[until].GetAABB(_getAABB).CopyTo(right);

            for (int i = 1; i < until; i++)
            {
                childSpan[i].GetAABB(_getAABB).MergeInto(left);
            }

            for (int i = until + 1; i < node.Children.Count; i++)
            {
                childSpan[i].GetAABB(_getAABB).MergeInto(right);
            }

            for (int k = minNodeEntries; k < until; k++)
            {
                left.CopyTo(leftModified);
                right.CopyTo(rightModified);

                foreach (Node<TElement, TValue> child in childSpan[..k])
                {
                    child.GetAABB(_getAABB).MergeInto(leftModified);
                }

                foreach (Node<TElement, TValue> child in childSpan[k..])
                {
                    child.GetAABB(_getAABB).MergeInto(rightModified);
                }

                var leftAABB = AABB<TValue>.CreateFromCombinedBuffer(leftModified);
                var rightAABB = AABB<TValue>.CreateFromCombinedBuffer(rightModified);

                // We can use the half perimeter as we are only interested in the relative size and are axis-aligned
                TValue perimeter = leftAABB.HalfPerimeter() + rightAABB.HalfPerimeter();

                if (perimeter < bestPerimeter)
                {
                    bestPerimeter = perimeter;
                    bestAxis = axis;
                }
            }
        }
        return bestAxis;
    }

    private List<Node<TElement, TValue>> GetNodesForReinsertaion(ParentNode<TElement, TValue> node)
    {
        // TODO: think about optimizing, maybe with an comparer that has preallocated buffers and can be initialized with the center of the AABB
        node.Children.Sort((left, right) =>
        {
            Span<TValue> center = stackalloc TValue[_dimensions];
            node.GetAABB(_getAABB).Center(center);

            Span<TValue> leftCenter = stackalloc TValue[_dimensions];
            Span<TValue> rightCenter = stackalloc TValue[_dimensions];

            left.GetAABB(_getAABB).Center(leftCenter);
            right.GetAABB(_getAABB).Center(rightCenter);

            TValue leftDistance = TensorPrimitives.Distance<TValue>(leftCenter, center);
            TValue rightDistance = TensorPrimitives.Distance<TValue>(rightCenter, center);

            return leftDistance.CompareTo(rightDistance);
        });

        Span<Node<TElement, TValue>> childSpan = CollectionsMarshal.AsSpan(node.Children);

        int splitOffStart = node.Children.Count - _settings.ReinsertionCount;
        List<Node<TElement, TValue>> reinsertionNodes = new(node.Children.Count - splitOffStart);
        reinsertionNodes.AddRange(childSpan[splitOffStart..]);
        node.Children.RemoveRange(splitOffStart, node.Children.Count - splitOffStart);

        node.RecalculateAABB(_getAABB);
        
        return reinsertionNodes;
    }
}


internal readonly record struct InsertionAction<TElement, TValue>(InsertionActionType ActionType, Node<TElement, TValue> Node)
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{

}

internal enum InsertionActionType
{
    Split,
    Reinsert
}

internal abstract record class InsertionResult;

internal sealed record class InsertionResultComplete : InsertionResult
{
    public static InsertionResultComplete Instance { get; } = new();
}

internal sealed record class InsertionResultSplit<TElement, TValue>(Node<TElement, TValue> Node) : InsertionResult
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>;

internal sealed record class InsertionResultReinsert<TElement, TValue>(List<Node<TElement, TValue>> Nodes, int level) : InsertionResult
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>;

#endif