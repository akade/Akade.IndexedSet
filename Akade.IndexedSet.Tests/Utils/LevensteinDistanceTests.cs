using Akade.IndexedSet.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Utils;

[TestClass]
public class LevensteinDistanceTests
{
    [TestMethod]
    public void distance_zero_should_perform_normal_match()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "Best", 0));
    }

    [TestMethod]
    public void distance_one_should_only_match_a_single_change_or_deletion()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Best", 1));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "est", 1));

        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "st", 1));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "Bast", 1));
    }

    [TestMethod]
    public void distance_two_should_match_two_changes_or_deletions()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Best", 2));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "est", 2));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "st", 2));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Bast", 2));
    }
}
