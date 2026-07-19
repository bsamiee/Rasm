# [RASM_API_CSPARSE]

`CSparse` supplies CSC-backed direct sparse Cholesky, LDL', LU, and QR factorizations; polymorphic dense, sparse, diagonal, and indexed ingestion; COO accumulation; AMD fill-reducing ordering; and the `ILinearOperator<T>` GEMV seam. `Refactorize` reuses symbolic analysis for a fixed sparsity pattern, while `SparseCholesky.Update` and `Downdate` apply rank-1 `L·L' ± w·w'` changes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse`
- assembly: `CSparse`
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Factorization`, `CSparse.Ordering`, `CSparse.Storage`
- asset: runtime library (pure managed, no native dependency; license LGPL-2.1-only)
- floor: net10.0 (multi-target net10.0/net8.0/netstandard2.0; the net10 consumer binds `lib/net10.0`)
- owners: `Rasm`, `Rasm.Compute`, `Rasm.Fabrication`
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: storage carriers
- rail: numeric

`CompressedColumnStorage<T>` is `Matrix<T>` (CSparse's own abstract base, not MathNet's) and owns the `Of*` ingestion family plus structural algebra; `SparseMatrix` is the `double` concrete that realizes the abstract `Multiply`/`Add`/`Keep`/`DropZeros`. `T: struct, IEquatable<T>, IFormattable` on every generic carrier.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `CompressedColumnStorage<T>`  | abstract class | CSC base; `Of*` ingestion, transpose, permute, GEMV seam        |
|  [02]   | `CSparse.Double.SparseMatrix` | concrete class | CSC double matrix; realizes GEMV/GEMM/norms/`Keep`/`DropZeros`  |
|  [03]   | `CoordinateStorage<T>`        | concrete class | COO triplet accumulator (`At`/`Keep`/`Transpose`) for CSC build |
|  [04]   | `DenseColumnMajorStorage<T>`  | concrete class | dense column-major backing (diagonal-extraction source)         |
|  [05]   | `SymbolicColumnStorage`       | concrete class | symbolic CSC produced by the analysis phase                     |

[PUBLIC_TYPE_SCOPE]: ordering and seams
- rail: numeric

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `ColumnOrdering`          | enum          | AMD fill-reducing ordering selector                                     |
|  [02]   | `ILinearOperator<T>`      | interface     | GEMV seam: `Multiply`/`TransposeMultiply`, plain + scaled, array + span |
|  [03]   | `ISolver<T>`              | interface     | in-place `Solve(input, result)` seam (array + span)                     |
|  [04]   | `ISparseFactorization<T>` | interface     | `ISolver<T>` + `NonZerosCount` (the fill metric)                        |

[PUBLIC_TYPE_SCOPE]: double factorizations
- rail: numeric

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]  | [CAPABILITY]                                                         |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `CSparse.Double.Factorization.SparseCholesky` | concrete class | direct sparse Cholesky (SPD); `Refactorize`/`Update`/`Downdate`      |
|  [02]   | `CSparse.Double.Factorization.SparseLDL`      | concrete class | direct sparse LDL' (symmetric indefinite); `Refactorize`             |
|  [03]   | `CSparse.Double.Factorization.SparseLU`       | concrete class | direct sparse LU, partial pivoting; `SolveTranspose`/`Refactorize`   |
|  [04]   | `CSparse.Double.Factorization.SparseQR`       | concrete class | direct sparse QR (least-squares / underdetermined); `SolveTranspose` |

[PUBLIC_TYPE_SCOPE]: ordering enum cases
- rail: numeric

| [INDEX] | [SYMBOL]                              | [VALUE] | [CAPABILITY]                                          |
| :-----: | :------------------------------------ | ------: | :---------------------------------------------------- |
|  [01]   | `ColumnOrdering.Natural`              |       0 | natural column order; no fill reduction               |
|  [02]   | `ColumnOrdering.MinimumDegreeAtPlusA` |       1 | AMD on `A'+A`; the only ordering Cholesky/LDL' accept |
|  [03]   | `ColumnOrdering.MinimumDegreeStS`     |       2 | AMD on `A'*A` dropping dense rows; LU/QR only         |
|  [04]   | `ColumnOrdering.MinimumDegreeAtA`     |       3 | AMD on `A'*A`; LU/QR only                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingestion — the polymorphic `Of*` owner
- rail: numeric

