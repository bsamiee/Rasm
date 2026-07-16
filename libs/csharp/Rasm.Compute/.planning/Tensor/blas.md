# [COMPUTE_BLAS]

Rasm.Compute dense linear-algebra lane: BLAS-class dense linear algebra over the admitted MathNet provider stack and the native `TorchSharp` ATen substrate, admitted once and routed by operand shape — definite, square, overdetermined, symmetric, periodic-grid — never by the call site and never by a knob riding beside the matrix. Every library refuses its own gates — no constructor checks finiteness, `IsSymmetric()` compares by exact `!=`, a zero-norm `QR` fills `NaN` while `IsFullRank` returns `true` — so `Admission` re-imposes each refused gate and every result leaves as a typed `ComputeReceipt.Factorization` carrying the route variant, the scale-derived tolerance, the provider determinism tag, and the recomputed true relative residual against the original operator, never a `Matrix<double>`/`Vector<double>` or factorization instance.

`LinearProvider` ranks native MKL then OpenBLAS then the managed terminal by RID claim; `DenseSubstrate` routes the osx-arm64 dense solve to `torch.linalg` over `libtorch_cpu`'s Apple-Accelerate BLAS/LAPACK and keeps the managed `Matrix<double>` route as the proved cold-start terminal. Every dense and sparse solve emits the `Factorization` `ComputeReceipt` and rides the `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` solve stream, into which the `OnlineStat` fourth-order residual-moment accumulator folds; the provider-rank claim resolves at composition against the Persistence `ModelResultIndex.Claim` owner.

## [01]-[INDEX]

- [01]-[DENSE_ALGEBRA]: RID provider table; `FactorRoute` shape-spine; admission gate; held witness.
- [02.1]-[LEVENBERG_MARQUARDT]: damped Gauss-Newton nonlinear least-squares; HyperJet the canonical exact-Jacobian provider.
- [02.2]-[SPECTRAL_LAW]: dense-symmetric/general spectral split; Schur-pair decode; eigen-filter weights.
- [02]-[PROVIDER_CLAIMS]: claim-gated provider rank; provenance snapshot; online fourth-moment solve stream.

