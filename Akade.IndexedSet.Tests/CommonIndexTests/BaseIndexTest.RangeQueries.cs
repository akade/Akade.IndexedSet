using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.CommonIndexTests;
internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex>
{
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
            AddElements(data, index);

            TElement[] orderedElements = data.OrderBy(_keyAccessor).ToArray();

            TIndexKey rangeStart = _keyAccessor(orderedElements[1]);
            TIndexKey rangeEnd = _keyAccessor(orderedElements[^2]);
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
            AddElements(data, index);
            TElement[] orderedElements = data.OrderBy(_keyAccessor).ToArray();

            TIndexKey boundary = _keyAccessor(orderedElements[3]);
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
            AddElements(data, index);

            TElement[] orderedElements = data.OrderBy(_keyAccessor).ToArray();

            Assert.AreEqual(_keyAccessor(orderedElements[0]), index.Min());
            Assert.AreEqual(orderedElements[0], index.MinBy().Single());

            Assert.AreEqual(_keyAccessor(orderedElements[^1]), index.Max());
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
            AddElements(data, index);
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
            AddElements(data, index);
            TElement[] orderedElements = data.OrderBy(_keyAccessor).ToArray();

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
            AddElements(data, index);
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
            AddElements(data, index);
            TElement[] orderedElements = data.OrderByDescending(_keyAccessor).ToArray();

            for (int i = 0; i < orderedElements.Length; i++)
            {
                CollectionAssert.AreEqual(orderedElements[i..], index.OrderByDescending(i).ToArray());
            }
        }
    }
}
