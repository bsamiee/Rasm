# [RASM_API_MATHNET_PROVIDERS]

`MathNet.Numerics` supplies dense linear algebra over `Matrix<double>`/`Vector<double>`,
the full factorization family (`LU`/`QR`/`Cholesky`/`Svd`/`Evd`/`GramSchmidt`) with the
analytical surface (`Solve`/`Inverse`/`PseudoInverse`/`Rank`/`Kernel`/`Range`/
`Determinant`/`ConditionNumber`), the RID-keyed native-provider selection façade with
OpenBLAS as the sole opt-in native accelerator and a managed-path parallelism governor,
the CSR sparse storage surface with CSC/COO/indexed ingestion, and the MathNet iterative
solvers with a composable stop-criteria `Iterator` and preconditioner seam; `CSparse`
(`api-csparse`) supplies the direct sparse factorizations beside them.
ABI: MathNet `lib/net8.0` is the highest TFM in 6.0.0-beta2 — the net10 consumer binds
net8.0; MIT. OpenBLAS native assets are x64-only (no osx-arm64); the managed provider is
the kernel's operating path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MathNet.Numerics`
- package: `MathNet.Numerics` (6.0.0-beta2; license MIT)
- assembly: `MathNet.Numerics`
- namespace: `MathNet.Numerics`, `MathNet.Numerics.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Storage`, `.LinearAlgebra.Factorization`, `.LinearAlgebra.Solvers`, `.LinearAlgebra.Double.Solvers`, `.Providers.LinearAlgebra`
- asset: runtime library (managed; native providers ride sibling asset packages)
- floor: net8.0 (no net10/net9 lib in 6.0.0-beta2; the net10 consumer binds `lib/net8.0`)
- rail: numeric

[PACKAGE_SURFACE]: `MathNet.Numerics.Providers.OpenBLAS`
- package: `MathNet.Numerics.Providers.OpenBLAS` (6.0.0-beta2)
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

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider selection
- rail: numeric

`Control` is the top-level façade (selection + parallelism governor + diagnostics); `LinearAlgebraControl` is the provider-level twin; the per-provider control types carry the tuning entrypoints. `IProviderCreator<ILinearAlgebraProvider>` is the factory seam.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Control`                      | static façade  | provider selection + managed-path parallelism + `Describe` |
|  [02]   | `LinearAlgebraControl`         | static façade  | provider-level selection; `Provider`/`TryUse`/`HintPath` |
|  [03]   | `ILinearAlgebraProvider`       | provider seam  | the active provider handle                             |
|  [04]   | `OpenBlasLinearAlgebraControl` | provider type  | OpenBLAS adapter; `CreateNativeOpenBLAS`/`UseNativeOpenBLAS` |

[PUBLIC_TYPE_SCOPE]: dense algebra
- rail: numeric

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]                            |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `Matrix<T>`                        | dense matrix   | dense carrier; algebra, factorization, analysis |
|  [02]   | `Vector<T>`                        | dense vector   | dense vector value carrier              |
|  [03]   | `Matrix<double>` (`Double.Matrix`) | dense matrix   | the numeric-lane dense carrier          |
|  [3a]   | `Double.DenseMatrix`               | dense matrix   | concrete dense `Matrix<double>`; owns the static `OfArray`/`Create`/`CreateDiagonal` factory family + value operators |
|  [3b]   | `Double.DenseVector`               | dense vector   | concrete dense `Vector<double>`; owns the static `OfArray`/`Create` factory family |
|  [04]   | `MatrixBuilder<T>` (`Matrix<T>.Build`) | factory     | dense + sparse matrix factories         |
|  [05]   | `LU<T>` / `QR<T>` / `Cholesky<T>`  | factorization  | LU / QR / Cholesky decomposition + solve|
|  [06]   | `Svd<T>` / `Evd<T>` / `GramSchmidt<T>` | factorization | SVD / eigen / Gram-Schmidt decomposition |
|  [07]   | `DenseColumnMajorMatrixStorage<T>` | dense storage  | column-major dense backing              |
|  [08]   | `DenseVectorStorage<T>`            | dense storage  | dense vector backing                    |

[PUBLIC_TYPE_SCOPE]: sparse algebra
- rail: numeric

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `SparseCompressedRowMatrixStorage<T>`                        | sparse storage | CSR matrix backing (only native MathNet sparse form) |
|  [02]   | `SparseVectorStorage<T>`                                     | sparse storage | COO-style sparse vector backing       |
|  [03]   | `CSparse.Storage.CompressedColumnStorage<T>`                 | csc storage    | CSparse CSC backing (`api-csparse`)   |
|  [04]   | `CSparse.Double.SparseMatrix`                                | sparse matrix  | CSparse double CSC matrix             |
|  [05]   | `CSparse.Double.Factorization.SparseCholesky/SparseLU/SparseQR` | factorization | direct sparse Cholesky/LU/QR (`api-csparse`) |
|  [06]   | `CSparse.ColumnOrdering`                                     | ordering enum  | fill-reducing ordering selector       |

[PUBLIC_TYPE_SCOPE]: iterative solvers
- rail: numeric

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab`    | solver         | biconjugate gradient stabilized       |
|  [02]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.GpBiCg`      | solver         | generalized product BiCG              |
|  [03]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.TFQMR`       | solver         | transpose-free QMR                    |
|  [04]   | `MathNet.Numerics.LinearAlgebra.Double.Solvers.MlkBiCgStab` | solver         | multiple-Lanczos BiCGStab             |
|  [05]   | `MathNet.Numerics.LinearAlgebra.Solvers.IIterativeSolver<T>`| solver seam    | `Solve(matrix, input, result, Iterator, IPreconditioner)` |
|  [06]   | `Iterator<T>` / `IIterationStopCriterion<T>`                | control        | composable stop-criteria + cancellation |
|  [07]   | `IPreconditioner<T>`                                        | precondition   | left preconditioner seam              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider selection + governor
- rail: numeric

