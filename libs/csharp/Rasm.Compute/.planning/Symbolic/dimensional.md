# [SYMBOLIC_DIMENSIONAL]

Pre-numeric dimensional proof for the symbolic CAS arm. Every parsed `SymbolicExpr` folds onto a `DimensionMonomial` — one `Seq<ERational>` of seven SI base-dimension exponents, never seven scalar fields — and `DimensionProof` accumulates every compound mismatch on a `Validation<Error,DimensionMonomial>` rail before a single numeric value reaches the optimizer or the cost catalog. Exponents ride the `PeterO.Numbers` `ERational` the engine's `Entity.Number.Rational` leaves carry, never `int`, so a `sqrt` lowering to `Powf(arg, 1/2)` makes a half-power root of an area exactly a length and a float-rounded exponent never decides consistency.

That rational vector is the ℚ⁷ symbolic generalization of the ℤ⁷ integer-exponent `Dimension` the seam `Rasm.Element/Properties/quantity#DIMENSION` carries for measured quantities — both project from the one `UnitsNet` `BaseDimensions` 7-vector and align solely there, never coupled and never re-minted. Onward resolution is not shared: the symbolic side alone resolves a proven monomial to the Compute-internal `QuantityFamily` row, while the lower-stratum seam `Dimension` resolves its quantity through the `UnitsNet` registry directly and never names `QuantityFamily`. Every dimensional failure — a heterogeneous sum, a dimensioned transcendental argument, a non-literal power exponent, an undeclared free symbol, an unresolvable declared family key — folds onto one `ComputeFault.DimensionMismatch` arm (code 2216 on the `SymbolicFault` family), never a parallel `DimensionError`; a sound monomial the admitted roster names no row for is a verdict, not a failure. Spine: `AngouriMath` (the `Entity` node records and `Vars` census), `PeterO.Numbers` (`ERational`), `UnitsNet` (`BaseDimensions`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

## [01]-[INDEX]

- [02]-[DIMENSION_MONOMIAL]: `DimensionMonomial` carries seven SI base-dimension exponents under a generic-math rational group.
- [03]-[DIMENSION_PROOF]: `DimensionProof` folds every `Entity` node onto one accumulating `Validation` rail.
- [04]-[UNITS_BRIDGE]: `QuantityFamily` projects `BaseDimensions` into the named-or-unnamed `DimensionVerdict` the pre-numeric admission gate reads, over the intent-declared symbol bindings.

## [02]-[DIMENSION_MONOMIAL]

- Owner: `DimensionMonomial` `[ValueObject]` over a seven-element `ERational` exponent vector (SI base order — length, mass, time, current, temperature, amount, luminous-intensity — as UnitsNet `BaseDimensions` exposes), implementing the `System.Numerics` generic-math group (`IMultiplyOperators`/`IDivisionOperators`/`IMultiplicativeIdentity`) so product, quotient, and scalar-power are the type's own operators; `SiAxis` the axis-index and glyph constant table; `DimensionContext` (in `UNITS_BRIDGE`) the parse-supplied free-symbol binding.
- Cases: one value carries all seven exponents as `ERational`, so a half-power root and a reciprocal both stay exact; `Dimensionless` is the zero vector and the group's multiplicative identity; `Base(index)` mints a unit exponent on one axis.
- Law: the monomial is CANONICAL BY CONSTRUCTION. `ERational` equality is numerator/denominator-exact and its arithmetic never reduces, so `*`, `/`, and `Pow` alone leave `2/4` and `1/2` as distinct keys for one dimension; the factory hook reduces every exponent to lowest terms at the single mint, so equality, hashing, and the `FrozenDictionary` family lookup all read one representative per physical dimension and no operator needs a reduction of its own.
- Entry: `From(BaseDimensions)` totalizes a UnitsNet vector through `ERational.FromInt32` (each integer axis lifts through the verified factory, never a cast); `Of(params (int Axis, ERational Exponent)[])` sparse-constructs over the zero seed; equality, hashing, and `==` are the generated `[ValueObject]` members over the seven exact exponents, so the monomial is a dictionary key with no hand-written comparer.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject]` generator, structural equality, `ValidateFactoryArguments` hook), PeterO.Numbers (`ERational` `Zero`/`One`/`IsZero`/`FromInt32`/`FromEDecimal` and its `+`/`-`/`*` operators), UnitsNet (`BaseDimensions` axis order and its `.Length`/`.Mass`/`.Time`/`.Current`/`.Temperature`/`.Amount`/`.LuminousIntensity` `int` accessors), LanguageExt.Core (`Seq<ERational>`, the indexed instance `Map((value, index) => …)`, `Zip`/`ForAll`/`Filter`/`Find`, `toSeq`).
- Growth: a new SI axis is impossible (the seven are closed); a new compound relation is one row on `Symbolic/units#DIMENSIONAL_LAW`, never a `DimensionMonomial` change; the exponent type stays `ERational`, so no precision-widening edit is ever needed; a richer diagnostic is one `Format` change.
- Boundary: an interior value that never crosses a wire and never re-mints a `QuantityFamily`. Carrier is a `Seq<ERational>` vector, not seven scalar fields and not the seam's integer `[ComplexValueObject]` `Dimension`, because the algebra is uniform exponent-vector arithmetic whose `Zip`/`ForAll`/`Filter` combinators express the group operators and the render fold directly, and whose structural equality makes it a `FrozenDictionary` key with no comparer; the rational carrier exists because a `Powf` sub-tree carries a transient fractional exponent the integer `Dimension` cannot hold, and a proven monomial with all-integral exponents is exactly that seam `Dimension`. Render routing takes the indexed instance `Value.Map((exponent, axis) => …)` — `(value, index)` argument order; the module `Seq.map` twin transposes to `(index, value)` and is the rejected spelling where the instance form composes; sparse construction writes THROUGH a guarded axis slot (`ArgumentOutOfRangeException.ThrowIfNegative`/`ThrowIfGreaterThanOrEqual` against `SiAxis.Rank`), because the deleted `Find`-filter form silently dropped an out-of-range axis and minted `Dimensionless` from invalid input. Group law lives on the type (`operator *` addition, `operator /` subtraction, `Pow(ERational)` scalar action, `MultiplicativeIdentity` zero vector); a parallel `DimensionAlgebra` static class and a hand-rolled element-wise `Equal` are the collapsed forms. `ERational` compares through `Equals`/`CompareTo` (the type ships no `==` operator), so every equality read spells `Equals`, never a phantom operator. Rank-7 is enforced through the Thinktecture `ValidateFactoryArguments` generator hook — a hand-rolled `static ComputeFault Validate` is not a generator-recognized hook and never runs, so a `Create` over a wrong-length `Seq` is rejected at admission while the rank-7-only interior algebra never trips it. Two display registers stay distinct: the symbolic `Format` physics-glyph projection (`SiAxis.Symbol` L·M·T·I·Θ·N·J) and the seam `Dimension.SiSymbol` SI-unit projection carry no string-equality obligation, both anchored to the same UnitsNet 7-vector in the same axis order. Projection to a named family runs at the `UNITS_BRIDGE` gate, never inside the algebra.

