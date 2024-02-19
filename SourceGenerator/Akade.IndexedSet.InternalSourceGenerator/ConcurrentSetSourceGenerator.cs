using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Akade.IndexedSet.InternalSourceGenerator;

[Generator]
public class ConcurrentSetSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<MethodDeclarationSyntax>> readMethods = context.SyntaxProvider.ForAttributeWithMetadataName("Akade.IndexedSet.Concurrency.ReadAccessAttribute",
            predicate: (node, _) => node is MethodDeclarationSyntax,
            transform: (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode).Collect();

        context.RegisterSourceOutput(readMethods, (context, source) =>
        {
            if (source.IsDefaultOrEmpty)
            {
                return;
            }

            IntendedStringBuilder stringBuilder = new();
            WriteDelegationForAllMethods(source, stringBuilder, true);

            context.AddSource("ConcurrentIndexedSet.read.generated.cs", stringBuilder.ToString());
        });

        IncrementalValueProvider<ImmutableArray<MethodDeclarationSyntax>> writeMethods = context.SyntaxProvider.ForAttributeWithMetadataName("Akade.IndexedSet.Concurrency.WriteAccessAttribute",
        predicate: (node, _) => node is MethodDeclarationSyntax,
        transform: (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode).Collect();

        context.RegisterSourceOutput(writeMethods, (context, source) =>
        {
            if (source.IsDefaultOrEmpty)
            {
                return;
            }

            IntendedStringBuilder stringBuilder = new();
            WriteDelegationForAllMethods(source, stringBuilder, isReader: false);

            context.AddSource("ConcurrentIndexedSet.write.generated.cs", stringBuilder.ToString());
        });

    }

    private void WriteDelegationForAllMethods(ImmutableArray<MethodDeclarationSyntax> methods, IntendedStringBuilder stringBuilder, bool isReader)
    {
        _ = stringBuilder.AppendLine(
                        """
                using System.Diagnostics.CodeAnalysis;
                using System.Runtime.CompilerServices;

                namespace Akade.IndexedSet.Concurrency;
                #nullable enable
                """);

        WriteClassAndMethods(methods.Where(m => !IsWithinPrimaryKeySet(m)), stringBuilder, isReader, isPrimaryKeySet: false);
        WriteClassAndMethods(methods.Where(IsWithinPrimaryKeySet), stringBuilder, isReader, isPrimaryKeySet: true);

        _ = stringBuilder.AppendLine("#nullable restore");

    }

    private void WriteClassAndMethods(IEnumerable<MethodDeclarationSyntax> methods, IntendedStringBuilder stringBuilder, bool isReader, bool isPrimaryKeySet)
    {
        if (methods.Any())
        {
            string primaryKeyTypeArgOrEmpty = isPrimaryKeySet ? "TPrimaryKey, " : "";

            _ = stringBuilder.AppendLine($"public partial class ConcurrentIndexedSet<{primaryKeyTypeArgOrEmpty}TElement>");
            using (stringBuilder.StartCodeBlock())
            {
                foreach (MethodDeclarationSyntax method in methods)
                {
                    WriteMethodDelegation(stringBuilder, method, isReader, isPrimaryKeySet);
                }
            }
        }
    }

    private static bool IsWithinPrimaryKeySet(MethodDeclarationSyntax method)
    {
        return method.FirstAncestorOrSelf<ClassDeclarationSyntax>()?.TypeParameterList?.Parameters.Count == 2;
    }

    private void WriteMethodDelegation(IntendedStringBuilder stringBuilder, MethodDeclarationSyntax method, bool isReader, bool isPrimaryKeySet)
    {
        string acquireLock = $"using ({(isReader ? "AcquireReaderLock()" : "AcquireWriterLock()")})";
        string setVariable = isPrimaryKeySet ? "_primaryKeyIndexedSet" : "_indexedSet";

        WriteMethodHeader(method, stringBuilder);
        using (stringBuilder.StartCodeBlock())
        {
            _ = stringBuilder.AppendLine(acquireLock);
            using (stringBuilder.StartCodeBlock())
            {

                bool isVoidMethod = method.ReturnType is PredefinedTypeSyntax { Keyword: SyntaxToken keyword } && keyword.IsKind(SyntaxKind.VoidKeyword);
                if (!isVoidMethod)
                {
                    _ = stringBuilder.Append("return ");
                }

                _ = stringBuilder.Append(setVariable)
                                 .Append(".")
                                 .Append(method.Identifier.Text);

                _ = stringBuilder.Append("(")
                                 .AppendJoin(", ", method.ParameterList.Parameters.Select(p => (p.Modifiers.Any(SyntaxKind.OutKeyword) ? "out " : "") + p.Identifier.Text))
                                 .Append(")");

                if (method.ReturnType is GenericNameSyntax { Arity: 1, Identifier.Text: "IEnumerable" })
                {
                    _ = stringBuilder.Append(".ToArray()");

                }
                _ = stringBuilder.AppendLine(";");
            }
        }
    }

    private void WriteMethodHeader(MethodDeclarationSyntax method, IntendedStringBuilder stringBuilder)
    {
        _ = stringBuilder.Append(method.GetLeadingTrivia().ToString().TrimStart());


        if (method.AttributeLists.Select(a => a.Attributes.SingleOrDefault(a => a.Name is IdentifierNameSyntax { Identifier.Text: "Experimental" }))
                                 .FirstOrDefault(a => a is not null) is AttributeSyntax experimentalAttribute)
        {
            _ = stringBuilder.Append("[");
            _ = stringBuilder.Append(experimentalAttribute.ToString());
            _ = stringBuilder.AppendLine("]");
        }

        _ = stringBuilder.Append(method.Modifiers.ToString()).Append(" ")
                         .Append(method.ReturnType.ToString()).Append(" ")
                         .Append(method.Identifier.ToString());

        if (method.TypeParameterList is { Parameters: { Count: >= 1 } typeParameters })
        {
            _ = stringBuilder.Append("<")
                             .Append(typeParameters.ToString())
                             .Append(">");
        }

        _ = stringBuilder.Append(method.ParameterList.ToString());

        if (method.ConstraintClauses is { Count: >= 1 })
        {
            _ = stringBuilder.AppendLine().Append("    ").Append(method.ConstraintClauses.ToString());
        }

        _ = stringBuilder.AppendLine();
    }
}