`Try*` variants return `false` instead of throwing on a missing native asset; `Use*` throw. `MaxDegreeOfParallelism`/`UseSingleThread`/`UseMultiThreading` govern the managed BLAS path (the only lever on osx-arm64 where native is unavailable). `Describe()` is the provider diagnostic string for the receipt.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]    | [CAPABILITY]                              |
| :-----: | :------------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Control.UseManaged()`                             | static `void`   | selects the pure-managed provider         |
|  [02]   | `Control.TryUseNativeOpenBLAS()`                   | static `bool`   | selects OpenBLAS; `false` on load failure |
|  [03]   | `Control.TryUseNative()`                           | static `bool`   | selects the best-available native provider |
|  [04]   | `Control.UseBestProviders()` / `ConfigureAuto()`   | static `void`   | native-then-managed probe ladder           |
|  [05]   | `Control.MaxDegreeOfParallelism`                   | static `int`    | managed-path parallel degree (get/set)     |
|  [06]   | `Control.UseSingleThread()` / `UseMultiThreading()`| static `void`   | force serial / parallel managed BLAS       |
|  [07]   | `Control.NativeProviderPath` / `LinearAlgebraControl.HintPath` | static `string` | native binary search hint            |
|  [08]   | `Control.Describe()`                               | static `string` | active-provider diagnostic for the receipt |
|  [09]   | `Control.FreeResources()` / `LinearAlgebraControl.FreeResources()` | static `void` | release native provider resources    |
|  [10]   | `LinearAlgebraControl.Provider`                    | static prop     | gets/sets the active provider handle       |
|  [11]   | `LinearAlgebraControl.TryUse(ILinearAlgebraProvider)` | static `bool`| activates a provided handle, no-throw      |
|  [12]   | `OpenBlasLinearAlgebraControl.CreateNativeOpenBLAS()` | static handle | mint a provider handle for `TryUse`/`Provider` |

[ENTRYPOINT_SCOPE]: dense factorization + analysis
- rail: numeric

Factorization builders carry parameters (`QR(QRMethod = Thin)`, `Svd(bool computeVectors = true)`, `Evd(Symmetricity)`); `Matrix<T>.Solve` auto-selects a factorization. The analytical surface (`Rank`/`Kernel`/`Range`/`Inverse`/`PseudoInverse`/`Determinant`/`ConditionNumber`) composes the same factorizations. `Solve` admits both matrix and vector right-hand sides.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]       | [CAPABILITY]                          |
| :-----: | :---------------------------------------------- | :----------------- | :------------------------------------ |
|  [01]   | `Matrix<T>.Multiply(Matrix<T>)` / `Multiply(Vector<T>)` | matrix call  | provider-routed dense GEMM / GEMV     |
|  [02]   | `Matrix<T>.TransposeThisAndMultiply(...)` / `TransposeAndMultiply(...)` | matrix call | fused `AᵀB` / `ABᵀ`, no transpose materialized |
|  [03]   | `Matrix<T>.LU()` / `QR(QRMethod = Thin)` / `Cholesky()` | matrix call  | builds `LU<T>` / `QR<T>` / `Cholesky<T>` |
|  [04]   | `Matrix<T>.Svd(bool computeVectors = true)` / `Evd(Symmetricity)` / `GramSchmidt()` | matrix call | builds `Svd<T>` / `Evd<T>` / `GramSchmidt<T>` |
|  [05]   | `LU<T>.Solve` / `Cholesky<T>.Solve` / `QR<T>.Solve` / `Svd<T>.Solve` | factorization call | solves right-hand sides through `ISolver<T>` |
|  [06]   | `Matrix<T>.Solve(Vector<T>)` / `Solve(Matrix<T>)` | matrix call      | auto-selected factorization solve     |
|  [07]   | `Matrix<T>.Inverse()` / `PseudoInverse()`        | matrix call       | inverse / Moore-Penrose pseudoinverse |
|  [08]   | `Matrix<T>.Rank()` / `Kernel()` / `Range()`      | matrix call       | numeric rank / null-space / column-space basis |
|  [09]   | `Matrix<T>.Determinant()` / `ConditionNumber()`  | matrix call       | determinant / 2-norm condition number |
|  [10]   | `Matrix<double>.Build.DenseOfArray(...)` / `Dense(rows, cols, value)` | factory call | dense matrix from array / by shape  |
|  [11]   | `Matrix<double>.Build.SparseOfIndexed(...)` / `Sparse(...)` | factory call | sparse (CSR) matrix from indexed data |
|  [12]   | `Matrix<T>.SubMatrix(...)` / `SetSubMatrix(...)` | matrix call        | extracts / writes a tile in place     |
|  [13]   | `DenseMatrix.OfArray(double[,])` / `Create(rows, cols, value)` / `Create(rows, cols, Func<int,int,double>)` / `CreateDiagonal(rows, cols, Func<int,double>)` | static factory | concrete dense-matrix construction (type-static twin of `Build.Dense*`) |
|  [14]   | `DenseVector.OfArray(double[])` / `Create(length, value)` / `Create(length, Func<int,double>)` | static factory | concrete dense-vector construction |
|  [15]   | `Matrix<T>.Transpose()`                          | matrix call        | materialized transpose (distinct from the fused `Transpose*Multiply`) |
|  [16]   | `Matrix<T>.RowCount` / `ColumnCount` / `[i, j]`; `Vector<T>[i]` | property / indexer | dimensions + element get/set          |
|  [17]   | `Svd<T>.U` / `VT` / `S` / `W`                    | factorization prop | left/right singular vectors, singular-value vector `S`, diagonal-matrix `W` |
|  [18]   | `Vector<T>.L2Norm()` / `L1Norm()` / `InfinityNorm()` | vector call    | vector norms (residual / step magnitude) |
|  [19]   | `Matrix<T>`/`Vector<T>` `+` / `-` (binary + unary) / scalar `*` / `Matrix·Matrix` / `Matrix·Vector` | operator | value algebra: add / subtract / negate / scalar-scale; `·` products alias `Multiply` |

[ENTRYPOINT_SCOPE]: sparse ingestion + iterative solve
- rail: numeric

MathNet sparse imports normalize to CSR via the `Of*` family; the direct sparse path consumes CSparse CSC (`api-csparse`). The iterative `Solve` seam carries an `Iterator<T>` (composed `IIterationStopCriterion<T>`) and an `IPreconditioner<T>`.

| [INDEX] | [SURFACE]                                                            | [CALL_SHAPE]   | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseRowFormat(...)` | static factory | direct CSR import               |
|  [02]   | `SparseCompressedRowMatrixStorage<T>.OfCompressedSparseColumnFormat(...)` | static factory | CSC import to CSR               |
|  [03]   | `SparseCompressedRowMatrixStorage<T>.OfCoordinateFormat(...)`       | static factory | COO import to CSR               |
|  [04]   | `SparseCompressedRowMatrixStorage<T>.OfIndexedEnumerable(...)`      | static factory | indexed enumerable import to CSR|
|  [05]   | `IIterativeSolver<double>.Solve(matrix, input, result, Iterator, IPreconditioner)` | solver call | iterative solve with stop criteria + preconditioner |
|  [06]   | `new Iterator<double>(params IIterationStopCriterion<double>[])`    | constructor    | compose residual/iteration/divergence stop criteria |
|  [07]   | `Iterator<double>.DetermineStatus(...)` / `Cancel()` / `Reset()`    | instance       | drive / cancel the iteration; `IterationStatus` |

