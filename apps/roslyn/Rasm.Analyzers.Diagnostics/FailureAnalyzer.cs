using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Rasm.Analyzers;

// --- [COMPOSITION] ---------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FailureAnalyzer : RasmAnalyzer {

    // --- [RULES]

    private const string Category = "LanguageExt";

    public static readonly DiagnosticDescriptor Predicate = Rule(
        "RASM1013",
        Category,
        "`Catch` predicate bypassed by an evaluated failure, apply it through `@catch`",
        "`Catch` ignores its predicate for an evaluated failure, use `| @catch(...)`",
        "`IO` failure cases bypass the predicate passed to `Catch`, the catch operator applies the predicate before invoking recovery.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Predicate];

    protected override void Start(CompilationStartAnalysisContext context) {
        LanguageExtSymbols symbols = new(context.Compilation);
        ControlFlow flow = ControlFlow.For(context.Compilation);
        if (symbols is not { IO: { } module, IO1: { } io }) return;
        ImmutableArray<IParameterSymbol> predicates = [.. Members(io, "Catch").SelectMany(static method => method.Parameters)
            .Where(static parameter => parameter.Type is INamedTypeSymbol { DelegateInvokeMethod.ReturnType.SpecialType: SpecialType.System_Boolean })];
        ImmutableArray<IMethodSymbol> suspended = [
            .. Members(module, "pure", "pureAsync", "pureVAsync"),
            .. Members(module, "lift", "liftAsync", "liftVAsync").Where(Delegated),
            .. Members(io, "Uninterruptible", "Timeout", "ApplyBack"),
        ];
        ImmutableArray<IMethodSymbol> recoveries = Members(io, "Catch", "IfFail", "Match", "Combine");
        ImmutableArray<IMethodSymbol> narrowed = Members(context.Compilation.GetTypeByMetadataName("LanguageExt.IOExtensions"), "As");
        bool Evaluated(IOperation source, CancellationToken token) {
            Stack<ControlFlow.Value> remaining = new([ControlFlow.At(source)]);
            HashSet<ControlFlow.Value> seen = [];
            while (remaining.Count is not 0) {
                token.ThrowIfCancellationRequested();
                ControlFlow.Value current = remaining.Pop();
                if (!seen.Add(current)) continue;
                IEnumerable<ControlFlow.Value>? next = current.Operation switch {
                    IParenthesizedOperation grouped => [current.With(grouped.Operand)],
                    IConversionOperation { OperatorMethod.ContainingType: { } owner } when Same(owner, io) => [],
                    IConversionOperation { OperatorMethod: null } converted => [current.With(converted.Operand)],
                    IConditionalOperation { WhenFalse: { } absent } conditional => [current.With(conditional.WhenTrue), current.With(absent)],
                    ICoalesceOperation coalesced => [current.With(coalesced.Value), current.With(coalesced.WhenNull)],
                    ISwitchExpressionOperation switched => switched.Arms.Select(arm => current.With(arm.Value)),
                    ILocalReferenceOperation or IFlowCaptureReferenceOperation when flow.Values(current, token) is var values && values.All(static definition => definition.HasValue) => values.Select(static definition => definition.Value),
                    IInvocationOperation call when Among(call.TargetMethod, suspended) => [],
                    IInvocationOperation call when Among(call.TargetMethod, recoveries) => call.Arguments
                        .Where(argument => argument.Parameter?.OriginalDefinition.Type is INamedTypeSymbol parameter &&
                            (Same(parameter, io) || (parameter.DelegateInvokeMethod?.ReturnType is INamedTypeSymbol returned &&
                                (Same(returned, io) || Same(returned, symbols.K)))))
                        .Select(argument => Lambda(argument.Value) is { } handler && Sole(handler.Body) is { } returned ? ControlFlow.At(returned) : current.With(argument.Value)),
                    IInvocationOperation { Instance: { } receiver } call when Same(call.TargetMethod.ContainingType, io) => [current.With(receiver)],
                    IInvocationOperation { Arguments: { Length: 1 } arguments } call when Among(call.TargetMethod, narrowed) => [current.With(arguments[0].Value)],
                    _ => null,
                };
                if (next is null) return true;
                foreach (ControlFlow.Value child in next) remaining.Push(child);
            }
            return false;
        }
        Report<IInvocationOperation>(context, [OperationKind.Invocation], (call, analysis) =>
            call is { Syntax: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax }, Instance: { } source }
            && call.Arguments.FirstOrDefault(argument => Among(argument.Parameter, predicates)) is { } predicate
            && !(Lambda(predicate.Value) is { } function && Sole(function.Body)?.ConstantValue is { HasValue: true, Value: true })
            && Evaluated(source, analysis.CancellationToken)
                ? [Diagnostic.Create(Predicate, call.Syntax.GetLocation())]
                : []);
    }
}
