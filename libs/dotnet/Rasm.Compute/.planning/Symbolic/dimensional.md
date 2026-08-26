# [SYMBOLIC_DIMENSIONAL]

Pre-numeric dimensional proof for the symbolic CAS arm. Every parsed `SymbolicExpr` folds onto a `DimensionMonomial` — one `Seq<ERational>` of seven SI base-dimension exponents, never seven scalar fields — and `DimensionProof` accumulates every compound mismatch on a `Validation<Error,DimensionMonomial>` result before a single numeric value reaches the optimizer or the cost catalog. Exponents ride the `PeterO.Numbers` `ERational` the engine's `Entity.Number.Rational` leaves carry, never `int`, so a `sqrt` lowering to `Powf(arg, 1/2)` makes a half-power root of an area exactly a length and a float-rounded exponent never decides consistency.

That rational vector is the ℚ⁷ symbolic generalization of the ℤ⁷ integer-exponent `Dimension` the contract `Rasm.Element/Properties/quantity#DIMENSION` carries for measured quantities — both project from the one `UnitsNet` `BaseDimensions` 7-vector and align solely there, never coupled and never re-minted. Onward resolution is not shared: the symbolic side alone resolves a proven monomial to the Compute-internal `QuantityFamily` row, while the lower-stratum contract `Dimension` resolves its quantity through the `UnitsNet` registry directly and never names `QuantityFamily`. Every refusal rides a direct `ComputeFault` arm `Symbolic/expression` owns, never a parallel `DimensionError`, and rides the arm its RECOVERY names: a heterogeneous sum, a dimensioned transcendental argument, and a non-literal power exponent are `DimensionMismatch` (2215) because the algebra is what must change; an undeclared free symbol is `SymbolUndefined` (2213) because a declaration is what is missing; an unresolvable family key and a non-formula node are `ParseRejected` (2212) because the input itself is inadmissible. A sound monomial the admitted roster names no row for is a verdict, not a failure. Spine: `AngouriMath` (the `Entity` node records and `Vars` census), `PeterO.Numbers` (`ERational`), `UnitsNet` (`BaseDimensions`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

## [01]-[INDEX]

- [02]-[DIMENSION_MONOMIAL]: `SiAxis` rows carry the glyph and the `BaseDimensions` reader; `DimensionMonomial` carries seven SI base-dimension exponents under a generic-math rational group and projects to the contract `Dimension`.
- [03]-[DIMENSION_PROOF]: `DimensionContext` admits the intent-declared symbol bindings; `DimensionProof` folds every `Entity` node onto one accumulating `Validation` result.
- [04]-[UNITS_BRIDGE]: `QuantityFamily` projects `BaseDimensions` into the named-or-unnamed `DimensionVerdict` the `Symbolic/lowering#LOWERING` compile gate binds its proof to.

## [02]-[DIMENSION_MONOMIAL]

- Owner: `DimensionMonomial` `[ValueObject]` over a seven-element `ERational` exponent vector (SI base order — length, mass, time, current, temperature, amount, luminous-intensity — as UnitsNet `BaseDimensions` exposes), implementing the `System.Numerics` generic-math group (`IMultiplyOperators`/`IDivisionOperators`/`IUnaryNegationOperators`/`IMultiplicativeIdentity`) so product, quotient, inverse, and scalar-power are the type's own operators; `SiAxis` the `[SmartEnum<int>]` axis roster whose two columns carry the physics glyph and the `BaseDimensions` component reader.
- Cases: one value carries all seven exponents as `ERational`, so a half-power root and a reciprocal both stay exact; `Dimensionless` is the zero vector and the group's multiplicative identity; `Base(axis)` mints a unit exponent on one axis row.
- Law: the monomial is CANONICAL BY CONSTRUCTION. `ERational` equality is numerator/denominator-exact and its arithmetic never reduces, so `*`, `/`, and `Pow` alone leave `2/4` and `1/2` as distinct keys for one dimension; the factory hook reduces every exponent to lowest terms at the single mint, so equality, hashing, and the `FrozenDictionary` family lookup all read one representative per physical dimension and no operator needs a reduction of its own.
- Entry: `From(BaseDimensions)` totalizes a UnitsNet vector by traversing the `SiAxis` rows through each row's `Read` column and `ERational.FromInt32` (each integer axis lifts through the verified factory, never a cast), so the seven-line hand transposition exists once and `Symbolic/units#DIMENSIONAL_LAW` composes it; `Of(params ReadOnlySpan<(SiAxis Axis, ERational Exponent)>)` sparse-constructs over the zero seed; `ToContract()` projects an all-integral monomial onto the contract `Dimension`; equality, hashing, and `==` are the generated `[ValueObject]` members over the seven exact exponents, so the monomial is a dictionary key with no hand-written comparer.
- Packages: Thinktecture.Runtime.Extensions (`[ValueObject]`/`[SmartEnum<int>]` generators, `[UseDelegateFromConstructor]`, structural equality, `ValidateFactoryArguments` hook), PeterO.Numbers (`ERational` `Zero`/`One`/`IsZero`/`IsInteger()`/`Negate()`/`ToInt32Checked()`/`FromInt32`/`FromEDecimal` and its `+`/`-`/`*` operators), UnitsNet (`BaseDimensions` axis order and its `.Length`/`.Mass`/`.Time`/`.Current`/`.Temperature`/`.Amount`/`.LuminousIntensity` `int` accessors), LanguageExt.Core (`Seq<ERational>`, `Zip`/`ForAll`/`Filter`/`Find`/`Traverse`, `toSeq`), Rasm.Element (project — the contract `Dimension` `[ComplexValueObject]` `ToContract` mints).
- Growth: a new SI axis is impossible (the seven are closed); a new compound relation is one row on `Symbolic/units#DIMENSIONAL_LAW`, never a `DimensionMonomial` change; the exponent type stays `ERational`, so no precision-widening edit is ever needed; a richer diagnostic is one `Format` change.
- Boundary: an interior value that never crosses a wire and never re-mints a `QuantityFamily`. Carrier is a `Seq<ERational>` vector, not seven scalar fields and not the contract's integer `[ComplexValueObject]` `Dimension`, because the algebra is uniform exponent-vector arithmetic whose `Zip`/`ForAll`/`Filter` combinators express the group operators and the render fold directly, and whose structural equality makes it a `FrozenDictionary` key with no comparer; the rational carrier exists because a `Powf` sub-tree carries a transient fractional exponent the integer `Dimension` cannot hold, and a proven monomial with all-integral exponents is exactly that contract `Dimension`. Render routing zips the `SiAxis` rows against the exponent vector, so the glyph and its slot arrive together and no index arithmetic pairs them; sparse construction writes THROUGH an axis ROW, so an out-of-range slot is unspellable rather than guarded — the deleted `Find`-filter form silently dropped an invalid axis and minted `Dimensionless` from it, and the `ArgumentOutOfRangeException.ThrowIf*` pair that replaced it was exception control flow standing in for a type. Group law lives on the type (`operator *` addition, `operator /` subtraction, unary `operator -` the group inverse, `Pow(ERational)` scalar action, `MultiplicativeIdentity` zero vector); a parallel `DimensionAlgebra` static class, a hand-rolled element-wise `Equal`, and a `Pow(ERational.FromInt32(-1))` standing in for the inverse are the collapsed forms. `ERational` compares through `Equals`/`CompareTo` (the type ships no `==` operator), so every equality read spells `Equals`, never a phantom operator. Rank-7 is enforced through the Thinktecture `ValidateFactoryArguments` generator hook under `[ValidationError]`, so the generated `Validate` returns the domain fault directly and a hand-rolled `static ComputeFault Validate` — not a generator-recognized hook, and never run — stays the deleted form; a `Create` over a wrong-length `Seq` is rejected at admission while the rank-7-only interior algebra never trips it. Two display registers stay distinct: the symbolic `Format` physics-glyph projection (the `SiAxis` rows' `SymbolName` column, L·M·T·I·Θ·N·J) and the contract `Dimension.SiSymbol` SI-unit projection carry no string-equality obligation, both anchored to the same UnitsNet 7-vector in the same axis order — and the contract's own `DimensionAxis` roster stays separate because it carries an eighth non-SI display axis this vector has no slot for. Projection to a named family runs at the `UNITS_BRIDGE` gate, never inside the algebra, while `ToContract()` projects the CARRIER and asserts no name.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SiAxis {
    public static readonly SiAxis Length = new(0, "L", static dims => dims.Length);
    public static readonly SiAxis Mass = new(1, "M", static dims => dims.Mass);
    public static readonly SiAxis Time = new(2, "T", static dims => dims.Time);
    public static readonly SiAxis Current = new(3, "I", static dims => dims.Current);
    public static readonly SiAxis Temperature = new(4, "Θ", static dims => dims.Temperature);
    public static readonly SiAxis Amount = new(5, "N", static dims => dims.Amount);
    public static readonly SiAxis LuminousIntensity = new(6, "J", static dims => dims.LuminousIntensity);

    public static int Rank => Items.Count;

    public string SymbolName { get; }

    [UseDelegateFromConstructor]
    public partial int Read(BaseDimensions dims);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Seq<ERational>>]
