# [COMPUTE_NUMERIC_LANE]

Rasm.Compute numeric lane: BLAS-class dense and sparse linear algebra over the admitted MathNet provider stack, the RID-keyed `LinearProvider` availability table that selects native MKL or OpenBLAS where the asset exists and falls back to managed on osx-arm64, the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `Factorization` decomposition union collapsing LU, QR, Cholesky, SVD, and EVD to one solve admission, and the `KernelLowering` binding table that gives the tensor-lane matrix and structural rows a real kernel. The page owns the `LinearProvider`, `SparseFormat`, and `Factorization` axes, the `DenseOps`/`SparseOps` solve folds, the `KernelLowering` table, and the `ShardPlan` fan-out column, composing the tensor-lane operation rows, the `BenchmarkRow` claim authority, the `Substrate.RemoteGrpc` row, and the AppHost clock and CPU-budget ports as settled vocabulary.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                |
| :-----: | :--------------- | :------------------------------------------------------------------- |
|   [1]   | DENSE_ALGEBRA    | RID-keyed provider table; dense GEMM/solve fold; factorization union |
|   [2]   | SPARSE_SOLVE     | Sparse-format ingestion axis; direct and iterative sparse solve      |
|   [3]   | KERNEL_LOWERING  | Tensor matrix/structural rows lower onto real GEMM/im2col/pool       |
|   [4]   | PROVIDER_CLAIMS  | Provider rank reads benchmark claims; every solve emits a receipt    |

## [2]-[DENSE_ALGEBRA]

- Owner: `NumericKeyPolicy` ordinal accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed provider-availability rows; `DenseOps` GEMM/solve fold over MathNet `Matrix<double>`; `Factorization` `[Union]` one-case-per-decomposition collapsing to one solve admission.
- Cases: `LinearProvider` rows managed · native-mkl · native-openblas; `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd`.
- Entry: `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` — `Fin<T>` aborts on a dimension mismatch or a non-SPD Cholesky reject; `Solve` over the resulting case threads the active provider with zero call-site selection.
- Auto: `LinearProvider.Select` runs once at composition and returns the highest-rank available provider per RID, binding `LinearAlgebraControl.Provider` so every `Matrix<double>.Multiply` and factorization routes through the chosen `ILinearAlgebraProvider`; `Available` reads the RID plus the `Control.TryUse*` boolean probe so a missing native asset degrades to the next row instead of throwing; managed is the universal terminal.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new native provider is one `LinearProvider` row with its RID predicate, rank, and `Control.TryUse*` probe; a new decomposition is one `Factorization` case with its `FactorizationKind` row; zero new surface.
- Boundary: the decomposition union is `Factorization` and the per-solve receipt case is `ComputeReceipt.Factorization` — distinct C# symbols, the unqualified `Factorization` inside `Numeric/Lane.cs` is always the union; `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`, `DenseMatrix`, or matrix-wrapper face is the deleted form mirroring the tensor-lane no-`TensorService` law; provider selection runs ONCE through `LinearProvider.Select` binding `LinearAlgebraControl.Provider`, and a per-call-site `Control.UseNativeMKL()` is the named defect mirroring the model-lane thread-count law; MKL native assets are x64-only so the `native-mkl` row carries a win-x64/linux-x64 RID predicate and managed is the only osx-arm64 lane; the `Try*` provider probes return `false` rather than throwing, so `Available` never lifts an exception into the rail; provider rank reads `BenchmarkRow.Claim` and never a static default, so a native lane wins only behind a fingerprint-matched claim.

```csharp signature
public sealed class NumericKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

public static class LinearProviderProbes {
    public static bool Managed() => true;

    public static bool NativeMkl() =>
        (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        && RuntimeInformation.ProcessArchitecture is Architecture.X64
        && Control.TryUseNativeMKL();

    public static bool NativeOpenBlas() =>
        (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        && RuntimeInformation.ProcessArchitecture is Architecture.X64
        && Control.TryUseNativeOpenBLAS();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class LinearProvider {
    public static readonly LinearProvider Managed = new("managed", rank: 0, probe: LinearProviderProbes.Managed, activate: static () => Control.UseManaged());
    public static readonly LinearProvider NativeMkl = new("native-mkl", rank: 2, probe: LinearProviderProbes.NativeMkl, activate: static () => Control.UseNativeMKL());
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
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — `Fin<T>` aborts on a length or bound mismatch; `Solve` over the ingested CSR storage dispatches the direct or iterative route by the case row.
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
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (designed-only frontier) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ShardPlan` cases `Single` · `Blocked(int Tile)` (row-block partition).
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` — `Fin<T>` aborts with `<lowering-row-miss>` on any row whose kernel arm is not yet landed, including the designed-only convolution rows; the pooling entrypoint takes its window as the span argument.
- Auto: the tensor-lane `MatMul`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; the convolution rows are the designed-only im2col-patch-projection frontier that fail until the im2col arm lands; a `Blocked` shard plan partitions the GEMM into row-blocks each routed through `Substrate.RemoteGrpc` and joined by the `Factorization`-receipt fold, with single-node solve as the `Single` leaf.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and a `Blocked` fan-out emits one shard count per node; the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel the moment its lowering row lands, replacing the tensor-lane `<kernel-row-miss>` Fin.Fail; the `Conv1D`/`Conv2D`/`Conv3D` rows are the designed-only im2col-patch-projection frontier, so `Lower` returns the `<lowering-row-miss>` Fin.Fail for a convolution row until the im2col arm lands and the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; the `Blocked` shard fan-out is a row-block partition column on the dense fold dispatching each row-block sub-solve through the existing `Substrate.RemoteGrpc` and the `Solve` rpc owned by `remote-lane#PROTO_VOCABULARY` — a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter`, a second substrate, or a block-decomposition routing owner is the deleted form; the sub-block result keys on a sub-block content-address by reference to the Persistence model-result index so a re-run reuses computed blocks; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

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

public static class KernelLowering {
    static readonly FrozenSet<TensorOpFamily> MatrixRows = new[] {
        TensorOpFamily.MatMul, TensorOpFamily.Conv1D, TensorOpFamily.Conv2D, TensorOpFamily.Conv3D,
    }.ToFrozenSet();

    static readonly FrozenSet<TensorOpFamily> PoolRows = new[] {
        TensorOpFamily.MaxPool, TensorOpFamily.AvgPool, TensorOpFamily.GlobalAvgPool,
    }.ToFrozenSet();

    public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        row == TensorOpFamily.MatMul ? Fin.Succ(plan.Lower(left, right))
        : Fin.Fail<Matrix<double>>(ComputeFault.Create($"<lowering-row-miss:{row.Key}>"));

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

- [NATIVE_BLAS_EXECUTION]: the native-mkl and native-openblas dense GEMM and the CSparse direct sparse solve are member-shape FINALIZED and managed-provider-proven on arm64; native MKL execution is x64-only behind the documented RID gate (no osx-arm64 asset) and the native-openblas osx-arm64 asset presence is the live-host probe; the row predicates and `Control.TryUse*` boolean fall-throughs are the residual host-asset confirmation, never an open member spelling.
