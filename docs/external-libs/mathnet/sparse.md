# [H1][MATHNET_SPARSE]

[IMPORTANT] Pin **`MathNet.Numerics`** and co-primary **`CSparse`** (NuGet id **`CSparse`**, not `CSparse.NET`) at the versions pinned in `Directory.Packages.props`. Verify MathNet names against `MathNet.Numerics.xml`; verify CSparse against `CSparse.xml` under `lib/{tfm}/`.

## [1][SOURCE_TRUTH]

| [INDEX] | [PACKAGE]          |          [VERSION]           | [XML]                                           |
| :-----: | ------------------ | :--------------------------: | ----------------------------------------------- |
|   [1]   | `MathNet.Numerics` | `Directory.Packages.props`   | `MathNet.Numerics.xml`                          |
|   [2]   | `CSparse`          | `Directory.Packages.props`   | `CSparse.xml` — zero deps on `net10.0`/`net8.0` |

The pinned `MathNet.Numerics` XML contains **zero** CSparse references — hybrid routing is integrator-authored.

## [2][ROLE_SPLIT]

| [INDEX] | [CONCERN]                                                  | [OWNER]                                           |
| :-----: | ---------------------------------------------------------- | ------------------------------------------------- |
|   [1]   | Dense LU / QR / Cholesky / SVD / EVD                       | MathNet                                           |
|   [2]   | Sparse matrix construction, SpMV, norms, dense export      | MathNet                                           |
|   [3]   | Sparse iterative solve (BiCGSTAB and related Krylov types) | MathNet                                           |
|   [4]   | Sparse SPD direct Cholesky, many solves on same pattern    | CSparse                                           |
|   [5]   | Sparse nonsymmetric / rectangular direct                   | CSparse LU/QR                                     |
|   [6]   | Sparse direct via provider API                             | MathNet `Providers.SparseSolver` — adopt-on-proof |
|   [7]   | Partial / block eigen algorithms                           | Application layer — no LOBPCG type in MathNet XML |
|   [8]   | Solver path/stop/residual vocabulary                       | Application layer                                 |

## [3][FORMAT_STRATEGY]

| [INDEX] | [FORMAT]                       | [NATIVE]                                                  | [BEST_FOR]                                |
| :-----: | ------------------------------ | --------------------------------------------------------- | ----------------------------------------- |
|   [1]   | CSR (row ptr, col ind, values) | MathNet `SparseCompressedRowMatrixStorage`                | Row assembly, row SpMV, iterative solvers |
|   [2]   | CSC (col ptr, row ind, values) | CSparse; MathNet `SparseFromCompressedSparseColumnFormat` | Direct factorization boundary             |

**Hybrid pipelines:**

```
Triplets -> dedupe/sum -> CSR (MathNet iterative path)
Triplets -> dedupe/sum -> CSC (CSparse direct path)
```

**Symmetric Cholesky admission:**
- Normalize to upper triangle `(min(i,j), max(i,j))`.
- Reject duplicate positions that disagree beyond tolerance.
- Require square `n × n`.
- Pin or shift semidefinite operators before Cholesky.

## [4][DECISION_FLOW]

| [INDEX] | [SCENARIO]                        | [PRIMARY]                   | [SECONDARY]                       |
| :-----: | --------------------------------- | --------------------------- | --------------------------------- |
|   [1]   | SPD, fixed pattern, many RHS      | CSparse Cholesky            | —                                 |
|   [2]   | SPD, one-off or changing pattern  | MathNet BiCGSTAB            | CSparse after SPD certification   |
|   [3]   | Nonsymmetric or unknown structure | MathNet BiCGSTAB            | CSparse LU                        |
|   [4]   | Tiny system (`n` below crossover) | MathNet dense               | —                                 |
|   [5]   | Rectangular sparse least squares  | CSparse QR                  | MathNet dense QR after projection |
|   [6]   | Partial eigenpairs                | Application iterative layer | CSparse for inner SPD solves      |

**Iterative stack (MathNet):**
- Preconditioner: `DiagonalPreconditioner` first; `ILU0Preconditioner`, `ILUTPPreconditioner`, `MILU0Preconditioner` when CSR allows.
- Iterator: `Iterator<T>` with composite stop criteria.
- Status: `IIterativeSolver.Solve` writes `result`; read `Iterator.Status` or use `TrySolveIterative`.

