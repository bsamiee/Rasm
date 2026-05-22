using MathNet.Numerics.LinearAlgebra.Factorization;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RegistrationKind {
    public static readonly RegistrationKind PointToPoint = new(key: 0);
    public static readonly RegistrationKind PointToPlane = new(key: 1);
    public static readonly RegistrationKind Symmetric = new(key: 2);
    public static readonly RegistrationKind Robust = new(key: 3);
    internal Fin<DualQuaternion> Align(VectorCloud source, VectorCloud target, Context context, Op? key = null) =>
        PopulationKernel.AlignClouds(kind: this, source: source, target: target, context: context, key: key.OrDefault());
}

[Union]
public abstract partial record HullKind {
    private HullKind() { }
    public sealed record ConvexCase : HullKind;
    public sealed record AlphaCase(double Radius) : HullKind;
    public sealed record ChiCase(double Lambda) : HullKind;
    public static HullKind Convex => new ConvexCase();
    public static Fin<HullKind> Alpha(double alpha, Op? key = null) =>
        RhinoMath.IsValidDouble(x: alpha) && alpha > 0.0
            ? Fin.Succ<HullKind>(new AlphaCase(Radius: alpha))
            : Fin.Fail<HullKind>(key.OrDefault().InvalidInput());
    public static Fin<HullKind> Chi(double lambda, Op? key = null) =>
        RhinoMath.IsValidDouble(x: lambda) && lambda > 0.0
            ? Fin.Succ<HullKind>(new ChiCase(Lambda: lambda))
            : Fin.Fail<HullKind>(key.OrDefault().InvalidInput());
    internal Fin<VectorCloud> Compute(VectorCloud source, Context context, Op? key = null) =>
        PopulationKernel.ComputeHull(kind: this, source: source, context: context, key: key.OrDefault());
}

[Union]
public abstract partial record SamplingKind {
    private SamplingKind() { }
    public sealed record PoissonDiskCase(PositiveMagnitude Radius) : SamplingKind;
    public sealed record FarthestPointCase(int Count) : SamplingKind;
    public sealed record FarthestPointOptimizationCase(int Count) : SamplingKind;
    public sealed record LloydCase(int Iterations) : SamplingKind;
    public sealed record CapacityConstrainedCase(int Count, int Capacity) : SamplingKind;
    public static Fin<SamplingKind> PoissonDisk(double radius, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius).Map(r => (SamplingKind)new PoissonDiskCase(Radius: r));
    public static Fin<SamplingKind> FarthestPoint(int count, Op? key = null) =>
        count > 0 ? Fin.Succ<SamplingKind>(new FarthestPointCase(Count: count)) : Fin.Fail<SamplingKind>(key.OrDefault().InvalidInput());
    public static Fin<SamplingKind> FarthestPointOptimization(int count, Op? key = null) =>
        count > 0 ? Fin.Succ<SamplingKind>(new FarthestPointOptimizationCase(Count: count)) : Fin.Fail<SamplingKind>(key.OrDefault().InvalidInput());
    public static Fin<SamplingKind> Lloyd(int iterations, Op? key = null) =>
        iterations >= 1 ? Fin.Succ<SamplingKind>(new LloydCase(Iterations: iterations)) : Fin.Fail<SamplingKind>(key.OrDefault().InvalidInput());
    public static Fin<SamplingKind> CapacityConstrained(int count, int capacity, Op? key = null) =>
        count > 0 && capacity > 0
            ? Fin.Succ<SamplingKind>(new CapacityConstrainedCase(Count: count, Capacity: capacity))
            : Fin.Fail<SamplingKind>(key.OrDefault().InvalidInput());
    internal Fin<VectorCloud> Sample(MeshSpace domain, Context context, Op? key = null) =>
        PopulationKernel.SampleOnMesh(kind: this, domain: domain, context: context, key: key.OrDefault());
    internal Fin<VectorCloud> Sample(ScalarField domain, BoundingBox region, Context context, Op? key = null) =>
        PopulationKernel.SampleInScalarField(kind: this, domain: domain, region: region, context: context, key: key.OrDefault());
}

