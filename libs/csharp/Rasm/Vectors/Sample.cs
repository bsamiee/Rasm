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
    public sealed record PowerCcvtCase : SampleKind { internal PowerCcvtCase(Dimension count, Dimension iterations, PositiveMagnitude tolerance) { Count = count; Iterations = iterations; Tolerance = tolerance; } public Dimension Count { get; } public Dimension Iterations { get; } public PositiveMagnitude Tolerance { get; } }
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
    public static Fin<SampleKind> PowerCcvt(int count, int iterations = 16, double tolerance = 1.0e-6, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from iter in op.AcceptValidated<Dimension>(candidate: iterations)
               from tol in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               from admitted in Admit(value: new PowerCcvtCase(count: c, iterations: iter, tolerance: tol), key: op)
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
        PowerCcvtCase => Fin.Succ(this),
        _ => Fin.Fail<SampleKind>(key.InvalidInput()),
    };
    internal static Fin<SampleKind> Admit(SampleKind value, Op key) =>
        FieldNabla.NotNull(value: value, key: key).Bind(kind => kind.Admit(key: key));
    internal Fin<SampleResult> Evaluate(ExtractionDomain domain, Context context, Op key) =>
        Admit(key: key).Bind(kind => SampleKernel.Sample(kind: kind, domain: domain, context: context, key: key));
    internal (Option<int> Count, Option<int> Iterations, double MeshScale, bool Density, SampleAlgorithmKind Algorithm) Request => this switch { ExplicitCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.Explicit), PoissonDiskCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.BridsonActiveListPoisson), FarthestCase c => (Some(c.Count.Value), Option<int>.None, 1.0, false, SampleAlgorithmKind.FarthestCandidate), OptimizeCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false, SampleAlgorithmKind.FarthestOptimize), LloydCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false, SampleAlgorithmKind.LloydCandidateRelaxation), CapacityCase c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value, false, SampleAlgorithmKind.CapacityLimitedLloydCandidate), WeightedCase => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.WeightedMassPropagation), ScalarDensityCase c => (Some(c.Count.Value), Option<int>.None, 8.0, true, SampleAlgorithmKind.VariableDensityPoisson), AdaptiveCase c => (Some(c.Count.Value), Option<int>.None, 12.0, true, SampleAlgorithmKind.VariableDensityPoisson), SampleEliminationCase c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value, false, SampleAlgorithmKind.YukselWeightedSampleElimination), DworkVariableDensityCase c => (Some(c.Count.Value), Option<int>.None, 12.0, true, SampleAlgorithmKind.DworkVariableDensity), PowerCcvtCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false, SampleAlgorithmKind.ContinuousPowerCcvt), _ => (Option<int>.None, Option<int>.None, 0.0, false, SampleAlgorithmKind.Explicit) };
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
public readonly record struct DworkReceipt(DworkSamplingDomain Domain, double RMin, Option<double> BackgroundCellSize, Option<int> BackgroundGridCells, int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain, double LocalRadiusMin, double LocalRadiusMax, bool ActiveListAnnulusSampling, bool LocalRadiusConflictChecks, bool DeterministicSeed) {
    public bool CandidateOnly => Domain.Equals(DworkSamplingDomain.CandidateSet);
    public bool ContinuousMesh => Domain.Equals(DworkSamplingDomain.ContinuousMesh);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleAlgorithmReceipt(SampleAlgorithmKind Kind, Option<int> Seed, Option<int> TargetCount, Option<int> OversampleCount, Option<int> OversampleFactor, Option<double> Alpha, Option<double> Beta, Option<double> Gamma, Option<double> Radius, Option<double> WeightLimitRadius, Option<int> Eliminated, Option<int> NeighborUpdates, bool DeterministicCandidateSource, bool EuclideanMetric, bool MaximalCoverageGuaranteed, bool CapacityResidualValidated, bool TransportAssignmentValidated, bool MeshSpectrumValidated, Option<int> Attempts = default, Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default, Option<double> DensityMin = default, Option<double> DensityMax = default, Option<double> LocalRadiusMin = default, Option<double> LocalRadiusMax = default, Option<double> CapacityResidual = default, Option<MeshSamplingSpectrumReceipt> Spectrum = default, Option<DworkReceipt> Dwork = default, Option<int> CapacityAssignedCandidates = default, Option<int> CapacityUnassignedCandidates = default);

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
            return Fin.Fail<SampleResult>(key.Unsupported(geometryType: power.GetType(), outputType: typeof(SampleResult)));
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
    private static SampleAlgorithmReceipt AlgorithmFacts(SampleAlgorithmKind kind, Option<int> seed = default, Option<int> targetCount = default, Option<int> oversampleCount = default, Option<int> oversampleFactor = default, Option<double> alpha = default, Option<double> beta = default, Option<double> gamma = default, Option<double> radius = default, Option<double> weightLimitRadius = default, Option<int> eliminated = default, Option<int> neighborUpdates = default, bool capacityResidualValidated = false, bool transportAssignmentValidated = false, bool meshSpectrumValidated = false, Option<int> attempts = default, Option<int> activePops = default, Option<int> rejectedTooClose = default, Option<int> rejectedDomain = default, Option<double> densityMin = default, Option<double> densityMax = default, Option<double> localRadiusMin = default, Option<double> localRadiusMax = default, Option<double> capacityResidual = default, Option<MeshSamplingSpectrumReceipt> spectrum = default, Option<DworkReceipt> dwork = default, Option<int> capacityAssignedCandidates = default, Option<int> capacityUnassignedCandidates = default) =>
        new(Kind: kind, Seed: seed, TargetCount: targetCount, OversampleCount: oversampleCount, OversampleFactor: oversampleFactor, Alpha: alpha, Beta: beta, Gamma: gamma, Radius: radius, WeightLimitRadius: weightLimitRadius, Eliminated: eliminated, NeighborUpdates: neighborUpdates, DeterministicCandidateSource: true, EuclideanMetric: true, MaximalCoverageGuaranteed: false, CapacityResidualValidated: capacityResidualValidated, TransportAssignmentValidated: transportAssignmentValidated, MeshSpectrumValidated: meshSpectrumValidated, Attempts: attempts, ActivePops: activePops, RejectedTooClose: rejectedTooClose, RejectedDomain: rejectedDomain, DensityMin: densityMin, DensityMax: densityMax, LocalRadiusMin: localRadiusMin, LocalRadiusMax: localRadiusMax, CapacityResidual: capacityResidual, Spectrum: spectrum, Dwork: dwork, CapacityAssignedCandidates: capacityAssignedCandidates, CapacityUnassignedCandidates: capacityUnassignedCandidates);
    private static Fin<SampleSelection> DensitySelection(Seq<SampleCandidate> candidates, ScalarField density, int count, double minSpacing, SampleAlgorithmKind algorithm, Context context, Op key) {
        double[] weights = new double[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, MinWeight: double.PositiveInfinity, MaxWeight: 0.0)),
            f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
                .Bind(value => RhinoMath.IsValidDouble(x: value) && value > 0.0
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
                .Bind(value => RhinoMath.IsValidDouble(x: value) && value > 0.0
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
        List<DworkCandidate> chosen = ordered.Length > 0 ? [ordered[0]] : [];
        List<DworkCandidate> active = ordered.Length > 0 ? [ordered[0]] : [];
        int activePops = 0;
        int tooClose = 0;
        int outside = 0;
        while (active.Count > 0 && chosen.Count < count) {
            int activeOffset = (int)(CandidateOrderKey(point: candidates[index: active[0].Index].Point, seed: seed + activePops) % (ulong)active.Count);
            DworkCandidate parent = active[activeOffset];
            bool accepted = false;
            for (int attempt = 0; attempt < attempts && !accepted; attempt++) {
                DworkCandidate candidate = ordered[(int)(CandidateOrderKey(point: candidates[index: parent.Index].Point, seed: seed + activePops + attempt + 1) % (ulong)ordered.Length)];
                if (chosen.Exists(item => item.Index == candidate.Index)) { tooClose++; continue; }
                double distance = candidates[index: parent.Index].Point.DistanceTo(other: candidates[index: candidate.Index].Point);
                if (distance < parent.Radius || distance > 2.0 * parent.Radius) { outside++; continue; }
                bool conflict = chosen.Exists(item => candidates[index: item.Index].Point.DistanceTo(other: candidates[index: candidate.Index].Point) < Math.Max(val1: item.Radius, val2: candidate.Radius));
                if (conflict) { tooClose++; continue; }
                chosen.Add(item: candidate);
                active.Add(item: candidate);
                accepted = true;
            }
            if (!accepted) active.RemoveAt(index: activeOffset);
            activePops++;
        }
        DworkReceipt dwork = new(
            Domain: DworkSamplingDomain.CandidateSet,
            RMin: radiusMin,
            BackgroundCellSize: Option<double>.None,
            BackgroundGridCells: Option<int>.None,
            AttemptsPerActive: attempts,
            GeneratedCandidates: chosen.Count + tooClose + outside + rejectedDomain,
            ActivePops: activePops,
            RejectedTooClose: tooClose,
            RejectedDomain: rejectedDomain + outside,
            LocalRadiusMin: radiusMin,
            LocalRadiusMax: radiusMax,
            ActiveListAnnulusSampling: false,
            LocalRadiusConflictChecks: true,
            DeterministicSeed: true);
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
                Succ: value => RhinoMath.IsValidDouble(x: value) && value > 0.0 ? Some(new DworkSurfacePoint(Point: point, Radius: Math.Max(val1: minRadius, val2: value))) : Option<DworkSurfacePoint>.None,
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
                LocalRadiusMax: radiusMax,
                ActiveListAnnulusSampling: true,
                LocalRadiusConflictChecks: true,
                DeterministicSeed: true);
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
        SampleAlgorithmReceipt algorithm = AlgorithmFacts(kind: SampleAlgorithmKind.BridsonActiveListPoisson, seed: Some(seed), radius: Some(radius), attempts: Some(attempts), activePops: Some(activePops), rejectedTooClose: Some(tooClose), rejectedDomain: Some(outside));
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
