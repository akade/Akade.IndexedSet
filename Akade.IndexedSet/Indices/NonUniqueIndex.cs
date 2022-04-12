namespace Akade.IndexedSet.Indices;

/// <summary>
/// Nonunique index implementation based on <see cref="Lookup{TKey, TElement}"/>
/// </summary>
internal class NonUniqueIndex<TPrimaryKey, TElement, TIndexKey> : TypedIndex<TPrimaryKey, TElement, TIndexKey>
    where TPrimaryKey : notnull
    where TIndexKey : notnull
{
    private readonly DataStructures.Lookup<TIndexKey, TElement> _data = new();
    private readonly Func<TElement, TIndexKey> _keyAccessor;

    public NonUniqueIndex(Func<TElement, TIndexKey> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
    }

    public override void Add(TElement value)
    {
        TIndexKey key = _keyAccessor(value);
        _ = _data.Add(key, value);
    }

    public override void AddRange(IEnumerable<TElement> elementsToAdd)
    {
        if (elementsToAdd.TryGetNonEnumeratedCount(out int count))
        {
            _ = _data.EnsureCapacity(_data.Count + count);
        }

        base.AddRange(elementsToAdd);
    }

    public override void Remove(TElement value)
    {
        TIndexKey key = _keyAccessor(value);
        _ = _data.Remove(key, value);
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
