# [COMPUTE_BLAS]

Rasm.Compute dense linear-algebra lane: BLAS-class dense linear algebra over the admitted MathNet provider stack and the native `TorchSharp` ATen substrate, admitted once and routed by operand shape — definite, square, overdetermined, symmetric, periodic-grid — never by the call site and never by a knob riding beside the matrix. Every library refuses its own gates, so `Admission` re-imposes each refused gate and every result leaves as a typed `ComputeReceipt.Factorization` carrying the route variant, the scale-derived tolerance, the provider determinism tag, and the recomputed true relative residual against the original operator, never a `Matrix<double>`/`Vector<double>` or factorization instance.

`LinearProvider` ranks native MKL then OpenBLAS then the managed terminal by RID claim; `DenseSubstrate` routes the dense solve to `torch.linalg` over the vendored CPU ATen payload wherever a one-shot LOAD probe proves that payload brings up, and keeps the managed `Matrix<double>` route as the terminal a refused floor degrades onto with its refusal class on the determinism tag. Every dense and sparse solve emits the `Factorization` `ComputeReceipt` and rides the `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` solve stream, into which the `OnlineStat` fourth-order residual-moment accumulator folds; the provider-rank claim resolves at composition against the Persistence `ModelResultIndex.Claim` owner.

## [01]-[INDEX]

- [02]-[DENSE_ALGEBRA]: RID provider table; `FactorRoute` shape-spine; admission gate; held witness.
- [02.1]-[LEVENBERG_MARQUARDT]: damped Gauss-Newton nonlinear least-squares; HyperJet the canonical exact-Jacobian provider.
- [02.2]-[SPECTRAL_LAW]: dense-symmetric/general spectral split; Schur-pair decode; kernel `SpectralFilter` eigenvalue weights.
- [02.3]-[BASIS_ARTIFACT]: one HDF5 basis container over the sketch/modal/rbf row axis; rank-truncated read-back.
- [03]-[PROVIDER_CLAIMS]: claim-gated provider rank; provenance snapshot; online fourth-moment solve stream.

