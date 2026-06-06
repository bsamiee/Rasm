# [MATHNET_LINEAR]

[IMPORTANT] MathNet owns linear algebra after external-domain validity projection.

Spatial geometry semantics belong to the owning host SDK; MathNet receives explicit coordinates, dimensions, and tolerance policy after validity projection.

## [1][DENSE_SURFACES]

| [INDEX] | [SURFACE]                                                 | [USE]                                                    |
| :-----: | --------------------------------------------------------- | -------------------------------------------------------- |
|   [1]   | Dense matrix/vector builders                              | Small to medium coordinate and basis problems            |
|   [2]   | `Cholesky`, `LU`, `QR`, `Svd`, `Evd`                      | Reusable factorization and diagnostic access             |
|   [3]   | Direct `Solve`                                            | Simple systems where factorization policy is not exposed |
|   [4]   | User-defined factorizations (`UserCholesky`, `UserLU`, …) | Custom storage layouts                                   |

**Dense factorization families:** standard and `User*` variants for specialized storage.

## [2][SPARSE_SURFACES]

| [INDEX] | [SURFACE]                                                | [USE]                                   |
| :-----: | -------------------------------------------------------- | --------------------------------------- |
|   [1]   | `SparseCompressedRowMatrixStorage` / sparse matrix types | CSR assembly, SpMV                      |
|   [2]   | `OfIndexed`, builders, compress                          | Triplet → CSR normalization             |
|   [3]   | `BiCgStab` and Krylov family                             | Iterative nonsymmetric / general sparse |
|   [4]   | Preconditioners (Diagonal, ILU0, ILUTP, MILU0)           | Iterative acceleration                  |
|   [5]   | `Iterator<T>` + stop criteria                            | Iteration control and status            |
|   [6]   | `CompositeSolver`                                        | Solver composition                      |
|   [7]   | Direct sparse `Solve` / sparse LU bridge                 | Fallback when iterative fails           |

Full sparse strategy and CSparse boundary: **`sparse.md`**.

## [3][ITERATIVE_CATALOG]

| [INDEX] | [TYPE]                           | [ROLE]                          |
| :-----: | -------------------------------- | ------------------------------- |
|   [1]   | `BiCgStab`                       | Primary nonsymmetric Krylov     |
|   [2]   | `GpBiCg`, `TFQMR`, `MlkBiCgStab` | Alternative Krylov families     |
|   [3]   | `FailureStopCriterion`           | Hard failure stop               |
|   [4]   | `DivergenceStopCriterion`        | Relative residual blow-up guard |
|   [5]   | `ResidualStopCriterion`          | Target residual                 |
|   [6]   | `IterationCountStopCriterion`    | Max iterations                  |

Document solver family, preconditioner, stop criteria, iteration count, residual, and final status in application receipts.

## [4][EIGEN_AND_OPTIMIZATION]

| [INDEX] | [DOMAIN]                    | [ENTRY SURFACES]                                                                              |
| :-----: | --------------------------- | --------------------------------------------------------------------------------------------- |
|   [1]   | Symmetric / general EVD     | `Evd`, dense eigen APIs                                                                       |
|   [2]   | Partial eigen               | Application-layer block iterative methods on sparse operators — no LOBPCG type in MathNet XML |
|   [3]   | Statistics                  | `Statistics.*`, `SortedArrayStatistics`                                                       |
|   [4]   | Optimization                | `FindMinimum.*`, `BfgsMinimizer`, `NelderMeadSimplex`, trust-region family                    |
|   [5]   | Integration / interpolation | `Integrate`, spline APIs                                                                      |

MathNet.Symbolics is a separate package graph entry; see `symbolics.md`.

## [5][SCALAR_NAMESPACES]

| [INDEX] | [NAMESPACE]                                | [SCALAR]            |
| :-----: | ------------------------------------------ | ------------------- |
|   [1]   | `MathNet.Numerics.LinearAlgebra`           | `double` / `float`  |
|   [2]   | `MathNet.Numerics.LinearAlgebra.Complex`   | `Complex`           |
|   [3]   | `MathNet.Numerics.LinearAlgebra.Complex32` | `Complex32`         |
|   [4]   | `MathNet.Numerics.LinearAlgebra.Single`    | `float` specialized |

Do not mix scalar namespaces within one transform pipeline without explicit conversion.

## [6][BOUNDARY]

- Wrap native/MathNet throws in typed result rails at boundary only.
- Preserve non-convergence and non-finite scalars as typed failures.
- Never expose MathNet storage types as public host output identity.
- Build matrices from explicit coordinate order and units.

## [7][PERFORMANCE]

Use MathNet vector and matrix operations for algorithmic linear algebra. Use standard-library span or tensor primitives only behind project package adoption and measured proof. Do not replace MathNet solvers with primitive reductions without profiling.

## [8][RULES]

- Dense direct for small `n` or when factorization reuse dominates.
- Sparse iterative when structure is uncertain or pattern changes frequently.
- CSparse Cholesky when SPD and pattern-stable — iterative and direct catalog in `sparse.md`.
