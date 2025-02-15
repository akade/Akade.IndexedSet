using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class NonUniqueIndices
{
    private IndexedSet<int, TestData> _indexedSet = null!;
    private readonly TestData _a = new(0, 10, GuidGen.Get(1), "AA");
    private readonly TestData _b = new(1, 10, GuidGen.Get(2), "BB");
    private readonly TestData _c = new(2, 11, GuidGen.Get(3), "CC");
    private readonly TestData _d = new(3, 12, GuidGen.Get(4), "CC");
    private readonly TestData _e = new(4, 12, GuidGen.Get(4), "CC");

    [TestInitialize]
    public void Init()
    {
        TestData[] data = [_a, _b, _c, _d, _e];
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithIndex(x => x.IntProperty)
                          .WithIndex(x => x.GuidProperty)
                          .WithIndex(x => x.StringProperty)
                          .WithIndex(CaseInsensitiveStringProperty, StringComparer.OrdinalIgnoreCase)
                          .Build();
    }

    [TestMethod]
    public void retrieval_via_secondary_int_key_returns_correct_items()
    {
        _indexedSet.AssertMultipleItems(x => x.IntProperty, expectedElements: [_a, _b]);
        _indexedSet.AssertSingleItem(x => x.IntProperty, _c);
        _indexedSet.AssertMultipleItems(x => x.IntProperty, expectedElements: [_d, _e]);
    }

    [TestMethod]
    public void retrieval_via_secondary_guid_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _a);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _b);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _c);
        _indexedSet.AssertMultipleItems(x => x.GuidProperty, expectedElements: [_d, _e]);
    }

    [TestMethod]
    public void retrieval_via_secondary_string_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.StringProperty, _a);
        _indexedSet.AssertSingleItem(x => x.StringProperty, _b);
        _indexedSet.AssertMultipleItems(x => x.StringProperty, expectedElements: [_c, _d, _e]);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void range_queries_throw_exception()
    {
        _ = _indexedSet.Range(x => x.IntProperty, 5, 10).ToList();
    }

    [TestMethod]
    public void retrieval_via_compound_key_returns_correct_items()
    {
        TestData[] data = [_a, _b, _c, _d, _e];
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithIndex(x => (x.IntProperty, x.StringProperty))
                          .Build();

        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _a);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _b);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _c);
        _indexedSet.AssertMultipleItems(x => (x.IntProperty, x.StringProperty), expectedElements: [_d, _e]);
    }

    [TestMethod]
    public void Removal()
    {
        Assert.IsTrue(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Contains(0));
    }

    [TestMethod]
    public void string_query_selects_the_correct_where_overload()
    {
        _indexedSet.AssertMultipleItems(x => x.StringProperty, expectedElements: [_c, _d, _e]);
        Assert.AreEqual(_a, _indexedSet.Where(x => x.StringProperty, "AA").Single());
    }

    [TestMethod]
    public void TryGetSingle_on_secondary_key()
    {
        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntProperty, 8, out TestData? data1));
        Assert.IsNull(data1);

        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntProperty, 10, out TestData? data2));
        Assert.IsNull(data2);

        Assert.IsTrue(_indexedSet.TryGetSingle(x => x.IntProperty, 11, out TestData? data3));
        Assert.IsNotNull(data3);
    }

    private static string CaseInsensitiveStringProperty(TestData data)
    {
        return data.StringProperty;
    }

    [TestMethod]
    public void Case_insensitive_string_property_matching()
    {
        TestData[] actual = _indexedSet.Where(CaseInsensitiveStringProperty, "aa").ToArray();
        CollectionAssert.AreEquivalent(new[] { _a }, actual);

        actual = _indexedSet.Where(CaseInsensitiveStringProperty, "cc").ToArray();
        CollectionAssert.AreEquivalent(new[] { _c, _d, _e }, actual);
    }
}
