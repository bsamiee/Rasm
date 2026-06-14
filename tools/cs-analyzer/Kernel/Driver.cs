using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rasm.Csp.Kernel;

// --- [SERVICES] ------------------------------------------------------------------------

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Driver : DiagnosticAnalyzer {
    // --- [TABLES]

    // Build one trigger registration per Roslyn kind; row dispatch stays array-backed.
    private static readonly ImmutableArray<KindBucket> SyntaxBuckets = Buckets(TriggerKind.Syntax);
    private static readonly ImmutableArray<KindBucket> OperationBuckets = Buckets(TriggerKind.Operation);
    private static readonly ImmutableArray<KindBucket> SymbolBuckets = Buckets(TriggerKind.Symbol);
    private static readonly ImmutableArray<KindBucket> SymbolStartBuckets = Buckets(TriggerKind.SymbolStart);
    private static readonly ImmutableArray<Slot> CompilationEndSlots = EndSlots();

    // --- [EXPORTS]

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Descriptors();

    // --- [ENTRY]

    public override void Initialize(AnalysisContext context) {
        _ = context ?? throw new ArgumentNullException(paramName: nameof(context));
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static start => {
            CompilationFacts facts = new(start);
            foreach (KindBucket bucket in SyntaxBuckets) {
                ImmutableArray<Slot> slots = bucket.Slots;
                start.RegisterSyntaxNodeAction(syntaxContext => DispatchSyntax(syntaxContext, facts, slots), (SyntaxKind)bucket.Kind);
            }
            foreach (KindBucket bucket in OperationBuckets) {
                ImmutableArray<Slot> slots = bucket.Slots;
                start.RegisterOperationAction(operationContext => DispatchOperation(operationContext, facts, slots), (OperationKind)bucket.Kind);
            }
            foreach (KindBucket bucket in SymbolBuckets) {
                ImmutableArray<Slot> slots = bucket.Slots;
                start.RegisterSymbolAction(symbolContext => DispatchSymbol(symbolContext, facts, slots), (SymbolKind)bucket.Kind);
            }
            foreach (KindBucket bucket in SymbolStartBuckets) {
                ImmutableArray<Slot> slots = bucket.Slots;
                start.RegisterSymbolStartAction(symbolStartContext => symbolStartContext.RegisterSymbolEndAction(endContext => DispatchSymbol(endContext, facts, slots)), (SymbolKind)bucket.Kind);
            }
            // Compilation-end checks are batch-build-only; IDE live analysis skips them.
            if (!CompilationEndSlots.IsEmpty) start.RegisterCompilationEndAction(endContext => DispatchCompilationEnd(endContext, facts, CompilationEndSlots));
        });
    }

    // --- [OPERATIONS]

    private static ImmutableArray<KindBucket> Buckets(TriggerKind trigger) {
        Dictionary<int, ImmutableArray<Slot>.Builder> groups = [];
        foreach (RuleRow row in Catalog.All) {
            foreach (RuleBinding binding in row.Bindings) {
                if (binding.Trigger != trigger) continue;
                foreach (int kind in binding.Kinds) {
                    if (!groups.TryGetValue(kind, out ImmutableArray<Slot>.Builder? slots)) {
                        slots = ImmutableArray.CreateBuilder<Slot>();
                        groups.Add(kind, slots);
                    }
                    slots.Add(new Slot(row, binding.Check));
                }
            }
        }
        ImmutableArray<KindBucket>.Builder buckets = ImmutableArray.CreateBuilder<KindBucket>(groups.Count);
        foreach (KeyValuePair<int, ImmutableArray<Slot>.Builder> group in groups)
            buckets.Add(new KindBucket(group.Key, group.Value.ToImmutable()));
        return buckets.ToImmutable();
    }

    private static ImmutableArray<DiagnosticDescriptor> Descriptors() {
        ImmutableArray<DiagnosticDescriptor>.Builder descriptors = ImmutableArray.CreateBuilder<DiagnosticDescriptor>(Catalog.All.Length);
        foreach (RuleRow row in Catalog.All) descriptors.Add(row.Descriptor);
        return descriptors.ToImmutable();
    }

    private static void DispatchCompilationEnd(CompilationAnalysisContext context, CompilationFacts facts, ImmutableArray<Slot> slots) {
        SyntaxTree? tree = FirstTree(context.Compilation);
        if (tree is null) return;
        // Whole-compilation rules gate on assembly scope; per-tree overrides cannot apply.
        CspScope scope = facts.ScopeOf(tree, context.Compilation.Assembly);
        ScopeGate gate = GateOf(scope);
        foreach (Slot slot in slots) {
            if ((slot.Row.Scope & gate) == 0) continue;
            slot.Check(new RuleContext(
                descriptor: slot.Row.Descriptor,
                report: context.ReportDiagnostic,
                tier: slot.Row.Tier,
                facts: facts,
                scope: scope,
                node: null,
                operation: null,
                symbol: context.Compilation.Assembly,
                model: null,
                cancel: context.CancellationToken));
        }
    }

    private static void DispatchOperation(OperationAnalysisContext context, CompilationFacts facts, ImmutableArray<Slot> slots) {
        CspScope scope = facts.ScopeOf(context.Operation.Syntax.SyntaxTree, context.ContainingSymbol);
        ScopeGate gate = GateOf(scope);
        foreach (Slot slot in slots) {
            if ((slot.Row.Scope & gate) == 0) continue;
            slot.Check(new RuleContext(
                descriptor: slot.Row.Descriptor,
                report: context.ReportDiagnostic,
                tier: slot.Row.Tier,
                facts: facts,
                scope: scope,
                node: context.Operation.Syntax,
                operation: context.Operation,
                symbol: context.ContainingSymbol,
                model: context.Operation.SemanticModel,
                cancel: context.CancellationToken));
        }
    }

    private static void DispatchSymbol(SymbolAnalysisContext context, CompilationFacts facts, ImmutableArray<Slot> slots) {
        if (context.Symbol.DeclaringSyntaxReferences.IsEmpty) return;
        CspScope scope = facts.ScopeOf(context.Symbol.DeclaringSyntaxReferences[0].SyntaxTree, context.Symbol);
        ScopeGate gate = GateOf(scope);
        foreach (Slot slot in slots) {
            if ((slot.Row.Scope & gate) == 0) continue;
            slot.Check(new RuleContext(
                descriptor: slot.Row.Descriptor,
                report: context.ReportDiagnostic,
                tier: slot.Row.Tier,
                facts: facts,
                scope: scope,
                node: null,
                operation: null,
                symbol: context.Symbol,
                model: null,
                cancel: context.CancellationToken));
        }
    }

    private static void DispatchSyntax(SyntaxNodeAnalysisContext context, CompilationFacts facts, ImmutableArray<Slot> slots) {
        CspScope scope = facts.ScopeOf(context.Node.SyntaxTree, context.ContainingSymbol);
        ScopeGate gate = GateOf(scope);
        foreach (Slot slot in slots) {
            if ((slot.Row.Scope & gate) == 0) continue;
            slot.Check(new RuleContext(
                descriptor: slot.Row.Descriptor,
                report: context.ReportDiagnostic,
                tier: slot.Row.Tier,
                facts: facts,
                scope: scope,
                node: context.Node,
                operation: null,
                symbol: context.ContainingSymbol,
                model: context.SemanticModel,
                cancel: context.CancellationToken));
        }
    }

    private static ImmutableArray<Slot> EndSlots() {
        ImmutableArray<Slot>.Builder slots = ImmutableArray.CreateBuilder<Slot>();
        foreach (RuleRow row in Catalog.All)
            foreach (RuleBinding binding in row.Bindings)
                if (binding.Trigger == TriggerKind.CompilationEnd) slots.Add(new Slot(row, binding.Check));
        return slots.ToImmutable();
    }

    private static SyntaxTree? FirstTree(Compilation compilation) {
        foreach (SyntaxTree tree in compilation.SyntaxTrees) return tree;
        return null;
    }

    private static ScopeGate GateOf(CspScope scope) => (ScopeGate)(1 << (int)scope);

    // --- [MODELS]

    private sealed record KindBucket(int Kind, ImmutableArray<Slot> Slots);

    private sealed record Slot(RuleRow Row, RuleCheck Check);
}
