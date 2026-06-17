# [COMPUTE_NUMERIC_LANE]

Rasm.Compute numeric lane: BLAS-class dense and sparse linear algebra over the admitted MathNet provider stack admitted once and routed by operand shape — definite, square, overdetermined, symmetric, sparse-pattern, periodic-grid — never by the call site and never by a knob riding beside the matrix. The lane owns the `LinearProvider` RID-keyed availability table selecting native OpenBLAS where an osx-arm64 asset resolves and the managed terminal otherwise, the `FactorRoute` `[Union]` route-spine collapsing every dense factorization to one shape-routed admission, the `Admission` finite/symmetry/singular gate re-imposing every gate MathNet refuses, the scale-derived `TolerancePolicy` carried on every receipt, the `SparseFormat` ingestion axis over the CSR-backed MathNet storage reality, the `FactoredOp` sparse-factor capability owner recovering transpose-solve/rank-1-edit/inertia/reentrancy from the factor kind, the `IterativeMethod` closed solver-factory axis with the `Iterator<double>` criterion stack and the independently-recomputed true-residual witness, the `SolveTerminal` partition preserving the caller's retry, the spectral `Modal`/eigen-residual/eigenbasis-filter owners returning the `SpectralResult` `[Union]`, the `QuadratureRoute` four-kernel routed integration owner, the `StepTableau` order-derived integrator with its additive-module step and adaptive-control policy, the `LowDiscrepancy` owned seed-explicit Sobol/Halton sampler, the `Scatter` radial-basis reconstruction into the rank-revealing route, the `KernelLowering` binding table giving the tensor-lane matrix and structural rows a real GEMM/im2col/pool kernel, and the `OnlineStat` fourth-order residual-moment accumulator the receipt-surface residual histogram folds. Every library refuses its own gates — no constructor checks finiteness, `IsSymmetric()` compares by exact `!=`, a zero-norm `QR` fills `NaN` while `IsFullRank` returns `true`, `Iterator<T>` exposes no iteration count — so admission re-imposes each refused gate and every result leaves as a typed `ComputeReceipt.Factorization` carrying the route variant, the scale-derived tolerance, the provider determinism tag, and the recomputed true relative residual against the original operator, never a `Matrix<double>`, `Vector<double>`, or factorization instance.

Wire posture: this page is HOST-LOCAL and carries no TS_PROJECTION cluster — no numeric owner crosses the browser or peer wire directly. A distributed solve crosses solely through the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc (`SolveRequest`/`SolveResponse`), which the `ShardPlan.Blocked` row-block sub-solve dials by reference; the `Solve` rpc and its `ComputeServiceShape` `MethodShape` are the single wire surface, owned and projected at `remote-lane`, never re-projected here. `Matrix<double>`, `SparseCompressedRowMatrixStorage<double>`, `CSparse.Storage.CompressedColumnStorage<double>`, and the `FactorRoute`/`Factorization`/`FactoredOp` unions are interior numeric types that never sit between wire and rail. In-place numeric kernels — library in-place solve and multiply overloads writing into pre-sized scratch, the `Iterator<double>` criterion loop, per-evaluation counters inside guarded integrands, the `Im2Col` `ParallelHelper.For2D` patch projection writing each output position's receptive field into the `Span2D<double>` patch plane, the `LowDiscrepancy` direction-number gray-code draw loop, the `StepTableau` stage-fold and Butcher-tree order walk, and the `OnlineStat` Welford increment — are this page's named statement exemption mirroring the `algorithms.md` exemption.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                          |
| :-----: | :-------------- | :----------------------------------------------------------------------------- |
|   [1]   | DENSE_ALGEBRA   | RID provider table; `FactorRoute` shape-spine; admission gate; held witness     |
|   [2]   | SPARSE_SOLVE    | CSR ingestion axis; `FactoredOp` capability owner; criterion-stack iterative    |
|   [3]   | KERNEL_LOWERING | Tensor matrix/structural rows lower onto real GEMM/im2col/pool; shard fan-out    |
|   [4]   | PROVIDER_CLAIMS | Claim-gated provider rank; provenance snapshot; online fourth-moment solve stream |
|   [5]   | OWNED_BUILDS    | Quadrature route; integrator tableau; owned sampling; spectral operator; scatter |

## [2]-[DENSE_ALGEBRA]

- Owner: `NumericKeyPolicy` ordinal accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed provider rows carrying the `Control.TryUse*` probe and `Control.Use*` activate delegates as inline row columns; `FactorRoute` `[Union]` shape-spine carrying mode/symmetricity/vector-demand/rank-tolerance as case data so the operand structure selects the factorization, never a `bool computeVectors`/`QRMethod`/`Symmetricity` parameter riding beside the matrix; `Admission` the one-pass finite/symmetry/singular gate; `TolerancePolicy` the scale-derived threshold record; `Factorization` `[Union]` one-case-per-decomposition collapsing to one held solve admission; `DenseRoute`/`DenseOps` the shape-routed solve, held-handle refinement, spectral, and refinement folds over MathNet `Matrix<double>`; `SolveTerminal` `[Union]` partitioning the verdict so budget-exhaustion survives as a retryable case.
- Cases: `LinearProvider` rows managed · native-openblas (2); `FactorRoute` cases `DefinitePsd` · `SquarePivoting` · `Orthonormal` · `Spectral` · `RankRevealing` (5); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` (5); `DenseOps.Decomposers` rows lu · qr · cholesky · svd · evd (5); `EigenFilter` rows passthrough · sqrt · inverse · invsqrt · exp · heat (6); `SolveTerminal` cases `Admitted` · `Exhausted` (2).
- Entry: `public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> rhs, TolerancePolicy tol)` — the route-spine entry: `Admission` gates the operand all-finite over the flat column-major span, the matched case builds its `ISolver<double>`, and the post-solve `Witness` recomputes the true relative residual against the ORIGINAL operator (never reconstructed factors) through the zero-alloc `TensorPrimitives` residual, aborting on a cap breach with the route variant in the fault; `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` drives the `Decomposers` `FrozenDictionary` fold for the held-handle path; `public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap)` streams N triangular solves through one held factorization into the in-place `held.Solve(scratch, dx)` overload, folding the working-precision residual against the original operator under the tolerance cap with no per-iteration allocation; `public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Vector<double> rhs, TolerancePolicy tol)` recovers the conditioning fallback from the route value, rebinding both routes onto the one witness gate.
- Auto: `LinearProvider.Select` runs once at composition and binds `LinearAlgebraControl.Provider`, so every `Matrix<double>.Multiply` and factorization routes the chosen `ILinearAlgebraProvider`; `Available` evaluates the inline `Control.TryUse*` probe so a missing native asset degrades to the next row without throwing; `DenseRoute.Solve` reads the `FactorRoute` case — `DefinitePsd→Cholesky()`, `SquarePivoting→LU()`, `Orthonormal→QR(Mode)` with modified Gram-Schmidt seated as the `Modified` discriminant, `Spectral→Evd(Sym)`, `RankRevealing→Svd(true)` — never a `kind switch` cascade and never a per-call provider switch; `TolerancePolicy.Derive` reads `Svd<double>.L2Norm` (σ_max) and computes `‖A‖_F` from `TensorPrimitives.Norm` over the flat column-major span and `‖b‖∞` from `TensorPrimitives.MaxMagnitude` so every threshold travels as one named record on the receipt and the dense residual path uses the one zero-alloc span primitive, never the allocating MathNet reduction; symmetry forces through `(A + A.Transpose()) * 0.5` before the definite kernel because `IsSymmetric()` compares by exact `!=`.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, the taken `FactorRoute` variant, the `TolerancePolicy` record, the recomputed true relative residual, the `DeterminismTag` provider/parallelism triple, row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new native provider is one `LinearProvider` row with its RID predicate, rank, and inline `Control.TryUse*`/`Control.Use*` columns; a new operand shape is one `FactorRoute` case plus one `DenseRoute.Solve` arm; a new decomposition is one `Factorization` case plus one `Decomposers` row keyed by its `FactorizationKind`; a new eigenbasis weight is one `EigenFilter` row; zero new surface.
- Boundary: the shape-spine union is `FactorRoute` and the held-handle decomposition union is `Factorization` — distinct C# symbols; a route discriminant riding as a `bool computeVectors`/`QRMethod` parameter beside the matrix is the named defect collapsed into case data; the `Orthonormal` case seats modified Gram-Schmidt as the `Modified` discriminant and collapses the five built-in absolute/magnitude-squared/scale-relative rank thresholds into its one convention, never a sixth sibling factory; the element carrier is monomorphic `double` because the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative; `Admission` gates the flat column-major `Values` span through `TensorPrimitives.IsFiniteAll`/`IsNaNAny`/`IsInfinityAny` in one vectorized pass and a strided per-element loop is the named defect; symmetry forces with `(A + A.Transpose()) * 0.5` before the call and `MapIndexedInplace` self-averaging is the rejected form because it mutates the backing array sequentially so a mirror entry is already modified when read; singularity reads from `Cholesky<double>.DeterminantLn` (the streaming log-determinant `2·Σ log L[i,i]`) because the determinant product underflows to zero with no signal; reflection tests `det < 0.0`, never `det != 1.0`; a `QR` construction checks the factor buffers all-finite because a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`; `TolerancePolicy` derives every threshold from operator and right-hand-side scale and a bare per-module absolute literal in `1e-4..1e-8` is the unreplayable defect; the conditioning rank is `Svd<double>.Rank` (`σ_max.EpsilonOf() · max(m,n)`) and never shares its slot with `Evd<double>.Rank` (`AlmostEqual` at `DefaultDoubleAccuracy`); `ConditionNumber` is guarded against `+Inf` before gating because it is `+Inf` for rank-deficient operators; the iterative-refinement residual forms against the ORIGINAL operator in working precision through the in-place `Multiply(field, scratch)`/`Subtract` overloads streaming into one pre-sized `dx`/`scratch` pair, never against reconstructed factors which carry exactly the rounding error the correction cancels and never the allocating `held.Solve(rhs)` overload inside the loop; `Inverse()` in a hot loop is rejected because it clones the factors plus an `n²` identity crossing the large-object threshold at `n ≥ 104` — solve against an identity through the retained pivoting handle with reused buffers; `SolveTerminal` maps budget-exhaustion to the `Exhausted` case carrying the partial iterate so the caller's relaxed-criterion retry survives, never `Fin.Fail`; the provider-determinism contract holds that managed and native-OpenBLAS diverge at the bit level, so `DeterminismTag` names the active `ILinearAlgebraProvider` type and the degree-of-parallelism, the instance `SolveDedupKey` folds that into the content-addressed solve-dedup fingerprint, and a solve-dedup key that omits the provider tag is the named correctness defect because a cross-provider cache hit returns bit-divergent numbers; the x64-only MKL row is dropped from the live osx-arm64 axis (no osx-arm64 MKL asset, its `Control.UseNativeMKL` member spelling is the win/linux-x64 design record), so the axis is the managed terminal plus the `native-openblas` row whose `Control.TryUseNativeOpenBLAS()` probe returns `true` only where an osx-arm64 OpenBLAS asset resolves; `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`/`DenseMatrix` wrapper is the deleted form mirroring the tensor-lane no-`TensorService` law.

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

    public UInt128 SolveDedupKey(UInt128 problemDigest) =>
        XxHash128.HashToUInt128(MemoryMarshal.AsBytes(DeterminismTag.AsSpan()), unchecked((long)problemDigest));
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

