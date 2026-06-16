# [COMPUTE_NUMERIC_LANE]

Rasm.Compute numeric lane: BLAS-class dense and sparse linear algebra over the admitted MathNet provider stack, the RID-keyed `LinearProvider` availability table that selects native OpenBLAS where an osx-arm64 asset resolves and falls back to the managed terminal otherwise, the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `IterativeMethod` closed solver-factory axis with the `IterationPolicy` tolerance/max-iter/preconditioner record, the `Factorization` decomposition union collapsing LU, QR, Cholesky, SVD, and EVD to one solve admission, and the `KernelLowering` binding table that gives the tensor-lane matrix and structural rows a real kernel. The page owns the `LinearProvider`, `SparseFormat`, `IterativeMethod`, and `Factorization` axes, the `DenseOps`/`SparseOps` solve, refinement, and ingestion/direct `FrozenDictionary` folds, the `KernelLowering` table, and the `ShardPlan` fan-out column, composing the tensor-lane operation rows, the `BenchmarkRow` claim authority, the `Substrate.RemoteGrpc` row, and the AppHost clock and CPU-budget ports as settled vocabulary.

Wire posture: this page is HOST-LOCAL and carries no TS_PROJECTION cluster — no numeric owner crosses the browser or peer wire directly. A distributed solve crosses solely through the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc (`SolveRequest`/`SolveResponse`), which the `ShardPlan.Blocked` row-block sub-solve dials by reference; the `Solve` rpc and its `ComputeServiceShape` `MethodShape` are the single wire surface, owned and projected at `remote-lane`, never re-projected here. `Matrix<double>`, `SparseCompressedRowMatrixStorage<double>`, and the `Factorization` union are interior numeric types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                               |
| :-----: | :-------------- | :------------------------------------------------------------------- |
|   [1]   | DENSE_ALGEBRA   | RID-keyed provider table; dense GEMM/solve fold; factorization union |
|   [2]   | SPARSE_SOLVE    | Sparse-format ingestion axis; direct and iterative sparse solve      |
|   [3]   | KERNEL_LOWERING | Tensor matrix/structural rows lower onto real GEMM/im2col/pool       |
|   [4]   | PROVIDER_CLAIMS | Provider rank reads benchmark claims; every solve emits a receipt    |

## [2]-[DENSE_ALGEBRA]