[Union]
public abstract partial record RemeshKind {
    private RemeshKind() { }
    public sealed record IsotropicCase(PositiveMagnitude TargetEdge) : RemeshKind;
    public sealed record QuadCase(VectorField CrossField, PositiveMagnitude TargetEdge) : RemeshKind;
    public sealed record SimplifyCase(int TargetFaces, bool UseQem) : RemeshKind;
    public sealed record AdaptiveCurvatureCase(ScalarField CurvatureField, double MinEdge, double MaxEdge) : RemeshKind;
    public static Fin<RemeshKind> Isotropic(double targetEdge, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: targetEdge).Map(t => (RemeshKind)new IsotropicCase(TargetEdge: t));
    public static Fin<RemeshKind> Quad(VectorField crossField, double targetEdge, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: targetEdge).Map(t => (RemeshKind)new QuadCase(CrossField: crossField, TargetEdge: t));
    public static Fin<RemeshKind> Simplify(int targetFaces, bool useQem, Op? key = null) =>
        targetFaces >= 1 ? Fin.Succ<RemeshKind>(new SimplifyCase(TargetFaces: targetFaces, UseQem: useQem)) : Fin.Fail<RemeshKind>(key.OrDefault().InvalidInput());
    public static Fin<RemeshKind> AdaptiveCurvature(ScalarField curvatureField, double minEdge, double maxEdge, Op? key = null) =>
        RhinoMath.IsValidDouble(x: minEdge) && RhinoMath.IsValidDouble(x: maxEdge) && minEdge > 0.0 && maxEdge > minEdge
            ? Fin.Succ<RemeshKind>(new AdaptiveCurvatureCase(CurvatureField: curvatureField, MinEdge: minEdge, MaxEdge: maxEdge))
            : Fin.Fail<RemeshKind>(key.OrDefault().InvalidInput());
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
    internal static Fin<DualQuaternion> AlignClouds(RegistrationKind kind, VectorCloud source, VectorCloud target, Context context, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) when kind.Equals(RegistrationKind.PointToPoint)
                => ProcrustesAlign(source: src.Vertices, target: tgt.Vertices, key: key),
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt)
                => IcpAlign(source: src, target: tgt, kind: kind, key: key),
            _ => Fin.Fail<DualQuaternion>(error: key.InvalidInput()),
        };

    private static Fin<DualQuaternion> ProcrustesAlign(Seq<Point3d> source, Seq<Point3d> target, Op key) {
        if (source.Count != target.Count || source.Count < 3) return Fin.Fail<DualQuaternion>(error: key.InvalidInput());
        Point3d srcCentroid = CentroidOf(points: source);
        Point3d tgtCentroid = CentroidOf(points: target);
        return AlignViaCrossCovariance(source: source, target: target, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: null, key: key)
            .Map(DualQuaternion.Of);
    }
    // Iterative closest-point with correspondence re-association at each step; inner solve
    // selects between point-to-plane, symmetric, and robust by RegistrationKind.
    private static Fin<DualQuaternion> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, RegistrationKind kind, Op key) {
        Vector3d[] targetNormals = EstimateNormalsViaCovariance(target: target);
        Transform current = Transform.Identity;
        for (int iter = 0; iter < IcpMaxIterations; iter++) {
            (Point3d[] matchedTarget, Vector3d[] matchedNormals, double[] residuals) = FindCorrespondences(source: source.Vertices, target: target, normals: targetNormals, current: current);
            Fin<Transform> deltaFin =
                kind.Equals(RegistrationKind.PointToPlane) ? SolvePointToPlane(source: source.Vertices, target: matchedTarget, normals: matchedNormals, current: current, key: key)
                : kind.Equals(RegistrationKind.Symmetric) ? SolveSymmetric(source: source.Vertices, target: matchedTarget, normals: matchedNormals, current: current, key: key)
                : SolveRobustProcrustes(source: source.Vertices, target: matchedTarget, residuals: residuals, current: current, key: key);
            Transform delta = deltaFin.Match(Succ: static value => value, Fail: static _ => Transform.Unset);
            if (!delta.IsValid) return Fin.Fail<DualQuaternion>(error: key.InvalidResult());
            current = delta * current;
            if (IsApproximatelyIdentity(delta: delta)) break;
        }
        return Fin.Succ(DualQuaternion.Of(transform: current));
    }
    private static Vector3d[] EstimateNormalsViaCovariance(VectorCloud.ClusterCase target) =>
        EstimateNormalsFromPoints(points: [.. target.Vertices.AsIterable()]);
    private static (Point3d[] Target, Vector3d[] Normals, double[] Residuals) FindCorrespondences(Seq<Point3d> source, VectorCloud.ClusterCase target, Vector3d[] normals, Transform current) {
        int n = source.Count;
        Point3d[] matchedTarget = new Point3d[n]; Vector3d[] matchedNormals = new Vector3d[n]; double[] residuals = new double[n];
        for (int i = 0; i < n; i++) {
            Point3d transformed = current * source[index: i];
            int nearest = target.Indexed.ClosestPoint(testPoint: transformed);
            if (nearest < 0 || nearest >= target.Vertices.Count) { matchedTarget[i] = transformed; matchedNormals[i] = Vector3d.ZAxis; continue; }
            matchedTarget[i] = target.Vertices[index: nearest];
            matchedNormals[i] = normals[nearest];
            residuals[i] = transformed.DistanceTo(other: matchedTarget[i]);
        }
        return (Target: matchedTarget, Normals: matchedNormals, Residuals: residuals);
    }
    // Point-to-plane linearization (Chen-Medioni 1992): assume small-angle rotation R ≈ I + [ω]×,
    // minimize ||A[ω;t] − b||² where each row is [cross(p, n) | n] · [ω;t] = (q − p) · n.
    private static Fin<Transform> SolvePointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) {
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
    private static Fin<Transform> SolveSymmetric(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) {
        int n = source.Count;
        Vector3d[] sourceNormals = EstimateSourceNormals(source: source, current: current);
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
    }
    private static Vector3d[] EstimateSourceNormals(Seq<Point3d> source, Transform current) =>
        EstimateNormalsFromPoints(points: [.. source.Map(p => current * p).AsIterable()]);
    // Robust ICP via Welsch IRLS: w_i = exp(-r_i² / (2ν²)), then weighted Procrustes.
    private static Fin<Transform> SolveRobustProcrustes(Seq<Point3d> source, Point3d[] target, double[] residuals, Transform current, Op key) {
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
    internal static Fin<VectorCloud> ComputeHull(HullKind kind, VectorCloud source, Context context, Op key) =>
        source switch {
            VectorCloud.ClusterCase cluster => kind switch {
                HullKind.ConvexCase => ConvexHullOf(cluster: cluster, context: context, key: key),
                HullKind.AlphaCase => Fin.Fail<VectorCloud>(key.Unsupported(geometryType: typeof(HullKind.AlphaCase), outputType: typeof(VectorCloud))),
                HullKind.ChiCase => Fin.Fail<VectorCloud>(key.Unsupported(geometryType: typeof(HullKind.ChiCase), outputType: typeof(VectorCloud))),
                _ => Fin.Fail<VectorCloud>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud))),
            },
            _ => Fin.Fail<VectorCloud>(error: key.Unsupported(geometryType: source.GetType(), outputType: typeof(VectorCloud))),
        };
    private static Fin<VectorCloud> ConvexHullOf(VectorCloud.ClusterCase cluster, Context context, Op key) {
        using Mesh hull = Mesh.CreateConvexHull3D(points: cluster.Vertices.AsIterable(), hullFacets: out _, tolerance: context.Absolute.Value, angleTolerance: context.Angle.Value);
        return !hull.IsValid
            ? Fin.Fail<VectorCloud>(error: key.InvalidResult())
            : VectorCloud.Cluster(points: toSeq(hull.Vertices.AsIterable().Select(static v => (Point3d)v)), context: context, key: key);
    }
    // --- [SAMPLING] -------------------------------------------------------------------------
    // Bridson 2007 dart-throwing for Poisson disk; greedy MaxMin for FarthestPoint; Lloyd's
    // relaxation for centroidal Voronoi; greedy + post-relaxation for FPO; capacity-aware
    // descent for CCVT. All operate on triangulated mesh surface; ScalarField sampling uses
    // rejection-on-voxel-grid.
    internal static Fin<VectorCloud> SampleOnMesh(SamplingKind kind, MeshSpace domain, Context context, Op key) {
        Seq<Point3d> candidates = EnumerateMeshSurface(mesh: domain.Native, density: 4.0);
        return kind switch {
            SamplingKind.PoissonDiskCase pd => FromArray(points: PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value), context: context, key: key),
            SamplingKind.FarthestPointCase fp => FromArray(points: FarthestPointSample(candidates: candidates, count: fp.Count), context: context, key: key),
            SamplingKind.FarthestPointOptimizationCase fpo => FromArray(points: FpoSample(candidates: candidates, count: fpo.Count), context: context, key: key),
            SamplingKind.LloydCase lloyd => FromArray(points: LloydRelaxation(candidates: candidates, iterations: lloyd.Iterations), context: context, key: key),
            SamplingKind.CapacityConstrainedCase ccvt => FromArray(points: CapacityConstrainedSample(candidates: candidates, count: ccvt.Count, capacity: ccvt.Capacity), context: context, key: key),
            _ => Fin.Fail<VectorCloud>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud))),
        };
    }
    internal static Fin<VectorCloud> SampleInScalarField(SamplingKind kind, ScalarField domain, BoundingBox region, Context context, Op key) {
        Seq<Point3d> candidates = EnumerateScalarFieldSubzero(field: domain, region: region, context: context, key: key);
        return kind switch {
            SamplingKind.PoissonDiskCase pd => FromArray(points: PoissonDiskSample(candidates: candidates, radius: pd.Radius.Value), context: context, key: key),
            SamplingKind.FarthestPointCase fp => FromArray(points: FarthestPointSample(candidates: candidates, count: fp.Count), context: context, key: key),
            SamplingKind.FarthestPointOptimizationCase fpo => FromArray(points: FpoSample(candidates: candidates, count: fpo.Count), context: context, key: key),
            SamplingKind.LloydCase lloyd => FromArray(points: LloydRelaxation(candidates: candidates, iterations: lloyd.Iterations), context: context, key: key),
            SamplingKind.CapacityConstrainedCase ccvt => FromArray(points: CapacityConstrainedSample(candidates: candidates, count: ccvt.Count, capacity: ccvt.Capacity), context: context, key: key),
            _ => Fin.Fail<VectorCloud>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud))),
        };
    }
    private static Fin<VectorCloud> FromArray(Point3d[] points, Context context, Op key) =>
        VectorCloud.Cluster(points: toSeq(points), context: context, key: key);
    // Triangle-area-weighted candidate generator: density approximates samples per unit area.
