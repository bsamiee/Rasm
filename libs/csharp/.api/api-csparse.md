# [RASM_API_CSPARSE]

`CSparse` owns the direct sparse solve lane over compressed-column storage: pattern-phase fill-reducing ordering and structural block decomposition, numeric factorization, and in-place triangular solve. Its boundary is the CSC matrix and the caller-owned result span; dense factorization and Krylov iteration stay outside.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSparse`
- package: `CSparse` (LGPL-2.1-only, Christian Woltering)
- assembly: `CSparse`
- namespace: `CSparse`, `CSparse.Storage`, `CSparse.Ordering`, `CSparse.Factorization`, `CSparse.IO`, `CSparse.Double`, `CSparse.Double.Factorization`, `CSparse.Complex`, `CSparse.Complex.Factorization`
- asset: runtime library, pure managed, zero native dependency
- abi: every generic carrier constrains `T : struct, IEquatable<T>, IFormattable`; `CSparse.Double` and `CSparse.Complex` carry mirror rosters over one generic surface
- rail: numeric

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: storage carriers, solver contracts, and the concrete realizations

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                                       |
| :-----: | :---------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `Matrix<T>`                   | abstract class | `ILinearOperator<T>` root: norms, GEMV, entry and vector access    |
|  [02]   | `CompressedColumnStorage<T>`  | abstract class | CSC carrier exposing its three index and value buffers directly    |
|  [03]   | `CoordinateStorage<T>`        | class          | COO triplet accumulator, duplicates summed at finalize             |
|  [04]   | `DenseColumnMajorStorage<T>`  | abstract class | dense column-major carrier with sub-matrix and triangle views      |
|  [05]   | `SymbolicColumnStorage`       | class          | pattern-only CSC the ordering and decomposition owners consume     |
|  [06]   | `SymbolicFactorization`       | class          | analysis cache (`pinv`, `q`, `parent`, `cp`) `Refactorize` reuses  |
|  [07]   | `ILinearOperator<T>`          | interface      | dimensions plus plain and scaled GEMV, forward and transposed      |
|  [08]   | `ISolver<T>`                  | interface      | in-place `Solve` over array and span                               |
|  [09]   | `ISparseFactorization<T>`     | interface      | `ISolver<T>` plus the `NonZerosCount` fill metric                  |
|  [10]   | `ColumnOrdering`              | enum           | fill-reducing permutation selector resolved once per assembly      |
|  [11]   | `SparseMatrix`                | class          | concrete CSC realizing GEMV, GEMM, add, norms, `Keep`, `DropZeros` |
|  [12]   | `DenseMatrix`                 | class          | concrete dense column-major over the same operator surface         |
|  [13]   | `SparseCholesky`              | class          | SPD factorization carrying `Refactorize`, `Update`, `Downdate`     |
|  [14]   | `SparseLDL`                   | class          | symmetric-indefinite LDL' carrying `Refactorize`                   |
|  [15]   | `SparseLU`                    | class          | square LU, partial pivoting, `Refactorize`, `SolveTranspose`       |
|  [16]   | `SparseQR`                    | class          | rectangular Householder QR carrying `SolveTranspose`               |
|  [17]   | `AMD`                         | static class   | fill-reducing permutation mint every ordering-taking `Create` runs |
|  [18]   | `DulmageMendelsohn`           | sealed class   | block-triangular decomposition and structural rank                 |
|  [19]   | `StronglyConnectedComponents` | sealed class   | strong-component blocks of the pattern digraph                     |

[PUBLIC_TYPE_SCOPE]: `ColumnOrdering` cases

| [INDEX] | [SYMBOL]               | [CONSUMER]             | [CAPABILITY]                            |
| :-----: | :--------------------- | :--------------------- | :-------------------------------------- |
|  [01]   | `Natural`              | every factorization    | identity permutation, no fill reduction |
|  [02]   | `MinimumDegreeAtPlusA` | every factorization    | AMD on `A'+A`                           |
|  [03]   | `MinimumDegreeStS`     | `SparseLU`, `SparseQR` | AMD on `A'*A` with dense rows dropped   |
|  [04]   | `MinimumDegreeAtA`     | `SparseLU`, `SparseQR` | AMD on `A'*A`                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `CompressedColumnStorage<T>` static ingestion — one family discriminating on input shape

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `OfIndexed(CoordinateStorage<T>, bool)`                    | factory  | finalize COO, `inplace` reusing the triplet arrays |
|  [02]   | `OfIndexed(int, int, IEnumerable<(int, int, T)>)`          | factory  | admit a value-tuple triplet sequence               |
|  [03]   | `OfIndexed(int, int, IEnumerable<Tuple<int, int, T>>)`     | factory  | admit a reference-tuple triplet sequence           |
|  [04]   | `OfArray(T[,])`                                            | factory  | admit a rectangular dense array                    |
|  [05]   | `OfJaggedArray(T[][])`                                     | factory  | admit a jagged dense array                         |
|  [06]   | `OfRowMajor(int, int, T[])`                                | factory  | admit a flat row-major buffer                      |
|  [07]   | `OfColumnMajor(int, int, T[])`                             | factory  | admit a flat column-major buffer                   |
|  [08]   | `OfDiagonalArray(T[])`                                     | factory  | admit a diagonal as a square CSC                   |
|  [09]   | `OfDiagonals(DenseColumnMajorStorage<T>, int[], int, int)` | factory  | admit banded diagonals at given offsets            |
|  [10]   | `OfMatrix(Matrix<T>)`                                      | factory  | admit any dense or sparse `Matrix<T>`              |
|  [11]   | `CreateDiagonal(int, T)`                                   | factory  | mint a constant-diagonal CSC                       |
|  [12]   | `CreateIdentity(int)`                                      | factory  | mint an identity CSC                               |
|  [13]   | `AutoTrimStorage`                                          | property | hold factor buffers across a `Refactorize` loop    |