```csharp signature
// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class SiAxis {
    public const int Length = 0;
    public const int Mass = 1;
    public const int Time = 2;
    public const int Current = 3;
    public const int Temperature = 4;
    public const int Amount = 5;
    public const int LuminousIntensity = 6;
    public const int Rank = 7;

    // Dimension glyphs (L M T I Θ N J) indexed by axis; a fault renders "M L^2 T^-2", not a raw exponent array.
    // Immutable by carrier, not by convention — a `string[]` publishes every element as a writable slot, and one
    // stray write there re-labels every fault message in the process.
    public static readonly ImmutableArray<string> Symbol = ["L", "M", "T", "I", "Θ", "N", "J"];
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Seq<ERational>>]
public readonly partial struct DimensionMonomial :
    IMultiplyOperators<DimensionMonomial, DimensionMonomial, DimensionMonomial>,
    IDivisionOperators<DimensionMonomial, DimensionMonomial, DimensionMonomial>,
    IMultiplicativeIdentity<DimensionMonomial, DimensionMonomial> {
    public static readonly DimensionMonomial Dimensionless = Create(Seq(
        ERational.Zero, ERational.Zero, ERational.Zero, ERational.Zero,
        ERational.Zero, ERational.Zero, ERational.Zero));

    // Group identity of the monomial product — the value the domain reads as `Dimensionless`.
    public static DimensionMonomial MultiplicativeIdentity => Dimensionless;

    // Rank gate then CANONICALIZATION, both at the one mint. `ERational.Equals` is numerator/denominator-exact —
    // 1/2 and 2/4 are distinct values — and no arithmetic operator reduces, so two monomials reached by different
    // operation orders would key two dictionary slots for one physical dimension. Lowest terms here makes the
    // canonical form a construction property every operator, `Of`, and `From` inherits for free.
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ERational> exponents) {
        if (exponents.Count != SiAxis.Rank) {
            validationError = new ValidationError($"dimension-monomial: rank {exponents.Count} not {SiAxis.Rank}");
            return;
        }

        exponents = exponents.Map(static e => e.ToLowestTerms());
    }

    // Range guards make every sparse term write through a valid axis slot; invalid programmer-constant axes
    // throw instead of silently minting `Dimensionless`.
    public static DimensionMonomial Base(int axis) => Of((axis, ERational.One));

    public static DimensionMonomial Of(params (int Axis, ERational Exponent)[] terms) {
        ERational[] slots = new ERational[SiAxis.Rank];
        Array.Fill(slots, ERational.Zero);
        foreach ((int axis, ERational exponent) in terms) {
            ArgumentOutOfRangeException.ThrowIfNegative(axis);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(axis, SiAxis.Rank);
            slots[axis] += exponent;
        }

        return Create(toSeq(slots));
    }

    public static DimensionMonomial From(BaseDimensions dims) =>
        Create(Seq(
            ERational.FromInt32(dims.Length),
            ERational.FromInt32(dims.Mass),
            ERational.FromInt32(dims.Time),
            ERational.FromInt32(dims.Current),
            ERational.FromInt32(dims.Temperature),
            ERational.FromInt32(dims.Amount),
            ERational.FromInt32(dims.LuminousIntensity)));

    public ERational this[int axis] => Value[axis];

    public bool IsDimensionless => Value.ForAll(static e => e.IsZero);

    // Free-Abelian-group algebra over ℚ⁷: product adds exponent vectors, quotient subtracts, `Pow` scales (area^(1/2) is a length).
    public static DimensionMonomial operator *(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.Value.Zip(right.Value, static (a, b) => a + b));

    public static DimensionMonomial operator /(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.Value.Zip(right.Value, static (a, b) => a - b));

    public DimensionMonomial Pow(ERational exponent) =>
        Create(Value.Map(e => e * exponent));

    // Physics-notation projection a fault reports ("1" dimensionless, else "M L^2 T^-2").
    public string Format() =>
        Value.Map(static (exponent, axis) => (Symbol: SiAxis.Symbol[axis], Exponent: exponent))
            .Filter(static t => !t.Exponent.IsZero)
            .Map(static t => t.Exponent.Equals(ERational.One) ? t.Symbol : $"{t.Symbol}^{t.Exponent}") is { IsEmpty: false } factors
                ? string.Join(" ", factors)
                : "1";
}
```

