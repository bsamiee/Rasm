# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics` supplies dense and sparse linear algebra, RID-keyed native-provider selection over MKL and OpenBLAS, CSR storage with CSC/COO/indexed ingestion conversions, matrix factorizations, `IntegralTransforms.Fourier` with `FourierOptions`, `Window` tapers, probability `Distributions`, and descriptive `Statistics`; `CSparse` supplies direct sparse Cholesky, LU, and QR beside MathNet iterative solvers. This catalog owns provider selection, dense algebra, factorization, sparse ingestion, iterative solve, signal transform, distribution, and statistics for Compute Tensor, Stats, and Solver; `libs/csharp/.api/api-mathnet-numerics.md` retains shared non-provider MathNet functions, and `libs/csharp/.api/api-csparse.md` retains direct sparse solvers.

## [01]-[PROVIDER_STACK]

[PROVIDER_STACK]: Compute numeric provider surface
- provider-selection, dense-algebra, sparse-algebra, and iterative-solver type rosters and their call-shape tables live in this catalog, including the `Svd`/`Evd`/`Cholesky`/`LU` factorization members, the concrete `IIterationStopCriterion<T>` rows, and `DiagonalPreconditioner`
- shared non-provider numerical functions stay in `libs/csharp/.api/api-mathnet-numerics.md`; direct sparse Cholesky/LU/QR facts stay in `libs/csharp/.api/api-csparse.md`
- rail: numeric

## [02]-[COMPUTE_SCOPES]

[ENTRYPOINT_SCOPE]: discrete Fourier transform + window
- rail: numeric

`Fourier` transforms in place over `Complex[]`/`Complex32[]` (the signal lane marshals the real `Tensor<float>` into `Complex[]` once); `FourierOptions.Default` is symmetric `1/√n` scaling, `NoScaling` is the FFT-then-IFFT round-trip, `AsymmetricScaling`/`Matlab` matches MATLAB. `Window` factories return a `double[]` taper of the requested width; only `Hann`/`Hamming`/`Cosine`/`Lanczos` carry a `*Periodic` twin (symmetric for filter design, periodic for FFT framing) — every other taper, `Bartlett` and `BartlettHann` included, ships one form only.

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------- |
|  [01]   | `Fourier.Forward(Complex[], FourierOptions)`                              | in-place forward DFT, scaling-governed         |
|  [02]   | `Fourier.Inverse(Complex[], FourierOptions)`                              | in-place inverse DFT, scaling-governed         |
|  [03]   | `Fourier.ForwardReal(double[], int, FourierOptions)`                      | real-packed forward (half-spectrum `rfft`)     |
|  [04]   | `Fourier.InverseReal(double[], int, FourierOptions)`                      | real-packed inverse                            |
|  [05]   | `Fourier.Forward2D` / `Inverse2D` / `ForwardMultiDim` / `InverseMultiDim` | 2-D / N-D forward + inverse transform          |
|  [06]   | `Fourier.FrequencyScale(int length, double sampleRate)`                   | the FFT-bin frequency axis (`static double[]`) |

