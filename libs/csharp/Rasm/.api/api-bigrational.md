# [RASM_API_BIGRATIONAL]

`ExtendedNumerics.BigRational` supplies the exact-rational oracle the Geometry predicate
floor is proved against: two `BigInteger`-backed value structs — `Fraction` (the canonical
flat `Numerator`/`Denominator` rational) and `BigRational` (a mixed `WholePart` +
`FractionalPart Fraction`) — carrying exact `Sign`, sign canonicalization (`NormalizeSign`/
`Reduce`/`Simplify`), the full arithmetic/operator/comparison algebra, and the `Mediant`
Stern-Brocot bracketing step. It is the topmost, infinitely-precise tier of the predicate
precision ladder: interior `double` (`NumericsPolicy`) -> `TYoshimura.DoubleDouble` 106-bit
middle tier -> `Expansion` sign-exact expansion -> `Fraction` exact-rational adjudication.
Orient2D/Orient3D/InCircle/InSphere and the four implicit-point predicates (OrientLPI/
OrientTPI/InCircleLPI/InSphereTPI) reduce their determinant to a `Fraction.Sign` query when
every faster tier reports an indeterminate sign. It does NOT implement `INumber<T>` or any
`System.Numerics` generic-math interface — the only contracts are `IComparable`/`IComparable<T>`/
`IEquatable<T>` (plus `IEqualityComparer<Fraction>` on `Fraction`), so a rail composes it as a
hand-typed value, never through `INumberBase<T>`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ExtendedNumerics.BigRational`
- package: `ExtendedNumerics.BigRational`
- version: `3000.0.2.132`
- license: MIT (Adam White / ExtendedNumerics)
- assembly: `ExtendedNumerics.BigRational`
- namespace: `ExtendedNumerics`
- target: multi-target (`net45`, `net46`, `net48`, `netstandard2.1`, `netcoreapp3.1`, `net5.0`, `net6.0`, `net7.0`); the `net10.0` consumer binds `lib/net7.0` (highest available)
- asset: pure-managed runtime library, AnyCPU, no native runtime; `System.Numerics.BigInteger` is the only dependency
- abi: no generic-math interfaces — `BigRational`/`Fraction` are plain `struct` values over `IComparable`/`IEquatable<T>`; not `INumber<T>`, not `ISpanParsable<T>`, not `IFormattable` beyond the explicit `ToString` overloads
- rail: exact-precision oracle

## [02]-[PUBLIC_TYPES]

[VALUE_STRUCTS]: the two exact-rational carriers (namespace `ExtendedNumerics`)
- rail: exact-precision oracle

`Fraction` is the canonical flat rational the predicate oracle reduces to; `BigRational` is the
mixed-radix carrier whose `FractionalPart` is a `Fraction`. The two interconvert by implicit
operator. `Fraction.DecimalUInt32` is a private nested decimal-decomposition helper with no
public surface.

| [INDEX] | [SYMBOL]      | [PACKAGE_ROLE]            | [CAPABILITY]                                                                       |
| :-----: | :------------ | :------------------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `Fraction`    | flat exact rational       | `struct`; `BigInteger Numerator`/`Denominator`; `Sign`/`IsZero`/`IsOne`; full algebra + `Mediant`/`Simplify`/`Reciprocal`/`DivRem` |
|  [02]   | `BigRational` | mixed whole+fraction rational | `struct`; `BigInteger WholePart` + `Fraction FractionalPart`; `Sign`/`IsZero`; full algebra + `Reduce`/`NormalizeSign`/`NthRoot` |

[STATIC_ANCHORS]: dependency-free constant values
- rail: exact-precision oracle

| [INDEX] | [SYMBOL]                                            | [VALUE]                | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :--------------------- | :------------------------------------ |
|  [01]   | `BigRational.Zero` / `One` / `MinusOne`             | static `BigRational`   | additive/multiplicative identity anchors |
|  [02]   | `Fraction.Zero` / `One` / `MinusOne` / `OneHalf`    | static `Fraction`      | rational identity anchors + `1/2`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Fraction` construction + canonicalization (the oracle's flat tier)
- rail: exact-precision oracle

`Fraction(BigInteger numerator, BigInteger denominator)` is the exact construction path the
predicate determinants build directly from the `BigInteger`-promoted ordinates; the `float`/
`double`/`decimal` constructors decompose the IEEE bit pattern exactly (no rounding). `Sign`
normalizes the sign onto the numerator first, so a negative denominator never flips the sign
query. `Simplify`/`ReduceToProperFraction` are the canonical reducers.

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]    | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `new Fraction(BigInteger numerator, BigInteger denominator)` | constructor | exact rational from a `BigInteger` pair                |
|  [02]   | `new Fraction(double)` / `new Fraction(float)` / `new Fraction(decimal)` | constructor | exact IEEE/decimal decomposition (lossless)   |
|  [03]   | `Fraction.Parse(string)`                               | static factory  | parses a rational literal                                |
|  [04]   | `Fraction.Sign`                                        | instance prop   | `int` exact sign via `NormalizeSign(this).Numerator.Sign` |
|  [05]   | `Fraction.Simplify(Fraction)`                          | static          | reduces to lowest terms                                  |
|  [06]   | `Fraction.ReduceToProperFraction(Fraction)`            | static `BigRational` | splits into whole + proper-fractional `BigRational` |
|  [07]   | `Fraction.Reciprocal(Fraction)`                        | static          | `1/x` exact                                              |
|  [08]   | `Fraction.Mediant(Fraction left, Fraction right)`      | static          | Stern-Brocot mediant `(a+c)/(b+d)` — bracketing step     |

