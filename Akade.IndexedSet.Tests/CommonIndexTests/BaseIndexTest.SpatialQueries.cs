using Akade.IndexedSet.Tests.TestUtilities;

namespace Akade.IndexedSet.Tests.CommonIndexTests;

internal abstract partial class BaseIndexTest<TIndexKey, TElement, TIndex, TComparer>
{
    protected virtual bool SupportsSpatialQueries => false;

    protected virtual float NearestNeighborsDistance(TIndexKey a, TIndexKey b)
    {
        throw new NotSupportedException();
    }

    protected virtual bool Intersects(TIndexKey a, TIndexKey min, TIndexKey max, bool inclusiveBoundary)
    {
        throw new NotSupportedException();
    }

    [BaseTestMethod]
    public void NearestNeighbors_returns_all_elements_sorted_by_distance()
    {
        if (SupportsSpatialQueries)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);

            TIndexKey point = GetNotExistingKey();
            TElement[] orderedElements = [.. data.OrderBy(e => NearestNeighborsDistance(_keyAccessor(e), point))];

            CollectionAssert.AreEqual(orderedElements, index.NearestNeighbors(point).ToArray());
        }
    }

    [BaseTestMethod]
    public void Intersect_returns_all_intersecting_elements()
    {
        if (SupportsSpatialQueries)
        {
            TElement[] data = GetNonUniqueData();
            TIndex index = CreateIndexWithData(data);

            TElement[] orderedValues = data.OrderBy(_keyAccessor, ComparerUtils.GetComparer<TIndexKey>(_comparer)).ToArray();

            TIndexKey min = _keyAccessor(orderedValues[0]);
            TIndexKey max = _keyAccessor(orderedValues[^1]);

            TElement[] expectedElements = orderedValues.Where(v => Intersects(_keyAccessor(v), min, max, false)).ToArray();
            CollectionAssert.AreEquivalent(expectedElements, index.Intersects(min, max, false).ToArray());

            expectedElements = orderedValues.Where(v => Intersects(_keyAccessor(v), min, max, true)).ToArray();
            CollectionAssert.AreEquivalent(expectedElements, index.Intersects(min, max, true).ToArray());
        }
    }

    [BaseTestMethod]
    public void Spatial_methods_throw_if_not_supported()
    {
        if (!SupportsSpatialQueries)
        {
            TIndex index = CreateIndex();
            _ = Assert.ThrowsExactly<NotSupportedException>(() => index.Intersects(GetNotExistingKey(), GetNotExistingKey(), false).ToArray());
            _ = Assert.Throws<NotSupportedException>(() => index.NearestNeighbors(GetNotExistingKey()).ToArray());
        }
    }

}
