# [RASM_VECTORS_SAMPLE]

`SampleKind` owns point sampling: every draw is one closed `[Union]` case admitted at its factory, and `SampleKernel.Sample` folds all cases through one domain dispatch total over admitted kinds. Grouped `PowerCcvtPolicy.Preset` mints the BNOT tuning surface on the `Op` rail, one `with` mutation overrides it, and every convergence threshold reads its own `ToleranceLane` at the run.

Rebuild work composes settled owners: `extract.md` `ExtractionDomain` carries the domain axis, `intent.md` the consumer rail, `evaluation.md` `Evaluate(EvaluationRequest.Sample(...))` the support-space candidate draw, `matrix.md` `SparseMatrix.SingularSolveDetailed` the gauge-fixed solve, `segment.md` `SegmentKernel.ValidateSamplingSpectrum` the blue-noise witness, `mesh.md` `RestrictedPowerDiagram` the restricted power cells this page reads one-directionally, and `identity.md` `Deterministic` every draw addresses through a declared `SampleLane` ordinal.

## [01]-[INDEX]

- [02]-[SAMPLING]: `SampleKind` mints the draw vocabulary `SampleKernel` folds onto every extraction domain.
- [03]-[POWER_CCVT]: `PowerCcvtRun` solves the continuous BNOT sampler under one grouped policy value.

## [02]-[SAMPLING]

- Owner: `SampleKind` `[Union]` mints every draw case, `SampleLane` the declared draw-ordinal roster, `SampleAssurance` the guarantee vocabulary the receipts publish, and each discriminating axis rides its own closed typed vocabulary.
- Cases: `SampleAlgorithmKind` carries twelve rows — the closed roster of published blue-noise and CVT algorithms this page realizes (Bridson, Yuksel elimination, Dwork variable-density and its adaptive sibling, de Goes BNOT, capacity-limited Lloyd, farthest-point and its optimizing variant), each row's `CandidateScale` the oversample factor its selection needs; `SpatialRank` carries the two ambient ranks with their published packing densities. Upstream is the sampling literature, and a row without a suite arm cannot construct.
- Entry: each case factory admits raw scalars through `Op.AcceptValidated` under the case's own `Admit` invariants; `Project<TOut>` is the one evaluation entry, `TOut` selecting the output shape.
- Auto: `SampleKernel.Sample` discriminates on domain shape alone — supplied points project, generated kinds draw an oversampled candidate pool the selection suite reduces — and a kind whose candidates a domain cannot supply stays typed `Unsupported`.
- Law: `SampleKernel.Trial` is the ONE bounded rejection fold — an attempt budget, a proposal, and a caller-shaped tally. Every rejection sampler on this page instantiates it, a drawn state is the explicit committed settlement fact, and an exhausted budget yields `None`, which each caller lowers to its own typed terminal instead of a success-shaped fall-through. Every other bounded iterate on the page — the outer CCVT schedule, the dual Newton, the farthest-point-optimization swap — rides the same owner.
- Law: a draw addresses a DECLARED `SampleLane` ordinal, never a hand-packed salt or a seed the caller offsets — `Deterministic.Draw.At(lanes)` keys the multi-level draws (active-list pick, per-attempt annulus pick, barycentric coordinate) and `Deterministic.UnitInterval(point, lane.Lane, seed)` keys the position-stable exponential races off the ONE lane ordinal, so two samplers under one seed never interleave and no draw index collides with another's.
- Receipt: `SampleReceipt` nests `SampleAlgorithmReceipt`, so every algorithm's facts ride one evidence stream, never a parallel receipt type per algorithm; spacing rides ONE `Distribution<Scalar>` over nearest-neighbour distances rather than a min/mean/max triple beside an all-pairs mean that measured a different quantity under the same word; `SampleAlgorithmReceipt.Assurances` is the ONE guarantee column over `SampleAssurance`, replacing four independent bool flags whose corners the roster fixes.
- Packages: RhinoCommon is the one boundary-admitted host surface; every other member composes the `Rasm` substrate.
- Growth: a new algorithm is one `SampleKind` case, one `SampleAlgorithmKind` row, and one suite arm the total `Switch` breaks on; a new per-algorithm fact is one `SampleAlgorithmReceipt` field; a new guarantee is one `SampleAssurance` row; a new draw purpose is one `SampleLane` row; a new candidate domain ripens one `ExtractionDomain` case into a dispatch arm.
- Exemption: the candidate-suite kernels are the named statement-kernel exemption — hot spatial loops with typed-receipt egress. Their background `Dictionary` hashes survive only where the set GROWS per admission: `NeighborIndex` publishes no incremental insert, so a Bridson active-list frontier cannot compose it without rebuilding per accepted sample, while every FROZEN point set — the candidate cloud, the alias mask, the conflict graph, every spacing read — routes the one neighbourhood owner. `Spacing` folds every reference scale from the domain measure, never an absolute literal; a candidate shortfall terminates `CandidateExhausted` carrying the count on the receipt.

## [03]-[POWER_CCVT]

- Owner: `PowerCcvtPolicy` composes every BNOT tuning axis into one nested policy value whose single `ArmijoPolicy` line search serves both the dual-Newton and the site-motion ascent; `PowerCcvtGauge` mints the `matrix.md` `GaugePolicy` row and owns the Hessian nullspace fix.
- Entry: `SampleKind.PowerCcvt` mints `PowerCcvtPolicy.Preset(key)`; a preset `with { … }` re-admitted through `policy.Admit` is the whole override surface, and a throwing factory inside a static initializer is the deleted form.
- Auto: `PowerCcvtRun` executes BNOT on the mesh — capacity Newton enforces cell mass through the power-graph Laplacian under the selected gauge, then two-phase site motion runs Lloyd sweeps into Armijo transport-energy ascent, both loops stopping on scale-relative tolerances floored by their own `ToleranceLane` rows; its terminal breaks lattice regularity once and lifts every site to the surface.
- Law: the Armijo line search returns its rebuilt diagram as `Option` — absence IS non-improvement, so the search carries no `bool Improved` beside a `default` diagram no arm may read.
- Receipt: `PowerCcvtReceipt` folds the run's evidence with its `PowerCellFragmentFacts` and composed solve children into one stream — cell mass and dual weights each riding ONE `Stat<Scalar>` off their own column — and `MeanZeroGaugeApplied` cross-checks the applied gauge against the solve's `GaugeShift`. Rebuilds are counted at the one rebuild owner and sites the surface refuses to accept census on `UnliftedSiteCount`.
- Growth: a new gauge is one `PowerCcvtGauge` row minting its `GaugePolicy`; a new motion schedule or line-search variant is one policy-record field on the same run; a density-transport variant is a `MotionPolicy` column; a new convergence gate is one `ToleranceLane` read, never a stored epsilon.
- Boundary: the per-iteration diagram rebuild, triplet assembly, and Armijo searches are the named statement-kernel exemption while the outer schedules stay domain flow; continuous BNOT transport is its own estimator, distinct from the `transport.md` discrete Sinkhorn plan.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
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
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms and LanguageExt.HashSet collides with the BCL name.
using Dimension = Rasm.Numerics.Dimension;
using IndexSet = System.Collections.Generic.HashSet<int>;

namespace Rasm.Processing;

// --- [TYPES] ----------------------------------------------------------------------------------
// Draw ordinals are DECLARED here and nowhere else: a hand-packed salt or a caller-offset seed collides two draws
// whose lanes differ, which is exactly the defect the lane fold exists to close.
[SmartEnum<int>]
public sealed partial class SampleLane : IDrawLane<SampleLane> {
    public static readonly SampleLane Priority    = new(key: 0);
    public static readonly SampleLane Active      = new(key: 1);
    public static readonly SampleLane Annulus     = new(key: 2);
    public static readonly SampleLane Area        = new(key: 3);
    public static readonly SampleLane Barycentric = new(key: 4);
    public static readonly SampleLane Jitter      = new(key: 5);

    // ONE ordinal, one authority: a second column mirroring the key by hand splits the draw streams the first time
    // a row mints a lane the key does not match, and nothing raises.
    public long Lane => Key;
}

// Four guarantees ride a sampled draw. They co-occur in corners the algorithm roster fixes — a Poisson draw
// can be maximal and spectrum-validated but never transport-assigned — so one set replaces four bool columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SampleAssurance : ICapability<SampleAssurance> {
    public static readonly SampleAssurance MaximalCoverage     = new("maximal-coverage", rank: 0);
    public static readonly SampleAssurance TransportAssignment = new("transport-assignment", rank: 1);
    public static readonly SampleAssurance MeshSpectrum        = new("mesh-spectrum", rank: 2);
    public static readonly SampleAssurance CapacityResidual    = new("capacity-residual", rank: 3);

    public int Rank { get; }
}

[SmartEnum<int>]
public sealed partial class DworkSamplingDomain {
    public static readonly DworkSamplingDomain ContinuousMesh = new(key: 0);
    public static readonly DworkSamplingDomain CandidateSet = new(key: 1);
}

// Selection kinds NEED a strict oversample: a pool equal to the request degenerates farthest to the identity and fixes Lloyd at iteration zero.
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
    // Adaptive's per-sample local radius rejects harder than a plain density draw, so its oversample is its OWN row's
    // column — borrowing a sibling row and overriding its declared scale makes the roster lie for one case.
    public static readonly SampleAlgorithmKind AdaptiveVariableDensityPoisson = new(key: 11, candidateScale: 12.0, densityDriven: true);

    public double CandidateScale { get; }
    public bool DensityDriven { get; }
}

// Packing densities are published constants of Yuksel 2015 §4 — 2√3 hexagonal in the plane, 4√2 face-centred cubic
// in the volume — and the exponent is the rank's own root. The bounding fold answers a ROW, so no consumer re-tests
// `dimensions == 3` and no radius formula carries the pair as literals.
[SmartEnum<int>]
public sealed partial class SpatialRank {
    public static readonly SpatialRank Planar = new(key: 2, packingDensity: 2.0 * Math.Sqrt(d: 3.0), exponent: 0.5);
    public static readonly SpatialRank Volumetric = new(key: 3, packingDensity: 4.0 * Math.Sqrt(d: 2.0), exponent: 1.0 / 3.0);

