using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentRobustReceipt(double Scale, double MinWeight, double MaxWeight);
internal readonly record struct AlignmentStep(Transform Delta, Option<SolveReceipt> Solve = default, Option<AlignmentRobustReceipt> Robust = default);
internal readonly record struct AlignmentMatch(CloudCorrespondenceSet Correspondences, Point3d[] Targets, Vector3d[] Normals, double[] Distances, double[] RowMass);

[SmartEnum<int>]
public sealed partial class AlignKind {
    public static readonly AlignKind Point = new(key: 0,
        needsTargetNormals: false,
        approximation: AlignmentApproximationStatus.RigidPointToPoint,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolvePointToPoint(source: source, target: match.Targets, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Plane = new(key: 1,
        needsTargetNormals: true,
        approximation: AlignmentApproximationStatus.LinearizedPointToPlane,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolvePointToPlane(source: source, target: match.Targets, normals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Symmetric = new(key: 2,
        needsTargetNormals: true,
        approximation: AlignmentApproximationStatus.SymmetricNormalSumLinearized,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolveSymmetric(source: source, target: match.Targets, normals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Robust = new(key: 3,
        needsTargetNormals: false,
        approximation: AlignmentApproximationStatus.RobustWeightedPointToPoint,
        solveStep: static (source, match, current, policy, key) => AlignKernel.SolveRobustProcrustes(source: source, target: match.Targets, residuals: match.Distances, rowMass: match.RowMass, current: current, robustScale: policy.RobustScale.Value, key: key));
    public static readonly AlignKind NormalWeightedPointToPlane = new(key: 4,
        needsTargetNormals: true,
        approximation: AlignmentApproximationStatus.NormalWeightedPointToPlane,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolveNormalWeightedPointToPlane(source: source, target: match.Targets, targetNormals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public bool NeedsTargetNormals { get; }
    public AlignmentApproximationStatus Approximation { get; }
    [UseDelegateFromConstructor] internal partial Fin<AlignmentStep> SolveStep(Seq<Point3d> source, AlignmentMatch match, Transform current, AlignmentPolicy policy, Op key);
    internal Fin<AlignmentReceipt> AlignDetailed(VectorCloud source, VectorCloud target, Op? key = null) =>
        AlignDetailed(source: source, target: target, policy: AlignmentPolicy.Default, key: key);
    internal Fin<AlignmentReceipt> AlignDetailed(VectorCloud source, VectorCloud target, AlignmentPolicy policy, Op? key = null) =>
        AlignKernel.AlignClouds(kind: this, source: source, target: target, policy: policy, key: key.OrDefault());
}

[SmartEnum<int>]
public sealed partial class AlignmentStopKind {
    public static readonly AlignmentStopKind Converged = new(key: 0), MaxIterationsExhausted = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class AlignmentApproximationStatus {
    public static readonly AlignmentApproximationStatus RigidPointToPoint = new(key: 0), LinearizedPointToPlane = new(key: 1), SymmetricNormalSumLinearized = new(key: 2), RobustWeightedPointToPoint = new(key: 3), NormalWeightedPointToPlane = new(key: 4);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentPolicy(Dimension MaxIterations, PositiveMagnitude ConvergenceTolerance, PositiveMagnitude RobustScale) {
    internal static AlignmentPolicy Default => new(MaxIterations: Dimension.Create(value: 30), ConvergenceTolerance: PositiveMagnitude.Create(value: 1.0e-6), RobustScale: PositiveMagnitude.Create(value: 0.1));
    internal Fin<AlignmentPolicy> Admit(Op key) {
        AlignmentPolicy self = this;
        return from iterations in FieldNabla.Dimension(value: self.MaxIterations, key: key)
               from tolerance in FieldNabla.Positive(value: self.ConvergenceTolerance, key: key)
               from scale in FieldNabla.Positive(value: self.RobustScale, key: key)
               select self;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentReceipt(Transform Transform, AlignKind Kind, AlignmentApproximationStatus Approximation, AlignmentStopKind Stop, int Iterations, double FinalDelta, Option<AlignmentRobustReceipt> Robust, CloudCorrespondenceSet Correspondences, Option<SolveReceipt> Solve);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class AlignKernel {
    private readonly record struct IcpState(Transform Current, double FinalDelta, int Iterations, AlignmentStep Step, Option<AlignmentStopKind> Stop);

    // --- [ALIGNMENT] ------------------------------------------------------------------------
    // Umeyama 1991 (point-to-point) | Chen-Medioni 1992 (point-to-plane) | Rusinkiewicz 2019
    // (symmetric) | MAD-scaled Welsch weighting. All modes share the iterative
    // correspondence-finding outer loop; the inner solve switches on kind.
    internal static Fin<AlignmentReceipt> AlignClouds(AlignKind kind, VectorCloud source, VectorCloud target, AlignmentPolicy policy, Op key) =>
        from activePolicy in policy.Admit(key: key)
        from receipt in (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt)
                => IcpAlign(source: src, target: tgt, kind: kind, policy: activePolicy, key: key),
            _ => Fin.Fail<AlignmentReceipt>(error: key.InvalidInput()),
        }
        select receipt;

    private static Fin<AlignmentReceipt> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, AlignmentPolicy policy, Op key) =>
        from targetNormals in kind.NeedsTargetNormals
            ? CloudKernel.EstimateNormalsViaCovariance(target: target, key: key)
            : Fin.Succ(System.Array.Empty<Vector3d>())
        from sourceMass in CloudKernel.MassOf(cluster: source, key: key)
        from final in toSeq(Enumerable.Range(start: 0, count: policy.MaxIterations.Value)).Fold(
            initialState: Fin.Succ(new IcpState(Current: Transform.Identity, FinalDelta: double.PositiveInfinity, Iterations: 0, Step: new AlignmentStep(Delta: Transform.Identity), Stop: Option<AlignmentStopKind>.None)),
            f: (acc, iter) => acc.Bind(state => state.Stop.IsSome
                ? Fin.Succ(state)
                : from match in FindCorrespondences(source: source.Vertices, sourceMass: sourceMass, target: target, normals: targetNormals, current: state.Current, key: key)
                  from step in kind.SolveStep(source: source.Vertices, match: match, current: state.Current, policy: policy, key: key)
                  let current = step.Delta * state.Current
                  let finalDelta = DeltaMagnitude(delta: step.Delta)
                  select new IcpState(
                      Current: current,
                      FinalDelta: finalDelta,
                      Iterations: iter + 1,
                      Step: step,
                      Stop: finalDelta < policy.ConvergenceTolerance.Value ? Some(AlignmentStopKind.Converged) : Option<AlignmentStopKind>.None)))
        from finalMatch in FindCorrespondences(source: source.Vertices, sourceMass: sourceMass, target: target, normals: targetNormals, current: final.Current, key: key)
        select new AlignmentReceipt(Transform: final.Current, Kind: kind, Approximation: kind.Approximation, Stop: final.Stop.IfNone(AlignmentStopKind.MaxIterationsExhausted), Iterations: final.Iterations, FinalDelta: final.FinalDelta, Robust: final.Step.Robust, Correspondences: finalMatch.Correspondences, Solve: final.Step.Solve);
    private static Fin<AlignmentMatch> FindCorrespondences(Seq<Point3d> source, Arr<double> sourceMass, VectorCloud.ClusterCase target, Vector3d[] normals, Transform current, Op key) {
        int n = source.Count;
        Point3d[] transformed = [.. source.AsIterable().Select(point => current * point)];
        int[][] nearestIds = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: transformed, amount: 1)];
        return CloudKernel.MassOf(cluster: target, key: key).Bind(targetMass => {
            Point3d[] targets = new Point3d[n]; Vector3d[] rowNormals = normals.Length == 0 ? [] : new Vector3d[n]; double[] distances = new double[n]; double[] rowMass = new double[n];
            List<CloudCorrespondence> items = new(capacity: n);
            for (int i = 0; i < n; i++) {
                int nearest = nearestIds.Length > i && nearestIds[i].Length > 0 ? nearestIds[i][0] : -1;
                if (nearest < 0 || nearest >= target.Vertices.Count || sourceMass.Count <= i || (normals.Length > 0 && nearest >= normals.Length)) return Fin.Fail<AlignmentMatch>(key.InvalidResult());
                Point3d targetPoint = target.Indexed.PointAt(index: nearest);
                Vector3d residual = targetPoint - transformed[i];
                double squared = residual.SquareLength;
                targets[i] = targetPoint; distances[i] = Math.Sqrt(d: squared); rowMass[i] = sourceMass[index: i];
                if (normals.Length > 0) rowNormals[i] = normals[nearest];
                items.Add(item: new CloudCorrespondence(
                    SourceIndex: i,
                    TargetIndex: nearest,
                    SourcePoint: transformed[i],
                    TargetPoint: targetPoint,
                    Residual: residual,
                    Distance: distances[i],
                    SquaredDistance: squared,
                    SourceMass: Some(sourceMass[index: i]),
                    TargetMass: Some(targetMass[index: nearest]),
                    CouplingMass: Some(sourceMass[index: i]),
                    Confidence: Option<double>.None));
            }
            return Fin.Succ(new AlignmentMatch(Correspondences: CloudCorrespondenceSet.Of(items: toSeq(items), sourceCount: source.Count, targetCount: target.Vertices.Count), Targets: targets, Normals: rowNormals, Distances: distances, RowMass: rowMass));
        });
    }
    internal static Fin<AlignmentStep> SolvePointToPoint(Seq<Point3d> source, Point3d[] target, double[] rowMass, Transform current, Op key) =>
        SolveProcrustes(source: source, target: target, weights: rowMass, current: current, key: key)
            .Map(static delta => new AlignmentStep(Delta: delta));
    // Point-to-plane linearization (Chen-Medioni 1992): assume small-angle rotation R ≈ I + [ω]×,
    // minimize ||A[ω;t] − b||² where each row is [cross(p, n) | n] · [ω;t] = (q − p) · n.
    internal static Fin<AlignmentStep> SolvePointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] rowMass, Transform current, Op key) =>
        SolveLinearizedRows(source: source, target: target, normals: normals, rowMass: rowMass, current: current, key: key, rowNormal: static (_, normal) => (Normal: normal, Weight: 1.0));
    internal static Fin<AlignmentStep> SolveSymmetric(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] rowMass, Transform current, Op key) =>
        EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => SolveLinearizedRows(
            source: source,
            target: target,
            normals: normals,
            rowMass: rowMass,
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
    internal static Fin<AlignmentStep> SolveNormalWeightedPointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] targetNormals, double[] rowMass, Transform current, Op key) =>
        EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => SolveLinearizedRows(
            source: source,
            target: target,
            normals: targetNormals,
            rowMass: rowMass,
            current: current,
            key: key,
            rowNormal: (i, normal) => (Normal: normal, Weight: Math.Sqrt(d: Math.Max(val1: Math.Abs(value: sourceNormals[i] * normal), val2: RhinoMath.SqrtEpsilon)))));
    private static Fin<int> AdmitAlignmentRows(Seq<Point3d> source, Point3d[] target, double[] weights, int minimum, Op key) =>
        from count in FieldNabla.CountAtLeast(count: source.Count, minimum: minimum, key: key).Map(_ => source.Count)
        from same in FieldNabla.SameCount(expected: count, key: key, counts: [target.Length, weights.Length])
        from sourceFinite in FieldNabla.AllFinite(points: source, key: key)
        from targetFinite in guard(target.All(static point => FieldNabla.Finite(point: point)), key.InvalidInput())
        from mass in FieldNabla.PositiveFiniteWeights(weights: weights, count: count, key: key)
        select count;
    private static Fin<AlignmentStep> SolveLinearizedRows(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] rowMass, Transform current, Op key, Func<int, Vector3d, (Vector3d Normal, double Weight)> rowNormal) {
        int n = source.Count;
        Fin<int> admitted = from count in AdmitAlignmentRows(source: source, target: target, weights: rowMass, minimum: 6, key: key)
                            from normalCount in FieldNabla.SameCount(expected: count, key: key, counts: [normals.Length])
                            from finiteNormals in guard(normals.All(static normal => normal.IsValid), key.InvalidInput())
                            select count;
        return admitted.Bind(_ => {
            double[] aFlat = new double[n * 6]; double[] b = new double[n];
            for (int i = 0; i < n; i++) {
                (Vector3d rawNormal, double weight) = rowNormal(i, normals[i]);
                double massWeight = Math.Sqrt(d: rowMass[i]);
                if (!rawNormal.IsValid || rawNormal.SquareLength <= RhinoMath.SqrtEpsilon * RhinoMath.SqrtEpsilon || !RhinoMath.IsValidDouble(x: weight) || weight <= 0.0)
                    return Fin.Fail<AlignmentStep>(key.InvalidResult());
                Point3d p = current * source[index: i]; Point3d q = target[i]; Vector3d nrm = weight * massWeight * rawNormal;
                Vector3d cross = Vector3d.CrossProduct(a: (Vector3d)p, b: nrm);
                aFlat[(i * 6) + 0] = cross.X; aFlat[(i * 6) + 1] = cross.Y; aFlat[(i * 6) + 2] = cross.Z;
                aFlat[(i * 6) + 3] = nrm.X; aFlat[(i * 6) + 4] = nrm.Y; aFlat[(i * 6) + 5] = nrm.Z;
                b[i] = (q - p) * nrm;
            }
            return Matrix.Of(rows: Dimension.Create(value: n), cols: Dimension.Create(value: 6), entries: new Arr<double>(aFlat), key: key)
            .Bind(design => design.LeastSquaresDetailed(rhs: new Arr<double>(b), key: key))
            .Bind(receipt => receipt.Solution.Count == 6 && receipt.Solution.ForAll(RhinoMath.IsValidDouble)
                ? Fin.Succ(new AlignmentStep(
                    Delta: ComposeRigidTransform(omega: new Vector3d(x: receipt.Solution[0], y: receipt.Solution[1], z: receipt.Solution[2]), translation: new Vector3d(x: receipt.Solution[3], y: receipt.Solution[4], z: receipt.Solution[5])),
                    Solve: Some(receipt)))
                : Fin.Fail<AlignmentStep>(key.InvalidResult()));
        });
    }

    internal static Fin<AlignmentStep> SolveRobustProcrustes(Seq<Point3d> source, Point3d[] target, double[] residuals, double[] rowMass, Transform current, double robustScale = 0.1, Op? key = null) {
        Op op = key.OrDefault();
        int n = source.Count;
        Fin<int> admitted = from count in AdmitAlignmentRows(source: source, target: target, weights: rowMass, minimum: 3, key: op)
                            from residualCount in FieldNabla.SameCount(expected: count, key: op, counts: [residuals.Length])
                            from finiteResiduals in guard(residuals.All(static residual => RhinoMath.IsValidDouble(x: residual)) && RhinoMath.IsValidDouble(x: robustScale) && robustScale > 0.0, op.InvalidInput())
                            select count;
        return admitted.Bind(_ => {
            double[] weights = new double[n];
            double[] sortedResiduals = [.. residuals.Select(static residual => Math.Abs(value: residual)).Order()];
            double median = sortedResiduals.Length == 0 ? 1.0 : sortedResiduals[sortedResiduals.Length / 2];
            double nu = Math.Max(val1: 1.4826 * median * robustScale, val2: RhinoMath.SqrtEpsilon);
            for (int i = 0; i < n; i++) weights[i] = rowMass[i] * Math.Exp(d: -(residuals[i] * residuals[i]) / (2.0 * nu * nu));
            return from aligned in SolveProcrustes(source: source, target: target, weights: weights, current: current, key: op)
                   select new AlignmentStep(Delta: aligned, Robust: Some(new AlignmentRobustReceipt(Scale: nu, MinWeight: weights.Min(), MaxWeight: weights.Max())));
        });
    }
    private static Fin<Transform> SolveProcrustes(Seq<Point3d> source, Point3d[] target, double[] weights, Transform current, Op key) {
        Seq<Point3d> transformedSource = toSeq(source.AsIterable().Select(p => current * p));
        Seq<Point3d> targetSeq = toSeq(target);
        return from rows in AdmitAlignmentRows(source: transformedSource, target: target, weights: weights, minimum: 3, key: key)
               from srcCentroid in WeightedCentroidOf(points: transformedSource, weights: weights, key: key)
               from tgtCentroid in WeightedCentroidOf(points: targetSeq, weights: weights, key: key)
               from aligned in AlignViaCrossCovariance(source: transformedSource, target: targetSeq, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: weights, key: key)
               select aligned;
    }
    private static Fin<Point3d> WeightedCentroidOf(Seq<Point3d> points, double[] weights, Op key) {
        Vector3d sum = Vector3d.Zero; double totalW = 0.0;
        for (int i = 0; i < points.Count; i++) { sum += weights[i] * (Vector3d)points[index: i]; totalW += weights[i]; }
        return totalW > RhinoMath.ZeroTolerance && RhinoMath.IsValidDouble(x: totalW)
            ? key.AcceptValue(value: Point3d.Origin + (sum / totalW))
            : Fin.Fail<Point3d>(key.InvalidResult());
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
               from vu in svd.V.Multiply(other: svd.U.Transpose(), key: key)
               from det in vu.Determinant(key: key)
               let diag = new[] { 1.0, 1.0, det >= 0.0 ? 1.0 : -1.0 }
               let d = new Matrix(Rows: Dimension.Create(value: 3), Cols: Dimension.Create(value: 3),
                   Entries: [.. Enumerable.Range(start: 0, count: 9).Select(idx => (idx / 3) == (idx % 3) ? diag[idx / 3] : 0.0)])
               from vd in svd.V.Multiply(other: d, key: key)
               from rot in vd.Multiply(other: svd.U.Transpose(), key: key)
               let rotation = RotationTransformOf(rotation: rot)
               select WithTranslation(rotation: rotation, translation: tgtCentroid - (rotation * srcCentroid));
    }
    private static Transform RotationTransformOf(Matrix rotation) {
        Transform xform = Transform.Identity;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                xform[i, j] = rotation.At(i: i, j: j);
        return xform;
    }
    private static Transform WithTranslation(Transform rotation, Vector3d translation) {
        Transform aligned = rotation;
        aligned[0, 3] = translation.X; aligned[1, 3] = translation.Y; aligned[2, 3] = translation.Z;
        return aligned;
    }
    // Compose the linearized increment as an exact axis-angle rotation plus translation.
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
