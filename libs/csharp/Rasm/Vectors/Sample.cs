using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record SampleKind {
    private SampleKind() { }
    public sealed record ExplicitCase(Seq<Point3d> Points) : SampleKind;
    public sealed record PoissonDiskCase(PositiveMagnitude Radius) : SampleKind;
    public sealed record FarthestCase(Dimension Count) : SampleKind;
    public sealed record OptimizeCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record CapacityCase(Dimension Count, Dimension Limit) : SampleKind;
    public sealed record WeightedCase(Seq<(Point3d Point, double Mass)> Points) : SampleKind;
    public sealed record ScalarDensityCase(ScalarField Density, Dimension Count) : SampleKind;
    public sealed record AdaptiveCase(ScalarField Density, Dimension Count, PositiveMagnitude MinSpacing) : SampleKind;
    public static Fin<SampleKind> Explicit(Seq<Point3d> points, Op? key = null) =>
        points.IsEmpty ? Fin.Fail<SampleKind>(key.OrDefault().InvalidInput()) : Fin.Succ<SampleKind>(new ExplicitCase(Points: points));
    public static Fin<SampleKind> PoissonDisk(double radius, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius).Map(r => (SampleKind)new PoissonDiskCase(Radius: r));
    public static Fin<SampleKind> Farthest(int count, Op? key = null) =>
        key.OrDefault().AcceptValidated<Dimension>(candidate: count).Map(static value => (SampleKind)new FarthestCase(Count: value));
    public static Fin<SampleKind> Optimize(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (count, iterations) => new OptimizeCase(Count: count, Iterations: iterations), key: key);
    public static Fin<SampleKind> Lloyd(int count, int iterations, Op? key = null) =>
        Counted(count: count, value: iterations, create: static (count, iterations) => new LloydCase(Count: count, Iterations: iterations), key: key);
    public static Fin<SampleKind> Capacity(int count, int capacity, Op? key = null) =>
        Counted(count: count, value: capacity, create: static (count, capacity) => new CapacityCase(Count: count, Limit: capacity), key: key);
    private static Fin<SampleKind> Counted(int count, int value, Func<Dimension, Dimension, SampleKind> create, Op? key) =>
        key.OrDefault().AcceptValidated<Dimension>(candidate: count).Bind(c => key.OrDefault().AcceptValidated<Dimension>(candidate: value).Map(v => create(arg1: c, arg2: v)));
    public static Fin<SampleKind> Weighted(Seq<(Point3d Point, double Mass)> points, Op? key = null) =>
        points.IsEmpty || points.Exists(static item => !item.Point.IsValid || !RhinoMath.IsValidDouble(x: item.Mass) || item.Mass <= 0.0) ? Fin.Fail<SampleKind>(key.OrDefault().InvalidInput()) : Fin.Succ<SampleKind>(new WeightedCase(Points: points));
    public static Fin<SampleKind> ScalarDensity(ScalarField density, int count, Op? key = null) => Optional(density).ToFin(key.OrDefault().InvalidInput()).Bind(active => key.OrDefault().AcceptValidated<Dimension>(candidate: count).Map(c => (SampleKind)new ScalarDensityCase(Density: active, Count: c)));
    public static Fin<SampleKind> Adaptive(ScalarField density, int count, double minSpacing, Op? key = null) => Optional(density).ToFin(key.OrDefault().InvalidInput()).Bind(active => key.OrDefault().AcceptValidated<Dimension>(candidate: count).Bind(c => key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: minSpacing).Map(spacing => (SampleKind)new AdaptiveCase(Density: active, Count: c, MinSpacing: spacing))));
    internal Fin<SampleResult> Evaluate(ExtractionDomain domain, Context context, Op key) =>
        SampleKernel.Sample(kind: this, domain: domain, context: context, key: key);
    internal (Option<int> Count, Option<int> Iterations, double MeshScale, bool Density) Request => this switch { FarthestCase c => (Some(c.Count.Value), Option<int>.None, 1.0, false), OptimizeCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false), LloydCase c => (Some(c.Count.Value), Some(c.Iterations.Value), 1.0, false), CapacityCase c => (Some(c.Count.Value), Option<int>.None, c.Limit.Value, false), ScalarDensityCase c => (Some(c.Count.Value), Option<int>.None, 8.0, true), AdaptiveCase c => (Some(c.Count.Value), Option<int>.None, 12.0, true), _ => (Option<int>.None, Option<int>.None, 0.0, false) };
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
}

