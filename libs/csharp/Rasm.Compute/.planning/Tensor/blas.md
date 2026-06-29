# [COMPUTE_BLAS]

Rasm.Compute dense linear-algebra lane: BLAS-class dense linear algebra over the admitted MathNet provider stack and the native `TorchSharp` ATen substrate, admitted once and routed by operand shape — definite, square, overdetermined, symmetric, periodic-grid — never by the call site and never by a knob riding beside the matrix. The lane owns the `LinearProvider` RID-keyed availability table selecting native MKL then OpenBLAS where a win-x64/linux-x64 asset resolves and the managed terminal otherwise, the `DenseSubstrate` execution axis routing the osx-arm64 dense solve to `torch.linalg.*` (native Apple-Accelerate BLAS/LAPACK compiled into `libtorch_cpu`) with the managed `Matrix<double>` route as the proved cold-start terminal, the `FactorRoute` `[Union]` route-spine collapsing every dense factorization to one shape-routed admission, the `Admission` finite/symmetry/singular gate re-imposing every gate MathNet refuses, the scale-derived `TolerancePolicy` carried on every receipt, the spectral `Modal`/eigen-residual/eigenbasis-filter owners returning the `SpectralResult` `[Union]`, and the claim-gated provider-rank selection with its provenance snapshot and the `OnlineStat` fourth-order residual-moment accumulator the receipt-surface residual histogram folds. Every library refuses its own gates — no constructor checks finiteness, `IsSymmetric()` compares by exact `!=`, a zero-norm `QR` fills `NaN` while `IsFullRank` returns `true` — so admission re-imposes each refused gate and every result leaves as a typed `ComputeReceipt.Factorization` carrying the route variant, the scale-derived tolerance, the provider determinism tag, and the recomputed true relative residual against the original operator, never a `Matrix<double>`, `Vector<double>`, or factorization instance.

## [01]-[INDEX]

- [01]-[DENSE_ALGEBRA]: RID provider table; `FactorRoute` shape-spine; admission gate; held witness; Levenberg-Marquardt nonlinear least-squares route.
- [02]-[PROVIDER_CLAIMS]: claim-gated provider rank; provenance snapshot; online fourth-moment solve stream.

