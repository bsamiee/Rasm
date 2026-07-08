# [RASM_API_PETERONUMBERS]

`PeterO.Numbers` supplies four arbitrary-precision value types — `EInteger` (big integer),
`EDecimal` (arbitrary-precision base-10 floating point), `EFloat` (arbitrary-precision base-2
/ binary floating point), and `ERational` (arbitrary-precision rational `Numerator`/
`Denominator`) — every operation parameterized by an `EContext` precision/rounding/exponent
policy with an explicit `ERounding` mode and IEEE-754-style trap/flag accounting. For the
Geometry predicate floor it adds the arbitrary-precision BINARY floating-point tier (`EFloat`)
*between* the fixed 106-bit `TYoshimura.DoubleDouble` middle tier (`api-doubledouble`) and the
`ExtendedNumerics.BigRational` exact-rational oracle (`api-bigrational`): under
`EContext.Unlimited` the polynomial orient/incircle/insphere determinants over dyadic-double
coordinates evaluate EXACTLY in a representation INDEPENDENT of `BigRational`, so `EFloat` is a
second-source exact adjudicator that cross-validates the hand-rolled `Expansion` sign, and
`ERational` is a third independent exact-rational adjudicator (a `BigInteger` rational in a
different library than `Fraction`). `ERounding.Floor`/`Ceiling` give software directed rounding
for an interval-arithmetic sign filter, and the `EContext.Flag*` set reports `FlagInexact`/
`FlagInvalid`/`FlagDivideByZero` so an exact computation proves its own exactness. Like
`BigRational` and UNLIKE `ddouble`, none of the four types implements `System.Numerics`
generic math — they are plain `IComparable<T>`/`IEquatable<T>` values composed by hand.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PeterO.Numbers`
- package: `PeterO.Numbers`
- license: CC0-1.0 (Peter Occil; public-domain dedication — NOT MIT; `github.com/peteroupc/Numbers`)
- assembly: `Numbers.dll` (the assembly name is `Numbers`, NOT `PeterO.Numbers`)
- namespace: `PeterO.Numbers` (`EInteger`/`EDecimal`/`EFloat`/`ERational`/`EContext`/`ERounding`/`ETrapException` + the `EDecimals`/`EFloats`/`Extras` general-arithmetic-spec static helpers)
- target: multi-target (`lib/net20`, `lib/net40`, `lib/netstandard1.0`); the `net10.0` consumer binds `lib/netstandard1.0` — the highest available is `netstandard1.0`, so there is NO `net5.0`+ asset and NO `Span<char>` parse overload; the surface is the netstandard1.0 ABI on every modern runtime
- asset: pure-managed runtime library, AnyCPU, no native runtime and ZERO package dependencies; `osx-arm64`-safe — `EInteger` is a self-contained `uint[]`-backed bignum, NOT a `System.Numerics.BigInteger` wrapper, so the type is fully decoupled from the BCL bignum and from the `BigRational`/`Fraction` `BigInteger` representation
- abi: NO generic-math interfaces — `EInteger`/`EDecimal`/`EFloat`/`ERational` are `sealed class` values over `IComparable<T>`/`IEquatable<T>` only (not `INumber<T>`, not `ISpanParsable<T>`, not `IFormattable` beyond the explicit `ToString` overloads); a rail composes them as hand-typed values, never through `INumberBase<T>` — the same constraint as `api-bigrational`, the opposite of `api-doubledouble`
- rail: arbitrary-precision exact adjudicator (binary `EFloat` tier + independent `ERational` oracle)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the four number types + the context/rounding policy (namespace `PeterO.Numbers`)
- rail: arbitrary-precision exact adjudicator

The four numeric carriers interconvert losslessly upward (`EInteger` -> `EFloat`/`EDecimal`/
`ERational`; `EFloat`/`EDecimal` -> `ERational` exactly) and adjudicate downward through
`EContext`. `EContext` is the policy object EVERY rounding operation takes; `EContext.Unlimited`
is the exact (no-rounding) mode the predicate adjudication runs under. The `FastInteger`/
`FastIntegerFixed`/`BitShiftAccumulator`/`RadixMath`/`IRadixMath*` types are `internal`
arithmetic machinery — never the consumed surface even where a `IRadixMathHelper<T>` explicit
interface implementation leaks `public` (`GetMantissa`/`GetExponent`/`CreateNewWithFlags`/
`GetRadix` on `EFloat`/`EDecimal` are interface plumbing, not consumer API).

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:---------- |:----------------------------------- |:--------------------------------------------------------------------------------------------------- |
| [01] | `EInteger` | arbitrary-precision integer | `sealed class`; self-contained `uint[]` bignum; `Add`/`Subtract`/`Multiply`/`DivRem`/`Pow`/`Gcd`/`Sqrt`/`Mod`/`ModPow`, exact `Sign`/`CompareTo`, `FromBytes`/`ToBytes` — the EXACT integer the predicate determinant numerators/denominators are built from |
| [02] | `EFloat` | arbitrary-precision binary FP | `sealed class`; mantissa×2^exponent; every op `EContext`-parameterized; `Sqrt`/`Pow`/`Exp`/`Log`, exact under `EContext.Unlimited` for the dyadic-coordinate determinant — the INDEPENDENT binary exact adjudicator between `ddouble` and `Fraction` |
| [03] | `EDecimal` | arbitrary-precision decimal FP | `sealed class`; mantissa×10^exponent; the General Decimal Arithmetic (IEEE 754-2008 decimal) carrier — exact decimal I/O and human-readable rounding, NOT the geometry interior tier |
| [04] | `ERational` | arbitrary-precision rational | `sealed class`; `EInteger Numerator`/`Denominator`; exact `Sign`/`CompareTo`/`ToLowestTerms`, `CompareToBinary(EFloat)`/`CompareToDecimal(EDecimal)` cross-tier compare — a THIRD independent exact-rational oracle parallel to `Fraction` in a different library |
| [05] | `EContext` | precision / rounding / exponent policy | `sealed class`; `Precision`/`Rounding`/`EMin`/`EMax`/`Traps`/`Flags`; the `Unlimited` (exact), `Binary64`/`Binary128`/`Decimal*` IEEE, and `ForPrecision`/`ForRounding` factories — the policy EVERY rounding op consumes |
| [06] | `ERounding` | rounding-mode discriminant | `enum`; `None`/`Up`/`Down`/`Ceiling`/`Floor`/`HalfUp`/`HalfDown`/`HalfEven`/`Odd`/`ZeroFiveUp`/`OddOrZeroFiveUp` — `Floor`/`Ceiling` are the directed-rounding modes for an interval/bracket filter; `None` raises on inexact (exact-proof mode) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EContext` — the precision/rounding/exponent policy
- rail: arbitrary-precision exact adjudicator

