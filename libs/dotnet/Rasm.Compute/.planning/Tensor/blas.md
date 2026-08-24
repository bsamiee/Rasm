# [COMPUTE_BLAS]

Rasm.Compute dense linear-algebra lane: BLAS-class dense linear algebra over the admitted MathNet provider stack and the native `TorchSharp` ATen substrate, admitted once and routed by operand shape — definite, square, overdetermined, symmetric, periodic-grid — never by the call site and never by a knob riding beside the matrix. Every library refuses its own gates, so `OperandGate` re-imposes each refused gate and every result leaves as ONE `SolveOutcome<T>` carrying the iterate, the substrate that served it, the witnessed residual, the steps it spent, and a total `SolveTermination`, projected at the composition edge into a typed `ComputeReceipt.Factorization` — never a `Matrix<double>`/`Vector<double>` or factorization instance.

`LinearProvider` ranks native MKL then OpenBLAS then the managed terminal by RID claim; `DenseSubstrate` routes the dense solve to `torch.linalg` over the vendored CPU ATen payload wherever a one-shot LOAD probe proves that payload brings up, and keeps the managed `Matrix<double>` route as the terminal a refused floor degrades onto with its refusal class on the determinism tag. Every dense and sparse solve emits the `Factorization` `ComputeReceipt` and rides the `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` solve stream, into which the `ResidualStream` fourth-order residual-moment accumulator folds over the kernel `Stat<Scalar>` receipt; the provider-rank claim resolves at composition against the Persistence `ModelResultIndex.Claim` owner.

## [01]-[INDEX]

- [02]-[DENSE_ALGEBRA]: RID provider table; `FactorRoute` shape-spine; operand gate; the one `SolveOutcome` carrier and its held witness.
- [03]-[LEVENBERG_MARQUARDT]: damped Gauss-Newton nonlinear least-squares; HyperJet the canonical exact-Jacobian provider.
- [04]-[SPECTRAL_LAW]: dense-symmetric/general spectral split; Schur-pair decode; kernel `SpectralFilter` eigenvalue weights.
- [05]-[BASIS_ARTIFACT]: one HDF5 basis container over the sketch/modal/rbf row axis; rank-truncated read-back.
- [06]-[PROVIDER_CLAIMS]: claim-gated provider rank; provenance snapshot; online fourth-moment solve stream.

## [02]-[DENSE_ALGEBRA]

- Owner: `LinearProvider` the RID-keyed MathNet-provider rows carrying their probe and activate delegates as GENERATED columns; `DenseSubstrate` the execution-substrate axis choosing the managed `Matrix<double>` route or the native `torch.linalg` ATen leg; `FactorRoute` the shape-spine whose cases carry ONLY per-occurrence factorization policy while the operand rides the entrypoint argument; `OperandGate` the one-pass finite/symmetry/singular gate with the modified Gram-Schmidt realization; `TolerancePolicy` the scale-derived threshold record reading its residual cap off the `Tensor/vocabulary#OPERATION_TABLE` envelope; `SigmaEvidence` the three-state singular-value witness; `SketchPolicy` the seeded randomized range-finder policy; `Factorization` the held decomposition family; `AtenFloor`/`AtenDense` the native-substrate runtime probe and route-discriminated `torch.linalg` solve leg; `DenseRoute`/`DenseOps` the shape-routed solve, held-handle refinement, and receipt projection; and `SolveOutcome<T>`/`SolveTermination` the ONE outcome carrier every solve, refinement, fit, and sketch on this page returns.
- Cases: `LinearProvider` rows managed · native-openblas · native-mkl (3); `DenseSubstrate` rows managed · native-aten (2); `FactorRoute` cases `DefinitePsd` · `SquarePivoting` · `Orthonormal` · `ModifiedOrthonormal` · `Spectral` · `RankRevealing` (6); `Factorization` cases `Lu` · `Qr` · `Cholesky` · `Svd` · `Evd` · `Sketched` (6) against `FactorizationKind` rows lu · qr · cholesky · svd · evd · sketched (6 — the roster is its union's own length); `SolveTermination` cases `Converged` · `Exhausted` · `Truncated` (3); `SigmaEvidence` cases `Measured` · `Unavailable` (2); `SolveDirection` rows forward · adjoint (2).
- Entry: `public static Fin<SolveOutcome<Vector<double>>> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate)` — the route-spine entry gates the operand, dispatches the substrate it was HANDED through one total `Switch`, degrades a declining native leg onto the managed terminal, and recomputes the true relative residual against the original operator once into the returned carrier; `public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind)` drives the generated total `FactorizationKind.Switch` for the held-handle path; `public static Fin<SolveOutcome<Factorization>> Sketch(Matrix<double> a, SketchPolicy policy)` builds a seed-replayable randomized range finder and retains both `Q` and the small `Svd` in `Factorization.Sketched`, so `Factorization.Solve(Matrix<double>)` applies `Qᵀrhs` before the reduced solve instead of misrepresenting the small factor as a factorization of `A`; `public static Fin<SolveOutcome<Vector<double>>> Refine(Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap)` streams N triangular solves through one held factorization; `public static Fin<SolveOutcome<Vector<double>>> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate)` recovers the conditioning fallback from the route value, accumulating BOTH attempts' refusals rather than discarding the primary's.
- Auto: `LinearProvider.Select` and `DenseSubstrate.Select` run once at composition together — the former binds `LinearAlgebraControl.Provider` for the managed leg, the latter picks `NativeAten` where `AtenFloor.Resident` proves the native bring-up by RUNNING it (the one-shot probe sets the ATen OpenMP thread count, pins `set_default_dtype(Float64)`, and materializes one witness tensor), falling to `Managed` with its refusal class folded onto the determinism tag otherwise; the two axes are orthogonal — the ATen leg replaces the whole `Matrix<double>` solve, never the MathNet provider behind it. Selection THREADS from composition as an argument rather than resting in a process static, so a signature declares every input the solve reads — and `SolveProvenance.Snapshot(provider)` takes that same threaded row rather than re-reading the ambient cell the substrate comment argues against. `DenseRoute.Solve` branches on the substrate argument's `Native` column, route-discriminating the native `torch.linalg` factorization by the SAME `FactorRoute` case the managed leg switches on, never a `kind switch` cascade and never a per-call provider switch. `TolerancePolicy.Derive` seeds `SigmaMax` from `‖A‖_F` (`TensorPrimitives.Norm` over the flat column-major span, the O(n²) upper bound `σ_max ≤ ‖A‖_F`) and `‖b‖∞` from `TensorPrimitives.MaxMagnitude` — a fresh O(n³) `Svd` per tolerance derivation is the deleted hidden decomposition — refining through `WithSigma(Svd<double>)` exactly where a held handle already exists, so every threshold travels as one named record on the receipt and the dense residual path uses the one zero-alloc span primitive, never the allocating MathNet reduction; symmetry forces through `(A + A.Transpose()) * 0.5` before the definite kernel because `IsSymmetric()` compares by exact `!=`.
- Receipt: every dense solve materializes the `Factorization` `ComputeReceipt` case carrying provider key, decomposition kind, the taken `FactorRoute` variant read off the case's own `Key` column, the `TolerancePolicy` record, the recomputed true relative residual, the `DeterminismTag` substrate/provider/parallelism string (the SERVING `DenseSubstrate.DeterminismTag` ATen-vs-managed prefix folded onto the provider triple so a cross-substrate cache hit is a distinct fingerprint), row and column extents, a structurally-zero nnz, and `dense` format; emission rides the sink port at the composition edge.
- Packages: MathNet.Numerics, MathNet.Numerics.Providers.MKL, MathNet.Numerics.Providers.OpenBLAS, TorchSharp, libtorch-cpu, HyperJet (the LM canonical exact-Jacobian scalar-AD leg), System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, the kernel `SpectralFilter` weight algebra), BCL inbox
- Growth: a new MathNet provider is one `LinearProvider` row with its RID predicate, rank, and generated probe/activate columns; a new execution substrate is one `DenseSubstrate` row with its `Available` probe and solve leg; a new operand shape is one `FactorRoute` case and one `DenseRoute`/`AtenDense` arm the total `Switch` breaks on; a new decomposition is one `Factorization` case, one `FactorizationKind` row, and one `Decompose` arm; a new termination is one `SolveTermination` case breaking every consumer's fold; a new sketch posture is one `SketchPolicy` row with its `Sketch` arm; a new eigenbasis weight is one kernel `SpectralFilter` case at `Rasm/Numerics/spectral#FILTER_ALGEBRA`; zero new surface.
- Boundary — carriers: ONE outcome shape serves every solve on this page. `SolveOutcome<T>` carries the iterate, the substrate that SERVED it, the witnessed residual, the steps spent, and a total `SolveTermination`; the five carriers it absorbs each contributed exactly one fact and lost none. The three-field `DenseSolve` contributed the serving substrate, which is now a column; the two-case `SolveTerminal` contributed budget exhaustion carrying the partial iterate, which is now the `Exhausted(Budget)` case a consumer must switch on before it reads the iterate; the refinement 4-tuple contributed the refinement count, now `Steps`, and leaked the foreign MathNet `IterationStatus` enum, whose only two reachable values on this page are the two termination cases (a divergent residual is a `Fin.Fail`, never a status); the LM carrier contributed the iteration count and spelled convergence as a `bool` where the kernel spells `SolveStatus`, so the collapse restores the discriminant the kernel already ruled and retires a public name that COLLIDED with the kernel's own `LmResult` across a live project reference; the sketch tuple contributed the a-posteriori truncation gauge, now the `Truncated(Gauge, Cap)` case rather than an unnamed second tuple slot. The one shape a caller loses is a type that made "final" and "partial" distinct at construction — `SolveTermination` is total, so the iterate cannot be read without the case that produced it.
- Boundary — routing: an unused `computeVectors` knob on the solve route is deleted because every rank-revealing solve requires vectors internally, while `QRMethod` remains load-bearing case data. `ModifiedOrthonormal` is its OWN `FactorRoute` case rather than a boolean payload the entrypoint peeks at before the `Switch` that dispatches it: a property-pattern fork ahead of a total dispatch is a route the compiler stops checking. `FactorRoute.Key` is a generated `Switch` projection, so the receipt's route variant is a case discriminant rather than `GetType().Name` reflection. Identical operand `Matrix<double>` payloads never repeat on cases: the operand has ONE owner at the entrypoint. Every element carrier stays monomorphic `double` because the `struct, IEquatable<T>, IFormattable` family excludes `INumber<T>`, so a generic-math route signature is decorative. The held-LU solve carries its direction as a `SolveDirection` row, never a defaulted `bool transpose` — the sparse lane's landed answer to this exact defect is `GemvForm` (`Rasm.Compute/RULINGS.md [02]`), which this lane composes NOT because a triangular solve is a GEMV: `GemvForm` carries α/β accumulation a solve never reads, so the dense direction is its own two-row axis and the shared law is the refusal of the flag, not the type. `HeldFactor` is the one public held-solve surface and the raw `(lu, pivots)` pair stays inside it.
- Boundary — admission: `OperandGate` (the operand gate, named for what it gates rather than the one-word `Admission` three packages spell) reads the flat column-major `Values` span through `TensorPrimitives.IsFiniteAll`/`IsNaNAny`/`IsInfinityAny` in one vectorized pass, never a strided per-element loop, and symmetry forces with `(A + A.Transpose()) * 0.5` before the call, never `MapIndexedInplace` self-averaging that mutates the backing array sequentially so a mirror entry is already modified when read. MathNet's `GramSchmidt<T>` IS the modified variant — `DenseGramSchmidt.Factorize` orthogonalizes by in-place update of the remaining columns against each normalized q_i — so the modified route COMPOSES the package factorization and gates rank on its R diagonal; the former in-page MGS loop was a hand-roll built on the false classical-only premise. Singularity reads from `Cholesky<double>.DeterminantLn` because the determinant product underflows to zero with no signal, and a `QR` construction checks the factor buffers all-finite because a near-zero column norm divides through and fills `Q`/`R` with `NaN` while `IsFullRank` still returns `true`. `TolerancePolicy` derives every threshold from operator and right-hand-side scale, so a bare per-module absolute literal in `1e-4..1e-8` is the unreplayable defect, and the residual cap READS the `Tensor/vocabulary#OPERATION_TABLE` `ToleranceClass.CrossPlatformVariant` envelope rather than re-spelling its `16ε` body — the envelope owner states the de-sync a stored copy causes and this lane was the copy. Conditioning rank is `Svd<double>.Rank` (`σ_max.EpsilonOf() · max(m,n)`) and never shares its slot with `Evd<double>.Rank`, and `ConditionNumber` is guarded against `+Inf` before gating because it is `+Inf` for rank-deficient operators. Iterative refinement forms its residual against the ORIGINAL operator in working precision through the in-place `Multiply(field, scratch)`/`Subtract` overloads streaming into one pre-sized `dx`/`scratch` pair, never against reconstructed factors which carry exactly the rounding error the correction cancels and never the allocating `held.Solve(rhs)` overload inside the loop; `Inverse()` in a hot loop is rejected because it clones the factors and an `n²` identity crossing the large-object threshold at `n ≥ 104`, so a solve against an identity rides the retained pivoting handle with reused buffers. Rank-revealing solves run on BOTH substrates: a rank-deficient `lstsq` verdict is the native leg DECLINING the operand — `gelsy` gives its minimum-norm answer no meaning at deficient rank — so the managed `Svd` pseudo-inverse serves it as the route's terminal, and the receipt tag records the substrate that served rather than the one first asked. The singular-value floor rides `SigmaEvidence`, never a boolean whose `None` arm passed: the driverless CPU `gelsy` surface reports rank and returns an EMPTY singular-value tensor, so an unmeasured spectrum is the `Unavailable` case the receipt records rather than a pass no measurement supports.
- Boundary — substrate: `AtenFloor` admits its substrate by EXECUTION, never by inventory: the vendored CPU payload resolves its OpenMP dependency through an absolute path outside the package, so the host process must carry the consolidated payload directory on the platform dynamic-library search path before its first `torch` touch — dyld fixes that path at process start and no library call adds to it later, which is why the floor probes instead of asserting, and why loading the aggregate and the CPU library together is the rejected shortcut (the aggregate already pulls the CPU library, so a second registration aborts the process on a duplicate-priority key rather than failing a rail). `DenseSubstrate` degrades a refused floor onto its managed row with the refusal class on the tag — one row behaviour, never a new surface and never a throw — and the selected row is a VALUE the composition threads, never a mutable process static: an ambient cell let two compositions in one process overwrite each other's choice, made a substrate unpinnable without mutating the world, and stamped receipts with whatever the cell held at read time instead of what served the solve. Managed, native-OpenBLAS, and native-ATen legs diverge at the bit level, so the receipt `DeterminismTag` folds both the serving `DenseSubstrate.DeterminismTag` substrate prefix and the provider type/parallelism triple, the `SolveDedupKey` folds that whole tag, and a dedup key omitting either dimension is the named correctness defect because a cross-substrate or cross-provider cache hit returns bit-divergent numbers. The `_ex` info tensor is the typed-fault rail on every native leg, never a caught native throw, and the three info-gated arms share ONE gate body. `DenseOps` composes MathNet `Matrix<double>`/`Vector<double>` directly — a package-local `RasmMatrix`/`DenseMatrix` wrapper is the deleted form mirroring the tensor-lane no-`TensorService` law.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinearProvider {
    public static readonly LinearProvider Managed = new("managed", rank: 0, Always, Control.UseManaged);
    public static readonly LinearProvider NativeOpenBlas = new("native-openblas", rank: 1, Control.TryUseNativeOpenBLAS, Control.UseNativeOpenBLAS);
    public static readonly LinearProvider NativeMkl = new("native-mkl", rank: 2, Control.TryUseNativeMKL, Control.UseNativeMKL);

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial bool Available();

    [UseDelegateFromConstructor]
    public partial void Activate();

    private static bool Always() => true;

    // `Managed` probes true on every host, so the ordered roster is never empty; the prior fall-through re-spelled
    // that row's activation as a second path, which is one row's behaviour landed twice.
    public static LinearProvider Select(Option<BenchmarkRow> claim) =>
        toSeq(toSeq(Items)
            .Filter(static row => row.Available())
            .OrderByDescending(row => claim.Map(c => StringComparer.Ordinal.Equals(c.Route, row.Key) ? int.MaxValue : row.Rank).IfNone(row.Rank)))
            .Head
            .Match(Some: Activated, None: static () => Activated(Managed));

    private static LinearProvider Activated(LinearProvider row) {
        row.Activate();
        return row;
    }

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

