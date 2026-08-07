# [COMPUTE_FACTOR]

Rasm.Compute sparse-solve and kernel-lowering lane: the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SolveTerminal` partition preserving the caller's retry, and the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel with the `ShardDispatch` local-or-farm axis the dense GEMM reads. Every library refuses its own gates — `Iterator<T>` exposes no iteration count — so the criterion stack re-imposes each and every result leaves as a typed `ComputeReceipt` carrying the route variant, the scale-derived tolerance, and the recomputed true relative residual against the original operator. Distributed solves cross solely through the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc, which the `ShardDispatch.Farm` row-block sub-solve dials by reference.

## [01]-[INDEX]

- [02]-[SPARSE_SOLVE]: CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative with the solver ladder and the budget criterion; overdetermined sparse-QR least-squares route; `.mtx` exchange.
- [03]-[SPARSE_ALGEBRA]: `SparseTensorOpFamily` op axis over CSR storage; the `GemvForm` forward/adjoint held GEMV; SpMM/add/transpose/Kronecker/contract; `EinsumPlan` pairwise lowering over dense GEMM and sparse contract.
- [04]-[KERNEL_LOWERING]: tensor matrix/structural rows lower onto real GEMM/im2col/pool; shard fan-out.

## [02]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows carrying the CSR-conversion `ingest` delegate as row data; `FactorKind` `[SmartEnum<string>]` direct-factor rows carrying the capability columns (rank-1 edit, transpose-solve, inertia, reentrancy, symmetry), the fill-formula and transpose-solve-recovery delegate, AND the permutation-keyed `create` factory as row data; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis whose `Ladder` column also composes the rungs into the `CompositeSolver` fall-through row, with `MethodSetup` the `IIterativeSolverSetup<double>` projection of one row and the `IterationPolicy` record (tolerance · max-iter · criterion stack · preconditioner · clock · deadline · cancellation); `FactoredOp` the typed sparse-operator value owning the factorization instance, the cached AMD permutation, the `ColumnOrdering` it was factored with, symbolic fill, and kind discriminant; `Edit` `[Union]` the structural-edit dialect; `SparseOps` the direct-and-iterative sparse-solve fold over CSR-backed MathNet storage and CSparse CSC direct factorizations, and the `.mtx` exchange pair carrying both directions of the one portable operator correspondence — ingestion routes through the `SparseFormat` row's `ingest` delegate, direct factorization through the `FactorKind` row's `create` delegate, neither through a parallel `FrozenDictionary` keyed by the same enum.
- Cases: `SparseFormat` `ingest`-delegate rows csr · csc · coo · dok (4); `FactorKind` verified `create`-delegate rows spd · ldl · lu · qr (4, every row wired — `Ldl` binds `SparseLDL.Create`); `IterativeMethod` rows bicgstab · gpbicg · tfqmr · mlk-bicgstab · composite (5, the composite deriving its ladder from the four `Ladder` rows); `Edit` cases `Pin` · `Prune` · `Bump` · `Revalue` (4, every case realized, admitted before mutation, and carrying its own structural-re-gate column).
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — the ONE sparse admission seam: extents, minor/values congruence, the pointer-form major run (length axis+1, `[0]==0`, `[^1]==nnz`, monotone) or the index-form major bounds, minor-index bounds, and one vectorized `TensorPrimitives.IsFiniteAll` values pass all gate BEFORE the storage factory, each refusal a typed `PayloadOverBounds` fault — the format row's `PointerForm`/`MajorIsRow` columns drive the one shared body, never a per-format admission path; `public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor)` converts the CSR triplets once to a CSparse `CompressedColumnStorage<double>` through `CoordinateStorage` + the admitted `CompressedColumnStorage<double>.OfIndexed` CSC factory, reads the symbolic fill before the numeric sweep, and collapses the completed factorization to one `FactoredOp` value; `FactoredOp.Solve(double[] rhs, double cap)` is the one polymorphic solve over both shapes — a square operator routes the forward triangular solve and a rectangular operator on the `Qr` kind routes the SparseQR least-squares `min‖Ax−b‖`, both landing in an `A.ColumnCount`-length result (CSparse sizes the caller buffer at `n` for every kind and allocates the augmented `S.m2` work row INTERNALLY — that augmentation is private factor state, never a caller dimension to over-size), the witness recomputing the true relative residual against the ORIGINAL rectangular `A` through the `ILinearOperator<double>` vector GEMV `A` inherits from the CSparse `Matrix<T>` base — `A` (a `CompressedColumnStorage<double>`) calls `A.Multiply(x, ax)` directly because `CSparse.Matrix<T> : ILinearOperator<double>` declares the array/span vector multiply the concrete `SparseMatrix` overrides, `ax` sized `A.RowCount` — rather than the square normal-equations operator — so a sparse PCE fit, a sparse-Jacobian recovery, and an overdetermined FEM normal-equations recovery solve through the one `FactoredOp` capsule without densifying to `Matrix<double>.QR`; `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected `IIterativeSolver<double>` under the explicitly-ordered criterion stack and returns the field with the recomputed true relative residual and `SolveTerminal` verdict — the iteration count is NOT read from `Iterator<double>` (which exposes the terminal `Status` and the `DetermineStatus`/`Cancel`/`Reset` drivers but no iteration count), it is the criterion-stack-bounded cap; `ReadExchange(StreamPool pool, CorrelationId correlation, long byteLength, Stream source)` and `WriteExchange(StreamPool pool, CorrelationId correlation, FactoredOp op)` are the `.mtx` ingest and reproduction ends of one correspondence, both staging through the pooled recyclable stream.
- Auto: every format row maps to one CSR ingestion conversion through the `SparseFormat` row's `ingest` delegate — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type and the build closure rides the row, not a parallel ingestion table; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `FactorKind` row's `create` delegate binding the explicit-permutation `SparseCholesky.Create(csc, p)`/`SparseLDL.Create(csc, p)`/`SparseLU.Create(csc, p, pivotTol)` and the ordering-based `SparseQR.Create(csc, ordering)`, so the AMD ordering is computed once by `Build` and the symmetric/lu kinds reuse that permutation rather than re-deriving it inside `Create`, then solve in place through `ISparseFactorization<double>.Solve(double[], double[])` (the residual witness calls the vector GEMV directly on the `CompressedColumnStorage<double>` operator, inherited from the CSparse `Matrix<T> : ILinearOperator<double>` base — no residency cast); iterative solves run the `IterativeMethod` row's `Solver(policy)` factory under the `IterationPolicy.Iterator()` `Iterator<double>` criterion stack constructed in precedence order `Failure → Budget → Divergence → Residual → IterationCount`, the `composite` row folding the four `Ladder` rows into a `CompositeSolver` that falls through to the next rung on divergence, breakdown, or a swallowed throw instead of returning one method's failure; `FactoredOp.TransposeSolve` recovers the transpose-solve action from the `FactorKind` row's `TransposeRecover` delegate column alone (some for lu and qr, none for spd and ldl) because the shared `ISparseFactorization<double>` exposes only the forward solve and `SolveTranspose` closes over the concrete `SparseLU`/`SparseQR`.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, factor kind, the symbolic fill, the recomputed true relative residual, row and column extents, the `ValueCount` non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, Microsoft.IO.RecyclableMemoryStream, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `SparseFormat` row carrying its `ingest` delegate; a new direct solver is one `FactorKind` row carrying its capability, symmetry, fill, transpose-recovery, AND `create` columns together (one row, never a row beside a parallel `DirectSolvers` table edit); a new iterative method is one `IterativeMethod` row carrying its `Ladder` column and its `IIterativeSolver<double>` factory column, and the composite ladder absorbs it with no fold edit; a new structural-edit dialect is one `Edit` case carrying its `Regates` column with its arm on the total `Apply` Switch; a new iteration knob is one column on the `IterationPolicy` record and one criterion on `Composed`, minted through `IterationPolicy.Of` so the lane's clock, budget, and token stay the record's only ambient-free ingress; zero new surface.
- Boundary — storage: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage, so csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner per format is the deleted form. The CSR-to-CSC handoff builds a `CoordinateStorage<double>(rows, cols, nnz)`, calls `.At(i, j, v)` per entry, and converts once through the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` factory — the CSparse static that internally runs `Converter.ToCompressedColumnStorage` with cleanup — so a hand-rolled `Converter` detour beside it is the named reimplementation defect, and `inplace: true` is rejected wherever the triplet must survive a structural-edit increment because it invalidates the source arrays and dangles references. Bare `SparseMatrix` is reserved for the MathNet CSR concrete (`SolveIterative`), so the two sparse libraries never alias one name.
- Boundary — admission: `CSparse.Helper.ValidateStorage(csc, strict: true)` gates the CSC inside `Factor` before any factory touches it, because it returns `bool` rather than throwing and factorizing invalid storage produces silently incorrect factors.
- Boundary — ordering and fill: the `ColumnOrdering.MinimumDegreeAtPlusA` permutation `int[]` from `CSparse.Ordering.AMD.Generate(CompressedColumnStorage<double>, ColumnOrdering)` caches as the value-only refactor key over an invariant pattern (`ColumnOrdering` values are `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, `MinimumDegreeAtA`; the AMD ordering type lives in `CSparse.Ordering`, distinct from the `ColumnOrdering` enum, and its `Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order)` takes the matrix first and the ordering second). Symbolic fill is read before the numeric sweep to route direct versus iterative, and the count is per-kind through the `FactorKind.Fill` delegate column — one factor for the symmetric kinds, `L + U − n` for `SparseLU`, `Q + R − m` for `SparseQR` — so a bare fill integer compared across kinds is meaningless. Assembly residue drops with a structural tolerance near `machineEps · ‖A‖_F` through `DropZeros(tolerance)` because the default `0.0` removes only binary zeros, and `SparseLU` pivot `tol` is `[0, 1]` as a relative column threshold (`1` full partial pivoting, `0` disabled), never an absolute floor.
- Boundary — structural gate: `DulmageMendelsohn.Generate(csc)` runs on the DEFAULT seed, because that second parameter is a randomization SEED selecting the maximum-matching order (`0` natural, `-1` reverse, anything else random) and NOT a dimension — feeding it a row count picks a randomized matching whose block boundaries move run to run, which forfeits the replayability every other axis on this page holds. `Generate` returns `null` when the matching or either breadth-first sweep fails, so the null converts to the typed refusal rather than dereferencing. Structural rank reads `dm.StructuralRank` (the library's own `rr[3]`) and the deficiency is `min(m, n) − StructuralRank`; a hand-derived deficiency over coarse-block arithmetic is the deleted reimplementation. The coarse decomposition is the 3-by-3 block structure of `A(p, q)`, `CoarseRowDecomposition` and `CoarseColumnDecomposition` both `int[5]` boundary arrays: rows split `[rr[0], rr[1])` under-determined, `[rr[1], rr[2])` square well-determined, `[rr[2], rr[3])` over-determined, `[rr[3], rr[4] = m)` unmatched; columns split `[cc[0], cc[1])` unmatched, `[cc[1], cc[2])` under-determined, `[cc[2], cc[3])` square, `[cc[3], cc[4] = n)` over-determined. The refusal localizes the two deficient blocks off those spans, so a caller reads WHERE the pattern failed rather than a bare rank integer.
- Boundary — capability recovery: transpose-solve, rank-1 edit, inertia, and reentrancy recover from the `FactorKind` row alone because the shared `ISparseFactorization<double>` exposes only the forward solve. The `Ldl` symmetric-indefinite/inertia kind binds `SparseLDL.Create` as a real `create` row — a capability row with no factory delegate (a `<sparse-direct-miss>` fall-through) is the named declared-but-unbound defect. An asymmetric input to a symmetric kind factors as its symmetrization and returns a correct answer to the WRONG system, so the post-solve true residual is the only signal it fires.
- Boundary — failure capture and reentrancy: a typed-only catch at the factorization boundary is rejected because SPD pivot loss and the zero-diagonal break throw bare `Exception`. The cached square factorization's one constructor-allocated scratch is non-reentrant, so solves serialize through the `FactoredOp` capsule and the `SparseQR` reentrant kind is the one parallel-safe row. The cache populates success-only, so only residual-witnessed factorizations enter and a diverged solve never poisons reuse.
- Boundary — rectangular least squares: the result buffer sizes from `A.ColumnCount` exactly like the square solve, because `SparseQR.Solve` writes the `n`-length left-hand side and allocates the augmented `S.m2` work row INTERNALLY as private factor state with no public accessor — over-sizing the caller buffer from a nonexistent "solution dimension" member is the named phantom. The `Qr` row is the one rectangular route on `FactoredOp.Solve`, so an overdetermined sparse system (`Solver/contract#SOLVE_CONTRACT` normal-equations recovery, `Solver/uncertainty#UNCERTAINTY_LANE` PCE coefficient fit, `Tensor/dispatch#EQUIVALENCE_INTEROP` sparse-Jacobian recovery) minimizes `‖Ax−b‖` through `SparseQR.Solve` and the witness recomputes against the ORIGINAL rectangular `A` (`ax` sized `A.RowCount`, the m-residual against the b-vector) — never a dense `Matrix<double>.QR` fallback and never the square normal-equations operator whose conditioning the rectangular QR avoids.
- Boundary — GEMV ownership is ONE spelling PER LIBRARY, because the two solve legs hold two operator types and neither converts to reach the other's kernel. Every CSparse `CompressedColumnStorage<double>` path — the direct-solve witness, the adjoint sweep, the einsum contract — routes `SparseTensorOps.Spmv`, calling `Multiply`/`TransposeMultiply` on the held operator because CSparse's `Matrix<T>` base implements `ILinearOperator<double>` and declares the vector `Multiply(ReadOnlySpan<double>, Span<double>)`/`Multiply(double[], double[])` the concrete `SparseMatrix` overrides; a residency cast to `CSparse.Double.SparseMatrix` to reach a member the base already exposes is the deleted ceremony, and the `double[]` operands bind the inherited array GEMV with no collision against the matrix-matrix overloads `Multiply(CompressedColumnStorage<double>[, result])`. The iterative leg holds a MathNet `SparseMatrix` the Krylov solver itself constructed, so its residual reads `matrix.Multiply(x)` on THAT operator — converting an iterate's operand to CSC per solve to force one spelling pays a full conversion for a norm.
- Boundary — edit dialect: every `Edit` applies to the operator before re-factoring — `Pin` drops row+column `node` and seats a unit diagonal, `Prune` `DropZeros` over a clone, a rank-1-edit kind's `Bump` runs the `SparseCholesky` `Update`/`Downdate` and discards-and-reconstructs the BUMPED operator (never the unedited one) on a `false` result, a non-rank-1-edit `Bump` accumulates `A + sign·w·wᵀ` over the column support and re-factors, and a `Bump` on a rectangular operator is rejected because a symmetric rank-1 update is ill-defined there; a default arm that silently re-factors the unedited operator and drops the payload is the deleted form. A value-only `Revalue` clones the CSC through `Clone()` before overwriting the value array, because the old `FactoredOp` still references the original storage and an in-place `CopyTo` corrupts the pre-edit operator, then re-creates with the SAME `op.Kind` from the cached permutation — a hardcoded `SparseLU.Create` re-create silently changing a non-LU operator's kind is the deleted correctness defect. The explicit-permutation `Create` amortizes the dominant symbolic cost and yields a fully INDEPENDENT factor, so the in-place CSparse `Refactorize` — which reuses the elimination tree and column counts too but MUTATES the shared factor instance, aliasing the pre-edit `FactoredOp` whose `Inner` other readers and the non-reentrant single-owner solve still hold — is deliberately not taken: value immutability outranks the marginal numeric-phase saving, and `SparseQR` exposes no `Refactorize` at all.
- Boundary — re-gate discriminant: the structural gate re-runs for the edits that can REMOVE pattern entries and skips for the rest, read off the `Edit` row's own `Regates` column rather than an edit-name test at the call. `Pin` drops a whole row and column and `Prune` drops residue whose removal can empty a row, so either can lower a structural rank the pre-edit gate proved; `Bump` only ADDS entries over the column support and `Revalue` rewrites values under an invariant pattern, so neither can, and re-running the Dulmage-Mendelsohn sweep on them pays a full pattern decomposition for an answer that cannot change.
- Boundary — iterative axis: the method is the closed `IterativeMethod` SmartEnum and a raw-`string` discriminant beside it is the named defect. The criterion stack constructs explicitly in precedence order because insertion order IS precedence — `Failure` first keeps `NaN` terminal, and `Residual` before the count cap suppresses convergence on the final iteration. Preconditioners initialize outside the solve and catch their throw there, because the init throw otherwise escapes the verdict-returning entrypoint, and that pre-initialize runs for the `Ladder` rows alone since `CompositeSolver` resolves each rung's preconditioner in its own constructor and passes `setup ?? argument`, making the seam's argument provably dead on the composite row where a second incomplete factorization is pure waste. The ladder swallows every rung throw and falls through, so a breakdown inside it is invisible to the caller and the recomputed true residual is the only gate. The budget criterion anchors on ONE instant captured at `Iterator()` construction, so the ladder's per-rung `Iterator.Reset()` — which clears only criterion status — cannot re-arm it, where the per-rung `IterationCountStopCriterion` does re-arm and an unbounded ladder burns `rungs × MaxIterations`. `MethodSetup` binds `double.NaN` to `SolutionSpeed`/`Reliability` because no producer measured them and the ONE member reading them is `SolverSetup<T>.LoadFromAssembly`, the reflection discovery form this lane rejects, so a zero there ranks a ladder nothing measured.
- Boundary — witness and terminal: the iterate is admitted only on the independently recomputed true relative residual against the original operator, because the converged verdict certifies solely that the PRECONDITIONED residual fell below tolerance and left preconditioning distorts the norm. The structural substitution path is the most dangerous form because it certifies an arbitrary iterate under a normal verdict and the ULP guard fails open on `NaN`. A deadline breach is budget exhaustion and lands `StoppedWithoutConvergence` so the partial iterate survives a relaxed-criterion retry, while cancellation lands its own verdict and is never transient. The terminal partition is three-way — `Converged` admits, `StoppedWithoutConvergence` exhausts, and divergence, breakdown, cancellation, and a `Continue` terminal each fail the rail — because folding them into `Exhausted` publishes a diverged iterate as a retryable partial.
- Boundary — exchange: `.mtx` rides `MatrixMarketReader`/`MatrixMarketWriter` through the pooled recyclable stream and never a bare `FileStream`. `ReadStorage` runs `autoExpand: true` as fixed law, because an unexpanded symmetric file lands one stored triangle the structural read then calls rank-deficient, and a MatrixMarket ARRAY file throws `NotSupportedException` which converts to the typed refusal at the trap. The write binds the `StreamWriter` overload alone because the `string` and `Stream` siblings DROP the `symmetric` argument on their way to it and emit a `general` header for a symmetric operator, and that flag reads off the `FactorKind.Symmetric` column rather than a caller knob.
- Boundary — fan-out: the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseFormat {
    // Rows carry CSR conversion plus pointer-form and major-axis shape; one admission body reads those columns.
    public static readonly SparseFormat Csr = new("csr", pointerForm: true, majorIsRow: true, static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Csc = new("csc", pointerForm: true, majorIsRow: false, static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(r, c, vals.Length, minor, major, vals));
    public static readonly SparseFormat Coo = new("coo", pointerForm: false, majorIsRow: true, static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Dok = new("dok", pointerForm: false, majorIsRow: true, static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(r, c, vals.Select((v, k) => Tuple.Create(major[k], minor[k], v))));

    private readonly Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>> ingest;

    public bool PointerForm { get; }
    public bool MajorIsRow { get; }

    public SparseCompressedRowMatrixStorage<double> Ingest(int rows, int columns, int[] major, int[] minor, double[] values) =>
        ingest(rows, columns, major, minor, values);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FactorKind {
    // Capability, symmetry, fill, transpose recovery, and factory remain one row; symmetric and LU rows consume cached AMD permutations.
    public static readonly FactorKind Spd = new("spd", rank1Edit: true, transposeSolve: false, inertia: false, reentrant: false, symmetric: true,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseCholesky.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Ldl = new("ldl", rank1Edit: false, transposeSolve: false, inertia: true, reentrant: false, symmetric: true,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseLDL.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Lu = new("lu", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: false, symmetric: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, perm, _, tol) => SparseLU.Create(csc, perm, tol),
        transposeRecover: static inner => inner is SparseLU lu ? Some<Action<double[], double[]>>(lu.SolveTranspose) : None);
    public static readonly FactorKind Qr = new("qr", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: true, symmetric: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, _, ordering, _) => SparseQR.Create(csc, ordering),
        transposeRecover: static inner => inner is SparseQR qr ? Some<Action<double[], double[]>>(qr.SolveTranspose) : None);
    private readonly Func<int, int, int, int> fill;
    private readonly Func<CompressedColumnStorage<double>, int[], ColumnOrdering, double, ISparseFactorization<double>> create;
    private readonly Func<ISparseFactorization<double>, Option<Action<double[], double[]>>> transposeRecover;

    public bool Rank1Edit { get; }
    public bool TransposeSolve { get; }
    public bool Inertia { get; }
    public bool Reentrant { get; }
    public bool Symmetric { get; }

    public int Fill(int nonZeros, int rows, int columns) => fill(nonZeros, rows, columns);
    public ISparseFactorization<double> Create(CompressedColumnStorage<double> csc, int[] permutation, ColumnOrdering ordering, double pivotTol) => create(csc, permutation, ordering, pivotTol);
    public Option<Action<double[], double[]>> TransposeRecover(ISparseFactorization<double> inner) => transposeRecover(inner);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IterativeMethod {
    public static readonly IterativeMethod BiCgStab = new("bicgstab", ladder: true, solverType: typeof(BiCgStab), static _ => new BiCgStab());
    public static readonly IterativeMethod GpBiCg = new("gpbicg", ladder: true, solverType: typeof(GpBiCg), static _ => new GpBiCg());
    public static readonly IterativeMethod Tfqmr = new("tfqmr", ladder: true, solverType: typeof(TFQMR), static _ => new TFQMR());
    public static readonly IterativeMethod MlkBiCgStab = new("mlk-bicgstab", ladder: true, solverType: typeof(MlkBiCgStab), static _ => new MlkBiCgStab());
    // Ladder row: `Items` reads inside the DEFERRED body, never at field-initializer time, so the four rungs are
    // materialized before the fold runs; `Ladder` keeps the composite out of its own ladder by column rather than
    // by name, so a further non-ladder row is one column value.
    public static readonly IterativeMethod Composite = new("composite", ladder: false, solverType: typeof(CompositeSolver),
        static policy => new CompositeSolver(toSeq(Items).Filter(static row => row.Ladder)
            .Map(row => (IIterativeSolverSetup<double>)new MethodSetup(row, policy, policy.Preconditioner()))));

    private readonly Func<IterationPolicy, IIterativeSolver<double>> build;

    public bool Ladder { get; }

    public Type SolverType { get; }

    public IIterativeSolver<double> Solver(IterationPolicy policy) => build(policy);
}

// `CompositeSolver`'s constructor reads `CreateSolver`/`CreatePreconditioner` ALONE and preserves enumeration
// order, so the ladder order IS `Items` declaration order; `SolutionSpeed`/`Reliability` are read only by
// `SolverSetup<T>.LoadFromAssembly`, the reflection discovery form this lane rejects (assembly scan, NativeAOT-
// hostile, ordering by an unmeasurable speed-over-reliability quotient). Those two publish `double.NaN` because
// no producer measured them and an unmeasured figure spells absence, never a zero a loader would silently rank.
public sealed record MethodSetup(IterativeMethod Row, IterationPolicy Policy, IPreconditioner<double> Preconditioner) : IIterativeSolverSetup<double> {
    public Type SolverType => Row.SolverType;
    public Type PreconditionerType => Preconditioner.GetType();
    public double SolutionSpeed => double.NaN;
    public double Reliability => double.NaN;

    public IIterativeSolver<double> CreateSolver() => Row.Solver(Policy);
    public IPreconditioner<double> CreatePreconditioner() => Preconditioner;
}

public sealed record IterationPolicy(
    double Tolerance,
    int MaxIterations,
    double DivergenceIncrease,
    int DivergenceFloor,
    Func<IPreconditioner<double>> Preconditioner,
    IClock Clock,
    Duration Deadline,
    CancellationToken Cancel) {
    // Clock, wall-clock budget, and token belong to the lane, never to this record: a canonical static
    // binding `SystemClock.Instance` beside a literal deadline mints a second clock the receipt durations do not
    // read and a cap no budget derived, so the settled numeric canon mints only through the seam that supplies
    // all three.
    public static IterationPolicy Of(IClock clock, Duration deadline, CancellationToken cancel) =>
        new(1e-10, 1_000, 0.08, 10, static () => new DiagonalPreconditioner(), clock, deadline, cancel);

    public Iterator<double> Iterator() => Composed(Clock.GetCurrentInstant());

    // Insertion order IS precedence and `DetermineStatus` short-circuits on the first non-`Continue`: `Failure`
    // keeps `NaN` terminal, the budget criterion bounds wall clock and cancellation before a divergence window or
    // a residual read fires, and `Residual` precedes the count cap so the final iteration never reads as
    // non-convergence. The deadline anchors on the ONE instant captured at construction, so `CompositeSolver`'s
    // per-rung `Iterator.Reset()` — which clears only the criterion status — cannot re-arm it, where the per-rung
    // `IterationCountStopCriterion` does re-arm and a four-rung ladder would otherwise burn `4 × MaxIterations`.
    Iterator<double> Composed(Instant opened) =>
        new(
            new FailureStopCriterion<double>(),
            new DelegateStopCriterion<double>((_, _, _, _) => Budget(opened)),
            new DivergenceStopCriterion<double>(DivergenceIncrease, DivergenceFloor),
            new ResidualStopCriterion<double>(Tolerance),
            new IterationCountStopCriterion<double>(MaxIterations));

    // Deadline breach is budget exhaustion, so it lands `StoppedWithoutConvergence` and the partial iterate
    // survives the caller's relaxed-criterion retry; cancellation is never transient and lands its own verdict.
    IterationStatus Budget(Instant opened) =>
        Cancel.IsCancellationRequested ? IterationStatus.Cancelled
        : Clock.GetCurrentInstant() - opened >= Deadline ? IterationStatus.StoppedWithoutConvergence
        : IterationStatus.Continue;
}

[Union]
public abstract partial record Edit {
    private Edit() { }

    public sealed record Pin(int Node) : Edit;
    public sealed record Prune(double Tolerance) : Edit;
    public sealed record Bump(int Sign, double[] Column) : Edit;
    public sealed record Revalue(double[] Values) : Edit;

    // Entry-REMOVING edits owe a fresh Dulmage-Mendelsohn sweep; entry-adding and value-only edits cannot lower a
    // structural rank the pre-edit gate already proved, so they skip a full pattern decomposition per edit.
    public bool Regates => Switch(
        pin: static _ => true,
        prune: static _ => true,
        bump: static _ => false,
        revalue: static _ => false);
}

public sealed record FactoredOp(ISparseFactorization<double> Inner, FactorKind Kind, CompressedColumnStorage<double> A, int[] Permutation, ColumnOrdering Ordering, int Fill, double FrobeniusNorm) {
    public Option<Action<double[], double[]>> TransposeSolve => Kind.TransposeRecover(Inner);

    public bool Rectangular => A.RowCount != A.ColumnCount;

    // CSparse returns `A.ColumnCount` unknowns for square solve or rectangular QR least squares and owns its
    // augmented work row; the witness routes the one held-operator GEMV owner against the ORIGINAL operator.
    public Fin<double[]> Solve(double[] rhs, double cap) {
        if (rhs.Length != A.RowCount || !double.IsFinite(cap) || cap < 0.0) {
            return Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<sparse-solve-shape:rhs={rhs.Length}:rows={A.RowCount}:cap={cap:e3}>"));
        }
        double[] x = new double[A.ColumnCount];
        Fin<double[]> solved = Try.lift(() => {
                Inner.Solve(rhs, x);
                return x;
            }).Run()
            .MapFail(error => (Error)new ComputeFault.ModelRejected($"<sparse-solve-break:{Kind.Key}:{error.Message}>"));
        double[] ax = new double[A.RowCount];
        return solved.Bind(field => SparseTensorOps.Spmv(A, GemvForm.Apply, field, ax).Bind(_ =>
            TensorPrimitives.Distance<double>(ax, rhs) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs)) is var residual
            && double.IsFinite(residual) && residual <= cap
                ? Fin.Succ(field)
                : Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<sparse-witness-fail:kind={Kind.Key}:rect={Rectangular}:fill={Fill}:r={residual:e3}>")));
    }
}

public static class SparseOps {
    // Sparse admission gates positive extents, congruent arrays, pointer anchors/monotonicity, index bounds,
    // and finite values before a storage factory sees provider data.
    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) {
        int majorDim = format.MajorIsRow ? rows : columns;
        int minorDim = format.MajorIsRow ? columns : rows;
        Option<string> refusal =
            rows <= 0 || columns <= 0 ? Some($"<extent:{rows}x{columns}>")
            : minorIndices.Length != values.Length ? Some($"<minor-values:{minorIndices.Length}!={values.Length}>")
            : format.PointerForm && majorIndices.Length != majorDim + 1 ? Some($"<pointer-length:{majorIndices.Length}!={majorDim + 1}>")
            : format.PointerForm && (majorIndices[0] != 0 || majorIndices[^1] != values.Length) ? Some($"<pointer-anchor:{majorIndices[0]}..{majorIndices[^1]}:nnz={values.Length}>")
            : format.PointerForm && !Monotone(majorIndices) ? Some("<pointer-nonmonotone>")
            : !format.PointerForm && majorIndices.Length != values.Length ? Some($"<major-values:{majorIndices.Length}!={values.Length}>")
            : !format.PointerForm && !Bounded(majorIndices, majorDim) ? Some($"<major-bound:{majorDim}>")
            : !Bounded(minorIndices, minorDim) ? Some($"<minor-bound:{minorDim}>")
            : !TensorPrimitives.IsFiniteAll<double>(values) ? Some("<values-nonfinite>")
            : None;
        return refusal.Match(
            Some: cause => Fin.Fail<SparseCompressedRowMatrixStorage<double>>(new ComputeFault.PayloadOverBounds($"sparse-ingest:{format.Key}:{cause}")),
            None: () => Fin.Succ(format.Ingest(rows, columns, majorIndices, minorIndices, values)));
    }

    static bool Monotone(int[] pointers) {
        for (int i = 1; i < pointers.Length; i++) { if (pointers[i] < pointers[i - 1]) { return false; } }
        return true;
    }

    static bool Bounded(int[] indices, int extent) {
        for (int i = 0; i < indices.Length; i++) { if ((uint)indices[i] >= (uint)extent) { return false; } }
        return true;
    }

    // Cross-lane CSC projections compose the `Solver/contract#SOLVE_CONTRACT` condensed modal pencil: the CSR-to-CSC
    // conversion the Factor path already owns, exposed once so the condensed and coupling blocks reach the held GEMV
    // and the column reads without a second conversion spelling, and a diagonal CSC off a lumped-inertia vector, so a
    // condensed mass block rides the SAME held GEMV owner as its stiffness sweep rather than an elementwise
    // transcription beside it.
    public static CSparse.Double.SparseMatrix ToCsc(SparseCompressedRowMatrixStorage<double> csr) =>
        (CSparse.Double.SparseMatrix)ToColumnStorage(csr);

    // Diagonal CSC carries its own factory, so the triplet detour never runs for a shape CSparse mints directly.
    public static CSparse.Double.SparseMatrix Diagonal(double[] diagonal) =>
        (CSparse.Double.SparseMatrix)CompressedColumnStorage<double>.OfDiagonalArray(diagonal);

    public static CSparse.Double.SparseMatrix Diagonal(int order, double value) =>
        (CSparse.Double.SparseMatrix)CompressedColumnStorage<double>.CreateDiagonal(order, value);

    // `.mtx` is the one portable exchange for a sparse operator — ingest of an external test operator and the
    // reproduction artifact a failed factorization emits — and both directions are operations of THIS owner, so
    // no direction-named sibling exists. Every byte crosses the pooled `Tensor/memory#STREAM_POOL` stream, so a
    // large operator never stages a second contiguous copy.
    //
    // `ReadStorage` yields the COO the CSC factory finalizes once; `autoExpand: true` is fixed law rather than a
    // caller knob, because a symmetric file read unexpanded lands ONE stored triangle whose `Structural`
    // Dulmage-Mendelsohn read then reports a false rank deficiency on a structurally full operator. A
    // MatrixMarket ARRAY (dense) file throws `NotSupportedException` — only the coordinate form is read — so the
    // trap converts it to the typed refusal rather than letting it escape the rail.
    public static Fin<CompressedColumnStorage<double>> ReadExchange(StreamPool pool, CorrelationId correlation, long byteLength, Stream source) =>
        pool.Get(correlation, new StreamGrant.Sized(byteLength))
            .Bind(staged => Try.lift(() => {
                    using (staged) {
                        source.CopyTo(staged);
                        staged.Position = 0;
                        return CompressedColumnStorage<double>.OfIndexed(
                            MatrixMarketReader.ReadStorage<double>(new StreamReader(staged, Encoding.ASCII), autoExpand: true),
                            inplace: false);
                    }
                }).Run()
                .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<mtx-read:{error.Message}>")));

    // Symmetry is the factor row's own `Symmetric` column, never a caller flag, and the write binds the
    // `StreamWriter` overload alone: the `string` and `Stream` siblings DROP the `symmetric` argument on their
    // way to it, so either would silently emit a `general` header for a symmetric operator and store both
    // triangles. The stream returns positioned at zero and the caller owns its disposal, exactly as the pooled
    // protobuf write does.
    public static Fin<RecyclableMemoryStream> WriteExchange(StreamPool pool, CorrelationId correlation, FactoredOp op) =>
        pool.Get(correlation, new StreamGrant.Open())
            .Bind(staged => Try.lift(() => {
                    try {
                        StreamWriter writer = new(staged, Encoding.ASCII, leaveOpen: true);
                        MatrixMarketWriter.WriteMatrix(writer, op.A, op.Kind.Symmetric);
                        writer.Flush();
                        staged.Position = 0;
                        return staged;
                    }
                    catch { staged.Dispose(); throw; }
                }).Run()
                .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<mtx-write:{error.Message}>")));

    // Helper.ValidateStorage(strict: true) gates the CSC before any factory touches it — the check returns
    // bool and never throws, and factorizing invalid storage produces silently incorrect factors.
    public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor) {
        if (!double.IsFinite(pivotTol) || pivotTol < 0.0 || pivotTol > 1.0 || !double.IsFinite(dropFloor) || dropFloor < 0.0) {
            return Fin.Fail<FactoredOp>(new ComputeFault.ModelRejected($"<sparse-factor-policy:pivot={pivotTol:e3}:drop={dropFloor:e3}>"));
        }
        CompressedColumnStorage<double> csc = ToColumnStorage(csr);
        csc.DropZeros(dropFloor);
        return Helper.ValidateStorage(csc, strict: true)
            ? Structural(csc, kind).Bind(_ => Lift(() => Build(csc, kind, ordering, pivotTol)))
            : Fin.Fail<FactoredOp>(new ComputeFault.ModelRejected($"<sparse-storage-invalid:{kind.Key}:{csc.RowCount}x{csc.ColumnCount}>"));
    }

    // Structural rank is a PATTERN property, readable before any numeric sweep: the Dulmage-Mendelsohn coarse
    // decomposition names the deficient blocks, where a post-hoc residual witness reports only that the answer is
    // wrong. The seed argument is DEFAULTED — it selects the maximum-matching order (0 natural, -1 reverse,
    // anything else random), so passing a dimension there buys a randomized matching whose block boundaries and
    // refusal spans move between two runs of one operator. `Generate` answers null when the matching or either
    // breadth-first sweep fails, and that null is a refusal, never a dereference.
    static Fin<Unit> Structural(CompressedColumnStorage<double> csc, FactorKind kind) {
        int order = Math.Min(csc.RowCount, csc.ColumnCount);
        return Optional(DulmageMendelsohn.Generate(csc)).Match(
            None: () => Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<sparse-structural-matching:{kind.Key}:{csc.RowCount}x{csc.ColumnCount}>")),
            Some: dm => dm.StructuralRank == order
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new ComputeFault.ModelRejected(Deficient(dm, kind, csc, order))));
    }

    // Coarse decomposition is the 3-by-3 block structure of A(p,q) as two `int[5]` boundary arrays. The refusal
    // carries BOTH deficient blocks — under-determined rows `[rr0, rr1)` against columns `[cc1, cc2)`, and
    // over-determined rows `[rr2, rr3)` against columns `[cc3, cc4)` — beside the unmatched rows and columns, so
    // an assembly defect localizes to its own span instead of reporting one rank scalar.
    static string Deficient(DulmageMendelsohn dm, FactorKind kind, CompressedColumnStorage<double> csc, int order) {
        int[] rr = dm.CoarseRowDecomposition, cc = dm.CoarseColumnDecomposition;
        return $"<sparse-structural-rank:{kind.Key}:rank={dm.StructuralRank}:deficiency={order - dm.StructuralRank}"
            + $":order={csc.RowCount}x{csc.ColumnCount}"
            + $":under={rr[0]}..{rr[1]}x{cc[1]}..{cc[2]}"
            + $":over={rr[2]}..{rr[3]}x{cc[3]}..{cc[4]}"
            + $":unmatched-rows={rr[3]}..{rr[4]}:unmatched-cols={cc[0]}..{cc[1]}>";
    }

    public static Fin<(Vector<double> Field, double Residual, SolveTerminal Terminal)> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy) =>
        csr.RowCount != csr.ColumnCount || rhs.Length != csr.RowCount
            ? Fin.Fail<(Vector<double>, double, SolveTerminal)>(new ComputeFault.ModelRejected($"<iterative-shape:{csr.RowCount}x{csr.ColumnCount}:rhs={rhs.Length}>"))
            : policy.MaxIterations < 1 || !double.IsFinite(policy.Tolerance) || policy.Tolerance <= 0.0 || policy.Deadline <= Duration.Zero
                ? Fin.Fail<(Vector<double>, double, SolveTerminal)>(new ComputeFault.ModelRejected($"<iteration-policy:tol={policy.Tolerance:e3}:max={policy.MaxIterations}:deadline={policy.Deadline}>"))
                : Try.lift(() => {
            SparseMatrix matrix = new(csr);
            Vector<double> b = Vector<double>.Build.DenseOfArray(rhs);
            Vector<double> x = Vector<double>.Build.Dense(rhs.Length);
            // `CompositeSolver` resolves each rung's preconditioner in its own constructor, so the
            // argument this seam passes is provably dead there; the pre-initialize that surfaces an ILU
            // factorization throw on the caller's rail therefore runs for the single-solver rows alone, and a
            // rung breakdown inside the ladder is swallowed by its own fall-through with the witness as the gate.
            IPreconditioner<double> pre = method.Ladder ? policy.Preconditioner() : new UnitPreconditioner<double>();
            if (method.Ladder) { pre.Initialize(matrix); }
            IterationStatus verdict = matrix.TrySolveIterative(b, x, method.Solver(policy), policy.Iterator(), pre);
            double residual = (matrix.Multiply(x) - b).L2Norm() / Math.Max(1.0, b.L2Norm());
            return (Verdict: verdict, Field: x, Residual: residual);
        }).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message))
            .Bind(run => Partition(run.Verdict, run.Field, run.Residual, policy.MaxIterations));

    // Three-way terminal partition: budget exhaustion keeps the partial iterate so a relaxed-criterion or
    // different-preconditioner retry survives, while divergence, breakdown, cancellation, and a `Continue`
    // terminal (a criterion-stack defect) each fail the rail — folding them into `Exhausted` publishes a
    // diverged iterate as a retryable partial.
    static Fin<(Vector<double> Field, double Residual, SolveTerminal Terminal)> Partition(IterationStatus verdict, Vector<double> x, double residual, int budget) =>
        verdict switch {
            IterationStatus.Converged => Fin.Succ((x, residual, (SolveTerminal)new SolveTerminal.Admitted(x, residual))),
            IterationStatus.StoppedWithoutConvergence => Fin.Succ((x, residual, (SolveTerminal)new SolveTerminal.Exhausted(x, budget, residual))),
            var hard => Fin.Fail<(Vector<double>, double, SolveTerminal)>(new ComputeFault.ModelRejected($"<iterative-terminal:{hard}:r={residual:e3}>")),
        };

    // Closed `Edit.Switch` forces every dialect; Cholesky rank-one edits use `Update`/`Downdate`, while pattern
    // edits refactor fully and `Revalue` reuses the cached permutation with the same kind. The re-gate rides the
    // row's own `Regates` column into `Refactor`, so a new dialect declares its structural obligation where it is
    // declared rather than at this call.
    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit, double pivotTol) =>
        Admit(op, edit).Bind(admitted => admitted.Switch(
            pin: pin => Refactor(Pinned(op.A, pin.Node), op, pivotTol, admitted.Regates),
            prune: prune => Refactor(Cleaned(op.A, prune.Tolerance), op, pivotTol, admitted.Regates),
            bump: bump => op.Kind.Rank1Edit
                ? Downdate(op, bump, pivotTol)
                : op.Rectangular
                    ? Fin.Fail<FactoredOp>(ComputeFault.Create($"<bump-rectangular:{op.Kind.Key}>"))
                    : Refactor(Bumped(op.A, bump), op, pivotTol, admitted.Regates),
            revalue: revalue => Revalue(op, revalue.Values, pivotTol)));

    static Fin<Edit> Admit(FactoredOp op, Edit edit) =>
        edit.Switch(
            pin: pin => !op.Rectangular && (uint)pin.Node < (uint)op.A.RowCount
                ? Fin.Succ<Edit>(pin)
                : Fin.Fail<Edit>(ComputeFault.Create($"<pin-bound:{pin.Node}:shape={op.A.RowCount}x{op.A.ColumnCount}>")),
            prune: prune => double.IsFinite(prune.Tolerance) && prune.Tolerance >= 0.0
                ? Fin.Succ<Edit>(prune)
                : Fin.Fail<Edit>(ComputeFault.Create($"<prune-tolerance:{prune.Tolerance:e3}>")),
            bump: bump => !op.Rectangular && bump.Sign is -1 or 1 && bump.Column.Length == op.A.RowCount && TensorPrimitives.IsFiniteAll<double>(bump.Column)
                ? Fin.Succ<Edit>(bump)
                : Fin.Fail<Edit>(ComputeFault.Create($"<bump-shape:sign={bump.Sign}:column={bump.Column.Length}:shape={op.A.RowCount}x{op.A.ColumnCount}>")),
            revalue: revalue => revalue.Values.Length == op.A.NonZerosCount && TensorPrimitives.IsFiniteAll<double>(revalue.Values)
                ? Fin.Succ<Edit>(revalue)
                : Fin.Fail<Edit>(ComputeFault.Create($"<revalue-count:{revalue.Values.Length}!={op.A.NonZerosCount}>")));

    static Fin<FactoredOp> Downdate(FactoredOp op, Edit.Bump bump, double pivotTol) {
        CompressedColumnStorage<double> changed = Bumped(op.A, bump);
        return op.Inner is SparseCholesky chol && RankOne(chol, RankOneColumn(bump.Column), bump.Sign)
            ? Fin.Succ(op with {
                A = changed,
                Fill = op.Kind.Fill(changed.NonZerosCount, changed.RowCount, changed.ColumnCount),
                FrobeniusNorm = changed.FrobeniusNorm(),
            })
            : Refactor(changed, op, pivotTol, regate: false);
    }

    // Value-only refactor reuses the cached AMD permutation with the same kind; clone-before-overwrite leaves
    // prior `FactoredOp` storage valid.
    static Fin<FactoredOp> Revalue(FactoredOp op, double[] values, double pivotTol) =>
        Lift(() => {
            CompressedColumnStorage<double> fresh = op.A.Clone();
            values.CopyTo(fresh.Values, 0);
            return op with {
                Inner = op.Kind.Create(fresh, op.Permutation, op.Ordering, pivotTol),
                A = fresh,
                Fill = op.Kind.Fill(fresh.NonZerosCount, fresh.RowCount, fresh.ColumnCount),
                FrobeniusNorm = fresh.FrobeniusNorm(),
            };
        });

    static Fin<FactoredOp> Refactor(CompressedColumnStorage<double> csc, FactoredOp op, double pivotTol, bool regate) =>
        regate
            ? Structural(csc, op.Kind).Bind(_ => Lift(() => Build(csc, op.Kind, op.Ordering, pivotTol)))
            : Lift(() => Build(csc, op.Kind, op.Ordering, pivotTol));

    static Fin<FactoredOp> Lift(Func<FactoredOp> build) =>
        Try.lift(build).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<sparse-factor-break:{error.Message}>"));

    static FactoredOp Build(CompressedColumnStorage<double> csc, FactorKind kind, ColumnOrdering ordering, double pivotTol) {
        int[] permutation = AMD.Generate(csc, ordering);
        ISparseFactorization<double> inner = kind.Create(csc, permutation, ordering, pivotTol);
        return new FactoredOp(inner, kind, csc, permutation, ordering, kind.Fill(csc.NonZerosCount, csc.RowCount, csc.ColumnCount), csc.FrobeniusNorm());
    }

    static bool RankOne(SparseCholesky chol, CompressedColumnStorage<double> w, int sign) =>
        sign >= 0 ? chol.Update(w) : chol.Downdate(w);

    static CompressedColumnStorage<double> RankOneColumn(double[] column) {
        CoordinateStorage<double> coords = new(column.Length, 1, column.Length);
        toSeq(Enumerable.Range(0, column.Length)).Iter(row => coords.At(row, 0, column[row]));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    // `Prune` drops residue on a clone, `Pin` seats a Dirichlet unit row/column, and `Bump` accumulates
    // `sign·w·wᵀ`; every pattern edit returns a fresh CSC.
    static CompressedColumnStorage<double> Cleaned(CompressedColumnStorage<double> a, double tolerance) {
        CompressedColumnStorage<double> fresh = a.Clone();
        fresh.DropZeros(tolerance);
        return fresh;
    }

    static CompressedColumnStorage<double> Pinned(CompressedColumnStorage<double> a, int node) {
        CoordinateStorage<double> coords = new(a.RowCount, a.ColumnCount, a.NonZerosCount + 1);
        toSeq(a.EnumerateIndexedAsValueTuples()).Filter(t => t.row != node && t.column != node).Iter(t => coords.At(t.row, t.column, t.value));
        coords.At(node, node, 1.0);
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    static CompressedColumnStorage<double> Bumped(CompressedColumnStorage<double> a, Edit.Bump bump) {
        int[] support = toSeq(Enumerable.Range(0, bump.Column.Length)).Filter(i => bump.Column[i] != 0.0).ToArray();
        CoordinateStorage<double> coords = new(a.RowCount, a.ColumnCount, a.NonZerosCount + support.Length * support.Length);
        toSeq(a.EnumerateIndexedAsValueTuples()).Iter(t => coords.At(t.row, t.column, t.value));
        toSeq(support).Iter(i => toSeq(support).Iter(j => coords.At(i, j, bump.Sign * bump.Column[i] * bump.Column[j])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorKind kind, SparseCompressedRowMatrixStorage<double> csr, SparseFormat format, int fill, double residual, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, csr.RowCount, csr.ColumnCount, csr.ValueCount, format.Key) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            DeterminismTag = provider.DeterminismTag, SymbolicFill = fill, TrueResidual = residual,
        };

    static CompressedColumnStorage<double> ToColumnStorage(SparseCompressedRowMatrixStorage<double> csr) {
        CoordinateStorage<double> coords = new(csr.RowCount, csr.ColumnCount, csr.ValueCount);
        toSeq(Enumerable.Range(0, csr.RowCount)).Iter(row =>
            toSeq(Enumerable.Range(csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row]))
                .Iter(slot => coords.At(row, csr.ColumnIndices[slot], csr.Values[slot])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }
}
```


## [03]-[SPARSE_ALGEBRA]

- Owner: `SparseTensorOpFamily` `[SmartEnum<string>]` the sparse op axis carrying binary arity and the optional MathNet kernel delegate on the row itself; `GemvForm` `[Union]` the held-GEMV direction-and-accumulation payload; `SparseTensorOps` the static algebra fold over the one `SparseCompressedRowMatrixStorage<double>` CSR storage, routing each op to the row-owned `SparseMatrix` operation where MathNet owns the concern, the held mat-vec to the CSparse CSC span GEMV in both directions, and arbitrary contraction to CSparse pattern construction; `EinsumPlan` the index-subscript contraction planner deriving a greedy pairwise contraction order and lowering each step to dense GEMM or sparse contract; `SparseRun` the per-op nnz-and-format witness.
- Cases: `SparseTensorOpFamily` rows spmv · spmm · sp-add · sp-scale · sp-transpose · kronecker · contract · einsum (8); `GemvForm` cases `Forward` · `Adjoint` (2, each carrying its own α/β so the four CSparse GEMV overloads are one family); the dense lane `TensorOpFamily` rows stay the dense owner and a sparse row is never aliased onto a dense key.
- Entry: `Apply` owns sparse arithmetic, `Spmv(CompressedColumnStorage<double>, GemvForm, ReadOnlySpan<double>, Span<double>)` owns the held mat-vec in both directions into a caller-owned destination, and `Contract` folds one unified dense-or-sparse operand store. `Contract(EinsumPlan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>>, ShardDispatch, DenseSubstrate)` returns `IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>>`, so local and distributed contraction share one entry while cache, blob, and RPC work stays deferred.
- Auto: each MathNet-owned `SparseTensorOpFamily` row carries its kernel delegate directly — spmm binds `Multiply`, sp-add `Add`, sp-scale scalar `Multiply`, sp-transpose `Transpose`, and kronecker `KroneckerProduct` — so row admission and dispatch cannot drift across a parallel dictionary; spmv is the held-operator vector GEMV whose direction and α/β ride the `GemvForm` payload — never a `transpose` flag beside the operand — and contract binds the CSC pattern build MathNet does not own; `EinsumPlan.Of` parses the subscript spec, derives the greedy pairwise order, and folds the merged shape into the surviving slot; an all-dense operand vector under a NATIVE `DenseSubstrate` argument collapses the plan to one `AtenDense.Einsum` call, otherwise each working pair routes from its current sparsity and writes its intermediate back over the surviving index.
- Receipt: every sparse op materializes the `Factorization`/`TensorRun` `ComputeReceipt` evidence carrying the result nnz, the source format key, the op key, and the `AllocationClass` — a sparse op that grows nnz stamps `AllocationClass.PooledMemory` because the MathNet sparse operators allocate fresh storage per op against the dense lane's in-place `SpanOwner` discipline, so the sparse fold fixes an nnz-growth allocation policy explicitly rather than pretending an in-place fold.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new sparse op is one `SparseTensorOpFamily` row carrying its kernel; a new GEMV modality is one `GemvForm` case with its arm on the flattened weight pattern, never a direction-named sibling entrypoint; a new contraction-order heuristic is one column on `EinsumPlan`; zero new surface.
- Boundary: `SparseTensorOps` is the sparse parallel of the dense `Tensor/dispatch#KERNEL_DISPATCH` `TensorOps`/`TensorOpFamily` split and operates on the one `SparseCompressedRowMatrixStorage<double>` the `[02]-[SPARSE_SOLVE]` ingestion fold owns — a parallel sparse-tensor type is the deleted form; the fold routes each op to the library that owns it — the inherited `Matrix<double>` `Multiply(Matrix)`/`Add(Matrix)`/`Transpose()`/scalar `Multiply(double)`/`Multiply(Vector, Vector)` for SpMM/add/scale/transpose/SpMV (MathNet sparse owns these), `Matrix<double>.KroneckerProduct(Matrix<double>)` for the tensor-product element space (the sparse operands build through the `new SparseMatrix(storage)` ctor and the Kronecker rides the inherited `Matrix<T>` member, never a `SparseMatrix.OfStorage` phantom), and CSparse `CoordinateStorage` pattern construction for an arbitrary index `contract` MathNet does not own — a hand-rolled triple loop beside `Matrix<double>.Multiply` or a managed Kronecker beside `KroneckerProduct` is the named reimplementation defect; the `EinsumPlan` contraction order is the named statement seam (the greedy heuristic walks a mutable cost array) bounded by the binary-pairwise reduction so each lowered step is exactly one settled `MatMul`/`Contract` row, never an n-ary kernel that bypasses the `KernelLowering` table, and the contraction-order optimization is exponential in operand count so the planner uses a greedy/DP heuristic bounded by intermediate-size cost rather than an exhaustive search, and the multi-operand contraction threads each intermediate back into one unified working operand store keyed by the surviving `Left` index (dense `Left` | sparse-CSR `Right`, the route decided at execution from the working operands' real sparsity) so a 3+-operand einsum chains correctly — an n-ary fold that indexes two disjoint dense/sparse operand arrays by one shared tree index (crashing a mixed contraction, dropping every dense intermediate to the empty-result fault) is the deleted incoherent form; the sparse SpMV `Solver/contract#SOLVE_CONTRACT` Newton residual and every adjoint leg consume `SparseTensorOps.Spmv` over one held `CompressedColumnStorage<double>` (finalized once through `SparseOps.ToCsc`, or read straight off `FactoredOp.A`) rather than re-wrapping the storage per call (the named per-call-rematerialize defect the contract page already carries) — the adjoint arm binds CSparse `TransposeMultiply`, so a gradient of a residual, a normal-equation assembly, and a PCE design-matrix transpose never allocate a second CSC through `A.Transpose()`, and the accumulating `αA'x + βy` arm carries the iterative adjoint sweep with no temporary; the caller-owned destination and the span overloads keep the whole GEMV path allocation-free where a `Vector<double>` round trip staged three arrays per iterate; `FactoredOp.Solve` routes its own residual witness through this owner, so every CSparse-held operand on the page carries one GEMV spelling and the MathNet-held iterative leg carries the other — one per library, because converting an iterate's operand to CSC per solve pays a full conversion for a norm; the sparse `contract` feeds the einsum planner, the `Tensor/dispatch#EQUIVALENCE_INTEROP` colored Jacobian assembles as sparse contractions over the `contract` row, and the `Spmv`/`Spmm` rows stay CPU-lowered — the `Tensor/dispatch#DEVICE_KERNELS` registry carries no sparse shader row and the device path is never a phantom mapping; the nnz-growth allocation policy is fixed and stamped on the receipt because the MathNet sparse operators return fresh storage per op — a sparse fold that claims the dense lane's in-place `SpanOwner` discipline is dishonest, so the sparse `AllocationClass` is `PooledMemory` and the receipt records the result nnz.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseTensorOpFamily {
    public static readonly SparseTensorOpFamily Spmv = new("spmv", binary: false, kernel: None);
    public static readonly SparseTensorOpFamily Spmm = new("spmm", binary: true, kernel: Some<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>>(static (a, b, _) => b.ToFin(ComputeFault.Create("<spmm-missing-rhs>")).Map(rhs => (SparseMatrix)a.Multiply(rhs))));
    public static readonly SparseTensorOpFamily SpAdd = new("sp-add", binary: true, kernel: Some<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>>(static (a, b, _) => b.ToFin(ComputeFault.Create("<sp-add-missing-rhs>")).Map(rhs => (SparseMatrix)a.Add(rhs))));
    public static readonly SparseTensorOpFamily SpScale = new("sp-scale", binary: false, kernel: Some<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>>(static (a, _, scalar) => Fin.Succ((SparseMatrix)a.Multiply(scalar))));
    public static readonly SparseTensorOpFamily SpTranspose = new("sp-transpose", binary: false, kernel: Some<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>>(static (a, _, _) => Fin.Succ((SparseMatrix)a.Transpose())));
    public static readonly SparseTensorOpFamily Kronecker = new("kronecker", binary: true, kernel: Some<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>>(static (a, b, _) => b.ToFin(ComputeFault.Create("<kronecker-missing-rhs>")).Map(rhs => (SparseMatrix)a.KroneckerProduct(rhs))));
    public static readonly SparseTensorOpFamily Contract = new("contract", binary: true, kernel: None);
    public static readonly SparseTensorOpFamily Einsum = new("einsum", binary: true, kernel: None);

    private readonly Option<Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>> kernel;

    public bool Binary { get; }
    public bool MatNetOwned => kernel.IsSome;

    public Fin<SparseMatrix> Apply(SparseMatrix left, Option<SparseMatrix> right, double scalar) =>
        kernel.ToFin(ComputeFault.Create($"<sparse-op-miss:{Key}>")).Bind(run => run(left, right, scalar));
}

public readonly record struct SparseRun(string Op, int Nnz, int Rows, int Columns, string Route);

// Direction and accumulation are ONE per-occurrence payload: `y = αA'x + βy` at `(1, 0)` IS `y = A'x`, so the two
// CSparse overload pairs are one closed case family and never a `transpose` flag riding beside the operand.
[Union]
public abstract partial record GemvForm {
    private GemvForm() { }

    public sealed record Forward(double Alpha, double Beta) : GemvForm;
    public sealed record Adjoint(double Alpha, double Beta) : GemvForm;

    public static GemvForm Apply => new Forward(1.0, 0.0);
    public static GemvForm Transposed => new Adjoint(1.0, 0.0);
    public static GemvForm Accumulate(double alpha, double beta) => new Forward(alpha, beta);
    public static GemvForm AccumulateTransposed(double alpha, double beta) => new Adjoint(alpha, beta);

    public (bool Transposed, double Alpha, double Beta) Weights => Switch(
        forward: static f => (false, f.Alpha, f.Beta),
        adjoint: static a => (true, a.Alpha, a.Beta));

    // Route key the calling lane stamps into `SparseRun.Route`, so an adjoint sweep is attributable in the
    // receipt without a second op-family row keyed on direction.
    public string Key => Weights switch {
        (false, 1.0, 0.0) => "gemv",
        (false, _, _) => "gemv-axpby",
        (true, 1.0, 0.0) => "gemv-t",
        (true, _, _) => "gemv-t-axpby",
    };
}

public sealed record EinsumPlan(Seq<string> OperandSubscripts, string OutputSubscript, Seq<(int Left, int Right, string Subscripts)> Tree) {
    public bool MatrixChain =>
        OperandSubscripts.Count >= 2
        && OperandSubscripts.ForAll(static symbols => symbols.Length == 2)
        && OutputSubscript.Length == 2
        && OutputSubscript[0] == OperandSubscripts[0][0]
        && OutputSubscript[1] == OperandSubscripts[^1][1]
        && toSeq(Enumerable.Range(1, OperandSubscripts.Count - 1)).ForAll(index => OperandSubscripts[index - 1][1] == OperandSubscripts[index][0]);

    public static Fin<EinsumPlan> Of(string spec, Seq<(int Rows, int Columns, bool Sparse)> shapes) {
        string[] sides = spec.Split("->", StringSplitOptions.TrimEntries);
        if (sides.Length != 2) { return Fin.Fail<EinsumPlan>(ComputeFault.Create($"<einsum-spec-miss:{spec}>")); }
        Seq<string> operands = toSeq(sides[0].Split(',', StringSplitOptions.TrimEntries));
        return operands.Count == shapes.Count
            ? Fin.Succ(new EinsumPlan(operands, sides[1], GreedyOrder(operands, shapes)))
            : Fin.Fail<EinsumPlan>(ComputeFault.Create($"<einsum-operand-arity:{operands.Count}!={shapes.Count}>"));
    }

    // Greedy planning contracts the smallest live intermediate, writes its merged shape over `Left`, and retires
    // `Right`; later costs and route selection observe the real intermediate.
    static Seq<(int Left, int Right, string Subscripts)> GreedyOrder(Seq<string> operands, Seq<(int Rows, int Columns, bool Sparse)> shapes) {
        Seq<int> live = toSeq(Enumerable.Range(0, operands.Count));
        (int Rows, int Columns, bool Sparse)[] dims = shapes.ToArray();
        Seq<string> labels = operands;
        Seq<(int, int, string)> steps = Seq<(int, int, string)>();
        while (live.Count > 1) {
            (int li, int ri, long cost) best = (live[0], live[1], long.MaxValue);
            for (int a = 0; a < live.Count; a++)
                for (int b = a + 1; b < live.Count; b++) {
                    long intermediate = (long)dims[live[a]].Rows * dims[live[b]].Columns;
                    if (intermediate < best.cost) { best = (live[a], live[b], intermediate); }
                }
            steps = steps.Add((best.li, best.ri, $"{labels[best.li]},{labels[best.ri]}"));
            dims[best.li] = (dims[best.li].Rows, dims[best.ri].Columns, dims[best.li].Sparse || dims[best.ri].Sparse);
            live = live.Filter(i => i != best.ri);
        }
        return steps;
    }
}

public static class SparseTensorOps {
    public static Fin<SparseCompressedRowMatrixStorage<double>> Apply(SparseTensorOpFamily op, SparseCompressedRowMatrixStorage<double> left, Option<SparseCompressedRowMatrixStorage<double>> right, double scalar) =>
        op == SparseTensorOpFamily.Contract || op == SparseTensorOpFamily.Einsum
            ? right.ToFin(ComputeFault.Create($"<sparse-{op.Key}-missing-rhs>")).Bind(r => ContractPair(left, r))
            : op.Apply(new SparseMatrix(left), right.Map(static s => new SparseMatrix(s)), scalar)
                .Map(static result => (SparseCompressedRowMatrixStorage<double>)result.Storage);

    // ONE held-operator GEMV owns both directions over the CSparse CSC the factor lane already holds: the adjoint
    // arm binds `TransposeMultiply`, so a residual gradient, a normal-equation assembly, and a PCE design-matrix
    // transpose never materialize `A.Transpose()` — a full second CSC allocation every adjoint leg otherwise
    // pays — and the accumulating `αA'x + βy` arm lands the iterative adjoint sweep with no temporary. Shape
    // congruence derives from the direction, and the destination is caller-owned so a Newton or Krylov sweep
    // reuses one span pair across every iterate with zero allocation on the whole path.
    public static Fin<Unit> Spmv(CompressedColumnStorage<double> a, GemvForm form, ReadOnlySpan<double> x, Span<double> y) =>
        (form.Weights.Transposed ? (Source: a.RowCount, Sink: a.ColumnCount) : (Source: a.ColumnCount, Sink: a.RowCount)) is var shape
        && (x.Length != shape.Source || y.Length != shape.Sink)
            ? Fin.Fail<Unit>(ComputeFault.Create($"<spmv-dim:{form.Key}:x={x.Length}!={shape.Source}:y={y.Length}!={shape.Sink}>"))
            : Applied(a, form, x, y);

    // Named statement seam: a `Span<T>` operand cannot cross the generated `Switch`'s lambda arms, and the
    // CSparse GEMV members return `void`, so the closed family dispatches through one flattened tuple pattern
    // over the direction and the α/β payload the union already projected, landing on the rail at its edge.
    static Fin<Unit> Applied(CompressedColumnStorage<double> a, GemvForm form, ReadOnlySpan<double> x, Span<double> y) {
        (bool transposed, double alpha, double beta) = form.Weights;
        switch ((transposed, alpha, beta)) {
            case (false, 1.0, 0.0): a.Multiply(x, y); break;
            case (false, _, _): a.Multiply(alpha, x, beta, y); break;
            case (true, 1.0, 0.0): a.TransposeMultiply(x, y); break;
            case (true, _, _): a.TransposeMultiply(alpha, x, beta, y); break;
        }
        return Fin.Succ(unit);
    }

    // One working store threads each intermediate through the surviving `Left` key; both-dense steps lower to
    // GEMM, while mixed/sparse steps coerce once and contract CSR before the final `CsrOf` projection.
    public static IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>> Contract(EinsumPlan plan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> operands, ShardDispatch dispatch, DenseSubstrate substrate) =>
        operands.ForAll(static op => op.IsLeft) && substrate.Native
            ? IO.pure((plan.MatrixChain
                ? AtenDense.MultiDot(operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr))))
                : AtenDense.Einsum($"{string.Join(',', plan.OperandSubscripts)}->{plan.OutputSubscript}", operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr)))))
                .Map(dense => ((SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage,
                    Seq(new SparseRun(plan.OutputSubscript, 0, dense.RowCount, dense.ColumnCount, plan.MatrixChain ? "aten-multi-dot" : "aten-einsum")))))
            : plan.Tree.Fold(
                IO.pure(Fin.Succ((Work: toHashMap(operands.Map(static (op, i) => (i, op))), Steps: Seq<SparseRun>()))),
                (effect, step) => effect.Bind(state => state.Match(
                    Succ: held => Step(held.Work, step, plan.OutputSubscript, dispatch).Map(next => next.Map(row => (row.Work, held.Steps.Add(row.Run)))),
                    Fail: static error => IO.pure(Fin.Fail<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, Seq<SparseRun> Steps)>(error)))))
            .Map(state => state.Map(held => (CsrOf(held.Work[plan.Tree.IsEmpty ? 0 : plan.Tree[plan.Tree.Count - 1].Left]), held.Steps)));

    // The lowered step reads the SOLUTION alone: shard receipts are the fan-out's own evidence and the einsum
    // step's is its `SparseRun` row, so neither carrier absorbs the other's facts.
    static IO<Fin<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, SparseRun Run)>> Step(
        HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> work, (int Left, int Right, string Subscripts) step, string output, ShardDispatch dispatch) {
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> l = work[step.Left];
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> r = work[step.Right];
        return l.IsLeft && r.IsLeft
            ? KernelLowering.Lower(TensorOpFamily.MatMul, DenseOf(l), DenseOf(r), dispatch)
                .Map(result => result.Map(outcome => outcome.Solution).Map(dense => (work.AddOrUpdate(step.Left, Left<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(dense)).Remove(step.Right),
                    new SparseRun(output, 0, dense.RowCount, dense.ColumnCount, "dense"))))
            : IO.pure(ContractPair(CsrOf(l), CsrOf(r))
                .Map(csr => (work.AddOrUpdate(step.Left, Right<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(csr)).Remove(step.Right),
                    new SparseRun(output, csr.ValueCount, csr.RowCount, csr.ColumnCount, "sparse"))));
    }

    static Matrix<double> DenseOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => dense, Right: static csr => new SparseMatrix(csr));

    static SparseCompressedRowMatrixStorage<double> CsrOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => (SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage, Right: static csr => csr);

    // Arbitrary index contraction sits outside MathNet: build the result pattern through the CSparse
    // CoordinateStorage the [02]-[SPARSE_SOLVE] handoff already uses, never a managed triple loop.
    static Fin<SparseCompressedRowMatrixStorage<double>> ContractPair(SparseCompressedRowMatrixStorage<double> left, SparseCompressedRowMatrixStorage<double> right) =>
        left.ColumnCount == right.RowCount
            ? Fin.Succ((SparseCompressedRowMatrixStorage<double>)((SparseMatrix)new SparseMatrix(left).Multiply(new SparseMatrix(right))).Storage)
            : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<contract-inner-dim:{left.ColumnCount}!={right.RowCount}>"));
}
```

## [04]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, with the `ShardDispatch` local-or-farm axis the dense GEMM reads (its `Farm` case pairing the replayable `ShardPlan` block decomposition with the `ShardContext` ambient that plan runs against), and the `ProveGemm` GEMM-vs-naive-reference proof the `Tensor/dispatch#EQUIVALENCE_INTEROP` equivalence law's matrix arm reads (the matrix lane has no scalar-tail kernel to span-prove, so the lowering owner carrying the GEMM also OWNS its proof and MatMul/Conv admission).
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalMaxPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor owning both flat index tables; `ShardDispatch` cases `Local` (nullary — the in-process `Matrix<double>.Multiply` leaf, carrying nothing) · `Farm(ShardPlan Plan, ShardContext Context)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardPlan(int Tile, LinearProvider Provider, FactorizationKind Kind, Duration Deadline)` the replayable block decomposition the `Farm` case carries; `ShardContext(ComputeService.ComputeServiceClient Compute, string Node, ModelResultIndex Reuse, Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload, Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload, CorrelationId Correlation, IClock Clock, CancellationToken Cancel)` the composition-supplied run carrier; `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentKey, ComputeReceipt.Factorization Receipt)` the per-block join carrier; `ShardOutcome(Matrix<double> Solution, Seq<ComputeReceipt.Factorization> Receipts)` the solution-with-evidence egress.
- Entry: both `Lower` overloads take ONE `ShardDispatch` and return `IO<Fin<ShardOutcome>>`; `ShardDispatch.Local` lifts the pure GEMM under an empty receipt roster and reads nothing, while `ShardDispatch.Farm` composes cache, blob, and RPC effects off the pair it carries. `Pool` remains pure over its span, `ProveGemm(int order, long seed)` reads the same local kernel, and `Lowers`/`IsMatrix`/`NeedsWindow` report the three entrypoint row sets.
- Auto: matrix rows consult `KernelLowering`; `MatMul` lowers to the active-provider GEMM, `Conv*` projects through `Im2Col`, and pooling folds `TensorPrimitives`. `ShardDispatch.Farm` traverses row blocks on `IO<Fin<ShardBlock>>`, so lookup, payload fetch/store, publish, and RPC dial remain one deferred algebra; `Fin` aborts the join after effects yield typed results.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt, and the `Farm` fan-out returns one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc` and the dialed `ShardNode`, or `Substrate.CpuTensor` and a null node on a content-address cache hit) beside the one `Merged` receipt the join folds over the assembled extents — the shard count is the block count on every shard row, and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Deterministic.Source` the one draw adapter), Rasm.Persistence (project), BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardDispatch` case carrying whatever that topology genuinely needs; a new fan-out knob is one `ShardPlan` column and a new fan-out ambient one `ShardContext` column; a new matrix row inherits `ProveGemm` (the shared GEMM-core proof) with no new proof surface; zero new surface.
- Boundary — the pairing is STRUCTURAL: `ShardDispatch` is the one lowering argument, its `Farm` case holding the `ShardPlan` and the `ShardContext` together so a block decomposition without the transport that runs it is unrepresentable, and its nullary `Local` case carrying nothing so an in-process lowering spells `Local` and names no transport. The plan stays a separate record inside that case because it alone is REPLAYABLE — a value a caller writes down, keys a plan cache on, and compares across runs — where a live gRPC stub, a reuse index, object-store closures, and a cancellation token are none of those; fusing the two into one record made the decomposition uncomparable and unserializable, and threading them as a loose `(plan, context)` pair forced every local call site to name transport it never dials and widened every consumer's delegate columns with an ambient they never read. `ShardContext.Node` is composition's own dial-target spelling, since the wire response carries no node column and the receipt's `ShardNode` must name the farm node that ran the shard.
- Boundary — `Im2Col` computes both index tables ONCE per lowering off the `ConvWindow` — the per-tap axis offsets with dilation and padding folded in, and the per-position strided origins — so the gather cell is pure index math over shared heap tables and allocates nothing per position, where a per-tap unravel-and-project chain allocated two `int[]` per tap on the hottest lowering path. Out-of-range taps pad by branch inside the same walk rather than through a second bounds pass, each `(position, channel)` owns one disjoint patch-row run, and one provider GEMM then carries `MatMul` proof evidence for every convolution row.
- Boundary — both operands ride raw column-major float64 bytes through `UnsafeByteOperations.UnsafeWrap`; request hashing uses pooled serialization, cache lookup resolves only a residence, and blob custody stays on the object-store ports. `IO<Fin<T>>` composes lookup, fetch, dial, store, and publish without an interior effect run; `Fin` gates the private join target after traversal.
- Boundary — the sub-solve content address is the request bytes folded with the provider determinism tag and NOTHING else. Two row blocks carrying identical content dedup to one dialed solve, which is the whole point of the reuse index; salting the digest with the block's row offset made every block its own key, so the index never hit and the write path paid a publish per block for a table nothing read. Provider stays in the key through `SolveDedupKey`, so a cross-provider hit — bit-divergent numbers under one address — remains unrepresentable.
- Boundary — the dial fence is proven at both ends: the generated `ComputeService` stub round-trips over a Unix-domain socket (Kestrel `ListenUnixSocket` against `SocketsHttpHandler.ConnectCallback` over a `UnixDomainSocketEndPoint`, HTTP/2) and in-process (`TestServer.CreateHandler` into `GrpcChannelOptions.HttpHandler`), each preserving the deferred algebra, the raw column-major request bytes, the bounded call options, residence lookup, payload custody, and the per-shard receipts. Channel warm-up is a throwaway unary or health probe and NEVER the connectivity-state API: `ConnectAsync`, `State`, and `WaitForStateChangedAsync` throw `InvalidOperationException` on any channel configured with a `ConnectCallback`, the two being mutually exclusive, so a warm-up written against the state surface fails every custom-transport shard channel this lane dials. SPIKE — dialing inside the integrated-host collectible ALC converges only against a live host; the deterministic floor beside it is the in-process handler, which needs no ALC and proves the same algebra.

```csharp signature
public sealed record ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentKey, ComputeReceipt.Factorization Receipt) {
    public static ShardBlock Join(Matrix<double> target, ShardBlock block) {
        target.SetSubMatrix(block.Start, 0, block.Solution);
        return block;
    }
}

