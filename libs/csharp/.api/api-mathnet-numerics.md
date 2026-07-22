# [RASM_API_MATHNET_NUMERICS]

`MathNet.Numerics` owns the branch's analytic numeric kernel — probability, quadrature, interpolation, root finding, nonlinear least squares, special functions, spectral transform, and metric reduction — each a static owner folding plain `double[]`, `Func`, and `Complex[]` carriers. Every surface here runs on the managed provider; native-kernel selection and the dense factorization lane bind at their own owners, and `Vector<double>` enters only as the minimizer's carrier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (MIT, Math.NET Project)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `.Distributions`, `.Integration`, `.IntegralTransforms`, `.Interpolation`, `.RootFinding`, `.Optimization`, `.Differentiation`, `.Statistics`, `.OdeSolvers`, `.Random`, `.LinearAlgebra`
- asset: managed runtime library; MKL and OpenBLAS kernels ride sibling provider packages
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: distribution seams and the univariate roster under its constructor parameterization

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `IDistribution`                                    | interface     | `RandomSource` ownership every draw reads |
|  [02]   | `IUnivariateDistribution`                          | interface     | moments and `CumulativeDistribution`      |
|  [03]   | `IContinuousDistribution`                          | interface     | density, mode, and continuous sampling    |
|  [04]   | `IDiscreteDistribution`                            | interface     | mass, mode, and integer sampling          |
|  [05]   | `Normal(mean, stddev)`                             | class         | Gaussian                                  |
|  [06]   | `LogNormal(mu, sigma)`                             | class         | log-Gaussian                              |
|  [07]   | `Gamma(shape, rate)`                               | class         | Gamma, rate-parameterized                 |
|  [08]   | `InverseGamma(shape, scale)`                       | class         | inverse Gamma                             |
|  [09]   | `Erlang(shape, rate)`                              | class         | integer-shape Gamma                       |
|  [10]   | `Beta(a, b)`                                       | class         | unit-interval Beta                        |
|  [11]   | `BetaScaled(a, b, location, scale)`                | class         | affine-mapped Beta                        |
|  [12]   | `ChiSquared(freedom)`                              | class         | chi-squared                               |
|  [13]   | `Chi(freedom)`                                     | class         | chi                                       |
|  [14]   | `StudentT(location, scale, freedom)`               | class         | Student t                                 |
|  [15]   | `FisherSnedecor(d1, d2)`                           | class         | F ratio                                   |
|  [16]   | `Exponential(rate)`                                | class         | exponential                               |
|  [17]   | `Weibull(shape, scale)`                            | class         | Weibull                                   |
|  [18]   | `Rayleigh(scale)`                                  | class         | Rayleigh                                  |
|  [19]   | `Pareto(scale, shape)`                             | class         | Pareto                                    |
|  [20]   | `Cauchy(location, scale)`                          | class         | Cauchy                                    |
|  [21]   | `Laplace(location, scale)`                         | class         | Laplace                                   |
|  [22]   | `Logistic(mean, scale)`                            | class         | logistic                                  |
|  [23]   | `Stable(alpha, beta, scale, location)`             | class         | stable, heavy-tailed                      |
|  [24]   | `SkewedGeneralizedT(location, scale, skew, p, q)`  | class         | skew and kurtosis-tunable t               |
|  [25]   | `SkewedGeneralizedError(location, scale, skew, p)` | class         | skew-tunable error                        |
|  [26]   | `Triangular(lower, upper, mode)`                   | class         | triangular                                |
|  [27]   | `ContinuousUniform(lower, upper)`                  | class         | continuous uniform                        |
|  [28]   | `Bernoulli(p)`                                     | class         | single trial                              |
|  [29]   | `Binomial(p, n)`                                   | class         | successes in a trial count                |
|  [30]   | `BetaBinomial(n, a, b)`                            | class         | over-dispersed binomial                   |
|  [31]   | `NegativeBinomial(r, p)`                           | class         | failures before a success count           |
|  [32]   | `Geometric(p)`                                     | class         | trials to first success                   |
|  [33]   | `Poisson(lambda)`                                  | class         | Poisson count                             |
|  [34]   | `ConwayMaxwellPoisson(lambda, nu)`                 | class         | dispersion-tuned Poisson                  |
|  [35]   | `Hypergeometric(population, success, draws)`       | class         | draws without replacement                 |
|  [36]   | `DiscreteUniform(lower, upper)`                    | class         | discrete uniform                          |
|  [37]   | `Categorical(probabilityMass)`                     | class         | arbitrary mass vector                     |
|  [38]   | `Zipf(s, n)`                                       | class         | power-law rank                            |

