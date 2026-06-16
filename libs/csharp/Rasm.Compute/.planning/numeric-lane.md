# [COMPUTE_NUMERIC_LANE]

Rasm.Compute numeric lane: BLAS-class dense and sparse linear algebra over the admitted MathNet provider stack, the RID-keyed `LinearProvider` availability table that selects native OpenBLAS where an osx-arm64 asset resolves and falls back to the managed terminal otherwise, the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `Factorization` decomposition union collapsing LU, QR, Cholesky, SVD, and EVD to one solve admission, and the `KernelLowering` binding table that gives the tensor-lane matrix and structural rows a real kernel. The page owns the `LinearProvider`, `SparseFormat`, and `Factorization` axes, the `DenseOps`/`SparseOps` solve folds, the `KernelLowering` table, and the `ShardPlan` fan-out column, composing the tensor-lane operation rows, the `BenchmarkRow` claim authority, the `Substrate.RemoteGrpc` row, and the AppHost clock and CPU-budget ports as settled vocabulary.

Wire posture: this page is HOST-LOCAL and carries no TS_PROJECTION cluster — no numeric owner crosses the browser or peer wire directly. A distributed solve crosses solely through the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc (`SolveRequest`/`SolveResponse`), which the `ShardPlan.Blocked` row-block sub-solve dials by reference; the `Solve` rpc and its `ComputeServiceShape` `MethodShape` are the single wire surface, owned and projected at `remote-lane`, never re-projected here. `Matrix<double>`, `SparseCompressedRowMatrixStorage<double>`, and the `Factorization` union are interior numeric types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                               |
| :-----: | :-------------- | :------------------------------------------------------------------- |
|   [1]   | DENSE_ALGEBRA   | RID-keyed provider table; dense GEMM/solve fold; factorization union |
|   [2]   | SPARSE_SOLVE    | Sparse-format ingestion axis; direct and iterative sparse solve      |
|   [3]   | KERNEL_LOWERING | Tensor matrix/structural rows lower onto real GEMM/im2col/pool       |
|   [4]   | PROVIDER_CLAIMS | Provider rank reads benchmark claims; every solve emits a receipt    |

## [2]-[DENSE_ALGEBRA]

- Owner: `NumericKeyPolicy` ordinal accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed provider-availability rows; `DenseOps` GEMM/solve fold over MathNet `Matrix<double>`; `Factorization` `[Union]` one-case-per-decomposition collapsing to one solve admission.
- Cases: `LinearProvider` rows managed · native-openblas; `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd`.
- Entry: `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` — `Fin<T>` aborts on a dimension mismatch or a non-SPD Cholesky reject; `Solve` over the resulting case threads the active provider with zero call-site selection.
- Auto: `LinearProvider.Select` runs once at composition and returns the highest-rank available provider per RID, binding `LinearAlgebraControl.Provider` so every `Matrix<double>.Multiply` and factorization routes through the chosen `ILinearAlgebraProvider`; `Available` reads the RID plus the `Control.TryUse*` boolean probe so a missing native asset degrades to the next row instead of throwing; managed is the universal terminal.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new native provider is one `LinearProvider` row with its RID predicate, rank, and `Control.TryUse*` probe; a new decomposition is one `Factorization` case with its `FactorizationKind` row; zero new surface.
- Boundary: the decomposition union is `Factorization` and the per-solve receipt case is `ComputeReceipt.Factorization` — distinct C# symbols, the unqualified `Factorization` inside `Numeric/Lane.cs` is always the union; `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`, `DenseMatrix`, or matrix-wrapper face is the deleted form mirroring the tensor-lane no-`TensorService` law; provider selection runs ONCE through `LinearProvider.Select` binding `LinearAlgebraControl.Provider`, and a per-call-site `Control.UseNativeOpenBLAS()` is the named defect mirroring the model-lane thread-count law; the x64-only MKL row is dropped from the live osx-arm64 axis (no osx-arm64 MKL asset, its `Control.UseNativeMKL` member spelling is the win/linux-x64 design record re-entering as one row only behind a MKL-carrying RID) so the axis is the managed terminal plus the `native-openblas` row whose `Control.TryUseNativeOpenBLAS()` probe returns `true` only where an osx-arm64 OpenBLAS asset resolves and otherwise degrades to managed; the `Try*` provider probes return `false` rather than throwing, so `Available` never lifts an exception into the rail; provider rank reads `BenchmarkRow.Claim` and never a static default, so a native lane wins only behind a fingerprint-matched claim; the provider-determinism contract holds that managed, native-MKL, and native-OpenBLAS diverge at the bit level, so `DeterminismTag` names the active `ILinearAlgebraProvider` type and the degree-of-parallelism and `SolveDedupKey` folds that tag into the content-addressed solve-dedup fingerprint — a solve result cached under one provider never serves a request that resolved a different provider, and the alternative pin-one-provider posture binds `Managed` as the dedup-stable terminal when a deployment requires bit-identical reproducibility; a solve-dedup key that omits the provider tag is the named correctness defect because a cross-provider cache hit returns bit-divergent numbers.

