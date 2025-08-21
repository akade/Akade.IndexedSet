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

        if(leafNodesList.Count == 0)
        {
            return;
        }

        // We are using OMT: 
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

        int height = (int)Math.Ceiling(Math.Log(leafNodes.Length, _settings.MaxNodeEntries));
        int numberOfSubTrees = (int)Math.Pow(_settings.MaxNodeEntries, height - 1);
        int numberOfSlices = (int)Math.Floor(Math.Sqrt(Math.Ceiling(leafNodes.Length / (double)numberOfSubTrees)));

        // Split into slices


        // Split the leaf nodes into M parts
        int nodesPerPart = (int)Math.Ceiling((double)leafNodes.Length / _settings.MaxNodeEntries);
        int parts = (int)Math.Ceiling((double)leafNodes.Length / nodesPerPart);

        // For bulkloading, we permit that the last node can have less than MinNodeEntries

        for (int i = 0; i < parts; i++)
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
