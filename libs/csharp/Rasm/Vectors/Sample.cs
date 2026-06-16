using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DworkSamplingDomain {
    public static readonly DworkSamplingDomain ContinuousMesh = new(key: 0);
    public static readonly DworkSamplingDomain CandidateSet = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SampleAlgorithmKind {
    public static readonly SampleAlgorithmKind Explicit = new(key: 0);
    public static readonly SampleAlgorithmKind BridsonActiveListPoisson = new(key: 1);
    public static readonly SampleAlgorithmKind FarthestCandidate = new(key: 2);
    public static readonly SampleAlgorithmKind FarthestOptimize = new(key: 3);
    public static readonly SampleAlgorithmKind LloydCandidateRelaxation = new(key: 4);
    public static readonly SampleAlgorithmKind CapacityLimitedLloydCandidate = new(key: 5);
    public static readonly SampleAlgorithmKind WeightedMassPropagation = new(key: 6);
    public static readonly SampleAlgorithmKind VariableDensityPoisson = new(key: 7);
    public static readonly SampleAlgorithmKind YukselWeightedSampleElimination = new(key: 8);
    public static readonly SampleAlgorithmKind DworkVariableDensity = new(key: 9);
    public static readonly SampleAlgorithmKind ContinuousPowerCcvt = new(key: 10);
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

// de Goes BNOT dual-weight gauge selector. The ENFORCE-CAPACITY concave Newton Hessian is the density-weighted
// power-diagram graph Laplacian whose nullspace is the constant-per-component vector (k=1 for a connected planar
// domain), so the singular SPSD ascent is gauge-fixed by composing the item-1 surface — never a parallel solve.
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
    public sealed record ExplicitCase : SampleKind { internal ExplicitCase(Seq<Point3d> points) => Points = points; public Seq<Point3d> Points { get; } }
    public sealed record PoissonDiskCase : SampleKind { internal PoissonDiskCase(PositiveMagnitude radius, Dimension attempts, int seed) { Radius = radius; Attempts = attempts; Seed = seed; } public PositiveMagnitude Radius { get; } public Dimension Attempts { get; } public int Seed { get; } }
    public sealed record FarthestCase : SampleKind { internal FarthestCase(Dimension count) => Count = count; public Dimension Count { get; } }
    public sealed record OptimizeCase : SampleKind { internal OptimizeCase(Dimension count, Dimension iterations) { Count = count; Iterations = iterations; } public Dimension Count { get; } public Dimension Iterations { get; } }
    public sealed record LloydCase : SampleKind { internal LloydCase(Dimension count, Dimension iterations) { Count = count; Iterations = iterations; } public Dimension Count { get; } public Dimension Iterations { get; } }
    public sealed record CapacityCase : SampleKind { internal CapacityCase(Dimension count, Dimension limit, Dimension iterations, PositiveMagnitude tolerance) { Count = count; Limit = limit; Iterations = iterations; Tolerance = tolerance; } public Dimension Count { get; } public Dimension Limit { get; } public Dimension Iterations { get; } public PositiveMagnitude Tolerance { get; } }
    public sealed record WeightedCase : SampleKind { internal WeightedCase(Seq<(Point3d Point, double Mass)> points) => Points = points; public Seq<(Point3d Point, double Mass)> Points { get; } }
    public sealed record ScalarDensityCase : SampleKind { internal ScalarDensityCase(ScalarField density, Dimension count) { Density = density; Count = count; } public ScalarField Density { get; } public Dimension Count { get; } }
    public sealed record AdaptiveCase : SampleKind { internal AdaptiveCase(ScalarField density, Dimension count, PositiveMagnitude minSpacing) { Density = density; Count = count; MinSpacing = minSpacing; } public ScalarField Density { get; } public Dimension Count { get; } public PositiveMagnitude MinSpacing { get; } }
    public sealed record SampleEliminationCase : SampleKind { internal SampleEliminationCase(Dimension count, Dimension oversampleFactor, PositiveMagnitude alpha, PositiveMagnitude beta, PositiveMagnitude gamma, int seed) { Count = count; OversampleFactor = oversampleFactor; Alpha = alpha; Beta = beta; Gamma = gamma; Seed = seed; } public Dimension Count { get; } public Dimension OversampleFactor { get; } public PositiveMagnitude Alpha { get; } public PositiveMagnitude Beta { get; } public PositiveMagnitude Gamma { get; } public int Seed { get; } }
    public sealed record DworkVariableDensityCase : SampleKind { internal DworkVariableDensityCase(ScalarField radius, Dimension count, PositiveMagnitude minRadius, Dimension attempts, int seed) { Radius = radius; Count = count; MinRadius = minRadius; Attempts = attempts; Seed = seed; } public ScalarField Radius { get; } public Dimension Count { get; } public PositiveMagnitude MinRadius { get; } public Dimension Attempts { get; } public int Seed { get; } }
    public sealed record PowerCcvtCase : SampleKind { internal PowerCcvtCase(Dimension count, Dimension iterations, PositiveMagnitude tolerance, Option<ScalarField> density, PositiveMagnitude capacityTol, PositiveMagnitude lloydPosTol, PositiveMagnitude gradPosTol, PositiveMagnitude newtonResidualTol, Dimension maxNewton, Dimension lloydIterations, Dimension gradientIterations, PositiveMagnitude armijoC1, PositiveMagnitude armijoBacktrack, PositiveMagnitude armijoInitStep, Dimension armijoMaxHalvings, PositiveMagnitude regularityScale, PositiveMagnitude jitterVariance, PositiveMagnitude magnitudeScale, PositiveMagnitude relocateFraction, PowerCcvtGauge gauge, int seed) { Count = count; Iterations = iterations; Tolerance = tolerance; Density = density; CapacityTol = capacityTol; LloydPosTol = lloydPosTol; GradPosTol = gradPosTol; NewtonResidualTol = newtonResidualTol; MaxNewton = maxNewton; LloydIterations = lloydIterations; GradientIterations = gradientIterations; ArmijoC1 = armijoC1; ArmijoBacktrack = armijoBacktrack; ArmijoInitStep = armijoInitStep; ArmijoMaxHalvings = armijoMaxHalvings; RegularityScale = regularityScale; JitterVariance = jitterVariance; MagnitudeScale = magnitudeScale; RelocateFraction = relocateFraction; Gauge = gauge; Seed = seed; } public Dimension Count { get; } public Dimension Iterations { get; } public PositiveMagnitude Tolerance { get; } public Option<ScalarField> Density { get; } public PositiveMagnitude CapacityTol { get; } public PositiveMagnitude LloydPosTol { get; } public PositiveMagnitude GradPosTol { get; } public PositiveMagnitude NewtonResidualTol { get; } public Dimension MaxNewton { get; } public Dimension LloydIterations { get; } public Dimension GradientIterations { get; } public PositiveMagnitude ArmijoC1 { get; } public PositiveMagnitude ArmijoBacktrack { get; } public PositiveMagnitude ArmijoInitStep { get; } public Dimension ArmijoMaxHalvings { get; } public PositiveMagnitude RegularityScale { get; } public PositiveMagnitude JitterVariance { get; } public PositiveMagnitude MagnitudeScale { get; } public PositiveMagnitude RelocateFraction { get; } public PowerCcvtGauge Gauge { get; } public int Seed { get; } }
    private SampleKind() { }
    public static Fin<SampleKind> Explicit(Seq<Point3d> points, Op? key = null) =>
        Admit(value: new ExplicitCase(points: points), key: key.OrDefault());
    public static Fin<SampleKind> PoissonDisk(double radius, int attempts = 30, int seed = 0, Op? key = null) {
        Op op = key.OrDefault();
        return from r in op.AcceptValidated<PositiveMagnitude>(candidate: radius)
               from a in op.AcceptValidated<Dimension>(candidate: attempts)
               from admitted in Admit(value: new PoissonDiskCase(radius: r, attempts: a, seed: seed), key: op)
               select admitted;
    }
    public static Fin<SampleKind> Farthest(int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(value => Admit(value: new FarthestCase(count: value), key: op));
    }
    public static Fin<SampleKind> Optimize(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (count, iterations) => new OptimizeCase(count: count, iterations: iterations), key: key);
    public static Fin<SampleKind> Lloyd(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (count, iterations) => new LloydCase(count: count, iterations: iterations), key: key);
    public static Fin<SampleKind> Capacity(int count, int capacity, int iterations = 8, double tolerance = 1.0e-6, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from limit in op.AcceptValidated<Dimension>(candidate: capacity)
               from iter in op.AcceptValidated<Dimension>(candidate: iterations)
               from tol in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from admitted in Admit(value: new CapacityCase(count: c, limit: limit, iterations: iter, tolerance: tol), key: op)
               select admitted;
    }
    public static Fin<SampleKind> Weighted(Seq<(Point3d Point, double Mass)> points, Op? key = null) =>
        Admit(value: new WeightedCase(points: points), key: key.OrDefault());
    public static Fin<SampleKind> ScalarDensity(ScalarField density, int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(c => Admit(value: new ScalarDensityCase(density: density, count: c), key: op));
    }
    public static Fin<SampleKind> Adaptive(ScalarField density, int count, double minSpacing, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from spacing in op.AcceptValidated<PositiveMagnitude>(candidate: minSpacing)
               from admitted in Admit(value: new AdaptiveCase(density: density, count: c, minSpacing: spacing), key: op)
               select admitted;
    }
    public static Fin<SampleKind> SampleElimination(int count, int oversampleFactor, double alpha, double beta, double gamma, int seed, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from oversample in op.AcceptValidated<Dimension>(candidate: oversampleFactor)
               from a in op.AcceptValidated<PositiveMagnitude>(candidate: alpha)
               from b in op.AcceptValidated<PositiveMagnitude>(candidate: beta)
               from g in op.AcceptValidated<PositiveMagnitude>(candidate: gamma)
               from admitted in Admit(value: new SampleEliminationCase(count: c, oversampleFactor: oversample, alpha: a, beta: b, gamma: g, seed: seed), key: op)
               select admitted;
    }
    public static Fin<SampleKind> DworkVariableDensity(ScalarField radius, int count, double minRadius, int attempts = 30, int seed = 0, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from min in op.AcceptValidated<PositiveMagnitude>(candidate: minRadius)
               from a in op.AcceptValidated<Dimension>(candidate: attempts)
               from admitted in Admit(value: new DworkVariableDensityCase(radius: radius, count: c, minRadius: min, attempts: a, seed: seed), key: op)
               select admitted;
    }
    public static Fin<SampleKind> PowerCcvt(
        int count, int iterations = 16, double tolerance = 1.0e-6, Option<ScalarField> density = default,
        double capacityTol = 0.01, double lloydPosTol = 0.01, double gradPosTol = 0.1, double newtonResidualTol = 1.0e-6,
        int maxNewton = 32, int lloydIterations = 1, int gradientIterations = 8,
        double armijoC1 = 1.0e-4, double armijoBacktrack = 0.5, double armijoInitStep = 1.0, int armijoMaxHalvings = 32,
        double regularityScale = 0.65, double jitterVariance = 0.05, double magnitudeScale = 0.5, double relocateFraction = 0.05,
        PowerCcvtGauge? gauge = null, int seed = 0, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from iter in op.AcceptValidated<Dimension>(candidate: iterations)
               from tol in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from capTol in op.AcceptValidated<PositiveMagnitude>(candidate: capacityTol)
               from lloydTol in op.AcceptValidated<PositiveMagnitude>(candidate: lloydPosTol)
               from gradTol in op.AcceptValidated<PositiveMagnitude>(candidate: gradPosTol)
               from newtonTol in op.AcceptValidated<PositiveMagnitude>(candidate: newtonResidualTol)
               from maxN in op.AcceptValidated<Dimension>(candidate: maxNewton)
               from lloydN in op.AcceptValidated<Dimension>(candidate: lloydIterations)
               from gradN in op.AcceptValidated<Dimension>(candidate: gradientIterations)
               from c1 in op.AcceptValidated<PositiveMagnitude>(candidate: armijoC1)
               from backtrack in op.AcceptValidated<PositiveMagnitude>(candidate: armijoBacktrack)
               from initStep in op.AcceptValidated<PositiveMagnitude>(candidate: armijoInitStep)
               from maxHalvings in op.AcceptValidated<Dimension>(candidate: armijoMaxHalvings)
               from regularity in op.AcceptValidated<PositiveMagnitude>(candidate: regularityScale)
               from jitter in op.AcceptValidated<PositiveMagnitude>(candidate: jitterVariance)
               from magnitude in op.AcceptValidated<PositiveMagnitude>(candidate: magnitudeScale)
               from relocate in op.AcceptValidated<PositiveMagnitude>(candidate: relocateFraction)
               from admitted in Admit(value: new PowerCcvtCase(count: c, iterations: iter, tolerance: tol, density: density, capacityTol: capTol, lloydPosTol: lloydTol, gradPosTol: gradTol, newtonResidualTol: newtonTol, maxNewton: maxN, lloydIterations: lloydN, gradientIterations: gradN, armijoC1: c1, armijoBacktrack: backtrack, armijoInitStep: initStep, armijoMaxHalvings: maxHalvings, regularityScale: regularity, jitterVariance: jitter, magnitudeScale: magnitude, relocateFraction: relocate, gauge: gauge ?? PowerCcvtGauge.ZeroMean, seed: seed), key: op)
               select admitted;
    }
    internal Fin<SampleKind> Admit(Op key) => this switch {
        ExplicitCase c => c.Points.IsEmpty ? Fin.Fail<SampleKind>(key.InvalidInput()) : Fin.Succ(this),
        PoissonDiskCase => Fin.Succ(this),
        FarthestCase => Fin.Succ(this),
        OptimizeCase => Fin.Succ(this),
        LloydCase => Fin.Succ(this),
        CapacityCase => Fin.Succ(this),
        WeightedCase c => c.Points.IsEmpty
            ? Fin.Fail<SampleKind>(key.InvalidInput())
            : CloudKernel.MassOf(mass: new Arr<double>([.. c.Points.AsIterable().Select(static item => item.Mass)]), count: c.Points.Count, key: key).Map(_ => this),
        ScalarDensityCase c => FieldNabla.NotNull(value: c.Density, key: key).Map(_ => this),
        AdaptiveCase c => FieldNabla.NotNull(value: c.Density, key: key).Map(_ => this),
        SampleEliminationCase c => guard(c.OversampleFactor.Value > 1 && c.Beta.Value <= 1.0, key.InvalidInput()).ToFin().Map(_ => this),
        DworkVariableDensityCase c => FieldNabla.NotNull(value: c.Radius, key: key).Map(_ => this),
        PowerCcvtCase c => guard(c.ArmijoBacktrack.Value < 1.0 && c.ArmijoC1.Value < 1.0 && c.RelocateFraction.Value <= 1.0 && c.Gauge is not null && (c.Density.IsNone || c.Density.Map(static field => field is not null).IfNone(noneValue: false)), key.InvalidInput()).ToFin().Map(_ => this),
        _ => Fin.Fail<SampleKind>(key.InvalidInput()),
    };
    internal static Fin<SampleKind> Admit(SampleKind value, Op key) =>
        FieldNabla.NotNull(value: value, key: key).Bind(kind => kind.Admit(key: key));
    internal Fin<SampleResult> Evaluate(ExtractionDomain domain, Context context, Op key) =>
        Admit(key: key).Bind(kind => SampleKernel.Sample(kind: kind, domain: domain, context: context, key: key));
    internal (Option<int> Count, Option<int> Iterations, double MeshScale, bool Density, SampleAlgorithmKind Algorithm) Request => this switch { ExplicitCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.Explicit), PoissonDiskCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.BridsonActiveListPoisson), FarthestCase c => (Some(c.Count.Value), Option<int>.None, 1.0, false, SampleAlgorithmKind.FarthestCandidate), OptimizeCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false, SampleAlgorithmKind.FarthestOptimize), LloydCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false, SampleAlgorithmKind.LloydCandidateRelaxation), CapacityCase c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value, false, SampleAlgorithmKind.CapacityLimitedLloydCandidate), WeightedCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.WeightedMassPropagation), ScalarDensityCase c => (Some(c.Count.Value), Option<int>.None, 8.0, true, SampleAlgorithmKind.VariableDensityPoisson), AdaptiveCase c => (Some(c.Count.Value), Option<int>.None, 12.0, true, SampleAlgorithmKind.VariableDensityPoisson), SampleEliminationCase c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value, false, SampleAlgorithmKind.YukselWeightedSampleElimination), DworkVariableDensityCase c => (Some(c.Count.Value), Option<int>.None, 12.0, true, SampleAlgorithmKind.DworkVariableDensity), PowerCcvtCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 8.0, c.Density.IsSome, SampleAlgorithmKind.ContinuousPowerCcvt), _ => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.Explicit) };
    internal Option<double> DensityError(int emitted) => Request is { Density: true, Count: Option<int> count } ? count.Map(value => Math.Abs(value: emitted - value) / Math.Max(val1: 1.0, val2: value)) : Option<double>.None;
    internal Fin<TOut> Project<TOut>(ExtractionDomain domain, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from result in Evaluate(domain: domain, context: context, key: op)
               from output in typeof(TOut) switch {
                   Type t when t == typeof(Seq<Point3d>) => Fin.Succ((TOut)(object)result.Points),
                   Type t when t == typeof(VectorCloud) => result.Mass.Match(
                       Some: mass => VectorCloud.WeightedCluster(points: result.Points, mass: toSeq(mass.AsIterable()), context: context, key: op).Map(static value => (TOut)(object)value),
                       None: () => VectorCloud.Cluster(points: result.Points, context: context, key: op).Map(static value => (TOut)(object)value)),
                   Type t when t == typeof(PointCloud) => VectorCloud.Cluster(points: result.Points, context: context, key: op)
                       .Bind(cloud => cloud is VectorCloud.ClusterCase cluster
                           ? Fin.Succ((TOut)(object)cluster.Indexed)
                           : Fin.Fail<TOut>(op.InvalidResult())),
                   Type t when t == typeof(SampleReceipt) => Fin.Succ((TOut)(object)result.Receipt),
                   _ => Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(SampleKind), outputType: typeof(TOut))),
               }
               select output;
    }
    internal Fin<double> MeshCandidateDensity(double area, Op key) {
        double safeArea = Math.Max(val1: area, val2: RhinoMath.SqrtEpsilon);
        double target = this switch {
            ExplicitCase ex => ex.Points.Count,
            PoissonDiskCase pd => safeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: RhinoMath.SqrtEpsilon),
            WeightedCase weighted => weighted.Points.Count,
            _ => Request.Count.Map(value => value * Request.MeshScale).IfNone(0.0),
        };
        return RhinoMath.IsValidDouble(x: target) && target > 0.0
            ? key.AcceptValue(value: Math.Max(val1: target / safeArea, val2: 1.0 / safeArea))
            : Fin.Fail<double>(key.Unsupported(geometryType: GetType(), outputType: typeof(SampleResult)));
    }
    private static Fin<SampleKind> Counted(int count, int value, Func<Dimension, Dimension, SampleKind> create, Op? key) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from v in op.AcceptValidated<Dimension>(candidate: value)
               from admitted in Admit(value: create(arg1: c, arg2: v), key: op)
               select admitted;
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DworkReceipt(DworkSamplingDomain Domain, double RMin, Option<double> BackgroundCellSize, Option<int> BackgroundGridCells, int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain, double LocalRadiusMin, double LocalRadiusMax) {
    public bool CandidateOnly => Domain.Equals(DworkSamplingDomain.CandidateSet);
    public bool ContinuousMesh => Domain.Equals(DworkSamplingDomain.ContinuousMesh);
}