`CompressedColumnStorage<T>.Of*` is the single CSC ingestion surface — one static family discriminates by input shape (COO accumulator, dense/jagged array, row-/column-major flat array, indexed enumerable of `Tuple`/`ValueTuple`, diagonal, identity). `CoordinateStorage<T>` is the mutable accumulator; `Converter.From*` mints a COO directly from dense/enumerable data. There is no CSR type in CSparse.

| [INDEX] | [SURFACE]                                          | [KIND]   | [CAPABILITY]               |
| :-----: | :------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `CompressedColumnStorage<T>.OfIndexed(coo)`        | factory  | converts COO to CSC        |
|  [02]   | `CompressedColumnStorage<T>.OfIndexed(indexed)`    | factory  | converts indexed rows      |
|  [03]   | `CompressedColumnStorage<T>.OfArray`               | factory  | converts dense arrays      |
|  [04]   | `CompressedColumnStorage<T>.OfJaggedArray`         | factory  | converts jagged arrays     |
|  [05]   | `CompressedColumnStorage<T>.OfRowMajor`            | factory  | converts row-major buffers |
|  [06]   | `CompressedColumnStorage<T>.OfColumnMajor`         | factory  | converts column buffers    |
|  [07]   | `CompressedColumnStorage<T>.OfDiagonalArray`       | factory  | converts diagonals         |
|  [08]   | `CompressedColumnStorage<T>.CreateIdentity`        | factory  | creates identity CSC       |
|  [09]   | `SparseMatrix(rows, cols)`                         | ctor     | creates empty CSC          |
|  [10]   | `SparseMatrix(rows, cols, valueCount)`             | ctor     | pre-sizes CSC storage      |
|  [11]   | `SparseMatrix(rows, cols, values, rowIdx, colPtr)` | ctor     | wraps raw CSC arrays       |
|  [12]   | `CoordinateStorage<T>(rows, cols, nzmax)`          | ctor     | allocates COO storage      |
|  [13]   | `CoordinateStorage<T>.At`                          | instance | appends triplets           |
|  [14]   | `Converter.ToCompressedColumnStorage`              | factory  | finalizes COO to CSC       |
|  [15]   | `Converter.FromDenseArray<T>`                      | factory  | converts dense arrays      |
|  [16]   | `Converter.FromRowMajorArray<T>`                   | factory  | converts flat arrays       |
|  [17]   | `Converter.FromEnumerable<T>`                      | factory  | converts indexed rows      |

[ENTRYPOINT_SCOPE]: CSC algebra (`SparseMatrix` / `ILinearOperator<T>`)
- rail: numeric

