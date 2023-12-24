using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex>
{
    [TestMethod]
    public void Contains_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.Contains(GetNotExistingKey().ToString()));
        }
    }

    [TestMethod]
    public void Contains_returns_empty_set_if_set_is_empty()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.Contains(GetNotExistingKey().ToString()).Any());
        }
    }

    [TestMethod]
    public void Contains_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndexWithData(GetUniqueData());
            Assert.IsFalse(index.Contains(GetNotExistingKey().ToString()).Any());
        }
    }

    [TestMethod]
    public void Contains_returns_matching_item()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);
            Assert.AreEqual(data[0], index.Contains(_keyAccessor(data[0]).ToString()).Single());
        }
    }

    [TestMethod]
    public void Contains_returns_multiple_matching_item()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);

            IGrouping<string, TElement> group = data.GroupBy(x => _keyAccessor(x).ToString()![1..3])
                                                    .First(group => group.Count() > 1);

            CollectionAssert.AreEquivalent(group.ToArray(), index.Contains(group.Key).ToArray());
        }
    }

    [TestMethod]
    public void FuzzyContains_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.FuzzyContains(GetNotExistingKey().ToString(), 1));
        }
    }

    [TestMethod]
    public void FuzzyContains_returns_empty_set_if_set_is_empty()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.FuzzyContains(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [TestMethod]
    public void FuzzyContains_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndexWithData(GetUniqueData());
            Assert.IsFalse(index.FuzzyContains(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [TestMethod]
    public void FuzzyContains_returns_matching_item()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();

            AddElements(data, index);
            string key = _keyAccessor(data[0]).ToString() ?? throw new ArgumentException();
            string distanceOneKey = key[..2] + "z" + key[3..];

            Assert.AreEqual(data[0], index.FuzzyContains(key, 0).Single());
            Assert.AreEqual(data[0], index.FuzzyContains(distanceOneKey, 1).Single());
        }
    }

    [TestMethod]
    public void FuzzyContains_returns_multiple_matching_item()
    {
        if (SupportsContainsQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);

            IGrouping<string, TElement> group = data.GroupBy(x => _keyAccessor(x).ToString()![0..2])
                                                    .First(group => group.Count() > 1);

            string distanceOneKey = group.Key[0] + "z" + group.Key[1];

            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(group.Key, 0).ToArray());
            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(distanceOneKey, 1).ToArray());
        }
    }
}
