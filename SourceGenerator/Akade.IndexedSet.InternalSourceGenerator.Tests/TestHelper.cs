﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;

namespace Akade.IndexedSet.InternalSourceGenerator.Tests;

internal sealed class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
        VerifyDiffPlex.Initialize();
    }
}

public static class TestHelper
{
    public static TestHelperWithVerify<TVerifyBase> VerifySourceGen<TVerifyBase>(this TVerifyBase verifyBase)
       where TVerifyBase : VerifyBase
    {
        return new TestHelperWithVerify<TVerifyBase>(verifyBase);
    }

    public sealed class TestHelperWithVerify<TVerifyBase>(TVerifyBase verifyBase)
       where TVerifyBase : VerifyBase
    {
        private readonly TVerifyBase _verifyBase = verifyBase;

        public Task ForSource<TSourceGen>(string source) where TSourceGen : IIncrementalGenerator, new()
        {
            SyntaxTree attributeSyntaxTree = CSharpSyntaxTree.ParseText(
                """
                using System.Diagnostics;

                namespace Akade.IndexedSet.Concurrency;

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                [Conditional("RemovedFromBuiltCode")]
                internal class ReadAccessAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                [Conditional("RemovedFromBuiltCode")]
                internal class WriteAccessAttribute : Attribute { }
                """);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: [attributeSyntaxTree, syntaxTree]
                );

            TSourceGen generator = new();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            return _verifyBase.Verify(driver);
        }
    }
}