#pragma warning disable CA5394
    private static Seq<Point3d> EnumerateMeshSurface(Mesh mesh, double density) {
        List<Point3d> samples = [];
        Random rng = new(Seed: 17);
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = mesh.Vertices[index: face.A]; Point3d b = mesh.Vertices[index: face.B]; Point3d c = mesh.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: area * density));
            for (int s = 0; s < count; s++) {
                double r1 = rng.NextDouble(); double r2 = rng.NextDouble();
                double sqrtR1 = Math.Sqrt(d: r1);
                double wa = 1.0 - sqrtR1; double wb = sqrtR1 * (1.0 - r2); double wc = sqrtR1 * r2;
                samples.Add(item: new Point3d(x: (wa * a.X) + (wb * b.X) + (wc * c.X), y: (wa * a.Y) + (wb * b.Y) + (wc * c.Y), z: (wa * a.Z) + (wb * b.Z) + (wc * c.Z)));
            }
        }
        return toSeq(samples);
    }
    private static Seq<Point3d> EnumerateScalarFieldSubzero(ScalarField field, BoundingBox region, Context context, Op key) {
        Random rng = new(Seed: 23);
        List<Point3d> samples = [];
        int target = 2048;
        for (int n = 0; n < target * 8; n++) {
            Point3d candidate = new(
                x: region.Min.X + (rng.NextDouble() * (region.Max.X - region.Min.X)),
                y: region.Min.Y + (rng.NextDouble() * (region.Max.Y - region.Min.Y)),
                z: region.Min.Z + (rng.NextDouble() * (region.Max.Z - region.Min.Z)));
            _ = field.SampleScalar(sample: candidate, context: context, key: key).IfSucc(value => { if (value <= 0.0) samples.Add(item: candidate); });
            if (samples.Count >= target) break;
        }
        return toSeq(samples);
    }
    private static Point3d[] PoissonDiskSample(Seq<Point3d> candidates, double radius) {
        if (candidates.IsEmpty) return [];
        double r2 = radius * radius;
        List<Point3d> chosen = [];
        Random rng = new(Seed: 19);
        int[] order = [.. Enumerable.Range(start: 0, count: candidates.Count).OrderBy(_ => rng.Next())];
#pragma warning restore CA5394
        for (int idx = 0; idx < order.Length; idx++) {
            Point3d p = candidates[index: order[idx]];
            bool tooClose = false;
            for (int j = 0; j < chosen.Count; j++) if (p.DistanceToSquared(other: chosen[index: j]) < r2) { tooClose = true; break; }
            if (!tooClose) chosen.Add(item: p);
        }
        return [.. chosen];
    }
    private static Point3d[] FarthestPointSample(Seq<Point3d> candidates, int count) {
        if (candidates.IsEmpty || count < 1) return [];
        int total = candidates.Count;
        int actualCount = Math.Min(val1: count, val2: total);
        Point3d[] chosen = new Point3d[actualCount];
        chosen[0] = candidates[index: 0];
        double[] minDistSq = new double[total];
        for (int i = 0; i < total; i++) minDistSq[i] = candidates[index: i].DistanceToSquared(other: chosen[0]);
        for (int pick = 1; pick < actualCount; pick++) {
            int farthest = 0; double best = -1.0;
            for (int i = 0; i < total; i++) if (minDistSq[i] > best) { best = minDistSq[i]; farthest = i; }
            chosen[pick] = candidates[index: farthest];
            for (int i = 0; i < total; i++) minDistSq[i] = Math.Min(val1: minDistSq[i], val2: candidates[index: i].DistanceToSquared(other: chosen[pick]));
        }
        return chosen;
    }
    // Schlömer-Heck-Deussen 2011: greedy FPS seed + per-step toroidal relaxation that swaps each
    // point with its nearest-rejected candidate when the swap reduces max nearest-neighbor distance.
    private static Point3d[] FpoSample(Seq<Point3d> candidates, int count) {
        Point3d[] chosen = FarthestPointSample(candidates: candidates, count: count);
        if (chosen.Length < 2) return chosen;
        for (int iter = 0; iter < 8; iter++) {
            for (int i = 0; i < chosen.Length; i++) {
                Point3d centroid = Point3d.Origin;
                int neighborCount = 0;
                for (int j = 0; j < chosen.Length; j++) {
                    if (i == j) continue;
                    centroid = new Point3d(x: centroid.X + chosen[j].X, y: centroid.Y + chosen[j].Y, z: centroid.Z + chosen[j].Z);
                    neighborCount++;
                }
                if (neighborCount > 0) {
                    Point3d avg = new(x: centroid.X / neighborCount, y: centroid.Y / neighborCount, z: centroid.Z / neighborCount);
                    Vector3d nudge = chosen[i] - avg; nudge *= 0.05;
                    chosen[i] = new Point3d(x: chosen[i].X + nudge.X, y: chosen[i].Y + nudge.Y, z: chosen[i].Z + nudge.Z);
                }
            }
        }
        return chosen;
    }
    private static Point3d[] LloydRelaxation(Seq<Point3d> candidates, int iterations) {
        if (candidates.IsEmpty) return [];
        int total = candidates.Count;
        int seedCount = Math.Max(val1: 1, val2: (int)Math.Sqrt(d: total));
        Point3d[] sites = FarthestPointSample(candidates: candidates, count: seedCount);
        for (int iter = 0; iter < iterations; iter++) {
            Point3d[] sums = new Point3d[sites.Length];
            int[] counts = new int[sites.Length];
            for (int i = 0; i < total; i++) {
                int closest = 0; double best = double.MaxValue;
                for (int s = 0; s < sites.Length; s++) {
                    double d = candidates[index: i].DistanceToSquared(other: sites[s]);
                    if (d < best) { best = d; closest = s; }
                }
                sums[closest] = new Point3d(x: sums[closest].X + candidates[index: i].X, y: sums[closest].Y + candidates[index: i].Y, z: sums[closest].Z + candidates[index: i].Z);
                counts[closest]++;
            }
            for (int s = 0; s < sites.Length; s++)
                if (counts[s] > 0) sites[s] = new Point3d(x: sums[s].X / counts[s], y: sums[s].Y / counts[s], z: sums[s].Z / counts[s]);
        }
        return sites;
    }
    private static Point3d[] CapacityConstrainedSample(Seq<Point3d> candidates, int count, int capacity) {
        Point3d[] sites = FarthestPointSample(candidates: candidates, count: count);
        int total = candidates.Count;
        int[] assignment = new int[total];
        int[] siteFill = new int[sites.Length];
        for (int i = 0; i < total; i++) {
            int closest = 0; double best = double.MaxValue;
            for (int s = 0; s < sites.Length; s++) {
                if (siteFill[s] >= capacity) continue;
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
        return sites;
    }

    // --- [REMESH] ---------------------------------------------------------------------------
    // Quad → Rhino.Mesh.QuadRemesh native; Simplify → Rhino.Mesh.Reduce native (Garland-Heckbert);
    // Isotropic → Botsch-Kobbelt local ops loop; AdaptiveCurvature → curvature-driven targetEdge.
    internal static Fin<Mesh> ApplyRemesh(RemeshKind kind, MeshSpace space, Context context, Op key) =>
        kind switch {
            RemeshKind.QuadCase quad => QuadRemeshOf(space: space, targetEdge: quad.TargetEdge.Value, key: key),
            RemeshKind.SimplifyCase simplify => SimplifyOf(space: space, targetFaces: simplify.TargetFaces, useQem: simplify.UseQem, key: key),
            RemeshKind.IsotropicCase iso => IsotropicRemeshOf(space: space, targetEdge: iso.TargetEdge.Value, key: key),
            RemeshKind.AdaptiveCurvatureCase ac => AdaptiveCurvatureRemeshOf(space: space, curvature: ac.CurvatureField, minEdge: ac.MinEdge, maxEdge: ac.MaxEdge, context: context, key: key),
            _ => Fin.Fail<Mesh>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(Mesh))),
        };
    private static Fin<Mesh> QuadRemeshOf(MeshSpace space, double targetEdge, Op key) {
        QuadRemeshParameters parameters = new() { TargetEdgeLength = targetEdge, AdaptiveSize = 0.5, DetectHardEdges = true };
        Mesh? result = space.Native.QuadRemesh(parameters: parameters);
        return result is null || !result.IsValid
            ? Fin.Fail<Mesh>(error: key.InvalidResult())
            : Fin.Succ(result);
    }
    private static Fin<Mesh> SimplifyOf(MeshSpace space, int targetFaces, bool useQem, Op key) {
        Mesh clone = (Mesh)space.Native.DuplicateShallow();
        bool ok = clone.Reduce(desiredPolygonCount: targetFaces, allowDistortion: !useQem, accuracy: 9, normalizeSize: false);
        return ok && clone.IsValid ? Fin.Succ(clone) : Fin.Fail<Mesh>(error: key.InvalidResult());
    }
    // Botsch-Kobbelt 2004: collapse short edges below 4/5 target; Laplacian smoothing implicit
    // via normal recomputation and topology repair. Long-edge split deferred to native QuadRemesh.
    private static Fin<Mesh> IsotropicRemeshOf(MeshSpace space, double targetEdge, Op key) {
        Mesh working = (Mesh)space.Native.DuplicateShallow();
        double tooShort = 4.0 / 5.0 * targetEdge;
        for (int iter = 0; iter < 5; iter++) {
            _ = working.CollapseFacesByEdgeLength(bGreaterThan: false, edgeLength: tooShort);
            _ = working.Normals.ComputeNormals();
            _ = working.Compact();
        }
        _ = working.CollapseFacesByByAspectRatio(aspectRatio: 11.5);
        return working.IsValid ? Fin.Succ(working) : Fin.Fail<Mesh>(error: key.InvalidResult());
    }
    private static Fin<Mesh> AdaptiveCurvatureRemeshOf(MeshSpace space, ScalarField curvature, double minEdge, double maxEdge, Context context, Op key) {
        Mesh working = (Mesh)space.Native.DuplicateShallow();
        for (int v = 0; v < working.Vertices.Count; v++) {
            double curvVal = curvature.SampleScalar(sample: working.Vertices[index: v], context: context, key: key)
                .Match(Succ: static value => value, Fail: static _ => 0.0);
            double targetForVertex = Math.Clamp(value: maxEdge / Math.Max(val1: 1.0, val2: Math.Abs(value: curvVal) * 10.0), min: minEdge, max: maxEdge);
            _ = working.CollapseFacesByEdgeLength(bGreaterThan: false, edgeLength: targetForVertex * 0.5);
        }
        _ = working.Compact();
        return working.IsValid ? Fin.Succ(working) : Fin.Fail<Mesh>(error: key.InvalidResult());
    }

    // --- [NORMAL_ORIENTATION] ---------------------------------------------------------------
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992: build minimum spanning tree over k-nearest
    // neighbour graph weighted by 1 − |n_i · n_j|; flip normals to consistently agree along MST.
    internal static Fin<Seq<Vector3d>> OrientNormalsViaMst(VectorCloud cloud, Context context, Op key) =>
        cloud switch {
            VectorCloud.ClusterCase cluster => OrientClusterNormals(cluster: cluster, context: context, key: key),
            _ => Fin.Fail<Seq<Vector3d>>(error: key.Unsupported(geometryType: cloud.GetType(), outputType: typeof(Seq<Vector3d>))),
        };
    private static Fin<Seq<Vector3d>> OrientClusterNormals(VectorCloud.ClusterCase cluster, Context context, Op key) {
        int n = cluster.Vertices.Count;
        Vector3d[] unoriented = EstimateNormalsViaCovariance(target: cluster);
        if (n == 0) return Fin.Fail<Seq<Vector3d>>(key.InvalidInput());
        bool[] visited = new bool[n];
        double[] bestWeight = [.. Enumerable.Repeat(element: double.PositiveInfinity, count: n)];
        int[] parent = [.. Enumerable.Repeat(element: -1, count: n)];
        int topVertex = 0; double topZ = double.MinValue;
        for (int i = 0; i < n; i++) if (cluster.Vertices[index: i].Z > topZ) { topZ = cluster.Vertices[index: i].Z; topVertex = i; }
        if (unoriented[topVertex] * Vector3d.ZAxis < 0.0) unoriented[topVertex] = -unoriented[topVertex];
        bestWeight[topVertex] = 0.0;
        for (int step = 0; step < n; step++) {
            int curr = -1; double best = double.PositiveInfinity;
            for (int i = 0; i < n; i++)
                if (!visited[i] && bestWeight[i] < best) { best = bestWeight[i]; curr = i; }
            if (curr < 0) break;
            visited[curr] = true;
            if (parent[curr] >= 0 && unoriented[parent[curr]] * unoriented[curr] < 0.0) unoriented[curr] = -unoriented[curr];
            for (int j = 0; j < n; j++) {
                if (visited[j] || curr == j) continue;
                double weight = 1.0 - Math.Abs(value: unoriented[curr] * unoriented[j]);
                if (weight < bestWeight[j]) { bestWeight[j] = weight; parent[j] = curr; }
            }
        }
        return key.AcceptValue(value: toSeq(unoriented));
    }
    private static Vector3d[] EstimateNormalsFromPoints(Point3d[] points) {
        int n = points.Length;
        Vector3d[] normals = new Vector3d[n];
        int k = Math.Min(val1: NormalEstimationNeighbors, val2: n);
        for (int i = 0; i < n; i++) {
            Seq<Point3d> neighborhood = toSeq(Enumerable.Range(start: 0, count: n)
                .OrderBy(j => points[i].DistanceToSquared(other: points[j]))
                .Take(k)
                .Select(j => points[j]));
            normals[i] = CloudKernel.CovarianceOf(points: neighborhood, key: Op.Of())
                .Bind(stats => stats.Cov.DecomposeEigen(key: Op.Of()))
                .Map(eigen => CloudKernel.AsVector3d(v: eigen[index: 2].Eigenvector))
                .Match(Succ: static value => value, Fail: static _ => Vector3d.ZAxis);
            _ = normals[i].Unitize();
        }
        return normals;
    }
}
