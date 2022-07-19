using BenchmarkDotNet.Attributes;

namespace Akade.IndexedSet.Benchmarks;

[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class MultiValueIndexBenchmarks
{
    private record Order(int ProductId, decimal Price);
    private readonly List<Order> _orders;
    private readonly List<int> _productIds;
    private readonly ILookup<int, Order> _lookup;
    private readonly IndexedSet<Order> _indexedSet;

    public MultiValueIndexBenchmarks()
    {
        Random r = new(42);
        _orders = new(500);

        _orders.AddRange(Enumerable.Range(0, 500).Select(_ => new Order(r.Next(1, 10), (decimal)r.NextDouble() * 100m)));
        _productIds = Enumerable.Range(1, 10).ToList();

        _lookup = _orders.ToLookup(x => x.ProductId);
        _indexedSet = _orders.ToIndexedSet().WithIndex(x => x.ProductId).Build();
    }

    [Benchmark]
    public decimal MultivalueLookupWithinALoop_NaiveLinqImplementation()
    {
        decimal total = 0;
        foreach (int productId in _productIds)
        {
            foreach (Order product in _orders.Where(x => x.ProductId == productId))
            {
                total += product.Price;
            }
        }
        return total;
    }

    [Benchmark]
    public decimal MultivalueLookupWithinALoop_UsingToBuiltInLookup()
    {
        decimal total = 0;
        foreach (int productId in _productIds)
        {
            foreach (Order product in _lookup[productId])
            {
                total += product.Price;
            }
        }
        return total;
    }

    [Benchmark(Baseline = true)]
    public decimal MultivalueLookupWithinALoop_UsingToIndexedSet()
    {
        decimal total = 0;
        foreach (int productId in _productIds)
        {
            foreach (Order product in _indexedSet.Where(x => x.ProductId, productId))
            {
                total += product.Price;
            }
        }
        return total;
    }

}
