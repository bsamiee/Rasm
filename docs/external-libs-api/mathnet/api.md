# [H1][MATHNET_API]
>**Dictum:** *MathNet is the approved numerical and symbolic substrate for managed algorithms.*

<br>

[IMPORTANT] This file maps available API families. It is not a generated member dump. Use local XML for exact signatures before implementation.

---
## [1][FOUNDATION]
>**Dictum:** *Foundation APIs cover scalar math, precision, fitting, and generated data.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Numerics` | `Precision` | Tolerance helpers, epsilon comparisons, decimal-place utilities. | Use for managed numeric admission only when Rhino tolerance is not the owner. |
| [2] | `MathNet.Numerics` | `SpecialFunctions` | Gamma, beta, erf, Bessel, factorial, binomial, logarithmic special functions. | Use for statistical or analytic kernels. |
| [3] | `MathNet.Numerics` | `Fit` | Polynomial, exponential, linear, and curve fitting helpers. | Activate as soon as fitting enters a typed Rasm operation. |
| [4] | `MathNet.Numerics` | `Distance` | Euclidean, Manhattan, Chebyshev, cosine, Hamming, Jaccard-like distances. | Prefer for point-cloud descriptors only after semantics are named. |
| [5] | `MathNet.Numerics` | `Generate`, `Series`, `Window` | Linear, log-spaced, periodic, windowed, and sequence-generation helpers. | Use only with deterministic seeds and explicit output ownership. |
| [6] | `MathNet.Numerics` | `Polynomial` | Polynomial creation, evaluation, roots, derivatives, integrals. | Pair with Symbolics polynomial transforms for exact-plus-numeric workflows. |
| [7] | `MathNet.Numerics` | `Trig`, `Euclid`, `Combinatorics`, `Constants` | Trigonometric helpers, Euclidean algorithms, combinatorial functions, scalar constants. | Use only where RhinoCommon is not the semantic owner. |
| [8] | `MathNet.Numerics.LinearRegression` | `SimpleRegression`, `MultipleRegression`, `WeightedRegression` | Regression helpers for fitted scalar models. | Keep estimator names and residual policy explicit. |
| [9] | `MathNet.Numerics.GoodnessOfFit` | Fit metrics | R-squared, error, and distance metrics for model fit. | Use only beside a named fitting operation. |
| [10] | `MathNet.Numerics.Financial` | Financial math helpers | Rates, payments, and cash-flow calculations. | Reserve for non-geometry financial features. |
| [11] | `MathNet.Numerics` | `FindRoots`, `FindMinimum`, `Integrate`, `Interpolate`, `Differentiate` | Static facades: `FindRoots.OfFunction`, derivative, quadratic, cubic, polynomial, Chebyshev; `FindMinimum` scalar, constrained, gradient, Hessian paths. | Use when a compact facade removes ceremony without hiding operation policy. |
| [12] | `MathNet.Numerics` | `ContourIntegrate`, `DifferIntegrate` | `ContourIntegrate` complex paths; `DifferIntegrate.DoubleExponential`, `GaussLegendre`, `GaussKronrod` fractional derivative/integral bridge APIs. | Activate for advanced analytic kernels and symbolic-to-numeric validation. |
| [13] | `MathNet.Numerics` | `Sorting`, `Permutation`, `ExcelFunctions`, `TestFunctions` | Ordering utilities, permutation representation, Excel-style functions, benchmark/test functions. | Use directly for algorithm infrastructure and optimization diagnostics. |
| [14] | `MathNet.Numerics` | `Complex32`, `ComplexExtensions`, `Evaluate`, `AppSwitches`, `ArrayExtensions`, `IPrecisionSupport<T>`, `RandomSeed`, `RandomExtensions` | Single-precision complex support, Chebyshev evaluation, app switches, array helpers, precision contracts, and random seeding helpers. | Use as direct infrastructure inside owning numeric kernels. |

---
## [2][LINEAR_ALGEBRA]
>**Dictum:** *Linear algebra is the active MathNet integration surface.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Numerics.LinearAlgebra` | `Matrix<T>` | Dense/sparse abstraction, multiplication, inverse, transpose, norms, decomposition entrypoints. | Active inside `Rasm.Vectors.Matrix`. |
| [2] | `MathNet.Numerics.LinearAlgebra` | `Vector<T>` | Indexed vector abstraction, dot products, norms, pointwise transforms. | Active as internal bridge type only. |
| [3] | `MathNet.Numerics.LinearAlgebra` | `MatrixBuilder<T>` / `VectorBuilder<T>`, `CreateMatrix`, `CreateVector`, `BuilderInstance<T>` | Factory APIs for dense, sparse, diagonal, existing-data, random, zero, and typed values. | Use to build MathNet values directly inside Rasm kernels. |
| [4] | `MathNet.Numerics.LinearAlgebra` | `MatrixExtensions`, `VectorExtensions`, `ExistingData`, `Zeros`, `Symmetricity` | Extension and construction policies shared by typed implementations. | Keep construction and symmetric/Hermitian admission explicit. |
| [5] | `MathNet.Numerics.LinearAlgebra.Double` | `Matrix`, `Vector`, `DenseMatrix`, `SparseMatrix`, `DiagonalMatrix`, `DenseVector`, `SparseVector` | Concrete double-precision storage. | Active for Rasm real matrix operations. |
| [6] | `MathNet.Numerics.LinearAlgebra.Complex` | `Matrix`, `Vector`, `DenseMatrix`, `SparseMatrix`, `DiagonalMatrix`, `DenseVector`, `SparseVector` | Concrete complex storage. | Active for Hermitian and complex eigen paths. |
| [7] | `MathNet.Numerics.LinearAlgebra.Single` | Typed matrix/vector, dense, sparse, and diagonal storage. | Float matrix/vector algorithms. | Use for dense sampling, previews, and memory-heavy fields with explicit precision policy. |
| [8] | `MathNet.Numerics.LinearAlgebra.Complex32` | Typed matrix/vector, dense, sparse, and diagonal storage. | Float complex algorithms. | Use for spectral previews and memory-heavy complex fields with explicit precision policy. |

