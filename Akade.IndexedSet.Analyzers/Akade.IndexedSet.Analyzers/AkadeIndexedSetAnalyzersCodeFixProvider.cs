
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Akade.IndexedSet.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AkadeIndexedSetAnalyzersCodeFixProvider)), Shared]
public class AkadeIndexedSetAnalyzersCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId,
                                                                                                IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId,
                                                                                                IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            LambdaExpressionSyntax? lambda = root.FindToken(diagnosticSpan.Start).Parent?
                                                 .AncestorsAndSelf()
                                                 .OfType<LambdaExpressionSyntax>()
                                                 .First();

            if (lambda is null)
            {
                throw new InvalidOperationException("Codefix cannot find associated node from diagnostic");
            }

            CodeAction? action = diagnostic.Id switch
            {
                IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId => FixUseXNaming(context, lambda),
                IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId => RemoveParenthesesInLambda(context, lambda),
                IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId => RemoveBlockBodiedLambda(context, lambda),
                _ => null
            };
            if (action != null)
            {
                context.RegisterCodeFix(action, diagnostic);
            }
        }

    }

    private CodeAction? RemoveBlockBodiedLambda(CodeFixContext context, LambdaExpressionSyntax lambda)
    {
        if (lambda.Body is BlockSyntax block
            && block.Statements.SingleOrDefault() is ReturnStatementSyntax returnStatement
            && returnStatement.Expression != null)
        {
            return CodeAction.Create(
                title: "Use expression-bodied lambdas",
                createChangedSolution: cancellationToken => MakeExpressionBodyAsync(context.Document, lambda, cancellationToken),
                equivalenceKey: nameof(IndexNamingRulesAnalyzer.DoNotUseBlockBodiedLambdaRuleId));
        }

        return null;
    }

    private CodeAction? RemoveParenthesesInLambda(CodeFixContext context, LambdaExpressionSyntax lambda)
    {
        if (lambda is not ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            throw new InvalidOperationException("Codefix cannot find associated node from diagnostic");
        }

        return parenthesizedLambda.ParameterList.Parameters.Count != 1
            ? null
            : CodeAction.Create(
            title: "Remove parenthesis",
            createChangedSolution: cancellationToken => RemoveParenthesisAsync(context.Document, parenthesizedLambda, cancellationToken),
            equivalenceKey: nameof(IndexNamingRulesAnalyzer.DoNotUseParenthesesInLambdaRuleId));
    }

    private CodeAction FixUseXNaming(CodeFixContext context, LambdaExpressionSyntax lambda)
    {
        return CodeAction.Create(
                title: "Rename to x",
                createChangedSolution: cancellationToken => RenameToXAsync(context.Document, lambda, cancellationToken),
                equivalenceKey: nameof(IndexNamingRulesAnalyzer.UseXAsIdentifierInLambdaRuleId));
    }

    private async Task<Solution> RemoveParenthesisAsync(Document document, ParenthesizedLambdaExpressionSyntax lambda, CancellationToken cancellationToken)
    {
        var solutionEditor = new SolutionEditor(document.Project.Solution);
        DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Project.Solution.GetDocumentId(lambda.SyntaxTree), cancellationToken);

        var syntaxGenerator = SyntaxGenerator.GetGenerator(documentEditor.OriginalDocument);

        string parameterName = lambda.ParameterList.Parameters.Single().Identifier.Text;

        SyntaxNode simpleLambda = lambda switch
        {
            { ExpressionBody: not null } => syntaxGenerator.ValueReturningLambdaExpression(parameterName, lambda.ExpressionBody),
            { Block: not null } => syntaxGenerator.ValueReturningLambdaExpression(parameterName, lambda.Block.Statements),
            _ => throw new InvalidOperationException("Invalid input lambda")
        };

        documentEditor.ReplaceNode(lambda, simpleLambda);

        return solutionEditor.GetChangedSolution();
    }

    private async Task<Solution> MakeExpressionBodyAsync(Document document, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
    {
        var solutionEditor = new SolutionEditor(document.Project.Solution);
        DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Project.Solution.GetDocumentId(lambda.SyntaxTree), cancellationToken);

        var syntaxGenerator = SyntaxGenerator.GetGenerator(documentEditor.OriginalDocument);
        IEnumerable<ParameterSyntax> parameters = lambda switch
        {
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters,
            SimpleLambdaExpressionSyntax simpleLambda => new[] { simpleLambda.Parameter },
            _ => throw new ArgumentOutOfRangeException(nameof(lambda)),
        };

        if (lambda.Block is not BlockSyntax block)
        {
            throw new InvalidOperationException("No block.");
        }

        var returnStatement = (ReturnStatementSyntax)block.Statements.Single();

        SyntaxNode refactored = syntaxGenerator.ValueReturningLambdaExpression(parameters, returnStatement.Expression
            ?? throw new InvalidOperationException("No return expression"));

        documentEditor.ReplaceNode(lambda, refactored);
        return solutionEditor.GetChangedSolution();
    }

    private async Task<Solution> RenameToXAsync(Document document, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
    {
        Solution originalSolution = document.Project.Solution;
        SemanticModel semanticModel = await document.GetSemanticModelAsync() ?? throw new InvalidOperationException("Cannot obtain semantic model");

        ParameterSyntax parameter = lambda switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter,
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters.First(),
            _ => throw new ArgumentOutOfRangeException(nameof(lambda))
        };

        ISymbol symbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken) ?? throw new InvalidOperationException("Cannot obtain symbol");
        return await Renamer.RenameSymbolAsync(originalSolution, symbol, default, "x", cancellationToken);
    }
}
