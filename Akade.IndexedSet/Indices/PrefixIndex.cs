using Akade.IndexedSet.DataStructures;

namespace Akade.IndexedSet.Indices;
internal class PrefixIndex<TElement> : TypedIndex<TElement, ReadOnlyMemory<char>>
{
    private readonly Trie<TElement> _trie;
    private readonly Func<TElement, ReadOnlyMemory<char>> _keyAccessor;

    public PrefixIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
        _trie = new();
    }

    public override void Add(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _trie.Add(key.Span, value);
    }

    public override void Remove(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _trie.Remove(key.Span, value);
    }

    internal override TElement Single(ReadOnlyMemory<char> indexKey)
    {
        return _trie.GetAll(indexKey.Span).Single();
    }

    internal override bool TryGetSingle(ReadOnlyMemory<char> indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _trie.Get(indexKey.Span);
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
        return _trie.Get(indexKey.Span);
    }

    internal override IEnumerable<TElement> StartsWith(ReadOnlyMemory<char> indexKey)
    {
        return _trie.GetAll(indexKey.Span);
    }

    internal override IEnumerable<TElement> FuzzyStartsWith(ReadOnlyMemory<char> indexKey, int maxDistance)
    {
        return _trie.FuzzySearch(indexKey.Span, maxDistance, false);
    }

    public override void Clear()
    {
        _trie.Clear();
    }
}
