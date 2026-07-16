# [SYMBOLIC_DIMENSIONAL]

Pre-numeric dimensional proof for the symbolic CAS arm. A parsed `SymbolicExpr` folds onto a `DimensionMonomial` — one `Seq<ERational>` of seven SI base-dimension exponents, never seven scalar fields — and `DimensionProof` accumulates every compound mismatch on a `Validation<Error,DimensionMonomial>` rail before a single numeric value reaches the optimizer or the cost catalog. Exponents ride the `PeterO.Numbers` `ERational` the engine's `Entity.Number.Rational` leaves carry, never `int`, so a `sqrt` lowering to `Powf(arg, 1/2)` makes a half-power root of an area exactly a length and a float-rounded exponent never decides consistency.

That rational vector is the ℚ⁷ symbolic generalization of the ℤ⁷ integer-exponent `Dimension` the seam `Rasm.Element/Properties/quantity#DIMENSION` carries for measured quantities — both project from the one `UnitsNet` `BaseDimensions` 7-vector and align solely there, never coupled and never re-minted. Onward resolution is not shared: the symbolic side alone resolves a proven monomial to the Compute-internal `QuantityFamily` row, while the lower-stratum seam `Dimension` resolves its quantity through the `UnitsNet` registry directly and never names `QuantityFamily`. Every dimensional failure — a heterogeneous sum, a dimensioned transcendental argument, a non-literal power exponent, an undeclared free symbol, a result monomial naming no admitted `QuantityFamily` — folds onto one `ComputeFault.DimensionMismatch` arm (code 2216 on the `SymbolicFault` family), never a parallel `DimensionError`. Spine: `AngouriMath` (the `Entity` node records and `Vars` census), `PeterO.Numbers` (`ERational`), `UnitsNet` (`BaseDimensions`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

## [01]-[INDEX]

- [01]-[DIMENSION_MONOMIAL]: the seven-exponent SI base-dimension `[ValueObject]` and its generic-math rational-exponent group.
- [02]-[DIMENSION_PROOF]: the exhaustive `Entity`-node fold and the `Validation` accumulating rail over a parsed `SymbolicExpr`.
- [03]-[UNITS_BRIDGE]: the `QuantityFamily` `BaseDimensions` projection, the `DimensionVerdict` candidate set, and the pre-numeric admission gate.

## [02]-[DIMENSION_MONOMIAL]

- Owner: `DimensionMonomial` `[ValueObject]` over a seven-element `ERational` exponent vector (SI base order — length, mass, time, current, temperature, amount, luminous-intensity — as UnitsNet `BaseDimensions` exposes), implementing the `System.Numerics` generic-math group (`IMultiplyOperators`/`IDivisionOperators`/`IMultiplicativeIdentity`) so product, quotient, and scalar-power are the type's own operators; `SiAxis` the axis-index and glyph constant table; `DimensionContext` (in `UNITS_BRIDGE`) the parse-supplied free-symbol binding.
- Cases: one value carries all seven exponents as `ERational`, so a half-power root and a reciprocal both stay exact; `Dimensionless` is the zero vector and the group's multiplicative identity; `Base(index)` mints a unit exponent on one axis.
- Entry: `From(BaseDimensions)` totalizes a UnitsNet vector through `ERational.FromInt32` (each integer axis lifts through the verified factory, never a cast); `Of(params (int Axis, ERational Exponent)[])` sparse-constructs over the zero seed; equality, hashing, and `==` are the generated `[ValueObject]` members over the seven exact exponents, so the monomial is a dictionary key with no hand-written comparer.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject]` generator, structural equality, `ValidateFactoryArguments` hook), PeterO.Numbers (`ERational` `Zero`/`One`/`IsZero`/`FromInt32`/`FromEDecimal` and its `+`/`-`/`*` operators), UnitsNet (`BaseDimensions` axis order and its `.Length`/`.Mass`/`.Time`/`.Current`/`.Temperature`/`.Amount`/`.LuminousIntensity` `int` accessors), LanguageExt.Core (`Seq<ERational>`, the index-first static `Seq.map`, `Zip`/`ForAll`/`Filter`/`Find`, `toSeq`).
- Growth: a new SI axis is impossible (the seven are closed); a new compound relation is one row on `Symbolic/units#DIMENSIONAL_LAW`, never a `DimensionMonomial` change; the exponent type stays `ERational`, so no precision-widening edit is ever needed; a richer diagnostic is one `Format` change.
- Boundary: an interior value that never crosses a wire and never re-mints a `QuantityFamily`. Carrier is a `Seq<ERational>` vector, not seven scalar fields and not the seam's integer `[ComplexValueObject]` `Dimension`, because the algebra is uniform exponent-vector arithmetic whose `Zip`/`ForAll`/`Filter` combinators express the group operators and the render fold directly, and whose structural equality makes it a `FrozenDictionary` key with no comparer; the rational carrier exists because a `Powf` sub-tree carries a transient fractional exponent the integer `Dimension` cannot hold, and a proven monomial with all-integral exponents is exactly that seam `Dimension`. Axis-indexed sparse-construction and render routing take the index-first static `Seq.map(seq, (axis, exponent) => …)` — the instance `.Map` is the unindexed `Func<A,B>` form, so no phantom indexed instance overload exists. Group law lives on the type (`operator *` addition, `operator /` subtraction, `Pow(ERational)` scalar action, `MultiplicativeIdentity` zero vector); a parallel `DimensionAlgebra` static class and a hand-rolled element-wise `Equal` are the collapsed forms. `ERational` compares through `Equals`/`CompareTo` (the type ships no `==` operator), so every equality read spells `Equals`, never a phantom operator. Rank-7 is enforced through the Thinktecture `ValidateFactoryArguments` generator hook — a hand-rolled `static ComputeFault Validate` is not a generator-recognized hook and never runs, so a `Create` over a wrong-length `Seq` is rejected at admission while the rank-7-only interior algebra never trips it. Two display registers stay distinct: the symbolic `Format` physics-glyph projection (`SiAxis.Symbol` L·M·T·I·Θ·N·J) and the seam `Dimension.SiSymbol` SI-unit projection carry no string-equality obligation, both anchored to the same UnitsNet 7-vector in the same axis order. Projection to a named family runs at the `UNITS_BRIDGE` gate, never inside the algebra.

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
    public static readonly string[] Symbol = ["L", "M", "T", "I", "Θ", "N", "J"];
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

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ERational> exponents) =>
        validationError = exponents.Count == SiAxis.Rank
            ? null
            : new ValidationError($"dimension-monomial: rank {exponents.Count} not {SiAxis.Rank}");

    public static DimensionMonomial Base(int axis) => Of((axis, ERational.One));

    public static DimensionMonomial Of(params (int Axis, ERational Exponent)[] terms) =>
        Create(Seq.map(Dimensionless.Value, (axis, zero) =>
            toSeq(terms).Find(t => t.Axis == axis).Map(static t => t.Exponent).IfNone(zero)));

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
        Seq.map(Value, static (axis, exponent) => (Symbol: SiAxis.Symbol[axis], Exponent: exponent))
            .Filter(static t => !t.Exponent.IsZero)
            .Map(static t => t.Exponent.Equals(ERational.One) ? t.Symbol : $"{t.Symbol}^{t.Exponent}") is { IsEmpty: false } factors
                ? string.Join(" ", factors)
                : "1";
}
```

## [03]-[DIMENSION_PROOF]

- Owner: `DimensionProof` the static fold entry and the recursive `Descend`; `DimensionContext` (in `UNITS_BRIDGE`) the free-symbol binding; `ComputeFault.DimensionMismatch` (code 2216) the one arm every failure rides.
- Cases: every `Entity.Number` leaf is `Dimensionless`; `Entity.Variable` reads its declared monomial from the context, a constant leaf (pi/e) discriminated by an empty `Vars` census; `Sumf`/`Minusf` demand identical operand monomials (the canonical defect this fold catches); `Mulf`/`Divf` fold through `*`/`/`; `Powf` demands a numeric-literal exponent and scales through `Pow`, covering `sqrt` as `Powf(arg, 1/2)` with no special case; `Absf` preserves dimension, `Signumf` erases it, `Logf` and every trig/unary `Function` demand dimensionless arguments through `IUnaryNode.NodeChild`; `atan2` needs no arm because the engine spells it `arctan(y/x)` and the homogeneous `Divf` ratio is already dimensionless; the `CalculusOperator` family carries `Derivativef` = `dim(f)/dim(x)`, `Integralf` = `dim(f)·dim(x)`, `Limitf` = `dim(f)`; a `Statement`/`Set`/boolean node short-circuits to the fault.
- Entry: `Prove(SymbolicExpr, DimensionContext)` — one polymorphic entry returning `Validation<Error,DimensionMonomial>`, discriminating on the carried `Entity` case, never a per-case public method; the accumulating rail collects every `Sumf`-mismatch and undeclared symbol across the tree in one pass.
- Packages: AngouriMath (the `Entity` records pattern-matched positionally — `Sumf(Augend,Addend)`, `Minusf(Subtrahend,Minuend)`, `Mulf(Multiplier,Multiplicand)`, `Divf(Dividend,Divisor)`, `Powf(Base,Exponent)`, `Logf(Base,Antilogarithm)`, unary `(Argument)` behind `IUnaryNode.NodeChild`, `Entity.Variable.Name`, `Entity.Number.Rational.ERational`/`Real.EDecimal`, the per-node `Vars` census, `Stringize`), LanguageExt.Core (the accumulating `Validation` applicative, `Traverse`, `Seq`, `Distinct`), PeterO.Numbers (`ERational`), Thinktecture.Runtime.Extensions.
- Growth: the unary-function law covers every transcendental through the `IUnaryNode` floor without a per-name table; a new node family (the engine's hierarchy is closed at the pin) surfaces as the typed unmapped-node fault, never a silent fall-through; zero new entrypoint.
- Boundary: the fold reads `Entity` payloads through positional record patterns, never re-parsing the infix string, so the proof runs once over the canonical tree. Constant discrimination is structural — a `Variable` leaf with an empty `Vars` census is a constant and resolves `Dimensionless`, so no constant-name table exists; every other `Variable` resolves through `DimensionContext.Resolve`, and one absent from the context accumulates as `DimensionMismatch`, never a hidden dimensionless default. A `Powf` exponent admits an exact `Rational` (subsuming `Integer` by inheritance) or a finite `Real` (`x^0.5`, lifted through `ERational.FromEDecimal`); a symbolic exponent has no static scale and accumulates as `DimensionMismatch`. Transcendental arms preserve dimension only for `Absf` and erase only for `Signumf`, so a blanket dimensionless-demanding rule that rejected `abs(force)` or `sign(moment)` is wrong. A foreign-node `_` arm produces a fault, never a silent fall-through. Rail is `Validation<Error,DimensionMonomial>` (the monoidal `Error` carrier every sibling lane uses; `ComputeFault` is not its own monoid, so the typed arm lifts onto `Error` through its `Expected` base), so one ill-formed compound surfaces every constituent mismatch at once. Proof never evaluates a number and never compiles a delegate — it is the gate the `Symbolic/lowering#LOWERING` compile fence runs behind.

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
        Descend(expr.Entity, context);

    static Validation<Error, DimensionMonomial> Descend(Entity node, DimensionContext context) =>
        node switch {
            Entity.Number =>
                Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless),
            // A constant Variable leaf (pi, e) has an empty Vars census — the discriminant that needs no constant-name table.
            Entity.Variable variable =>
                toSeq(variable.Vars).IsEmpty
                    ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                    : context.Resolve(variable.Name),
            Entity.Sumf(var augend, var addend) =>
                Homogeneous(Seq(augend, addend), context),
            Entity.Minusf(var subtrahend, var minuend) =>
                Homogeneous(Seq(subtrahend, minuend), context),
            Entity.Mulf(var multiplier, var multiplicand) =>
                (Descend(multiplier, context), Descend(multiplicand, context)).Apply(static (a, b) => a * b),
            Entity.Divf(var dividend, var divisor) =>
                (Descend(dividend, context), Descend(divisor, context)).Apply(static (a, b) => a / b),
            Entity.Powf(var @base, var exponent) =>
                (Descend(@base, context), Literal(exponent)).Apply(static (b, e) => b.Pow(e)),
            Entity.Absf(var argument) =>
                Descend(argument, context),
            Entity.Signumf(var argument) =>
                Descend(argument, context).Map(static _ => DimensionMonomial.Dimensionless),
            Entity.Logf(var @base, var antilogarithm) =>
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
            Entity.Statement or Entity.Set or Entity.Boolean =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: non-numeric node {node.GetType().Name} in a formula proof")),
            _ =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: unmapped node {node.GetType().Name}")),
        };

    // Dimensionless-demanding args; the accumulating `Traverse` surfaces every dimensioned arg at once.
    static Validation<Error, DimensionMonomial> Dimensionless(Seq<Entity> args, DimensionContext context, string name) =>
        args.Traverse(arg => Descend(arg, context)).Bind(dims =>
            dims.ForAll(static d => d.IsDimensionless)
                ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(
                    $"dimension: {name} requires dimensionless arguments, got {string.Join(", ", dims.Map(static d => d.Format()))}")));

    static Validation<Error, DimensionMonomial> Homogeneous(Seq<Entity> addends, DimensionContext context) =>
        addends.Traverse(addend => Descend(addend, context)).Bind(static dims =>
            dims.Distinct().ToSeq() is { Count: <= 1 } distinct
                ? Success<Error, DimensionMonomial>(distinct.Head.IfNone(DimensionMonomial.Dimensionless))
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(
                    $"dimension: heterogeneous sum over {string.Join(" vs ", distinct.Map(static d => d.Format()))}")));

    static Validation<Error, ERational> Literal(Entity exponent) =>
        exponent switch {
            // Rational subsumes Integer by inheritance; the ERational payload is exact.
            Entity.Number.Rational rational =>
                Success<Error, ERational>(rational.ERational),
            // A finite decimal exponent (`x^0.5`) lifts exactly through FromEDecimal; NaN/∞ fault.
            Entity.Number.Real real when real.EDecimal.IsFinite =>
                Success<Error, ERational>(ERational.FromEDecimal(real.EDecimal)),
            _ =>
                Fail<Error, ERational>(new ComputeFault.DimensionMismatch($"dimension: non-literal power exponent {exponent.Stringize()}")),
        };
}
```

## [04]-[UNITS_BRIDGE]

- Owner: `DimensionContext` the parse-context binding the fold resolves free symbols through; `DimensionVerdict` the typed receipt carrying the proven monomial and its candidate `QuantityFamily` set; `DimensionAdmission` the static projection that censuses, proves, and matches against the `Symbolic/units#DIMENSIONAL_LAW` SI baseline.
- Cases: a proven monomial resolves to the set of `QuantityFamily` rows whose `Info.BaseDimensions` equal it — usually one, but `Energy` and `Torque` share `M·L²·T⁻²` and `Ratio` and `Angle` share the zero vector, so the verdict carries every candidate and `Unique` is `Some` only at exactly one match; a monomial matching no admitted row faults as `DimensionMismatch`, so a formula whose result dimension has no admitted quantity is rejected before numeric admission; bound free symbols arrive from the `UnitProject` parse context, each carrying its declared `QuantityFamily`.
- Entry: `Admit(SymbolicExpr, DimensionContext)` — `Validation<Error,DimensionVerdict>` composing the `FreeSymbols` census, `DimensionProof.Prove`, and the row match; the census fails fast on any undeclared symbol, the proof then accumulates every structural mismatch, and the match names the candidate families; no `IQuantity` is ever constructed, admission running before any value materializes.
- Packages: UnitsNet (the frozen `QuantityFamily.Info.BaseDimensions` rows, never re-minted), LanguageExt.Core (`Validation`, `Map`, `Seq`, `Option`, the census filter), Thinktecture.Runtime.Extensions (`QuantityFamily.Items`, the `DimensionMonomial` dictionary key), AngouriMath (`SymbolicExpr` input, the `FreeSymbols` set driving the census), BCL inbox (`FrozenDictionary`).
- Growth: a new admitted result dimension is one `QuantityFamily` row on `Symbolic/units#QUANTITY_TABLE` — the match table groups `Items` by `DimensionMonomial` at static construction, so a row added there extends admission (or joins an existing dimensionally-equal candidate set) with zero edit here; a richer verdict is one `DimensionVerdict` field; zero new surface.
- Boundary: the single seam between the symbolic arm and the units boundary — it consumes the `Symbolic/units#DIMENSIONAL_LAW` SI vocabulary as the proof target, never re-mints a quantity type, never crosses a wire, and never constructs an `IQuantity`. Match table is a `FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>>` built once by grouping `QuantityFamily.Items` on their `DimensionMonomial` (a valid key because `[ValueObject]` generates structural equality over the exact exponents), so the O(1) lookup replaces a linear scan and the candidate set is the value; a `Find`-first-match silently collapses the `Energy`/`Torque` and `Ratio`/`Angle` non-injectivity and is the deleted form, because dimension → quantity is not a function, so the verdict reports the full candidate set and a consumer needing one family resolves through `Unique`. Census fails fast on an undeclared free symbol before descent, the proof accumulates every mismatch, and the match runs only on a clean proof — fail-fast `Bind` over the accumulating `Prove`. Gate runs strictly before the `Symbolic/units#QUANTITY_TABLE` `Admit` value-conversion entrypoint, so a dimension-inconsistent formula never reaches `Admit`, the optimizer oracle, or the cost catalog; a `UnitProject` intent supplies the `DimensionContext` from its parse, and the `Validation` failure aborts the project before numeric admission.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
public sealed record DimensionContext(Map<string, DimensionMonomial> Bindings) {
    public static DimensionContext Of(Seq<(string Symbol, QuantityFamily Family)> declarations) =>
        new(declarations.Fold(
            Map<string, DimensionMonomial>(),
            static (acc, d) => acc.Add(d.Symbol, DimensionMonomial.From(d.Family.Info.BaseDimensions))));

