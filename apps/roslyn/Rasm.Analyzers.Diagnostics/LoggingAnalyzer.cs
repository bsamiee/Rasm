using System.Collections.Concurrent;
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

    public static readonly DiagnosticDescriptor Called = Rule(
        "RASM1202",
        Category,
        "Logger called by hand, declare a generated log member",
        "`ILogger.{0}` called with hand-written state, declare {1}",
        "`ILogger.Log` and `ILogger.BeginScope` calls run only in generated code and types implementing `ILogger`.");

    public static readonly DiagnosticDescriptor Twice = Rule(
        "RASM1203",
        Category,
        "Failure reported twice, report once at host translation",
        "`{0}` reports failure `{1}` a second time, report once at the host translation",
        "`IfFail`, `Catch`, and `|` fallbacks report through `ILogger`, `[LoggerMessage]`, or `HostUtils.ExceptionReport` on receiver or handler side alone.",
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Called, Twice];

    protected override void Start(CompilationStartAnalysisContext context) {
        LoggingSymbols symbols = new(context.Compilation);
        if (symbols.Logger is { } logger) Calls(context, logger, symbols.Calls);
        LanguageExtSymbols languageExt = new(context.Compilation);
        if (languageExt.IO?.ContainingAssembly is { } assembly) Fallbacks(new SemanticModels(context), ControlFlow.For(context.Compilation), symbols, Members(context.Compilation.GetTypeByMetadataName("Rhino.Runtime.HostUtils"), "ExceptionReport"), assembly, languageExt.Lifts);
    }

    // --- [SYMBOLS]

    private sealed class LoggingSymbols {
        public LoggingSymbols(Compilation compilation) {
            Logger = compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
            Extensions = compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");
            Calls = Members(Logger, "Log", "BeginScope");
            Message = compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
        }

        public INamedTypeSymbol? Logger { get; }

        public INamedTypeSymbol? Extensions { get; }

        public ImmutableArray<IMethodSymbol> Calls { get; }

        public INamedTypeSymbol? Message { get; }
    }

    // --- [CALLS]

    private static void Calls(CompilationStartAnalysisContext context, INamedTypeSymbol logger, ImmutableArray<IMethodSymbol> members) {
        bool Logs(IMethodSymbol method) => members.Any(member => Implements(method, member));
        bool Forwards(ISymbol symbol) => Containers(symbol).OfType<INamedTypeSymbol>().Any(type => Faces(type, logger).Any());
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

    private static void Fallbacks(SemanticModels models, ControlFlow flow, LoggingSymbols symbols, ImmutableArray<IMethodSymbol> crashes, IAssemblySymbol assembly, ImmutableArray<IMethodSymbol> callbacks) {
        ConcurrentDictionary<IMethodSymbol, ImmutableArray<IMethodSymbol>> calls = new(SymbolEqualityComparer.Default);
        ConcurrentDictionary<IMethodSymbol, bool> reports = new(SymbolEqualityComparer.Default);
        bool Logs(IMethodSymbol method) =>
            method.ReturnsVoid && (symbols.Calls.Any(member => Implements(method, member)) || Same(method.ContainingType, symbols.Extensions));
        bool Reports(IMethodSymbol method, Reading reading) {
            HashSet<IMethodSymbol> seen = [with(SymbolEqualityComparer.Default)];
            Stack<IMethodSymbol> pending = new([method.OriginalDefinition]);
            while (pending.Count is > 0) {
                IMethodSymbol current = pending.Pop();
                reading.Token.ThrowIfCancellationRequested();
                if (!seen.Add(current)) continue;
                if (reports.TryGetValue(current, out bool reported)) {
                    if (reported) return true;
                    continue;
                }
                if (Logs(current) || Marked(current, symbols.Message) is not null || Among(current, crashes)) return true;
                foreach (IMethodSymbol called in calls.GetOrAdd(current, held => [
                    .. Declarations(held, reading).SelectMany(body => ControlFlow.Operations(body, reading.Token)).OfType<IInvocationOperation>()
                        .SelectMany(call => Calls(call, reading, flow, callbacks)).Select(static target => target.OriginalDefinition),
                ])) pending.Push(called);
            }
            foreach (IMethodSymbol visited in seen) _ = reports.TryAdd(visited, value: false);
            return false;
        }
        IInvocationOperation? Reporter(IOperation tree, Reading reading) =>
            ControlFlow.Operations(Lambda(tree) ?? tree, reading.Token).OfType<IInvocationOperation>()
                .FirstOrDefault(call => Calls(call, reading, flow, callbacks).Any(target => reports.GetOrAdd(target.OriginalDefinition, method => Reports(method, reading))));
        static (IOperation Receiver, ImmutableArray<IOperation> Handlers)? Fallback(IOperation operation) =>
            operation switch {
                IInvocationOperation { TargetMethod.Name: "IfFail" or "Catch" } call when Receiver(call) is { } receiver =>
                    (receiver, [.. call.Arguments.Select(static argument => argument.Value).Where(value => value != receiver)]),
                IBinaryOperation { OperatorKind: BinaryOperatorKind.Or, OperatorMethod: not null } either => (either.LeftOperand, [either.RightOperand]),
                _ => null,
            };
        models.Report<IOperation>([OperationKind.Invocation, OperationKind.Binary], (operation, _, reading) =>
            operation switch { IInvocationOperation call => call.TargetMethod, IBinaryOperation either => either.OperatorMethod, _ => null } is { } method && Same(method.ContainingAssembly, assembly) && Fallback(operation) is ( { } receiver, var handlers)
            && Reporter(receiver, reading) is { } first
            && handlers.Select(handler => Reporter(handler, reading)).FirstOrDefault(static again => again is not null) is { } second
                ? [Diagnostic.Create(Twice, operation is IInvocationOperation fallback ? Named(fallback) : operation.Syntax.GetLocation(), Label(second), Label(first))]
                : []);
    }
}
