using BenchmarkDotNet.Attributes;

namespace Akade.IndexedSet.Benchmarks;

[MemoryDiagnoser]
[DisassemblyDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net60)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net70)]
public class UniqueIndexBenchmarks
{
    private record Product(int Id, int Stock);

    private readonly List<Product> _products;
    private readonly List<(int productId, int amount)> _orders;
    private readonly Dictionary<int, Product> _dictionary;
    private readonly IndexedSet<int, Product> _indexedSet;

    public UniqueIndexBenchmarks()
    {
        Random r = new(42);
        _products = Enumerable.Range(0, 1000)
                              .Select(i => new Product(i, r.Next(0, 10)))
                              .ToList();

        _orders = Enumerable.Range(0, 100)
                            .Select(_ => (r.Next(0, 500), r.Next(0, 10)))
                            .ToList();

        _dictionary = _products.ToDictionary(x => x.Id);
        _indexedSet = _products.ToIndexedSet(x => x.Id)
                               .Build();
    }

    [Benchmark(Baseline = true)]
    public bool Unqiue_Linq()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _products.First(product => product.Id == productId).Stock >= amount;
        }
        return result;
    }

    [Benchmark]
    public bool Unique_Dictionary()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _dictionary[productId].Stock >= amount;
        }
        return result;
    }

    [Benchmark]
    public bool Unique_IndexedSet_PrimaryKey()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _indexedSet[productId].Stock >= amount;
        }
        return result;
    }

    [Benchmark()]
    public bool Unique_IndexedSet_Single()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _indexedSet.Single(x => x.Id, productId).Stock >= amount;
        }
        return result;
    }

}
