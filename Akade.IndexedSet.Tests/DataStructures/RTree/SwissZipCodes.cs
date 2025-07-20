#if NET9_0_OR_GREATER
using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics.Tensors;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;

[TestClass]
public class RTreeTests
{
    private static async Task<(RTree<SwissZipCode, double> rTree, IEnumerable<SwissZipCode> zipCodeData)> CreateRTreeAsync(bool useBulkLoading)
    {
        IEnumerable<SwissZipCode> zipCodeData = await SwissZipCodes.LoadAsync();
        RTree<SwissZipCode, double> rTree = new(s => AABB<double>.CreateFromPoint(s.GetCoordinatesSpan()), 2, RTreeSettings.Default);

        if (useBulkLoading)
        {
            rTree.BulkLoad(zipCodeData);
        }
        else
        {
            foreach (SwissZipCode zipCode in zipCodeData)
            {
                rTree.Insert(zipCode);
            }
        }
        rTree.CheckForCorruption();
        return (rTree, zipCodeData);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Intersection(bool useBulkLoading)
    {
        (RTree<SwissZipCode, double> rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);

        Span<double> searchRect = [8.683342209696512, 47.41151475464969, 8.92130422393863, 47.53819397959311];
        var searchAABB = AABB<double>.CreateFromCombinedBuffer(searchRect);

        List<SwissZipCode> expectedResults = [];
        foreach (SwissZipCode zipCode in zipCodeData)
        {
            var zipCodeAABB = AABB<double>.CreateFromPoint(zipCode.GetCoordinatesSpan());
            if (searchAABB.Contains(zipCodeAABB))
            {
                expectedResults.Add(zipCode);
            }
        }

        IEnumerable<SwissZipCode> results = rTree.IntersectWith(searchAABB);
        CollectionAssert.AreEquivalent(expectedResults, results.ToList());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task NearestNeighbours(bool useBulkLoading)
    {
        (RTree<SwissZipCode, double> rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);
        double[] winterthur = [8.729481588728078, 47.48885146772];

        var expectedNearestNeighbours = zipCodeData
           .Select(zip => (zip, TensorPrimitives.Distance(zip.GetCoordinatesSpan(), winterthur)))
           .OrderBy(x => x.Item2)
           .Take(10)
           .ToList();

        var nearestNeighbours = rTree.GetNearestNeighbours(winterthur).Take(10).ToList();
        CollectionAssert.AreEquivalent(expectedNearestNeighbours, nearestNeighbours);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Removal_of_5_random_entries(bool useBulkLoading)
    {
        (RTree<SwissZipCode, double> rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);
        double[] winterthur = [8.729481588728078, 47.48885146772];

        var expectedNearestNeighbours = zipCodeData
            .Select(zip => (zip, TensorPrimitives.Distance(zip.GetCoordinatesSpan(), winterthur)))
            .OrderBy(x => x.Item2)
            .Take(15)
            .ToList();

        HashSet<SwissZipCode> removedItems = [];

        for (int i = 0; i < 5; i++)
        {
            int toBeRemoved = Random.Shared.Next(0, expectedNearestNeighbours.Count);
            SwissZipCode zipCodeToRemove = expectedNearestNeighbours[toBeRemoved].zip;
            expectedNearestNeighbours.RemoveAt(toBeRemoved);
            _ = rTree.Remove(zipCodeToRemove);
            _ = removedItems.Add(zipCodeToRemove);
        }

        rTree.CheckForCorruption();

        var nearestNeighbours = rTree.GetNearestNeighbours(winterthur).Take(10).ToList();
        CollectionAssert.AreEquivalent(expectedNearestNeighbours, nearestNeighbours);
    }

}

#endif