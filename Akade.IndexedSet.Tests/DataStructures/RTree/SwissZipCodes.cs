#if NET9_0_OR_GREATER
using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using System.Numerics.Tensors;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;

[TestClass]
public class RTreeTests
{
    private static async Task<(RTree<SwissZipCode, Vector2, VecRec2, float, Vector2Math> rTree, IEnumerable<SwissZipCode> zipCodeData)> CreateRTreeAsync(bool useBulkLoading)
    {
        IEnumerable<SwissZipCode> zipCodeData = await SwissZipCodes.LoadAsync();
        RTree<SwissZipCode, Vector2, VecRec2, float, Vector2Math> rTree = new(s => VecRec2.CreateFromPoint(s.Coordinates), 2, RTreeSettings.Default);

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
        rTree.CheckForCorruption(useBulkLoading);
        return (rTree, zipCodeData);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Intersection(bool useBulkLoading)
    {
        (RTree<SwissZipCode, Vector2, VecRec2, float, Vector2Math>? rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);

        VecRec2 searchRect = new(8.683342209696512f, 47.41151475464969f, 8.92130422393863f, 47.53819397959311f);

        List<SwissZipCode> expectedResults = [];
        foreach (SwissZipCode zipCode in zipCodeData)
        {
            var zipCodeAABB = VecRec2.CreateFromPoint(zipCode.Coordinates);
            if (Vector2Math.Contains(searchRect, zipCodeAABB))
            {
                expectedResults.Add(zipCode);
            }
        }

        IEnumerable<SwissZipCode> results = rTree.IntersectWith(searchRect);
        CollectionAssert.AreEquivalent(expectedResults, results.ToList());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task NearestNeighbours(bool useBulkLoading)
    {
        (RTree<SwissZipCode, Vector2, VecRec2, float, Vector2Math>? rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);
        Vector2 winterthur = new(8.729481588728078f, 47.48885146772f);

        var expectedNearestNeighbours = zipCodeData
           .Select(zip => (zip, Vector2.Distance(zip.Coordinates, winterthur)))
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
        (RTree<SwissZipCode, Vector2, VecRec2, float, Vector2Math>? rTree, IEnumerable<SwissZipCode> zipCodeData) = await CreateRTreeAsync(useBulkLoading);
        Vector2 winterthur = new(8.729481588728078f, 47.48885146772f);

        var expectedNearestNeighbours = zipCodeData
            .Select(zip => (zip, Vector2.Distance(zip.Coordinates, winterthur)))
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


        var nearestNeighbours = rTree.GetNearestNeighbours(winterthur).Take(10).ToList();
        CollectionAssert.AreEquivalent(expectedNearestNeighbours, nearestNeighbours);
        rTree.CheckForCorruption(useBulkLoading);
    }

}

#endif