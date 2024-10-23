
namespace Akade.IndexedSet.Indices;

internal abstract class IndexWriter<TElement>
{
    internal abstract void Add(TElement element);

    internal abstract void AddRange(IEnumerable<TElement> elements);

    internal abstract void Remove(TElement element);
}

internal abstract class TypedIndexWriter<TElement, TIndexKey, TIndex> : IndexWriter<TElement>
    where TIndexKey : notnull
    where TIndex : TypedIndex<TElement, TIndexKey>
{

}

internal sealed class SingleKeyIndexWriter<TElement, TIndexKey, TIndex>(Func<TElement, TIndexKey> keyAccessor, TIndex index) : TypedIndexWriter<TElement, TIndexKey, TIndex>
    where TIndexKey : notnull
    where TIndex : TypedIndex<TElement, TIndexKey>
{
    private readonly Func<TElement, TIndexKey> _keyAccessor = keyAccessor;
    private readonly TIndex _index = index;

    internal override void Add(TElement element)
    {
        _index.Add(_keyAccessor(element), element);
    }

    internal override void AddRange(IEnumerable<TElement> elements)
    {
        _index.AddRange(elements.Select(elem => KeyValuePair.Create(_keyAccessor(elem), elem)));
    }

    internal override void Remove(TElement element)
    {
        _index.Remove(_keyAccessor(element), element);
    }
}

internal sealed class MultiKeyIndexWriter<TElement, TIndexKey, TIndex>(Func<TElement, IEnumerable<TIndexKey>> keyAccessor, TIndex index) : TypedIndexWriter<TElement, TIndexKey, TIndex>
    where TIndexKey : notnull
    where TIndex : TypedIndex<TElement, TIndexKey>
{
    private readonly Func<TElement, IEnumerable<TIndexKey>> _keyAccessor = keyAccessor;
    private readonly TIndex _index = index;

    internal override void Add(TElement element)
    {
        foreach (TIndexKey key in _keyAccessor(element))
        {
            _index.Add(key, element);
        }
    }

    internal override void AddRange(IEnumerable<TElement> elements)
    {
        _index.AddRange(elements.SelectMany(elem => _keyAccessor(elem).Select(key => KeyValuePair.Create(key, elem))));
    }

    internal override void Remove(TElement element)
    {
        foreach (TIndexKey key in _keyAccessor(element))
        {
            _index.Remove(key, element);
        }
    }
}
