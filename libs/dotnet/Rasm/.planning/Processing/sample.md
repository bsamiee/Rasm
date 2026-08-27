# [RASM_VECTORS_SAMPLE]

`SampleKind` owns point sampling: every draw is one closed `[Union]` case admitted at its factory, and `SampleKernel.Sample` folds all cases through one domain dispatch total over admitted kinds. `PowerCcvtPolicy.Preset` mints the BNOT tuning surface from trusted declaration values through the generated `Create` factories and admits its cross-field clauses once on the admission gate, one `with` mutation overrides it, and every convergence threshold reads its own `ToleranceLane` at the run.

Rebuild work composes settled owners: `extract.md` `ExtractionDomain` carries the domain axis, `evaluation.md` `Evaluate(EvaluationRequest.Sample(...))` the support-space candidate draw, `matrix.md` `SparseMatrix.SingularSolveDetailed` the gauge-fixed solve, `segment.md` `SegmentKernel.ValidateSamplingSpectrum` the blue-noise witness, `mesh.md` `RestrictedPowerDiagram` the restricted power cells this page reads one-directionally, and `identity.md` `Deterministic` every draw addresses through a declared `SampleLane` ordinal.

## [01]-[INDEX]

- [02]-[SAMPLING]: `SampleKind` mints the draw vocabulary `SampleKernel` folds onto every extraction domain.
- [03]-[POWER_CCVT]: `PowerCcvtRun` solves the continuous BNOT sampler under one flat policy value.

## [02]-[SAMPLING]

- Owner: `SampleKind` `[Union]` mints every draw case, `SampleLane` the declared draw-ordinal roster, and `SampleAssurance` the guarantee vocabulary `SampleAlgorithm` publishes.
- Cases: `SampleKind` closes the roster of published blue-noise and CVT algorithms this page realizes (Bridson, Yuksel elimination, Dwork variable-density and its adaptive sibling, de Goes BNOT, capacity-limited Lloyd, farthest-point and its optimizing variant), each case's `Facts` the oversample scale its selection needs; the ambient rank rides the domain measure as a bare planar-or-volumetric integer the kernel's spacing and elimination-radius formulas read. Upstream is the sampling literature, and a case without a suite arm cannot construct.
- Entry: each case factory admits raw scalars through `FactoryBridge.Accept` under the case's own `Admit` invariants; `Project<TOut>` is the one evaluation entry, `TOut` selecting the output shape.
- Auto: `SampleKernel.Sample` discriminates on domain shape alone — supplied points project, generated kinds draw an oversampled candidate pool the selection suite reduces — and a kind whose candidates a domain cannot supply stays typed `Unsupported`.
- Law: `SampleKernel.Trial` is the ONE bounded rejection fold — an attempt budget, a proposal, and a caller-shaped tally. Every rejection sampler on this page instantiates it, a drawn state is the explicit committed settlement fact, and an exhausted budget yields `None`, which each caller lowers to its own typed terminal instead of a success-shaped fall-through. Every other bounded iterate on the page — the outer CCVT schedule, the dual Newton, the farthest-point-optimization swap — rides the same owner.
- Law: a draw addresses a DECLARED `SampleLane` ordinal, never a hand-packed salt or a seed the caller offsets — `Deterministic.Draw.At(lanes)` keys the multi-level draws (active-list pick, per-attempt annulus pick, barycentric coordinate) and `Deterministic.UnitInterval(point, lane.Lane, seed)` keys the position-stable exponential races off the ONE lane ordinal, so two samplers under one seed never interleave and no draw index collides with another's.
- Output: `SampleTally` nests `SampleAlgorithm`, so every algorithm's facts ride one evidence stream, never a parallel type per algorithm; `SampleAlgorithm.Kind` carries the admitted `SampleKind`, so the case IS the algorithm identity and no roster mirrors it, and request-owned scalars read off that case rather than a copied column; every successful selection carries its algorithm fact bare, so `SampleTally.Algorithm` is never optional; the sampling spectrum has ONE seat, `SampleAlgorithm.Spectrum`, which `SegmentKernel.ValidateSamplingSpectrum` writes for every mesh-sampled case, CCVT included; spacing rides ONE `Distribution<Scalar>` over nearest-neighbour distances rather than a min/mean/max triple beside an all-pairs mean that measured a different quantity under the same word; `SampleAlgorithm.Assurances` is the ONE guarantee column over `SampleAssurance`, replacing four independent bool flags whose corners the roster fixes; terminal classification is a projection of the one tally — all-rejected is `Emitted == 0`, a shortfall is `Emitted` under the case's `Facts.Count`, capacity refusal is the capacity case without `CapacityResidual` assurance — never a stored stop or status row.
- Packages: RhinoCommon is the one boundary-admitted host surface; every other member composes the `Rasm` substrate.
- Growth: a new algorithm is one `SampleKind` case and one suite arm the total `Switch` breaks on; a new per-algorithm fact is one `SampleAlgorithm` field; a new guarantee is one `SampleAssurance` row; a new draw purpose is one `SampleLane` row; a new candidate domain ripens one `ExtractionDomain` case into a dispatch arm.
- Exemption: the candidate-suite kernels are the named statement-kernel exemption — hot spatial loops with typed egress. Their background `Dictionary` hashes survive only where the set GROWS per admission: `NeighborIndex` publishes no incremental insert, so a Bridson active-list frontier cannot compose it without rebuilding per accepted sample, while every FROZEN point set — the candidate cloud, the alias mask, the conflict graph, every spacing read — routes the one neighbourhood owner. `SampleKernel` folds every reference spacing scale — hexagonal reference, nearest-neighbour distribution, normalized Poisson radius — from the domain measure, never an absolute literal; a candidate shortfall lands as the tally's `Emitted` against the requested count, never a terminal row.

## [03]-[POWER_CCVT]

- Owner: `PowerCcvtPolicy` composes every BNOT tuning axis into one flat policy value — capacity Newton budget, motion sweeps and steps, the two motion tolerances, the one Armijo triple (`Backtrack`, `InitialStep`, `MaxHalvings`) serving both the dual-Newton and the site-motion ascent, and the regularity break's `AliasScale`, `JitterScale`, and `RelocateFraction` — with no nested policy shell; the dual solve composes the `matrix.md` `GaugePolicy` directly off the policy's optional `PinnedSite` — absence selects the mean-zero constant gauge, a present site selects `GaugePolicy.Pinned` on it with no post-shift — so the Hessian nullspace fix is one admitted pin coordinate, never a second gauge vocabulary.
- Entry: `SampleKind.PowerCcvt` mints `PowerCcvtPolicy.Preset(key)`; a preset `with { … }` re-admitted through `policy.Admit` is the whole override surface, and a throwing factory inside a static initializer is the deleted form.
- Auto: `PowerCcvtRun` executes BNOT on the mesh — capacity Newton enforces cell mass through the power-graph Laplacian under the selected gauge, then two-phase site motion runs Lloyd sweeps into Armijo transport-energy ascent, both loops stopping on scale-relative tolerances floored by their own `ToleranceLane` rows; its terminal breaks lattice regularity once and lifts every site to the surface.
- Law: ONE generic `Armijo<T>` search serves the site-motion ascent and the dual Newton alike — the caller hands in a trial arrow from a step size to its rebuilt state and objective, the search walks the backtracking budget and keeps the first sufficient improvement, and an exhausted budget is a typed `Fin` failure, never the last non-improving candidate certified as a step.
- Output: `PowerCcvtSolution` folds the run's evidence, the power-cell census, and the composed solve child into one flat stream — fragment count, facet count, cell mass, and integration residual are columns on the solution, never a nested wrapper, and cell mass and dual weights each ride ONE `Stat<Scalar>` off their own column — and its `IsValid` witnesses the solve's `GaugeShift` — a mean-zero post-shift on an unpinned solve, none owed on a pinned one — so the gauge claim is a validity clause, never a forwarding property, and convergence rides one `Converged` column the outer schedule already holds. Rebuilds are counted at the one rebuild owner and sites the surface refuses to accept census on `UnliftedSiteCount`.
- Growth: a gauge choice is the `PinnedSite` coordinate on the policy, composing the `matrix.md` `GaugePolicy` cases directly; a new motion schedule, line-search variant, or density-transport variant is one column on the flat policy read by the same run; jitter is the one `JitterScale` column, since only the product of a variance and a magnitude ever reached the run; a new convergence gate is one `ToleranceLane` read, never a stored epsilon.
- Boundary: the per-iteration diagram rebuild, triplet assembly, and Armijo trial arrows are the named statement-kernel exemption while the outer schedules stay domain flow; continuous BNOT transport is its own estimator, distinct from the `transport.md` discrete Sinkhorn plan.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Collections;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using Dimension = Rasm.Numerics.Dimension;
using IndexSet = System.Collections.Generic.HashSet<int>;

namespace Rasm.Processing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<long>(KeyMemberName = nameof(IDrawLane<SampleLane>.Lane))]
public sealed partial class SampleLane : IDrawLane<SampleLane> {
    public static readonly SampleLane Priority    = new(0L);
    public static readonly SampleLane Active      = new(1L);
    public static readonly SampleLane Annulus     = new(2L);
    public static readonly SampleLane Area        = new(3L);
    public static readonly SampleLane Barycentric = new(4L);
    public static readonly SampleLane Jitter      = new(5L);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SampleAssurance : ICapability<SampleAssurance> {
    public static readonly SampleAssurance MaximalCoverage     = new("maximal-coverage", rank: 0);
    public static readonly SampleAssurance TransportAssignment = new("transport-assignment", rank: 1);
    public static readonly SampleAssurance MeshSpectrum        = new("mesh-spectrum", rank: 2);
    public static readonly SampleAssurance CapacityResidual    = new("capacity-residual", rank: 3);

    public int Rank { get; }
}

[Union]
public abstract partial record SampleKind {
    public sealed record ExplicitCase(Seq<Point3d> Points) : SampleKind;
    public sealed record PoissonDiskCase(PositiveMagnitude Radius, Dimension Attempts, int Seed) : SampleKind;
    public sealed record FarthestCase(Dimension Count) : SampleKind;
    public sealed record OptimizeCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record CapacityCase(Dimension Count, Dimension Limit, Dimension Iterations) : SampleKind;
    public sealed record WeightedCase(Seq<(Point3d Point, double Mass)> Points) : SampleKind;
    public sealed record ScalarDensityCase(ScalarField Density, Dimension Count, int Seed) : SampleKind;
    public sealed record AdaptiveCase(ScalarField Density, Dimension Count, PositiveMagnitude MinSpacing, int Seed) : SampleKind;
    public sealed record SampleEliminationCase(Dimension Count, Dimension OversampleFactor, PositiveMagnitude Alpha, PositiveMagnitude Beta, PositiveMagnitude Gamma, int Seed) : SampleKind;
    public sealed record DworkVariableDensityCase(ScalarField Radius, Dimension Count, PositiveMagnitude MinRadius, Dimension Attempts, int Seed) : SampleKind;
    public sealed record PowerCcvtCase(Dimension Count, PowerCcvtPolicy Policy) : SampleKind;
    private SampleKind() { }

