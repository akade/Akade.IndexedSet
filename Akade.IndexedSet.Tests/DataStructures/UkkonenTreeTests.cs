// Ignore Spelling: Ukkonen

using Akade.IndexedSet.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.DataStructures;

[TestClass]
public class UkkonenTreeTests
{
    [TestMethod]
    public void UkkonenTree_CreatesTreeForSingleCharacter()
    {
        // Act
        var tree = new UkkonenTree();
        tree.Add("a");
    }

    [TestMethod]
    public void UkkonenTree_CreatesTreeForMultipleCharacters()
    {
        // Act
        var tree = new UkkonenTree();
        tree.Add("abc");
    }

    [TestMethod]
    public void UkkonenTree_CreatesTreeForRepeatedCharacters()
    {
        // Act
        var tree = new UkkonenTree();
        tree.Add("aaa");
    }

    [TestMethod]
    public void UkkonenTree_CreatesTreeForEmptyString()
    {
        // Act
        var tree = new UkkonenTree();
        tree.Add("");
    }

    [TestMethod]
    public void UkkonenTree_CreatesTreeForComplexString()
    {
        // Act
        var tree = new UkkonenTree();
        tree.Add("xabxac");

    }
}