// Native ATen leg is the osx-arm64 dense substrate the x64-only OpenBLAS/MKL providers cannot serve; the MathNet
// `Matrix<double>` route stays the managed cold-start terminal. Selection runs once at composition and the WINNER
// THREADS as a value from there: a mutable process static made every solve read ambient state no signature
// declared, so two compositions in one process fought over one cell, a test could not pin a substrate without
// mutating the world, and the determinism tag on a receipt named whatever the static held at read time rather
// than what served the solve.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenseSubstrate {
    public static readonly DenseSubstrate Managed = new("managed", native: false, Always, Inert);
    public static readonly DenseSubstrate NativeAten = new("native-aten", native: true, AtenFloor.Resident, AtenFloor.Configure);

    public bool Native { get; }

    [UseDelegateFromConstructor]
    public partial bool Available();

    [UseDelegateFromConstructor]
    public partial void Activate();

    private static bool Always() => true;

    private static void Inert() { }

    public static DenseSubstrate Select() => Bind(NativeAten.Available() ? NativeAten : Managed);

    private static DenseSubstrate Bind(DenseSubstrate row) {
        row.Activate();
        return row;
    }

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
    // The sixth row the union always had: a rank-64 randomized sketch reported as a full SVD stamped every
    // receipt with a decomposition nobody ran, and a receipt reader could not tell one from the other.
    public static readonly FactorizationKind Sketched = new("sketched");
}

// A held triangular solve runs forward or adjoint; the direction is a ROW, never a defaulted `bool`. The sparse
// lane's `GemvForm` is the landed answer to the same defect on a different concern and is NOT composed here: it
// carries the α/β accumulation a triangular solve never reads, so composing it would seat two columns no site
// consumes to avoid one flag.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolveDirection {
    public static readonly SolveDirection Forward = new("forward", adjoint: false);
    public static readonly SolveDirection Adjoint = new("adjoint", adjoint: true);

    public bool IsAdjoint { get; }
}

// Cases carry ONLY per-occurrence factorization policy; the operand `Matrix<double>` is the entrypoint's own
// argument, so route identity never restates the operand and no per-case `A` re-projection switch exists.
// `ModifiedOrthonormal` is a CASE because the prior `Orthonormal { Modified: true }` property pattern forked one
// route out of the total dispatch ahead of the `Switch` that was supposed to close it.
[Union]
public abstract partial record FactorRoute {
    private FactorRoute() { }

    public sealed record DefinitePsd : FactorRoute;
    public sealed record SquarePivoting : FactorRoute;
    public sealed record Orthonormal(QRMethod Mode) : FactorRoute;
    public sealed record ModifiedOrthonormal : FactorRoute;
    public sealed record Spectral(Symmetricity Sym) : FactorRoute;
    public sealed record RankRevealing : FactorRoute;

    // The receipt's route variant is a case DISCRIMINANT the generator already emits, never `GetType().Name`.
    public string Key => Switch(
        definitePsd: static _ => "definite-psd", squarePivoting: static _ => "square-pivoting",
        orthonormal: static _ => "orthonormal", modifiedOrthonormal: static _ => "modified-orthonormal",
        spectral: static _ => "spectral", rankRevealing: static _ => "rank-revealing");
}

