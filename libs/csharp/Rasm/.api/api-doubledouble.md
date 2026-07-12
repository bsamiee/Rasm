# [RASM_API_DOUBLEDOUBLE]

`TYoshimura.DoubleDouble` supplies `ddouble`, a `readonly struct` carrying an unevaluated two-`double` hi/lo pair with a 106-bit significand and approximately 31 decimal digits. `ddouble` implements the complete `System.Numerics` number, function-group, operator, parse, and format contracts, so generic numeric kernels bind it directly and parameterized `MathNet.Numerics` or `System.Numerics.Tensors` code lifts from `double` to the middle precision tier. Its special-function library evaluates the gamma, beta, error, Fresnel, Bessel, Airy, Struve, elliptic, Jacobi, integral, zeta, polylogarithm, polygamma, orthogonal-polynomial, Owen's T, Lambert W, and Mathieu families at double-double precision. FMA `TwoProduct` and Knuth/Dekker `TwoSum` match the in-house `Expansion` rounding model. Fixed two-double precision makes `ddouble` a deterministic refinement of `double`, not an arbitrary-precision or exact type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TYoshimura.DoubleDouble`
- package: `TYoshimura.DoubleDouble`
- license: MIT (T.Yoshimura; `github.com/tk-yoshimura/DoubleDouble`)
- assembly: `DoubleDouble.dll`
- namespace: `DoubleDouble` (the `ddouble` value + extension/converter siblings); `DoubleDouble.Utils` (`ResourceUnpack` coefficient-table deserialization — not consumer surface)
- target: single-target `lib/net10.0` only — no multi-target fallback ambiguity, the `net10.0` consumer binds the one shipped asset
- asset: pure-managed runtime library, AnyCPU, no native runtime and ZERO package dependencies (`System.Numerics.BigInteger` is the only BCL touch, for the `BigInteger` conversion); a source-generated `System.Text.RegularExpressions.Generated` parser backs `Parse`
- abi: full `System.Numerics` generic-math — a kernel constrained `where T: INumber<T>` / `IFloatingPointConstants<T>` / `I{Power,Root,Exponential,Logarithmic,Trigonometric,Hyperbolic}Functions<T>` binds `ddouble` with no adapter; the lowercase type name `ddouble` is the public spelling
- rail: middle-precision FP (106-bit double-double)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the value + its three first-class siblings (namespace `DoubleDouble`)
- rail: middle-precision FP

`ddouble` owns the numeric surface, and its three public siblings extend it into LINQ aggregation, binary I/O, and `System.Text.Json`. The package keeps `UInt128`, `FloatSplitter`, `IntegerSplitter`, `SeriesUtil`, and the per-function `Consts` kernels internal.

| [INDEX] | [SYMBOL]                       | [ROLE]                 |
| :-----: | :----------------------------- | :--------------------- |
|  [01]   | `ddouble`                      | opaque numeric value   |
|  [02]   | `DoubleDoubleEnumerableExpand` | aggregation extensions |
|  [03]   | `DoubleDoubleIOExpand`         | binary I/O extensions  |
|  [04]   | `DDoubleJsonConverter`         | JSON converter         |

[PUBLIC_TYPE_DETAIL]:
- `ddouble` is a `readonly struct` with the full generic-math, operator, and special-function surface; no public field or property exposes its hi/lo representation.
- `DoubleDoubleEnumerableExpand` supplies `Sum`, `Average`, `Min`, `Max`, `MinIndex`, and `MaxIndex` over `IEnumerable<ddouble>` and `IReadOnlyList<ddouble>` with 106-bit accumulation.
- `DoubleDoubleIOExpand` supplies `BinaryWriter.Write(ddouble)` and `BinaryReader.ReadDDouble()` with exact hi/lo round-tripping.
- `DDoubleJsonConverter` is a `JsonConverter<ddouble>` registered on `JsonSerializerOptions.Converters` for lossless serialization.

## [03]-[NUMERIC_CONTRACT]

[CONTRACT_SCOPE]: construction 'no public constructor — operators + `Parse` only'
- rail: middle-precision FP

