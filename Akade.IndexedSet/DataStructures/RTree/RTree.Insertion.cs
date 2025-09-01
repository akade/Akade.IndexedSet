using System.Diagnostics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{
    internal void Insert(TElement element)
    {
        Insert(new LeafNode(element, _getAABB(element)));
    }

    internal void Insert(LeafNode leafToInsert)
    {
        Count++;

        InsertionResult first = RecursiveInsert(_root, leafToInsert, 0);
        int targetHeight = 0;
        Stack<InsertionAction> insertion_stack = new();

        switch (first)
        {
            case InsertionResultComplete:
                return;
            case InsertionResultSplit split:
                insertion_stack.Push(new InsertionAction(InsertionActionType.Split, split.Node));

                break;
            case InsertionResultReinsert reinsert:
                for (int i = 0; i < reinsert.Nodes.Count; i++)
                {
                    insertion_stack.Push(new InsertionAction(InsertionActionType.Reinsert, reinsert.Nodes[i]));
                }
                targetHeight = reinsert.level;
                break;
            default:
                throw new InvalidOperationException("Should never happen: Bug?");
        }

        while (insertion_stack.TryPop(out InsertionAction next))
        {
            switch (next)
            {
                case { ActionType: InsertionActionType.Split, Node: var node }:
                    ParentNode newRoot = new();
                    ParentNode oldRoot = _root;
                    _root = newRoot;
                    newRoot.Children.Add(oldRoot);
                    newRoot.Children.Add(node);
                    newRoot.RecalculateAABB();
                    targetHeight += 1;

                    break;
                case { ActionType: InsertionActionType.Reinsert, Node: var nodeToReinsert }:
                    InsertionResult reinsertResult = ForcedInsertion(_root, nodeToReinsert, targetHeight);

                    switch (reinsertResult)
                    {
                        case InsertionResultComplete:
                            break; ;
                        case InsertionResultSplit split:
                            insertion_stack.Push(new InsertionAction(InsertionActionType.Split, split.Node));
                            break;
                        case InsertionResultReinsert:
                        default:
                            throw new InvalidOperationException("Should never happen: Bug?");
                    }

                    break;
            }
        }

        CheckForCorruption(false);
    }

    private InsertionResult ForcedInsertion(ParentNode node, Node nodeToReinsert, int targetHeight)
    {
        node.MergeEnvelope(ref nodeToReinsert.Envelope);
        int expandIndex = ChooseSubTree(node, nodeToReinsert);

        if (targetHeight == 0 || node.Children.Count < expandIndex)
        {
            node.Children.Add(nodeToReinsert);
            return ResolveOverflowWithoutReinsertion(node);
        }

        var follow = (ParentNode)node.Children[expandIndex];
        InsertionResult expand = ForcedInsertion(follow, nodeToReinsert, targetHeight - 1);

        return expand switch
        {
            InsertionResultSplit { Node: var child } => SplitWithoutReinsertion(node, child),
            _ => expand
        };
    }

    private InsertionResult SplitWithoutReinsertion(ParentNode node, ParentNode child)
    {
        node.MergeEnvelope(ref child.Envelope);
        node.Children.Add(child);
        return ResolveOverflowWithoutReinsertion(node);
    }

    private InsertionResult RecursiveInsert(ParentNode node, LeafNode leafNode, int currentHeight)
    {
        node.MergeEnvelope(ref leafNode.Envelope);

        int expandIndex = ChooseSubTree(node, leafNode);

        if (node.Children.Count < expandIndex)
        {
            node.Children.Add(leafNode);
            return ResolveOverflow(node, currentHeight);
        }

        var follow = (ParentNode)node.Children[expandIndex];
        InsertionResult expand = RecursiveInsert(follow, leafNode, currentHeight + 1);

        return expand switch
        {
            InsertionResultSplit { Node: var child } => Split(node, child, currentHeight),
            InsertionResultReinsert reinsert => Reinsert(node, reinsert),
            InsertionResultComplete => InsertionResultComplete.Instance,
            _ => throw new InvalidOperationException("Should never happen: Bug?")
        };
    }

    private static InsertionResultReinsert Reinsert(ParentNode node, InsertionResultReinsert reinsert)
    {
        node.RecalculateAABB();
        return reinsert;
    }

    private InsertionResult Split(ParentNode node, Node child, int currentHeight)
    {
        node.MergeEnvelope(ref child.Envelope);
        node.Children.Add(child);
        return ResolveOverflow(node, currentHeight);
    }

    private int ChooseSubTree(ParentNode node, Node toInsert)
    {
        Node? firstChild = node.Children.FirstOrDefault();
        if (firstChild is null or LeafNode)
        {
            return int.MaxValue;
        }

        bool allLeaves = firstChild is ParentNode { Children: [LeafNode, ..] };

        TEnvelope insertionAABB = toInsert.Envelope;

        int inclusionCount = 0;
        TValue minArea = TValue.MaxValue;
        int minIndex = -1;

        Span<Node> children = CollectionsMarshal.AsSpan(node.Children);

        for (int index = 0; index < children.Length; index++)
        {
            Node child = children[index];

            ref TEnvelope childAABB = ref child.Envelope;

            if (TEnvelopeMath.Contains(ref childAABB, ref insertionAABB))
            {
                inclusionCount++;

                TValue childArea = TEnvelopeMath.Area(ref childAABB);
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

            TEnvelope buffer = TEnvelopeMath.Empty(_dimensions);

            for (int index = 0; index < node.Children.Count; index++)
            {
                Node child = node.Children[index];

                TEnvelope aabb = child.Envelope;
                buffer = aabb;

                TEnvelopeMath.MergeInto(ref insertionAABB, ref buffer);

                TValue overlapIncrease = TValue.Zero;

                if (allLeaves)
                {
                    TValue overlap = TValue.Zero;
                    TValue newOverlap = TValue.Zero;

                    foreach (Node otherChild in node.Children)
                    {
                        if (otherChild == child)
                        {
                            continue;
                        }

                        TEnvelope otherAABB = otherChild.Envelope;

                        overlap += TEnvelopeMath.IntersectionArea(ref aabb, ref otherAABB);
                        newOverlap += TEnvelopeMath.IntersectionArea(ref buffer, ref otherAABB);
                    }

                    overlapIncrease = newOverlap - overlap;
                }

                TValue area = TEnvelopeMath.Area(ref buffer);
                TValue areaIncrease = area - TEnvelopeMath.Area(ref aabb);

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

    private InsertionResult ResolveOverflowWithoutReinsertion(ParentNode node)
    {
        return node.Children.Count > _settings.MaxNodeEntries
            ? new InsertionResultSplit(Split(node))
            : InsertionResultComplete.Instance;
    }

    private InsertionResult ResolveOverflow(ParentNode node, int currentHeight)
    {
        if (_settings.ReinsertionCount == 0)
        {
            return ResolveOverflowWithoutReinsertion(node);
        }

        if (node.Children.Count > _settings.MaxNodeEntries)
        {
            List<Node> reinsertionNodes = GetNodesForReinsertaion(node);
            return new InsertionResultReinsert(reinsertionNodes, currentHeight);
        }
        else
        {
            return InsertionResultComplete.Instance;
        }
    }

    private ParentNode Split(ParentNode node)
    {
        int axis = GetSplitAxis(node);

        Debug.Assert(node.Children.Count >= 2);

        node.Children.Sort(_axisComparers[axis]);

        TValue bestOverlap = TValue.MaxValue;
        TValue bestArea = TValue.MaxValue;
        int minNodeEntries = _settings.MinNodeEntries;
        int bestIndex = minNodeEntries;

        TEnvelope leftAABB = TEnvelopeMath.Empty(_dimensions);
        TEnvelope rightAABB = TEnvelopeMath.Empty(_dimensions);
        Span<Node> childSpan = CollectionsMarshal.AsSpan(node.Children);

        for (int k = minNodeEntries; k <= node.Children.Count - minNodeEntries; k++)
        {
            Node firstChild = node.Children[k - 1];
            Node secondChild = node.Children[k];
            TEnvelope firstAABB = firstChild.Envelope;
            TEnvelope secondAABB = secondChild.Envelope;

            leftAABB = firstAABB;
            rightAABB = secondAABB;

            foreach (Node child in childSpan[..k])
            {
                TEnvelopeMath.MergeInto(ref child.Envelope, ref leftAABB);
            }

            foreach (Node child in childSpan[k..])
            {
                TEnvelopeMath.MergeInto(ref child.Envelope, ref rightAABB);
            }

            TValue overlap = TEnvelopeMath.IntersectionArea(ref leftAABB, ref rightAABB);
            TValue area = TEnvelopeMath.Area(ref leftAABB) + TEnvelopeMath.Area(ref rightAABB);

            if (k == minNodeEntries || (overlap < bestOverlap && area < bestArea))
            {
                bestOverlap = overlap;
                bestArea = area;
                bestIndex = k;
            }
        }

        ParentNode newNode = new(childSpan[bestIndex..]);
        node.Children.RemoveRange(bestIndex, node.Children.Count - bestIndex);
        node.RecalculateAABB();
        return newNode;
    }

    private int GetSplitAxis(ParentNode node)
    {
        TValue bestPerimeter = TValue.MaxValue;
        int bestAxis = 0;
        int minNodeEntries = _settings.MinNodeEntries;
        int until = node.Children.Count - minNodeEntries + 1;

        TEnvelope left = TEnvelopeMath.Empty(_dimensions);
        TEnvelope right = TEnvelopeMath.Empty(_dimensions);

        TEnvelope leftModified = TEnvelopeMath.Empty(_dimensions);
        TEnvelope rightModified = TEnvelopeMath.Empty(_dimensions);

        Span<Node> childSpan = CollectionsMarshal.AsSpan(node.Children);

        for (int axis = 0; axis < _dimensions; axis++)
        {
            node.Children.Sort(_axisComparers[axis]);

            // initialize the memory for left and right AABBs
            left = childSpan[0].Envelope;
            right = childSpan[until].Envelope;

            for (int i = 1; i < until; i++)
            {
                TEnvelopeMath.MergeInto(ref childSpan[i].Envelope, ref left);
            }

            for (int i = until + 1; i < node.Children.Count; i++)
            {
                TEnvelopeMath.MergeInto(ref childSpan[i].Envelope, ref right);
            }

            for (int k = minNodeEntries; k < until; k++)
            {
                leftModified = left;
                rightModified = right;

                foreach (Node child in childSpan[..k])
                {
                    TEnvelopeMath.MergeInto(ref child.Envelope, ref leftModified);
                }

                foreach (Node child in childSpan[k..])
                {
                    TEnvelopeMath.MergeInto(ref child.Envelope, ref rightModified);
                }

                // We can use the half perimeter as we are only interested in the relative size and are axis-aligned
                TValue perimeter = TEnvelopeMath.HalfPerimeter(ref leftModified) + TEnvelopeMath.HalfPerimeter(ref rightModified);

                if (perimeter < bestPerimeter)
                {
                    bestPerimeter = perimeter;
                    bestAxis = axis;
                }
            }
        }
        return bestAxis;
    }

    private List<Node> GetNodesForReinsertaion(ParentNode node)
    {
        // TODO: think about optimizing, maybe with an comparer that has preallocated buffers and can be initialized with the center of the AABB
        node.Children.Sort((left, right) =>
        {
            Span<TValue> center = stackalloc TValue[_dimensions];
            TEnvelopeMath.CopyCenterTo(ref node.Envelope, center);

            Span<TValue> leftCenter = stackalloc TValue[_dimensions];
            Span<TValue> rightCenter = stackalloc TValue[_dimensions];

            TEnvelopeMath.CopyCenterTo(ref left.Envelope, leftCenter);
            TEnvelopeMath.CopyCenterTo(ref right.Envelope, rightCenter);

            TValue leftDistance = TensorPrimitives.Distance<TValue>(leftCenter, center);
            TValue rightDistance = TensorPrimitives.Distance<TValue>(rightCenter, center);

            return leftDistance.CompareTo(rightDistance);
        });

        Span<Node> childSpan = CollectionsMarshal.AsSpan(node.Children);

        int splitOffStart = node.Children.Count - _settings.ReinsertionCount;
        List<Node> reinsertionNodes = new(node.Children.Count - splitOffStart);
        reinsertionNodes.AddRange(childSpan[splitOffStart..]);
        node.Children.RemoveRange(splitOffStart, node.Children.Count - splitOffStart);

        node.RecalculateAABB();

        return reinsertionNodes;
    }

    internal readonly record struct InsertionAction(InsertionActionType ActionType, Node Node)
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
    internal sealed record class InsertionResultSplit(ParentNode Node) : InsertionResult;
    internal sealed record class InsertionResultReinsert(List<Node> Nodes, int level) : InsertionResult;
}
