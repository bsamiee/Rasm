# [RASM_API_PETERONUMBERS]

`PeterO.Numbers` supplies four arbitrary-precision value types: `EInteger` is a big integer, `EDecimal` is a base-10 floating point, `EFloat` is a base-2 floating point, and `ERational` carries an exact `Numerator`/`Denominator` ratio. Every rounding operation accepts an `EContext` precision, rounding, and exponent policy with an explicit `ERounding` mode and IEEE-754-style trap and flag accounting. The geometry predicate floor places `EFloat` between the fixed 106-bit `TYoshimura.DoubleDouble` tier (`api-doubledouble`) and the `ExtendedNumerics.BigRational` exact-rational oracle (`api-bigrational`). Under `EContext.Unlimited`, polynomial orient, incircle, and insphere determinants over dyadic-double coordinates evaluate exactly in a representation independent of `BigRational`, so `EFloat` cross-validates the hand-rolled `Expansion` sign and `ERational` supplies another exact-rational adjudicator through a bignum implementation distinct from `Fraction`. `ERounding.Floor` and `ERounding.Ceiling` direct software rounding for an interval-arithmetic sign filter, while the `EContext.Flag*` set reports inexact, invalid, and divide-by-zero conditions. Like `BigRational` and unlike `ddouble`, the number types expose plain `IComparable<T>` and `IEquatable<T>` values instead of `System.Numerics` generic math.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PeterO.Numbers`

- Package: `PeterO.Numbers`
- License: CC0-1.0; Peter Occil applies a public-domain dedication rather than MIT terms at `github.com/peteroupc/Numbers`.
- Assembly: `Numbers.dll`; the assembly name is `Numbers`, not `PeterO.Numbers`.
- Namespace: `PeterO.Numbers`; it contains the numeric carriers, policy types, trap exception, and the `EDecimals`, `EFloats`, and `Extras` general-arithmetic-spec helpers.
- Target: `lib/net20`, `lib/net40`, and `lib/netstandard1.0`; the `net10.0` consumer binds the highest available `lib/netstandard1.0` asset, whose ABI excludes `net5.0` assets and `Span<char>` parsing.
- Asset: Pure-managed AnyCPU library with no native runtime or package dependencies; `EInteger` is a self-contained `uint[]` bignum rather than a `System.Numerics.BigInteger` wrapper, so it remains representation-independent from `BigRational` and `Fraction`.
- ABI: The numeric carriers implement `IComparable<T>` and `IEquatable<T>` but not `INumber<T>`, `ISpanParsable<T>`, or general `IFormattable`; explicit `ToString` overloads own formatting.
- Rail: Arbitrary-precision exact adjudicator through a binary `EFloat` tier and an independent `ERational` oracle.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: The number carriers and their context and rounding policy inhabit `PeterO.Numbers`.

- Rail: Arbitrary-precision exact adjudicator.

The numeric carriers interconvert losslessly upward: `EInteger` converts exactly to `EFloat`, `EDecimal`, and `ERational`, while `EFloat` and `EDecimal` convert exactly to `ERational`. Downward conversion adjudicates through `EContext`; `EContext.Unlimited` is the no-rounding policy used by predicate adjudication. Internal arithmetic machinery includes `FastInteger`, `FastIntegerFixed`, `BitShiftAccumulator`, `RadixMath`, and `IRadixMath*`; explicit `IRadixMathHelper<T>` implementations such as `GetMantissa`, `GetExponent`, `CreateNewWithFlags`, and `GetRadix` remain interface plumbing rather than consumer API.

| [INDEX] | [SYMBOL]    | [KIND] | [ROLE]                            |
| :-----: | :---------- | :----- | :-------------------------------- |
|  [01]   | `EInteger`  | class  | arbitrary-precision integer       |
|  [02]   | `EFloat`    | class  | arbitrary-precision binary float  |
|  [03]   | `EDecimal`  | class  | arbitrary-precision decimal float |
|  [04]   | `ERational` | class  | arbitrary-precision rational      |
|  [05]   | `EContext`  | class  | arithmetic policy                 |
|  [06]   | `ERounding` | enum   | rounding discriminant             |

[EINTEGER]:

- Form: `sealed class` over a self-contained `uint[]` bignum.
- Arithmetic: `Add`, `Subtract`, `Multiply`, `DivRem`, `Pow`, `Gcd`, `Sqrt`, `Mod`, and `ModPow` remain exact.
- Boundary: `Sign`, `CompareTo`, `FromBytes`, and `ToBytes` construct and inspect determinant numerators and denominators.

[EFLOAT]:

- Form: `sealed class` representing `mantissa × 2^exponent`; every rounded operation accepts `EContext`.
- Arithmetic: `Sqrt`, `Pow`, `Exp`, and `Log` compose finite-context calculation, while dyadic determinant arithmetic remains exact under `EContext.Unlimited`.
- Predicate: The binary representation supplies an exact adjudicator independent from `ddouble` and `Fraction`.

[EDECIMAL]:

- Form: `sealed class` representing `mantissa × 10^exponent` under General Decimal Arithmetic and IEEE 754 decimal semantics.
- Boundary: Exact decimal I/O and readable rounding remain outside the geometry interior tier.

[ERATIONAL]:

- Form: `sealed class` over `EInteger Numerator` and `EInteger Denominator`.
- Arithmetic: `Sign`, `CompareTo`, and `ToLowestTerms` own exact rational normalization and comparison.
- Boundary: `CompareToBinary(EFloat)` and `CompareToDecimal(EDecimal)` compare representations exactly and form an independent oracle parallel to `Fraction`.

[ECONTEXT]:

- Form: `sealed class` carrying `Precision`, `Rounding`, `EMin`, `EMax`, `Traps`, and `Flags`.
- Policy: `Unlimited`, the binary and decimal IEEE presets, `ForPrecision`, and `ForRounding` derive arithmetic policy.

[EROUNDING]:

- Members: `None`, `Up`, `Down`, `Ceiling`, `Floor`, `HalfUp`, `HalfDown`, `HalfEven`, `Odd`, `ZeroFiveUp`, and `OddOrZeroFiveUp`.
- Policy: `Floor` and `Ceiling` direct interval bounds, while `None` raises on inexact output.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EContext` owns the precision, rounding, exponent, trap, and flag policy.