## [02]-[DENSE_ALGEBRA]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed MathNet-provider rows carrying the `Control.TryUse*` probe and `Control.Use*` activate delegates as inline row columns; `DenseSubstrate` `[SmartEnum<string>]` the execution-substrate axis choosing the managed `Matrix<double>` route or the native `torch.linalg` ATen leg, each carrying its `Available` probe and a substrate-determinism tag; `FactorRoute` `[Union]` shape-spine whose cases carry ONLY per-occurrence factorization policy (mode, orthogonalization law, symmetricity) while the operand `Matrix<double>` rides the entrypoint argument; `Admission` the one-pass finite/symmetry/singular gate plus the modified Gram-Schmidt realization; `TolerancePolicy` the scale-derived threshold record seeded O(n²) from `‖A‖_F` and refined through `WithSigma` where an `Svd` handle is already held; `SketchPolicy` the seeded randomized range-finder policy; `Factorization` `[Union]` the held decomposition family, including the range basis required to solve through a randomized sketch; `AtenFloor`/`AtenDense` the native-substrate runtime probe and route-discriminated `torch.linalg` solve leg — `cholesky_ex`/`ldl_factor_ex`/`solve_ex` info-gated one-shots, the full-tuple `lstsq` whose reported rank always gates rank-deficiency and whose singular-value floor binds only where the driver yields the spectrum (the driverless surface runs CPU `gelsy`, whose singular-values tensor is empty), the disposable `HeldFactor` owner over the `lu_factor`/`lu_solve` pair, and the all-dense `torch.einsum` contraction; `DenseRoute`/`DenseOps` the shape-routed solve, held-handle refinement, and spectral folds over MathNet `Matrix<double>`; `SolveTerminal` `[Union]` partitioning the verdict so budget-exhaustion survives as a retryable case.
- Cases: `LinearProvider` rows managed · native-openblas · native-mkl (3); `DenseSubstrate` rows managed · native-aten (2); `FactorRoute` cases `DefinitePsd` · `SquarePivoting` · `Orthonormal` · `Spectral` · `RankRevealing` (5); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` · `Sketched` (6); `DenseOps.Decompose` `FactorizationKind.Switch` arms lu · qr · cholesky · svd · evd (5); `EigenFilter` rows passthrough · sqrt · inverse · invsqrt · exp · heat (6); `SolveTerminal` cases `Admitted` · `Exhausted` (2).
- Entry: `public static Fin<Vector<double>> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol)` — the route-spine entry gates the operand, realizes `Orthonormal.Modified` through the in-page modified Gram-Schmidt kernel, otherwise dispatches the selected substrate, and recomputes the true relative residual against the original operator; `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` drives the generated total `FactorizationKind.Switch` for the held-handle path; `public static Fin<(Factorization Sketch, double Truncation)> Sketch(Matrix<double> a, SketchPolicy policy)` builds a seed-replayable randomized range finder and retains both `Q` and the small `Svd` in `Factorization.Sketched`, so `Factorization.Solve(Matrix<double>)` applies `Qᵀrhs` before the reduced solve instead of misrepresenting the small factor as a factorization of `A`; `public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap)` streams N triangular solves through one held factorization; `public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol)` recovers the conditioning fallback from the route value.
- Auto: `LinearProvider.Select` and `DenseSubstrate.Select` run once at composition together — the former binds `LinearAlgebraControl.Provider` for the managed leg, the latter picks `NativeAten` where `AtenFloor.Resident` (the RID ships a `libtorch-cpu` CPU payload — osx-arm64/linux-x64/win-x64) and `Configure` sets the ATen OpenMP thread count and `set_default_dtype(Float64)`, falling to `Managed` otherwise; the two axes are orthogonal — the ATen leg replaces the whole `Matrix<double>` solve, never the MathNet provider behind it. `DenseRoute.Solve` branches on `DenseSubstrate.Active.Native`, route-discriminating the native `torch.linalg` factorization by the SAME `FactorRoute` case the managed leg switches on, never a `kind switch` cascade and never a per-call provider switch. `TolerancePolicy.Derive` seeds `SigmaMax` from `‖A‖_F` (`TensorPrimitives.Norm` over the flat column-major span, the O(n²) upper bound `σ_max ≤ ‖A‖_F`) and `‖b‖∞` from `TensorPrimitives.MaxMagnitude` — a fresh O(n³) `Svd` per tolerance derivation is the deleted hidden decomposition — refining through `WithSigma(Svd<double>)` exactly where a held handle already exists, so every threshold travels as one named record on the receipt and the dense residual path uses the one zero-alloc span primitive, never the allocating MathNet reduction; symmetry forces through `(A + A.Transpose()) * 0.5` before the definite kernel because `IsSymmetric()` compares by exact `!=`.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, the taken `FactorRoute` variant, the `TolerancePolicy` record, the recomputed true relative residual, the `DeterminismTag` substrate/provider/parallelism string (the `DenseSubstrate.Active.DeterminismTag` ATen-vs-managed prefix folded onto the provider triple so a cross-substrate cache hit is a distinct fingerprint), row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, TorchSharp, libtorch-cpu, HyperJet (the LM canonical exact-Jacobian scalar-AD leg), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new MathNet provider is one `LinearProvider` row with its RID predicate, rank, and inline `Control.TryUse*`/`Control.Use*` columns; a new execution substrate is one `DenseSubstrate` row with its `Available` probe and solve leg; a new operand shape is one `FactorRoute` case plus one `DenseRoute.Solve`/`AtenDense.Solve` arm; a new decomposition is one `Factorization` case plus one `FactorizationKind` row plus one `Decompose` `Switch` arm the generated total Switch breaks at compile time until it lands; a new sketch posture (Nyström, single-pass streaming) is one `SketchPolicy` row plus its `Sketch` arm, never a sibling decomposition owner; a new eigenbasis weight is one `EigenFilter` row; zero new surface.
- Boundary: the shape-spine union `FactorRoute` and the held-handle decomposition union `Factorization` are distinct C# symbols; an unused `computeVectors` knob on the solve route is deleted because every rank-revealing solve requires vectors internally, while `QRMethod` and modified-orthogonalization policy remain load-bearing case data. Identical operand `Matrix<double>` payloads never repeat on cases: the operand has ONE owner at the entrypoint. `Orthonormal` seats modified Gram-Schmidt as the `Modified` discriminant and collapses the built-in absolute/magnitude-squared/scale-relative rank thresholds into its one convention, never a sixth sibling factory. A monomorphic `double` element carrier is forced because the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative. `Admission` gates the flat column-major `Values` span through `TensorPrimitives.IsFiniteAll`/`IsNaNAny`/`IsInfinityAny` in one vectorized pass, never a strided per-element loop, and symmetry forces with `(A + A.Transpose()) * 0.5` before the call, never `MapIndexedInplace` self-averaging that mutates the backing array sequentially so a mirror entry is already modified when read. Singularity reads from `Cholesky<double>.DeterminantLn` because the determinant product underflows to zero with no signal, reflection tests `det < 0.0` never `det != 1.0`, and a `QR` construction checks the factor buffers all-finite because a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`. `TolerancePolicy` derives every threshold from operator and right-hand-side scale, so a bare per-module absolute literal in `1e-4..1e-8` is the unreplayable defect; conditioning rank is `Svd<double>.Rank` (`σ_max.EpsilonOf() · max(m,n)`) and never shares its slot with `Evd<double>.Rank`, and `ConditionNumber` is guarded against `+Inf` before gating because it is `+Inf` for rank-deficient operators. Iterative refinement forms its residual against the ORIGINAL operator in working precision through the in-place `Multiply(field, scratch)`/`Subtract` overloads streaming into one pre-sized `dx`/`scratch` pair, never against reconstructed factors which carry exactly the rounding error the correction cancels and never the allocating `held.Solve(rhs)` overload inside the loop; `Inverse()` in a hot loop is rejected because it clones the factors plus an `n²` identity crossing the large-object threshold at `n ≥ 104`, so a solve against an identity rides the retained pivoting handle with reused buffers. `SolveTerminal` maps budget-exhaustion to the `Exhausted` case carrying the partial iterate so the caller's relaxed-criterion retry survives, never `Fin.Fail`. Managed, native-OpenBLAS, and native-ATen legs diverge at the bit level, so the receipt `DeterminismTag` folds both the `DenseSubstrate.Active.DeterminismTag` substrate prefix and the provider type/parallelism triple, the `SolveDedupKey` folds that whole tag, and a dedup key omitting either dimension is the named correctness defect because a cross-substrate or cross-provider cache hit returns bit-divergent numbers. `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`/`DenseMatrix` wrapper is the deleted form mirroring the tensor-lane no-`TensorService` law.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinearProvider {
    public static readonly LinearProvider Managed = new("managed", rank: 0, probe: static () => true, activate: static () => Control.UseManaged());
    public static readonly LinearProvider NativeOpenBlas = new("native-openblas", rank: 1, probe: static () => Control.TryUseNativeOpenBLAS(), activate: static () => Control.UseNativeOpenBLAS());
    public static readonly LinearProvider NativeMkl = new("native-mkl", rank: 2, probe: static () => Control.TryUseNativeMKL(), activate: static () => Control.UseNativeMKL());

    private readonly Func<bool> probe;
    private readonly Action activate;

    public int Rank { get; }

    public bool Available => probe();

    public static LinearProvider Select(Option<BenchmarkRow> claim) =>
        toSeq(toSeq(Items)
            .Filter(static row => row.Available)
            .OrderByDescending(row => claim.Map(c => StringComparer.Ordinal.Equals(c.Route, row.Key) ? int.MaxValue : row.Rank).IfNone(row.Rank)))
            .Head
            .Map(static row => { row.activate(); return row; })
            .IfNone(static () => { Managed.activate(); return Managed; });

    public string DeterminismTag =>
        $"{Key}:{LinearAlgebraControl.Provider.GetType().Name}:{Control.MaxDegreeOfParallelism}";

    public UInt128 SolveDedupKey(UInt128 problemDigest) {
        byte[] tag = Encoding.UTF8.GetBytes(DeterminismTag);
        byte[] frame = GC.AllocateUninitializedArray<byte>(tag.Length + 16);
        tag.CopyTo(frame, 0);
        BinaryPrimitives.WriteUInt64LittleEndian(frame.AsSpan(tag.Length), (ulong)problemDigest);
        BinaryPrimitives.WriteUInt64LittleEndian(frame.AsSpan(tag.Length + 8), (ulong)(problemDigest >> 64));
        return XxHash128.HashToUInt128(frame);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenseSubstrate {
    public static readonly DenseSubstrate Managed = new("managed", native: false, probe: static () => true, activate: static () => { });
    public static readonly DenseSubstrate NativeAten = new("native-aten", native: true, probe: static () => AtenFloor.Resident, activate: AtenFloor.Configure);

    private readonly Func<bool> probe;
    private readonly Action activate;

    public bool Native { get; }
    public bool Available => probe();

    public static DenseSubstrate Active { get; private set; } = Managed;

    // Native ATen leg is the osx-arm64 dense substrate the x64-only OpenBLAS/MKL providers cannot serve; the
    // MathNet Matrix<double> route stays the managed cold-start terminal. Selection runs once at composition.
    public static DenseSubstrate Select() =>
        Active = NativeAten.Available ? Bind(NativeAten) : Bind(Managed);

    static DenseSubstrate Bind(DenseSubstrate s) { s.activate(); return s; }

    public string DeterminismTag =>
        Native ? $"{Key}:aten:omp{torch.get_num_threads()}" : Key;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FactorizationKind {
    public static readonly FactorizationKind Lu = new("lu");
    public static readonly FactorizationKind Qr = new("qr");
    public static readonly FactorizationKind Cholesky = new("cholesky");
    public static readonly FactorizationKind Svd = new("svd");
    public static readonly FactorizationKind Evd = new("evd");
}

// `Derive` seeds `SigmaMax` from the O(n²) Frobenius bound; an existing `Svd` refines through `WithSigma`
// without paying a second decomposition for a threshold.
public sealed record TolerancePolicy(double SigmaMax, double FrobeniusNorm, double RhsInfinityNorm, int MaxDim, double RankFloor, double ResidualCap) {
    public static TolerancePolicy Derive(Matrix<double> a, Vector<double> rhs) {
        double[] flat = a.ToColumnMajorArray();
        double[] b = rhs.AsArray() ?? rhs.ToArray();
        double frobenius = TensorPrimitives.Norm<double>(flat);
        int maxDim = Math.Max(a.RowCount, a.ColumnCount);
        return new TolerancePolicy(
            SigmaMax: frobenius,
            FrobeniusNorm: frobenius,
            RhsInfinityNorm: Math.Abs(TensorPrimitives.MaxMagnitude<double>(b)),
            MaxDim: maxDim,
            RankFloor: frobenius.EpsilonOf() * maxDim,
            ResidualCap: Math.ScaleB(16.0, -52) * Math.Max(1.0, frobenius));
    }

    public TolerancePolicy WithSigma(Svd<double> held) =>
        held.L2Norm is var sigma && double.IsFinite(sigma)
            ? this with { SigmaMax = sigma, RankFloor = sigma.EpsilonOf() * MaxDim, ResidualCap = Math.ScaleB(16.0, -52) * Math.Max(1.0, sigma) }
            : this;

    public bool Admits(double residual) => double.IsFinite(residual) && residual <= ResidualCap;
}

// Cases carry ONLY per-occurrence factorization policy; the operand Matrix<double> is the entrypoint's own
// argument, so route identity never restates the operand and no per-case `A` re-projection switch exists.
[Union]
public abstract partial record FactorRoute {
    private FactorRoute() { }

    public sealed record DefinitePsd : FactorRoute;
    public sealed record SquarePivoting : FactorRoute;
    public sealed record Orthonormal(QRMethod Mode, bool Modified) : FactorRoute;
    public sealed record Spectral(Symmetricity Sym) : FactorRoute;
    public sealed record RankRevealing : FactorRoute;
}

public sealed record SketchPolicy(int Rank, int Oversample, int PowerIterations, double TruncationCap, int Seed) {
    public static readonly SketchPolicy Rom = new(Rank: 64, Oversample: 10, PowerIterations: 2, TruncationCap: 1e-6, Seed: 0);
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

    // MathNet `Cholesky()` throws on a non-square or non-PD operand; `Try.lift` captures that seam and
    // `DeterminantLn` finiteness rejects a degenerate factor that did not throw.
    public static Fin<Cholesky<double>> Definite(Matrix<double> spd) =>
        spd.RowCount != spd.ColumnCount
            ? Fin.Fail<Cholesky<double>>(new ComputeFault.ModelRejected($"<non-square-spd:{spd.RowCount}x{spd.ColumnCount}>"))
            : Try.lift(() => spd.Cholesky()).Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<non-spd:{error.Message}>"))
                .Bind(static chol => double.IsFinite(chol.DeterminantLn)
                    ? Fin.Succ(chol)
                    : Fin.Fail<Cholesky<double>>(new ComputeFault.ModelRejected("<spd-degenerate-logdet>")));

    public static Fin<QR<double>> Orthonormal(Matrix<double> a, QRMethod mode, double floor) =>
        a.QR(mode) is var qr && qr.R.Diagonal().Map(Math.Abs).All(value => double.IsFinite(value) && value >= floor)
            ? Fin.Succ(qr)
            : Fin.Fail<QR<double>>(new ComputeFault.ModelRejected("<rank-deficient-qr>"));

    public static Fin<Vector<double>> Modified(Matrix<double> a, Vector<double> rhs, double floor) {
        int rows = a.RowCount;
        int columns = a.ColumnCount;
        Matrix<double> q = Matrix<double>.Build.Dense(rows, columns);
        Matrix<double> r = Matrix<double>.Build.Dense(columns, columns);
        for (int column = 0; column < columns; column++) {
            Vector<double> v = a.Column(column);
            for (int basis = 0; basis < column; basis++) {
                Vector<double> qi = q.Column(basis);
                double projection = qi.DotProduct(v);
                r[basis, column] = projection;
                v.Subtract(qi.Multiply(projection), v);
            }
            double norm = v.L2Norm();
            if (!double.IsFinite(norm) || norm < floor) {
                return Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<rank-deficient-modified-gram-schmidt:column={column}:norm={norm:e3}:floor={floor:e3}>"));
            }
            r[column, column] = norm;
            q.SetColumn(column, v.Divide(norm));
        }
        return Fin.Succ(r.Solve(q.TransposeThisAndMultiply(rhs)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Factorization {
    private Factorization() { }

    public sealed record Lu(LU<double> Decomposition) : Factorization;
    public sealed record Qr(QR<double> Decomposition) : Factorization;
    public sealed record Cholesky(Cholesky<double> Decomposition) : Factorization;
    public sealed record Svd(Svd<double> Decomposition) : Factorization;
    public sealed record Evd(Evd<double> Decomposition) : Factorization;
    public sealed record Sketched(Matrix<double> Range, Svd<double> Core, double Truncation) : Factorization;

    public FactorizationKind Kind => Switch(
        lu: static _ => FactorizationKind.Lu, qr: static _ => FactorizationKind.Qr, cholesky: static _ => FactorizationKind.Cholesky,
        svd: static _ => FactorizationKind.Svd, evd: static _ => FactorizationKind.Evd, sketched: static _ => FactorizationKind.Svd);

    public Matrix<double> Solve(Matrix<double> rhs) => Switch(
        state: rhs,
        lu: static (b, f) => f.Decomposition.Solve(b),
        qr: static (b, f) => f.Decomposition.Solve(b),
        cholesky: static (b, f) => f.Decomposition.Solve(b),
        svd: static (b, f) => f.Decomposition.Solve(b),
        evd: static (b, f) => f.Decomposition.Solve(b),
        sketched: static (b, f) => f.Core.Solve(f.Range.TransposeThisAndMultiply(b)));
}

// ATen dispatch preserves the managed `FactorRoute` structure, runs under public `torch.inference_mode(true)`,
// and returns only `double[]`; `DenseRoute.Solve` witnesses both substrates against the original operator.
public static class AtenFloor {
    public static bool Resident =>
        RuntimeInformation.RuntimeIdentifier is "osx-arm64" or "linux-x64" or "win-x64";

    public static void Configure() {
        torch.set_num_threads(Environment.ProcessorCount);
        torch.set_default_dtype(ScalarType.Float64);
    }
}

public static class AtenDense {
    public sealed class HeldFactor(Tensor lu, Tensor pivots) : IDisposable {
        public Fin<Vector<double>> Solve(Vector<double> rhs, bool transpose = false) => HeldSolve(lu, pivots, rhs, transpose);
        public void Dispose() {
            pivots.Dispose();
            lu.Dispose();
        }
    }

    // Definite and spectral routes symmetrize before ingress, then select native factorization by the same
    // `FactorRoute` case as the managed leg.
    public static Fin<Vector<double>> Solve(FactorRoute route, Matrix<double> matrix, Vector<double> rhs, TolerancePolicy tol) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Matrix<double> operand = route is FactorRoute.DefinitePsd or FactorRoute.Spectral ? Admission.Symmetrize(matrix) : matrix;
        Tensor a = torch.from_array(operand.ToColumnMajorArray(), ScalarType.Float64).reshape(operand.ColumnCount, operand.RowCount).t();
        Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
        return route.Switch(
            definitePsd:    _ => Spd(a, b),
            squarePivoting: _ => General(a, b),
            orthonormal:    _ => LeastSquares(a, b, tol),
            spectral:       _ => SymmetricIndefinite(a, b),
            rankRevealing:  _ => LeastSquares(a, b, tol));
    }

    // SPD: Cholesky factor + triangular `cholesky_solve`, info-gated — the structure the general `solve_ex`
    // discards; the `_ex` info tensor is the typed-fault rail, never a caught native throw.
    static Fin<Vector<double>> Spd(Tensor a, Tensor b) {
        (Tensor l, Tensor info) = torch.linalg.cholesky_ex(a, check_errors: false);
        return info.ReadCpuInt32(0) == 0
            ? Egress(torch.cholesky_solve(b, l, upper: false))
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-cholesky-nonspd:info={info.ReadCpuInt32(0)}>"));
    }

    // Symmetric-indefinite route uses Bunch-Kaufman `ldl_factor_ex`/`ldl_solve`; nullable pivots/info gate
    // before solve, and the downstream witness catches a null-info residual breach.
    static Fin<Vector<double>> SymmetricIndefinite(Tensor a, Tensor b) {
        (Tensor ld, Tensor? pivots, Tensor? info) = torch.linalg.ldl_factor_ex(a, hermitian: true, check_errors: false);
        int status = info is { } reported ? reported.ReadCpuInt32(0) : 0;
        return pivots is { } p && status == 0
            ? Egress(torch.linalg.ldl_solve(ld, p, b, hermitian: true))
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-ldl-singular:info={status}:pivots={pivots is not null}>"));
    }

    // General square: pivoted-LU `solve_ex`, info-gated.
    static Fin<Vector<double>> General(Tensor a, Tensor b) {
        (Tensor result, Tensor info) = torch.linalg.solve_ex(a, b, left: true, check_errors: false);
        return info.ReadCpuInt32(0) == 0
            ? Egress(result)
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-solve-singular:info={info.ReadCpuInt32(0)}>"));
    }

    // `lstsq` rank always gates; the sigma floor binds only where the driver yields the spectrum — the
    // driverless TorchSharp surface runs CPU `gelsy`, which reports rank but returns EMPTY singular values,
    // so an unconditional finite-sigma demand rejects every least-squares solve.
    static Fin<Vector<double>> LeastSquares(Tensor a, Tensor b, TolerancePolicy tol) {
        (Tensor solution, Tensor residuals, Tensor rank, Tensor singular) = torch.linalg.lstsq(a, b);
        long observed = rank.NumberOfElements > 0 ? rank.ReadCpuInt64(0) : Math.Min(a.shape[0], a.shape[1]);
        Option<double> sigmaMin = singular.NumberOfElements > 0 ? Some(singular.ReadCpuDouble(singular.NumberOfElements - 1)) : None;
        bool sigmaAdmits = sigmaMin.Match(Some: s => double.IsFinite(s) && s >= tol.RankFloor, None: () => true);
        return observed == Math.Min(a.shape[0], a.shape[1]) && sigmaAdmits
            ? Egress(solution)
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-lstsq-rank-deficient:rank={observed}:sigma-min={sigmaMin.Map(static s => s.ToString("e3")).IfNone("absent")}:floor={tol.RankFloor:e3}>"));
    }

    // `lu_factor` pays O(n³) once; `lu_solve` streams right-hand sides and its adjoint mode recovers the
    // transpose solve without refactorization.
    public static Fin<HeldFactor> Held(Matrix<double> operand) {
        using DisposeScope owner = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        return Try.lift(() => {
                using Tensor a = torch.from_array(operand.ToColumnMajorArray(), ScalarType.Float64).reshape(operand.ColumnCount, operand.RowCount).t();
                (Tensor lu, Tensor pivots) = torch.linalg.lu_factor(a, pivot: true);
                owner.Detach(lu);
                owner.Detach(pivots);
                return new HeldFactor(lu, pivots);
            }).Run()
            .MapFail(error => (Error)new ComputeFault.ModelRejected($"<aten-lu-factor:{error.Message}>"));
    }

    public static Fin<Vector<double>> HeldSolve(Tensor lu, Tensor pivots, Vector<double> rhs, bool transpose = false) {
        using DisposeScope scope = torch.NewDisposeScope();
        Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
        return Egress(torch.linalg.lu_solve(lu, pivots, b, left: true, adjoint: transpose));
    }

    // All-dense native contraction collapses to one `torch.einsum`; mixed and managed plans retain the
    // pairwise fold owned in `factor.md`.
    public static Fin<Matrix<double>> Einsum(string spec, Seq<Matrix<double>> operands) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Tensor[] lifted = [.. operands.Map(static m =>
            torch.from_array(m.ToColumnMajorArray(), ScalarType.Float64).reshape(m.ColumnCount, m.RowCount).t())];
        Tensor result = torch.einsum(spec, lifted);
        return result.shape.Length == 2
            ? Fin.Succ(Matrix<double>.Build.Dense((int)result.shape[0], (int)result.shape[1], result.t().reshape(result.NumberOfElements).data<double>().ToArray()))
            : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected($"<aten-einsum-rank:{result.shape.Length}>"));
    }

    public static Fin<Matrix<double>> MultiDot(Seq<Matrix<double>> operands) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Tensor[] lifted = [.. operands.Map(static matrix =>
            torch.from_array(matrix.ToColumnMajorArray(), ScalarType.Float64).reshape(matrix.ColumnCount, matrix.RowCount).t())];
        Tensor result = torch.linalg.multi_dot(lifted);
        return result.shape.Length == 2
            ? Fin.Succ(Matrix<double>.Build.Dense((int)result.shape[0], (int)result.shape[1], result.t().reshape(result.NumberOfElements).data<double>().ToArray()))
            : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected($"<aten-multi-dot-rank:{result.shape.Length}>"));
    }

    static Fin<Vector<double>> Egress(Tensor x) =>
        Fin.Succ(Vector<double>.Build.DenseOfArray(x.reshape(x.NumberOfElements).data<double>().ToArray()));
}

public static class DenseRoute {
    // Substrate legs return an unwitnessed solution; one original-operator gate gives either leg an identical
    // typed residual rejection.
    public static Fin<Vector<double>> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol) =>
        Admission.Admit(operand).Bind(_ =>
            (route is FactorRoute.Orthonormal { Modified: true }
                ? Admission.Modified(operand, rhs, tol.RankFloor)
                : DenseSubstrate.Active.Native ? AtenDense.Solve(route, operand, rhs, tol) : Managed(route, operand, rhs, tol))
                .Bind(x => Witness(operand, x, rhs, tol)));

    static Fin<Vector<double>> Managed(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol) =>
        route.Switch<(Matrix<double> A, double Floor), Fin<ISolver<double>>>(
                state: (A: operand, Floor: tol.RankFloor),
                definitePsd:    static (s, _) => Admission.Definite(Admission.Symmetrize(s.A)).Map(static h => (ISolver<double>)h),
                squarePivoting: static (s, _) => Fin.Succ((ISolver<double>)s.A.LU()),
                orthonormal:    static (s, c) => Admission.Orthonormal(s.A, c.Mode, s.Floor).Map(static h => (ISolver<double>)h),
                spectral:       static (s, c) => Fin.Succ((ISolver<double>)Admission.Symmetrize(s.A).Evd(c.Sym)),
                rankRevealing:  static (s, _) => Fin.Succ((ISolver<double>)s.A.Svd(computeVectors: true)))
            .Map(solver => solver.Solve(rhs));

    public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol) =>
        Solve(primary, operand, rhs, tol)
            .Map(x => (SolveTerminal)new SolveTerminal.Admitted(x, Relative(operand, x, rhs)))
            .BindFail(_ => Solve(secondary, operand, rhs, tol).Map(x => (SolveTerminal)new SolveTerminal.Admitted(x, Relative(operand, x, rhs))));

    static Fin<Vector<double>> Witness(Matrix<double> a, Vector<double> x, Vector<double> rhs, TolerancePolicy tol) =>
        Relative(a, x, rhs) is var residual && tol.Admits(residual)
            ? Fin.Succ(x)
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<witness-fail:residual={residual:e3}:cap={tol.ResidualCap:e3}>"));

    static double Relative(Matrix<double> a, Vector<double> x, Vector<double> rhs) {
        Vector<double> residual = rhs - a.Multiply(x);
        double[] flat = residual.AsArray() ?? residual.ToArray();
        double[] b = rhs.AsArray() ?? rhs.ToArray();
        return TensorPrimitives.Norm<double>(flat) / Math.Max(1.0, TensorPrimitives.Norm<double>(b));
    }
}

