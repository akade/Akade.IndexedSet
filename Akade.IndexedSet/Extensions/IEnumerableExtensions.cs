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
}