- Rail: Arbitrary-precision exact adjudicator.

`EContext` is constructed once and threaded into every rounding operation. `Unlimited` governs exact predicate determinants, `Binary64` and `Binary128` mirror IEEE binary interchange contexts, and the `With*` builders derive immutable policies. `WithBlankFlags()` records operation conditions; the absence of `FlagInexact` certifies exact output.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE] | [CAPABILITY]             |
| :-----: | :------------------------ | :----------- | :----------------------- |
|  [01]   | `EContext.Unlimited`      | field        | unlimited arithmetic     |
|  [02]   | `EContext.Binary16`       | field        | interchange policy       |
|  [03]   | `EContext.ForPrecision`   | factory      | precision policy         |
|  [04]   | `EContext(...)`           | constructor  | custom policy            |
|  [05]   | `EContext.WithRounding`   | builder      | immutable derivation     |
|  [06]   | `EContext.WithBlankFlags` | builder      | flag and trap policy     |
|  [07]   | `EContext.Flags`          | property     | condition readout        |
|  [08]   | `EContext.FlagInexact`    | constant     | condition bit vocabulary |

[ECONTEXT_UNLIMITED]:

- Members: `EContext.Unlimited` and `EContext.UnlimitedHalfEven`.
- Capability: Unlimited precision without exponent clamping; predicate adjudication selects `ERounding.None` or `ERounding.HalfEven`.

[ECONTEXT_PRESETS]:

- Members: `EContext.Binary16`, `Binary32`, `Binary64`, `Binary128`, `Decimal32`, `Decimal64`, `Decimal128`, `CliDecimal`, and `Basic`.
- Capability: Preconfigured IEEE-754 binary and decimal interchange precision, exponent range, and `HalfEven` rounding.

