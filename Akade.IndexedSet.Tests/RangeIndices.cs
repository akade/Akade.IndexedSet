using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class RangeIndices
{
    private IndexedSet<int, TestData> _indexedSet = null!;
    private readonly TestData _a = new(0, 10, GuidGen.Get(1), "B");
    private readonly TestData _b = new(1, 10, GuidGen.Get(2), "C");
    private readonly TestData _c = new(2, 11, GuidGen.Get(3), "E");
    private readonly TestData _d = new(3, 13, GuidGen.Get(5), "E");
    private readonly TestData _e = new(4, 13, GuidGen.Get(5), "E");

    [TestInitialize]
    public void Init()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
            .WithRangeIndex(x => x.IntProperty)
            .WithRangeIndex(x => x.GuidProperty)
            .WithRangeIndex(x => x.StringProperty)
            .Build();

        _indexedSet.Add(_a);
        _indexedSet.Add(_b);
        _indexedSet.Add(_c);
        _indexedSet.Add(_d);
        _indexedSet.Add(_e);
    }

    [TestMethod]
    public void range_query_on_ints_returns_correct_items()
    {
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 3, 25, inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 8, 12, inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 11, 14, inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 12, 14, inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _d, _e });
    }

    [TestMethod]
    public void range_query_on_ints_correctly_respects_inclusive_or_exclusive_boundaries()
    {
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 10, 13, inclusiveStart: false, inclusiveEnd: false, expectedElements: new[] { _c });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 10, 13, inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 10, 13, inclusiveStart: false, inclusiveEnd: true, expectedElements: new[] { _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.IntProperty, 10, 13, inclusiveStart: true, inclusiveEnd: true, expectedElements: new[] { _a, _b, _c, _d, _e });
    }

    [TestMethod]
    public void range_query_on_strings_returns_correct_items()
    {
        _indexedSet.AssertMultipleItemsViaRange(x => x.StringProperty, "A", "F", inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.StringProperty, "A", "D", inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b });
        _indexedSet.AssertMultipleItemsViaRange(x => x.StringProperty, "D", "Z", inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.StringProperty, "E", "Z", inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _c, _d, _e });
    }

    [TestMethod]
    public void range_query_on_guids_returns_correct_items()
    {
        _indexedSet.AssertMultipleItemsViaRange(x => x.GuidProperty, GuidGen.Get(0), GuidGen.Get(12), inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c, _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.GuidProperty, GuidGen.Get(0), GuidGen.Get(4), inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _a, _b, _c });
        _indexedSet.AssertMultipleItemsViaRange(x => x.GuidProperty, GuidGen.Get(4), GuidGen.Get(8), inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _d, _e });
        _indexedSet.AssertMultipleItemsViaRange(x => x.GuidProperty, GuidGen.Get(5), GuidGen.Get(8), inclusiveStart: true, inclusiveEnd: false, expectedElements: new[] { _d, _e });
    }



    [TestMethod]
    public void retrieval_via_secondary_int_key_returns_correct_items()
    {
        _indexedSet.AssertMultipleItems(x => x.IntProperty, expectedElements: new[] { _a, _b });
        _indexedSet.AssertSingleItem(x => x.IntProperty, _c);
        _indexedSet.AssertMultipleItems(x => x.IntProperty, expectedElements: new[] { _d, _e });
    }

    [TestMethod]
    public void retrieval_via_secondary_guid_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _a);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _b);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _c);
        _indexedSet.AssertMultipleItems(x => x.GuidProperty, expectedElements: new[] { _d, _e });
    }

    [TestMethod]
    public void retrieval_via_secondary_string_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.StringProperty, _a);
        _indexedSet.AssertSingleItem(x => x.StringProperty, _b);
        _indexedSet.AssertMultipleItems(x => x.StringProperty, expectedElements: new[] { _c, _d, _e });
    }

    [TestMethod]
    public void retrieval_via_compound_key_returns_correct_items()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
            .WithRangeIndex(x => (x.IntProperty, x.StringProperty))
            .Build();

        _indexedSet.Add(_a);
        _indexedSet.Add(_b);
        _indexedSet.Add(_c);
        _indexedSet.Add(_d);
        _indexedSet.Add(_e);

        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _a);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _b);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _c);
        _indexedSet.AssertMultipleItems(x => (x.IntProperty, x.StringProperty), expectedElements: new[] { _d, _e });
    }

    [TestMethod]
    public void min_and_max_return_correct_key_values()
    {
        Assert.AreEqual(10, _indexedSet.Min(x => x.IntProperty));
        Assert.AreEqual(13, _indexedSet.Max(x => x.IntProperty));

        Assert.AreEqual(GuidGen.Get(1), _indexedSet.Min(x => x.GuidProperty));
        Assert.AreEqual(GuidGen.Get(5), _indexedSet.Max(x => x.GuidProperty));

        Assert.AreEqual("B", _indexedSet.Min(x => x.StringProperty));
        Assert.AreEqual("E", _indexedSet.Max(x => x.StringProperty));
    }

    [TestMethod]
    public void minby_and_maxby_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new[] { _a, _b }, _indexedSet.MinBy(x => x.IntProperty).ToArray());
        CollectionAssert.AreEquivalent(new[] { _d, _e }, _indexedSet.MaxBy(x => x.IntProperty).ToArray());

        CollectionAssert.AreEquivalent(new[] { _a }, _indexedSet.MinBy(x => x.GuidProperty).ToArray());
        CollectionAssert.AreEquivalent(new[] { _d, _e }, _indexedSet.MaxBy(x => x.GuidProperty).ToArray());

        CollectionAssert.AreEquivalent(new[] { _a, }, _indexedSet.MinBy(x => x.StringProperty).ToArray());
        CollectionAssert.AreEquivalent(new[] { _c, _d, _e }, _indexedSet.MaxBy(x => x.StringProperty).ToArray());
    }

    [TestMethod]
    public void LessThan_and_LessThanOrEquals_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new[] { _a, _b }, _indexedSet.LessThan(x => x.IntProperty, 11).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a, _b, _c }, _indexedSet.LessThanOrEqual(x => x.IntProperty, 11).ToArray());
    }

    [TestMethod]
    public void GreaterThan_and_GreaterThanOrEquals_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new[] { _d, _e }, _indexedSet.GreaterThan(x => x.IntProperty, 11).ToArray());
        CollectionAssert.AreEquivalent(new[] { _c, _d, _e }, _indexedSet.GreaterThanOrEqual(x => x.IntProperty, 11).ToArray());
    }

    [TestMethod]
    public void Comparison_queries_return_empty_set_if_out_of_bounds()
    {
        Assert.IsFalse(_indexedSet.LessThan(x => x.IntProperty, 5).Any());
        Assert.IsFalse(_indexedSet.LessThanOrEqual(x => x.IntProperty, 5).Any());

        Assert.IsFalse(_indexedSet.GreaterThan(x => x.IntProperty, 20).Any());
        Assert.IsFalse(_indexedSet.GreaterThanOrEqual(x => x.IntProperty, 20).Any());
    }

    [TestMethod]
    public void All_queries_return_empty_enumerable_for_empty_set()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
           .WithRangeIndex(x => x.IntProperty)
           .Build();

        Assert.IsFalse(_indexedSet.Where(x => x.IntProperty, 5).Any());
        Assert.IsFalse(_indexedSet.Range(x => x.IntProperty, 5, 10).Any());

        Assert.IsFalse(_indexedSet.LessThan(x => x.IntProperty, 5).Any());
        Assert.IsFalse(_indexedSet.LessThanOrEqual(x => x.IntProperty, 5).Any());

        Assert.IsFalse(_indexedSet.GreaterThan(x => x.IntProperty, 5).Any());
        Assert.IsFalse(_indexedSet.GreaterThanOrEqual(x => x.IntProperty, 5).Any());

        Assert.IsFalse(_indexedSet.MaxBy(x => x.IntProperty).Any());
        Assert.IsFalse(_indexedSet.MinBy(x => x.IntProperty).Any());

        Assert.IsFalse(_indexedSet.OrderBy(x => x.IntProperty).Any());
        Assert.IsFalse(_indexedSet.OrderByDescending(x => x.IntProperty).Any());
    }

    [TestMethod]
    public void Min_and_max_throws_for_empty_set()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
           .WithRangeIndex(x => x.IntProperty)
           .Build();

        _ = Assert.ThrowsException<InvalidOperationException>(() => _indexedSet.Min(x => x.IntProperty));
        _ = Assert.ThrowsException<InvalidOperationException>(() => _indexedSet.Max(x => x.IntProperty));
    }
}
