#if NET9_0_OR_GREATER
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.DataStructures.RTree;

/// <summary>
/// Axis-aligned bounding box (AABB) for a generic unmanaged type T that supports numeric operations.
/// </summary>
internal readonly ref struct AABB<T>
    where T : unmanaged, INumber<T>, IMinMaxValue<T>, IRootFunctions<T>
{
    /// <summary>
    /// Lower (left) corner of the AABB.
    /// </summary>
    public ReadOnlySpan<T> Min { get; }

    /// <summary>
    /// Upper (right) corner of the AABB.
    /// </summary>
    public ReadOnlySpan<T> Max { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AABB{T}"/> struct with the specified minimum and maximum corners.
    /// </summary>
    public AABB(ReadOnlySpan<T> min, ReadOnlySpan<T> max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(min.Length, 1, nameof(min));
        ArgumentOutOfRangeException.ThrowIfNotEqual(min.Length, max.Length, nameof(min));

        Span<T> buffer = stackalloc T[min.Length];

        TensorPrimitives.Subtract(max, min, buffer);

        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
        {
            ThrowMaxMinException();
            return;
        }

        Min = min;
        Max = max;
    }

    public bool Contains(AABB<T> other)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(Min.Length, other.Min.Length, nameof(other));

        Span<T> buffer = stackalloc T[Min.Length];

        TensorPrimitives.Subtract(other.Min, Min, buffer);

        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
        {
            return false; // other.Min is less than Min in at least one dimension
        }

        TensorPrimitives.Subtract(Max, other.Max, buffer);

        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
        {
            return false; // other.Max is greater than Max in at least one dimension
        }

        return true;
    }

    /// <summary>
    /// Creates a new AABB from a single point, where both the minimum and maximum corners are the same.
    /// </summary>
    public static AABB<T> CreateFromPoint(ReadOnlySpan<T> point)
    {
        // Create a new AABB with the point as both min and max
        return new AABB<T>(point, point);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaxMinException()
    {
        throw new InvalidOperationException("Max must be greater than or equal to Min in all dimensions.");
    }

    internal T Area()
    {
        return TensorPrimitives.ProductOfDifferences(Max, Min);
    }

    internal void CopyTo(Span<T> destination)
    {
        Min.CopyTo(destination[..Min.Length]);
        Max.CopyTo(destination[Min.Length..]);
    }

    internal void MergeInto(Span<T> buffer)
    {
        Span<T> minBuffer = [.. buffer[..Min.Length]];
        Span<T> maxBuffer = [.. buffer[Min.Length..]];

        TensorPrimitives.Min(Min, minBuffer, buffer[..Min.Length]);
        TensorPrimitives.Max(Max, maxBuffer, buffer[Min.Length..]);
    }

    internal static AABB<T> CreateFromCombinedBuffer(Span<T> buffer)
    {
        Span<T> min = buffer[..(buffer.Length / 2)];
        Span<T> max = buffer[(buffer.Length / 2)..];
        return new AABB<T>(min, max);

    }

    internal T IntersectionArea(AABB<T> otherAABB)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(Min.Length, otherAABB.Min.Length, nameof(otherAABB));
        Span<T> intersectionMin = stackalloc T[Min.Length];
        Span<T> intersectionMax = stackalloc T[Min.Length];
        Span<T> diff = stackalloc T[Min.Length];
        TensorPrimitives.Max(Min, otherAABB.Min, intersectionMin);
        TensorPrimitives.Min(Max, otherAABB.Max, intersectionMax);

        TensorPrimitives.Subtract(intersectionMax, intersectionMin, diff);

        if (TensorPrimitives.Min<T>(diff) < T.Zero)
        {
            return T.Zero; // No intersection, return zero area
        }

        return TensorPrimitives.ProductOfDifferences<T>(intersectionMax, intersectionMin);
    }

    internal T HalfPerimeter()
    {
        Span<T> diff = stackalloc T[Min.Length];
        TensorPrimitives.Subtract(Max, Min, diff);
        return TensorPrimitives.Sum<T>(diff);
    }

    internal void Center(Span<T> center)
    {
        Span<T> diff = stackalloc T[Min.Length];
        TensorPrimitives.Subtract(Max, Min, diff);
        TensorPrimitives.Add(Min, diff, center);
    }

    internal bool Intersects(AABB<T> otherAABB)
    {
        Span<T> diff = stackalloc T[Min.Length];

        TensorPrimitives.Subtract(Min, otherAABB.Max, diff);

        if (TensorPrimitives.Max<T>(diff) > T.Zero)
        {
            return false;
        }

        TensorPrimitives.Subtract(Max, otherAABB.Min, diff);

        return TensorPrimitives.Min<T>(diff) >= T.Zero;

    }

    public bool Equals(AABB<T> other)
    {
        if (Min.Length != other.Min.Length)
        {
            return false;
        }
       
        return TensorPrimitives.Distance<T>(Min, other.Min) == T.Zero &&
               TensorPrimitives.Distance<T>(Max, other.Max) == T.Zero;
    }

    public override string ToString()
    {
        return $"AABB(Min: [{string.Join(", ", Min.ToArray())}], Max: [{string.Join(", ", Max.ToArray())}])";
    }
}

#endif