```csharp signature
public sealed class NumericKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

public static class LinearProviderProbes {
    public static bool Managed() => true;

    public static bool NativeOpenBlas() => Control.TryUseNativeOpenBLAS();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class LinearProvider {
    public static readonly LinearProvider Managed = new("managed", rank: 0, probe: LinearProviderProbes.Managed, activate: static () => Control.UseManaged());
    public static readonly LinearProvider NativeOpenBlas = new("native-openblas", rank: 1, probe: LinearProviderProbes.NativeOpenBlas, activate: static () => Control.UseNativeOpenBLAS());

    private readonly Func<bool> probe;
    private readonly Action activate;

    public int Rank { get; }

    public bool Available => probe();

    public static LinearProvider Select(Option<BenchmarkRow> claim) =>
        toSeq(Items)
            .Filter(static row => row.Available)
            .OrderByDescending(row => claim.Map(c => StringComparer.Ordinal.Equals(c.Route, row.Key) ? int.MaxValue : row.Rank).IfNone(row.Rank))
            .HeadOrNone()
            .Map(static row => { row.activate(); return row; })
            .IfNone(static () => { Managed.activate(); return Managed; });

    public string DeterminismTag =>
        $"{Key}:{Control.LinearAlgebraProvider.GetType().Name}:{Control.MaxDegreeOfParallelism}";

    public static UInt128 SolveDedupKey(LinearProvider provider, UInt128 problemDigest) =>
        XxHash128.HashToUInt128(MemoryMarshal.AsBytes(provider.DeterminismTag.AsSpan()), unchecked((long)problemDigest));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class FactorizationKind {
    public static readonly FactorizationKind Lu = new("lu");
    public static readonly FactorizationKind Qr = new("qr");
    public static readonly FactorizationKind Cholesky = new("cholesky");
    public static readonly FactorizationKind Svd = new("svd");
    public static readonly FactorizationKind Evd = new("evd");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Factorization {
    private Factorization() { }

    public sealed record Lu(LU<double> Decomposition) : Factorization;

    public sealed record Qr(QR<double> Decomposition) : Factorization;

    public sealed record Cholesky(Cholesky<double> Decomposition) : Factorization;

    public sealed record Svd(Svd<double> Decomposition) : Factorization;

    public sealed record Evd(Evd<double> Decomposition) : Factorization;

    public FactorizationKind Kind =>
        Switch(
            lu: static _ => FactorizationKind.Lu,
            qr: static _ => FactorizationKind.Qr,
            cholesky: static _ => FactorizationKind.Cholesky,
            svd: static _ => FactorizationKind.Svd,
            evd: static _ => FactorizationKind.Evd);

    public Vector<double> Solve(Vector<double> rhs) =>
        Switch(
            state: rhs,
            lu: static (r, f) => f.Decomposition.Solve(r),
            qr: static (r, f) => f.Decomposition.Solve(r),
            cholesky: static (r, f) => f.Decomposition.Solve(r),
            svd: static (r, f) => f.Decomposition.Solve(r),
            evd: static (r, f) => f.Decomposition.Solve(r));
}

public static class DenseOps {
    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        kind switch {
            _ when kind == FactorizationKind.Lu => Fin.Succ<Factorization>(new Factorization.Lu(matrix.LU())),
            _ when kind == FactorizationKind.Qr => Fin.Succ<Factorization>(new Factorization.Qr(matrix.QR())),
            _ when kind == FactorizationKind.Cholesky =>
                matrix.RowCount == matrix.ColumnCount
                    ? Fin.Succ<Factorization>(new Factorization.Cholesky(matrix.Cholesky()))
                    : Fin.Fail<Factorization>(new ComputeFault.ModelRejected($"<cholesky-non-square:{matrix.RowCount}x{matrix.ColumnCount}>")),
            _ when kind == FactorizationKind.Svd => Fin.Succ<Factorization>(new Factorization.Svd(matrix.Svd())),
            _ when kind == FactorizationKind.Evd => Fin.Succ<Factorization>(new Factorization.Evd(matrix.Evd())),
            _ => Fin.Fail<Factorization>(ComputeFault.Create($"<factorization-kind-miss:{kind.Key}>")),
        };

    public static Matrix<double> Gemm(Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        plan.Lower(left, right);

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, Factorization factor, int rows, int cols, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, factor.Kind.Key, rows, cols, 0L, "dense") {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };
}
```

