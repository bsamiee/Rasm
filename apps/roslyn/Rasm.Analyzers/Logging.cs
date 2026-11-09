using System.Collections.Concurrent;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HandWrittenLogAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1202",
        "Logger called by hand, declare a generated log member",
        "`{0}` called with hand-written state, declare {1}",
        Categories.Logging,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`ILogger.Log` and `ILogger.BeginScope` calls run only in generated code and types implementing `ILogger`, and `LoggerMessage.Define` delegates give way to `[LoggerMessage]` members.",
        Link("RASM1202"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.ILogger") is not { } logger) return;
            ImmutableArray<IMethodSymbol> members = Members(logger, "Log", "BeginScope");
            ImmutableArray<IMethodSymbol> defines = Members(types.GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessage"), "Define");
            const string Generated = "a `partial void` method under `[LoggerMessage]`";
            bool Forwards(ISymbol symbol) => Containers(symbol).OfType<INamedTypeSymbol>().Any(type => Faces(type, logger).Any());
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, analysis) =>
                members.Any(member => Implements(invocation.TargetMethod, member)) && !Forwards(analysis.ContainingSymbol)
                    ? [Diagnostic.Create(Rule, Named(invocation), $"ILogger.{invocation.TargetMethod.Name}",
                        invocation.TargetMethod.ReturnsVoid ? Generated : "a `LoggerMessage.DefineScope` delegate in a static field")]
                    : Among(invocation.TargetMethod, defines) ? [Diagnostic.Create(Rule, Named(invocation), Label(invocation.TargetMethod), Generated)]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoubleReportAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1203",
        "Failure reported twice, report once at host translation",
        "`{0}` reports failure `{1}` a second time, report once at the host translation",
        Categories.Logging,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`IfFail`, `Catch`, and `|` fallbacks report through `ILogger`, `[LoggerMessage]`, or a `RASM1203.reporters` member on receiver or handler side alone.",
        Link("RASM1203"),
        WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            if (types.GetOrCreateTypeByMetadataName("LanguageExt.IO") is not { } io) return;
            INamedTypeSymbol? logger = types.GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
            INamedTypeSymbol? extensions = types.GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");
            INamedTypeSymbol? message = types.GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
            ImmutableArray<IMethodSymbol> logs = Members(logger, "Log", "BeginScope");
            ImmutableArray<IMethodSymbol> crashes = [.. Option(start.Options, "RASM1203.reporters").SelectMany(id => Declared(start.Compilation, id)).OfType<IMethodSymbol>()];
            ImmutableArray<IMethodSymbol> callbacks = Members(io, "lift", "liftAsync", "liftVAsync", "pure");
            ControlFlow flow = ControlFlow.For(start.Compilation);
            ConcurrentDictionary<IMethodSymbol, ImmutableArray<IMethodSymbol>> calls = new(SymbolEqualityComparer.Default);
            ConcurrentDictionary<IMethodSymbol, bool> reports = new(SymbolEqualityComparer.Default);
            bool Logs(IMethodSymbol method) =>
                method.ReturnsVoid && (logs.Any(member => Implements(method, member)) || Same(method.ContainingType, extensions));
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
                    if (Logs(current) || Marked(current, message) is not null || Among(current, crashes)) return true;
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
            new SemanticModels(start).Report<IOperation>([OperationKind.Invocation, OperationKind.Binary], (operation, _, reading) =>
                operation switch { IInvocationOperation call => call.TargetMethod, IBinaryOperation either => either.OperatorMethod, _ => null } is { } method && Same(method.ContainingAssembly, io.ContainingAssembly)
                && Fallback(operation) is ( { } receiver, var handlers)
                && Reporter(receiver, reading) is { } first
                && handlers.Select(handler => Reporter(handler, reading)).FirstOrDefault(static again => again is not null) is { } second
                    ? [Diagnostic.Create(Rule, operation is IInvocationOperation fallback ? Named(fallback) : operation.Syntax.GetLocation(), Label(second), Label(first))]
                    : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticSinkAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Sink = new(
        "RASM1204",
        "Static output sink, log through an `ILogger`",
        "`{0}` writes to static output `{1}`, log through an `ILogger` the composition root creates",
        Categories.Logging,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "`Debug`, `Trace`, `TraceSource`, and `Debugger.Log` write to process-wide sinks no caller configures, an `ILogger` from the composition root carries the output.",
        Link("RASM1204"));

    private static readonly DiagnosticDescriptor Console = new(
        "RASM1206",
        "Console output outside a composition root, log through an `ILogger`",
        "`{0}` writes to `Console`, log through an `ILogger` the composition root creates and let the application compose its console sink",
        Categories.Logging,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Library code writes no `Console` output, the application composes a console sink behind the `ILogger` it hands down, executables are composition roots and path rows name others.",
        Link("RASM1206"));

    private static readonly ImmutableArray<string> Sinks = ["System.Diagnostics.Debug", "System.Diagnostics.Trace", "System.Diagnostics.TraceSource"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Sink, Console];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            WellKnownTypeProvider types = WellKnownTypeProvider.GetOrCreate(start.Compilation);
            ImmutableArray<INamedTypeSymbol> sinks = [.. Sinks.Select(types.GetOrCreateTypeByMetadataName).OfType<INamedTypeSymbol>()];
            ImmutableArray<IMethodSymbol> logs = Members(types.GetOrCreateTypeByMetadataName("System.Diagnostics.Debugger"), "Log");
            INamedTypeSymbol? console = start.Compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication or OutputKind.WindowsRuntimeApplication
                ? null : types.GetOrCreateTypeByMetadataName("System.Console");
            Report<IOperation>(start, [OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.FieldReference, OperationKind.EventReference, OperationKind.MethodReference, OperationKind.ObjectCreation], (operation, _) =>
                (operation switch { IObjectCreationOperation creation => creation.Constructor, _ => Target(operation) }) is not { } member || Inside<INameOfOperation>(operation) ? []
                : Same(member.ContainingType, console) ? [Diagnostic.Create(Console, operation.Syntax.GetLocation(), operation.Syntax)]
                : Among(member.ContainingType, sinks) || Among(member, logs) ? [Diagnostic.Create(Sink, operation.Syntax.GetLocation(), operation.Syntax, member.ContainingType.Name)]
                : []);
        });
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggerCategoryAnalyzer : DiagnosticAnalyzer {

    // --- [RULES]

    private static readonly DiagnosticDescriptor Rule = new(
        "RASM1205",
        "String logger category, derive the category from a type",
        "`{0}` names a logger category by string, call `CreateLogger<T>()` or `CreateLogger(Type)`",
        Categories.Logging,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        "Logger categories derive from the type that logs through `CreateLogger<T>()` or `CreateLogger(Type)`, a string category drifts from the type it names.",
        Link("RASM1205"));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            ImmutableArray<IMethodSymbol> named = [.. Members(WellKnownTypeProvider.GetOrCreate(start.Compilation).GetOrCreateTypeByMetadataName("Microsoft.Extensions.Logging.ILoggerFactory"), "CreateLogger")
                .Where(static method => method.Parameters is { Length: 1 } parameters && parameters[0].Type.SpecialType is SpecialType.System_String)];
            if (named.IsEmpty) return;
            Report<IInvocationOperation>(start, [OperationKind.Invocation], (invocation, _) =>
                named.Any(member => Implements(invocation.TargetMethod, member))
                    ? [Diagnostic.Create(Rule, Named(invocation), invocation.Syntax)]
                    : []);
        });
    }
}
