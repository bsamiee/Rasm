# [RASM_API_BIGRATIONAL]

`ExtendedNumerics.BigRational` owns the exact-rational oracle at the Geometry predicate floor through two `BigInteger`-backed value structs. Both carry infinite-precision arithmetic, total comparison, and a `NormalizeSign`-canonicalized `Sign` a predicate reads once a faster tier reports an indeterminate sign. This exact tier is the runtime adjudicator and differential-fuzzing ground truth beneath the `double`, `ddouble`, and `Expansion` tiers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ExtendedNumerics.BigRational`
- package: `ExtendedNumerics.BigRational` (MIT)
- assembly: `ExtendedNumerics.BigRational`
- namespace: `ExtendedNumerics`
- target: multi-target; the `net10.0` consumer binds `lib/net7.0` as highest available
- asset: pure-managed AnyCPU, `System.Numerics.BigInteger` the sole dependency
- abi: plain `struct` values implementing `IComparable`, `IComparable<T>`, `IEquatable<T>`, and `IEqualityComparer<Fraction>`
- rail: exact-precision oracle

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two exact-rational value structs (namespace `ExtendedNumerics`)

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------ | :------------ | :----------------------------------------------- |
|  [01]   | `Fraction`    | struct        | flat `BigInteger` numerator/denominator rational |
|  [02]   | `BigRational` | struct        | mixed `BigInteger` whole part plus `Fraction`    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: identity and unit anchors

| [INDEX] | [SURFACE]              | [SHAPE] | [CAPABILITY]                |
| :-----: | :--------------------- | :------ | :-------------------------- |
|  [01]   | `Fraction.Zero`        | static  | additive identity `0`       |
|  [02]   | `Fraction.One`         | static  | multiplicative identity `1` |
|  [03]   | `Fraction.MinusOne`    | static  | negative unit `-1`          |
|  [04]   | `Fraction.OneHalf`     | static  | rational half `1/2`         |
|  [05]   | `BigRational.Zero`     | static  | additive identity `0`       |
|  [06]   | `BigRational.One`      | static  | multiplicative identity `1` |
|  [07]   | `BigRational.MinusOne` | static  | negative unit `-1`          |

[ENTRYPOINT_SCOPE]: `Fraction` construction and canonicalization

`Fraction(BigInteger, BigInteger)` builds the predicate determinant from promoted ordinates; `Mediant` returns the Stern-Brocot bracketing value `(a+c)/(b+d)`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `Fraction(BigInteger, BigInteger)`                         | ctor     | exact rational pair         |
|  [02]   | `Fraction(double)`                                         | ctor     | exact IEEE decomposition    |
|  [03]   | `Fraction(float)`                                          | ctor     | exact IEEE decomposition    |
|  [04]   | `Fraction(decimal)`                                        | ctor     | exact decimal decomposition |
|  [05]   | `Fraction.Parse(string)`                                   | factory  | rational literal parse      |
|  [06]   | `Fraction.Sign`                                            | property | exact normalized sign       |
|  [07]   | `Fraction.Simplify(Fraction)`                              | static   | lowest-terms reduction      |
|  [08]   | `Fraction.ReduceToProperFraction(Fraction) -> BigRational` | static   | proper mixed reduction      |
|  [09]   | `Fraction.Reciprocal(Fraction)`                            | static   | exact reciprocal            |
|  [10]   | `Fraction.Mediant(Fraction, Fraction)`                     | static   | rational bracketing         |

[ENTRYPOINT_SCOPE]: `Fraction` exact algebra and comparison

Every binary arithmetic operation carries a static method and an operator twin; comparison is total through the relational and equality operators, `Compare`, and `CompareTo`, and `DivRem` returns its exact quotient with an `out` remainder.

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]             |
| :-----: | :---------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `Fraction.Add(Fraction, Fraction)`                    | static   | exact addition           |
|  [02]   | `Fraction.Subtract(Fraction, Fraction)`               | static   | exact subtraction        |
|  [03]   | `Fraction.Multiply(Fraction, Fraction)`               | static   | exact multiplication     |
|  [04]   | `Fraction.Divide(Fraction, Fraction)`                 | static   | exact division           |
|  [05]   | `Fraction.Remainder(Fraction, Fraction)`              | static   | exact rational remainder |
|  [06]   | `Fraction.DivRem(Fraction, Fraction, out Fraction)`   | static   | quotient plus remainder  |
|  [07]   | `Fraction.Pow(Fraction, BigInteger)`                  | static   | integer exponent         |
|  [08]   | `Fraction.Pow(Fraction, Fraction)`                    | static   | rational exponent        |
|  [09]   | `Fraction.Sqrt(Fraction, int)`                        | static   | precision-bounded root   |
|  [10]   | `Fraction.NthRoot(Fraction, BigInteger, int)`         | static   | precision-bounded root   |
|  [11]   | `Fraction.Abs(Fraction)`                              | static   | absolute value           |
|  [12]   | `Fraction.Negate(Fraction)`                           | static   | exact negation           |
|  [13]   | `Fraction.GreatestCommonDivisor(Fraction, Fraction)`  | static   | exact GCD                |
|  [14]   | `Fraction.LeastCommonDenominator(Fraction, Fraction)` | static   | exact LCD                |
|  [15]   | `Fraction.Compare(Fraction, Fraction) -> int`         | static   | total ordering           |
|  [16]   | `Fraction.CompareTo(Fraction) -> int`                 | instance | total ordering           |
|  [17]   | `Fraction.operator < <= > >= == !=`                   | operator | total ordering, equality |
|  [18]   | `Fraction.Log(Fraction) -> double`                    | static   | lossy natural logarithm  |

[ENTRYPOINT_SCOPE]: `BigRational` construction and mixed-radix algebra

`BigRational` builds from a `(numerator, denominator)` pair or an explicit `(whole, numerator, denominator)` triple; the flat-input arithmetic overloads promote a `Fraction` pair into the mixed carrier.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `BigRational(BigInteger, BigInteger)`                          | ctor     | mixed rational pair         |
|  [02]   | `BigRational(BigInteger, BigInteger, BigInteger)`              | ctor     | explicit mixed components   |
|  [03]   | `BigRational(double)`                                          | ctor     | exact IEEE decomposition    |
|  [04]   | `BigRational(float)`                                           | ctor     | exact IEEE decomposition    |
|  [05]   | `BigRational(decimal)`                                         | ctor     | exact decimal decomposition |
|  [06]   | `BigRational.WholePart`                                        | property | `BigInteger` whole part     |
|  [07]   | `BigRational.FractionalPart`                                   | property | `Fraction` remainder        |
|  [08]   | `BigRational.GetImproperFraction() -> Fraction`                | instance | flat `Fraction` collapse    |
|  [09]   | `implicit operator Fraction(BigRational)`                      | operator | flat-tier bridge            |
|  [10]   | `BigRational.Sign`                                             | property | exact normalized sign       |
|  [11]   | `BigRational.IsZero`                                           | property | exact zero test             |
|  [12]   | `BigRational.Add(BigRational, BigRational)`                    | static   | exact addition              |
|  [13]   | `BigRational.Subtract(BigRational, BigRational)`               | static   | exact subtraction           |
|  [14]   | `BigRational.Multiply(BigRational, BigRational)`               | static   | exact multiplication        |
|  [15]   | `BigRational.Divide(BigRational, BigRational)`                 | static   | exact division              |
|  [16]   | `BigRational.Remainder(BigRational, BigRational)`              | static   | exact rational remainder    |
|  [17]   | `BigRational.Add(Fraction, Fraction) -> BigRational`           | static   | mixed-carrier result        |
|  [18]   | `BigRational.Subtract(Fraction, Fraction) -> BigRational`      | static   | mixed-carrier result        |
|  [19]   | `BigRational.Multiply(Fraction, Fraction) -> BigRational`      | static   | mixed-carrier result        |
|  [20]   | `BigRational.Divide(Fraction, Fraction) -> BigRational`        | static   | mixed-carrier result        |
|  [21]   | `BigRational.Pow(BigRational, BigInteger)`                     | static   | exact integer power         |
|  [22]   | `BigRational.Sqrt(BigRational)`                                | static   | root without precision arg  |
|  [23]   | `BigRational.NthRoot(BigRational, int, int)`                   | static   | precision-bounded root      |
|  [24]   | `BigRational.Mod(BigRational, BigRational)`                    | static   | exact modulo                |
|  [25]   | `BigRational.Remainder(BigInteger, BigInteger)`                | static   | exact integer remainder     |
|  [26]   | `BigRational.Abs(BigRational)`                                 | static   | absolute value              |
|  [27]   | `BigRational.Negate(BigRational)`                              | static   | exact negation              |
|  [28]   | `BigRational.GreatestCommonDivisor(BigRational, BigRational)`  | static   | exact GCD                   |
|  [29]   | `BigRational.LeastCommonDenominator(BigRational, BigRational)` | static   | exact LCD                   |
|  [30]   | `BigRational.Reduce(BigRational)`                              | static   | lowest-terms reduction      |
|  [31]   | `BigRational.NormalizeSign(BigRational)`                       | static   | sign canonicalization       |
|  [32]   | `BigRational.Compare(BigRational, BigRational) -> int`         | static   | total ordering              |
|  [33]   | `BigRational.CompareTo(BigRational) -> int`                    | instance | total ordering              |
|  [34]   | `BigRational.operator < <= > >= == !=`                         | operator | total ordering, equality    |
|  [35]   | `BigRational.Parse(string)`                                    | factory  | rational literal parse      |

[ENTRYPOINT_SCOPE]: conversions

Conversions in from `float`, `double`, and `decimal` are explicit but exact; conversions out to floating types round, and `BigInteger` conversion truncates.

[INTEGER_WIDENING]: `byte` `sbyte` `short` `ushort` `int` `uint` `long` `ulong` `BigInteger` widen implicitly and losslessly into both `Fraction` and `BigRational`.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------- | :------- | :-------------------------- |
|  [01]   | `explicit operator Fraction(float)`      | operator | exact IEEE decomposition    |
|  [02]   | `explicit operator Fraction(double)`     | operator | exact IEEE decomposition    |
|  [03]   | `explicit operator Fraction(decimal)`    | operator | exact decimal decomposition |
|  [04]   | `explicit operator BigRational(float)`   | operator | exact IEEE decomposition    |
|  [05]   | `explicit operator BigRational(double)`  | operator | exact IEEE decomposition    |
|  [06]   | `explicit operator BigRational(decimal)` | operator | exact decimal decomposition |
|  [07]   | `explicit operator double(Fraction)`     | operator | lossy floating readout      |
|  [08]   | `explicit operator decimal(Fraction)`    | operator | lossy decimal readout       |
|  [09]   | `explicit operator double(BigRational)`  | operator | lossy floating readout      |
|  [10]   | `explicit operator decimal(BigRational)` | operator | lossy decimal readout       |
|  [11]   | `explicit operator BigInteger(Fraction)` | operator | truncating integer readout  |
|  [12]   | `ToString()`                             | instance | default render              |
|  [13]   | `ToString(string)`                       | instance | format-string render        |
|  [14]   | `ToString(string, IFormatProvider)`      | instance | culture-aware render        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Canonical carrier: `Fraction` stores the `BigInteger` numerator and denominator predicate determinants consume; `BigRational` is the mixed carrier, collapsing to the flat tier through `GetImproperFraction()` or its implicit `Fraction` conversion.
- Sign query: `Fraction.Sign` and `BigRational.Sign` call `NormalizeSign` before reading the numerator, so a negative denominator never inverts a predicate verdict; predicates read `Sign`, never a floating or decimal readout.
- Exactness boundary: `Add`, `Subtract`, `Multiply`, `Divide`, `%`, `DivRem`, and integer `Pow` are infinite-precision over `BigInteger`; `Sqrt`, `NthRoot`, and `Log` return precision-bounded or `double` results and stay outside predicate adjudication.
- Generic surface: the structs expose no `INumber<T>` generic-math dispatch, so a consumer composes hand-typed values.

[STACKING]:
- `TYoshimura.DoubleDouble`(`.api/api-doubledouble.md`): the 106-bit `ddouble` tier sits one rung below the oracle; only a determinant sign unresolved at 106 bits promotes to `Fraction.Sign`, confining the `BigInteger` cost to degenerate cases.
- Predicate floor: the `Predicate` family (`Orient2D`/`Orient3D`/`InCircle`/`InSphere` and their axis-projected and implicit-point variants) reduces each determinant to `Fraction.Sign` as both runtime adjudicator and `CsCheck` differential ground truth against the `double`, `Expansion`, and `DoubleDouble` tiers.
- Ordering keys: `Arrangement` planar-overlay cells and `IntersectOp` parametric `t` keys carry `Fraction.Compare` or `CompareTo` when an `Lpi` or `Tpi` coordinate exists only as an exact rational, keeping the cell complex free of any floating tolerance.

[LOCAL_ADMISSION]:
- Tier entry: the oracle adjudicates only after interior `double`, the `ddouble` tier, and the `Expansion` branch report an indeterminate determinant sign; each adjudication allocates `BigInteger`, so the faster tiers keep the ordinary path.
- Verdict: the determinant promotes exact ordinates to `BigInteger`, constructs `new Fraction(num, den)`, reads `Fraction.Sign`, and folds `-1`, `0`, or `+1` into the `Orient` or `InCircle` result without materializing a `double`.
- Interior form: explicit `double`, `decimal`, and `BigInteger` conversions occur only at reporting boundaries; interior values stay `Fraction` or `BigRational`.

[RAIL_LAW]:
- Package: `ExtendedNumerics.BigRational`
- Owns: exact-rational adjudication through the flat `Fraction` ratio and mixed `BigRational` carrier — infinite-precision arithmetic, total comparison, `NormalizeSign`-canonicalized `Sign`, `Reduce`, `Mediant`, lossless integer widening, and exact IEEE or decimal source decomposition.
- Accept: predicate determinants reduced through `new Fraction(num, den)` and `Sign`; parametric and cell-ordering keys compared through `Compare` or `CompareTo`; differential ground truth for the predicate law matrix.
- Reject: entering the oracle before the cheaper tiers escalate; `Sqrt`, `NthRoot`, or `Log` inside a predicate; a floating or decimal readout substituted for exact `Sign`; a parallel hand-rolled `BigInteger` rational.