[MULTIVARIATE]: `Dirichlet` `Multinomial` `NormalGamma` `MeanPrecisionPair` `MatrixNormal` `Wishart` `InverseWishart`

[PUBLIC_TYPE_SCOPE]: analytic, solver, and spectral carriers the entrypoints take and return

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :---------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `IInterpolation`              | interface      | fitted curve: evaluate, differentiate twice, integrate |
|  [02]   | `Polynomial`                  | class          | dense polynomial algebra and root extraction           |
|  [03]   | `Complex32`                   | struct         | single-precision complex carrier                       |
|  [04]   | `FourierOptions`              | enum           | transform scaling and exponent convention              |
|  [05]   | `HartleyOptions`              | enum           | Hartley scaling convention                             |
|  [06]   | `SplineBoundaryCondition`     | enum           | cubic-spline end condition                             |
|  [07]   | `StepType`                    | enum           | finite-difference step policy                          |
|  [08]   | `DirectRegressionMethod`      | enum           | normal-equations or QR route every `Fit` member takes  |
|  [09]   | `IObjectiveModel`             | interface      | residual model the least-squares minimizer folds       |
|  [10]   | `IObjectiveFunction`          | interface      | value, gradient, and Hessian objective                 |
|  [11]   | `NonlinearMinimizationResult` | class          | minimizing point, covariance, and exit condition       |
|  [12]   | `MinimizationResult`          | class          | unconstrained-minimizer point and exit condition       |
|  [13]   | `ExitCondition`               | enum           | minimizer stop reason                                  |
|  [14]   | `NonConvergenceException`     | class          | failure the throwing iterative forms raise             |
|  [15]   | `RandomSource`                | abstract class | seeded generator base every distribution binds         |
|  [16]   | `NumericalDerivative`         | class          | reusable finite-difference derivative engine           |
|  [17]   | `Matrix<T>`                   | abstract class | dense and sparse algebra carrier                       |
|  [18]   | `Vector<T>`                   | abstract class | vector carrier the minimizer takes and returns         |

[RANDOM_SOURCE]: `SystemRandomSource` `MersenneTwister` `Xoshiro256StarStar` `Mrg32k3a` `Mcg31m1` `Mcg59` `Palf` `WH1982` `WH2006` `Xorshift` `CryptoRandomSource`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: distribution evaluation, sampling, and parameter admission

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `IContinuousDistribution.Density(double) -> double`        | instance | probability density                          |
|  [02]   | `IContinuousDistribution.DensityLn(double) -> double`      | instance | log density                                  |
|  [03]   | `IDiscreteDistribution.Probability(int) -> double`         | instance | probability mass                             |
|  [04]   | `IDiscreteDistribution.ProbabilityLn(int) -> double`       | instance | log probability mass                         |
|  [05]   | `IDiscreteDistribution.Sample() -> int`                    | instance | one integer draw                             |
|  [06]   | `IUnivariateDistribution.CumulativeDistribution(double)`   | instance | cumulative probability                       |
|  [07]   | `Normal.InverseCumulativeDistribution(double) -> double`   | instance | quantile, per concrete distribution          |
|  [08]   | `IContinuousDistribution.Sample() -> double`               | instance | one draw                                     |
|  [09]   | `IContinuousDistribution.Samples(double[])`                | instance | fill a caller-owned buffer                   |
|  [10]   | `IContinuousDistribution.Samples() -> IEnumerable<double>` | instance | lazy unbounded draw stream                   |
|  [11]   | `IDistribution.RandomSource`                               | property | swap the generator on a standing instance    |
|  [12]   | `Normal.WithMeanVariance(double, double, Random)`          | factory  | admit an alternate parameterization          |
|  [13]   | `Gamma.WithShapeScale(double, double, Random)`             | factory  | scale form beside the rate constructor       |
|  [14]   | `Normal.Estimate(IEnumerable<double>, Random)`             | factory  | maximum-likelihood fit from samples          |
|  [15]   | `Normal.CDF(double, double, double) -> double`             | static   | evaluation at a parameter tuple, no instance |

