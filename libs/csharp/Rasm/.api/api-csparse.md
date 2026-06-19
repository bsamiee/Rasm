# [RASM_API_CSPARSE]

`CSparse` supplies direct sparse Cholesky, LDL', LU, and QR factorizations over
CSC-backed `CompressedColumnStorage<T>`, COO ingestion via `CoordinateStorage<T>`,
AMD fill-reducing ordering through `ColumnOrdering`, and CSC matrix-vector products
for the numeric lane's direct sparse solve path.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse`
- assembly: `CSparse`
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Factorization`, `CSparse.Ordering`, `CSparse.Storage`
- asset: runtime library (pure managed; no native dependencies)
- rail: numeric

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: storage carriers
- rail: numeric

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                               |
| :-----: | :---------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `CompressedColumnStorage<T>`  | abstract class | CSC matrix base; owns arrays and operators |
|   [2]   | `CSparse.Double.SparseMatrix` | concrete class | CSC double matrix; inherits CCS base       |
|   [3]   | `CoordinateStorage<T>`        | concrete class | COO triplet accumulator for CSC conversion |
|   [4]   | `DenseColumnMajorStorage<T>`  | concrete class | dense column-major matrix backing          |
|   [5]   | `SymbolicColumnStorage`       | concrete class | symbolic CSC used by factorization phase   |

[PUBLIC_TYPE_SCOPE]: ordering and seams
- rail: numeric

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------ | :------------ | :---------------------------------- |
|   [1]   | `ColumnOrdering`          | enum          | AMD fill-reducing ordering selector |
|   [2]   | `ILinearOperator<T>`      | interface     | matrix-vector multiply seam         |
|   [3]   | `ISparseFactorization<T>` | interface     | factorization solve seam            |
|   [4]   | `ISolver<T>`              | interface     | in-place solve seam                 |

[PUBLIC_TYPE_SCOPE]: double factorizations
- rail: numeric

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `CSparse.Double.Factorization.SparseCholesky` | concrete class | direct sparse Cholesky (SPD)            |
|   [2]   | `CSparse.Double.Factorization.SparseLDL`      | concrete class | direct sparse LDL' (symmetric)          |
|   [3]   | `CSparse.Double.Factorization.SparseLU`       | concrete class | direct sparse LU with partial pivoting  |
|   [4]   | `CSparse.Double.Factorization.SparseQR`       | concrete class | direct sparse QR (overdetermined/under) |

[PUBLIC_TYPE_SCOPE]: ordering enum cases
- rail: numeric

| [INDEX] | [SYMBOL]                              | [VALUE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------ | ------: | :-------------------------------------------- |
|   [1]   | `ColumnOrdering.Natural`              |       0 | natural column order; no fill reduction       |
|   [2]   | `ColumnOrdering.MinimumDegreeAtPlusA` |       1 | AMD on `A'+A`; valid for Cholesky and LDL'    |
|   [3]   | `ColumnOrdering.MinimumDegreeStS`     |       2 | AMD on `A'*A` dropping dense rows; LU/QR only |
|   [4]   | `ColumnOrdering.MinimumDegreeAtA`     |       3 | AMD on `A'*A`; LU/QR only                     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: matrix construction and COO ingestion
- rail: numeric

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE]   | [CAPABILITY]                |
| :-----: | :------------------------------------------------------ | :------------- | :-------------------------- |
|   [1]   | `SparseMatrix(rows, cols)`                              | constructor    | empty CSC double matrix     |
|   [2]   | `SparseMatrix(rows, cols, valueCount)`                  | constructor    | pre-sized CSC double matrix |
|   [3]   | `SparseMatrix(rows, cols, values, rowIndices, colPtrs)` | constructor    | CSC double from raw arrays  |
|   [4]   | `CoordinateStorage<T>(rows, cols, nz)`                  | constructor    | COO triplet accumulator     |
|   [5]   | `Converter.ToCompressedColumnStorage(storage)`          | static factory | COO to CSC conversion       |

