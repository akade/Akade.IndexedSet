using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Akade.IndexedSet.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseXAsParameterNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AkadeIndexedSet0001";
        private const string _category = "Naming";
        private const string _title = "Use x as parameter name in any lambda's that determines an index name";
        private const string _description = "Use the convention of naming the parameter x in any lambda's that determines an index name to avoid IndexNotFoundException at runtime.";
        private const string _messageFormat = "Use x instead of {0} to follow the recommended convention";

        private static readonly DiagnosticDescriptor _useXInLambda = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: _title,
            messageFormat: _messageFormat,
            category: _category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_useXInLambda);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax)context.Node;

            var _relevantTypes = new INamedTypeSymbol[]
            {
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`1"),
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`2"),
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`1"),
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`2"),
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSetBuilder`1"),
                context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSetBuilder`2"),
            }.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                ITypeSymbol targetExpressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (targetExpressionType is INamedTypeSymbol namedType
                    && namedType.IsGenericType
                    && _relevantTypes.Contains(namedType.OriginalDefinition))
                {
                    if (node.ArgumentList.Arguments.FirstOrDefault()?.Expression is LambdaExpressionSyntax lambda)
                    {
                        ParameterSyntax parameterSyntax = null;
                        if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
                        {
                            parameterSyntax = simpleLambda.Parameter;

                        }
                        else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
                        {
                            parameterSyntax = parenthesizedLambda.ParameterList.Parameters.FirstOrDefault();
                        }

                        string name = parameterSyntax?.Identifier.ValueText;
                        if (name != "x")
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_useXInLambda, parameterSyntax.GetLocation(), name));
                        }
                    }
                }
            }
        }

    }
}