Every `ddouble` constructor is internal. Implicit operators build the value from primitive integers, `BigInteger`, `double`, `decimal`, `string`, or its raw bit tuple; explicit operators narrow only at a boundary. `Parse` and `TryParse` cover `string` and span inputs with `NumberStyles` and `IFormatProvider` overloads.

| [INDEX] | [SURFACE]                                                                 | [KIND]     |
| :-----: | :------------------------------------------------------------------------ | :--------- |
|  [01]   | `implicit operator ddouble(double)`                                       | widening   |
|  [02]   | `implicit operator ddouble(int / uint / long / ulong)`                    | widening   |
|  [03]   | `implicit operator ddouble(BigInteger)`                                   | widening   |
|  [04]   | `implicit operator ddouble(decimal)`                                      | conversion |
|  [05]   | `implicit operator ddouble(string)`                                       | parsing    |
|  [06]   | `implicit operator ddouble((int sign, int exponent, ulong hi, ulong lo))` | bit tuple  |
|  [07]   | `explicit operator double / float (ddouble)`                              | narrowing  |
|  [08]   | `explicit operator int / uint / long / ulong / decimal (ddouble)`         | narrowing  |
|  [09]   | `ddouble.Parse(string / ReadOnlySpan<char>)`                              | factory    |
|  [10]   | `ddouble.Parse(..., NumberStyles, IFormatProvider)`                       | factory    |
|  [11]   | `ddouble.TryParse(..., out ddouble result)`                               | no-throw   |

[CONSTRUCTION_DETAIL]: Primitive integers, `BigInteger`, and `double` widen losslessly; `decimal` is exact within the 106-bit significand. String conversion forwards to the full-precision decimal-literal parser, and the tuple conversion round-trips the `(sign, exponent, hi, lo)` decomposition.

[CONTRACT_SCOPE]: identity, limit, and named mathematical constants
- rail: middle-precision FP

The identity and limit anchors are static properties, mathematical constants are `static readonly ddouble` fields computed once at type initialization, and coefficient tables are `ReadOnlyCollection<ddouble>` properties.

| [INDEX] | [SURFACE]                                                                 | [KIND]           |
| :-----: | :------------------------------------------------------------------------ | :--------------- |
|  [01]   | `ddouble.Zero / One / MinusOne / PlusZero / MinusZero`                    | identity         |
|  [02]   | `ddouble.AdditiveIdentity / MultiplicativeIdentity / NegativeOne`         | generic identity |
|  [03]   | `ddouble.NaN / PositiveInfinity / NegativeInfinity`                       | non-finite       |
|  [04]   | `ddouble.Epsilon / MaxValue / MinValue`                                   | range            |
|  [05]   | `ddouble.NormalMinExponent`                                               | `const int`      |
|  [06]   | `ddouble.DecimalDigits`                                                   | `const int`      |
|  [07]   | `ddouble.Pi / E / RcpPi / RcpE / Ln2 / Lb10 / Lg2 / LbE / Sqrt2 / Point5` | constant         |
|  [08]   | `ddouble.EulerGamma / CatalanG / GoldenRatio / GlaisherA`                 | constant         |
|  [09]   | `ddouble.Zeta3..Zeta9 / LemniscatePi / ErdosBorwein / LambertOmega`       | constant         |
|  [10]   | `ddouble.BernoulliSequence / Factorial / TaylorSequence / StieltjesGamma` | coefficients     |

[CONSTANT_DETAIL]: `NormalMinExponent` is `-968`, `DecimalDigits` is `31`, and the named constants and coefficient tables retain full 106-bit precision.

[CONTRACT_SCOPE]: arithmetic, comparison, and IEEE classification
- rail: middle-precision FP

Arithmetic covers error-free-transform operators with mixed primitive overloads on both sides, unary sign, increment, decrement, and checked increment or decrement. Comparison covers total ordering, equality, `CompareTo`, `Equals`, and `GetHashCode`; classification covers the full `INumberBase` predicate set.