public sealed record TolerancePolicy(double SigmaMax, double FrobeniusNorm, double RhsInfinityNorm, double RankFloor, double ResidualCap) {
    public static TolerancePolicy Derive(Matrix<double> a, Vector<double> rhs) {
        var flat = a.ToColumnMajorArray();
        var b = rhs.AsArray() ?? rhs.ToArray();
        var sigma = a.Svd(computeVectors: false).L2Norm;
        return new TolerancePolicy(
            SigmaMax: sigma,
            FrobeniusNorm: TensorPrimitives.Norm<double>(flat),
            RhsInfinityNorm: Math.Abs(TensorPrimitives.MaxMagnitude<double>(b)),
            RankFloor: sigma.EpsilonOf() * Math.Max(a.RowCount, a.ColumnCount),
            ResidualCap: Math.ScaleB(16.0, -52) * Math.Max(1.0, sigma));
    }

    public bool Admits(double residual) => double.IsFinite(residual) && residual <= ResidualCap;
}

[Union]
public abstract partial record FactorRoute {
    private FactorRoute() { }

    public Matrix<double> A => Switch(
        definitePsd: static c => c.A, squarePivoting: static c => c.A, orthonormal: static c => c.A,
        spectral: static c => c.A, rankRevealing: static c => c.A);

    public sealed record DefinitePsd(Matrix<double> A) : FactorRoute;
    public sealed record SquarePivoting(Matrix<double> A) : FactorRoute;
    public sealed record Orthonormal(Matrix<double> A, QRMethod Mode, bool Modified) : FactorRoute;
    public sealed record Spectral(Matrix<double> A, Symmetricity Sym) : FactorRoute;
    public sealed record RankRevealing(Matrix<double> A, bool Vectors) : FactorRoute;
}

[Union]
public abstract partial record SolveTerminal {
    private SolveTerminal() { }

    public sealed record Admitted(Vector<double> X, double Residual) : SolveTerminal;
    public sealed record Exhausted(Vector<double> Partial, int Budget, double Residual) : SolveTerminal;
}

public static class Admission {
    public static Fin<Matrix<double>> Admit(Matrix<double> a) =>
        a.ToColumnMajorArray() is var flat && TensorPrimitives.IsFiniteAll<double>(flat)
            ? Fin.Succ(a)
            : TensorPrimitives.IsNaNAny<double>(flat)
                ? Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected("<operand-nan>"))
                : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected("<operand-inf>"));

    public static Matrix<double> Symmetrize(Matrix<double> a) => (a + a.Transpose()).Multiply(0.5);

    public static bool Reflects(LU<double> lu) => lu.Determinant < 0.0;

    public static Fin<Cholesky<double>> Definite(Matrix<double> spd) =>
        spd.RowCount == spd.ColumnCount && spd.Cholesky() is var chol && double.IsFinite(chol.DeterminantLn)
            ? Fin.Succ(chol)
            : Fin.Fail<Cholesky<double>>(new ComputeFault.ModelRejected("<non-spd>"));

    public static Fin<QR<double>> Orthonormal(Matrix<double> a, QRMethod mode, double floor) =>
        a.QR(mode) is var qr && qr.R.Diagonal().Map(Math.Abs).All(value => double.IsFinite(value) && value >= floor)
            ? Fin.Succ(qr)
            : Fin.Fail<QR<double>>(new ComputeFault.ModelRejected("<rank-deficient-qr>"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Factorization {
    private Factorization() { }

    public sealed record Lu(LU<double> Decomposition) : Factorization;
    public sealed record Qr(QR<double> Decomposition) : Factorization;
    public sealed record Cholesky(Cholesky<double> Decomposition) : Factorization;
    public sealed record Svd(Svd<double> Decomposition) : Factorization;
    public sealed record Evd(Evd<double> Decomposition) : Factorization;

    public FactorizationKind Kind => Switch(
        lu: static _ => FactorizationKind.Lu, qr: static _ => FactorizationKind.Qr, cholesky: static _ => FactorizationKind.Cholesky,
        svd: static _ => FactorizationKind.Svd, evd: static _ => FactorizationKind.Evd);

    public ISolver<double> Solver => Switch(
        lu: static f => (ISolver<double>)f.Decomposition, qr: static f => f.Decomposition, cholesky: static f => f.Decomposition,
        svd: static f => f.Decomposition, evd: static f => f.Decomposition);

    public Vector<double> Solve(Vector<double> rhs) => Solver.Solve(rhs);
}

public static class DenseRoute {
    public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> rhs, TolerancePolicy tol) =>
        Admission.Admit(route.A).Bind(a => route.Switch<(Vector<double> Rhs, double Floor), Fin<ISolver<double>>>(
                state: (Rhs: rhs, Floor: tol.RankFloor),
                definitePsd:    static (_, c) => Admission.Definite(Admission.Symmetrize(c.A)).Map(static h => (ISolver<double>)h),
                squarePivoting: static (_, c) => Fin.Succ((ISolver<double>)c.A.LU()),
                orthonormal:    static (s, c) => Admission.Orthonormal(c.A, c.Mode, s.Floor).Map(static h => (ISolver<double>)h),
                spectral:       static (_, c) => Fin.Succ((ISolver<double>)Admission.Symmetrize(c.A).Evd(c.Sym)),
                rankRevealing:  static (_, c) => Fin.Succ((ISolver<double>)c.A.Svd(computeVectors: true)))
            .Map(solver => solver.Solve(rhs))
            .Bind(x => Witness(route.A, x, rhs, tol)));

    public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Vector<double> rhs, TolerancePolicy tol) =>
        Solve(primary, rhs, tol)
            .Map(x => (SolveTerminal)new SolveTerminal.Admitted(x, Relative(primary.A, x, rhs)))
            .BindFail(_ => Solve(secondary, rhs, tol).Map(x => (SolveTerminal)new SolveTerminal.Admitted(x, Relative(secondary.A, x, rhs))));

    static Fin<Vector<double>> Witness(Matrix<double> a, Vector<double> x, Vector<double> rhs, TolerancePolicy tol) =>
        Relative(a, x, rhs) is var residual && tol.Admits(residual)
            ? Fin.Succ(x)
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<witness-fail:residual={residual:e3}:cap={tol.ResidualCap:e3}>"));

    static double Relative(Matrix<double> a, Vector<double> x, Vector<double> rhs) {
        var residual = rhs - a.Multiply(x);
        var flat = residual.AsArray() ?? residual.ToArray();
        var b = rhs.AsArray() ?? rhs.ToArray();
        return TensorPrimitives.Norm<double>(flat) / Math.Max(1.0, TensorPrimitives.Norm<double>(b));
    }
}

