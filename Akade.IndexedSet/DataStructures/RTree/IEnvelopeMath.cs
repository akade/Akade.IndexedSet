using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

internal interface IEnvelopeMath<TPoint, TEnvelope, TValue>
    where TPoint : struct
    where TEnvelope : struct
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    static abstract TEnvelope Empty(int dimensions);

    static abstract void CopyTo(TEnvelope envelope, ref TEnvelope target);

    static abstract int GetDimensions(TEnvelope memoryEnvelope);

    static abstract TValue GetMin(TEnvelope envelope, int axis);
    static abstract TValue GetMax(TEnvelope envelope, int axis);

    static abstract TValue Area(TEnvelope envelope);
    static abstract TValue HalfPerimeter(TEnvelope envelope);
    static abstract TValue DistanceToBoundary(TEnvelope envelope, TPoint other);

    static abstract void CopyCenterTo(TEnvelope envelope, Span<TValue> center);

    static abstract bool Intersects(TEnvelope a, TEnvelope b);
    static abstract TValue IntersectionArea(TEnvelope a, TEnvelope b);

    static abstract bool Contains(TEnvelope a, TEnvelope b);


    static abstract void Merge(TEnvelope a, TEnvelope b, ref TEnvelope result);
    static abstract void MergeInto(TEnvelope a, ref TEnvelope b);

    static abstract TEnvelope Create(TPoint min, TPoint max);
    static abstract TEnvelope CreateFromPoint(TPoint point);

    static abstract string ToString(TEnvelope envelope);
}