[ENTRYPOINT_SCOPE]: CSC matrix-vector operations
- rail: numeric

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE] | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :----------- | :--------------------------- |
|   [1]   | `SparseMatrix.Multiply(ReadOnlySpan<double>, Span<double>)` | instance     | `y = Ax` sparse GEMV         |
|   [2]   | `SparseMatrix.Multiply(alpha, x, beta, y)`                  | instance     | `y = αAx + βy` scaled GEMV   |
|   [3]   | `SparseMatrix.TransposeMultiply(x, y)`                      | instance     | `y = A'x` transposed GEMV    |
|   [4]   | `SparseMatrix.TransposeMultiply(alpha, x, beta, y)`         | instance     | `y = αA'x + βy` scaled       |
|   [5]   | `SparseMatrix.Add(alpha, beta, other, result)`              | instance     | `result = αA + βB`           |
|   [6]   | `SparseMatrix.DropZeros(tolerance)`                         | instance     | structural zero removal      |
|   [7]   | `SparseMatrix.Keep(Func<i,j,v,bool>)`                       | instance     | predicate-based entry filter |
|   [8]   | `SparseMatrix.L1Norm` / `InfinityNorm` / `FrobeniusNorm`    | instance     | matrix norm family           |

[ENTRYPOINT_SCOPE]: factorization construction and solve
- rail: numeric

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE]   | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------- | :------------- | :-------------------------------------- |
|   [1]   | `SparseCholesky.Create(A, ColumnOrdering)`            | static factory | factors SPD CSC matrix                  |
|   [2]   | `SparseCholesky.Create(A, ColumnOrdering, IProgress)` | static factory | factors with progress reporting         |
|   [3]   | `SparseCholesky.Create(A, int[])`                     | static factory | factors with explicit permutation       |
|   [4]   | `SparseLDL.Create(A, ColumnOrdering)`                 | static factory | factors symmetric CSC matrix            |
|   [5]   | `SparseLDL.Create(A, ColumnOrdering, IProgress)`      | static factory | factors with progress reporting         |
|   [6]   | `SparseLU.Create(A, ColumnOrdering, tol)`             | static factory | factors square CSC matrix with pivot    |
|   [7]   | `SparseLU.Create(A, ColumnOrdering, tol, IProgress)`  | static factory | factors with progress and pivot tol     |
|   [8]   | `SparseLU.Create(A, int[], tol)`                      | static factory | factors with explicit column ordering   |
|   [9]   | `SparseQR.Create(A, ColumnOrdering)`                  | static factory | factors rectangular CSC matrix          |
|  [10]   | `SparseQR.Create(A, ColumnOrdering, IProgress)`       | static factory | factors with progress reporting         |
|  [11]   | `ISparseFactorization<T>.Solve(input, result)`        | instance       | `Ax=b` solve in place; arrays and spans |
|  [12]   | `SparseCholesky.NonZerosCount`                        | property       | L factor nonzero count                  |
|  [13]   | `SparseLU.NonZerosCount`                              | property       | L+U nonzero count (minus diagonal)      |
|  [14]   | `SparseLDL.NonZerosCount`                             | property       | L factor nonzero count                  |

## [4]-[IMPLEMENTATION_LAW]

[SPARSE_TOPOLOGY]:
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`
- storage: `CompressedColumnStorage<T>` (CSC) is the only factorization-ready storage form; `CoordinateStorage<T>` (COO) is the ingestion accumulator
- ingestion: `Converter.ToCompressedColumnStorage` converts COO to CSC; no intermediate CSR type in CSparse
- factorizations: all four (`SparseCholesky`, `SparseLDL`, `SparseLU`, `SparseQR`) consume `CompressedColumnStorage<double>` and solve through `ISparseFactorization<double>`
- ordering restriction: `SparseCholesky` and `SparseLDL` reject orderings beyond `MinimumDegreeAtPlusA`; `SparseLU` and `SparseQR` accept all four cases
- `SparseQR`: if `m >= n`, solves least-squares `min|Ax-b|`; if `m < n`, solves underdetermined system; auto-transposes internally for the underdetermined branch

[LOCAL_ADMISSION]:
- The numeric lane consumes CSparse via `CompressedColumnStorage<double>` at the direct-solve boundary; COO accumulation happens outside the factorization call.
- Factorization ordering is chosen once per matrix assembly; the choice is a `ColumnOrdering` discriminant, not a per-solver flag.
- `IProgress<double>` callbacks in `Create` report symbolic + numeric phase completion; pass `null` to skip reporting.
- `ISparseFactorization<double>.Solve` overwrites the result array in place; the caller owns the output buffer.

[RAIL_LAW]:
- Package: `CSparse`
- Owns: COO ingestion to CSC, direct sparse Cholesky/LDL'/LU/QR factorization and solve
- Accept: `CompressedColumnStorage<double>` for factorization, `CoordinateStorage<T>` for ingestion
- Reject: hand-rolled sparse factorization, CSR-native factorization, per-element indexed access as the primary solve path
