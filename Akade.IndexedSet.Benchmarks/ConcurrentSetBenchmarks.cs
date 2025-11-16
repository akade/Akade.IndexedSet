using Akade.IndexedSet.Concurrency;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[JsonExporter]
public class ConcurrentSetBenchmarks
{
    private readonly List<Person> _persons;
    private readonly IndexedSet<Person> _indexedSet;
    private readonly ConcurrentIndexedSet<Person> _concurrentIndexedSet;

    public ConcurrentSetBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _persons = Enumerable.Range(0, 1000)
                             .Select(_ => new Person())
                             .ToList();

        _indexedSet = _persons.ToIndexedSet()
                              .WithUniqueIndex(x => x.Phone)
                              .WithFullTextIndex(x => x.FullName)
                              .WithRangeIndex(GetAge)
                              .Build();

        _concurrentIndexedSet = _persons.ToIndexedSet()
                                        .WithUniqueIndex(x => x.Phone)
                                        .WithFullTextIndex(x => x.FullName)
                                        .WithRangeIndex(GetAge)
                                        .BuildConcurrent();
    }

    public static int GetAge(Person p)
    {
        DateTime today = DateTime.Today;
        int age = today.Year - p.DateOfBirth.Year;

        if (p.DateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Unique")]
    public bool UniqueLookup()
    {
        return _indexedSet.TryGetSingle(x => x.Phone, "random", out _);
    }

    [Benchmark]
    [BenchmarkCategory("Unique")]
    public bool ConcurrentUniqueLookup()
    {
        return _concurrentIndexedSet.TryGetSingle(x => x.Phone, "random", out _);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LessThan")]
    public int LessThanLookup()
    {
        return _indexedSet.LessThan(GetAge, 12).Count();
    }

    [Benchmark]
    [BenchmarkCategory("LessThan")]
    public int ConcurrentLessThanLookup()
    {
        return _concurrentIndexedSet.LessThan(GetAge, 12).Count();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("FullText")]
    public int FullTextLookup()
    {
        return _indexedSet.FuzzyContains(x => x.FullName, "Peter", 1).Count();
    }

    [Benchmark]
    [BenchmarkCategory("FullText")]
    public int ConcurrentFullTextLookup()
    {
        return _concurrentIndexedSet.FuzzyContains(x => x.FullName, "Peter", 1).Count();
    }
}
