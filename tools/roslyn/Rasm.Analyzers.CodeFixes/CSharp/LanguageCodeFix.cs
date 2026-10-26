using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Rasm.Analyzers.CSharp;

namespace Rasm.Analyzers.CodeFixes.CSharp;

// --- [COMPOSITION] ---------------------------------------------------------------------
[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public sealed class LanguageCodeFix() : RasmCodeFix((LanguageAnalyzer.Covered, "Delete catch-all arm", static site => site.FirstAncestorOrSelf<SwitchExpressionArmSyntax>() is not null, Delete)) {
    private static void Delete(DocumentEditor editor, SyntaxNode site, Diagnostic diagnostic, CancellationToken cancellation) {
        if (site.FirstAncestorOrSelf<SwitchExpressionArmSyntax>() is { Parent: SwitchExpressionSyntax dispatch } arm)
            editor.ReplaceNode(dispatch, dispatch.WithArms(dispatch.Arms.Remove(arm)).WithAdditionalAnnotations(Formatter.Annotation));
    }
}
