using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class CodeFixTests
{
    [TestMethod]
    public async Task WhyDoesItFail()
    {
        string code = $$"""
            using Akade.IndexedSet;

            IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                 .WithFullTextIndex({|{{IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId}}:(x)|} => x.ToString().ToLowerInvariant())
                                                 .Build();
            """;

        string fixedCode = $$"""
             using Akade.IndexedSet;

             IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                  .WithFullTextIndex(x => x.ToString().ToLowerInvariant())
                                                  .Build();
             """;

        await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
    }

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
        return code.Replace("{|#0:x => { return x; }|}", $"x => x")
                   .Replace("{|#1:a|} => a", $"x => x")
                   .Replace("{|#2:(x)|} => x", $"x => x");
    }

    private static string SetDiagnostics(string code)
    {
        return code.Replace("{|#0:x => { return x; }|}", $"{{|{IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId}:x => {{ return x; }}|}}")
                   .Replace("{|#1:a|} => a", $"{{|{IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId}:a|}} => a")
                   .Replace("{|#2:(x)|} => x", $"{{|{IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId}:(x)|}} => x");
    }
}
