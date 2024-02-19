namespace Akade.IndexedSet.InternalSourceGenerator.Tests;

[TestClass]
public class WriteTests : VerifyBase
{
    [TestMethod]
    public async Task Generates_boolean_return_wrapped_in_write_lock_including_summary()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
            public class IndexedSet<TElement>
            {
                /// <summary>
                /// Tries to remove an element from the set.
                /// </summary>
                /// <param name="element">The element to remove</param>
                /// <returns>True if an element was removed otherwise, false.</returns>
                [Akade.IndexedSet.Concurrency.WriteAccess]
                public bool Remove(TElement element)
                {
                    if (!_data.Remove(element))
                    {
                        return false;
                    }

                    foreach (Index<TElement> index in _indices.Values)
                    {
                        index.Remove(element);
                    }
                    return true;
                }
            }
            """);
    }

    [TestMethod]
    public async Task Generates_wrapped_call_without_return_in_write_lock()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
            public class IndexedSet<TElement>
            {
                /// <summary>
                /// Removes all elements
                /// </summary>
                [Akade.IndexedSet.Concurrency.WriteAccess]
                public void Clear()
                {
                    _data.Clear();
                    foreach (Index<TElement> index in _indices.Values)
                    {
                        index.Clear();
                    }
                }
            }
        """);
    }

    [TestMethod]
    public async Task Generates_boolean_return_wrapped_in_write_lock_for_primary_key_set()
    {
        await this.VerifySourceGen().ForSource<ConcurrentSetSourceGenerator>("""
         public class IndexedSet<TPrimaryKey, TElement>
         {
             [Akade.IndexedSet.Concurrency.WriteAccess]
             /// <summary>
             /// Attempts to remove an item with the given primary key and returns true, if one was found and removed. Otherwise, false.
             /// </summary>
             public bool Remove(TPrimaryKey key)
             {
                 return TryGetSingle(_primaryKeyAccessor, key, out TElement? result, _primaryKeyIndexName) && Remove(result);
             }
         }
        """);
    }
}