[ENTRYPOINT_SCOPE]: triplet accumulation, raw-buffer wrapping, and text interchange

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `CoordinateStorage<T>(int, int, int)`                        | ctor     | allocate a triplet accumulator at an nz budget |
|  [02]   | `CoordinateStorage<T>(int, int, int[], int[], T[])`          | ctor     | wrap existing triplet arrays                   |
|  [03]   | `CoordinateStorage<T>.At(int, int, T)`                       | instance | append one triplet, growing the buffers        |
|  [04]   | `CoordinateStorage<T>.Keep(Func<int, int, T, bool>) -> int`  | instance | filter triplets before finalize                |
|  [05]   | `CoordinateStorage<T>.Transpose(bool)`                       | instance | swap the row and column index arrays           |
|  [06]   | `SparseMatrix(int, int, double[], int[], int[])`             | ctor     | wrap CSC buffers with zero copy                |
|  [07]   | `SparseMatrix(int, int, int)`                                | ctor     | pre-size CSC storage at an nz budget           |
|  [08]   | `MatrixMarketReader.ReadMatrix<T>(string)`                   | static   | read Matrix Market text into CSC               |
|  [09]   | `MatrixMarketReader.ReadStorage<T>(TextReader, bool)`        | static   | read Matrix Market text into COO               |
|  [10]   | `MatrixMarketWriter.WriteMatrix<T>(string, Matrix<T>, bool)` | static   | write any matrix as Matrix Market text         |