| [INDEX] | [SURFACE]                                                                       | [ROLE]         |
| :-----: | :------------------------------------------------------------------------------ | :------------- |
|  [01]   | `operator + - * / %`                                                            | arithmetic     |
|  [02]   | mixed `ddouble` and `double / int / uint / long / ulong` operators              | arithmetic     |
|  [03]   | unary `operator + - / ++ / -- / checked ++ / checked --`                        | unary          |
|  [04]   | `operator < <= > >= == !=`                                                      | comparison     |
|  [05]   | `ddouble.Sign(value)`                                                           | sign           |
|  [06]   | `ddouble.Abs / CopySign`                                                        | magnitude      |
|  [07]   | `ddouble.MaxMagnitude / MinMagnitude / MaxMagnitudeNumber / MinMagnitudeNumber` | magnitude      |
|  [08]   | `ddouble.Min / Max / Clamp`                                                     | bounds         |
|  [09]   | `ddouble.IsNaN / IsFinite / IsInfinity`                                         | classification |
|  [10]   | `ddouble.IsPositiveInfinity / IsNegativeInfinity / IsNormal / IsSubnormal`      | classification |
|  [11]   | `ddouble.IsZero / IsPlusZero / IsMinusZero`                                     | classification |
|  [12]   | `ddouble.IsInteger / IsEvenInteger / IsOddInteger`                              | classification |
|  [13]   | `ddouble.IsPositive / IsNegative / IsCanonical / IsRealNumber`                  | classification |
|  [14]   | `ddouble.IsImaginaryNumber / IsComplexNumber`                                   | classification |
|  [15]   | `ddouble.CompareTo / Equals / GetHashCode`                                      | equality       |
|  [16]   | `ddouble.Floor / Ceiling / Round / Truncate`                                    | rounding       |
|  [17]   | `ddouble.Ldexp / ScaleB / Frexp / ILogB`                                        | exponent       |
|  [18]   | `ddouble.BitIncrement / BitDecrement / TruncateMantissa`                        | bit control    |
|  [19]   | `ddouble.AdjustScale`                                                           | tuple scaling  |

[ARITHMETIC_DETAIL]: Mixed arithmetic avoids widening allocation. `Sign` returns `-1`, `0`, or `+1`; `Frexp` returns `(int exp, ddouble value)`, and `ScaleB` accepts `int` or `long`. `AdjustScale` aligns one-, two-, three-, or four-component tuples to one binary exponent and returns the exponent with the scaled values. Finite real values make `IsRealNumber` and `IsCanonical` true and `IsImaginaryNumber` and `IsComplexNumber` false.

[CONTRACT_SCOPE]: `INumberBase` generic-math conversion (cross-`T` interop)
- rail: middle-precision FP

The no-throw conversion triads bridge `ddouble` and any `INumberBase<TOther>` under checked, saturating, or truncating rounding. Formatting implements `IFormattable`, `ISpanFormattable`, and `IUtf8SpanFormattable`.

| [INDEX] | [SURFACE]                                                      | [DIRECTION]           |
| :-----: | :------------------------------------------------------------- | :-------------------- |
|  [01]   | `TryConvertFromChecked<TOther>`                                | `TOther` to `ddouble` |
|  [02]   | `TryConvertFromSaturating<TOther>`                             | `TOther` to `ddouble` |
|  [03]   | `TryConvertFromTruncating<TOther>`                             | `TOther` to `ddouble` |
|  [04]   | `TryConvertToChecked<TOther>`                                  | `ddouble` to `TOther` |
|  [05]   | `TryConvertToSaturating<TOther>`                               | `ddouble` to `TOther` |
|  [06]   | `TryConvertToTruncating<TOther>`                               | `ddouble` to `TOther` |
|  [07]   | `TryFormat(Span<char>, out int, ReadOnlySpan<char>, provider)` | span render           |
|  [08]   | `ToString(format, provider)`                                   | string render         |

## [04]-[ELEMENTARY_AND_SPECIAL_FUNCTIONS]

[FUNCTION_SCOPE]: power, root, exp, log (the function-group interfaces)
- rail: middle-precision FP

`ddouble` satisfies `IPowerFunctions`, `IRootFunctions`, `IExponentialFunctions`, and `ILogarithmicFunctions`. The `*M1`, `*P1`, `Pow1p`, and `Pow2m1` variants preserve accuracy near one or zero, and `Rcp` supplies the double-double reciprocal.

