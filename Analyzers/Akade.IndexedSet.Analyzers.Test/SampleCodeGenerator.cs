using Microsoft.CodeAnalysis.Testing;
using System.Collections.Generic;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;
internal static class SampleCodeGenerator
{
    private static readonly string[] _setTypes =
    [
        "IndexedSet<int>",
        "IndexedSet<int, int>",
        "ConcurrentIndexedSet<int>",
        "ConcurrentIndexedSet<int, int>"
    ];

    private static readonly string[] _setIncorrectMethods =
    [
        "Single({|#0:a => a|}, 1)",
        "Where({|#0:(x) => x|}, 2)",
        "StartsWith({|#0:x => { return x.ToString().ToLowerInvariant(); }|}, \"Test\")",
    ];

    private static readonly string[] _setFixedMethods =
    [
        "Single(x => x, 1)",
        "Where(x => x, 2)",
        "StartsWith(x => x.ToString().ToLowerInvariant(), \"Test\")",
    ];

    private static readonly string[] _builderIncorrectMethods =
    [
       "WithIndex({|#0:a => a|})",
       "WithRangeIndex({|#0:(x) => x|})",
       "WithFullTextIndex({|#0:x => { return x.ToString().ToLowerInvariant(); }|})",
    ];

    private static readonly string[] _builderFixedMethods =
    [
       "WithIndex(x => x)",
       "WithRangeIndex(x => x)",
       "WithFullTextIndex(x => x.ToString().ToLowerInvariant())",
    ];

    private static readonly string[] _builderTypes =
    [
         "ToIndexedSet()",
         "ToIndexedSet(x => x)"
    ];

    private static readonly DiagnosticResult[] _diagnosticIds =
    [
        VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId).WithLocation(0).WithArguments("a"),
        VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId).WithLocation(0),
        VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId).WithLocation(0),
    ];

    public static IEnumerable<(string incorrectCode, DiagnosticResult diagnostic, string fixedCode)> GetAllSamples()
    {
        for (int i = 0; i < _setTypes.Length; i++)
        {
            for (int j = 0; j < _setIncorrectMethods.Length; j++)
            {
                string incorrect = CreateSetSample(_setTypes[i], _setIncorrectMethods[j]);
                string @fixed = CreateSetSample(_setTypes[i], _setFixedMethods[j]);

                yield return (incorrect, _diagnosticIds[j], @fixed);
            }
        }

        for (int i = 0; i < _builderTypes.Length; i++)
        {
            for (int j = 0; j < _builderIncorrectMethods.Length; j++)
            {
                string incorrect = CreateBuilderSample(_builderTypes[i], _builderIncorrectMethods[j]);
                string @fixed = CreateBuilderSample(_builderTypes[i], _builderFixedMethods[j]);

                yield return (incorrect, _diagnosticIds[j], @fixed);
            }
        }
    }

    public static string ReplaceIndexWithId(string code, DiagnosticResult diagnostic)
    {
        return code.Replace("#0", diagnostic.Id);
    }

    private static string CreateSetSample(string setType, string method)
    {
        return $"""
            using Akade.IndexedSet;
            using Akade.IndexedSet.Concurrency;

            {setType} test = null!;
            test.{method};
            """;

    }

    private static string CreateBuilderSample(string builderMethod, string method)
    {
        return $$"""
        using System;
        using Akade.IndexedSet;
        using Akade.IndexedSet.Concurrency;

        var test = new int[]{1,2}.{{builderMethod}}.{{method}}.Build();
        """;

    }
}