// How a bounded solve ENDED, as a total union every consumer folds. `Exhausted` carries the budget it spent and
// the iterate rides the outcome, so the ruled relaxed-criterion retry survives without a `Fin.Fail`; `Truncated`
// carries the a-posteriori gauge and the cap it cleared, which the sketch tuple used to leave unnamed.
[Union]
public abstract partial record SolveTermination {
    private SolveTermination() { }

    public sealed record Converged : SolveTermination;
    public sealed record Exhausted(int Budget) : SolveTermination;
    public sealed record Truncated(double Gauge, double Cap) : SolveTermination;
}

// Whether the driver reported a spectrum at all. The driverless CPU `gelsy` path reports rank and returns an
// EMPTY singular-value tensor, so an unconditional finite-sigma demand rejects every least-squares solve — and a
// boolean whose absent arm answered `true` published a pass no measurement supports. The case travels to the
// receipt so a reader knows the floor was never checked.
[Union]
public abstract partial record SigmaEvidence {
    private SigmaEvidence() { }

    public sealed record Measured(double SigmaMin) : SigmaEvidence;
    public sealed record Unavailable : SigmaEvidence;

    public bool Admits(double floor) => Switch(
        measured: m => double.IsFinite(m.SigmaMin) && m.SigmaMin >= floor,
        unavailable: static _ => true);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ONE outcome every solve, refinement, fit, and sketch on this page returns, generic over what it iterated
// to: a `Vector<double>` for a linear or nonlinear solve, a `Factorization` for a randomized sketch. Five
// carriers answered this question in four shapes, one of them a naked tuple leaking a foreign iteration enum and
// one of them a `bool` the kernel had already refuted.
public sealed record SolveOutcome<T>(T Iterate, DenseSubstrate Served, double Residual, SolveTermination Termination, int Steps) {
    public static SolveOutcome<T> Settled(T iterate, DenseSubstrate served, double residual) =>
        new(iterate, served, residual, new SolveTermination.Converged(), Steps: 0);
}

// `Derive` seeds `SigmaMax` from the O(n²) Frobenius bound; an existing `Svd` refines through `WithSigma`
// without paying a second decomposition for a threshold. `ResidualCap` READS the envelope owner rather than
// re-spelling its `16ε` body, which is the de-sync that owner exists to foreclose.
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
            ResidualCap: Cap(frobenius));
    }

    public TolerancePolicy WithSigma(Svd<double> held) =>
        held.L2Norm is var sigma && double.IsFinite(sigma)
            ? this with { SigmaMax = sigma, RankFloor = sigma.EpsilonOf() * MaxDim, ResidualCap = Cap(sigma) }
            : this;

    private static double Cap(double scale) => ToleranceClass.CrossPlatformVariant.Bound(1, Math.Max(1.0, scale));

    public bool Admits(double residual) => double.IsFinite(residual) && residual <= ResidualCap;
}

public sealed record SketchPolicy(int Rank, int Oversample, int PowerIterations, double TruncationCap, long Seed) {
    public static readonly SketchPolicy Rom = new(Rank: 64, Oversample: 10, PowerIterations: 2, TruncationCap: 1e-6, Seed: 0L);

    // Four INDEPENDENT policy facts accumulate into one refusal; short-circuiting them made a caller fix one
    // field per round trip.
    public Fin<SketchPolicy> Admit() =>
        (Gate(Rank >= 1, TensorReason.PolicyInvalid, "sketch-rank", Rank),
         Gate(Oversample >= 0, TensorReason.PolicyInvalid, "sketch-oversample", Oversample),
         Gate(PowerIterations >= 0, TensorReason.PolicyInvalid, "sketch-power", PowerIterations),
         Gate(double.IsFinite(TruncationCap) && TruncationCap >= 0.0, TensorReason.PolicyInvalid, "sketch-cap", TruncationCap))
            .Apply((_, _, _, _) => this).As().ToFin();

    private static Validation<Error, Unit> Gate<TValue>(bool holds, TensorReason reason, string site, TValue observed)
        where TValue : IFormattable =>
        holds ? unit : reason.Fault(site, observed.ToString(null, CultureInfo.InvariantCulture));
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
        svd: static _ => FactorizationKind.Svd, evd: static _ => FactorizationKind.Evd, sketched: static _ => FactorizationKind.Sketched);

    public Matrix<double> Solve(Matrix<double> rhs) => Switch(
        state: rhs,
        lu: static (b, f) => f.Decomposition.Solve(b),
        qr: static (b, f) => f.Decomposition.Solve(b),
        cholesky: static (b, f) => f.Decomposition.Solve(b),
        svd: static (b, f) => f.Decomposition.Solve(b),
        evd: static (b, f) => f.Decomposition.Solve(b),
        sketched: static (b, f) => f.Core.Solve(f.Range.TransposeThisAndMultiply(b)));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class OperandGate {
    public static Fin<Matrix<double>> Admit(Matrix<double> a) =>
        a.ToColumnMajorArray() is var flat && TensorPrimitives.IsFiniteAll<double>(flat)
            ? Fin.Succ(a)
            : TensorPrimitives.IsNaNAny<double>(flat)
                ? TensorReason.NonFinite.Fail<Matrix<double>>("operand-nan")
                : TensorReason.NonFinite.Fail<Matrix<double>>("operand-inf");

    public static Matrix<double> Symmetrize(Matrix<double> a) => (a + a.Transpose()).Multiply(0.5);

    // MathNet `Cholesky()` throws on a non-square or non-PD operand; `Op.Catch` preserves that provider error,
    // while `DeterminantLn` finiteness rejects a degenerate factor that did not throw.
    public static Fin<Cholesky<double>> Definite(Matrix<double> spd) =>
        spd.RowCount != spd.ColumnCount
            ? TensorReason.ShapeMismatch.Fail<Cholesky<double>>("non-square-spd", $"{spd.RowCount}x{spd.ColumnCount}")
            : Op.Of(name: "tensor.cholesky").Catch(() => Fin.Succ(spd.Cholesky()))
                .Bind(static chol => double.IsFinite(chol.DeterminantLn)
                    ? Fin.Succ(chol)
                    : TensorReason.StructuralRank.Fail<Cholesky<double>>("spd-degenerate-logdet"));

    public static Fin<QR<double>> Orthonormal(Matrix<double> a, QRMethod mode, double floor) =>
        a.QR(mode) is var qr && qr.R.Diagonal().Map(Math.Abs).All(value => double.IsFinite(value) && value >= floor)
            ? Fin.Succ(qr)
            : TensorReason.StructuralRank.Fail<QR<double>>("rank-deficient-qr");

    // `GramSchmidt<T>` IS the modified variant — `DenseGramSchmidt.Factorize` updates every remaining column in
    // place against each normalized q_i (decompile-proved) — so the package factorization serves whole and the
    // rank gate reads the R diagonal exactly as `Orthonormal` gates QR; the refused column and its norm are the
    // first sub-floor diagonal, the same quantity the deleted hand loop measured.
    public static Fin<Vector<double>> Modified(Matrix<double> a, Vector<double> rhs, double floor) =>
        a.GramSchmidt() is var mgs
        && mgs.R.Diagonal().Map(Math.Abs).ToList() is var pivots
        && pivots.FindIndex(value => !double.IsFinite(value) || value < floor) is var deficient
        && deficient < 0
            ? Fin.Succ(mgs.Solve(rhs))
            : TensorReason.StructuralRank.Fail<Vector<double>>("rank-deficient-modified-gram-schmidt", $"column={deficient}", $"norm={pivots[deficient]:e3}", $"floor={floor:e3}");
}

// --- [NATIVE_SUBSTRATE] --------------------------------------------------------------------
public static class AtenFloor {
    // Residency is a LOAD probe, never a RID predicate or a file-presence check. The vendored CPU payload hard-links
    // an ABSOLUTE OpenMP library path that its own package does not place, so every RID predicate can pass over a
    // fully-present payload while the first tensor touch throws a type-initializer failure out of the two-step native
    // loader — a presence-only gate therefore publishes an accelerated route that cannot execute one operand, and its
    // receipt would carry a substrate tag for a substrate that never ran. The probe forces the native bring-up ONCE
    // behind `LazyThreadSafetyMode.ExecutionAndPublication`, and its refusal is the typed evidence the managed degrade
    // rides rather than an exception escaping whichever solve happened to touch `torch` first.
    static readonly Lazy<Fin<Unit>> Load = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    public static bool Resident() => Load.Value.IsSucc;

    // Refusal CLASS, never the loader's own message: the determinism tag folds it, so an operand solved on the managed
    // substrate BECAUSE the floor refused keys distinctly from one solved on a host that ships no payload at all.
    public static Option<string> Refusal => Load.Value.Match(Succ: static _ => None, Fail: static _ => Some("aten-refused"));

    // Thread count and default dtype bind INSIDE the probe because they ARE the first native touch; a caller that
    // configured before probing would take the load failure on its own frame instead of on the rail.
    public static void Configure() => ignore(Load.Value);

    static Fin<Unit> Probe() =>
        Op.Of(name: "tensor.aten-load").Catch(() => {
            torch.set_num_threads(Environment.ProcessorCount);
            torch.set_default_dtype(ScalarType.Float64);
            using Tensor witness = torch.from_array(new[] { 0.0 }, ScalarType.Float64);
            return Fin.Succ(witness.NumberOfElements);
        })
        .Bind(static elements => elements == 1L
            ? Fin.Succ(unit)
            : TensorReason.ShapeMismatch.Fail<Unit>("aten-witness", elements.ToString(CultureInfo.InvariantCulture)));
}

public static class AtenDense {
    // The held LU capsule is the ONE public held-solve surface: the raw `(lu, pivots)` pair stays inside it, and
    // the forwarding overload that published that pair as a second public entrypoint is deleted.
    public sealed class HeldFactor(Tensor lu, Tensor pivots) : IDisposable {
        public Fin<Vector<double>> Solve(Vector<double> rhs, SolveDirection direction) {
            using DisposeScope scope = torch.NewDisposeScope();
            Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
            return Egress(torch.linalg.lu_solve(lu, pivots, b, left: true, adjoint: direction.IsAdjoint));
        }

