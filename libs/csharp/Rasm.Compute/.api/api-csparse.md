# [RASM_COMPUTE_API_CSPARSE]

`CSparse` (CSparse.NET) is the managed port of Tim Davis' CSparse: compressed-column sparse storage,
a coordinate (triplet) assembly front-end, AMD fill-reducing ordering, and direct factorization
solvers — LU with partial pivoting, LDL' and Cholesky for symmetric matrices (Cholesky with
rank-1 update/downdate), and QR for least-squares — over `double` and `Complex` scalars, plus a
Dulmage-Mendelsohn / strongly-connected-component graph layer and Matrix Market I/O.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse` `4.4.0`
- assembly: `CSparse` (consumer-bound `lib/net10.0`; multi-targets `net8.0`, `netstandard2.0`)
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Complex`,
  `CSparse.Complex.Factorization`, `CSparse.Storage`, `CSparse.Factorization`, `CSparse.Ordering`,
  `CSparse.IO`
- license: LGPL-2.1-only (copyleft — dynamic-linking obligation; keep CSparse a referenced assembly,
  never IL-merge or statically embed, and do not vendor its source into a Rasm assembly)
- asset: pure-managed runtime library (no native dependency)
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: storage, assembly, and operator family
- rail: numeric

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Matrix<T>`                    | abstract root      | shared base of dense + sparse storage            |
|  [02]   | `CompressedColumnStorage<T>`   | abstract CCS owner | `ColumnPointers`/`RowIndices`/`Values` matrix    |
|  [03]   | `CoordinateStorage<T>`         | COO triplet owner  | canonical assembly front-end (`At` accumulation) |
|  [04]   | `SparseMatrix`                 | concrete CCS       | `double` compressed-column matrix                |
|  [05]   | `DenseMatrix` / `DenseColumnMajorStorage<T>` | dense        | dense interop + factorization fallback           |
|  [06]   | `Converter`                    | static converter   | COO ↔ CCS, dense/jagged/enumerable ingest        |
|  [07]   | `Vector`                       | static utility     | dense vector axpy/dot/norm helpers               |
|  [08]   | `ILinearOperator<T>`           | operator contract  | matrix-free matrix-vector product seam           |
|  [09]   | `ISolver<T>` / `ISparseFactorization<T>` | factorization contracts | unified `Solve` / `NonZerosCount` seam   |
|  [10]   | `ColumnOrdering`               | ordering enum      | AMD ordering mode selection                      |

[PUBLIC_TYPE_SCOPE]: factorization family (`CSparse.Double.Factorization`)
- rail: numeric

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]      | [RAIL]                                       |
| :-----: | :--------------- | :----------------- | :------------------------------------------- |
|  [01]   | `SparseLU`       | LU factorization   | general square systems (partial pivoting)    |
|  [02]   | `SparseCholesky` | Cholesky           | symmetric positive-definite; `Update`/`Downdate` |
|  [03]   | `SparseLDL`      | LDL' factorization | symmetric (indefinite-tolerant) systems      |
|  [04]   | `SparseQR`       | QR factorization   | least-squares and underdetermined            |

[PUBLIC_TYPE_SCOPE]: ordering, graph, and I/O family
- rail: numeric

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Helper`                                  | static utility | storage validation, sort, trim, prefix sum   |
|  [02]   | `Permutation`                             | static utility | permutation apply / invert / random         |
|  [03]   | `AMD` (`CSparse.Ordering`)                | static utility | approximate-minimum-degree ordering vectors  |
|  [04]   | `DulmageMendelsohn` / `MaximumMatching` / `StronglyConnectedComponents` | static graph | block-triangular decomposition + matching |
|  [05]   | `MatrixMarketReader` / `MatrixMarketWriter` (`CSparse.IO`) | static I/O   | `.mtx` round-trip                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: assembly and conversion (the canonical intake path)
- rail: numeric

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new CoordinateStorage<T>(rowCount, columnCount, nzmax)`                                    | constructor    | empty triplet builder                        |
|  [02]   | `CoordinateStorage<T>.At(int i, int j, T value)`                                            | accumulate     | append/accumulate a triplet (duplicates sum) |
|  [03]   | `Converter.ToCompressedColumnStorage<T>(CoordinateStorage<T>, cleanup=true, inplace=false)` | finalize       | COO → CCS, summing + sorting duplicates      |
|  [04]   | `Converter.FromEnumerable<T>(IEnumerable<(int,int,T)>, rows, cols)`                         | ingest         | sparse triple stream → COO                    |
|  [05]   | `Converter.FromDenseArray<T>(T[,])` / `FromJaggedArray<T>(T[][])`                            | ingest         | dense → COO (drops structural zeros)         |
|  [06]   | `Converter.FromColumnMajorArray<T>` / `FromRowMajorArray<T>(T[], rows, cols)`                | ingest         | flat dense buffer → COO                       |
|  [07]   | `CoordinateStorage<T>.Transpose(alloc=false)` / `Keep(Func<int,int,T,bool>)` / `Clear()`    | reshape        | triplet transpose / structural filter / reset |

[ENTRYPOINT_SCOPE]: CCS arithmetic, matvec, and norms (`SparseMatrix`)
- rail: numeric

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]   | [RAIL]                            |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `new SparseMatrix(rows, cols)` / `(rows, cols, valueCount)`                           | constructor      | empty / pre-allocated CCS         |
|  [02]   | `new SparseMatrix(rows, cols, double[] values, int[] rowIndices, int[] columnPointers)` | constructor   | adopt existing CCS arrays         |
|  [03]   | `void Multiply(ReadOnlySpan<double> x, Span<double> y)`                               | matrix-vector    | y = A·x                           |
|  [04]   | `void Multiply(double alpha, ReadOnlySpan<double> x, double beta, Span<double> y)`    | scaled matvec    | y = α·A·x + β·y                   |
|  [05]   | `void TransposeMultiply(ReadOnlySpan<double> x, Span<double> y)` / `(α, x, β, y)`     | matrix-vector    | y = Aᵀ·x (plain + scaled)         |
|  [06]   | `void Multiply(CompressedColumnStorage<double> other, CompressedColumnStorage<double> result)` | matrix-matrix | A·B into result                 |
|  [07]   | `CompressedColumnStorage<double> ParallelMultiply(other, ParallelOptions=null)`      | parallel product | parallel A·B (serial fallback)    |
|  [08]   | `void Add(double alpha, double beta, other, result)`                                  | matrix add       | α·A + β·B into result             |
|  [09]   | `int Keep(Func<int,int,double,bool>)` / `int DropZeros(tolerance=0.0)`                | filter           | structural drop by predicate / ε  |
|  [10]   | `double L1Norm()` / `InfinityNorm()` / `FrobeniusNorm()`                              | norm             | sparse matrix norms               |

[ENTRYPOINT_SCOPE]: factorization create and solve
- rail: numeric

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]    | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------------ | :---------------- | :-------------------------------------- |
|  [01]   | `SparseLU.Create(A, ColumnOrdering, double tol)` / `(A, int[] permutation, tol)` | factory call    | LU; AMD or explicit ordering + pivot tol |
|  [02]   | `SparseLU.Refactorize(A, tol)`                                                   | numeric re-factor | reuse symbolic; same pattern only       |
|  [03]   | `SparseLU.Solve(ReadOnlySpan<double>, Span<double>)` / `SolveTranspose(...)`     | solve             | A·x=b / Aᵀ·x=b                          |
|  [04]   | `SparseCholesky.Create(A, ColumnOrdering[, IProgress<double>])` / `(A, int[] p[, progress])` | factory call | Cholesky for SPD; progress 0→1   |
|  [05]   | `SparseCholesky.Update(CompressedColumnStorage<double> w)` / `Downdate(w)`       | rank-1 modify     | L·Lᵀ ± w·wᵀ without full re-factor       |
|  [06]   | `SparseCholesky.Refactorize(A)` / `Solve(ReadOnlySpan, Span)`                    | re-factor / solve | reuse symbolic / solve SPD              |
|  [07]   | `SparseLDL.Create(A, ColumnOrdering[, progress])` / `(A, int[] p[, progress])`   | factory call      | LDL'; `NonZerosCount = L` nnz           |
|  [08]   | `SparseLDL.Refactorize(A)` / `Solve(ReadOnlySpan, Span)`                         | re-factor / solve | reuse symbolic / solve symmetric        |
|  [09]   | `SparseQR.Create(A, ColumnOrdering[, progress])`                                 | factory call      | QR; no `Refactorize` (always symbolic)  |
|  [10]   | `SparseQR.Solve(ReadOnlySpan, Span)` / `SolveTranspose(...)`                     | solve             | least-squares (m≥n) / min-norm (m<n)    |

[ENTRYPOINT_SCOPE]: storage utilities, ordering, and graph decomposition
- rail: numeric

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `Helper.ValidateStorage<T>(CompressedColumnStorage<T>, bool strict=false)`           | validation     | CCS structure validity (T : struct)     |
|  [02]   | `Helper.SortIndices<T>(storage)` / `TrimStorage<T>(storage)`                          | normalize      | sort rows in column / trim to nnz       |
|  [03]   | `Helper.CumulativeSum(int[] sum, int[] counts, int size)`                             | accumulate     | prefix sum for column pointers          |
|  [04]   | `Permutation.Apply` / `Invert` / `Create` (`int[]`)                                   | permute        | apply / invert permutation vectors      |
|  [05]   | `AMD.Generate<T>(CompressedColumnStorage<T>, ColumnOrdering)`                         | order          | AMD permutation vector for a pattern    |
|  [06]   | `DulmageMendelsohn.Generate<T>(...)` / `MaximumMatching` / `StronglyConnectedComponents` | graph     | block-triangular form + matching        |
|  [07]   | `MatrixMarketReader.ReadMatrix<T>(...)` / `MatrixMarketWriter.WriteMatrix<T>(...)`    | I/O            | `.mtx` round-trip                       |

`ColumnOrdering`: `Natural` (no permutation), `MinimumDegreeAtPlusA` (AMD on A+Aᵀ; the only valid
mode for `SparseCholesky`/`SparseLDL`), `MinimumDegreeStS` (AMD on AᵀA, dense rows dropped),
`MinimumDegreeAtA` (AMD on AᵀA).

## [04]-[IMPLEMENTATION_LAW]

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

[STACKING] — single dense numeric rail with sibling Compute libs:
- assemble through `CoordinateStorage<T>.At` (NOT raw CCS array surgery): it accumulates duplicate
  `(i,j)` contributions, exactly the FEM/graph-Laplacian element-assembly pattern, then
  `Converter.ToCompressedColumnStorage` finalizes once. Direct `ColumnPointers`/`RowIndices`/`Values`
  adoption is reserved for buffers that are already sorted and deduplicated.
- `Multiply(ReadOnlySpan<double>, Span<double>)` consumes RHS/solution vectors that
  `System.Numerics.Tensors` (`TensorPrimitives`) and `CommunityToolkit.HighPerformance`
  (`MemoryOwner<double>`/`Span2D`) own — no copy at the boundary.
- pair with `MathNet.Numerics`: CSparse owns the sparse direct factorizations MathNet lacks, while
  MathNet (+ the MKL/OpenBLAS providers) owns dense BLAS/LAPACK and iterative solvers; route a
  problem to CSparse for sparse LU/LDL/Cholesky/QR and to MathNet for dense or Krylov work.
- expose a matrix-free seam via `ILinearOperator<T>` when the operator is implicit (a stencil or
  Schur complement) and only the matvec is needed by a downstream iterative driver.
- `.mtx` exchange (`MatrixMarketReader/Writer`) is the interchange format for fixtures and external
  solver hand-off; gate large reads through `Microsoft.IO.RecyclableMemoryStream`.

[LOCAL_ADMISSION]:
- Compute owns COO assembly, factorization, and solve. Treat `CoordinateStorage<T>` + `Converter` as
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
  primitives — those route to `MathNet.Numerics` and its native providers
