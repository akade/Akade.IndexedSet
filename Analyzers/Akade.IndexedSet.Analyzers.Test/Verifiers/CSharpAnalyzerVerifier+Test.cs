using Akade.IndexedSet.Analyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;

namespace Akade.IndexedSet.Analyzers.Test;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
    {
        public Test()
        {
            string config = "Release";
            SetVersion(ref config);

            ReferenceAssemblies = new ReferenceAssemblies("net8.0", new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.0"), Path.Combine("ref", "net8.0"));

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
