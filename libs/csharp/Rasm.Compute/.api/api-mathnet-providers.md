# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics` supplies dense and sparse linear algebra, the RID-keyed
native-provider selection façade over MKL and OpenBLAS, the CSR sparse storage
surface with its CSC/COO/indexed ingestion conversions, the matrix
factorization family, and the in-assembly probability `Distributions` and
descriptive `Statistics` surfaces the uncertainty lane samples and reduces;
`CSparse` supplies direct sparse Cholesky, LU, and QR factorizations beside the
MathNet iterative solvers for the numeric lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics`
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `MathNet.Numerics.LinearAlgebra.Double`, `MathNet.Numerics.LinearAlgebra.Storage`, `MathNet.Numerics.LinearAlgebra.Factorization`, `MathNet.Numerics.Providers.LinearAlgebra`
- asset: runtime library (managed; native providers ride sibling asset packages)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.MKL`
- package: `MathNet.Numerics.Providers.MKL`
- assembly: `MathNet.Numerics.Providers.MKL`
- namespace: `MathNet.Numerics.Providers.MKL.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in `MathNet.Numerics.MKL.Win-x64` / `MathNet.Numerics.MKL.Linux-x64`; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS`
- assembly: `MathNet.Numerics.Providers.OpenBLAS`
- namespace: `MathNet.Numerics.Providers.OpenBLAS.LinearAlgebra`
- asset: managed provider adapter (native binaries ship in platform OpenBLAS asset packages; no osx-arm64 asset)
- rail: numeric

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse`
- assembly: `CSparse`
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

[PUBLIC_TYPE_SCOPE]: iterative solvers
- rail: numeric

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE] | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab`    | solver         | biconjugate gradient stabilized |
|  [02]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.GpBiCg`      | solver         | generalized product BiCG        |
|  [03]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.TFQMR`       | solver         | transpose-free QMR              |
|  [04]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.MlkBiCgStab` | solver         | multiple-Lanczos BiCGStab       |
|  [05]   | `MathNet.Numerics.LinearAlgebra.Solvers.Iterator<T>`        | control        | iteration stop criteria         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]    | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Control.UseManaged`                 | static `void`   | selects the pure-managed provider         |
|  [02]   | `Control.UseNativeMKL`               | static `void`   | selects MKL; throws on load failure       |
|  [03]   | `Control.TryUseNativeMKL`            | static `bool`   | selects MKL; `false` on load failure      |
|  [04]   | `Control.UseNativeOpenBLAS`          | static `void`   | selects OpenBLAS; throws on load failure  |
|  [05]   | `Control.TryUseNativeOpenBLAS`       | static `bool`   | selects OpenBLAS; `false` on load failure |
|  [06]   | `Control.UseBestProviders`           | static `void`   | tries MKL→CUDA→OpenBLAS→managed           |
|  [07]   | `Control.NativeProviderPath`         | static `string` | sets native hint path on all controls     |
|  [08]   | `LinearAlgebraControl.Provider`      | static prop     | gets/sets the active provider handle      |
|  [09]   | `LinearAlgebraControl.TryUse`        | static `bool`   | activates a provided handle, no-throw     |
|  [10]   | `LinearAlgebraControl.FreeResources` | static `void`   | releases native provider resources        |

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

[ENTRYPOINT_SCOPE]: probability distributions + descriptive statistics
- rail: numeric

The `Distributions` and `Statistics` surfaces ship inside the admitted `MathNet.Numerics` assembly (no separate package); the uncertainty lane consumes them for forward-UQ random-variable sampling, moment reduction, and quantile estimation. Each `IContinuousDistribution` carries a `RandomSource` `System.Random` for seeded draws.

| [INDEX] | [SYMBOL]                                                   | [NAMESPACE]                      | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------------- | :------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Normal(mean, stddev)`                                     | `MathNet.Numerics.Distributions` | Gaussian continuous distribution                               |
|  [02]   | `LogNormal(mu, sigma)`                                     | `MathNet.Numerics.Distributions` | log-normal continuous distribution                             |
|  [03]   | `ContinuousUniform(lower, upper)`                          | `MathNet.Numerics.Distributions` | uniform continuous distribution                                |
|  [04]   | `Weibull(shape, scale)`                                    | `MathNet.Numerics.Distributions` | Weibull reliability distribution                               |
|  [05]   | `Beta(a, b)`                                               | `MathNet.Numerics.Distributions` | Beta continuous distribution                                   |
|  [06]   | `IContinuousDistribution.Sample()`                         | `MathNet.Numerics.Distributions` | one draw; `Samples()` is the lazy `IEnumerable<double>` stream |
|  [07]   | `IContinuousDistribution.InverseCumulativeDistribution(p)` | `MathNet.Numerics.Distributions` | quantile / PPF for inverse-transform sampling                  |
|  [08]   | `IContinuousDistribution.CumulativeDistribution(x)`        | `MathNet.Numerics.Distributions` | CDF for reliability / failure-probability scoring              |
|  [09]   | `{Mean, Variance, StdDev, Median}`                         | `MathNet.Numerics.Distributions` | closed-form distribution moments                               |
|  [10]   | `Statistics.Mean` / `Variance` / `StandardDeviation`       | `MathNet.Numerics.Statistics`    | sample moments over an `IEnumerable<double>`                   |
|  [11]   | `Statistics.Quantile(data, tau)` / `Percentile`            | `MathNet.Numerics.Statistics`    | sample quantile / percentile estimate                          |
|  [12]   | `Statistics.Covariance` / `Correlation.Pearson`            | `MathNet.Numerics.Statistics`    | pairwise covariance / Pearson correlation                      |
|  [13]   | `DescriptiveStatistics(data)`                              | `MathNet.Numerics.Statistics`    | one-pass mean/variance/skewness/kurtosis carrier               |

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

[RAIL_LAW]:
- Package: `MathNet.Numerics` (+ `.Providers.MKL`, `.Providers.OpenBLAS`), `CSparse`
- Owns: dense + sparse BLAS-class algebra, native-provider selection, matrix factorization, probability distributions + descriptive statistics
- Accept: `Matrix<double>`/`Vector<double>` dense work, CSR/CSC/COO/DOK sparse ingestion, direct + iterative solve, `IContinuousDistribution` sampling + `Statistics` moment/quantile reduction
- Reject: a package-local matrix wrapper face, a second provider selector beside `LinearProvider`, per-call-site provider switches, a hand-rolled distribution sampler or moment accumulator beside the `Distributions`/`Statistics` surface