## [3]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows; `SparseOps` direct-and-iterative sparse-solve fold over the CSR-backed MathNet storage and CSparse direct factorizations.
- Cases: `SparseFormat` rows csr · csc · coo · dok.
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — `Fin<T>` aborts on a length or bound mismatch; `SolveDirect` factors a CSparse CSC and `SolveIterative` runs the MathNet `IIterativeSolver<double>` under an `Iterator<double>` stop-criteria control, returning the field plus the real iteration count, final relative residual, and converged flag.
- Auto: every format row maps to one CSR ingestion conversion — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type, never four storage types; direct solves factor a CSparse `CompressedColumnStorage<double>` through `SparseCholesky`/`SparseLU`/`SparseQR` and iterative solves run `BiCgStab`/`GpBiCg`/`TFQMR` under an `Iterator<double>` stop-criteria control.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition or solver kind, row and column extents, the non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `SparseFormat` row mapping onto its CSR conversion; a new sparse solver is one row on the direct or iterative dispatch; zero new surface.
- Boundary: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage — csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner for each format is the deleted form; the `Values`/`ColumnIndices`/`RowPointers` fields back the receipt nnz count from `ValueCount` and never a re-scan; CSparse direct factorizations consume a CSC `CompressedColumnStorage<double>` so the CSR-to-CSC handoff is one transpose at the solver edge, not a stored second copy; the iterative `Iterator<double>` stop criteria bind from the policy row and never a per-call literal; the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

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

