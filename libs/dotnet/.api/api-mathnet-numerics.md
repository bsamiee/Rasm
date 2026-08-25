# [RASM_API_MATHNET_NUMERICS]

`MathNet.Numerics` owns the branch's analytic numeric kernel and its linear-algebra plane, each domain a static owner folding plain `double[]`, `Func`, `Complex[]`, and `Matrix<double>`/`Vector<double>` carriers. Provider selection, dense factorization, sparse ingestion, and Krylov iteration are members of this one assembly; the MKL and OpenBLAS adapter packages supply native kernels behind that selection and own no algebra of their own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (MIT)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `.Distributions`, `.Integration`, `.IntegralTransforms`, `.Interpolation`, `.RootFinding`, `.Optimization`, `.Differentiation`, `.Statistics`, `.OdeSolvers`, `.Random`, `.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Storage`, `.LinearAlgebra.Factorization`, `.LinearAlgebra.Solvers`, `.LinearAlgebra.Double.Solvers`, `.Providers.LinearAlgebra`
- asset: managed runtime library; MKL and OpenBLAS kernels ride sibling provider packages
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: distribution seams and the univariate roster under its constructor parameterization

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `IDistribution`                                    | interface     | `RandomSource` ownership every draw reads          |
|  [02]   | `IUnivariateDistribution`                          | interface     | moments and `CumulativeDistribution`               |
|  [03]   | `IContinuousDistribution`                          | interface     | density, mode, and continuous sampling             |
|  [04]   | `IDiscreteDistribution`                            | interface     | mass, mode, and integer sampling                   |
|  [05]   | `Normal(mean, stddev)`                             | class         | Gaussian                                           |
|  [06]   | `LogNormal(mu, sigma)`                             | class         | log-Gaussian                                       |
|  [07]   | `InverseGaussian(mu, lambda)`                      | class         | Wald — mean/shape, the inverse-Gaussian GLM family |
|  [08]   | `Gamma(shape, rate)`                               | class         | Gamma, rate-parameterized                          |
|  [09]   | `InverseGamma(shape, scale)`                       | class         | inverse Gamma                                      |
|  [10]   | `Erlang(shape, rate)`                              | class         | integer-shape Gamma                                |
|  [11]   | `Beta(a, b)`                                       | class         | unit-interval Beta                                 |
|  [12]   | `BetaScaled(a, b, location, scale)`                | class         | affine-mapped Beta                                 |
|  [13]   | `ChiSquared(freedom)`                              | class         | chi-squared                                        |
|  [14]   | `Chi(freedom)`                                     | class         | chi                                                |
|  [15]   | `StudentT(location, scale, freedom)`               | class         | Student t                                          |
|  [16]   | `FisherSnedecor(d1, d2)`                           | class         | F ratio                                            |
|  [17]   | `Exponential(rate)`                                | class         | exponential                                        |
|  [18]   | `Weibull(shape, scale)`                            | class         | Weibull                                            |
|  [19]   | `Rayleigh(scale)`                                  | class         | Rayleigh                                           |
|  [20]   | `Pareto(scale, shape)`                             | class         | Pareto                                             |
|  [21]   | `TruncatedPareto(scale, shape, truncation)`        | class         | Pareto capped at a truncation point T > xm         |
|  [22]   | `Burr(a, c, k)`                                    | class         | Burr XII — scale a, shape pair (c, k)              |
|  [23]   | `Cauchy(location, scale)`                          | class         | Cauchy                                             |
|  [24]   | `Laplace(location, scale)`                         | class         | Laplace                                            |
|  [25]   | `Logistic(mean, scale)`                            | class         | logistic                                           |
|  [26]   | `Stable(alpha, beta, scale, location)`             | class         | stable, heavy-tailed                               |
|  [27]   | `SkewedGeneralizedT(location, scale, skew, p, q)`  | class         | skew and kurtosis-tunable t                        |
|  [28]   | `SkewedGeneralizedError(location, scale, skew, p)` | class         | skew-tunable error                                 |
|  [29]   | `Triangular(lower, upper, mode)`                   | class         | triangular                                         |
|  [30]   | `ContinuousUniform(lower, upper)`                  | class         | continuous uniform                                 |
|  [31]   | `Bernoulli(p)`                                     | class         | single trial                                       |
|  [32]   | `Binomial(p, n)`                                   | class         | successes in a trial count                         |
|  [33]   | `BetaBinomial(n, a, b)`                            | class         | over-dispersed binomial                            |
|  [34]   | `NegativeBinomial(r, p)`                           | class         | failures before a success count                    |
|  [35]   | `Geometric(p)`                                     | class         | trials to first success                            |
|  [36]   | `Poisson(lambda)`                                  | class         | Poisson count                                      |
|  [37]   | `ConwayMaxwellPoisson(lambda, nu)`                 | class         | dispersion-tuned Poisson                           |
|  [38]   | `Hypergeometric(population, success, draws)`       | class         | draws without replacement                          |
|  [39]   | `DiscreteUniform(lower, upper)`                    | class         | discrete uniform                                   |
|  [40]   | `Categorical(probabilityMass)`                     | class         | arbitrary mass vector                              |
|  [41]   | `Zipf(s, n)`                                       | class         | power-law rank                                     |

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