// Solution leaves WITH its evidence: a local lowering runs no factorization and carries an empty roster, so a
// caller emits what the route measured instead of fabricating a row for a GEMM nothing decomposed.
public sealed record ShardOutcome(Matrix<double> Solution, Seq<ComputeReceipt.Factorization> Receipts);

// Ambient the fan-out RUNS against: a live stub, a reuse index, object-store closures, and a token are
// per-composition and unserializable, so they stay their own record rather than fusing into the replayable plan.
// The two travel together inside the ONE `ShardDispatch.Farm` case, so neither reaches a lowering that dials
// nothing. `Node` is composition's dial-target spelling, because the wire response carries no node column and
// the shard receipt owes one.
public sealed record ShardContext(
    ComputeService.ComputeServiceClient Compute,
    string Node,
    ModelResultIndex Reuse,
    Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload,
    Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload,
    CorrelationId Correlation,
    IClock Clock,
    CancellationToken Cancel);

// The replayable half alone: extents, provider, decomposition, and per-call deadline are the value a caller
// writes down and a plan cache keys on. It rides inside `ShardDispatch.Farm` and reaches no local lowering.
public sealed record ShardPlan(int Tile, LinearProvider Provider, FactorizationKind Kind, Duration Deadline);

// The ONE lowering argument. `Local` is nullary because an in-process GEMM decomposes nothing and dials nothing,
// so it carries nothing to read; `Farm` holds the plan WITH the transport that runs it, making a block
// decomposition without its context unrepresentable by type rather than by call-site discipline.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShardDispatch {
    private ShardDispatch() { }

    public sealed record Local : ShardDispatch;
    public sealed record Farm(ShardPlan Plan, ShardContext Context) : ShardDispatch;

    public IO<Fin<ShardOutcome>> Lower(Matrix<double> left, Matrix<double> right) =>
        left.ColumnCount != right.RowCount
            ? IO.pure(Fin.Fail<ShardOutcome>(ComputeFault.Create($"<gemm-inner-dim:{left.ColumnCount}!={right.RowCount}>")))
            : Switch(
                state: (Left: left, Right: right),
                local: static (s, _) => IO.pure(Fin.Succ(new ShardOutcome(Gemm(s.Left, s.Right), Seq<ComputeReceipt.Factorization>()))),
                farm: static (s, farm) => farm.Plan.Tile > 0 && farm.Plan.Deadline > Duration.Zero
                    ? Fanout(s.Left, s.Right, farm.Plan, farm.Context)
                    : IO.pure(Fin.Fail<ShardOutcome>(ComputeFault.Create($"<shard-policy:tile={farm.Plan.Tile}:deadline={farm.Plan.Deadline}>"))));

    internal static Matrix<double> Gemm(Matrix<double> left, Matrix<double> right) => left.Multiply(right);

    // Shard count is the block count on every shard row, and the fold mints ONE further receipt marked `Merged`
    // over the ASSEMBLED extents — the row that folds shard results and executes no shard of its own, so a
    // convergence ratio over the stream never counts a run's own parts as independent solves.
    static IO<Fin<ShardOutcome>> Fanout(Matrix<double> left, Matrix<double> right, ShardPlan plan, ShardContext context) {
        int blocks = (left.RowCount + plan.Tile - 1) / plan.Tile;
        Instant opened = context.Clock.GetCurrentInstant();
        return toSeq(Enumerable.Range(0, blocks))
            .Traverse(block => {
                int start = block * plan.Tile;
                int height = Math.Min(plan.Tile, left.RowCount - start);
                return SubSolve(left.SubMatrix(start, height, 0, left.ColumnCount), right, start, height, blocks, plan, context);
            })
            .Map(traversed => traversed.Traverse(identity).Map(rows => {
                Matrix<double> target = rows.Fold(
                    Matrix<double>.Build.Dense(left.RowCount, right.ColumnCount),
                    static (held, block) => { ShardBlock.Join(held, block); return held; });
                return new ShardOutcome(
                    target,
                    rows.Map(static block => block.Receipt)
                        .Add(Merged(target, rows.Count, plan, context, context.Clock.GetCurrentInstant() - opened)));
            }).As())
            .As();
    }

    // `ShardNode` stays null on the merge because no single node ran the fold, and the extents are the assembled
    // operator's so a reader summing shard rows against this row reads one operator.
    static ComputeReceipt.Factorization Merged(Matrix<double> target, int blocks, ShardPlan plan, ShardContext context, Duration elapsed) =>
        new(plan.Provider.Key, plan.Kind.Key, target.RowCount, target.ColumnCount, 0L, "dense") {
            Scope = new ReceiptScope.Execution(context.Correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            DeterminismTag = plan.Provider.DeterminismTag, Shards = blocks, Merged = true,
        };

    static IO<Fin<ShardBlock>> SubSolve(Matrix<double> rowBlock, Matrix<double> right, int start, int height, int blocks, ShardPlan plan, ShardContext context) {
        // Fresh column-major arrays are cast to byte memory and adopted by `UnsafeWrap`; their roots outlive the
        // request read, and no geometry envelope or second copy intervenes.
        Memory<byte> matrix = rowBlock.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        Memory<byte> rhs = right.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        SolveRequest request = new() {
            Matrix = UnsafeByteOperations.UnsafeWrap(matrix),
            Rhs = UnsafeByteOperations.UnsafeWrap(rhs),
            FactorizationKind = plan.Kind.Key,
            SparseFormat = string.Empty,
            ShardTile = plan.Tile,
        };
        UInt128 address = plan.Provider.SolveDedupKey(Digest(request));
        Instant dialedAt = context.Clock.GetCurrentInstant();
        // `ModelResultIndex` resolves only payload residence; object-store absence misses cleanly to a re-dial,
        // preserving one reuse seam without Compute-side payload custody. A cache hit ran on no node, so its
        // receipt carries the local substrate and no `ShardNode`.
        return context.Reuse.Lookup(address).Bind(row => row.Match(
            Some: cached => context.FetchPayload(cached.Residence).Bind(bytes => bytes.Match(
                Some: payload => IO.pure(Try.lift<Fin<ShardBlock>>(() =>
                        Materialize(SolveResponse.Parser.ParseFrom(payload.Span), address, start, height, right.ColumnCount, blocks, Substrate.CpuTensor, null, Duration.Zero, plan, context))
                    .Run()
                    .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<cached-solve-payload:{error.Message}>"))
                    .Bind(identity)),
                None: () => DialAndStore(plan, context, request, address, start, height, right.ColumnCount, blocks, dialedAt))),
            None: () => DialAndStore(plan, context, request, address, start, height, right.ColumnCount, blocks, dialedAt)));
    }

    // Write-blob-first stores one pooled serialization, then publishes the dedup row over its residence;
    // provider determinism already participates in the lookup key.
    static IO<Fin<ShardBlock>> DialAndStore(ShardPlan plan, ShardContext context, SolveRequest request, UInt128 address, int start, int height, int cols, int blocks, Instant dialedAt) =>
        Dial(plan, context, request).Bind(result => result.Match(
            Succ: response => Materialize(response, address, start, height, cols, blocks, Substrate.RemoteGrpc, context.Node, context.Clock.GetCurrentInstant() - dialedAt, plan, context).Match(
                Succ: block => Store(context, plan, response, address).Map(_ => Fin.Succ(block)),
                Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))),
            Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))));

    static IO<Unit> Store(ShardContext context, ShardPlan plan, SolveResponse response, UInt128 address) =>
        IO.lift(() => {
            MemoryOwner<byte> rent = MemoryOwner<byte>.Allocate(response.CalculateSize());
            response.WriteTo(rent.Span);
            return rent;
        }).Bracket(
            Use: rent => from residence in context.StorePayload(rent.Memory)
                         from _ in context.Reuse.Publish(new ModelResultRow(address, residence, plan.Provider.DeterminismTag, context.Clock.GetCurrentInstant()))
                         select unit,
            Fin: static rent => IO.lift(() => { rent.Dispose(); return unit; }));

    // Address is the block CONTENT alone, folded with the provider determinism tag by `SolveDedupKey`: two row
    // blocks carrying identical bytes dedup to one dialed solve, where salting by row offset made every block its
    // own key and the reuse index published rows nothing could ever hit.
    static UInt128 Digest(SolveRequest request) {
        int width = request.CalculateSize();
        using SpanOwner<byte> rent = SpanOwner<byte>.Allocate(width);
        request.WriteTo(rent.Span);
        return XxHash128.HashToUInt128(rent.Span);
    }

    static Fin<ShardBlock> Materialize(SolveResponse response, UInt128 address, int start, int height, int defaultCols, int blocks, Substrate substrate, string? node, Duration elapsed, ShardPlan plan, ShardContext context) {
        int cols = response.Cols == 0 ? defaultCols : (int)response.Cols;
        if (cols <= 0 || response.Solution.Length != (long)height * cols * sizeof(double)) {
            return Fin.Fail<ShardBlock>(new ComputeFault.PayloadOverBounds($"<solve-shape:height={height}:cols={cols}:bytes={response.Solution.Length}>"));
        }
        ComputeReceipt.Factorization receipt = new(response.Provider, response.Decomposition, height, cols, response.Nnz, "dense") {
            Scope = new ReceiptScope.Execution(context.Correlation, WorkLane.Background, substrate, AllocationClass.PooledMemory, elapsed),
            DeterminismTag = plan.Provider.DeterminismTag, Shards = blocks, ShardNode = node,
        };
        return Try.lift(() => new ShardBlock(start, height, Restore(response, height, cols), address, receipt))
            .Run()
            .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<solve-materialize:{error.Message}>"));
    }

    // Warm-up, where a caller wants one, is a throwaway unary or health call: `ConnectAsync`, `State`, and
    // `WaitForStateChangedAsync` throw `InvalidOperationException` on a `ConnectCallback` channel, and every
    // custom-transport shard channel is exactly that.
    static IO<Fin<SolveResponse>> Dial(ShardPlan plan, ShardContext context, SolveRequest request) =>
        IO.lift(() => Try.lift(() => context.Compute.Solve(request, new CallOptions(new Metadata { { "rasm-correlation", context.Correlation.ToString() } })
                    .WithDeadline(context.Clock.GetCurrentInstant().Plus(plan.Deadline).ToDateTimeUtc())
                    .WithCancellationToken(context.Cancel)))
                .Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<shard-dial:{error.Message}>")));

    static Matrix<double> Restore(SolveResponse response, int rows, int cols) =>
        Matrix<double>.Build.Dense(rows, cols, MemoryMarshal.Cast<byte, double>(response.Solution.Span).ToArray());
}

