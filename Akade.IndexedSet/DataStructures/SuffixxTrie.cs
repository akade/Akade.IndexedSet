namespace Akade.IndexedSet.DataStructures;
internal class SuffixxTrie<TElement>
{
    private readonly Trie<TElement> _trie = new();

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


    public IEnumerable<TElement> GetAll(string key)
    {
        return GetAll(key.AsMemory());
    }

    public IEnumerable<TElement> GetAll(ReadOnlyMemory<char> key)
    {
        HashSet<TElement> set = new();
        foreach (TElement element in _trie.GetAll(key))
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }

    internal IEnumerable<TElement> FuzzySearch(ReadOnlyMemory<char> key, int maxDistance, bool exactMatches)
    {
        HashSet<TElement> set = new();
        foreach (TElement element in _trie.FuzzySearch(key.Span, maxDistance, exactMatches))
        {
            if (set.Add(element))
            {
                yield return element;
            }
        }
    }
}