---
## [3][FACTORIZATION_SOLVERS]
>**Dictum:** *Decomposition APIs are fallible numerical boundaries.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Numerics.LinearAlgebra.Factorization` | `Svd<T>` | Singular values, singular vectors, rank and pseudo-inverse foundations. | Active for spectral, rank, and pseudo-inverse operations. |
| [2] | `MathNet.Numerics.LinearAlgebra.Factorization` | `LU<T>` | Square solves, determinant, permutation-backed factorization. | Active for determinant and direct solve. |
| [3] | `MathNet.Numerics.LinearAlgebra.Factorization` | `QR<T>` / `QRMethod` | Orthogonal decomposition and least-squares foundations. | Active for QR result exposure. |
| [4] | `MathNet.Numerics.LinearAlgebra.Factorization` | `Cholesky<T>` | SPD/Hermitian positive-definite factorization. | Active but must map exceptions to `Fin<T>`. |
| [5] | `MathNet.Numerics.LinearAlgebra.Factorization` | `Evd<T>` / `Symmetricity`, `GramSchmidt<T>`, `ISolver<T>` | General, symmetric, Hermitian, orthogonalization, and solve-backed decomposition surfaces. | Active for eigen and orthogonalization paths. |
| [6] | Typed factorization namespaces | `DenseCholesky`, `DenseEvd`, `DenseGramSchmidt`, `DenseLU`, `DenseQR`, `DenseSvd`, `User*` variants. | Concrete double, single, complex, and complex32 factorization implementations. | Keep internal and project into Rasm result records only when public semantics are needed. |
| [7] | `MathNet.Numerics.LinearAlgebra.Solvers` | Solver infrastructure | `IIterativeSolver<T>`, `Iterator<T>`, `SolverSetup<T>`, `UnitPreconditioner<T>`, `CompositeSolver`, stop criteria, `IterationStatus`. | Use for linear-system solve policy only. |
| [8] | `MathNet.Numerics.LinearAlgebra.Double.Solvers` and typed peers | Concrete solvers and preconditioners | `BiCgStab`, TFQMR, `GpBiCg`, `MlkBiCgStab`, ILU0, ILUTP, diagonal preconditioners. | Does not replace Rasm sparse eigen or LOBPCG code. |
| [9] | `MathNet.Numerics.LinearAlgebra.Storage` | Matrix and vector storage classes | Dense, diagonal, sparse vector, and sparse compressed-row matrix storage. | Internal implementation detail only. |

---
## [4][STATISTICS_PROBABILITY]
>**Dictum:** *Statistical APIs require named estimator semantics.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Numerics.Statistics` | `Statistics` | Mean, variance, standard deviation, quantiles, order statistics. | Use only after matching `Rasm.Domain.StatKind` semantics. |
| [2] | `MathNet.Numerics.Statistics` | `ArrayStatistics` / `SortedArrayStatistics` | Allocation-light array and sorted-array statistics. | Preferred for hot managed numeric batches. |
| [3] | `MathNet.Numerics.Statistics` | `DescriptiveStatistics` / `RunningStatistics` | Snapshot and streaming descriptive stats. | Back Rasm accumulator records and streaming geometry descriptors. |
| [4] | `MathNet.Numerics.Statistics` | `Correlation`, `Histogram`, `Bucket`, `QuantileDefinition`, `RankDefinition` | Correlation matrices, bucket summaries, quantile and rank policy. | Useful for point-cloud descriptors; define bins and estimator policy first. |
| [5] | `MathNet.Numerics.Statistics` | `StreamingStatistics`, `MeanVariance`, `MeanStandardDeviation`, `Covariance`, `PopulationCovariance`, `QuantileCustom`, `QuantileRank`, `Entropy` | Exact estimator and streaming surfaces. | Encode estimator mode before exposing outputs. |
| [6] | `MathNet.Numerics.Distributions` | Continuous, discrete, and multivariate distributions. | Normal, beta, `BetaScaled`, gamma, Poisson, binomial, uniform, student-t, Burr, inverse Gaussian, Pareto, matrix normal, Wishart, inverse Wishart, Dirichlet, Normal-Gamma, Zipf, skewed generalized families. | Use with deterministic `RandomSource` only. |
| [7] | `MathNet.Numerics.Random` | `RandomSource`, `Xoshiro256StarStar`, Xorshift, Mersenne Twister, system sources, `CryptoRandomSource`. | Seeded and non-deterministic random generation. | Prefer `Xoshiro256StarStar` for graph-visible sampling; keep `CryptoRandomSource` out of replay-sensitive GH2 outputs. |
| [8] | `MathNet.Numerics.Statistics` | `RunningWeightedStatistics`, `WeightedDescriptiveStatistics`, `MovingStatistics` | Weighted, streaming, and moving-window summaries. | Use only with explicit weighting/window semantics. |
| [9] | `MathNet.Numerics.Statistics` | `KernelDensity`, `Mcmc` | Kernel density and Markov-chain sampling. | Use for probabilistic geometry descriptors and stochastic search workflows. |

