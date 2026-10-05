using Akade.IndexedSet.Indices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Akade.IndexedSet.Serialization;

public static class IndexedSetSerializer
{
    private const int SerializationDataFormatVersion = 1;

    public static async ValueTask SerializeAsync<TElement>(this IndexedSet<TElement> indexedSet, ISerializationAdapter serializationAdapter, Stream target, CancellationToken cancellationToken = default)
        where TElement : notnull
    {
        IndexedSetSerializationContext<TElement> context = new(serializationAdapter, indexedSet.Count);

        using BinaryWriter writer = new(target, Encoding.UTF8, leaveOpen: true);

        Index<TElement>[] indices = indexedSet.GetIndices()
                                              .Where(x => x.SupportsSerialization)
                                              .ToArray();

        // Magic bytes and version
        writer.Write("Akade.IndexedSet"u8);
        writer.Write(SerializationDataFormatVersion);
        writer.Write(indexedSet.Count);
        writer.Write(indices.Length);

        using MemoryStream buffer = new();

        foreach (TElement element in indexedSet.FullScan())
        {
            _ = context.GetOrAddElementId(element);
            buffer.SetLength(0);
            // TODO: Test wether making this synchronous is faster
            await serializationAdapter.SerializeAsync(element, buffer, cancellationToken);

            if (buffer.Length > int.MaxValue)
            {
                throw new InvalidOperationException("Serialized element is too large.");
            }

            writer.Write((int)buffer.Length);
            buffer.Position = 0;
            await buffer.CopyToAsync(target, cancellationToken);
        }

        foreach (Index<TElement> index in indices)
        {
            writer.Write(index.IndexTypeNumber);
            writer.Write(index.Name);
            writer.Flush();
            await index.SerializeAsync(context, target, cancellationToken);
        }
    }

    public static async ValueTask DeserializeAsync<TElement>(this IndexedSet<TElement> indexedSet, ISerializationAdapter serializationAdapter, Stream source, CancellationToken cancellationToken = default)
        where TElement : notnull
    {
        if (indexedSet.Count > 0)
        {
            throw new ArgumentOutOfRangeException(nameof(indexedSet), "Deserialization can only target an empty IndexedSet.");
        }

        var indicesByName = indexedSet.GetIndices()
                                      .ToDictionary(x => x.Name);

        using BinaryReader reader = new(source, Encoding.UTF8, leaveOpen: true);
        VerifyHeader(reader, out int numberOfElements, out int numberOfIndices);

        IndexedSetDeserializationContext<TElement> context = new(serializationAdapter, numberOfElements);
        indexedSet.EnsureElementNumberWithoutIndices(numberOfElements);
        
        PartialReadOnlyStream elementStream = new(source);

        for (int i = 0; i < numberOfElements; i++)
        {
            int elementLength = reader.ReadInt32();
            elementStream.SetSegment(elementLength);

            TElement element = await serializationAdapter.DeserializeAsync<TElement>(elementStream, cancellationToken);
            if (!indexedSet.AddWithoutIndexing(element))
            {
                ThrowElementAlreadyPresent(element);
            }
            context.SetElementId(element, i);

            if (elementStream.Position != elementLength)
            {
                throw new InvalidOperationException($"Element deserializer consumed {elementStream.Position} instead of the expected {elementLength}.");
            }
        }

        for (int i = 0; i < numberOfIndices; i++)
        {
            int indexType = reader.ReadInt32();
            string indexName = reader.ReadString();

            if (!indicesByName.TryGetValue(indexName, out Index<TElement>? index))
            {
                ThrowInvalidIndexName(indexName);
            }

            if (index.IndexTypeNumber != indexType)
            {
                ThrowInvalidIndexType(index, indexType);
            }
            // TODO: How the f do we do this with multi key indices
            // Require stable ordering of returned keys from accessors? (Doesn't work, as this is not the order of how they appear in the index)
            // 
            await index.DeserializeAsync(context, source, cancellationToken);
            indicesByName.Remove(indexName);
        }

        foreach ((string name, _) in indicesByName)
        {
            indexedSet.FillIndexWithCurrentData(name);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowInvalidIndexType<TElement>(Index<TElement> index, int indexType)
        where TElement : notnull
    {
        throw new InvalidOperationException($"Expected index of type {index.GetType().Name}({index.IndexTypeNumber}) but found index of type number {indexType}. Did you change the index type between serialization & deserialization?");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowInvalidIndexName(string indexName)
    {
        throw new InvalidOperationException($"IndexedSet does not contain an index with the name \"{indexName}\". Did you change the indices between serialization & deserialization?");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowElementAlreadyPresent<TElement>(TElement element)
    {
        throw new InvalidOperationException($"The element {element} was already present. Did you change the equality contract between serialization & deserialization?");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void VerifyHeader(BinaryReader reader, out int numberOfElements, out int numberOfIndices)
    {
        Span<byte> bytes = stackalloc byte[28];
        reader.ReadExactly(bytes);

        if (!bytes[..16].SequenceEqual("Akade.IndexedSet"u8))
        {
            throw new InvalidOperationException("Expected magic bytes 'Akade.IndexedSet' but the found bytes did not match.");
        }

        int version = ReadInt(bytes[16..20]);
        if (version != SerializationDataFormatVersion)
        {
            throw new InvalidOperationException($"Invalid version: {version}. Expected version: {SerializationDataFormatVersion}.");
        }

        numberOfElements = ReadInt(bytes[20..24]);
        numberOfIndices = ReadInt(bytes[24..28]);
    }

    private static void VerifyVersion(BinaryReader reader)
    {
        
    }

    private static int ReadInt(Span<byte> bytes)
    {
        return MemoryMarshal.Cast<byte, int>(bytes)[0];
    }

#if !NET10_0_OR_GREATER
    private static void ReadExactly(this BinaryReader reader, Span<byte> bytes)
    {
        int bytesRead = 0;

        do
        {
            bytesRead += reader.Read(bytes[bytesRead..]);
        } while (bytesRead < bytes.Length);
    }
#endif
}
