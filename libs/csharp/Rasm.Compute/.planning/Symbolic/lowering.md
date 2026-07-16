# [SYMBOLIC_LOWERING]

Compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` lowers once to a native delegate through the engine's typed `Compile<TIn1..TIn8, TOut>(vars)` IL-compiling surface (arities one through eight instantiate `double` and lower through the LINQ-expression protocol) with the interpreter `Compile(params Variable[]) → FastExpression` absorbing every arity past eight, carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` mints, and reused through a `LoweringCache` read-through over the one model-lane `HybridCache` (`Model/inference#RESULT_CACHE` `CacheLane.ModelResult`, never a second instance). Owned here: the `CompiledExpr` carrier, the `CompileArity` `[SmartEnum<string>]` that selects and owns the arity-exact compile-and-invoke behavior (one delegate-backed row per arity, the variadic row wrapping the `FastExpression` interpreter in a `Func<double[], double>`), the `LoweringCache` L1-only read-through with its `LoweringSlot` carrier and `CompiledKey` derivation composing the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` law, and the cross-lane `SymbolicJacobian` that differentiates a formula by each free design symbol, compiles each partial, and packs the partials WITH the design point into a `SymbolicTape` whose `SymbolicAdjoint.Chain` answers the same two-argument reverse-mode contract `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain` answers — the design point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`. Symbolic gradients enter as the additive `DesignVariable.Symbolic` arm the optimizer admits — its lowering yielding a `SymbolicTape`, its adjoint routing `SymbolicAdjoint.Chain` — never a standalone `GradientSource` and never a parallel `(Seq<double>, double)` path (that pair is the `Surrogate.Predict` RETURN shape, never the gradient contract).

Lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before: a formula compiles and registers a Jacobian only after `DimensionProof` admits, so a dimension-inconsistent expression never reaches a `CompiledExpr` slot, the optimizer oracle, or the integrator seed. A formula carrying no free design symbol lowers an empty `SymbolicTape` and `DescendAdjoint` falls to finite-difference exactly as the absent-`DesignMesh` case lowers an empty geometry tape, so the symbolic source admits without breaking that invariant or adding a parallel descent. Host-local, no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior value, and the only cross-surface fact is the `SymbolicExpr.ContentKey` the `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` seam crosses by reference to the `Rasm.Persistence/Query/cache#MODEL_RESULT_INDEX` cost-catalog/QTO consumers, keyed by its OWN content identity, never a fabricated `ModelResultKey`. Compiled delegates are ALC-safe yet not durably serializable, so the cache entry is L1-only by construction — cross-process reuse is deterministic re-lowering off the content-addressed key, never a serialized delegate. An in-proc symbolic-regression fit is the rejected form: offline formula discovery is the Python branch's, and compile-and-cache plus the analytic-Jacobian lowering over an already-admitted expression is all this owner holds.

## [01]-[INDEX]

- [01]-[LOWERING]: `CompiledExpr` delegate carrier; `CompileArity` delegate-backed arity owner (compile + invoke in one row); the typed `Compile<>`/interpreter `FastExpression` lowering.
- [02]-[LOWERING_CACHE]: `LoweringCache` L1-only read-through over `CacheLane.ModelResult`; `LoweringSlot` `[ImmutableObject]` carrier; `CompiledKey` content-key derivation.
- [03]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering; `SymbolicTape` (design point baked in) / `SymbolicAdjoint` two-argument transpose `Chain`.

