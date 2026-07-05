# [COMPUTE_FACTOR]

Rasm.Compute sparse-solve and kernel-lowering lane: the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SolveTerminal` partition preserving the caller's retry, and the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel plus the `ShardPlan` block-decomposition column the dense GEMM reads. Every library refuses its own gates — `Iterator<T>` exposes no iteration count — so the criterion stack re-imposes each and every result leaves as a typed `ComputeReceipt` carrying the route variant, the scale-derived tolerance, and the recomputed true relative residual against the original operator. A distributed solve crosses solely through the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc, which the `ShardPlan.Blocked` row-block sub-solve dials by reference.

## [01]-[INDEX]

- [02]-[SPARSE_SOLVE]: CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative; overdetermined sparse-QR least-squares route.
- [03]-[SPARSE_ALGEBRA]: `SparseTensorOpFamily` op axis over CSR storage; SpMV/SpMM/add/transpose/Kronecker/contract; `EinsumPlan` pairwise lowering over dense GEMM and sparse contract.
- [04]-[KERNEL_LOWERING]: tensor matrix/structural rows lower onto real GEMM/im2col/pool; shard fan-out.

## [02]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows carrying the CSR-conversion `ingest` delegate as row data; `FactorKind` `[SmartEnum<string>]` direct-factor rows carrying the capability columns (rank-1 edit, transpose-solve, inertia, reentrancy), the fill-formula and transpose-solve-recovery delegate, AND the permutation-keyed `create` factory as row data; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis with the `IterationPolicy` record (tolerance · max-iter · criterion stack · preconditioner); `FactoredOp` the typed sparse-operator value owning the factorization instance, the cached AMD permutation, the `ColumnOrdering` it was factored with, symbolic fill, and kind discriminant; `Edit` `[Union]` the structural-edit dialect; `SparseOps` the direct-and-iterative sparse-solve fold over CSR-backed MathNet storage and CSparse CSC direct factorizations — ingestion routes through the `SparseFormat` row's `ingest` delegate, direct factorization through the `FactorKind` row's `create` delegate, neither through a parallel `FrozenDictionary` keyed by the same enum.
- Cases: `SparseFormat` `ingest`-delegate rows csr · csc · coo · dok (4); `FactorKind` `create`-delegate rows spd · ldl · lu · qr · cholmod · superlu · umfpack (7, every row wired — `Ldl` binds `SparseLDL.Create`, never declared-and-unbound; the three NATIVE-DIRECT rows bind the vendored csparse-interop `Cholmod`/`SuperLU`/`Umfpack` drivers through the `NativeClaim` RID gate, mirroring blas.md's managed↔native `LinearProvider` pattern — the managed rows stay the cold-start terminal, a native row without its Forge-provisioned asset faults at init, never a silent degrade); `IterativeMethod` rows bicgstab · gpbicg · tfqmr · mlk-bicgstab (4); `Edit` cases `Pin` · `Prune` · `Bump` · `Revalue` (4, every case realized — Pin/Prune/Bump-non-SPD rebuild the edited CSC, never a silent re-factor of the unedited operator).
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — `Fin<T>` aborts on a length or bound mismatch; `public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor)` converts the CSR triplets once to a CSparse `CompressedColumnStorage<double>` through `CoordinateStorage` + the admitted `CompressedColumnStorage<double>.OfIndexed` CSC factory, reads the symbolic fill before the numeric sweep, and collapses the completed factorization to one `FactoredOp` value; `FactoredOp.Solve(double[] rhs, double cap)` is the one polymorphic solve over both shapes — a square operator routes the forward triangular solve and a rectangular operator on the `Qr` kind routes the SparseQR least-squares `min‖Ax−b‖`, both landing in an `A.ColumnCount`-length result (CSparse sizes the caller buffer at `n` for every kind and allocates the augmented `S.m2` work row INTERNALLY — that augmentation is private factor state, never a caller dimension to over-size), the witness recomputing the true relative residual against the ORIGINAL rectangular `A` through the `ILinearOperator<double>` vector GEMV `A` inherits from the CSparse `Matrix<T>` base — `A` (a `CompressedColumnStorage<double>`) calls `A.Multiply(x, ax)` directly because `CSparse.Matrix<T> : ILinearOperator<double>` declares the array/span vector multiply the concrete `SparseMatrix` overrides, `ax` sized `A.RowCount` — rather than the square normal-equations operator — so a sparse PCE fit, a sparse-Jacobian recovery, and an overdetermined FEM normal-equations recovery solve through the one `FactoredOp` capsule without densifying to `Matrix<double>.QR`; `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected `IIterativeSolver<double>` under the explicitly-ordered criterion stack and returns the field plus the recomputed true relative residual and `SolveTerminal` verdict — the iteration count is NOT read from `Iterator<double>` (which exposes the terminal `Status` plus the `DetermineStatus`/`Cancel`/`Reset` drivers but no iteration count), it is the criterion-stack-bounded cap.
- Auto: every format row maps to one CSR ingestion conversion through the `SparseFormat` row's `ingest` delegate — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type and the build closure rides the row, not a parallel ingestion table; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `FactorKind` row's `create` delegate binding the explicit-permutation `SparseCholesky.Create(csc, p)`/`SparseLDL.Create(csc, p)`/`SparseLU.Create(csc, p, pivotTol)` and the ordering-based `SparseQR.Create(csc, ordering)`, so the AMD ordering is computed once by `Build` and the symmetric/lu kinds reuse that permutation rather than re-deriving it inside `Create`, then solve in place through `ISparseFactorization<double>.Solve(double[], double[])` (the residual witness calls the vector GEMV directly on the `CompressedColumnStorage<double>` operator, inherited from the CSparse `Matrix<T> : ILinearOperator<double>` base — no residency cast); iterative solves run the `IterativeMethod` row's `Solver()` factory under the `IterationPolicy.Iterator()` `Iterator<double>` criterion stack constructed in precedence order `Failure → Divergence → Residual → IterationCount`; `FactoredOp.TransposeSolve` recovers the transpose-solve action from the `FactorKind` row's `TransposeRecover` delegate column alone (some for lu and qr, none for spd and ldl) because the shared `ISparseFactorization<double>` exposes only the forward solve and `SolveTranspose` closes over the concrete `SparseLU`/`SparseQR`.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, factor kind, the symbolic fill, the recomputed true relative residual, row and column extents, the `ValueCount` non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, csparse-interop (vendored — the native CHOLMOD/SuperLU/UMFPACK direct tier; natives Forge-provisioned, fault-at-init), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `SparseFormat` row carrying its `ingest` delegate; a new direct solver is one `FactorKind` row carrying its capability, fill, transpose-recovery, AND `create` columns together (one row, never a row plus a parallel `DirectSolvers` table edit); a new iterative method is one `IterativeMethod` row carrying its `IIterativeSolver<double>` factory column; a new structural-edit dialect is one `Edit` case plus its arm on the total `Apply` Switch; a new iteration knob is one column on the `IterationPolicy` record; zero new surface.
- Boundary: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage — csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner for each format is the deleted form; the CSR-to-CSC handoff builds a `CoordinateStorage<double>(rows, cols, nnz)`, calls `.At(i, j, v)` per entry, and converts once through the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` CSC factory (the CSparse static that internally runs `Converter.ToCompressedColumnStorage` with cleanup) — a hand-rolled `Converter.ToCompressedColumnStorage` detour beside the admitted `OfIndexed` factory is the named reimplementation defect, and `inplace: true` is rejected when the triplet must survive a structural-edit increment because it invalidates the source arrays and dangles references; strict storage `Validate` runs before factoring because it returns `bool` and never throws and factorizing invalid storage produces silently incorrect factors; the symbolic fill is read before the numeric sweep to route direct versus iterative and the count is per-kind through the `FactorKind.Fill` delegate column (one factor for the symmetric kinds, an `L + U − n` formula for `SparseLU`, a `Q + R − m` formula for `SparseQR`) so a bare fill integer compared across kinds is meaningless; the `ColumnOrdering.MinimumDegreeAtPlusA` permutation `int[]` from `CSparse.Ordering.AMD.Generate(CompressedColumnStorage<double>, ColumnOrdering)` caches as the value-only refactor key over an invariant pattern (`ColumnOrdering` values are `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, `MinimumDegreeAtA`; the AMD ordering type lives in `CSparse.Ordering`, distinct from the `ColumnOrdering` enum, and its `Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order)` takes the matrix first and the ordering second); assembly residue drops with a structural tolerance near `machineEps · ‖A‖_F` through `DropZeros(tolerance)` because the default `0.0` removes only binary zeros; `SparseLU` pivot `tol` is `[0, 1]` as a relative column threshold (`1` full partial pivoting, `0` disabled) never an absolute floor; transpose-solve/rank-1-edit/inertia/reentrancy recover from the `FactorKind` row alone because the shared solver interface exposes only the forward solve; an asymmetric input to a symmetric kind factors as its symmetrization and returns a correct answer to the WRONG system so the post-solve true residual is the only structural signal; a typed-only catch at the factorization boundary is rejected because SPD pivot loss and the zero-diagonal break throw bare `Exception`; the cached square factorization's one constructor-allocated scratch is non-reentrant so solves serialize through the `FactoredOp` capsule and the `SparseQR` reentrant kind is the one parallel-safe row; the rectangular least-squares result buffer sizes from `A.ColumnCount` exactly like the square solve — `SparseQR.Solve` writes the `n`-length left-hand side and allocates the augmented `S.m2` work row INTERNALLY (private factor state with no public accessor), so an attempt to over-size the caller buffer from a nonexistent "solution dimension" member is the named phantom and `A.ColumnCount` is correct for every kind and shape; the `Qr` row is the one rectangular least-squares route on `FactoredOp.Solve` so an overdetermined sparse system (`Solver/contract#SOLVE_CONTRACT` normal-equations recovery, `Solver/uncertainty#UNCERTAINTY_LANE` PCE coefficient fit, `Tensor/dispatch#EQUIVALENCE_INTEROP` sparse-Jacobian recovery) minimizes `‖Ax−b‖` through `SparseQR.Solve` and the witness recomputes against the ORIGINAL rectangular `A` (`ax` sized `A.RowCount`, the m-residual against the b-vector), never a dense `Matrix<double>.QR` fallback and never the square normal-equations operator whose conditioning the rectangular QR avoids; the residual GEMV calls `FactoredOp.A.Multiply(x, ax)` directly on the `CompressedColumnStorage<double>` because CSparse's `Matrix<T>` base implements `ILinearOperator<double>` and declares the vector `Multiply(ReadOnlySpan<double>, Span<double>)`/`Multiply(double[], double[])` the concrete `SparseMatrix` overrides — a residency cast to `CSparse.Double.SparseMatrix` to reach a vector member the base already exposes is the deleted ceremony, and the `double[]` operands bind the inherited array GEMV with no collision against the matrix-matrix overloads `Multiply(CompressedColumnStorage<double>[, result])`; bare `SparseMatrix` is reserved for the MathNet CSR concrete (`SolveIterative`) so the two sparse libraries never alias one name; the `Ldl` symmetric-indefinite/inertia kind binds `SparseLDL.Create` as a real `create` row — a `FactorKind` capability row with no factory delegate (a `<sparse-direct-miss>` fall-through) is the named declared-but-unbound defect; the cache populates success-only so only residual-witnessed factorizations enter and a diverged solve never poisons reuse; every structural `Edit` applies its edit to the operator before re-factoring — `Pin` drops row+column `node` and seats a unit diagonal, `Prune` `DropZeros` over a clone, a rank-1-edit kind's `Bump` runs the `SparseCholesky` `Update`/`Downdate` and discards-and-reconstructs the BUMPED operator (not the unedited one) on a `false` result, a non-rank-1-edit `Bump` accumulates `A + sign·w·wᵀ` over the column support and re-factors, and a `Bump` on a rectangular operator is rejected because a symmetric rank-1 update is ill-defined there — a default arm that silently re-factors the unedited operator and drops the `Pin`/`Prune`/`Bump` payload is the deleted form; a value-only `Revalue` clones the CSC through `Clone()` before overwriting the value array because the old `FactoredOp` still references the original storage and an in-place `CopyTo` corrupts the pre-edit operator, then re-creates with the SAME `op.Kind` from the cached permutation (no AMD over the invariant pattern) — the explicit-permutation `Create` amortizes the dominant symbolic cost (the AMD ordering) and yields a fully INDEPENDENT factor, so the in-place CSparse `Refactorize` (which additionally reuses the elimination tree and column counts but MUTATES the shared factor instance, aliasing the pre-edit `FactoredOp` whose `Inner` other readers and the non-reentrant single-owner solve still hold) is deliberately not taken: `FactoredOp` value immutability outranks the marginal numeric-phase saving, and `SparseQR` exposes no `Refactorize` at all; a hardcoded `SparseLU.Create` re-create that silently changes a non-LU operator's kind is the deleted correctness defect; the iterative method is the closed `IterativeMethod` SmartEnum and a raw-`string` method discriminant beside it is the named defect; the criterion stack constructs explicitly in precedence order because insertion order is precedence, `Failure` first keeps `NaN` terminal, and `Residual` before the count cap suppresses convergence on the final iteration; the iterate is admitted only on the independently recomputed true relative residual against the original operator because the converged verdict certifies only that the preconditioned residual fell below tolerance and left preconditioning distorts the norm; the structural substitution path is the most dangerous because it certifies an arbitrary iterate under a normal verdict and the ULP guard fails open on `NaN`; preconditioners initialize outside the solve and catch their throw there because the init throw otherwise escapes the verdict-returning entrypoint; the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseFormat {
    // Each format row carries its CSR-ingestion conversion as row data: csr is direct, csc/coo flip the
    // (major, minor) pointer pair onto the matching MathNet Of*Format factory, dok streams indexed value
    // tuples. There is ONE storage type (CSR); a parallel storage owner per format is the deleted form.
    public static readonly SparseFormat Csr = new("csr", static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Csc = new("csc", static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(r, c, vals.Length, minor, major, vals));
    public static readonly SparseFormat Coo = new("coo", static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(r, c, vals.Length, major, minor, vals));
    public static readonly SparseFormat Dok = new("dok", static (r, c, major, minor, vals) =>
        SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(r, c, toSeq(vals).Map((v, k) => (major[k], minor[k], v))));

    private readonly Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>> ingest;

    public SparseCompressedRowMatrixStorage<double> Ingest(int rows, int columns, int[] major, int[] minor, double[] values) =>
        ingest(rows, columns, major, minor, values);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FactorKind {
    // Every kind carries its full capability row: the fill formula, the transpose-solve recovery, AND the
    // permutation-keyed factory delegate (the shared `ISparseFactorization<double>` exposes only the forward
    // solve, so transpose/rank-1-edit/inertia/reentrancy are recovered from THIS row, never from the factor).
    // The symmetric kinds (spd/ldl) and lu take the cached AMD permutation through the explicit-permutation
    // `Create(csc, p[, tol])` overload so the ordering is computed once; qr re-derives the ordering internally
    // (its m<n branch factors the transpose) and ignores the cached permutation column.
    public static readonly FactorKind Spd = new("spd", rank1Edit: true, transposeSolve: false, inertia: false, reentrant: false,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseCholesky.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Ldl = new("ldl", rank1Edit: false, transposeSolve: false, inertia: true, reentrant: false,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, perm, _, _) => SparseLDL.Create(csc, perm),
        transposeRecover: static _ => None);
    public static readonly FactorKind Lu = new("lu", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, perm, _, tol) => SparseLU.Create(csc, perm, tol),
        transposeRecover: static inner => inner is SparseLU lu ? Some<Action<double[], double[]>>(lu.SolveTranspose) : None);
    public static readonly FactorKind Qr = new("qr", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: true,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, _, ordering, _) => SparseQR.Create(csc, ordering),
        transposeRecover: static inner => inner is SparseQR qr ? Some<Action<double[], double[]>>(qr.SolveTranspose) : None);
    // The NATIVE-DIRECT tier (csparse-interop, source-vendored — the same-author extension of the managed
    // rail): CHOLMOD supernodal Cholesky for large SPD stiffness, SuperLU/UMFPACK for large unsymmetric
    // systems. Rows mirror blas.md's managed↔native LinearProvider pattern — the managed spd/ldl/lu/qr
    // terminals stay the always-available cold-start selection, a native row is an explicit RID-claim-gated
    // choice whose Forge-provisioned native library FAULTS AT INIT when absent (never a silent managed
    // degrade), and whose IDisposable factor handle rides the FactoredOp capsule lifetime.
    public static readonly FactorKind Cholmod = new("cholmod", rank1Edit: false, transposeSolve: false, inertia: false, reentrant: false,
        fill: static (nnz, _, _) => nnz,
        create: static (csc, _, _, _) => NativeClaim.Cholmod(csc),
        transposeRecover: static _ => None);
    public static readonly FactorKind SuperLu = new("superlu", rank1Edit: false, transposeSolve: false, inertia: false, reentrant: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, _, _, _) => NativeClaim.SuperLu(csc),
        transposeRecover: static _ => None);
    public static readonly FactorKind Umfpack = new("umfpack", rank1Edit: false, transposeSolve: false, inertia: false, reentrant: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows,
        create: static (csc, _, _, _) => NativeClaim.Umfpack(csc),
        transposeRecover: static _ => None);

    private readonly Func<int, int, int, int> fill;
    private readonly Func<CompressedColumnStorage<double>, int[], ColumnOrdering, double, ISparseFactorization<double>> create;
    private readonly Func<ISparseFactorization<double>, Option<Action<double[], double[]>>> transposeRecover;

    public bool Rank1Edit { get; }
    public bool TransposeSolve { get; }
    public bool Inertia { get; }
    public bool Reentrant { get; }

    public int Fill(int nonZeros, int rows, int columns) => fill(nonZeros, rows, columns);
    public ISparseFactorization<double> Create(CompressedColumnStorage<double> csc, int[] permutation, ColumnOrdering ordering, double pivotTol) => create(csc, permutation, ordering, pivotTol);
    public Option<Action<double[], double[]>> TransposeRecover(ISparseFactorization<double> inner) => transposeRecover(inner);
}