// Restricted-power-cell fragment evidence the BNOT outer schedule rebuilds each iteration: the per-site cell masses
// b_i = m_i and barycenters are the transport facts the capacity Newton and the Lloyd step both consume.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCellFragmentFacts(int SiteCount, int FragmentCount, int FacetCount, int EmptyCellCount, double TotalMass, double MassMin, double MassMax, double IntegrationResidual) {
    public bool IsValid =>
        SiteCount > 0 && FragmentCount >= 0 && FacetCount >= 0 && EmptyCellCount >= 0 && EmptyCellCount <= SiteCount
        && RhinoMath.IsValidDouble(x: TotalMass) && TotalMass >= 0.0
        && RhinoMath.IsValidDouble(x: MassMin) && RhinoMath.IsValidDouble(x: MassMax) && MassMax >= MassMin
        && RhinoMath.IsValidDouble(x: IntegrationResidual) && IntegrationResidual >= 0.0;
}

// de Goes 2012 BNOT continuous-power-CCVT witness. The gauge-fixed ENFORCE-CAPACITY concave Newton (the capacity
// sub-problem) populates the capacity/weight/dual fields and surfaces the composed item-1 SolveReceipt + GaugeReceipt;
// the position/transport/gradient/rebuild/aliased/jittered/relocated/Poisson-radius fields are 7a-typed defaults that
// the 7b two-phase site-motion outer loop populates. TargetMass = totalMass / n; CapacityResidual = max_i |m_i - m*|.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCcvtReceipt(
    int SiteCount, double TargetMass, double ActualMassMin, double ActualMassMax,
    double CapacityResidualInf, double CapacityResidualL1, double CapacityResidualL2, double CapacityResidualNormalized,
    int OuterIterations, int LloydIterations, int GradientIterations, int DualNewtonIterations,
    double WeightMin, double WeightMax, double WeightMean, double TransportEnergy, double TransportEnergyDelta,
    double DualObjective, double CentroidShift, double PositionGradientNorm, double WeightGradientNorm,
    int EmptyCellCount, int StepHalvingCount, int RebuildCount, int AliasedSiteCount, int JitteredSiteCount, int RelocatedSiteCount,
    double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
    PowerCellFragmentFacts Fragments, Option<SolveReceipt> DualSolve = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default) {
    public bool MeanZeroGaugeApplied => Gauge.Equals(PowerCcvtGauge.ZeroMean) && DualSolve.Bind(static solve => solve.Gauge).Map(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero)).IfNone(noneValue: false);
    public bool IsValid =>
        SiteCount > 0 && Gauge is not null && Stop is not null && Fragments.IsValid && Fragments.SiteCount == SiteCount
        && RhinoMath.IsValidDouble(x: TargetMass) && TargetMass > 0.0
        && RhinoMath.IsValidDouble(x: ActualMassMin) && RhinoMath.IsValidDouble(x: ActualMassMax) && ActualMassMax >= ActualMassMin
        && RhinoMath.IsValidDouble(x: CapacityResidualInf) && CapacityResidualInf >= 0.0
        && RhinoMath.IsValidDouble(x: CapacityResidualL1) && CapacityResidualL1 >= 0.0
        && RhinoMath.IsValidDouble(x: CapacityResidualL2) && CapacityResidualL2 >= 0.0
        && RhinoMath.IsValidDouble(x: CapacityResidualNormalized) && CapacityResidualNormalized >= 0.0
        && OuterIterations >= 0 && LloydIterations >= 0 && GradientIterations >= 0 && DualNewtonIterations >= 0
        && RhinoMath.IsValidDouble(x: WeightMin) && RhinoMath.IsValidDouble(x: WeightMax) && WeightMax >= WeightMin
        && RhinoMath.IsValidDouble(x: WeightMean) && RhinoMath.IsValidDouble(x: TransportEnergy) && TransportEnergy >= 0.0
        && RhinoMath.IsValidDouble(x: TransportEnergyDelta) && RhinoMath.IsValidDouble(x: DualObjective)
        && RhinoMath.IsValidDouble(x: CentroidShift) && CentroidShift >= 0.0
        && RhinoMath.IsValidDouble(x: PositionGradientNorm) && PositionGradientNorm >= 0.0
        && RhinoMath.IsValidDouble(x: WeightGradientNorm) && WeightGradientNorm >= 0.0
        && EmptyCellCount >= 0 && EmptyCellCount <= SiteCount && StepHalvingCount >= 0 && RebuildCount >= 0
        && AliasedSiteCount >= 0 && JitteredSiteCount >= 0 && RelocatedSiteCount >= 0
        && RhinoMath.IsValidDouble(x: NormalizedPoissonRadius) && NormalizedPoissonRadius is >= 0.0 and <= 1.0
        && RhinoMath.IsValidDouble(x: PlanarityDeviation) && PlanarityDeviation >= 0.0
        && DualSolve.Map(static solve => solve.IsUsable).IfNone(noneValue: true)
        && Spectrum.Map(static spectrum => spectrum.IsValid).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleAlgorithmReceipt(SampleAlgorithmKind Kind, Option<int> Seed, Option<int> TargetCount, Option<int> OversampleCount, Option<int> OversampleFactor, Option<double> Alpha, Option<double> Beta, Option<double> Gamma, Option<double> Radius, Option<double> WeightLimitRadius, Option<int> Eliminated, Option<int> NeighborUpdates, bool MaximalCoverageGuaranteed, bool CapacityResidualValidated, bool TransportAssignmentValidated, bool MeshSpectrumValidated, Option<int> Attempts = default, Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default, Option<double> DensityMin = default, Option<double> DensityMax = default, Option<double> LocalRadiusMin = default, Option<double> LocalRadiusMax = default, Option<double> CapacityResidual = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default, Option<DworkReceipt> Dwork = default, Option<int> CapacityAssignedCandidates = default, Option<int> CapacityUnassignedCandidates = default, Option<PowerCcvtReceipt> PowerCcvt = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleReceipt(int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<double> MinSpacing, Option<double> MeanSpacing, Option<double> MaxSpacing, Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations, SampleStopKind Stop, SampleDomainStatus DomainStatus, Option<SampleAlgorithmReceipt> Algorithm);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct SampleCandidate(Point3d Point, Option<double> Mass);

internal readonly record struct SampleResult(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleReceipt Receipt);

internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithmReceipt> Algorithm);

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
                    ? CloudKernel.MassOf(cluster: cluster, key: state.Key)
                        .Bind(mass => SampleOnCandidates(
                            kind: state.Kind,
                            candidates: cluster.Vertices.Map((point, index) => new SampleCandidate(Point: point, Mass: Some(mass[index: index]))),
                            admitsPoisson: false,
                            domainMeasure: Option<(int Dimensions, double Measure)>.None,
                            context: state.Context,
                            key: state.Key))
                    : Fin.Fail<SampleResult>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(SampleResult)))),
        };
    private static Fin<SampleResult> SampleAdmitted(Seq<SampleCandidate> points, ExtractionDomain domain, SampleAlgorithmKind algorithm, Context context, Op key) =>
        from admitted in points.Fold(
            initialState: Fin.Succ((Accepted: (Seq<Point3d>)[], Mass: (Seq<double>)[], Weighted: false, Rejected: 0)),
            f: (state, item) => state.Bind(current =>
                AdmitPoint(point: item.Point, domain: domain, context: context, key: key).Match(
                    Succ: accepted => item.Mass.Match(
                        Some: mass => Fin.Succ((Accepted: current.Accepted.Add(accepted), Mass: current.Mass.Add(mass), Weighted: true, current.Rejected)),
                        None: () => Fin.Succ((Accepted: current.Accepted.Add(accepted), current.Mass, current.Weighted, current.Rejected))),
                    Fail: _ => Fin.Succ((current.Accepted, current.Mass, current.Weighted, Rejected: current.Rejected + 1)))))
        from mass in admitted.Weighted && !admitted.Accepted.IsEmpty
            ? NormalizeMass(mass: admitted.Mass, key: key).Map(Some)
            : Fin.Succ(Option<Arr<double>>.None)
        select new SampleResult(
            Points: admitted.Accepted,
            Mass: mass,
            Receipt: ReceiptOf(attempted: points.Count, emitted: admitted.Accepted, rejected: admitted.Rejected, candidates: Some(points.Count), iterations: Option<int>.None, stop: admitted.Accepted.IsEmpty ? SampleStopKind.AllRejected : SampleStopKind.Completed, status: SampleDomainStatus.Projected, densityError: Option<double>.None, algorithm: Some(AlgorithmFacts(kind: algorithm))));
    private static Fin<Point3d> AdmitPoint(Point3d point, ExtractionDomain domain, Context context, Op key) =>
        key.AcceptValue(value: point).Bind(valid => domain.Switch(
            state: (Point: valid, Context: context, Key: key),
            supportCase: static (state, d) => d.Value.Closest(sample: state.Point, key: state.Key)
                .Bind(hit => state.Key.AcceptValue(value: hit.Point)),
            meshCase: static (state, d) => Optional(d.Value.Native.ClosestMeshPoint(testPoint: state.Point, maximumDistance: state.Context.Absolute.Value))
                .ToFin(state.Key.InvalidResult())
                .Bind(meshPoint => state.Key.AcceptValue(value: meshPoint.Point)),
            cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                ? (cluster.Vertices.Find(vertex => vertex.DistanceToSquared(other: state.Point) <= state.Context.Absolute.Value * state.Context.Absolute.Value) switch {
                    { IsSome: true, Case: Point3d hit } => state.Key.AcceptValue(value: hit),
                    _ => Fin.Fail<Point3d>(state.Key.InvalidInput()),
                })
                : Fin.Fail<Point3d>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(Point3d)))));
    private static Fin<SampleResult> SampleGeneratedSupport(SampleKind kind, SupportSpace space, Context context, Op key) =>
        (kind switch {
            SampleKind.FarthestCase or SampleKind.OptimizeCase or SampleKind.LloydCase or SampleKind.CapacityCase or SampleKind.SampleEliminationCase => kind.Request.Count.ToFin(Fail: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<int>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }).Bind(count =>
            from domain in ExtractionDomain.Support(value: space, key: key)
            from points in GeometryKernel.SamplePoints(source: space.Value, count: (int)Math.Ceiling(a: count * Math.Max(val1: kind.Request.MeshScale, val2: 1.0)), context: context, key: key)
            from sampled in kind is SampleKind.SampleEliminationCase
                ? SampleOnCandidates(kind: kind, candidates: points.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: false, domainMeasure: Option<(int Dimensions, double Measure)>.None, context: context, key: key)
                : SampleAdmitted(points: points.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), domain: domain, algorithm: kind.Request.Algorithm, context: context, key: key)
            select sampled);
    private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        if (kind is SampleKind.PowerCcvtCase power)
            return PowerCcvtMeshSolve(domain: domain, kind: power, context: context, key: key);
        if (kind is SampleKind.DworkVariableDensityCase dwork)
            return from selection in DworkVariableDensityMeshSelection(domain: domain, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts.Value, seed: dwork.Seed, context: context, key: key)
                   let points = toSeq(selection.Points)
                   let receipt = selection.Algorithm.Bind(static algorithm => algorithm.Dwork)
                   let rejected = receipt.Map(static value => value.RejectedTooClose + value.RejectedDomain).IfNone(0)
                   let attempted = receipt.Map(static value => value.GeneratedCandidates).IfNone(points.Count + rejected)
                   let result = new SampleResult(
                       Points: points,
                       Mass: selection.Mass,
                       Receipt: ReceiptOf(
                           attempted: attempted,
                           emitted: points,
                           rejected: rejected,
                           candidates: Option<int>.None,
                           iterations: Option<int>.None,
                           stop: points.Count <= 0 ? SampleStopKind.AllRejected : points.Count < dwork.Count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                           status: rejected > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                           densityError: kind.DensityError(emitted: points.Count),
                           algorithm: selection.Algorithm))
                   from validated in MeshKernel.ValidateSamplingSpectrum(space: domain, result: result, key: key)
                   select validated;
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).ToFin(key.InvalidResult()).Bind(p =>
            from density in kind.MeshCandidateDensity(area: p.Area, key: key)
            from candidates in MeshKernel.SurfaceCandidatePoints(space: domain, density: density, key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), admitsPoisson: true, domainMeasure: Some((Dimensions: 2, Measure: p.Area)), context: context, key: key)
            from validated in MeshKernel.ValidateSamplingSpectrum(space: domain, result: sampled, key: key)
            select validated);
    }
    // de Goes 2012 BNOT continuous power CCVT — capacity sub-problem (item 7a). Fits the mesh canonical plane, draws n
    // density-importance-sampled sites in-plane (seed-explicit, deterministic), runs the gauge-fixed ENFORCE-CAPACITY
    // concave Newton in dual weights W (composing item-1 SingularSolveDetailed + GaugePolicy.MeanZeroConstant, NEVER a
    // parallel solve), then takes a SINGLE Lloyd step q_i <- b_i. The two-phase site-motion outer loop is 7b; the
    // position/transport/gradient/rebuild/aliased/jittered/relocated/Poisson-radius receipt fields default here.
    private static Fin<SampleResult> PowerCcvtMeshSolve(MeshSpace domain, SampleKind.PowerCcvtCase kind, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).Map(static p => p.Area).Filter(static area => RhinoMath.IsValidDouble(x: area) && area > 0.0).ToFin(key.InvalidResult()).Bind(meshArea =>
            from density in kind.MeshCandidateDensity(area: meshArea, key: key)
            from candidates in MeshKernel.SurfaceCandidatePoints(space: domain, density: density, key: key)
            from fit in CanonicalPlaneOf(points: candidates, key: key)
            let sites = DensityImportanceSites(candidates: candidates, count: Math.Min(val1: kind.Count.Value, val2: candidates.Count), density: kind.Density, context: context, seed: kind.Seed, key: key)
            from run in new PowerCcvtRun(domain: domain, kind: kind, sites: sites, totalMass: meshArea, planarityDeviation: fit.Deviation, context: context, key: key).Run()
            from validated in MeshKernel.ValidateSamplingSpectrum(space: domain, result: run, key: key)
            select SurfacePowerCcvtReceipt(result: validated));
    }
    // Canonical plane fit via the RhinoCommon static (boundary-admitted exactly as the cloud best-fit seam). BNOT is
    // intrinsically 2D planar; maximumDeviation witnesses faithfulness on curved patches (FieldNabla.FitPlaneToPoints
    // does not exist — the static is the only canonical-plane fit).
    private static Fin<(Plane Plane, double Deviation)> CanonicalPlaneOf(Seq<Point3d> points, Op key) =>
        (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane plane, maximumDeviation: out double deviation), plane) switch {
            (PlaneFitResult.Success, { IsValid: true } valid) => from acceptedPlane in key.AcceptValue(value: valid) from acceptedDeviation in key.AcceptValue(value: deviation) select (Plane: acceptedPlane, Deviation: acceptedDeviation),
            _ => Fin.Fail<(Plane Plane, double Deviation)>(error: key.InvalidResult()),
        };
    // Density-importance site draw: rank candidates by -log(U)/w (exponential-clock weighted reservoir, deterministic in
    // seed); constant density falls back to farthest-point coverage. The continuous OT solver never reuses discrete
    // Sinkhorn / CapacityCvtSelection / RelaxationSample.
    private static Seq<Point3d> DensityImportanceSites(Seq<Point3d> candidates, int count, Option<ScalarField> density, Context context, int seed, Op key) =>
        density.Match(
            Some: field => toSeq(Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Weight: field.SampleScalar(sample: candidates[index: i], context: context, key: key).Match(Succ: value => key.AcceptValidated<PositiveMagnitude>(candidate: value).IsSucc ? value : 0.0, Fail: static _ => 0.0)))
                .Where(static row => row.Weight > 0.0)
                .OrderBy(row => -Math.Log(d: UnitInterval(point: candidates[index: row.Index], salt: 0, seed: seed)) / row.Weight)
                .Take(count: count)
                .Select(row => candidates[index: row.Index])),
            None: () => toSeq(FarthestIndices(candidates: candidates.Map(static point => new SampleCandidate(Point: point, Mass: Option<double>.None)), count: count).Select(i => candidates[index: i])));
    // Re-run the spectrum validator's surfaced child receipt into the PowerCcvtReceipt so the typed witness carries the
    // mesh-spectrum evidence alongside the dual-solve receipt — one fact stream, no parallel spectrum field.
    private static SampleResult SurfacePowerCcvtReceipt(SampleResult result) =>
        result.Receipt.Algorithm.Bind(static algorithm => algorithm.PowerCcvt.Map(ccvt => (Algorithm: algorithm, Ccvt: ccvt))).Match(
            Some: pair => result with {
                Receipt = result.Receipt with {
                    Algorithm = Some(pair.Algorithm with { PowerCcvt = Some(pair.Ccvt with { Spectrum = pair.Algorithm.Spectrum }) }),
                },
            },
            None: () => result);
    private sealed class PowerCcvtRun {
        private readonly MeshSpace domain;
        private readonly SampleKind.PowerCcvtCase kind;
        private readonly Seq<Point3d> sites;
        private readonly double totalMass;
        private readonly double planarityDeviation;
        private readonly Context context;
        private readonly Op key;
        private readonly int siteCount;
        private readonly double targetMass;
        private readonly double searchDistance;
        internal PowerCcvtRun(MeshSpace domain, SampleKind.PowerCcvtCase kind, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context, Op key) {
            this.domain = domain;
            this.kind = kind;
            this.sites = sites;
            this.totalMass = totalMass;
            this.planarityDeviation = planarityDeviation;
            this.context = context;
            this.key = key;
            siteCount = sites.Count;
            targetMass = totalMass / Math.Max(val1: 1, val2: sites.Count);
            searchDistance = domain.Native.GetBoundingBox(accurate: true).Diagonal.Length;
        }
        // The two-phase de Goes BNOT driver. Each outer iteration runs the gauge-fixed ENFORCE-CAPACITY concave Newton
        // ASCENT in dual weights W (the capacity sub-problem) over the restricted power-cell fragments rebuilt at the live
        // sites, then takes a two-phase site step — Lloyd q_i <- b_i followed by an Armijo-INCREASE transport-energy
        // gradient ascent. The per-iteration diagram rebuild (mass m_i, barycenter b_i, symmetric facet weights), the
        // in-place Hessian/gradient triplet assembly, and the monotone dual-objective Armijo line search are the named
        // statement-kernel exemption; the OUTER convergence schedule is domain flow — a Schedule-driven RepeatWhile over
        // the advanced (sites, weights) state, partitioned Converged | StoppedWithoutConvergence, NOT a raw counter and
        // never budget-exhaustion -> Fin.Fail. The converged sites are then regularity-broken once, lifted to the surface,
        // and the previously-defaulted receipt fields populated.
        internal Fin<SampleResult> Run() =>
            siteCount < 1
                ? Fin.Fail<SampleResult>(key.InvalidResult())
                : ConvergeNewton(currentSites: sites, seed: RebuildDiagram(currentSites: sites, weights: ZeroWeights()))
                    .Bind(seed => ConvergeOuter(seed: OuterState.Of(sites: sites, capacity: seed)).Bind(Finalize));
        private Arr<double> ZeroWeights() => new([.. Enumerable.Repeat(element: 0.0, count: siteCount)]);
        // Schedule-driven two-phase outer loop. An Atom holds the advanced (sites, weights, diagram) state; IO.lift swaps
        // one outer step into it and RepeatWhile retires the iteration counter, stopping on the scale-relative joint
        // criterion (Lloyd displacement <= kappa_L * meanSpacing AND position-gradient norm <= kappa_X * meanSpacing).
        // Budget exhaustion is a typed terminal (Stop = StoppedWithoutConvergence), never Fin.Fail.
        private Fin<OuterState> ConvergeOuter(OuterState seed) {
            Atom<OuterState> cell = Atom(value: seed);
            _ = IO.lift(() => { _ = cell.Swap(f: state => state.Converged || state.Fault.IsSome ? state : OuterStep(state: state)); })
                .RepeatWhile(schedule: Schedule.recurs(times: kind.Iterations.Value), predicate: _ => !cell.Value.Converged && cell.Value.Fault.IsNone)
                .Run();
            OuterState terminal = cell.Value;
            return terminal.Fault.Match(Some: Fin.Fail<OuterState>, None: () => Fin.Succ(terminal));
        }
        // One outer step: two-phase site motion (Lloyd q_i <- b_i, then Armijo transport-energy gradient ASCENT) off the
        // already-capacity-converged diagram, then re-run ENFORCE-CAPACITY at the advanced sites. Convergence is the joint
        // displacement/gradient criterion; the per-iteration capacity rebuild keeps the fragments live (d_ij offset
        // recomputed from the dual weights inside RestrictedPowerCells, never trusted from item-6 PowerFacet.OffsetI).
        private OuterState OuterStep(OuterState state) {
            SiteMotion motion = TwoPhaseSiteMotion(currentSites: state.Sites, capacity: state.Capacity);
            double meanSpacing = MeanSpacingOf(points: motion.Sites);
            return ConvergeNewton(currentSites: motion.Sites, seed: RebuildDiagram(currentSites: motion.Sites, weights: state.Capacity.Weights)).Match(
                Succ: advanced => state with {
                    Sites = motion.Sites,
                    Capacity = advanced,
                    OuterIterations = state.OuterIterations + 1,
                    LloydIterations = state.LloydIterations + motion.LloydIterations,
                    GradientIterations = state.GradientIterations + motion.GradientIterations,
                    StepHalvings = state.StepHalvings + motion.GradientHalvings,
                    RebuildCount = state.RebuildCount + advanced.RebuildCount + motion.GradientHalvings + 2,
                    PositionGradientNorm = motion.PositionGradientNorm,
                    CentroidShift = motion.Displacement,
                    TransportEnergyDelta = advanced.TransportEnergy - state.Capacity.TransportEnergy,
                    Converged = motion.Displacement <= kind.LloydPosTol.Value * meanSpacing && motion.PositionGradientNorm <= kind.GradPosTol.Value * meanSpacing,
                },
                Fail: error => state with { Fault = Some(error) });
        }
        // Two-phase site motion off the converged power diagram. Phase 1 (Lloyd, kind.LloydIterations sweeps): q_i <- b_i,
        // the cell barycenter (Aurenhammer-dominated empty cells hold their seat). Phase 2 (gradient ascent on the
        // transport energy E = Sum_i Integral_cell rho |x - q_i|^2): the per-site descent direction of E is -2 m_i (q_i -
        // b_i), so ASCENT on -E (equivalently Lloyd's energy minimisation) moves q_i toward b_i; the Armijo-INCREASE line
        // search accepts the largest step for which -E strictly improves. Both phases re-evaluate the cell masses/
        // barycentres at the moved sites; the position gradient norm is the post-motion ||2 m_i (q_i - b_i)||.
        private SiteMotion TwoPhaseSiteMotion(Seq<Point3d> currentSites, NewtonState capacity) {
            (Seq<Point3d> lloydSites, int lloydSweeps, RestrictedPowerDiagram lloydDiagram) = LloydPhase(currentSites: currentSites, diagram: capacity.Diagram, weights: capacity.Weights);
            (Seq<Point3d> gradientSites, int gradientSteps, int gradientHalvings, RestrictedPowerDiagram gradientDiagram) = GradientPhase(currentSites: lloydSites, diagram: lloydDiagram, weights: capacity.Weights);
            double displacement = PairwiseShift(from: currentSites, to: gradientSites);
            double positionGradientNorm = PositionGradientNormOf(sitesAt: gradientSites, diagram: gradientDiagram);
            return new SiteMotion(Sites: gradientSites, LloydIterations: lloydSweeps, GradientIterations: gradientSteps, GradientHalvings: gradientHalvings, Displacement: displacement, PositionGradientNorm: positionGradientNorm);
        }
        // Phase 1 — Lloyd relaxation: kind.LloydIterations sweeps of q_i <- b_i, each rebuilding the diagram at the moved
        // sites so successive barycentres track the advancing partition. A rebuild failure freezes the last good sites/
        // diagram (the capacity solve already proved the partition admissible) rather than failing the rail.
        private (Seq<Point3d> Sites, int Sweeps, RestrictedPowerDiagram Diagram) LloydPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) {
            Seq<Point3d> liveSites = currentSites;
            RestrictedPowerDiagram liveDiagram = diagram;
            int sweeps = 0;
            for (int sweep = 0; sweep < kind.LloydIterations.Value; sweep++) {
                Seq<Point3d> moved = BarycenterSites(currentSites: liveSites, diagram: liveDiagram);
                Fin<RestrictedPowerDiagram> rebuilt = RebuildPowerCells(currentSites: moved, weights: weights);
                if (rebuilt.IsFail) break;
                liveSites = moved;
                liveDiagram = rebuilt.Match(Succ: static value => value, Fail: _ => liveDiagram);
                sweeps++;
            }
            return (Sites: liveSites, Sweeps: sweeps, Diagram: liveDiagram);
        }
        // Phase 2 — Armijo-INCREASE transport-energy gradient ascent. The ascent target is -E (E the CCVT transport
        // energy); its per-site gradient is +2 m_i (b_i - q_i), so the step moves q_i toward b_i and accepts the largest
        // alpha for which -E(q + alpha d) >= -E(q) + c1 alpha (grad . d). A sufficient-DECREASE test here would stall the
        // outer loop at iteration 1 (concave maximisation) — the monotone -E is the guard. kind.GradientIterations steps.
        private (Seq<Point3d> Sites, int Steps, int Halvings, RestrictedPowerDiagram Diagram) GradientPhase(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram, Arr<double> weights) {
            Seq<Point3d> liveSites = currentSites;
            RestrictedPowerDiagram liveDiagram = diagram;
            int steps = 0, halvings = 0;
            for (int step = 0; step < kind.GradientIterations.Value; step++) {
                Vector3d[] direction = AscentDirection(sitesAt: liveSites, diagram: liveDiagram);
                double slope = AscentSlope(direction: direction);
                if (!(RhinoMath.IsValidDouble(x: slope) && slope > 0.0)) break;
                double baseEnergy = -TransportEnergyOf(diagram: liveDiagram);
                (Seq<Point3d> Sites, RestrictedPowerDiagram Diagram, int Halvings, bool Improved) = AscentLineSearch(currentSites: liveSites, direction: direction, slope: slope, baseEnergy: baseEnergy, weights: weights, alpha: kind.ArmijoInitStep.Value, halvings: 0);
                halvings += Halvings;
                if (!Improved) break;
                liveSites = Sites;
                liveDiagram = Diagram;
                steps++;
            }
            return (Sites: liveSites, Steps: steps, Halvings: halvings, Diagram: liveDiagram);
        }
        // Backtracking Armijo-INCREASE on -E: accept the largest alpha for which -E(q + alpha d) >= -E(q) + c1 alpha slope.
        private (Seq<Point3d> Sites, RestrictedPowerDiagram Diagram, int Halvings, bool Improved) AscentLineSearch(Seq<Point3d> currentSites, Vector3d[] direction, double slope, double baseEnergy, Arr<double> weights, double alpha, int halvings) {
            Seq<Point3d> trial = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => currentSites[index: i] + (alpha * direction[i])));
            Fin<RestrictedPowerDiagram> rebuilt = RebuildPowerCells(currentSites: trial, weights: weights);
            return rebuilt.Match(
                Succ: trialDiagram => -TransportEnergyOf(diagram: trialDiagram) >= baseEnergy + (kind.ArmijoC1.Value * alpha * slope)
                    ? (Sites: trial, Diagram: trialDiagram, Halvings: halvings, Improved: true)
                    : halvings >= kind.ArmijoMaxHalvings.Value
                        ? (Sites: currentSites, Diagram: default, Halvings: halvings, Improved: false)
                        : AscentLineSearch(currentSites: currentSites, direction: direction, slope: slope, baseEnergy: baseEnergy, weights: weights, alpha: alpha * kind.ArmijoBacktrack.Value, halvings: halvings + 1),
                Fail: _ => halvings >= kind.ArmijoMaxHalvings.Value
                    ? (Sites: currentSites, Diagram: default, Halvings: halvings, Improved: false)
                    : AscentLineSearch(currentSites: currentSites, direction: direction, slope: slope, baseEnergy: baseEnergy, weights: weights, alpha: alpha * kind.ArmijoBacktrack.Value, halvings: halvings + 1));
        }
        // q_i <- b_i over every cell; empty (Aurenhammer-dominated) cells keep their seat. The barycentre is the
        // density-weighted first moment item 6 already integrated, so the Lloyd target needs no re-integration.
        private Seq<Point3d> BarycenterSites(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram) =>
            toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: diagram, site: i).Match(
                Some: cell => cell.Empty || !cell.Barycenter.IsValid ? currentSites[index: i] : cell.Barycenter,
                None: () => currentSites[index: i])));
        // Per-site ascent direction of -E: +2 m_i (b_i - q_i) scaled to unit per-site mass; empty cells contribute zero.
        private Vector3d[] AscentDirection(Seq<Point3d> sitesAt, RestrictedPowerDiagram diagram) =>
            [.. Enumerable.Range(start: 0, count: siteCount).Select(i => CellOf(diagram: diagram, site: i).Match(
                Some: cell => cell.Empty || !cell.Barycenter.IsValid ? Vector3d.Zero : 2.0 * Math.Max(val1: cell.Mass, val2: 0.0) * (cell.Barycenter - sitesAt[index: i]),
                None: () => Vector3d.Zero))];
        private static double AscentSlope(Vector3d[] direction) =>
            direction.Aggregate(seed: 0.0, func: static (acc, d) => acc + d.SquareLength);
        // ||+2 m_i (b_i - q_i)|| over all sites — the post-motion position-gradient norm (the centroidal-CVT residual).
        private double PositionGradientNormOf(Seq<Point3d> sitesAt, RestrictedPowerDiagram diagram) {
            Vector3d[] direction = AscentDirection(sitesAt: sitesAt, diagram: diagram);
            return Math.Sqrt(d: direction.Aggregate(seed: 0.0, func: static (acc, d) => acc + d.SquareLength));
        }
        // E = Sum_i TransportCost_i where item 6 integrates TransportCost_i = Integral_cell rho |x - q_i|^2 about the SITE
        // q_i directly (not the barycentre), so the rebuilt diagram already carries the exact transport energy at the
        // moved sites — adding a parallel-axis m_i |q_i - b_i|^2 term would double-count. The sitesAt argument is unused
        // here precisely because the integral is site-anchored at build time, and is kept for call-shape symmetry with the
        // gradient/position-norm folds that DO read the moved sites.
        private static double TransportEnergyOf(RestrictedPowerDiagram diagram) =>
            diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.TransportCost);
        // Item-6 emits cells in site order (Cells[i].Site == i, Cells.Count == siteCount), so the per-site lookup is a
        // bounds-guarded positional read — no Find scan, matching the capacity Newton's positional cell access.
        private static Option<PowerCell> CellOf(RestrictedPowerDiagram diagram, int site) =>
            site >= 0 && site < diagram.Cells.Count ? Some(diagram.Cells[index: site]) : Option<PowerCell>.None;
        private Fin<RestrictedPowerDiagram> RebuildPowerCells(Seq<Point3d> currentSites, Arr<double> weights) =>
            MeshKernel.RestrictedPowerCells(space: domain, sites: currentSites, weights: Some(weights), density: kind.Density, key: key);
        // Per-index Euclidean displacement sum between two equal-length site sequences (the Lloyd+gradient centroid shift).
        private static double PairwiseShift(Seq<Point3d> from, Seq<Point3d> to) =>
            Enumerable.Range(start: 0, count: Math.Min(val1: from.Count, val2: to.Count)).Sum(i => from[index: i].DistanceTo(other: to[index: i]));
        // Mean nearest-neighbour spacing as the scale for the outer Lloyd/gradient stopping thresholds (NEVER an absolute
        // literal). Falls back to the hexagonal-packing reference when fewer than two sites carry a finite neighbour.
        private double MeanSpacingOf(Seq<Point3d> points) {
            if (points.Count < 2) return Math.Sqrt(d: 2.0 * totalMass / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: siteCount)));
            double accum = 0.0; int counted = 0;
            for (int i = 0; i < points.Count; i++) {
                double nearest = double.PositiveInfinity;
                for (int j = 0; j < points.Count; j++) if (j != i) nearest = Math.Min(val1: nearest, val2: points[index: i].DistanceTo(other: points[index: j]));
                if (RhinoMath.IsValidDouble(x: nearest) && !double.IsPositiveInfinity(d: nearest)) { accum += nearest; counted++; }
            }
            double reference = Math.Sqrt(d: 2.0 * totalMass / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: siteCount)));
            return counted > 0 ? accum / counted : reference;
        }
        // Schedule-driven dual-Newton at the given sites: an Atom holds the advanced weight state; IO.lift swaps one Newton
        // ascent step into it and RepeatWhile retires the iteration counter, stopping on the scale-relative capacity
        // residual. Partitioned Converged | StoppedWithoutConvergence — budget exhaustion is a typed terminal, never
        // Fin.Fail; a failed seed rebuild routes to the fault arm so the outer loop terminates on the typed error.
        private Fin<NewtonState> ConvergeNewton(Seq<Point3d> currentSites, Fin<NewtonState> seed) =>
            seed.Bind(seedState => {
                Atom<NewtonState> cell = Atom(value: seedState);
                double residualFloor = kind.CapacityTol.Value * targetMass;
                _ = IO.lift(() => { _ = cell.Swap(f: state => state.Converged || state.Fault.IsSome ? state : NewtonStep(currentSites: currentSites, state: state)); })
                    .RepeatWhile(schedule: Schedule.recurs(times: kind.MaxNewton.Value), predicate: _ => !cell.Value.Converged && cell.Value.Fault.IsNone)
                    .Run();
                NewtonState terminal = cell.Value;
                return terminal.Fault.Match(Some: Fin.Fail<NewtonState>, None: () => Fin.Succ(terminal with { Converged = terminal.Residual.Inf <= residualFloor }));
            });
        // One concave-Newton ascent step: solve L delta = g (gauge-fixed, L the density-weighted power-graph Laplacian,
        // g_i = m* - m_i the consistent RHS with Sum g_i = 0), Armijo line search on the monotone dual objective
        // (sufficient-INCREASE — concave maximisation), then rebuild the diagram at the advanced weights.
        private NewtonState NewtonStep(Seq<Point3d> currentSites, NewtonState state) {
            Arr<double> gradient = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => targetMass - state.Diagram.Cells[index: i].Mass)]);
            double gradNorm = Math.Sqrt(d: gradient.AsIterable().Fold(initialState: 0.0, f: static (acc, value) => acc + (value * value)));
            return HessianTriplets(currentSites: currentSites, diagram: state.Diagram).Bind(triplets =>
                    SparseMatrix.FromTriplets(rows: Dimension.Create(value: siteCount), cols: Dimension.Create(value: siteCount), triplets: triplets, key: key))
                .Bind(laplacian => laplacian.SingularSolveDetailed(rhs: gradient, gauge: kind.Gauge.Policy(fragmentMasses: FragmentMasses(diagram: state.Diagram)), context: context, key: key))
                .Bind(solve => solve.IsUsable
                    ? ArmijoAscent(currentSites: currentSites, state: state, direction: solve.Solution, gradient: gradient).Map(advanced => (Solve: solve, Advanced: advanced))
                    : Fin.Fail<(SolveReceipt Solve, NewtonState Advanced)>(key.InvalidResult()))
                .Match(
                    Succ: step => step.Advanced with {
                        DualSolve = Some(step.Solve),
                        WeightGradientNorm = gradNorm,
                        NewtonIterations = state.NewtonIterations + 1,
                        StepHalvings = step.Advanced.StepHalvings,
                        Converged = step.Advanced.Residual.Inf <= kind.CapacityTol.Value * targetMass,
                    },
                    Fail: error => state with { Fault = Some(error), DualSolve = state.DualSolve });
        }
        // Backtracking Armijo on the dual objective Phi(W) — ASCENT: accept the largest alpha for which
        // Phi(W + alpha*delta) >= Phi(W) + c1*alpha*(g . delta); a sufficient-decrease test would stall at iteration 1.
        private Fin<NewtonState> ArmijoAscent(Seq<Point3d> currentSites, NewtonState state, Arr<double> direction, Arr<double> gradient) {
            double slope = Enumerable.Range(start: 0, count: siteCount).Sum(i => gradient[index: i] * direction[index: i]);
            double baseObjective = state.DualObjective;
            return AscentSearch(currentSites: currentSites, state: state, direction: direction, slope: slope, baseObjective: baseObjective, alpha: kind.ArmijoInitStep.Value, halvings: 0);
        }
        private Fin<NewtonState> AscentSearch(Seq<Point3d> currentSites, NewtonState state, Arr<double> direction, double slope, double baseObjective, double alpha, int halvings) {
            Arr<double> advanced = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => state.Weights[index: i] + (alpha * direction[index: i]))]);
            return RebuildDiagram(currentSites: currentSites, weights: advanced).Bind(rebuilt =>
                rebuilt.DualObjective >= baseObjective + (kind.ArmijoC1.Value * alpha * slope) || halvings >= kind.ArmijoMaxHalvings.Value
                    ? Fin.Succ(rebuilt with { StepHalvings = state.StepHalvings + halvings, RebuildCount = state.RebuildCount + halvings + 2, NewtonIterations = state.NewtonIterations })
                    : AscentSearch(currentSites: currentSites, state: state, direction: direction, slope: slope, baseObjective: baseObjective, alpha: alpha * kind.ArmijoBacktrack.Value, halvings: halvings + 1));
        }
        // Density-weighted power-graph Laplacian L (SPSD, constant nullspace): edge weight w_ij = l_ij / (2 |p_i - p_j|)
        // per facet (constant density rho-bar = 1); the FIFO-pushed A_ij == A_ji symmetry from item 6 is inherited by
        // scattering BOTH (i,j) and (j,i) off-diagonals plus the matched diagonal accumulation. d_ij = l_ij/2 +
        // (w_i - w_j)/(2 l_ij) is recomputed from the live dual weights but the Laplacian weight needs only l_ij and the
        // primal site distance, so the offset is the transport-cost factor, never trusted from item-6 PowerFacet.OffsetI.
        private Fin<List<(int Row, int Col, double Value)>> HessianTriplets(Seq<Point3d> currentSites, RestrictedPowerDiagram diagram) {
            List<(int Row, int Col, double Value)> triplets = [];
            double[] diagonal = new double[siteCount];
            double floor = kind.NewtonResidualTol.Value * Math.Max(val1: searchDistance, val2: RhinoMath.SqrtEpsilon);
            foreach (PowerFacet facet in diagram.Facets.AsIterable()) {
                if (facet.SiteI < 0 || facet.SiteI >= siteCount || facet.SiteJ < 0 || facet.SiteJ >= siteCount || facet.SiteI == facet.SiteJ) continue;
                double siteDistance = currentSites[index: facet.SiteI].DistanceTo(other: currentSites[index: facet.SiteJ]);
                if (!(RhinoMath.IsValidDouble(x: siteDistance) && siteDistance > floor && RhinoMath.IsValidDouble(x: facet.Length) && facet.Length >= 0.0)) continue;
                double weight = facet.Length / (2.0 * siteDistance);
                if (!(RhinoMath.IsValidDouble(x: weight) && weight >= 0.0)) continue;
                triplets.Add(item: (facet.SiteI, facet.SiteJ, -weight));
                triplets.Add(item: (facet.SiteJ, facet.SiteI, -weight));
                diagonal[facet.SiteI] += weight;
                diagonal[facet.SiteJ] += weight;
            }
            double tikhonov = kind.NewtonResidualTol.Value * Math.Max(val1: targetMass, val2: RhinoMath.SqrtEpsilon);
            for (int i = 0; i < siteCount; i++) triplets.Add(item: (i, i, diagonal[i] + tikhonov));
            return triplets.Exists(static row => row.Value != 0.0) ? Fin.Succ(triplets) : Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidResult());
        }
        private Arr<double> FragmentMasses(RestrictedPowerDiagram diagram) =>
            new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => Math.Max(val1: diagram.Cells[index: i].Mass, val2: 0.0))]);
        // Rebuild the restricted power diagram at the given sites/weights and recompute the advanced NewtonState facts:
        // cell masses m_i, capacity residual g_i = m_i - m*, and the monotone dual objective Phi = Sum_i TransportCost_i +
        // Sum_i w_i (m* - m_i) (the de Goes semi-discrete OT dual the ascent maximises).
        private Fin<NewtonState> RebuildDiagram(Seq<Point3d> currentSites, Arr<double> weights) =>
            RebuildPowerCells(currentSites: currentSites, weights: weights)
                .Map(diagram => {
                    double transport = diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.TransportCost);
                    double dual = transport + Enumerable.Range(start: 0, count: siteCount).Sum(i => weights[index: i] * (targetMass - diagram.Cells[index: i].Mass));
                    CapacityResidual residual = CapacityResidualOf(diagram: diagram);
                    return new NewtonState(Weights: weights, Diagram: diagram, Residual: residual, DualObjective: dual, TransportEnergy: transport, Converged: false, NewtonIterations: 0, StepHalvings: 0, RebuildCount: 0, Fault: Option<Error>.None, DualSolve: Option<SolveReceipt>.None, WeightGradientNorm: 0.0);
                });
        private CapacityResidual CapacityResidualOf(RestrictedPowerDiagram diagram) {
            double inf = 0.0, l1 = 0.0, l2 = 0.0;
            for (int i = 0; i < siteCount; i++) {
                double deviation = Math.Abs(value: diagram.Cells[index: i].Mass - targetMass);
                inf = Math.Max(val1: inf, val2: deviation);
                l1 += deviation;
                l2 += deviation * deviation;
            }
            return new CapacityResidual(Inf: inf, L1: l1, L2: Math.Sqrt(d: l2), Normalized: inf / Math.Max(val1: targetMass, val2: RhinoMath.SqrtEpsilon));
        }
        // Break lattice regularity ONCE on the converged sites (jitter + relocate aliased sites), lift the result to the
        // surface via Mesh.ClosestMeshPoint, and emit the receipt with the 7a-defaulted fields populated (position/
        // centroid/transport/gradient norms, outer/Lloyd/gradient iteration counts, rebuild/aliased/jittered/relocated
        // counts, normalised Poisson-disk radius). Empty (Aurenhammer-dominated) cells hold their seat.
        private Fin<SampleResult> Finalize(OuterState outer) {
            NewtonState terminal = outer.Capacity;
            double meanSpacing = MeanSpacingOf(points: outer.Sites);
            Regularity broken = BreakRegularity(currentSites: outer.Sites, meanSpacing: meanSpacing);
            Seq<Point3d> lifted = LiftToSurface(currentSites: broken.Sites);
            double centroidShift = PairwiseShift(from: lifted, to: sites);
            (double weightMin, double weightMax, double weightMean) = WeightStats(weights: terminal.Weights);
            double normalizedPoissonRadius = NormalizedPoissonRadiusOf(points: lifted);
            RestrictedPowerReceipt diagramReceipt = terminal.Diagram.Receipt;
            PowerCellFragmentFacts fragments = new(
                SiteCount: siteCount,
                FragmentCount: diagramReceipt.FragmentCount,
                FacetCount: diagramReceipt.NeighborFacetCount,
                EmptyCellCount: diagramReceipt.EmptyCellCount,
                TotalMass: terminal.Diagram.Cells.AsIterable().Fold(initialState: 0.0, f: static (acc, cell) => acc + cell.Mass),
                MassMin: terminal.Diagram.Cells.AsIterable().Map(static cell => cell.Mass).Fold(initialState: double.PositiveInfinity, f: static (min, mass) => Math.Min(val1: min, val2: mass)) switch { double m when double.IsPositiveInfinity(d: m) => 0.0, double m => m },
                MassMax: terminal.Diagram.Cells.AsIterable().Map(static cell => cell.Mass).Fold(initialState: 0.0, f: static (max, mass) => Math.Max(val1: max, val2: mass)),
                IntegrationResidual: diagramReceipt.IntegrationResidual);
            PowerCcvtReceipt receipt = new(
                SiteCount: siteCount, TargetMass: targetMass, ActualMassMin: fragments.MassMin, ActualMassMax: fragments.MassMax,
                CapacityResidualInf: terminal.Residual.Inf, CapacityResidualL1: terminal.Residual.L1, CapacityResidualL2: terminal.Residual.L2, CapacityResidualNormalized: terminal.Residual.Normalized,
                OuterIterations: Math.Max(val1: 1, val2: outer.OuterIterations), LloydIterations: outer.LloydIterations, GradientIterations: outer.GradientIterations, DualNewtonIterations: terminal.NewtonIterations,
                WeightMin: weightMin, WeightMax: weightMax, WeightMean: weightMean, TransportEnergy: terminal.TransportEnergy, TransportEnergyDelta: outer.TransportEnergyDelta,
                DualObjective: terminal.DualObjective, CentroidShift: centroidShift, PositionGradientNorm: outer.PositionGradientNorm, WeightGradientNorm: terminal.WeightGradientNorm,
                EmptyCellCount: diagramReceipt.EmptyCellCount, StepHalvingCount: outer.StepHalvings, RebuildCount: outer.RebuildCount, AliasedSiteCount: broken.AliasedCount, JitteredSiteCount: broken.JitteredCount, RelocatedSiteCount: broken.RelocatedCount,
                NormalizedPoissonRadius: normalizedPoissonRadius, PlanarityDeviation: planarityDeviation, Gauge: kind.Gauge, Stop: outer.Converged ? PowerCcvtStopKind.Converged : PowerCcvtStopKind.StoppedWithoutConvergence,
                Fragments: fragments, DualSolve: terminal.DualSolve, Spectrum: Option<MeshSamplingSpectrumReceipt>.None);
            return receipt.IsValid
                ? Fin.Succ(new SampleResult(
                    Points: lifted,
                    Mass: Option<Arr<double>>.None,
                    Receipt: ReceiptOf(
                        attempted: siteCount,
                        emitted: lifted,
                        rejected: diagramReceipt.EmptyCellCount,
                        candidates: Some(siteCount),
                        iterations: Some(outer.OuterIterations),
                        stop: lifted.IsEmpty ? SampleStopKind.AllRejected : lifted.Count < kind.Count.Value ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed,
                        status: diagramReceipt.EmptyCellCount > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted,
                        densityError: Option<double>.None,
                        algorithm: Some(AlgorithmFacts(
                            kind: SampleAlgorithmKind.ContinuousPowerCcvt,
                            seed: Some(kind.Seed),
                            targetCount: Some(kind.Count.Value),
                            capacityResidual: Some(terminal.Residual.Inf),
                            capacityResidualValidated: terminal.Converged,
                            transportAssignmentValidated: !lifted.IsEmpty && terminal.DualSolve.Map(static solve => solve.IsUsable).IfNone(noneValue: false),
                            powerCcvt: Some(receipt))))))
                : Fin.Fail<SampleResult>(key.InvalidResult());
        }
        // Lift each converged site to the surface via Mesh.ClosestMeshPoint; a missed/invalid projection holds the
        // pre-lift in-plane site (the canonical plane already lies near a low-curvature patch).
        private Seq<Point3d> LiftToSurface(Seq<Point3d> currentSites) =>
            toSeq(currentSites.AsIterable().Map(site => {
                MeshPoint meshPoint = domain.Native.ClosestMeshPoint(testPoint: site, maximumDistance: searchDistance);
                return meshPoint is not null && meshPoint.Point.IsValid ? meshPoint.Point : site;
            }));
        // Break lattice regularity ONCE: a site closer than RegularityScale * meanSpacing to any earlier site is aliased.
        // Each aliased site is deterministically jittered by a Gaussian-magnitude offset (JitterVariance * MagnitudeScale *
        // meanSpacing, salt-seeded); the first RelocateFraction of the aliased set is additionally relocated by a full
        // mean-spacing step. The jitter/relocate offsets are deterministic in kind.Seed so the break is replayable.
        private Regularity BreakRegularity(Seq<Point3d> currentSites, double meanSpacing) {
            if (currentSites.Count < 2 || meanSpacing <= RhinoMath.SqrtEpsilon)
                return new Regularity(Sites: currentSites, AliasedCount: 0, JitteredCount: 0, RelocatedCount: 0);
            double aliasRadius = kind.RegularityScale.Value * meanSpacing;
            double jitterMagnitude = kind.JitterVariance.Value * kind.MagnitudeScale.Value * meanSpacing;
            int count = currentSites.Count;
            bool[] aliased = [.. Enumerable.Range(start: 0, count: count)
                .Select(i => Enumerable.Range(start: 0, count: i).Any(j => currentSites[index: i].DistanceTo(other: currentSites[index: j]) < aliasRadius))];
            int aliasedCount = aliased.Count(static flag => flag);
            int relocateBudget = Math.Min(val1: aliasedCount, val2: (int)Math.Floor(d: kind.RelocateFraction.Value * aliasedCount));
            int relocated = 0;
            Point3d[] moved = new Point3d[count];
            for (int i = 0; i < count; i++) {
                bool relocate = aliased[i] && relocated < relocateBudget;
                moved[i] = aliased[i] ? currentSites[index: i] + JitterOffset(site: currentSites[index: i], salt: i, magnitude: relocate ? jitterMagnitude + meanSpacing : jitterMagnitude) : currentSites[index: i];
                if (relocate) relocated++;
            }
            return new Regularity(Sites: toSeq(moved), AliasedCount: aliasedCount, JitteredCount: aliasedCount, RelocatedCount: relocated);
        }
        // Deterministic planar jitter offset: a Box-Muller Gaussian radius (seed-explicit, in-plane via the canonical XY
        // basis of the site neighbourhood) scaled by the magnitude. The salt threads the site index so each aliased site
        // draws an independent but replayable offset.
        private Vector3d JitterOffset(Point3d site, int salt, double magnitude) {
            double u1 = UnitInterval(point: site, salt: (salt * 4) + 1, seed: kind.Seed);
            double u2 = UnitInterval(point: site, salt: (salt * 4) + 2, seed: kind.Seed);
            double radius = magnitude * Math.Sqrt(d: Math.Max(val1: 0.0, val2: -2.0 * Math.Log(d: u1)));
            double angle = RhinoMath.TwoPI * u2;
            return new Vector3d(x: radius * Math.Cos(d: angle), y: radius * Math.Sin(a: angle), z: 0.0);
        }
        private static (double Min, double Max, double Mean) WeightStats(Arr<double> weights) =>
            weights.Count == 0
                ? (Min: 0.0, Max: 0.0, Mean: 0.0)
                : (Min: weights.AsIterable().Fold(initialState: double.PositiveInfinity, f: static (min, value) => Math.Min(val1: min, val2: value)),
                   Max: weights.AsIterable().Fold(initialState: double.NegativeInfinity, f: static (max, value) => Math.Max(val1: max, val2: value)),
                   Mean: weights.AsIterable().Fold(initialState: 0.0, f: static (acc, value) => acc + value) / weights.Count);
        // Normalised minimum-pair Poisson-disk radius: r_min / r_ref where r_ref = sqrt(2 * area / (sqrt(3) * n)) is the
        // hexagonal-packing reference spacing; >0 and <=1 by construction (a saturated lattice approaches 1).
        private double NormalizedPoissonRadiusOf(Seq<Point3d> points) {
            if (points.Count < 2) return 0.0;
            double minSpacing = double.PositiveInfinity;
            for (int i = 0; i < points.Count - 1; i++)
                for (int j = i + 1; j < points.Count; j++)
                    minSpacing = Math.Min(val1: minSpacing, val2: points[index: i].DistanceTo(other: points[index: j]));
            double reference = Math.Sqrt(d: 2.0 * totalMass / (Math.Sqrt(d: 3.0) * Math.Max(val1: 1, val2: points.Count)));
            return RhinoMath.IsValidDouble(x: minSpacing) && RhinoMath.IsValidDouble(x: reference) && reference > RhinoMath.SqrtEpsilon
                ? Math.Clamp(value: minSpacing / reference, min: 0.0, max: 1.0)
                : 0.0;
        }
        [StructLayout(LayoutKind.Auto)] private readonly record struct CapacityResidual(double Inf, double L1, double L2, double Normalized);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct NewtonState(
            Arr<double> Weights, RestrictedPowerDiagram Diagram, CapacityResidual Residual, double DualObjective, double TransportEnergy,
            bool Converged, int NewtonIterations, int StepHalvings, int RebuildCount, Option<Error> Fault, Option<SolveReceipt> DualSolve, double WeightGradientNorm);
        // The two-phase outer-loop state the Schedule advances: the live in-plane sites, the capacity-converged Newton
        // state at those sites, the accumulated outer/Lloyd/gradient/halving/rebuild counters, the latest position-gradient
        // norm and centroid shift, the last transport-energy delta, the joint Converged flag, and the typed fault arm.
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct OuterState(
            Seq<Point3d> Sites, NewtonState Capacity, int OuterIterations, int LloydIterations, int GradientIterations,
            int StepHalvings, int RebuildCount, double PositionGradientNorm, double CentroidShift, double TransportEnergyDelta,
            bool Converged, Option<Error> Fault) {
            internal static OuterState Of(Seq<Point3d> sites, NewtonState capacity) => new(
                Sites: sites, Capacity: capacity, OuterIterations: 0, LloydIterations: 0, GradientIterations: 0,
                StepHalvings: capacity.StepHalvings, RebuildCount: capacity.RebuildCount, PositionGradientNorm: 0.0, CentroidShift: 0.0,
                TransportEnergyDelta: 0.0, Converged: false, Fault: Option<Error>.None);
        }
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct SiteMotion(Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm);
        [StructLayout(LayoutKind.Auto)]
        private readonly record struct Regularity(Seq<Point3d> Sites, int AliasedCount, int JitteredCount, int RelocatedCount);
    }
    private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<SampleCandidate> candidates, bool admitsPoisson, Option<(int Dimensions, double Measure)> domainMeasure, Context context, Op key) =>
        from selection in kind switch {
            SampleKind.PoissonDiskCase pd when admitsPoisson => PoissonDiskSelection(candidates: candidates, radius: pd.Radius.Value, attempts: pd.Attempts.Value, seed: pd.Seed, key: key),
            SampleKind.FarthestCase fp => SelectionOf(kind: kind, candidates: candidates, indices: FarthestIndices(candidates: candidates, count: fp.Count.Value), key: key),
            SampleKind.OptimizeCase fpo => SelectionOf(kind: kind, candidates: candidates, indices: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value), key: key),
            SampleKind.LloydCase lloyd => RelaxationSample(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value, capacity: Option<int>.None, key: key).Bind(indices => SelectionOf(kind: kind, candidates: candidates, indices: indices, key: key)),
            SampleKind.CapacityCase ccvt => CapacityCvtSelection(candidates: candidates, count: ccvt.Count.Value, limit: ccvt.Limit.Value, iterations: ccvt.Iterations.Value, tolerance: ccvt.Tolerance.Value, key: key),
            SampleKind.ScalarDensityCase density => DensitySelection(candidates: candidates, density: density.Density, count: density.Count.Value, minSpacing: VariableDensitySpacing(candidates: candidates, count: density.Count.Value), algorithm: SampleAlgorithmKind.VariableDensityPoisson, context: context, key: key),
            SampleKind.AdaptiveCase adaptive => DensitySelection(candidates: candidates, density: adaptive.Density, count: adaptive.Count.Value, minSpacing: adaptive.MinSpacing.Value, algorithm: SampleAlgorithmKind.VariableDensityPoisson, context: context, key: key),
            SampleKind.SampleEliminationCase elimination => SampleElimination(candidates: candidates, count: elimination.Count.Value, alpha: elimination.Alpha.Value, beta: elimination.Beta.Value, gamma: elimination.Gamma.Value, seed: elimination.Seed, domainMeasure: domainMeasure, key: key)
                .Bind(result => SelectionOf(candidates: candidates, indices: result.Indices, algorithm: Some(result.Algorithm), key: key)),
            SampleKind.DworkVariableDensityCase dwork => DworkVariableDensitySelection(candidates: candidates, radius: dwork.Radius, count: dwork.Count.Value, minRadius: dwork.MinRadius.Value, attempts: dwork.Attempts.Value, seed: dwork.Seed, context: context, key: key),
            SampleKind.PoissonDiskCase pd => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: pd.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }
        let sampled = toSeq(selection.Points)
        let rejected = selection.DensityRejected.IfNone(Math.Max(val1: 0, val2: candidates.Count - selection.Points.Length))
        let capacityLimited = selection.Algorithm.Map(static receipt => receipt.Kind.Equals(SampleAlgorithmKind.CapacityLimitedLloydCandidate) && !receipt.CapacityResidualValidated).IfNone(noneValue: false)
        select new SampleResult(
            Points: sampled,
            Mass: selection.Mass,
            Receipt: ReceiptOf(
                attempted: candidates.Count, emitted: sampled, rejected: rejected, candidates: Some(candidates.Count), iterations: kind.Request.Iterations,
                stop: sampled.Count <= 0 ? SampleStopKind.AllRejected : capacityLimited ? SampleStopKind.CapacityLimited : kind.Request.Count.Map(requested => sampled.Count < requested ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed).IfNone(SampleStopKind.Completed),
                status: selection.DensityRejected.Map(static rejectedCount => rejectedCount > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted).IfNone(SampleDomainStatus.CandidateAccepted),
                densityError: kind.DensityError(emitted: sampled.Count), densityAccepted: selection.DensityAccepted, densityRejected: selection.DensityRejected, algorithm: selection.Algorithm));
    private static Fin<SampleSelection> SelectionOf(SampleKind kind, Seq<SampleCandidate> candidates, int[] indices, Op key, Option<double> radius = default) =>
        SelectionOf(candidates: candidates, indices: indices, algorithm: Some(AlgorithmFacts(kind: kind.Request.Algorithm, targetCount: kind.Request.Count, radius: radius)), key: key);
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
                .Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0, Sum: 0.0, Count: 0), f: static (acc, distance) => (Min: Math.Min(val1: acc.Min, val2: distance), Max: Math.Max(val1: acc.Max, val2: distance), Sum: acc.Sum + distance, Count: acc.Count + 1)) switch { { Count: > 0 } stats => (Some(stats.Min), Some(stats.Sum / stats.Count), Some(stats.Max)), _ => (Option<double>.None, Option<double>.None, Option<double>.None), }) switch {
                    (Option<double> min, Option<double> mean, Option<double> max) => new SampleReceipt(Attempted: attempted, Emitted: emitted.Count, Rejected: rejected, CandidateCount: candidates, MinSpacing: min, MeanSpacing: mean, MaxSpacing: max, DensityError: densityError, DensityAccepted: densityAccepted, DensityRejected: densityRejected, Iterations: iterations, Stop: stop, DomainStatus: status, Algorithm: algorithm),
                };
    private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, Op key) =>
        CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: mass.Count, key: key);
    private static SampleAlgorithmReceipt AlgorithmFacts(SampleAlgorithmKind kind, Option<int> seed = default, Option<int> targetCount = default, Option<int> oversampleCount = default, Option<int> oversampleFactor = default, Option<double> alpha = default, Option<double> beta = default, Option<double> gamma = default, Option<double> radius = default, Option<double> weightLimitRadius = default, Option<int> eliminated = default, Option<int> neighborUpdates = default, bool maximalCoverageGuaranteed = false, bool capacityResidualValidated = false, bool transportAssignmentValidated = false, bool meshSpectrumValidated = false, Option<int> attempts = default, Option<int> activePops = default, Option<int> rejectedTooClose = default, Option<int> rejectedDomain = default, Option<double> densityMin = default, Option<double> densityMax = default, Option<double> localRadiusMin = default, Option<double> localRadiusMax = default, Option<double> capacityResidual = default, Option<MeshSamplingSpectrumReceipt> spectrum = default, Option<DworkReceipt> dwork = default, Option<int> capacityAssignedCandidates = default, Option<int> capacityUnassignedCandidates = default, Option<PowerCcvtReceipt> powerCcvt = default) =>
        new(Kind: kind, Seed: seed, TargetCount: targetCount, OversampleCount: oversampleCount, OversampleFactor: oversampleFactor, Alpha: alpha, Beta: beta, Gamma: gamma, Radius: radius, WeightLimitRadius: weightLimitRadius, Eliminated: eliminated, NeighborUpdates: neighborUpdates, MaximalCoverageGuaranteed: maximalCoverageGuaranteed, CapacityResidualValidated: capacityResidualValidated, TransportAssignmentValidated: transportAssignmentValidated, MeshSpectrumValidated: meshSpectrumValidated, Attempts: attempts, ActivePops: activePops, RejectedTooClose: rejectedTooClose, RejectedDomain: rejectedDomain, DensityMin: densityMin, DensityMax: densityMax, LocalRadiusMin: localRadiusMin, LocalRadiusMax: localRadiusMax, CapacityResidual: capacityResidual, Spectrum: spectrum, Dwork: dwork, CapacityAssignedCandidates: capacityAssignedCandidates, CapacityUnassignedCandidates: capacityUnassignedCandidates, PowerCcvt: powerCcvt);
    private static Fin<SampleSelection> DensitySelection(Seq<SampleCandidate> candidates, ScalarField density, int count, double minSpacing, SampleAlgorithmKind algorithm, Context context, Op key) {
        double[] weights = new double[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, MinWeight: double.PositiveInfinity, MaxWeight: 0.0)),
            f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => key.AcceptValidated<PositiveMagnitude>(candidate: value).IsSucc
                    ? key.AcceptValue(value: value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => { weights[i] = valid; return (Accepted: current.Accepted + 1, current.Rejected, MinWeight: Math.Min(val1: current.MinWeight, val2: valid), MaxWeight: Math.Max(val1: current.MaxWeight, val2: valid)); })
                    : Fin.Succ((current.Accepted, Rejected: current.Rejected + 1, current.MinWeight, current.MaxWeight)))))
            .Bind(stats => stats.Accepted > 0 ? PrioritySelection(candidates: candidates, weights: weights, count: count, minSpacing: minSpacing, minWeight: stats.MinWeight, maxWeight: stats.MaxWeight, accepted: stats.Accepted, rejected: stats.Rejected, algorithm: algorithm, key: key) : Fin.Fail<SampleSelection>(key.InvalidResult()));
    }
    private static Fin<SampleSelection> PrioritySelection(Seq<SampleCandidate> candidates, double[] weights, int count, double minSpacing, double minWeight, double maxWeight, int accepted, int rejected, SampleAlgorithmKind algorithm, Op key) {
        List<(Point3d Point, double Radius)> chosen = []; List<double> mass = [];
        double localMin = double.PositiveInfinity;
        double localMax = 0.0;
        using IEnumerator<int> ordered = Enumerable.Range(start: 0, count: candidates.Count)
            .Where(i => weights[i] > 0.0)
            .OrderBy(i => -Math.Log(d: UnitInterval(point: candidates[index: i].Point, salt: 0)) / weights[i])
            .GetEnumerator();
        while (chosen.Count < count && ordered.MoveNext()) {
            int index = ordered.Current;
            Point3d candidate = candidates[index: index].Point;
            double local = minSpacing / Math.Sqrt(d: Math.Max(val1: weights[index] / Math.Max(val1: maxWeight, val2: RhinoMath.SqrtEpsilon), val2: RhinoMath.SqrtEpsilon));
            localMin = Math.Min(val1: localMin, val2: local);
            localMax = Math.Max(val1: localMax, val2: local);
            if (chosen.TrueForAll(existing => candidate.DistanceTo(other: existing.Point) >= Math.Max(val1: existing.Radius, val2: local))) { chosen.Add(item: (candidate, local)); mass.Add(item: weights[index]); }
        }
        return NormalizeMass(mass: toSeq(mass), key: key)
            .Map(normalized => new SampleSelection(Points: [.. chosen.Select(static sample => sample.Point)], Mass: Some(normalized), DensityAccepted: Some(accepted), DensityRejected: Some(rejected), Algorithm: Some(AlgorithmFacts(kind: algorithm, targetCount: Some(count), densityMin: Some(minWeight), densityMax: Some(maxWeight), localRadiusMin: Some(localMin), localRadiusMax: Some(localMax)))));
    }
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCell(long X, long Y, long Z);
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkSurfacePoint(Point3d Point, double Radius);
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkTriangle(Point3d A, Point3d B, Point3d C, double CumulativeArea);
    [StructLayout(LayoutKind.Auto)] private readonly record struct DworkCandidate(int Index, double Radius);
    private static Fin<SampleSelection> DworkVariableDensityMeshSelection(MeshSpace domain, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
        using Mesh mesh = domain.Native.DuplicateMesh();
        if (mesh.Faces.QuadCount > 0 && !mesh.Faces.ConvertQuadsToTriangles()) return Fin.Fail<SampleSelection>(key.InvalidResult());
        _ = mesh.FaceNormals.ComputeFaceNormals();
        return new DworkMeshRun(mesh: mesh, radius: radius, count: count, minRadius: minRadius, attempts: attempts, seed: seed, context: context, key: key).Run();
    }
    private static Fin<SampleSelection> DworkVariableDensitySelection(Seq<SampleCandidate> candidates, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
        DworkCandidate[] admitted = new DworkCandidate[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, MinRadius: double.PositiveInfinity, MaxRadius: 0.0)),
            f: (state, i) => state.Bind(current => radius.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => key.AcceptValidated<PositiveMagnitude>(candidate: value).IsSucc
                    ? key.AcceptValue(value: Math.Max(val1: minRadius, val2: value)).Map(local => {
                        admitted[current.Accepted] = new DworkCandidate(Index: i, Radius: local);
                        return (Accepted: current.Accepted + 1, current.Rejected, MinRadius: Math.Min(val1: current.MinRadius, val2: local), MaxRadius: Math.Max(val1: current.MaxRadius, val2: local));
                    })
                    : Fin.Succ((current.Accepted, Rejected: current.Rejected + 1, current.MinRadius, current.MaxRadius)))))
            .Bind(stats => stats.Accepted > 0
                ? DworkVariableDensitySelection(candidates: candidates, admitted: [.. admitted.Take(count: stats.Accepted)], count: count, attempts: attempts, seed: seed, radiusMin: stats.MinRadius, radiusMax: stats.MaxRadius, rejectedDomain: stats.Rejected, key: key)
                : Fin.Fail<SampleSelection>(key.InvalidResult()));
    }
    private static Fin<SampleSelection> DworkVariableDensitySelection(Seq<SampleCandidate> candidates, DworkCandidate[] admitted, int count, int attempts, int seed, double radiusMin, double radiusMax, int rejectedDomain, Op key) {
        DworkCandidate[] ordered = [.. admitted.OrderBy(item => CandidateOrderKey(point: candidates[index: item.Index].Point, seed: seed))];
        // Spatial hash over the admitted candidate cloud: each ordered candidate scatters into the cell of its point so
        // the per-parent annulus band [r, 2r] resolves through a bounded ring query instead of rejection sampling the
        // whole cloud. The cell pitch is r_min/sqrt(3) (the variable-density Bridson covering pitch), so an [r, 2r]
        // band is reached by ceil(2r / cellSize) shells regardless of the parent's local radius.
        double cellSize = Math.Max(val1: radiusMin / Math.Sqrt(d: 3.0), val2: RhinoMath.SqrtEpsilon);
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
            int shells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: radiusMax) / cellSize));
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
        int activePops = 0;
        int tooClose = 0;
        int outside = 0;
        while (active.Count > 0 && chosen.Count < count) {
            int activeOffset = (int)(CandidateOrderKey(point: candidates[index: active[0].Index].Point, seed: seed + activePops) % (ulong)active.Count);
            DworkCandidate parent = active[activeOffset];
            Point3d parentPoint = candidates[index: parent.Index].Point;
            DworkCell parentCell = CellOf(point: parentPoint);
            int bandShells = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: 2.0 * parent.Radius / cellSize));
            // Gather the cloud candidates whose cells intersect the parent's [r, 2r] band, then exact-test the annulus.
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
                DworkCandidate candidate = annulus[(int)(CandidateOrderKey(point: parentPoint, seed: seed + activePops + attempt + 1) % (ulong)annulus.Count)];
                if (chosen.Exists(item => item.Index == candidate.Index)) { tooClose++; continue; }
                if (Conflicts(candidate: candidate)) { tooClose++; continue; }
                chosen.Add(item: candidate);
                Record(candidate: candidate);
                active.Add(item: candidate);
                accepted = true;
            }
            if (!accepted && annulus.Count == 0) outside++;
            if (!accepted) active.RemoveAt(index: activeOffset);
            activePops++;
        }
        DworkReceipt dwork = new(
            Domain: DworkSamplingDomain.CandidateSet,
            RMin: radiusMin,
            BackgroundCellSize: Some(cellSize),
            BackgroundGridCells: Some(cloudGrid.Count),
            AttemptsPerActive: attempts,
            GeneratedCandidates: chosen.Count + tooClose + outside + rejectedDomain,
            ActivePops: activePops,
            RejectedTooClose: tooClose,
            RejectedDomain: rejectedDomain + outside,
            LocalRadiusMin: radiusMin,
            LocalRadiusMax: radiusMax);
        SampleAlgorithmReceipt algorithm = AlgorithmFacts(kind: SampleAlgorithmKind.DworkVariableDensity, seed: Some(seed), targetCount: Some(count), oversampleCount: Some(admitted.Length), attempts: Some(attempts), activePops: Some(activePops), rejectedTooClose: Some(tooClose), rejectedDomain: Some(rejectedDomain + outside), localRadiusMin: Some(radiusMin), localRadiusMax: Some(radiusMax), dwork: Some(dwork));
        return SelectionOf(candidates: candidates, indices: [.. chosen.Select(static item => item.Index)], algorithm: Some(algorithm), key: key);
    }
    private sealed class DworkMeshRun {
        private readonly Mesh mesh;
        private readonly ScalarField radius;
        private readonly int count;
        private readonly double minRadius;
        private readonly int attempts;
        private readonly int seed;
        private readonly Context context;
        private readonly Op key;
        private readonly double cellSize;
        private readonly List<DworkSurfacePoint> chosen = [];
        private readonly List<int> active = [];
        private readonly Dictionary<DworkCell, List<int>> grid = [];
        private DworkTriangle[] triangles = [];
        private Point3d gridOrigin = Point3d.Origin;
        private double totalArea;
        private double radiusMin = double.PositiveInfinity;
        private double radiusMax;
        private int proposals;
        private int activePops;
        private int rejectedTooClose;
        private int rejectedDomain;
        internal DworkMeshRun(Mesh mesh, ScalarField radius, int count, double minRadius, int attempts, int seed, Context context, Op key) {
            this.mesh = mesh;
            this.radius = radius;
            this.count = count;
            this.minRadius = minRadius;
            this.attempts = attempts;
            this.seed = seed;
            this.context = context;
            this.key = key;
            cellSize = minRadius / Math.Sqrt(d: 3.0);
        }
        internal Fin<SampleSelection> Run() =>
            BuildTriangles().Bind(_ => {
                Option<DworkSurfacePoint> first = Option<DworkSurfacePoint>.None;
                for (int attempt = 0; attempt < attempts && first.IsNone; attempt++) {
                    proposals++;
                    first = SurfaceSample(salt: attempt * 3).Bind(RadiusAt);
                    if (first.IsNone) rejectedDomain++;
                }
                return first.Match(
                    Some: sample => {
                        Add(sample: sample);
                        while (active.Count > 0 && chosen.Count < count) {
                            int activeOffset = (int)(CandidateOrderKey(point: chosen[index: active[0]].Point, seed: seed + activePops) % (ulong)active.Count);
                            int parentIndex = active[index: activeOffset];
                            bool accepted = false;
                            for (int attempt = 0; attempt < attempts && !accepted; attempt++) {
                                proposals++;
                                Option<DworkSurfacePoint> candidate = AnnulusCandidate(parent: chosen[index: parentIndex], attempt: attempt);
                                accepted = candidate.Match(
                                    Some: value => {
                                        if (Conflicts(candidate: value)) {
                                            rejectedTooClose++;
                                            return false;
                                        }
                                        Add(sample: value);
                                        return true;
                                    },
                                    None: () => {
                                        rejectedDomain++;
                                        return false;
                                    });
                            }
                            if (!accepted) active.RemoveAt(index: activeOffset);
                            activePops++;
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
                if (!RhinoMath.IsValidDouble(x: area) || area <= RhinoMath.SqrtEpsilon) continue;
                cumulative += area;
                built.Add(item: new DworkTriangle(A: a, B: b, C: c, CumulativeArea: cumulative));
            }
            BoundingBox bounds = mesh.GetBoundingBox(accurate: true);
            triangles = [.. built];
            totalArea = cumulative;
            gridOrigin = bounds.IsValid ? bounds.Min : Point3d.Origin;
            return triangles.Length > 0 && RhinoMath.IsValidDouble(x: totalArea) && totalArea > RhinoMath.SqrtEpsilon && bounds.IsValid
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(key.InvalidResult());
        }
        private Option<Point3d> SurfaceSample(int salt) {
            double target = UnitInterval(point: Point3d.Origin, salt: salt, seed: seed) * totalArea;
            DworkTriangle triangle = triangles[^1];
            for (int i = 0; i < triangles.Length; i++)
                if (target <= triangles[i].CumulativeArea) {
                    triangle = triangles[i];
                    break;
                }
            double u = Math.Sqrt(d: UnitInterval(point: triangle.A, salt: salt + 1, seed: seed));
            double v = UnitInterval(point: triangle.B, salt: salt + 2, seed: seed);
            double wa = 1.0 - u;
            double wb = u * (1.0 - v);
            double wc = u * v;
            Point3d sample = new(x: (wa * triangle.A.X) + (wb * triangle.B.X) + (wc * triangle.C.X), y: (wa * triangle.A.Y) + (wb * triangle.B.Y) + (wc * triangle.C.Y), z: (wa * triangle.A.Z) + (wb * triangle.B.Z) + (wc * triangle.C.Z));
            return sample.IsValid ? Some(sample) : Option<Point3d>.None;
        }
        private Option<DworkSurfacePoint> RadiusAt(Point3d point) =>
            radius.SampleScalar(sample: point, context: context, key: key).Match(
                Succ: value => key.AcceptValidated<PositiveMagnitude>(candidate: value).IsSucc ? Some(new DworkSurfacePoint(Point: point, Radius: Math.Max(val1: minRadius, val2: value))) : Option<DworkSurfacePoint>.None,
                Fail: _ => Option<DworkSurfacePoint>.None);
        private Option<DworkSurfacePoint> AnnulusCandidate(DworkSurfacePoint parent, int attempt) {
            double angle = RhinoMath.TwoPI * UnitInterval(point: parent.Point, salt: (activePops * attempts * 4) + (attempt * 4) + 7, seed: seed);
            double distance = parent.Radius * Math.Sqrt(d: 1.0 + (3.0 * UnitInterval(point: parent.Point, salt: (activePops * attempts * 4) + (attempt * 4) + 8, seed: seed)));
            Vector3d normal = NormalAt(point: parent.Point);
            Vector3d tangent = VectorFrame.SeedPerpendicular(axis: normal);
            Vector3d bitangent = Vector3d.CrossProduct(a: normal, b: tangent);
            if (!bitangent.Unitize()) return Option<DworkSurfacePoint>.None;
            Point3d raw = parent.Point + (distance * ((Math.Cos(d: angle) * tangent) + (Math.Sin(a: angle) * bitangent)));
            MeshPoint? hit = mesh.ClosestMeshPoint(testPoint: raw, maximumDistance: distance + radiusMax + context.Absolute.Value);
            if (hit is null || hit.FaceIndex < 0 || hit.FaceIndex >= mesh.Faces.Count) return Option<DworkSurfacePoint>.None;
            Point3d projected = hit.Point;
            double projectedDistance = projected.DistanceTo(other: parent.Point);
            return projectedDistance >= parent.Radius && projectedDistance <= (2.0 * parent.Radius) + context.Absolute.Value
                ? RadiusAt(point: projected)
                : Option<DworkSurfacePoint>.None;
        }
        private Vector3d NormalAt(Point3d point) {
            MeshPoint? hit = mesh.ClosestMeshPoint(testPoint: point, maximumDistance: Math.Max(val1: minRadius, val2: context.Absolute.Value));
            Vector3d normal = hit is { FaceIndex: >= 0 } && hit.FaceIndex < mesh.FaceNormals.Count ? mesh.FaceNormals[index: hit.FaceIndex] : Vector3d.ZAxis;
            return normal.IsValid && !normal.IsTiny(context.Absolute.Value) && normal.Unitize() ? normal : Vector3d.ZAxis;
        }
        private bool Conflicts(DworkSurfacePoint candidate) {
            int range = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Max(val1: candidate.Radius, val2: radiusMax) / cellSize));
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
            radiusMin = Math.Min(val1: radiusMin, val2: sample.Radius);
            radiusMax = Math.Max(val1: radiusMax, val2: sample.Radius);
            DworkCell cell = CellOf(point: sample.Point);
            if (!grid.TryGetValue(key: cell, value: out List<int>? bucket)) {
                bucket = [];
                grid.Add(key: cell, value: bucket);
            }
            bucket.Add(item: index);
        }
        private DworkCell CellOf(Point3d point) =>
            new(
                X: (long)Math.Floor(d: (point.X - gridOrigin.X) / cellSize),
                Y: (long)Math.Floor(d: (point.Y - gridOrigin.Y) / cellSize),
                Z: (long)Math.Floor(d: (point.Z - gridOrigin.Z) / cellSize));
        private Fin<SampleSelection> Selection() {
            DworkReceipt dwork = new(
                Domain: DworkSamplingDomain.ContinuousMesh,
                RMin: minRadius,
                BackgroundCellSize: Some(cellSize),
                BackgroundGridCells: Some(grid.Count),
                AttemptsPerActive: attempts,
                GeneratedCandidates: proposals,
                ActivePops: activePops,
                RejectedTooClose: rejectedTooClose,
                RejectedDomain: rejectedDomain,
                LocalRadiusMin: radiusMin,
                LocalRadiusMax: radiusMax);
            SampleAlgorithmReceipt algorithm = AlgorithmFacts(kind: SampleAlgorithmKind.DworkVariableDensity, seed: Some(seed), targetCount: Some(count), oversampleCount: Some(proposals), attempts: Some(attempts), activePops: Some(activePops), rejectedTooClose: Some(rejectedTooClose), rejectedDomain: Some(rejectedDomain), localRadiusMin: Some(radiusMin), localRadiusMax: Some(radiusMax), dwork: Some(dwork));
            return Fin.Succ(new SampleSelection(Points: [.. chosen.Select(static sample => sample.Point)], Mass: Option<Arr<double>>.None, DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None, Algorithm: Some(algorithm)));
        }
    }
    private static double VariableDensitySpacing(Seq<SampleCandidate> candidates, int count) {
        (int dimensions, double measure) = BoundingMeasure(candidates: candidates);
        return 0.5 * (dimensions == 3 ? Math.Pow(x: measure / Math.Max(val1: 1, val2: count), y: 1.0 / 3.0) : Math.Sqrt(d: measure / Math.Max(val1: 1, val2: count)));
    }
    private static Fin<SampleSelection> PoissonDiskSelection(Seq<SampleCandidate> candidates, double radius, int attempts, int seed, Op key) {
        if (candidates.IsEmpty || radius <= 0.0 || attempts < 1) return Fin.Fail<SampleSelection>(key.InvalidInput());
        double r2 = radius * radius;
        double r4 = 4.0 * r2;
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count).OrderBy(i => CandidateOrderKey(point: candidates[index: i].Point, seed: seed))];
        List<int> chosen = [order[0]];
        List<int> active = [order[0]];
        int activePops = 0;
        int tooClose = 0;
        int outside = 0;
        while (active.Count > 0) {
            int activeOffset = (int)(CandidateOrderKey(point: candidates[index: active[0]].Point, seed: seed + activePops) % (ulong)active.Count);
            int parent = active[activeOffset];
            bool accepted = false;
            for (int attempt = 0; attempt < attempts && !accepted; attempt++) {
                int candidate = order[(int)(CandidateOrderKey(point: candidates[index: parent].Point, seed: seed + activePops + attempt + 1) % (ulong)order.Length)];
                double fromParent = candidates[index: parent].Point.DistanceToSquared(other: candidates[index: candidate].Point);
                if (fromParent < r2 || fromParent > r4) { outside++; continue; }
                bool rejected = chosen.Exists(index => candidates[index: index].Point.DistanceToSquared(other: candidates[index: candidate].Point) < r2);
                if (rejected) { tooClose++; continue; }
                chosen.Add(item: candidate);
                active.Add(item: candidate);
                accepted = true;
            }
            if (!accepted) active.RemoveAt(index: activeOffset);
            activePops++;
        }
        bool maximalCoverage = active.Count == 0;
        SampleAlgorithmReceipt algorithm = AlgorithmFacts(kind: SampleAlgorithmKind.BridsonActiveListPoisson, seed: Some(seed), radius: Some(radius), attempts: Some(attempts), maximalCoverageGuaranteed: maximalCoverage, activePops: Some(activePops), rejectedTooClose: Some(tooClose), rejectedDomain: Some(outside));
        return SelectionOf(candidates: candidates, indices: [.. chosen.Distinct()], algorithm: Some(algorithm), key: key);
    }
    private static Fin<SampleSelection> CapacityCvtSelection(Seq<SampleCandidate> candidates, int count, int limit, int iterations, double tolerance, Op key) =>
        RelaxationSample(candidates: candidates, count: count, iterations: iterations, capacity: Some(limit), key: key).Bind(indices => {
            (double residual, int assigned, int unassigned) = CapacityResidualOf(candidates: candidates, sites: indices, limit: limit);
            SampleAlgorithmReceipt algorithm = AlgorithmFacts(kind: SampleAlgorithmKind.CapacityLimitedLloydCandidate, targetCount: Some(count), capacityResidual: Some(residual), capacityResidualValidated: unassigned == 0 && residual <= tolerance, capacityAssignedCandidates: Some(assigned), capacityUnassignedCandidates: Some(unassigned));
            return SelectionOf(candidates: candidates, indices: indices, algorithm: Some(algorithm), key: key);
        });
    private static (double Residual, int Assigned, int Unassigned) CapacityResidualOf(Seq<SampleCandidate> candidates, int[] sites, int limit) {
        if (candidates.IsEmpty || sites.Length == 0 || limit < 1) return (Residual: 1.0, Assigned: 0, Unassigned: candidates.Count);
        int[] fill = new int[sites.Length];
        int assigned = 0;
        int rejected = 0;
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
    private static Fin<(int[] Indices, SampleAlgorithmReceipt Algorithm)> SampleElimination(Seq<SampleCandidate> candidates, int count, double alpha, double beta, double gamma, int seed, Option<(int Dimensions, double Measure)> domainMeasure, Op key) {
        SampleCandidate[] input = [.. candidates.AsIterable()];
        (int dimensions, double measure) = domainMeasure.IfNone(BoundingMeasure(candidates: candidates));
        double dMax = 2.0 * (dimensions == 3 ? Math.Pow(x: measure / count / (4.0 * Math.Sqrt(d: 2.0)), y: 1.0 / 3.0) : Math.Sqrt(d: measure / count / (2.0 * Math.Sqrt(d: 3.0))));
        double dMin = dMax * (1.0 - Math.Pow(x: (double)count / input.Length, y: gamma)) * beta;
        if (input.Length <= count || count <= 0 || !RhinoMath.IsValidDouble(x: dMax) || dMax <= 0.0 || !RhinoMath.IsValidDouble(x: dMin) || dMin < 0.0) return Fin.Fail<(int[] Indices, SampleAlgorithmReceipt Algorithm)>(key.InvalidInput());
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
        int neighborUpdates = 0;
        int activeCount = input.Length;
        int eliminated = 0;
        while (activeCount > count) {
            int remove = MaxWeightIndex(active: active, weights: weights, input: input, seed: seed);
            active[remove] = false;
            activeCount--;
            eliminated++;
            for (int edge = 0; edge < edges.Length; edge++) {
                int other = edges[edge].Left == remove ? edges[edge].Right : edges[edge].Right == remove ? edges[edge].Left : -1;
                if (other >= 0 && active[other]) {
                    weights[other] -= edges[edge].Weight;
                    neighborUpdates++;
                }
            }
        }
        return Fin.Succ<(int[] Indices, SampleAlgorithmReceipt Algorithm)>((
            Indices: [.. Enumerable.Range(start: 0, count: input.Length).Where(i => active[i]).OrderBy(i => CandidateOrderKey(point: input[i].Point, seed: seed))],
            Algorithm: AlgorithmFacts(kind: SampleAlgorithmKind.YukselWeightedSampleElimination, seed: Some(seed), targetCount: Some(count), oversampleCount: Some(input.Length), oversampleFactor: Some(input.Length / Math.Max(val1: 1, val2: count)), alpha: Some(alpha), beta: Some(beta), gamma: Some(gamma), radius: Some(dMax), weightLimitRadius: Some(dMin), eliminated: Some(eliminated), neighborUpdates: Some(neighborUpdates))));
    }
    private static (int Dimensions, double Measure) BoundingMeasure(Seq<SampleCandidate> candidates) {
        BoundingBox box = new(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        double dx = Math.Max(val1: box.Max.X - box.Min.X, val2: 0.0);
        double dy = Math.Max(val1: box.Max.Y - box.Min.Y, val2: 0.0);
        double dz = Math.Max(val1: box.Max.Z - box.Min.Z, val2: 0.0);
        double volume = dx * dy * dz;
        double area = Math.Max(val1: dx * dy, val2: Math.Max(val1: dx * dz, val2: dy * dz));
        return volume > RhinoMath.SqrtEpsilon
            ? (Dimensions: 3, Measure: volume)
            : (Dimensions: 2, Measure: Math.Max(val1: area, val2: RhinoMath.SqrtEpsilon));
    }
    private static int MaxWeightIndex(bool[] active, double[] weights, SampleCandidate[] input, int seed) =>
        Enumerable.Range(start: 0, count: input.Length)
            .Where(i => active[i])
            .Select(i => (Index: i, Weight: weights[i], Key: CandidateOrderKey(point: input[i].Point, seed: seed)))
            .Aggregate((best, item) => item.Weight > best.Weight || (Math.Abs(value: item.Weight - best.Weight) <= RhinoMath.SqrtEpsilon && item.Key < best.Key) ? item : best)
            .Index;
    private static ulong CandidateOrderKey(Point3d point, int seed = 0) {
        static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
        unchecked {
            ulong unsignedSeed = (uint)seed;
            ulong hash = (unsignedSeed + 0x9E3779B97F4A7C15UL) * 0x9E3779B185EBCA87UL;
            hash ^= Bits(value: point.X) * 0x9E3779B185EBCA87UL;
            hash ^= Bits(value: point.Y) + 0xC2B2AE3D27D4EB4FUL + (hash << 6) + (hash >> 2);
            hash ^= Bits(value: point.Z) + 0x165667B19E3779F9UL + (hash << 6) + (hash >> 2);
            return hash;
        }
    }
    private static double UnitInterval(Point3d point, int salt, int seed = 0) {
        int mixed = unchecked((seed * 16_777_619) + salt);
        double unit = ((CandidateOrderKey(point: point, seed: mixed) >> 11) + 1.0) * (1.0 / 9007199254740992.0);
        return Math.Clamp(value: unit, min: RhinoMath.SqrtEpsilon, max: 1.0 - RhinoMath.SqrtEpsilon);
    }
    private static int[] FarthestIndices(Seq<SampleCandidate> candidates, int count) {
        if (candidates.IsEmpty || count < 1) return [];
        int total = candidates.Count; int actualCount = Math.Min(val1: count, val2: total); int[] chosen = new int[actualCount]; bool[] selected = new bool[total];
        Point3d centroid = toSeq(Enumerable.Range(start: 0, count: total))
            .Fold(initialState: Point3d.Origin, f: (acc, i) => new Point3d(x: acc.X + candidates[index: i].Point.X, y: acc.Y + candidates[index: i].Point.Y, z: acc.Z + candidates[index: i].Point.Z)) switch { Point3d sum => new Point3d(x: sum.X / total, y: sum.Y / total, z: sum.Z / total), };
        chosen[0] = Enumerable.Range(start: 0, count: total)
            .Select(i => (Index: i, Distance: candidates[index: i].Point.DistanceToSquared(other: centroid)))
            .Aggregate((best, item) => item.Distance > best.Distance ? item : best).Index;
        selected[chosen[0]] = true; double[] minDistSq = [.. Enumerable.Range(start: 0, count: total).Select(i => candidates[index: i].Point.DistanceToSquared(other: candidates[index: chosen[0]].Point))];
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
        double bestScore = WorstCoverage(candidates: candidates, chosen: chosen).Distance; bool improved = true;
        for (int iter = 0; iter < iterations && improved; iter++) {
            improved = false;
            int replacement = WorstCoverage(candidates: candidates, chosen: chosen).Index;
            for (int i = 0; i < chosen.Length && !improved; i++) {
                if (chosen.Contains(value: replacement)) continue;
                int previous = chosen[i]; chosen[i] = replacement;
                double score = WorstCoverage(candidates: candidates, chosen: chosen).Distance;
                if (score + RhinoMath.SqrtEpsilon < bestScore) { bestScore = score; improved = true; }
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
        Vector3d[] sums = new Vector3d[sites.Length]; int[] counts = new int[sites.Length]; int[] siteFill = new int[sites.Length];
        PointCloud siteIndex = []; siteIndex.AddRange(points: sites.Select(i => candidates[index: i].Point));
        for (int i = 0; i < total; i++) {
            int closest = capacity.Match(
                Some: limit => {
                    int hit = -1; double best = double.MaxValue;
                    for (int s = 0; s < sites.Length; s++) {
                        if (siteFill[s] >= limit) continue;
                        double distance = candidates[index: i].Point.DistanceToSquared(other: candidates[index: sites[s]].Point);
                        if (distance < best) { best = distance; hit = s; }
                    }
                    return hit;
                },
                None: () => siteIndex.ClosestPoint(testPoint: candidates[index: i].Point));
            if (closest < 0) return Fin.Fail<int[]>(key.InvalidResult());
            siteFill[closest]++; sums[closest] += (Vector3d)candidates[index: i].Point; counts[closest]++;
        }
        PointCloud candidateIndex = []; candidateIndex.AddRange(points: candidates.AsIterable().Select(static candidate => candidate.Point));
        return toSeq(Enumerable.Range(start: 0, count: sites.Length)).Fold(
            initialState: Fin.Succ(sites),
            f: (state, s) => counts[s] <= 0
                ? state
                : state.Bind(active => (candidateIndex.ClosestPoint(testPoint: Point3d.Origin + (sums[s] / counts[s])) switch {
                    int nearest when nearest >= 0 && nearest < candidates.Count => Fin.Succ(nearest),
                    _ => Fin.Fail<int>(key.InvalidResult()),
                })
                    .Map(nearest => {
                        active[s] = nearest;
                        return active;
                    })));
    }
}
