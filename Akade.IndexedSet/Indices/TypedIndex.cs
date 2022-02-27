namespace Akade.IndexedSet.Indices;

/// <summary>
/// Fully-generic index including on the index key
/// </summary>
internal abstract class TypedIndex<TPrimaryKey, TElement, TIndexKey> : Index<TPrimaryKey, TElement>
    where TPrimaryKey : notnull
    where TIndexKey : notnull
{
    protected TypedIndex(string name) : base(name)
    {

    }

    internal abstract TElement Single(TIndexKey indexKey);

    internal abstract IEnumerable<TElement> Where(TIndexKey indexKey);

    internal virtual IEnumerable<TElement> Range(TIndexKey start, TIndexKey end, bool inclusiveStart, bool inclusiveEnd)
    {
        throw new NotSupportedException($"Range queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> LessThan(TIndexKey value)
    {
        throw new NotSupportedException($"LessThan queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> LessThanOrEqual(TIndexKey value){
        throw new NotSupportedException($"LessThanOrEquals queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> GreaterThan(TIndexKey value){
        throw new NotSupportedException($"GreaterThan queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }

    internal virtual IEnumerable<TElement> GreaterThanOrEqual(TIndexKey value){
        throw new NotSupportedException($"GreaterThanOrEqual queries are not supported on {GetType().Name}-indices. Use a range index to support this scenario.");
    }
}
