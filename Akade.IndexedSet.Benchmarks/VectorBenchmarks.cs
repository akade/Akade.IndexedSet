using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Bogus;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics.Tensors;

namespace Akade.IndexedSet.Benchmarks;

[MemoryDiagnoser]
[DisassemblyDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[JsonExporter]
public class VectorBenchmarks
{
    private List<Product> _largeProductCollection = [];
    private List<Product> _smallProductCollection = [];
    private readonly Consumer _consumer = new();

    private IndexedSet<Product> _indexedSetLarge = null!;
    private IndexedSet<Product> _indexedSetSmall = null!;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        ServiceCollection services = new();
        services.AddBertOnnxEmbeddingGenerator(
            onnxModelPath: "../../../../../../../../Akade.IndexedSet.Tests/Models/BgeMicroV2/model.onnx",
            vocabPath: "../../../../../../../../Akade.IndexedSet.Tests/Models/BgeMicroV2/vocab.txt");


        using ServiceProvider sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();


        Randomizer.Seed = new Random(42);

        var productFaker = new Faker<Product>().CustomInstantiator(x => new(x.Commerce.ProductName()));

        _largeProductCollection = productFaker.Generate(10_000);
        GeneratedEmbeddings<Embedding<float>> embeddings = await generator.GenerateAsync(_largeProductCollection.Select(x => x.Name));
        for (int i = 0; i < _largeProductCollection.Count; i++)
        {
            _largeProductCollection[i].Embedding = embeddings[i];
        }

        _smallProductCollection = _largeProductCollection.Take(100).ToList();

        _indexedSetLarge = _largeProductCollection.ToIndexedSet()
                                                  .WithVectorIndex(x => x.Embedding!.Vector.Span)
                                                  .Build();

        _indexedSetSmall = _smallProductCollection.ToIndexedSet()
                                                  .WithVectorIndex(x => x.Embedding!.Vector.Span)
                                                  .Build();
    }

    [Benchmark]
    [BenchmarkCategory("large")]
    public void NearestNeighbor_Large_Linq()
    {
        Product queryProduct = _largeProductCollection[0];
        _largeProductCollection
            .OrderBy(x => 1 - TensorPrimitives.CosineSimilarity(queryProduct.Embedding!.Vector.Span, x.Embedding!.Vector.Span))
            .Take(10)
            .Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("large")]
    public void NearestNeighbor_Large_IndexedSet()
    {
        Product queryProduct = _largeProductCollection[0];
        _indexedSetLarge.ApproximateNearestNeighbors(x => x.Embedding!.Vector.Span, queryProduct.Embedding!.Vector.Span, 10)
                        .Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("small")]
    public void NearestNeighbor_Small_Linq()
    {
        Product queryProduct = _smallProductCollection[0];
        _smallProductCollection
            .OrderBy(x => 1 - TensorPrimitives.CosineSimilarity(queryProduct.Embedding!.Vector.Span, x.Embedding!.Vector.Span))
            .Take(10)
            .Consume(_consumer);
    }

    [Benchmark]
    [BenchmarkCategory("small")]
    public void NearestNeighbor_Small_IndexedSet()
    {
        Product queryProduct = _smallProductCollection[0];
        _indexedSetLarge.ApproximateNearestNeighbors(x => x.Embedding!.Vector.Span, queryProduct.Embedding!.Vector.Span, 10)
                        .Consume(_consumer);
    }

    public record class Product(string Name)
    {
        public Embedding<float>? Embedding { get; set; }
    }
}
