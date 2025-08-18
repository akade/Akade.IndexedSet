//#if NET9_0_OR_GREATER

//using Akade.IndexedSet.DataStructures.RTree;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Akade.IndexedSet.Tests.DataStructures.RTree;

//[TestClass]
//public class AABBTests
//{
//    [TestMethod]
//    public void Enlarging_AABB_contains_both_original_values()
//    {
//        ParentNode<int, float> parentNode = new();

//        AABB<float> a = new([0f, 0f], [1f, 1f]);
//        AABB<float> b = new([0.5f, 0.5f], [1.5f, 1.5f]);

//        parentNode.MergeAABB(a);
//        parentNode.MergeAABB(b);

//        AABB<float> expected = parentNode.GetAABB(null!);

//        Assert.IsTrue(expected.Contains(a));
//        Assert.IsTrue(expected.Contains(b));
//    }

//    [TestMethod]
//    public void Does_not_contain()
//    {
//        AABB<float> a = new([0f, 0f], [1f, 1f]);
//        AABB<float> b = new([0.5f, 0.5f], [3f, 3f]);

//        Assert.IsFalse(a.Contains(b));
//        Assert.IsFalse(b.Contains(a));
//    }

//    [TestMethod]
//    // fully contained AABB should return true
//    [DataRow(0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.75f, 0.75f, true)]
//    // partially contained AABB should return true
//    [DataRow(0f, 0f, 1f, 1f, 0.5f, 0.5f, 1.5f, 1.5f, true)]
//    // touching should return true
//    [DataRow(0f, 0f, 1f, 1f, 1f, 1f, 2f, 2f, true)]
//    // not touching should return false
//    [DataRow(0f, 0f, 1f, 1f, 2f, 2f, 3f, 3f, false)]

//    public void Intersects(float aX1, float aY1, float aX2, float aY2, float bX1, float bY1, float bX2, float bY2, bool result)
//    {
//        AABB<float> aabb1 = new([aX1, aY1], [aX2, aY2]);
//        AABB<float> aabb2 = new([bX1, bY1], [bX2, bY2]);

//        Assert.AreEqual(result, aabb1.Intersects(aabb2));
//        Assert.AreEqual(result, aabb2.Intersects(aabb1));
//    }

//    [TestMethod]
//    // fully contained AABB should return true
//    [DataRow(0f, 0f, 1f, 1f, 0.5f, 0.5f, 0.75f, 0.75f, true)]
//    // contained but touching AABB should return true
//    [DataRow(0f, 0f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, true)]
//    // partially contained AABB should return false
//    [DataRow(0f, 0f, 1f, 1f, 0.5f, 0.5f, 1.5f, 1.5f, false)]
//    // touching should return false
//    [DataRow(0f, 0f, 1f, 1f, 1f, 1f, 2f, 2f, false)]
//    // not touching should return false
//    [DataRow(0f, 0f, 1f, 1f, 2f, 2f, 3f, 3f, false)]
//    public void Contains(float aX1, float aY1, float aX2, float aY2, float bX1, float bY1, float bX2, float bY2, bool result)
//    {
//        AABB<float> aabb1 = new([aX1, aY1], [aX2, aY2]);
//        AABB<float> aabb2 = new([bX1, bY1], [bX2, bY2]);
//        Assert.AreEqual(result, aabb1.Contains(aabb2));
//    }
//}

//#endif
