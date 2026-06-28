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
MathNet iterative solvers for the numeric lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics`
- version: `6.0.0-beta2`
- assembly: `MathNet.Numerics`
- license: MIT/X11
- bound asset: `lib/net8.0/MathNet.Numerics.dll` (ships `net48`/`net6.0`/`net8.0`/`netstandard2.0`; no `net10.0` asset, so consumer `net10.0` binds `net8.0`)
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `MathNet.Numerics.LinearAlgebra.Double`, `MathNet.Numerics.LinearAlgebra.Storage`, `MathNet.Numerics.LinearAlgebra.Factorization`, `MathNet.Numerics.Providers.LinearAlgebra`, `MathNet.Numerics.IntegralTransforms`, `MathNet.Numerics.Distributions`, `MathNet.Numerics.Statistics`
- asset: runtime library (managed; native providers ride sibling asset packages)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.MKL`
- package: `MathNet.Numerics.Providers.MKL`
- version: `6.0.0-beta2`
- assembly: `MathNet.Numerics.Providers.MKL`
- license: MIT/X11 (managed adapter; the native MKL binary carries the Intel Simplified Software License)
- namespace: `MathNet.Numerics.Providers.MKL.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in `MathNet.Numerics.MKL.Win-x64` / `MathNet.Numerics.MKL.Linux-x64`; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS`
- version: `6.0.0-beta2`
- assembly: `MathNet.Numerics.Providers.OpenBLAS`
- license: MIT/X11 (managed adapter; the native OpenBLAS binary carries the BSD-3-Clause license)
- namespace: `MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in platform OpenBLAS asset packages; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse`
- version: `4.4.0`
- assembly: `CSparse`
- license: LGPL-2.1-or-later
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Factorization`, `CSparse.Ordering`, `CSparse.Storage`
- asset: runtime library (pure managed direct sparse solvers)
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :----------------------------- | :------------- | :----------------------------------- |
|  [01]   | `Control`                      | static façade  | selects + probes the active provider |
|  [02]   | `LinearAlgebraControl`         | static façade  | provider-level direct selection API  |
|  [03]   | `ILinearAlgebraProvider`       | provider seam  | the active provider handle           |
|  [04]   | `MklLinearAlgebraControl`      | provider type  | MKL native adapter control           |
|  [05]   | `OpenBlasLinearAlgebraControl` | provider type  | OpenBLAS native adapter control      |

[PUBLIC_TYPE_SCOPE]: dense algebra
- rail: numeric

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :------------- | :----------------------------- |
|  [01]   | `Matrix<T>`                        | dense matrix   | dense matrix value carrier     |
|  [02]   | `Vector<T>`                        | dense vector   | dense vector value carrier     |
|  [03]   | `Matrix<double>` (`Double.Matrix`) | dense matrix   | the numeric-lane dense carrier |
|  [04]   | `LU<T>`                            | factorization  | LU decomposition + solve       |
|  [05]   | `QR<T>`                            | factorization  | QR decomposition + solve       |
|  [06]   | `Cholesky<T>`                      | factorization  | Cholesky decomposition + solve |
|  [07]   | `Svd<T>`                           | factorization  | singular value decomposition   |
|  [08]   | `Evd<T>`                           | factorization  | eigenvalue decomposition       |
|  [09]   | `DenseColumnMajorMatrixStorage<T>` | dense storage  | column-major dense backing     |
|  [10]   | `DenseVectorStorage<T>`            | dense storage  | dense vector backing           |

[PUBLIC_TYPE_SCOPE]: sparse algebra
- rail: numeric

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `SparseCompressedRowMatrixStorage<T>`                        | sparse storage | CSR matrix backing (only native form) |
|  [02]   | `SparseVectorStorage<T>`                                     | sparse storage | COO-style sparse vector backing       |
|  [03]   | `CSparse.Storage.CompressedColumnStorage<T>`                 | csc storage    | CSparse CSC matrix backing            |
|  [04]   | `CSparse.Double.SparseMatrix`                                | sparse matrix  | CSparse double CSC matrix             |
|  [05]   | `CSparse.Double.Factorization.SparseCholesky`                | factorization  | direct sparse Cholesky                |
|  [06]   | `CSparse.Double.Factorization.SparseLU`                      | factorization  | direct sparse LU                      |
|  [07]   | `CSparse.Double.Factorization.SparseQR`                      | factorization  | direct sparse QR                      |
|  [08]   | `CSparse.ColumnOrdering`                                     | ordering enum  | fill-reducing ordering selector       |
|  [09]   | `MathNet.Numerics.LinearAlgebra.Solvers.IIterativeSolver<T>` | solver seam    | iterative-solve seam                  |
|  [10]   | `MathNet.Numerics.LinearAlgebra.Double.SparseMatrix`         | sparse matrix  | MathNet CSR-backed sparse matrix (derives `Matrix<double>`, so the `Multiply`/`Transpose`/`Add`/`KroneckerProduct` ops apply); distinct from the CSparse CSC `SparseMatrix` |
|  [11]   | `CSparse.Ordering.AMD`                                       | ordering kernel | approximate-minimum-degree fill-reducing permutation generator over a CSC matrix |

[PUBLIC_TYPE_SCOPE]: iterative solvers
- rail: numeric

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE] | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab`    | solver         | biconjugate gradient stabilized |
|  [02]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.GpBiCg`      | solver         | generalized product BiCG        |
|  [03]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.TFQMR`       | solver         | transpose-free QMR              |
|  [04]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.MlkBiCgStab` | solver         | multiple-Lanczos BiCGStab       |
|  [05]   | `MathNet.Numerics.LinearAlgebra.Solvers.Iterator<T>`        | control        | iteration stop criteria         |

[PUBLIC_TYPE_SCOPE]: signal transform + window
- rail: numeric

The `IntegralTransforms.Fourier` static surface owns the in-place DFT over `Complex[]`/`Complex32[]`, the real-packed `ForwardReal`/`InverseReal`, the 2-D and N-D transforms, and `FrequencyScale`; `Window` is the static taper family the signal lane reads by `WindowKind` row. MathNet ships no wavelet (`dwt`) or analog-prototype IIR design surface — those ground in-fence at the signal-lane design gate.

| [INDEX] | [SYMBOL]                                             | [PACKAGE_ROLE] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `MathNet.Numerics.IntegralTransforms.Fourier`        | static DFT     | in-place forward/inverse FFT family            |
|  [02]   | `MathNet.Numerics.IntegralTransforms.FourierOptions` | scaling enum   | symmetric/asymmetric/no-scaling convention     |
|  [03]   | `MathNet.Numerics.Window`                            | static tapers  | window function family (Hann/Hamming/Blackman) |
|  [04]   | `System.Numerics.Complex` / `Complex32`              | sample carrier | the in-place transform value type              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]    | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `Control.UseManaged`                            | static `void`   | selects the pure-managed provider                        |
|  [02]   | `Control.UseNativeMKL` / `TryUseNativeMKL`      | static `void`/`bool` | selects MKL; `Try*` returns `false` on load failure |
|  [03]   | `Control.UseNativeOpenBLAS` / `TryUseNativeOpenBLAS` | static `void`/`bool` | selects OpenBLAS; `Try*` returns `false` on load failure |
|  [04]   | `Control.UseNativeCUDA` / `TryUseNativeCUDA`    | static `void`/`bool` | selects CUDA (no `osx-arm64`/`linux`/`win` asset admitted) |
|  [05]   | `Control.TryUseNative`                          | static `bool`   | tries the best available native; `false` if none load    |
|  [06]   | `Control.UseBestProviders`                      | static `void`   | tries MKL→CUDA→OpenBLAS→managed                           |
|  [07]   | `Control.NativeProviderPath`                    | static `string` | hint path for native binaries; setter cascades `InitializeVerify` on every provider control |
|  [08]   | `Control.UseSingleThread` / `UseMultiThreading` | static `void`   | forces serial vs parallel managed evaluation             |
|  [09]   | `Control.Describe`                              | static `string` | one-line active-provider/threading diagnostic for a receipt |
|  [10]   | `LinearAlgebraControl.Provider`                 | static prop     | gets/sets the active `ILinearAlgebraProvider` handle     |
|  [11]   | `LinearAlgebraControl.TryUse(ILinearAlgebraProvider)` | static `bool` | activates a provided handle, no-throw                    |
|  [12]   | `LinearAlgebraControl.HintPath`                 | static `string` | the LA-provider-specific native hint path                |
|  [13]   | `LinearAlgebraControl.FreeResources`            | static `void`   | releases native provider resources                       |
|  [14]   | `Control.MaxDegreeOfParallelism`                | static `int`    | the managed parallelism cap (read into the `DeterminismTag`/`SolveProvenance` receipt; pairs with `UseSingleThread`/`UseMultiThreading`) |

[ENTRYPOINT_SCOPE]: dense factorization
- rail: numeric

Dense builders and tile methods keep exact overload shape outside the table; `Solve` admits both matrix and vector right-hand sides through `ISolver<T>`.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]       | [CAPABILITY]                       |
| :-----: | :---------------------------------------------- | :----------------- | :--------------------------------- |
|  [01]   | `Matrix<T>.Multiply`                            | matrix call        | provider-routed dense GEMM         |
|  [02]   | `Matrix<T>.LU`                                  | matrix call        | builds `LU<T>`                     |
|  [03]   | `Matrix<T>.QR`                                  | matrix call        | builds `QR<T>`                     |
|  [04]   | `Matrix<T>.Cholesky`                            | matrix call        | builds `Cholesky<T>`               |
|  [05]   | `Matrix<T>.Svd`                                 | matrix call        | builds `Svd<T>`                    |
|  [06]   | `Matrix<T>.Evd`                                 | matrix call        | builds `Evd<T>`                    |
|  [07]   | `LU<T>.Solve`                                   | factorization call | solves right-hand sides            |
|  [08]   | `Cholesky<T>.Solve`                             | factorization call | solves SPD systems                 |
|  [09]   | `QR<T>.Solve` / `Svd<T>.Solve` / `Evd<T>.Solve` | factorization call | solves through `ISolver<T>`        |
|  [10]   | `Matrix<double>.Build.DenseOfArray`             | factory call       | builds dense matrix from array     |
|  [11]   | `Matrix<double>.Build.Dense`                    | factory call       | builds dense matrix by shape/value |
|  [12]   | `Matrix<T>.SubMatrix`                           | matrix call        | extracts a tile                    |
|  [13]   | `Matrix<T>.SetSubMatrix`                        | matrix call        | writes a tile in place             |
|  [14]   | `Matrix<T>.Build` / `Vector<T>.Build`           | builder accessor   | static `MatrixBuilder<T>` / `VectorBuilder<T>` — the factory root the `DenseOf*`/`OfStorage` family hangs off (`Matrix<Complex>`/`Vector<Complex>` instantiate the same generic) |
|  [15]   | `Matrix<double>.Build.OfStorage`                | factory call       | wraps an existing `MatrixStorage<T>` (dense or `SparseCompressedRowMatrixStorage<double>`) into the concrete matrix without copy — the storage→matrix bridge (no `SparseMatrix.OfStorage` static exists) |
|  [16]   | `Matrix<double>.Build.DenseOfColumnMajor`       | factory call       | dense matrix from a column-major `IEnumerable<T>` |
|  [17]   | `Matrix<double>.Build.DenseOfColumns` / `DenseOfColumnVectors` | factory call | dense matrix from per-column sequences / `Vector<T>` columns |
|  [18]   | `Matrix<double>.Build.DenseOfDiagonalVector` / `DiagonalOfDiagonalVector` | factory call | dense (or diagonal-storage) matrix from a diagonal `Vector<T>` |
|  [19]   | `Vector<double>.Build.Dense` / `DenseOfArray`   | factory call       | dense vector by size/value/init or from an array |
|  [20]   | `Matrix<T>.KroneckerProduct`                    | matrix call        | Kronecker (tensor) product `A ⊗ B` |
|  [21]   | `Matrix<T>.Transpose`                           | matrix call        | matrix transpose (`ConjugateTranspose` is the Hermitian form) |
|  [22]   | `Matrix<T>.Add`                                 | matrix call        | matrix/scalar addition (`Add(Matrix<T>)` / `Add(T)`) |

[ENTRYPOINT_SCOPE]: sparse ingestion + solve
- rail: numeric

Math.NET sparse imports normalize to CSR; CSparse factorization consumes CSC storage from indexed entries.

| [INDEX] | [SURFACE]                                                            | [CALL_SHAPE]       | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------- | :----------------- | :------------------------------ |
|  [01]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseRowFormat`    | static factory     | direct CSR import               |
|  [02]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseColumnFormat` | static factory     | CSC import to CSR               |
|  [03]   | `SparseCompressedRowMatrixStorage<T>.OfCoordinateFormat`             | static factory     | COO import to CSR               |
|  [04]   | `SparseCompressedRowMatrixStorage<T>.OfIndexedEnumerable`            | static factory     | indexed import to CSR           |
|  [05]   | `CSparse.Storage.CompressedColumnStorage<T>.OfIndexed`               | static factory     | CSparse CSC import              |
|  [06]   | `SparseCholesky.Create`                                              | static factory     | factors a CSparse CSC matrix    |
|  [07]   | `SparseLU.Create`                                                    | static factory     | factors a CSparse CSC matrix    |
|  [08]   | `SparseQR.Create`                                                    | static factory     | factors a CSparse CSC matrix    |
|  [09]   | `ISparseFactorization<T>.Solve`                                      | factorization call | solves `Ax=b` in place          |
|  [10]   | `IIterativeSolver<T>.Solve`                                          | solver call        | iterative solve with `Iterator` |
|  [11]   | `new SparseMatrix(SparseCompressedRowMatrixStorage<double>)`         | ctor               | wraps CSR storage into the MathNet sparse matrix without copy (the direct twin of `Matrix<double>.Build.OfStorage`) |
|  [12]   | `CSparse.Ordering.AMD.Generate`                                      | static factory     | `int[] Generate<T>(CompressedColumnStorage<T>, ColumnOrdering)` / `Generate(SymbolicColumnStorage, ColumnOrdering)` — fill-reducing permutation before a direct factorization |

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

## [04]-[IMPLEMENTATION_LAW]

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