    public double PackingDensity { get; }
    public double Exponent { get; }
    // Yuksel d_max: twice the packing radius the rank's own density and exponent define.
    internal double MaxRadius(double measure, int count) =>
        2.0 * Math.Pow(x: measure / Math.Max(val1: 1, val2: count) / PackingDensity, y: Exponent);
    // Mean spacing for a rank-uniform pool: the same root with no packing correction.
    internal double MeanSpacing(double measure, int count) =>
        Math.Pow(x: measure / Math.Max(val1: 1, val2: count), y: Exponent);
    internal static SpatialRank Of(int rank) => rank >= 3 ? Volumetric : Planar;
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

// Capacity Hessian is the power-graph Laplacian, singular on the constant vector — hence one gauge row per dual-weight fix.
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
[BoundaryAdapter]
public abstract partial record SampleKind {
    public sealed record ExplicitCase(Seq<Point3d> Points) : SampleKind;
    public sealed record PoissonDiskCase(PositiveMagnitude Radius, Dimension Attempts, int Seed) : SampleKind;
    public sealed record FarthestCase(Dimension Count) : SampleKind;
    public sealed record OptimizeCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record CapacityCase(Dimension Count, Dimension Limit, Dimension Iterations) : SampleKind;
    public sealed record WeightedCase(Seq<(Point3d Point, double Mass)> Points) : SampleKind;
    // Both density rows carry the run SEED their Poisson, elimination, and Dwork siblings already carry — the
    // weighted-race draw inside their selection is a stochastic step, so a seedless row cannot replay.
    public sealed record ScalarDensityCase(ScalarField Density, Dimension Count, int Seed) : SampleKind;
    public sealed record AdaptiveCase(ScalarField Density, Dimension Count, PositiveMagnitude MinSpacing, int Seed) : SampleKind;
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
    // The residual gate is a lane read at evaluation, so the case stores no epsilon of its own.
    public static Fin<SampleKind> Capacity(int count, int capacity, int iterations = 8, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from limit in op.AcceptValidated<Dimension>(candidate: capacity)
               from iter in op.AcceptValidated<Dimension>(candidate: iterations)
               from admitted in new CapacityCase(Count: c, Limit: limit, Iterations: iter).Admit(key: op)
               select admitted;
    }
    public static Fin<SampleKind> Weighted(Seq<(Point3d Point, double Mass)> points, Op? key = null) =>
        new WeightedCase(Points: points).Admit(key: key.OrDefault());
    public static Fin<SampleKind> ScalarDensity(ScalarField density, int count, int seed, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(c => new ScalarDensityCase(Density: density, Count: c, Seed: seed).Admit(key: op));
    }
    public static Fin<SampleKind> Adaptive(ScalarField density, int count, double minSpacing, int seed, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from spacing in op.AcceptValidated<PositiveMagnitude>(candidate: minSpacing)
               from admitted in new AdaptiveCase(Density: density, Count: c, MinSpacing: spacing, Seed: seed).Admit(key: op)
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
    public static Fin<SampleKind> PowerCcvt(int count, Option<PowerCcvtPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from active in policy.Match(Some: held => held.Admit(key: op), None: () => PowerCcvtPolicy.Preset(key: op))
               from admitted in new PowerCcvtCase(Count: c, Policy: active).Admit(key: op)
               select admitted;
    }

    // Op.Need is the null gate: the Admit member shadows the Rasm.Domain.Admit class inside this type.
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
    internal (Option<int> Count, Option<int> Iterations, double CandidateScale, SampleAlgorithmKind Algorithm) Facts => Switch(
        explicitCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.Explicit),
        poissonDiskCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.BridsonActiveListPoisson),
        farthestCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.FarthestCandidate.CandidateScale, SampleAlgorithmKind.FarthestCandidate),
        optimizeCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), SampleAlgorithmKind.FarthestOptimize.CandidateScale, SampleAlgorithmKind.FarthestOptimize),
        lloydCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), SampleAlgorithmKind.LloydCandidateRelaxation.CandidateScale, SampleAlgorithmKind.LloydCandidateRelaxation),
        capacityCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value, SampleAlgorithmKind.CapacityLimitedLloydCandidate),
        weightedCase: static _ => (Option<int>.None, Option<int>.None, 0.0, SampleAlgorithmKind.WeightedMassPropagation),
        scalarDensityCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.VariableDensityPoisson.CandidateScale, SampleAlgorithmKind.VariableDensityPoisson),
        adaptiveCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.AdaptiveVariableDensityPoisson.CandidateScale, SampleAlgorithmKind.AdaptiveVariableDensityPoisson),
        sampleEliminationCase: static c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value, SampleAlgorithmKind.YukselWeightedSampleElimination),
        dworkVariableDensityCase: static c => (Some(c.Count.Value), Option<int>.None, SampleAlgorithmKind.DworkVariableDensity.CandidateScale, SampleAlgorithmKind.DworkVariableDensity),
        powerCcvtCase: static c => (Some(c.Count.Value), Some(c.Policy.Iterations.Value), SampleAlgorithmKind.ContinuousPowerCcvt.CandidateScale, SampleAlgorithmKind.ContinuousPowerCcvt));
    internal Option<double> DensityError(int emitted) =>
        Facts is { Algorithm.DensityDriven: true, Count: Option<int> count }
            ? count.Map(value => Math.Abs(value: emitted - value) / Math.Max(1.0, value))
            : Option<double>.None;
    // Only generated kinds reach this lattice density; supplied points project instead.
    internal Fin<double> MeshCandidateDensity(double area, Op key) {
        double safeArea = Math.Max(val1: area, val2: EpsilonPolicy.ZeroTolerance);
        double target = this switch {
            PoissonDiskCase pd => safeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: EpsilonPolicy.ZeroTolerance),
            _ => Facts.Count.Map(value => value * Math.Max(1.0, Facts.CandidateScale)).IfNone(0.0),
        };
        return double.IsFinite(target) && target > 0.0
            ? key.AcceptValue(value: Math.Max(val1: target / safeArea, val2: 1.0 / safeArea))
            : Fin.Fail<double>(key.Unsupported(inputType: GetType(), outputType: typeof(SampleResult)));
    }
    public Fin<TOut> Project<TOut>(ExtractionDomain domain, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from result in Evaluate(domain: domain, context: context, key: op)
               from output in AtomProjection.Rows<SampleReceipt, TOut>(self: result.Receipt, key: op, owner: typeof(SampleKind),
                   ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(result.Points)),
                   ProjectionRow.Of<VectorCloud>(() => result.Mass.Match(
                       Some: mass => VectorCloud.Cluster(points: result.Points, context: context, mass: Some(mass), key: op),
                       None: () => VectorCloud.Cluster(points: result.Points, context: context, key: op))))
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

// --- [MODELS] ---------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DworkReceipt(
    DworkSamplingDomain Domain, double RMin, Option<double> BackgroundCellSize, Option<int> BackgroundGridCells,
    int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain,
    double LocalRadiusMin, double LocalRadiusMax) : IValidityEvidence {
    public bool CandidateOnly => Domain.Equals(DworkSamplingDomain.CandidateSet);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(RMin),
        BackgroundCellSize.Map(static size => double.IsFinite(size) && size > 0.0).IfNone(noneValue: true),
        BackgroundGridCells.Map(static cells => cells >= 0).IfNone(noneValue: true),
        ValidityClaim.CountAtLeast(AttemptsPerActive, 1), ValidityClaim.CountAtLeast(GeneratedCandidates, 0),
        ValidityClaim.CountAtLeast(ActivePops, 0), ValidityClaim.CountAtLeast(RejectedTooClose, 0),
        ValidityClaim.CountAtLeast(RejectedDomain, 0), ValidityClaim.Ordered(LocalRadiusMin, LocalRadiusMax));
}

// Cell mass is ONE moment summary off its own column: three separate folds beside it forked the statistic per
// consumer, and the total is the summary's own mean times its count rather than a fourth accumulation.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCellFragmentFacts(
    int SiteCount, int FragmentCount, int FacetCount, int EmptyCellCount,
    Stat<Scalar> Mass, double IntegrationResidual) : IValidityEvidence {
    public double TotalMass => Mass.Mean * Mass.Count;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.CountAtLeast(FragmentCount, 0),
        ValidityClaim.CountAtLeast(FacetCount, 0), ValidityClaim.CountAtLeast(EmptyCellCount, 0),
        EmptyCellCount <= SiteCount, ValidityClaim.Evidence(Mass), ValidityClaim.Nonnegative(IntegrationResidual));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCcvtReceipt(
    int SiteCount, double TargetMass,
    double CapacityResidualInf, double CapacityResidualL1, double CapacityResidualL2, double CapacityResidualNormalized,
    int OuterIterations, int LloydIterations, int GradientIterations, int DualNewtonIterations,
    Stat<Scalar> Weights, double TransportEnergy, double TransportEnergyDelta,
    double DualObjective, double CentroidShift, double PositionGradientNorm, double WeightGradientNorm,
    int EmptyCellCount, int StepHalvingCount, int RebuildCount, int AliasedSiteCount, int RelocatedSiteCount, int UnliftedSiteCount,
    double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
    PowerCellFragmentFacts Fragments, Option<SolveReceipt> DualSolve = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default) : IValidityEvidence {
    public bool MeanZeroGaugeApplied =>
        Gauge.Equals(PowerCcvtGauge.ZeroMean)
        && DualSolve.Bind(static solve => solve.Gauge).Map(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero)).IfNone(noneValue: false);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.Positive(TargetMass),
        ValidityClaim.Nonnegative(CapacityResidualInf), ValidityClaim.Nonnegative(CapacityResidualL1),
        ValidityClaim.Nonnegative(CapacityResidualL2), ValidityClaim.Nonnegative(CapacityResidualNormalized),
        ValidityClaim.CountAtLeast(OuterIterations, 0), ValidityClaim.CountAtLeast(LloydIterations, 0),
        ValidityClaim.CountAtLeast(GradientIterations, 0), ValidityClaim.CountAtLeast(DualNewtonIterations, 0),
        ValidityClaim.Evidence(Weights),
        ValidityClaim.Finite(TransportEnergy), ValidityClaim.Finite(TransportEnergyDelta), ValidityClaim.Finite(DualObjective),
        ValidityClaim.Nonnegative(CentroidShift), ValidityClaim.Nonnegative(PositionGradientNorm), ValidityClaim.Nonnegative(WeightGradientNorm),
        EmptyCellCount >= 0 && EmptyCellCount <= SiteCount,
        ValidityClaim.CountAtLeast(StepHalvingCount, 0), ValidityClaim.CountAtLeast(RebuildCount, 0),
        ValidityClaim.CountAtLeast(AliasedSiteCount, 0), ValidityClaim.CountAtLeast(RelocatedSiteCount, 0), ValidityClaim.CountAtLeast(UnliftedSiteCount, 0),
        ValidityClaim.UnitInterval(NormalizedPoissonRadius), ValidityClaim.Nonnegative(PlanarityDeviation),
        Fragments.SiteCount == SiteCount, Fragments.IsValid,
        ValidityClaim.Evidence(DualSolve), ValidityClaim.Evidence(Spectrum));
}

// Assurances is the ONE guarantee column: a bool per guarantee would let a producer narrow one flag without any
// consumer seam noticing, where a set states its whole content and every reader tests it with AdmitsAll.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleAlgorithmReceipt(
    SampleAlgorithmKind Kind, CapabilitySet<SampleAssurance> Assurances,
    Option<int> Seed = default, Option<int> TargetCount = default, Option<int> OversampleCount = default, Option<int> OversampleFactor = default,
    Option<double> Alpha = default, Option<double> Beta = default, Option<double> Gamma = default, Option<double> Radius = default, Option<double> WeightLimitRadius = default,
    Option<int> Eliminated = default, Option<int> NeighborUpdates = default,
    Option<int> Attempts = default, Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default,
    Option<double> DensityMin = default, Option<double> DensityMax = default, Option<double> LocalRadiusMin = default, Option<double> LocalRadiusMax = default,
    Option<double> CapacityResidual = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default, Option<DworkReceipt> Dwork = default,
    Option<int> CapacityAssignedCandidates = default, Option<int> CapacityUnassignedCandidates = default,
    Option<int> CandidatePoolTruncatedTo = default, Option<PowerCcvtReceipt> PowerCcvt = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Kind is not null,
        TargetCount.Map(static count => count >= 0).IfNone(noneValue: true),
        OversampleCount.Map(static count => count >= 0).IfNone(noneValue: true),
        Attempts.Map(static count => count >= 1).IfNone(noneValue: true),
        ActivePops.Map(static count => count >= 0).IfNone(noneValue: true),
        Eliminated.Map(static count => count >= 0).IfNone(noneValue: true),
        NeighborUpdates.Map(static count => count >= 0).IfNone(noneValue: true),
        RejectedTooClose.Map(static count => count >= 0).IfNone(noneValue: true),
        RejectedDomain.Map(static count => count >= 0).IfNone(noneValue: true),
        CandidatePoolTruncatedTo.Map(static count => count >= 0).IfNone(noneValue: true),
        Radius.Map(static radius => double.IsFinite(radius) && radius > 0.0).IfNone(noneValue: true),
        WeightLimitRadius.Map(static radius => double.IsFinite(radius) && radius >= 0.0).IfNone(noneValue: true),
        CapacityResidual.Map(static residual => double.IsFinite(residual) && residual >= 0.0).IfNone(noneValue: true),
        DensityMin.Bind(min => DensityMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true),
        LocalRadiusMin.Bind(min => LocalRadiusMax.Map(max => (bool)ValidityClaim.Ordered(min, max))).IfNone(noneValue: true),
        ValidityClaim.Evidence(Dwork), ValidityClaim.Evidence(Spectrum), ValidityClaim.Evidence(PowerCcvt));
}

// Spacing is ONE order-statistic summary over the NEAREST-neighbour distances: a min/mean/max triple beside a
// mean-of-all-pairs fold published two incompatible quantities under one word.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleReceipt(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations,
    SampleStopKind Stop, SampleDomainStatus DomainStatus, Option<SampleAlgorithmReceipt> Algorithm) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0), ValidityClaim.CountAtLeast(Rejected, 0),
        Emitted <= Attempted + Rejected,
        DensityError.Map(static error => double.IsFinite(error) && error >= 0.0).IfNone(noneValue: true),
        ValidityClaim.Evidence(Spacing), ValidityClaim.Evidence(Algorithm));
}

