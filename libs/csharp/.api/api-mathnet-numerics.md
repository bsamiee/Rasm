# [RASM_API_MATHNET_NUMERICS]

`MathNet.Numerics` owns probability, approximation, quadrature, root finding, special functions, spectral transforms, and statistical distance for the numeric rail. Linear algebra, provider selection, and sparse solve remain outside the core assembly.

## [01]-[PACKAGE_SURFACE]

- Package: `MathNet.Numerics`
- License: MIT
- Assembly: `MathNet.Numerics`
- Namespace: `MathNet.Numerics`, `MathNet.Numerics.Distributions`, `MathNet.Numerics.Integration`, `MathNet.Numerics.IntegralTransforms`, `MathNet.Numerics.Interpolation`, `MathNet.Numerics.RootFinding`
- Asset: Managed runtime library
- Rail: Numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: distribution seams

Each interface owns one seam in the distribution hierarchy.

| [INDEX] | [SYMBOL]                  | [KIND]    | [CAPABILITY]                   |
| :-----: | :------------------------ | :-------- | :----------------------------- |
|  [01]   | `IDistribution`           | interface | random source                  |
|  [02]   | `IUnivariateDistribution` | interface | univariate statistics          |
|  [03]   | `IContinuousDistribution` | interface | continuous density and samples |
|  [04]   | `IDiscreteDistribution`   | interface | discrete mass and samples      |

[INTERFACE_MEMBERS]:

- `IDistribution`: `RandomSource`
- `IUnivariateDistribution`: `Mean`, `Variance`, `StdDev`, `Entropy`, `Skewness`, `Median`, and `CumulativeDistribution(x)`
- `IContinuousDistribution`: `Density`, `DensityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples(double[])`, and `Samples()`
- `IDiscreteDistribution`: `Probability`, `ProbabilityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples(int[])`, and `Samples()`

[PUBLIC_TYPE_SCOPE]: continuous distributions

Each continuous distribution binds its constructor parameter set.

| [INDEX] | [SYMBOL]            | [PARAMETERS]               |
| :-----: | :------------------ | :------------------------- |
|  [01]   | `Normal`            | `mean, stdDev`             |
|  [02]   | `LogNormal`         | `mu, sigma`                |
|  [03]   | `Gamma`             | `shape, rate`              |
|  [04]   | `Beta`              | `a, b`                     |
|  [05]   | `ChiSquared`        | `freedom`                  |
|  [06]   | `StudentT`          | `location, scale, freedom` |
|  [07]   | `Exponential`       | `rate`                     |
|  [08]   | `Weibull`           | `shape, scale`             |
|  [09]   | `ContinuousUniform` | `lower, upper`             |
|  [10]   | `Cauchy`            | `location, scale`          |
|  [11]   | `Laplace`           | `location, scale`          |
|  [12]   | `Rayleigh`          | `scale`                    |
|  [13]   | `FisherSnedecor`    | `d1, d2`                   |
|  [14]   | `Triangular`        | `lower, upper, mode`       |
|  [15]   | `Pareto`            | `scale, shape`             |
|  [16]   | `InverseGamma`      | `shape, scale`             |
|  [17]   | `BetaScaled`        | `a, b, lower, upper`       |
|  [18]   | `Logistic`          | `mean, scale`              |

[PUBLIC_TYPE_SCOPE]: discrete distributions

Each discrete distribution binds its constructor parameter set.

| [INDEX] | [SYMBOL]           | [PARAMETERS]       |
| :-----: | :----------------- | :----------------- |
|  [01]   | `Binomial`         | `p, n`             |
|  [02]   | `Poisson`          | `lambda`           |
|  [03]   | `NegativeBinomial` | `r, p`             |
|  [04]   | `Bernoulli`        | `p`                |
|  [05]   | `DiscreteUniform`  | `lower, upper`     |
|  [06]   | `Geometric`        | `p`                |
|  [07]   | `Hypergeometric`   | `population, k, n` |
|  [08]   | `Categorical`      | `probabilities[]`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: distribution construction and sampling

Distribution surfaces own construction, probability evaluation, sampling, and sequence projection.

