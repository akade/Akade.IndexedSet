using System.Runtime.InteropServices;

namespace Akade.IndexedSet.DataStructures.RTree;
internal sealed partial class RTree<TElement, TPoint, TEnvelope, TValue, TEnvelopeMath>
{
    public void BulkLoad(IEnumerable<TElement> elements)
    {
        if (Count > 0)
        {
            throw new InvalidOperationException("Bulk loading can only be done on an empty R-Tree");
        }

        var leafNodesList = elements.Select(elem => new LeafNode(elem))
                                    .ToList();

        // We are using OMT: Overlap Minimizing Top-Down algorithm for bulk loading
        // - slower than STR but should produce better query performance
        // - at each level, we sort the leaf nodes by their AABBs by alternating axis and split according to the maximum node occupancy

        Span<LeafNode> leafNodes = CollectionsMarshal.AsSpan(leafNodesList);

      

        SplitAndAdd(_root, currentDimension: 0, leafNodes);
    }

    private void SplitAndAdd(ParentNode currentParent, int currentDimension, Span<LeafNode> leafNodes)
    {
        leafNodes.Sort(_axisComparers[currentDimension]);

        if (leafNodes.Length <= _settings.MaxNodeEntries)
        {
            foreach (LeafNode leafNode in leafNodes)
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
            Span<LeafNode> partNodes = leafNodes[start..end];

            if (partNodes.Length > 0)
            {
                ParentNode childNode = new();
                SplitAndAdd(childNode, (currentDimension + 1) % _dimensions, partNodes);

                currentParent.Children.Add(childNode);
                currentParent.MergeEnvelope(childNode.GetEnvelope(_getAABB));
            }
        }

    }
}