## [02]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a lowered native delegate to its source content key and ordered free-symbol vector; `CompileArity` the `[SmartEnum<string>]` whose ten rows (`nullary` … `octonary`, `variadic`) each own both compile form and invoke form as delegate-backed behavior — a typed row's `Lower` instantiates the arity-exact `Entity.Compile<double, …>(vars)` generic, its `Invoke` performs the strongly-typed down-cast, and the variadic row's `Lower` wraps the `Entity.Compile(params Variable[]) → FastExpression` interpreter in one `Func<double[], double>` closure so no `FastExpression` leaks past the row; `CompileCapsule` the one boundary owner gating the lowering — it pre-validates no analytic residue (`Derivativef`/`Integralf`/`Limitf`) and no non-numeric node (`Set`/`Statement`), then converts the engine's compile throw onto the `ComputeFault.NonDifferentiable` rail.
- Cases: `nullary` (rank 0 — a constant evaluated once, no delegate), `unary` … `octonary` (ranks 1–8, the engine's COMPLETE strongly-typed generic set, each `Func<double,…,double>`), `variadic` (rank −1, the `FastExpression` interpreter behind a `Func<double[], double>`, reached only past eight); the row is `CompileArity.Select(symbolOrder.Count)`, never a call-site branch.
- Entry: `Compile(SymbolicExpr, Seq<string> symbolOrder)` is the one polymorphic lowering — the symbol order fixes the positional argument convention (the i-th `double` binds the i-th symbol), `Select(Count)` picks the row, `arity.Lower(entity, variables)` returns the boxed `Delegate`, and the capsule's residue gate plus exception seam lift the outcome onto the `Fin` rail; `Invoke(ImmutableArray<double>)` validates the argument count against the symbol order (a mis-arity call is a `ComputeFault`, never an engine index fault) then delegates to `Arity.Invoke`, so the down-cast and the variadic array bind are both owned by the row.
- Auto: `Compile` reads the ordered `symbolOrder` rather than `FreeSymbols` directly, so the positional convention is caller-fixed and stable across a re-compile; the typed rows hold the exact `Func<…>` the generic `Compile<>` returns, so those call sites invoke a strongly-typed compiled-IL delegate with no reflection — the interpreter path is reached only past eight symbols; the nullary row never compiles a delegate — a free-symbol-empty formula evaluates once through `Symbolic/expression#SYMBOLIC_EXPR` `Evaluate` and the `CompiledExpr` carries the cached constant.
- Receipt: none of its own — the compile outcome rides the `LOWERING_CACHE` hit/miss/store slot the model lane's `ComputeReceipt.Cache` fact stamps, a compile-decline rides the `ComputeFault.NonDifferentiable` 2215 arm, and the downstream optimize outcome carries the `Optimization` slot.
- Packages: AngouriMath (`Entity.Compile<TIn1..TIn8, TOut>(Variable…)` typed IL lowering, `Entity.Compile(params Variable[]) → FastExpression` interpreter with `Call(params Complex[]) → Complex`, `MathS.Var`), Thinktecture.Runtime.Extensions (`CompileArity` `[SmartEnum]`), LanguageExt.Core (`Fin`, `Seq`/`ImmutableArray`/`Option`), BCL inbox (`System.Numerics.Complex` at the interpreter marshal).
- Growth: a new arity past eight is impossible as a typed row — the variadic interpreter absorbs every arity of nine or more, and the eight typed generics are the complete set the engine ships; a new numeric domain (a complex-valued lane) is one companion-row family instantiating `Complex` type arguments on the SAME generic surface, never a parallel `CompiledComplexExpr`; a new evaluation convention (a `Span<double>` bind for a hot loop) is one more `Invoke` shape on the same row.
- Boundary: `Compile` is the single lowering entry — a `CompileUnary`/`CompileBinary`/`CompileVariadic` factory trio is the collapsed defect, and the two-parallel-switch shape (one for compile, one for invoke) is what the delegate-backed rows collapse; the typed generic `Compile<>` is the admitted fast path lowering to IL through the engine's LINQ-expression protocol, and a hand-rolled `Reflection.Emit` or expression-tree re-implementation is the deleted form; the residue gate runs BEFORE the engine compile so the throwing seam is reached only by genuinely un-compilable nodes, and that one `try` is the named platform-forced exception exemption; the positional symbol order is the one argument convention, and an unordered `Map<string,double>`-keyed invoke is rejected because the compiled delegate is positional by construction.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompileArity {
    public static readonly CompileArity Nullary = new("nullary", 0,
        lower: static (_, _) => NoOp,
        invoke: static (_, _) => double.NaN);
    public static readonly CompileArity Unary = new("unary", 1,
        lower: static (e, s) => e.Compile<double, double>(s[0]),
        invoke: static (d, a) => ((Func<double, double>)d)(a[0]));
    public static readonly CompileArity Binary = new("binary", 2,
        lower: static (e, s) => e.Compile<double, double, double>(s[0], s[1]),
        invoke: static (d, a) => ((Func<double, double, double>)d)(a[0], a[1]));
    public static readonly CompileArity Ternary = new("ternary", 3,
        lower: static (e, s) => e.Compile<double, double, double, double>(s[0], s[1], s[2]),
        invoke: static (d, a) => ((Func<double, double, double, double>)d)(a[0], a[1], a[2]));
    public static readonly CompileArity Quaternary = new("quaternary", 4,
        lower: static (e, s) => e.Compile<double, double, double, double, double>(s[0], s[1], s[2], s[3]),
        invoke: static (d, a) => ((Func<double, double, double, double, double>)d)(a[0], a[1], a[2], a[3]));
    public static readonly CompileArity Quinary = new("quinary", 5,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4]),
        invoke: static (d, a) => ((Func<double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4]));
    public static readonly CompileArity Senary = new("senary", 6,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5]),
        invoke: static (d, a) => ((Func<double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5]));
    public static readonly CompileArity Septenary = new("septenary", 7,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6]),
        invoke: static (d, a) => ((Func<double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6]));
    public static readonly CompileArity Octonary = new("octonary", 8,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]),
        invoke: static (d, a) => ((Func<double, double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7]));
    public static readonly CompileArity Variadic = new("variadic", -1,
        lower: static (e, s) => Interpret(e.Compile(s)),
        invoke: static (d, a) => ((Func<double[], double>)d)([.. a]));

    private readonly int rank;
    private readonly Func<Entity, Entity.Variable[], Delegate> lower;
    private readonly Func<Delegate, ImmutableArray<double>, double> invoke;

    public int Rank => rank;

    // Symbol-order count selects the row; typed compiled-IL to eight, the FastExpression interpreter absorbing nine-plus.
    public static CompileArity Select(int symbolCount) =>
        symbolCount switch {
            0 => Nullary, 1 => Unary, 2 => Binary, 3 => Ternary, 4 => Quaternary,
            5 => Quinary, 6 => Senary, 7 => Septenary, 8 => Octonary, _ => Variadic,
        };

    public Delegate Lower(Entity entity, Entity.Variable[] variables) => lower(entity, variables);

    public double Invoke(Delegate evaluator, ImmutableArray<double> arguments) => invoke(evaluator, arguments);

    // Interpreter stays behind the row: one closure marshals double[] -> Complex[] -> Call -> Real, so the carrier is uniformly a Delegate.
    static Func<double[], double> Interpret(FastExpression fast) =>
        args => fast.Call([.. args.Select(static x => new Complex(x, 0))]).Real;

    static readonly Func<double> NoOp = static () => double.NaN;
}

