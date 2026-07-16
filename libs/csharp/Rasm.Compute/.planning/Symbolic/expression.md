# [COMPUTE_SYMBOLIC_EXPRESSION]

Rasm.Compute symbolic lane head: the closed computer-algebra owner over the admitted `AngouriMath` `Entity` tree, internalized as the `SymbolicExpr` `[ComplexValueObject]` whose identity is the canonical-normal-form content key ALONE. A stored `Entity` member is equality-inert through a constant `EntityInert` `[MemberEqualityComparer]`, so two algebraically-equal builds whose raw trees differ collapse onto one key and one cache slot. Owned here: the value itself, the one polymorphic `Build` entry over the `BuildSpec` `[Union]`, the `SymbolicOp` `[Union]` folding the whole CAS surface through one generated total `Switch`, and the `SymbolicFault` rail extending the `ComputeFault` 2200 band at codes 2213/2214/2215. Canonical form is the `Simplify()` normal form rendered through `Stringize()` — at the pin, commutative operand ordering is structural-lexicographic through `SortAndGroup` over pure-recursive `SortHash` strings, with no `GetHashCode`, `Random`, or dictionary-enumeration dependence, so the key is byte-stable across process runs. Engine crossings are two: `Entity.TryParse(string, IFormatProvider, out Entity)` the non-throwing parse seam (the ANTLR front-end that replaced FParsec), and `NumberBox` collapsing the numeric-tower leaf to `Fin<double>` at the evaluation seam; no engine carrier ever crosses a wire.

That canonical content key is the single identity the lane composes by: `Symbolic/dimensional#DIMENSION_PROOF` walks the same `Entity` records for its SI dimensional fold, and `Symbolic/lowering#LOWERING` keys its `CompiledExpr` cache and `SymbolicJacobian` off the same `XxHash128` over the canonical form — the operation fold and canonical key live here, the compiled-delegate carrier and cache plumbing on `lowering`, the dimension monomial on `dimensional`. `Solver/satisfy#RULE_SATISFACTION` consumes the same tree from the constraint side, an `Entity.Statement` lowering term-by-term to Z3, this CAS the lowering source. Host-local, no TS_PROJECTION cluster: the `SymbolicExpr` is an interior value whose only cross-surface contribution is the content key the `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` seam crosses by reference to the Persistence cost-catalog/QTO-formula consumers, aligned at the key, never coupled. One statement exemption survives — the `TryParse` out-parameter and the `MathS.Utils.TryGetPolynomial` out-dictionary reads project foreign carriers into the rail in language-owned control flow; every other operation is a generated `Switch` fold or a `Bind` chain. Algebraic equivalence between differently-spelled inputs is not identity — the content key binds the canonical form under ordinal comparison. An in-proc symbolic-regression fit is the rejected form: offline formula discovery belongs to the Python branch, never this analytic-algebra owner over an already-known expression.

## [01]-[INDEX]

- [01]-[SYMBOLIC_EXPR]: `SymbolicExpr` `[ComplexValueObject]`; the `Entity` numeric-leaf capsule; canonical key.
- [02]-[BUILD_ADMISSION]: `BuildSpec` `[Union]` parse/structured entry; the `SymbolicFault` 2213–2215 rail.
- [03]-[OPERATION_FOLD]: `SymbolicOp` `[Union]` over the CAS surface; one total `Apply` switch; typed harvests.

## [02]-[SYMBOLIC_EXPR]

