# [RASM_API_DOUBLEDOUBLE]

`TYoshimura.DoubleDouble` supplies `ddouble`: a single `readonly struct` carrying an
unevaluated hi/lo two-`double` (a ~106-bit significand, ~31 decimal digits) that is the
middle precision tier of the Geometry predicate ladder AND a full generic-math floating
type. Unlike `ExtendedNumerics.BigRational` (which implements no generic-math interface),
`ddouble` implements the COMPLETE `System.Numerics` hierarchy — `INumber<ddouble>`,
`INumberBase<ddouble>`, `ISignedNumber<ddouble>`, `IFloatingPointConstants<ddouble>`,
`IMinMaxValue<ddouble>`, the `IPowerFunctions`/`IRootFunctions`/`IExponentialFunctions`/
`ILogarithmicFunctions`/`ITrigonometricFunctions`/`IHyperbolicFunctions` function-group
interfaces, every arithmetic/comparison/equality operator interface, and the full
parse/format surface (`IParsable`/`ISpanParsable`/`IUtf8SpanParsable` +
`IFormattable`/`ISpanFormattable`/`IUtf8SpanFormattable`) — so a generic numeric kernel
written `where T : INumber<T>` (or any function-group constraint) binds `ddouble` directly
with no hand-typing, and `MathNet.Numerics`/`System.Numerics.Tensors` `double` code lifts
to 106-bit by substituting the type parameter. On top of the number contract it ships a
large self-contained special-function library (gamma/beta, error/Fresnel, Bessel/Airy/
Struve, elliptic + Jacobi, exponential/sine/cosine integrals, zeta/polylog/polygamma,
orthogonal polynomials, Owen's T, Lambert W, Mathieu) all evaluated at double-double
precision — the precision tier above `MathNet.Numerics.SpecialFunctions` (`double`) and
below the `BigRational` exact oracle. The double-double arithmetic uses FMA `TwoProduct`
and Knuth/Dekker `TwoSum` error-free transforms identical to the in-house `Expansion`
kernel, so the predicate's 106-bit middle stage and the `Expansion` exact branch share one
rounding model. It is NOT a `BigInteger`/arbitrary-precision type: precision is fixed at
two doubles, so it is a fast deterministic refinement of `double`, never an exact oracle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TYoshimura.DoubleDouble`
- package: `TYoshimura.DoubleDouble`
- version: `5.0.8`
- license: MIT (T.Yoshimura; `github.com/tk-yoshimura/DoubleDouble`)
- assembly: `DoubleDouble.dll`
- namespace: `DoubleDouble` (the `ddouble` value + extension/converter siblings); `DoubleDouble.Utils` (`ResourceUnpack` coefficient-table deserialization — not consumer surface)
- target: single-target `lib/net10.0` only — no multi-target fallback ambiguity, the `net10.0` consumer binds the one shipped asset
- asset: pure-managed runtime library, AnyCPU, no native runtime and ZERO package dependencies (`System.Numerics.BigInteger` is the only BCL touch, for the `BigInteger` conversion); a source-generated `System.Text.RegularExpressions.Generated` parser backs `Parse`
- abi: full `System.Numerics` generic-math — a kernel constrained `where T : INumber<T>` / `IFloatingPointConstants<T>` / `I{Power,Root,Exponential,Logarithmic,Trigonometric,Hyperbolic}Functions<T>` binds `ddouble` with no adapter; the lowercase type name `ddouble` is the public spelling
- rail: middle-precision FP (106-bit double-double)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the value + its three first-class siblings (namespace `DoubleDouble`)
- rail: middle-precision FP

`ddouble` is the whole numeric surface; the three sibling static/converter types extend it
into LINQ aggregation, binary I/O, and `System.Text.Json`. The package's `UInt128`,
`FloatSplitter`, `IntegerSplitter`, `SeriesUtil`, and the per-function `Consts` kernels are
`internal` implementation detail — none is part of the consumed surface.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]                   | [CAPABILITY]                                                                                          |
| :-----: | :----------------------------- | :------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `ddouble`                      | the double-double number         | `readonly struct`; full `INumber<ddouble>` generic-math + operators + the special-function library; opaque hi/lo (no public field/property exposing the representation) |
|  [02]   | `DoubleDoubleEnumerableExpand` | LINQ aggregation extensions      | `static`; `Sum`/`Average`/`Min`/`Max`/`MinIndex`/`MaxIndex` over `IEnumerable<ddouble>`/`IReadOnlyList<ddouble>` at 106-bit accumulation |
|  [03]   | `DoubleDoubleIOExpand`         | binary serialization extensions  | `static`; `BinaryWriter.Write(ddouble)` / `BinaryReader.ReadDDouble()` round-trip the exact hi/lo bits |
|  [04]   | `DDoubleJsonConverter`         | `System.Text.Json` converter     | `JsonConverter<ddouble>`; register on `JsonSerializerOptions.Converters` to (de)serialize `ddouble` losslessly |

## [03]-[NUMERIC_CONTRACT]

[CONTRACT_SCOPE]: construction (no public constructor — operators + `Parse` only)
- rail: middle-precision FP

Every `ddouble` constructor is `internal`; the hi/lo representation is never built directly
by a consumer. Construction is the implicit widening conversions (lossless from any integer
incl. `BigInteger`, and from `double`/`decimal`/`string`) and `Parse`/`TryParse`. The
`BigInteger` conversion is the bridge down from the `BigRational` exact tier into the 106-bit
tier; the `double` conversion is the bridge up from the conservative `double` filter.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `implicit operator ddouble(double / int / uint / long / ulong)`            | conversion     | lossless widening of any primitive integer / IEEE `double` into `ddouble` |
|  [02]   | `implicit operator ddouble(BigInteger)`                                    | conversion     | lossless `BigInteger` -> `ddouble` (the bridge down from the `BigRational` tier) |
|  [03]   | `implicit operator ddouble(decimal)`                                       | conversion     | `decimal` -> `ddouble` (exact within the 106-bit significand)        |
|  [04]   | `implicit operator ddouble(string)`                                        | conversion     | parse-on-assignment — a string literal widens straight to `ddouble` (forwards to `Parse`; full-precision decimal-literal regex) |
|  [05]   | `implicit operator ddouble((int sign, int exponent, ulong hi, ulong lo))`  | conversion     | construct from a raw bit-tuple (round-trips the `(sign,exp,hi,lo)` decomposition) |
|  [06]   | `explicit operator double / float / int / uint / long / ulong / decimal (ddouble)` | conversion | narrowing readout (rounds to the target precision — boundary only)   |
|  [07]   | `ddouble.Parse(string / ReadOnlySpan<char>[, NumberStyles][, IFormatProvider])` | static factory | parse a decimal literal at full precision (regex-backed)             |
|  [08]   | `ddouble.TryParse(... , out ddouble result)`                               | static (no-throw) | the `ISpanParsable`/`IParsable` no-throw parse mirror; all four `string`/span × style overloads + the bare `(string, out)` form |

[CONTRACT_SCOPE]: identity, limit, and named mathematical constants
- rail: middle-precision FP

The generic-math identity/limit anchors are properties; the mathematical constants are
`static readonly ddouble` fields computed to full 106-bit precision once at type init. The
named-constant set is far broader than `Pi`/`E` — it is the type's reference table for
double-double-accurate constants.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `ddouble.Zero` / `One` / `MinusOne` / `PlusZero` / `MinusZero`             | static prop    | additive/multiplicative identities + signed zeros                    |
|  [02]   | `ddouble.AdditiveIdentity` / `MultiplicativeIdentity` / `NegativeOne`      | static prop    | `INumberBase`/`ISignedNumber` identity contract                      |
|  [03]   | `ddouble.NaN` / `PositiveInfinity` / `NegativeInfinity`                    | static prop    | non-finite anchors                                                   |
|  [04]   | `ddouble.Epsilon` / `MaxValue` / `MinValue`                               | static prop    | `IMinMaxValue` range + smallest subnormal                            |
|  [05]   | `ddouble.NormalMinExponent` (`const int = -968`) / `DecimalDigits` (`const int = 31`) | const | normal-range exponent floor; guaranteed decimal digits               |
|  [06]   | `ddouble.Pi` / `E` / `RcpPi` / `RcpE` / `Ln2` / `Lb10` / `Lg2` / `LbE` / `Sqrt2` / `Point5` | static readonly | `IFloatingPointConstants` Pi/E + log/sqrt anchors at 106-bit  |
|  [07]   | `ddouble.EulerGamma` / `CatalanG` / `GoldenRatio` / `GlaisherA` / `Zeta3`..`Zeta9` / `LemniscatePi` / `ErdosBorwein` / `LambertOmega` (+ ~20 more named constants) | static readonly | reference table of double-double-accurate mathematical constants |
|  [08]   | `ddouble.BernoulliSequence` / `Factorial` / `TaylorSequence` / `StieltjesGamma` | static prop (`ReadOnlyCollection<ddouble>`) | precomputed 106-bit Bernoulli / factorial / Taylor / Stieltjes coefficient tables |

[CONTRACT_SCOPE]: arithmetic, comparison, and IEEE classification
- rail: middle-precision FP

Arithmetic is the full operator set with `double`/`int`/`uint`/`long`/`ulong` mixed-operand
overloads on both sides (so a `ddouble`-`double` expression never forces a widening
allocation), plus `checked` increment/decrement. Comparison is total (`< <= > >= == !=`,
each with primitive mixed overloads) and `CompareTo`/`Equals`/`GetHashCode`. Classification
is the full `INumberBase` static predicate set.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `operator + - * / %` (`ddouble`×`ddouble`, and mixed `ddouble`×{`double`,`int`,`uint`,`long`,`ulong`} both orders) | operator | error-free-transform arithmetic; mixed overloads avoid a widening step |
|  [02]   | `operator + - ` (unary) / `++` / `--` / `checked ++` / `checked --`        | operator       | unary sign / increment / overflow-checked increment                  |
|  [03]   | `operator < <= > >= == !=` (`ddouble`×`ddouble` + mixed primitive overloads) | operator     | total ordering / equality without a widening conversion              |
|  [04]   | `ddouble.Sign(value)` (`int`)                                              | static         | `-1`/`0`/`+1` exact sign of the hi/lo value — the predicate sign readout |
|  [05]   | `ddouble.Abs` / `CopySign` / `MaxMagnitude` / `MinMagnitude` / `MaxMagnitudeNumber` / `MinMagnitudeNumber` | static | magnitude + sign-transfer (`INumberBase`)                       |
|  [06]   | `ddouble.Min` / `Max` (2/3/4-arg) / `Clamp(v,min,max)`                     | static         | n-ary min/max + clamp                                                |
|  [07]   | `ddouble.IsNaN` / `IsFinite` / `IsInfinity` / `IsPositiveInfinity` / `IsNegativeInfinity` / `IsNormal` / `IsSubnormal` / `IsZero` / `IsPlusZero` / `IsMinusZero` / `IsInteger` / `IsEvenInteger` / `IsOddInteger` / `IsPositive` / `IsNegative` / `IsCanonical` / `IsRealNumber` / `IsImaginaryNumber` / `IsComplexNumber` | static (`bool`) | full IEEE + `INumberBase` classification (a real-only type: `IsRealNumber`/`IsCanonical` are always true on a finite value, `IsImaginaryNumber`/`IsComplexNumber` always false) |
|  [08]   | `ddouble.CompareTo` / `Equals(ddouble)` / `Equals(x,y)` / `GetHashCode()` | instance/static | `IComparable<T>`/`IEquatable<T>`/`IEqualityComparer<ddouble>` contract |
|  [09]   | `ddouble.Floor` / `Ceiling` / `Round` / `Truncate`                        | static         | rounding to integral `ddouble`                                       |
|  [10]   | `ddouble.Ldexp(x,n)` / `ScaleB(x,n)` (`int`/`long`) / `Frexp(x)` (`(int exp, ddouble value)`) / `ILogB(x)` (`int`) / `BitIncrement` / `BitDecrement` / `TruncateMantissa(x,keep_bits)` | static | exponent scale / decompose (`Frexp`) / binary-exponent extract (`ILogB`), ULP step, mantissa truncation (bit-level control) |
|  [11]   | `ddouble.AdjustScale(int exp, ddouble x)` / `AdjustScale(int exp, (ddouble,ddouble))` / `…(…,ddouble,ddouble)` / `…(…,ddouble,ddouble,ddouble)` | static | shared-exponent alignment of a 1-/2-/3-/4-component tuple — returns `(int exp, … scaled)` so a coordinate tuple normalizes to one binary exponent before an error-free-transform determinant (the geometry-predicate scaling primitive) |

[CONTRACT_SCOPE]: `INumberBase` generic-math conversion (cross-`T` interop)
- rail: middle-precision FP

The `TryConvertFrom*`/`TryConvertTo*` triad is the generic bridge: it converts between
`ddouble` and ANY `INumberBase<TOther>` under checked/saturating/truncating rounding, so a
polymorphic kernel `where T : INumber<T>` round-trips a `ddouble` operand to/from any other
generic-math type (e.g. `double`, `Half`, `decimal`, a third-party big-float) without a
hand-coded conversion table.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `ddouble.TryConvertFromChecked / FromSaturating / FromTruncating<TOther>(TOther, out ddouble)` | static (no-throw) | `INumberBase<TOther>` -> `ddouble` under the three rounding modes |
|  [02]   | `ddouble.TryConvertToChecked / ToSaturating / ToTruncating<TOther>(ddouble, out TOther)` | static (no-throw) | `ddouble` -> any `INumberBase<TOther>` under the three rounding modes |
|  [03]   | `ddouble.TryFormat(Span<char>, out int, ReadOnlySpan<char> format, IFormatProvider)` / `ToString([format][, provider])` | instance | `ISpanFormattable`/`IFormattable` render (UTF-8 mirrors via `IUtf8SpanFormattable`) |

## [04]-[ELEMENTARY_AND_SPECIAL_FUNCTIONS]

[FUNCTION_SCOPE]: power, root, exp, log (the function-group interfaces)
- rail: middle-precision FP

These satisfy `IPowerFunctions`/`IRootFunctions`/`IExponentialFunctions`/
`ILogarithmicFunctions`, so a generic kernel constrained on those interfaces gets the
106-bit implementations for free. `Pow1p`/`Pow2m1`/`Exp*M1`/`Log*P1` are the
accuracy-preserving near-1/near-0 variants, and `Rcp` is the fast double-double reciprocal.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Pow(x,y)` / `Pow(x,long n)` / `Pow1p(x,y)` / `Pow2(x)` / `Pow2m1(x)` / `Pow10(x)` | general / integer power, `(1+x)^y`, base-2/base-10 power, `2^x − 1`  |
|  [02]   | `Sqrt(x)` / `Cbrt(x)` / `RootN(x,int n)` / `Square(x)` / `Cube(x)` / `Rcp(x)` | square / cube / n-th root, `x²`/`x³`, reciprocal                     |
|  [03]   | `Hypot(x,y)` / `Hypot(x,y,z)` / `Agm(a,b)` / `GeometricMean(a,b[,c])`      | overflow-safe Euclidean norm (2D/3D), arithmetic-geometric mean, geometric mean |
|  [04]   | `Exp(x)` / `Exp2(x)` / `Exp10(x)` / `Expm1(x)` / `Exp2M1(x)` / `Exp10M1(x)` | base-e/2/10 exponential + `eˣ − 1` accuracy variants                 |
|  [05]   | `Log(x)` / `Log(x,b)` / `Log2(x)` / `Log10(x)` / `Log1p(x)` / `LogP1(x)` / `Log2P1(x)` / `Log10P1(x)` | natural / arbitrary-base / base-2/10 log + `log(1+x)` variants      |
|  [06]   | `Expit(x)` / `Logit(x)` / `Bump(x)`                                        | logistic sigmoid, logit, smooth bump — ready statistical/activation kernels |

[FUNCTION_SCOPE]: trigonometric, hyperbolic, and their inverses
- rail: middle-precision FP

Satisfies `ITrigonometricFunctions`/`IHyperbolicFunctions`. Includes the `*Pi` argument-in-
half-turns variants (exact at rational multiples of pi) and the `Sinc`/`Sinhc`/`Jinc`
cardinal forms used in signal/optics kernels.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Sin` / `Cos` / `Tan` / `Asin` / `Acos` / `Atan` / `Atan2(y,x)`           | circular trig + 2-arg arctangent                                     |
|  [02]   | `SinCos(x)` (`(ddouble Sin, ddouble Cos)`) / `SinCosPi(x)` (`(ddouble SinPi, ddouble CosPi)`) | fused sine+cosine in one argument reduction — the rotation-matrix / polar-to-Cartesian primitive (one range-reduce, two outputs) |
|  [03]   | `SinPi` / `CosPi` / `TanPi` / `AsinPi` / `AcosPi` / `AtanPi` / `Atan2Pi`  | half-turn (×pi) argument variants — exact at rational angle multiples |
|  [04]   | `Sinh` / `Cosh` / `Tanh` / `Asinh` / `Acosh` / `Atanh`                     | hyperbolic + inverse hyperbolic                                      |
|  [05]   | `Sinc(x[,normalized])` / `Sinhc(x)` / `Jinc(x)`                            | sine-cardinal (normalized/unnormalized), hyperbolic sinc, jinc (J1-cardinal) |

[FUNCTION_SCOPE]: gamma, beta, digamma, and incomplete forms
- rail: middle-precision FP

The full gamma/beta tower at 106-bit, including the log-space, reciprocal, inverse, and
regularized incomplete forms required for distribution-tail and Bayesian work where
`double`-precision `MathNet` loses digits.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Gamma(x)` / `LogGamma(x)` / `RcpGamma(x)` / `InverseGamma(x)`             | gamma, log-gamma, reciprocal-gamma, functional inverse               |
|  [02]   | `Digamma(x)` / `InverseDigamma(x)` / `Polygamma(int n, x)`                 | digamma ψ, its inverse, n-th polygamma                               |
|  [03]   | `Beta(a,b)` / `LogBeta(a,b)` / `IncompleteBeta(x,a,b)` / `IncompleteBetaRegularized(x,a,b)` / `InverseIncompleteBeta(x,a,b)` | beta, log-beta, (regularized/inverse) incomplete beta |
|  [04]   | `LowerIncompleteGamma(nu,x)` / `UpperIncompleteGamma(nu,x)` / `…Regularized` / `InverseLowerIncompleteGamma` / `InverseUpperIncompleteGamma` | (regularized/inverse) lower & upper incomplete gamma |
|  [05]   | `Binomial(int n,int k)` / `HarmonicNumber(int n)` / `BarnesG(x)` / `LogBarnesG(x)` | binomial coefficient, harmonic number, Barnes G + log-Barnes G       |

[FUNCTION_SCOPE]: error, Fresnel, Dawson, and the integral functions
- rail: middle-precision FP

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Erf(x)` / `Erfc(x)` / `Erfcx(x)` / `Erfi(x)` / `InverseErf(x)` / `InverseErfc(x)` | error, complementary, scaled-complementary, imaginary error + inverses |
|  [02]   | `FresnelC(x)` / `FresnelS(x)` / `FresnelF(x)` / `FresnelG(x)` / `DawsonF(x)` | Fresnel cosine/sine + auxiliary f/g, Dawson F                        |
|  [03]   | `Ei(x)` / `Ein(x)` / `Li(x)` / `En(int n,x)`                              | exponential integral Ei, entire Ein, logarithmic integral li, generalized Eₙ |
|  [04]   | `Si(x[,limit_zero])` / `Ci(x)` / `Shi(x)` / `Chi(x)` / `Ti(x)`            | sine / cosine / hyperbolic-sine / hyperbolic-cosine / arctangent integrals |
|  [05]   | `OwenT(h,a)`                                                              | Owen's T (bivariate-normal orthant probability)                      |

[FUNCTION_SCOPE]: Bessel, Airy, Scorer, Struve, and Anger-Weber
- rail: middle-precision FP

Integer-order and real-order (`ddouble nu`) Bessel families with an optional `scale` flag
(exponentially-scaled `I`/`K` to avoid over/underflow); the Airy/Scorer/Struve companions.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `BesselJ(nu,x)` / `BesselY(nu,x)` / `BesselI(nu,x[,scale])` / `BesselK(nu,x[,scale])` | real-order Bessel J/Y/I/K; `scale` toggles exponential scaling for I/K |
|  [02]   | `BesselJ(int n,x)` / `BesselY(int n,x)` / `BesselI(int n,x[,scale])` / `BesselK(int n,x[,scale])` | integer-order overloads                                |
|  [03]   | `AiryAi(x)` / `AiryBi(x)` / `ScorerGi(x)` / `ScorerHi(x)`                  | Airy Ai/Bi + Scorer Gi/Hi (inhomogeneous Airy)                       |
|  [04]   | `StruveH(int n,x)` / `StruveK(int n,x)` / `StruveL(int n,x)` / `StruveM(int n,x)` | Struve H/K and modified Struve L/M                              |
|  [05]   | `AngerJ(int n,x)` / `WeberE(int n,x)`                                      | Anger J and Weber E functions                                        |

[FUNCTION_SCOPE]: elliptic integrals and Jacobi elliptic functions
- rail: middle-precision FP

Both Legendre-form (`EllipticK`/`E`/`F`/`Pi`, complete and incomplete) and the symmetric
Carlson forms (`RC`/`RD`/`RF`/`RG`/`RJ`), plus the Jacobi amplitude/sn/cn/dn and their
inverses, and the theta functions — the parameter convention is the parameter `m` (= k²).

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `EllipticK(m)` / `EllipticE(m)` / `EllipticE(x,m)` / `EllipticF(x,m)` / `EllipticPi(n,m)` / `EllipticPi(n,x,m)` | complete & incomplete Legendre elliptic integrals K/E/F/Π |
|  [02]   | `CarlsonRC(x,y)` / `CarlsonRD(x,y,z)` / `CarlsonRF(x,y,z)` / `CarlsonRG(x,y,z)` / `CarlsonRJ(x,y,z,rho)` | symmetric Carlson elliptic integrals (numerically robust basis) |
|  [03]   | `JacobiSn(x,m)` / `JacobiCn(x,m)` / `JacobiDn(x,m)` / `JacobiAm(x,m)`      | Jacobi elliptic sn/cn/dn + amplitude                                 |
|  [04]   | `JacobiArcSn(x,m)` / `JacobiArcCn(x,m)` / `JacobiArcDn(x,m)`               | inverse Jacobi elliptic functions                                    |
|  [05]   | `EllipticTheta(int a,x,q)` / `EulerQ(q)` / `LogEulerQ(q)`                  | Jacobi theta θₐ, Euler function ∏(1−qⁿ) + its log                    |

[FUNCTION_SCOPE]: zeta family, Clausen, Kepler, and special transcendentals
- rail: middle-precision FP

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `RiemannZeta(x)` / `HurwitzZeta(x,a)` / `DirichletEta(x)` / `Polylog(int n,x)` | Riemann/Hurwitz zeta, Dirichlet eta, polylogarithm Liₙ            |
|  [02]   | `Clausen(x[,normalized])` / `LambertW(x)`                                  | Clausen Cl₂, Lambert W (principal branch)                            |
|  [03]   | `KeplerE(m,e[,centered])` / `Bernoulli(int n,x[,centered])`               | Kepler-equation eccentric-anomaly solve; Bernoulli polynomial Bₙ(x)  |
|  [04]   | `MathieuA(int n,q)` / `MathieuB(int n,q)` / `MathieuC(int n,q,x)` / `MathieuS(int n,q,x)` | Mathieu characteristic values a/b + even/odd Mathieu functions |
|  [05]   | `Cyclotomic(int n,x)`                                                      | n-th cyclotomic polynomial Φₙ(x) evaluated at `ddouble`              |

[FUNCTION_SCOPE]: orthogonal polynomials
- rail: middle-precision FP

The classical orthogonal-polynomial families evaluated at 106-bit — the building blocks for
spectral methods, Gauss quadrature node generation, and Zernike optical analysis.

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `LegendreP(int n,x)` / `LegendreP(int n,int m,x)`                          | Legendre P + associated Legendre Pₙᵐ                                 |
|  [02]   | `ChebyshevT(int n,x)` / `ChebyshevU(int n,x)`                              | Chebyshev first/second kind                                          |
|  [03]   | `HermiteH(int n,x)` / `LaguerreL(int n,x)` / `LaguerreL(int n,alpha,x)`    | Hermite (physicists') + (associated) Laguerre                        |
|  [04]   | `GegenbauerC(int n,alpha,x)` / `JacobiP(int n,alpha,beta,x)`               | Gegenbauer (ultraspherical) + Jacobi polynomials                     |
|  [05]   | `ZernikeR(int n,int m,x)`                                                  | Zernike radial polynomial Rₙᵐ                                        |

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: one `readonly struct ddouble` holding a private unevaluated hi/lo `double` pair (~106-bit significand, ~31 guaranteed decimal digits via `DecimalDigits`); the representation is opaque — no public field/property exposes hi/lo, so a consumer treats `ddouble` purely as a number value.
- construction: NO public constructor — build only via the implicit widening conversions (incl. `BigInteger`/`decimal`/the bit-tuple) or `Parse`/`TryParse`; never hand-assemble a hi/lo pair.
- precision boundary: precision is FIXED at two doubles. `ddouble` is a deterministic refinement of `double`, NOT an arbitrary-precision/exact type — it cannot stand in for the `BigRational` exact `Sign` oracle, and a result that needs more than ~106 bits must escalate to `Fraction`.
- error-free transforms: arithmetic uses FMA `TwoProduct` + Knuth/Dekker `TwoSum` identical to the in-house `Expansion` kernel, so the 106-bit middle stage and the `Expansion` exact branch share one rounding model and agree on the sign of a determinant up to the 106-bit resolution.
- generic math: `ddouble` IS the full `INumber<ddouble>`/`INumberBase<ddouble>` + every function-group interface; a kernel `where T : INumber<T>` / `IFloatingPointConstants<T>` / `I{Power,Root,Exponential,Logarithmic,Trigonometric,Hyperbolic}Functions<T>` binds `ddouble` with zero adapter, and `TryConvertFrom*`/`TryConvertTo*` move values to/from any other `INumberBase<TOther>`.

[LOCAL_ADMISSION]:
- `ddouble` is the MIDDLE precision tier (`.planning/Numerics/predicates#PRECISION_LADDER`): interior `double` (`NumericsPolicy`) -> `ddouble` 106-bit -> `Expansion` sign-exact -> `Fraction` exact-rational adjudication. The predicate recomputes the determinant in `ddouble` when the `double` filter's error bound brackets zero; the cheap 106-bit refinement resolves the near-degenerate sign for the vast majority of non-trivial cases, so the `Expansion`/`Fraction` branches fire only on the genuinely sub-106-bit-degenerate residue.
- the predicate reads `ddouble.Sign` (or an operator comparison) for the verdict and discards the value; a `ddouble` is never narrowed back to `double` mid-adjudication, because the narrowing readout loses the refinement that justified the tier.
- the `implicit operator ddouble(BigInteger)` and `explicit operator BigInteger`-free design means the bridge to the `BigRational` tier is one-directional at the value level: integer ordinates widen UP into `ddouble`, and only the still-indeterminate determinant promotes to the `Fraction` `BigInteger` path.
- non-predicate numeric work (curvature, geodesic, fitting residuals, quadrature weights) uses the special-function library directly at 106-bit where the `double` `MathNet.Numerics.SpecialFunctions`/`Integrate` result loses too many digits; the result narrows to `double` only at the reporting boundary.

[STACKING_LAW]:
- precision ladder: `ddouble` sits above the conservative `double` filter and below the `Expansion` exact branch and the `BigRational` (`api-bigrational`) exact-rational oracle. Because its `TwoProduct`/`TwoSum` transforms match the `Expansion` kernel, the middle stage and the exact branch never disagree on a sign they can both resolve — the ladder is monotone, each tier strictly refining the one below.
- vs MathNet (`api-mathnet-numerics`): `MathNet.Numerics.SpecialFunctions`/`Integrate`/`RootFinding` own the BROAD `double`-precision numeric lane (distributions, quadrature, root-finding, interpolation); `ddouble` owns the SAME special functions (gamma/beta/erf/Bessel/elliptic/zeta) at 106-bit precision for the accuracy-critical path. A `double` MathNet kernel lifts to double-double by parameterizing on `INumber<T>` and substituting `ddouble`; the division is precision tier, not capability — never re-host a MathNet distribution on `ddouble` by hand when only the special-function evaluation needs the extra digits.
- vs Tensors (`api-tensors`): `System.Numerics.Tensors` generic `TensorPrimitives<T>` operate on any `INumber<T>`/`IFloatingPoint<T>`, so a span of `ddouble` flows through the generic tensor primitives directly — bulk 106-bit reductions/maps need no `ddouble`-specific SIMD path, and `DoubleDoubleEnumerableExpand.Sum`/`Average` give the accumulation-accurate fold for the non-SIMD case.
- serialization: `DDoubleJsonConverter` (register on `JsonSerializerOptions.Converters`) and `DoubleDoubleIOExpand.Write`/`ReadDDouble` round-trip the exact hi/lo bits through a `System.Text.Json` document or a `BinaryWriter` stream, so a 106-bit value crosses a wire/persistence boundary losslessly rather than degrading to `double`.
- proof: in the predicate law-matrix `ddouble` is one of the differentially-fuzzed tiers — `CsCheck` random determinants compare `double`/`Expansion`/`DoubleDouble` against the `Fraction.Sign` ground truth, so the 106-bit stage's agreement with the exact oracle on the resolvable set is a verified invariant, not an assumption.

[RAIL_LAW]:
- Package: `TYoshimura.DoubleDouble`
- Owns: the 106-bit double-double number `ddouble` — full `INumber<ddouble>`/`INumberBase<ddouble>` generic math, the complete operator/comparison/classification/parse/format contract, the `TryConvertFrom*`/`TryConvertTo*` cross-`T` bridge, the named-constant table, and the double-double-precision special-function library (gamma/beta, error/Fresnel, Bessel/Airy/Struve, elliptic + Jacobi, integral functions, zeta/polylog/polygamma, orthogonal polynomials, Owen's T, Lambert W, Mathieu); the `DoubleDoubleEnumerableExpand`/`DoubleDoubleIOExpand`/`DDoubleJsonConverter` siblings for LINQ aggregation, binary I/O, and JSON.
- Accept: the predicate middle tier (recompute the determinant in `ddouble`, read `Sign`, escalate only the indeterminate residue to `Expansion`/`Fraction`); a generic numeric kernel `where T : INumber<T>` (or a function-group constraint) bound to `ddouble`; accuracy-critical special-function/quadrature evaluation that loses digits at `double`; lossless 106-bit (de)serialization via the converter / IO extensions.
- Reject: treating `ddouble` as an exact/arbitrary-precision oracle (it is fixed two-double precision — use `Fraction` for the exact `Sign`); hand-assembling a hi/lo pair or reading a non-existent `Hi`/`Lo` property (no public constructor, opaque representation); narrowing `ddouble` to `double` mid-adjudication; re-hosting a whole MathNet distribution/solver on `ddouble` by hand when only the special-function evaluation needs the precision; documenting an `internal` `Consts`-kernel helper (`Coef`/`Value`/`Kernel`/`SeriesValue`/`ErfcGt*`/`PadeApprox` and the `Asymptotic`/`Iter`/`Q0` family) as public API — they are implementation detail, never the consumed surface.