[PUBLIC_TYPE_SCOPE]: provider control, sparse storage, and the iterative-solve carriers of the linear-algebra plane

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------ | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Control`                             | static class  | package-wide provider, threading, and diagnostic façade           |
|  [02]   | `LinearAlgebraControl`                | static class  | linear-algebra provider selection and native probe path           |
|  [03]   | `ILinearAlgebraProvider`              | interface     | the BLAS-class kernel every dense product routes through          |
|  [04]   | `ManagedLinearAlgebraProvider`        | class         | the pure-managed provider carrying `Instance`                     |
|  [05]   | `SparseMatrix`                        | class         | double-precision sparse carrier over CSR storage                  |
|  [06]   | `SparseCompressedRowMatrixStorage<T>` | class         | the one native sparse form; every other layout ingests            |
|  [07]   | `Iterator<T>`                         | sealed class  | stop-criteria fold carrying `IterationStatus`                     |
|  [08]   | `IIterationStopCriterion<T>`          | interface     | one stop criterion the iterator composes                          |
|  [09]   | `IIterativeSolver<T>`                 | interface     | Krylov solver seam writing into a caller result vector            |
|  [10]   | `IPreconditioner<T>`                  | interface     | left preconditioner the solver applies per iteration              |
|  [11]   | `DiagonalPreconditioner`              | sealed class  | Jacobi preconditioner for the Krylov lane                         |
|  [12]   | `ILU0Preconditioner`                  | sealed class  | zero-fill incomplete LU; parameterless ctor, zero knobs           |
|  [13]   | `MILU0Preconditioner`                 | sealed class  | modified ILU(0), row-sum-preserving                               |
|  [14]   | `ILUTPPreconditioner`                 | sealed class  | threshold-and-pivot ILU                                           |
|  [15]   | `UnitPreconditioner<T>`               | class         | identity preconditioner, generic `.Solvers` tier                  |
|  [16]   | `CompositeSolver`                     | sealed class  | `.ctor(IEnumerable<IIterativeSolverSetup<double>>)` solver ladder |
|  [17]   | `QRMethod`                            | enum          | `Full` or `Thin` QR shape                                         |
|  [18]   | `IterationStatus`                     | enum          | per-iteration verdict the iterator settles on                     |

[ITERATION_STATUS]: `Continue` `Converged` `Diverged` `StoppedWithoutConvergence` `Cancelled` `Failure`
[EVD_PAIR_ORDER]: the real Schur reduction writes each conjugate pair as `e[k] = +z`, `e[k + 1] = −z` with `z` a non-negative square root, so the POSITIVE imaginary part always occupies the lower index and a negative-imaginary eigenvalue always has its partner at `j − 1`; index `0` therefore never carries a negative imaginary part and a backward modal-column read cannot underflow.

[INTERPOLATION_CAPABILITY]: the two `IInterpolation` flags are an ORTHOGONAL pair, not a ladder — `NevillePolynomialInterpolation` (`Interpolate.Polynomial`) reports `SupportsDifferentiation` true and `SupportsIntegration` false, `Barycentric` (`Interpolate.Common`) reports both false, and `CubicSpline`/`LinearSpline`/`StepInterpolation` report both true.

[DENSE_FACTORIZATION]: `LU<T>` `QR<T>` `Cholesky<T>` `Svd<T>` `Evd<T>` `GramSchmidt<T>` — each an `MathNet.Numerics.LinearAlgebra.Factorization` owner a `Matrix<T>` instance member builds. `SparseMatrix` declares NO `Cholesky()` override — a sparse operator routed there factorizes DENSELY through `UserCholesky.Create` with no fill-reducing ordering; sparse direct stays the CSparse peer's.
[ITERATIVE_SOLVER]: `BiCgStab` `GpBiCg` `TFQMR` `MlkBiCgStab` — `MathNet.Numerics.LinearAlgebra.Double.Solvers` owners; each precision plane (`Single`, `Double`, `Complex`, `Complex32`) carries its own closed set (`IncompleteLU` exists on NO plane — the spellings are the `ILU*Preconditioner` trio), so the solver and its preconditioner spell the plane's namespace and never a shared generic.
[PRECONDITIONER_CTOR]: `MILU0Preconditioner(bool modified = true)` exposing `UseModified` · `ILUTPPreconditioner()` and `ILUTPPreconditioner(double fillLevel, double dropTolerance, double pivotTolerance)` over defaults `200.0`/`1e-4`/`0.0`, pivoting off at zero.
[STOP_CRITERION]: `IterationCountStopCriterion<T>` `ResidualStopCriterion<T>` `DivergenceStopCriterion<T>` `FailureStopCriterion<T>` `DelegateStopCriterion<T>` — `DelegateStopCriterion<T>(Func<int, Vector<T>, Vector<T>, Vector<T>, IterationStatus> determine)` mirrors `Iterator<T>.DetermineStatus` exactly; `Reset()` clears only its held status, so a criterion closing over an absolute start instant survives a per-rung reset.
[COMPOSITE_LADDER]: `CompositeSolver.Solve` runs the setups in enumeration order under ONE shared `Iterator<T>` it `Reset()`s per rung — `Converged` copies out and returns, `StoppedWithoutConvergence` copies out and CONTINUES, every other verdict restores the input and continues, and a thrown rung is swallowed, so an all-rungs-failed ladder returns no exception and `Iterator.Status` carries only the LAST rung's verdict; the ctor's `preconditioner` argument is dead — a null setup preconditioner substitutes `UnitPreconditioner<T>`, so a fallback preconditioner never fires.
[TRY_SOLVE_ITERATIVE]: `IIterativeSolver<T>.Solve` returns `void` and leaves the verdict on `Iterator<T>.Status`, but the `MatrixExtensions.TrySolveIterative` family RETURNS `IterationStatus` directly — the `Iterator<T>` and `IPreconditioner<T>` tails default to `null`, and two further `params IIterationStopCriterion<T>[]` overloads build the iterator for the caller. A consumer reading the verdict off a member it also passed the iterator to is reading one fact twice.
[KRONECKER_PRODUCT]: the product is an inherited `Matrix<T>` member, so a `SparseMatrix` operand builds through the `new SparseMatrix(storage)` ctor and rides it — `SparseMatrix.OfStorage` is a phantom spelling. The result-writing overload is `virtual` and the allocating one delegates to it, matching the `Solve` pair's shape.
[SOLVER_SETUP]: `IIterativeSolverSetup<T>` — `SolverType` `PreconditionerType` `SolutionSpeed` `Reliability` `CreateSolver()` `CreatePreconditioner()`; MathNet ships NO concrete implementation — `SolverSetup<T>.LoadFromAssembly(Assembly, bool, params Type[])` reflection-scans for them and orders by `SolutionSpeed / Reliability`, and a consumer authoring its own setups reads neither figure.

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

- The quantile roster is NOT uniform: `Poisson` and `Binomial` ship no inverse at all (neither `InvCDF` nor `InverseCumulativeDistribution`), `Gamma` ships both spellings, and `InverseGaussian` alone spells its INSTANCE quantile `InvCDF(double)` where every sibling spells `InverseCumulativeDistribution(double)` — so a generic quantile call over a distribution set binds four different contracts.
- `InverseGaussian.Median` has no closed form: it Brent-solves numerically and CAN THROW, unlike every other row's arithmetic `Median`.

[ENTRYPOINT_SCOPE]: sequence and signal generation via `Generate`

[AXIS]: `LinearSpaced` `LogSpaced` `LinearRange` `LinearRangeInt32` `Periodic` `Sinusoidal` `Square` `Triangle` `Sawtooth` `Step` `Impulse` `PeriodicImpulse` `Repeat` `Unfold` `Fibonacci` `Map` `Map2`

[ENTRYPOINT_SCOPE]: quadrature via `Integrate`

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `Integrate.OnClosedInterval(F1, double, double)`                              | static  | double-exponential, `1e-8` absolute target |
|  [02]   | `Integrate.OnClosedInterval(F1, double, double, double)`                      | static  | the same rule at a caller error target     |
|  [03]   | `Integrate.DoubleExponential(F1, double, double, double)`                     | static  | the transformation named directly          |
|  [04]   | `Integrate.GaussLegendre(F1, double, double, int)`                            | static  | fixed-order Legendre rule                  |
|  [05]   | `Integrate.GaussKronrod(F1, double, double, double, int, int)`                | static  | adaptive rule at a relative-error target   |
|  [06]   | `Integrate.OnRectangle(F2, double, double, double, double)`                   | static  | 2-D Legendre product rule                  |
|  [07]   | `Integrate.OnRectangle(F2, double, double, double, double, int)`              | static  | the same rule at a caller node order       |
|  [08]   | `Integrate.OnCuboid(F3, double, double, double, double, double, double, int)` | static  | 3-D Legendre product rule                  |

- `F1`, `F2`, and `F3` abbreviate `Func<double,double>`, `Func<double,double,double>`, and `Func<double,double,double,double>`; every surface returns `double`.
- `Integrate.GaussKronrod` seats `out double` error and L1-norm estimates ahead of the optional tail in its second overload.

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
|  [18]   | `CreateMatrix.Dense<double>(int, int) -> M`                                        | static   | mint a zeroed dense carrier          |

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
|  [09]   | `ComplexExtensions.MagnitudeSquared(this Complex) -> double` | ext     | per-bin power with no square root   |

- Multidim rows `[04]`-`[06]` route to `FourierTransformControl.Provider.ForwardMultidim`/`BackwardMultidim`, and the MANAGED provider throws `NotSupportedException` on both — they run only under a native FFT provider, which the admitted MKL/OpenBLAS rows do not supply on this platform. The `[01]`-`[03]` 1D rows are managed-complete (Radix-2 at a power of two, Bluestein otherwise), and `FourierOptions.Default` symmetric scaling composes per axis (`1/√w · 1/√h = 1/√(w·h)`), so a row-column fold over the 1D pair IS the platform-total 2D transform and the multidim rows never reach a fence without a native-provider gate.

[ENTRYPOINT_SCOPE]: window tapers via `Window`, every factory returning a `double[]` of the requested width

[PAIRED_TAPER]: `Hann` `Hamming` `Cosine` `Lanczos`
[SINGLE_TAPER]: `Blackman` `BlackmanHarris` `BlackmanNuttall` `Nuttall` `FlatTop` `Bartlett` `BartlettHann` `Triangular` `Dirichlet`
[SHAPED_TAPER]: `Gauss(width, sigma)` `Tukey(width, r)`
- The 19 members above (15 tapers plus the four `*Periodic` twins) are the DECOMPILE-VERIFIED complete `Window` surface — no `Kaiser` and no `Bohman` member ships, so a Kaiser response is an owned Bessel-`I0` fold — the `Rasm` `Numerics/transform#WINDOW` `WindowTaper.Kaiser`/`Bohman` rows own the taper-array response and `Rasm.Materials` `Raster/plane#TEXTURE_PYRAMID` owns the polyphase windowed-sinc tap table — and never a phantom `Window.Kaiser` spelling.

