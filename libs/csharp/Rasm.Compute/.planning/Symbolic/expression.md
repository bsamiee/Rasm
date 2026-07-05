# [COMPUTE_SYMBOLIC_EXPRESSION]

Rasm.Compute symbolic lane head: the closed computer-algebra owner over the admitted `AngouriMath` `Entity` expression tree, internalized once as the `SymbolicExpr` `[ComplexValueObject]` that carries the boundary-admitted `Entity` as a stored member while its identity is the canonical-normal-form content key ALONE — the `Entity` member is equality-inert through a constant `[MemberEqualityComparer]` (the owner-local `EntityInert` accessor whose comparer holds every pair equal at hash zero, so the generated `Equals`/`GetHashCode` discriminate on `ContentKey` alone; `[IgnoreMember]` is the rejected form because it hides the member from the generated `Create` factory, and `Entity.GetHashCode()` is per-process randomized, the forbidden identity source), so two algebraically-equal builds whose raw trees differ collapse onto one key and one cache slot. The canonical form is the pin-stable `Simplify()` normal form rendered through `Stringize()`: at the admitted 1.4.0 pin, commutative operand ordering is structural-lexicographic — `SortAndGroup` orders children by pure-recursive `SortHash` strings through an explicit `OrderBy`, with no `GetHashCode`, no `Random`, and no dictionary-enumeration dependence on the canonical path — so the canonical form is byte-stable across process runs and the content key never drifts. The page owns the `SymbolicExpr` value, the one polymorphic `Build` entry discriminating an infix-string parse from a structured construction over the `BuildSpec` `[Union]` (never a `Parse`/`FromEntity`/`FromInfix` factory trio), the `SymbolicOp` `[Union]` operation family folding the full CAS surface — `Differentiate` (order-parameterized), `Integrate` (indefinite and definite on one case), `Limit` (`ApproachFrom`-directed), `Solve` (the solution-`Set` capability `MathNet.Symbolics` never had, reaching the AEC cut-parameter inversion), `Taylor` (`MathS.Taylor`, degree 1 the tangent line — one case, never a `TangentLine` sibling), `Simplify` over the `SymbolicForm` normalization rows, `Substitute`, and `Approximate` partial numeric substitution — through one generated total `Switch`, never a per-operation method ladder, and the `SymbolicFault` rail extending the existing `ComputeFault` 2200 band at the verified codes 2213/2214/2215 (parse-failure / undefined-symbol / analytic-decline), one fault family per package, never a parallel `SymbolicError`. The engine crosses one boundary: `Entity.TryParse(string, IFormatProvider, out Entity)` is the non-throwing parse seam (the ANTLR front-end that deleted the FParsec parser outright), and `NumberBox` collapses the numeric-tower leaf (`Entity.Number.Real` ⊃ `Rational` ⊃ `Integer`, beside the non-real `Complex`) into a `Fin<double>` at the evaluation seam; no engine carrier ever crosses a wire.

The canonical content key this page mints is the single identity the lane composes by: `Symbolic/dimensional#DIMENSION_PROOF` walks the admitted `Entity` node records (`Sumf`/`Mulf`/`Divf`/`Powf`/`Minusf` and the function nodes) for its SI base-dimension fold, and `Symbolic/lowering#LOWERING` keys its read-through `CompiledExpr` cache and its `SymbolicJacobian` partial-derivative lowering (the `SymbolicTape` the `Solver/optimizer#OPTIMIZER_LANE` `DescendAdjoint` folds) off this same `XxHash128` over the canonical form — the operation fold and the canonical key both live here, the compiled-delegate carrier and the cache plumbing live on `Symbolic/lowering#LOWERING`, and the dimension monomial lives on `Symbolic/dimensional#DIMENSION_PROOF`, three distinct concerns over one shared value. `Solver/satisfy#RULE_SATISFACTION` consumes the same tree from the constraint side: an `Entity.Statement` (an (in)equality-sorted `Entity`) lowers term-by-term to Z3 assertions, the CAS the lowering source, never a second expression algebra. The page is host-local and carries no TS_PROJECTION cluster: the `SymbolicExpr` is an interior value that never sits between wire and rail; its only cross-surface contribution is the content key the branch `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` seam crosses by reference to the Persistence cost-catalog/QTO-formula consumers, aligned at the key, never coupled into a sibling interior. The single statement exemption is the boundary marshal: the `TryParse` out-parameter read and the `MathS.Utils.TryGetPolynomial` out-dictionary read project foreign carriers into the rail in language-owned control flow; every other operation is a generated `Switch` fold or a `Bind` chain. Algebraic equivalence between differently-spelled inputs is not identity — the content key binds the admitted expression's canonical form, and identity comparisons over the canonical string bind ordinal comparison, culture-independent by construction. An in-proc symbolic-regression fit is the rejected form — offline formula discovery belongs to the Python branch, and this page owns only the analytic algebra over an already-known expression.