// --- [MODELS] ----------------------------------------------------------------------------
public sealed record CompiledExpr(
    UInt128 ContentKey,
    Seq<string> SymbolOrder,
    CompileArity Arity,
    Delegate Evaluator,
    Option<double> Constant) {
    public Fin<double> Invoke(ImmutableArray<double> arguments) =>
        Arity == CompileArity.Nullary
            ? Constant.Match(Some: Fin.Succ, None: () => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<nullary-without-constant>")))
        : arguments.Length != SymbolOrder.Count
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<arity:{arguments.Length}≠{SymbolOrder.Count}>"))
        : Fin.Succ(Arity.Invoke(Evaluator, arguments));
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class CompileCapsule {
    public static Fin<CompiledExpr> Compile(SymbolicExpr source, Seq<string> symbolOrder) {
        var arity = CompileArity.Select(symbolOrder.Count);
        if (arity == CompileArity.Nullary) {
            return source.Evaluate(Map<string, double>())
                .Map(value => new CompiledExpr(source.ContentKey, symbolOrder, arity, CompileArity.Select(0).Lower(source.Entity, []), Some(value)));
        }
        if (source.Entity.Nodes.Any(static n => n is Entity.CalculusOperator or Entity.Set or Entity.Statement)) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.NonDifferentiable($"<compile-residue:{source.Canonical}>"));
        }
        var variables = symbolOrder.Map(MathS.Var).ToArray();
        // Exemption: the engine's compile throws on an un-compilable node the residue gate cannot see; the one try converts that platform-forced seam onto the Fin rail.
        try {
            return Fin.Succ(new CompiledExpr(source.ContentKey, symbolOrder, arity, arity.Lower(source.Entity, variables), None));
        }
        catch (Exception decline) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.NonDifferentiable($"<compile-declined:{source.Canonical}:{decline.GetType().Name}>"));
        }
    }
}
```

## [03]-[LOWERING_CACHE]

- Owner: `CompiledKey` the static derivation folding the source `SymbolicExpr.ContentKey` and the ordered symbol-vector digest into one `XxHash128` cache key, composing the suite hash law `Runtime/codecs#CONTENT_ADDRESSING` holds (content-key bytes hashed under an order-derived `XxHash3` seed, never a string-path key); `LoweringSlot` the `[ImmutableObject(true)]` carrier wrapping the produced `Fin<CompiledExpr>` so the model-lane `HybridCache` stores the live delegate by reference in L1 without serialization; `LoweringCache` the L1-only read-through over the one `HybridCache` substrate the model lane runs — it scopes the content key onto `CacheLane.ModelResult` and dispatches `HybridCache.GetOrCreateAsync` with the `HybridCacheEntryFlags.DisableDistributedCache` bypass, never constructing a `HybridCache` of its own and never minting a parallel `CacheLane`. Keying by its own content identity rather than a `ModelResultKey` (which carries `ModelIdentity`/`ExecutionProvider`/`ModelPrecision` ONNX-run facts a compiled formula lacks), it composes the substrate and matches the `CachePolicy.ReadThrough` posture.
- Cases: one `LoweringSlot` per content key — a compiled success and a deterministic `NonDifferentiable` decline both ride the same `Fin<CompiledExpr>` slot under the lane TTL, so a re-attempt of a deterministically-declining formula serves the cached decline rather than re-running the engine compile; the entry is L1-only because a compiled `Delegate` is not durably serializable — the `DisableDistributedCache` flag bypasses the L2 tier entirely, so a cross-process consumer re-lowers from the content-addressed key.
- Entry: `Through(SymbolicExpr, Seq<string> symbolOrder, CancellationToken)` is the one read-through — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult` under a `symbolic:` prefix, and dispatches `cache.GetOrCreateAsync(...)` where the stampede-aware factory runs `CompileCapsule.Compile` and wraps the `Fin` in a `LoweringSlot`, so identical-formula-identical-order calls coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering.
- Auto: `CompiledKey.Of` stamps the `SymbolicExpr.ContentKey` as the hash source and the ordered-symbol digest as the `XxHash3` seed (so a re-order of the positional convention keys distinctly) in one `XxHash128.HashToUInt128(content, seed)`, never a path- or display-string key; the `GetOrCreateAsync` single-flight is the single population point, so stampede control rides the cache surface with zero call-site ceremony; the entry copies the lane's `Expiration`/`LocalCacheExpiration` from `CacheLane.ModelResult.Entry` and adds only the L2-bypass `Flags` (`HybridCacheEntryOptions` is a sealed non-record class, so the copy is an object initializer over the lane values — a `with` expression the type cannot support, and a literal dropping the lane TTL, are the rejected forms); the `LoweringSlot` `[ImmutableObject(true)]` holds the slot by reference and never serializes the wrapped `Delegate`, so cross-process reuse is deterministic re-lowering off the shared content key, not a serialized delegate and not a re-derivation seed (a seed without the source `Entity` cannot reconstruct the delegate, so an L2 round-trip buys nothing).
- Receipt: the lowering rides the model-lane `ComputeReceipt.Cache` hit/miss/store slots, never a parallel receipt; a cached compile-decline rides the `NonDifferentiable` 2215 arm — no new case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed)`/`XxHash3.HashToUInt64`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, `HybridCacheEntryFlags.DisableDistributedCache`, reached over `CacheLane.ModelResult`, never registered here), System.ComponentModel (`[ImmutableObject(true)]`), LanguageExt.Core (`Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` descriptor).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/inference#RESULT_CACHE`, never a lowering-local policy; a new key contributor (a target-runtime tag if a future ALC variant compiled distinct delegates) is one more stamp in `CompiledKey.Of`, never a parallel key; a new cache substrate is impossible — the one `HybridCache` is the single substrate, and a lowering-local registration or a `ConcurrentDictionary<UInt128, CompiledExpr>` memoization is the named defect.
- Boundary: `LoweringCache` never owns a cache instance — a hand-rolled `ConcurrentDictionary<UInt128, CompiledExpr>` memoization is the deleted form; a `source.Canonical` string key is redundant because the content key already digests the canonical form; keying by the ONNX `ModelResultKey` is rejected because a compiled formula carries no `ModelIdentity`/`ExecutionProvider`/`ModelPrecision`; a `DisableDistributedCacheWrite`-only half-measure is rejected (it leaves the entry probing a permanently-empty L2 on every miss), and an "L2 carries a re-lowering seed" design is illusory because a seed without the source `Entity` cannot reconstruct the delegate; caching the bare `Fin<CompiledExpr>` instead of the `[ImmutableObject]` `LoweringSlot` is rejected because HybridCache serializes the non-immutable value and fails on the `Delegate`; a caller that compiles-then-caches in two steps duplicates the stampede lock the `GetOrCreateAsync` single-flight owns.

