using Akade.IndexedSet.Serialization;
using System.Text;
using System.Text.Json;

namespace Akade.IndexedSet.Tests.Serialization;

[TestClass]
public class SerializationTests
{
    private static readonly STJSerializer _serializer = new();

    [TestMethod]
    public async Task Round_tripping_preserves_count()
    {
        IndexedSet<TestElement> set = await RoundTripAsync(CreateSampleSet());
        Assert.AreEqual(3, set.Count);
    }

    [TestMethod]
    public async Task Round_tripping_preserves_unique_index_lookup()
    {
        IndexedSet<TestElement> set = await RoundTripAsync(CreateSampleSet());

        _ = set.TryGetSingle(x => x.Id, 2, out TestElement? element);

        Assert.IsNotNull(element);
        Assert.AreEqual("Two", element.Name);
    }

    [TestMethod]
    public async Task Round_tripping_preserves_value_index_lookup()
    {
        IndexedSet<TestElement> set = await RoundTripAsync(CreateSampleSet());

        _ = set.TryGetSingle(x => x.Value, 3.0, out TestElement? element);

        Assert.IsNotNull(element);
        Assert.AreEqual(3, element.Id);
    }

    [TestMethod]
    public async Task Round_tripping_empty_set_preserves_count()
    {
        IndexedSet<TestElement> set = await RoundTripAsync(CreateSet([]));

        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public async Task Deserialization_throws_when_not_all_element_bytes_are_consumed()
    {
        IndexedSet<TestElement> set = CreateSampleSet();

        using MemoryStream stream = await SerializeToStreamAsync(set, _serializer);

        set = CreateSet([]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            set.DeserializeAsync(new ShortReadSerializer(), stream, TestContext.CancellationToken).AsTask());
    }

    [TestMethod]
    public async Task Deserialization_throws_when_stream_is_truncated()
    {
        IndexedSet<TestElement> set = CreateSampleSet();

        using MemoryStream stream = await SerializeToStreamAsync(set, _serializer);
        stream.SetLength(stream.Length - 1);
        stream.Position = 0;

        IndexedSet<TestElement> target = CreateSet([]);

        await Assert.ThrowsAsync<Exception>(() =>
            target.DeserializeAsync(_serializer, stream, TestContext.CancellationToken).AsTask());
    }

    [TestMethod]
    public async Task Deserialization_throws_when_magic_bytes_are_invalid()
    {
        using MemoryStream stream = await SerializeToStreamAsync(CreateSampleSet(), _serializer);
        stream.Position = 0;
        stream.WriteByte(0);
        stream.Position = 0;

        IndexedSet<TestElement> target = CreateSet([]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            target.DeserializeAsync(_serializer, stream, TestContext.CancellationToken).AsTask());
    }

    [TestMethod]
    public async Task Deserialization_throws_when_version_is_invalid()
    {
        using MemoryStream stream = await SerializeToStreamAsync(CreateSampleSet(), _serializer);
        stream.Position = 16;
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(999);
        }

        stream.Position = 0;

        IndexedSet<TestElement> target = CreateSet([]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            target.DeserializeAsync(_serializer, stream, TestContext.CancellationToken).AsTask());
    }

    private record TestElement(int Id, string Name, double Value);

    private static IndexedSet<TestElement> CreateSampleSet()
    {
        TestElement[] elements = [
            new(1, "One", 1.0),
            new(2, "Two", 1.0),
            new(3, "Three", 3.0)
        ];

        return CreateSet(elements);
    }

    private static IndexedSet<TestElement> CreateSet(IEnumerable<TestElement> elements)
    {
        return elements.ToIndexedSet()
                       .WithUniqueIndex(x => x.Id)
                       .WithFullTextIndex(x => x.Name)
                       .WithIndex(x => x.Value)
                       .Build();
    }


    private async Task<IndexedSet<TestElement>> RoundTripAsync(IndexedSet<TestElement> set)
    {
        using MemoryStream stream = await SerializeToStreamAsync(set, _serializer);

        set = CreateSet([]);
        await set.DeserializeAsync(_serializer, stream, TestContext.CancellationToken);

        return set;
    }

    private async Task<MemoryStream> SerializeToStreamAsync(IndexedSet<TestElement> set, ISerializationAdapter serializer)
    {
        MemoryStream stream = new();
        await set.SerializeAsync(serializer, stream, TestContext.CancellationToken);
        stream.Position = 0;
        return stream;
    }

    private class STJSerializer : ISerializationAdapter
    {
        public async ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken)
        {
            await JsonSerializer.SerializeAsync(stream, element, cancellationToken: cancellationToken);
        }

        public async ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
            where TElement : notnull
        {
            return await JsonSerializer.DeserializeAsync<TElement>(source, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Deserialization of {typeof(TElement).Name} resulted in null.");
        }
    }

    private class ShortReadSerializer : ISerializationAdapter
    {
        public async ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken)
        {
            await JsonSerializer.SerializeAsync(stream, element, cancellationToken: cancellationToken);
        }

        public async ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
            where TElement : notnull
        {
            byte[] buffer = new byte[1];
            _ = await source.ReadAsync(buffer, cancellationToken);
            return (TElement)(object)new TestElement(0, string.Empty, 0);
        }
    }

    public TestContext TestContext { get; set; }
}
