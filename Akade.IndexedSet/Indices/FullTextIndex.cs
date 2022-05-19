using Akade.IndexedSet.DataStructures;

namespace Akade.IndexedSet.Indices;
internal class FullTextIndex<TElement> : TypedIndex<TElement, ReadOnlyMemory<char>>
{
    private readonly SuffixxTrie<TElement> _suffixxTrie;
    private readonly Func<TElement, ReadOnlyMemory<char>> _keyAccessor;

    public FullTextIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, string name) : base(name)
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
        return _suffixxTrie.GetAll(indexKey.Span).Single();
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

    internal override IEnumerable<TElement> StartsWith(ReadOnlyMemory<char> indexKey)
    {
        return _suffixxTrie.GetAll(indexKey.Span)
                           .Where(candidate => _keyAccessor(candidate).Span.StartsWith(indexKey.Span))
                           .Distinct();
    }

    internal override IEnumerable<TElement> FuzzyMatch(ReadOnlyMemory<char> indexKey, int maxDistance)
    {
        return _suffixxTrie.FuzzySearch(indexKey.Span, maxDistance, false).Distinct();
    }
}