```csharp signature
// --- [CONSTANTS] -------------------------------------------------------------------------
file static class LoweringEntry {
    // Carries the model lane's TTL/policy forward and adds only the L2 bypass; a compiled Delegate is not serializable, so this entry is L1-only.
    // HybridCacheEntryOptions is a sealed non-record, so the copy is an object initializer over the lane values — `with` is unavailable.
    public static readonly HybridCacheEntryOptions Compiled = new() {
        Expiration = CacheLane.ModelResult.Entry.Expiration,
        LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
        Flags = HybridCacheEntryFlags.DisableDistributedCache,
    };
}

// --- [MODELS] ----------------------------------------------------------------------------
// [ImmutableObject(true)] is the HybridCache immutable marker: L1 holds this slot by reference and never serializes the live Delegate;
// success and a deterministic decline both ride the one Fin<CompiledExpr>.
[ImmutableObject(true)]
public sealed record LoweringSlot(Fin<CompiledExpr> Result);

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class CompiledKey {
    public static UInt128 Of(SymbolicExpr source, Seq<string> symbolOrder) {
        Span<byte> content = stackalloc byte[16];
        MemoryMarshal.Write(content, source.ContentKey);
        long orderSeed = unchecked((long)XxHash3.HashToUInt64(
            MemoryMarshal.AsBytes(string.Join('\u001f', symbolOrder).AsSpan())));
        return XxHash128.HashToUInt128(content, orderSeed);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
public sealed class LoweringCache(HybridCache cache) {
    public async ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        (await cache.GetOrCreateAsync(
            CacheLane.ModelResult.Scoped($"symbolic:{CompiledKey.Of(source, symbolOrder):x32}"),
            (Source: source, Order: symbolOrder),
            static (state, _) => new ValueTask<LoweringSlot>(new LoweringSlot(CompileCapsule.Compile(state.Source, state.Order))),
            LoweringEntry.Compiled,
            [CacheLane.ModelResult.Key],
            token)).Result;
}
```

