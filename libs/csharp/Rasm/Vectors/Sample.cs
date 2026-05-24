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
    public static Fin<SampleKind> Explicit(Seq<Point3d> points, Op? key = null) {
        Op op = key.OrDefault();
        return points.IsEmpty
            ? Fin.Fail<SampleKind>(op.InvalidInput())
            : Fin.Succ<SampleKind>(new ExplicitCase(Points: points));
    }
    public static Fin<SampleKind> PoissonDisk(double radius, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius).Map(r => (SampleKind)new PoissonDiskCase(Radius: r));
    public static Fin<SampleKind> Farthest(int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Map(static value => (SampleKind)new FarthestCase(Count: value));
    }
    public static Fin<SampleKind> Optimize(int count, int iterations, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from i in op.AcceptValidated<Dimension>(candidate: iterations)
               select (SampleKind)new OptimizeCase(Count: c, Iterations: i);
    }
    public static Fin<SampleKind> Lloyd(int count, int iterations, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from i in op.AcceptValidated<Dimension>(candidate: iterations)
               select (SampleKind)new LloydCase(Count: c, Iterations: i);
    }
    public static Fin<SampleKind> Capacity(int count, int capacity, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from cap in op.AcceptValidated<Dimension>(candidate: capacity)
               select (SampleKind)new CapacityCase(Count: c, Limit: cap);
    }
    internal Fin<SampleResult> Evaluate(ExtractionDomain domain, Context context, Op key) =>
        SampleKernel.Sample(kind: this, domain: domain, context: context, key: key);
    internal Fin<TOut> Project<TOut>(ExtractionDomain domain, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from result in Evaluate(domain: domain, context: context, key: op)
               from output in typeof(TOut) switch {
                   Type t when t == typeof(Seq<Point3d>) => Fin.Succ((TOut)(object)result.Points),
                   Type t when t == typeof(VectorCloud) => VectorCloud.Cluster(points: result.Points, context: context, key: op).Map(static value => (TOut)(object)value),
                   Type t when t == typeof(PointCloud) => VectorCloud.Cluster(points: result.Points, context: context, key: op)
                       .Bind(cloud => cloud is VectorCloud.ClusterCase cluster
                           ? Fin.Succ((TOut)(object)cluster.Indexed)
                           : Fin.Fail<TOut>(op.InvalidResult())),
                   Type t when t == typeof(SampleReceipt) => Fin.Succ((TOut)(object)result.Receipt),
                   _ => Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(SampleKind), outputType: typeof(TOut))),
               }
               select output;
    }
    internal double MeshCandidateDensity(double area) {
        double safeArea = Math.Max(val1: area, val2: RhinoMath.SqrtEpsilon);
        double target = this switch {
            ExplicitCase ex => ex.Points.Count,
            PoissonDiskCase pd => safeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: RhinoMath.SqrtEpsilon),
            FarthestCase fp => fp.Count.Value,
            OptimizeCase fpo => fpo.Count.Value,
            LloydCase lloyd => lloyd.Count.Value,
            CapacityCase ccvt => ccvt.Count.Value * ccvt.Limit.Value,
            _ => 1.0,
        };
        return Math.Max(val1: target / safeArea, val2: 1.0 / safeArea);
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SampleReceipt(int Attempted, int Emitted, int Rejected);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct SampleResult(Seq<Point3d> Points, SampleReceipt Receipt);

