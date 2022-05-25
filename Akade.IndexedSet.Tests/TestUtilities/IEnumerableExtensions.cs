namespace Akade.IndexedSet.Tests.TestUtilities;

internal static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source)
        {
            action(item);
        }
    }

    public static string StringJoin<T>(this IEnumerable<T> source, string? seperator)
    {
        return string.Join(seperator, source);
    }
}
