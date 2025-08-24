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

        var leafNodesList = elements.Select(elem => new LeafNode(elem, _getAABB(elem)))
                                    .ToList();

        if (leafNodesList.Count == 0)
        {
            return;
        }

        // We are using OMT: 
        // - slower than STR but should produce better query performance
        // - at each level, we sort the leaf nodes by their AABBs by alternating axis and split according to the maximum node occupancy

        Span<LeafNode> leafNodes = CollectionsMarshal.AsSpan(leafNodesList);
        SplitAndAdd(_root, leafNodes);

        Count = leafNodes.Length;
    }

    private void SplitAndAdd(ParentNode currentParent, Span<LeafNode> leafNodes)
    {
        if (leafNodes.Length <= _settings.MaxNodeEntries)
        {
            foreach (LeafNode leafNode in leafNodes)
            {
                currentParent.Children.Add(leafNode);
            }
            currentParent.RecalculateAABB();
            return;
        }

        // TODO:Maybe calculate for each axis, if all have the same value then Math.Pow(numberOfClustersPerAxis, _dimensions) == _maxNodeEntries would need to match, otherwise
        // we violate that condition
        int numberOfClustersPerAxis = Math.Max(2, NumberOfClustersPerAxis(leafNodes.Length));


        ClusterAndAdd(currentParent, leafNodes, numberOfClustersPerAxis, 0);
        currentParent.RecalculateAABB();
    }

    private void ClusterAndAdd(ParentNode parent, Span<LeafNode> elements, int numberOfClustersPerAxis, int currentAxis)
    {
        if (currentAxis == _dimensions)
        {
            ParentNode subParent = new();
            SplitAndAdd(subParent, elements);
            parent.Children.Add(subParent);
            parent.MergeEnvelope(subParent.Envelope);
        }
        else
        {
            // sort remaining nodes by the current axis
            elements.Sort(_axisComparers[currentAxis]);

            int numberOfElementsPerCluster = (elements.Length + numberOfClustersPerAxis - 1) / numberOfClustersPerAxis;

            Span<LeafNode> remaining = elements; 
            
            while (remaining.Length > 0)
            {
                // Split off the first cluster
                int currentSize = Math.Min(numberOfElementsPerCluster, remaining.Length);
                Span<LeafNode> currentCluster = remaining[..currentSize];
                remaining = remaining[currentSize..];

                ClusterAndAdd(parent, currentCluster, numberOfClustersPerAxis, currentAxis + 1);
            }

        }
    }

    private int NumberOfClustersPerAxis(int numberOfElements)
    {
        int height = (int)Math.Ceiling(Math.Log(numberOfElements, _settings.MaxNodeEntries));
        double numberOfSubTrees = Math.Pow(_settings.MaxNodeEntries, height - 1);
        int numberOfClusters = (int)Math.Ceiling(numberOfElements / numberOfSubTrees);
        return (int)Math.Ceiling(Math.Pow(numberOfClusters, 1d / _dimensions)); // for 2d, this is the square root. For 3d, this is the cube root, etc.
    }
}