public static class DenseOps {
    // Generated total `FactorizationKind.Switch` makes a new row require a build arm at compile time;
    // reference-typed `matrix` threads as switch state without span-lane restrictions.
    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        kind.Switch(
            state: matrix,
            lu: static m => Fin.Succ<Factorization>(new Factorization.Lu(m.LU())),
            qr: static m => Fin.Succ<Factorization>(new Factorization.Qr(m.QR())),
            cholesky: static m => Admission.Definite(Admission.Symmetrize(m)).Map(static h => (Factorization)new Factorization.Cholesky(h)),
            svd: static m => Fin.Succ<Factorization>(new Factorization.Svd(m.Svd(computeVectors: true))),
            evd: static m => Fin.Succ<Factorization>(new Factorization.Evd(m.Evd())));

    public static IO<Fin<Matrix<double>>> Gemm(Matrix<double> left, Matrix<double> right, ShardPlan plan) =>
        plan.Lower(left, right);

    // Halko range capture computes `Y = (A·Aᵀ)^q·A·Ω`, thin-QR, then the small SVD of `QᵀA`.
    // A-posteriori `‖A − Q·QᵀA‖_F/‖A‖_F` rejects insufficient sketch rank.
    public static Fin<(Factorization Sketch, double Truncation)> Sketch(Matrix<double> a, SketchPolicy policy) {
        if (policy.Rank < 1 || policy.Oversample < 0 || policy.PowerIterations < 0 || !double.IsFinite(policy.TruncationCap) || policy.TruncationCap < 0.0) {
            return Fin.Fail<(Factorization, double)>(new ComputeFault.ModelRejected($"<sketch-policy:rank={policy.Rank}:oversample={policy.Oversample}:power={policy.PowerIterations}:cap={policy.TruncationCap:e3}>"));
        }
        int width = Math.Min(policy.Rank + policy.Oversample, Math.Min(a.RowCount, a.ColumnCount));
        Matrix<double> omega = Gaussian(a.ColumnCount, width, policy.Seed);
        Matrix<double> y = Enumerable.Range(0, policy.PowerIterations)
            .Aggregate(a.Multiply(omega), (range, _) => a.Multiply(a.TransposeThisAndMultiply(range)));
        Matrix<double> q = y.QR(QRMethod.Thin).Q;
        Matrix<double> b = q.TransposeThisAndMultiply(a);
        Svd<double> small = b.Svd(computeVectors: true);
        double truncation = (a - q.Multiply(b)).FrobeniusNorm() / Math.Max(1.0, a.FrobeniusNorm());
        return double.IsFinite(truncation) && truncation <= policy.TruncationCap
            ? Fin.Succ(((Factorization)new Factorization.Sketched(q, small, truncation), truncation))
            : Fin.Fail<(Factorization, double)>(new ComputeFault.ModelRejected($"<sketch-truncation:{truncation:e3}:cap={policy.TruncationCap:e3}:rank={policy.Rank}>"));
    }

