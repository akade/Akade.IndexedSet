//HintName: ConcurrentIndexedSet.write.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TPrimaryKey, TElement>
{
    public bool Remove(TPrimaryKey key)
    {
        using (AcquireWriterLock())
        {
            return _indexedSet.Remove(key);
        }
    }
}
#nullable restore
