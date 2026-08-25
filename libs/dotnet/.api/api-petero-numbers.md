# [RASM_API_PETERO_NUMBERS]

`PeterO.Numbers` mints four arbitrary-precision carriers — `EInteger`, `EFloat`, `EDecimal`, `ERational` — over a self-contained `uint[]` bignum sharing no substrate with `System.Numerics.BigInteger`. `EContext` threads precision, rounding, exponent, trap, and flag policy through every rounded operation; `EContext.Unlimited` holds arithmetic exact. Two folders bind disjoint lanes: the `Rasm` kernel seats `EFloat` as the binary predicate adjudicator and `ERational` as an independent exact-rational oracle on the geometry predicate ladder, and `Rasm.Compute` carries `ERational` as the ℚ⁷ SI dimension-exponent vector — bridging the AngouriMath numeric tower, where `Entity.Number.Rational` leaves carry `ERational` and `Real` leaves carry `EDecimal` — beside the `EFloat` exact criterion-sum accumulator.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

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

- `[EFLOAT_ARITHMETIC]`: `Add` `Subtract` `Multiply` `Divide` — `EFloat`/`int`/`long` operands with optional `EContext`; no context or `Unlimited` stays exact, a finite context rounds. The exact unbounded fold (`EFloat.Zero` seed, `FromDouble` lift per term, `Add` with no context, one terminal `RoundToPrecision(EContext.Binary64)` then `ToDouble()`) is the Compute large-n criterion-sum lane.
- `[EFLOAT_ANALYTIC]`: `Sqrt` `Pow` `Exp` `Log` `Log10` `LogN` — irrational results require a finite `EContext`.
- `[EFLOAT_ROUNDING]`: `RoundToExponent` `RoundToExponentExact` `RoundToIntegerExact` `RoundToPrecision` `Quantize` — directed rounding forms the interval bracket with `ERounding.Floor`/`Ceiling`.
- `[EFLOAT_ADJACENCY]`: `Ulp()` `Increment()` `Decrement()` `ScaleByPowerOfTwo(int|EInteger[, EContext])` — context-free binary scaling and unit-step traversal, exact at every precision.
- `[EFLOAT_NEIGHBOUR]`: `NextPlus(EContext)` `NextMinus(EContext)` — the context is REQUIRED and decides the answer, because the neighbour of a value exists only against a declared precision and exponent range. Both signal `FlagInvalid` and return NaN on a null context, a zero precision, or an UNLIMITED exponent range — so `EContext.Unlimited`, the context `[04]-[LOCAL_ADMISSION]` seats the predicate ladder on, yields a silent NaN with no throw. Their working context is the interval bracket's own `EContext.ForPrecisionAndRounding(53, …).WithPrecisionInBits(true)`.
- `[EFLOAT_NARROWING]`: `ToEInteger` `ToEIntegerIfExact` `ToSizedEInteger` `ToDouble` `ToSingle` `ToEDecimal` — exact integer conversions return null for non-integers; `ToDouble`/`ToSingle` are lossy readouts.
- `[EFLOAT_CLASSIFY]`: `IsZero` `IsNegative` `IsFinite` `IsNaN()` `IsInfinity()` `IsSignalingNaN()` — exact sign and IEEE classification.
- `[EFLOAT_ANCHORS]`: `Zero` `One` `Ten` `NaN` `SignalingNaN` `PositiveInfinity` `NegativeInfinity` `NegativeZero` — canonical finite and non-finite fields.

[ENTRYPOINT_SCOPE]: `ERational` is an `EInteger`-backed exact rational; `FromEFloat`/`FromDouble` decompose IEEE values without rounding, and `CompareToBinary`/`CompareToDecimal` cross-compare against `EFloat`/`EDecimal` exactly.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `ERational.Zero` / `One` / `Ten`             | static   | canonical constants; the zero vector  |
|  [02]   | `ERational.Create(EInteger, EInteger)`       | factory  | exact pair; `(int,int)`/`(long,long)` |
|  [03]   | `ERational.FromInt32(int)` / `FromEInteger`  | factory  | integer lift — each UnitsNet axis     |
|  [04]   | `ERational.FromEFloat(EFloat)`               | factory  | exact binary-float lift               |
|  [05]   | `ERational.FromEDecimal(EDecimal)`           | factory  | exact decimal lift — the CAS bridge   |
|  [06]   | `ERational.FromDouble(double)`               | factory  | lossless dyadic lift                  |
|  [07]   | `ERational.FromString(string)`               | factory  | rational literal parse                |
|  [08]   | `ERational.Numerator` / `Denominator`        | property | exact `EInteger` components           |
|  [09]   | `ERational.Sign`                             | property | exact normalized sign                 |
|  [10]   | `ERational.ToLowestTerms()`                  | method   | canonical normalization               |
|  [11]   | `ERational.CompareToBinary(EFloat)`          | method   | exact cross-compare vs binary float   |
|  [12]   | `ERational.CompareToDecimal(EDecimal)`       | method   | exact cross-compare vs decimal        |
|  [13]   | `ERational.ToEInteger() -> EInteger`         | method   | exact integer narrowing               |
|  [14]   | `ERational.ToSizedEInteger(int) -> EInteger` | method   | bit-bounded narrowing                 |
|  [15]   | `ERational.ToDouble()` / `ToEDecimal(ctx)`   | method   | lossy readout under explicit context  |

