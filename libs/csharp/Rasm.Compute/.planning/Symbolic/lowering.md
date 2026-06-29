# [SYMBOLIC_LOWERING]

The compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` (the F# `Expression` wrapped by `Symbolic/expression#SYMBOLIC_EXPR`) lowers once to a native `Func<double…>`/`Delegate` evaluator through the verified `Compile.compileExpression{,1,2,3,4}` `FSharpOption<Func<…>>` arity surface (`.api/api-mathnet-symbolics.md#COMPILE_SURFACE`), is carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` already mints, and is reused through a `LoweringCache` read-through over the one model-lane `HybridCache` the inference lane owns (`Model/inference#RESULT_CACHE` `CacheLane.ModelResult`, never a second cache instance). The page owns the `CompiledExpr` delegate carrier, the `CompileArity` `[SmartEnum<string>]` that both selects AND owns the arity-exact compile-and-invoke behavior over the symbol-order count (one delegate-backed row per arity, never two parallel arity switches), the `LoweringCache` L1-only read-through with its `LoweringSlot` carrier and its `CompiledKey` content-key derivation composing the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` law, and — the cross-lane contribution — the `SymbolicJacobian` lowering that differentiates a formula by each free design symbol through `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate`, compiles each partial to a native delegate, and packs the partials WITH the design point into a `SymbolicTape` whose `SymbolicAdjoint.Chain` answers the SAME verified two-argument reverse-mode contract `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` answers — the design point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`, so the optimizer `Solver/optimizer#OPTIMIZER_LANE` `DescendAdjoint` folds a mixed geometry-and-symbolic tape under one `Chain`. The symbolic source is the additive `DesignVariable.Symbolic` arm the optimizer's `DesignVariable` `[Union]` admits — its lowering yielding a `SymbolicTape`, its adjoint routing `SymbolicAdjoint.Chain`, a sibling adjoint-registry entry — never a standalone `GradientSource` type and never a parallel `(Seq<double>, double)` gradient path (that pair is the `Surrogate.Predict` RETURN shape, never the gradient contract).

The lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before: a formula compiles and registers a Jacobian only after its `DimensionProof` admits, so a dimension-inconsistent expression never reaches a `CompiledExpr` slot, the optimizer oracle, or the integrator seed. A formula carrying no free design symbol lowers an empty `SymbolicTape` and `DescendAdjoint` falls to finite-difference exactly as the absent-`DesignMesh` case lowers an empty geometry tape and descends degenerate by construction — the symbolic source admits without breaking that invariant and without a parallel descent. The page is host-local and carries no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior runtime value that never crosses a wire, and the only cross-surface fact is the `SymbolicExpr.ContentKey` `Symbolic/expression#SYMBOLIC_EXPR` mints (the durable canonical-NF formula identity `CompiledKey` composes as its hash source) that the `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` branch seam crosses by reference to the `Rasm.Persistence/Query/cache#MODEL_RESULT_INDEX` cost-catalog/QTO-formula consumers — keyed by its OWN content identity, never a fabricated `ModelResultKey` — aligned at the key and never coupled into a sibling interior; the `CompiledKey` itself is the L1-local cache-partition derivation over that content key and never crosses a process boundary. The compiled `Delegate` is netstandard2.0 ALC-safe (the F# `Expression` core and the `LambdaExpression.Compile()` it lowers carry no runtime-codegen hazard the live plugin host forbids) yet not durably serializable, so the cache entry is L1-only by construction — cross-process reuse is deterministic re-lowering off the content-addressed key, never a serialized delegate or a re-derivation seed crossing a process boundary. An in-proc symbolic-regression fit is the rejected form — offline formula discovery is the Python branch's, and this page owns compile-and-cache plus the analytic-Jacobian lowering over an already-known, already-admitted expression.

## [01]-[INDEX]

- [01]-[LOWERING]: `CompiledExpr` delegate carrier; `CompileArity` delegate-backed arity owner (compile + invoke in one row); the `Compile.compileExpression{,1,2,3,4}` lowering.
- [02]-[LOWERING_CACHE]: `LoweringCache` L1-only read-through over `CacheLane.ModelResult`; `LoweringSlot` `[ImmutableObject]` carrier; `CompiledKey` content-key derivation.
- [03]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering; `SymbolicTape` (design point baked in) / `SymbolicAdjoint` two-argument transpose `Chain`.

## [02]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a lowered native delegate to its source `SymbolicExpr` content key and its ordered free-symbol vector; `CompileArity` the `[SmartEnum<string>]` whose six rows (`nullary` · `unary` · `binary` · `ternary` · `quaternary` · `variadic`) EACH own both their compile form and their invoke form as delegate-backed row behavior — a row's `Lower` selects the arity-exact `Compile.compileExpression{1,2,3,4}` module form (or the variadic `compileExpression` over a `Symbol list`) and boxes its `FSharpOption<Func<…>>` into one `FSharpOption<Delegate>` carrier, and a row's `Invoke` performs the strongly-typed down-cast call (`((Func<double,double>)d)(a[0])` … `((Func<double,double,double,double,double>)d)(a[0..3])`), only the variadic row crossing a single `DynamicInvoke`; `CompileCapsule` the one boundary owner projecting the boxed compile carrier into the `Fin` rail (composing `Symbolic/expression#SYMBOLIC_EXPR` `ExpressionCapsule.ProjectCompile`), so a compile-decline (an unsupported `FunctionN` node) is a `ComputeFault.NonDifferentiable` fault and never an unwrapped null delegate.
- Cases: `CompileArity` rows `nullary` (rank 0 — no free symbol, the formula is a constant evaluated once, no delegate) · `unary` (rank 1, `compileExpression1`, `Func<double,double>`) · `binary` (rank 2, `compileExpression2`, `Func<double,double,double>`) · `ternary` (rank 3, `compileExpression3`, `Func<double,double,double,double>`) · `quaternary` (rank 4, `compileExpression4`, `Func<double,double,double,double,double>`) · `variadic` (rank −1, `compileExpression` over a `Symbol list`, the boxed `Delegate` invoked with a positional `double[]`); the row is selected by `CompileArity.Select(symbolOrder.Count)`, never a call-site branch.
- Entry: `public static Fin<CompiledExpr> Compile(SymbolicExpr source, Seq<string> symbolOrder)` is the one polymorphic lowering — the symbol order fixes the positional argument convention (the i-th `double` binds the i-th symbol), `CompileArity.Select(symbolOrder.Count)` picks the row, `arity.Lower(source.Expression, symbols)` returns the boxed `FSharpOption<Delegate>`, and `ExpressionCapsule.ProjectCompile` lifts it to a `CompiledExpr` or a `NonDifferentiable` fault; `public Fin<double> Invoke(ImmutableArray<double> arguments)` is the typed call site — it validates the argument count against the symbol order (a mis-arity call is a `ComputeFault`, never a `TargetParameterCountException`) then delegates to `Arity.Invoke(Evaluator, arguments)`, so the strongly-typed `Func` down-cast and the variadic `DynamicInvoke` are both owned by the row, never re-switched at the call site.
- Auto: `Compile` reads the ordered `symbolOrder` rather than `source.FreeSymbols` directly so the positional convention is caller-fixed and stable across a re-compile (two callers compiling the same formula with the same symbol order share one delegate and one cache slot); the arity-exact rows (`unary`…`quaternary`) pre-cast the boxed `LambdaExpression.Compile()` delegate to the exact `Func<…>` so those call sites invoke a strongly-typed delegate with no reflection, and only the variadic row (arity ≥ 5) crosses a single `DynamicInvoke` collapsed to one positional `double[]` bind — the four arity-exact `compileExpression{1,2,3,4}` overloads cover one-through-four symbols, so the reflective path is reached only past four; the nullary row never compiles a delegate — a free-symbol-empty formula evaluates once through `Symbolic/expression#SYMBOLIC_EXPR` `Evaluate` and the `CompiledExpr` carries the cached constant, so a constant formula is a zero-argument value, never a degenerate delegate.
- Receipt: `CompiledExpr` carries no receipt of its own — the compile outcome rides the `LOWERING_CACHE` hit/miss/store slot the model-lane `Model/inference#RESULT_CACHE` `ComputeReceipt.Cache` fact already stamps, and a compile-decline rides the `ComputeFault.NonDifferentiable` 2215 arm `Symbolic/expression#BUILD_ADMISSION` declares; the downstream optimize outcome carries the `Optimization` receipt slot.
- Packages: MathNet.Symbolics (`Compile.compileExpression{,1,2,3,4}` module forms and the variadic `compileExpression(Expression, FSharpList<Symbol>)`, `Symbol.NewSymbol(string)` raw-`Symbol` construction — the sole public `Symbol` factory, no `Symbol.symbol`/`Symbol.Symbol`, the `FSharpOption<Delegate>` carrier), MathNet.Numerics.FSharp (the F# facade the compile surface ships on), Thinktecture.Runtime.Extensions (`CompileArity` `[SmartEnum]`), LanguageExt.Core (`Fin` rail, `Seq`/`ImmutableArray`/`Option` carriers), FSharp.Core (transitive `FSharpOption`/`ListModule`/`OptionModule`).
- Growth: a new arity past four is impossible — the variadic row absorbs every arity of five or more through the boxed `Delegate` positional form, and the four arity-exact `compileExpression{1,2,3,4}` overloads are the complete strongly-typed set the package ships (there is no `compileExpression5`); a new compile target (the `Compile.compileComplexExpression{,1,2,3,4}` complex-domain mirror) is one `CompileArity` companion-row family binding the complex delegate, never a parallel `CompiledComplexExpr` carrier; a new evaluation convention (e.g. a `Span<double>` argument bind for a hot loop) is one more `Invoke` shape on the same row, never a second carrier.
- Boundary: `Compile` is the single lowering entry — a `CompileUnary`/`CompileBinary`/`CompileVariadic` factory trio modeling one concept is the collapsed defect, and the symbol-order count selects the row, never the call site; the arity dispatch is owned ONCE by the `CompileArity` rows (each row carries both its `Lower` compile-form selector and its `Invoke` down-cast), so the compile path and the invoke path never re-switch the arity axis in two parallel `switch` bodies — the two-switch shape is the collapsed defect; the module-level `Compile.compileExpression{1,2,3,4}` `FSharpOption` form is the admitted surface because the explicit `None`-projection distinguishes a compile-decline from a parse-decline (the fluent `SymbolicExpression.Compile(params string[])` wrapper unwraps the option and throws on decline, so it is the rejected source for the cache path where a decline must land on the `Fin` rail — `.api/api-mathnet-symbolics.md#COMPILE_SURFACE`); the `FSharpOption<Delegate>` never crosses into the interior — `CompileCapsule` projects it at the seam exactly once and a bare `.Value` on a `None` is the eliminated null-deref; the positional symbol order is the one argument convention and an unordered `Map<string,double>`-keyed invoke is the rejected form because the compiled delegate is positional by construction (the F# `Symbol list` the compile call takes fixes the order); the compiled delegate is netstandard2.0 ALC-safe and a `System.Reflection.Emit`-based re-implementation of the lowering is the deleted form because `Compile.compileExpression` already owns the `LambdaExpression.Compile()` lowering.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompileArity {
    public static readonly CompileArity Nullary = new("nullary", 0,
        lower: static (_, _) => FSharpOption<Delegate>.None,
        invoke: static (_, _) => double.NaN);
    public static readonly CompileArity Unary = new("unary", 1,
        lower: static (e, s) => Box(Compile.compileExpression1(e, s[0])),
        invoke: static (d, a) => ((Func<double, double>)d)(a[0]));
    public static readonly CompileArity Binary = new("binary", 2,
        lower: static (e, s) => Box(Compile.compileExpression2(e, s[0], s[1])),
        invoke: static (d, a) => ((Func<double, double, double>)d)(a[0], a[1]));
    public static readonly CompileArity Ternary = new("ternary", 3,
        lower: static (e, s) => Box(Compile.compileExpression3(e, s[0], s[1], s[2])),
        invoke: static (d, a) => ((Func<double, double, double, double>)d)(a[0], a[1], a[2]));
    public static readonly CompileArity Quaternary = new("quaternary", 4,
        lower: static (e, s) => Box(Compile.compileExpression4(e, s[0], s[1], s[2], s[3])),
        invoke: static (d, a) => ((Func<double, double, double, double, double>)d)(a[0], a[1], a[2], a[3]));
    public static readonly CompileArity Variadic = new("variadic", -1,
        lower: static (e, s) => Compile.compileExpression(e, ListModule.OfSeq(s)),
        invoke: static (d, a) => (double)d.DynamicInvoke([.. a.Select(static x => (object)x)])!);

    private readonly int rank;
    private readonly Func<Expression, Symbol[], FSharpOption<Delegate>> lower;
    private readonly Func<Delegate, ImmutableArray<double>, double> invoke;

    public int Rank => rank;

    // The symbol-order count selects the row; arity-exact to four (the package's complete
    // strongly-typed overload set), variadic absorbing five-plus through the boxed Delegate.
    public static CompileArity Select(int symbolCount) =>
        symbolCount switch { 0 => Nullary, 1 => Unary, 2 => Binary, 3 => Ternary, 4 => Quaternary, _ => Variadic };

    public FSharpOption<Delegate> Lower(Expression expression, Symbol[] symbols) => lower(expression, symbols);

    public double Invoke(Delegate evaluator, ImmutableArray<double> arguments) => invoke(evaluator, arguments);

    static FSharpOption<Delegate> Box<TDelegate>(FSharpOption<TDelegate> option) where TDelegate : Delegate =>
        OptionModule.IsSome(option)
            ? FSharpOption<Delegate>.Some(option.Value)
            : FSharpOption<Delegate>.None;
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
                .Map(value => new CompiledExpr(source.ContentKey, symbolOrder, arity, NoOp, Some(value)));
        }
        var symbols = symbolOrder.Map(Symbol.NewSymbol).ToArray();
        return ExpressionCapsule.ProjectCompile(arity.Lower(source.Expression, symbols), source.Canonical)
            .Map(@delegate => new CompiledExpr(source.ContentKey, symbolOrder, arity, @delegate, None));
    }

    static readonly Func<double> NoOp = static () => double.NaN;
}
```

## [03]-[LOWERING_CACHE]

- Owner: `CompiledKey` the static derivation folding the source `SymbolicExpr.ContentKey` and the ordered symbol-vector digest into one `XxHash128` cache key, composing the suite hash law `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key` holds — the content-key bytes hashed under an order-derived `XxHash3` seed, never a second hashing scheme and never a string-path key; `LoweringSlot` the `[ImmutableObject(true)]` cache carrier wrapping the produced `Fin<CompiledExpr>` so the model-lane `HybridCache` stores the live delegate by reference in its L1 tier without a serialization round-trip; `LoweringCache` the L1-only read-through service over the one `HybridCache` substrate the model lane already runs — it scopes the content key onto the settled `CacheLane.ModelResult` lane `Model/inference#RESULT_CACHE` declares and dispatches `HybridCache.GetOrCreateAsync` with the `HybridCacheEntryFlags.DisableDistributedCache` bypass, never constructing or registering a `HybridCache` instance of its own and never minting a parallel `CacheLane`. The lowering keys by its own content identity rather than a `ModelResultKey` (which carries `ModelIdentity`/`ExecutionProvider`/`ModelPrecision` ONNX-run facts a compiled formula has none of), so it composes the cache substrate and matches the `CachePolicy.ReadThrough` posture (serve a hit, store both the compiled success and a deterministic decline) while keying by content.
- Cases: the cache carries one `LoweringSlot` per content key — a compiled success and a deterministic `NonDifferentiable` decline both ride the same `Fin<CompiledExpr>` slot under the lane TTL, so a re-attempt of a deterministically-declining formula serves the cached decline rather than re-running `Compile.compileExpression`; the entry is L1-only because a compiled `Delegate` is not durably serializable — the `DisableDistributedCache` flag bypasses the L2 tier entirely (both read and write), so the live delegate lives only in the L1 tier and a cross-process consumer re-lowers from the content-addressed key rather than reading a serialized form.
- Entry: `public ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default)` is the one read-through — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult` under a `symbolic:` key prefix, and dispatches `cache.GetOrCreateAsync(scopedKey, state, factory, entryOptions, tags, token)` where the stampede-aware `factory` runs `CompileCapsule.Compile` and wraps the `Fin` in a `LoweringSlot`, so identical-formula-identical-order calls across distinct callers coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering — and the `.Result` projection unwraps the cached `LoweringSlot` back onto the `Fin` rail.
- Auto: `CompiledKey.Of` stamps the `SymbolicExpr.ContentKey` (already the canonical-NF `XxHash128` over the `Rational.Reduce`-normalized `Infix.Format` projection `Symbolic/expression#SYMBOLIC_EXPR` mints, so two structurally distinct but algebraically equal formulas that reduce to the same normal form key identically) as the hash source and the ordered-symbol digest as the `XxHash3` seed (so a re-order of the positional convention keys distinctly because the compiled delegate's argument order differs) in one `XxHash128.HashToUInt128(content, seed)` call, never a path-derived or display-string key; the `GetOrCreateAsync` single-flight is the single population point so stampede control rides the cache surface with zero call-site ceremony; the entry copies the lane's `HybridCacheEntryOptions` TTL/policy fields (`Expiration`/`LocalCacheExpiration`) from `CacheLane.ModelResult.Entry` and adds only the L2-bypass `Flags`, so the symbolic entry shares the lane TTL and adds only the L2 bypass — `HybridCacheEntryOptions` is a sealed non-record class, so the copy is an `init` object initializer over the lane values (a `with` expression the type cannot support, and a literal that drops the lane TTL, are both the rejected forms); the `LoweringSlot` is `[ImmutableObject(true)]` so the L1 tier holds the slot instance by reference and never serializes the wrapped `Delegate`, and the `DisableDistributedCache` flag keeps the entry off the L2 tier entirely — the cross-process reuse horizon is therefore deterministic re-lowering off the shared content key, not a serialized delegate and not a re-derivation seed (a `Delegate` carries no durable serialization, and a seed of `(ContentKey, SymbolOrder, Arity)` without the source `Expression` tree cannot reconstruct the delegate, so an L2 round-trip would buy nothing the content-keyed factory does not already give).
- Receipt: the lowering rides the model-lane `ComputeReceipt.Cache` hit/miss/store slots `Model/inference#RESULT_CACHE` stamps, never a parallel lowering receipt; a compile-decline cached in a `LoweringSlot` rides the `NonDifferentiable` 2215 arm — no new receipt case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed)`/`XxHash3.HashToUInt64(ReadOnlySpan<byte>)` for `CompiledKey`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, and `HybridCacheEntryFlags.DisableDistributedCache`, reached over the settled `CacheLane.ModelResult` lane — never registered or instantiated here), System.ComponentModel (`[ImmutableObject(true)]`, the L1 by-reference marker), LanguageExt.Core (`Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` lane descriptor the model lane composes).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/inference#RESULT_CACHE`, never a lowering-local cache policy; a new key contributor (e.g. a target-runtime tag if a future ALC variant compiled distinct delegates) is one more stamp in `CompiledKey.Of`, never a parallel key; a new cache substrate is impossible — the `CacheLane.ModelResult` lane over the one `HybridCache` is the single substrate and a lowering-local `HybridCache` registration or a `ConcurrentDictionary<UInt128, CompiledExpr>` memoization beside it is the named defect.
- Boundary: `LoweringCache` rides the model-lane cache substrate and never owns a cache instance — `CacheLane.ModelResult` over the one `HybridCache` is the single cache owner `Model/inference#RESULT_CACHE` declares, and a hand-rolled `ConcurrentDictionary<UInt128, CompiledExpr>` memoization beside it is the deleted form; the cache key is `XxHash128` over the canonical content key seeded by the symbol order — the suite hash law `Runtime/codecs#CONTENT_ADDRESSING` holds, never a second hashing scheme and never a `source.Canonical` string key (the content key already digests the canonical form, so re-hashing the string is the redundant pass); the lowering keys by its own content identity rather than the ONNX `ModelResultKey` because a compiled formula carries no `ModelIdentity`/`ExecutionProvider`/`ModelPrecision` — forcing a fabricated `ModelResultKey` is the rejected form; the entry is L1-only — a compiled `Delegate` is not serializable, so `DisableDistributedCache` is the admitted flag and a `DisableDistributedCacheWrite`-only half-measure is the rejected form (it leaves the entry probing a permanently-empty L2 on every miss), and an "L2 carries a re-lowering seed" design is the deleted illusory form because a seed without the source `Expression` cannot reconstruct the delegate and the content-keyed factory already re-lowers deterministically; the `LoweringSlot` `[ImmutableObject(true)]` carrier is what makes the L1 tier hold the live delegate by reference — caching the bare `Fin<CompiledExpr>` is the rejected form because HybridCache would serialize the non-immutable value and fail on the `Delegate`; the `GetOrCreateAsync` factory is the single population point and a caller that compiles-then-caches in two steps is the rejected form because it duplicates the stampede lock the single-flight already owns.

