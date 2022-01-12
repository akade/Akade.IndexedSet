﻿namespace Akade.IndexedSet.DataStructures;

/// <summary>
/// Sorted lookup using <see cref="BinaryHeap{TValue}"/> as sorted key storage and a "normal"
/// <see cref="List{T}"/> as storage for the values where positions are kept in sync.
/// Provides O(log(n)) operations as well as range queries based on the key.
/// Suffers from the same insertion order problematic as <see cref="BinaryHeap{TValue}"/>
/// </summary>
internal class SortedLookup<TKey, TValue>
    where TKey : notnull
{
    private readonly List<TValue> _sortedValues = new();
    private readonly BinaryHeap<TKey> _sortedKeys = new();
    private readonly Func<TValue, TKey> _keyFunction;

    public SortedLookup(Func<TValue, TKey> keyFunction)
    {
        _keyFunction = keyFunction;
    }

    public void Add(TValue value)
    {
        TKey key = _keyFunction(value);

        int targetIndex = _sortedKeys.Add(key);
        _sortedValues.Insert(targetIndex, value);
    }

    public bool Remove(TValue value)
    {
        TKey key = _keyFunction(value);
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

    public IEnumerable<TValue> GetValuesInRange(TKey inclusiveStart, TKey exclusiveEnd)
    {
        Range range = _sortedKeys.GetRange(inclusiveStart, exclusiveEnd);
        return GetValues(range);
    }

    private IEnumerable<TValue> GetValues(Range range)
    {
        (int offset, int length) = range.GetOffsetAndLength(_sortedKeys.Count);

        for (int i = 0; i < length; i++)
        {
            yield return _sortedValues[i + offset];
        }
    }
}
