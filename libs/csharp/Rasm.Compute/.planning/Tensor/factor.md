# [COMPUTE_FACTOR]

Rasm.Compute sparse-solve and kernel-lowering lane: the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SolveTerminal` partition preserving the caller's retry, and the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel plus the `ShardPlan` block-decomposition column the dense GEMM reads. Every library refuses its own gates — `Iterator<T>` exposes no iteration count — so the criterion stack re-imposes each and every result leaves as a typed `ComputeReceipt` carrying the route variant, the scale-derived tolerance, and the recomputed true relative residual against the original operator. A distributed solve crosses solely through the `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc, which the `ShardPlan.Blocked` row-block sub-solve dials by reference.

## [1]-[INDEX]

- [1]-[SPARSE_SOLVE]: CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative.
- [2]-[KERNEL_LOWERING]: tensor matrix/structural rows lower onto real GEMM/im2col/pool; shard fan-out.

## [2]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows; `FactorKind` `[SmartEnum<string>]` direct-factor rows carrying the capability columns (rank-1 edit, transpose-solve, inertia, reentrancy) and the fill-formula and transpose-solve-recovery delegate as row data; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis with the `IterationPolicy` record (tolerance · max-iter · criterion stack · preconditioner); `FactoredOp` the typed sparse-operator value owning the factorization instance, cached `ColumnOrdering` permutation, symbolic fill counts, solution dimension, and kind discriminant; `Edit` `[Union]` the structural-edit dialect; `SparseOps` direct-and-iterative sparse-solve fold over CSR-backed MathNet storage and CSparse CSC direct factorizations, ingestion and direct dispatch each driven by one `FrozenDictionary` factory fold.
- Cases: `SparseFormat` rows csr · csc · coo · dok (4); `FactorKind` rows spd · ldl · lu · qr (4); `IterativeMethod` rows bicgstab · gpbicg · tfqmr · mlk-bicgstab (4); `Edit` cases `Pin` · `Prune` · `Bump` · `Revalue` (4); `SparseOps.DirectSolvers` rows spd · lu · qr (3); `SparseOps.Ingestors` rows csr · csc · coo · dok (4).
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — `Fin<T>` aborts on a length or bound mismatch; `public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor)` converts the CSR triplets once to a CSparse `CompressedColumnStorage<double>` through `CoordinateStorage` + the admitted `CompressedColumnStorage<double>.OfIndexed` CSC factory, reads the symbolic fill before the numeric sweep, and collapses the completed factorization to one `FactoredOp` value; `FactoredOp.Solve(double[] rhs, double cap)` returns the witnessed field; `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected `IIterativeSolver<double>` under the explicitly-ordered criterion stack and returns the field plus the recomputed true relative residual and `SolveTerminal` verdict — the iteration count is NOT read from `Iterator<double>` (which exposes only `Status`), it is the criterion-stack-bounded cap.
- Auto: every format row maps to one CSR ingestion conversion through the `Ingestors` `FrozenDictionary` fold — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `DirectSolvers` `FrozenDictionary` fold binding `SparseCholesky.Create(csc, ordering)`/`SparseLU.Create(csc, ordering, pivotTol)`/`SparseQR.Create(csc, ordering)` and solve in place through `ISparseFactorization<double>.Solve(double[], double[])`; iterative solves run the `IterativeMethod` row's `Solver()` factory under the `IterationPolicy.Iterator()` `Iterator<double>` criterion stack constructed in precedence order `Failure → Divergence → Residual → IterationCount`; `FactoredOp.TransposeSolve` recovers the transpose-solve action from the `FactorKind` row's `TransposeRecover` delegate column alone (some for lu and qr, none for spd and ldl) because the shared `ISparseFactorization<double>` exposes only the forward solve and `SolveTranspose` closes over the concrete `SparseLU`/`SparseQR`.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, factor kind, the symbolic fill, the recomputed true relative residual, row and column extents, the `ValueCount` non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `Ingestors` row keyed by its `SparseFormat`; a new direct solver is one `DirectSolvers` row plus one `FactorKind` row carrying its capability, fill, and transpose-recovery columns; a new iterative method is one `IterativeMethod` row carrying its `IIterativeSolver<double>` factory column; a new structural-edit dialect is one `Edit` case over the three primitives; a new iteration knob is one column on the `IterationPolicy` record; zero new surface.
- Boundary: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage — csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner for each format is the deleted form; the CSR-to-CSC handoff builds a `CoordinateStorage<double>(rows, cols, nnz)`, calls `.At(i, j, v)` per entry, and converts once through the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` CSC factory (the CSparse static that internally runs `Converter.ToCompressedColumnStorage` with cleanup) — a hand-rolled `Converter.ToCompressedColumnStorage` detour beside the admitted `OfIndexed` factory is the named reimplementation defect, and `inplace: true` is rejected when the triplet must survive a structural-edit increment because it invalidates the source arrays and dangles references; strict storage `Validate` runs before factoring because it returns `bool` and never throws and factorizing invalid storage produces silently incorrect factors; the symbolic fill is read before the numeric sweep to route direct versus iterative and the count is per-kind through the `FactorKind.Fill` delegate column (one factor for the symmetric kinds, an `L + U − n` formula for `SparseLU`, a `Q + R − m` formula for `SparseQR`) so a bare fill integer compared across kinds is meaningless; the `ColumnOrdering.MinimumDegreeAtPlusA` permutation `int[]` from `CSparse.Ordering.AMD.Generate(CompressedColumnStorage<double>, ColumnOrdering)` caches as the value-only refactor key over an invariant pattern (`ColumnOrdering` values are `Natural`, `MinimumDegreeAtPlusA`, `MinimumDegreeStS`, `MinimumDegreeAtA`; the AMD ordering type lives in `CSparse.Ordering`, distinct from the `ColumnOrdering` enum, and its `Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order)` takes the matrix first and the ordering second); assembly residue drops with a structural tolerance near `machineEps · ‖A‖_F` through `DropZeros(tolerance)` because the default `0.0` removes only binary zeros; `SparseLU` pivot `tol` is `[0, 1]` as a relative column threshold (`1` full partial pivoting, `0` disabled) never an absolute floor; transpose-solve/rank-1-edit/inertia/reentrancy recover from the `FactorKind` row alone because the shared solver interface exposes only the forward solve; an asymmetric input to a symmetric kind factors as its symmetrization and returns a correct answer to the WRONG system so the post-solve true residual is the only structural signal; a typed-only catch at the factorization boundary is rejected because SPD pivot loss and the zero-diagonal break throw bare `Exception`; the cached square factorization's one constructor-allocated scratch is non-reentrant so solves serialize through the `FactoredOp` capsule and the `SparseQR` reentrant kind is the one parallel-safe row; the rectangular kind's work buffer sizes from the factorization's solution dimension (which exceeds the row count for structurally singular systems) and sizing from the matrix shape is the off-by-augmentation fault; the cache populates success-only so only residual-witnessed factorizations enter and a diverged solve never poisons reuse; a structural rank-1 SPD edit applies the `SparseCholesky` rank-1 update with discard-and-reconstruct on a `false` result, never a no-op success arm that returns the unedited operator; a value-only `Revalue` clones the CSC through `Clone()` before overwriting the value array because the old `FactoredOp` still references the original storage and an in-place `CopyTo` corrupts the pre-edit operator; the iterative method is the closed `IterativeMethod` SmartEnum and a raw-`string` method discriminant beside it is the named defect; the criterion stack constructs explicitly in precedence order because insertion order is precedence, `Failure` first keeps `NaN` terminal, and `Residual` before the count cap suppresses convergence on the final iteration; the iterate is admitted only on the independently recomputed true relative residual against the original operator because the converged verdict certifies only that the preconditioned residual fell below tolerance and left preconditioning distorts the norm; the structural substitution path is the most dangerous because it certifies an arbitrary iterate under a normal verdict and the ULP guard fails open on `NaN`; preconditioners initialize outside the solve and catch their throw there because the init throw otherwise escapes the verdict-returning entrypoint; the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class SparseFormat {
    public static readonly SparseFormat Csr = new("csr");
    public static readonly SparseFormat Csc = new("csc");
    public static readonly SparseFormat Coo = new("coo");
    public static readonly SparseFormat Dok = new("dok");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class FactorKind {
    public static readonly FactorKind Spd = new("spd", rank1Edit: true, transposeSolve: false, inertia: false, reentrant: false,
        fill: static (nnz, _, _) => nnz, transposeRecover: static _ => None);
    public static readonly FactorKind Ldl = new("ldl", rank1Edit: false, transposeSolve: false, inertia: true, reentrant: false,
        fill: static (nnz, _, _) => nnz, transposeRecover: static _ => None);
    public static readonly FactorKind Lu = new("lu", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: false,
        fill: static (nnz, rows, _) => 2 * nnz - rows, transposeRecover: static inner => inner is SparseLU lu ? Some<Action<double[], double[]>>(lu.SolveTranspose) : None);
    public static readonly FactorKind Qr = new("qr", rank1Edit: false, transposeSolve: true, inertia: false, reentrant: true,
        fill: static (nnz, rows, _) => 2 * nnz - rows, transposeRecover: static inner => inner is SparseQR qr ? Some<Action<double[], double[]>>(qr.SolveTranspose) : None);

    private readonly Func<int, int, int, int> fill;
    private readonly Func<ISparseFactorization<double>, Option<Action<double[], double[]>>> transposeRecover;

    public bool Rank1Edit { get; }
    public bool TransposeSolve { get; }
    public bool Inertia { get; }
    public bool Reentrant { get; }

    public int Fill(int nonZeros, int rows, int columns) => fill(nonZeros, rows, columns);
    public Option<Action<double[], double[]>> TransposeRecover(ISparseFactorization<double> inner) => transposeRecover(inner);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
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

public sealed record FactoredOp(ISparseFactorization<double> Inner, FactorKind Kind, CompressedColumnStorage<double> A, int[] Permutation, int Fill, int SolutionDim, double FrobeniusNorm) {
    public Option<Action<double[], double[]>> TransposeSolve => Kind.TransposeRecover(Inner);

    public Fin<double[]> Solve(double[] rhs, double cap) {
        var x = new double[SolutionDim];
        Inner.Solve(rhs, x);
        var ax = new double[rhs.Length];
        A.Multiply(x, ax);
        double residual = TensorPrimitives.Distance<double>(ax, rhs) / Math.Max(1.0, TensorPrimitives.Norm<double>(rhs));
        return double.IsFinite(residual) && residual <= cap
            ? Fin.Succ(x)
            : Fin.Fail<double[]>(new ComputeFault.ModelRejected($"<sparse-witness-fail:kind={Kind.Key}:fill={Fill}:r={residual:e3}>"));
    }

    public FactoredOp Revalue(double[] values, double pivotTol) {
        var fresh = A.Clone();
        values.CopyTo(fresh.Values, 0);
        return this with { Inner = SparseLU.Create(fresh, Permutation, pivotTol), A = fresh };
    }
}

public static class SparseOps {
    static readonly FrozenDictionary<SparseFormat, Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>>> Ingestors =
        new (SparseFormat Format, Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>> Build)[] {
            (SparseFormat.Csr, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(r, c, vals.Length, major, minor, vals)),
            (SparseFormat.Csc, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(r, c, vals.Length, minor, major, vals)),
            (SparseFormat.Coo, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(r, c, vals.Length, major, minor, vals)),
            (SparseFormat.Dok, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(r, c, Indexed(major, minor, vals))),
        }.ToFrozenDictionary(static row => row.Format, static row => row.Build);

    static readonly FrozenDictionary<FactorKind, Func<CompressedColumnStorage<double>, ColumnOrdering, double, ISparseFactorization<double>>> DirectSolvers =
        new (FactorKind Kind, Func<CompressedColumnStorage<double>, ColumnOrdering, double, ISparseFactorization<double>> Create)[] {
            (FactorKind.Spd, static (csc, ordering, _) => SparseCholesky.Create(csc, ordering)),
            (FactorKind.Lu, static (csc, ordering, pivotTol) => SparseLU.Create(csc, ordering, pivotTol)),
            (FactorKind.Qr, static (csc, ordering, _) => SparseQR.Create(csc, ordering)),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Create);

    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) =>
        minorIndices.Length != values.Length
            ? Fin.Fail<SparseCompressedRowMatrixStorage<double>>(new ComputeFault.PayloadOverBounds($"sparse-values:{values.Length}:{minorIndices.Length}"))
            : Ingestors.TryGetValue(format, out var build)
                ? Fin.Succ(build(rows, columns, majorIndices, minorIndices, values))
                : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<sparse-format-miss:{format.Key}>"));

    public static Fin<FactoredOp> Factor(SparseCompressedRowMatrixStorage<double> csr, FactorKind kind, ColumnOrdering ordering, double pivotTol, double dropFloor) =>
        DirectSolvers.TryGetValue(kind, out var create)
            ? Try.lift(() => {
                var csc = ToColumnStorage(csr);
                csc.DropZeros(dropFloor);
                var permutation = AMD.Generate(csc, ordering);
                var inner = create(csc, ordering, pivotTol);
                return new FactoredOp(inner, kind, csc, permutation, kind.Fill(csc.NonZerosCount, csc.RowCount, csc.ColumnCount), csc.ColumnCount, FrobeniusOf(csc));
            }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected($"<sparse-factor-break:{error.Message}>"))
            : Fin.Fail<FactoredOp>(ComputeFault.Create($"<sparse-direct-miss:{kind.Key}>"));

    public static Fin<(Vector<double> Field, double Residual, SolveTerminal Terminal)> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy) =>
        Try.lift(() => {
            var matrix = SparseMatrix.OfStorage(csr);
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

    public static Fin<FactoredOp> Apply(FactoredOp op, Edit edit, double pivotTol) => edit switch {
        Edit.Bump bump when op.Kind == FactorKind.Spd => Downdate(op, bump, pivotTol),
        Edit.Revalue revalue => Fin.Succ(op.Revalue(revalue.Values, pivotTol)),
        _ => Factor(ToRowStorage(op.A), op.Kind, ColumnOrdering.MinimumDegreeAtPlusA, pivotTol, op.FrobeniusNorm * Math.ScaleB(1.0, -52)),
    };

    static Fin<FactoredOp> Downdate(FactoredOp op, Edit.Bump bump, double pivotTol) =>
        op.Inner is SparseCholesky chol && RankOne(chol, RankOneColumn(bump.Column), bump.Sign)
            ? Fin.Succ(op)
            : Factor(ToRowStorage(op.A), op.Kind, ColumnOrdering.MinimumDegreeAtPlusA, pivotTol, op.FrobeniusNorm * Math.ScaleB(1.0, -52));

    static bool RankOne(SparseCholesky chol, CompressedColumnStorage<double> w, int sign) =>
        sign >= 0 ? chol.Update(w) : chol.Downdate(w);

    static CompressedColumnStorage<double> RankOneColumn(double[] column) {
        var coords = new CoordinateStorage<double>(column.Length, 1, column.Length);
        toSeq(Enumerable.Range(0, column.Length)).Iter(row => coords.At(row, 0, column[row]));
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

    static SparseCompressedRowMatrixStorage<double> ToRowStorage(CompressedColumnStorage<double> csc) =>
        SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(csc.RowCount, csc.ColumnCount, csc.NonZerosCount, csc.RowIndices, csc.ColumnPointers, csc.Values);

    static double FrobeniusOf(CompressedColumnStorage<double> csc) => Math.Sqrt(TensorPrimitives.SumOfSquares<double>(csc.Values));

    static IEnumerable<(int Row, int Column, double Value)> Indexed(int[] rows, int[] columns, double[] values) =>
        toSeq(values).Map((value, index) => (rows[index], columns[index], value));
}
```


## [3]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, plus the `ShardPlan` block-decomposition column the dense GEMM reads.
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` (local `Matrix<double>.Multiply` leaf) · `Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentAddress, ComputeReceipt.Factorization Receipt)` the per-block join carrier.
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` is the matmul lowering; the convolution overload `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan)` lowers Conv1D/Conv2D/Conv3D through `Im2Col` patch projection then one GEMM; `Fin<T>` aborts with `<lowering-row-miss>` only on a row outside the bound matrix set; the pooling entrypoint takes its window as the span argument.
- Auto: the tensor-lane `MatMul`/`Conv*`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider, each `Conv*` row lowers through the `Im2Col` patch projection that flattens every receptive field to a column then one `Matrix<double>.Multiply` GEMM against the reshaped kernel, and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; the `Im2Col` patch gather runs `ParallelHelper.For2D` over the `(outputPosition × channel)` rectangle writing each receptive field into the dense patch plane projected as `Span2D<double>` so the embarrassingly-parallel gather rides the owned parallel-kernel row; the `ShardPlan.Single` leaf runs the local `Matrix<double>.Multiply`, and the `ShardPlan.Blocked` fan-out partitions the GEMM into `Tile`-high row-blocks through `ParallelHelper.ForEach` over the row-block sweep, dials the EXISTING `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub once per block under the block's `WithDeadline`/`WithCancellationToken` call options (each block building a `SolveRequest` from its row-block and the active `FactorizationKind`), content-addresses every sub-block by writing the `SolveRequest` once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width then `XxHash128` against the Persistence `ModelResultIndex` so a re-run reuses computed blocks, joins the per-node `SolveResponse` solutions into the result via the associative `ShardBlock.Join` `SetSubMatrix` over a private join target, and aggregates each sub-block's `Factorization` receipt — `Traverse`-collected on the `Fin<Matrix<double>>` rail so a single failed shard aborts the join.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and the `Blocked` fan-out aggregates one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the per-node `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc`, or `Substrate.CpuTensor` on a content-address cache hit) — the shard count is the block count and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel directly and the `Conv1D`/`Conv2D`/`Conv3D` rows inherit it through the `Im2Col` patch projection, so the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; `Im2Col` enumerates each output spatial position over the `ConvWindow` stride/padding/dilation lattice through `ParallelHelper.For2D` writing into the patch plane's `GetRowSpan`/`Span2D` projection, gathers the dilated receptive field across every channel into one patch row, and the patch matrix `[outPositions × Channels·KernelVolume]` multiplies the reshaped kernel `[Channels·KernelVolume × Filters]` in one `Matrix<double>.Multiply` whose tolerance is the `MatMul` proof the convolution row inherits through its `ToleranceClass.Tight` column — the `ParallelHelper.For2D` patch projection is this lane's named statement seam and a managed nested `Enumerable.Range` gather with `patch[i,j] =` mutation outside the parallel row is the deleted form; a Conv row routed without a `ConvWindow` (the matmul overload) returns the `<lowering-row-miss>` Fin.Fail because the geometry is absent; the `Blocked` shard fan-out is a row-block partition over the dense fold dialing each row-block sub-solve through the EXISTING `Substrate.RemoteGrpc` `ComputeService.ComputeServiceClient` stub and the `Solve` rpc owned by `Runtime/channels#PROTO_VOCABULARY` by reference — the `Blocked` case carries the stub, provider, kind, reuse index, correlation, clock, deadline, and cancellation token as constructor columns so the arm is a real dial with a per-call `WithDeadline`/`WithCancellationToken` bound derived from the clock and budget, the channel pins `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` against the payload cap, and a local-only tile loop, an unbounded dial with no deadline, or an uncapped channel is the named defect; the RHS rides the wire through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` no-copy because the interior-owned RHS outlives the request, the request content-addresses by writing once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width rather than a throwaway `ToByteArray()` per sub-block, and the `SolveResponse` solution span casts into `Build.Dense` directly — a full `CopyFrom`/`ToByteArray`/`ToArray` copy per shard is the deleted allocation; a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter` or a second substrate is the deleted form; each sub-block keys on the streamed-`SolveRequest` `XxHash128` folded against the provider `SolveDedupKey` against the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam by reference so a re-run reuses computed blocks (the cache-hit receipt carries `Substrate.CpuTensor`, the dialed receipt `Substrate.RemoteGrpc`), and the join writes each `ShardBlock` through `SetSubMatrix` into a private per-fan-out target — a shared mutable accumulator threaded through the per-shard `Map` is the named race defect; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

