namespace Akade.IndexedSet.DataStructures;
internal class SuffixTrie<TElement>(IEqualityComparer<char> equalityComparer)
{
    private readonly Trie<TElement> _trie = new(equalityComparer);

    public bool Add(ReadOnlySpan<char> key, TElement element)
    {
        if (_trie.Contains(key, element))
        {
            return false;
        }

        for (int i = 0; i < key.Length; i++)
        {
            _ = _trie.Add(key[i..key.Length], element);
        }

        return true;
    }

    public bool Contains(ReadOnlySpan<char> key, TElement element)
    {
        return _trie.Contains(key, element);
    }

    public bool Remove(ReadOnlySpan<char> key, TElement element)
    {
        if (!_trie.Contains(key, element))
        {
            return false;
        }

        for (int i = 0; i < key.Length; i++)
        {
            _ = _trie.Remove(key[i..key.Length], element);
        }

        return true;
    }

    public IEnumerable<TElement> Get(ReadOnlySpan<char> key)
    {
        return _trie.Get(key);
    }


    public IEnumerable<TElement> GetAll(ReadOnlySpan<char> key)
    {
        return _trie.GetAll(key).Distinct();
    }

    internal IEnumerable<TElement> FuzzySearch(ReadOnlySpan<char> key, int maxDistance, bool exactMatches)
    {
        return _trie.FuzzySearch(key, maxDistance, exactMatches).Distinct();
    }

    internal void Clear()
    {
        _trie.Clear();
    }
}
