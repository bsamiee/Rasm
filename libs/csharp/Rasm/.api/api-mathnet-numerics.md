# [RASM_API_MATHNET_NUMERICS]

`MathNet.Numerics` supplies continuous and discrete probability distributions over
`IContinuousDistribution` / `IDiscreteDistribution`, numerical quadrature via
`Integrate`, root-finding via `FindRoots`, Floater-Hormann and other interpolation
schemes via `Interpolate`, and a broad `SpecialFunctions` catalog (Bessel, Gamma,
error function, etc.) for the numeric lane's statistical, analytical, and domain
computation paths. Linear algebra, provider selection, and sparse solve surfaces
are covered by `api-mathnet-providers.md`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics`
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.Distributions`, `MathNet.Numerics.Integration`, `MathNet.Numerics.Interpolation`, `MathNet.Numerics.RootFinding`
- asset: runtime library (managed; native providers are a separate concern)
- rail: numeric

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: distribution seams
- rail: numeric

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------------- |
|   [1]   | `IDistribution`           | interface     | root distribution seam; `Mean`, `Variance`, entropy                                    |
|   [2]   | `IUnivariateDistribution` | interface     | adds `CumulativeDistribution(x)`                                                       |
|   [3]   | `IContinuousDistribution` | interface     | adds `Density`, `DensityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples`         |
|   [4]   | `IDiscreteDistribution`   | interface     | adds `Probability`, `ProbabilityLn`, `Minimum`, `Maximum`, `Mode`, `Sample`, `Samples` |

[PUBLIC_TYPE_SCOPE]: continuous distributions
- rail: numeric

| [INDEX] | [SYMBOL]            | [PARAMETERS]               | [CAPABILITY]                         |
| :-----: | :------------------ | :------------------------- | :----------------------------------- |
|   [1]   | `Normal`            | `mean, stdDev`             | Gaussian; PDF/CDF/sample/inverse CDF |
|   [2]   | `LogNormal`         | `mu, sigma`                | log-normal                           |
|   [3]   | `Gamma`             | `shape, rate`              | Gamma                                |
|   [4]   | `Beta`              | `a, b`                     | Beta                                 |
|   [5]   | `ChiSquared`        | `freedom`                  | chi-squared                          |
|   [6]   | `StudentT`          | `location, scale, freedom` | Student's t                          |
|   [7]   | `Exponential`       | `rate`                     | exponential                          |
|   [8]   | `Weibull`           | `shape, scale`             | Weibull                              |
|   [9]   | `ContinuousUniform` | `lower, upper`             | continuous uniform                   |
|  [10]   | `Cauchy`            | `location, scale`          | Cauchy / Lorentz                     |
|  [11]   | `Laplace`           | `location, scale`          | Laplace                              |
|  [12]   | `Rayleigh`          | `scale`                    | Rayleigh                             |
|  [13]   | `FisherSnedecor`    | `d1, d2`                   | F-distribution                       |
|  [14]   | `Triangular`        | `lower, upper, mode`       | triangular                           |
|  [15]   | `Pareto`            | `scale, shape`             | Pareto                               |
|  [16]   | `InverseGamma`      | `shape, scale`             | inverse-Gamma                        |
|  [17]   | `BetaScaled`        | `a, b, lower, upper`       | scaled Beta                          |
|  [18]   | `Logistic`          | `mean, scale`              | logistic                             |

[PUBLIC_TYPE_SCOPE]: discrete distributions
- rail: numeric

| [INDEX] | [SYMBOL]           | [PARAMETERS]       | [CAPABILITY]              |
| :-----: | :----------------- | :----------------- | :------------------------ |
|   [1]   | `Binomial`         | `p, n`             | binomial                  |
|   [2]   | `Poisson`          | `lambda`           | Poisson                   |
|   [3]   | `NegativeBinomial` | `r, p`             | negative binomial         |
|   [4]   | `Bernoulli`        | `p`                | Bernoulli trial           |
|   [5]   | `DiscreteUniform`  | `lower, upper`     | discrete uniform          |
|   [6]   | `Geometric`        | `p`                | geometric                 |
|   [7]   | `Hypergeometric`   | `population, k, n` | hypergeometric            |
|   [8]   | `Categorical`      | `probabilities[]`  | categorical / multinoulli |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: distribution construction and sampling
- rail: numeric

