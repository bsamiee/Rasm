using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------

public abstract class RasmCodeFix(params (string Id, Func<SyntaxNode, SyntaxGenerator, string> Title)[] fixes) : CodeFixProvider {

    // --- [RULES]

    private readonly ImmutableDictionary<string, Func<SyntaxNode, SyntaxGenerator, string>> titles = fixes.ToImmutableDictionary(static fix => fix.Id, static fix => fix.Title, StringComparer.Ordinal);

    // --- [REGISTRATION]

    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [.. fixes.Select(static fix => fix.Id)];

    [SuppressMessage("Roslynator", "RCS1174", Justification = "FixAll requires a nullable task result; the action requires a nonnullable task result.")]
    public sealed override FixAllProvider GetFixAllProvider() =>
        FixAllProvider.Create(async (context, document, diagnostics) =>
            await FixedAsync(document, [.. diagnostics.Where(diagnostic => string.Equals(diagnostic.Id, context.CodeActionEquivalenceKey, StringComparison.Ordinal))], context.CancellationToken).ConfigureAwait(false),
            [.. WellKnownFixAllProviders.BatchFixer.GetSupportedFixAllScopes()]);

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        SyntaxGenerator generator = SyntaxGenerator.GetGenerator(context.Document);
        foreach (Diagnostic diagnostic in context.Diagnostics)
            context.RegisterCodeFix(CodeAction.Create(titles[diagnostic.Id](Site(root, diagnostic.Location), generator), token => FixedAsync(context.Document, [diagnostic], token), diagnostic.Id), diagnostic);
    }

    // --- [SYNTAX]

    protected static SyntaxNode Site(SyntaxNode root, Location location) => root.FindNode(location.SourceSpan, getInnermostNodeForTie: true);

    private protected static SyntaxTriviaList Relocated(IEnumerable<SyntaxTrivia> triviaList) =>
        SyntaxFactory.TriviaList(triviaList.SkipWhile(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia)));

    protected static SyntaxTriviaList Following(IEnumerable<SyntaxTrivia> triviaList) =>
        Relocated(triviaList) switch { { Count: 0 } empty => empty, var trivia => trivia.Insert(0, SyntaxFactory.Space) };

    protected static T Extracted<T>(SyntaxNode owner, T value) where T : SyntaxNode =>
        value.WithLeadingTrivia(owner.GetLeadingTrivia().Concat(Relocated(owner.DescendantTrivia(TextSpan.FromBounds(owner.SpanStart, value.SpanStart)))))
            .WithTrailingTrivia(Following(owner.DescendantTrivia(TextSpan.FromBounds(value.Span.End, owner.Span.End))).Concat(owner.GetTrailingTrivia()));

    private protected static ParenthesizedExpressionSyntax Parenthesized(ExpressionSyntax expression) =>
        SyntaxFactory.ParenthesizedExpression(Punctuation(SyntaxKind.OpenParenToken), expression.WithoutTrivia(), Punctuation(SyntaxKind.CloseParenToken))
            .WithTriviaFrom(expression).WithAdditionalAnnotations(Simplifier.Annotation);

    private protected static SyntaxToken Punctuation(SyntaxKind kind) => SyntaxFactory.Token(default, kind, default);

    private protected static SeparatedSyntaxList<T> Separated<T>(IEnumerable<T> nodes) where T : SyntaxNode =>
        nodes.ToImmutableArray() switch { var items => SyntaxFactory.SeparatedList(items, items.Skip(1).Select(static _ => Punctuation(SyntaxKind.CommaToken))) };

    protected static IdentifierNameSyntax Identifier(SyntaxGenerator generator, string name) => ((IdentifierNameSyntax)generator.IdentifierName(name)).WithoutTrivia();

    private protected static TypeSyntax TypeExpression(SyntaxGenerator generator, ITypeSymbol type) => ((TypeSyntax)generator.TypeExpression(type)).NormalizeWhitespace().WithoutLeadingTrivia();

    protected static MemberAccessExpressionSyntax Access(ExpressionSyntax expression, SimpleNameSyntax name) =>
        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, Punctuation(SyntaxKind.DotToken), name)
            .WithAdditionalAnnotations(Simplifier.Annotation);

    protected static InvocationExpressionSyntax Invoked(ExpressionSyntax expression, IEnumerable<ExpressionSyntax> arguments) =>
        SyntaxFactory.InvocationExpression(expression, SyntaxFactory.ArgumentList(Punctuation(SyntaxKind.OpenParenToken),
            Separated(arguments.Select(static argument => SyntaxFactory.Argument(argument))), Punctuation(SyntaxKind.CloseParenToken)));

    // --- [EDITS]

    protected abstract Task<Document> ChangedAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellation);

    protected static void Replaced(DocumentEditor editor, SyntaxNode site, Func<SyntaxNode, SyntaxGenerator, SyntaxNode> replace) {
        SyntaxToken previous = site.GetFirstToken().GetPreviousToken();
        if (previous.Parent is not { } parent) {
            editor.ReplaceNode(site, replace);
            return;
        }
        SyntaxNode owner = site.Ancestors().First(parent.AncestorsAndSelf().Contains);
        editor.TrackNode(site);
        editor.ReplaceNode(owner, (current, generator) => {
            SyntaxNode at = current.GetCurrentNode(site)!;
            SyntaxToken before = at.GetFirstToken().GetPreviousToken();
            SyntaxNode replacement = replace(at, generator);
            SyntaxTriviaList leading = replacement.GetLeadingTrivia();
            int moved = before.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) ? 0 : leading.TakeWhile(static trivia => !trivia.IsKind(SyntaxKind.EndOfLineTrivia)).Count() + 1;
            return current.ReplaceSyntax([at], (_, _) => replacement.WithLeadingTrivia(leading.Skip(moved)),
                [before], (_, rewritten) => rewritten.WithTrailingTrivia(rewritten.TrailingTrivia.Concat(leading.Take(moved))), trivia: null, computeReplacementTrivia: null);
        });
    }

    private async Task<Document> FixedAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellation) {
        Document changed = await ChangedAsync(document, diagnostics, cancellation).ConfigureAwait(false);
        Document simplified = await Simplifier.ReduceAsync(changed, Simplifier.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
        return await Formatter.FormatAsync(simplified, Formatter.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
    }
}

public abstract class DocumentEditorCodeFix(params (string Id, Func<SyntaxNode, SyntaxGenerator, string> Title, Action<DocumentEditor, SyntaxNode, Diagnostic, CancellationToken> Edit)[] fixes)
    : RasmCodeFix([.. fixes.Select(static fix => (fix.Id, fix.Title))]) {

    // --- [RULES]

    private readonly ImmutableDictionary<string, Action<DocumentEditor, SyntaxNode, Diagnostic, CancellationToken>> edits = fixes.ToImmutableDictionary(static fix => fix.Id, static fix => fix.Edit, StringComparer.Ordinal);

    // --- [EDITS]

    protected sealed override async Task<Document> ChangedAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellation) {
        DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellation).ConfigureAwait(false);
        foreach (Diagnostic diagnostic in diagnostics.OrderBy(static diagnostic => diagnostic.Location.SourceSpan.End).ThenBy(static diagnostic => diagnostic.Location.SourceSpan.Length)) {
            cancellation.ThrowIfCancellationRequested();
            edits[diagnostic.Id](editor, Site(editor.OriginalRoot, diagnostic.Location), diagnostic, cancellation);
        }
        return editor.GetChangedDocument();
    }
}
