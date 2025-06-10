#if NET9_0_OR_GREATER

using Akade.IndexedSet.DataStructures.RTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures.RTree;

[TestClass]
public class AABBTests
{
    [TestMethod]
    public void Enlarging_AABB_contains_both_original_values()
    {
        ParentNode<int, float> parentNode = new();

        AABB<float> a = new([0f, 0f], [1f, 1f]);
        AABB<float> b = new([0.5f, 0.5f], [1.5f, 1.5f]);

        parentNode.MergeAABB(a);
        parentNode.MergeAABB(b);

        AABB<float> expected = parentNode.GetAABB(null!);

        Assert.IsTrue(expected.Contains(a));
        Assert.IsTrue(expected.Contains(b));
    }

    [TestMethod]
    public void Does_not_contain()
    {
        AABB<float> a = new([0f, 0f], [1f, 1f]);
        AABB<float> b = new([0.5f, 0.5f], [3f, 3f]);
        
        Assert.IsFalse(a.Contains(b));
        Assert.IsFalse(b.Contains(a));
    }
}

#endif
