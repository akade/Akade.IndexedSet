using Akade.IndexedSet.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class MultiValueIndices
{
    private IndexedSet<int, DenormalizedTestData> _indexedSet = null!;
    private readonly DenormalizedTestData _a = new(0, new[] { 1, 2, 3, 4 });
    private readonly DenormalizedTestData _b = new(1, new[] { 2, 3 });
    private readonly DenormalizedTestData _c = new(2, new[] { 3 });
    private readonly DenormalizedTestData _d = new(3, new[] { 1, 2, 3 });

    [TestInitialize]
    public void Init()
    {
        _indexedSet = new[] { _a, _b, _c, _d }.ToIndexedSet(x => x.PrimaryKey)
                                              .WithIndex(x => x.IntList)
                                              .Build();
    }

    [TestMethod]
    public void contains_queries_return_correct_results()
    {
        CollectionAssert.AreEquivalent(new[] { _a, _d }, _indexedSet.Where(x => x.IntList, contains: 1).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a, _b, _d }, _indexedSet.Where(x => x.IntList, contains: 2).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a, _b, _c, _d }, _indexedSet.Where(x => x.IntList, contains: 3).ToArray());
        CollectionAssert.AreEquivalent(new[] { _a }, _indexedSet.Where(x => x.IntList, contains: 4).ToArray());
    }

    [TestMethod]
    public void single_queries_return_correct_results()
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
}
