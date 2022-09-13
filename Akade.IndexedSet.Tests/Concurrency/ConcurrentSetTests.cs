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
        ConcurrentIndexedSet<int, TestData> sut = IndexedSetBuilder<TestData>.Create(t => t.PrimaryKey)
                                                                             .WithIndex(t => t.IntProperty)
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

    private static ConcurrentIndexedSet<Person> CreateSet()
    {
        return IndexedSetBuilder<Person>.Create()
                                        .WithUniqueIndex(x => x.Phone)
                                        .WithPrefixIndex(x => x.FullName.AsMemory())
                                        .WithIndex(x => x.Company)
                                        .BuildConcurrent();
    }
}
