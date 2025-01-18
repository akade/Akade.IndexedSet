namespace Akade.IndexedSet.DataStructures;
internal class SuffixTrie<TElement>
{
    private readonly Trie<TElement> _trie = new();
    private readonly UkkonenTree _ukkonenTree = new();

    public bool Add(ReadOnlySpan<char> key, TElement element)
    {
        if (_trie.Contains(key, element))
        {
            return false;
        }

        _ukkonenTree.Clear();
        _ukkonenTree.Add(key);

        MergeIntoTree(_ukkonenTree._root, _trie._root, element);

        return true;
    }

    private void MergeIntoTree(UkkonenTree.Node sourceNode, Trie<TElement>.TrieNode targetNode, TElement element)
    {
        Trie<TElement>.TrieNode trieNode = GetActualNode(targetNode, sourceNode.Start, sourceNode.End);
        
        if (sourceNode.IsLeaf)
        {
            trieNode.AddElement(element);
        }
        else
        {
            foreach ((_, UkkonenTree.Node value) in sourceNode.Children)
            {
                MergeIntoTree(value, trieNode, element);
            }
        }
    }

    private Trie<TElement>.TrieNode GetActualNode(Trie<TElement>.TrieNode targetNode, int start, int end)
    {
        end = end == UkkonenTree.BOUNDLESS ? _ukkonenTree._chars.Count : end;
        Trie<TElement>.TrieNode node = targetNode;

        for (int i = start; i < end; i++)
        {
            node = node.GetOrAddChildNode(_ukkonenTree._chars[i]);
        }

        return node;
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