## [02]-[DENSE_ALGEBRA]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `LinearProvider` `[SmartEnum<string>]` RID-keyed MathNet-provider rows carrying the `Control.TryUse*` probe and `Control.Use*` activate delegates as inline row columns; `DenseSubstrate` `[SmartEnum<string>]` the execution-substrate axis choosing the managed `Matrix<double>` route or the native `torch.linalg` ATen leg, each carrying its `Available` probe and a substrate-determinism tag; `FactorRoute` `[Union]` shape-spine carrying mode/symmetricity/vector-demand/rank-tolerance as case data so the operand structure selects the factorization, never a `bool computeVectors`/`QRMethod`/`Symmetricity` parameter riding beside the matrix; `Admission` the one-pass finite/symmetry/singular gate; `TolerancePolicy` the scale-derived threshold record; `Factorization` `[Union]` one-case-per-decomposition collapsing to one held solve admission; `AtenFloor`/`AtenDense` the native-substrate runtime probe and the route-discriminated `torch.linalg` solve leg under one `DisposeScope`+`torch.inference_mode` no-grad scope; `DenseRoute`/`DenseOps` the shape-routed solve, held-handle refinement, spectral, and refinement folds over MathNet `Matrix<double>`; `SolveTerminal` `[Union]` partitioning the verdict so budget-exhaustion survives as a retryable case.
- Cases: `LinearProvider` rows managed · native-openblas · native-mkl (3); `DenseSubstrate` rows managed · native-aten (2); `FactorRoute` cases `DefinitePsd` · `SquarePivoting` · `Orthonormal` · `Spectral` · `RankRevealing` (5); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` (5); `DenseOps.Decompose` `FactorizationKind.Switch` arms lu · qr · cholesky · svd · evd (5); `EigenFilter` rows passthrough · sqrt · inverse · invsqrt · exp · heat (6); `SolveTerminal` cases `Admitted` · `Exhausted` (2).
- Entry: `public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> rhs, TolerancePolicy tol)` — the route-spine entry: `Admission` gates the operand all-finite over the flat column-major span, the matched case builds its `ISolver<double>`, and the post-solve `Witness` recomputes the true relative residual against the ORIGINAL operator (never reconstructed factors) through the zero-alloc `TensorPrimitives` residual, aborting on a cap breach with the route variant in the fault; `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` drives the generated total `FactorizationKind.Switch` (state-threaded on the reference-type `matrix`) for the held-handle path; `public static Fin<(IterationStatus Verdict, Vector<double> Field, int Refinements, double Residual)> Refine(Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap)` streams N triangular solves through one held factorization into the in-place `held.Solve(scratch, dx)` overload, folding the working-precision residual against the original operator under the tolerance cap with no per-iteration allocation; `public static Fin<SolveTerminal> Conditioned(FactorRoute primary, FactorRoute secondary, Vector<double> rhs, TolerancePolicy tol)` recovers the conditioning fallback from the route value, rebinding both routes onto the one witness gate.
- Auto: `LinearProvider.Select` and `DenseSubstrate.Select` run once at composition together — the former binds `LinearAlgebraControl.Provider` for the managed leg, the latter picks `NativeAten` where `AtenFloor.Resident` (the RID ships a `libtorch-cpu` CPU payload — osx-arm64/linux-x64/win-x64) and `Configure` sets the ATen OpenMP thread count and `set_default_dtype(Float64)`, falling to `Managed` otherwise; `DenseRoute.Solve` branches on `DenseSubstrate.Active.Native` — the ATen leg ingests the `Matrix<double>` column-major buffer through `torch.from_array(...).reshape(cols, rows).t()` then route-discriminates the native factorization EXACTLY as the managed leg does — `DefinitePsd`→`cholesky_ex`+`cholesky_solve`, `Spectral`→`ldl_factor_ex`+`ldl_solve`, `SquarePivoting`→`solve_ex`, `Orthonormal`/`RankRevealing`→`lstsq` — inside one `torch.NewDisposeScope()`+`torch.inference_mode(true)` (the PUBLIC no-grad scope; `TorchSharp.InferenceMode` is an internal type a consumer cannot `new`), reading each `_ex` `info` tensor (`info.ReadCpuInt32(0) != 0` → typed fault, never a caught native exception), and egresses `data<double>()` to a `Vector<double>` so no `Tensor` escapes the lane — while the managed leg reads the `FactorRoute` case — `DefinitePsd→Cholesky()`, `SquarePivoting→LU()`, `Orthonormal→QR(Mode)` with modified Gram-Schmidt seated as the `Modified` discriminant, `Spectral→Evd(Sym)`, `RankRevealing→Svd(true)` — never a `kind switch` cascade and never a per-call provider switch; `TolerancePolicy.Derive` reads `Svd<double>.L2Norm` (σ_max) and computes `‖A‖_F` from `TensorPrimitives.Norm` over the flat column-major span and `‖b‖∞` from `TensorPrimitives.MaxMagnitude` so every threshold travels as one named record on the receipt and the dense residual path uses the one zero-alloc span primitive, never the allocating MathNet reduction; symmetry forces through `(A + A.Transpose()) * 0.5` before the definite kernel because `IsSymmetric()` compares by exact `!=`.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, the taken `FactorRoute` variant, the `TolerancePolicy` record, the recomputed true relative residual, the `DeterminismTag` substrate/provider/parallelism string (the `DenseSubstrate.Active.DeterminismTag` ATen-vs-managed prefix folded onto the provider triple so a cross-substrate cache hit is a distinct fingerprint), row and column extents, zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, TorchSharp, libtorch-cpu, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new MathNet provider is one `LinearProvider` row with its RID predicate, rank, and inline `Control.TryUse*`/`Control.Use*` columns; a new execution substrate is one `DenseSubstrate` row with its `Available` probe and solve leg; a new operand shape is one `FactorRoute` case plus one `DenseRoute.Solve`/`AtenDense.Solve` arm; a new decomposition is one `Factorization` case plus one `FactorizationKind` row plus one `Decompose` `Switch` arm the generated total Switch breaks at compile time until it lands; a new eigenbasis weight is one `EigenFilter` row; zero new surface.
- Boundary: the shape-spine union is `FactorRoute` and the held-handle decomposition union is `Factorization` — distinct C# symbols; a route discriminant riding as a `bool computeVectors`/`QRMethod` parameter beside the matrix is the named defect collapsed into case data; the `Orthonormal` case seats modified Gram-Schmidt as the `Modified` discriminant and collapses the five built-in absolute/magnitude-squared/scale-relative rank thresholds into its one convention, never a sixth sibling factory; the element carrier is monomorphic `double` because the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative; `Admission` gates the flat column-major `Values` span through `TensorPrimitives.IsFiniteAll`/`IsNaNAny`/`IsInfinityAny` in one vectorized pass and a strided per-element loop is the named defect; symmetry forces with `(A + A.Transpose()) * 0.5` before the call and `MapIndexedInplace` self-averaging is the rejected form because it mutates the backing array sequentially so a mirror entry is already modified when read; singularity reads from `Cholesky<double>.DeterminantLn` (the streaming log-determinant `2·Σ log L[i,i]`) because the determinant product underflows to zero with no signal, and the MathNet `Matrix<double>.Cholesky()` construction itself THROWS `ArgumentException` on a non-square or non-positive-definite operand, so `Admission.Definite` wraps it in `Try.lift` (the dense parallel of the sparse lane's `Try.lift` factorization discipline) before the `DeterminantLn` finiteness gate — a bare `spd.Cholesky()` in an expression returning `Fin` is the illusory no-throw form this very boundary law forbids; reflection tests `det < 0.0`, never `det != 1.0`; a `QR` construction checks the factor buffers all-finite because a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`; `TolerancePolicy` derives every threshold from operator and right-hand-side scale and a bare per-module absolute literal in `1e-4..1e-8` is the unreplayable defect; the conditioning rank is `Svd<double>.Rank` (`σ_max.EpsilonOf() · max(m,n)`) and never shares its slot with `Evd<double>.Rank` (`AlmostEqual` at `DefaultDoubleAccuracy`); `ConditionNumber` is guarded against `+Inf` before gating because it is `+Inf` for rank-deficient operators; the iterative-refinement residual forms against the ORIGINAL operator in working precision through the in-place `Multiply(field, scratch)`/`Subtract` overloads streaming into one pre-sized `dx`/`scratch` pair, never against reconstructed factors which carry exactly the rounding error the correction cancels and never the allocating `held.Solve(rhs)` overload inside the loop; `Inverse()` in a hot loop is rejected because it clones the factors plus an `n²` identity crossing the large-object threshold at `n ≥ 104` — solve against an identity through the retained pivoting handle with reused buffers; `SolveTerminal` maps budget-exhaustion to the `Exhausted` case carrying the partial iterate so the caller's relaxed-criterion retry survives, never `Fin.Fail`; the provider-determinism contract holds that managed and native-OpenBLAS diverge at the bit level, so `DeterminismTag` names the active `ILinearAlgebraProvider` type and the degree-of-parallelism, the instance `SolveDedupKey` folds that into the content-addressed solve-dedup fingerprint, and a solve-dedup key that omits the provider tag is the named correctness defect because a cross-provider cache hit returns bit-divergent numbers; the `DenseSubstrate` axis is the second determinism dimension — the native `torch.linalg` ATen leg (Apple-Accelerate BLAS/LAPACK in `libtorch_cpu`, OpenMP-parallel) and the managed `Matrix<double>` route diverge in their last bits, so the receipt `DeterminismTag` carries the `DenseSubstrate.Active.DeterminismTag` ATen/managed+OMP-thread prefix and an `AtenDense` solve omitting it from the dedup key is the same cross-provider correctness defect; the ATen leg never relies on GC finalization for native-tensor reclamation — every `Tensor` born in the `torch.NewDisposeScope()` is freed on scope exit and only the egressed `double[]` crosses the lane boundary (a `Tensor` escaping onto the Compute wire is the deleted form), the forward-only solve runs inside the public `torch.inference_mode(true)` no-grad scope (never the internal `TorchSharp.InferenceMode` type, which a consumer assembly cannot construct) to skip autograd bookkeeping, and a numerical failure surfaces through the `cholesky_ex`/`ldl_factor_ex`/`solve_ex` `info` tensor mapped to a typed `ComputeFault` rather than a caught native exception so the no-exception-control-flow law holds across the seam; the native ATen substrate is the osx-arm64 dense path the x64-only OpenBLAS/MKL managed providers cannot serve, with the managed `Matrix<double>` route the proved cold-start terminal where no `libtorch-cpu` RID payload resides (`linux-arm64`/`win-arm64`/`osx-x64`); the `TorchSharp.torch.*` calls are the managed surface only — the native LibTorch RID/ABI/OpenMP floor is the `libtorch-cpu` owner's, never restated here; the `native-mkl` row carries the `Control.TryUseNativeMKL()` probe and `Control.UseNativeMKL()` activate and ranks above `native-openblas`, but its probe returns `false` on osx-arm64 (no native MKL asset ships there) so `LinearProvider.Select`'s `Available` filter drops it from the live osx-arm64 axis while a win-x64/linux-x64 host lights it as the fastest provider — the RID-keyed availability table thus spans EVERY target RID rather than the osx-arm64 slice alone, with the `native-openblas` row (whose `Control.TryUseNativeOpenBLAS()` probe returns `true` only where an OpenBLAS asset resolves) the x64 fallback below it and the `managed` `Matrix<double>` route the proved cold-start terminal where no native asset resolves; `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`/`DenseMatrix` wrapper is the deleted form mirroring the tensor-lane no-`TensorService` law; the `LevenbergMarquardt` damped Gauss-Newton route is the one Compute-internal nonlinear-least-squares owner, each LM step solving the damped normal-equation system through the lane's gated `Admission.Definite` SPD route (a rank-deficient Jacobian's failed factor raises λ and retries — never a raw `damped.Cholesky()` whose `ArgumentException` would escape the fold) so a nonlinear fit shares the dense-algebra admission discipline, while a linear least-squares stays on the one-shot `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR — a one-shot QR misused for a nonlinear residual or an LM loop for a linear system is the deleted form. The strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so the AEC-domain `Rasm.Materials` owns its appearance fit and grounding IN-FOLDER and never references this Compute owner: the acquisition `FitBrdf` keeps its in-folder roughness estimate, the spectral grounding its in-folder `SpectralUpsample`, and the `algorithms#ROUTE_SPINE` thin-QR fit is a doctrine reference a Materials probe cites, never a `Rasm.Compute` project reference and never a "MathNet transitive via Rasm.Compute" edge (Materials cannot reference Compute to obtain MathNet transitively — that is the forbidden AEC→app-platform downward edge). This `LevenbergMarquardt` owner, the `Symbolic/units#QUANTITY_TABLE` `Illuminance` row, and the `Model/inference#INFERENCE_MODES` ONNX spectral-reconstruction run are Compute-internal capabilities serving Compute's own solves and the host-free graduation/inference peers — a Compute page asserting the Materials fit reads its solver over a reference is the named seam-direction defect.

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
        toSeq(Items)
            .Filter(static row => row.Available)
            .OrderByDescending(row => claim.Map(c => StringComparer.Ordinal.Equals(c.Route, row.Key) ? int.MaxValue : row.Rank).IfNone(row.Rank))
            .HeadOrNone()
            .Map(static row => { row.activate(); return row; })
            .IfNone(static () => { Managed.activate(); return Managed; });

    public string DeterminismTag =>
        $"{Key}:{LinearAlgebraControl.Provider.GetType().Name}:{Control.MaxDegreeOfParallelism}";

    public UInt128 SolveDedupKey(UInt128 problemDigest) =>
        XxHash128.HashToUInt128(MemoryMarshal.AsBytes(DeterminismTag.AsSpan()), unchecked((long)problemDigest));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenseSubstrate {
    public static readonly DenseSubstrate Managed = new("managed", native: false, probe: static () => true, activate: static () => { });
    public static readonly DenseSubstrate NativeAten = new("native-aten", native: true, probe: static () => AtenFloor.Resident, activate: static AtenFloor.Configure);

    private readonly Func<bool> probe;
    private readonly Action activate;

    public bool Native { get; }
    public bool Available => probe();

    public static DenseSubstrate Active { get; private set; } = Managed;

    // The native ATen leg is the osx-arm64 dense substrate the OpenBLAS/MKL managed providers cannot serve
    // (no osx-arm64 OpenBLAS/MKL native asset); the MathNet Matrix<double> route stays the managed cold-start
    // terminal. Selection runs once at composition beside LinearProvider.Select so both bind together.
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

    // MathNet `Matrix<double>.Cholesky()` THROWS ArgumentException on a non-square or non-positive-definite
    // operand, so the SPD gate wraps the construction in `Try.lift` exactly as the sparse lane wraps its
    // throwing direct factorizations — a bare `spd.Cholesky()` in an expression returning `Fin` is the
    // illusory no-throw form the page's own boundary law forbids. DeterminantLn finiteness is the second
    // gate, catching a numerically-degenerate factor whose construction did not throw.
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

// ATen dense substrate: the osx-arm64 native dense-LA leg `DenseRoute.Solve` dispatches to when
// `DenseSubstrate.Active` is `NativeAten`. The leg route-discriminates the native factorization EXACTLY as
// the managed leg discriminates the MathNet one — SPD drives `cholesky_ex`+`cholesky_solve`,
// symmetric-indefinite drives `ldl_factor_ex`+`ldl_solve`, general-square drives `solve_ex`, overdetermined
// drives `lstsq` — never collapsing every square operand onto one general `solve_ex` that discards the
// SPD/symmetric structure the operand carries. Each `_ex` `info` tensor maps a numerical failure to a typed
// fault (`info.ReadCpuInt32(0) != 0`) rather than catching a native exception — the same no-throw
// control-flow law the managed leg holds. Every `Tensor` lives inside one `DisposeScope` reclaimed on exit;
// `torch.inference_mode(true)` (the PUBLIC no-grad scope — `TorchSharp.InferenceMode` is an internal type a
// consumer assembly cannot construct) skips autograd bookkeeping for the forward-only solve; only the
// egressed `double[]` crosses the boundary, so no `Tensor` escapes onto the Compute wire. The un-witnessed
// solution returns to `DenseRoute.Solve`, where the ONE shared `Witness` gate against the ORIGINAL MathNet
// operator admits both substrate legs identically.
public static class AtenFloor {
    public static bool Resident =>
        RuntimeInformation.RuntimeIdentifier is "osx-arm64" or "linux-x64" or "win-x64";

    public static void Configure() {
        torch.set_num_threads(Environment.ProcessorCount);
        torch.set_default_dtype(ScalarType.Float64);
    }
}

public static class AtenDense {
    // Symmetricity forcing for the definite/spectral cases happens before ingress on the MathNet operator so
    // ATen factorizes the symmetrized matrix, identical to the managed leg; the native factorization family
    // is then selected by the SAME `FactorRoute` case the managed leg switches on, never a square-vs-tall
    // binary that throws away the SPD/symmetric structure.
    public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> rhs, TolerancePolicy tol) {
        using var scope = torch.NewDisposeScope();
        using var noGrad = torch.inference_mode(true);
        Matrix<double> operand = route is FactorRoute.DefinitePsd or FactorRoute.Spectral ? Admission.Symmetrize(route.A) : route.A;
        Tensor a = torch.from_array(operand.ToColumnMajorArray(), ScalarType.Float64).reshape(operand.ColumnCount, operand.RowCount).t();
        Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
        return route.Switch(
            definitePsd:    _ => Spd(a, b),
            squarePivoting: _ => General(a, b),
            orthonormal:    _ => LeastSquares(a, b),
            spectral:       _ => SymmetricIndefinite(a, b),
            rankRevealing:  _ => LeastSquares(a, b));
    }

    // SPD: Cholesky factor + triangular `cholesky_solve`, info-gated — the structure the general `solve_ex`
    // discards; the `_ex` info tensor is the typed-fault rail, never a caught native throw.
    static Fin<Vector<double>> Spd(Tensor a, Tensor b) {
        var (l, info) = torch.linalg.cholesky_ex(a, check_errors: false);
        return info.ReadCpuInt32(0) == 0
            ? Egress(torch.cholesky_solve(b, l, upper: false))
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-cholesky-nonspd:info={info.ReadCpuInt32(0)}>"));
    }

    // Symmetric-indefinite: Bunch-Kaufman LDL factor + `ldl_solve`, the spectral route's native realization;
    // the nullable `pivots`/`info` ldl_factor_ex returns are both gated before the solve (a null `info` under
    // `check_errors: false` reads as status 0, the Witness gate downstream catching any residual breach).
    static Fin<Vector<double>> SymmetricIndefinite(Tensor a, Tensor b) {
        var (ld, pivots, info) = torch.linalg.ldl_factor_ex(a, hermitian: true, check_errors: false);
        int status = info is { } reported ? reported.ReadCpuInt32(0) : 0;
        return pivots is { } p && status == 0
            ? Egress(torch.linalg.ldl_solve(ld, p, b, hermitian: true))
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-ldl-singular:info={status}:pivots={pivots is not null}>"));
    }

    // General square: pivoted-LU `solve_ex`, info-gated.
    static Fin<Vector<double>> General(Tensor a, Tensor b) {
        var (result, info) = torch.linalg.solve_ex(a, b, left: true, check_errors: false);
        return info.ReadCpuInt32(0) == 0
            ? Egress(result)
            : Fin.Fail<Vector<double>>(new ComputeFault.ModelRejected($"<aten-solve-singular:info={info.ReadCpuInt32(0)}>"));
    }

    static Fin<Vector<double>> LeastSquares(Tensor a, Tensor b) {
        var (solution, _, _, _) = torch.linalg.lstsq(a, b);
        return Egress(solution);
    }

    static Fin<Vector<double>> Egress(Tensor x) =>
        Fin.Succ(Vector<double>.Build.DenseOfArray(x.reshape(x.NumberOfElements).data<double>().ToArray()));
}

public static class DenseRoute {
    // The substrate leg (native ATen or managed MathNet) returns the un-witnessed solution; the ONE shared
    // `Witness` gate against the ORIGINAL operator runs here so a substrate divergence past the residual cap
    // is the same typed rejection on either leg, never duplicated per leg nor skipped on one.
    public static Fin<Vector<double>> Solve(FactorRoute route, Vector<double> rhs, TolerancePolicy tol) =>
        Admission.Admit(route.A).Bind(_ =>
            (DenseSubstrate.Active.Native ? AtenDense.Solve(route, rhs, tol) : Managed(route, rhs, tol))
                .Bind(x => Witness(route.A, x, rhs, tol)));

    static Fin<Vector<double>> Managed(FactorRoute route, Vector<double> rhs, TolerancePolicy tol) =>
        route.Switch<(Vector<double> Rhs, double Floor), Fin<ISolver<double>>>(
                state: (Rhs: rhs, Floor: tol.RankFloor),
                definitePsd:    static (_, c) => Admission.Definite(Admission.Symmetrize(c.A)).Map(static h => (ISolver<double>)h),
                squarePivoting: static (_, c) => Fin.Succ((ISolver<double>)c.A.LU()),
                orthonormal:    static (s, c) => Admission.Orthonormal(c.A, c.Mode, s.Floor).Map(static h => (ISolver<double>)h),
                spectral:       static (_, c) => Fin.Succ((ISolver<double>)Admission.Symmetrize(c.A).Evd(c.Sym)),
                rankRevealing:  static (_, c) => Fin.Succ((ISolver<double>)c.A.Svd(computeVectors: true)))
            .Map(solver => solver.Solve(rhs));

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
    // The held-handle decomposition dispatches through the generated total `FactorizationKind.Switch`, never a
    // `FrozenDictionary` keyed by the smart-enum whose missing row surfaces only as a runtime kind-miss: a new
    // `FactorizationKind` row breaks this Switch signature at compile time, forcing its build arm. `matrix` is a
    // reference type, so it threads as the Switch state without the ref-struct restriction the span lanes hit.
    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        kind.Switch(
            state: matrix,
            lu: static m => Fin.Succ<Factorization>(new Factorization.Lu(m.LU())),
            qr: static m => Fin.Succ<Factorization>(new Factorization.Qr(m.QR())),
            cholesky: static m => Admission.Definite(Admission.Symmetrize(m)).Map(static h => (Factorization)new Factorization.Cholesky(h)),
            svd: static m => Fin.Succ<Factorization>(new Factorization.Svd(m.Svd(computeVectors: true))),
            evd: static m => Fin.Succ<Factorization>(new Factorization.Evd(m.Evd())));

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
                // Short-circuit once the residual admits, identical to the `LevenbergMarquardt` `Done` latch: the
                // initial MaxValue never admits so the first sweep always runs, and the converged branch never
                // mutates Field/Refinements, so this skips the redundant O(n^2) `Multiply` for the residual cap's
                // remaining sweeps with an output identical to running them.
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
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
            RouteVariant = route.GetType().Name, DeterminismTag = $"{DenseSubstrate.Active.DeterminismTag}|{provider.DeterminismTag}", ResidualCap = tol.ResidualCap, TrueResidual = residual,
        };
}

