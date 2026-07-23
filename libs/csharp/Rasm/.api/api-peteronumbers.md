# [RASM_API_PETERONUMBERS]

`PeterO.Numbers` mints four arbitrary-precision carriers — `EInteger`, `EFloat`, `EDecimal`, `ERational` — over a self-contained `uint[]` bignum sharing no substrate with `System.Numerics.BigInteger`. `EContext` threads precision, rounding, exponent, trap, and flag policy through every rounded operation; `EContext.Unlimited` holds arithmetic exact, `EFloat` is the binary predicate adjudicator, and `ERational` an independent exact-rational oracle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PeterO.Numbers`
- package: `PeterO.Numbers` (CC0-1.0)
- assembly: `Numbers.dll` (assembly name `Numbers`)
- namespace: `PeterO.Numbers`
- asset: pure-managed AnyCPU, no native runtime or package dependency
- target: binds the `netstandard1.0` asset — no `ReadOnlySpan<char>` parse overload
- abi: carriers implement `IComparable<T>` and `IEquatable<T>`, never `INumber<T>`, `ISpanParsable<T>`, or general `IFormattable`; explicit `ToString` overloads own formatting
- rail: arbitrary-precision exact adjudicator through a binary `EFloat` tier and an independent `ERational` oracle

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the numeric four interconvert losslessly upward — `EInteger` into `EFloat`/`EDecimal`/`ERational`, `EFloat`/`EDecimal` into `ERational` — and adjudicate downward through `EContext`.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :---------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `EInteger`  | class         | exact `uint[]` big integer under rational parts and mantissas  |
|  [02]   | `EFloat`    | class         | arbitrary-precision `mantissa × 2^exponent` binary adjudicator |
|  [03]   | `EDecimal`  | class         | arbitrary-precision `mantissa × 10^exponent`, decimal IEEE I/O |
|  [04]   | `ERational` | class         | exact `EInteger` numerator/denominator oracle                  |
|  [05]   | `EContext`  | class         | precision, rounding, exponent, trap, and flag policy           |
|  [06]   | `ERounding` | enum          | rounding-mode discriminant                                     |

`[EROUNDING]`: `None` `Up` `Down` `HalfUp` `HalfDown` `HalfEven` `Ceiling` `Floor` `OddOrZeroFiveUp` — `Floor` and `Ceiling` direct interval bounds; `None` raises on inexact output.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EContext` threads into every rounding operation; `Unlimited` governs exact predicate determinants and `WithBlankFlags()` arms condition recording, where an absent `FlagInexact` certifies exact output.

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `EContext.ForPrecision(int)`                              | factory  | digit-precision policy (`0` = unlimited)    |
|  [02]   | `EContext.ForPrecisionAndRounding(int, ERounding)`        | factory  | precision plus rounding                     |
|  [03]   | `EContext.ForRounding(ERounding)`                         | factory  | rounding-only policy                        |
|  [04]   | `EContext(int, ERounding, int, int, bool)`                | ctor     | precision, rounding, exponent bounds, clamp |
|  [05]   | `EContext(EInteger, ERounding, EInteger, EInteger, bool)` | ctor     | big-precision variant                       |
|  [06]   | `EContext.WithBlankFlags()`                               | builder  | arm condition recording                     |
|  [07]   | `EContext.WithTraps(int)`                                 | builder  | raise on masked conditions                  |
|  [08]   | `EContext.GetNontrapping()`                               | builder  | strip traps, keep flags                     |
|  [09]   | `EContext.Flags`                                          | property | raised-condition readout                    |
|  [10]   | `EContext.Traps`                                          | property | active trap mask                            |

- `[ECONTEXT_ANCHORS]`: `Unlimited` `UnlimitedHalfEven` — no-rounding exact-arithmetic fields.
- `[ECONTEXT_BUILDERS]`: `WithRounding` `WithPrecision` `WithExponentRange` `WithExponentClamp` `WithSimplified` `WithPrecisionInBits` `WithUnlimitedExponents` — each returns an immutable derived context.
- `[ECONTEXT_PRESETS]`: `Binary16` `Binary32` `Binary64` `Binary128` `Decimal32` `Decimal64` `Decimal128` `CliDecimal` `Basic` — IEEE-754 interchange fields at `HalfEven`.
- `[ECONTEXT_FLAGS]`: `FlagInexact` `FlagInvalid` `FlagDivideByZero` `FlagOverflow` `FlagUnderflow` `FlagSubnormal` `FlagRounded` `FlagClamped` `FlagLostDigits` — const bits OR-combined into `Flags` and `Traps`.

