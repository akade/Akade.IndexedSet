using Akade.IndexedSet.DataStructures;

namespace Akade.IndexedSet.Indices;
internal class TextIndex<TElement> : TypedIndex<TElement, ReadOnlyMemory<char>>
    where TElement : class
{
    private readonly SuffixxTrie<TElement> _suffixxTrie;
    private readonly Func<TElement, ReadOnlyMemory<char>> _keyAccessor;

    public TextIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
        _suffixxTrie = new();
    }


    public override void Add(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _suffixxTrie.Add(key.Span, value);
    }

    public override void Remove(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _suffixxTrie.Remove(key.Span, value);
    }

    internal override TElement Single(ReadOnlyMemory<char> indexKey)
    {
        return _suffixxTrie.GetAll(indexKey).Single();
    }

    internal override bool TryGetSingle(ReadOnlyMemory<char> indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _suffixxTrie.Get(indexKey.Span);
        element = default;

        IEnumerator<TElement> enumerator = allMatches.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return false;
        }

        element = enumerator.Current;

        return !enumerator.MoveNext();
    }

    internal override IEnumerable<TElement> Where(ReadOnlyMemory<char> indexKey)
    {
        return _suffixxTrie.Get(indexKey.Span);
    }
}
