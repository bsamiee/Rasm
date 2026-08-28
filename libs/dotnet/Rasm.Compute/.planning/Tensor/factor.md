# [COMPUTE_FACTOR]

Rasm.Compute sparse-solve and kernel-lowering lane: the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SparseContainer` two-container exchange correspondence, and the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel with the `ShardDispatch` local-or-farm axis the dense GEMM reads. Every library refuses its own gates — `Iterator<T>` exposes no iteration count — so the criterion stack re-imposes each, and every solve leaves as the branch's ONE `Tensor/blas#DENSE_ALGEBRA` `SolveOutcome<T>` carrying the iterate, the substrate that served it, the witnessed residual, the steps spent, and a total `SolveTermination`. Distributed solves cross solely through the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc, whose frozen `shard_tile` column the `ShardDispatch.Farm` row-block sub-solve dials.

## [01]-[INDEX]

- [02]-[SPARSE_SOLVE]: CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative with the solver ladder and the budget criterion; overdetermined sparse-QR least-squares route; the `SparseContainer` exchange correspondence (`.mtx` · HDF5 archive).
- [03]-[SPARSE_ALGEBRA]: `SparseTensorOpFamily` op axis over CSR storage; the `GemvForm` forward/adjoint held GEMV; SpMM/add/transpose/Kronecker/contract; `EinsumPlan` pairwise lowering over dense GEMM and sparse contract.
- [04]-[KERNEL_LOWERING]: tensor matrix/structural rows lower onto real GEMM/im2col/pool through one operand union; shard fan-out.