        public void Dispose() {
            pivots.Dispose();
            lu.Dispose();
        }
    }

    // Definite and spectral routes symmetrize before ingress, then select native factorization by the same
    // `FactorRoute` case as the managed leg. `None` is this substrate DECLINING the operand — the managed
    // terminal then serves it — where a fault is the operand itself refusing on both. `ModifiedOrthonormal`
    // declines outright because ATen ships no modified Gram-Schmidt.
    public static Fin<Option<Vector<double>>> Solve(FactorRoute route, Matrix<double> matrix, Vector<double> rhs, TolerancePolicy tol) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Matrix<double> operand = route is FactorRoute.DefinitePsd or FactorRoute.Spectral ? OperandGate.Symmetrize(matrix) : matrix;
        Tensor a = Lift(operand);
        Tensor b = torch.from_array(rhs.AsArray() ?? rhs.ToArray(), ScalarType.Float64).reshape(rhs.Count, 1);
        return route.Switch(
            definitePsd:         _ => Spd(a, b).Map(static x => Some(x)),
            squarePivoting:      _ => General(a, b).Map(static x => Some(x)),
            orthonormal:         _ => LeastSquares(a, b, tol),
            modifiedOrthonormal: _ => Fin.Succ(Option<Vector<double>>.None),
            spectral:            _ => SymmetricIndefinite(a, b).Map(static x => Some(x)),
            rankRevealing:       _ => LeastSquares(a, b, tol));
    }

    // ONE info gate for the three factor-then-solve legs: run the factorization, read the `_ex` info code, and
    // solve only on a zero. A driver that reports NO info tensor reported no error, and the original-operator
    // witness downstream is what catches a silent breach on that path.
    static Fin<Vector<double>> Gated(TensorReason reason, string site, Option<Tensor> info, Func<Fin<Tensor>> solve) =>
        info.Match(
            None: solve,
            Some: reported => reported.ReadCpuInt32(0) is var status && status == 0
                ? solve()
                : reason.Fail<Tensor>(site, $"info={status}")
        .Bind(Egress);

    // SPD: Cholesky factor + triangular `cholesky_solve` — the structure the general `solve_ex` discards.
    static Fin<Vector<double>> Spd(Tensor a, Tensor b) {
        (Tensor l, Tensor info) = torch.linalg.cholesky_ex(a, check_errors: false);
        return Gated(TensorReason.NativeRejected, "aten-cholesky-nonspd", Some(info), () => Fin.Succ(torch.cholesky_solve(b, l, upper: false)));
    }

    // Symmetric-indefinite route uses Bunch-Kaufman `ldl_factor_ex`/`ldl_solve`; absent pivots refuse before solve.
    static Fin<Vector<double>> SymmetricIndefinite(Tensor a, Tensor b) {
        (Tensor ld, Tensor? pivots, Tensor? info) = torch.linalg.ldl_factor_ex(a, hermitian: true, check_errors: false);
        return Gated(TensorReason.NativeRejected, "aten-ldl-singular", Optional(info), () => Optional(pivots).Match(
            Some: p => Fin.Succ(torch.linalg.ldl_solve(ld, p, b, hermitian: true)),
            None: () => TensorReason.NativeRejected.Fail<Tensor>("aten-ldl-no-pivots")));
    }

    // General square: pivoted-LU `solve_ex`.
    static Fin<Vector<double>> General(Tensor a, Tensor b) {
        (Tensor result, Tensor info) = torch.linalg.solve_ex(a, b, left: true, check_errors: false);
        return Gated(TensorReason.NativeRejected, "aten-solve-singular", Some(info), () => Fin.Succ(result));
    }

    // `lstsq` rank always gates; the sigma floor binds only where the driver YIELDS the spectrum, which
    // `SigmaEvidence` now states as a case rather than a boolean whose absent arm passed. Rank-deficient
    // verdicts are DECLINES, not refusals of the problem: `gelsy` gives the minimum-norm answer no meaning at
    // deficient rank, while the managed `Svd` route's pseudo-inverse solves exactly that shape, so rank-revealing
    // routing runs on BOTH substrates with the managed leg as its terminal.
    static Fin<Option<Vector<double>>> LeastSquares(Tensor a, Tensor b, TolerancePolicy tol) {
        (Tensor solution, Tensor residuals, Tensor rank, Tensor singular) = torch.linalg.lstsq(a, b);
        long full = Math.Min(a.shape[0], a.shape[1]);
        long observed = rank.NumberOfElements > 0 ? rank.ReadCpuInt64(0) : full;
        SigmaEvidence sigma = singular.NumberOfElements > 0
            ? new SigmaEvidence.Measured(singular.ReadCpuDouble(singular.NumberOfElements - 1))
            : new SigmaEvidence.Unavailable();
        return observed == full && sigma.Admits(tol.RankFloor)
            ? Egress(solution).Map(static x => Some(x))
            : Fin.Succ(Option<Vector<double>>.None);
    }

    // `lu_factor` pays O(n³) once; the capsule streams right-hand sides through both directions.
    public static Fin<HeldFactor> Held(Matrix<double> operand) {
        using DisposeScope owner = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        return Op.Of(name: "tensor.aten-lu-factor").Catch(() => {
                using Tensor a = Lift(operand);
                (Tensor lu, Tensor pivots) = torch.linalg.lu_factor(a, pivot: true);
                owner.Detach(lu);
                owner.Detach(pivots);
                return Fin.Succ(new HeldFactor(lu, pivots));
            });
    }

    // The contraction is chosen by its INPUT: a subscript spec means `einsum`, its absence means the chain
    // product `multi_dot`. The two prior bodies were verbatim identical but for one native call and one slug.
    public static Fin<Matrix<double>> Contract(Option<string> spec, Seq<Matrix<double>> operands) {
        using DisposeScope scope = torch.NewDisposeScope();
        using IDisposable noGrad = torch.inference_mode(true);
        Tensor[] lifted = [.. operands.Map(Lift)];
        Tensor result = spec.Match(Some: s => torch.einsum(s, lifted), None: () => torch.linalg.multi_dot(lifted));
        return result.shape.Length == 2
            ? Fin.Succ(Matrix<double>.Build.Dense((int)result.shape[0], (int)result.shape[1], result.t().reshape(result.NumberOfElements).data<double>().ToArray()))
            : TensorReason.ShapeMismatch.Fail<Matrix<double>>("aten-contract-rank",
                result.shape.Length.ToString(CultureInfo.InvariantCulture),
                spec.Match(Some: static _ => "einsum", None: static () => "multi-dot"));
    }

    // One column-major lift for every ingress; five hand-spelled copies of this reshape-transpose drifted apart.
    static Tensor Lift(Matrix<double> m) =>
        torch.from_array(m.ToColumnMajorArray(), ScalarType.Float64).reshape(m.ColumnCount, m.RowCount).t();

    static Fin<Vector<double>> Egress(Tensor x) =>
        Fin.Succ(Vector<double>.Build.DenseOfArray(x.reshape(x.NumberOfElements).data<double>().ToArray()));
}

// --- [DENSE_ROUTE] -------------------------------------------------------------------------
public static class DenseRoute {
    // Substrate legs return an unwitnessed solution; one original-operator gate gives either leg an identical
    // typed residual rejection. The native leg's `None` is a decline, so the managed terminal serves and the
    // carrier records WHICH substrate that was.
    public static Fin<SolveOutcome<Vector<double>>> Solve(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate) =>
        OperandGate.Admit(operand).Bind(_ =>
            substrate.Native
                ? AtenDense.Solve(route, operand, rhs, tol).Bind(served => served.Match(
                    Some: x => Witness(operand, x, rhs, tol, substrate),
                    None: () => Managed(route, operand, rhs, tol).Bind(x => Witness(operand, x, rhs, tol, DenseSubstrate.Managed))))
                : Managed(route, operand, rhs, tol).Bind(x => Witness(operand, x, rhs, tol, DenseSubstrate.Managed)));

    // `ModifiedOrthonormal` solves in place rather than returning a handle, so the arm answers the field
    // directly; every other arm answers an `ISolver<double>` and the shared tail solves it.
    static Fin<Vector<double>> Managed(FactorRoute route, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol) =>
        route.Switch<(Matrix<double> A, Vector<double> B, double Floor), Fin<Vector<double>>>(
            state: (A: operand, B: rhs, Floor: tol.RankFloor),
            definitePsd:         static (s, _) => OperandGate.Definite(OperandGate.Symmetrize(s.A)).Map(h => h.Solve(s.B)),
            squarePivoting:      static (s, _) => Fin.Succ(s.A.LU().Solve(s.B)),
            orthonormal:         static (s, c) => OperandGate.Orthonormal(s.A, c.Mode, s.Floor).Map(h => h.Solve(s.B)),
            modifiedOrthonormal: static (s, _) => OperandGate.Modified(s.A, s.B, s.Floor),
            spectral:            static (s, c) => Fin.Succ(OperandGate.Symmetrize(s.A).Evd(c.Sym).Solve(s.B)),
            rankRevealing:       static (s, _) => Fin.Succ(s.A.Svd(computeVectors: true).Solve(s.B)));

    // Both attempts read the residual the witness already measured, and BOTH refusals reach the caller: the
    // prior form discarded the primary error whole under a wildcard and dropped the serving substrate with it,
    // so a receipt could not say which route or which substrate served a conditioned solve.
    public static Fin<SolveOutcome<Vector<double>>> Conditioned(FactorRoute primary, FactorRoute secondary, Matrix<double> operand, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate substrate) =>
        Solve(primary, operand, rhs, tol, substrate)
            .BindFail(first => Solve(secondary, operand, rhs, tol, substrate)
                .MapFail(second => Error.Many([first, second])));