[STATIC_FAMILY]: `PDF` `PDFLn` `PMF` `PMFLn` `CDF` `InvCDF` `Sample` `Samples` `IsValidParameterSet`
[MOMENTS]: `Mean` `Variance` `StdDev` `Entropy` `Skewness` `Median` `Mode` `Minimum` `Maximum`

[ENTRYPOINT_SCOPE]: sequence and signal generation via `Generate`

[AXIS]: `LinearSpaced` `LogSpaced` `LinearRange` `LinearRangeInt32` `Periodic` `Sinusoidal` `Square` `Triangle` `Sawtooth` `Step` `Impulse` `PeriodicImpulse` `Repeat` `Unfold` `Fibonacci` `Map` `Map2`

[ENTRYPOINT_SCOPE]: quadrature via `Integrate`

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `Integrate.OnClosedInterval(F1, double, double)`                              | static  | double-exponential at a `1e-8` absolute target |
|  [02]   | `Integrate.OnClosedInterval(F1, double, double, double)`                      | static  | the same rule at a caller error target         |
|  [03]   | `Integrate.DoubleExponential(F1, double, double, double)`                     | static  | the transformation named directly              |
|  [04]   | `Integrate.GaussLegendre(F1, double, double, int)`                            | static  | fixed-order Legendre rule                      |
|  [05]   | `Integrate.GaussKronrod(F1, double, double, double, int, int)`                | static  | adaptive rule at a relative-error target       |
|  [06]   | `Integrate.GaussKronrod(F1, double, double, out double, out double)`          | static  | adds an error and L1-norm estimate             |
|  [07]   | `Integrate.OnRectangle(F2, double, double, double, double)`                   | static  | 2-D Legendre product rule                      |
|  [08]   | `Integrate.OnRectangle(F2, double, double, double, double, int)`              | static  | the same rule at a caller node order           |
|  [09]   | `Integrate.OnCuboid(F3, double, double, double, double, double, double, int)` | static  | 3-D Legendre product rule                      |

- `F1`, `F2`, and `F3` abbreviate `Func<double,double>`, `Func<double,double,double>`, and `Func<double,double,double,double>`; every surface returns `double`.

[ENTRYPOINT_SCOPE]: root finding via `MathNet.Numerics.RootFinding`

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `Brent.TryFindRoot(F1, double, double, double, int, out double) -> bool`       | static  | bracketed inverse-quadratic solve, no throw  |
|  [02]   | `Brent.FindRoot(F1, double, double, double, int) -> double`                    | static  | the same solve raising on non-convergence    |
|  [03]   | `Brent.FindRootExpand(F1, double, double, double, int, double, int)`           | static  | grow the bracket onto a sign change          |
|  [04]   | `Bisection.FindRoot(F1, double, double, double, int) -> double`                | static  | bisection on a guaranteed bracket            |
|  [05]   | `NewtonRaphson.FindRoot(F1, F1, double, double, double, int) -> double`        | static  | derivative-driven solve inside a bracket     |
|  [06]   | `NewtonRaphson.FindRootNearGuess(F1, F1, double, double, double, double, int)` | static  | the same solve seeded from one guess         |
|  [07]   | `RobustNewtonRaphson.FindRoot(F1, F1, double, double, double, int, int)`       | static  | subdivided Newton recovering from a bad step |
|  [08]   | `Secant.FindRoot(F1, double, double, double, double, double, int)`             | static  | derivative-free two-point solve              |
|  [09]   | `Broyden.FindRoot(Func<double[],double[]>, double[], double, int, double)`     | static  | quasi-Newton solve of a square system        |
|  [10]   | `Cubic.RealRoots(double, double, double) -> (double, double, double)`          | static  | closed-form real cubic roots                 |
|  [11]   | `Cubic.Roots(double, double, double, double) -> (Complex, Complex, Complex)`   | static  | closed-form complex cubic roots              |

