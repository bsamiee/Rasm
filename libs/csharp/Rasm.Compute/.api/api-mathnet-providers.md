# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics` binds the Compute numeric lane's provider selection — the managed default and the RID-keyed native MKL and OpenBLAS adapters — and the dense factorization, sparse ingestion, and iterative-solve surface every Tensor, Stats, and Solver op folds onto one solve receipt. Provider selection resolves once at composition and governs every dense product; osx-arm64 carries no native asset and rides the managed provider. `CSparse` supplies the direct sparse Cholesky, LU, and QR lane beside the MathNet Krylov solvers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (MIT)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Storage`, `.LinearAlgebra.Factorization`, `.LinearAlgebra.Solvers`, `.LinearAlgebra.Double.Solvers`, `.Providers.LinearAlgebra`, `.IntegralTransforms`, `.Distributions`, `.Statistics`
- asset: managed runtime library; native MKL and OpenBLAS kernels ride the sibling provider packages
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.MKL`
- package: `MathNet.Numerics.Providers.MKL` (MIT)
- assembly: `MathNet.Numerics.Providers.MKL`
- namespace: `MathNet.Numerics.Providers.MKL`, `.Providers.MKL.LinearAlgebra`
- asset: managed provider adapter; native binaries ship in the x64 `MathNet.Numerics.MKL.Win-x64` and `.Linux-x64` asset packages, no osx-arm64 asset
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS` (MIT)
- assembly: `MathNet.Numerics.Providers.OpenBLAS`
- namespace: `MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra`
- asset: managed provider adapter; native binaries ship in the x64 OpenBLAS asset packages, no osx-arm64 asset
- rail: numeric

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse` (LGPL-2.1-only)
- assembly: `CSparse`
- namespace: `CSparse`, `.Double`, `.Double.Factorization`, `.Ordering`, `.Storage`
- asset: pure managed direct-sparse solvers
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider control, algebra carriers, iterative-solver, transform, and probability types

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------ | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Control`                             | class         | top-level provider and configuration façade                       |
|  [02]   | `LinearAlgebraControl`                | class         | provider-level selection over managed, MKL, and OpenBLAS          |
|  [03]   | `Matrix<double>`                      | class         | dense algebra carrier; `Multiply` routes the active provider      |
|  [04]   | `Vector<double>`                      | class         | dense vector carrier                                              |
|  [05]   | `SparseCompressedRowMatrixStorage<T>` | class         | the only native MathNet sparse form (CSR)                         |
|  [06]   | `Iterator<T>`                         | class         | stop-criteria control the Krylov solvers fold                     |
|  [07]   | `IIterationStopCriterion<T>`          | interface     | one iterative stop criterion                                      |
|  [08]   | `DiagonalPreconditioner`              | class         | diagonal preconditioner for the Krylov lane                       |
|  [09]   | `FourierOptions`                      | enum          | transform scaling convention                                      |
|  [10]   | `IContinuousDistribution`             | interface     | distribution seam: `RandomSource`, static and instance evaluation |
|  [11]   | `DescriptiveStatistics`               | class         | one-pass mean, variance, skewness, kurtosis carrier               |