- Owner: `NumericKeyPolicy` ordinal accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed provider-availability rows carrying the `Control.TryUse*` probe delegate as an inline row column; `DenseOps` GEMM/solve/refinement fold over MathNet `Matrix<double>`, decomposition dispatch driven by one `Decomposers` `FrozenDictionary` factory fold; `Factorization` `[Union]` one-case-per-decomposition collapsing to one solve admission.
- Cases: `LinearProvider` rows managed · native-openblas (2); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` (5); `DenseOps.Decomposers` rows lu · qr · cholesky · svd · evd (5).
- Entry: `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` — `Fin<T>` aborts on a dimension mismatch or a non-SPD Cholesky reject — drives the `Decomposers` fold; `Solve` over the resulting case threads the active provider with zero call-site selection; `public static Fin<(Vector<double> Field, int Refinements, double Residual)> SolveRefined(Matrix<double> matrix, Vector<double> rhs, FactorizationKind kind, IterationPolicy policy)` factors once at single precision then folds the double-precision residual-refine step under the `IterationPolicy` tolerance/max-iter cap.
- Auto: `LinearProvider.Select` runs once at composition and returns the highest-rank available provider per RID, binding `LinearAlgebraControl.Provider` so every `Matrix<double>.Multiply` and factorization routes through the chosen `ILinearAlgebraProvider`; `Available` evaluates the inline `Control.TryUse*` probe delegate so a missing native asset degrades to the next row instead of throwing — the prior standalone `LinearProviderProbes` type is the deleted form, the probe is a `LinearProvider` row column; managed is the universal terminal; `Decompose` reads the `Decomposers` `FrozenDictionary` instead of a `kind switch` arm cascade.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new native provider is one `LinearProvider` row with its RID predicate, rank, and inline `Control.TryUse*` probe column; a new decomposition is one `Factorization` case plus one `Decomposers` `FrozenDictionary` row keyed by its `FactorizationKind`; zero new surface.
- Boundary: the decomposition union is `Factorization` and the per-solve receipt case is `ComputeReceipt.Factorization` — distinct C# symbols, the unqualified `Factorization` inside `Numeric/Lane.cs` is always the union; the `Decomposers` `FrozenDictionary` is the single decomposition data table and the prior `kind switch` arm cascade is the deleted form; mixed-precision iterative refinement is the `SolveRefined` fold — single-precision factor seed plus double-precision residual-refine fold under the `IterationPolicy` cap, never a parallel half-precision solver type; the standalone `LinearProviderProbes` type is inlined onto the `LinearProvider` rows as the probe-delegate column and a separate probe owner is the deleted form; `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`, `DenseMatrix`, or matrix-wrapper face is the deleted form mirroring the tensor-lane no-`TensorService` law; provider selection runs ONCE through `LinearProvider.Select` binding `LinearAlgebraControl.Provider`, and a per-call-site `Control.UseNativeOpenBLAS()` is the named defect mirroring the model-lane thread-count law; the x64-only MKL row is dropped from the live osx-arm64 axis (no osx-arm64 MKL asset, its `Control.UseNativeMKL` member spelling is the win/linux-x64 design record re-entering as one row only behind a MKL-carrying RID) so the axis is the managed terminal plus the `native-openblas` row whose `Control.TryUseNativeOpenBLAS()` probe returns `true` only where an osx-arm64 OpenBLAS asset resolves and otherwise degrades to managed; the `Try*` provider probes return `false` rather than throwing, so `Available` never lifts an exception into the rail; provider rank reads `BenchmarkRow.Claim` and never a static default, so a native lane wins only behind a fingerprint-matched claim; the provider-determinism contract holds that managed, native-MKL, and native-OpenBLAS diverge at the bit level, so `DeterminismTag` names the active `ILinearAlgebraProvider` type and the degree-of-parallelism and `SolveDedupKey` folds that tag into the content-addressed solve-dedup fingerprint — a solve result cached under one provider never serves a request that resolved a different provider, and the alternative pin-one-provider posture binds `Managed` as the dedup-stable terminal when a deployment requires bit-identical reproducibility; a solve-dedup key that omits the provider tag is the named correctness defect because a cross-provider cache hit returns bit-divergent numbers.

```csharp signature
public sealed class NumericKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.Ordinal;

    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class LinearProvider {
    public static readonly LinearProvider Managed = new("managed", rank: 0, probe: static () => true, activate: static () => Control.UseManaged());
    public static readonly LinearProvider NativeOpenBlas = new("native-openblas", rank: 1, probe: static () => Control.TryUseNativeOpenBLAS(), activate: static () => Control.UseNativeOpenBLAS());

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
    static readonly FrozenDictionary<FactorizationKind, Func<Matrix<double>, Fin<Factorization>>> Decomposers =
        new (FactorizationKind Kind, Func<Matrix<double>, Fin<Factorization>> Build)[] {
            (FactorizationKind.Lu, static m => Fin.Succ<Factorization>(new Factorization.Lu(m.LU()))),
            (FactorizationKind.Qr, static m => Fin.Succ<Factorization>(new Factorization.Qr(m.QR()))),
            (FactorizationKind.Cholesky, static m =>
                m.RowCount == m.ColumnCount
                    ? Fin.Succ<Factorization>(new Factorization.Cholesky(m.Cholesky()))
                    : Fin.Fail<Factorization>(new ComputeFault.ModelRejected($"<cholesky-non-square:{m.RowCount}x{m.ColumnCount}>"))),
            (FactorizationKind.Svd, static m => Fin.Succ<Factorization>(new Factorization.Svd(m.Svd()))),
            (FactorizationKind.Evd, static m => Fin.Succ<Factorization>(new Factorization.Evd(m.Evd()))),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Build);

    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        Decomposers.TryGetValue(kind, out var build)
            ? build(matrix)
            : Fin.Fail<Factorization>(ComputeFault.Create($"<factorization-kind-miss:{kind.Key}>"));

    public static Fin<Matrix<double>> Gemm(Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        plan.Lower(left, right);

    public static Fin<(Vector<double> Field, int Refinements, double Residual)> SolveRefined(Matrix<double> matrix, Vector<double> rhs, FactorizationKind kind, IterationPolicy policy) =>
        Decompose(matrix.Map(static value => (double)(float)value), kind).Map(seed => {
            var x = seed.Solve(rhs.Map(static value => (double)(float)value));
            return toSeq(Enumerable.Range(0, policy.MaxIterations)).Fold((Field: x, Refinements: 0, Residual: double.MaxValue), (acc, _) => {
                var residual = rhs - matrix.Multiply(acc.Field);
                double norm = residual.L2Norm() / Math.Max(1.0, rhs.L2Norm());
                return norm <= policy.Tolerance
                    ? acc
                    : (acc.Field + seed.Solve(residual), acc.Refinements + 1, norm);
            });
        });

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, Factorization factor, int rows, int cols, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, factor.Kind.Key, rows, cols, 0L, "dense") {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };
}
```

## [3]-[SPARSE_SOLVE]

- Owner: `SparseFormat` `[SmartEnum<string>]` ingestion-axis rows; `IterativeMethod` `[SmartEnum<string>]` closed solver-factory axis with the `IterationPolicy` record (tolerance · max-iter · preconditioner); `SparseOps` direct-and-iterative sparse-solve fold over the CSR-backed MathNet storage and CSparse direct factorizations, ingestion and direct dispatch each driven by one `FrozenDictionary` factory fold.
- Cases: `SparseFormat` rows csr · csc · coo · dok (4); `IterativeMethod` rows bicgstab · gpbicg · tfqmr (3); `SparseOps.DirectSolvers` rows cholesky · lu · qr (3); `SparseOps.Ingestors` rows csr · csc · coo · dok (4).
- Entry: `public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values)` — `Fin<T>` aborts on a length or bound mismatch; `SolveDirect` factors a CSparse CSC through the `DirectSolvers` fold and `SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy)` runs the `IterativeMethod`-selected MathNet `IIterativeSolver<double>` under the `IterationPolicy.Iterator()` stop-criteria control, returning the field plus the real iteration count, final relative residual, and converged flag — the prior raw-`string` method discriminant is the deleted form, the closed `IterativeMethod` SmartEnum carries the solver factory as a row column.
- Auto: every format row maps to one CSR ingestion conversion through the `Ingestors` `FrozenDictionary` fold — csr direct, csc through `OfCompressedSparseColumnFormat`, coo through `OfCoordinateFormat`, dok through `OfIndexedEnumerable` over the indexed-entry buffer — so the format axis is an ingestion discriminant over one storage type, never four storage types and never four switch arms; direct solves factor a CSparse `CompressedColumnStorage<double>` through the `DirectSolvers` `FrozenDictionary` fold binding `SparseCholesky`/`SparseLU`/`SparseQR`, and iterative solves run the `IterativeMethod` row's `Solver()` factory (`BiCgStab`/`GpBiCg`/`TFQMR`) under the `IterationPolicy.Iterator()` `Iterator<double>` stop-criteria control with the policy-row `Preconditioner()`.
- Receipt: every sparse solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition or solver kind, row and column extents, the non-zero count, and the source format key; emission rides the sink port.
- Packages: MathNet.Numerics, CSparse, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new ingestion path is one `Ingestors` row keyed by its `SparseFormat`; a new direct solver is one `DirectSolvers` row keyed by its `FactorizationKind`; a new iterative method is one `IterativeMethod` row carrying its `IIterativeSolver<double>` factory column; a new iteration knob is one column on the `IterationPolicy` record; zero new surface.
- Boundary: `SparseCompressedRowMatrixStorage<double>` is the only native MathNet sparse matrix storage — csc/coo/dok are ingestion conversions into CSR through the `Of*` factories and a parallel storage owner for each format is the deleted form; the `Ingestors` and `DirectSolvers` `FrozenDictionary` folds are the single data table each for format ingestion and direct factorization — the prior `format switch`/`kind switch` arm cascades are the deleted form, a fourth-plus near-identical `_ when` arm is the density defect collapsed here; the `Values`/`ColumnIndices`/`RowPointers` fields back the receipt nnz count from `ValueCount` and never a re-scan; CSparse direct factorizations consume a CSC `CompressedColumnStorage<double>` so the CSR-to-CSC handoff is one transpose at the solver edge, not a stored second copy; the iterative method is the closed `IterativeMethod` SmartEnum and a raw-`string` method discriminant beside it is the named defect, the `Iterator<double>` stop criteria and `IPreconditioner<double>` bind from the `IterationPolicy` row and never a per-call literal; the row-block partition over CSR is the `ShardPlan` fan-out column read by the solve, never a second routing owner.

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
public sealed partial class IterativeMethod {
    public static readonly IterativeMethod BiCgStab = new("bicgstab", static () => new BiCgStab());
    public static readonly IterativeMethod GpBiCg = new("gpbicg", static () => new GpBiCg());
    public static readonly IterativeMethod Tfqmr = new("tfqmr", static () => new TFQMR());

    private readonly Func<IIterativeSolver<double>> build;

    public IIterativeSolver<double> Solver() => build();
}

public sealed record IterationPolicy(double Tolerance, int MaxIterations, Func<IPreconditioner<double>> Preconditioner) {
    public static readonly IterationPolicy Default = new(1e-10, 1_000, static () => new DiagonalPreconditioner());

    public Iterator<double> Iterator() =>
        new(new IterationCountStopCriterion<double>(MaxIterations), new ResidualStopCriterion<double>(Tolerance));
}

public static class SparseOps {
    static readonly FrozenDictionary<SparseFormat, Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>>> Ingestors =
        new (SparseFormat Format, Func<int, int, int[], int[], double[], SparseCompressedRowMatrixStorage<double>> Build)[] {
            (SparseFormat.Csr, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(r, c, vals.Length, major, minor, vals)),
            (SparseFormat.Csc, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCompressedSparseColumnFormat(r, c, vals.Length, minor, major, vals)),
            (SparseFormat.Coo, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfCoordinateFormat(r, c, vals.Length, major, minor, vals)),
            (SparseFormat.Dok, static (r, c, major, minor, vals) => SparseCompressedRowMatrixStorage<double>.OfIndexedEnumerable(r, c, Indexed(major, minor, vals))),
        }.ToFrozenDictionary(static row => row.Format, static row => row.Build);

    static readonly FrozenDictionary<FactorizationKind, Func<CSparse.Storage.CompressedColumnStorage<double>, CSparse.ColumnOrdering, double[], double[], Fin<double[]>>> DirectSolvers =
        new (FactorizationKind Kind, Func<CSparse.Storage.CompressedColumnStorage<double>, CSparse.ColumnOrdering, double[], double[], Fin<double[]>> Solve)[] {
            (FactorizationKind.Cholesky, static (csc, ordering, rhs, result) => Run(() => CSparse.Double.Factorization.SparseCholesky.Create(csc, ordering).Solve(rhs, result), result)),
            (FactorizationKind.Lu, static (csc, ordering, rhs, result) => Run(() => CSparse.Double.Factorization.SparseLU.Create(csc, ordering, 1.0).Solve(rhs, result), result)),
            (FactorizationKind.Qr, static (csc, ordering, rhs, result) => Run(() => CSparse.Double.Factorization.SparseQR.Create(csc, ordering).Solve(rhs, result), result)),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Solve);

    public static Fin<SparseCompressedRowMatrixStorage<double>> Ingest(SparseFormat format, int rows, int columns, int[] majorIndices, int[] minorIndices, double[] values) =>
        minorIndices.Length != values.Length
            ? Fin.Fail<SparseCompressedRowMatrixStorage<double>>(new ComputeFault.PayloadOverBounds($"sparse-values:{values.Length}:{minorIndices.Length}"))
            : Ingestors.TryGetValue(format, out var build)
                ? Fin.Succ(build(rows, columns, majorIndices, minorIndices, values))
                : Fin.Fail<SparseCompressedRowMatrixStorage<double>>(ComputeFault.Create($"<sparse-format-miss:{format.Key}>"));

    public static Fin<double[]> SolveDirect(SparseCompressedRowMatrixStorage<double> csr, FactorizationKind kind, double[] rhs, CSparse.ColumnOrdering ordering) =>
        DirectSolvers.TryGetValue(kind, out var solve)
            ? solve(CSparse.Storage.CompressedColumnStorage<double>.OfIndexed(csr.RowCount, csr.ColumnCount, Entries(csr)), ordering, rhs, new double[rhs.Length])
            : Fin.Fail<double[]>(ComputeFault.Create($"<sparse-direct-miss:{kind.Key}>"));

    public static Fin<(double[] Field, int Iterations, double Residual, bool Converged)> SolveIterative(SparseCompressedRowMatrixStorage<double> csr, IterativeMethod method, double[] rhs, IterationPolicy policy) =>
        Try.lift(() => {
            var matrix = SparseMatrix.OfStorage(csr);
            var b = Vector<double>.Build.DenseOfArray(rhs);
            var x = Vector<double>.Build.Dense(rhs.Length);
            var iterator = policy.Iterator();
            method.Solver().Solve(matrix, b, x, iterator, policy.Preconditioner());
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
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` (local `Matrix<double>.Multiply` leaf) · `Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, CorrelationId Correlation, IClock Clock)` (distributed row-block fan-out dialing the `Solve` rpc per block); `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentAddress, ComputeReceipt.Factorization Receipt)` the per-block join carrier.
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` is the matmul lowering; the convolution overload `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan)` lowers Conv1D/Conv2D/Conv3D through `Im2Col` patch projection then one GEMM; `Fin<T>` aborts with `<lowering-row-miss>` only on a row outside the bound matrix set; the pooling entrypoint takes its window as the span argument.
- Auto: the tensor-lane `MatMul`/`Conv*`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider, each `Conv*` row lowers through the `Im2Col` patch projection that flattens every receptive field to a column then one `Matrix<double>.Multiply` GEMM against the reshaped kernel, and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; the `ShardPlan.Single` leaf runs the local `Matrix<double>.Multiply`, and the `ShardPlan.Blocked` fan-out partitions the GEMM into `Tile`-high row-blocks, dials the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub once per block (each block building a `SolveRequest` from its row-block and the active `FactorizationKind`), content-addresses every sub-block on `XxHash128.HashToUInt128(request.ToByteArray(), …)` against the Persistence `ModelResultIndex` so a re-run reuses computed blocks, joins the per-node `SolveResponse` solutions into the result via the associative `ShardBlock.Join` `SetSubMatrix` over a private join target, and aggregates each sub-block's `Factorization` receipt — `Traverse`-collected on the `Fin<Matrix<double>>` rail so a single failed shard aborts the join.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and the `Blocked` fan-out aggregates one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the per-node `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc`, or `Substrate.CpuTensor` on a content-address cache hit) — the shard count is the block count and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel directly and the `Conv1D`/`Conv2D`/`Conv3D` rows inherit it through the `Im2Col` patch projection, so the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; `Im2Col` enumerates each output spatial position over the `ConvWindow` stride/padding/dilation lattice, gathers the dilated receptive field across every channel into one patch row, and the patch matrix `[outPositions × Channels·KernelVolume]` multiplies the reshaped kernel `[Channels·KernelVolume × Filters]` in one `Matrix<double>.Multiply` whose tolerance is the `MatMul` proof the convolution row inherits through its `ToleranceClass.Tight` column; a Conv row routed without a `ConvWindow` (the matmul overload) returns the `<lowering-row-miss>` Fin.Fail because the geometry is absent, never silently correlating; the `Blocked` shard fan-out is a row-block partition over the dense fold dialing each row-block sub-solve through the EXISTING `Substrate.RemoteGrpc` `ComputeService.ComputeServiceClient` stub and the `Solve` rpc owned by `remote-lane#PROTO_VOCABULARY` by reference — the `Blocked` case carries the stub, provider, kind, reuse index, correlation, and clock as constructor columns so the arm is a real dial and a local-only tile loop is the named defect, a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter`, a second substrate, or a block-decomposition routing owner is the deleted form; each sub-block keys on `XxHash128.HashToUInt128(request.ToByteArray(), …)` against the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam by reference so a re-run reuses computed blocks (the cache-hit receipt carries `Substrate.CpuTensor`, the dialed receipt `Substrate.RemoteGrpc`), and the join writes each `ShardBlock` through `SetSubMatrix` into a private per-fan-out target — a shared mutable accumulator threaded through the per-shard `Map` is the named race defect; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

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

    public sealed record Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, CorrelationId Correlation, IClock Clock) : ShardPlan;

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
        var request = new SolveRequest {
            Matrix = GeometryPayload.OfDense(rowBlock),
            Rhs = ByteString.CopyFrom(MemoryMarshal.AsBytes<double>(right.ToColumnMajorArray())),
            FactorizationKind = plan.Kind.Key,
            SparseFormat = string.Empty,
            ShardTile = plan.Tile,
        };
        var address = XxHash128.HashToUInt128(request.ToByteArray(), unchecked((long)plan.Provider.SolveDedupKey(plan.Provider, (UInt128)start)));
        var dialedAt = plan.Clock.GetCurrentInstant();
        return plan.Reuse.Lookup(address)
            .Match(
                Some: response => Fin.Succ(Materialize(response, address, start, height, right.ColumnCount, Substrate.CpuTensor, Duration.Zero, plan)),
                None: () => Dial(plan.Compute, request, plan.Correlation)
                    .Map(response => {
                        plan.Reuse.Publish(address, response);
                        return Materialize(response, address, start, height, right.ColumnCount, Substrate.RemoteGrpc, plan.Clock.GetCurrentInstant() - dialedAt, plan);
                    }));
    }

    static ShardBlock Materialize(SolveResponse response, UInt128 address, int start, int height, int defaultCols, Substrate substrate, Duration elapsed, Blocked plan) {
        int cols = response.Cols == 0 ? defaultCols : (int)response.Cols;
        var receipt = new ComputeReceipt.Factorization(response.Provider, response.Decomposition, height, cols, response.Nnz, "dense") {
            Correlation = plan.Correlation, Lane = WorkLane.Background, Substrate = substrate, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };
        return new ShardBlock(start, height, Restore(response, height, cols), address, receipt);
    }

    static Fin<SolveResponse> Dial(ComputeService.ComputeServiceClient compute, SolveRequest request, CorrelationId correlation) =>
        Try.lift(() => compute.Solve(request, new CallOptions(new Metadata { { "rasm-correlation", correlation.Value } })))
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