public readonly partial struct DimensionMonomial :
    IMultiplyOperators<DimensionMonomial, DimensionMonomial, DimensionMonomial>,
    IDivisionOperators<DimensionMonomial, DimensionMonomial, DimensionMonomial>,
    IUnaryNegationOperators<DimensionMonomial, DimensionMonomial>,
    IMultiplicativeIdentity<DimensionMonomial, DimensionMonomial> {
    public static readonly DimensionMonomial Dimensionless =
        Create(toSeq(SiAxis.Items).Map(static _ => ERational.Zero));

    public static DimensionMonomial MultiplicativeIdentity => Dimensionless;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<ERational> exponents) {
        if (exponents.Count != SiAxis.Rank) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { $"<monomial-rank:{exponents.Count}≠{SiAxis.Rank}>" }));
            return;
        }

        exponents = exponents.Map(static e => e.ToLowestTerms());
    }

    public static DimensionMonomial Base(SiAxis axis) => Of((axis, ERational.One));

    public static DimensionMonomial Of(params ReadOnlySpan<(SiAxis Axis, ERational Exponent)> terms) {
        ERational[] slots = new ERational[SiAxis.Rank];
        Array.Fill(slots, ERational.Zero);
        foreach ((SiAxis axis, ERational exponent) in terms) {
            slots[axis.Key] += exponent;
        }

        return Create(toSeq(slots));
    }

    public static DimensionMonomial From(BaseDimensions dims) =>
        Create(toSeq(SiAxis.Items).Map(axis => ERational.FromInt32(axis.Read(dims))));

    public ERational this[SiAxis axis] => ToValue()[axis.Key];

    public bool IsDimensionless => ToValue().ForAll(static e => e.IsZero);

    public Option<Dimension> ToContract() =>
        ToValue().Traverse(static e => e.IsInteger() ? Some(e.ToInt32Checked()) : Option<int>.None)
            .Map(static axes => Dimension.Create(axes[0], axes[1], axes[2], axes[3], axes[4], axes[5], axes[6]))
            .As();

    public static DimensionMonomial operator *(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.ToValue().Zip(right.ToValue(), static (a, b) => a + b));

    public static DimensionMonomial operator /(DimensionMonomial left, DimensionMonomial right) =>
        Create(left.ToValue().Zip(right.ToValue(), static (a, b) => a - b));

    public static DimensionMonomial operator -(DimensionMonomial value) =>
        Create(value.ToValue().Map(static e => e.Negate()));

    public DimensionMonomial Pow(ERational exponent) =>
        Create(ToValue().Map(e => e * exponent));

    public string Format() =>
        toSeq(SiAxis.Items).Zip(ToValue())
            .Filter(static t => !t.Right.IsZero)
            .Map(static t => t.Right.Equals(ERational.One) ? t.Left.Symbol : $"{t.Left.Symbol}^{t.Right}") is { IsEmpty: false } factors
                ? string.Join(" ", factors)
                : "1";
}
```

## [03]-[DIMENSION_PROOF]

- Owner: `DimensionProof` the static fold entry and the recursive `Descend`; `DimensionContext` the admitted free-symbol binding, seated here because the fold is what resolves through it; `ComputeFault.DimensionMismatch` (code 2215) the arm every DIMENSIONAL disagreement rides, beside its two lane siblings for the refusals that are not dimensional.
- Cases: every `Entity.Number` leaf is `Dimensionless`; `Entity.Variable` reads its declared monomial from the context, a constant leaf (pi/e) discriminated by an empty `Vars` census; `Sumf`/`Minusf` demand identical operand monomials (the canonical defect this fold catches); `Mulf`/`Divf` fold through `*`/`/`; `Powf` demands a numeric-literal exponent and scales through `Pow`, covering `sqrt` as `Powf(arg, 1/2)` with no special case; `Absf` preserves dimension, `Signumf` erases it, `Logf` and every trig/unary `Function` demand dimensionless arguments through `IUnaryNode.NodeChild`; `atan2` needs no arm because the engine spells it `arctan(y/x)` and the homogeneous `Divf` ratio is already dimensionless; the `CalculusOperator` family carries `Derivativef` = `dim(f)/dim(x)`, `Integralf` = `dim(f)·dim(x)`, `Limitf` = `dim(f)`; the regime-switch family proves structurally — `Providedf(Expression, Predicate)` is `dim(Expression)` under a proven predicate, `Piecewise` is the `Homogeneous` fold of its case expressions (one dimension across every branch, the design-code piecewise law), and a predicate proves through the `ComparisonSign` arms (`Equalsf`/`Greaterf`/`GreaterOrEqualf`/`Lessf`/`LessOrEqualf` demand homogeneous operands, `Andf`/`Orf`/`Xorf`/`Impliesf`/`Notf` recurse) — so a slenderness-regime or spectrum-branch formula is provable end to end; any other `Statement`/`Set`/boolean node in a VALUE position short-circuits to the fault.
- Entry: `DimensionContext.Of(Map<string,string>)` admits the intent's declaration map — both columns are foreign text, so the symbol and the family key admit together through one applicative and every malformed pair reports at once; `Prove(SymbolicExpr, DimensionContext)` — one polymorphic entry returning `Validation<Error,DimensionMonomial>`, taking the forged-`default` gate from `SymbolicExpr.Tree` rather than re-testing it, discriminating on the carried `Entity` case, never a per-case public method; the accumulating `Validation` collects every `Sumf`-mismatch and undeclared symbol across the tree in one pass.
- Packages: AngouriMath (the `Entity` records pattern-matched positionally — `Sumf(Augend,Addend)`, `Minusf(Subtrahend,Minuend)`, `Mulf(Multiplier,Multiplicand)`, `Divf(Dividend,Divisor)`, `Powf(Base,Exponent)`, `Logf(Base,Antilogarithm)`, unary `(Argument)` behind `IUnaryNode.NodeChild`, `Entity.Variable.Name`, `Entity.Number.Rational.ERational`/`Real.EDecimal`, the per-node `Vars` census, `Stringize`), LanguageExt.Core (the accumulating `Validation` applicative, `Traverse`, `Seq`, `Distinct`, the proof-carrying `{ IsSome: true, Case: … }` probe), PeterO.Numbers (`ERational`), Thinktecture.Runtime.Extensions, UnitsNet (`QuantityFamily.Info.BaseDimensions` through the admitted context).
- Growth: the unary-function law covers every transcendental through the `IUnaryNode` floor without a per-name table; a new node family (the engine's hierarchy is closed at the pin) surfaces as the typed unmapped-node fault, never a silent fall-through; zero new entrypoint.
- Boundary: the fold reads `Entity` payloads through positional record patterns, never re-parsing the infix string, so the proof runs once over the canonical tree. Constant discrimination is structural — a `Variable` leaf with an empty `Vars` census is a constant and resolves `Dimensionless`, so no constant-name table exists; every other `Variable` resolves through `DimensionContext.Resolve`, and one absent from the context accumulates as `SymbolUndefined` — the arm whose recovery is "declare it", distinct from the `DimensionMismatch` arm whose recovery is "fix the algebra" — never a hidden dimensionless default. Every `Powf` exponent admits an exact `Rational` (subsuming `Integer` by inheritance) or a finite `Real` (`x^0.5`, lifted through `ERational.FromEDecimal`); a symbolic exponent has no static scale and accumulates as `DimensionMismatch`. Transcendental arms preserve dimension only for `Absf` and erase only for `Signumf`, so a blanket dimensionless-demanding rule that rejected `abs(force)` or `sign(moment)` is wrong. Foreign-node `_` arms produce a `ParseRejected` fault — a tree this lane declines to admit as a formula, never a dimensional disagreement and never a silent fall-through. Result is `Validation<Error,DimensionMonomial>` (the monoidal `Error` carrier every sibling lane uses; `ComputeFault` is not its own monoid, so the typed arm lifts onto `Error` through its `Fault` base), so one ill-formed compound surfaces every constituent mismatch at once. Proof never evaluates a number and never compiles a delegate — it is the gate the `Symbolic/lowering#LOWERING` compile fence runs behind.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record DimensionContext(Map<SymbolName, DimensionMonomial> Bindings) {
    public static Validation<Error, DimensionContext> Of(Map<string, string> declarations) =>
        toSeq(declarations.AsIterable())
            .Traverse(static pair => (Admit(pair.Key), Family(pair.Value))
                .Apply(static (symbol, family) => (SymbolName: symbol, Dimension: DimensionMonomial.From(family.Info.BaseDimensions))))
            .Map(static rows => new DimensionContext(rows.Fold(
                Map<SymbolName, DimensionMonomial>(),
                static (bindings, row) => bindings.Add(row.Symbol, row.Dimension))))
            .As();

    public Validation<Error, DimensionMonomial> Resolve(SymbolName symbol) =>
        Bindings.Find(symbol).ToValidation(
            new ComputeFault.SymbolUndefined($"<undeclared-symbol:{symbol.ToValue()}>"));

    static Validation<Error, SymbolName> Admit(string name) =>
        Op.Of(name: nameof(Admit)).AcceptValidated<SymbolName>(name).ToValidation();

    static Validation<Error, QuantityFamily> Family(string key) =>
        QuantityFamily.TryGet(key, out QuantityFamily? row)
            ? Success<Error, QuantityFamily>(row)
            : Fail<Error, QuantityFamily>(new ComputeFault.ParseRejected($"<symbolic-family-unknown:{key}>"));
}