[DENSE_FACTORIZATION]: `LU` `QR` `Cholesky` `Svd` `Evd`
[ITERATIVE_SOLVER]: `BiCgStab` `GpBiCg` `TFQMR` `MlkBiCgStab`
[DISTRIBUTION]: `Normal` `LogNormal` `ContinuousUniform` `Weibull` `Beta` `StudentT` `FisherSnedecor` `ChiSquared` `Gamma` `Poisson`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider selection and dense factorization

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :---------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Control.UseManaged()`                    | static   | select the managed provider                               |
|  [02]   | `Control.TryUseNativeMKL() -> bool`       | static   | select MKL, `false` on a missing native asset             |
|  [03]   | `Control.TryUseNativeOpenBLAS() -> bool`  | static   | select OpenBLAS, `false` on a missing native asset        |
|  [04]   | `Matrix<double>.Multiply(Matrix<double>)` | instance | GEMM through the active provider                          |
|  [05]   | `Matrix<double>.QR() -> QR<double>`       | instance | build a factorization; `LU`/`Cholesky`/`Svd`/`Evd` mirror |
|  [06]   | `QR<double>.Solve(Vector<double>)`        | instance | solve through the standing factor                         |

[ENTRYPOINT_SCOPE]: sparse CSR ingestion and iterative solve

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `OfCompressedSparseRowFormat(int, int, int, int[], int[], T[])`    | factory  | admit CSR; row pointers, column indices, values     |
|  [02]   | `OfCompressedSparseColumnFormat(int, int, int, int[], int[], T[])` | factory  | admit CSC                                           |
|  [03]   | `OfCoordinateFormat(int, int, int, int[], int[], T[])`             | factory  | admit COO triplets                                  |
|  [04]   | `OfIndexedEnumerable(int, int, IEnumerable<Tuple<int,int,T>>)`     | factory  | admit an indexed triplet sequence                   |
|  [05]   | `BiCgStab.Solve(Matrix, Vector, Iterator, IPreconditioner)`        | instance | Krylov solve; `GpBiCg`/`TFQMR`/`MlkBiCgStab` mirror |

- `OfCompressedSparseColumnFormat`: row INDICES precede column POINTERS — the argument-order trap; `OfIndexedEnumerable` carries a value-tuple `(int, int, T)` twin.

[ENTRYPOINT_SCOPE]: discrete Fourier transform

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------ | :------ | :---------------------------------------- |
|  [01]   | `Fourier.Forward(Complex[], FourierOptions)`                              | static  | in-place forward DFT, scaling-governed    |
|  [02]   | `Fourier.Inverse(Complex[], FourierOptions)`                              | static  | in-place inverse DFT, scaling-governed    |
|  [03]   | `Fourier.ForwardReal(double[], int, FourierOptions)`                      | static  | real-packed forward, half-spectrum `rfft` |
|  [04]   | `Fourier.InverseReal(double[], int, FourierOptions)`                      | static  | real-packed inverse                       |
|  [05]   | `Fourier.Forward2D` / `Inverse2D` / `ForwardMultiDim` / `InverseMultiDim` | static  | 2-D and N-D forward and inverse transform |
|  [06]   | `Fourier.FrequencyScale(int, double) -> double[]`                         | static  | the FFT-bin frequency axis                |

[ENTRYPOINT_SCOPE]: window tapers, each `Window.*` factory returning a `double[]` of the requested width

| [INDEX] | [SURFACE]                                                                              | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `Hann` / `HannPeriodic`                                                                | static  | Hann symmetric-periodic pair       |
|  [02]   | `Hamming` / `HammingPeriodic`                                                          | static  | Hamming symmetric-periodic pair    |
|  [03]   | `Cosine` / `CosinePeriodic` ; `Lanczos` / `LanczosPeriodic`                            | static  | Cosine and Lanczos periodic pairs  |
|  [04]   | `Blackman` / `BlackmanHarris` / `BlackmanNuttall`                                      | static  | Blackman family, no periodic split |
|  [05]   | `Dirichlet`                                                                            | static  | rectangular all-ones taper         |
|  [06]   | `Bartlett` / `BartlettHann` / `Tukey` / `FlatTop` / `Gauss` / `Nuttall` / `Triangular` | static  | single-form taper family           |

[ENTRYPOINT_SCOPE]: distribution evaluation and descriptive statistics

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `IContinuousDistribution.Sample() -> double`                       | instance | one draw; `Samples()` is the lazy `IEnumerable<double>` |
|  [02]   | `IContinuousDistribution.InverseCumulativeDistribution(double)`    | instance | quantile / PPF for inverse-transform sampling           |
|  [03]   | `IContinuousDistribution.CumulativeDistribution(double)`           | instance | CDF for reliability and failure-probability scoring     |
|  [04]   | `{Mean, Variance, StdDev, Median}`                                 | property | closed-form distribution moments                        |
|  [05]   | `Normal.CDF(double, double, double) -> double`                     | static   | static eval; each class mirrors `.CDF`/`.InvCDF`        |
|  [06]   | `Gamma.Density(double)` / `Poisson.Probability(int)`               | instance | PDF and PMF for the GLM deviance                        |
|  [07]   | `Statistics.Mean` / `Variance` / `StandardDeviation`               | static   | sample moments over an `IEnumerable<double>`            |
|  [08]   | `Statistics.MeanVariance(data)`                                    | static   | one-pass `(Mean, Variance)` tuple                       |
|  [09]   | `Statistics.Quantile(data, tau)` / `QuantileCustom` / `Percentile` | static   | sample quantile and percentile estimate                 |
|  [10]   | `Statistics.Covariance` / `Correlation.Pearson` / `Spearman`       | static   | pairwise covariance and correlation                     |
|  [11]   | `GoodnessOfFit.CoefficientOfDetermination(modelled, observed)`     | static   | R² fit quality; `RSquared` is the squared correlation   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Provider selection resolves once at composition through `LinearAlgebraControl`; the `Try*` forms return `false` on a missing native asset rather than throwing, and every dense `Matrix<double>.Multiply` routes through the active `ILinearAlgebraProvider`, so one selection governs every dense product. osx-arm64 carries no MKL or OpenBLAS asset and rides `UseManaged`.
- Dense factorization builds from `Matrix<double>` instance methods and solves through each factor's `Solve`; the numeric lane composes `Matrix<double>` and `Vector<double>` directly.
- `SparseCompressedRowMatrixStorage<T>` (CSR) is the only native MathNet sparse form; CSC, COO, and DOK are ingestion conversions through the `Of*` factories, never separate storage types.
- Every `Fourier` transform mutates the caller-owned `Complex[]` under one `FourierOptions` scaling — `Default` symmetric `1/√n` for a magnitude spectrum, `NoScaling` for an FFT-then-IFFT round-trip; `ForwardReal`/`InverseReal` carry the half-spectrum `rfft`, `Forward2D`/`ForwardMultiDim` the image and volume transforms.
- A paired taper's bare name is the symmetric filter-design form and its `*Periodic` twin the FFT-framing form; only `Hann`/`Hamming`/`Cosine`/`Lanczos` carry the pair. `Gauss(width, sigma)` takes sigma relative to the half-width `c = (width−1)/2` through `((i−c)/(σ·c))²`, so an absolute sample-count sigma flattens to rectangular; `Tukey(width, r)` takes the tapered fraction.
- Each `IContinuousDistribution` carries a `RandomSource` `System.Random` for seeded draws and exposes static `CDF`/`InvCDF`/`Sample` beside the instance `CumulativeDistribution`/`InverseCumulativeDistribution`/`Sample`.

[STACKING]:
- `MathNet.Numerics`(`libs/csharp/.api/api-mathnet-numerics.md`): the non-provider analytic surface — interpolation, root finding, optimization, special functions — stays there; this overlay owns provider selection and the factorization, solve, signal, and probability lane the Compute folder consumes.
- `CSparse`(`libs/csharp/.api/api-csparse.md`): a residual or stiffness operator assembled as `CompressedColumnStorage<double>` factors on the direct sparse `SparseCholesky`/`SparseLU`/`SparseQR` lane and solves in place, while the MathNet Krylov `BiCgStab`/`GpBiCg`/`TFQMR`/`MlkBiCgStab` under an `Iterator<T>` stop-criteria control and `DiagonalPreconditioner` cover the matrix-free peer; matrix density and factor reuse select among the three.
- Tensor/Stats/Solver: the numeric lane selects the provider once through `LinearProvider.Select()`, folds dense and sparse solves onto one `Factorization` `ComputeReceipt`, and claim-gates provider rank through `BenchmarkRow.Claim`.
- uncertainty lane: each `RandomVariable` case lowers onto one `IContinuousDistribution` (`Normal`/`LogNormal`/`ContinuousUniform`/`Weibull`/`Beta`); Monte-Carlo and LHS draw through `Sample`/`Samples` seeded from the owned `LowDiscrepancy` sequence, PCE fits its coefficients through the Tensor/blas least-squares/QR route, response moments fold through `Statistics.Mean`/`Variance`/`Quantile`, and the reliability index β is `Normal.InvCDF(1 − pf)` over the limit-state CDF.
- inference lane: each `StatisticalTest` reads its p-value from the matching CDF — `StudentT.CDF` for `t`/`welch-t`, `FisherSnedecor.CDF` for `anova`, `ChiSquared.CDF` for `chi-square`, `Normal.CDF` for the `mann-whitney` large-sample tail; the GLM IRLS loop reads variance and deviance from the response family (`Poisson.Probability`/`ProbabilityLn` for count, `Gamma.Density`/`DensityLn` for gamma), and `naive-bayes` fits per-class `Normal`/`Poisson` moments through `Statistics`.
- signal lane: the real `Tensor<float>` marshals into `Complex[]` once through the dispatch-lane Complex kernels under one consistent `FourierOptions` scaling; MathNet ships no DWT/wavelet and no analog-prototype IIR design, so the `dwt` QMF cascade and the Butterworth, Chebyshev, and elliptic bilinear design ground in-fence at the signal-lane gate.

[LOCAL_ADMISSION]:
- Numeric composition selects the provider once through `LinearProvider.Select()`; a per-call-site `Control.UseNativeMKL()` is the named defect.
- Sparse format is an ingestion discriminant over CSR-backed storage; the direct CSC factorization lane registers `CSparse` rather than re-tabling it.
- `Window.Dirichlet` is the rectangular all-ones taper, and `CumulativeDistribution` over the limit-state response gives the failure probability.

[RAIL_LAW]:
- Package: `MathNet.Numerics` (+ `.Providers.MKL`, `.Providers.OpenBLAS`), `CSparse`
- Owns: native-provider selection, dense and sparse BLAS-class factorization and iterative solve, the discrete Fourier transform and window taper family, and the probability distributions and descriptive statistics the uncertainty and inference lanes consume
- Accept: `Matrix<double>` and `Vector<double>` dense work, CSR ingestion through the `Of*` factories, direct and iterative solve, in-place `Fourier` over `Complex[]` with `Window` tapers, `IContinuousDistribution` sampling and CDF/PDF inference and `Statistics` moment, quantile, and correlation reduction
- Reject: a package-local matrix wrapper, a second provider selector beside `LinearProvider`, per-call-site provider switches, a re-implemented radix-2 or Bluestein FFT or a hand-rolled taper beside `Fourier`/`Window`, and a hand-rolled distribution sampler or moment accumulator beside the `Distributions`/`Statistics` surface