## [03]-[DIMENSION_PROOF]

- Owner: `DimensionProof` the static fold entry and the recursive `Descend`; `DimensionContext` (in `UNITS_BRIDGE`) the free-symbol binding; `ComputeFault.DimensionMismatch` (code 2216) the one arm every failure rides.
- Cases: every `Entity.Number` leaf is `Dimensionless`; `Entity.Variable` reads its declared monomial from the context, a constant leaf (pi/e) discriminated by an empty `Vars` census; `Sumf`/`Minusf` demand identical operand monomials (the canonical defect this fold catches); `Mulf`/`Divf` fold through `*`/`/`; `Powf` demands a numeric-literal exponent and scales through `Pow`, covering `sqrt` as `Powf(arg, 1/2)` with no special case; `Absf` preserves dimension, `Signumf` erases it, `Logf` and every trig/unary `Function` demand dimensionless arguments through `IUnaryNode.NodeChild`; `atan2` needs no arm because the engine spells it `arctan(y/x)` and the homogeneous `Divf` ratio is already dimensionless; the `CalculusOperator` family carries `Derivativef` = `dim(f)/dim(x)`, `Integralf` = `dim(f)·dim(x)`, `Limitf` = `dim(f)`; the regime-switch family proves structurally — `Providedf(Expression, Predicate)` is `dim(Expression)` under a proven predicate, `Piecewise` is the `Homogeneous` fold of its case expressions (one dimension across every branch, the design-code piecewise law), and a predicate proves through the `ComparisonSign` arms (`Equalsf`/`Greaterf`/`GreaterOrEqualf`/`Lessf`/`LessOrEqualf` demand homogeneous operands, `Andf`/`Orf`/`Xorf`/`Impliesf`/`Notf` recurse) — so a slenderness-regime or spectrum-branch formula is provable end to end; any other `Statement`/`Set`/boolean node in a VALUE position short-circuits to the fault.
- Entry: `Prove(SymbolicExpr, DimensionContext)` — one polymorphic entry returning `Validation<Error,DimensionMonomial>`, discriminating on the carried `Entity` case, never a per-case public method; the accumulating rail collects every `Sumf`-mismatch and undeclared symbol across the tree in one pass.
- Packages: AngouriMath (the `Entity` records pattern-matched positionally — `Sumf(Augend,Addend)`, `Minusf(Subtrahend,Minuend)`, `Mulf(Multiplier,Multiplicand)`, `Divf(Dividend,Divisor)`, `Powf(Base,Exponent)`, `Logf(Base,Antilogarithm)`, unary `(Argument)` behind `IUnaryNode.NodeChild`, `Entity.Variable.Name`, `Entity.Number.Rational.ERational`/`Real.EDecimal`, the per-node `Vars` census, `Stringize`), LanguageExt.Core (the accumulating `Validation` applicative, `Traverse`, `Seq`, `Distinct`), PeterO.Numbers (`ERational`), Thinktecture.Runtime.Extensions.
- Growth: the unary-function law covers every transcendental through the `IUnaryNode` floor without a per-name table; a new node family (the engine's hierarchy is closed at the pin) surfaces as the typed unmapped-node fault, never a silent fall-through; zero new entrypoint.
- Boundary: the fold reads `Entity` payloads through positional record patterns, never re-parsing the infix string, so the proof runs once over the canonical tree. Constant discrimination is structural — a `Variable` leaf with an empty `Vars` census is a constant and resolves `Dimensionless`, so no constant-name table exists; every other `Variable` resolves through `DimensionContext.Resolve`, and one absent from the context accumulates as `DimensionMismatch`, never a hidden dimensionless default. Every `Powf` exponent admits an exact `Rational` (subsuming `Integer` by inheritance) or a finite `Real` (`x^0.5`, lifted through `ERational.FromEDecimal`); a symbolic exponent has no static scale and accumulates as `DimensionMismatch`. Transcendental arms preserve dimension only for `Absf` and erase only for `Signumf`, so a blanket dimensionless-demanding rule that rejected `abs(force)` or `sign(moment)` is wrong. Foreign-node `_` arms produce a fault, never a silent fall-through. Rail is `Validation<Error,DimensionMonomial>` (the monoidal `Error` carrier every sibling lane uses; `ComputeFault` is not its own monoid, so the typed arm lifts onto `Error` through its `Expected` base), so one ill-formed compound surfaces every constituent mismatch at once. Proof never evaluates a number and never compiles a delegate — it is the gate the `Symbolic/lowering#LOWERING` compile fence runs behind.

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------
public abstract partial record ComputeFault {
    public sealed record DimensionMismatch : SymbolicFault {
        public DimensionMismatch(string detail) : base(detail, 2216) { }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionProof {
    public static Validation<Error, DimensionMonomial> Prove(SymbolicExpr expr, DimensionContext context) =>
        expr.Entity is not null && context is not null
            ? Descend(expr.Entity, context)
            : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: null expression or context"));

    static Validation<Error, DimensionMonomial> Descend(Entity node, DimensionContext context) =>
        node switch {
            null =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: null node")),
            Entity.Number =>
                Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless),
            // Constant Variable leaves (pi, e) carry an empty Vars census — the discriminant needing no constant-name table.
            Entity.Variable variable =>
                toSeq(variable.Vars).IsEmpty
                    ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                    : context.Resolve(variable.Name),
            Entity.Sumf(Entity augend, Entity addend) =>
                Homogeneous(Seq(augend, addend), context),
            Entity.Minusf(Entity subtrahend, Entity minuend) =>
                Homogeneous(Seq(subtrahend, minuend), context),
            Entity.Mulf(Entity multiplier, Entity multiplicand) =>
                (Descend(multiplier, context), Descend(multiplicand, context)).Apply(static (a, b) => a * b),
            Entity.Divf(Entity dividend, Entity divisor) =>
                (Descend(dividend, context), Descend(divisor, context)).Apply(static (a, b) => a / b),
            Entity.Powf(Entity @base, Entity exponent) =>
                (Descend(@base, context), Literal(exponent)).Apply(static (b, e) => b.Pow(e)),
            Entity.Absf(Entity argument) =>
                Descend(argument, context),
            Entity.Signumf(Entity argument) =>
                Descend(argument, context).Map(static _ => DimensionMonomial.Dimensionless),
            Entity.Logf(Entity @base, Entity antilogarithm) =>
                Dimensionless(Seq(@base, antilogarithm), context, "log"),
            // dim(f)/dim(x), dim(f)·dim(x), dim(f) — the calculus-residue dimensional laws.
            Entity.Derivativef derivative =>
                (Descend(derivative.Expression, context), Descend(derivative.Var, context)).Apply(static (f, x) => f / x),
            Entity.Integralf integral =>
                (Descend(integral.Expression, context), Descend(integral.Var, context)).Apply(static (f, x) => f * x),
            Entity.Limitf limit =>
                Descend(limit.Expression, context),
            // IUnaryNode floor covers the whole trig/transcendental family in one arm.
            Entity.Function and IUnaryNode unary =>
                Dimensionless(Seq(unary.NodeChild), context, node.GetType().Name),
            // `Piecewise` folds homogeneous case expressions; each `Providedf` predicate proves comparison
            // operands homogeneous, making regime formulas dimension-provable.
            Entity.Providedf(Entity expression, Entity predicate) =>
                Predicate(predicate, context).Bind(_ => Descend(expression, context)),
            Entity.Piecewise piecewise when !toSeq(piecewise.Cases).IsEmpty =>
                toSeq(piecewise.Cases).Traverse(c => Predicate(c.Predicate, context)).Bind(_ =>
                    Homogeneous(toSeq(piecewise.Cases).Map(static c => c.Expression), context)).As(),
            Entity.Piecewise =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: empty piecewise has no result dimension")),
            Entity.Statement or Entity.Set or Entity.Boolean =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: non-numeric node {node.GetType().Name} in a formula proof")),
            _ =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: unmapped node {node.GetType().Name}")),
        };

    // Comparison predicates require equal operand monomials; boolean connectives recurse into comparison
    // leaves, and every unrecognized predicate faults.
    static Validation<Error, DimensionMonomial> Predicate(Entity predicate, DimensionContext context) =>
        predicate switch {
            null =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: null predicate")),
            Entity.Equalsf(Entity l, Entity r) or Entity.Greaterf(Entity l, Entity r) or Entity.GreaterOrEqualf(Entity l, Entity r)
                or Entity.Lessf(Entity l, Entity r) or Entity.LessOrEqualf(Entity l, Entity r) =>
                Homogeneous(Seq(l, r), context),
            Entity.Boolean =>
                Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless),
            IBinaryNode connective and (Entity.Andf or Entity.Orf or Entity.Impliesf or Entity.Xorf) =>
                (Predicate(connective.NodeFirstChild, context), Predicate(connective.NodeSecondChild, context))
                    .Apply(static (_, _) => DimensionMonomial.Dimensionless),
            Entity.Notf(Entity inner) =>
                Predicate(inner, context),
            _ =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: unprovable predicate {predicate.GetType().Name}")),
        };

    // Dimensionless-demanding args; the accumulating `Traverse` surfaces every dimensioned arg at once.
    static Validation<Error, DimensionMonomial> Dimensionless(Seq<Entity> args, DimensionContext context, string name) =>
        args.Traverse(arg => Descend(arg, context)).Bind(dims =>
            dims.ForAll(static d => d.IsDimensionless)
                ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(
                    $"dimension: {name} requires dimensionless arguments, got {string.Join(", ", dims.Map(static d => d.Format()))}"))).As();

    static Validation<Error, DimensionMonomial> Homogeneous(Seq<Entity> addends, DimensionContext context) =>
        addends.Traverse(addend => Descend(addend, context)).Bind(static dims =>
            dims.Distinct() is { Count: <= 1 } distinct
                ? Success<Error, DimensionMonomial>(distinct.Head.IfNone(DimensionMonomial.Dimensionless))
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(
                    $"dimension: heterogeneous sum over {string.Join(" vs ", distinct.Map(static d => d.Format()))}"))).As();

    static Validation<Error, ERational> Literal(Entity exponent) =>
        exponent switch {
            null =>
                Fail<Error, ERational>(new ComputeFault.DimensionMismatch("dimension: null power exponent")),
            // Rational subsumes Integer by inheritance; the ERational payload is exact.
            Entity.Number.Rational rational =>
                Success<Error, ERational>(rational.ERational),
            // Finite decimal exponents (`x^0.5`) lift exactly through FromEDecimal; NaN/∞ fault.
            Entity.Number.Real real when real.EDecimal.IsFinite =>
                Success<Error, ERational>(ERational.FromEDecimal(real.EDecimal)),
            _ =>
                Fail<Error, ERational>(new ComputeFault.DimensionMismatch($"dimension: non-literal power exponent {exponent.Stringize()}")),
        };
}
```

## [04]-[UNITS_BRIDGE]

- Owner: `DimensionContext` the parse-context binding the fold resolves free symbols through; `DimensionVerdict` the `[Union]` receipt carrying the proven monomial and, where the roster names it, its candidate `QuantityFamily` set; `DimensionAdmission` the static projection that censuses, proves, and matches against the `Symbolic/units#DIMENSIONAL_LAW` SI baseline.
- Cases: `DimensionVerdict` cases `Named(Dimension, Families)` — the roster carries one or more rows at that dimension — and `Unnamed(Dimension)` — the formula is dimensionally sound and the admitted roster names nothing at that dimension; bound free symbols arrive from the `Runtime/admission#DISPATCH_SPINE` `ComputeIntent.SymbolicProject.Dimensions` map, each carrying its declared `QuantityFamily` KEY.
- Law: dimensional soundness and quantity NAMING are two questions, and only the first is the proof's. Curvature reciprocal-length and per-length stiffness are sound intermediates the admitted roster carries no row for, so the verdict reports `Unnamed` with its proven monomial and downstream admission decides whether an unnamed result is admissible for its own consumer; faulting there rejected formulas whose algebra was never in doubt.
- Law: the dimension-to-family map is NOT injective, and the `GroupBy` table detects EVERY collision at static construction — a monomial with two or more rows arrives as one candidate `Seq`, and `Unique` is `Some` only at exactly one. Enumerating the colliding pairs in prose goes stale the moment a `QuantityFamily` row lands beside an existing dimension, so the table is the roster of collisions and the prose states only that they are preserved.
- Entry: `DimensionContext.Of(Map<string, DimensionMonomial>)` builds the binding from the intent's symbol-to-family-key map through `QuantityFamily.TryGet`, faulting `<symbolic-family-unknown:{symbol}={key}>` on every unresolvable key at once; `Admit(SymbolicExpr, DimensionContext)` — `Validation<Error,DimensionVerdict>` composing the `FreeSymbols` census, `DimensionProof.Prove`, and the row match; the census fails fast on any undeclared symbol, the proof then accumulates every structural mismatch, and the match names the candidate families or reports the dimension unnamed; no `IQuantity` is ever constructed, admission running before any value materializes.
- Packages: UnitsNet (the frozen `QuantityFamily.Info.BaseDimensions` rows, never re-minted), LanguageExt.Core (`Validation`, `Traverse`, `Map`, `Seq`, `Option`, the census filter), Thinktecture.Runtime.Extensions (`QuantityFamily.Items`/`TryGet`, the `DimensionMonomial` dictionary key), AngouriMath (`SymbolicExpr` input, the `FreeSymbols` set driving the census), BCL inbox (`FrozenDictionary`).
- Growth: a new admitted result dimension is one `QuantityFamily` row on `Symbolic/units#QUANTITY_TABLE` — the match table groups `Items` by `DimensionMonomial` at static construction, so a row added there turns an `Unnamed` verdict into a `Named` one (or joins an existing candidate set) with zero edit here; a richer verdict is one field on the owning case; zero new surface.
- Boundary: symbolic admission consumes the declared SI `QuantityFamily` vocabulary without constructing `IQuantity`. Declarations arrive on the intent's `Map<string, string>`, so symbol uniqueness is a property of that carrier rather than a duplicate-detection fold, and a family key the roster does not carry is the ONE resolution fault — the context never falls back to a dimensionless default for an unresolvable symbol. `FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>>` preserves the non-injective match whole; full-roster UnitsNet discovery and first-match scans are rejected. Symbol census fails before the accumulating proof, and numeric admission runs only after a clean verdict.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public sealed record DimensionContext(Map<string, DimensionMonomial> Bindings) {
    // Declarations arrive as the intent's symbol-to-family-KEY map: the carrier already enforces one declaration
    // per symbol, so the fold owes only key resolution, and the accumulating `Traverse` names every unresolvable
    // family at once rather than one per round trip.
    public static Validation<Error, DimensionContext> Of(Map<string, string> declarations) =>
        toSeq(declarations.AsIterable())
            .Traverse(static pair =>
                string.IsNullOrWhiteSpace(pair.Key) || !QuantityFamily.TryGet(pair.Value, out QuantityFamily? family)
                    ? Fail<Error, (string Symbol, DimensionMonomial Dimension)>(
                        new ComputeFault.DimensionMismatch($"<symbolic-family-unknown:{pair.Key}={pair.Value}>"))
                    : Success<Error, (string Symbol, DimensionMonomial Dimension)>(
                        (pair.Key, DimensionMonomial.From(family.Info.BaseDimensions))))
            .Map(static rows => new DimensionContext(rows.Fold(
                Map<string, DimensionMonomial>(),
                static (bindings, row) => bindings.Add(row.Symbol, row.Dimension))))
            .As();

    public Validation<Error, DimensionMonomial> Resolve(string symbol) =>
        string.IsNullOrWhiteSpace(symbol)
            ? Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: blank symbol"))
            : Bindings.Find(symbol).Match(
            Some: static m => Success<Error, DimensionMonomial>(m),
            None: () => Fail<Error, DimensionMonomial>(
                new ComputeFault.DimensionMismatch($"dimension: free symbol '{symbol}' has no declared quantity")));
}

