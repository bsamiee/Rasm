using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Rasm.Analyzers.CodeFixes.LanguageExt;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public sealed class ExplicitResultLiftCodeFix : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => ["RASM1001", "RASM1002"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root?.FindNode(context.Span, getInnermostNodeForTie: true) is not ExpressionSyntax lift) return;
        ExpressionSyntax? value = lift switch {
            InvocationExpressionSyntax { ArgumentList.Arguments.Count: 1 } invocation => invocation.ArgumentList.Arguments[0].Expression,
            ObjectCreationExpressionSyntax { ArgumentList.Arguments.Count: 1 } creation => creation.ArgumentList.Arguments[0].Expression,
            CastExpressionSyntax cast => cast.Expression,
            _ => null,
        };
        if (value is null) return;
        context.RegisterCodeFix(
            CodeAction.Create(
                "Remove explicit conversion",
                _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(lift, SyntaxFactory.ParenthesizedExpression(value.WithoutTrivia()).WithTriviaFrom(lift).WithAdditionalAnnotations(Simplifier.Annotation)))),
                nameof(ExplicitResultLiftCodeFix)),
            context.Diagnostics);
    }
}
