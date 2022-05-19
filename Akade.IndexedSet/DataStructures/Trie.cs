using Akade.IndexedSet.Extensions;
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
        return matchingNode is null ? Enumerable.Empty<TElement>() : matchingNode.GetLocalElements();
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

        IEnumerable<TElement> result = matchingNode.GetLocalElements();

        foreach (TrieNode node in matchingNode.GetAllChildren())
        {
            result = result.Concat(node.GetLocalElements());
        }

        return result;
    }


    public IEnumerable<TElement> FuzzySearch(ReadOnlySpan<char> word, int maxDistance, bool exactMatches)
    {
        int wordLength = word.Length;

        int[] currentRow = new int[wordLength + 1];

        for (int i = 0; i < currentRow.Length; i++)
        {
            currentRow[i] = i;
        }

        PriorityQueue<TrieNode, int> results = new();


        for (int i = 0; i < wordLength; i++)
        {
            if (_root.TryGetChild(word[i], out TrieNode? startNode))
            {
                FuzzySearchInternal(startNode, word[i], currentRow, word, results, maxDistance, exactMatches);
            }
        }

        return exactMatches
            ? results.DequeueAsIEnumerable().SelectMany(node => node.GetLocalElements())
            : results.DequeueAsIEnumerable().SelectMany(node => node.GetAllChildren().SelectMany(n => n.GetLocalElements()));
    }

    private void FuzzySearchInternal(TrieNode currentNode, char ch, int[] lastRow, ReadOnlySpan<char> word, PriorityQueue<TrieNode, int> results, int maxDistance, bool exploreSubTrees)
    {
        int[] currentRow = new int[lastRow.Length];
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
            results.Enqueue(currentNode, currentRow[^1]);
            found = true;
        }

        if (!exploreSubTrees && found)
        {
            return;
        }

        if (minDistance <= maxDistance)
        {
            foreach (KeyValuePair<char, TrieNode> child in currentNode.GetLocalChildren())
            {
                FuzzySearchInternal(child.Value, child.Key, currentRow, word, results, maxDistance, exploreSubTrees);
            }
        }
    }

    private class TrieNode
    {
        private SortedDictionary<char, TrieNode>? _children;
        private HashSet<TElement>? _elements;

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

        internal IEnumerable<TrieNode> GetAllChildren()
        {
            if (_children is null)
            {
                yield break;
            }

            foreach (TrieNode node in _children.Values)
            {
                yield return node;

                foreach (TrieNode child in node.GetAllChildren())
                {
                    yield return child;
                }
            }
        }

        internal IEnumerable<KeyValuePair<char, TrieNode>> GetLocalChildren()
        {
            return _children ?? Enumerable.Empty<KeyValuePair<char, TrieNode>>();
        }


        internal IEnumerable<TElement> GetLocalElements()
        {
            return _elements ?? Enumerable.Empty<TElement>();
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
            if (key.IsEmpty && _elements is not null)
            {
                return _elements.Contains(element);
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
    }
}
