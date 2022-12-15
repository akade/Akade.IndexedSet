using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Akade.IndexedSet.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AkadeIndexedSetAnalyzersCodeFixProvider)), Shared]
    public class AkadeIndexedSetAnalyzersCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseXAsParameterNameAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            LambdaExpressionSyntax lambda = root.FindToken(diagnosticSpan.Start).Parent
                                                .AncestorsAndSelf()
                                                .OfType<LambdaExpressionSyntax>()
                                                .First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Rename to x",
                    createChangedSolution: cancellationToken => RenameToXAsync(context.Document, lambda, cancellationToken),
                    equivalenceKey: nameof(UseXAsParameterNameAnalyzer.DiagnosticId)),
                diagnostic);
        }

        private async Task<Solution> RenameToXAsync(Document document, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
        {
            if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
            {
                return await FixSimpleLambdaAsync(document, simpleLambda, cancellationToken);
            }
            else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
            {
                return await FixParenthesizedLambdaAsync(document, parenthesizedLambda, cancellationToken);
            }
            throw new ArgumentOutOfRangeException(nameof(lambda));
        }

        private async Task<Solution> FixParenthesizedLambdaAsync(Document document, ParenthesizedLambdaExpressionSyntax parenthesizedLambda, CancellationToken cancellationToken)
        {
            var solutionEditor = new SolutionEditor(document.Project.Solution);
            DocumentEditor documentEditor = await solutionEditor.GetDocumentEditorAsync(document.Project.Solution.GetDocumentId(parenthesizedLambda.SyntaxTree), cancellationToken);

            var syntaxGenerator = SyntaxGenerator.GetGenerator(documentEditor.OriginalDocument);

            SyntaxNode simpleLambda = parenthesizedLambda.ExpressionBody is null
                                    ? syntaxGenerator.VoidReturningLambdaExpression(parenthesizedLambda.ParameterList.Parameters, parenthesizedLambda.Block.Statements)
                                    : syntaxGenerator.VoidReturningLambdaExpression(parenthesizedLambda.ParameterList.Parameters, parenthesizedLambda.ExpressionBody);

            documentEditor.ReplaceNode(parenthesizedLambda, simpleLambda);

            return solutionEditor.GetChangedSolution();
        }

        private async Task<Solution> FixSimpleLambdaAsync(Document document, SimpleLambdaExpressionSyntax simpleLambda, CancellationToken cancellationToken)
        {
            Solution originalSolution = document.Project.Solution;
            SemanticModel semanticModel = await document.GetSemanticModelAsync();

            ISymbol symbol = semanticModel.GetDeclaredSymbol(simpleLambda.Parameter, cancellationToken);
            return await Renamer.RenameSymbolAsync(originalSolution, symbol, default, "x", cancellationToken);
        }
    }
}
