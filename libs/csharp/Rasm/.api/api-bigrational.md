# [RASM_API_BIGRATIONAL]

`ExtendedNumerics.BigRational` owns the exact-rational oracle for the Geometry predicate floor through two `BigInteger`-backed value structs: `Fraction`, the canonical flat `Numerator` and `Denominator` rational, and `BigRational`, a mixed `WholePart` plus `FractionalPart` carrier; both carry exact `Sign`, sign canonicalization through `NormalizeSign`, `Reduce`, or `Simplify`, the full arithmetic, operator, and comparison algebra, and the `Mediant` Stern-Brocot bracketing step. The predicate precision ladder rises from interior `double` under `NumericsPolicy` through the 106-bit `TYoshimura.DoubleDouble` middle tier and the sign-exact `Expansion` tier to `Fraction` adjudication. `Orient2D`, `Orient3D`, `InCircle`, `InSphere`, and the implicit-point family reduce their determinants to `Fraction.Sign` when every faster tier reports an indeterminate sign. Neither struct implements `INumber<T>` or another `System.Numerics` generic-math interface; their contracts are `IComparable`, `IComparable<T>`, and `IEquatable<T>`, with `IEqualityComparer<Fraction>` on `Fraction`, so the rail composes hand-typed values instead of `INumberBase<T>`.

## [01]-[PACKAGE_SURFACE]

- Package: `ExtendedNumerics.BigRational`
- License: MIT (Adam White / ExtendedNumerics)
- Assembly: `ExtendedNumerics.BigRational`
- Namespace: `ExtendedNumerics`
- Target: multi-target (`net45`, `net46`, `net48`, `netstandard2.1`, `netcoreapp3.1`, `net5.0`, `net6.0`, `net7.0`); the `net10.0` consumer binds `lib/net7.0` as the highest available target
- Asset: pure-managed AnyCPU runtime library with no native runtime and `System.Numerics.BigInteger` as its only dependency
- ABI: plain `struct` values over `IComparable` and `IEquatable<T>`; no generic-math interfaces, `ISpanParsable<T>`, or `IFormattable` beyond the explicit `ToString` overloads
- Rail: exact-precision oracle

## [02]-[PUBLIC_TYPES]

[VALUE_STRUCTS]:

The exact-rational carriers share the `ExtendedNumerics` namespace. `Fraction` stores the canonical flat rational, and `BigRational` stores a mixed-radix rational whose `FractionalPart` is a `Fraction`; implicit operators interconvert them. `Fraction.DecimalUInt32` is a private nested decimal-decomposition helper with no public surface.

| [INDEX] | [SYMBOL]      | [REPRESENTATION] | [STORAGE]                              |
| :-----: | :------------ | :--------------- | :------------------------------------- |
|  [01]   | `Fraction`    | flat rational    | `BigInteger` numerator and denominator |
|  [02]   | `BigRational` | mixed rational   | `BigInteger` whole plus `Fraction`     |

`Fraction` carries `Sign`, `IsZero`, `IsOne`, the full algebra, `Mediant`, `Simplify`, `Reciprocal`, and `DivRem`. `BigRational` carries `Sign`, `IsZero`, the full algebra, `Reduce`, `NormalizeSign`, and `NthRoot`.

[STATIC_ANCHORS]:

The dependency-free constants bind exact-precision identities.

