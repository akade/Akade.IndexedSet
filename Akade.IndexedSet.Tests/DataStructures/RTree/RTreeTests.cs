#if NET9_0_OR_GREATER
using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;


[TestClass]
public class RTreeTests
{
    [TestMethod]
    public async Task Rtree()
    {
        IEnumerable<SwissZipCode> zipCodeData = await SwissZipCodes.LoadAsync();

        Span<double> completeRectBuffer = stackalloc double[4];

        // init with the first
        var firstAABB = AABB<double>.CreateFromPoint(zipCodeData.First().GetCoordinatesSpan());
        firstAABB.CopyTo(completeRectBuffer);

        foreach (SwissZipCode zipCode in zipCodeData.Skip(1))
        {
            var aabb = AABB<double>.CreateFromPoint(zipCode.GetCoordinatesSpan());
            aabb.MergeInto(completeRectBuffer);
        }
        AABB<double> completeAABB = AABB<double>.CreateFromCombinedBuffer(completeRectBuffer);



        RTree<SwissZipCode, double> rTree = new(s => AABB<double>.CreateFromPoint(s.GetCoordinatesSpan()), 2, RTreeSettings.Default);

        foreach (SwissZipCode zipCode in zipCodeData)
        {
            rTree.Insert(zipCode);
        }




        Span<double> searchRect = [8.683342209696512, 47.41151475464969, 8.92130422393863, 47.53819397959311];
        AABB<double> searchAABB = AABB<double>.CreateFromCombinedBuffer(searchRect);

        IEnumerable<SwissZipCode> results = rTree.IntersectWith(searchAABB);

        List<SwissZipCode> expectedResults = [];

        foreach(SwissZipCode zipCode in zipCodeData)
        {
            AABB<double> zipCodeAABB = AABB<double>.CreateFromPoint(zipCode.GetCoordinatesSpan());  
            if (searchAABB.Contains(zipCodeAABB))
            {
                expectedResults.Add(zipCode);
            }
        }

        CollectionAssert.AreEquivalent(expectedResults, results.ToList());

        return;
    }
}

#endif