## [04]-[IMPLEMENTATION_LAW]

[PROVIDER_SELECTION]:
- namespace: `MathNet.Numerics`, `MathNet.Numerics.Providers.LinearAlgebra`
- façade: `Control` (selection + parallelism + diagnostics), `LinearAlgebraControl` (provider-level), `OpenBlasLinearAlgebraControl` (provider tuning)
- selection: `UseManaged`, `TryUseNativeOpenBLAS`/`TryUseNative` (`false` instead of throwing on a missing native asset), `UseBestProviders`/`ConfigureAuto` (native-then-managed probe); the canonical pattern is `TryUse*` so a missing asset degrades, never throws
- managed governor: on osx-arm64 (no native asset) `Control.MaxDegreeOfParallelism`/`UseMultiThreading`/`UseSingleThread` are the only performance levers — managed multithreading governs every dense product when no native provider loads
- RID reality: OpenBLAS native assets are x64-only; no `.OSX` asset exists; osx-arm64 falls back to `UseManaged` — OpenBLAS is the sole opt-in native accelerator the kernel documents (`Numerics/matrix`)

[DENSE_ALGEBRA]:
- namespace: `MathNet.Numerics.LinearAlgebra`, `.LinearAlgebra.Double`, `.LinearAlgebra.Factorization`
- carrier: `Matrix<double>`/`Vector<double>` — the numeric lane composes these directly, never a package-local matrix wrapper; `Matrix<double>.Build` (`MatrixBuilder<T>`) is the dense+sparse factory owner
- construction: the concrete `DenseMatrix`/`DenseVector` (`.LinearAlgebra.Double`) static `OfArray`/`Create`/`CreateDiagonal` factories are the type-static twins of `Matrix<double>.Build.Dense*` — same dense result, the numeric lane picks whichever reads cleaner; `Transpose()` materializes the transpose (distinct from the fused `Transpose*Multiply`), and `RowCount`/`ColumnCount`/`[i, j]` are the dimension + element accessors
- decomposition outputs: `Svd<T>` exposes `U` (left singular vectors), `VT` (transposed right singular vectors), `S` (singular-value vector), and `W` (singular values on a diagonal matrix) — the witness DOF projection reads the `U`-tail directly; `Vector<T>.L2Norm()`/`L1Norm()`/`InfinityNorm()` are the residual/step-norm reads the LM loop folds
- factorizations: `LU`/`QR`/`Cholesky`/`Svd`/`Evd`/`GramSchmidt` build from `Matrix<T>` instance methods and solve through their `Solve` members; `Matrix<T>.Solve` auto-selects a factorization for a one-shot solve
- analysis: `Rank`/`Kernel`/`Range`/`Inverse`/`PseudoInverse`/`Determinant`/`ConditionNumber` are first-class — a witness DOF analysis reads `Matrix<double>.Rank()` and the `Svd` `U`-tail rather than re-deriving rank
- GEMM: `Matrix<T>.Multiply` and the fused `TransposeThisAndMultiply`/`TransposeAndMultiply` route through the active `ILinearAlgebraProvider`, so provider selection governs every dense product