- [ITERATIVE_SOLVE] SPIKE: the `SparseOps.SolveIterative` body binds the catalogued `IIterativeSolver<double>` factories `BiCgStab`/`GpBiCg`/`TFQMR` (the closed `IterativeMethod` SmartEnum's `Solver()` columns) and `Iterator<double>` control; the still-unverified live-host member spellings — the `IIterativeSolver<double>.Solve(matrix, b, x, iterator, preconditioner)` 5-arg overload, the `IPreconditioner<double>` interface seam that types the `IterationPolicy.Preconditioner` factory return and `DiagonalPreconditioner`'s implemented interface, the `ResidualStopCriterion<double>`/`IterationCountStopCriterion<double>` constructor arity, `Iterator<double>.Status` returning `IterationStatus.Converged`, `Iterator<double>.NumberOfIterations`, and `SparseMatrix.OfStorage(SparseCompressedRowMatrixStorage<double>)` — confirm against the admitted MathNet.Numerics 6.0.0-beta2 `MathNet.Numerics.LinearAlgebra.Solvers`/`.Double.Solvers` surface by tier-3 LIVE-HOST decompile (the `IPreconditioner<double>` interface name is not in the .api catalogue and must be probed, not assumed); the verified iteration count and final relative residual fold into the `SolveResult` the `solver-and-optimization#SOLVE_CONTRACT` iterative route reads, replacing the prior direct-solve mislabel.
- [CSPARSE_DIRECT] SPIKE: the `SparseOps.DirectSolvers` rows bind `CSparse.Double.Factorization.SparseCholesky.Create(csc, ordering)`/`SparseLU.Create(csc, ordering, 1.0)`/`SparseQR.Create(csc, ordering)` and `ISparseFactorization<double>.Solve(rhs, result)` — the `.api/api-mathnet-providers.md` `[ENTRYPOINTS]` table lists each `Create` symbol and `ISparseFactorization<T>.Solve` only by name, not overload arity, so the 2-arg Cholesky/QR `Create(csc, ordering)`, the 3-arg `SparseLU.Create(csc, ordering, double pivotTolerance)` pivot-tolerance argument, the `CompressedColumnStorage<double>.OfIndexed(rows, cols, entries)` ctor arity, and the in-place `Solve(double[] rhs, double[] result)` signature must be confirmed against the admitted `CSparse` `CSparse.Double.Factorization`/`CSparse.Storage` surface by tier-3 LIVE-HOST decompile before the `DirectSolvers` fold is FINALIZED; until then `SolveDirect` carries the same residual-probe posture as `SolveIterative`.
- [SHARD_FANOUT] SPIKE: the `ShardPlan.Blocked` fan-out dials the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub by reference, builds `SolveRequest` field-for-field (`matrix`/`rhs`/`factorization_kind`/`sparse_format`/`shard_tile`), content-addresses each row-block on `XxHash128.HashToUInt128(request.ToByteArray(), …)` against the Persistence `ModelResultIndex` for sub-block reuse, joins the per-node `SolveResponse` solutions into the result via the associative `ShardBlock.Join` `SetSubMatrix` into a private join target (never a shared mutable accumulator inside the per-shard `Map`), and aggregates the per-shard `Factorization` receipts — the `Fin<Matrix<double>>` rail throughout; the still-unverified live-host/cross-lane spellings are the generated `SolveRequest`/`SolveResponse` field accessors (`Matrix`/`Rhs`/`FactorizationKind`/`SparseFormat`/`ShardTile`; `Solution`/`Provider`/`Decomposition`/`Rows`/`Cols`/`Nnz`), the `GeometryPayload.OfDense(Matrix<double>)` dense-envelope projection owned at `remote-lane`/`interchange`, the synchronous `ComputeService.ComputeServiceClient.Solve(SolveRequest, CallOptions)` blocking-unary stub overload, and the Persistence `ModelResultIndex.Lookup(UInt128) : Option<SolveResponse>`/`Publish(UInt128, SolveResponse)` content-address reuse seam — confirm the generated stub members against the Grpc.Tools-compiled `ComputeService` client and the `ModelResultIndex` owner by tier-3 LIVE-HOST decompile, the live `Solve`-stub dial being the residual cross-lane probe that holds the `ShardPlan` owner at SPIKE.