`EContext` is constructed once and threaded into every rounding operation. `Unlimited` is the
exact mode the predicate determinant runs under; `Binary64`/`Binary128` mirror IEEE `double`/
`quad`; the `With*` builders derive a context immutably. `Flags`/`HasFlags`/`Traps` carry the
IEEE-754 condition accounting — under `WithBlankFlags()` an operation records whether it was
`FlagInexact`, so an exact result is PROVABLY exact (no inexact flag raised).

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------- |:------------- |:------------------------------------------------------------------- |
| [01] | `EContext.Unlimited` / `UnlimitedHalfEven` | static field | exact arithmetic, unlimited precision, no exponent clamp — the predicate adjudication context (`ERounding.None`/`HalfEven`) |
| [02] | `EContext.Binary16/32/64/128` / `Decimal32/64/128` / `CliDecimal` / `Basic` | static field | preconfigured IEEE-754 binary + decimal interchange contexts (precision + exponent range + `HalfEven`) |
| [03] | `EContext.ForPrecision(int)` / `ForPrecisionAndRounding(int, ERounding)` / `ForRounding(ERounding)` | static factory | derive a context by digit precision and/or rounding mode (`precision=0` = unlimited) |
| [04] | `new EContext(int precision, ERounding, int expMin, int expMax, bool clampNormalExponents)` / `(EInteger …)` | constructor | full custom context (small-int or `EInteger` precision + exponent bounds) |
| [05] | `EContext.WithRounding(ERounding)` / `WithPrecision(int)` / `WithExponentRange(int,int)` / `WithExponentClamp(bool)` / `WithSimplified(bool)` / `WithPrecisionInBits(bool)` / `WithUnlimitedExponents()` | builder | immutable derivation of a modified context |
| [06] | `EContext.WithBlankFlags()` / `WithNoFlags()` / `WithTraps(int)` / `WithNoFlagsOrTraps()` / `GetNontrapping()` | builder | enable/disable IEEE condition-flag recording and trap raising |
| [07] | `EContext.Flags` / `HasFlags` / `HasFlagsOrTraps` / `Traps` | instance prop | read the raised condition flags (`FlagInexact`/`FlagInvalid`/…) after an operation — the exactness proof readout |
| [08] | `EContext.FlagInexact` / `FlagInvalid` / `FlagDivideByZero` / `FlagOverflow` / `FlagUnderflow` / `FlagSubnormal` / `FlagRounded` / `FlagClamped` / `FlagLostDigits` | const int | the IEEE-754 condition-flag bit constants (OR-combined into `Flags`/`Traps`) |