| [INDEX] | [SYMBOL]               | [VALUE] | [ROLE]                  |
| :-----: | :--------------------- | :------ | :---------------------- |
|  [01]   | `BigRational.Zero`     | `0`     | additive identity       |
|  [02]   | `BigRational.One`      | `1`     | multiplicative identity |
|  [03]   | `BigRational.MinusOne` | `-1`    | negative unit           |
|  [04]   | `Fraction.Zero`        | `0`     | additive identity       |
|  [05]   | `Fraction.One`         | `1`     | multiplicative identity |
|  [06]   | `Fraction.MinusOne`    | `-1`    | negative unit           |
|  [07]   | `Fraction.OneHalf`     | `1/2`   | rational half           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Fraction` construction and canonicalization

`Fraction(BigInteger numerator, BigInteger denominator)` constructs the predicate determinant directly from `BigInteger`-promoted ordinates. The `float`, `double`, and `decimal` constructors decompose their source representations exactly without rounding. `Sign` reads `NormalizeSign(this).Numerator.Sign`, so a negative denominator does not flip the sign query. `Simplify` and `ReduceToProperFraction` are the canonical reducers, and `Mediant` returns the Stern-Brocot bracketing value `(a+c)/(b+d)`.

| [INDEX] | [SURFACE]                                                    | [KIND]        | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------- | :------------ | :-------------------------- |
|  [01]   | `new Fraction(BigInteger numerator, BigInteger denominator)` | constructor   | exact rational pair         |
|  [02]   | `new Fraction(double)`                                       | constructor   | exact IEEE decomposition    |
|  [03]   | `new Fraction(float)`                                        | constructor   | exact IEEE decomposition    |
|  [04]   | `new Fraction(decimal)`                                      | constructor   | exact decimal decomposition |
|  [05]   | `Fraction.Parse(string)`                                     | factory       | rational literal parse      |
|  [06]   | `Fraction.Sign`                                              | property      | exact normalized sign       |
|  [07]   | `Fraction.Simplify(Fraction)`                                | static        | lowest-terms reduction      |
|  [08]   | `Fraction.ReduceToProperFraction(Fraction)`                  | `BigRational` | proper mixed reduction      |
|  [09]   | `Fraction.Reciprocal(Fraction)`                              | static        | exact reciprocal            |
|  [10]   | `Fraction.Mediant(Fraction left, Fraction right)`            | static        | rational bracketing         |

[ENTRYPOINT_SCOPE]: `Fraction` exact algebra and comparison

Every binary arithmetic operation has a static method and an operator twin. Comparison is total through the relational and equality operators, `Compare`, and `CompareTo`; `DivRem` returns its exact quotient and `out` remainder together. All arithmetic is infinite-precision over `BigInteger` except the precision-bounded roots and lossy `Log` readout.

| [INDEX] | [SURFACE]                                                                      | [KIND]   | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `Fraction.Add(Fraction, Fraction)`                                             | `+`      | exact addition           |
|  [02]   | `Fraction.Subtract(Fraction, Fraction)`                                        | `-`      | exact subtraction        |
|  [03]   | `Fraction.Multiply(Fraction, Fraction)`                                        | `*`      | exact multiplication     |
|  [04]   | `Fraction.Divide(Fraction, Fraction)`                                          | `/`      | exact division           |
|  [05]   | `Fraction.Remainder(Fraction, Fraction)`                                       | `%`      | exact rational remainder |
|  [06]   | `Fraction.DivRem(Fraction dividend, Fraction divisor, out Fraction remainder)` | static   | quotient plus remainder  |
|  [07]   | `Fraction.Pow(Fraction, BigInteger)`                                           | static   | integer exponent         |
|  [08]   | `Fraction.Pow(Fraction, Fraction)`                                             | static   | rational exponent        |
|  [09]   | `Fraction.Sqrt(Fraction, int precision = 30)`                                  | static   | precision-bounded root   |
|  [10]   | `Fraction.NthRoot(Fraction, BigInteger root, int precision = 30)`              | static   | precision-bounded root   |
|  [11]   | `Fraction.Abs(Fraction)`                                                       | static   | absolute value           |
|  [12]   | `Fraction.Negate(Fraction)`                                                    | `-`      | exact negation           |
|  [13]   | `Fraction.GreatestCommonDivisor(Fraction, Fraction)`                           | static   | exact GCD                |
|  [14]   | `Fraction.LeastCommonDenominator(Fraction, Fraction)`                          | static   | exact LCD                |
|  [15]   | `Fraction.Compare(Fraction, Fraction)`                                         | static   | total ordering           |
|  [16]   | `Fraction.CompareTo(Fraction)`                                                 | instance | total ordering           |
|  [17]   | `Fraction.operator <`                                                          | operator | less than                |
|  [18]   | `Fraction.operator <=`                                                         | operator | less than or equal       |
|  [19]   | `Fraction.operator >`                                                          | operator | greater than             |
|  [20]   | `Fraction.operator >=`                                                         | operator | greater than or equal    |
|  [21]   | `Fraction.operator ==`                                                         | operator | exact equality           |
|  [22]   | `Fraction.operator !=`                                                         | operator | exact inequality         |
|  [23]   | `Fraction.Log(Fraction)`                                                       | `double` | lossy natural logarithm  |

[ENTRYPOINT_SCOPE]: `BigRational` construction and mixed-radix algebra

The two-argument constructor and the three-argument `(whole, numerator, denominator)` constructor build the mixed carrier. `GetImproperFraction()` and the implicit `BigRational` to `Fraction` operator collapse it into the oracle's flat tier, while the flat-input arithmetic overloads return `BigRational`. `Reduce` and `NormalizeSign` canonicalize the mixed carrier.

| [INDEX] | [SURFACE]                                                                         | [KIND]      | [CAPABILITY]                |
| :-----: | :-------------------------------------------------------------------------------- | :---------- | :-------------------------- |
|  [01]   | `new BigRational(BigInteger numerator, BigInteger denominator)`                   | constructor | mixed rational pair         |
|  [02]   | `new BigRational(BigInteger whole, BigInteger numerator, BigInteger denominator)` | constructor | explicit mixed components   |
|  [03]   | `new BigRational(double)`                                                         | constructor | exact IEEE decomposition    |
|  [04]   | `new BigRational(float)`                                                          | constructor | exact IEEE decomposition    |
|  [05]   | `new BigRational(decimal)`                                                        | constructor | exact decimal decomposition |
|  [06]   | `BigRational.WholePart`                                                           | property    | `BigInteger` whole          |
|  [07]   | `BigRational.FractionalPart`                                                      | property    | `Fraction` remainder        |
|  [08]   | `BigRational.GetImproperFraction()`                                               | instance    | flat `Fraction` collapse    |
|  [09]   | `implicit operator Fraction(BigRational)`                                         | conversion  | flat-tier bridge            |
|  [10]   | `BigRational.Sign`                                                                | property    | exact normalized sign       |
|  [11]   | `BigRational.IsZero`                                                              | property    | exact zero test             |
|  [12]   | `BigRational.Add(BigRational, BigRational)`                                       | `+`         | exact addition              |
|  [13]   | `BigRational.Subtract(BigRational, BigRational)`                                  | `-`         | exact subtraction           |
|  [14]   | `BigRational.Multiply(BigRational, BigRational)`                                  | `*`         | exact multiplication        |
|  [15]   | `BigRational.Divide(BigRational, BigRational)`                                    | `/`         | exact division              |
|  [16]   | `BigRational.Remainder(BigRational, BigRational)`                                 | `%`         | exact rational remainder    |
|  [17]   | `BigRational.Add(Fraction, Fraction)`                                             | static      | mixed-carrier result        |
|  [18]   | `BigRational.Subtract(Fraction, Fraction)`                                        | static      | mixed-carrier result        |
|  [19]   | `BigRational.Multiply(Fraction, Fraction)`                                        | static      | mixed-carrier result        |
|  [20]   | `BigRational.Divide(Fraction, Fraction)`                                          | static      | mixed-carrier result        |
|  [21]   | `BigRational.Pow(BigRational, BigInteger)`                                        | static      | exact integer power         |
|  [22]   | `BigRational.Sqrt(BigRational)`                                                   | static      | root without precision arg  |
|  [23]   | `BigRational.NthRoot(BigRational, int root, int precision = 30)`                  | static      | precision-bounded root      |
|  [24]   | `BigRational.Mod(BigRational, BigRational)`                                       | static      | exact modulo                |
|  [25]   | `BigRational.Remainder(BigInteger, BigInteger)`                                   | static      | exact integer remainder     |
|  [26]   | `BigRational.Abs(BigRational)`                                                    | static      | absolute value              |
|  [27]   | `BigRational.Negate(BigRational)`                                                 | static      | exact negation              |
|  [28]   | `BigRational.GreatestCommonDivisor(BigRational, BigRational)`                     | static      | exact GCD                   |
|  [29]   | `BigRational.LeastCommonDenominator(BigRational, BigRational)`                    | static      | exact LCD                   |
|  [30]   | `BigRational.Reduce(BigRational)`                                                 | static      | lowest-terms reduction      |
|  [31]   | `BigRational.NormalizeSign(BigRational)`                                          | static      | sign canonicalization       |
|  [32]   | `BigRational.Compare(BigRational, BigRational)`                                   | static      | total ordering              |
|  [33]   | `BigRational.CompareTo(BigRational)`                                              | instance    | total ordering              |
|  [34]   | `BigRational.operator <`                                                          | operator    | less than                   |
|  [35]   | `BigRational.operator <=`                                                         | operator    | less than or equal          |
|  [36]   | `BigRational.operator >`                                                          | operator    | greater than                |
|  [37]   | `BigRational.operator >=`                                                         | operator    | greater than or equal       |
|  [38]   | `BigRational.operator ==`                                                         | operator    | exact equality              |
|  [39]   | `BigRational.operator !=`                                                         | operator    | exact inequality            |
|  [40]   | `BigRational.Parse(string)`                                                       | factory     | rational literal parse      |

[ENTRYPOINT_SCOPE]: conversions

The integer family is `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, and `BigInteger`; its widening conversions are implicit and lossless. The `float`, `double`, and `decimal` conversions into both rational carriers are explicit but preserve the source representation exactly. Conversions from the rational carriers to floating types round at the boundary, and the `BigInteger` conversion truncates.