| [INDEX] | [SURFACE]                                           | [CALL_SHAPE]  | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------- | :------------ | :--------------------------------------------------- |
|   [1]   | `new Normal(mean, stdDev)`                          | constructor   | Gaussian instance                                    |
|   [2]   | `new Normal(mean, stdDev, randomSource)`            | constructor   | Gaussian with explicit RNG                           |
|   [3]   | `IContinuousDistribution.Density(x)`                | instance      | PDF at x                                             |
|   [4]   | `IContinuousDistribution.DensityLn(x)`              | instance      | log-PDF at x                                         |
|   [5]   | `IUnivariateDistribution.CumulativeDistribution(x)` | instance      | CDF at x                                             |
|   [6]   | `IContinuousDistribution.Sample()`                  | instance      | single random sample                                 |
|   [7]   | `IContinuousDistribution.Samples(double[])`         | instance      | fill array with samples                              |
|   [8]   | `IContinuousDistribution.Samples()`                 | instance      | lazy infinite sample sequence                        |
|   [9]   | `IDiscreteDistribution.Probability(k)`              | instance      | PMF at k                                             |
|  [10]   | `IDiscreteDistribution.ProbabilityLn(k)`            | instance      | log-PMF at k                                         |
|  [11]   | `IDiscreteDistribution.Sample()`                    | instance      | single integer sample                                |
|  [12]   | `Normal.RandomSource`                               | instance prop | gets or sets the RNG (replaces with default on null) |

[ENTRYPOINT_SCOPE]: integration (`Integrate` class)
- rail: numeric

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------- | :------------ | :---------------------------------------------- |
|   [1]   | `Integrate.OnClosedInterval(f, a, b, targetError)` | static method | double-exponential quadrature with error target |
|   [2]   | `Integrate.OnClosedInterval(f, a, b)`              | static method | double-exponential quadrature (default 1e-8)    |
|   [3]   | `Integrate.OnRectangle(f2d, a, b, c, d, order)`    | static method | 2D Gauss-Legendre over `[a,b]×[c,d]`            |

[ENTRYPOINT_SCOPE]: root-finding (`FindRoots` class)
- rail: numeric

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------- | :------------ | :--------------------------------------- |
|   [1]   | `FindRoots.OfFunction(f, lower, upper, accuracy, maxIter)`     | static method | Brent + Bisection with bracket expansion |
|   [2]   | `FindRoots.OfFunction(f, df, lower, upper, accuracy, maxIter)` | static method | Newton-Raphson with derivative           |

[ENTRYPOINT_SCOPE]: interpolation (`Interpolate` class)
- rail: numeric

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------ |
|   [1]   | `Interpolate.Common(points, values)`               | static method | Floater-Hormann rational; arbitrary unsorted data |
|   [2]   | `Interpolate.RationalWithoutPoles(points, values)` | static method | Floater-Hormann rational; pole-free               |

