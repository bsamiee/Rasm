# [MATHNET_RASM]

[IMPORTANT] Production numerics: `libs/csharp/Rasm/Vectors/Matrix.cs`, `Cloud.cs`, `Mesh.cs` (CSparse hybrid). `MathNet.Symbolics`, Optimization, Integration, and Interpolation have zero production `.cs` references.

## [1][PUBLIC_SURFACES]

| [INDEX] | [RASM_PUBLIC_API]                                                 | [BACKEND]                                                       |
| :-----: | ----------------------------------------------------------------- | --------------------------------------------------------------- |
|   [1]   | `DecomposeSvd`, `DecomposeLu`, `DecomposeQr`, `DecomposeCholesky` | MathNet factorizations                                          |
|   [2]   | `DecomposeEigenDetailed`, `SmallestEigenpairsDetailed`            | MathNet EVD / custom LOBPCG                                     |
|   [3]   | `SolveDetailed` on sparse types                                   | BiCgStab + preconditioner + stop criteria                       |
|   [4]   | `CholeskySparse.Of` / `SolveDetailed`                             | CSparse `SparseCholesky`                                        |
|   [5]   | Cloud Sinkhorn / PCA                                              | `DenseMatrixD`, symmetric EVD, `SortedArrayStatistics.Quantile` |

Internal implementation types (`MatrixKernel`, `LobpcgCore`) are not public API.

RhinoCommon owns geometry; MathNet receives explicit coordinates after validity projection.

## [2][RECEIPTS_AND_PATHS]

| [INDEX] | [TYPE]                              | [KIND]                   |
| :-----: | ----------------------------------- | ------------------------ |
|   [1]   | `SolveReceipt`                      | `readonly record struct` |
|   [2]   | `SolvePath`, `SolveStop`            | `[SmartEnum<int>]`       |
|   [3]   | `EigenSolveReceipt<TEigen,TVector>` | `readonly record struct` |
|   [4]   | `EigenSolvePath`, `EigenSolveStop`  | `[SmartEnum<int>]`       |

**SolvePath** members: `DenseLu`, `DenseQrLeastSquares`, `DenseCholesky`, `SparseBiCgStabDiagonal`, `SparseMathNetDirectFallback`, `SparseCholesky`.
**SolveStop** members: `DirectSolved`, `LeastSquaresSolved`, `ResidualConverged`, `DirectFallbackSolved`.
**EigenSolvePath** members: `DenseSymmetricEvd`, `DenseGeneralEvd`, `SparseLobpcg`, `SparseHermitianLobpcg`, `SparseGeneralizedCholeskyCongruence`.
**EigenSolveStop** members: `DirectSolved`, `ResidualConverged`, `MaxIterationsExhausted`.

MathNet solver type for sparse iterative: **`BiCgStab`** (spelling in MathNet API). Package id for sparse Cholesky co-primary: **`CSparse`** (not `CSparse.NET`).

## [3][SOLVE_FLOWS]

| [INDEX] | [FLOW]                 | [BACKEND]                                                      |
| :-----: | ---------------------- | -------------------------------------------------------------- |
|   [1]   | Dense direct           | LU / Cholesky / QR via factorization                           |
|   [2]   | Sparse iterative       | `BiCgStab` + `DiagonalPreconditioner` + iterator stop criteria |
|   [3]   | Sparse direct fallback | MathNet `A.Solve(b)` when iterative residual fails policy      |
|   [4]   | SPD sparse mesh/heat   | CSparse Cholesky via `LaplacianCache` in `Mesh.cs`             |
|   [5]   | Partial eigen          | `SmallestEigenpairsDetailed` → internal LOBPCG                 |

Document non-convergence and non-finite scalars as typed `Fin` failures at boundary — see `linear.md`.

## [4][BOUNDARY]

- Wrap native/MathNet throws in `op.Catch` or `Try.lift` at Vectors boundary.
- Project GH2/Rhino outputs from Rasm receipts — never expose MathNet matrix types on public API.

## [5][ADOPTION_CANDIDATES]

| [INDEX] | [SURFACE]                                  | [ADOPTION_STATE]                         |
| :-----: | ------------------------------------------ | ---------------------------------------- |
|   [1]   | `MathNet.Symbolics`                        | No production `.cs` references           |
|   [2]   | Optimization / Integration / Interpolation | Named in `api.md`; not consumed by libs  |
|   [3]   | Control providers                          | Named; waits for measured adoption       |

Mark future consumers explicitly when first call site lands.