## [04]-[SYMBOLIC_JACOBIAN]

- Owner: `SymbolicJacobian` the static lowering differentiating a formula by each free design symbol through `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate`, compiling each partial through the `LoweringCache`, and packing the ordered partials WITH the design point into a `SymbolicTape`; `SymbolicTape` the symbolic analog of the DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` carrying the design-symbol vector, the compiled partial delegates, AND the design point the partials evaluate at — the point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`, so the reverse sweep needs no external primal; `SymbolicAdjoint` the reverse-mode transpose whose `Chain` folds a `Seq<SymbolicTape>` under the SAME two-argument `Chain(tape, seed)` contract `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` answers, the cotangent the only external seed.
- Cases: a free design symbol present in `formula.FreeSymbols` lowers to one compiled partial `∂f/∂xᵢ` on the tape; a formula with no free design symbol lowers an empty `SymbolicTape` so `Chain` returns the upstream cotangent unchanged and `DescendAdjoint` falls to finite-difference — the absent-`DesignMesh` degenerate-descent invariant, held for the symbolic source exactly as for geometry; a non-differentiable node (a surviving `Derivativef` residue the `SymbolicOps.Apply` gate converts) is the `NonDifferentiable` 2215 fault before any tape records.
- Entry: `Lower(formula, designSymbols, designPoint, cache, token)` is the one lowering — it filters `designSymbols` to those present in `formula.FreeSymbols`, differentiates the formula by each through `SymbolicOps.Apply(..., Differentiate)` in one short-circuiting `Traverse` (a non-differentiable partial faults here, before any compile), compiles every surviving partial over the full design-symbol order through `LoweringCache.Through` CONCURRENTLY (the cache single-flight de-duplicating identical partials), and packs the ordered partials with the design point into the tape; `Backward(tape, cotangent)` evaluates each compiled partial at the tape's baked design point and applies the Jacobian-transpose-times-cotangent contraction `x̄ = Jᵀ·ȳ`, returning the gradient in the optimizer's `ReadOnlyMemory<float>` seed shape.
- Auto: `Lower` reuses the `SymbolicOp.Differentiate` arm rather than re-implementing the analytic derivative, and reuses the `LoweringCache` so a partial `∂f/∂x` compiled once is reused across optimizer generations; for a scalar formula the Jacobian is the single gradient row `∇f`, so `Backward` evaluates each partial through `CompiledExpr.Invoke` to recover `∇f` (analytic, never a finite-difference stencil) and contracts `x̄ = ∇f · ȳ₀` by SCALING the gradient row by the scalar upstream cotangent through the verified scalar-broadcast `TensorPrimitives.Multiply(gradient, ȳ₀, gradient)` — an elementwise `Multiply` against an n-vector cotangent is the rejected form (Hadamard, not a scalar output-sensitivity scale); `SymbolicAdjoint.Chain` reverses the tape and threads the cotangent through each step's `Backward` against THAT step's baked design point, an empty tape short-circuiting to the identity so the degenerate case is structural.
- Receipt: none of its own — the gradient feeds the optimizer `DescendAdjoint` which stamps the `Optimization` slot; a lowering fault rides the `ComputeFault` rail at the `NonDifferentiable` arm.
- Packages: AngouriMath (`Differentiate` through the `OPERATION_FOLD` `Apply`, `Compile<>` through `LOWERING`), System.Numerics.Tensors (`TensorPrimitives.Multiply(ReadOnlySpan<float>, float, Span<float>)` for the scalar-broadcast `∇f · ȳ₀` contraction, the same SIMD surface `Tensor/dispatch#EQUIVALENCE_INTEROP` uses), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Seq`, `Traverse`), Rasm (project — the `SensitivityLaw`/`AdjointMode` reverse-mode contract the symbolic tape conforms to).
- Growth: a new gradient source is one more additive `DesignVariable` arm at `Solver/optimizer#OPTIMIZER_LANE` (geometry fields, the symbolic field, the hyperdual scalar leg, a future analytic-FEM field are sibling arms on the ONE `Sensitivity` family), never a parallel adjoint path; a higher-order symbolic sensitivity (a Hessian for a Newton step) is one `SymbolicJacobian` companion differentiating the partials a second time (`Differentiate(symbol, 2)`), riding the same tape/transpose surface, never a new descent; the two tapes stay distinct carriers under one `Chain` contract, so a unifying `Seq<AdjointTape>` `[Union]` of `GeometryTape | SymbolicTape` is the only collapse the optimizer registry admits, never a duplicated descent kernel.
- Boundary: the symbolic-Jacobian arm is the additive `DesignVariable.Symbolic` case the optimizer admits — a standalone `GradientSource` or a parallel `(Seq<double>, double)` path is the named defect (that signature is the `Surrogate.Predict` RETURN shape; the evaluate oracle is `Func<DesignPoint, Fin<Seq<double>>>` and the gradient surface is `DescendAdjoint` reading `SensitivityLaw.Chain`); the symbolic source is NOT a row on the `DesignProblem.OperatorRows` `FrozenDictionary<Type, TensorOpFamily>` (tensor-op families only), it is the sibling symbolic-tape entry the future `AdjointTape` `[Union]` unifies; `SymbolicAdjoint.Chain` is the VERIFIED two-argument `Chain(Seq<SymbolicTape>, seed)` overload, and a three-argument `Chain(tape, designPoint, seed)` is the deleted form (exactly the phantom the geometry `Chain` removed), so the design point lives on the `SymbolicTape`; the dimensional gate runs strictly first — `Lower` is invoked only after `Symbolic/dimensional#DIMENSION_PROOF` admits.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------
// Design point baked into the tape (mirroring the GeometryTape MeshAdjointSnapshot), so the reverse sweep needs no external primal and Chain stays two-argument.
public sealed record SymbolicTape(Seq<string> DesignSymbols, Seq<CompiledExpr> Partials, ImmutableArray<double> DesignPoint) {
    public static readonly SymbolicTape Empty = new(Seq<string>(), Seq<CompiledExpr>(), ImmutableArray<double>.Empty);

