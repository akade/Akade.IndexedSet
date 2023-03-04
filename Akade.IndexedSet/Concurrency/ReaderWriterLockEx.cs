namespace Akade.IndexedSet.Concurrency;

internal sealed class ReaderWriterLockEx : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ReaderDisposable _readerDisposable;
    private readonly WriterDisposable _writerDisposable;

    public ReaderWriterLockEx()
    {
        _readerDisposable = new(this);
        _writerDisposable = new(this);
    }

    public void Dispose()
    {
        _readerDisposable.Dispose();
        _writerDisposable.Dispose();
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

    private sealed class ReaderDisposable : IDisposable
    {
        private readonly ReaderWriterLockEx _parent;

        public ReaderDisposable(ReaderWriterLockEx parent)
        {
            _parent = parent;
        }

        public void Dispose()
        {
            _parent._lock.ExitReadLock();
        }
    }

    private sealed class WriterDisposable : IDisposable
    {
        private readonly ReaderWriterLockEx _parent;

        public WriterDisposable(ReaderWriterLockEx parent)
        {
            _parent = parent;
        }

        public void Dispose()
        {
            _parent._lock.ExitWriteLock();
        }
    }
}