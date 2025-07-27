#if NET9_0_OR_GREATER
using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

internal interface IEnvelopeMath<TEnvelope, TValue, TMemoryEnvelope>
    where TEnvelope : allows ref struct
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    static abstract TMemoryEnvelope GetEmptyMemory();
    static abstract TMemoryEnvelope InitMemory(int dimensions);
    static abstract TEnvelope Init(int dimensions);

    static abstract TEnvelope AsEnvelope(TMemoryEnvelope memoryEnvelope);

    static abstract void CopyTo(TEnvelope envelope, TEnvelope target);
    static abstract void CopyTo(TEnvelope envelope, TMemoryEnvelope target);

    static abstract int GetDimensions(TEnvelope memoryEnvelope);
    static abstract int GetDimensions(TMemoryEnvelope memoryEnvelope);

    static abstract TValue GetMin(TEnvelope envelope, int axis);
    static abstract TValue GetMax(TEnvelope envelope, int axis);

    static abstract TValue Area(TEnvelope envelope);
    static abstract TValue HalfPerimeter(TEnvelope envelope);
    static abstract TValue DistanceToBoundary(TEnvelope envelope, ReadOnlySpan<TValue> point);

    static abstract void CopyCenterTo(TEnvelope envelope, Span<TValue> center);

    static abstract bool Intersects(TEnvelope a, TEnvelope b);
    static abstract TValue IntersectionArea(TEnvelope a, TEnvelope b);

    static abstract bool IsEmpty(TMemoryEnvelope memoryEnvelope);
    static abstract bool Contains(TEnvelope a, TEnvelope b);


    static abstract void Merge(TMemoryEnvelope a, TEnvelope b, TMemoryEnvelope result);

    static abstract void MergeInto(TEnvelope a, TEnvelope b);

    static abstract string ToString(TEnvelope envelope);
    static abstract string ToString(TMemoryEnvelope envelope);
}
#endif