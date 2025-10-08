using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

internal interface IEnvelopeMath<TPoint, TEnvelope, TValue>
    where TPoint : struct
    where TEnvelope : struct
    where TValue : unmanaged, INumber<TValue>, IMinMaxValue<TValue>, IRootFunctions<TValue>
{
    static abstract TEnvelope Empty(int dimensions);

    static abstract void CopyTo(ref TEnvelope envelope, ref TEnvelope target);

    static abstract int GetDimensions(ref TEnvelope memoryEnvelope);

    static abstract TValue GetMin(ref TEnvelope envelope, int axis);
    static abstract TValue GetMax(ref TEnvelope envelope, int axis);

    static abstract TValue Area(ref TEnvelope envelope);
    static abstract TValue HalfPerimeter(ref TEnvelope envelope);
    static abstract TValue DistanceToBoundarySquared(ref TEnvelope envelope, TPoint other);

    static abstract void CopyCenterTo(ref TEnvelope envelope, Span<TValue> center);

    static abstract bool Intersects(ref TEnvelope a, ref TEnvelope b);

    static abstract bool IntersectsWithoutBoundary(ref TEnvelope a, ref TEnvelope b);



    static abstract TValue IntersectionArea(ref TEnvelope a, ref TEnvelope b);

    static abstract bool Contains(ref TEnvelope a, ref TEnvelope b);


    static abstract void Merge(ref TEnvelope a, ref TEnvelope b, ref TEnvelope result);
    static abstract void MergeInto(ref TEnvelope a, ref TEnvelope b);

    static abstract TEnvelope Create(TPoint min, TPoint max);
    static abstract TEnvelope CreateFromPoint(TPoint point);

    static abstract string ToString(ref TEnvelope envelope);
}