[ENTRYPOINT_SCOPE]: nonlinear least squares and unconstrained minimization via `MathNet.Numerics.Optimization`

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `LevenbergMarquardtMinimizer(double, double, double, double, int)`                 | ctor     | damping and the four tolerances      |
|  [02]   | `LevenbergMarquardtMinimizer.FindMinimum(IObjectiveModel, V, V, V, V, List<bool>)` | instance | bounded, scaled, partially-fixed fit |
|  [03]   | `LevenbergMarquardtMinimizer.Minimum(IObjectiveModel, V, V, V, V, List<bool>)`     | static   | one-call solve holding no minimizer  |
|  [04]   | `ObjectiveFunction.NonlinearModel(Func<V,V,V>, V, V, V, int)`                      | static   | residual model, differenced Jacobian |
|  [05]   | `ObjectiveFunction.NonlinearModel(Func<V,V,V>, Func<V,V,M>, V, V, V)`              | static   | the same model, analytic Jacobian    |
|  [06]   | `ObjectiveFunction.Value(Func<V,double>) -> IObjectiveFunction`                    | static   | value-only objective                 |
|  [07]   | `ObjectiveFunction.Gradient(Func<V,double>, Func<V,V>)`                            | static   | value with an analytic gradient      |
|  [08]   | `ObjectiveFunction.GradientHessian(Func<V,double>, Func<V,V>, Func<V,M>)`          | static   | value, gradient, and Hessian         |
|  [09]   | `BfgsMinimizer.FindMinimum(IObjectiveFunction, V)`                                 | instance | quasi-Newton unconstrained minimum   |
|  [10]   | `BfgsBMinimizer.FindMinimum(IObjectiveFunction, V, V, V)`                          | instance | box-bounded quasi-Newton minimum     |
|  [11]   | `LimitedMemoryBfgsMinimizer.FindMinimum(IObjectiveFunction, V)`                    | instance | limited-memory BFGS for wide fits    |
|  [12]   | `NelderMeadSimplex.Minimum(IObjectiveFunction, V, double, int)`                    | static   | derivative-free simplex search       |
|  [13]   | `NonlinearMinimizationResult.MinimizingPoint -> V`                                 | property | fitted parameter vector              |
|  [14]   | `NonlinearMinimizationResult.Covariance -> M`                                      | property | parameter covariance at the minimum  |
|  [15]   | `NonlinearMinimizationResult.StandardErrors -> V`                                  | property | per-parameter standard error         |
|  [16]   | `NonlinearMinimizationResult.ReasonForExit -> ExitCondition`                       | property | stop condition the run hit           |
|  [17]   | `CreateVector.DenseOfArray<double>(double[]) -> V`                                 | static   | admit a `double[]` as the carrier    |

- `V` and `M` abbreviate `Vector<double>` and `Matrix<double>`; `LevenbergMarquardtMinimizer.FindMinimum` mirrors its `V` arguments with a `double[]`/`bool[]` overload.

[ENTRYPOINT_SCOPE]: interpolation via `Interpolate`, every factory taking `IEnumerable<double>` points and values

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `Interpolate.CubicSpline(points, values)`                                  | factory | natural cubic spline                     |
|  [02]   | `Interpolate.CubicSplineRobust(points, values)`                            | factory | Akima cubic, outlier-tolerant            |
|  [03]   | `Interpolate.CubicSplineMonotone(points, values)`                          | factory | PCHIP monotone cubic                     |
|  [04]   | `Interpolate.CubicSplineWithDerivatives(points, values, firstDerivatives)` | factory | Hermite cubic at prescribed slopes       |
|  [05]   | `Interpolate.Common(points, values)`                                       | factory | Floater-Hormann barycentric rational     |
|  [06]   | `Interpolate.RationalWithoutPoles(points, values)`                         | factory | the same pole-free rational scheme       |
|  [07]   | `Interpolate.RationalWithPoles(points, values)`                            | factory | Bulirsch-Stoer rational, poles admitted  |
|  [08]   | `Interpolate.Polynomial(points, values)`                                   | factory | Neville polynomial                       |
|  [09]   | `Interpolate.PolynomialEquidistant(points, values)`                        | factory | barycentric polynomial on a uniform grid |
|  [10]   | `Interpolate.Linear(points, values)`                                       | factory | piecewise linear spline                  |
|  [11]   | `Interpolate.LogLinear(points, values)`                                    | factory | log-linear spline                        |
|  [12]   | `Interpolate.Step(points, values)`                                         | factory | piecewise constant                       |

