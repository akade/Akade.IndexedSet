#if NET9_0_OR_GREATER
using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TValue>
{
    public void BulkLoad(IEnumerable<TElement> elements)
    {
        if (Count > 0)
        {
            throw new InvalidOperationException("Bulk loading can only be done on an empty R-Tree");
        }

        var leafNodesList = elements.Select(elem => new LeafNode<TElement, TValue>(elem))
                                    .ToList();

        // We are using OMT: Overlap Minimizing Top-Down algorithm for bulk loading
        // - slower than STR but should produce better query performance
        // - at each level, we sort the leaf nodes by their AABBs by alternating axis and split according to the maximum node occupancy

        Span<LeafNode<TElement, TValue>> leafNodes = CollectionsMarshal.AsSpan(leafNodesList);

        Comparison<LeafNode<TElement, TValue>>[] axisComparers = Enumerable.Range(0, _dimensions)
            .Select(i => new Comparison<LeafNode<TElement, TValue>>((a, b) =>
            {
                AABB<TValue> aabbA = a.GetAABB(_getAABB);
                AABB<TValue> aabbB = b.GetAABB(_getAABB);
                return aabbA.Min[i].CompareTo(aabbB.Min[i]);
            }))
            .ToArray();

        SplitAndAdd(_root, currentDimension: 0, leafNodes, axisComparers);
    }

    private void SplitAndAdd(ParentNode<TElement, TValue> currentParent, int currentDimension, Span<LeafNode<TElement, TValue>> leafNodes, Comparison<LeafNode<TElement, TValue>>[] axisComparers)
    {
        leafNodes.Sort(axisComparers[currentDimension]);

        if (leafNodes.Length <= _settings.MaxNodeEntries)
        {
            foreach (LeafNode<TElement, TValue> leafNode in leafNodes)
            {
                currentParent.Children.Add(leafNode);
            }
            currentParent.RecalculateAABB(_getAABB);
            Count += leafNodes.Length;
            return;
        }

        // Split the leaf nodes into M parts
        int parts = (int)Math.Ceiling((double)leafNodes.Length / _settings.MaxNodeEntries);
        int nodesPerPart = (int)Math.Ceiling((double)leafNodes.Length / parts);

        for(int i = 0; i < parts; i++)
        {
            int start = i * nodesPerPart;
            int end = Math.Min(start + nodesPerPart, leafNodes.Length);
            Span<LeafNode<TElement, TValue>> partNodes = leafNodes[start..end];

            if (partNodes.Length > 0)
            {
                ParentNode<TElement, TValue> childNode = new();
                SplitAndAdd(childNode, (currentDimension + 1) % _dimensions, partNodes, axisComparers);

                currentParent.Children.Add(childNode);
                currentParent.MergeAABB(childNode.GetAABB(_getAABB));
            }
        }

    }
}
#endif