| [INDEX] | [SURFACE]                                                        | [KIND]            | [CAPABILITY]           |
| :-----: | :--------------------------------------------------------------- | :---------------- | :--------------------- |
|  [01]   | `new Normal(mean, stdDev)`                                       | constructor       | Gaussian instance      |
|  [02]   | `new Normal(mean, stdDev, randomSource)`                         | constructor       | explicit RNG           |
|  [03]   | `IContinuousDistribution.Density(x)`                             | instance `double` | probability density    |
|  [04]   | `IContinuousDistribution.DensityLn(x)`                           | instance `double` | log density            |
|  [05]   | `IUnivariateDistribution.CumulativeDistribution(x)`              | instance `double` | cumulative probability |
|  [06]   | `Normal.InverseCumulativeDistribution(p)`                        | instance `double` | quantile               |
|  [07]   | `IContinuousDistribution.Sample()`                               | instance `double` | one sample             |
|  [08]   | `IContinuousDistribution.Samples(double[])`                      | instance `void`   | fill caller buffer     |
|  [09]   | `IContinuousDistribution.Samples()`                              | instance sequence | `IEnumerable<double>`  |
|  [10]   | `IDiscreteDistribution.Probability(k)`                           | instance `double` | probability mass       |
|  [11]   | `IDiscreteDistribution.ProbabilityLn(k)`                         | instance `double` | log probability mass   |
|  [12]   | `IDiscreteDistribution.Sample()`                                 | instance `int`    | one sample             |
|  [13]   | `Normal.RandomSource`                                            | instance property | RNG ownership          |
|  [14]   | `Normal.CDF(mean, stdDev, x)`                                    | static `double`   | cumulative probability |
|  [15]   | `StudentT.CDF(location, scale, freedom, x)`                      | static `double`   | cumulative probability |
|  [16]   | `Normal.InvCDF(mean, stdDev, p)`                                 | static `double`   | quantile               |
|  [17]   | `StudentT.InvCDF(location, scale, freedom, p)`                   | static `double`   | quantile               |
|  [18]   | `Generate.LinearRange(start, step, stop)`                        | static `double[]` | arithmetic progression |
|  [19]   | `Generate.LinearSpaced(length, start, stop)`                     | static `double[]` | evenly spaced axis     |
|  [20]   | `Generate.LinearRangeMap(start, step, stop, Func<double, T>)`    | static `T[]`      | fused range mapping    |
|  [21]   | `Generate.LinearSpacedMap(length, start, stop, Func<double, T>)` | static `T[]`      | fused spacing mapping  |
|  [22]   | `Generate.Map(double[], Func<double, T>)`                        | static `T[]`      | element projection     |
|  [23]   | `Generate.Map2(a, b, Func<double, double, T>)`                   | static `T[]`      | pairwise projection    |

[ENTRYPOINT_SCOPE]: integration via `Integrate`

Every `Integrate` surface is static and returns `double`. Overload pairs separate default-policy calls from explicit error, order, or evidence inputs, and the `GaussKronrod` evidence overload shares the non-evidence defaults.

| [INDEX] | [SURFACE]                                                                                          |
| :-----: | :------------------------------------------------------------------------------------------------- |
|  [01]   | `Integrate.OnClosedInterval(f, a, b)`                                                              |
|  [02]   | `Integrate.OnClosedInterval(f, a, b, targetAbsoluteError)`                                         |
|  [03]   | `Integrate.GaussLegendre(f, a, b, order = 128)`                                                    |
|  [04]   | `Integrate.GaussKronrod(f, a, b, targetRelativeError = 1e-8, maximumDepth = 15, order = 15)`       |
|  [05]   | `Integrate.GaussKronrod(f, a, b, out error, out L1Norm, targetRelativeError, maximumDepth, order)` |
|  [06]   | `Integrate.DoubleExponential(f, a, b, targetAbsoluteError = 1e-8)`                                 |
|  [07]   | `Integrate.OnRectangle(f2d, aA, bA, aB, bB)`                                                       |
|  [08]   | `Integrate.OnRectangle(f2d, aA, bA, aB, bB, order)`                                                |
|  [09]   | `Integrate.OnCuboid(f3d, aA, bA, aB, bB, aC, bC, order = 32)`                                      |

[ENTRYPOINT_SCOPE]: root finding

