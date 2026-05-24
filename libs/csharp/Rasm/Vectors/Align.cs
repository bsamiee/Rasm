using Foundation.CSharp.Analyzers.Contracts;
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
    public static readonly AlignKind NormalWeightedPointToPlane = new(key: 4,
        solveStep: static (source, target, normals, _, current, key) => AlignKernel.SolveNormalWeightedPointToPlane(source: source, target: target, targetNormals: normals, current: current, key: key));
    [UseDelegateFromConstructor] internal partial Fin<Transform> SolveStep(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] residuals, Transform current, Op key);
    internal Fin<Transform> Align(VectorCloud source, VectorCloud target, Op? key = null) =>
        AlignKernel.AlignClouds(kind: this, source: source, target: target, key: key.OrDefault())
            .Bind(receipt => receipt.Stop.Equals(AlignmentStopKind.Converged)
                ? Fin.Succ(receipt.Transform)
                : Fin.Fail<Transform>(key.OrDefault().InvalidResult()));
    internal Fin<AlignmentReceipt> AlignDetailed(VectorCloud source, VectorCloud target, Op? key = null) =>
        AlignKernel.AlignClouds(kind: this, source: source, target: target, key: key.OrDefault());
}

[SmartEnum<int>]
public sealed partial class AlignmentStopKind {
    public static readonly AlignmentStopKind Converged = new(key: 0);
    public static readonly AlignmentStopKind IterationCapExhausted = new(key: 1);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentReceipt(Transform Transform, AlignmentStopKind Stop, int Iterations, double FinalDelta, int Count, double Rmse, double MedianResidual);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class AlignKernel {
    private const int IcpMaxIterations = 30;
    private const double IcpConvergenceTolerance = 1e-6;
    private const double WelschNu = 0.1;
    private readonly record struct IcpState(Transform Current, double FinalDelta, int Iterations, int Count, double Rmse, double MedianResidual, Option<AlignmentStopKind> Stop);

    // --- [ALIGNMENT] ------------------------------------------------------------------------
    // Umeyama 1991 (point-to-point) | Chen-Medioni 1992 (point-to-plane) | Rusinkiewicz 2019
    // (symmetric) | yaoyx689 2020 robust IRLS with Welsch loss. All four share the iterative
    // correspondence-finding outer loop; the inner solve switches on kind.
    internal static Fin<AlignmentReceipt> AlignClouds(AlignKind kind, VectorCloud source, VectorCloud target, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt)
                => IcpAlign(source: src, target: tgt, kind: kind, key: key),
            _ => Fin.Fail<AlignmentReceipt>(error: key.InvalidInput()),
        };

