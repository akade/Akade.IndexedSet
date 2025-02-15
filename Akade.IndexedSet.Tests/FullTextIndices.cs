using Akade.IndexedSet.StringUtilities;
using Akade.IndexedSet.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.Tests;

[TestClass]
public class FullTextIndices
{
    private record class Animal(string Name, string Category);

    private IndexedSet<Animal> _indexedSet = null!;
    private Animal[] _allAnimals = null!;
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
        _allAnimals = [
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
        ];
        _indexedSet = _allAnimals.ToIndexedSet()
                                 .WithFullTextIndex(x => x.Category)
                                 .WithFullTextIndex(x => x.Name)
                                 .Build();
    }

    [TestMethod]
    public void single_item_retrieval_works()
    {
        _indexedSet.AssertSingleItem(x => x.Category, _boomslang);
        _indexedSet.AssertSingleItem(x => x.Category, _tarantula);
    }

    [TestMethod]
    public void single_item_retrieval_throws_exception_if_there_is_more_than_one_result()
    {
        _ = Assert.ThrowsException<InvalidOperationException>(() => _indexedSet.AssertSingleItem(x => x.Category, _bonobo));
    }

    [TestMethod]
    public void multi_item_retrieval_works()
    {
        _indexedSet.AssertMultipleItems(x => x.Category, expectedElements: [_bonobo, _borador, _tiger, _tapir, _panther, _pangolin]);
        _indexedSet.AssertMultipleItems(x => x.Category, expectedElements: [_booby, _penguin, _parrot]);
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
    public void search_via_contains()
    {
        CollectionAssert.AreEquivalent(new[] { _boomslang, _tarantula, _panther, _pangolin }, _indexedSet.Contains(x => x.Name, "an").ToArray());
    }

    [TestMethod]
    public void search_via_fuzzy_contains()
    {
        Animal[] actual = _indexedSet.FuzzyContains(x => x.Name, "Pan", 1).ToArray();
        CollectionAssert.AreEquivalent(new[] { _boomslang, _tarantula, _penguin, _parrot, _panther, _pangolin }, actual);
    }

    [TestMethod]
    public void TryGetSingle()
    {
        Assert.IsTrue(_indexedSet.TryGetSingle(x => x.Name, "Bonobo", out Animal? animal1));
        Assert.IsNotNull(animal1);

        Assert.IsFalse(_indexedSet.TryGetSingle(x => x.Name, "Elephant", out Animal? animal2));
        Assert.IsNull(animal2);
    }

    [TestMethod]
    [Experimental(Experiments.TextSearchImprovements)]
    public void Retrieval_via_multi_key_retrieves_correct_items()
    {
        static IEnumerable<string> Multikeys(Animal d) => [d.Name, d.Category];

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
                          .WithFullTextIndex(Multikeys)
                          .Build();

        // only reptile & spider
        _indexedSet.AssertSingleItem(Multikeys, _boomslang);
        _indexedSet.AssertSingleItem(Multikeys, _tarantula);

        CollectionAssert.AreEquivalent(_indexedSet.Where(Multikeys, "Bird").ToArray(), new[] { _booby, _penguin, _parrot });

        CollectionAssert.AreEquivalent(_indexedSet.Contains(Multikeys, "ird").ToArray(), new[] { _booby, _penguin, _parrot });
        CollectionAssert.AreEquivalent(_indexedSet.FuzzyContains(Multikeys, "ard", 1).ToArray(), new[] { _borador, _booby, _penguin, _parrot, _tarantula });

        CollectionAssert.AreEquivalent(_indexedSet.StartsWith(Multikeys, "Bir").ToArray(), new[] { _booby, _penguin, _parrot });
        CollectionAssert.AreEquivalent(_indexedSet.FuzzyStartsWith(Multikeys, "Lir", 1).ToArray(), new[] { _booby, _penguin, _parrot });
    }

    [TestMethod]
    public void Case_insensitive_matching()
    {
        static string CategoryCaseInsensitive(Animal a) => a.Category;

        _indexedSet = _allAnimals.ToIndexedSet()
                                 .WithFullTextIndex(CategoryCaseInsensitive, CharEqualityComparer.OrdinalIgnoreCase)
                                 .Build();

        Animal[] actual = _indexedSet.StartsWith(CategoryCaseInsensitive, "MAMM").ToArray();
        CollectionAssert.AreEquivalent(_allAnimals.Where(x => x.Category == "Mammal").ToArray(), actual);


    }

    [TestMethod]
    public void Case_insensitive_fuzzy_matching()
    {
        static string NameCaseInsensitive(Animal a) => a.Name;

        _indexedSet = _allAnimals.ToIndexedSet()
                                 .WithFullTextIndex(NameCaseInsensitive, CharEqualityComparer.OrdinalIgnoreCase)
                                 .Build();

        Animal[] actual = _indexedSet.FuzzyStartsWith(NameCaseInsensitive, "PAN", 1).ToArray();
        CollectionAssert.AreEquivalent(new[] { _penguin, _parrot, _panther, _pangolin }, actual);
    }
}
