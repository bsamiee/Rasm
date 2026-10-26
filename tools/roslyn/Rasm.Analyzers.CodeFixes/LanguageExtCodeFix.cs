using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public sealed class LanguageExtCodeFix() : RasmCodeFix(
    (LanguageExtAnalyzer.Lift, static site => LanguageExtAnalyzer.Unlifted(site) is { } value ? FormattableString.Invariant($"Replace `{site}` with `{value}`") : null, Unlift),
    (LanguageExtAnalyzer.RoundTrip, static site => site.FirstAncestorOrSelf<InvocationExpressionSyntax>() is { } exit ? FormattableString.Invariant($"Delete `{(SyntaxNode?)Called(exit) ?? exit.Expression}()` round trip") : null, Unwrap),
    (LanguageExtAnalyzer.Spread, static site => site is CollectionExpressionSyntax { Elements: { Count: 2 } elements } && elements[0] is SpreadElementSyntax spread && elements[1] is ExpressionElementSyntax item ? FormattableString.Invariant($"Replace `{site}` with `{spread.Expression}.Add({item})`") : null, Append),
    (LanguageExtAnalyzer.LiftedApply, Lowered, Lower),
    (LanguageExtAnalyzer.Narrowed, static site => Called(site) is { } narrowing ? FormattableString.Invariant($"Delete `{narrowing}()`") : null, Unnarrow)) {
    // --- [CALLS]
    private static SimpleNameSyntax? Called(SyntaxNode site) => (site.FirstAncestorOrSelf<InvocationExpressionSyntax>()?.Expression as MemberAccessExpressionSyntax)?.Name;

    // --- [NARROWINGS]
    private static void Unnarrow(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        editor.ReplaceNode(site.FirstAncestorOrSelf<InvocationExpressionSyntax>()!, static (current, _) => current is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access }
            ? access.Expression.WithTriviaFrom(current).WithAdditionalAnnotations(Simplifier.Annotation)
            : current);

    // --- [LIFTS]
    private static void Unlift(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        editor.ReplaceNode(site, static (current, _) => LanguageExtAnalyzer.Unlifted(current) is { } value
            ? SyntaxFactory.ParenthesizedExpression(value.WithoutTrivia()).WithTriviaFrom(current).WithAdditionalAnnotations(Simplifier.Annotation)
            : current);

    // --- [ROUND_TRIPS]
    private static void Unwrap(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) {
        foreach ((SyntaxNode hop, bool body) in diagnostic.AdditionalLocations.Select(location => Site(editor.OriginalRoot, location)).Select(static node => (node, node.Parent is LambdaExpressionSyntax)))
            editor.ReplaceNode(hop, (current, _) => current switch {
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } when body => Unwrapped(access.Expression).WithoutTrivia().WithTriviaFrom(current).WithAdditionalAnnotations(Formatter.Annotation),
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax access } => SyntaxFactory.ParenthesizedExpression(Unwrapped(access.Expression).WithoutTrivia()).WithTriviaFrom(current).WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation),
                _ => current,
            });
    }

    // --- [SPREADS]
    private static void Append(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) =>
        editor.ReplaceNode(site, static (current, generator) => current is CollectionExpressionSyntax { Elements: { Count: 2 } elements } && elements[0] is SpreadElementSyntax spread && elements[1] is ExpressionElementSyntax item
            ? generator.InvocationExpression(generator.MemberAccessExpression(spread.Expression.WithoutTrivia(), "Add"), item.Expression.WithoutTrivia()).WithTriviaFrom(current).WithAdditionalAnnotations(Formatter.Annotation)
            : current);

    // --- [LIFTED_APPLIES]
    private static ImmutableArray<InvocationExpressionSyntax> Lifts(SyntaxNode apply) =>
        apply is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: var receiver } } && Unwrapped(receiver) is TupleExpressionSyntax tuple
            ? [.. tuple.Arguments.Select(static argument => Unwrapped(argument.Expression)).OfType<InvocationExpressionSyntax>()]
            : [];

    private static ExpressionSyntax Unwrapped(ExpressionSyntax expression) => expression is ParenthesizedExpressionSyntax grouped ? Unwrapped(grouped.Expression) : expression;

    private static string? Lowered(SyntaxNode site) =>
        site.FirstAncestorOrSelf<InvocationExpressionSyntax>() is { Expression: MemberAccessExpressionSyntax access } apply && Lifts(apply) is { IsEmpty: false } lifts
        && lifts.All(static lift => lift.ArgumentList.Arguments.Count is 1 && lift.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax { ExpressionBody: null })
            ? FormattableString.Invariant($"Call `{access.Name}` inside one `{lifts[0].Expression}`")
            : null;

    private static void Lower(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) {
        if (site.FirstAncestorOrSelf<InvocationExpressionSyntax>() is not { } apply) return;
        SemanticModel model = editor.SemanticModel;
        ImmutableArray<ExpressionSyntax> values = [
            .. Lifts(apply).Select(lift => lift.ArgumentList.Arguments[0].Expression switch {
                LambdaExpressionSyntax lambda => lambda.ExpressionBody!,
                var value when model.GetTypeInfo(value, cancellation).ConvertedType is INamedTypeSymbol { TypeKind: TypeKind.Delegate } => SyntaxFactory.InvocationExpression(value.WithoutTrivia()),
                var value => value,
            }),
        ];
        bool captureless = Lifts(apply).All(static lift => lift.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax { Modifiers: var modifiers } && modifiers.Any(SyntaxKind.StaticKeyword));
        SyntaxNode replaced = apply.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0 } narrowed } ? narrowed : apply;
        editor.ReplaceNode(replaced, (current, generator) => {
            ExpressionSyntax applied = (ExpressionSyntax)generator.InvocationExpression(generator.MemberAccessExpression(
                generator.InvocationExpression(generator.MemberAccessExpression(generator.TupleExpression(values.Select(value => generator.Argument(value.WithoutTrivia()))), "Apply"), apply.ArgumentList.Arguments),
                "As"));
            ParenthesizedLambdaExpressionSyntax thunk = SyntaxFactory.ParenthesizedLambdaExpression(applied);
            return SyntaxFactory.InvocationExpression(Lifts(apply)[0].Expression.WithoutTrivia(), SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(captureless ? thunk.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword)) : thunk))))
                .WithTriviaFrom(current).WithAdditionalAnnotations(Formatter.Annotation);
        });
    }
}
