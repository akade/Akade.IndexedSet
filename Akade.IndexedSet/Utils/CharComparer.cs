using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Akade.IndexedSet.StringUtilities;


/// <summary>
/// Provides a set of standard comparer for <see cref="char"/> values.
/// </summary>
public static class CharEqualityComparer
{
    /// <summary>
    /// Gets a <see cref="IEqualityComparer{T}"/> that performs an ordinal equality comparison of <see cref="char"/> values using <see cref="char.ToUpperInvariant(char)"/>.
    /// </summary>
    public static IEqualityComparer<char> OrdinalIgnoreCase { get; } = new OrdinalIgnoreCaseCharEqualityComparer();

    /// <summary>
    /// Gets a <see cref="IEqualityComparer{T}"/> that performs an ordinal equality comparison of <see cref="char"/> values using <see cref="char.ToUpper(char, CultureInfo)"/>.
    /// </summary>
    /// <param name="cultureInfo">The culture used for the case transformation</param>
    public static IEqualityComparer<char> IgnoreCase(CultureInfo cultureInfo) => new IgnoreCaseCharEqualityComparer(cultureInfo);
}

internal class IgnoreCaseCharEqualityComparer(CultureInfo cultureInfo) : IEqualityComparer<char>
{
    public bool Equals(char x, char y)
    {
        return char.ToUpper(x, cultureInfo) == char.ToUpper(y, cultureInfo);
    }

    public int GetHashCode([DisallowNull] char obj)
    {
        return char.ToUpper(obj, cultureInfo).GetHashCode();
    }
}

internal class OrdinalIgnoreCaseCharEqualityComparer : IEqualityComparer<char>
{
    public bool Equals(char x, char y)
    {
        return char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
    }

    public int GetHashCode([DisallowNull] char obj)
    {
        return char.ToUpperInvariant(obj).GetHashCode();
    }
}
