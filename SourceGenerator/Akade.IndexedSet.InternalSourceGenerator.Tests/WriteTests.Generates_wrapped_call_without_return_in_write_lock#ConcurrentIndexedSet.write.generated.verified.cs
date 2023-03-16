//HintName: ConcurrentIndexedSet.write.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TElement>
{
    /// <summary>
        /// Removes all elements
        /// </summary>
        public void Clear()
    {
        using (AcquireWriterLock())
        {
            _indexedSet.Clear();
        }
    }
}
#nullable restore
