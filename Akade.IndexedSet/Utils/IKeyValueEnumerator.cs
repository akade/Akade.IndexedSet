namespace Akade.IndexedSet.Utils;

internal interface IKeyValueEnumerator<out TKey, out TValue>
#if NET9_0_OR_GREATER
    where TKey : notnull, allows ref struct
#else
    where TKey : notnull
#endif
{
    bool MoveNext();
    TKey CurrentKey { get; }
    TValue CurrentValue { get; }
    IEnumerable<TValue> GetRawValues();
    bool TryGetEstimatedCount(out int count);
}

internal static class IKeyValueEnumeratorExtensions
{
    public static IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable<TKey, TValue>(this IKeyValueEnumerator<TKey, TValue> enumerator)
        where TKey : notnull
    {
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<TKey, TValue>(enumerator.CurrentKey, enumerator.CurrentValue);
        }
    }
}