| [INDEX] | [SURFACE]                                      | [FAMILY]     |
| :-----: | :--------------------------------------------- | :----------- |
|  [01]   | `Pow(x,y) / Pow(x,long n) / Pow1p(x,y)`        | power        |
|  [02]   | `Pow2(x) / Pow2m1(x) / Pow10(x)`               | power        |
|  [03]   | `Sqrt(x) / Cbrt(x) / RootN(x,int n)`           | root         |
|  [04]   | `Square(x) / Cube(x) / Rcp(x)`                 | algebraic    |
|  [05]   | `Hypot(x,y) / Hypot(x,y,z)`                    | norm         |
|  [06]   | `Agm(a,b) / GeometricMean(a,b[,c])`            | mean         |
|  [07]   | `Exp(x) / Exp2(x) / Exp10(x)`                  | exponential  |
|  [08]   | `Expm1(x) / Exp2M1(x) / Exp10M1(x)`            | exponential  |
|  [09]   | `Log(x) / Log(x,b) / Log2(x) / Log10(x)`       | logarithm    |
|  [10]   | `Log1p(x) / LogP1(x) / Log2P1(x) / Log10P1(x)` | logarithm    |
|  [11]   | `Expit(x) / Logit(x) / Bump(x)`                | distribution |

[FUNCTION_SCOPE]: trigonometric, hyperbolic, and their inverses
- rail: middle-precision FP

`ddouble` satisfies `ITrigonometricFunctions` and `IHyperbolicFunctions`. The `*Pi` variants accept half-turn arguments and remain exact at rational angle multiples; `SinCos` and `SinCosPi` return both outputs after one argument reduction. `Sinc`, `Sinhc`, and `Jinc` supply the cardinal forms used by signal and optics kernels.

| [INDEX] | [SURFACE]                                      | [FAMILY]   |
| :-----: | :--------------------------------------------- | :--------- |
|  [01]   | `Sin / Cos / Tan / Asin / Acos / Atan / Atan2` | circular   |
|  [02]   | `SinCos(x)`                                    | fused      |
|  [03]   | `SinCosPi(x)`                                  | fused      |
|  [04]   | `SinPi / CosPi / TanPi`                        | half-turn  |
|  [05]   | `AsinPi / AcosPi / AtanPi / Atan2Pi`           | half-turn  |
|  [06]   | `Sinh / Cosh / Tanh / Asinh / Acosh / Atanh`   | hyperbolic |
|  [07]   | `Sinc(x[,normalized]) / Sinhc(x) / Jinc(x)`    | cardinal   |

[FUSED_TRIG_DETAIL]: `SinCos(x)` returns `(ddouble Sin, ddouble Cos)`, and `SinCosPi(x)` returns `(ddouble SinPi, ddouble CosPi)`.

[FUNCTION_SCOPE]: gamma, beta, digamma, and incomplete forms
- rail: middle-precision FP

The gamma and beta tower includes log-space, reciprocal, inverse, regularized, and incomplete forms for distribution-tail and Bayesian work beyond `double` precision.

| [INDEX] | [SURFACE]                                                           | [FAMILY] |
| :-----: | :------------------------------------------------------------------ | :------- |
|  [01]   | `Gamma(x) / LogGamma(x) / RcpGamma(x) / InverseGamma(x)`            | gamma    |
|  [02]   | `Digamma(x) / InverseDigamma(x) / Polygamma(int n,x)`               | gamma    |
|  [03]   | `Beta(a,b) / LogBeta(a,b)`                                          | beta     |
|  [04]   | `IncompleteBeta(x,a,b) / IncompleteBetaRegularized(x,a,b)`          | beta     |
|  [05]   | `InverseIncompleteBeta(x,a,b)`                                      | beta     |
|  [06]   | `LowerIncompleteGamma(nu,x) / UpperIncompleteGamma(nu,x)`           | gamma    |
|  [07]   | `LowerIncompleteGammaRegularized / UpperIncompleteGammaRegularized` | gamma    |
|  [08]   | `InverseLowerIncompleteGamma / InverseUpperIncompleteGamma`         | gamma    |
|  [09]   | `Binomial(int n,int k) / HarmonicNumber(int n)`                     | sequence |
|  [10]   | `BarnesG(x) / LogBarnesG(x)`                                        | Barnes G |