[ENTRYPOINT_SCOPE]: `Fraction` exact algebra + comparison
- rail: exact-precision oracle

Every binary op has both a static method (`Add`/`Subtract`/`Multiply`/`Divide`) and an
operator (`+`/`-`/`*`/`/`/`%`); comparison is total (`<`/`<=`/`>`/`>=`/`==`/`!=`) plus
`Compare(left,right)` and `CompareTo`. `DivRem` returns quotient and `out` remainder in one
exact step. All arithmetic is infinite-precision over `BigInteger`.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Fraction.Add/Subtract/Multiply/Divide(Fraction, Fraction)`     | static         | exact field arithmetic (operator twins `+ - * /`)  |
|  [02]   | `Fraction.Remainder(Fraction, Fraction)` / `operator %`         | static         | exact rational remainder                           |
|  [03]   | `Fraction.DivRem(Fraction dividend, Fraction divisor, out Fraction remainder)` | static | exact quotient + remainder in one call    |
|  [04]   | `Fraction.Pow(Fraction, BigInteger)` / `Pow(Fraction, Fraction)` | static        | integer / rational exponent power                  |
|  [05]   | `Fraction.Sqrt(Fraction, int precision = 30)` / `NthRoot(Fraction, BigInteger root, int precision = 30)` | static | precision-bounded root (NOT exact — `precision` digits) |
|  [06]   | `Fraction.Abs/Negate(Fraction)`                                 | static         | absolute value / negation (operator `-` twin)      |
|  [07]   | `Fraction.GreatestCommonDivisor/LeastCommonDenominator(Fraction, Fraction)` | static | exact GCD / LCD                            |
|  [08]   | `Fraction.Compare(Fraction, Fraction)` / `CompareTo` / `==`..`>=` | static/instance | total exact ordering                            |
|  [09]   | `Fraction.Log(Fraction)`                                        | static `double`| natural log (lossy `double` — boundary readout)   |

[ENTRYPOINT_SCOPE]: `BigRational` construction + mixed-radix algebra
- rail: exact-precision oracle

`BigRational(BigInteger numerator, BigInteger denominator)` and the three-arg
`(whole, numerator, denominator)` build the mixed carrier; `GetImproperFraction()` collapses
the mixed form back to a flat `Fraction`, and the implicit `BigRational -> Fraction` operator
is the canonical bridge into the oracle's flat tier. `Reduce`/`NormalizeSign` canonicalize.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `new BigRational(BigInteger numerator, BigInteger denominator)` | constructor    | mixed rational from a pair                          |
|  [02]   | `new BigRational(BigInteger whole, BigInteger numerator, BigInteger denominator)` | constructor | explicit whole + fractional parts        |
|  [03]   | `new BigRational(double/float/decimal)`                         | constructor    | exact IEEE/decimal decomposition                    |
|  [04]   | `BigRational.WholePart` / `FractionalPart`                      | instance prop  | `BigInteger` whole + `Fraction` fractional remainder|
|  [05]   | `BigRational.GetImproperFraction()`                             | instance       | collapses the mixed form to a flat `Fraction`       |
|  [06]   | `(Fraction)bigRational` (implicit operator)                     | conversion     | bridge into the oracle flat tier                    |
|  [07]   | `BigRational.Sign` / `IsZero`                                   | instance prop  | exact sign / zero test                              |
|  [08]   | `BigRational.Add/Subtract/Multiply/Divide(BigRational, …)` + `+ - * / %` | static/operator | exact mixed-radix arithmetic               |
|  [09]   | `BigRational.Add/Subtract/Multiply/Divide(Fraction, Fraction)`  | static `BigRational` | flat-input overloads returning the mixed carrier |
|  [10]   | `BigRational.Pow(BigRational, BigInteger)` / `Sqrt(BigRational)` / `NthRoot(BigRational, int root, int precision = 30)` | static | exact integer power / `Sqrt` (no precision arg) / `precision`-bounded `NthRoot` |
|  [11]   | `BigRational.Mod(BigRational, BigRational)` / `Remainder(BigInteger, BigInteger)` | static | modulo / integer remainder              |
|  [12]   | `BigRational.Abs/Negate(BigRational)` + `GreatestCommonDivisor/LeastCommonDenominator` | static | abs / negate / GCD / LCD              |
|  [13]   | `BigRational.Reduce(BigRational)` / `NormalizeSign(BigRational)` | static         | lowest-terms reduction / sign canonicalization      |
|  [14]   | `BigRational.Compare` / `CompareTo` / `==`..`>=` / `Parse(string)` | static/instance | total exact ordering + literal parse           |

[ENTRYPOINT_SCOPE]: conversions (boundary readout)
- rail: exact-precision oracle

Widening integer conversions (`byte`..`ulong`, `BigInteger`) are implicit and lossless; the
floating/`decimal` conversions are explicit because they round, and are the lossy boundary
readout only — never an interior predicate step.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]   | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `implicit operator Fraction(byte/sbyte/short/ushort/int/uint/long/ulong/BigInteger)` | conversion | lossless widening into `Fraction`     |
|  [02]   | `implicit operator BigRational(…integers/BigInteger)`           | conversion     | lossless widening into `BigRational`                |
|  [03]   | `explicit operator Fraction(float/double/decimal)` / `BigRational(…)` | conversion | exact IEEE decomposition (explicit by API)    |
|  [04]   | `explicit operator double/decimal(Fraction)` / `(BigRational)`  | conversion     | lossy readout to floating/`decimal`                 |
|  [05]   | `explicit operator BigInteger(Fraction)`                        | conversion     | truncating integer readout                          |
|  [06]   | `ToString()` / `ToString(string format[, IFormatProvider])`     | instance       | culture/format string render                        |

## [04]-[IMPLEMENTATION_LAW]

[ORACLE_PROFILE]:
- canonical flat carrier: `Fraction` (`BigInteger Numerator`/`Denominator`) — the type the predicate determinant reduces to; `BigRational` is the mixed convenience carrier and collapses to `Fraction` via `GetImproperFraction()` / the implicit operator.
- sign query: `Fraction.Sign` / `BigRational.Sign` normalize the sign onto the numerator (`NormalizeSign`) first, so a negative-denominator construction never inverts the predicate verdict; the predicate reads `Sign`, never the `double`/`decimal` readout.
- exactness boundary: arithmetic (`Add`/`Subtract`/`Multiply`/`Divide`/`%`/`DivRem`/`Pow(int)`) is infinite-precision over `BigInteger`; `Sqrt`/`NthRoot`/`Log` are NOT exact (`precision`-digit or `double`), so they are forbidden inside a predicate adjudication and confined to non-predicate readout.
- no generic math: the structs implement `IComparable`/`IComparable<T>`/`IEquatable<T>` (and `IEqualityComparer<Fraction>` on `Fraction`) only — a rail must NOT bind `INumber<BigRational>`, `INumberBase<T>`, or `ISpanParsable<T>`; the comparison/equality contract is the whole generic surface.

[LOCAL_ADMISSION]:
- `BigRational`/`Fraction` are the topmost predicate tier (`.planning/Numerics/predicates#PRECISION_LADDER`), reached only when interior `double`, the `TYoshimura.DoubleDouble` 106-bit middle tier, and the `Expansion` exact branch all report an indeterminate determinant sign — never on the fast path, because each oracle adjudication allocates `BigInteger` heap.
- the predicate builds the determinant from `BigInteger`-promoted exact ordinates straight into `new Fraction(num, den)`, reads `Fraction.Sign`, and discards the value; the oracle verdict is a sign (`-1`/`0`/`+1`), folded into the `Orient`/`InCircle` result, never materialized as a `double`.
- the exact parametric-ordering keys (`IntersectOp` crossing `t`-keys, arrangement cell ordering) are totally ordered by `Fraction.Compare`/`CompareTo`, breaking a near-degenerate ordering tie without a floating epsilon; `Fraction.Mediant` — the Stern-Brocot rational strictly between two neighbours — is an available bracketing/fractional-indexing primitive no current consumer composes, documented as a member rather than assigned a consumer home.
- conversions out of the oracle use the explicit `double`/`decimal`/`BigInteger` operators only at a reporting boundary; an interior step that needs a value stays in `Fraction`/`BigRational`.