## [02]-[DENSE_ALGEBRA]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed MathNet-provider rows carrying the `Control.TryUse*` probe and `Control.Use*` activate delegates as inline row columns; `DenseSubstrate` `[SmartEnum<string>]` the execution-substrate axis choosing the managed `Matrix<double>` route or the native `torch.linalg` ATen leg, each carrying its `Available` probe and a substrate-determinism tag; `FactorRoute` `[Union]` shape-spine whose cases carry ONLY per-occurrence factorization policy (mode, orthogonalization law, symmetricity) while the operand `Matrix<double>` rides the entrypoint argument; `Admission` the one-pass finite/symmetry/singular gate with the modified Gram-Schmidt realization; `TolerancePolicy` the scale-derived threshold record seeded O(n²) from `‖A‖_F` and refined through `WithSigma` where an `Svd` handle is already held; `SketchPolicy` the seeded randomized range-finder policy; `Factorization` `[Union]` the held decomposition family, including the range basis required to solve through a randomized sketch; `AtenFloor`/`AtenDense` the native-substrate runtime probe and route-discriminated `torch.linalg` solve leg — `cholesky_ex`/`ldl_factor_ex`/`solve_ex` info-gated one-shots, the full-tuple `lstsq` whose reported rank always gates rank-deficiency and whose singular-value floor binds only where the driver yields the spectrum (the driverless surface runs CPU `gelsy`, whose singular-values tensor is empty), the disposable `HeldFactor` owner over the `lu_factor`/`lu_solve` pair, and the all-dense `torch.einsum` contraction; `DenseRoute`/`DenseOps` the shape-routed solve, held-handle refinement, and spectral folds over MathNet `Matrix<double>`; `SolveTerminal` `[Union]` partitioning the verdict so budget-exhaustion survives as a retryable case.
- Cases: `LinearProvider` rows managed · native-openblas · native-mkl (3); `DenseSubstrate` rows managed · native-aten (2); `FactorRoute` cases `DefinitePsd` · `SquarePivoting` · `Orthonormal` · `Spectral` · `RankRevealing` (5); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` · `Sketched` (6); `DenseOps.Decompose` `FactorizationKind.Switch` arms lu · qr · cholesky · svd · evd (5); `SolveTerminal` cases `Admitted` · `Exhausted` (2).
- Entry: `public static Fin<DenseSolve> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate)` — the route-spine entry gates the operand, realizes `Orthonormal.Modified` through the in-page modified Gram-Schmidt kernel, otherwise dispatches the substrate it was HANDED, degrades a declining native leg onto the managed terminal, and recomputes the true relative residual against the original operator once into the returned carrier; `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` drives the generated total `FactorizationKind.Switch` for the held-handle path; `public static Fin<(Factorization Sketch, double Truncation)> Sketch(Matrix<double> a, SketchPolicy policy)` builds a seed-replayable randomized range finder and retains both `Q` and the small `Svd` in `Factorization.Sketched`, so `Factorization.Solve(Matrix<double>)` applies `Qᵀrhs` before the reduced solve instead of misrepresenting the small factor as a factorization of `A`; `public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap)` streams N triangular solves through one held factorization; `public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate)` recovers the conditioning fallback from the route value, reading each attempt's already-witnessed residual.
- Auto: `LinearProvider.Select` and `DenseSubstrate.Select` run once at composition together — the former binds `LinearAlgebraControl.Provider` for the managed leg, the latter picks `NativeAten` where `AtenFloor.Resident` proves the native bring-up by RUNNING it (the one-shot probe sets the ATen OpenMP thread count, pins `set_default_dtype(Float64)`, and materializes one witness tensor), falling to `Managed` with its refusal class folded onto the determinism tag otherwise; the two axes are orthogonal — the ATen leg replaces the whole `Matrix<double>` solve, never the MathNet provider behind it. Selection THREADS from composition as an argument rather than resting in a process static, so a signature declares every input the solve reads. `DenseRoute.Solve` branches on that argument's `Native` column, route-discriminating the native `torch.linalg` factorization by the SAME `FactorRoute` case the managed leg switches on, never a `kind switch` cascade and never a per-call provider switch. `TolerancePolicy.Derive` seeds `SigmaMax` from `‖A‖_F` (`TensorPrimitives.Norm` over the flat column-major span, the O(n²) upper bound `σ_max ≤ ‖A‖_F`) and `‖b‖∞` from `TensorPrimitives.MaxMagnitude` — a fresh O(n³) `Svd` per tolerance derivation is the deleted hidden decomposition — refining through `WithSigma(Svd<double>)` exactly where a held handle already exists, so every threshold travels as one named record on the receipt and the dense residual path uses the one zero-alloc span primitive, never the allocating MathNet reduction; symmetry forces through `(A + A.Transpose()) * 0.5` before the definite kernel because `IsSymmetric()` compares by exact `!=`.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, the taken `FactorRoute` variant, the `TolerancePolicy` record, the recomputed true relative residual, the `DeterminismTag` substrate/provider/parallelism string (the SERVING `DenseSubstrate.DeterminismTag` ATen-vs-managed prefix folded onto the provider triple so a cross-substrate cache hit is a distinct fingerprint), row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, TorchSharp, libtorch-cpu, HyperJet (the LM canonical exact-Jacobian scalar-AD leg), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, the kernel `SpectralFilter` weight algebra), BCL inbox
- Growth: a new MathNet provider is one `LinearProvider` row with its RID predicate, rank, and inline `Control.TryUse*`/`Control.Use*` columns; a new execution substrate is one `DenseSubstrate` row with its `Available` probe and solve leg; a new operand shape is one `FactorRoute` case and one `DenseRoute.Solve`/`AtenDense.Solve` arm; a new decomposition is one `Factorization` case, one `FactorizationKind` row, and one `Decompose` `Switch` arm the generated total Switch breaks at compile time until it lands; a new sketch posture (Nyström, single-pass streaming) is one `SketchPolicy` row with its `Sketch` arm, never a sibling decomposition owner; a new eigenbasis weight is one kernel `SpectralFilter` case at `Rasm/Numerics/spectral#FILTER_ALGEBRA`, never a lane-local weight vocabulary; zero new surface.
- Boundary: the shape-spine union `FactorRoute` and the held-handle decomposition union `Factorization` are distinct C# symbols; an unused `computeVectors` knob on the solve route is deleted because every rank-revealing solve requires vectors internally, while `QRMethod` and modified-orthogonalization policy remain load-bearing case data. Identical operand `Matrix<double>` payloads never repeat on cases: the operand has ONE owner at the entrypoint. `Orthonormal` seats modified Gram-Schmidt as the `Modified` discriminant and collapses the built-in absolute/magnitude-squared/scale-relative rank thresholds into its one convention, never a sixth sibling factory. Every element carrier stays monomorphic `double` because the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative. `Admission` gates the flat column-major `Values` span through `TensorPrimitives.IsFiniteAll`/`IsNaNAny`/`IsInfinityAny` in one vectorized pass, never a strided per-element loop, and symmetry forces with `(A + A.Transpose()) * 0.5` before the call, never `MapIndexedInplace` self-averaging that mutates the backing array sequentially so a mirror entry is already modified when read. Singularity reads from `Cholesky<double>.DeterminantLn` because the determinant product underflows to zero with no signal, reflection tests `det < 0.0` never `det != 1.0`, and a `QR` construction checks the factor buffers all-finite because a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`. `TolerancePolicy` derives every threshold from operator and right-hand-side scale, so a bare per-module absolute literal in `1e-4..1e-8` is the unreplayable defect; conditioning rank is `Svd<double>.Rank` (`σ_max.EpsilonOf() · max(m,n)`) and never shares its slot with `Evd<double>.Rank`, and `ConditionNumber` is guarded against `+Inf` before gating because it is `+Inf` for rank-deficient operators. Iterative refinement forms its residual against the ORIGINAL operator in working precision through the in-place `Multiply(field, scratch)`/`Subtract` overloads streaming into one pre-sized `dx`/`scratch` pair, never against reconstructed factors which carry exactly the rounding error the correction cancels and never the allocating `held.Solve(rhs)` overload inside the loop; `Inverse()` in a hot loop is rejected because it clones the factors and an `n²` identity crossing the large-object threshold at `n ≥ 104`, so a solve against an identity rides the retained pivoting handle with reused buffers. `SolveTerminal` maps budget-exhaustion to the `Exhausted` case carrying the partial iterate so the caller's relaxed-criterion retry survives, never `Fin.Fail`. Rank-revealing solves run on BOTH substrates: a rank-deficient `lstsq` verdict is the native leg DECLINING the operand — `gelsy` gives its minimum-norm answer no meaning at deficient rank — so the managed `Svd` pseudo-inverse serves it as the route's terminal, and the receipt tag records the substrate that served rather than the one first asked. `AtenFloor` admits its substrate by EXECUTION, never by inventory: the vendored CPU payload resolves its OpenMP dependency through an absolute path outside the package, so the host process must carry the consolidated payload directory on the platform dynamic-library search path before its first `torch` touch — dyld fixes that path at process start and no library call adds to it later, which is why the floor probes instead of asserting, and why loading the aggregate and the CPU library together is the rejected shortcut (the aggregate already pulls the CPU library, so a second registration aborts the process on a duplicate-priority key rather than failing a rail). `DenseSubstrate` degrades a refused floor onto its managed row with the refusal class on the tag — one row behaviour, never a new surface and never a throw — and the selected row is a VALUE the composition threads, never a mutable process static: an ambient cell let two compositions in one process overwrite each other's choice, made a substrate unpinnable without mutating the world, and stamped receipts with whatever the cell held at read time instead of what served the solve. Managed, native-OpenBLAS, and native-ATen legs diverge at the bit level, so the receipt `DeterminismTag` folds both the serving `DenseSubstrate.DeterminismTag` substrate prefix and the provider type/parallelism triple, the `SolveDedupKey` folds that whole tag, and a dedup key omitting either dimension is the named correctness defect because a cross-substrate or cross-provider cache hit returns bit-divergent numbers. `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`/`DenseMatrix` wrapper is the deleted form mirroring the tensor-lane no-`TensorService` law.

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
        BinaryPrimitives.WriteUInt64LittleEndian(frame.AsSpan(tag.Length), ContentHash.Half(problemDigest, 0));
        BinaryPrimitives.WriteUInt64LittleEndian(frame.AsSpan(tag.Length + 8), ContentHash.Half(problemDigest, 1));
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

    // Native ATen leg is the osx-arm64 dense substrate the x64-only OpenBLAS/MKL providers cannot serve; the
    // MathNet Matrix<double> route stays the managed cold-start terminal. Selection runs once at composition
    // and the WINNER THREADS as a value from there: a mutable process static made every solve read ambient state no
    // signature declared, so two compositions in one process fought over one cell, a test could not pin a
    // substrate without mutating the world, and the determinism tag on a receipt named whatever the static held
    // at read time rather than what served the solve.
    public static DenseSubstrate Select() =>
        NativeAten.Available ? Bind(NativeAten) : Bind(Managed);

    static DenseSubstrate Bind(DenseSubstrate s) { s.activate(); return s; }

    public string DeterminismTag =>
        Native ? $"{Key}:aten:omp{torch.get_num_threads()}" : AtenFloor.Refusal.Match(Some: reason => $"{Key}:{reason}", None: static () => Key);
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
    // Residency is a LOAD probe, never a RID predicate or a file-presence check. The vendored CPU payload hard-links
    // an ABSOLUTE OpenMP library path that its own package does not place, so every RID predicate can pass over a
    // fully-present payload while the first tensor touch throws a type-initializer failure out of the two-step native
    // loader — a presence-only gate therefore publishes an accelerated route that cannot execute one operand, and its
    // receipt would carry a substrate tag for a substrate that never ran. The probe forces the native bring-up ONCE
    // behind `LazyThreadSafetyMode.ExecutionAndPublication`, and its refusal is the typed evidence the managed degrade
    // rides rather than an exception escaping whichever solve happened to touch `torch` first.
    static readonly Lazy<Fin<Unit>> Load = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool Resident => Load.Value.IsSucc;

    // Refusal CLASS, never the loader's own message: the determinism tag folds it, so an operand solved on the managed
    // substrate BECAUSE the floor refused keys distinctly from one solved on a host that ships no payload at all.
    public static Option<string> Refusal => Load.Value.Match(Succ: static _ => None, Fail: static _ => Some("aten-refused"));

    // Thread count and default dtype bind INSIDE the probe because they ARE the first native touch; a caller that
    // configured before probing would take the load failure on its own frame instead of on the rail.
    public static void Configure() => ignore(Load.Value);

    static Fin<Unit> Probe() =>
        Try.lift(() => {
            torch.set_num_threads(Environment.ProcessorCount);
            torch.set_default_dtype(ScalarType.Float64);
            using Tensor witness = torch.from_array(new[] { 0.0 }, ScalarType.Float64);
            return witness.NumberOfElements;
        }).Run()
        .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<aten-load:{error.Message}>"))
        .Bind(static elements => elements == 1L
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<aten-witness:{elements}>")));
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
    // `FactorRoute` case as the managed leg. `None` is this substrate DECLINING the operand — the managed
    // terminal then serves it — where a fault is the operand itself refusing on both.
    public static Fin<Option<Vector<double>>> Solve(FactorRoute route, Matrix<double> matrix, Vector<double> rhs, TolerancePolicy tol) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Matrix<double> operand = route is FactorRoute.DefinitePsd or FactorRoute.Spectral ? Admission.Symmetrize(matrix) : matrix;
        Tensor a = torch.from_array(operand.ToColumnMajorArray(), ScalarType.Float64).reshape(operand.ColumnCount, operand.RowCount).t();
        Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
        return route.Switch(
            definitePsd:    _ => Spd(a, b).Map(static x => Some(x)),
            squarePivoting: _ => General(a, b).Map(static x => Some(x)),
            orthonormal:    _ => LeastSquares(a, b, tol),
            spectral:       _ => SymmetricIndefinite(a, b).Map(static x => Some(x)),
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
    //
    // Rank-deficient verdicts are DECLINES, not refusals of the problem: `gelsy` gives the minimum-norm answer
    // no meaning at deficient rank, while the managed `Svd` route's pseudo-inverse solves exactly that shape. So
    // rank-revealing routing runs on BOTH substrates with the managed leg as its terminal, and the serving
    // substrate — never the one first asked — is what the receipt's determinism tag records.
    static Fin<Option<Vector<double>>> LeastSquares(Tensor a, Tensor b, TolerancePolicy tol) {
        (Tensor solution, Tensor residuals, Tensor rank, Tensor singular) = torch.linalg.lstsq(a, b);
        long observed = rank.NumberOfElements > 0 ? rank.ReadCpuInt64(0) : Math.Min(a.shape[0], a.shape[1]);
        Option<double> sigmaMin = singular.NumberOfElements > 0 ? Some(singular.ReadCpuDouble(singular.NumberOfElements - 1)) : None;
        bool sigmaAdmits = sigmaMin.Match(Some: s => double.IsFinite(s) && s >= tol.RankFloor, None: () => true);
        return observed == Math.Min(a.shape[0], a.shape[1]) && sigmaAdmits
            ? Egress(solution).Map(static x => Some(x))
            : Fin.Succ(Option<Vector<double>>.None);
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

// Solution, the substrate that actually SERVED it, and the witnessed residual travel as one value: the receipt
// needs all three and each is measured exactly once here, so a caller that re-derived the residual paid a second
// GEMV for a number the gate already had and a caller reading a substrate off ambient state named the wrong leg
// whenever the native leg declined.
public sealed record DenseSolve(Vector<double> X, DenseSubstrate Served, double Residual);

public static class DenseRoute {
    // Substrate legs return an unwitnessed solution; one original-operator gate gives either leg an identical
    // typed residual rejection. The native leg's `None` is a decline, so the managed terminal serves and the
    // carrier records it.
    public static Fin<DenseSolve> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate) =>
        Admission.Admit(operand).Bind(_ =>
            route is FactorRoute.Orthonormal { Modified: true }
                ? Admission.Modified(operand, rhs, tol.RankFloor).Bind(x => Witness(operand, x, rhs, tol, DenseSubstrate.Managed))
                : substrate.Native
                    ? AtenDense.Solve(route, operand, rhs, tol).Bind(served => served.Match(
                        Some: x => Witness(operand, x, rhs, tol, substrate),
                        None: () => Managed(route, operand, rhs, tol).Bind(x => Witness(operand, x, rhs, tol, DenseSubstrate.Managed))))
                    : Managed(route, operand, rhs, tol).Bind(x => Witness(operand, x, rhs, tol, DenseSubstrate.Managed)));

    static Fin<Vector<double>> Managed(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol) =>
        route.Switch<(Matrix<double> A, double Floor), Fin<ISolver<double>>>(
                state: (A: operand, Floor: tol.RankFloor),
                definitePsd:    static (s, _) => Admission.Definite(Admission.Symmetrize(s.A)).Map(static h => (ISolver<double>)h),
                squarePivoting: static (s, _) => Fin.Succ((ISolver<double>)s.A.LU()),
                orthonormal:    static (s, c) => Admission.Orthonormal(s.A, c.Mode, s.Floor).Map(static h => (ISolver<double>)h),
                spectral:       static (s, c) => Fin.Succ((ISolver<double>)Admission.Symmetrize(s.A).Evd(c.Sym)),
                rankRevealing:  static (s, _) => Fin.Succ((ISolver<double>)s.A.Svd(computeVectors: true)))
            .Map(solver => solver.Solve(rhs));

    // Both attempts read the residual the witness already measured — re-running the GEMV here paid a second
    // O(n²) pass to recompute a number the gate had just computed and admitted on.
    public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate) =>
        Solve(primary, operand, rhs, tol, substrate)
            .BindFail(_ => Solve(secondary, operand, rhs, tol, substrate))
            .Map(static solved => (SolveTerminal)new SolveTerminal.Admitted(solved.X, solved.Residual));

    static Fin<DenseSolve> Witness(Matrix<double> a, Vector<double> x, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate served) =>
        Relative(a, x, rhs) is var residual && tol.Admits(residual)
            ? Fin.Succ(new DenseSolve(x, served, residual))
            : Fin.Fail<DenseSolve>(new ComputeFault.ModelRejected($"<witness-fail:substrate={served.Key}:residual={residual:e3}:cap={tol.ResidualCap:e3}>"));

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

    public static IO<Fin<ShardOutcome>> Gemm(Matrix<double> left, Matrix<double> right, ShardDispatch dispatch) =>
        dispatch.Lower(left, right);

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

    // Sketch draws cross through the kernel Deterministic.Source adapter — the one sanctioned System.Random crossing.
    static Matrix<double> Gaussian(int rows, int columns, long seed) {
        double[] values = new double[rows * columns];
        new Normal(0.0, 1.0, Deterministic.Source(seed)).Samples(values);
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

    // Tag folds the SERVING substrate off the solve carrier, so a native leg that declined and degraded to the
    // managed terminal keys as the managed run it was rather than the accelerated one it was asked to be.
    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorRoute route, FactorizationKind kind, TolerancePolicy tol, DenseSolve solved, int rows, int cols, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, rows, cols, 0L, "dense") {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            RouteVariant = route.GetType().Name, DeterminismTag = $"{solved.Served.DeterminismTag}|{provider.DeterminismTag}", ResidualCap = tol.ResidualCap, TrueResidual = solved.Residual,
        };
}
```

### [02.1]-[LEVENBERG_MARQUARDT]

- Owner: `LevenbergMarquardt` the damped Gauss-Newton nonlinear least-squares owner minimizing `‖r(θ)‖²`, solving each step's normal-equation system `(JᵀJ + λ·diag(JᵀJ))·δ = −Jᵀr` through the lane's gated `Admission.Definite` SPD route (the damped normal matrix is symmetric-PD by construction), adapting the damping λ on the actual-versus-predicted reduction; `LmPolicy` the iteration policy record; `LmResult` the typed convergence carrier. This is the one Compute-INTERNAL nonlinear-least-squares owner, serving Compute's own solves and the host-free graduation/inference peers over the wire.
- Entry: ONE `Minimize` discriminating on the residual's input shape — the HYPERJET arm (`Func<DDScalar[], DDScalar[]>` residual authored once over the hyper-dual scalar) derives the EXACT Jacobian row-by-row through `GetGradient()` and is the CANONICAL provider (the `Stats/estimator` ARMA/Holt/state-space fits and the `Solver/uncertainty` FORM/SORM exact-AD row all arrive on this arm; the finite-difference fall those consumers carried is deleted), while the black-box arm (`Func<Vector<double>, Matrix<double>>` caller-supplied Jacobian) survives for residuals authored outside the hyper-dual reach; both converge on the identical damped fold.
- Boundary: the AEC-domain `Rasm.Materials` does NOT reference this owner — the strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so the Materials BRDF fit stays in-folder and the algorithms-doc thin-QR fit is a doctrine reference a Materials probe cites; a `Rasm.Compute` project reference or a "MathNet transitive via Rasm.Compute" claim from Materials is the forbidden AEC→app-platform edge. Linear least-squares stays on the one-shot `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR; LM is the nonlinear damped iteration. Hand-rolled finite-difference Jacobians beside the HyperJet arm are the deleted form — FD survives ONLY where the residual is a genuine black box no hyper-dual instantiation can reach (the honest caller-supplied-Jacobian arm). This kernel dropped its own MKL reference (x86-64-only, cannot load on osx-arm64) and flagged Compute's as sibling-roster debt: the `native-mkl` `LinearProvider` row above IS the resolution — the RID-claim `Available` filter is the design (osx-arm64 resolves managed/OpenBLAS, an x64 deployment claims MKL), recorded here so the kernel's flag closes.

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

- Owner: `SpectralResult` `[Union]` carrying distinct dense-symmetric and dense-general cases — the `Symmetricity` flag selects five output axes together (eigenvector norm, real versus block-diagonal `D`, single-column versus column-pair encoding, ascending versus Schur-deflation order, working versus norm-gated solve); `SpectralOps.Decompose` the one constructor projecting `Evd<double>` onto the matched `SpectralResult` case; `SpectralOps.Modal` the Schur-pair decoder; `SpectralOps.Defect` the block eigen-residual; the kernel `Rasm/Numerics/spectral#FILTER_ALGEBRA` `SpectralFilter` `[Union]` the ONE eigenvalue-weight vocabulary this lane composes through its public `Weight(double)` and partial-monoid `Compose`.
- Entry: `public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym)` builds the `Symmetric` case from the real eigenvalues and the orthonormal vectors for a symmetric/Hermitian spectrum and the `General` case decoding the Schur pairs for the nonsymmetric spectrum, each carrying its block defect; `public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values)` decodes real conjugate pairs from adjacent columns dispatched on `Math.Sign(values[j].Imaginary)`; `public static double Defect(Matrix<double> a, Matrix<double> vectors, Matrix<double> d)` computes `(A·V − V·D).FrobeniusNorm()`; `public static Fin<Vector<double>> Filtered(Evd<double> evd, double zeroFloor, double massFloor, params ReadOnlySpan<SpectralFilter> chain)` fuses the chain through `SpectralFilter.Compose` and applies the fused weight under two independent floors — an eigenvalue magnitude excluding the null space and a summed-weight magnitude gating emptiness — carrying the weight sum as evidence — one entrypoint over the singular, plural, and empty call, the empty spread yielding `SpectralFilter.Identity` because the partial monoid supplies the unit.
- Auto: `Decompose` is the single producer of `SpectralResult` — `SpectralOps.Modal`/`Defect`/`Filtered` are the per-axis kernels it composes and never independent return surfaces, so the result-union owner is always constructed and never unwired; `Modal` reads `Column(j)`+`Column(j+1)` for a positive-imaginary pair and `Column(j-1)`+`Column(j)` for a negative-imaginary pair, never `Column(j)` whole because that discards the imaginary half, and the backward read needs no bound guard because the real Schur reduction emits every conjugate pair positive-first, so index zero can never carry a negative imaginary part; `Defect` is the one signal both the managed throw rail and the native in-band info-code rail surface identically since no built-in eigen residual exists; `Filtered` fuses its chain before any spectrum walk so a non-composable pair rails at the fold instead of silently applying the last filter alone, then weights each `EigenValues` entry through the fused `SpectralFilter.Weight` excluding the zero mode (`|λ| < ε_zero ? 0.0 : Weight(λ)`, excluded never clamped) and fails a spectrum whose summed weight falls below its OWN floor rather than reading it as a zero signal.
- Boundary: `EigenValues` interprets `EigenVectors` because no parallel pairing array exists; nonsymmetric columns `Normalize(2)` before any modal weight because recovered columns are raw triangular solutions with arbitrary per-column norms; Hermitian eigenvectors stay complex because projecting them to real parts is incorrect; the library `Determinant`/`Rank`/`IsFullRank` are rejected in domain logic because `Determinant` short-circuits to `0.0` the moment any eigenvalue crosses the absolute zero test; eigenvalue equality is never asserted tighter than the convergence band because the exceptional-shift escape bakes the literal `0.964` into the last bits; only `DenseMatrix` reaches the native `EigenDecomp` and the managed `Evd` kernels are serial regardless of degree, so sign, ordering, and last bits differ across the seam and provider-mismatched eigenvector comparison short-circuits to span equivalence; `SpectralResult` is the only spectral return — a raw `Matrix<Complex>`/`double`/`Fin<Vector<double>>` leaking the spectral verdict past the owner is the deleted form because the consumer must dispatch on the `Symmetric`/`General` case to read the right `D` shape and ordering contract. Eigenvalue weights belong to the kernel vocabulary alone: a lane-local `EigenFilter` `[SmartEnum<string>]` carrying `passthrough`/`sqrt`/`inverse`/`inv-sqrt`/`exp`/`heat` weight lambdas is the DELETED form — it shadowed `SpectralFilter` (`passthrough` ≡ `IdentityCase`, `heat` ≡ `HeatCase` at unit time, `sqrt`/`inverse`/`inv-sqrt` ≡ `Power(½)`/`Power(−1)`/`Power(−½)`, `exp` ≡ `AmplifyCase`), and no alias or forwarding row survives it; this lane composes the kernel owner and NEVER enumerates its cases, so a new weight form lands as one kernel case with zero edit here.
- Boundary — persistence: a computed basis leaving this lane persists through `[02.3]-[BASIS_ARTIFACT]`, never a per-carrier serializer beside each union case.

```csharp signature
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

    // INVARIANT the `j - 1` read stands on: MathNet's real Schur reduction writes each conjugate pair as
    // `e[k] = +z, e[k + 1] = -z` with `z = sqrt(|discriminant|)` non-negative, so the POSITIVE imaginary part
    // always occupies the lower index and a negative-imaginary eigenvalue always has a partner at `j - 1`.
    // `j == 0` therefore cannot be negative-imaginary and the backward read cannot underflow.
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

    // Kernel `SpectralFilter.Weight` is the one transfer function; the chain fuses through the partial-monoid
    // `Compose` BEFORE the spectrum walk, so a non-composable pair rails instead of applying the last filter alone.
    // TWO floors, because the quantities are unrelated: `zeroFloor` is an EIGENVALUE magnitude below which the
    // mode is the operator's null space and excludes, while `massFloor` is a summed-WEIGHT magnitude below
    // which the filtered basis carries no signal. One value serving both tied a null-space cutoff to a
    // transfer-function total, so tightening the zero-mode exclusion silently tightened the emptiness verdict
    // on a different scale.
    public static Fin<Vector<double>> Filtered(Evd<double> evd, double zeroFloor, double massFloor, params ReadOnlySpan<SpectralFilter> chain) =>
        Fused(chain).Bind(filter =>
            evd.EigenValues.Map(static v => v.Real).ToArray() is var spectrum
            && spectrum.Select(lambda => Math.Abs(lambda) < zeroFloor ? 0.0 : filter.Weight(lambda)).ToArray() is var weights
            && TensorPrimitives.Sum<double>(weights) is var mass && Math.Abs(mass) >= massFloor
                ? Fin.Succ(Vector<double>.Build.DenseOfArray(weights))
                : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<spectrum-fully-excluded:mass={mass:e3}:floor={massFloor:e3}>")));

    // `Identity` is the monoid unit, so the empty spread is total and the singular call fuses to itself.
    static Fin<SpectralFilter> Fused(ReadOnlySpan<SpectralFilter> chain) =>
        LanguageExt.Iterable<SpectralFilter>.FromSpan(chain).Fold(
            Fin.Succ(SpectralFilter.Identity),
            static (fused, next) => fused.Bind(held => held.Compose(next).ToFin(
                (Error)new ComputeFault.ModelRejected($"<filter-not-composable:{held.GetType().Name}+{next.GetType().Name}>"))));
}
```

### [02.3]-[BASIS_ARTIFACT]

- Owner: `BasisKind` `[SmartEnum<string>]` the artifact row axis — sketch · modal · rbf — each row carrying its support-block roster as row data; `BasisArtifact` the one container carrier absorbing the three unpersisted basis carriers (`Factorization.Sketched`, `SpectralResult.Symmetric`, `RbfFit`) behind ONE parameterized writer and one rank-truncating reader over `Runtime/codecs#HDF_ARCHIVE`.
- Cases: `BasisKind` rows sketch (range basis + singular values + `truncation` gauge) · modal (mode shapes + eigenvalues + `defect` gauge) · rbf (weight columns + `centres`/`trend` support blocks + `radius`/`order` gauges + the radial `family` key) (3).
- Entry: `BasisArtifact.Of` is the polymorphic projection — one overload per absorbed carrier, so no carrier grows a serializer of its own; `Write(StreamPool, CorrelationId, HdfArchivePolicy, BasisArtifact)` is the ONE writer — the primary `/basis` matrix chunks on its COLUMN axis (`[rows, 1]`), support blocks ride whole, gauges and the family key ride typed attributes; `Read(HdfHandle, BasisKind, Option<int> rank)` reads back rank-truncated — the first `rank` columns and their paired values as one hyperslab, exactly `rank` chunk reads by the column-axis layout.
- Output: every artifact lands content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit`; the capsule is job machinery, never a store.
- Packages: MathNet.Numerics (`Matrix<double>.ToArray`, `Build.DenseOfRowMajor`, `Svd<double>.S`), PureHDF (`H5File`, `H5Group`, `H5Dataset<T>`, `HyperslabSelection`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`), Microsoft.IO.RecyclableMemoryStream, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new basis family is one `BasisKind` row with one `Of` overload projecting its carrier — the writer and reader bodies stay untouched; a new gauge is one map entry.
- Boundary: `Values` pairs the basis columns and is EMPTY where the kind measures none (rbf) — a zero-filled vector publishes singular values nobody computed; the rank truncation serves the reader that restarts a solve warm from a stored range or modal basis, so it applies to the primary matrix and its paired values alone and support blocks always read whole; the `SolveDedupKey`/determinism-tag law holds — a basis read back re-enters the solve rails as data and never claims the provenance of the run that wrote it.