[FUNCTION_SCOPE]: error, Fresnel, Dawson, and the integral functions
- rail: middle-precision FP

The integral surface includes error and inverse-error functions, Fresnel and Dawson forms, exponential and logarithmic integrals, trigonometric and hyperbolic integrals, and Owen's bivariate-normal orthant function.

| [INDEX] | [SURFACE]                                               | [FAMILY]      |
| :-----: | :------------------------------------------------------ | :------------ |
|  [01]   | `Erf(x) / Erfc(x) / Erfcx(x) / Erfi(x)`                 | error         |
|  [02]   | `InverseErf(x) / InverseErfc(x)`                        | inverse error |
|  [03]   | `FresnelC(x) / FresnelS(x) / FresnelF(x) / FresnelG(x)` | Fresnel       |
|  [04]   | `DawsonF(x)`                                            | Dawson        |
|  [05]   | `Ei(x) / Ein(x) / Li(x) / En(int n,x)`                  | exponential   |
|  [06]   | `Si(x[,limit_zero]) / Ci(x)`                            | trigonometric |
|  [07]   | `Shi(x) / Chi(x) / Ti(x)`                               | hyperbolic    |
|  [08]   | `OwenT(h,a)`                                            | probability   |

[FUNCTION_SCOPE]: Bessel, Airy, Scorer, Struve, and Anger-Weber
- rail: middle-precision FP

The Bessel families accept integer or real `ddouble` order. `BesselI` and `BesselK` accept `scale` for exponential scaling that avoids overflow or underflow; Airy, Scorer, Struve, Anger, and Weber functions complete the family.

| [INDEX] | [SURFACE]                                             | [FAMILY]    |
| :-----: | :---------------------------------------------------- | :---------- |
|  [01]   | `BesselJ(nu,x) / BesselY(nu,x)`                       | Bessel      |
|  [02]   | `BesselI(nu,x[,scale]) / BesselK(nu,x[,scale])`       | Bessel      |
|  [03]   | `BesselJ(int n,x) / BesselY(int n,x)`                 | Bessel      |
|  [04]   | `BesselI(int n,x[,scale]) / BesselK(int n,x[,scale])` | Bessel      |
|  [05]   | `AiryAi(x) / AiryBi(x)`                               | Airy        |
|  [06]   | `ScorerGi(x) / ScorerHi(x)`                           | Scorer      |
|  [07]   | `StruveH(int n,x) / StruveK(int n,x)`                 | Struve      |
|  [08]   | `StruveL(int n,x) / StruveM(int n,x)`                 | Struve      |
|  [09]   | `AngerJ(int n,x) / WeberE(int n,x)`                   | Anger-Weber |

[FUNCTION_SCOPE]: elliptic integrals and Jacobi elliptic functions
- rail: middle-precision FP

The elliptic surface includes complete and incomplete Legendre forms, symmetric Carlson forms, Jacobi amplitude and functions with inverses, theta functions, and Euler's product with its logarithm. Every elliptic function uses parameter `m = k²`.

| [INDEX] | [SURFACE]                                                       | [FAMILY] |
| :-----: | :-------------------------------------------------------------- | :------- |
|  [01]   | `EllipticK(m) / EllipticE(m)`                                   | Legendre |
|  [02]   | `EllipticE(x,m) / EllipticF(x,m)`                               | Legendre |
|  [03]   | `EllipticPi(n,m) / EllipticPi(n,x,m)`                           | Legendre |
|  [04]   | `CarlsonRC(x,y) / CarlsonRD(x,y,z) / CarlsonRF(x,y,z)`          | Carlson  |
|  [05]   | `CarlsonRG(x,y,z) / CarlsonRJ(x,y,z,rho)`                       | Carlson  |
|  [06]   | `JacobiSn(x,m) / JacobiCn(x,m) / JacobiDn(x,m) / JacobiAm(x,m)` | Jacobi   |
|  [07]   | `JacobiArcSn(x,m) / JacobiArcCn(x,m) / JacobiArcDn(x,m)`        | inverse  |
|  [08]   | `EllipticTheta(int a,x,q)`                                      | theta    |
|  [09]   | `EulerQ(q) / LogEulerQ(q)`                                      | Euler    |

