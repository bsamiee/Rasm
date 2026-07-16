# [SYMBOLIC_LOWERING]

Compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` lowers once to a native delegate through the engine's typed `Compile<TIn1..TIn8, TOut>(vars)` IL-compiling surface (arities one through eight instantiate `double` and lower through the LINQ-expression protocol) with the interpreter `Compile(params Variable[]) → FastExpression` absorbing every arity past eight, carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` mints, and reused through a `LoweringCache` read-through over the one model-lane `HybridCache` (`Model/inference#RESULT_CACHE` `CacheLane.ModelResult`, never a second instance). Owned here: the `CompiledExpr` carrier, the `CompileArity` `[SmartEnum<string>]` that selects and owns the arity-exact compile-and-invoke behavior (one delegate-backed row per arity, the variadic row retaining `Complex` until the real-result gate), the `LoweringCache` L1-only read-through with its `LoweringSlot` carrier and `CompiledKey` derivation composing the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` law, and the cross-lane `SymbolicJacobian` that differentiates a formula by each free design symbol, compiles each partial, and packs the partials WITH the design point into a `SymbolicTape` whose `SymbolicAdjoint.Chain` answers the same two-argument reverse-mode contract `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain` answers — the design point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`. Symbolic gradients enter as the additive `DesignVariable.Symbolic` arm the optimizer admits — its lowering yielding a `SymbolicTape`, its adjoint routing `SymbolicAdjoint.Chain` — never a standalone `GradientSource` and never a parallel `(Seq<double>, double)` path (that pair is the `Surrogate.Predict` RETURN shape, never the gradient contract).

Lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before: a formula compiles and registers a Jacobian only after `DimensionProof` admits, so a dimension-inconsistent expression never reaches a `CompiledExpr` slot, the optimizer oracle, or the integrator seed. `SymbolicTape.ActiveIndices` maps free-symbol partials back onto the full design vector; inactive symbols and a constant formula produce exact zero gradient coordinates rather than shortening the vector or forwarding the scalar cotangent as a false identity. Host-local, no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior value, and the only cross-surface fact is the `SymbolicExpr.ContentKey` crossing by reference to the `Rasm.Persistence/Query/cache#MODEL_RESULT_INDEX` cost-catalog/QTO consumers, keyed by its OWN content identity, never a fabricated `ModelResultKey`. Compiled delegates are ALC-safe yet not durably serializable, so the cache entry is L1-only by construction — cross-process reuse is deterministic re-lowering off the content-addressed key, never a serialized delegate — and a compiled delegate roots its load context, so ALC teardown drives the `Evict`/`Purge` invalidation surface rather than leaking the context through a live L1 reference. An in-proc symbolic-regression fit is the rejected form: offline formula discovery is the Python branch's, and compile-and-cache, the analytic-Jacobian lowering, and the enclosure/column evaluation modalities over an already-admitted expression are all this owner holds.

## [01]-[INDEX]

- [01]-[LOWERING]: `CompiledExpr` delegate carrier; `CompileArity` delegate-backed arity owner (compile + invoke in one row); the typed `Compile<>`/interpreter `FastExpression` lowering.
- [02]-[LOWERING_CACHE]: `LoweringCache` L1-only read-through over `CacheLane.ModelResult`; `LoweringSlot` `[ImmutableObject]` carrier; `CompiledKey` content-key derivation.
- [03]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering; `SymbolicTape` (design point baked in) / `SymbolicAdjoint` two-argument transpose `Chain`.
- [04]-[ENCLOSURE_AND_COLUMNS]: `Interval`/`EnclosureFold` verified range enclosure over a box domain with the `IntervalVerdict` constraint pre-gate; `ColumnProgram` tensor-lane column evaluation running one formula over N design points in SIMD passes.