[ENTRYPOINT_SCOPE]: `CompressedColumnStorage<T>` instance algebra and structure

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `Multiply(ReadOnlySpan<T>, Span<T>)`                                | instance | `y = Ax`                                       |
|  [02]   | `Multiply(T, ReadOnlySpan<T>, T, Span<T>)`                          | instance | `y = αAx + βy`                                 |
|  [03]   | `TransposeMultiply(ReadOnlySpan<T>, Span<T>)`                       | instance | `y = A'x`                                      |
|  [04]   | `TransposeMultiply(T, ReadOnlySpan<T>, T, Span<T>)`                 | instance | `y = αA'x + βy`                                |
|  [05]   | `Multiply(CompressedColumnStorage<T>, CompressedColumnStorage<T>)`  | instance | `C = AB` into a caller-owned CSC               |
|  [06]   | `ParallelMultiply(CompressedColumnStorage<T>, ParallelOptions)`     | instance | the same GEMM across the thread pool           |
|  [07]   | `Add(T, T, CompressedColumnStorage<T>, CompressedColumnStorage<T>)` | instance | `C = αA + βB` into a caller-owned CSC          |
|  [08]   | `Transpose() -> CompressedColumnStorage<T>`                         | instance | transpose, conjugating on the `Complex` lane   |
|  [09]   | `Transpose(bool) -> CompressedColumnStorage<T>`                     | instance | storage-level transpose reading the CSC as CSR |
|  [10]   | `PermuteRows(int[])`                                                | instance | permute rows IN PLACE                          |
|  [11]   | `PermuteColumns(int[]) -> CompressedColumnStorage<T>`               | instance | permute columns into a NEW matrix              |
|  [12]   | `DropZeros(double) -> int`                                          | instance | drop entries at or under a tolerance           |
|  [13]   | `Keep(Func<int, int, T, bool>) -> int`                              | instance | retain entries a predicate admits              |
|  [14]   | `FindDiagonalIndices(bool) -> int[]`                                | instance | value-buffer positions of the diagonal         |
|  [15]   | `IsSymmetric() -> bool`                                             | instance | structural and numeric symmetry test           |
|  [16]   | `Equals(Matrix<T>, double) -> bool`                                 | instance | entrywise comparison at a tolerance            |
|  [17]   | `Row(int, Span<T>)`                                                 | instance | scatter one row into a caller-owned span       |
|  [18]   | `Column(int, Span<T>)`                                              | instance | scatter one column into a caller-owned span    |
|  [19]   | `EnumerateIndexed(Action<int, int, T>)`                             | instance | allocation-free `(i, j, v)` visit              |
|  [20]   | `EnumerateIndexedAsValueTuples()`                                   | instance | lazy `(row, column, value)` sequence           |
|  [21]   | `Clone(bool) -> CompressedColumnStorage<T>`                         | instance | copy the pattern, values optional              |
|  [22]   | `At(int, int) -> T`                                                 | instance | single-entry read by column binary search      |
|  [23]   | `NonZerosCount -> int`                                              | property | stored-entry count, `ColumnPointers[cols]`     |

[CSC_BUFFERS]: `Values` `RowIndices` `ColumnPointers`
[NORMS]: `L1Norm` `InfinityNorm` `FrobeniusNorm`

[ENTRYPOINT_SCOPE]: ordering, structural decomposition, and storage hygiene

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `AMD.Generate<T>(CompressedColumnStorage<T>, ColumnOrdering) -> int[]` | factory | mint the fill-reducing permutation            |
|  [02]   | `AMD.Generate(SymbolicColumnStorage, ColumnOrdering) -> int[]`         | factory | the same mint over a pattern-only matrix      |
|  [03]   | `DulmageMendelsohn.Generate<T>(CompressedColumnStorage<T>, int)`       | factory | block-triangular decomposition of the pattern |
|  [04]   | `StronglyConnectedComponents.Generate<T>(CompressedColumnStorage<T>)`  | factory | strong components of the pattern digraph      |
|  [05]   | `SymbolicColumnStorage.Create<T>(CompressedColumnStorage<T>, bool)`    | factory | strip values to a pattern-only carrier        |
|  [06]   | `Helper.ValidateStorage<T>(CompressedColumnStorage<T>, bool) -> bool`  | static  | verify sorted unique rows and pointer order   |
|  [07]   | `Helper.SortIndices<T>(CompressedColumnStorage<T>)`                    | static  | restore sorted rows after buffer surgery      |
|  [08]   | `Helper.TrimStorage<T>(CompressedColumnStorage<T>)`                    | static  | shrink buffers to the stored-entry count      |

