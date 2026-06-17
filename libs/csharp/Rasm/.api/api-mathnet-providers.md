# [RASM_COMPUTE_API_MATHNET_PROVIDERS]

`MathNet.Numerics` supplies dense and sparse linear algebra, the RID-keyed
native-provider selection façade over MKL and OpenBLAS, the CSR sparse storage
surface with its CSC/COO/indexed ingestion conversions, and the matrix
factorization family; `CSparse` supplies direct sparse Cholesky, LU, and QR
factorizations beside the MathNet iterative solvers for the numeric lane.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :----------------------------- | :------------- | :----------------------------------- |
|   [1]   | `Control`                      | static façade  | selects + probes the active provider |
|   [2]   | `LinearAlgebraControl`         | static façade  | provider-level direct selection API  |
|   [3]   | `ILinearAlgebraProvider`       | provider seam  | the active provider handle           |
|   [4]   | `MklLinearAlgebraControl`      | provider type  | MKL native adapter control           |
|   [5]   | `OpenBlasLinearAlgebraControl` | provider type  | OpenBLAS native adapter control      |

[PUBLIC_TYPE_SCOPE]: dense algebra
- rail: numeric

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :------------- | :----------------------------- |
|   [1]   | `Matrix<T>`                        | dense matrix   | dense matrix value carrier     |
|   [2]   | `Vector<T>`                        | dense vector   | dense vector value carrier     |
|   [3]   | `Matrix<double>` (`Double.Matrix`) | dense matrix   | the numeric-lane dense carrier |
|   [4]   | `LU<T>`                            | factorization  | LU decomposition + solve       |
|   [5]   | `QR<T>`                            | factorization  | QR decomposition + solve       |
|   [6]   | `Cholesky<T>`                      | factorization  | Cholesky decomposition + solve |
|   [7]   | `Svd<T>`                           | factorization  | singular value decomposition   |
|   [8]   | `Evd<T>`                           | factorization  | eigenvalue decomposition       |
|   [9]   | `DenseColumnMajorMatrixStorage<T>` | dense storage  | column-major dense backing     |
|  [10]   | `DenseVectorStorage<T>`            | dense storage  | dense vector backing           |

[PUBLIC_TYPE_SCOPE]: sparse algebra
- rail: numeric

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `SparseCompressedRowMatrixStorage<T>`                        | sparse storage | CSR matrix backing (only native form) |
|   [2]   | `SparseVectorStorage<T>`                                     | sparse storage | COO-style sparse vector backing       |
|   [3]   | `CSparse.Storage.CompressedColumnStorage<T>`                 | csc storage    | CSparse CSC matrix backing            |
|   [4]   | `CSparse.Double.SparseMatrix`                                | sparse matrix  | CSparse double CSC matrix             |
|   [5]   | `CSparse.Double.Factorization.SparseCholesky`                | factorization  | direct sparse Cholesky                |
|   [6]   | `CSparse.Double.Factorization.SparseLU`                      | factorization  | direct sparse LU                      |
|   [7]   | `CSparse.Double.Factorization.SparseQR`                      | factorization  | direct sparse QR                      |
|   [8]   | `CSparse.ColumnOrdering`                                     | ordering enum  | fill-reducing ordering selector       |
|   [9]   | `MathNet.Numerics.LinearAlgebra.Solvers.IIterativeSolver<T>` | solver seam    | iterative-solve seam                  |

[PUBLIC_TYPE_SCOPE]: iterative solvers
- rail: numeric

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE] | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab`    | solver         | biconjugate gradient stabilized |
|   [2]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.GpBiCg`      | solver         | generalized product BiCG        |
|   [3]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.TFQMR`       | solver         | transpose-free QMR              |
|   [4]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.MlkBiCgStab` | solver         | multiple-Lanczos BiCGStab       |
|   [5]   | `MathNet.Numerics.LinearAlgebra.Solvers.Iterator<T>`        | control        | iteration stop criteria         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider selection
- rail: numeric

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]    | [CAPABILITY]                              |
| :-----: | :----------------------------------- | :-------------- | :---------------------------------------- |
|   [1]   | `Control.UseManaged`                 | static `void`   | selects the pure-managed provider         |
|   [2]   | `Control.UseNativeMKL`               | static `void`   | selects MKL; throws on load failure       |
|   [3]   | `Control.TryUseNativeMKL`            | static `bool`   | selects MKL; `false` on load failure      |
|   [4]   | `Control.UseNativeOpenBLAS`          | static `void`   | selects OpenBLAS; throws on load failure  |
|   [5]   | `Control.TryUseNativeOpenBLAS`       | static `bool`   | selects OpenBLAS; `false` on load failure |
|   [6]   | `Control.UseBestProviders`           | static `void`   | tries MKL→CUDA→OpenBLAS→managed           |
|   [7]   | `Control.NativeProviderPath`         | static `string` | sets native hint path on all controls     |
|   [8]   | `LinearAlgebraControl.Provider`      | static prop     | gets/sets the active provider handle      |
|   [9]   | `LinearAlgebraControl.TryUse`        | static `bool`   | activates a provided handle, no-throw     |
|  [10]   | `LinearAlgebraControl.FreeResources` | static `void`   | releases native provider resources        |

