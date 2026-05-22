namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RegistrationKind {
    public static readonly RegistrationKind PointToPoint = new(key: 0);
    public static readonly RegistrationKind PointToPlane = new(key: 1);
    public static readonly RegistrationKind Symmetric = new(key: 2);
    public static readonly RegistrationKind Robust = new(key: 3);
    public Fin<DualQuaternion> Align(VectorCloud source, VectorCloud target, Context context, Op? key = null) =>
        PopulationKernel.AlignClouds(kind: this, source: source, target: target, context: context, key: key.OrDefault());
}

[Union]
public abstract partial record HullKind {
    private HullKind() { }
    public sealed record ConvexCase : HullKind;
    public sealed record AlphaCase(double Radius) : HullKind;
    public sealed record ChiCase(double Lambda) : HullKind;
    public static HullKind Convex => new ConvexCase();
    public static Fin<HullKind> Alpha(double alpha, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: alpha) && alpha > 0.0
            ? Fin.Succ<HullKind>(new AlphaCase(Radius: alpha))
            : Fin.Fail<HullKind>(op.InvalidInput());
    }
    public static Fin<HullKind> Chi(double lambda, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: lambda) && lambda > 0.0
            ? Fin.Succ<HullKind>(new ChiCase(Lambda: lambda))
            : Fin.Fail<HullKind>(op.InvalidInput());
    }
    public Fin<VectorCloud> Compute(VectorCloud source, Context context, Op? key = null) =>
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
    public static Fin<SamplingKind> PoissonDisk(double radius, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius).Map(r => (SamplingKind)new PoissonDiskCase(Radius: r));
    }
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
    public Fin<VectorCloud> Sample(MeshSpace domain, Context context, Op? key = null) =>
        PopulationKernel.SampleOnMesh(kind: this, domain: domain, context: context, key: key.OrDefault());
    public Fin<VectorCloud> Sample(ScalarField domain, BoundingBox region, Context context, Op? key = null) =>
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
    public static Fin<RemeshKind> Quad(VectorField crossField, double targetEdge, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: targetEdge).Map(t => (RemeshKind)new QuadCase(CrossField: crossField, TargetEdge: t));
    }
    public static Fin<RemeshKind> Simplify(int targetFaces, bool useQem, Op? key = null) =>
        targetFaces >= 1 ? Fin.Succ<RemeshKind>(new SimplifyCase(TargetFaces: targetFaces, UseQem: useQem)) : Fin.Fail<RemeshKind>(key.OrDefault().InvalidInput());
    public static Fin<RemeshKind> AdaptiveCurvature(ScalarField curvatureField, double minEdge, double maxEdge, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: minEdge) && RhinoMath.IsValidDouble(x: maxEdge) && minEdge > 0.0 && maxEdge > minEdge
            ? Fin.Succ<RemeshKind>(new AdaptiveCurvatureCase(CurvatureField: curvatureField, MinEdge: minEdge, MaxEdge: maxEdge))
            : Fin.Fail<RemeshKind>(op.InvalidInput());
    }
    public Fin<Mesh> Apply(MeshSpace space, Context context, Op? key = null) =>
        PopulationKernel.ApplyRemesh(kind: this, space: space, context: context, key: key.OrDefault());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class PopulationKernel {
    // Umeyama 1991 Procrustes: rigid alignment via SVD of cross-covariance H = X^T Y.
    // R = V * diag(1,1,det(VU^T)) * U^T; t = ȳ - R x̄.
    internal static Fin<DualQuaternion> AlignClouds(RegistrationKind kind, VectorCloud source, VectorCloud target, Context context, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) when kind == RegistrationKind.PointToPoint
                => ProcrustesAlign(source: src.Vertices, target: tgt.Vertices, context: context, key: key),
            (VectorCloud.ClusterCase, VectorCloud.ClusterCase) => Fin.Fail<DualQuaternion>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(DualQuaternion))),
            _ => Fin.Fail<DualQuaternion>(key.InvalidInput()),
        };
    // ALGORITHM KERNEL — cross-covariance H = X^T Y accumulated via loop; SVD reduction follows.
    private static Fin<DualQuaternion> ProcrustesAlign(Seq<Point3d> source, Seq<Point3d> target, Context context, Op key) {
        if (source.Count != target.Count || source.Count < 3) return Fin.Fail<DualQuaternion>(key.InvalidInput());
        Point3d srcCentroid = CentroidOf(points: source); Point3d tgtCentroid = CentroidOf(points: target);
        Dimension dim3 = Dimension.Create(value: 3);
        double[] crossCov = new double[9];
        for (int i = 0; i < source.Count; i++) {
            Vector3d sv = source[index: i] - srcCentroid; Vector3d tv = target[index: i] - tgtCentroid;
            crossCov[0] += sv.X * tv.X; crossCov[1] += sv.X * tv.Y; crossCov[2] += sv.X * tv.Z;
            crossCov[3] += sv.Y * tv.X; crossCov[4] += sv.Y * tv.Y; crossCov[5] += sv.Y * tv.Z;
            crossCov[6] += sv.Z * tv.X; crossCov[7] += sv.Z * tv.Y; crossCov[8] += sv.Z * tv.Z;
        }
        return from H in Matrix.Of(rows: dim3, cols: dim3, entries: new Arr<double>(crossCov), key: key)
               from svd in H.DecomposeSvd(key: key)
               from rotMatrix in svd.IsValid ? BuildRotation(u: svd.U, v: svd.V) : Fin.Fail<Transform>(key.InvalidResult())
               from result in BuildAlignment(rotation: rotMatrix, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, key: key)
               select result;
    }
    private static Point3d CentroidOf(Seq<Point3d> points) {
        Vector3d sum = points.Fold(initialState: Vector3d.Zero, f: static (acc, p) => acc + (Vector3d)p);
        return Point3d.Origin + (sum / points.Count);
    }
    private static Fin<Transform> BuildRotation(Matrix u, Matrix v) {
        Matrix vu = v * u.Transpose();
        double det = vu.Determinant().IfFail(0.0);
        double[] diag = [1.0, 1.0, det >= 0.0 ? 1.0 : -1.0];
        Matrix d = new(Rows: Dimension.Create(value: 3), Cols: Dimension.Create(value: 3),
            Entries: [.. Enumerable.Range(0, 9).Select(idx => (idx / 3) == (idx % 3) ? diag[idx / 3] : 0.0)]);
        Matrix rot = v * d * u.Transpose();
        Transform xform = Transform.Identity;
        for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) xform[i, j] = rot.At(i: i, j: j);
        return Fin.Succ(xform);
    }
    private static Fin<DualQuaternion> BuildAlignment(Transform rotation, Point3d srcCentroid, Point3d tgtCentroid, Op key) {
        Point3d rotated = rotation * srcCentroid;
        Vector3d translation = tgtCentroid - rotated;
        Transform aligned = rotation;
        aligned[0, 3] = translation.X; aligned[1, 3] = translation.Y; aligned[2, 3] = translation.Z;
        return key.AcceptValue(value: DualQuaternion.Of(transform: aligned));
    }

    // Hull / sampling / remesh substrates ship as honest Op.Unsupported until consumers exist
    // — per `feedback_speculative_scaffolding`, structural surfaces stay; algorithms ship when
    // consumers demand them rather than as zero-consumer scaffolding.
    internal static Fin<VectorCloud> ComputeHull(HullKind kind, VectorCloud source, Context context, Op key) =>
        Fin.Fail<VectorCloud>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud)));
    internal static Fin<VectorCloud> SampleOnMesh(SamplingKind kind, MeshSpace domain, Context context, Op key) =>
        Fin.Fail<VectorCloud>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud)));
    internal static Fin<VectorCloud> SampleInScalarField(SamplingKind kind, ScalarField domain, BoundingBox region, Context context, Op key) =>
        Fin.Fail<VectorCloud>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(VectorCloud)));
    internal static Fin<Mesh> ApplyRemesh(RemeshKind kind, MeshSpace space, Context context, Op key) =>
        Fin.Fail<Mesh>(key.Unsupported(geometryType: kind.GetType(), outputType: typeof(Mesh)));
    // Hoppe-DeRose-Duchamp-McDonald-Stuetzle 1992 MST normal propagation. Cloud-level entry.
    internal static Fin<Seq<Vector3d>> OrientNormalsViaMst(VectorCloud cloud, Context context, Op key) =>
        Fin.Fail<Seq<Vector3d>>(key.Unsupported(geometryType: typeof(VectorCloud), outputType: typeof(Seq<Vector3d>)));
}
