using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Akade.IndexedSet.Tests;
internal abstract class BaseIndexTest<TIndexKey, TSearchKey, TElement, TIndex>
    where TIndex : TypedIndex<TElement, TSearchKey>
    where TIndexKey : notnull
    where TSearchKey : notnull
{
    private readonly Func<TElement, TIndexKey> _indexExpression;
    private readonly Func<TElement, TSearchKey> _searchExpression;

    public BaseIndexTest(Func<TElement, TIndexKey> indexExpression)
    {
        _indexExpression = indexExpression;
        _searchExpression = element => SearchKeyFromIndexKey(indexExpression(element));
    }

    protected abstract TElement[] GetUniqueData();

    protected abstract TElement[] GetNonUniqueData();

    protected virtual TSearchKey GetNotExistingKey()
    {
        return default!;
    }

    protected abstract bool SupportsNonUniqueKeys { get; }

    protected virtual bool SupportsRangeBasedQueries => false;
    protected virtual bool SupportsStartsWithQueries => false;
    protected virtual bool SupportsContainsQueries => false;

    protected abstract TIndex CreateIndex();

    protected virtual TSearchKey SearchKeyFromIndexKey(TIndexKey key)
    {
        if (typeof(TSearchKey).IsAssignableFrom(typeof(TIndexKey)))
        {
            return (TSearchKey)(object)key;
        }
        throw new NotSupportedException($"{typeof(TIndexKey)} is not assignable to {typeof(TSearchKey)}");
    }

    private TIndex CreateIndexWithData(TElement[] elements)
    {
        TIndex result = CreateIndex();
        result.AddRange(elements);
        return result;
    }

    [TestMethod]
    public void Adding_unique_data_should_throw_if_nonunique_keys_are_not_supported()
    {
        if (!SupportsNonUniqueKeys)
        {
            _ = Assert.ThrowsException<ArgumentException>(() => CreateIndexWithData(GetNonUniqueData()));
        }
    }

    [TestMethod]
    public void Single_should_return_matching_element()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.AreEqual(data[0], index.Single(_searchExpression(data[0])));
    }

    [TestMethod]
    public void Single_should_throw_if_empty()
    {
        TIndex index = CreateIndex();
        _ = Assert.ThrowsException<KeyNotFoundException>(() => index.Single(GetNotExistingKey()));
    }

    [TestMethod]
    public void Single_should_throw_if_not_found()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        _ = Assert.ThrowsException<KeyNotFoundException>(() => index.Single(GetNotExistingKey()));
    }

    [TestMethod]
    public void Single_should_throw_if_multiple_entries_are_found()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);
            TSearchKey nonUniqueKey = data.GroupBy(_searchExpression).Where(x => x.Count() > 1).First().Key;
            _ = Assert.ThrowsException<InvalidOperationException>(() => index.Single(nonUniqueKey));
        }
    }

    [TestMethod]
    public void TryGetSingle_should_return_false_if_empty()
    {
        TIndex index = CreateIndex();
        Assert.IsFalse(index.TryGetSingle(GetNotExistingKey(), out _));
    }

    [TestMethod]
    public void TryGetSingle_should_return_false_if_key_is_not_present()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        Assert.IsFalse(index.TryGetSingle(GetNotExistingKey(), out _));
    }

    [TestMethod]
    public void TryGetSingle_should_return_true_for_matching_element()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.IsTrue(index.TryGetSingle(_searchExpression(data[0]), out TElement? element));
        Assert.AreEqual(data[0], element);
    }

    [TestMethod]
    public void TryGetSingle_should_return_false_if_multiple_entries_are_found()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);
            TSearchKey nonUniqueKey = data.GroupBy(_searchExpression).Where(x => x.Count() > 1).First().Key;
            Assert.IsFalse(index.TryGetSingle(nonUniqueKey, out _));
        }
    }

    [TestMethod]
    public void Where_should_return_empty_result_when_no_data_is_present()
    {
        TIndex index = CreateIndex();
        Assert.IsFalse(index.Where(GetNotExistingKey()).Any());
    }

    [TestMethod]
    public void Where_should_return_empty_result_when_key_is_not_present()
    {
        TIndex index = CreateIndexWithData(GetUniqueData());
        Assert.IsFalse(index.Where(GetNotExistingKey()).Any());
    }

    [TestMethod]
    public void Where_should_return_single_element_for_unique_key()
    {
        TElement[] data = GetUniqueData();
        TIndex index = CreateIndexWithData(data);
        Assert.AreEqual(data[0], index.Where(_searchExpression(data[0])).Single());
    }

    [TestMethod]
    public void Where_should_return_multiple_element_for_unique_key()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            IGrouping<TIndexKey, TElement>[] groups = data.GroupBy(_indexExpression).Where(g => g.Count() > 1).ToArray();

            TIndex index = CreateIndexWithData(data);

            foreach (IGrouping<TIndexKey, TElement> group in groups)
            {
                CollectionAssert.AreEquivalent(group.ToArray(), index.Where(SearchKeyFromIndexKey(group.Key)).ToArray());
            }
        }
    }

    [TestMethod]
    public void Range_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.Range(GetNotExistingKey(), GetNotExistingKey(), false, false));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.LessThan(GetNotExistingKey()));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.LessThanOrEqual(GetNotExistingKey()));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.GreaterThan(GetNotExistingKey()));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.GreaterThanOrEqual(GetNotExistingKey()));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.Min());
            _ = Assert.ThrowsException<NotSupportedException>(index.MinBy);
            _ = Assert.ThrowsException<NotSupportedException>(() => index.Max());
            _ = Assert.ThrowsException<NotSupportedException>(index.MaxBy);
            _ = Assert.ThrowsException<NotSupportedException>(() => index.OrderBy(0));
            _ = Assert.ThrowsException<NotSupportedException>(() => index.OrderByDescending(0));
        }
    }

    [TestMethod]
    public void Range_returns_empty_result_if_not_present()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.Range(GetNotExistingKey(), GetNotExistingKey(), false, false).Any());
        }
    }

    [TestMethod]
    public void Range_returns_sorted_elements_respecting_boundary_parameters_for_unique_data()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);

            TElement[] orderedElements = data.OrderBy(_searchExpression).ToArray();

            TSearchKey rangeStart = _searchExpression(orderedElements[1]);
            TSearchKey rangeEnd = _searchExpression(orderedElements[^2]);
            CollectionAssert.AreEqual(orderedElements[2..^2], index.Range(rangeStart, rangeEnd, inclusiveStart: false, inclusiveEnd: false).ToArray());
            CollectionAssert.AreEqual(orderedElements[2..^1], index.Range(rangeStart, rangeEnd, inclusiveStart: false, inclusiveEnd: true).ToArray());
            CollectionAssert.AreEqual(orderedElements[1..^2], index.Range(rangeStart, rangeEnd, inclusiveStart: true, inclusiveEnd: false).ToArray());
            CollectionAssert.AreEqual(orderedElements[1..^1], index.Range(rangeStart, rangeEnd, inclusiveStart: true, inclusiveEnd: true).ToArray());
        }
    }

    [TestMethod]
    public void Comparison_queries_return_empty_result_if_no_element_is_present()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.LessThan(GetNotExistingKey()).Any());
            Assert.IsFalse(index.LessThanOrEqual(GetNotExistingKey()).Any());
            Assert.IsFalse(index.GreaterThan(GetNotExistingKey()).Any());
            Assert.IsFalse(index.GreaterThanOrEqual(GetNotExistingKey()).Any());
        }
    }

    [TestMethod]
    public void Comparison_queries_return_sorted_elements_respecting_boundary()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            TElement[] orderedElements = data.OrderBy(_searchExpression).ToArray();

            TSearchKey boundary = _searchExpression(orderedElements[3]);
            CollectionAssert.AreEqual(orderedElements[0..3], index.LessThan(boundary).ToArray());
            CollectionAssert.AreEqual(orderedElements[0..4], index.LessThanOrEqual(boundary).ToArray());
            CollectionAssert.AreEqual(orderedElements[4..], index.GreaterThan(boundary).ToArray());
            CollectionAssert.AreEqual(orderedElements[3..], index.GreaterThanOrEqual(boundary).ToArray());
        }
    }

    [TestMethod]
    public void MaxMin_throw_if_the_set_is_empty()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<InvalidOperationException>(() => index.Min());
            _ = Assert.ThrowsException<InvalidOperationException>(() => index.Max());
        }
    }

    [TestMethod]
    public void MaxMin_return_empty_enumerable_if_the_set_is_empty()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.MinBy().Any());
            Assert.IsFalse(index.MaxBy().Any());
        }
    }

    [TestMethod]
    public void MaxMin_return_correct_key_and_values()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            TElement[] orderedElements = data.OrderBy(_searchExpression).ToArray();

            Assert.AreEqual(_searchExpression(orderedElements[0]), index.Min());
            Assert.AreEqual(orderedElements[0], index.MinBy().Single());

            Assert.AreEqual(_searchExpression(orderedElements[^1]), index.Max());
            Assert.AreEqual(orderedElements[^1], index.MaxBy().Single());
        }
    }

    [TestMethod]
    public void OrderBy_returns_empty_values_with_no_data()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.OrderBy(0).Any());
        }
    }

    [TestMethod]
    public void OrderBy_throws_if_skip_value_is_too_large()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            _ = Assert.ThrowsException<ArgumentOutOfRangeException>(() => index.OrderBy(data.Length + 1).Any());
        }
    }

    [TestMethod]
    public void OrderBy_returns_sorted_values()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            TElement[] orderedElements = data.OrderBy(_searchExpression).ToArray();

            for (int i = 0; i < orderedElements.Length; i++)
            {
                CollectionAssert.AreEqual(orderedElements[i..], index.OrderBy(i).ToArray());
            }
        }
    }

    [TestMethod]
    public void OrderByDescending_returns_empty_values_with_no_data()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.OrderByDescending(0).Any());
        }
    }

    [TestMethod]
    public void OrderByDescending_throws_if_skip_value_is_too_large()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            _ = Assert.ThrowsException<ArgumentOutOfRangeException>(() => index.OrderByDescending(data.Length + 1).Any());
        }
    }

    [TestMethod]
    public void OrderByDescending_returns_sorted_values()
    {
        if (SupportsRangeBasedQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);
            TElement[] orderedElements = data.OrderByDescending(_searchExpression).ToArray();

            for (int i = 0; i < orderedElements.Length; i++)
            {
                CollectionAssert.AreEqual(orderedElements[i..], index.OrderByDescending(i).ToArray());
            }
        }
    }

    [TestMethod]
    public void StartsWith_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.StartsWith(GetNotExistingKey().ToString()));
        }
    }

    [TestMethod]
    public void StartsWith_returns_empty_set_if_set_is_empty()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.StartsWith(GetNotExistingKey().ToString()).Any());
        }
    }

    [TestMethod]
    public void StartsWith_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            index.AddRange(GetUniqueData());
            Assert.IsFalse(index.StartsWith(GetNotExistingKey().ToString()).Any());
        }
    }

    [TestMethod]
    public void StartsWith_returns_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();

            index.AddRange(data);
            Assert.AreEqual(data[0], index.StartsWith(_searchExpression(data[0]).ToString()).Single());
        }
    }

    [TestMethod]
    public void StartsWith_returns_multiple_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);

            IGrouping<string, TElement> group = data.GroupBy(x => _searchExpression(x).ToString()![0..2])
                                                    .First(group => group.Count() > 1);

            CollectionAssert.AreEquivalent(group.ToArray(), index.StartsWith(group.Key).ToArray());
        }
    }

    [TestMethod]
    public void FuzzyStartsWith_based_methods_should_throw_if_not_supported()
    {
        if (!SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsException<NotSupportedException>(() => index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1));
        }
    }

    [TestMethod]
    public void FuzzyStartsWith_returns_empty_set_if_set_is_empty()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            Assert.IsFalse(index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [TestMethod]
    public void FuzzyStartsWith_returns_empty_set_if_no_matching_key_is_available()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            index.AddRange(GetUniqueData());
            Assert.IsFalse(index.FuzzyStartsWith(GetNotExistingKey().ToString(), 1).Any());
        }
    }

    [TestMethod]
    public void FuzzyStartsWith_returns_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();

            index.AddRange(data);
            string key = _searchExpression(data[0]).ToString() ?? throw new ArgumentException();
            string distanceOneKey = "z" + key[1..];

            Assert.AreEqual(data[0], index.FuzzyStartsWith(key, 0).Single());
            Assert.AreEqual(data[0], index.FuzzyStartsWith(distanceOneKey, 1).Single());
        }
    }

    [TestMethod]
    public void FuzzyStartsWith_returns_multiple_matching_item()
    {
        if (SupportsStartsWithQueries)
        {
            TIndex index = CreateIndex();
            TElement[] data = GetUniqueData();
            index.AddRange(data);

            IGrouping<string, TElement> group = data.GroupBy(x => _searchExpression(x).ToString()![0..2])
                                                    .First(group => group.Count() > 1);

            string distanceOneKey = "z" + group.Key[1..];

            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(group.Key, 0).ToArray());
            CollectionAssert.AreEquivalent(group.ToArray(), index.FuzzyStartsWith(distanceOneKey, 1).ToArray());
        }
    }
}

public static class IndexHelper
{
    public static IEnumerable<object[]> GetTestMethods<T>()
    {
        return typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.Name != "Test")
                        .Where(m => m.GetCustomAttribute(typeof(TestMethodAttribute)) is not null)
                        .Select(m => new object[] { m.Name })
                        .ToArray();
    }

    public static void RunTest<T>(string method)
    {
        T testClass = Activator.CreateInstance<T>();
        MethodInfo methodInfo = typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new ArgumentOutOfRangeException(nameof(method));

        _ = methodInfo.Invoke(testClass, null);
    }
}