- Owner: `SymbolicExpr` `[ComplexValueObject]` over the `AngouriMath` `Entity`, identity-bearing through its canonical `XxHash128` content key (never `Entity.GetHashCode()`, per-process randomized and forbidden as a key source even though `Entity` equality is structural); `NumberBox` the one projector collapsing the evaluated numeric tower to `Fin<double>`; `ComparerAccessors.StringOrdinal` the ordinal accessor the free-symbol set folds and sorts through.
- Cases: the wrapped `Entity` is the engine's closed node hierarchy — numeric-tower leaves (`Integer` ⊂ `Rational` ⊂ `Real` ⊂ `Complex`, exact via the `PeterO.Numbers` carriers the content key depends on), `Entity.Variable` (free symbols `Name`-keyed, pi/e excluded from `Vars`), the arithmetic and function records, the deferred-analytic residues (`Derivativef`/`Integralf`/`Limitf` — the decline sentinels the operation fold gates on), `Entity.Set` (the solve-result carrier), and the statement nodes (`Solver/satisfy` lowers); `NumberBox` collapses the tower to one `Fin<double>` at the `Real` leaf (subsuming `Rational`/`Integer`, carrying ±∞ as `Real` specials) and faults the `Complex`/unbound arms.
- Entry: `SymbolicExpr.Of(Entity)` routes the generated `Create(entity, default)` whose `ValidateFactoryArguments` derives the content key once from the entity (the passed slot is always overwritten, so a forged key cannot desync identity); `CanonicalProjection` is `e.Simplify().Stringize()` — the reduce pass first, because two algebraically equal trees can render distinct strings without it; `ContentKey` mints through the kernel `ContentHash.Of` federation entry over the canonical-form UTF-16 bytes (`MemoryMarshal.AsBytes` over the `ReadOnlySpan<char>`, deterministic across .NET targets, never a UTF-8 transcode, never a per-call-site `XxHash128.HashToUInt128`); `FreeSymbols` folds `Entity.Vars` (constants excluded) into the ordinal-sorted set; `Evaluate(Map<string,double>)` substitutes through `Entity.Substitute` and projects `Evaled` through `NumberBox`.
- Auto: the canonical key derives once at construction, so two algebraically equal builds share one key and one cache slot; equality and hashing are the generated members over `ContentKey` ALONE (the `Entity` member carrying the constant `EntityInert` comparer), so a `SymbolicExpr` is a dictionary key and content-address seed with no hand-written `GetHashCode`; `Evaluate` reads the non-throwing `Evaled` reduction, never `EvalNumerical()` whose unbound-symbol path throws — an under-bound expression surfaces as a typed `SymbolUndefined` at the `NumberBox` seam, not an exception.
- Receipt: none of its own — an interior value; differentiate/compile/evaluate outcomes ride the `lowering` `CompiledExpr` cache receipt and the `optimizer` `Optimization` slot, and a parse/dimension/analytic fault rides the `ComputeFault` rail at the admitting edge.
- Packages: AngouriMath (`Entity`, `Simplify`/`Stringize`/`Latexise`/`Vars`/`Substitute`/`Evaled`), PeterO.Numbers (the exact-number carriers the `Entity.Number` leaves expose), Rasm (project, `Domain.ContentHash` — the one federation content-hash entry), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new canonical-form policy is one `CanonicalProjection` change, never a parallel key; a new evaluation-result projection is one `NumberBox` arm; the wrapped hierarchy grows only when `AngouriMath` adds a node type, which the dimensional fold's exhaustive walk surfaces as a typed fault, never a silent default.
- Boundary: identity is the `ContentKey`, never the engine; the canonical projection is `Stringize()` over the `Simplify()`-normalized tree, so rendering the raw unnormalized tree is the rejected key source; the canonical-string fold binds `ComparerAccessors.StringOrdinal`, so a culture-sensitive compare on the identity path is deleted; minting through the kernel `ContentHash.Of` federation entry is the identity the Persistence cost-catalog/QTO consumers dedup by.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

