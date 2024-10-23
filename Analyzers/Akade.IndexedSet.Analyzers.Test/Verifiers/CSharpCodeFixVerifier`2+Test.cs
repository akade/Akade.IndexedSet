﻿using Akade.IndexedSet.Analyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Diagnostics;
using System.IO;

namespace Akade.IndexedSet.Analyzers.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            string config = "Release";
            SetVersion(ref config);

            ReferenceAssemblies = ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

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
