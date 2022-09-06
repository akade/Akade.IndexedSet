namespace Akade.IndexedSet.Indices;

/// <summary>
/// Unique index providing O(1) retrieval and insertion as well as enforcing unqueness
/// </summary>
internal class UniqueIndex<TElement, TIndexKey> : TypedIndex<TElement, TIndexKey>
    where TIndexKey : notnull
{
    private readonly Dictionary<TIndexKey, TElement> _data = new();
    private readonly Func<TElement, TIndexKey> _keyAccessor;

    public UniqueIndex(Func<TElement, TIndexKey> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
    }

    public override void Add(TElement value)
    {
        TIndexKey key = _keyAccessor(value);
        try
        {
            _data.Add(key, value);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"An item with the key \"{key}\" has already been added to the unique index \"{Name}\".");
        }
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
        _ = _data.Remove(key);
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _data[indexKey];
    }

    internal override bool TryGetSingle(TIndexKey indexKey, out TElement? element)
    {
        return _data.TryGetValue(indexKey, out element);
    }

    internal override IEnumerable<TElement> Where(TIndexKey indexKey)
    {
        return _data.TryGetValue(indexKey, out TElement? result) ? new[] { result } : Enumerable.Empty<TElement>();
    }
}
