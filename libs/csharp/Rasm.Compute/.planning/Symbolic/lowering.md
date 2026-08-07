# [SYMBOLIC_LOWERING]

Compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` lowers once to a native delegate through the engine's typed `Compile<TIn1..TIn8, TOut>(vars)` IL-compiling surface (arities one through eight instantiate `double` and lower through the LINQ-expression protocol) with the interpreter `Compile(params Variable[]) → FastExpression` absorbing every arity past eight, carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` mints, and reused through a `LoweringCache` read-through over the one model-lane `HybridCache` (`Model/inference#RESULT_CACHE` `CacheLane.ModelResult`, never a second instance). Owned here: the `CompiledExpr` carrier, the `CompileArity` `[SmartEnum<string>]` that selects and owns the arity-exact compile-and-invoke behavior (one delegate-backed row per arity, the variadic row retaining `Complex` until the real-result gate), the `LoweringCache` L1-only read-through with its `LoweringSlot` carrier and `CompiledKey` derivation composing the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` law, and the cross-lane `SymbolicJacobian` that differentiates a formula by each free design symbol, compiles each partial, and packs the partials WITH the design point into a `SymbolicTape` whose `SymbolicAdjoint.Chain` answers the same two-argument reverse-mode contract `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain` answers — the design point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`. Symbolic gradients enter as the additive `DesignVariable.Symbolic` arm the optimizer admits — its lowering yielding a `SymbolicTape`, its adjoint routing `SymbolicAdjoint.Chain` — never a standalone `GradientSource` and never a parallel `(Seq<double>, double)` path (that pair is the `Surrogate.Predict` RETURN shape, never the gradient contract).

Lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before: a formula compiles and registers a Jacobian only after `DimensionProof` admits, so a dimension-inconsistent expression never reaches a `CompiledExpr` slot, the optimizer oracle, or the integrator seed. `SymbolicTape.ActiveIndices` maps free-symbol partials back onto the full design vector; inactive symbols and a constant formula produce exact zero gradient coordinates rather than shortening the vector or forwarding the scalar cotangent as a false identity. Host-local, no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior value, and the only cross-surface fact is the `SymbolicExpr.ContentKey` crossing by reference to the `Rasm.Persistence/Query/cache#MODEL_RESULT_INDEX` cost-catalog/QTO consumers, keyed by its OWN content identity, never a fabricated `ModelResultKey`. Compiled delegates are ALC-safe yet not durably serializable, so the cache entry is L1-only by construction — cross-process reuse is deterministic re-lowering off the content-addressed key, never a serialized delegate — and a compiled delegate roots its load context, so ALC teardown drives the `Evict`/`Purge` invalidation surface rather than leaking the context through a live L1 reference. In-proc symbolic-regression fitting is the rejected form: offline formula discovery is the Python branch's, and compile-and-cache, the analytic-Jacobian lowering, and the enclosure/column evaluation modalities over an already-admitted expression are all this owner holds.

## [01]-[INDEX]

- [02]-[LOWERING]: `CompiledExpr` delegate carrier; `CompileArity` delegate-backed arity owner (compile + invoke in one row); the typed `Compile<>`/interpreter `FastExpression` lowering.
- [03]-[LOWERING_CACHE]: `LoweringCache` L1-only read-through over `CacheLane.ModelResult`; `LoweringSlot` `[ImmutableObject]` carrier; `CompiledKey` content-key derivation.
- [04]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering; `SymbolicTape` (design point baked in) / `SymbolicAdjoint` two-argument transpose `Chain`.
- [05]-[ENCLOSURE_AND_COLUMNS]: `Interval`/`EnclosureFold` range enclosure over a box domain — algebraic bounds proven, transcendental bounds pad-widened — with the `IntervalVerdict` constraint pre-gate; `ColumnProgram` stack-allocated column evaluation running one formula over N design points in SIMD passes.

