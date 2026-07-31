# [COMPUTE_FACTOR]

Rasm.Compute sparse-solve and kernel-lowering lane: the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SolveTerminal` partition preserving the caller's retry, and the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel with the `ShardPlan` block-decomposition column the dense GEMM reads. Every library refuses its own gates — `Iterator<T>` exposes no iteration count — so the criterion stack re-imposes each and every result leaves as a typed `ComputeReceipt` carrying the route variant, the scale-derived tolerance, and the recomputed true relative residual against the original operator. Distributed solves cross solely through the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc, which the `ShardPlan.Blocked` row-block sub-solve dials by reference.

## [01]-[INDEX]

- [02]-[SPARSE_SOLVE]: CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative with the solver ladder and the budget criterion; overdetermined sparse-QR least-squares route; `.mtx` exchange.
- [03]-[SPARSE_ALGEBRA]: `SparseTensorOpFamily` op axis over CSR storage; the `GemvForm` forward/adjoint held GEMV; SpMM/add/transpose/Kronecker/contract; `EinsumPlan` pairwise lowering over dense GEMM and sparse contract.
- [04]-[KERNEL_LOWERING]: tensor matrix/structural rows lower onto real GEMM/im2col/pool; shard fan-out.

## [02]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows carrying the CSR-conversion `ingest` delegate as row data; `FactorKind` `[SmartEnum<string>]` direct-factor rows carrying the capability columns (rank-1 edit, transpose-solve, inertia, reentrancy, symmetry), the fill-formula and transpose-solve-recovery delegate, AND the permutation-keyed `create` factory as row data; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis whose `Ladder` column also composes the rungs into the `CompositeSolver` fall-through row, with `MethodSetup` the `IIterativeSolverSetup<double>` projection of one row and the `IterationPolicy` record (tolerance · max-iter · criterion stack · preconditioner · clock · deadline · cancellation); `FactoredOp` the typed sparse-operator value owning the factorization instance, the cached AMD permutation, the `ColumnOrdering` it was factored with, symbolic fill, and kind discriminant; `Edit` `[Union]` the structural-edit dialect; `SparseOps` the direct-and-iterative sparse-solve fold over CSR-backed MathNet storage and CSparse CSC direct factorizations, and the `.mtx` exchange pair carrying both directions of the one portable operator correspondence — ingestion routes through the `SparseFormat` row's `ingest` delegate, direct factorization through the `FactorKind` row's `create` delegate, neither through a parallel `FrozenDictionary` keyed by the same enum.
- Cases: `SparseFormat` `ingest`-delegate rows csr · csc · coo · dok (4); `FactorKind` verified `create`-delegate rows spd · ldl · lu · qr (4, every row wired — `Ldl` binds `SparseLDL.Create`); `IterativeMethod` rows bicgstab · gpbicg · tfqmr · mlk-bicgstab · composite (5, the composite deriving its ladder from the four `Ladder` rows); `Edit` cases `Pin` · `Prune` · `Bump` · `Revalue` (4, every case realized and admitted before mutation).
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — the ONE sparse admission seam: extents, minor/values congruence, the pointer-form major run (length axis+1, `[0]==0`, `[^1]==nnz`, monotone) or the index-form major bounds, minor-index bounds, and one vectorized `TensorPrimitives.IsFiniteAll` values pass all gate BEFORE the storage factory, each refusal a typed `PayloadOverBounds` fault — the format row's `PointerForm`/`MajorIsRow` columns drive the one shared body, never a per-format admission path; `public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor)` converts the CSR triplets once to a CSparse `CompressedColumnStorage<double>` through `CoordinateStorage` + the admitted `CompressedColumnStorage<double>.OfIndexed` CSC factory, reads the symbolic fill before the numeric sweep, and collapses the completed factorization to one `FactoredOp` value; `FactoredOp.Solve(double[] rhs, double cap)` is the one polymorphic solve over both shapes — a square operator routes the forward triangular solve and a rectangular operator on the `Qr` kind routes the SparseQR least-squares `min‖Ax−b‖`, both landing in an `A.ColumnCount`-length result (CSparse sizes the caller buffer at `n` for every kind and allocates the augmented `S.m2` work row INTERNALLY — that augmentation is private factor state, never a caller dimension to over-size), the witness recomputing the true relative residual against the ORIGINAL rectangular `A` through the `ILinearOperator<double>` vector GEMV `A` inherits from the CSparse `Matrix<T>` base — `A` (a `CompressedColumnStorage<double>`) calls `A.Multiply(x, ax)` directly because `CSparse.Matrix<T> : ILinearOperator<double>` declares the array/span vector multiply the concrete `SparseMatrix` overrides, `ax` sized `A.RowCount` — rather than the square normal-equations operator — so a sparse PCE fit, a sparse-Jacobian recovery, and an overdetermined FEM normal-equations recovery solve through the one `FactoredOp` capsule without densifying to `Matrix<double>.QR`; `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected `IIterativeSolver<double>` under the explicitly-ordered criterion stack and returns the field with the recomputed true relative residual and `SolveTerminal` verdict — the iteration count is NOT read from `Iterator<double>` (which exposes the terminal `Status` and the `DetermineStatus`/`Cancel`/`Reset` drivers but no iteration count), it is the criterion-stack-bounded cap; `ReadExchange(StreamPool pool, CorrelationId correlation, long byteLength, Stream source)` and `WriteExchange(StreamPool pool, CorrelationId correlation, FactoredOp op)` are the `.mtx` ingest and reproduction ends of one correspondence, both staging through the pooled recyclable stream.
- Auto: every format row maps to one CSR ingestion conversion through the `SparseFormat` row's `ingest` delegate — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type and the build closure rides the row, not a parallel ingestion table; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `FactorKind` row's `create` delegate binding the explicit-permutation `SparseCholesky.Create(csc, p)`/`SparseLDL.Create(csc, p)`/`SparseLU.Create(csc, p, pivotTol)` and the ordering-based `SparseQR.Create(csc, ordering)`, so the AMD ordering is computed once by `Build` and the symmetric/lu kinds reuse that permutation rather than re-deriving it inside `Create`, then solve in place through `ISparseFactorization<double>.Solve(double[], double[])` (the residual witness calls the vector GEMV directly on the `CompressedColumnStorage<double>` operator, inherited from the CSparse `Matrix<T> : ILinearOperator<double>` base — no residency cast); iterative solves run the `IterativeMethod` row's `Solver(policy)` factory under the `IterationPolicy.Iterator()` `Iterator<double>` criterion stack constructed in precedence order `Failure → Budget → Divergence → Residual → IterationCount`, the `composite` row folding the four `Ladder` rows into a `CompositeSolver` that falls through to the next rung on divergence, breakdown, or a swallowed throw instead of returning one method's failure; `FactoredOp.TransposeSolve` recovers the transpose-solve action from the `FactorKind` row's `TransposeRecover` delegate column alone (some for lu and qr, none for spd and ldl) because the shared `ISparseFactorization<double>` exposes only the forward solve and `SolveTranspose` closes over the concrete `SparseLU`/`SparseQR`.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, factor kind, the symbolic fill, the recomputed true relative residual, row and column extents, the `ValueCount` non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, Microsoft.IO.RecyclableMemoryStream, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `SparseFormat` row carrying its `ingest` delegate; a new direct solver is one `FactorKind` row carrying its capability, symmetry, fill, transpose-recovery, AND `create` columns together (one row, never a row beside a parallel `DirectSolvers` table edit); a new iterative method is one `IterativeMethod` row carrying its `Ladder` column and its `IIterativeSolver<double>` factory column, and the composite ladder absorbs it with no fold edit; a new structural-edit dialect is one `Edit` case with its arm on the total `Apply` Switch; a new iteration knob is one column on the `IterationPolicy` record and one criterion on `Composed`, minted through `IterationPolicy.Of` so the lane's clock, budget, and token stay the record's only ambient-free ingress; zero new surface.
- Boundary: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage — csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner for each format is the deleted form; the CSR-to-CSC handoff builds a `CoordinateStorage<double>(rows, cols, nnz)`, calls `.At(i, j, v)` per entry, and converts once through the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` CSC factory (the CSparse static that internally runs `Converter.ToCompressedColumnStorage` with cleanup) — a hand-rolled `Converter.ToCompressedColumnStorage` detour beside the admitted `OfIndexed` factory is the named reimplementation defect, and `inplace: true` is rejected when the triplet must survive a structural-edit increment because it invalidates the source arrays and dangles references; strict storage validation runs before factoring through `CSparse.Helper.ValidateStorage(csc, strict: true)` inside `Factor` because it returns `bool` and never throws and factorizing invalid storage produces silently incorrect factors; the symbolic fill is read before the numeric sweep to route direct versus iterative and the count is per-kind through the `FactorKind.Fill` delegate column (one factor for the symmetric kinds, an `L + U − n` formula for `SparseLU`, a `Q + R − m` formula for `SparseQR`) so a bare fill integer compared across kinds is meaningless; the `ColumnOrdering.MinimumDegreeAtPlusA` permutation `int[]` from `CSparse.Ordering.AMD.Generate(CompressedColumnStorage<double>, ColumnOrdering)` caches as the value-only refactor key over an invariant pattern (`ColumnOrdering` values are `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, `MinimumDegreeAtA`; the AMD ordering type lives in `CSparse.Ordering`, distinct from the `ColumnOrdering` enum, and its `Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order)` takes the matrix first and the ordering second); assembly residue drops with a structural tolerance near `machineEps · ‖A‖_F` through `DropZeros(tolerance)` because the default `0.0` removes only binary zeros; `SparseLU` pivot `tol` is `[0, 1]` as a relative column threshold (`1` full partial pivoting, `0` disabled) never an absolute floor; transpose-solve/rank-1-edit/inertia/reentrancy recover from the `FactorKind` row alone because the shared solver interface exposes only the forward solve; an asymmetric input to a symmetric kind factors as its symmetrization and returns a correct answer to the WRONG system so the post-solve true residual is the only structural signal; a typed-only catch at the factorization boundary is rejected because SPD pivot loss and the zero-diagonal break throw bare `Exception`; the cached square factorization's one constructor-allocated scratch is non-reentrant so solves serialize through the `FactoredOp` capsule and the `SparseQR` reentrant kind is the one parallel-safe row; the rectangular least-squares result buffer sizes from `A.ColumnCount` exactly like the square solve — `SparseQR.Solve` writes the `n`-length left-hand side and allocates the augmented `S.m2` work row INTERNALLY (private factor state with no public accessor), so an attempt to over-size the caller buffer from a nonexistent "solution dimension" member is the named phantom and `A.ColumnCount` is correct for every kind and shape; the `Qr` row is the one rectangular least-squares route on `FactoredOp.Solve` so an overdetermined sparse system (`Solver/contract#SOLVE_CONTRACT` normal-equations recovery, `Solver/uncertainty#UNCERTAINTY_LANE` PCE coefficient fit, `Tensor/dispatch#EQUIVALENCE_INTEROP` sparse-Jacobian recovery) minimizes `‖Ax−b‖` through `SparseQR.Solve` and the witness recomputes against the ORIGINAL rectangular `A` (`ax` sized `A.RowCount`, the m-residual against the b-vector), never a dense `Matrix<double>.QR` fallback and never the square normal-equations operator whose conditioning the rectangular QR avoids; the residual GEMV calls `FactoredOp.A.Multiply(x, ax)` directly on the `CompressedColumnStorage<double>` because CSparse's `Matrix<T>` base implements `ILinearOperator<double>` and declares the vector `Multiply(ReadOnlySpan<double>, Span<double>)`/`Multiply(double[], double[])` the concrete `SparseMatrix` overrides — a residency cast to `CSparse.Double.SparseMatrix` to reach a vector member the base already exposes is the deleted ceremony, and the `double[]` operands bind the inherited array GEMV with no collision against the matrix-matrix overloads `Multiply(CompressedColumnStorage<double>[, result])`; bare `SparseMatrix` is reserved for the MathNet CSR concrete (`SolveIterative`) so the two sparse libraries never alias one name; the `Ldl` symmetric-indefinite/inertia kind binds `SparseLDL.Create` as a real `create` row — a `FactorKind` capability row with no factory delegate (a `<sparse-direct-miss>` fall-through) is the named declared-but-unbound defect; the cache populates success-only so only residual-witnessed factorizations enter and a diverged solve never poisons reuse; every structural `Edit` applies its edit to the operator before re-factoring — `Pin` drops row+column `node` and seats a unit diagonal, `Prune` `DropZeros` over a clone, a rank-1-edit kind's `Bump` runs the `SparseCholesky` `Update`/`Downdate` and discards-and-reconstructs the BUMPED operator (not the unedited one) on a `false` result, a non-rank-1-edit `Bump` accumulates `A + sign·w·wᵀ` over the column support and re-factors, and a `Bump` on a rectangular operator is rejected because a symmetric rank-1 update is ill-defined there — a default arm that silently re-factors the unedited operator and drops the `Pin`/`Prune`/`Bump` payload is the deleted form; a value-only `Revalue` clones the CSC through `Clone()` before overwriting the value array because the old `FactoredOp` still references the original storage and an in-place `CopyTo` corrupts the pre-edit operator, then re-creates with the SAME `op.Kind` from the cached permutation (no AMD over the invariant pattern) — the explicit-permutation `Create` amortizes the dominant symbolic cost (the AMD ordering) and yields a fully INDEPENDENT factor, so the in-place CSparse `Refactorize` (which reuses the elimination tree and column counts too but MUTATES the shared factor instance, aliasing the pre-edit `FactoredOp` whose `Inner` other readers and the non-reentrant single-owner solve still hold) is deliberately not taken: `FactoredOp` value immutability outranks the marginal numeric-phase saving, and `SparseQR` exposes no `Refactorize` at all; a hardcoded `SparseLU.Create` re-create that silently changes a non-LU operator's kind is the deleted correctness defect; the iterative method is the closed `IterativeMethod` SmartEnum and a raw-`string` method discriminant beside it is the named defect; the criterion stack constructs explicitly in precedence order because insertion order is precedence, `Failure` first keeps `NaN` terminal, and `Residual` before the count cap suppresses convergence on the final iteration; the iterate is admitted only on the independently recomputed true relative residual against the original operator because the converged verdict certifies only that the preconditioned residual fell below tolerance and left preconditioning distorts the norm; the structural substitution path is the most dangerous because it certifies an arbitrary iterate under a normal verdict and the ULP guard fails open on `NaN`; preconditioners initialize outside the solve and catch their throw there because the init throw otherwise escapes the verdict-returning entrypoint, and that pre-initialize runs for the `Ladder` rows alone because `CompositeSolver` resolves each rung's preconditioner in its own constructor and passes `setup ?? argument`, making the seam's argument provably dead on the composite row where a second incomplete factorization is pure waste; the ladder swallows every rung throw and falls through, so a breakdown inside it is invisible to the caller and the recomputed true residual is the only gate — the same certify-an-arbitrary-iterate danger the substitution path carries; the budget criterion anchors on ONE instant captured at `Iterator()` construction so the ladder's per-rung `Iterator.Reset()`, which clears only criterion status, cannot re-arm it, where the per-rung `IterationCountStopCriterion` does re-arm and an unbounded ladder burns `rungs × MaxIterations`; a deadline breach is budget exhaustion and lands `StoppedWithoutConvergence` so the partial iterate survives a relaxed-criterion retry while cancellation lands its own verdict and is never transient; the terminal partition is three-way — `Converged` admits, `StoppedWithoutConvergence` exhausts, and divergence, breakdown, cancellation, and a `Continue` terminal each fail the rail — because folding them into `Exhausted` publishes a diverged iterate as a retryable partial; `MethodSetup` binds `double.NaN` to `SolutionSpeed`/`Reliability` because no producer measured them and the ONE member that reads them is `SolverSetup<T>.LoadFromAssembly`, the reflection discovery form this lane rejects, so a zero there ranks a ladder nothing measured; `.mtx` exchange rides `MatrixMarketReader`/`MatrixMarketWriter` through the pooled recyclable stream and never a bare `FileStream`, `ReadStorage` runs `autoExpand: true` as fixed law because an unexpanded symmetric file lands one stored triangle the structural read then calls rank-deficient, a MatrixMarket ARRAY file throws `NotSupportedException` and converts to the typed refusal at the trap, the write binds the `StreamWriter` overload alone because the `string` and `Stream` siblings DROP the `symmetric` argument on their way to it and emit a `general` header for a symmetric operator, and that flag reads off the `FactorKind.Symmetric` column rather than a caller knob; the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

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
    // decomposition names the deficient block, where a post-hoc residual witness reports only that the answer is
    // wrong. The under-determined coarse block is the deficiency, and its row/column spans localize it.
    static Fin<Unit> Structural(CompressedColumnStorage<double> csc, FactorKind kind) {
        DulmageMendelsohn dm = DulmageMendelsohn.Generate(csc, csc.RowCount);
        int structural = Math.Min(csc.RowCount, csc.ColumnCount) - Deficiency(dm);
        return structural == Math.Min(csc.RowCount, csc.ColumnCount)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                $"<sparse-structural-rank:{kind.Key}:rank={structural}:order={csc.RowCount}x{csc.ColumnCount}:rows={dm.CoarseRowDecomposition[1]}..{dm.CoarseRowDecomposition[2]}:cols={dm.CoarseColumnDecomposition[1]}..{dm.CoarseColumnDecomposition[2]}>"));
    }

    static int Deficiency(DulmageMendelsohn dm) =>
        Math.Max(dm.CoarseRowDecomposition[1], dm.CoarseColumnDecomposition[3] - dm.CoarseColumnDecomposition[2]);

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
    // edits refactor fully and `Revalue` reuses the cached permutation with the same kind.
    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit, double pivotTol) =>
        Admit(op, edit).Bind(admitted => admitted.Switch(
            pin: pin => Refactor(Pinned(op.A, pin.Node), op, pivotTol),
            prune: prune => Refactor(Cleaned(op.A, prune.Tolerance), op, pivotTol),
            bump: bump => op.Kind.Rank1Edit
                ? Downdate(op, bump, pivotTol)
                : op.Rectangular
                    ? Fin.Fail<FactoredOp>(ComputeFault.Create($"<bump-rectangular:{op.Kind.Key}>"))
                    : Refactor(Bumped(op.A, bump), op, pivotTol),
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
            : Refactor(changed, op, pivotTol);
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

    static Fin<FactoredOp> Refactor(CompressedColumnStorage<double> csc, FactoredOp op, double pivotTol) =>
        Lift(() => Build(csc, op.Kind, op.Ordering, pivotTol));

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
- Entry: `Apply` owns sparse arithmetic, `Spmv(CompressedColumnStorage<double>, GemvForm, ReadOnlySpan<double>, Span<double>)` owns the held mat-vec in both directions into a caller-owned destination, and `Contract` folds one unified dense-or-sparse operand store. `Contract` returns `IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>>`, so local and distributed contraction share one entry while cache, blob, and RPC work stays deferred.
- Auto: each MathNet-owned `SparseTensorOpFamily` row carries its kernel delegate directly — spmm binds `Multiply`, sp-add `Add`, sp-scale scalar `Multiply`, sp-transpose `Transpose`, and kronecker `KroneckerProduct` — so row admission and dispatch cannot drift across a parallel dictionary; spmv is the held-operator vector GEMV whose direction and α/β ride the `GemvForm` payload — never a `transpose` flag beside the operand — and contract binds the CSC pattern build MathNet does not own; `EinsumPlan.Of` parses the subscript spec, derives the greedy pairwise order, and folds the merged shape into the surviving slot; an all-dense operand vector under `DenseSubstrate.Active.Native` collapses the plan to one `AtenDense.Einsum` call, otherwise each working pair routes from its current sparsity and writes its intermediate back over the surviving index.
- Receipt: every sparse op materializes the `Factorization`/`TensorRun` `ComputeReceipt` evidence carrying the result nnz, the source format key, the op key, and the `AllocationClass` — a sparse op that grows nnz stamps `AllocationClass.PooledMemory` because the MathNet sparse operators allocate fresh storage per op against the dense lane's in-place `SpanOwner` discipline, so the sparse fold fixes an nnz-growth allocation policy explicitly rather than pretending an in-place fold.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new sparse op is one `SparseTensorOpFamily` row carrying its kernel; a new GEMV modality is one `GemvForm` case with its arm on the flattened weight pattern, never a direction-named sibling entrypoint; a new contraction-order heuristic is one column on `EinsumPlan`; zero new surface.
- Boundary: `SparseTensorOps` is the sparse parallel of the dense `Tensor/dispatch#KERNEL_DISPATCH` `TensorOps`/`TensorOpFamily` split and operates on the one `SparseCompressedRowMatrixStorage<double>` the `[02]-[SPARSE_SOLVE]` ingestion fold owns — a parallel sparse-tensor type is the deleted form; the fold routes each op to the library that owns it — the inherited `Matrix<double>` `Multiply(Matrix)`/`Add(Matrix)`/`Transpose()`/scalar `Multiply(double)`/`Multiply(Vector, Vector)` for SpMM/add/scale/transpose/SpMV (MathNet sparse owns these), `Matrix<double>.KroneckerProduct(Matrix<double>)` for the tensor-product element space (the sparse operands build through the `new SparseMatrix(storage)` ctor and the Kronecker rides the inherited `Matrix<T>` member, never a `SparseMatrix.OfStorage` phantom), and CSparse `CoordinateStorage` pattern construction for an arbitrary index `contract` MathNet does not own — a hand-rolled triple loop beside `Matrix<double>.Multiply` or a managed Kronecker beside `KroneckerProduct` is the named reimplementation defect; the `EinsumPlan` contraction order is the named statement seam (the greedy heuristic walks a mutable cost array) bounded by the binary-pairwise reduction so each lowered step is exactly one settled `MatMul`/`Contract` row, never an n-ary kernel that bypasses the `KernelLowering` table, and the contraction-order optimization is exponential in operand count so the planner uses a greedy/DP heuristic bounded by intermediate-size cost rather than an exhaustive search, and the multi-operand contraction threads each intermediate back into one unified working operand store keyed by the surviving `Left` index (dense `Left` | sparse-CSR `Right`, the route decided at execution from the working operands' real sparsity) so a 3+-operand einsum chains correctly — an n-ary fold that indexes two disjoint dense/sparse operand arrays by one shared tree index (crashing a mixed contraction, dropping every dense intermediate to the empty-result fault) is the deleted incoherent form; the sparse SpMV `Solver/contract#SOLVE_CONTRACT` Newton residual and every adjoint leg consume `SparseTensorOps.Spmv` over one held `CompressedColumnStorage<double>` (finalized once through `SparseOps.ToCsc`, or read straight off `FactoredOp.A`) rather than re-wrapping the storage per call (the named per-call-rematerialize defect the contract page already carries) — the adjoint arm binds CSparse `TransposeMultiply`, so a gradient of a residual, a normal-equation assembly, and a PCE design-matrix transpose never allocate a second CSC through `A.Transpose()`, and the accumulating `αA'x + βy` arm carries the iterative adjoint sweep with no temporary; the caller-owned destination and the span overloads keep the whole GEMV path allocation-free where a `Vector<double>` round trip staged three arrays per iterate; `FactoredOp.Solve` routes its own residual witness through the same owner so the page carries exactly one GEMV spelling; the sparse `contract` feeds the einsum planner, the `AUTODIFF_DUAL_MODE_ENGINE` colored Jacobian assembles as sparse contractions over the `contract` row, and the `Spmv`/`Spmm` rows stay CPU-lowered — the `Tensor/dispatch#DEVICE_KERNELS` registry carries no sparse shader row and the device path is never a phantom mapping; the nnz-growth allocation policy is fixed and stamped on the receipt because the MathNet sparse operators return fresh storage per op — a sparse fold that claims the dense lane's in-place `SpanOwner` discipline is dishonest, so the sparse `AllocationClass` is `PooledMemory` and the receipt records the result nnz.

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
    public static IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>> Contract(EinsumPlan plan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> operands, ShardPlan shard) =>
        operands.ForAll(static op => op.IsLeft) && DenseSubstrate.Active.Native
            ? IO.pure((plan.MatrixChain
                ? AtenDense.MultiDot(operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr))))
                : AtenDense.Einsum($"{string.Join(',', plan.OperandSubscripts)}->{plan.OutputSubscript}", operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr)))))
                .Map(dense => ((SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage,
                    Seq(new SparseRun(plan.OutputSubscript, 0, dense.RowCount, dense.ColumnCount, plan.MatrixChain ? "aten-multi-dot" : "aten-einsum")))))
            : plan.Tree.Fold(
                IO.pure(Fin.Succ((Work: toHashMap(operands.Map(static (op, i) => (i, op))), Steps: Seq<SparseRun>()))),
                (effect, step) => effect.Bind(state => state.Match(
                    Succ: held => Step(held.Work, step, plan.OutputSubscript, shard).Map(next => next.Map(row => (row.Work, held.Steps.Add(row.Run)))),
                    Fail: static error => IO.pure(Fin.Fail<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, Seq<SparseRun> Steps)>(error)))))
            .Map(state => state.Map(held => (CsrOf(held.Work[plan.Tree.IsEmpty ? 0 : plan.Tree[plan.Tree.Count - 1].Left]), held.Steps)));

    static IO<Fin<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, SparseRun Run)>> Step(
        HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> work, (int Left, int Right, string Subscripts) step, string output, ShardPlan shard) {
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> l = work[step.Left];
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> r = work[step.Right];
        return l.IsLeft && r.IsLeft
            ? KernelLowering.Lower(TensorOpFamily.MatMul, DenseOf(l), DenseOf(r), shard)
                .Map(result => result.Map(dense => (work.AddOrUpdate(step.Left, Left<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(dense)).Remove(step.Right),
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

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, with the `ShardPlan` block-decomposition column the dense GEMM reads and the `ProveGemm` GEMM-vs-naive-reference proof the `Tensor/dispatch#EQUIVALENCE_INTEROP` equivalence law's matrix arm reads (the matrix lane has no scalar-tail kernel to span-prove, so the lowering owner that carries the GEMM also OWNS its proof and MatMul/Conv admission).
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` (local `Matrix<double>.Multiply` leaf) · `Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload, Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentKey, ComputeReceipt.Factorization Receipt)` the per-block join carrier.
- Entry: both `Lower` overloads return `IO<Fin<Matrix<double>>>`; `ShardPlan.Single` lifts the pure GEMM and `ShardPlan.Blocked` composes cache, blob, and RPC effects. `Pool` remains pure over its span, `ProveGemm` reads the same local kernel, and `IsMatrix` reports the proof-owned row set.
- Auto: matrix rows consult `KernelLowering`; `MatMul` lowers to the active-provider GEMM, `Conv*` projects through `Im2Col`, and pooling folds `TensorPrimitives`. `ShardPlan.Blocked` traverses row blocks on `IO<Fin<ShardBlock>>`, so lookup, payload fetch/store, publish, and RPC dial remain one deferred algebra; `Fin` aborts the join after effects yield typed results.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and the `Blocked` fan-out aggregates one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the per-node `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc`, or `Substrate.CpuTensor` on a content-address cache hit) — the shard count is the block count and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Deterministic.Source` the one draw adapter), Rasm.Persistence (project), BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; a new matrix row inherits `ProveGemm` (the shared GEMM-core proof) with no new proof surface; zero new surface.
- Boundary: `Im2Col` assigns each `(position, channel)` a disjoint heap-array run, then one provider GEMM carries `MatMul` proof evidence for every convolution row. `ShardPlan.Blocked` carries the bounded RPC stub, provider, factor kind, reuse index, object-store ports, correlation, clock, deadline, and cancellation token as row data. Both operands ride raw column-major float64 bytes through `UnsafeByteOperations.UnsafeWrap`; request hashing uses pooled serialization, cache lookup resolves only a residence, and blob custody stays on the object-store ports. `IO<Fin<T>>` composes lookup, fetch, dial, store, and publish without an interior effect run; `Fin` gates the private join target after traversal.

```csharp signature
public sealed record ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentKey, ComputeReceipt.Factorization Receipt) {
    public static ShardBlock Join(Matrix<double> target, ShardBlock block) {
        target.SetSubMatrix(block.Start, 0, block.Solution);
        return block;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShardPlan {
    private ShardPlan() { }

    public sealed record Single : ShardPlan;
    public sealed record Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload, Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel) : ShardPlan;

    public IO<Fin<Matrix<double>>> Lower(Matrix<double> left, Matrix<double> right) =>
        left.ColumnCount != right.RowCount
            ? IO.pure(Fin.Fail<Matrix<double>>(ComputeFault.Create($"<gemm-inner-dim:{left.ColumnCount}!={right.RowCount}>")))
            : Switch(
                state: (Left: left, Right: right),
                single: static (s, _) => IO.pure(Fin.Succ(Local(s.Left, s.Right))),
                blocked: static (s, plan) => plan.Tile > 0 && plan.Deadline > Duration.Zero
                    ? Fanout(s.Left, s.Right, plan)
                    : IO.pure(Fin.Fail<Matrix<double>>(ComputeFault.Create($"<shard-policy:tile={plan.Tile}:deadline={plan.Deadline}>"))));

    internal static Matrix<double> Local(Matrix<double> left, Matrix<double> right) => left.Multiply(right);

    static IO<Fin<Matrix<double>>> Fanout(Matrix<double> left, Matrix<double> right, Blocked plan) =>
        toSeq(Enumerable.Range(0, (left.RowCount + plan.Tile - 1) / plan.Tile))
            .Traverse(block => {
                int start = block * plan.Tile;
                int height = Math.Min(plan.Tile, left.RowCount - start);
                return SubSolve(left.SubMatrix(start, height, 0, left.ColumnCount), right, start, height, plan);
            })
            .Map(blocks => blocks.Traverse(identity).Map(rows =>
                rows.Fold(Matrix<double>.Build.Dense(left.RowCount, right.ColumnCount), static (target, block) => { ShardBlock.Join(target, block); return target; })));

    static IO<Fin<ShardBlock>> SubSolve(Matrix<double> rowBlock, Matrix<double> right, int start, int height, Blocked plan) {
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
        UInt128 address = Digest(request, plan.Provider.SolveDedupKey((UInt128)start));
        Instant dialedAt = plan.Clock.GetCurrentInstant();
        // `ModelResultIndex` resolves only payload residence; object-store absence misses cleanly to a re-dial,
        // preserving one reuse seam without Compute-side payload custody.
        return plan.Reuse.Lookup(address).Bind(row => row.Match(
            Some: cached => plan.FetchPayload(cached.Residence).Bind(bytes => bytes.Match(
                Some: payload => IO.pure(Try.lift<Fin<ShardBlock>>(() =>
                        Materialize(SolveResponse.Parser.ParseFrom(payload.Span), address, start, height, right.ColumnCount, Substrate.CpuTensor, Duration.Zero, plan))
                    .Run()
                    .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<cached-solve-payload:{error.Message}>"))
                    .Bind(identity)),
                None: () => DialAndStore(plan, request, address, start, height, right.ColumnCount, dialedAt))),
            None: () => DialAndStore(plan, request, address, start, height, right.ColumnCount, dialedAt)));
    }

    // Write-blob-first stores one pooled serialization, then publishes the dedup row over its residence;
    // provider determinism already participates in the lookup key.
    static IO<Fin<ShardBlock>> DialAndStore(Blocked plan, SolveRequest request, UInt128 address, int start, int height, int cols, Instant dialedAt) =>
        Dial(plan, request).Bind(result => result.Match(
            Succ: response => Materialize(response, address, start, height, cols, Substrate.RemoteGrpc, plan.Clock.GetCurrentInstant() - dialedAt, plan).Match(
                Succ: block => Store(plan, response, address).Map(_ => Fin.Succ(block)),
                Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))),
            Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))));

    static IO<Unit> Store(Blocked plan, SolveResponse response, UInt128 address) =>
        IO.lift(() => {
            MemoryOwner<byte> rent = MemoryOwner<byte>.Allocate(response.CalculateSize());
            response.WriteTo(rent.Span);
            return rent;
        }).Bracket(
            Use: rent => from residence in plan.StorePayload(rent.Memory)
                         from _ in plan.Reuse.Publish(new ModelResultRow(address, residence, plan.Provider.DeterminismTag, plan.Clock.GetCurrentInstant()))
                         select unit,
            Fin: static rent => IO.lift(() => { rent.Dispose(); return unit; }));

    static UInt128 Digest(SolveRequest request, UInt128 salt) {
        int width = request.CalculateSize();
        using SpanOwner<byte> rent = SpanOwner<byte>.Allocate(width + 16);
        request.WriteTo(rent.Span[..width]);
        BinaryPrimitives.WriteUInt64LittleEndian(rent.Span[width..], (ulong)salt);
        BinaryPrimitives.WriteUInt64LittleEndian(rent.Span[(width + 8)..], (ulong)(salt >> 64));
        return XxHash128.HashToUInt128(rent.Span);
    }

    static Fin<ShardBlock> Materialize(SolveResponse response, UInt128 address, int start, int height, int defaultCols, Substrate substrate, Duration elapsed, Blocked plan) {
        int cols = response.Cols == 0 ? defaultCols : (int)response.Cols;
        if (cols <= 0 || response.Solution.Length != (long)height * cols * sizeof(double)) {
            return Fin.Fail<ShardBlock>(new ComputeFault.PayloadOverBounds($"<solve-shape:height={height}:cols={cols}:bytes={response.Solution.Length}>"));
        }
        ComputeReceipt.Factorization receipt = new(response.Provider, response.Decomposition, height, cols, response.Nnz, "dense") {
            Scope = new ReceiptScope.Execution(plan.Correlation, WorkLane.Background, substrate, AllocationClass.PooledMemory, elapsed), DeterminismTag = plan.Provider.DeterminismTag,
        };
        return Try.lift(() => new ShardBlock(start, height, Restore(response, height, cols), address, receipt))
            .Run()
            .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<solve-materialize:{error.Message}>"));
    }

    static IO<Fin<SolveResponse>> Dial(Blocked plan, SolveRequest request) =>
        IO.lift(() => Try.lift(() => plan.Compute.Solve(request, new CallOptions(new Metadata { { "rasm-correlation", plan.Correlation.ToString() } })
                    .WithDeadline(plan.Clock.GetCurrentInstant().Plus(plan.Deadline).ToDateTimeUtc())
                    .WithCancellationToken(plan.Cancel)))
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
}

public static class KernelLowering {
    // Convolution spatial rank is row data, never literal arithmetic over a key; this table's keys are the
    // geometry overload's admitted convolution rows.
    static readonly FrozenDictionary<TensorOpFamily, int> ConvRank = new (TensorOpFamily Row, int Rank)[] {
        (TensorOpFamily.Conv1D, 1), (TensorOpFamily.Conv2D, 2), (TensorOpFamily.Conv3D, 3),
    }.ToFrozenDictionary(static r => r.Row, static r => r.Rank);

    static readonly FrozenSet<TensorOpFamily> MatrixRows = new[] {
        TensorOpFamily.MatMul, TensorOpFamily.Conv1D, TensorOpFamily.Conv2D, TensorOpFamily.Conv3D,
    }.ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> PoolRows = new[] {
        TensorOpFamily.MaxPool, TensorOpFamily.AvgPool, TensorOpFamily.GlobalMaxPool, TensorOpFamily.GlobalAvgPool,
    }.ToFrozenSet();

    public static IO<Fin<Matrix<double>>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        row == TensorOpFamily.MatMul ? plan.Lower(left, right)
        : IO.pure(Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>")));

    public static IO<Fin<Matrix<double>>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan) =>
        ConvRank.TryGetValue(row, out int rank) && window.Rank == rank
            ? plan.Lower(Im2Col(input, window), kernel)
            : IO.pure(Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>")));

    static Matrix<double> Im2Col(Matrix<double> input, ConvWindow window) {
        int[] extents = window.OutputExtents;
        double[,] patch = new double[window.OutputPositions, window.PatchWidth];
        PatchGather gather = new(input, window, extents, patch);
        ParallelHelper.For2D(0, window.OutputPositions, 0, window.Channels, in gather);
        return Matrix<double>.Build.DenseOfArray(patch);
    }

    // Each `(outputPosition, channel)` owns one disjoint patch-row run; heap storage satisfies `IAction2D` without a ref-struct field.
    readonly struct PatchGather(Matrix<double> input, ConvWindow window, int[] extents, double[,] patch) : IAction2D {
        public void Invoke(int position, int channel) {
            int[] origin = Unravel(position, extents);
            for (int tap = 0; tap < window.KernelVolume; tap++) {
                int[] offset = Unravel(tap, window.Kernel);
                int[] coords = toSeq(Enumerable.Range(0, window.Rank))
                    .Map(axis => origin[axis] * window.Stride[axis] + offset[axis] * window.Dilation[axis] - window.Padding[axis])
                    .ToArray();
                patch[position, channel * window.KernelVolume + tap] =
                    toSeq(coords).Zip(toSeq(window.Spatial)).ForAll(static pair => pair.First >= 0 && pair.First < pair.Second)
                        ? input[channel, Ravel(coords, window.Spatial)]
                        : 0d;
            }
        }
    }

    static int[] Unravel(int flat, int[] extents) =>
        toSeq(Enumerable.Range(0, extents.Length).Reverse())
            .Fold((Rest: flat, Coords: new int[extents.Length]), static (acc, axis) => {
                acc.Coords[axis] = acc.Rest % extents[axis];
                return (acc.Rest / extents[axis], acc.Coords);
            }).Coords;

    static int Ravel(int[] coords, int[] extents) =>
        toSeq(Enumerable.Range(0, extents.Length)).Fold(0, (index, axis) => index * extents[axis] + coords[axis]);

    public static Fin<double> Pool(TensorOpFamily row, ReadOnlySpan<double> window) =>
        row == TensorOpFamily.MaxPool || row == TensorOpFamily.GlobalMaxPool ? Fin.Succ(TensorPrimitives.Max(window))
        : row == TensorOpFamily.AvgPool || row == TensorOpFamily.GlobalAvgPool ? Fin.Succ(TensorPrimitives.Sum(window) / window.Length)
        : Fin.Fail<double>(ComputeFault.Create($"<pool-row-miss:{row.Key}>"));

    public static bool Lowers(TensorOpFamily row) => MatrixRows.Contains(row) || PoolRows.Contains(row);
    public static bool IsMatrix(TensorOpFamily row) => MatrixRows.Contains(row);

    // GEMM proof compares the exact local lowering kernel with an independent triple-loop reference.
    public static ProofEvidence ProveGemm(int sampleCount) {
        int n = Math.Max(2, (int)Math.Sqrt(sampleCount));
        Matrix<double> left = Gaussian(n, sampleCount, lane: 0), right = Gaussian(n, sampleCount, lane: 1);
        Matrix<double> gemm = ShardPlan.Local(left, right);
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

- [SHARD_FANOUT]-[OPEN]: does the generated `ComputeService` stub dial resolve inside the integrated host ALC while preserving `IO<Fin<Matrix<double>>>`, raw column-major request bytes, bounded call options, `ModelResultIndex` residence lookup, object-store payload custody, and per-shard receipts; `tools.assay bridge` live-host scenario against the loaded plugin ALC.
