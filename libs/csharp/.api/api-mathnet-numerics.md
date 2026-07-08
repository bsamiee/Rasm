# [RASM_API_MATHNET_NUMERICS]

`MathNet.Numerics` supplies continuous and discrete probability distributions over
`IContinuousDistribution`/`IDiscreteDistribution`, numerical quadrature via `Integrate`
(double-exponential, Gauss-Legendre, adaptive Gauss-Kronrod, 2D/3D), root-finding via
the per-method `Brent`/`Bisection`/`NewtonRaphson`/`RobustNewtonRaphson`/`Secant`/
`Broyden`/`Cubic` static classes (each with a no-throw `TryFindRoot`), the
`Interpolate` scheme family (cubic spline + variants, polynomial, Floater-Hormann
rational, linear/log-linear/step), a broad `SpecialFunctions` catalog (Gamma/Beta
families, error function, general-order and modified Bessel), and the `Distance` metric
catalog (L1/L2/L∞/Minkowski over `Vector<T>` and raw arrays; cosine/Canberra/Hamming/
Jaccard array-only) for the numeric lane's statistical, analytical, and domain
computation paths; the spectral lane rides
`IntegralTransforms.Fourier` (in-place complex / split-real / packed-real FFT + inverse,
multi-dimensional and 2D transforms, and the `FrequencyScale` bin axis) tapered by the
`Window` factory catalog (19 symmetric / `*Periodic` analysis tapers). Linear algebra, provider
selection, and sparse solve are `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md` / `api-csparse`. ABI: MathNet
`lib/net8.0` is the highest TFM in — the net10 consumer binds net8.0; MIT.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (; license MIT)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.Distributions`, `MathNet.Numerics.Integration`, `MathNet.Numerics.IntegralTransforms`, `MathNet.Numerics.Interpolation`, `MathNet.Numerics.RootFinding`
- asset: runtime library (managed; native providers are a separate concern, `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md`)
- floor: net8.0 (no net10/net9 lib in; the net10 consumer binds `lib/net8.0`)
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: distribution seams
- rail: numeric

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------ |:------------ |:------------------------------------------------------------------------------------- |
| [01] | `IDistribution` | interface | root distribution seam; `Mean`, `Variance`, entropy |
| [02] | `IUnivariateDistribution` | interface | adds `CumulativeDistribution(x)` |
| [03] | `IContinuousDistribution` | interface | adds `Density`, `DensityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples` |
| [04] | `IDiscreteDistribution` | interface | adds `Probability`, `ProbabilityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples` |

[PUBLIC_TYPE_SCOPE]: continuous distributions
- rail: numeric

| [INDEX] | [SYMBOL] | [PARAMETERS] | [CAPABILITY] |
|:-----: |:------------------ |:------------------------- |:----------------------------------- |
| [01] | `Normal` | `mean, stdDev` | Gaussian; PDF/CDF/sample/inverse CDF |
| [02] | `LogNormal` | `mu, sigma` | log-normal |
| [03] | `Gamma` | `shape, rate` | Gamma |
| [04] | `Beta` | `a, b` | Beta |
| [05] | `ChiSquared` | `freedom` | chi-squared |
| [06] | `StudentT` | `location, scale, freedom` | Student's t |
| [07] | `Exponential` | `rate` | exponential |
| [08] | `Weibull` | `shape, scale` | Weibull |
| [09] | `ContinuousUniform` | `lower, upper` | continuous uniform |
| [10] | `Cauchy` | `location, scale` | Cauchy / Lorentz |
| [11] | `Laplace` | `location, scale` | Laplace |
| [12] | `Rayleigh` | `scale` | Rayleigh |
| [13] | `FisherSnedecor` | `d1, d2` | F-distribution |
| [14] | `Triangular` | `lower, upper, mode` | triangular |
| [15] | `Pareto` | `scale, shape` | Pareto |
| [16] | `InverseGamma` | `shape, scale` | inverse-Gamma |
| [17] | `BetaScaled` | `a, b, lower, upper` | scaled Beta |
| [18] | `Logistic` | `mean, scale` | logistic |

[PUBLIC_TYPE_SCOPE]: discrete distributions
- rail: numeric

| [INDEX] | [SYMBOL] | [PARAMETERS] | [CAPABILITY] |
|:-----: |:----------------- |:----------------- |:------------------------ |
| [01] | `Binomial` | `p, n` | binomial |
| [02] | `Poisson` | `lambda` | Poisson |
| [03] | `NegativeBinomial` | `r, p` | negative binomial |
| [04] | `Bernoulli` | `p` | Bernoulli trial |
| [05] | `DiscreteUniform` | `lower, upper` | discrete uniform |
| [06] | `Geometric` | `p` | geometric |
| [07] | `Hypergeometric` | `population, k, n` | hypergeometric |
| [08] | `Categorical` | `probabilities[]` | categorical / multinoulli |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: distribution construction and sampling
- rail: numeric

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:-------------------------------------------------- |:------------ |:--------------------------------------------------- |
| [01] | `new Normal(mean, stdDev)` | constructor | Gaussian instance |
| [02] | `new Normal(mean, stdDev, randomSource)` | constructor | Gaussian with explicit RNG |
| [03] | `IContinuousDistribution.Density(x)` / `DensityLn(x)` | instance | PDF / log-PDF at x |
| [04] | `IUnivariateDistribution.CumulativeDistribution(x)` | instance | CDF at x |
| [05] | `Normal.InverseCumulativeDistribution(p)` | instance | quantile / inverse CDF (per concrete type) |
| [06] | `IContinuousDistribution.Sample()` / `Samples(double[])` / `Samples()` | instance | one sample / fill caller buffer / lazy infinite seq |
| [07] | `IDiscreteDistribution.Probability(k)` / `ProbabilityLn(k)` | instance | PMF / log-PMF at k |
| [08] | `IDiscreteDistribution.Sample()` | instance | single integer sample |
| [09] | `Normal.RandomSource` | instance prop | gets or sets the RNG (replaces with default on null) |
| [10] | `Normal.CDF(mean, stdDev, x)` / `StudentT.CDF(location, scale, freedom, x)` / per-type static twins | static `double` | parameterized CDF without an instance — the hypothesis-test p-value form |
| [11] | `Normal.InvCDF(mean, stdDev, p)` / `StudentT.InvCDF(location, scale, freedom, p)` / per-type static twins | static `double` | parameterized quantile without an instance (β = `Normal.InvCDF(0, 1, 1 - pf)`) |
| [12] | `Generate.LinearRange(start, step, stop)` / `LinearSpaced(length, start, stop)` | static `double[]` | arithmetic-progression / evenly-spaced axis generation |
| [13] | `Generate.LinearRangeMap(start, step, stop, Func<double,double>)` / `LinearSpacedMap(length, start, stop, map)` | static `T[]` | fused axis-generate-and-map — one pass, no intermediate axis array |
| [14] | `Generate.Map(double[], Func<double,T>)` / `Map2(a, b, Func<double,double,T>)` | static `T[]` | element / pairwise projection over existing arrays |

[ENTRYPOINT_SCOPE]: integration (`Integrate` class)
- rail: numeric

`OnClosedInterval` is double-exponential with an absolute-error target; `GaussKronrod` is the adaptive method with explicit error/`L1Norm` out-params and depth control; 2D/3D are `OnRectangle`/`OnCuboid`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------- |:------------ |:---------------------------------------------- |
| [01] | `Integrate.OnClosedInterval(f, a, b[, targetAbsoluteError])` | static `double` | double-exponential quadrature (default 1e-8) |
| [02] | `Integrate.GaussLegendre(f, a, b, order = 128)` | static `double` | fixed-order Gauss-Legendre |
| [03] | `Integrate.GaussKronrod(f, a, b, [out error, out L1Norm,] targetRelativeError = 1e-8, maximumDepth = 15, order = 15)` | static `double` | adaptive Gauss-Kronrod with error estimate |
| [04] | `Integrate.DoubleExponential(f, a, b, targetAbsoluteError = 1e-8)` | static `double` | explicit double-exponential transform |
| [05] | `Integrate.OnRectangle(f2d, aA, bA, aB, bB[, order])` | static `double` | 2D Gauss-Legendre over `[a,b]×[c,d]` |
| [06] | `Integrate.OnCuboid(f3d, aA, bA, aB, bB, aC, bC, order = 32)` | static `double` | 3D Gauss-Legendre over a box |

[ENTRYPOINT_SCOPE]: root-finding (per-method static classes)
- rail: numeric

There is no `FindRoots` aggregator type — root-finding is one static class per method, each exposing `FindRoot` (throws `NonConvergenceException` on failure), the no-throw `TryFindRoot(..., out double root) -> bool`, and (bracketing methods) `FindRootExpand` with automatic bracket expansion.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------- |:------------ |:--------------------------------------- |
| [01] | `Brent.TryFindRoot(f, lower, upper, accuracy, maxIter, out root)` | static `bool` | Brent on a bracket; canonical no-throw entry |
| [02] | `Brent.FindRootExpand(f, guessLower, guessUpper,...)` | static `double` | Brent with automatic bracket expansion |
| [03] | `Bisection.FindRoot(f, lower, upper, accuracy = 1e-14, maxIter)` | static `double` | bisection (robust, slow) |
| [04] | `NewtonRaphson.FindRoot(f, df, lower, upper, accuracy, maxIter)` | static `double` | Newton-Raphson with derivative |
| [05] | `RobustNewtonRaphson.FindRoot(f, df, lower, upper,..., subdivision = 20)` | static `double` | Newton guarded by bisection subdivision |
| [06] | `Secant.FindRoot(f, guess, secondGuess, lower, upper,...)` | static `double` | secant (no derivative) |
| [07] | `Broyden.FindRoot(Func<double[],double[]> f, double[] initialGuess, accuracy, maxIter)` | static `double[]` | quasi-Newton for nonlinear systems |
| [08] | `Cubic.RealRoots(a0, a1, a2)` → `(double,double,double)` / `Roots(d, c, b, a)` → `(Complex,Complex,Complex)` | static tuple | closed-form cubic roots |

[ENTRYPOINT_SCOPE]: interpolation (`Interpolate` class)
- rail: numeric

Every factory returns an `IInterpolation` (`Interpolate(x)`, `Differentiate(x)`, `Integrate(a,b)`). Cubic-spline variants are the load-bearing schemes for geometry; Floater-Hormann rational handles arbitrary unsorted data without poles.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------ |:------------------------------------------------ |
| [01] | `Interpolate.CubicSpline(points, values)` | static `IInterpolation` | natural cubic spline |
| [02] | `Interpolate.CubicSplineRobust(points, values)` | static `IInterpolation` | robust (Akima-style) cubic spline |
| [03] | `Interpolate.CubicSplineMonotone(points, values)` | static `IInterpolation` | shape-preserving monotone cubic spline |
| [04] | `Interpolate.CubicSplineWithDerivatives(points, values, firstDerivatives)` | static `IInterpolation` | Hermite cubic with given slopes |
| [05] | `Interpolate.Common(points, values)` / `RationalWithoutPoles(...)` | static `IInterpolation` | Floater-Hormann rational; arbitrary unsorted / pole-free |
| [06] | `Interpolate.Polynomial(points, values)` / `PolynomialEquidistant(...)` | static `IInterpolation` | barycentric polynomial |
| [07] | `Interpolate.Linear(points, values)` / `LogLinear(...)` / `Step(...)` | static `IInterpolation` | piecewise linear / log-linear / step |

[ENTRYPOINT_SCOPE]: special functions (`SpecialFunctions` class)
- rail: numeric

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------------------- |:------------ |:----------------------------------- |
| [01] | `SpecialFunctions.Gamma(x)` / `GammaLn(x)` | static `double` | Gamma Γ(x) / log-Gamma ln Γ(x) |
| [02] | `SpecialFunctions.GammaLowerRegularized(a, x)` / `GammaUpperRegularized(a, x)` | static `double` | regularized incomplete gamma P/Q |
| [03] | `SpecialFunctions.GammaLowerIncomplete(a, x)` / `GammaUpperIncomplete(a, x)` | static `double` | unregularized incomplete gamma |
| [04] | `SpecialFunctions.DiGamma(x)` / `DiGammaInv(p)` | static `double` | digamma ψ(x) / its inverse |
| [05] | `SpecialFunctions.Beta(a, b)` / `BetaLn(a, b)` | static `double` | Beta B(a,b) / log-Beta |
| [06] | `SpecialFunctions.BetaRegularized(a, b, x)` / `BetaIncomplete(a, b, x)` | static `double` | regularized / unregularized incomplete beta |
| [07] | `SpecialFunctions.Erf(x)` / `Erfc(x)` | static `double` | error / complementary error function |
| [08] | `SpecialFunctions.Logistic(x)` / `Logit(p)` | static `double` | logistic sigmoid / its inverse (logit) |
| [09] | `SpecialFunctions.Harmonic(t)` / `Factorial(n)` / `FactorialLn(n)` | static `double` | harmonic number / factorial / log-factorial |
| [10] | `SpecialFunctions.BesselJ(nu, x)` / `BesselY(nu, x)` | static `double` | Bessel J_ν / Y_ν (general order) |
| [11] | `SpecialFunctions.BesselI0(x)` / `BesselI1(x)` / `BesselK0(x)` / `BesselK1(x)` | static `double` | modified Bessel I/K, order 0 and 1 |

[ENTRYPOINT_SCOPE]: distance metrics (`Distance` class, root `MathNet.Numerics`)
- rail: numeric

Every metric is a static reduce over a vector pair. The core metrics ship three overloads — `Distance.M<T>(Vector<T> a, Vector<T> b) where T: struct, IEquatable<T>, IFormattable` plus raw `(double[], double[]) → double` and `(float[], float[]) → float` — while `Cosine`/`Canberra`/`Hamming`/`Jaccard` are ARRAY-ONLY (no `Vector<T>` overload) and `Pearson` takes `IEnumerable<double>`; a descriptor-ranking lane that must swap metrics under one delegate therefore normalizes on the array forms.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------- |:---------------- |:--------------------------------------------------------- |
| [01] | `Distance.Euclidean(a, b)` | static `double`/`float` | L2 metric; `Vector<T>` + `double[]` + `float[]` overloads |
| [02] | `Distance.Manhattan(a, b)` | static `double`/`float` | L1 metric; `Vector<T>` + `double[]` + `float[]` overloads |
| [03] | `Distance.Chebyshev(a, b)` | static `double`/`float` | L∞ metric; `Vector<T>` + `double[]` + `float[]` overloads |
| [04] | `Distance.Minkowski(p, a, b)` | static `double`/`float` | order-`p` metric; `Vector<T>` + array overloads |
| [05] | `Distance.Cosine(double[] a, double[] b)` / `(float[] …)` | static `double`/`float` | cosine distance `1 − cos θ`; ARRAY-ONLY |
| [06] | `Distance.Canberra(a, b)` / `Hamming(a, b)` / `Jaccard(a, b)` | static `double`/`float` | weighted-L1 / count-differing / set-overlap; ARRAY-ONLY |
| [07] | `Distance.SAD(a, b)` / `MAE(a, b)` / `SSD(a, b)` / `MSE(a, b)` | static `double`/`float` | absolute-sum / mean-absolute / squared-sum / mean-squared deviation; `Vector<T>` + array overloads |
| [08] | `Distance.Pearson(IEnumerable<double> a, IEnumerable<double> b)` | static `double` | correlation distance `1 − r` |

[ENTRYPOINT_SCOPE]: least-squares fitting (`Fit` class, root `MathNet.Numerics`)
- rail: numeric

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------- |:---------------------------- |:------------------------------------------------------------------------------ |
| [01] | `Fit.Line(double[] x, double[] y)` | static `(double A, double B)` | least-squares line `y = a + b·x` — intercept `A`, slope `B`; the log-log calibration substrate (Kienzle `kc`, Taylor life, wear-rate trajectories) |

[ENTRYPOINT_SCOPE]: discrete Fourier transform (`Fourier` class, `MathNet.Numerics.IntegralTransforms`)
- rail: numeric

Every transform is IN-PLACE over the caller's buffer; the scaling convention is the `FourierOptions` flags enum — `Default` (0, symmetric `1/√N` + forward exponent), `AsymmetricScaling` (2, alias `Matlab`), `NoScaling` (4 — `Forward`∘`Inverse` composes to identity only here), `InverseExponent` (1), `NumericalRecipes` (5). The split `double[] real, double[] imaginary` form is preferred over `Complex[]` because a vectorized magnitude/phase reads the contiguous spans with no `Complex` marshalling; `ForwardReal`/`InverseReal` pack the conjugate-even half-spectrum into an `N+2` (even N) / `N+1` (odd N) buffer; `FrequencyScale` is the bin axis, never `1/length`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------------ |:-------------- |:--------------------------------------------------------- |
| [01] | `Fourier.Forward(Complex[] samples[, FourierOptions])` / `Forward(Complex32[] …)` | static `void` | in-place complex FFT (double / single precision) |
| [02] | `Fourier.Forward(double[] real, double[] imaginary[, FourierOptions = Default])` / `Forward(float[] …)` | static `void` | in-place split real/imaginary FFT (no `Complex` marshalling) |
| [03] | `Fourier.Inverse(Complex[] spectrum[, FourierOptions])` / `Inverse(double[] real, double[] imaginary[, …])` | static `void` | in-place inverse FFT (complex or split form) |
| [04] | `Fourier.ForwardReal(double[] data, int n[, FourierOptions = Default])` / `ForwardReal(float[] …)` | static `void` | packed real→half-spectrum FFT into the `N+2`/`N+1` buffer |
| [05] | `Fourier.InverseReal(double[] data, int n[, FourierOptions = Default])` / `InverseReal(float[] …)` | static `void` | packed half-spectrum→real inverse FFT |
| [06] | `Fourier.ForwardMultiDim(Complex[] samples, int[] dimensions[, FourierOptions])` / `Forward2D(Complex[] samplesRowWise, int rows, int columns[, …])` / `Forward2D(Matrix<Complex>[, …])` | static `void` | in-place multi-dimensional / 2D FFT (row-major) |
| [07] | `Fourier.InverseMultiDim(...)` / `Inverse2D(...)` | static `void` | in-place multi-dimensional / 2D inverse FFT |
| [08] | `Fourier.FrequencyScale(int length, double sampleRate)` | static `double[]` | per-bin frequency axis (positive bins then wrapped negatives; `Δf = sampleRate/length`) |

[ENTRYPOINT_SCOPE]: window tapers (`Window` class)
- rail: numeric

19 taper factories, each `(int width) → double[]`. The four cosine-sum families ship a symmetric form (filter design) and a `*Periodic` form (FFT/STFT framing — these four are the only periodic pairs MathNet exposes). `Dirichlet` is the all-ones rectangular taper; `Gauss`/`Tukey` are the only parameterized factories. MathNet ships NO wavelet surface.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:---------------- |:-------------------------------------------- |
| [01] | `Window.Hann(width)` / `Window.HannPeriodic(width)` | static `double[]` | Hann (raised cosine); symmetric / FFT-periodic |
| [02] | `Window.Hamming(width)` / `Window.HammingPeriodic(width)` | static `double[]` | Hamming; symmetric / periodic |
| [03] | `Window.Cosine(width)` / `Window.CosinePeriodic(width)` | static `double[]` | cosine / sine taper; symmetric / periodic |
| [04] | `Window.Lanczos(width)` / `Window.LanczosPeriodic(width)` | static `double[]` | Lanczos (sinc); symmetric / periodic |
| [05] | `Window.Blackman(width)` | static `double[]` | Blackman three-term |
| [06] | `Window.BlackmanHarris(width)` | static `double[]` | Blackman-Harris four-term |
| [07] | `Window.BlackmanNuttall(width)` | static `double[]` | Blackman-Nuttall four-term |
| [08] | `Window.Nuttall(width)` | static `double[]` | Nuttall four-term |
| [09] | `Window.FlatTop(width)` | static `double[]` | flat-top five-term (amplitude-accurate) |
| [10] | `Window.Bartlett(width)` | static `double[]` | Bartlett (triangular, zero endpoints) |
| [11] | `Window.BartlettHann(width)` | static `double[]` | Bartlett-Hann |
| [12] | `Window.Triangular(width)` | static `double[]` | triangular (non-zero endpoints) |
| [13] | `Window.Dirichlet(width)` | static `double[]` | rectangular (all-ones) |
| [14] | `Window.Gauss(width, sigma)` | static `double[]` | Gaussian taper, std-dev `sigma` |
| [15] | `Window.Tukey(width, r =)` | static `double[]` | tapered-cosine, taper fraction `r` |

## [04]-[IMPLEMENTATION_LAW]

[DISTRIBUTION_TOPOLOGY]:
- namespace: `MathNet.Numerics.Distributions`
- seam hierarchy: `IDistribution` → `IUnivariateDistribution` → `IContinuousDistribution` or `IDiscreteDistribution`
- construction: each distribution has a constructor per parameter set plus an optional `System.Random randomSource` overload; passing `null` or omitting assigns `SystemRandomSource.Default`
- sampling: `Sample()` returns one value; `Samples(array)` fills the caller-owned buffer; `Samples()` returns a lazy `IEnumerable<T>` — none allocate an internal result store
- `RandomSource` is a mutable property; setting it to `null` replaces with the shared default, never throws — `CumulativeDistribution` lives on `IUnivariateDistribution`, so cast to it (or the concrete type) to call CDF/inverse-CDF from C#

[INTEGRATION_TOPOLOGY]:
- namespace: `MathNet.Numerics`; delegates to `MathNet.Numerics.Integration` (`DoubleExponentialTransformation`, `GaussLegendreRule`, `GaussKronrodRule`)
- `OnClosedInterval` is double-exponential with a `1e-8` default absolute-error target — suitable for smooth analytic functions; `GaussKronrod` is the adaptive method with an explicit relative-error target, `maximumDepth`, and an `out error`/`out L1Norm` estimate, for functions where the error must be bounded
- `OnRectangle`/`OnCuboid` use fixed-order Gauss-Legendre product rules over a box; the `order` argument selects the per-axis node count

[ROOT_FINDING_TOPOLOGY]:
- namespace: `MathNet.Numerics.RootFinding` — no `FindRoots` aggregator; the surface is one static class per method (`Brent`, `Bisection`, `NewtonRaphson`, `RobustNewtonRaphson`, `Secant`, `Broyden`, `Cubic`)
- selection by available information: `Brent.TryFindRoot` (bracket, no derivative) is the canonical entry; `NewtonRaphson`/`RobustNewtonRaphson` when an analytic derivative `df` is available; `Secant` for derivative-free local convergence; `Broyden` for nonlinear systems; `Cubic` for closed-form cubic roots
- `FindRoot` throws `NonConvergenceException` on failure; `TryFindRoot(..., out double root)` is the no-throw twin that returns `false` — the no-throw form is the one a `Fin`/`Option` rail composes; `FindRootExpand` adds automatic bracket expansion to the bracketing methods

[SPECTRAL_TOPOLOGY]:
- namespace: `MathNet.Numerics.IntegralTransforms` (`Fourier`, `FourierOptions`) plus root `MathNet.Numerics` (`Window`)
- every `Fourier` transform is IN-PLACE over the caller's buffer; the scaling convention is the `FourierOptions` flags enum (`Default` = 0 symmetric, `AsymmetricScaling` = 2 alias `Matlab`, `NoScaling` = 4, `InverseExponent` = 1, `NumericalRecipes` = 5) — a `Forward`∘`Inverse` round-trip composes to identity only under `NoScaling`, so an FFT-then-IFFT path pins that option
- three real-signal carriers: the `Complex[]`/`Complex32[]` form; the split `double[] real, double[] imaginary` form (a vectorized magnitude/phase reads the contiguous spans with no `Complex` marshalling — the preferred form); and the packed `ForwardReal`/`InverseReal` form that stores the conjugate-even half-spectrum in an `N+2` (even N) / `N+1` (odd N) buffer; `ForwardMultiDim`/`Forward2D` (and their inverses) cover row-major multi-dimensional and 2D data
- `FrequencyScale(length, sampleRate)` returns the per-bin frequency axis — the positive bins `0, Δf, 2Δf, …` over the first `⌊N/2⌋+1` then the wrapped negative bins, `Δf = sampleRate/length` — never the meaningless `1/length`
- `Window` is 19 `(width) → double[]` taper factories; the four cosine-sum families (`Hann`/`Hamming`/`Cosine`/`Lanczos`) ship a symmetric form (filter design) and a `*Periodic` form (FFT/STFT framing), the only periodic pairs MathNet exposes; `Dirichlet` is the all-ones rectangular taper; `Gauss(width, sigma)` and `Tukey(width, r =)` are the only parameterized factories — MathNet ships NO wavelet surface

[STACK]:
- A root-find on a boundary rail composes the no-throw `Brent.TryFindRoot(f, lo, hi, acc, maxIter, out root)`: the `bool` maps to `Fin.Succ(root)`/`Fin.Fail(...)` (or `Option`) at the seam, so a non-convergence is a typed failure row, never a `NonConvergenceException` thrown through the receipt path
- A distribution is an owned value (stateful through `RandomSource`), not a static singleton — under parallel sampling each worker holds its own distribution instance with its own `RandomSource`, threaded through the same anyio/Task fan the surrounding pipeline uses, so the RNG never races
- `Interpolate.CubicSpline*` returns an `IInterpolation` whose `Interpolate(x)`/`Differentiate(x)`/`Integrate(a,b)` compose into a geometry sampling pipeline; the rational/Floater-Hormann path is the fallback for unsorted or pole-prone data — the scheme is a policy row over one `IInterpolation` seam, never a per-scheme public surface
- `SpecialFunctions` is the closed-form anchor under distribution/statistics work: `GammaLn`/`BetaRegularized`/`Erf` back the analytic CDFs, so a hand-rolled Gamma/Beta/erf beside them is the deleted form
- A spectral pass widens the real signal to `double[]` once, runs `Fourier.Forward(real, imaginary, FourierOptions.Default)` over the split pair, and reads magnitude/phase with a vectorized `Hypot`/`Atan2` over the contiguous spans — the split form is chosen precisely to avoid per-element `Complex` marshalling; an FFT-then-IFFT round-trip pins `FourierOptions.NoScaling` so `Forward`∘`Inverse` is identity; bin spacing reads `Fourier.FrequencyScale`, never `1/N`; the analysis window is one `Window.*` taper (`*Periodic` for FFT framing, symmetric for filter design), never a hand-rolled cosine or `Enumerable.Repeat` rectangular taper

[LOCAL_ADMISSION]:
- Distributions are stateful through `RandomSource`; treat them as owned values, not static singletons, when parallel sampling is required
- `Integrate.OnClosedInterval` integrates a real scalar `Func<double,double>`; `GaussKronrod` is the adaptive form when an error bound is required; the two-/three-dimensional overloads are `OnRectangle`/`OnCuboid`
- Root-finding is per-method static classes (`Brent`/`NewtonRaphson`/...), not a `FindRoots` aggregator; the no-throw `TryFindRoot` is the rail-composable form
- Spectral transforms are `MathNet.Numerics.IntegralTransforms.Fourier` (in-place, `FourierOptions`-scaled; split `double[]` real/imaginary form preferred, packed `ForwardReal`/`InverseReal` for the Hermitian half-spectrum) and the analysis tapers are the 19-factory `MathNet.Numerics.Window` catalog; the radix-2/Bluestein kernel, the taper math, and the bin axis (`FrequencyScale`) are owned here, never re-implemented — MathNet has no wavelet surface, so a wavelet filter bank is authored against frozen scaling tables
- Linear algebra, provider selection, and sparse solve are `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md` / `api-csparse` and are not duplicated here

[RAIL_LAW]:
- Package: `MathNet.Numerics` (core assembly, non-provider namespaces)
- Owns: probability distributions, numerical integration (incl. adaptive Gauss-Kronrod and 2D/3D), root-finding (`Brent`/`Bisection`/`Newton`/`RobustNewton`/`Secant`/`Broyden`/`Cubic`), interpolation (cubic-spline family + polynomial + rational), special functions (Gamma/Beta/erf/Bessel), least-squares line fit (`Fit.Line`), the `Distance` metric catalog (Euclidean/Manhattan/Chebyshev/Minkowski over `Vector<T>`+arrays; Cosine/Canberra/Hamming/Jaccard array-only; Pearson), discrete Fourier transforms (in-place complex / split-real / packed-real / multi-dim + `FrequencyScale`), window tapers (19-factory `Window` catalog)
- Accept: `Func<double, double>` integrands and root targets, `IContinuousDistribution`/`IDiscreteDistribution` seams, `IInterpolation` results, the no-throw `TryFindRoot` rail form, in-place `Complex[]`/split `double[]` spectral buffers under a `FourierOptions` scaling, `Window` tapers as `double[]`
- Reject: hand-rolled distribution PDF/CDF, custom quadrature when `Integrate` covers the interval shape, a phantom `FindRoots` aggregator, local reimplementations of Gamma/Beta/erf/Bessel, a hand-rolled least-squares line beside `Fit.Line`, a hand-rolled pairwise-metric loop beside the `Distance` catalog, a phantom `Distance.Cosine(Vector<T>, Vector<T>)` overload (cosine is array-only), a hand-rolled radix-2/Bluestein FFT, a hand-rolled cosine/rectangular taper, `1/N` bin spacing where `Fourier.FrequencyScale` gives the real axis
