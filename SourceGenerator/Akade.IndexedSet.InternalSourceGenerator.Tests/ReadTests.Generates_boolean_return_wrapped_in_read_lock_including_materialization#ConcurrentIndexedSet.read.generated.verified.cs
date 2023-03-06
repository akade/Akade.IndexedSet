//HintName: ConcurrentIndexedSet.read.generated.cs
using Akade.IndexedSet.Concurrency;
namespace Akade.IndexedSet.Concurrency;

public partial class ConcurrentIndexedSet<TElement>
{
    public IEnumerable<TElement> Where(
            Func<TElement, TIndexKey> indexAccessor,
            TIndexKey indexKey,
            [CallerArgumentExpression("indexAccessor")] string? indexName = null) 
    {
        using(AcquireReaderLock())
        {
            return _indexedSet.Where(indexAccessor, indexKey, indexName).ToArray();
        }
    }
}
