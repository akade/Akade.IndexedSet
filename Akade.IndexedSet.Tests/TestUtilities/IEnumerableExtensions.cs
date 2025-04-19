using System.Collections;

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

    /// <summary>
    /// Same as <see cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})"/> but
    /// uses sorting and comparison instead of hashing if an IComparer is provided.
    /// </summary>
    public static IEnumerable<IGrouping<TKey, TElement>> GroupByWithSortBasedFallback<TKey, TElement>(this IEnumerable<TElement> source, Func<TElement, TKey> keySelector, object comparer)
        where TKey : notnull
    {
        comparer = ComparerUtils.ConvertCharToStringComparer<TKey>(comparer);
        if (comparer is IEqualityComparer<TKey> equalityComparer)
        {
            return source.GroupBy(keySelector, equalityComparer);
        }
        else if (comparer is IComparer<TKey> comp)
        {
            return GroupBySort(source, keySelector, comp);
        }

        throw new ArgumentException($"Invalid comparer type {comparer}", nameof(comparer));
    }

    private static IEnumerable<IGrouping<TKey, TElement>> GroupBySort<TKey, TElement>(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comp) 
        where TKey : notnull
    {
        Grouping<TKey, TElement>? currentGroup = null;
        foreach (TElement elemennt in source.OrderBy(keySelector, comp))
        {
            TKey key = keySelector(elemennt);

            if (currentGroup is null || comp.Compare(currentGroup.Key, key) != 0)
            {
                if (currentGroup is not null)
                {
                    yield return currentGroup;
                }
                currentGroup = new Grouping<TKey, TElement>(key, elemennt);
            }
            else
            {
                currentGroup.Add(elemennt);
            }

        }

        if (currentGroup is not null)
        {
            yield return currentGroup;
        }
    }

    private class Grouping<TKey, TElement>(TKey key, TElement firstElement) : IGrouping<TKey, TElement>
    {
        private readonly List<TElement> _elements = new([firstElement]);

        public void Add(TElement element)
        {
            _elements.Add(element);
        }

        public TKey Key { get; } = key;
        public IEnumerable<TElement> Elements => _elements;

        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