[INTERPOLATION_SEAM]: `Interpolate` `Differentiate` `Differentiate2` `Integrate` `SupportsDifferentiation` `SupportsIntegration`

[ENTRYPOINT_SCOPE]: regression and curve fitting via `Fit`, every surface static over `double[]` samples

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `Fit.Line(double[], double[]) -> (double A, double B)`                       | static  | least-squares intercept and slope               |
|  [02]   | `Fit.LineThroughOrigin(double[], double[]) -> double`                        | static  | slope with the intercept pinned to zero         |
|  [03]   | `Fit.Polynomial(double[], double[], int, DirectRegressionMethod)`            | static  | polynomial coefficients at an order             |
|  [04]   | `Fit.PolynomialWeighted(double[], double[], double[], int) -> double[]`      | static  | the same fit under per-sample weights           |
|  [05]   | `Fit.Exponential(double[], double[], DirectRegressionMethod)`                | static  | `a·e^(r·x)` fit                                 |
|  [06]   | `Fit.Logarithm(double[], double[], DirectRegressionMethod)`                  | static  | `a + b·ln(x)` fit                               |
|  [07]   | `Fit.Power(double[], double[], DirectRegressionMethod)`                      | static  | `a·x^b` fit                                     |
|  [08]   | `Fit.LinearCombination(double[], double[], params Func<double,double>[])`    | static  | arbitrary basis-function fit                    |
|  [09]   | `Fit.MultiDim(double[][], double[], bool, DirectRegressionMethod)`           | static  | multivariate linear fit, intercept optional     |
|  [10]   | `Fit.MultiDimWeighted(double[][], double[], double[]) -> double[]`           | static  | the same fit under per-sample weights           |
|  [11]   | `Fit.LinearGeneric<T>(T[], double[], params Func<T,double>[]) -> double[]`   | static  | basis fit over an arbitrary sample carrier      |
|  [12]   | `Fit.Curve(double[], double[], Func<double,double,double>, double)`          | static  | nonlinear curve fit, one fitted parameter       |
|  [13]   | `GoodnessOfFit.RSquared(IEnumerable<double>, IEnumerable<double>)`           | static  | coefficient of determination over a fit         |
|  [14]   | `GoodnessOfFit.StandardError(IEnumerable<double>, IEnumerable<double>, int)` | static  | residual standard error at a degrees-of-freedom |

[ENTRYPOINT_SCOPE]: special functions via `SpecialFunctions`, every surface static

[GAMMA]: `Gamma` `GammaLn` `GammaLowerIncomplete` `GammaUpperIncomplete` `GammaLowerRegularized` `GammaUpperRegularized` `GammaLowerRegularizedInv` `DiGamma` `DiGammaInv`
[BETA]: `Beta` `BetaLn` `BetaIncomplete` `BetaRegularized`
[ERROR_FUNCTION]: `Erf` `Erfc` `ErfInv` `ErfcInv`
[COMBINATORIC]: `Factorial` `FactorialLn` `Binomial` `BinomialLn` `Multinomial` `RisingFactorial` `FallingFactorial` `Harmonic` `GeneralHarmonic`
[BESSEL]: `BesselJ` `BesselY` `BesselI` `BesselK` `BesselI0` `BesselI1` `BesselK0` `BesselK1` `SphericalBesselJ` `SphericalBesselY` `HankelH1` `HankelH2`
[AIRY_KELVIN_STRUVE]: `AiryAi` `AiryBi` `AiryAiPrime` `AiryBiPrime` `KelvinBer` `KelvinBei` `KelvinKer` `KelvinKei` `StruveL0` `StruveL1`
[ELEMENTARY]: `Logistic` `Logit` `Log1p` `Expm1` `Hypotenuse` `ExponentialIntegral` `MarcumQ` `MittagLefflerE` `GeneralizedHypergeometric`