| [INDEX] | [SURFACE]                                   | [KIND]     | [CAPABILITY]                |
| :-----: | :------------------------------------------ | :--------- | :-------------------------- |
|  [01]   | `implicit operator Fraction(byte)`          | conversion | lossless widening           |
|  [02]   | `implicit operator Fraction(sbyte)`         | conversion | lossless widening           |
|  [03]   | `implicit operator Fraction(short)`         | conversion | lossless widening           |
|  [04]   | `implicit operator Fraction(ushort)`        | conversion | lossless widening           |
|  [05]   | `implicit operator Fraction(int)`           | conversion | lossless widening           |
|  [06]   | `implicit operator Fraction(uint)`          | conversion | lossless widening           |
|  [07]   | `implicit operator Fraction(long)`          | conversion | lossless widening           |
|  [08]   | `implicit operator Fraction(ulong)`         | conversion | lossless widening           |
|  [09]   | `implicit operator Fraction(BigInteger)`    | conversion | lossless widening           |
|  [10]   | `implicit operator BigRational(byte)`       | conversion | lossless widening           |
|  [11]   | `implicit operator BigRational(sbyte)`      | conversion | lossless widening           |
|  [12]   | `implicit operator BigRational(short)`      | conversion | lossless widening           |
|  [13]   | `implicit operator BigRational(ushort)`     | conversion | lossless widening           |
|  [14]   | `implicit operator BigRational(int)`        | conversion | lossless widening           |
|  [15]   | `implicit operator BigRational(uint)`       | conversion | lossless widening           |
|  [16]   | `implicit operator BigRational(long)`       | conversion | lossless widening           |
|  [17]   | `implicit operator BigRational(ulong)`      | conversion | lossless widening           |
|  [18]   | `implicit operator BigRational(BigInteger)` | conversion | lossless widening           |
|  [19]   | `explicit operator Fraction(float)`         | conversion | exact IEEE decomposition    |
|  [20]   | `explicit operator Fraction(double)`        | conversion | exact IEEE decomposition    |
|  [21]   | `explicit operator Fraction(decimal)`       | conversion | exact decimal decomposition |
|  [22]   | `explicit operator BigRational(float)`      | conversion | exact IEEE decomposition    |
|  [23]   | `explicit operator BigRational(double)`     | conversion | exact IEEE decomposition    |
|  [24]   | `explicit operator BigRational(decimal)`    | conversion | exact decimal decomposition |
|  [25]   | `explicit operator double(Fraction)`        | conversion | lossy floating readout      |
|  [26]   | `explicit operator decimal(Fraction)`       | conversion | lossy decimal readout       |
|  [27]   | `explicit operator double(BigRational)`     | conversion | lossy floating readout      |
|  [28]   | `explicit operator decimal(BigRational)`    | conversion | lossy decimal readout       |
|  [29]   | `explicit operator BigInteger(Fraction)`    | conversion | truncating integer readout  |
|  [30]   | `ToString()`                                | instance   | default render              |
|  [31]   | `ToString(string format)`                   | instance   | format-string render        |
|  [32]   | `ToString(string format, IFormatProvider)`  | instance   | culture-aware render        |

