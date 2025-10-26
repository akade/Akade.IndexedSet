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


    [TestMethod]
    public async Task SampleTestAsync()
    {
        ServiceCollection services = new();
        services.AddBertOnnxEmbeddingGenerator(
            onnxModelPath: "../../../Models/BgeMicroV2/model.onnx",
            vocabPath: "../../../Models/BgeMicroV2/vocab.txt");


        using ServiceProvider sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();


        Randomizer.Seed = new Random(42);

        var productFaker = new Faker<Product>().CustomInstantiator(x => new(x.Commerce.ProductName()));

        List<Product> allProducts = [];

        // below the threshold for ANN indexing
        allProducts.AddRange(await GenerateProductsAsync(100));
        ConcurrentIndexedSet<Product> indexedSet = allProducts.ToIndexedSet()
                                                              .WithVectorIndex(x => x.Embedding!.Vector.Span)
                                                              .BuildConcurrent();


        IEnumerable<Product> found = await SearchAsync("trousers", 5);
        Assert.IsTrue(found.All(x => x.Name.Contains("pants", StringComparison.OrdinalIgnoreCase)));

        // after the threshold for ANN indexing
        allProducts.AddRange(await GenerateProductsAsync(1_000));
        indexedSet.AddRange(allProducts[100..]);

        found = await SearchAsync("trousers", 5);
        Assert.IsTrue(found.All(x => x.Name.Contains("pants", StringComparison.OrdinalIgnoreCase)));


        async Task<List<Product>> GenerateProductsAsync(int count)
        {
            List<Product> products = productFaker.Generate(count);
            GeneratedEmbeddings<Embedding<float>> embeddings = await generator.GenerateAsync(products.Select(x => x.Name));

            foreach ((Product? product, Embedding<float>? embedding) in products.Zip(embeddings))
            {
                product.Embedding = embedding;
            }

            return products;
        }

        async Task<IEnumerable<Product>> SearchAsync(string search, int nearestNeighbors)
        {
            Embedding<float> embedding = await generator.GenerateAsync(search);
            return indexedSet.ApproximateNearestNeighbors(x => x.Embedding!.Vector.Span, embedding.Vector.Span, nearestNeighbors);
        }
    }
}

#endif
