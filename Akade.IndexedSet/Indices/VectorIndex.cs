#if NET9_0_OR_GREATER

using Akade.IndexedSet.DataStructures.FreshVamana;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.Indices;

internal class VectorIndex<TElement>(Func<TElement, ReadOnlySpan<float>> keyAccessor, string name) : TypedIndex<TElement, ReadOnlySpan<float>>(name)
    where TElement : notnull
{
    private readonly FreshVamanaGraph<TElement> _graph = new(keyAccessor, FreshVamanaSettings.Default);

    public override void Clear()
    {
        _graph.Clear();
    }

    internal override void Add(ReadOnlySpan<float> key, TElement value)
    {
        _graph.Add(value);
    }

    internal override void Remove(ReadOnlySpan<float> key, TElement value)
    {
        _graph.Delete(value);
    }

    internal override TElement Single(ReadOnlySpan<float> indexKey)
    {
        ThrowNotSupportedExceptionDueToANN();
        return default;
    }

    [DoesNotReturn]
    private static void ThrowNotSupportedExceptionDueToANN()
    {
        throw new NotSupportedException("VectorIndex is uses approximate nearest neighbor search and hence, does not support finding item(s) with a specific key");
    }

    internal override bool TryGetSingle(ReadOnlySpan<float> indexKey, out TElement? element)
    {
        element = default;
        ThrowNotSupportedExceptionDueToANN();
        return default;
    }

    internal override IEnumerable<TElement> Where(ReadOnlySpan<float> indexKey)
    {
        ThrowNotSupportedExceptionDueToANN();
        return default;
    }

    internal override IEnumerable<TElement> ApproximateNearestNeighbors(ReadOnlySpan<float> indexKey, int k)
    {
        return _graph.NeareastNeighbors(indexKey, k);
    }
}

#endif
