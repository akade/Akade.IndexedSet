using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Akade.IndexedSet.Tests.TestUtilities;
#if NET7_0_OR_GREATER
[SuppressMessage("Style", "IDE0280:Use 'nameof'", Justification = ".NET 6 is still supported")]
#endif
internal static class IndexAssert
{
    public static void AssertSingleItem<TElement, TIndexKey>(this IndexedSet<TElement> indexedSet, Func<TElement, TIndexKey> indexAccessor, TElement testData, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        Assert.IsNotNull(indexName);

        TIndexKey indexKey = indexAccessor(testData);
        TElement accessViaSingle = indexedSet.Single(indexAccessor, indexKey, indexName);
        TElement accessViaWhere = indexedSet.Where(indexAccessor, indexKey, indexName).Single();

        Assert.AreEqual(testData, accessViaSingle);
        Assert.AreEqual(testData, accessViaWhere);
    }

    public static void AssertSingleItem<TElement, TIndexKey>(this IndexedSet<TElement> indexedSet, Func<TElement, IEnumerable<TIndexKey>> indexAccessor, TElement testData, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        Assert.IsNotNull(indexName);

        foreach (TIndexKey indexKey in indexAccessor(testData))
        {
            TElement accessViaSingle = indexedSet.Single(indexAccessor, indexKey, indexName);
            TElement accessViaWhere = indexedSet.Where(indexAccessor, indexKey, indexName).Single();

            Assert.AreEqual(testData, accessViaSingle);
            Assert.AreEqual(testData, accessViaWhere);
        }
    }

    public static void AssertMultipleItems<TElement, TIndexKey>(this IndexedSet<TElement> indexedSet, Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null, bool requireOrder = false, params TElement[] expectedElements)
        where TIndexKey : notnull
    {
        if (expectedElements.Length < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedElements));
        }
        Assert.IsNotNull(indexName);

        TIndexKey indexKey = expectedElements.Select(indexAccessor).Distinct().Single();

        try
        {
            _ = indexedSet.Single(indexAccessor, indexKey, indexName);
            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
            // We expect multiple elements => this has to fail
        }

        IEnumerable<TElement> accessViaWhere = indexedSet.Where(indexAccessor, indexKey, indexName);

        if (requireOrder)
        {
            CollectionAssert.AreEqual(expectedElements, accessViaWhere.ToArray());
        }
        else
        {
            CollectionAssert.AreEquivalent(expectedElements, accessViaWhere.ToArray());
        }
    }

    public static void AssertMultipleItemsViaRange<TElement, TIndexKey>
        (this IndexedSet<TElement> indexedSet,
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart,
        bool inclusiveEnd,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        params TElement[] expectedElements)
        where TIndexKey : notnull
    {
        Assert.IsNotNull(indexName);

        IEnumerable<TElement> actualElements = indexedSet.Range(indexAccessor, start, end, inclusiveStart, inclusiveEnd, indexName);

        CollectionAssert.AreEqual(expectedElements.Select(indexAccessor).ToArray(), actualElements.Select(indexAccessor).ToArray());
        CollectionAssert.AreEquivalent(expectedElements, actualElements.ToArray());
    }
}