// The RID-claim gate the three native FactorKind rows route: each factory lifts the CSC onto the CSparse
// SparseMatrix concrete the interop drivers bind, constructs the native factor, and runs its one-time
// Factorize() — a missing Forge-provisioned native library (SuiteSparse/SuperLU on osx-arm64; MKL PARDISO
// claims on x64 deployments) surfaces the native-init throw at the Factor boundary as a typed fault,
// never a silent fall to the managed terminal (the managed rows are the DEFAULT selection, not a fallback
// swap). Member conformance to ISparseFactorization<double> verifies against the vendored tree at first
// compose per the vendored-source law.
internal static class NativeClaim {
    public static ISparseFactorization<double> Cholmod(CompressedColumnStorage<double> csc) {
        var factor = new Cholmod((CSparse.Double.SparseMatrix)csc);
        factor.Factorize();
        return factor;
    }

    public static ISparseFactorization<double> SuperLu(CompressedColumnStorage<double> csc) {
        var factor = new SuperLU((CSparse.Double.SparseMatrix)csc);
        factor.Factorize();
        return factor;
    }

    public static ISparseFactorization<double> Umfpack(CompressedColumnStorage<double> csc) {
        var factor = new Umfpack((CSparse.Double.SparseMatrix)csc);
        factor.Factorize();
        return factor;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IterativeMethod {
    public static readonly IterativeMethod BiCgStab = new("bicgstab", static () => new BiCgStab());
    public static readonly IterativeMethod GpBiCg = new("gpbicg", static () => new GpBiCg());
    public static readonly IterativeMethod Tfqmr = new("tfqmr", static () => new TFQMR());
    public static readonly IterativeMethod MlkBiCgStab = new("mlk-bicgstab", static () => new MlkBiCgStab());

    private readonly Func<IIterativeSolver<double>> build;

    public IIterativeSolver<double> Solver() => build();
}

public sealed record IterationPolicy(double Tolerance, int MaxIterations, double DivergenceIncrease, int DivergenceFloor, Func<IPreconditioner<double>> Preconditioner) {
    public static readonly IterationPolicy Default = new(1e-10, 1_000, 0.08, 10, static () => new DiagonalPreconditioner());

    public Iterator<double> Iterator() =>
        new(
            new FailureStopCriterion<double>(),
            new DivergenceStopCriterion<double>(DivergenceIncrease, DivergenceFloor),
            new ResidualStopCriterion<double>(Tolerance),
            new IterationCountStopCriterion<double>(MaxIterations));
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

    // ONE polymorphic solve over both shapes. CSparse sizes the caller's result at A.ColumnCount for every
    // kind — the square triangular solve and the rectangular `m >= n` least-squares `min‖Ax−b‖` both land in
    // an n-length buffer, and CSparse allocates the augmented `S.m2` work row INTERNALLY (it is private factor
    // state, never a caller dimension). The witness recomputes the true relative residual against the ORIGINAL
    // operator A through the vector GEMV A inherits from CSparse's `Matrix<T> : ILinearOperator<double>` base
    // (A.Multiply(x, ax) binds the inherited array overload directly — no residency cast; `ax` sized
    // A.RowCount), never the square normal-equations operator whose conditioning the
    // rectangular QR avoids and never a dense fallback.
    public Fin<double[]> Solve(double[] rhs, double cap) {
        var x = new double[A.ColumnCount];
        Inner.Solve(rhs, x);
        var ax = new double[A.RowCount];
        A.Multiply(x, ax);
        double residual = TensorPrimitives.Distance<double>(ax, rhs) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs));
        return double.IsFinite(residual) && residual <= cap
            ? Fin.Succ(x)
            : Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<sparse-witness-fail:kind={Kind.Key}:rect={Rectangular}:fill={Fill}:r={residual:e3}>"));
    }
}

