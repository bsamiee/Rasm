using MathNet.Numerics.LinearAlgebra.Factorization;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AlignKind {
    public static readonly AlignKind Point = new(key: 0,
        solveStep: static (source, target, _, _, current, key) => AlignKernel.SolvePointToPoint(source: source, target: target, current: current, key: key));
    public static readonly AlignKind Plane = new(key: 1,
        solveStep: static (source, target, normals, _, current, key) => AlignKernel.SolvePointToPlane(source: source, target: target, normals: normals, current: current, key: key));
    public static readonly AlignKind Symmetric = new(key: 2,
        solveStep: static (source, target, normals, _, current, key) => AlignKernel.SolveSymmetric(source: source, target: target, normals: normals, current: current, key: key));
    public static readonly AlignKind Robust = new(key: 3,
        solveStep: static (source, target, _, residuals, current, key) => AlignKernel.SolveRobustProcrustes(source: source, target: target, residuals: residuals, current: current, key: key));
    [UseDelegateFromConstructor] internal partial Fin<Transform> SolveStep(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] residuals, Transform current, Op key);
    internal Fin<Transform> Align(VectorCloud source, VectorCloud target, Op? key = null) =>
        AlignKernel.AlignClouds(kind: this, source: source, target: target, key: key.OrDefault());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class AlignKernel {
    private const int IcpMaxIterations = 30;
    private const double IcpConvergenceTolerance = 1e-6;
    private const double WelschNu = 0.1;

    // --- [ALIGNMENT] ------------------------------------------------------------------------
    // Umeyama 1991 (point-to-point) | Chen-Medioni 1992 (point-to-plane) | Rusinkiewicz 2019
    // (symmetric) | yaoyx689 2020 robust IRLS with Welsch loss. All four share the iterative
    // correspondence-finding outer loop; the inner solve switches on kind.
    internal static Fin<Transform> AlignClouds(AlignKind kind, VectorCloud source, VectorCloud target, Op key) =>
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
    // selects between point-to-plane, symmetric, and robust by AlignKind.
    private static Fin<Transform> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, Op key) =>
        from targetNormals in CloudKernel.EstimateNormalsViaCovariance(target: target, key: key)
        from aligned in IcpAlignWithNormals(source: source, target: target, kind: kind, targetNormals: targetNormals, key: key)
        select aligned;
    private static Fin<Transform> IcpAlignWithNormals(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, Vector3d[] targetNormals, Op key) {
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
        CloudKernel.EstimateNormalsFromPoints(points: [.. source.Map(p => current * p).AsIterable()], key: key);
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
}