- `[ERATIONAL_ARITHMETIC]`: `Add` `Subtract` `Multiply` `Divide` `Remainder` — infinite-precision over `EInteger`, rational/`int`/`long` operands; operator spellings (`+` `-` `*` `/`) alias the instance folds.
- `[ERATIONAL_SIGN]`: `Abs` `Negate` `CopySign` `Increment` `Decrement` — exact sign and step transforms.
- `[ERATIONAL_CLASSIFY]`: `IsZero` `IsNegative` `IsFinite` `IsInteger()` `IsNaN()` `IsInfinity()` — sign, finiteness, integrality, specials.
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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Representation independence: `EInteger` shares no substrate with `System.Numerics.BigInteger`, `Fraction`, or `BigRational`, so agreement between the `PeterO` and `Fraction` oracles verifies a determinant sign across unrelated implementations.
- Exactness gate: `EContext.Unlimited` or an omitted context holds `Add`/`Subtract`/`Multiply` exact, a finite context rounds through `ERounding`, and `WithBlankFlags()` with `Flags == 0` certifies no `FlagInexact`.
- Predicate fold: determinant ordinates lift through `EFloat.FromDouble`, accumulate exactly under `Unlimited`, and `EFloat.Sign` yields `-1`/`0`/`+1` before the value is discarded.
- Comparison surface: `Sign` and `CompareToTotal` order values through `IComparable<T>`/`IEquatable<T>`, never generic-math or span-parse bindings; every equality read spells `Equals` and every ordering read `CompareTo` — the type ships no `==`/`<` operators, so a phantom operator spelling fails at compile and generated structural equality composes `Equals`/`GetHashCode`.
- Rational arithmetic NEVER reduces: `+`, `-`, `*`, `/`, and `Pow` return an unreduced numerator/denominator pair, and `Equals` compares those components EXACTLY, so `1/2` and `2/4` are distinct values under equality while `CompareTo` reads them equal. Canonicalizing through `ToLowestTerms()` at a carrier's one mint is what keeps an exponent-vector dictionary key single-valued; without it one physical dimension keys two slots depending on the operation order that produced it.
- Lossy egress (`ToDouble`, `ToEDecimal(EContext)`) runs only at a render or diagnostic edge, never inside the exponent group algebra; `NaN`/infinity specials never enter a `DimensionMonomial` — admission lifts finite sources through `FromInt32`/`FromEDecimal`.

[STACKING]:
- `TYoshimura.DoubleDouble`(`Rasm/.api/api-doubledouble.md`): `ddouble` is the fixed 106-bit tier below, its `TwoProduct`/`TwoSum` transforms matching the `Expansion` kernel; only sub-106-bit-indeterminate residue promotes to `EFloat`/`ERational`, confining heap cost to the degenerate set.
- `ExtendedNumerics.BigRational`(`Rasm/.api/api-bigrational.md`): `Fraction` is the `System.Numerics.BigInteger`-backed exact oracle at the same altitude; `ERational` is the independent-representation twin and `EFloat` the exact binary adjudicator, cross-checking rather than collapsing onto one implementation.
- `AngouriMath`(`Rasm.Compute/.api/api-angourimath.md`): `Entity.Number.Rational` carries `ERational` and `Real` carries `EDecimal`; `FromEDecimal` is the one lift from a CAS literal into the exponent algebra.
- `UnitsNet`(`api-unitsnet.md`): the seven `BaseDimensions` `int` axes lift through `ERational.FromInt32` into the ℚ⁷ vector.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions.md`): `[ValueObject<Seq<ERational>>]` structural equality rides `ERational.Equals`, making the monomial a comparer-free dictionary key.
- Kernel consumer anchor: the in-house adaptive Shewchuk `Expansion` kernel and `EFloat` evaluate one determinant sign through unrelated arithmetic; a `CsCheck` differential test compares `Expansion.Sign`, `EFloat.Sign`, `Fraction.Sign`, and `ERational.Sign` as a four-way invariant. `ERounding.Floor`/`Ceiling` with `ForPrecisionAndRounding(53, …).WithPrecisionInBits(true)` and `RoundToExponentExact` bracket the software-rounded interval endpoints of the sign filter — the form `Numerics/predicates.md` `Interval` composes at its `Down`/`Up` statics, and the SAME finite bounded context `NextPlus`/`NextMinus` require to answer at all.
- Compute consumer anchor: `Symbolic` owns the ℚ⁷ dimensional-proof lane — dimensional consistency decides by exact `ERational` equality, so a float-rounded exponent never admits an inconsistent formula — and `Stats/estimator` the `EFloat` criterion-accumulation lane: per-sample log-likelihood terms lift exactly, fold with no intermediate rounding, and round ONCE under `EContext.Binary64` at the terminal readout.

[LOCAL_ADMISSION]:
- `EFloat` under `EContext.Unlimited` is the arbitrary-precision binary tier of the kernel predicate ladder above interior `double`, 106-bit `ddouble`, and exact `Expansion`. `Unlimited` carries an unlimited exponent range, so it governs the accumulate-and-read-sign path ALONE — the adjacency neighbours and every analytic operation take the bracket's finite bounded context instead, and reusing the ladder's own context there returns NaN or raises `FlagInvalid` rather than refusing.
- `EDecimal` carries General Decimal Arithmetic and IEEE-754 decimal semantics for exact human-readable I/O and banker's rounding; geometry adjudication uses `EFloat` and `ERational`.
- At Compute, `ERational` is the sole dimension-exponent carrier and the CAS numeric leaves consume the engine's own carriers with zero conversion loss.