**Direct fallback:** when iterative residual fails policy, `Matrix<T>.Solve(b)` — provider-dependent; validate residuals explicitly.

## [5][MATHNET_ITERATIVE]

| [INDEX] | [SURFACE]                                                                                                 | [ROLE]                      |
| :-----: | --------------------------------------------------------------------------------------------------------- | --------------------------- |
|   [1]   | `BiCgStab`                                                                                                | Primary nonsymmetric Krylov |
|   [2]   | `GpBiCg`, `TFQMR`, `MlkBiCgStab`                                                                          | Alternative Krylov families |
|   [3]   | `CompositeSolver`                                                                                         | Solver composition          |
|   [4]   | `DiagonalPreconditioner`, `ILU0Preconditioner`, `ILUTPPreconditioner`, `MILU0Preconditioner`              | Preconditioners             |
|   [5]   | `UnitPreconditioner`, `CancellationStopCriterion`, `DelegateStopCriterion`                                | Iterator plumbing           |
|   [6]   | `FailureStopCriterion`, `DivergenceStopCriterion`, `ResidualStopCriterion`, `IterationCountStopCriterion` | Composite stopping          |
|   [7]   | `SolveIterative` / `TrySolveIterative`                                                                    | Matrix entrypoints          |

Solvers: `MathNet.Numerics.LinearAlgebra.{Double|Single|Complex|Complex32}.Solvers.*`. Shared: `MathNet.Numerics.LinearAlgebra.Solvers.*`.

## [6][HYBRID_PATTERNS]

| [INDEX] | [PATTERN]                          | [SHAPE]                                                                 |
| :-----: | ---------------------------------- | ----------------------------------------------------------------------- |
|   [1]   | CSR hub                            | MathNet assembly; CSC from symmetric upper triangle at CSparse boundary |
|   [2]   | Iterative first, direct safety net | BiCGSTAB with strict iterator -> validated direct fallback              |
|   [3]   | Factor cache                       | Cache `SparseCholesky` + metadata keyed by topology                     |
|   [4]   | Residual validation                | Post-solve residual via MathNet SpMV on CSR even when factor is CSparse |
|   [5]   | Eigen outer / solve inner          | Block iterative layer on MathNet; inner SPD shifts via CSparse Cholesky |

## [7][CSPARSE_SURFACE]

| [INDEX] | [NAMESPACE]                               | [OWNS]                                                                          |
| :-----: | ----------------------------------------- | ------------------------------------------------------------------------------- |
|   [1]   | `CSparse`                                 | `ColumnOrdering`, `Permutation`, `Converter`, `Matrix<T>`, `ILinearOperator<T>` |
|   [2]   | `CSparse.Storage`                         | `CompressedColumnStorage<T>`, `CoordinateStorage<T>`, `SymbolicColumnStorage`   |
|   [3]   | `CSparse.Ordering`                        | `AMD`, `DulmageMendelsohn`, `StronglyConnectedComponents`                       |
|   [4]   | `CSparse.{Double\|Complex}.Factorization` | `SparseCholesky`, `SparseLDL`, `SparseLU`, `SparseQR`                           |
|   [5]   | `CSparse.Double` / `CSparse.Complex`      | `SparseMatrix`, `SolverHelper`                                                  |

**Storage:** COO assembly via `CoordinateStorage.At`; CSC fields `ColumnPointers`, `RowIndices`, `Values`. Factories: `Converter`, `CompressedColumnStorage.OfIndexed`, `OfRowMajor`, `OfColumnMajor`, etc.
**Matvec:** `Multiply`, `TransposeMultiply` with optional `α, β` scaling.

## [8][CSPARSE_ORDERING]

| [INDEX] | [ORDERING]             | [SYMBOLIC_GRAPH]        | [CHOL_LDL] |       [LU_QR]       |
| :-----: | ---------------------- | ----------------------- | :--------: | :-----------------: |
|   [1]   | `Natural`              | identity                |  [o] [o]   |       [o] [o]       |
|   [2]   | `MinimumDegreeAtPlusA` | A+A′ off-diagonal       |  [o] [o]   | [o] [o] square only |
|   [3]   | `MinimumDegreeStS`     | A′A, dense rows dropped |  [x] [x]   |       [o] [o]       |
|   [4]   | `MinimumDegreeAtA`     | A′A                     |  [x] [x]   |       [o] [o]       |

