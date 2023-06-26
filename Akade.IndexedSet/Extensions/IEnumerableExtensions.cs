namespace Akade.IndexedSet.Extensions;
internal static class IEnumerableExtensions
{
    public static IEnumerable<TElement> DequeueAsIEnumerable<TElement, TPriority>(this PriorityQueue<TElement, TPriority> queue)
    {
        while (queue.TryDequeue(out TElement? element, out _))
        {
            yield return element;
        }
    }

    public static TElement SingleThrowingKeyNotFoundException<TElement>(this IEnumerable<TElement> source)
    {
        using IEnumerator<TElement> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            throw new KeyNotFoundException();
        }

        TElement result = enumerator.Current;

        if (enumerator.MoveNext())
        {
            throw new InvalidOperationException();
        }

        return result;
    }
}