Each root-finding method owns one static class. Iterative classes expose `TryFindRoot`, whose `bool` return carries failure without throwing; `FindRoot` raises `NonConvergenceException`, and bracketing classes expose `FindRootExpand`.

| [INDEX] | [SURFACE]                                                                                         | [RETURNS]                     |
| :-----: | :------------------------------------------------------------------------------------------------ | :---------------------------- |
|  [01]   | `Brent.TryFindRoot(f, lower, upper, accuracy, maxIter, out root)`                                 | `bool`                        |
|  [02]   | `Brent.FindRootExpand(f, guessLower, guessUpper, accuracy, maxIter, expandFactor, maxExpandIter)` | `double`                      |
|  [03]   | `Bisection.FindRoot(f, lower, upper, accuracy = 1e-14, maxIter = 100)`                            | `double`                      |
|  [04]   | `NewtonRaphson.FindRoot(f, df, lower, upper, accuracy = 1e-8, maxIter = 100)`                     | `double`                      |
|  [05]   | `RobustNewtonRaphson.FindRoot(f, df, lower, upper, accuracy, maxIter, subdivision = 20)`          | `double`                      |
|  [06]   | `Secant.FindRoot(f, guess, secondGuess, lower, upper, accuracy, maxIter)`                         | `double`                      |
|  [07]   | `Broyden.FindRoot(Func<double[], double[]> f, double[] initialGuess, accuracy, maxIter)`          | `double[]`                    |
|  [08]   | `Cubic.RealRoots(a0, a1, a2)`                                                                     | `(double, double, double)`    |
|  [09]   | `Cubic.Roots(d, c, b, a)`                                                                         | `(Complex, Complex, Complex)` |

[ENTRYPOINT_SCOPE]: interpolation via `Interpolate`

Every factory returns an `IInterpolation` with `Interpolate(x)`, `Differentiate(x)`, and `Integrate(a, b)`. Cubic variants cover natural, robust, monotone, and prescribed-derivative schemes, while Floater-Hormann rational handles unsorted data without poles.

| [INDEX] | [SURFACE]                                                                  | [SCHEME]                 |
| :-----: | :------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `Interpolate.CubicSpline(points, values)`                                  | natural cubic            |
|  [02]   | `Interpolate.CubicSplineRobust(points, values)`                            | Akima-style cubic        |
|  [03]   | `Interpolate.CubicSplineMonotone(points, values)`                          | monotone cubic           |
|  [04]   | `Interpolate.CubicSplineWithDerivatives(points, values, firstDerivatives)` | Hermite cubic            |
|  [05]   | `Interpolate.Common(points, values)`                                       | Floater-Hormann rational |
|  [06]   | `Interpolate.RationalWithoutPoles(points, values)`                         | pole-free rational       |
|  [07]   | `Interpolate.Polynomial(points, values)`                                   | barycentric polynomial   |
|  [08]   | `Interpolate.PolynomialEquidistant(points, values)`                        | equidistant polynomial   |
|  [09]   | `Interpolate.Linear(points, values)`                                       | piecewise linear         |
|  [10]   | `Interpolate.LogLinear(points, values)`                                    | log-linear               |
|  [11]   | `Interpolate.Step(points, values)`                                         | step                     |

[ENTRYPOINT_SCOPE]: special functions via `SpecialFunctions`

Each listed function is static and returns `double`.

