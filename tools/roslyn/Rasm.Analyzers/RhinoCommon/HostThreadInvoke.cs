using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers.RhinoCommon;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HostThreadInvokeAnalyzer : RasmAnalyzer {
    private static readonly DiagnosticDescriptor Marshal = Rule(
        "RASM2006",
        "RhinoCommon",
        "Single call marshaled to the UI thread",
        "`{0}` marshals one call to the UI thread",
        "Compose UI-thread calls into one `IO` run through `OnUiThread()`, an Eto `Invoke` constructing the `JoinableTaskContext` behind `OnUiThread()` stays exempt.");

    private static readonly ImmutableArray<string> Invokes = ["Invoke", "AsyncInvoke", "InvokeAsync"];

    private static readonly ImmutableArray<string> Dispatches = ["DispatchAsync", "DispatchSync", "DispatchBarrierAsync", "DispatchBarrierSync"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Marshal];

    protected override void Start(CompilationStartAnalysisContext context) {
        Compilation compilation = context.Compilation;
        INamedTypeSymbol? application = compilation.GetTypeByMetadataName("Eto.Forms.Application");
        INamedTypeSymbol? queue = compilation.GetTypeByMetadataName("CoreFoundation.DispatchQueue");
        INamedTypeSymbol? thread = compilation.GetTypeByMetadataName("Foundation.NSThread");
        ImmutableArray<ISymbol> invokes = application is null ? [] : [.. Invokes.SelectMany(name => application.GetMembers(name))];
        ImmutableArray<ISymbol> dispatches = queue is null ? [] : [.. Dispatches.SelectMany(name => queue.GetMembers(name))];
        ImmutableArray<ISymbol> performs = compilation.GetTypeByMetadataName("Foundation.NSObject")?.GetMembers("PerformSelector") ?? [];
        ISymbol? main = queue?.GetMembers("MainQueue").FirstOrDefault();
        ISymbol? mainThread = thread?.GetMembers("MainThread").FirstOrDefault();
        if (invokes.IsEmpty && dispatches.IsEmpty && performs.IsEmpty) return;
        INamedTypeSymbol? threading = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.Threading.JoinableTaskContext");
        static bool Reads(IOperation? value, ISymbol? member) => value is IMemberReferenceOperation reference && Same(reference.Member, member);
        context.RegisterOperationAction(
            operation => {
                IOperation call = operation.Operation;
                (IMethodSymbol? method, IOperation? instance, ImmutableArray<IArgumentOperation> arguments) = call switch {
                    IInvocationOperation invocation => (invocation.TargetMethod, invocation.Instance, invocation.Arguments),
                    IMethodReferenceOperation reference => (reference.Method, reference.Instance, ImmutableArray<IArgumentOperation>.Empty),
                    _ => default,
                };
                if (method is null || InNameOf(call)) return;
                if (invokes.Any(invoke => Same(method, invoke))
                        ? !(threading is not null && arguments.Any(argument => argument.Value.DescendantsAndSelf().Any(created => created is IObjectCreationOperation creation && Same(creation.Type, threading))))
                        : dispatches.Any(dispatch => Same(method, dispatch))
                            ? Reads(instance, main)
                            : performs.Any(perform => Same(method, perform)) && arguments.Any(argument => Reads(argument.Value, mainThread)))
                    operation.ReportDiagnostic(Diagnostic.Create(Marshal, call.Syntax.GetLocation(), method.Name));
            },
            OperationKind.Invocation,
            OperationKind.MethodReference);
    }
}
