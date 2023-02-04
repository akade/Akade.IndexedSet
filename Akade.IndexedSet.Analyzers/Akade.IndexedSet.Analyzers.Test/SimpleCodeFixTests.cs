using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpCodeFixVerifier<
    Akade.IndexedSet.Analyzers.IndexNamingRulesAnalyzer,
    Akade.IndexedSet.Analyzers.AkadeIndexedSetAnalyzersCodeFixProvider>;

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class SimpleCodeFixTests
{
    [TestMethod]
    public async Task Fix_namingAsync()
    {
        string code = $$"""
            using Akade.IndexedSet;

            IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                 .WithIndex({|{{IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId}}:y => y|})
                                                 .Build();
            """;

        string fixedCode = $$"""
             using Akade.IndexedSet;

             IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                  .WithIndex(x => x)
                                                  .Build();
             """;

        await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
    }

    [TestMethod]
    public async Task Fix_parenthesisAsync()
    {
        string code = $$"""
        using Akade.IndexedSet;

        IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                             .WithIndex({|{{IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId}}:(x) => x|})
                                             .Build();
        """;

        string fixedCode = $$"""
         using Akade.IndexedSet;

         IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                              .WithIndex(x => x)
                                              .Build();
         """;

        await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
    }

    [TestMethod]
    public async Task Fix_block_bodied_lambda_expressionAsync()
    {
        string code = $$"""
        using Akade.IndexedSet;

        IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                             .WithIndex({|{{IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId}}:x => {return x;}|})
                                             .Build();
        """;

        string fixedCode = $$"""
         using Akade.IndexedSet;

         IndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                              .WithIndex(x => x)
                                              .Build();
         """;

        await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
    }
}