| [INDEX] | [SURFACE]                                      | [CAPABILITY]                 |
| :-----: | :--------------------------------------------- | :--------------------------- |
|  [01]   | `SpecialFunctions.Gamma(x)`                    | Gamma                        |
|  [02]   | `SpecialFunctions.GammaLn(x)`                  | log Gamma                    |
|  [03]   | `SpecialFunctions.GammaLowerRegularized(a, x)` | regularized lower Gamma      |
|  [04]   | `SpecialFunctions.GammaUpperRegularized(a, x)` | regularized upper Gamma      |
|  [05]   | `SpecialFunctions.GammaLowerIncomplete(a, x)`  | incomplete lower Gamma       |
|  [06]   | `SpecialFunctions.GammaUpperIncomplete(a, x)`  | incomplete upper Gamma       |
|  [07]   | `SpecialFunctions.DiGamma(x)`                  | digamma                      |
|  [08]   | `SpecialFunctions.DiGammaInv(p)`               | inverse digamma              |
|  [09]   | `SpecialFunctions.Beta(a, b)`                  | Beta                         |
|  [10]   | `SpecialFunctions.BetaLn(a, b)`                | log Beta                     |
|  [11]   | `SpecialFunctions.BetaRegularized(a, b, x)`    | regularized incomplete Beta  |
|  [12]   | `SpecialFunctions.BetaIncomplete(a, b, x)`     | incomplete Beta              |
|  [13]   | `SpecialFunctions.Erf(x)`                      | error function               |
|  [14]   | `SpecialFunctions.Erfc(x)`                     | complementary error function |
|  [15]   | `SpecialFunctions.Logistic(x)`                 | logistic sigmoid             |
|  [16]   | `SpecialFunctions.Logit(p)`                    | logit                        |
|  [17]   | `SpecialFunctions.Harmonic(t)`                 | harmonic number              |
|  [18]   | `SpecialFunctions.Factorial(n)`                | factorial                    |
|  [19]   | `SpecialFunctions.FactorialLn(n)`              | log factorial                |
|  [20]   | `SpecialFunctions.BesselJ(nu, x)`              | Bessel J                     |
|  [21]   | `SpecialFunctions.BesselY(nu, x)`              | Bessel Y                     |
|  [22]   | `SpecialFunctions.BesselI0(x)`                 | modified Bessel I0           |
|  [23]   | `SpecialFunctions.BesselI1(x)`                 | modified Bessel I1           |
|  [24]   | `SpecialFunctions.BesselK0(x)`                 | modified Bessel K0           |
|  [25]   | `SpecialFunctions.BesselK1(x)`                 | modified Bessel K1           |

[ENTRYPOINT_SCOPE]: distance metrics via `Distance`

Every metric is a static reduction over a pair. A metric-dispatch delegate normalizes on arrays because the complete family shares only that carrier.

| [INDEX] | [SURFACE]                                                        | [METRIC]                |
| :-----: | :--------------------------------------------------------------- | :---------------------- |
|  [01]   | `Distance.Euclidean(a, b)`                                       | L2                      |
|  [02]   | `Distance.Manhattan(a, b)`                                       | L1                      |
|  [03]   | `Distance.Chebyshev(a, b)`                                       | L∞                      |
|  [04]   | `Distance.Minkowski(p, a, b)`                                    | order-`p`               |
|  [05]   | `Distance.Cosine(double[] a, double[] b)`                        | cosine                  |
|  [06]   | `Distance.Cosine(float[] a, float[] b)`                          | cosine                  |
|  [07]   | `Distance.Canberra(a, b)`                                        | weighted L1             |
|  [08]   | `Distance.Hamming(a, b)`                                         | differing-value count   |
|  [09]   | `Distance.Jaccard(a, b)`                                         | set overlap             |
|  [10]   | `Distance.SAD(a, b)`                                             | absolute sum            |
|  [11]   | `Distance.MAE(a, b)`                                             | mean absolute deviation |
|  [12]   | `Distance.SSD(a, b)`                                             | squared sum             |
|  [13]   | `Distance.MSE(a, b)`                                             | mean squared deviation  |
|  [14]   | `Distance.Pearson(IEnumerable<double> a, IEnumerable<double> b)` | correlation distance    |

[DISTANCE_OVERLOADS]:

- Vector and array carriers: `Euclidean`, `Manhattan`, `Chebyshev`, `Minkowski`, `SAD`, `MAE`, `SSD`, and `MSE`
- Array-only carriers: `Cosine`, `Canberra`, `Hamming`, and `Jaccard`
- Sequence carrier: `Pearson(IEnumerable<double>, IEnumerable<double>)`
- Array returns: Carrier scalar except `Jaccard`, which returns `double`
- Vector and sequence returns: `double`

[ENTRYPOINT_SCOPE]: least-squares fitting via `Fit`

[FIT_LINE]:

- Surface: `Fit.Line(double[] x, double[] y)`
- Returns: `(double A, double B)`
- Capability: Least-squares line `y = a + b·x`, with intercept `A` and slope `B`

