using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DocumentOffUiThreadAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Reach = Rule(
        "RASM2003",
        "RhinoCommon",
        "RhinoDoc access off the UI thread",
        "`{0}` accesses `RhinoDoc` in a lambda that runs off the UI thread",
        "Access `RhinoDoc` in an `IO` composed with `OnUiThread()`, and pass the lambda values or duplicated geometry read there.");

    private static readonly ImmutableArray<string> Schedulers = [
        "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Threading.Thread", "System.Threading.Tasks.TaskFactory", "System.Threading.Tasks.TaskFactory`1",
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Reach];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        if (compilation.GetTypeByMetadataName("Rhino.RhinoDoc") is not { } document) return;
        ImmutableArray<INamedTypeSymbol> schedulers = [.. Schedulers.Select(compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>()];
        ImmutableArray<ISymbol> runs = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")?.GetMembers("Run") ?? [];
        void Scan(OperationAnalysisContext operation, IMethodSymbol method, ImmutableArray<IArgumentOperation> arguments) {
            if (!schedulers.Any(scheduler => Same(method.ContainingType, scheduler)) && !runs.Any(run => Same(method, run))) return;
            foreach (IOperation reach in arguments
                .SelectMany(static argument => argument.Value.DescendantsAndSelf().OfType<IAnonymousFunctionOperation>().Take(1))
                .SelectMany(static callback => callback.Body.Descendants())
                .Where(reach => reach is not IConversionOperation && Same(reach.Type, document) && !Same(Consumer(reach)?.Type, document)))
                operation.ReportDiagnostic(Diagnostic.Create(Reach, reach.Syntax.GetLocation(), reach.Syntax));
        }
        context.RegisterOperationAction(
            operation => {
                if (operation.Operation is IInvocationOperation invocation) Scan(operation, invocation.TargetMethod, invocation.Arguments);
                else if (operation.Operation is IObjectCreationOperation { Constructor: { } constructor } creation) Scan(operation, constructor, creation.Arguments);
            },
            OperationKind.Invocation,
            OperationKind.ObjectCreation);
    }
}
