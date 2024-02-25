using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Extensions;

namespace Akade.IndexedSet.Indices;

/// <summary>
/// O(log(n)) range queries based on <see cref="SortedLookup{TKey, TValue}"/>. Filters for distinct values as it is used
/// for indices where elements can have multiple keys.
/// </summary>
internal class MultiRangeIndex<TElement, TIndexKey> : TypedIndex<TElement, TIndexKey>
    where TIndexKey : notnull
{
    private readonly SortedLookup<TIndexKey, TElement> _lookup;

    public MultiRangeIndex(string name) : base(name)
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
        return _lookup.GetValuesInRange(start, end, inclusiveStart, inclusiveEnd).Distinct();
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _lookup.GetValues(indexKey).Distinct().SingleThrowingKeyNotFoundException();
    }

    internal override bool TryGetSingle(TIndexKey indexKey, out TElement? element)
    {
        using IEnumerator<TElement> enumerator = _lookup.GetValues(indexKey).GetEnumerator();

        if (enumerator.MoveNext())
        {
            element = enumerator.Current;
            while (enumerator.MoveNext())
            {
                if (!EqualityComparer<TElement>.Default.Equals(element, enumerator.Current))
                {
                    element = default;
                    return false;
                }
            }

            return true;
        }
        else
        {
            element = default;
            return false;
        }
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _lookup.GetValues(indexKey).Distinct();
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
            : _lookup.GetValuesInRange(value, maxKey, false, true).Distinct();
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
            : _lookup.GetValuesInRange(value, maxKey, true, true).Distinct();
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
            : _lookup.GetValuesInRange(minKey, value, true, false).Distinct();
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
            : _lookup.GetValuesInRange(minKey, value, true, true).Distinct();
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
        return _lookup.GetMaximumValues().Distinct();
    }

    internal override IEnumerable<TElement> MinBy()
    {
        return _lookup.GetMinimumValues().Distinct();
    }

    internal override IEnumerable<TElement> OrderBy(int skip)
    {
        return _lookup.GetValues(skip..).Distinct();
    }

    internal override IEnumerable<TElement> OrderByDescending(int skip)
    {
        return _lookup.GetValuesDescending(skip..).Distinct();
    }

    public override void Clear()
    {
        _lookup.Clear();
    }
}