[ENTRYPOINT_SCOPE]: discrete Fourier transforms via `Fourier`

Every transform mutates the caller-owned buffer. `FourierOptions` selects scaling and exponent convention, split real and imaginary arrays preserve contiguous scalar spans, real transforms pack the conjugate-even half-spectrum, and `FrequencyScale` returns the bin axis.

| [INDEX] | [SURFACE]                                                                               | [RETURNS]  | [CARRIER]        |
| :-----: | :-------------------------------------------------------------------------------------- | :--------- | :--------------- |
|  [01]   | `Fourier.Forward(Complex[] samples[, FourierOptions])`                                  | `void`     | complex          |
|  [02]   | `Fourier.Forward(Complex32[] samples[, FourierOptions])`                                | `void`     | complex32        |
|  [03]   | `Fourier.Forward(double[] real, double[] imaginary[, FourierOptions = Default])`        | `void`     | split double     |
|  [04]   | `Fourier.Forward(float[] real, float[] imaginary[, FourierOptions = Default])`          | `void`     | split float      |
|  [05]   | `Fourier.Inverse(Complex[] spectrum[, FourierOptions])`                                 | `void`     | complex          |
|  [06]   | `Fourier.Inverse(double[] real, double[] imaginary[, FourierOptions])`                  | `void`     | split double     |
|  [07]   | `Fourier.ForwardReal(double[] data, int n[, FourierOptions = Default])`                 | `void`     | packed double    |
|  [08]   | `Fourier.ForwardReal(float[] data, int n[, FourierOptions = Default])`                  | `void`     | packed float     |
|  [09]   | `Fourier.InverseReal(double[] data, int n[, FourierOptions = Default])`                 | `void`     | packed double    |
|  [10]   | `Fourier.InverseReal(float[] data, int n[, FourierOptions = Default])`                  | `void`     | packed float     |
|  [11]   | `Fourier.ForwardMultiDim(Complex[] samples, int[] dimensions[, FourierOptions])`        | `void`     | multidimensional |
|  [12]   | `Fourier.Forward2D(Complex[] samplesRowWise, int rows, int columns[, FourierOptions])`  | `void`     | row-major matrix |
|  [13]   | `Fourier.Forward2D(Matrix<Complex> samples[, FourierOptions])`                          | `void`     | complex matrix   |
|  [14]   | `Fourier.InverseMultiDim(Complex[] spectrum, int[] dimensions[, FourierOptions])`       | `void`     | multidimensional |
|  [15]   | `Fourier.Inverse2D(Complex[] spectrumRowWise, int rows, int columns[, FourierOptions])` | `void`     | row-major matrix |
|  [16]   | `Fourier.Inverse2D(Matrix<Complex> spectrum[, FourierOptions])`                         | `void`     | complex matrix   |
|  [17]   | `Fourier.FrequencyScale(int length, double sampleRate)`                                 | `double[]` | frequency axis   |

[ENTRYPOINT_SCOPE]: window tapers via `Window`

Every factory is static and returns `double[]`. Paired rows distinguish symmetric filter-design tapers from periodic FFT-framing tapers.

| [INDEX] | [SURFACE]                       | [TAPER]                     |
| :-----: | :------------------------------ | :-------------------------- |
|  [01]   | `Window.Hann(width)`            | symmetric Hann              |
|  [02]   | `Window.HannPeriodic(width)`    | periodic Hann               |
|  [03]   | `Window.Hamming(width)`         | symmetric Hamming           |
|  [04]   | `Window.HammingPeriodic(width)` | periodic Hamming            |
|  [05]   | `Window.Cosine(width)`          | symmetric cosine            |
|  [06]   | `Window.CosinePeriodic(width)`  | periodic cosine             |
|  [07]   | `Window.Lanczos(width)`         | symmetric Lanczos           |
|  [08]   | `Window.LanczosPeriodic(width)` | periodic Lanczos            |
|  [09]   | `Window.Blackman(width)`        | three-term Blackman         |
|  [10]   | `Window.BlackmanHarris(width)`  | four-term Blackman-Harris   |
|  [11]   | `Window.BlackmanNuttall(width)` | four-term Blackman-Nuttall  |
|  [12]   | `Window.Nuttall(width)`         | four-term Nuttall           |
|  [13]   | `Window.FlatTop(width)`         | five-term flat-top          |
|  [14]   | `Window.Bartlett(width)`        | zero-endpoint triangular    |
|  [15]   | `Window.BartlettHann(width)`    | Bartlett-Hann               |
|  [16]   | `Window.Triangular(width)`      | nonzero-endpoint triangular |
|  [17]   | `Window.Dirichlet(width)`       | rectangular                 |
|  [18]   | `Window.Gauss(width, sigma)`    | Gaussian                    |
|  [19]   | `Window.Tukey(width, r = 0.5)`  | tapered cosine              |

