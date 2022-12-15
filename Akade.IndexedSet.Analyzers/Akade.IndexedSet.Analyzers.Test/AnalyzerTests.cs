using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.UseXAsParameterNameAnalyzer,
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

    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public async Task Not_using_x_on_builder_triggers_warnings()
    {

        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.IndexedSetBuilder, simpleLamda, parantherizedLambda);
    }

    [TestMethod]
    public async Task Not_using_x_on_set_triggers_warnings()
    {
        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.IndexedSet, simpleLamda, parantherizedLambda);
    }

    [TestMethod]
    public async Task Not_using_x_on_concurrent_set_triggers_warnings()
    {
        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.ConcurrentIndexedSet, simpleLamda, parantherizedLambda);
    }

    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public async Task Not_using_x_on_primarykey_builder_triggers_warnings()
    {
        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.PrimaryKeyIndexedSetBuilder, simpleLamda, parantherizedLambda);
    }

    [TestMethod]
    public async Task Not_using_x_on_primarykey_set_triggers_warnings()
    {
        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.PrimaryKeyIndexedSet, simpleLamda, parantherizedLambda);
    }

    [TestMethod]
    public async Task Not_using_x_on_concurrent_primarykey_set_triggers_warnings()
    {
        DiagnosticResult simpleLamda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(0).WithArguments("a");
        DiagnosticResult parantherizedLambda = VerifyCS.Diagnostic(UseXAsParameterNameAnalyzer.DiagnosticId).WithLocation(1).WithArguments("b");
        await VerifyCS.VerifyAnalyzerAsync(SampleCode.PrimaryKeyConcurrentIndexedSet, simpleLamda, parantherizedLambda);
    }
}