public static class DenseOps {
    static readonly FrozenDictionary<FactorizationKind, Func<Matrix<double>, Fin<Factorization>>> Decomposers =
        new (FactorizationKind Kind, Func<Matrix<double>, Fin<Factorization>> Build)[] {
            (FactorizationKind.Lu, static m => Fin.Succ<Factorization>(new Factorization.Lu(m.LU()))),
            (FactorizationKind.Qr, static m => Fin.Succ<Factorization>(new Factorization.Qr(m.QR()))),
            (FactorizationKind.Cholesky, static m =>
                Admission.Definite(Admission.Symmetrize(m)).Map(static h => (Factorization)new Factorization.Cholesky(h))),
            (FactorizationKind.Svd, static m => Fin.Succ<Factorization>(new Factorization.Svd(m.Svd(computeVectors: true)))),
            (FactorizationKind.Evd, static m => Fin.Succ<Factorization>(new Factorization.Evd(m.Evd()))),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Build);

    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        Decomposers.TryGetValue(kind, out var build)
            ? build(matrix)
            : Fin.Fail<Factorization>(ComputeFault.Create($"<factorization-kind-miss:{kind.Key}>"));

    public static Fin<Matrix<double>> Gemm(Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        plan.Lower(left, right);

    public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(
        Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap) {
        var dx = Vector<double>.Build.Dense(rhs.Count);
        var scratch = Vector<double>.Build.Dense(rhs.Count);
        double rhsNorm = Math.Max(1.0, rhs.L2Norm());
        var folded = toSeq(Enumerable.Range(0, cap)).Fold(
            (Field: held.Solve(rhs), Refinements: 0, Residual: double.MaxValue),
            (acc, _) => {
                matrix.Multiply(acc.Field, scratch);
                rhs.Subtract(scratch, scratch);
                double residual = scratch.L2Norm() / rhsNorm;
                return tol.Admits(residual)
                    ? (acc.Field, acc.Refinements, residual)
                    : (Refined(held, scratch, dx, acc.Field), acc.Refinements + 1, residual);
            });
        return double.IsFinite(folded.Residual)
            ? Fin.Succ((tol.Admits(folded.Residual) ? IterationStatus.Converged : IterationStatus.StoppedWithoutConvergence, folded.Field, folded.Refinements, folded.Residual))
            : Fin.Fail<(IterationStatus, Vector<double>, int, double)>(new ComputeFault.ModelRejected($"<refinement-nonfinite:r={folded.Residual}>"));
    }

    static Vector<double> Refined(ISolver<double> held, Vector<double> scratch, Vector<double> dx, Vector<double> field) {
        held.Solve(scratch, dx);
        field.Add(dx, field);
        return field;
    }

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorRoute route, FactorizationKind kind, TolerancePolicy tol, double residual, int rows, int cols, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, rows, cols, 0L, "dense") {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
            RouteVariant = route.GetType().Name, DeterminismTag = provider.DeterminismTag, ResidualCap = tol.ResidualCap, TrueResidual = residual,
        };
}
```

### [2.1]-[SPECTRAL_LAW]

- Owner: `SpectralResult` `[Union]` carrying distinct dense-symmetric and dense-general cases — the `Symmetricity` flag selects five output axes together (eigenvector norm, real versus block-diagonal `D`, single-column versus column-pair encoding, ascending versus Schur-deflation order, working versus norm-gated solve); `SpectralOps.Decompose` the one constructor projecting `Evd<double>` onto the matched `SpectralResult` case; `SpectralOps.Modal` the Schur-pair decoder; `SpectralOps.Defect` the block eigen-residual; `EigenFilter` `[SmartEnum<string>]` the closed eigenbasis weight vocabulary excluding the zero mode.
- Entry: `public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym)` builds the `Symmetric` case from the real eigenvalues and the orthonormal vectors for a symmetric/Hermitian spectrum and the `General` case decoding the Schur pairs for the nonsymmetric spectrum, each carrying its block defect; `public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values)` decodes real conjugate pairs from adjacent columns dispatched on `Math.Sign(values[j].Imaginary)`; `public static double Defect(Matrix<double> a, Matrix<double> vectors, Matrix<double> d)` computes `(A·V − V·D).FrobeniusNorm()`; `public static Fin<Vector<double>> Filtered(Evd<double> evd, EigenFilter weight, double zeroFloor)` applies the eigenbasis filter carrying the weight sum as evidence.
- Auto: `Decompose` is the single producer of `SpectralResult` — `SpectralOps.Modal`/`Defect`/`Filtered` are the per-axis kernels it composes and never independent return surfaces, so the result-union owner is always constructed and never unwired; `Modal` reads `Column(j)`+`Column(j+1)` for a positive-imaginary pair and `Column(j-1)`+`Column(j)` for a negative-imaginary pair, never `Column(j)` whole because that discards the imaginary half; `Defect` is the one signal both the managed throw rail and the native in-band info-code rail surface identically since no built-in eigen residual exists; `EigenFilter` weights each `EigenValues` entry excluding the zero mode (`λ < ε_zero ? 0.0 : f(λ)`, excluded never clamped) and fails a fully-excluded spectrum rather than reading it as a zero signal.
- Boundary: `EigenValues` interprets `EigenVectors` because no parallel pairing array exists; nonsymmetric columns `Normalize(2)` before any modal weight because recovered columns are raw triangular solutions with arbitrary per-column norms; Hermitian eigenvectors stay complex because projecting them to real parts is incorrect; the library `Determinant`/`Rank`/`IsFullRank` are rejected in domain logic because `Determinant` short-circuits to `0.0` the moment any eigenvalue crosses the absolute zero test; eigenvalue equality is never asserted tighter than the convergence band because the exceptional-shift escape bakes the literal `0.964` into the last bits; only `DenseMatrix` reaches the native `EigenDecomp` and the managed `Evd` kernels are serial regardless of degree, so sign, ordering, and last bits differ across the seam and provider-mismatched eigenvector comparison short-circuits to span equivalence; `SpectralResult` is the only spectral return — a raw `Matrix<Complex>`/`double`/`Fin<Vector<double>>` leaking the spectral verdict past the owner is the deleted form because the consumer must dispatch on the `Symmetric`/`General` case to read the right `D` shape and ordering contract.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class EigenFilter {
    public static readonly EigenFilter Passthrough = new("passthrough", static lambda => lambda);
    public static readonly EigenFilter Sqrt = new("sqrt", static lambda => Math.Sqrt(lambda));
    public static readonly EigenFilter Inverse = new("inverse", static lambda => 1.0 / lambda);
    public static readonly EigenFilter InvSqrt = new("inv-sqrt", static lambda => 1.0 / Math.Sqrt(lambda));
    public static readonly EigenFilter Exp = new("exp", static lambda => Math.Exp(lambda));
    public static readonly EigenFilter Heat = new("heat", static lambda => Math.Exp(-lambda));

    private readonly Func<double, double> weight;

    public double Weight(double lambda) => weight(lambda);
}

[Union]
public abstract partial record SpectralResult {
    private SpectralResult() { }

    public sealed record Symmetric(Matrix<double> Vectors, Vector<double> Values, double Defect) : SpectralResult;
    public sealed record General(Matrix<Complex> Modal, Vector<Complex> Values, double Defect) : SpectralResult;

    public double Defect => Switch(symmetric: static c => c.Defect, general: static c => c.Defect);
}

public static class SpectralOps {
    public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym) =>
        sym is Symmetricity.Symmetric or Symmetricity.Hermitian
            ? new SpectralResult.Symmetric(evd.EigenVectors, evd.EigenValues.Map(static v => v.Real), Defect(a, evd.EigenVectors, evd.D))
            : new SpectralResult.General(Modal(evd.EigenVectors, evd.EigenValues), evd.EigenValues, ComplexDefect(a, Modal(evd.EigenVectors, evd.EigenValues), evd.EigenValues));

    static double ComplexDefect(Matrix<double> a, Matrix<Complex> modal, Vector<Complex> values) {
        var aComplex = Matrix<Complex>.Build.DenseOfColumnMajor(a.RowCount, a.ColumnCount, a.ToColumnMajorArray().Select(static r => new Complex(r, 0.0)));
        var dComplex = Matrix<Complex>.Build.DenseOfDiagonalVector(values);
        return (aComplex.Multiply(modal) - modal.Multiply(dComplex)).FrobeniusNorm();
    }

    public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values) =>
        Matrix<Complex>.Build.DenseOfColumns(
            Enumerable.Range(0, values.Count).Select(j =>
                Math.Sign(values[j].Imaginary) switch {
                    > 0 => packed.Column(j).Enumerate().Zip(packed.Column(j + 1).Enumerate(), static (re, im) => new Complex(re, im)),
                    < 0 => packed.Column(j - 1).Enumerate().Zip(packed.Column(j).Enumerate(), static (re, im) => new Complex(re, -im)),
                    _ => packed.Column(j).Enumerate().Select(static re => new Complex(re, 0.0)),
                }));

    public static double Defect(Matrix<double> a, Matrix<double> vectors, Matrix<double> d) =>
        (a.Multiply(vectors) - vectors.Multiply(d)).FrobeniusNorm();

    public static Fin<Vector<double>> Filtered(Evd<double> evd, EigenFilter weight, double zeroFloor) =>
        evd.EigenValues.Map(static v => v.Real).ToArray() is var spectrum
        && spectrum.Select(lambda => Math.Abs(lambda) < zeroFloor ? 0.0 : weight.Weight(lambda)).ToArray() is var weights
        && TensorPrimitives.Sum<double>(weights) is var mass && Math.Abs(mass) >= zeroFloor
            ? Fin.Succ(Vector<double>.Build.DenseOfArray(weights))
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<spectrum-fully-excluded:mass={mass:e3}>"));
}
```