[ENTRYPOINT_SCOPE]: window tapers — `Window.*` factories return a `double[]`; only `Hann`/`Hamming`/`Cosine`/`Lanczos` carry a `*Periodic` twin
- rail: numeric

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Hann` / `HannPeriodic`                                                                | Hann symmetric + FFT-periodic pair            |
|  [02]   | `Hamming` / `HammingPeriodic`                                                          | Hamming symmetric + FFT-periodic pair         |
|  [03]   | `Cosine` / `CosinePeriodic` ; `Lanczos` / `LanczosPeriodic`                            | Cosine + Lanczos symmetric + periodic pairs   |
|  [04]   | `Blackman` / `BlackmanHarris` / `BlackmanNuttall`                                      | Blackman family, no periodic split            |
|  [05]   | `Dirichlet`                                                                            | rectangular all-ones taper (over hand-rolled) |
|  [06]   | `Bartlett` / `BartlettHann` / `Tukey` / `FlatTop` / `Gauss` / `Nuttall` / `Triangular` | single-form taper family                      |

`Gauss(int width, double sigma)` takes sigma RELATIVE to the half-width — the exponent is `((i−c)/(σ·c))²` with `c = (width−1)/2` — so an absolute sample-count sigma flattens the taper to rectangular; `Tukey(int width, double r = 0.5)` takes the taper fraction.

[ENTRYPOINT_SCOPE]: probability distributions — namespace `MathNet.Numerics.Distributions`
- rail: numeric

`Distributions` and `Statistics` ship inside `MathNet.Numerics`; uncertainty samples forward-UQ distributions, estimator rows fit per-class moments and read IRLS variance functions, and hypothesis rows read inference CDFs. Each `IContinuousDistribution` carries a `RandomSource` `System.Random` for seeded draws and exposes static `CDF`/`InvCDF`/`Sample` forms beside instance `CumulativeDistribution`/`InverseCumulativeDistribution`/`Sample`.

| [INDEX] | [SYMBOL]                                                   | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `Normal(mean, stddev)`                                     | Gaussian continuous distribution                                 |
|  [02]   | `LogNormal(mu, sigma)`                                     | log-normal continuous distribution                               |
|  [03]   | `ContinuousUniform(lower, upper)`                          | uniform continuous distribution                                  |
|  [04]   | `Weibull(shape, scale)`                                    | Weibull reliability distribution                                 |
|  [05]   | `Beta(a, b)`                                               | Beta continuous distribution                                     |
|  [06]   | `IContinuousDistribution.Sample()`                         | one draw; `Samples()` is the lazy `IEnumerable<double>` stream   |
|  [07]   | `IContinuousDistribution.InverseCumulativeDistribution(p)` | quantile / PPF for inverse-transform sampling                    |
|  [08]   | `IContinuousDistribution.CumulativeDistribution(x)`        | CDF for reliability / failure-probability scoring                |
|  [09]   | `{Mean, Variance, StdDev, Median}`                         | closed-form distribution moments                                 |
|  [10]   | `StudentT(location, scale, freedom)` `.CDF`/`.InvCDF`      | t-test p-value CDF (the `t`/`welch-t` test statistic)            |
|  [11]   | `FisherSnedecor(d1, d2)` `.CDF`/`.InvCDF`                  | F-distribution CDF (the `anova` test statistic)                  |
|  [12]   | `ChiSquared(freedom)` `.CDF`/`.InvCDF`                     | χ² CDF (the `chi-square` test statistic)                         |
|  [13]   | `Gamma(shape, rate)` `.CDF`/`.Density`                     | Gamma CDF/PDF (GLM-Gamma deviance + `glm-gamma` IRLS variance)   |
|  [14]   | `Poisson(lambda)` `.CumulativeDistribution`/`.Probability` | discrete Poisson CDF/PMF (GLM-Poisson + `naive-bayes` per-class) |

[ENTRYPOINT_SCOPE]: descriptive statistics — namespace `MathNet.Numerics.Statistics`
- rail: numeric

| [INDEX] | [SYMBOL]                                                           | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Statistics.Mean` / `Variance` / `StandardDeviation`               | sample moments over an `IEnumerable<double>`          |
|  [02]   | `Statistics.MeanVariance(data)`                                    | one-pass `(double Mean, double Variance)` tuple       |
|  [03]   | `Statistics.Quantile(data, tau)` / `QuantileCustom` / `Percentile` | sample quantile / percentile estimate                 |
|  [04]   | `Statistics.Covariance` / `Correlation.Pearson` / `Spearman`       | pairwise covariance / Pearson + Spearman correlation  |
|  [05]   | `DescriptiveStatistics(data)`                                      | one-pass mean/variance/skewness/kurtosis carrier      |
|  [06]   | `GoodnessOfFit.CoefficientOfDetermination(modelled, observed)`     | R² fit quality; `RSquared` is the squared correlation |

## [03]-[IMPLEMENTATION_LAW]

