using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Extensions;

namespace Akade.IndexedSet.Indices;
internal class PrefixIndex<TElement>(string name) : TypedIndex<TElement, string>(name)
{
    private readonly Trie<TElement> _trie = new();

    internal override void Add(string key, TElement value)
    {
        _ = _trie.Add(key, value);
    }

    internal override void Remove(string key, TElement value)
    {
        _ = _trie.Remove(key, value);
    }

    internal override TElement Single(string indexKey)
    {
        return _trie.GetAll(indexKey).SingleThrowingKeyNotFoundException();
    }

    internal override bool TryGetSingle(string indexKey, out TElement? element)
    {
        IEnumerable<TElement> allMatches = _trie.Get(indexKey);
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
        return _trie.Get(indexKey);
    }

    internal override IEnumerable<TElement> StartsWith(ReadOnlySpan<char> indexKey)
    {
        return _trie.GetAll(indexKey);
    }

    internal override IEnumerable<TElement> FuzzyStartsWith(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        return _trie.FuzzySearch(indexKey, maxDistance, false);
    }

    public override void Clear()
    {
        _trie.Clear();
    }
}