[ECONTEXT_FACTORIES]:

- Members: `EContext.ForPrecision(int)`, `ForPrecisionAndRounding(int, ERounding)`, and `ForRounding(ERounding)`.
- Capability: Digit precision and rounding derive a policy; `precision=0` selects unlimited precision.

[ECONTEXT_CONSTRUCTORS]:

- Members: `new EContext(int precision, ERounding, int expMin, int expMax, bool clampNormalExponents)` and its `EInteger` precision overload.
- Capability: Small-integer or `EInteger` precision combines with explicit exponent bounds and clamping.

[ECONTEXT_DERIVATION]:

- Members: `EContext.WithRounding(ERounding)`, `WithPrecision(int)`, `WithExponentRange(int,int)`, `WithExponentClamp(bool)`, `WithSimplified(bool)`, `WithPrecisionInBits(bool)`, and `WithUnlimitedExponents()`.
- Capability: Each builder returns an immutable context variation.

[ECONTEXT_CONDITIONS]:

- Members: `EContext.WithBlankFlags()`, `WithNoFlags()`, `WithTraps(int)`, `WithNoFlagsOrTraps()`, and `GetNontrapping()`.
- Capability: Condition recording and trap raising remain explicit policy dimensions.

[ECONTEXT_READOUT]:

- Members: `EContext.Flags`, `HasFlags`, `HasFlagsOrTraps`, and `Traps`.
- Capability: Raised conditions expose exactness and exceptional arithmetic outcomes after an operation.

[ECONTEXT_FLAG_VOCABULARY]:

- Members: `EContext.FlagInexact`, `FlagInvalid`, `FlagDivideByZero`, `FlagOverflow`, `FlagUnderflow`, `FlagSubnormal`, `FlagRounded`, `FlagClamped`, and `FlagLostDigits`.
- Capability: The constants combine through bitwise OR into `Flags` and `Traps`.

[ENTRYPOINT_SCOPE]: `EFloat` owns the arbitrary-precision binary predicate tier.

- Rail: Arbitrary-precision exact adjudicator.

`EFloat` represents `mantissa × 2^exponent` over `EInteger`. `FromDouble` lifts every finite IEEE `double` without rounding because each value is a dyadic rational. Arithmetic without an `EContext`, or with `EContext.Unlimited`, remains exact; a finite-precision context rounds through `ERounding`. Predicate adjudication accumulates the determinant under `Unlimited`, reads `Sign`, and discards the value.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE] | [CAPABILITY]           |
| :-----: | :------------------------- | :----------- | :--------------------- |
|  [01]   | `EFloat.FromDouble`        | factory      | binary lift            |
|  [02]   | `EFloat.Create`            | factory      | dyadic construction    |
|  [03]   | `EFloat.FromString`        | factory      | literal parsing        |
|  [04]   | `EFloat.Zero`              | field        | canonical values       |
|  [05]   | `EFloat.Add`               | method       | binary arithmetic      |
|  [06]   | `EFloat.Sqrt`              | method       | finite-context math    |
|  [07]   | `EFloat.Abs`               | method       | sign transformation    |
|  [08]   | `EFloat.Sign`              | property     | classification         |
|  [09]   | `EFloat.CompareTo`         | method       | ordering               |
|  [10]   | `EFloat.RoundToExponent`   | method       | directed rounding      |
|  [11]   | `EFloat.ScaleByPowerOfTwo` | method       | adjacent-value control |
|  [12]   | `EFloat.ToEInteger`        | method       | narrowing              |
|  [13]   | `EFloat.Mantissa`          | property     | decomposition          |

[EFLOAT_BINARY_LIFT]:

- Members: `EFloat.FromDouble(double)`, `FromSingle(float)`, `FromDoubleBits(long)`, and `FromSingleBits(int)`.
- Capability: Every finite IEEE binary value lifts losslessly into its exact dyadic representation.

[EFLOAT_CONSTRUCTION]:

