using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.Utils;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.DataStructures;

internal class Trie<TElement>
{
    private readonly TrieNode _root = new();

    public bool Add(ReadOnlySpan<char> key, TElement element)
    {
        return _root.Add(key, element);
    }

    public bool Remove(ReadOnlySpan<char> key, TElement element)
    {
        return _root.Remove(key, element);
    }

    public IEnumerable<TElement> Get(ReadOnlySpan<char> key)
    {
        TrieNode? matchingNode = _root.Find(key);
        return matchingNode is null || matchingNode._elements is null ? Enumerable.Empty<TElement>() : matchingNode._elements;
    }

    public bool Contains(ReadOnlySpan<char> key, TElement element)
    {
        return _root.Contains(key, element);
    }

    public IEnumerable<TElement> GetAll(ReadOnlySpan<char> prefix)
    {
        TrieNode? matchingNode = _root.Find(prefix);

        if (matchingNode is null)
        {
            return Enumerable.Empty<TElement>();
        }

        List<TElement> result = new();
        AddRecursivlyToResult(matchingNode, result);
        return result;
    }

    public IEnumerable<TElement> FuzzySearch(ReadOnlySpan<char> word, int maxDistance, bool exactMatches)
    {
        int rowLength = word.Length + 1;

        Span<int> currentRow = rowLength < LevenshteinDistance.MaxStackAlloc ? stackalloc int[rowLength] : new int[rowLength];

        for (int i = 0; i < currentRow.Length; i++)
        {
            currentRow[i] = i;
        }

        PriorityQueue<TElement, int> results = new();

        if (_root._children is not null)
        {
            foreach (KeyValuePair<char, TrieNode> child in _root._children)
            {
                FuzzySearchInternal(child.Value, child.Key, currentRow, word, results, maxDistance, exactMatches);
            }
        }

        return results.DequeueAsIEnumerable();
    }

    private void FuzzySearchInternal(TrieNode currentNode, char ch, ReadOnlySpan<int> lastRow, ReadOnlySpan<char> word, PriorityQueue<TElement, int> results, int maxDistance, bool exactMatches)
    {

        Span<int> currentRow = lastRow.Length < LevenshteinDistance.MaxStackAlloc ? stackalloc int[lastRow.Length] : new int[lastRow.Length];
        currentRow[0] = lastRow[0] + 1;

        int minDistance = currentRow[0];

        for (int i = 1; i < currentRow.Length; i++)
        {
            int insertOrDeletion = Math.Min(currentRow[i - 1] + 1, lastRow[i] + 1);
            int replacement = word[i - 1] == ch ? lastRow[i - 1] : lastRow[i - 1] + 1;
            currentRow[i] = Math.Min(insertOrDeletion, replacement);
            minDistance = Math.Min(minDistance, currentRow[i]);
        }

        bool found = false;

        if (currentRow[^1] <= maxDistance)
        {

            if (exactMatches)
            {
                if (currentNode._elements is not null)
                {
                    results.EnqueueRange(currentNode._elements, currentRow[^1]);
                }
            }
            else
            {
                AddRecursivlyToResult(currentNode, results, currentRow[^1]);
            }

            found = true;
        }

        if (!exactMatches && found)
        {
            return;
        }

        if (minDistance <= maxDistance)
        {
            if (currentNode._children is not null)
            {
                foreach (KeyValuePair<char, TrieNode> child in currentNode._children)
                {
                    FuzzySearchInternal(child.Value, child.Key, currentRow, word, results, maxDistance, exactMatches);
                }
            }
        }
    }

    private static void AddRecursivlyToResult(Trie<TElement>.TrieNode currentNode, PriorityQueue<TElement, int> results, int distance)
    {
        if (currentNode._elements is not null)
        {
            results.EnqueueRange(currentNode._elements, distance);
        }
        if (currentNode._children is not null)
        {
            foreach ((_, Trie<TElement>.TrieNode child) in currentNode._children)
            {
                AddRecursivlyToResult(child, results, distance);
            }
        }
    }

    private static void AddRecursivlyToResult(Trie<TElement>.TrieNode currentNode, List<TElement> results)
    {
        if (currentNode._elements is not null)
        {
            results.AddRange(currentNode._elements);
        }

        if (currentNode._children is not null)
        {
            foreach ((_, Trie<TElement>.TrieNode child) in currentNode._children)
            {
                AddRecursivlyToResult(child, results);
            }
        }
    }

    internal void Clear()
    {
        _root.Clear();
    }

    private class TrieNode
    {
        internal SortedDictionary<char, TrieNode>? _children;
        internal HashSet<TElement>? _elements;

        internal bool Add(ReadOnlySpan<char> key, TElement element)
        {
            if (key.IsEmpty)
            {
                _elements ??= new HashSet<TElement>();
                return _elements.Add(element);
            }
            else
            {
                _children ??= new();
                if (!_children.TryGetValue(key[0], out TrieNode? trieNode))
                {
                    _children[key[0]] = trieNode = new();
                }

                return trieNode.Add(key[1..], element);
            }
        }

        internal TrieNode? Find(ReadOnlySpan<char> key)
        {
            if (key.IsEmpty)
            {
                return this;
            }
            else if (_children?.TryGetValue(key[0], out TrieNode? trieNode) ?? false)
            {
                return trieNode.Find(key[1..]);
            }

            return null;
        }

        internal bool Remove(ReadOnlySpan<char> key, TElement element)
        {
            if (key.IsEmpty)
            {
                return _elements?.Remove(element) ?? false;
            }
            else if (_children?.TryGetValue(key[0], out TrieNode? trieNode) ?? false)
            {
                return trieNode.Remove(key[1..], element);
            }

            return false;
        }

        internal bool Contains(ReadOnlySpan<char> key, TElement element)
        {
            if (key.IsEmpty)
            {
                return _elements is not null && _elements.Contains(element);
            }
            else if (_children?.TryGetValue(key[0], out TrieNode? trieNode) ?? false)
            {
                return trieNode.Contains(key[1..], element);
            }

            return false;
        }

        internal bool TryGetChild(char key, [NotNullWhen(true)] out TrieNode? node)
        {
            node = default;
            return _children?.TryGetValue(key, out node) ?? false;
        }

        internal bool HasElements()
        {
            return _elements is not null && _elements.Count > 0;
        }

        internal void Clear()
        {
            _elements?.Clear();
            _children?.Clear();
        }
    }
}