// --- [MODELS] ----------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct SymbolicExpr {
    // EntityInert holds every Entity pair equal at hash zero, so Equals/GetHashCode discriminate on
    // ContentKey ALONE; [IgnoreMember] is rejected (it hides the member from the generated factory).
    [MemberEqualityComparer<EntityInert, Entity>]
    public Entity Entity { get; }

    // Key derives from the entity here through the kernel ContentHash.Of federation entry, never the Create slot, never a per-call-site hasher.
    public UInt128 ContentKey { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Entity entity, ref UInt128 contentKey) =>
        contentKey = ContentHash.Of(MemoryMarshal.AsBytes(CanonicalProjection(entity).AsSpan()));

    public static SymbolicExpr Of(Entity entity) => Create(entity, default);

    // Simplify() is the canonicalization pass (structural-lexicographic order, byte-stable at the pin); Stringize() renders the settled tree once.
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
    // Real subsumes Rational/Integer and carries ±∞ as Real specials; the Complex leaf and any unbound residue fault typed, never a NaN sentinel.
    public static Fin<double> Project(Entity evaled) =>
        evaled switch {
            Entity.Number.Real real => Fin.Succ(real.AsDouble()),
            Entity.Number.Complex complex => Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<non-real:{complex.Stringize()}>")),
            _ => Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<unbound:{evaled.Stringize()}>")),
        };
}
```

## [03]-[BUILD_ADMISSION]

- Owner: `BuildSpec` `[Union]` discriminating an infix-string parse (`Infix`) from a structured entity construction (`Structured`) so one `Build` entry serves both by shape; `Build` the one polymorphic entrypoint over the generated total `Switch`; `SymbolicFault` the abstract base nested on `ComputeFault`, its three arms (`ParseRejected` 2213, `SymbolUndefined` 2214, `NonDifferentiable` 2215) extending the 2200 band so `dimensional`'s `DimensionMismatch` (2216) lands as one more `SymbolicFault` arm, never a second family.
- Cases: `BuildSpec` — `Infix(string Source)`, `Structured(Entity Tree)`; `SymbolicFault` arms — `ParseRejected` 2213 (`Entity.TryParse` declined), `SymbolUndefined` 2214 (a reduction hit a non-real or unbound leaf, an absent symbol was named, or a harvest met a non-constant coefficient), `NonDifferentiable` 2215 (a transform left an unresolved `Derivativef`/`Integralf`/`Limitf` residue, or a compile declined), with 2216 the next code `dimensional` claims.
- Entry: `Build(BuildSpec)` dispatches on input shape over the generated `Switch` — `Infix` routes `Entity.TryParse(source, CultureInfo.InvariantCulture, out var parsed)` so a malformed string is a `ParseRejected` fault, never a thrown exception, and `Structured` wraps a pre-built `Entity` through `SymbolicExpr.Of`; `Build(string)` and `Build(Entity)` are the two shape-typed conveniences over the one fold, never a parallel parser.
- Auto: `Build` reads the non-throwing `Entity.TryParse` pinned to `InvariantCulture`, never `MathS.FromString` or the implicit `string`→`Entity` conversion (both throw on malformed input); the `Structured` case carries an already-valid value, so it skips the parse and only computes the canonical key; the three `SymbolicFault` arms are partial-record extensions on the existing `ComputeFault` `[Union]`, sharing the `Expected` shape and the dual-tier `Create` contract without a second family.
- Receipt: a build lands no receipt — it admits a value or a `ComputeFault`; the downstream compile/optimize outcome carries the receipt, and the fault's code (2213/2214/2215) projects through `FaultDetail` at the wire edge like every other `ComputeFault`.
- Packages: AngouriMath (`Entity.TryParse` — the ANTLR front-end), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new input shape is one `BuildSpec` case plus one `Build` arm (a LaTeX-source admission is one `BuildSpec.Latex` case routing a LaTeX parser), never a `BuildFromLatex` sibling; a new failure mode is one `SymbolicFault` arm on the next-free 2200-band code, never a parallel error type — `dimensional` takes 2216, and 2221+ stays free for the next symbolic failure class.
- Boundary: `Build` is the single entry — a `Parse`/`FromEntity`/`FromInfix` factory trio modeling one concept is the collapsed defect, and the input shape selects the arm, never the call site; the union `Switch` is exhaustive at compile time, so a new `BuildSpec` case breaks loudly rather than falling through a runtime `_`; `Entity.TryParse` is the admitted parse surface and `MathS.FromString`/the implicit conversion are rejected in domain flow because they raise into the rail; the `SymbolicFault` arms extend `ComputeFault`, so a standalone `SymbolicError`/`ParseError` union is the rejected parallel rail; FParsec is gone — the engine's ANTLR front-end owns the grammar, and no parser package survives in the folder roster.

```csharp signature
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

