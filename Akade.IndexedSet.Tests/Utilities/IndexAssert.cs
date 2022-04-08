using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Tests.Utilities;

internal static class IndexAssert
{
    public static void AssertSingleItem<TPrimaryKey, TElement, TIndexKey>(this IndexedSet<TPrimaryKey, TElement> indexedSet, Func<TElement, TIndexKey> indexAccessor, TElement testData, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TPrimaryKey : notnull
        where TIndexKey : notnull
    {
        Assert.IsNotNull(indexName);

        TIndexKey indexKey = indexAccessor(testData);
        TElement accessViaSingle = indexedSet.Single(indexAccessor, indexKey, indexName);
        TElement accessViaWhere = indexedSet.Where(indexAccessor, indexKey, indexName).Single();

        Assert.AreEqual(testData, accessViaSingle);
        Assert.AreEqual(testData, accessViaWhere);
    }

    public static void AssertMultipleItems<TPrimaryKey, TElement, TIndexKey>(this IndexedSet<TPrimaryKey, TElement> indexedSet, Func<TElement, TIndexKey> indexAccessor, [CallerArgumentExpression("indexAccessor")] string? indexName = null, bool requireOrder = false, params TElement[] expectedElements)
        where TPrimaryKey : notnull
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

    public static void AssertMultipleItemsViaRange<TPrimaryKey, TElement, TIndexKey>
        (this IndexedSet<TPrimaryKey, TElement> indexedSet,
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey start,
        TIndexKey end,
        bool inclusiveStart,
        bool inclusiveEnd,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null,
        params TElement[] expectedElements)
        where TPrimaryKey : notnull
        where TIndexKey : notnull
    {
        Assert.IsNotNull(indexName);

        IEnumerable<TElement> actualElements = indexedSet.Range(indexAccessor, start, end, inclusiveStart, inclusiveEnd, indexName);

        CollectionAssert.AreEqual(expectedElements.Select(indexAccessor).ToArray(), actualElements.Select(indexAccessor).ToArray());
        CollectionAssert.AreEquivalent(expectedElements, actualElements.ToArray());
    }
}