// Soundness and naming are separate verdicts: the algebra proves either way, and only the roster decides whether
// its proven dimension has a name. `Monomial` names the base projection because a base member sharing a case
// positional parameter's name suppresses that case's property synthesis outright.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionVerdict {
    private DimensionVerdict() { }

    public sealed record Named(DimensionMonomial Dimension, Seq<QuantityFamily> Families) : DimensionVerdict;

    public sealed record Unnamed(DimensionMonomial Dimension) : DimensionVerdict;

    public DimensionMonomial Monomial => Switch(
        named: static verdict => verdict.Dimension,
        unnamed: static verdict => verdict.Dimension);

    public bool IsAmbiguous => Switch(
        named: static verdict => verdict.Families.Count > 1,
        unnamed: static _ => false);

    public Option<QuantityFamily> Unique => Switch(
        named: static verdict => verdict.Families.Count == 1 ? verdict.Families.Head : Option<QuantityFamily>.None,
        unnamed: static _ => Option<QuantityFamily>.None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionAdmission {
    // Grouping IS the collision census: every dimension shared by two or more rows arrives as one candidate
    // sequence at static construction, so no enumerated pair roster exists to fall behind the table.
    static readonly FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>> Table =
        QuantityFamily.Items
            .GroupBy(static row => DimensionMonomial.From(row.Info.BaseDimensions))
            .ToFrozenDictionary(static g => g.Key, static g => toSeq(g));

    public static Validation<Error, DimensionVerdict> Admit(SymbolicExpr expr, DimensionContext context) =>
        expr.Entity is not null && context is not null
            ? Census(expr, context).Bind(_ => DimensionProof.Prove(expr, context)).Bind(Match)
            : Fail<Error, DimensionVerdict>(new ComputeFault.DimensionMismatch("dimension: null expression or context"));

    static Validation<Error, Unit> Census(SymbolicExpr expr, DimensionContext context) =>
        expr.FreeSymbols.Filter(symbol => !context.Bindings.ContainsKey(symbol)) is { IsEmpty: false } undeclared
            ? Fail<Error, Unit>(new ComputeFault.DimensionMismatch(
                $"dimension: free symbols [{string.Join(", ", undeclared)}] undeclared in context"))
            : Success<Error, Unit>(unit);

    // Proven monomials the roster names nothing for are SOUND and unnamed, never failures: the algebra held and
    // only the vocabulary is silent, so the consumer that needs a name is the one entitled to refuse.
    static Validation<Error, DimensionVerdict> Match(DimensionMonomial monomial) =>
        Success<Error, DimensionVerdict>(Table.TryGetValue(monomial, out Seq<QuantityFamily> families)
            ? new DimensionVerdict.Named(monomial, families)
            : new DimensionVerdict.Unnamed(monomial));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
