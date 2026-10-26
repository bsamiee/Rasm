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
        "`LoggerMessage.{0}` defines a log message by hand, declare a `partial void` method under `[LoggerMessage]`",
        "`LoggerMessage.Define` call outside generated code.");

    public static readonly DiagnosticDescriptor Called = Rule(
        "RASM1202",
        Category,
        "Logger called by hand",
        "`ILogger.{0}` called with hand-written state, declare {1}",
        "`ILogger.Log` or `ILogger.BeginScope` call outside generated code and types implementing `ILogger`.");

    public static readonly DiagnosticDescriptor Twice = Rule(
        "RASM1203",
        Category,
        "Failure reported twice",
        "`{0}` reports a failure `{1}` already reported, report once at the host translation",
        "`IfFail`, `Catch`, or `|` fallback with an `ILogger` call, `[LoggerMessage]` method, or `HostUtils.ExceptionReport` in both its receiver and its handler.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Defined, Called, Twice];

    protected override void Start(CompilationStartAnalysisContext context) {
        LoggingSymbols symbols = new(context.Compilation);
        if (symbols.Define is { IsEmpty: false } defines) Definitions(context, defines);
        if (symbols.Logger is { } logger) Calls(context, logger, symbols.Calls);
        if (new LanguageExtSymbols(context.Compilation).Namespace is { } space) Fallbacks(context, symbols, new RhinoCommonSymbols(context.Compilation).ExceptionReport, space);
    }

    // --- [DEFINITIONS]
    private static void Definitions(CompilationStartAnalysisContext context, ImmutableArray<IMethodSymbol> defines) =>
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, _) =>
            Among(invocation.TargetMethod, defines) ? [Diagnostic.Create(Defined, Named(invocation), invocation.TargetMethod.Name)] : []);

    // --- [CALLS]
    private static void Calls(CompilationStartAnalysisContext context, INamedTypeSymbol logger, ImmutableArray<IMethodSymbol> members) {
        bool Logs(IMethodSymbol method) => members.Any(member => Implements(method, member));
        bool Forwards(ISymbol? symbol) =>
            symbol is not null && ((symbol is INamedTypeSymbol type && type.AllInterfaces.Any(face => Same(face, logger))) || Forwards(symbol.ContainingSymbol));
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (invocation, analysis) =>
            Logs(invocation.TargetMethod) && !Forwards(analysis.ContainingSymbol)
                ? [Diagnostic.Create(
                    Called,
                    Named(invocation),
                    invocation.TargetMethod.Name,
                    invocation.TargetMethod.ReturnsVoid ? "a `partial void` method under `[LoggerMessage]`" : "a `LoggerMessage.DefineScope` delegate in a static field")]
                : []);
    }

    // --- [FALLBACKS]
    private static void Fallbacks(CompilationStartAnalysisContext context, LoggingSymbols symbols, ImmutableArray<IMethodSymbol> crashes, ISymbol space) {
        Compilation compilation = context.Compilation;
        bool Logs(IMethodSymbol method) =>
            symbols.Logger is { } logger
            && (logger.GetMembers().Any(member => Implements(method, member)) || (method is { IsExtensionMethod: true, Parameters: { IsEmpty: false } parameters } && Same(parameters[0].Type, logger)));
        bool Reports(IMethodSymbol method, Reading reading, int depth) =>
            Logs(method) || Marked(method, symbols.Message) is not null || Among(method, crashes)
            || (depth > 0 && Declarations(method.OriginalDefinition, reading).Any(body => Reporter(body, reading, depth - 1) is not null));
        IInvocationOperation? Reporter(IOperation tree, Reading reading, int depth) =>
            tree.DescendantsAndSelf().OfType<IInvocationOperation>().FirstOrDefault(call => Reports(call.TargetMethod, reading, depth));
        bool Owned(ISymbol method) => method.ContainingNamespace is { } held && Within(compilation.GetCompilationNamespace(held), space);
        static (IOperation Receiver, ImmutableArray<IOperation> Handlers)? Fallback(IOperation operation) =>
            operation switch {
                IInvocationOperation { TargetMethod.Name: "IfFail" or "Catch" } call when Receiver(call) is { } receiver =>
                    (receiver, [.. call.Arguments.Select(static argument => argument.Value).Where(value => value != receiver)]),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Or, OperatorMethod: not null } either => (either.LeftOperand, [either.RightOperand]),
                _ => null,
            };
        Report<IOperation>(context, [OperationKind.Invocation, OperationKind.Binary], (operation, analysis) =>
            operation switch { IInvocationOperation call => call.TargetMethod, IBinaryOperation either => either.OperatorMethod, _ => null } is { } method && Owned(method) && Fallback(operation) is ( { } receiver, var handlers) && operation.SemanticModel is { } model
            && new Reading(model, analysis.CancellationToken) is var reading && Reporter(receiver, reading, 1) is { } first
            && handlers.Select(handler => Reporter(handler, reading, 1)).FirstOrDefault(static again => again is not null) is { } second
                ? [Diagnostic.Create(Twice, operation is IInvocationOperation fallback ? Named(fallback) : operation.Syntax.GetLocation(), Label(second), Label(first))]
                : []);
    }
}
