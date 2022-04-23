namespace Akade.IndexedSet.DataStructures;

/// <summary>
/// Modifiable lookup based on <see cref="Dictionary{TKey, TValue}"/> with <see cref="HashSet{T}"/> as value collection per key.
/// O(1) retreival and insertion.
/// </summary>
internal class Lookup<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, HashSet<TValue>> _values = new();

    public bool Add(TKey key, TValue value)
    {
        if (!_values.TryGetValue(key, out HashSet<TValue>? keySet))
        {
            keySet = _values[key] = new();
        }

        if (keySet.Add(value))
        {
            Count++;
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerable<TValue> GetValues(TKey key)
    {
        return _values.TryGetValue(key, out HashSet<TValue>? keySet) ? keySet : Enumerable.Empty<TValue>();
    }

    public int CountValues(TKey key)
    {
        return _values.TryGetValue(key, out HashSet<TValue>? keySet) ? keySet.Count : 0;
    }

    public bool Remove(TKey key, TValue value)
    {
        if (!_values.TryGetValue(key, out HashSet<TValue>? keySet))
        {
            return false;
        }

        if (keySet.Remove(value))
        {
            Count--;
            if (keySet.Count == 0)
            {
                _ = _values.Remove(key);
            }

            return true;
        }

        return false;
    }

    public int Count { get; private set; }

    public int EnsureCapacity(int capacity)
    {
        return _values.EnsureCapacity(capacity);
    }
}