## [02]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a lowered native delegate to its source content key and ordered free-symbol vector; `CompileArity` the `[SmartEnum<string>]` whose ten rows (`nullary` … `octonary`, `variadic`) each own both compile form and invoke form as delegate-backed behavior — a typed row's `Lower` instantiates the arity-exact `Entity.Compile<double, …>(vars)` generic, its `Invoke` performs the strongly-typed down-cast, and the variadic row's `Lower` wraps the `Entity.Compile(params Variable[]) → FastExpression` interpreter in one `Func<double[], Complex>` closure so no `FastExpression` leaks past the row and no imaginary residual is discarded; `CompileCapsule` the one boundary owner gating the lowering — it pre-validates no analytic residue (`Derivativef`/`Integralf`/`Limitf`) and no non-numeric node (`Set`/`Statement`), then converts the engine's compile throw onto the `ComputeFault.NonDifferentiable` rail.
- Cases: `nullary` (rank 0 — a constant evaluated once, no delegate), `unary` … `octonary` (ranks 1–8, the engine's COMPLETE strongly-typed generic set, each `Func<double,…,double>`), `variadic` (rank −1, the `FastExpression` interpreter behind a `Func<double[], Complex>`, reached only past eight); every row returns `Fin<double>` after finite/real admission, and `CompileArity.Select(symbolOrder.Count)` selects the row.
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
        invoke: static (_, _) => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<nullary-delegate-invocation>")));
    public static readonly CompileArity Unary = new("unary", 1,
        lower: static (e, s) => e.Compile<double, double>(s[0]),
        invoke: static (d, a) => Admit(((Func<double, double>)d)(a[0])));
    public static readonly CompileArity Binary = new("binary", 2,
        lower: static (e, s) => e.Compile<double, double, double>(s[0], s[1]),
        invoke: static (d, a) => Admit(((Func<double, double, double>)d)(a[0], a[1])));
    public static readonly CompileArity Ternary = new("ternary", 3,
        lower: static (e, s) => e.Compile<double, double, double, double>(s[0], s[1], s[2]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double>)d)(a[0], a[1], a[2])));
    public static readonly CompileArity Quaternary = new("quaternary", 4,
        lower: static (e, s) => e.Compile<double, double, double, double, double>(s[0], s[1], s[2], s[3]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double>)d)(a[0], a[1], a[2], a[3])));
    public static readonly CompileArity Quinary = new("quinary", 5,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4])));
    public static readonly CompileArity Senary = new("senary", 6,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5])));
    public static readonly CompileArity Septenary = new("septenary", 7,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6])));
    public static readonly CompileArity Octonary = new("octonary", 8,
        lower: static (e, s) => e.Compile<double, double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7]),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7])));
    public static readonly CompileArity Variadic = new("variadic", -1,
        lower: static (e, s) => Interpret(e.Compile(s)),
        invoke: static (d, a) => Admit(((Func<double[], Complex>)d)([.. a])));

    private readonly int rank;
    private readonly Func<Entity, Entity.Variable[], Delegate> lower;
    private readonly Func<Delegate, ImmutableArray<double>, Fin<double>> invoke;

    public int Rank => rank;

    // Symbol-order count selects the row; typed compiled-IL to eight, the FastExpression interpreter absorbing nine-plus.
    internal static CompileArity Select(int symbolCount) =>
        symbolCount switch {
            0 => Nullary, 1 => Unary, 2 => Binary, 3 => Ternary, 4 => Quaternary,
            5 => Quinary, 6 => Senary, 7 => Septenary, 8 => Octonary, _ => Variadic,
        };

    internal Delegate Lower(Entity entity, Entity.Variable[] variables) => lower(entity, variables);

    internal Fin<double> Invoke(Delegate evaluator, ImmutableArray<double> arguments) => invoke(evaluator, arguments);

    // Interpreter stays behind the row: one closure marshals double[] -> Complex[] -> Call; `Admit` preserves the imaginary residual through the real-result gate.
    static Func<double[], Complex> Interpret(FastExpression fast) =>
        args => fast.Call([.. args.Select(static x => new Complex(x, 0))]);

    static Fin<double> Admit(double value) =>
        double.IsFinite(value)
            ? Fin.Succ(value)
            : Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<compiled-non-finite:{value}>"));

    static Fin<double> Admit(Complex value) =>
        double.IsFinite(value.Real) && double.IsFinite(value.Imaginary)
        && Math.Abs(value.Imaginary) <= 1e-12 * Math.Max(1.0, Math.Abs(value.Real))
            ? Fin.Succ(value.Real)
            : Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<compiled-non-real:{value}>"));

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
        Arity is null
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<null-compiled-arity>"))
        : Arity == CompileArity.Nullary
            ? Constant.Match(Some: Fin.Succ, None: () => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<nullary-without-constant>")))
        : Evaluator is null || arguments.Length != SymbolOrder.Count
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<arity:{arguments.Length}≠{SymbolOrder.Count}>"))
        : !arguments.All(double.IsFinite)
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<non-finite-argument>"))
        : Try.lift<Fin<double>>(() => Arity.Invoke(Evaluator, arguments)).Run()
            .MapFail(static error => (Error)new ComputeFault.SymbolUndefined($"<compiled-invoke:{error.Message}>"))
            .Bind(identity);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class CompileCapsule {
    public static Fin<CompiledExpr> Compile(SymbolicExpr source, Seq<string> symbolOrder) {
        // `SymbolicExpr` is a struct — a forged `default` carries a null `Entity`, the one null gate it admits.
        if (source.Entity is null) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.SymbolUndefined("<null-compile-source>"));
        }

        Seq<string> missing = source.FreeSymbols.Filter(symbol => !symbolOrder.Contains(symbol));
        if (symbolOrder.Exists(string.IsNullOrWhiteSpace) || symbolOrder.Distinct().Count != symbolOrder.Count || !missing.IsEmpty) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.SymbolUndefined(
                $"<symbol-order-invalid:missing={string.Join(",", missing)}>"));
        }

        CompileArity arity = CompileArity.Select(symbolOrder.Count);
        if (arity == CompileArity.Nullary) {
            return source.Evaluate(Map<string, double>())
                .Map(value => new CompiledExpr(source.ContentKey, symbolOrder, arity, CompileArity.Select(0).Lower(source.Entity, []), Some(value)));
        }
        if (source.Entity.Nodes.Any(static n => n is Entity.CalculusOperator or Entity.Set or Entity.Statement)) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.NonDifferentiable($"<compile-residue:{source.Canonical}>"));
        }
        Entity.Variable[] variables = symbolOrder.Map(MathS.Var).ToArray();
        // Engine compile can reject a node the residue gate cannot see; `Try.lift` converts that seam once.
        return Try.lift(() => new CompiledExpr(source.ContentKey, symbolOrder, arity, arity.Lower(source.Entity, variables), None))
            .Run()
            .MapFail(error => (Error)new ComputeFault.NonDifferentiable($"<compile-declined:{source.Canonical}:{error.Message}>"));
    }
}
```

## [03]-[LOWERING_CACHE]

- Owner: `CompiledKey` length-frames UTF-8 symbol names beside explicit little-endian `SymbolicExpr.ContentKey` bytes before one `XxHash128`; `LoweringSlot` wraps `Fin<CompiledExpr>` as immutable L1 state; `LoweringCache` composes the shared `HybridCache` with distributed storage disabled.
- Cases: one `LoweringSlot` per content key — a compiled success and a deterministic `NonDifferentiable` decline both ride the same `Fin<CompiledExpr>` slot under the lane TTL, so a re-attempt of a deterministically-declining formula serves the cached decline rather than re-running the engine compile; the entry is L1-only because a compiled `Delegate` is not durably serializable — the `DisableDistributedCache` flag bypasses the L2 tier entirely, so a cross-process consumer re-lowers from the content-addressed key.
- Entry: `Through(SymbolicExpr, Seq<string> symbolOrder, CancellationToken)` is the one read-through — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult` under a `symbolic:` prefix, and dispatches `cache.GetOrCreateAsync(...)` where the stampede-aware factory runs `CompileCapsule.Compile` and wraps the `Fin` in a `LoweringSlot`, so identical-formula-identical-order calls coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering; `Evict(source, symbolOrder)` drops one content-keyed slot through `HybridCache.RemoveAsync` and `Purge()` drops every symbolic slot through `RemoveByTagAsync` on the `symbolic-lowering` sub-tag — the mandatory teardown surface because a live L1 delegate pins its collectible `AssemblyLoadContext`, while the lane's model-result entries riding the shared `CacheLane.ModelResult.Key` tag survive a symbolic purge untouched.
- Auto: `CompiledKey.Of` writes both `UInt128` halves little-endian and length-prefixes every UTF-8 symbol, so symbol order and boundaries are collision-distinct across runtimes. `GetOrCreateAsync` owns single-flight population, and the entry copies shared expiration policy while adding only `DisableDistributedCache`.
- Receipt: the lowering rides the model-lane `ComputeReceipt.Cache` hit/miss/store slots, never a parallel receipt; a cached compile-decline rides the `NonDifferentiable` 2215 arm — no new case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed)`/`XxHash3.HashToUInt64`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, `HybridCacheEntryFlags.DisableDistributedCache`, reached over `CacheLane.ModelResult`, never registered here), System.ComponentModel (`[ImmutableObject(true)]`), LanguageExt.Core (`Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` descriptor).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/inference#RESULT_CACHE`; a target-runtime contributor that changes delegate identity is one more stamp in `CompiledKey.Of`; a new cache substrate is rejected.
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
    public static Fin<UInt128> Of(SymbolicExpr source, Seq<string> symbolOrder) =>
        source.Entity is null || symbolOrder.Exists(string.IsNullOrWhiteSpace) || symbolOrder.Distinct().Count != symbolOrder.Count
            ? Fin.Fail<UInt128>(new ComputeFault.SymbolUndefined("<compiled-key-input-invalid>"))
            : Try.lift(() => {
                    ArrayBufferWriter<byte> symbols = new();
                    foreach (string symbol in symbolOrder) {
                        byte[] encoded = Encoding.UTF8.GetBytes(symbol);
                        Span<byte> slot = symbols.GetSpan(4 + encoded.Length);
                        BinaryPrimitives.WriteInt32LittleEndian(slot, encoded.Length);
                        encoded.CopyTo(slot[4..]);
                        symbols.Advance(4 + encoded.Length);
                    }

                    Span<byte> frame = stackalloc byte[24];
                    BinaryPrimitives.WriteUInt64LittleEndian(frame, (ulong)source.ContentKey);
                    BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], (ulong)(source.ContentKey >> 64));
                    BinaryPrimitives.WriteUInt64LittleEndian(frame[16..], XxHash3.HashToUInt64(symbols.WrittenSpan));
                    return XxHash128.HashToUInt128(frame);
                })
                .Run()
                .MapFail(static error => (Error)new ComputeFault.SymbolUndefined($"<compiled-key:{error.Message}>"));
}

