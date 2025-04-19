namespace Akade.IndexedSet.DataStructures;

/// <summary>
/// Sorted lookup using <see cref="BinaryHeap{TValue}"/> as sorted key storage and a "normal"
/// <see cref="List{T}"/> as storage for the values where positions are kept in sync.
/// Provides O(log(n)) operations as well as range queries based on the key.
/// Suffers from the same insertion order problematic as <see cref="BinaryHeap{TValue}"/>
/// </summary>
internal class SortedLookup<TKey, TValue>(IComparer<TKey> keyComparer)
    where TKey : notnull
{
    private readonly List<TValue> _sortedValues = [];
    private readonly BinaryHeap<TKey> _sortedKeys = new(keyComparer);
    private readonly IComparer<TKey> _keyComparer = keyComparer;

    public void Add(TKey key, TValue value)
    {
        int targetIndex = _sortedKeys.Add(key);
        _sortedValues.Insert(targetIndex, value);
    }

    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> elementsToAdd)
    {
        if (elementsToAdd.TryGetNonEnumeratedCount(out int count))
        {
            _ = _sortedValues.EnsureCapacity(_sortedValues.Count + count);
            _ = _sortedKeys.EnsureCapacity(_sortedKeys.Count + count);
        }

        foreach (KeyValuePair<TKey, TValue> element in elementsToAdd.OrderBy(kvp => kvp.Key, _keyComparer))
        {
            Add(element.Key, element.Value);
        }
    }

    public bool Remove(TKey key, TValue value)
    {
        Range removalCandidates = _sortedKeys.GetRange(key);

        (int offset, int length) = removalCandidates.GetOffsetAndLength(_sortedKeys.Count);
        for (int i = 0; i < length; i++)
        {
            int index = offset + i;
            if (EqualityComparer<TValue>.Default.Equals(_sortedValues[index], value))
            {
                _sortedKeys.RemoveAt(index);
                _sortedValues.RemoveAt(index);
                return true;
            }
        }

        return false;
    }

    public IEnumerable<TValue> GetValues(TKey key)
    {
        Range range = _sortedKeys.GetRange(key);
        return GetValues(range);
    }

    public TValue Single(TKey key)
    {
        Range range = _sortedKeys.GetRange(key);
        (int offset, int length) = range.GetOffsetAndLength(_sortedKeys.Count);

        return length switch
        {
            0 => throw new KeyNotFoundException(),
            1 => _sortedValues[offset],
            _ => throw new InvalidOperationException()
        };
    }

    public int CountValues(TKey key)
    {
        Range range = _sortedKeys.GetRange(key);
        return range.GetOffsetAndLength(_sortedKeys.Count).Length;
    }

    public IEnumerable<TValue> GetValuesInRange(TKey start, TKey end, bool inclusiveStart, bool inclusiveEnd)
    {
        Range range = _sortedKeys.GetRange(start, end, inclusiveStart, inclusiveEnd);
        return GetValues(range);
    }

    public IEnumerable<TValue> GetValues(Range range)
    {
        (int offset, int length) = range.GetOffsetAndLength(_sortedKeys.Count);

        for (int i = 0; i < length; i++)
        {
            yield return _sortedValues[i + offset];
        }
    }

    public IEnumerable<TValue> GetMaximumValues()
    {
        if (_sortedKeys.Count == 0)
        {
            return [];
        }

        Range maximumRange = _sortedKeys.GetRange(GetMaximumKey());
        return GetValues(maximumRange);
    }

    public IEnumerable<TValue> GetMinimumValues()
    {
        if (_sortedKeys.Count == 0)
        {
            return [];
        }

        Range maximumRange = _sortedKeys.GetRange(GetMinimumKey());
        return GetValues(maximumRange);
    }

    public TKey GetMaximumKey()
    {
        return _sortedKeys.Count == 0
            ? throw new InvalidOperationException("Cannot retrieve the maximum value when the set is empty.")
            : _sortedKeys[^1];
    }

    public TKey GetMinimumKey()
    {
        return _sortedKeys.Count == 0
           ? throw new InvalidOperationException("Cannot retrieve the minimum value when the set is empty.")
           : _sortedKeys[0];
    }

    public int Count => _sortedKeys.Count;

    public IEnumerable<TValue> GetValuesDescending(Range range)
    {
        (int offset, int length) = range.GetOffsetAndLength(_sortedKeys.Count);

        for (int i = 1; i <= length; i++)
        {
            yield return _sortedValues[_sortedValues.Count - offset - i];
        }
    }

    internal void Clear()
    {
        _sortedKeys.Clear();
        _sortedValues.Clear();
    }
}
