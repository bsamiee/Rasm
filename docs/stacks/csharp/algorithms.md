# [ALGORITHMS]

Every numeric concern reduces to an admitted problem shape, and the shape — not the call site — selects the owning algorithm. Numeric algorithms start after external-domain validity projection: host semantics stay with host owners, and numeric code receives explicit coordinates, dimensions, tolerances, units, and policy values. MathNet owns dense algebra, sparse construction, and iterative solves; CSparse owns direct sparse factorization, factor reuse, fill-in policy, and ordering once the matrix is admitted; a hand-rolled kernel is admitted only after a benchmark defeats both.

## [1]-[DECISION_SPINE]

| [INDEX] | [PROBLEM]                                  | [OWNER]                        | [FALLBACK]                             |
| :-----: | :----------------------------------------- | :----------------------------- | :------------------------------------- |
|   [1]   | host geometry or topology                  | RhinoCommon or GH2             | —                                      |
|   [2]   | dense matrix or vector solve               | MathNet linear algebra         | —                                      |
|   [3]   | tiny system below sparse crossover         | MathNet dense                  | —                                      |
|   [4]   | sparse construction                        | MathNet storage                | —                                      |
|   [5]   | sparse SPD, fixed pattern, many RHS        | CSparse Cholesky               | `BiCgStab` until SPD proof             |
|   [6]   | sparse SPD, changing pattern               | MathNet `BiCgStab`             | CSparse after SPD proof                |
|   [7]   | sparse symmetric direct solve              | CSparse LDL                    | `BiCgStab` while structure is unproven |
|   [8]   | sparse nonsymmetric or uncertain structure | MathNet `BiCgStab`             | CSparse LU on proven reuse             |
|   [9]   | rectangular least squares                  | CSparse QR                     | dense QR after projection              |
|  [10]   | partial eigenpairs                         | application block iteration    | CSparse inner SPD solve                |
|  [11]   | symbolic formula work                      | adoption-gated Symbolics owner | —                                      |
|  [12]   | convergence or failure rail                | typed result rails             | —                                      |
|  [13]   | solver receipt vocabulary                  | application receipts           | —                                      |

A fallback is the route taken while the owner's admission proof is not yet held or after its residual fails; taking it is recorded in the receipt. Build matrices from explicit coordinate order, dimensions, units, and tolerance policy; public outputs carry domain receipts and diagnostics, never MathNet storage types. MathNet has no CSparse API surface; hybrid routing is integrator-authored and must carry residual validation.

## [2]-[DENSE_LINEAR_ALGEBRA]

[CONSTRUCTION]:
- Owner: `Matrix<T>.Build` and `Vector<T>.Build`.
- Gate: construct from admitted coordinates, dimensions, and scalar policy.

[FACTORIZATION]:
- Owner: `LU`, `QR`, `Cholesky`, `Svd`, and `Evd`.
- Use: reusable factorization, diagnostics, determinant, rank, residual, and eigen receipts.
- Gate: record factorization family, status, tolerance, and residual when downstream behavior depends on those facts.

[DIRECT_SOLVE]:
- Owner: direct `Solve` only when no exposed policy or diagnostic path is needed.
- Repository path: `SolvePath.DenseLu`, `SolvePath.DenseQrLeastSquares`, and `SolvePath.DenseCholesky`.

[EIGEN_SOLVE]:
- Owner: dense `Evd` for symmetric and general dense eigen.
- Repository path: `EigenSolvePath.DenseSymmetricEvd` and `EigenSolvePath.DenseGeneralEvd`.

[SCALAR_NAMESPACES]:
- Rule: keep `MathNet.Numerics.LinearAlgebra`, `.Complex`, `.Complex32`, and `.Single` scalar families isolated inside one transform pipeline.
- Gate: add an explicit conversion boundary before mixing scalar namespaces.

## [3]-[SPARSE_ITERATIVE_SOLVE]

Use sparse iterative solve when structure is uncertain, the pattern changes often, or SPD admission is not proven.

[SOLVER]:
- Owner: `BiCgStab` as the primary nonsymmetric Krylov path.
- Alternatives: `GpBiCg`, `TFQMR`, and `MlkBiCgStab` only when the algorithm owner proves the fit.
- Repository path: `SolvePath.SparseBiCgStabDiagonal`.

[PRECONDITIONER]:
- Owner: diagonal, ILU0, ILUTP, and MILU0 preconditioner families.
- Gate: record whether the preconditioner changes convergence or fallback behavior.

[ITERATOR]:
- Owner: `Iterator<T>` with `FailureStopCriterion`, `DivergenceStopCriterion`, `ResidualStopCriterion`, and `IterationCountStopCriterion`.
- Gate: preserve `Iterator.Status`, stop reason, iteration count, tolerance, and residual in the receipt.

