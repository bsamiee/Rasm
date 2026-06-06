using Foundation.CSharp.Analyzers.Contracts;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AlignKind {
    public static readonly AlignKind Point = new(key: 0,
        needsTargetNormals: false,
        needsCovariances: false,
        approximation: AlignmentApproximationStatus.RigidPointToPoint,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolvePointToPoint(source: source, target: match.Targets, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Plane = new(key: 1,
        needsTargetNormals: true,
        needsCovariances: false,
        approximation: AlignmentApproximationStatus.LinearizedPointToPlane,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolvePointToPlane(source: source, target: match.Targets, normals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Symmetric = new(key: 2,
        needsTargetNormals: true,
        needsCovariances: false,
        approximation: AlignmentApproximationStatus.SymmetricNormalSumLinearized,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolveSymmetric(source: source, target: match.Targets, normals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Robust = new(key: 3,
        needsTargetNormals: false,
        needsCovariances: false,
        approximation: AlignmentApproximationStatus.RobustWeightedPointToPoint,
        solveStep: static (source, match, current, policy, key) => AlignKernel.SolveRobustProcrustes(source: source, target: match.Targets, residuals: match.Distances, rowMass: match.RowMass, current: current, robustScale: policy.RobustScale.Value, key: key));
    public static readonly AlignKind NormalWeightedPointToPlane = new(key: 4,
        needsTargetNormals: true,
        needsCovariances: false,
        approximation: AlignmentApproximationStatus.NormalWeightedPointToPlane,
        solveStep: static (source, match, current, _, key) => AlignKernel.SolveNormalWeightedPointToPlane(source: source, target: match.Targets, targetNormals: match.Normals, rowMass: match.RowMass, current: current, key: key));
    public static readonly AlignKind Generalized = new(key: 5,
        needsTargetNormals: false,
        needsCovariances: true,
        approximation: AlignmentApproximationStatus.GeneralizedIterativeClosestPoint,
        solveStep: static (source, match, current, policy, key) => AlignKernel.SolveGeneralizedIcp(source: source, match: match, current: current, policy: policy, key: key));
    public bool NeedsTargetNormals { get; }
    public bool NeedsCovariances { get; }
    public AlignmentApproximationStatus Approximation { get; }
    [UseDelegateFromConstructor] internal partial Fin<AlignmentStep> SolveStep(Seq<Point3d> source, AlignmentMatch match, Transform current, AlignmentPolicy policy, Op key);
    internal Fin<AlignmentReceipt> AlignDetailed(VectorCloud source, VectorCloud target, Op? key = null) =>
        AlignDetailed(source: source, target: target, policy: AlignmentPolicy.Default, key: key);
    internal Fin<AlignmentReceipt> AlignDetailed(VectorCloud source, VectorCloud target, AlignmentPolicy policy, Op? key = null) =>
        AlignKernel.AlignClouds(kind: this, source: source, target: target, policy: policy, key: key.OrDefault());
}

[SmartEnum<int>]
public sealed partial class AlignmentApproximationStatus {
    public static readonly AlignmentApproximationStatus RigidPointToPoint = new(key: 0), LinearizedPointToPlane = new(key: 1), SymmetricNormalSumLinearized = new(key: 2), RobustWeightedPointToPoint = new(key: 3), NormalWeightedPointToPlane = new(key: 4), GeneralizedIterativeClosestPoint = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class AlignmentOptimizerStopKind {
    public static readonly AlignmentOptimizerStopKind LineSearchAccepted = new(key: 0), StepBelowTolerance = new(key: 1), LineSearchExhausted = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class AlignmentStopKind {
    public static readonly AlignmentStopKind Converged = new(key: 0), MaxIterationsExhausted = new(key: 1), OptimizerStopped = new(key: 2);
}

// --- [MODELS] -----------------------------------------------------------------------------
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
public readonly record struct AlignmentRobustReceipt(double Scale, double MinWeight, double MaxWeight);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentOptimizerReceipt(AlignmentOptimizerStopKind Stop, int LineSearchSteps, double InitialCost, double FinalCost, double StepNorm, double StepScale, double MeanMahalanobis, double MaxMahalanobis, Option<SolveReceipt> Solve, CloudNeighborhoodPcaReceipt SourcePca, CloudNeighborhoodPcaReceipt TargetPca) {
    internal bool IsValid =>
        Stop is not null
        && LineSearchSteps >= 0
        && new[] { InitialCost, FinalCost, StepNorm, StepScale, MeanMahalanobis, MaxMahalanobis }.All(RhinoMath.IsValidDouble)
        && InitialCost >= 0.0
        && FinalCost >= 0.0
        && StepNorm >= 0.0
        && StepScale >= 0.0
        && MeanMahalanobis >= 0.0
        && MaxMahalanobis >= 0.0
        && SourcePca.IsValid
        && TargetPca.IsValid
        && Solve.Map(static solve => solve.IsUsable).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentReceipt(Transform Transform, AlignKind Kind, AlignmentApproximationStatus Approximation, AlignmentStopKind Stop, int Iterations, double FinalDelta, Option<AlignmentRobustReceipt> Robust, CloudCorrespondenceSet Correspondences, Option<SolveReceipt> Solve, Option<AlignmentOptimizerReceipt> Optimizer) {
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(AlignmentReceipt) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Transform) && Stop.Equals(AlignmentStopKind.Converged) => key.AcceptValue(value: Transform).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Transform) => Fin.Fail<TOut>(key.InvalidResult()),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(AlignmentReceipt), outputType: typeof(TOut))),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct AlignmentMatch(CloudCorrespondenceSet Correspondences, Point3d[] Targets, Vector3d[] Normals, double[] Distances, double[] RowMass, int[] TargetIndices, Option<CloudNeighborhoodPcaResult> SourcePca = default, Option<CloudNeighborhoodPcaResult> TargetPca = default);

internal readonly record struct AlignmentStep(Transform Delta, Option<SolveReceipt> Solve = default, Option<AlignmentRobustReceipt> Robust = default, Option<AlignmentOptimizerReceipt> Optimizer = default, Option<AlignmentStopKind> Stop = default);

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
        from neighborhoodPolicy in CloudNeighborhoodPolicy.Default(key: key)
        from targetNormals in kind.NeedsTargetNormals
            ? CloudKernel.EstimateNormalsViaCovariance(target: target, key: key)
            : Fin.Succ(System.Array.Empty<Vector3d>())
        from sourcePca in kind.NeedsCovariances
            ? CloudKernel.NeighborhoodPcaOf(cluster: source, policy: neighborhoodPolicy, key: key).Map(Some)
            : Fin.Succ(Option<CloudNeighborhoodPcaResult>.None)
        from targetPca in kind.NeedsCovariances
            ? CloudKernel.NeighborhoodPcaOf(cluster: target, policy: neighborhoodPolicy, key: key).Map(Some)
            : Fin.Succ(Option<CloudNeighborhoodPcaResult>.None)
        from sourceMass in CloudKernel.MassOf(cluster: source, key: key)
        from targetMass in CloudKernel.MassOf(cluster: target, key: key)
        from final in toSeq(Enumerable.Range(start: 0, count: policy.MaxIterations.Value)).Fold(
            initialState: Fin.Succ(new IcpState(Current: Transform.Identity, FinalDelta: double.PositiveInfinity, Iterations: 0, Step: new AlignmentStep(Delta: Transform.Identity), Stop: Option<AlignmentStopKind>.None)),
            f: (acc, iter) => acc.Bind(state => state.Stop.IsSome
                ? Fin.Succ(state)
                : from match in FindCorrespondences(source: source.Vertices, sourceMass: sourceMass, target: target, targetMass: targetMass, normals: targetNormals, current: state.Current, sourcePca: sourcePca, targetPca: targetPca, key: key)
                  from step in kind.SolveStep(source: source.Vertices, match: match, current: state.Current, policy: policy, key: key)
                  let current = step.Delta * state.Current
                  let finalDelta = DeltaMagnitude(delta: step.Delta)
                  select new IcpState(
                      Current: current,
                      FinalDelta: finalDelta,
                      Iterations: iter + 1,
                      Step: step,
                      Stop: step.Stop.IsSome ? step.Stop : finalDelta < policy.ConvergenceTolerance.Value ? Some(AlignmentStopKind.Converged) : Option<AlignmentStopKind>.None)))
        from finalMatch in FindCorrespondences(source: source.Vertices, sourceMass: sourceMass, target: target, targetMass: targetMass, normals: targetNormals, current: final.Current, sourcePca: sourcePca, targetPca: targetPca, key: key)
        select new AlignmentReceipt(Transform: final.Current, Kind: kind, Approximation: kind.Approximation, Stop: final.Stop.IfNone(AlignmentStopKind.MaxIterationsExhausted), Iterations: final.Iterations, FinalDelta: final.FinalDelta, Robust: final.Step.Robust, Correspondences: finalMatch.Correspondences, Solve: final.Step.Solve, Optimizer: final.Step.Optimizer);
    private static Fin<AlignmentMatch> FindCorrespondences(Seq<Point3d> source, Arr<double> sourceMass, VectorCloud.ClusterCase target, Arr<double> targetMass, Vector3d[] normals, Transform current, Option<CloudNeighborhoodPcaResult> sourcePca, Option<CloudNeighborhoodPcaResult> targetPca, Op key) {
        int n = source.Count;
        Point3d[] transformed = [.. source.AsIterable().Select(point => current * point)];
        int[][] nearestIds = [.. RTree.PointCloudKNeighbors(pointcloud: target.Indexed, needlePts: transformed, amount: 1)];
        return FieldNabla.SameCount(expected: source.Count, key: key, counts: [sourceMass.Count])
        .Bind(_ => FieldNabla.SameCount(expected: target.Vertices.Count, key: key, counts: [targetMass.Count]))
        .Bind(_ => {
            Point3d[] targets = new Point3d[n]; Vector3d[] rowNormals = normals.Length == 0 ? [] : new Vector3d[n]; double[] distances = new double[n]; double[] rowMass = new double[n]; int[] targetIndices = new int[n];
            List<CloudCorrespondence> items = new(capacity: n);
            for (int i = 0; i < n; i++) {
                int nearest = nearestIds.Length > i && nearestIds[i].Length > 0 ? nearestIds[i][0] : -1;
                if (nearest < 0 || nearest >= target.Vertices.Count || (normals.Length > 0 && nearest >= normals.Length)) return Fin.Fail<AlignmentMatch>(key.InvalidResult());
                Point3d targetPoint = target.Indexed.PointAt(index: nearest);
                Vector3d residual = targetPoint - transformed[i];
                double squared = residual.SquareLength;
                targets[i] = targetPoint; distances[i] = Math.Sqrt(d: squared); rowMass[i] = sourceMass[index: i];
                targetIndices[i] = nearest;
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
            return Fin.Succ(new AlignmentMatch(Correspondences: CloudCorrespondenceSet.Of(items: toSeq(items), sourceCount: source.Count, targetCount: target.Vertices.Count), Targets: targets, Normals: rowNormals, Distances: distances, RowMass: rowMass, TargetIndices: targetIndices, SourcePca: sourcePca, TargetPca: targetPca));
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
            double[] logs = [.. residuals.Select(residual => -(residual * residual) / (2.0 * nu * nu))];
            double offset = logs.Max();
            for (int i = 0; i < n; i++) weights[i] = rowMass[i] * Math.Exp(d: logs[i] - offset);
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
    internal static Fin<AlignmentStep> SolveNormalWeightedPointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] targetNormals, double[] rowMass, Transform current, Op key) =>
        EstimateSourceNormals(source: source, current: current, key: key).Bind(sourceNormals => SolveLinearizedRows(
            source: source,
            target: target,
            normals: targetNormals,
            rowMass: rowMass,
            current: current,
            key: key,
            rowNormal: (i, normal) => (Normal: normal, Weight: Math.Sqrt(d: Math.Max(val1: Math.Abs(value: sourceNormals[i] * normal), val2: RhinoMath.SqrtEpsilon)))));
    private static Fin<Vector3d[]> EstimateSourceNormals(Seq<Point3d> source, Transform current, Op key) =>
        CloudKernel.EstimateNormalsFromPoints(points: [.. source.Map(p => current * p).AsIterable()], key: key);
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct GicpObjective(double Cost, double MeanMahalanobis, double MaxMahalanobis);
    internal static Fin<AlignmentStep> SolveGeneralizedIcp(Seq<Point3d> source, AlignmentMatch match, Transform current, AlignmentPolicy policy, Op key) =>
        from sourcePca in match.SourcePca.ToFin(key.InvalidInput())
        from targetPca in match.TargetPca.ToFin(key.InvalidInput())
        from rows in AdmitAlignmentRows(source: source, target: match.Targets, weights: match.RowMass, minimum: 3, key: key)
        from sourcePcaCount in FieldNabla.SameCount(expected: rows, key: key, counts: [sourcePca.Samples.Count])
        from targetIndexCount in FieldNabla.SameCount(expected: rows, key: key, counts: [match.TargetIndices.Length])
        from targetIndices in guard(match.TargetIndices.All(index => index >= 0 && index < targetPca.Samples.Count), key.InvalidInput())
        from initial in GicpObjectiveOf(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: current, key: key)
        from solve in GicpNormalEquation(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: current, damping: policy.ConvergenceTolerance.Value, key: key)
        from step in GicpLineSearch(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: current, initial: initial, solution: solve, tolerance: policy.ConvergenceTolerance.Value, key: key)
        select step;
    private static Fin<SolveReceipt> GicpNormalEquation(Seq<Point3d> source, AlignmentMatch match, CloudNeighborhoodPcaResult sourcePca, CloudNeighborhoodPcaResult targetPca, Transform current, double damping, Op key) =>
        key.Catch(() => {
            double[] h = new double[36];
            double[] g = new double[6];
            for (int i = 0; i < source.Count; i++) {
                Point3d x = current * source[index: i];
                Vector3d residual = match.Targets[i] - x;
                LinearMatrix precision = GicpPrecisionOf(current: current, source: sourcePca.Samples[index: i].Covariance, target: targetPca.Samples[index: match.TargetIndices[i]].Covariance);
                double[] jacobian = GicpJacobianOf(point: x);
                for (int a = 0; a < 6; a++) {
                    double weightedResidual = 0.0;
                    for (int r = 0; r < 3; r++)
                        weightedResidual += jacobian[(r * 6) + a] * ((precision[r, 0] * residual.X) + (precision[r, 1] * residual.Y) + (precision[r, 2] * residual.Z));
                    g[a] += match.RowMass[i] * weightedResidual;
                    for (int b = 0; b < 6; b++) {
                        double weightedJacobian = 0.0;
                        for (int r = 0; r < 3; r++)
                            for (int c = 0; c < 3; c++)
                                weightedJacobian += jacobian[(r * 6) + a] * precision[r, c] * jacobian[(c * 6) + b];
                        h[(a * 6) + b] += match.RowMass[i] * weightedJacobian;
                    }
                }
            }
            double lambda = Math.Max(val1: RhinoMath.SqrtEpsilon, val2: damping);
            for (int i = 0; i < 6; i++) h[(i * 6) + i] += lambda;
            double[] rhs = [.. g.Select(static value => -value)];
            return Matrix.Of(rows: Dimension.Create(value: 6), cols: Dimension.Create(value: 6), entries: new Arr<double>(h), key: key)
                .Bind(normal => normal.SolveDetailed(rhs: new Arr<double>(rhs), key: key));
        });
    private static Fin<AlignmentStep> GicpLineSearch(Seq<Point3d> source, AlignmentMatch match, CloudNeighborhoodPcaResult sourcePca, CloudNeighborhoodPcaResult targetPca, Transform current, GicpObjective initial, SolveReceipt solution, double tolerance, Op key) {
        double stepNorm = Math.Sqrt(d: solution.Solution.AsIterable().Sum(static value => value * value));
        if (stepNorm <= tolerance) {
            AlignmentOptimizerReceipt settled = new(
                Stop: AlignmentOptimizerStopKind.StepBelowTolerance,
                LineSearchSteps: 0,
                InitialCost: initial.Cost,
                FinalCost: initial.Cost,
                StepNorm: stepNorm,
                StepScale: 0.0,
                MeanMahalanobis: initial.MeanMahalanobis,
                MaxMahalanobis: initial.MaxMahalanobis,
                Solve: Some(solution),
                SourcePca: sourcePca.Receipt,
                TargetPca: targetPca.Receipt);
            return settled.IsValid ? Fin.Succ(new AlignmentStep(Delta: Transform.Identity, Solve: Some(solution), Optimizer: Some(settled), Stop: Some(AlignmentStopKind.Converged))) : Fin.Fail<AlignmentStep>(key.InvalidResult());
        }
        AlignmentStep exhausted = new(Delta: Transform.Identity);
        for (int line = 0; line < 8; line++) {
            double scale = Math.Pow(x: 0.5, y: line);
            Transform delta = ComposeRigidTransform(
                omega: new Vector3d(x: solution.Solution[0] * scale, y: solution.Solution[1] * scale, z: solution.Solution[2] * scale),
                translation: new Vector3d(x: solution.Solution[3] * scale, y: solution.Solution[4] * scale, z: solution.Solution[5] * scale));
            GicpObjective candidate = GicpObjectiveOf(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: delta * current, key: key).Match(Succ: static value => value, Fail: _ => new GicpObjective(Cost: double.PositiveInfinity, MeanMahalanobis: double.PositiveInfinity, MaxMahalanobis: double.PositiveInfinity));
            if (RhinoMath.IsValidDouble(x: candidate.Cost) && candidate.Cost < initial.Cost) {
                AlignmentOptimizerReceipt accepted = new(
                    Stop: AlignmentOptimizerStopKind.LineSearchAccepted,
                    LineSearchSteps: line + 1,
                    InitialCost: initial.Cost,
                    FinalCost: candidate.Cost,
                    StepNorm: stepNorm,
                    StepScale: scale,
                    MeanMahalanobis: candidate.MeanMahalanobis,
                    MaxMahalanobis: candidate.MaxMahalanobis,
                    Solve: Some(solution),
                    SourcePca: sourcePca.Receipt,
                    TargetPca: targetPca.Receipt);
                return accepted.IsValid ? Fin.Succ(new AlignmentStep(Delta: delta, Solve: Some(solution), Optimizer: Some(accepted))) : Fin.Fail<AlignmentStep>(key.InvalidResult());
            }
        }
        AlignmentOptimizerReceipt receipt = new(
            Stop: AlignmentOptimizerStopKind.LineSearchExhausted,
            LineSearchSteps: 8,
            InitialCost: initial.Cost,
            FinalCost: initial.Cost,
            StepNorm: stepNorm,
            StepScale: 0.0,
            MeanMahalanobis: initial.MeanMahalanobis,
            MaxMahalanobis: initial.MaxMahalanobis,
            Solve: Some(solution),
            SourcePca: sourcePca.Receipt,
            TargetPca: targetPca.Receipt);
        return receipt.IsValid ? Fin.Succ(exhausted with { Solve = Some(solution), Optimizer = Some(receipt), Stop = Some(AlignmentStopKind.OptimizerStopped) }) : Fin.Fail<AlignmentStep>(key.InvalidResult());
    }
    private static Fin<GicpObjective> GicpObjectiveOf(Seq<Point3d> source, AlignmentMatch match, CloudNeighborhoodPcaResult sourcePca, CloudNeighborhoodPcaResult targetPca, Transform current, Op key) =>
        key.Catch(() => {
            double total = 0.0;
            double max = 0.0;
            for (int i = 0; i < source.Count; i++) {
                Point3d x = current * source[index: i];
                Vector3d residual = match.Targets[i] - x;
                MathNet.Numerics.LinearAlgebra.Vector<double> solved = GicpPrecisionOf(current: current, source: sourcePca.Samples[index: i].Covariance, target: targetPca.Samples[index: match.TargetIndices[i]].Covariance)
                    .Multiply(DenseVectorD.OfArray([residual.X, residual.Y, residual.Z]));
                double mahalanobis = (residual.X * solved[0]) + (residual.Y * solved[1]) + (residual.Z * solved[2]);
                if (!RhinoMath.IsValidDouble(x: mahalanobis) || mahalanobis < 0.0) return Fin.Fail<GicpObjective>(key.InvalidResult());
                total += match.RowMass[i] * mahalanobis;
                max = Math.Max(val1: max, val2: mahalanobis);
            }
            double massTotal = match.RowMass.Sum();
            double mean = massTotal > RhinoMath.ZeroTolerance ? total / massTotal : total;
            return RhinoMath.IsValidDouble(x: total) && RhinoMath.IsValidDouble(x: mean) && RhinoMath.IsValidDouble(x: massTotal)
                ? Fin.Succ(new GicpObjective(Cost: total, MeanMahalanobis: mean, MaxMahalanobis: max))
                : Fin.Fail<GicpObjective>(key.InvalidResult());
        });
    private static LinearMatrix GicpPrecisionOf(Transform current, SymmetricMatrix source, SymmetricMatrix target) {
        LinearMatrix rotation = DenseMatrixD.Create(rows: 3, columns: 3, init: (row, col) => current[row, col]);
        LinearMatrix metric = MatrixKernel.ToMathNet(target.ToDense()) + (rotation * MatrixKernel.ToMathNet(source.ToDense()) * rotation.Transpose());
        return (DenseMatrixD)metric.Cholesky().Solve(DenseMatrixD.CreateIdentity(order: 3));
    }
    private static double[] GicpJacobianOf(Point3d point) => [
        0.0, -point.Z, point.Y, -1.0, 0.0, 0.0,
        point.Z, 0.0, -point.X, 0.0, -1.0, 0.0,
        -point.Y, point.X, 0.0, 0.0, 0.0, -1.0,
    ];
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
