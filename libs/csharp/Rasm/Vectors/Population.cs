using MathNet.Numerics.LinearAlgebra.Factorization;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RegistrationKind {
    public static readonly RegistrationKind PointToPoint = new(key: 0,
        solveStep: static (source, target, _, _, current, key) => PopulationKernel.SolvePointToPoint(source: source, target: target, current: current, key: key));
    public static readonly RegistrationKind PointToPlane = new(key: 1,
        solveStep: static (source, target, normals, _, current, key) => PopulationKernel.SolvePointToPlane(source: source, target: target, normals: normals, current: current, key: key));
    public static readonly RegistrationKind Symmetric = new(key: 2,
        solveStep: static (source, target, normals, _, current, key) => PopulationKernel.SolveSymmetric(source: source, target: target, normals: normals, current: current, key: key));
    public static readonly RegistrationKind Robust = new(key: 3,
        solveStep: static (source, target, _, residuals, current, key) => PopulationKernel.SolveRobustProcrustes(source: source, target: target, residuals: residuals, current: current, key: key));
    [UseDelegateFromConstructor] internal partial Fin<Transform> SolveStep(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] residuals, Transform current, Op key);
    internal Fin<Transform> Align(VectorCloud source, VectorCloud target, Context context, Op? key = null) =>
        PopulationKernel.AlignClouds(kind: this, source: source, target: target, context: context, key: key.OrDefault());
}

[Union]
public abstract partial record SamplingKind {
    private SamplingKind() { }
    public sealed record PoissonDiskCase(PositiveMagnitude Radius) : SamplingKind;
    public sealed record FarthestPointCase(Dimension Count) : SamplingKind;
    public sealed record FarthestPointOptimizationCase(Dimension Count, Dimension Iterations) : SamplingKind;
    public sealed record LloydCase(Dimension Count, Dimension Iterations) : SamplingKind;
    public sealed record CapacityConstrainedCase(Dimension Count, Dimension Capacity) : SamplingKind;
    public static Fin<SamplingKind> PoissonDisk(double radius, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius).Map(r => (SamplingKind)new PoissonDiskCase(Radius: r));
    public static Fin<SamplingKind> FarthestPoint(int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Map(static value => (SamplingKind)new FarthestPointCase(Count: value));
    }
    public static Fin<SamplingKind> FarthestPointOptimization(int count, int iterations, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from i in op.AcceptValidated<Dimension>(candidate: iterations)
               select (SamplingKind)new FarthestPointOptimizationCase(Count: c, Iterations: i);
    }
    public static Fin<SamplingKind> Lloyd(int count, int iterations, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from i in op.AcceptValidated<Dimension>(candidate: iterations)
               select (SamplingKind)new LloydCase(Count: c, Iterations: i);
    }
    public static Fin<SamplingKind> CapacityConstrained(int count, int capacity, Op? key = null) {
        Op op = key.OrDefault();
        return from c in op.AcceptValidated<Dimension>(candidate: count)
               from cap in op.AcceptValidated<Dimension>(candidate: capacity)
               select (SamplingKind)new CapacityConstrainedCase(Count: c, Capacity: cap);
    }
    internal Fin<VectorCloud> Sample(MeshSpace domain, Context context, Op? key = null) =>
        PopulationKernel.SampleOnMesh(kind: this, domain: domain, context: context, key: key.OrDefault());
    internal double MeshCandidateDensity(double area) {
        double safeArea = Math.Max(val1: area, val2: RhinoMath.SqrtEpsilon);
        double target = this switch {
            PoissonDiskCase pd => safeArea / Math.Max(val1: pd.Radius.Value * pd.Radius.Value, val2: RhinoMath.SqrtEpsilon),
            FarthestPointCase fp => fp.Count.Value,
            FarthestPointOptimizationCase fpo => fpo.Count.Value,
            LloydCase lloyd => lloyd.Count.Value,
            CapacityConstrainedCase ccvt => ccvt.Count.Value * ccvt.Capacity.Value,
            _ => 1.0,
        };
        return Math.Max(val1: target / safeArea, val2: 1.0 / safeArea);
    }
}