// Levenberg-Marquardt damped Gauss-Newton nonlinear least-squares: minimizes ‖r(θ)‖² over a parameter
// vector θ given a residual function and its Jacobian, solving each step's normal-equation system
// (JᵀJ + λ·diag(JᵀJ))·δ = −Jᵀr through the lane's gated `Admission.Definite` SPD route (the damped normal
// matrix is symmetric-PD by construction), adapting the damping λ on the actual-versus-predicted reduction. This is the one Compute-INTERNAL nonlinear-least-
// squares owner, serving Compute's own solves and the host-free graduation/inference peers over the wire.
// The AEC-domain Rasm.Materials does NOT reference this owner: the strata graph is acyclic (app-platform
// consumes AEC-domain, never the reverse), so the Materials BRDF fit stays in-folder and the algorithms-doc
// thin-QR fit is a doctrine reference a Materials probe cites — a Rasm.Compute project reference or a
// "MathNet transitive via Rasm.Compute" claim from Materials is the forbidden AEC->app-platform edge. A
// linear least-squares stays on the one-shot DenseRoute.Solve(FactorRoute.Orthonormal) thin-QR; LM is the
// nonlinear damped iteration.
public sealed record LmPolicy(int MaxIterations, double GradientTolerance, double StepTolerance, double InitialDamping, double DampingUp, double DampingDown) {
    public static readonly LmPolicy Canonical = new(MaxIterations: 200, GradientTolerance: 1e-10, StepTolerance: 1e-12, InitialDamping: 1e-3, DampingUp: 10.0, DampingDown: 0.1);
}

