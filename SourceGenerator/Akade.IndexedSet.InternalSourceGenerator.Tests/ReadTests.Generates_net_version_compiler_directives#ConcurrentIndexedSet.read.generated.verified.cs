//HintName: ConcurrentIndexedSet.read.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TElement>
{
    #if NET9_0_OR_GREATER
    /// <summary>
    /// Returns the approximate k nearest neighbors of the given value.
    /// </summary>
    public IEnumerable<TElement> ApproximateNearestNeighbors(Func<TElement, ReadOnlySpan<float>> indexAccessor, ReadOnlySpan<float> value, int k, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.ApproximateNearestNeighbors(indexAccessor, value, k, indexName).ToArray();
        }
    }
    #endif
}
#nullable restore