## [01]-[INDEX]

- [01]-[SYMBOLIC_EXPR]: `SymbolicExpr` `[ComplexValueObject]`; the `Entity` numeric-leaf capsule; canonical key.
- [02]-[BUILD_ADMISSION]: `BuildSpec` `[Union]` parse/structured entry; the `SymbolicFault` 2213–2215 rail.
- [03]-[OPERATION_FOLD]: `SymbolicOp` `[Union]` over the CAS surface; one total `Apply` switch; typed harvests.

## [02]-[SYMBOLIC_EXPR]

- Owner: `SymbolicExpr` `[ComplexValueObject]` over the `AngouriMath` `Entity`, identity-bearing through its canonical `XxHash128` content key (never `Entity.GetHashCode()`, whose per-process randomization forbids it as a key source even though `Entity` equality itself is structural); `NumberBox` the one projector collapsing the evaluated numeric tower to a `Fin<double>`; `ComparerAccessors.StringOrdinal` the ordinal equality/comparer accessor so the free-symbol set folds and sorts deterministically.
- Cases: the wrapped `Entity` is the engine's closed node hierarchy — the numeric-tower leaves (`Entity.Number.Integer` ⊂ `Rational` ⊂ `Real` ⊂ `Complex`, exact via the `PeterO.Numbers` `EInteger`/`ERational`/`EDecimal` carriers — the exactness the content key depends on), `Entity.Variable` (free symbols, `Name`-keyed; the pi/e constants ride the same type and surface through `VarsAndConsts`, excluded from `Vars`), the arithmetic node records (`Sumf(Augend, Addend)` · `Minusf(Subtrahend, Minuend)` · `Mulf(Multiplier, Multiplicand)` · `Divf(Dividend, Divisor)` · `Powf(Base, Exponent)` · `Logf(Base, Antilogarithm)`), the function nodes (`Absf`/`Signumf`/the trig family, each `(Argument)`), the deferred-analytic residues (`Derivativef`/`Integralf`/`Limitf` — the decline sentinels the operation fold gates on), `Entity.Set` (`FiniteSet`/`Interval`/`ConditionalSet` — the solve-result carrier), and `Entity.Boolean`/statement nodes (`Equalsf`/`Greaterf`/… — the constraint form `Solver/satisfy` lowers); the numeric projection collapses to one `Fin<double>` at the `Real` leaf (subsuming `Rational`/`Integer`, and carrying ±∞ as `Real` specials) and faults the `Complex`/unbound arms.
- Entry: `SymbolicExpr.Of(Entity entity)` is the interior factory — it routes the generated `[ComplexValueObject]` `Create(entity, default)` whose `ValidateFactoryArguments` derives the content key once from the entity (the passed key slot is always overwritten, so a forged key can never desync identity); `private static string CanonicalProjection(Entity e)` is the canonical-normal-form projector — `e.Simplify().Stringize()`, the engine's own canonicalization pass rendered once (never the raw `Stringize()` of an unsimplified tree, because two algebraically equal trees can render distinct strings without the reduce pass), and `public string Canonical` re-exposes it; `public UInt128 ContentKey` is the kernel `ContentHash.Of` federation entry over the canonical-form UTF-16 bytes (`MemoryMarshal.AsBytes` reinterprets the `ReadOnlySpan<char>` of the normalized string — the host-local LE char bytes, deterministic across .NET targets, never a UTF-8 transcode; a per-call-site `XxHash128.HashToUInt128` is the named second-hasher defect the one-hasher law deletes), the one identity `dimensional`/`lowering` compose by; `public Seq<string> FreeSymbols` folds `Entity.Vars` (free variables only — constants stay out) into the ordinal-sorted free-identifier set; `public Fin<double> Evaluate(Map<string, double> bindings)` substitutes each binding through `Entity.Substitute` and projects the `Evaled` reduction through `NumberBox`.
- Auto: the canonical key derives once at construction in `ValidateFactoryArguments` from `CanonicalProjection` so two structurally distinct but algebraically equal builds that reduce to the same normal form share one key and one cache slot; `FreeSymbols` reads the engine's own `Vars` census rather than walking the tree because the engine owns the free-variable traversal; equality and hashing are the `[ComplexValueObject]`-generated members over `ContentKey` ALONE (the `Entity` member carries the constant `EntityInert` `[MemberEqualityComparer]`, so it stays a generated `Create` parameter yet contributes nothing to `Equals`/`GetHashCode`), so a `SymbolicExpr` is a dictionary key, a content-address seed, and a receipt fact with no hand-written `GetHashCode`; `Evaluate` reads the non-throwing `Evaled` reduction (evaluate-as-far-as-possible), never `EvalNumerical()` whose unbound-symbol path throws — an under-bound expression surfaces as a typed `SymbolUndefined` fault at the `NumberBox` seam, not an exception.
- Receipt: `SymbolicExpr` carries no receipt of its own — it is an interior value; the differentiate/compile/evaluate outcomes ride the `lowering` `CompiledExpr` cache receipt and the `optimizer` `Optimization` receipt slot, and a parse/dimension/analytic fault rides the `ComputeFault` rail at the admitting edge.
- Packages: AngouriMath (the `Entity` tree, `Simplify`/`Stringize`/`Latexise`/`Vars`/`Substitute`/`Evaled`), PeterO.Numbers (the exact-number carriers the `Entity.Number` leaves expose), Rasm (project, `Domain.ContentHash` — the one federation content-hash entry), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new canonical-form policy is one change at `CanonicalProjection`, never a parallel key; a new evaluation-result projection is one `NumberBox` arm; the wrapped hierarchy grows only when `AngouriMath` adds a node type, which the dimensional fold's exhaustive walk surfaces as a typed fault, never a silent default.
- Boundary: identity is the `ContentKey`, never the engine — the `[ComplexValueObject]` carries the `Entity` as a stored member but holds it equality-INERT through the constant `EntityInert` `[MemberEqualityComparer]` on the member itself (`[IgnoreMember]` would drop it from the generated `Create` factory; a default comparer on `ContentKey` excludes nothing), so two algebraically-equal builds with distinct raw trees share one identity, and a key derived from `Entity.GetHashCode()` is the deleted form because the engine's hash is per-process randomized while the content key must be durable and cross-process; the key mints through the kernel `ContentHash.Of` federation entry — the durable cross-package identity the Persistence cost-catalog/QTO-formula consumers dedup by, so a per-call-site hasher is the one-hasher-law breach; the canonical projection is `Stringize()` over the `Simplify()`-normalized tree — determinism is source-settled at the 1.4.0 pin (`SortAndGroup` structural-lexicographic operand ordering over pure-recursive `SortHash` strings, no randomized or enumeration-order dependence on the canonical path), and rendering the raw unnormalized tree is the rejected key source; the canonical-string fold binds `ComparerAccessors.StringOrdinal` — a culture-sensitive compare on the identity path is the deleted form; the numeric tower never crosses into the interior — `NumberBox` collapses it at the seam, mapping the `Real` leaf (which subsumes `Rational`/`Integer` by inheritance and carries ±∞ as `Real` specials) through `AsDouble()` and every `Complex`/unbound arm to a `ComputeFault.SymbolUndefined` because the units/optimizer consumers admit only a real scalar; `Entity.TryParse` is the one parse seam and a `MathS.FromString` call in domain flow is the rejected throwing form — the throw-on-malformed path never enters the rail.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------

