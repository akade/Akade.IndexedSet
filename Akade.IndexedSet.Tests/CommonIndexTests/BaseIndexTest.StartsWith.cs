using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex>
{
    [BaseTestMethod]
    public void StartsWith_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.StartsWith(GetNotExistingKey().ToString()));
        }
    }

    [BaseTestMethod]
    public void StartsWith_returns_empty_set_if_set_is_empty()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.StartsWith(GetNotExistingKey().ToString()).Any());
        }
    }

    [BaseTestMethod]
    public void StartsWith_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            AddElements(GetUniqueData(), index);
            Assert.IsFalse(index.StartsWith(GetNotExistingKey().ToString()).Any());
        }
    }

    [BaseTestMethod]
    public void StartsWith_returns_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);
            Assert.AreEqual(data[0], index.StartsWith(_keyAccessor(data[0]).ToString()).Single());
        }
    }

    [BaseTestMethod]
    public void StartsWith_returns_multiple_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);

            IGrouping<string, TElement> group = data.GroupBy(x => _keyAccessor(x).ToString()![0..2])
                                                    .First(group => group.Count() > 1);

            CollectionAssert.AreEquivalent(group.ToArray(), index.StartsWith(group.Key).ToArray());
        }
    }

    [BaseTestMethod]
    public void FuzzyStartsWith_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1));
        }
    }

    [BaseTestMethod]
    public void FuzzyStartsWith_returns_empty_set_if_set_is_empty()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [BaseTestMethod]
    public void FuzzyStartsWith_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            AddElements(GetUniqueData(), index);
            Assert.IsFalse(index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [BaseTestMethod]
    public void FuzzyStartsWith_returns_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();

            AddElements(data, index);
            string key = _keyAccessor(data[0]).ToString() ?? throw new ArgumentException();
            string distanceOneKey = "z" + key[1..];

            Assert.AreEqual(data[0], index.FuzzyStartsWith(key, 0).Single());
            Assert.AreEqual(data[0], index.FuzzyStartsWith(distanceOneKey, 1).Single());
        }
    }

    [BaseTestMethod]
    public void FuzzyStartsWith_returns_multiple_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            AddElements(data, index);

            IGrouping<string, TElement> group = data.GroupBy(x => _keyAccessor(x).ToString()![0..4])
                                                    .First(group => group.Count() > 1);

            string distanceOneKey = "z" + group.Key[1..];

            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(group.Key, 0).ToArray());
            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(distanceOneKey, 1).ToArray());
        }
    }
}
