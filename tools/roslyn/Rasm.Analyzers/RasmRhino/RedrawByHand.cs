using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RasmRhino;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedrawByHandAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Redraw = Rule(
        "RASM2102",
        "Rasm.Rhino",
        "View redraw outside Commits",
        "`{0}` controls view redraw outside `Commits`",
        "Pass a `RedrawPolicy` to `Commits.Commit` or `Commits.WithinRedraw`, which apply it after the body on every path and restore prior redraw state.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Redraw];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.DocObjects.Tables.ViewTable") is not { } views
            || compilation.GetTypeByMetadataName("Rhino.Display.RhinoView") is not { } view
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Document.Commits") is not { } commits) return;
        ImmutableArray<ISymbol> redraws = [.. views.GetMembers("Redraw"), .. views.GetMembers("EnableRedraw"), .. view.GetMembers("Redraw")];
        context.RegisterOperationAction(
            operation => {
                IOperation call = operation.Operation;
                if (call switch { IInvocationOperation invocation => invocation.TargetMethod, IMethodReferenceOperation reference => reference.Method, _ => null } is { } method
                    && redraws.Any(redraw => Same(method, redraw)) && !InNameOf(call) && !Within(operation.ContainingSymbol, commits))
                    operation.ReportDiagnostic(Diagnostic.Create(Redraw, call.Syntax.GetLocation(), call.Syntax));
            },
            OperationKind.Invocation,
            OperationKind.MethodReference);
    }
}