- Owner: `SymbolicOp` `[Union]` the closed operation family over the CAS surface, each case carrying its operand data so one total `Apply` switch dispatches every transform; `SymbolicForm` `[SmartEnum<string>]` the normalization-route rows folded into one `Simplify` arm; `Apply` the one fold returning `Fin<SymbolicExpr>`; `Coefficients` and `Solutions` the sibling typed projections (`Fin<Seq<double>>`, `Fin<Seq<SymbolicExpr>>`), kept off the transform union because a harvest yields a vector, never a `SymbolicExpr`.
- Cases: `SymbolicOp` — `Differentiate(Symbol, Order=1)`, `Integrate(Symbol, Bounds=default)` (indefinite and definite on ONE case, bounds' presence selecting the engine overload), `Limit(Symbol, Destination, Approach=BothSides)`, `Solve(Symbol)`, `Taylor(Symbol, Degree, At)` (degree 1 IS the tangent line — a `TangentLine` sibling is the collapsed form), `Simplify(Form)`, `Substitute(Bindings)`, `Approximate(Bindings)`; `SymbolicForm` rows `simplified`/`expanded`/`factorized`/`inner`; the `Coefficients` harvest (dense ascending power vector off `MathS.Utils.TryGetPolynomial`) and `Solutions` harvest (enumerated `FiniteSet` roots) are the sibling projections, and `Evaluate`/`Compile`/`Stringize`/`Latexise` are owned at their home clusters.
- Entry: `Apply(SymbolicExpr, SymbolicOp)` folds the eight transform cases, each re-wrapping the resulting `Entity` through `SymbolicExpr.Of` so the canonical key recomputes; `Coefficients(source, symbol)` reads `MathS.Utils.TryGetPolynomial` into the dense ascending vector the optimizer seed consumes (absent powers zero-fill; a non-polynomial or symbolic-coefficient source faults); `Solutions(source, symbol)` applies `Solve` and enumerates `FiniteSet.Elements` — the cut-parameter inversion — faulting a non-finite (`Interval`/`ConditionalSet`) solution a numeric consumer cannot enumerate.
- Auto: `Apply` is a generated total `Switch` in the allocation-free stateful form — `source` threads as `state` and each `static` case delegate takes `(case, source)`, so no closure allocates and an unhandled case is a compile break; every symbol-naming arm validates the symbol against `FreeSymbols` before the engine call, so an absent-symbol transform is a typed `SymbolUndefined`, not a silent no-op; the residue gate (`Derivativef`/`Integralf`/`Limitf` surviving in `result.Nodes`) is ONE shared guard parameterized by residue type, never three copies; `Simplify` reads the `SymbolicForm` row's delegate, so the route is a row value, never a `form switch`; `Approximate` lifts each `double` through the engine's numeric conversion at the substitution seam, the substituted tree staying exact where bindings are absent.
- Receipt: none of its own — the differentiate output feeds `lowering` (compiled-Jacobian cache receipt), the optimizer adjoint (`Optimization` slot), the coefficient vector feeds the optimizer seed, the solve/integrate rows feed the AEC cost/QTO lane, and a transform fault rides the `ComputeFault` rail.
- Packages: AngouriMath (`Differentiate`/`Integrate`/`Limit`/`Solve`/`Simplify`/`Expand`/`Factorize`/`InnerSimplified`/`Substitute`, `MathS.Var`/`MathS.Taylor`/`MathS.Utils.TryGetPolynomial`, `ApproachFrom`), PeterO.Numbers (`EInteger` power keys off the polynomial harvest), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new transform is one `SymbolicOp` case plus one `Apply` arm; a new normalization route is one `SymbolicForm` row with its inline delegate; a new projection (e.g. `ToSymPy`) is one terminal beside `Stringize`/`Latexise`; polynomial gcd/partial-fraction algebra has NO engine owner at the pin — a recorded charter deferral admitted as a vendored row only when a consumer names it, never a hand-rolled kernel here; a `Differentiator`/`Solver`/`Integrator` sibling family is the collapsed defect folded onto `Apply`.
- Boundary: `Apply` is the one transform surface — a per-operation static-method ladder is the rejected form, and `Coefficients`/`Solutions` stay off `Apply` because their results are vectors (a degenerate echo case inside `Apply` is the rejected shoehorn); the engine surface is mined whole (order-parameterized `Differentiate`, both `Integrate` arities, directed `Limit`, typed-`Set` `Solve`, `MathS.Taylor`, `Simplify`/`Expand`/`Factorize`/`InnerSimplified`, `Substitute`, `TryGetPolynomial`, `Compile`, `Stringize`/`Latexise`/`ToSymPy`), so a local finite-difference gradient, a string `eval`, or a hand-rolled root-finder beside `Solve` is the deleted lower-level form; `Solve` returns the typed `Entity.Set` and a first-element grab off an un-discriminated set is deleted because an `Interval`/`ConditionalSet` is not enumerable — `Solutions` is the one projection that discriminates; the symbol argument constructs through `MathS.Var(name)` (a `MathS.Variable` spelling is a phantom), and pi/e never enter `FreeSymbols` because `Vars` excludes them.

```csharp signature
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

    // Dense ascending coefficient vector: absent powers zero-fill; a symbolic coefficient faults rather than corrupting the numeric seed.
    public static Fin<Seq<double>> Coefficients(SymbolicExpr source, string symbol) =>
        Guard(source, symbol).Bind(sym =>
            MathS.Utils.TryGetPolynomial(source.Entity, sym, out Dictionary<EInteger, Entity>? terms)
                ? toSeq(Enumerable.Range(0, 1 + terms.Keys.Max(static p => checked((int)p.ToInt32Checked()))))
                    .Traverse(power => terms.TryGetValue(EInteger.FromInt32(power), out Entity? c)
                        ? NumberBox.Project(c.Evaled)
                        : Fin.Succ(0d))
                : Fin.Fail<Seq<double>>(new ComputeFault.SymbolUndefined($"<non-polynomial:{source.Canonical}>")));

    // Cut-parameter inversion: only a FiniteSet enumerates; Interval/ConditionalSet solutions are typed declines a numeric consumer cannot enumerate.
    public static Fin<Seq<SymbolicExpr>> Solutions(SymbolicExpr source, string symbol) =>
        Apply(source, new SymbolicOp.Solve(symbol)).Bind(static solved =>
            solved.Entity is Entity.Set.FiniteSet finite
                ? Fin.Succ(toSeq(finite.Elements).Map(SymbolicExpr.Of))
                : Fin.Fail<Seq<SymbolicExpr>>(new ComputeFault.SymbolUndefined($"<non-finite-solution:{solved.Canonical}>")));

    static Fin<Entity.Variable> Guard(SymbolicExpr source, string symbol) =>
        source.FreeSymbols.Contains(symbol)
            ? Fin.Succ(MathS.Var(symbol))
            : Fin.Fail<Entity.Variable>(new ComputeFault.SymbolUndefined($"<absent-symbol:{symbol}>"));

    // Engine defers what it cannot solve as a Derivativef/Integralf/Limitf residue; a survivor is the typed 2215 decline, never a symbolic leftover downstream.
    static Fin<SymbolicExpr> Settle<TResidue>(Entity result) where TResidue : Entity =>
        result.Nodes.Any(static n => n is TResidue)
            ? Fin.Fail<SymbolicExpr>(new ComputeFault.NonDifferentiable($"<analytic-decline:{typeof(TResidue).Name}:{result.Stringize()}>"))
            : Fin.Succ(SymbolicExpr.Of(result));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
