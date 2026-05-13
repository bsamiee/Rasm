namespace Rasm.Analysis;

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record Query<TGeometry, TOut>(
    Op Key,
    Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> Evaluate,
    Requirement Requirement,
    bool RequiresContext,
    Option<Error> Rejection,
    Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> AggregatePlan) where TGeometry : notnull {
    internal Query(Op key, Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>> effect, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default, Option<Error> rejection = default)
        : this(Key: key, Evaluate: effect, Requirement: requirement ?? Requirement.None, RequiresContext: requiresContext, Rejection: rejection, AggregatePlan: aggregate) { }
    public Eff<Env, Seq<TOut>> Apply(TGeometry geometry) => Evaluate(arg: Seq(geometry));
    public Eff<Env, Seq<TOut>> Apply(Seq<TGeometry> geometry) => Evaluate(arg: geometry);
    internal Query<TIn, TOut> Contramap<TIn>(Func<TIn, TGeometry> map) where TIn : notnull => new(
        Key: Key, Requirement: Requirement, RequiresContext: RequiresContext, Rejection: Rejection,
        AggregatePlan: AggregatePlan.Map<Func<Seq<TIn>, Eff<Env, Seq<TOut>>>>(project => input => project(arg: input.Map(value => map(arg: value)))),
        Evaluate: input => Evaluate(arg: input.Map(value => map(arg: value))));
    public Query<TGeometry, TOut> Aggregate() => AggregatePlan.Match(
        Some: project => this with { Evaluate = project },
        None: () => Reject(key: Key, fault: Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))));
    internal static Query<TGeometry, TOut> Build(Op key, Func<TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) =>
        Build(key: key, state: Unit.Default, evaluator: (_, geometry) => evaluator(arg: geometry), requirement: requirement, requiresContext: requiresContext, aggregate: aggregate);
    internal static Query<TGeometry, TOut> Build<TState>(Op key, TState state, Func<TState, TGeometry, Eff<Env, Seq<TOut>>> evaluator, Requirement? requirement = null, bool requiresContext = false, Option<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>> aggregate = default) {
        Requirement active = requirement ?? Requirement.None;
        return new(
            key: key, requirement: active, requiresContext: requiresContext,
            aggregate: aggregate.Map<Func<Seq<TGeometry>, Eff<Env, Seq<TOut>>>>(project => geometry =>
                from runtime in Env.EnvAsks
                from resolved in geometry.Traverse(item => Prepare(geometry: item, requirement: active).Run(env: runtime)).As().ToEff()
                from result in project(arg: resolved)
                select result),
            effect: geometry => from runtime in Env.EnvAsks
                                from result in geometry.Traverse(item => (
                                    from prepared in Prepare(geometry: item, requirement: active)
                                    from value in evaluator(arg1: state, arg2: prepared)
                                    select value).Run(env: runtime))
                                .Map(static chunks => chunks.Bind(static chunk => chunk))
                                .As().ToEff()
                                select result);
    }
    internal static Query<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(key: key, effect: _ => Fin.Fail<Seq<TOut>>(fault).ToEff(), rejection: Some(fault));
    private static Eff<Env, TGeometry> Prepare(TGeometry geometry, Requirement requirement) =>
        from runtime in Env.EnvAsks
        from ready in (runtime.Cancellation.IsCancellationRequested switch {
            true => Fin.Fail<TGeometry>(new Fault.Cancelled()),
            false => Optional(geometry).ToFin(new Fault.MissingGeometry()),
        }).ToEff()
        from validated in (requirement.IsEmpty, ready) switch {
            (false, GeometryBase native) => from context in Env.Asks
                                            from _ in context.Validate(geometry: native, requirement: requirement).ToEff()
                                            select ready,
            _ => Fin.Succ(ready).ToEff(),
        }
        select validated;
}