    public bool IsDegenerate => Partials.IsEmpty;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicJacobian {
    public static async ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, ImmutableArray<double> designPoint, LoweringCache cache, CancellationToken token = default) {
        Seq<string> free = designSymbols.Filter(formula.FreeSymbols.Contains);
        if (free.IsEmpty) { return Fin.Succ(SymbolicTape.Empty); }
        // Partials compile over `free` (design symbols PRESENT in the formula), so the baked point projects onto `free` in lockstep:
        // a declared-but-absent symbol drops from BOTH the partial order and the point (baking unfiltered `designPoint` arity-mismatches
        // every `Backward`, whose check is `designPoint.Length == SymbolOrder.Count`); the recovered gradient is labelled by `free`.
        ImmutableArray<double> freePoint =
            [.. designSymbols.Zip(toSeq(designPoint)).Filter(pair => formula.FreeSymbols.Contains(pair.Item1)).Map(static pair => pair.Item2)];
        return await free.Traverse(symbol => SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol)))
            .Match(
                Fail: error => new ValueTask<Fin<SymbolicTape>>(Fin.Fail<SymbolicTape>(error)),
                Succ: async partials => {
                    Fin<CompiledExpr>[] compiled = await Task.WhenAll(partials.Map(partial => cache.Through(partial, free, token).AsTask()));
                    return toSeq(compiled).Traverse(static slot => slot).Map(rows => new SymbolicTape(free, rows, freePoint));
                });
    }

    public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ReadOnlyMemory<float> cotangent) =>
        tape.IsDegenerate
            ? Fin.Succ(cotangent)
            : tape.Partials.Traverse(partial => partial.Invoke(tape.DesignPoint)).Map(gradient => Contract(gradient, cotangent));

    // Scalar f : R^n -> R, so J is the 1xn gradient row and the reverse VJP is x̄ = ∇f·ȳ₀: the scalar cotangent ȳ₀ SCALES the gradient row,
    // never an elementwise Hadamard product against an n-vector cotangent.
    static ReadOnlyMemory<float> Contract(Seq<double> gradient, ReadOnlyMemory<float> cotangent) {
        float seed = cotangent.Length > 0 ? cotangent.Span[0] : 0f;
        float[] result = [.. gradient.Map(static g => (float)g)];
        TensorPrimitives.Multiply(result, seed, result);
        return result;
    }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public static class SymbolicAdjoint {
    public static Fin<ReadOnlyMemory<float>> Chain(Seq<SymbolicTape> tape, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(
            Fin.Succ(upstream),
            (gradient, step) => gradient.Bind(seed => SymbolicJacobian.Backward(step, seed)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SYMBOLIC_REGISTRY_WIRING]-[OPEN]: how the `Solver/optimizer#OPTIMIZER_LANE` `DesignVariable` `[Union]` gains the additive `DesignVariable.Symbolic` arm (lowering `SymbolicJacobian.Lower` at the current iterate, adjoint routing `SymbolicAdjoint.Chain`) and the `Seq<AdjointTape>` `[Union]` unifying `GeometryTape`/`SymbolicTape` under one two-argument `Chain`; the optimizer page owns the consumer-side carrier and the `AdjointTape` union, this page the producer contract.
