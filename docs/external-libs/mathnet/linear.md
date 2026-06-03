# [H1][MATHNET_LINEAR]
>**Dictum:** *Linear algebra executes in MathNet; diagnostics and receipts exit at the application boundary.*

<br>

[IMPORTANT] Pin **`MathNet.Numerics`** at the version pinned in `Directory.Packages.props`. Verify solver and factorization names against local XML before documenting new call sites.

Spatial geometry semantics belong to the host (RhinoCommon, etc.) — MathNet receives explicit coordinates, dimensions, and tolerance policy after validity projection.

---
## [1][DENSE_SURFACES]
>**Dictum:** *Choose the algorithm object by diagnostic need.*

<br>

| [INDEX] | [SURFACE]                                                 | [USE]                                                    |
| :-----: | --------------------------------------------------------- | -------------------------------------------------------- |
|   [1]   | Dense matrix/vector builders                              | Small to medium coordinate and basis problems            |
|   [2]   | `Cholesky`, `LU`, `QR`, `Svd`, `Evd`                      | Reusable factorization and diagnostic access             |
|   [3]   | Direct `Solve`                                            | Simple systems where factorization policy is not exposed |
|   [4]   | User-defined factorizations (`UserCholesky`, `UserLU`, …) | Custom storage layouts                                   |

**Dense factorization families:** standard and `User*` variants for specialized storage — verify availability in pinned XML.

---
## [2][SPARSE_SURFACES]
>**Dictum:** *Sparse assembly and iteration live in MathNet; SPD direct Cholesky often delegates to CSparse.*

<br>

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

---
## [3][ITERATIVE_CATALOG]
>**Dictum:** *Iterative solvers return status, not just values.*

<br>

| [INDEX] | [TYPE]                           | [ROLE]                          |
| :-----: | -------------------------------- | ------------------------------- |
|   [1]   | `BiCgStab`                       | Primary nonsymmetric Krylov     |
|   [2]   | `GpBiCg`, `TFQMR`, `MlkBiCgStab` | Alternative Krylov families     |
|   [3]   | `FailureStopCriterion`           | Hard failure stop               |
|   [4]   | `DivergenceStopCriterion`        | Relative residual blow-up guard |
|   [5]   | `ResidualStopCriterion`          | Target residual                 |
|   [6]   | `IterationCountStopCriterion`    | Max iterations                  |

Document solver family, preconditioner, stop criteria, iteration count, residual, and final status in application receipts.

---
## [4][EIGEN_AND_OPTIMIZATION]
>**Dictum:** *Partial eigen and optimization are MathNet-owned.*

<br>

| [INDEX] | [DOMAIN]                    | [ENTRY SURFACES]                                                                              |
| :-----: | --------------------------- | --------------------------------------------------------------------------------------------- |
|   [1]   | Symmetric / general EVD     | `Evd`, dense eigen APIs                                                                       |
|   [2]   | Partial eigen               | Application-layer block iterative methods on sparse operators — no LOBPCG type in MathNet XML |
|   [3]   | Statistics                  | `Statistics.*`, `SortedArrayStatistics`                                                       |
|   [4]   | Optimization                | `FindMinimum.*`, `BfgsMinimizer`, `NelderMeadSimplex`, trust-region family — verify XML       |
|   [5]   | Integration / interpolation | `Integrate`, spline APIs — verify XML                                                         |

MathNet.Symbolics is pinned separately in `Directory.Packages.props` — see `symbolics.md`.

---
## [5][SCALAR_NAMESPACES]
>**Dictum:** *Four scalar namespaces coexist — pick one per module.*

<br>

| [INDEX] | [NAMESPACE]                                | [SCALAR]            |
| :-----: | ------------------------------------------ | ------------------- |
|   [1]   | `MathNet.Numerics.LinearAlgebra`           | `double` / `float`  |
|   [2]   | `MathNet.Numerics.LinearAlgebra.Complex`   | `Complex`           |
|   [3]   | `MathNet.Numerics.LinearAlgebra.Complex32` | `Complex32`         |
|   [4]   | `MathNet.Numerics.LinearAlgebra.Single`    | `float` specialized |

Do not mix scalar namespaces within one transform pipeline without explicit conversion.

---
## [6][BOUNDARY]
>**Dictum:** *Numeric failure is fallible admission at the host edge.*

<br>

- Wrap native/MathNet throws in typed result rails at boundary only.
- Preserve non-convergence and non-finite scalars as typed failures.
- Never expose MathNet storage types as public host output identity.
- Build matrices from explicit coordinate order and units.

---
## [7][PERFORMANCE]
>**Dictum:** *Hot-path claims require measurement.*

<br>

Use MathNet vector and matrix operations for algorithmic linear algebra. Use BCL spans or tensor primitives only behind `docs/system-api-map/packages.md` adoption and measured proof. Do not replace MathNet solvers with primitive reductions without profiling.

---
## [8][RULES]
>**Dictum:** *Algorithm choice follows structure and diagnostic need.*

<br>

- Dense direct for small `n` or when factorization reuse dominates.
- Sparse iterative when structure is uncertain or pattern changes frequently.
- CSparse Cholesky when SPD and pattern-stable — iterative and direct catalog in `sparse.md`.
