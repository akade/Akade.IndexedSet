using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;
[assembly: TestDataSourceDiscovery(TestDataSourceDiscoveryOption.DuringExecution)]

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class AnalyzerAndCodeFixTests
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
    [DynamicData(nameof(AnalyzerTestCases), DynamicDataSourceType.Property)]
    public async Task Analyzer_reports_diagnostics(string input, DiagnosticResult diagnostic)
    {
        await VerifyCS.VerifyAnalyzerAsync(input, diagnostic);
    }

    [DataTestMethod]
    [DynamicData(nameof(CodeFixTestCases), DynamicDataSourceType.Property)]
    public async Task Codefixes_work(string input, string fixedCode)
    {
        await VerifyCS.VerifyCodeFixAsync(input, fixedCode);
    }

    public static IEnumerable<object[]> AnalyzerTestCases { get; }
        = SampleCodeGenerator.GetAllSamples().Select(tuple => new object[] { tuple.incorrectCode, tuple.diagnostic }).ToArray();

    public static IEnumerable<object[]> CodeFixTestCases { get; }
    = SampleCodeGenerator.GetAllSamples()
                         .Select(tuple => new object[] { SampleCodeGenerator.ReplaceIndexWithId(tuple.incorrectCode, tuple.diagnostic), tuple.fixedCode }).ToArray();

}