[DULMAGE_MENDELSOHN]: `Blocks` `Singletons` `StructuralRank` `RowPermutation` `ColumnPermutation` `BlockRowPointers` `BlockColumnPointers` `CoarseRowDecomposition` `CoarseColumnDecomposition`
[STRONGLY_CONNECTED_COMPONENTS]: `Blocks` `BlockPointers` `Indices`
[SYMBOLIC_COLUMN_STORAGE]: `Add` `Multiply` `Transpose` `Permute` `Sort` `Clone` `Resize` `ColumnPointers` `RowIndices` `NonZerosCount`
[PERMUTATION]: `Create` `Invert` `IsValid` `Apply` `ApplyInverse`

[ENTRYPOINT_SCOPE]: factorization — construct, amortize, solve

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `SparseCholesky.Create(CCS<double>, ColumnOrdering, IProgress<double>)`   | factory  | factor an SPD matrix under an AMD mint         |
|  [02]   | `SparseLDL.Create(CCS<double>, ColumnOrdering, IProgress<double>)`        | factory  | factor a symmetric-indefinite matrix           |
|  [03]   | `SparseLU.Create(CCS<double>, ColumnOrdering, double, IProgress<double>)` | factory  | factor a square matrix at a pivot tol          |
|  [04]   | `SparseQR.Create(CCS<double>, ColumnOrdering, IProgress<double>)`         | factory  | factor a rectangular matrix                    |
|  [05]   | `SparseCholesky.Create(CCS<double>, int[], IProgress<double>)`            | factory  | factor under a caller-minted permutation       |
|  [06]   | `SparseLDL.Create(CCS<double>, int[], IProgress<double>)`                 | factory  | factor under a caller-minted permutation       |
|  [07]   | `SparseLU.Create(CCS<double>, int[], double, IProgress<double>)`          | factory  | factor under a caller-minted permutation       |
|  [08]   | `SparseCholesky.Refactorize(CCS<double>)`                                 | instance | re-run the numeric phase on the cached pattern |
|  [09]   | `SparseLDL.Refactorize(CCS<double>)`                                      | instance | re-run the numeric phase on the cached pattern |
|  [10]   | `SparseLU.Refactorize(CCS<double>, double)`                               | instance | the same re-run, re-clamping the pivot tol     |
|  [11]   | `ISolver<T>.Solve(ReadOnlySpan<T>, Span<T>)`                              | instance | `Ax = b` into the caller's span                |
|  [12]   | `SparseLU.SolveTranspose(ReadOnlySpan<double>, Span<double>)`             | instance | `A'x = b` into the caller's span               |
|  [13]   | `SparseQR.SolveTranspose(ReadOnlySpan<double>, Span<double>)`             | instance | `A'x = b` into the caller's span               |
|  [14]   | `SparseCholesky.Update(CCS<double>) -> bool`                              | instance | rank-1 `L·L' + w·w'` on the standing factor    |
|  [15]   | `SparseCholesky.Downdate(CCS<double>) -> bool`                            | instance | rank-1 `L·L' − w·w'` on the standing factor    |
|  [16]   | `ISparseFactorization<T>.NonZerosCount -> int`                            | property | factor fill count                              |

- `CCS<double>` abbreviates `CompressedColumnStorage<double>` inside the cells above.

[ENTRYPOINT_SCOPE]: `Double` and `Complex` kernel rosters

