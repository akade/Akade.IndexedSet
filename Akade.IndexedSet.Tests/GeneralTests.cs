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
        TestData[] data = [_a, _b, _c, _d, _e];
        _indexedSet = data.ToIndexedSet(x => x.PrimaryKey)
                          .WithIndex(x => x.IntProperty)
                          .WithIndex(x => x.WritableProperty)
                          .WithRangeIndex(x => x.GuidProperty)
                          .WithFullTextIndex(x => x.StringProperty)
                          .Build();
    }

    [TestMethod]
    public void Clear_removes_all_elements()
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

    [TestMethod]
    public void Adding_a_conflicting_item_should_keep_the_set_in_a_consistent_state()
    {
        IndexedSet<TestData> set = new[] { _a, _c, _d }.ToIndexedSet()
                                                       .WithIndex(x => x.GuidProperty)
                                                       .WithUniqueIndex(x => x.IntProperty)
                                                       .Build();

        _ = Assert.ThrowsExactly<ArgumentException>(() => set.Add(_b));
        Assert.IsFalse(set.TryGetSingle(x => x.GuidProperty, _b.GuidProperty, out _));
        Assert.AreEqual(3, set.Count);
        Assert.IsFalse(set.Contains(_b));
    }

    [TestMethod]
    public void Adding_multiple_items_with_a_conflicting_item_should_keep_the_set_in_a_consistent_state()
    {
        IndexedSet<TestData> set = new[] { _a, _c, }.ToIndexedSet()
                                                    .WithIndex(x => x.GuidProperty)
                                                    .WithUniqueIndex(x => x.IntProperty)
                                                    .Build();
        TestData[] dataToAdd = [_d, _b, _e];

        _ = Assert.ThrowsExactly<ArgumentException>(() => set.AddRange(dataToAdd));
        Assert.AreEqual(2, set.Count);

        foreach (TestData item in dataToAdd)
        {
            Assert.IsFalse(set.TryGetSingle(x => x.GuidProperty, _b.GuidProperty, out _));
            Assert.IsFalse(set.Contains(item));

        }
    }
}
