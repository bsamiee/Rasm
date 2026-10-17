using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ScriptRunnerCommandStyleAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Style = Rule(
        "RASM2005",
        "RhinoCommon",
        "RunScript in a command without ScriptRunner style",
        "`RhinoApp.RunScript` is called in command `{0}` without `Style.ScriptRunner`",
        "Mark the command `[CommandStyle(Style.ScriptRunner)]`, under which `RunScript` runs its whole script before returning.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Style];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.Commands.Command") is not { } command
            || compilation.GetTypeByMetadataName("Rhino.Commands.CommandStyleAttribute") is not { } attribute
            || compilation.GetTypeByMetadataName("Rhino.Commands.Style")?.GetMembers("ScriptRunner").OfType<IFieldSymbol>().FirstOrDefault()?.ConstantValue is not int runner
            || compilation.GetTypeByMetadataName("Rhino.RhinoApp") is not { } app) return;
        ImmutableArray<ISymbol> scripts = app.GetMembers("RunScript");
        bool Runs(INamedTypeSymbol? type) =>
            type is not null && (type.GetAttributes().FirstOrDefault(data => Same(data.AttributeClass, attribute)) is { } style
                ? style.ConstructorArguments is { Length: 1 } arguments && arguments[0].Value is int styles && (styles & runner) is not 0
                : Runs(type.BaseType));
        context.RegisterOperationAction(
            operation => {
                IInvocationOperation invocation = (IInvocationOperation)operation.Operation;
                if (scripts.Any(script => Same(invocation.TargetMethod, script))
                    && operation.ContainingSymbol.ContainingType is { } owner
                    && Derives(owner, command) && !Runs(owner))
                    operation.ReportDiagnostic(Diagnostic.Create(Style, invocation.Syntax.GetLocation(), owner.Name));
            },
            OperationKind.Invocation);
    }
}