    static Matrix<double> Gaussian(int rows, int columns, int seed) {
        double[] values = new double[rows * columns];
        new Normal(0.0, 1.0, new Random(seed)).Samples(values);
        return Matrix<double>.Build.Dense(rows, columns, values);
    }

    public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(
        Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap) {
        Vector<double> dx = Vector<double>.Build.Dense(rhs.Count);
        Vector<double> scratch = Vector<double>.Build.Dense(rhs.Count);
        double rhsNorm = Math.Max(1.0, rhs.L2Norm());
        (Vector<double> Field, int Refinements, double Residual) folded = toSeq(Enumerable.Range(0, cap)).Fold(
            (Field: held.Solve(rhs), Refinements: 0, Residual: double.MaxValue),
            (acc, _) => {
                // Initial `MaxValue` forces one sweep; an admitted residual freezes the iterate and avoids
                // redundant O(n²) multiplies through the remaining bounded fold.
                if (tol.Admits(acc.Residual)) { return acc; }
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
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            RouteVariant = route.GetType().Name, DeterminismTag = $"{DenseSubstrate.Active.DeterminismTag}|{provider.DeterminismTag}", ResidualCap = tol.ResidualCap, TrueResidual = residual,
        };
}
```

### [02.1]-[LEVENBERG_MARQUARDT]

- Owner: `LevenbergMarquardt` the damped Gauss-Newton nonlinear least-squares owner minimizing `‖r(θ)‖²`, solving each step's normal-equation system `(JᵀJ + λ·diag(JᵀJ))·δ = −Jᵀr` through the lane's gated `Admission.Definite` SPD route (the damped normal matrix is symmetric-PD by construction), adapting the damping λ on the actual-versus-predicted reduction; `LmPolicy` the iteration policy record; `LmResult` the typed convergence carrier. This is the one Compute-INTERNAL nonlinear-least-squares owner, serving Compute's own solves and the host-free graduation/inference peers over the wire.
- Entry: ONE `Minimize` discriminating on the residual's input shape — the HYPERJET arm (`Func<DDScalar[], DDScalar[]>` residual authored once over the hyper-dual scalar) derives the EXACT Jacobian row-by-row through `GetGradient()` and is the CANONICAL provider (the `Stats/estimator` ARMA/Holt/state-space fits and the `Solver/uncertainty` FORM/SORM exact-AD row all arrive on this arm; the finite-difference fall those consumers carried is deleted), while the black-box arm (`Func<Vector<double>, Matrix<double>>` caller-supplied Jacobian) survives for residuals authored outside the hyper-dual reach; both converge on the identical damped fold.
- Boundary: the AEC-domain `Rasm.Materials` does NOT reference this owner — the strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so the Materials BRDF fit stays in-folder and the algorithms-doc thin-QR fit is a doctrine reference a Materials probe cites; a `Rasm.Compute` project reference or a "MathNet transitive via Rasm.Compute" claim from Materials is the forbidden AEC→app-platform edge. A linear least-squares stays on the one-shot `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR; LM is the nonlinear damped iteration. A hand-rolled finite-difference Jacobian beside the HyperJet arm is the deleted form — FD survives ONLY where the residual is a genuine black box no hyper-dual instantiation can reach (the honest caller-supplied-Jacobian arm). This kernel dropped its own MKL reference (x86-64-only, cannot load on osx-arm64) and flagged Compute's as sibling-roster debt: the `native-mkl` `LinearProvider` row above IS the resolution — the RID-claim `Available` filter is the design (osx-arm64 resolves managed/OpenBLAS, an x64 deployment claims MKL), recorded here so the kernel's flag closes.

```csharp signature
public sealed record LmPolicy(int MaxIterations, double GradientTolerance, double StepTolerance, double InitialDamping, double DampingUp, double DampingDown) {
    public static readonly LmPolicy Canonical = new(MaxIterations: 200, GradientTolerance: 1e-10, StepTolerance: 1e-12, InitialDamping: 1e-3, DampingUp: 10.0, DampingDown: 0.1);
}

public sealed record LmResult(Vector<double> Parameters, double Residual, int Iterations, bool Converged);

public static class LevenbergMarquardt {
    // `DDScalar.Variables` seeds each parameter; every residual contributes its primal `Value` and exact
    // `GetGradient()` Jacobian row from one hyper-dual authoring.
    public static Fin<LmResult> Minimize(Func<DDScalar[], DDScalar[]> residual, Vector<double> initial, LmPolicy policy) {
        // Reference-keyed memoization shares one hyper-dual pass between residual and Jacobian projections;
        // identical `theta` inside a step never seeds AD twice.
        (Vector<double> Theta, DDScalar[] Dual) cache = (initial, residual(DDScalar.Variables(initial.ToArray(), order: 1)));
        DDScalar[] At(Vector<double> theta) {
            if (!ReferenceEquals(cache.Theta, theta)) {
                cache = (theta, residual(DDScalar.Variables(theta.ToArray(), order: 1)));
            }
            return cache.Dual;
        }
        return Minimize(
            theta => Vector<double>.Build.Dense([.. At(theta).Select(static c => c.Value)]),
            theta => {
                DDScalar[] r = At(theta);
                Matrix<double> j = Matrix<double>.Build.Dense(r.Length, theta.Count);
                for (int row = 0; row < r.Length; row++) { j.SetRow(row, r[row].GetGradient()); }
                return j;
            },
            initial, policy);
    }

    public static Fin<LmResult> Minimize(Func<Vector<double>, Vector<double>> residual, Func<Vector<double>, Matrix<double>> jacobian, Vector<double> initial, LmPolicy policy) {
        Vector<double> theta = initial;
        double lambda = policy.InitialDamping;
        (Vector<double> Theta, double Lambda, double Cost, int Iterations, bool Done) folded = toSeq(Enumerable.Range(0, policy.MaxIterations)).Fold(
            (Theta: theta, Lambda: lambda, Cost: Cost(residual(theta)), Iterations: 0, Done: false),
            (state, _) => {
                if (state.Done) { return state; }
                Vector<double> r = residual(state.Theta);
                Matrix<double> j = jacobian(state.Theta);
                Matrix<double> jtj = j.TransposeThisAndMultiply(j);
                Vector<double> jtr = j.TransposeThisAndMultiply(r);
                Matrix<double> damped = jtj + Matrix<double>.Build.DiagonalOfDiagonalVector(jtj.Diagonal()) * state.Lambda;
                // Gated SPD admission turns a rank-deficient damped factor into the LM increase-λ response,
                // never an escaped `Cholesky()` exception.
                return Admission.Definite(damped).Match(
                    Succ: chol => {
                        Vector<double> step = chol.Solve(-jtr);
                        Vector<double> candidate = state.Theta + step;
                        double trial = Cost(residual(candidate));
                        // Gain ratio ρ = actual/predicted reduction under the damped quadratic model:
                        // predicted = ½·δᵀ(λ·D·δ − Jᵀr) with D = diag(JᵀJ); acceptance and λ adaptation
                        // read model agreement, never the bare cost-decrease sign.
                        double predicted = 0.5 * step.DotProduct(jtj.Diagonal().PointwiseMultiply(step) * state.Lambda - jtr);
                        double rho = predicted > 0.0 ? (state.Cost - trial) / predicted : double.NegativeInfinity;
                        return rho > 0.0
                            ? (candidate, Math.Max(1e-12, state.Lambda * policy.DampingDown), trial, state.Iterations + 1, jtr.InfinityNorm() < policy.GradientTolerance || step.L2Norm() < policy.StepTolerance)
                            : (state.Theta, state.Lambda * policy.DampingUp, state.Cost, state.Iterations + 1, false);
                    },
                    Fail: _ => (state.Theta, state.Lambda * policy.DampingUp, state.Cost, state.Iterations + 1, false));
            });
        return double.IsFinite(folded.Cost)
            ? Fin.Succ(new LmResult(folded.Theta, Math.Sqrt(2.0 * folded.Cost), folded.Iterations, folded.Done))
            : Fin.Fail<LmResult>(new ComputeFault.ModelRejected($"<lm-nonfinite-cost>"));
    }

    static double Cost(Vector<double> r) => 0.5 * r.DotProduct(r);
}
```

### [02.2]-[SPECTRAL_LAW]

- Owner: `SpectralResult` `[Union]` carrying distinct dense-symmetric and dense-general cases — the `Symmetricity` flag selects five output axes together (eigenvector norm, real versus block-diagonal `D`, single-column versus column-pair encoding, ascending versus Schur-deflation order, working versus norm-gated solve); `SpectralOps.Decompose` the one constructor projecting `Evd<double>` onto the matched `SpectralResult` case; `SpectralOps.Modal` the Schur-pair decoder; `SpectralOps.Defect` the block eigen-residual; `EigenFilter` `[SmartEnum<string>]` the closed eigenbasis weight vocabulary excluding the zero mode.
- Entry: `public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym)` builds the `Symmetric` case from the real eigenvalues and the orthonormal vectors for a symmetric/Hermitian spectrum and the `General` case decoding the Schur pairs for the nonsymmetric spectrum, each carrying its block defect; `public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values)` decodes real conjugate pairs from adjacent columns dispatched on `Math.Sign(values[j].Imaginary)`; `public static double Defect(Matrix<double> a, Matrix<double> vectors, Matrix<double> d)` computes `(A·V − V·D).FrobeniusNorm()`; `public static Fin<Vector<double>> Filtered(Evd<double> evd, EigenFilter weight, double zeroFloor)` applies the eigenbasis filter carrying the weight sum as evidence.
- Auto: `Decompose` is the single producer of `SpectralResult` — `SpectralOps.Modal`/`Defect`/`Filtered` are the per-axis kernels it composes and never independent return surfaces, so the result-union owner is always constructed and never unwired; `Modal` reads `Column(j)`+`Column(j+1)` for a positive-imaginary pair and `Column(j-1)`+`Column(j)` for a negative-imaginary pair, never `Column(j)` whole because that discards the imaginary half; `Defect` is the one signal both the managed throw rail and the native in-band info-code rail surface identically since no built-in eigen residual exists; `EigenFilter` weights each `EigenValues` entry excluding the zero mode (`λ < ε_zero ? 0.0 : f(λ)`, excluded never clamped) and fails a fully-excluded spectrum rather than reading it as a zero signal.
- Boundary: `EigenValues` interprets `EigenVectors` because no parallel pairing array exists; nonsymmetric columns `Normalize(2)` before any modal weight because recovered columns are raw triangular solutions with arbitrary per-column norms; Hermitian eigenvectors stay complex because projecting them to real parts is incorrect; the library `Determinant`/`Rank`/`IsFullRank` are rejected in domain logic because `Determinant` short-circuits to `0.0` the moment any eigenvalue crosses the absolute zero test; eigenvalue equality is never asserted tighter than the convergence band because the exceptional-shift escape bakes the literal `0.964` into the last bits; only `DenseMatrix` reaches the native `EigenDecomp` and the managed `Evd` kernels are serial regardless of degree, so sign, ordering, and last bits differ across the seam and provider-mismatched eigenvector comparison short-circuits to span equivalence; `SpectralResult` is the only spectral return — a raw `Matrix<Complex>`/`double`/`Fin<Vector<double>>` leaking the spectral verdict past the owner is the deleted form because the consumer must dispatch on the `Symmetric`/`General` case to read the right `D` shape and ordering contract.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
    public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym) {
        if (sym is Symmetricity.Symmetric or Symmetricity.Hermitian) {
            return new SpectralResult.Symmetric(evd.EigenVectors, evd.EigenValues.Map(static v => v.Real), Defect(a, evd.EigenVectors, evd.D));
        }
        // Schur-pair modal matrix is built once and shared by result and defect; constructing it
        // twice (once for the carrier, once for the residual) doubles the complex-column reconstruction.
        Matrix<Complex> modal = Modal(evd.EigenVectors, evd.EigenValues);
        return new SpectralResult.General(modal, evd.EigenValues, ComplexDefect(a, modal, evd.EigenValues));
    }