// --- [SERVICES] --------------------------------------------------------------------------
public sealed class LoweringCache(HybridCache cache) {
    public async ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        await CompiledKey.Of(source, symbolOrder).Match(
            Fail: static error => new ValueTask<Fin<CompiledExpr>>(Fin.Fail<CompiledExpr>(error)),
            Succ: async key => (await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"symbolic:{key:x32}"),
                (Source: source, Order: symbolOrder),
                static (state, _) => new ValueTask<LoweringSlot>(new LoweringSlot(CompileCapsule.Compile(state.Source, state.Order))),
                LoweringEntry.Compiled,
                [CacheLane.ModelResult.Key, SymbolicTag],
                token)).Result);

    // A compiled delegate roots its `AssemblyLoadContext`; `Evict` drops one key and `Purge` drops the symbolic
    // sub-tag before collectible-context unload while model-result entries survive.
    public async ValueTask<Fin<Unit>> Evict(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        await CompiledKey.Of(source, symbolOrder).Match(
            Fail: static error => new ValueTask<Fin<Unit>>(Fin.Fail<Unit>(error)),
            Succ: async key => { await cache.RemoveAsync(CacheLane.ModelResult.Scoped($"symbolic:{key:x32}"), token); return Fin.Succ(unit); });

    public ValueTask Purge(CancellationToken token = default) =>
        cache.RemoveByTagAsync(SymbolicTag, token);

    const string SymbolicTag = "symbolic-lowering";
}
```

## [04]-[SYMBOLIC_JACOBIAN]

- Owner: `SymbolicJacobian` differentiates a scalar formula by each free design symbol through `Symbolic/expression#OPERATION_FOLD`, compiles each partial through `LoweringCache`, and packs the full design-symbol vector, active-index map, partials, and design point into one `SymbolicTape`; `SymbolicAdjoint` owns the scalar reverse-mode transpose with the cotangent as the only external seed.
- Cases: each free design symbol lowers to one compiled partial paired positionally with `ActiveIndices`; inactive design symbols and a constant formula scatter as exact zeroes in the full-width gradient; a non-differentiable residue faults before any tape records.
- Entry: `Lower(formula, designSymbols, designPoint, cache, token)` derives `ActiveIndices`, differentiates each active symbol through one short-circuiting `Traverse`, compiles every partial over the active symbol order through concurrent `LoweringCache.Through`, and retains the full design point; `Backward(tape, cotangent)` evaluates at the active projection, scatters into full width, and applies `x̄ = Jᵀ·ȳ` in the optimizer's `ReadOnlyMemory<float>` seed shape.
- Auto: `Lower` reuses the `SymbolicOp.Differentiate` arm and cache; `Backward` evaluates each partial through `CompiledExpr.Invoke`, scatters by `ActiveIndices`, then scales through `TensorPrimitives.Multiply`. `SymbolicAdjoint.Chain` forwards one tape to `Backward`; independent scalar formulas never form a `Seq<SymbolicTape>` composition.
- Receipt: none of its own — the gradient feeds the optimizer `DescendAdjoint` which stamps the `Optimization` slot; a lowering fault rides the `ComputeFault` rail at the `NonDifferentiable` arm.
- Packages: AngouriMath (`Differentiate` through the `OPERATION_FOLD` `Apply`, `Compile<>` through `LOWERING`), System.Numerics.Tensors (`TensorPrimitives.Multiply(ReadOnlySpan<float>, float, Span<float>)` for the scalar-broadcast `∇f · ȳ₀` contraction, the same SIMD surface `Tensor/dispatch#EQUIVALENCE_INTEROP` uses), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Seq`, `Traverse`), Rasm (project — the `SensitivityLaw`/`AdjointMode` reverse-mode contract the symbolic tape conforms to).
- Growth: a new gradient source is one more additive `DesignVariable` arm at `Solver/optimizer#OPTIMIZER_LANE`; a higher-order symbolic sensitivity is one `SymbolicJacobian` operation differentiating the partials a second time through `Differentiate(symbol, 2)`, riding the same tape and transpose surface.
- Boundary: the symbolic-Jacobian arm is the additive `DesignVariable.Symbolic` case the optimizer admits — a standalone `GradientSource`, a parallel `(Seq<double>, double)` path, or a `Seq<SymbolicTape>` composition is rejected. `AdjointTape` is a closed `[Union]` whose `Geometry` case carries the composable `Seq<GeometryTape>` and whose `Symbolic` case carries one scalar `SymbolicTape`; each arm retains its honest arity under one optimizer dispatch. `SymbolicAdjoint.Chain(SymbolicTape, ReadOnlyMemory<float>)` stays two-argument because the design point lives on the tape, and `Lower` runs only after `Symbolic/dimensional#DIMENSION_PROOF` admits.