[FUNCTION_SCOPE]: zeta family, Clausen, Kepler, and special transcendentals
- rail: middle-precision FP

The transcendental surface includes Riemann and Hurwitz zeta, Dirichlet eta, polylogarithm, normalized Clausen, principal Lambert W, Kepler's equation, Bernoulli and cyclotomic polynomials, and Mathieu characteristic values and functions.

| [INDEX] | [SURFACE]                                   | [FAMILY]       |
| :-----: | :------------------------------------------ | :------------- |
|  [01]   | `RiemannZeta(x) / HurwitzZeta(x,a)`         | zeta           |
|  [02]   | `DirichletEta(x) / Polylog(int n,x)`        | zeta           |
|  [03]   | `Clausen(x[,normalized]) / LambertW(x)`     | transcendental |
|  [04]   | `KeplerE(m,e[,centered])`                   | Kepler         |
|  [05]   | `Bernoulli(int n,x[,centered])`             | polynomial     |
|  [06]   | `MathieuA(int n,q) / MathieuB(int n,q)`     | Mathieu        |
|  [07]   | `MathieuC(int n,q,x) / MathieuS(int n,q,x)` | Mathieu        |
|  [08]   | `Cyclotomic(int n,x)`                       | polynomial     |

[FUNCTION_SCOPE]: orthogonal polynomials
- rail: middle-precision FP

The classical orthogonal-polynomial families evaluate at 106-bit precision for spectral methods, Gauss quadrature node generation, and Zernike optical analysis.

| [INDEX] | [SURFACE]                                       | [FAMILY]   |
| :-----: | :---------------------------------------------- | :--------- |
|  [01]   | `LegendreP(int n,x) / LegendreP(int n,int m,x)` | Legendre   |
|  [02]   | `ChebyshevT(int n,x) / ChebyshevU(int n,x)`     | Chebyshev  |
|  [03]   | `HermiteH(int n,x)`                             | Hermite    |
|  [04]   | `LaguerreL(int n,x) / LaguerreL(int n,alpha,x)` | Laguerre   |
|  [05]   | `GegenbauerC(int n,alpha,x)`                    | Gegenbauer |
|  [06]   | `JacobiP(int n,alpha,beta,x)`                   | Jacobi     |
|  [07]   | `ZernikeR(int n,int m,x)`                       | Zernike    |

## [05]-[IMPLEMENTATION_LAW]

[VALUE_PROFILE]:
- representation: one `readonly struct ddouble` holding a private unevaluated hi/lo `double` pair (~106-bit significand, ~31 guaranteed decimal digits via `DecimalDigits`); the representation is opaque — no public field/property exposes hi/lo, so a consumer treats `ddouble` purely as a number value.
- construction: NO public constructor — build only via the implicit widening conversions (incl. `BigInteger`/`decimal`/the bit-tuple) or `Parse`/`TryParse`; never hand-assemble a hi/lo pair.
- precision boundary: precision is FIXED at two doubles. `ddouble` is a deterministic refinement of `double`, NOT an arbitrary-precision/exact type — it cannot stand in for the `BigRational` exact `Sign` oracle, and a result that needs more than ~106 bits must escalate to `Fraction`.
- error-free transforms: arithmetic uses FMA `TwoProduct` + Knuth/Dekker `TwoSum` identical to the in-house `Expansion` kernel, so the 106-bit middle stage and the `Expansion` exact branch share one rounding model and agree on the sign of a determinant up to the 106-bit resolution.
- generic math: `ddouble` IS the full `INumber<ddouble>`/`INumberBase<ddouble>` + every function-group interface; a kernel `where T: INumber<T>` / `IFloatingPointConstants<T>` / `I{Power,Root,Exponential,Logarithmic,Trigonometric,Hyperbolic}Functions<T>` binds `ddouble` with zero adapter, and `TryConvertFrom*`/`TryConvertTo*` move values to/from any other `INumberBase<TOther>`.

