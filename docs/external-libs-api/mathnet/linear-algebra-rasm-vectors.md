# [H1][MATHNET_LINEAR_ALGEBRA_RASM_VECTORS]
>**Dictum:** *Rasm owns domain shape; MathNet owns managed linear algebra execution.*

<br>

[IMPORTANT] `Rasm.Vectors.Matrix` is the active bridge. Keep MathNet types internal unless a public Rasm type deliberately exposes that external surface.

---
## [1][OWNERSHIP]
>**Dictum:** *Public matrix semantics stay in Rasm value shapes.*

<br>

| [INDEX] | [RASM_SURFACE] | [MATHNET_SURFACE] | [ROLE] |
| :-----: | -------------- | ----------------- | ------ |
| **[1]** | `Matrix` | `Matrix<double>` / `DenseMatrix` | Dense real matrix execution. |
| **[2]** | `SymmetricMatrix` | `Matrix<double>` plus `Symmetricity.Symmetric` | Symmetric eigen and Cholesky paths. |
| **[3]** | `SparseMatrix` | `MathNet.Numerics.LinearAlgebra.Double.SparseMatrix` | Internal sparse assembly and matvec bridge. |
| **[4]** | `SparseHermitian` | `MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix` / `Matrix<Complex>` | Hermitian complex operator bridge. |
| **[5]** | Public factorization result records | MathNet factorization objects | Use only when the result has Rasm semantics such as tolerance, ordering, diagnostics, or GH2 output shape. |

[CRITICAL] Keep MathNet factorization objects direct inside owning operations. Convert to Rasm-owned records only at public boundaries or when domain semantics are added.

---
## [2][FAILURE_RAILS]
>**Dictum:** *Numerical failure must enter `Fin<T>` at the call site.*

<br>

| [INDEX] | [OPERATION] | [MATHNET_RISK] | [RASM_RULE] |
| :-----: | ----------- | -------------- | --------- |
| **[1]** | `Cholesky` | Non-SPD input can throw or fail decomposition. | Required policy: catch at boundary and return `key.InvalidInput()`. |
| **[2]** | `LU` / `Solve` | Singular or non-square matrices can fail. | Required policy: validate shape and pivot quality before returning results. |
| **[3]** | `Inverse` | Singular matrices produce invalid numerical results. | Required policy: prefer solve or pseudo-inverse; validate finite output. |
| **[4]** | `EVD` / `SVD` | Ill-conditioned inputs change rank and ordering semantics. | Document tolerance and sort output deterministically. |
| **[5]** | Iterative solvers | Non-convergence and stop criteria are algorithmic outputs. | Model convergence status before exposing solver results. |

[CRITICAL] For greenfield `net10.0`, prefer direct MathNet factorization and solver objects inside the owning Rasm operation. Collapse local numeric ceremony unless it carries Rasm-specific tolerance, ownership, or GH2 output semantics.

| [INDEX] | [KERNEL_INVARIANT] | [RULE] |
| :-----: | ------------------ | ------ |
| **[1]** | Rank threshold | Use `max(sigma) * RhinoMath.SqrtEpsilon * max(rows, cols)` unless operation policy overrides it. |
| **[2]** | Pivot quality | LU solve must reject tiny diagonal pivots before division. |
| **[3]** | Eigen ordering | Symmetric eigen output sorts by declared operation semantics: magnitude, value, or residual. |
| **[4]** | Iterative result | Non-convergence returns a typed result or `InvalidResult`; never expose partial vectors as success. |
| **[5]** | Sparse direct solve | Densification is allowed only as a named policy; large sparse paths use iterative operators. |

---
## [3][SPARSE_POLICY]
>**Dictum:** *Sparse operators are algorithm substrates, not public storage knobs.*

<br>

| [INDEX] | [CONCERN] | [POLICY] |
| :-----: | --------- | -------- |
| **[1]** | Assembly | Accept triplets, normalize duplicates, store canonical row order. |
| **[2]** | Hermitian storage | Store upper triangle and reconstruct lower triangle by conjugate transpose. |
| **[3]** | Sparse factorization | Sparse matvec and eigen bridges are sparse; current sparse direct Cholesky densifies. |
| **[4]** | Native providers | Keep managed provider until RhinoWIP macOS loading and benchmarks prove value. |
| **[5]** | LOBPCG | Rasm owns sparse-eigen algorithm policy where MathNet lacks the operation; MathNet iterative solvers target linear systems `Ax = b`. |
| **[6]** | Preconditioners | Use typed solver namespaces only for future linear solve paths. |

---
## [4][VECTOR_ROADMAP]
>**Dictum:** *MathNet should remove numerical ceremony from vector work.*

<br>

| [INDEX] | [RASM_AREA] | [MATHNET_CAPABILITY] | [NEXT_USE] |
| :-----: | ----------- | -------------------- | ---------- |
| **[1]** | Covariance and principal axes | `Statistics`, `Matrix<double>.Evd`, `Svd<T>` | Compare estimator semantics before replacing current covariance code. |
| **[2]** | Mesh Laplacian spectra | Sparse matrix, Hermitian matrix, EVD, iterative solvers | Keep Rasm sparse shape; use MathNet execution internally. |
| **[3]** | Registration | SVD and least-squares | Return Rasm transforms or future pose atoms, not MathNet matrices. |
| **[4]** | Field solvers | Linear solves and interpolation | Bind convergence into `Fin<T>` and `Op` diagnostics. |
| **[5]** | Descriptors | Eigenpairs, FFT, histograms | Define stable descriptor ordering and scaling first. |
