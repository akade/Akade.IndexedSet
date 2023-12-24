using Akade.IndexedSet.DataStructures;

namespace Akade.IndexedSet.Indices;

/// <summary>
/// O(log(n)) range queries based on <see cref="SortedLookup{TKey, TValue}"/>.
/// </summary>
internal class RangeIndex<TElement, TIndexKey> : TypedIndex<TElement, TIndexKey>
    where TIndexKey : notnull
{
    private readonly SortedLookup<TIndexKey, TElement> _lookup;

    public RangeIndex(string name) : base(name)
    {
        _lookup = new();
    }

    internal override void Add(TIndexKey key, TElement value)
    {
        _lookup.Add(key, value);
    }

    internal override void AddRange(IEnumerable<KeyValuePair<TIndexKey, TElement>> elementsToAdd)
    {
        _lookup.AddRange(elementsToAdd);
    }

    internal override void Remove(TIndexKey key, TElement value)
    {
        _ = _lookup.Remove(key, value);
    }

    internal override IEnumerable<TElement> Range(TIndexKey start, TIndexKey end, bool inclusiveStart, bool inclusiveEnd)
    {
        return _lookup.GetValuesInRange(start, end, inclusiveStart, inclusiveEnd);
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _lookup.Single(indexKey);
    }

    internal override bool TryGetSingle(TIndexKey indexKey, out TElement? element)
    {
        element = default;

        if (_lookup.CountValues(indexKey) == 1)
        {
            element = _lookup.GetValues(indexKey).Single();
            return true;
        }

        return false;
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _lookup.GetValues(indexKey);
    }

    internal override IEnumerable<TElement> GreaterThan(TIndexKey value)
    {
        if (_lookup.Count == 0)
        {
            return Enumerable.Empty<TElement>();
        }

        TIndexKey maxKey = _lookup.GetMaximumKey();

        return Comparer<TIndexKey>.Default.Compare(value, maxKey) >= 0
            ? Enumerable.Empty<TElement>()
            : _lookup.GetValuesInRange(value, maxKey, false, true);
    }

    internal override IEnumerable<TElement> GreaterThanOrEqual(TIndexKey value)
    {
        if (_lookup.Count == 0)
        {
            return Enumerable.Empty<TElement>();
        }

        TIndexKey maxKey = _lookup.GetMaximumKey();

        return Comparer<TIndexKey>.Default.Compare(value, maxKey) > 0
            ? Enumerable.Empty<TElement>()
            : _lookup.GetValuesInRange(value, maxKey, true, true);
    }

    internal override IEnumerable<TElement> LessThan(TIndexKey value)
    {
        if (_lookup.Count == 0)
        {
            return Enumerable.Empty<TElement>();
        }

        TIndexKey minKey = _lookup.GetMinimumKey();

        return Comparer<TIndexKey>.Default.Compare(value, minKey) <= 0
            ? Enumerable.Empty<TElement>()
            : _lookup.GetValuesInRange(minKey, value, true, false);
    }

    internal override IEnumerable<TElement> LessThanOrEqual(TIndexKey value)
    {
        if (_lookup.Count == 0)
        {
            return Enumerable.Empty<TElement>();
        }

        TIndexKey? minKey = _lookup.GetMinimumKey();

        return Comparer<TIndexKey>.Default.Compare(value, minKey) < 0
            ? Enumerable.Empty<TElement>()
            : _lookup.GetValuesInRange(minKey, value, true, true);
    }

    internal override TIndexKey Max()
    {
        return _lookup.GetMaximumKey();
    }

    internal override TIndexKey Min()
    {
        return _lookup.GetMinimumKey();
    }

    internal override IEnumerable<TElement> MaxBy()
    {
        return _lookup.GetMaximumValues();
    }

    internal override IEnumerable<TElement> MinBy()
    {
        return _lookup.GetMinimumValues();
    }

    internal override IEnumerable<TElement> OrderBy(int skip)
    {
        return _lookup.GetValues(skip..);
    }

    internal override IEnumerable<TElement> OrderByDescending(int skip)
    {
        return _lookup.GetValuesDescending(skip..);
    }

    public override void Clear()
    {
        _lookup.Clear();
    }
}