[VECTOR]: `Norm` `NormRobust` `DotProduct` `Axpy` `Scale` `Add` `Copy` `Clone` `Create` `Clear` `PointwiseMultiply`
[SOLVER_HELPER]: `SolveLower` `SolveLowerTranspose` `SolveUpper` `SolveUpperTranspose`
[CONSTANTS]: `MachineEpsilon` `EqualsThreshold` `SizeOfDouble` `SizeOfInt` `SizeOfComplex`
[DENSE_COLUMN_MAJOR_STORAGE]: `OfArray` `OfJaggedArray` `OfRowMajor` `OfColumnMajor` `OfMatrix` `OfIndexed` `OfDiagonalArray` `CreateIdentity` `CreateDiagonal` `SubMatrix` `SetSubMatrix` `SetRow` `SetColumn` `LowerTriangle` `UpperTriangle` `Transpose` `PointwiseMultiply` `ParallelMultiply`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CompressedColumnStorage<T>` is the only factorization-ready form and `CoordinateStorage<T>` the accumulator finalizing into it; a new input form lands as one `Of*` overload on that static family.
- Ordering resolves once per assembly: an ordering-taking `Create` mints the permutation through `AMD.Generate` and forwards to the `int[]` overload, so a caller holding its own permutation skips the mint. `SparseCholesky` and `SparseLDL` throw above `MinimumDegreeAtPlusA`.
- `Refactorize` re-runs the numeric phase against the cached `SymbolicFactorization` and binds only where the new matrix carries the prior pattern, so an iterate re-forming values alone pays the symbolic cost once.
- `SparseCholesky.Update` and `Downdate` move the standing factor by a rank-1 term, so one added or removed constraint costs no refactor.
- `SparseQR` transposes internally below `m = n`: at `m ≥ n` it returns the least-squares minimizer, below it the minimum-norm solution of the underdetermined system.
- `PermuteRows(int[])` mutates the receiver where `PermuteColumns(int[])` returns a new matrix.
- `NonZerosCount` is the fill receipt: `L` for Cholesky and LDL', `L + U − n` for LU, `Q + R − m` for QR.
- `ColumnPointers` runs `cols + 1` long and every solver reads its columns as sorted unique rows.

[STACKING]:
- `MathNet.Numerics`(`Rasm.Compute/.api/api-mathnet-providers.md`): CSR storage, dense factorization, and Krylov iteration are the peer lanes; matrix density and factor reuse select among the three, and all exit on one solve receipt.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Try.lift(...).Run()` traps a singular or indefinite factorization onto `Fin<A>` with a domain `Error` payload, so every `Create` and `Solve` crosses one rail seam.
- `System.Numerics.Tensors`(`.api/api-tensors.md`): `TensorPrimitives` folds over the same `Span<double>` GEMV and `Solve` write, so residual, axpy, and norm passes vectorize on the solve buffers with no copy.
- `QuikGraph`(`.api/api-quikgraph.md`): pattern-graph work stays on `SymbolicColumnStorage` — `DulmageMendelsohn` and `StronglyConnectedComponents` decompose the CSC directly, so a matrix never round-trips through a vertex-and-edge container.
- `MatrixKernel` threads the whole lane: a scatter accumulates shared parameters into `CoordinateStorage<double>`, `OfIndexed` finalizes once, the standing factorization is held behind `ISolver<double>` so LU, LDL', and Cholesky swap by problem class, and `NonZerosCount` lands in the emitted receipt.

[LOCAL_ADMISSION]:
- `CompressedColumnStorage<double>` is the numeric lane's entry shape; COO accumulation and the `Of*` finalize complete before the factorization call, and `At(int, int)` stays a diagnostic read.
- `Solve` overwrites a caller-owned buffer, so an iterate reuses one result array across every step and `AutoTrimStorage = false` holds the factor buffers with it.
- `IProgress<double>` reports the symbolic and numeric phases of `Create`; `Refactorize` carries none, its symbolic phase already cached.
- `Helper.ValidateStorage` gates any CSC assembled by direct buffer surgery, and `Helper.SortIndices` restores the row invariant it checks.

[RAIL_LAW]:
- Package: `CSparse`
- Owns: CSC and COO storage, polymorphic ingestion, fill-reducing ordering, structural block decomposition, direct Cholesky/LDL'/LU/QR factorization and solve, factor amortization, sparse GEMV and GEMM
- Accept: `CompressedColumnStorage<double>` at the factorization boundary, the `Of*` family and `CoordinateStorage<T>` for assembly, a caller-minted `int[]` permutation, `IProgress<double>` phase reporting, span `Solve` over a reused buffer
- Reject: a hand-rolled sparse factorization or minimum-degree ordering, per-element indexed access as the solve path, a fresh result buffer per iterate where `Refactorize` and buffer reuse apply