[SmartEnum<int>] public sealed partial class SampleStopKind { public static readonly SampleStopKind Completed = new(key: 0), CapacityLimited = new(key: 1), AllRejected = new(key: 2), CandidateExhausted = new(key: 3); }
[SmartEnum<int>] public sealed partial class SampleDomainStatus { public static readonly SampleDomainStatus Projected = new(key: 0), CandidateAccepted = new(key: 1), CandidateRejected = new(key: 2); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleReceipt(int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<double> MinSpacing, Option<double> MeanSpacing, Option<double> MaxSpacing, Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations, SampleStopKind Stop, SampleDomainStatus DomainStatus);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct SampleResult(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleReceipt Receipt);
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected);

internal static class SampleKernel {
    internal static Fin<SampleResult> Sample(SampleKind kind, ExtractionDomain domain, Context context, Op key) =>
        kind switch {
            SampleKind.ExplicitCase explicitCase => SampleAdmitted(points: explicitCase.Points.Map(static point => (Point: point, Mass: Option<double>.None)), domain: domain, context: context, key: key),
            SampleKind.WeightedCase weightedCase => SampleAdmitted(points: weightedCase.Points.Map(static item => (item.Point, Mass: Some(item.Mass))), domain: domain, context: context, key: key),
            _ => domain.Switch(
                state: (Kind: kind, Context: context, Key: key),
                supportCase: static (state, d) => SampleGeneratedSupport(kind: state.Kind, space: d.Value, context: state.Context, key: state.Key),
                meshCase: static (state, d) => SampleOnMesh(kind: state.Kind, domain: d.Value, context: state.Context, key: state.Key),
                cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                    ? SampleOnCandidates(kind: state.Kind, candidates: cluster.Vertices, admitsPoisson: false, context: state.Context, key: state.Key)
                    : Fin.Fail<SampleResult>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(SampleResult)))),
        };
    private static Fin<SampleResult> SampleAdmitted(Seq<(Point3d Point, Option<double> Mass)> points, ExtractionDomain domain, Context context, Op key) =>
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
            Receipt: ReceiptOf(attempted: points.Count, emitted: admitted.Accepted, rejected: admitted.Rejected, candidates: Some(points.Count), iterations: Option<int>.None, stop: admitted.Accepted.IsEmpty ? SampleStopKind.AllRejected : SampleStopKind.Completed, status: SampleDomainStatus.Projected, densityError: Option<double>.None));
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
            SampleKind.FarthestCase or SampleKind.OptimizeCase or SampleKind.LloydCase or SampleKind.CapacityCase => kind.Request.Count.ToFin(Fail: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<int>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }).Bind(count =>
            GeometryKernel.SamplePoints(source: space.Value, count: count, context: context, key: key)
                .Bind(points => SampleAdmitted(points: points.Map(static point => (Point: point, Mass: Option<double>.None)), domain: ExtractionDomain.Support(value: space), context: context, key: key)));
    private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return Optional(props).ToFin(key.InvalidResult()).Bind(p =>
            from density in kind.MeshCandidateDensity(area: p.Area, key: key)
            from candidates in EnumerateMeshSurface(mesh: domain.Native, density: density, key: key)
            from sampled in SampleOnCandidates(kind: kind, candidates: candidates, admitsPoisson: true, context: context, key: key)
            select sampled);
    }
    private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<Point3d> candidates, bool admitsPoisson, Context context, Op key) =>
        from selection in kind switch {
            SampleKind.PoissonDiskCase pd when admitsPoisson => Fin.Succ(SelectionOf(points: PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value))),
            SampleKind.FarthestCase fp => Fin.Succ(SelectionOf(points: SelectedPoints(candidates: candidates, indices: FarthestIndices(candidates: candidates, count: fp.Count.Value)))),
            SampleKind.OptimizeCase fpo => Fin.Succ(SelectionOf(points: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value))),
            SampleKind.LloydCase lloyd => RelaxationSample(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value, capacity: Option<int>.None, key: key).Map(SelectionOf),
            SampleKind.CapacityCase ccvt => RelaxationSample(candidates: candidates, count: ccvt.Count.Value, iterations: 1, capacity: Some(ccvt.Limit.Value), key: key).Map(SelectionOf),
            SampleKind.ScalarDensityCase density => DensitySelection(candidates: candidates, density: density.Density, count: density.Count.Value, minSpacing: 0.0, context: context, key: key),
            SampleKind.AdaptiveCase adaptive => DensitySelection(candidates: candidates, density: adaptive.Density, count: adaptive.Count.Value, minSpacing: adaptive.MinSpacing.Value, context: context, key: key),
            SampleKind.PoissonDiskCase pd => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: pd.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<SampleSelection>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }
        let sampled = toSeq(selection.Points)
        let rejected = selection.DensityRejected.IfNone(Math.Max(val1: 0, val2: candidates.Count - selection.Points.Length))
        select new SampleResult(
            Points: sampled,
            Mass: selection.Mass,
            Receipt: ReceiptOf(
                attempted: candidates.Count, emitted: sampled, rejected: rejected, candidates: Some(candidates.Count), iterations: kind.Request.Iterations,
                stop: sampled.Count <= 0 ? SampleStopKind.AllRejected : kind.Request.Count.Map(requested => sampled.Count < requested ? SampleStopKind.CandidateExhausted : SampleStopKind.Completed).IfNone(SampleStopKind.Completed),
                status: selection.DensityRejected.Map(static rejectedCount => rejectedCount > 0 ? SampleDomainStatus.CandidateRejected : SampleDomainStatus.CandidateAccepted).IfNone(SampleDomainStatus.CandidateAccepted),
                densityError: kind.DensityError(emitted: sampled.Count), densityAccepted: selection.DensityAccepted, densityRejected: selection.DensityRejected));
    private static SampleSelection SelectionOf(Point3d[] points) => new(Points: points, Mass: Option<Arr<double>>.None, DensityAccepted: Option<int>.None, DensityRejected: Option<int>.None);
    private static SampleReceipt ReceiptOf(int attempted, Seq<Point3d> emitted, int rejected, Option<int> candidates, Option<int> iterations, SampleStopKind stop, SampleDomainStatus status, Option<double> densityError, Option<int> densityAccepted = default, Option<int> densityRejected = default) =>
        (emitted.Count < 2
            ? (Option<double>.None, Option<double>.None, Option<double>.None)
            : toSeq(Enumerable.Range(start: 0, count: emitted.Count - 1)
                .SelectMany(collectionSelector: i => Enumerable.Range(start: i + 1, count: emitted.Count - i - 1), resultSelector: (i, j) => emitted[index: i].DistanceTo(other: emitted[index: j])))
                .Fold(initialState: (Min: double.PositiveInfinity, Max: 0.0, Sum: 0.0, Count: 0), f: static (acc, distance) => (Min: Math.Min(val1: acc.Min, val2: distance), Max: Math.Max(val1: acc.Max, val2: distance), Sum: acc.Sum + distance, Count: acc.Count + 1)) switch { { Count: > 0 } stats => (Some(stats.Min), Some(stats.Sum / stats.Count), Some(stats.Max)), _ => (Option<double>.None, Option<double>.None, Option<double>.None) }) switch {
                    (Option<double> min, Option<double> mean, Option<double> max) => new SampleReceipt(Attempted: attempted, Emitted: emitted.Count, Rejected: rejected, CandidateCount: candidates, MinSpacing: min, MeanSpacing: mean, MaxSpacing: max, DensityError: densityError, DensityAccepted: densityAccepted, DensityRejected: densityRejected, Iterations: iterations, Stop: stop, DomainStatus: status),
                };
    private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, Op key) =>
        mass.Fold(initialState: 0.0, f: static (sum, value) => sum + value) switch { double total when RhinoMath.IsValidDouble(x: total) && total > 0.0 => key.AcceptValue(value: new Arr<double>([.. mass.AsIterable().Select(value => value / total)])), _ => Fin.Fail<Arr<double>>(key.InvalidResult()) };
    private static Fin<SampleSelection> DensitySelection(Seq<Point3d> candidates, ScalarField density, int count, double minSpacing, Context context, Op key) {
        double[] weights = new double[candidates.Count];
        return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
            initialState: Fin.Succ((Accepted: 0, Rejected: 0, MaxWeight: 0.0)),
            f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i], context: context, key: key)
                .Bind(value => RhinoMath.IsValidDouble(x: value) && value > 0.0
                    ? key.AcceptValue(value: value).Map(valid => { weights[i] = valid; return (Accepted: current.Accepted + 1, current.Rejected, MaxWeight: Math.Max(val1: current.MaxWeight, val2: valid)); })
                    : Fin.Succ((current.Accepted, Rejected: current.Rejected + 1, current.MaxWeight)))))
            .Bind(stats => stats.Accepted > 0 ? PrioritySelection(candidates: candidates, weights: weights, count: count, minSpacing: minSpacing, maxWeight: stats.MaxWeight, accepted: stats.Accepted, rejected: stats.Rejected, key: key) : Fin.Fail<SampleSelection>(key.InvalidResult()));
    }
    private static Fin<SampleSelection> PrioritySelection(Seq<Point3d> candidates, double[] weights, int count, double minSpacing, double maxWeight, int accepted, int rejected, Op key) {
        List<Point3d> chosen = []; List<double> mass = [];
        using IEnumerator<int> ordered = Enumerable.Range(start: 0, count: candidates.Count)
            .Where(i => weights[i] > 0.0)
            .OrderBy(i => -Math.Log(d: Math.Max(val1: (CandidateOrderKey(point: candidates[index: i]) + 1.0) / 18446744073709551616.0, val2: RhinoMath.SqrtEpsilon)) / weights[i])
            .GetEnumerator();
        for (; chosen.Count < count && ordered.MoveNext();) {
            int index = ordered.Current;
            Point3d candidate = candidates[index: index];
            double local = minSpacing / Math.Sqrt(d: Math.Max(val1: weights[index] / Math.Max(val1: maxWeight, val2: RhinoMath.SqrtEpsilon), val2: RhinoMath.SqrtEpsilon)); double localSq = local * local;
            if (chosen.All(point => point.DistanceToSquared(other: candidate) >= localSq)) { chosen.Add(item: candidate); mass.Add(item: weights[index]); }
        }
        return NormalizeMass(mass: toSeq(mass), key: key)
            .Map(normalized => new SampleSelection(Points: [.. chosen], Mass: Some(normalized), DensityAccepted: Some(accepted), DensityRejected: Some(rejected)));
    }
    private static Fin<Seq<Point3d>> EnumerateMeshSurface(Mesh mesh, double density, Op key) {
        List<Point3d> samples = [];
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<Seq<Point3d>>(key.InvalidResult());
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A]; Point3d b = triangulated.Vertices[index: face.B]; Point3d c = triangulated.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: area * density)); int side = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Sqrt(d: count * 2.0)));
            int emitted = 0;
            for (int i = 0; i <= side && emitted < count; i++) {
                for (int j = 0; j <= side - i && emitted < count; j++) {
                    double wa = (i + 1.0) / (side + 3.0); double wb = (j + 1.0) / (side + 3.0); double wc = 1.0 - wa - wb;
                    samples.Add(item: new Point3d(x: (wa * a.X) + (wb * b.X) + (wc * c.X), y: (wa * a.Y) + (wb * b.Y) + (wc * c.Y), z: (wa * a.Z) + (wb * b.Z) + (wc * c.Z)));
                    emitted++;
                }
            }
        }
        return key.AcceptValue(value: toSeq(samples));
    }
    private static Point3d[] SelectedPoints(Seq<Point3d> candidates, int[] indices) => [.. indices.Select(i => candidates[index: i])];
    private static Point3d[] PoissonDiskSample(Seq<Point3d> candidates, double radius) {
        double r2 = radius * radius;
        List<Point3d> chosen = []; PointCloud chosenIndex = [];
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count).OrderBy(i => CandidateOrderKey(point: candidates[index: i]))];
        for (int idx = 0; idx < order.Length; idx++) {
            Point3d p = candidates[index: order[idx]];
            int nearest = chosenIndex.Count > 0 ? chosenIndex.ClosestPoint(testPoint: p) : -1;
            bool tooClose = nearest >= 0 && nearest < chosen.Count && p.DistanceToSquared(other: chosen[index: nearest]) < r2;
            if (!tooClose) { chosen.Add(item: p); chosenIndex.Add(point: p); }
        }
        return [.. chosen];
    }
    private static ulong CandidateOrderKey(Point3d point) {
        static ulong Bits(double value) => (ulong)BitConverter.DoubleToInt64Bits(value: value == 0.0 ? 0.0 : value);
        unchecked {
            ulong hash = Bits(value: point.X) * 0x9E3779B185EBCA87UL;
            hash ^= Bits(value: point.Y) + 0xC2B2AE3D27D4EB4FUL + (hash << 6) + (hash >> 2);
            hash ^= Bits(value: point.Z) + 0x165667B19E3779F9UL + (hash << 6) + (hash >> 2);
            return hash;
        }
    }
    private static int[] FarthestIndices(Seq<Point3d> candidates, int count) {
        if (candidates.IsEmpty || count < 1) return [];
        int total = candidates.Count; int actualCount = Math.Min(val1: count, val2: total); int[] chosen = new int[actualCount]; bool[] selected = new bool[total];
        Point3d centroid = toSeq(Enumerable.Range(start: 0, count: total))
            .Fold(initialState: Point3d.Origin, f: (acc, i) => new Point3d(x: acc.X + candidates[index: i].X, y: acc.Y + candidates[index: i].Y, z: acc.Z + candidates[index: i].Z)) switch { Point3d sum => new Point3d(x: sum.X / total, y: sum.Y / total, z: sum.Z / total) };
        chosen[0] = Enumerable.Range(start: 0, count: total)
            .Select(i => (Index: i, Distance: candidates[index: i].DistanceToSquared(other: centroid)))
            .Aggregate((best, item) => item.Distance > best.Distance ? item : best).Index;
        selected[chosen[0]] = true; double[] minDistSq = [.. Enumerable.Range(start: 0, count: total).Select(i => candidates[index: i].DistanceToSquared(other: candidates[index: chosen[0]]))];
        for (int pick = 1; pick < actualCount; pick++) {
            int farthest = Enumerable.Range(start: 0, count: total).Where(i => !selected[i]).Aggregate((best, i) => minDistSq[i] > minDistSq[best] ? i : best);
            chosen[pick] = farthest;
            selected[farthest] = true;
            for (int i = 0; i < total; i++) minDistSq[i] = Math.Min(val1: minDistSq[i], val2: candidates[index: i].DistanceToSquared(other: candidates[index: farthest]));
        }
        return chosen;
    }
    private static Point3d[] FpoSample(Seq<Point3d> candidates, int count, int iterations) {
        int[] chosen = FarthestIndices(candidates: candidates, count: count);
        if (chosen.Length < 2) return SelectedPoints(candidates: candidates, indices: chosen);
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
        return SelectedPoints(candidates: candidates, indices: chosen);
    }
    private static (int Index, double Distance) WorstCoverage(Seq<Point3d> candidates, int[] chosen) =>
        candidates.Count <= 0 || chosen.Length <= 0
            ? (0, -1.0)
            : Enumerable.Range(start: 0, count: candidates.Count)
                .Select(i => (Index: i, Distance: chosen.Min(c => candidates[index: i].DistanceToSquared(other: candidates[index: c]))))
                .Aggregate((worst, item) => item.Distance > worst.Distance ? item : worst);
    private static Fin<Point3d[]> RelaxationSample(Seq<Point3d> candidates, int count, int iterations, Option<int> capacity, Op key) =>
        toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
            initialState: Fin.Succ(SelectedPoints(candidates: candidates, indices: FarthestIndices(candidates: candidates, count: count))),
            f: (state, _) => state.Bind(sites => RelaxSites(sites: sites, candidates: candidates, total: capacity.Map(limit => Math.Min(val1: candidates.Count, val2: sites.Length * limit)).IfNone(candidates.Count), capacity: capacity, key: key)));
    private static Fin<Point3d[]> RelaxSites(Point3d[] sites, Seq<Point3d> candidates, int total, Option<int> capacity, Op key) {
        if (sites.Length == 0) return Fin.Succ(sites);
        Vector3d[] sums = new Vector3d[sites.Length]; int[] counts = new int[sites.Length]; int[] siteFill = new int[sites.Length];
        PointCloud siteIndex = []; siteIndex.AddRange(points: sites);
        for (int i = 0; i < total; i++) {
            int closest = capacity.Match(
                Some: limit => {
                    int hit = -1; double best = double.MaxValue;
                    for (int s = 0; s < sites.Length; s++) {
                        if (siteFill[s] >= limit) continue;
                        double distance = candidates[index: i].DistanceToSquared(other: sites[s]);
                        if (distance < best) { best = distance; hit = s; }
                    }
                    return hit;
                },
                None: () => siteIndex.ClosestPoint(testPoint: candidates[index: i]));
            if (closest < 0) return Fin.Fail<Point3d[]>(key.InvalidResult());
            siteFill[closest]++; sums[closest] += (Vector3d)candidates[index: i]; counts[closest]++;
        }
        PointCloud candidateIndex = []; candidateIndex.AddRange(points: candidates.AsIterable());
        return toSeq(Enumerable.Range(start: 0, count: sites.Length)).Fold(
            initialState: Fin.Succ(sites),
            f: (state, s) => counts[s] <= 0
                ? state
                : state.Bind(active => (candidateIndex.ClosestPoint(testPoint: Point3d.Origin + (sums[s] / counts[s])) switch {
                    int nearest when nearest >= 0 && nearest < candidates.Count => Fin.Succ(candidates[index: nearest]),
                    _ => Fin.Fail<Point3d>(key.InvalidResult()),
                })
                    .Map(nearest => {
                        active[s] = nearest;
                        return active;
                    })));
    }
}