```csharp signature
public sealed record ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentAddress, ComputeReceipt.Factorization Receipt) {
    public static ShardBlock Join(Matrix<double> target, ShardBlock block) {
        target.SetSubMatrix(block.Start, 0, block.Solution);
        return block;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShardPlan {
    private ShardPlan() { }

    public sealed record Single : ShardPlan;
    public sealed record Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel) : ShardPlan;

    public Fin<Matrix<double>> Lower(Matrix<double> left, Matrix<double> right) =>
        Switch(
            state: (Left: left, Right: right),
            single: static s => Fin.Succ(s.Left.Multiply(s.Right)),
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
        var rhsBytes = MemoryMarshal.AsBytes<double>(right.ToColumnMajorArray()).ToArray();
        var request = new SolveRequest {
            Matrix = GeometryPayload.OfDense(rowBlock),
            Rhs = UnsafeByteOperations.UnsafeWrap(rhsBytes),
            FactorizationKind = plan.Kind.Key,
            SparseFormat = string.Empty,
            ShardTile = plan.Tile,
        };
        var address = Digest(request, plan.Provider.SolveDedupKey((UInt128)start));
        var dialedAt = plan.Clock.GetCurrentInstant();
        return plan.Reuse.Lookup(address)
            .Match(
                Some: response => Fin.Succ(Materialize(response, address, start, height, right.ColumnCount, Substrate.CpuTensor, Duration.Zero, plan)),
                None: () => Dial(plan, request)
                    .Map(response => {
                        plan.Reuse.Publish(address, response);
                        return Materialize(response, address, start, height, right.ColumnCount, Substrate.RemoteGrpc, plan.Clock.GetCurrentInstant() - dialedAt, plan);
                    }));
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
        Try.lift(() => plan.Compute.Solve(request, new CallOptions(new Metadata { { "rasm-correlation", plan.Correlation.Value } })
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
    static readonly FrozenSet<TensorOpFamily> ConvRows = new[] {
        TensorOpFamily.Conv1D, TensorOpFamily.Conv2D, TensorOpFamily.Conv3D,
    }.ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> MatrixRows = new[] {
        TensorOpFamily.MatMul, TensorOpFamily.Conv1D, TensorOpFamily.Conv2D, TensorOpFamily.Conv3D,
    }.ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> PoolRows = new[] {
        TensorOpFamily.MaxPool, TensorOpFamily.AvgPool, TensorOpFamily.GlobalAvgPool,
    }.ToFrozenSet();

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        row == TensorOpFamily.MatMul ? plan.Lower(left, right)
        : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan) =>
        ConvRows.Contains(row) && window.Rank == (int)(row.Key[^2] - '0')
            ? plan.Lower(Im2Col(input, window), kernel)
            : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    static Matrix<double> Im2Col(Matrix<double> input, ConvWindow window) {
        int[] extents = window.OutputExtents;
        var patch = new double[window.OutputPositions, window.PatchWidth];
        var plane = patch.AsSpan2D();
        var gather = new PatchGather(input, window, extents, plane);
        ParallelHelper.For2D(0, window.OutputPositions, 0, window.Channels, in gather);
        return Matrix<double>.Build.DenseOfArray(patch);
    }

    readonly struct PatchGather(Matrix<double> input, ConvWindow window, int[] extents, Span2D<double> plane) : IAction2D {
        public void Invoke(int position, int channel) {
            int[] origin = Unravel(position, extents);
            var row = plane.GetRowSpan(position);
            for (int tap = 0; tap < window.KernelVolume; tap++) {
                int[] offset = Unravel(tap, window.Kernel);
                int[] coords = toSeq(Enumerable.Range(0, window.Rank))
                    .Map(axis => origin[axis] * window.Stride[axis] + offset[axis] * window.Dilation[axis] - window.Padding[axis])
                    .ToArray();
                row[channel * window.KernelVolume + tap] =
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
        row == TensorOpFamily.MaxPool ? Fin.Succ(TensorPrimitives.Max(window))
        : row == TensorOpFamily.AvgPool || row == TensorOpFamily.GlobalAvgPool ? Fin.Succ(TensorPrimitives.Sum(window) / window.Length)
        : Fin.Fail<double>(ComputeFault.Create($"<pool-row-miss:{row.Key}>"));

    public static bool Lowers(TensorOpFamily row) => MatrixRows.Contains(row) || PoolRows.Contains(row);
}
```