[ENTRYPOINT_SCOPE]: `EFloat` represents `mantissa × 2^exponent` over `EInteger`; `FromDouble` lifts every finite `double` exactly since each is dyadic, and arithmetic under `Unlimited` stays exact.

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `EFloat.FromDouble(double)`           | factory  | lossless dyadic lift                   |
|  [02]   | `EFloat.FromDoubleBits(long)`         | factory  | lift from raw IEEE bit pattern         |
|  [03]   | `EFloat.FromEInteger(EInteger)`       | factory  | integer lift                           |
|  [04]   | `EFloat.Create(EInteger, EInteger)`   | factory  | exact `mantissa × 2^exponent`          |
|  [05]   | `EFloat.FromString(string, EContext)` | factory  | literal parse into context             |
|  [06]   | `EFloat.Sign`                         | property | exact sign verdict `-1`/`0`/`+1`       |
|  [07]   | `EFloat.CompareTo(EFloat)`            | method   | value ordering                         |
|  [08]   | `EFloat.CompareToTotal(EFloat)`       | method   | IEEE total order over NaN, signed zero |
|  [09]   | `EFloat.Mantissa`                     | property | `EInteger` significand                 |
|  [10]   | `EFloat.Exponent`                     | property | binary exponent                        |
|  [11]   | `EFloat.Precision() -> EInteger`      | method   | significant-digit count                |

- `[EFLOAT_ARITHMETIC]`: `Add` `Subtract` `Multiply` `Divide` — `EFloat`/`int`/`long` operands with optional `EContext`; no context or `Unlimited` stays exact, a finite context rounds.
- `[EFLOAT_ANALYTIC]`: `Sqrt` `Pow` `Exp` `Log` `Log10` `LogN` — irrational results require a finite `EContext`.
- `[EFLOAT_ROUNDING]`: `RoundToExponent` `RoundToExponentExact` `RoundToIntegerExact` `RoundToPrecision` `Quantize` — directed rounding forms the interval bracket with `ERounding.Floor`/`Ceiling`.
- `[EFLOAT_ADJACENCY]`: `ScaleByPowerOfTwo` `Ulp` `NextPlus` `NextMinus` `Increment` `Decrement` — binary scaling and adjacent-value traversal construct interval endpoints.
- `[EFLOAT_NARROWING]`: `ToEInteger` `ToEIntegerIfExact` `ToSizedEInteger` `ToDouble` `ToSingle` `ToEDecimal` — exact integer conversions return null for non-integers; `ToDouble`/`ToSingle` are lossy readouts.
- `[EFLOAT_CLASSIFY]`: `IsZero` `IsNegative` `IsFinite` `IsNaN()` `IsInfinity()` `IsSignalingNaN()` — exact sign and IEEE classification.
- `[EFLOAT_ANCHORS]`: `Zero` `One` `Ten` `NaN` `SignalingNaN` `PositiveInfinity` `NegativeInfinity` `NegativeZero` — canonical finite and non-finite fields.

[ENTRYPOINT_SCOPE]: `ERational` is an `EInteger`-backed exact rational; `FromEFloat`/`FromDouble` decompose IEEE values without rounding, and `CompareToBinary`/`CompareToDecimal` cross-compare against `EFloat`/`EDecimal` exactly.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `ERational.Create(EInteger, EInteger)`       | factory  | exact numerator/denominator pair    |
|  [02]   | `ERational.FromEInteger(EInteger)`           | factory  | integer lift                        |
|  [03]   | `ERational.FromEFloat(EFloat)`               | factory  | exact binary-float lift             |
|  [04]   | `ERational.FromEDecimal(EDecimal)`           | factory  | exact decimal lift                  |
|  [05]   | `ERational.FromDouble(double)`               | factory  | lossless dyadic lift                |
|  [06]   | `ERational.FromString(string)`               | factory  | rational literal parse              |
|  [07]   | `ERational.Numerator`                        | property | exact `EInteger` numerator          |
|  [08]   | `ERational.Denominator`                      | property | exact `EInteger` denominator        |
|  [09]   | `ERational.Sign`                             | property | exact normalized sign               |
|  [10]   | `ERational.ToLowestTerms()`                  | method   | canonical normalization             |
|  [11]   | `ERational.CompareToBinary(EFloat)`          | method   | exact cross-compare vs binary float |
|  [12]   | `ERational.CompareToDecimal(EDecimal)`       | method   | exact cross-compare vs decimal      |
|  [13]   | `ERational.ToEInteger() -> EInteger`         | method   | exact integer narrowing             |
|  [14]   | `ERational.ToSizedEInteger(int) -> EInteger` | method   | bit-bounded narrowing               |
|  [15]   | `ERational.ToDouble() -> double`             | method   | lossy readout                       |

- `[ERATIONAL_ARITHMETIC]`: `Add` `Subtract` `Multiply` `Divide` `Remainder` — infinite-precision over `EInteger`, rational/`int`/`long` operands.
- `[ERATIONAL_SIGN]`: `Abs` `Negate` `CopySign` `Increment` `Decrement` — exact sign and step transforms.
- `[ERATIONAL_CLASSIFY]`: `IsZero` `IsNegative` `IsFinite` `IsInteger()` — sign, finiteness, integrality.
- `[ERATIONAL_ORDER]`: `CompareTo` `CompareToValue` `CompareToTotal` — value and IEEE total ordering across `int`/`long`/`ERational`.

