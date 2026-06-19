# [COMPUTE_BLAS]

Rasm.Compute dense linear-algebra lane: BLAS-class dense linear algebra over the admitted MathNet provider stack admitted once and routed by operand shape — definite, square, overdetermined, symmetric, periodic-grid — never by the call site and never by a knob riding beside the matrix. The lane owns the `LinearProvider` RID-keyed availability table selecting native OpenBLAS where an osx-arm64 asset resolves and the managed terminal otherwise, the `FactorRoute` `[Union]` route-spine collapsing every dense factorization to one shape-routed admission, the `Admission` finite/symmetry/singular gate re-imposing every gate MathNet refuses, the scale-derived `TolerancePolicy` carried on every receipt, the spectral `Modal`/eigen-residual/eigenbasis-filter owners returning the `SpectralResult` `[Union]`, and the claim-gated provider-rank selection with its provenance snapshot and the `OnlineStat` fourth-order residual-moment accumulator the receipt-surface residual histogram folds. Every library refuses its own gates — no constructor checks finiteness, `IsSymmetric()` compares by exact `!=`, a zero-norm `QR` fills `NaN` while `IsFullRank` returns `true` — so admission re-imposes each refused gate and every result leaves as a typed `ComputeReceipt.Factorization` carrying the route variant, the scale-derived tolerance, the provider determinism tag, and the recomputed true relative residual against the original operator, never a `Matrix<double>`, `Vector<double>`, or factorization instance.

## [1]-[INDEX]

- [1]-[DENSE_ALGEBRA]: RID provider table; `FactorRoute` shape-spine; admission gate; held witness.
- [2]-[PROVIDER_CLAIMS]: claim-gated provider rank; provenance snapshot; online fourth-moment solve stream.

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


## [3]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — plus the `OnlineStat` fourth-order residual-moment accumulator the numeric lane owns because `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `BenchmarkRow.Claim` resolved at composition against the running fingerprint and the `ModelResultKey.RecencyHorizon` — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot()` captures the provider `ToString` tag, the provider type name, and the public `MaxDegreeOfParallelism` degree at solve construction because every kernel reads this ambient static at execution instant (the `ParallelizeOrder`/`ParallelizeElements` thresholds are `internal` to `Control` and unreadable, so the determinism triple is the public provider/type/degree); `OnlineStat.Push(residual)` folds each witnessed solve residual into the running fourth-order moment stream under the `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `BenchmarkRow.Claim` owner and threaded in, never re-resolved here; bit-versus-envelope equality derives from the provider/type/degree triple because the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, counts sharded sub-blocks by node, and accumulates the online residual fourth-order moments through the `OnlineStat` `MomentNormalizer`-policy merge whose `Combine` is a CAS-safe pure reduction asserting `combined.Count == a.Count + b.Count`; the stream guards at admission through the same all-finite predicate the operands cross because one pushed `NaN` permanently poisons every moment with no reset.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, route variant, tolerance, true residual, determinism tag, symbolic fill, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations`, `rasm.compute.solve.residual`, and `rasm.compute.solve.shards` instruments are owned by `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here; the `OnlineStat` accumulator is the numeric-lane moment owner whose skewness/kurtosis evidence feeds the residual histogram tail.
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


## [4]-[RESEARCH]

- [NATIVE_EXECUTION]: the `LinearProvider.NativeOpenBlas` row execution — `Control.TryUseNativeOpenBLAS()` returning `true` and `LinearAlgebraControl.Provider` binding the native `ILinearAlgebraProvider` so dense GEMM and factorization run through OpenBLAS — and the CSparse native sparse path resolve on a host RID that carries the native asset (`win-x64`/`linux-x64` with the `MathNet.Numerics.MKL.Win-x64`/`.Linux-x64` or OpenBLAS native asset). The managed terminal is the proved cold start: `Control.TryUse*` returns `false` where no native asset resolves and degrades to `Managed`. The MKL row stays the win/linux-x64 design record (`Control.UseNativeMKL`/`Control.TryUseNativeMKL`), re-entering as a row only behind a MKL-carrying RID predicate.