    static Fin<SolveOutcome<Vector<double>>> Witness(Matrix<double> a, Vector<double> x, Vector<double> rhs, TolerancePolicy tol, DenseSubstrate served) =>
        Relative(a, x, rhs) is var residual && tol.Admits(residual)
            ? Fin.Succ(SolveOutcome<Vector<double>>.Settled(x, served, residual))
            : TensorReason.WitnessFail.Fail<SolveOutcome<Vector<double>>>("witness-fail", $"substrate={served.Key}", $"residual={residual:e3}", $"cap={tol.ResidualCap:e3}");

    static double Relative(Matrix<double> a, Vector<double> x, Vector<double> rhs) {
        Vector<double> residual = rhs - a.Multiply(x);
        double[] flat = residual.AsArray() ?? residual.ToArray();
        double[] b = rhs.AsArray() ?? rhs.ToArray();
        return TensorPrimitives.Norm<double>(flat) / Math.Max(1.0, TensorPrimitives.Norm<double>(b));
    }
}

public static class DenseOps {
    // Generated total `FactorizationKind.Switch` makes a new row require a build arm at compile time;
    // reference-typed `matrix` threads as switch state without span-lane restrictions. `Sketched` builds through
    // `Sketch`, which needs a policy this arity does not carry, so the arm names that owner rather than
    // fabricating a default rank.
    public static Fin<Factorization> Decompose(Matrix<double> matrix, FactorizationKind kind) =>
        kind.Switch(
            state: matrix,
            lu: static m => Fin.Succ<Factorization>(new Factorization.Lu(m.LU())),
            qr: static m => Fin.Succ<Factorization>(new Factorization.Qr(m.QR())),
            cholesky: static m => OperandGate.Definite(OperandGate.Symmetrize(m)).Map(static h => (Factorization)new Factorization.Cholesky(h)),
            svd: static m => Fin.Succ<Factorization>(new Factorization.Svd(m.Svd(computeVectors: true))),
            evd: static m => Fin.Succ<Factorization>(new Factorization.Evd(m.Evd())),
            sketched: static _ => TensorReason.PolicyInvalid.Fail<Factorization>("sketch-needs-policy"));

    // Halko range capture computes `Y = (A·Aᵀ)^q·A·Ω`, thin-QR, then the small SVD of `QᵀA`. The a-posteriori
    // `‖A − Q·QᵀA‖_F/‖A‖_F` gauge is the outcome's residual and its `Truncated` termination, so a caller reads
    // the same carrier shape a solve returns.
    public static Fin<SolveOutcome<Factorization>> Sketch(Matrix<double> a, SketchPolicy policy) =>
        policy.Admit().Bind(admitted => {
            int width = Math.Min(admitted.Rank + admitted.Oversample, Math.Min(a.RowCount, a.ColumnCount));
            Matrix<double> omega = Gaussian(a.ColumnCount, width, admitted.Seed);
            Matrix<double> y = Enumerable.Range(0, admitted.PowerIterations)
                .Aggregate(a.Multiply(omega), (range, _) => a.Multiply(a.TransposeThisAndMultiply(range)));
            Matrix<double> q = y.QR(QRMethod.Thin).Q;
            Matrix<double> b = q.TransposeThisAndMultiply(a);
            Svd<double> small = b.Svd(computeVectors: true);
            double gauge = (a - q.Multiply(b)).FrobeniusNorm() / Math.Max(1.0, a.FrobeniusNorm());
            return double.IsFinite(gauge) && gauge <= admitted.TruncationCap
                ? Fin.Succ(new SolveOutcome<Factorization>(
                    new Factorization.Sketched(q, small, gauge), DenseSubstrate.Managed, gauge,
                    new SolveTermination.Truncated(gauge, admitted.TruncationCap), Steps: admitted.PowerIterations))
                : TensorReason.WitnessFail.Fail<SolveOutcome<Factorization>>("sketch-truncation", $"{gauge:e3}", $"cap={admitted.TruncationCap:e3}", $"rank={admitted.Rank}");
        });

    // Sketch draws cross through the kernel `Deterministic.Source` adapter — the one sanctioned `System.Random`
    // crossing. The draw buffer BECOMES the matrix's backing store under `Build.Dense(rows, columns, T[])`, so it
    // is owned storage rather than kernel scratch and a pooled rent would be released out from under the matrix.
    static Matrix<double> Gaussian(int rows, int columns, long seed) {
        double[] values = GC.AllocateUninitializedArray<double>(rows * columns);
        new Normal(0.0, 1.0, Deterministic.Source(seed)).Samples(values);
        return Matrix<double>.Build.Dense(rows, columns, values);
    }

    // The refinement sweep spends at most `cap` steps and reports which way it ended: an admitted residual is
    // `Converged`, a spent budget is `Exhausted(cap)` carrying the partial iterate the ruled relaxed-criterion
    // retry needs. A converged state passes through the remaining fold in constant time — the O(n²) multiply is
    // what the freeze avoids — and a non-finite residual is a refusal, never a status.
    public static Fin<SolveOutcome<Vector<double>>> Refine(
        Matrix<double> matrix, ISolver<double> held, Vector<double> rhs, TolerancePolicy tol, int cap) {
        Vector<double> dx = Vector<double>.Build.Dense(rhs.Count);
        Vector<double> scratch = Vector<double>.Build.Dense(rhs.Count);
        double rhsNorm = Math.Max(1.0, rhs.L2Norm());
        (Vector<double> Field, int Steps, double Residual) folded = toSeq(Enumerable.Range(0, cap)).Fold(
            (Field: held.Solve(rhs), Steps: 0, Residual: double.MaxValue),
            (acc, _) => {
                if (tol.Admits(acc.Residual)) { return acc; }
                matrix.Multiply(acc.Field, scratch);
                rhs.Subtract(scratch, scratch);
                double residual = scratch.L2Norm() / rhsNorm;
                return tol.Admits(residual)
                    ? (acc.Field, acc.Steps, residual)
                    : (Refined(held, scratch, dx, acc.Field), acc.Steps + 1, residual);
            });
        return double.IsFinite(folded.Residual)
            ? Fin.Succ(new SolveOutcome<Vector<double>>(
                folded.Field, DenseSubstrate.Managed, folded.Residual,
                tol.Admits(folded.Residual) ? new SolveTermination.Converged() : new SolveTermination.Exhausted(cap),
                folded.Steps))
            : TensorReason.NonFinite.Fail<SolveOutcome<Vector<double>>>("refinement-nonfinite", $"r={folded.Residual}");
    }

    static Vector<double> Refined(ISolver<double> held, Vector<double> scratch, Vector<double> dx, Vector<double> field) {
        held.Solve(scratch, dx);
        field.Add(dx, field);
        return field;
    }

    // nnz is STRUCTURALLY zero and `dense` is the settled receipt format vocabulary: a dense operand has no
    // sparsity to count, which is exactly the kind of zero that must say so at its site. Tag folds the SERVING
    // substrate off the outcome carrier, so a native leg that declined and degraded to the managed terminal keys
    // as the managed run it was rather than the accelerated one it was asked to be.
    const long DenseNnz = 0L;
    const string DenseFormat = "dense";

    public static ComputeReceipt.Factorization Receipt(LinearProvider provider, FactorRoute route, FactorizationKind kind, TolerancePolicy tol, SolveOutcome<Vector<double>> solved, int rows, int cols, CorrelationId correlation, Duration elapsed) =>
        new(provider.Key, kind.Key, rows, cols, DenseNnz, DenseFormat) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
            RouteVariant = route.Key, DeterminismTag = $"{solved.Served.DeterminismTag}|{provider.DeterminismTag}", ResidualCap = tol.ResidualCap, TrueResidual = solved.Residual,
        };
}
```

## [03]-[LEVENBERG_MARQUARDT]

- Owner: `LevenbergMarquardt` the damped Gauss-Newton nonlinear least-squares owner minimizing `‖r(θ)‖²`, solving each step's normal-equation system `(JᵀJ + λ·diag(JᵀJ))·δ = −Jᵀr` through the lane's gated `OperandGate.Definite` SPD route (the damped normal matrix is symmetric-PD by construction), adapting the damping λ on the actual-versus-predicted reduction; `LmPolicy` the iteration policy record; `DualCache` the value-keyed hyper-dual memo. The fit RETURNS the lane's `SolveOutcome<Vector<double>>` — there is no LM-local result carrier. This is the one Compute-INTERNAL nonlinear-least-squares owner, serving Compute's own solves and the host-free graduation/inference peers over the wire.
- Entry: ONE `Minimize` discriminating on the residual's input shape — the HYPERJET arm (`Func<DDScalar[], DDScalar[]>` residual authored once over the hyper-dual scalar) derives the EXACT Jacobian row-by-row through `GetGradient()` and is the CANONICAL provider (the `Stats/estimator` ARMA/Holt/state-space fits and the `Solver/uncertainty` FORM/SORM exact-AD row all arrive on this arm; the finite-difference fall those consumers carried is deleted), while the black-box arm (`Func<Vector<double>, Matrix<double>>` caller-supplied Jacobian) survives for residuals authored outside the hyper-dual reach; both converge on the identical damped fold.
- Boundary: the LM fit carries NO result type of its own. A local `LmResult` collided by name with the kernel's public `LmResult` across the live `Rasm.Compute.csproj` → `Rasm.csproj` reference and spelled convergence as a `bool` where the kernel spells a status discriminant and states why — so the fit returns `SolveOutcome<Vector<double>>`, whose `Termination` is `Converged` on a met gradient-or-step criterion and `Exhausted(MaxIterations)` on a spent budget, whose `Steps` is the iteration count, and whose `Residual` is `‖r(θ)‖`. HyperJet returns PLAIN .NET arrays — `GetGradient()` answers `double[]` and `GetHessian()` answers `double[,]` — so the Jacobian assembly lifts through `Matrix<double>.SetRow(int, double[])` at the seam and no signature claims a MathNet export the package does not make. The hyper-dual memo keys on the parameter VECTOR's value, not on reference identity: two `Vector<double>` instances with identical values are the same evaluation point, and a `ReferenceEquals` memo re-seeded AD for every one of them while claiming in a comment to share the pass. The AEC-domain `Rasm.Materials` does NOT reference this owner — the strata graph is acyclic (app-platform consumes AEC-domain, never the reverse), so the Materials BRDF fit stays in-folder and the algorithms-doc thin-QR fit is a doctrine reference a Materials probe cites; a `Rasm.Compute` project reference or a "MathNet transitive via Rasm.Compute" claim from Materials is the forbidden AEC→app-platform edge. Linear least squares stays on the one-shot `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR; LM is the nonlinear damped iteration. Hand-rolled finite-difference Jacobians beside the HyperJet arm are the deleted form — FD survives ONLY where the residual is a genuine black box no hyper-dual instantiation can reach. This kernel dropped its own MKL reference (x86-64-only, cannot load on osx-arm64) and flagged Compute's as sibling-roster debt: the `native-mkl` `LinearProvider` row above IS the resolution — the RID-claim `Available` filter is the design (osx-arm64 resolves managed/OpenBLAS, an x64 deployment claims MKL), recorded here so the kernel's flag closes.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record LmPolicy(int MaxIterations, double GradientTolerance, double StepTolerance, double InitialDamping, double DampingUp, double DampingDown) {
    public static readonly LmPolicy Canonical = new(MaxIterations: 200, GradientTolerance: 1e-10, StepTolerance: 1e-12, InitialDamping: 1e-3, DampingUp: 10.0, DampingDown: 0.1);
}

