using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

internal struct Vector3Math : IEnvelopeMath<Vector3, VecRec3, float>
{
    public static float Area(ref VecRec3 envelope)
    {
        Vector3 size = envelope.Max - envelope.Min;
        return size.X * size.Y * size.Z;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> completely contains <paramref name="b"/>.
    /// </summary>
    public static bool Contains(ref VecRec3 a, ref VecRec3 b)
    {
        (Vector3 minA, Vector3 maxA) = a;
        (Vector3 minB, Vector3 maxB) = b;
        return minA.X <= minB.X && maxA.X >= maxB.X && minA.Z <= minB.Z
            && minA.Y <= minB.Y && maxA.Y >= maxB.Y && maxA.Z >= maxB.Z;
    }

    public static void CopyCenterTo(ref VecRec3 envelope, Span<float> center)
    {
        (Vector3 a, Vector3 b) = envelope;
        Vector3 size = (b - a) / 2;
        Vector3 centerPoint = a + size;

        center[0] = centerPoint.X;
        center[1] = centerPoint.Y;
        center[2] = centerPoint.Z;
    }

    public static void CopyTo(ref VecRec3 envelope, ref VecRec3 target)
    {
        target.Min = envelope.Min;
        target.Max = envelope.Max;
    }

    public static VecRec3 Create(Vector3 min, Vector3 max)
    {
        return new(min, max);
    }

    public static VecRec3 CreateFromPoint(Vector3 point)
    {
        return VecRec3.CreateFromPoint(point);
    }

    public static float DistanceToBoundarySquared(ref VecRec3 envelope, Vector3 other)
    {
        var closestPoint = Vector3.Clamp(other, envelope.Min, envelope.Max);
        return Vector3.DistanceSquared(closestPoint, other);
    }

    public static VecRec3 Empty(int dimensions)
    {
        return new VecRec3();
    }

    public static int GetDimensions(ref VecRec3 memoryEnvelope)
    {
        return 3;
    }

    public static float GetMax(ref VecRec3 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Max.X,
            1 => envelope.Max.Y,
            2 => envelope.Max.Z,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec3.")
        };
    }

    public static float GetMin(ref VecRec3 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Min.X,
            1 => envelope.Min.Y,
            2 => envelope.Min.Z,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec3.")
        };
    }

    public static float HalfPerimeter(ref VecRec3 envelope)
    {
        Vector3 size = envelope.Size;
        return size.X + size.Y + size.Z;
    }

    public static float IntersectionArea(ref VecRec3 a, ref VecRec3 b)
    {
        (Vector3 minA, Vector3 maxA) = a;
        (Vector3 minB, Vector3 maxB) = b;

        var min = Vector3.Max(minA, minB);
        var max = Vector3.Min(maxA, maxB);

        if (min.X >= max.X || min.Y >= max.Y || min.Z >= max.Z)
        {
            return 0f; // No intersection
        }

        Vector3 size = max - min;
        return size.X * size.Y * size.Z;
    }

    public static bool Intersects(ref VecRec3 a, ref VecRec3 b)
    {
        (Vector3 minA, Vector3 maxA) = a;
        (Vector3 minB, Vector3 maxB) = b;

        var min = Vector3.Max(minA, minB);
        var max = Vector3.Min(maxA, maxB);

        return min.X <= max.X && min.Y <= max.Y && min.Z <= max.Z;
    }

    public static bool IntersectsWithoutBoundary(ref VecRec3 a, ref VecRec3 b)
    {
        (Vector3 minA, Vector3 maxA) = a;
        (Vector3 minB, Vector3 maxB) = b;

        var min = Vector3.Max(minA, minB);
        var max = Vector3.Min(maxA, maxB);

        return min.X < max.X && min.Y < max.Y && min.Z < max.Z;
    }

    public static bool IsEmpty(ref VecRec3 memoryEnvelope)
    {
        return memoryEnvelope.Min == memoryEnvelope.Max;
    }

    public static void Merge(ref VecRec3 a, ref VecRec3 b, ref VecRec3 result)
    {
        result.Min = Vector3.Min(a.Min, b.Min);
        result.Max = Vector3.Max(a.Max, b.Max);
    }

    public static void MergeInto(ref VecRec3 a, ref VecRec3 b)
    {
        b.Min = Vector3.Min(b.Min, a.Min);
        b.Max = Vector3.Max(b.Max, a.Max);
    }

    public static string ToString(ref VecRec3 envelope)
    {
        return $"VecRec3(Min: {envelope.Min}, Max: {envelope.Max})";
    }
}