public sealed record ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial) {
    public int Rank => Kernel.Length;
    public int KernelVolume => Kernel.Aggregate(1, static (acc, extent) => acc * extent);
    public int PatchWidth => Channels * KernelVolume;

    public int[] OutputExtents =>
        toSeq(Enumerable.Range(0, Rank))
            .Map(axis => (Spatial[axis] + 2 * Padding[axis] - Dilation[axis] * (Kernel[axis] - 1) - 1) / Stride[axis] + 1)
            .ToArray();

    public int OutputPositions => OutputExtents.Aggregate(1, static (acc, extent) => acc * extent);

    // Per-tap axis offsets with dilation and padding ALREADY folded (`Dilation·k − Padding`), flattened at
    // `tap·Rank + axis`. Mixed-radix over `Kernel` runs once per lowering, so the gather never re-unravels a tap.
    public int[] TapOffsets() {
        int rank = Rank, volume = KernelVolume;
        int[] taps = new int[volume * rank];
        for (int tap = 0; tap < volume; tap++) {
            int remainder = tap;
            for (int axis = rank - 1; axis >= 0; axis--) {
                taps[tap * rank + axis] = remainder % Kernel[axis] * Dilation[axis] - Padding[axis];
                remainder /= Kernel[axis];
            }
        }

        return taps;
    }

