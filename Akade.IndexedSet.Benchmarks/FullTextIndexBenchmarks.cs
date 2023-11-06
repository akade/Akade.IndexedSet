using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net70)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
[JsonExporter]
public class FullTextIndexBenchmarks
{
    public record class Document(string Content);

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
    public Document[] Contains_Linq()
    {
        return _document.Where(x => x.Content.Contains("cheeseburger")).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Contains")]
    public Document[] Contains_IndexedSet()
    {
        return _indexedSet.Contains(x => x.Content, "cheeseburger").ToArray();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Fuzzy Contains")]
    public Document[] FuzzyContains_Linq()
    {
        return _document.Where(x =>
        {
            if (Fastenshtein.Levenshtein.Distance("cheeseburger", x.Content) < 2)
            {
                return true;
            }

            for (int i = 0; i < x.Content.Length - 9; i++)
            {
                if (Fastenshtein.Levenshtein.Distance("cheeseburger", x.Content.Substring(i, 9)) < 2)
                {
                    return true;
                }
            }

            return false;
        }).ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("Fuzzy Contains")]
    public Document[] FuzzyContains_IndexedSet()
    {
        return _indexedSet.FuzzyContains(x => x.Content, "cheeseburger", 2).ToArray();
    }
}
