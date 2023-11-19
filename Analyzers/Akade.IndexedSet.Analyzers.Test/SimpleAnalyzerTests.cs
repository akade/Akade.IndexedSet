using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;

/// <summary>
/// Basic tests that test one occurrence for each diagnostics
/// </summary>
[TestClass]
public class SimpleAnalyzerTests
{
    [TestMethod]
    public async Task Lambda_parameter_should_be_named_xAsync()
    {
        string code = $$"""
            using Akade.IndexedSet;

            IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                 .WithIndex({|#0:y => y|})
                                                 .Build();
            """;

        DiagnosticResult name = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId)
                                        .WithLocation(0)
                                        .WithArguments("y");

        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Parenthesis_should_be_removed_Async()
    {
        string code = $$"""
        using Akade.IndexedSet;

        IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                             .WithIndex({|#0:(x) => x|})
                                             .Build();
        """;

        DiagnosticResult name = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId)
                                        .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Do_not_use_block_bodied_lambda_Async()
    {
        string code = $$"""
    using Akade.IndexedSet;

    IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                         .WithIndex({|#0:x => { return x; }|})
                                         .Build();
    """;

        DiagnosticResult name = VerifyCS.Diagnostic(IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId)
                                        .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task No_diagnostic_reported_for_concurrent_set_update()
    {
        string code = $$"""
         using Akade.IndexedSet;
         using Akade.IndexedSet.Concurrency;
         

         ConcurrentIndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                        .WithIndex(x => x)
                                                        .BuildConcurrent();

         test.Update(set => { 
            set.Add(1);
            set.Add(2);
         });
         test.Update((set, state) => {
            set.Add(3);
            set.Add(4);
         }, 8);
         """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [TestMethod]
    public async Task No_diagnostic_reported_for_concurrent_set_read()
    {
        string code = $$"""
         using Akade.IndexedSet;
         using Akade.IndexedSet.Concurrency;
         using System.Linq;
     

         ConcurrentIndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                        .WithIndex(x => x)
                                                        .BuildConcurrent();

         test.Read(set => { 
            return Enumerable.Empty<int>();
         });
         """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }
}