## [02]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows carrying the CSR-conversion `Ingest` delegate as a GENERATED column; `FactorKind` `[SmartEnum<string>]` direct-factor rows carrying the capability columns (rank-1 edit, transpose-solve, inertia, reentrancy, symmetry) and the `Fill`/`Create`/`TransposeRecover` delegates as generated columns; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis whose `Ladder` column also composes the rungs into the `CompositeSolver` fall-through row, with `MethodSetup` the `IIterativeSolverSetup<double>` projection of one row and the `IterationPolicy` record (tolerance · max-iter · criterion stack · preconditioner · clock · deadline · cancellation); `FactoredOp` the typed sparse-operator value owning the factorization instance, the cached AMD permutation, the `ColumnOrdering` it was factored with, symbolic fill, and kind discriminant; `StructuralDeficiency` the typed Dulmage-Mendelsohn refusal carrying both deficient block spans; `Edit` `[Union]` the structural-edit dialect; `SparseContainer` `[SmartEnum<string>]` the portable-exchange correspondence carrying its own read and write columns; `SparseExchange` the closed read carrier preserving the HDF-only reproduction metadata in its `Archive` case; `SparseOps` the direct-and-iterative sparse-solve fold over CSR-backed MathNet storage and CSparse CSC direct factorizations.
- Cases: `SparseFormat` `Ingest`-column rows csr · csc · coo · dok (4); `FactorKind` verified `Create`-column rows spd · ldl · lu · qr (4, every row wired — `Ldl` binds `SparseLDL.Create`); `IterativeMethod` rows bicgstab · gpbicg · tfqmr · mlk-bicgstab · composite (5, the composite deriving its ladder from the four `Ladder` rows); `Edit` cases `Pin` · `Prune` · `Bump` · `Revalue` (4, every case realized, admitted before mutation, and carrying its own structural-re-gate column); `SparseContainer` rows mtx · hdf5 (2, each carrying both directions of the one correspondence); `SparseExchange` cases `MatrixMarket` · `Archive`, the latter carrying one `SparseArchiveMeta` value.
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — the ONE sparse admission boundary, whose nine independent facts ACCUMULATE so a caller handed a short minor run over a non-monotone pointer array learns both; `public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor)` converts the CSR triplets once to a CSparse `CompressedColumnStorage<double>` through `CoordinateStorage` + the admitted `CompressedColumnStorage<double>.OfIndexed` CSC factory, reads the symbolic fill before the numeric sweep, and collapses the completed factorization to one `FactoredOp` value; `FactoredOp.Solve(double[] rhs, TolerancePolicy tol)` is the one polymorphic solve over both shapes, returning the lane-wide `SolveOutcome<double[]>`; `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected `IIterativeSolver<double>` under the explicitly-ordered criterion stack and returns the same outcome carrier; `SparseContainer.Read` returns the container-cased `SparseExchange`, and `SparseContainer.Write` is its row's inverse.
- Auto: every format row maps to one CSR ingestion conversion through its own generated `Ingest` column — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type and the build closure rides the row, not a parallel ingestion table; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `FactorKind` row's `Create` column binding the explicit-permutation `SparseCholesky.Create(csc, p)`/`SparseLDL.Create(csc, p)`/`SparseLU.Create(csc, p, pivotTol)` and the ordering-based `SparseQR.Create(csc, ordering)`, so the AMD ordering is computed once by `Build` and the symmetric/lu kinds reuse that permutation rather than re-deriving it inside `Create`, then solve in place through `ISparseFactorization<double>.Solve(double[], double[])`; iterative solves run the row's `Solver(policy)` factory under the `IterationPolicy.Iterator(observe)` criterion stack constructed in precedence order `Failure → Budget → Divergence → Residual → IterationCount`, the budget delegate observing each tested iteration without a second counter type, and the `composite` row folding the four `Ladder` rows into a `CompositeSolver` that falls through to the next rung on divergence, breakdown, or a swallowed throw instead of returning one method's failure; `FactoredOp.TransposeSolve` recovers the transpose-solve action from the `FactorKind` row's `TransposeRecover` column alone (some for lu and qr, none for spd and ldl) because the shared `ISparseFactorization<double>` exposes only the forward solve and `SolveTranspose` closes over the concrete `SparseLU`/`SparseQR`.
- Result: `FactoredOp` retains the sparse factorization, factor kind, admitted operator, permutation, ordering, fill, and Frobenius norm. Its solves return `SolveOutcome<double[]>` with the field and independently recomputed true relative residual.
- Packages: MathNet.Numerics, MathNet.Numerics.Data.Text (`MatrixMarketReader`/`MatrixMarketWriter` — the `.mtx` interop leg), CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Microsoft.IO.RecyclableMemoryStream, PureHDF (through `Runtime/archive#HDF_ARCHIVE`), Grpc.Net.Client, Google.Protobuf, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `SparseFormat` row carrying its `Ingest` column; a new direct solver is one `FactorKind` row carrying its capability, symmetry, fill, transpose-recovery, AND create columns together (one row, never a row beside a parallel `DirectSolvers` table edit); a new iterative method is one `IterativeMethod` row carrying its `Ladder` column and its factory column, and the composite ladder absorbs it with no fold edit; a new structural-edit dialect is one `Edit` case carrying its `Regates` column with its arm on the total `Apply` Switch; a new exchange container is one `SparseContainer` row carrying BOTH directions, never a direction-named entrypoint pair; a new iteration knob is one column on the `IterationPolicy` record and one criterion on `Composed`, minted through `IterationPolicy.Of` so the lane's clock, budget, and token stay the record's only ambient-free ingress; zero new surface.
- Boundary — storage: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage, so csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner per format is the deleted form. `Factor`'s CSR-to-CSC handoff builds a `CoordinateStorage<double>(rows, cols, nnz)`, calls `.At(i, j, v)` per entry, and converts once through the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` factory — the CSparse static that internally runs `Converter.ToCompressedColumnStorage` with cleanup — so a hand-rolled `Converter` detour beside it is the named reimplementation defect, and `inplace: true` is rejected wherever the triplet must survive a structural-edit increment because it invalidates the source arrays and dangles references. Bare `SparseMatrix` is reserved for the MathNet CSR concrete (`SolveIterative`), so the two sparse libraries never alias one name. `ToColumnStorage` is the ONE conversion spelling and the cross-lane projections the `Solver/contract#SOLVE_REQUEST` condensed modal pencil takes reach it directly — a one-line forwarder renaming it resolved the same name in two hops for no added meaning.
- Boundary — admission: `CSparse.Helper.ValidateStorage(csc, strict: true)` gates the CSC inside `Factor` before any factory touches it, because it returns `bool` rather than throwing and factorizing invalid storage produces silently incorrect factors. Ingestion's own nine gates ACCUMULATE through `Validation` rather than short-circuiting a ternary ladder onto one interpolated slug, so an operand that breaches three states three — the ladder answered the first and left a caller to re-submit twice more to discover the rest.
- Boundary — ordering and fill: the `ColumnOrdering.MinimumDegreeAtPlusA` permutation `int[]` from `CSparse.Ordering.AMD.Generate(CompressedColumnStorage<double>, ColumnOrdering)` caches as the value-only refactor key over an invariant pattern (`ColumnOrdering` values are `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, `MinimumDegreeAtA`; the AMD ordering type lives in `CSparse.Ordering`, distinct from the `ColumnOrdering` enum, and its `Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order)` takes the matrix first and the ordering second). Symbolic fill is read before the numeric sweep to route direct versus iterative, and the count is per-kind through the `FactorKind.Fill` column — one factor for the symmetric kinds, `L + U − n` for `SparseLU`, `Q + R − m` for `SparseQR` — so a bare fill integer compared across kinds is meaningless. Assembly residue drops with a structural tolerance near `machineEps · ‖A‖_F` through `DropZeros(tolerance)` because the default `0.0` removes only binary zeros, and `SparseLU` pivot `tol` is `[0, 1]` as a relative column threshold (`1` full partial pivoting, `0` disabled), never an absolute floor.
- Boundary — structural gate: `DulmageMendelsohn.Generate(csc)` runs on the DEFAULT seed, because that second parameter is a randomization SEED selecting the maximum-matching order (`0` natural, `-1` reverse, anything else random) and NOT a dimension — feeding it a row count picks a randomized matching whose block boundaries move run to run, which forfeits the replayability every other axis on this page holds. `Generate` returns `null` when the matching or either breadth-first sweep fails, so the null converts to the typed refusal rather than dereferencing. Structural rank reads `dm.StructuralRank` (the library's own `rr[3]`) and the deficiency is `min(m, n) − StructuralRank`; a hand-derived deficiency over coarse-block arithmetic is the deleted reimplementation. `A(p, q)`'s coarse decomposition is the 3-by-3 block structure, `CoarseRowDecomposition` and `CoarseColumnDecomposition` both `int[5]` boundary arrays: rows split `[rr[0], rr[1])` under-determined, `[rr[1], rr[2])` square well-determined, `[rr[2], rr[3])` over-determined, `[rr[3], rr[4] = m)` unmatched; columns split `[cc[0], cc[1])` unmatched, `[cc[1], cc[2])` under-determined, `[cc[2], cc[3])` square, `[cc[3], cc[4] = n)` over-determined. That refusal is a RECORD — rank, deficiency, order, and both deficient block spans as typed fields — because a caller localizing an assembly defect reads spans, and a six-part hand-concatenated string forced every consumer to parse the structure back out of the sentence describing it.
- Boundary — capability recovery: transpose-solve, rank-1 edit, inertia, and reentrancy recover from the `FactorKind` row alone because the shared `ISparseFactorization<double>` exposes only the forward solve. `Ldl` — the symmetric-indefinite/inertia kind — binds `SparseLDL.Create` as a real `Create` column: a capability row with no factory delegate is the named declared-but-unbound defect. Feeding an asymmetric input to a symmetric kind factors its symmetrization and returns a correct answer to the WRONG system, so the post-solve true residual is the only signal it fires.
- Boundary — failure capture and reentrancy: a typed-only catch at the factorization boundary is rejected because SPD pivot loss and the zero-diagonal break throw bare `Exception`. Cached square factorizations hold one constructor-allocated non-reentrant scratch, so solves serialize through the `FactoredOp` capsule and the `SparseQR` reentrant kind is the one parallel-safe row. Cache population is success-only, so only residual-witnessed factorizations enter and a diverged solve never poisons reuse.
- Boundary — rectangular least squares: the result buffer sizes from `A.ColumnCount` exactly like the square solve, because `SparseQR.Solve` writes the `n`-length left-hand side and allocates the augmented `S.m2` work row INTERNALLY as private factor state with no public accessor — over-sizing the caller buffer from a nonexistent "solution dimension" member is the named phantom. `Qr` is the one rectangular route on `FactoredOp.Solve`, so an overdetermined sparse system (`Solver/contract#SOLVE_REQUEST` normal-equations recovery, `Solver/uncertainty#UNCERTAINTY_LANE` PCE coefficient fit, `Tensor/dispatch#EQUIVALENCE_INTEROP` sparse-Jacobian recovery) minimizes `‖Ax−b‖` through `SparseQR.Solve` and the witness recomputes against the ORIGINAL rectangular `A` (`ax` sized `A.RowCount`, the m-residual against the b-vector) — never a dense `Matrix<double>.QR` fallback and never the square normal-equations operator whose conditioning the rectangular QR avoids. CSparse's `SparseQR` below `m = n` returns the MINIMUM-NORM solution rather than a least-squares one, which is a different answer to a different question, so an under-determined operand refuses BY NAME on this route instead of returning a plausible vector under a least-squares result.
- Boundary — GEMV ownership is ONE spelling PER LIBRARY, because the two solve legs hold two operator types and neither converts to reach the other's kernel. Every CSparse `CompressedColumnStorage<double>` path — the direct-solve witness, the adjoint sweep, the einsum contract — routes `SparseTensorOps.Spmv`, calling `Multiply`/`TransposeMultiply` on the held operator because CSparse's `Matrix<T>` base implements `ILinearOperator<double>` and declares the vector `Multiply(ReadOnlySpan<double>, Span<double>)`/`Multiply(double[], double[])` the concrete `SparseMatrix` overrides; a residency cast to `CSparse.Double.SparseMatrix` to reach a member the base already exposes is the deleted ceremony. `SolveIterative` holds a MathNet `SparseMatrix` the Krylov solver itself constructed, so its residual reads `matrix.Multiply(x)` on THAT operator — converting an iterate's operand to CSC per solve to force one spelling pays a full conversion for a norm.
- Boundary — edit dialect: every `Edit` applies to the operator before re-factoring — `Pin` drops row+column `node` and seats a unit diagonal, `Prune` `DropZeros` over a clone, a rank-1-edit kind's `Bump` runs the `SparseCholesky` `Update`/`Downdate` and discards-and-reconstructs the BUMPED operator (never the unedited one) on a `false` result, a non-rank-1-edit `Bump` accumulates `A + sign·w·wᵀ` over the column support and re-factors, and a `Bump` on a rectangular operator is rejected because a symmetric rank-1 update is ill-defined there; a default arm that silently re-factors the unedited operator and drops the payload is the deleted form. Value-only `Revalue` clones the CSC through `Clone()` before overwriting the value array, because the old `FactoredOp` still references the original storage and an in-place `CopyTo` corrupts the pre-edit operator, then re-creates with the SAME `op.Kind` from the cached permutation — a hardcoded `SparseLU.Create` re-create silently changing a non-LU operator's kind is the deleted correctness defect. Explicit-permutation `Create` amortizes the dominant symbolic cost and yields a fully INDEPENDENT factor, so the in-place CSparse `Refactorize` — which reuses the elimination tree and column counts too but MUTATES the shared factor instance, aliasing the pre-edit `FactoredOp` whose `Inner` other readers and the non-reentrant single-owner solve still hold — is deliberately not taken: value immutability outranks the marginal numeric-phase saving, and `SparseQR` exposes no `Refactorize` at all. `SparseOps.Apply(edit, pivotTol)` is the stable surface the `Solver/route#SOLVE_ROUTES` `SolveSession` composes, `Edit.Revalue` its standing case.
- Boundary — re-gate discriminant: the structural gate re-runs for the edits that can REMOVE pattern entries and skips for the rest, read off the `Edit` row's own `Regates` column rather than an edit-name test at the call. `Pin` drops a whole row and column and `Prune` drops residue whose removal can empty a row, so either can lower a structural rank the pre-edit gate proved; `Bump` only ADDS entries over the column support and `Revalue` rewrites values under an invariant pattern, so neither can, and re-running the Dulmage-Mendelsohn sweep on them pays a full pattern decomposition for an answer that cannot change.
- Boundary — iterative axis: the method is the closed `IterativeMethod` SmartEnum and a raw-`string` discriminant beside it is the named defect. Criterion stacks construct explicitly in precedence order because insertion order IS precedence — `Failure` first keeps `NaN` terminal, and `Residual` before the count cap suppresses convergence on the final iteration. The preconditioner is `MILU0Preconditioner` — the modified ILU(0) whose `Initialize` REQUIRES `SparseCompressedRowMatrixStorage<double>`, which is exactly the storage this lane holds, and whose cost tracks nnz over the raw CSR buffers where `ILU0Preconditioner` runs an indexer triple-loop and materializes a dense row per `Approximate`; the ILU spelling `IncompleteLU` exists on NO precision plane and is a phantom. It initializes outside the solve and catches its throw there, because the init throw otherwise escapes the verdict-returning entrypoint, and that pre-initialize runs for the `Ladder` rows alone since `CompositeSolver` resolves each rung's preconditioner in its own constructor and passes `setup ?? argument`, making the API's argument provably dead on the composite row where a second incomplete factorization is pure waste. `CompositeSolver` swallows every rung throw and falls through, so a breakdown inside it is invisible to the caller and the recomputed true residual is the only gate. `Iterator(observe)` construction captures the ONE instant the budget criterion anchors on and observes iteration checks through MathNet's own `DelegateStopCriterion<T>` callback, so the returned `SolveOutcome<T>.Steps` is measured rather than the declared cap; the ladder's per-rung `Iterator.Reset()` clears only criterion status and cannot re-arm that instant, where the per-rung `IterationCountStopCriterion` does re-arm and an unbounded ladder burns `rungs × MaxIterations`. `MethodSetup` binds `double.NaN` to `SolutionSpeed`/`Reliability` because no producer measured them and the ONE member reading them is `SolverSetup<T>.LoadFromAssembly`, the reflection discovery form this lane rejects, so a zero there ranks a ladder nothing measured.
- Boundary — witness and outcome: the iterate is admitted only on the independently recomputed true relative residual against the original operator, because the converged verdict certifies solely that the PRECONDITIONED residual fell below tolerance and left preconditioning distorts the norm. Structural substitution is the most dangerous form because it certifies an arbitrary iterate under a normal verdict and the ULP guard fails open on `NaN`. Deadline breaches are budget exhaustion and land `SolveTermination.Exhausted` so the partial iterate survives a relaxed-criterion retry, while divergence, breakdown, cancellation, and a `Continue` terminal each fail the result — folding them into `Exhausted` publishes a diverged iterate as a retryable partial. The carrier is the branch's ONE `SolveOutcome<T>`, so a sparse solve, a dense solve, a refinement, and a fit all report through one shape and no lane mints a third termination union.
- Boundary — exchange: the two containers are ONE correspondence carried by `SparseContainer` rows, each holding both directions as columns, because a read and a write of one format are the forward and inverse of a single map and four direction-named entrypoints made that map four names. `.mtx` rides the pinned `MathNet.Numerics.Data.Text` surface that actually exists: `ReadMatrix<double>(TextReader)` and `WriteMatrix(TextWriter, Matrix<double>)` over the pooled recyclable stream. A coordinate result must expose `SparseCompressedRowMatrixStorage<double>` and then re-enters `Ingest`; an ARRAY result refuses rather than densifying an exchange operand. The writer projects the held CSparse CSC through `SparseMatrix.OfIndexed(op.A.EnumerateIndexedAsValueTuples())`, the two libraries' admitted iterator/factory bridge, and MathNet emits its fixed `general` coordinate header — there is no symmetry parameter on any pinned writer overload. Matrix Market therefore carries operand values only in BOTH branches; structure and factor reproduction policy never hide in a header one peer cannot surface. The HDF5 sibling carries the scipy sparse group convention — rank-1 `indptr`/`indices` int32, `values` float64, extents in the int64 `shape` attribute, `format` naming the major axis — plus the reproduction metadata `.mtx` drops: kind, ordering, symbolic fill, ‖A‖_F, uint8 `symmetric`, and the applied AMD permutation as its own rank-1 dataset. INT32 index width is exchange law at both ends: an operand whose nnz, shape, or pointer run exceeds `int.MaxValue` refuses AT WRITE rather than emitting a container the peer decoder cannot address, and both routes end at `Ingest` so every admission gate re-runs on read. `.mtx` stays the SuiteSparse interop surface, never retired.
- Boundary — fan-out: the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseFormat {
    public static readonly SparseFormat Csr = new("csr", pointerForm: true, majorIsRow: true, ingest: static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Csc = new("csc", pointerForm: true, majorIsRow: false, ingest: static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(r, c, vals.Length, minor, major, vals));
    public static readonly SparseFormat Coo = new("coo", pointerForm: false, majorIsRow: true, ingest: static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Dok = new("dok", pointerForm: false, majorIsRow: true, ingest: static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(r, c, vals.Select((v, k) => Tuple.Create(major[k], minor[k], v))));

    public bool PointerForm { get; }
    public bool MajorIsRow { get; }

    [UseDelegateFromConstructor]
    public partial SparseCompressedRowMatrixStorage<double> Ingest(int rows, int columns, int[] major, int[] minor, double[] values);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FactorKind {
    public static readonly FactorKind Spd = new("spd", rank1Edit: true, transposeSolve: false, inertia: false, reentrant: false, symmetric: true, rectangular: false,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseCholesky.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Ldl = new("ldl", rank1Edit: false, transposeSolve: false, inertia: true, reentrant: false, symmetric: true, rectangular: false,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseLDL.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Lu = new("lu", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: false, symmetric: false, rectangular: false,
        fill: static (nnz, rows, _) => (2 * nnz) - rows,
        create: static (csc, perm, _, tol) => SparseLU.Create(csc, perm, tol),
        transposeRecover: static inner => inner is SparseLU lu ? Some<Action<double[], double[]>>(lu.SolveTranspose) : None);
    public static readonly FactorKind Qr = new("qr", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: true, symmetric: false, rectangular: true,
        fill: static (nnz, rows, _) => (2 * nnz) - rows,
        create: static (csc, _, ordering, _) => SparseQR.Create(csc, ordering),
        transposeRecover: static inner => inner is SparseQR qr ? Some<Action<double[], double[]>>(qr.SolveTranspose) : None);

    public bool Rank1Edit { get; }
    public bool TransposeSolve { get; }
    public bool Inertia { get; }
    public bool Reentrant { get; }
    public bool Symmetric { get; }
    public bool Rectangular { get; }

    [UseDelegateFromConstructor] public partial int Fill(int nonZeros, int rows, int columns);
    [UseDelegateFromConstructor] public partial ISparseFactorization<double> Create(CompressedColumnStorage<double> csc, int[] permutation, ColumnOrdering ordering, double pivotTol);
    [UseDelegateFromConstructor] public partial Option<Action<double[], double[]>> TransposeRecover(ISparseFactorization<double> inner);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IterativeMethod {
    public static readonly IterativeMethod BiCgStab = new("bicgstab", ladder: true, solverType: typeof(BiCgStab), build: static _ => new BiCgStab());
    public static readonly IterativeMethod GpBiCg = new("gpbicg", ladder: true, solverType: typeof(GpBiCg), build: static _ => new GpBiCg());
    public static readonly IterativeMethod Tfqmr = new("tfqmr", ladder: true, solverType: typeof(TFQMR), build: static _ => new TFQMR());
    public static readonly IterativeMethod MlkBiCgStab = new("mlk-bicgstab", ladder: true, solverType: typeof(MlkBiCgStab), build: static _ => new MlkBiCgStab());
    public static readonly IterativeMethod Composite = new("composite", ladder: false, solverType: typeof(CompositeSolver),
        build: static policy => new CompositeSolver(toSeq(Items).Filter(static row => row.Ladder)
            .Map(row => (IIterativeSolverSetup<double>)new MethodSetup(row, policy, policy.Preconditioner()))));

    public bool Ladder { get; }

    public Type SolverType { get; }

    [UseDelegateFromConstructor] public partial IIterativeSolver<double> Solver(IterationPolicy policy);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseContainer {
    public static readonly SparseContainer Mtx = new("mtx",
        read: static source => SparseOps.ReadMtx(source).Map(static storage => (SparseExchange)new SparseExchange.MatrixMarket(storage)),
        write: static (staged, op, policy) => SparseOps.WriteMtx(staged));
    public static readonly SparseContainer Hdf5 = new("hdf5",
        read: static source => SparseOps.ReadArchive(source),
        write: static (staged, op, policy) => SparseOps.WriteArchive(staged, policy));

    [UseDelegateFromConstructor] public partial Fin<SparseExchange> Read(ExchangeSource source);
    [UseDelegateFromConstructor] public partial Fin<Unit> Write(Stream staged, FactoredOp op, HdfArchivePolicy policy);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExchangeSource {
    private ExchangeSource() { }

    public sealed record Streamed(RecyclableMemoryStream Staged) : ExchangeSource;
    public sealed record Archived(HdfHandle Handle) : ExchangeSource;
}

[Union]
public abstract partial record SparseExchange {
    private SparseExchange() { }

    public sealed record MatrixMarket(SparseCompressedRowMatrixStorage<double> Storage) : SparseExchange;
    public sealed record Archive(SparseCompressedRowMatrixStorage<double> Storage, SparseArchiveMeta Meta) : SparseExchange;
}

public sealed record SparseArchiveMeta(
    FactorKind Kind,
    ColumnOrdering Ordering,
    long Fill,
    double Frobenius,
    ReadOnlyMemory<int> Permutation);

[Union]
public abstract partial record Edit {
    private Edit() { }

    public sealed record Pin(int Node) : Edit;
    public sealed record Prune(double Tolerance) : Edit;
    public sealed record Bump(int Sign, double[] Column) : Edit;
    public sealed record Revalue(double[] Values) : Edit;

    public bool Regates => Switch(
        pin: static _ => true,
        prune: static _ => true,
        bump: static _ => false,
        revalue: static _ => false);
}

// --- [MODELS] --------------------------------------------------------------------------
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
    public static IterationPolicy Of(IClock clock, Duration deadline, CancellationToken cancel) =>
        new(1e-10, 1_000, 0.08, 10, static () => new MILU0Preconditioner(), clock, deadline, cancel);

    internal Validation<Error, Unit> Admits =>
        (Gate(MaxIterations >= 1, "iteration-budget", MaxIterations),
         Gate(double.IsFinite(Tolerance) && Tolerance > 0.0, "iteration-tolerance", Tolerance),
         Gate(Deadline > Duration.Zero, "iteration-deadline", Deadline))
            .Apply(static (_, _, _) => unit).As();

    private static Validation<Error, Unit> Gate<T>(bool held, string site, T value) where T : notnull =>
        held ? unit : TensorReason.PolicyInvalid.Fault(site, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);

    public Iterator<double> Iterator(Action<int> observe) => Composed(Clock.GetCurrentInstant(), observe);

    Iterator<double> Composed(Instant opened, Action<int> observe) =>
        new(
            new FailureStopCriterion<double>(),
            new DelegateStopCriterion<double>((iteration, _, _, _) => { observe(iteration); return Budget(opened); }),
            new DivergenceStopCriterion<double>(DivergenceIncrease, DivergenceFloor),
            new ResidualStopCriterion<double>(Tolerance),
            new IterationCountStopCriterion<double>(MaxIterations));

    IterationStatus Budget(Instant opened) =>
        Cancel.IsCancellationRequested ? IterationStatus.Cancelled
        : Clock.GetCurrentInstant() - opened >= Deadline ? IterationStatus.StoppedWithoutConvergence
        : IterationStatus.Continue;
}

public readonly record struct StructuralDeficiency(
    int Rank, int Deficiency, int Rows, int Columns,
    (int From, int To) UnderRows, (int From, int To) UnderColumns,
    (int From, int To) OverRows, (int From, int To) OverColumns,
    (int From, int To) UnmatchedRows, (int From, int To) UnmatchedColumns) {
    public static StructuralDeficiency Of(DulmageMendelsohn dm, CompressedColumnStorage<double> csc, int order) {
        int[] rr = dm.CoarseRowDecomposition, cc = dm.CoarseColumnDecomposition;
        return new(dm.StructuralRank, order - dm.StructuralRank, csc.RowCount, csc.ColumnCount,
            (rr[0], rr[1]), (cc[1], cc[2]), (rr[2], rr[3]), (cc[3], cc[4]), (rr[3], rr[4]), (cc[0], cc[1]));
    }

    public string Witness =>
        $"rank={Rank}:deficiency={Deficiency}:order={Rows}x{Columns}"
        + $":under={UnderRows.From}..{UnderRows.To}x{UnderColumns.From}..{UnderColumns.To}"
        + $":over={OverRows.From}..{OverRows.To}x{OverColumns.From}..{OverColumns.To}"
        + $":unmatched-rows={UnmatchedRows.From}..{UnmatchedRows.To}:unmatched-cols={UnmatchedColumns.From}..{UnmatchedColumns.To}";
}

public sealed record FactoredOp(ISparseFactorization<double> Inner, FactorKind Kind, CompressedColumnStorage<double> A, int[] Permutation, ColumnOrdering Ordering, int Fill, double FrobeniusNorm) {
    public Option<Action<double[], double[]>> TransposeSolve => Kind.TransposeRecover(Inner);

    public bool Rectangular => A.RowCount != A.ColumnCount;

    public Fin<SolveOutcome<double[]>> Solve(double[] rhs, TolerancePolicy tol) =>
        rhs.Length != A.RowCount
            ? TensorReason.ShapeMismatch.Fail<SolveOutcome<double[]>>("sparse-solve-shape", $"rhs={rhs.Length}", $"rows={A.RowCount}")
        : A.RowCount < A.ColumnCount
            ? TensorReason.ShapeMismatch.Fail<SolveOutcome<double[]>>("sparse-underdetermined", Kind.Key, $"{A.RowCount}x{A.ColumnCount}")
        : Rectangular && !Kind.Rectangular
            ? TensorReason.RowMissing.Fail<SolveOutcome<double[]>>("sparse-rectangular-route", Kind.Key)
        : Try.lift(() => {
                double[] x = new double[A.ColumnCount];
                Inner.Solve(rhs, x);
                return Fin.Succ(x);
            }).Run().Bind(static inner => inner)
            .Bind(field => Witness(field, rhs, tol));

    Fin<SolveOutcome<double[]>> Witness(double[] field, double[] rhs, TolerancePolicy tol) {
        double[] ax = new double[A.RowCount];
        return SparseTensorOps.Spmv(A, GemvForm.Apply, field, ax).Bind(_ =>
            TensorPrimitives.Distance<double>(ax, rhs) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs)) is var residual
            && double.IsFinite(residual) && tol.Admits(residual)
                ? Fin.Succ(SolveOutcome<double[]>.Settled(field, DenseSubstrate.Managed, residual))
                : TensorReason.WitnessFail.Fail<SolveOutcome<double[]>>("sparse-witness",
                    Kind.Key, $"rect={Rectangular}", $"fill={Fill}", $"r={residual:e3}"));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SparseOps {
    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) {
        int majorDim = format.MajorIsRow ? rows : columns;
        int minorDim = format.MajorIsRow ? columns : rows;
        return (Gate(rows > 0 && columns > 0, "extent", $"{rows}x{columns}"),
                Gate(minorIndices.Length == values.Length, "minor-values", $"{minorIndices.Length}!={values.Length}"),
                Gate(!format.PointerForm || majorIndices.Length == majorDim + 1, "pointer-length", $"{majorIndices.Length}!={majorDim + 1}"),
                Gate(!format.PointerForm || (majorIndices[0] == 0 && majorIndices[^1] == values.Length), "pointer-anchor", $"{majorIndices[0]}..{majorIndices[^1]}:nnz={values.Length}"),
                Gate(!format.PointerForm || Monotone(majorIndices), "pointer-monotone", format.Key),
                Gate(format.PointerForm || majorIndices.Length == values.Length, "major-values", $"{majorIndices.Length}!={values.Length}"),
                Gate(format.PointerForm || Bounded(majorIndices, majorDim), "major-bound", majorDim.ToString(CultureInfo.InvariantCulture)),
                Gate(Bounded(minorIndices, minorDim), "minor-bound", minorDim.ToString(CultureInfo.InvariantCulture)),
                Gate(TensorPrimitives.IsFiniteAll<double>(values), "values-finite", format.Key))
            .Apply(static (_, _, _, _, _, _, _, _, _) => unit).As().ToFin()
            .Map(_ => format.Ingest(rows, columns, majorIndices, minorIndices, values));
    }

    static Validation<Error, Unit> Gate(bool held, string site, string witness) =>
        held ? unit : TensorReason.ShapeMismatch.Fault($"sparse-ingest-{site}", witness);

    static bool Monotone(int[] pointers) =>
        pointers.Length < 2 || TensorPrimitives.Min<int>(Deltas(pointers)) >= 0;

    static int[] Deltas(int[] pointers) {
        int[] deltas = new int[pointers.Length - 1];
        TensorPrimitives.Subtract<int>(pointers.AsSpan(1), pointers.AsSpan(0, pointers.Length - 1), deltas);
        return deltas;
    }

    static bool Bounded(int[] indices, int extent) =>
        indices.Length == 0 || (TensorPrimitives.Min<int>(indices) >= 0 && TensorPrimitives.Max<int>(indices) < extent);

    public static CompressedColumnStorage<double> ToColumnStorage(SparseCompressedRowMatrixStorage<double> csr) {
        CoordinateStorage<double> coords = new(csr.RowCount, csr.ColumnCount, csr.ValueCount);
        toSeq(Enumerable.Range(0, csr.RowCount)).Iter(row =>
            toSeq(Enumerable.Range(csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row]))
                .Iter(slot => coords.At(row, csr.ColumnIndices[slot], csr.Values[slot])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    public static CompressedColumnStorage<double> Diagonal(double[] diagonal) =>
        CompressedColumnStorage<double>.OfDiagonalArray(diagonal);

    public static CompressedColumnStorage<double> Diagonal(int order, double value) =>
        CompressedColumnStorage<double>.CreateDiagonal(order, value);

    // --- [EXCHANGE] --------------------------------------------------------------------
    internal static Fin<SparseCompressedRowMatrixStorage<double>> ReadMtx(ExchangeSource source) =>
        source is ExchangeSource.Streamed staged
            ? Try.lift(() => {
                using TextReader reader = new StreamReader(
                    staged.Staged, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                Matrix<double> matrix = MatrixMarketReader.ReadMatrix<double>(reader);
                return matrix.Storage is SparseCompressedRowMatrixStorage<double> csr
                    ? Ingest(SparseFormat.Csr, csr.RowCount, csr.ColumnCount, csr.RowPointers, csr.ColumnIndices, csr.Values)
                    : TensorReason.RowMissing.Fail<SparseCompressedRowMatrixStorage<double>>("mtx-coordinate");
            }).Run().Bind(static inner => inner)
            : TensorReason.RowMissing.Fail<SparseCompressedRowMatrixStorage<double>>("mtx-source");

    internal static Fin<Unit> WriteMtx(Stream staged, FactoredOp op) =>
        Try.lift(() => {
            using StreamWriter writer = new(staged, Encoding.ASCII, leaveOpen: true);
            MathNet.Numerics.LinearAlgebra.Double.SparseMatrix matrix =
                MathNet.Numerics.LinearAlgebra.Double.SparseMatrix.OfIndexed(
                    op.A.RowCount, op.A.ColumnCount, op.A.EnumerateIndexedAsValueTuples());
            MatrixMarketWriter.WriteMatrix(writer, matrix);
            writer.Flush();
            return Fin.Succ(unit);
        }).Run().Bind(static inner => inner);

    internal readonly record struct SparseArchiveRead(
        int Rows, int Columns, SparseFormat Format, string Kind, long Ordering, long Fill,
        double Frobenius, bool Symmetric, int[] Permutation, int[] Major, int[] Minor, double[] Payload);

    internal static Fin<SparseExchange> ReadArchive(ExchangeSource source) =>
        source is not ExchangeSource.Archived archived
            ? TensorReason.RowMissing.Fail<SparseExchange>("hdf5-source")
            : from group in archived.Handle.Group("A")
              from values in archived.Handle.Dataset("A/values")
              from pointers in archived.Handle.Dataset("A/indptr")
              from indices in archived.Handle.Dataset("A/indices")
              from permutation in archived.Handle.Dataset("A/permutation")
              from read in Try.lift(() => {
                  long[] shape = group.Attribute("shape").Read<long[]>();
                  if (shape is not [> 0L, > 0L]) { return TensorReason.RowMissing.Fail<SparseArchiveRead>("hdf5-sparse-shape"); }
                  string wireFormat = group.Attribute("format").Read<string>();
                  if (toSeq(SparseFormat.Items).Find(row => row.PointerForm && StringComparer.Ordinal.Equals(row.Key, wireFormat)) is not { Case: SparseFormat format }) {
                      return TensorReason.RowMissing.Fail<SparseArchiveRead>($"hdf5-sparse-format:{wireFormat}");
                  }
                  int major = checked((int)(format.MajorIsRow ? shape[0] : shape[1]));
                  ulong[] valueShape = values.Space.Dimensions;
                  ulong[] pointerShape = pointers.Space.Dimensions;
                  ulong[] indexShape = indices.Space.Dimensions;
                  ulong[] permutationShape = permutation.Space.Dimensions;
                  if (valueShape.Length != 1 || indexShape.Length != 1 || indexShape[0] != valueShape[0]
                      || pointerShape.Length != 1 || pointerShape[0] != (ulong)major + 1UL
                      || permutationShape.Length != 1 || permutationShape[0] != (ulong)shape[1]) {
                      return TensorReason.RowMissing.Fail<SparseArchiveRead>("hdf5-sparse-extents");
                  }
                  int nonZeros = checked((int)values.Space.Dimensions[0]);
                  int[] indptr = new int[major + 1];
                  int[] minor = new int[nonZeros];
                  double[] payload = new double[nonZeros];
                  int[] applied = new int[checked((int)permutation.Space.Dimensions[0])];
                  pointers.Read<int>(archived.Handle.Access, indptr.AsSpan(), new HyperslabSelection(0, (ulong)indptr.Length));
                  indices.Read<int>(archived.Handle.Access, minor.AsSpan(), new HyperslabSelection(0, (ulong)nonZeros));
                  values.Read<double>(archived.Handle.Access, payload.AsSpan(), new HyperslabSelection(0, (ulong)nonZeros));
                  permutation.Read<int>(archived.Handle.Access, applied.AsSpan(), new HyperslabSelection(0, (ulong)applied.Length));
                  return Fin.Succ(new SparseArchiveRead(Rows: checked((int)shape[0]), Columns: checked((int)shape[1]),
                      Format: format, Kind: group.Attribute("kind").Read<string>(),
                      Ordering: group.Attribute("ordering").Read<long>(), Fill: group.Attribute("fill").Read<long>(),
                      Frobenius: group.Attribute("frobenius").Read<double>(), Symmetric: group.Attribute("symmetric").Read<bool>(),
                      Permutation: applied, Major: indptr, Minor: minor, Payload: payload));
              }).Run().Bind(static inner => inner)
              from meta in AdmitsArchive(read)
              from storage in Ingest(read.Format, read.Rows, read.Columns, read.Major, read.Minor, read.Payload)
              select (SparseExchange)new SparseExchange.Archive(storage, meta);

    static Fin<SparseArchiveMeta> AdmitsArchive(SparseArchiveRead read) {
        Option<FactorKind> kind = toSeq(FactorKind.Items).Find(row => StringComparer.Ordinal.Equals(row.Key, read.Kind));
        bool held = read.Format.PointerForm
            && read.Ordering is >= 0L and <= 3L
            && read.Fill >= 0L
            && double.IsFinite(read.Frobenius) && read.Frobenius >= 0.0
            && kind.Exists(row => row.Symmetric == read.Symmetric)
            && read.Permutation.Length == read.Columns
            && read.Permutation.All(index => index >= 0 && index < read.Columns)
            && read.Permutation.ToFrozenSet().Count == read.Columns;
        Error refusal = TensorReason.ShapeMismatch.Fault("hdf5-sparse-roster", read.Format.Key, read.Kind, $"{read.Rows}x{read.Columns}");
        return held
            ? kind.ToFin(refusal).Map(factor => new SparseArchiveMeta(
                factor, (ColumnOrdering)read.Ordering, read.Fill, read.Frobenius, read.Permutation))
            : Fin.Fail<SparseArchiveMeta>(refusal);
    }

    internal static Fin<Unit> WriteArchive(Stream staged, FactoredOp op, HdfArchivePolicy policy) =>
        op.A.NonZerosCount > int.MaxValue || op.A.ColumnPointers.Length > int.MaxValue
            ? TensorReason.ExtentOverflow.Fail<Unit>("hdf5-sparse-int32", $"nnz={op.A.NonZerosCount}")
            : (from pointers in VectorGrid(op.A.ColumnPointers.Length)
               from indices in VectorGrid(op.A.RowIndices.Length)
               from values in VectorGrid(op.A.Values.Length)
               from permutation in VectorGrid(op.Permutation.Length)
               select (Pointers: new ArchiveSlot<int>("A/indptr", pointers), Indices: new ArchiveSlot<int>("A/indices", indices),
                       Values: new ArchiveSlot<double>("A/values", values), Permutation: new ArchiveSlot<int>("A/permutation", permutation)))
                .Bind(slots => ArchiveSession.Write(
                    staged, policy,
                    Seq<IArchiveSlot>(slots.Pointers, slots.Indices, slots.Values, slots.Permutation),
                    Seq<(string Key, ArchiveAttribute Value)>(),
                    session =>
                        IO.pure(from pointers in session.Cursor(slots.Pointers)
                                from _1 in pointers.WriteAll(op.A.ColumnPointers)
                                from indices in session.Cursor(slots.Indices)
                                from _2 in indices.WriteAll(op.A.RowIndices)
                                from values in session.Cursor(slots.Values)
                                from _3 in values.WriteAll(op.A.Values)
                                from permutation in session.Cursor(slots.Permutation)
                                from _4 in permutation.WriteAll(op.Permutation)
                                select unit),
                    Seq(new ArchiveAttributes("A", Seq(
                        ("shape", (ArchiveAttribute)new ArchiveAttribute.WholeVector(new long[] { op.A.RowCount, op.A.ColumnCount })),
                        ("format", new ArchiveAttribute.Text("csc")),
                        ("kind", new ArchiveAttribute.Text()),
                        ("ordering", new ArchiveAttribute.Whole((int)op.Ordering)),
                        ("fill", new ArchiveAttribute.Whole(op.Fill)),
                        ("frobenius", new ArchiveAttribute.Real(op.FrobeniusNorm)),
                        ("symmetric", new ArchiveAttribute.Flag(op.Kind.Symmetric))))))
                    .Run());

    static Validation<Error, ChunkGrid> VectorGrid(int length) =>
        ChunkGrid.Seat([(ulong)length], [(uint)Math.Min(length, ExchangeChunk)]);

    const int ExchangeChunk = 1 << 16;

    public static IO<Fin<RecyclableMemoryStream>> Emit(SparseContainer container, StreamPool pool, CorrelationId correlation, FactoredOp op, HdfArchivePolicy policy) =>
        IO.pure(pool.Get(correlation, new StreamGrant.Open())).Bind(opened => opened.Match(
            Succ: staged => IO.lift(() => staged).Bracket(
                Use: held => IO.lift(() => container.Write(held, op, policy).Map(_ => { held.Position = 0; return held; })),
                Catch: static error => IO.pure(Fin<RecyclableMemoryStream>.Fail(error)),
                Fin: static held => IO.lift(() => { held.Dispose(); return unit; })),
            Fail: static error => IO.pure(Fin<RecyclableMemoryStream>.Fail(error))));

    // --- [FACTORIZATION] ---------------------------------------------------------------
    public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor) {
        if (!double.IsFinite(pivotTol) || pivotTol < 0.0 || pivotTol > 1.0 || !double.IsFinite(dropFloor) || dropFloor < 0.0) {
            return TensorReason.PolicyInvalid.Fail<FactoredOp>("sparse-factor-policy", $"pivot={pivotTol:e3}", $"drop={dropFloor:e3}");
        }
        CompressedColumnStorage<double> csc = ToColumnStorage(csr);
        csc.DropZeros(dropFloor);
        return Helper.ValidateStorage(csc, strict: true)
            ? Structural(csc, kind).Bind(_ => Lift(() => Build(csc, kind, ordering, pivotTol)))
            : TensorReason.PermutationInvalid.Fail<FactoredOp>("sparse-storage-invalid", kind.Key, $"{csc.RowCount}x{csc.ColumnCount}");
    }

    static Fin<Unit> Structural(CompressedColumnStorage<double> csc, FactorKind kind) {
        int order = Math.Min(csc.RowCount, csc.ColumnCount);
        return Optional(DulmageMendelsohn.Generate(csc))
            .ToFin(TensorReason.StructuralRank.Fault("sparse-structural-matching", kind.Key, $"{csc.RowCount}x{csc.ColumnCount}"))
            .Bind(dm => dm.StructuralRank == order
                ? Fin.Succ(unit)
                : TensorReason.StructuralRank.Fail<Unit>("sparse-structural-rank", kind.Key,
                    StructuralDeficiency.Of(dm, csc, order).Witness));
    }

    public static Fin<SolveOutcome<Vector<double>>> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy) =>
        csr.RowCount != csr.ColumnCount || rhs.Length != csr.RowCount
            ? TensorReason.ShapeMismatch.Fail<SolveOutcome<Vector<double>>>("iterative-shape", $"{csr.RowCount}x{csr.ColumnCount}", $"rhs={rhs.Length}")
            : policy.Admits.ToFin().Bind(_ => Try.lift(() => {
                SparseMatrix matrix = new(csr);
                Vector<double> b = Vector<double>.Build.DenseOfArray(rhs);
                Vector<double> x = Vector<double>.Build.Dense(rhs.Length);
                IPreconditioner<double> pre = method.Ladder ? policy.Preconditioner() : new UnitPreconditioner<double>();
                if (method.Ladder) { pre.Initialize(matrix); }
                int iterations = 0;
                IterationStatus verdict = matrix.TrySolveIterative(b, x, method.Solver(policy), policy.Iterator(_ => iterations++), pre);
                double residual = (matrix.Multiply(x) - b).L2Norm() / Math.Max(1.0, b.L2Norm());
                return Fin.Succ((Verdict: verdict, Field: x, Residual: residual, Iterations: iterations));
            }).Run().Bind(static inner => inner))
            .Bind(run => Partition(run.Verdict, run.Field, run.Residual, run.Iterations, policy.MaxIterations));

    static Fin<SolveOutcome<Vector<double>>> Partition(IterationStatus verdict, Vector<double> x, double residual, int iterations, int budget) =>
        verdict switch {
            IterationStatus.Converged => Fin.Succ(new SolveOutcome<Vector<double>>(
                x, DenseSubstrate.Managed, residual, new SolveTermination.Converged(), iterations)),
            IterationStatus.StoppedWithoutConvergence => Fin.Succ(new SolveOutcome<Vector<double>>(
                x, DenseSubstrate.Managed, residual, new SolveTermination.Exhausted(budget), iterations)),
            var hard => TensorReason.WitnessFail.Fail<SolveOutcome<Vector<double>>>("iterative-terminal", hard.ToString(), $"r={residual:e3}"),
        };

    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit, double pivotTol) =>
        Admit(op, edit).Bind(admitted => admitted.Switch(
            pin: pin => Refactor(Pinned(op.A, pin.Node), pivotTol, admitted.Regates),
            prune: prune => Refactor(Cleaned(op.A, prune.Tolerance), pivotTol, admitted.Regates),
            bump: bump => op.Kind.Rank1Edit
                ? Downdate(op, bump, pivotTol)
                : Refactor(Bumped(op.A, bump), pivotTol, admitted.Regates),
            revalue: revalue => Revalue(op, revalue.Values, pivotTol)));

    static Fin<Edit> Admit(FactoredOp op, Edit edit) =>
        edit.Switch(
            pin: pin => !op.Rectangular && (uint)pin.Node < (uint)op.A.RowCount
                ? Fin.Succ<Edit>(pin)
                : TensorReason.AxisOutOfRange.Fail<Edit>("pin-bound", pin.Node.ToString(CultureInfo.InvariantCulture), $"{op.A.RowCount}x{op.A.ColumnCount}"),
            prune: prune => double.IsFinite(prune.Tolerance) && prune.Tolerance >= 0.0
                ? Fin.Succ<Edit>(prune)
                : TensorReason.PolicyInvalid.Fail<Edit>("prune-tolerance", prune.Tolerance.ToString("e3", CultureInfo.InvariantCulture)),
            bump: bump => !op.Rectangular && bump.Sign is -1 or 1 && bump.Column.Length == op.A.RowCount && TensorPrimitives.IsFiniteAll<double>(bump.Column)
                ? Fin.Succ<Edit>(bump)
                : TensorReason.ShapeMismatch.Fail<Edit>("bump-shape", $"sign={bump.Sign}", $"column={bump.Column.Length}", $"{op.A.RowCount}x{op.A.ColumnCount}"),
            revalue: revalue => revalue.Values.Length == op.A.NonZerosCount && TensorPrimitives.IsFiniteAll<double>(revalue.Values)
                ? Fin.Succ<Edit>(revalue)
                : TensorReason.ShapeMismatch.Fail<Edit>("revalue-count", $"{revalue.Values.Length}!={op.A.NonZerosCount}"));

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
        Try.lift(() => Fin.Succ(build())).Run().Bind(static inner => inner);

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
        CoordinateStorage<double> coords = new(a.RowCount, a.ColumnCount, a.NonZerosCount + (support.Length * support.Length));
        toSeq(a.EnumerateIndexedAsValueTuples()).Iter(t => coords.At(t.row, t.column, t.value));
        toSeq(support).Iter(i => toSeq(support).Iter(j => coords.At(i, j, bump.Sign * bump.Column[i] * bump.Column[j])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

}
```

## [03]-[SPARSE_ALGEBRA]

- Owner: `SparseTensorOpFamily` `[SmartEnum<string>]` the sparse op axis carrying binary arity and a TOTAL route column on the row itself; `GemvForm` `[Union]` the held-GEMV direction-and-accumulation payload; `SparseTensorOps` the static algebra fold over the one `SparseCompressedRowMatrixStorage<double>` CSR storage, routing each op to the library that owns it, the held mat-vec to the CSparse CSC span GEMV in both directions, and arbitrary contraction to CSparse pattern construction; `EinsumPlan` the index-subscript contraction planner deriving a greedy pairwise contraction order and lowering each step to dense GEMM or sparse contract; `SparseRun` the per-op nnz-and-format witness.
- Cases: `SparseTensorOpFamily` rows spmv · spmm · sp-add · sp-scale · sp-transpose · kronecker · contract · einsum (8, every row carrying a total route); `GemvForm` cases `Forward` · `Adjoint` (2, each carrying its own α/β so the four CSparse GEMV overloads are one family); the dense lane `TensorOpFamily` rows stay the dense owner and a sparse row is never aliased onto a dense key.
- Entry: `Apply` owns sparse arithmetic, `Spmv(CompressedColumnStorage<double>, GemvForm, ReadOnlySpan<double>, Span<double>)` owns the held mat-vec in both directions into a caller-owned destination, and `Contract` folds one unified dense-or-sparse operand store. `Contract(EinsumPlan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>>, ShardDispatch, DenseSubstrate)` returns `IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>>`, so local and distributed contraction share one entry while cache, blob, and RPC work stays deferred.
- Auto: each `SparseTensorOpFamily` row carries its route DIRECTLY — spmm binds `Multiply`, sp-add `Add`, sp-scale scalar `Multiply`, sp-transpose `Transpose`, kronecker `KroneckerProduct`, and spmv, contract, and einsum each bind the route that genuinely serves them — so row admission and dispatch cannot drift and no row answers a fall-through miss for an axis its own family declares; spmv is the held-operator vector GEMV whose direction and α/β ride the `GemvForm` payload, never a `transpose` flag beside the operand; `EinsumPlan.Of` parses the subscript spec, derives the greedy pairwise order, and folds the merged shape into the surviving slot the plan itself names; an all-dense operand vector under a NATIVE `DenseSubstrate` argument collapses the plan to one `AtenDense.Einsum` call, otherwise each working pair routes from its current sparsity and writes its intermediate back over the surviving index.
- Result: sparse operations return their native matrix result; its storage enumerator yields the resulting non-zero count when a caller needs it. Dense lowering returns the dense matrix directly.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new sparse op is one `SparseTensorOpFamily` row carrying its route; a new GEMV modality is one `GemvForm` case with its arm on the flattened weight pattern, never a direction-named sibling entrypoint; a new contraction-order heuristic is one column on `EinsumPlan`; zero new surface.
- Boundary: `SparseTensorOps` is the sparse parallel of the dense `Tensor/dispatch#KERNEL_DISPATCH` `TensorOps`/`TensorOpFamily` split and operates on the one `SparseCompressedRowMatrixStorage<double>` the `[02]-[SPARSE_SOLVE]` ingestion fold owns — a parallel sparse-tensor type is the deleted form; the fold routes each op to the library that owns it — the inherited `Matrix<double>` `Multiply(Matrix)`/`Add(Matrix)`/`Transpose()`/scalar `Multiply(double)` for SpMM/add/scale/transpose (MathNet sparse owns these), `Matrix<double>.KroneckerProduct(Matrix<double>)` for the tensor-product element space (the sparse operands build through the `new SparseMatrix(storage)` ctor and the Kronecker rides the inherited `Matrix<T>` member, never a `SparseMatrix.OfStorage` phantom), and CSparse `CoordinateStorage` pattern construction for an arbitrary index `contract` MathNet does not own — a hand-rolled triple loop beside `Matrix<double>.Multiply` or a managed Kronecker beside `KroneckerProduct` is the named reimplementation defect. Every row's route is TOTAL: a row whose concern another owner serves binds that owner rather than an absent kernel and a `<sparse-op-miss>` fall-through, because a family whose own `Apply` axis always fails for three of eight rows publishes an entrypoint no caller can use for them and hides which three. The `EinsumPlan` contraction order is the named statement boundary (the greedy heuristic walks a mutable cost array) bounded by the binary-pairwise reduction so each lowered step is exactly one settled `MatMul`/`Contract` row, never an n-ary kernel that bypasses the `KernelLowering` table, and the contraction-order optimization is exponential in operand count so the planner uses a greedy heuristic bounded by intermediate-size cost rather than an exhaustive search; the multi-operand contraction threads each intermediate back into one unified working operand store keyed by the surviving `Left` index (dense `Left` | sparse-CSR `Right`, the route decided at execution from the working operands' real sparsity) so a 3+-operand einsum chains correctly, and the plan publishes that surviving slot as its OWN column rather than leaving every consumer to re-derive it from a tree-emptiness test at the fold's edge. The sparse SpMV `Solver/contract#SOLVE_REQUEST` Newton residual and every adjoint leg consume `SparseTensorOps.Spmv` over one held `CompressedColumnStorage<double>` (finalized once through `SparseOps.ToColumnStorage`, or read straight off `FactoredOp.A`) rather than re-wrapping the storage per call — the adjoint arm binds CSparse `TransposeMultiply`, so a gradient of a residual, a normal-equation assembly, and a PCE design-matrix transpose never allocate a second CSC through `A.Transpose()`, and the accumulating `αA'x + βy` arm carries the iterative adjoint sweep with no temporary; the caller-owned destination and the span overloads keep the whole GEMV path allocation-free where a `Vector<double>` round trip staged three arrays per iterate; the sparse `contract` feeds the einsum planner, the `Tensor/dispatch#EQUIVALENCE_INTEROP` colored Jacobian assembles as sparse contractions over the `contract` row, and the `Spmv`/`Spmm` rows stay CPU-lowered — the `Tensor/dispatch#DEVICE_KERNELS` registry carries no sparse shader row and the device path is never a phantom mapping.

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseTensorOpFamily {
    public static readonly SparseTensorOpFamily Spmv = new("spmv", binary: false, route: SparseRoute.HeldGemv);
    public static readonly SparseTensorOpFamily Spmm = new("spmm", binary: true, route: SparseRoute.MathNet(static (a, b, _) => (SparseMatrix)a.Multiply(b)));
    public static readonly SparseTensorOpFamily SpAdd = new("sp-add", binary: true, route: SparseRoute.MathNet(static (a, b, _) => (SparseMatrix)a.Add(b)));
    public static readonly SparseTensorOpFamily SpScale = new("sp-scale", binary: false, route: SparseRoute.Scalar(static (a, scalar) => (SparseMatrix)a.Multiply(scalar)));
    public static readonly SparseTensorOpFamily SpTranspose = new("sp-transpose", binary: false, route: SparseRoute.Unary(static a => (SparseMatrix)a.Transpose()));
    public static readonly SparseTensorOpFamily Kronecker = new("kronecker", binary: true, route: SparseRoute.MathNet(static (a, b, _) => (SparseMatrix)a.KroneckerProduct(b)));
    public static readonly SparseTensorOpFamily Contract = new("contract", binary: true, route: SparseRoute.Pattern);
    public static readonly SparseTensorOpFamily Einsum = new("einsum", binary: true, route: SparseRoute.Pattern);

    public bool Binary { get; }

    public SparseRoute Route { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SparseRoute {
    private SparseRoute() { }

    public sealed record Binary(Func<SparseMatrix, SparseMatrix, double, SparseMatrix> Kernel) : SparseRoute;
    public sealed record UnaryOp(Func<SparseMatrix, SparseMatrix> Kernel) : SparseRoute;
    public sealed record ScalarOp(Func<SparseMatrix, double, SparseMatrix> Kernel) : SparseRoute;
    public sealed record HeldGemvOp : SparseRoute;
    public sealed record PatternOp : SparseRoute;

    public static SparseRoute MathNet(Func<SparseMatrix, SparseMatrix, double, SparseMatrix> kernel) => new Binary(kernel);
    public static SparseRoute Unary(Func<SparseMatrix, SparseMatrix> kernel) => new UnaryOp(kernel);
    public static SparseRoute Scalar(Func<SparseMatrix, double, SparseMatrix> kernel) => new ScalarOp(kernel);
    public static readonly SparseRoute HeldGemv = new HeldGemvOp();
    public static readonly SparseRoute Pattern = new PatternOp();
}

public readonly record struct SparseRun(string Op, Option<int> Nnz, int Rows, int Columns, string Route);

[Union]
public abstract partial record GemvForm {
    private GemvForm() { }

    public sealed record Forward(double Alpha, double Beta) : GemvForm;
    public sealed record Adjoint(double Alpha, double Beta) : GemvForm;

    public static GemvForm Apply => new Forward(1.0, 0.0);
    public static GemvForm Transposed => new Adjoint(1.0, 0.0);

    public (bool Transposed, double Alpha, double Beta) Weights => Switch(
        forward: static f => (false, f.Alpha, f.Beta),
        adjoint: static a => (true, a.Alpha, a.Beta));

    public string Key => Weights is var w && w.Alpha == 1.0 && w.Beta == 0.0
        ? w.Transposed ? "gemv-t" : "gemv"
        : w.Transposed ? "gemv-t-axpby" : "gemv-axpby";
}

public sealed record EinsumPlan(Seq<string> OperandSubscripts, string OutputSubscript, Seq<(int Left, int Right, string Subscripts)> Tree) {
    public bool MatrixChain =>
        OperandSubscripts.Count >= 2
        && OperandSubscripts.ForAll(static symbols => symbols.Length == 2)
        && OutputSubscript.Length == 2
        && OutputSubscript[0] == OperandSubscripts[0][0]
        && OutputSubscript[1] == OperandSubscripts[^1][1]
        && toSeq(Enumerable.Range(1, OperandSubscripts.Count - 1)).ForAll(index => OperandSubscripts[index - 1][1] == OperandSubscripts[index][0]);

    public int SurvivingSlot => Tree.IsEmpty ? 0 : Tree[^1].Left;

    public static Fin<EinsumPlan> Of(string spec, Seq<(int Rows, int Columns, bool Sparse)> shapes) {
        string[] sides = spec.Split("->", StringSplitOptions.TrimEntries);
        if (sides.Length != 2) { return TensorReason.ShapeMismatch.Fail<EinsumPlan>("einsum-spec", spec); }
        Seq<string> operands = toSeq(sides[0].Split(',', StringSplitOptions.TrimEntries));
        return operands.Count == shapes.Count
            ? Fin.Succ(new EinsumPlan(operands, sides[1], GreedyOrder(operands, shapes)))
            : TensorReason.ShapeMismatch.Fail<EinsumPlan>("einsum-operand-arity", $"{operands.Count}!={shapes.Count}");
    }

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
        op.Route.Switch(
            state: (Left: left, Right: right, Scalar: scalar),
            binary: static (s, route) => s.Right.ToFin(TensorReason.EmptyOperand.Fault("sparse-missing-rhs"))
                .Map(rhs => Storage(route.Kernel(new SparseMatrix(s.Left), new SparseMatrix(rhs), s.Scalar))),
            unaryOp: static (s, route) => Fin.Succ(Storage(route.Kernel(new SparseMatrix(s.Left)))),
            scalarOp: static (s, route) => Fin.Succ(Storage(route.Kernel(new SparseMatrix(s.Left), s.Scalar))),
            heldGemvOp: static (s, _) => TensorReason.OperandDomainMiss.Fail<SparseCompressedRowMatrixStorage<double>>(
                "sparse-held-gemv"),
            patternOp: static (s, _) => s.Right.ToFin(TensorReason.EmptyOperand.Fault("sparse-missing-rhs"))
                .Bind(rhs => ContractPair(s.Left, rhs)));

    static SparseCompressedRowMatrixStorage<double> Storage(SparseMatrix result) =>
        (SparseCompressedRowMatrixStorage<double>)result.Storage;

    public static Fin<Unit> Spmv(CompressedColumnStorage<double> a, GemvForm form, ReadOnlySpan<double> x, Span<double> y) =>
        (form.Weights.Transposed ? (Source: a.RowCount, Sink: a.ColumnCount) : (Source: a.ColumnCount, Sink: a.RowCount)) is var shape
        && (x.Length != shape.Source || y.Length != shape.Sink)
            ? TensorReason.ShapeMismatch.Fail<Unit>("spmv-dim", form.Key, $"x={x.Length}!={shape.Source}", $"y={y.Length}!={shape.Sink}")
            : Applied(a, form, x, y);

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

    public static IO<Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)>> Contract(EinsumPlan plan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> operands, ShardDispatch dispatch, DenseSubstrate substrate) =>
        operands.ForAll(static op => op.IsLeft) && substrate.Native
            ? IO.pure((plan.MatrixChain
                ? AtenDense.MultiDot(operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr))))
                : AtenDense.Einsum($"{string.Join(',', plan.OperandSubscripts)}->{plan.OutputSubscript}", operands.Map(static op => op.Match(Left: identity, Right: static csr => (Matrix<double>)new SparseMatrix(csr)))))
                .Map(dense => ((SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage,
                    Seq(new SparseRun(plan.OutputSubscript, None, dense.RowCount, dense.ColumnCount, plan.MatrixChain ? "aten-multi-dot" : "aten-einsum")))))
            : plan.Tree.Fold(
                IO.pure(Fin.Succ((Work: toHashMap(operands.Map(static (op, i) => (i))), Steps: Seq<SparseRun>()))),
                (effect, step) => effect.Bind(state => state.Match(
                    Succ: held => Step(held.Work, step, plan.OutputSubscript, dispatch).Map(next => next.Map(row => (row.Work, held.Steps.Add(row.Run)))),
                    Fail: static error => IO.pure(Fin.Fail<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, Seq<SparseRun> Steps)>(error)))))
            .Map(state => state.Map(held => (CsrOf(held.Work[plan.SurvivingSlot]), held.Steps)));

    static IO<Fin<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, SparseRun Run)>> Step(
        HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> work, (int Left, int Right, string Subscripts) step, string output, ShardDispatch dispatch) {
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> l = work[step.Left];
        Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> r = work[step.Right];
        return l.IsLeft && r.IsLeft
            ? KernelLowering.Lower(TensorOpFamily.MatMul, new LoweringOperands.Pair(DenseOf(l), DenseOf(r)), dispatch)
                .Map(result => result.Map(outcome => outcome.Solution).Map(dense => (work.AddOrUpdate(step.Left, Left<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(dense)).Remove(step.Right),
                    new SparseRun(output, None, dense.RowCount, dense.ColumnCount, "dense"))))
            : IO.pure(ContractPair(CsrOf(l), CsrOf(r))
                .Map(csr => (work.AddOrUpdate(step.Left, Right<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(csr)).Remove(step.Right),
                    new SparseRun(output, Some(csr.ValueCount), csr.RowCount, csr.ColumnCount, "sparse"))));
    }

    static Matrix<double> DenseOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => dense, Right: static csr => new SparseMatrix(csr));

    static SparseCompressedRowMatrixStorage<double> CsrOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => (SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage, Right: static csr => csr);

    static Fin<SparseCompressedRowMatrixStorage<double>> ContractPair(SparseCompressedRowMatrixStorage<double> left, SparseCompressedRowMatrixStorage<double> right) =>
        left.ColumnCount == right.RowCount
            ? Fin.Succ((SparseCompressedRowMatrixStorage<double>)((SparseMatrix)new SparseMatrix(left).Multiply(new SparseMatrix(right))).Storage)
            : TensorReason.ShapeMismatch.Fail<SparseCompressedRowMatrixStorage<double>>("contract-inner-dim", $"{left.ColumnCount}!={right.RowCount}");
}
```

## [04]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, with the `ShardDispatch` local-or-farm axis the dense GEMM reads (its `Farm` case pairing the replayable `ShardPlan` block decomposition with the `ShardContext` ambient that plan runs against), and the `ProveGemm` GEMM-vs-naive-reference proof the `Tensor/dispatch#EQUIVALENCE_INTEROP` equivalence law's matrix arm reads; `LoweringOperands` the `[Union]` operand shape a lowering takes.
- Cases: `LoweringOperands` cases `Pair(Matrix, Matrix)` · `Windowed(Matrix Input, Matrix Kernel, ConvWindow Window)` (2 — the operand shape IS the discriminant three predicates once reported); `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor owning both flat index tables and its once-derived output extents; `ShardDispatch` cases `Local` (nullary — the in-process `Matrix<double>.Multiply` leaf, carrying nothing) · `Farm(ShardPlan Plan, ShardContext Context)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardPlan(int Tile, LinearProvider Provider, FactorizationKind Kind, Duration Deadline)` the replayable block decomposition the `Farm` case carries; `ShardContext` the composition-supplied run carrier; `ShardBlock` the private per-block join carrier.
- Entry: `Lower(TensorOpFamily row, LoweringOperands operands, ShardDispatch dispatch)` is the ONE lowering entry and returns `IO<Fin<Matrix<double>>>`; `ShardDispatch.Local` returns the in-process GEMM, while `ShardDispatch.Farm` composes cache, blob, and RPC effects off the pair it carries and joins the row blocks into the same matrix result. `Pool` remains pure over its span and `ProveGemm(int order, long seed)` reads the same local kernel.
- Auto: `Lower` reads the row's OWN `TensorArity` column to route — `TensorArity.Matrix` takes the GEMM or the im2col-then-GEMM by which operand case arrived, `TensorArity.Pool` folds through the dispatch lane's own `PoolReducers<T>` rows, and every other arity refuses by name — so the three frozen row sets and the three predicates that mirrored that column are deleted. `ShardDispatch.Farm` traverses row blocks on `IO<Fin<ShardBlock>>`, so lookup, payload fetch/store, publish, and RPC dial remain one deferred algebra; `Fin` aborts the join after effects yield typed results.
- Result: matrix and convolution lowering return the lowered `Matrix<double>` directly. Farm execution keeps block offsets only inside the join.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Grpc.Core.Api, Google.Protobuf, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `ContentHash` the one digest owner), Rasm.Persistence (project), BCL inbox
- Growth: a new lowering is one `TensorOpFamily` row carrying its `TensorArity` column, which routes here with no table edit; a new operand shape is one `LoweringOperands` case the total `Switch` breaks on; a new shard topology is one `ShardDispatch` case carrying whatever that topology genuinely needs; a new fan-out knob is one `ShardPlan` column and a new fan-out ambient one `ShardContext` column; a new matrix row inherits `ProveGemm` with no new proof surface; zero new surface.
- Boundary — routing: the row's `Tensor/vocabulary#OPERATION_TABLE` `TensorArity` column is the authority a lowering routes on, so `ConvRank`, `PoolRows`, and `MatrixRows` — three frozen sets mirroring one column — and `Lowers`, `IsMatrix`, and `NeedsWindow` — three predicates reporting them — are all deleted. Convolution RANK likewise rides the request rather than three row names: the vocabulary carries one `Conv` row and the `ConvWindow` states its own rank, so a rank the window and the operand disagree on refuses by name at one site. A `Windowed` operand handed to a row whose arity is not `Matrix`, and a `Pair` operand handed to the convolution row, are both refusals the operand union makes legible instead of a predicate a caller had to consult first.
- Boundary — the pairing is STRUCTURAL: `ShardDispatch` is the one lowering argument, its `Farm` case holding the `ShardPlan` and the `ShardContext` together so a block decomposition without the transport that runs it is unrepresentable, and its nullary `Local` case carrying nothing so an in-process lowering spells `Local` and names no transport. `ShardPlan` stays a separate record inside that case because it alone is REPLAYABLE — a value a caller writes down, keys a plan cache on, and compares across runs — where a live gRPC stub, a reuse index, object-store closures, and a cancellation token are none of those; fusing the two made the decomposition uncomparable and unserializable, and threading them as a loose pair forced every local call site to name transport it never dials. It carries `[Equatable]` because a plan cache compares it by VALUE and record equality over its four columns is exactly that comparison stated once.
- Boundary — `Im2Col` computes both index tables ONCE per lowering off the `ConvWindow` — the per-tap axis offsets with dilation and padding folded in, and the per-position strided origins — so the gather cell is pure index math over shared heap tables and allocates nothing per position, and the output extents read once into the descriptor rather than re-deriving the whole vector on every property access three call sites make. The patch plane is a `Span2D<double>` over one pooled `MemoryOwner<double>` rent carrying its `Tensor/memory#ALLOCATION_AXIS` grant, because a `[positions, patchWidth]` plane sized by a caller's convolution geometry is staging, not kernel-interior scratch; the gather addresses it by row, where a `double[,]` indexer recomputed the same offset arithmetic on every one of the millions of taps. Out-of-range taps pad by branch inside the same walk rather than through a second bounds pass, each `(position, channel)` owns one disjoint patch-row run, and one provider GEMM then carries `MatMul` proof evidence for every convolution row.
- Boundary — both operands ride raw column-major float64 bytes through `UnsafeByteOperations.UnsafeWrap`; request hashing uses pooled serialization through the suite `Rasm/Domain/identity#CONTENT_KEY` `ContentHash.Of` owner rather than a second local digest spelling, cache lookup resolves only a location, and blob custody stays on the object-store ports. `IO<Fin<T>>` composes lookup, fetch, dial, store, and publish without an interior effect run; `Fin` gates the private join target after traversal.
- Boundary — the sub-solve content address is the request bytes folded with the provider determinism tag and NOTHING else. Two row blocks carrying identical content dedup to one dialed solve, which is the whole point of the reuse index; salting the digest with the block's row offset made every block its own key, so the index never hit and the write path paid a publish per block for a table nothing read. Provider stays in the key through `SolveDedupKey`, so a cross-provider hit — bit-divergent numbers under one address — remains unrepresentable.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LoweringOperands {
    private LoweringOperands() { }

    public sealed record Pair(Matrix<double> Left, Matrix<double> Right) : LoweringOperands;
    public sealed record Windowed(Matrix<double> Input, Matrix<double> Kernel, ConvWindow Window) : LoweringOperands;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ShardBlock(int Start, Matrix<double> Solution) {
    public static ShardBlock Join(Matrix<double> target, ShardBlock block) {
        target.SetSubMatrix(block.Start, 0, block.Solution);
        return block;
    }
}

public sealed record ShardContext(
    ComputeService.ComputeServiceClient Compute,
    ModelResultIndex Reuse,
    Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload,
    Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload,
    CorrelationId Correlation,
    IClock Clock,
    CancellationToken Cancel);

[Equatable]
public sealed partial record ShardPlan(int Tile, LinearProvider Provider, FactorizationKind Kind, Duration Deadline) {
    internal Validation<Error, Unit> Admits =>
        (Tile > 0 ? Validation<Error, Unit>.Success(unit) : TensorReason.PolicyInvalid.Fault("shard-tile", Tile.ToString(CultureInfo.InvariantCulture)),
         Deadline > Duration.Zero ? Validation<Error, Unit>.Success(unit) : TensorReason.PolicyInvalid.Fault("shard-deadline", Deadline.ToString()))
            .Apply(static (_, _) => unit).As();
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShardDispatch {
    private ShardDispatch() { }

    public sealed record Local : ShardDispatch;
    public sealed record Farm(ShardPlan Plan, ShardContext Context) : ShardDispatch;

    public IO<Fin<Matrix<double>>> Lower(Matrix<double> left, Matrix<double> right) =>
        left.ColumnCount != right.RowCount
            ? IO.pure(TensorReason.ShapeMismatch.Fail<Matrix<double>>("gemm-inner-dim", $"{left.ColumnCount}!={right.RowCount}"))
            : Switch(
                state: (Left: left, Right: right),
                local: static (s, _) => IO.pure(Fin.Succ(Gemm(s.Left, s.Right))),
                farm: static (s, farm) => farm.Plan.Admits.ToFin().Match(
                    Succ: _ => Fanout(s.Left, s.Right, farm.Plan, farm.Context),
                    Fail: error => IO.pure(Fin<Matrix<double>>.Fail(error))));

    internal static Matrix<double> Gemm(Matrix<double> left, Matrix<double> right) => left.Multiply(right);

    static IO<Fin<Matrix<double>>> Fanout(Matrix<double> left, Matrix<double> right, ShardPlan plan, ShardContext context) {
        int blocks = (left.RowCount + plan.Tile - 1) / plan.Tile;
        return toSeq(Enumerable.Range(0, blocks))
            .Traverse(block => {
                int start = block * plan.Tile;
                int height = Math.Min(plan.Tile, left.RowCount - start);
                return SubSolve(left.SubMatrix(start, height, 0, left.ColumnCount), right, start, height, plan, context);
            })
            .Map(traversed => traversed.Traverse(identity).Map(rows => {
                Matrix<double> target = rows.Fold(
                    Matrix<double>.Build.Dense(left.RowCount, right.ColumnCount),
                    static (held, block) => { ShardBlock.Join(held, block); return held; });
                return target;
            }).As())
            .As();
    }

    static IO<Fin<ShardBlock>> SubSolve(Matrix<double> rowBlock, Matrix<double> right, int start, int height, ShardPlan plan, ShardContext context) {
        Memory<byte> matrix = rowBlock.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        Memory<byte> rhs = right.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        SolveRequest request = new() {
            Matrix = UnsafeByteOperations.UnsafeWrap(matrix),
            Rhs = UnsafeByteOperations.UnsafeWrap(rhs),
            FactorizationKind = plan.Kind.Key,
            SparseFormat = string.Empty,
            ShardTile = plan.Tile,
        };
        ContentHash address = plan.Provider.SolveDedupKey(Digest(request));
        return context.Reuse.Lookup(address).Bind(row => row.Match(
            Some: cached => context.FetchPayload(cached.Placement).Bind(bytes => bytes.Match(
                Some: payload => IO.pure(Try.lift(() => Fin.Succ(SolveResponse.Parser.ParseFrom(payload.Span))).Run().Bind(static inner => inner)
                    .Bind(response => Materialize(response, start, height, right.ColumnCount))),
                None: () => DialAndStore(plan, context, request, address, start, height, right.ColumnCount))),
            None: () => DialAndStore(plan, context, request, address, start, height, right.ColumnCount)));
    }

    static IO<Fin<ShardBlock>> DialAndStore(ShardPlan plan, ShardContext context, SolveRequest request, ContentHash address, int start, int height, int cols) =>
        Dial(plan, context, request).Bind(result => result.Match(
            Succ: response => Materialize(response, start, height, cols).Match(
                Succ: block => Store(context, plan, response, address).Map(_ => Fin.Succ(block)),
                Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))),
            Fail: static error => IO.pure(Fin.Fail<ShardBlock>(error))));

    static IO<Unit> Store(ShardContext context, ShardPlan plan, SolveResponse response, ContentHash address) =>
        IO.lift(() => {
            MemoryOwner<byte> rent = MemoryOwner<byte>.Allocate(response.CalculateSize());
            response.WriteTo(rent.Span);
            return rent;
        }).Bracket(
            Use: rent => from location in context.StorePayload(rent.Memory)
                         from _ in context.Reuse.Publish(new ModelResultRow(address, location, plan.Provider.DeterminismTag, context.Clock.GetCurrentInstant()))
                         select unit,
            Fin: static rent => IO.lift(() => { rent.Dispose(); return unit; }));

    static ContentHash Digest(SolveRequest request) {
        int width = request.CalculateSize();
        using SpanOwner<byte> rent = SpanOwner<byte>.Allocate(width);
        request.WriteTo(rent.Span);
        return ContentHash.Of(rent.Span);
    }

    static Fin<ShardBlock> Materialize(SolveResponse response, int start, int height, int defaultCols) {
        int cols = response.Cols == 0 ? defaultCols : (int)response.Cols;
        if (cols <= 0 || response.Solution.Length != (long)height * cols * sizeof(double)) {
            return TensorReason.ShapeMismatch.Fail<ShardBlock>("solve-shape", $"height={height}", $"cols={cols}", $"bytes={response.Solution.Length}");
        }
        return Try.lift(() => Fin.Succ(new ShardBlock(start, Restore(response, height, cols)))).Run().Bind(static inner => inner);
    }

    static IO<Fin<SolveResponse>> Dial(ShardPlan plan, ShardContext context, SolveRequest request) =>
        IO.lift(() => RpcEdge.Rpc(() => context.Compute.Solve(request, Options(plan, context))))
            .RetryWhile(
                Schedule.exponential(TimeSpan.FromMilliseconds(20)) | Schedule.maxCumulativeDelay(plan.Deadline.ToTimeSpan()),
                static outcome => outcome.IsFail && outcome.Match(Succ: static _ => false, Fail: static error => error is ComputeFault.EndpointUnreachable));

    static CallOptions Options(ShardPlan plan, ShardContext context) =>
        new CallOptions(new Metadata { { "rasm-correlation", context.Correlation.ToString() } })
            .WithDeadline(context.Clock.GetCurrentInstant().Plus(plan.Deadline).ToDateTimeUtc())
            .WithCancellationToken(context.Cancel);

    static Matrix<double> Restore(SolveResponse response, int rows, int cols) =>
        Matrix<double>.Build.Dense(rows, cols, MemoryMarshal.Cast<byte, double>(response.Solution.Span).ToArray());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial) {
    public int Rank => Kernel.Length;
    public int KernelVolume => Kernel.Aggregate(1, static (acc, extent) => acc * extent);
    public int PatchWidth => Channels * KernelVolume;

    public int[] OutputExtents { get; } =
        [.. Enumerable.Range(0, Kernel.Length).Select(axis =>
            ((Spatial[axis] + (2 * Padding[axis]) - (Dilation[axis] * (Kernel[axis] - 1)) - 1) / Stride[axis]) + 1)];

    public int OutputPositions => OutputExtents.Aggregate(1, static (acc, extent) => acc * extent);

    public int[] TapOffsets() {
        int rank = Rank, volume = KernelVolume;
        int[] taps = new int[volume * rank];
        for (int tap = 0; tap < volume; tap++) {
            int remainder = tap;
            for (int axis = rank - 1; axis >= 0; axis--) {
                taps[(tap * rank) + axis] = (remainder % Kernel[axis] * Dilation[axis]) - Padding[axis];
                remainder /= Kernel[axis];
            }
        }

        return taps;
    }

    public int[] StridedOrigins() {
        int rank = Rank;
        int[] extents = OutputExtents;
        int positions = OutputPositions;
        int[] origins = new int[positions * rank];
        for (int position = 0; position < positions; position++) {
            int remainder = position;
            for (int axis = rank - 1; axis >= 0; axis--) {
                origins[(position * rank) + axis] = remainder % extents[axis] * Stride[axis];
                remainder /= extents[axis];
            }
        }

        return origins;
    }
}

public static class KernelLowering {
    public static IO<Fin<Matrix<double>>> Lower(TensorOpFamily row, LoweringOperands operands, ShardDispatch dispatch) =>
        row.Arity != TensorArity.Matrix
            ? IO.pure(TensorReason.OperandDomainMiss.Fail<Matrix<double>>("lowering-arity", row.Key, row.Arity.Key))
            : operands.Switch(
                state: (Row: row, Dispatch: dispatch),
                pair: static (s, p) => s.Row == TensorOpFamily.MatMul
                    ? s.Dispatch.Lower(p.Left, p.Right)
                    : IO.pure(TensorReason.OperandDomainMiss.Fail<Matrix<double>>("lowering-operands", s.Row.Key, "pair")),
                windowed: static (s, w) => s.Row != TensorOpFamily.Conv
                    ? IO.pure(TensorReason.OperandDomainMiss.Fail<Matrix<double>>("lowering-operands", s.Row.Key, "windowed"))
                    : Im2Col(w.Input, w.Window).Match(
                        Succ: patch => s.Dispatch.Lower(patch, w.Kernel),
                        Fail: error => IO.pure(Fin<Matrix<double>>.Fail(error))));

    static Fin<Matrix<double>> Im2Col(Matrix<double> input, ConvWindow window) {
        int positions = window.OutputPositions, channels = window.Channels, width = window.PatchWidth;
        return AllocationClass.PooledMemory
            .Rent<double>(new AllocationRequest(CorrelationId.None, (long)positions * width * sizeof(double),
                long.MaxValue, Async: false, AllocationMode.Default, None, None, None), positions * width)
            .Map(rent => {
                using (rent.Buffer) {
                    Span2D<double> patch = rent.Buffer.Span.AsSpan2D(positions, width);
                    PatchGather gather = new(input, window.Rank, window.KernelVolume, window.Spatial, window.StridedOrigins(), window.TapOffsets(), patch);
                    ParallelHelper.For2D(0, positions, 0, channels, in gather);
                    return Matrix<double>.Build.Dense(positions, width, rent.Buffer.Span.ToArray());
                }
            });
    }

    readonly ref struct PatchGather(Matrix<double> input, int rank, int kernelVolume, int[] spatial, int[] origins, int[] taps, Span2D<double> patch) : IAction2D {
        public void Invoke(int position, int channel) {
            int originBase = position * rank;
            int column = channel * kernelVolume;
            Span<double> row = patch.GetRowSpan(position);
            for (int tap = 0; tap < kernelVolume; tap++) {
                int tapBase = tap * rank;
                int flat = 0;
                bool inside = true;
                for (int axis = 0; axis < rank; axis++) {
                    int coordinate = origins[originBase + axis] + taps[tapBase + axis];
                    if ((uint)coordinate >= (uint)spatial[axis]) { inside = false; break; }
                    flat = (flat * spatial[axis]) + coordinate;
                }

                row[column + tap] = inside ? input[channel, flat] : 0d;
            }
        }
    }

    public static Fin<double> Pool(TensorOpFamily row, ReadOnlySpan<double> window) =>
        row.Arity != TensorArity.Pool
            ? TensorReason.OperandDomainMiss.Fail<double>("pool-arity", row.Key, row.Arity.Key)
            : PoolReducers<double>.Rows.TryGetValue(row, out var reduce)
                ? Fin.Succ(reduce(window))
                : TensorReason.RowMissing.Fail<double>("pool-row", row.Key);

    public static ProofEvidence ProveGemm(int order, long seed) {
        int n = Math.Max(2, order);
        Matrix<double> left = Square(n, seed, lane: 0L), right = Square(n, seed, lane: 1L);
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

    static Matrix<double> Square(int n, long seed, long lane) =>
        Matrix<double>.Build.Dense(n, n, ProofDraw.Gaussian(n * n, seed, lane).ToArray());
}
```
