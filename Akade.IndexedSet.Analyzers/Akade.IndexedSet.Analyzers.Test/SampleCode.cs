namespace Akade.IndexedSet.Analyzers.Test;
internal class SampleCode
{
    public static string IndexedSetBuilder = """
        using Akade.IndexedSet;

        IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                .WithRangeIndex({|#0:a|} => a)
                                                .WithFullTextIndex(({|#1:b|}) => b.ToString().ToLowerInvariant())
                                                .Build();
    """;

    public static string IndexedSet = """
        using Akade.IndexedSet;
    
        IndexedSet<int> test = null!;
    
        test.Range({|#0:a|} => a, 8, 22);
        test.Contains(({|#1:b|}) => b.ToString().ToLowerInvariant(), "10");
    """;

    public static string ConcurrentIndexedSet = """
        using Akade.IndexedSet.Concurrency;
    
        ConcurrentIndexedSet<int> test = null!;
    
        test.Range({|#0:a|} => a, 8, 22);
        test.Contains(({|#1:b|}) => b.ToString().ToLowerInvariant(), "10");
    """;

    public static string PrimaryKeyIndexedSetBuilder = """
        using Akade.IndexedSet;
    
        IndexedSet<int, int> test = new[]{5,10,20}.ToIndexedSet(x => x)
                                                  .WithRangeIndex({|#0:a|} => a)
                                                  .WithFullTextIndex(({|#1:b|}) => b.ToString().ToLowerInvariant())
                                                  .Build();
    """;

    public static string PrimaryKeyIndexedSet = """
        using Akade.IndexedSet;
    
        IndexedSet<int, int> test = null!;
    
        test.Range({|#0:a|} => a, 8, 22);
        test.Contains(({|#1:b|}) => b.ToString().ToLowerInvariant(), "10");
    """;

    public static string PrimaryKeyConcurrentIndexedSet = """
        using Akade.IndexedSet.Concurrency;
    
        ConcurrentIndexedSet<int, int> test = null!;
    
        test.Range({|#0:a|} => a, 8, 22);
        test.Contains(({|#1:b|}) => b.ToString().ToLowerInvariant(), "10");
    """;
}