// --- [TYPES] ------------------------------------------------------------------------------
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
public enum MeshFaceMetric { None = 0, AspectRatio = 1, Area = 2, Perimeter = 3, Skewness = 4, DihedralAngle = 5 }
public static class MeshFaceMetrics {
    public static Option<double> Sample(this MeshFaceMetric metric, Mesh mesh, int face) {
        ArgumentNullException.ThrowIfNull(argument: mesh);
        return metric switch {
            MeshFaceMetric.AspectRatio => Some(mesh.Faces.GetFaceAspectRatio(index: face)),
            MeshFaceMetric.Area => Some(FaceArea(mesh: mesh, face: face)),
            MeshFaceMetric.Perimeter => Some(FacePerimeter(mesh: mesh, face: face)),
            MeshFaceMetric.Skewness => Some(FaceSkewness(mesh: mesh, face: face)),
            MeshFaceMetric.DihedralAngle => Some(FaceMaxDihedral(mesh: mesh, face: face)),
            _ => Option<double>.None,
        };
    }
    private static double FaceArea(Mesh mesh, int face) =>
        new MeshFaceProjection(Mesh: mesh, Face: face).Vertices switch {
            Seq<Point3d> v when v.Count == 4 => 0.5 * (Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length + Vector3d.CrossProduct(a: v[2] - v[0], b: v[3] - v[0]).Length),
            Seq<Point3d> v => 0.5 * Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length,
        };
    private static double FacePerimeter(Mesh mesh, int face) =>
        new MeshFaceProjection(Mesh: mesh, Face: face).Vertices switch {
            Seq<Point3d> v => v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d),
        };
    private static double FaceSkewness(Mesh mesh, int face) {
        Seq<Point3d> v = new MeshFaceProjection(Mesh: mesh, Face: face).Vertices;
        double ideal = v.Count == 4 ? Math.PI / 2.0 : Math.PI / 3.0;
        return v.Map((vertex, i) => Vector3d.VectorAngle(a: v[(i + v.Count - 1) % v.Count] - vertex, b: v[(i + 1) % v.Count] - vertex))
            .Fold(initialState: 0.0, f: (acc, angle) => Math.Max(val1: acc, val2: Math.Max(val1: (angle - ideal) / (Math.PI - ideal), val2: (ideal - angle) / ideal)));
    }
    private static double FaceMaxDihedral(Mesh mesh, int face) {
        Vector3d normal = new MeshFaceProjection(Mesh: mesh, Face: face).Normal;
        return normal.IsValid switch {
            false => 0.0,
            true => toSeq(mesh.TopologyEdges.GetEdgesForFace(faceIndex: face))
                .Bind(edge => toSeq(mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge)).Filter(other => other != face))
                .Fold(initialState: 0.0, f: (max, other) => new MeshFaceProjection(Mesh: mesh, Face: other).Normal switch {
                    { IsValid: true } neighbour => Math.Max(val1: max, val2: Vector3d.VectorAngle(a: normal, b: neighbour)),
                    _ => max,
                }),
        };
    }
}
[StructLayout(LayoutKind.Auto)] public readonly record struct CurvatureProfile(CurvatureScalar Scalar, int Count, double Minimum, double Maximum, double Mean, double Variance);
[StructLayout(LayoutKind.Auto)] public readonly record struct ResidualProfile(int Count, double Minimum, double Maximum, double Mean, double Variance, double Rms, double Tolerance, bool WithinTolerance);
[StructLayout(LayoutKind.Auto)] public readonly record struct MeshFaceSample(int Face, double Value);
[StructLayout(LayoutKind.Auto)] public readonly record struct Hit(int Id);
[StructLayout(LayoutKind.Auto)] public readonly record struct Couple(int A, int B);
[StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);

