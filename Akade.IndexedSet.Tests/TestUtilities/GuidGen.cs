namespace Akade.IndexedSet.Tests.TestUtilities;

internal class GuidGen
{
    public static Guid Get(int id)
    {
        return new Guid(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}
