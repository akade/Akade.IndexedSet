using Akade.IndexedSet.SampleData;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using RBush;
using System.Numerics;

namespace Akade.IndexedSet.Benchmarks;

#pragma warning disable AkadeIndexedSetEXP0002 

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[JsonExporter]
public class SpatialIndexBenchmark
{
    private Vector2 _min = new(8.683342209696512f, 47.41151475464969f);
    private Vector2 _max = new(8.92130422393863f, 47.53819397959311f);

    private List<SwissZipCode> _swissZipCodes = null!;
    private IndexedSet<SwissZipCode> _setBulkLoaded = null!;
    private IndexedSet<SwissZipCode> _setNotBulkLoaded = null!;
    private readonly RBush<SwissZipCodeRBushAdapter> _rbushBulkLoaded = new(6);
    private readonly RBush<SwissZipCodeRBushAdapter> _rbushNotBulkLoaded = new(6);
    private readonly Consumer _consumer = new();

    [GlobalSetup]
    public async Task SetupAsync()
    {
        IEnumerable<SwissZipCode> swissZipCodes = await SwissZipCodes.LoadAsync("..\\..\\..\\..");
        _swissZipCodes = swissZipCodes.ToList();
        _setBulkLoaded = IndexedSetBuilder.Create(swissZipCodes)
                                          .WithSpatialIndex<Vector2>(x => x.Coordinates)
                                          .Build();

        _setNotBulkLoaded = IndexedSetBuilder.Create<SwissZipCode>([])
                                             .WithSpatialIndex<Vector2>(x => x.Coordinates)
                                             .Build();

        _rbushBulkLoaded.BulkLoad(swissZipCodes.Select(x => new SwissZipCodeRBushAdapter(x)));

        _swissZipCodes.ForEach(x =>
        {
            _ = _setNotBulkLoaded.Add(x);
            _rbushNotBulkLoaded.Insert(new(x));
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Area")]
    public void AreaLinq()
    {
        _swissZipCodes.Where(x => x.Coordinates.Easting >= _min.X && x.Coordinates.Northing >= _min.Y
                               && x.Coordinates.Easting <= _max.Y && x.Coordinates.Northing <= _max.Y).Consume(_consumer);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("knn10")]
    public void Knn10LinqPointBased()
    {
        // This is not entirely fair as both our R*Tree and RBush have rectangle comparisons to make
        // but it is good to illustrate when to use and when not to
        _swissZipCodes.OrderBy(x => Vector2.Distance(_min, x.Coordinates)).Take(10).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public void Knn10LinqRectangleBased()
    {
        // This is not entirely fair as both our R*Tree and RBush have rectangle comparisons to make
        // but it is good to illustrate when to use and when not to
        _swissZipCodes.OrderBy(x => x.CoordinateRectangle.DistanceTo(_min)).Take(10).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public void RBush_Area_BulkLoaded()
    {
        _rbushBulkLoaded.Search(new Envelope(_min.X, _min.Y, _max.X, _max.Y)).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public void RBush_Area_NotBulkLoaded()
    {
        _rbushNotBulkLoaded.Search(new Envelope(_min.X, _min.Y, _max.X, _max.Y)).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public void RBush_Knn10_BulkLoaded()
    {
        _rbushBulkLoaded.Knn(10, _min.X, _min.Y).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public void RBush_Knn10_NotBulkLoaded()
    {
        _rbushNotBulkLoaded.Knn(10, _min.X, _min.Y).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public void IndexedSet_Area_BulkLoaded()
    {
        _setBulkLoaded.Intersects(x => x.Coordinates, _min, _max).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("Area")]
    public void IndexedSet_Area_NotBulkLoaded()
    {
        _setNotBulkLoaded.Intersects(x => x.Coordinates, _min, _max).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public void IndexedSet_Knn10_BulkLoaded()
    {
        _setBulkLoaded.NearestNeighbors(x => x.Coordinates, _min).Take(10).Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("knn10")]
    public void IndexedSet_Knn10_NotBulkLoaded()
    {
        _setNotBulkLoaded.NearestNeighbors(x => x.Coordinates, _min).Take(10).Consume(_consumer);
    }
}

public class SwissZipCodeRBushAdapter(SwissZipCode code) : ISpatialData
{
    private readonly Envelope _envelope = new(code.Coordinates.Easting, code.Coordinates.Northing, code.Coordinates.Easting, code.Coordinates.Northing);

    public ref readonly Envelope Envelope => ref _envelope;
}
#pragma warning restore AkadeIndexedSetEXP0002