    public static Fin<SampleKind> Explicit(Seq<Point3d> points) =>
        new ExplicitCase(Points: points).Admit();
    public static Fin<SampleKind> PoissonDisk(double radius, int attempts = 30, int seed = 0) {
        return from r in FactoryBridge.Accept<PositiveMagnitude>(candidate: radius)
               from a in FactoryBridge.Accept<Dimension>(candidate: attempts)
               from admitted in new PoissonDiskCase(Radius: r, Attempts: a, Seed: seed).Admit()
               select admitted;
    }
    public static Fin<SampleKind> Farthest(int count) {
        return FactoryBridge.Accept<Dimension>(candidate: count).Bind(value => new FarthestCase(Count: value).Admit());
    }
    public static Fin<SampleKind> Optimize(int count, int iterations) =>
        Counted(count: count, value: iterations, create: static (c, i) => new OptimizeCase(Count: c, Iterations: i));
    public static Fin<SampleKind> Lloyd(int count, int iterations) =>
        Counted(count: count, value: iterations, create: static (c, i) => new LloydCase(Count: c, Iterations: i));
    public static Fin<SampleKind> Capacity(int count, int capacity, int iterations = 8) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from limit in FactoryBridge.Accept<Dimension>(candidate: capacity)
               from iter in FactoryBridge.Accept<Dimension>(candidate: iterations)
               from admitted in new CapacityCase(Count: c, Limit: limit, Iterations: iter).Admit()
               select admitted;
    }
    public static Fin<SampleKind> Weighted(Seq<(Point3d Point, double Mass)> points) =>
        new WeightedCase(Points: points).Admit();
    public static Fin<SampleKind> ScalarDensity(ScalarField density, int count, int seed) {
        return FactoryBridge.Accept<Dimension>(candidate: count).Bind(c => new ScalarDensityCase(Density: density, Count: c, Seed: seed).Admit());
    }
    public static Fin<SampleKind> Adaptive(ScalarField density, int count, double minSpacing, int seed) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from spacing in FactoryBridge.Accept<PositiveMagnitude>(candidate: minSpacing)
               from admitted in new AdaptiveCase(Density: density, Count: c, MinSpacing: spacing, Seed: seed).Admit()
               select admitted;
    }
    public static Fin<SampleKind> SampleElimination(int count, int oversampleFactor, double alpha, double beta, double gamma, int seed) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from oversample in FactoryBridge.Accept<Dimension>(candidate: oversampleFactor)
               from a in FactoryBridge.Accept<PositiveMagnitude>(candidate: alpha)
               from b in FactoryBridge.Accept<PositiveMagnitude>(candidate: beta)
               from g in FactoryBridge.Accept<PositiveMagnitude>(candidate: gamma)
               from admitted in new SampleEliminationCase(Count: c, OversampleFactor: oversample, Alpha: a, Beta: b, Gamma: g, Seed: seed).Admit()
               select admitted;
    }
    public static Fin<SampleKind> DworkVariableDensity(ScalarField radius, int count, double minRadius, int attempts = 30, int seed = 0) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from min in FactoryBridge.Accept<PositiveMagnitude>(candidate: minRadius)
               from a in FactoryBridge.Accept<Dimension>(candidate: attempts)
               from admitted in new DworkVariableDensityCase(Radius: radius, Count: c, MinRadius: min, Attempts: a, Seed: seed).Admit()
               select admitted;
    }
    public static Fin<SampleKind> PowerCcvt(int count, Option<PowerCcvtPolicy> policy = default) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from active in policy.Match(Some: held => held.Admit(), None: () => PowerCcvtPolicy.Preset())
               from admitted in new PowerCcvtCase(Count: c, Policy: active).Admit()
               select admitted;
    }

    internal Fin<SampleKind> Admit() => Switch(
        state: key,
        explicitCase: static (c) => c.Points.IsEmpty ? Fin.Fail<SampleKind>(new KernelFault.InvalidInput()) : Fin.Succ<SampleKind>(c),
        poissonDiskCase: static (_, c) => Fin.Succ<SampleKind>(c),
        farthestCase: static (_, c) => Fin.Succ<SampleKind>(c),
        optimizeCase: static (_, c) => Fin.Succ<SampleKind>(c),
        lloydCase: static (_, c) => Fin.Succ<SampleKind>(c),
        capacityCase: static (_, c) => Fin.Succ<SampleKind>(c),
        weightedCase: static (c) => c.Points.IsEmpty
            ? Fin.Fail<SampleKind>(new KernelFault.InvalidInput())
            : CloudKernel.MassOf(mass: new Arr<double>([.. c.Points.AsIterable().Select(static item => item.Mass)]), count: c.Points.Count).Map(_ => (SampleKind)c),
        scalarDensityCase: static (c) => Admit.Need(c.Density).Map(_ => (SampleKind)c),
        adaptiveCase: static (c) => Admit.Need(c.Density).Map(_ => (SampleKind)c),
        sampleEliminationCase: static (c) => guard(c.OversampleFactor.Value > 1 && c.Beta.Value <= 1.0, new KernelFault.InvalidInput()).ToFin().Map(_ => (SampleKind)c),
        dworkVariableDensityCase: static (c) => Admit.Need(c.Radius).Map(_ => (SampleKind)c),
        powerCcvtCase: static (c) => c.Policy.Admit().Map(_ => (SampleKind)c));
    internal static Fin<SampleKind> Admit(SampleKind value) =>
        Admit.Need(value).Bind(kind => kind.Admit());

    internal Fin<SampleKernel.Result> Evaluate(ExtractionDomain domain, Context context) =>
        Admit().Bind(kind => SampleKernel.Sample(kind: kind, domain: domain, context: context));
    internal (Option<int> Count, Option<int> Iterations, double CandidateScale) Facts => Switch(
        explicitCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
        poissonDiskCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
        farthestCase: static c => (Some(c.Count.Value), Option<int>.None, 8.0),
        optimizeCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), 8.0),
        lloydCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), 8.0),
        capacityCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value),
        weightedCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
        scalarDensityCase: static c => (Some(c.Count.Value), Option<int>.None, 8.0),
        adaptiveCase: static c => (Some(c.Count.Value), Option<int>.None, 12.0),
        sampleEliminationCase: static c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value),
        dworkVariableDensityCase: static c => (Some(c.Count.Value), Option<int>.None, 12.0),
        powerCcvtCase: static c => (Some(c.Count.Value), Some(c.Policy.Iterations.Value), 8.0));
    internal Fin<double> MeshCandidateDensity(double area) {
        double safeArea = Math.Max(val1: area, val2: EpsilonPolicy.ZeroTolerance);
        double target = SwitchPartially(
            state: (SafeArea: safeArea, Facts: Facts),
            @default: static (state, _) => state.Facts.Count.Map(value => value * Math.Max(1.0, state.Facts.CandidateScale)).IfNone(0.0),
            poissonDiskCase: static (state, pd) => state.SafeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: EpsilonPolicy.ZeroTolerance));
        return double.IsFinite(target) && target > 0.0
            ? Acceptance.Value(value: Math.Max(val1: target / safeArea, val2: 1.0 / safeArea))
            : Fin.Fail<double>(new KernelFault.Unsupported(InputType: GetType(), OutputType: typeof(SampleKernel.Result)));
    }
    public Fin<TOut> Project<TOut>(ExtractionDomain domain, Context context) {
        return from result in Evaluate(domain: domain, context: context)
               from output in ResultProjection.Rows<SampleTally, TOut>(self: result.Tally, owner: typeof(SampleKind),
                   ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(result.Points)),
                   ProjectionRow.Of<VectorCloud>(() => result.Mass.Match(
                       Some: mass => VectorCloud.Cluster(points: result.Points, context: context, mass: Some(mass)),
                       None: () => VectorCloud.Cluster(points: result.Points, context: context))))
               select output;
    }
    private static Fin<SampleKind> Counted(int count, int value, Func<Dimension, Dimension, SampleKind> create) {
        return from c in FactoryBridge.Accept<Dimension>(candidate: count)
               from v in FactoryBridge.Accept<Dimension>(candidate: value)
               from admitted in create(c, v).Admit()
               select admitted;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct DworkCensus(
    bool CandidateOnly, double RMin, double BackgroundCellSize, int BackgroundGridCells,
    int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain,
    double LocalRadiusMin, double LocalRadiusMax) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(RMin), ValidityClaim.Positive(BackgroundCellSize), ValidityClaim.CountAtLeast(BackgroundGridCells, 0),
        ValidityClaim.CountAtLeast(AttemptsPerActive, 1), ValidityClaim.CountAtLeast(GeneratedCandidates, 0),
        ValidityClaim.CountAtLeast(ActivePops, 0), ValidityClaim.CountAtLeast(RejectedTooClose, 0),
        ValidityClaim.CountAtLeast(RejectedDomain, 0), ValidityClaim.Ordered(LocalRadiusMin, LocalRadiusMax));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCcvtSolution(
    int SiteCount, double TargetMass,
    double CapacityResidualInf, double CapacityResidualL1, double CapacityResidualL2, double CapacityResidualNormalized,
    int OuterIterations, int LloydIterations, int GradientIterations, int DualNewtonIterations,
    Stat<Scalar> Weights, double TransportEnergy, double TransportEnergyDelta,
    double DualObjective, double CentroidShift, double PositionGradientNorm, double WeightGradientNorm,
    int EmptyCellCount, int StepHalvingCount, int RebuildCount, int AliasedSiteCount, int RelocatedSiteCount, int UnliftedSiteCount,
    double NormalizedPoissonRadius, double PlanarityDeviation, Option<int> PinnedSite, bool Converged,
    int FragmentCount, int FacetCount, Stat<Scalar> CellMass, double IntegrationResidual,
    Option<LinearSolution> DualSolve = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.Positive(TargetMass),
        ValidityClaim.Nonnegative(CapacityResidualInf), ValidityClaim.Nonnegative(CapacityResidualL1),
        ValidityClaim.Nonnegative(CapacityResidualL2), ValidityClaim.Nonnegative(CapacityResidualNormalized),
        ValidityClaim.CountAtLeast(OuterIterations, 0), ValidityClaim.CountAtLeast(LloydIterations, 0),
        ValidityClaim.CountAtLeast(GradientIterations, 0), ValidityClaim.CountAtLeast(DualNewtonIterations, 0),
        ValidityClaim.Evidence(Weights),
        ValidityClaim.Finite(TransportEnergy), ValidityClaim.Finite(TransportEnergyDelta), ValidityClaim.Finite(DualObjective),
        ValidityClaim.Nonnegative(CentroidShift), ValidityClaim.Nonnegative(PositionGradientNorm), ValidityClaim.Nonnegative(WeightGradientNorm),
        ValidityClaim.CountAtLeast(StepHalvingCount, 0), ValidityClaim.CountAtLeast(RebuildCount, 0),
        ValidityClaim.CountAtLeast(AliasedSiteCount, 0), ValidityClaim.CountAtLeast(RelocatedSiteCount, 0), ValidityClaim.CountAtLeast(UnliftedSiteCount, 0),
        ValidityClaim.UnitInterval(NormalizedPoissonRadius), ValidityClaim.Nonnegative(PlanarityDeviation),
        ValidityClaim.CountAtLeast(FragmentCount, 0), ValidityClaim.CountAtLeast(FacetCount, 0),
        ValidityClaim.CountAtLeast(EmptyCellCount, 0), EmptyCellCount <= SiteCount,
        ValidityClaim.Evidence(CellMass), ValidityClaim.Nonnegative(IntegrationResidual),
        PinnedSite.IsSome || DualSolve.Bind(static solve => solve.Gauge)
            .Exists(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero)),
        ValidityClaim.Evidence(DualSolve));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SampleAlgorithm(
    SampleKind Kind, CapabilitySet<SampleAssurance> Assurances,
    Option<int> OversampleCount = default,
    Option<double> EliminationRadius = default, Option<double> WeightLimitRadius = default,
    Option<int> Eliminated = default, Option<int> NeighborUpdates = default,
    Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default,
    Option<double> DensityMin = default, Option<double> DensityMax = default, Option<double> LocalRadiusMin = default, Option<double> LocalRadiusMax = default,
    Option<double> CapacityResidual = default, Option<SamplingSpectrum> Spectrum = default, Option<DworkCensus> Dwork = default,
    Option<int> CapacityAssignedCandidates = default, Option<int> CapacityUnassignedCandidates = default,
    Option<int> CandidatePoolTruncatedTo = default, Option<PowerCcvtSolution> PowerCcvt = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Kind is not null,
        OversampleCount.Map(static count => count >= 0).IfNone(noneValue: true),
        ActivePops.Map(static count => count >= 0).IfNone(noneValue: true),
        Eliminated.Map(static count => count >= 0).IfNone(noneValue: true),
        NeighborUpdates.Map(static count => count >= 0).IfNone(noneValue: true),
        RejectedTooClose.Map(static count => count >= 0).IfNone(noneValue: true),
        RejectedDomain.Map(static count => count >= 0).IfNone(noneValue: true),
        CandidatePoolTruncatedTo.Map(static count => count >= 0).IfNone(noneValue: true),
        EliminationRadius.Map(static radius => double.IsFinite(radius) && radius > 0.0).IfNone(noneValue: true),
        WeightLimitRadius.Map(static radius => double.IsFinite(radius) && radius >= 0.0).IfNone(noneValue: true),
        CapacityResidual.Map(static residual => double.IsFinite(residual) && residual >= 0.0).IfNone(noneValue: true),
        DensityMin.Bind(min => DensityMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true),
        LocalRadiusMin.Bind(min => LocalRadiusMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true),
        ValidityClaim.Evidence(Dwork), ValidityClaim.Evidence(Spectrum), ValidityClaim.Evidence(PowerCcvt));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SampleTally(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<int> DensityAccepted, Option<int> DensityRejected, SampleAlgorithm Algorithm) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0), ValidityClaim.CountAtLeast(Rejected, 0),
        Emitted <= Attempted + Rejected,
        ValidityClaim.Evidence(Spacing), ValidityClaim.Evidence(Algorithm));
}

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record PowerCcvtPolicy(
    Dimension Iterations, Option<ScalarField> Density, Dimension MaxNewton,
    Dimension LloydSweeps, Dimension GradientSteps,
    PositiveMagnitude LloydTolerance, PositiveMagnitude GradientTolerance,
    PositiveMagnitude Backtrack, PositiveMagnitude InitialStep, Dimension MaxHalvings,
    PositiveMagnitude AliasScale, PositiveMagnitude JitterScale, PositiveMagnitude RelocateFraction,
    Option<int> PinnedSite, int Seed) {
    internal static Fin<PowerCcvtPolicy> Preset() => new PowerCcvtPolicy(
        Iterations: Dimension.Create(value: 16), Density: None, MaxNewton: Dimension.Create(value: 32),
        LloydSweeps: Dimension.Create(value: 1), GradientSteps: Dimension.Create(value: 8),
        LloydTolerance: PositiveMagnitude.Create(value: 0.01), GradientTolerance: PositiveMagnitude.Create(value: 0.1),
        Backtrack: PositiveMagnitude.Create(value: 0.5), InitialStep: PositiveMagnitude.Create(value: 1.0), MaxHalvings: Dimension.Create(value: 32),
        AliasScale: PositiveMagnitude.Create(value: 0.65), JitterScale: PositiveMagnitude.Create(value: 0.025),
        RelocateFraction: PositiveMagnitude.Create(value: 0.05), PinnedSite: None, Seed: 0).Admit();
    internal Fin<PowerCcvtPolicy> Admit() =>
        guard(Backtrack.Value < 1.0 && RelocateFraction.Value <= 1.0 && PinnedSite.Map(static site => site >= 0).IfNone(noneValue: true), new KernelFault.InvalidInput())
            .ToFin().Map(_ => this);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class SampleKernel {
    internal readonly record struct Result(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleTally Tally);
    private readonly record struct Candidate(Point3d Point, Option<double> Mass);
    private readonly record struct Selection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, SampleAlgorithm Algorithm);

    // --- [SPACING]
    private static double HexagonalSpacing(double measure, int count) =>
        Math.Sqrt(d: 2.0 * measure / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: count)));
    private static Fin<Seq<double>> NearestSpacing(Seq<Point3d> points) =>
        points.Count < 2
            ? Fin.Succ(Seq<double>())
            : from index in NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: points))
              from pair in FactoryBridge.Accept<Dimension>(candidate: 2)
              from graph in NeighborKernel.GraphOf(index: index, needles: [.. points.AsIterable()], count: Some(pair), radius: Option<PositiveMagnitude>.None)
              select toSeq(Enumerable.Range(start: 0, count: points.Count)).Choose(i =>
                  graph.Ids.Length > i
                      ? toSeq(graph.Ids[i]).Find(id => id != i && id >= 0 && id < points.Count).Map(id => points[index: i].DistanceTo(other: points[index: id]))
                      : Option<double>.None);
    private static Fin<Distribution<Scalar>> SpacingDistribution(Seq<Point3d> points) =>
        from nearest in NearestSpacing(points: points)
        from distribution in Distribution<Scalar>.Of(values: nearest.Map(static value => (Scalar)value), percentiles: [])
        select distribution;
    private static Fin<double> MeanNearestSpacing(Seq<Point3d> points, double measure) =>
        NearestSpacing(points: points).Map(nearest =>
            nearest.IsEmpty ? HexagonalSpacing(measure: measure, count: points.Count) : nearest.Sum() / nearest.Count);
    private static Fin<double> NormalizedPoissonRadius(Seq<Point3d> points, double measure) =>
        NearestSpacing(points: points).Map(nearest => {
            if (nearest.IsEmpty) return 0.0;
            double minSpacing = nearest.Min(double.PositiveInfinity);
            double reference = HexagonalSpacing(measure: measure, count: points.Count);
            return double.IsFinite(minSpacing) && double.IsFinite(reference) && reference > EpsilonPolicy.ZeroTolerance
                ? Math.Clamp(value: minSpacing / reference, min: 0.0, max: 1.0)
                : 0.0;
        });

    // --- [TRIAL]
    [StructLayout(LayoutKind.Auto)] internal readonly record struct TrialState<T, TTally>(Option<T> Drawn, TTally Tally, int Attempts);

    internal static TrialState<T, TTally> Trial<T, TTally>(
        Dimension budget, TTally seed, Func<int, TTally, (Option<T> Drawn, TTally Tally)> propose) =>
        Cell.Converge(
            cell: Atom(value: new TrialState<T, TTally>(Drawn: Option<T>.None, Tally: seed, Attempts: 0)),
            step: state => propose(state.Attempts, state.Tally) switch {
                var proposed => Some(new TrialState<T, TTally>(Drawn: proposed.Drawn, Tally: proposed.Tally, Attempts: state.Attempts + 1)),
            },
            settled: static state => state.Drawn.IsSome, budget: budget, declined: new KernelFault.InvalidResult()).Current;

    // --- [DISPATCH]
    internal static Fin<Result> Sample(SampleKind kind, ExtractionDomain domain, Context context) =>
        kind.SwitchPartially(
            state: (Domain: domain, Context: context),
            @default: static (state, value) => state.Domain.Switch(
                state: (Kind: value, state.Context, state.Key),
                supportCase: static (s, d) => SampleGeneratedSupport(s.Kind, d.Value, s.Context, s.Key),
                meshCase: static (s, d) => SampleOnMesh(s.Kind, d.Value, s.Context, s.Key),
                cloudCase: static (s, d) => d.Value.SwitchPartially(
                    state: (s.Kind, s.Context, s.Key),
                    @default: static (held, value) => Fin.Fail<Result>(new KernelFault.Unsupported(InputType: value.GetType(), OutputType: typeof(Result))),
                    clusterCase: static (held, cluster) => CloudKernel.MassOf(cluster: cluster).Bind(mass => SampleOnCandidates(held.Kind,
                        cluster.Vertices.Map((point, index) => new Candidate(point, Some(mass[index]))), false, None, held.Context, held.Key))),
                latticeCase: static (s, d) => SampleOnCandidates(s.Kind,
                    toSeq(Enumerable.Range(0, (int)Math.Min(d.Value.CellCount, int.MaxValue)).Select(linear => d.Value.Coordinate(linear) switch {
                        (int column, int row, int layer) => new Candidate(d.Value.Center(column, row, layer), None),
                    })), true, Some((Rank: d.Value.Rank >= 3 ? 3 : 2, Measure: d.Value.CellCount * d.Value.CellMeasure)), s.Context, s.Key)),
            explicitCase: static (state, value) => SampleAdmitted(value.Points.Map(static point => new Candidate(point, None)), state.Domain, value, state.Context, state.Key),
            weightedCase: static (state, value) => SampleAdmitted(value.Points.Map(static item => new Candidate(item.Point, Some(item.Mass))), state.Domain, value, state.Context, state.Key));

    private static Fin<Result> SampleAdmitted(Seq<Candidate> points, ExtractionDomain domain, SampleKind kind, Context context) =>
        from admitted in points.Fold(
            initialState: Fin.Succ((Accepted: (Seq<Point3d>)[], Mass: (Seq<double>)[], Weighted: false, Rejected: 0)),
            f: (state, item) => state.Bind(current =>
                AdmitPoint(point: item.Point, domain: domain, context: context).Match(
                    Succ: accepted => item.Mass.Match(
                        Some: mass => Fin.Succ((current.Accepted.Add(accepted), current.Mass.Add(mass), true, current.Rejected)),
                        None: () => Fin.Succ((current.Accepted.Add(accepted), current.Mass, current.Weighted, current.Rejected))),
                    Fail: _ => Fin.Succ((current.Accepted, current.Mass, current.Weighted, current.Rejected + 1)))))
        from mass in admitted.Weighted && !admitted.Accepted.IsEmpty
            ? CloudKernel.MassOf(mass: new Arr<double>([.. admitted.Mass.AsIterable()]), count: admitted.Mass.Count).Map(Some)
            : Fin.Succ(Option<Arr<double>>.None)
        select new Result(
            Points: admitted.Accepted, Mass: mass,
            Tally: TallyOf(attempted: points.Count, emitted: admitted.Accepted, rejected: admitted.Rejected, candidates: Some(points.Count),
                algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None)));
    private static Fin<Point3d> AdmitPoint(Point3d point, ExtractionDomain domain, Context context) =>
        Acceptance.Value(value: point).Bind(valid => domain.Switch(
            state: (Point: valid, Context: context),
            supportCase: static (state, d) => d.Value.Closest(sample: state.Point).Bind(hit => Acceptance.Value(value: hit.Point)),
            meshCase: static (state, d) => Optional(d.Value.Native.ClosestMeshPoint(testPoint: state.Point, maximumDistance: state.Context.Absolute.Value))
                .ToFin(new KernelFault.InvalidResult()).Bind(meshPoint => Acceptance.Value(value: meshPoint.Point)),
            cloudCase: static (state, d) => d.Value.SwitchPartially(
                state: state,
                @default: static (held, value) => Fin.Fail<Point3d>(new KernelFault.Unsupported(InputType: value.GetType(), OutputType: typeof(Point3d))),
                clusterCase: static (held, cluster) => cluster.Vertices.Find(vertex => vertex.DistanceToSquared(other: held.Point) <= held.Context.Absolute.Value * held.Context.Absolute.Value)
                    .ToFin(new KernelFault.InvalidInput())),
            latticeCase: static (state, d) => {
                Point3d local = d.Value.Locate(sample: state.Point);
                bool inside = local.X >= 0.0 && local.X <= d.Value.Columns.Value
                    && local.Y >= 0.0 && local.Y <= d.Value.Rows.Value
                    && (d.Value.Rank is 2 || (local.Z >= 0.0 && local.Z <= d.Value.Layers.Value));
                (int column, int row, int layer) = d.Value.Nearest(sample: state.Point);
                return inside
                    ? Acceptance.Value(value: d.Value.Center(column: column, row: row, layer: layer))
                    : Fin.Fail<Point3d>(new KernelFault.InvalidInput());
            }));

    private static Fin<Result> SampleGeneratedSupport(SampleKind kind, SupportSpace space, Context context) =>
        kind.Facts.Count.ToFin(Fail: new KernelFault.Unsupported(InputType: kind.GetType(), OutputType: typeof(Result))).Bind(count =>
            from points in space.Payload.Evaluate<Seq<Point3d>>(new EvaluationRequest.Sample(Count: Dimension.Create(value: (int)Math.Ceiling(a: count * Math.Max(1.0, kind.Facts.CandidateScale))), Model: context))
            from sampled in SampleOnCandidates(kind: kind, candidates: points.Map(static point => new Candidate(Point: point, Mass: Option<double>.None)), admitsPoisson: false, domainMeasure: Option<(int Rank, double Measure)>.None, context: context)
            select sampled);
    private static Fin<Result> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context) =>
        kind.SwitchPartially(
            state: (Domain: domain, Context: context),
            @default: static (state, value) => {
                using AreaMassProperties? props = AreaMassProperties.Compute(mesh: state.Domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
                return Optional(props).ToFin(new KernelFault.InvalidResult()).Bind(p =>
                    from density in value.MeshCandidateDensity(p.Area, state.Key)
                    from candidates in SurfaceCandidatePoints(state.Domain, density, state.Key)
                    from sampled in SampleOnCandidates(value, candidates.Map(static point => new Candidate(point, None)), true,
                        Some((Rank: 2, Measure: p.Area)), state.Context, state.Key)
                    from validated in SegmentKernel.ValidateSamplingSpectrum(state.Domain, sampled, state.Key)
                    select validated);
            },
            powerCcvtCase: static (state, value) => PowerCcvtRun.Execute(state.Domain, value, state.Context, state.Key),
            dworkVariableDensityCase: static (state, value) =>
                from selection in DworkMeshRun.Execute(value, state.Domain, value.Radius, value.Count.Value, value.MinRadius.Value, value.Attempts, value.Seed, state.Context, state.Key)
                let points = toSeq(selection.Points)
                let census = selection.Algorithm.Dwork.ToFin(new KernelFault.InvalidResult())
                from dwork in census
                let result = new Result(points, selection.Mass, TallyOf(dwork.GeneratedCandidates, points,
                    dwork.RejectedTooClose + dwork.RejectedDomain, None, selection.Algorithm, state.Key))
                from validated in SegmentKernel.ValidateSamplingSpectrum(state.Domain, result, state.Key)
                select validated);

    private const double OpenLatticeInset = 1.0;
    private const double TriangularLatticeFactor = 2.0;
    private static Fin<Seq<Point3d>> SurfaceCandidatePoints(MeshSpace space, double density) {
        if (!double.IsFinite(density) || density <= 0.0) return Fin.Fail<Seq<Point3d>>(new KernelFault.InvalidInput());
        List<Point3d> samples = [];
        using Mesh triangulated = space.Native.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<Seq<Point3d>>(new KernelFault.InvalidResult());
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A]; Point3d b = triangulated.Vertices[index: face.B]; Point3d c = triangulated.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: area * density));
            int side = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Sqrt(d: count * TriangularLatticeFactor)));
            double denominator = side + (2.0 * OpenLatticeInset) + 1.0;
            samples.AddRange(collection: Enumerable.Range(start: 0, count: side + 1)
                .SelectMany(i => Enumerable.Range(start: 0, count: side - i + 1).Select(j => (Wa: (i + OpenLatticeInset) / denominator, Wb: (j + OpenLatticeInset) / denominator)))
                .Select(w => new Point3d(
                    x: (w.Wa * a.X) + (w.Wb * b.X) + ((1.0 - w.Wa - w.Wb) * c.X),
                    y: (w.Wa * a.Y) + (w.Wb * b.Y) + ((1.0 - w.Wa - w.Wb) * c.Y),
                    z: (w.Wa * a.Z) + (w.Wb * b.Z) + ((1.0 - w.Wa - w.Wb) * c.Z)))
                .Take(count));
        }
        return samples.Count > 0 && samples.TrueForAll(static point => point.IsValid)
            ? Fin.Succ(toSeq(samples))
            : Fin.Fail<Seq<Point3d>>(new KernelFault.InvalidResult());
    }

    private sealed class PowerCcvtRun(MeshSpace domain, SampleKind.PowerCcvtCase kind, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context) {
        internal static Fin<Result> Execute(MeshSpace domain, SampleKind.PowerCcvtCase kind, Context context) {
            Fin<(Plane Plane, double Deviation)> Fit(Seq<Point3d> points) =>
                (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
                    (PlaneFitResult.Success, { IsValid: true } valid) => Acceptance.Value(value: valid).Bind(p => Acceptance.Value(value: deviation).Map(d => (Plane: p, Deviation: d))),
                    _ => Fin.Fail<(Plane Plane, double Deviation)>(error: new KernelFault.InvalidResult()),
                };
            Fin<Seq<Point3d>> Sites(Seq<Point3d> candidates, int count) => kind.Policy.Density.Match(
                Some: field => candidates.TraverseM(point =>
                    field.SampleScalar(sample: point, context: context).Map(weight => (Point: point, Weight: weight))).As()
                    .Map(rows => toSeq(rows.AsIterable()
                        .Where(static row => double.IsFinite(row.Weight) && row.Weight > 0.0)
                        .OrderBy(row => -Math.Log(Deterministic.UnitInterval(row.Point, SampleLane.Priority.Lane, kind.Policy.Seed)) / row.Weight)
                        .Take(count).Select(static row => row.Point))),
                None: () => Fin.Succ(toSeq(FarthestIndices(candidates.Map(static point => new Candidate(point, None)), count)
                    .Select(i => candidates[i]))));
            using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
            return Optional(props).Map(static p => p.Area).Filter(static area => double.IsFinite(area) && area > 0.0).ToFin(new KernelFault.InvalidResult()).Bind(meshArea =>
                from density in kind.MeshCandidateDensity(area: meshArea)
                from candidates in SurfaceCandidatePoints(space: domain, density: density)
                from fit in Fit(candidates)
                from sites in Sites(candidates, Math.Min(val1: kind.Count.Value, val2: candidates.Count))
                from run in new PowerCcvtRun(domain: domain, kind: kind, sites: sites, totalMass: meshArea, planarityDeviation: fit.Deviation, context: context).Run()
                from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: run)
                select validated);
        }

        private readonly int siteCount = sites.Count;
        private readonly double targetMass = totalMass / Math.Max(val1: 1, val2: sites.Count);
        private readonly double searchDistance = domain.Native.GetBoundingBox(accurate: true).Diagonal.Length;
        private readonly Atom<int> rebuilds = Atom(value: 0);
        private double ResidualTol => context.For(lane: ToleranceLane.Residual).Value;
        private double NewtonFloor => context.For(lane: ToleranceLane.Step).Value;
        private double SufficientDecrease => context.For(lane: ToleranceLane.Kkt).Value;
        private double MotionFloor => context.For(lane: ToleranceLane.Convergence).Value;

        internal Fin<Result> Run() =>
            siteCount < 1
                ? Fin.Fail<Result>(new KernelFault.InvalidResult())
                : ConvergeNewton(currentSites: sites, seed: RebuildDiagram(currentSites: sites, weights: new Arr<double>([.. Enumerable.Repeat(element: 0.0, count: siteCount)])))
                    .Bind(seed => ConvergeOuter(seed: new OuterState(
                        Sites: sites, Capacity: seed, OuterIterations: 0, LloydIterations: 0, GradientIterations: 0,
                        StepHalvings: seed.StepHalvings, PositionGradientNorm: 0.0,
                        TransportEnergyDelta: 0.0, Converged: false, Fault: Option<Error>.None)).Bind(Finalize));

        private Fin<OuterState> ConvergeOuter(OuterState seed) =>
            Cell.Converge(
                cell: Atom(value: seed),
                step: state => Some(OuterStep(state: state)),
                settled: static state => state.Converged || state.Fault.IsSome,
                budget: kind.Policy.Iterations, declined: new KernelFault.InvalidResult()).Current
            switch { OuterState settled => settled.Fault.Match(Some: Fin.Fail<OuterState>, None: () => Fin.Succ(settled)) };
        private OuterState OuterStep(OuterState state) {
            (Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm) motion = TwoPhaseSiteMotion(currentSites: state.Sites, capacity: state.Capacity);
            return MeanNearestSpacing(points: motion.Sites, measure: totalMass).Bind(meanSpacing =>
                ConvergeNewton(currentSites: motion.Sites, seed: RebuildDiagram(currentSites: motion.Sites, weights: state.Capacity.Weights)).Map(advanced => state with {
                    Sites = motion.Sites, Capacity = advanced,
                    OuterIterations = state.OuterIterations + 1, LloydIterations = state.LloydIterations + motion.LloydIterations,
                    GradientIterations = state.GradientIterations + motion.GradientIterations,
                    StepHalvings = state.StepHalvings + motion.GradientHalvings,
                    PositionGradientNorm = motion.PositionGradientNorm,
                    TransportEnergyDelta = advanced.TransportEnergy - state.Capacity.TransportEnergy,
                    Converged = motion.Displacement <= Math.Max(val1: kind.Policy.LloydTolerance.Value * meanSpacing, val2: MotionFloor)
                             && motion.PositionGradientNorm <= Math.Max(val1: kind.Policy.GradientTolerance.Value * meanSpacing, val2: MotionFloor),
                }))
            .Match(Succ: static advanced => advanced, Fail: error => state with { Fault = Some(error) });
        }
        private (Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm) TwoPhaseSiteMotion(Seq<Point3d> currentSites, NewtonState capacity) {
            (Seq<Point3d> lloydSites, int sweeps, RestrictedPowerDiagram lloydDiagram) = LloydPhase(currentSites: currentSites, diagram: capacity.Diagram, weights: capacity.Weights);
            (Seq<Point3d> gradientSites, int steps, int halvings, RestrictedPowerDiagram gradientDiagram) = GradientPhase(currentSites: lloydSites, diagram: lloydDiagram, weights: capacity.Weights);
            return (Sites: gradientSites, LloydIterations: sweeps, GradientIterations: steps, GradientHalvings: halvings,
                Displacement: PairwiseShift(from: currentSites, to: gradientSites),
                PositionGradientNorm: Math.Sqrt(d: AscentSlope(direction: AscentDirection(sitesAt: gradientSites, diagram: gradientDiagram))));
        }
        private (Seq<Point3d> Sites, int Sweeps, RestrictedPowerDiagram Diagram) LloydPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) =>
            toSeq(Enumerable.Range(start: 0, count: kind.Policy.LloydSweeps.Value)).Fold(
                initialState: (Sites: currentSites, Sweeps: 0, Diagram: diagram),
                f: (state, _) => {
                    Seq<Point3d> moved = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: state.Diagram, site: i).Match(
                        Some: cell => cell.Empty || !cell.Barycenter.IsValid ? state.Sites[index: i] : cell.Barycenter,
                        None: () => state.Sites[index: i])));
                    return RebuildPowerCells(currentSites: moved, weights: weights).Match(
                        Succ: rebuilt => (Sites: moved, Sweeps: state.Sweeps + 1, Diagram: rebuilt),
                        Fail: _ => state);
                });
        private (Seq<Point3d> Sites, int Steps, int Halvings, RestrictedPowerDiagram Diagram) GradientPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) =>
            toSeq(Enumerable.Range(start: 0, count: kind.Policy.GradientSteps.Value)).Fold(
                initialState: (Sites: currentSites, Steps: 0, Halvings: 0, Diagram: diagram, Live: true),
                f: (state, _) => {
                    if (!state.Live) return state;
                    Vector3d[] direction = AscentDirection(sitesAt: state.Sites, diagram: state.Diagram);
                    double slope = AscentSlope(direction: direction);
                    if (!(double.IsFinite(slope) && slope > 0.0)) return state with { Live = false };
                    return Armijo(
                        baseline: -TransportEnergyOf(diagram: state.Diagram), slope: slope,
                        trial: step => {
                            Seq<Point3d> moved = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => state.Sites[index: i] + (step * direction[i])));
                            return RebuildPowerCells(currentSites: moved, weights: weights).Map(diagram => (State: (Sites: moved, Diagram: diagram), Objective: -TransportEnergyOf(diagram: diagram)));
                        })
                        .Match(
                            Succ: found => (Sites: found.State.Sites, Steps: state.Steps + 1, Halvings: state.Halvings + found.Halvings, Diagram: found.State.Diagram, Live: true),
                            Fail: _ => state with { Halvings = state.Halvings + kind.Policy.MaxHalvings.Value, Live = false });
                }) switch { var terminal => (terminal.Sites, terminal.Steps, terminal.Halvings, terminal.Diagram) };
        private Fin<(T State, int Halvings)> Armijo<T>(double baseline, double slope, Func<double, Fin<(T State, double Objective)>> trial) =>
            toSeq(Enumerable.Range(start: 0, count: kind.Policy.MaxHalvings.Value + 1)).Fold(
                initialState: Fin.Fail<(T State, int Halvings)>(new KernelFault.InvalidResult()),
                f: (accepted, halvings) => accepted.Match(
                    Succ: static found => Fin.Succ(found),
                    Fail: _ => {
                        double step = kind.Policy.InitialStep.Value * Math.Pow(kind.Policy.Backtrack.Value, halvings);
                        return trial(step).Bind(candidate => candidate.Objective >= baseline + (SufficientDecrease * step * slope)
                            ? Fin.Succ((candidate.State, halvings))
                            : Fin.Fail<(T State, int Halvings)>(new KernelFault.InvalidResult()));
                    }));
        private Vector3d[] AscentDirection(Seq<Point3d> sitesAt, RestrictedPowerDiagram diagram) =>
            [.. Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: diagram, site: i).Match(
                Some: cell => cell.Empty || !cell.Barycenter.IsValid ? Vector3d.Zero : 2.0 * Math.Max(val1: cell.Mass, val2: 0.0) * (cell.Barycenter - sitesAt[index: i]),
                None: () => Vector3d.Zero))];
        private static double AscentSlope(Vector3d[] direction) => direction.Sum(static d => d.SquareLength);
        private static double TransportEnergyOf(RestrictedPowerDiagram diagram) =>
            diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.TransportCost);
        private static Option<PowerCell> CellOf(RestrictedPowerDiagram diagram, int site) =>
            site >= 0 && site < diagram.Cells.Count ? Some(diagram.Cells[index: site]) : Option<PowerCell>.None;
        private Fin<RestrictedPowerDiagram> RebuildPowerCells(Seq<Point3d> currentSites, Arr<double> weights) =>
            MeshKernel.RestrictedPowerCells(space: domain, sites: currentSites, weights: Some(weights), density: kind.Policy.Density)
                .Map(diagram => { ignore(rebuilds.Swap(static held => held + 1)); return diagram; });
        private static double PairwiseShift(Seq<Point3d> from, Seq<Point3d> to) =>
            Enumerable.Range(start: 0, count: Math.Min(val1: from.Count, val2: to.Count)).Sum(i => from[index: i].DistanceTo(other: to[index: i]));

        private Fin<NewtonState> ConvergeNewton(Seq<Point3d> currentSites, Fin<NewtonState> seed) =>
            seed.Map(seedState => Cell.Converge(
                    cell: Atom(value: seedState),
                    step: state => Some(NewtonStep(currentSites: currentSites, state: state)),
                    settled: static state => state.Converged || state.Fault.IsSome,
                    budget: kind.Policy.MaxNewton, declined: new KernelFault.InvalidResult()).Current)
                .Bind(terminal => terminal.Fault.Match(Some: Fin.Fail<NewtonState>, None: () => Fin.Succ(terminal)));
        private NewtonState NewtonStep(Seq<Point3d> currentSites, NewtonState state) {
            Arr<double> gradient = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => targetMass - state.Diagram.Cells[index: i].Mass)]);
            double gradNorm = TensorPrimitives.Norm<double>([.. gradient.AsIterable()]);
            Arr<double> masses = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => Math.Max(val1: state.Diagram.Cells[index: i].Mass, val2: 0.0))]);
            GaugePolicy gauge = kind.Policy.PinnedSite.Match(
                Some: site => GaugePolicy.Pinned(indices: [site], mass: Some(masses), shift: GaugeShift.None),
                None: () => GaugePolicy.MeanZeroConstant(dimension: masses.Count, mass: Some(masses), shift: GaugeShift.MeanZero));
            return HessianTriplets(currentSites: currentSites, diagram: state.Diagram)
                .Bind(triplets => SparseMatrix.FromTriplets(rows: Dimension.Create(value: siteCount), cols: Dimension.Create(value: siteCount), triplets: triplets))
                .Bind(laplacian => laplacian.SingularSolveDetailed(rhs: gradient, gauge: gauge, context: context))
                .Bind(solve => GeodesicKernel.Solved(solve: Fin.Succ(solve))
                    .Bind(direction => Armijo(
                        baseline: state.DualObjective,
                        slope: TensorPrimitives.Dot<double>([.. gradient.AsIterable()], [.. direction.AsIterable()]),
                        trial: step => RebuildDiagram(currentSites: currentSites, weights: new Arr<double>([.. Enumerable.Range(start: 0, count: siteCount).Select(i => state.Weights[index: i] + (step * direction[index: i]))]))
                            .Map(rebuilt => (State: rebuilt, Objective: rebuilt.DualObjective)))
                        .Map(found => (Solve: solve, Advanced: found.State with { StepHalvings = state.StepHalvings + found.Halvings, NewtonIterations = state.NewtonIterations }))))
                .Match(
                    Succ: step => step.Advanced with {
                        DualSolve = Some(step.Solve), WeightGradientNorm = gradNorm,
                        NewtonIterations = state.NewtonIterations + 1,
                        Converged = step.Advanced.Residual.Inf <= ResidualTol * targetMass,
                    },
                    Fail: error => state with { Fault = Some(error), DualSolve = state.DualSolve });
        }
        private Fin<List<(int Row, int Col, double Value)>> HessianTriplets(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram) {
            List<(int Row, int Col, double Value)> triplets = [];
            double floor = NewtonFloor * Math.Max(val1: searchDistance, val2: EpsilonPolicy.ZeroTolerance);
            foreach (PowerFacet facet in diagram.Facets.AsIterable()) {
                if (facet.SiteI < 0 || facet.SiteI >= siteCount || facet.SiteJ < 0 || facet.SiteJ >= siteCount || facet.SiteI == facet.SiteJ) continue;
                double siteDistance = currentSites[index: facet.SiteI].DistanceTo(other: currentSites[index: facet.SiteJ]);
                if (!(double.IsFinite(siteDistance) && siteDistance > floor && double.IsFinite(facet.Length))) continue;
                double weight = facet.Length / (2.0 * siteDistance);
                if (!double.IsFinite(weight)) continue;
                triplets.Add(item: (facet.SiteI, facet.SiteJ, -weight));
                triplets.Add(item: (facet.SiteJ, facet.SiteI, -weight));
                triplets.Add(item: (facet.SiteI, facet.SiteI, weight));
                triplets.Add(item: (facet.SiteJ, facet.SiteJ, weight));
            }
            double tikhonov = NewtonFloor * Math.Max(val1: targetMass, val2: EpsilonPolicy.ZeroTolerance);
            for (int i = 0; i < siteCount; i++) triplets.Add(item: (i, i, tikhonov));
            return triplets.Exists(static row => row.Value != 0.0) ? Fin.Succ(triplets) : Fin.Fail<List<(int Row, int Col, double Value)>>(new KernelFault.InvalidResult());
        }
        private Fin<NewtonState> RebuildDiagram(Seq<Point3d> currentSites, Arr<double> weights) =>
            RebuildPowerCells(currentSites: currentSites, weights: weights).Map(diagram => {
                double transport = TransportEnergyOf(diagram: diagram);
                double dual = transport + Enumerable.Range(start: 0, count: siteCount).Sum(i => weights[index: i] * (targetMass - diagram.Cells[index: i].Mass));
                double[] deviation = [.. Enumerable.Range(start: 0, count: siteCount).Select(i => diagram.Cells[index: i].Mass - targetMass)];
                TensorPrimitives.Abs<double>(deviation, deviation);
                double inf = TensorPrimitives.Max<double>(deviation);
                (double Inf, double L1, double L2, double Normalized) residual = (inf, TensorPrimitives.Sum<double>(deviation), TensorPrimitives.Norm<double>(deviation),
                    inf / Math.Max(val1: targetMass, val2: EpsilonPolicy.ZeroTolerance));
                return new NewtonState(Weights: weights, Diagram: diagram, Residual: residual, DualObjective: dual, TransportEnergy: transport,
                    Converged: false, NewtonIterations: 0, StepHalvings: 0, Fault: Option<Error>.None,
                    DualSolve: Option<LinearSolution>.None, WeightGradientNorm: 0.0);
            });
        private Fin<Result> Finalize(OuterState outer) {
            NewtonState terminal = outer.Capacity;
            PowerCensus census = terminal.Diagram.Census;
            return from meanSpacing in MeanNearestSpacing(points: outer.Sites, measure: totalMass)
                   from broken in BreakRegularity(currentSites: outer.Sites, meanSpacing: meanSpacing)
                   let lifted = broken.Sites.Choose(site => Optional(domain.Native.ClosestMeshPoint(testPoint: site, maximumDistance: searchDistance))
                       .Filter(static hit => hit.Point.IsValid).Map(static hit => hit.Point))
                   from mass in Stat<Scalar>.Of(values: terminal.Diagram.Cells.AsIterable().Map(static cell => (Scalar)cell.Mass).ToSeq())
                   from weights in Stat<Scalar>.Of(values: terminal.Weights.AsIterable().Map(static weight => (Scalar)weight).ToSeq())
                   from poissonRadius in NormalizedPoissonRadius(points: lifted, measure: totalMass)
                   let solution = new PowerCcvtSolution(
                       SiteCount: siteCount, TargetMass: targetMass,
                       CapacityResidualInf: terminal.Residual.Inf, CapacityResidualL1: terminal.Residual.L1, CapacityResidualL2: terminal.Residual.L2, CapacityResidualNormalized: terminal.Residual.Normalized,
                       OuterIterations: outer.OuterIterations, LloydIterations: outer.LloydIterations, GradientIterations: outer.GradientIterations, DualNewtonIterations: terminal.NewtonIterations,
                       Weights: weights, TransportEnergy: terminal.TransportEnergy, TransportEnergyDelta: outer.TransportEnergyDelta,
                       DualObjective: terminal.DualObjective, CentroidShift: PairwiseShift(from: lifted, to: sites), PositionGradientNorm: outer.PositionGradientNorm, WeightGradientNorm: terminal.WeightGradientNorm,
                       EmptyCellCount: census.EmptyCellCount, StepHalvingCount: outer.StepHalvings, RebuildCount: rebuilds.Value,
                       AliasedSiteCount: broken.AliasedCount, RelocatedSiteCount: broken.RelocatedCount, UnliftedSiteCount: broken.Sites.Count - lifted.Count,
                       NormalizedPoissonRadius: poissonRadius, PlanarityDeviation: planarityDeviation,
                       PinnedSite: kind.Policy.PinnedSite, Converged: outer.Converged,
                       FragmentCount: census.FragmentCount, FacetCount: census.NeighborFacetCount, CellMass: mass, IntegrationResidual: census.IntegrationResidual,
                       DualSolve: terminal.DualSolve)
                   from admitted in solution.IsValid ? Fin.Succ(solution) : Fin.Fail<PowerCcvtSolution>(new KernelFault.InvalidResult())
                   select new Result(Points: lifted, Mass: Option<Arr<double>>.None,
                       Tally: TallyOf(attempted: siteCount, emitted: lifted, rejected: census.EmptyCellCount, candidates: Some(siteCount),
                           algorithm: new SampleAlgorithm(
                               Kind: kind,
                               Assurances: CapabilitySet<SampleAssurance>.Of([
                                   .. terminal.Converged ? (SampleAssurance[])[SampleAssurance.CapacityResidual] : [],
                                   .. !lifted.IsEmpty && terminal.DualSolve.Exists(static solve => solve.IsValid)
                                       ? (SampleAssurance[])[SampleAssurance.TransportAssignment] : []]),
                               CapacityResidual: Some(terminal.Residual.Inf),
                               PowerCcvt: Some(admitted))));
        }
        private Fin<(Seq<Point3d> Sites, int AliasedCount, int RelocatedCount)> BreakRegularity(Seq<Point3d> currentSites, double meanSpacing) {
            if (currentSites.Count < 2 || meanSpacing <= EpsilonPolicy.ZeroTolerance) return Fin.Succ((Sites: currentSites, AliasedCount: 0, RelocatedCount: 0));
            static Vector3d JitterOffset(Deterministic.Draw draw, double magnitude) {
                double u1 = draw.At(0L).Unit;
                double u2 = draw.At(1L).Unit;
                double radius = magnitude * Math.Sqrt(d: Math.Max(val1: 0.0, val2: -2.0 * Math.Log(d: u1)));
                double angle = 2.0 * Math.PI * u2;
                return new Vector3d(x: radius * Math.Cos(d: angle), y: radius * Math.Sin(a: angle), z: 0.0);
            }
            double jitterMagnitude = kind.Policy.JitterScale.Value * meanSpacing;
            Deterministic.Draw jitter = Deterministic.Of(kind.Policy.Seed, SampleLane.Jitter);
            return AliasMask(currentSites: currentSites, radius: kind.Policy.AliasScale.Value * meanSpacing).Map(aliased => {
                int total = currentSites.Count;
                int aliasedCount = aliased.Count(static flag => flag);
                int relocateBudget = Math.Min(val1: aliasedCount, val2: (int)Math.Floor(d: kind.Policy.RelocateFraction.Value * aliasedCount));
                int relocated = 0;
                Point3d[] moved = new Point3d[total];
                for (int i = 0; i < total; i++) {
                    bool relocate = aliased[i] && relocated < relocateBudget;
                    moved[i] = aliased[i]
                        ? currentSites[index: i] + JitterOffset(draw: jitter.At(i), magnitude: relocate ? jitterMagnitude + meanSpacing : jitterMagnitude)
                        : currentSites[index: i];
                    if (relocate) relocated++;
                }
                return (Sites: toSeq(moved), AliasedCount: aliasedCount, RelocatedCount: relocated);
            });
        }
        private Fin<bool[]> AliasMask(Seq<Point3d> currentSites, double radius) =>
            from index in NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: currentSites))
            from reach in FactoryBridge.Accept<PositiveMagnitude>(candidate: radius)
            from graph in NeighborKernel.GraphOf(index: index, needles: [.. currentSites.AsIterable()], count: Option<Dimension>.None, radius: Some(reach))
            select Enumerable.Range(0, currentSites.Count)
                .Select(i => graph.Ids.Length > i && graph.Ids[i].Any(id => id >= 0 && id < i)).ToArray();
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct NewtonState(
            Arr<double> Weights, RestrictedPowerDiagram Diagram, (double Inf, double L1, double L2, double Normalized) Residual, double DualObjective, double TransportEnergy,
            bool Converged, int NewtonIterations, int StepHalvings, Option<Error> Fault, Option<LinearSolution> DualSolve, double WeightGradientNorm);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct OuterState(
            Seq<Point3d> Sites, NewtonState Capacity, int OuterIterations, int LloydIterations, int GradientIterations,
            int StepHalvings, double PositionGradientNorm, double TransportEnergyDelta,
            bool Converged, Option<Error> Fault);
    }

    private static Fin<Selection> Select(
        SampleKind kind,
        (Seq<Candidate> Candidates, bool AdmitsPoisson, Option<(int Rank, double Measure)> DomainMeasure, Context Context) input) =>
        kind.SwitchPartially(
            state: input,
            @default: static (state, value) => Fin.Fail<Selection>(new KernelFault.Unsupported(InputType: value.GetType(), OutputType: typeof(Result))),
            poissonDiskCase: static (state, value) => state.AdmitsPoisson
                ? PoissonDiskSelection(value, state.Candidates, value.Radius, value.Attempts, value.Seed, state.Key)
                : Fin.Fail<Selection>(new KernelFault.Unsupported(InputType: value.GetType(), OutputType: typeof(Result))),
            farthestCase: static (state, value) => SelectionOf(value, state.Candidates, FarthestIndices(state.Candidates, value.Count.Value), state.Key),
            optimizeCase: static (state, value) => SelectionOf(value, state.Candidates, OptimizeFarthest(state.Candidates, value.Count.Value, value.Iterations.Value, state.Key), state.Key),
            lloydCase: static (state, value) => RelaxationSample(state.Candidates, value.Count.Value, value.Iterations.Value, None, state.Key)
                .Bind(relaxed => SelectionOf(value, state.Candidates, relaxed.Indices, state.Key)),
            capacityCase: static (state, value) => CapacityCvtSelection(value, state.Candidates, value.Count.Value, value.Limit.Value, value.Iterations.Value,
                state.Context.For(ToleranceLane.Convergence).Value, state.Key),
            scalarDensityCase: static (state, value) => BoundingMeasure(state.Candidates) switch {
                (int Rank, double Measure) bounds => DensitySelection(value, state.Candidates, value.Density, value.Count.Value,
                    0.5 * MeanSpacing(bounds.Rank, bounds.Measure, value.Count.Value), state.Context, value.Seed, state.Key),
            },
            adaptiveCase: static (state, value) => DensitySelection(value, state.Candidates, value.Density, value.Count.Value, value.MinSpacing.Value, state.Context, value.Seed, state.Key),
            sampleEliminationCase: static (state, value) => SampleElimination(value, state.Candidates, value.Count.Value, value.Alpha.Value, value.Beta.Value, value.Gamma.Value, value.Seed, state.DomainMeasure, state.Key)
                .Bind(run => SelectionOf(state.Candidates, run.Indices, run.Algorithm, state.Key)),
            dworkVariableDensityCase: static (state, value) => DworkCandidateSelection(value, state.Candidates, value.Radius, value.Count.Value, value.MinRadius.Value, value.Attempts, value.Seed, state.Context, state.Key));
    private static Fin<Result> SampleOnCandidates(SampleKind kind, Seq<Candidate> candidates, bool admitsPoisson, Option<(int Rank, double Measure)> domainMeasure, Context context) =>
        from selection in Select(kind, (candidates, admitsPoisson, domainMeasure, context, key))
        let sampled = toSeq(selection.Points)
        let rejected = selection.DensityRejected.IfNone(Math.Max(val1: 0, val2: candidates.Count - selection.Points.Length))
        select new Result(
            Points: sampled, Mass: selection.Mass,
            Tally: TallyOf(attempted: candidates.Count, emitted: sampled, rejected: rejected, candidates: Some(candidates.Count),
                densityAccepted: selection.DensityAccepted, densityRejected: selection.DensityRejected, algorithm: selection.Algorithm));

    private static Fin<Selection> SelectionOf(SampleKind kind, Seq<Candidate> candidates, int[] indices) =>
        SelectionOf(candidates: candidates, indices: indices, algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None));
    private static Fin<Selection> SelectionOf(Seq<Candidate> candidates, int[] indices, SampleAlgorithm algorithm) {
        Point3d[] points = [.. indices.Select(i => candidates[index: i].Point)];
        Seq<double> mass = toSeq(indices).Choose(i => candidates[index: i].Mass);
        return (indices.Length, mass.Count) switch {
            (0, _) or (_, 0) => Fin.Succ(new Selection(Points: points, Mass: Option<Arr<double>>.None, DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            (int count, int weights) when count == weights => CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: mass.Count).Map(normalized => new Selection(Points: points, Mass: Some(normalized), DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            _ => Fin.Fail<Selection>(new KernelFault.InvalidResult()),
        };
    }
    private static SampleTally TallyOf(int attempted, Seq<Point3d> emitted, int rejected, Option<int> candidates, SampleAlgorithm algorithm, Option<int> densityAccepted = default, Option<int> densityRejected = default) =>
        new(Attempted: attempted, Emitted: emitted.Count, Rejected: rejected, CandidateCount: candidates,
            Spacing: SpacingDistribution(points: emitted).ToOption(),
            DensityAccepted: densityAccepted, DensityRejected: densityRejected, Algorithm: algorithm);
    private static Fin<Selection> DensitySelection(SampleKind kind, Seq<Candidate> candidates, ScalarField density, int count, double minSpacing, Context context, int seed) =>
        toSeq(Enumerable.Range(start: 0, count: candidates.Count))
            .TraverseM(i => density.SampleScalar(sample: candidates[index: i].Point, context: context)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? Acceptance.Value(value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => Some((Index: i, Value: valid)))
                    : Fin.Succ(Option<(int Index, double Value)>.None)))
            .As()
            .Map(static rows => rows.Somes())
            .Bind(admitted => admitted.Fold(Option<(double Min, double Max)>.None, static (band, row) => Some(Widen(band, row.Value))).Match(
                Some: band => PrioritySelection(kind: kind, candidates: candidates, admitted: admitted, count: count, minSpacing: minSpacing, minWeight: band.Min, maxWeight: band.Max, rejected: candidates.Count - admitted.Count, seed: seed),
                None: () => Fin.Fail<Selection>(new KernelFault.InvalidResult())));
    private static (double Min, double Max) Widen(Option<(double Min, double Max)> band, double value) =>
        band.Map(held => (Math.Min(val1: held.Min, val2: value), Math.Max(val1: held.Max, val2: value))).IfNone((value, value));
    private static double BackgroundCellSize(double radius) =>
        Math.Max(val1: radius / Math.Sqrt(d: 3.0), val2: EpsilonPolicy.ZeroTolerance);
    private static Fin<Selection> PrioritySelection(SampleKind kind, Seq<Candidate> candidates, Seq<(int Index, double Value)> admitted, int count, double minSpacing, double minWeight, double maxWeight, int rejected, int seed) {
        (Seq<(Point3d Point, double Radius)> Chosen, Seq<double> Mass, Option<(double Min, double Max)> Band) drained =
            toSeq(admitted.OrderBy(row => -Math.Log(d: Deterministic.UnitInterval(point: candidates[index: row.Index].Point, salt: SampleLane.Priority.Lane, seed: seed)) / row.Value))
            .Fold(
                initialState: (Chosen: Seq<(Point3d Point, double Radius)>(), Mass: Seq<double>(), Band: Option<(double Min, double Max)>.None),
                f: (held, row) => {
                    if (held.Chosen.Count >= count) return held;
                    Point3d candidate = candidates[index: row.Index].Point;
                    double local = minSpacing / Math.Sqrt(d: Math.Max(val1: row.Value / Math.Max(val1: maxWeight, val2: EpsilonPolicy.ZeroTolerance), val2: EpsilonPolicy.ZeroTolerance));
                    (Seq<(Point3d Point, double Radius)> chosen, Seq<double> mass) = held.Chosen.ForAll(existing => candidate.DistanceTo(other: existing.Point) >= Math.Max(val1: existing.Radius, val2: local))
                        ? (held.Chosen.Add((candidate, local)), held.Mass.Add(row.Value))
                        : (held.Chosen, held.Mass);
                    return (chosen, mass, Some(Widen(held.Band, local)));
                });
        return CloudKernel.MassOf(mass: new Arr<double>([.. drained.Mass.AsIterable()]), count: drained.Mass.Count).Map(normalized => new Selection(
            Points: [.. drained.Chosen.Map(static sample => sample.Point)], Mass: Some(normalized), DensityAccepted: Some(admitted.Count), DensityRejected: Some(rejected),
            Algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None,
                DensityMin: Some(minWeight), DensityMax: Some(maxWeight),
                LocalRadiusMin: drained.Band.Map(static band => band.Min), LocalRadiusMax: drained.Band.Map(static band => band.Max))));
    }

    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCell(long X, long Y, long Z);
    private static Fin<Selection> DworkCandidateSelection(SampleKind kind, Seq<Candidate> candidates, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context) =>
        toSeq(Enumerable.Range(start: 0, count: candidates.Count))
            .TraverseM(i => radius.SampleScalar(sample: candidates[index: i].Point, context: context)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? Acceptance.Value(Math.Max(minRadius, value)).Map(local => Some((Index: i, Radius: local)))
                    : Fin.Succ(Option<(int Index, double Radius)>.None)))
            .As()
            .Map(static rows => rows.Somes())
            .Bind(admitted => admitted.Fold(Option<(double Min, double Max)>.None, static (band, row) => Some(Widen(band, row.Radius))).Match(None: () => Fin.Fail<Selection>(new KernelFault.InvalidResult()), Some: band => {
                int rejected = candidates.Count - admitted.Count;
                (int Index, double Radius)[] ordered = [.. admitted.OrderBy(item => Deterministic.OrderKey(point: candidates[index: item.Index].Point, seed: seed))];
                double cellSize = BackgroundCellSize(radius: band.Min);
                Point3d gridOrigin = ordered.Length > 0 ? new BoundingBox(points: ordered.Select(item => candidates[index: item.Index].Point)).Min : Point3d.Origin;
                DworkCell CellOf(Point3d point) => new(X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize), Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize), Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
                Point3d[] pool = [.. ordered.Select(item => candidates[index: item.Index].Point)];
                return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(pool))).Bind(poolIndex => {
                    List<(int Index, double Radius)> chosen = ordered.Length > 0 ? [ordered[0]] : [];
                    Dictionary<DworkCell, List<int>> chosenGrid = [];
                    void Record((int Index, double Radius) candidate) {
                        DworkCell cell = CellOf(point: candidates[index: candidate.Index].Point);
                        if (!chosenGrid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; chosenGrid.Add(key: cell, value: bucket); }
                        bucket.Add(item: chosen.Count - 1);
                    }
                    if (chosen.Count > 0) Record(candidate: chosen[0]);
                    bool Conflicts((int Index, double Radius) candidate) {
                        Point3d at = candidates[index: candidate.Index].Point;
                        int shells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: band.Max) / cellSize));
                        DworkCell home = CellOf(point: at);
                        for (int dx = -shells; dx <= shells; dx++)
                            for (int dy = -shells; dy <= shells; dy++)
                                for (int dz = -shells; dz <= shells; dz++)
                                    if (chosenGrid.TryGetValue(key: new DworkCell(X: home.X + dx, Y: home.Y + dy, Z: home.Z + dz), value: out List<int>? bucket))
                                        foreach (int slot in bucket) {
                                            (int Index, double Radius) other = chosen[index: slot];
                                            if (at.DistanceTo(other: candidates[index: other.Index].Point) < Math.Max(val1: other.Radius, val2: candidate.Radius)) return true;
                                        }
                        return false;
                    }
                    Deterministic.Draw activeDraw = Deterministic.Of(seed, SampleLane.Active);
                    Deterministic.Draw annulusDraw = Deterministic.Of(seed, SampleLane.Annulus);
                    List<(int Index, double Radius)> active = ordered.Length > 0 ? [ordered[0]] : [];
                    (int activePops, int tooClose, int outside) = (0, 0, 0);
                    while (active.Count > 0 && chosen.Count < count) {
                        int activeOffset = (int)(activeDraw.At(activePops).State % (ulong)active.Count);
                        (int Index, double Radius) parent = active[activeOffset];
                        Point3d parentPoint = candidates[index: parent.Index].Point;
                        Fin<NeighborhoodGraph> reach = FactoryBridge.Accept<PositiveMagnitude>(candidate: 2.0 * parent.Radius)
                            .Bind(ring => NeighborKernel.GraphOf(index: poolIndex, needles: [parentPoint], count: Option<Dimension>.None, radius: Some(ring)));
                        List<(int Index, double Radius)> annulus = reach.Match(
                            Succ: graph => graph.Ids.Length > 0
                                ? [.. graph.Ids[0].Where(o => o >= 0 && o < ordered.Length && parentPoint.DistanceTo(other: pool[o]) >= parent.Radius).Select(o => ordered[o])]
                                : [],
                            Fail: static _ => []);
                        if (annulus.Count == 0) { outside++; active.RemoveAt(index: activeOffset); activePops++; continue; }
                        int pops = activePops;
                        TrialState<(int Index, double Radius), int> trial = Trial<(int Index, double Radius), int>(
                            budget: attempts, seed: 0,
                            propose: (attempt, tally) => {
                                (int Index, double Radius) candidate = annulus[(int)(annulusDraw.At(pops, attempt).State % (ulong)annulus.Count)];
                                return chosen.Exists(item => item.Index == candidate.Index) || Conflicts(candidate: candidate)
                                    ? (Option<(int Index, double Radius)>.None, tally + 1)
                                    : (Some(candidate), tally);
                            });
                        tooClose += trial.Tally;
                        trial.Drawn.Match(
                            Some: candidate => { chosen.Add(item: candidate); Record(candidate: candidate); active.Add(item: candidate); },
                            None: () => active.RemoveAt(index: activeOffset));
                        activePops++;
                    }
                    DworkCensus dwork = new(CandidateOnly: true, RMin: band.Min, BackgroundCellSize: cellSize, BackgroundGridCells: chosenGrid.Count,
                        AttemptsPerActive: attempts.Value, GeneratedCandidates: chosen.Count + tooClose + outside + rejected, ActivePops: activePops,
                        RejectedTooClose: tooClose, RejectedDomain: rejected + outside, LocalRadiusMin: band.Min, LocalRadiusMax: band.Max);
                    return SelectionOf(candidates: candidates, indices: [.. chosen.Select(static item => item.Index)],
                        algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None,
                            OversampleCount: Some(ordered.Length),
                            ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(rejected + outside),
                            LocalRadiusMin: Some(band.Min), LocalRadiusMax: Some(band.Max), Dwork: Some(dwork)));
                });
            }));

    private sealed class DworkMeshRun(SampleKind kind, Mesh mesh, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context) {
        private readonly double cellSize = BackgroundCellSize(radius: minRadius);
        private readonly List<DworkSurfacePoint> chosen = [];
        private readonly List<int> active = [];
        private readonly Dictionary<DworkCell, List<int>> grid = [];
        private readonly Deterministic.Draw activeDraw = Deterministic.Of(seed, SampleLane.Active);
        private readonly Deterministic.Draw annulusDraw = Deterministic.Of(seed, SampleLane.Annulus);
        private readonly Deterministic.Draw areaDraw = Deterministic.Of(seed, SampleLane.Area);
        private readonly Deterministic.Draw barycentricDraw = Deterministic.Of(seed, SampleLane.Barycentric);
        private DworkTriangle[] triangles = [];
        private double[] cumulativeArea = [];
        private Point3d gridOrigin = Point3d.Origin;
        private double totalArea;
        private Option<(double Min, double Max)> radiusBand = None;
        private (int Proposals, int ActivePops, int TooClose, int Domain) tally;

        [StructLayout(LayoutKind.Auto)] private readonly record struct DworkSurfacePoint(Point3d Point, double Radius);
        [StructLayout(LayoutKind.Auto)] private readonly record struct DworkTriangle(Point3d A, Point3d B, Point3d C, double CumulativeArea);

        internal static Fin<Selection> Execute(SampleKind kind, MeshSpace domain, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context) {
            using Mesh mesh = domain.Native.DuplicateMesh();
            if (mesh.Faces.QuadCount > 0 && !mesh.Faces.ConvertQuadsToTriangles()) return Fin.Fail<Selection>(new KernelFault.InvalidResult());
            _ = mesh.FaceNormals.ComputeFaceNormals();
            return new DworkMeshRun(kind: kind, mesh: mesh, radius: radius, count: count, minRadius: minRadius, attempts: attempts, seed: seed, context: context).Run();
        }
        private Fin<Selection> Run() =>
            BuildTriangles().Bind(_ => {
                TrialState<DworkSurfacePoint, int> seedTrial = Trial<DworkSurfacePoint, int>(
                    budget: attempts, seed: 0,
                    propose: (attempt, domainMisses) => SurfaceSample(attempt: attempt).Bind(RadiusAt).Match(
                        Some: hit => (Some(hit), domainMisses),
                        None: () => (Option<DworkSurfacePoint>.None, domainMisses + 1)));
                tally.Proposals += seedTrial.Attempts;
                tally.Domain += seedTrial.Tally;
                return seedTrial.Drawn.Match(
                    Some: sample => {
                        Add(sample: sample);
                        while (active.Count > 0 && chosen.Count < count) {
                            int activeOffset = (int)(activeDraw.At(tally.ActivePops).State % (ulong)active.Count);
                            DworkSurfacePoint parent = chosen[index: active[index: activeOffset]];
                            int pops = tally.ActivePops;
                            TrialState<DworkSurfacePoint, (int TooClose, int Domain)> trial = Trial<DworkSurfacePoint, (int TooClose, int Domain)>(
                                budget: attempts, seed: (0, 0),
                                propose: (attempt, band) => AnnulusCandidate(parent: parent, pops: pops, attempt: attempt).Match(
                                    Some: value => Conflicts(candidate: value) ? (Option<DworkSurfacePoint>.None, (band.TooClose + 1, band.Domain)) : (Some(value), band),
                                    None: () => (Option<DworkSurfacePoint>.None, (band.TooClose, band.Domain + 1))));
                            tally = (tally.Proposals + trial.Attempts, tally.ActivePops + 1, tally.TooClose + trial.Tally.TooClose, tally.Domain + trial.Tally.Domain);
                            trial.Drawn.Match(Some: Add, None: () => active.RemoveAt(index: activeOffset));
                        }
                        return Settle();
                    },
                    None: () => Fin.Fail<Selection>(new KernelFault.InvalidResult()));
            });
        private Fin<Unit> BuildTriangles() {
            List<DworkTriangle> built = [];
            double cumulative = 0.0;
            for (int f = 0; f < mesh.Faces.Count; f++) {
                MeshFace face = mesh.Faces[index: f];
                if (!face.IsTriangle) continue;
                Point3d a = mesh.Vertices[index: face.A], b = mesh.Vertices[index: face.B], c = mesh.Vertices[index: face.C];
                double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
                if (!double.IsFinite(area) || area <= EpsilonPolicy.ZeroTolerance) continue;
                cumulative += area;
                built.Add(item: new DworkTriangle(A: a, B: b, C: c, CumulativeArea: cumulative));
            }
            BoundingBox bounds = mesh.GetBoundingBox(accurate: true);
            (triangles, cumulativeArea, totalArea, gridOrigin) = ([.. built], [.. built.Select(static item => item.CumulativeArea)], cumulative, bounds.IsValid ? bounds.Min : Point3d.Origin);
            return triangles.Length > 0 && double.IsFinite(totalArea) && totalArea > EpsilonPolicy.ZeroTolerance && bounds.IsValid
                ? Fin.Succ(unit) : Fin.Fail<Unit>(new KernelFault.InvalidResult());
        }
        private Option<Point3d> SurfaceSample(int attempt) {
            double target = areaDraw.At(attempt).Unit * totalArea;
            int hit = System.Array.BinarySearch(array: cumulativeArea, value: target);
            DworkTriangle triangle = triangles[hit < 0 ? Math.Min(val1: ~hit, val2: triangles.Length - 1) : hit];
            double u = Math.Sqrt(d: barycentricDraw.At(attempt, 0L).Unit);
            double v = barycentricDraw.At(attempt, 1L).Unit;
            (double wa, double wb, double wc) = (1.0 - u, u * (1.0 - v), u * v);
            Point3d sample = new(x: (wa * triangle.A.X) + (wb * triangle.B.X) + (wc * triangle.C.X), y: (wa * triangle.A.Y) + (wb * triangle.B.Y) + (wc * triangle.C.Y), z: (wa * triangle.A.Z) + (wb * triangle.B.Z) + (wc * triangle.C.Z));
            return sample.IsValid ? Some(sample) : Option<Point3d>.None;
        }
        private Option<DworkSurfacePoint> RadiusAt(Point3d point) =>
            radius.SampleScalar(sample: point, context: context).Match(
                Succ: value => value > 0.0 && double.IsFinite(value) ? Some(new DworkSurfacePoint(Point: point, Radius: Math.Max(val1: minRadius, val2: value))) : Option<DworkSurfacePoint>.None,
                Fail: _ => Option<DworkSurfacePoint>.None);
        private Option<DworkSurfacePoint> AnnulusCandidate(DworkSurfacePoint parent, int pops, int attempt) =>
            TangentFrameAt(point: parent.Point).Bind(frame => {
                double reach = radiusBand.Map(static band => band.Max).IfNone(parent.Radius);
                double angle = 2.0 * Math.PI * annulusDraw.At(pops, attempt, 0L).Unit;
                double distance = parent.Radius * Math.Sqrt(d: 1.0 + (3.0 * annulusDraw.At(pops, attempt, 1L).Unit));
                Point3d raw = parent.Point + (distance * ((Math.Cos(d: angle) * frame.Tangent) + (Math.Sin(a: angle) * frame.Bitangent)));
                return SurfaceHit(sample: raw, searchDistance: distance + reach + context.Absolute.Value)
                    .Filter(hit => hit.Point.DistanceTo(other: parent.Point) >= parent.Radius
                        && hit.Point.DistanceTo(other: parent.Point) <= (2.0 * parent.Radius) + context.Absolute.Value)
                    .Bind(hit => RadiusAt(point: hit.Point));
            });
        private Option<MeshPoint> SurfaceHit(Point3d sample, double searchDistance) =>
            Optional(mesh.ClosestMeshPoint(testPoint: sample, maximumDistance: searchDistance))
                .Filter(hit => hit.FaceIndex >= 0 && hit.FaceIndex < mesh.Faces.Count);
        private Option<(Vector3d Tangent, Vector3d Bitangent)> TangentFrameAt(Point3d point) {
            if (SurfaceHit(sample: point, searchDistance: Math.Max(val1: minRadius, val2: context.Absolute.Value)).Case is not MeshPoint hit) return Option<(Vector3d, Vector3d)>.None;
            Vector3d normal = mesh.NormalAt(meshPoint: hit);
            if (!normal.IsValid || normal.IsTiny(context.Absolute.Value) || !normal.Unitize()) return Option<(Vector3d, Vector3d)>.None;
            Vector3d tangent = VectorFrame.SeedPerpendicular(axis: normal);
            if (!tangent.IsValid || tangent.IsTiny(context.Absolute.Value) || !tangent.Unitize()) return Option<(Vector3d, Vector3d)>.None;
            Vector3d bitangent = Vector3d.CrossProduct(a: normal, b: tangent);
            return bitangent.IsValid && !bitangent.IsTiny(context.Absolute.Value) && bitangent.Unitize()
                ? Some((Tangent: tangent, Bitangent: bitangent))
                : Option<(Vector3d, Vector3d)>.None;
        }
        private bool Conflicts(DworkSurfacePoint candidate) {
            double reach = radiusBand.Map(static band => band.Max).IfNone(candidate.Radius);
            int range = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: reach) / cellSize));
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
            radiusBand = Some(Widen(radiusBand, sample.Radius));
            DworkCell cell = CellOf(point: sample.Point);
            if (!grid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; grid.Add(key: cell, value: bucket); }
            bucket.Add(item: index);
        }
        private DworkCell CellOf(Point3d point) =>
            new(X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize), Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize), Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
        private Fin<Selection> Settle() {
            (double radiusMin, double radiusMax) = radiusBand.IfNone((minRadius, minRadius));
            DworkCensus dwork = new(CandidateOnly: false, RMin: minRadius, BackgroundCellSize: cellSize, BackgroundGridCells: grid.Count,
                AttemptsPerActive: attempts.Value, GeneratedCandidates: tally.Proposals, ActivePops: tally.ActivePops,
                RejectedTooClose: tally.TooClose, RejectedDomain: tally.Domain, LocalRadiusMin: radiusMin, LocalRadiusMax: radiusMax);
            return Fin.Succ(new Selection(Points: [.. chosen.Select(static sample => sample.Point)], Mass: Option<Arr<double>>.None,
                DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None,
                Algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None,
                    OversampleCount: Some(tally.Proposals),
                    ActivePops: Some(tally.ActivePops), RejectedTooClose: Some(tally.TooClose), RejectedDomain: Some(tally.Domain),
                    LocalRadiusMin: Some(radiusMin), LocalRadiusMax: Some(radiusMax), Dwork: Some(dwork))));
        }
    }

    private static Fin<Selection> PoissonDiskSelection(SampleKind kind, Seq<Candidate> candidates, PositiveMagnitude radius, Dimension attempts, int seed) {
        (double r2, double r4) = (radius.Value * radius.Value, 4.0 * radius.Value * radius.Value);
        if (candidates.IsEmpty || !double.IsFinite(r4)) return Fin.Fail<Selection>(new KernelFault.InvalidInput());
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count).OrderBy(i => Deterministic.OrderKey(point: candidates[index: i].Point, seed: seed))];
        Dictionary<DworkCell, List<int>> grid = new();
        DworkCell CellOf(Point3d point) => new(X: (long)Math.Floor(d: point.X / radius.Value), Y: (long)Math.Floor(d: point.Y / radius.Value), Z: (long)Math.Floor(d: point.Z / radius.Value));
        void GridAdd(int index) {
            DworkCell cell = CellOf(candidates[index: index].Point);
            if (!grid.TryGetValue(key: cell, value: out List<int>? bucket)) { bucket = []; grid.Add(key: cell, value: bucket); }
            bucket.Add(item: index);
        }
        bool Conflicts(Point3d point) {
            DworkCell home = CellOf(point);
            for (long dx = -1; dx <= 1; dx++) for (long dy = -1; dy <= 1; dy++) for (long dz = -1; dz <= 1; dz++)
                if (grid.TryGetValue(key: new DworkCell(X: home.X + dx, Y: home.Y + dy, Z: home.Z + dz), value: out List<int>? bucket))
                    for (int i = 0; i < bucket.Count; i++) { if (candidates[index: bucket[index: i]].Point.DistanceToSquared(other: point) < r2) return true; }
            return false;
        }
        Deterministic.Draw activeDraw = Deterministic.Of(seed, SampleLane.Active);
        Deterministic.Draw annulusDraw = Deterministic.Of(seed, SampleLane.Annulus);
        List<int> chosen = [order[0]];
        List<int> active = [order[0]];
        GridAdd(index: order[0]);
        (int activePops, int tooClose, int outside) = (0, 0, 0);
        while (active.Count > 0) {
            int activeOffset = (int)(activeDraw.At(activePops).State % (ulong)active.Count);
            int parent = active[activeOffset];
            int pops = activePops;
            TrialState<int, (int TooClose, int Outside)> trial = Trial<int, (int TooClose, int Outside)>(
                budget: attempts, seed: (0, 0),
                propose: (attempt, band) => {
                    int candidate = order[(int)(annulusDraw.At(pops, attempt).State % (ulong)order.Length)];
                    double fromParent = candidates[index: parent].Point.DistanceToSquared(other: candidates[index: candidate].Point);
                    return fromParent < r2 || fromParent > r4 ? (Option<int>.None, (band.TooClose, band.Outside + 1))
                        : Conflicts(point: candidates[index: candidate].Point) ? (Option<int>.None, (band.TooClose + 1, band.Outside))
                        : (Some(candidate), band);
                });
            (tooClose, outside) = (tooClose + trial.Tally.TooClose, outside + trial.Tally.Outside);
            trial.Drawn.Match(
                Some: candidate => { chosen.Add(item: candidate); active.Add(item: candidate); GridAdd(index: candidate); },
                None: () => active.RemoveAt(index: activeOffset));
            activePops++;
        }
        return SelectionOf(candidates: candidates, indices: [.. chosen.Distinct()],
            algorithm: new SampleAlgorithm(Kind: kind,
                Assurances: active.Count == 0 ? CapabilitySet<SampleAssurance>.Of(SampleAssurance.MaximalCoverage) : CapabilitySet<SampleAssurance>.None,
                ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(outside)));
    }

    private const int UnassignedSite = -1;
    private static Option<(int[] Hits, int Assigned, int Unassigned, double Residual)> AssignUnderCapacity(Seq<Candidate> candidates, int[] sites, int limit) {
        if (candidates.IsEmpty || sites.Length == 0 || limit < 1) return None;
        int[] hits = new int[candidates.Count];
        int[] fill = new int[sites.Length];
        (int assigned, int rejected) = (0, 0);
        for (int i = 0; i < candidates.Count; i++) {
            Option<(int Site, double Distance)> nearest = toSeq(Enumerable.Range(start: 0, count: sites.Length)
                .Where(s => fill[s] < limit)
                .Select(s => (Site: s, Distance: candidates[index: i].Point.DistanceToSquared(other: candidates[index: sites[s]].Point))))
                .Fold(Option<(int Site, double Distance)>.None, static (best, item) => best.Map(held => item.Distance < held.Distance ? item : held).IfNone(item));
            (hits[i], assigned, rejected) = nearest.Match(
                Some: hit => { fill[hit.Site]++; return (hit.Site, assigned + 1, rejected); },
                None: () => (UnassignedSite, assigned, rejected + 1));
        }
        return Some((Hits: hits, Assigned: assigned, Unassigned: rejected, Residual: (double)rejected / candidates.Count));
    }
    private static Fin<Selection> CapacityCvtSelection(SampleKind kind, Seq<Candidate> candidates, int count, int limit, int iterations, double tolerance) =>
        RelaxationSample(candidates: candidates, count: count, iterations: iterations, capacity: Some(limit)).Bind(relaxed => {
            Option<(int[] Hits, int Assigned, int Unassigned, double Residual)> assignment = AssignUnderCapacity(candidates: candidates, sites: relaxed.Indices, limit: limit);
            return SelectionOf(candidates: candidates, indices: relaxed.Indices,
                algorithm: new SampleAlgorithm(Kind: kind,
                    Assurances: assignment.Exists(held => held.Unassigned == 0 && held.Residual <= tolerance)
                        ? CapabilitySet<SampleAssurance>.Of(SampleAssurance.CapacityResidual)
                        : CapabilitySet<SampleAssurance>.None,
                    CapacityResidual: assignment.Map(static held => held.Residual),
                    CapacityAssignedCandidates: assignment.Map(static held => held.Assigned),
                    CapacityUnassignedCandidates: assignment.Map(static held => held.Unassigned),
                    CandidatePoolTruncatedTo: relaxed.TruncatedTo));
        });

    private static Fin<(int[] Indices, SampleAlgorithm Algorithm)> SampleElimination(SampleKind kind, Seq<Candidate> candidates, int count, double alpha, double beta, double gamma, int seed, Option<(int Rank, double Measure)> domainMeasure) {
        Candidate[] input = [.. candidates.AsIterable()];
        (int rank, double measure) = domainMeasure.IfNone(BoundingMeasure(candidates: candidates));
        double dMax = MaxRadius(rank, measure, count);
        double dMin = dMax * (1.0 - Math.Pow(x: (double)count / input.Length, y: gamma)) * beta;
        if (input.Length <= count || count <= 0 || !double.IsFinite(dMax) || dMax <= 0.0 || !double.IsFinite(dMin) || dMin < 0.0)
            return Fin.Fail<(int[] Indices, SampleAlgorithm Algorithm)>(new KernelFault.InvalidInput());
        UndirectedGraph<int, TaggedEdge<int, double>> ConflictGraph(int[][] ids) {
            UndirectedGraph<int, TaggedEdge<int, double>> graph = new(allowParallelEdges: false);
            graph.AddVertexRange(vertices: Enumerable.Range(start: 0, count: input.Length));
            for (int i = 0; i < ids.Length && i < input.Length; i++)
                foreach (int j in ids[i]) {
                    if (j <= i || j >= input.Length) continue;
                    double distance = Math.Max(val1: input[i].Point.DistanceTo(other: input[j].Point), val2: dMin);
                    graph.AddEdge(edge: new TaggedEdge<int, double>(source: i, target: j, tag: Math.Pow(x: Math.Max(val1: 0.0, val2: 1.0 - (distance / dMax)), y: alpha)));
                }
            return graph;
        }
        (int[] Indices, int Eliminated, int NeighborUpdates) Eliminate(UndirectedGraph<int, TaggedEdge<int, double>> graph) {
            double[] weights = new double[input.Length];
            foreach (TaggedEdge<int, double> edge in graph.Edges) { weights[edge.Source] += edge.Tag; weights[edge.Target] += edge.Tag; }
            bool[] active = [.. Enumerable.Repeat(element: true, count: input.Length)];
            int[] rank = new int[input.Length];
            foreach ((int Index, int Position) row in Enumerable.Range(start: 0, count: input.Length)
                .OrderBy(i => Deterministic.OrderKey(point: input[i].Point, seed: seed))
                .Select(static (index, position) => (Index: index, Position: position)))
                rank[row.Index] = row.Position;
            BinaryQueue<int, (double NegativeWeight, int Rank)> frontier = new(i => (-weights[i], rank[i]));
            for (int i = 0; i < input.Length; i++) frontier.Enqueue(i);
            (int activeCount, int eliminated, int updates) = (input.Length, 0, 0);
            while (activeCount > count && frontier.Count > 0) {
                int remove = frontier.Dequeue();
                if (!active[remove]) continue;
                active[remove] = false; activeCount--; eliminated++;
                foreach (TaggedEdge<int, double> edge in graph.AdjacentEdges(vertex: remove)) {
                    int other = edge.GetOtherVertex(vertex: remove);
                    if (!active[other]) continue;
                    weights[other] -= edge.Tag;
                    frontier.Update(other);
                    updates++;
                }
            }
            return ([.. Enumerable.Range(start: 0, count: input.Length).Where(i => active[i]).OrderBy(i => rank[i])], eliminated, updates);
        }
        return from index in NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(input.Select(static item => item.Point))))
               from reach in FactoryBridge.Accept<PositiveMagnitude>(candidate: dMax)
               from neighbors in NeighborKernel.GraphOf(index: index, needles: [.. input.Select(static item => item.Point)], count: Option<Dimension>.None, radius: Some(reach))
               let run = Eliminate(ConflictGraph(neighbors.Ids))
               select (Indices: run.Indices,
                   Algorithm: new SampleAlgorithm(Kind: kind, Assurances: CapabilitySet<SampleAssurance>.None,
                       OversampleCount: Some(input.Length),
                       EliminationRadius: Some(dMax), WeightLimitRadius: Some(dMin), Eliminated: Some(run.Eliminated), NeighborUpdates: Some(run.NeighborUpdates)));
    }
    private static double MeanSpacing(int rank, double measure, int count) =>
        Math.Pow(measure / Math.Max(1, count), 1.0 / rank);
    private static double MaxRadius(int rank, double measure, int count) =>
        2.0 * Math.Pow(measure / Math.Max(1, count)
            / (rank >= 3 ? 4.0 * Math.Sqrt(2.0) : 2.0 * Math.Sqrt(3.0)), 1.0 / rank);
    private static (int Rank, double Measure) BoundingMeasure(Seq<Candidate> candidates) {
        BoundingBox box = new(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        (double dx, double dy, double dz) = (Math.Max(val1: box.Max.X - box.Min.X, val2: 0.0), Math.Max(val1: box.Max.Y - box.Min.Y, val2: 0.0), Math.Max(val1: box.Max.Z - box.Min.Z, val2: 0.0));
        double volume = dx * dy * dz;
        double area = Math.Max(val1: dx * dy, val2: Math.Max(val1: dx * dz, val2: dy * dz));
        return volume > EpsilonPolicy.ZeroTolerance
            ? (3, volume)
            : (2, Math.Max(val1: area, val2: EpsilonPolicy.ZeroTolerance));
    }
    private static int[] FarthestIndices(Seq<Candidate> candidates, int count) {
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
    private static int[] OptimizeFarthest(Seq<Candidate> candidates, int count, int iterations) {
        Option<(int Index, double Distance)> Worst(int[] chosen) =>
            candidates.Count <= 0 || chosen.Length <= 0
                ? None
                : Some(Enumerable.Range(start: 0, count: candidates.Count)
                    .Select(i => (Index: i, Distance: chosen.Min(c => candidates[index: i].Point.DistanceToSquared(other: candidates[index: c].Point))))
                    .Aggregate((worst, item) => item.Distance > worst.Distance ? item : worst));
        (int[] Chosen, double BestScore, bool Settled) Swap((int[] Chosen, double BestScore, bool Settled) state) =>
            Worst(state.Chosen).Bind(worst =>
                state.Chosen.Contains(value: worst.Index)
                    ? Option<(int[] Chosen, double BestScore, bool Settled)>.None
                    : Enumerable.Range(start: 0, count: state.Chosen.Length)
                        .Select(i => {
                            int[] trial = [.. state.Chosen];
                            trial[i] = worst.Index;
                            return Worst(trial)
                                .Filter(scored => scored.Distance < state.BestScore)
                                .Map(scored => (Chosen: trial, BestScore: scored.Distance, Settled: false));
                        })
                        .FirstOrDefault(static swapped => swapped.IsSome))
            .IfNone((state.Chosen, state.BestScore, true));
        int[] seeded = FarthestIndices(candidates: candidates, count: count);
        return seeded.Length < 2
            ? seeded
            : Worst(seeded)
                .Map(worst => Cell.Converge(
                    cell: Atom(value: (Chosen: seeded, BestScore: worst.Distance, Settled: false)),
                    step: state => Some(Swap(state)),
                    settled: static state => state.Settled, budget: Dimension.Create(value: Math.Max(val1: 1, val2: iterations)),
                    declined: new KernelFault.InvalidResult()).Current.Chosen)
                .IfNone(seeded);
    }
    private static Fin<(int[] Indices, Option<int> TruncatedTo)> RelaxationSample(Seq<Candidate> candidates, int count, int iterations, Option<int> capacity) {
        int total = capacity.Map(limit => Math.Min(val1: candidates.Count, val2: count * limit)).IfNone(candidates.Count);
        bool truncated = total != candidates.Count;
        int[] retained = truncated ? FarthestIndices(candidates: candidates, count: total) : [];
        Seq<Candidate> active = truncated ? toSeq(retained.Select(i => candidates[index: i])) : candidates;
        Fin<int[]> RelaxSites(int[] sites, NeighborIndex candidateIndex) {
            if (sites.Length == 0) return Fin.Succ(sites);
            Fin<int[]> assigned = capacity.Match(
                Some: limit => AssignUnderCapacity(candidates: active, sites: sites, limit: limit).Map(static held => held.Hits).ToFin(new KernelFault.InvalidResult()),
                None: () => NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(sites.Select(site => active[index: site].Point))))
                    .Bind(siteIndex => FactoryBridge.Accept<Dimension>(candidate: 1).Bind(one => NeighborKernel.GraphOf(index: siteIndex, needles: [.. active.AsIterable().Select(static candidate => candidate.Point)], count: Some(one), radius: Option<PositiveMagnitude>.None)))
                    .Bind(graph => Try.lift(() => {
                        int[] hits = new int[active.Count];
                        for (int i = 0; i < active.Count; i++) {
                            if (graph.Ids.Length <= i || graph.Ids[i].Length == 0) return Fin.Fail<int[]>(new KernelFault.InvalidResult());
                            hits[i] = graph.Ids[i][0];
                        }
                        return Fin.Succ(hits);
                    }).Run().Bind(static inner => inner)));
            return assigned.Bind(hits => {
                Vector3d[] sums = new Vector3d[sites.Length];
                int[] counts = new int[sites.Length];
                for (int i = 0; i < active.Count; i++) { if (hits[i] < 0) continue; sums[hits[i]] += (Vector3d)active[index: i].Point; counts[hits[i]]++; }
                Point3d[] centroids = [.. Enumerable.Range(start: 0, count: sites.Length).Select(s => counts[s] > 0 ? Point3d.Origin + (sums[s] / counts[s]) : active[index: sites[s]].Point)];
                return FactoryBridge.Accept<Dimension>(candidate: active.Count)
                    .Bind(bound => NeighborKernel.GraphOf(index: candidateIndex, needles: centroids, count: Some(bound), radius: Option<PositiveMagnitude>.None)).Bind(snap => Try.lift(() => {
                    int[] next = new int[sites.Length];
                    IndexSet occupied = [];
                    for (int s = 0; s < sites.Length; s++) {
                        if (snap.Ids.Length <= s) return Fin.Fail<int[]>(new KernelFault.InvalidResult());
                        Option<int> site = None;
                        foreach (int candidate in snap.Ids[s]
                            .Where(id => id >= 0 && id < active.Count)
                            .Distinct()
                            .OrderBy(id => centroids[s].DistanceToSquared(other: active[index: id].Point))
                            .ThenBy(static id => id)) {
                            if (!occupied.Add(item: candidate)) continue;
                            site = Some(candidate);
                            break;
                        }
                        if (site.Case is not int taken) return Fin.Fail<int[]>(new KernelFault.InvalidResult());
                        next[s] = taken;
                    }
                    return Fin.Succ(next);
                }).Run().Bind(static inner => inner));
            });
        }
        return NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(active.AsIterable().Select(static candidate => candidate.Point))))
            .Bind(candidateIndex => toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
                initialState: Fin.Succ(FarthestIndices(candidates: active, count: count)),
                f: (state, _) => state.Bind(sites => RelaxSites(sites, candidateIndex))))
            .Map(indices => (Indices: truncated ? [.. indices.Select(i => retained[i])] : indices, TruncatedTo: truncated ? Some(total) : Option<int>.None));
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
