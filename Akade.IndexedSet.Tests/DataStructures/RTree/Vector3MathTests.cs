using Akade.IndexedSet.DataStructures.RTree;
using System.Numerics;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;

[TestClass]
public class Vector3MathTests
{
    [TestMethod]
    [DataRow(1, 1, 1, 4, 5, 6, 60)]
    [DataRow(0, 0, 0, 2, 2, 2, 8)]
    [DataRow(1, 2, 3, 1, 2, 3, 0)]
    public void Area(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float expectedArea)
    {
        VecRec3 env = new(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        float area = Vector3Math.Area(ref env);
        Assert.AreEqual(expectedArea, area, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 1, 1, 4, 5, 6, 2, 2, 2, 3, 3, 3, true)]
    [DataRow(0, 0, 0, 2, 2, 2, -1, -1, -1, 1, 1, 1, false)]
    [DataRow(0, 0, 0, 2, 2, 2, 2, 0, 0, 3, 1, 1, false)]
    public void Contains(float minXA, float minYA, float minZA, float maxXA, float maxYA, float maxZA,
                         float minXB, float minYB, float minZB, float maxXB, float maxYB, float maxZB,
                         bool expectedContains)
    {
        VecRec3 a = new(new Vector3(minXA, minYA, minZA), new Vector3(maxXA, maxYA, maxZA));
        VecRec3 b = new(new Vector3(minXB, minYB, minZB), new Vector3(maxXB, maxYB, maxZB));
        bool contains = Vector3Math.Contains(ref a, ref b);
        Assert.AreEqual(expectedContains, contains);
    }

    [TestMethod]
    public void CopyCenterTo()
    {
        VecRec3 env = new(new Vector3(1, 1, 3), new Vector3(5, 5, 8));
        Span<float> center = stackalloc float[3];
        Vector3Math.CopyCenterTo(ref env, center);
        Assert.AreEqual(3f, center[0], 1e-6);
        Assert.AreEqual(3f, center[1], 1e-6);
        Assert.AreEqual(5.5f, center[2], 1e-6);
    }

    [TestMethod]
    public void CopyTo()
    {
        VecRec3 env = new(new Vector3(1, 1, 3), new Vector3(5, 5, 8));
        VecRec3 target = new();
        Vector3Math.CopyTo(ref env, ref target);
        Assert.AreEqual(env.Min, target.Min);
        Assert.AreEqual(env.Max, target.Max);
    }

    [TestMethod]
    // inside
    [DataRow(1, 1, 1, 0, 0, 0, 2, 2, 2, 0)]
    // on edge
    [DataRow(0, 0, 0, 0, 0, 0, 2, 2, 2, 0)]
    // [1 1 1] to [0 0 0]
    [DataRow(-1, -1, -1, 0, 0, 0, 2, 2, 2, 3)]
    // distance to boundary: 1
    [DataRow(3, 1, 1, 0, 0, 0, 2, 2, 2, 1)]
    public void DistanceToBoundarySquared(float x, float y, float z, float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float distanceSquared)
    {
        VecRec3 env = new(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        float distSq = Vector3Math.DistanceToBoundarySquared(ref env, new Vector3(x, y, z));
        Assert.AreEqual(distanceSquared, distSq, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 2, 4, 5, 7, 8, 0, 5)]
    [DataRow(1, 2, 4, 7, 8, 10, 1, 8)]
    [DataRow(1, 2, 4, 7, 8, 10, 2, 10)]

    public void GetMax(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, int axis, float expectedValue)
    {
        VecRec3 env = new(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        Assert.AreEqual(expectedValue, Vector3Math.GetMax(ref env, axis));
    }

    [TestMethod]
    [DataRow(1, 2, 4, 5, 7, 8, 0, 1)]
    [DataRow(1, 2, 4, 7, 8, 10, 1, 2)]
    [DataRow(1, 2, 4, 7, 8, 10, 2, 4)]
    public void GetMin(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, int axis, float expectedValue)
    {
        VecRec3 env = new(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        Assert.AreEqual(expectedValue, Vector3Math.GetMin(ref env, axis));
    }

    [TestMethod]
    [DataRow(1, 1, 1, 4, 5, 6, 12)] // 3 + 4 + 5 = 12
    [DataRow(0, 0, 0, 2, 2, 2, 6)] // 2 + 2 + 2 = 6
    public void HalfPerimeter(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, float expectedHalfPerimeter)
    {
        VecRec3 env = new(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        float halfPerimeter = Vector3Math.HalfPerimeter(ref env);
        Assert.AreEqual(expectedHalfPerimeter, halfPerimeter, 1e-6);
    }

    [TestMethod]
    // containing
    [DataRow(1, 1, 1, 4, 5, 6, 2, 2, 2, 3, 3, 3, 1)]
    // partial overlap
    [DataRow(0, 0, 0, 4, 3, 3, 2, 2, 2, 6, 6, 6, 2)]
    // no overlap
    [DataRow(0, 0, 0, 2, 2, 2, 3, 3, 3, 5, 5, 5, 0)]
    // touching edge
    [DataRow(0, 0, 0, 2, 2, 2, 2, 0, 0, 4, 2, 2, 0)]
    public void IntersectionArea(float minXA, float minYA, float minZA, float maxXA, float maxYA, float maxZA,
                                 float minXB, float minYB, float minZB, float maxXB, float maxYB, float maxZB,
                                 float expectedArea)
    {
        VecRec3 a = new(new Vector3(minXA, minYA, minZA), new Vector3(maxXA, maxYA, maxZA));
        VecRec3 b = new(new Vector3(minXB, minYB, minZB), new Vector3(maxXB, maxYB, maxZB));
        float area = Vector3Math.IntersectionArea(ref a, ref b);
        Assert.AreEqual(expectedArea, area, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 1, 1, 4, 5, 6, 2, 2, 2, 3, 3, 3, true, true)] // containing
    [DataRow(0, 0, 0, 4, 3, 3, 2, 2, 2, 6, 6, 6, true, true)] // partial overlap
    [DataRow(0, 0, 0, 2, 2, 2, 3, 3, 3, 5, 5, 5, false)] // no overlap
    [DataRow(0, 0, 0, 2, 2, 2, 2, 0, 0, 4, 2, 2, true)] // touching edge
    public void Intersects(float minXA, float minYA, float minZA, float maxXA, float maxYA, float maxZA,
                           float minXB, float minYB, float minZB, float maxXB, float maxYB, float maxZB,
                           bool intersects, bool intersectsWithBoundary = false)
    {
        VecRec3 a = new(new Vector3(minXA, minYA, minZA), new Vector3(maxXA, maxYA, maxZA));
        VecRec3 b = new(new Vector3(minXB, minYB, minZB), new Vector3(maxXB, maxYB, maxZB));
        Assert.AreEqual(intersects, Vector3Math.Intersects(ref a, ref b));
        Assert.AreEqual(intersectsWithBoundary, Vector3Math.IntersectsWithoutBoundary(ref a, ref b));
    }

    [TestMethod]
    [DataRow(1, 1, 1, 4, 5, 6, 2, 2, 2, 3, 3, 3)] // containing
    [DataRow(0, 0, 0, 4, 3, 3, 2, 2, 2, 6, 6, 6)] // partial overlap
    [DataRow(0, 0, 0, 2, 2, 2, 3, 3, 3, 5, 5, 5)] // no overlap
    [DataRow(0, 0, 0, 2, 2, 2, 2, 0, 0, 4, 2, 2)] // touching edge
    public void Merge_and_MergeAndInto(float minXA, float minYA, float minZA, float maxXA, float maxYA, float maxZA,
                                       float minXB, float minYB, float minZB, float maxXB, float maxYB, float maxZB)
    {
        Vector3 minA = new(minXA, minYA, minZA);
        Vector3 maxA = new(maxXA, maxYA, maxZA);
        Vector3 minB = new(minXB, minYB, minZB);
        Vector3 maxB = new(maxXB, maxYB, maxZB);

        var min = Vector3.Min(minA, minB);
        var max = Vector3.Max(maxA, maxB);

        VecRec3 a = new(minA, maxA);
        VecRec3 b = new(minB, maxB);
        VecRec3 result = new(min, max);
        Vector3Math.Merge(ref a, ref b, ref result);

        Assert.AreEqual(min, result.Min);
        Assert.AreEqual(max, result.Max);

        Vector3Math.MergeInto(ref a, ref b);
        Assert.AreEqual(min, b.Min);
        Assert.AreEqual(max, b.Max);
    }
}
