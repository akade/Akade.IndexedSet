﻿namespace Akade.IndexedSet.Indices;

/// <summary>
/// Fully-generic index including on the index key
/// </summary>
internal abstract class TypedIndex<TElement, TIndexKey>(string name) : Index<TElement>(name)
    where TIndexKey : notnull
{
    internal abstract void Add(TIndexKey key, TElement value);
    internal abstract void Remove(TIndexKey key, TElement value);

    internal virtual void AddRange(IEnumerable<KeyValuePair<TIndexKey, TElement>> enumerable)
    {
        foreach (KeyValuePair<TIndexKey, TElement> kvp in enumerable)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    internal abstract TElement Single(TIndexKey indexKey);
    internal abstract bool TryGetSingle(TIndexKey indexKey, out TElement? element);

    internal abstract IEnumerable<TElement> Where(TIndexKey indexKey);

    internal virtual IEnumerable<TElement> Range(TIndexKey start, TIndexKey end, bool inclusiveStart, bool inclusiveEnd)
    {
        throw new NotSupportedException($"Range queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> LessThan(TIndexKey value)
    {
        throw new NotSupportedException($"LessThan queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> LessThanOrEqual(TIndexKey value)
    {
        throw new NotSupportedException($"LessThanOrEquals queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> GreaterThan(TIndexKey value)
    {
        throw new NotSupportedException($"GreaterThan queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> GreaterThanOrEqual(TIndexKey value)
    {
        throw new NotSupportedException($"GreaterThanOrEqual queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual TIndexKey Min()
    {
        throw new NotSupportedException($"Min queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual TIndexKey Max()
    {
        throw new NotSupportedException($"Max queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> MinBy()
    {
        throw new NotSupportedException($"MinBy queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> MaxBy()
    {
        throw new NotSupportedException($"MaxBy queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> OrderBy(int skip)
    {
        throw new NotSupportedException($"OrderBy queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> OrderByDescending(int skip)
    {
        throw new NotSupportedException($"OrderByDescending queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> FuzzyStartsWith(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        throw new NotSupportedException($"Fuzzy starts with queries are not supported on {GetType().Name}-indices. Use a full text or prefix index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> FuzzyContains(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        throw new NotSupportedException($"Fuzzy contains queries are not supported on {GetType().Name}-indices. Use a full text or prefix index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> StartsWith(ReadOnlySpan<char> indexKey)
    {
        throw new NotSupportedException($"Fuzzy queries are not supported on {GetType().Name}-indices. Use a full text or or prefix to support this scenario.");
    }

    internal virtual IEnumerable<TElement> Contains(ReadOnlySpan<char> indexKey)
    {
        throw new NotSupportedException($"Contain queries are not supported on {GetType().Name}-indices. Use a full text to support this scenario.");
    }

}