- Members: `EFloat.FromEInteger(EInteger)`, `Create(EInteger mantissa, EInteger exponent)`, and `Create(long, int)`.
- Capability: The big-integer tier constructs an exact `mantissa × 2^exponent` value.

[EFLOAT_PARSE]:

- Members: `EFloat.FromString(string[, EContext])`, `FromString(string, int offset, int length[, EContext])`, and the corresponding `byte[]` and `char[]` overloads.
- Capability: Decimal and hexadecimal literals round into the supplied context; the `netstandard1.0` ABI has no `ReadOnlySpan<char>` overload.

[EFLOAT_ANCHORS]:

- Members: `EFloat.Zero`, `One`, `Ten`, `NaN`, `SignalingNaN`, `PositiveInfinity`, `NegativeInfinity`, and `NegativeZero`.
- Capability: The fields provide canonical finite and non-finite values.

[EFLOAT_ARITHMETIC]:

- Members: `EFloat.Add`, `Subtract`, `Multiply`, and `Divide` with `EFloat`, `int`, and `long` operands plus optional `EContext` parameters.
- Capability: No context or `EContext.Unlimited` preserves exactness; a finite context rounds determinant arithmetic.

[EFLOAT_ANALYTIC]:

- Members: `EFloat.Sqrt(EContext)`, `SquareRoot(EContext)`, `Pow(EFloat[, EContext])`, `Pow(int[, EContext])`, `Exp(ctx)`, `Log(ctx)`, `Log10(ctx)`, and `LogN(EFloat, ctx)`.
- Capability: Square roots, powers, and transcendental operations require a finite context when their results are irrational.

[EFLOAT_SIGN]:

- Members: `EFloat.Abs([ctx])`, `Negate([ctx])`, `CopySign(EFloat)`, `Reduce(ctx)`, and `Plus(ctx)`.
- Capability: The methods transform magnitude and sign or remove trailing zeros.

[EFLOAT_CLASSIFICATION]:

- Members: `EFloat.Sign`, `IsZero`, `IsNegative`, `IsFinite`, `IsNaN()`, `IsInfinity()`, and `IsSignalingNaN()`.
- Capability: Exact sign and IEEE classification expose the predicate verdict.

[EFLOAT_ORDERING]:

- Members: `EFloat.CompareTo(EFloat)`, `CompareTo(int)`, `CompareTo(long)`, `CompareToValue(...)`, `CompareToTotal([ctx])`, and `CompareToSignal(EFloat, ctx)`.
- Capability: Value ordering and IEEE total ordering distinguish NaN and signed zero.

[EFLOAT_ROUNDING]:

- Members: `EFloat.RoundToExponent(EInteger, ctx)`, `RoundToExponentExact(EInteger, ERounding)`, `RoundToIntegerExact(ctx)`, `RoundToPrecision(ctx)`, and `Quantize(EFloat, ctx)`.
- Capability: Directed rounding targets an exponent or precision and forms the interval-bracket primitive with `ERounding.Floor` and `ERounding.Ceiling`.

[EFLOAT_ADJACENCY]:

- Members: `EFloat.ScaleByPowerOfTwo(int[, ctx])`, `ScaleByPowerOfTwo(EInteger[, ctx])`, `Ulp()`, `NextPlus(ctx)`, `NextMinus(ctx)`, `Increment()`, and `Decrement()`.
- Capability: Binary exponent scaling, ULP steps, and adjacent-value traversal construct interval endpoints.

[EFLOAT_NARROWING]:

- Members: `EFloat.ToEInteger()`, `ToEIntegerExact()`, `ToEIntegerIfExact()`, `ToSizedEInteger(int)`, `ToDouble()`, `ToSingle()`, `ToInt32Checked()`, and `ToEDecimal()`.
- Capability: Exact integer conversions reject or return null for non-integers, while `ToDouble` is a lossy boundary readout.

[EFLOAT_DECOMPOSITION]:

- Members: `EFloat.Mantissa`, `UnsignedMantissa`, `Exponent`, and `Precision()`.
- Capability: The `EInteger` significand, binary exponent, and significant-digit count expose exact bounds.

