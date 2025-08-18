using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;
internal struct Vector2Math : IEnvelopeMath<Vector2, VecRec2, float>
{
    public static float Area(VecRec2 envelope)
    {
        Vector2 size = envelope.Max - envelope.Min;
        return size.X * size.Y;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> completely contains <paramref name="b"/>.
    /// </summary>
    public static bool Contains(VecRec2 a, VecRec2 b)
    {
        (Vector2 minA, Vector2 maxA) = a;
        (Vector2 minB, Vector2 maxB) = b;
        return minA.X <= minB.X && maxA.X >= maxB.X &&
               minA.Y <= minB.Y && maxA.Y >= maxB.Y;
    }

    public static void CopyCenterTo(VecRec2 envelope, Span<float> center)
    {
        (Vector2 a, Vector2 b) = envelope;
        Vector2 size = (b - a) / 2;
        Vector2 centerPoint = a + size;

        center[0] = centerPoint.X;
        center[1] = centerPoint.Y;
    }

    public static void CopyTo(VecRec2 envelope, ref VecRec2 target)
    {
        target.Min = envelope.Min;
        target.Max = envelope.Max;
    }

    public static float DistanceToBoundary(VecRec2 envelope, Vector2 other)
    {
        (Vector2 min, Vector2 max) = envelope;

        var closestPoint = Vector2.Clamp(other, min, max);
        return Vector2.Distance(closestPoint, other);
    }

    public static VecRec2 Empty(int dimensions)
    {
        return new VecRec2();
    }

    public static int GetDimensions(VecRec2 memoryEnvelope)
    {
        return 2;
    }

    public static float GetMax(VecRec2 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Max.X,
            1 => envelope.Max.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec2.")
        };
    }

    public static float GetMin(VecRec2 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Min.X,
            1 => envelope.Min.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec2.")
        };
    }

    public static float HalfPerimeter(VecRec2 envelope)
    {
        Vector2 size = envelope.Size;
        return size.X + size.Y;
    }

    public static float IntersectionArea(VecRec2 a, VecRec2 b)
    {
        (Vector2 minA, Vector2 maxA) = a;
        (Vector2 minB, Vector2 maxB) = b;

        var min = Vector2.Max(minA, minB);
        var max = Vector2.Min(maxA, maxB);

        if (min.X >= max.X || min.Y >= max.Y)
        {
            return 0f; // No intersection
        }

        Vector2 size = max - min;
        return size.X * size.Y;
    }

    public static bool Intersects(VecRec2 a, VecRec2 b)
    {
        (Vector2 minA, Vector2 maxA) = a;
        (Vector2 minB, Vector2 maxB) = b;

        var min = Vector2.Max(minA, minB);
        var max = Vector2.Min(maxA, maxB);

        return min.X <= max.X && min.Y <= max.Y;
    }

    public static bool IsEmpty(VecRec2 memoryEnvelope)
    {
        return memoryEnvelope.Min == memoryEnvelope.Max;
    }

    public static void Merge(VecRec2 a, VecRec2 b, ref VecRec2 result)
    {
        result.Min = Vector2.Min(a.Min, b.Min);
        result.Max = Vector2.Max(a.Max, b.Max);
    }

    public static void MergeInto(VecRec2 a, ref VecRec2 b)
    {
        b.Min = Vector2.Min(b.Min, a.Min);
        b.Max = Vector2.Max(b.Max, a.Max);
    }

    public static string ToString(VecRec2 envelope)
    {
        return $"VecRec2(Min: {envelope.Min}, Max: {envelope.Max})";
    }
}

