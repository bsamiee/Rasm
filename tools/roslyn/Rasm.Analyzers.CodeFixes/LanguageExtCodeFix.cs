using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

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
    (LanguageExtAnalyzer.Narrowed, static (site, _) => FormattableString.Invariant($"Delete `{Called(site)}()`"), Unnarrow),
    (LanguageExtAnalyzer.Continued, static (site, _) => FormattableString.Invariant($"Write `{site.FirstAncestorOrSelf<InvocationExpressionSyntax>()}` without its lift"), Continue)) {
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

    // --- [CONTINUATIONS]
    private static T Kept<T>(SyntaxNode owner, T value) where T : SyntaxNode =>
        Extracted(owner, value).WithTrailingTrivia(owner.DescendantTrivia(TextSpan.FromBounds(value.Span.End, owner.Span.End)).Concat(owner.GetTrailingTrivia()));

    private static ExpressionSyntax Lifted(InvocationExpressionSyntax step) =>
        step.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax { ExpressionBody: { } body } ? body : step.ArgumentList.Arguments[0].Expression;

    private static void Continue(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) {
        InvocationExpressionSyntax located = site.FirstAncestorOrSelf<InvocationExpressionSyntax>()!;
        if (diagnostic.Properties.ContainsKey(nameof(LetClauseSyntax))) {
            Replaced(editor, located.Parent!, static (current, _) => {
                FromClauseSyntax from = (FromClauseSyntax)current;
                InvocationExpressionSyntax step = (InvocationExpressionSyntax)from.Expression;
                return current.CopyAnnotationsTo(SyntaxFactory.LetClause(
                    SyntaxFactory.Token(from.FromKeyword.LeadingTrivia, SyntaxKind.LetKeyword, from.FromKeyword.TrailingTrivia),
                    from.Identifier,
                    SyntaxFactory.Token(from.InKeyword.LeadingTrivia, SyntaxKind.EqualsToken, from.InKeyword.TrailingTrivia),
                    Kept(step, Lifted(step)))).WithAdditionalAnnotations(Formatter.Annotation);
            });
            return;
        }
        Replaced(editor, located, (current, generator) => {
            InvocationExpressionSyntax call = (InvocationExpressionSyntax)current;
            ExpressionSyntax argument = call.ArgumentList.Arguments[0].Expression;
            if (diagnostic.Properties.ContainsKey(nameof(IParameterReferenceOperation)))
                return current.CopyAnnotationsTo(Extracted(call, ((MemberAccessExpressionSyntax)call.Expression).Expression)).WithAdditionalAnnotations(Formatter.Annotation);
            if (!diagnostic.Properties.TryGetValue(nameof(IMethodSymbol), out string? name) || name is null)
                return current.CopyAnnotationsTo(call.ReplaceNode(argument, Kept(argument, Lifted(call)))).WithAdditionalAnnotations(Formatter.Annotation);
            InvocationExpressionSyntax step = (InvocationExpressionSyntax)((LambdaExpressionSyntax)argument).ExpressionBody!;
            InvocationExpressionSyntax mapped = call.ReplaceNode(step, Kept(step, Lifted(step)));
            MemberAccessExpressionSyntax access = (MemberAccessExpressionSyntax)mapped.Expression;
            SimpleNameSyntax member = diagnostic.Properties.TryGetValue(nameof(ITypeSymbol), out string? type) && type is not null
                ? SyntaxFactory.GenericName(SyntaxFactory.Identifier(name), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ParseTypeName(type)))).WithAdditionalAnnotations(Simplifier.Annotation)
                : Identifier(generator, name);
            return current.CopyAnnotationsTo(mapped.WithExpression(access.WithName(member.WithTriviaFrom(access.Name)))).WithAdditionalAnnotations(Formatter.Annotation);
        });
    }

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