internal static class SampleKernel {
    // Boundary algorithms operate on deterministic triangulated mesh candidates.
    internal static Fin<SampleResult> Sample(SampleKind kind, ExtractionDomain domain, Context context, Op key) =>
        kind switch {
            SampleKind.ExplicitCase explicitCase => SampleExplicit(points: explicitCase.Points, domain: domain, context: context, key: key),
            _ => domain.Switch(
                state: (Kind: kind, Context: context, Key: key),
                supportCase: static (state, d) => SampleGeneratedSupport(kind: state.Kind, space: d.Value, context: state.Context, key: state.Key),
                meshCase: static (state, d) => SampleOnMesh(kind: state.Kind, domain: d.Value, context: state.Context, key: state.Key),
                cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                    ? SampleOnCandidates(kind: state.Kind, candidates: cluster.Vertices, admitsPoisson: false, context: state.Context, key: state.Key)
                    : Fin.Fail<SampleResult>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(SampleResult)))),
        };
    private static Fin<SampleResult> SampleExplicit(Seq<Point3d> points, ExtractionDomain domain, Context context, Op key) =>
        from admitted in points.Fold(
            initialState: Fin.Succ((Accepted: (Seq<Point3d>)[], Rejected: 0)),
            f: (state, point) => state.Bind(current =>
                AdmitPoint(point: point, domain: domain, context: context, key: key).Match(
                    Succ: accepted => Fin.Succ((Accepted: current.Accepted.Add(accepted), current.Rejected)),
                    Fail: _ => Fin.Succ((current.Accepted, Rejected: current.Rejected + 1)))))
        select new SampleResult(
            Points: admitted.Accepted,
            Receipt: new SampleReceipt(Attempted: points.Count, Emitted: admitted.Accepted.Count, Rejected: admitted.Rejected));
    private static Fin<Point3d> AdmitPoint(Point3d point, ExtractionDomain domain, Context context, Op key) =>
        key.AcceptValue(value: point).Bind(valid => domain.Switch(
            state: (Point: valid, Context: context, Key: key),
            supportCase: static (state, d) => d.Value.Closest(sample: state.Point, key: state.Key)
                .Bind(hit => state.Key.AcceptValue(value: hit.Point)),
            meshCase: static (state, d) => Optional(d.Value.Native.ClosestMeshPoint(testPoint: state.Point, maximumDistance: state.Context.Absolute.Value))
                .ToFin(state.Key.InvalidResult())
                .Bind(meshPoint => state.Key.AcceptValue(value: meshPoint.Point)),
            cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
                ? AdmitClusterPoint(cluster: cluster, point: state.Point, tolerance: state.Context.Absolute.Value, key: state.Key)
                : Fin.Fail<Point3d>(state.Key.Unsupported(geometryType: d.Value.GetType(), outputType: typeof(Point3d)))));
    private static Fin<Point3d> AdmitClusterPoint(VectorCloud.ClusterCase cluster, Point3d point, double tolerance, Op key) {
        double toleranceSquared = tolerance * tolerance;
        return cluster.Vertices.Find(vertex => vertex.DistanceToSquared(other: point) <= toleranceSquared) switch {
            { IsSome: true, Case: Point3d hit } => key.AcceptValue(value: hit),
            _ => Fin.Fail<Point3d>(key.InvalidInput()),
        };
    }
    private static Fin<SampleResult> SampleGeneratedSupport(SampleKind kind, SupportSpace space, Context context, Op key) =>
        CountOf(kind: kind, key: key).Bind(count =>
            GeometryKernel.SamplePoints(source: space.Value, count: count, context: context, key: key)
                .Bind(points => SampleExplicit(points: points, domain: ExtractionDomain.Support(value: space), context: context, key: key)));
    private static Fin<int> CountOf(SampleKind kind, Op key) =>
        kind switch {
            SampleKind.FarthestCase c => Fin.Succ(c.Count.Value),
            SampleKind.OptimizeCase c => Fin.Succ(c.Count.Value),
            SampleKind.LloydCase c => Fin.Succ(c.Count.Value),
            SampleKind.CapacityCase c => Fin.Succ(c.Count.Value),
            _ => Fin.Fail<int>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        };
    private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        return props switch {
            null => Fin.Fail<SampleResult>(error: key.InvalidResult()),
            _ => from candidates in EnumerateMeshSurface(mesh: domain.Native, density: kind.MeshCandidateDensity(area: props.Area), key: key)
                 from sampled in SampleOnCandidates(kind: kind, candidates: candidates, admitsPoisson: true, context: context, key: key)
                 select sampled,
        };
    }
    private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<Point3d> candidates, bool admitsPoisson, Context context, Op key) =>
        from points in kind switch {
            SampleKind.PoissonDiskCase pd when admitsPoisson => Fin.Succ(PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value)),
            SampleKind.FarthestCase fp => Fin.Succ(FarthestSample(candidates: candidates, count: fp.Count.Value)),
            SampleKind.OptimizeCase fpo => Fin.Succ(FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value)),
            SampleKind.LloydCase lloyd => LloydRelaxation(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value, key: key),
            SampleKind.CapacityCase ccvt => CapacitySample(candidates: candidates, count: ccvt.Count.Value, capacity: ccvt.Limit.Value, key: key),
            SampleKind.PoissonDiskCase pd => Fin.Fail<Point3d[]>(error: key.Unsupported(geometryType: pd.GetType(), outputType: typeof(SampleResult))),
            _ => Fin.Fail<Point3d[]>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(SampleResult))),
        }
        let sampled = toSeq(points)
        let rejected = Math.Max(val1: 0, val2: candidates.Count - points.Length)
        select new SampleResult(
            Points: sampled,
            Receipt: new SampleReceipt(Attempted: candidates.Count, Emitted: points.Length, Rejected: rejected));
    private static Fin<Seq<Point3d>> EnumerateMeshSurface(Mesh mesh, double density, Op key) {
        List<Point3d> samples = [];
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0
            && !triangulated.Faces.ConvertQuadsToTriangles())
            return Fin.Fail<Seq<Point3d>>(key.InvalidResult());
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
                    double wa = (i + 1.0) / (side + 3.0);
                    double wb = (j + 1.0) / (side + 3.0);
                    double wc = 1.0 - wa - wb;
                    samples.Add(item: new Point3d(x: (wa * a.X) + (wb * b.X) + (wc * c.X), y: (wa * a.Y) + (wb * b.Y) + (wc * c.Y), z: (wa * a.Z) + (wb * b.Z) + (wc * c.Z)));
                    emitted++;
                }
            }
        }
        return key.AcceptValue(value: toSeq(samples));
    }
    private static Point3d[] PoissonDiskSample(Seq<Point3d> candidates, double radius) {
        if (candidates.IsEmpty) return [];
        double r2 = radius * radius;
        List<Point3d> chosen = [];
        PointCloud chosenIndex = [];
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count)
            .OrderBy(i => CandidateOrderKey(point: candidates[index: i]))];
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
    private static Point3d[] FarthestSample(Seq<Point3d> candidates, int count) {
        int[] chosen = FarthestIndices(candidates: candidates, count: count);
        return [.. chosen.Select(i => candidates[index: i])];
    }
    private static int[] FarthestIndices(Seq<Point3d> candidates, int count) {
        if (candidates.IsEmpty || count < 1) return [];
        int total = candidates.Count;
        int actualCount = Math.Min(val1: count, val2: total);
        int[] chosen = new int[actualCount];
        bool[] selected = new bool[total];
        chosen[0] = InitialCandidateIndex(candidates: candidates);
        selected[chosen[0]] = true;
        double[] minDistSq = new double[total];
        for (int i = 0; i < total; i++) minDistSq[i] = candidates[index: i].DistanceToSquared(other: candidates[index: chosen[0]]);
        for (int pick = 1; pick < actualCount; pick++) {
            int farthest = 0; double best = -1.0;
            for (int i = 0; i < total; i++) if (!selected[i] && minDistSq[i] > best) { best = minDistSq[i]; farthest = i; }
            chosen[pick] = farthest;
            selected[farthest] = true;
            for (int i = 0; i < total; i++) minDistSq[i] = Math.Min(val1: minDistSq[i], val2: candidates[index: i].DistanceToSquared(other: candidates[index: farthest]));
        }
        return chosen;
    }
    private static int InitialCandidateIndex(Seq<Point3d> candidates) {
        Point3d centroid = Point3d.Origin;
        for (int i = 0; i < candidates.Count; i++)
            centroid = new Point3d(x: centroid.X + candidates[index: i].X, y: centroid.Y + candidates[index: i].Y, z: centroid.Z + candidates[index: i].Z);
        centroid = new Point3d(x: centroid.X / candidates.Count, y: centroid.Y / candidates.Count, z: centroid.Z / candidates.Count);
        int farthest = 0; double best = -1.0;
        for (int i = 0; i < candidates.Count; i++) {
            double d = candidates[index: i].DistanceToSquared(other: centroid);
            if (d > best) { best = d; farthest = i; }
        }
        return farthest;
    }
    private static Point3d[] FpoSample(Seq<Point3d> candidates, int count, int iterations) {
        int[] chosen = FarthestIndices(candidates: candidates, count: count);
        if (chosen.Length < 2) return [.. chosen.Select(i => candidates[index: i])];
        double bestScore = CoveringRadiusSquared(candidates: candidates, chosen: chosen);
        for (int iter = 0; iter < iterations; iter++) {
            bool improved = false;
            int replacement = WorstCoveredCandidate(candidates: candidates, chosen: chosen);
            for (int i = 0; i < chosen.Length; i++) {
                if (chosen.Contains(value: replacement)) continue;
                int previous = chosen[i];
                chosen[i] = replacement;
                double score = CoveringRadiusSquared(candidates: candidates, chosen: chosen);
                if (score + RhinoMath.SqrtEpsilon < bestScore) { bestScore = score; improved = true; break; }
                chosen[i] = previous;
            }
            if (!improved) break;
        }
        return [.. chosen.Select(i => candidates[index: i])];
    }
    private static double CoveringRadiusSquared(Seq<Point3d> candidates, int[] chosen) {
        double worst = 0.0;
        for (int i = 0; i < candidates.Count; i++) worst = Math.Max(val1: worst, val2: MinDistanceToChosen(candidates: candidates, chosen: chosen, candidateIndex: i));
        return worst;
    }
    private static int WorstCoveredCandidate(Seq<Point3d> candidates, int[] chosen) {
        int worst = 0; double best = -1.0;
        for (int i = 0; i < candidates.Count; i++) {
            double distance = MinDistanceToChosen(candidates: candidates, chosen: chosen, candidateIndex: i);
            if (distance > best) { best = distance; worst = i; }
        }
        return worst;
    }
    private static double MinDistanceToChosen(Seq<Point3d> candidates, int[] chosen, int candidateIndex) {
        double best = double.PositiveInfinity;
        for (int i = 0; i < chosen.Length; i++) best = Math.Min(val1: best, val2: candidates[index: candidateIndex].DistanceToSquared(other: candidates[index: chosen[i]]));
        return best;
    }
    private static Fin<Point3d[]> LloydRelaxation(Seq<Point3d> candidates, int count, int iterations, Op key) {
        if (candidates.IsEmpty) return Fin.Succ(System.Array.Empty<Point3d>());
        int total = candidates.Count;
        PointCloud candidateIndex = CandidateIndex(candidates: candidates);
        return toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
            initialState: Fin.Succ(FarthestSample(candidates: candidates, count: count)),
            f: (state, _) => state.Bind(sites => {
                Vector3d[] sums = new Vector3d[sites.Length];
                int[] counts = new int[sites.Length];
                PointCloud siteIndex = [];
                siteIndex.AddRange(points: sites);
                for (int i = 0; i < total; i++) {
                    int closest = siteIndex.ClosestPoint(testPoint: candidates[index: i]);
                    if (closest < 0 || closest >= sites.Length) return Fin.Fail<Point3d[]>(key.InvalidResult());
                    sums[closest] += (Vector3d)candidates[index: i];
                    counts[closest]++;
                }
                return SnapSitesToCandidateCentroids(sites: sites, sums: sums, counts: counts, candidates: candidates, index: candidateIndex, key: key);
            }));
    }
    private static PointCloud CandidateIndex(Seq<Point3d> candidates) {
        PointCloud cloud = [];
        cloud.AddRange(points: candidates.AsIterable());
        return cloud;
    }
    private static Fin<Point3d> NearestCandidate(Seq<Point3d> candidates, PointCloud index, Point3d point, Op key) =>
        index.ClosestPoint(testPoint: point) switch {
            int nearest when nearest >= 0 && nearest < candidates.Count => Fin.Succ(candidates[index: nearest]),
            _ => Fin.Fail<Point3d>(key.InvalidResult()),
        };
    private static Fin<Point3d[]> CapacitySample(Seq<Point3d> candidates, int count, int capacity, Op key) {
        Point3d[] sites = FarthestSample(candidates: candidates, count: count);
        int total = candidates.Count;
        if (sites.Length == 0) return Fin.Succ(sites);
        total = Math.Min(val1: total, val2: sites.Length * capacity);
        PointCloud candidateIndex = CandidateIndex(candidates: candidates);
        int[] assignment = new int[total];
        int[] siteFill = new int[sites.Length];
        for (int i = 0; i < total; i++) {
            int closest = -1; double best = double.MaxValue;
            for (int s = 0; s < sites.Length; s++) {
                if (siteFill[s] >= capacity) continue;
                double d = candidates[index: i].DistanceToSquared(other: sites[s]);
                if (d < best) { best = d; closest = s; }
            }
            if (closest < 0) return Fin.Fail<Point3d[]>(key.InvalidResult());
            assignment[i] = closest; siteFill[closest]++;
        }
        Vector3d[] sums = new Vector3d[sites.Length]; int[] counts = new int[sites.Length];
        for (int i = 0; i < total; i++) {
            int s = assignment[i];
            sums[s] += (Vector3d)candidates[index: i];
            counts[s]++;
        }
        return SnapSitesToCandidateCentroids(sites: sites, sums: sums, counts: counts, candidates: candidates, index: candidateIndex, key: key);
    }
    private static Fin<Point3d[]> SnapSitesToCandidateCentroids(Point3d[] sites, Vector3d[] sums, int[] counts, Seq<Point3d> candidates, PointCloud index, Op key) =>
        toSeq(Enumerable.Range(start: 0, count: sites.Length)).Fold(
            initialState: Fin.Succ(sites),
            f: (state, s) => counts[s] <= 0
                ? state
                : state.Bind(active => NearestCandidate(candidates: candidates, index: index, point: Point3d.Origin + (sums[s] / counts[s]), key: key)
                    .Map(nearest => {
                        active[s] = nearest;
                        return active;
                    })));
}
