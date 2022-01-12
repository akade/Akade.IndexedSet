namespace Akade.IndexedSet.Indices;

/// <summary>
/// Nonunique index implementation based on <see cref="Lookup{TKey, TElement}"/>
/// </summary>
internal class MultiValueIndex<TPrimaryKey, TElement, TIndexKey> : TypedIndex<TPrimaryKey, TElement, TIndexKey>
    where TPrimaryKey : notnull
    where TIndexKey : notnull
{
    private readonly DataStructures.Lookup<TIndexKey, TElement> _data = new();

    public MultiValueIndex(Func<TElement, TIndexKey> keyAccessor, string name) : base(keyAccessor, name)
    {
    }

    public override void Add(TElement value)
    {
        TIndexKey key = _keyAccessor(value);
        _ = _data.Add(key, value);
    }

    public override void Remove(TElement value)
    {
        TIndexKey key = _keyAccessor(value);
        _ = _data.Remove(key, value);
    }

    internal override IEnumerable<TElement> Range(TIndexKey inclusiveStart, TIndexKey exclusiveStart)
    {
        throw new NotSupportedException($"Range queries are not supported on {nameof(MultiValueIndex<TPrimaryKey, TElement, TIndexKey>)}-indices. Use a range index to support this scenario.");
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _data.GetValues(indexKey).Single();
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _data.GetValues(indexKey);
    }
}