[ENTRYPOINT_SCOPE]: `ERational` owns the independent exact-rational oracle.

- Rail: Arbitrary-precision exact adjudicator.

`ERational` is an `EInteger`-backed exact rational with the same adjudication role as `ExtendedNumerics.Fraction` (`api-bigrational`) but a distinct library and representation. `FromEFloat` and `FromDouble` decompose IEEE values without rounding. `CompareToBinary` and `CompareToDecimal` compare against `EFloat` and `EDecimal` exactly, supplying the cross-representation bridge for differential predicate checks.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE] | [CAPABILITY]        |
| :-----: | :------------------------ | :----------- | :------------------ |
|  [01]   | `ERational.Create`        | factory      | rational creation   |
|  [02]   | `ERational.FromEFloat`    | factory      | exact lift          |
|  [03]   | `ERational.FromString`    | factory      | literal parsing     |
|  [04]   | `ERational.Numerator`     | property     | exact decomposition |
|  [05]   | `ERational.Add`           | method       | rational arithmetic |
|  [06]   | `ERational.ToLowestTerms` | method       | normalization       |
|  [07]   | `ERational.CompareTo`     | method       | exact ordering      |
|  [08]   | `ERational.ToEInteger`    | method       | narrowing           |

[ERATIONAL_CONSTRUCTION]:

- Members: `ERational.Create(EInteger numerator, EInteger denominator)`, `Create(int, int)`, and `Create(long, long)`.
- Capability: Numerator and denominator pairs construct exact rational values.

[ERATIONAL_LIFT]:

- Members: `ERational.FromEInteger(EInteger)`, `FromEFloat(EFloat)`, `FromEDecimal(EDecimal)`, `FromDouble(double)`, `FromDoubleBits(long)`, `FromSingle(float)`, `FromDecimal(decimal)`, `FromInt32(...)`, and `FromInt64(...)`.
- Capability: Integers, IEEE values, `EFloat`, and `EDecimal` lift losslessly into exact rationals.

[ERATIONAL_PARSE]:

- Members: `ERational.FromString(string)` and the `FromString(byte[])` and `FromString(char[])` offset and length overloads.
- Capability: Rational literal parsing has no span overload in the `netstandard1.0` asset.

[ERATIONAL_DECOMPOSITION]:

- Members: `ERational.Numerator`, `Denominator`, `Sign`, `IsZero`, `IsInteger()`, and `IsFinite`.
- Capability: The exact `EInteger` numerator and denominator expose sign, zero, finiteness, and integrality.

[ERATIONAL_ARITHMETIC]:

- Members: `ERational.Add`, `Subtract`, `Multiply`, `Divide`, and `Remainder` with rational, `int`, and `long` operands.
- Capability: Every operation preserves infinite-precision rational arithmetic.

[ERATIONAL_NORMALIZATION]:

- Members: `ERational.ToLowestTerms()`, `Abs()`, `Negate()`, `CopySign(ERational)`, `Increment()`, and `Decrement()`.
- Capability: The methods normalize terms and transform signs exactly.

[ERATIONAL_ORDERING]:

- Members: `ERational.CompareTo(ERational)`, `CompareTo(int)`, `CompareTo(long)`, `CompareToValue(...)`, `CompareToBinary(EFloat)`, `CompareToDecimal(EDecimal)`, and `CompareToTotal(ERational)`.
- Capability: Exact ordering crosses rational, binary, and decimal representations without conversion loss.

[ERATIONAL_NARROWING]:

- Members: `ERational.ToEInteger()`, `ToEIntegerIfExact()`, `ToSizedEInteger(int)`, `ToDouble()`, and `ToInt32Checked()`.
- Capability: `ToDouble` is lossy, while exact integer conversion returns null or rejects a non-integer.

[ENTRYPOINT_SCOPE]: `EInteger` owns the exact big-integer substrate.

- Rail: Arbitrary-precision exact adjudicator.

