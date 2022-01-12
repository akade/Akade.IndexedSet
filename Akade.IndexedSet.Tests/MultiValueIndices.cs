using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class MultiValueIndices
{
    private IndexedSet<int, TestData> _indexedSet = null!;
    private readonly TestData _a = new(0, 10, GuidGen.Get(1), "A");
    private readonly TestData _b = new(1, 10, GuidGen.Get(2), "B");
    private readonly TestData _c = new(2, 11, GuidGen.Get(3), "C");
    private readonly TestData _d = new(3, 12, GuidGen.Get(4), "C");
    private readonly TestData _e = new(4, 12, GuidGen.Get(4), "C");

    [TestInitialize]
    public void Init()
    {
        _indexedSet = IndexedSetBuilder<TestData>.Create(x => x.PrimaryKey)
            .WithIndex(x => x.IntProperty)
            .WithIndex(x => x.GuidProperty)
            .WithIndex(x => x.StringProperty)
            .Build();

        _indexedSet.Add(_a);
        _indexedSet.Add(_b);
        _indexedSet.Add(_c);
        _indexedSet.Add(_d);
        _indexedSet.Add(_e);
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
    [ExpectedException(typeof(NotSupportedException))]
    public void range_queries_throw_exception()
    {
        _ = _indexedSet.Range(x => x.IntProperty, 5, 10).ToList();
    }

    [TestMethod]
    public void retrieval_via_compound_key_returns_correct_items()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
            .WithIndex(x => (x.IntProperty, x.StringProperty))
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


}
