﻿using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Extensions;
using Akade.IndexedSet.StringUtilities;
using Akade.IndexedSet.Utils;

namespace Akade.IndexedSet.Indices;

internal sealed class FullTextIndex<TElement> : TypedIndex<TElement, string>
{
    private readonly SuffixTrie<TElement> _suffixTrie;

    private delegate bool StartsWithCheck(ReadOnlySpan<char> key, ReadOnlySpan<char> startsWith);
    private delegate bool CandidateMatchCheck(TElement element, ReadOnlySpan<char> startsWith);
    private delegate bool FuzzyCandidateMatchCheck(TElement element, ReadOnlySpan<char> startsWith, int maxDistance);

    private readonly StartsWithCheck _startsWithCheck;
    private readonly CandidateMatchCheck _checkCandidateMatch;
    private readonly FuzzyCandidateMatchCheck _checkFuzzyCandidateMatch;
    private readonly IEqualityComparer<char> _equalityComparer;

    public FullTextIndex(Func<TElement, string> keyAccessor, IEqualityComparer<char> equalityComparer, string name) : base(name)
    {
        _startsWithCheck = GetStartsWithForComparer(equalityComparer);
        _checkCandidateMatch = (elem, startsWith) => _startsWithCheck(keyAccessor(elem), startsWith);
        _checkFuzzyCandidateMatch = (elem, startsWith, maxDistance) => VerifyFuzzyStartsWith(keyAccessor(elem), startsWith, maxDistance);
        _suffixTrie = new(equalityComparer);
        _equalityComparer = equalityComparer;
    }

    private static StartsWithCheck GetStartsWithForComparer(IEqualityComparer<char> equalityComparer)
    {
        return equalityComparer switch
        {
            IgnoreCaseCharEqualityComparer => (elem, startsWith) => elem.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase),
            { } x when x == EqualityComparer<char>.Default => (elem, startsWith) => elem.StartsWith(startsWith, StringComparison.Ordinal),
            _ => (elem, startsWith) => elem.StartsWith(startsWith, equalityComparer)
        };
    }

    public FullTextIndex(Func<TElement, IEnumerable<string>> keyAccessor, IEqualityComparer<char> equalityComparer, string name) : base(name)
    {
        _startsWithCheck = GetStartsWithForComparer(equalityComparer);

        _checkCandidateMatch = (elem, startsWith) =>
        {
            foreach (string key in keyAccessor(elem))
            {
                if (_startsWithCheck(key, startsWith))
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
        _suffixTrie = new(equalityComparer);
        _equalityComparer = equalityComparer;
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

    private bool VerifyFuzzyStartsWith(ReadOnlySpan<char> found,  ReadOnlySpan<char> startsWith, int maxDistance)
    {
        return LevenshteinDistance.FuzzyMatch(found[..startsWith.Length], startsWith, maxDistance, _equalityComparer);
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