```csharp signature
// --- [BASIS_ARTIFACT] ----------------------------------------------------------------------
// One container shape for every dense basis this lane computes: a basis IS columns of one primary matrix
// beside a per-column value vector, so the kind is a row and the writer is one parameterized body, never
// three artifact classes. Column-axis chunks make rank truncation a hyperslab over the first r columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BasisKind {
    public static readonly BasisKind Sketch = new("sketch", support: []);
    public static readonly BasisKind Modal = new("modal", support: []);
    public static readonly BasisKind Rbf = new("rbf", support: ["centres", "trend"]);

    // Support roster is ROW DATA so the reader resolves blocks by name without enumerating group children.
    public string[] Support { get; }
}

// `Values` pairs basis columns (singular values, eigenvalues) and is EMPTY where the kind measures none;
// `Gauges` carries the kind's scalar evidence; `Family` the radial row key only the rbf kind states.
public sealed record BasisArtifact(
    BasisKind Kind, Matrix<double> Basis, double[] Values, Map<string, Matrix<double>> Support, Map<string, double> Gauges, Option<string> Family) {

    public static BasisArtifact Of(Factorization.Sketched sketch) =>
        new(BasisKind.Sketch, sketch.Range, [.. sketch.Core.S], default, Map(("truncation", sketch.Truncation)), None);

    public static BasisArtifact Of(SpectralResult.Symmetric modal) =>
        new(BasisKind.Modal, modal.Vectors, [.. modal.Values], default, Map(("defect", modal.Defect)), None);

    public static BasisArtifact Of(RbfFit fit) =>
        new(BasisKind.Rbf, fit.Weights, [],
            Map(("centres", fit.Centres), ("trend", fit.PolynomialCoefficients)),
            Map(("radius", fit.Radius), ("order", (double)fit.PolynomialOrder)), Some(fit.Kernel.Key));

    // ONE writer: `/basis` chunks `[rows, 1]` so read-back truncates by rank; the create-only Begin session
    // closes in one call and the pooled stream returns positioned at zero, caller-owned.
    public static Fin<RecyclableMemoryStream> Write(StreamPool pool, CorrelationId correlation, HdfArchivePolicy policy, BasisArtifact artifact) =>
        pool.Get(correlation, new StreamGrant.Open())
            .Bind(staged => Try.lift(() => {
                    try {
                        H5DatasetCreation creation = policy.Creation();
                        H5Group group = new() {
                            ["basis"] = new H5Dataset<double[,]>(artifact.Basis.ToArray(), chunks: [(uint)artifact.Basis.RowCount, 1u], datasetCreation: creation),
                        };
                        if (artifact.Values.Length > 0) {
                            group["values"] = new H5Dataset<double[]>(artifact.Values, chunks: [(uint)artifact.Values.Length], datasetCreation: creation);
                        }

                        artifact.Support.Iter((name, block) =>
                            group[name] = new H5Dataset<double[,]>(block.ToArray(), chunks: [(uint)block.RowCount, 1u], datasetCreation: creation));
                        group.Attributes["kind"] = artifact.Kind.Key;
                        artifact.Gauges.Iter((name, value) => group.Attributes[name] = value);
                        artifact.Family.IfSome(family => group.Attributes["family"] = family);
                        HdfArchive.Begin(new H5File { [artifact.Kind.Key] = group }, staged, policy).Dispose();
                        staged.Position = 0;
                        return staged;
                    }
                    catch { staged.Dispose(); throw; }
                }).Run()
                .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<hdf5-basis-write:{error.Message}>")));

    // Rank-truncated read-back: the first `rank` columns of `/basis` and their paired values — exactly `rank`
    // chunk reads under the column-axis layout; support blocks read whole, gauges off the attribute roster.
    public static Fin<BasisArtifact> Read(HdfHandle archive, BasisKind kind, Option<int> rank) =>
        Try.lift(() => {
                NativeGroup group = archive.Group(kind.Key);
                NativeDataset basisSet = archive.Dataset($"{kind.Key}/basis");
                int rows = checked((int)basisSet.Space.Dimensions[0]);
                int cols = checked((int)basisSet.Space.Dimensions[1]);
                int keep = rank.Match(Some: r => Math.Min(r, cols), None: () => cols);
                double[] slab = new double[rows * keep];
                basisSet.Read<double>(archive.Access, slab.AsSpan(), new HyperslabSelection(2, [0UL, 0UL], [(ulong)rows, (ulong)keep]));
                double[] values = [];
                if (archive.Exists($"{kind.Key}/values")) {
                    values = new double[keep];
                    archive.Dataset($"{kind.Key}/values").Read<double>(archive.Access, values.AsSpan(), new HyperslabSelection(0, (ulong)keep));
                }

                Map<string, Matrix<double>> support = toSeq(kind.Support).Fold(
                    Map<string, Matrix<double>>(),
                    (held, name) => {
                        NativeDataset block = archive.Dataset($"{kind.Key}/{name}");
                        int r = checked((int)block.Space.Dimensions[0]);
                        int c = checked((int)block.Space.Dimensions[1]);
                        double[] data = new double[r * c];
                        block.Read<double>(archive.Access, data.AsSpan(), new HyperslabSelection(2, [0UL, 0UL], [(ulong)r, (ulong)c]));
                        return held.Add(name, Matrix<double>.Build.DenseOfRowMajor(r, c, data));
                    });
                Map<string, double> gauges = toSeq(group.Attributes())
                    .Filter(attribute => attribute.Name is not ("kind" or "family"))
                    .Fold(Map<string, double>(), (held, attribute) => held.Add(attribute.Name, attribute.Read<double>()));
                Option<string> family = group.AttributeExists("family") ? Some(group.Attribute("family").Read<string>()) : None;
                return new BasisArtifact(kind, Matrix<double>.Build.DenseOfRowMajor(rows, keep, slab), values, support, gauges, family);
            }).Run()
            .MapFail(static error => (Error)new ComputeFault.PayloadOverBounds($"<hdf5-basis-read:{error.Message}>"));
}
```