// --- [MODELS] ----------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct SymbolicExpr {
    // EntityInert holds every Entity pair equal at hash zero, so the generated Equals/GetHashCode
    // discriminate on ContentKey ALONE while Entity stays a stored, Create-constructed member —
    // [IgnoreMember] is the rejected form (it hides the member from the generated factory).
    [MemberEqualityComparer<EntityInert, Entity>]
    public Entity Entity { get; }

    // The key is derived from the entity in ValidateFactoryArguments through the kernel ContentHash.Of
    // federation entry, never trusted from the Create slot and never a per-call-site hasher.
    public UInt128 ContentKey { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Entity entity, ref UInt128 contentKey) =>
        contentKey = ContentHash.Of(MemoryMarshal.AsBytes(CanonicalProjection(entity).AsSpan()));

    public static SymbolicExpr Of(Entity entity) => Create(entity, default);

    // Simplify() is the engine's canonicalization pass — structural-lexicographic operand order,
    // byte-stable across process runs at the pin — and Stringize() renders the settled tree once.
    private static string CanonicalProjection(Entity e) => e.Simplify().Stringize();

    public string Canonical => CanonicalProjection(this.Entity);

    public string Display => this.Entity.Stringize();

    public string LaTeX => this.Entity.Latexise();

    public Seq<string> FreeSymbols =>
        toSeq(this.Entity.Vars)
            .Map(static v => v.Name)
            .Distinct()
            .OrderBy(static name => name, ComparerAccessors.StringOrdinal.Comparer)
            .ToSeq();

    public Fin<double> Evaluate(Map<string, double> bindings) =>
        NumberBox.Project(
            bindings.Fold(this.Entity, static (acc, pair) => acc.Substitute(MathS.Var(pair.Key), pair.Value)).Evaled);

    private sealed class EntityInert : IEqualityComparerAccessor<Entity>, IEqualityComparer<Entity> {
        public static IEqualityComparer<Entity> EqualityComparer { get; } = new EntityInert();
        public bool Equals(Entity? x, Entity? y) => true;
        public int GetHashCode(Entity obj) => 0;
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class NumberBox {
    // Real subsumes Rational/Integer by inheritance and carries ±∞ as Real specials; the
    // non-real Complex leaf and any unbound residue fault typed, never a NaN sentinel.
    public static Fin<double> Project(Entity evaled) =>
        evaled switch {
            Entity.Number.Real real => Fin.Succ(real.AsDouble()),
            Entity.Number.Complex complex => Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<non-real:{complex.Stringize()}>")),
            _ => Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<unbound:{evaled.Stringize()}>")),
        };
}
```

## [03]-[BUILD_ADMISSION]

- Owner: `BuildSpec` `[Union]` the single admission carrier discriminating an infix-string parse (`Infix`) from a structured entity construction (`Structured`) so one `Build` entry serves both inputs by shape; `Build` the one polymorphic entrypoint folding the spec through the generated total `Switch`; `SymbolicFault` the abstract base record nested on `ComputeFault` collecting the symbolic-lane failure class, with the three arms (`ParseRejected` 2213, `SymbolUndefined` 2214, `NonDifferentiable` 2215) extending the 2200 band beside `Text` 2200 … `CacheCorrupt` 2212 — the base every symbolic-lane arm derives from, so `Symbolic/dimensional#DIMENSION_PROOF`'s `DimensionMismatch` (2216) lands as one more `SymbolicFault` arm rather than a second fault family.
- Cases: `BuildSpec` cases `Infix(string Source)` · `Structured(Entity Tree)` (2); `SymbolicFault` arms `ParseRejected` (2213, `Entity.TryParse` declined the source text) · `SymbolUndefined` (2214, an evaluation/substitution reduced to a non-real or unbound leaf, an absent free symbol was named, or a harvest met a non-constant coefficient) · `NonDifferentiable` (2215, the analytic-decline arm — a `Differentiate`/`Integrate`/`Limit` transform left an unresolved `Derivativef`/`Integralf`/`Limitf` residue, or a compile declined), with 2216 the next code `Symbolic/dimensional#DIMENSION_PROOF` claims for `DimensionMismatch`.
- Entry: `public static Fin<SymbolicExpr> Build(BuildSpec spec)` is the one input-shape-dispatched entrypoint over the generated `Switch` — the `Infix` case routes `Entity.TryParse(source, CultureInfo.InvariantCulture, out var parsed)` so a malformed string is a `ParseRejected` fault, never a thrown parse exception, and the `Structured` case wraps a pre-built `Entity` (minted from `MathS.Var`/the `MathS` typed builders/the `Entity` operator algebra) directly through `SymbolicExpr.Of`; `public static Fin<SymbolicExpr> Build(string infix) => Build(new BuildSpec.Infix(infix))` and `public static SymbolicExpr Build(Entity tree) => SymbolicExpr.Of(tree)` are the two shape-typed conveniences over the one fold, never a parallel parser.
- Auto: `Build` reads the non-throwing `Entity.TryParse(string, IFormatProvider, out Entity)` form pinned to `CultureInfo.InvariantCulture` (the parse is culture-independent by construction), never `MathS.FromString` or the implicit `string`→`Entity` conversion, both of which throw on malformed input — the parse failure lands on the `Fin` rail at the boundary; the `Structured` case carries an already-valid engine value, so it skips the parse and only computes the canonical key; the three `SymbolicFault` arms are partial-record extensions on the existing `ComputeFault` `[Union]` declared at `Runtime/admission#DISPATCH_SPINE`, so they share the doctrine `Expected` shape, the dual-tier `Create` contract, and the `IValidationError<ComputeFault>` rail without a second fault family.
- Receipt: a build never lands a receipt — it admits a value or a `ComputeFault`; the downstream compile/optimize outcome carries the receipt, and the admitting fault's code (2213/2214/2215) projects through `FaultDetail` at the wire edge exactly as every other `ComputeFault` does.
- Packages: AngouriMath (`Entity.TryParse` — the ANTLR front-end), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new input shape is one `BuildSpec` case plus one `Build` arm (a LaTeX-source admission would be one `BuildSpec.Latex` case routing a LaTeX parser), never a `BuildFromLatex` sibling; a new symbolic failure mode is one `SymbolicFault` arm on the next-free 2200-band code, never a parallel error type — `Symbolic/dimensional#DIMENSION_PROOF` takes 2216 for `DimensionMismatch`, and 2221+ stays free for the next symbolic failure class.
- Boundary: `Build` is the single entry over the generated total `Switch` — a `Parse`/`FromEntity`/`FromInfix` factory trio modeling one concept is the collapsed defect, and the input shape (`string` vs `Entity`) selects the arm, never the call site; the union `Switch` is exhaustive at compile time so a new `BuildSpec` case breaks loudly rather than falling through a runtime-silent `_`; `Entity.TryParse` is the admitted parse surface and `MathS.FromString`/the implicit `string`→`Entity` conversion are rejected in domain flow because they raise into the rail; the `SymbolicFault` arms extend `ComputeFault` and a standalone `SymbolicError`/`ParseError` union is the rejected parallel rail; the FParsec combinator library is GONE — the engine's ANTLR front-end owns the grammar, and no parser package survives in the folder roster.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public abstract partial record BuildSpec {
    private BuildSpec() { }

    public sealed record Infix(string Source) : BuildSpec;
    public sealed record Structured(Entity Tree) : BuildSpec;
}