[ENTRYPOINT_SCOPE]: provider selection — `Control` sets every provider class at once, `LinearAlgebraControl` the linear-algebra class alone; each `Use*` throws on a missing native asset where its `Try*` twin returns `false`

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `Control.UseManaged()`                                | static  | pin every class to the pure-managed kernel         |
|  [02]   | `Control.UseDefaultProviders()`                       | static  | environment-variable-directed selection            |
|  [03]   | `Control.UseBestProviders()`                          | static  | best available native, managed on none             |
|  [04]   | `Control.TryUseNative() -> bool`                      | static  | any native provider, `false` on none               |
|  [05]   | `Control.TryUseNativeMKL() -> bool`                   | static  | MKL selection verdict                              |
|  [06]   | `Control.TryUseNativeOpenBLAS() -> bool`              | static  | OpenBLAS selection verdict                         |
|  [07]   | `Control.TryUseNativeCUDA() -> bool`                  | static  | CUDA selection verdict                             |
|  [08]   | `Control.NativeProviderPath`                          | static  | probe root for every native binary                 |
|  [09]   | `Control.MaxDegreeOfParallelism`                      | static  | worker cap over the managed parallel folds         |
|  [10]   | `Control.TaskScheduler`                               | static  | scheduler the parallel folds queue onto            |
|  [11]   | `Control.UseSingleThread()` / `UseMultiThreading()`   | static  | parallelism policy without touching provider class |
|  [12]   | `Control.FreeResources()`                             | static  | release native provider handles                    |
|  [13]   | `Control.Describe() -> string`                        | static  | active provider and threading description          |
|  [14]   | `LinearAlgebraControl.Provider`                       | static  | read or set `ILinearAlgebraProvider` directly      |
|  [15]   | `LinearAlgebraControl.TryUse(ILinearAlgebraProvider)` | static  | admit a caller-built provider, verified            |
|  [16]   | `LinearAlgebraControl.HintPath`                       | static  | class-local probe root overriding `Control`        |
|  [17]   | `ManagedLinearAlgebraProvider.Instance`               | static  | the managed provider singleton                     |