public sealed record LmResult(Vector<double> Parameters, double Residual, int Iterations, bool Converged);

public static class LevenbergMarquardt {
    public static Fin<LmResult> Minimize(Func<Vector<double>, Vector<double>> residual, Func<Vector<double>, Matrix<double>> jacobian, Vector<double> initial, LmPolicy policy) {
        var theta = initial;
        double lambda = policy.InitialDamping;
        var folded = toSeq(Enumerable.Range(0, policy.MaxIterations)).Fold(
            (Theta: theta, Lambda: lambda, Cost: Cost(residual(theta)), Iterations: 0, Done: false),
            (state, _) => {
                if (state.Done) { return state; }
                Vector<double> r = residual(state.Theta);
                Matrix<double> j = jacobian(state.Theta);
                Matrix<double> jtj = j.TransposeThisAndMultiply(j);
                Vector<double> jtr = j.TransposeThisAndMultiply(r);
                Matrix<double> damped = jtj + Matrix<double>.Build.DiagonalOfDiagonalVector(jtj.Diagonal()) * state.Lambda;
                // The damped normal-equation matrix routes through the lane's gated SPD admission, never a raw
                // `damped.Cholesky()` that THROWS on a zero-column (rank-deficient) Jacobian — a failed factor
                // is the textbook LM response (raise λ and retry), not an escaped exception.
                return Admission.Definite(damped).Match(
                    Succ: chol => {
                        Vector<double> step = chol.Solve(-jtr);
                        Vector<double> candidate = state.Theta + step;
                        double trial = Cost(residual(candidate));
                        return trial < state.Cost
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

### [2.1]-[SPECTRAL_LAW]

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
        // The Schur-pair modal matrix is built ONCE and shared by the result and its defect; constructing it
        // twice (once for the carrier, once for the residual) doubles the complex-column reconstruction.
        Matrix<Complex> modal = Modal(evd.EigenVectors, evd.EigenValues);
        return new SpectralResult.General(modal, evd.EigenValues, ComplexDefect(a, modal, evd.EigenValues));
    }

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

## [03]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — plus the `OnlineStat` fourth-order residual-moment accumulator the numeric lane owns because `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `BenchmarkRow.Claim` resolved at composition against the running fingerprint and the `ModelResultIndex.RecencyHorizon` — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot()` captures the `LinearAlgebraControl.Provider` `ToString` tag, the provider type name, and the public `Control.MaxDegreeOfParallelism` degree at solve construction because every kernel reads this ambient `LinearAlgebraControl.Provider` static at execution instant (`Control` exposes no `LinearAlgebraProvider` member — the active handle is `LinearAlgebraControl.Provider`; the `ParallelizeOrder`/`ParallelizeElements` thresholds are `internal` to `Control` and unreadable, so the determinism triple is the public provider/type/degree); `OnlineStat.Push(residual)` folds each witnessed solve residual into the running fourth-order moment stream under the `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
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

- [NATIVE_EXECUTION]: the osx-arm64 native dense path is the `DenseSubstrate.NativeAten` leg — `torch.linalg.*` over `libtorch_cpu`'s Apple-Accelerate BLAS/LAPACK — because no osx-arm64 OpenBLAS/MKL managed-provider asset resolves there, so the MathNet `LinearProvider.NativeOpenBlas`/MKL rows light up only on a host RID carrying their native asset (`win-x64`/`linux-x64`) while the `Managed` `Matrix<double>` route is the proved cold start where neither a `libtorch-cpu` RID payload nor a MathNet native provider resolves (`linux-arm64`/`win-arm64`/`osx-x64`). `LinearProvider.NativeOpenBlas` execution (`Control.TryUseNativeOpenBLAS()` returning `true` and `LinearAlgebraControl.Provider` binding the native `ILinearAlgebraProvider`) and the CSparse native sparse path stay the x64 design record; the `native-mkl` row (`Control.UseNativeMKL`/`Control.TryUseNativeMKL`) ranks highest and lights only behind a win-x64/linux-x64 RID where the native MKL asset resolves, dormant on osx-arm64 where `TryUseNativeMKL()` returns `false` and `Select`'s `Available` filter excludes it. `DenseSubstrate.NativeAten` and the MathNet provider axis are orthogonal — the ATen leg replaces the whole `Matrix<double>` solve on osx-arm64, not the MathNet provider behind it.
