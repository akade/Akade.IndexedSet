using System.Numerics;

#pragma warning disable IDE0130 // Namespace does not match folder structure, reason: we want this to be in the root namespace for easier consumption
namespace Akade.IndexedSet;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// 2D Rectangle using two <see cref="System.Numerics.Vector2"/>
/// </summary>
/// <param name="Min">The minimum corner of the rectangle.</param>
/// <param name="Max">The maximum corner of the rectangle.</param>
internal record struct VecRec2(Vector2 Min, Vector2 Max) 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VecRec2"/> struct with the specified minimum and maximum coordinates.
    /// </summary>
    public VecRec2(float minX, float minY, float maxX, float maxY) : this(new Vector2(minX, minY), new Vector2(maxX, maxY))
    {
    }

    internal readonly Vector2 Size => Max - Min;

    /// <summary>
    /// Creates a rectangle that contains exactly the point, i.e. <see cref="Min" /> = <see cref="Max" />
    /// </summary>
    public static VecRec2 CreateFromPoint(Vector2 point)
    {
        return new VecRec2(point, point);
    }
}

/// <summary>
/// 2D Rectangle using two <see cref="System.Numerics.Vector2"/>
/// </summary>
/// <param name="Min">The minimum corner of the rectangle.</param>
/// <param name="Max">The maximum corner of the rectangle.</param>
internal record struct VecRec3(Vector3 Min, Vector3 Max)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VecRec3"/> struct with the specified minimum and maximum coordinates.
    /// </summary>
    public VecRec3(float minX, float minY, float minZ, float maxX, float maxY, float maxZ) : this(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))
    {
    }

    internal readonly Vector3 Size => Max - Min;

    /// <summary>
    /// Creates a rectangle that contains exactly the point, i.e. <see cref="Min" /> = <see cref="Max" />
    /// </summary>
    public static VecRec3 CreateFromPoint(Vector3 point)
    {
        return new VecRec3(point, point);
    }
}