## [4]-[RESEARCH]

- [SHARD_FANOUT]: the `ShardPlan.Blocked` fan-out dials the `Runtime/channels#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub by reference, builds `SolveRequest` field-for-field (`matrix`/`rhs`/`factorization_kind`/`sparse_format`/`shard_tile`), no-copy-wraps the RHS through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)`, content-addresses each row-block by writing the request once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent folded through `XxHash128.HashToUInt128` against the provider `SolveDedupKey` and the Persistence `ModelResultIndex` for sub-block reuse, dials under a per-call `WithDeadline`/`WithCancellationToken` bound from the clock and budget, joins the per-node `SolveResponse` solutions via the associative `ShardBlock.Join` `SetSubMatrix` into a private join target, and aggregates the per-shard `Factorization` receipts on the `Fin<Matrix<double>>` rail. The open leaf is the live in-host stub dial: the Grpc.Tools-compiled `ComputeService` client and the `Solve`-stub call resolve only inside the running integrated host plugin ALC, so `ShardPlan.SubSolve` against the live stub is the cross-lane probe that grounds the fan-out; the `SolveRequest`/`SolveResponse` field shapes (`matrix=1 GeometryPayload`, `rhs=2 bytes`, `factorization_kind=3 string`, `sparse_format=4 string`, `shard_tile=5 int32`; `solution=1 bytes`, `provider=2 string`, `decomposition=3 string`, `rows=4 int64`, `cols=5 int64`, `nnz=6 int64`) are the `Runtime/channels#PROTO_VOCABULARY` rows consumed by reference, and the `GeometryPayload.OfDense(Matrix<double>)` dense envelope and the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam compose their owning lanes.
