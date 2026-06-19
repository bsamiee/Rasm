# [RASM_COMPUTE_API_CSPARSE]

`CSparse` supplies compressed-column sparse matrix storage, AMD fill-reducing ordering, and
direct factorization solvers — LU with partial pivoting, LDL' for symmetric matrices, and QR
for least-squares — over `double` and `Complex` scalars for numerical compute owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse`
- assembly: `CSparse`
- namespace: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`, `CSparse.Factorization`
- asset: runtime library
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: storage and contract family
- rail: numeric

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [RAIL]                            |
| :-----: | :--------------------------- | :----------------- | :-------------------------------- |
|  [01]   | `CompressedColumnStorage<T>` | abstract CCS owner | sparse matrix storage base        |
|  [02]   | `SparseMatrix`               | concrete CCS       | `double` compressed-column matrix |
|  [03]   | `ILinearOperator<T>`         | operator contract  | matrix-vector product seam        |
|  [04]   | `ColumnOrdering`             | ordering enum      | AMD ordering mode selection       |

[PUBLIC_TYPE_SCOPE]: factorization family
- rail: numeric

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]      | [RAIL]                              |
| :-----: | :---------- | :----------------- | :---------------------------------- |
|  [01]   | `SparseLU`  | LU factorization   | general square systems              |
|  [02]   | `SparseLDL` | LDL' factorization | symmetric positive definite systems |
|  [03]   | `SparseQR`  | QR factorization   | least-squares and underdetermined   |

[PUBLIC_TYPE_SCOPE]: utility family
- rail: numeric

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [RAIL]                         |
| :-----: | :------------ | :------------- | :----------------------------- |
|  [01]   | `Helper`      | static utility | storage validation and sorting |
|  [02]   | `Permutation` | static utility | permutation apply and invert   |
|  [03]   | `GraphHelper` | static utility | reachability graph operations  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: storage construction and arithmetic
- rail: numeric

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]   | [RAIL]                            |
| :-----: | :----------------------------------------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `SparseMatrix(rowCount, columnCount)`                        | constructor      | empty matrix construction         |
|  [02]   | `SparseMatrix(rowCount, columnCount, valueCount)`            | constructor      | pre-allocated matrix construction |
|  [03]   | `SparseMatrix(rows, cols, values, rowIndices, colPtrs)`      | constructor      | CCS array-backed construction     |
|  [04]   | `Multiply(ReadOnlySpan<double>, Span<double>)`               | matrix-vector    | Ax sparse matvec                  |
|  [05]   | `Multiply(alpha, x, beta, y)`                                | scaled matvec    | y = alpha*Ax + beta*y             |
|  [06]   | `TransposeMultiply(ReadOnlySpan<double>, Span<double>)`      | matrix-vector    | A'x sparse matvec                 |
|  [07]   | `Multiply(CompressedColumnStorage, CompressedColumnStorage)` | matrix-matrix    | sparse matrix product             |
|  [08]   | `ParallelMultiply(other, ParallelOptions?)`                  | parallel product | parallel sparse matrix product    |
|  [09]   | `Add(alpha, beta, other, result)`                            | matrix add       | alpha*A + beta*B into result      |
|  [10]   | `Keep(Func<int,int,double,bool>)`                            | filter           | structural drop by predicate      |
|  [11]   | `DropZeros(tolerance)`                                       | filter           | near-zero entry removal           |
|  [12]   | `L1Norm / InfinityNorm / FrobeniusNorm`                      | norm             | sparse matrix norm family         |