[ENTRYPOINT_SCOPE]: dense factorization
- rail: numeric

Dense builders and tile methods keep exact overload shape outside the table; `Solve` admits both matrix and vector right-hand sides through `ISolver<T>`.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]       | [CAPABILITY]                       |
| :-----: | :---------------------------------------------- | :----------------- | :--------------------------------- |
|   [1]   | `Matrix<T>.Multiply`                            | matrix call        | provider-routed dense GEMM         |
|   [2]   | `Matrix<T>.LU`                                  | matrix call        | builds `LU<T>`                     |
|   [3]   | `Matrix<T>.QR`                                  | matrix call        | builds `QR<T>`                     |
|   [4]   | `Matrix<T>.Cholesky`                            | matrix call        | builds `Cholesky<T>`               |
|   [5]   | `Matrix<T>.Svd`                                 | matrix call        | builds `Svd<T>`                    |
|   [6]   | `Matrix<T>.Evd`                                 | matrix call        | builds `Evd<T>`                    |
|   [7]   | `LU<T>.Solve`                                   | factorization call | solves right-hand sides            |
|   [8]   | `Cholesky<T>.Solve`                             | factorization call | solves SPD systems                 |
|   [9]   | `QR<T>.Solve` / `Svd<T>.Solve` / `Evd<T>.Solve` | factorization call | solves through `ISolver<T>`        |
|  [10]   | `Matrix<double>.Build.DenseOfArray`             | factory call       | builds dense matrix from array     |
|  [11]   | `Matrix<double>.Build.Dense`                    | factory call       | builds dense matrix by shape/value |
|  [12]   | `Matrix<T>.SubMatrix`                           | matrix call        | extracts a tile                    |
|  [13]   | `Matrix<T>.SetSubMatrix`                        | matrix call        | writes a tile in place             |

[ENTRYPOINT_SCOPE]: sparse ingestion + solve
- rail: numeric

Math.NET sparse imports normalize to CSR; CSparse factorization consumes CSC storage from indexed entries.

| [INDEX] | [SURFACE]                                                            | [CALL_SHAPE]       | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------- | :----------------- | :------------------------------ |
|   [1]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseRowFormat`    | static factory     | direct CSR import               |
|   [2]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseColumnFormat` | static factory     | CSC import to CSR               |
|   [3]   | `SparseCompressedRowMatrixStorage<T>.OfCoordinateFormat`             | static factory     | COO import to CSR               |
|   [4]   | `SparseCompressedRowMatrixStorage<T>.OfIndexedEnumerable`            | static factory     | indexed import to CSR           |
|   [5]   | `CSparse.Storage.CompressedColumnStorage<T>.OfIndexed`               | static factory     | CSparse CSC import              |
|   [6]   | `SparseCholesky.Create`                                              | static factory     | factors a CSparse CSC matrix    |
|   [7]   | `SparseLU.Create`                                                    | static factory     | factors a CSparse CSC matrix    |
|   [8]   | `SparseQR.Create`                                                    | static factory     | factors a CSparse CSC matrix    |
|   [9]   | `ISparseFactorization<T>.Solve`                                      | factorization call | solves `Ax=b` in place          |
|  [10]   | `IIterativeSolver<T>.Solve`                                          | solver call        | iterative solve with `Iterator` |

## [4]-[IMPLEMENTATION_LAW]

[PROVIDER_SELECTION]:
- namespace: `MathNet.Numerics`, `MathNet.Numerics.Providers.LinearAlgebra`
- façade: `Control` (top-level), `LinearAlgebraControl` (provider-level)
- selection: `UseManaged`, `TryUseNativeMKL`, `TryUseNativeOpenBLAS` — the `Try*` variants return `false` instead of throwing on missing native assets
- RID reality: MKL native assets are x64-only (`MathNet.Numerics.MKL.Win-x64` / `.Linux-x64`); no `MathNet.Numerics.MKL.OSX` or `OpenBLAS.OSX` package exists; osx-arm64 falls back to `UseManaged`
- version track: core `MathNet.Numerics 6.0.0-beta2`, `Providers.MKL 6.0.0-beta2`, `Providers.OpenBLAS 5.0.0` (stable; no beta2), native assets `MKL.Win-x64 3.0.0` / `MKL.Linux-x64 2.0.0` satisfying the provider `>= 5.0.0` floor

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

[RAIL_LAW]:
- Package: `MathNet.Numerics` (+ `.Providers.MKL`, `.Providers.OpenBLAS`), `CSparse`
- Owns: dense + sparse BLAS-class algebra, native-provider selection, matrix factorization
- Accept: `Matrix<double>`/`Vector<double>` dense work, CSR/CSC/COO/DOK sparse ingestion, direct + iterative solve
- Reject: a package-local matrix wrapper face, a second provider selector beside `LinearProvider`, per-call-site provider switches
