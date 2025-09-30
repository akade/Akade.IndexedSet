namespace Akade.IndexedSet.Utils;

internal sealed class KeyValueEnumerator<TKey, TValue>(IEnumerable<TValue> values, Func<TValue, TKey> keyAccessor) : IKeyValueEnumerator<TKey, TValue>, IDisposable
#if NET9_0_OR_GREATER
    where TKey : notnull, allows ref struct
#else
    where TKey : notnull
#endif
{
    private IEnumerator<TValue> _enumerator = values.GetEnumerator();
    private readonly Func<TValue, TKey> _keyAccessor = keyAccessor;
    public bool MoveNext()
    {
        _enumerator ??= values.GetEnumerator();
        bool result = _enumerator.MoveNext();
        if (!result)
        {
            _enumerator.Dispose();
        }
        return result;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public TKey CurrentKey => _keyAccessor(_enumerator.Current);
    public TValue CurrentValue => _enumerator.Current;
    public IEnumerable<TValue> GetRawValues()
    {
        return values;
    }

    public bool TryGetEstimatedCount(out int count)
    {
        return values.TryGetNonEnumeratedCount(out count);
    }
}

internal sealed class MultiKeyValueEnumerator<TKey, TValue>(IEnumerable<TValue> values, Func<TValue, IEnumerable<TKey>> keysAccessor) : IKeyValueEnumerator<TKey, TValue>, IDisposable
#if NET9_0_OR_GREATER
    where TKey : notnull, allows ref struct
#else
    where TKey : notnull
#endif
{
    private readonly IEnumerator<TValue> _enumerator = values.GetEnumerator();
    private readonly Func<TValue, IEnumerable<TKey>> _keysAccessor = keysAccessor;
    private IEnumerator<TKey>? _currentKeysEnumerator;

    public bool MoveNext()
    {
        // the loop is used to skip over empty keys enumerators
        while (true)
        {
            if (_currentKeysEnumerator is null)
            {
                if (!_enumerator.MoveNext())
                {
                    return false;
                }

                _currentKeysEnumerator = _keysAccessor(_enumerator.Current).GetEnumerator();
            }

            if (_currentKeysEnumerator.MoveNext())
            {
                return true;
            }
            else
            {
                _currentKeysEnumerator.Dispose();
                _currentKeysEnumerator = null;
            }
        }
    }

    public void Dispose()
    {
        _currentKeysEnumerator?.Dispose();
        _enumerator.Dispose();
    }

    public TKey CurrentKey => _currentKeysEnumerator!.Current;
    public TValue CurrentValue => _enumerator.Current;
    public IEnumerable<TValue> GetRawValues()
    {
        return values;
    }

    public bool TryGetEstimatedCount(out int count)
    {
        if (values.TryGetNonEnumeratedCount(out int rawCount))
        {
            count = rawCount * 2;
            return true;
        }
        count = 0;
        return false;
    }
}