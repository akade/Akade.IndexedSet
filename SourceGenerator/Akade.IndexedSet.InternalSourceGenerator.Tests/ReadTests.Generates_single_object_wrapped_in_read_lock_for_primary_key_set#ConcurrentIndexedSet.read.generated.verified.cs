//HintName: ConcurrentIndexedSet.read.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TPrimaryKey, TElement>
{
    public TElement Single(TPrimaryKey key)
    {
        using (AcquireReaderLock())
        {
            return _primaryKeyIndexedSet.Single(key);
        }
    }
}
#nullable restore
