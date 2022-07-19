using BenchmarkDotNet.Attributes;

namespace Akade.IndexedSet.Benchmarks;

[MemoryDiagnoser]
[DisassemblyDiagnoser]
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
        _products = new(500);
        _orders = new(100);

        _products.AddRange(Enumerable.Range(0, 500).Select(i => new Product(i, r.Next(0, 10))));
        _orders.AddRange(Enumerable.Range(0, 100).Select(_ => (r.Next(0, 500), r.Next(0, 10))));

        _dictionary = _products.ToDictionary(x => x.Id);

        _indexedSet = _products.ToIndexedSet(x => x.Id).Build();
    }

    [Benchmark]
    public bool UniqueLookupWithinALoop_NaiveLinqImplementation()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _products.First(product => product.Id == productId).Stock >= amount;
        }
        return result;
    }

    [Benchmark]
    public bool UniqueLookupWithinALoop_UsingDictionary()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _dictionary[productId].Stock >= amount;
        }
        return result;
    }

    [Benchmark(Baseline = true)]
    public bool UniqueLookupWithinALoop_UsingToIndexedSetPrimaryKey()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _indexedSet[productId].Stock >= amount;
        }
        return result;
    }

    [Benchmark()]
    public bool UniqueLookupWithinALoop_UsingToIndexedSetViaSingle()
    {
        bool result = true;
        foreach ((int productId, int amount) in _orders)
        {
            result &= _indexedSet.Single(x => x.Id, productId).Stock >= amount;
        }
        return result;
    }

}
