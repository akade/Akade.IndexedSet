//HintName: ConcurrentIndexedSet.read.generated.cs
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.Concurrency;
#nullable enable
public partial class ConcurrentIndexedSet<TElement>
{
    // <summary>
    /// Returns all elements that contain the given char sequence or a simalar one.
    /// </summary>
    /// <param name="indexAccessor">Accessor for the indexed property. The expression as a string is used as an identifier for the index. Hence, the convention is to always use x as an identifier. 
    /// Is passed to <paramref name="indexName"/> using <see cref="CallerArgumentExpressionAttribute"/>.</param>
    /// <param name="infix">The infix to use</param>
    /// <param name="maxDistance">The maximum distance (e.g. Levenshtein) between the input infix and matches</param>
    /// <param name="indexName">The name of the index. Usually, you should not specify this as the expression in <paramref name="indexAccessor"/> is automatically passed by the compiler.</param>   
    [Experimental(Experiments.TextSearchImprovements, UrlFormat = Experiments.UrlTemplate)]
    public IEnumerable<TElement> FuzzyContains(Func<TElement, IEnumerator<string>> indexAccessor, ReadOnlySpan<char> infix, int maxDistance, [CallerArgumentExpression("indexAccessor")] string? indexName = null)
    {
        using (AcquireReaderLock())
        {
            return _indexedSet.FuzzyContains(indexAccessor, infix, maxDistance, indexName).ToArray();
        }
    }
}
#nullable restore
