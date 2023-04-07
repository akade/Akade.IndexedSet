using Akade.IndexedSet.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class PrefixIndices
{
    private record class Animal(string Name, string Category);

    private IndexedSet<Animal> _indexedSet = null!;
    private readonly Animal _bonobo = new("Bonobo", "Mammal");
    private readonly Animal _booby = new("Booby", "Bird");
    private readonly Animal _boomslang = new("Boomslang", "Reptile");
    private readonly Animal _borador = new("Borador", "Mammal");
    private readonly Animal _tiger = new("Tiger", "Mammal");
    private readonly Animal _tarantula = new("Tarantula", "Spider");
    private readonly Animal _tapir = new("Tapir", "Mammal");
    private readonly Animal _penguin = new("Penguin", "Bird");
    private readonly Animal _panther = new("Panther", "Mammal");
    private readonly Animal _pangolin = new("Pangolin", "Mammal");
    private readonly Animal _parrot = new("Parrot", "Bird");

    [TestInitialize]
    public void Init()
    {
        var data = new Animal[] {
            _bonobo,
            _booby,
            _boomslang,
            _borador,
            _tiger,
            _tarantula,
            _tapir,
            _penguin,
            _panther,
            _pangolin,
            _parrot,
        };
        _indexedSet = data.ToIndexedSet()
                          .WithPrefixIndex(x => x.Category)
                          .WithPrefixIndex(x => x.Name)
                          .Build();
    }

    [TestMethod]
    public void single_item_retrieval_works()
    {
        _indexedSet.AssertSingleItem(x => x.Category, _boomslang);
        _indexedSet.AssertSingleItem(x => x.Category, _tarantula);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void single_item_retrieval_throws_exception_if_there_is_more_than_one_result()
    {
        _indexedSet.AssertSingleItem(x => x.Category, _bonobo);
    }

    [TestMethod]
    public void multi_item_retrieval_works()
    {
        _indexedSet.AssertMultipleItems(x => x.Category, expectedElements: new[] { _bonobo, _borador, _tiger, _tapir, _panther, _pangolin });
        _indexedSet.AssertMultipleItems(x => x.Category, expectedElements: new[] { _booby, _penguin, _parrot });
    }

    [TestMethod]
    public void search_via_starts_with()
    {
        CollectionAssert.AreEquivalent(new[] { _booby, _boomslang }, _indexedSet.StartsWith(x => x.Name, "Boo").ToArray());
        CollectionAssert.AreEquivalent(new[] { _panther, _pangolin }, _indexedSet.StartsWith(x => x.Name, "Pan").ToArray());
    }

    [TestMethod]
    public void search_via_fuzzy_starts_with()
    {
        CollectionAssert.AreEquivalent(new[] { _bonobo, _booby, _boomslang, _borador }, _indexedSet.FuzzyStartsWith(x => x.Name, "Boo", 1).ToArray());
        CollectionAssert.AreEquivalent(new[] { _penguin, _parrot, _panther, _pangolin }, _indexedSet.FuzzyStartsWith(x => x.Name, "Pan", 1).ToArray());
    }

    [TestMethod]
    public void TryGetSingle()
    {
        Assert.IsTrue(_indexedSet.TryGetSingle(x => x.Name, "Bonobo", out Animal? animal1));
        Assert.IsNotNull(animal1);

        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.Name, "Elephant", out Animal? animal2));
        Assert.IsNull(animal2);
    }
}