```csharp signature
// --- [MODELS] ----------------------------------------------------------------------------
// Design point rides the tape, so the reverse sweep needs no external primal.
public sealed record SymbolicTape(Seq<string> DesignSymbols, Seq<int> ActiveIndices, Seq<CompiledExpr> Partials, ImmutableArray<double> DesignPoint) {
    public static readonly SymbolicTape Empty = new(Seq<string>(), Seq<int>(), Seq<CompiledExpr>(), ImmutableArray<double>.Empty);

    public bool IsDegenerate => ActiveIndices.IsEmpty;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicJacobian {
    public static async ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, ImmutableArray<double> designPoint, LoweringCache cache, CancellationToken token = default) {
        if (formula.Entity is null || cache is null || designSymbols.Count != designPoint.Length || designSymbols.Exists(string.IsNullOrWhiteSpace)
            || designSymbols.Distinct().Count != designSymbols.Count || !designPoint.All(double.IsFinite)) {
            return Fin.Fail<SymbolicTape>(new ComputeFault.SymbolUndefined("<symbolic-design-point-invalid>"));
        }

        Seq<int> active = toSeq(Enumerable.Range(0, designSymbols.Count)).Filter(index => formula.FreeSymbols.Contains(designSymbols[index]));
        Seq<string> free = active.Map(index => designSymbols[index]);
        if (free.IsEmpty) { return Fin.Succ(new SymbolicTape(designSymbols, active, Seq<CompiledExpr>(), designPoint)); }
        return await free.Traverse(symbol => SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol)))
            .Match(
                Fail: error => new ValueTask<Fin<SymbolicTape>>(Fin.Fail<SymbolicTape>(error)),
                Succ: async partials => {
                    Fin<CompiledExpr>[] compiled = await Task.WhenAll(partials.Map(partial => cache.Through(partial, free, token).AsTask()));
                    return toSeq(compiled).Traverse(static slot => slot).Map(rows => new SymbolicTape(designSymbols, active, rows, designPoint));
                });
    }

    // Scalar-output VJP admits one cotangent component; inactive design coordinates remain exact zeroes.
    public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ReadOnlyMemory<float> cotangent) =>
        tape is null || tape.DesignPoint.Length != tape.DesignSymbols.Count || tape.ActiveIndices.Count != tape.Partials.Count
            || tape.DesignSymbols.Exists(string.IsNullOrWhiteSpace) || tape.ActiveIndices.Distinct().Count != tape.ActiveIndices.Count
            || tape.ActiveIndices.Exists(index => index < 0 || index >= tape.DesignSymbols.Count)
            ? Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.SymbolUndefined("<symbolic-tape-invalid>"))
        : cotangent.Length != 1
            ? Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.SymbolUndefined($"<cotangent-arity:{cotangent.Length}≠1:scalar-tape>"))
        : tape.IsDegenerate
            ? Fin.Succ<ReadOnlyMemory<float>>(new float[tape.DesignSymbols.Count])
            : BackwardActive(tape, cotangent.Span[0]);

    static Fin<ReadOnlyMemory<float>> BackwardActive(SymbolicTape tape, float seed) {
        ImmutableArray<double> activePoint = [.. tape.ActiveIndices.Map(index => tape.DesignPoint[index])];
        return tape.Partials.Traverse(partial => partial is null
                ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<null-symbolic-partial>"))
                : partial.Invoke(activePoint))
            .Map(gradient => Contract(gradient, tape.ActiveIndices, tape.DesignSymbols.Count, seed));
    }

    // Scalar `f : R^n -> R` scatters active partials into the full design gradient before scaling by the seed.
    static ReadOnlyMemory<float> Contract(Seq<double> gradient, Seq<int> activeIndices, int width, float seed) {
        float[] result = new float[width];
        activeIndices.Zip(gradient).Iter(pair => result[pair.First] = (float)pair.Second);
        TensorPrimitives.Multiply(result, seed, result);
        return result;
    }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public static class SymbolicAdjoint {
    public static Fin<ReadOnlyMemory<float>> Chain(SymbolicTape tape, ReadOnlyMemory<float> upstream) =>
        SymbolicJacobian.Backward(tape, upstream);
}
```