## [02]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a lowered native delegate to its source content key and ordered free-symbol vector; `CompileArity` the `[SmartEnum<string>]` whose ten rows (`nullary` … `octonary`, `variadic`) each own both compile form and invoke form as delegate-backed behavior — a typed row's `Lower` instantiates the arity-exact `Entity.Compile<double, …>(vars)` generic, its `Invoke` performs the strongly-typed down-cast, and the variadic row's `Lower` wraps the `Entity.Compile(params Variable[]) → FastExpression` interpreter in one `Func<double[], Complex>` closure so no `FastExpression` leaks past the row and no imaginary residual is discarded; `CompileCapsule` the one boundary owner gating the lowering — it pre-validates no analytic residue (`Derivativef`/`Integralf`/`Limitf`) and no non-numeric node (`Set`/`Statement`), then converts the engine's compile throw onto the `ComputeFault.NonDifferentiable` rail.
- Cases: `nullary` (rank 0 — a constant evaluated once, no delegate), `unary` … `octonary` (ranks 1–8, the engine's COMPLETE strongly-typed generic set, each `Func<double,…,double>`), `variadic` (rank −1, the `FastExpression` interpreter behind a `Func<double[], Complex>`, reached only past eight); every row returns `Fin<double>` after finite/real admission, and `CompileArity.Select(symbolOrder.Count)` selects the row.
- Entry: `Compile(SymbolicExpr, Seq<string> symbolOrder)` is the one polymorphic lowering — the symbol order fixes the positional argument convention (the i-th `double` binds the i-th symbol), `Select(Count)` picks the row, `arity.Lower(entity, variables)` returns `Fin<Delegate>` (the rank-0 row refusing, because a constant needs none), and the capsule's residue gate and exception seam lift the outcome onto the same rail; `Invoke(ImmutableArray<double>)` validates the argument count against the symbol order (a mis-arity call is a `ComputeFault`, never an engine index fault) then delegates to `Arity.Invoke`, so the down-cast and the variadic array bind are both owned by the row.
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
    // Free-symbol-empty formulas ARE constants, so the capsule folds one through `Evaluate` and seats the value:
    // NEITHER column is ever reached, and both refuse typed rather than minting a NaN delegate a caller can invoke.
    public static readonly CompileArity Nullary = new("nullary", 0,
        lower: static (_, _) => Fin.Fail<Delegate>(new ComputeFault.SymbolUndefined("<nullary-lowering>")),
        invoke: static (_, _) => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<nullary-delegate-invocation>")));
    public static readonly CompileArity Unary = new("unary", 1,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double>(s[0])),
        invoke: static (d, a) => Admit(((Func<double, double>)d)(a[0])));
    public static readonly CompileArity Binary = new("binary", 2,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double>(s[0], s[1])),
        invoke: static (d, a) => Admit(((Func<double, double, double>)d)(a[0], a[1])));
    public static readonly CompileArity Ternary = new("ternary", 3,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double>(s[0], s[1], s[2])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double>)d)(a[0], a[1], a[2])));
    public static readonly CompileArity Quaternary = new("quaternary", 4,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double, double>(s[0], s[1], s[2], s[3])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double>)d)(a[0], a[1], a[2], a[3])));
    public static readonly CompileArity Quinary = new("quinary", 5,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4])));
    public static readonly CompileArity Senary = new("senary", 6,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5])));
    public static readonly CompileArity Septenary = new("septenary", 7,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6])));
    public static readonly CompileArity Octonary = new("octonary", 8,
        lower: static (e, s) => Fin.Succ<Delegate>(e.Compile<double, double, double, double, double, double, double, double, double>(s[0], s[1], s[2], s[3], s[4], s[5], s[6], s[7])),
        invoke: static (d, a) => Admit(((Func<double, double, double, double, double, double, double, double, double>)d)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7])));
    public static readonly CompileArity Variadic = new("variadic", -1,
        lower: static (e, s) => Fin.Succ<Delegate>(Interpret(e.Compile(s))),
        invoke: static (d, a) => Admit(((Func<double[], Complex>)d)([.. a])));

    private readonly int rank;
    private readonly Func<Entity, Entity.Variable[], Fin<Delegate>> lower;
    private readonly Func<Delegate, ImmutableArray<double>, Fin<double>> invoke;

    public int Rank => rank;

    // Symbol-order count selects the row; typed compiled-IL to eight, the FastExpression interpreter absorbing nine-plus.
    internal static CompileArity Select(int symbolCount) =>
        symbolCount switch {
            0 => Nullary, 1 => Unary, 2 => Binary, 3 => Ternary, 4 => Quaternary,
            5 => Quinary, 6 => Senary, 7 => Septenary, 8 => Octonary, _ => Variadic,
        };

    internal Fin<Delegate> Lower(Entity entity, Entity.Variable[] variables) => lower(entity, variables);

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
}

