using Akade.IndexedSet.DataStructures;

namespace Akade.IndexedSet.Indices;

/// <summary>
/// O(log(n)) range queries based on <see cref="SortedLookup{TKey, TValue}"/>.
/// </summary>
internal class RangeIndex<TPrimaryKey, TElement, TIndexKey> : TypedIndex<TPrimaryKey, TElement, TIndexKey>
    where TPrimaryKey : notnull
    where TIndexKey : notnull
{
    private readonly SortedLookup<TIndexKey, TElement> _lookup;

    public RangeIndex(Func<TElement, TIndexKey> keyAccessor, string name) : base(keyAccessor, name)
    {
        _lookup = new SortedLookup<TIndexKey, TElement>(keyAccessor);
    }

    public override void Add(TElement value)
    {
        _lookup.Add(value);
    }

    public override void Remove(TElement value)
    {
        _ = _lookup.Remove(value);
    }

    internal override IEnumerable<TElement> Range(TIndexKey inclusiveStart, TIndexKey exclusiveStart)
    {
        return _lookup.GetValuesInRange(inclusiveStart, exclusiveStart);
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _lookup.GetValues(indexKey).Single();
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _lookup.GetValues(indexKey);
    }
}
