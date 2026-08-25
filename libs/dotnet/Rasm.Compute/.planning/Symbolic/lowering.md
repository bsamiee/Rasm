# [SYMBOLIC_LOWERING]

Compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` lowers once to a native delegate through the engine's typed `Compile<TIn1..TIn8, TOut>(vars)` IL-compiling surface (arities one through eight instantiate `double` and lower through the LINQ-expression protocol) with the interpreter `Compile(params Variable[]) → FastExpression` absorbing every arity past eight, carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` mints, and reused through a `LoweringCache` read-through over the one model-lane `HybridCache` (`Model/run#RESULT_CACHE` `CacheLane.ModelResult`, never a second instance). Owned here: the `CompiledExpr` carrier and its `CompiledBody` constant-or-lowered split, the `CompileArity` `[SmartEnum<string>]` that selects and owns the arity-exact compile-and-invoke behavior (one delegate-backed row per arity, the variadic row retaining `Complex` until the real-result gate), the `LoweringCache` L1-only read-through on the `IO` rail with its `LoweringSlot` carrier and `CompiledKey` derivation composing the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` law, and the cross-lane `SymbolicJacobian` that differentiates a formula by each free design symbol, compiles each partial behind that partial's OWN dimensional proof, and packs the partials WITH the design point into a `SymbolicTape` whose `Backward` answers the same two-argument reverse-mode contract `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain` answers. Symbolic gradients enter solely as the additive `DesignVariable.Symbolic` arm the optimizer admits.

Lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before, and the gate is EXECUTABLE rather than declared: `Compile` takes the `DimensionVerdict` that admitted this formula and refuses a verdict whose `Proved` key names another tree, so a dimension-inconsistent expression cannot reach a `CompiledExpr` slot, the optimizer oracle, or the integrator seed — and the carrier stamps the proven monomial and its unique family, so a compiled formula reports what its output MEANS. Host-local, no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior value, and the only cross-surface fact is the `SymbolicExpr.ContentKey` crossing by reference to the `Rasm.Persistence/Query/cache#MODEL_RESULT_INDEX` cost-catalog/QTO consumers, keyed by its OWN content identity, never a fabricated `ModelResultKey`. In-proc symbolic-regression fitting is the rejected form: offline formula discovery is the Python branch's, and compile-and-cache, the analytic-Jacobian lowering, and the box enclosure over an already-admitted expression are all this owner holds.

## [01]-[INDEX]

- [02]-[LOWERING]: `CompiledExpr` carrier over the `CompiledBody` constant-or-lowered union; `CompileArity` delegate-backed arity owner (compile + invoke in one row); the typed `Compile<>`/interpreter `FastExpression` lowering behind the dimensional-proof gate.
- [03]-[LOWERING_CACHE]: `LoweringCache` L1-only `IO` read-through over `CacheLane.ModelResult`; `LoweringSlot` `[ImmutableObject]` carrier; `CompiledKey` content-key derivation.
- [04]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering, each partial proved on its own; `SymbolicTape` (design point baked in) and its two-argument transpose `Backward`.
- [05]-[ENCLOSURE]: `Enclosure`/`EnclosureFold` range enclosure over a box domain — algebraic bounds proven, transcendental bounds pad-widened — with the `EnclosureVerdict` constraint pre-gate.

