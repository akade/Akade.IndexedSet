using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class GeneralTests
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
        TestData[] data = new[] { _a, _b, _c, _d, _e };
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithIndex(x => x.IntProperty)
                          .WithIndex(x => x.WritableProperty)
                          .WithRangeIndex(x => x.GuidProperty)
                          .WithFullTextIndex(x => x.StringProperty.AsMemory())
                          .Build();
    }

    [TestMethod]
    public void clear_removes_all_elements()
    {
        _indexedSet.Clear();
        Assert.AreEqual(0, _indexedSet.Count);
    }

    [TestMethod]
    public void Update_adds_non_existing_element()
    {
        _ = _indexedSet.Remove(_a);
        bool result = _indexedSet.Update(_a, _ => { });

        Assert.IsFalse(result);
        Assert.IsTrue(_indexedSet.Contains(_a));
    }

    [TestMethod]
    public void Updating_mutable_key_value_updates_index()
    {
        bool result = _indexedSet.Update(_a, _ => _a.WritableProperty = 10);

        Assert.IsTrue(result);
        Assert.AreEqual(_a, _indexedSet.Single(x => x.WritableProperty, 10));
        Assert.IsFalse(_indexedSet.Where(x => x.WritableProperty, 0).Contains(_a));
    }

    [TestMethod]
    public void Updating_immutable_key_value_updates_index()
    {
        bool result = _indexedSet.Update(_a, _ => _a with { IntProperty = 42 });

        Assert.IsTrue(result);
        Assert.AreEqual(_a.PrimaryKey, _indexedSet.Single(x => x.IntProperty, 42).PrimaryKey);
        Assert.IsFalse(_indexedSet.Where(x => x.IntProperty, 0).Any(a => a.PrimaryKey == _a.PrimaryKey));
    }
}