[Union]
public abstract partial record RemeshKind {
    private RemeshKind() { }
    public sealed record QuadCase(PositiveMagnitude TargetEdge) : RemeshKind;
    public sealed record SimplifyCase(ReduceMeshParameters Parameters) : RemeshKind;
    public static Fin<RemeshKind> Quad(double targetEdge, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: targetEdge).Map(t => (RemeshKind)new QuadCase(TargetEdge: t));
    public static Fin<RemeshKind> Simplify(ReduceMeshParameters parameters, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(parameters).ToFin(op.InvalidInput())
            .Bind(active => active.DesiredPolygonCount >= 1
                ? Fin.Succ<RemeshKind>(new SimplifyCase(Parameters: active))
                : Fin.Fail<RemeshKind>(op.InvalidInput()));
    }
    internal Fin<Mesh> Apply(MeshSpace space, Context context, Op? key = null) =>
        PopulationKernel.ApplyRemesh(kind: this, space: space, context: context, key: key.OrDefault());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class PopulationKernel {
    private const int IcpMaxIterations = 30;
    private const double IcpConvergenceTolerance = 1e-6;
    private const int NormalEstimationNeighbors = 10;
    private const double WelschNu = 0.1;

    // --- [REGISTRATION] ---------------------------------------------------------------------
    // Umeyama 1991 (point-to-point) | Chen-Medioni 1992 (point-to-plane) | Rusinkiewicz 2019
    // (symmetric) | yaoyx689 2020 robust IRLS with Welsch loss. All four share the iterative
    // correspondence-finding outer loop; the inner solve switches on kind.
    internal static Fin<Transform> AlignClouds(RegistrationKind kind, VectorCloud source, VectorCloud target, Context context, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt)
                => IcpAlign(source: src, target: tgt, kind: kind, key: key),
            _ => Fin.Fail<Transform>(error: key.InvalidInput()),
        };

