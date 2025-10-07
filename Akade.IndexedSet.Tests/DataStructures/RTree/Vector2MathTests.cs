using Akade.IndexedSet.DataStructures.RTree;
using System.Numerics;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;

[TestClass]
public class Vector2MathTests
{
    [TestMethod]
    [DataRow(1, 1, 4, 5, 12)]
    [DataRow(0, 0, 2, 2, 4)]
    [DataRow(-1, -1, 1, 1, 4)]
    [DataRow(1, 3, 1, 3, 0)]
    public void Area(float minX, float minY, float maxX, float maxY, float expectedArea)
    {
        VecRec2 env = new(new Vector2(minX, minY), new Vector2(maxX, maxY));
        float area = Vector2Math.Area(ref env);
        Assert.AreEqual(expectedArea, area, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 1, 4, 5, 2, 2, 3, 3, true)]
    [DataRow(0, 0, 2, 2, -1, -1, 1, 1, false)]
    [DataRow(0, 0, 2, 2, 2, 0, 3, 1, false)]
    public void Contains(float minXA, float minYA, float maxXA, float maxYA,
                         float minXB, float minYB, float maxXB, float maxYB,
                         bool expectedContains)
    {
        VecRec2 a = new(new Vector2(minXA, minYA), new Vector2(maxXA, maxYA));
        VecRec2 b = new(new Vector2(minXB, minYB), new Vector2(maxXB, maxYB));
        bool contains = Vector2Math.Contains(ref a, ref b);
        Assert.AreEqual(expectedContains, contains);
    }

    [TestMethod]
    public void CopyCenterTo()
    {
        VecRec2 env = new(new Vector2(1, 1), new Vector2(5, 5));
        Span<float> center = stackalloc float[2];
        Vector2Math.CopyCenterTo(ref env, center);
        Assert.AreEqual(3f, center[0], 1e-6);
        Assert.AreEqual(3f, center[1], 1e-6);
    }

    [TestMethod]
    public void CopyTo()
    {
        VecRec2 env = new(new Vector2(1, 1), new Vector2(5, 5));
        VecRec2 target = new();
        Vector2Math.CopyTo(ref env, ref target);
        Assert.AreEqual(env.Min, target.Min);
        Assert.AreEqual(env.Max, target.Max);
    }

    [TestMethod]
    [DataRow(1, 1, 0, 0, 2, 2, 0)] // inside
    [DataRow(0, 0, 0, 0, 2, 2, 0)] // on edge
    [DataRow(-1, -1, 0, 0, 2, 2, 2)] // [1 1] to [0 0]
    [DataRow(3, 1, 0, 0, 2, 2, 1)] // distance to boundary: 1
    public void DistanceToBoundarySquared(float x, float y, float minX, float minY, float maxX, float maxY, float distanceSquared)
    {
        VecRec2 env = new(new Vector2(minX, minY), new Vector2(maxX, maxY));
        float distSq = Vector2Math.DistanceToBoundarySquared(ref env, new Vector2(x, y));
        Assert.AreEqual(distanceSquared, distSq, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 2, 4, 5, 0, 4)]
    [DataRow(1, 2, 4, 7, 1, 7)]
    public void GetMax(float minX, float minY, float maxX, float maxY, int axis, float expectedValue)
    {
        VecRec2 env = new(new Vector2(minX, minY), new Vector2(maxX, maxY));
        Assert.AreEqual(expectedValue, Vector2Math.GetMax(ref env, axis));
    }

    [TestMethod]
    [DataRow(1, 2, 4, 5, 0, 1)]
    [DataRow(1, 2, 4, 7, 1, 2)]
    public void GetMin(float minX, float minY, float maxX, float maxY, int axis, float expectedValue)
    {
        VecRec2 env = new(new Vector2(minX, minY), new Vector2(maxX, maxY));
        Assert.AreEqual(expectedValue, Vector2Math.GetMin(ref env, axis));
    }

    [TestMethod]
    [DataRow(1, 1, 4, 5, 7)] // 3 + 4
    [DataRow(0, 0, 2, 2, 4)] // 2 + 2
    public void HalfPerimeter(float minX, float minY, float maxX, float maxY, float expectedHalfPerimeter)
    {
        VecRec2 env = new(new Vector2(minX, minY), new Vector2(maxX, maxY));
        float halfPerimeter = Vector2Math.HalfPerimeter(ref env);
        Assert.AreEqual(expectedHalfPerimeter, halfPerimeter, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 1, 4, 5, 2, 2, 3, 3, 1)] // containing
    [DataRow(0, 0, 4, 3, 2, 2, 6, 6, 2)] // partial overlap
    [DataRow(0, 0, 2, 2, 3, 3, 5, 5, 0)] // no overlap
    [DataRow(0, 0, 2, 2, 2, 0, 4, 2, 0)] // touching edge
    public void IntersectionArea(float minXA, float minYA, float maxXA, float maxYA,
                                 float minXB, float minYB, float maxXB, float maxYB,
                                 float expectedArea)
    {
        VecRec2 a = new(new Vector2(minXA, minYA), new Vector2(maxXA, maxYA));
        VecRec2 b = new(new Vector2(minXB, minYB), new Vector2(maxXB, maxYB));
        float area = Vector2Math.IntersectionArea(ref a, ref b);
        Assert.AreEqual(expectedArea, area, 1e-6);
    }

    [TestMethod]
    [DataRow(1, 1, 4, 5, 2, 2, 3, 3, true)] // containing
    [DataRow(0, 0, 4, 3, 2, 2, 6, 6, true)] // partial overlap
    [DataRow(0, 0, 2, 2, 3, 3, 5, 5, false)] // no overlap
    [DataRow(0, 0, 2, 2, 2, 0, 4, 2, true)] // touching edge
    public void Intersects(float minXA, float minYA, float maxXA, float maxYA,
                             float minXB, float minYB, float maxXB, float maxYB,
                             bool intersects)
    {
        VecRec2 a = new(new Vector2(minXA, minYA), new Vector2(maxXA, maxYA));
        VecRec2 b = new(new Vector2(minXB, minYB), new Vector2(maxXB, maxYB));
        Assert.AreEqual(intersects, Vector2Math.Intersects(ref a, ref b));
    }

    [TestMethod]
    [DataRow(1, 1, 4, 5, 2, 2, 3, 3)] // containing
    [DataRow(0, 0, 4, 3, 2, 2, 6, 6)] // partial overlap
    [DataRow(0, 0, 2, 2, 3, 3, 5, 5)] // no overlap
    [DataRow(0, 0, 2, 2, 2, 0, 4, 2)] // touching edge
    public void Merge_and_MergeAndInto(float minXA, float minYA, float maxXA, float maxYA,
                                       float minXB, float minYB, float maxXB, float maxYB)
    {
        Vector2 minA = new(minXA, minYA);
        Vector2 maxA = new(maxXA, maxYA);
        Vector2 minB = new(minXB, minYB);
        Vector2 maxB = new(maxXB, maxYB);

        var min = Vector2.Min(minA, minB);
        var max = Vector2.Max(maxA, maxB);

        VecRec2 a = new(minA, maxA);
        VecRec2 b = new(minB, maxB);
        VecRec2 result = new(min, max);
        Vector2Math.Merge(ref a, ref b, ref result);

        Assert.AreEqual(min, result.Min);
        Assert.AreEqual(max, result.Max);

        Vector2Math.MergeInto(ref a, ref b);
        Assert.AreEqual(min, b.Min);
        Assert.AreEqual(max, b.Max);
    }
}
