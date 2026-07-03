# [RASM_VECTORS_SAMPLE]

The point-sampling owner — ONE `SampleKind` `[Union]` whose twelve cases span the complete distribution suite (explicit and weighted pass-through, Bridson active-list Poisson disk, farthest-point and farthest-point-optimize, Lloyd and capacity-limited Lloyd relaxation, weighted-priority scalar density, adaptive variable-density, Yuksel weighted sample elimination, Dwork variable-density surface Poisson, and the de Goes BNOT continuous power-CCVT) behind ONE `SampleKernel.Sample` dispatch over the `extract.md` `ExtractionDomain` (support / mesh / cloud / candidate). The former 22-parameter `PowerCcvt` constructor is dead: every knob band lands in a grouped policy record family (`CapacityPolicy` / `MotionPolicy` / `ArmijoPolicy` / `RegularityPolicy` composed by `PowerCcvtPolicy`) with a canonical `Default` preset, and the ONE advanced override is a `with`-mutation of that preset — the `Meshing/mesh.md` `TuftedCoverPolicy.Default` pattern applied to the widest knob surface in the corpus. Construction admits once through the case factories; the interior is total over admitted kinds.

The BNOT solver (`PowerCcvtRun`) composes, never re-implements: the restricted Laguerre diagram is `Meshing/mesh.md`'s `RestrictedPowerDiagram` (sole consumer, the coupling preserved by decision); the gauge-fixed ENFORCE-CAPACITY concave Newton solves the density-weighted power-graph Laplacian through `Numerics/matrix.md`'s `SparseMatrix.SingularSolveDetailed` under `GaugePolicy.MeanZeroConstant`/`PinConstant` (the SPSD nullspace is the constant vector — `PowerCcvtGauge` selects the item-owned gauge, never a parallel solve); both convergence loops are `Schedule.recurs`-driven `RepeatWhile` over an `Atom` cell, partitioned `Converged | StoppedWithoutConvergence` as a typed terminal, never budget-exhaustion → `Fin.Fail`. Every stochastic draw is deterministic: order keys and unit intervals derive from `Domain/identity.md`'s one splitmix64 owner (`Deterministic.OrderKey`/`Deterministic.UnitInterval`) — the former private hash twin is dead. The hexagonal-packing reference spacing `√(2·measure/(√3·n))` is computed by ONE `Spacing` fold serving the outer stopping scale, the Lloyd fallback, and the normalized Poisson radius — the three former recomputation sites are collapsed. Blue-noise quality on meshes is witnessed by `Processing/segment.md`'s `SegmentKernel.ValidateSamplingSpectrum`; every receipt (`SampleReceipt`, `SampleAlgorithmReceipt`, `DworkReceipt`, `PowerCellFragmentFacts`, `PowerCcvtReceipt`) spells `IsValid` as ONE `Domain/rails.md` `ValidityClaim.All` fold over claim rows plus the cross-field claims only it can state — a hand-rolled `&&` chain is the deleted form. `Op` stays the explicit value key end to end.

## [01]-[INDEX]