// --- [MODELS] ----------------------------------------------------------------------------
// Constant-folded nullary rows carry `Constant` and NO evaluator; every other arity carries the evaluator and
// no constant, so both slots stay exclusive by construction and neither can hold a placeholder.
public sealed record CompiledExpr(
    UInt128 ContentKey,
    Seq<string> SymbolOrder,
    CompileArity Arity,
    Option<Delegate> Evaluator,
    Option<double> Constant) {
    public Fin<double> Invoke(ImmutableArray<double> arguments) =>
        Arity is null
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<null-compiled-arity>"))
        : Arity == CompileArity.Nullary
            ? Constant.Match(Some: Fin.Succ, None: () => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<nullary-without-constant>")))
        : arguments.Length != SymbolOrder.Count
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<arity:{arguments.Length}≠{SymbolOrder.Count}>"))
        : !arguments.All(double.IsFinite)
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<non-finite-argument>"))
        : Evaluator.Match(
            Some: evaluator => Try.lift<Fin<double>>(() => Arity.Invoke(evaluator, arguments)).Run()
                .MapFail(static error => (Error)new ComputeFault.SymbolUndefined($"<compiled-invoke:{error.Message}>"))
                .Bind(identity),
            None: () => Fin.Fail<double>(new ComputeFault.SymbolUndefined("<compiled-without-evaluator>")));
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
        // Rank 0 CONSTANT-FOLDS at compile: the value evaluates once and seats on the carrier, so no delegate is
        // lowered, no delegate is invoked, and no placeholder exists for a caller to reach.
        if (arity == CompileArity.Nullary) {
            return source.Evaluate(Map<string, double>())
                .Map(value => new CompiledExpr(source.ContentKey, symbolOrder, arity, None, Some(value)));
        }
        if (source.Entity.Nodes.Any(static n => n is Entity.CalculusOperator or Entity.Set or Entity.Statement)) {
            return Fin.Fail<CompiledExpr>(new ComputeFault.NonDifferentiable($"<compile-residue:{source.Canonical}>"));
        }
        Entity.Variable[] variables = symbolOrder.Map(MathS.Var).ToArray();
        // Engine compile can reject a node the residue gate cannot see; `Try.lift` converts that seam once.
        return Try.lift(() => arity.Lower(source.Entity, variables))
            .Run()
            .MapFail(error => (Error)new ComputeFault.NonDifferentiable($"<compile-declined:{source.Canonical}:{error.Message}>"))
            .Bind(identity)
            .Map(evaluator => new CompiledExpr(source.ContentKey, symbolOrder, arity, Some(evaluator), None));
    }
}
```

## [03]-[LOWERING_CACHE]

- Owner: `CompiledKey` length-frames UTF-8 symbol names beside explicit little-endian `SymbolicExpr.ContentKey` bytes before one `XxHash128`; `LoweringSlot` wraps `Fin<CompiledExpr>` as immutable L1 state; `LoweringCache` composes the shared `HybridCache` with distributed storage disabled.
- Cases: one `LoweringSlot` per content key — a compiled success and a deterministic `NonDifferentiable` decline both ride the same `Fin<CompiledExpr>` slot under the lane TTL, so a re-attempt of a deterministically-declining formula serves the cached decline rather than re-running the engine compile; the entry is L1-only because a compiled `Delegate` is not durably serializable — the `DisableDistributedCache` flag bypasses the L2 tier entirely, so a cross-process consumer re-lowers from the content-addressed key.
- Entry: `Through(SymbolicExpr, Seq<string> symbolOrder, CancellationToken)` is the one read-through — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult` under a `symbolic:` prefix, and dispatches `cache.GetOrCreateAsync(...)` where the stampede-aware factory runs `CompileCapsule.Compile` and wraps the `Fin` in a `LoweringSlot`, so identical-formula-identical-order calls coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering; `Evict(source, symbolOrder)` drops one content-keyed slot through the lane's own `Remove` and `Purge()` cuts every symbolic slot through `Invalidate(CacheLane.ModelResult, Seq(SymbolicOwner))` — the mandatory teardown surface because a live L1 delegate pins its collectible `AssemblyLoadContext`, while the lane's model-result entries, framed under their own owner tags, survive the cut untouched.
- Auto: `CompiledKey.Of` writes both `UInt128` halves little-endian and length-prefixes every UTF-8 symbol, so symbol order and boundaries are collision-distinct across runtimes. `GetOrCreateAsync` owns single-flight population, and the entry copies shared expiration policy while adding only `DisableDistributedCache`.
- Receipt: the lowering rides the model-lane `ComputeReceipt.Cache` hit/miss/store slots, never a parallel receipt; a cached compile-decline rides the `NonDifferentiable` 2215 arm — no new case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed)`/`XxHash3.HashToUInt64`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, `HybridCacheEntryFlags.DisableDistributedCache`, reached over `CacheLane.ModelResult`, never registered here), System.ComponentModel (`[ImmutableObject(true)]`), LanguageExt.Core (`Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` descriptor).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/inference#RESULT_CACHE`; a target-runtime contributor that changes delegate identity is one more stamp in `CompiledKey.Of`; a new cache substrate is rejected.
- Boundary: tags MINT at `CacheLane.Tag` alone — this cache names its owner key and the lane frames it, so a free-string tag stamped at the write and a `RemoveByTagAsync` over that string are one defect in two halves, cutting nothing the lane can find. `LoweringCache` never owns a cache instance — a hand-rolled `ConcurrentDictionary<UInt128, CompiledExpr>` memoization is the deleted form; a `source.Canonical` string key is redundant because the content key already digests the canonical form; keying by the ONNX `ModelResultKey` is rejected because a compiled formula carries no `ModelIdentity`/`ExecutionProvider`/`ModelPrecision`; a `DisableDistributedCacheWrite`-only half-measure is rejected (it leaves the entry probing a permanently-empty L2 on every miss), and an "L2 carries a re-lowering seed" design is illusory because a seed without the source `Entity` cannot reconstruct the delegate; caching the bare `Fin<CompiledExpr>` instead of the `[ImmutableObject]` `LoweringSlot` is rejected because HybridCache serializes the non-immutable value and fails on the `Delegate`; a caller that compiles-then-caches in two steps duplicates the stampede lock the `GetOrCreateAsync` single-flight owns.

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
                    BinaryPrimitives.WriteUInt64LittleEndian(frame, ContentHash.Half(source.ContentKey, 0));
                    BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], ContentHash.Half(source.ContentKey, 1));
                    BinaryPrimitives.WriteUInt64LittleEndian(frame[16..], XxHash3.HashToUInt64(symbols.WrittenSpan));
                    return XxHash128.HashToUInt128(frame);
                })
                .Run()
                .MapFail(static error => (Error)new ComputeFault.SymbolUndefined($"<compiled-key:{error.Message}>"));
}

