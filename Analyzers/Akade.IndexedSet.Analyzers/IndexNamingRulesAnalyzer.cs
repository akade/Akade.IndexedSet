using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Akade.IndexedSet.Analyzers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Does not work with Microsoft.CodeAnalysis.Analyzers")]
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IndexNamingRulesAnalyzer : DiagnosticAnalyzer
{
    public const string RulesCategory = "Akade.IndexedSet.IndexNaming";
    public const string HelpLinkBase = "https://github.com/akade/Akade.IndexedSet/tree/main/Akade.IndexedSet.Analyzers/Readme.md#";

    public const string UseXAsIdentifierInLambdaRuleId = "AkadeIndexedSet0001";
    public const string DoNotUseParenthesesInLambdaRuleId = "AkadeIndexedSet0002";
    public const string DoNotUseBlockBodiedLambdaRuleId = "AkadeIndexedSet0003";

    private static readonly DiagnosticDescriptor _useXInLambdaDescriptor = new DiagnosticDescriptor(
        id: UseXAsIdentifierInLambdaRuleId,
        title: "Convention: Use x as parameter name in any lambda that determines an index name",
        messageFormat: "Use x instead of {0} to follow the recommended convention for naming indices",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use the convention of naming the parameter x in any lambda's that determines an index name to consistently name indices.",
        helpLinkUri: HelpLinkBase + UseXAsIdentifierInLambdaRuleId);

    private static readonly DiagnosticDescriptor _doNotUseParenthesesDescriptor = new DiagnosticDescriptor(
        id: DoNotUseParenthesesInLambdaRuleId,
        title: "Convention: Do not use parentheses in any lambda that determines an index name",
        messageFormat: "Use a lambda parameters without parentheses to follow the recommended convention for naming indices",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use the convention of using parameters without parentheses in any lambda's that determines an index name to consistently name indices.",
        helpLinkUri: HelpLinkBase + DoNotUseParenthesesInLambdaRuleId);

    private static readonly DiagnosticDescriptor _doNotUseBlockBodiedLambdaDescriptor = new DiagnosticDescriptor(
        id: DoNotUseBlockBodiedLambdaRuleId,
        title: "Convention: Do not use block bodied in any lambda that determines an index name",
        messageFormat: "Use expression bodied lambdas for simple indices and static methods for more complex ones to follow the recommended convention for naming indices",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use the convention of expression bodied lambdas for simple indices or static methods for more complex indices to consistently name indices.",
        helpLinkUri: HelpLinkBase + DoNotUseBlockBodiedLambdaRuleId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(_useXInLambdaDescriptor, _doNotUseParenthesesDescriptor, _doNotUseBlockBodiedLambdaDescriptor);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = (InvocationExpressionSyntax)context.Node;

        var _relevantTypes = new INamedTypeSymbol?[]
        {
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`1"),
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`2"),
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`1"),
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`2"),
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSetBuilder`1"),
            context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSetBuilder`2"),
        }.OfType<INamedTypeSymbol>()
         .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            ITypeSymbol? targetExpressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (targetExpressionType is INamedTypeSymbol namedType
                && namedType.IsGenericType
                && _relevantTypes.Contains(namedType.OriginalDefinition))
            {
                if (node.ArgumentList.Arguments.FirstOrDefault()?.Expression is LambdaExpressionSyntax lambda
                    && !IsExcludedMethod(memberAccess.Name.Identifier))
                {
                    CheckParameterName(context, lambda);
                    CheckForParenthesized(context, lambda);
                    CheckForBlockBodiedLambda(context, lambda);
                }
            }
        }
    }

    private bool IsExcludedMethod(SyntaxToken identifier)
    {
        return identifier.ValueText is "Update";
    }

    private void CheckForBlockBodiedLambda(SyntaxNodeAnalysisContext context, LambdaExpressionSyntax lambda)
    {
        if (lambda.Block != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(_doNotUseBlockBodiedLambdaDescriptor, lambda.GetLocation()));
        }
    }

    private void CheckForParenthesized(SyntaxNodeAnalysisContext context, LambdaExpressionSyntax lambda)
    {
        if (lambda is ParenthesizedLambdaExpressionSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(_doNotUseParenthesesDescriptor, lambda.GetLocation()));
        }
    }

    private static void CheckParameterName(SyntaxNodeAnalysisContext context, LambdaExpressionSyntax lambda)
    {
        ParameterSyntax? parameterSyntax = null;
        if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
        {
            parameterSyntax = simpleLambda.Parameter;

        }
        else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            parameterSyntax = parenthesizedLambda.ParameterList.Parameters.FirstOrDefault();
        }

        if (parameterSyntax?.Identifier.ValueText is string name && name != "x")
        {
            context.ReportDiagnostic(Diagnostic.Create(_useXInLambdaDescriptor, lambda.GetLocation(), name));
        }
    }
}
