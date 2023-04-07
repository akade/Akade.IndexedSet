using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class UniqueIndices
{
    private IndexedSet<int, TestData> _indexedSet = null!;
    private readonly TestData _a = new(0, 10, Guid.NewGuid(), "A");
    private readonly TestData _b = new(1, 11, Guid.NewGuid(), "B");
    private readonly TestData _c = new(2, 12, Guid.NewGuid(), "C");

    [TestInitialize]
    public void Init()
    {
        TestData[] data = new[] { _a, _b, _c };
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithUniqueIndex(x => x.IntProperty)
                          .WithUniqueIndex(x => x.GuidProperty)
                          .WithUniqueIndex(x => x.StringProperty)
                          .Build();
    }

    [TestMethod]
    public void retrieval_via_secondary_int_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.IntProperty, _a);
        _indexedSet.AssertSingleItem(x => x.IntProperty, _b);
        _indexedSet.AssertSingleItem(x => x.IntProperty, _c);
    }

    [TestMethod]
    public void retrieval_via_secondary_guid_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _a);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _b);
        _indexedSet.AssertSingleItem(x => x.GuidProperty, _c);
    }

    [TestMethod]
    public void retrieval_via_secondary_string_key_returns_correct_items()
    {
        _indexedSet.AssertSingleItem(x => x.StringProperty, _a);
        _indexedSet.AssertSingleItem(x => x.StringProperty, _b);
        _indexedSet.AssertSingleItem(x => x.StringProperty, _c);
    }

    [TestMethod]
    public void retrieval_via_compound_key_returns_correct_items()
    {
        TestData[] data = new[] { _a, _b, _c };
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithUniqueIndex(x => (x.IntProperty, x.StringProperty))
                          .Build();

        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _a);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _b);
        _indexedSet.AssertSingleItem(x => (x.IntProperty, x.StringProperty), _c);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void range_queries_throw_exception()
    {
        _ = _indexedSet.Range(x => x.IntProperty, 5, 10).ToList();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void adding_duplicate_key_throws()
    {
        _ = _indexedSet.Add(new TestData(5, 10, Guid.NewGuid(), "ew"));
    }

    [TestMethod]
    public void Removal()
    {
        Assert.IsTrue(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Contains(0));
    }

    [TestMethod]
    public void TryGetSingle_on_secondary_key()
    {
        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntProperty, 8, out TestData? data1));
        Assert.IsNull(data1);

        Assert.IsTrue(_indexedSet.TryGetSingle(x => x.IntProperty, 10, out TestData? data2));
        Assert.IsNotNull(data2);
    }
}
