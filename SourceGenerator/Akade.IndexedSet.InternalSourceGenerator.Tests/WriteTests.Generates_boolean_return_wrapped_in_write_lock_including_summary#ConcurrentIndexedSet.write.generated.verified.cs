//HintName: ConcurrentIndexedSet.write.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TElement>
{
    /// <summary>
    /// Tries to remove an element from the set.
    /// </summary>
    /// <param name="element">The element to remove</param>
    /// <returns>True if an element was removed otherwise, false.</returns>
    public bool Remove(TElement element)
    {
        using (AcquireWriterLock())
        {
            return _indexedSet.Remove(element);
        }
    }
}
#nullable restore