[LOCAL_ADMISSION]:
- `ddouble` is the middle precision tier between the interior `double` filter and the sign-exact `Expansion` and `Fraction` adjudicators. The predicate recomputes the determinant in `ddouble` when the `double` error bound brackets zero, and only sub-106-bit-degenerate residue advances to exact adjudication.
- the predicate reads `ddouble.Sign` (or an operator comparison) for the verdict and discards the value; a `ddouble` is never narrowed back to `double` mid-adjudication, because the narrowing readout loses the refinement that justified the tier.
- the `implicit operator ddouble(BigInteger)` and `explicit operator BigInteger`-free design means the bridge to the `BigRational` tier is one-directional at the value level: integer ordinates widen UP into `ddouble`, and only the still-indeterminate determinant promotes to the `Fraction` `BigInteger` path.
- non-predicate numeric work (curvature, geodesic, fitting residuals, quadrature weights) uses the special-function library directly at 106-bit where the `double` `MathNet.Numerics.SpecialFunctions`/`Integrate` result loses too many digits; the result narrows to `double` only at the reporting boundary.

[STACKING_LAW]:
- precision ladder: `ddouble` sits above the conservative `double` filter and below the `Expansion` exact branch and the `BigRational` (`api-bigrational`) exact-rational oracle. Because its `TwoProduct`/`TwoSum` transforms match the `Expansion` kernel, the middle stage and the exact branch never disagree on a sign they can both resolve — the ladder is monotone, each tier strictly refining the one below.
- MathNet lane: `MathNet.Numerics.SpecialFunctions`, `Integrate`, and `RootFinding` own `double`-precision distributions, quadrature, root-finding, and interpolation.
- Double-double lane: `ddouble` evaluates the corresponding gamma, beta, error, Bessel, elliptic, and zeta functions at 106-bit precision.
- Generic lift: a MathNet kernel parameterized on `INumber<T>` lifts by substituting `ddouble`; only the accuracy-critical special-function evaluation changes precision.
- vs Tensors (`api-tensors`): `System.Numerics.Tensors` generic `TensorPrimitives<T>` operate on any `INumber<T>`/`IFloatingPoint<T>`, so a span of `ddouble` flows through the generic tensor primitives directly — bulk 106-bit reductions/maps need no `ddouble`-specific SIMD path, and `DoubleDoubleEnumerableExpand.Sum`/`Average` give the accumulation-accurate fold for the non-SIMD case.
- serialization: `DDoubleJsonConverter` (register on `JsonSerializerOptions.Converters`) and `DoubleDoubleIOExpand.Write`/`ReadDDouble` round-trip the exact hi/lo bits through a `System.Text.Json` document or a `BinaryWriter` stream, so a 106-bit value crosses a wire/persistence boundary losslessly rather than degrading to `double`.
- proof: in the predicate law-matrix `ddouble` is one of the differentially-fuzzed tiers — `CsCheck` random determinants compare `double`/`Expansion`/`DoubleDouble` against the `Fraction.Sign` ground truth, so the 106-bit stage's agreement with the exact oracle on the resolvable set is a verified invariant, not an assumption.

[RAIL_LAW]:
- Package: `TYoshimura.DoubleDouble`
- Owns: the 106-bit `ddouble` number, its complete generic-math and special-function surface, and its aggregation, binary I/O, and JSON siblings.
- Accept: the predicate middle tier (recompute the determinant in `ddouble`, read `Sign`, escalate only the indeterminate residue to `Expansion`/`Fraction`); a generic numeric kernel `where T: INumber<T>` (or a function-group constraint) bound to `ddouble`; accuracy-critical special-function/quadrature evaluation that loses digits at `double`; lossless 106-bit (de)serialization via the converter / IO extensions.
- Reject: treating `ddouble` as an exact/arbitrary-precision oracle (it is fixed two-double precision — use `Fraction` for the exact `Sign`); hand-assembling a hi/lo pair or reading a non-existent `Hi`/`Lo` property (no public constructor, opaque representation); narrowing `ddouble` to `double` mid-adjudication; re-hosting a whole MathNet distribution/solver on `ddouble` by hand when only the special-function evaluation needs the precision; documenting an `internal` `Consts`-kernel helper (`Coef`/`Value`/`Kernel`/`SeriesValue`/`ErfcGt*`/`PadeApprox` and the `Asymptotic`/`Iter`/`Q0` family) as public API — they are implementation detail, never the consumed surface.
