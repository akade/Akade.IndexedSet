//HintName: ConcurrentIndexedSet.read.generated.cs
using Akade.IndexedSet.Concurrency;
namespace Akade.IndexedSet.Concurrency;

public partial class ConcurrentIndexedSet<TElement>
{
    /// <summary>
    /// Searches for an element via an index.
    /// </summary>
    /// <typeparam name="TIndexKey">The type of the index key</typeparam>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="indexKey">The key within the index.</param>
    /// <param name="element">The element if found, otherwise null.</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>
    public bool TryGetSingle(
        Func<TElement, TIndexKey> indexAccessor,
        TIndexKey indexKey,
        [NotNullWhen(true)] out TElement? element,
        [CallerArgumentExpression("indexAccessor")] string? indexName = null) 
    {
        using(AcquireReaderLock())
        {
            return _indexedSet.TryGetSingle(indexAccessor, indexKey, element, indexName);
        }
    }
}