---
## [5][ANALYSIS_ALGORITHMS]
>**Dictum:** *Algorithm APIs need convergence and mutation boundaries.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Numerics.Interpolation` | `Interpolate`, `CubicSpline`, `Barycentric`, `LinearSpline`, `StepInterpolation`, rational and polynomial interpolation, `QuadraticSpline`, `LogLinear`, `TransformedInterpolation`, `SplineBoundaryCondition`, `IInterpolation` | Interpolate sampled values. | Use for scalar fields when boundary semantics are explicit. |
| [2] | `MathNet.Numerics.Integration` | `Integrate`, Simpson, Newton-Cotes, Gauss-Legendre, double-exponential, `OnClosedInterval`, `OnRectangle`, `OnCuboid`, `GaussKronrodRule`, `GaussRule` | Numerical integration over scalar domains. | Map convergence failures into `Fin<T>`. |
| [3] | `MathNet.Numerics.RootFinding` | Bisection, Brent, Broyden, cubic, Newton-Raphson, secant-style roots, zero-crossing bracketing. | Scalar and system root solving. | Prefer `TryFindRoot` paths where available; validate brackets and convergence policy at Rasm boundary. |
| [4] | `MathNet.Numerics.Optimization` | `FindMinimum`, objective functions, minimizers, line search, least squares, result records, exceptions. | BFGS, BFGS-B, golden section, conjugate gradient, Newton, trust-region dogleg/Newton-CG, line searches, Nelder-Mead, Levenberg-Marquardt, `ExitCondition`. | Primary substrate for fitting, registration, field calibration, and solver tuning. |
| [5] | `MathNet.Numerics.Differentiation` | Numerical derivatives and gradients. | Finite-difference differentiation. | Prefer Rhino analytic derivatives when available. |
| [6] | `MathNet.Numerics.IntegralTransforms` | `Fourier`, `Hartley`, options and frequency scales. | Mutating FFT APIs: forward/inverse, real, 2D, multi-dimensional, and frequency scale helpers. | Treat as mutating/scale-sensitive until proven at call site. |
| [7] | `MathNet.Numerics.OdeSolvers` | `RungeKutta`, `AdamsBashforth` | Fixed-step ordinary differential equation integration. | Activate for vector-field tracing and iterative geometry growth; do not treat as adaptive event simulation. |

---
## [6][DATA_PROVIDERS]
>**Dictum:** *Data and provider packages are separate decisions.*

<br>

| [INDEX] | [PACKAGE] | [SURFACE] | [POSTURE] |
| :-----: | --------- | --------- | :-------: |
| [1] | Core provider controls | `MathNet.Numerics.Control`, `LinearAlgebraControl`, `FourierTransformControl`, `SparseSolverControl` | Core control and provider activation surfaces. |
| [2] | `MathNet.Numerics.Data.Text` | Matrix and vector text readers/writers. | Add when matrix import/export becomes product surface. |
| [3] | `MathNet.Numerics.Data.Matlab` | MATLAB matrix data interchange. | Add when MATLAB interchange becomes product surface. |
| [4] | `MathNet.Numerics.Providers.OpenBLAS` | Native BLAS/LAPACK provider adapter. | Add after package add, RhinoWIP macOS arm64 native-path load, and benchmarks. |
| [5] | `MathNet.Numerics.Providers.MKL` | Native MKL provider adapter. | Add after package add, RhinoWIP macOS arm64 native-path load, and benchmarks. |
| [6] | `MathNet.Numerics.Providers.CUDA` | CUDA provider adapter. | Ignore for macOS-only target. |
| [7] | `MathNet.Numerics.Providers.SparseSolver` | `ManagedSparseSolverProvider`, DSS types. | Use for sparse linear-system strategy, not sparse eigen ownership. |

---
## [7][SYMBOLICS]
>**Dictum:** *Symbolic math is an expression substrate, not Rhino geometry.*

<br>

| [INDEX] | [USING] | [SURFACE] | [FUNCTIONALITY] | [RASM_POSTURE] |
| :-----: | ------- | --------- | --------------- | -------------- |
| [1] | `MathNet.Symbolics` | `SymbolicExpression` | C#-friendly facade for construction, arithmetic, parsing, substitution, differentiation, formatting, evaluation, and compilation; includes `Expression`, `Type`, numeric value accessors, `VariableName`, operands, `FreeOf`, substitution, variable/function collection, summands, factors. | Preferred C# entrypoint behind future Rasm expression records. |
| [2] | `MathNet.Symbolics` | `Expression` | Lower-level F# discriminated expression tree. | Keep internal unless exhaustive symbolic shape inspection is required. |
| [3] | `MathNet.Symbolics` | `Infix` | `Parse`, `TryParse`, `ParseOrThrow`, `ParseOrUndefined`, `ParseVisual`, `Format`, visual format. | Use for user text only through `Fin<T>` parse rails. |
| [4] | `MathNet.Symbolics` | `MathML`, `LaTeX` | MathML parse/format, strict content XML, semantic annotations, LaTeX output. | Use for portable display/import/export, not core identity. |
| [5] | `MathNet.Symbolics` | `Algebraic`, `Rational`, `Polynomial` | Expand, factor, numerator/denominator, rationalize, reduce, polynomial variables, monomial/polynomial predicates, degrees, coefficients, leading coefficients, division, pseudo-division, GCD, square-free factorization, partial fractions. | Own as symbolic expression transforms. |
| [6] | `MathNet.Symbolics` | `Calculus` | `Differentiate`, `DifferentiateAt`, `Taylor`, `TangentLine`, `NormalLine`. | Use for analytic scalar expressions before numerical sampling. |
| [7] | `MathNet.Symbolics` | `Trigonometric`, `Exponential` | Expand, contract, simplify, substitute trig forms and exponential/log terms. | Keep transform choice explicit in operation names. |
| [8] | `MathNet.Symbolics` | `Evaluate`, `Compile`, `Approximate`, `FloatingPoint` | Evaluate to `FloatingPoint`, compile real/complex delegates, approximate real/complex/vector/matrix values, represent infinities and undefined. | Validate all outputs with Rhino/Rasm scalar validity before geometry use. |
| [9] | `MathNet.Symbolics` | `Structure`, `Operators`, `VariableSets.Alphabet` | Collect variables/functions/constants, substitute, fold/map, operator functions, common alphabet symbols. | Use for expression inspection and canonicalization only inside Rasm. |

---
## [8][RASM_INTEGRATION_RAILS]
>**Dictum:** *External math power stays direct; Rasm owns meaning and rails.*

<br>

| [INDEX] | [LIBRARY] | [OWNS] | [RASM_RULE] |
| :-----: | --------- | ----- | ----------- |
| [1] | MathNet.Numerics | Execution objects: matrices, vectors, factorizations, solvers, minimizers, distributions, transforms. | Use directly inside owning kernels; project only user-facing semantics into Rasm records. |
| [2] | MathNet.Symbolics | Formula parse trees, transforms, formatting, calculus, compilation. | Keep `SymbolicExpression` cached behind one normalized `Formula` value. |
| [3] | LanguageExt | `Fin<T>`, `Validation<Error,T>`, `Eff<RT,T>`. | Use at parse, admission, solver, evaluation, and Rhino conversion boundaries. |
| [4] | Thinktecture | Value objects, smart enums, unions, generated dispatch. | Encode formulas, symbol names, transform modes, output modes, solver modes, and typed result shapes. |