    public Validation<Error, DimensionMonomial> Resolve(string symbol) =>
        Bindings.Find(symbol).Match(
            Some: static m => Success<Error, DimensionMonomial>(m),
            None: () => Fail<Error, DimensionMonomial>(
                new ComputeFault.DimensionMismatch($"dimension: free symbol '{symbol}' has no declared quantity")));
}

// Dimension → quantity is NOT injective (Energy/Torque both M·L²·T⁻²; Ratio/Angle both zero), so the verdict
// carries every candidate; a consumer needing one resolves through Unique, never a first-match scan.
public sealed record DimensionVerdict(DimensionMonomial Dimension, Seq<QuantityFamily> Families) {
    public bool IsAmbiguous => Families.Count > 1;

    public Option<QuantityFamily> Unique => Families.Count == 1 ? Families.Head : Option<QuantityFamily>.None;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionAdmission {
    static readonly FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>> Table =
        QuantityFamily.Items.ToSeq()
            .GroupBy(static row => DimensionMonomial.From(row.Info.BaseDimensions))
            .ToFrozenDictionary(static g => g.Key, static g => g.ToSeq());

    public static Validation<Error, DimensionVerdict> Admit(SymbolicExpr expr, DimensionContext context) =>
        Census(expr, context).Bind(_ => DimensionProof.Prove(expr, context)).Bind(Match);

    static Validation<Error, Unit> Census(SymbolicExpr expr, DimensionContext context) =>
        expr.FreeSymbols.Filter(symbol => !context.Bindings.ContainsKey(symbol)) is { IsEmpty: false } undeclared
            ? Fail<Error, Unit>(new ComputeFault.DimensionMismatch(
                $"dimension: free symbols [{string.Join(", ", undeclared)}] undeclared in context"))
            : Success<Error, Unit>(unit);

    static Validation<Error, DimensionVerdict> Match(DimensionMonomial monomial) =>
        Table.TryGetValue(monomial, out var families)
            ? Success<Error, DimensionVerdict>(new DimensionVerdict(monomial, families))
            : Fail<Error, DimensionVerdict>(new ComputeFault.DimensionMismatch(
                $"dimension: result {monomial.Format()} names no admitted QuantityFamily"));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [DIMENSION_CONTEXT_SOURCE]-[OPEN]: which `Symbolic/units` `UnitProject` intent field carries the free-symbol-to-`QuantityFamily` declarations the fold reads; widen the existing `UnitProject` parse surface with an additive symbolic-formula case, not a new parse owner.
