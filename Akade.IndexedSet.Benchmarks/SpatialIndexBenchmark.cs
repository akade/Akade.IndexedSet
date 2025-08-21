using Akade.IndexedSet.SampleData;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using RBush;
using System.ComponentModel;
using System.Numerics;

namespace Akade.IndexedSet.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[JsonExporter]
public class SpatialIndexBenchmark
{
    private Vector2 _min = new(8.683342209696512f, 47.41151475464969f);
    private Vector2 _max = new(8.92130422393863f, 47.53819397959311f);

    private List<SwissZipCode> _swissZipCodes = null!;
    private IndexedSet<SwissZipCode> _setBulkLoaded = null!;
    private IndexedSet<SwissZipCode> _setNotBulkLoaded = null!;
    private readonly RBush<SwissZipCodeRBushAdapter> _rbushBulkLoaded = new();
    private readonly RBush<SwissZipCodeRBushAdapter> _rbushNotBulkLoaded = new();

    [GlobalSetup]
    public async Task SetupAsync()
    {
        var swissZipCodes = await SwissZipCodes.LoadAsync("..\\..\\..\\..");
        _swissZipCodes = swissZipCodes.ToList();
        _setBulkLoaded = IndexedSetBuilder.Create(swissZipCodes)
                                          .WithSpatialIndex<Vector2>(x => x.Coordinates)
                                          .Build();

        _setNotBulkLoaded = IndexedSetBuilder.Create<SwissZipCode>([])
                                             .WithSpatialIndex<Vector2>(x => x.Coordinates)
                                             .Build();

        RBush<SwissZipCodeRBushAdapter> r = new();
        r.BulkLoad(swissZipCodes.Select(x => new SwissZipCodeRBushAdapter(x)));


        _swissZipCodes.ForEach(x =>
        {
            _setBulkLoaded.Add(x);
            _rbushNotBulkLoaded.Insert(new(x));
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Area")]
    public int AreaLinq()
    {
        return _swissZipCodes.Where(x => x.Coordinates.Easting >= _min.X && x.Coordinates.Northing >= _min.Y
                                      && x.Coordinates.Easting <= _min.Y && x.Coordinates.Northing <= _max.Y).Count();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("knn10")]
    public int Knn10Linq()
    {
        return _swissZipCodes.OrderBy(x => Vector2.Distance(_min, x.Coordinates)).Take(10).Count();
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public int RBush_Area_BulkLoaded()
    {
        return _rbushBulkLoaded.Search(new Envelope(_min.X, _min.Y, _max.X, _max.Y)).Count;
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public int RBush_Area_NotBulkLoaded()
    {
        return _rbushNotBulkLoaded.Search(new Envelope(_min.X, _min.Y, _max.X, _max.Y)).Count;
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public int RBush_Knn10_BulkLoaded()
    {
        return _rbushBulkLoaded.Knn(10, _min.X, _min.Y).Count;
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public int RBush_Knn10_NotBulkLoaded()
    {
        return _rbushNotBulkLoaded.Knn(10, _min.X, _min.Y).Count;
    }


    [Benchmark]
    [BenchmarkCategory("Area")]
    public int IndexedSet_Area_BulkLoaded()
    {
        return _setBulkLoaded.Range<Vector2>(x => x.Coordinates, _min, _max).Count();
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public int IndexedSet_Area_NotBulkLoaded()
    {
        return _setNotBulkLoaded.Range<Vector2>(x => x.Coordinates, _min, _max).Count();
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public int IndexedSet_Knn10_BulkLoaded()
    {
        return _setBulkLoaded.NearestNeighbors<Vector2>(x => x.Coordinates, _min).Take(10).Count();
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public int IndexedSet_Knn10_NotBulkLoaded()
    {
        return _setNotBulkLoaded.NearestNeighbors<Vector2>(x => x.Coordinates, _min).Take(10).Count();
    }


}

public class SwissZipCodeRBushAdapter(SwissZipCode code) : ISpatialData
{
    private readonly Envelope _envelope = new Envelope(code.Coordinates.Easting, code.Coordinates.Northing, code.Coordinates.Easting, code.Coordinates.Northing);

    public ref readonly Envelope Envelope => ref _envelope;
}