## [04]-[IMPLEMENTATION_LAW]

[DISTRIBUTION_TOPOLOGY]:

- namespace: `MathNet.Numerics.Distributions`
- seam hierarchy: `IDistribution` → `IUnivariateDistribution` → `IContinuousDistribution` or `IDiscreteDistribution`
- construction: each distribution has a constructor per parameter set plus an optional `System.Random randomSource` overload; passing `null` or omitting assigns `SystemRandomSource.Default`
- sampling: `Sample()` returns one value; `Samples(array)` fills the caller-owned buffer; `Samples()` returns a lazy `IEnumerable<T>` — none allocate an internal result store
- `RandomSource` replaces `null` with the shared default. `CumulativeDistribution` lives on `IUnivariateDistribution`, while inverse CDF remains a concrete-distribution surface.

[INTEGRATION_TOPOLOGY]:

- namespace: `MathNet.Numerics`; delegates to `MathNet.Numerics.Integration` (`DoubleExponentialTransformation`, `GaussLegendreRule`, `GaussKronrodRule`)
- `OnClosedInterval` is double-exponential with a `1e-8` default absolute-error target — suitable for smooth analytic functions; `GaussKronrod` is the adaptive method with an explicit relative-error target, `maximumDepth`, and an `out error`/`out L1Norm` estimate, for functions where the error must be bounded
- `OnRectangle`/`OnCuboid` use fixed-order Gauss-Legendre product rules over a box; the `order` argument selects the per-axis node count

[ROOT_FINDING_TOPOLOGY]:

- namespace: `MathNet.Numerics.RootFinding` — no `FindRoots` aggregator; the surface is one static class per method (`Brent`, `Bisection`, `NewtonRaphson`, `RobustNewtonRaphson`, `Secant`, `Broyden`, `Cubic`)
- selection by available information: `Brent.TryFindRoot` (bracket, no derivative) is the canonical entry; `NewtonRaphson`/`RobustNewtonRaphson` when an analytic derivative `df` is available; `Secant` for derivative-free local convergence; `Broyden` for nonlinear systems; `Cubic` for closed-form cubic roots
- `FindRoot` throws `NonConvergenceException` on failure, while `TryFindRoot` returns `false` for a `Fin` or `Option` rail. `FindRootExpand` adds automatic bracket expansion to bracketing methods.

[SPECTRAL_TOPOLOGY]:

- namespace: `MathNet.Numerics.IntegralTransforms` (`Fourier`, `FourierOptions`) plus root `MathNet.Numerics` (`Window`)
- Every `Fourier` transform mutates the caller-owned buffer. `Default` applies symmetric scaling, `AsymmetricScaling` scales the inverse by `1/N`, and `NoScaling` omits scaling; a forward-inverse round trip is identity under `Default` or `AsymmetricScaling` and carries a factor of `N` under `NoScaling`.
- three real-signal carriers: the `Complex[]`/`Complex32[]` form; the split `double[] real, double[] imaginary` form (a vectorized magnitude/phase reads the contiguous spans with no `Complex` marshalling — the preferred form); and the packed `ForwardReal`/`InverseReal` form that stores the conjugate-even half-spectrum in an `N+2` (even N) / `N+1` (odd N) buffer; `ForwardMultiDim`/`Forward2D` (and their inverses) cover row-major multi-dimensional and 2D data
- `FrequencyScale(length, sampleRate)` returns the per-bin frequency axis — the positive bins `0, Δf, 2Δf, …` over the first `⌊N/2⌋+1` then the wrapped negative bins, `Δf = sampleRate/length` — never the meaningless `1/length`
- `Hann`, `Hamming`, `Cosine`, and `Lanczos` pair symmetric filter-design tapers with `*Periodic` FFT-framing tapers. `Dirichlet` is rectangular, while `Gauss(width, sigma)` and `Tukey(width, r = 0.5)` carry shape parameters.

