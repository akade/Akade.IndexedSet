using Akade.IndexedSet.Concurrency;
using Akade.IndexedSet.Tests.Data;
using Akade.IndexedSet.Tests.TestUtilities;
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

    [TestMethod]
    public async Task Update_method_allows_to_execute_code_in_isolation()
    {
        ConcurrentIndexedSet<int, TestData> sut = IndexedSetBuilder<TestData>.Create(x => x.PrimaryKey)
                                                                             .WithIndex(x => x.IntProperty)
                                                                             .BuildConcurrent();
        var element = new TestData(1, 0, GuidGen.Get(1), "Test");
        _ = sut.Add(element);

        await Task.WhenAll(Enumerable.Range(0, 100).Select(__ => Task.Run(() => sut.Update(set =>
        {
            TestData dataToUpdate = set[1];
            _ = set.Update(dataToUpdate, x => x with { IntProperty = x.IntProperty + 10 });
        }))));

        Assert.AreEqual(1000, sut.Single(1).IntProperty);
    }

    [TestMethod]
    public async Task Parallel_reads_and_writes_with_read_method()
    {
        ConcurrentIndexedSet<int, TestData> sut = IndexedSetBuilder<TestData>.Create(x => x.PrimaryKey)
                                                                             .WithIndex(x => x.IntProperty)
                                                                             .WithIndex(x => x.GuidProperty)
                                                                             .BuildConcurrent();
        _ = sut.Add(new TestData(1, 0, GuidGen.Get(1), "Test"));
        _ = sut.Add(new TestData(2, 0, GuidGen.Get(1), "Test"));

        IEnumerable<Task> tasks = Enumerable.Range(0, 100).Select(i => i % 2 == 0
            ? Task.Run(() => sut.Update(set =>
                {
                    TestData dataToUpdate = set[1];
                    _ = set.Update(dataToUpdate, x => x with { IntProperty = x.IntProperty + 10 });
                }))
            : Task.Run(() => Assert.AreEqual(2, sut.Read(set => set.Where(x => x.GuidProperty, GuidGen.Get(1))).Count()))
        );
        await Task.WhenAll(tasks);

        Assert.AreEqual(500, sut.Single(1).IntProperty);
    }

    private static ConcurrentIndexedSet<Person> CreateSet()
    {
        return IndexedSetBuilder<Person>.Create()
                                        .WithUniqueIndex(x => x.Phone)
                                        .WithPrefixIndex(x => x.FullName)
                                        .WithIndex(x => x.Company)
                                        .BuildConcurrent();
    }
}
