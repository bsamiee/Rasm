# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics` supplies dense and sparse linear algebra, the RID-keyed
native-provider selection façade over MKL and OpenBLAS, the CSR sparse storage
surface with its CSC/COO/indexed ingestion conversions, the matrix
factorization family, the in-assembly `IntegralTransforms.Fourier` discrete
Fourier transform with its `FourierOptions` scaling and `Window` taper family the
signal lane marshals and windows, and the in-assembly probability
`Distributions` and descriptive `Statistics` surfaces the uncertainty,
estimator, and hypothesis-test lanes sample, reduce, and read CDFs from;
`CSparse` supplies direct sparse Cholesky, LU, and QR factorizations beside the
MathNet iterative solvers for the numeric lane. This catalog owns provider
selection, dense algebra, factorization, sparse ingestion, iterative solve,
signal transform, distribution, and statistics scopes for the Compute Tensor,
Stats, and Solver lanes; `libs/csharp/.api/api-mathnet-numerics.md` retains the
shared non-provider MathNet functions, and `libs/csharp/.api/api-csparse.md`
retains the direct sparse-solver catalog.

## [01]-[PROVIDER_STACK]

[PROVIDER_STACK]: Compute numeric provider surface
- provider-selection, dense-algebra, sparse-algebra, and iterative-solver type rosters and their call-shape tables live in this catalog, including the `Svd`/`Evd`/`Cholesky`/`LU` factorization members, the concrete `IIterationStopCriterion<T>` rows, and `DiagonalPreconditioner`
- shared non-provider numerical functions stay in `libs/csharp/.api/api-mathnet-numerics.md`; direct sparse Cholesky/LU/QR facts stay in `libs/csharp/.api/api-csparse.md`
- rail: numeric

## [02]-[COMPUTE_SCOPES]

[ENTRYPOINT_SCOPE]: discrete Fourier transform + window
- rail: numeric

`Fourier` transforms in place over `Complex[]`/`Complex32[]` (the signal lane marshals the real `Tensor<float>` into `Complex[]` once); `FourierOptions.Default` is symmetric `1/√n` scaling, `NoScaling` is the FFT-then-IFFT round-trip, `AsymmetricScaling`/`Matlab` matches MATLAB. `Window` factories return a `double[]` taper of the requested width; only `Hann`/`Hamming`/`Cosine`/`Lanczos` carry a `*Periodic` twin (symmetric for filter design, periodic for FFT framing) — every other taper, `Bartlett` and `BartlettHann` included, ships one form only.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]      | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `Fourier.Forward(Complex[], FourierOptions)`                                 | static `void`     | in-place forward DFT, scaling-governed            |
|  [02]   | `Fourier.Inverse(Complex[], FourierOptions)`                                 | static `void`     | in-place inverse DFT, scaling-governed            |
|  [03]   | `Fourier.ForwardReal(double[], int, FourierOptions)`                         | static `void`     | real-packed forward (half-spectrum `rfft`)        |
|  [04]   | `Fourier.InverseReal(double[], int, FourierOptions)`                         | static `void`     | real-packed inverse                               |
|  [05]   | `Fourier.Forward2D` / `Inverse2D` / `ForwardMultiDim` / `InverseMultiDim`    | static `void`     | 2-D / N-D forward + inverse transform             |
|  [06]   | `Fourier.FrequencyScale(int length, double sampleRate)`                      | static `double[]` | the FFT-bin frequency axis                        |
|  [07]   | `Window.Hann` / `HannPeriodic`                                               | static `double[]` | Hann symmetric / FFT-periodic taper               |
|  [08]   | `Window.Hamming` / `HammingPeriodic`                                         | static `double[]` | Hamming symmetric / FFT-periodic taper            |
|  [09]   | `Window.Cosine` / `CosinePeriodic` ; `Window.Lanczos` / `LanczosPeriodic`    | static `double[]` | Cosine / Lanczos symmetric + FFT-periodic taper   |
|  [10]   | `Window.Blackman` / `BlackmanHarris` / `BlackmanNuttall`                     | static `double[]` | Blackman family taper (no periodic split)         |
|  [11]   | `Window.Dirichlet`                                                           | static `double[]` | rectangular all-ones taper (use over hand-rolled) |
|  [12]   | `Window.Bartlett` / `BartlettHann` / `Tukey` / `FlatTop` / `Gauss` / `Nuttall` / `Triangular` | static `double[]` | the remaining single-form taper family            |

[ENTRYPOINT_SCOPE]: probability distributions + descriptive statistics
- rail: numeric

The `Distributions` and `Statistics` surfaces ship inside the admitted `MathNet.Numerics` assembly (no separate package); the uncertainty lane samples the forward-UQ continuous distributions, the estimator lane fits per-class moments and reads the IRLS variance function, and the hypothesis-test lane reads the test-statistic CDF from the inference distributions. Each `IContinuousDistribution` carries a `RandomSource` `System.Random` for seeded draws; each distribution also exposes the static `CDF`/`InvCDF`/`Sample` form (parameters + value) beside the instance `CumulativeDistribution`/`InverseCumulativeDistribution`/`Sample`.