[SPARSE_SOLVE]:
- namespace: `MathNet.Numerics.LinearAlgebra.Storage`, `MathNet.Numerics.LinearAlgebra.Double.Solvers`, `CSparse.Double.Factorization`
- storage: `SparseCompressedRowMatrixStorage<T>` is the only native MathNet sparse matrix form (CSR); CSC/COO/indexed are ingestion conversions via the `Of*` factories, never separate storage types
- direct solvers: the direct sparse path is CSparse — `SparseCholesky`/`SparseLU`/`SparseQR` over a CSC `CompressedColumnStorage<double>` with `Refactorize` amortization (`api-csparse`); MathNet owns the iterative path
- iterative solvers: `BiCgStab`/`GpBiCg`/`TFQMR`/`MlkBiCgStab` over MathNet sparse matrices; the `Solve(matrix, input, result, Iterator, IPreconditioner)` seam composes stop criteria via `IIterationStopCriterion<T>` and an `IPreconditioner<T>` — the iterator carries cancellation and `IterationStatus`

[STACK]:
- The dense solve stacks under a LanguageExt `Fin` rail emitting a typed receipt (`Geometry/Processing/solver`): `Try.lift(() => spd.Cholesky().Solve(rhs)).Run()` maps a singular/indefinite factorization to a typed `GeometryFault.SingularSystem` over `Fin<Vector<double>>`, never a thrown exception — the factorization is the BLAS-class numeric, the `Try.lift`/`Fin` is the rail, the `SolveReceipt` (residual norm, iteration count, terminal damping, `DofAnalysis`) is the typed evidence
- The witness DOF refinement stacks the analytical surface: `Matrix<double>.Rank()` plus `Svd` `U`-tail projection distinguishes redundant-consistent from over-constrained — the analysis members are composed, never a hand-rolled rank reduction
- `Control` selection happens ONCE at composition (the numeric lane's provider selector), then every `Matrix<double>.Multiply`/factorization in the process routes the chosen provider; `Control.Describe()` flows into the benchmark/solve receipt as the provider-rank fact, so a run records which BLAS it executed on
- The direct (`CSparse`) and iterative (`MathNet`) sparse strategies and the dense (`MathNet`) lane meet at the `SolveReceipt` boundary — density and reusability are the discriminants (dense small SPD, CSC direct with `Refactorize` for repeated solves, iterative for systems too large to factor), all folding the same typed receipt under the same `Fin` rail

[LOCAL_ADMISSION]:
- The numeric lane selects the provider once at composition through `LinearProvider.Select()`; a per-call-site provider switch is the named defect
- Dense and sparse solves emit the `Factorization` `ComputeReceipt` case; provider rank is claim-gated through `BenchmarkRow.Claim`, never a static default — `Control.Describe()` is the receipt's provider fact
- The MathNet sparse format axis is an ingestion discriminant over CSR-backed storage, not four storage types; the CSC direct path is CSparse (`api-csparse`)

[RAIL_LAW]:
- Package: `MathNet.Numerics` (+ `.Providers.OpenBLAS`), `CSparse`
- Owns: dense BLAS-class algebra + analysis, native-provider selection + managed parallelism governor, the full factorization family, CSR ingestion, MathNet iterative solvers
- Accept: `Matrix<double>`/`Vector<double>` dense work, CSR/CSC/COO/indexed sparse ingestion, direct (CSparse) + iterative (MathNet) solve
- Reject: a package-local matrix wrapper face, a second provider selector beside `LinearProvider`, per-call-site provider switches, a hand-rolled rank/inverse/pseudoinverse beside the `Matrix<T>` analytical surface
