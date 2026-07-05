# [RASM_COMPUTE_API_CSPARSE]

`CSparse` (CSparse.NET) is the managed port of Tim Davis' CSparse: compressed-column sparse storage,
a coordinate (triplet) assembly front-end, AMD fill-reducing ordering, and direct factorization
solvers — LU with partial pivoting, LDL' and Cholesky for symmetric matrices (Cholesky with
rank-1 update/downdate), and QR for least-squares — over `double` and `Complex` scalars, plus a
Dulmage-Mendelsohn / strongly-connected-component graph layer and Matrix Market I/O. The substrate
canonical member catalog is `libs/csharp/.api/api-csparse.md`; this overlay carries only the
Compute delta — the assembly intake, factorization lifecycle, and solver-rail stacking the
`Solver`/`Tensor` pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-csparse.md`
- the storage/factorization/ordering type roster and the assembly, arithmetic, factorization, and utility call-shape tables live on the substrate catalog — this overlay never re-states them
- rail: numeric

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- `ColumnOrdering` law: `MinimumDegreeAtPlusA` (AMD on A+Aᵀ) is the ONLY valid mode for `SparseCholesky`/`SparseLDL`; `MinimumDegreeStS`/`MinimumDegreeAtA` serve the QR/least-squares path; `Natural` disables permutation.
- license: LGPL-2.1-only (copyleft — dynamic-linking obligation; keep CSparse a referenced assembly, never IL-merge or statically embed, and do not vendor its source into a Rasm assembly).
- `Tensor/factor` `SparseOps.ToCsc`/`SparseOps.Diagonal` are the canonical `CoordinateStorage<double>` → `CompressedColumnStorage<double>` finalization owners; every assembled operator (stiffness, mass, geometric, Laplacian) reaches a factorization through them, never through page-local `Converter` calls.

## [03]-[IMPLEMENTATION_LAW]

[MATRIX_TOPOLOGY]:
- namespaces: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`,
  `CSparse.Ordering`, `CSparse.IO`; `CSparse.Complex` mirrors the `double` surface for `Complex`.
- storage: CCS via `ColumnPointers` (length cols+1), `RowIndices`, `Values` on
  `CompressedColumnStorage<T>`; rows within a column must be sorted/unique for solvers — enforce with
  `Helper.SortIndices` / `ValidateStorage(strict)` after manual array surgery.
- scalar constraint: `T : struct, IEquatable<T>, IFormattable` — the `struct` bound is load-bearing
  (the generic surface is value-type only; do not parameterize with reference types).
- `CompressedColumnStorage<T>.AutoTrimStorage` controls whether arithmetic auto-trims trailing slack.

[FACTORIZATION_TOPOLOGY]:
- lifecycle: `Create` (symbolic analysis + numeric factorization) → `Solve`. `Refactorize` reuses the
  symbolic structure and is correct ONLY when the sparsity pattern is byte-identical; `SparseQR` has
  no `Refactorize` (it always re-derives the symbolic Householder structure).
- `SparseCholesky` requires SPD; `Update`/`Downdate` apply a rank-1 modification to the existing
  factor — the canonical path for incremental/active-set solves that re-solve after a single edge
  change, far cheaper than a full re-factor.
- `SparseCholesky`/`SparseLDL` accept only `ColumnOrdering.Natural` or `MinimumDegreeAtPlusA`.
- all four factorizations accept `IProgress<double>` reporting 0.0→1.0 for long symbolic phases.
- every factorization implements `ISolver<double>`; `SparseLU`/`SparseLDL`/`SparseCholesky` also
  implement `ISparseFactorization<double>` exposing `NonZerosCount`, so a Compute owner can hold the
  active factorization behind the `ISolver<double>` seam and swap LU↔LDL↔Cholesky by problem class.

[STACKING] — single sparse numeric rail with sibling Compute owners:
- assemble through `CoordinateStorage<T>.At` (NOT raw CCS array surgery): it accumulates duplicate
  `(i,j)` contributions, exactly the FEM/graph-Laplacian element-assembly pattern, then
  `SparseOps.ToCsc` finalizes once. Direct `ColumnPointers`/`RowIndices`/`Values`
  adoption is reserved for buffers that are already sorted and deduplicated.
- `Multiply(ReadOnlySpan<double>, Span<double>)` consumes RHS/solution vectors that
  `System.Numerics.Tensors` (`TensorPrimitives`) and `CommunityToolkit.HighPerformance`
  (`MemoryOwner<double>`/`Span2D`) own — no copy at the boundary.
- pair with `MathNet.Numerics`: CSparse owns the sparse direct factorizations MathNet lacks, while
  MathNet (+ the MKL/OpenBLAS providers) owns dense BLAS/LAPACK and iterative solvers; route a
  problem to CSparse for sparse LU/LDL/Cholesky/QR and to MathNet for dense or Krylov work.
- the OWNED frame spine (`Solver/discretization` `ShapeFamily.Frame` / `Beam2Euler` / `Beam2Timoshenko`
  local kernels in `Topology`) assembles its reduced global stiffness through `CoordinateStorage<double>.At`
  and factors it through THIS owner behind the one `Solver/contract` `SolveLane` — there is no external
  frame-FE package holding a parallel sparse-factor stack; continuum and frame problems share the same
  `ISparseFactorization<double>` seam.
- the NATIVE tier (`api-csparse-interop`): the `Tensor/factor` `FactorKind` cholmod/superlu/umfpack rows
  and the `SolveMethod.ArpackShiftInvert` eigensolve consume the SAME assembled
  `CompressedColumnStorage<double>` this owner finalizes — the managed factorizations here are the
  routing terminal chosen BEFORE a `NativeClaim`, never a silent post-claim fallback.
- expose a matrix-free seam via `ILinearOperator<T>` when the operator is implicit (a stencil or
  Schur complement) and only the matvec is needed by a downstream iterative driver.
- `.mtx` exchange (`MatrixMarketReader/Writer`) is the interchange format for fixtures and external
  solver hand-off; gate large reads through `Microsoft.IO.RecyclableMemoryStream`.

[LOCAL_ADMISSION]:
- Compute owns COO assembly, factorization, and solve. Treat `CoordinateStorage<T>` + `SparseOps.ToCsc` as
  the canonical intake; reach for raw `ColumnPointers`/`RowIndices`/`Values` only for pre-sorted,
  pre-deduplicated external buffers, and validate them with `Helper.ValidateStorage(strict:true)`.
- permutations enter as `int[]` through `Permutation` / `AMD`; never re-derive ordering from matrix
  structure inline.
- `ParallelMultiply` falls back to serial for small problems or a single processor;
  `ParallelOptions.MaxDegreeOfParallelism` caps the fan.

[RAIL_LAW]:
- Package: `CSparse`
- Owns: compressed-column + coordinate sparse storage, AMD ordering, direct factorization solvers
  (LU, Cholesky with update/downdate, LDL', QR), and Dulmage-Mendelsohn graph decomposition
- Accept: square solves via LU/LDL'/Cholesky, least-squares via QR, incremental SPD via update/downdate
- Reject: iterative/Krylov solvers, dense BLAS, eigensolvers, and general-purpose linear-algebra
  primitives — those route to `MathNet.Numerics` and its native providers; a second frame-FE package
  standing up a parallel sparse-factor stack beside the owned spine