[ENTRYPOINT_SCOPE]: `EFloat` — the arbitrary-precision binary tier (the predicate adjudicator)
- rail: arbitrary-precision exact adjudicator

`EFloat` is `mantissa × 2^exponent` over `EInteger`. `FromDouble` lifts an IEEE `double`
EXACTLY (every finite double is a dyadic rational, representable with no rounding), so the
geometry determinant's double-coordinate inputs cross into the exact binary tier losslessly.
Arithmetic without an `EContext` (or with `EContext.Unlimited`) is exact; with a finite-
precision context it rounds per `ERounding`. The predicate computes the determinant in `EFloat`
under `Unlimited`, reads `Sign`, and discards the value.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------- |:------------- |:------------------------------------------------------------------- |
| [01] | `EFloat.FromDouble(double)` / `FromSingle(float)` / `FromDoubleBits(long)` / `FromSingleBits(int)` | static factory | LOSSLESS lift of an IEEE binary float (every finite `double` is exactly dyadic — no rounding) |
| [02] | `EFloat.FromEInteger(EInteger)` / `Create(EInteger mantissa, EInteger exponent)` / `Create(long, int)` | static factory | exact `mantissa × 2^exponent` construction from the big-integer tier |
| [03] | `EFloat.FromString(string[, EContext])` / `FromString(string, int offset, int length[, EContext])` | static factory | parse a decimal/hex literal (rounded into `ctx` precision; `byte[]`/`char[]` overloads — NO `ReadOnlySpan<char>` on netstandard1.0) |
| [04] | `EFloat.Zero` / `One` / `Ten` / `NaN` / `SignalingNaN` / `PositiveInfinity` / `NegativeInfinity` / `NegativeZero` | static field | canonical anchors + non-finite values |
| [05] | `EFloat.Add/Subtract/Multiply/Divide(EFloat[, EContext])` (+ `int`/`long` operand overloads) | instance | exact (no `ctx` / `Unlimited`) or rounded (finite `ctx`) binary FP arithmetic — the determinant accumulation |
| [06] | `EFloat.Sqrt(EContext)` / `SquareRoot(EContext)` / `Pow(EFloat / int[, EContext])` / `Exp(ctx)` / `Log(ctx)` / `Log10(ctx)` / `LogN(EFloat, ctx)` | instance | square root / power / transcendental (REQUIRE a finite `ctx` — irrational results are inexact) |
| [07] | `EFloat.Abs([ctx])` / `Negate([ctx])` / `CopySign(EFloat)` / `Reduce(ctx)` / `Plus(ctx)` | instance | magnitude / sign / trailing-zero reduction |
| [08] | `EFloat.Sign` / `IsZero` / `IsNegative` / `IsFinite` / `IsNaN()` / `IsInfinity()` / `IsSignalingNaN()` | instance prop/method | exact sign + IEEE classification — `Sign` is the predicate verdict readout |
| [09] | `EFloat.CompareTo(EFloat / int / long)` / `CompareToValue(...)` / `CompareToTotal([ctx])` / `CompareToSignal(EFloat, ctx)` | instance | value ordering vs IEEE total-order (`CompareToTotal` orders NaN/±0) |
| [10] | `EFloat.RoundToExponent(EInteger, ctx)` / `RoundToExponentExact(EInteger, ERounding)` / `RoundToIntegerExact(ctx)` / `RoundToPrecision(ctx)` / `Quantize(EFloat, ctx)` | instance | directed rounding to a target exponent/precision (the interval-bracket primitive with `ERounding.Floor`/`Ceiling`) |
| [11] | `EFloat.ScaleByPowerOfTwo(int / EInteger[, ctx])` / `Ulp()` / `NextPlus(ctx)` / `NextMinus(ctx)` / `Increment()` / `Decrement()` | instance | binary exponent scale, ULP step, adjacent-value walk — interval-filter endpoints |
| [12] | `EFloat.ToEInteger()` / `ToEIntegerExact()` / `ToEIntegerIfExact()` / `ToSizedEInteger(int)` / `ToDouble()` / `ToSingle()` / `ToInt32Checked()` / `ToEDecimal()` | instance | narrowing readout (`*Exact`/`*IfExact` throw/null on a non-integer; `ToDouble` is the lossy boundary readout) |
| [13] | `EFloat.Mantissa` / `UnsignedMantissa` / `Exponent` / `Precision()` | instance prop | the `EInteger` significand + binary exponent + significant-digit count (decomposition for an exact bound) |

