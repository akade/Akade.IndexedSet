//#if NET9_0_OR_GREATER
//using System.Numerics;
//using System.Numerics.Tensors;
//using System.Runtime.CompilerServices;

//namespace Akade.IndexedSet.DataStructures.RTree;


//internal struct SpanEnvelopeMath<T> : IEnvelopeMath<Span<T>, T, Memory<T>>
//    where T : unmanaged, INumber<T>, IMinMaxValue<T>, IRootFunctions<T>
//{
//    public static T Area(Span<T> envelope)
//    {
//        int dimensions = envelope.Length / 2;

//        ReadOnlySpan<T> min = envelope[..dimensions];
//        ReadOnlySpan<T> max = envelope[dimensions..];

//        return TensorPrimitives.ProductOfDifferences(min, max);
//    }

//    public static Span<T> AsEnvelope(Memory<T> memoryEnvelope)
//    {
//        return memoryEnvelope.Span;
//    }

//    public static bool Contains(Span<T> a, Span<T> b)
//    {
//        ArgumentOutOfRangeException.ThrowIfNotEqual(a.Length, b.Length, nameof(b));
//        int dimensions = a.Length / 2;

//        ReadOnlySpan<T> aMin = a[..dimensions];
//        ReadOnlySpan<T> aMax = a[dimensions..];

//        ReadOnlySpan<T> bMin = b[..dimensions];
//        ReadOnlySpan<T> bMax = b[dimensions..];

//        Span<T> buffer = stackalloc T[dimensions];

//        TensorPrimitives.Subtract(bMin, aMin, buffer);

//        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
//        {
//            return false; // bMin is less than Min in at least one dimension
//        }

//        TensorPrimitives.Subtract(aMax, bMax, buffer);

//        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
//        {
//            return false; // bMax is greater than Max in at least one dimension
//        }

//        return true;
//    }

//    public static void CopyCenterTo(Span<T> envelope, Span<T> center)
//    {
//        int dimensions = envelope.Length / 2;

//        ReadOnlySpan<T> min = envelope[..dimensions];
//        ReadOnlySpan<T> max = envelope[dimensions..];

//        Span<T> diff = stackalloc T[dimensions];
//        TensorPrimitives.Subtract(max, min, diff);
//        TensorPrimitives.Add(min, diff, center);
//    }

//    public static void CopyTo(Span<T> envelope, ref Span<T> target)
//    {
//        envelope.CopyTo(target);
//    }

//    public static void CopyTo(Span<T> envelope, ref Memory<T> target)
//    {
//        envelope.CopyTo(target.Span);
//    }

//    public static T DistanceToBoundary(Span<T> envelope, ReadOnlySpan<T> point)
//    {
//        int dimensions = envelope.Length / 2;

//        ReadOnlySpan<T> min = envelope[..dimensions];
//        ReadOnlySpan<T> max = envelope[dimensions..];

//        Span<T> buffer = stackalloc T[min.Length];
//        Span<T> closestPoint = stackalloc T[min.Length];

//        // clamp point to the AABB bounds
//        TensorPrimitives.Max(min, point, buffer);
//        TensorPrimitives.Min(max, buffer, closestPoint);

//        return TensorPrimitives.Distance(closestPoint, point);
//    }

//    public static int GetDimensions(Span<T> memoryEnvelope)
//    {
//        return memoryEnvelope.Length / 2;
//    }

//    public static int GetDimensions(Memory<T> memoryEnvelope)
//    {
//        return memoryEnvelope.Length / 2;
//    }

//    public static Memory<T> GetEmptyMemory()
//    {
//        return Memory<T>.Empty;
//    }

//    public static T GetMax(Span<T> envelope, int axis)
//    {
//        int dimensions = envelope.Length / 2;
//        return envelope[dimensions + axis];
//    }

//    public static T GetMin(Span<T> envelope, int axis)
//    {
//        return envelope[axis];
//    }

//    public static T HalfPerimeter(Span<T> envelope)
//    {
//        int dimensions = envelope.Length / 2;

//        ReadOnlySpan<T> min = envelope[..dimensions];
//        ReadOnlySpan<T> max = envelope[dimensions..];

//        Span<T> diff = stackalloc T[dimensions];
//        TensorPrimitives.Subtract(max, min, diff);
//        return TensorPrimitives.Sum<T>(diff);
//    }

//    public static Span<T> Init(int dimensions)
//    {
//        // TODO: Can this be somehow done with stackalloc
//        return new T[dimensions * 2];
//    }

//    public static Memory<T> InitMemory(int dimensions)
//    {
//        return new Memory<T>(new T[dimensions * 2]);
//    }

//    public static T IntersectionArea(Span<T> a, Span<T> b)
//    {
//        int dimensions = a.Length / 2;

//        ReadOnlySpan<T> aMin = a[..dimensions];
//        ReadOnlySpan<T> aMax = a[dimensions..];

//        ReadOnlySpan<T> bMin = b[..dimensions];
//        ReadOnlySpan<T> bMax = b[dimensions..];

//        Span<T> intersectionMin = stackalloc T[dimensions];
//        Span<T> intersectionMax = stackalloc T[dimensions];
//        Span<T> diff = stackalloc T[dimensions];

//        TensorPrimitives.Max(aMin, bMin, intersectionMin);
//        TensorPrimitives.Min(aMax, bMax, intersectionMax);

