using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            IntendedStringBuilder stringBuilder = new();

            _ = stringBuilder.AppendLine(
                """
                using Akade.IndexedSet.Concurrency;
                namespace Akade.IndexedSet.Concurrency;
                
                public partial class ConcurrentIndexedSet<TElement>
                """);

            using (stringBuilder.StartCodeBlock())
            {
                foreach (MethodDeclarationSyntax targetNode in source)
                {
                    WriteMethodHeader(targetNode, stringBuilder);
                    using (stringBuilder.StartCodeBlock())
                    {
                        _ = stringBuilder.AppendLine("using (AcquireReaderLock())");
                        using (stringBuilder.StartCodeBlock())
                        {

                            if (targetNode.ReturnType is not null)
                            {
                                _ = stringBuilder.Append("return ");
                            }

                            _ = stringBuilder.Append("_indexedSet.")
                                             .Append(targetNode.Identifier.Text)
                                             .Append("(")
                                             .AppendJoin(", ", targetNode.ParameterList.Parameters.Select(p => p.Identifier.Text))
                                             .Append(")");

                            if (targetNode.ReturnType is GenericNameSyntax { Arity: 1, Identifier.Text: "IEnumerable" })
                            {
                                _ = stringBuilder.Append(".ToArray()");

                            }
                            _ = stringBuilder.AppendLine(";");
                        }
                    }
                }
            }

            context.AddSource("ConcurrentIndexedSet.read.generated.cs", stringBuilder.ToString());
        });

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetNode"></param>
    /// <param name="stringBuilder"></param>
    private void WriteMethodHeader(MethodDeclarationSyntax targetNode, IntendedStringBuilder stringBuilder)
    {
        _ = stringBuilder.Append(targetNode.GetLeadingTrivia().ToString().TrimStart())
                         .Append(targetNode.Modifiers.ToString()).Append(" ")
                         .Append(targetNode.ReturnType.ToString()).Append(" ")
                         .Append(targetNode.Identifier.ToString())
                         .Append(targetNode.ParameterList.ToString()).Append(" ")
                         .AppendLine();
    }

    private static bool IsMethodWithinIndexedSet(MethodDeclarationSyntax method)
    {
        static bool IsWithinIndexedSet(SyntaxNode node) => node switch
        {
            ClassDeclarationSyntax { Identifier.Text: "IndexedSet" } => true,
            { Parent: SyntaxNode parent } => IsWithinIndexedSet(parent),
            _ => false
        };

        return IsWithinIndexedSet(method);
    }
}