public static class SparseOps {
    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) =>
        minorIndices.Length == values.Length
            ? format switch {
                _ when format == SparseFormat.Csr => Fin.Succ(SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(rows, columns, values.Length, majorIndices, minorIndices, values)),
                _ when format == SparseFormat.Csc => Fin.Succ(SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(rows, columns, values.Length, minorIndices, majorIndices, values)),
                _ when format == SparseFormat.Coo => Fin.Succ(SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(rows, columns, values.Length, majorIndices, minorIndices, values)),
                _ when format == SparseFormat.Dok => Fin.Succ(SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(rows, columns, Indexed(majorIndices, minorIndices, values))),
                _ => Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<sparse-format-miss:{format.Key}>")),
            }
            : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(new ComputeFault.PayloadOverBounds($"sparse-values:{values.Length}:{minorIndices.Length}"));

    public static Fin<double[]> SolveDirect(SparseCompressedRowMatrixStorage<double> csr, FactorizationKind kind, double[] rhs, CSparse.ColumnOrdering ordering) {
        var csc = CSparse.Storage.CompressedColumnStorage<double>.OfIndexed(csr.RowCount, csr.ColumnCount, Entries(csr));
        var result = new double[rhs.Length];
        return kind switch {
            _ when kind == FactorizationKind.Cholesky => Run(() => CSparse.Double.Factorization.SparseCholesky.Create(csc, ordering).Solve(rhs, result), result),
            _ when kind == FactorizationKind.Lu => Run(() => CSparse.Double.Factorization.SparseLU.Create(csc, ordering, 1.0).Solve(rhs, result), result),
            _ when kind == FactorizationKind.Qr => Run(() => CSparse.Double.Factorization.SparseQR.Create(csc, ordering).Solve(rhs, result), result),
            _ => Fin.Fail<double[]>(ComputeFault.Create($"<sparse-direct-miss:{kind.Key}>")),
        };
    }

    public static Fin<(double[] Field, int Iterations, double Residual, bool Converged)> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, string method, double[] rhs, int maxIterations, double tolerance) =>
        Try.lift(() => {
            var matrix = SparseMatrix.OfStorage(csr);
            var b = Vector<double>.Build.DenseOfArray(rhs);
            var x = Vector<double>.Build.Dense(rhs.Length);
            var iterator = new Iterator<double>(new IterationCountStopCriterion<double>(maxIterations), new ResidualStopCriterion<double>(tolerance));
            IIterativeSolver<double> solver = method switch {
                "gmres" => new GpBiCg(),
                "tfqmr" => new TFQMR(),
                _ => new BiCgStab(),
            };
            solver.Solve(matrix, b, x, iterator, new DiagonalPreconditioner());
            double residual = (matrix.Multiply(x) - b).L2Norm() / Math.Max(1.0, b.L2Norm());
            return (x.ToArray(), iterator.NumberOfIterations, residual, iterator.Status == IterationStatus.Converged);
        }).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorizationKind kind, SparseCompressedRowMatrixStorage<double> csr, SparseFormat format, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, csr.RowCount, csr.ColumnCount, csr.Values.Length, format.Key) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static IEnumerable<(int Row, int Column, double Value)> Indexed(int[] rows, int[] columns, double[] values) =>
        toSeq(values).Map((value, index) => (rows[index], columns[index], value));

    static IEnumerable<(int Row, int Column, double Value)> Entries(SparseCompressedRowMatrixStorage<double> csr) =>
        toSeq(Enumerable.Range(0, csr.RowCount)).Bind(row =>
            toSeq(Enumerable.Range(csr.RowPointers[row], csr.RowPointers[row + 1] - csr.RowPointers[row]))
                .Map(slot => (row, csr.ColumnIndices[slot], csr.Values[slot])));

    static Fin<double[]> Run(Action solve, double[] result) {
        solve();
        return Fin.Succ(result);
    }
}
```

## [4]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, plus the `ShardPlan` block-decomposition column the dense GEMM reads.
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` · `Blocked(int Tile)` (row-block partition).
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` is the matmul lowering; the convolution overload `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan)` lowers Conv1D/Conv2D/Conv3D through `Im2Col` patch projection then one GEMM; `Fin<T>` aborts with `<lowering-row-miss>` only on a row outside the bound matrix set; the pooling entrypoint takes its window as the span argument.
- Auto: the tensor-lane `MatMul`/`Conv*`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider, each `Conv*` row lowers through the `Im2Col` patch projection that flattens every receptive field to a column then one `Matrix<double>.Multiply` GEMM against the reshaped kernel, and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; a `Blocked` shard plan partitions the GEMM into row-blocks each routed through `Substrate.RemoteGrpc` and joined by the `Factorization`-receipt fold, with single-node solve as the `Single` leaf.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and a `Blocked` fan-out emits one shard count per node; the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel directly and the `Conv1D`/`Conv2D`/`Conv3D` rows inherit it through the `Im2Col` patch projection, so the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; `Im2Col` enumerates each output spatial position over the `ConvWindow` stride/padding/dilation lattice, gathers the dilated receptive field across every channel into one patch row, and the patch matrix `[outPositions × Channels·KernelVolume]` multiplies the reshaped kernel `[Channels·KernelVolume × Filters]` in one `Matrix<double>.Multiply` whose tolerance is the `MatMul` proof the convolution row inherits through its `ToleranceClass.Tight` column; a Conv row routed without a `ConvWindow` (the matmul overload) returns the `<lowering-row-miss>` Fin.Fail because the geometry is absent, never silently correlating; the `Blocked` shard fan-out is a row-block partition column on the dense fold dispatching each row-block sub-solve through the existing `Substrate.RemoteGrpc` and the `Solve` rpc owned by `remote-lane#PROTO_VOCABULARY` — a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter`, a second substrate, or a block-decomposition routing owner is the deleted form; the sub-block result keys on a sub-block content-address by reference to the Persistence model-result index so a re-run reuses computed blocks; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShardPlan {
    private ShardPlan() { }

    public sealed record Single : ShardPlan;

    public sealed record Blocked(int Tile) : ShardPlan;

    public Matrix<double> Lower(Matrix<double> left, Matrix<double> right) =>
        Switch(
            state: (Left: left, Right: right),
            single: static s => s.Left.Multiply(s.Right),
            blocked: static (s, plan) => Tiled(s.Left, s.Right, plan.Tile));

    static Matrix<double> Tiled(Matrix<double> left, Matrix<double> right, int tile) =>
        toSeq(Enumerable.Range(0, (left.RowCount + tile - 1) / tile))
            .Fold(Matrix<double>.Build.Dense(left.RowCount, right.ColumnCount), (acc, block) => {
                int start = block * tile;
                int height = Math.Min(tile, left.RowCount - start);
                acc.SetSubMatrix(start, 0, left.SubMatrix(start, height, 0, left.ColumnCount).Multiply(right));
                return acc;
            });
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
        row == TensorOpFamily.MatMul ? Fin.Succ(plan.Lower(left, right))
        : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan) =>
        ConvRows.Contains(row) && window.Rank == (int)(row.Key[^2] - '0')
            ? Fin.Succ(plan.Lower(Im2Col(input, window), kernel))
            : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

    static Matrix<double> Im2Col(Matrix<double> input, ConvWindow window) {
        int[] extents = window.OutputExtents;
        var patch = Matrix<double>.Build.Dense(window.OutputPositions, window.PatchWidth);
        toSeq(Enumerable.Range(0, window.OutputPositions)).Iter(position => {
            int[] origin = Unravel(position, extents);
            toSeq(Enumerable.Range(0, window.Channels)).Iter(channel =>
                toSeq(Enumerable.Range(0, window.KernelVolume)).Iter(tap => {
                    int[] offset = Unravel(tap, window.Kernel);
                    int[] coords = toSeq(Enumerable.Range(0, window.Rank))
                        .Map(axis => origin[axis] * window.Stride[axis] + offset[axis] * window.Dilation[axis] - window.Padding[axis])
                        .ToArray();
                    patch[position, channel * window.KernelVolume + tap] =
                        toSeq(coords).Zip(toSeq(window.Spatial)).ForAll(static pair => pair.First >= 0 && pair.First < pair.Second)
                            ? input[channel, Ravel(coords, window.Spatial)]
                            : 0d;
                }));
        });
        return patch;
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

## [5]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `BenchmarkRow.Claim` resolved at composition against the running fingerprint and the `ModelResultKey.RecencyHorizon` — so the chosen provider RID is claim-gated, never a static default; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `BenchmarkRow.Claim` owner and threaded in, never re-resolved here; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, and counts sharded sub-blocks by node.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations`, `rasm.compute.solve.residual`, and `rasm.compute.solve.shards` instruments are owned by `receipts-and-benchmarks#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here.
- Packages: Rasm.Persistence (project), LanguageExt.Core, BCL inbox
- Growth: a new claim dimension is one column on the existing `BenchmarkClaim`; a new solve instrument is one row on `ReceiptSurface.Instruments`; zero new surface.
- Boundary: provider rank is the `BenchmarkClaim` `Provider` column gated exactly like the SIMD and partition claims — a static native default beside the claim is the named defect; the claim is resolved by the Persistence `BenchmarkRow.Claim` owner against the recency horizon read by reference from the Persistence model-result index owner and threaded in, never re-resolved and never a second horizon; the solve and shard instruments live on the `ReceiptSurface.Instruments` stream and a second numeric-lane-local instrument owner is the deleted form.

## [6]-[RESEARCH]

- [ITERATIVE_SOLVE]: the `SparseOps.SolveIterative` body binds the catalogued `IIterativeSolver<double>.Solve(matrix, b, x, iterator)` and `Iterator<double>` control; the iterator construction and convergence-evidence member spellings — `ResidualStopCriterion<double>`/`IterationCountStopCriterion<double>` constructor arity, `Iterator<double>.Status` returning `IterationStatus.Converged`, `Iterator<double>.NumberOfIterations`, `DiagonalPreconditioner`, and `SparseMatrix.OfStorage(SparseCompressedRowMatrixStorage<double>)` — confirm against the admitted MathNet.Numerics 6.0.0-beta2 `MathNet.Numerics.LinearAlgebra.Solvers`/`.Double.Solvers` surface by tier-1 decompile; the verified iteration count and final relative residual fold into the `SolveResult` the `solver-and-optimization#SOLVE_CONTRACT` iterative route reads, replacing the prior direct-solve mislabel.