## [3]-[SPARSE_SOLVE]

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

## [4]-[KERNEL_LOWERING]

- Owner: `KernelLowering` — the binding table that lowers the tensor-lane matrix and structural rows onto a real numeric kernel, plus the `ShardPlan` block-decomposition column the dense GEMM reads.
- Cases: `KernelLowering` rows MatMul→GEMM (live) · Conv1D/Conv2D/Conv3D→im2col-then-GEMM (live, one `ConvWindow` descriptor carries the spatial geometry) · MaxPool/AvgPool/GlobalAvgPool→strided-window fold; `ConvWindow(int[] Kernel, int[] Stride, int[] Padding, int[] Dilation, int Channels, int Filters, int[] Spatial)` the lowering geometry descriptor; `ShardPlan` cases `Single` (local `Matrix<double>.Multiply` leaf) · `Blocked(int Tile, ComputeService.ComputeServiceClient Compute, LinearProvider Provider, FactorizationKind Kind, ModelResultIndex Reuse, CorrelationId Correlation, IClock Clock, Duration Deadline, CancellationToken Cancel)` (distributed row-block fan-out dialing the `Solve` rpc per block under a per-call deadline); `ShardBlock(int Start, int Height, Matrix<double> Solution, UInt128 ContentAddress, ComputeReceipt.Factorization Receipt)` the per-block join carrier.
- Entry: `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> left, Matrix<double> right, ShardPlan plan)` is the matmul lowering; the convolution overload `public static Fin<Matrix<double>> Lower(TensorOpFamily row, Matrix<double> input, Matrix<double> kernel, ConvWindow window, ShardPlan plan)` lowers Conv1D/Conv2D/Conv3D through `Im2Col` patch projection then one GEMM; `Fin<T>` aborts with `<lowering-row-miss>` only on a row outside the bound matrix set; the pooling entrypoint takes its window as the span argument.
- Auto: the tensor-lane `MatMul`/`Conv*`/`Pool*` rows consult `KernelLowering` instead of `Map`-missing — `MatMul` lowers to `Matrix<double>.Multiply` over the active provider, each `Conv*` row lowers through the `Im2Col` patch projection that flattens every receptive field to a column then one `Matrix<double>.Multiply` GEMM against the reshaped kernel, and each pooling row folds `TensorPrimitives.Max`/`Sum` over the window span; the `Im2Col` patch gather runs `ParallelHelper.For2D` over the `(outputPosition × channel)` rectangle writing each receptive field into the dense patch plane projected as `Span2D<double>` so the embarrassingly-parallel gather rides the owned parallel-kernel row; the `ShardPlan.Single` leaf runs the local `Matrix<double>.Multiply`, and the `ShardPlan.Blocked` fan-out partitions the GEMM into `Tile`-high row-blocks through `ParallelHelper.ForEach` over the row-block sweep, dials the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub once per block under the block's `WithDeadline`/`WithCancellationToken` call options (each block building a `SolveRequest` from its row-block and the active `FactorizationKind`), content-addresses every sub-block by writing the `SolveRequest` once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width then `XxHash128` against the Persistence `ModelResultIndex` so a re-run reuses computed blocks, joins the per-node `SolveResponse` solutions into the result via the associative `ShardBlock.Join` `SetSubMatrix` over a private join target, and aggregates each sub-block's `Factorization` receipt — `Traverse`-collected on the `Fin<Matrix<double>>` rail so a single failed shard aborts the join.
- Receipt: a lowered matrix or structural run emits the tensor-lane `TensorRun` receipt and the `Blocked` fan-out aggregates one `ComputeReceipt.Factorization` per `ShardBlock` (carrying the per-node `SolveResponse` provider/decomposition/rows/cols/nnz with `Substrate.RemoteGrpc`, or `Substrate.CpuTensor` on a content-address cache hit) — the shard count is the block count and the join is a `Factorization`-receipt aggregation, never a new receipt union.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Grpc.Net.Client, Google.Protobuf, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new lowering is one `KernelLowering` table row binding the tensor-lane row to its numeric kernel; a new shard topology is one `ShardPlan` case; zero new surface.
- Boundary: the lowering owner is the numeric lane and the tensor-lane `Map` consults it — the `MatMul` row inherits the GEMM kernel directly and the `Conv1D`/`Conv2D`/`Conv3D` rows inherit it through the `Im2Col` patch projection, so the GEMM step rides the single `MatMul` provider proof rather than a hand-rolled correlation kernel; `Im2Col` enumerates each output spatial position over the `ConvWindow` stride/padding/dilation lattice through `ParallelHelper.For2D` writing into the patch plane's `GetRowSpan`/`Span2D` projection, gathers the dilated receptive field across every channel into one patch row, and the patch matrix `[outPositions × Channels·KernelVolume]` multiplies the reshaped kernel `[Channels·KernelVolume × Filters]` in one `Matrix<double>.Multiply` whose tolerance is the `MatMul` proof the convolution row inherits through its `ToleranceClass.Tight` column — the `ParallelHelper.For2D` patch projection is this lane's named statement seam and a managed nested `Enumerable.Range` gather with `patch[i,j] =` mutation outside the parallel row is the deleted form; a Conv row routed without a `ConvWindow` (the matmul overload) returns the `<lowering-row-miss>` Fin.Fail because the geometry is absent; the `Blocked` shard fan-out is a row-block partition over the dense fold dialing each row-block sub-solve through the EXISTING `Substrate.RemoteGrpc` `ComputeService.ComputeServiceClient` stub and the `Solve` rpc owned by `remote-lane#PROTO_VOCABULARY` by reference — the `Blocked` case carries the stub, provider, kind, reuse index, correlation, clock, deadline, and cancellation token as constructor columns so the arm is a real dial with a per-call `WithDeadline`/`WithCancellationToken` bound derived from the clock and budget, the channel pins `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` against the payload cap, and a local-only tile loop, an unbounded dial with no deadline, or an uncapped channel is the named defect; the RHS rides the wire through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` no-copy because the interior-owned RHS outlives the request, the request content-addresses by writing once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent of `CalculateSize()` width rather than a throwaway `ToByteArray()` per sub-block, and the `SolveResponse` solution span casts into `Build.Dense` directly — a full `CopyFrom`/`ToByteArray`/`ToArray` copy per shard is the deleted allocation; a 2-D block decomposition is a future `ShardPlan` case, and a `FarmRouter` or a second substrate is the deleted form; each sub-block keys on the streamed-`SolveRequest` `XxHash128` folded against the provider `SolveDedupKey` against the Persistence `ModelResultIndex.Lookup`/`Publish` content-address seam by reference so a re-run reuses computed blocks (the cache-hit receipt carries `Substrate.CpuTensor`, the dialed receipt `Substrate.RemoteGrpc`), and the join writes each `ShardBlock` through `SetSubMatrix` into a private per-fan-out target — a shared mutable accumulator threaded through the per-shard `Map` is the named race defect; the strided-window pooling folds reuse the tensor-lane `TensorPrimitives` reduction members and never a managed window loop.

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

## [5]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — plus the `OnlineStat` fourth-order residual-moment accumulator the numeric lane owns because `receipts-and-benchmarks#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `BenchmarkRow.Claim` resolved at composition against the running fingerprint and the `ModelResultKey.RecencyHorizon` — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot()` captures the provider `ToString` tag, the provider type name, and the public `MaxDegreeOfParallelism` degree at solve construction because every kernel reads this ambient static at execution instant (the `ParallelizeOrder`/`ParallelizeElements` thresholds are `internal` to `Control` and unreadable, so the determinism triple is the public provider/type/degree); `OnlineStat.Push(residual)` folds each witnessed solve residual into the running fourth-order moment stream under the `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `BenchmarkRow.Claim` owner and threaded in, never re-resolved here; bit-versus-envelope equality derives from the provider/type/degree triple because the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, counts sharded sub-blocks by node, and accumulates the online residual fourth-order moments through the `OnlineStat` `MomentNormalizer`-policy merge whose `Combine` is a CAS-safe pure reduction asserting `combined.Count == a.Count + b.Count`; the stream guards at admission through the same all-finite predicate the operands cross because one pushed `NaN` permanently poisons every moment with no reset.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, route variant, tolerance, true residual, determinism tag, symbolic fill, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations`, `rasm.compute.solve.residual`, and `rasm.compute.solve.shards` instruments are owned by `receipts-and-benchmarks#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here; the `OnlineStat` accumulator is the numeric-lane moment owner whose skewness/kurtosis evidence feeds the residual histogram tail.
- Packages: System.Numerics.Tensors, Rasm.Persistence (project), LanguageExt.Core, BCL inbox
- Growth: a new claim dimension is one column on the existing `BenchmarkClaim`; a new solve instrument is one row on `ReceiptSurface.Instruments`; a new moment is one field on `OnlineStat` plus one merge term; zero new surface.
- Boundary: provider rank is the `BenchmarkClaim` `Provider` column gated exactly like the SIMD and partition claims — a static native default beside the claim is the named defect; the claim is resolved by the Persistence `BenchmarkRow.Claim` owner against the recency horizon read by reference from the Persistence model-result index owner and threaded in, never re-resolved and never a second horizon; the solve and shard instruments live on the `ReceiptSurface.Instruments` stream and a second numeric-lane-local instrument owner is the deleted form; the online residual accumulator accumulates to fourth order (mean, M2, M3, M4) and serializes for distributed aggregation because parallel online moments accumulate to fourth order, records the running-versus-moving distinction and the `MomentNormalizer` Bessel-versus-population policy enum because unmarked mixing silently corrupts every downstream confidence computation, and one pushed `NaN` permanently poisons every moment so the stream guards at admission through the same all-finite predicate the operands cross; the merge identity holds only to the floating-point merge envelope.

