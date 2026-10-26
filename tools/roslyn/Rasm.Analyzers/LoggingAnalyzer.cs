using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggingAnalyzer : RasmAnalyzer {
    // --- [RULES]
    private const string Category = "Microsoft.Extensions.Logging";

    public static readonly DiagnosticDescriptor Defined = Rule(
        "RASM1201",
        Category,
        "Log message defined by hand",
        "`LoggerMessage.{0}` defines a log message by hand. Declare a `partial void` method under `[LoggerMessage]`",
        "`LoggerMessage.Define` call outside generated code.");

    public static readonly DiagnosticDescriptor Called = Rule(
        "RASM1202",
        Category,
        "Logger called by hand",
        "`ILogger.{0}` called with hand-written state. Replace with {1}",
        "`ILogger.Log` or `ILogger.BeginScope` call outside generated code and types implementing `ILogger`.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Defined, Called];

    protected override void Start(CompilationStartAnalysisContext context) {
        if (Members(context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessage"), "Define") is { IsEmpty: false } defines)
            Definitions(context, defines);
        if (context.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger") is { } logger)
            Calls(context, logger);
    }

    // --- [DEFINITIONS]
    private static void Definitions(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> defines) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, defines) ? [Diagnostic.Create(Defined, Named(invocation), invocation.TargetMethod.Name)] : []);

    // --- [CALLS]
    private static void Calls(CompilationStartAnalysisContext context, INamedTypeSymbol logger) {
        ImmutableArray<IMethodSymbol> members = Members(logger, "Log", "BeginScope");
        bool Logs(IMethodSymbol method) => members.Any(member => Implements(method, member));
        bool Forwards(ISymbol? symbol) =>
            symbol is not null && ((symbol is INamedTypeSymbol type && type.AllInterfaces.Any(face => Same(face, logger))) || Forwards(symbol.ContainingSymbol));
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Logs(invocation.TargetMethod) && !Forwards(analysis.ContainingSymbol)
                ? [Diagnostic.Create(
                    Called,
                    Named(invocation),
                    invocation.TargetMethod.Name,
                    invocation.TargetMethod.ReturnsVoid ? "a `partial void` method under `[LoggerMessage]`" : "a `LoggerMessage.DefineScope` delegate held in a static field")]
                : []);
    }
}
