using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class FullTextIndexBenchmarks
{
    private record class Document(string Content);

    private readonly IndexedSet<Document> _indexedSet;
    private readonly List<Document> _document;

    public FullTextIndexBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _document = new Faker<Document>().CustomInstantiator(f => new Document(f.Rant.Review()))
                                         .Generate(1000);

        _indexedSet = _document.ToIndexedSet()
                               .WithFullTextIndex(x => x.Content)
                               .Build();

    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Contains")]
    public int Contains_Linq()
    {
        return _document.Count(x => x.Content.Contains("excellent"));
    }

    [Benchmark]
    [BenchmarkCategory("Contains")]
    public int Contains_IndexedSet()
    {
        return _indexedSet.Contains(x => x.Content, "excellent").Count();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Fuzzy Contains")]
    public int FuzzyContains_Linq()
    {
        return _document.Count(x =>
        {
            if (Fastenshtein.Levenshtein.Distance("excellent", x.Content) < 2)
            {
                return true;
            }

            for (int i = 0; i < x.Content.Length - 9; i++)
            {
                if (Fastenshtein.Levenshtein.Distance("excellent", x.Content.Substring(i, 9)) < 2)
                {
                    return true;
                }
            }

            return false;
        });
    }

    [Benchmark]
    [BenchmarkCategory("Fuzzy Contains")]
    public int FuzzyContains_IndexedSet()
    {
        return _indexedSet.FuzzyContains(x => x.Content, "excellent", 2).Count();
    }
}