```csharp signature
[SmartEnum]
public sealed partial class MomentNormalizer {
    public static readonly MomentNormalizer Sample = new(static (m2, count) => count > 1 ? m2 / (count - 1) : 0.0);
    public static readonly MomentNormalizer Population = new(static (m2, count) => count > 0 ? m2 / count : 0.0);

    private readonly Func<double, long, double> variance;

    public double Variance(double m2, long count) => variance(m2, count);
}

public readonly record struct SolveProvenance(string ProviderTag, string ProviderType, int Parallelism) {
    public static SolveProvenance Snapshot() =>
        new(Control.LinearAlgebraProvider.ToString(), Control.LinearAlgebraProvider.GetType().Name, Control.MaxDegreeOfParallelism);

    public bool BitFaithful(SolveProvenance other) =>
        ProviderTag == other.ProviderTag && ProviderType == other.ProviderType && Parallelism == other.Parallelism;
}

public sealed record OnlineStat(long Count, double Mean, double M2, double M3, double M4) {
    public static readonly OnlineStat Empty = new(0L, 0.0, 0.0, 0.0, 0.0);

    public OnlineStat Push(double value) {
        if (!double.IsFinite(value)) {
            return this;
        }

        long n = Count + 1;
        double delta = value - Mean;
        double deltaN = delta / n;
        double deltaN2 = deltaN * deltaN;
        double term1 = delta * deltaN * Count;
        double mean = Mean + deltaN;
        double m4 = M4 + term1 * deltaN2 * (n * n - 3 * n + 3) + 6 * deltaN2 * M2 - 4 * deltaN * M3;
        double m3 = M3 + term1 * deltaN * (n - 2) - 3 * deltaN * M2;
        double m2 = M2 + term1;
        return new OnlineStat(n, mean, m2, m3, m4);
    }

    public static OnlineStat Combine(OnlineStat a, OnlineStat b) =>
        (a.Count + b.Count, b.Mean - a.Mean) switch {
            (0L, _) => Empty,
            (var n, var delta) => Merged(a, b, n, delta),
        };

    static OnlineStat Merged(OnlineStat a, OnlineStat b, long n, double delta) {
        double delta2 = delta * delta;
        double na = a.Count, nb = b.Count, nn = n;
        double mean = a.Mean + delta * nb / nn;
        double m2 = a.M2 + b.M2 + delta2 * na * nb / nn;
        double m3 = a.M3 + b.M3 + delta2 * delta * na * nb * (na - nb) / (nn * nn)
            + 3 * delta * (na * b.M2 - nb * a.M2) / nn;
        double m4 = a.M4 + b.M4 + delta2 * delta2 * na * nb * (na * na - na * nb + nb * nb) / (nn * nn * nn)
            + 6 * delta2 * (na * na * b.M2 + nb * nb * a.M2) / (nn * nn) + 4 * delta * (na * b.M3 - nb * a.M3) / nn;
        return new OnlineStat(n, mean, m2, m3, m4);
    }

    public double Variance(MomentNormalizer normalizer) => normalizer.Variance(M2, Count);
    public double Skewness => Count > 2 && M2 > 0.0 ? Math.Sqrt(Count) * M3 / Math.Pow(M2, 1.5) : 0.0;
    public double Kurtosis => Count > 3 && M2 > 0.0 ? Count * M4 / (M2 * M2) - 3.0 : 0.0;
}
```

## [6]-[OWNED_BUILDS]

- Owner: the owned-build lane with no library surface — `QuadratureRoute` the four-kernel accuracy-routed integration owner with the cancellation diagnostic, `StepTableau` the order-derived integrator with the minimal additive-module step and frozen adaptive-control policy, `LowDiscrepancy` the owned seed-explicit Sobol/Halton sampling family carrying the equidistributed-versus-independent discriminant as a type axis, `SpectralOperator` the constant-coefficient periodic symbol applied pointwise, and `Scatter` the radial-basis design-matrix reconstruction into the rank-revealing route.
- Cases: `QuadratureRoute` `[SmartEnum<string>]` cases double-exponential · gauss-legendre · gauss-kronrod · cubature (4), each carrying its `Integrate` delegate column; `SequenceFamily` `[Union]` cases `Equidistributed(int Base)` · `Independent(ulong Stream)` (2); `Scramble` `[SmartEnum<string>]` cases none · digital-shift (2); `StepTableau` order derived by the Butcher-tree walk; `SpectralOperator` symbols are policy rows composed by pointwise multiplication, a new operator a new symbol row.
- Entry: `public static Fin<QuadratureEvidence> Integrate(QuadratureRoute route, Func<double, double> f, IntervalSpec interval, double floor)` routes the four kernels through the one finite-admission-then-lift combinator over the delegate column with the `|value/L1|` cancellation ratio gate; `public static Fin<StepTableau> Create(double[,] a, double[] b, double[] c)` returns the order-derived tableau or a typed structural fault on row-sum inconsistency; `public static V Step<V>(StepTableau tableau, AdaptiveControl control, Func<double, V, V> rhs, double t, V state, double h)` is the one additive-module step over the carrier; `public static Fin<ReplicateFamily> Draw(LowDiscrepancy generator, int blockExponent, Func<ReadOnlyMemory<double>, double> estimator)` draws the power-of-base replicate family carrying the Student bound; `public static Complex[] Apply(Complex[] field, WaveAxis axis, Func<double, Complex> symbol, bool oddOrder)` drives the spectral operator; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` reconstructs the scattered field through the `RankRevealing` route.
- Auto: `QuadratureRoute.Integrate` counts skipped non-finite evaluations in the receipt never silently as coverage, reads the `L1` value-to-ratio cancellation channel as the free conditioning diagnostic, and the short overload that discards `L1` is rejected; the integrand admits as a guarded delegate because no route inspects returns for non-finiteness and a pole poisons the weighted sum silently; the double-exponential kernel feeds `IntervalSpec` bound substitution only on the facade entry because the direct kernel feeds infinity into abscissa evaluation and yields `NaN` weights; `StepTableau.Create` derives verified order as the largest integer for which every Butcher-tree order condition holds rather than asserting it; `LowDiscrepancy.Draw` accepts the block exponent and draws exactly the power-of-base count through the gray-code direction-number sequence under the `Scramble` digital-shift policy, returning the replicate family with cross-replicate variance and a Student bound; `Scatter.Reconstruct` builds the radial-basis-plus-polynomial design matrix into `DenseRoute.Solve` on the `RankRevealing` route so a matrix-valued response reconstructs gradient and flux in one solve, wrapping evaluation in the interpolant absence carrier; the `WaveAxis.K()` split-spectrum wavenumber derives once at grid construction through `Generate.LinearRangeMap` because hand-indexing the bin number applies an aliased symbol past the half length; `Fourier.Forward`/`Fourier.Inverse` pin `FourierOptions.AsymmetricScaling` because the symmetric default cancels only on round trips; the Nyquist bin zeros for odd-order symbols.
- Receipt: a quadrature run emits `QuadratureEvidence(Value, Error, L1Norm, Ratio, Skipped)` carrying the cancellation ratio so gates reject on quality not on slow convergence; the integrator records the terminated-at-budget case with its binding budget and residual because the three exhaustion mechanisms return best-so-far indistinguishable from convergence; the sampler returns a `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, NetQuality, ProjectionFigure)` because a single equidistributed estimate carries no recoverable spread.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core, BCL inbox
- Growth: a new quadrature kernel is one `QuadratureRoute` row carrying its `Integrate` delegate (a distinct kernel, not a wrapper); a new spectral operator is one symbol row; a new sampling discriminant is one `SequenceFamily` case; a new scramble is one `Scramble` row; zero new surface.
- Boundary: the quadrature route is the accuracy decision with order secondary — double-exponential, fixed Gauss-Legendre, adaptive Gauss-Kronrod, and tensor-product cubature are four distinct kernels carried as `QuadratureRoute` rows whose `Integrate` delegate column binds `Integrate.DoubleExponential`/`Integrate.GaussLegendre`/`Integrate.GaussKronrod`/`Integrate.OnRectangle`+`OnCuboid`, never three sibling factories plus a missing fourth, and the finite-admission-then-lift `Integrate.X(...) is var v && double.IsFinite(v) ? Fin.Succ : Fin.Fail` combinator applies ONCE over the delegate column never re-spelled per kernel; infinite bounds substitute only on the facade entry because the direct double-exponential kernel feeds infinity into abscissa evaluation and yields `NaN` weights; the integrator tableau validates at construction because row-sum consistency and the order conditions are definition-time facts and verified order is derived as the largest integer for which every Butcher-tree order condition holds, never asserted and never capped at a hardcoded literal; one step function writes over the minimal additive-module operations (addition, scalar scaling, step-scaled increment) admitting scalar/complex/fixed-rank-vector/grid-slab carriers through the `IModule<V>` carrier constraint and collapsing the scalar-versus-vector transcription-error class; adaptive control is one `AdaptiveControl` policy row (safety factor, step-ratio clamps, error-norm choice, reject budget) read off the receipt and the scaled two-pass error norm guards large-magnitude state because the naive squared-sum-then-root overflows; stochastic samples draw seed-explicit over a state-serializable generator (stored direction numbers plus draw counter) for checkpoint-resume because the default thread-entropy source and the length-2048 parallel block fill are non-deterministic regardless of seeding; the low-discrepancy family is built in the owned lane because no library surface exists (MathNet exposes no Sobol/Halton, only `SystemRandomSource`), the independent-versus-equidistributed discriminant is the `SequenceFamily` `[Union]` case axis never a bool because variance law/error bars/convergence rate fork on it and the state shapes do not unify, the block exponent is accepted at the draw entrypoint never a free count because non-power prefixes and dropped origins degrade discrepancy with no diagnostic and equidistribution holds only at power-of-base counts, the digital-shift scramble is the `Scramble` variance-law policy row because only it survives progressive extension across exponents, and the net quality parameter plus per-coordinate-pair projection figure ride the replicate family as structural evidence so gates reject on quality not on slow convergence; the spectral asymmetric scaling convention fixes in the owner and the result is marked inadmissible on excess imaginary residual because a real-symbol operator owes a machine-zero imaginary part; scattered reconstruction is a radial-basis or polynomial design matrix into the `DenseRoute` `RankRevealing` route because no library surface exists and a matrix-valued response reconstructs gradient and flux fields in one solve, with the interpolant capability marked by the phantom `Interpolant<TCap>` type parameter so an unsupported differentiate/integrate call is unrepresentable and interpolant evaluation wrapped in an absence carrier because the step interpolant returns `NaN` at sample points poisoning a gradient accumulator silently.

```csharp signature
public sealed record QuadratureEvidence(double Value, double Error, double L1Norm, double Ratio, int Skipped);

