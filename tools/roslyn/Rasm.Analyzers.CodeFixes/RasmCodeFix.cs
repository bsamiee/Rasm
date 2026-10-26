using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Rasm.Analyzers.CodeFixes;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class RasmCodeFix(params (DiagnosticDescriptor Rule, string Title, Func<SyntaxNode, bool> Fixable, Action<DocumentEditor, SyntaxNode, Diagnostic, CancellationToken> Edit)[] fixes) : CodeFixProvider {
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [.. fixes.Select(static fix => fix.Rule.Id)];

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root) return;
        foreach (Diagnostic diagnostic in context.Diagnostics)
            if (fixes.First(fix => string.Equals(fix.Rule.Id, diagnostic.Id, StringComparison.Ordinal)) is var (_, title, fixable, edit) && fixable(Site(root, diagnostic.Location)))
                context.RegisterCodeFix(CodeAction.Create(title, token => FixedAsync(context.Document, diagnostic, edit, token), diagnostic.Id), diagnostic);
    }

    protected static SyntaxNode Site(SyntaxNode root, Location location) => root.FindNode(location.SourceSpan, getInnermostNodeForTie: true);

    private static async Task<Document> FixedAsync(Document document, Diagnostic diagnostic, Action<DocumentEditor, SyntaxNode, Diagnostic, CancellationToken> edit, CancellationToken cancellation) {
        DocumentEditor editor = await DocumentEditor.CreateAsync(document, cancellation).ConfigureAwait(false);
        edit(editor, Site(editor.OriginalRoot, diagnostic.Location), diagnostic, cancellation);
        Document simplified = await Simplifier.ReduceAsync(editor.GetChangedDocument(), Simplifier.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
        Document formatted = await Formatter.FormatAsync(simplified, Formatter.Annotation, cancellationToken: cancellation).ConfigureAwait(false);
        return await formatted.GetSyntaxTreeAsync(cancellation).ConfigureAwait(false) is { } tree
            ? formatted.WithSyntaxRoot(await SyntaxFactory.ParseSyntaxTree(await tree.GetTextAsync(cancellation).ConfigureAwait(false), tree.Options, tree.FilePath, cancellation).GetRootAsync(cancellation).ConfigureAwait(false))
            : formatted;
    }
}
