using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class LanguageExtCodeFix() : DocumentEditorCodeFix(
    (LanguageExtAnalyzer.Lift, static (site, _) => FormattableString.Invariant($"Replace `{site}` with `{LanguageExtAnalyzer.Unlifted(site)!}`"), Unlift),
    (LanguageExtAnalyzer.RoundTrip, static (site, _) => FormattableString.Invariant($"Delete `{Called(site)}()` round trip"), Unwrap),
    (LanguageExtAnalyzer.Spread, static (site, _) => {
        SeparatedSyntaxList<CollectionElementSyntax> elements = ((CollectionExpressionSyntax)site).Elements;
        return FormattableString.Invariant($"Replace `{site}` with `{((SpreadElementSyntax)elements[0]).Expression}.Add({elements[1]})`");
    }, Append),
    (LanguageExtAnalyzer.Narrowed, static (site, _) => FormattableString.Invariant($"Delete `{Called(site)}()`"), Unnarrow)) {
    // --- [CALLS]
    private static SimpleNameSyntax Called(SyntaxNode site) {
        ExpressionSyntax expression = site.FirstAncestorOrSelf<InvocationExpressionSyntax>()!.Expression;
        return expression is MemberAccessExpressionSyntax access ? access.Name : (SimpleNameSyntax)expression;
    }

    private static ExpressionSyntax Receiver(InvocationExpressionSyntax call) =>
        call.ArgumentList.Arguments.Count is 0 ? ((MemberAccessExpressionSyntax)call.Expression).Expression : call.ArgumentList.Arguments[0].Expression;

    private static ExpressionSyntax Unwrapped(ExpressionSyntax expression) =>
        expression.DescendantNodesAndSelf(static node => node is ParenthesizedExpressionSyntax).OfType<ExpressionSyntax>().First(static node => node is not ParenthesizedExpressionSyntax);

    // --- [NARROWINGS]
    private static void Unnarrow(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        Replaced(editor, site.FirstAncestorOrSelf<InvocationExpressionSyntax>()!, static (current, _) =>
            current.CopyAnnotationsTo(Parenthesized(Extracted(current, Receiver((InvocationExpressionSyntax)current))))
                .WithAdditionalAnnotations(Formatter.Annotation));

    // --- [LIFTS]
    private static void Unlift(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        Replaced(editor, site, static (current, _) =>
            current.CopyAnnotationsTo(Parenthesized(Extracted(current, LanguageExtAnalyzer.Unlifted(current)!)))
                .WithAdditionalAnnotations(Formatter.Annotation));

    // --- [ROUND_TRIPS]
    private static void Unwrap(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) {
        foreach (SyntaxNode hop in diagnostic.AdditionalLocations.Select(location => Site(editor.OriginalRoot, location)))
            Replaced(editor, hop, (current, _) => {
                ExpressionSyntax value = Extracted(current, Unwrapped(Receiver((InvocationExpressionSyntax)current)));
                return current.CopyAnnotationsTo(hop.Parent is LambdaExpressionSyntax ? value : Parenthesized(value))
                    .WithAdditionalAnnotations(Formatter.Annotation);
            });
    }

    // --- [SPREADS]
    private static void Append(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        Replaced(editor, site, static (current, generator) => {
            CollectionExpressionSyntax collection = (CollectionExpressionSyntax)current;
            SpreadElementSyntax spread = (SpreadElementSyntax)collection.Elements[0];
            SyntaxToken separator = collection.Elements.GetSeparator(0);
            ExpressionSyntax item = ((ExpressionElementSyntax)collection.Elements[1]).Expression;
            InvocationExpressionSyntax call = Invoked(Access(Parenthesized(Extracted(spread, spread.Expression)), Identifier(generator, "Add")),
                [item.WithTrailingTrivia(item.GetTrailingTrivia().Concat(Following(collection.Elements.GetSeparators().Skip(1)
                    .SelectMany(static separator => separator.LeadingTrivia.Concat(separator.TrailingTrivia)).Concat(collection.CloseBracketToken.LeadingTrivia))))]);
            return current.CopyAnnotationsTo(call
                .WithLeadingTrivia(collection.OpenBracketToken.LeadingTrivia.Concat(Relocated(collection.OpenBracketToken.TrailingTrivia)).Concat(call.GetLeadingTrivia()))
                .WithArgumentList(call.ArgumentList
                    .WithOpenParenToken(call.ArgumentList.OpenParenToken.WithLeadingTrivia(separator.LeadingTrivia)
                        .WithTrailingTrivia(Relocated(separator.TrailingTrivia)))
                    .WithCloseParenToken(call.ArgumentList.CloseParenToken
                        .WithTrailingTrivia(collection.CloseBracketToken.TrailingTrivia))))
                .WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        });
}