- `Control.CheckDistributionParameters` and `ThreadSafeRandomNumberGenerators` are the two admission switches: the first gates every distribution constructor's parameter validation, the second decides whether `SystemRandomSource.Default` is shared or per-thread.
- `LinearAlgebraControl.Provider`'s setter runs `InitializeVerify()` on the incoming provider, so an unusable native provider faults at assignment rather than at first GEMM.

[ENTRYPOINT_SCOPE]: dense factorization and solve — every factorization is a `Matrix<T>` instance member, every solve a factorization instance member, and every construction a builder-handle member

| [INDEX] | [SURFACE]                                                     | [SHAPE]   | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------ | :-------- | :----------------------------------------------------- |
|  [01]   | `Matrix<T>.QR(QRMethod)`                                      | instance  | QR, `Thin` by default                                  |
|  [02]   | `Matrix<T>.Cholesky() -> Cholesky<T>`                         | instance  | SPD factorization                                      |
|  [03]   | `Matrix<T>.LU() -> LU<T>`                                     | instance  | pivoted LU                                             |
|  [04]   | `Matrix<T>.GramSchmidt() -> GramSchmidt<T>`                   | instance  | modified Gram-Schmidt QR                               |
|  [05]   | `Matrix<T>.Svd(bool computeVectors)`                          | instance  | singular values, vectors optional                      |
|  [06]   | `Matrix<T>.Evd(Symmetricity)`                                 | instance  | eigen decomposition under a symmetry hint              |
|  [07]   | `QR<T>.Solve(Vector<T>) -> Vector<T>`                         | instance  | least-squares solve allocating the result              |
|  [08]   | `QR<T>.Solve(Vector<T>, Vector<T>)`                           | instance  | the same solve into a caller-owned result              |
|  [09]   | `QR<T>.Solve(Matrix<T>)` / `Solve(Matrix, Matrix)`            | instance  | multi-right-hand-side pair                             |
|  [10]   | `QR<T>.Q` / `.R` / `.Determinant`                             | property  | the standing factors and the determinant               |
|  [11]   | `Matrix<T>.TransposeThisAndMultiply(Matrix<T>)`               | instance  | Gram product with no explicit transpose allocated      |
|  [12]   | `Matrix<T>.Build` / `Vector<T>.Build`                         | static    | root handle every builder factory hangs off            |
|  [13]   | `MatrixBuilder<T>.Dense(int, int, Func<int,int,T>)`           | instance  | dense matrix off an index projection                   |
|  [14]   | `MatrixBuilder<T>.DenseOfArray(T[,])`                         | instance  | dense matrix over a rectangular buffer                 |
|  [15]   | `VectorBuilder<T>.Dense(int, Func<int,T>)`                    | instance  | dense vector off an index projection                   |
|  [16]   | `Svd<T>.ConditionNumber` / `Svd<T>.Rank`                      | property  | conditioning witness on the factorization handle       |
|  [17]   | `Vector<T>.L2Norm()`                                          | instance  | Euclidean norm                                         |
|  [18]   | `Vector<T>.Enumerate()`                                       | instance  | lazy element walk carrying the finiteness probe        |
|  [19]   | `MatrixBuilder<T>.OfStorage(MatrixStorage<T>)`                | instance  | wrap prepared storage as the dense/sparse carrier      |
|  [20]   | `VectorBuilder<T>.OfStorage(VectorStorage<T>)`                | instance  | the vector half of the same admission                  |
|  [21]   | `VectorBuilder<T>.DenseOfArray(T[])`                          | instance  | dense vector over a caller buffer                      |
|  [22]   | `Vector<T>.AsArray() -> T[]`                                  | instance  | the backing buffer where dense, else `null`            |
|  [23]   | `Matrix<T>.AsArray() -> T[,]` / `ToArray()`                   | instance  | backing rectangle where dense; `ToArray` copies        |
|  [24]   | `Matrix<T>.Multiply(Matrix<T>)` / `Multiply(Vector<T>)`       | instance  | allocating product over a matrix or vector             |
|  [25]   | `Matrix<T>.Multiply(T)`                                       | instance  | allocating scalar scale                                |
|  [26]   | `Matrix<T>.Transpose() -> Matrix<T>`                          | instance  | allocating transpose                                   |
|  [27]   | `Matrix<T>.Column(int)` / `Column(int, int, int)`             | instance  | one column, whole or as a row-bounded slice            |
|  [28]   | `Matrix<T>.IsSymmetric() -> bool`                             | instance  | EXACT elementwise symmetry test                        |
|  [29]   | `Matrix<T>.Inverse()` / `PseudoInverse()`                     | instance  | full inverse and its rank-deficient counterpart        |
|  [30]   | `Matrix<T>.Storage` / `Vector<T>.Storage`                     | property  | the underlying `MatrixStorage<T>`/`VectorStorage<T>`   |
|  [31]   | `Cholesky<T>.Factor -> Matrix<T>`                             | property  | the standing lower triangular factor                   |
|  [32]   | `LU<T>.Solve(Matrix<T>)` / `LU<T>.Inverse()`                  | instance  | multi-right-hand-side solve and the full inverse       |
|  [33]   | `Evd<T>.EigenValues -> Vector<Complex>`                       | property  | the spectrum as complex values                         |
|  [34]   | `Evd<T>.EigenVectors -> Matrix<T>`                            | property  | the modal matrix                                       |
|  [35]   | `Cholesky<T>.DeterminantLn`                                   | property  | log determinant off the SPD factor                     |
|  [36]   | `LU<T>.Determinant`                                           | property  | determinant off the pivoted factor                     |
|  [37]   | `Svd<T>.S` / `.U` / `.VT` / `.W`                              | property  | singular values and the factor triple                  |
|  [38]   | `Svd<T>.L2Norm`                                               | property  | spectral norm off the standing factorization           |
|  [39]   | `Evd<T>.D` / `.Rank` / `.IsFullRank`                          | property  | block-diagonal spectrum and the rank verdict           |
|  [40]   | `Matrix<T>.ToColumnMajorArray()` / `ToRowMajorArray()`        | instance  | flat copy in either storage order                      |
|  [41]   | `Matrix<T>.FrobeniusNorm()`                                   | instance  | entrywise L2 norm                                      |
|  [42]   | `Matrix<T>.SetColumn(int, Vector<T>)` / `SetRow(...)`         | instance  | write one column or row in place                       |
|  [43]   | `Matrix<T>.Diagonal() -> Vector<T>`                           | instance  | the diagonal as a vector                               |
|  [44]   | `Matrix<T>.PointwiseMultiply(Matrix<T>)`                      | instance  | Hadamard product                                       |
|  [45]   | `Matrix<T>.Multiply(Vector<T>, Vector<T>)`                    | instance  | GEMV into a caller-owned result                        |
|  [46]   | `Vector<T>.DotProduct(Vector<T>) -> T`                        | instance  | inner product                                          |
|  [47]   | `Vector<T>.InfinityNorm()`                                    | instance  | max-magnitude norm                                     |
|  [48]   | `Vector<T>.PointwiseMultiply(Vector<T>)`                      | instance  | elementwise product                                    |
|  [49]   | `Vector<T>.Add`/`Subtract`(_, `Vector<T>` result)             | instance  | the same two into a caller-owned result                |
|  [50]   | `MatrixBuilder<T>.Dense(int, int, T[])`                       | instance  | dense matrix over a COLUMN-MAJOR flat buffer           |
|  [51]   | `MatrixBuilder<T>.DenseOfColumns(IEnumerable<...>)`           | instance  | dense matrix from column sequences                     |
|  [52]   | `MatrixBuilder<T>.DenseOfDiagonalVector(Vector<T>)`           | instance  | dense carrier with the vector on its diagonal          |
|  [53]   | `MatrixBuilder<T>.DiagonalOfDiagonalVector(Vector<T>)`        | instance  | diagonal-STORAGE carrier from the same vector          |
|  [54]   | `VectorBuilder<T>.Dense(int)`                                 | instance  | zeroed dense vector at a length                        |
|  [55]   | `Matrix<T>.KroneckerProduct(Matrix<T>) -> Matrix<T>`          | instance  | allocating tensor product over the two operands        |
|  [56]   | `Matrix<T>.KroneckerProduct(Matrix<T>, Matrix<T>)`            | instance  | the same product into a caller-owned result, `virtual` |
|  [57]   | `MatrixExtensions.TrySolveIterative(Vector<T>, Vector<T>, …)` | extension | Krylov solve, RETURNS `IterationStatus`                |
|  [58]   | `MatrixExtensions.TrySolveIterative(Matrix<T>, Matrix<T>, …)` | extension | the multi-RHS shape of the same solve                  |