## [03]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — and the `OnlineStat` fourth-order residual-moment accumulator the numeric lane owns because `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `ModelResultIndex.Claim(rows, fingerprint)` resolved at composition against the running fingerprint under the index-owned `RecencyHorizon` and clock — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot()` captures the `LinearAlgebraControl.Provider` `ToString` tag, the provider type name, and the public `Control.MaxDegreeOfParallelism` degree at solve construction because every kernel reads this ambient `LinearAlgebraControl.Provider` static at execution instant (`Control` exposes no `LinearAlgebraProvider` member — the active handle is `LinearAlgebraControl.Provider`; the `ParallelizeOrder`/`ParallelizeElements` thresholds are `internal` to `Control` and unreadable, so the determinism triple is the public provider/type/degree); `OnlineStat.Push(residual)` folds each witnessed solve residual into the running fourth-order moment stream under the `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `ModelResultIndex.Claim` owner and threaded in, never re-resolved here; bitwise-versus-bounded equality derives from the provider/type/degree triple because the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, and accumulates the online residual fourth-order moments through the `OnlineStat` `MomentNormalizer`-policy merge whose `Combine` is a CAS-safe pure reduction asserting `combined.Count == a.Count + b.Count`; the stream guards at admission through the same all-finite predicate the operands cross because one pushed `NaN` permanently poisons every moment with no reset.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, route variant, tolerance, true residual, determinism tag, symbolic fill, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations` and `rasm.compute.solve.residual` instruments are owned by `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here; the `OnlineStat` accumulator is the numeric-lane moment owner whose skewness/kurtosis evidence feeds the residual histogram tail.
- Packages: System.Numerics.Tensors, Rasm.Persistence (project), LanguageExt.Core, BCL inbox
- Growth: a new claim dimension is one column on the existing `BenchmarkClaim`; a new solve instrument is one row on `ReceiptSurface.Instruments`; a new moment is one field on `OnlineStat` with one merge term; zero new surface.
- Boundary — native BLAS is unreachable on every estate RID at the pinned adapter version, and the rows stay because the gate is what proves it. osx-arm64 answers `false` to every `Control.TryUseNative*` and resolves managed, since neither adapter ships an osx-arm64 payload. linux-x64 has a payload that cannot pass: `MathNet.Numerics.MKL.Linux-x64` tops out at native revision 9 while `MathNet.Numerics.Providers.MKL` holds a `MinimumCompatibleRevision` of 15 and throws `NotSupportedException("MKL Native Provider too old")` on load, and no OpenBLAS payload exists for a non-Windows RID at all — the stock system `libopenblas` exports no `query_capability`, which is the first symbol the adapter's own `IsAvailable` reads, so it is not a substitutable shim. win-x64 is the sole arm still open to a compatible payload. Those rows are therefore honest fail-closed capability: each `Available` probe answers `false`, `Select` falls to `Managed`, and a benchmark claim asserting native rank fails its gate rather than degrading silently — deleting them forfeits the win-x64 arm and re-opens a claim nothing checks.
- Boundary: provider rank is the `BenchmarkClaim` `Provider` column gated exactly like the SIMD and partition claims — a static native default beside the claim is the named defect; the claim is resolved by the Persistence `ModelResultIndex.Claim` owner whose recency horizon and clock are closed inside the index and threaded in, never re-resolved and never a second horizon; the solve instruments live on the `ReceiptSurface.Instruments` stream and a second numeric-lane-local instrument owner is the deleted form; the online residual accumulator accumulates to fourth order (mean, M2, M3, M4) and serializes for distributed aggregation because parallel online moments accumulate to fourth order, records the running-versus-moving distinction and the `MomentNormalizer` Bessel-versus-population policy enum because unmarked mixing silently corrupts every downstream confidence computation, and one pushed `NaN` permanently poisons every moment so the stream guards at admission through the same all-finite predicate the operands cross — and COUNTS what it turned away, because silently returning the prior state made a producer emitting nothing but sentinels indistinguishable from a lane nobody pushed, and that count merges on every arm including the empty one; the merge identity holds only to the floating-point merge bound.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MomentNormalizer {
    public static readonly MomentNormalizer Sample = new("sample", static (m2, count) => count > 1 ? m2 / (count - 1) : 0.0);
    public static readonly MomentNormalizer Population = new("population", static (m2, count) => count > 0 ? m2 / count : 0.0);

    private readonly Func<double, long, double> variance;

    public double Variance(double m2, long count) => variance(m2, count);
}

