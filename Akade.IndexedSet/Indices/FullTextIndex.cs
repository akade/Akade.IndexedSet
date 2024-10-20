using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.Utils;

namespace Akade.IndexedSet.Indices;

internal sealed class FullTextIndex<TElement> : TypedIndex<TElement, string>
{
    private readonly SuffixTrie<TElement> _suffixTrie;

    private delegate bool CandidateMatchCheck(TElement element, ReadOnlySpan<char> startsWith);
    private delegate bool FuzzyCandidateMatchCheck(TElement element, ReadOnlySpan<char> startsWith, int maxDistance);

    private readonly CandidateMatchCheck _checkCandidateMatch;
    private readonly FuzzyCandidateMatchCheck _checkFuzzyCandidateMatch;

    public FullTextIndex(Func<TElement, string> keyAccessor, string name) : base(name)
    {
        _checkCandidateMatch = (elem, startsWith) => MemoryExtensions.StartsWith(keyAccessor(elem), startsWith, StringComparison.Ordinal);
        _checkFuzzyCandidateMatch = (elem, startsWith, maxDistance) => VerifyFuzzyStartsWith(keyAccessor(elem), startsWith, maxDistance);
        _suffixTrie = new();
    }

    public FullTextIndex(Func<TElement, IEnumerable<string>> keyAccessor, string name) : base(name)
    {
        _checkCandidateMatch = (elem, startsWith) =>
        {
            foreach (string key in keyAccessor(elem))
            {
                if (MemoryExtensions.StartsWith(key, startsWith, StringComparison.Ordinal))
                    return true;
            }

            return false;
        };

        _checkFuzzyCandidateMatch = (elem, startsWith, maxDistance) =>
        {
            foreach(string key in keyAccessor(elem))
            {
                if (VerifyFuzzyStartsWith(key, startsWith, maxDistance))
                    return true;
            }

            return false;
        };
        _suffixTrie = new();
    }

    internal override void Add(string key, TElement value)
    {
        _ = _suffixTrie.Add(key, value);
    }

    internal override void Remove(string key, TElement value)
    {
        _ = _suffixTrie.Remove(key, value);
    }

    internal override TElement Single(string indexKey)
    {
        return _suffixTrie.GetAll(indexKey).SingleThrowingKeyNotFoundException();
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
        HashSet<TElement> matches = [];

        foreach (TElement candidate in _suffixTrie.GetAll(indexKey))
        {
            if (_checkCandidateMatch(candidate, indexKey))
            {
                _ = matches.Add(candidate);
            }
        }

        return matches;
    }

    private static bool VerifyFuzzyStartsWith(ReadOnlySpan<char> found,  ReadOnlySpan<char> startsWith, int maxDistance)
    {
        return LevenshteinDistance.FuzzyMatch(found[..startsWith.Length], startsWith, maxDistance);
    }

    internal override IEnumerable<TElement> FuzzyStartsWith(ReadOnlySpan<char> indexKey, int maxDistance)
    {
        HashSet<TElement> matches = [];

        foreach (TElement candidate in _suffixTrie.FuzzySearch(indexKey, maxDistance, false))
        {
            if (_checkFuzzyCandidateMatch(candidate, indexKey, maxDistance))
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