// One hyper-dual pass shared between the residual and Jacobian projections of the SAME evaluation point, keyed
// on the point's VALUE: two vectors carrying identical parameters are one point, and a reference-keyed memo
// missed every one of them while its own comment claimed the pass was shared.
public sealed class DualCache(Func<DDScalar[], DDScalar[]> residual) {
    Option<(Vector<double> Theta, DDScalar[] Dual)> held = None;

    public DDScalar[] At(Vector<double> theta) {
        if (held.Map(cell => cell.Theta.Equals(theta)).IfNone(false)) { return held.Map(static cell => cell.Dual).IfNone([]); }
        DDScalar[] evaluated = residual(DDScalar.Variables(theta.ToArray(), order: 1));
        held = Some((theta, evaluated));
        return evaluated;
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class LevenbergMarquardt {
    // `DDScalar.Variables` seeds each parameter; every residual contributes its primal `Value` and exact
    // `GetGradient()` Jacobian row from one hyper-dual authoring. `GetGradient()` answers `double[]`, which is
    // exactly what `SetRow(int, double[])` takes, so no MathNet lift is fabricated at this seam.
    public static Fin<SolveOutcome<Vector<double>>> Minimize(Func<DDScalar[], DDScalar[]> residual, Vector<double> initial, LmPolicy policy) {
        DualCache cache = new(residual);
        return Minimize(
            theta => Vector<double>.Build.Dense([.. cache.At(theta).Select(static c => c.Value)]),
            theta => {
                DDScalar[] r = cache.At(theta);
                Matrix<double> j = Matrix<double>.Build.Dense(r.Length, theta.Count);
                for (int row = 0; row < r.Length; row++) { j.SetRow(row, r[row].GetGradient()); }
                return j;
            },
            initial, policy);
    }

    public static Fin<SolveOutcome<Vector<double>>> Minimize(Func<Vector<double>, Vector<double>> residual, Func<Vector<double>, Matrix<double>> jacobian, Vector<double> initial, LmPolicy policy) {
        (Vector<double> Theta, double Lambda, double Cost, int Steps, bool Done) folded = toSeq(Enumerable.Range(0, policy.MaxIterations)).Fold(
            (Theta: initial, Lambda: policy.InitialDamping, Cost: Cost(residual(initial)), Steps: 0, Done: false),
            (state, _) => {
                if (state.Done) { return state; }
                Vector<double> r = residual(state.Theta);
                Matrix<double> j = jacobian(state.Theta);
                Matrix<double> jtj = j.TransposeThisAndMultiply(j);
                Vector<double> jtr = j.TransposeThisAndMultiply(r);
                Matrix<double> damped = jtj + Matrix<double>.Build.DiagonalOfDiagonalVector(jtj.Diagonal()) * state.Lambda;
                // Gated SPD admission turns a rank-deficient damped factor into the LM increase-λ response,
                // never an escaped `Cholesky()` exception.
                return OperandGate.Definite(damped).Match(
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
                            ? (candidate, Math.Max(1e-12, state.Lambda * policy.DampingDown), trial, state.Steps + 1, jtr.InfinityNorm() < policy.GradientTolerance || step.L2Norm() < policy.StepTolerance)
                            : (state.Theta, state.Lambda * policy.DampingUp, state.Cost, state.Steps + 1, false);
                    },
                    Fail: _ => (state.Theta, state.Lambda * policy.DampingUp, state.Cost, state.Steps + 1, false));
            });
        return double.IsFinite(folded.Cost)
            ? Fin.Succ(new SolveOutcome<Vector<double>>(
                folded.Theta, DenseSubstrate.Managed, Math.Sqrt(2.0 * folded.Cost),
                folded.Done ? new SolveTermination.Converged() : new SolveTermination.Exhausted(policy.MaxIterations),
                folded.Steps))
            : TensorReason.NonFinite.Fail<SolveOutcome<Vector<double>>>("lm-nonfinite-cost");
    }

    static double Cost(Vector<double> r) => 0.5 * r.DotProduct(r);
}
```

## [04]-[SPECTRAL_LAW]

- Owner: `SpectralResult` `[Union]` carrying distinct dense-symmetric and dense-general cases — the `Symmetricity` flag selects five output axes together (eigenvector norm, real versus block-diagonal `D`, single-column versus column-pair encoding, ascending versus Schur-deflation order, working versus norm-gated solve); `SpectralOps.Decompose` the one constructor projecting `Evd<double>` onto the matched `SpectralResult` case; `SpectralOps.Modal` the Schur-pair decoder; `SpectralOps.Defect` the block eigen-residual; the kernel `Rasm/Numerics/spectral#FILTER_ALGEBRA` `SpectralFilter` `[Union]` the ONE eigenvalue-weight vocabulary this lane composes through its public `Weight(double)` and partial-monoid `Compose`.
- Entry: `public static SpectralResult Decompose(Matrix<double> a, Evd<double> evd, Symmetricity sym)` builds the `Symmetric` case from the real eigenvalues and the orthonormal vectors for a symmetric/Hermitian spectrum and the `General` case decoding the Schur pairs for the nonsymmetric spectrum, each carrying its block defect; `public static Matrix<Complex> Modal(Matrix<double> packed, Vector<Complex> values)` decodes real conjugate pairs from adjacent columns dispatched on `Math.Sign(values[j].Imaginary)`; `public static double Defect(Matrix<double> a, Matrix<double> vectors, Matrix<double> d)` computes `(A·V − V·D).FrobeniusNorm()`; `public static Fin<Vector<double>> Filtered(Evd<double> evd, double zeroFloor, double massFloor, params ReadOnlySpan<SpectralFilter> chain)` fuses the chain through `SpectralFilter.Compose` and applies the fused weight under two independent floors — an eigenvalue magnitude excluding the null space and a summed-weight magnitude gating emptiness — carrying the weight sum as evidence — one entrypoint over the singular, plural, and empty call, the empty spread yielding `SpectralFilter.Identity` because the partial monoid supplies the unit.
- Auto: `Decompose` is the single producer of `SpectralResult` — `SpectralOps.Modal`/`Defect`/`Filtered` are the per-axis kernels it composes and never independent return surfaces, so the result-union owner is always constructed and never unwired; `Modal` reads `Column(j)`+`Column(j+1)` for a positive-imaginary pair and `Column(j-1)`+`Column(j)` for a negative-imaginary pair, never `Column(j)` whole because that discards the imaginary half, and the backward read needs no bound guard because the real Schur reduction emits every conjugate pair positive-first, so index zero can never carry a negative imaginary part; `Defect` is the one signal both the managed throw rail and the native in-band info-code rail surface identically since no built-in eigen residual exists; `Filtered` fuses its chain before any spectrum walk so a non-composable pair rails at the fold instead of silently applying the last filter alone, then weights each `EigenValues` entry through the fused `SpectralFilter.Weight` excluding the zero mode (`|λ| < ε_zero ? 0.0 : Weight(λ)`, excluded never clamped) and fails a spectrum whose summed weight falls below its OWN floor rather than reading it as a zero signal.
- Boundary: `EigenValues` interprets `EigenVectors` because no parallel pairing array exists; nonsymmetric columns `Normalize(2)` before any modal weight because recovered columns are raw triangular solutions with arbitrary per-column norms; Hermitian eigenvectors stay complex because projecting them to real parts is incorrect; the library `Determinant`/`Rank`/`IsFullRank` are rejected in domain logic because `Determinant` short-circuits to `0.0` the moment any eigenvalue crosses the absolute zero test; eigenvalue equality is never asserted tighter than the convergence band because the exceptional-shift escape bakes the literal `0.964` into the last bits; only `DenseMatrix` reaches the native `EigenDecomp` and the managed `Evd` kernels are serial regardless of degree, so sign, ordering, and last bits differ across the seam and provider-mismatched eigenvector comparison short-circuits to span equivalence; `SpectralResult` is the only spectral return — a raw `Matrix<Complex>`/`double`/`Fin<Vector<double>>` leaking the spectral verdict past the owner is the deleted form because the consumer must dispatch on the `Symmetric`/`General` case to read the right `D` shape and ordering contract. Eigenvalue weights belong to the kernel vocabulary alone: a lane-local `EigenFilter` `[SmartEnum<string>]` carrying `passthrough`/`sqrt`/`inverse`/`inv-sqrt`/`exp`/`heat` weight lambdas is the DELETED form — it shadowed `SpectralFilter` (`passthrough` ≡ `IdentityCase`, `heat` ≡ `Heat` at unit time, `sqrt`/`inverse`/`inv-sqrt` ≡ `Power(½)`/`Power(−1)`/`Power(−½)`, `exp` ≡ `Amplify`), and no alias or forwarding row survives it; this lane composes the kernel owner and NEVER enumerates its cases, so a new weight form lands as one kernel case with zero edit here.
- Boundary — persistence: a computed basis leaving this lane persists through `[05]-[BASIS_ARTIFACT]`, never a per-carrier serializer beside each union case.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[Union]
public abstract partial record SpectralResult {
    private SpectralResult() { }