[ENTRYPOINT_SCOPE]: `ERational` — the independent exact-rational oracle
- rail: arbitrary-precision exact adjudicator

`ERational` is an `EInteger`-backed exact rational, the SAME role as `ExtendedNumerics`
`Fraction` (`api-bigrational`) but in an independent library and representation. `FromEFloat`/
`FromDouble` decompose an IEEE value into an exact rational with NO rounding; `CompareToBinary`/
`CompareToDecimal` compare an `ERational` against an `EFloat`/`EDecimal` EXACTLY across tiers —
the cross-representation bridge a differential predicate test compares against.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------- |:------------- |:------------------------------------------------------------------- |
| [01] | `ERational.Create(EInteger numerator, EInteger denominator)` / `Create(int, int)` / `Create(long, long)` | static factory | exact rational from a numerator/denominator pair |
| [02] | `ERational.FromEInteger(EInteger)` / `FromEFloat(EFloat)` / `FromEDecimal(EDecimal)` / `FromDouble(double)` / `FromDoubleBits(long)` / `FromSingle(float)` / `FromDecimal(decimal)` / `FromInt32/64(...)` | static factory | LOSSLESS lift of any integer / IEEE float / `EFloat` / `EDecimal` into an exact rational |
| [03] | `ERational.FromString(string)` / `FromString(byte[] / char[][, offset, length])` | static factory | parse a rational literal (NO span overload on netstandard1.0) |
| [04] | `ERational.Numerator` / `Denominator` / `Sign` / `IsZero` / `IsInteger()` / `IsFinite` | instance prop/method | exact `EInteger` numerator/denominator + exact sign / zero / integrality test — the oracle verdict |
| [05] | `ERational.Add/Subtract/Multiply/Divide(ERational)` (+ `int`/`long` overloads) / `Remainder(ERational)` | instance | exact infinite-precision rational arithmetic |
| [06] | `ERational.ToLowestTerms()` / `Abs()` / `Negate()` / `CopySign(ERational)` / `Increment()` / `Decrement()` | instance | canonicalize to lowest terms / sign transforms |
| [07] | `ERational.CompareTo(ERational / int / long)` / `CompareToValue(...)` / `CompareToBinary(EFloat)` / `CompareToDecimal(EDecimal)` / `CompareToTotal(ERational)` | instance | total exact ordering — `CompareToBinary`/`CompareToDecimal` are the EXACT cross-tier comparisons (rational vs binary/decimal float) |
| [08] | `ERational.ToEInteger()` / `ToEIntegerIfExact()` / `ToSizedEInteger(int)` / `ToDouble()` / `ToInt32Checked()` | instance | narrowing readout (`ToDouble` lossy boundary; `*IfExact` exact-or-null) |

[ENTRYPOINT_SCOPE]: `EInteger` — the exact big-integer the determinant builds from
- rail: arbitrary-precision exact adjudicator