// --- [POLICIES] -------------------------------------------------------------------------------
// Records carrying admission and nested composition seat AFTER the models they gate: `[CONSTANTS]` is the
// dependency-free anchor section, and a policy family with an `Admit` body is not one.
public sealed record CapacityPolicy(Dimension MaxNewton);
public sealed record MotionPolicy(Dimension LloydSweeps, Dimension GradientSteps, PositiveMagnitude LloydPosTol, PositiveMagnitude GradPosTol);
public sealed record ArmijoPolicy(PositiveMagnitude Backtrack, PositiveMagnitude InitialStep, Dimension MaxHalvings);
public sealed record RegularityPolicy(PositiveMagnitude AliasScale, PositiveMagnitude JitterVariance, PositiveMagnitude MagnitudeScale, PositiveMagnitude RelocateFraction);

public sealed record PowerCcvtPolicy(
    Dimension Iterations, Option<ScalarField> Density,
    CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity,
    PowerCcvtGauge Gauge, int Seed) {
    // Every convergence threshold reads its own `ToleranceLane` at the run — the residual band, the Newton floor, the
    // sufficient-decrease constant, the motion floor — so this record carries budgets, fractions, and the gauge alone.
    // A throwing factory inside a static field initializer surfaces as a TypeInitializationException no rail catches,
    // so the preset mints on the `Op` rail behind a key and the caller seats it.
    internal static Fin<PowerCcvtPolicy> Preset(Op key) =>
        from iterations in key.AcceptValidated<Dimension>(candidate: 16)
        from maxNewton in key.AcceptValidated<Dimension>(candidate: 32)
        from lloydSweeps in key.AcceptValidated<Dimension>(candidate: 1)
        from gradientSteps in key.AcceptValidated<Dimension>(candidate: 8)
        from lloydPosTol in key.AcceptValidated<PositiveMagnitude>(candidate: 0.01)
        from gradPosTol in key.AcceptValidated<PositiveMagnitude>(candidate: 0.1)
        from backtrack in key.AcceptValidated<PositiveMagnitude>(candidate: 0.5)
        from initialStep in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0)
        from maxHalvings in key.AcceptValidated<Dimension>(candidate: 32)
        from aliasScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.65)
        from jitterVariance in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
        from magnitudeScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.5)
        from relocateFraction in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
        from admitted in new PowerCcvtPolicy(
            Iterations: iterations, Density: Option<ScalarField>.None,
            Capacity: new CapacityPolicy(MaxNewton: maxNewton),
            Motion: new MotionPolicy(LloydSweeps: lloydSweeps, GradientSteps: gradientSteps, LloydPosTol: lloydPosTol, GradPosTol: gradPosTol),
            Search: new ArmijoPolicy(Backtrack: backtrack, InitialStep: initialStep, MaxHalvings: maxHalvings),
            Regularity: new RegularityPolicy(AliasScale: aliasScale, JitterVariance: jitterVariance, MagnitudeScale: magnitudeScale, RelocateFraction: relocateFraction),
            Gauge: PowerCcvtGauge.ZeroMean, Seed: 0).Admit(key: key)
        select admitted;
    internal Fin<PowerCcvtPolicy> Admit(Op key) =>
        guard(Search.Backtrack.Value < 1.0 && Regularity.RelocateFraction.Value <= 1.0, key.InvalidInput())
            .ToFin().Map(_ => this);
}

// --- [OPERATIONS] -----------------------------------------------------------------------------
internal readonly record struct SampleCandidate(Point3d Point, Option<double> Mass);
internal readonly record struct SampleResult(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleReceipt Receipt);
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithmReceipt> Algorithm);

internal static class Spacing {
    internal static double Hexagonal(double measure, int count) =>
        Math.Sqrt(d: 2.0 * measure / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: count)));
    // ONE neighbourhood answers every spacing read: the index builds once over the point set and the k=2 graph hands
    // each point its nearest OTHER point, where the all-pairs scans it replaced re-walked every pair per receipt and
    // per outer iterate. The closest pair is a MUTUAL nearest neighbour, so the minimum here IS the global minimum.
    internal static Fin<Seq<double>> Nearest(Seq<Point3d> points, Op key) =>
        points.Count < 2
            ? Fin.Succ(Seq<double>.Empty)
            : from index in NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: points), key: key)
              from graph in NeighborKernel.GraphOf(index: index, needles: [.. points.AsIterable()], count: Some(2), radius: Option<double>.None, key: key)
              select toSeq(Enumerable.Range(start: 0, count: points.Count)).Choose(i =>
                  graph.Ids.Length > i
                      ? toSeq(graph.Ids[i]).Find(id => id != i && id >= 0 && id < points.Count).Map(id => points[index: i].DistanceTo(other: points[index: id]))
                      : Option<double>.None);
    // THE spacing definition every receipt reads — nearest-neighbour order statistics, never a mean chord length of
    // the whole cloud wearing the same word.
    internal static Fin<Distribution<Scalar>> DistributionOf(Seq<Point3d> points, Op key) =>
        from nearest in Nearest(points: points, key: key)
        from distribution in Distribution<Scalar>.Of(values: nearest.Map(static value => (Scalar)value), percentiles: [], key: key)
        select distribution;
    internal static Fin<double> MeanNearest(Seq<Point3d> points, double measure, Op key) =>
        Nearest(points: points, key: key).Map(nearest =>
            nearest.IsEmpty ? Hexagonal(measure: measure, count: points.Count) : nearest.Sum() / nearest.Count);
    internal static Fin<double> NormalizedPoissonRadius(Seq<Point3d> points, double measure, Op key) =>
        Nearest(points: points, key: key).Map(nearest => {
            if (nearest.IsEmpty) return 0.0;
            double minSpacing = nearest.Min();
            double reference = Hexagonal(measure: measure, count: points.Count);
            return double.IsFinite(minSpacing) && double.IsFinite(reference) && reference > EpsilonPolicy.ZeroTolerance
                ? Math.Clamp(value: minSpacing / reference, min: 0.0, max: 1.0)
                : 0.0;
        });
}

internal static class SampleKernel {
    // --- [TRIAL]
    [StructLayout(LayoutKind.Auto)] internal readonly record struct TrialState<T, TTally>(Option<T> Drawn, TTally Tally, int Attempts);

    // THE bounded rejection fold every sampler on this page instantiates: `budget` attempts, one proposal per attempt,
    // and a caller-shaped tally threaded through. A DRAWN state is the committed settlement fact, while an exhausted
    // budget hands back a `None` draw each caller lowers to its own typed terminal.
    internal static TrialState<T, TTally> Trial<T, TTally>(
        Dimension budget, TTally seed, Func<int, TTally, (Option<T> Drawn, TTally Tally)> propose, Op key) =>
        Cell.Converge(
            cell: Atom(value: new TrialState<T, TTally>(Drawn: Option<T>.None, Tally: seed, Attempts: 0)),
            step: state => propose(state.Attempts, state.Tally) switch {
                var proposed => Some(new TrialState<T, TTally>(Drawn: proposed.Drawn, Tally: proposed.Tally, Attempts: state.Attempts + 1)),
            },
            settled: static state => state.Drawn.IsSome, budget: budget, declined: key.InvalidResult()).Current;

