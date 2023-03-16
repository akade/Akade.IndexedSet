//HintName: ConcurrentIndexedSet.read.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TElement>
{
    public IEnumerable<TElement> Where<TIndexKey>(
            Func<TElement, TIndexKey> indexAccessor,
            TIndexKey indexKey,
            [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        where TIndexKey : notnull
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.Where(indexAccessor, indexKey, indexName).ToArray();
        }
    }
}
#nullable restore
