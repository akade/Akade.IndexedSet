using System.Numerics;

namespace Akade.IndexedSet.DataStructures.RTree;

internal struct Vector2Math : IEnvelopeMath<Vector2, VecRec2, float>
{
    public static float Area(ref VecRec2 envelope)
    {
        Vector2 size = envelope.Max - envelope.Min;
        return size.X * size.Y;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> completely contains <paramref name="b"/>.
    /// </summary>
    public static bool Contains(ref VecRec2 a, ref VecRec2 b)
    {
        (Vector2 minA, Vector2 maxA) = a;
        (Vector2 minB, Vector2 maxB) = b;
        return minA.X <= minB.X && maxA.X >= maxB.X &&
               minA.Y <= minB.Y && maxA.Y >= maxB.Y;
    }

    public static void CopyCenterTo(ref VecRec2 envelope, Span<float> center)
    {
        (Vector2 a, Vector2 b) = envelope;
        Vector2 size = (b - a) / 2;
        Vector2 centerPoint = a + size;

        center[0] = centerPoint.X;
        center[1] = centerPoint.Y;
    }

    public static void CopyTo(ref VecRec2 envelope, ref VecRec2 target)
    {
        target.Min = envelope.Min;
        target.Max = envelope.Max;
    }

    public static VecRec2 Create(Vector2 min, Vector2 max)
    {
        return new(min, max);
    }

    public static VecRec2 CreateFromPoint(Vector2 point)
    {
        return VecRec2.CreateFromPoint(point);
    }

    public static float DistanceToBoundarySquared(ref VecRec2 envelope, Vector2 other)
    {
        var closestPoint = Vector2.Clamp(other, envelope.Min, envelope.Max);
        return Vector2.DistanceSquared(closestPoint, other);
    }

    public static VecRec2 Empty(int dimensions)
    {
        return new VecRec2();
    }

    public static int GetDimensions(ref VecRec2 memoryEnvelope)
    {
        return 2;
    }

    public static float GetMax(ref VecRec2 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Max.X,
            1 => envelope.Max.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec2.")
        };
    }

    public static float GetMin(ref VecRec2 envelope, int axis)
    {
        return axis switch
        {
            0 => envelope.Min.X,
            1 => envelope.Min.Y,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be 0 or 1 for VecRec2.")
        };
    }

    public static float HalfPerimeter(ref VecRec2 envelope)
    {
        Vector2 size = envelope.Size;
        return size.X + size.Y;
    }

    public static float IntersectionArea(ref VecRec2 a, ref VecRec2 b)
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

    public static bool Intersects(ref VecRec2 a, ref VecRec2 b)
    {
        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X &&
               a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;
    }

    public static bool IsEmpty(ref VecRec2 memoryEnvelope)
    {
        return memoryEnvelope.Min == memoryEnvelope.Max;
    }

    public static void Merge(ref VecRec2 a, ref VecRec2 b, ref VecRec2 result)
    {
        result.Min = Vector2.Min(a.Min, b.Min);
        result.Max = Vector2.Max(a.Max, b.Max);
    }

    public static void MergeInto(ref VecRec2 a, ref VecRec2 b)
    {
        b.Min = Vector2.Min(b.Min, a.Min);
        b.Max = Vector2.Max(b.Max, a.Max);
    }

    public static string ToString(ref VecRec2 envelope)
    {
        return $"VecRec2(Min: {envelope.Min}, Max: {envelope.Max})";
    }
}