## [05]-[ENCLOSURE_AND_COLUMNS]

- Owner: `Interval` the inf-sup carrier whose every operation rounds outward through `Math.BitDecrement`/`Math.BitIncrement`, so the returned bounds are a GUARANTEED enclosure under double arithmetic, never a sampled estimate; `EnclosureFold` the `Entity` tree fold evaluating a formula over a box domain in interval arithmetic; `IntervalVerdict` `[Union]` the three-way constraint pre-gate a `g(x) ≤ 0` question answers over an entire box in one evaluation; `ColumnProgram` the register program lowering one formula onto `TensorPrimitives` span kernels so N design points evaluate as columns; `ColumnStep` its one instruction row. Both modalities key on the same canonical-NF `SymbolicExpr.ContentKey` the scalar lowering keys on.
- Cases: `IntervalVerdict` cases `ProvenSatisfied` (upper bound ≤ 0 — every point of the box satisfies), `ProvenViolated` (lower bound > 0 — no point can), `Indeterminate(Interval)` (the enclosure straddles zero — the box splits or the exact engine answers); `ColumnStep` kinds `Variable` (column bind) · `Constant` (broadcast fill) · `Unary` · `Binary` over the verified kernel set `Add`/`Subtract`/`Multiply`/`Divide`/`Pow`/`Abs`/`Log`/`Negate`.
- Entry: `EnclosureFold.Enclose(SymbolicExpr, Seq<string> symbolOrder, ImmutableArray<Interval> box)` folds the tree over the catalog-verified node records — `Sumf`/`Minusf`/`Mulf`/`Divf`/`Powf`/`Absf`/`Signumf`/`Logf`, `Variable`, the numeric leaves — and declines any other node typed, so the enclosure never silently widens to `(−∞, +∞)` on a node it cannot bound; `Certify(...)` projects the enclosure onto `IntervalVerdict`. `ColumnProgram.Lower(SymbolicExpr, Seq<string> symbolOrder)` compiles the enclosure node set minus `Signumf` (no verified `TensorPrimitives` sign kernel exists, so the sign node declines typed to the scalar fallback) into a `Seq<ColumnStep>` register program — `Logf` lowers as `Log(x)/Log(b)` and a `-1` multiplier lowers as the `Negate` kernel; `Evaluate(ReadOnlyMemory<double>[] columns)` runs it over one pooled register file in a handful of SIMD passes — a 10⁴-point DOE grid pays tens of span kernels instead of 10⁴ delegate dispatches; a formula outside the lowered node set declines typed and the caller loops the scalar `CompiledExpr.Invoke` as the honest fallback.
- Receipt: none of its own — a branch-and-prune consumer counts discarded boxes on its own receipt, and the column sweep rides the sweep's `ComputeReceipt.Sweep`; an enclosure or lowering decline rides the `ComputeFault.NonDifferentiable` 2215 arm exactly as a compile decline does.
- Packages: AngouriMath (the positional node records, `IUnaryNode.NodeChild`, `IBinaryNode.NodeFirstChild`/`NodeSecondChild`), System.Numerics.Tensors (`TensorPrimitives.Add`/`Subtract`/`Multiply`/`Divide`/`Pow`/`Abs`/`Log`/`Negate` span kernels), CommunityToolkit.HighPerformance (`MemoryOwner<double>` register file), LanguageExt.Core (`Fin`, `Seq`), BCL inbox (`Math.BitDecrement`/`BitIncrement`).
- Growth: a new bounded node family (the trig records, once their monotonicity split lands) is one arm on BOTH folds — the interval bound and the column kernel land together or the node stays declined; a tighter enclosure (affine arithmetic, mean-value forms) is a policy row on `EnclosureFold`, never a sibling evaluator; a new column kernel is one `ColumnStep` row binding its verified `TensorPrimitives` member.
- Boundary: the pre-gate serves `Solver/satisfy#RULE_SATISFACTION` — a rule whose enclosure proves over the declared bounds never spends the Z3 timeout, and `Indeterminate` falls through to the exact check, so the gate is a filter, never a verdict authority; `Solver/optimizer` box screening discards `ProvenViolated` regions without oracle calls. Interval division by a zero-straddling denominator and `Logf` over a non-positive interval decline typed rather than returning an infinite enclosure that certifies nothing. The column program is an evaluation plan over already-admitted values — it re-validates nothing, and the register file is pooled and returned deterministically.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
// Outward rounding after every operation keeps the enclosure sound under double arithmetic.
public readonly record struct Interval(double Lo, double Hi) {
    public static Interval Of(double lo, double hi) => new(Math.BitDecrement(lo), Math.BitIncrement(hi));

    // A leaf constant is exactly representable — no outward expansion, so an exact integer exponent stays
    // detectable by the integer-power law; rounding applies after OPERATIONS, never on the leaf itself.
    public static Interval Point(double value) => new(value, value);

    public bool Valid => double.IsFinite(Lo) && double.IsFinite(Hi) && Lo <= Hi;
    public bool Contains(double value) => value >= Lo && value <= Hi;

    public static Interval operator +(Interval a, Interval b) => Of(a.Lo + b.Lo, a.Hi + b.Hi);
    public static Interval operator -(Interval a, Interval b) => Of(a.Lo - b.Hi, a.Hi - b.Lo);
    public static Interval operator -(Interval a) => new(-a.Hi, -a.Lo);

    public static Interval operator *(Interval a, Interval b) {
        ReadOnlySpan<double> products = [a.Lo * b.Lo, a.Lo * b.Hi, a.Hi * b.Lo, a.Hi * b.Hi];
        return Of(TensorPrimitives.Min(products), TensorPrimitives.Max(products));
    }

    public Interval Abs() => Lo >= 0.0 ? this : Hi <= 0.0 ? -this : Of(0.0, Math.Max(-Lo, Hi));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IntervalVerdict {
    private IntervalVerdict() { }
    public sealed record ProvenSatisfied(Interval Range) : IntervalVerdict;
    public sealed record ProvenViolated(Interval Range) : IntervalVerdict;
    public sealed record Indeterminate(Interval Range) : IntervalVerdict;
}

public readonly record struct ColumnStep(int Target, int Left, int Right, TensorOpFamily Op, double Scalar, ColumnStepKind Kind);

[SmartEnum<string>]
public sealed partial class ColumnStepKind {
    public static readonly ColumnStepKind Variable = new("variable");
    public static readonly ColumnStepKind Constant = new("constant");
    public static readonly ColumnStepKind Unary = new("unary");
    public static readonly ColumnStepKind Binary = new("binary");
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class EnclosureFold {
    public static Fin<Interval> Enclose(SymbolicExpr source, Seq<string> symbolOrder, ImmutableArray<Interval> box) =>
        source.Entity is null || symbolOrder.Count != box.Length || !box.All(static i => i.Valid)
            ? Fin.Fail<Interval>(new ComputeFault.SymbolUndefined("<enclosure-box-invalid>"))
            : Descend(source.Entity, toSeq(symbolOrder).Zip(toSeq(box)).ToMap());

    // Constraint pre-gate over g(x) <= 0: a proven verdict answers the whole box in one evaluation.
    public static Fin<IntervalVerdict> Certify(SymbolicExpr constraint, Seq<string> symbolOrder, ImmutableArray<Interval> box) =>
        Enclose(constraint, symbolOrder, box).Map(range => (IntervalVerdict)(
            range.Hi <= 0.0 ? new IntervalVerdict.ProvenSatisfied(range)
            : range.Lo > 0.0 ? new IntervalVerdict.ProvenViolated(range)
            : new IntervalVerdict.Indeterminate(range)));

    static Fin<Interval> Descend(Entity node, Map<string, Interval> bindings) => node switch {
        Entity.Variable v => bindings.Find(v.Name).ToFin(new ComputeFault.SymbolUndefined($"<enclosure-unbound:{v.Name}>")),
        Entity.Number n => NumberBox.Project(n).Map(Interval.Point),
        Entity.Sumf s => from a in Descend(s.Augend, bindings) from b in Descend(s.Addend, bindings) select a + b,
        Entity.Minusf m => from a in Descend(m.Subtrahend, bindings) from b in Descend(m.Minuend, bindings) select a - b,
        Entity.Mulf m => from a in Descend(m.Multiplier, bindings) from b in Descend(m.Multiplicand, bindings) select a * b,
        Entity.Divf d =>
            from a in Descend(d.Dividend, bindings)
            from b in Descend(d.Divisor, bindings)
            from q in b.Contains(0.0)
                ? Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-zero-straddling-divisor>"))
                : Fin.Succ(a * Interval.Of(1.0 / b.Hi, 1.0 / b.Lo))
            select q,
        Entity.Powf p =>
            from a in Descend(p.Base, bindings)
            from e in Descend(p.Exponent, bindings)
            from r in Power(a, e)
            select r,
        Entity.Absf a => Descend(a.Argument, bindings).Map(static i => i.Abs()),
        Entity.Signumf s => Descend(s.Argument, bindings).Map(static i =>
            i.Lo > 0.0 ? Interval.Point(1.0) : i.Hi < 0.0 ? Interval.Point(-1.0) : Interval.Of(-1.0, 1.0)),
        // log_b(x) is monotone in each argument with the b-direction sign flipping at x = 1, so the enclosure
        // is the four-corner min/max — a crossed endpoint pairing returns Lo > Hi on sub-unit arguments.
        Entity.Logf l =>
            from b in Descend(l.Base, bindings)
            from x in Descend(l.Antilogarithm, bindings)
            from r in x.Lo > 0.0 && b.Lo > 0.0 && (b.Hi < 1.0 || b.Lo > 1.0)
                ? Fin.Succ(Corners(x, b, Math.Log))
                : Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-log-domain>"))
            select r,
        _ => Fin.Fail<Interval>(new ComputeFault.NonDifferentiable($"<enclosure-node:{node.GetType().Name}>")),
    };

    // A positive-base bivariate map is monotone in each argument for the other fixed, so its extrema over a
    // box sit on the four corners; partial corner pairings under-enclose whenever a monotonicity direction flips.
    static Interval Corners(Interval x, Interval y, Func<double, double, double> f) {
        double a = f(x.Lo, y.Lo), b = f(x.Lo, y.Hi), c = f(x.Hi, y.Lo), d = f(x.Hi, y.Hi);
        return Interval.Of(Math.Min(Math.Min(a, b), Math.Min(c, d)), Math.Max(Math.Max(a, b), Math.Max(c, d)));
    }

    // Integer exponents split by parity and base sign; a non-integer exponent demands a positive base.
    static Fin<Interval> Power(Interval baseRange, Interval exponent) =>
        exponent.Lo == exponent.Hi && double.IsInteger(exponent.Lo)
            ? Fin.Succ(IntegerPower(baseRange, (int)exponent.Lo))
            : baseRange.Lo > 0.0
                ? Fin.Succ(Corners(baseRange, exponent, Math.Pow))
                : Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-pow-domain>"));

    static Interval IntegerPower(Interval a, int n) =>
        n == 0 ? Interval.Point(1.0)
        : n < 0 ? IntegerPower(a, -n) switch { var p => Interval.Of(1.0 / p.Hi, 1.0 / p.Lo) }
        : (n & 1) == 0 && a.Contains(0.0) ? Interval.Of(0.0, Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)))
        : Interval.Of(Math.Min(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)));
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public sealed record ColumnProgram(UInt128 ContentKey, Seq<string> SymbolOrder, Seq<ColumnStep> Steps, int Registers) {
    public static Fin<ColumnProgram> Lower(SymbolicExpr source, Seq<string> symbolOrder) =>
        source.Entity is null || symbolOrder.Exists(string.IsNullOrWhiteSpace) || symbolOrder.Distinct().Count != symbolOrder.Count
            ? Fin.Fail<ColumnProgram>(new ComputeFault.SymbolUndefined("<column-lowering-invalid>"))
            : Emit(source.Entity, symbolOrder, Seq<ColumnStep>(), symbolOrder.Count)
                .Map(plan => new ColumnProgram(source.ContentKey, symbolOrder, plan.Steps, plan.Next));

    // One pooled register file; each step is one span kernel over all N points — the point loop is inside the kernel.
    public Fin<ReadOnlyMemory<double>> Evaluate(params ReadOnlyMemory<double>[] columns) {
        if (columns.Length != SymbolOrder.Count || columns.Length == 0 || !columns.All(c => c.Length == columns[0].Length)) {
            return Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.SymbolUndefined("<column-arity-or-length>"));
        }
        int n = columns[0].Length;
        using MemoryOwner<double> file = MemoryOwner<double>.Allocate(Registers * n, AllocationMode.Clear);
        for (int c = 0; c < columns.Length; c++) { columns[c].Span.CopyTo(file.Span.Slice(c * n, n)); }
        foreach (ColumnStep step in Steps) {
            Span<double> target = file.Span.Slice(step.Target * n, n);
            ReadOnlySpan<double> left = file.Span.Slice(step.Left * n, n);
            ReadOnlySpan<double> right = file.Span.Slice(step.Right * n, n);
            if (step.Kind == ColumnStepKind.Variable) { left.CopyTo(target); continue; }
            if (step.Kind == ColumnStepKind.Constant) { target.Fill(step.Scalar); continue; }
            if (step.Kind == ColumnStepKind.Unary) { UnaryKernel(step.Op, left, target); continue; }
            if (step.Kind == ColumnStepKind.Binary) { BinaryKernel(step.Op, left, right, target); }
        }
        double[] result = file.Span.Slice((Registers - 1) * n, n).ToArray();
        return TensorPrimitives.IsFiniteAll<double>(result)
            ? Fin.Succ<ReadOnlyMemory<double>>(result)
            : Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.SymbolUndefined("<column-non-finite>"));
    }

    static void UnaryKernel(TensorOpFamily op, ReadOnlySpan<double> source, Span<double> target) {
        if (op == TensorOpFamily.Abs) { TensorPrimitives.Abs(source, target); }
        else if (op == TensorOpFamily.Negate) { TensorPrimitives.Negate(source, target); }
        else { TensorPrimitives.Log(source, target); }
    }

    static void BinaryKernel(TensorOpFamily op, ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> target) {
        if (op == TensorOpFamily.Add) { TensorPrimitives.Add(left, right, target); }
        else if (op == TensorOpFamily.Subtract) { TensorPrimitives.Subtract(left, right, target); }
        else if (op == TensorOpFamily.Multiply) { TensorPrimitives.Multiply(left, right, target); }
        else if (op == TensorOpFamily.Divide) { TensorPrimitives.Divide(left, right, target); }
        else { TensorPrimitives.Pow(left, right, target); }
    }

    static Fin<(Seq<ColumnStep> Steps, int Next)> Emit(Entity node, Seq<string> order, Seq<ColumnStep> steps, int next) => node switch {
        Entity.Variable v => toSeq(Enumerable.Range(0, order.Count)).Find(i => order[i] == v.Name).Match(
            Some: slot => Fin.Succ((steps.Add(new ColumnStep(next, slot, slot, TensorOpFamily.Add, 0.0, ColumnStepKind.Variable)), next + 1)),
            None: () => Fin.Fail<(Seq<ColumnStep>, int)>(new ComputeFault.SymbolUndefined($"<column-unbound:{v.Name}>"))),
        Entity.Number number => NumberBox.Project(number).Map(value =>
            (steps.Add(new ColumnStep(next, 0, 0, TensorOpFamily.Add, value, ColumnStepKind.Constant)), next + 1)),
        Entity.Sumf s => Binary(s.Augend, s.Addend, TensorOpFamily.Add, order, steps, next),
        Entity.Minusf m => Binary(m.Subtrahend, m.Minuend, TensorOpFamily.Subtract, order, steps, next),
        Entity.Mulf m when IsNegOne(m.Multiplier) => Unary(m.Multiplicand, TensorOpFamily.Negate, order, steps, next),
        Entity.Mulf m => Binary(m.Multiplier, m.Multiplicand, TensorOpFamily.Multiply, order, steps, next),
        Entity.Divf d => Binary(d.Dividend, d.Divisor, TensorOpFamily.Divide, order, steps, next),
        Entity.Powf p => Binary(p.Base, p.Exponent, TensorOpFamily.Pow, order, steps, next),
        Entity.Absf a => Unary(a.Argument, TensorOpFamily.Abs, order, steps, next),
        // log_b(x) lowers as Log(x)/Log(b) — two unary Log registers and one Divide, all verified kernels.
        Entity.Logf l =>
            Emit(l.Antilogarithm, order, steps, next).Bind(x => {
                Seq<ColumnStep> logX = x.Steps.Add(new ColumnStep(x.Next, x.Next - 1, x.Next - 1, TensorOpFamily.Log, 0.0, ColumnStepKind.Unary));
                return Emit(l.Base, order, logX, x.Next + 1).Map(b => {
                    Seq<ColumnStep> logB = b.Steps.Add(new ColumnStep(b.Next, b.Next - 1, b.Next - 1, TensorOpFamily.Log, 0.0, ColumnStepKind.Unary));
                    return (logB.Add(new ColumnStep(b.Next + 1, x.Next, b.Next, TensorOpFamily.Divide, 0.0, ColumnStepKind.Binary)), b.Next + 2);
                });
            }),
        _ => Fin.Fail<(Seq<ColumnStep>, int)>(new ComputeFault.NonDifferentiable($"<column-node:{node.GetType().Name}>")),
    };

    static bool IsNegOne(Entity node) =>
        node is Entity.Number number && NumberBox.Project(number).Map(static value => value == -1.0).IfFail(false);

    static Fin<(Seq<ColumnStep> Steps, int Next)> Binary(Entity left, Entity right, TensorOpFamily op, Seq<string> order, Seq<ColumnStep> steps, int next) =>
        Emit(left, order, steps, next).Bind(l => Emit(right, order, l.Steps, l.Next)
            .Map(r => (r.Steps.Add(new ColumnStep(r.Next, l.Next - 1, r.Next - 1, op, 0.0, ColumnStepKind.Binary)), r.Next + 1)));

    static Fin<(Seq<ColumnStep> Steps, int Next)> Unary(Entity argument, TensorOpFamily op, Seq<string> order, Seq<ColumnStep> steps, int next) =>
        Emit(argument, order, steps, next).Map(a => (a.Steps.Add(new ColumnStep(a.Next, a.Next - 1, a.Next - 1, op, 0.0, ColumnStepKind.Unary)), a.Next + 1));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SYMBOLIC_REGISTRY_WIRING]-[OPEN]: how `Solver/optimizer#OPTIMIZER_LANE` gains `DesignVariable.Symbolic` and an `AdjointTape` `[Union]` whose `Geometry(Seq<GeometryTape>)` and `Symbolic(SymbolicTape)` cases preserve their distinct composition arities under one generated dispatch; the optimizer owns the consumer carrier and this page owns the symbolic producer.