    // Per-position axis origins with stride ALREADY applied, flattened at `position·Rank + axis`; the extents
    // read ONCE because the property re-derives the whole vector per access.
    public int[] StridedOrigins() {
        int rank = Rank;
        int[] extents = OutputExtents;
        int positions = extents.Aggregate(1, static (acc, extent) => acc * extent);
        int[] origins = new int[positions * rank];
        for (int position = 0; position < positions; position++) {
            int remainder = position;
            for (int axis = rank - 1; axis >= 0; axis--) {
                origins[position * rank + axis] = remainder % extents[axis] * Stride[axis];
                remainder /= extents[axis];
            }
        }

        return origins;
    }
}

public static class KernelLowering {
    // Convolution spatial rank is row data, never literal arithmetic over a key; this table's keys are the
    // geometry overload's admitted convolution rows.
    static readonly FrozenDictionary<TensorOpFamily, int> ConvRank = new (TensorOpFamily Row, int Rank)[] {
        (TensorOpFamily.Conv1D, 1), (TensorOpFamily.Conv2D, 2), (TensorOpFamily.Conv3D, 3),
    }.ToFrozenDictionary(static r => r.Row, static r => r.Rank);

    // Matrix rows DERIVE from the conv table beside the one direct GEMM row, so the convolution roster has a
    // single spelling and a new conv rank cannot land in one place and go missing in the other.
    static readonly FrozenSet<TensorOpFamily> MatrixRows =
        ConvRank.Keys.Append(TensorOpFamily.MatMul).ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> PoolRows = new[] {
        TensorOpFamily.MaxPool, TensorOpFamily.AvgPool, TensorOpFamily.GlobalMaxPool, TensorOpFamily.GlobalAvgPool,
    }.ToFrozenSet();