GEMV is the `ILinearOperator<T>` seam: `Multiply`/`TransposeMultiply`, plain and scaled, over `ReadOnlySpan`/`Span` (and `T[]` overloads). Structural ops (`Transpose`/`Permute*`/`IsSymmetric`/`FindDiagonalIndices`/`EnumerateIndexed`) and `ParallelMultiply` round out the matrix surface.

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]   | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Multiply(ReadOnlySpan<double> x, Span<double> y)`             | instance       | `y = Ax` sparse GEMV                       |
|  [02]   | `Multiply(double alpha, x, double beta, y)`                    | instance       | `y = αAx + βy` scaled GEMV                 |
|  [03]   | `TransposeMultiply(x, y)` / `(alpha, x, beta, y)`              | instance       | `y = A'x` / `y = αA'x + βy`                |
|  [04]   | `Multiply(CompressedColumnStorage<double> B, result)`          | instance       | sparse matrix-matrix `C = AB`              |
|  [05]   | `ParallelMultiply(B, ParallelOptions = null)`                  | instance       | parallel sparse GEMM                       |
|  [06]   | `Add(double alpha, double beta, other, result)`                | instance       | `result = αA + βB`                         |
|  [07]   | `Transpose()` / `PermuteRows(int[])` / `PermuteColumns(int[])` | instance       | structural transpose / row-col permute     |
|  [08]   | `DropZeros(tolerance = 0)` / `Keep(Func<int,int,double,bool>)` | instance `int` | structural zero removal / predicate filter |
|  [09]   | `L1Norm()` / `InfinityNorm()` / `FrobeniusNorm()`              | instance       | matrix norm family                         |
|  [10]   | `NonZerosCount` / `IsSymmetric()` / `FindDiagonalIndices()`    | instance       | fill metric / symmetry / diagonal locator  |
|  [11]   | `EnumerateIndexed()` / `EnumerateIndexedAsValueTuples()`       | instance seq   | `(i, j, v)` triplet enumeration            |

[ENTRYPOINT_SCOPE]: factorization — construct, amortize, solve
- rail: numeric

`Create(A, ColumnOrdering[, IProgress<double>])` runs symbolic + numeric analysis; `Create(A, int[] p)` supplies an explicit permutation. `Refactorize(A)` re-runs the numeric phase on the SAME sparsity pattern reusing the cached ordering/elimination-tree/column-counts — the factor-once/re-solve-many amortization. `Solve` overwrites the caller-owned result in place.

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE]    | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `SparseCholesky.Create(A, ColumnOrdering[, IProgress])`         | static factory  | factor SPD CSC matrix                            |
|  [02]   | `SparseLDL.Create(A, ColumnOrdering[, IProgress])`              | static factory  | factor symmetric-indefinite CSC matrix           |
|  [03]   | `SparseLU.Create(A, ColumnOrdering, tol[, IProgress])`          | static factory  | factor square CSC matrix with pivot tol          |
|  [04]   | `SparseQR.Create(A, ColumnOrdering[, IProgress])`               | static factory  | factor rectangular CSC matrix                    |
|  [05]   | `<factorization>.Create(A, int[] p[,...])`                      | static factory  | factor with explicit fill-reducing permutation   |
|  [06]   | `<factorization>.Refactorize(A)` (LU: `Refactorize(A, tol)`)    | instance `void` | re-run numeric phase, reuse symbolic analysis    |
|  [07]   | `ISolver<double>.Solve(input, result)`                          | instance `void` | `Ax = b` in place; `double[]` and span overloads |
|  [08]   | `SparseLU.SolveTranspose(...)` / `SparseQR.SolveTranspose(...)` | instance `void` | `A'x = b` in place                               |
|  [09]   | `SparseCholesky.Update(w)` / `Downdate(w)`                      | instance `bool` | rank-1 `L·L' + w·w'` / `L·L' − w·w'`             |
|  [10]   | `ISparseFactorization<double>.NonZerosCount`                    | property `int`  | factor fill (L for Cholesky/LDL'; L+U−n for LU)  |

## [04]-[IMPLEMENTATION_LAW]

[SPARSE_TOPOLOGY]:
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`
- storage: `CompressedColumnStorage<T>` (CSC) is the only factorization-ready form; `CoordinateStorage<T>` (COO) is the accumulator; there is no CSR type in CSparse (CSR ingestion is the MathNet lane's `SparseCompressedRowMatrixStorage`, `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md`)
- ingestion: the `Of*` family is ONE polymorphic owner over input shape — COO accumulator, dense/jagged array, row-/column-major flat buffer, indexed enumerable, diagonal, identity — never a per-shape conversion helper; `Converter.From*` mints the COO from dense/enumerable data, `Converter.ToCompressedColumnStorage`/`OfIndexed` finalize to CSC
- ordering restriction: `SparseCholesky` and `SparseLDL` accept only `MinimumDegreeAtPlusA` (AMD on `A'+A`); `SparseLU` and `SparseQR` accept all four cases — the ordering is a `ColumnOrdering` discriminant chosen once per matrix assembly, never a per-solver flag
- `SparseQR`: `m ≥ n` solves the least-squares `min‖Ax−b‖`; `m < n` solves the underdetermined system (basic solution); `SolveTranspose` solves the dual

[AMORTIZATION]:
- `Refactorize` reuses the cached symbolic analysis (ordering, elimination tree, column counts) when the new matrix shares the prior sparsity pattern — for an iterated solve (an LM reject-loop that re-forms only the numeric values, a time-stepped system whose structure is fixed) the symbolic cost is paid once and every later solve is numeric-only
- `SparseCholesky.Update`/`Downdate` apply the rank-1 modification `L·L' ± w·w'` directly on the factor, avoiding a full refactor when a single constraint/edge is added or removed (incremental SPD systems)
- `NonZerosCount` is the fill receipt fact — the factor's nonzero count is the cost signal a `BenchmarkRow`/solve receipt carries, distinguishing a well-ordered factorization from fill-in blow-up

[STACK]:
- Rail: `MatrixKernel` lifts `SparseCholesky.Create(A, MinimumDegreeAtPlusA).Solve(b, x)` through `Try` onto the shared `Fin` result rail.
- Fault: A singular or indefinite factorization maps to `GeometryFault.SingularSystem` rather than escaping as an exception.
- Buffer: In-place `Solve` writes the caller-owned `x` buffer, avoiding a result allocation per iteration.
- The CSC matrix is assembled by sparse scatter into a `CoordinateStorage<double>` (the same accumulate-not-overwrite discipline the dense Jacobian uses for shared parameters), then `OfIndexed` finalizes once; the factorization then consumes the CSC and emits its fill count into the typed solve/`Benchmark` receipt
- The dense lane (`Matrix<double>.Cholesky`/`Svd`, `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md`) and this direct sparse lane meet at the `SolveReceipt` boundary: dense for small SPD normal equations, CSC + `SparseCholesky`/`SparseLU` when the system is large and sparse — the discriminant is matrix density, both rails fold the same typed receipt
- An iterative MathNet solver (`BiCgStab`/`GpBiCg`, `libs/csharp/Rasm.Compute/.api/api-mathnet-providers.md`) and a CSparse direct factorization are the two solve strategies behind one numeric-lane entry: direct when a reusable factor amortizes (`Refactorize`), iterative when the matrix is too large to factor — the strategy is a policy row, not a parallel public surface

[LOCAL_ADMISSION]:
- The numeric lane consumes CSparse via `CompressedColumnStorage<double>` at the direct-solve boundary; COO accumulation and the `Of*` finalize happen before the factorization call, never per-element indexed access as the solve path
- `IProgress<double>` callbacks in `Create` report symbolic + numeric phase completion; pass `null` to skip; `Refactorize` carries no progress arg (the symbolic phase is already cached)
- `ISolver<double>.Solve` overwrites the result array in place; the caller owns the output buffer — a fresh-array allocation per solve is the named defect when the buffer is reused across an iterate

[RAIL_LAW]:
- Package: `CSparse`
- Owns: polymorphic CSC ingestion, direct sparse Cholesky/LDL'/LU/QR factorization + solve, factor amortization (`Refactorize`/`Update`/`Downdate`), sparse GEMV/GEMM
- Accept: `CompressedColumnStorage<double>` for factorization, `CoordinateStorage<T>` / the `Of*` family for ingestion, `IProgress<double>` phase reporting, in-place span `Solve`
- Reject: hand-rolled sparse factorization, CSR-native factorization (CSparse has no CSR), per-element indexed access as the primary solve path, a fresh result buffer per iterate where `Refactorize` + buffer reuse applies
