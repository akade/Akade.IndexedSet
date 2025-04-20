namespace Akade.IndexedSet.Concurrency;

internal sealed class ReaderWriterLockEx : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly ReaderDisposable _readerDisposable;
    private readonly WriterDisposable _writerDisposable;

    public ReaderWriterLockEx()
    {
        _readerDisposable = new(this);
        _writerDisposable = new(this);
    }

    public void Dispose()
    {
        _lock.Dispose();
    }

    public IDisposable EnterReadLock()
    {
        _lock.EnterReadLock();
        return _readerDisposable;
    }

    public IDisposable EnterWriteLock()
    {
        _lock.EnterWriteLock();
        return _writerDisposable;
    }

    private sealed class ReaderDisposable(ReaderWriterLockEx parent) : IDisposable
    {
        private readonly ReaderWriterLockEx _parent = parent;

        public void Dispose()
        {
            _parent._lock.ExitReadLock();
        }
    }

    private sealed class WriterDisposable(ReaderWriterLockEx parent) : IDisposable
    {
        private readonly ReaderWriterLockEx _parent = parent;

        public void Dispose()
        {
            _parent._lock.ExitWriteLock();
        }
    }
}