// --- [ERRORS] ----------------------------------------------------------------------------
public abstract partial record ComputeFault {
    public abstract partial record SymbolicFault : ComputeFault {
        protected SymbolicFault(string detail, int code) : base(detail, code) { }
    }

    public sealed record ParseRejected : SymbolicFault { public ParseRejected(string detail) : base(detail, 2213) { } }
    public sealed record SymbolUndefined : SymbolicFault { public SymbolUndefined(string detail) : base(detail, 2214) { } }
    public sealed record NonDifferentiable : SymbolicFault { public NonDifferentiable(string detail) : base(detail, 2215) { } }
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicBuild {
    public static Fin<SymbolicExpr> Build(BuildSpec spec) =>
        spec.Switch(
            infix: static i => Entity.TryParse(i.Source, CultureInfo.InvariantCulture, out Entity parsed)
                ? Fin.Succ(SymbolicExpr.Of(parsed))
                : Fin.Fail<SymbolicExpr>(new ComputeFault.ParseRejected(i.Source)),
            structured: static s => Fin.Succ(SymbolicExpr.Of(s.Tree)));

    public static Fin<SymbolicExpr> Build(string infix) => Build(new BuildSpec.Infix(infix));

    public static SymbolicExpr Build(Entity tree) => SymbolicExpr.Of(tree);
}
```

## [04]-[OPERATION_FOLD]

- Owner: `SymbolicOp` `[Union]` the closed operation family over the CAS surface, each case carrying its operand data so one total `Apply` switch dispatches every algebraic transform; `SymbolicForm` `[SmartEnum<string>]` the normalization-route rows (`simplified` · `expanded` · `factorized` · `inner`) folded into one `Simplify` arm; `Apply` the one static fold over `SymbolicOp` returning `Fin<SymbolicExpr>` so a new transform is a case, never a method; `Coefficients` and `Solutions` the sibling typed projections (`Fin<Seq<double>>` and `Fin<Seq<SymbolicExpr>>`), kept off the transform union because a harvest yields a vector, never a `SymbolicExpr`.
- Cases: `SymbolicOp` cases `Differentiate(string Symbol, int Order = 1)` · `Integrate(string Symbol, Option<(SymbolicExpr Lower, SymbolicExpr Upper)> Bounds = default)` (indefinite and definite on ONE case — the bounds' presence selects the engine overload) · `Limit(string Symbol, SymbolicExpr Destination, ApproachFrom Approach = ApproachFrom.BothSides)` · `Solve(string Symbol)` (the solution `Entity.Set` wraps as a set-sorted `SymbolicExpr`) · `Taylor(string Symbol, int Degree, double At)` (degree 1 IS the tangent line — a `TangentLine` sibling is the collapsed form) · `Simplify(SymbolicForm Form)` · `Substitute(Map<string, Entity> Bindings)` · `Approximate(Map<string, double> Bindings)` (8 transform cases, each a `SymbolicExpr → Fin<SymbolicExpr>` arm); `SymbolicForm` rows `simplified` (`Simplify()` — the canonical pass) · `expanded` (`Expand()`) · `factorized` (`Factorize()`) · `inner` (`InnerSimplified` — the cheap non-canonical reduction) (4); the `Coefficients` harvest (dense ascending power vector off `MathS.Utils.TryGetPolynomial`) and the `Solutions` harvest (the enumerated `FiniteSet` roots) are the sibling typed projections, and the `Evaluate`/`Compile`/`Stringize`/`Latexise` terminals are owned at their home clusters.
- Entry: `public static Fin<SymbolicExpr> Apply(SymbolicExpr source, SymbolicOp op)` is the one fold over the eight transform cases — `Differentiate` routes `entity.Differentiate(MathS.Var(name), order)` and gates the result against a surviving `Derivativef` residue (the engine defers what it cannot differentiate as a symbolic derivative node rather than throwing — a residue is the typed 2215 decline, never a silent symbolic leftover reaching a numeric consumer), `Integrate` routes `entity.Integrate(var)` or the definite `entity.Integrate(var, lower, upper)` on the bounds' presence with the same `Integralf` residue gate, `Limit` routes `entity.Limit(var, destination, approach)` gated on `Limitf`, `Solve` routes `entity.Solve(var)` whose typed `Entity.Set` result (a `FiniteSet`, `Interval`, or `ConditionalSet` — never a bare array, so an empty/parametric/conditional solution is a typed value) wraps as a `SymbolicExpr`, `Taylor` routes `MathS.Taylor(entity, degree, (var, at))`, `Simplify` reads the `SymbolicForm` row's delegate, `Substitute` folds `entity.Substitute(MathS.Var(key), value)` over the binding map, and `Approximate` substitutes numeric bindings then reduces through `InnerSimplified` (a partial substitution — unbound symbols survive symbolically) — every arm re-wraps the resulting `Entity` through `SymbolicExpr.Of` so the canonical key recomputes; `public static Fin<Seq<double>> Coefficients(SymbolicExpr source, string symbol)` reads `MathS.Utils.TryGetPolynomial(entity, var, out Dictionary<EInteger, Entity> terms)` into the dense ascending coefficient vector the `Solver/optimizer#OPTIMIZER_LANE` seed consumes (absent powers zero-fill; a non-polynomial or symbolic-coefficient source faults); `public static Fin<Seq<SymbolicExpr>> Solutions(SymbolicExpr source, string symbol)` applies `Solve` and enumerates the `FiniteSet.Elements` roots — the cut-parameter inversion projection — faulting a non-finite (`Interval`/`ConditionalSet`) solution the numeric consumer cannot enumerate.
- Auto: `Apply` is a generated total `Switch` over the `SymbolicOp` `[Union]` in the allocation-free stateful form — the `source` threads as the `state` argument and each `static` case delegate takes `(case, source)`, so a closure is never allocated and an unhandled case is a compile break, never a runtime fall-through; every symbol-naming arm validates the symbol against `FreeSymbols` before the engine call so an absent-symbol transform is a typed `SymbolUndefined` fault rather than a silent zero/no-op; the residue gate (`Derivativef`/`Integralf`/`Limitf` surviving in `result.Nodes`) is ONE shared guard parameterized by the residue type, never three copies; `Simplify` reads the `SymbolicForm` row's delegate so the normalization route is a row value, never a `form switch` cascade; `Approximate` lifts each `double` through the engine's numeric implicit conversion at the substitution seam — the substituted tree stays exact where bindings are absent, the partial-substitution posture the F# predecessor needed a second carrier DU for.
- Receipt: the operation fold lands no receipt of its own; the differentiate output feeds `lowering` (which caches the compiled Jacobian delegate and stamps the `CompiledExpr` cache receipt) and the optimizer adjoint (which stamps the `Optimization` slot), the coefficient vector feeds the `Solver/optimizer#OPTIMIZER_LANE` numeric seed, the solve/integrate rows feed the AEC cost/QTO formula lane (cut-parameter inversion, quantity integrals), and a transform fault rides the `ComputeFault` rail.
- Packages: AngouriMath (`Differentiate`/`Integrate`/`Limit`/`Solve`/`Simplify`/`Expand`/`Factorize`/`InnerSimplified`/`Substitute`, `MathS.Var`/`MathS.Taylor`/`MathS.Utils.TryGetPolynomial`, `ApproachFrom`), PeterO.Numbers (`EInteger` power keys off the polynomial harvest), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new analytic transform is one `SymbolicOp` case plus one `Apply` arm binding its engine member; a new normalization route is one `SymbolicForm` row with its inline delegate; a new projection (e.g. `ToSymPy` for the Python companion) is one terminal beside `Stringize`/`Latexise`; polynomial gcd/partial-fraction algebra has NO engine owner at the pin — a recorded charter deferral, admitted as a vendored row only when a consumer names it, never a hand-rolled polynomial kernel here; zero new surface — a `Differentiator`/`Solver`/`Integrator` sibling family is the collapsed defect folded onto `Apply`.
- Boundary: `Apply` is the one transform surface and a per-operation `Differentiate`/`Simplify`/`Solve` static-method ladder is the rejected form collapsed into the union fold; `Coefficients`/`Solutions` are the collapsed exceptions that do NOT ride `Apply` because their results are vectors, never a `SymbolicExpr` — degenerate echo cases inside `Apply` are the rejected shoehorn; the engine surface is mined whole — `Differentiate` (order-parameterized), `Integrate` (both arities), `Limit` (directed), `Solve`/`SolveEquation` (typed `Set` results), `MathS.Taylor` (multivariate-capable expansion), `Simplify`/`Expand`/`Factorize`/`InnerSimplified`, `Substitute` (node-level), `MathS.Utils.TryGetPolynomial` (the coefficient harvest), `Compile` (delegate lowering, transcribed at `lowering`), and `Stringize`/`Latexise`/`ToSymPy` (projection) — a local finite-difference gradient, a string `eval`, or a hand-rolled root-finder beside `Solve` is the deleted lower-level form; the residue gate is load-bearing — the engine's defer-don't-throw analytic posture means a "successful" transform can carry an unresolved `Derivativef`/`Integralf`/`Limitf` node, and only the gate converts that into the typed 2215 decline; `Solve` returns the typed `Entity.Set` and a first-element grab off an un-discriminated set is the deleted form because an `Interval`/`ConditionalSet` solution is not enumerable — `Solutions` is the one projection that discriminates; the symbol argument constructs through `MathS.Var(name)` (the one `Variable` factory — a `MathS.Variable` spelling is a phantom) and the pi/e constants never enter `FreeSymbols` because `Vars` excludes them by construction.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolicForm {
    public static readonly SymbolicForm Simplified = new("simplified", static e => e.Simplify());
    public static readonly SymbolicForm Expanded = new("expanded", static e => e.Expand());
    public static readonly SymbolicForm Factorized = new("factorized", static e => e.Factorize());
    public static readonly SymbolicForm Inner = new("inner", static e => e.InnerSimplified);

    private readonly Func<Entity, Entity> normalize;

    public Entity Normalize(Entity entity) => normalize(entity);
}

[Union]
public abstract partial record SymbolicOp {
    private SymbolicOp() { }