    static double ComplexDefect(Matrix<double> a, Matrix<Complex> modal, Vector<Complex> values) {
        Matrix<Complex> aComplex = Matrix<Complex>.Build.DenseOfColumnMajor(a.RowCount, a.ColumnCount, a.ToColumnMajorArray().Select(static r => new Complex(r, 0.0)));
        Matrix<Complex> dComplex = Matrix<Complex>.Build.DenseOfDiagonalVector(values);
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

## [03]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — plus the `OnlineStat` fourth-order residual-moment accumulator the numeric lane owns because `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `ModelResultIndex.Claim(rows, fingerprint)` resolved at composition against the running fingerprint under the index-owned `RecencyHorizon` and clock — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot()` captures the `LinearAlgebraControl.Provider` `ToString` tag, the provider type name, and the public `Control.MaxDegreeOfParallelism` degree at solve construction because every kernel reads this ambient `LinearAlgebraControl.Provider` static at execution instant (`Control` exposes no `LinearAlgebraProvider` member — the active handle is `LinearAlgebraControl.Provider`; the `ParallelizeOrder`/`ParallelizeElements` thresholds are `internal` to `Control` and unreadable, so the determinism triple is the public provider/type/degree); `OnlineStat.Push(residual)` folds each witnessed solve residual into the running fourth-order moment stream under the `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `ModelResultIndex.Claim` owner and threaded in, never re-resolved here; bit-versus-envelope equality derives from the provider/type/degree triple because the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, counts sharded sub-blocks by node, and accumulates the online residual fourth-order moments through the `OnlineStat` `MomentNormalizer`-policy merge whose `Combine` is a CAS-safe pure reduction asserting `combined.Count == a.Count + b.Count`; the stream guards at admission through the same all-finite predicate the operands cross because one pushed `NaN` permanently poisons every moment with no reset.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, route variant, tolerance, true residual, determinism tag, symbolic fill, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations`, `rasm.compute.solve.residual`, and `rasm.compute.solve.shards` instruments are owned by `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here; the `OnlineStat` accumulator is the numeric-lane moment owner whose skewness/kurtosis evidence feeds the residual histogram tail.
- Packages: System.Numerics.Tensors, Rasm.Persistence (project), LanguageExt.Core, BCL inbox
- Growth: a new claim dimension is one column on the existing `BenchmarkClaim`; a new solve instrument is one row on `ReceiptSurface.Instruments`; a new moment is one field on `OnlineStat` plus one merge term; zero new surface.
- Boundary: provider rank is the `BenchmarkClaim` `Provider` column gated exactly like the SIMD and partition claims — a static native default beside the claim is the named defect; the claim is resolved by the Persistence `ModelResultIndex.Claim` owner whose recency horizon and clock are closed inside the index and threaded in, never re-resolved and never a second horizon; the solve and shard instruments live on the `ReceiptSurface.Instruments` stream and a second numeric-lane-local instrument owner is the deleted form; the online residual accumulator accumulates to fourth order (mean, M2, M3, M4) and serializes for distributed aggregation because parallel online moments accumulate to fourth order, records the running-versus-moving distinction and the `MomentNormalizer` Bessel-versus-population policy enum because unmarked mixing silently corrupts every downstream confidence computation, and one pushed `NaN` permanently poisons every moment so the stream guards at admission through the same all-finite predicate the operands cross; the merge identity holds only to the floating-point merge envelope.

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
        new(LinearAlgebraControl.Provider.ToString() ?? string.Empty, LinearAlgebraControl.Provider.GetType().Name, Control.MaxDegreeOfParallelism);

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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [NATIVE_PROVIDER_EXECUTION]-[BLOCKED]: does `Control.TryUseNativeOpenBLAS()`/`TryUseNativeMKL()` return `true` and `LinearAlgebraControl.Provider` bind the native `ILinearAlgebraProvider`, and does the CSparse native sparse path load; a win-x64/linux-x64 host carrying the native asset, both rows dormant on osx-arm64.