// --- [ERRORS] --------------------------------------------------------------------------
public abstract partial record ComputeFault {
    [FaultCase(15)] public sealed partial record DimensionMismatch(string Detail) : ComputeFault(Detail);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionProof {
    public static Validation<Error, DimensionMonomial> Prove(SymbolicExpr expr, DimensionContext context) =>
        expr.Tree.Match(
            Succ: tree => Descend(tree, context),
            Fail: static error => Fail<Error, DimensionMonomial>(error));

    static Validation<Error, DimensionMonomial> Descend(Entity node, DimensionContext context) =>
        node switch {
            null =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: null node")),
            Entity.Number =>
                Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless),
            Entity.Variable variable =>
                toSeq(variable.Vars).IsEmpty
                    ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                    : context.Resolve(SymbolName.Create(variable.Name)),
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
            Entity.Derivativef derivative =>
                (Descend(derivative.Expression, context), Descend(derivative.Var, context)).Apply(static (f, x) => f / x),
            Entity.Integralf integral =>
                (Descend(integral.Expression, context), Descend(integral.Var, context)).Apply(static (f, x) => f * x),
            Entity.Limitf limit =>
                Descend(limit.Expression, context),
            Entity.Function and IUnaryNode unary =>
                Dimensionless(Seq(unary.NodeChild), context, node.GetType().Name),
            Entity.Providedf(Entity expression, Entity predicate) =>
                Predicate(predicate, context).Bind(_ => Descend(expression, context)),
            Entity.Piecewise piecewise when !toSeq(piecewise.Cases).IsEmpty =>
                toSeq(piecewise.Cases).Traverse(c => Predicate(c.Predicate, context)).Bind(_ =>
                    Homogeneous(toSeq(piecewise.Cases).Map(static c => c.Expression), context)).As(),
            Entity.Piecewise =>
                Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch("dimension: empty piecewise has no result dimension")),
            Entity.Statement or Entity.Set or Entity.Boolean =>
                Fail<Error, DimensionMonomial>(new ComputeFault.ParseRejected($"<non-numeric-node:{node.GetType().Name}>")),
            _ =>
                Fail<Error, DimensionMonomial>(new ComputeFault.ParseRejected($"<unmapped-node:{node.GetType().Name}>")),
        };

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
                Fail<Error, DimensionMonomial>(new ComputeFault.ParseRejected($"<unprovable-predicate:{predicate.GetType().Name}>")),
        };

    static Validation<Error, DimensionMonomial> Dimensionless(Seq<Entity> args, DimensionContext context, string name) =>
        args.Traverse(arg => Descend(arg, context)).Bind(dims =>
            dims.ForAll(static d => d.IsDimensionless)
                ? Success<Error, DimensionMonomial>(DimensionMonomial.Dimensionless)
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(
                    $"dimension: {name} requires dimensionless arguments, got {string.Join(", ", dims.Map(static d => d.Format()))}"))).As();

    static Validation<Error, DimensionMonomial> Homogeneous(Seq<Entity> addends, DimensionContext context) =>
        addends.Traverse(addend => Descend(addend, context)).Bind(static dims =>
            dims.Distinct() is var distinct && distinct.Count == 1 && distinct.Head is { IsSome: true, Case: DimensionMonomial only }
                ? Success<Error, DimensionMonomial>(only)
                : Fail<Error, DimensionMonomial>(new ComputeFault.DimensionMismatch(distinct.IsEmpty
                    ? "<homogeneous-empty>"
                    : $"<heterogeneous-sum:{string.Join(" vs ", distinct.Map(static d => d.Format()))}>"))).As();

    static Validation<Error, ERational> Literal(Entity exponent) =>
        exponent switch {
            null =>
                Fail<Error, ERational>(new ComputeFault.DimensionMismatch("dimension: null power exponent")),
            Entity.Number.Rational rational =>
                Success<Error, ERational>(rational.ERational),
            Entity.Number.Real real when real.EDecimal.IsFinite =>
                Success<Error, ERational>(ERational.FromEDecimal(real.EDecimal)),
            _ =>
                Fail<Error, ERational>(new ComputeFault.DimensionMismatch($"dimension: non-literal power exponent {exponent.Stringize()}")),
        };
}
```

## [04]-[UNITS_BRIDGE]

- Owner: `DimensionVerdict` the `[Union]` result binding the PROVED expression's content key to its monomial and, where the roster names it, its candidate `QuantityFamily` set; `DimensionAdmission` the static projection that censuses, proves, and matches against the `Symbolic/units#DIMENSIONAL_LAW` SI baseline.
- Cases: `DimensionVerdict` cases `Named(Subject, Dimension, Candidates)` — the roster carries one or more rows at that dimension — and `Unnamed(Subject, Dimension)` — the formula is dimensionally sound and the admitted roster names nothing at that dimension; bound free symbols arrive from the `Runtime/admission#DISPATCH_SPINE` `ComputeIntent.SymbolicProject.Dimensions` map, each carrying its declared `QuantityFamily` KEY.
- Law: the verdict is evidence about ONE expression. `Subject` carries the proved formula's content key, so `Symbolic/lowering#LOWERING` binds a compile to the proof that admitted it and a verdict minted for a different tree is a typed refusal — the gate that page's Law always claimed and, without this column, could only claim.
- Law: dimensional soundness and quantity NAMING are two questions, and only the first is the proof's. Curvature reciprocal-length and per-length stiffness are sound intermediates the admitted roster carries no row for, so the verdict reports `Unnamed` with its proven monomial and downstream admission decides whether an unnamed result is admissible for its own consumer; faulting there rejected formulas whose algebra was never in doubt.
- Law: the dimension-to-family map is NOT injective, and the `GroupBy` table detects EVERY collision at static construction — a monomial with two or more rows arrives as one candidate `Seq`, and `Unique` is `Some` only at exactly one. Enumerating the colliding pairs in prose goes stale the moment a `QuantityFamily` row lands beside an existing dimension, so the table is the roster of collisions and the prose states only that they are preserved. An `IsAmbiguous` boolean beside `Candidates` restated a count the set already carries and is the deleted knob.
- Entry: `Admit(SymbolicExpr, DimensionContext)` — `Validation<Error,DimensionVerdict>` composing the `FreeSymbols` census, `DimensionProof.Prove`, and the row match; the census fails fast on any undeclared symbol, the proof then accumulates every structural mismatch, and the match is TOTAL, so it rides `Map` rather than a `Validation` arm no input reaches; no `IQuantity` is ever constructed, admission running before any value materializes.
- Packages: UnitsNet (the frozen `QuantityFamily.Info.BaseDimensions` rows, never re-minted), LanguageExt.Core (`Validation`, `Traverse`, `Map`, `Seq`, `Option`, the census filter), Thinktecture.Runtime.Extensions (`QuantityFamily.Items`, the generated union `Switch`, the `DimensionMonomial` dictionary key), AngouriMath (`SymbolicExpr` input, the `FreeSymbols` set driving the census), BCL inbox (`FrozenDictionary`, `UInt128`).
- Growth: a new admitted result dimension is one `QuantityFamily` row on `Symbolic/units#QUANTITY_TABLE` — the match table groups `Items` by `DimensionMonomial` at static construction, so a row added there turns an `Unnamed` verdict into a `Named` one (or joins an existing candidate set) with zero edit here; a richer verdict is one field on the owning case; zero new surface.
- Boundary: symbolic admission consumes the declared SI `QuantityFamily` vocabulary without constructing `IQuantity`. `FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>>` preserves the non-injective match whole; full-roster UnitsNet discovery and first-match scans are rejected. SymbolName census fails before the accumulating proof, and the verdict is what `Symbolic/lowering#LOWERING` consumes — the monomial stamps the compiled carrier's result dimension and `Unique` its result family, so a compiled formula reports what its output MEANS instead of leaving the proof unread.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionVerdict {
    private DimensionVerdict() { }

    public sealed record Named(UInt128 Subject, DimensionMonomial Dimension, Seq<QuantityFamily> Candidates) : DimensionVerdict;

    public sealed record Unnamed(UInt128 Subject, DimensionMonomial Dimension) : DimensionVerdict;

    public UInt128 Proved => Switch(
        named: static verdict => verdict.Subject,
        unnamed: static verdict => verdict.Subject);

    public DimensionMonomial Monomial => Switch(
        named: static verdict => verdict.Dimension,
        unnamed: static verdict => verdict.Dimension);

    public Seq<QuantityFamily> Families => Switch(
        named: static verdict => verdict.Candidates,
        unnamed: static _ => Seq<QuantityFamily>());

    public Option<QuantityFamily> Unique =>
        Families is { Count: 1 } only ? only.Head : Option<QuantityFamily>.None;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DimensionAdmission {
    static readonly FrozenDictionary<DimensionMonomial, Seq<QuantityFamily>> Table =
        QuantityFamily.Items
            .GroupBy(static row => DimensionMonomial.From(row.Info.BaseDimensions))
            .ToFrozenDictionary(static g => g.Key, static g => toSeq(g));

    public static Validation<Error, DimensionVerdict> Admit(SymbolicExpr expr, DimensionContext context) =>
        Census(expr, context).Bind(_ => DimensionProof.Prove(expr, context)).Map(monomial => Match(expr.ContentKey, monomial)).As();

    static Validation<Error, Unit> Census(SymbolicExpr expr, DimensionContext context) =>
        expr.FreeSymbols.Filter(symbol => !context.Bindings.ContainsKey(symbol)) is { IsEmpty: false } undeclared
            ? Fail<Error, Unit>(new ComputeFault.SymbolUndefined(
                $"<undeclared-symbols:{string.Join(",", undeclared.Map(static s => s.ToValue()))}>"))
            : Success<Error, Unit>(unit);

    static DimensionVerdict Match(UInt128 subject, DimensionMonomial monomial) =>
        Table.TryGetValue(monomial, out Seq<QuantityFamily> families)
            ? new DimensionVerdict.Named(subject, monomial, families)
            : new DimensionVerdict.Unnamed(subject, monomial);
}
```