    public sealed record Differentiate(string Symbol, int Order = 1) : SymbolicOp;
    public sealed record Integrate(string Symbol, Option<(SymbolicExpr Lower, SymbolicExpr Upper)> Bounds = default) : SymbolicOp;
    public sealed record Limit(string Symbol, SymbolicExpr Destination, ApproachFrom Approach = ApproachFrom.BothSides) : SymbolicOp;
    public sealed record Solve(string Symbol) : SymbolicOp;
    public sealed record Taylor(string Symbol, int Degree, double At) : SymbolicOp;
    public sealed record Simplify(SymbolicForm Form) : SymbolicOp;
    public sealed record Substitute(Map<string, Entity> Bindings) : SymbolicOp;
    public sealed record Approximate(Map<string, double> Bindings) : SymbolicOp;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicOps {
    public static Fin<SymbolicExpr> Apply(SymbolicExpr source, SymbolicOp op) =>
        op.Switch(
            source,
            differentiate: static (c, src) => Guard(src, c.Symbol).Bind(sym => Settle<Entity.Derivativef>(src.Entity.Differentiate(sym, c.Order))),
            integrate: static (c, src) => Guard(src, c.Symbol).Bind(sym => Settle<Entity.Integralf>(
                c.Bounds.Match(
                    Some: b => src.Entity.Integrate(sym, b.Lower.Entity, b.Upper.Entity),
                    None: () => src.Entity.Integrate(sym)))),
            limit: static (c, src) => Guard(src, c.Symbol).Bind(sym => Settle<Entity.Limitf>(src.Entity.Limit(sym, c.Destination.Entity, c.Approach))),
            solve: static (c, src) => Guard(src, c.Symbol).Map(sym => SymbolicExpr.Of(src.Entity.Solve(sym))),
            taylor: static (c, src) => Guard(src, c.Symbol).Map(sym => SymbolicExpr.Of(MathS.Taylor(src.Entity, c.Degree, (sym, c.At)))),
            simplify: static (c, src) => Fin.Succ(SymbolicExpr.Of(c.Form.Normalize(src.Entity))),
            substitute: static (c, src) => Fin.Succ(SymbolicExpr.Of(
                c.Bindings.Fold(src.Entity, static (acc, pair) => acc.Substitute(MathS.Var(pair.Key), pair.Value)))),
            approximate: static (c, src) => Fin.Succ(SymbolicExpr.Of(
                c.Bindings.Fold(src.Entity, static (acc, pair) => acc.Substitute(MathS.Var(pair.Key), pair.Value)).InnerSimplified)));

