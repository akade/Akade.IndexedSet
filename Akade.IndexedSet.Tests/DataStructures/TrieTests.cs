﻿using Akade.IndexedSet.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "In unit tests: readability > performance")]
public class TrieTests
{
    private readonly Trie<string> _trie = GetAnimalTrie();

    private static Trie<string> GetAnimalTrie()
    {
        Trie<string> trie = new();

        _ = AddToStringTrie(trie, "Tiger");
        _ = AddToStringTrie(trie, "Tarantula");
        _ = AddToStringTrie(trie, "Penguin");
        _ = AddToStringTrie(trie, "Panther");
        _ = AddToStringTrie(trie, "Pangolin");
        _ = AddToStringTrie(trie, "Parrot");
        return trie;
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
    public void adding_the_same_element_returns_false()
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
    public void exact_fuzzy_search_with_single_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Panter", 1, true);
        Assert.AreEqual("Panther", result.Single());
    }

    [TestMethod]
    public void exact_fuzzy_search_without_results()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Panner", 1, true);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]

    public void inexact_fuzzy_search_and_single_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Pangolin", 2, false);
        CollectionAssert.AreEquivalent(new[] { "Pangolin" }, result.ToArray());
    }

    [TestMethod]
    public void inexact_fuzzy_search_and_multiple_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Pan", 2, false);
        CollectionAssert.AreEquivalent(new[] { "Penguin", "Panther", "Pangolin", "Parrot", "Tarantula" }, result.ToArray());
    }

    [TestMethod]
    public void inexact_fuzzy_search_without_result()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Non", 1, false);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void inexact_fuzzy_search_and_multiple_result_with_first_character_changed()
    {
        IEnumerable<string> result = _trie.FuzzySearch("Zan", 1, false);
        CollectionAssert.AreEquivalent(new[] { "Panther", "Pangolin" }, result.ToArray());
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