`EInteger` is the self-contained `uint[]` bignum the rational numerators/denominators and the
binary mantissas are built from — decoupled from `System.Numerics.BigInteger` and from the
`Fraction` representation, so the two exact oracles share NO underlying integer type.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------- |:------------- |:------------------------------------------------------------------- |
| [01] | `EInteger.Zero` / `One` / `FromInt32(int)` / `FromInt64(long)` / `FromString(string)` / `FromBytes(byte[], bool littleEndian)` | static factory | exact integer construction (incl. raw two's-complement bytes) |
| [02] | `EInteger.Add/Subtract/Multiply/Divide(EInteger)` (+ `int`/`long`) / `DivRem(EInteger[, out])` / `Mod(EInteger)` / `Remainder(...)` | instance/static | exact integer arithmetic + combined quotient/remainder |
| [03] | `EInteger.Pow(int / long / EInteger)` / `Sqrt()` / `Gcd(EInteger)` / `ModPow(EInteger pow, EInteger mod)` / `Abs()` / `Negate()` | instance | exact power / integer square root / GCD / modular exponentiation |
| [04] | `EInteger.Sign` / `IsZero` / `IsEven` / `CompareTo(EInteger / int / long)` / `GetSignedBit(int)` / `GetSignedBitLength()` | instance | exact sign / parity / ordering / bit inspection |
| [05] | `EInteger.ToBytes(bool littleEndian)` / `ToInt64Checked()` / `ToInt32Checked()` / `ToString()` | instance | narrowing / serialization readout |

## [04]-[IMPLEMENTATION_LAW]

[ADJUDICATOR_PROFILE]:
- representation independence: `EInteger` is a self-contained `uint[]` bignum — it shares NO underlying type with `System.Numerics.BigInteger` or the `ExtendedNumerics` `Fraction`/`BigRational`. That independence is the WHOLE point of admitting this package alongside `api-bigrational`: a determinant sign confirmed by BOTH the `Fraction` `BigInteger` oracle AND the `PeterO` `EFloat`/`ERational` oracle is verified across two unrelated arbitrary-precision implementations, defeating a shared-implementation blind spot.
- exactness gate: `EContext.Unlimited` (or no `ctx`) makes `Add`/`Subtract`/`Multiply` exact; a finite `ctx` rounds per `ERounding`. `Sqrt`/`Pow`/`Exp`/`Log` are inexact (irrational) and REQUIRE a finite `ctx` — they are forbidden inside a predicate adjudication and confined to non-predicate readout, exactly as `Fraction.Sqrt`/`NthRoot`/`Log` are.
- exactness proof: under `WithBlankFlags()` an exact computation raises NO `FlagInexact`; reading `EContext.Flags == 0` after the determinant accumulation PROVES the binary-tier result is exact, so the `EFloat` adjudication is self-certifying rather than assumed.
- dyadic lossless lift: every finite IEEE `double` is a dyadic rational, so `EFloat.FromDouble` and `ERational.FromDouble` lift the determinant's double-precision coordinate inputs with ZERO rounding — the exact tier sees the true input value, not a re-rounded approximation.
- no generic math: the four types implement `IComparable<T>`/`IEquatable<T>` only — a rail must NOT bind `INumber<EFloat>`, `INumberBase<T>`, or `ISpanParsable<T>`; the comparison/equality contract plus `Sign`/`CompareToTotal` is the whole generic surface (identical constraint to `api-bigrational`, opposite of `api-doubledouble`).

[LOCAL_ADMISSION]:
- `EFloat` under `EContext.Unlimited` is the arbitrary-precision BINARY adjudicator tier of the predicate precision ladder (`.planning/Numerics/predicates#PRECISION_LADDER`): interior `double` (`NumericsPolicy`) -> `TYoshimura.DoubleDouble` 106-bit -> `Expansion` sign-exact -> `EFloat`/`ERational`/`Fraction` exact adjudication. It is the SECOND-SOURCE exact check in a representation independent of both `Expansion` and `Fraction`, reached only on the sub-106-bit-degenerate residue.
- the predicate lifts the determinant's double ordinates via `EFloat.FromDouble`, accumulates the orient/incircle/insphere polynomial with exact `Multiply`/`Subtract` under `Unlimited`, reads `EFloat.Sign`, and discards the value — the verdict is a sign (`-1`/`0`/`+1`), never materialized as a `double`.
- `ERational.FromDouble` + `CompareToBinary(EFloat)` is the EXACT cross-tier comparison the predicate law-matrix uses to confirm the `EFloat` binary accumulation and the `Fraction`/`ddouble` tiers agree on the sign of the same determinant across THREE representations.
- `ERounding.Floor`/`Ceiling` with `EContext.ForPrecisionAndRounding(53, …).WithPrecisionInBits(true)` bracket a value into a directed-rounded interval — the software-rounded endpoints of an interval-arithmetic sign filter; the `.planning/Numerics/predicates` `Interval` carrier composes exactly this as the implicit-member filter tier (`PrecisionTier.Interval`), resolving the bulk of constructed-point signs before any `Expansion`/`Fraction` work.
- `EDecimal` is NOT a geometry interior tier — it is the General Decimal Arithmetic (IEEE 754-2008 decimal) carrier for exact human-readable decimal I/O and banker's rounding; the geometry path uses `EFloat` (binary) and `ERational` (rational), never `EDecimal`, for adjudication.

[STACKING_LAW]:
- precision ladder: `EFloat`/`ERational` slot ABOVE `TYoshimura.DoubleDouble` (`api-doubledouble`, fixed 106-bit) and the `Expansion` branch, at the SAME exact-adjudication altitude as `ExtendedNumerics.BigRational` (`api-bigrational`) but in an independent library — the kernel escalates tier-by-tier and only the indeterminate residue reaches an arbitrary-precision oracle, so the heap cost is paid on the measure-zero degenerate set.
- vs BigRational (`api-bigrational`): `Fraction`/`BigRational` are the `System.Numerics.BigInteger`-backed exact-rational oracle and the predicate law-matrix's PRIMARY ground truth; `ERational` is a SECOND, independently-implemented exact rational, and `EFloat` is an exact BINARY adjudicator with no rational analogue in `ExtendedNumerics`. The two are differential cross-checks of one another, not substitutes — never collapse the determinant onto a single oracle when the design's robustness claim rests on two unrelated implementations agreeing.
- vs DoubleDouble (`api-doubledouble`): `ddouble` is the fast fixed-precision (106-bit) middle tier with full `INumber<T>` generic math; `EFloat`/`ERational` are the slow UNBOUNDED-precision exact tier with NO generic math. `ddouble` resolves the bulk; `EFloat`/`ERational` adjudicate only what `ddouble` cannot — the division is precision-vs-cost, and an `EFloat` operand never flows through a generic `where T: INumber<T>` kernel (it must be hand-typed).
- vs Expansion (in-house): the `Expansion` sign-exact branch is the kernel's own Shewchuk-style adaptive floating-point expansion; `EFloat` under `Unlimited` is an external arbitrary-precision binary FP. They compute the same determinant sign by different algorithms in different representations, so a `CsCheck` differential test (`Expansion.Sign` vs `EFloat.Sign` vs `Fraction.Sign` vs `ERational.Sign`) is a four-way agreement invariant, not a self-comparison.

[RAIL_LAW]:
- Package: `PeterO.Numbers`
- Owns: arbitrary-precision binary/decimal/rational/integer arithmetic — `EFloat` (the independent exact BINARY adjudicator under `EContext.Unlimited`), `ERational` (a second independent exact-rational oracle parallel to `Fraction`), `EInteger` (the self-contained `uint[]` bignum), `EDecimal` (exact decimal I/O), the `EContext`/`ERounding` precision/rounding/exponent/trap-flag policy with `Floor`/`Ceiling` directed rounding and `FlagInexact` exactness proof
- Accept: a predicate determinant lifted via `EFloat.FromDouble`, accumulated exactly under `EContext.Unlimited`, read via `EFloat.Sign`; a cross-tier exactness check via `ERational.FromDouble` + `CompareToBinary`/`CompareToDecimal`; an interval-bracket filter via `ERounding.Floor`/`Ceiling` + `RoundToExponentExact`; `EFloat`/`ERational`/`Expansion`/`Fraction` as the four differential tiers in the predicate law-matrix; exact decimal I/O via `EDecimal`
- Reject: binding `INumber<T>`/`INumberBase<T>`/`ISpanParsable<T>` (none implemented — hand-type the value); a `ReadOnlySpan<char>` parse overload (netstandard1.0 asset has none); `Sqrt`/`Pow`/`Exp`/`Log` inside a predicate adjudication (inexact, require a finite `ctx`); a `double`/`EDecimal` readout standing in for the exact `EFloat.Sign`/`ERational.Sign` verdict; collapsing the `EFloat`/`ERational`/`Fraction` differential cross-check onto a single oracle (the robustness claim rests on independent implementations agreeing); documenting the `internal` `FastInteger`/`RadixMath`/`IRadixMathHelper`/`BitShiftAccumulator` machinery (or the `GetMantissa`/`CreateNewWithFlags`/`GetRadix` interface-plumbing leak) as consumer API; mistaking the assembly name (`Numbers`) for the package id (`PeterO.Numbers`) or the license (CC0-1.0, not MIT)
