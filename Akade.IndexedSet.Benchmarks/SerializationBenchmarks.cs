using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Indices;
using Akade.IndexedSet.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Akade.IndexedSet.Benchmarks;


[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[JsonExporter]
public class SerializationBenchmarks
{
    private const int SerializationDataFormatVersion = 1;
    private static ReadOnlySpan<byte> MagicBytes => "Akade.IndexedSet"u8;
    private const int ElementCount = 10_000;

    private readonly ISerializationAdapter _serializer = new JsonSerializationAdapter();
    private byte[] _serializedData = [];

    [Params(
        //UniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue
        //NonUniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue
        RangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue
        //MultiRangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue,
        //PrefixIndex<BenchmarkElement>.IndexTypeNumberValue,
        //FullTextIndex<BenchmarkElement>.IndexTypeNumberValue,
        //SpatialIndex<BenchmarkElement, Vector2, VecRec2, float, Vector2Math>.IndexTypeNumberValue,
        //VectorIndex<BenchmarkElement>.IndexTypeNumberValue
        )]
    public int IndexTypeNumber { get; set; }

    [GlobalSetup]
    public async Task SetupAsync()
    {
        List<BenchmarkElement> elements = CreateElements(ElementCount);
        IndexedSet<BenchmarkElement> indexedSet = CreateIndexedSet(elements);

        using MemoryStream stream = new();
        await indexedSet.SerializeAsync(_serializer, stream);
        _serializedData = stream.ToArray();
    }
#pragma warning restore AkadeIndexedSetEXP0003

    [Benchmark]
    public async Task Deserialize_IndexedSet()
    {
        IndexedSet<BenchmarkElement> target = CreateIndexedSet();
        using MemoryStream stream = new(_serializedData);
        await target.DeserializeAsync(_serializer, stream);
    }

    [Benchmark]
    public async Task Deserialize_Data_And_AddRange()
    {
        using MemoryStream stream = new(_serializedData);
        List<BenchmarkElement> elements = await DeserializeElementsAsync(stream);

        IndexedSet<BenchmarkElement> target = CreateIndexedSet();
        _ = target.AddRange(elements);
    }

    private static List<BenchmarkElement> CreateElements(int count)
    {
        Random random = new(42);
        List<BenchmarkElement> elements = new(count);
        DateOnly start = new(2020, 1, 1);

        for (int i = 0; i < count; i++)
        {
            DateOnly date = start.AddDays(random.Next(0, 3650));
            DateOnly[] dates = [date, date.AddDays(1), date.AddDays(2)];

            Vector2 position = new(random.NextSingle() * 100, random.NextSingle() * 100);
            float[] vector = [
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle()
            ];

            elements.Add(new BenchmarkElement(
                i,
                random.Next(0, 250),
                date,
                dates,
                $"Item-{i}",
                $"Serialized benchmark item {i}",
                position,
                vector));
        }

        return elements;
    }

    private IndexedSet<BenchmarkElement> CreateIndexedSet(IEnumerable<BenchmarkElement>? elements = null)
    {
        IndexedSetBuilder<BenchmarkElement> builder = elements is null
            ? IndexedSetBuilder<BenchmarkElement>.Create()
            : IndexedSetBuilder.Create(elements);

        _ = IndexTypeNumber switch
        {
            UniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue => builder.WithUniqueIndex(x => x.Id),
            NonUniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue => builder.WithIndex(x => x.Category),
            RangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue => builder.WithRangeIndex(x => x.Date),
            MultiRangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue => builder.WithRangeIndex(x => x.Dates),
            PrefixIndex<BenchmarkElement>.IndexTypeNumberValue => builder.WithPrefixIndex(x => x.Name),
            FullTextIndex<BenchmarkElement>.IndexTypeNumberValue => builder.WithFullTextIndex(x => x.Description),
            SpatialIndex<BenchmarkElement, Vector2, VecRec2, float, Vector2Math>.IndexTypeNumberValue => builder.WithSpatialIndex(x => x.Position),
#pragma warning disable AkadeIndexedSetEXP0003
            VectorIndex<BenchmarkElement>.IndexTypeNumberValue => builder.WithVectorIndex(x => x.Vector),
            _ => throw new InvalidOperationException($"Unsupported index type number: {IndexTypeNumber}")
        };

        return builder.Build();
    }

    private async Task<List<BenchmarkElement>> DeserializeElementsAsync(Stream source)
    {
        using BinaryReader reader = new(source, Encoding.UTF8, leaveOpen: true);
        VerifyMagicBytes(reader);
        VerifyVersion(reader);

        int numberOfElements = reader.ReadInt32();
        _ = reader.ReadInt32();

        PartialReadOnlyStream elementStream = new(source);
        List<BenchmarkElement> elements = new(numberOfElements);

        for (int i = 0; i < numberOfElements; i++)
        {
            int elementLength = reader.ReadInt32();
            elementStream.SetSegment(elementLength);

            BenchmarkElement element = await _serializer.DeserializeAsync<BenchmarkElement>(elementStream, CancellationToken.None);
            elements.Add(element);

            if (elementStream.Position != elementLength)
            {
                throw new InvalidOperationException($"Element deserializer consumed {elementStream.Position} instead of the expected {elementLength}.");
            }
        }

        return elements;
    }

    private static void VerifyVersion(BinaryReader reader)
    {
        int version = reader.ReadInt32();
        if (version != SerializationDataFormatVersion)
        {
            throw new InvalidOperationException($"Invalid version: {version}. Expected version: {SerializationDataFormatVersion}.");
        }
    }

    private static void VerifyMagicBytes(BinaryReader reader)
    {
        Span<byte> bytes = stackalloc byte[MagicBytes.Length];
        ReadExactly(reader, bytes);

        if (!bytes.SequenceEqual(MagicBytes))
        {
            throw new InvalidOperationException("Expected magic bytes 'Akade.IndexedSet' but the found bytes did not match.");
        }
    }

    private static void ReadExactly(BinaryReader reader, Span<byte> bytes)
    {
        int bytesRead = 0;

        while (bytesRead < bytes.Length)
        {
            int read = reader.Read(bytes[bytesRead..]);
            if (read == 0)
            {
                throw new EndOfStreamException();
            }

            bytesRead += read;
        }
    }

    private record BenchmarkElement(
        int Id,
        int Category,
        DateOnly Date,
        DateOnly[] Dates,
        string Name,
        string Description,
        Vector2 Position,
        float[] Vector);

    private sealed class JsonSerializationAdapter : ISerializationAdapter
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.General);

        public async ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken)
        {
            if (element is not BenchmarkElement benchmark)
            {
                throw new InvalidOperationException($"Unsupported serialization type {typeof(T).Name}.");
            }

            await JsonSerializer.SerializeAsync(stream, element, _jsonSerializerOptions, cancellationToken);
        }

        public async ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
            where TElement : notnull
        {
            if (typeof(TElement) != typeof(BenchmarkElement))
            {
                throw new InvalidOperationException($"Unsupported serialization type {typeof(TElement).Name}.");
            }
            return await JsonSerializer.DeserializeAsync<TElement>(source, _jsonSerializerOptions, cancellationToken)
                ?? throw new InvalidOperationException($"Failed to deserialize {typeof(TElement).Name}.");

        }
    }
}
