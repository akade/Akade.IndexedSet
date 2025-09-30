using Akade.IndexedSet.Utils;

namespace Akade.IndexedSet.Indices;

/// <summary>
/// Unique index providing O(1) retrieval and insertion as well as enforcing unqueness
/// </summary>
internal sealed class UniqueIndex<TElement, TIndexKey>(IEqualityComparer<TIndexKey> equalityComparer, string name) : TypedIndex<TElement, TIndexKey>(name)
    where TIndexKey : notnull
{
    private readonly Dictionary<TIndexKey, TElement> _data = new(equalityComparer);

    internal override void Add(TIndexKey key, TElement value)
    {
        try
        {
            _data.Add(key, value);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"An item with the key \"{key}\" has already been added to the unique index \"{Name}\".");
        }
    }

    internal override void AddRange(IKeyValueEnumerator<TIndexKey, TElement> elementsToAdd)
    {
        if (elementsToAdd.TryGetEstimatedCount(out int count))
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
        return _data.TryGetValue(indexKey, out TElement? result) ? [result] : [];
    }
}