```csharp contract
// --- [CONSTANTS] -------------------------------------------------------------------------
file static class LoweringEntry {
    // Carries the model lane's TTL/policy forward and adds only the L2 bypass: a compiled Delegate is
    // not serializable, so the symbolic entry is L1-only while ONNX entries keep their L2 reuse.
    // `HybridCacheEntryOptions` is a sealed non-record class with `init` setters, so the lane policy is
    // copied through an object initializer over the lane values — `with` is not available on the type.
    public static readonly HybridCacheEntryOptions Compiled = new() {
        Expiration = CacheLane.ModelResult.Entry.Expiration,
        LocalCacheExpiration = CacheLane.ModelResult.Entry.LocalCacheExpiration,
        Flags = HybridCacheEntryFlags.DisableDistributedCache,
    };
}

// --- [MODELS] ----------------------------------------------------------------------------
// [ImmutableObject(true)] is the HybridCache immutable-type marker: the L1 tier holds this slot
// instance by reference and never serializes the wrapped live Delegate. Success and a deterministic
// decline both ride the one Fin<CompiledExpr>.
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

- Owner: `SymbolicJacobian` the static lowering differentiating a formula by each of its free design symbols through `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate`, compiling each partial to a `CompiledExpr` through the `LoweringCache`, and packing the ordered partials WITH the design point into a `SymbolicTape`; `SymbolicTape` the symbolic analog of the DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` carrying the ordered design-symbol vector, the compiled partial-derivative delegates, AND the design point the partials evaluate at — the point baked into the tape exactly as the geometry primal `MeshAdjointSnapshot` is baked into `GeometryTape`, so the reverse sweep needs no external primal; `SymbolicAdjoint` the reverse-mode transpose owner whose `Chain` folds a `Seq<SymbolicTape>` under the SAME two-argument `Chain(tape, seed)` contract `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` answers — the cotangent the only external seed — so the optimizer `Solver/optimizer#OPTIMIZER_LANE` `DescendAdjoint` descends a mixed geometry-and-symbolic tape under one fold.
- Cases: a free design symbol present in `formula.FreeSymbols` lowers to one compiled partial `∂f/∂xᵢ` on the tape; a formula with no free design symbol (a constant, or a formula whose free symbols are all non-design parse-context symbols) lowers an empty `SymbolicTape` so `SymbolicAdjoint.Chain` returns the upstream cotangent unchanged and `DescendAdjoint` falls to finite-difference — the absent-`DesignMesh` degenerate-descent invariant `Solver/optimizer#OPTIMIZER_LANE` holds for the symbolic source exactly as it holds for the geometry source; a non-differentiable node (a `FunctionN` the `Calculus.Differentiate` declines, surfaced as the `compileExpression` `None`) is the `NonDifferentiable` 2215 fault on the lowering rail before any tape records.
- Entry: `public static ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, ImmutableArray<double> designPoint, LoweringCache cache, CancellationToken token = default)` is the one lowering — it filters `designSymbols` to those present in `formula.FreeSymbols` (a declared-but-absent design symbol drops out so the tape carries only the live partials), differentiates the formula by each through `SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol))` in one short-circuiting `Traverse` (a non-differentiable partial faults here, before any compile), compiles every surviving partial over the full design-symbol order through the `LoweringCache.Through` read-through CONCURRENTLY (independent partials lower in parallel, the cache single-flight de-duplicating identical ones), and packs the ordered partials with the design point into the tape; `public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ReadOnlyMemory<float> cotangent)` evaluates each compiled partial at the tape's baked design point and applies the Jacobian-transpose-times-cotangent contraction `x̄ = Jᵀ·ȳ`, returning the gradient in the optimizer's `ReadOnlyMemory<float>` seed shape.
- Auto: `Lower` reuses the `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate` arm rather than re-implementing the analytic derivative (the `Calculus.Differentiate` over the F# `Expression` is the single derivative owner) and reuses the `LoweringCache` so a partial `∂f/∂x` compiled once for one design symbol is reused across optimizer generations; for a scalar formula `f : ℝⁿ → ℝ` the Jacobian is the single gradient row `∇f`, so `Backward` evaluates each compiled partial at the tape's design point through `CompiledExpr.Invoke` to recover `∇f` (the analytic gradient, never a finite-difference stencil) and contracts the reverse vector-Jacobian-product `x̄ = Jᵀ·ȳ = ∇f · ȳ₀` by SCALING the gradient row by the scalar upstream cotangent through the verified scalar-broadcast `TensorPrimitives.Multiply(gradient, ȳ₀, gradient)` — an elementwise `Multiply(gradient, cotangent[..n], gradient)` is the rejected form because it Hadamard-multiplies the gradient by an n-vector cotangent rather than scaling it by the scalar output sensitivity; `SymbolicAdjoint.Chain` reverses the tape and threads the cotangent through each step's `Backward` against THAT step's baked design point exactly as the geometry `Chain` applies each step against its own `MeshAdjointSnapshot`, and an empty tape short-circuits `Chain` to the identity so the degenerate case is structural, never a guarded special case.
- Receipt: the symbolic-Jacobian source lands no receipt of its own — the gradient feeds the optimizer `DescendAdjoint` which stamps the existing `Optimization` receipt slot (`Solver/optimizer#OPTIMIZER_LANE`), and the surrogate-hit/evaluation counts ride that same slot; a lowering fault rides the `ComputeFault` rail at the `NonDifferentiable` arm.
- Packages: MathNet.Symbolics (`Calculus.Differentiate` through the `Symbolic/expression#OPERATION_FOLD` `Apply` fold, `Compile.compileExpression` through the `LOWERING` lowering), System.Numerics.Tensors (`TensorPrimitives.Multiply(ReadOnlySpan<float>, float, Span<float>)` for the scalar-broadcast `∇f · ȳ₀` contraction, the same SIMD surface `Tensor/dispatch#EQUIVALENCE_INTEROP` uses), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Seq`, `Traverse`), Rasm (project — the `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw`/`AdjointMode` reverse-mode contract the symbolic tape conforms to).
- Growth: a new gradient source is one more additive `DesignVariable` arm at `Solver/optimizer#OPTIMIZER_LANE` (the geometry fields, the symbolic field, and a future analytic-FEM field are sibling arms), never a parallel adjoint path; a higher-order symbolic sensitivity (a Hessian for a Newton step) is one `SymbolicJacobian` companion differentiating the partials a second time, riding the same tape/transpose surface, never a new descent; the symbolic tape and the geometry tape stay distinct carriers but answer one `Chain` contract, so a unifying `Seq<AdjointTape>` `[Union]` of `GeometryTape | SymbolicTape` is the only collapse the optimizer registry would admit, never a duplicated descent kernel.
- Boundary: the symbolic-Jacobian arm is the additive `DesignVariable.Symbolic` case the optimizer's `DesignVariable` `[Union]` admits (its lowering yielding a `SymbolicTape`, its adjoint routing `SymbolicAdjoint.Chain`) — a standalone `GradientSource` type or a parallel `(Seq<double>, double)` gradient path is the named defect (that `(Seq<double>, double)` signature is the `Surrogate.Predict` RETURN shape, never the gradient contract; the optimizer evaluate oracle is `Func<DesignPoint, Fin<Seq<double>>>` and the gradient surface is `DescendAdjoint` reading `SensitivityLaw.Chain`); the symbolic source is NOT a row on the `DesignProblem.OperatorRows` `FrozenDictionary<Type, TensorOpFamily>` map (those rows carry tensor-op families such as `Gradient`/`CotangentLaplacian` only), it is the sibling symbolic-tape registry entry the future `AdjointTape` `[Union]` unifies; `SymbolicAdjoint.Chain` is the VERIFIED two-argument `Chain(Seq<SymbolicTape>, seed)` overload with the cotangent the only seed — a three-argument `Chain(tape, designPoint, seed)` shape (the design point passed alongside the cotangent rather than baked into the tape) is the deleted form, exactly the phantom three-argument call the geometry `SensitivityLaw.Chain` removed, so the design point lives on the `SymbolicTape` and the symbolic and geometry tapes fold under one contract; the analytic derivative is the `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate` arm and a lowering-local finite-difference stencil is the deleted form because the package owns the exact analytic derivative; the reverse VJP is the scalar-broadcast `∇f · ȳ₀` (the scalar output cotangent scaling the gradient row), never an elementwise gradient-by-cotangent-vector product; the empty-tape degenerate descent is the invariant the symbolic source must preserve — a formula with no free design symbol lowers an empty Jacobian and falls to finite-difference exactly as a `DesignProblem` with no `DesignMesh` lowers an empty geometry tape, so the symbolic source never breaks the degenerate-descent law and never introduces a third descent branch; the dimensional gate runs strictly first — `Lower` is invoked only after `Symbolic/dimensional#DIMENSION_PROOF` admits, so a dimension-inconsistent formula never reaches a compiled Jacobian.

```csharp contract
// --- [MODELS] ----------------------------------------------------------------------------
// The design point is baked into the tape (mirroring the GeometryTape MeshAdjointSnapshot), so
// the reverse sweep needs no external primal and Chain stays two-argument.
public sealed record SymbolicTape(Seq<string> DesignSymbols, Seq<CompiledExpr> Partials, ImmutableArray<double> DesignPoint) {
    public static readonly SymbolicTape Empty = new(Seq<string>(), Seq<CompiledExpr>(), ImmutableArray<double>.Empty);

    public bool IsDegenerate => Partials.IsEmpty;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicJacobian {
    public static async ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, ImmutableArray<double> designPoint, LoweringCache cache, CancellationToken token = default) {
        Seq<string> free = designSymbols.Filter(formula.FreeSymbols.Contains);
        if (free.IsEmpty) { return Fin.Succ(SymbolicTape.Empty); }
        // The partials compile over `free` (the design symbols PRESENT in the formula), so the baked design
        // point projects onto `free` in lockstep: a declared-but-absent design symbol drops from BOTH the
        // partial order and the point. Baking the unfiltered `designPoint` would arity-mismatch every
        // `Backward` invoke (its check is `designPoint.Length == SymbolOrder.Count`). The recovered gradient is
        // therefore labelled by `free`, which the tape carries for the consumer to scatter back by symbol name.
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

    // Scalar f : R^n -> R, so J is the 1xn gradient row and the reverse VJP is x̄ = Jᵀ·ȳ = ∇f·ȳ₀:
    // the scalar output cotangent ȳ₀ SCALES the gradient row (broadcast multiply), never an
    // elementwise Hadamard product against an n-vector cotangent.
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

- [SYMBOLIC_REGISTRY_WIRING]: the live `DesignVariable` `[Union]` widening that lowers a free symbolic design field (a `DesignVariable.Symbolic` carrying the formula and its declared design symbols) to a `SymbolicTape` lands at `Solver/optimizer#OPTIMIZER_LANE` as one additive arm on the existing `DesignVariable` union — the lowering calling `SymbolicJacobian.Lower(formula, designSymbols, designPoint, cache)` at the current iterate (the analytic partials are functions of the design point, so the tape is re-pointed each generation off the content-addressed cached delegates) and the adjoint routing `SymbolicAdjoint.Chain(Seq<SymbolicTape>, seed)`, the two carriers (`GeometryTape`, `SymbolicTape`) unified under a `Seq<AdjointTape>` `[Union]` so `DescendAdjoint` folds a mixed tape under the one two-argument `Chain` contract — never a parallel registry and never a row on the `Type→TensorOpFamily` `OperatorRows` map (which carries tensor-op-family geometry rows only). The `SymbolicTape`/`SymbolicAdjoint` two-argument contract this page mints is the producer surface that arm composes; the optimizer page owns the consumer-side `DesignVariable.Symbolic` carrier and the `AdjointTape` union.