    private static Fin<Transform> ProcrustesAlign(Seq<Point3d> source, Seq<Point3d> target, Op key) {
        if (source.Count != target.Count || source.Count < 3) return Fin.Fail<Transform>(error: key.InvalidInput());
        Point3d srcCentroid = CentroidOf(points: source);
        Point3d tgtCentroid = CentroidOf(points: target);
        return AlignViaCrossCovariance(source: source, target: target, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: null, key: key);
    }
    // Iterative closest-point with correspondence re-association at each step; inner solve
    // selects between point-to-plane, symmetric, and robust by AlignKind.
    private static Fin<AlignmentReceipt> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, Op key) =>
        from targetNormals in CloudKernel.EstimateNormalsViaCovariance(target: target, key: key)
        from aligned in IcpAlignWithNormals(source: source, target: target, kind: kind, targetNormals: targetNormals, key: key)
        select aligned;
    private static Fin<AlignmentReceipt> IcpAlignWithNormals(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, Vector3d[] targetNormals, Op key) =>
        from final in toSeq(Enumerable.Range(start: 0, count: IcpMaxIterations)).Fold(
            initialState: Fin.Succ(new IcpState(Current: Transform.Identity, FinalDelta: double.PositiveInfinity, Iterations: 0, Count: 0, Rmse: 0.0, MedianResidual: 0.0, Stop: Option<AlignmentStopKind>.None)),
            f: (acc, iter) => acc.Bind(state => state.Stop.IsSome
                ? Fin.Succ(state)
                : from correspondences in FindCorrespondences(source: source.Vertices, target: target, normals: targetNormals, current: state.Current, key: key)
                  from delta in kind.SolveStep(source: source.Vertices, target: correspondences.Target, normals: correspondences.Normals, residuals: correspondences.Residuals, current: state.Current, key: key)
                  let current = delta * state.Current
                  let finalDelta = DeltaMagnitude(delta: delta)
                  select new IcpState(
                      Current: current,
                      FinalDelta: finalDelta,
                      Iterations: iter + 1,
                      Count: correspondences.Count,
                      Rmse: correspondences.Rmse,
                      MedianResidual: correspondences.MedianResidual,
                      Stop: finalDelta < IcpConvergenceTolerance ? Some(AlignmentStopKind.Converged) : Option<AlignmentStopKind>.None)))
        select new AlignmentReceipt(
            Transform: final.Current,
            Stop: final.Stop.IfNone(AlignmentStopKind.IterationCapExhausted),
            Iterations: final.Iterations,
            FinalDelta: final.FinalDelta,
            Count: final.Count,
            Rmse: final.Rmse,
            MedianResidual: final.MedianResidual);
    private static Fin<(Point3d[] Target, Vector3d[] Normals, double[] Residuals, int Count, double Rmse, double MedianResidual)> FindCorrespondences(Seq<Point3d> source, VectorCloud.ClusterCase target, Vector3d[] normals, Transform current, Op key) {
        int n = source.Count;
        Point3d[] transformed = [.. source.AsIterable().Select(point => current * point)];
        int[][] nearestIds = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: transformed, amount: 1)];
        Point3d[] matchedTarget = new Point3d[n]; Vector3d[] matchedNormals = new Vector3d[n]; double[] residuals = new double[n];
        double squared = 0.0;
        for (int i = 0; i < n; i++) {
            int nearest = nearestIds.Length > i && nearestIds[i].Length > 0 ? nearestIds[i][0] : -1;
            if (nearest < 0 || nearest >= target.Vertices.Count || nearest >= normals.Length) return Fin.Fail<(Point3d[] Target, Vector3d[] Normals, double[] Residuals, int Count, double Rmse, double MedianResidual)>(key.InvalidResult());
            matchedTarget[i] = target.Indexed.PointAt(index: nearest);
            matchedNormals[i] = normals[nearest];
            residuals[i] = transformed[i].DistanceTo(other: matchedTarget[i]);
            squared += residuals[i] * residuals[i];
        }
        double[] sorted = [.. residuals.Select(static residual => Math.Abs(value: residual)).Order()];
        return Fin.Succ((
            Target: matchedTarget,
            Normals: matchedNormals,
            Residuals: residuals,
            Count: n,
            Rmse: n > 0 ? Math.Sqrt(d: squared / n) : 0.0,
            MedianResidual: sorted.Length > 0 ? sorted[sorted.Length / 2] : 0.0));
    }
    internal static Fin<Transform> SolvePointToPoint(Seq<Point3d> source, Point3d[] target, Transform current, Op key) {
        Seq<Point3d> transformedSource = toSeq(source.AsIterable().Select(p => current * p));
        return ProcrustesAlign(source: transformedSource, target: toSeq(target), key: key);
    }
    // Point-to-plane linearization (Chen-Medioni 1992): assume small-angle rotation R ≈ I + [ω]×,
    // minimize ||A[ω;t] − b||² where each row is [cross(p, n) | n] · [ω;t] = (q − p) · n.
    internal static Fin<Transform> SolvePointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) =>
        SolveLinearizedRows(source: source, target: target, normals: normals, current: current, key: key, rowNormal: static (_, normal) => (Normal: normal, Weight: 1.0));
    internal static Fin<Transform> SolveSymmetric(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key) =>
        EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => SolveLinearizedRows(
            source: source,
            target: target,
            normals: normals,
            current: current,
            key: key,
            rowNormal: (i, targetNormal) => {
                Vector3d sourceNormal = sourceNormals[i] * targetNormal < 0.0 ? -sourceNormals[i] : sourceNormals[i];
                Vector3d combined = sourceNormal + targetNormal;
                _ = combined.Unitize();
                return (Normal: combined, Weight: 1.0);
            }));
    private static Fin<Vector3d[]> EstimateSourceNormals(Seq<Point3d> source, Transform current, Op key) =>
        CloudKernel.EstimateNormalsFromPoints(points: [.. source.Map(p => current * p).AsIterable()], key: key);
    internal static Fin<Transform> SolveNormalWeightedPointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] targetNormals, Transform current, Op key) =>
        EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => SolveLinearizedRows(
            source: source,
            target: target,
            normals: targetNormals,
            current: current,
            key: key,
            rowNormal: (i, normal) => (Normal: normal, Weight: Math.Sqrt(d: Math.Max(val1: Math.Abs(value: sourceNormals[i] * normal), val2: RhinoMath.SqrtEpsilon)))));
    private static Fin<Transform> SolveLinearizedRows(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Transform current, Op key, Func<int, Vector3d, (Vector3d Normal, double Weight)> rowNormal) {
        int n = source.Count;
        if (n < 6 || target.Length != n || normals.Length != n) return Fin.Fail<Transform>(key.InvalidInput());
        double[] aFlat = new double[n * 6]; double[] b = new double[n];
        for (int i = 0; i < n; i++) {
            (Vector3d rawNormal, double weight) = rowNormal(i, normals[i]);
            if (!rawNormal.IsValid || rawNormal.IsTiny() || !RhinoMath.IsValidDouble(x: weight) || weight <= 0.0)
                return Fin.Fail<Transform>(key.InvalidResult());
            Point3d p = current * source[index: i]; Point3d q = target[i]; Vector3d nrm = weight * rawNormal;
            Vector3d cross = Vector3d.CrossProduct(a: (Vector3d)p, b: nrm);
            aFlat[(i * 6) + 0] = cross.X; aFlat[(i * 6) + 1] = cross.Y; aFlat[(i * 6) + 2] = cross.Z;
            aFlat[(i * 6) + 3] = nrm.X; aFlat[(i * 6) + 4] = nrm.Y; aFlat[(i * 6) + 5] = nrm.Z;
            b[i] = (q - p) * nrm;
        }
        return SolveLeastSquares6(aFlat: aFlat, b: b, n: n, key: key).Map(static x => ComposeRigidTransform(omega: new Vector3d(x: x[0], y: x[1], z: x[2]), translation: new Vector3d(x: x[3], y: x[4], z: x[5])));
    }

    internal static Fin<Transform> SolveRobustProcrustes(Seq<Point3d> source, Point3d[] target, double[] residuals, Transform current, Op key) {
        int n = source.Count;
        if (n < 3 || target.Length != n || residuals.Length != n) return Fin.Fail<Transform>(key.InvalidInput());
        double[] weights = new double[n];
        double[] sortedResiduals = [.. residuals.Select(static residual => Math.Abs(value: residual)).Order()];
        double median = sortedResiduals.Length == 0 ? 1.0 : sortedResiduals[sortedResiduals.Length / 2];
        double nu = Math.Max(val1: 1.4826 * median * WelschNu, val2: RhinoMath.SqrtEpsilon);
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
        n < 6 || aFlat.Length != n * 6 || b.Length != n || !aFlat.All(RhinoMath.IsValidDouble) || !b.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<double[]>(key.InvalidInput())
            : key.Catch(() => {
                LinearMatrix design = DenseMatrixD.Build.Dense(rows: n, columns: 6, init: (i, j) => aFlat[(i * 6) + j]);
                DenseVectorD rhs = DenseVectorD.OfArray(b);
                QR<double> qr = design.QR(QRMethod.Full);
                double[] solved = [.. qr.Solve(rhs)];
                return solved.Length == 6 && solved.All(RhinoMath.IsValidDouble)
                    ? Fin.Succ(solved)
                    : Fin.Fail<double[]>(key.InvalidResult());
            });
    // Construct R ≈ I + [ω]× via Rodrigues approximation for small ω; combine with translation.
    private static Transform ComposeRigidTransform(Vector3d omega, Vector3d translation) {
        double theta = omega.Length;
        Transform rot = theta < RhinoMath.SqrtEpsilon
            ? Transform.Identity
            : Transform.Rotation(angleRadians: theta, rotationAxis: omega / theta, rotationCenter: Point3d.Origin);
        return WithTranslation(rotation: rot, translation: translation);
    }
    private static double DeltaMagnitude(Transform delta) {
        double diff = 0.0;
        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) diff += Math.Abs(value: delta[i, j] - (i == j ? 1.0 : 0.0));
        return diff;
    }
}
