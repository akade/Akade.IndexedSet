using Akade.IndexedSet.Analyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace Akade.IndexedSet.Analyzers.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net60
                .WithPackages(ImmutableArray.Create(new PackageIdentity("Akade.IndexedSet", "0.7.0")));

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
    }
}
