using Akade.IndexedSet.Analyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;
using System.Reflection;

namespace Akade.IndexedSet.Analyzers.Test;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.AddAssemblies(ImmutableArray.Create(Assembly.GetAssembly(typeof(IndexedSet<>)).Location));

            SolutionTransforms.Add((solution, projectId) =>
            {
                Project project = solution.GetProject(projectId);

                var parseOptions = (CSharpParseOptions)project.ParseOptions;
                parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.Latest);

                CompilationOptions compilationOptions = project.CompilationOptions;
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
