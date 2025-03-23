namespace Akade.IndexedSet.Tests.TestUtilities;

/// <summary>
/// Helper utilities for converting comparers for the generic base test classes
/// </summary>
public static class ComparerUtils
{
    public static IEqualityComparer<T> GetEqualityComparer<T>(object comparer)
        where T : notnull
    {
        comparer = ConvertCharToStringComparer<T>(comparer);
        if (comparer is IEqualityComparer<T> equalityComparer)
        {
            return equalityComparer;
        }
        else if (comparer is IComparer<T> comp)
        {
            return EqualityComparer<T>.Create((a, b) => comp.Compare(a, b) == 0, a => throw new NotSupportedException("Cannot use GetHashCode() for a wrapped IComparer"));
        }
        else
        {
            throw new ArgumentException($"Invalid comparer type {comparer}", nameof(comparer));
        }
    }

    public static IComparer<T> GetComparer<T>(object comparer)
        where T : notnull
    {
        comparer = ConvertCharToStringComparer<T>(comparer);
        if (comparer is IComparer<T> comp)
        {
            return comp;
        }
        else
        {
            throw new ArgumentException($"Invalid comparer type {comparer}", nameof(comparer));
        }
    }

    /// <summary>
    /// Prefix and fulltext indices use a char comparer for their internal data structures => this method creates
    /// a string comparer from a char comparer (that compares char by char) if the target type is string.
    /// </summary>
    public static object ConvertCharToStringComparer<TTarget>(object comparer)
    {
        if (typeof(TTarget) != typeof(string))
        {
            return comparer;
        }

        if (comparer is IEqualityComparer<char> charComparer)
        {
            return new StringEqualityComparerByChar(charComparer);
        }
        else if (comparer is IComparer<char> charComp)
        {
            return new StringComparerByChar(charComp);
        }
        else
        {
            throw new ArgumentException($"Invalid comparer type {comparer}", nameof(comparer));
        }
    }

    private class StringComparerByChar(IComparer<char> charComparer) : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x is null || y is null)
            {
                return x == y ? 0 : x is null ? -1 : 1;
            }
            int i, j;
            for (i = 0, j = 0; i < x.Length && j < y.Length; i++, j++)
            {
                if (charComparer.Compare(x[i], y[j]) != 0)
                {
                    return charComparer.Compare(x[i], y[j]);
                }
            }

            if (i == j)
            {
                return 0;
            }
            else
            {
                return i < j ? -1 : 1;
            }
        }
    }

    private class StringEqualityComparerByChar(IEqualityComparer<char> charComparer) : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            if (x is null || y is null)
            {
                return x == y;
            }
            int i, j;
            for (i = 0, j = 0; i < x.Length && j < y.Length; i++, j++)
            {
                if (!charComparer.Equals(x[i], y[j]))
                {
                    return false;
                }
            }
            return i == j;
        }
        public int GetHashCode(string obj)
        {
            int hash = 0;
            foreach (char c in obj)
            {
                hash = HashCode.Combine(hash, charComparer.GetHashCode(c));
            }
            return hash;
        }
    }
}
