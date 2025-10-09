using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Bogus;

namespace Akade.IndexedSet.Benchmarks;
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[JsonExporter]
public class RangeIndexBenchmarks
{
    private record class Appointment(DateOnly Date);

    private readonly IndexedSet<Appointment> _indexedSet;
    private readonly List<Appointment> _appointments;

    private readonly DateOnly _start = new(1879, 3, 14);
    private readonly DateOnly _end = new(1955, 4, 18);

    public RangeIndexBenchmarks()
    {
        Randomizer.Seed = new Random(42);
        _appointments = new Faker<Appointment>().CustomInstantiator(f => new Appointment(f.Date.BetweenDateOnly(_start, _end)))
                                                .Generate(10000);

        _indexedSet = _appointments.ToIndexedSet()
                                   .WithRangeIndex(x => x.Date)
                                   .Build();

    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Range")]
    public int Range_Linq()
    {
        DateOnly start = _start.AddYears(10);
        DateOnly end = _start.AddYears(18);
        return _appointments.Where(x => x.Date < start && x.Date > end).Count();
    }

    [Benchmark]
    [BenchmarkCategory("Range")]
    public int Range_IndexedSet()
    {
        DateOnly start = _start.AddYears(10);
        DateOnly end = _start.AddYears(18);
        return _indexedSet.Range(x => x.Date, start, end).Count();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Paging")]
    public int Paging_Linq()
    {
        // ToArray() is important here as directly using Count() is optimized and does not perform the OrderBy
        return _appointments.OrderBy(x => x.Date).Skip(100).Take(10).ToArray().Length;
    }

    [Benchmark]
    [BenchmarkCategory("Paging")]
    public int Paging_IndexedSet()
    {
        return _indexedSet.OrderBy(x => x.Date, 100).Take(10).ToArray().Length;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Min")]
    public DateOnly Min_Linq()
    {
        return _appointments.Min(x => x.Date);
    }

    [Benchmark]
    [BenchmarkCategory("Min")]
    public DateOnly Min_IndexedSet()
    {
        return _indexedSet.Min(x => x.Date);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LessThan")]
    public int LessThan_Linq()
    {
        DateOnly end = _start.AddYears(10);
        return _appointments.Where(x => x.Date < end).Count();
    }

    [Benchmark]
    [BenchmarkCategory("LessThan")]
    public int LessThan_IndexedSet()
    {
        DateOnly end = _start.AddYears(10);
        return _indexedSet.LessThan(x => x.Date, end).Count();
    }

}
