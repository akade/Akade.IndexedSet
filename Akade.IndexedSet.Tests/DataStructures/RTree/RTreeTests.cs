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

        RTree<SwissZipCode, double> rTree = new(s => AABB<double>.CreateFromPoint(s.GetCoordinatesSpan()), 2, RTreeSettings.Default);

        foreach (SwissZipCode zipCode in zipCodeData)
        {
            rTree.Insert(zipCode);
        }


    }
}

#endif