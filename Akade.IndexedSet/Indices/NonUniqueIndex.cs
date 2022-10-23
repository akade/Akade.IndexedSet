namespace Akade.IndexedSet.Indices;

/// <summary>
/// Nonunique index implementation based on <see cref="Lookup{TKey, TElement}"/>
/// </summary>
internal class NonUniqueIndex<TElement, TIndexKey> : TypedIndex<TElement, TIndexKey>
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

    public override void Clear()
    {
        _data.Clear();
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

    internal override bool TryGetSingle(TIndexKey indexKey, out TElement? element)
    {
        element = default;

        if (_data.CountValues(indexKey) == 1)
        {
            element = _data.GetValues(indexKey).Single();
            return true;
        }

        return false;
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _data.GetValues(indexKey);
    }
}