    // --- [DISPATCH]
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
                        admitsPoisson: false, domainMeasure: Option<(SpatialRank Rank, double Measure)>.None, context: state.Context, key: state.Key))
                    : Fin.Fail<SampleResult>(state.Key.Unsupported(inputType: d.Value.GetType(), outputType: typeof(SampleResult))),
                // Texel-grid draws: cell centres are the candidate pool — ceiling-bounded by the lattice admission —
                // and the domain measure is the lattice's own, so the whole blue-noise/Poisson/CCVT family reaches
                // a rank-2 plane and a rank-3 voxel sweep through one arm.
                latticeCase: static (state, d) => SampleOnCandidates(
                    kind: state.Kind, candidates: LatticeCandidates(grid: d.Value), admitsPoisson: true,
                    domainMeasure: Some((Rank: SpatialRank.Of(rank: d.Value.Rank), Measure: d.Value.CellCount * d.Value.CellMeasure)),
                    context: state.Context, key: state.Key)),
        };

    // The roster materializes whole, so the cell count is int-bounded by construction: a lattice past that bound
    // produces no candidate Seq at all and refuses at its own admission.
    private static Seq<SampleCandidate> LatticeCandidates(CellLattice grid) =>
        toSeq(Enumerable.Range(start: 0, count: (int)Math.Min(val1: grid.CellCount, val2: int.MaxValue))
            .Select(linear => grid.Coordinate(linear: linear) switch {
                (int column, int row, int layer) => new SampleCandidate(Point: grid.Center(column: column, row: row, layer: layer), Mass: Option<double>.None),
            }));

    // Supplied points project onto the domain, never accepted raw.
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
                status: SampleDomainStatus.Projected, densityError: Option<double>.None, key: key,
                algorithm: Some(new SampleAlgorithmReceipt(Kind: algorithm, Assurances: CapabilitySet<SampleAssurance>.None))));
    private static Fin<Point3d> AdmitPoint(Point3d point, ExtractionDomain domain, Context context, Op key) =>
        key.AcceptValue(value: point).Bind(valid => domain.Switch(
            state: (Point: valid, Context: context, Key: key),
            supportCase: static (state, d) => d.Value.Closest(sample: state.Point, key: state.Key).Bind(hit => state.Key.AcceptValue(value: hit.Point)),
            meshCase: static (state, d) => Optional(d.Value.Native.ClosestMeshPoint(testPoint: state.Point, maximumDistance: state.Context.Absolute.Value))
                .ToFin(state.Key.InvalidResult()).Bind(meshPoint => state.Key.AcceptValue(value: meshPoint.Point)),
            cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                ? cluster.Vertices.Find(vertex => vertex.DistanceToSquared(other: state.Point) <= state.Context.Absolute.Value * state.Context.Absolute.Value)
                    .ToFin(state.Key.InvalidInput())
                : Fin.Fail<Point3d>(state.Key.Unsupported(inputType: d.Value.GetType(), outputType: typeof(Point3d))),
            // Supplied points project to their containing cell centre; a point outside the lattice extent rejects
            // rather than clamping, so the sparsification the caller sees is honest.
            latticeCase: static (state, d) => {
                Point3d local = d.Value.Locate(sample: state.Point);
                bool inside = local.X >= 0.0 && local.X <= d.Value.Columns.Value
                    && local.Y >= 0.0 && local.Y <= d.Value.Rows.Value
                    && (d.Value.Rank is 2 || (local.Z >= 0.0 && local.Z <= d.Value.Layers.Value));
                (int column, int row, int layer) = d.Value.Nearest(sample: state.Point);
                return inside
                    ? state.Key.AcceptValue(value: d.Value.Center(column: column, row: row, layer: layer))
                    : Fin.Fail<Point3d>(state.Key.InvalidInput());
            }));

    private static Fin<SampleResult> SampleGeneratedSupport(SampleKind kind, SupportSpace space, Context context, Op key) =>
        kind.Facts.Count.ToFin(Fail: key.Unsupported(inputType: kind.GetType(), outputType: typeof(SampleResult))).Bind(count =>
            from evaluated in space.Payload.Evaluate(new EvaluationRequest.Sample(Count: (int)Math.Ceiling(a: count * Math.Max(1.0, kind.Facts.CandidateScale)), Model: context), key)
            from points in evaluated.Project<Seq<Point3d>>(key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: points.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: false, domainMeasure: Option<(SpatialRank Rank, double Measure)>.None, context: context, key: key)
            select sampled);
    private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        if (kind is SampleKind.PowerCcvtCase power) return PowerCcvtMeshSolve(domain: domain, kind: power, context: context, key: key);
        if (kind is SampleKind.DworkVariableDensityCase dwork)
            return from selection in DworkMeshRun.Execute(domain: domain, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts, seed: dwork.Seed, context: context, key: key)
                   let points = toSeq(selection.Points)
                   let receipt = selection.Algorithm.Bind(static algorithm => algorithm.Dwork)
                   let rejected = receipt.Map(static value => value.RejectedTooClose + value.RejectedDomain).IfNone(0)
                   let result = new SampleResult(Points: points, Mass: selection.Mass,
                       Receipt: ReceiptOf(attempted: receipt.Map(static value => value.GeneratedCandidates).IfNone(points.Count + rejected), emitted: points, rejected: rejected,
                           candidates: Option<int>.None, iterations: Option<int>.None,
                           stop: points.Count <= 0 ? SampleStopKind.AllRejected : points.Count < dwork.Count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                           status: rejected > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                           densityError: kind.DensityError(emitted: points.Count), key: key, algorithm: selection.Algorithm))
                   from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: result, key: key)
                   select validated;
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).ToFin(key.InvalidResult()).Bind(p =>
            from density in kind.MeshCandidateDensity(area: p.Area, key: key)
            from candidates in SurfaceCandidatePoints(space: domain, density: density, key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: true, domainMeasure: Some((Rank: SpatialRank.Planar, Measure: p.Area)), context: context, key: key)
            from validated in SegmentKernel.ValidateSamplingSpectrum(space: domain, result: sampled, key: key)
            select validated);
    }

    // OPEN barycentric lattice: every weight is inset by one lattice step from the triangle's own edges, so the
    // denominator is `side + 2·inset + 1` and no candidate lands on a shared edge two faces both emit. The row count
    // for a triangular lattice of n cells is ceil(sqrt(2n)), the inverse of the n(n+1)/2 triangular number.
    private const double OpenLatticeInset = 1.0;
    private const double TriangularLatticeFactor = 2.0;
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
            : Fin.Fail<Seq<Point3d>>(key.InvalidResult());
    }

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
    // Exponential-clock weighted reservoir; constant density falls to farthest coverage. The race keys on POSITION so
    // a candidate's clock is stable under any enumeration order, and on the declared Priority lane so it never
    // collides with the Dwork or jitter draws under one seed.
    private static Seq<Point3d> DensityImportanceSites(Seq<Point3d> candidates, int count, Option<ScalarField> density, Context context, int seed, Op key) =>
        density.Match(
            Some: field => toSeq(Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Weight: field.SampleScalar(sample: candidates[index: i], context: context, key: key).Match(Succ: value => value > 0.0 && double.IsFinite(value) ? value : 0.0, Fail: static _ => 0.0)))
                .Where(static row => row.Weight > 0.0)
                .OrderBy(row => -Math.Log(d: Deterministic.UnitInterval(point: candidates[index: row.Index], salt: SampleLane.Priority.Lane, seed: seed)) / row.Weight)
                .Take(count: count)
                .Select(row => candidates[index: row.Index])),
            None: () => toSeq(FarthestIndices(candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), count: count).Select(i => candidates[index: i])));
    // PowerCcvtReceipt owns spectrum evidence; the generic slot clears.
    private static SampleResult SurfaceSpectrumIntoReceipt(SampleResult result) =>
        result.Receipt.Algorithm.Bind(static algorithm => algorithm.PowerCcvt.Map(ccvt => (Algorithm: algorithm, Ccvt: ccvt))).Match(
            Some: pair => result with { Receipt = result.Receipt with { Algorithm = Some(pair.Algorithm with {
                Spectrum = Option<MeshSamplingSpectrumReceipt>.None,
                PowerCcvt = Some(pair.Ccvt with { Spectrum = pair.Algorithm.Spectrum }) }) } },
            None: () => result);

    private sealed class PowerCcvtRun(MeshSpace domain, Dimension count, PowerCcvtPolicy policy, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context, Op key) {
        private readonly int siteCount = sites.Count;
        private readonly double targetMass = totalMass / Math.Max(val1: 1, val2: sites.Count);
        private readonly double searchDistance = domain.Native.GetBoundingBox(accurate: true).Diagonal.Length;
        // Rebuilds are COUNTED where they happen, never asserted at the fold that presumed them: one counter on the
        // run, incremented by the one rebuild owner, read once at the terminal.
        private readonly Atom<int> rebuilds = Atom(value: 0);
        // Every convergence threshold is a lane read off the run's own context — the residual band, the Newton floor,
        // the sufficient-decrease constant, and the motion floor — so no policy column carries an epsilon literal.
        private double ResidualTol => context.For(lane: ToleranceLane.Residual).Value;
        private double NewtonFloor => context.For(lane: ToleranceLane.Step).Value;
        private double SufficientDecrease => context.For(lane: ToleranceLane.Kkt).Value;
        private double MotionFloor => context.For(lane: ToleranceLane.Convergence).Value;

        internal Fin<SampleResult> Run() =>
            siteCount < 1
                ? Fin.Fail<SampleResult>(key.InvalidResult())
                : ConvergeNewton(currentSites: sites, seed: RebuildDiagram(currentSites: sites, weights: new Arr<double>([.. Enumerable.Repeat(element: 0.0, count: siteCount)])))
                    .Bind(seed => ConvergeOuter(seed: OuterState.Of(sites: sites, capacity: seed)).Bind(Finalize));

        // A converged or faulted state is the committed settlement fact, so the columns are read as measured — never
        // re-derived after the loop from the state the step already decided.
        private Fin<OuterState> ConvergeOuter(OuterState seed) =>
            Cell.Converge(
                cell: Atom(value: seed),
                step: state => Some(OuterStep(state: state)),
                settled: static state => state.Converged || state.Fault.IsSome,
                budget: policy.Iterations, declined: key.InvalidResult()).Current
            switch { OuterState settled => settled.Fault.Match(Some: Fin.Fail<OuterState>, None: () => Fin.Succ(settled)) };
        private OuterState OuterStep(OuterState state) {
            SiteMotion motion = TwoPhaseSiteMotion(currentSites: state.Sites, capacity: state.Capacity);
            return Spacing.MeanNearest(points: motion.Sites, measure: totalMass, key: key).Bind(meanSpacing =>
                ConvergeNewton(currentSites: motion.Sites, seed: RebuildDiagram(currentSites: motion.Sites, weights: state.Capacity.Weights)).Map(advanced => state with {
                    Sites = motion.Sites, Capacity = advanced,
                    OuterIterations = state.OuterIterations + 1, LloydIterations = state.LloydIterations + motion.LloydIterations,
                    GradientIterations = state.GradientIterations + motion.GradientIterations,
                    StepHalvings = state.StepHalvings + motion.GradientHalvings,
                    PositionGradientNorm = motion.PositionGradientNorm,
                    TransportEnergyDelta = advanced.TransportEnergy - state.Capacity.TransportEnergy,
                    // The relative motion gate floors on the convergence lane, so a vanishing mean spacing cannot
                    // make an unconverged run read as settled.
                    Converged = motion.Displacement <= Math.Max(val1: policy.Motion.LloydPosTol.Value * meanSpacing, val2: MotionFloor)
                             && motion.PositionGradientNorm <= Math.Max(val1: policy.Motion.GradPosTol.Value * meanSpacing, val2: MotionFloor),
                }))
            .Match(Succ: static advanced => advanced, Fail: error => state with { Fault = Some(error) });
        }
        private SiteMotion TwoPhaseSiteMotion(Seq<Point3d> currentSites, NewtonState capacity) {
            (Seq<Point3d> lloydSites, int sweeps, RestrictedPowerDiagram lloydDiagram) = LloydPhase(currentSites: currentSites, diagram: capacity.Diagram, weights: capacity.Weights);
            (Seq<Point3d> gradientSites, int steps, int halvings, RestrictedPowerDiagram gradientDiagram) = GradientPhase(currentSites: lloydSites, diagram: lloydDiagram, weights: capacity.Weights);
            return new SiteMotion(Sites: gradientSites, LloydIterations: sweeps, GradientIterations: steps, GradientHalvings: halvings,
                Displacement: PairwiseShift(from: currentSites, to: gradientSites),
                PositionGradientNorm: Math.Sqrt(d: AscentSlope(direction: AscentDirection(sitesAt: gradientSites, diagram: gradientDiagram))));
        }
        // Rebuild failures freeze the last admissible partition rather than failing the rail.
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
        // Ascent on -E: a sufficient-decrease test stalls this concave maximization at step one.
        private (Seq<Point3d> Sites, int Steps, int Halvings, RestrictedPowerDiagram Diagram) GradientPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) =>
            toSeq(Enumerable.Range(start: 0, count: policy.Motion.GradientSteps.Value)).Fold(
                initialState: (Sites: currentSites, Steps: 0, Halvings: 0, Diagram: diagram, Live: true),
                f: (state, _) => {
                    if (!state.Live) return state;
                    Vector3d[] direction = AscentDirection(sitesAt: state.Sites, diagram: state.Diagram);
                    double slope = AscentSlope(direction: direction);
                    if (!(double.IsFinite(slope) && slope > 0.0)) return state with { Live = false };
                    (Seq<Point3d> sites, Option<RestrictedPowerDiagram> moved, int halvings) = AscentLineSearch(
                        currentSites: state.Sites, direction: direction, slope: slope,
                        baseEnergy: -TransportEnergyOf(diagram: state.Diagram), weights: weights, alpha: policy.Search.InitialStep.Value, halvings: 0);
                    return moved.Match(
                        Some: rebuilt => (Sites: sites, Steps: state.Steps + 1, Halvings: state.Halvings + halvings, Diagram: rebuilt, Live: true),
                        None: () => state with { Halvings = state.Halvings + halvings, Live = false });
                }) switch { var terminal => (terminal.Sites, terminal.Steps, terminal.Halvings, terminal.Diagram) };
        // Absent diagrams ARE non-improvement, so no bool rides beside a default no arm may read.
        private (Seq<Point3d> Sites, Option<RestrictedPowerDiagram> Diagram, int Halvings) AscentLineSearch(Seq<Point3d> currentSites, Vector3d[] direction, double slope, double baseEnergy, Arr<double> weights, double alpha, int halvings) {
            Seq<Point3d> trial = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => currentSites[index: i] + (alpha * direction[i])));
            return RebuildPowerCells(currentSites: trial, weights: weights).Match(
                Succ: diagram => -TransportEnergyOf(diagram: diagram) >= baseEnergy + (SufficientDecrease * alpha * slope)
                    ? (trial, Some(diagram), halvings)
                    : Backtrack(),
                Fail: _ => Backtrack());
            (Seq<Point3d>, Option<RestrictedPowerDiagram>, int) Backtrack() =>
                halvings >= policy.Search.MaxHalvings.Value
                    ? (currentSites, Option<RestrictedPowerDiagram>.None, halvings)
                    : AscentLineSearch(currentSites: currentSites, direction: direction, slope: slope, baseEnergy: baseEnergy, weights: weights, alpha: alpha * policy.Search.Backtrack.Value, halvings: halvings + 1);
        }
        private Vector3d[] AscentDirection(Seq<Point3d> sitesAt, RestrictedPowerDiagram diagram) =>
            [.. Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: diagram, site: i).Match(
                Some: cell => cell.Empty || !cell.Barycenter.IsValid ? Vector3d.Zero : 2.0 * Math.Max(val1: cell.Mass, val2: 0.0) * (cell.Barycenter - sitesAt[index: i]),
                None: () => Vector3d.Zero))];
        // ONE slope owner: `TensorPrimitives` publishes no `Vector3d` reduction, so the squared-length fold is named
        // once rather than re-spelled at the phase gate and the motion norm.
        private static double AscentSlope(Vector3d[] direction) => direction.Sum(static d => d.SquareLength);
        // Cell integrals are site-anchored at build time; a parallel-axis m_i |q_i - b_i|^2 term would double-count.
        private static double TransportEnergyOf(RestrictedPowerDiagram diagram) =>
            diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.TransportCost);
        private static Option<PowerCell> CellOf(RestrictedPowerDiagram diagram, int site) =>
            site >= 0 && site < diagram.Cells.Count ? Some(diagram.Cells[index: site]) : Option<PowerCell>.None;
        private Fin<RestrictedPowerDiagram> RebuildPowerCells(Seq<Point3d> currentSites, Arr<double> weights) =>
            MeshKernel.RestrictedPowerCells(space: domain, sites: currentSites, weights: Some(weights), density: policy.Density, key: key)
                .Map(diagram => { ignore(rebuilds.Swap(static held => held + 1)); return diagram; });
        private static double PairwiseShift(Seq<Point3d> from, Seq<Point3d> to) =>
            Enumerable.Range(start: 0, count: Math.Min(val1: from.Count, val2: to.Count)).Sum(i => from[index: i].DistanceTo(other: to[index: i]));

        // Budget exhaustion is a typed terminal, never a Fin.Fail; the step's own Converged column is the verdict, so
        // nothing after the loop recomputes it from the residual the step already tested.
        private Fin<NewtonState> ConvergeNewton(Seq<Point3d> currentSites, Fin<NewtonState> seed) =>
            seed.Map(seedState => Cell.Converge(
                    cell: Atom(value: seedState),
                    step: state => Some(NewtonStep(currentSites: currentSites, state: state)),
                    settled: static state => state.Converged || state.Fault.IsSome,
                    budget: policy.Capacity.MaxNewton, declined: key.InvalidResult()).Current)
                .Bind(terminal => terminal.Fault.Match(Some: Fin.Fail<NewtonState>, None: () => Fin.Succ(terminal)));
        private NewtonState NewtonStep(Seq<Point3d> currentSites, NewtonState state) {
            Arr<double> gradient = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => targetMass - state.Diagram.Cells[index: i].Mass)]);
            double gradNorm = TensorPrimitives.Norm<double>([.. gradient.AsIterable()]);
            return HessianTriplets(currentSites: currentSites, diagram: state.Diagram)
                .Bind(triplets => SparseMatrix.FromTriplets(rows: Dimension.Create(value: siteCount), cols: Dimension.Create(value: siteCount), triplets: triplets, key: key))
                .Bind(laplacian => laplacian.SingularSolveDetailed(rhs: gradient, gauge: policy.Gauge.Policy(fragmentMasses: FragmentMasses(diagram: state.Diagram)), context: context, key: key))
                // The receipt is READ through the ONE gate: a boolean test beside it discards WHICH claim failed.
                .Bind(solve => GeodesicKernel.Solved(solve: Fin.Succ(solve), key: key)
                    .Bind(direction => AscentSearch(currentSites: currentSites, state: state, direction: direction,
                        slope: TensorPrimitives.Dot<double>([.. gradient.AsIterable()], [.. direction.AsIterable()]),
                        baseObjective: state.DualObjective, alpha: policy.Search.InitialStep.Value, halvings: 0).Map(advanced => (Solve: solve, Advanced: advanced))))
                .Match(
                    Succ: step => step.Advanced with {
                        DualSolve = Some(step.Solve), WeightGradientNorm = gradNorm,
                        NewtonIterations = state.NewtonIterations + 1,
                        Converged = step.Advanced.Residual.Inf <= ResidualTol * targetMass,
                    },
                    Fail: error => state with { Fault = Some(error), DualSolve = state.DualSolve });
        }
        private Fin<NewtonState> AscentSearch(Seq<Point3d> currentSites, NewtonState state, Arr<double> direction, double slope, double baseObjective, double alpha, int halvings) {
            Arr<double> advanced = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => state.Weights[index: i] + (alpha * direction[index: i]))]);
            return RebuildDiagram(currentSites: currentSites, weights: advanced).Bind(rebuilt =>
                rebuilt.DualObjective >= baseObjective + (SufficientDecrease * alpha * slope) || halvings >= policy.Search.MaxHalvings.Value
                    ? Fin.Succ(rebuilt with { StepHalvings = state.StepHalvings + halvings, NewtonIterations = state.NewtonIterations })
                    : AscentSearch(currentSites: currentSites, state: state, direction: direction, slope: slope, baseObjective: baseObjective, alpha: alpha * policy.Search.Backtrack.Value, halvings: halvings + 1));
        }
        // Each facet owns the SIGNED dual measure — a negative Length flags a site about to become hidden and is
        // CARRIED into the Hessian, never dropped, or the weight-Newton step silently loses the hiding signal;
        // l_ij reads the live site pair the diagram was just built from, so facet and distance share one epoch.
        // Diagonal entries emit INTO the stream: `FromTriplets` sums duplicates, so a pre-summing array and its
        // second pass are the accumulation the owner already performs.
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
            return triplets.Exists(static row => row.Value != 0.0) ? Fin.Succ(triplets) : Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidResult());
        }
        private Arr<double> FragmentMasses(RestrictedPowerDiagram diagram) =>
            new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => Math.Max(val1: diagram.Cells[index: i].Mass, val2: 0.0))]);
        private Fin<NewtonState> RebuildDiagram(Seq<Point3d> currentSites, Arr<double> weights) =>
            RebuildPowerCells(currentSites: currentSites, weights: weights).Map(diagram => {
                double transport = TransportEnergyOf(diagram: diagram);
                double dual = transport + Enumerable.Range(start: 0, count: siteCount).Sum(i => weights[index: i] * (targetMass - diagram.Cells[index: i].Mass));
                // One scratch deviation plane, three vectorized reductions — the hand Aggregate over a mutable seed
                // re-spelled max, sum, and sum-of-squares the reduction family already owns.
                double[] deviation = [.. Enumerable.Range(start: 0, count: siteCount).Select(i => diagram.Cells[index: i].Mass - targetMass)];
                TensorPrimitives.Abs<double>(deviation, deviation);
                double inf = TensorPrimitives.Max<double>(deviation);
                CapacityResidual residual = new(Inf: inf, L1: TensorPrimitives.Sum<double>(deviation), L2: TensorPrimitives.Norm<double>(deviation),
                    Normalized: inf / Math.Max(val1: targetMass, val2: EpsilonPolicy.ZeroTolerance));
                return new NewtonState(Weights: weights, Diagram: diagram, Residual: residual, DualObjective: dual, TransportEnergy: transport,
                    Converged: false, NewtonIterations: 0, StepHalvings: 0, Fault: Option<Error>.None,
                    DualSolve: Option<SolveReceipt>.None, WeightGradientNorm: 0.0);
            });
        private Fin<SampleResult> Finalize(OuterState outer) {
            NewtonState terminal = outer.Capacity;
            RestrictedPowerReceipt diagramReceipt = terminal.Diagram.Receipt;
            return from meanSpacing in Spacing.MeanNearest(points: outer.Sites, measure: totalMass, key: key)
                   let broken = BreakRegularity(currentSites: outer.Sites, meanSpacing: meanSpacing)
                   // A site the surface refuses to accept is UNLIFTED, censused, and dropped — publishing the raw site
                   // as if it had landed is the silent capability loss the funnel closes.
                   let lifted = broken.Sites.Choose(site => Optional(domain.Native.ClosestMeshPoint(testPoint: site, maximumDistance: searchDistance))
                       .Filter(static hit => hit.Point.IsValid).Map(static hit => hit.Point))
                   from mass in Stat<Scalar>.Of(values: terminal.Diagram.Cells.AsIterable().Map(static cell => (Scalar)cell.Mass).ToSeq(), key: key)
                   from weights in Stat<Scalar>.Of(values: terminal.Weights.AsIterable().Map(static weight => (Scalar)weight).ToSeq(), key: key)
                   from poissonRadius in Spacing.NormalizedPoissonRadius(points: lifted, measure: totalMass, key: key)
                   let fragments = new PowerCellFragmentFacts(
                       SiteCount: siteCount, FragmentCount: diagramReceipt.FragmentCount, FacetCount: diagramReceipt.NeighborFacetCount,
                       EmptyCellCount: diagramReceipt.EmptyCellCount, Mass: mass, IntegrationResidual: diagramReceipt.IntegrationResidual)
                   let receipt = new PowerCcvtReceipt(
                       SiteCount: siteCount, TargetMass: targetMass,
                       CapacityResidualInf: terminal.Residual.Inf, CapacityResidualL1: terminal.Residual.L1, CapacityResidualL2: terminal.Residual.L2, CapacityResidualNormalized: terminal.Residual.Normalized,
                       OuterIterations: outer.OuterIterations, LloydIterations: outer.LloydIterations, GradientIterations: outer.GradientIterations, DualNewtonIterations: terminal.NewtonIterations,
                       Weights: weights, TransportEnergy: terminal.TransportEnergy, TransportEnergyDelta: outer.TransportEnergyDelta,
                       DualObjective: terminal.DualObjective, CentroidShift: PairwiseShift(from: lifted, to: sites), PositionGradientNorm: outer.PositionGradientNorm, WeightGradientNorm: terminal.WeightGradientNorm,
                       EmptyCellCount: diagramReceipt.EmptyCellCount, StepHalvingCount: outer.StepHalvings, RebuildCount: rebuilds.Value,
                       AliasedSiteCount: broken.AliasedCount, RelocatedSiteCount: broken.RelocatedCount, UnliftedSiteCount: broken.Sites.Count - lifted.Count,
                       NormalizedPoissonRadius: poissonRadius, PlanarityDeviation: planarityDeviation,
                       Gauge: policy.Gauge, Stop: outer.Converged ? PowerCcvtStopKind.Converged : PowerCcvtStopKind.StoppedWithoutConvergence,
                       Fragments: fragments, DualSolve: terminal.DualSolve, Spectrum: Option<MeshSamplingSpectrumReceipt>.None)
                   from admitted in receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<PowerCcvtReceipt>(key.InvalidResult())
                   select new SampleResult(Points: lifted, Mass: Option<Arr<double>>.None,
                       Receipt: ReceiptOf(attempted: siteCount, emitted: lifted, rejected: diagramReceipt.EmptyCellCount, candidates: Some(siteCount),
                           iterations: Some(outer.OuterIterations),
                           stop: lifted.IsEmpty ? SampleStopKind.AllRejected : lifted.Count < count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                           status: diagramReceipt.EmptyCellCount > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                           densityError: Option<double>.None, key: key,
                           algorithm: Some(new SampleAlgorithmReceipt(
                               Kind: SampleAlgorithmKind.ContinuousPowerCcvt,
                               // Set algebra over the roster: the local shell rebuilding a set by reassignment is the
                               // parallel-bool form the capability owner exists to collapse.
                               Assurances: CapabilitySet<SampleAssurance>.Of([
                                   .. terminal.Converged ? (SampleAssurance[])[SampleAssurance.CapacityResidual] : [],
                                   .. !lifted.IsEmpty && terminal.DualSolve.Map(static solve => solve.IsValid).IfNone(noneValue: false)
                                       ? (SampleAssurance[])[SampleAssurance.TransportAssignment] : []]),
                               Seed: Some(policy.Seed), TargetCount: Some(count.Value),
                               CapacityResidual: Some(terminal.Residual.Inf),
                               PowerCcvt: Some(admitted))));
        }
        private Regularity BreakRegularity(Seq<Point3d> currentSites, double meanSpacing) {
            if (currentSites.Count < 2 || meanSpacing <= EpsilonPolicy.ZeroTolerance) return new Regularity(Sites: currentSites, AliasedCount: 0, RelocatedCount: 0);
            double aliasRadius = policy.Regularity.AliasScale.Value * meanSpacing;
            double jitterMagnitude = policy.Regularity.JitterVariance.Value * policy.Regularity.MagnitudeScale.Value * meanSpacing;
            int total = currentSites.Count;
            // The alias mask is a RADIUS QUERY over the one neighbourhood owner, not an O(n²) prefix scan per site.
            bool[] aliased = AliasMask(currentSites: currentSites, radius: aliasRadius);
            int aliasedCount = aliased.Count(static flag => flag);
            int relocateBudget = Math.Min(val1: aliasedCount, val2: (int)Math.Floor(d: policy.Regularity.RelocateFraction.Value * aliasedCount));
            int relocated = 0;
            Point3d[] moved = new Point3d[total];
            Deterministic.Draw jitter = Deterministic.Of(policy.Seed, SampleLane.Jitter);
            for (int i = 0; i < total; i++) {
                bool relocate = aliased[i] && relocated < relocateBudget;
                moved[i] = aliased[i]
                    ? currentSites[index: i] + JitterOffset(draw: jitter.At(i), magnitude: relocate ? jitterMagnitude + meanSpacing : jitterMagnitude)
                    : currentSites[index: i];
                if (relocate) relocated++;
            }
            // Every aliased site jitters by construction, so a second column counting them is provably the first.
            return new Regularity(Sites: toSeq(moved), AliasedCount: aliasedCount, RelocatedCount: relocated);
        }
        private bool[] AliasMask(Seq<Point3d> currentSites, double radius) =>
            (from index in NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: currentSites), key: key)
             from graph in NeighborKernel.GraphOf(index: index, needles: [.. currentSites.AsIterable()], count: Option<int>.None, radius: Some(radius), key: key)
             select graph.Ids)
            .Match(
                Succ: ids => [.. Enumerable.Range(start: 0, count: currentSites.Count).Select(i => ids.Length > i && ids[i].Any(id => id >= 0 && id < i))],
                Fail: _ => new bool[currentSites.Count]);
        // Box-Muller off two coordinate lanes of the site's own bound draw — no hand-packed salt.
        private static Vector3d JitterOffset(Deterministic.Draw draw, double magnitude) {
            double u1 = draw.At(0L).Unit;
            double u2 = draw.At(1L).Unit;
            double radius = magnitude * Math.Sqrt(d: Math.Max(val1: 0.0, val2: -2.0 * Math.Log(d: u1)));
            double angle = 2.0 * Math.PI * u2;
            return new Vector3d(x: radius * Math.Cos(d: angle), y: radius * Math.Sin(a: angle), z: 0.0);
        }
        [StructLayout(LayoutKind.Auto)] private readonly record struct CapacityResidual(double Inf, double L1, double L2, double Normalized);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct NewtonState(
            Arr<double> Weights, RestrictedPowerDiagram Diagram, CapacityResidual Residual, double DualObjective, double TransportEnergy,
            bool Converged, int NewtonIterations, int StepHalvings, Option<Error> Fault, Option<SolveReceipt> DualSolve, double WeightGradientNorm);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct OuterState(
            Seq<Point3d> Sites, NewtonState Capacity, int OuterIterations, int LloydIterations, int GradientIterations,
            int StepHalvings, double PositionGradientNorm, double TransportEnergyDelta,
            bool Converged, Option<Error> Fault) {
            internal static OuterState Of(Seq<Point3d> sites, NewtonState capacity) => new(
                Sites: sites, Capacity: capacity, OuterIterations: 0, LloydIterations: 0, GradientIterations: 0,
                StepHalvings: capacity.StepHalvings, PositionGradientNorm: 0.0,
                TransportEnergyDelta: 0.0, Converged: false, Fault: Option<Error>.None);
        }
        [StructLayout(LayoutKind.Auto)] private readonly record struct SiteMotion(Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm);
        [StructLayout(LayoutKind.Auto)] private readonly record struct Regularity(Seq<Point3d> Sites, int AliasedCount, int RelocatedCount);
    }

    private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<SampleCandidate> candidates, bool admitsPoisson, Option<(SpatialRank Rank, double Measure)> domainMeasure, Context context, Op key) =>
        from selection in kind switch {
            SampleKind.PoissonDiskCase pd when admitsPoisson => PoissonDiskSelection(candidates: candidates, radius: pd.Radius, attempts: pd.Attempts, seed: pd.Seed, key: key),
            SampleKind.FarthestCase fp => SelectionOf(kind: kind, candidates: candidates, indices: FarthestIndices(candidates: candidates, count: fp.Count.Value), key: key),
            SampleKind.OptimizeCase fpo => SelectionOf(kind: kind, candidates: candidates, indices: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value, key: key), key: key),
            SampleKind.LloydCase lloyd => RelaxationSample(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value, capacity: Option<int>.None, key: key)
                .Bind(relaxed => SelectionOf(kind: kind, candidates: candidates, indices: relaxed.Indices, key: key)),
            SampleKind.CapacityCase ccvt => CapacityCvtSelection(candidates: candidates, count: ccvt.Count.Value, limit: ccvt.Limit.Value, iterations: ccvt.Iterations.Value, tolerance: context.For(lane: ToleranceLane.Convergence).Value, key: key),
            SampleKind.ScalarDensityCase density => DensitySelection(candidates: candidates, density: density.Density, count: density.Count.Value,
                minSpacing: 0.5 * (BoundingMeasure(candidates: candidates) switch { var box => box.Rank.MeanSpacing(measure: box.Measure, count: density.Count.Value) }),
                context: context, seed: density.Seed, key: key),
            SampleKind.AdaptiveCase adaptive => DensitySelection(candidates: candidates, density: adaptive.Density, count: adaptive.Count.Value, minSpacing: adaptive.MinSpacing.Value, context: context, seed: adaptive.Seed, key: key),
            SampleKind.SampleEliminationCase elimination => SampleElimination(candidates: candidates, count: elimination.Count.Value, alpha: elimination.Alpha.Value, beta: elimination.Beta.Value, gamma: elimination.Gamma.Value, seed: elimination.Seed, domainMeasure: domainMeasure, key: key)
                .Bind(result => SelectionOf(candidates: candidates, indices: result.Indices, algorithm: Some(result.Algorithm), key: key)),
            SampleKind.DworkVariableDensityCase dwork => DworkCandidateSelection(candidates: candidates, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts, seed: dwork.Seed, context: context, key: key),
            SampleKind.PoissonDiskCase pd => Fin.Fail<SampleSelection>(error: key.Unsupported(inputType: pd.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<SampleSelection>(error: key.Unsupported(inputType: kind.GetType(), outputType: typeof(SampleResult))),
        }
        let sampled = toSeq(selection.Points)
        let rejected = selection.DensityRejected.IfNone(Math.Max(val1: 0, val2: candidates.Count - selection.Points.Length))
        let capacityLimited = selection.Algorithm.Map(static receipt => receipt.Kind.Equals(SampleAlgorithmKind.CapacityLimitedLloydCandidate) && !receipt.Assurances.Admits(SampleAssurance.CapacityResidual)).IfNone(noneValue: false)
        select new SampleResult(
            Points: sampled, Mass: selection.Mass,
            Receipt: ReceiptOf(attempted: candidates.Count, emitted: sampled, rejected: rejected, candidates: Some(candidates.Count), iterations: kind.Facts.Iterations,
                stop: sampled.Count <= 0 ? SampleStopKind.AllRejected : capacityLimited ? SampleStopKind.CapacityLimited : kind.Facts.Count.Map(requested => sampled.Count < requested ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed).IfNone(SampleStopKind.Completed),
                status: selection.DensityRejected.Map(static count => count > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted).IfNone(SampleDomainStatus.CandidateAccepted),
                densityError: kind.DensityError(emitted: sampled.Count), key: key, densityAccepted: selection.DensityAccepted, densityRejected: selection.DensityRejected, algorithm: selection.Algorithm));

    private static Fin<SampleSelection> SelectionOf(SampleKind kind, Seq<SampleCandidate> candidates, int[] indices, Op key, Option<double> radius = default) =>
        SelectionOf(candidates: candidates, indices: indices, algorithm: Some(new SampleAlgorithmReceipt(Kind: kind.Facts.Algorithm, Assurances: CapabilitySet<SampleAssurance>.None, TargetCount: kind.Facts.Count, Radius: radius)), key: key);
    private static Fin<SampleSelection> SelectionOf(Seq<SampleCandidate> candidates, int[] indices, Option<SampleAlgorithmReceipt> algorithm, Op key) {
        Point3d[] points = [.. indices.Select(i => candidates[index: i].Point)];
        Seq<double> mass = toSeq(indices).Choose(i => candidates[index: i].Mass);
        return (indices.Length, mass.Count) switch {
            (0, _) or (_, 0) => Fin.Succ(new SampleSelection(Points: points, Mass: Option<Arr<double>>.None, DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            (int count, int weights) when count == weights => NormalizeMass(mass: mass, key: key).Map(normalized => new SampleSelection(Points: points, Mass: Some(normalized), DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: algorithm)),
            _ => Fin.Fail<SampleSelection>(key.InvalidResult()),
        };
    }
    private static SampleReceipt ReceiptOf(int attempted, Seq<Point3d> emitted, int rejected, Option<int> candidates, Option<int> iterations, SampleStopKind stop, SampleDomainStatus status, Option<double> densityError, Op key, Option<int> densityAccepted = default, Option<int> densityRejected = default, Option<SampleAlgorithmReceipt> algorithm = default) =>
        new(Attempted: attempted, Emitted: emitted.Count, Rejected: rejected, CandidateCount: candidates,
            Spacing: Spacing.DistributionOf(points: emitted, key: key).ToOption(),
            DensityError: densityError, DensityAccepted: densityAccepted, DensityRejected: densityRejected, Iterations: iterations,
            Stop: stop, DomainStatus: status, Algorithm: algorithm);
    private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, Op key) =>
        CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: mass.Count, key: key);
    private static Fin<SampleSelection> DensitySelection(Seq<SampleCandidate> candidates, ScalarField density, int count, double minSpacing, Context context, int seed, Op key) {
        double[] weights = new double[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, Band: Option<(double Min, double Max)>.None)),
            f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? key.AcceptValue(value: value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => { weights[i] = valid; return (current.Accepted + 1, current.Rejected, Some(Widen(current.Band, valid))); })
                    : Fin.Succ((current.Accepted, current.Rejected + 1, current.Band)))))
            .Bind(stats => stats.Band.Match(
                Some: band => PrioritySelection(candidates: candidates, weights: weights, count: count, minSpacing: minSpacing, minWeight: band.Min, maxWeight: band.Max, accepted: stats.Accepted, rejected: stats.Rejected, seed: seed, key: key),
                None: () => Fin.Fail<SampleSelection>(key.InvalidResult())));
    }
    // One band widener for every Option-seeded min/max fold on this page: the first sample IS the seed, so no forged
    // infinity can reach a receipt column.
    private static (double Min, double Max) Widen(Option<(double Min, double Max)> band, double value) =>
        band.Map(held => (Math.Min(val1: held.Min, val2: value), Math.Max(val1: held.Max, val2: value))).IfNone((value, value));
    // Bridson background cell: r/sqrt(d) admits at most one sample per cell, d the ambient rank the cell hash carries.
    // ONE owner, so no site divides by a bare sqrt(3) and none of them loses the zero floor the divisor needs.
    private static double BackgroundCellSize(double radius) =>
        Math.Max(val1: radius / Math.Sqrt(d: SpatialRank.Volumetric.Key), val2: EpsilonPolicy.ZeroTolerance);
    // Run seed threads into the exponential-race key on the declared Priority lane exactly as the density-weighted
    // sibling threads it: a hardcoded zero replays ONE distribution for every seed, which the page's determinism law forbids.
    private static Fin<SampleSelection> PrioritySelection(Seq<SampleCandidate> candidates, double[] weights, int count, double minSpacing, double minWeight, double maxWeight, int accepted, int rejected, int seed, Op key) {
        // The ordered drain is a FOLD over the race order: three mutable accumulators and a raw enumerator beside it
        // were one state the fold already carries, and a full pool is the halt the state itself answers.
        (Seq<(Point3d Point, double Radius)> Chosen, Seq<double> Mass, Option<(double Min, double Max)> Band) drained =
            toSeq(Enumerable.Range(start: 0, count: candidates.Count)
                .Where(i => weights[i] > 0.0)
                .OrderBy(i => -Math.Log(d: Deterministic.UnitInterval(point: candidates[index: i].Point, salt: SampleLane.Priority.Lane, seed: seed)) / weights[i]))
            .Fold(
                initialState: (Chosen: Seq<(Point3d Point, double Radius)>.Empty, Mass: Seq<double>.Empty, Band: Option<(double Min, double Max)>.None),
                f: (held, index) => {
                    if (held.Chosen.Count >= count) return held;
                    Point3d candidate = candidates[index: index].Point;
                    double local = minSpacing / Math.Sqrt(d: Math.Max(val1: weights[index] / Math.Max(val1: maxWeight, val2: EpsilonPolicy.ZeroTolerance), val2: EpsilonPolicy.ZeroTolerance));
                    (Seq<(Point3d Point, double Radius)> chosen, Seq<double> mass) = held.Chosen.ForAll(existing => candidate.DistanceTo(other: existing.Point) >= Math.Max(val1: existing.Radius, val2: local))
                        ? (held.Chosen.Add((candidate, local)), held.Mass.Add(weights[index]))
                        : (held.Chosen, held.Mass);
                    return (chosen, mass, Some(Widen(held.Band, local)));
                });
        return NormalizeMass(mass: drained.Mass, key: key).Map(normalized => new SampleSelection(
            Points: [.. drained.Chosen.Map(static sample => sample.Point)], Mass: Some(normalized), DensityAccepted: Some(accepted), DensityRejected: Some(rejected),
            Algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.VariableDensityPoisson, Assurances: CapabilitySet<SampleAssurance>.None, TargetCount: Some(count),
                DensityMin: Some(minWeight), DensityMax: Some(maxWeight),
                LocalRadiusMin: drained.Band.Map(static band => band.Min), LocalRadiusMax: drained.Band.Map(static band => band.Max)))));
    }

    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCell(long X, long Y, long Z);
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCandidate(int Index, double Radius);
    private static Fin<SampleSelection> DworkCandidateSelection(Seq<SampleCandidate> candidates, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context, Op key) {
        DworkCandidate[] admitted = new DworkCandidate[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, Band: Option<(double Min, double Max)>.None)),
            f: (state, i) => state.Bind(current => radius.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => value > 0.0 && double.IsFinite(value)
                    ? key.AcceptValue(value: Math.Max(val1: minRadius, val2: value)).Map(local => {
                        admitted[current.Accepted] = new DworkCandidate(Index: i, Radius: local);
                        return (current.Accepted + 1, current.Rejected, Some(Widen(current.Band, local)));
                    })
                    : Fin.Succ((current.Accepted, current.Rejected + 1, current.Band)))))
            .Bind(stats => stats.Band.Match(None: () => Fin.Fail<SampleSelection>(key.InvalidResult()), Some: band => {
                DworkCandidate[] ordered = [.. admitted.Take(count: stats.Accepted).OrderBy(item => Deterministic.OrderKey(point: candidates[index: item.Index].Point, seed: seed))];
                double cellSize = BackgroundCellSize(radius: band.Min);
                Point3d gridOrigin = ordered.Length > 0 ? new BoundingBox(points: ordered.Select(item => candidates[index: item.Index].Point)).Min : Point3d.Origin;
                DworkCell CellOf(Point3d point) => new(X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize), Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize), Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
                Point3d[] pool = [.. ordered.Select(item => candidates[index: item.Index].Point)];
                // The candidate cloud is FROZEN once the race order settles, so its neighbourhood is the one index
                // owner's — only the chosen set grows per admission, and that one keeps the background hash the
                // section exemption names.
                return NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: toSeq(pool)), key: key).Bind(poolIndex => {
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
                        int shells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: band.Max) / cellSize));
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
                    Deterministic.Draw activeDraw = Deterministic.Of(seed, SampleLane.Active);
                    Deterministic.Draw annulusDraw = Deterministic.Of(seed, SampleLane.Annulus);
                    List<DworkCandidate> active = ordered.Length > 0 ? [ordered[0]] : [];
                    (int activePops, int tooClose, int outside) = (0, 0, 0);
                    while (active.Count > 0 && chosen.Count < count) {
                        int activeOffset = (int)(activeDraw.At(activePops).State % (ulong)active.Count);
                        DworkCandidate parent = active[activeOffset];
                        Point3d parentPoint = candidates[index: parent.Index].Point;
                        // Annulus band = one RADIUS QUERY over the frozen pool, then the inner-radius screen.
                        Fin<NeighborhoodGraph> reach = NeighborKernel.GraphOf(index: poolIndex, needles: [parentPoint], count: Option<int>.None, radius: Some(2.0 * parent.Radius), key: key);
                        List<DworkCandidate> annulus = reach.Match(
                            Succ: graph => graph.Ids.Length > 0
                                ? [.. graph.Ids[0].Where(o => o >= 0 && o < ordered.Length && parentPoint.DistanceTo(other: pool[o]) >= parent.Radius).Select(o => ordered[o])]
                                : [],
                            Fail: static _ => []);
                        if (annulus.Count == 0) { outside++; active.RemoveAt(index: activeOffset); activePops++; continue; }
                        int pops = activePops;
                        TrialState<DworkCandidate, int> trial = Trial<DworkCandidate, int>(
                            budget: attempts, seed: 0, key: key,
                            propose: (attempt, tally) => {
                                DworkCandidate candidate = annulus[(int)(annulusDraw.At(pops, attempt).State % (ulong)annulus.Count)];
                                return chosen.Exists(item => item.Index == candidate.Index) || Conflicts(candidate: candidate)
                                    ? (Option<DworkCandidate>.None, tally + 1)
                                    : (Some(candidate), tally);
                            });
                        tooClose += trial.Tally;
                        trial.Drawn.Match(
                            Some: candidate => { chosen.Add(item: candidate); Record(candidate: candidate); active.Add(item: candidate); },
                            None: () => active.RemoveAt(index: activeOffset));
                        activePops++;
                    }
                    DworkReceipt dwork = new(Domain: DworkSamplingDomain.CandidateSet, RMin: band.Min, BackgroundCellSize: Some(cellSize), BackgroundGridCells: Some(chosenGrid.Count),
                        AttemptsPerActive: attempts.Value, GeneratedCandidates: chosen.Count + tooClose + outside + stats.Rejected, ActivePops: activePops,
                        RejectedTooClose: tooClose, RejectedDomain: stats.Rejected + outside, LocalRadiusMin: band.Min, LocalRadiusMax: band.Max);
                    return SelectionOf(candidates: candidates, indices: [.. chosen.Select(static item => item.Index)],
                        algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.DworkVariableDensity, Assurances: CapabilitySet<SampleAssurance>.None,
                            Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(ordered.Length),
                            Attempts: Some(attempts.Value), ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(stats.Rejected + outside),
                            LocalRadiusMin: Some(band.Min), LocalRadiusMax: Some(band.Max), Dwork: Some(dwork))), key: key);
                });
            }));
    }

    private sealed class DworkMeshRun(Mesh mesh, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context, Op key) {
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

        internal static Fin<SampleSelection> Execute(MeshSpace domain, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context, Op key) {
            using Mesh mesh = domain.Native.DuplicateMesh();
            if (mesh.Faces.QuadCount > 0 && !mesh.Faces.ConvertQuadsToTriangles()) return Fin.Fail<SampleSelection>(key.InvalidResult());
            _ = mesh.FaceNormals.ComputeFaceNormals();
            return new DworkMeshRun(mesh: mesh, radius: radius, count: count, minRadius: minRadius, attempts: attempts, seed: seed, context: context, key: key).Run();
        }
        private Fin<SampleSelection> Run() =>
            BuildTriangles().Bind(_ => {
                TrialState<DworkSurfacePoint, int> seedTrial = Trial<DworkSurfacePoint, int>(
                    budget: attempts, seed: 0, key: key,
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
                                budget: attempts, seed: (0, 0), key: key,
                                propose: (attempt, band) => AnnulusCandidate(parent: parent, pops: pops, attempt: attempt).Match(
                                    Some: value => Conflicts(candidate: value) ? (Option<DworkSurfacePoint>.None, (band.TooClose + 1, band.Domain)) : (Some(value), band),
                                    None: () => (Option<DworkSurfacePoint>.None, (band.TooClose, band.Domain + 1))));
                            tally = (tally.Proposals + trial.Attempts, tally.ActivePops + 1, tally.TooClose + trial.Tally.TooClose, tally.Domain + trial.Tally.Domain);
                            trial.Drawn.Match(Some: Add, None: () => active.RemoveAt(index: activeOffset));
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
                if (!double.IsFinite(area) || area <= EpsilonPolicy.ZeroTolerance) continue;
                cumulative += area;
                built.Add(item: new DworkTriangle(A: a, B: b, C: c, CumulativeArea: cumulative));
            }
            BoundingBox bounds = mesh.GetBoundingBox(accurate: true);
            (triangles, cumulativeArea, totalArea, gridOrigin) = ([.. built], [.. built.Select(static item => item.CumulativeArea)], cumulative, bounds.IsValid ? bounds.Min : Point3d.Origin);
            return triangles.Length > 0 && double.IsFinite(totalArea) && totalArea > EpsilonPolicy.ZeroTolerance && bounds.IsValid
                ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult());
        }
        // Area-weighted triangle pick then a barycentric pair, each off its own declared lane addressed by attempt —
        // no hand-packed salt and no triangle-keyed draw whose ordering a re-triangulation would move.
        private Option<Point3d> SurfaceSample(int attempt) {
            double target = areaDraw.At(attempt).Unit * totalArea;
            // CumulativeArea is a sorted prefix array, so the inverse CDF is a binary search — the linear scan it
            // replaced re-walked every triangle per proposal, and the proposal count is attempts x active pops.
            int hit = System.Array.BinarySearch(array: cumulativeArea, value: target);
            DworkTriangle triangle = triangles[hit < 0 ? Math.Min(val1: ~hit, val2: triangles.Length - 1) : hit];
            double u = Math.Sqrt(d: barycentricDraw.At(attempt, 0L).Unit);
            double v = barycentricDraw.At(attempt, 1L).Unit;
            (double wa, double wb, double wc) = (1.0 - u, u * (1.0 - v), u * v);
            Point3d sample = new(x: (wa * triangle.A.X) + (wb * triangle.B.X) + (wc * triangle.C.X), y: (wa * triangle.A.Y) + (wb * triangle.B.Y) + (wc * triangle.C.Y), z: (wa * triangle.A.Z) + (wb * triangle.B.Z) + (wc * triangle.C.Z));
            return sample.IsValid ? Some(sample) : Option<Point3d>.None;
        }
        private Option<DworkSurfacePoint> RadiusAt(Point3d point) =>
            radius.SampleScalar(sample: point, context: context, key: key).Match(
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
        // ONE host-boundary funnel for the closest-point probe: a raw null test beside an Optional funnel is two
        // absence regimes on one page, and the silent fall-through the raw form invites publishes an unlifted point.
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
        private Fin<SampleSelection> Selection() {
            (double radiusMin, double radiusMax) = radiusBand.IfNone((minRadius, minRadius));
            DworkReceipt dwork = new(Domain: DworkSamplingDomain.ContinuousMesh, RMin: minRadius, BackgroundCellSize: Some(cellSize), BackgroundGridCells: Some(grid.Count),
                AttemptsPerActive: attempts.Value, GeneratedCandidates: tally.Proposals, ActivePops: tally.ActivePops,
                RejectedTooClose: tally.TooClose, RejectedDomain: tally.Domain, LocalRadiusMin: radiusMin, LocalRadiusMax: radiusMax);
            return Fin.Succ(new SampleSelection(Points: [.. chosen.Select(static sample => sample.Point)], Mass: Option<Arr<double>>.None,
                DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None,
                Algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.DworkVariableDensity, Assurances: CapabilitySet<SampleAssurance>.None,
                    Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(tally.Proposals),
                    Attempts: Some(attempts.Value), ActivePops: Some(tally.ActivePops), RejectedTooClose: Some(tally.TooClose), RejectedDomain: Some(tally.Domain),
                    LocalRadiusMin: Some(radiusMin), LocalRadiusMax: Some(radiusMax), Dwork: Some(dwork)))));
        }
    }

    // Radius and attempts keep their admitting value-object evidence; only the derived squared band takes a local gate.
    private static Fin<SampleSelection> PoissonDiskSelection(Seq<SampleCandidate> candidates, PositiveMagnitude radius, Dimension attempts, int seed, Op key) {
        (double r2, double r4) = (radius.Value * radius.Value, 4.0 * radius.Value * radius.Value);
        if (candidates.IsEmpty || !double.IsFinite(r4)) return Fin.Fail<SampleSelection>(key.InvalidInput());
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
                budget: attempts, seed: (0, 0), key: key,
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
        // Drained active lists ARE Bridson's maximality proof: no admissible annulus point remains anywhere.
        return SelectionOf(candidates: candidates, indices: [.. chosen.Distinct()],
            algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.BridsonActiveListPoisson,
                Assurances: active.Count == 0 ? CapabilitySet<SampleAssurance>.Of(SampleAssurance.MaximalCoverage) : CapabilitySet<SampleAssurance>.None,
                Seed: Some(seed), Radius: Some(radius.Value), Attempts: Some(attempts.Value),
                ActivePops: Some(activePops), RejectedTooClose: Some(tooClose), RejectedDomain: Some(outside))), key: key);
    }

    [StructLayout(LayoutKind.Auto)] private readonly record struct CapacityAssignment(int[] Hits, int Assigned, int Unassigned, double Residual);
    private const int UnassignedSite = -1;
    // ONE capacity-assignment body both the residual census and the relaxation step compose, so the two cannot drift.
    // REFUSED OPERATOR: a capacity-bounded transport is a max-flow problem, but `EdmondsKarpMaximumFlowAlgorithm`
    // publishes the flow VALUE and a residual map, while this kernel needs the per-candidate site ASSIGNMENT —
    // recovered by walking saturated arcs under `Edge<TVertex>` reference identity, a read the catalog documents for
    // the minimum CUT alone. The greedy stands until that recovery re-proves on the installed assembly.
    private static Option<CapacityAssignment> AssignUnderCapacity(Seq<SampleCandidate> candidates, int[] sites, int limit) {
        if (candidates.IsEmpty || sites.Length == 0 || limit < 1) return None;
        int[] hits = new int[candidates.Count];
        int[] fill = new int[sites.Length];
        (int assigned, int rejected) = (0, 0);
        for (int i = 0; i < candidates.Count; i++) {
            Option<(int Site, double Distance)> nearest = Enumerable.Range(start: 0, count: sites.Length)
                .Where(s => fill[s] < limit)
                .Select(s => (Site: s, Distance: candidates[index: i].Point.DistanceToSquared(other: candidates[index: sites[s]].Point)))
                .Fold(Option<(int Site, double Distance)>.None, static (best, item) => best.Map(held => item.Distance < held.Distance ? item : held).IfNone(item));
            (hits[i], assigned, rejected) = nearest.Match(
                Some: hit => { fill[hit.Site]++; return (hit.Site, assigned + 1, rejected); },
                None: () => (UnassignedSite, assigned, rejected + 1));
        }
        return Some(new CapacityAssignment(Hits: hits, Assigned: assigned, Unassigned: rejected, Residual: (double)rejected / candidates.Count));
    }
    private static Fin<SampleSelection> CapacityCvtSelection(Seq<SampleCandidate> candidates, int count, int limit, int iterations, double tolerance, Op key) =>
        RelaxationSample(candidates: candidates, count: count, iterations: iterations, capacity: Some(limit), key: key).Bind(relaxed => {
            // A degenerate pool takes NO measurement, so the residual column stays empty rather than publishing a
            // fabricated 1.0 a consumer cannot tell from a measured total rejection.
            Option<CapacityAssignment> assignment = AssignUnderCapacity(candidates: candidates, sites: relaxed.Indices, limit: limit);
            return SelectionOf(candidates: candidates, indices: relaxed.Indices,
                algorithm: Some(new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.CapacityLimitedLloydCandidate,
                    Assurances: assignment.Map(held => held.Unassigned == 0 && held.Residual <= tolerance).IfNone(noneValue: false)
                        ? CapabilitySet<SampleAssurance>.Of(SampleAssurance.CapacityResidual)
                        : CapabilitySet<SampleAssurance>.None,
                    TargetCount: Some(count), CapacityResidual: assignment.Map(static held => held.Residual),
                    CapacityAssignedCandidates: assignment.Map(static held => held.Assigned),
                    CapacityUnassignedCandidates: assignment.Map(static held => held.Unassigned),
                    CandidatePoolTruncatedTo: relaxed.TruncatedTo)), key: key);
        });

    private static Fin<(int[] Indices, SampleAlgorithmReceipt Algorithm)> SampleElimination(Seq<SampleCandidate> candidates, int count, double alpha, double beta, double gamma, int seed, Option<(SpatialRank Rank, double Measure)> domainMeasure, Op key) {
        SampleCandidate[] input = [.. candidates.AsIterable()];
        (SpatialRank rank, double measure) = domainMeasure.IfNone(BoundingMeasure(candidates: candidates));
        double dMax = rank.MaxRadius(measure: measure, count: count);
        double dMin = dMax * (1.0 - Math.Pow(x: (double)count / input.Length, y: gamma)) * beta;
        if (input.Length <= count || count <= 0 || !double.IsFinite(dMax) || dMax <= 0.0 || !double.IsFinite(dMin) || dMin < 0.0)
            return Fin.Fail<(int[] Indices, SampleAlgorithmReceipt Algorithm)>(key.InvalidInput());
        // The conflict set is a RADIUS GRAPH over the one neighbourhood owner with its weight on the edge tag, so the
        // elimination decrements the removed sample's INCIDENT edges instead of sweeping every edge per removal.
        return from index in NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: toSeq(input.Select(static item => item.Point))), key: key)
               from neighbors in NeighborKernel.GraphOf(index: index, needles: [.. input.Select(static item => item.Point)], count: Option<int>.None, radius: Some(dMax), key: key)
               let graph = ConflictGraph(input: input, ids: neighbors.Ids, dMax: dMax, dMin: dMin, alpha: alpha)
               let run = Eliminate(graph: graph, input: input, count: count, seed: seed)
               select (Indices: run.Indices,
                   Algorithm: new SampleAlgorithmReceipt(Kind: SampleAlgorithmKind.YukselWeightedSampleElimination, Assurances: CapabilitySet<SampleAssurance>.None,
                       Seed: Some(seed), TargetCount: Some(count), OversampleCount: Some(input.Length),
                       OversampleFactor: Some(input.Length / Math.Max(val1: 1, val2: count)), Alpha: Some(alpha), Beta: Some(beta), Gamma: Some(gamma),
                       Radius: Some(dMax), WeightLimitRadius: Some(dMin), Eliminated: Some(run.Eliminated), NeighborUpdates: Some(run.NeighborUpdates)));
    }
    private static UndirectedGraph<int, TaggedEdge<int, double>> ConflictGraph(SampleCandidate[] input, int[][] ids, double dMax, double dMin, double alpha) {
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
    // Yuksel weighted elimination on a heap: the argmax scan per removal and the full edge sweep after it spelled the
    // published O(n log n) algorithm at O(n² + nE). The deterministic order rank rides the PRIORITY, so the heap alone
    // decides ties and no post-hoc comparison re-breaks them.
    private static (int[] Indices, int Eliminated, int NeighborUpdates) Eliminate(UndirectedGraph<int, TaggedEdge<int, double>> graph, SampleCandidate[] input, int count, int seed) {
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
    private static (SpatialRank Rank, double Measure) BoundingMeasure(Seq<SampleCandidate> candidates) {
        BoundingBox box = new(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        (double dx, double dy, double dz) = (Math.Max(val1: box.Max.X - box.Min.X, val2: 0.0), Math.Max(val1: box.Max.Y - box.Min.Y, val2: 0.0), Math.Max(val1: box.Max.Z - box.Min.Z, val2: 0.0));
        double volume = dx * dy * dz;
        double area = Math.Max(val1: dx * dy, val2: Math.Max(val1: dx * dz, val2: dy * dz));
        return volume > EpsilonPolicy.ZeroTolerance
            ? (SpatialRank.Volumetric, volume)
            : (SpatialRank.Planar, Math.Max(val1: area, val2: EpsilonPolicy.ZeroTolerance));
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
    [StructLayout(LayoutKind.Auto)] private readonly record struct FpoState(int[] Chosen, double BestScore, bool Settled);
    // A round that improves nothing commits its explicit settled column; the error-bearing `Refused` arm is never
    // borrowed for a successful fixpoint.
    private static int[] FpoSample(Seq<SampleCandidate> candidates, int count, int iterations, Op key) {
        int[] seeded = FarthestIndices(candidates: candidates, count: count);
        return seeded.Length < 2
            ? seeded
            : WorstCoverage(candidates: candidates, chosen: seeded)
                .Map(worst => Cell.Converge(
                    cell: Atom(value: new FpoState(Chosen: seeded, BestScore: worst.Distance, Settled: false)),
                    step: state => Some(SwapRound(candidates: candidates, state: state)),
                    settled: static state => state.Settled, budget: Dimension.Create(value: Math.Max(val1: 1, val2: iterations)),
                    declined: key.InvalidResult()).Current.Chosen)
                .IfNone(seeded);
    }
    private static FpoState SwapRound(Seq<SampleCandidate> candidates, FpoState state) =>
        WorstCoverage(candidates: candidates, chosen: state.Chosen).Bind(worst =>
            state.Chosen.Contains(value: worst.Index)
                ? Option<FpoState>.None
                : Enumerable.Range(start: 0, count: state.Chosen.Length)
                    .Select(i => {
                        int[] trial = [.. state.Chosen];
                        trial[i] = worst.Index;
                        return WorstCoverage(candidates: candidates, chosen: trial)
                            .Filter(scored => scored.Distance < state.BestScore)
                            .Map(scored => new FpoState(Chosen: trial, BestScore: scored.Distance, Settled: false));
                    })
                    .FirstOrDefault(static swapped => swapped.IsSome))
        .IfNone(state with { Settled = true });
    // An empty pool has NO worst-covered candidate: an index-zero sentinel beside a negative distance is absence
    // spelled as a magic default the caller then compares.
    private static Option<(int Index, double Distance)> WorstCoverage(Seq<SampleCandidate> candidates, int[] chosen) =>
        candidates.Count <= 0 || chosen.Length <= 0
            ? None
            : Some(Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Distance: chosen.Min(c => candidates[index: i].Point.DistanceToSquared(other: candidates[index: c].Point))))
                .Aggregate((worst, item) => item.Distance > worst.Distance ? item : worst));
    private static Fin<(int[] Indices, Option<int> TruncatedTo)> RelaxationSample(Seq<SampleCandidate> candidates, int count, int iterations, Option<int> capacity, Op key) {
        int total = capacity.Map(limit => Math.Min(val1: candidates.Count, val2: count * limit)).IfNone(candidates.Count);
        bool truncated = total != candidates.Count;
        // Truncation is COVERAGE-PRESERVING: enumeration order over a mesh pool is triangle order, so a prefix take
        // biases the retained set by tessellation, and the cut lands on the receipt rather than nowhere.
        int[] retained = truncated ? FarthestIndices(candidates: candidates, count: total) : [];
        Seq<SampleCandidate> active = truncated ? toSeq(retained.Select(i => candidates[index: i])) : candidates;
        // candidateIndex builds once and stays immutable across rounds; every centroid re-snaps through one GraphOf.
        return NeighborIndex.Of(source: new NeighborSource.StaticCase(Values: toSeq(active.AsIterable().Select(static candidate => candidate.Point))), key: key)
            .Bind(candidateIndex => toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
                initialState: Fin.Succ(FarthestIndices(candidates: active, count: count)),
                f: (state, _) => state.Bind(sites => RelaxSites(sites: sites, candidates: active, candidateIndex: candidateIndex, total: active.Count, capacity: capacity, key: key))))
            .Map(indices => (Indices: truncated ? [.. indices.Select(i => retained[i])] : indices, TruncatedTo: truncated ? Some(total) : Option<int>.None));
    }
    private static Fin<int[]> RelaxSites(int[] sites, Seq<SampleCandidate> candidates, NeighborIndex candidateIndex, int total, Option<int> capacity, Op key) {
        if (sites.Length == 0) return Fin.Succ(sites);
        Fin<int[]> assigned = capacity.Match(
            Some: limit => AssignUnderCapacity(candidates: candidates, sites: sites, limit: limit).Map(static held => held.Hits).ToFin(key.InvalidResult()),
            None: () => NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: toSeq(sites.Select(site => candidates[index: site].Point))), key: key)
                .Bind(siteIndex => NeighborKernel.GraphOf(index: siteIndex, needles: [.. candidates.AsIterable().Select(static candidate => candidate.Point)], count: Some(1), radius: Option<double>.None, key: key))
                .Bind(graph => key.Catch(() => {
                    int[] hits = new int[total];
                    for (int i = 0; i < total; i++) {
                        if (graph.Ids.Length <= i || graph.Ids[i].Length == 0) return Fin.Fail<int[]>(key.InvalidResult());
                        hits[i] = graph.Ids[i][0];
                    }
                    return Fin.Succ(hits);
                })));
        return assigned.Bind(hits => {
            Vector3d[] sums = new Vector3d[sites.Length];
            int[] counts = new int[sites.Length];
            for (int i = 0; i < total; i++) { if (hits[i] < 0) continue; sums[hits[i]] += (Vector3d)candidates[index: i].Point; counts[hits[i]]++; }
            Point3d[] centroids = [.. Enumerable.Range(start: 0, count: sites.Length).Select(s => counts[s] > 0 ? Point3d.Origin + (sums[s] / counts[s]) : candidates[index: sites[s]].Point)];
            return NeighborKernel.GraphOf(index: candidateIndex, needles: centroids, count: Some(total), radius: Option<double>.None, key: key).Bind(snap => key.Catch(() => {
                int[] next = new int[sites.Length];
                IndexSet occupied = [];
                for (int s = 0; s < sites.Length; s++) {
                    if (snap.Ids.Length <= s) return Fin.Fail<int[]>(key.InvalidResult());
                    Option<int> site = None;
                    foreach (int candidate in snap.Ids[s]
                        .Where(id => id >= 0 && id < total)
                        .Distinct()
                        .OrderBy(id => centroids[s].DistanceToSquared(other: candidates[index: id].Point))
                        .ThenBy(static id => id)) {
                        if (!occupied.Add(item: candidate)) continue;
                        site = Some(candidate);
                        break;
                    }
                    if (site.Case is not int taken) return Fin.Fail<int[]>(key.InvalidResult());
                    next[s] = taken;
                }
                return Fin.Succ(next);
            }));
        });
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
