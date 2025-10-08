using Akade.IndexedSet.Utils;
using Akade.IndexedSet.StringUtilities;

namespace Akade.IndexedSet.Tests.Utils;

[TestClass]
public class LevensteinDistanceTests
{
    [TestMethod]
    public void Distance_zero_should_perform_normal_match()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0, EqualityComparer<char>.Default));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "Best", 0, EqualityComparer<char>.Default));
    }

    [TestMethod]
    public void Distance_one_should_only_match_a_single_change_or_deletion()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0, EqualityComparer<char>.Default));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Best", 1, EqualityComparer<char>.Default));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "est", 1, EqualityComparer<char>.Default));

        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "st", 1, EqualityComparer<char>.Default));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "Bast", 1, EqualityComparer<char>.Default));
    }

    [TestMethod]
    public void Distance_two_should_match_two_changes_or_deletions()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Test", 0, EqualityComparer<char>.Default));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Best", 2, EqualityComparer<char>.Default));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "est", 2, EqualityComparer<char>.Default));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "st", 2, EqualityComparer<char>.Default));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "Bast", 2, EqualityComparer<char>.Default));
    }

    [TestMethod]
    public void Distance_zero_should_perform_normal_match_IgnoreCase()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "tESt", 0, CharEqualityComparer.OrdinalIgnoreCase));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "bESt", 0, CharEqualityComparer.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Distance_one_should_only_match_a_single_change_or_deletion_IgnoreCase()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "tESt", 0, CharEqualityComparer.OrdinalIgnoreCase));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "bESt", 1, CharEqualityComparer.OrdinalIgnoreCase));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "eST", 1, CharEqualityComparer.OrdinalIgnoreCase));

        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "sT", 1, CharEqualityComparer.OrdinalIgnoreCase));
        Assert.IsFalse(LevenshteinDistance.FuzzyMatch("Test", "bAst", 1, CharEqualityComparer.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Distance_two_should_match_two_changes_or_deletions_IgnoreCase()
    {
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "tESt", 0, CharEqualityComparer.OrdinalIgnoreCase));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "bESt", 2, CharEqualityComparer.OrdinalIgnoreCase));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "eST", 2, CharEqualityComparer.OrdinalIgnoreCase));

        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "sT", 2, CharEqualityComparer.OrdinalIgnoreCase));
        Assert.IsTrue(LevenshteinDistance.FuzzyMatch("Test", "bAst", 2, CharEqualityComparer.OrdinalIgnoreCase));
    }
}
