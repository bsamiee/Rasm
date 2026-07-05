# [SYMBOLIC_DIMENSIONAL]

The pre-numeric dimensional proof for the symbolic CAS arm. A parsed `SymbolicExpr` (the `AngouriMath` `Entity` tree wrapped by `Symbolic/expression#SYMBOLIC_EXPR`) folds onto a `DimensionMonomial` — one `Seq<ERational>` of seven SI base-dimension exponents, never seven scalar fields — and `DimensionProof` accumulates every compound mismatch on the `Validation<Error,DimensionMonomial>` rail before a single numeric value reaches the optimizer or the cost catalog. The exponent carrier is the `PeterO.Numbers` `ERational` (the same exact-rational arithmetic the engine's `Entity.Number.Rational` leaves carry), not `int`: a `sqrt` lowers to `Powf(arg, 1/2)` and a reciprocal to a negative exponent, so a half-power root of an area is exactly a length and a float-rounded exponent never decides consistency. That rational vector is the symbolic generalization of the integer-exponent `Dimension` the seam `Rasm.Element/Properties/quantity#DIMENSION` carries for measured quantities — both project from the one `UnitsNet` `BaseDimensions` 7-vector and align solely at that vector, never coupled and never re-minted: the symbolic side alone resolves a proven monomial onward to the Compute-internal `QuantityFamily` row, while the lower-stratum seam `Dimension` resolves its quantity through the `UnitsNet` registry directly and never names `QuantityFamily`.

The fold descends the `Entity` node hierarchy directly through positional record patterns — the numeric leaves, `Entity.Variable` (free symbols resolved through the `DimensionContext`, the pi/e constants structurally discriminated as `Vars`-empty leaves), the binary arithmetic records (`Sumf`/`Minusf`/`Mulf`/`Divf`/`Powf`/`Logf`), the unary function nodes (`Absf` dimension-preserving, `Signumf` dimension-erasing, the `TrigonometricFunction` family dimensionless-demanding), and the `CalculusOperator` residues (`Derivativef` scales as `dim(f)/dim(x)`, `Integralf` as `dim(f)·dim(x)`, `Limitf` as `dim(f)` — dimensional laws the F# predecessor could not state) — reads each free `Variable`'s declared dimension from a `DimensionContext` carried out of the parse (never an inferred default), and the `UNITS_BRIDGE` proves the resulting monomial against the SI baseline `Symbolic/units#DIMENSIONAL_LAW` owns. The two-argument `atan2` law is now structural rather than special-cased: the engine spells it `arctan(y/x)`, so a homogeneous ratio is dimensionless through the `Divf` arm and `Arctanf` admits it with zero name-table rows. Every dimensional failure — a heterogeneous sum, a dimensioned transcendental argument, a non-literal power exponent, an undeclared free symbol, and a result monomial that names no admitted `QuantityFamily` — folds onto the one `ComputeFault.DimensionMismatch` arm (code 2216 on the `Symbolic/expression#SYMBOLIC_EXPR` `SymbolicFault` family, the units-symbolic contribution to the `ComputeFault` 2200 band), never a parallel `DimensionError`. The monomial algebra is a generic-math group on `DimensionMonomial` itself (`operator *`/`operator /`/`Pow`/`MultiplicativeIdentity`), not a parallel `DimensionAlgebra` static class, and equality is the generated `[ValueObject]` structural compare, not a hand-rolled `Equal`. The spine is `AngouriMath` (the `Entity` node records and the `Vars` census), `PeterO.Numbers` (`ERational`), `UnitsNet` (`BaseDimensions`), Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [01]-[DIMENSION_MONOMIAL]: the seven-exponent SI base-dimension `[ValueObject]` and its generic-math rational-exponent group.
- [02]-[DIMENSION_PROOF]: the exhaustive `Entity`-node fold and the `Validation` accumulating rail over a parsed `SymbolicExpr`.
- [03]-[UNITS_BRIDGE]: the `QuantityFamily` `BaseDimensions` projection, the `DimensionVerdict` candidate set, and the pre-numeric admission gate.

## [02]-[DIMENSION_MONOMIAL]

- Owner: `DimensionMonomial` `[ValueObject]` over a seven-element rational exponent vector (length, mass, time, current, temperature, amount, luminous-intensity — the SI base order UnitsNet `BaseDimensions` exposes), implementing the `System.Numerics` generic-math group surface (`IMultiplyOperators`/`IDivisionOperators`/`IMultiplicativeIdentity`) so the monomial product/quotient/scalar-power are the type's own operators; `SiAxis` the axis-index and dimension-glyph constant table; `DimensionContext` (declared in `UNITS_BRIDGE`) the parse-supplied free-symbol-to-monomial binding the fold reads.
- Cases: one value carries all seven exponents as `ERational` (the same exact-rational arithmetic the engine's `Entity.Number.Rational` payload exposes through its `ERational` accessor), so a half-power root and a reciprocal both stay exact and a float-rounded exponent never decides consistency; `Dimensionless` is the zero vector and the multiplicative identity of the group; `Base(index)` mints a unit exponent on one axis.
- Entry: `DimensionMonomial.From(BaseDimensions dims)` — total projection of a UnitsNet dimension vector through `ERational.FromInt32` (each integer axis lifts through the verified factory, never a cast); `Of(params (int Axis, ERational Exponent)[])` — sparse construction over the zero seed; equality, hashing, and `==` are the generated `[ValueObject]` members over the seven exact exponents, so the monomial is a dictionary key with no hand-written comparer.
- Packages: Thinktecture.Runtime.Extensions (the `[ValueObject]` generator, structural equality, and the `ValidateFactoryArguments` rank hook), PeterO.Numbers (`ERational` `Zero`/`One`/`IsZero`/`FromInt32`/`FromEDecimal` and the `+`/`-`/`*` operators the exponent algebra folds with), UnitsNet (`BaseDimensions` axis order and the `.Length`/`.Mass`/`.Time`/`.Current`/`.Temperature`/`.Amount`/`.LuminousIntensity` `int` exponent accessors), LanguageExt.Core (the `Seq<ERational>` carrier with structural equality, the index-first static `Seq.map(seq, (axis, exponent) => …)` axis projection, and the `Zip`/`ForAll`/`Filter`/`Find` combinators plus `toSeq`).
- Growth: a new SI axis is impossible (the seven are closed by definition); a new compound relation is one row on `Symbolic/units#DIMENSIONAL_LAW`, never a `DimensionMonomial` change; the exponent type stays `ERational` so no precision-widening edit is ever needed; a richer diagnostic is one change to `Format`, never a parallel renderer.
- Boundary: the monomial is an interior value that never crosses a wire and never re-mints a `QuantityFamily` — it is the dimension vector UnitsNet already computes, lifted to exact rationals so the symbolic `Powf` case can carry a non-integer exponent UnitsNet's `int`-exponent `BaseDimensions` cannot. The carrier is a `Seq<ERational>` vector, not seven scalar fields and not the seam's `[ComplexValueObject]` integer `Dimension`, because the proof's algebra is uniform exponent-vector arithmetic — `Seq` is the canonical structural-equality sequence carrier whose `Zip`/`ForAll`/`Filter` combinators express the group operators and the renderer fold directly, and whose axis-aware sparse-construction and render projections route through the index-first static `Seq.map(seq, (axis, exponent) => …)` (the instance `.Map` is the unindexed `Func<A,B>` form, so the axis-indexed fold takes the static `Seq.map`, never a phantom indexed instance overload), and its structural equality makes the monomial a `FrozenDictionary` key with no hand-rolled comparer. The group law lives on the type as `operator *` (exponent-vector addition), `operator /` (subtraction), `Pow(ERational)` (the scalar group action), and `MultiplicativeIdentity` (the zero vector) — a parallel `DimensionAlgebra` static class is the collapsed defect, and a hand-rolled element-wise `Equal` is deleted because the generated `[ValueObject]` `==` is the structural compare. The `ERational` exponents compare through `Equals(ERational)`/`CompareTo` (the type ships no `==` operator), so every equality read in the algebra spells `Equals`, never a phantom operator. The rank-7 invariant is enforced through the Thinktecture `ValidateFactoryArguments` generator hook — a hand-rolled `static ComputeFault Validate` method is not a generator-recognized hook and never runs, so it is the rejected form — so a `Create` over a wrong-length `Seq` is rejected at admission while the interior algebra — which only ever composes rank-7 vectors — never trips it. The projection back to a named family runs at the `UNITS_BRIDGE` admission gate, never inside the algebra.

```csharp contract
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

    // The dimension glyphs UnitsNet `BaseDimensions` documents (L M T I Θ N J), indexed by axis, so a
    // `DimensionMonomial` renders "M L^2 T^-2" in a fault rather than a raw exponent array.
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

    // The empty dimension is the neutral element of the monomial product — the group identity an
    // `IMultiplyOperators` fold seeds with, the same value the domain reads as `Dimensionless`.
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

    // The free-Abelian-group algebra over ℚ⁷: monomial product is exponent-vector addition, quotient is
    // subtraction, and the rational `Pow` is the scalar group action (an area to the 1/2 is a length).
    public static DimensionMonomial operator *(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.Value.Zip(right.Value, static (a, b) => a + b));

    public static DimensionMonomial operator /(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.Value.Zip(right.Value, static (a, b) => a - b));

    public DimensionMonomial Pow(ERational exponent) =>
        Create(Value.Map(e => e * exponent));

    // The physics-notation projection a fault reports ("1" for dimensionless, else "M L^2 T^-2").
    public string Format() =>
        Seq.map(Value, static (axis, exponent) => (Symbol: SiAxis.Symbol[axis], Exponent: exponent))
            .Filter(static t => !t.Exponent.IsZero)
            .Map(static t => t.Exponent.Equals(ERational.One) ? t.Symbol : $"{t.Symbol}^{t.Exponent}") is { IsEmpty: false } factors
                ? string.Join(" ", factors)
                : "1";
}
```

## [03]-[DIMENSION_PROOF]

- Owner: `DimensionProof` the static fold entry and the recursive `Entity`-node `Descend`; `DimensionContext` (declared in `UNITS_BRIDGE`) the free-symbol binding the fold reads; `ComputeFault.DimensionMismatch` (code 2216) the one fault arm every dimensional failure rides.
- Cases: the `Entity` node hierarchy `Symbolic/expression#SYMBOLIC_EXPR` confirms — every `Entity.Number` leaf is a `Dimensionless` literal; `Entity.Variable` reads its declared monomial from the `DimensionContext`, with the pi/e constants structurally discriminated (a constant `Variable` leaf has an EMPTY `Vars` census — the engine excludes constants from free variables — so it resolves `Dimensionless` with no name table); `Sumf`/`Minusf` require both operands to carry the identical monomial (the canonical defect this fold catches); `Mulf`/`Divf` fold the operand monomials through the `*`/`/` operators; `Powf` requires a numeric-literal exponent and scales the base monomial through `Pow`, rejecting a symbolic (dimensioned) exponent — this arm also covers `sqrt`, which the engine lowers to `Powf(arg, 1/2)` so a half-power root scales by `1/2` with no name-table special case; `Absf` is dimension-preserving (abs of a force is a force); `Signumf` admits any dimension and returns dimensionless; `Logf` demands both base and antilogarithm dimensionless; every `TrigonometricFunction` and remaining unary `Function` node is dimensionless-demanding through the `IUnaryNode.NodeChild` accessor — `atan2` needs no arm because the engine spells it `arctan(y/x)` and the homogeneous `Divf` ratio is already dimensionless; the `CalculusOperator` family carries derived laws — `Derivativef` is `dim(f)/dim(x)`, `Integralf` is `dim(f)·dim(x)`, `Limitf` is `dim(f)`; a `Statement`/`Set`/boolean-sorted node short-circuits to the fault arm (a constraint is not a numeric monomial).
- Entry: `Prove(SymbolicExpr expr, DimensionContext context)` — one polymorphic entry returning `Validation<Error,DimensionMonomial>`; arity discriminates on the carried `Entity` node case, never a per-case public method; the accumulating rail collects every `Sumf`-mismatch and every undeclared symbol across the whole tree in one pass rather than aborting at the first.
- Packages: AngouriMath (the `Entity` node records pattern-matched positionally — `Sumf(Augend, Addend)`, `Minusf(Subtrahend, Minuend)`, `Mulf(Multiplier, Multiplicand)`, `Divf(Dividend, Divisor)`, `Powf(Base, Exponent)`, `Logf(Base, Antilogarithm)`, the unary `(Argument)` function nodes behind `IUnaryNode.NodeChild`, `Entity.Variable.Name`, the `Entity.Number.Rational.ERational`/`Real.EDecimal` payload accessors, and the per-node `Vars` census that discriminates constants; `Stringize` renders a rejected exponent), LanguageExt.Core (the `Validation` applicative accumulating rail, `Traverse`, `Seq` fold, and `Distinct`), PeterO.Numbers (`ERational`), Thinktecture.Runtime.Extensions (the `DimensionMonomial` value).
- Growth: the unary-function law covers every transcendental the engine adds through the `IUnaryNode` floor without a per-name table; a genuinely new node family (the engine's hierarchy is closed at the pin) surfaces as the typed unmapped-node fault, never a silent fall-through; zero new entrypoint.
- Boundary: the fold reads the `Entity` node payloads directly through positional record patterns rather than re-parsing the infix string, so the proof runs once over the already-canonical tree. The constant discrimination is structural — a `Variable` leaf whose own `Vars` census is empty is a constant (pi/e) and resolves `Dimensionless`, so no constant-name string table exists; every other `Variable` resolves through `DimensionContext.Resolve` inline, and one absent from the context accumulates as a `DimensionMismatch` (an undeclared symbol has no static dimension) instead of a hidden dimensionless default — the `UNITS_BRIDGE` census over the `SymbolicExpr.FreeSymbols` identifier set pre-validates that the context covers every free symbol before descent. A `Powf` exponent admits an exact `Rational` (which subsumes `Integer` by inheritance) or a finite `Real` (`x^0.5`, lifted exactly through `ERational.FromEDecimal`), and a symbolic exponent has no static dimension scale so it accumulates as `DimensionMismatch`. The transcendental arms preserve dimension only for `Absf` and erase it only for `Signumf` — a blanket "every function is dimensionless-demanding" rule would wrongly reject `abs(force)` and `sign(moment)`. The `_` arm is the foreign-node-future-case guard producing a fault, never a silent fall-through. The proof never evaluates a number and never compiles a delegate — it is the gate the `Symbolic/lowering#LOWERING` compile fence runs behind. The rail is `Validation<Error,DimensionMonomial>` (the monoidal `Error` failure carrier every sibling lane uses, never `Validation<ComputeFault,…>` — `ComputeFault` is not its own monoid; the typed arm lifts onto `Error` through its `Expected` base) so a single ill-formed compound surfaces every constituent mismatch at once, the accumulating posture `Symbolic/units#DIMENSIONAL_LAW` already uses for its compound sweep.

```csharp contract
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
            // A constant Variable leaf (pi, e) has an empty free-variable census — the structural
            // discriminant that keeps constants out of the context with zero name-table rows.
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
            // The IUnaryNode floor covers the whole trig/transcendental family in one arm.
            Entity.Function and IUnaryNode unary =>
                Dimensionless(Seq(unary.NodeChild), context, node.GetType().Name),
            Entity.Statement or Entity.Set or Entity.Boolean =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: non-numeric node {node.GetType().Name} in a formula proof")),
            _ =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch($"dimension: unmapped node {node.GetType().Name}")),
        };

    // Dimensionless-demanding arguments returning dimensionless; the accumulating `Traverse`
    // surfaces every dimensioned argument at once.
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

- Owner: `DimensionContext` the parse-context binding the fold resolves free symbols through; `DimensionVerdict` the typed dimensional receipt carrying the proven monomial and its candidate `QuantityFamily` set; `DimensionAdmission` the static projection that censuses, proves, and matches a proven monomial against the `Symbolic/units#DIMENSIONAL_LAW` SI baseline.
- Cases: a proven monomial resolves to the set of `QuantityFamily` rows whose `Info.BaseDimensions` (lifted through `DimensionMonomial.From`) equal it — usually one, but `Energy` and `Torque` share `M·L²·T⁻²` and `Ratio` and `Angle` share the zero vector, so the verdict carries every candidate and `Unique` is `Some` only when exactly one row matches; a proven monomial that matches no admitted row is a `DimensionMismatch` so a formula whose result dimension has no admitted quantity is rejected before numeric admission; the bound free symbols arrive from the `UnitProject` parse context, each free `Variable` carrying its declared `QuantityFamily`.
- Entry: `Admit(SymbolicExpr expr, DimensionContext context)` — `Validation<Error,DimensionVerdict>` composing the `SymbolicExpr.FreeSymbols` census, `DimensionProof.Prove`, and the row match; the census fails fast if any free symbol is undeclared, the proof then accumulates every structural mismatch, and the match names the candidate families; no `IQuantity` is ever constructed because admission runs before any value materializes.
- Packages: UnitsNet (the frozen `QuantityFamily.Info.BaseDimensions` rows, never re-minted), LanguageExt.Core (`Validation`, `Map`, `Seq`, `Option`, and the census filter), Thinktecture.Runtime.Extensions (`QuantityFamily.Items` and the `DimensionMonomial` dictionary key), AngouriMath (`SymbolicExpr` input and the `FreeSymbols` set driving the pre-descent census), BCL inbox (`FrozenDictionary` for the dimension-keyed match table).
- Growth: a new admitted result dimension is one new `QuantityFamily` row on `Symbolic/units#QUANTITY_TABLE` — the match table groups `QuantityFamily.Items` by `DimensionMonomial` at static construction, so a row added there extends admission (or joins an existing dimensionally-equal candidate set) with zero edit here; a richer verdict is one field on `DimensionVerdict`; zero new surface.
- Boundary: the bridge is the single seam between the symbolic arm and the units boundary — it consumes the `Symbolic/units#DIMENSIONAL_LAW` SI vocabulary as the proof target and never re-mints a quantity type, never crosses a wire, and never constructs an `IQuantity` (the proof is purely over dimensions). The match table is a `FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>>` built once at static construction by grouping `QuantityFamily.Items` on their `DimensionMonomial` (a valid key because `[ValueObject]` generates structural equality and hashing over the exact exponents), so the O(1) lookup replaces a linear scan and the candidate set is the value — a `Find`-first-match silently collapses the `Energy`/`Torque` and `Ratio`/`Angle` non-injectivity and is the deleted form, because dimension → quantity is not a function. The verdict reports the full candidate set rather than naming one family the dimension cannot uniquely identify; a consumer needing one quantity resolves the ambiguity through `Unique` rather than trusting an arbitrary first row. The census fails fast on an undeclared free symbol before descent, the proof accumulates every structural mismatch, and the match runs only on a clean proof — the sequencing is fail-fast `Bind` over the accumulating `Prove`. The dimensional gate runs strictly before the `Symbolic/units#QUANTITY_TABLE` `Admit` value-conversion entrypoint, so a dimension-inconsistent formula never reaches `Admit`, the optimizer oracle, or the cost catalog; a `UnitProject` intent supplies the `DimensionContext` from its parse, and the gate's `Validation` failure aborts the project before numeric admission, the pre-numeric posture this whole arm exists to enforce.

```csharp contract
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

// The proven dimension plus every admitted `QuantityFamily` that shares it: dimension → quantity is NOT
// injective (`Energy`/`Torque` are both M·L²·T⁻²; `Ratio`/`Angle` are both the zero vector), so the bridge
// reports the candidate set and a consumer that needs one family resolves the ambiguity through `Unique`,
// never the first row a linear scan happened to hit.
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

- [SEAM_DIMENSION_ALIGNMENT]: `DimensionMonomial` is the ℚ⁷ symbolic-proof generalization of the seam `Rasm.Element/Properties/quantity#DIMENSION` ℤ⁷ measure discriminator — distinct carriers (rational vs integer exponents, symbolic-CAS app-platform vs AEC-domain measure) aligned SOLELY at the one `UnitsNet` `BaseDimensions` 7-vector both project from (`DimensionMonomial.From` and the seam `Dimension.Of`), never coupled and with no reference either way. The onward resolution is NOT shared: the symbolic side alone groups a proven monomial onto the Compute-internal `QuantityFamily` row (`DimensionAdmission`), while the lower-stratum seam `Dimension` resolves its quantity through the `UnitsNet` registry directly and never names `QuantityFamily` (the AEC-domain seam cannot reference an app-platform owner) — asserting a shared `QuantityFamily` resolution is the illusory-seam defect. The rational carrier exists because a symbolic `Powf` sub-tree carries a transient fractional exponent the integer measure `Dimension` cannot hold; a proven monomial whose exponents are all integral is exactly the seam `Dimension` the admitted measure carries. The two stay intentionally distinct and aligned — not an accidental duplicate concept — and the two display projections stay in their own registers: the symbolic `Format` renders physics dimension-glyph notation (`SiAxis.Symbol`, e.g. `M L^2 T^-2`) while the seam `Dimension.SiSymbol` renders the SI coherent-unit symbol (e.g. `Pa`) — distinct output registers with NO string-equality obligation between them, yet not free to drift: both project the SAME `UnitsNet` `BaseDimensions` 7-vector in the SAME SI axis order (`SiAxis` and the seam `Dimension.Of` read the identical `Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity` field order, and `SiAxis.Symbol`'s glyphs are the `L`·`M`·`T`·`I`·`Θ`·`N`·`J` dimensions `UnitsNet` documents), so the shared `UnitsNet` anchor IS the coherence guarantee and neither register can encode an axis convention the other does not.
- [DIMENSION_CONTEXT_SOURCE]: the `UnitProject` parse context that supplies the free-symbol-to-`QuantityFamily` declarations is the `Symbolic/units` boundary's `UnitProject` intent shape; the exact field the symbol declarations ride lands when the `UnitProject` parse surface is widened with a symbolic-formula case, run as an additive case on the existing intent rather than a new parse owner.