## [04]-[IMPLEMENTATION_LAW]

[ORACLE_PROFILE]:
- Canonical carrier: `Fraction` stores the `BigInteger` numerator and denominator consumed by predicate determinants; `BigRational` is the mixed carrier and collapses through `GetImproperFraction()` or its implicit conversion
- Sign query: `Fraction.Sign` and `BigRational.Sign` call `NormalizeSign` before reading the numerator, so negative denominators do not invert predicate verdicts; predicates read `Sign` instead of floating or decimal output
- Exactness boundary: `Add`, `Subtract`, `Multiply`, `Divide`, `%`, `DivRem`, and integer `Pow` are infinite-precision over `BigInteger`; `Sqrt`, `NthRoot`, and `Log` remain outside predicate adjudication because they return precision-bounded or `double` results
- Generic surface: the structs implement only `IComparable`, `IComparable<T>`, and `IEquatable<T>`, with `IEqualityComparer<Fraction>` on `Fraction`; the rail does not bind `INumber<BigRational>`, `INumberBase<T>`, or `ISpanParsable<T>`

[LOCAL_ADMISSION]:
- Predicate escalation: `BigRational` and `Fraction` form the highest predicate tier after interior `double`, the 106-bit `TYoshimura.DoubleDouble` tier, and the exact `Expansion` branch report an indeterminate determinant sign; each oracle adjudication allocates `BigInteger` values, so the faster tiers retain the ordinary path
- Predicate verdict: the determinant promotes exact ordinates to `BigInteger`, constructs `new Fraction(num, den)`, reads `Fraction.Sign`, and folds `-1`, `0`, or `+1` into the `Orient` or `InCircle` result without materializing a `double`
- Ordering keys: `IntersectOp` crossing `t` keys and arrangement-cell keys use `Fraction.Compare` or `CompareTo` to break near-degenerate ordering ties without a floating epsilon
- Bracketing: `Fraction.Mediant` returns the Stern-Brocot rational strictly between two neighbors as an available bracketing and fractional-indexing primitive
- Boundary conversion: explicit `double`, `decimal`, and `BigInteger` conversions occur only at reporting boundaries; interior values remain `Fraction` or `BigRational`