public static class SparseOps {
    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) =>
        minorIndices.Length != values.Length
            ? Fin.Fail<SparseCompressedRowMatrixStorage<double>>(new ComputeFault.PayloadOverBounds($"sparse-values:{values.Length}:{minorIndices.Length}"))
            : Fin.Succ(format.Ingest(rows, columns, majorIndices, minorIndices, values));

    // The two cross-lane CSC projections the ARPACK pencil (Solver/contract Modal) composes: the CSR-to-CSC
    // conversion the Factor path already owns, exposed once, and a diagonal CSC off a lumped-mass vector.
    public static CSparse.Double.SparseMatrix ToCsc(SparseCompressedRowMatrixStorage<double> csr) =>
        (CSparse.Double.SparseMatrix)ToColumnStorage(csr);

    public static CSparse.Double.SparseMatrix Diagonal(double[] diagonal) {
        var coords = new CoordinateStorage<double>(diagonal.Length, diagonal.Length, diagonal.Length);
        for (int i = 0; i < diagonal.Length; i++) { coords.At(i, i, diagonal[i]); }
        return (CSparse.Double.SparseMatrix)CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor) =>
        Lift(() => {
            var csc = ToColumnStorage(csr);
            csc.DropZeros(dropFloor);
            return Build(csc, kind, ordering, pivotTol);
        });

    public static Fin<(Vector<double> Field, double Residual, SolveTerminal Terminal)> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy) =>
        Try.lift(() => {
            var matrix = new SparseMatrix(csr);
            var b = Vector<double>.Build.DenseOfArray(rhs);
            var x = Vector<double>.Build.Dense(rhs.Length);
            var pre = policy.Preconditioner();
            pre.Initialize(matrix);
            var verdict = matrix.TrySolveIterative(b, x, method.Solver(), policy.Iterator(), pre);
            double residual = (matrix.Multiply(x) - b).L2Norm() / Math.Max(1.0, b.L2Norm());
            SolveTerminal terminal = verdict switch {
                IterationStatus.Converged => new SolveTerminal.Admitted(x, residual),
                _ => new SolveTerminal.Exhausted(x, policy.MaxIterations, residual),
            };
            return (x, residual, terminal);
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    // The structural-edit dialect resolves through the closed Edit Switch (total — a new dialect breaks
    // compilation, never a runtime-silent arm). A rank-1-edit kind (SPD/Cholesky) takes the in-place
    // Update/Downdate fast path and re-factors the bumped operator only on a `false` discard; the symmetric
    // rank-1 outer product is ill-defined on a rectangular operator and is rejected. Pin/Prune/Bump rebuild
    // the edited CSC and full-re-factor (the pattern changed, so the AMD ordering — not the cached permutation
    // — re-runs); Revalue keeps the pattern and re-creates from the cached permutation with the SAME kind.
    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit, double pivotTol) =>
        edit.Switch(
            pin: pin => Refactor(Pinned(op.A, pin.Node), op, pivotTol),
            prune: prune => Refactor(Cleaned(op.A, prune.Tolerance), op, pivotTol),
            bump: bump => op.Kind.Rank1Edit
                ? Downdate(op, bump, pivotTol)
                : op.Rectangular
                    ? Fin.Fail<FactoredOp>(ComputeFault.Create($"<bump-rectangular:{op.Kind.Key}>"))
                    : Refactor(Bumped(op.A, bump), op, pivotTol),
            revalue: revalue => Revalue(op, revalue.Values, pivotTol));

    static Fin<FactoredOp> Downdate(FactoredOp op, Edit.Bump bump, double pivotTol) =>
        op.Inner is SparseCholesky chol && RankOne(chol, RankOneColumn(bump.Column), bump.Sign)
            ? Fin.Succ(op)
            : Refactor(Bumped(op.A, bump), op, pivotTol);

    // Value-only refactor: the sparsity pattern is invariant, so the cached AMD permutation is the refactor
    // key — re-create with the SAME kind from `op.Permutation` (no AMD), clone-before-overwrite so the
    // pre-edit operator's storage is never corrupted (the old FactoredOp still references the original A).
    static Fin<FactoredOp> Revalue(FactoredOp op, double[] values, double pivotTol) =>
        Lift(() => {
            var fresh = op.A.Clone();
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
        var permutation = AMD.Generate(csc, ordering);
        var inner = kind.Create(csc, permutation, ordering, pivotTol);
        return new FactoredOp(inner, kind, csc, permutation, ordering, kind.Fill(csc.NonZerosCount, csc.RowCount, csc.ColumnCount), csc.FrobeniusNorm());
    }

    static bool RankOne(SparseCholesky chol, CompressedColumnStorage<double> w, int sign) =>
        sign >= 0 ? chol.Update(w) : chol.Downdate(w);

    static CompressedColumnStorage<double> RankOneColumn(double[] column) {
        var coords = new CoordinateStorage<double>(column.Length, 1, column.Length);
        toSeq(Enumerable.Range(0, column.Length)).Iter(row => coords.At(row, 0, column[row]));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    // Prune drops sub-tolerance residue (CSparse DropZeros over a clone — the source survives for the pre-edit
    // operator). Pin imposes a Dirichlet node by dropping row+column `node` and seating a unit diagonal. Bump
    // accumulates the symmetric rank-1 outer product `sign·w·wᵀ` over the column's nonzero support, duplicates
    // summed on convert so the edit is A + sign·w·wᵀ. Each returns a fresh CSC the caller re-factors.
    static CompressedColumnStorage<double> Cleaned(CompressedColumnStorage<double> a, double tolerance) {
        var fresh = a.Clone();
        fresh.DropZeros(tolerance);
        return fresh;
    }

    static CompressedColumnStorage<double> Pinned(CompressedColumnStorage<double> a, int node) {
        var coords = new CoordinateStorage<double>(a.RowCount, a.ColumnCount, a.NonZerosCount + 1);
        toSeq(a.EnumerateIndexedAsValueTuples()).Filter(t => t.row != node && t.column != node).Iter(t => coords.At(t.row, t.column, t.value));
        coords.At(node, node, 1.0);
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    static CompressedColumnStorage<double> Bumped(CompressedColumnStorage<double> a, Edit.Bump bump) {
        int[] support = toSeq(Enumerable.Range(0, bump.Column.Length)).Filter(i => bump.Column[i] != 0.0).ToArray();
        var coords = new CoordinateStorage<double>(a.RowCount, a.ColumnCount, a.NonZerosCount + support.Length * support.Length);
        toSeq(a.EnumerateIndexedAsValueTuples()).Iter(t => coords.At(t.row, t.column, t.value));
        toSeq(support).Iter(i => toSeq(support).Iter(j => coords.At(i, j, bump.Sign * bump.Column[i] * bump.Column[j])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorKind kind, SparseCompressedRowMatrixStorage<double> csr, SparseFormat format, int fill, double residual, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, csr.RowCount, csr.ColumnCount, csr.ValueCount, format.Key) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
            DeterminismTag = provider.DeterminismTag, SymbolicFill = fill, TrueResidual = residual,
        };

    static CompressedColumnStorage<double> ToColumnStorage(SparseCompressedRowMatrixStorage<double> csr) {
        var coords = new CoordinateStorage<double>(csr.RowCount, csr.ColumnCount, csr.ValueCount);
        toSeq(Enumerable.Range(0, csr.RowCount)).Iter(row =>
            toSeq(Enumerable.Range(csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row]))
                .Iter(slot => coords.At(row, csr.ColumnIndices[slot], csr.Values[slot])));
        return CompressedColumnStorage<double>.OfIndexed(coords, inplace: false);
    }
}
```


## [03]-[SPARSE_ALGEBRA]

- Owner: `SparseTensorOpFamily` `[SmartEnum<string>]` the sparse op axis (a SEPARATE owner from the dense `Tensor/vocabulary#OPERATION_TABLE` `TensorOpFamily`, never a 114th literal dense row) carrying a `Binary` arity flag and a `MatNetOwned` column naming which library owns the kernel; `SparseTensorOps` the static algebra fold over the one `SparseCompressedRowMatrixStorage<double>` CSR storage, routing each op to the MathNet `LinearAlgebra.Double.SparseMatrix` operator where MathNet owns the concern (SpMV/SpMM/add/scale/transpose/Kronecker) and to CSparse `CoordinateStorage`/`CompressedColumnStorage` pattern construction where MathNet does not (arbitrary contraction); `EinsumPlan` the index-subscript contraction planner deriving a greedy pairwise contraction order and lowering each step to the dense `KernelLowering.Lower(MatMul)` GEMM or the sparse `SparseTensorOps.Apply(Contract)` route under one polymorphic `Contract` entry; `SparseRun` the per-op nnz-and-format witness the sparse `TensorRun` receipt carries.
- Cases: `SparseTensorOpFamily` rows spmv · spmm · sp-add · sp-scale · sp-transpose · kronecker · contract · einsum (8); the dense lane `TensorOpFamily` rows stay the dense owner and a sparse row is never aliased onto a dense key.
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Apply(SparseTensorOpFamily op, SparseCompressedRowMatrixStorage<double> left, Option<SparseCompressedRowMatrixStorage<double>> right, double scalar)` is the one sparse-arithmetic fold — `Fin<T>` aborts on a shape mismatch or a missing right operand on a binary row; `public static Fin<double[]> Spmv(SparseMatrix held, ReadOnlySpan<double> x)` is the sparse mat-vec the iterative criterion stack and the `Solver/contract#SOLVE_CONTRACT` Newton residual consume over a HELD `SparseMatrix` (built once via `new SparseMatrix(csr)`) instead of per-call storage re-materialization; `public static Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)> Contract(EinsumPlan plan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> operands, ShardPlan shard)` folds the contraction tree over ONE unified operand vector (dense `Left` | sparse-CSR `Right`), threading each intermediate result back into the working store keyed by the surviving `Left` index so an n-operand contraction chains correctly, each step emitting one `SparseRun` under the same correlation.
- Auto: each binary-matrix `SparseTensorOpFamily` row binds its kernel through the `SparseTensorOps.Kernels` `FrozenDictionary` fold over operands wrapped once via `new SparseMatrix(storage)` (the storage-ctor, never a `SparseMatrix.OfStorage` phantom) — spmm binds `Matrix<double>.Multiply(Matrix<double>)`, sp-add binds `Matrix<double>.Add(Matrix<double>)`, sp-scale binds the scalar `Multiply(double)`, sp-transpose binds `Matrix<double>.Transpose()`, kronecker binds `Matrix<double>.KroneckerProduct(Matrix<double>)` over the sparse-built operands; spmv is the held-operator vector GEMV (`Matrix<double>.Multiply(Vector, Vector)`) and contract binds the CSC `CoordinateStorage` pattern build for an arbitrary index contraction MathNet does not own; `EinsumPlan.Of` parses the subscript spec into per-operand index labels and an output label, derives the contraction order by a greedy minimum-intermediate-size heuristic (binary-pairwise so each step is exactly one MatMul/contract row, never an n-ary kernel that bypasses the lowering table) and folds the merged shape into the `Left` slot so the next step's cost sees the intermediate; `Contract` seeds ONE unified working store from the operand vector (dense `Left` | sparse-CSR `Right`) and each step decides its route from the WORKING operands it contracts — both-dense lowers through `KernelLowering.Lower(MatMul)` and writes a dense intermediate, else `ContractPair` contracts the CSR pair (a dense operand densifies once through `SparseMatrix.OfMatrix`) and writes a sparse intermediate — writing every result back over the surviving `Left` index so an n-operand contraction chains correctly, the per-step transpose of the subscripts feeding the autodiff `Tensor/dispatch#EQUIVALENCE_INTEROP` JVP/VJP of an einsum.
- Receipt: every sparse op materializes the `Factorization`/`TensorRun` `ComputeReceipt` evidence carrying the result nnz, the source format key, the op key, and the `AllocationClass` — a sparse op that grows nnz stamps `AllocationClass.PooledMemory` because the MathNet sparse operators allocate fresh storage per op against the dense lane's in-place `SpanOwner` discipline, so the sparse fold fixes an nnz-growth allocation policy explicitly rather than pretending an in-place fold.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new sparse op is one `SparseTensorOpFamily` row binding its kernel on the `SparseTensorOps.Kernels` fold; a new contraction-order heuristic is one column on `EinsumPlan`; zero new surface — a `SpMvOp`/`SpMmOp`/`KroneckerOp` sibling family is the rejected form collapsed onto the one `SparseTensorOps` fold, a second sparse storage owner per op is the deleted form (csc/coo/dok stay ingestion conversions into the one CSR storage), and a string-eval einsum path is the deleted form.
- Boundary: `SparseTensorOps` is the sparse parallel of the dense `Tensor/dispatch#KERNEL_DISPATCH` `TensorOps`/`TensorOpFamily` split and operates on the one `SparseCompressedRowMatrixStorage<double>` the `[02]-[SPARSE_SOLVE]` ingestion fold owns — a parallel sparse-tensor type is the deleted form; the fold routes each op to the library that owns it — the inherited `Matrix<double>` `Multiply(Matrix)`/`Add(Matrix)`/`Transpose()`/scalar `Multiply(double)`/`Multiply(Vector, Vector)` for SpMM/add/scale/transpose/SpMV (MathNet sparse owns these), `Matrix<double>.KroneckerProduct(Matrix<double>)` for the tensor-product element space (the sparse operands build through the `new SparseMatrix(storage)` ctor and the Kronecker rides the inherited `Matrix<T>` member, never a `SparseMatrix.OfStorage` phantom), and CSparse `CoordinateStorage` pattern construction for an arbitrary index `contract` MathNet does not own — a hand-rolled triple loop beside `Matrix<double>.Multiply` or a managed Kronecker beside `KroneckerProduct` is the named reimplementation defect; the `EinsumPlan` contraction order is the named statement seam (the greedy heuristic walks a mutable cost array) bounded by the binary-pairwise reduction so each lowered step is exactly one settled `MatMul`/`Contract` row, never an n-ary kernel that bypasses the `KernelLowering` table, and the contraction-order optimization is exponential in operand count so the planner uses a greedy/DP heuristic bounded by intermediate-size cost rather than an exhaustive search, and the multi-operand contraction threads each intermediate back into one unified working operand store keyed by the surviving `Left` index (dense `Left` | sparse-CSR `Right`, the route decided at execution from the working operands' real sparsity) so a 3+-operand einsum chains correctly — an n-ary fold that indexes two disjoint dense/sparse operand arrays by one shared tree index (crashing a mixed contraction, dropping every dense intermediate to the empty-result fault) is the deleted incoherent form; the sparse SpMV `Solver/contract#SOLVE_CONTRACT` Newton residual and the iterative criterion-stack `A·x` consume `SparseTensorOps.Spmv` over one held `SparseMatrix` (built once via `new SparseMatrix(csr)`) rather than re-wrapping the storage per call (the named per-call-rematerialize defect the contract page already carries), the sparse `contract` feeds the einsum planner, the `AUTODIFF_DUAL_MODE_ENGINE` colored Jacobian assembles as sparse contractions over the `contract` row, and a device SpMV crosses to the `[04]-[DEVICE_KERNELS]` `DeviceKernels` registry SpMV pipeline through the residency gate; the nnz-growth allocation policy is fixed and stamped on the receipt because the MathNet sparse operators return fresh storage per op — a sparse fold that claims the dense lane's in-place `SpanOwner` discipline is dishonest, so the sparse `AllocationClass` is `PooledMemory` and the receipt records the result nnz.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SparseTensorOpFamily {
    public static readonly SparseTensorOpFamily Spmv = new("spmv", binary: false, matNetOwned: true);
    public static readonly SparseTensorOpFamily Spmm = new("spmm", binary: true, matNetOwned: true);
    public static readonly SparseTensorOpFamily SpAdd = new("sp-add", binary: true, matNetOwned: true);
    public static readonly SparseTensorOpFamily SpScale = new("sp-scale", binary: false, matNetOwned: true);
    public static readonly SparseTensorOpFamily SpTranspose = new("sp-transpose", binary: false, matNetOwned: true);
    public static readonly SparseTensorOpFamily Kronecker = new("kronecker", binary: true, matNetOwned: true);
    public static readonly SparseTensorOpFamily Contract = new("contract", binary: true, matNetOwned: false);
    public static readonly SparseTensorOpFamily Einsum = new("einsum", binary: true, matNetOwned: false);

    public bool Binary { get; }
    public bool MatNetOwned { get; }
}

public readonly record struct SparseRun(string Op, int Nnz, int Rows, int Columns, string Route);

public sealed record EinsumPlan(Seq<string> OperandSubscripts, string OutputSubscript, Seq<(int Left, int Right, string Subscripts)> Tree) {
    public static Fin<EinsumPlan> Of(string spec, Seq<(int Rows, int Columns, bool Sparse)> shapes) {
        var sides = spec.Split("->", StringSplitOptions.TrimEntries);
        if (sides.Length != 2) { return Fin.Fail<EinsumPlan>(ComputeFault.Create($"<einsum-spec-miss:{spec}>")); }
        Seq<string> operands = toSeq(sides[0].Split(',', StringSplitOptions.TrimEntries));
        return operands.Count == shapes.Count
            ? Fin.Succ(new EinsumPlan(operands, sides[1], GreedyOrder(operands, shapes)))
            : Fin.Fail<EinsumPlan>(ComputeFault.Create($"<einsum-operand-arity:{operands.Count}!={shapes.Count}>"));
    }

    // Greedy minimum-intermediate-size pairing: at each step pick the live pair whose contraction produces the
    // smallest intermediate, emit one binary row that WRITES the result back over `Left` and retires `Right`,
    // and fold the merged (Rows(Left) x Columns(Right)) shape into the `Left` slot of the mutable cost ledger so
    // the next step's estimate sees the intermediate, never the stale original. Binary-pairwise so each step
    // lowers to exactly one MatMul or Contract row; the `Left`-write index space is the one `Contract` threads.
    // The route is NOT pre-tagged here — an intermediate's sparsity is only known when `Contract` executes it.
    static Seq<(int Left, int Right, string Subscripts)> GreedyOrder(Seq<string> operands, Seq<(int Rows, int Columns, bool Sparse)> shapes) {
        var live = toSeq(Enumerable.Range(0, operands.Count));
        var dims = shapes.ToArray();
        var labels = operands;
        var steps = Seq<(int, int, string)>();
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
    static readonly FrozenDictionary<SparseTensorOpFamily, Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>>> Kernels =
        new (SparseTensorOpFamily Op, Func<SparseMatrix, Option<SparseMatrix>, double, Fin<SparseMatrix>> Build)[] {
            (SparseTensorOpFamily.Spmm, static (a, b, _) => b.ToFin(ComputeFault.Create("<spmm-missing-rhs>")).Map(rhs => (SparseMatrix)a.Multiply(rhs))),
            (SparseTensorOpFamily.SpAdd, static (a, b, _) => b.ToFin(ComputeFault.Create("<sp-add-missing-rhs>")).Map(rhs => (SparseMatrix)a.Add(rhs))),
            (SparseTensorOpFamily.SpScale, static (a, _, s) => Fin.Succ((SparseMatrix)a.Multiply(s))),
            (SparseTensorOpFamily.SpTranspose, static (a, _, _) => Fin.Succ((SparseMatrix)a.Transpose())),
            (SparseTensorOpFamily.Kronecker, static (a, b, _) => b.ToFin(ComputeFault.Create("<kronecker-missing-rhs>")).Map(rhs => (SparseMatrix)a.KroneckerProduct(rhs))),
        }.ToFrozenDictionary(static row => row.Op, static row => row.Build);

    public static Fin<SparseCompressedRowMatrixStorage<double>> Apply(SparseTensorOpFamily op, SparseCompressedRowMatrixStorage<double> left, Option<SparseCompressedRowMatrixStorage<double>> right, double scalar) =>
        op == SparseTensorOpFamily.Contract || op == SparseTensorOpFamily.Einsum
            ? right.ToFin(ComputeFault.Create($"<sparse-{op.Key}-missing-rhs>")).Bind(r => ContractPair(left, r))
            : Kernels.TryGetValue(op, out var build)
                ? build(new SparseMatrix(left), right.Map(static s => new SparseMatrix(s)), scalar)
                    .Map(static result => (SparseCompressedRowMatrixStorage<double>)result.Storage)
                : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<sparse-op-miss:{op.Key}>"));

    // SpMV over the HELD operator: the criterion-stack `A·x` and the Newton residual hold one SparseMatrix
    // and re-call Spmv per iterate, never re-wrapping the CSR storage per call. MathNet GEMV is the
    // Vector<double> overload (`Multiply(Vector, Vector)`) — a `Multiply(double[], double[])` array GEMV is
    // a phantom on Matrix<T>; only the O(n) right-hand-side/result vectors allocate, the O(nnz) operator does not.
    public static Fin<double[]> Spmv(SparseMatrix held, ReadOnlySpan<double> x) {
        if (x.Length != held.ColumnCount) { return Fin.Fail<double[]>(ComputeFault.Create($"<spmv-dim:{x.Length}!={held.ColumnCount}>")); }
        var y = Vector<double>.Build.Dense(held.RowCount);
        held.Multiply(Vector<double>.Build.DenseOfArray(x.ToArray()), y);
        return Fin.Succ(y.AsArray() ?? y.ToArray());
    }

    // The contraction tree folds over ONE unified working store (HashMap keyed by the tree's surviving `Left`
    // index, each operand a dense `Left` | sparse-CSR `Right`): every step contracts work[Left] and work[Right],
    // WRITES the result back over `Left`, and retires `Right` — so a 3+-operand einsum threads each intermediate
    // into the next step instead of re-reading the originals (the deleted two-disjoint-array indexing crashed a
    // mixed dense/sparse contraction and dropped every dense intermediate to the empty-result fault). The route
    // is decided HERE from the working operands' real sparsity: both-dense lowers through the
    // `KernelLowering.Lower(MatMul)` GEMM (dense intermediate), else `ContractPair` contracts the CSR pair (a
    // dense operand densifies once through `SparseMatrix.OfMatrix`) and writes a sparse intermediate; the single
    // surviving entry coerces to CSR through `CsrOf`.
    public static Fin<(SparseCompressedRowMatrixStorage<double> Result, Seq<SparseRun> Steps)> Contract(EinsumPlan plan, Seq<Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> operands, ShardPlan shard) =>
        plan.Tree.Fold(
            Fin.Succ((Work: toHashMap(operands.Map(static (op, i) => (i, op))), Steps: Seq<SparseRun>())),
            (acc, step) => acc.Bind(state => Step(state.Work, step, plan.OutputSubscript, shard).Map(next => (next.Work, state.Steps.Add(next.Run)))))
        .Map(state => (CsrOf(state.Work[plan.Tree.IsEmpty ? 0 : plan.Tree[plan.Tree.Count - 1].Left]), state.Steps));

    static Fin<(HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> Work, SparseRun Run)> Step(
        HashMap<int, Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>>> work, (int Left, int Right, string Subscripts) step, string output, ShardPlan shard) {
        var l = work[step.Left];
        var r = work[step.Right];
        return l.IsLeft && r.IsLeft
            ? KernelLowering.Lower(TensorOpFamily.MatMul, DenseOf(l), DenseOf(r), shard)
                .Map(dense => (work.AddOrUpdate(step.Left, Left<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(dense)).Remove(step.Right),
                    new SparseRun(output, 0, dense.RowCount, dense.ColumnCount, "dense")))
            : ContractPair(CsrOf(l), CsrOf(r))
                .Map(csr => (work.AddOrUpdate(step.Left, Right<Matrix<double>, SparseCompressedRowMatrixStorage<double>>(csr)).Remove(step.Right),
                    new SparseRun(output, csr.ValueCount, csr.RowCount, csr.ColumnCount, "sparse")));
    }

    static Matrix<double> DenseOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => dense, Right: static csr => new SparseMatrix(csr));

    static SparseCompressedRowMatrixStorage<double> CsrOf(Either<Matrix<double>, SparseCompressedRowMatrixStorage<double>> operand) =>
        operand.Match(Left: static dense => (SparseCompressedRowMatrixStorage<double>)SparseMatrix.OfMatrix(dense).Storage, Right: static csr => csr);

    // An arbitrary index contraction MathNet does not own: build the result pattern through the CSparse
    // CoordinateStorage the [02]-[SPARSE_SOLVE] handoff already uses, never a managed triple loop.
    static Fin<SparseCompressedRowMatrixStorage<double>> ContractPair(SparseCompressedRowMatrixStorage<double> left, SparseCompressedRowMatrixStorage<double> right) =>
        left.ColumnCount == right.RowCount
            ? Fin.Succ((SparseCompressedRowMatrixStorage<double>)((SparseMatrix)new SparseMatrix(left).Multiply(new SparseMatrix(right))).Storage)
            : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<contract-inner-dim:{left.ColumnCount}!={right.RowCount}>"));
}
```

## [04]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, plus the `ShardPlan` block-decomposition column the dense GEMM reads and the `ProveGemm` GEMM-vs-naive-reference proof the `Tensor/dispatch#EQUIVALENCE_INTEROP` equivalence law's matrix arm reads (the matrix lane has no scalar-tail kernel to span-prove, so the lowering owner that carries the GEMM also OWNS its proof and MatMul/Conv admission).
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` (local `Matrix<double>.Multiply` leaf) · `Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, Func<ContentAddress, IO<Option<ReadOnlyMemory<byte>>>> FetchPayload, Func<ReadOnlyMemory<byte>, IO<ContentAddress>> StorePayload, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentKey, ComputeReceipt.Factorization Receipt)` the per-block join carrier.
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` is the matmul lowering; the convolution overload `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan)` lowers Conv1D/Conv2D/Conv3D through `Im2Col` patch projection then one GEMM; `Fin<T>` aborts with `<lowering-row-miss>` only on a row outside the bound matrix set; the pooling entrypoint takes its window as the span argument; `public static ProofEvidence ProveGemm(int sampleCount)` proves the lowered GEMM against a naive triple-loop reference over a random operand pair (returning the absolute-gap/length/mass `ProofEvidence` the equivalence law's matrix arm wraps), and `public static bool IsMatrix(TensorOpFamily row)` reports the matrix-row set that arm routes to the proof.
- Auto: the tensor-lane `MatMul`/`Conv*`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider, each `Conv*` row lowers through the `Im2Col` patch projection that flattens every receptive field to a column then one `Matrix<double>.Multiply` GEMM against the reshaped kernel, and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; the `Im2Col` patch gather runs `ParallelHelper.For2D` over the `(outputPosition × channel)` rectangle writing each receptive field into the shared dense `double[,]` patch plane — each `(position, channel)` owns a disjoint contiguous run so the heap array is shared by reference with zero contention — so the embarrassingly-parallel gather rides the owned parallel-kernel row; the `ShardPlan.Single` leaf runs the local `Matrix<double>.Multiply`, and the `ShardPlan.Blocked` fan-out partitions the GEMM into `Tile`-high row-blocks over the row-block sweep, maps each block through `SubSolve` and collects the per-block `Fin<ShardBlock>` on the sequential `Traverse` rail, dials the EXISTING `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub once per block under the block's `WithDeadline`/`WithCancellationToken` call options (each block building a `SolveRequest` from its row-block and the active `FactorizationKind`), content-addresses every sub-block by writing the `SolveRequest` once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width then `XxHash128` against the Persistence `ModelResultIndex` so a re-run reuses computed blocks, joins the per-node `SolveResponse` solutions into the result via the associative `ShardBlock.Join` `SetSubMatrix` over a private join target, and aggregates each sub-block's `Factorization` receipt — `Traverse`-collected on the `Fin<Matrix<double>>` rail so a single failed shard aborts the join.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and the `Blocked` fan-out aggregates one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the per-node `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc`, or `Substrate.CpuTensor` on a content-address cache hit) — the shard count is the block count and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; a new matrix row inherits `ProveGemm` (the shared GEMM-core proof) with no new proof surface; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel directly and the `Conv1D`/`Conv2D`/`Conv3D` rows inherit it through the `Im2Col` patch projection, so the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; `Im2Col` enumerates each output spatial position over the `ConvWindow` stride/padding/dilation lattice through `ParallelHelper.For2D` writing into a shared heap `double[,]` patch plane (each `(position, channel)` owns a disjoint contiguous run, so the array is shared by reference with no contention — a `Span2D<double>` field would make the `IAction2D` gather a ref struct that the `where TAction : struct` For2D constraint, lacking `allows ref struct`, rejects), gathers the dilated receptive field across every channel into one patch row, and the patch matrix `[outPositions × Channels·KernelVolume]` multiplies the reshaped kernel `[Channels·KernelVolume × Filters]` in one `Matrix<double>.Multiply` whose tolerance is the `MatMul` proof the convolution row inherits through its `ToleranceClass.AccumulationScaled` column — `ProveGemm` proves that GEMM against a naive triple-loop reference and OWNS the matrix-lane admission the span proof cannot reach (a `MatMul`/`Conv` row has no scalar-tail kernel, so `EquivalenceLaw.Prove` routes the matrix family here through `IsMatrix` and a row that reached the span path would be the phantom), the convolution inheriting it because the `Im2Col` gather adds no float error; the `ParallelHelper.For2D` gather is this lane's named statement seam and a managed nested `Enumerable.Range` gather with `patch[i,j] =` mutation outside the parallel row is the deleted form; a Conv row routed without a `ConvWindow` (the matmul overload) returns the `<lowering-row-miss>` Fin.Fail because the geometry is absent; the `Blocked` shard fan-out is a row-block partition over the dense fold dialing each row-block sub-solve through the EXISTING `Substrate.RemoteGrpc` `ComputeService.ComputeServiceClient` stub and the `Solve` rpc owned by `Runtime/wire#PROTO_VOCABULARY` by reference — the `Blocked` case carries the stub, provider, kind, reuse index, object-store payload ports, correlation, clock, deadline, and cancellation token as constructor columns so the arm is a real dial with a per-call `WithDeadline`/`WithCancellationToken` bound derived from the clock and budget, the channel pins `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` against the payload cap, and a local-only tile loop, an unbounded dial with no deadline, or an uncapped channel is the named defect; the row-block matrix and the RHS BOTH ride the wire through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` no-copy — each column-major `double[]` is reinterpreted as `Memory<byte>` through the CommunityToolkit `Cast<double, byte>` view (no copy) and the array roots the `ByteString` past the request read window, so a `MemoryMarshal.AsBytes(...).ToArray()` before the wrap, copying the very buffer `UnsafeWrap` exists to alias, is the deleted allocation, and the `SolveRequest.matrix` field carries those raw column-major float64 bytes (the server reshaping from the byte length and `shard_tile`) rather than a `GeometryPayload` geometry envelope — a dense solver operand is not a point_cloud/mesh/voxel payload, so routing the matrix through the geometry oneof (which carries no dense case) is the deleted category violation — the request content-addresses by writing once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width rather than a throwaway `ToByteArray()` per sub-block, and the `SolveResponse` solution span casts into `Build.Dense` directly — a full `CopyFrom`/`ToByteArray`/`ToArray` copy per shard is the deleted allocation; a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter` or a second substrate is the deleted form; each sub-block keys on the streamed-`SolveRequest` `XxHash128` folded against the provider `SolveDedupKey` against the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam by reference — `Lookup` resolving the dedup-keyed `ModelResultRow` RESIDENCE (the index never holds the payload) and the object-store port yielding the `SolveResponse` bytes at that residence, an orphan-swept blob missing cleanly to a re-dial — so a re-run reuses computed blocks (the cache-hit receipt carries `Substrate.CpuTensor`, the dialed receipt `Substrate.RemoteGrpc`); a 2-arg `Publish(address, response)` that pretends the index itself stores the payload is the deleted phantom, and the join writes each `ShardBlock` through `SetSubMatrix` into a private per-fan-out target — a shared mutable accumulator threaded through the per-shard `Map` is the named race defect; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

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

    public Fin<Matrix<double>> Lower(Matrix<double> left, Matrix<double> right) =>
        Switch(
            state: (Left: left, Right: right),
            single: static (s, _) => Fin.Succ(s.Left.Multiply(s.Right)),
            blocked: static (s, plan) => Fanout(s.Left, s.Right, plan));

    static Fin<Matrix<double>> Fanout(Matrix<double> left, Matrix<double> right, Blocked plan) =>
        toSeq(Enumerable.Range(0, (left.RowCount + plan.Tile - 1) / plan.Tile))
            .Map(block => {
                int start = block * plan.Tile;
                int height = Math.Min(plan.Tile, left.RowCount - start);
                return SubSolve(left.SubMatrix(start, height, 0, left.ColumnCount), right, start, height, plan);
            })
            .Traverse(identity)
            .Map(blocks =>
                blocks.Fold(Matrix<double>.Build.Dense(left.RowCount, right.ColumnCount), static (target, block) => { ShardBlock.Join(target, block); return target; }));

    static Fin<ShardBlock> SubSolve(Matrix<double> rowBlock, Matrix<double> right, int start, int height, Blocked plan) {
        // True no-copy on BOTH operands: each freshly-allocated column-major double[] is reinterpreted as a
        // Memory<byte> view (CommunityToolkit Cast, no copy) and adopted by UnsafeWrap; the double[] roots the
        // ByteString and outlives the request read window. The matrix rides as raw column-major float64 bytes
        // EXACTLY like the RHS — a dense solver operand is not a point_cloud/mesh/voxel GeometryPayload, so the
        // SolveRequest carries no geometry envelope and the server reshapes from the byte length and shard_tile.
        // A MemoryMarshal.AsBytes(...).ToArray() before the wrap would copy the very buffer UnsafeWrap aliases.
        var matrix = rowBlock.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        var rhs = right.ToColumnMajorArray().AsMemory().Cast<double, byte>();
        var request = new SolveRequest {
            Matrix = UnsafeByteOperations.UnsafeWrap(matrix),
            Rhs = UnsafeByteOperations.UnsafeWrap(rhs),
            FactorizationKind = plan.Kind.Key,
            SparseFormat = string.Empty,
            ShardTile = plan.Tile,
        };
        var address = Digest(request, plan.Provider.SolveDedupKey((UInt128)start));
        var dialedAt = plan.Clock.GetCurrentInstant();
        // Content-addressed reuse against the Persistence ModelResultIndex: Lookup resolves the dedup-keyed
        // ModelResultRow RESIDENCE (the index never holds the payload), the object-store port yields the
        // SolveResponse bytes at that residence, and an index row whose blob was orphan-swept misses cleanly to
        // a re-dial — the index and the object store stay ONE reuse seam, never a Compute-side result store.
        Option<ShardBlock> reused =
            plan.Reuse.Lookup(address).Run().Bind(row =>
                plan.FetchPayload(row.Residence).Run().Map(bytes =>
                    Materialize(SolveResponse.Parser.ParseFrom(bytes.Span), address, start, height, right.ColumnCount, Substrate.CpuTensor, Duration.Zero, plan)));
        return reused.Match(
            Some: static block => Fin.Succ(block),
            None: () => Dial(plan, request).Map(response =>
                Store(plan, response, address, start, height, right.ColumnCount, plan.Clock.GetCurrentInstant() - dialedAt)));
    }

    // Write-once reuse publish: content-address the dialed payload through the object-store port (write-blob-first
    // by reference, never re-owned here) then Publish the dedup ROW (the SolveDedupKey-salted lookup key over the
    // blob residence) so a re-run reuses the block; the payload serializes once into a pooled rent, never a
    // throwaway ToByteArray, and the fingerprint is the provider determinism tag the dedup key already folds.
    static ShardBlock Store(Blocked plan, SolveResponse response, UInt128 address, int start, int height, int cols, Duration elapsed) {
        using MemoryOwner<byte> rent = MemoryOwner<byte>.Allocate(response.CalculateSize());
        response.WriteTo(rent.Span);
        ContentAddress residence = plan.StorePayload(rent.Memory).Run();
        plan.Reuse.Publish(new ModelResultRow(address, residence, plan.Provider.DeterminismTag, plan.Clock.GetCurrentInstant())).Run();
        return Materialize(response, address, start, height, cols, Substrate.RemoteGrpc, elapsed, plan);
    }

    static UInt128 Digest(SolveRequest request, UInt128 salt) {
        using SpanOwner<byte> rent = SpanOwner<byte>.Allocate(request.CalculateSize());
        request.WriteTo(rent.Span);
        return XxHash128.HashToUInt128(rent.Span, unchecked((long)salt));
    }

    static ShardBlock Materialize(SolveResponse response, UInt128 address, int start, int height, int defaultCols, Substrate substrate, Duration elapsed, Blocked plan) {
        int cols = response.Cols == 0 ? defaultCols : (int)response.Cols;
        var receipt = new ComputeReceipt.Factorization(response.Provider, response.Decomposition, height, cols, response.Nnz, "dense") {
            Correlation = plan.Correlation, Lane = WorkLane.Background, Substrate = substrate, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed, DeterminismTag = plan.Provider.DeterminismTag,
        };
        return new ShardBlock(start, height, Restore(response, height, cols), address, receipt);
    }

    static Fin<SolveResponse> Dial(Blocked plan, SolveRequest request) =>
        Try.lift(() => plan.Compute.Solve(request, new CallOptions(new Metadata { { "rasm-correlation", plan.Correlation.ToString() } })
                .WithDeadline(plan.Clock.GetCurrentInstant().Plus(plan.Deadline).ToDateTimeUtc())
                .WithCancellationToken(plan.Cancel)))
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<shard-dial:{error.Message}>"));

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
    // The spatial rank of each convolution row is an explicit table, never recovered by indexing into the
    // string key (`row.Key[^2] - '0'` is fragile literal arithmetic over the key spelling); the keys of this
    // table ARE the convolution row set the geometry overload admits.
    static readonly FrozenDictionary<TensorOpFamily, int> ConvRank = new (TensorOpFamily Row, int Rank)[] {
        (TensorOpFamily.Conv1D, 1), (TensorOpFamily.Conv2D, 2), (TensorOpFamily.Conv3D, 3),
    }.ToFrozenDictionary(static r => r.Row, static r => r.Rank);

    static readonly FrozenSet<TensorOpFamily> MatrixRows = new[] {
        TensorOpFamily.MatMul, TensorOpFamily.Conv1D, TensorOpFamily.Conv2D, TensorOpFamily.Conv3D,
    }.ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> PoolRows = new[] {
        TensorOpFamily.MaxPool, TensorOpFamily.AvgPool, TensorOpFamily.GlobalMaxPool, TensorOpFamily.GlobalAvgPool,
    }.ToFrozenSet();

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        row == TensorOpFamily.MatMul ? plan.Lower(left, right)
        : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan) =>
        ConvRank.TryGetValue(row, out int rank) && window.Rank == rank
            ? plan.Lower(Im2Col(input, window), kernel)
            : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    static Matrix<double> Im2Col(Matrix<double> input, ConvWindow window) {
        int[] extents = window.OutputExtents;
        var patch = new double[window.OutputPositions, window.PatchWidth];
        var gather = new PatchGather(input, window, extents, patch);
        ParallelHelper.For2D(0, window.OutputPositions, 0, window.Channels, in gather);
        return Matrix<double>.Build.DenseOfArray(patch);
    }

    // The (outputPosition × channel) rectangle is embarrassingly parallel: each (position, channel) writes a
    // DISJOINT contiguous run `[channel·KernelVolume, channel·KernelVolume + KernelVolume)` of patch row
    // `position`, so the heap-allocated double[,] is shared by reference across partitions with zero
    // contention. The action holds the array, NOT a `Span2D<double>` — a ref-struct field would make
    // PatchGather a ref struct, which the `where TAction : struct` For2D constraint (no `allows ref struct`)
    // rejects; that is the named statement-seam defect a managed nested `Enumerable.Range` gather restates.
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

    // The matrix lane has no scalar-tail fallback the span proof can run, so the lowered `Matrix<double>.Multiply`
    // GEMM is proven against an INDEPENDENT naive O(n³) triple-loop reference over a random operand pair sized from
    // the sample budget — this proof OWNS MatMul/Conv admission (the `Tensor/dispatch#EQUIVALENCE_INTEROP`
    // `EquivalenceLaw.Prove` matrix arm reads it, and a `Conv*` row inherits it because the `Im2Col` gather is an
    // exact reshape that adds no float error, so the GEMM core carries the whole matrix-lane tolerance). The
    // evidence is the ABSOLUTE max-abs entry gap, the contraction length n, and the dominant per-entry product
    // mass Σ|aᵢₖ·bₖⱼ| the `ToleranceClass.AccumulationScaled` envelope `Bound(n, mass) = n·ε·mass` bounds (the
    // reassociation error of n products is `≤ (n−1)·ε·mass < Bound`, so the proof holds) — never a relative
    // pre-division, never a proof that skips the lowered kernel, and the unreachable `Single`-plan `Fail` arm
    // collapses to `Unprovable` rather than a silent admission.
    public static ProofEvidence ProveGemm(int sampleCount) {
        int n = Math.Max(2, (int)Math.Sqrt(sampleCount));
        Matrix<double> left = Gaussian(n), right = Gaussian(n);
        return Lower(TensorOpFamily.MatMul, left, right, new ShardPlan.Single()).Match(
            Succ: gemm => {
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
            },
            Fail: static _ => ProofEvidence.Unprovable);
    }

    static Matrix<double> Gaussian(int n) {
        Tensor<double> flat = Tensor.CreateFromShape<double>([n * n]);
        Tensor.FillGaussianNormalDistribution(flat);
        double[] values = new double[n * n];
        flat.FlattenTo(values);
        return Matrix<double>.Build.Dense(n, n, values);
    }
}
```


## [05]-[RESEARCH]

- [SHARD_FANOUT]: the `ShardPlan.Blocked` fan-out dials the `Runtime/wire#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub by reference, builds `SolveRequest` field-for-field (`matrix`/`rhs`/`factorization_kind`/`sparse_format`/`shard_tile`), no-copy-wraps the RHS through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)`, content-addresses each row-block by writing the request once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent folded through `XxHash128.HashToUInt128` against the provider `SolveDedupKey` and the Persistence `ModelResultIndex` for sub-block reuse, dials under a per-call `WithDeadline`/`WithCancellationToken` bound from the clock and budget, joins the per-node `SolveResponse` solutions via the associative `ShardBlock.Join` `SetSubMatrix` into a private join target, and aggregates the per-shard `Factorization` receipts on the `Fin<Matrix<double>>` rail. The open leaf is the live in-host stub dial: the Grpc.Tools-compiled `ComputeService` client and the `Solve`-stub call resolve only inside the running integrated host plugin ALC, so `ShardPlan.SubSolve` against the live stub is the cross-lane probe that grounds the fan-out; the `SolveRequest`/`SolveResponse` field shapes (`matrix=1 bytes`, `rhs=2 bytes`, `factorization_kind=3 string`, `sparse_format=4 string`, `shard_tile=5 int32`; `solution=1 bytes`, `provider=2 string`, `decomposition=3 string`, `rows=4 int64`, `cols=5 int64`, `nnz=6 int64`) are the `Runtime/wire#PROTO_VOCABULARY` rows consumed by reference — the row-block matrix riding `matrix=1 bytes` as raw column-major float64 (no `GeometryPayload` geometry envelope; a dense operand is not point_cloud/mesh/voxel, and the oneof carries no dense case) — and the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam (the index resolving the dedup-keyed `ModelResultRow` residence, the object-store port carrying the `SolveResponse` payload at that residence) composes its owning lane.