// --- [SERVICES] --------------------------------------------------------------------------
public sealed class LoweringCache(HybridCache cache) {
    // OWNER keys cross the lane seam, never tags: `CacheLane.ModelResult.Tag` frames this owner into the lane's
    // own tag space and every write carries the bare lane key beside it, so a raw `"symbolic-lowering"` literal
    // stamped at the call site would be a tag no lane ever minted and no `Invalidate` could ever reach.
    const string SymbolicOwner = "symbolic-lowering";

    public async ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        await CompiledKey.Of(source, symbolOrder).Match(
            Fail: static error => new ValueTask<Fin<CompiledExpr>>(Fin.Fail<CompiledExpr>(error)),
            Succ: async key => (await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"symbolic:{key:x32}"),
                (Source: source, Order: symbolOrder),
                static (state, _) => new ValueTask<LoweringSlot>(new LoweringSlot(CompileCapsule.Compile(state.Source, state.Order))),
                LoweringEntry.Compiled,
                [CacheLane.ModelResult.Key, CacheLane.ModelResult.Tag(SymbolicOwner)],
                token)).Result);

    // Every compiled delegate roots its `AssemblyLoadContext`; `Evict` drops one key through the lane's physical
    // remove and `Purge` cuts the symbolic owner tag before collectible-context unload, while the lane's
    // model-result entries — framed under their own owner tags — survive the cut untouched.
    public async ValueTask<Fin<Unit>> Evict(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        await CompiledKey.Of(source, symbolOrder).Match(
            Fail: static error => new ValueTask<Fin<Unit>>(Fin.Fail<Unit>(error)),
            Succ: async key => { await cache.Remove(CacheLane.ModelResult, $"symbolic:{key:x32}", token); return Fin.Succ(unit); });

    public ValueTask Purge(CancellationToken token = default) =>
        cache.Invalidate(CacheLane.ModelResult, Seq(SymbolicOwner), token);
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
- Boundary: the symbolic-Jacobian arm is the additive `DesignVariable.Symbolic` case the optimizer admits — a standalone `GradientSource`, a parallel `(Seq<double>, double)` path, or a `Seq<SymbolicTape>` composition is rejected. `AdjointTape` is a closed `[Union]` whose `Geometry` case carries the composable `Seq<GeometryTape>` and whose `Symbolic` case carries one scalar `SymbolicTape`; each arm retains its honest arity under one optimizer dispatch. `SymbolicAdjoint.Chain(SymbolicTape, ReadOnlyMemory<float>)` stays two-argument because the design point lives on the tape, and `Lower` runs only after `Symbolic/dimensional#DIMENSION_PROOF` admits. Re-pointing that tape at the current design state is `tape with { DesignPoint = origin }` — the SANCTIONED per-iteration move `Solver/optimizer#OPTIMIZER_LANE` makes before each reverse sweep, because the partials are position-independent and only the evaluation point moves; re-lowering the Jacobian per iteration re-compiles what the cache already holds, and reusing a stale point silently returns the first iterate's gradient forever.

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
        return await free.Traverse(symbol => SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol))).As()
            .Match(
                Fail: error => new ValueTask<Fin<SymbolicTape>>(Fin.Fail<SymbolicTape>(error)),
                Succ: async partials => {
                    Fin<CompiledExpr>[] compiled = await Task.WhenAll(partials.Map(partial => cache.Through(partial, free, token).AsTask()));
                    return toSeq(compiled).Traverse(static slot => slot).Map(rows => new SymbolicTape(designSymbols, active, rows, designPoint)).As();
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
            .Map(gradient => Contract(gradient, tape.ActiveIndices, tape.DesignSymbols.Count, seed))
            .As();
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

- Owner: `Interval` the inf-sup carrier whose algebraic operations round outward through `Math.BitDecrement`/`Math.BitIncrement` and whose transcendental arms carry an accumulated soundness `Pad`; `EnclosureFold` the `Entity` tree fold evaluating a formula over a box domain in interval arithmetic; `IntervalVerdict` `[Union]` the three-way constraint pre-gate a `g(x) <= 0` question answers over an entire box in one evaluation; `ColumnProgram` the stack-allocated register program lowering one formula onto `TensorPrimitives` span kernels so N design points evaluate as columns; `ColumnPlan` its emit state and `ColumnStep` its one instruction row. Both modalities key on the same canonical-NF `SymbolicExpr.ContentKey` the scalar lowering keys on.
- Cases: `IntervalVerdict` cases `ProvenSatisfied` (the padded upper bound <= 0 — every point of the box satisfies), `ProvenViolated` (the padded lower bound > 0 — no point can), `Indeterminate(Interval)` (the sound enclosure straddles zero — the box splits or the exact engine answers); `ColumnStep` kinds `Variable` (input-column bind) · `Constant` (broadcast fill) · `Unary` · `Binary` over the verified kernel set `Add`/`Subtract`/`Multiply`/`Divide`/`Pow`/`Abs`/`Log`/`Negate`.
- Law: soundness is TWO claims, not one. `+`, `-`, `*`, `/`, and negation are correctly rounded under IEEE double, so the outward step after each is a PROOF and those bounds are a guaranteed enclosure at one ULP. `Math.Log` and `Math.Pow` are NOT correctly rounded — the BCL contracts them to a few ULP — so every arm reaching them is a WIDENED ESTIMATE: it pads by a directed multi-ULP step with a relative floor, and that pad accumulates through every downstream operation. `Certify` therefore reads the SOUND range, so a verdict whose path crossed a transcendental arm demotes to `Indeterminate` unless its margin exceeds the accumulated pad — a proof that a widened estimate cannot support is exactly the false certificate this split exists to refuse.
- Entry: `EnclosureFold.Enclose(SymbolicExpr, Seq<string> symbolOrder, ImmutableArray<Interval> box)` folds the tree over the catalog-verified node records — `Sumf`/`Minusf`/`Mulf`/`Divf`/`Powf`/`Absf`/`Signumf`/`Logf`, `Variable`, the numeric leaves — and declines any other node typed, so the enclosure never silently widens to `(-inf, +inf)` on a node it cannot bound; `Certify(...)` projects the sound enclosure onto `IntervalVerdict`. `ColumnProgram.Lower(SymbolicExpr, Seq<string> symbolOrder)` compiles the enclosure node set minus `Signumf` (no verified `TensorPrimitives` sign kernel exists, so the sign node declines typed to the scalar fallback) into a `Seq<ColumnStep>` register program — `Logf` lowers as `Log(x)/Log(b)` and a `-1` multiplier lowers as the `Negate` kernel; `Evaluate(ReadOnlyMemory<double>[] columns)` runs it over one pooled register file in a handful of SIMD passes — a 10^4-point DOE grid pays tens of span kernels instead of 10^4 delegate dispatches; a formula outside the lowered node set declines typed and the caller loops the scalar `CompiledExpr.Invoke` as the honest fallback.
- Law: register allocation is STACK-DISCIPLINED, because the post-order emit already is: every operand sits at the top of the register stack the moment its consumer lands, so a consumer writes over its own operands and the PEAK depth — not the node count — is the rent. Monotone one-register-per-node counting rents the whole expression tree for values live exactly one step, so a 200-node formula over a 10^4-point grid rented 16 MB where peak depth rents a handful of columns. Both bounds refuse rather than allocate: `RegisterCeiling` caps the peak depth at lowering and `RentCeiling` caps the `Registers x N` doubles at evaluation, so a pathological formula or an oversized grid is a typed decline, never an out-of-memory throw from inside a span kernel.
- Receipt: none of its own — a branch-and-prune consumer counts discarded boxes on its own receipt, and the column sweep rides the sweep's `ComputeReceipt.Sweep`; an enclosure or lowering decline rides the `ComputeFault.NonDifferentiable` 2215 arm exactly as a compile decline does.
- Packages: AngouriMath (the positional node records, `IUnaryNode.NodeChild`, `IBinaryNode.NodeFirstChild`/`NodeSecondChild`), System.Numerics.Tensors (`TensorPrimitives.Add`/`Subtract`/`Multiply`/`Divide`/`Pow`/`Abs`/`Log`/`Negate` span kernels), CommunityToolkit.HighPerformance (`MemoryOwner<double>` register file), LanguageExt.Core (`Fin`, `Seq`), BCL inbox (`Math.BitDecrement`/`BitIncrement`, `FrozenDictionary`).
- Growth: a new bounded node family (the trig records, once their monotonicity split lands) is one arm on BOTH folds — the interval bound and the column kernel land together or the node stays declined; a tighter enclosure (affine arithmetic, mean-value forms) is a policy row on `EnclosureFold`, never a sibling evaluator; a new column kernel is one row in the unary or binary kernel table binding its verified `TensorPrimitives` member.
- Boundary: the pre-gate serves `Solver/satisfy#RULE_SATISFACTION` — a rule whose enclosure proves over the declared bounds never spends the Z3 timeout, and `Indeterminate` falls through to the exact check, so the gate is a filter, never a verdict authority; `Solver/optimizer` box screening discards `ProvenViolated` regions without oracle calls. Interval division by a zero-straddling denominator, a NEGATIVE integer power over a zero-straddling base (the same reciprocal, so the same refusal), and `Logf` over a non-positive interval each decline typed rather than returning an infinite enclosure that certifies nothing; every arm that can leave the finite range re-checks `Valid` before it returns, so a non-finite bound faults at the node that produced it instead of propagating as an enclosure. Kernel dispatch is TOTAL — the unary and binary op tables are frozen and a step naming no row is a typed decline, never a silent fall-through into whichever kernel the last `else` happened to hold. Each column program is an evaluation plan over already-admitted values — it re-validates nothing, and the register file is pooled and returned deterministically.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
// `Lo`/`Hi` are the computed bounds and `Pad` is the accumulated soundness margin the transcendental arms owe.
// Keeping them apart lets a consumer read the computed range while every verdict reads `Sound`, so widening is
// never mistaken for a wider answer and a zero pad recovers the exact algebraic rule with no second code path.
public readonly record struct Interval(double Lo, double Hi, double Pad) {
    // BCL `Math.Log`/`Math.Pow` are contracted to a few ULP and are NOT correctly rounded, so a transcendental
    // bound owes a directed multi-ULP step; the relative floor keeps a bound near zero from claiming exactness.
    const int TranscendentalUlps = 4;
    const double TranscendentalRelative = 1e-15;

    // Correctly-rounded algebra: the outward step alone is the proof, and the pad stays whatever flowed in.
    public static Interval Of(double lo, double hi) => new(Math.BitDecrement(lo), Math.BitIncrement(hi), 0.0);

    // Estimated algebra: the outward step plus the accumulated margin the arm cannot discharge.
    public static Interval Widened(double lo, double hi, double carried) =>
        new(Math.BitDecrement(lo), Math.BitIncrement(hi), carried + Margin(lo, hi));

    // Leaf constants stay exactly representable — no outward expansion, so an exact integer exponent stays
    // detectable by the integer-power law; rounding applies after OPERATIONS, never on the leaf itself.
    public static Interval Point(double value) => new(value, value, 0.0);

    public bool Valid => double.IsFinite(Lo) && double.IsFinite(Hi) && double.IsFinite(Pad) && Pad >= 0.0 && Lo <= Hi;
    public bool Contains(double value) => value >= Lo && value <= Hi;

    // Enclosure a verdict is entitled to assert: computed bounds opened by everything the path could not prove.
    public Interval Sound => new(Lo - Pad, Hi + Pad, 0.0);

    public static Interval operator +(Interval a, Interval b) => Of(a.Lo + b.Lo, a.Hi + b.Hi) with { Pad = a.Pad + b.Pad };
    public static Interval operator -(Interval a, Interval b) => Of(a.Lo - b.Hi, a.Hi - b.Lo) with { Pad = a.Pad + b.Pad };
    public static Interval operator -(Interval a) => new(-a.Hi, -a.Lo, a.Pad);

    public static Interval operator *(Interval a, Interval b) {
        ReadOnlySpan<double> products = [a.Lo * b.Lo, a.Lo * b.Hi, a.Hi * b.Lo, a.Hi * b.Hi];
        return Of(TensorPrimitives.Min(products), TensorPrimitives.Max(products)) with { Pad = a.Pad + b.Pad };
    }

    public Interval Abs() => Lo >= 0.0 ? this : Hi <= 0.0 ? -this : Of(0.0, Math.Max(-Lo, Hi)) with { Pad = Pad };

    static double Margin(double lo, double hi) {
        double magnitude = Math.Max(Math.Abs(lo), Math.Abs(hi));
        return (TranscendentalUlps * (Math.BitIncrement(magnitude) - magnitude)) + (TranscendentalRelative * magnitude);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IntervalVerdict {
    private IntervalVerdict() { }
    public sealed record ProvenSatisfied(Interval Range) : IntervalVerdict;
    public sealed record ProvenViolated(Interval Range) : IntervalVerdict;
    public sealed record Indeterminate(Interval Range) : IntervalVerdict;
}

// `Left` names an input COLUMN on a `Variable` step and a register on every other kind, because variables are
// what read outside the register file.
public readonly record struct ColumnStep(int Target, int Left, int Right, TensorOpFamily Op, double Scalar, ColumnStepKind Kind);

[SmartEnum<string>]
public sealed partial class ColumnStepKind {
    public static readonly ColumnStepKind Variable = new("variable");
    public static readonly ColumnStepKind Constant = new("constant");
    public static readonly ColumnStepKind Unary = new("unary");
    public static readonly ColumnStepKind Binary = new("binary");
}

// Emit state: the step list, the stack top, and the high-water mark that becomes the rented register count.
public readonly record struct ColumnPlan(Seq<ColumnStep> Steps, int Top, int Peak) {
    public static readonly ColumnPlan Empty = new(Seq<ColumnStep>(), 0, 0);

    public ColumnPlan Push(ColumnStep step) => new(Steps.Add(step), Top + 1, Math.Max(Peak, Top + 1));

    // Consumers WRITE OVER their operands: the stack falls by the operand count and rises by one result.
    public ColumnPlan Fuse(ColumnStep step, int operands) => new(Steps.Add(step), Top - operands + 1, Peak);
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class EnclosureFold {
    public static Fin<Interval> Enclose(SymbolicExpr source, Seq<string> symbolOrder, ImmutableArray<Interval> box) =>
        source.Entity is null || symbolOrder.Count != box.Length || !box.All(static i => i.Valid)
            ? Fin.Fail<Interval>(new ComputeFault.SymbolUndefined("<enclosure-box-invalid>"))
            : Descend(source.Entity, toSeq(symbolOrder).Zip(toSeq(box)).ToMap());

    // Constraint pre-gate over g(x) <= 0 read against the SOUND range, so a path that crossed a widened
    // transcendental arm proves only when its margin clears the pad that arm could not discharge.
    public static Fin<IntervalVerdict> Certify(SymbolicExpr constraint, Seq<string> symbolOrder, ImmutableArray<Interval> box) =>
        Enclose(constraint, symbolOrder, box).Map(range => (IntervalVerdict)(
            range.Sound.Hi <= 0.0 ? new IntervalVerdict.ProvenSatisfied(range)
            : range.Sound.Lo > 0.0 ? new IntervalVerdict.ProvenViolated(range)
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
                : Finite(a * (Interval.Of(1.0 / b.Hi, 1.0 / b.Lo) with { Pad = b.Pad }), "div")
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
                ? Finite(Corners(x, b, Math.Log), "log")
                : Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-log-domain>"))
            select r,
        _ => Fin.Fail<Interval>(new ComputeFault.NonDifferentiable($"<enclosure-node:{node.GetType().Name}>")),
    };

    // Every arm that can leave the finite range re-checks admissibility at the node that produced the bound,
    // because an infinite or inverted enclosure downstream reports the wrong node as the cause.
    static Fin<Interval> Finite(Interval value, string node) =>
        value.Valid
            ? Fin.Succ(value)
            : Fin.Fail<Interval>(new ComputeFault.NonDifferentiable($"<enclosure-nonfinite:{node}>"));

    // Positive-base bivariate maps stay monotone in each argument for the other fixed, so extrema over a box
    // sit on the four corners; partial corner pairings under-enclose whenever a monotonicity direction flips.
    // Both `f` bindings are BCL transcendental kernels, so the result is widened, never proven.
    static Interval Corners(Interval x, Interval y, Func<double, double, double> f) {
        double a = f(x.Lo, y.Lo), b = f(x.Lo, y.Hi), c = f(x.Hi, y.Lo), d = f(x.Hi, y.Hi);
        return Interval.Widened(Math.Min(Math.Min(a, b), Math.Min(c, d)), Math.Max(Math.Max(a, b), Math.Max(c, d)), x.Pad + y.Pad);
    }

    // Integer exponents split by parity and base sign; a non-integer exponent demands a positive base.
    static Fin<Interval> Power(Interval baseRange, Interval exponent) =>
        exponent.Lo == exponent.Hi && double.IsInteger(exponent.Lo)
            ? IntegerPower(baseRange, (int)exponent.Lo)
            : baseRange.Lo > 0.0
                ? Finite(Corners(baseRange, exponent, Math.Pow), "pow")
                : Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-pow-domain>"));

    // NEGATIVE exponents are reciprocals, so a positive-power range straddling zero has no bound at all — same
    // refusal `Divf` makes, because the alternative is an infinite enclosure certifying nothing.
    static Fin<Interval> IntegerPower(Interval a, int n) =>
        n == 0 ? Fin.Succ(Interval.Point(1.0))
        : n < 0 ? IntegerPower(a, -n).Bind(static p => p.Contains(0.0)
            ? Fin.Fail<Interval>(new ComputeFault.NonDifferentiable("<enclosure-zero-straddling-power>"))
            : Finite(Interval.Of(1.0 / p.Hi, 1.0 / p.Lo) with { Pad = p.Pad }, "pow"))
        : (n & 1) == 0 && a.Contains(0.0)
            ? Finite(Interval.Widened(0.0, Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), a.Pad), "pow")
            : Finite(Interval.Widened(Math.Min(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), a.Pad), "pow");
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public sealed record ColumnProgram(UInt128 ContentKey, Seq<string> SymbolOrder, Seq<ColumnStep> Steps, int Registers) {
    // Peak stack depth a lowered formula may rent, and the total doubles one evaluation may hold live. Both are
    // refusals rather than clamps: a clamped program silently computes a different plan than the one authored.
    const int RegisterCeiling = 64;
    const long RentCeiling = 1L << 26;

    delegate void UnarySpan(ReadOnlySpan<double> source, Span<double> target);

    delegate void BinarySpan(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> target);

    // Frozen op tables make the kernel dispatch TOTAL: a step naming no row is a typed decline, where an
    // if-ladder falling through to its last `else` runs whichever kernel that branch happened to hold.
    static readonly FrozenDictionary<TensorOpFamily, UnarySpan> UnaryKernels =
        new KeyValuePair<TensorOpFamily, UnarySpan>[] {
            new(TensorOpFamily.Abs, TensorPrimitives.Abs<double>),
            new(TensorOpFamily.Negate, TensorPrimitives.Negate<double>),
            new(TensorOpFamily.Log, TensorPrimitives.Log<double>),
        }.ToFrozenDictionary();

    static readonly FrozenDictionary<TensorOpFamily, BinarySpan> BinaryKernels =
        new KeyValuePair<TensorOpFamily, BinarySpan>[] {
            new(TensorOpFamily.Add, TensorPrimitives.Add<double>),
            new(TensorOpFamily.Subtract, TensorPrimitives.Subtract<double>),
            new(TensorOpFamily.Multiply, TensorPrimitives.Multiply<double>),
            new(TensorOpFamily.Divide, TensorPrimitives.Divide<double>),
            new(TensorOpFamily.Pow, TensorPrimitives.Pow<double>),
        }.ToFrozenDictionary();

    public static Fin<ColumnProgram> Lower(SymbolicExpr source, Seq<string> symbolOrder) =>
        source.Entity is null || symbolOrder.Exists(string.IsNullOrWhiteSpace) || symbolOrder.Distinct().Count != symbolOrder.Count
            ? Fin.Fail<ColumnProgram>(new ComputeFault.SymbolUndefined("<column-lowering-invalid>"))
            : Emit(source.Entity, symbolOrder, ColumnPlan.Empty).Bind(plan => plan.Peak <= RegisterCeiling
                ? Fin.Succ(new ColumnProgram(source.ContentKey, symbolOrder, plan.Steps, plan.Peak))
                : Fin.Fail<ColumnProgram>(new ComputeFault.NonDifferentiable($"<column-registers:{plan.Peak}-over-{RegisterCeiling}>")));

    // One pooled register file; each step is one span kernel over all N points — the point loop is inside the
    // kernel, and the statement body is this page's named span-kernel exemption.
    public Fin<ReadOnlyMemory<double>> Evaluate(params ReadOnlyMemory<double>[] columns) {
        if (columns.Length != SymbolOrder.Count || columns.Length == 0 || !columns.All(c => c.Length == columns[0].Length)) {
            return Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.SymbolUndefined("<column-arity-or-length>"));
        }
        int n = columns[0].Length;
        long rent = (long)Registers * n;
        if (rent > RentCeiling) {
            return Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.NonDifferentiable($"<column-rent:{rent}-over-{RentCeiling}>"));
        }
        using MemoryOwner<double> file = MemoryOwner<double>.Allocate(Registers * n, AllocationMode.Clear);
        foreach (ColumnStep step in Steps) {
            Span<double> target = file.Span.Slice(step.Target * n, n);
            if (step.Kind == ColumnStepKind.Variable) { columns[step.Left].Span.CopyTo(target); continue; }
            if (step.Kind == ColumnStepKind.Constant) { target.Fill(step.Scalar); continue; }
            ReadOnlySpan<double> left = file.Span.Slice(step.Left * n, n);
            if (step.Kind == ColumnStepKind.Unary && UnaryKernels.TryGetValue(step.Op, out UnarySpan? unary)) { unary(left, target); continue; }
            if (step.Kind == ColumnStepKind.Binary && BinaryKernels.TryGetValue(step.Op, out BinarySpan? binary)) {
                binary(left, file.Span.Slice(step.Right * n, n), target);
                continue;
            }
            return Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.NonDifferentiable($"<column-kernel:{step.Kind.Key}:{step.Op.Key}>"));
        }
        // Post-order emit leaves the whole formula's value in register 0 with the stack at depth one.
        double[] result = file.Span[..n].ToArray();
        return TensorPrimitives.IsFiniteAll<double>(result)
            ? Fin.Succ<ReadOnlyMemory<double>>(result)
            : Fin.Fail<ReadOnlyMemory<double>>(new ComputeFault.SymbolUndefined("<column-non-finite>"));
    }

    static Fin<ColumnPlan> Emit(Entity node, Seq<string> order, ColumnPlan plan) => node switch {
        Entity.Variable v => toSeq(Enumerable.Range(0, order.Count)).Find(i => order[i] == v.Name).Match(
            Some: column => Fin.Succ(plan.Push(new ColumnStep(plan.Top, column, column, TensorOpFamily.Add, 0.0, ColumnStepKind.Variable))),
            None: () => Fin.Fail<ColumnPlan>(new ComputeFault.SymbolUndefined($"<column-unbound:{v.Name}>"))),
        Entity.Number number => NumberBox.Project(number).Map(value =>
            plan.Push(new ColumnStep(plan.Top, 0, 0, TensorOpFamily.Add, value, ColumnStepKind.Constant))),
        Entity.Sumf s => Binary(s.Augend, s.Addend, TensorOpFamily.Add, order, plan),
        Entity.Minusf m => Binary(m.Subtrahend, m.Minuend, TensorOpFamily.Subtract, order, plan),
        Entity.Mulf m when IsNegOne(m.Multiplier) => Unary(m.Multiplicand, TensorOpFamily.Negate, order, plan),
        Entity.Mulf m => Binary(m.Multiplier, m.Multiplicand, TensorOpFamily.Multiply, order, plan),
        Entity.Divf d => Binary(d.Dividend, d.Divisor, TensorOpFamily.Divide, order, plan),
        Entity.Powf p => Binary(p.Base, p.Exponent, TensorOpFamily.Pow, order, plan),
        Entity.Absf a => Unary(a.Argument, TensorOpFamily.Abs, order, plan),
        // log_b(x) lowers as Log(x)/Log(b) — two unary Log steps that reuse their own operand slots and one
        // Divide that folds both, so the whole node costs two registers rather than five.
        Entity.Logf l =>
            Unary(l.Antilogarithm, TensorOpFamily.Log, order, plan)
                .Bind(x => Unary(l.Base, TensorOpFamily.Log, order, x))
                .Map(static b => b.Fuse(new ColumnStep(b.Top - 2, b.Top - 2, b.Top - 1, TensorOpFamily.Divide, 0.0, ColumnStepKind.Binary), operands: 2)),
        _ => Fin.Fail<ColumnPlan>(new ComputeFault.NonDifferentiable($"<column-node:{node.GetType().Name}>")),
    };

    static bool IsNegOne(Entity node) =>
        node is Entity.Number number && NumberBox.Project(number).Map(static value => value == -1.0).IfFail(false);

    static Fin<ColumnPlan> Binary(Entity left, Entity right, TensorOpFamily op, Seq<string> order, ColumnPlan plan) =>
        Emit(left, order, plan).Bind(l => Emit(right, order, l)
            .Map(r => r.Fuse(new ColumnStep(r.Top - 2, r.Top - 2, r.Top - 1, op, 0.0, ColumnStepKind.Binary), operands: 2)));

    static Fin<ColumnPlan> Unary(Entity argument, TensorOpFamily op, Seq<string> order, ColumnPlan plan) =>
        Emit(argument, order, plan)
            .Map(a => a.Fuse(new ColumnStep(a.Top - 1, a.Top - 1, a.Top - 1, op, 0.0, ColumnStepKind.Unary), operands: 1));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