[ENTRYPOINT_SCOPE]: distance reduction via `Distance`, every surface a static reduction over one pair

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Distance.Euclidean(Vector<T>, Vector<T>) -> double`         | static  | L2                                |
|  [02]   | `Distance.Manhattan(Vector<T>, Vector<T>) -> double`         | static  | L1                                |
|  [03]   | `Distance.Chebyshev(Vector<T>, Vector<T>) -> double`         | static  | L∞                                |
|  [04]   | `Distance.Minkowski(double, Vector<T>, Vector<T>) -> double` | static  | order-`p`                         |
|  [05]   | `Distance.SAD(Vector<T>, Vector<T>) -> double`               | static  | sum of absolute deviations        |
|  [06]   | `Distance.MAE(Vector<T>, Vector<T>) -> double`               | static  | mean absolute deviation           |
|  [07]   | `Distance.SSD(Vector<T>, Vector<T>) -> double`               | static  | sum of squared deviations         |
|  [08]   | `Distance.MSE(Vector<T>, Vector<T>) -> double`               | static  | mean squared deviation            |
|  [09]   | `Distance.Cosine(double[], double[]) -> double`              | static  | cosine, array carrier only        |
|  [10]   | `Distance.Canberra(double[], double[]) -> double`            | static  | weighted L1, array carrier only   |
|  [11]   | `Distance.Hamming(double[], double[]) -> double`             | static  | differing-value count, array only |
|  [12]   | `Distance.Jaccard(double[], double[]) -> double`             | static  | set overlap, array only           |
|  [13]   | `Distance.Pearson(IEnumerable<double>, IEnumerable<double>)` | static  | correlation distance              |

- Each array metric mirrors a `float[]` overload returning `float`, `Jaccard` excepted; the `Vector<T>` form constrains `T : struct, IEquatable<T>, IFormattable`.

[ENTRYPOINT_SCOPE]: integral transforms via `Fourier` and `Hartley`

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Fourier.Forward(Complex[], FourierOptions)`                 | static  | in-place complex transform          |
|  [02]   | `Fourier.Forward(double[], double[], FourierOptions)`        | static  | split real and imaginary spans      |
|  [03]   | `Fourier.ForwardReal(double[], int, FourierOptions)`         | static  | packed conjugate-even half-spectrum |
|  [04]   | `Fourier.ForwardMultiDim(Complex[], int[], FourierOptions)`  | static  | row-major N-dimensional transform   |
|  [05]   | `Fourier.Forward2D(Complex[], int, int, FourierOptions)`     | static  | row-major matrix transform          |
|  [06]   | `Fourier.Forward2D(Matrix<Complex>, FourierOptions)`         | static  | `Matrix<T>` carrier transform       |
|  [07]   | `Fourier.FrequencyScale(int, double) -> double[]`            | static  | per-bin frequency axis              |
|  [08]   | `Hartley.NaiveForward(double[], HartleyOptions) -> double[]` | static  | real-valued Hartley transform       |

[ENTRYPOINT_SCOPE]: window tapers via `Window`, every factory returning a `double[]` of the requested width

[PAIRED_TAPER]: `Hann` `Hamming` `Cosine` `Lanczos`
[SINGLE_TAPER]: `Blackman` `BlackmanHarris` `BlackmanNuttall` `Nuttall` `FlatTop` `Bartlett` `BartlettHann` `Triangular` `Dirichlet`
[SHAPED_TAPER]: `Gauss(width, sigma)` `Tukey(width, r)`

[ENTRYPOINT_SCOPE]: statistics, differentiation, and the numeric-utility owners