    // Dense ascending coefficient vector off the engine's polynomial harvest: absent powers zero-fill,
    // a symbolic coefficient faults at the projection rather than corrupting the numeric seed.
    public static Fin<Seq<double>> Coefficients(SymbolicExpr source, string symbol) =>
        Guard(source, symbol).Bind(sym =>
            MathS.Utils.TryGetPolynomial(source.Entity, sym, out Dictionary<EInteger, Entity>? terms)
                ? toSeq(Enumerable.Range(0, 1 + terms.Keys.Max(static p => checked((int)p.ToInt32Checked()))))
                    .Traverse(power => terms.TryGetValue(EInteger.FromInt32(power), out Entity? c)
                        ? NumberBox.Project(c.Evaled)
                        : Fin.Succ(0d))
                : Fin.Fail<Seq<double>>(new ComputeFault.SymbolUndefined($"<non-polynomial:{source.Canonical}>")));

    // The cut-parameter inversion projection: only a FiniteSet enumerates; Interval/ConditionalSet
    // solutions are typed declines because a numeric consumer cannot enumerate them.
    public static Fin<Seq<SymbolicExpr>> Solutions(SymbolicExpr source, string symbol) =>
        Apply(source, new SymbolicOp.Solve(symbol)).Bind(static solved =>
            solved.Entity is Entity.Set.FiniteSet finite
                ? Fin.Succ(toSeq(finite.Elements).Map(SymbolicExpr.Of))
                : Fin.Fail<Seq<SymbolicExpr>>(new ComputeFault.SymbolUndefined($"<non-finite-solution:{solved.Canonical}>")));

    static Fin<Entity.Variable> Guard(SymbolicExpr source, string symbol) =>
        source.FreeSymbols.Contains(symbol)
            ? Fin.Succ(MathS.Var(symbol))
            : Fin.Fail<Entity.Variable>(new ComputeFault.SymbolUndefined($"<absent-symbol:{symbol}>"));

    // The engine defers what it cannot solve analytically as a Derivativef/Integralf/Limitf residue
    // node; a surviving residue is the typed 2215 decline, never a symbolic leftover downstream.
    static Fin<SymbolicExpr> Settle<TResidue>(Entity result) where TResidue : Entity =>
        result.Nodes.Any(static n => n is TResidue)
            ? Fin.Fail<SymbolicExpr>(new ComputeFault.NonDifferentiable($"<analytic-decline:{typeof(TResidue).Name}:{result.Stringize()}>"))
            : Fin.Succ(SymbolicExpr.Of(result));
}
```