// Bit-faithfulness IS the record's own value equality — the hand-written three-field compare re-implemented the
// compiler `==` and is deleted; `[Equatable]` keeps the compare generated and adds the `Inequalities` diff, so a
// stale cache verdict names WHICH coordinate moved (`ProviderType: MklLinearAlgebraProvider -> Managed…`,
// `Parallelism: 8 -> 4`) instead of a bare false.
[Equatable]
public readonly partial record struct SolveProvenance(string ProviderTag, string ProviderType, int Parallelism) {
    public static SolveProvenance Snapshot() =>
        new(LinearAlgebraControl.Provider.ToString() ?? string.Empty, LinearAlgebraControl.Provider.GetType().Name, Control.MaxDegreeOfParallelism);
}

// `Rejected` counts what the finite guard turned away. Silently returning the prior state made a stream of pure
// `NaN` indistinguishable from a stream nobody pushed, so a reader could not tell a healthy quiet lane from a
// producer emitting nothing but sentinels — the count is the diagnostic that separates them.
public sealed record OnlineStat(long Count, double Mean, double M2, double M3, double M4, long Rejected) {
    public static readonly OnlineStat Empty = new(0L, 0.0, 0.0, 0.0, 0.0, 0L);

    public OnlineStat Push(double value) {
        if (!double.IsFinite(value)) {
            return this with { Rejected = Rejected + 1L };
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
        return new OnlineStat(n, mean, m2, m3, m4, Rejected);
    }

    // Rejections merge on every arm INCLUDING the empty one: two lanes that admitted nothing and rejected
    // thousands must not combine to a receipt reading zero rejections.
    public static OnlineStat Combine(OnlineStat a, OnlineStat b) =>
        (a.Count + b.Count, b.Mean - a.Mean) switch {
            (0L, _) => Empty with { Rejected = a.Rejected + b.Rejected },
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
        return new OnlineStat(n, mean, m2, m3, m4, a.Rejected + b.Rejected);
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

(none)