## [02]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a `CompiledBody` to its source content key, ordered free-symbol vector, and proven result dimension; `CompiledBody` the `[Union]` splitting a folded `Constant` from a `Lowered` delegate; `CompileArity` the `[SmartEnum<string>]` whose nine rows (`unary` … `octonary`, `variadic`) each own both compile form and invoke form as delegate-backed behavior — a typed row's `Lower` instantiates the arity-exact `Entity.Compile<double, …>(vars)` generic, its `Invoke` performs the strongly-typed down-cast, and the variadic row's `Lower` wraps the `Entity.Compile(params Variable[]) → FastExpression` interpreter in one `Func<double[], Complex>` closure so no `FastExpression` leaks past the row and no imaginary residual is discarded; `CompileCapsule` the one boundary owner gating the lowering — it admits the source, the proof, and the symbol order together, pre-validates no analytic residue (`Derivativef`/`Integralf`/`Limitf`) and no non-numeric node (`Set`/`Statement`), then converts the engine's compile throw onto the `ComputeFault.NonDifferentiable` rail.
- Cases: `CompiledBody` — `Constant(double)` for a free-symbol-empty formula, `Lowered(CompileArity, Delegate)` for every other; `CompileArity` rows `unary` … `octonary` (ranks 1–8, the engine's COMPLETE strongly-typed generic set, each `Func<double,…,double>`) and `variadic` (rank −1, the `FastExpression` interpreter behind a `Func<double[], Complex>`, reached only past eight); every row returns `Fin<double>` after finite/real admission, and `CompileArity.Select(symbolOrder.Count)` returns `Option<CompileArity>` — absence IS the constant case, so no roster row exists to be skipped.
- Law: the eight typed rows are HAND-WRITTEN and stay so. Each is a distinct closed generic instantiation of `Compile<double,…,double>` with its own `Func<>` down-cast, and no fold, table, or generator can produce a type argument list in C#; the ROW SET is derived — the rank axis alone selects — and that is the whole derivable part.
- Entry: `Compile(SymbolicExpr, DimensionVerdict, Seq<SymbolName>)` is the one polymorphic lowering — the symbol order fixes the positional argument convention (the i-th `double` binds the i-th symbol), `Select(Count)` picks the row or names the constant, `arity.Lower(entity, variables)` returns `Fin<Delegate>`, and the capsule's residue gate and exception seam lift the outcome onto the same rail; `Invoke(ImmutableArray<double>)` proves argument count against the symbol order and finiteness across the arguments (a mis-arity call is a `ComputeFault`, never an engine index fault) then dispatches the body totally, so the constant read, the down-cast, and the variadic array bind are all owned by their own case.
- Auto: `Compile` reads the ordered `symbolOrder` rather than `FreeSymbols` directly, so the positional convention is caller-fixed and stable across a re-compile; the typed rows hold the exact `Func<…>` the generic `Compile<>` returns, so those call sites invoke a strongly-typed compiled-IL delegate with no reflection — the interpreter path is reached only past eight symbols; a free-symbol-empty formula evaluates once through `Symbolic/expression#SYMBOLIC_EXPR` `Evaluate` and the `Constant` case carries the value, so no delegate is lowered and no placeholder exists for a caller to reach.
- Receipt: none of its own — the compile outcome rides the `LOWERING_CACHE` hit/miss/store slot the model lane's `ComputeReceipt.Cache` fact stamps, a compile-decline rides the `ComputeFault.NonDifferentiable` 2214 arm, an unproven or foreign-proof compile the `DimensionMismatch` 2215 arm, and the downstream optimize outcome carries the `Optimization` slot.
- Packages: AngouriMath (`Entity.Compile<TIn1..TIn8, TOut>(Variable…)` typed IL lowering, `Entity.Compile(params Variable[]) → FastExpression` interpreter with `Call(params Complex[]) → Complex`), Thinktecture.Runtime.Extensions (`CompileArity` `[SmartEnum]`, `CompiledBody` `[Union]`), Generator.Equals (`[Equatable]`/`[OrderedEquality]` over the carrier's sequence members), LanguageExt.Core (`Fin`, `Validation`, `Seq`/`ImmutableArray`/`Option`), Rasm (project — kernel `EpsilonPolicy.SeamUlp`, the imaginary-residual floor), `Symbolic/expression` (in-branch — `SymbolName`, `Captured`, `SymbolicExpr.Tree`), `Symbolic/dimensional` (in-branch — `DimensionVerdict`, `DimensionMonomial`), BCL inbox (`System.Numerics.Complex` at the interpreter marshal).
- Growth: a new arity past eight is impossible as a typed row — the variadic interpreter absorbs every arity of nine or more, and the eight typed generics are the complete set the engine ships; a new numeric domain (a complex-valued lane) is one companion-row family instantiating `Complex` type arguments on the SAME generic surface, never a parallel `CompiledComplexExpr`; a new evaluation convention (a `Span<double>` bind for a hot loop) is one more `Invoke` shape on the same row; a new body modality is one `CompiledBody` case and the total `Switch` breaks until its arm lands.
- Boundary: `Compile` is the single lowering entry — a `CompileUnary`/`CompileBinary`/`CompileVariadic` factory trio is the collapsed defect, and the two-parallel-switch shape (one for compile, one for invoke) is what the delegate-backed rows collapse; a compile runs only behind a `DimensionVerdict` whose `Proved` key names THIS tree, so the pre-numeric gate is a signature obligation rather than a call-order convention; the typed generic `Compile<>` is the admitted fast path lowering to IL through the engine's LINQ-expression protocol, and a hand-rolled `Reflection.Emit` or expression-tree re-implementation is the deleted form; the residue gate runs BEFORE the engine compile so the throwing seam is reached only by genuinely un-compilable nodes, and that one capture is the named platform-forced exception exemption; the positional symbol order is the one argument convention, and an unordered `Map<SymbolName,double>`-keyed invoke is rejected because the compiled delegate is positional by construction; a sentinel roster row whose columns no path reaches is the deleted form the `Option`-returning `Select` replaces.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompileArity {
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

    internal static Option<CompileArity> Select(int symbolCount) =>
        symbolCount <= 0
            ? None
            : Some(toSeq(Items).Find(row => row.Rank == symbolCount).IfNone(Variadic));

    internal Fin<Delegate> Lower(Entity entity, Entity.Variable[] variables) => lower(entity, variables);

    internal Fin<double> Invoke(Delegate evaluator, ImmutableArray<double> arguments) => invoke(evaluator, arguments);

    static Func<double[], Complex> Interpret(FastExpression fast) =>
        args => fast.Call([.. args.Select(static x => new Complex(x, 0))]);

    static Fin<double> Admit(double value) =>
        double.IsFinite(value)
            ? Fin.Succ(value)
            : Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<compiled-non-finite:{value}>"));

    static Fin<double> Admit(Complex value) =>
        double.IsFinite(value.Real) && double.IsFinite(value.Imaginary)
        && Math.Abs(value.Imaginary) <= EpsilonPolicy.SeamUlp * Math.Max(1.0, Math.Abs(value.Real))
            ? Fin.Succ(value.Real)
            : Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<compiled-non-real:{value}>"));
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompiledBody {
    private CompiledBody() { }

    public sealed record Constant(double Value) : CompiledBody;
    public sealed record Lowered(CompileArity Arity, Delegate Evaluator) : CompiledBody;
}

[Equatable]
public sealed partial record CompiledExpr(
    UInt128 ContentKey,
    [property: OrderedEquality] Seq<SymbolName> SymbolOrder,
    DimensionMonomial Dimension,
    Option<QuantityFamily> Family,
    CompiledBody Body) {
    public Fin<double> Invoke(ImmutableArray<double> arguments) =>
        arguments.Length != SymbolOrder.Count
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<arity:{arguments.Length}≠{SymbolOrder.Count}>"))
        : !arguments.All(double.IsFinite)
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined("<non-finite-argument>"))
        : Body.Switch(
            arguments,
            constant: static (c, _) => Fin.Succ(c.Value),
            lowered: static (l, args) => Captured.Of(() => l.Arity.Invoke(l.Evaluator, args)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CompileCapsule {
    public static Fin<CompiledExpr> Compile(SymbolicExpr source, DimensionVerdict proof, Seq<SymbolName> symbolOrder) =>
        Admit(source, proof, symbolOrder).Bind(tree =>
            CompileArity.Select(symbolOrder.Count).Match(
                None: () => source.Evaluate(Map<SymbolName, Finite>())
                    .Map(value => Carry(source, proof, symbolOrder, new CompiledBody.Constant(value))),
                Some: arity => tree.Nodes.Any(static n => n is Entity.CalculusOperator or Entity.Set or Entity.Statement)
                    ? Fin.Fail<CompiledExpr>(new ComputeFault.NonDifferentiable($"<compile-residue:{source.Canonical}>"))
                    : Captured.Of(() => arity.Lower(tree, symbolOrder.Map(static s => s.Var).ToArray()))
                        .Map(evaluator => Carry(source, proof, symbolOrder, new CompiledBody.Lowered(arity, evaluator)))));

    static Fin<Entity> Admit(SymbolicExpr source, DimensionVerdict proof, Seq<SymbolName> symbolOrder) =>
        (source.Tree.Match(Succ: Success<Error, Entity>, Fail: static error => Fail<Error, Entity>(error)),
         Proven(source, proof),
         Distinct(symbolOrder),
         Covering(source, symbolOrder))
            .Apply(static (tree, _, _, _) => tree)
            .As()
            .ToFin();

    static Validation<Error, Unit> Proven(SymbolicExpr source, DimensionVerdict proof) =>
        proof.Proved == source.ContentKey
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new ComputeFault.DimensionMismatch(
                $"<compile-unproven:{source.ContentKey:x32}≠{proof.Proved:x32}>"));

    static Validation<Error, Unit> Distinct(Seq<SymbolName> symbolOrder) =>
        symbolOrder.Distinct().Count == symbolOrder.Count
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new ComputeFault.ParseRejected("<symbol-order-duplicate>"));

    static Validation<Error, Unit> Covering(SymbolicExpr source, Seq<SymbolName> symbolOrder) =>
        source.FreeSymbols.Filter(symbol => !symbolOrder.Contains(symbol)) is { IsEmpty: false } missing
            ? Fail<Error, Unit>(new ComputeFault.SymbolUndefined(
                $"<symbol-order-missing:{string.Join(",", missing.Map(static s => s.Value))}>"))
            : Success<Error, Unit>(unit);

    static CompiledExpr Carry(SymbolicExpr source, DimensionVerdict proof, Seq<SymbolName> symbolOrder, CompiledBody body) =>
        new(source.ContentKey, symbolOrder, proof.Monomial, proof.Unique, body);
}
```

## [03]-[LOWERING_CACHE]

- Owner: `CompiledKey` length-frames UTF-8 symbol names beside explicit little-endian `SymbolicExpr.ContentKey` bytes before one `XxHash128`; `LoweringSlot` wraps `Fin<CompiledExpr>` as immutable L1 state; `LoweringCache` composes the shared `HybridCache` with distributed storage disabled.
- Cases: one `LoweringSlot` per content key — a compiled success and a deterministic `NonDifferentiable` decline both ride the same `Fin<CompiledExpr>` slot under the lane TTL, so a re-attempt of a deterministically-declining formula serves the cached decline rather than re-running the engine compile; the entry is L1-only because a compiled `Delegate` is not durably serializable — the `DisableDistributedCache` flag bypasses the L2 tier entirely, so a cross-process consumer re-lowers from the content-addressed key.
- Entry: `Through(SymbolicExpr, DimensionVerdict, Seq<SymbolName>)` is the one read-through, returning `IO<CompiledExpr>` — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult` under a `symbolic:` prefix, and dispatches `cache.GetOrCreateAsync(...)` where the stampede-aware factory runs `CompileCapsule.Compile` and wraps the `Fin` in a `LoweringSlot`, so identical-formula-identical-order calls coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering; `Evict(source, symbolOrder)` drops one content-keyed slot through the lane's own `Remove` and `Purge()` cuts every symbolic slot through `Invalidate(CacheLane.ModelResult, Seq(SymbolicOwner))` — the mandatory teardown surface because a live L1 delegate pins its collectible `AssemblyLoadContext`, while the lane's model-result entries, framed under their own owner tags, survive the cut untouched.
- Law: the lane rides `IO`, never a bare `async ValueTask<Fin<T>>`. Cancellation reaches the cache through `EnvIO` rather than a parameter tail every caller re-threads, the fallible key derivation composes as one `IO.lift` step instead of a `Match` returning two `ValueTask` shapes, and the Jacobian's fan-out can `Fork` these reads directly — the three members were the package's last three off-rail async entrypoints.
- Auto: `CompiledKey.Of` writes both `UInt128` halves little-endian and length-prefixes every UTF-8 symbol, so symbol order and boundaries are collision-distinct across runtimes; it re-admits NOTHING, because the capsule owns the symbol-order gate and a key derived for an inadmissible order simply misses and lands that capsule's own refusal in the slot. `GetOrCreateAsync` owns single-flight population, and the entry copies shared expiration policy while adding only `DisableDistributedCache`.
- Receipt: the lowering rides the model-lane `ComputeReceipt.Cache` hit/miss/store slots, never a parallel receipt; a cached compile-decline rides the `NonDifferentiable` 2214 arm — no new case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed)`/`XxHash3.HashToUInt64`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, `HybridCacheEntryFlags.DisableDistributedCache`, reached over `CacheLane.ModelResult`, never registered here), System.ComponentModel (`[ImmutableObject(true)]`), LanguageExt.Core (`IO.lift`/`IO.liftAsync` over `EnvIO`, `Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` descriptor).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/run#RESULT_CACHE`; a target-runtime contributor that changes delegate identity is one more stamp in `CompiledKey.Of`; a new cache substrate is rejected.
- Boundary: tags MINT at `CacheLane.Tag` alone — this cache names its owner key and the lane frames it; the spelling is the same one `Runtime/lifecycle` composes, so it stays stable across both. `LoweringCache` never owns a cache instance — a hand-rolled `ConcurrentDictionary<UInt128, CompiledExpr>` memoization is the deleted form; a `source.Canonical` string key is redundant because the content key already digests the canonical form; keying by the ONNX `ModelResultKey` is rejected because a compiled formula carries no `ModelIdentity`/`ExecutionProvider`/`ModelPrecision`; a `DisableDistributedCacheWrite`-only half-measure is rejected (it leaves the entry probing a permanently-empty L2 on every miss), and an "L2 carries a re-lowering seed" design is illusory because a seed without the source `Entity` cannot reconstruct the delegate; caching the bare `Fin<CompiledExpr>` instead of the `[ImmutableObject]` `LoweringSlot` is rejected because HybridCache serializes the non-immutable value and fails on the `Delegate`; a caller that compiles-then-caches in two steps duplicates the stampede lock the `GetOrCreateAsync` single-flight owns.

```csharp signature
// --- [CONSTANTS] -----------------------------------------------------------------------
file static class LoweringEntry {
    public static readonly HybridCacheEntryOptions Compiled = new() {
        Expiration = CacheLane.ModelResult.Entry.Expiration,
        LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
        Flags = HybridCacheEntryFlags.DisableDistributedCache,
    };
}

// --- [MODELS] --------------------------------------------------------------------------
[ImmutableObject(true)]
public sealed record LoweringSlot(Fin<CompiledExpr> Result);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CompiledKey {
    public static Fin<UInt128> Of(SymbolicExpr source, Seq<SymbolName> symbolOrder) =>
        Captured.Of(() => {
                ArrayBufferWriter<byte> symbols = new();
                foreach (SymbolName symbol in symbolOrder) {
                    byte[] encoded = Encoding.UTF8.GetBytes(symbol.Value);
                    Span<byte> slot = symbols.GetSpan(4 + encoded.Length);
                    BinaryPrimitives.WriteInt32LittleEndian(slot, encoded.Length);
                    encoded.CopyTo(slot[4..]);
                    symbols.Advance(4 + encoded.Length);
                }

                Span<byte> frame = stackalloc byte[24];
                BinaryPrimitives.WriteUInt64LittleEndian(frame, ContentHash.Half(source.ContentKey, 0));
                BinaryPrimitives.WriteUInt64LittleEndian(frame[8..], ContentHash.Half(source.ContentKey, 1));
                BinaryPrimitives.WriteUInt64LittleEndian(frame[16..], XxHash3.HashToUInt64(symbols.WrittenSpan));
                return Fin.Succ(XxHash128.HashToUInt128(frame));
            });
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class LoweringCache(HybridCache cache) {
    const string SymbolicOwner = "symbolic-lowering";

    public IO<CompiledExpr> Through(SymbolicExpr source, DimensionVerdict proof, Seq<SymbolName> symbolOrder) =>
        IO.lift(() => CompiledKey.Of(source, symbolOrder))
            .Bind(key => IO.liftAsync(async (EnvIO env) => (await cache.GetOrCreateAsync(
                CacheLane.ModelResult.Scoped($"symbolic:{key:x32}"),
                (Source: source, Proof: proof, Order: symbolOrder),
                static (state, _) => new ValueTask<LoweringSlot>(new LoweringSlot(
                    CompileCapsule.Compile(state.Source, state.Proof, state.Order))),
                LoweringEntry.Compiled,
                [CacheLane.ModelResult.Key, CacheLane.ModelResult.Tag(SymbolicOwner)],
                env.Token)).Result))
            .Bind(IO.lift);

    public IO<Unit> Evict(SymbolicExpr source, Seq<SymbolName> symbolOrder) =>
        IO.lift(() => CompiledKey.Of(source, symbolOrder))
            .Bind(key => IO.liftAsync(async (EnvIO env) => {
                await cache.Remove(CacheLane.ModelResult, $"symbolic:{key:x32}", env.Token);
                return unit;
            }));

    public IO<Unit> Purge() =>
        IO.liftAsync(async (EnvIO env) => {
            await cache.Invalidate(CacheLane.ModelResult, Seq(SymbolicOwner), env.Token);
            return unit;
        });
}
```

## [04]-[SYMBOLIC_JACOBIAN]

- Owner: `SymbolicJacobian` differentiates a scalar formula by each free design symbol through `Symbolic/expression#OPERATION_FOLD`, proves each partial through `Symbolic/dimensional#UNITS_BRIDGE`, compiles it through `LoweringCache`, and packs the full design-symbol vector, active-index map, partials, and design point into one `SymbolicTape`; `Backward` owns the scalar reverse-mode transpose with the cotangent as the only external seed.
- Cases: each free design symbol lowers to one compiled partial paired positionally with `ActiveIndices`; inactive design symbols and a constant formula scatter as exact zeroes in the full-width gradient; a non-differentiable residue faults before any tape records.
- Law: a partial carries its OWN dimension — dim(∂f/∂x) is dim(f)/dim(x) — so what crosses into `Lower` is the `DimensionContext`, never the formula's verdict. Each partial admits through `DimensionAdmission` and reaches the cache behind its own proof; handing the formula's verdict down would stamp every partial with the formula's dimension, which is wrong on every partial of a dimensioned formula.
- Entry: `Lower(formula, context, designSymbols, designPoint, cache)` returns `IO<SymbolicTape>` — it admits the design point's arity and finiteness together, derives `ActiveIndices`, differentiates each active symbol through one short-circuiting `Traverse`, forks the per-partial prove-and-compile reads and harvests them through `awaitAll`, and mints the tape through its one `Of` factory; `Backward(tape, cotangent)` evaluates at the active projection, scatters into full width, and applies `x̄ = Jᵀ·ȳ` in the optimizer's `ReadOnlyMemory<float>` seed shape.
- Auto: `Lower` reuses the `SymbolicOp.Differentiate` arm and cache; `Backward` evaluates each partial through `CompiledExpr.Invoke`, scatters by `ActiveIndices`, then scales through `TensorPrimitives.Multiply`. The tape's correspondence invariants prove at its ONE mint, so the sweep re-proves only the slot a caller can still re-point; a forwarding `Chain` shell over `Backward` with an identical signature and no added law is the deleted hop, and independent scalar formulas never form a `Seq<SymbolicTape>` composition.
- Receipt: none of its own — the gradient feeds the optimizer `DescendAdjoint` which stamps the `Optimization` slot; a lowering fault rides the `ComputeFault` rail at the `NonDifferentiable` arm.
- Packages: AngouriMath (`Differentiate` through the `OPERATION_FOLD` `Apply`, `Compile<>` through `LOWERING`), System.Numerics.Tensors (`TensorPrimitives.Multiply(ReadOnlySpan<float>, float, Span<float>)` for the scalar-broadcast `∇f · ȳ₀` contraction, the same SIMD surface `Tensor/dispatch#EQUIVALENCE_INTEROP` uses), Thinktecture.Runtime.Extensions, Generator.Equals (`[Equatable]`/`[OrderedEquality]` over the tape's four sequence members), LanguageExt.Core (`IO`, `Fork`/`awaitAll`, `Fin`, `Validation`, `Seq`, `Traverse`), Rasm (project — the `SensitivityLaw`/`AdjointMode` reverse-mode contract the symbolic tape conforms to).
- Growth: a new gradient source is one more additive `DesignVariable` arm at `Solver/optimizer#OPTIMIZER_LANE`; a higher-order symbolic sensitivity is one `SymbolicJacobian` operation differentiating the partials a second time through `Differentiate(symbol, 2)`, riding the same tape and transpose surface.
- Boundary: the symbolic-Jacobian arm is the additive `DesignVariable.Symbolic` case the optimizer admits — a standalone `GradientSource`, a parallel `(Seq<double>, double)` path (the `Surrogate.Predict` RETURN shape, never the gradient contract), or a `Seq<SymbolicTape>` composition is rejected. `AdjointTape` is a closed `[Union]` whose `Geometry` case carries the composable `Seq<GeometryTape>` and whose `Symbolic` case carries one scalar `SymbolicTape`; each arm retains its honest arity under one optimizer dispatch. `SymbolicJacobian.Backward(SymbolicTape, ReadOnlyMemory<float>)` IS the two-argument transpose the `SensitivityLaw.Chain` contract names — a `SymbolicAdjoint.Chain` re-spelling it verbatim resolved the same name in two hops and is deleted — and it stays two-argument because the design point lives on the tape. Re-pointing that tape at the current design state is `tape with { DesignPoint = origin }` — the SANCTIONED per-iteration move `Solver/optimizer#OPTIMIZER_LANE` makes before each reverse sweep, because the partials are position-independent and only the evaluation point moves; re-lowering the Jacobian per iteration re-compiles what the cache already holds, and reusing a stale point silently returns the first iterate's gradient forever.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record SymbolicTape(
    [property: OrderedEquality] Seq<SymbolName> DesignSymbols,
    [property: OrderedEquality] Seq<int> ActiveIndices,
    [property: OrderedEquality] Seq<CompiledExpr> Partials,
    [property: OrderedEquality] ImmutableArray<double> DesignPoint) {
    internal static Fin<SymbolicTape> Of(Seq<SymbolName> designSymbols, Seq<int> activeIndices, Seq<CompiledExpr> partials, ImmutableArray<double> designPoint) =>
        designSymbols.Distinct().Count != designSymbols.Count
            ? Fin.Fail<SymbolicTape>(new ComputeFault.ParseRejected("<design-symbols-duplicate>"))
        : activeIndices.Count != partials.Count
            ? Fin.Fail<SymbolicTape>(new ComputeFault.SymbolUndefined($"<tape-partials:{partials.Count}≠{activeIndices.Count}>"))
        : activeIndices.Exists(index => index < 0 || index >= designSymbols.Count)
            ? Fin.Fail<SymbolicTape>(new ComputeFault.SymbolUndefined("<tape-index-out-of-design>"))
        : Fin.Succ(new SymbolicTape(designSymbols, activeIndices, partials, designPoint));

    public bool IsDegenerate => ActiveIndices.IsEmpty;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SymbolicJacobian {
    public static IO<SymbolicTape> Lower(SymbolicExpr formula, DimensionContext context, Seq<SymbolName> designSymbols, ImmutableArray<double> designPoint, LoweringCache cache) =>
        IO.lift(() => Admit(designSymbols, designPoint)).Bind(_ =>
            toSeq(Enumerable.Range(0, designSymbols.Count))
                .Filter(index => formula.FreeSymbols.Contains(designSymbols[index])) is var active
            && active.Map(index => designSymbols[index]) is var free
            && free.IsEmpty
                ? IO.lift(() => SymbolicTape.Of(designSymbols, active, Seq<CompiledExpr>(), designPoint))
                : IO.lift(() => free.Traverse(symbol => SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol, Order.Create(1)))).As())
                    .Bind(partials => partials.Traverse(partial => Compiled(partial, context, free, cache).Fork()).As())
                    .Bind(awaitAll)
                    .Bind(rows => IO.lift(() => SymbolicTape.Of(designSymbols, active, rows, designPoint))));

    static IO<CompiledExpr> Compiled(SymbolicExpr partial, DimensionContext context, Seq<SymbolName> free, LoweringCache cache) =>
        IO.lift(() => DimensionAdmission.Admit(partial, context).ToFin())
            .Bind(verdict => cache.Through(partial, verdict, free));

    static Fin<Unit> Admit(Seq<SymbolName> designSymbols, ImmutableArray<double> designPoint) =>
        (designSymbols.Count == designPoint.Length
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new ComputeFault.SymbolUndefined($"<design-arity:{designPoint.Length}≠{designSymbols.Count}>")),
         designPoint.All(double.IsFinite)
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new ComputeFault.SymbolUndefined("<design-point-non-finite>")))
            .Apply(static (_, _) => unit)
            .As()
            .ToFin();

    public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ReadOnlyMemory<float> cotangent) =>
        tape.DesignPoint.Length != tape.DesignSymbols.Count || !tape.DesignPoint.All(double.IsFinite)
            ? Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.SymbolUndefined("<tape-design-point-invalid>"))
        : cotangent.Length != 1
            ? Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.SymbolUndefined($"<cotangent-arity:{cotangent.Length}≠1:scalar-tape>"))
        : tape.IsDegenerate
            ? Fin.Succ<ReadOnlyMemory<float>>(new float[tape.DesignSymbols.Count])
            : BackwardActive(tape, cotangent.Span[0]);

    static Fin<ReadOnlyMemory<float>> BackwardActive(SymbolicTape tape, float seed) {
        ImmutableArray<double> activePoint = [.. tape.ActiveIndices.Map(index => tape.DesignPoint[index])];
        return tape.Partials.Traverse(partial => partial.Invoke(activePoint))
            .Map(gradient => Contract(gradient, tape.ActiveIndices, tape.DesignSymbols.Count, seed))
            .As();
    }

    static ReadOnlyMemory<float> Contract(Seq<double> gradient, Seq<int> activeIndices, int width, float seed) {
        float[] result = new float[width];
        activeIndices.Zip(gradient).Iter(pair => result[pair.First] = (float)pair.Second);
        TensorPrimitives.Multiply(result, seed, result);
        return result;
    }
}
```

## [05]-[ENCLOSURE]

- Owner: `Enclosure` the inf-sup carrier whose algebraic operations round outward through `Math.BitDecrement`/`Math.BitIncrement` and whose transcendental arms carry an accumulated soundness `Pad`; `EnclosureFold` the `Entity` tree fold evaluating a formula over a box domain in interval arithmetic; `EnclosureVerdict` `[Union]` the three-way constraint pre-gate a `g(x) <= 0` question answers over an entire box in one evaluation. The fold keys on the same canonical-NF `SymbolicExpr.ContentKey` the scalar lowering keys on.
- Cases: `EnclosureVerdict` cases `ProvenSatisfied` (the padded upper bound <= 0 — every point of the box satisfies), `ProvenViolated` (the padded lower bound > 0 — no point can), `Indeterminate(Enclosure)` (the sound enclosure straddles zero — the box splits or the exact engine answers).
- Law: the carrier is `Enclosure`, never `Interval`. The kernel `Rasm/Numerics/predicates` `Interval` is a public type this package references directly, so two public `Interval`s over one compile edge is a collision no reader resolves without namespace archaeology; the surviving name states what this carrier IS — an outward-rounded enclosure carrying the margin it could not prove — where the kernel's states an exact algebraic interval, and the two are not one concept wearing two spellings.
- Law: soundness is TWO claims, not one. `+`, `-`, `*`, `/`, and negation are correctly rounded under IEEE double, so the outward step after each is a PROOF and those bounds are a guaranteed enclosure at one ULP. `Math.Log` and `Math.Pow` are NOT correctly rounded — the BCL contracts them to a few ULP — so every arm reaching them is a WIDENED ESTIMATE: it pads by a directed multi-ULP step with a relative floor, and that pad accumulates through every downstream operation. `Certify` therefore reads the SOUND range, so a verdict whose path crossed a transcendental arm demotes to `Indeterminate` unless its margin exceeds the accumulated pad — a proof that a widened estimate cannot support is exactly the false certificate this split exists to refuse.
- Law: the ULP count and the relative floor are this carrier's OWN, not epsilon rows. `TranscendentalUlps` is a contracted bound on the BCL transcendental kernels rather than a machine-precision anchor, and `TranscendentalRelative` sits a decade below the kernel `EpsilonPolicy.SeamUlp` residual floor; the one epsilon this page shares — the imaginary-residual floor at the interpreter's real-result gate — reads `SeamUlp` directly and spells no literal.
- Entry: `EnclosureFold.Enclose(SymbolicExpr, Seq<SymbolName> symbolOrder, ImmutableArray<Enclosure> box)` folds the tree over the catalog-verified node records — `Sumf`/`Minusf`/`Mulf`/`Divf`/`Powf`/`Absf`/`Signumf`/`Logf`, `Variable`, the numeric leaves — and declines any other node typed, so the enclosure never silently widens to `(-inf, +inf)` on a node it cannot bound; `Certify(...)` projects the sound enclosure onto `EnclosureVerdict`.
- Receipt: none of its own — a branch-and-prune consumer counts discarded boxes on its own receipt; an enclosure decline rides the `ComputeFault.NonDifferentiable` 2214 arm exactly as a compile decline does, and an ill-shaped box rides `ParseRejected`.
- Packages: AngouriMath (the positional node records, whose `Minusf` names its FIRST child `Subtrahend`), System.Numerics.Tensors (`TensorPrimitives.Min`/`Max` over the product corner span), LanguageExt.Core (`Fin`, `Seq`, `Map`, the LINQ `from`/`select` bind over `Fin`), Rasm (project — kernel `EpsilonPolicy`), `Symbolic/expression` (in-branch — `SymbolName`, `NumberBox`, `SymbolicExpr.Tree`), BCL inbox (`Math.BitDecrement`/`BitIncrement`).
- Growth: a new bounded node family (the trig records, once their monotonicity split lands) is one arm on the fold; a tighter enclosure (affine arithmetic, mean-value forms) is a policy row on `EnclosureFold`, never a sibling evaluator.
- Boundary: the pre-gate serves `Solver/satisfy#RULE_SATISFACTION` — a rule whose enclosure proves over the declared bounds never spends the Z3 timeout, and `Indeterminate` falls through to the exact check, so the gate is a filter, never a verdict authority; `Solver/optimizer` box screening discards `ProvenViolated` regions without oracle calls. Interval division by a zero-straddling denominator, a NEGATIVE integer power over a zero-straddling base (the same reciprocal, so the same refusal), and `Logf` over a non-positive interval each decline typed rather than returning an infinite enclosure that certifies nothing; every arm that can leave the finite range re-checks `Valid` before it returns, so a non-finite bound faults at the node that produced it — and the node NAMES itself, because a string tag passed beside a node whose type already carries its identity is the knob that gate does not need. A columnar SIMD sweep over N design points is NOT owned here: the register-program form this page once carried had no consumer estate-wide, and a DOE sweep loops the scalar `CompiledExpr.Invoke` until a sweep owner names the batched shape it wants.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct Enclosure(double Lo, double Hi, double Pad) {
    const int TranscendentalUlps = 4;
    const double TranscendentalRelative = 1e-15;

    public static Enclosure Of(double lo, double hi) => new(Math.BitDecrement(lo), Math.BitIncrement(hi), 0.0);

    public static Enclosure Widened(double lo, double hi, double carried) =>
        new(Math.BitDecrement(lo), Math.BitIncrement(hi), carried + Margin(lo, hi));

    public static Enclosure Point(double value) => new(value, value, 0.0);

    public bool Valid => double.IsFinite(Lo) && double.IsFinite(Hi) && double.IsFinite(Pad) && Pad >= 0.0 && Lo <= Hi;
    public bool Contains(double value) => value >= Lo && value <= Hi;

    public Enclosure Sound => new(Lo - Pad, Hi + Pad, 0.0);

    public static Enclosure operator +(Enclosure a, Enclosure b) => Of(a.Lo + b.Lo, a.Hi + b.Hi) with { Pad = a.Pad + b.Pad };
    public static Enclosure operator -(Enclosure a, Enclosure b) => Of(a.Lo - b.Hi, a.Hi - b.Lo) with { Pad = a.Pad + b.Pad };
    public static Enclosure operator -(Enclosure a) => new(-a.Hi, -a.Lo, a.Pad);

    public static Enclosure operator *(Enclosure a, Enclosure b) {
        ReadOnlySpan<double> products = [a.Lo * b.Lo, a.Lo * b.Hi, a.Hi * b.Lo, a.Hi * b.Hi];
        return Of(TensorPrimitives.Min(products), TensorPrimitives.Max(products)) with { Pad = a.Pad + b.Pad };
    }

    public Enclosure Abs() => Lo >= 0.0 ? this : Hi <= 0.0 ? -this : Of(0.0, Math.Max(-Lo, Hi)) with { Pad = Pad };

    static double Margin(double lo, double hi) {
        double magnitude = Math.Max(Math.Abs(lo), Math.Abs(hi));
        return (TranscendentalUlps * (Math.BitIncrement(magnitude) - magnitude)) + (TranscendentalRelative * magnitude);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnclosureVerdict {
    private EnclosureVerdict() { }
    public sealed record ProvenSatisfied(Enclosure Range) : EnclosureVerdict;
    public sealed record ProvenViolated(Enclosure Range) : EnclosureVerdict;
    public sealed record Indeterminate(Enclosure Range) : EnclosureVerdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EnclosureFold {
    public static Fin<Enclosure> Enclose(SymbolicExpr source, Seq<SymbolName> symbolOrder, ImmutableArray<Enclosure> box) =>
        source.Tree.Bind(tree => symbolOrder.Count != box.Length || !box.All(static i => i.Valid)
            ? Fin.Fail<Enclosure>(new ComputeFault.ParseRejected($"<enclosure-box:{box.Length}≠{symbolOrder.Count}>"))
            : Descend(tree, symbolOrder.Zip(toSeq(box)).ToMap()));

    public static Fin<EnclosureVerdict> Certify(SymbolicExpr constraint, Seq<SymbolName> symbolOrder, ImmutableArray<Enclosure> box) =>
        Enclose(constraint, symbolOrder, box).Map(range => (EnclosureVerdict)(
            range.Sound.Hi <= 0.0 ? new EnclosureVerdict.ProvenSatisfied(range)
            : range.Sound.Lo > 0.0 ? new EnclosureVerdict.ProvenViolated(range)
            : new EnclosureVerdict.Indeterminate(range)));

    static Fin<Enclosure> Descend(Entity node, Map<SymbolName, Enclosure> bindings) => node switch {
        Entity.Variable v => bindings.Find(SymbolName.Create(v.Name)).ToFin(new ComputeFault.SymbolUndefined($"<enclosure-unbound:{v.Name}>")),
        Entity.Number n => NumberBox.Project(n).Map(Enclosure.Point),
        Entity.Sumf s => from a in Descend(s.Augend, bindings) from b in Descend(s.Addend, bindings) select a + b,
        Entity.Minusf m => from a in Descend(m.Subtrahend, bindings) from b in Descend(m.Minuend, bindings) select a - b,
        Entity.Mulf m => from a in Descend(m.Multiplier, bindings) from b in Descend(m.Multiplicand, bindings) select a * b,
        Entity.Divf d =>
            from a in Descend(d.Dividend, bindings)
            from b in Descend(d.Divisor, bindings)
            from q in b.Contains(0.0)
                ? Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable("<enclosure-zero-straddling-divisor>"))
                : Finite(a * (Enclosure.Of(1.0 / b.Hi, 1.0 / b.Lo) with { Pad = b.Pad }), d)
            select q,
        Entity.Powf p =>
            from a in Descend(p.Base, bindings)
            from e in Descend(p.Exponent, bindings)
            from r in Power(a, e, p)
            select r,
        Entity.Absf a => Descend(a.Argument, bindings).Map(static i => i.Abs()),
        Entity.Signumf s => Descend(s.Argument, bindings).Map(static i =>
            i.Lo > 0.0 ? Enclosure.Point(1.0) : i.Hi < 0.0 ? Enclosure.Point(-1.0) : Enclosure.Of(-1.0, 1.0)),
        Entity.Logf l =>
            from b in Descend(l.Base, bindings)
            from x in Descend(l.Antilogarithm, bindings)
            from r in x.Lo > 0.0 && b.Lo > 0.0 && (b.Hi < 1.0 || b.Lo > 1.0)
                ? Finite(Corners(x, b, Math.Log), l)
                : Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable("<enclosure-log-domain>"))
            select r,
        _ => Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable($"<enclosure-node:{node.GetType().Name}>")),
    };

    static Fin<Enclosure> Finite(Enclosure value, Entity node) =>
        value.Valid
            ? Fin.Succ(value)
            : Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable($"<enclosure-nonfinite:{node.GetType().Name}>"));

    static Enclosure Corners(Enclosure x, Enclosure y, Func<double, double, double> f) {
        double a = f(x.Lo, y.Lo), b = f(x.Lo, y.Hi), c = f(x.Hi, y.Lo), d = f(x.Hi, y.Hi);
        return Enclosure.Widened(Math.Min(Math.Min(a, b), Math.Min(c, d)), Math.Max(Math.Max(a, b), Math.Max(c, d)), x.Pad + y.Pad);
    }

    static Fin<Enclosure> Power(Enclosure baseRange, Enclosure exponent, Entity node) =>
        exponent.Lo == exponent.Hi && double.IsInteger(exponent.Lo)
            ? IntegerPower(baseRange, (int)exponent.Lo, node)
            : baseRange.Lo > 0.0
                ? Finite(Corners(baseRange, exponent, Math.Pow), node)
                : Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable("<enclosure-pow-domain>"));

    static Fin<Enclosure> IntegerPower(Enclosure a, int n, Entity node) =>
        n == 0 ? Fin.Succ(Enclosure.Point(1.0))
        : n < 0 ? IntegerPower(a, -n, node).Bind(p => p.Contains(0.0)
            ? Fin.Fail<Enclosure>(new ComputeFault.NonDifferentiable("<enclosure-zero-straddling-power>"))
            : Finite(Enclosure.Of(1.0 / p.Hi, 1.0 / p.Lo) with { Pad = p.Pad }, node))
        : (n & 1) == 0 && a.Contains(0.0)
            ? Finite(Enclosure.Widened(0.0, Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), a.Pad), node)
            : Finite(Enclosure.Widened(Math.Min(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), Math.Max(Math.Pow(a.Lo, n), Math.Pow(a.Hi, n)), a.Pad), node);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
