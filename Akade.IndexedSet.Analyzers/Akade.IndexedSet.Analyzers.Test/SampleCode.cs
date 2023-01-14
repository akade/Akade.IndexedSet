namespace Akade.IndexedSet.Analyzers.Test;
internal static class SampleCode
{
    private const string _buildSuffixx = """
        .WithIndex({|#0:x => { return x; }|})
        .WithRangeIndex({|#1:a|} => a)
        .WithFullTextIndex({|#2:(x)|} => x.ToString().ToLowerInvariant())
        .Build();
    """;

    private const string _setSuffixx = """
        test.Single({|#0:x => { return x; }|}, 22);
        test.Range({|#1:a|} => a, 8, 22);
        test.Contains({|#2:(x)|} => x.ToString().ToLowerInvariant(), "10");

    """;

    public const string IndexedSetBuilder = $$"""
        using Akade.IndexedSet;

        IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                              {{_buildSuffixx}}
    """;

    public const string IndexedSet = $"""
        using Akade.IndexedSet;
    
        IndexedSet<int> test = null!;
        {_setSuffixx}
    """;

    public const string ConcurrentIndexedSet = $"""
        using Akade.IndexedSet.Concurrency;
    
        ConcurrentIndexedSet<int> test = null!;
        {_setSuffixx}
    """;

    public const string PrimaryKeyIndexedSetBuilder = $$"""
        using Akade.IndexedSet;
    
        IndexedSet<int, int> test = new[]{5,10,20}.ToIndexedSet(x => x)
                                                  {{_buildSuffixx}}
    """;

    public const string PrimaryKeyIndexedSet = $"""
        using Akade.IndexedSet;
    
        IndexedSet<int, int> test = null!;
        {_setSuffixx}
    """;

    public const string PrimaryKeyConcurrentIndexedSet = $"""
        using Akade.IndexedSet.Concurrency;
    
        ConcurrentIndexedSet<int, int> test = null!;
        {_setSuffixx}
    """;
}