Legend: `[CHOL_LDL]` — Cholesky then LDL; `[LU_QR]` — LU then QR. `[o]` supported; `[x]` rejected by enum gate.
Cholesky/LDL enum gate: `(int)order > 1` throws `ArgumentException`. `Create(A, int[] p)` bypasses the enum gate.
Rectangular QR + `MinimumDegreeAtPlusA`: AMD requires square `A` — prefer `MinimumDegreeStS` or `MinimumDegreeAtA` for `m > n`.

`Permutation`: `Apply`, `ApplyInverse`, `Create`, `Invert`, `IsValid`.

## [9][CSPARSE_FACTORIZATION]

| [INDEX] | [CLASS]          | [MATRIX]                         | [ORDERING]                            |
| :-----: | ---------------- | -------------------------------- | ------------------------------------- |
|   [1]   | `SparseCholesky` | square SPD                       | enum gate or explicit `p`             |
|   [2]   | `SparseLDL`      | square symmetric (indefinite OK) | same gate as Cholesky                 |
|   [3]   | `SparseLU`       | square                           | all four orderings                    |
|   [4]   | `SparseQR`       | rectangular                      | AMD on `A` or `A′` if underdetermined |

[EXTRAS]
- [1] `Update`/`Downdate`; `IProgress<double>?`
- [2] progress
- [3] `tol ∈ [0,1]`; `SolveTranspose`
- [4] LS if `m≥n`, min-norm if `m<n`; no `p` overload

**Factories (double; Complex mirrors):** `SparseCholesky.Create`, `SparseLDL.Create`, `SparseLU.Create`, `SparseQR.Create` — each with `(A, order[, progress])` and `(A, p[, progress])` except QR (order only).
**NonZerosCount:** Cholesky/LDL — L nnz only; LU — `L + U − n`; QR — `Q + R − m` (row count).
**Failures (exceptions, not result rails):** Cholesky non-SPD; LDL zero diagonal; LU no pivot; invalid enum order — all plain `Exception` / `ArgumentException`.
**Update/Downdate (Cholesky only):** `CompressedColumnStorage<T> w` parameter; returns `bool`.

All implement `ISparseFactorization<T>` → `ISolver<T>` with Span-based `Solve`.

## [10][CSPARSE_SOLVE]

Use `Permutation.Apply` / `ApplyInverse` and `SolverHelper` kernels — not abstract `P`/`Q` alone.

| [INDEX] | [FACTORIZATION] | [PIPELINE]                                                                               |
| :-----: | --------------- | ---------------------------------------------------------------------------------------- |
|   [1]   | Cholesky        | `ApplyInverse(pinv,b)` -> `SolveLower(L)` -> `SolveLowerTranspose(L)` -> `Apply(pinv,·)` |
|   [2]   | LDL′            | same perm; D solve is elementwise divide                                                 |
|   [3]   | LU              | `ApplyInverse(pinv,b)` -> `SolveLower(L)` -> `SolveUpper(U)` -> `ApplyInverse(q,·)`      |
|   [4]   | QR (m≥n)        | permute b -> Householder forward -> `SolveUpper(R)` -> column perm                       |

**Advanced:** `SymbolicFactorization` + split `SymbolicAnalysis`/`Factorize`; `SparseCholesky.UpDown`; Matrix Market I/O.

## [11][RULES]

- Do not convert CSR->CSC every step without caching factorization.
- Profile fill-in (factor nnz / input nnz) before choosing direct over iterative.
- Validate SPD before `SparseCholesky`; regularize rank-deficient operators explicitly.
- Cache factors by topology hash, not only dimension `n`.
- Default Cholesky ordering: `MinimumDegreeAtPlusA` unless profiling says otherwise.
- Post-solve diagnostics are caller-owned; libraries return vectors only.
- Cross-ref `linear.md` for dense factorization surfaces.