    public static IO<Fin<ShardOutcome>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardDispatch dispatch) =>
        row == TensorOpFamily.MatMul ? dispatch.Lower(left, right)
        : IO.pure(Fin.Fail<ShardOutcome>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>")));

    public static IO<Fin<ShardOutcome>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardDispatch dispatch) =>
        ConvRank.TryGetValue(row, out int rank) && window.Rank == rank
            ? dispatch.Lower(Im2Col(input, window), kernel)
            : IO.pure(Fin.Fail<ShardOutcome>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>")));

    // Both index tables mint ONCE here and the gather then reads them; the geometry properties re-derive their
    // vectors per access, so each is read exactly once into the cell state.
    static Matrix<double> Im2Col(Matrix<double> input, ConvWindow window) {
        int positions = window.OutputPositions;
        int channels = window.Channels;
        double[,] patch = new double[positions, window.PatchWidth];
        PatchGather gather = new(input, window.Rank, window.KernelVolume, window.Spatial, window.StridedOrigins(), window.TapOffsets(), patch);
        ParallelHelper.For2D(0, positions, 0, channels, in gather);
        return Matrix<double>.Build.DenseOfArray(patch);
    }

    // Each `(outputPosition, channel)` owns one disjoint patch-row run; the shared heap tables satisfy `IAction2D`
    // without a ref-struct field, and the cell body is pure index math — origin plus tap offset per axis, raveled
    // in the same walk, padding taken by the range branch rather than a second bounds pass.
    readonly struct PatchGather(Matrix<double> input, int rank, int kernelVolume, int[] spatial, int[] origins, int[] taps, double[,] patch) : IAction2D {
        public void Invoke(int position, int channel) {
            int originBase = position * rank;
            int column = channel * kernelVolume;
            for (int tap = 0; tap < kernelVolume; tap++) {
                int tapBase = tap * rank;
                int flat = 0;
                bool inside = true;
                for (int axis = 0; axis < rank; axis++) {
                    int coordinate = origins[originBase + axis] + taps[tapBase + axis];
                    if ((uint)coordinate >= (uint)spatial[axis]) { inside = false; break; }
                    flat = flat * spatial[axis] + coordinate;
                }

                patch[position, column + tap] = inside ? input[channel, flat] : 0d;
            }
        }
    }

    public static Fin<double> Pool(TensorOpFamily row, ReadOnlySpan<double> window) =>
        row == TensorOpFamily.MaxPool || row == TensorOpFamily.GlobalMaxPool ? Fin.Succ(TensorPrimitives.Max(window))
        : row == TensorOpFamily.AvgPool || row == TensorOpFamily.GlobalAvgPool ? Fin.Succ(TensorPrimitives.Sum(window) / window.Length)
        : Fin.Fail<double>(ComputeFault.Create($"<pool-row-miss:{row.Key}>"));

    // One predicate per entrypoint, so a caller ROUTES before it calls: `Lowers` reports the union any
    // entrypoint serves, `NeedsWindow` the rows demanding the geometry overload, and `IsMatrix` the GEMM-proof
    // set. A `NeedsWindow` row handed to the two-operand `Lower` is the miss its fault names.
    public static bool Lowers(TensorOpFamily row) => MatrixRows.Contains(row) || PoolRows.Contains(row);
    public static bool IsMatrix(TensorOpFamily row) => MatrixRows.Contains(row);
    public static bool NeedsWindow(TensorOpFamily row) => ConvRank.ContainsKey(row);

    // GEMM proof compares the exact local lowering kernel with an independent triple-loop reference. Order and
    // draw seed are SEPARATE arguments: conflating them let the proof size dictate the stream, so re-proving at
    // a second order silently drew a different operand pair and the two runs compared nothing.
    public static ProofEvidence ProveGemm(int order, long seed) {
        int n = Math.Max(2, order);
        Matrix<double> left = Gaussian(n, seed, lane: 0), right = Gaussian(n, seed, lane: 1);
        Matrix<double> gemm = ShardDispatch.Gemm(left, right);
        double deviation = 0.0, mass = 0.0;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                double reference = 0.0, entryMass = 0.0;
                for (int p = 0; p < n; p++) {
                    double term = left[i, p] * right[p, j];
                    reference += term;
                    entryMass += double.Abs(term);
                }
                deviation = Math.Max(deviation, double.Abs(gemm[i, j] - reference));
                mass = Math.Max(mass, entryMass);
            }
        }
        return new ProofEvidence(deviation, n, mass, 1.0);
    }

    // Draws cross through the kernel Deterministic.Source adapter; lane keying replaces the hand-mixed second seed.
    static Matrix<double> Gaussian(int n, long seed, long lane) {
        double[] values = new double[n * n];
        new Normal(0.0, 1.0, Deterministic.Source(seed, lane)).Samples(values);
        return Matrix<double>.Build.Dense(n, n, values);
    }
}
```


## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
