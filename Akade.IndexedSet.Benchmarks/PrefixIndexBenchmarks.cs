using Autypo;
using Autypo.Configuration;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[JsonExporter]
public class PrefixIndexBenchmarks
{
    private readonly List<Person> _persons;
    private readonly IndexedSet<Person> _indexedSet;
    private IAutypoSearch<Person> _autoTypo = null!;
    private IAutypoSearch<Person> _autoTypoFuzzy = null!;

    public PrefixIndexBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _persons = Enumerable.Range(0, 1000)
                             .Select(_ => new Person())
                             .ToList();

        _indexedSet = _persons.ToIndexedSet()
                              .WithPrefixIndex(x => x.FullName)
                              .Build();

        
    }

    [GlobalSetup]
    public async Task InitAsync()
    {
        _autoTypo = await AutypoFactory.CreateSearchAsync<Person>(config => 
            config.WithDataSource(_persons)
                  .WithIndex(x => x.FullName, x => x.WithFuzziness(0)));

        _autoTypoFuzzy = await AutypoFactory.CreateSearchAsync<Person>(config =>
            config.WithDataSource(_persons)
                  .WithIndex(x => x.FullName, x => x.WithFuzziness(2)));
    }


    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StartsWith")]
    public Person[] StartsWith_Linq()
    {
        return _persons.Where(x => x.FullName.StartsWith("Tiffany", StringComparison.Ordinal)).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("StartsWith")]
    public Person[] StartsWith_IndexedSet()
    {
        return _indexedSet.StartsWith(x => x.FullName, "Tiffany").ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("StartsWith")]
    public Person[] StartsWith_Autypo()
    {
        return _autoTypo.Search("Tiffany").Select(x => x.Value).ToArray();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Fuzzy StartsWith")]
    public Person[] FuzzyStartsWith_Linq()
    {
        return _persons.Where(x => Fastenshtein.Levenshtein.Distance(x.FullName[..Math.Min(7, x.FullName.Length)], "Tiffany") <= 2).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Fuzzy StartsWith")]
    public Person[] FuzzyStartsWith_IndexedSet()
    {
        return _indexedSet.FuzzyStartsWith(x => x.FullName, "Tiffany", 2).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Fuzzy StartsWith")]
    public Person[] FuzzyStartsWith_Autypo()
    {
        return _autoTypoFuzzy.Search("Tiffany").Select(x => x.Value).ToArray();
    }
}
