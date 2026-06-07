# [NUMERIC_ALGORITHMS]

Numeric algorithms start after external-domain validity projection. Host semantics stay with host owners; numeric code receives explicit coordinates, dimensions, tolerances, units, and policy values.

## [1][DECISION_SPINE]

| [INDEX] | [INPUT]                     | [OWNER]                        |
| :-----: | :-------------------------- | :----------------------------- |
|   [1]   | host geometry or topology   | RhinoCommon or GH2             |
|   [2]   | dense matrix/vector solve   | MathNet linear algebra         |
|   [3]   | sparse iterative solve      | MathNet Krylov stack           |
|   [4]   | sparse direct solve         | CSparse factorization          |
|   [5]   | partial eigenpairs          | application block iteration    |
|   [6]   | symbolic formula work       | adoption-gated Symbolics owner |
|   [7]   | convergence or failure rail | LanguageExt boundary           |

Build matrices from explicit coordinate order, dimensions, units, and tolerance policy. Public outputs carry domain receipts and diagnostics, not MathNet storage types.

## [2][DENSE_LINEAR_ALGEBRA]

Construction:
    Owner: `Matrix<T>.Build` and `Vector<T>.Build`.
    Gate: construct from admitted coordinates, dimensions, and scalar policy.

Factorization:
    Owner: `LU`, `QR`, `Cholesky`, `Svd`, and `Evd`.
    Use: reusable factorization, diagnostics, determinant, rank, residual, and eigen receipts.
    Gate: record factorization family, status, tolerance, and residual when downstream behavior depends on those facts.

Direct solve:
    Owner: direct `Solve` only when no exposed policy or diagnostic path is needed.
    Repository path: `SolvePath.DenseLu`, `SolvePath.DenseQrLeastSquares`, and `SolvePath.DenseCholesky`.

Eigen solve:
    Owner: dense `Evd` for symmetric and general dense eigen.
    Repository path: `EigenSolvePath.DenseSymmetricEvd` and `EigenSolvePath.DenseGeneralEvd`.

Scalar namespaces:
    Rule: keep `MathNet.Numerics.LinearAlgebra`, `.Complex`, `.Complex32`, and `.Single` scalar families isolated inside one transform pipeline.
    Gate: add an explicit conversion boundary before mixing scalar namespaces.

## [3][SPARSE_ITERATIVE_SOLVE]

Use sparse iterative solve when structure is uncertain, the pattern changes often, or SPD admission is not proven.

Solver:
    Owner: `BiCgStab` as the primary nonsymmetric Krylov path.
    Alternatives: `GpBiCg`, `TFQMR`, and `MlkBiCgStab` only when the algorithm owner proves the fit.
    Repository path: `SolvePath.SparseBiCgStabDiagonal`.

Preconditioner:
    Owner: diagonal, ILU0, ILUTP, and MILU0 preconditioner families.
    Gate: record whether the preconditioner changes convergence or fallback behavior.

Iterator:
    Owner: `Iterator<T>` with `FailureStopCriterion`, `DivergenceStopCriterion`, `ResidualStopCriterion`, and `IterationCountStopCriterion`.
    Gate: preserve `Iterator.Status`, stop reason, iteration count, tolerance, and residual in the receipt.

Fallback:
    Rule: use provider-dependent `Matrix<T>.Solve(b)` only after iterative residual failure is recorded and the fallback residual is validated explicitly.
    Repository path: `SolvePath.SparseMathNetDirectFallback`.

## [4][EIGEN_STATISTICS_SYMBOLICS]

Partial eigen:
    Owner: local block-iterative methods on sparse operators.
    Repository path: `EigenSolvePath.SparseLobpcg`, `EigenSolvePath.SparseHermitianLobpcg`, and `EigenSolvePath.SparseGeneralizedCholeskyCongruence`.
    Reject: claiming a MathNet LOBPCG type when local XML or source does not prove one.

Statistics:
    Owner: `MathNet.Numerics.Statistics`, `Statistics.*`, and `SortedArrayStatistics`.
    Gate: data ownership, sort policy, and empty-sample behavior must be explicit before the statistic runs.

Optimization, integration, and interpolation:
Owner: MathNet only with an accepted algorithm owner and failure rail.
    Gate: keep API catalog rows out of active guidance until source use exists.

Symbolics:
    Owner: `MathNet.Symbolics`.
    Gate: the project graph references the package, but active guidance starts only when production source owns formula parsing, transformation, evaluation, or compilation.

## [5][RECEIPTS_AND_FAILURES]

Carry solver path, stop reason, iterations, residual, tolerance, dimensions, RHS length, and non-finite status through domain receipts. Convert exceptions, non-convergence, non-finite values, unsupported result shapes, and residual failure into `Fin<T>` at the algorithm boundary.

Never expose `Matrix<T>`, `Vector<T>`, storage classes, or factorization instances as public host output identity.

Boundary rules:
- Wrap native or MathNet throws in typed result rails at the vector or algorithm boundary.
- Preserve non-convergence and non-finite scalars as typed failures.
- Use BenchmarkDotNet before replacing MathNet solvers with primitive reductions.
- Keep random or distribution flows seed-explicit for GH-visible output.
