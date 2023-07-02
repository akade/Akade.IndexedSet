using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net70)]
public class PrefixIndexBenchmarks
{
    private readonly List<Person> _persons;
    private readonly IndexedSet<Person> _indexedSet;

    public PrefixIndexBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _persons = Enumerable.Range(0, 5000)
                             .Select(_ => new Person())
                             .ToList();

        _indexedSet = _persons.ToIndexedSet()
                              .WithPrefixIndex(x => x.FullName)
                              .Build();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StartsWith")]
    public int StartsWith_Linq()
    {
        return _persons.Count(x => x.FullName.StartsWith("Peter", StringComparison.Ordinal));
    }

    [Benchmark]
    [BenchmarkCategory("StartsWith")]
    public int StartsWith_IndexedSet()
    {
        return _indexedSet.StartsWith(x => x.FullName, "Peter").Count();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Fuzzy StartsWith")]
    public int FuzzyStartsWith_Linq()
    {
        return _persons.Count(x => Fastenshtein.Levenshtein.Distance(x.FullName[..5], "Peter") <= 2);
    }

    [Benchmark]
    [BenchmarkCategory("Fuzzy StartsWith")]
    public int FuzzyStartsWith_IndexedSet()
    {
        return _indexedSet.FuzzyStartsWith(x => x.FullName, "Peter", 2).Count();
    }
}
