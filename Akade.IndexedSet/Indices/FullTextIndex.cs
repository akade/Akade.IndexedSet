using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Utils;

namespace Akade.IndexedSet.Indices;
internal class FullTextIndex<TElement> : TypedIndex<TElement, ReadOnlyMemory<char>>
{
    private readonly SuffixTrie<TElement> _suffixTrie;
    private readonly Func<TElement, ReadOnlyMemory<char>> _keyAccessor;

    public FullTextIndex(Func<TElement, ReadOnlyMemory<char>> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
        _suffixTrie = new();
    }

    public override void Add(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _suffixTrie.Add(key.Span, value);
    }

    public override void Remove(TElement value)
    {
        ReadOnlyMemory<char> key = _keyAccessor(value);
        _ = _suffixTrie.Remove(key.Span, value);
    }

    internal override TElement Single(ReadOnlyMemory<char> indexKey)
    {
        return _suffixTrie.GetAll(indexKey.Span).Single();
    }

    internal override bool TryGetSingle(ReadOnlyMemory<char> indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _suffixTrie.Get(indexKey.Span);
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
        return _suffixTrie.Get(indexKey.Span);
    }

    internal override IEnumerable<TElement> StartsWith(ReadOnlyMemory<char> indexKey)
    {
        return _suffixTrie.GetAll(indexKey.Span)
                           .Where(candidate => _keyAccessor(candidate).Span.StartsWith(indexKey.Span))
                           .Distinct();
    }

    internal override IEnumerable<TElement> FuzzyStartsWith(ReadOnlyMemory<char> indexKey, int maxDistance)
    {
        return _suffixTrie.FuzzySearch(indexKey.Span, maxDistance, false)
                           .Where(candidate => LevenshteinDistance.FuzzyMatch(_keyAccessor(candidate).Span[..indexKey.Length], indexKey.Span, maxDistance))
                           .Distinct();
    }

    internal override IEnumerable<TElement> FuzzyContains(ReadOnlyMemory<char> indexKey, int maxDistance)
    {
        return _suffixTrie.FuzzySearch(indexKey.Span, maxDistance, false).Distinct();
    }

    internal override IEnumerable<TElement> Contains(ReadOnlyMemory<char> indexKey)
    {
        return _suffixTrie.GetAll(indexKey.Span).Distinct();
    }
}
