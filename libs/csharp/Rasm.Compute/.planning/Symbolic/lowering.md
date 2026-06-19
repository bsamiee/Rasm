# [SYMBOLIC_LOWERING]

The compile-and-reuse terminal of the symbolic CAS arm: a simplified `SymbolicExpr` (the F# `Expression` wrapped by `Symbolic/expression#SYMBOLIC_EXPR`) lowers once to a native `Func<double…>`/`Delegate` evaluator through the verified `Compile.compileExpression{,1,2,3}` `FSharpOption<Func<…>>` arity surface (`.api/api-mathnet-symbolics.md#COMPILE_SURFACE` L62-L64), is carried by the `CompiledExpr` value keyed on the canonical-NF `XxHash128` content key `Symbolic/expression#SYMBOLIC_EXPR` already mints, and is reused through a `LoweringCache` read-through over the one model-lane cache the inference lane owns (`Model/inference#RESULT_CACHE` `CachePolicy.ReadThrough` over `CacheSurface`/`CacheLane.ModelResult`, never a second cache instance). The page owns the `CompiledExpr` delegate carrier, the `CompileArity` axis selecting the arity-exact module-level compile form over the symbol order, the `LoweringCache` read-through and its `CompiledKey` derivation composing `Runtime/codecs#CONTENT_ADDRESSING`, and — the cross-lane contribution — the `SymbolicJacobian` lowering that differentiates a formula by each free design symbol through `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate`, compiles each partial to a native delegate, and packs them into a `SymbolicTape` whose `SymbolicAdjoint.Backward` conforms to the identical reverse-mode transpose law `x̄ = Jᵀ·ȳ` the DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape`/`SensitivityLaw.Chain(Seq<GeometryTape>, seed)` rows answer, so the symbolic source registers AS a third row on the existing `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` adjoint registry beside the `Gradient`/`CotangentLaplacian` geometry rows — the `csharp:IDEAS#SYMBOLIC_CAS_SUBDOMAIN` `GradientSource.Symbolic` label is the design name for this one registry entry, never a parallel `(Seq<double>, double)` gradient path and never a standalone `GradientSource` type.

The lowering is the gate the `Symbolic/dimensional#DIMENSION_PROOF` pre-numeric admission runs strictly before: a formula compiles and registers a Jacobian only after its `DimensionProof` admits, so a dimension-inconsistent expression never reaches a `CompiledExpr` slot, the optimizer oracle, or the integrator seed. A formula carrying no free design symbol lowers an empty Jacobian and `DescendAdjoint` falls to finite-difference exactly as the absent-`DesignMesh` case lowers an empty geometry tape and descends degenerate by construction (`Solver/optimizer#OPTIMIZER_LANE` L786) — the symbolic source admits without breaking that invariant and without a parallel descent. The page is host-local and carries no TS_PROJECTION cluster: the `CompiledExpr` delegate is an interior runtime value that never crosses a wire, and the only cross-surface fact is the `CompiledKey` content key the `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` branch seam crosses by reference to the Persistence cost-catalog/QTO-formula consumers, aligned at the key and never coupled into a sibling interior. The compiled delegate is netstandard2.0 ALC-safe (the F# `Expression` core and the `LambdaExpression.Compile()` it lowers carry no runtime-codegen ALC hazard the live plugin host forbids). An in-proc symbolic-regression fit is the rejected form — offline formula discovery is the Python branch's, and this page owns compile-and-cache plus the analytic-Jacobian lowering over an already-known, already-admitted expression.

## [1]-[INDEX]

- [1]-[LOWERING]: `CompiledExpr` delegate carrier; `CompileArity` axis; the `Compile.compileExpression` lowering.
- [2]-[LOWERING_CACHE]: `LoweringCache` read-through over `CacheLane.ModelResult`; `CompiledKey` content-key derivation.
- [3]-[SYMBOLIC_JACOBIAN]: `SymbolicJacobian` partial-derivative lowering; `SymbolicTape`/`SymbolicAdjoint` transpose row.

## [2]-[LOWERING]

- Owner: `CompiledExpr` the carrier binding a lowered native delegate to its source `SymbolicExpr` content key and its ordered free-symbol vector; `CompileArity` `[SmartEnum<string>]` the five rows (`nullary` · `unary` · `binary` · `ternary` · `variadic`) selecting the arity-exact `Compile.compileExpression{,1,2,3}` module form over the symbol order, each row carrying the boxed-delegate down-cast so a five-symbol formula routes the variadic `compileExpression` `Delegate` form and a one-symbol formula routes the `compileExpression1` `Func<double,double>` form; `CompileCapsule` the one boundary owner projecting the `FSharpOption<Delegate>`/`FSharpOption<Func<…>>` compile carrier into the `Fin` rail (composing the `Symbolic/expression#SYMBOLIC_EXPR` `ExpressionCapsule.ProjectCompile`), so a compile-decline (an unsupported `FunctionN` node) is a `ComputeFault.NonDifferentiable` fault and never an unwrapped null delegate.
- Cases: `CompileArity` rows `nullary` (no free symbol — the formula is a constant, evaluated once) · `unary` (`compileExpression1`, `Func<double,double>`) · `binary` (`compileExpression2`, `Func<double,double,double>`) · `ternary` (`compileExpression3`, `Func<double,double,double,double>`) · `variadic` (`compileExpression` over a `Symbol list`, the boxed `Delegate` the caller invokes with a positional `double[]`); the arity row is selected by the count of the ordered free-symbol vector, never a call-site branch.
- Entry: `public static Fin<CompiledExpr> Compile(SymbolicExpr source, Seq<string> symbolOrder)` is the one polymorphic lowering — the symbol order fixes the positional argument convention (the i-th `double` binds the i-th symbol), `CompileArity.Select(symbolOrder.Count)` picks the module form, the chosen `Compile.compileExpression*` call returns its `FSharpOption<Delegate>`, and `CompileCapsule.Project` lifts it to a `CompiledExpr` or a `NonDifferentiable` fault; `public Fin<double> Invoke(ImmutableArray<double> arguments)` is the typed call site dispatching the stored delegate by arity (a `Func<double,double>` for the unary row, an `Array`-bound `DynamicInvoke` collapsed once for the variadic row), validating the argument count against the symbol order so a mis-arity call is a `ComputeFault` rather than a `TargetParameterCountException`.
- Auto: `Compile` reads the ordered `symbolOrder` rather than `source.FreeSymbols` directly so the positional convention is caller-fixed and stable across a re-compile (two callers compiling the same formula with the same symbol order share one delegate and one cache slot); the arity row's `compileExpression{1,2,3}` form pre-casts the boxed `LambdaExpression.Compile()` delegate to the arity-exact `Func` so the unary/binary/ternary call sites invoke a strongly-typed `Func` with no reflection, and only the variadic row crosses a single `DynamicInvoke` collapsed to one positional `double[]` bind; the nullary row never compiles a delegate — a free-symbol-empty formula evaluates once through `Symbolic/expression#SYMBOLIC_EXPR` `Evaluate` and the `CompiledExpr` carries the cached constant, so a constant formula is a zero-argument value, never a degenerate delegate.
- Receipt: `CompiledExpr` carries no receipt of its own — the compile outcome rides the `LOWERING_CACHE` `CacheIndexFact` hit/miss/store slot the model-lane `Model/inference#RESULT_CACHE` already stamps, and a compile-decline rides the `ComputeFault.NonDifferentiable` 2215 arm `Symbolic/expression#BUILD_ADMISSION` declares; the downstream optimize outcome carries the `Optimization` receipt slot.
- Packages: MathNet.Symbolics (`Compile.compileExpression{,1,2,3}` module forms, `Symbol.symbol` construction, the `FSharpOption<Delegate>` carrier), MathNet.Numerics.FSharp (the F# facade the compile surface ships on), Thinktecture.Runtime.Extensions (`CompileArity` `[SmartEnum]`), LanguageExt.Core (`Fin` rail, `Seq`/`ImmutableArray` carriers), FSharp.Core (transitive `FSharpOption`).
- Growth: a new arity is impossible past ternary — the variadic row absorbs every higher arity through the boxed `Delegate` positional form, so a `compileExpression4` is never minted; a new compile target (the `Compile.compileComplexExpression` complex-domain form) is one `CompileArity` companion row binding the complex delegate, never a parallel `CompiledComplexExpr` carrier; a new evaluation convention (e.g. a `Span<double>` argument bind for a hot loop) is one `Invoke` overload over the same delegate, never a second carrier.
- Boundary: `Compile` is the single lowering entry — a `CompileUnary`/`CompileBinary`/`CompileVariadic` factory trio modeling one concept is the collapsed defect, and the symbol-order count selects the arity row, never the call site; the module-level `Compile.compileExpression{,1,2,3}` `FSharpOption` form is the admitted surface because the explicit `None`-projection distinguishes a compile-decline from a parse-decline (the fluent `SymbolicExpression.Compile(params string[])` wrapper unwraps the option and throws on decline, so it is the rejected source for the cache path where a decline must land on the `Fin` rail — `.api/api-mathnet-symbolics.md#COMPILE_SURFACE` L64); the `FSharpOption<Delegate>` never crosses into the interior — `CompileCapsule` projects it at the seam exactly once and a bare `.Value` on a `None` is the eliminated null-deref; the positional symbol order is the one argument convention and an unordered `Map<string,double>`-keyed invoke is the rejected form because the compiled delegate is positional by construction (the F# `Symbol list` the compile call takes fixes the order); the compiled delegate is netstandard2.0 ALC-safe and a `System.Reflection.Emit`-based re-implementation of the lowering is the deleted form because `Compile.compileExpression` already owns the `LambdaExpression.Compile()` lowering.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<SymbolicKeyPolicy, string>]
[KeyMemberComparer<SymbolicKeyPolicy, string>]
public sealed partial class CompileArity {
    public static readonly CompileArity Nullary = new("nullary", 0);
    public static readonly CompileArity Unary = new("unary", 1);
    public static readonly CompileArity Binary = new("binary", 2);
    public static readonly CompileArity Ternary = new("ternary", 3);
    public static readonly CompileArity Variadic = new("variadic", -1);

    private readonly int rank;

    public static CompileArity Select(int symbolCount) =>
        symbolCount switch { 0 => Nullary, 1 => Unary, 2 => Binary, 3 => Ternary, _ => Variadic };

    public int Rank => rank;
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
            ? Constant.Match(Some: Fin.Succ, None: () => Fin.Fail<double>(new ComputeFault.NonDifferentiable("<nullary-without-constant>")))
        : arguments.Length != SymbolOrder.Count
            ? Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<arity:{arguments.Length}≠{SymbolOrder.Count}>"))
        : Arity switch {
            { } a when a == CompileArity.Unary => Fin.Succ(((Func<double, double>)Evaluator)(arguments[0])),
            { } a when a == CompileArity.Binary => Fin.Succ(((Func<double, double, double>)Evaluator)(arguments[0], arguments[1])),
            { } a when a == CompileArity.Ternary => Fin.Succ(((Func<double, double, double, double>)Evaluator)(arguments[0], arguments[1], arguments[2])),
            _ => Fin.Succ((double)Evaluator.DynamicInvoke([.. arguments.Select(static x => (object)x)])!),
        };
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class CompileCapsule {
    public static Fin<CompiledExpr> Compile(SymbolicExpr source, Seq<string> symbolOrder) {
        var arity = CompileArity.Select(symbolOrder.Count);
        if (arity == CompileArity.Nullary) {
            return source.Evaluate(Map<string, double>())
                .Map(value => new CompiledExpr(source.ContentKey, symbolOrder, arity, NoOp, Some(value)));
        }
        var symbols = ListModule.OfSeq(symbolOrder.Map(Symbol.symbol));
        var lowered = arity switch {
            { } a when a == CompileArity.Unary => Box(Compile.compileExpression1(source.Expression, symbols.Head)),
            { } a when a == CompileArity.Binary => Box(Compile.compileExpression2(source.Expression, symbols.Head, symbols.Tail.Head)),
            { } a when a == CompileArity.Ternary => Box(Compile.compileExpression3(source.Expression, symbols.Head, symbols.Tail.Head, symbols.Tail.Tail.Head)),
            _ => Compile.compileExpression(source.Expression, symbols),
        };
        return ExpressionCapsule.ProjectCompile(lowered, source.Canonical)
            .Map(@delegate => new CompiledExpr(source.ContentKey, symbolOrder, arity, @delegate, None));
    }

    static readonly Func<double> NoOp = static () => double.NaN;

    static FSharpOption<Delegate> Box<TDelegate>(FSharpOption<TDelegate> option) where TDelegate : Delegate =>
        OptionModule.IsSome(option)
            ? FSharpOption<Delegate>.Some(option.Value)
            : FSharpOption<Delegate>.None;
}
```

## [3]-[LOWERING_CACHE]

- Owner: `CompiledKey` the static derivation folding the source `SymbolicExpr.ContentKey` and the ordered symbol-vector digest into one `XxHash128` cache key, composing the suite hash law `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key` holds (never a second hashing pass and never a string-path key); `LoweringCache` the read-through service over the one `HybridCache` substrate the model lane already runs — it scopes the content key onto the settled `CacheLane.ModelResult` lane `Model/inference#RESULT_CACHE` declares and dispatches `HybridCache.GetOrCreateAsync` under the `CachePolicy.ReadThrough` row, never constructing or registering a `HybridCache` instance of its own and never minting a parallel `CacheLane`. The lowering key is a content-addressed symbolic key, not a `ModelResultKey` (the `ModelResultKey` carries `ModelIdentity`/`ExecutionProvider`/`ModelPrecision` ONNX-run facts a compiled formula has none of), so the lowering composes the cache substrate and the `CachePolicy` vocabulary while keying by its own content identity.
- Cases: the cache posture is the existing `Model/inference#RESULT_CACHE` `CachePolicy` `[SmartEnum]` (`ReadThrough` serves a hit and stores a miss — the produced `Fin<CompiledExpr>` is stored whether success or fault, so a deterministic `NonDifferentiable` compile-decline serves the cached failure rather than re-compiling under the lane's entry TTL the model lane owns) — no new cache policy and no new fact slot; the lowering rides the model-lane `CacheLane.ModelResult` lane scoped by the symbolic content key, never a parallel `CacheLane`.
- Entry: `public ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default)` is the one read-through — it derives the `CompiledKey`, scopes it onto `CacheLane.ModelResult`, and dispatches `cache.GetOrCreateAsync(scopedKey, state, factory, entryOptions, token)` where the stampede-aware `factory` runs `CompileCapsule.Compile` so identical-formula-identical-order calls across distinct callers coalesce on the content-addressed key — a cost-catalog formula compiled once for an optimizer Jacobian is reused for a QTO evaluation without a second lowering; a faulted compile re-runs only after the negative TTL expires.
- Auto: `CompiledKey.Of` stamps the `SymbolicExpr.ContentKey` (already the canonical-NF `XxHash128` over `Infix.formatStrict`, so two structurally distinct but algebraically equal formulas that simplify to the same normal form key identically) plus the ordered-symbol digest (so a re-order of the positional convention keys distinctly because the compiled delegate's argument order differs) in one `XxHash128` call, never a path-derived or display-string key; the `GetOrCreateAsync` single-flight is the single population point so stampede control rides the cache surface with zero call-site ceremony; the cache stores the `CompiledExpr` value (the delegate plus its key, order, and arity) so a warm read re-binds the strongly-typed `Func` without re-lowering — the local tier carries the live delegate and the distributed tier carries the `(ContentKey, SymbolOrder, Arity)` re-lowering seed because a compiled `Delegate` is not durably serializable across a process boundary, so a cold-store miss re-lowers from the seed while a hot-process hit reuses the live `Func`.
- Receipt: the lowering rides the model-lane `CacheIndexFact` hit/miss/store/negative slots `Model/inference#RESULT_CACHE` stamps, never a parallel lowering receipt; a compile-decline stored negative is the `NonDifferentiable` 2215 arm — no new receipt case.
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128`/`XxHash3.HashToUInt64` for `CompiledKey`), Microsoft.Extensions.Caching.Hybrid (the `HybridCache` substrate, `HybridCacheEntryOptions`, and `HybridCacheEntryFlags`, reached over the settled `CacheLane.ModelResult` lane — never registered or instantiated here), LanguageExt.Core (`Fin`, `Seq`), Rasm.AppHost (project — the `CacheLane` lane descriptor the model lane composes).
- Growth: a new cache posture is one row on the existing `CachePolicy` `[SmartEnum]` at `Model/inference#RESULT_CACHE`, never a lowering-local cache policy; a new key contributor (e.g. a target-runtime tag if a future ALC variant compiles distinct delegates) is one more stamp in `CompiledKey.Of`, never a parallel key; a new cache substrate is impossible — the `CacheLane.ModelResult` lane over the one `HybridCache` is the single substrate and a lowering-local `HybridCache` registration is the named defect.
- Boundary: `LoweringCache` rides the model-lane cache substrate and never owns a cache instance — `CacheLane.ModelResult` over the one `HybridCache` is the single cache owner `Model/inference#RESULT_CACHE` declares, and a hand-rolled `ConcurrentDictionary<UInt128, CompiledExpr>` memoization beside it is the deleted form; the cache key is `XxHash128` over the canonical content key plus the symbol order — the suite hash law `Runtime/codecs#CONTENT_ADDRESSING` holds, never a second hashing scheme and never a `source.Canonical` string key (the content key already digests the canonical form, so re-hashing the string is the redundant pass); the lowering keys by its own content identity rather than the ONNX `ModelResultKey` because a compiled formula carries no `ModelIdentity`/`ExecutionProvider`/`ModelPrecision` — forcing a fabricated `ModelResultKey` is the rejected form; the `GetOrCreateAsync` factory is the single population point and a caller that compiles-then-caches in two steps is the rejected form because it duplicates the stampede lock the single-flight already owns; the cross-process result-reuse horizon is the lane's `HybridCacheEntryOptions` the model lane configures, never a lowering-local `Duration horizon` parameter.

```csharp contract
// --- [CONSTANTS] -------------------------------------------------------------------------
file static class LoweringEntry {
    public static readonly HybridCacheEntryOptions Compiled = new() { Flags = HybridCacheEntryFlags.DisableDistributedCacheWrite };
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class CompiledKey {
    public static UInt128 Of(SymbolicExpr source, Seq<string> symbolOrder) {
        Span<byte> seed = stackalloc byte[16];
        MemoryMarshal.Write(seed, source.ContentKey);
        long orderDigest = unchecked((long)XxHash3.HashToUInt64(
            MemoryMarshal.AsBytes(string.Join('\u001f', symbolOrder).AsSpan())));
        return XxHash128.HashToUInt128(seed, orderDigest);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
public sealed class LoweringCache(HybridCache cache) {
    public ValueTask<Fin<CompiledExpr>> Through(SymbolicExpr source, Seq<string> symbolOrder, CancellationToken token = default) =>
        cache.GetOrCreateAsync(
            CacheLane.ModelResult.Scoped($"symbolic:{CompiledKey.Of(source, symbolOrder):x32}"),
            (Source: source, Order: symbolOrder),
            static (state, _) => new ValueTask<Fin<CompiledExpr>>(CompileCapsule.Compile(state.Source, state.Order)),
            LoweringEntry.Compiled,
            [CacheLane.ModelResult.Key],
            token);
}
```

## [4]-[SYMBOLIC_JACOBIAN]

- Owner: `SymbolicJacobian` the static lowering differentiating a formula by each of its free design symbols through `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate`, compiling each partial to a `CompiledExpr`, and packing them into a `SymbolicTape`; `SymbolicTape` the symbolic analog of the DDG `Tensor/dispatch#EQUIVALENCE_INTEROP` `GeometryTape` carrying the ordered design-symbol vector and the compiled partial-derivative delegates evaluated at the design point; `SymbolicAdjoint` the reverse-mode transpose owner whose `Backward` applies the analytic Jacobian transpose `x̄ = Jᵀ·ȳ` and whose `Chain` folds a `Seq<SymbolicTape>` exactly as `SensitivityLaw.Chain(Seq<GeometryTape>, seed)` folds the geometry tape — the same `Fin<ReadOnlyMemory<float>>` reverse-mode contract the optimizer `DescendAdjoint` reads, so the symbolic source is a row on the `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` registry, not a parallel descent.
- Cases: a free design symbol present in `source.FreeSymbols` lowers to one compiled partial `∂f/∂xᵢ` on the tape; a formula with no free design symbol (a constant, or a formula whose free symbols are all non-design parse-context symbols) lowers an empty `SymbolicTape` so `SymbolicAdjoint.Chain` returns the upstream seed unchanged and `DescendAdjoint` falls to finite-difference — the degenerate-descent invariant `Solver/optimizer#OPTIMIZER_LANE` L786 holds for the symbolic source exactly as it holds for the absent-`DesignMesh` case; a non-differentiable node (a `FunctionN` the `Calculus.differentiate` declines, surfaced as the `compileExpression` `None`) is the `NonDifferentiable` 2215 fault on the lowering rail before any tape records.
- Entry: `public static ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, LoweringCache cache, CancellationToken token = default)` is the one lowering — it filters `designSymbols` to those present in `formula.FreeSymbols` (a declared-but-absent design symbol drops out so the tape carries only the live partials), differentiates the formula by each through `SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol))`, compiles each partial over the full design-symbol order through the `LoweringCache.Through` read-through so every partial delegate shares the one positional `designPoint` convention and is content-addressed and reused, and packs the ordered partials into the tape (the async cache read is awaited per-partial and a compile-decline short-circuits the fold on the `Fin` rail, never a blocked `.GetResult()`); `public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ImmutableArray<double> designPoint, ReadOnlyMemory<float> cotangent)` evaluates each compiled partial at the design point and applies the Jacobian-transpose-times-cotangent contraction, returning the gradient in the optimizer's `ReadOnlyMemory<float>` seed shape.
- Auto: `Lower` reuses the `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate` arm rather than re-implementing the analytic derivative (the `Calculus.differentiate` over the F# `Expression` is the single derivative owner) and reuses the `LoweringCache` so a partial `∂f/∂x` compiled once for one design symbol is reused across optimizer generations; the tape evaluates each partial at the design point through `CompiledExpr.Invoke` so the Jacobian row is the analytic gradient at that point, never a finite-difference stencil; `SymbolicAdjoint.Backward` contracts `Jᵀ·ȳ` where the cotangent `ȳ` is the upstream objective sensitivity and the result is the design-space gradient, the same transpose law `Tensor/dispatch#EQUIVALENCE_INTEROP` `Backward.Operator` applies for the geometry rows, so the symbolic and geometry sources compose under one `Chain` fold; an empty tape short-circuits `Chain` to the identity so the degenerate case is structural, never a guarded special case.
- Receipt: the symbolic-Jacobian source lands no receipt of its own — the gradient feeds the optimizer `DescendAdjoint` which stamps the existing `Optimization` receipt slot (`Solver/optimizer#OPTIMIZER_LANE`), and the surrogate-hit/evaluation counts ride that same slot; a lowering fault rides the `ComputeFault` rail at the `NonDifferentiable` arm.
- Packages: MathNet.Symbolics (`Calculus.differentiate` through the `Symbolic/expression#OPERATION_FOLD` `Apply` fold, `Compile.compileExpression` through the `LOWERING` lowering), System.Numerics.Tensors (`TensorPrimitives` for the `Jᵀ·ȳ` contraction over the gradient vector, the same SIMD surface `Tensor/dispatch#EQUIVALENCE_INTEROP` uses), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Seq`, `Traverse`), Rasm (project — the `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw`/`AdjointMode` reverse-mode contract the symbolic tape conforms to).
- Growth: a new gradient source on the optimizer registry is one more `OperatorRows` row at `Solver/optimizer#OPTIMIZER_LANE` (the geometry source, the symbolic source, and a future analytic-FEM source are sibling rows), never a parallel adjoint path; a higher-order symbolic sensitivity (a Hessian for a Newton step) is one `SymbolicJacobian` companion differentiating the partials a second time, riding the same tape/transpose surface, never a new descent; the symbolic tape and the geometry tape stay distinct carriers but answer one `Chain` contract, so a unifying `Seq<AdjointTape>` `[Union]` is the only collapse the optimizer registry would admit, never a duplicated descent kernel.
- Boundary: the symbolic-Jacobian arm registers AS a row on the existing `Solver/optimizer#OPTIMIZER_LANE` `DesignProblem.OperatorRows` adjoint registry — the `GradientSource.Symbolic` `csharp:IDEAS#SYMBOLIC_CAS_SUBDOMAIN` label is the design name for this one registry entry, and a standalone `GradientSource` type or a parallel `(Seq<double>, double)` gradient path is the named defect (that `(Seq<double>, double)` signature is the `Surrogate.Predict` RETURN shape `Solver/optimizer#OPTIMIZER_LANE` L629/L773, never the gradient contract; the optimizer evaluate oracle is `Func<DesignPoint, Fin<Seq<double>>>` and the gradient surface is `DescendAdjoint` reading `SensitivityLaw.Chain(Seq<GeometryTape>, seed)`); the analytic derivative is the `Symbolic/expression#OPERATION_FOLD` `SymbolicOp.Differentiate` arm and a lowering-local finite-difference stencil is the deleted form because the package owns the exact analytic derivative; the empty-tape degenerate descent is the invariant the symbolic source must preserve — a formula with no free design symbol lowers an empty Jacobian and falls to finite-difference exactly as a `DesignProblem` with no `DesignMesh` lowers an empty geometry tape (`Solver/optimizer#OPTIMIZER_LANE` L786), so the symbolic source never breaks the degenerate-descent law and never introduces a third descent branch; `SymbolicAdjoint.Chain` conforms to the identical reverse-mode `Fin<ReadOnlyMemory<float>>` transpose contract the geometry `SensitivityLaw.Chain` answers, so the optimizer descends a mixed geometry-and-symbolic tape under one fold; the dimensional gate runs strictly first — `Lower` is invoked only after `Symbolic/dimensional#DIMENSION_PROOF` admits, so a dimension-inconsistent formula never reaches a compiled Jacobian.

```csharp contract
// --- [MODELS] ----------------------------------------------------------------------------
public sealed record SymbolicTape(Seq<string> DesignSymbols, Seq<CompiledExpr> Partials) {
    public static readonly SymbolicTape Empty = new(Seq<string>(), Seq<CompiledExpr>());

    public bool IsDegenerate => Partials.IsEmpty;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicJacobian {
    public static async ValueTask<Fin<SymbolicTape>> Lower(SymbolicExpr formula, Seq<string> designSymbols, LoweringCache cache, CancellationToken token = default) {
        var free = designSymbols.Filter(formula.FreeSymbols.Contains);
        if (free.IsEmpty) { return Fin.Succ(SymbolicTape.Empty); }
        Fin<Seq<CompiledExpr>> partials = Fin.Succ(Seq<CompiledExpr>());
        foreach (var symbol in free) {
            if (partials.IsFail) { break; }
            Fin<CompiledExpr> compiled = await SymbolicOps.Apply(formula, new SymbolicOp.Differentiate(symbol))
                .Match(
                    Succ: partial => cache.Through(partial, free, token),
                    Fail: fault => new ValueTask<Fin<CompiledExpr>>(Fin.Fail<CompiledExpr>(fault)));
            partials = partials.Bind(acc => compiled.Map(acc.Add));
        }
        return partials.Map(rows => new SymbolicTape(free, rows));
    }

    public static Fin<ReadOnlyMemory<float>> Backward(SymbolicTape tape, ImmutableArray<double> designPoint, ReadOnlyMemory<float> cotangent) =>
        tape.IsDegenerate
            ? Fin.Succ(cotangent)
            : tape.Partials.Traverse(partial => partial.Invoke(designPoint))
                .Map(row => Contract([.. row], cotangent));

    static ReadOnlyMemory<float> Contract(Seq<double> jacobianRow, ReadOnlyMemory<float> cotangent) {
        float[] jacobian = [.. jacobianRow.Map(static g => (float)g)];
        float[] gradient = new float[jacobian.Length];
        TensorPrimitives.Multiply(jacobian, cotangent.Span[..jacobian.Length], gradient);
        return gradient;
    }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
public static class SymbolicAdjoint {
    public static Fin<ReadOnlyMemory<float>> Chain(Seq<SymbolicTape> tape, ImmutableArray<double> designPoint, ReadOnlyMemory<float> upstream) =>
        tape.Rev().Fold(
            Fin.Succ(upstream),
            (grad, step) => grad.Bind(seed => SymbolicJacobian.Backward(step, designPoint, seed)));
}
```

## [5]-[RESEARCH]

- [SYMBOLIC_REGISTRY_WIRING]: the live `DesignProblem.OperatorRows` registry extension that lowers a free symbolic design field (a `DesignVariable.Symbolic` carrying the formula and its declared design symbols) to a `SymbolicTape` lands when the `Solver/optimizer#OPTIMIZER_LANE` `DesignVariable` `[Union]` is widened with the symbolic case — an additive arm on the existing union, the `OperatorRows` table gaining one `typeof(DesignVariable.Symbolic)` row whose lowering calls `SymbolicJacobian.Lower` and whose `.Adjoint` routes `SymbolicAdjoint.Chain`, never a parallel registry. The `SymbolicTape`/`SymbolicAdjoint` contract this page mints is the producer surface that arm composes; the optimizer page owns the consumer-side registry row and the `DesignVariable.Symbolic` carrier.