//        TensorPrimitives.Subtract(intersectionMax, intersectionMin, diff);

//        if (TensorPrimitives.Min<T>(diff) < T.Zero)
//        {
//            return T.Zero; // No intersection, return zero area
//        }

//        return TensorPrimitives.ProductOfDifferences<T>(intersectionMax, intersectionMin);
//    }

//    public static bool Intersects(Span<T> a, Span<T> b)
//    {
//        int dimensions = a.Length / 2;

//        ReadOnlySpan<T> aMin = a[..dimensions];
//        ReadOnlySpan<T> aMax = a[dimensions..];

//        ReadOnlySpan<T> bMin = b[..dimensions];
//        ReadOnlySpan<T> bMax = b[dimensions..];

//        Span<T> diff = stackalloc T[dimensions];

//        TensorPrimitives.Subtract(aMin, bMax, diff);

//        if (TensorPrimitives.Max<T>(diff) > T.Zero)
//        {
//            return false;
//        }

//        TensorPrimitives.Subtract(aMax, bMin, diff);

//        return TensorPrimitives.Min<T>(diff) >= T.Zero;
//    }

//    public static bool IsEmpty(Memory<T> memoryEnvelope)
//    {
//        throw new NotImplementedException();
//    }

//    public static void Merge(Memory<T> a, Span<T> b, Memory<T> result)
//    {
//        throw new NotImplementedException();
//    }

//    public static void MergeInto(Span<T> a, Span<T> b)
//    {
//        throw new NotImplementedException();
//    }

//    public static string ToString(Span<T> envelope)
//    {
//        throw new NotImplementedException();
//    }

//    public static string ToString(Memory<T> envelope)
//    {
//        throw new NotImplementedException();
//    }
//}


///// <summary>
///// Axis-aligned bounding box (AABB) for a generic unmanaged type T that supports numeric operations.
///// </summary>
//internal readonly ref struct AABB<T>
//    where T : unmanaged, INumber<T>, IMinMaxValue<T>, IRootFunctions<T>
//{
//    /// <summary>
//    /// Lower (left) corner of the AABB.
//    /// </summary>
//    public ReadOnlySpan<T> Min { get; }

//    /// <summary>
//    /// Upper (right) corner of the AABB.
//    /// </summary>
//    public ReadOnlySpan<T> Max { get; }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="AABB{T}"/> struct with the specified minimum and maximum corners.
//    /// </summary>
//    public AABB(ReadOnlySpan<T> min, ReadOnlySpan<T> max)
//    {
//        ArgumentOutOfRangeException.ThrowIfLessThan(min.Length, 1, nameof(min));
//        ArgumentOutOfRangeException.ThrowIfNotEqual(min.Length, max.Length, nameof(min));

//        Span<T> buffer = stackalloc T[min.Length];

//        TensorPrimitives.Subtract(max, min, buffer);

//        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
//        {
//            ThrowMaxMinException();
//            return;
//        }

//        Min = min;
//        Max = max;
//    }

//    /// <summary>
//    /// Creates a new AABB from a single point, where both the minimum and maximum corners are the same.
//    /// </summary>
//    public static AABB<T> CreateFromPoint(ReadOnlySpan<T> point)
//    {
//        // Create a new AABB with the point as both min and max
//        return new AABB<T>(point, point);
//    }

//    [MethodImpl(MethodImplOptions.NoInlining)]
//    private static void ThrowMaxMinException()
//    {
//        throw new InvalidOperationException("Max must be greater than or equal to Min in all dimensions.");
//    }

//    internal void MergeInto(Span<T> buffer)
//    {
//        Span<T> minBuffer = [.. buffer[..Min.Length]];
//        Span<T> maxBuffer = [.. buffer[Min.Length..]];

//        TensorPrimitives.Min(Min, minBuffer, buffer[..Min.Length]);
//        TensorPrimitives.Max(Max, maxBuffer, buffer[Min.Length..]);
//    }

//    internal static AABB<T> CreateFromCombinedBuffer(Span<T> buffer)
//    {
//        Span<T> min = buffer[..(buffer.Length / 2)];
//        Span<T> max = buffer[(buffer.Length / 2)..];
//        return new AABB<T>(min, max);

//    }



//    internal bool Intersects(AABB<T> otherAABB)
//    {

//    }

//    public bool Equals(AABB<T> other)
//    {
//        if (Min.Length != other.Min.Length)
//        {
//            return false;
//        }

//        return TensorPrimitives.Distance<T>(Min, other.Min) == T.Zero &&
//               TensorPrimitives.Distance<T>(Max, other.Max) == T.Zero;
//    }

//    public override string ToString()
//    {
//        return $"AABB(Min: [{string.Join(", ", Min.ToArray())}], Max: [{string.Join(", ", Max.ToArray())}])";
//    }

//    internal bool Contains(ReadOnlySpan<T> position)
//    {
//        Span<T> buffer = stackalloc T[Min.Length];

//        TensorPrimitives.Subtract(position, Min, buffer);

//        if (TensorPrimitives.Min<T>(buffer) < T.Zero)
//        {
//            return false; // position is less than Min in at least one dimension
//        }

//        TensorPrimitives.Subtract(Max, position, buffer);

//        return TensorPrimitives.Min<T>(buffer) >= T.Zero; // position is greater than Max in at least one dimension
//    }


//}

//#endif