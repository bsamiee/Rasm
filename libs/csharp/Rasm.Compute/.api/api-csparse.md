# [RASM_COMPUTE_API_CSPARSE]

`CSparse` (CSparse.NET) is the managed port of Tim Davis' CSparse: compressed-column sparse storage, a coordinate (triplet) assembly front-end, AMD fill-reducing ordering, direct LU/LDL'/Cholesky/QR factorization over `double` and `Complex`, Dulmage-Mendelsohn graph decomposition, and Matrix Market I/O. Substrate members live in `libs/csharp/.api/api-csparse.md`; this overlay owns Compute assembly intake, factorization lifecycle, and solver-rail stacking.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-csparse.md`
- Substrate catalog owns storage/factorization/ordering types plus assembly, arithmetic, factorization, and utility call shapes; this overlay never re-states them.
- rail: numeric

## [02]-[COMPUTE_BINDINGS]

- `ColumnOrdering` law: `MinimumDegreeAtPlusA` (AMD on A+Aᵀ) is the ONLY valid mode for `SparseCholesky`/`SparseLDL`; `MinimumDegreeStS`/`MinimumDegreeAtA` serve the QR/least-squares path; `Natural` disables permutation.
- license: LGPL-2.1-only (copyleft — dynamic-linking obligation; keep CSparse a referenced assembly, never IL-merge or statically embed, and do not vendor its source into a Rasm assembly).
- `Tensor/factor` `SparseOps.ToCsc`/`SparseOps.Diagonal` are the canonical `CoordinateStorage<double>` → `CompressedColumnStorage<double>` finalization owners; every assembled operator (stiffness, mass, geometric, Laplacian) reaches a factorization through them, never through page-local `Converter` calls.

## [03]-[IMPLEMENTATION_LAW]

[MATRIX_TOPOLOGY]:
- namespaces: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`, `CSparse.Ordering`, `CSparse.IO`; `CSparse.Complex` mirrors the `double` surface for `Complex`.
- storage: CCS via `ColumnPointers` (length cols+1), `RowIndices`, and `Values` on `CompressedColumnStorage<T>`; solver columns require sorted unique rows, enforced through `Helper.SortIndices` / `ValidateStorage(strict)` after manual array surgery.
- scalar constraint: `T : struct, IEquatable<T>, IFormattable`; `struct` is load-bearing because the generic surface admits value types only.
- `CompressedColumnStorage<T>.AutoTrimStorage` controls whether arithmetic auto-trims trailing slack.

[FACTORIZATION_TOPOLOGY]:
- lifecycle: `Create` (symbolic analysis + numeric factorization) → `Solve`; `Refactorize` reuses symbolic structure only when the sparsity pattern is byte-identical, while `SparseQR` always re-derives its symbolic Householder structure.
- `SparseCholesky` requires SPD; `Update`/`Downdate` apply a rank-1 modification to an existing factor for incremental or active-set solves after one edge change.
- `SparseCholesky`/`SparseLDL` accept only `ColumnOrdering.Natural` or `MinimumDegreeAtPlusA`.
- all four factorizations accept `IProgress<double>` reporting 0.0→1.0 for long symbolic phases.
- every factorization implements `ISolver<double>`; `SparseLU`/`SparseLDL`/`SparseCholesky` also implement `ISparseFactorization<double>` exposing `NonZerosCount`, so a Compute owner holds an active factorization behind `ISolver<double>` and swaps LU↔LDL↔Cholesky by problem class.

[STACKING] — single sparse numeric rail with sibling Compute owners:
- assemble through `CoordinateStorage<T>.At`, which accumulates duplicate `(i,j)` FEM/graph-Laplacian contributions before one `SparseOps.ToCsc` finalization; direct `ColumnPointers`/`RowIndices`/`Values` adoption requires buffers already sorted and deduplicated.
- `Multiply(ReadOnlySpan<double>, Span<double>)` consumes RHS/solution vectors owned by `System.Numerics.Tensors` (`TensorPrimitives`) and `CommunityToolkit.HighPerformance` (`MemoryOwner<double>`/`Span2D`) without a boundary copy.
- pair with `MathNet.Numerics`: CSparse owns sparse LU/LDL/Cholesky/QR, while MathNet plus MKL/OpenBLAS providers owns dense BLAS/LAPACK and Krylov work.
- owned frame `ShapeFamily.Frame` / `Beam2Euler` / `Beam2Timoshenko` kernels assemble reduced global stiffness through `CoordinateStorage<double>.At` and factor it behind `Solver/contract` `SolveLane`; continuum and frame problems share one `ISparseFactorization<double>` seam.
- expose `ILinearOperator<T>` when an implicit stencil or Schur complement needs only matvec behavior.
- `.mtx` exchange through `MatrixMarketReader/Writer` owns fixtures and external solver hand-off; large reads route through `Microsoft.IO.RecyclableMemoryStream`.

[LOCAL_ADMISSION]:
- Compute owns COO assembly, factorization, and solve. `CoordinateStorage<T>` + `SparseOps.ToCsc` is canonical intake; raw `ColumnPointers`/`RowIndices`/`Values` requires pre-sorted, pre-deduplicated external buffers validated through `Helper.ValidateStorage(strict:true)`.
- permutations enter as `int[]` through `Permutation` / `AMD`; ordering never re-derives from matrix structure inline.
- `ParallelMultiply` falls back to serial for small problems or one processor; `ParallelOptions.MaxDegreeOfParallelism` caps the fan.

[RAIL_LAW]:
- Package: `CSparse`
- Owns: compressed-column + coordinate sparse storage, AMD ordering, direct LU/Cholesky-with-update-or-downdate/LDL'/QR factorization, and Dulmage-Mendelsohn graph decomposition
- Accept: square solves via LU/LDL'/Cholesky, least-squares via QR, incremental SPD via update/downdate
- Reject: iterative/Krylov solvers, dense BLAS, eigensolvers, general-purpose linear algebra, and any second frame-FE sparse-factor stack; those route to `MathNet.Numerics`, native providers, or the owned frame spine.
