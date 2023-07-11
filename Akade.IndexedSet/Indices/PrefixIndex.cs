using Akade.IndexedSet.DataStructures;
using Akade.IndexedSet.Extensions;

namespace Akade.IndexedSet.Indices;
internal class PrefixIndex<TElement> : TypedIndex<TElement, string>
{
    private readonly Trie<TElement> _trie;
    private readonly Func<TElement, string> _keyAccessor;

    public PrefixIndex(Func<TElement, string> keyAccessor, string name) : base(name)
    {
        _keyAccessor = keyAccessor;
        _trie = new();
    }

    public override void Add(TElement value)
    {
        string key = _keyAccessor(value);
        _ = _trie.Add(key, value);
    }

    public override void Remove(TElement value)
    {
        string key = _keyAccessor(value);
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
