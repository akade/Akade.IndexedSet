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
    public async Task Generates_net_version_compiler_directives()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
        #define NET9_0_OR_GREATER
        public class IndexedSet<TElement>
        {
        #if NET9_0_OR_GREATER
            /// <summary>
            /// Returns the approximate k nearest neighbors of the given value.
            /// </summary>
            [Akade.IndexedSet.Concurrency.ReadAccess]
            public IEnumerable<TElement> ApproximateNearestNeighbors(Func<TElement, ReadOnlySpan<float>> indexAccessor, ReadOnlySpan<float> value, int k, [CallerArgumentExpression(nameof(indexAccessor))] string? indexName = null)
            {
                TypedIndex<TElement, ReadOnlySpan<float>> typedIndex = GetIndex<ReadOnlySpan<float>>(indexName);
                return typedIndex.ApproximateNearestNeighbors(value, k);
            }
        #endif
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

    [TestMethod]
    public async Task Generates_experimental_attribute()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
     public class IndexedSet<TElement>
     {
         // <summary>
        /// Returns all elements that contain the given char sequence or a simalar one.
        /// </summary>
        /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
        /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
        /// <param name="infix">The infix to use</param>
        /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
        /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
        [Akade.IndexedSet.Concurrency.ReadAccess]
        [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
        public IEnumerable<TElement> FuzzyContains(Func<TElement, IEnumerator<string>> indexAccessor, ReadOnlySpan<char> infix, int maxDistance, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
        {
            TypedIndex<TElement, string> typedIndex = GetIndex<string>(indexName);
            return typedIndex.FuzzyContains(infix, maxDistance);
        }
     }
    """);
    }
}