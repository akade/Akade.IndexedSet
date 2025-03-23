namespace Akade.IndexedSet.Extensions;

internal static class SpanExtensions
{
    internal static bool StartsWith<TElement, TComparer>(this ReadOnlySpan<TElement> span, ReadOnlySpan<TElement> contains, TComparer comparer)
        where TComparer : IEqualityComparer<TElement>
    {
        if (span.Length < contains.Length)
        {
            return false;
        }
        for (int i = 0; i < contains.Length; i++)
        {
            if (!comparer.Equals(span[i], contains[i]))
            {
                return false;
            }
        }
        return true;
    }
}
