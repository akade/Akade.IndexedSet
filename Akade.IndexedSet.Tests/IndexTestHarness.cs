using Akade.IndexedSet.Indices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Akade.IndexedSet.Tests;
internal abstract class BaseIndexTest<TKey, TElement, TIndex>
    where TIndex : TypedIndex<TElement, TKey>
    where TKey : notnull
{
    private readonly Func<TElement, TKey> _indexExpression;

    public BaseIndexTest(Func<TElement, TKey> indexExpression)
    {
        _indexExpression = indexExpression;
    }

    protected abstract TElement[] GetUniqueData();

    protected abstract TElement[] GetNonUniqueData();

    protected virtual TKey GetNotExistingKey()
    {
        return default!;
    }

    protected abstract bool SupportsNonUniqueKeys { get; }

    protected virtual bool SupportsRangeBasedQueries => false;

    protected abstract TIndex CreateIndex();

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
        Assert.AreEqual(data[0], index.Single(_indexExpression(data[0])));
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
            TKey nonUniqueKey = data.GroupBy(_indexExpression).Where(x => x.Count() > 1).First().Key;
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
        Assert.IsTrue(index.TryGetSingle(_indexExpression(data[0]), out TElement? element));
        Assert.AreEqual(data[0], element);
    }

    [TestMethod]
    public void TryGetSingle_should_return_false_if_multiple_entries_are_found()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);
            TKey nonUniqueKey = data.GroupBy(_indexExpression).Where(x => x.Count() > 1).First().Key;
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
        Assert.AreEqual(data[0], index.Where(_indexExpression(data[0])).Single());
    }

    [TestMethod]
    public void Where_should_return_multiple_element_for_unique_key()
    {
        if (SupportsNonUniqueKeys)
        {
            TElement[] data = GetNonUniqueData();
            IGrouping<TKey, TElement>[] groups = data.GroupBy(_indexExpression).Where(g => g.Count() > 1).ToArray();

            TIndex index = CreateIndexWithData(data);

            foreach (IGrouping<TKey, TElement> group in groups)
            {
                CollectionAssert.AreEquivalent(group.ToArray(), index.Where(group.Key).ToArray());
            }
        }
    }

    [TestMethod]
    public void Range_based_mnethods_should_throw_if_not_supported()
    {
        if (SupportsRangeBasedQueries)
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
        TIndex index = CreateIndex();
        Assert.IsFalse(index.Range(GetNotExistingKey(), GetNotExistingKey(), false, false).Any());
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