    private static Fin<Transform> ProcrustesAlign(Seq<Point3d> source, Seq<Point3d> target, Op key) {
        if (source.Count != target.Count || source.Count < 3) return Fin.Fail<Transform>(error: key.InvalidInput());
        Point3d srcCentroid = CentroidOf(points: source);
        Point3d tgtCentroid = CentroidOf(points: target);
        return AlignViaCrossCovariance(source: source, target: target, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: null, key: key);
    }
    // Iterative closest-point with correspondence re-association at each step; inner solve
    // selects between point-to-plane, symmetric, and robust by RegistrationKind.
    private static Fin<Transform> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, RegistrationKind kind, Op key) =>
        from targetNormals in EstimateNormalsViaCovariance(target: target, key: key)
        from aligned in IcpAlignWithNormals(source: source, target: target, kind: kind, targetNormals: targetNormals, key: key)
        select aligned;
    private static Fin<Transform> IcpAlignWithNormals(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, RegistrationKind kind, Vector3d[] targetNormals, Op key) {
        Transform current = Transform.Identity;
        for (int iter = 0; iter < IcpMaxIterations; iter++) {
            (Point3d[] matchedTarget, Vector3d[] matchedNormals, double[] residuals) = FindCorrespondences(source: source.Vertices, target: target, normals: targetNormals, current: current);
            Fin<Transform> deltaFin = kind.SolveStep(source: source.Vertices, target: matchedTarget, normals: matchedNormals, residuals: residuals, current: current, key: key);
            if (deltaFin.IsFail) return deltaFin;
            Transform delta = deltaFin.IfFail(Transform.Unset);
            current = delta * current;
            if (IsApproximatelyIdentity(delta: delta)) break;
        }
        return Fin.Succ(current);
    }
    private static Fin<Vector3d[]> EstimateNormalsViaCovariance(VectorCloud.ClusterCase target, Op key) =>
        EstimateNormalGraph(target: target, key: key).Map(static graph => graph.Normals);
    private static Fin<(Vector3d[] Normals, int[][] Neighbors)> EstimateNormalGraph(VectorCloud.ClusterCase target, Op key) {
        Point3d[] points = [.. target.Vertices.AsIterable()];
        if (points.Length < 3) return Fin.Fail<(Vector3d[], int[][])>(key.InvalidInput());
        int k = Math.Min(val1: NormalEstimationNeighbors, val2: points.Length);
        int[][] neighborhoods = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: points, amount: k)];
        return EstimateNormalsFromPoints(points: points, neighborhoodOf: i => NeighborhoodOf(points: points, ids: neighborhoods.Length > i ? neighborhoods[i] : []), key: key)
            .Map(normals => (Normals: normals, Neighbors: neighborhoods));
    }
    private static (Point3d[] Target, Vector3d[] Normals, double[] Residuals) FindCorrespondences(Seq<Point3d> source, VectorCloud.ClusterCase target, Vector3d[] normals, Transform current) {
        int n = source.Count;
        Point3d[] transformed = [.. source.AsIterable().Select(point => current * point)];
        int[][] nearestIds = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: transformed, amount: 1)];
        Point3d[] matchedTarget = new Point3d[n]; Vector3d[] matchedNormals = new Vector3d[n]; double[] residuals = new double[n];
        for (int i = 0; i < n; i++) {
            int nearest = nearestIds.Length > i && nearestIds[i].Length > 0 ? nearestIds[i][0] : -1;
            if (nearest < 0 || nearest >= target.Vertices.Count) { matchedTarget[i] = transformed[i]; matchedNormals[i] = Vector3d.ZAxis; continue; }
            matchedTarget[i] = target.Vertices[index: nearest];
            matchedNormals[i] = normals[nearest];
            residuals[i] = transformed[i].DistanceTo(other: matchedTarget[i]);
        }
        return (Target: matchedTarget, Normals: matchedNormals, Residuals: residuals);
    }
    internal static Fin<Transform> SolvePointToPoint(Seq<Point3d> source, Point3d[] target, Transform current, Op key) {
        Seq<Point3d> transformedSource = toSeq(source.AsIterable().Select(p => current * p));
        return ProcrustesAlign(source: transformedSource, target: toSeq(target), key: key);
    }
    // Point-to-plane linearization (Chen-Medioni 1992): assume small-angle rotation R ≈ I + [ω]×,
    // minimize ||A[ω;t] − b||² where each row is [cross(p, n) | n] · [ω;t] = (q − p) · n.
    internal static Fin<Transform> SolvePointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) {
        int n = source.Count;
        double[] aFlat = new double[n * 6]; double[] b = new double[n];
        for (int i = 0; i < n; i++) {
            Point3d p = current * source[index: i]; Point3d q = target[i]; Vector3d nrm = normals[i];
            Vector3d cross = Vector3d.CrossProduct(a: (Vector3d)p, b: nrm);
            aFlat[(i * 6) + 0] = cross.X; aFlat[(i * 6) + 1] = cross.Y; aFlat[(i * 6) + 2] = cross.Z;
            aFlat[(i * 6) + 3] = nrm.X; aFlat[(i * 6) + 4] = nrm.Y; aFlat[(i * 6) + 5] = nrm.Z;
            b[i] = (q - p) * nrm;
        }
        return SolveLeastSquares6(aFlat: aFlat, b: b, n: n, key: key).Map(static x => ComposeRigidTransform(omega: new Vector3d(x: x[0], y: x[1], z: x[2]), translation: new Vector3d(x: x[3], y: x[4], z: x[5])));
    }
    // Symmetric objective (Rusinkiewicz 2019): use the half-sum of source and target normals;
    // the symmetric formulation has better convergence for high-curvature surfaces.
    internal static Fin<Transform> SolveSymmetric(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) {
        int n = source.Count;
        return EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => {
            double[] aFlat = new double[n * 6]; double[] b = new double[n];
            for (int i = 0; i < n; i++) {
                Point3d p = current * source[index: i]; Point3d q = target[i];
                Vector3d nrm = (sourceNormals[i] + normals[i]) * 0.5; _ = nrm.Unitize();
                Vector3d cross = Vector3d.CrossProduct(a: (Vector3d)p, b: nrm);
                aFlat[(i * 6) + 0] = cross.X; aFlat[(i * 6) + 1] = cross.Y; aFlat[(i * 6) + 2] = cross.Z;
                aFlat[(i * 6) + 3] = nrm.X; aFlat[(i * 6) + 4] = nrm.Y; aFlat[(i * 6) + 5] = nrm.Z;
                b[i] = (q - p) * nrm;
            }
            return SolveLeastSquares6(aFlat: aFlat, b: b, n: n, key: key).Map(static x => ComposeRigidTransform(omega: new Vector3d(x: x[0], y: x[1], z: x[2]), translation: new Vector3d(x: x[3], y: x[4], z: x[5])));
        });
    }
    private static Fin<Vector3d[]> EstimateSourceNormals(Seq<Point3d> source, Transform current, Op key) =>
        EstimateNormalsFromPoints(points: [.. source.Map(p => current * p).AsIterable()], key: key);
    // Robust ICP via Welsch IRLS: w_i = exp(-r_i² / (2ν²)), then weighted Procrustes.
    internal static Fin<Transform> SolveRobustProcrustes(Seq<Point3d> source, Point3d[] target, double[] residuals, Transform current, Op key) {
        int n = source.Count;
        double[] weights = new double[n];
        double maxResidual = residuals.Length > 0 ? residuals.Max() : 1.0;
        double nu = Math.Max(val1: maxResidual * WelschNu, val2: RhinoMath.SqrtEpsilon);
        for (int i = 0; i < n; i++) weights[i] = Math.Exp(d: -(residuals[i] * residuals[i]) / (2.0 * nu * nu));
        Seq<Point3d> transformedSource = toSeq(source.AsIterable().Select(p => current * p));
        Seq<Point3d> targetSeq = toSeq(target);
        Point3d srcCentroid = WeightedCentroidOf(points: transformedSource, weights: weights);
        Point3d tgtCentroid = WeightedCentroidOf(points: targetSeq, weights: weights);
        return AlignViaCrossCovariance(source: transformedSource, target: targetSeq, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: weights, key: key);
    }
    private static Point3d WeightedCentroidOf(Seq<Point3d> points, double[] weights) {
        Vector3d sum = Vector3d.Zero; double totalW = 0.0;
        for (int i = 0; i < points.Count; i++) { sum += weights[i] * (Vector3d)points[index: i]; totalW += weights[i]; }
        return totalW > RhinoMath.ZeroTolerance ? Point3d.Origin + (sum / totalW) : Point3d.Origin;
    }
    // Cross-covariance H = Σ w_i (s_i − ȳ)(t_i − ȳ)ᵀ → SVD → R = V·diag(1,1,det(VUᵀ))·Uᵀ;
    // post-rotation translation = ȳ − R x̄ assembles the full rigid transform.
    private static Fin<Transform> AlignViaCrossCovariance(Seq<Point3d> source, Seq<Point3d> target, Point3d srcCentroid, Point3d tgtCentroid, double[]? weights, Op key) {
        Dimension dim3 = Dimension.Create(value: 3);
        double[] cross = new double[9];
        for (int i = 0; i < source.Count; i++) {
            double w = weights is null ? 1.0 : weights[i];
            Vector3d sv = source[index: i] - srcCentroid; Vector3d tv = target[index: i] - tgtCentroid;
            cross[0] += w * sv.X * tv.X; cross[1] += w * sv.X * tv.Y; cross[2] += w * sv.X * tv.Z;
            cross[3] += w * sv.Y * tv.X; cross[4] += w * sv.Y * tv.Y; cross[5] += w * sv.Y * tv.Z;
            cross[6] += w * sv.Z * tv.X; cross[7] += w * sv.Z * tv.Y; cross[8] += w * sv.Z * tv.Z;
        }
        return from h in Matrix.Of(rows: dim3, cols: dim3, entries: new Arr<double>(cross), key: key)
               from svd in h.DecomposeSvd(key: key)
               from rotation in BuildRotation(u: svd.U, v: svd.V, key: key)
               select PromoteToTransform(rotation: rotation, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid);
    }
    private static Fin<Transform> BuildRotation(Matrix u, Matrix v, Op key) {
        Matrix vu = v * u.Transpose();
        return from det in vu.Determinant(key: key)
               let diag = new[] { 1.0, 1.0, det >= 0.0 ? 1.0 : -1.0 }
               let d = new Matrix(Rows: Dimension.Create(value: 3), Cols: Dimension.Create(value: 3),
                   Entries: [.. Enumerable.Range(start: 0, count: 9).Select(idx => (idx / 3) == (idx % 3) ? diag[idx / 3] : 0.0)])
               let rot = v * d * u.Transpose()
               select RotationTransformOf(rotation: rot);
    }
    private static Transform RotationTransformOf(Matrix rotation) {
        Transform xform = Transform.Identity;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                xform[i, j] = rotation.At(i: i, j: j);
        return xform;
    }
    private static Transform PromoteToTransform(Transform rotation, Point3d srcCentroid, Point3d tgtCentroid) =>
        WithTranslation(rotation: rotation, translation: tgtCentroid - (rotation * srcCentroid));
    private static Transform WithTranslation(Transform rotation, Vector3d translation) {
        Transform aligned = rotation;
        aligned[0, 3] = translation.X; aligned[1, 3] = translation.Y; aligned[2, 3] = translation.Z;
        return aligned;
    }
    private static Point3d CentroidOf(Seq<Point3d> points) {
        Vector3d sum = points.Fold(initialState: Vector3d.Zero, f: static (acc, p) => acc + (Vector3d)p);
        return Point3d.Origin + (sum / points.Count);
    }
    private static Fin<double[]> SolveLeastSquares6(double[] aFlat, double[] b, int n, Op key) =>
        n < 6
            ? Fin.Fail<double[]>(key.InvalidInput())
            : key.Catch(() => {
                LinearMatrix design = DenseMatrixD.Build.Dense(rows: n, columns: 6, init: (i, j) => aFlat[(i * 6) + j]);
                DenseVectorD rhs = DenseVectorD.OfArray(b);
                QR<double> qr = design.QR(QRMethod.Full);
                return Fin.Succ(qr.Solve(rhs).ToArray());
            });
    // Construct R ≈ I + [ω]× via Rodrigues approximation for small ω; combine with translation.
    private static Transform ComposeRigidTransform(Vector3d omega, Vector3d translation) {
        double theta = omega.Length;
        Transform rot = theta < RhinoMath.SqrtEpsilon
            ? Transform.Identity
            : Transform.Rotation(angleRadians: theta, rotationAxis: omega / theta, rotationCenter: Point3d.Origin);
        Transform result = rot;
        result[0, 3] = translation.X; result[1, 3] = translation.Y; result[2, 3] = translation.Z;
        return result;
    }
    private static bool IsApproximatelyIdentity(Transform delta) {
        double diff = 0.0;
        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) diff += Math.Abs(value: delta[i, j] - (i == j ? 1.0 : 0.0));
        return diff < IcpConvergenceTolerance;
    }

    // --- [HULL] -----------------------------------------------------------------------------
    internal static Fin<Mesh> ComputeHull(VectorCloud source, Context context, Op key) =>
        source switch {
            VectorCloud.ClusterCase cluster => ConvexHullOf(cluster: cluster, context: context, key: key),
            _ => Fin.Fail<Mesh>(error: key.Unsupported(geometryType: source.GetType(), outputType: typeof(Mesh))),
        };
    private static Fin<Mesh> ConvexHullOf(VectorCloud.ClusterCase cluster, Context context, Op key) {
        if (cluster.Vertices.Count < 4 || Point3d.ArePointsCoplanar(points: cluster.Vertices.AsIterable(), tolerance: context.Absolute.Value))
            return Fin.Fail<Mesh>(error: key.InvalidInput());
        using Mesh? hull = Mesh.CreateConvexHull3D(points: cluster.Vertices.AsIterable(), hullFacets: out _, tolerance: context.Absolute.Value, angleTolerance: context.Angle.Value);
        return hull is not { IsValid: true }
            ? Fin.Fail<Mesh>(error: key.InvalidResult())
            : Fin.Succ(hull.DuplicateMesh());
    }
    // --- [SAMPLING] -------------------------------------------------------------------------
    // Boundary algorithms operate on deterministic triangulated mesh candidates.
    internal static Fin<VectorCloud> SampleOnMesh(SamplingKind kind, MeshSpace domain, Context context, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: domain.Native);
        return props switch {
            null => Fin.Fail<VectorCloud>(error: key.InvalidResult()),
            _ => EnumerateMeshSurface(mesh: domain.Native, density: kind.MeshCandidateDensity(area: props.Area)) switch {
                Seq<Point3d> candidates => kind switch {
                    SamplingKind.PoissonDiskCase pd => FromArray(points: PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value), context: context, key: key),
                    SamplingKind.FarthestPointCase fp => FromArray(points: FarthestPointSample(candidates: candidates, count: fp.Count.Value), context: context, key: key),
                    SamplingKind.FarthestPointOptimizationCase fpo => FromArray(points: FpoSample(candidates: candidates, count: fpo.Count.Value, iterations: fpo.Iterations.Value), context: context, key: key),
                    SamplingKind.LloydCase lloyd => FromArray(points: LloydRelaxation(candidates: candidates, count: lloyd.Count.Value, iterations: lloyd.Iterations.Value), context: context, key: key),
                    SamplingKind.CapacityConstrainedCase ccvt => CapacityConstrainedSample(candidates: candidates, count: ccvt.Count.Value, capacity: ccvt.Capacity.Value, key: key)
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
    private static Point3d[] FarthestPointSample(Seq<Point3d> candidates, int count) {
        int[] chosen = FarthestPointIndices(candidates: candidates, count: count);
        return [.. chosen.Select(i => candidates[index: i])];
    }
    private static int[] FarthestPointIndices(Seq<Point3d> candidates, int count) {
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
        int[] chosen = FarthestPointIndices(candidates: candidates, count: count);
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
        Point3d[] sites = FarthestPointSample(candidates: candidates, count: count);
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
    private static Fin<Point3d[]> CapacityConstrainedSample(Seq<Point3d> candidates, int count, int capacity, Op key) {
        Point3d[] sites = FarthestPointSample(candidates: candidates, count: count);
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

    // --- [REMESH] ---------------------------------------------------------------------------
    internal static Fin<Mesh> ApplyRemesh(RemeshKind kind, MeshSpace space, Context context, Op key) =>
        kind switch {
            RemeshKind.QuadCase quad => QuadRemeshOf(space: space, targetEdge: quad.TargetEdge.Value, key: key),
            RemeshKind.SimplifyCase simplify => SimplifyOf(space: space, parameters: simplify.Parameters, key: key),
            _ => Fin.Fail<Mesh>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(Mesh))),
        };
    private static Fin<Mesh> QuadRemeshOf(MeshSpace space, double targetEdge, Op key) {
        QuadRemeshParameters parameters = new() { TargetEdgeLength = targetEdge, AdaptiveSize = 0.5, DetectHardEdges = true };
        Mesh? result = space.Native.QuadRemesh(parameters: parameters);
        return result is null || !result.IsValid
            ? Fin.Fail<Mesh>(error: key.InvalidResult())
            : Fin.Succ(result);
    }
    private static Fin<Mesh> SimplifyOf(MeshSpace space, ReduceMeshParameters parameters, Op key) {
        Mesh clone = space.Native.DuplicateMesh();
        bool ok = clone.Reduce(parameters: parameters);
        return ok && clone.IsValid ? Fin.Succ(clone) : Fin.Fail<Mesh>(error: key.InvalidResult());
    }
    // --- [NORMAL_ORIENTATION] ---------------------------------------------------------------
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992: build minimum spanning tree over k-nearest
    // neighbour graph weighted by 1 − |n_i · n_j|; flip normals to consistently agree along MST.
    internal static Fin<Seq<Vector3d>> OrientNormalsViaMst(VectorCloud cloud, Op key) =>
        cloud switch {
            VectorCloud.ClusterCase cluster => OrientClusterNormals(cluster: cluster, key: key),
            _ => Fin.Fail<Seq<Vector3d>>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(Seq<Vector3d>))),
        };
    private static Fin<Seq<Vector3d>> OrientClusterNormals(VectorCloud.ClusterCase cluster, Op key) {
        int n = cluster.Vertices.Count;
        return n == 0
            ? Fin.Fail<Seq<Vector3d>>(key.InvalidInput())
            : EstimateNormalGraph(target: cluster, key: key).Bind(((Vector3d[] Normals, int[][] Neighbors) graph) => {
                bool[] visited = new bool[n];
                double[] bestWeight = [.. Enumerable.Repeat(element: double.PositiveInfinity, count: n)];
                int[] parent = [.. Enumerable.Repeat(element: -1, count: n)];
                for (int root = 0; root < n; root++) {
                    if (visited[root]) continue;
                    bestWeight[root] = 0.0;
                    for (int step = 0; step < n; step++) {
                        int curr = -1; double best = double.PositiveInfinity;
                        for (int i = 0; i < n; i++)
                            if (!visited[i] && bestWeight[i] < best) { best = bestWeight[i]; curr = i; }
                        if (curr < 0 || double.IsPositiveInfinity(best)) break;
                        visited[curr] = true;
                        if (parent[curr] >= 0 && graph.Normals[parent[curr]] * graph.Normals[curr] < 0.0) graph.Normals[curr] = -graph.Normals[curr];
                        int[] neighbors = graph.Neighbors.Length > curr ? graph.Neighbors[curr] : [];
                        for (int e = 0; e < neighbors.Length; e++) {
                            int j = neighbors[e];
                            if (j < 0 || j >= n || visited[j] || curr == j) continue;
                            double weight = 1.0 - Math.Abs(value: graph.Normals[curr] * graph.Normals[j]);
                            if (weight < bestWeight[j]) { bestWeight[j] = weight; parent[j] = curr; }
                        }
                    }
                }
                return toSeq(graph.Normals).TraverseM(normal => key.AcceptValue(value: normal)).As();
            });
    }
    private static Fin<Vector3d[]> EstimateNormalsFromPoints(Point3d[] points, Op key) =>
        EstimateNormalsFromPoints(points: points, neighborhoodOf: BatchedNeighborhoods(points: points), key: key);
    private static Func<int, Seq<Point3d>> BatchedNeighborhoods(Point3d[] points) {
        PointCloud cloud = [];
        cloud.AddRange(points: points);
        int k = Math.Min(val1: NormalEstimationNeighbors, val2: points.Length);
        int[][] ids = [.. RTree.PointCloudKNeighbors(pointcloud: cloud, needlePts: points, amount: k)];
        return i => NeighborhoodOf(points: points, ids: ids.Length > i ? ids[i] : []);
    }
    private static Seq<Point3d> NeighborhoodOf(Point3d[] points, int[] ids) =>
        ids.Length == 0
            ? toSeq(points.Take(Math.Min(val1: NormalEstimationNeighbors, val2: points.Length)))
            : toSeq(ids.Where(i => i >= 0 && i < points.Length).Select(i => points[i]));
    private static Fin<Vector3d[]> EstimateNormalsFromPoints(Point3d[] points, Func<int, Seq<Point3d>> neighborhoodOf, Op key) {
        int n = points.Length;
        if (n < 3) return Fin.Fail<Vector3d[]>(key.InvalidInput());
        Vector3d[] normals = new Vector3d[n];
        for (int i = 0; i < n; i++) {
            Seq<Point3d> neighborhood = neighborhoodOf(arg: i);
            if (neighborhood.Count < 3) return Fin.Fail<Vector3d[]>(key.InvalidInput());
            Fin<Vector3d> normal =
                from stats in CloudKernel.CovarianceOf(points: neighborhood, key: key)
                from eigen in stats.Cov.DecomposeEigen(key: key)
                from _ in eigen.Count >= 3 && eigen[index: 1].Eigenvalue > RhinoMath.SqrtEpsilon
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(key.InvalidResult())
                let raw = CloudKernel.AsVector3d(v: eigen[index: 2].Eigenvector)
                from direction in Direction.Of(value: raw, tolerance: RhinoMath.ZeroTolerance, key: key)
                select direction.Value;
            if (normal.IsFail) return normal.Map(static value => new[] { value });
            normals[i] = normal.IfFail(Vector3d.Unset);
        }
        return Fin.Succ(normals);
    }
}