`EInteger` is a self-contained `uint[]` bignum for rational numerators, rational denominators, and binary mantissas. Its independence from `System.Numerics.BigInteger` and the `Fraction` representation keeps the exact oracles on unrelated integer substrates.

| [INDEX] | [SURFACE]            | [CALL_SHAPE] | [CAPABILITY]         |
| :-----: | :------------------- | :----------- | :------------------- |
|  [01]   | `EInteger.FromInt32` | factory      | integer construction |
|  [02]   | `EInteger.Add`       | method       | integer arithmetic   |
|  [03]   | `EInteger.Pow`       | method       | integer algorithms   |
|  [04]   | `EInteger.Sign`      | property     | bit inspection       |
|  [05]   | `EInteger.ToBytes`   | method       | serialization        |

[EINTEGER_CONSTRUCTION]:

- Members: `EInteger.Zero`, `One`, `FromInt32(int)`, `FromInt64(long)`, `FromString(string)`, and `FromBytes(byte[], bool littleEndian)`.
- Capability: Exact integer construction includes raw two's-complement bytes.

[EINTEGER_ARITHMETIC]:

- Members: `EInteger.Add`, `Subtract`, `Multiply`, `Divide`, `DivRem`, `Mod`, and `Remainder` with `EInteger`, `int`, and `long` operands.
- Capability: Exact arithmetic includes combined quotient and remainder projection.

[EINTEGER_ALGORITHMS]:

- Members: `EInteger.Pow(int)`, `Pow(long)`, `Pow(EInteger)`, `Sqrt()`, `Gcd(EInteger)`, `ModPow(EInteger pow, EInteger mod)`, `Abs()`, and `Negate()`.
- Capability: The algorithms preserve exact powers, integer roots, greatest common divisors, and modular exponentiation.

[EINTEGER_INSPECTION]:

- Members: `EInteger.Sign`, `IsZero`, `IsEven`, `CompareTo(EInteger)`, `CompareTo(int)`, `CompareTo(long)`, `GetSignedBit(int)`, and `GetSignedBitLength()`.
- Capability: Exact sign, parity, ordering, and signed-bit inspection expose integer state.

[EINTEGER_READOUT]:

- Members: `EInteger.ToBytes(bool littleEndian)`, `ToInt64Checked()`, `ToInt32Checked()`, and `ToString()`.
- Capability: Serialization and checked narrowing expose boundary values.

## [04]-[IMPLEMENTATION_LAW]

[ADJUDICATOR_PROFILE]:

- Representation: `EInteger` is a self-contained `uint[]` bignum with no shared substrate beneath `System.Numerics.BigInteger`, `ExtendedNumerics.Fraction`, or `BigRational`.
- Independence: Agreement between the `Fraction` oracle and the `PeterO` `EFloat` or `ERational` oracle verifies determinant signs across unrelated arbitrary-precision implementations.
- Exactness gate: `EContext.Unlimited`, or an omitted context, keeps `Add`, `Subtract`, and `Multiply` exact; a finite context rounds through `ERounding`.
- Analytic boundary: `Sqrt`, `Pow`, `Exp`, and `Log` require a finite context for irrational results and remain outside predicate adjudication, matching the `Fraction.Sqrt`, `NthRoot`, and `Log` boundary.
- Exactness proof: `WithBlankFlags()` records arithmetic conditions, and `EContext.Flags == 0` after determinant accumulation certifies that no `FlagInexact` condition occurred.
- Dyadic lift: Every finite IEEE `double` is dyadic, so `EFloat.FromDouble` and `ERational.FromDouble` preserve the determinant's input coordinates without rounding.
- Generic boundary: The numeric carriers implement only `IComparable<T>` and `IEquatable<T>`; a rail hand-types values instead of binding `INumber<EFloat>`, `INumberBase<T>`, or `ISpanParsable<T>`.
- Comparison: `Sign` and `CompareToTotal` form the generic comparison and equality surface, matching `api-bigrational` and differing from `api-doubledouble`.

[LOCAL_ADMISSION]:

