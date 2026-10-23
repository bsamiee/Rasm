using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggingAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Domain = "Microsoft.Extensions.Logging";

    public static readonly DiagnosticDescriptor Defined = Rule(
        "RASM1201",
        Domain,
        "Log message defined by hand",
        "`LoggerMessage.{0}` defines a log message by hand. Declare a `partial void` method under `[LoggerMessage]`",
        "`LoggerMessage.Define` call outside generated code.");

    public static readonly DiagnosticDescriptor Called = Rule(
        "RASM1202",
        Domain,
        "Logger called by hand",
        "`ILogger.{0}` called with hand-written state. Replace with {1}",
        "`ILogger.Log` or `ILogger.BeginScope` call outside generated code and types implementing `ILogger`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Defined, Called];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessage")?.GetMembers("Define").OfType<IMethodSymbol>().ToImmutableArray() is { IsEmpty: false } defines)
            Definitions(context, defines);
        if (context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger") is { } logger)
            Calls(context, logger);
    }

    // --- [DEFINITIONS]
    private static void Definitions(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> defines) =>
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (defines.Any(define => Same(invocation.TargetMethod, define)))
                    analysis.ReportDiagnostic(Diagnostic.Create(Defined, Named(invocation), invocation.TargetMethod.Name));
            },
            OperationKind.Invocation);

    // --- [CALLS]
    private static void Calls(CompilationStartAnalysisContext context, INamedTypeSymbol logger) {
        ImmutableArray<IMethodSymbol> members = [.. logger.GetMembers().OfType<IMethodSymbol>().Where(static member => member.Name is "Log" or "BeginScope")];
        bool Logs(IMethodSymbol method) =>
            members.Any(member => Same(method, member) || Same(method.ContainingType?.FindImplementationForInterfaceMember(member), method));
        bool Forwards(ISymbol? symbol) =>
            symbol is not null && ((symbol is INamedTypeSymbol type && type.AllInterfaces.Any(face => Same(face, logger))) || Forwards(symbol.ContainingSymbol));
        context.RegisterOperationAction(
            analysis => {
                IInvocationOperation invocation = (IInvocationOperation)analysis.Operation;
                if (Logs(invocation.TargetMethod) && !Forwards(analysis.ContainingSymbol))
                    analysis.ReportDiagnostic(Diagnostic.Create(
                        Called,
                        Named(invocation),
                        invocation.TargetMethod.Name,
                        invocation.TargetMethod.ReturnsVoid ? "a `partial void` method under `[LoggerMessage]`" : "a `LoggerMessage.DefineScope` delegate held in a static field"));
            },
            OperationKind.Invocation);
    }
}