[ENTRYPOINT_SCOPE]: special functions (`SpecialFunctions` class)
- rail: numeric

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------- |
|   [1]   | `SpecialFunctions.Gamma(x)`                    | static method | Gamma function Γ(x)                  |
|   [2]   | `SpecialFunctions.GammaLn(x)`                  | static method | log-Gamma ln Γ(x)                    |
|   [3]   | `SpecialFunctions.GammaLowerRegularized(a, x)` | static method | regularized lower incomplete gamma   |
|   [4]   | `SpecialFunctions.GammaUpperRegularized(a, x)` | static method | regularized upper incomplete gamma   |
|   [5]   | `SpecialFunctions.Beta(a, b)`                  | static method | Beta function B(a,b)                 |
|   [6]   | `SpecialFunctions.BetaRegularized(a, b, x)`    | static method | regularized incomplete beta I_x(a,b) |
|   [7]   | `SpecialFunctions.Erf(x)`                      | static method | error function erf(x)                |
|   [8]   | `SpecialFunctions.ErfInverse(x)`               | static method | inverse error function               |
|   [9]   | `SpecialFunctions.Erfc(x)`                     | static method | complementary error function erfc(x) |
|  [10]   | `SpecialFunctions.Logistic(x)`                 | static method | logistic sigmoid 1/(1+e^-x)          |
|  [11]   | `SpecialFunctions.LogisticLn(x)`               | static method | log-logistic                         |
|  [12]   | `SpecialFunctions.Harmonic(t)`                 | static method | harmonic number H(t)                 |
|  [13]   | `SpecialFunctions.BesselJ0(x)`                 | static method | Bessel J0                            |
|  [14]   | `SpecialFunctions.BesselJ1(x)`                 | static method | Bessel J1                            |

## [4]-[IMPLEMENTATION_LAW]

[DISTRIBUTION_TOPOLOGY]:
- namespace: `MathNet.Numerics.Distributions`
- seam hierarchy: `IDistribution` → `IUnivariateDistribution` → `IContinuousDistribution` or `IDiscreteDistribution`
- construction: each distribution class has a constructor per parameter set plus an optional `System.Random randomSource` overload; passing `null` or omitting assigns `SystemRandomSource.Default`
- sampling: `Sample()` returns a single value; `Samples(array)` fills the caller-owned buffer; `Samples()` returns a lazy `IEnumerable<T>` — none allocate an internal result store
- `RandomSource` is a mutable property; setting it to `null` replaces with the shared default, never throws

[INTEGRATION_TOPOLOGY]:
- namespace: `MathNet.Numerics`; delegates to `MathNet.Numerics.Integration.DoubleExponentialTransformation` and `GaussLegendreRule`
- `OnClosedInterval` default error target: `1e-8`; suitable for smooth analytic functions on bounded intervals
- `OnRectangle` uses precomputed Gauss-Legendre rules for orders 2–20, 32, 64, 96, 100, 128, 256, 512, 1024; rules outside this set are computed on the fly

[ROOT_FINDING_TOPOLOGY]:
- namespace: `MathNet.Numerics`; delegates to `MathNet.Numerics.RootFinding` (Brent, Bisection, Newton)
- `FindRoots.OfFunction` without derivative: bracket expansion via `ZeroCrossingBracketing`, then Brent, then Bisection fallback; throws `NonConvergenceException` on failure
- `FindRoots.OfFunction` with derivative: Newton-Raphson guarded by bisection fallback

[LOCAL_ADMISSION]:
- Distributions are stateful through `RandomSource`; treat them as owned values, not static singletons, when parallel sampling is required.
- `IContinuousDistribution.CumulativeDistribution` is on the `IUnivariateDistribution` interface; cast to `IUnivariateDistribution` or the concrete type to call it from C#.
- `Integrate.OnClosedInterval` integrates a real scalar `Func<double,double>`; the two-dimensional overload is `OnRectangle`.
- Linear algebra, provider selection, and sparse solve surfaces are in `api-mathnet-providers.md` and are not duplicated here.

[RAIL_LAW]:
- Package: `MathNet.Numerics` (core assembly, non-provider namespaces)
- Owns: probability distributions, numerical integration, root-finding, interpolation, special functions
- Accept: `Func<double, double>` integrands, `IContinuousDistribution` / `IDiscreteDistribution` seams, `IInterpolation` results
- Reject: hand-rolled distribution PDF/CDF, custom quadrature when `Integrate` covers the interval shape, local reimplementations of Gamma/Beta/Erf
