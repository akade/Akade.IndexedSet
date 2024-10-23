using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Akade.IndexedSet.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Does not work with Microsoft.CodeAnalysis.Analyzers")]
public sealed class ConcurrentSetUsageAnalyzers : DiagnosticAnalyzer
{
    public const string RulesCategory = "Akade.IndexedSet.ConcurrencyRules";
    public const string HelpLinkBase = "https://github.com/akade/Akade.IndexedSet/tree/main/Analyzers/Readme.md#";

    public const string DoNotPerformWritesWithinReadLockRuleId = "AkadeIndexedSet0004";

    private static readonly DiagnosticDescriptor _doNotPerformWritesWithinReadLock = new DiagnosticDescriptor(
        id: DoNotPerformWritesWithinReadLockRuleId,
        title: "Do not perform writes within a read-lock",
        messageFormat: "Do not perform writes within a read-lock",
        category: RulesCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Do not perform writes within a read-lock as it may result in incorrect reads, or even corrupted state.",
        helpLinkUri: HelpLinkBase + DoNotPerformWritesWithinReadLockRuleId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [_doNotPerformWritesWithinReadLock];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = (InvocationExpressionSyntax)context.Node;

        var concurrentSet = new INamedTypeSymbol?[]
        {
             context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`1"),
             context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.Concurrency.ConcurrentIndexedSet`2"),
        }.OfType<INamedTypeSymbol>()
         .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var indexedSet = new INamedTypeSymbol?[]
        {
             context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`1"),
             context.Compilation.GetTypeByMetadataName("Akade.IndexedSet.IndexedSet`2"),
        }.OfType<INamedTypeSymbol>()
         .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            ITypeSymbol? targetExpressionType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (targetExpressionType is INamedTypeSymbol namedType
                && namedType.IsGenericType
                && indexedSet.Contains(namedType.OriginalDefinition))
            {
                if (IsWriteMethod(memberAccess.Name.Identifier.ValueText))
                {
                    bool isWithinRead = node.Ancestors()
                                            .OfType<InvocationExpressionSyntax>()
                                            .Where(invocation => invocation.Expression is MemberAccessExpressionSyntax)
                                            .Select(invocation => (MemberAccessExpressionSyntax)invocation.Expression)
                                            .Where(m => context.SemanticModel.GetTypeInfo(m.Expression).Type is INamedTypeSymbol namedType
                                                        && namedType.IsGenericType
                                                        && concurrentSet.Contains(namedType.OriginalDefinition))
                                            .Where(m => m.Name.Identifier.ValueText == "Read").Any();

                    if(isWithinRead)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(_doNotPerformWritesWithinReadLock, memberAccess.GetLocation()));
                    }
                }
            }
        }
    }

    private bool IsWriteMethod(string valueText)
    {
        return valueText
            is "Add"
            or "AddRange"
            or "Remove"
            or "Clear"
            or "Update";


    }
}