[STACK]:

- A root-find on a boundary rail composes the no-throw `Brent.TryFindRoot(f, lo, hi, acc, maxIter, out root)`: the `bool` maps to `Fin.Succ(root)`/`Fin.Fail(...)` (or `Option`) at the seam, so a non-convergence is a typed failure row, never a `NonConvergenceException` thrown through the receipt path
- Parallel sampling assigns each worker a distribution instance with its own `RandomSource`, so the RNG never races
- An `IInterpolation` composes `Interpolate(x)`, `Differentiate(x)`, and `Integrate(a, b)` through one scheme policy keyed by point order and pole constraints
- `SpecialFunctions.GammaLn`, `SpecialFunctions.BetaRegularized`, and `SpecialFunctions.Erf` anchor analytic CDFs
- A spectral pass widens the real signal to `double[]`, runs `Fourier.Forward(real, imaginary, FourierOptions.Default)`, and reads magnitude and phase through vectorized `Hypot` and `Atan2` operations over the contiguous spans
- `FourierOptions.Default` makes the forward-inverse round trip identity through symmetric scaling
- Bin spacing derives from `Fourier.FrequencyScale`
- The analysis window selects a `Window.*` taper, using `*Periodic` for FFT framing and the symmetric variant for filter design

[LOCAL_ADMISSION]:

- Parallel samplers own one distribution instance and `RandomSource` per worker
- `Integrate.OnClosedInterval` integrates a real scalar `Func<double,double>`; `GaussKronrod` is the adaptive form when an error bound is required; the two-/three-dimensional overloads are `OnRectangle`/`OnCuboid`
- Each root-finding method owns one static class, and `TryFindRoot` is the rail-composable failure form
- `MathNet.Numerics.IntegralTransforms.Fourier` owns in-place transforms under `FourierOptions`, with split scalar arrays for contiguous spans and packed real transforms for Hermitian half-spectra
- `MathNet.Numerics.Window` owns analysis tapers, and `FrequencyScale` owns the bin axis
- Wavelet filter banks bind through their scaling tables outside this package
- Linear algebra, provider selection, and sparse solve remain outside the core assembly

[RAIL_LAW]:

- Package: `MathNet.Numerics` (core assembly, non-provider namespaces)
- Owns: probability distributions, numerical integration (incl. adaptive Gauss-Kronrod and 2D/3D), root-finding (`Brent`/`Bisection`/`Newton`/`RobustNewton`/`Secant`/`Broyden`/`Cubic`), interpolation (cubic-spline family + polynomial + rational), special functions (Gamma/Beta/erf/Bessel), least-squares line fit (`Fit.Line`), the `Distance` metric catalog (Euclidean/Manhattan/Chebyshev/Minkowski over `Vector<T>`+arrays; Cosine/Canberra/Hamming/Jaccard array-only; Pearson), discrete Fourier transforms (in-place complex / split-real / packed-real / multi-dim + `FrequencyScale`), and the `Window` taper catalog
- Accept: `Func<double, double>` integrands and root targets, `IContinuousDistribution`/`IDiscreteDistribution` seams, `IInterpolation` results, the no-throw `TryFindRoot` rail form, in-place `Complex[]`/split `double[]` spectral buffers under a `FourierOptions` scaling, `Window` tapers as `double[]`
- Reject: hand-rolled distribution PDF/CDF, custom quadrature when `Integrate` covers the interval shape, a phantom `FindRoots` aggregator, local reimplementations of Gamma/Beta/erf/Bessel, a hand-rolled least-squares line beside `Fit.Line`, a hand-rolled pairwise-metric loop beside the `Distance` catalog, a phantom `Distance.Cosine(Vector<T>, Vector<T>)` overload (cosine is array-only), a hand-rolled radix-2/Bluestein FFT, a hand-rolled cosine/rectangular taper, `1/N` bin spacing where `Fourier.FrequencyScale` gives the real axis
