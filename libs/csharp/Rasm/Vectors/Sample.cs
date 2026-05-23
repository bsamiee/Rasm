namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record SampleKind {
    private SampleKind() { }
    public sealed record PoissonDiskCase(PositiveMagnitude Radius) : SampleKind;
    public sealed record FarthestCase(Dimension Count) : SampleKind;
    public sealed record OptimizeCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SampleKind;
    public sealed record CapacityCase(Dimension Count, Dimension Limit) : SampleKind;
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
    internal Fin<VectorCloud> Sample(MeshSpace domain, Context context, Op? key = null) =>
        SampleKernel.SampleOnMesh(kind: this, domain: domain, context: context, key: key.OrDefault());
    internal double MeshCandidateDensity(double area) {
        double safeArea = Math.Max(val1: area, val2: RhinoMath.SqrtEpsilon);
        double target = this switch {
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

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class SampleKernel {
    // Boundary algorithms operate on deterministic triangulated mesh candidates.
    internal static Fin<VectorCloud> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native);
        return props switch {
            null => Fin.Fail<VectorCloud>(error: key.InvalidResult()),
            _ => EnumerateMeshSurface(mesh: domain.Native, density: kind.MeshCandidateDensity(area: props.Area)) switch {
                Seq<Point3d> candidates => kind switch {
                    SampleKind.PoissonDiskCase pd => FromArray(points: PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value), context: context, key: key),
                    SampleKind.FarthestCase fp => FromArray(points: FarthestSample(candidates: candidates, count: fp.Count.Value), context: context, key: key),
                    SampleKind.OptimizeCase fpo => FromArray(points: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value), context: context, key: key),
                    SampleKind.LloydCase lloyd => FromArray(points: LloydRelaxation(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value), context: context, key: key),
                    SampleKind.CapacityCase ccvt => CapacitySample(candidates: candidates, count: ccvt.Count.Value, capacity: ccvt.Limit.Value, key: key)
                        .Bind(points => FromArray(points: points, context: context, key: key)),
                    _ => Fin.Fail<VectorCloud>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud))),
                },
            },
        };
    }
    private static Fin<VectorCloud> FromArray(Point3d[] points, Context context, Op key) =>
        VectorCloud.Cluster(points: toSeq(points), context: context, key: key);
    private static Seq<Point3d> EnumerateMeshSurface(Mesh mesh, double density) {
        List<Point3d> samples = [];
        using Mesh triangulated = mesh.DuplicateMesh();
        _ = triangulated.Faces.ConvertQuadsToTriangles();
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
        return toSeq(samples);
    }
    private static Point3d[] PoissonDiskSample(Seq<Point3d> candidates, double radius) {
        if (candidates.IsEmpty) return [];
        double r2 = radius * radius;
        List<Point3d> chosen = [];
        PointCloud chosenIndex = [];
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count)
            .OrderBy(i => candidates[index: i].X)
            .ThenBy(i => candidates[index: i].Y)
            .ThenBy(i => candidates[index: i].Z)];
        for (int idx = 0; idx < order.Length; idx++) {
            Point3d p = candidates[index: order[idx]];
            int nearest = chosenIndex.Count > 0 ? chosenIndex.ClosestPoint(testPoint: p) : -1;
            bool tooClose = nearest >= 0 && nearest < chosen.Count && p.DistanceToSquared(other: chosen[index: nearest]) < r2;
            if (!tooClose) { chosen.Add(item: p); chosenIndex.Add(point: p); }
        }
        return [.. chosen];
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
        chosen[0] = InitialCandidateIndex(candidates: candidates);
        double[] minDistSq = new double[total];
        for (int i = 0; i < total; i++) minDistSq[i] = candidates[index: i].DistanceToSquared(other: candidates[index: chosen[0]]);
        for (int pick = 1; pick < actualCount; pick++) {
            int farthest = 0; double best = -1.0;
            for (int i = 0; i < total; i++) if (minDistSq[i] > best) { best = minDistSq[i]; farthest = i; }
            chosen[pick] = farthest;
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
    private static Point3d[] LloydRelaxation(Seq<Point3d> candidates, int count, int iterations) {
        if (candidates.IsEmpty) return [];
        int total = candidates.Count;
        Point3d[] sites = FarthestSample(candidates: candidates, count: count);
        PointCloud candidateIndex = CandidateIndex(candidates: candidates);
        for (int iter = 0; iter < iterations; iter++) {
            Point3d[] sums = new Point3d[sites.Length];
            int[] counts = new int[sites.Length];
            PointCloud siteIndex = [];
            siteIndex.AddRange(points: sites);
            for (int i = 0; i < total; i++) {
                int closest = siteIndex.ClosestPoint(testPoint: candidates[index: i]);
                if (closest < 0 || closest >= sites.Length) closest = 0;
                sums[closest] = new Point3d(x: sums[closest].X + candidates[index: i].X, y: sums[closest].Y + candidates[index: i].Y, z: sums[closest].Z + candidates[index: i].Z);
                counts[closest]++;
            }
            for (int s = 0; s < sites.Length; s++)
                if (counts[s] > 0) sites[s] = NearestCandidate(candidates: candidates, index: candidateIndex, point: new Point3d(x: sums[s].X / counts[s], y: sums[s].Y / counts[s], z: sums[s].Z / counts[s]));
        }
        return sites;
    }
    private static PointCloud CandidateIndex(Seq<Point3d> candidates) {
        PointCloud cloud = [];
        cloud.AddRange(points: candidates.AsIterable());
        return cloud;
    }
    private static Point3d NearestCandidate(Seq<Point3d> candidates, PointCloud index, Point3d point) =>
        index.ClosestPoint(testPoint: point) switch {
            int nearest when nearest >= 0 && nearest < candidates.Count => candidates[index: nearest],
            _ => candidates[index: 0],
        };
    private static Fin<Point3d[]> CapacitySample(Seq<Point3d> candidates, int count, int capacity, Op key) {
        Point3d[] sites = FarthestSample(candidates: candidates, count: count);
        int total = candidates.Count;
        if (sites.Length == 0 || (sites.Length * capacity) < total) return Fin.Fail<Point3d[]>(key.InvalidInput());
        int[] assignment = new int[total];
        int[] siteFill = new int[sites.Length];
        for (int i = 0; i < total; i++) {
            int closest = -1; double best = double.MaxValue;
            for (int s = 0; s < sites.Length; s++) {
                if (siteFill[s] >= capacity) continue;
                double d = candidates[index: i].DistanceToSquared(other: sites[s]);
                if (d < best) { best = d; closest = s; }
            }
            if (closest < 0)
                for (int s = 0; s < sites.Length; s++) {
                    double d = candidates[index: i].DistanceToSquared(other: sites[s]);
                    if (d < best) { best = d; closest = s; }
                }
            assignment[i] = closest; siteFill[closest]++;
        }
        Point3d[] sums = new Point3d[sites.Length]; int[] counts = new int[sites.Length];
        for (int i = 0; i < total; i++) {
            int s = assignment[i];
            sums[s] = new Point3d(x: sums[s].X + candidates[index: i].X, y: sums[s].Y + candidates[index: i].Y, z: sums[s].Z + candidates[index: i].Z);
            counts[s]++;
        }
        for (int s = 0; s < sites.Length; s++)
            if (counts[s] > 0) sites[s] = new Point3d(x: sums[s].X / counts[s], y: sums[s].Y / counts[s], z: sums[s].Z / counts[s]);
        return Fin.Succ(sites);
    }
}