[STACKING_LAW]:
- Precision ladder: the oracle follows the 106-bit `TYoshimura.DoubleDouble` tier, whose FMA `TwoProduct` and Knuth `TwoSum` transforms match the kernel, and the sign-exact `Expansion` branch; only indeterminate residue reaches `Fraction.Sign`, which confines the `BigInteger` cost to degenerate cases
- Predicate floor: `Predicate.Orient2D`, `Orient3D`, `InCircle`, `InSphere`, axis-projected `Orient2D(in Implicit, …)`, the `Compare` order key, and the implicit-query `InCircle` and `InSphere` members use the `Fraction` oracle as both runtime adjudicator and differential-fuzzing ground truth; `CsCheck` compares `double`, `Expansion`, `DoubleDouble`, and `Interval` determinants against `Fraction.Sign`
- Arrangement keys: `Arrangement` planar-overlay cells and `IntersectOp` parametric `t` keys carry `Fraction` ordering when an `Lpi` or `Tpi` coordinate exists only as an exact rational; `Compare` and `CompareTo` keep the cell complex independent of floating tolerance

[RAIL_LAW]:
- Package: `ExtendedNumerics.BigRational`
- Owns: exact-rational adjudication through the flat `Fraction` ratio, mixed `BigRational` carrier, exact arithmetic, comparison, `Sign`, `NormalizeSign`, `Reduce`, `Mediant`, lossless integer construction, and exact IEEE or decimal source decomposition
- Accept: predicate determinants reduced through `new Fraction(num, den)` and `Sign`; exact parametric and cell-ordering keys compared through `Compare` or `CompareTo`; differential ground truth for the predicate law matrix
- Reject: oracle execution before cheaper tiers escalate; `Sqrt`, `NthRoot`, or `Log` inside a predicate; bindings to unimplemented `INumber<T>`, `INumberBase<T>`, or `ISpanParsable<T>`; floating or decimal readouts substituted for exact `Sign`; parallel hand-rolled `BigInteger` rationals
