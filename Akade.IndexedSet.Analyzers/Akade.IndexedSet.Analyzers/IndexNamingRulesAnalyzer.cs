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
        title: "Convention: Use x as parameter name in any lambda's that determines an index name",
        messageFormat: "Use x instead of {0} to follow the recommended convention for naming indices",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use the convention of naming the parameter x in any lambda's that determines an index name to consistently name indices.",
        helpLinkUri: HelpLinkBase + UseXAsIdentifierInLambdaRuleId);

    private static readonly DiagnosticDescriptor _doNotUseParenthesesDescriptor = new DiagnosticDescriptor(
        id: DoNotUseParenthesesInLambdaRuleId,
        title: "Convention: Do not use parentheses in any lambda's that determines an index name",
        messageFormat: "Use a lambda parameters without parentheses to follow the recommended convention for naming indices",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use the convention of using parameters without parentheses in any lambda's that determines an index name to consistently name indices.",
        helpLinkUri: HelpLinkBase + DoNotUseParenthesesInLambdaRuleId);

    private static readonly DiagnosticDescriptor _doNotUseBlockBodiedLambdaDescriptor = new DiagnosticDescriptor(
        id: DoNotUseBlockBodiedLambdaRuleId,
        title: "Convention: Do not use block bodied in any lambda's that determines an index name",
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
        
    }
}