    public sealed record Symmetric(Matrix<double> Vectors, Vector<double> Values, double Defect) : SpectralResult;
    public sealed record General(Matrix<Complex> Modal, Vector<Complex> Values, double Defect) : SpectralResult;

    public double Defect => Switch(symmetric: static c => c.Defect, general: static c => c.Defect);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
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
        Fused(chain).Bind(filter => Weighted(evd, filter, zeroFloor, massFloor));

    static Fin<Vector<double>> Weighted(Evd<double> evd, SpectralFilter filter, double zeroFloor, double massFloor) {
        using MemoryOwner<double> weights = MemoryOwner<double>.Allocate(evd.EigenValues.Count);
        Span<double> lane = weights.Span;
        for (int mode = 0; mode < lane.Length; mode++) {
            double lambda = evd.EigenValues[mode].Real;
            lane[mode] = Math.Abs(lambda) < zeroFloor ? 0.0 : filter.Weight(lambda);
        }
        double mass = TensorPrimitives.Sum<double>(lane);
        return Math.Abs(mass) >= massFloor
            ? Fin.Succ(Vector<double>.Build.DenseOfArray(lane.ToArray()))
            : TensorReason.WitnessFail.Fail<Vector<double>>("spectrum-fully-excluded", $"mass={mass:e3}", $"floor={massFloor:e3}");
    }

    // `Identity` is the monoid unit, so the empty spread is total and the singular call fuses to itself.
    static Fin<SpectralFilter> Fused(ReadOnlySpan<SpectralFilter> chain) =>
        LanguageExt.Iterable<SpectralFilter>.FromSpan(chain).Fold(
            Fin.Succ(SpectralFilter.Identity),
            static (fused, next) => fused.Bind(held => held.Compose(next).ToFin(
                (Error)TensorReason.RowMissing.Fault("filter-not-composable", $"{held.GetType().Name}+{next.GetType().Name}"))));
}
```

## [05]-[BASIS_ARTIFACT]

- Owner: `BasisKind` `[SmartEnum<string>]` the artifact row axis — sketch · modal · rbf — each row carrying its support-block roster as row data; `BasisArtifact` the one container carrier absorbing the three unpersisted basis carriers (`Factorization.Sketched`, `SpectralResult.Symmetric`, `RbfFit`) behind ONE parameterized writer and one rank-truncating reader over `Runtime/archive#HDF_ARCHIVE`.
- Cases: `BasisKind` rows sketch (range basis + singular values + `truncation` gauge) · modal (mode shapes + eigenvalues + `defect` gauge) · rbf (weight columns + `centres`/`trend` support blocks + `radius`/`order` gauges + the radial `family` key) (3).
- Entry: `BasisArtifact.Of` is the polymorphic projection — one overload per absorbed carrier, so no carrier grows a serializer of its own; `Write<A>(StreamPool, CorrelationId, HdfArchivePolicy, BasisArtifact, Func<RecyclableMemoryStream, Fin<A>>)` is the ONE writer and it OWNS the pooled rent for the whole encode, handing a continuation the positioned reader; `Read(HdfHandle, BasisKind, Option<int> rank)` reads back rank-truncated — the first `rank` columns and their paired values as one hyperslab, exactly `rank` chunk reads by the column-axis layout.
- Output: every artifact lands content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit`; the capsule is job machinery, never a store.
- Packages: MathNet.Numerics (`Matrix<double>.ToArray`, `Build.DenseOfRowMajor`, `Svd<double>.S`), PureHDF (`H5File`, `H5Group`, `H5Dataset<T>`, `HyperslabSelection`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`), Microsoft.IO.RecyclableMemoryStream, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, kernel signal capsule), Rasm.Persistence (project), BCL inbox
- Growth: a new basis family is one `BasisKind` row with one `Of` overload projecting its carrier — the writer and reader bodies stay untouched; a new gauge is one map entry.
- Boundary: `Values` pairs the basis columns and is EMPTY where the kind measures none (rbf) — a zero-filled vector publishes singular values nobody computed; the rank truncation serves the reader that restarts a solve warm from a stored range or modal basis, so it applies to the primary matrix and its paired values alone and support blocks always read whole; the `SolveDedupKey`/determinism-tag law holds — a basis read back re-enters the solve rails as data and never claims the provenance of the run that wrote it. The pooled stream NEVER leaves the writer: release brackets the acquisition on every path, where a `catch { staged.Dispose(); throw; }` bracketed the failure OUTCOME and let a typed refusal raised inside the same body walk past the release. Read-back slabs are pooled rents sized by the dimensions the container declares, released on the same frame the matrix is built.

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
    public ImmutableArray<string> Support { get; }
}

