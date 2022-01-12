using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class TestsWithoutIndices
{
    private IndexedSet<int, TestData> _indexedSet = null!;
    private readonly TestData _a = new(0, 10, Guid.NewGuid(), "A");
    private readonly TestData _b = new(1, 11, Guid.NewGuid(), "B");
    private readonly TestData _c = new(2, 12, Guid.NewGuid(), "C");

    [TestInitialize]
    public void Init()
    {
        _indexedSet = new IndexedSetBuilder<int, TestData>(x => x.PrimaryKey)
            .Build();


    }

    [TestMethod]
    public void adding_multiple_items_alawys_returns_correct_count()
    {
        Assert.AreEqual(0, _indexedSet.Count);

        _indexedSet.Add(_a);
        Assert.AreEqual(1, _indexedSet.Count);

        _indexedSet.Add(_b);
        Assert.AreEqual(2, _indexedSet.Count);

        _indexedSet.Add(_c);
        Assert.AreEqual(3, _indexedSet.Count);
    }

    [TestMethod]
    public void retrieval_via_primary_key_returns_matching_items()
    {
        AddAll();
        Assert.AreEqual(_a, _indexedSet[0]);
        Assert.AreEqual(_b, _indexedSet[1]);
        Assert.AreEqual(_c, _indexedSet[2]);
    }

    [TestMethod]
    public void removing_multiple_items_alawys_returns_correct_count()
    {
        AddAll();
        Assert.AreEqual(3, _indexedSet.Count);

        Assert.IsTrue(_indexedSet.Remove(_a));
        Assert.AreEqual(2, _indexedSet.Count);

        Assert.IsTrue(_indexedSet.Remove(_b));
        Assert.AreEqual(1, _indexedSet.Count);

        Assert.IsTrue(_indexedSet.Remove(_c));
        Assert.AreEqual(0, _indexedSet.Count);
    }

    [TestMethod]
    public void removing_multiple_items_removes_the_correct_items()
    {
        AddAll();
        Assert.IsTrue(_indexedSet.Contains(_b));
        Assert.IsTrue(_indexedSet.Remove(_b));

        Assert.AreEqual(_a, _indexedSet[0]);
        Assert.AreEqual(_c, _indexedSet[2]);

        Assert.IsFalse(_indexedSet.Contains(_b));
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void missing_primarykey_throws()
    {
        AddAll();
        _ = _indexedSet[42];
    }

    [TestMethod]
    [ExpectedException(typeof(IndexNotFoundException))]
    public void missing_index_throws()
    {
        AddAll();
        _ = _indexedSet.Single(x => x.IntProperty, _a.IntProperty);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void adding_duplicate_primary_key_throws()
    {
        AddAll();
        _indexedSet.Add(_a);
    }

    private void AddAll()
    {
        _indexedSet.Add(_a);
        _indexedSet.Add(_b);
        _indexedSet.Add(_c);
    }
}
