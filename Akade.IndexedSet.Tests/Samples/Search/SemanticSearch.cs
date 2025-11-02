#if NET9_0_OR_GREATER
using Akade.IndexedSet.Concurrency;
using Bogus;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Akade.IndexedSet.Tests.Samples.Search;


[TestClass]
public class SemanticSearch
{
    private record class Product(string Name)
    {
        public Embedding<float>? Embedding { get; set; }
    }

    private static ServiceProvider SetupServices()
    {
        ServiceCollection services = new();
        services.AddBertOnnxEmbeddingGenerator(
            onnxModelPath: "../../../Models/BgeMicroV2/model.onnx",
            vocabPath: "../../../Models/BgeMicroV2/vocab.txt");
        return services.BuildServiceProvider();
    }

    private async Task<List<Product>> GenerateProductsAsync(IServiceProvider serviceProvider, int count)
    {
        IEmbeddingGenerator<string, Embedding<float>> generator = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        Randomizer.Seed = new Random(42);

        List<Product> products = new Faker<Product>().CustomInstantiator(x => new(x.Commerce.ProductName()))
                                                     .Generate(500);

        GeneratedEmbeddings<Embedding<float>> embeddings = await generator.GenerateAsync(products.Select(x => x.Name));
        foreach ((Product? product, Embedding<float>? embedding) in products.Zip(embeddings))
        {
            product.Embedding = embedding;
        }
        return products;
    }

    [TestMethod]
    [DataRow(100)]
    [DataRow(1_000)]
    public async Task Semantic_search_via_BgeMicroV2(int count)
    {
        using ServiceProvider sp = SetupServices();

        List<Product> products = await GenerateProductsAsync(sp, count);


        ConcurrentIndexedSet<Product> indexedSet = products.ToIndexedSet()
                                                           .WithVectorIndex(x => x.Embedding!.Vector.Span)
                                                           .BuildConcurrent();

        ReadOnlyMemory<float> searchVector = await EmbedAsync(sp, "trousers");
        IEnumerable<Product> found = indexedSet.ApproximateNearestNeighbors(x => x.Embedding!.Vector.Span, searchVector.Span, 5);

        Assert.IsTrue(found.All(x => x.Name.Contains("pants", StringComparison.OrdinalIgnoreCase)));
    }

    private static async Task<ReadOnlyMemory<float>> EmbedAsync(ServiceProvider sp, string term)
    {
        IEmbeddingGenerator<string, Embedding<float>> generator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        Embedding<float> searchEmbedding = await generator.GenerateAsync(term);
        return searchEmbedding.Vector;
    }
}

#endif
