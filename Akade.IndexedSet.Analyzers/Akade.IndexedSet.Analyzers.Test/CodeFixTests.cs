using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.UseXAsParameterNameAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class CodeFixTests
{
    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public async Task Not_using_x_on_builder_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.IndexedSetBuilder), FixCode(SampleCode.IndexedSetBuilder));
    }

    [TestMethod]
    public async Task Not_using_x_on_set_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.IndexedSet), FixCode(SampleCode.IndexedSet));
    }

    [TestMethod]
    public async Task Not_using_x_on_concurrent_set_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.ConcurrentIndexedSet), FixCode(SampleCode.ConcurrentIndexedSet));
    }

    //Diagnostic and CodeFix both triggered and checked for
    [TestMethod]
    public async Task Not_using_x_on_primarykey_builder_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.PrimaryKeyIndexedSetBuilder), FixCode(SampleCode.PrimaryKeyIndexedSetBuilder));
    }

    [TestMethod]
    public async Task Not_using_x_on_primarykey_set_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.PrimaryKeyIndexedSet), FixCode(SampleCode.PrimaryKeyIndexedSet));
    }

    [TestMethod]
    public async Task Not_using_x_on_concurrent_primarykey_set_triggers_warnings()
    {
        await VerifyCS.VerifyCodeFixAsync(SetDiagnostics(SampleCode.PrimaryKeyConcurrentIndexedSet), FixCode(SampleCode.PrimaryKeyConcurrentIndexedSet));
    }

    private static string FixCode(string code)
    {
        return code.Replace("{|#0:a|} => a", "x => x").Replace("({|#1:b|}) => b", "x => x");
    }

    private static string SetDiagnostics(string code)
    {
        return code.Replace("{|#0:a|} => a", $"{{|{UseXAsParameterNameAnalyzer.DiagnosticId}:a|}} => a")
                   .Replace("({|#1:b|}) => b", $"({{|{UseXAsParameterNameAnalyzer.DiagnosticId}:b|}}) => b");
    }
}
