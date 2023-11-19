using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Xml.Linq;
using VerifyCS = Akade.IndexedSet.Analyzers.Test.CSharpAnalyzerVerifier<Akade.IndexedSet.Analyzers.ConcurrentSetUsageAnalyzers>;

namespace Akade.IndexedSet.Analyzers.Test;

[TestClass]
public class ReadMethodDoesNotContainWriteMethodAccess
{
    private const string _initialization = $$"""
     using Akade.IndexedSet;
     using Akade.IndexedSet.Concurrency;
     using System.Linq;

     ConcurrentIndexedSet<int> test = new[]{5,10,20}.ToIndexedSet()
                                                    .WithIndex(x => x)
                                                    .BuildConcurrent();
     """;


    [TestMethod]
    public async Task Calling_add_within_read_fails()
    {
        string code = $$"""
         {{_initialization}}
         test.Read(set => { 
            {|#0:set.Add|}(1);
            return Enumerable.Empty<int>();
         });
         """;

        DiagnosticResult name = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId)
                                .WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Calling_AddRange_within_read_fails()
    {
        string code = $$"""
         {{_initialization}}
         test.Read(set => { 
            {|#0:set.AddRange|}(new[]{ 1, 2 });
            return Enumerable.Empty<int>();
         });
         """;

        DiagnosticResult name = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId)
                                .WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Calling_Remove_within_read_fails()
    {
        string code = $$"""
         {{_initialization}}
         test.Read(set => { 
            {|#0:set.Remove|}(5);
            return Enumerable.Empty<int>();
         });
         """;

        DiagnosticResult name = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId)
                                .WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Calling_Clear_within_read_fails()
    {
        string code = $$"""
         {{_initialization}}
         test.Read(set => { 
            {|#0:set.Clear|}();
            return Enumerable.Empty<int>();
         });
         """;

        DiagnosticResult name = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId)
                                .WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(code, name);
    }

    [TestMethod]
    public async Task Calling_Update_within_read_fails()
    {
        string code = $$"""
         {{_initialization}}
         test.Read(set => { 
            {|#0:set.Update|}(5, element => {});
            {|#1:set.Update|}(5, 3, (element, state) => {});
            {|#2:set.Update|}(5, element => element * 3);
            {|#3:set.Update|}(5, 3, (element, state) => element * state);
            return Enumerable.Empty<int>();
         });
         """;

        DiagnosticResult overload1 = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId).WithLocation(0);
        DiagnosticResult overload2 = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId).WithLocation(1);
        DiagnosticResult overload3 = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId).WithLocation(2);
        DiagnosticResult overload4 = VerifyCS.Diagnostic(ConcurrentSetUsageAnalyzers.DoNotPerformWritesWithinReadLockRuleId).WithLocation(3);

        await VerifyCS.VerifyAnalyzerAsync(code, overload1, overload2, overload3, overload4);

    }
}
