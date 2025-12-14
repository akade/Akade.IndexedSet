using Akade.IndexedSet.Analyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Diagnostics;

namespace Akade.IndexedSet.Analyzers.Test;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test()
        {
            string config = "Release";
            SetVersion(ref config);

            ReferenceAssemblies = NetRefAssemblies.Current;

            TestState.AdditionalReferences.Add(typeof(IndexedSet<>).Assembly);

            SolutionTransforms.Add((solution, projectId) =>
            {
                Project project = solution.GetProject(projectId);

                var parseOptions = (CSharpParseOptions)project.ParseOptions;
                parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.Latest);

                CompilationOptions compilationOptions = project.CompilationOptions.WithOutputKind(OutputKind.ConsoleApplication);
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings)
                    );

                solution = solution.WithProjectParseOptions(projectId, parseOptions)
                                   .WithProjectCompilationOptions(projectId, compilationOptions);

                return solution;
            });
        }

        [Conditional("Debug")]
        private static void SetVersion(ref string config)
        {
            config = "Debug";
        }
    }
}
