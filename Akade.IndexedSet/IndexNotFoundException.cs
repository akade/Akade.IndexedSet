namespace Akade.IndexedSet;

/// <summary>
/// Exception that is raised if no index was found to support the query.
/// </summary>
public class IndexNotFoundException : Exception
{
    internal IndexNotFoundException(string? indexName) : base($"The index with name {indexName} has not been found. Index names are dependent on the passed key accessor. For lambdas, the convention is to always use x as a parameter i.e. x => x.IndexedProperty")
    {
    }

}