- Precision tier: `EFloat` under `EContext.Unlimited` is the arbitrary-precision binary adjudicator in the predicate precision ladder: interior `double` through `NumericsPolicy`, `TYoshimura.DoubleDouble` at 106 bits, exact `Expansion` sign, and exact `EFloat`, `ERational`, or `Fraction` adjudication.
- Independence: The binary tier checks the sub-106-bit-degenerate residue through a representation independent of both `Expansion` and `Fraction`.
- Predicate fold: The determinant ordinates lift through `EFloat.FromDouble`; exact `Multiply` and `Subtract` under `Unlimited` accumulate orient, incircle, and insphere polynomials before `EFloat.Sign` yields `-1`, `0`, or `+1` and the value is discarded.
- Rational check: `ERational.FromDouble` and `CompareToBinary(EFloat)` compare the `EFloat`, `Fraction`, and `ddouble` signs across three representations.
- Interval filter: `ERounding.Floor` and `Ceiling` with `EContext.ForPrecisionAndRounding(53, ...).WithPrecisionInBits(true)` bracket software-rounded interval endpoints.
- Interval tier: The predicate `Interval` carrier composes the bracket as the implicit-member `PrecisionTier.Interval` filter before `Expansion` and `Fraction` work.
- Decimal boundary: `EDecimal` carries General Decimal Arithmetic and IEEE 754 decimal semantics for exact human-readable I/O and banker's rounding; geometry adjudication uses `EFloat` and `ERational`.

[STACKING_LAW]:

- Precision ladder: `EFloat` and `ERational` sit above fixed 106-bit `TYoshimura.DoubleDouble` and `Expansion` at the same exact-adjudication altitude as `ExtendedNumerics.BigRational`, but on an independent library.
- Escalation: The kernel advances tier by tier, and only indeterminate residue reaches the arbitrary-precision oracle, confining heap cost to the degenerate set.
- Big-rational roles: `Fraction` and `BigRational` are the `System.Numerics.BigInteger`-backed ground truth, `ERational` is an independently implemented exact rational, and `EFloat` is an exact binary adjudicator.
- Differential rule: The independent oracles cross-check one another rather than collapsing determinant adjudication onto one implementation.
- Double-double role: `ddouble` is the fast fixed 106-bit tier with `INumber<T>` generic math, while `EFloat` and `ERational` are unbounded exact tiers without generic math.
- Cost boundary: `ddouble` resolves the bulk; hand-typed `EFloat` and `ERational` adjudicate the residue outside generic `where T : INumber<T>` kernels.
- Expansion role: The in-house `Expansion` branch evaluates adaptive Shewchuk-style floating-point expansions, while external `EFloat` evaluates the same sign through arbitrary-precision binary arithmetic.
- Agreement: A `CsCheck` differential test compares `Expansion.Sign`, `EFloat.Sign`, `Fraction.Sign`, and `ERational.Sign` as a four-way invariant.

[RAIL_LAW]:

- Package: `PeterO.Numbers`
- Owns: Arbitrary-precision binary, decimal, rational, and integer arithmetic through `EFloat`, `ERational`, `EInteger`, and `EDecimal`; `EContext` and `ERounding` own precision, rounding, exponents, traps, flags, directed rounding, and exactness proof.
- Accept: Predicate determinants lifted through `EFloat.FromDouble`, accumulated under `EContext.Unlimited`, and read through `EFloat.Sign`; exact cross-tier checks through `ERational.FromDouble`, `CompareToBinary`, and `CompareToDecimal`; interval brackets through `ERounding.Floor`, `Ceiling`, and `RoundToExponentExact`; four-way differential adjudication across `EFloat`, `ERational`, `Expansion`, and `Fraction`; exact decimal I/O through `EDecimal`.
- Reject: Generic-math or span-parsing bindings absent from the ABI; finite-context analytic operations inside predicate adjudication; `double` or `EDecimal` readouts substituted for exact sign verdicts; single-oracle collapse of differential checks; internal arithmetic machinery presented as consumer API; or assembly and license identities substituted for package identity.
