# [H1][MATHNET_LINEAR]
>**Dictum:** *Linear algebra executes in MathNet and exits through Rasm diagnostics.*

<br>

[IMPORTANT] RhinoCommon owns spatial semantics. MathNet owns matrix/vector computation after Rasm selects coordinates, dimensions, and tolerance policy.

---
## [1][SURFACES]
>**Dictum:** *Choose the algorithm object by diagnostic need.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | Dense matrix/vector builders | Small to medium coordinate and basis problems. |
| [2] | Sparse matrix/vector builders | Large sparse systems with explicit sparsity. |
| [3] | `Cholesky`, `LU`, `QR`, `Svd`, `Evd` | Reusable factorization and diagnostic access. |
| [4] | Direct `Solve` | Simple systems where factorization policy is not exposed. |
| [5] | Iterative solvers and criteria | Large or sparse systems requiring status and iteration reporting. |

---
## [2][ITERATIVE]
>**Dictum:** *Iterative solvers return status, not just values.*

<br>

Document solver family, preconditioner, stop criteria, iteration count, residual, and final status. Verify exact solver names from pinned XML before use. Keep solver result shape Rasm-owned; never expose MathNet storage as GH2 output identity.

---
## [3][RHINO_PROJECTION]
>**Dictum:** *Coordinates are projections from native geometry, not replacement geometry.*

<br>

- Admit `Point3d`, `Vector3d`, `Plane`, `Transform`, and mesh/curve samples through Rhino validity first.
- Build MathNet matrices from explicit coordinate order and units.
- Project results back through Rhino validity and tolerance checks.
- Preserve non-convergence and non-finite scalars as typed failures.

---
## [4][PERFORMANCE]
>**Dictum:** *Hot-path claims require measurement.*

<br>

Use MathNet vector and matrix operations for algorithmic linear algebra. Use BCL spans or tensor primitives only behind `docs/system-api-map/packages.md` adoption and measured proof. Do not replace MathNet solvers with primitive reductions.