[STACKING_LAW]:
- precision ladder: this oracle sits above `TYoshimura.DoubleDouble` (`api-doubledouble`, the 106-bit hi/lo middle tier whose FMA `TwoProduct` + Knuth `TwoSum` transforms match the kernel) and the in-house `Expansion` sign-exact branch; the kernel escalates tier-by-tier and only the indeterminate residue reaches `Fraction.Sign`, so the BigInteger cost is paid on the measure-zero degenerate set, never the bulk.
- predicate floor: `Predicate.Orient2D/Orient3D/InCircle/InSphere` and the implicit-point `OrientLPI/OrientTPI/InCircleLPI/InSphereTPI` are PROVED against the `Fraction` oracle in the law-matrix — the oracle is both the runtime last-resort adjudicator and the test-time ground truth the faster tiers are differentially fuzzed against (`CsCheck` random determinants compared `double`/`Expansion`/`DoubleDouble` vs `Fraction.Sign`).
- arrangement / intersection keys: `Arrangement` planar-overlay cell ordering and `IntersectOp` exact parametric `t`-keys carry a `Fraction` ordering key when an `Lpi`/`Tpi` implicit point's coordinate is only known as an exact rational; `Fraction.Compare`/`CompareTo` totally orders them, so the cell complex never depends on a floating tolerance for its combinatorial structure.

[RAIL_LAW]:
- Package: `ExtendedNumerics.BigRational`
- Owns: exact-rational adjudication — the flat `Fraction` `BigInteger` rational, the mixed `BigRational` carrier, exact arithmetic/comparison/`Sign`/`NormalizeSign`/`Reduce`/`Mediant`, and lossless integer + exact IEEE-decomposition construction
- Accept: a predicate determinant reduced to `new Fraction(num, den)` then `Sign`; an exact parametric/cell ordering key compared via `Compare`/`CompareTo`; the oracle as differential ground truth in the predicate law-matrix
- Reject: using the oracle on the fast path before the cheaper tiers escalate; `Sqrt`/`NthRoot`/`Log` inside a predicate (not exact); binding `INumber<T>`/`INumberBase<T>`/`ISpanParsable<T>` (not implemented); a `double`/`decimal` readout standing in for the exact `Sign` verdict; a hand-rolled `BigInteger` numerator/denominator rational beside this owner
