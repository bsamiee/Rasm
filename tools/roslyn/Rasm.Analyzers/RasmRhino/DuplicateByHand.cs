using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RasmRhino;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DuplicateByHandAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Narrowed = Rule(
        "RASM2103",
        "Rasm.Rhino",
        "Cast or type test on a GeometryBase duplicate",
        "Cast or type test narrows `{0}` to `{1}`",
        "Call `Copies.Duplicate(<source>, <retain>)` or `Copies.DuplicateShallow(<source>, <retain>)`, which return the static type of `<source>` and pass the copy to `<retain>` for disposal.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Narrowed];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.Geometry.GeometryBase") is not { } geometry
            || compilation.GetTypeByMetadataName("Rasm.Rhino.Modeling.Copies") is not { } copies) return;
        ImmutableArray<ISymbol> duplicates = [.. geometry.GetMembers("Duplicate"), .. geometry.GetMembers("DuplicateShallow")];
        bool Copies(IMethodSymbol? method) => method is not null && (duplicates.Any(duplicate => Same(method, duplicate)) || Copies(method.OverriddenMethod));
        context.RegisterOperationAction(
            operation => {
                IInvocationOperation invocation = (IInvocationOperation)operation.Operation;
                if (!Copies(invocation.TargetMethod) || Within(operation.ContainingSymbol, copies) || invocation.Parent is not { } parent) return;
                ITypeSymbol? narrowed = parent switch {
                    IConversionOperation { IsImplicit: false } conversion => conversion.Type,
                    IIsTypeOperation test => test.TypeOperand,
                    IIsPatternOperation test => test.Pattern.NarrowedType,
                    ISwitchExpressionOperation dispatch when dispatch.Value == invocation => dispatch.Arms.Select(static arm => arm.Pattern.NarrowedType).FirstOrDefault(narrowed => !Same(narrowed, invocation.Type)),
                    _ => null,
                };
                if (narrowed is not null && !Same(narrowed, invocation.Type))
                    operation.ReportDiagnostic(Diagnostic.Create(Narrowed, parent.Syntax.GetLocation(), invocation.Syntax, narrowed.Name));
            },
            OperationKind.Invocation);
    }
}
