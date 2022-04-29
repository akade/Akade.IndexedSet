using Akade.IndexedSet.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
public class TrieTests
{
    [TestMethod]
    public void querying_common_prefixes_return_correct_elements()
    {
        Trie<string> trie = new();

        _ = AddToStringTrie(trie, "Tiger");
        _ = AddToStringTrie(trie, "Tarantula");
        _ = AddToStringTrie(trie, "Penguin");
        _ = AddToStringTrie(trie, "Panther");
        _ = AddToStringTrie(trie, "Pangolin");
        _ = AddToStringTrie(trie, "Parrot");

        CollectionAssert.AreEquivalent(new string[] { "Tiger", "Tarantula" }, trie.GetAll("T").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Penguin", "Panther", "Pangolin", "Parrot" }, trie.GetAll("P").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Pangolin", "Parrot" }, trie.GetAll("Pa").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther", "Pangolin" }, trie.GetAll("Pan").ToArray());
        CollectionAssert.AreEquivalent(new string[] { "Panther" }, trie.GetAll("Pant").ToArray());
    }

    [TestMethod]
    public void adding_the_same_element_return_false()
    {
        Trie<string> trie = new();

        Assert.IsTrue(AddToStringTrie(trie, "Cat"));
        Assert.IsFalse(AddToStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void contains_returns_correct_value_when_adding_elements()
    {
        Trie<string> trie = new();

        Assert.IsFalse(ContainsInStringTrie(trie, "Cat"));
        Assert.IsTrue(AddToStringTrie(trie, "Cat"));
        Assert.IsTrue(ContainsInStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void contains_returns_correct_value_when_removing_elements()
    {
        Trie<string> trie = new();

        _ = AddToStringTrie(trie, "Cat");
        Assert.IsTrue(ContainsInStringTrie(trie, "Cat"));
        Assert.IsTrue(RemoveFromStringTrie(trie, "Cat"));
        Assert.IsFalse(ContainsInStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void removing_returns_false_if_the_element_is_not_present()
    {
        Trie<string> trie = new();
        Assert.IsFalse(RemoveFromStringTrie(trie, "Cat"));
    }

    [TestMethod]
    public void fuzzy_search_hell_yeah()
    {
        Trie<string> trie = new();

        _ = AddToStringTrie(trie, "Tiger");
        _ = AddToStringTrie(trie, "Tarantula");
        _ = AddToStringTrie(trie, "Penguin");
        _ = AddToStringTrie(trie, "Panther");
        _ = AddToStringTrie(trie, "Pangolin");
        _ = AddToStringTrie(trie, "Parrot");

        IEnumerable<string> result = trie.FuzzySearch("Panter", 1, true);
        Assert.AreEqual("Panther", result.Single());
    }

    private static bool AddToStringTrie(Trie<string> stringTrie, string value)
    {
        return stringTrie.Add(value, value);
    }

    private static bool RemoveFromStringTrie(Trie<string> stringTrie, string value)
    {
        return stringTrie.Remove(value, value);
    }

    private static bool ContainsInStringTrie(Trie<string> stringTrie, string value)
    {
        return stringTrie.Contains(value, value);
    }
}