// `Values` pairs basis columns (singular values, eigenvalues) and is EMPTY where the kind measures none;
// `Gauges` carries the kind's scalar evidence; `Family` the radial row key only the rbf kind states.
public sealed record BasisArtifact(
    BasisKind Kind, Matrix<double> Basis, ImmutableArray<double> Values, Map<string, Matrix<double>> Support, Map<string, double> Gauges, Option<string> Family) {

    public static BasisArtifact Of(Factorization.Sketched sketch) =>
        new(BasisKind.Sketch, sketch.Range, [.. sketch.Core.S], default, Map(("truncation", sketch.Truncation)), None);

    public static BasisArtifact Of(SpectralResult.Symmetric modal) =>
        new(BasisKind.Modal, modal.Vectors, [.. modal.Values], default, Map(("defect", modal.Defect)), None);

    public static BasisArtifact Of(RbfFit fit) =>
        new(BasisKind.Rbf, fit.Weights, [],
            Map(("centres", fit.Centres), ("trend", fit.PolynomialCoefficients)),
            Map(("radius", fit.Radius), ("order", (double)fit.PolynomialOrder)), Some(fit.Kernel.Key));

    // ONE writer, and the pooled rent NEVER escapes it: `bracket` releases the acquisition on the success arm,
    // the refusal arm, and the throw arm alike, where the prior hand `catch/rethrow` released on the throw
    // alone. `/basis` chunks `[rows, 1]` so read-back truncates by rank; the create-only Begin session closes in
    // one call and the continuation reads the stream positioned at zero.
    public static IO<Fin<A>> Write<A>(StreamPool pool, CorrelationId correlation, HdfArchivePolicy policy, BasisArtifact artifact, Func<RecyclableMemoryStream, Fin<A>> read) =>
        IO.bracket(
            acquire: IO.lift(() => pool.Get(correlation, new StreamGrant.Open())),
            release: static staged => staged.Iter(static stream => stream.Dispose()),
            use: staged => IO.lift(() => staged.Bind(stream => Encode(stream, policy, artifact).Bind(read))));

    static Fin<RecyclableMemoryStream> Encode(RecyclableMemoryStream staged, HdfArchivePolicy policy, BasisArtifact artifact) =>
        Op.Of(name: "hdf5.basis-write").Catch(() => {
                H5DatasetCreation creation = policy.Creation();
                H5Group group = new() {
                    ["basis"] = new H5Dataset<double[,]>(artifact.Basis.ToArray(), chunks: [(uint)artifact.Basis.RowCount, 1u], datasetCreation: creation),
                };
                if (artifact.Values.Length > 0) {
                    group["values"] = new H5Dataset<double[]>([.. artifact.Values], chunks: [(uint)artifact.Values.Length], datasetCreation: creation);
                }

                artifact.Support.Iter((name, block) =>
                    group[name] = new H5Dataset<double[,]>(block.ToArray(), chunks: [(uint)block.RowCount, 1u], datasetCreation: creation));
                group.Attributes["kind"] = artifact.Kind.Key;
                artifact.Gauges.Iter((name, value) => group.Attributes[name] = value);
                artifact.Family.IfSome(family => group.Attributes["family"] = family);
                HdfArchive.Begin(new H5File { [artifact.Kind.Key] = group }, staged, policy).Dispose();
                staged.Position = 0;
                return Fin.Succ(staged);
            });

    // Rank-truncated read-back: the first `rank` columns of `/basis` and their paired values — exactly `rank`
    // chunk reads under the column-axis layout; support blocks read whole, gauges off the attribute roster.
    // Every slab is a pooled rent released on this frame, not a fresh array per block.
    public static Fin<BasisArtifact> Read(HdfHandle archive, BasisKind kind, Option<int> rank) =>
        Op.Of(name: "hdf5.basis-read").Catch(() => {
                NativeGroup group = archive.Group(kind.Key);
                NativeDataset basisSet = archive.Dataset($"{kind.Key}/basis");
                int rows = checked((int)basisSet.Space.Dimensions[0]);
                int cols = checked((int)basisSet.Space.Dimensions[1]);
                int keep = rank.Match(Some: r => Math.Min(r, cols), None: () => cols);
                using MemoryOwner<double> slab = MemoryOwner<double>.Allocate(rows * keep);
                basisSet.Read<double>(archive.Access, slab.Span, new HyperslabSelection(2, [0UL, 0UL], [(ulong)rows, (ulong)keep]));
                ImmutableArray<double> values = [];
                if (archive.Exists($"{kind.Key}/values")) {
                    using MemoryOwner<double> paired = MemoryOwner<double>.Allocate(keep);
                    archive.Dataset($"{kind.Key}/values").Read<double>(archive.Access, paired.Span, new HyperslabSelection(0, (ulong)keep));
                    values = [.. paired.Span];
                }

                Map<string, Matrix<double>> support = toSeq(kind.Support).Fold(
                    Map<string, Matrix<double>>(),
                    (held, name) => {
                        NativeDataset block = archive.Dataset($"{kind.Key}/{name}");
                        int r = checked((int)block.Space.Dimensions[0]);
                        int c = checked((int)block.Space.Dimensions[1]);
                        using MemoryOwner<double> data = MemoryOwner<double>.Allocate(r * c);
                        block.Read<double>(archive.Access, data.Span, new HyperslabSelection(2, [0UL, 0UL], [(ulong)r, (ulong)c]));
                        return held.Add(name, Matrix<double>.Build.DenseOfRowMajor(r, c, data.Span.ToArray()));
                    });
                Map<string, double> gauges = toSeq(group.Attributes())
                    .Filter(attribute => attribute.Name is not ("kind" or "family"))
                    .Fold(Map<string, double>(), (held, attribute) => held.Add(attribute.Name, attribute.Read<double>()));
                Option<string> family = group.AttributeExists("family") ? Some(group.Attribute("family").Read<string>()) : None;
                return Fin.Succ(new BasisArtifact(kind, Matrix<double>.Build.DenseOfRowMajor(rows, keep, slab.Span.ToArray()), values, support, gauges, family));
            });
}
```

## [06]-[PROVIDER_CLAIMS]

- Owner: the claim-gated provider-rank selection, the provenance snapshot taken at solve construction, and the per-solve telemetry deepening over the existing `BenchmarkRow`, `BenchmarkClaim`, and `ReceiptSurface` owners — and the `ResidualStream` fourth-order residual-moment accumulator the numeric lane owns over the kernel `Rasm/Domain/stats#MOMENTS` `Stat<Scalar>` receipt because `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` carries the `rasm.compute.solve.residual` histogram but no moment accumulator, so the accumulator that folds into that histogram is a numeric-lane owner consumed at the receipt sink.
- Entry: `LinearProvider.Select` consumes the resolved `BenchmarkRow` claim — the winner of `ModelResultIndex.Claim(rows, fingerprint)` resolved at composition against the running fingerprint under the index-owned `RecencyHorizon` and clock — so the chosen provider RID is claim-gated, never a static default; `SolveProvenance.Snapshot(LinearProvider provider)` captures the SELECTED row beside the `LinearAlgebraControl.Provider` type name and the public `Control.MaxDegreeOfParallelism` degree; `ResidualStream.Push(residual, key)` folds each witnessed solve residual into the kernel `Stat<Scalar>` receipt, whose `Variance`/`Deviation` read the caller-stated kernel `MomentNormalizer` policy; the `Selection`-class evidence row names the chosen provider and the claim that gated it.
- Auto: a native BLAS provider rank wins only behind a fingerprint-matched `BenchmarkRow` resolved by the Persistence `ModelResultIndex.Claim` owner and threaded in, never re-resolved here; bitwise-versus-bounded equality derives from the provider/type/degree triple because the partition-tree topology varies run-to-run, so a recorded value is correct for one core count only and bit-comparison on another host falsely flags tampering; every dense and sparse solve emits the `Factorization` receipt and rides the `ReceiptSurface.Instruments` solve stream that counts factorizations by provider and kind, histograms the iterative-solver convergence residual, and accumulates the online residual fourth-order moments through the kernel `Stat<Scalar>.Merge` pairwise join `ResidualStream.Combine` composes, which re-enters the kernel's own validity oracle rather than asserting a count identity at the call; the stream guards at admission through the same all-finite predicate the operands cross because one pushed `NaN` permanently poisons every moment with no reset.
- Receipt: the `Factorization` `ComputeReceipt` case (provider, kind, route variant, tolerance, true residual, determinism tag, symbolic fill, rows, cols, nnz, format) is the per-solve evidence; the `rasm.compute.solve.factorizations` and `rasm.compute.solve.residual` instruments are owned by `Runtime/receipts#RECEIPT_UNION` `ReceiptSurface.Instruments` as settled vocabulary and never re-declared here; the `ResidualStream` accumulator is the numeric-lane moment carrier whose kernel-computed skewness/kurtosis evidence feeds the residual histogram tail.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, Rasm.Persistence (project), LanguageExt.Core, BCL inbox
- Growth: a new claim dimension is one column on the existing `BenchmarkClaim`; a new solve instrument is one row on `ReceiptSurface.Instruments`; a new moment is one column on the kernel `Stat<TCarrier>` receipt reaching this lane with zero edit here; zero new surface.
- Boundary — native BLAS is unreachable on every estate RID at the pinned adapter version, and the rows stay because the gate is what proves it. osx-arm64 answers `false` to every `Control.TryUseNative*` and resolves managed, since neither adapter ships an osx-arm64 payload. linux-x64 has a payload that cannot pass: `MathNet.Numerics.MKL.Linux-x64` tops out at native revision 9 while `MathNet.Numerics.Providers.MKL` holds a `MinimumCompatibleRevision` of 15 and throws `NotSupportedException("MKL Native Provider too old")` on load, and no OpenBLAS payload exists for a non-Windows RID at all — the stock system `libopenblas` exports no `query_capability`, which is the first symbol the adapter's own `IsAvailable` reads, so it is not a substitutable shim. win-x64 is the sole arm still open to a compatible payload. Those rows are therefore honest fail-closed capability: each `Available` probe answers `false`, `Select` falls to `Managed`, and a benchmark claim asserting native rank fails its gate rather than degrading silently — deleting them forfeits the win-x64 arm and re-opens a claim nothing checks.
- Boundary: provider rank is the `BenchmarkClaim` `Provider` column gated exactly like the SIMD and partition claims — a static native default beside the claim is the named defect; the claim is resolved by the Persistence `ModelResultIndex.Claim` owner whose recency horizon and clock are closed inside the index and threaded in, never re-resolved and never a second horizon; the solve instruments live on the `ReceiptSurface.Instruments` stream and a second numeric-lane-local instrument owner is the deleted form. `SolveProvenance` takes the SELECTED `LinearProvider` as an argument and records its key beside the ambient handle it is checking against: the substrate axis argues at length that a winner threads as a value because a receipt naming whatever a static held at read time names the wrong leg, and a provenance snapshot reading three ambient statics was the same defect landed twice on one page. The online residual accumulator is the KERNEL's: `Rasm/Domain/stats#MOMENTS` owns the four-moment weighted Welford recurrence, its Pebay pairwise join, and the `MomentNormalizer` Bessel-versus-population row, so the second copy this lane carried — a 2-arg normalizer and a hand `Push`/`Combine`/`Skewness`/`Kurtosis` — is DELETED rather than kept beside it. NAMED LOSS: the local delegates fabricated `0.0` for a variance at one observation and for a skew or kurtosis at zero spread, and the kernel answers `NaN` at exactly those points; a reader that treated the zero as a measured spread now reads the undefined the IEEE rail already spells and screens it through `ValidityClaim.Finite`. WITNESS: `held.Fold(Empty, (acc, r) => acc.Push(r)).Variance(MomentNormalizer.Sample)` over a local seed rebuilds as a `ResidualStream` fold whose `Held` is `None` until the first push, so a lane no solve has run reports absence where the `Empty` seed reported a zero-count receipt. The stream still records the running-versus-moving distinction and states its normalizer at every read because unmarked mixing silently corrupts every downstream confidence computation, and one pushed `NaN` permanently poisons every moment so the stream guards at admission through the same all-finite predicate the operands cross — and COUNTS what it turned away, because silently returning the prior state made a producer emitting nothing but sentinels indistinguishable from a lane nobody pushed, and that count merges on every arm including the empty one; the merge identity holds only to the floating-point merge bound.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Bit-faithfulness IS the record's own value equality — the hand-written three-field compare re-implemented the
// compiler `==` and is deleted; `[Equatable]` keeps the compare generated and adds the `Inequalities` diff, so a
// stale cache verdict names WHICH coordinate moved (`ProviderType: MklLinearAlgebraProvider -> Managed…`,
// `Parallelism: 8 -> 4`) instead of a bare false. `Selected` is the row the composition THREADED; the ambient
// handle beside it is the world the snapshot is checking that row against.
[Equatable]
public readonly partial record struct SolveProvenance(string Selected, string ProviderTag, string ProviderType, int Parallelism) {
    public static SolveProvenance Snapshot(LinearProvider provider) =>
        new(provider.Key, LinearAlgebraControl.Provider.ToString() ?? string.Empty, LinearAlgebraControl.Provider.GetType().Name, Control.MaxDegreeOfParallelism);
}

// The residual stream is the kernel `Rasm/Domain/stats#MOMENTS` `Stat<Scalar>` receipt, folded through its own
// `Update`/`Merge` — the same four-moment Welford recurrence and the same Pebay pairwise join this lane once
// carried as a second copy. ABSENCE is `None`: a lane no solve has pushed has no receipt, where an `Empty` seed
// carrying `Count: 0` was the forged-zero shape this page's own comment complained about, indistinguishable
// from a stream that admitted nothing. `Rejected` still counts what the finite guard turned away — the kernel
// column carries it and `Merge` sums it on every arm including the empty one.
public sealed record ResidualStream(Option<Stat<Scalar>> Held) {
    public static readonly ResidualStream Empty = new(None);

    public ResidualStream Push(double residual, Op key) =>
        Held.Match(
            None: () => new ResidualStream(Stat<Scalar>.Of(Seq((Scalar)residual), key).ToOption()),
            Some: prior => new ResidualStream(Stat<Scalar>.Update(prior, (Scalar)residual, key: key).ToOption() | prior));

    public ResidualStream Combine(ResidualStream other, Op key) =>
        (Held, other.Held) switch {
            ({ IsSome: true, Case: Stat<Scalar> a }, { IsSome: true, Case: Stat<Scalar> b }) =>
                new ResidualStream(Stat<Scalar>.Merge(a, b, key).ToOption()),
            ({ IsSome: true }, _) => this,
            _ => other,
        };
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
