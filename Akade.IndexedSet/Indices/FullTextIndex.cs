using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Utils;

namespace Akade.IndexedSet.Indices;
internal class FullTextIndex<TElement> : TypedIndex<TElement, string>
{
    private readonly SuffixTrie<TElement> _suffixTrie;
    private readonly Func<TElement, string> _keyAccessor;

    public FullTextIndex(Func<TElement, string> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
        _suffixTrie = new();
    }

    public override void Add(TElement value)
    {
        string key = _keyAccessor(value);
        _ = _suffixTrie.Add(key, value);
    }

    public override void Remove(TElement value)
    {
        string key = _keyAccessor(value);
        _ = _suffixTrie.Remove(key, value);
    }

    internal override TElement Single(string indexKey)
    {
        return _suffixTrie.GetAll(indexKey).Single();
    }

    internal override bool TryGetSingle(string indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _suffixTrie.Get(indexKey);
        element = default;

        IEnumerator<TElement> enumerator = allMatches.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return false;
        }

        element = enumerator.Current;

        return !enumerator.MoveNext();
    }

    internal override IEnumerable<TElement> Where(string indexKey)
    {
        return _suffixTrie.Get(indexKey);
    }

    internal override IEnumerable<TElement> StartsWith(ReadOnlySpan<char> indexKey)
    {
        HashSet<TElement> matches = new();

        foreach (TElement candidate in _suffixTrie.GetAll(indexKey))
        {
            if (MemoryExtensions.StartsWith(_keyAccessor(candidate), indexKey, StringComparison.Ordinal))
            {
                _ = matches.Add(candidate);
            }
        }

        return matches;
    }

    internal override IEnumerable<TElement> FuzzyStartsWith(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        HashSet<TElement> matches = new();

        foreach (TElement candidate in _suffixTrie.FuzzySearch(indexKey, maxDistance, false))
        {
            if (LevenshteinDistance.FuzzyMatch(_keyAccessor(candidate).AsSpan()[..indexKey.Length], indexKey, maxDistance))
            {
                _ = matches.Add(candidate);
            }
        }

        return matches;
    }

    internal override IEnumerable<TElement> FuzzyContains(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        return _suffixTrie.FuzzySearch(indexKey, maxDistance, false);
    }

    internal override IEnumerable<TElement> Contains(ReadOnlySpan<char> indexKey)
    {
        return _suffixTrie.GetAll(indexKey);
    }

    public override void Clear()
    {
        _suffixTrie.Clear();
    }
}