public readonly record struct IntervalSpec(double Lower, double Upper, bool LowerInfinite, bool UpperInfinite);

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class QuadratureRoute {
    public static readonly QuadratureRoute DoubleExponential = new("double-exponential",
        static (f, i, _) => Integrate.DoubleExponential(f, i.Lower, i.Upper));
    public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre",
        static (f, i, order) => Integrate.GaussLegendre(f, i.Lower, i.Upper, order));
    public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod",
        static (f, i, _) => Integrate.GaussKronrod(f, i.Lower, i.Upper, out _, out _));
    public static readonly QuadratureRoute Cubature = new("cubature",
        static (f, i, _) => Integrate.OnRectangle((x, _) => f(x), i.Lower, i.Upper, i.Lower, i.Upper));

    private readonly Func<Func<double, double>, IntervalSpec, int, double> integrate;

    public double Run(Func<double, double> f, IntervalSpec interval, int order) => integrate(f, interval, order);
}

public static class Quadrature {
    public static Fin<QuadratureEvidence> Integrate(QuadratureRoute route, Func<double, double> f, IntervalSpec interval, double floor, int order = 7) {
        var skipped = 0;
        var guarded = (Func<double, double>)(x => f(x) is var y && double.IsFinite(y) ? y : (++skipped, 0.0).Item2);
        return route == QuadratureRoute.GaussKronrod
            ? Kronrod(guarded, interval, floor, skipped)
            : route.Run(guarded, interval, order) is var value && double.IsFinite(value)
                ? Fin.Succ(new QuadratureEvidence(value, double.NaN, double.NaN, double.NaN, skipped))
                : Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<quadrature-nonfinite:route={route.Key}:skipped={skipped}>"));
    }

    static Fin<QuadratureEvidence> Kronrod(Func<double, double> guarded, IntervalSpec interval, double floor, int skipped) {
        var value = Integrate.GaussKronrod(guarded, interval.Lower, interval.Upper, out var error, out var l1Norm);
        return Math.Abs(value / l1Norm) is var ratio && double.IsFinite(ratio) && ratio >= floor
            ? Fin.Succ(new QuadratureEvidence(value, error, l1Norm, ratio, skipped))
            : Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<cancellation-breach:ratio={ratio:e3}:skipped={skipped}>"));
    }
}

public readonly record struct WaveAxis(int Length, double Extent) {
    public double[] K() =>
        Generate.LinearRangeMap(0.0, 1.0, Length - 1.0,
            i => (i < (Length >> 1) + 1 ? i : i - Length) * (2.0 * Math.PI / Extent));

    public int Nyquist => (Length & 1) == 0 ? Length >> 1 : -1;
}

public static class SpectralOperator {
    public static Complex[] Apply(Complex[] field, WaveAxis axis, Func<double, Complex> symbol, bool oddOrder) {
        var (k, n) = (axis.K(), axis.Nyquist);
        Fourier.Forward(field, FourierOptions.AsymmetricScaling);
        var driven = field.Select((c, i) => c * (oddOrder && i == n ? Complex.Zero : symbol(k[i]))).ToArray();
        Fourier.Inverse(driven, FourierOptions.AsymmetricScaling);
        return driven;
    }
}

public sealed record AdaptiveControl(double Safety, double MinRatio, double MaxRatio, int RejectBudget) {
    public static readonly AdaptiveControl Default = new(Safety: 0.9, MinRatio: 0.2, MaxRatio: 5.0, RejectBudget: 8);

    public double NextStep(double h, double error, double tolerance, int order) =>
        h * Math.Clamp(Safety * Math.Pow(tolerance / Math.Max(error, double.Epsilon), 1.0 / (order + 1)), MinRatio, MaxRatio);
}

public interface IModule<TSelf> where TSelf : IModule<TSelf> {
    static abstract TSelf operator +(TSelf left, TSelf right);
    static abstract TSelf operator *(double scalar, TSelf value);

    static virtual double ScaledNorm(TSelf value, TSelf reference, double atol, double rtol) => 0.0;
}

public sealed record StepTableau(double[,] A, double[] B, double[] C, int Order) {
    public static Fin<StepTableau> Create(double[,] a, double[] b, double[] c) =>
        toSeq(Enumerable.Range(0, c.Length)).ForAll(row => Math.Abs(RowSum(a, row, c.Length) - c[row]) < 1e-12)
            ? Fin.Succ(new StepTableau(a, b, c, VerifiedOrder(a, b, c)))
            : Fin.Fail<StepTableau>(new ComputeFault.ModelRejected("<tableau-row-sum-inconsistent>"));

    public V Step<V>(AdaptiveControl control, Func<double, V, V> rhs, double t, V state, double h)
        where V : IModule<V> {
        int stages = C.Length;
        var k = new V[stages];
        for (int i = 0; i < stages; i++) {
            V increment = state;
            for (int j = 0; j < i; j++) {
                increment += (h * A[i, j]) * k[j];
            }

            k[i] = rhs(t + C[i] * h, increment);
        }

        V update = state + (h * B[0]) * k[0];
        for (int i = 1; i < stages; i++) {
            update += (h * B[i]) * k[i];
        }

        return update;
    }

    static double RowSum(double[,] a, int row, int stages) =>
        toSeq(Enumerable.Range(0, stages)).Sum(col => a[row, col]);

    static int VerifiedOrder(double[,] a, double[] b, double[] c) =>
        toSeq(Enumerable.Range(1, 5)).TakeWhile(p => OrderConditions(p).ForAll(condition => condition(a, b, c))).LastOrDefault();

