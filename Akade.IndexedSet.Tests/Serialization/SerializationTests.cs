using Akade.IndexedSet.Serialization;
using System.Text;
using System.Text.Json;

namespace Akade.IndexedSet.Tests.Serialization;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public async Task TestSerialization()
    {
        IndexedSet<TestElement> set =
       new TestElement[] {
            new(1, "One", 1.0),
            new(2, "Two", 1.0),
            new(3, "Three", 3.0)
        }.ToIndexedSet()
         .WithUniqueIndex(x => x.Id)
         .WithFullTextIndex(x => x.Name)
         .WithIndex(x => x.Value)
         .Build();
        
        using MemoryStream stream = new();
        await set.SerializeAsync(new STJSerializer(), stream, TestContext.CancellationToken);

        // reset 
        set.Clear();
        stream.Position = 0;

        await set.DeserializeAsync(new STJSerializer(), stream, TestContext.CancellationToken);

        Assert.AreEqual(3, set.Count);
    }

    private record TestElement(int Id, string Name, double Value);

    private class STJSerializer : ISerializationAdapter
    {
        public async ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken)
        {
            await JsonSerializer.SerializeAsync(stream, element, cancellationToken: cancellationToken);
        }

        public async ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
        {
            return await JsonSerializer.DeserializeAsync<TElement>(source, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Deserialization of {typeof(TElement).Name} resulted in null.");
        }
    }

    public TestContext TestContext { get; set; }
}
