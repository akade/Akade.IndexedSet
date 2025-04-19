using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class MultiValueIndices
{
    private IndexedSet<int, DenormalizedTestData> _indexedSet = null!;
    private readonly DenormalizedTestData _a = new(0, [1, 2, 3, 4]);
    private readonly DenormalizedTestData _b = new(1, [2, 3]);
    private readonly DenormalizedTestData _c = new(2, [3]);
    private readonly DenormalizedTestData _d = new(3, [1, 2, 3]);

    [TestInitialize]
    public void Init()
    {
        _indexedSet = new[] { _a, _b, _c, _d }.ToIndexedSet(x => x.PrimaryKey)
                                              .WithIndex(x => x.IntList)
                                              .WithIndex(IntListWithComparer, EqualityComparer<int>.Create((a, b) => a / 2 == b / 2, x => x / 2))
                                              .Build();
    }

    private static IEnumerable<int> IntListWithComparer(DenormalizedTestData data)
    {
        return data.IntList;
    }

    [TestMethod]
    public void Contains_queries_return_correct_results()
    {
        CollectionAssert.AreEquivalent(new[] { _a, _d }, _indexedSet.Where(x => x.IntList, contains: 1).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a, _b, _d }, _indexedSet.Where(x => x.IntList, contains: 2).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a, _b, _c, _d }, _indexedSet.Where(x => x.IntList, contains: 3).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a }, _indexedSet.Where(x => x.IntList, contains: 4).ToArray());
    }

    [TestMethod]
    public void Single_queries_return_correct_results()
    {
        Assert.AreEqual(_a, _indexedSet.Single(x => x.IntList, 4));
    }

    [TestMethod]
    public void Removal()
    {
        Assert.IsTrue(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Remove(0));
        Assert.IsFalse(_indexedSet.Contains(0));
    }

    [TestMethod]
    public void TryGetSingle()
    {
        Assert.IsTrue(_indexedSet.TryGetSingle(x => x.IntList, 4, out DenormalizedTestData? test1));
        Assert.IsNotNull(test1);

        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntList, 1, out DenormalizedTestData? test2));
        Assert.IsNull(test2);

        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntList, 5, out DenormalizedTestData? test3));
        Assert.IsNull(test3);
    }

    [TestMethod]
    public void Custom_comparer_where()
    {
        // a: [1, 2, 3, 4] => [0, 1, 1, 2] 
        // b: [2, 3]       => [1, 1]
        // c: [3]          => [1]
        // d: [1, 2, 3]    => [0, 1, 1]

        CollectionAssert.AreEquivalent(new[] { _a, _d }, _indexedSet.Where(IntListWithComparer, contains: 0).ToArray());         // 0
        CollectionAssert.AreEquivalent(new[] { _a, _d }, _indexedSet.Where(IntListWithComparer, contains: 1).ToArray());         // 0
        CollectionAssert.AreEquivalent(new[] { _a, _b, _c, _d }, _indexedSet.Where(IntListWithComparer, contains: 2).ToArray()); // 1
        CollectionAssert.AreEquivalent(new[] { _a, _b, _c, _d }, _indexedSet.Where(IntListWithComparer, contains: 3).ToArray()); // 1
        CollectionAssert.AreEquivalent(new[] { _a }, _indexedSet.Where(IntListWithComparer, contains: 4).ToArray());             // 2
    }

    [TestMethod]
    public void Custom_comparer_trygetsingle()
    {
        Assert.IsTrue(_indexedSet.TryGetSingle(IntListWithComparer, 4, out DenormalizedTestData? test1));
        Assert.AreEqual(_a, test1);

        Assert.IsFalse(_indexedSet.TryGetSingle(IntListWithComparer, 1, out DenormalizedTestData? test2));
        Assert.IsNull(test2);

        Assert.IsFalse(_indexedSet.TryGetSingle(IntListWithComparer, 6, out DenormalizedTestData? test3));
        Assert.IsNull(test3);
    }

    [TestMethod]
    public void Clear()
    {
        _indexedSet.Clear();
        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.IntList, 1, out _));
    }
}
