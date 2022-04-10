using System.Collections;

namespace Akade.IndexedSet.DataStructures;

/// <summary>
/// O(log(n)) heap based on <see cref="List{T}.BinarySearch(T)"/>.
/// Does not preserve insertion order for same values.
/// Note that this implementation might suffer severe performance degeneration if
/// values are inserted in reverse order: insertion at the beginning requires moving all the values
/// within the list and its backing array.
/// </summary>
internal class BinaryHeap<TValue> : ICollection<TValue>
{
    private readonly List<TValue> _data = new();
    private readonly Comparer<TValue> _comparer = Comparer<TValue>.Default;

    public int Count => _data.Count;

    public bool IsReadOnly => false;

    public int Add(TValue value)
    {
        int index = _data.BinarySearch(value, _comparer);

        if (index < 0)
        {
            index = ~index;
        }

        _data.Insert(index, value);

        return index;
    }

    public void AddRange(IEnumerable<TValue> values)
    {
        foreach (TValue value in values)
        {
            _ = Add(value);
        }
    }

    public int IndexOf(TValue value)
    {
        return _data.BinarySearch(value);
    }

    public int EnsureCapacity(int capacity)
    {
        return _data.EnsureCapacity(capacity);
    }

    public bool Contains(TValue value)
    {
        return IndexOf(value) >= 0;
    }

    public void RemoveAt(int index)
    {
        _data.RemoveAt(index);
    }

    public int RemoveValue(TValue value)
    {
        int index = _data.BinarySearch(value, _comparer);

        if (index >= 0)
        {
            _data.RemoveAt(index);
        }

        return index;
    }

    public Range GetRange(TValue value)
    {
        if (_data.Count == 0)
        {
            return 0..0;
        }

        int start = _data.BinarySearch(value, _comparer);
        if (start < 0)
        {
            start = ~start;
        }

        int actualStart = GetFirstIndexWithValue(value, start);
        int end = GetFirstIndexWithDifferentValue(value, start);

        return new Range(actualStart, end);
    }

    public Range GetRange(TValue start, TValue end, bool inclusiveStart = true, bool inclusiveEnd = false)
    {
        if (_comparer.Compare(start, end) >= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(end));
        }

        if (_data.Count == 0)
        {
            return 0..0;
        }

        int startIndex = _data.BinarySearch(start, _comparer);
        if (startIndex < 0)
        {
            startIndex = ~startIndex;
        }

        startIndex = inclusiveStart ? GetFirstIndexWithValue(start, startIndex) : GetFirstIndexWithDifferentValue(start, startIndex);


        int endIndex = _data.BinarySearch(startIndex, _data.Count - startIndex, end, _comparer);

        if (endIndex < 0)
        {
            endIndex = ~endIndex;
        }

        if (endIndex < _data.Count)
        {
            endIndex = inclusiveEnd ? GetFirstIndexWithDifferentValue(end, endIndex) : GetFirstIndexWithValue(end, endIndex);
        }

        return new Range(startIndex, endIndex);
    }

    private int GetFirstIndexWithValue(TValue value, int start)
    {
        if (_comparer.Compare(_data[start], value) != 0)
        {
            return start;
        }

        while (start >= 0 && _comparer.Compare(_data[start], value) == 0)
        {
            start--;
        }

        return start + 1;
    }

    private int GetFirstIndexWithDifferentValue(TValue value, int start)
    {
        if (_comparer.Compare(_data[start], value) != 0)
        {
            return start;
        }

        while (start < _data.Count && _comparer.Compare(_data[start], value) == 0)
        {
            start++;
        }

        return start;
    }

    void ICollection<TValue>.Add(TValue item)
    {
        _ = Add(item);
    }

    public void Clear()
    {
        _data.Clear();
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        _data.CopyTo(array, arrayIndex);
    }

    public bool Remove(TValue item)
    {
        return RemoveValue(item) >= 0;
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TValue this[int index] => _data[index];
    public TValue this[Index index] => _data[index];
    public IEnumerable<TValue> this[Range range]
    {
        get
        {
            (int offset, int length) = range.GetOffsetAndLength(_data.Count);

            for (int i = 0; i < length; i++)
            {
                yield return _data[i + offset];
            }
        }
    }
}