- Every factorization owner mirrors the four `Solve` shapes; the allocating forms are `virtual` over the `abstract` result-writing pair, so a hot loop reuses one result carrier.
- `Matrix<T>.Inverse()` is `virtual` and routes `LU().Inverse()` on the base implementation, so an "inverse of a Cholesky factor" densely LU-inverts a triangular matrix — the triangular solve the factor admits is the cheaper spelling and the one a reduction congruence wants.
- `Matrix<T>.IsSymmetric()` compares elements by exact inequality, so an accumulated block that is symmetric to round-off FAILS it; a reduction handing a pencil to a symmetric terminal symmetrizes first rather than asserting.
- `AsArray()` returns the BACKING buffer for a dense carrier and `null` for every other storage, where `ToArray()` always copies — a caller reading `AsArray()` off a sparse or diagonal carrier reads absence, not an empty rectangle.
- `Evd<T>.EigenValues` is `Vector<Complex>` on every plane including the symmetric one, so a real spectrum reads `.Real` per entry and a consumer typing it as `Vector<double>` does not compile.
- `Vector<T>.Multiply` and `Divide` take a SCALAR right operand only — their result-writing forms are `(T scalar, Vector<T> result)`, never a vector pair — so an elementwise product or quotient spells `PointwiseMultiply`/`PointwiseDivide`; only `Add` and `Subtract` carry both scalar and `Vector<T>` operands. `Matrix<T>.Multiply(Vector<T>, Vector<T>)` IS the GEMV-into-a-result form and `TransposeThisAndMultiply` mirrors it on the transpose.
- `MatrixBuilder<T>.Dense(int, int, T[])` binds a COLUMN-MAJOR flat buffer and `DenseOfDiagonalVector`/`DiagonalOfDiagonalVector` differ by STORAGE, not by value — the first materializes a dense rectangle, the second keeps diagonal storage, so `AsArray()` reads absence off the second.

