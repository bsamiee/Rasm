# [SPARSE_FACTORIZATION]

CSparse owns direct sparse factorization when matrix shape justifies it. MathNet owns sparse construction and iterative solves; CSparse owns direct sparse solves, factor reuse, fill-in policy, and ordering once the matrix is admitted.

## [1]-[ROLE_SPLIT]

| [INDEX] | [CONCERN]                     | [OWNER]              |
| :-----: | :---------------------------- | :------------------- |
|   [1]   | dense factorization           | MathNet              |
|   [2]   | sparse construction           | MathNet              |
|   [3]   | sparse iterative solve        | MathNet              |
|   [4]   | sparse SPD direct solve       | CSparse Cholesky     |
|   [5]   | sparse symmetric direct solve | CSparse LDL          |
|   [6]   | nonsymmetric direct solve     | CSparse LU           |
|   [7]   | rectangular least squares     | CSparse QR           |
|   [8]   | solver receipt vocabulary     | application receipts |

MathNet.Numerics has no CSparse API surface. Hybrid routing is integrator-authored and must carry residual validation.

## [2]-[STORAGE_ADMISSION]

[CSR]:
- Native owner: MathNet sparse compressed row storage.
- Best for: row assembly, row SpMV, and iterative solves.

[CSC]:
- Native owner: CSparse compressed column storage.
- Best for: direct factorization.

[TRIPLETS]:
- Rule: deduplicate and sum before building CSR or CSC.
- Gate: do not convert CSR to CSC every step without factorization caching.

[SYMMETRIC_CHOLESKY_ADMISSION]:
- Rule: normalize to upper triangle, reject disagreeing duplicate positions beyond tolerance, require square `n x n`, and pin or shift semidefinite operators before Cholesky.
- Repository path: `SolvePath.SparseCholesky`.

## [3]-[DIRECT_SOLVE_SELECTION]

Use direct sparse factorization only when the sparse structure, reuse pattern, and residual policy justify it.

| [INDEX] | [SCENARIO]                   | [PRIMARY]          | [SECONDARY]               |
| :-----: | :--------------------------- | :----------------- | :------------------------ |
|   [1]   | SPD, fixed pattern, many RHS | CSparse Cholesky   | none                      |
|   [2]   | SPD, changing pattern        | MathNet `BiCgStab` | CSparse after SPD proof   |
|   [3]   | nonsymmetric structure       | MathNet `BiCgStab` | CSparse LU                |
|   [4]   | tiny system below crossover  | MathNet dense      | none                      |
|   [5]   | rectangular least squares    | CSparse QR         | dense QR after projection |
|   [6]   | partial eigenpairs           | block iteration    | CSparse inner SPD solve   |

Validate direct and fallback residuals explicitly. Factorizations and solvers return vectors; callers own diagnostics.

## [4]-[CSPARSE_SURFACE]

[CORE_NAMESPACES]:
- Use: `CSparse`, `CSparse.Storage`, `CSparse.Ordering`, `CSparse.Double`, `CSparse.Complex`, and factorization namespaces.

[STORAGE]:
- Use: `CoordinateStorage<T>` for COO assembly and compressed column storage fields for CSC.
- Fields: `ColumnPointers`, `RowIndices`, and `Values`.

[FACTORIZATION]:
- Use: `SparseCholesky` for square SPD.
- Use: `SparseLDL` for square symmetric matrices.
- Use: `SparseLU` for square nonsymmetric matrices.
- Use: `SparseQR` for rectangular systems.

[ORDERING]:
- Cholesky and LDL enum gate: `Natural` and `MinimumDegreeAtPlusA`.
- LU and QR enum gate: `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, and `MinimumDegreeAtA` where matrix shape allows them.
- Rectangular QR: avoid `MinimumDegreeAtPlusA`; prefer `MinimumDegreeStS` or `MinimumDegreeAtA` for rectangular least squares.
- Explicit permutation: bypasses the enum gate when the factorization owner proves the permutation.

[SOLVE]:
- Use: `Permutation.Apply`, `ApplyInverse`, and `SolverHelper` kernels where the factorization owner needs them.
- Reject: documenting abstract `P` and `Q` alone.

## [5]-[CACHE_AND_FAILURES]

Cache factors by topology hash and factorization policy, not only dimension. Profile fill-in as factor nonzeros over input nonzeros before choosing direct over iterative.

[NONZEROSCOUNT_INTERPRETATION]:
- Cholesky and LDL: count `L`.
- LU: count `L + U - n`.
- QR: count `Q + R - m`.
- Receipt: store the family beside `FactorNonZeros` so downstream code reads the count correctly.

[FAILURE_RAILS]:
- Map: non-SPD, zero diagonal, missing pivot, invalid ordering, residual failure, and update or downdate failure into typed rails.
- Update: Cholesky update and downdate are factor-owner behavior; record whether the update returned `false`.

Keep `CSparse` as the exact package id. Do not call it `CSparse.NET`.