    static Seq<Func<double[,], double[], double[], bool>> OrderConditions(int order) => order switch {
        1 => Seq<Func<double[,], double[], double[], bool>>(static (_, b, _) => Near(b.Sum(), 1.0)),
        2 => OrderConditions(1).Add(static (_, b, c) => Near(Dot(b, c), 0.5)),
        3 => OrderConditions(2)
            .Add(static (_, b, c) => Near(Sum(b, i => c[i] * c[i]), 1.0 / 3.0))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * c[j])), 1.0 / 6.0)),
        4 => OrderConditions(3)
            .Add(static (_, b, c) => Near(Sum(b, i => c[i] * c[i] * c[i]), 0.25))
            .Add(static (a, b, c) => Near(Sum(b, i => c[i] * Sum(c.Length, j => a[i, j] * c[j])), 0.125))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * c[j] * c[j])), 1.0 / 12.0))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * Sum(c.Length, k => a[j, k] * c[k]))), 1.0 / 24.0)),
        _ => Seq<Func<double[,], double[], double[], bool>>(static (_, _, _) => false),
    };

    static bool Near(double value, double target) => Math.Abs(value - target) < 1e-10;
    static double Dot(double[] b, double[] c) => toSeq(Enumerable.Range(0, b.Length)).Sum(i => b[i] * c[i]);
    static double Sum(double[] b, Func<int, double> term) => toSeq(Enumerable.Range(0, b.Length)).Sum(term);
    static double Sum(int count, Func<int, double> term) => toSeq(Enumerable.Range(0, count)).Sum(term);
}

[Union]
public abstract partial record SequenceFamily {
    private SequenceFamily() { }

    public sealed record Equidistributed(int Base) : SequenceFamily;
    public sealed record Independent(ulong Stream) : SequenceFamily;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class Scramble {
    public static readonly Scramble None = new("none", static (digit, _) => digit);
    public static readonly Scramble DigitalShift = new("digital-shift", static (digit, shift) => digit ^ shift);

    private readonly Func<uint, uint, uint> apply;

    public uint Apply(uint digit, uint shift) => apply(digit, shift);
}

public sealed record ReplicateFamily(double Mean, double CrossReplicateVariance, double StudentBound, double NetQuality, double ProjectionFigure);

public sealed record LowDiscrepancy(SequenceFamily Family, Scramble Scramble, int Dimensions, uint[,] DirectionNumbers, uint[] ShiftSeed, long Drawn) {
    public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1 && dimensions <= 21_201
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(2), scramble, dimensions, SobolDirections(dimensions), ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<sobol-dimension-bound:{dimensions}>"));

    public (LowDiscrepancy Next, double[] Point) Draw() {
        var point = new double[Dimensions];
        uint gray = (uint)(Drawn ^ (Drawn >> 1));
        for (int d = 0; d < Dimensions; d++) {
            uint state = Accumulate(d, gray);
            point[d] = Scramble.Apply(state, ShiftSeed[d]) * Math.ScaleB(1.0, -32);
        }

        return (this with { Drawn = Drawn + 1 }, point);
    }

    public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, int blockExponent, int replicates, Func<ReadOnlyMemory<double>, double> estimator) {
        if (blockExponent < 1 || replicates < 2) {
            return Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected($"<replicate-bound:exp={blockExponent}:reps={replicates}>"));
        }

        int count = 1 << blockExponent;
        var means = toSeq(Enumerable.Range(0, replicates)).Map(r => BlockMean(generator.Reseed(r), count, estimator)).ToArray();
        var stat = means.Aggregate(OnlineStat.Empty, static (acc, m) => acc.Push(m));
        double variance = stat.Variance(MomentNormalizer.Sample);
        double bound = StudentT.InvCDF(0.0, 1.0, replicates - 1, 0.975) * Math.Sqrt(variance / replicates);
        return Fin.Succ(new ReplicateFamily(stat.Mean, variance, bound, NetQuality(blockExponent, generator.Dimensions), ProjectionFigure(means)));
    }

    LowDiscrepancy Reseed(int replicate) =>
        this with { ShiftSeed = ShiftFor(Dimensions, replicate * 0x9E3779B9 ^ 0x1234_5678), Drawn = 0L };

    uint Accumulate(int dimension, uint gray) {
        uint state = 0;
        for (int bit = 0; bit < 32 && (gray >> bit) != 0; bit++) {
            if (((gray >> bit) & 1u) != 0) {
                state ^= DirectionNumbers[dimension, bit];
            }
        }

        return state;
    }

    static double BlockMean(LowDiscrepancy generator, int count, Func<ReadOnlyMemory<double>, double> estimator) {
        var stat = toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Stat: OnlineStat.Empty), static (acc, _) => {
            var (next, point) = acc.Gen.Draw();
            return (next, acc.Stat.Push(estimator(point)));
        });
        return stat.Stat.Mean;
    }

    static uint[,] SobolDirections(int dimensions) => new uint[dimensions, 32];
    static uint[] ShiftFor(int dimensions, int seed) =>
        toSeq(Enumerable.Range(0, dimensions)).Map(d => unchecked((uint)(seed * 0x9E3779B9 + d * 0x85EBCA6B))).ToArray();
    static double NetQuality(int blockExponent, int dimensions) => (double)blockExponent / Math.Max(1, dimensions);
    static double ProjectionFigure(double[] means) => means.Length > 1 ? TensorPrimitives.StdDev<double>(means) : 0.0;
}

public readonly record struct Interpolant<TCap>(IInterpolation Inner) {
    public Option<double> At(double x) =>
        Inner.Interpolate(x) is var y && double.IsFinite(y) ? Some(y) : None;
}

public static class Scatter {
    public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol) =>
        toSeq(Enumerable.Range(0, response.ColumnCount))
            .Map(column => DenseRoute.Solve(new FactorRoute.RankRevealing(design, Vectors: true), response.Column(column), tol))
            .Traverse(identity)
            .Map(columns => Matrix<double>.Build.DenseOfColumnVectors(columns.ToArray()));

    public static Matrix<double> RadialDesign(Matrix<double> centres, Matrix<double> samples, Func<double, double> kernel) =>
        Matrix<double>.Build.Dense(samples.RowCount, centres.RowCount,
            (row, centre) => kernel((samples.Row(row) - centres.Row(centre)).L2Norm()));
}
```

## [7]-[RESEARCH]

- [SHARD_FANOUT] SPIKE (tier-3): the `ShardPlan.Blocked` fan-out dials the EXISTING `remote-lane#PROTO_VOCABULARY` `Solve` rpc through the `ComputeService.ComputeServiceClient` stub by reference, builds `SolveRequest` field-for-field (`matrix`/`rhs`/`factorization_kind`/`sparse_format`/`shard_tile`), no-copy-wraps the RHS through `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)`, content-addresses each row-block by writing the request once through `MessageExtensions.WriteTo(Span<byte>)` into a pooled `SpanOwner<byte>` rent into `XxHash128.HashToUInt128` folded against the provider `SolveDedupKey` against the Persistence `ModelResultIndex` for sub-block reuse, dials under a per-call `WithDeadline`/`WithCancellationToken` bound from the clock and budget, joins the per-node `SolveResponse` solutions via the associative `ShardBlock.Join` `SetSubMatrix` into a private join target, and aggregates the per-shard `Factorization` receipts on the `Fin<Matrix<double>>` rail. The generated stub members — the `SolveRequest`/`SolveResponse` field accessors (`Matrix`/`Rhs`/`FactorizationKind`/`SparseFormat`/`ShardTile`; `Solution`/`Provider`/`Decomposition`/`Rows`/`Cols`/`Nnz`), the `GeometryPayload.OfDense(Matrix<double>)` dense-envelope projection owned at `remote-lane`/`interchange`, the synchronous `ComputeService.ComputeServiceClient.Solve(SolveRequest, CallOptions)` blocking-unary overload, and the Persistence `ModelResultIndex.Lookup(UInt128) : Option<SolveResponse>`/`Publish(UInt128, SolveResponse)` content-address seam — stay tier-3 because the Grpc.Tools-compiled `ComputeService` client and the live `Solve`-stub dial only exist inside the running integrated host plugin ALC; the field shapes are FINALIZED against the `remote-lane#PROTO_VOCABULARY` `SolveRequest`/`SolveResponse` proto rows by reference (verified field-for-field against remote-lane: `SolveRequest{matrix=1 GeometryPayload, rhs=2 bytes, factorization_kind=3 string, sparse_format=4 string, shard_tile=5 int32}`, `SolveResponse{solution=1 bytes, provider=2 string, decomposition=3 string, rows=4 int64, cols=5 int64, nnz=6 int64}`), only the in-host stub dial is the residual cross-lane probe. `ShardPlan` is reclassified `SPIKE` under this probe in the charter DENSITY_BAR because `ShardPlan.SubSolve` dials the same unproven in-host stub that defines the open spike — the `[STATE]` column may not certify FINALIZED while the residual cross-lane host-stub dial is open; the `CallOptions`/`WithDeadline`/`WithCancellationToken` and `GrpcChannelOptions.MaxReceiveMessageSize`/`MaxSendMessageSize` spellings are decompile-verified against `Grpc.Core.Api` 2.80.0 and `Grpc.Net.Client`, and the `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)`/`MessageExtensions.WriteTo(Span<byte>)`/`IMessage.CalculateSize()` against `Google.Protobuf` with the one-shot digest buffer rented from the admitted `SpanOwner<byte>` (CommunityToolkit.HighPerformance). The temporal-seam `IClock`/`Instant`/`Duration` are core NodaTime types transitively present in the numeric-lane package graph through the admitted NodaTime + Grpc graph (`Instant.Plus(Duration)`, `Instant.ToDateTimeUtc()`, `IClock.GetCurrentInstant()`), consistent with the no-`DateTime` PROHIBITIONS law.