[ENTRYPOINT_SCOPE]: `EInteger` is the self-contained `uint[]` bignum under rational parts and binary mantissas.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `EInteger.FromInt32(int)`                      | factory  | integer construction                       |
|  [02]   | `EInteger.FromInt64(long)`                     | factory  | wide-integer construction                  |
|  [03]   | `EInteger.FromString(string)`                  | factory  | decimal literal parse                      |
|  [04]   | `EInteger.FromBytes(byte[], bool)`             | factory  | two's-complement bytes, little-endian flag |
|  [05]   | `EInteger.DivRem(EInteger) -> EInteger[]`      | method   | combined quotient and remainder            |
|  [06]   | `EInteger.Gcd(EInteger)`                       | method   | greatest common divisor                    |
|  [07]   | `EInteger.ModPow(EInteger, EInteger)`          | method   | modular exponentiation                     |
|  [08]   | `EInteger.Sqrt() -> EInteger`                  | method   | integer square root                        |
|  [09]   | `EInteger.Sign`                                | property | sign                                       |
|  [10]   | `EInteger.CompareTo(EInteger)`                 | method   | ordering                                   |
|  [11]   | `EInteger.GetSignedBitLengthAsInt64() -> long` | method   | signed-bit length                          |
|  [12]   | `EInteger.ToBytes(bool) -> byte[]`             | method   | two's-complement serialization             |
|  [13]   | `EInteger.ToInt32Checked() -> int`             | method   | checked narrowing                          |

- `[EINTEGER_ARITHMETIC]`: `Add` `Subtract` `Multiply` `Divide` `Mod` `Remainder` `Pow` — exact, `EInteger`/`int`/`long` operands.
- `[EINTEGER_SIGN]`: `Abs` `Negate` `Increment` `Decrement` — exact sign transforms.
- `[EINTEGER_INSPECT]`: `IsZero` `IsEven` `GetSignedBit(int)` — parity and signed-bit inspection.
- `[EINTEGER_ANCHORS]`: `Zero` `One` — canonical values.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Representation independence: `EInteger` shares no substrate with `System.Numerics.BigInteger`, `Fraction`, or `BigRational`, so agreement between the `PeterO` and `Fraction` oracles verifies a determinant sign across unrelated implementations.
- Exactness gate: `EContext.Unlimited` or an omitted context holds `Add`/`Subtract`/`Multiply` exact, a finite context rounds through `ERounding`, and `WithBlankFlags()` with `Flags == 0` certifies no `FlagInexact`.
- Predicate fold: determinant ordinates lift through `EFloat.FromDouble`, accumulate exactly under `Unlimited`, and `EFloat.Sign` yields `-1`/`0`/`+1` before the value is discarded.
- Comparison surface: `Sign` and `CompareToTotal` order values through `IComparable<T>`/`IEquatable<T>`, never generic-math or span-parse bindings.

[STACKING]:
- `api-doubledouble`(`.api/api-doubledouble.md`): `ddouble` is the fixed 106-bit tier below, its `TwoProduct`/`TwoSum` transforms matching the `Expansion` kernel; only sub-106-bit-indeterminate residue promotes to `EFloat`/`ERational`, confining heap cost to the degenerate set.
- `api-bigrational`(`.api/api-bigrational.md`): `Fraction` is the `System.Numerics.BigInteger`-backed exact oracle at the same altitude; `ERational` is the independent-representation twin and `EFloat` the exact binary adjudicator, cross-checking rather than collapsing onto one implementation.
- Geometry `Expansion` kernel: the in-house adaptive Shewchuk expansion and `EFloat` evaluate one determinant sign through unrelated arithmetic; a `CsCheck` differential test compares `Expansion.Sign`, `EFloat.Sign`, `Fraction.Sign`, and `ERational.Sign` as a four-way invariant.
- Interval bracket: `ERounding.Floor`/`Ceiling` with `ForPrecisionAndRounding(53, …).WithPrecisionInBits(true)` and `RoundToExponentExact` bracket the software-rounded interval endpoints of the sign filter.

[LOCAL_ADMISSION]:
- `EFloat` under `EContext.Unlimited` is the arbitrary-precision binary tier of the predicate ladder above interior `double`, 106-bit `ddouble`, and exact `Expansion`.
- `EDecimal` carries General Decimal Arithmetic and IEEE-754 decimal semantics for exact human-readable I/O and banker's rounding; geometry adjudication uses `EFloat` and `ERational`.

[RAIL_LAW]:
- Package: `PeterO.Numbers`
- Owns: arbitrary-precision binary, decimal, rational, and integer arithmetic (`EFloat`/`EDecimal`/`ERational`/`EInteger`); `EContext` and `ERounding` own precision, rounding, exponents, traps, flags, directed rounding, and exactness proof.
- Accept: determinants lifted through `EFloat.FromDouble`, accumulated under `Unlimited`, read through `EFloat.Sign`; the cross-tier, four-way differential, and interval-bracket compositions its `[STACKING]` seams own; exact decimal I/O through `EDecimal`.
- Reject: generic-math or span-parsing bindings absent from the ABI; finite-context analytic operations inside predicate adjudication; `double` or `EDecimal` readouts substituted for exact sign verdicts; single-oracle collapse of differential checks; `IRadixMathHelper` interface plumbing presented as consumer API.
