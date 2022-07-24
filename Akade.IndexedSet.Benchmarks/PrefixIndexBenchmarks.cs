using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PrefixIndexBenchmarks
{
    private readonly List<Person> _persons;
    private readonly IndexedSet<Person> _indexedSet;

    public PrefixIndexBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _persons = Enumerable.Range(0, 1000)
                             .Select(_ => new Person())
                             .ToList();

        _indexedSet = _persons.ToIndexedSet()
                              .WithPrefixIndex(x => x.FullName.AsMemory())
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
        return _indexedSet.StartsWith(x => x.FullName.AsMemory(), "Peter".AsMemory()).Count();
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
        return _indexedSet.FuzzyStartsWith(x => x.FullName.AsMemory(), "Peter".AsMemory(), 2).Count();
    }
}
