namespace Akade.IndexedSet.Concurrency;
internal class AsyncReaderWriterLock
{
    private readonly object _synchronizationRoot = new();
    private readonly Queue<TaskCompletionSource<IDisposable>> _waitingWriter = new();
    private readonly List<TaskCompletionSource<IDisposable>> _waitingReaders = new();

    private uint _currentReaders;
    private bool _hasWriter;

    private readonly ReaderLock _readerLock;
    private readonly WriterLock _writerLock;

    public AsyncReaderWriterLock()
    {
        _readerLock = new(this);
        _writerLock = new(this);
    }

    // entrance methods (i.e. required to be synchronized)
    public ValueTask<IDisposable> AcquireReaderLockAsync(CancellationToken cancellationToken)
    {
        lock (_synchronizationRoot)
        {
            if (_hasWriter || _waitingWriter.Any())
            {
                TaskCompletionSource<IDisposable> tcs = new();
                _ = cancellationToken.Register(tcs.SetCanceled);
                _waitingReaders.Add(tcs);

                return new ValueTask<IDisposable>(tcs.Task);
            }
            else
            {
                return ValueTask.FromResult(GetReaderLock());
            }
        }
    }

    public ValueTask<IDisposable> AcquireWriterLockAsync(CancellationToken cancellationToken)
    {
        lock (_synchronizationRoot)
        {
            if (_hasWriter || _currentReaders > 0)
            {
                TaskCompletionSource<IDisposable> tcs = new();
                _ = cancellationToken.Register(tcs.SetCanceled);
                _waitingWriter.Enqueue(tcs);

                return new ValueTask<IDisposable>(tcs.Task);
            }
            else
            {
                return ValueTask.FromResult(GetWriterLock());
            }
        }
    }

    private void OnReaderLockReleased()
    {
        lock (_synchronizationRoot)
        {
            _currentReaders--;

            if (_currentReaders == 0)
            {
                DispatchWaitingWriter();
            }
        }
    }

    private void OnWriterLockReleased()
    {
        lock (_synchronizationRoot)
        {
            _hasWriter = false;
            DispatchWaitingWriter();
        }
    }

    // Non-entrance methods (no need to require lock)
    private IDisposable GetReaderLock()
    {
        _currentReaders++;
        return _readerLock;
    }

    private IDisposable GetWriterLock()
    {
        _hasWriter = true;
        return _writerLock;
    }

    private void DispatchWaitingWriter()
    {
        while (_waitingWriter.TryDequeue(out TaskCompletionSource<IDisposable>? tcs))
        {
            if (!tcs.Task.IsCanceled)
            {
                tcs.SetResult(GetWriterLock());
                return;
            }
        }
        DispatchWaitingReaders();
    }

    private void DispatchWaitingReaders()
    {
        foreach (TaskCompletionSource<IDisposable> reader in _waitingReaders)
        {
            if (!reader.Task.IsCanceled)
            {
                reader.SetResult(GetReaderLock());
            }
        }
        _waitingReaders.Clear();
    }

    private class ReaderLock : IDisposable
    {
        private readonly AsyncReaderWriterLock _parentLock;
        public ReaderLock(AsyncReaderWriterLock parentLock)
        {
            _parentLock = parentLock;
        }

        public void Dispose()
        {
            _parentLock.OnReaderLockReleased();
        }
    }

    private class WriterLock : IDisposable
    {
        private readonly AsyncReaderWriterLock _parentLock;
        public WriterLock(AsyncReaderWriterLock parentLock)
        {
            _parentLock = parentLock;
        }

        public void Dispose()
        {
            _parentLock.OnWriterLockReleased();
        }
    }

}
