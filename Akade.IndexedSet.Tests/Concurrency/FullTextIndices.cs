using Akade.IndexedSet.Concurrency;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Concurrency;

/// <summary>
/// Special thanks belong to https://github.com/StephenCleary/AsyncEx for giving me a good starting point of necessary unit test scenarios
/// </summary>
[TestClass]
public class AsyncReaderWriterLockTests
{
    [TestMethod]
    public async Task Unlocked_PermitsWriterLock()
    {
        AsyncReaderWriterLock sut = new();
        _ = await sut.AcquireWriterLockAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task Unlocked_PermitsMultipleReaderLocks()
    {
        AsyncReaderWriterLock sut = new();
        _ = await sut.AcquireReaderLockAsync(CancellationToken.None);
        _ = await sut.AcquireReaderLockAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task WriteLocked_PreventsAnotherWriterLock()
    {
        AsyncReaderWriterLock sut = new();
        _ = await sut.AcquireWriterLockAsync(CancellationToken.None);
        ValueTask<IDisposable> secondWriter = sut.AcquireWriterLockAsync(CancellationToken.None);
        await DoesNotCompleteAsync(secondWriter);
    }

    [TestMethod]
    public async Task WriteLocked_PreventsReaderLock()
    {
        AsyncReaderWriterLock sut = new();
        _ = await sut.AcquireWriterLockAsync(CancellationToken.None);
        Task<IDisposable> reader = sut.AcquireReaderLockAsync(CancellationToken.None).AsTask();
        await Task.Delay(500);
        Assert.IsFalse(reader.IsCompleted);
    }

    [TestMethod]
    public async Task WriteLocked_Unlocked_PermitsAnotherWriterLock()
    {
        AsyncReaderWriterLock sut = new();

        TaskCompletionSource secondWriterLockRequest = new();

        var firstWriter = Task.Run(async () =>
        {
            using (await sut.AcquireWriterLockAsync(CancellationToken.None))
            {
                await secondWriterLockRequest.Task;
            }
        });

        bool secondWriterExecuted = false;

        var secondWriter = Task.Run(async () =>
        {
            using (await sut.AcquireWriterLockAsync(CancellationToken.None))
            {
                secondWriterExecuted = true;
            }
        });

        secondWriterLockRequest.SetResult();
        await secondWriter;
        Assert.IsTrue(secondWriterExecuted);
    }

    [TestMethod]
    public async Task ReadLocked_PreventsWriterLock()
    {
        AsyncReaderWriterLock sut = new();
        _ = await sut.AcquireReaderLockAsync(CancellationToken.None);
        ValueTask<IDisposable> task = sut.AcquireWriterLockAsync(CancellationToken.None);
        await DoesNotCompleteAsync(task);
    }

    [TestMethod]
    public void WriterLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
    {
        AsyncReaderWriterLock sut = new();
        var token = new CancellationToken(true);

        ValueTask<IDisposable> task = sut.AcquireWriterLockAsync(token);

        Assert.IsTrue(task.IsCompleted);
        Assert.IsFalse(task.IsCanceled);
        Assert.IsFalse(task.IsFaulted);
    }

    [TestMethod]
    public void WriterLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
    {
        AsyncReaderWriterLock sut = new();
        var token = new CancellationToken(true);
        _ = sut.AcquireWriterLockAsync(CancellationToken.None);

        ValueTask<IDisposable> task = sut.AcquireWriterLockAsync(token);

        Assert.IsTrue(task.IsCompleted);
        Assert.IsTrue(task.IsCanceled);
        Assert.IsFalse(task.IsFaulted);
    }

    [TestMethod]
    public void ReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
    {
        AsyncReaderWriterLock sut = new();
        var token = new CancellationToken(true);

        ValueTask<IDisposable> task = sut.AcquireReaderLockAsync(token);

        Assert.IsTrue(task.IsCompleted);
        Assert.IsFalse(task.IsCanceled);
        Assert.IsFalse(task.IsFaulted);
    }

    [TestMethod]
    public void ReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
    {
        AsyncReaderWriterLock sut = new();
        var token = new CancellationToken(true);
        _ = sut.AcquireWriterLockAsync(CancellationToken.None);

        ValueTask<IDisposable> task = sut.AcquireReaderLockAsync(token);

        Assert.IsTrue(task.IsCompleted);
        Assert.IsTrue(task.IsCanceled);
        Assert.IsFalse(task.IsFaulted);
    }

    [TestMethod]
    public async Task WriteLocked_WriterLockCancelled_DoesNotTakeLockWhenUnlocked()
    {
        AsyncReaderWriterLock sut = new();
        using (await sut.AcquireWriterLockAsync(CancellationToken.None))
        {
            var cts = new CancellationTokenSource();
            Task<IDisposable>? task = sut.AcquireWriterLockAsync(cts.Token).AsTask();
            cts.Cancel();
            Assert.IsTrue(task.IsCanceled);
        }
        _ = await sut.AcquireWriterLockAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task WriteLocked_ReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
    {
        AsyncReaderWriterLock sut = new();
        using (await sut.AcquireWriterLockAsync(CancellationToken.None))
        {
            var cts = new CancellationTokenSource();
            ValueTask<IDisposable> task = sut.AcquireReaderLockAsync(cts.Token);
            cts.Cancel();
            Assert.IsTrue(task.IsCanceled);
        }

        _ = await sut.AcquireReaderLockAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task LockReleased_WriteTakesPriorityOverRead()
    {
        AsyncReaderWriterLock sut = new();
        ValueTask<IDisposable> writeLock, readLock;
        using (await sut.AcquireWriterLockAsync(CancellationToken.None))
        {
            readLock = sut.AcquireReaderLockAsync(CancellationToken.None);
            writeLock = sut.AcquireWriterLockAsync(CancellationToken.None);
        }

        _ = await writeLock;
        await DoesNotCompleteAsync(readLock);
    }

    [TestMethod]
    public async Task ReaderLocked_ReaderReleased_ReaderAndWriterWaiting_DoesNotReleaseReaderOrWriter()
    {
        AsyncReaderWriterLock sut = new();
        ValueTask<IDisposable> readLock, writeLock;
        _ = await sut.AcquireReaderLockAsync(CancellationToken.None);
        using (await sut.AcquireReaderLockAsync(CancellationToken.None))
        {
            writeLock = sut.AcquireWriterLockAsync(CancellationToken.None);
            readLock = sut.AcquireReaderLockAsync(CancellationToken.None);
        }

        await Task.WhenAll(DoesNotCompleteAsync(writeLock), DoesNotCompleteAsync(readLock));
    }

    [TestMethod]
    public async Task ReadLock_WriteLockCanceled_TakesLock()
    {
        AsyncReaderWriterLock sut = new();

        IDisposable initialReadLock = await sut.AcquireReaderLockAsync(CancellationToken.None);

        CancellationTokenSource cancellationTokenSource = new();

        ValueTask<IDisposable> waitingWriter = sut.AcquireWriterLockAsync(cancellationTokenSource.Token);

        bool executedSecondReader = false;

        var secondReaderTask = Task.Run(async () =>
        {
            using (await sut.AcquireReaderLockAsync(CancellationToken.None))
            {
                executedSecondReader = true;
            }
        });

        cancellationTokenSource.Cancel();
        initialReadLock.Dispose();

        await secondReaderTask;
        Assert.IsTrue(executedSecondReader);
    }

    private static async Task DoesNotCompleteAsync(ValueTask<IDisposable> secondWriter)
    {
        await Task.Delay(500);
        Assert.IsFalse(secondWriter.IsCompleted);
    }
}
