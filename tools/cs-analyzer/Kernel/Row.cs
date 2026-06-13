using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rasm.Csp.Kernel;

// --- [TYPES] ---------------------------------------------------------------------------

// NAMED delegate, NOT Action<RuleContext>: RuleContext is a ref struct, and a ref struct is
// illegal as a generic type argument on netstandard2.0; a delegate parameter of ref-struct
// type is legal. Check is total; no throw.
internal delegate void RuleCheck(in RuleContext context);

internal enum TriggerKind : byte { Syntax, Operation, Symbol, SymbolStart, CompilationEnd }

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record RuleRow(
    DiagnosticDescriptor Descriptor,
    Tier Tier,
    ScopeGate Scope,
    ImmutableArray<RuleBinding> Bindings);

internal sealed record RuleBinding(TriggerKind Trigger, RuleCheck Check, ImmutableArray<int> Kinds) {
    // The three kind enums erase to one int slot; TriggerKind recovers the meaning.
    public static RuleBinding Syntax(RuleCheck check, params SyntaxKind[] kinds) => new(TriggerKind.Syntax, check, Erase(kinds));
    public static RuleBinding Operation(RuleCheck check, params OperationKind[] kinds) => new(TriggerKind.Operation, check, Erase(kinds));
    public static RuleBinding Symbol(RuleCheck check, params SymbolKind[] kinds) => new(TriggerKind.Symbol, check, Erase(kinds));
    public static RuleBinding SymbolStart(RuleCheck check, SymbolKind kind) => new(TriggerKind.SymbolStart, check, [(int)kind]);
    public static RuleBinding CompilationEnd(RuleCheck check) => new(TriggerKind.CompilationEnd, check, []);
    private static ImmutableArray<int> Erase(SyntaxKind[] kinds) => [.. Array.ConvertAll(kinds, static kind => (int)kind)];
    private static ImmutableArray<int> Erase(OperationKind[] kinds) => [.. Array.ConvertAll(kinds, static kind => (int)kind)];
    private static ImmutableArray<int> Erase(SymbolKind[] kinds) => [.. Array.ConvertAll(kinds, static kind => (int)kind)];
}

internal readonly ref struct RuleContext {
    private readonly DiagnosticDescriptor _descriptor;
    private readonly Action<Diagnostic> _report;
    private readonly Tier _tier;

    internal RuleContext(
        DiagnosticDescriptor descriptor,
        Action<Diagnostic> report,
        Tier tier,
        CompilationFacts facts,
        CspScope scope,
        SyntaxNode? node,
        IOperation? operation,
        ISymbol? symbol,
        SemanticModel? model,
        CancellationToken cancel) {
        _descriptor = descriptor;
        _report = report;
        _tier = tier;
        Facts = facts;
        Scope = scope;
        Node = node;
        Operation = operation;
        Symbol = symbol;
        Model = model;
        Cancel = cancel;
    }

    public SyntaxNode? Node { get; }
    public IOperation? Operation { get; }
    public ISymbol? Symbol { get; }

    // ONLY the context-handed model (RS1030); never Compilation.GetSemanticModel.
    public SemanticModel? Model { get; }
    public CompilationFacts Facts { get; }

    // Resolved per the scope priority chain BEFORE the check runs; the gate is already applied.
    public CspScope Scope { get; }
    public CancellationToken Cancel { get; }

    // Projects messageFormat; display formatting lives ONLY here. The properties bag carries
    // exactly tier/doctrine/scope for the SARIF rail; fact/fix/exempt stay in the message line.
    public void Report(Location location, params object?[] args) =>
        _report(Diagnostic.Create(
            _descriptor,
            location,
            properties: ImmutableDictionary<string, string?>.Empty
                .Add("tier", _tier.ToString())
                .Add("doctrine", _descriptor.Description.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .Add("scope", Scope.ToString()),
            messageArgs: args));
}
