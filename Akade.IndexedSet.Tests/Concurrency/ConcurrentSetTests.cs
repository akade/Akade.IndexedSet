using Akade.IndexedSet.Concurrency;
using Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Concurrency;

/// <summary>
/// Special thanks belong to https://github.com/StephenCleary/AsyncEx for giving me a good starting point of necessary unit test scenarios
/// </summary>
[TestClass]
public class ConcurrentSetTests
{
    private readonly Person[] _testData;

    public ConcurrentSetTests()
    {
        _testData = Enumerable.Range(0, 100).Select(i => new Person(seed: i)).ToArray();
    }

    [TestMethod]
    public async Task ParallelWrites()
    {
        ConcurrentIndexedSet<Person> sut = CreateSet();

        Task[] tasks = _testData.Select(p => Task.Run(() => sut.Add(p))).ToArray();
        await Task.WhenAll(tasks);
        Assert.AreEqual(_testData.Length, tasks.Length);
        Assert.AreEqual(_testData.Length, sut.Count);
    }

    [TestMethod]
    public async Task ParallelReads()
    {
        ConcurrentIndexedSet<Person> sut = CreateSet();
        _ = sut.AddRange(_testData);

        Task<bool>[] tasks = _testData.Select(p => Task.Run(() => sut.TryGetSingle(x => x.Phone, p.Phone, out _))).ToArray();
        bool[] result = await Task.WhenAll(tasks);
        Assert.IsTrue(result.Distinct().Single());
    }

    [TestMethod]
    public async Task ParallelReadsAndWrites()
    {
        ConcurrentIndexedSet<Person> sut = CreateSet();
        _ = sut.AddRange(_testData);

        Task<bool>[] tasks = _testData.Select((p, i) => Task.Run(() =>
            i % 2 == 0
            ? sut.TryGetSingle(x => x.Phone, p.Phone, out _)
            : sut.Remove(p))).ToArray();

        bool[] result = await Task.WhenAll(tasks);
        Assert.IsTrue(result.Distinct().Single());
    }

    private static ConcurrentIndexedSet<Person> CreateSet()
    {
        return IndexedSetBuilder<Person>.Create()
                                        .WithUniqueIndex(x => x.Phone)
                                        .WithPrefixIndex(x => x.FullName.AsMemory())
                                        .WithIndex(x => x.Company)
                                        .BuildConcurrent();
    }
}