[ENTRYPOINT_SCOPE]: sparse ingestion and Krylov solve — `SparseCompressedRowMatrixStorage<T>` (namespace `MathNet.Numerics.LinearAlgebra.Storage`) is the one storage form, and each `Of*` static converts its layout into it

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `OfCompressedSparseRowFormat(r, c, nnz, rowPointers, columnIndices, v)`    | static   | admit CSR                               |
|  [02]   | `OfCompressedSparseColumnFormat(r, c, nnz, rowIndices, columnPointers, v)` | static   | admit CSC                               |
|  [03]   | `OfCoordinateFormat(r, c, nnz, rowIndices, columnIndices, v)`              | static   | admit COO triplets                      |
|  [04]   | `OfIndexedEnumerable(r, c, IEnumerable<(int, int, T)>)`                    | static   | admit an indexed triplet sequence       |
|  [05]   | `SparseMatrix.OfIndexed(r, c, IEnumerable<(int, int, double)>)`            | static   | the same admission returning the matrix |
|  [06]   | `new SparseMatrix(SparseCompressedRowMatrixStorage<double>)`               | ctor     | wrap prepared storage as the carrier    |
|  [07]   | `IIterativeSolver<T>.Solve(A, b, x, Iterator<T>, IPreconditioner<T>)`      | instance | Krylov solve into a caller result       |
|  [08]   | `new Iterator<T>(params IIterationStopCriterion<T>[])`                     | ctor     | compose the stop-criteria set           |
|  [09]   | `Iterator<T>.DetermineStatus(int, Vector<T>, Vector<T>, Vector<T>)`        | instance | per-iteration verdict off the residual  |
|  [10]   | `Iterator<T>.Status` / `Reset()` / `Cancel()` / `Clone()`                  | instance | verdict read, re-arm, abort, reuse      |

- `A`, `b`, and `x` abbreviate the coefficient `Matrix<T>`, the source `Vector<T>`, and the result `Vector<T>`; rows [01]-[04] are `SparseCompressedRowMatrixStorage<T>` statics and `v` abbreviates the `T[]` value buffer.

- `OfCompressedSparseColumnFormat`: row INDICES precede column POINTERS — the argument-order trap CSR inverts; `OfIndexedEnumerable` carries a `Tuple<int, int, T>` twin beside the value-tuple form.
- `SparseMatrix.OfIndexed` APPENDS duplicate `(row, column)` triplets — it sorts and emits without summing, silently corrupting an accumulated assembly; `SparseCompressedRowMatrixStorage<double>.NormalizeDuplicates()` is the one member that adds coincident entries, and zero-valued triplets DROP at admission so a cancelled diagonal goes structurally missing until `PopulateExplicitZerosOnDiagonal()` restores it.
- `IIterativeSolver<T>.Solve` returns `void` and writes into the `result` vector, so the convergence verdict reads from `Iterator<T>.Status` alone; the solver, its preconditioner, and its stop criteria all bind at one precision, and a plane switch re-spells the whole triple.
- `Solve` calls `preconditioner.Initialize(matrix)` ITSELF on every invocation — the factorization cannot amortize across right-hand sides through this seam, so a multi-RHS solve re-pays the incomplete factor per call; null `iterator`/`preconditioner` arguments substitute `new Iterator<double>()` / `new UnitPreconditioner<double>()`.
- `MILU0Preconditioner.Initialize` REQUIRES `matrix.Storage is SparseCompressedRowMatrixStorage<double>` — a dense matrix throws — and is the only ILU whose cost tracks nnz (the Saad MSR kernel over raw CSR buffers); `ILU0Preconditioner` runs an indexer triple-loop and materializes a dense row per `Approximate`, so the MODIFIED row is the production spelling on any real grid.
- Production's canonical stop set (what the parameterless `Iterator<double>()` seeds, order semantic — `DetermineStatus` short-circuits on the first non-`Continue`): `FailureStopCriterion<double>()`, `DivergenceStopCriterion<double>()`, `IterationCountStopCriterion<double>(1000)`, `ResidualStopCriterion<double>(1e-12)`. `ResidualStopCriterion` tests `‖r‖∞ ≤ tolerance · ‖b‖∞` — infinity-norm RELATIVE, never absolute L2.

[ENTRYPOINT_SCOPE]: statistics, differentiation, and the numeric-utility owners

[STATISTICS]: `Mean` `Variance` `StandardDeviation` `PopulationVariance` `Covariance` `Skewness` `Kurtosis` `Median` `Quantile` `QuantileCustom` `Percentile` `InterquartileRange` `FiveNumberSummary` `Ranks` `QuantileRank` `EmpiricalCDF` `EmpiricalInvCDF` `RootMeanSquare` `GeometricMean` `HarmonicMean` `Entropy` `MovingAverage` `OrderStatistic`
[STATISTICS_OWNER]: `Statistics` `ArrayStatistics` `SortedArrayStatistics` `StreamingStatistics` `DescriptiveStatistics` `WeightedDescriptiveStatistics` `RunningStatistics` `RunningWeightedStatistics` `MovingStatistics` `Correlation` `Histogram` `KernelDensity` `GoodnessOfFit`

[CORRELATION]: `MathNet.Numerics.Statistics.Correlation` — `Pearson(IEnumerable<double>, IEnumerable<double>)`, `Spearman(IEnumerable<double>, IEnumerable<double>)`; matrix forms take an ARRAY OF SERIES: `PearsonMatrix(double[][])` / `PearsonMatrix(IEnumerable<double[]>)`, `SpearmanMatrix(double[][])` / `SpearmanMatrix(IEnumerable<double[]>)` — each inner array is one data vector, the result the square correlation matrix over them (shipped-XML-doc verified, 6.0.0-beta2); `Rasm.Compute/Stats/signal` `DependenceKind` rows bind the `double[][]` arity through a collection expression.

[KERNEL_DENSITY]: `MathNet.Numerics.Statistics.KernelDensity` — argument ORDER is `(double x, double bandwidth, IList<double> samples)`: `EstimateGaussian` / `EstimateEpanechnikov` / `EstimateUniform` / `EstimateTriangular`, plus the kernel-taking `Estimate(double x, double bandwidth, IList<double> samples, Func<double, double> kernel)` (shipped-XML-doc verified, 6.0.0-beta2); `Rasm.Compute/Stats/signal` `DensityKernel` rows bind the four named-kernel method groups whole.