[SHARED_PROVIDER_SURFACE]:
- source: `MathNet.Numerics`, `MathNet.Numerics.Providers.MKL`, `MathNet.Numerics.Providers.OpenBLAS`, `CSparse`
- provider selection, dense factorization, sparse ingestion, and iterative-solver rosters remain the Compute numeric-lane substrate row.

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (-beta2; license MIT)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Storage`, `.LinearAlgebra.Factorization`, `.LinearAlgebra.Solvers`, `.LinearAlgebra.Double.Solvers`, `.Providers.LinearAlgebra`
- asset: runtime library (managed; native providers ride sibling asset packages)
- floor: net8.0 (no net10/net9 lib in -beta2; the net10 consumer binds `lib/net8.0`)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.MKL`
- package: `MathNet.Numerics.Providers.MKL` (-beta2)
- assembly: `MathNet.Numerics.Providers.MKL`
- namespace: `MathNet.Numerics.Providers.MKL`, `.Providers.MKL.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in `MathNet.Numerics.MKL.Win-x64` / `.Linux-x64`; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS` (-beta2)
- assembly: `MathNet.Numerics.Providers.OpenBLAS`
- namespace: `MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in platform OpenBLAS asset packages; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse` (full direct-sparse surface in `api-csparse`)
- assembly: `CSparse`
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Ordering`, `CSparse.Storage`
- asset: runtime library (pure managed direct sparse solvers; LGPL-2.1-only)
- rail: numeric

[PROVIDER_SELECTION]:
- namespace: `MathNet.Numerics`, `MathNet.Numerics.Providers.LinearAlgebra`
- façade: `Control` (top-level), `LinearAlgebraControl` (provider-level)
- selection: `UseManaged`, `TryUseNativeMKL`, `TryUseNativeOpenBLAS` — the `Try*` variants return `false` instead of throwing on missing native assets
- RID reality: MKL native assets are x64-only (`MathNet.Numerics.MKL.Win-x64` / `.Linux-x64`); no `MathNet.Numerics.MKL.OSX` or `OpenBLAS.OSX` package exists; osx-arm64 falls back to `UseManaged`
- version track: core `MathNet.Numerics 6.0.0-beta2`, `Providers.MKL 6.0.0-beta2`, `Providers.OpenBLAS 6.0.0-beta2`, native asset packages ship the MKL/OpenBLAS binaries on x64 RIDs

[DENSE_ALGEBRA]:
- namespace: `MathNet.Numerics.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Factorization`
- carrier: `Matrix<double>` / `Vector<double>` — the numeric-lane composes these directly, never a package-local matrix wrapper
- factorizations: `LU`, `QR`, `Cholesky`, `Svd`, `Evd` build from `Matrix<T>` instance methods and solve through their `Solve` members
- GEMM: `Matrix<T>.Multiply` routes through the active `ILinearAlgebraProvider`, so provider selection governs every dense product

[SPARSE_SOLVE]:
- namespace: `MathNet.Numerics.LinearAlgebra.Storage`, `CSparse.Double.Factorization`
- storage: `SparseCompressedRowMatrixStorage<T>` is the only native MathNet sparse matrix form (CSR); CSC/COO/DOK are ingestion conversions via the `Of*` factories, never separate storage types
- ingestion factories (decompile-verified): `OfCompressedSparseRowFormat(int rows, int columns, int valueCount, int[] rowPointers, int[] columnIndices, T[] values)`, `OfCompressedSparseColumnFormat(int rows, int columns, int valueCount, int[] rowIndices, int[] columnPointers, T[] values)` — row INDICES precede column POINTERS, the argument-order trap — `OfCoordinateFormat(int rows, int columns, int valueCount, int[] rowIndices, int[] columnIndices, T[] values)`, and `OfIndexedEnumerable(int rows, int columns, IEnumerable<Tuple<int,int,T>>)` with a value-tuple `(int, int, T)` twin
- direct solvers: `CSparse.Double.Factorization.SparseCholesky`/`SparseLU`/`SparseQR` factor a CSparse `CompressedColumnStorage<double>` (CSC) and solve in place
- iterative solvers: `BiCgStab`/`GpBiCg`/`TFQMR`/`MlkBiCgStab` over MathNet sparse matrices with an `Iterator<T>` stop-criteria control

[SIGNAL_TRANSFORM]:
- namespace: `MathNet.Numerics.IntegralTransforms`, `MathNet.Numerics`
- transform: `Fourier.Forward`/`Inverse` mutate a `Complex[]` in place; the signal lane marshals the real `Tensor<float>` into `Complex[]` once through the dispatch-lane Complex kernels and applies one consistent `FourierOptions` scaling — `Default` symmetric `1/√n` for a magnitude spectrum, `NoScaling` for an FFT-then-IFFT round-trip — never re-implementing the radix-2/Bluestein kernel
- real packing: `ForwardReal`/`InverseReal` carry the half-spectrum `rfft`; `Forward2D`/`ForwardMultiDim` carry the image/volume transforms; `FrequencyScale(length, sampleRate)` is the FFT-bin frequency axis
- window: only `Window.{Hann,Hamming,Cosine,Lanczos}` ship both a symmetric and a `*Periodic` form (filter-design symmetric, FFT framing periodic); `Window.{Bartlett,BartlettHann,Blackman,BlackmanHarris,BlackmanNuttall,Dirichlet,FlatTop,Gauss,Nuttall,Tukey,Triangular}` ship one form only (`Bartlett` has no `BartlettPeriodic` — `BartlettHann` is a distinct window, not its periodic twin) — the rectangular window is `Window.Dirichlet` (all-ones), never a hand-rolled `Enumerable.Repeat`
- not shipped: MathNet has no DWT/wavelet surface and no analog-prototype IIR design — the `dwt` QMF cascade and the Butterworth/Chebyshev/elliptic bilinear design ground in-fence at the signal-lane design gate; only the FFT and window tapers ride this package

[LOCAL_ADMISSION]:
- Numeric composition selects the provider once through `LinearProvider.Select()`; per-call-site `Control.UseNativeMKL()` is rejected.
- Dense and sparse solves emit the `Factorization` `ComputeReceipt` case; provider rank is claim-gated through `BenchmarkRow.Claim`, never a static default.
- Sparse format is an ingestion discriminant over CSR-backed storage, not four storage types.

[UNCERTAINTY_LAW]:
- namespace: `MathNet.Numerics.Distributions`, `MathNet.Numerics.Statistics`
- random variables: each `RandomVariable` union case lowers onto one `IContinuousDistribution` (`Normal`/`LogNormal`/`ContinuousUniform`/`Weibull`/`Beta`); the `empirical` case samples its provided CDF through inverse-transform over the owned `LowDiscrepancy` draw
- propagation: Monte-Carlo and LHS draw through `Sample()`/`Samples()` seeded from the owned `LowDiscrepancy` low-discrepancy sequence (never a per-call fresh `System.Random`); PCE fits the orthogonal-polynomial coefficients through the `Tensor/blas#DENSE_ALGEBRA` least-squares/QR route
- reduction: response moments fold through `Statistics.Mean`/`Variance` and `Statistics.Quantile`, never a hand-rolled accumulator; the failure probability is `CumulativeDistribution` over the limit-state response and the reliability index β is `Normal.InvCDF(1 - pf)`
- Reject: a per-call fresh `System.Random` seed beside the owned sampler, a hand-rolled moment accumulator beside `Statistics`, an in-process distribution-learning loop (the learned input distribution is the offline-science companion's)

[INFERENCE_LAW]:
- namespace: `MathNet.Numerics.Distributions`, `MathNet.Numerics.Statistics`
- hypothesis tests: each `StatisticalTest` row reads its p-value from the matching CDF — `StudentT.CDF` for `t`/`welch-t`, `FisherSnedecor.CDF` for `anova`, `ChiSquared.CDF` for `chi-square`, `Normal.CDF` for the large-sample `mann-whitney` normal approximation — never a hand-derived error function; the `ks` Kolmogorov statistic is distribution-free and grounds its asymptotic in-fence
- GLM variance: the IRLS inner loop reads the variance function and deviance from the response-family distribution — binomial through `Logit`, `Poisson.Probability`/`ProbabilityLn` for the count deviance, `Gamma.Density`/`DensityLn` for the gamma deviance — so the GLM fit-quality metric is the package deviance, not a hand-coded link
- descriptive reduction: the estimator lane folds `Statistics.Mean`/`Variance`/`Covariance`/`Correlation.Pearson` for the moment/correlation features and `Statistics.Quantile` for the robust statistics, and the `naive-bayes` row fits per-class `Normal`/`Poisson` moments through `Statistics` rather than a hand-rolled Gaussian
- Reject: a hand-derived error function or incomplete-beta/gamma reimplementation beside the `Distributions` CDF, a hand-rolled Welford accumulator beside `Statistics`, a per-algorithm distribution class beside the one MathNet distribution surface

[RAIL_LAW]:
- Package: `MathNet.Numerics` (+ `.Providers.MKL`, `.Providers.OpenBLAS`), `CSparse`
- Owns: dense + sparse BLAS-class algebra, native-provider selection, matrix factorization, the discrete Fourier transform + window taper family, probability distributions + descriptive statistics
- Accept: `Matrix<double>`/`Vector<double>` dense work, CSR/CSC/COO/DOK sparse ingestion, direct + iterative solve, in-place `Fourier` transform over `Complex[]` with `Window` tapers, `IContinuousDistribution` sampling + `Distributions` CDF/PDF inference + `Statistics` moment/quantile/correlation reduction
- Reject: a package-local matrix wrapper face, a second provider selector beside `LinearProvider`, per-call-site provider switches, a re-implemented radix-2/Bluestein FFT or hand-rolled cosine/rectangular taper beside `Fourier`/`Window`, a hand-rolled distribution sampler, error-function reimplementation, or moment accumulator beside the `Distributions`/`Statistics` surface
