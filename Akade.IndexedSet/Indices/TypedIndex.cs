namespace Akade.IndexedSet.Indices;

/// <summary>
/// Fully-generic index including on the index key
/// </summary>
internal abstract class TypedIndex<TPrimaryKey, TElement, TIndexKey> : Index<TPrimaryKey, TElement>
    where TPrimaryKey : notnull
    where TIndexKey : notnull
{
    protected readonly Func<TElement, TIndexKey> _keyAccessor;

    protected TypedIndex(Func<TElement, TIndexKey> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
    }

    internal abstract TElement Single(TIndexKey indexKey);

    internal abstract IEnumerable<TElement> Where(TIndexKey indexKey);

    internal abstract IEnumerable<TElement> Range(TIndexKey inclusiveStart, TIndexKey exclusiveStart);
}
