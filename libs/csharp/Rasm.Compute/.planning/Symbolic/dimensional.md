# [SYMBOLIC_DIMENSIONAL]

The pre-numeric dimensional proof for the symbolic CAS arm: a parsed `SymbolicExpr` (the F# `Expression` DU wrapped by `Symbolic/expression#SYMBOLIC_EXPR`) folds onto a `DimensionMonomial` — one seven-exponent SI base-dimension vector, never seven scalar fields — and `DimensionProof` accumulates every compound mismatch on the `Validation<ComputeFault,DimensionMonomial>` rail before a single numeric value reaches the optimizer or the cost catalog. The fold descends the `Sum`/`Product`/`Power`/`Function` DU payloads `Symbolic/expression#SYMBOLIC_EXPR` confirms, reads each free `Identifier`'s declared dimension from a `DimensionContext` carried out of the parse (never an inferred default), and proves the resulting monomial against the SI baseline `Symbolic/units#DIMENSIONAL_LAW` already owns — the `BaseDimensions` algebra over the frozen `QuantityFamily` rows, re-read here, never re-minted. Every dimensional failure — a heterogeneous sum, a dimensioned transcendental argument, a non-literal power exponent, an undeclared free symbol, and a result monomial that names no admitted `QuantityFamily` row — folds onto the one new arm `SymbolicFault.DimensionMismatch` on the `Symbolic/expression#SYMBOLIC_EXPR` `SymbolicFault` family at code 2216, the units-symbolic contribution to the `ComputeFault` 2200 band, never a parallel `DimensionError` type or a second arm. The proof admits a formula `Symbolic/units#DIMENSIONAL_LAW` and the numeric admission gate then trust without re-deriving. The spine is MathNet.Symbolics (the closed `Expression` DU pattern-matched directly over its `Sum`/`Product`/`Power`/`Function`/`Identifier`/`Number` payloads, plus the `SymbolicExpr.FreeSymbols` `CollectVariables()` census), UnitsNet (`BaseDimensions` SI vocabulary), Thinktecture.Runtime.Extensions, and LanguageExt.Core over the symbolic chain head.

## [1]-[INDEX]

- [1]-[DIMENSION_MONOMIAL]: the seven-exponent SI base-dimension `[ValueObject]` and its rational exponent algebra.
- [2]-[DIMENSION_PROOF]: the recursive DU fold and the `Validation` accumulating rail over a parsed `SymbolicExpr`.
- [3]-[UNITS_BRIDGE]: the `QuantityFamily` `BaseDimensions` projection and the pre-numeric admission gate.

## [2]-[DIMENSION_MONOMIAL]

- Owner: `DimensionMonomial` `[ValueObject]` over a seven-element rational exponent vector (length, mass, time, current, temperature, amount, luminous-intensity — the SI base order UnitsNet `BaseDimensions` exposes); `DimensionContext` the parse-supplied free-symbol-to-monomial binding; `DimensionAlgebra` the static product/quotient/power/equality surface.
- Cases: one value carries all seven exponents as `BigRational` (the same exact-rational arithmetic the F# `Expression` `Number of BigRational` payload carries), so a half-power root (`sqrt` of an area is a length) and a reciprocal both stay exact and a float-rounded exponent never decides consistency; `Dimensionless` is the zero vector; `Base(index)` mints a unit exponent on one axis.
- Entry: `DimensionMonomial.From(BaseDimensions dims)` — total projection of a UnitsNet dimension vector; `Of(params (int Axis, BigRational Exponent)[])` — sparse construction; equality is structural over the seven exact exponents.
- Packages: Thinktecture.Runtime.Extensions (the `[ValueObject]` generator and structural equality), UnitsNet (`BaseDimensions` axis order and the `.Length`/`.Mass`/`.Time`/`.Current`/`.Temperature`/`.Amount`/`.LuminousIntensity` exponent accessors), LanguageExt.Core (the `Seq`/`Arr` carrier).
- Growth: a new SI axis is impossible (the seven are closed by definition); a new compound relation is one row on `Symbolic/units#DIMENSIONAL_LAW`, never a `DimensionMonomial` change; the exponent type stays `BigRational` so no precision-widening edit is ever needed.
- Boundary: the monomial is an interior value that never crosses a wire and never re-mints a `QuantityFamily` — it is the dimension vector UnitsNet already computes, lifted to exact rationals so the symbolic `Power` case can carry a non-integer exponent UnitsNet's `int`-exponent `BaseDimensions` cannot; the projection back to a named family runs at the `UNITS_BRIDGE` admission gate, never inside the algebra; the `DimensionContext` binds a free `Identifier` to its declared monomial from the parse context and an unbound identifier is a `SymbolicFault.DimensionMismatch` (a symbol with no declared quantity has no static dimension), never a silent dimensionless default.

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
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Arr<BigRational>>]
[ValueObjectValidationError<ComputeFault>]
public readonly partial struct DimensionMonomial {
    public static readonly DimensionMonomial Dimensionless = new(Arr.create(
        BigRational.Zero, BigRational.Zero, BigRational.Zero, BigRational.Zero,
        BigRational.Zero, BigRational.Zero, BigRational.Zero));

    private static ComputeFault Validate(Arr<BigRational> exponents) =>
        exponents.Count == SiAxis.Rank
            ? null
            : ComputeFault.Create($"dimension-monomial: rank {exponents.Count} not {SiAxis.Rank}");

    public static DimensionMonomial Base(int axis) =>
        Of((axis, BigRational.One));

    public static DimensionMonomial Of(params (int Axis, BigRational Exponent)[] terms) =>
        Create(Range(0, SiAxis.Rank)
            .Map(axis => terms.Find(t => t.Axis == axis).Map(static t => t.Exponent).IfNone(BigRational.Zero))
            .ToArr());

    public static DimensionMonomial From(BaseDimensions dims) =>
        Create(Arr.create(
            (BigRational)dims.Length,
            (BigRational)dims.Mass,
            (BigRational)dims.Time,
            (BigRational)dims.Current,
            (BigRational)dims.Temperature,
            (BigRational)dims.Amount,
            (BigRational)dims.LuminousIntensity));

    public BigRational this[int axis] => Value[axis];

    public bool IsDimensionless => Value.ForAll(static e => e.IsZero);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionAlgebra {
    public static DimensionMonomial Product(DimensionMonomial left, DimensionMonomial right) =>
        DimensionMonomial.Create(left.Value.Zip(right.Value, static (a, b) => a + b).ToArr());

    public static DimensionMonomial Quotient(DimensionMonomial left, DimensionMonomial right) =>
        DimensionMonomial.Create(left.Value.Zip(right.Value, static (a, b) => a - b).ToArr());

    public static DimensionMonomial Power(DimensionMonomial bas, BigRational exponent) =>
        DimensionMonomial.Create(bas.Value.Map(e => e * exponent).ToArr());

    public static bool Equal(DimensionMonomial left, DimensionMonomial right) =>
        left.Value.Zip(right.Value, static (a, b) => a == b).ForAll(static eq => eq);
}
```

## [3]-[DIMENSION_PROOF]

- Owner: `DimensionProof` the static fold entry; `DimensionFold` the recursive `Expression`-DU descent; `DimensionContext` (declared in `DIMENSION_MONOMIAL`) the free-symbol binding the fold reads.
- Cases: the closed F# `Expression` DU `Symbolic/expression#SYMBOLIC_EXPR` confirms — `Number`/`Approximation`/`Constant` and the bare `PositiveInfinity`/`NegativeInfinity`/`ComplexInfinity` constants are `Dimensionless` literals; `Identifier` reads its declared monomial from the `DimensionContext`; `Sum` requires every addend to carry the identical monomial (a dimensioned sum of incompatible terms is the canonical defect this fold catches); `Product` folds the factor monomials through `DimensionAlgebra.Product`; `Power` requires an integer-or-rational `Number` exponent and scales the base monomial through `DimensionAlgebra.Power`, rejecting a symbolic (dimensioned) exponent — this arm also covers `sqrt`, which the `MathNet.Symbolics` parser lowers to `Power(arg, 1/2)` rather than a `Function.Sqrt` case, so a half-power root scales the argument monomial by `1/2` with no name-table special case; `Function` recurses through the `Function of Function * Expression` payload, requiring a `Dimensionless` argument for every transcendental (`Sin`/`Cos`/`Exp`/`Ln`/`...`); `Undefined` short-circuits to a `SymbolicFault`.
- Entry: `Prove(SymbolicExpr expr, DimensionContext context)` — one polymorphic entry returning `Validation<ComputeFault,DimensionMonomial>`; arity discriminates on the carried `Expression` DU case, never a per-case public method; the accumulating rail collects every `Sum`-mismatch and every undefined symbol across the whole tree in one pass rather than aborting at the first.
- Packages: MathNet.Symbolics (the closed F# `Expression` DU pattern-matched directly over its confirmed `Sum`/`Product`/`Power`/`Function`/`Identifier`/`Number` payloads — the `Identifier of Symbol` payload reads the name through the `Symbol of string` `.Item` accessor, the verified F# DU projection; the free-symbol census the `UNITS_BRIDGE` validates reuses the `SymbolicExpr.FreeSymbols` `CollectVariables()` set `Symbolic/expression#SYMBOLIC_EXPR` owns), LanguageExt.Core (the `Validation` applicative accumulating rail, `Traverse`, and `Seq` fold), Thinktecture.Runtime.Extensions (the `DimensionMonomial` value).
- Growth: the F# `Function` DU is closed, so the dimensionless-argument requirement covers every transcendental by one structural arm without a name table (`sqrt` is not a `Function` case — it parses to `Power(arg, 1/2)` and the `Power` arm scales it); a new structural `Expression` DU case (also closed, so this never fires) would be one match arm; zero new entrypoint.
- Boundary: the fold reads the F# `Expression` DU payloads directly through pattern matching — the `Number of BigRational` and `Function of Function * Expression` payloads `Symbolic/expression#SYMBOLIC_EXPR` verified — rather than re-parsing the infix string, so the proof runs once over the already-canonical tree; the descent resolves each free `Identifier` through `DimensionContext.Resolve` inline, so an `Identifier` absent from the `DimensionContext` accumulates as a `SymbolicFault.DimensionMismatch` (an undeclared symbol has no static dimension) instead of a hidden dimensionless default — the `UNITS_BRIDGE` census over the `SymbolicExpr.FreeSymbols` identifier set pre-validates that the context covers every free symbol before descent; the proof never evaluates a number and never compiles a delegate — it is the gate the `Symbolic/lowering#LOWERING` compile fence runs behind; a `Power` whose exponent is not a literal `Number` accumulates as the same `DimensionMismatch` arm because a symbolic exponent has no static dimension scale; the rail is `Validation<ComputeFault,DimensionMonomial>` so a single ill-formed compound surfaces every constituent mismatch at once, the accumulating posture `Symbolic/units#DIMENSIONAL_LAW` already uses for its compound sweep.

```csharp contract
// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DimensionMismatch : SymbolicFault {
    public DimensionMismatch(string detail) : base(detail, 2216) { }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionProof {
    public static Validation<ComputeFault, DimensionMonomial> Prove(SymbolicExpr expr, DimensionContext context) =>
        DimensionFold.Descend(expr.Expression, context);

    private static class DimensionFold {
        public static Validation<ComputeFault, DimensionMonomial> Descend(Expression node, DimensionContext context) =>
            node switch {
                Expression.Number or Expression.Approximation or Expression.Constant
                    or Expression.PositiveInfinity or Expression.NegativeInfinity or Expression.ComplexInfinity =>
                    Success<ComputeFault, DimensionMonomial>(DimensionMonomial.Dimensionless),
                Expression.Identifier identifier =>
                    context.Resolve(identifier.Item.Item),
                Expression.Sum sum =>
                    Homogeneous(sum.Item.ToSeq(), context),
                Expression.Product product =>
                    Fold(product.Item.ToSeq(), context, DimensionMonomial.Dimensionless, DimensionAlgebra.Product),
                Expression.Power power =>
                    Scale(power, context),
                Expression.Function function =>
                    Apply(function, context),
                _ =>
                    Fail<ComputeFault, DimensionMonomial>(
                        new DimensionMismatch($"dimension: undefined node {node.GetType().Name}")),
            };

        private static Validation<ComputeFault, DimensionMonomial> Fold(
            Seq<Expression> factors,
            DimensionContext context,
            DimensionMonomial seed,
            Func<DimensionMonomial, DimensionMonomial, DimensionMonomial> compose) =>
            factors.Fold(
                Success<ComputeFault, DimensionMonomial>(seed),
                (acc, factor) => (acc, Descend(factor, context)).Apply(compose));

        private static Validation<ComputeFault, DimensionMonomial> Homogeneous(
            Seq<Expression> addends,
            DimensionContext context) =>
            addends.Traverse(addend => Descend(addend, context)).Bind(static monomials =>
                monomials.Distinct(DimensionAlgebra.Equal).Count <= 1
                    ? Success<ComputeFault, DimensionMonomial>(monomials.HeadOrNone().IfNone(DimensionMonomial.Dimensionless))
                    : Fail<ComputeFault, DimensionMonomial>(
                        new DimensionMismatch($"dimension: heterogeneous sum over {monomials.Count} terms")));

        private static Validation<ComputeFault, DimensionMonomial> Scale(Expression.Power power, DimensionContext context) =>
            (Descend(power.Item1, context), Literal(power.Item2))
                .Apply(DimensionAlgebra.Power);

        private static Validation<ComputeFault, DimensionMonomial> Apply(Expression.Function function, DimensionContext context) =>
            Descend(function.Item2, context).Bind(argument =>
                argument.IsDimensionless
                    ? Success<ComputeFault, DimensionMonomial>(DimensionMonomial.Dimensionless)
                    : Fail<ComputeFault, DimensionMonomial>(
                        new DimensionMismatch($"dimension: {function.Item1} requires a dimensionless argument")));

        private static Validation<ComputeFault, BigRational> Literal(Expression exponent) =>
            exponent is Expression.Number number
                ? Success<ComputeFault, BigRational>(number.Item)
                : Fail<ComputeFault, BigRational>(
                    new DimensionMismatch($"dimension: non-literal power exponent {exponent.GetType().Name}"));
    }
}
```

## [4]-[UNITS_BRIDGE]

- Owner: `DimensionContext` the parse-context binding (declared in `DIMENSION_MONOMIAL`, composed here); `DimensionAdmission` the static projection that hands a proven monomial to the `Symbolic/units#DIMENSIONAL_LAW` SI baseline and names the matching `QuantityFamily` row.
- Cases: a proven monomial resolves to exactly one `QuantityFamily` row whose `Info.BaseDimensions` (lifted through `DimensionMonomial.From`) equals it — the dimensionless monomial resolves to the `Ratio` family `Symbolic/units#DIMENSIONAL_LAW` carries; a proven monomial that matches no admitted row is the same `SymbolicFault.DimensionMismatch` arm so a formula whose result dimension has no admitted quantity is rejected before numeric admission rather than silently accepted; the bound free symbols arrive from the `UnitProject` parse context, each `Identifier` carrying its declared `QuantityFamily`.
- Entry: `Admit(SymbolicExpr expr, DimensionContext context)` — `Validation<ComputeFault,QuantityFamily>` composing the `SymbolicExpr.FreeSymbols` free-symbol census, `DimensionProof.Prove`, and the row match; the census fails fast if any free symbol is undeclared, the proof then accumulates every structural mismatch, and the row match names the result family; no `IQuantity` is ever constructed because admission runs before any value materializes.
- Packages: UnitsNet (the frozen `QuantityFamily.Info.BaseDimensions` rows, `UnitSystem.SI` baseline, never re-minted), LanguageExt.Core (`Validation`, the row search, and the census filter), MathNet.Symbolics (`SymbolicExpr` input and the `FreeSymbols` `CollectVariables()` set driving the pre-descent free-symbol census).
- Growth: a new admitted result dimension is one new `QuantityFamily` row on `Symbolic/units#QUANTITY_TABLE` — the bridge derives its match table from `QuantityFamily.Items` at static construction, so a row added there extends admission with zero edit here; zero new surface.
- Boundary: the bridge is the single seam between the symbolic arm and the units boundary — it consumes the `Symbolic/units#DIMENSIONAL_LAW` SI vocabulary as the proof target and never re-mints a quantity type, never crosses a wire, and never constructs an `IQuantity` (the proof is purely over dimensions); the match table reads `QuantityFamily.Items` once at static construction (the same metadata-at-construction discipline `Symbolic/units#QUANTITY_TABLE` uses for its canonical column) so a family added there is admitted here automatically; the dimensional gate runs strictly before the `Symbolic/units#QUANTITY_TABLE` `Admit` value-conversion entrypoint, so a dimension-inconsistent formula never reaches `Admit`, the optimizer oracle, or the cost catalog; a `UnitProject` intent supplies the `DimensionContext` from its parse, and the gate's `Validation` failure aborts the project before numeric admission, the pre-numeric posture this whole arm exists to enforce.

```csharp contract
// --- [SERVICES] ------------------------------------------------------------------------
public sealed record DimensionContext(Map<string, DimensionMonomial> Bindings) {
    public static DimensionContext Of(Seq<(string Symbol, QuantityFamily Family)> declarations) =>
        new(declarations.Fold(
            Map<string, DimensionMonomial>(),
            static (acc, d) => acc.Add(d.Symbol, DimensionMonomial.From(d.Family.Info.BaseDimensions))));

    public Validation<ComputeFault, DimensionMonomial> Resolve(string symbol) =>
        Bindings.Find(symbol).Match(
            Some: static m => Success<ComputeFault, DimensionMonomial>(m),
            None: () => Fail<ComputeFault, DimensionMonomial>(
                new DimensionMismatch($"dimension: free symbol '{symbol}' has no declared quantity")));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionAdmission {
    private static readonly Seq<(QuantityFamily Family, DimensionMonomial Monomial)> Table =
        QuantityFamily.Items.ToSeq().Map(static row => (row, DimensionMonomial.From(row.Info.BaseDimensions)));

    public static Validation<ComputeFault, QuantityFamily> Admit(SymbolicExpr expr, DimensionContext context) =>
        Census(expr, context).Bind(_ => DimensionProof.Prove(expr, context)).Bind(Match);

    private static Validation<ComputeFault, Unit> Census(SymbolicExpr expr, DimensionContext context) =>
        expr.FreeSymbols
            .Filter(symbol => !context.Bindings.ContainsKey(symbol)) is { IsEmpty: false } undeclared
            ? Fail<ComputeFault, Unit>(
                new DimensionMismatch($"dimension: free symbols [{string.Join(", ", undeclared)}] undeclared in context"))
            : Success<ComputeFault, Unit>(unit);

    private static Validation<ComputeFault, QuantityFamily> Match(DimensionMonomial monomial) =>
        Table.Find(row => DimensionAlgebra.Equal(row.Monomial, monomial)).Match(
            Some: static row => Success<ComputeFault, QuantityFamily>(row.Family),
            None: () => Fail<ComputeFault, QuantityFamily>(
                new DimensionMismatch($"dimension: result {monomial} names no admitted QuantityFamily")));
}
```

## [5]-[RESEARCH]

- [DIMENSION_CONTEXT_SOURCE]: the `UnitProject` parse context that supplies the free-symbol-to-`QuantityFamily` declarations is the `Symbolic/units` boundary's `UnitProject` intent shape; the exact field the symbol declarations ride lands when the `UnitProject` parse surface is widened with a symbolic-formula case, run as an additive case on the existing intent rather than a new parse owner.