[ENTRYPOINT_SCOPE]: factorization create and solve
- rail: numeric

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]    | [RAIL]                            |
| :-----: | :-------------------------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `SparseLU.Create(A, ColumnOrdering, tol)`     | factory call      | LU with AMD ordering and pivoting |
|  [02]   | `SparseLU.Create(A, permutation, tol)`        | factory call      | LU with explicit permutation      |
|  [03]   | `SparseLU.Refactorize(A, tol)`                | numeric re-factor | reuse symbolic; re-factor values  |
|  [04]   | `SparseLU.Solve(ReadOnlySpan, Span)`          | solve             | solve Ax=b using stored factors   |
|  [05]   | `SparseLU.SolveTranspose(ReadOnlySpan, Span)` | solve             | solve A'x=b                       |
|  [06]   | `SparseLDL.Create(A, ColumnOrdering)`         | factory call      | LDL' with AMD ordering            |
|  [07]   | `SparseLDL.Create(A, permutation)`            | factory call      | LDL' with explicit permutation    |
|  [08]   | `SparseLDL.Refactorize(A)`                    | numeric re-factor | reuse symbolic; re-factor values  |
|  [09]   | `SparseLDL.Solve(ReadOnlySpan, Span)`         | solve             | solve Ax=b for symmetric SPD      |
|  [10]   | `SparseQR.Create(A, ColumnOrdering)`          | factory call      | QR with AMD ordering              |
|  [11]   | `SparseQR.Solve(ReadOnlySpan, Span)`          | solve             | least-squares or underdetermined  |
|  [12]   | `SparseQR.SolveTranspose(ReadOnlySpan, Span)` | solve             | solve A'x=b                       |

[ENTRYPOINT_SCOPE]: utility operations
- rail: numeric

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `Helper.ValidateStorage<T>(storage, strict)` | validation     | CCS structure validity check     |
|  [02]   | `Helper.SortIndices<T>(storage)`             | normalization  | sort row indices within columns  |
|  [03]   | `Helper.TrimStorage<T>(storage)`             | resize         | trim arrays to non-zero count    |
|  [04]   | `Helper.CumulativeSum(sum, counts, size)`    | accumulation   | prefix sum for column pointers   |
|  [05]   | `ColumnOrdering.Natural`                     | ordering value | no fill-reducing permutation     |
|  [06]   | `ColumnOrdering.MinimumDegreeAtPlusA`        | ordering value | AMD for symmetric factorizations |
|  [07]   | `ColumnOrdering.MinimumDegreeStS`            | ordering value | AMD on A'*A without dense rows   |
|  [08]   | `ColumnOrdering.MinimumDegreeAtA`            | ordering value | AMD on A'*A                      |

## [04]-[IMPLEMENTATION_LAW]

[MATRIX_TOPOLOGY]:
- namespaces: `CSparse`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Storage`
- storage format: CCS — `ColumnPointers`, `RowIndices`, `Values` arrays on `CompressedColumnStorage<T>`
- concrete types: `CSparse.Double.SparseMatrix` for `double`; a symmetric `Complex` variant is in `CSparse.Complex`
- scalar constraint: `T` must implement `IEquatable<T>` and `IFormattable`
- auto-trim: `CompressedColumnStorage<T>.AutoTrimStorage` controls whether `Resize(0)` is called after operations

[FACTORIZATION_TOPOLOGY]:
- all three factorizations follow: `Create` → symbolic analysis + numeric factorization → `Solve`
- `Refactorize` skips symbolic analysis; safe only when the sparsity pattern is unchanged
- `SparseLDL.Create` accepts only `ColumnOrdering.Natural` or `ColumnOrdering.MinimumDegreeAtPlusA`
- `SparseLU.NonZerosCount` returns L + U non-zeros minus n (shared diagonal pivot row)
- `IProgress<double>` accepted by all factorizations for range reporting from 0.0 to 1.0

[LOCAL_ADMISSION]:
- Compute owns CCS construction, factorization, and solve; direct access to `ColumnPointers`, `RowIndices`, and `Values` arrays is the canonical intake form.
- Permutations enter as `int[]` through `Helper` and `Permutation` utilities; never re-derive from matrix structure.
- `ParallelMultiply` falls back to serial when the problem is small or the processor count is 1; `ParallelOptions.MaxDegreeOfParallelism` caps parallelism.

[RAIL_LAW]:
- Package: `CSparse`
- Owns: compressed-column sparse storage, AMD ordering, and direct factorization solvers
- Accept: square-system solves via LU or LDL', least-squares via QR
- Reject: iterative solvers, dense BLAS, and general-purpose linear-algebra primitives