- [02]-[SAMPLING]: `SampleKind` union + factories; the algorithm/stop/status vocabularies; `SampleKernel` domain dispatch (admitted projection, support candidates via `Domain/evaluation.md`'s `SamplePoints` extension, the page-owned `SurfaceCandidatePoints` mesh-boundary lattice, cloud candidates via `Spatial/cloud.md` mass); the classical candidate suite (Bridson, farthest, FPO, Lloyd, capacity, priority density, Yuksel elimination, Dwork annulus); the ONE `Spacing` fold; `SampleReceipt`/`SampleAlgorithmReceipt` evidence.
- [03]-[POWER_CCVT]: the grouped `PowerCcvtPolicy` family; `PowerCcvtGauge` gauge selection over `matrix.md` `GaugePolicy`; `PowerCcvtRun` — capacity Newton (7a) + two-phase site motion (7b) + regularity break + surface lift; `PowerCellFragmentFacts`/`PowerCcvtReceipt` witnesses.

## [02]-[SAMPLING]

- Owner: `SampleKind` `[Union]` — `Explicit(Seq<Point3d>)` · `PoissonDisk(PositiveMagnitude Radius, Dimension Attempts, int Seed)` · `Farthest(Dimension)` · `Optimize(Dimension, Dimension)` · `Lloyd(Dimension, Dimension)` · `Capacity(Dimension Count, Dimension Limit, Dimension Iterations, PositiveMagnitude Tolerance)` · `Weighted(Seq<(Point3d, double)>)` · `ScalarDensity(ScalarField, Dimension)` · `Adaptive(ScalarField, Dimension, PositiveMagnitude MinSpacing)` · `SampleElimination(Dimension Count, Dimension OversampleFactor, PositiveMagnitude Alpha, PositiveMagnitude Beta, PositiveMagnitude Gamma, int Seed)` · `DworkVariableDensity(ScalarField Radius, Dimension Count, PositiveMagnitude MinRadius, Dimension Attempts, int Seed)` · `PowerCcvt(Dimension Count, PowerCcvtPolicy Policy)`; the vocabularies `SampleAlgorithmKind` (11 rows), `SampleStopKind` (`Completed`/`CapacityLimited`/`AllRejected`/`CandidateExhausted`), `SampleDomainStatus` (`Projected`/`CandidateAccepted`/`CandidateRejected`), `DworkSamplingDomain` (`ContinuousMesh`/`CandidateSet`).
- Entry: one factory per case admits raw scalars through `Op.AcceptValidated<Dimension|PositiveMagnitude>` and the case's `Admit` invariants (Yuksel `OversampleFactor > 1 ∧ Beta ≤ 1`; weighted mass through `CloudKernel.MassOf`; density fields non-null); evaluation is `SampleKind.Project<TOut>(ExtractionDomain, Context, Op?)` reached through the `intent.md` rail — `TOut` discriminates `Seq<Point3d>` / `VectorCloud` (weighted cluster when mass survives) / `PointCloud` / `SampleReceipt` through `AtomProjection.Rows` typed rows.
- Auto: `SampleKernel.Sample` discriminates by domain shape — explicit/weighted points PROJECT onto the domain (closest support hit, `ClosestMeshPoint` within `Context.Absolute`, cluster-vertex coincidence); generated kinds on a support space draw candidates through `Domain/evaluation.md`'s geometry-extension `SamplePoints` oversampled by the kind's candidate scale (the algorithm row's default; `Adaptive` carries its own denser case-owned pool), then RUN the candidate suite over them (the retired file's silent projection-passthrough for density kinds is dead; `PoissonDisk` and `PowerCcvt` stay typed `Unsupported` off a support space); mesh domains generate surface candidates through the page-owned `SurfaceCandidatePoints` lattice at the kind's `MeshCandidateDensity` (area-derived, `PoissonDisk` from `radius²`), then run the candidate suite and witness the result through `SegmentKernel.ValidateSamplingSpectrum`; cloud domains sample the cluster's own vertices with `CloudKernel.MassOf` mass carried through. The candidate suite: Bridson active-list Poisson (annulus `[r,2r]`, deterministic parent/candidate draws, maximal-coverage flag = exhausted active list); farthest-point (centroid-seeded min-distance maximization); FPO (worst-coverage swap improvement); Lloyd relaxation (site ← nearest candidate to cell mean); capacity-limited Lloyd (per-site fill ceiling + residual = unassigned fraction); weighted-priority density (exponential-clock `−log(U)/w` ordering + per-sample local radius `minSpacing/√(w/wMax)`); Yuksel weighted elimination (`(1−d/dMax)^α` heap weights, `dMin = dMax·(1−(n/N)^γ)·β`, max-weight removal with neighbor weight decrement); Dwork variable-density (spatial-hash grid at pitch `rMin/√3`, per-parent annulus band ring query, radius field re-sampled at every accepted point — one run over candidate sets, one over the live mesh surface with area-CDF barycentric seeding and tangent-frame annulus proposals re-projected by `ClosestMeshPoint`).
- Receipt: `SampleReceipt` — attempted/emitted/rejected, candidate count, min/mean/max pairwise spacing, density error (`|emitted−target|/target` for density-driven kinds), density accepted/rejected, iterations, `SampleStopKind`, `SampleDomainStatus`, and the nested `SampleAlgorithmReceipt` carrying per-algorithm facts (seed, target, oversample, α/β/γ, radii, eliminations, neighbor updates, coverage/capacity/transport/spectrum validation flags, and the `DworkReceipt`/`PowerCcvtReceipt`/`MeshSamplingSpectrumReceipt` children) — one algorithm-evidence stream, never parallel receipt types per algorithm.
- Packages: `Rasm`/Domain (`Op`/`Context`/`Admit`/`Deterministic.OrderKey`/`Deterministic.UnitInterval`/the `evaluation.md` geometry-extension `SamplePoints`/`ValidityClaim`), `Rasm`/Numerics (`Dimension`/`PositiveMagnitude`/`AtomProjection`/`ProjectionRow`), `Rasm`/Spatial (`VectorCloud`/`CloudKernel.MassOf`/`ScalarField`), `Rasm`/Meshing (`MeshSpace`/`MeshKernel.RestrictedPowerCells` + the `RestrictedPowerDiagram`/`PowerCell`/`PowerFacet` carriers), `Rasm`/Processing (`ExtractionDomain` ingress, `SegmentKernel.ValidateSamplingSpectrum`), LanguageExt.Core (`Fin`/`Seq`/`Arr`/`Option`/`Atom`/`IO`/`Schedule`), Thinktecture.Runtime.Extensions, RhinoCommon (`Point3d`/`Mesh.ClosestMeshPoint`/`AreaMassProperties`/`Plane.FitPlaneToPoints`/`BoundingBox`/`PointCloud` — boundary-admitted).
- Growth: a new sampling algorithm is one `SampleKind` case + one `SampleAlgorithmKind` row + one suite arm (the union `Switch` breaks loudly); a new per-algorithm fact is one `SampleAlgorithmReceipt` field; a new candidate domain is one `ExtractionDomain` case ripening here as one dispatch arm — never a sibling sampler class.
- Boundary: the suite kernels (Bridson conflict scan, Dwork shell walk, Yuksel weight heap, FPO swap loop) are named statement-kernel exemptions — hot spatial loops with typed-receipt egress; every draw threads `Determinism` so identical seeds replay identical distributions; hexagonal reference spacing, stopping scales, and local radii are scale-derived (`Spacing` fold over the domain measure), never absolute literals; a kind whose candidate demand the domain cannot meet terminates `CandidateExhausted` with the shortfall on the receipt — capability is never silently degraded.

## [03]-[POWER_CCVT]

- Owner: `PowerCcvtPolicy(Dimension Iterations, PositiveMagnitude Tolerance, Option<ScalarField> Density, CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity, PowerCcvtGauge Gauge, int Seed)` with nested `CapacityPolicy(ResidualTol, NewtonFloor, MaxNewton)`, `MotionPolicy(LloydSweeps, GradientSteps, LloydPosTol, GradPosTol)`, `ArmijoPolicy(C1, Backtrack, InitialStep, MaxHalvings)` (shared verbatim by the dual-Newton ascent and the site-motion gradient ascent — one line-search policy, two consumers), `RegularityPolicy(AliasScale, JitterVariance, MagnitudeScale, RelocateFraction)`; `PowerCcvtGauge` (`ZeroMean`/`PinIndexZero`) whose `Policy(fragmentMasses)` column mints the `matrix.md` `GaugePolicy` row — the Hessian nullspace fix is the item's own behavior; `PowerCcvtStopKind` (`Converged`/`StoppedWithoutConvergence`).
- Entry: `SampleKind.PowerCcvt(int count, Option<PowerCcvtPolicy> policy = default, Op? key = null)` — the preset is `PowerCcvtPolicy.Default`; the ONE advanced override is `PowerCcvtPolicy.Default with { … }` admitted through `policy.Admit` (backtrack < 1, C1 < 1, relocate ≤ 1, positive budgets); twenty-two loose knobs collapse to one composed value.
- Auto: `PowerCcvtRun` executes de Goes 2012 BNOT on the mesh: fit the canonical plane (`Plane.FitPlaneToPoints`, deviation witnessed as `PlanarityDeviation`), draw `n` density-importance sites (exponential-clock reservoir over the density field, farthest-point fallback for constant density), then iterate — (7a) ENFORCE-CAPACITY: rebuild the `RestrictedPowerDiagram` at live sites/weights, assemble the density-weighted power-graph Laplacian from facet lengths (`w_ij = l_ij/(2|p_i−p_j|)`, symmetric scatter + matched diagonal + scale-derived Tikhonov), solve `L·δ = g` (`g_i = m*−m_i`, `Σg_i = 0`) through `SingularSolveDetailed` under the selected gauge, Armijo-ASCEND the monotone dual objective `Φ = Σᵢ TransportCostᵢ + Σᵢ wᵢ(m*−mᵢ)`, converging on the scale-relative capacity residual; (7b) two-phase site motion: Lloyd sweeps `qᵢ ← bᵢ` (empty Aurenhammer-dominated cells hold their seat) then Armijo-INCREASE transport-energy gradient ascent along `+2mᵢ(bᵢ−qᵢ)` — a sufficient-DECREASE test would stall the concave maximization at iteration one; the joint stop is `displacement ≤ LloydPosTol·meanSpacing ∧ ‖∇q‖ ≤ GradPosTol·meanSpacing` with `meanSpacing` from the ONE `Spacing` fold. Both loops are `Atom` + `IO.lift` + `RepeatWhile(Schedule.recurs(budget))`. Terminal: break lattice regularity ONCE (alias radius `AliasScale·meanSpacing`; Box-Muller jitter and relocation offsets deterministic in seed via `Deterministic.UnitInterval`), lift sites to the surface by `ClosestMeshPoint` (a missed projection holds the in-plane site), and populate the receipt.
- Receipt: `PowerCellFragmentFacts` (site/fragment/facet/empty-cell counts, total/min/max mass, integration residual) + `PowerCcvtReceipt` (target mass, ∞/L1/L2/normalized capacity residuals, outer/Lloyd/gradient/Newton iteration counts, weight extrema and mean, transport energy + delta, dual objective, centroid shift, position/weight gradient norms, halvings/rebuilds/aliased/jittered/relocated counts, normalized Poisson radius, planarity deviation, gauge, stop, fragment facts, the composed `SolveReceipt` dual-solve witness, and the `MeshSamplingSpectrumReceipt` surfaced INTO the receipt — one fact stream, no parallel spectrum field); `MeanZeroGaugeApplied` cross-checks the gauge against the solve's `GaugeShift` evidence.
- Growth: a new gauge is one `PowerCcvtGauge` row minting its `GaugePolicy`; a new motion schedule or line-search variant is one policy record field on the SAME run; a density-transport variant (Lloyd energy vs. capacity weighting) is a `MotionPolicy` column — never a second solver class.
- Boundary: the per-iteration diagram rebuild, triplet assembly, and Armijo searches are the named statement-kernel exemption; the OUTER schedules are domain flow; `d_ij` offsets are recomputed from live dual weights inside the diagram build, never trusted from stale facets; the transport energy reads the site-anchored cell integrals directly — a parallel-axis correction would double-count; the solver never reuses the discrete Sinkhorn/capacity-Lloyd machinery (continuous OT is a different estimator) and the diagram build never re-enters this page (the `mesh.md` coupling is one-directional).

```csharp contract
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Vectors;

// --- [TYPES] ----------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DworkSamplingDomain {
    public static readonly DworkSamplingDomain ContinuousMesh = new(key: 0);
    public static readonly DworkSamplingDomain CandidateSet = new(key: 1);
}

// CandidateScale: candidate-pool oversampling multiplier (support draws and mesh lattices); DensityDriven:
// emits a density-error witness. Selection kinds NEED a strict oversample — a pool equal to the request
// degenerates farthest selection to the identity and fixes Lloyd at iteration zero.
[SmartEnum<int>]
public sealed partial class SampleAlgorithmKind {
    public static readonly SampleAlgorithmKind Explicit = new(key: 0, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind BridsonActiveListPoisson = new(key: 1, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind FarthestCandidate = new(key: 2, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind FarthestOptimize = new(key: 3, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind LloydCandidateRelaxation = new(key: 4, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind CapacityLimitedLloydCandidate = new(key: 5, candidateScale: 1.0, densityDriven: false);
    public static readonly SampleAlgorithmKind WeightedMassPropagation = new(key: 6, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind VariableDensityPoisson = new(key: 7, candidateScale: 8.0, densityDriven: true);
    public static readonly SampleAlgorithmKind YukselWeightedSampleElimination = new(key: 8, candidateScale: 1.0, densityDriven: false);
    public static readonly SampleAlgorithmKind DworkVariableDensity = new(key: 9, candidateScale: 12.0, densityDriven: true);
    public static readonly SampleAlgorithmKind ContinuousPowerCcvt = new(key: 10, candidateScale: 8.0, densityDriven: false);

    public double CandidateScale { get; }
    public bool DensityDriven { get; }
}

[SmartEnum<int>]
public sealed partial class SampleDomainStatus {
    public static readonly SampleDomainStatus Projected = new(key: 0);
    public static readonly SampleDomainStatus CandidateAccepted = new(key: 1);
    public static readonly SampleDomainStatus CandidateRejected = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SampleStopKind {
    public static readonly SampleStopKind Completed = new(key: 0);
    public static readonly SampleStopKind CapacityLimited = new(key: 1);
    public static readonly SampleStopKind AllRejected = new(key: 2);
    public static readonly SampleStopKind CandidateExhausted = new(key: 3);
}

// BNOT dual-weight gauge selector. The ENFORCE-CAPACITY Hessian is the density-weighted power-graph
// Laplacian whose nullspace is the constant vector; the row mints the matrix.md GaugePolicy — never a parallel solve.
[SmartEnum<int>]
public sealed partial class PowerCcvtGauge {
    public static readonly PowerCcvtGauge ZeroMean = new(key: 0);
    public static readonly PowerCcvtGauge PinIndexZero = new(key: 1);
    internal GaugePolicy Policy(Arr<double> fragmentMasses) => Switch(
        state: fragmentMasses,
        zeroMean: static mass => GaugePolicy.MeanZeroConstant(dimension: mass.Count, mass: Some(mass), shift: GaugeShift.MeanZero),
        pinIndexZero: static mass => GaugePolicy.PinConstant(index: 0, mass: Some(mass), shift: GaugeShift.MeanZero));
}

[SmartEnum<int>]
public sealed partial class PowerCcvtStopKind {
    public static readonly PowerCcvtStopKind Converged = new(key: 0);
    public static readonly PowerCcvtStopKind StoppedWithoutConvergence = new(key: 1);
}

[Union]
public abstract partial record SampleKind {
    public sealed record ExplicitCase(Seq<Point3d> Points) : SampleKind;
    public sealed record PoissonDiskCase(PositiveMagnitude Radius, Dimension Attempts, int Seed) : SampleKind;
    public sealed record FarthestCase(Dimension Count) : SampleKind;
    public sealed record OptimizeCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record CapacityCase(Dimension Count, Dimension Limit, Dimension Iterations, PositiveMagnitude Tolerance) : SampleKind;
    public sealed record WeightedCase(Seq<(Point3d Point, double Mass)> Points) : SampleKind;
    public sealed record ScalarDensityCase(ScalarField Density, Dimension Count) : SampleKind;
    public sealed record AdaptiveCase(ScalarField Density, Dimension Count, PositiveMagnitude MinSpacing) : SampleKind;
    public sealed record SampleEliminationCase(Dimension Count, Dimension OversampleFactor, PositiveMagnitude Alpha, PositiveMagnitude Beta, PositiveMagnitude Gamma, int Seed) : SampleKind;
    public sealed record DworkVariableDensityCase(ScalarField Radius, Dimension Count, PositiveMagnitude MinRadius, Dimension Attempts, int Seed) : SampleKind;
    public sealed record PowerCcvtCase(Dimension Count, PowerCcvtPolicy Policy) : SampleKind;
    private SampleKind() { }

    public static Fin<SampleKind> Explicit(Seq<Point3d> points, Op? key = null) =>
        new ExplicitCase(Points: points).Admit(key: key.OrDefault());
    public static Fin<SampleKind> PoissonDisk(double radius, int attempts = 30, int seed = 0, Op? key = null) {
        Op op = key.OrDefault();
        return from r in op.AcceptValidated<PositiveMagnitude>(candidate: radius)
               from a in op.AcceptValidated<Dimension>(candidate: attempts)
               from admitted in new PoissonDiskCase(Radius: r, Attempts: a, Seed: seed).Admit(key: op)
               select admitted;
    }
    public static Fin<SampleKind> Farthest(int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(value => new FarthestCase(Count: value).Admit(key: op));
    }
    public static Fin<SampleKind> Optimize(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (c, i) => new OptimizeCase(Count: c, Iterations: i), key: key);
    public static Fin<SampleKind> Lloyd(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (c, i) => new LloydCase(Count: c, Iterations: i), key: key);
    public static Fin<SampleKind> Capacity(int count, int capacity, int iterations = 8, double tolerance = 1.0e-6, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from limit in op.AcceptValidated<Dimension>(candidate: capacity)
               from iter in op.AcceptValidated<Dimension>(candidate: iterations)
               from tol in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from admitted in new CapacityCase(Count: c, Limit: limit, Iterations: iter, Tolerance: tol).Admit(key: op)
               select admitted;
    }
    public static Fin<SampleKind> Weighted(Seq<(Point3d Point, double Mass)> points, Op? key = null) =>
        new WeightedCase(Points: points).Admit(key: key.OrDefault());
    public static Fin<SampleKind> ScalarDensity(ScalarField density, int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(c => new ScalarDensityCase(Density: density, Count: c).Admit(key: op));
    }
    public static Fin<SampleKind> Adaptive(ScalarField density, int count, double minSpacing, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from spacing in op.AcceptValidated<PositiveMagnitude>(candidate: minSpacing)
               from admitted in new AdaptiveCase(Density: density, Count: c, MinSpacing: spacing).Admit(key: op)
               select admitted;
    }
    public static Fin<SampleKind> SampleElimination(int count, int oversampleFactor, double alpha, double beta, double gamma, int seed, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from oversample in op.AcceptValidated<Dimension>(candidate: oversampleFactor)
               from a in op.AcceptValidated<PositiveMagnitude>(candidate: alpha)
               from b in op.AcceptValidated<PositiveMagnitude>(candidate: beta)
               from g in op.AcceptValidated<PositiveMagnitude>(candidate: gamma)
               from admitted in new SampleEliminationCase(Count: c, OversampleFactor: oversample, Alpha: a, Beta: b, Gamma: g, Seed: seed).Admit(key: op)
               select admitted;
    }
    public static Fin<SampleKind> DworkVariableDensity(ScalarField radius, int count, double minRadius, int attempts = 30, int seed = 0, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from min in op.AcceptValidated<PositiveMagnitude>(candidate: minRadius)
               from a in op.AcceptValidated<Dimension>(candidate: attempts)
               from admitted in new DworkVariableDensityCase(Radius: radius, Count: c, MinRadius: min, Attempts: a, Seed: seed).Admit(key: op)
               select admitted;
    }
    // Preset + one advanced override: PowerCcvtPolicy.Default `with { … }` is the whole tuning surface.
    public static Fin<SampleKind> PowerCcvt(int count, Option<PowerCcvtPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from active in policy.IfNone(PowerCcvtPolicy.Default).Admit(key: op)
               from admitted in new PowerCcvtCase(Count: c, Policy: active).Admit(key: op)
               select admitted;
    }

    // Generated total Switch: a new case breaks this member at compile time — the loud-break growth law.
    // Op.Need is the null gate here: the Admit member name shadows the Rasm.Domain.Admit class inside this type.
    internal Fin<SampleKind> Admit(Op key) => Switch(
        state: key,
        explicitCase: static (op, c) => c.Points.IsEmpty ? Fin.Fail<SampleKind>(op.InvalidInput()) : Fin.Succ<SampleKind>(c),
        poissonDiskCase: static (_, c) => Fin.Succ<SampleKind>(c),
        farthestCase: static (_, c) => Fin.Succ<SampleKind>(c),
        optimizeCase: static (_, c) => Fin.Succ<SampleKind>(c),
        lloydCase: static (_, c) => Fin.Succ<SampleKind>(c),
        capacityCase: static (_, c) => Fin.Succ<SampleKind>(c),
        weightedCase: static (op, c) => c.Points.IsEmpty
            ? Fin.Fail<SampleKind>(op.InvalidInput())
            : CloudKernel.MassOf(mass: new Arr<double>([.. c.Points.AsIterable().Select(static item => item.Mass)]), count: c.Points.Count, key: op).Map(_ => (SampleKind)c),
        scalarDensityCase: static (op, c) => op.Need(c.Density).Map(_ => (SampleKind)c),
        adaptiveCase: static (op, c) => op.Need(c.Density).Map(_ => (SampleKind)c),
        sampleEliminationCase: static (op, c) => guard(c.OversampleFactor.Value > 1 && c.Beta.Value <= 1.0, op.InvalidInput()).ToFin().Map(_ => (SampleKind)c),
        dworkVariableDensityCase: static (op, c) => op.Need(c.Radius).Map(_ => (SampleKind)c),
        powerCcvtCase: static (op, c) => c.Policy.Admit(key: op).Map(_ => (SampleKind)c));
    internal static Fin<SampleKind> Admit(SampleKind value, Op key) =>
        key.Need(value).Bind(kind => kind.Admit(key: key));

    internal Fin<SampleResult> Evaluate(ExtractionDomain domain, Context context, Op key) =>
        Admit(key: key).Bind(kind => SampleKernel.Sample(kind: kind, domain: domain, context: context, key: key));
    // Adaptive's per-sample local radius rejects harder than plain density draws; its pool is case-owned,
    // never borrowed from another algorithm row.
    private const double AdaptiveCandidateScale = 12.0;
    // One derived projection: per-case target/iteration/scale facts read off the payload + the algorithm row.
    // Generated total Switch — a new case cannot silently mis-report; it breaks here at compile time.
    internal (Option<int> Count, Option<int> Iterations, double CandidateScale, SampleAlgorithmKind Algorithm) Facts => Switch(
        explicitCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.Explicit),
        poissonDiskCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.BridsonActiveListPoisson),
        farthestCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.FarthestCandidate.CandidateScale, SampleAlgorithmKind.FarthestCandidate),
        optimizeCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), SampleAlgorithmKind.FarthestOptimize.CandidateScale, SampleAlgorithmKind.FarthestOptimize),
        lloydCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), SampleAlgorithmKind.LloydCandidateRelaxation.CandidateScale, SampleAlgorithmKind.LloydCandidateRelaxation),
        capacityCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value, SampleAlgorithmKind.CapacityLimitedLloydCandidate),
        weightedCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.WeightedMassPropagation),
        scalarDensityCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.VariableDensityPoisson.CandidateScale, SampleAlgorithmKind.VariableDensityPoisson),
        adaptiveCase: static c => (Some(c.Count.Value), Option<int>.None, AdaptiveCandidateScale, SampleAlgorithmKind.VariableDensityPoisson),
        sampleEliminationCase: static c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value, SampleAlgorithmKind.YukselWeightedSampleElimination),
        dworkVariableDensityCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.DworkVariableDensity.CandidateScale, SampleAlgorithmKind.DworkVariableDensity),
        powerCcvtCase: static c => (Some(c.Count.Value), Some(c.Policy.Iterations.Value), SampleAlgorithmKind.ContinuousPowerCcvt.CandidateScale, SampleAlgorithmKind.ContinuousPowerCcvt));
    internal Option<double> DensityError(int emitted) =>
        Facts is { Algorithm.DensityDriven: true, Count: Option<int> count }
            ? count.Map(value => Math.Abs(value: emitted - value) / Math.Max(1.0, value))
            : Option<double>.None;
    // Explicit/weighted kinds PROJECT and never reach this lattice density; only generated kinds dispatch here.
    internal Fin<double> MeshCandidateDensity(double area, Op key) {
        double safeArea = Math.Max(val1: area, val2: double.Epsilon);
        double target = this switch {
            PoissonDiskCase pd => safeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: double.Epsilon),
            _ => Facts.Count.Map(value => value * Math.Max(1.0, Facts.CandidateScale)).IfNone(0.0),
        };
        return double.IsFinite(target) && target > 0.0
            ? key.AcceptValue(value: Math.Max(val1: target / safeArea, val2: 1.0 / safeArea))
            : Fin.Fail<double>(key.Unsupported(geometryType: GetType(), outputType: typeof(SampleResult)));
    }
    internal Fin<TOut> Project<TOut>(ExtractionDomain domain, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from result in Evaluate(domain: domain, context: context, key: op)
               from output in AtomProjection.Rows<SampleReceipt, TOut>(self: result.Receipt, key: op, owner: typeof(SampleKind),
                   ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(result.Points)),
                   ProjectionRow.Of<VectorCloud>(() => result.Mass.Match(
                       Some: mass => VectorCloud.Cluster(points: result.Points, context: context, mass: Some(toSeq(mass.AsIterable())), key: op),
                       None: () => VectorCloud.Cluster(points: result.Points, context: context, key: op))),
                   ProjectionRow.Of<PointCloud>(() => VectorCloud.Cluster(points: result.Points, context: context, key: op)
                       .Bind(cloud => cloud is VectorCloud.ClusterCase cluster ? Fin.Succ(cluster.Indexed) : Fin.Fail<PointCloud>(op.InvalidResult()))))
               select output;
    }
    private static Fin<SampleKind> Counted(int count, int value, Func<Dimension, Dimension, SampleKind> create, Op? key) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from v in op.AcceptValidated<Dimension>(candidate: value)
               from admitted in create(c, v).Admit(key: op)
               select admitted;
    }
}

// --- [CONSTANTS] ------------------------------------------------------------------------------
public sealed record CapacityPolicy(PositiveMagnitude ResidualTol, PositiveMagnitude NewtonFloor, Dimension MaxNewton);
public sealed record MotionPolicy(Dimension LloydSweeps, Dimension GradientSteps, PositiveMagnitude LloydPosTol, PositiveMagnitude GradPosTol);
public sealed record ArmijoPolicy(PositiveMagnitude C1, PositiveMagnitude Backtrack, PositiveMagnitude InitialStep, Dimension MaxHalvings);
public sealed record RegularityPolicy(PositiveMagnitude AliasScale, PositiveMagnitude JitterVariance, PositiveMagnitude MagnitudeScale, PositiveMagnitude RelocateFraction);

// The grouped BNOT tuning surface; Default is canonical, `Default with { … }` is the one advanced override.
public sealed record PowerCcvtPolicy(
    Dimension Iterations, PositiveMagnitude Tolerance, Option<ScalarField> Density,
    CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity,
    PowerCcvtGauge Gauge, int Seed) {
    public static readonly PowerCcvtPolicy Default = new(
        Iterations: Dimension.Create(value: 16), Tolerance: PositiveMagnitude.Create(value: 1.0e-6), Density: Option<ScalarField>.None,
        Capacity: new CapacityPolicy(ResidualTol: PositiveMagnitude.Create(value: 0.01), NewtonFloor: PositiveMagnitude.Create(value: 1.0e-6), MaxNewton: Dimension.Create(value: 32)),
        Motion: new MotionPolicy(LloydSweeps: Dimension.Create(value: 1), GradientSteps: Dimension.Create(value: 8), LloydPosTol: PositiveMagnitude.Create(value: 0.01), GradPosTol: PositiveMagnitude.Create(value: 0.1)),
        Search: new ArmijoPolicy(C1: PositiveMagnitude.Create(value: 1.0e-4), Backtrack: PositiveMagnitude.Create(value: 0.5), InitialStep: PositiveMagnitude.Create(value: 1.0), MaxHalvings: Dimension.Create(value: 32)),
        Regularity: new RegularityPolicy(AliasScale: PositiveMagnitude.Create(value: 0.65), JitterVariance: PositiveMagnitude.Create(value: 0.05), MagnitudeScale: PositiveMagnitude.Create(value: 0.5), RelocateFraction: PositiveMagnitude.Create(value: 0.05)),
        Gauge: PowerCcvtGauge.ZeroMean, Seed: 0);
    internal Fin<PowerCcvtPolicy> Admit(Op key) =>
        guard(Search.Backtrack.Value < 1.0 && Search.C1.Value < 1.0 && Regularity.RelocateFraction.Value <= 1.0
              && Gauge is not null && Density.Map(static field => field is not null).IfNone(noneValue: true), key.InvalidInput())
            .ToFin().Map(_ => this);
}

// --- [MODELS] ---------------------------------------------------------------------------------
// Every receipt's IsValid is ONE ValidityClaim.All fold (Domain/rails.md); the cross-field claims are
// the rows only this receipt can state. A hand-rolled && chain is the deleted form.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DworkReceipt(
    DworkSamplingDomain Domain, double RMin, Option<double> BackgroundCellSize, Option<int> BackgroundGridCells,
    int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain,
    double LocalRadiusMin, double LocalRadiusMax) : IValidityEvidence {
    public bool CandidateOnly => Domain.Equals(DworkSamplingDomain.CandidateSet);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(RMin),
        ValidityClaim.Of(BackgroundCellSize.Map(static size => double.IsFinite(size) && size > 0.0).IfNone(noneValue: true)),
        ValidityClaim.Of(BackgroundGridCells.Map(static cells => cells >= 0).IfNone(noneValue: true)),
        ValidityClaim.CountAtLeast(AttemptsPerActive, 1), ValidityClaim.CountAtLeast(GeneratedCandidates, 0),
        ValidityClaim.CountAtLeast(ActivePops, 0), ValidityClaim.CountAtLeast(RejectedTooClose, 0),
        ValidityClaim.CountAtLeast(RejectedDomain, 0), ValidityClaim.Ordered(LocalRadiusMin, LocalRadiusMax));
}

// Restricted-power-cell fragment evidence the BNOT outer schedule rebuilds each iteration.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCellFragmentFacts(
    int SiteCount, int FragmentCount, int FacetCount, int EmptyCellCount,
    double TotalMass, double MassMin, double MassMax, double IntegrationResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.CountAtLeast(FragmentCount, 0),
        ValidityClaim.CountAtLeast(FacetCount, 0), ValidityClaim.CountAtLeast(EmptyCellCount, 0),
        ValidityClaim.Of(EmptyCellCount <= SiteCount), ValidityClaim.Nonnegative(TotalMass),
        ValidityClaim.Ordered(MassMin, MassMax), ValidityClaim.Nonnegative(IntegrationResidual));
}

// BNOT witness: the capacity Newton populates the dual/weight/residual fields; the two-phase outer loop
// populates motion/rebuild/regularity fields; the spectrum child is surfaced INTO this receipt (one stream).
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCcvtReceipt(
    int SiteCount, double TargetMass, double ActualMassMin, double ActualMassMax,
    double CapacityResidualInf, double CapacityResidualL1, double CapacityResidualL2, double CapacityResidualNormalized,
    int OuterIterations, int LloydIterations, int GradientIterations, int DualNewtonIterations,
    double WeightMin, double WeightMax, double WeightMean, double TransportEnergy, double TransportEnergyDelta,
    double DualObjective, double CentroidShift, double PositionGradientNorm, double WeightGradientNorm,
    int EmptyCellCount, int StepHalvingCount, int RebuildCount, int AliasedSiteCount, int JitteredSiteCount, int RelocatedSiteCount,
    double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
    PowerCellFragmentFacts Fragments, Option<SolveReceipt> DualSolve = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default) : IValidityEvidence {
    public bool MeanZeroGaugeApplied =>
        Gauge.Equals(PowerCcvtGauge.ZeroMean)
        && DualSolve.Bind(static solve => solve.Gauge).Map(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero)).IfNone(noneValue: false);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.Positive(TargetMass),
        ValidityClaim.Ordered(ActualMassMin, ActualMassMax),
        ValidityClaim.Nonnegative(CapacityResidualInf), ValidityClaim.Nonnegative(CapacityResidualL1),
        ValidityClaim.Nonnegative(CapacityResidualL2), ValidityClaim.Nonnegative(CapacityResidualNormalized),
        ValidityClaim.CountAtLeast(OuterIterations, 0), ValidityClaim.CountAtLeast(LloydIterations, 0),
        ValidityClaim.CountAtLeast(GradientIterations, 0), ValidityClaim.CountAtLeast(DualNewtonIterations, 0),
        ValidityClaim.Ordered(WeightMin, WeightMax), ValidityClaim.Finite(WeightMean),
        ValidityClaim.Finite(TransportEnergy), ValidityClaim.Finite(TransportEnergyDelta), ValidityClaim.Finite(DualObjective),
        ValidityClaim.Nonnegative(CentroidShift), ValidityClaim.Nonnegative(PositionGradientNorm), ValidityClaim.Nonnegative(WeightGradientNorm),
        ValidityClaim.Of(EmptyCellCount >= 0 && EmptyCellCount <= SiteCount),
        ValidityClaim.CountAtLeast(StepHalvingCount, 0), ValidityClaim.CountAtLeast(RebuildCount, 0),
        ValidityClaim.CountAtLeast(AliasedSiteCount, 0), ValidityClaim.CountAtLeast(JitteredSiteCount, 0), ValidityClaim.CountAtLeast(RelocatedSiteCount, 0),
        ValidityClaim.UnitInterval(NormalizedPoissonRadius), ValidityClaim.Nonnegative(PlanarityDeviation),
        ValidityClaim.Of(Fragments.SiteCount == SiteCount), ValidityClaim.Evidence(Fragments),
        ValidityClaim.Of(DualSolve.Map(static solve => solve.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(Spectrum.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

// Every non-Kind slot defaults so call sites name only the facts their algorithm states — the former
// 30-parameter forwarding helper is the deleted thin-indirection form.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleAlgorithmReceipt(
    SampleAlgorithmKind Kind, Option<int> Seed = default, Option<int> TargetCount = default, Option<int> OversampleCount = default, Option<int> OversampleFactor = default,
    Option<double> Alpha = default, Option<double> Beta = default, Option<double> Gamma = default, Option<double> Radius = default, Option<double> WeightLimitRadius = default,
    Option<int> Eliminated = default, Option<int> NeighborUpdates = default, bool MaximalCoverageGuaranteed = false, bool CapacityResidualValidated = false,
    bool TransportAssignmentValidated = false, bool MeshSpectrumValidated = false,
    Option<int> Attempts = default, Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default,
    Option<double> DensityMin = default, Option<double> DensityMax = default, Option<double> LocalRadiusMin = default, Option<double> LocalRadiusMax = default,
    Option<double> CapacityResidual = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default, Option<DworkReceipt> Dwork = default,
    Option<int> CapacityAssignedCandidates = default, Option<int> CapacityUnassignedCandidates = default, Option<PowerCcvtReceipt> PowerCcvt = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Kind is not null),
        ValidityClaim.Of(TargetCount.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(OversampleCount.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Attempts.Map(static count => count >= 1).IfNone(noneValue: true)),
        ValidityClaim.Of(ActivePops.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Eliminated.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(NeighborUpdates.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(RejectedTooClose.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(RejectedDomain.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Radius.Map(static radius => double.IsFinite(radius) && radius > 0.0).IfNone(noneValue: true)),
        ValidityClaim.Of(WeightLimitRadius.Map(static radius => double.IsFinite(radius) && radius >= 0.0).IfNone(noneValue: true)),
        ValidityClaim.Of(CapacityResidual.Map(static residual => double.IsFinite(residual) && residual >= 0.0).IfNone(noneValue: true)),
        ValidityClaim.Of(DensityMin.Bind(min => DensityMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true)),
        ValidityClaim.Of(LocalRadiusMin.Bind(min => LocalRadiusMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true)),
        ValidityClaim.Of(Dwork.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(Spectrum.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(PowerCcvt.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleReceipt(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount,
    Option<double> MinSpacing, Option<double> MeanSpacing, Option<double> MaxSpacing,
    Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations,
    SampleStopKind Stop, SampleDomainStatus DomainStatus, Option<SampleAlgorithmReceipt> Algorithm) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0), ValidityClaim.CountAtLeast(Rejected, 0),
        ValidityClaim.Of(Emitted <= Attempted + Rejected),
        ValidityClaim.Of(MinSpacing.Map(double.IsFinite).IfNone(noneValue: true)),
        ValidityClaim.Of(DensityError.Map(static error => double.IsFinite(error) && error >= 0.0).IfNone(noneValue: true)),
        ValidityClaim.Of(Algorithm.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------------
internal readonly record struct SampleCandidate(Point3d Point, Option<double> Mass);
internal readonly record struct SampleResult(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleReceipt Receipt);
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithmReceipt> Algorithm);

// THE one spacing owner: hexagonal-packing reference, mean-nearest-neighbour scale, and normalized
// Poisson radius all read Hexagonal — the three former recomputation sites are dead.
internal static class Spacing {
    internal static double Hexagonal(double measure, int count) =>
        Math.Sqrt(d: 2.0 * measure / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: count)));
    internal static double MeanNearest(Seq<Point3d> points, double measure) {
        if (points.Count < 2) return Hexagonal(measure: measure, count: points.Count);
        (double accum, int counted) = Enumerable.Range(start: 0, count: points.Count).Aggregate(
            seed: (Accum: 0.0, Counted: 0),
            func: (state, i) => Enumerable.Range(start: 0, count: points.Count)
                .Where(j => j != i)
                .Min(j => points[index: i].DistanceTo(other: points[index: j])) switch {
                    double nearest when double.IsFinite(nearest) => (state.Accum + nearest, state.Counted + 1),
                    _ => state,
                });
        return counted > 0 ? accum / counted : Hexagonal(measure: measure, count: points.Count);
    }
    internal static double NormalizedPoissonRadius(Seq<Point3d> points, double measure) {
        if (points.Count < 2) return 0.0;
        double minSpacing = Enumerable.Range(start: 0, count: points.Count - 1)
            .SelectMany(i => Enumerable.Range(start: i + 1, count: points.Count - i - 1), (i, j) => points[index: i].DistanceTo(other: points[index: j]))
            .Min();
        double reference = Hexagonal(measure: measure, count: points.Count);
        return double.IsFinite(minSpacing) && double.IsFinite(reference) && reference > double.Epsilon
            ? Math.Clamp(value: minSpacing / reference, min: 0.0, max: 1.0)
            : 0.0;
    }
}

internal static class SampleKernel {
    internal static Fin<SampleResult> Sample(SampleKind kind, ExtractionDomain domain, Context context, Op key) =>
        kind switch {
            SampleKind.ExplicitCase explicitCase => SampleAdmitted(points: explicitCase.Points.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), domain: domain, algorithm: SampleAlgorithmKind.Explicit, context: context, key: key),
            SampleKind.WeightedCase weightedCase => SampleAdmitted(points: weightedCase.Points.Map(static item => new SampleCandidate(Point: item.Point, Mass: Some(item.Mass))), domain: domain, algorithm: SampleAlgorithmKind.WeightedMassPropagation, context: context, key: key),
            _ => domain.Switch(
                state: (Kind: kind, Context: context, Key: key),
                supportCase: static (state, d) => SampleGeneratedSupport(kind: state.Kind, space: d.Value, context: state.Context, key: state.Key),
                meshCase: static (state, d) => SampleOnMesh(kind: state.Kind, domain: d.Value, context: state.Context, key: state.Key),
                cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                    ? CloudKernel.MassOf(cluster: cluster, key: state.Key).Bind(mass => SampleOnCandidates(
                        kind: state.Kind,
                        candidates: cluster.Vertices.Map((point, index) => new SampleCandidate(Point: point, Mass: Some(mass[index: index]))),
                        admitsPoisson: false, domainMeasure: Option<(int Dimensions, double Measure)>.None, context: state.Context, key: state.Key))
                    : Fin.Fail<SampleResult>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(SampleResult)))),
        };

    // Explicit/weighted points PROJECT onto the domain: support closest hit, mesh closest point within
    // Context.Absolute, or cluster-vertex coincidence — never accepted raw.
    private static Fin<SampleResult> SampleAdmitted(Seq<SampleCandidate> points, ExtractionDomain domain, SampleAlgorithmKind algorithm, Context context, Op key) =>
        from admitted in points.Fold(
            initialState: Fin.Succ((Accepted: (Seq<Point3d>)[], Mass: (Seq<double>)[], Weighted: false, Rejected: 0)),
            f: (state, item) => state.Bind(current =>
                AdmitPoint(point: item.Point, domain: domain, context: context, key: key).Match(
                    Succ: accepted => item.Mass.Match(
                        Some: mass => Fin.Succ((current.Accepted.Add(accepted), current.Mass.Add(mass), true, current.Rejected)),
                        None: () => Fin.Succ((current.Accepted.Add(accepted), current.Mass, current.Weighted, current.Rejected))),
                    Fail: _ => Fin.Succ((current.Accepted, current.Mass, current.Weighted, current.Rejected + 1)))))
        from mass in admitted.Weighted && !admitted.Accepted.IsEmpty
            ? NormalizeMass(mass: admitted.Mass, key: key).Map(Some)
            : Fin.Succ(Option<Arr<double>>.None)
        select new SampleResult(
            Points: admitted.Accepted, Mass: mass,
            Receipt: ReceiptOf(attempted: points.Count, emitted: admitted.Accepted, rejected: admitted.Rejected, candidates: Some(points.Count),
                iterations: Option<int>.None, stop: admitted.Accepted.IsEmpty ? SampleStopKind.AllRejected : SampleStopKind.Completed,
                status: SampleDomainStatus.Projected, densityError: Option<double>.None, algorithm: Some(new SampleAlgorithmReceipt(Kind: algorithm))));
    private static Fin<Point3d> AdmitPoint(Point3d point, ExtractionDomain domain, Context context, Op key) =>
        key.AcceptValue(value: point).Bind(valid => domain.Switch(
            state: (Point: valid, Context: context, Key: key),
            supportCase: static (state, d) => d.Value.Closest(sample: state.Point, key: state.Key).Bind(hit => state.Key.AcceptValue(value: hit.Point)),
            meshCase: static (state, d) => Optional(d.Value.Native.ClosestMeshPoint(testPoint: state.Point, maximumDistance: state.Context.Absolute.Value))
                .ToFin(state.Key.InvalidResult()).Bind(meshPoint => state.Key.AcceptValue(value: meshPoint.Point)),
            cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                ? cluster.Vertices.Find(vertex => vertex.DistanceToSquared(other: state.Point) <= state.Context.Absolute.Value * state.Context.Absolute.Value)
                    .ToFin(state.Key.InvalidInput())
                : Fin.Fail<Point3d>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(Point3d)))));

    // Generated kinds on a support space: candidates via the Domain/evaluation.md sampler at CandidateScale.
    private static Fin<SampleResult> SampleGeneratedSupport(SampleKind kind, SupportSpace space, Context context, Op key) =>
        kind.Facts.Count.ToFin(Fail: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))).Bind(count =>
            from points in space.Value.SamplePoints(count: (int)Math.Ceiling(a: count * Math.Max(1.0, kind.Facts.CandidateScale)), context: context, key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: points.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: false, domainMeasure: Option<(int Dimensions, double Measure)>.None, context: context, key: key)
            select sampled);
    private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        if (kind is SampleKind.PowerCcvtCase power) return PowerCcvtMeshSolve(domain: domain, kind: power, context: context, key: key);
        if (kind is SampleKind.DworkVariableDensityCase dwork)
            return from selection in DworkMeshRun.Execute(domain: domain, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts.Value, seed: dwork.Seed, context: context, key: key)
                   let points = toSeq(selection.Points)
                   let receipt = selection.Algorithm.Bind(static algorithm => algorithm.Dwork)
                   let rejected = receipt.Map(static value => value.RejectedTooClose + value.RejectedDomain).IfNone(0)
                   let result = new SampleResult(Points: points, Mass: selection.Mass,
                       Receipt: ReceiptOf(attempted: receipt.Map(static value => value.GeneratedCandidates).IfNone(points.Count + rejected), emitted: points, rejected: rejected,
                           candidates: Option<int>.None, iterations: Option<int>.None,
                           stop: points.Count <= 0 ? SampleStopKind.AllRejected : points.Count < dwork.Count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                           status: rejected > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                           densityError: kind.DensityError(emitted: points.Count), algorithm: selection.Algorithm))
                   from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: result, key: key)
                   select validated;
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).ToFin(key.InvalidResult()).Bind(p =>
            from density in kind.MeshCandidateDensity(area: p.Area, key: key)
            from candidates in SurfaceCandidatePoints(space: domain, density: density, key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: true, domainMeasure: Some((Dimensions: 2, Measure: p.Area)), context: context, key: key)
            from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: sampled, key: key)
            select validated);
    }

    // Mesh-boundary candidate lattice: per-triangle interior barycentric grid at area-proportional density.
    // Sole consumer is this kernel; a second mesh candidate generator is the deleted parallel-rail form.
    private static Fin<Seq<Point3d>> SurfaceCandidatePoints(MeshSpace space, double density, Op key) {
        if (!double.IsFinite(density) || density <= 0.0) return Fin.Fail<Seq<Point3d>>(key.InvalidInput());
        List<Point3d> samples = [];
        using Mesh triangulated = space.Native.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<Seq<Point3d>>(key.InvalidResult());
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A]; Point3d b = triangulated.Vertices[index: face.B]; Point3d c = triangulated.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: area * density));
            int side = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Sqrt(d: count * 2.0)));
            int emitted = 0;
            for (int i = 0; i <= side && emitted < count; i++) {
                for (int j = 0; j <= side - i && emitted < count; j++) {
                    double wa = (i + 1.0) / (side + 3.0); double wb = (j + 1.0) / (side + 3.0); double wc = 1.0 - wa - wb;
                    samples.Add(item: new Point3d(x: (wa * a.X) + (wb * b.X) + (wc * c.X), y: (wa * a.Y) + (wb * b.Y) + (wc * c.Y), z: (wa * a.Z) + (wb * b.Z) + (wc * c.Z)));
                    emitted++;
                }
            }
        }
        return samples.Count > 0 && samples.TrueForAll(static point => point.IsValid)
            ? Fin.Succ(toSeq(samples))
            : Fin.Fail<Seq<Point3d>>(key.InvalidResult());
    }

    // de Goes BNOT: canonical plane fit + density-importance site draw feed the PowerCcvtRun two-phase solver.
    private static Fin<SampleResult> PowerCcvtMeshSolve(MeshSpace domain, SampleKind.PowerCcvtCase kind, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).Map(static p => p.Area).Filter(static area => double.IsFinite(area) && area > 0.0).ToFin(key.InvalidResult()).Bind(meshArea =>
            from density in kind.MeshCandidateDensity(area: meshArea, key: key)
            from candidates in SurfaceCandidatePoints(space: domain, density: density, key: key)
            from fit in CanonicalPlaneOf(points: candidates, key: key)
            let sites = DensityImportanceSites(candidates: candidates, count: Math.Min(val1: kind.Count.Value, val2: candidates.Count), density: kind.Policy.Density, context: context, seed: kind.Policy.Seed, key: key)
            from run in new PowerCcvtRun(domain: domain, count: kind.Count, policy: kind.Policy, sites: sites, totalMass: meshArea, planarityDeviation: fit.Deviation, context: context, key: key).Run()
            from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: run, key: key)
            select SurfaceSpectrumIntoReceipt(result: validated));
    }
    private static Fin<(Plane Plane, double Deviation)> CanonicalPlaneOf(Seq<Point3d> points, Op key) =>
        (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
            (PlaneFitResult.Success, { IsValid: true } valid) => key.AcceptValue(value: valid).Bind(p => key.AcceptValue(value: deviation).Map(d => (Plane: p, Deviation: d))),
            _ => Fin.Fail<(Plane Plane, double Deviation)>(error: key.InvalidResult()),
        };
    // Exponential-clock weighted reservoir (deterministic in seed); constant density falls to farthest coverage.
    private static Seq<Point3d> DensityImportanceSites(Seq<Point3d> candidates, int count, Option<ScalarField> density, Context context, int seed, Op key) =>
        density.Match(
            Some: field => toSeq(Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Weight: field.SampleScalar(sample: candidates[index: i], context: context, key: key).Match(Succ: value => value > 0.0 && double.IsFinite(value) ? value : 0.0, Fail: static _ => 0.0)))
                .Where(static row => row.Weight > 0.0)
                .OrderBy(row => -Math.Log(d: Deterministic.UnitInterval(point: candidates[index: row.Index], salt: 0, seed: seed)) / row.Weight)
                .Take(count: count)
                .Select(row => candidates[index: row.Index])),
            None: () => toSeq(FarthestIndices(candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), count: count).Select(i => candidates[index: i])));
    // The spectrum MOVES into the BNOT receipt — the generic slot clears so exactly one stream carries it.
    private static SampleResult SurfaceSpectrumIntoReceipt(SampleResult result) =>
        result.Receipt.Algorithm.Bind(static algorithm => algorithm.PowerCcvt.Map(ccvt => (Algorithm: algorithm, Ccvt: ccvt))).Match(
            Some: pair => result with { Receipt = result.Receipt with { Algorithm = Some(pair.Algorithm with {
                Spectrum = Option<MeshSamplingSpectrumReceipt>.None,
                PowerCcvt = Some(pair.Ccvt with { Spectrum = pair.Algorithm.Spectrum }) }) } },
            None: () => result);

    // The BNOT two-phase driver. Inner statement kernels (diagram rebuild, triplet assembly, Armijo searches)
    // are the named exemption; both convergence schedules are Atom + RepeatWhile with typed terminals.
    private sealed class PowerCcvtRun(MeshSpace domain, Dimension count, PowerCcvtPolicy policy, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context, Op key) {
        private readonly int siteCount = sites.Count;
        private readonly double targetMass = totalMass / Math.Max(val1: 1, val2: sites.Count);
        private readonly double searchDistance = domain.Native.GetBoundingBox(accurate: true).Diagonal.Length;

        internal Fin<SampleResult> Run() =>
            siteCount < 1
                ? Fin.Fail<SampleResult>(key.InvalidResult())
                : ConvergeNewton(currentSites: sites, seed: RebuildDiagram(currentSites: sites, weights: new Arr<double>([.. Enumerable.Repeat(element: 0.0, count: siteCount)])))
                    .Bind(seed => ConvergeOuter(seed: OuterState.Of(sites: sites, capacity: seed)).Bind(Finalize));

        private Fin<OuterState> ConvergeOuter(OuterState seed) {
            Atom<OuterState> cell = Atom(value: seed);
            _ = IO.lift(() => { _ = cell.Swap(f: state => state.Converged || state.Fault.IsSome ? state : OuterStep(state: state)); })
                .RepeatWhile(schedule: Schedule.recurs(times: policy.Iterations.Value), predicate: _ => !cell.Value.Converged && cell.Value.Fault.IsNone)
                .Run();
            return cell.Value.Fault.Match(Some: Fin.Fail<OuterState>, None: () => Fin.Succ(cell.Value));
        }
        // Two-phase site step (Lloyd then gradient ascent) off the capacity-converged diagram, then re-ENFORCE.
        private OuterState OuterStep(OuterState state) {
            SiteMotion motion = TwoPhaseSiteMotion(currentSites: state.Sites, capacity: state.Capacity);
            double meanSpacing = Spacing.MeanNearest(points: motion.Sites, measure: totalMass);
            return ConvergeNewton(currentSites: motion.Sites, seed: RebuildDiagram(currentSites: motion.Sites, weights: state.Capacity.Weights)).Match(
                Succ: advanced => state with {
                    Sites = motion.Sites, Capacity = advanced,
                    OuterIterations = state.OuterIterations + 1, LloydIterations = state.LloydIterations + motion.LloydIterations,
                    GradientIterations = state.GradientIterations + motion.GradientIterations,
                    StepHalvings = state.StepHalvings + motion.GradientHalvings,
                    RebuildCount = state.RebuildCount + advanced.RebuildCount + motion.GradientHalvings + 2,
                    PositionGradientNorm = motion.PositionGradientNorm,
                    TransportEnergyDelta = advanced.TransportEnergy - state.Capacity.TransportEnergy,
                    Converged = motion.Displacement <= policy.Motion.LloydPosTol.Value * meanSpacing
                             && motion.PositionGradientNorm <= policy.Motion.GradPosTol.Value * meanSpacing,
                },
                Fail: error => state with { Fault = Some(error) });
        }
        private SiteMotion TwoPhaseSiteMotion(Seq<Point3d> currentSites, NewtonState capacity) {
            (Seq<Point3d> lloydSites, int sweeps, RestrictedPowerDiagram lloydDiagram) = LloydPhase(currentSites: currentSites, diagram: capacity.Diagram, weights: capacity.Weights);
            (Seq<Point3d> gradientSites, int steps, int halvings, RestrictedPowerDiagram gradientDiagram) = GradientPhase(currentSites: lloydSites, diagram: lloydDiagram, weights: capacity.Weights);
            return new SiteMotion(Sites: gradientSites, LloydIterations: sweeps, GradientIterations: steps, GradientHalvings: halvings,
                Displacement: PairwiseShift(from: currentSites, to: gradientSites),
                PositionGradientNorm: Math.Sqrt(d: AscentDirection(sitesAt: gradientSites, diagram: gradientDiagram).Sum(static d => d.SquareLength)));
        }
        // Lloyd q_i <- b_i; a rebuild failure freezes the last admissible partition rather than failing the rail.
        private (Seq<Point3d> Sites, int Sweeps, RestrictedPowerDiagram Diagram) LloydPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) =>
            toSeq(Enumerable.Range(start: 0, count: policy.Motion.LloydSweeps.Value)).Fold(
                initialState: (Sites: currentSites, Sweeps: 0, Diagram: diagram),
                f: (state, _) => {
                    Seq<Point3d> moved = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: state.Diagram, site: i).Match(
                        Some: cell => cell.Empty || !cell.Barycenter.IsValid ? state.Sites[index: i] : cell.Barycenter,
                        None: () => state.Sites[index: i])));
                    return RebuildPowerCells(currentSites: moved, weights: weights).Match(
                        Succ: rebuilt => (Sites: moved, Sweeps: state.Sweeps + 1, Diagram: rebuilt),
                        Fail: _ => state);
                });
        // Armijo-INCREASE gradient ascent on -E along +2 m_i (b_i - q_i); sufficient-decrease would stall at step 1.
        private (Seq<Point3d> Sites, int Steps, int Halvings, RestrictedPowerDiagram Diagram) GradientPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) =>
            toSeq(Enumerable.Range(start: 0, count: policy.Motion.GradientSteps.Value)).Fold(
                initialState: (Sites: currentSites, Steps: 0, Halvings: 0, Diagram: diagram, Live: true),
                f: (state, _) => {
                    if (!state.Live) return state;
                    Vector3d[] direction = AscentDirection(sitesAt: state.Sites, diagram: state.Diagram);
                    double slope = direction.Sum(static d => d.SquareLength);
                    if (!(double.IsFinite(slope) && slope > 0.0)) return state with { Live = false };
                    (Seq<Point3d> sites, RestrictedPowerDiagram moved, int halvings, bool improved) = AscentLineSearch(
                        currentSites: state.Sites, direction: direction, slope: slope,
                        baseEnergy: -TransportEnergyOf(diagram: state.Diagram), weights: weights, alpha: policy.Search.InitialStep.Value, halvings: 0);
                    return improved
                        ? (Sites: sites, Steps: state.Steps + 1, Halvings: state.Halvings + halvings, Diagram: moved, Live: true)
                        : state with { Halvings = state.Halvings + halvings, Live = false };
                }) switch { var terminal => (terminal.Sites, terminal.Steps, terminal.Halvings, terminal.Diagram) };
        private (Seq<Point3d> Sites, RestrictedPowerDiagram Diagram, int Halvings, bool Improved) AscentLineSearch(Seq<Point3d> currentSites, Vector3d[] direction, double slope, double baseEnergy, Arr<double> weights, double alpha, int halvings) {
            Seq<Point3d> trial = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => currentSites[index: i] + (alpha * direction[i])));
            return RebuildPowerCells(currentSites: trial, weights: weights).Match(
                Succ: diagram => -TransportEnergyOf(diagram: diagram) >= baseEnergy + (policy.Search.C1.Value * alpha * slope)
                    ? (trial, diagram, halvings, true)
                    : Backtrack(),
                Fail: _ => Backtrack());
            (Seq<Point3d>, RestrictedPowerDiagram, int, bool) Backtrack() =>
                halvings >= policy.Search.MaxHalvings.Value
                    ? (currentSites, default, halvings, false)
                    : AscentLineSearch(currentSites: currentSites, direction: direction, slope: slope, baseEnergy: baseEnergy, weights: weights, alpha: alpha * policy.Search.Backtrack.Value, halvings: halvings + 1);
        }
        private Vector3d[] AscentDirection(Seq<Point3d> sitesAt, RestrictedPowerDiagram diagram) =>
            [.. Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: diagram, site: i).Match(
                Some: cell => cell.Empty || !cell.Barycenter.IsValid ? Vector3d.Zero : 2.0 * Math.Max(val1: cell.Mass, val2: 0.0) * (cell.Barycenter - sitesAt[index: i]),
                None: () => Vector3d.Zero))];
        // Cell integrals are site-anchored at build time; a parallel-axis m_i |q_i - b_i|^2 term would double-count.
        private static double TransportEnergyOf(RestrictedPowerDiagram diagram) =>
            diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.TransportCost);
        private static Option<PowerCell> CellOf(RestrictedPowerDiagram diagram, int site) =>
            site >= 0 && site < diagram.Cells.Count ? Some(diagram.Cells[index: site]) : Option<PowerCell>.None;
        private Fin<RestrictedPowerDiagram> RebuildPowerCells(Seq<Point3d> currentSites, Arr<double> weights) =>
            MeshKernel.RestrictedPowerCells(space: domain, sites: currentSites, weights: Some(weights), density: policy.Density, key: key);
        private static double PairwiseShift(Seq<Point3d> from, Seq<Point3d> to) =>
            Enumerable.Range(start: 0, count: Math.Min(val1: from.Count, val2: to.Count)).Sum(i => from[index: i].DistanceTo(other: to[index: i]));

        // Schedule-driven dual Newton: budget exhaustion is a typed terminal, never Fin.Fail.
        private Fin<NewtonState> ConvergeNewton(Seq<Point3d> currentSites, Fin<NewtonState> seed) =>
            seed.Bind(seedState => {
                Atom<NewtonState> cell = Atom(value: seedState);
                _ = IO.lift(() => { _ = cell.Swap(f: state => state.Converged || state.Fault.IsSome ? state : NewtonStep(currentSites: currentSites, state: state)); })
                    .RepeatWhile(schedule: Schedule.recurs(times: policy.Capacity.MaxNewton.Value), predicate: _ => !cell.Value.Converged && cell.Value.Fault.IsNone)
                    .Run();
                NewtonState terminal = cell.Value;
                return terminal.Fault.Match(Some: Fin.Fail<NewtonState>, None: () => Fin.Succ(terminal with { Converged = terminal.Residual.Inf <= policy.Capacity.ResidualTol.Value * targetMass }));
            });
        // One concave-Newton ascent step: L delta = g gauge-fixed through matrix.md SingularSolveDetailed.
        private NewtonState NewtonStep(Seq<Point3d> currentSites, NewtonState state) {
            Arr<double> gradient = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => targetMass - state.Diagram.Cells[index: i].Mass)]);
            double gradNorm = Math.Sqrt(d: gradient.AsIterable().Sum(static value => value * value));
            return HessianTriplets(currentSites: currentSites, diagram: state.Diagram)
                .Bind(triplets => SparseMatrix.FromTriplets(rows: Dimension.Create(value: siteCount), cols: Dimension.Create(value: siteCount), triplets: triplets, key: key))
                .Bind(laplacian => laplacian.SingularSolveDetailed(rhs: gradient, gauge: policy.Gauge.Policy(fragmentMasses: FragmentMasses(diagram: state.Diagram)), context: context, key: key))
                .Bind(solve => solve.IsValid
                    ? AscentSearch(currentSites: currentSites, state: state, direction: solve.Solution,
                        slope: Enumerable.Range(start: 0, count: siteCount).Sum(i => gradient[index: i] * solve.Solution[index: i]),
                        baseObjective: state.DualObjective, alpha: policy.Search.InitialStep.Value, halvings: 0).Map(advanced => (Solve: solve, Advanced: advanced))
                    : Fin.Fail<(SolveReceipt Solve, NewtonState Advanced)>(key.InvalidResult()))
                .Match(
                    Succ: step => step.Advanced with {
                        DualSolve = Some(step.Solve), WeightGradientNorm = gradNorm,
                        NewtonIterations = state.NewtonIterations + 1,
                        Converged = step.Advanced.Residual.Inf <= policy.Capacity.ResidualTol.Value * targetMass,
                    },
                    Fail: error => state with { Fault = Some(error), DualSolve = state.DualSolve });
        }
        private Fin<NewtonState> AscentSearch(Seq<Point3d> currentSites, NewtonState state, Arr<double> direction, double slope, double baseObjective, double alpha, int halvings) {
            Arr<double> advanced = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => state.Weights[index: i] + (alpha * direction[index: i]))]);
            return RebuildDiagram(currentSites: currentSites, weights: advanced).Bind(rebuilt =>
                rebuilt.DualObjective >= baseObjective + (policy.Search.C1.Value * alpha * slope) || halvings >= policy.Search.MaxHalvings.Value
                    ? Fin.Succ(rebuilt with { StepHalvings = state.StepHalvings + halvings, RebuildCount = state.RebuildCount + halvings + 2, NewtonIterations = state.NewtonIterations })
                    : AscentSearch(currentSites: currentSites, state: state, direction: direction, slope: slope, baseObjective: baseObjective, alpha: alpha * policy.Search.Backtrack.Value, halvings: halvings + 1));
        }
        // Density-weighted power-graph Laplacian: w_ij = l_ij / (2 |p_i - p_j|); symmetric scatter + matched
        // diagonal + scale-derived Tikhonov. d_ij re-derives from live dual weights inside the diagram build.
        private Fin<List<(int Row, int Col, double Value)>> HessianTriplets(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram) {
            List<(int Row, int Col, double Value)> triplets = [];
            double[] diagonal = new double[siteCount];
            double floor = policy.Capacity.NewtonFloor.Value * Math.Max(val1: searchDistance, val2: double.Epsilon);
            foreach (PowerFacet facet in diagram.Facets.AsIterable()) {
                if (facet.SiteI < 0 || facet.SiteI >= siteCount || facet.SiteJ < 0 || facet.SiteJ >= siteCount || facet.SiteI == facet.SiteJ) continue;
                double siteDistance = currentSites[index: facet.SiteI].DistanceTo(other: currentSites[index: facet.SiteJ]);
                if (!(double.IsFinite(siteDistance) && siteDistance > floor && double.IsFinite(facet.Length) && facet.Length >= 0.0)) continue;
                double weight = facet.Length / (2.0 * siteDistance);
                if (!(double.IsFinite(weight) && weight >= 0.0)) continue;
                triplets.Add(item: (facet.SiteI, facet.SiteJ, -weight));
                triplets.Add(item: (facet.SiteJ, facet.SiteI, -weight));
                diagonal[facet.SiteI] += weight;
                diagonal[facet.SiteJ] += weight;
            }
            double tikhonov = policy.Capacity.NewtonFloor.Value * Math.Max(val1: targetMass, val2: double.Epsilon);
            for (int i = 0; i < siteCount; i++) triplets.Add(item: (i, i, diagonal[i] + tikhonov));
            return triplets.Exists(static row => row.Value != 0.0) ? Fin.Succ(triplets) : Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidResult());
        }
        private Arr<double> FragmentMasses(RestrictedPowerDiagram diagram) =>
            new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => Math.Max(val1: diagram.Cells[index: i].Mass, val2: 0.0))]);
        // Rebuild + recompute the dual facts: Phi = Sum TransportCost_i + Sum w_i (m* - m_i).
        private Fin<NewtonState> RebuildDiagram(Seq<Point3d> currentSites, Arr<double> weights) =>
            RebuildPowerCells(currentSites: currentSites, weights: weights).Map(diagram => {
                double transport = TransportEnergyOf(diagram: diagram);
                double dual = transport + Enumerable.Range(start: 0, count: siteCount).Sum(i => weights[index: i] * (targetMass - diagram.Cells[index: i].Mass));
                (double inf, double l1, double l2) = Enumerable.Range(start: 0, count: siteCount).Aggregate(
                    seed: (Inf: 0.0, L1: 0.0, L2: 0.0),
                    func: (acc, i) => Math.Abs(value: diagram.Cells[index: i].Mass - targetMass) switch {
                        double deviation => (Math.Max(val1: acc.Inf, val2: deviation), acc.L1 + deviation, acc.L2 + (deviation * deviation)),
                    });
                CapacityResidual residual = new(Inf: inf, L1: l1, L2: Math.Sqrt(d: l2), Normalized: inf / Math.Max(val1: targetMass, val2: double.Epsilon));
                return new NewtonState(Weights: weights, Diagram: diagram, Residual: residual, DualObjective: dual, TransportEnergy: transport,
                    Converged: false, NewtonIterations: 0, StepHalvings: 0, RebuildCount: 0, Fault: Option<Error>.None,
                    DualSolve: Option<SolveReceipt>.None, WeightGradientNorm: 0.0);
            });
        // Break lattice regularity ONCE, lift to surface, emit the witness with all outer fields populated.
        private Fin<SampleResult> Finalize(OuterState outer) {
            NewtonState terminal = outer.Capacity;
            double meanSpacing = Spacing.MeanNearest(points: outer.Sites, measure: totalMass);
            Regularity broken = BreakRegularity(currentSites: outer.Sites, meanSpacing: meanSpacing);
            Seq<Point3d> lifted = toSeq(broken.Sites.AsIterable().Map(site => {
                MeshPoint meshPoint = domain.Native.ClosestMeshPoint(testPoint: site, maximumDistance: searchDistance);
                return meshPoint is not null && meshPoint.Point.IsValid ? meshPoint.Point : site;
            }));
            RestrictedPowerReceipt diagramReceipt = terminal.Diagram.Receipt;
            PowerCellFragmentFacts fragments = new(
                SiteCount: siteCount, FragmentCount: diagramReceipt.FragmentCount, FacetCount: diagramReceipt.NeighborFacetCount,
                EmptyCellCount: diagramReceipt.EmptyCellCount,
                TotalMass: terminal.Diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.Mass),
                MassMin: terminal.Diagram.Cells.AsIterable().Map(static cell => cell.Mass).Fold(initialState: double.PositiveInfinity, f: Math.Min) switch { double m when double.IsPositiveInfinity(d: m) => 0.0, double m => m },
                MassMax: terminal.Diagram.Cells.AsIterable().Map(static cell => cell.Mass).Fold(initialState: 0.0, f: Math.Max),
                IntegrationResidual: diagramReceipt.IntegrationResidual);
            (double weightMin, double weightMax, double weightMean) = terminal.Weights.Count == 0
                ? (0.0, 0.0, 0.0)
                : (terminal.Weights.AsIterable().Fold(initialState: double.PositiveInfinity, f: Math.Min),
                   terminal.Weights.AsIterable().Fold(initialState: double.NegativeInfinity, f: Math.Max),
                   terminal.Weights.AsIterable().Sum() / terminal.Weights.Count);
            PowerCcvtReceipt receipt = new(
                SiteCount: siteCount, TargetMass: targetMass, ActualMassMin: fragments.MassMin, ActualMassMax: fragments.MassMax,
                CapacityResidualInf: terminal.Residual.Inf, CapacityResidualL1: terminal.Residual.L1, CapacityResidualL2: terminal.Residual.L2, CapacityResidualNormalized: terminal.Residual.Normalized,
                OuterIterations: Math.Max(val1: 1, val2: outer.OuterIterations), LloydIterations: outer.LloydIterations, GradientIterations: outer.GradientIterations, DualNewtonIterations: terminal.NewtonIterations,
                WeightMin: weightMin, WeightMax: weightMax, WeightMean: weightMean, TransportEnergy: terminal.TransportEnergy, TransportEnergyDelta: outer.TransportEnergyDelta,
                DualObjective: terminal.DualObjective, CentroidShift: PairwiseShift(from: lifted, to: sites), PositionGradientNorm: outer.PositionGradientNorm, WeightGradientNorm: terminal.WeightGradientNorm,
                EmptyCellCount: diagramReceipt.EmptyCellCount, StepHalvingCount: outer.StepHalvings, RebuildCount: outer.RebuildCount,
                AliasedSiteCount: broken.AliasedCount, JitteredSiteCount: broken.JitteredCount, RelocatedSiteCount: broken.RelocatedCount,
                NormalizedPoissonRadius: Spacing.NormalizedPoissonRadius(points: lifted, measure: totalMass), PlanarityDeviation: planarityDeviation,
                Gauge: policy.Gauge, Stop: outer.Converged ? PowerCcvtStopKind.Converged : PowerCcvtStopKind.StoppedWithoutConvergence,
                Fragments: fragments, DualSolve: terminal.DualSolve, Spectrum: Option<MeshSamplingSpectrumReceipt>.None);
            return receipt.IsValid
                ? Fin.Succ(new SampleResult(Points: lifted, Mass: Option<Arr<double>>.None,
                    Receipt: ReceiptOf(attempted: siteCount, emitted: lifted, rejected: diagramReceipt.EmptyCellCount, candidates: Some(siteCount),
                        iterations: Some(outer.OuterIterations),
                        stop: lifted.IsEmpty ? SampleStopKind.AllRejected : lifted.Count < count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                        status: diagramReceipt.EmptyCellCount > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                        densityError: Option<double>.None,
                        algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.ContinuousPowerCcvt, Seed: Some(policy.Seed), TargetCount: Some(count.Value),
                            CapacityResidual: Some(terminal.Residual.Inf), CapacityResidualValidated: terminal.Converged,
                            TransportAssignmentValidated: !lifted.IsEmpty && terminal.DualSolve.Map(static solve => solve.IsValid).IfNone(noneValue: false),
                            PowerCcvt: Some(receipt))))))
                : Fin.Fail<SampleResult>(key.InvalidResult());
        }
        // Deterministic Box-Muller jitter + relocation; every offset replays from policy.Seed.
        private Regularity BreakRegularity(Seq<Point3d> currentSites, double meanSpacing) {
            if (currentSites.Count < 2 || meanSpacing <= double.Epsilon) return new Regularity(Sites: currentSites, AliasedCount: 0, JitteredCount: 0, RelocatedCount: 0);
            double aliasRadius = policy.Regularity.AliasScale.Value * meanSpacing;
            double jitterMagnitude = policy.Regularity.JitterVariance.Value * policy.Regularity.MagnitudeScale.Value * meanSpacing;
            int total = currentSites.Count;
            bool[] aliased = [.. Enumerable.Range(start: 0, count: total).Select(i => Enumerable.Range(start: 0, count: i).Any(j => currentSites[index: i].DistanceTo(other: currentSites[index: j]) < aliasRadius))];
            int aliasedCount = aliased.Count(static flag => flag);
            int relocateBudget = Math.Min(val1: aliasedCount, val2: (int)Math.Floor(d: policy.Regularity.RelocateFraction.Value * aliasedCount));
            int relocated = 0;
            Point3d[] moved = new Point3d[total];
            for (int i = 0; i < total; i++) {
                bool relocate = aliased[i] && relocated < relocateBudget;
                moved[i] = aliased[i]
                    ? currentSites[index: i] + JitterOffset(site: currentSites[index: i], salt: i, magnitude: relocate ? jitterMagnitude + meanSpacing : jitterMagnitude)
                    : currentSites[index: i];
                if (relocate) relocated++;
            }
            return new Regularity(Sites: toSeq(moved), AliasedCount: aliasedCount, JitteredCount: aliasedCount, RelocatedCount: relocated);
        }
        private Vector3d JitterOffset(Point3d site, int salt, double magnitude) {
            double u1 = Deterministic.UnitInterval(point: site, salt: (salt * 4) + 1, seed: policy.Seed);
            double u2 = Deterministic.UnitInterval(point: site, salt: (salt * 4) + 2, seed: policy.Seed);
            double radius = magnitude * Math.Sqrt(d: Math.Max(val1: 0.0, val2: -2.0 * Math.Log(d: u1)));
            double angle = 2.0 * Math.PI * u2;
            return new Vector3d(x: radius * Math.Cos(d: angle), y: radius * Math.Sin(a: angle), z: 0.0);
        }
        [StructLayout(LayoutKind.Auto)] private readonly record struct CapacityResidual(double Inf, double L1, double L2, double Normalized);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct NewtonState(
            Arr<double> Weights, RestrictedPowerDiagram Diagram, CapacityResidual Residual, double DualObjective, double TransportEnergy,
            bool Converged, int NewtonIterations, int StepHalvings, int RebuildCount, Option<Error> Fault, Option<SolveReceipt> DualSolve, double WeightGradientNorm);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct OuterState(
            Seq<Point3d> Sites, NewtonState Capacity, int OuterIterations, int LloydIterations, int GradientIterations,
            int StepHalvings, int RebuildCount, double PositionGradientNorm, double TransportEnergyDelta,
            bool Converged, Option<Error> Fault) {
            internal static OuterState Of(Seq<Point3d> sites, NewtonState capacity) => new(
                Sites: sites, Capacity: capacity, OuterIterations: 0, LloydIterations: 0, GradientIterations: 0,
                StepHalvings: capacity.StepHalvings, RebuildCount: capacity.RebuildCount, PositionGradientNorm: 0.0,
                TransportEnergyDelta: 0.0, Converged: false, Fault: Option<Error>.None);
        }
        [StructLayout(LayoutKind.Auto)] private readonly record struct SiteMotion(Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm);
        [StructLayout(LayoutKind.Auto)] private readonly record struct Regularity(Seq<Point3d> Sites, int AliasedCount, int JitteredCount, int RelocatedCount);
    }

    // 11-arm candidate-suite dispatch; each arm reads its case payload and emits a SampleSelection.
    private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<SampleCandidate> candidates, bool admitsPoisson, Option<(int Dimensions, double Measure)> domainMeasure, Context context, Op key) =>
        from selection in kind switch {
            SampleKind.PoissonDiskCase pd when admitsPoisson => PoissonDiskSelection(candidates: candidates, radius: pd.Radius.Value, attempts: pd.Attempts.Value, seed: pd.Seed, key: key),
            SampleKind.FarthestCase fp => SelectionOf(kind: kind, candidates: candidates, indices: FarthestIndices(candidates: candidates, count: fp.Count.Value), key: key),
            SampleKind.OptimizeCase fpo => SelectionOf(kind: kind, candidates: candidates, indices: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value), key: key),
            SampleKind.LloydCase lloyd => RelaxationSample(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value, capacity: Option<int>.None, key: key)
                .Bind(indices => SelectionOf(kind: kind, candidates: candidates, indices: indices, key: key)),
            SampleKind.CapacityCase ccvt => CapacityCvtSelection(candidates: candidates, count: ccvt.Count.Value, limit: ccvt.Limit.Value, iterations: ccvt.Iterations.Value, tolerance: ccvt.Tolerance.Value, key: key),
            SampleKind.ScalarDensityCase density => DensitySelection(candidates: candidates, density: density.Density, count: density.Count.Value,
                minSpacing: 0.5 * (BoundingMeasure(candidates: candidates) switch { (3, double m) => Math.Pow(x: m / Math.Max(val1: 1, val2: density.Count.Value), y: 1.0 / 3.0), (_, double m) => Math.Sqrt(d: m / Math.Max(val1: 1, val2: density.Count.Value)) }),
                context: context, key: key),
            SampleKind.AdaptiveCase adaptive => DensitySelection(candidates: candidates, density: adaptive.Density, count: adaptive.Count.Value, minSpacing: adaptive.MinSpacing.Value, context: context, key: key),
            SampleKind.SampleEliminationCase elimination => SampleElimination(candidates: candidates, count: elimination.Count.Value, alpha: elimination.Alpha.Value, beta: elimination.Beta.Value, gamma: elimination.Gamma.Value, seed: elimination.Seed, domainMeasure: domainMeasure, key: key)
                .Bind(result => SelectionOf(candidates: candidates, indices: result.Indices, algorithm: Some(result.Algorithm), key: key)),
            SampleKind.DworkVariableDensityCase dwork => DworkCandidateSelection(candidates: candidates, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts.Value, seed: dwork.Seed, context: context, key: key),
            SampleKind.PoissonDiskCase pd => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: pd.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }
        let sampled = toSeq(selection.Points)
        let rejected = selection.DensityRejected.IfNone(Math.Max(val1: 0, val2: candidates.Count - selection.Points.Length))
        let capacityLimited = selection.Algorithm.Map(static receipt => receipt.Kind.Equals(SampleAlgorithmKind.CapacityLimitedLloydCandidate) && !receipt.CapacityResidualValidated).IfNone(noneValue: false)
        select new SampleResult(
            Points: sampled, Mass: selection.Mass,
            Receipt: ReceiptOf(attempted: candidates.Count, emitted: sampled, rejected: rejected, candidates: Some(candidates.Count), iterations: kind.Facts.Iterations,
                stop: sampled.Count <= 0 ? SampleStopKind.AllRejected : capacityLimited ? SampleStopKind.CapacityLimited : kind.Facts.Count.Map(requested => sampled.Count < requested ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed).IfNone(SampleStopKind.Completed),
                status: selection.DensityRejected.Map(static count => count > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted).IfNone(SampleDomainStatus.CandidateAccepted),
                densityError: kind.DensityError(emitted: sampled.Count), densityAccepted: selection.DensityAccepted, densityRejected: selection.DensityRejected, algorithm: selection.Algorithm));

    private static Fin<SampleSelection> SelectionOf(SampleKind kind, Seq<SampleCandidate> candidates, int[] indices, Op key, Option<double> radius = default) =>
        SelectionOf(candidates: candidates, indices: indices, algorithm: Some(new SampleAlgorithmReceipt(Kind: kind.Facts.Algorithm, TargetCount: kind.Facts.Count, Radius: radius)), key: key);
    private static Fin<SampleSelection> SelectionOf(Seq<SampleCandidate> candidates, int[] indices, Option<SampleAlgorithmReceipt> algorithm, Op key) {
        Point3d[] points = [.. indices.Select(i => candidates[index: i].Point)];
        Seq<double> mass = toSeq(indices.Select(i => candidates[index: i].Mass).Somes());
        return (indices.Length, mass.Count) switch {
            (0, _) or (_, 0) => Fin.Succ(new SampleSelection(Points: points, Mass: Option<Arr<double>>.None, DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            (int count, int weights) when count == weights => NormalizeMass(mass: mass, key: key).Map(normalized => new SampleSelection(Points: points, Mass: Some(normalized), DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            _ => Fin.Fail<SampleSelection>(key.InvalidResult()),
        };
    }
    private static SampleReceipt ReceiptOf(int attempted, Seq<Point3d> emitted, int rejected, Option<int> candidates, Option<int> iterations, SampleStopKind stop, SampleDomainStatus status, Option<double> densityError, Option<int> densityAccepted = default, Option<int> densityRejected = default, Option<SampleAlgorithmReceipt> algorithm = default) =>
        (emitted.Count < 2
            ? (Option<double>.None, Option<double>.None, Option<double>.None)
            : toSeq(Enumerable.Range(start: 0, count: emitted.Count - 1)
                .SelectMany(collectionSelector: i => Enumerable.Range(start: i + 1, count: emitted.Count - i - 1), resultSelector: (i, j) => emitted[index: i].DistanceTo(other: emitted[index: j])))
                .Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0, Sum: 0.0, Count: 0), f: static (acc, distance) => (Math.Min(val1: acc.Min, val2: distance), Math.Max(val1: acc.Max, val2: distance), acc.Sum + distance, acc.Count + 1))
                switch { { Count: > 0 } stats => (Some(stats.Min), Some(stats.Sum / stats.Count), Some(stats.Max)), _ => (Option<double>.None, Option<double>.None, Option<double>.None) })
        switch {
            (Option<double> min, Option<double> mean, Option<double> max) => new SampleReceipt(
                Attempted: attempted, Emitted: emitted.Count, Rejected: rejected, CandidateCount: candidates,
                MinSpacing: min, MeanSpacing: mean, MaxSpacing: max, DensityError: densityError,
                DensityAccepted: densityAccepted, DensityRejected: densityRejected, Iterations: iterations,
                Stop: stop, DomainStatus: status, Algorithm: algorithm),
        };
    private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, Op key) =>
        CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: mass.Count, key: key);
    // Weighted-priority density: exponential-clock ordering -log(U)/w, per-sample local radius from the weight.
    private static Fin<SampleSelection> DensitySelection(Seq<SampleCandidate> candidates, ScalarField density, int count, double minSpacing, Context context, Op key) {
        double[] weights = new double[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, MinWeight: double.PositiveInfinity, MaxWeight: 0.0)),
            f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? key.AcceptValue(value: value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => { weights[i] = valid; return (current.Accepted + 1, current.Rejected, Math.Min(val1: current.MinWeight, val2: valid), Math.Max(val1: current.MaxWeight, val2: valid)); })
                    : Fin.Succ((current.Accepted, current.Rejected + 1, current.MinWeight, current.MaxWeight)))))
            .Bind(stats => stats.Accepted > 0
                ? PrioritySelection(candidates: candidates, weights: weights, count: count, minSpacing: minSpacing, minWeight: stats.MinWeight, maxWeight: stats.MaxWeight, accepted: stats.Accepted, rejected: stats.Rejected, key: key)
                : Fin.Fail<SampleSelection>(key.InvalidResult()));
    }
    private static Fin<SampleSelection> PrioritySelection(Seq<SampleCandidate> candidates, double[] weights, int count, double minSpacing, double minWeight, double maxWeight, int accepted, int rejected, Op key) {
        List<(Point3d Point, double Radius)> chosen = []; List<double> mass = [];
        (double localMin, double localMax) = (double.PositiveInfinity, 0.0);
        using IEnumerator<int> ordered = Enumerable.Range(start: 0, count: candidates.Count)
            .Where(i => weights[i] > 0.0)
            .OrderBy(i => -Math.Log(d: Deterministic.UnitInterval(point: candidates[index: i].Point, salt: 0, seed: 0)) / weights[i])
            .GetEnumerator();
        while (chosen.Count < count && ordered.MoveNext()) {
            int index = ordered.Current;
            Point3d candidate = candidates[index: index].Point;
            double local = minSpacing / Math.Sqrt(d: Math.Max(val1: weights[index] / Math.Max(val1: maxWeight, val2: double.Epsilon), val2: double.Epsilon));
            (localMin, localMax) = (Math.Min(val1: localMin, val2: local), Math.Max(val1: localMax, val2: local));
            if (chosen.TrueForAll(existing => candidate.DistanceTo(other: existing.Point) >= Math.Max(val1: existing.Radius, val2: local))) { chosen.Add(item: (candidate, local)); mass.Add(item: weights[index]); }
        }
        return NormalizeMass(mass: toSeq(mass), key: key).Map(normalized => new SampleSelection(
            Points: [.. chosen.Select(static sample => sample.Point)], Mass: Some(normalized), DensityAccepted: Some(accepted), DensityRejected: Some(rejected),
            Algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.VariableDensityPoisson, TargetCount: Some(count), DensityMin: Some(minWeight), DensityMax: Some(maxWeight), LocalRadiusMin: Some(localMin), LocalRadiusMax: Some(localMax)))));
    }

    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCell(long X, long Y, long Z);
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCandidate(int Index, double Radius);
    // Dwork variable-density over an admitted candidate cloud: spatial hash at pitch rMin/sqrt(3), per-parent
    // annulus band [r, 2r] resolved by a bounded shell ring query — named statement-kernel exemption.
    private static Fin<SampleSelection> DworkCandidateSelection(Seq<SampleCandidate> candidates, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
        DworkCandidate[] admitted = new DworkCandidate[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, RadiusMin: double.PositiveInfinity, RadiusMax: 0.0)),
            f: (state, i) => state.Bind(current => radius.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? key.AcceptValue(value: Math.Max(val1: minRadius, val2: value)).Map(local => {
                        admitted[current.Accepted] = new DworkCandidate(Index: i, Radius: local);
                        return (current.Accepted + 1, current.Rejected, Math.Min(val1: current.RadiusMin, val2: local), Math.Max(val1: current.RadiusMax, val2: local));
                    })
                    : Fin.Succ((current.Accepted, current.Rejected + 1, current.RadiusMin, current.RadiusMax)))))
            .Bind(stats => {
                if (stats.Accepted <= 0) return Fin.Fail<SampleSelection>(key.InvalidResult());
                DworkCandidate[] ordered = [.. admitted.Take(count: stats.Accepted).OrderBy(item => Deterministic.OrderKey(point: candidates[index: item.Index].Point, seed: seed))];
                double cellSize = Math.Max(val1: stats.RadiusMin / Math.Sqrt(d: 3.0), val2: double.Epsilon);
                Point3d gridOrigin = ordered.Length > 0 ? new BoundingBox(points: ordered.Select(item => candidates[index: item.Index].Point)).Min : Point3d.Origin;
                DworkCell CellOf(Point3d point) => new(X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize), Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize), Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
                Dictionary<DworkCell, List<int>> cloudGrid = [];
                for (int o = 0; o < ordered.Length; o++) {
                    DworkCell cell = CellOf(point: candidates[index: ordered[o].Index].Point);
                    if (!cloudGrid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; cloudGrid.Add(key: cell, value: bucket); }
                    bucket.Add(item: o);
                }
                List<DworkCandidate> chosen = ordered.Length > 0 ? [ordered[0]] : [];
                Dictionary<DworkCell, List<int>> chosenGrid = [];
                void Record(DworkCandidate candidate) {
                    DworkCell cell = CellOf(point: candidates[index: candidate.Index].Point);
                    if (!chosenGrid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; chosenGrid.Add(key: cell, value: bucket); }
                    bucket.Add(item: chosen.Count - 1);
                }
                if (chosen.Count > 0) Record(candidate: chosen[0]);
                bool Conflicts(DworkCandidate candidate) {
                    Point3d at = candidates[index: candidate.Index].Point;
                    int shells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: stats.RadiusMax) / cellSize));
                    DworkCell home = CellOf(point: at);
                    for (int dx = -shells; dx <= shells; dx++)
                        for (int dy = -shells; dy <= shells; dy++)
                            for (int dz = -shells; dz <= shells; dz++)
                                if (chosenGrid.TryGetValue(key: new DworkCell(X: home.X + dx, Y: home.Y + dy, Z: home.Z + dz), value: out List<int>? bucket))
                                    foreach (int slot in bucket) {
                                        DworkCandidate other = chosen[index: slot];
                                        if (at.DistanceTo(other: candidates[index: other.Index].Point) < Math.Max(val1: other.Radius, val2: candidate.Radius)) return true;
                                    }
                    return false;
                }
                List<DworkCandidate> active = ordered.Length > 0 ? [ordered[0]] : [];
                (int activePops, int tooClose, int outside) = (0, 0, 0);
                while (active.Count > 0 && chosen.Count < count) {
                    int activeOffset = (int)(Deterministic.OrderKey(point: candidates[index: active[0].Index].Point, seed: seed + activePops) % (ulong)active.Count);
                    DworkCandidate parent = active[activeOffset];
                    Point3d parentPoint = candidates[index: parent.Index].Point;
                    DworkCell parentCell = CellOf(point: parentPoint);
                    int bandShells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: 2.0 * parent.Radius / cellSize));
                    List<DworkCandidate> annulus = [];
                    for (int dx = -bandShells; dx <= bandShells; dx++)
                        for (int dy = -bandShells; dy <= bandShells; dy++)
                            for (int dz = -bandShells; dz <= bandShells; dz++)
                                if (cloudGrid.TryGetValue(key: new DworkCell(X: parentCell.X + dx, Y: parentCell.Y + dy, Z: parentCell.Z + dz), value: out List<int>? bucket))
                                    foreach (int o in bucket) {
                                        DworkCandidate candidate = ordered[o];
                                        double distance = parentPoint.DistanceTo(other: candidates[index: candidate.Index].Point);
                                        if (distance >= parent.Radius && distance <= 2.0 * parent.Radius) annulus.Add(item: candidate);
                                    }
                    bool accepted = false;
                    for (int attempt = 0; attempt < attempts && !accepted && annulus.Count > 0; attempt++) {
                        DworkCandidate candidate = annulus[(int)(Deterministic.OrderKey(point: parentPoint, seed: seed + activePops + attempt + 1) % (ulong)annulus.Count)];
                        if (chosen.Exists(item => item.Index == candidate.Index) || Conflicts(candidate: candidate)) { tooClose++; continue; }
                        chosen.Add(item: candidate);
                        Record(candidate: candidate);
                        active.Add(item: candidate);
                        accepted = true;
                    }
                    if (!accepted && annulus.Count == 0) outside++;
                    if (!accepted) active.RemoveAt(index: activeOffset);
                    activePops++;
                }
                DworkReceipt dwork = new(Domain: DworkSamplingDomain.CandidateSet, RMin: stats.RadiusMin, BackgroundCellSize: Some(cellSize), BackgroundGridCells: Some(cloudGrid.Count),
                    AttemptsPerActive: attempts, GeneratedCandidates: chosen.Count + tooClose + outside + stats.Rejected, ActivePops: activePops,
                    RejectedTooClose: tooClose, RejectedDomain: stats.Rejected + outside, LocalRadiusMin: stats.RadiusMin, LocalRadiusMax: stats.RadiusMax);
                return SelectionOf(candidates: candidates, indices: [.. chosen.Select(static item => item.Index)],
                    algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.DworkVariableDensity, Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(ordered.Length),
                        Attempts: Some(attempts), ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(stats.Rejected + outside),
                        LocalRadiusMin: Some(stats.RadiusMin), LocalRadiusMax: Some(stats.RadiusMax), Dwork: Some(dwork))), key: key);
            });
    }

    // Dwork on the live mesh surface: area-CDF barycentric seeding, tangent-frame annulus proposals projected
    // back by ClosestMeshPoint, radius field re-sampled at every accepted point — statement-kernel exemption.
    private sealed class DworkMeshRun(Mesh mesh, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
        private readonly double cellSize = minRadius / Math.Sqrt(d: 3.0);
        private readonly List<DworkSurfacePoint> chosen = [];
        private readonly List<int> active = [];
        private readonly Dictionary<DworkCell, List<int>> grid = [];
        private DworkTriangle[] triangles = [];
        private Point3d gridOrigin = Point3d.Origin;
        private double totalArea;
        private (double Min, double Max) radiusBand = (double.PositiveInfinity, 0.0);
        private (int Proposals, int ActivePops, int TooClose, int Domain) tally;

        [StructLayout(LayoutKind.Auto)] private readonly record struct DworkSurfacePoint(Point3d Point, double Radius);
        [StructLayout(LayoutKind.Auto)] private readonly record struct DworkTriangle(Point3d A, Point3d B, Point3d C, double CumulativeArea);

        internal static Fin<SampleSelection> Execute(MeshSpace domain, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
            using Mesh mesh = domain.Native.DuplicateMesh();
            if (mesh.Faces.QuadCount > 0 && !mesh.Faces.ConvertQuadsToTriangles()) return Fin.Fail<SampleSelection>(key.InvalidResult());
            _ = mesh.FaceNormals.ComputeFaceNormals();
            return new DworkMeshRun(mesh: mesh, radius: radius, count: count, minRadius: minRadius, attempts: attempts, seed: seed, context: context, key: key).Run();
        }
        private Fin<SampleSelection> Run() =>
            BuildTriangles().Bind(_ => {
                Option<DworkSurfacePoint> first = Option<DworkSurfacePoint>.None;
                for (int attempt = 0; attempt < attempts && first.IsNone; attempt++) {
                    tally.Proposals++;
                    first = SurfaceSample(salt: attempt * 3).Bind(RadiusAt);
                    if (first.IsNone) tally.Domain++;
                }
                return first.Match(
                    Some: sample => {
                        Add(sample: sample);
                        while (active.Count > 0 && chosen.Count < count) {
                            int activeOffset = (int)(Deterministic.OrderKey(point: chosen[index: active[0]].Point, seed: seed + tally.ActivePops) % (ulong)active.Count);
                            int parentIndex = active[index: activeOffset];
                            bool accepted = false;
                            for (int attempt = 0; attempt < attempts && !accepted; attempt++) {
                                tally.Proposals++;
                                accepted = AnnulusCandidate(parent: chosen[index: parentIndex], attempt: attempt).Match(
                                    Some: value => { if (Conflicts(candidate: value)) { tally.TooClose++; return false; } Add(sample: value); return true; },
                                    None: () => { tally.Domain++; return false; });
                            }
                            if (!accepted) active.RemoveAt(index: activeOffset);
                            tally.ActivePops++;
                        }
                        return Selection();
                    },
                    None: () => Fin.Fail<SampleSelection>(key.InvalidResult()));
            });
        private Fin<Unit> BuildTriangles() {
            List<DworkTriangle> built = [];
            double cumulative = 0.0;
            for (int f = 0; f < mesh.Faces.Count; f++) {
                MeshFace face = mesh.Faces[index: f];
                if (!face.IsTriangle) continue;
                Point3d a = mesh.Vertices[index: face.A], b = mesh.Vertices[index: face.B], c = mesh.Vertices[index: face.C];
                double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
                if (!double.IsFinite(area) || area <= double.Epsilon) continue;
                cumulative += area;
                built.Add(item: new DworkTriangle(A: a, B: b, C: c, CumulativeArea: cumulative));
            }
            BoundingBox bounds = mesh.GetBoundingBox(accurate: true);
            (triangles, totalArea, gridOrigin) = ([.. built], cumulative, bounds.IsValid ? bounds.Min : Point3d.Origin);
            return triangles.Length > 0 && double.IsFinite(totalArea) && totalArea > double.Epsilon && bounds.IsValid
                ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult());
        }
        private Option<Point3d> SurfaceSample(int salt) {
            double target = Deterministic.UnitInterval(point: Point3d.Origin, salt: salt, seed: seed) * totalArea;
            DworkTriangle triangle = triangles[^1];
            for (int i = 0; i < triangles.Length; i++) if (target <= triangles[i].CumulativeArea) { triangle = triangles[i]; break; }
            double u = Math.Sqrt(d: Deterministic.UnitInterval(point: triangle.A, salt: salt + 1, seed: seed));
            double v = Deterministic.UnitInterval(point: triangle.B, salt: salt + 2, seed: seed);
            (double wa, double wb, double wc) = (1.0 - u, u * (1.0 - v), u * v);
            Point3d sample = new(x: (wa * triangle.A.X) + (wb * triangle.B.X) + (wc * triangle.C.X), y: (wa * triangle.A.Y) + (wb * triangle.B.Y) + (wc * triangle.C.Y), z: (wa * triangle.A.Z) + (wb * triangle.B.Z) + (wc * triangle.C.Z));
            return sample.IsValid ? Some(sample) : Option<Point3d>.None;
        }
        private Option<DworkSurfacePoint> RadiusAt(Point3d point) =>
            radius.SampleScalar(sample: point, context: context, key: key).Match(
                Succ: value => value > 0.0 && double.IsFinite(value) ? Some(new DworkSurfacePoint(Point: point, Radius: Math.Max(val1: minRadius, val2: value))) : Option<DworkSurfacePoint>.None,
                Fail: _ => Option<DworkSurfacePoint>.None);
        private Option<DworkSurfacePoint> AnnulusCandidate(DworkSurfacePoint parent, int attempt) {
            double angle = 2.0 * Math.PI * Deterministic.UnitInterval(point: parent.Point, salt: (tally.ActivePops * attempts * 4) + (attempt * 4) + 7, seed: seed);
            double distance = parent.Radius * Math.Sqrt(d: 1.0 + (3.0 * Deterministic.UnitInterval(point: parent.Point, salt: (tally.ActivePops * attempts * 4) + (attempt * 4) + 8, seed: seed)));
            Vector3d normal = NormalAt(point: parent.Point);
            Vector3d tangent = VectorFrame.SeedPerpendicular(axis: normal);
            Vector3d bitangent = Vector3d.CrossProduct(a: normal, b: tangent);
            if (!bitangent.Unitize()) return Option<DworkSurfacePoint>.None;
            Point3d raw = parent.Point + (distance * ((Math.Cos(d: angle) * tangent) + (Math.Sin(a: angle) * bitangent)));
            MeshPoint? hit = mesh.ClosestMeshPoint(testPoint: raw, maximumDistance: distance + radiusBand.Max + context.Absolute.Value);
            if (hit is null || hit.FaceIndex < 0 || hit.FaceIndex >= mesh.Faces.Count) return Option<DworkSurfacePoint>.None;
            double projectedDistance = hit.Point.DistanceTo(other: parent.Point);
            return projectedDistance >= parent.Radius && projectedDistance <= (2.0 * parent.Radius) + context.Absolute.Value
                ? RadiusAt(point: hit.Point) : Option<DworkSurfacePoint>.None;
        }
        private Vector3d NormalAt(Point3d point) {
            MeshPoint? hit = mesh.ClosestMeshPoint(testPoint: point, maximumDistance: Math.Max(val1: minRadius, val2: context.Absolute.Value));
            Vector3d normal = hit is { FaceIndex: >= 0 } && hit.FaceIndex < mesh.FaceNormals.Count ? mesh.FaceNormals[index: hit.FaceIndex] : Vector3d.ZAxis;
            return normal.IsValid && !normal.IsTiny(context.Absolute.Value) && normal.Unitize() ? normal : Vector3d.ZAxis;
        }
        private bool Conflicts(DworkSurfacePoint candidate) {
            int range = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: radiusBand.Max) / cellSize));
            DworkCell cell = CellOf(point: candidate.Point);
            for (int dx = -range; dx <= range; dx++)
                for (int dy = -range; dy <= range; dy++)
                    for (int dz = -range; dz <= range; dz++)
                        if (grid.TryGetValue(key: new DworkCell(X: cell.X + dx, Y: cell.Y + dy, Z: cell.Z + dz), value: out List<int>? bucket))
                            for (int i = 0; i < bucket.Count; i++) {
                                DworkSurfacePoint other = chosen[index: bucket[index: i]];
                                double limit = Math.Max(val1: candidate.Radius, val2: other.Radius);
                                if (candidate.Point.DistanceToSquared(other: other.Point) < limit * limit) return true;
                            }
            return false;
        }
        private void Add(DworkSurfacePoint sample) {
            int index = chosen.Count;
            chosen.Add(item: sample);
            active.Add(item: index);
            radiusBand = (Math.Min(val1: radiusBand.Min, val2: sample.Radius), Math.Max(val1: radiusBand.Max, val2: sample.Radius));
            DworkCell cell = CellOf(point: sample.Point);
            if (!grid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; grid.Add(key: cell, value: bucket); }
            bucket.Add(item: index);
        }
        private DworkCell CellOf(Point3d point) =>
            new(X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize), Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize), Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
        private Fin<SampleSelection> Selection() {
            DworkReceipt dwork = new(Domain: DworkSamplingDomain.ContinuousMesh, RMin: minRadius, BackgroundCellSize: Some(cellSize), BackgroundGridCells: Some(grid.Count),
                AttemptsPerActive: attempts, GeneratedCandidates: tally.Proposals, ActivePops: tally.ActivePops,
                RejectedTooClose: tally.TooClose, RejectedDomain: tally.Domain, LocalRadiusMin: radiusBand.Min, LocalRadiusMax: radiusBand.Max);
            return Fin.Succ(new SampleSelection(Points: [.. chosen.Select(static sample => sample.Point)], Mass: Option<Arr<double>>.None,
                DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None,
                Algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.DworkVariableDensity, Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(tally.Proposals),
                    Attempts: Some(attempts), ActivePops: Some(tally.ActivePops), RejectedTooClose: Some(tally.TooClose), RejectedDomain: Some(tally.Domain),
                    LocalRadiusMin: Some(radiusBand.Min), LocalRadiusMax: Some(radiusBand.Max), Dwork: Some(dwork)))));
        }
    }

    // Bridson active-list Poisson over a candidate cloud: annulus [r, 2r], deterministic draws, maximal
    // coverage flagged when the active list exhausts — statement-kernel exemption.
    private static Fin<SampleSelection> PoissonDiskSelection(Seq<SampleCandidate> candidates, double radius, int attempts, int seed, Op key) {
        if (candidates.IsEmpty || radius <= 0.0 || attempts < 1) return Fin.Fail<SampleSelection>(key.InvalidInput());
        (double r2, double r4) = (radius * radius, 4.0 * radius * radius);
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count).OrderBy(i => Deterministic.OrderKey(point: candidates[index: i].Point, seed: seed))];
        List<int> chosen = [order[0]];
        List<int> active = [order[0]];
        (int activePops, int tooClose, int outside) = (0, 0, 0);
        while (active.Count > 0) {
            int activeOffset = (int)(Deterministic.OrderKey(point: candidates[index: active[0]].Point, seed: seed + activePops) % (ulong)active.Count);
            int parent = active[activeOffset];
            bool accepted = false;
            for (int attempt = 0; attempt < attempts && !accepted; attempt++) {
                int candidate = order[(int)(Deterministic.OrderKey(point: candidates[index: parent].Point, seed: seed + activePops + attempt + 1) % (ulong)order.Length)];
                double fromParent = candidates[index: parent].Point.DistanceToSquared(other: candidates[index: candidate].Point);
                if (fromParent < r2 || fromParent > r4) { outside++; continue; }
                if (chosen.Exists(index => candidates[index: index].Point.DistanceToSquared(other: candidates[index: candidate].Point) < r2)) { tooClose++; continue; }
                chosen.Add(item: candidate);
                active.Add(item: candidate);
                accepted = true;
            }
            if (!accepted) active.RemoveAt(index: activeOffset);
            activePops++;
        }
        return SelectionOf(candidates: candidates, indices: [.. chosen.Distinct()],
            algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.BridsonActiveListPoisson, Seed: Some(seed), Radius: Some(radius), Attempts: Some(attempts),
                MaximalCoverageGuaranteed: active.Count == 0, ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(outside))), key: key);
    }
    private static Fin<SampleSelection> CapacityCvtSelection(Seq<SampleCandidate> candidates, int count, int limit, int iterations, double tolerance, Op key) =>
        RelaxationSample(candidates: candidates, count: count, iterations: iterations, capacity: Some(limit), key: key).Bind(indices => {
            (double residual, int assigned, int unassigned) = CapacityResidualOf(candidates: candidates, sites: indices, limit: limit);
            return SelectionOf(candidates: candidates, indices: indices,
                algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.CapacityLimitedLloydCandidate, TargetCount: Some(count), CapacityResidual: Some(residual),
                    CapacityResidualValidated: unassigned == 0 && residual <= tolerance, CapacityAssignedCandidates: Some(assigned), CapacityUnassignedCandidates: Some(unassigned))), key: key);
        });
    private static (double Residual, int Assigned, int Unassigned) CapacityResidualOf(Seq<SampleCandidate> candidates, int[] sites, int limit) {
        if (candidates.IsEmpty || sites.Length == 0 || limit < 1) return (Residual: 1.0, Assigned: 0, Unassigned: candidates.Count);
        int[] fill = new int[sites.Length];
        (int assigned, int rejected) = (0, 0);
        for (int i = 0; i < candidates.Count; i++) {
            int slot = Enumerable.Range(start: 0, count: sites.Length)
                .Where(s => fill[s] < limit)
                .Select(s => (Index: s, Distance: candidates[index: i].Point.DistanceToSquared(other: candidates[index: sites[s]].Point)))
                .DefaultIfEmpty((Index: -1, Distance: double.PositiveInfinity))
                .Aggregate((best, item) => item.Distance < best.Distance ? item : best).Index;
            if (slot < 0) rejected++; else { fill[slot]++; assigned++; }
        }
        return (Residual: (double)rejected / candidates.Count, Assigned: assigned, Unassigned: rejected);
    }
    // Yuksel weighted sample elimination: (1 - d/dMax)^alpha weights, dMin = dMax (1 - (n/N)^gamma) beta,
    // max-weight removal with neighbor decrement — statement-kernel exemption.
    private static Fin<(int[] Indices, SampleAlgorithmReceipt Algorithm)> SampleElimination(Seq<SampleCandidate> candidates, int count, double alpha, double beta, double gamma, int seed, Option<(int Dimensions, double Measure)> domainMeasure, Op key) {
        SampleCandidate[] input = [.. candidates.AsIterable()];
        (int dimensions, double measure) = domainMeasure.IfNone(BoundingMeasure(candidates: candidates));
        double dMax = 2.0 * (dimensions == 3 ? Math.Pow(x: measure / count / (4.0 * Math.Sqrt(d: 2.0)), y: 1.0 / 3.0) : Math.Sqrt(d: measure / count / (2.0 * Math.Sqrt(d: 3.0))));
        double dMin = dMax * (1.0 - Math.Pow(x: (double)count / input.Length, y: gamma)) * beta;
        if (input.Length <= count || count <= 0 || !double.IsFinite(dMax) || dMax <= 0.0 || !double.IsFinite(dMin) || dMin < 0.0)
            return Fin.Fail<(int[] Indices, SampleAlgorithmReceipt Algorithm)>(key.InvalidInput());
        bool[] active = [.. Enumerable.Repeat(element: true, count: input.Length)];
        double[] weights = new double[input.Length];
        double dMaxSq = dMax * dMax;
        (int Left, int Right, double Weight)[] edges = [..
            from i in Enumerable.Range(start: 0, count: input.Length - 1)
            from j in Enumerable.Range(start: i + 1, count: input.Length - i - 1)
            let distanceSq = input[i].Point.DistanceToSquared(other: input[j].Point)
            let distance = Math.Max(val1: Math.Sqrt(d: distanceSq), val2: dMin)
            where distanceSq <= dMaxSq
            select (Left: i, Right: j, Weight: Math.Pow(x: Math.Max(val1: 0.0, val2: 1.0 - (distance / dMax)), y: alpha))];
        for (int edge = 0; edge < edges.Length; edge++) { weights[edges[edge].Left] += edges[edge].Weight; weights[edges[edge].Right] += edges[edge].Weight; }
        (int neighborUpdates, int activeCount, int eliminated) = (0, input.Length, 0);
        while (activeCount > count) {
            int remove = Enumerable.Range(start: 0, count: input.Length)
                .Where(i => active[i])
                .Select(i => (Index: i, Weight: weights[i], Key: Deterministic.OrderKey(point: input[i].Point, seed: seed)))
                .Aggregate((best, item) => item.Weight > best.Weight || (item.Weight == best.Weight && item.Key < best.Key) ? item : best)
                .Index;
            active[remove] = false;
            activeCount--;
            eliminated++;
            for (int edge = 0; edge < edges.Length; edge++) {
                int other = edges[edge].Left == remove ? edges[edge].Right : edges[edge].Right == remove ? edges[edge].Left : -1;
                if (other >= 0 && active[other]) { weights[other] -= edges[edge].Weight; neighborUpdates++; }
            }
        }
        return Fin.Succ<(int[] Indices, SampleAlgorithmReceipt Algorithm)>((
            Indices: [.. Enumerable.Range(start: 0, count: input.Length).Where(i => active[i]).OrderBy(i => Deterministic.OrderKey(point: input[i].Point, seed: seed))],
            Algorithm: new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.YukselWeightedSampleElimination, Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(input.Length),
                OversampleFactor: Some(input.Length / Math.Max(val1: 1, val2: count)), Alpha: Some(alpha), Beta: Some(beta), Gamma: Some(gamma),
                Radius: Some(dMax), WeightLimitRadius: Some(dMin), Eliminated: Some(eliminated), NeighborUpdates: Some(neighborUpdates))));
    }
    private static (int Dimensions, double Measure) BoundingMeasure(Seq<SampleCandidate> candidates) {
        BoundingBox box = new(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        (double dx, double dy, double dz) = (Math.Max(val1: box.Max.X - box.Min.X, val2: 0.0), Math.Max(val1: box.Max.Y - box.Min.Y, val2: 0.0), Math.Max(val1: box.Max.Z - box.Min.Z, val2: 0.0));
        double volume = dx * dy * dz;
        double area = Math.Max(val1: dx * dy, val2: Math.Max(val1: dx * dz, val2: dy * dz));
        return volume > double.Epsilon ? (3, volume) : (2, Math.Max(val1: area, val2: double.Epsilon));
    }
    private static int[] FarthestIndices(Seq<SampleCandidate> candidates, int count) {
        if (candidates.IsEmpty || count < 1) return [];
        int total = candidates.Count;
        int actualCount = Math.Min(val1: count, val2: total);
        int[] chosen = new int[actualCount];
        bool[] selected = new bool[total];
        Point3d centroid = toSeq(Enumerable.Range(start: 0, count: total))
            .Fold(initialState: Point3d.Origin, f: (acc, i) => new Point3d(x: acc.X + candidates[index: i].Point.X, y: acc.Y + candidates[index: i].Point.Y, z: acc.Z + candidates[index: i].Point.Z))
            switch { Point3d sum => new Point3d(x: sum.X / total, y: sum.Y / total, z: sum.Z / total) };
        chosen[0] = Enumerable.Range(start: 0, count: total)
            .Select(i => (Index: i, Distance: candidates[index: i].Point.DistanceToSquared(other: centroid)))
            .Aggregate((best, item) => item.Distance > best.Distance ? item : best).Index;
        selected[chosen[0]] = true;
        double[] minDistSq = [.. Enumerable.Range(start: 0, count: total).Select(i => candidates[index: i].Point.DistanceToSquared(other: candidates[index: chosen[0]].Point))];
        for (int pick = 1; pick < actualCount; pick++) {
            int farthest = Enumerable.Range(start: 0, count: total).Where(i => !selected[i]).Aggregate((best, i) => minDistSq[i] > minDistSq[best] ? i : best);
            chosen[pick] = farthest;
            selected[farthest] = true;
            for (int i = 0; i < total; i++) minDistSq[i] = Math.Min(val1: minDistSq[i], val2: candidates[index: i].Point.DistanceToSquared(other: candidates[index: farthest].Point));
        }
        return chosen;
    }
    private static int[] FpoSample(Seq<SampleCandidate> candidates, int count, int iterations) {
        int[] chosen = FarthestIndices(candidates: candidates, count: count);
        if (chosen.Length < 2) return chosen;
        double bestScore = WorstCoverage(candidates: candidates, chosen: chosen).Distance;
        bool improved = true;
        for (int iter = 0; iter < iterations && improved; iter++) {
            improved = false;
            int replacement = WorstCoverage(candidates: candidates, chosen: chosen).Index;
            for (int i = 0; i < chosen.Length && !improved; i++) {
                if (chosen.Contains(value: replacement)) continue;
                int previous = chosen[i];
                chosen[i] = replacement;
                double score = WorstCoverage(candidates: candidates, chosen: chosen).Distance;
                if (score < bestScore) { bestScore = score; improved = true; }
                if (!improved) chosen[i] = previous;
            }
        }
        return chosen;
    }
    private static (int Index, double Distance) WorstCoverage(Seq<SampleCandidate> candidates, int[] chosen) =>
        candidates.Count <= 0 || chosen.Length <= 0
            ? (0, -1.0)
            : Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Distance: chosen.Min(c => candidates[index: i].Point.DistanceToSquared(other: candidates[index: c].Point))))
                .Aggregate((worst, item) => item.Distance > worst.Distance ? item : worst);
    private static Fin<int[]> RelaxationSample(Seq<SampleCandidate> candidates, int count, int iterations, Option<int> capacity, Op key) {
        int total = capacity.Map(limit => Math.Min(val1: candidates.Count, val2: count * limit)).IfNone(candidates.Count);
        Seq<SampleCandidate> active = total == candidates.Count ? candidates : toSeq(Enumerable.Take(source: candidates.AsIterable(), count: total));
        return toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
            initialState: Fin.Succ(FarthestIndices(candidates: active, count: count)),
            f: (state, _) => state.Bind(sites => RelaxSites(sites: sites, candidates: active, total: active.Count, capacity: capacity, key: key)));
    }
    private static Fin<int[]> RelaxSites(int[] sites, Seq<SampleCandidate> candidates, int total, Option<int> capacity, Op key) {
        if (sites.Length == 0) return Fin.Succ(sites);
        Vector3d[] sums = new Vector3d[sites.Length];
        int[] counts = new int[sites.Length];
        int[] siteFill = new int[sites.Length];
        PointCloud siteIndex = [];
        siteIndex.AddRange(points: sites.Select(i => candidates[index: i].Point));
        for (int i = 0; i < total; i++) {
            int closest = capacity.Match(
                Some: limit => {
                    (int hit, double best) = (-1, double.MaxValue);
                    for (int s = 0; s < sites.Length; s++) {
                        if (siteFill[s] >= limit) continue;
                        double distance = candidates[index: i].Point.DistanceToSquared(other: candidates[index: sites[s]].Point);
                        if (distance < best) { best = distance; hit = s; }
                    }
                    return hit;
                },
                None: () => siteIndex.ClosestPoint(testPoint: candidates[index: i].Point));
            if (closest < 0) return Fin.Fail<int[]>(key.InvalidResult());
            siteFill[closest]++;
            sums[closest] += (Vector3d)candidates[index: i].Point;
            counts[closest]++;
        }
        PointCloud candidateIndex = [];
        candidateIndex.AddRange(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        return toSeq(Enumerable.Range(start: 0, count: sites.Length)).Fold(
            initialState: Fin.Succ(sites),
            f: (state, s) => counts[s] <= 0
                ? state
                : state.Bind(active => (candidateIndex.ClosestPoint(testPoint: Point3d.Origin + (sums[s] / counts[s])) switch {
                    int nearest when nearest >= 0 && nearest < candidates.Count => Fin.Succ(nearest),
                    _ => Fin.Fail<int>(key.InvalidResult()),
                }).Map(nearest => { active[s] = nearest; return active; })));
    }
}
```