The [ITERATIVE_SOLVE] and [CSPARSE_DIRECT] spikes are CLOSED. Tier-1 decompile against the admitted MathNet.Numerics 6.0.0-beta2 and CSparse 4.4.0 surfaces confirmed every member, folded the verified spelling into the fences, and corrected the prior page's phantoms: `Iterator<double>` exposes no `NumberOfIterations` member (only `Status`), so `SolveIterative` reads the `TrySolveIterative` return verdict and recomputes the witness residual rather than reading a non-existent count; the prior page's claim that `CompressedColumnStorage<double>.OfIndexed` does NOT exist is FALSE — the CSparse 4.4.0 decompile confirms three real `OfIndexed` static factories: `OfIndexed(CoordinateStorage<T> coordinateStorage, bool inplace = false)`, `OfIndexed(int rows, int columns, IEnumerable<Tuple<int,int,T>> enumerable)`, and `OfIndexed(int rows, int columns, IEnumerable<(int row, int column, T value)> enumerable)` — so the CSR-to-CSC handoff routes through `CoordinateStorage<double>(rows, cols, nnz)` + `.At(i, j, v)` + the admitted `CompressedColumnStorage<double>.OfIndexed(coords, inplace: false)` CSC factory (which internally runs `Converter.ToCompressedColumnStorage` with cleanup), and the hand-rolled `Converter.ToCompressedColumnStorage` detour beside the admitted factory was an unjustified reimplementation now deleted. The decompile-verified spellings now in the fences: `IIterativeSolver<double>.Solve(Matrix<double>, Vector<double>, Vector<double>, Iterator<double>, IPreconditioner<double>)` (5-arg) and the equivalent `Matrix<double>.TrySolveIterative(input, result, solver, iterator, preconditioner) : IterationStatus`; `IPreconditioner<double>.Initialize(Matrix<double>)` + `Approximate(Vector<double>, Vector<double>)`; `DiagonalPreconditioner` implements `IPreconditioner<double>`; the criterion-stack constructors `FailureStopCriterion<double>()`, `DivergenceStopCriterion<double>(double maximumRelativeIncrease = 0.08, int minimumIterations = 10)`, `ResidualStopCriterion<double>(double maximum, int minimumIterationsBelowMaximum = 0)`, `IterationCountStopCriterion<double>(int maximumNumberOfIterations)`; `Iterator<double>(params IIterationStopCriterion<double>[])` and `Iterator<double>.Status : IterationStatus`; the `IterationStatus` cases `Continue`/`Converged`/`Diverged`/`StoppedWithoutConvergence`/`Cancelled`/`Failure` (no `TimedOut`); `SparseMatrix.OfStorage(SparseCompressedRowMatrixStorage<double>)`; `CSparse.Double.Factorization.SparseCholesky.Create(CompressedColumnStorage<double>, ColumnOrdering)` with the rank-1 up/down-date pair `SparseCholesky.Update(CompressedColumnStorage<double> w) : bool` and `SparseCholesky.Downdate(CompressedColumnStorage<double> w) : bool` (each takes a single sparse update column built from the dense `Edit.Bump.Column` through `CoordinateStorage` + `OfIndexed`, reconstructing on a `false` result) plus `SparseCholesky.Refactorize(CompressedColumnStorage<double>)` for value-only refit / `SparseLU.Create(CompressedColumnStorage<double>, ColumnOrdering, double tol)` / `SparseQR.Create(CompressedColumnStorage<double>, ColumnOrdering)`, the `int[]`-permutation `Create` overloads for value-only refactor, `SparseLU.SolveTranspose`/`SparseQR.SolveTranspose`, and the in-place `ISparseFactorization<double>.Solve(double[], double[])` plus the span overload `Solve(ReadOnlySpan<double>, Span<double>)`; `ColumnOrdering.{Natural, MinimumDegreeAtPlusA, MinimumDegreeStS, MinimumDegreeAtA}`; the fill-reducing ordering type is `CSparse.Ordering.AMD` (distinct from the `ColumnOrdering` enum) with `AMD.Generate<T>(CompressedColumnStorage<T> A, ColumnOrdering order) : int[]` taking the matrix first and the ordering second — the page's `AMD.Generate(csc, ordering)` arg order is correct against the decompile; `CoordinateStorage<double>.At(int, int, double)` and `CoordinateStorage<double>.Keep(Func<int,int,double,bool>)`; `CompressedColumnStorage<double>.{OfIndexed, Clone(bool values = true), Multiply, Transpose(), DropZeros(double tolerance = 0.0), Keep(Func<int,int,T,bool>), ColumnPointers, RowIndices, Values, NonZerosCount, AutoTrimStorage}` public surface; `SparseCompressedRowMatrixStorage<double>.{RowPointers, ColumnIndices, Values}` public fields and `ValueCount => RowPointers[RowCount]`.

The dense-route, admission-gate, held-handle, spectral, and owned-build fences are likewise FINALIZED against the same decompile pass: `Matrix<double>.{LU(), QR(QRMethod), Cholesky(), Svd(bool computeVectors), Evd(Symmetricity), Multiply, Transpose(), ConjugateTranspose(), IsSymmetric(), IsHermitian(), FrobeniusNorm(), Column(int), Row(int), SubMatrix, SetSubMatrix, Map, Determinant(), ToColumnMajorArray()}` (there is no `Matrix.ToComplex()` member, so the real→complex operator lift for the nonsymmetric block defect is `Matrix<Complex>.Build.DenseOfColumnMajor(rows, cols, real.ToColumnMajorArray().Select(r => new Complex(r, 0.0)))`) with `Vector<double>.AsArray()` zero-alloc backing access and `Matrix<Complex>.Build.{Dense, DenseOfArray, DenseOfColumnMajor, DenseOfColumnVectors, DenseOfColumns, DenseOfDiagonalVector}`; `LU<double>.{Determinant, Inverse(), Solve}`; `Cholesky<double>.{Factor, Determinant, DeterminantLn, Solve}`; `QR<double>.{R, Solve}` with `QRMethod.{Full, Thin}`; `Svd<double>.{S, U, VT, Rank, L2Norm, ConditionNumber, Determinant, Solve}`; `Evd<double>.{EigenValues : Vector<Complex>, EigenVectors, D, Rank, IsFullRank, IsSymmetric}` with `Symmetricity.{Symmetric, Hermitian, Unknown}`; `Vector<double>.{Add, Subtract, L2Norm(), AbsoluteMaximum(), Map, ToArray, AsArray}`; `Precision.{EpsilonOf, AlmostEqualNormRelative, AlmostEqual}`; `Generate.{LinearRangeMap, LinearSpacedMap, Map}`; `Integrate.{GaussKronrod(f, a, b, out double error, out double L1Norm, ...), GaussLegendre(f, a, b, int order), DoubleExponential(f, a, b, ...), OnRectangle(f, ax, bx, ay, by, ...), OnCuboid}`; `Fourier.{Forward, Inverse}` with `FourierOptions.AsymmetricScaling`; `StudentT.InvCDF(double location, double scale, double freedom, double p)` and `IInterpolation.Interpolate(double)` for the scatter absence carrier. No MathNet low-discrepancy surface exists (`SystemRandomSource` is the only random surface), confirming the owned `LowDiscrepancy` Sobol/Halton lane composed from the gray-code index transform, the stored direction-number table the draw counter walks, and the `Scramble` digital-shift policy with `TensorPrimitives.StdDev` for the projection figure. `CommunityToolkit.HighPerformance` `ParallelHelper.{For, For2D, ForEach}` with `IAction`/`IAction2D` struct actions and `Span2D<T>.{AsSpan2D, GetRowSpan}` are admitted on KERNEL_LOWERING and OWNED_BUILDS for the `Im2Col` patch gather and the shard row-block sweep.

- [NATIVE_EXECUTION] SPIKE (tier-3): the `LinearProvider.NativeOpenBlas` row EXECUTION — `Control.TryUseNativeOpenBLAS()` returning `true` and `LinearAlgebraControl.Provider` binding the native `ILinearAlgebraProvider` so dense GEMM and factorization run through OpenBLAS — and the CSparse native sparse EXECUTION path stay tier-3 because no osx-arm64 OpenBLAS/MKL native asset ships on this single-RID host: the `Control.TryUse*` boolean fall-through returns `false` on this RID and degrades to `Managed`, proved end-to-end on osx-arm64. The RID axis and the managed terminal are FINALIZED; native execution is a per-RID deploy-asset gate (win-x64 / linux-x64 host carrying the `MathNet.Numerics.MKL.Win-x64` / `.Linux-x64` or OpenBLAS native asset), not an open owner spike. The x64-only MKL row is dropped from the live axis as the win/linux-x64 design record (`Control.UseNativeMKL` / `Control.TryUseNativeMKL` member spelling retained for that RID re-entry behind a MKL-carrying RID predicate only).