[STATISTICS]: `Mean` `Variance` `StandardDeviation` `PopulationVariance` `Covariance` `Skewness` `Kurtosis` `Median` `Quantile` `QuantileCustom` `Percentile` `InterquartileRange` `FiveNumberSummary` `Ranks` `QuantileRank` `EmpiricalCDF` `EmpiricalInvCDF` `RootMeanSquare` `GeometricMean` `HarmonicMean` `Entropy` `MovingAverage` `OrderStatistic`
[STATISTICS_OWNER]: `Statistics` `ArrayStatistics` `SortedArrayStatistics` `StreamingStatistics` `DescriptiveStatistics` `WeightedDescriptiveStatistics` `RunningStatistics` `RunningWeightedStatistics` `MovingStatistics` `Correlation` `Histogram` `KernelDensity` `GoodnessOfFit`
[CORRELATION]: `Pearson` `WeightedPearson` `Spearman` `PearsonMatrix` `SpearmanMatrix` `Auto`
[DIFFERENTIATE]: `FirstDerivative` `SecondDerivative` `Derivative` `PartialDerivative` `FirstPartialDerivative` `PartialDerivative2` `Points` `Order`
[PRECISION]: `AlmostEqual` `AlmostEqualRelative` `AlmostEqualNumbersBetween` `CoerceZero` `EpsilonOf` `Increment` `Decrement` `Round` `RoundToMultiple` `RoundToPower` `Magnitude` `NumbersBetween`
[EUCLID]: `GreatestCommonDivisor` `ExtendedGreatestCommonDivisor` `LeastCommonMultiple` `Modulus` `Remainder` `IsEven` `IsOdd` `IsPowerOfTwo` `CeilingToPowerOfTwo` `PowerOfTwo` `Log2` `IsPerfectSquare`
[TRIG]: `Sinc` `Cot` `Sec` `Csc` `Acot` `Asec` `Acsc` `Sinh` `Cosh` `Tanh` `Asinh` `Acosh` `Atanh` `DegreeToRadian` `RadianToDegree` `DegreeToGrad` `GradToRadian`
[COMBINATORICS]: `Combinations` `CombinationsWithRepetition` `Permutations` `Variations` `VariationsWithRepetition` `GeneratePermutation` `GenerateCombination` `GenerateVariation` `SelectPermutation` `SelectCombination` `SelectVariation`
[POLYNOMIAL]: `Evaluate` `Fit` `Roots` `Differentiate` `Integrate` `Add` `Subtract` `Multiply` `Divide` `PointwiseMultiply` `PointwiseDivide` `EigenvalueMatrix`
[ODE]: `RungeKutta.SecondOrder` `RungeKutta.FourthOrder`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IDistribution` to `IUnivariateDistribution` to `IContinuousDistribution` or `IDiscreteDistribution` is the seam ladder: `CumulativeDistribution` rides the univariate seam and `InverseCumulativeDistribution` stays a concrete-distribution member.
- Every distribution mints a parallel static family keyed on its constructor tuple, so a one-shot evaluation allocates no instance; an alternate parameterization arrives as a `With*` factory rather than a second constructor, and `Gamma`'s constructor takes a rate with `WithShapeScale` holding the scale form.
- Omitting `System.Random` binds `SystemRandomSource.Default`; `Sample()` returns one value, `Samples(array)` fills the caller-owned buffer, and `Samples()` yields lazily, none allocating an internal store.
- `Generate.*Map` fuses the projection into the axis walk and `Generate.*Sequence` yields it lazily, so neither materializes an intermediate array.
- `OnClosedInterval` is the double-exponential rule at a `1e-8` absolute target; `GaussKronrod` is the adaptive rule whose `out error`/`out L1Norm` overload is the only quadrature carrying its own error estimate; `OnRectangle` and `OnCuboid` are fixed-order Legendre product rules whose `order` sets the per-axis node count.
- Root finding is one static class per method with no aggregator: every iterative class mirrors `FindRoot` with `TryFindRoot`, whose `bool` return carries non-convergence where `FindRoot` raises `NonConvergenceException`.
- `Broyden.FindRoot` solves a square system carrying no bounds; a rectangular residual or a per-parameter travel limit routes to `LevenbergMarquardtMinimizer.FindMinimum`, which takes the full residual and the bound vectors natively.
- Least squares fits `f(parameters, observedX) -> predicted` against `observedY`, so a pure residual formulation passes a zero `observedY` of the residual's rank and `MinimizingPoint` minimizes the residual norm; `ReasonForExit` reports the stop condition while `Covariance` and `StandardErrors` carry the fit's uncertainty.
- Every `Fourier` transform mutates the caller-owned buffer: `Default` applies symmetric scaling, `AsymmetricScaling` scales the inverse by `1/N`, and `NoScaling` omits it, so a forward-inverse round trip is identity under the first two and carries a factor of `N` under the third.
- `Forward*` and `Inverse*` mirror at one signature across both transform owners, and each `Complex`/`double[]` carrier mirrors a `Complex32`/`float[]` twin: the split `double[] real, double[] imaginary` form keeps contiguous scalar spans for a vectorized magnitude-phase pass, and the packed `ForwardReal`/`InverseReal` form stores the conjugate-even half-spectrum in an `N+2` (even `N`) or `N+1` (odd `N`) buffer.
- `FrequencyScale(length, sampleRate)` returns the per-bin axis: positive bins over the first `⌊N/2⌋+1` entries then the wrapped negative bins, spaced `sampleRate/length`.
- A paired taper's bare name is the symmetric filter-design form and its `*Periodic` twin the FFT-framing form; `Gauss` takes sigma relative to the half-width and `Tukey` the tapered fraction.
- `*Scaled` forms hold large-argument stability and `*Prime` forms give the derivative across the Bessel, Hankel, Airy, and Kelvin families.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): `Brent.TryFindRoot`'s `bool`/`out` pair and `NonlinearMinimizationResult.ReasonForExit` lift to `Fin<double>` and `Fin<Vector<double>>` at the seam, so non-convergence lands as a typed failure row instead of an exception crossing the receipt path.
- `CSparse`(`.api/api-csparse.md`): a residual Jacobian assembled as `CompressedColumnStorage<double>` factors on the direct sparse lane and steps through `ISolver<double>.Solve`, while this package keeps the model, the tolerances, and the exit condition.
- `System.Numerics.Tensors`(`.api/api-tensors.md`): `TensorPrimitives` folds the split `double[] real, double[] imaginary` spectral spans and the `Generate`/`Window` axes in place, so magnitude, phase, and taper application vectorize with no `Complex` marshalling.
- `UnitsNet`(`.api/api-unitsnet.md`): a quantity-typed integrand or sample set enters through `IQuantity.As(Enum)` as a base-unit `double` and the returned scalar re-enters its quantity type, so dimensional identity rides the caller and never the kernel.
- Numeric-rail fold: one `Generate.LinearSpaced` axis threads `Interpolate` fitting the sampled response, `IInterpolation.Differentiate` and `Differentiate` supplying the Jacobian column, `Integrate` reducing over the domain, and `Fourier` under a `Window` taper reading the spectrum.

[LOCAL_ADMISSION]:
- Every analytic kernel on the numeric rail enters through a `MathNet.Numerics` static owner; a parallel sampler owns one distribution instance and one `RandomSource` per worker.
- Wavelet filter banks and analog-prototype IIR design bind through their own scaling tables outside this package.

[RAIL_LAW]:
- Package: `MathNet.Numerics`
- Owns: provider-free analytic numeric kernel — probability, quadrature, interpolation, root finding, nonlinear least squares, special functions, spectral transform, metric reduction, and descriptive statistics
- Accept: `Func<double,double>` integrands and root targets, `double[]` sample and signal axes, the distribution seams, `IInterpolation` results, the no-throw `TryFindRoot` rail, in-place `Complex[]` and split `double[]` spectral buffers under a `FourierOptions` scaling
- Reject: a hand-rolled analytic kernel — quadrature, FFT, taper, CDF, or special-function series — beside the static owner already carrying it
