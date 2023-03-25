namespace Akade.IndexedSet.InternalSourceGenerator.Tests;

[TestClass]
public class ReadTests : VerifyBase
{
    [TestMethod]
    public async Task Generates_boolean_return_wrapped_in_read_lock_including_summary()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
            public class IndexedSet<TElement>
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
                [Akade.IndexedSet.Concurrency.ReadAccess]
                public bool TryGetSingle<TIndexKey>(
                    Func<TElement, TIndexKey> indexAccessor,
                    TIndexKey indexKey,
                    [NotNullWhen(true)] out TElement? element,
                    [CallerArgumentExpression("indexAccessor")] string? indexName = null)
                    where TIndexKey : notnull
                {
                    TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
                    return typedIndex.TryGetSingle(indexKey, out element);
                }
            }
            """);
    }

    [TestMethod]
    public async Task Generates_boolean_return_wrapped_in_read_lock_including_materialization()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
            public class IndexedSet<TElement>
            {
                [Akade.IndexedSet.Concurrency.ReadAccess]
                public IEnumerable<TElement> Where<TIndexKey>(
                    Func<TElement, TIndexKey> indexAccessor,
                    TIndexKey indexKey,
                    [CallerArgumentExpression("indexAccessor")] string? indexName = null)
                    where TIndexKey : notnull
                {
                    TypedIndex<TElement, TIndexKey> typedIndex = GetIndex<TIndexKey>(indexName);
                    return typedIndex.Where(indexKey);
                }
            }
        """);
    }

    [TestMethod]
    public async Task Generates_single_object_wrapped_in_read_lock_for_primary_key_set()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
         public class IndexedSet<TPrimaryKey, TElement>
         {
             [Akade.IndexedSet.Concurrency.ReadAccess]
             public TElement Single(TPrimaryKey key)
             {
                 return Single(_primaryKeyAccessor, key, _primaryKeyIndexName);
             }
         }
     """);
    }
}