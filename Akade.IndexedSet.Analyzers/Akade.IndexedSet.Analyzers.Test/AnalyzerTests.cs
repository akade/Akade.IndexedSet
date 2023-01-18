using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class AkadeIndexedSetAnalyzersUnitTest
{
    [TestMethod]
    public async Task Hello_world_gives_no_diagnostics()
    {
        string test = """
               System.Console.WriteLine("Hello World");
               """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [DataTestMethod]
    [DataRow(SampleCode.ConcurrentIndexedSet, DisplayName = nameof(SampleCode.ConcurrentIndexedSet))]
    [DataRow(SampleCode.IndexedSet, DisplayName = nameof(SampleCode.IndexedSet))]
    [DataRow(SampleCode.IndexedSetBuilder, DisplayName = nameof(SampleCode.IndexedSetBuilder))]
    [DataRow(SampleCode.PrimaryKeyConcurrentIndexedSet, DisplayName = nameof(SampleCode.PrimaryKeyConcurrentIndexedSet))]
    [DataRow(SampleCode.PrimaryKeyIndexedSet, DisplayName = nameof(SampleCode.PrimaryKeyIndexedSet))]
    [DataRow(SampleCode.PrimaryKeyIndexedSetBuilder, DisplayName = nameof(SampleCode.PrimaryKeyIndexedSetBuilder))]
    public async Task All_relevant_types_are_supported_and_reported(string input)
    {
        DiagnosticResult blockBody = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId).WithLocation(0);
        DiagnosticResult name = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId).WithLocation(1).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId).WithLocation(2);
        await VerifyCS.VerifyAnalyzerAsync(input, blockBody, name, parantherizedLambda);
    }
}
