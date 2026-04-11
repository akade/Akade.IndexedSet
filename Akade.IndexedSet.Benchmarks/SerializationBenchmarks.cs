using Akade.IndexedSet.DataStructures.RTree;
using Akade.IndexedSet.Indices;
using Akade.IndexedSet.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Numerics;
using System.Text;

namespace Akade.IndexedSet.Benchmarks;

#pragma warning disable AkadeIndexedSetEXP0003

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
[JsonExporter]
public class SerializationBenchmarks
{
    private const int SerializationDataFormatVersion = 1;
    private static ReadOnlySpan<byte> MagicBytes => "Akade.IndexedSet"u8;
    private const int ElementCount = 10_000;

    private readonly ISerializationAdapter _serializer = new BinarySerializer();
    private byte[] _serializedData = [];

    [Params(
        UniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue,
        NonUniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue,
        RangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue,
        MultiRangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue,
        PrefixIndex<BenchmarkElement>.IndexTypeNumberValue,
        FullTextIndex<BenchmarkElement>.IndexTypeNumberValue,
        SpatialIndex<BenchmarkElement, Vector2, VecRec2, float, Vector2Math>.IndexTypeNumberValue,
        VectorIndex<BenchmarkElement>.IndexTypeNumberValue)]
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

        switch (IndexTypeNumber)
        {
            case UniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue:
                builder.WithUniqueIndex(x => x.Id);
                break;
            case NonUniqueIndex<BenchmarkElement, int>.IndexTypeNumberValue:
                builder.WithIndex(x => x.Category);
                break;
            case RangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue:
                builder.WithRangeIndex(x => x.Date);
                break;
            case MultiRangeIndex<BenchmarkElement, DateOnly>.IndexTypeNumberValue:
                builder.WithRangeIndex(x => x.Dates);
                break;
            case PrefixIndex<BenchmarkElement>.IndexTypeNumberValue:
                builder.WithPrefixIndex(x => x.Name);
                break;
            case FullTextIndex<BenchmarkElement>.IndexTypeNumberValue:
                builder.WithFullTextIndex(x => x.Description);
                break;
            case SpatialIndex<BenchmarkElement, Vector2, VecRec2, float, Vector2Math>.IndexTypeNumberValue:
                builder.WithSpatialIndex(x => x.Position);
                break;
            case VectorIndex<BenchmarkElement>.IndexTypeNumberValue:
                builder.WithVectorIndex(x => x.Vector);
                break;
            default:
                throw new InvalidOperationException($"Unsupported index type number: {IndexTypeNumber}");
        }

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

    private sealed class BinarySerializer : ISerializationAdapter
    {
        public ValueTask SerializeAsync<T>(T element, Stream stream, CancellationToken cancellationToken)
        {
            if (element is not BenchmarkElement benchmark)
            {
                throw new InvalidOperationException($"Unsupported serialization type {typeof(T).Name}.");
            }

            using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(benchmark.Id);
            writer.Write(benchmark.Category);
            writer.Write(benchmark.Date.DayNumber);
            writer.Write(benchmark.Dates.Length);
            foreach (DateOnly date in benchmark.Dates)
            {
                writer.Write(date.DayNumber);
            }

            writer.Write(benchmark.Name);
            writer.Write(benchmark.Description);
            writer.Write(benchmark.Position.X);
            writer.Write(benchmark.Position.Y);
            writer.Write(benchmark.Vector.Length);
            foreach (float value in benchmark.Vector)
            {
                writer.Write(value);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<TElement> DeserializeAsync<TElement>(Stream source, CancellationToken cancellationToken)
            where TElement : notnull
        {
            if (typeof(TElement) != typeof(BenchmarkElement))
            {
                throw new InvalidOperationException($"Unsupported serialization type {typeof(TElement).Name}.");
            }

            using BinaryReader reader = new(source, Encoding.UTF8, leaveOpen: true);
            int id = reader.ReadInt32();
            int category = reader.ReadInt32();
            DateOnly date = DateOnly.FromDayNumber(reader.ReadInt32());
            int datesCount = reader.ReadInt32();

            DateOnly[] dates = new DateOnly[datesCount];
            for (int i = 0; i < datesCount; i++)
            {
                dates[i] = DateOnly.FromDayNumber(reader.ReadInt32());
            }

            string name = reader.ReadString();
            string description = reader.ReadString();
            Vector2 position = new(reader.ReadSingle(), reader.ReadSingle());
            int vectorLength = reader.ReadInt32();

            float[] vector = new float[vectorLength];
            for (int i = 0; i < vectorLength; i++)
            {
                vector[i] = reader.ReadSingle();
            }

            BenchmarkElement element = new(id, category, date, dates, name, description, position, vector);
            return new ValueTask<TElement>((TElement)(object)element);
        }
    }
}