[FALLBACK]:
- Rule: use provider-dependent `Matrix<T>.Solve(b)` only after iterative residual failure is recorded and the fallback residual is validated explicitly.
- Repository path: `SolvePath.SparseMathNetDirectFallback`.

## [4]-[SPARSE_DIRECT_SOLVE]

Use direct sparse factorization only when the sparse structure, reuse pattern, and residual policy justify it. Validate direct and fallback residuals explicitly; factorizations and solvers return vectors, and callers own diagnostics.

[STORAGE_ADMISSION]:
- CSR: MathNet compressed row storage; best for row assembly, row SpMV, and iterative solves.
- CSC: CSparse compressed column storage; best for direct factorization.
- Triplets: deduplicate and sum before building CSR or CSC; do not convert CSR to CSC every step without factorization caching.
- Symmetric Cholesky admission: normalize to upper triangle, reject disagreeing duplicate positions beyond tolerance, require square `n x n`, and pin or shift semidefinite operators before Cholesky.
- Repository path: `SolvePath.SparseCholesky`.

[CSPARSE_SURFACE]:
- Namespaces: `CSparse`, `CSparse.Storage`, `CSparse.Ordering`, `CSparse.Double`, `CSparse.Complex`, and factorization namespaces.
- Storage: `CoordinateStorage<T>` for COO assembly; compressed column fields are `ColumnPointers`, `RowIndices`, and `Values`.
- Factorization: `SparseCholesky` for square SPD, `SparseLDL` for square symmetric, `SparseLU` for square nonsymmetric, and `SparseQR` for rectangular systems.
- Solve: `Permutation.Apply`, `ApplyInverse`, and `SolverHelper` kernels where the factorization owner needs them; never document abstract `P` and `Q` alone.
- Package identity: the exact package id is `CSparse`, never `CSparse.NET`.

[ORDERING]:
- Cholesky and LDL enum gate: `Natural` and `MinimumDegreeAtPlusA`.
- LU and QR enum gate: `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, and `MinimumDegreeAtA` where matrix shape allows them.
- Rectangular QR: avoid `MinimumDegreeAtPlusA`; prefer `MinimumDegreeStS` or `MinimumDegreeAtA` for rectangular least squares.
- Explicit permutation: bypasses the enum gate when the factorization owner proves the permutation.

[FACTOR_CACHE]:
- Rule: cache factors by topology hash and factorization policy, not only dimension.
- Gate: profile fill-in as factor nonzeros over input nonzeros before choosing direct over iterative.
- `NonZerosCount` interpretation: Cholesky and LDL count `L`; LU counts `L + U - n`; QR counts `Q + R - m`; store the family beside `FactorNonZeros` so downstream code reads the count correctly.

## [5]-[EIGEN_STATISTICS_SYMBOLICS]

[PARTIAL_EIGEN]:
- Owner: local block-iterative methods on sparse operators.
- Repository path: `EigenSolvePath.SparseLobpcg`, `EigenSolvePath.SparseHermitianLobpcg`, and `EigenSolvePath.SparseGeneralizedCholeskyCongruence`.
- Reject: claiming a MathNet LOBPCG type when local XML or source does not prove one.

[STATISTICS]:
- Owner: `MathNet.Numerics.Statistics`, `Statistics.*`, and `SortedArrayStatistics`.
- Gate: data ownership, sort policy, and empty-sample behavior must be explicit before the statistic runs.

[OPTIMIZATION_INTEGRATION_INTERPOLATION]:
- Owner: MathNet only with an accepted algorithm owner and failure rail.
- Gate: keep API catalog rows out of active guidance until source use exists.

[SYMBOLICS]:
- Owner: `MathNet.Symbolics`.
- Gate: the project graph references the package, but active guidance starts only when production source owns formula parsing, transformation, evaluation, or compilation.

## [6]-[RECEIPTS_AND_FAILURES]

Carry solver path, stop reason, iterations, residual, tolerance, dimensions, RHS length, and non-finite status through domain receipts. Convert exceptions, non-convergence, non-finite values, unsupported result shapes, and residual failure into `Fin<T>` at the algorithm boundary.

Never expose `Matrix<T>`, `Vector<T>`, storage classes, or factorization instances as public host output identity.

[FAILURE_RAILS]:
- Wrap native or MathNet throws in typed result rails at the vector or algorithm boundary.
- Preserve non-convergence and non-finite scalars as typed failures.
- Map non-SPD, zero diagonal, missing pivot, invalid ordering, residual failure, and update or downdate failure into typed rails.
- Cholesky update and downdate are factor-owner behavior; record whether the update returned `false`.
- Use BenchmarkDotNet before replacing MathNet solvers with primitive reductions.
- Keep random or distribution flows seed-explicit for GH-visible output.