[SORTED_ARRAY_STATISTICS]: `MathNet.Numerics.Statistics.SortedArrayStatistics` — every member takes ASCENDING-sorted data, O(1) after the sort, `double[]` and `float[]` overload pairs

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `Median(double[] data)`                                           | static  | R8-definition median over sorted data           |
|  [02]   | `OrderStatistic(double[] data, int order)`                        | static  | exact k-th smallest, 1-based order              |
|  [03]   | `Percentile(double[], int p)` / `LowerQuartile` / `UpperQuartile` | static  | percentile and quartile reads                   |
|  [04]   | `InterquartileRange(double[] data)`                               | static  | `UpperQuartile - LowerQuartile`                 |
|  [05]   | `FiveNumberSummary(double[] data) -> double[]`                    | static  | min, Q1, median, Q3, max                        |
|  [06]   | `Quantile(double[] data, double tau)`                             | static  | R8 default quantile                             |
|  [07]   | `QuantileCustom(double[], double tau, QuantileDefinition d)`      | static  | definition-selected; `R1` is exact nearest-rank |
|  [08]   | `QuantileCustom(double[], double tau, double a, b, c, d)`         | static  | four-parameter quantile family                  |
|  [09]   | `QuantileRank(double[], double x, RankDefinition r)` / `Ranks`    | static  | inverse-quantile rank reads                     |
|  [10]   | `EmpiricalCDF(double[] data, double x)`                           | static  | empirical distribution read                     |

- `QuantileDefinition` (`MathNet.Numerics.Statistics`) carries the R1-R9 aliases: `R1 = EmpiricalInvCDF = SAS3` is the exact ceiling nearest-rank definition, `R3 = Nearest = SAS2` rounds HALF-TO-EVEN at the rank boundary (decompile-verified — not an exact order pick on ties), `R8 = Median = Default`, `R7 = Excel`; a nearest-rank p95 spells `QuantileCustom(sorted, 0.95, QuantileDefinition.R1)` and stays an exact order statistic, never an interpolation; `Median(double[])` is the classic central-or-two-central-mean fold despite its R8 doc comment, and every member returns `double.NaN` on an empty array rather than throwing.
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
- Every distribution mints a parallel static family keyed on its constructor tuple, so a one-shot evaluation allocates no instance; an alternate parameterization arrives as a `With*` factory rather than a second constructor.
- Omitting `System.Random` binds `SystemRandomSource.Default`, and no sampling form allocates an internal store.
- `Generate.*Map` fuses the projection into the axis walk and `Generate.*Sequence` yields it lazily, so neither materializes an intermediate array.
- `GaussKronrod`'s `out error`/`out L1Norm` overload is the only quadrature carrying its own error estimate; the `order` argument on `OnRectangle`/`OnCuboid` sets the per-axis node count.
- Root finding is one static class per method with no aggregator: every iterative class mirrors `FindRoot` with `TryFindRoot`, whose `bool` return carries non-convergence where `FindRoot` raises `NonConvergenceException`.
- `Broyden.FindRoot` solves a square system carrying no bounds; a rectangular residual or a per-parameter travel limit routes to `LevenbergMarquardtMinimizer.FindMinimum`, which takes the full residual and the bound vectors natively.
- Least squares fits `f(parameters, observedX) -> predicted` against `observedY`, so a pure residual formulation passes a zero `observedY` of the residual's rank and `MinimizingPoint` minimizes the residual norm.
- Every `Fourier` transform mutates the caller-owned buffer: `Default` applies symmetric scaling, `AsymmetricScaling` scales the inverse by `1/N`, and `NoScaling` omits it, so a forward-inverse round trip is identity under the first two and carries a factor of `N` under the third.
- `Forward*` and `Inverse*` mirror at one signature across both transform owners, and each `Complex`/`double[]` carrier mirrors a `Complex32`/`float[]` twin: the split `double[] real, double[] imaginary` form keeps contiguous scalar spans for a vectorized magnitude-phase pass, and the packed `ForwardReal`/`InverseReal` form stores the conjugate-even half-spectrum in an `N+2` (even `N`) or `N+1` (odd `N`) buffer.
- `FrequencyScale(length, sampleRate)` returns the per-bin axis: positive bins over the first `⌊N/2⌋+1` entries then the wrapped negative bins, spaced `sampleRate/length`.
- Paired tapers name the symmetric filter-design form bare and the FFT-framing form `*Periodic`; `Gauss` takes sigma relative to the half-width and `Tukey` the tapered fraction.
- `*Scaled` forms hold large-argument stability and `*Prime` forms give the derivative across the Bessel, Hankel, Airy, and Kelvin families.
- Dense least squares routes an overdetermined `m > n` system through `Matrix<double>.QR(QRMethod.Thin)` then `QR<double>.Solve(b)` — the `argmin‖Ax−b‖₂` minimizer — with `Svd(true)` the truncated pseudo-inverse conditioning fallback; the `A.TransposeThisAndMultiply(A)` Gram path (`Cholesky().Solve(Aᵀb)`, no explicit transpose allocation) squares the condition number and stays the SPD-only fallback.
- Near-zero column norms divide through and fill `Q`/`R` with `NaN` while `IsFullRank` returns `true`, so admission gates the design matrix all-finite before factoring and probes the factor output all-finite after.
- Provider selection is process-wide static state resolved once at composition: `Control` sets every provider class and `LinearAlgebraControl` the linear-algebra class alone, `Provider`'s setter verifies through `InitializeVerify()`, and every dense `Multiply` thereafter routes the active `ILinearAlgebraProvider`. Each `Try*` form returns `false` on a missing native asset where its `Use*` twin throws, so admission reads the verdict rather than trapping.
- Native adapters are separate x64-only companion packages probed by assembly-qualified type name off `NativeProviderPath` or the class-local `HintPath`, so osx-arm64 resolves no asset and rides `ManagedLinearAlgebraProvider.Instance`.
- `SparseCompressedRowMatrixStorage<T>` is the one native sparse form; CSC, COO, and indexed-triplet layouts are ingestion conversions through the `Of*` statics rather than separate storage types, and the CSC form inverts the CSR argument order.
- Krylov solves write into their caller-owned result vector and return `void`, so `Iterator<T>.Status` carries the convergence verdict; solver, preconditioner, and stop criteria bind at one precision plane, and a plane switch re-spells all three.