| [INDEX] | [SYMBOL]                                                           | [NAMESPACE]                      | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------------------------------- | :------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `Normal(mean, stddev)`                                             | `MathNet.Numerics.Distributions` | Gaussian continuous distribution                                 |
|  [02]   | `LogNormal(mu, sigma)`                                             | `MathNet.Numerics.Distributions` | log-normal continuous distribution                               |
|  [03]   | `ContinuousUniform(lower, upper)`                                  | `MathNet.Numerics.Distributions` | uniform continuous distribution                                  |
|  [04]   | `Weibull(shape, scale)`                                            | `MathNet.Numerics.Distributions` | Weibull reliability distribution                                 |
|  [05]   | `Beta(a, b)`                                                       | `MathNet.Numerics.Distributions` | Beta continuous distribution                                     |
|  [06]   | `IContinuousDistribution.Sample()`                                 | `MathNet.Numerics.Distributions` | one draw; `Samples()` is the lazy `IEnumerable<double>` stream   |
|  [07]   | `IContinuousDistribution.InverseCumulativeDistribution(p)`         | `MathNet.Numerics.Distributions` | quantile / PPF for inverse-transform sampling                    |
|  [08]   | `IContinuousDistribution.CumulativeDistribution(x)`                | `MathNet.Numerics.Distributions` | CDF for reliability / failure-probability scoring                |
|  [09]   | `{Mean, Variance, StdDev, Median}`                                 | `MathNet.Numerics.Distributions` | closed-form distribution moments                                 |
|  [10]   | `Statistics.Mean` / `Variance` / `StandardDeviation`               | `MathNet.Numerics.Statistics`    | sample moments over an `IEnumerable<double>`                     |
|  [11]   | `Statistics.Quantile(data, tau)` / `QuantileCustom` / `Percentile` | `MathNet.Numerics.Statistics`    | sample quantile / percentile estimate                            |
|  [12]   | `Statistics.Covariance` / `Correlation.Pearson` / `Spearman`       | `MathNet.Numerics.Statistics`    | pairwise covariance / Pearson + Spearman correlation             |
|  [13]   | `DescriptiveStatistics(data)`                                      | `MathNet.Numerics.Statistics`    | one-pass mean/variance/skewness/kurtosis carrier                 |
|  [14]   | `StudentT(location, scale, freedom)` `.CDF`/`.InvCDF`              | `MathNet.Numerics.Distributions` | t-test p-value CDF (the `t`/`welch-t` test statistic)            |
|  [15]   | `FisherSnedecor(d1, d2)` `.CDF`/`.InvCDF`                          | `MathNet.Numerics.Distributions` | F-distribution CDF (the `anova` test statistic)                  |
|  [16]   | `ChiSquared(freedom)` `.CDF`/`.InvCDF`                             | `MathNet.Numerics.Distributions` | χ² CDF (the `chi-square` test statistic)                         |
|  [17]   | `Gamma(shape, rate)` `.CDF`/`.Density`                             | `MathNet.Numerics.Distributions` | Gamma CDF/PDF (GLM-Gamma deviance + `glm-gamma` IRLS variance)   |
|  [18]   | `Poisson(lambda)` `.CumulativeDistribution`/`.Probability`         | `MathNet.Numerics.Distributions` | discrete Poisson CDF/PMF (GLM-Poisson + `naive-bayes` per-class) |

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
- direct solvers: `CSparse.Double.Factorization.SparseCholesky`/`SparseLU`/`SparseQR` factor a CSparse `CompressedColumnStorage<double>` (CSC) and solve in place
- iterative solvers: `BiCgStab`/`GpBiCg`/`TFQMR`/`MlkBiCgStab` over MathNet sparse matrices with an `Iterator<T>` stop-criteria control

[SIGNAL_TRANSFORM]:
- namespace: `MathNet.Numerics.IntegralTransforms`, `MathNet.Numerics`
- transform: `Fourier.Forward`/`Inverse` mutate a `Complex[]` in place; the signal lane marshals the real `Tensor<float>` into `Complex[]` once through the dispatch-lane Complex kernels and applies one consistent `FourierOptions` scaling — `Default` symmetric `1/√n` for a magnitude spectrum, `NoScaling` for an FFT-then-IFFT round-trip — never re-implementing the radix-2/Bluestein kernel
- real packing: `ForwardReal`/`InverseReal` carry the half-spectrum `rfft`; `Forward2D`/`ForwardMultiDim` carry the image/volume transforms; `FrequencyScale(length, sampleRate)` is the FFT-bin frequency axis
- window: only `Window.{Hann,Hamming,Cosine,Lanczos}` ship both a symmetric and a `*Periodic` form (filter-design symmetric, FFT framing periodic); `Window.{Bartlett,BartlettHann,Blackman,BlackmanHarris,BlackmanNuttall,Dirichlet,FlatTop,Gauss,Nuttall,Tukey,Triangular}` ship one form only (`Bartlett` has no `BartlettPeriodic` — `BartlettHann` is a distinct window, not its periodic twin) — the rectangular window is `Window.Dirichlet` (all-ones), never a hand-rolled `Enumerable.Repeat`
- not shipped: MathNet has no DWT/wavelet surface and no analog-prototype IIR design — the `dwt` QMF cascade and the Butterworth/Chebyshev/elliptic bilinear design ground in-fence at the signal-lane design gate; only the FFT and window tapers ride this package

[LOCAL_ADMISSION]:
- The numeric lane selects the provider once at composition through `LinearProvider.Select()`; a per-call-site `Control.UseNativeMKL()` is the named defect.
- Dense and sparse solves emit the `Factorization` `ComputeReceipt` case; provider rank is claim-gated through `BenchmarkRow.Claim`, never a static default.
- The sparse format axis is an ingestion discriminant over CSR-backed storage, not four storage types.

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
