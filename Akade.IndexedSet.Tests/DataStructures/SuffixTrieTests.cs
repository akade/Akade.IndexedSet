using Akade.IndexedSet.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
public class SuffixTrieTests
{
    private SuffixTrie<string> _trie = default!;

    [TestInitialize]
    public void TestInitializer()
    {
        _trie = new();
        _ = AddToStringTrie(_trie, "Tiger");
        _ = AddToStringTrie(_trie, "Tarantula");
        _ = AddToStringTrie(_trie, "Penguin");
        _ = AddToStringTrie(_trie, "Panther");
        _ = AddToStringTrie(_trie, "Pangolin");
        _ = AddToStringTrie(_trie, "Parrot");
        _ = AddToStringTrie(_trie, "Chihuahua");
    }

    [TestMethod]
    public void querying_common_prefixes_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new string[] { "Tiger", "Tarantula" }, _trie.GetAll("T").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Penguin", "Panther", "Pangolin", "Parrot" }, _trie.GetAll("P").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Pangolin", "Parrot" }, _trie.GetAll("Pa").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Pangolin" }, _trie.GetAll("Pan").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther" }, _trie.GetAll("Pant").ToArray());
    }

    [TestMethod]
    public void querying_common_infixes_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Pangolin", "Tarantula" }, _trie.GetAll("an").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Chihuahua" }, _trie.GetAll("hua").ToArray());
    }

    [TestMethod]
    public void querying_common_suffixes_return_correct_elements()
    {
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Tiger" }, _trie.GetAll("er").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Chihuahua" }, _trie.GetAll("hua").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Penguin", "Pangolin" }, _trie.GetAll("in").ToArray());
    }

    [TestMethod]
    public void adding_the_same_element_return_false()
    {
        SuffixTrie<string> trie = new();

        Assert.IsTrue(AddToStringTrie(trie, "Cat"));
        Assert.IsFalse(AddToStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void contains_returns_correct_value_when_adding_elements()
    {
        SuffixTrie<string> trie = new();

        Assert.IsFalse(ContainsInStringTrie(trie, "Cat"));
        Assert.IsTrue(AddToStringTrie(trie, "Cat"));
        Assert.IsTrue(ContainsInStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void contains_returns_correct_value_when_removing_elements()
    {
        SuffixTrie<string> trie = new();

        _ = AddToStringTrie(trie, "Cat");
        Assert.IsTrue(ContainsInStringTrie(trie, "Cat"));
        Assert.IsTrue(RemoveFromStringTrie(trie, "Cat"));
        Assert.IsFalse(ContainsInStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void removing_returns_false_if_the_element_is_not_present()
    {
        SuffixTrie<string> trie = new();
        Assert.IsFalse(RemoveFromStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void exact_fuzzy_search_with_single_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("rantul", 1, true);
        Assert.AreEqual("Tarantula", result.Single());
    }

    [TestMethod]
    public void exact_fuzzy_search_without_results()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Panner", 1, true);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]

    public void inexact_fuzzy_search_and_multiple_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Pan", 2, false);
        CollectionAssert.AreEquivalent(new[] { "Penguin", "Panther", "Pangolin", "Parrot", "Tarantula", "Chihuahua" }, result.ToArray());
    }

    [TestMethod]
    public void inexact_fuzzy_search_without_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Non", 1, false);
        Assert.IsFalse(result.Any());
    }


    // https://murilo.wordpress.com/2011/02/01/fast-and-easy-levenshtein-distance-using-a-trie-in-c/
    private static bool AddToStringTrie(SuffixTrie<string> stringTrie, string value)
    {
        return stringTrie.Add(value, value);
    }

    private static bool RemoveFromStringTrie(SuffixTrie<string> stringTrie, string value)
    {
        return stringTrie.Remove(value, value);
    }

    private static bool ContainsInStringTrie(SuffixTrie<string> stringTrie, string value)
    {
        return stringTrie.Contains(value, value);
    }
}