[STACKING]:
- The row-column 2D fold — 1D `Fourier.Forward`/`Inverse` per axis, the managed-total form the multidim provider gap forces — is OWNED at `Rasm/Numerics/transform#SPECTRAL` (`SpectralArena.Transform` over the `MatrixKernel` transform half); `Rasm.Materials` `Raster/filter#HEIGHT_FIELD` composes that kernel band for the Frankot-Chellappa integration and `Raster/tile#TILE_GATE` reads the power spectrum off the same owner, while `Raster/tile#TILE_SYNTH` reads `Distributions.Normal.InvCDF`/`CDF` for the variance-preserving rank blend; every `Forward*` carries an on-disk `Inverse*` mirror, so the round trip is the catalogued pair, never a hand-built adjoint.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Brent.TryFindRoot`'s `bool`/`out` pair and `NonlinearMinimizationResult.ReasonForExit` lift to `Fin<double>` and `Fin<Vector<double>>` at the seam, so non-convergence lands as a typed failure row instead of an exception crossing the result path.
- `CSparse`(`.api/api-csparse.md`): a residual Jacobian assembled as `CompressedColumnStorage<double>` factors on the direct sparse lane and steps through `ISolver<double>.Solve`, while this package keeps the model, the tolerances, and the exit condition; matrix density and factor reuse select between that direct lane and the Krylov solvers under an `Iterator<T>` stop-criteria control. The split is STRUCTURAL for SPD grids: MathNet ships NO sparse Cholesky (a `SparseMatrix.Cholesky()` densifies), so the direct SPD route is always the CSparse `CholeskySparse` peer, and this package's contribution above the direct ceiling is the PRECONDITIONED KRYLOV lane — `MILU0Preconditioner` under `BiCgStab.Solve` over the same CSR assembly, the composition the `Rasm.Materials` `Raster/filter#HEIGHT_FIELD` bounded Poisson arm rides at large extents.
- `MathNet.Numerics.Providers.MKL` and `.Providers.OpenBLAS`(`Rasm.Compute/.api/api-mathnet-providers.md`): the two native adapter packages this assembly probes by type name, each carrying its own control class and native asset matrix and no algebra of its own; that catalogue also owns how `Rasm.Compute` folds this plane onto its solve result.
- `System.Numerics.Tensors`(`.api/api-tensors.md`): `TensorPrimitives` folds the split `double[] real, double[] imaginary` spectral spans and the `Generate`/`Window` axes in place, so magnitude, phase, and taper application vectorize with no `Complex` marshalling.
- `UnitsNet`(`.api/api-unitsnet.md`): a quantity-typed integrand or sample set enters through `IQuantity.As(Enum)` as a base-unit `double` and the returned scalar re-enters its quantity type, so dimensional identity rides the caller and never the kernel.
- Numeric-rail fold: one `Generate.LinearSpaced` axis threads `Interpolate` fitting the sampled response, `IInterpolation.Differentiate` and `Differentiate` supplying the Jacobian column, `Integrate` reducing over the domain, and `Fourier` under a `Window` taper reading the spectrum.
- `Rasm.Materials`: `acquisition#ACQUISITION` `SolveGgx` runs the thin-QR Gauss-Newton step `Δp = Matrix.Build.Dense(m, n, Jacobian).QR(QRMethod.Thin).Solve(−r)` (`n ∈ {3, 4}` — the conductor arm fits the alpha pair plus the grain azimuth, the dielectric adds η) over the log-residual, switches to `Svd(true).Solve` on a non-finite step, and witnesses `‖r‖/‖logMeasured‖` via `Vector.L2Norm` — the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel form stays the forward model, MathNet owning only the dense solve; the fitted `FitResidual` and the `Wacton.Unicolour` spectral-grounded scene-linear base colour pair on the acquisition result, never a fused colour-plus-numeric kernel, and MathNet is a direct Materials pin — the acyclic strata forbids a `Rasm.Compute` project reference to obtain it transitively.

[LOCAL_ADMISSION]:
- Every analytic kernel on the numeric rail enters through a `MathNet.Numerics` static owner; a parallel sampler owns one distribution instance and one `RandomSource` per worker.
- Wavelet filter banks and analog-prototype IIR design bind through their own scaling tables outside this package.
- `Matrix<double>`/`Vector<double>` compose directly — never a package-local matrix wrapper — a residual witness recomputes `‖A·x − b‖₂ / ‖b‖₂` against the original operator in working precision, never the reconstructed factors, and `Control.UseManaged()` selects once at composition: the native MKL/OpenBLAS/CUDA providers are separate x64-only companion packages, so osx-arm64 always rides `ManagedLinearAlgebraProvider.Instance` and a per-call-site `Control.TryUseNativeMKL()` is the named defect.

[RAIL_LAW]:
- Package: `MathNet.Numerics`
- Owns: the analytic numeric kernel — probability, quadrature, interpolation, root finding, nonlinear least squares, special functions, spectral transform, metric reduction, descriptive statistics — and the linear-algebra plane's provider selection, dense factorization, sparse ingestion, and Krylov solve
- Accept: `Func<double,double>` integrands and root targets, `double[]` sample and signal axes, the distribution seams, `IInterpolation` results, the no-throw `TryFindRoot` rail, in-place `Complex[]` and split `double[]` spectral buffers under a `FourierOptions` scaling, `Matrix<double>`/`Vector<double>` dense work, CSR ingestion through the `Of*` statics, and one composition-time provider selection
- Reject: a hand-rolled analytic kernel — quadrature, FFT, taper, CDF, or special-function series — beside the static owner already carrying it, a hand-rolled Levenberg-Marquardt or normal-equations loop, a Gram-plus-ridge squaring `κ` where thin-QR avoids it, a package-local matrix wrapper, and a per-call-site provider switch beside the one composition selection