// --- [ASPECTS] ----------------------------------------------------------------------------
[Union]
public partial record Bounds {
    public sealed record Box : Bounds; public sealed record Oriented(Plane Plane) : Bounds; public sealed record Transformed(Transform Transform) : Bounds; public sealed record Center : Bounds;
    public sealed record Corners : Bounds; public sealed record Edges : Bounds; public sealed record Area : Bounds; public sealed record Volume : Bounds;
}
[Union]
public partial record Measure {
    public sealed record Length : Measure; public sealed record Area : Measure; public sealed record Volume : Measure; public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure; public sealed record MassError(MassKind Mass) : Measure; public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure; public sealed record PrincipalAxes(MassKind Mass) : Measure;
}
[Union]
public partial record Location {
    public sealed record Midpoint : Location; public sealed record Tangent : Location; public sealed record Closest(Point3d Point) : Location;
    public sealed record PointAtCurve(double Parameter) : Location; public sealed record PointAtSurface(Point2d Uv) : Location; public sealed record PointAtLength(double Length) : Location;
    public sealed record FrameAtCurve(double Parameter) : Location; public sealed record FrameAtSurface(Point2d Uv) : Location; public sealed record PerpendicularFrameAt(double Parameter) : Location;
    public sealed record NormalAt(Point2d Uv) : Location; public sealed record CurvatureAtCurve(double Parameter) : Location; public sealed record CurvatureAtSurface(Point2d Uv) : Location;
    public sealed record CurvatureProfile(int Count, CurvatureScalar Scalar) : Location; public sealed record DerivativeAt(double Parameter, int Count) : Location;
    public sealed record DivideByCount(int Count) : Location; public sealed record DivideByLength(double Length) : Location; public sealed record Orientation(Plane Plane) : Location;
    public sealed record Contains(Point3d Point, Plane Plane) : Location; public sealed record ShortPath(Point2d Start, Point2d End) : Location;
    public sealed record ControlPoints : Location;
}
[Union]
public partial record Faces {
    public sealed record AllCase : Faces; public sealed record TopCase(Vector3d Axis) : Faces; public sealed record BottomCase(Vector3d Axis) : Faces; public sealed record AtCase(int? Value) : Faces;
    public static Faces All => new AllCase(); public static Faces Top(Vector3d axis) => new TopCase(Axis: axis); public static Faces Bottom(Vector3d axis) => new BottomCase(Axis: axis);
    public static Faces At(int? index = null) => new AtCase(Value: index);
}
public enum MeshCheckCount {
    None = 0, DegenerateFaces = 1, DisjointMeshes = 2, DuplicateFaces = 3, ExtremelyShortEdges = 4,
    InvalidNgons = 5, NakedEdges = 6, NonManifoldEdges = 7, NonUnitVectorNormals = 8,
    RandomFaceNormals = 9, SelfIntersectingPairs = 10, UnusedVertices = 11,
    VertexFaceNormalsDiffer = 12, ZeroLengthNormals = 13,
}
public static class MeshCheckCounts {
    public static int Get(this MeshCheckCount metric, MeshCheckParameters parameters) => metric switch {
        MeshCheckCount.DegenerateFaces => parameters.DegenerateFaceCount,
        MeshCheckCount.DisjointMeshes => parameters.DisjointMeshCount,
        MeshCheckCount.DuplicateFaces => parameters.DuplicateFaceCount,
        MeshCheckCount.ExtremelyShortEdges => parameters.ExtremelyShortEdgeCount,
        MeshCheckCount.InvalidNgons => parameters.InvalidNgonCount,
        MeshCheckCount.NakedEdges => parameters.NakedEdgeCount,
        MeshCheckCount.NonManifoldEdges => parameters.NonManifoldEdgeCount,
        MeshCheckCount.NonUnitVectorNormals => parameters.NonUnitVectorNormalCount,
        MeshCheckCount.RandomFaceNormals => parameters.RandomFaceNormalCount,
        MeshCheckCount.SelfIntersectingPairs => parameters.SelfIntersectingPairsCount,
        MeshCheckCount.UnusedVertices => parameters.UnusedVertexCount,
        MeshCheckCount.VertexFaceNormalsDiffer => parameters.VertexFaceNormalsDifferCount,
        MeshCheckCount.ZeroLengthNormals => parameters.ZeroLengthNormalCount,
        _ => 0,
    };
    internal static IEnumerable<MeshCheckCount> Defects => Enum.GetValues<MeshCheckCount>().Where(static m => m != MeshCheckCount.None);
}
[Union]
public partial record Curves {
    public sealed record AllCase : Curves; public sealed record SegmentsCase : Curves; public sealed record BoundaryCase : Curves; public sealed record NakedOuterCase : Curves; public sealed record NakedInnerCase : Curves; public sealed record InteriorCase : Curves; public sealed record NonManifoldCase : Curves; public sealed record OuterLoopCase : Curves; public sealed record InnerLoopCase : Curves; public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves; public sealed record SubCurvesCase : Curves; public sealed record SilhouetteCase(Vector3d? Direction) : Curves; public sealed record DraftCase(Vector3d? Direction, double? Angle) : Curves; public sealed record AtCase(int? Value) : Curves;
    public static Curves All => new AllCase(); public static Curves Segments => new SegmentsCase(); public static Curves Boundary => new BoundaryCase(); public static Curves NakedOuter => new NakedOuterCase();
    public static Curves NakedInner => new NakedInnerCase(); public static Curves Interior => new InteriorCase(); public static Curves NonManifold => new NonManifoldCase(); public static Curves OuterLoop => new OuterLoopCase();
    public static Curves InnerLoop => new InnerLoopCase(); public static Curves SubCurves => new SubCurvesCase();
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new DraftCase(Direction: direction, Angle: angle);
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal CurveSelector ToSelector(Topology topology) => this switch {
        AllCase => new CurveSelector(Feature: topology == Topology.Curve ? CurveFeature.Input : CurveFeature.Edge, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        SegmentsCase => new CurveSelector(Feature: CurveFeature.Segment, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        BoundaryCase => new CurveSelector(Feature: CurveFeature.Boundary, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        NakedOuterCase => new CurveSelector(Feature: CurveFeature.NakedOuter, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        NakedInnerCase => new CurveSelector(Feature: CurveFeature.NakedInner, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        InteriorCase => new CurveSelector(Feature: CurveFeature.Interior, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        NonManifoldCase => new CurveSelector(Feature: CurveFeature.NonManifold, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        OuterLoopCase => new CurveSelector(Feature: CurveFeature.OuterLoop, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        InnerLoopCase => new CurveSelector(Feature: CurveFeature.InnerLoop, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        SubCurvesCase => new CurveSelector(Feature: CurveFeature.SubCurve, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
        IsoCase iso => new CurveSelector(Feature: CurveFeature.Iso, Direction: None, Angle: None, Index: None, Normalized: Some(iso.Normalized), Iso: Some(iso.Direction)),
        SilhouetteCase silhouette => new CurveSelector(Feature: CurveFeature.Silhouette, Direction: Optional(silhouette.Direction), Angle: None, Index: None, Normalized: None, Iso: None),
        DraftCase draft => new CurveSelector(Feature: CurveFeature.Draft, Direction: Optional(draft.Direction), Angle: Optional(draft.Angle), Index: None, Normalized: None, Iso: None),
        AtCase at => new CurveSelector(Feature: topology == Topology.Curve ? CurveFeature.Input : CurveFeature.Edge, Direction: None, Angle: None, Index: Optional(at.Value), Normalized: None, Iso: None),
        _ => new CurveSelector(Feature: CurveFeature.Input, Direction: None, Angle: None, Index: None, Normalized: None, Iso: None),
    };
}
[Union]
public partial record Meshes {
    public sealed record ValidityBundleCase : Meshes; public sealed record StatsBundleCase : Meshes; public sealed record DefectsBundleCase : Meshes;
    public sealed record FaceQualityCase(MeshFaceMetric Metric) : Meshes; public sealed record AtFaceCase(int? Value) : Meshes;
    public static Meshes ValidityBundle => new ValidityBundleCase(); public static Meshes StatsBundle => new StatsBundleCase(); public static Meshes DefectsBundle => new DefectsBundleCase();
    public static Meshes FaceQuality(MeshFaceMetric metric) => new FaceQualityCase(Metric: metric);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
}
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record ProfileResidual(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Query {
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(Op key, object query) where TGeometry : notnull => query switch {
        Query<TGeometry, TOut> typed => typed,
        _ => Query<TGeometry, TOut>.Reject(key: key, fault: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
    };
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue>(Op key, Func<TNative, Eff<Env, Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, Func<TNative, Eff<Env, Seq<TValue>>>>(key: key, state: project, project: static (nativeProject, native) => nativeProject(arg: native));
    internal static Query<TGeometry, TOut> Native<TGeometry, TOut, TNative, TValue, TState>(Op key, TState state, Func<TState, TNative, Eff<Env, Seq<TValue>>> project, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull where TNative : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (Key: key, State: state, Project: project),
            evaluator: static (state, geometry) => geometry switch {
                TNative native => state.Project(arg1: state.State, arg2: native),
                _ => Fin.Fail<Seq<TValue>>(state.Key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TValue))).ToEff(),
            }));
    internal static Fin<Seq<TValue>> One<TValue>(this Op key, TValue value) => key.RequireValid(value: value).Map(static candidate => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue>? values) => Optional(values).ToSeq().Bind(static value => value.AsIterable().ToSeq()).Traverse(value => key.RequireValid(value: value)).As();
    internal static Fin<Seq<TValue>> Solved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch { true => key.One(value: value), false => Fin.Fail<Seq<TValue>>(key.InvalidResult()) };
    internal static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    internal static Unit DisposeAll<TResource>(Seq<TResource> resources) where TResource : class, IDisposable {
        _ = resources.Iter(static resource => resource.Dispose());
        return Unit.Default;
    }
    internal static Fin<Seq<TOut>> Results<TValue, TOut>(this Op key, IEnumerable<TValue>? values) => typeof(TValue).Equals(typeof(TOut)) switch {
        true => Many(key: key, values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
        false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
    };
    internal static Eff<Env, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(TGeometry geometry, Op key, Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve => from context in Env.Asks
                           from validated in context.Validate(geometry: curve, requirement: Requirement.CurveLength).ToEff()
                           from parameter in (validated.NormalizedLengthParameter(s: 0.5, t: out double p, fractionalTolerance: context.Relative.Value) switch {
                               true => Fin.Succ(p),
                               false => Fin.Fail<double>(key.InvalidResult()),
                           }).ToEff()
                           from result in One(key: key, value: project(arg1: validated, arg2: parameter)).ToEff()
                           select result,
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TOut))).ToEff(),
        };
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal<TMass>(this Op key, TMass mass) where TMass : class =>
        mass switch {
            LengthMassProperties length => PrincipalFromMoments(key: key,
                solved: length.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => PrincipalFromMoments(key: key,
                solved: area.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => PrincipalFromMoments(key: key,
                solved: volume.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalFromMoments(Op key, bool solved, double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
}
