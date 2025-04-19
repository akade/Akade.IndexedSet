namespace Akade.IndexedSet.Indices;

/// <summary>
/// Nonunique index implementation based on <see cref="DataStructures.Lookup{TKey, TElement}"/>
/// </summary>
internal sealed class NonUniqueIndex<TElement, TIndexKey>(IEqualityComparer<TIndexKey> equalityComparer, string name) : TypedIndex<TElement, TIndexKey>(name)
    where TIndexKey : notnull
{
    private readonly DataStructures.Lookup<TIndexKey, TElement> _data = new(equalityComparer);

    internal override void Add(TIndexKey key, TElement value)
    {
        _ = _data.Add(key, value);
    }

    internal override void AddRange(IEnumerable<KeyValuePair<TIndexKey, TElement>> elementsToAdd)
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

    internal override void Remove(TIndexKey key, TElement value)
    {
        _ = _data.Remove(key, value);
    }

    internal override TElement Single(TIndexKey indexKey)
    {
        return _data.Single(indexKey);
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
