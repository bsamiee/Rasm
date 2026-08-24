# [RASM_API_DOUBLEDOUBLE]

`TYoshimura.DoubleDouble` mints `ddouble`, a `readonly struct` holding an unevaluated 106-bit hi/lo `double` pair — the deterministic middle tier refining `double`, never arbitrary-precision or exact. `ddouble` implements the complete `System.Numerics` number, function-group, operator, parse, and format contracts, so a generic kernel binds it and its special-function library evaluates every classical family at double-double precision.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TYoshimura.DoubleDouble`
- package: `TYoshimura.DoubleDouble` (MIT)
- assembly: `DoubleDouble.dll`
- namespace: `DoubleDouble` holds `ddouble` and its extension/converter siblings
- target: single `lib/net10.0` asset
- asset: pure-managed AnyCPU, zero package dependencies, `System.Numerics.BigInteger` the sole BCL touch
- abi: full `System.Numerics` generic-math — a kernel constrained on `INumber<T>`, `IFloatingPointConstants<T>`, or any function-group interface binds `ddouble` with no adapter; `ddouble` (lowercase) is the public spelling
- rail: middle-precision FP (106-bit double-double)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `ddouble` value and its three first-class siblings in namespace `DoubleDouble`

`ddouble` owns the numeric surface; splitters, series utilities, and the per-function `Consts` kernels stay internal.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------------------------- | :------------ | :------------------------------- |
|  [01]   | `ddouble`                      | struct        | the opaque 106-bit numeric value |
|  [02]   | `DoubleDoubleEnumerableExpand` | static class  | 106-bit LINQ aggregation         |
|  [03]   | `DoubleDoubleIOExpand`         | static class  | exact-bit binary I/O extensions  |
|  [04]   | `DDoubleJsonConverter`         | class         | `System.Text.Json` converter     |

[PUBLIC_TYPE_DETAIL]:
- `DoubleDoubleEnumerableExpand` folds `Sum`/`Average`/`Min`/`Max` over `IEnumerable<ddouble>` and `MinIndex`/`MaxIndex` over `IReadOnlyList<ddouble>` at 106-bit.
- `DoubleDoubleIOExpand` supplies `Write(BinaryWriter, ddouble)` and `ReadDDouble(BinaryReader) -> ddouble` with exact hi/lo round-tripping.
- `DDoubleJsonConverter : JsonConverter<ddouble>` overrides `Read` and `Write`, registering on `JsonSerializerOptions.Converters` for lossless serialization.

## [03]-[NUMERIC_CONTRACT]

[CONTRACT_SCOPE]: construction — no public constructor, operators and `Parse` only

Every constructor is internal — implicit operators are the sole construction. Integers, `BigInteger`, and `double` widen losslessly, `decimal` is exact within the 106-bit significand, and the raw `(sign, exponent, hi, lo)` bit tuple builds directly.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY] |
| :-----: | :------------------------------------------------------------------------ | :----------- |
|  [01]   | `implicit operator ddouble(double)`                                       | widening     |
|  [02]   | `implicit operator ddouble(int / uint / long / ulong)`                    | widening     |
|  [03]   | `implicit operator ddouble(BigInteger)`                                   | widening     |
|  [04]   | `implicit operator ddouble(decimal)`                                      | conversion   |
|  [05]   | `implicit operator ddouble(string)`                                       | parsing      |
|  [06]   | `implicit operator ddouble((int sign, int exponent, ulong hi, ulong lo))` | bit tuple    |
|  [07]   | `explicit operator double / float (ddouble)`                              | narrowing    |
|  [08]   | `explicit operator int / uint / long / ulong / decimal (ddouble)`         | narrowing    |
|  [09]   | `ddouble.Parse(string / ReadOnlySpan<char>)`                              | factory      |
|  [10]   | `ddouble.Parse(..., NumberStyles, IFormatProvider)`                       | factory      |
|  [11]   | `ddouble.TryParse(..., out ddouble result)`                               | no-throw     |

[CONTRACT_SCOPE]: identity, limit, and named mathematical constants

Identity and limit anchors are static properties; named constants are `static readonly ddouble` fields; coefficient tables are `ReadOnlyCollection<ddouble>` properties. `NormalMinExponent` is `-968`, `DecimalDigits` `31`.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]     |
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

[CONTRACT_SCOPE]: arithmetic, comparison, and IEEE classification

Arithmetic operators carry mixed-primitive overloads on both sides, unary sign, increment/decrement, and checked variants. `Sign` returns `-1`/`0`/`+1`, `Frexp` returns `(int exp, ddouble value)`, and `AdjustScale` aligns one-to-four-component tuples to one binary exponent.

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]   |
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

[CONTRACT_SCOPE]: `INumberBase` generic-math conversion and formatting

No-throw conversion triads bridge `ddouble` and any `INumberBase<TOther>` under checked, saturating, or truncating rounding; formatting implements `IFormattable`, `ISpanFormattable`, and `IUtf8SpanFormattable`.

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]          |
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

[FUNCTION_SCOPE]: power, root, exponential, logarithm

`ddouble` satisfies `IPowerFunctions`, `IRootFunctions`, `IExponentialFunctions`, and `ILogarithmicFunctions`; the `*M1`/`*P1`/`Pow1p`/`Pow2m1` variants hold accuracy near one or zero.

| [INDEX] | [SURFACE]                                      | [CAPABILITY] |
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

[FUNCTION_SCOPE]: trigonometric, hyperbolic, and inverses

`ddouble` satisfies `ITrigonometricFunctions` and `IHyperbolicFunctions`. `*Pi` variants take half-turn arguments and stay exact at rational angle multiples; `SinCos(x) -> (Sin, Cos)` and `SinCosPi(x) -> (SinPi, CosPi)` return both outputs after one argument reduction.

| [INDEX] | [SURFACE]                                      | [CAPABILITY] |
| :-----: | :--------------------------------------------- | :----------- |
|  [01]   | `Sin / Cos / Tan / Asin / Acos / Atan / Atan2` | circular     |
|  [02]   | `SinCos(x)`                                    | fused        |
|  [03]   | `SinCosPi(x)`                                  | fused        |
|  [04]   | `SinPi / CosPi / TanPi`                        | half-turn    |
|  [05]   | `AsinPi / AcosPi / AtanPi / Atan2Pi`           | half-turn    |
|  [06]   | `Sinh / Cosh / Tanh / Asinh / Acosh / Atanh`   | hyperbolic   |
|  [07]   | `Sinc(x[,normalized]) / Sinhc(x) / Jinc(x)`    | cardinal     |

[FUNCTION_SCOPE]: gamma, beta, digamma, and incomplete forms

| [INDEX] | [SURFACE]                                                           | [CAPABILITY] |
| :-----: | :------------------------------------------------------------------ | :----------- |
|  [01]   | `Gamma(x) / LogGamma(x) / RcpGamma(x) / InverseGamma(x)`            | gamma        |
|  [02]   | `Digamma(x) / InverseDigamma(x) / Polygamma(int n,x)`               | gamma        |
|  [03]   | `Beta(a,b) / LogBeta(a,b)`                                          | beta         |
|  [04]   | `IncompleteBeta(x,a,b) / IncompleteBetaRegularized(x,a,b)`          | beta         |
|  [05]   | `InverseIncompleteBeta(x,a,b)`                                      | beta         |
|  [06]   | `LowerIncompleteGamma(nu,x) / UpperIncompleteGamma(nu,x)`           | gamma        |
|  [07]   | `LowerIncompleteGammaRegularized / UpperIncompleteGammaRegularized` | gamma        |
|  [08]   | `InverseLowerIncompleteGamma / InverseUpperIncompleteGamma`         | gamma        |
|  [09]   | `Binomial(int n,int k) / HarmonicNumber(int n)`                     | sequence     |
|  [10]   | `BarnesG(x) / LogBarnesG(x)`                                        | Barnes G     |

[FUNCTION_SCOPE]: error, Fresnel, Dawson, and integral functions

| [INDEX] | [SURFACE]                                               | [CAPABILITY]  |
| :-----: | :------------------------------------------------------ | :------------ |
|  [01]   | `Erf(x) / Erfc(x) / Erfcx(x) / Erfi(x)`                 | error         |
|  [02]   | `InverseErf(x) / InverseErfc(x)`                        | inverse error |
|  [03]   | `FresnelC(x) / FresnelS(x) / FresnelF(x) / FresnelG(x)` | Fresnel       |
|  [04]   | `DawsonF(x)`                                            | Dawson        |
|  [05]   | `Ei(x) / Ein(x) / Li(x) / En(int n,x)`                  | exponential   |
|  [06]   | `Si(x[,limit_zero]) / Ci(x)`                            | trigonometric |
|  [07]   | `Shi(x) / Chi(x) / Ti(x)`                               | hyperbolic    |
|  [08]   | `OwenT(h,a)`                                            | probability   |

[FUNCTION_SCOPE]: Bessel, Airy, Scorer, Struve, Anger-Weber

`BesselI`/`BesselK` take a `scale` flag for exponential scaling that avoids overflow.

| [INDEX] | [SURFACE]                                             | [CAPABILITY] |
| :-----: | :---------------------------------------------------- | :----------- |
|  [01]   | `BesselJ(nu,x) / BesselY(nu,x)`                       | Bessel       |
|  [02]   | `BesselI(nu,x[,scale]) / BesselK(nu,x[,scale])`       | Bessel       |
|  [03]   | `BesselJ(int n,x) / BesselY(int n,x)`                 | Bessel       |
|  [04]   | `BesselI(int n,x[,scale]) / BesselK(int n,x[,scale])` | Bessel       |
|  [05]   | `AiryAi(x) / AiryBi(x)`                               | Airy         |
|  [06]   | `ScorerGi(x) / ScorerHi(x)`                           | Scorer       |
|  [07]   | `StruveH(int n,x) / StruveK(int n,x)`                 | Struve       |
|  [08]   | `StruveL(int n,x) / StruveM(int n,x)`                 | Struve       |
|  [09]   | `AngerJ(int n,x) / WeberE(int n,x)`                   | Anger-Weber  |

[FUNCTION_SCOPE]: elliptic integrals and Jacobi elliptic functions

Every elliptic function takes parameter `m = k²`.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY] |
| :-----: | :-------------------------------------------------------------- | :----------- |
|  [01]   | `EllipticK(m) / EllipticE(m)`                                   | Legendre     |
|  [02]   | `EllipticE(x,m) / EllipticF(x,m)`                               | Legendre     |
|  [03]   | `EllipticPi(n,m) / EllipticPi(n,x,m)`                           | Legendre     |
|  [04]   | `CarlsonRC(x,y) / CarlsonRD(x,y,z) / CarlsonRF(x,y,z)`          | Carlson      |
|  [05]   | `CarlsonRG(x,y,z) / CarlsonRJ(x,y,z,rho)`                       | Carlson      |
|  [06]   | `JacobiSn(x,m) / JacobiCn(x,m) / JacobiDn(x,m) / JacobiAm(x,m)` | Jacobi       |
|  [07]   | `JacobiArcSn(x,m) / JacobiArcCn(x,m) / JacobiArcDn(x,m)`        | inverse      |
|  [08]   | `EllipticTheta(int a,x,q)`                                      | theta        |
|  [09]   | `EulerQ(q) / LogEulerQ(q)`                                      | Euler        |

[FUNCTION_SCOPE]: zeta family, Clausen, Kepler, Mathieu

| [INDEX] | [SURFACE]                                   | [CAPABILITY]   |
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

| [INDEX] | [SURFACE]                                       | [CAPABILITY] |
| :-----: | :---------------------------------------------- | :----------- |
|  [01]   | `LegendreP(int n,x) / LegendreP(int n,int m,x)` | Legendre     |
|  [02]   | `ChebyshevT(int n,x) / ChebyshevU(int n,x)`     | Chebyshev    |
|  [03]   | `HermiteH(int n,x)`                             | Hermite      |
|  [04]   | `LaguerreL(int n,x) / LaguerreL(int n,alpha,x)` | Laguerre     |
|  [05]   | `GegenbauerC(int n,alpha,x)`                    | Gegenbauer   |
|  [06]   | `JacobiP(int n,alpha,beta,x)`                   | Jacobi       |
|  [07]   | `ZernikeR(int n,int m,x)`                       | Zernike      |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- representation: one `readonly struct ddouble` holds a private unevaluated hi/lo `double` pair (~106-bit significand, ~31 decimal digits via `DecimalDigits`); the pair is internal, so a consumer treats `ddouble` purely as a number.
- precision boundary: precision is fixed at two doubles, so `ddouble` deterministically refines `double` but cannot stand in for the `BigRational` exact `Sign` oracle; a result needing more than ~106 bits escalates to `Fraction`.
- error-free transforms: FMA `TwoProduct` and Knuth/Dekker `TwoSum` match the `Expansion` kernel, so the 106-bit stage and the `Expansion` exact branch share one rounding model and agree on a determinant sign up to 106-bit resolution.

[STACKING]:
- `ExtendedNumerics.BigRational`(`.api/api-bigrational.md`): `ddouble` sits above the `double` filter and below the `Expansion` exact branch and the `Fraction` rational oracle; the shared `Expansion` rounding model makes the ladder monotone, each tier refining the one below, and only sub-106-bit-degenerate residue promotes to `Fraction.Sign`.
- `MathNet.Numerics`(`.api/api-mathnet-numerics.md`): a kernel parameterized on `INumber<T>` lifts the `double` `SpecialFunctions`/`Integrate`/`RootFinding` lane to `ddouble` by substituting the type argument, changing precision only in the accuracy-critical special-function evaluation.
- `System.Numerics.Tensors`(`.api/api-tensors.md`): `TensorPrimitives<T>` operate on any `INumber<T>`, so a span of `ddouble` flows through the generic tensor primitives with no `ddouble`-specific SIMD path, and `DoubleDoubleEnumerableExpand.Sum`/`Average` give the accumulation-accurate fold off that path.
- within-lib: `DDoubleJsonConverter` on `JsonSerializerOptions.Converters` and `DoubleDoubleIOExpand.Write`/`ReadDDouble` carry a 106-bit value across a wire or persistence boundary without degrading to `double`.

[LOCAL_ADMISSION]:
- `ddouble` is the middle predicate tier between the interior `double` filter and the sign-exact `Expansion`/`Fraction` adjudicators: the predicate recomputes the determinant in `ddouble` when the `double` error bound brackets zero, and only sub-106-bit-degenerate residue advances to exact adjudication.
- `ddouble.Sign` or an operator comparison gives the predicate its verdict and the value is discarded.
- non-predicate numeric work (curvature, geodesic, fitting residuals, quadrature weights) uses the special-function library at 106-bit where the `double` `MathNet.Numerics` result loses too many digits, narrowing to `double` only at the reporting boundary.

[RAIL_LAW]:
- Package: `TYoshimura.DoubleDouble`
- Owns: the 106-bit `ddouble` number, its complete generic-math and special-function surface, and its aggregation, binary I/O, and JSON siblings.
- Accept: the predicate middle tier (recompute in `ddouble`, read `Sign`, escalate only indeterminate residue to `Expansion`/`Fraction`); a generic numeric kernel constrained on `INumber<T>` or a function-group interface bound to `ddouble`; accuracy-critical special-function or quadrature evaluation that loses digits at `double`; lossless 106-bit (de)serialization through the converter and I/O extensions.
- Reject: treating `ddouble` as an exact or arbitrary-precision oracle, where `Fraction` owns the exact `Sign`; hand-assembling or reading its internal `Hi`/`Lo` pair; narrowing `ddouble` to `double` mid-adjudication; re-hosting a whole `MathNet.Numerics` distribution or solver on `ddouble` by hand when only the special-function evaluation needs the precision; presenting an internal `Consts`-kernel helper as public API.
