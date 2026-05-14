using System.Collections.Frozen;

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
            (false, GeometryBase native) => from _ in runtime.Context.Validate(geometry: native, requirement: requirement, cancel: runtime.Cancellation).ToEff()
                                            select ready,
            _ => Fin.Succ(ready).ToEff(),
        }
        select validated;
}

// --- [TYPES] ------------------------------------------------------------------------------
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
public enum MeshFaceMetric { None = 0, AspectRatio = 1, Area = 2, Perimeter = 3, Skewness = 4, DihedralAngle = 5 }
public static class MeshFaceMetrics {
    private static readonly FrozenDictionary<MeshFaceMetric, Func<MeshFaceProjection, Fin<double>>> Metrics =
        new Dictionary<MeshFaceMetric, Func<MeshFaceProjection, Fin<double>>> {
            [MeshFaceMetric.AspectRatio] = static projection => Fin.Succ(projection.Mesh.Faces.GetFaceAspectRatio(index: projection.Face)),
            [MeshFaceMetric.Area] = FaceArea,
            [MeshFaceMetric.Perimeter] = FacePerimeter,
            [MeshFaceMetric.Skewness] = FaceSkewness,
            [MeshFaceMetric.DihedralAngle] = FaceMaxDihedral,
        }.ToFrozenDictionary();
    public static Fin<double> Sample(this MeshFaceMetric metric, Mesh mesh, int face) {
        ArgumentNullException.ThrowIfNull(argument: mesh);
        return MeshFaceProjection.Create(mesh: mesh, face: face)
            .Bind(projection => Optional(Metrics.GetValueOrDefault(key: metric))
                .ToFin(Op.Of(name: nameof(MeshFaceMetric)).InvalidInput())
                .Bind(sample => sample(arg: projection)));
    }
    private static Fin<double> FaceArea(MeshFaceProjection projection) =>
        projection.Vertices.Map(vertices => vertices switch {
            Seq<Point3d> v when v.Count == 4 => 0.5 * (Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length + Vector3d.CrossProduct(a: v[2] - v[0], b: v[3] - v[0]).Length),
            Seq<Point3d> v => 0.5 * Vector3d.CrossProduct(a: v[1] - v[0], b: v[2] - v[0]).Length,
        });
    private static Fin<double> FacePerimeter(MeshFaceProjection projection) =>
        projection.Vertices.Map(static v => v.Map((p, i) => p.DistanceTo(other: v[(i + 1) % v.Count])).Fold(initialState: 0.0, f: static (acc, d) => acc + d));
    private static Fin<double> FaceSkewness(MeshFaceProjection projection) =>
        projection.Vertices.Map(static v => (Ideal: v.Count == 4 ? Math.PI / 2.0 : Math.PI / 3.0, Vertices: v))
            .Map(static state => state.Vertices.Map((vertex, i) => Vector3d.VectorAngle(a: state.Vertices[(i + state.Vertices.Count - 1) % state.Vertices.Count] - vertex, b: state.Vertices[(i + 1) % state.Vertices.Count] - vertex))
                .Fold(initialState: 0.0, f: (acc, angle) => Math.Max(val1: acc, val2: Math.Max(val1: (angle - state.Ideal) / (Math.PI - state.Ideal), val2: (state.Ideal - angle) / state.Ideal))));
    private static Fin<double> FaceMaxDihedral(MeshFaceProjection projection) =>
        projection.Normal.Bind(normal => normal.IsValid switch {
            false => Fin.Succ(0.0),
            true => toSeq(projection.Mesh.TopologyEdges.GetEdgesForFace(faceIndex: projection.Face))
                .Bind(edge => toSeq(projection.Mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge)).Filter(other => other != projection.Face))
                .Fold(initialState: Fin.Succ((Max: 0.0, projection.Mesh, Normal: normal)), f: static (state, other) => state.Bind(s => MeshFaceProjection.Create(mesh: s.Mesh, face: other)
                    .Bind(static otherProjection => otherProjection.Normal)
                    .Map(neighbour => neighbour.IsValid switch {
                        true => (Math.Max(val1: s.Max, val2: Vector3d.VectorAngle(a: s.Normal, b: neighbour)), s.Mesh, s.Normal),
                        false => s,
                    })))
                .Map(static state => state.Max),
        });
}
[StructLayout(LayoutKind.Auto)] public readonly record struct CurvatureProfile(CurvatureScalar Scalar, int Count, double Minimum, double Maximum, double Mean, double Variance);
[StructLayout(LayoutKind.Auto)] public readonly record struct ResidualProfile(int Count, double Minimum, double Maximum, double Mean, double Variance, double Rms, double Tolerance, bool WithinTolerance);
[StructLayout(LayoutKind.Auto)] public readonly record struct MeshFaceSample(int Face, double Value);
[StructLayout(LayoutKind.Auto)] public readonly record struct Hit(int Id);
[StructLayout(LayoutKind.Auto)] public readonly record struct Couple(int A, int B);
[StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);

// --- [ASPECTS] ----------------------------------------------------------------------------
public interface IAspect {
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull;
}
[Union]
public partial record Bounds : IAspect {
    public sealed record Box : Bounds; public sealed record Oriented(Plane Plane) : Bounds; public sealed record Transformed(Transform Transform) : Bounds; public sealed record Center : Bounds;
    public sealed record Corners(bool Unique = false) : Bounds; public sealed record Edges : Bounds; public sealed record Area : Bounds; public sealed record Volume : Bounds;
    private static readonly Op BoundsKey = Op.Of(name: nameof(Bounds));
    private static readonly Op OrientedKey = Op.Of(name: "OrientedBounds");
    private static readonly Op TransformedKey = Op.Of(name: "TransformedBounds");
    private static readonly Op CenterKey = Op.Of(name: "BoundsCenter");
    private static readonly Op CornersKey = Op.Of(name: "BoundsCorners");
    private static readonly Op BoxEdgesKey = Op.Of(name: "BoxEdges");
    private static readonly Op BoxAreaKey = Op.Of(name: "BoxArea");
    private static readonly Op BoxVolumeKey = Op.Of(name: "BoxVolume");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        box: static _ => (typeof(TOut) == typeof(BoundingBox) && typeof(TGeometry).SupportsBounds(includeSphere: true))
            ? Query.Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                key: BoundsKey, state: BoundsKey,
                evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(b => Query.One(key: op, value: b)).ToEff()))
            : BoundsKey.Unsupported<TGeometry, TOut>(),
        oriented: static o => (typeof(TOut) == typeof(Rhino.Geometry.Box) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Query.Native<TGeometry, TOut, GeometryBase, Rhino.Geometry.Box, (Op Key, Plane Plane)>(
                key: OrientedKey, state: (OrientedKey, o.Plane),
                project: static (state, native) => new Rhino.Geometry.Box(basePlane: state.Plane, geometry: native) switch {
                    { IsValid: true } valid => Query.One(key: state.Key, value: valid).ToEff(),
                    _ => Fin.Fail<Seq<Rhino.Geometry.Box>>(state.Key.InvalidResult()).ToEff(),
                })
            : OrientedKey.Unsupported<TGeometry, TOut>(),
        transformed: static t => (typeof(TOut) == typeof(BoundingBox) && typeof(GeometryBase).IsAssignableFrom(c: typeof(TGeometry)))
            ? Query.Native<TGeometry, TOut, GeometryBase, BoundingBox, (Op Key, Transform Xform)>(
                key: TransformedKey, state: (Key: TransformedKey, Xform: t.Transform),
                project: static (state, native) => Query.One(key: state.Key, value: native.GetBoundingBox(xform: state.Xform)).ToEff())
            : TransformedKey.Unsupported<TGeometry, TOut>(),
        center: static _ => typeof(TOut) == typeof(Point3d)
            ? Query.Cast<TGeometry, TOut>(key: CenterKey, query: Query<TGeometry, Point3d>.Build(
                key: CenterKey, state: CenterKey,
                evaluator: static (op, geometry) => geometry.Bounds(op: op).Bind(b => Query.One(key: op, value: b.Center)).ToEff()))
            : CenterKey.Unsupported<TGeometry, TOut>(),
        corners: static c => typeof(TOut) == typeof(Point3d)
            ? Query.Cast<TGeometry, TOut>(key: CornersKey, query: Query<TGeometry, Point3d>.Build(
                key: CornersKey, requiresContext: c.Unique, state: (Key: CornersKey, c.Unique),
                evaluator: static (state, geometry) =>
                    from runtime in Env.EnvAsks
                    from bbox in geometry.Bounds(op: state.Key).ToEff()
                    from result in Query.Many(key: state.Key, values: state.Unique ? Point3d.CullDuplicates(points: bbox.GetCorners(), tolerance: runtime.Context.Absolute.Value) : bbox.GetCorners()).ToEff()
                    select result))
            : CornersKey.Unsupported<TGeometry, TOut>(),
        edges: static _ => (typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line))
            ? Query.Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                key: BoxEdgesKey, state: BoxEdgesKey,
                evaluator: static (op, geometry) => Query.Many(key: op, values: geometry.GetEdges()).ToEff()))
            : BoxEdgesKey.Unsupported<TGeometry, TOut>(),
        area: static _ => Query.BoxMetric<TGeometry, TOut>(key: BoxAreaKey, boundingBox: static g => g.Area, box: static g => g.Area),
        volume: static _ => Query.BoxMetric<TGeometry, TOut>(key: BoxVolumeKey, boundingBox: static g => g.Volume, box: static g => g.Volume));
}
[Union]
public partial record Measure : IAspect {
    public sealed record Length : Measure; public sealed record Area : Measure; public sealed record Volume : Measure; public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure; public sealed record MassError(MassKind Mass) : Measure; public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure; public sealed record PrincipalAxes(MassKind Mass) : Measure;
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        spatialMidpoint: static _ => typeof(TOut) == typeof(Point3d) ? Query.SpatialMidpoint<TGeometry, TOut>() : Op.Of(name: "SpatialMidpoint").Unsupported<TGeometry, TOut>(),
        length: static _ => Query.Length<TGeometry, TOut>(),
        area: static a => Query.MassMeasure<TGeometry, TOut>(mass: MassKind.Area, aspect: a),
        volume: static v => Query.MassMeasure<TGeometry, TOut>(mass: MassKind.Volume, aspect: v),
        massError: static e => Query.MassMeasure<TGeometry, TOut>(mass: e.Mass, aspect: e),
        centroid: static c => Query.MassMeasure<TGeometry, TOut>(mass: c.Mass, aspect: c),
        centroidError: static ce => Query.MassMeasure<TGeometry, TOut>(mass: ce.Mass, aspect: ce),
        radii: static r => Query.MassMeasure<TGeometry, TOut>(mass: r.Mass, aspect: r),
        principalAxes: static p => Query.MassMeasure<TGeometry, TOut>(mass: p.Mass, aspect: p));
}
[Union]
public partial record Location : IAspect {
    public sealed record Midpoint : Location; public sealed record Tangent : Location; public sealed record Closest(Point3d Point) : Location;
    public sealed record PointAtCurve(double Parameter) : Location; public sealed record PointAtSurface(Point2d Uv) : Location; public sealed record PointAtLength(double Length) : Location;
    public sealed record FrameAtCurve(double Parameter) : Location; public sealed record FrameAtSurface(Point2d Uv) : Location; public sealed record PerpendicularFrameAt(double Parameter) : Location;
    public sealed record NormalAt(Point2d Uv) : Location; public sealed record CurvatureAtCurve(double Parameter) : Location; public sealed record CurvatureAtSurface(Point2d Uv) : Location;
    public sealed record CurvatureProfile(int Count, CurvatureScalar Scalar) : Location; public sealed record DerivativeAt(double Parameter, int Count) : Location;
    public sealed record DivideByCount(int Count) : Location; public sealed record DivideByLength(double Length) : Location; public sealed record Orientation(Plane Plane) : Location;
    public sealed record Contains(Point3d Point, Plane Plane) : Location; public sealed record ShortPath(Point2d Start, Point2d End) : Location;
    private static readonly Op PointAtKey = Op.Of(name: "PointAt");
    private static readonly Op PointAtLengthKey = Op.Of(name: "PointAtLength");
    private static readonly Op FrameAtKey = Op.Of(name: "FrameAt");
    private static readonly Op PerpendicularFrameAtKey = Op.Of(name: "PerpendicularFrameAt");
    private static readonly Op CurvatureAtKey = Op.Of(name: "CurvatureAt");
    private static readonly Op DerivativeAtKey = Op.Of(name: "DerivativeAt");
    private static readonly Op DivideByCountKey = Op.Of(name: "DivideByCount");
    private static readonly Op DivideByLengthKey = Op.Of(name: "DivideByLength");
    private static readonly Op OrientationKey = Op.Of(name: "Orientation");
    private static readonly Op ContainsKey = Op.Of(name: "Contains");
    private static readonly Op NormalAtKey = Op.Of(name: "NormalAt");
    private static readonly Op ShortPathKey = Op.Of(name: "ShortPath");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        midpoint: static _ => Query.Mid<TGeometry, TOut>(),
        tangent: static _ => Query.TangentAtMiddle<TGeometry, TOut>(),
        closest: static c => Query.Closest<TGeometry, TOut>(point: c.Point),
        curvatureProfile: static cp => Query.CurvatureProfile<TGeometry, TOut>(count: cp.Count, scalar: cp.Scalar),
        pointAtCurve: static pac => Query.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, query: () => Query.CurveAt<TGeometry, Point3d>(key: PointAtKey, parameter: pac.Parameter, project: static (curve, p) => Query.One(key: PointAtKey, value: curve.PointAt(t: p)))),
        pointAtLength: static pal => Query.Located<TGeometry, TOut, Curve, Point3d>(
            key: PointAtLengthKey, query: () => Query<TGeometry, Point3d>.Build(
                key: PointAtLengthKey, requirement: Requirement.CurveLength, state: (Key: PointAtLengthKey, Distance: pal.Length),
                evaluator: static (state, geometry) => geometry switch {
                    Curve curve => from context in Env.Asks
                                   from result in (curve.LengthParameter(segmentLength: state.Distance, t: out double parameter, fractionalTolerance: context.Relative.Value) switch {
                                       true => Query.One(key: state.Key, value: curve.PointAt(t: parameter)),
                                       false => Fin.Fail<Seq<Point3d>>(state.Key.InvalidResult()),
                                   }).ToEff()
                                   select result,
                    _ => Fin.Fail<Seq<Point3d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))).ToEff(),
                })),
        frameAtCurve: static fac => Query.Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, query: () => Query.CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
        perpendicularFrameAt: static pfa => Query.Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, query: () => Query.CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
        curvatureAtCurve: static cac => Query.Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, query: () => Query.CurveAt<TGeometry, Vector3d>(key: CurvatureAtKey, parameter: cac.Parameter, project: static (curve, p) => Query.One(key: CurvatureAtKey, value: curve.CurvatureAt(t: p)))),
        derivativeAt: static da => da.Count < 0
            ? Query<TGeometry, TOut>.Reject(key: DerivativeAtKey, fault: DerivativeAtKey.InvalidInput())
            : Query.Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, query: () => Query.CurveAt<TGeometry, Vector3d>(key: DerivativeAtKey, parameter: da.Parameter, project: (curve, p) => Query.Many(key: DerivativeAtKey, values: curve.DerivativeAt(t: p, derivativeCount: da.Count)))),
        divideByCount: static dbc => Query.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, query: () => Query.DividePoly<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        divideByLength: static dbl => Query.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, query: () => Query.DividePoly<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        orientation: static o => Query.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, query: () => Query<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) => geometry switch {
                Curve curve => Query.One(key: state.Key, value: curve.ClosedCurveOrientation(plane: state.Frame)).ToEff(),
                _ => Fin.Fail<Seq<CurveOrientation>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
            })),
        contains: static cnt => Query.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, query: () => Query<TGeometry, PointContainment>.Build(
            key: ContainsKey, requiresContext: true, state: (Key: ContainsKey, Probe: cnt.Point, Frame: cnt.Plane),
            evaluator: static (state, geometry) => geometry switch {
                Curve curve => from context in Env.Asks
                               from result in (curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                                   PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                                   PointContainment containment => Query.One(key: state.Key, value: containment),
                               }).ToEff()
                               select result,
                _ => Fin.Fail<Seq<PointContainment>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))).ToEff(),
            })),
        pointAtSurface: static pas => Query.Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, query: () => Query.SurfaceUv<TGeometry, Point3d>(key: PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => Query.One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
        frameAtSurface: static fas => Query.Located<TGeometry, TOut, Surface, Plane>(key: FrameAtKey, query: () => Query.SurfaceUv<TGeometry, Plane>(
            key: FrameAtKey, uv: fas.Uv, project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                true => Query.One(key: FrameAtKey, value: frame),
                false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
            })),
        normalAt: static na => Query.Located<TGeometry, TOut, Surface, Vector3d>(key: NormalAtKey, query: () => Query.SurfaceUv<TGeometry, Vector3d>(
            key: NormalAtKey, uv: na.Uv, project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                Vector3d normal when normal.IsValid && !normal.IsTiny() => Query.One(key: NormalAtKey, value: normal),
                _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
            })),
        curvatureAtSurface: static cas => Query.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, query: () => Query.SurfaceUv<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
        shortPath: static sp => Query.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, query: () => Query.ShortPath<TGeometry>(start: sp.Start, end: sp.End)));
}
[Union]
public partial record Faces : IAspect {
    public sealed record AllCase : Faces; public sealed record TopCase(Vector3d Axis) : Faces; public sealed record BottomCase(Vector3d Axis) : Faces; public sealed record AtCase(int? Value) : Faces;
    public static Faces All => new AllCase();
    public static Faces Top(Vector3d? axis = null) => new TopCase(Axis: axis ?? Vector3d.ZAxis);
    public static Faces Bottom(Vector3d? axis = null) => new BottomCase(Axis: axis ?? Vector3d.ZAxis);
    public static Faces At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Faces));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull =>
        Dispatch.SupportsFaces(source: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Query.FaceQuery<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Transfer, project: static (chosen, _) => Query.Many(key: Key, values: chosen.Map(static face => face.Brep))),
                Type t when t == typeof(Plane) => Query.FaceQuery<TGeometry, TOut, Plane>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Point3d) => Query.FaceQuery<TGeometry, TOut, Point3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Query.FaceCentroid(face: face, runtime: runtime)).As()),
                Type t when t == typeof(Vector3d) => Query.FaceQuery<TGeometry, TOut, Vector3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, runtime) => chosen.Traverse(face => Query.FrameAtCentroid(face: face, runtime: runtime).Map(static frame => frame.ZAxis)).As()),
                Type t when t == typeof(int) => Query.FaceQuery<TGeometry, TOut, int>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => Query.Many(key: Key, values: chosen.Map(static face => face.FaceIndex))),
                Type t when t == typeof(ComponentIndex) => Query.FaceQuery<TGeometry, TOut, ComponentIndex>(key: Key, selector: this, requirement: Requirement.None, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => Query.Many(key: Key, values: chosen.Map(static face => new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex)))),
                Type t when t == typeof(Interval) => Query.FaceQuery<TGeometry, TOut, Interval>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation, ownership: ProjectionOwnership.Dispose, project: static (chosen, _) => chosen.Traverse(Query.FaceDomains).Map(static nested => nested.Bind(static domain => domain)).As()),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
}
[SmartEnum<int>]
public partial class MeshCheckCount {
    public static readonly MeshCheckCount None = new(key: 0, get: static _ => 0);
    public static readonly MeshCheckCount DegenerateFaces = new(key: 1, get: static p => p.DegenerateFaceCount);
    public static readonly MeshCheckCount DisjointMeshes = new(key: 2, get: static p => p.DisjointMeshCount);
    public static readonly MeshCheckCount DuplicateFaces = new(key: 3, get: static p => p.DuplicateFaceCount);
    public static readonly MeshCheckCount ExtremelyShortEdges = new(key: 4, get: static p => p.ExtremelyShortEdgeCount);
    public static readonly MeshCheckCount InvalidNgons = new(key: 5, get: static p => p.InvalidNgonCount);
    public static readonly MeshCheckCount NakedEdges = new(key: 6, get: static p => p.NakedEdgeCount);
    public static readonly MeshCheckCount NonManifoldEdges = new(key: 7, get: static p => p.NonManifoldEdgeCount);
    public static readonly MeshCheckCount NonUnitVectorNormals = new(key: 8, get: static p => p.NonUnitVectorNormalCount);
    public static readonly MeshCheckCount RandomFaceNormals = new(key: 9, get: static p => p.RandomFaceNormalCount);
    public static readonly MeshCheckCount SelfIntersectingPairs = new(key: 10, get: static p => p.SelfIntersectingPairsCount);
    public static readonly MeshCheckCount UnusedVertices = new(key: 11, get: static p => p.UnusedVertexCount);
    public static readonly MeshCheckCount VertexFaceNormalsDiffer = new(key: 12, get: static p => p.VertexFaceNormalsDifferCount);
    public static readonly MeshCheckCount ZeroLengthNormals = new(key: 13, get: static p => p.ZeroLengthNormalCount);
    private readonly Func<MeshCheckParameters, int> get;
    public int Get(MeshCheckParameters parameters) => get(arg: parameters);
    internal static IEnumerable<MeshCheckCount> Defects => Items.Where(static m => m != None);
}
[Union]
public partial record Curves : IAspect {
    public sealed record AllCase : Curves; public sealed record SegmentsCase : Curves; public sealed record BoundaryCase : Curves; public sealed record NakedOuterCase : Curves; public sealed record NakedInnerCase : Curves; public sealed record InteriorCase : Curves; public sealed record NonManifoldCase : Curves; public sealed record OuterLoopCase : Curves; public sealed record InnerLoopCase : Curves; public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves; public sealed record SubCurvesCase : Curves; public sealed record SilhouetteCase(Vector3d? Direction) : Curves; public sealed record DraftCase(Vector3d? Direction, double? Angle) : Curves; public sealed record AtCase(int? Value) : Curves;
    public static Curves All => new AllCase(); public static Curves Segments => new SegmentsCase(); public static Curves Boundary => new BoundaryCase(); public static Curves NakedOuter => new NakedOuterCase();
    public static Curves NakedInner => new NakedInnerCase(); public static Curves Interior => new InteriorCase(); public static Curves NonManifold => new NonManifoldCase(); public static Curves OuterLoop => new OuterLoopCase();
    public static Curves InnerLoop => new InnerLoopCase(); public static Curves SubCurves => new SubCurvesCase();
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new DraftCase(Direction: direction, Angle: angle);
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull =>
        Dispatch.SupportsCurves(source: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => Query.CurveProject<TGeometry, TOut, Curve>(key: Key, aspect: this, project: static p => p.Curve),
                Type t when t == typeof(CurveFeature) => Query.CurveProject<TGeometry, TOut, CurveFeature>(key: Key, aspect: this, project: static p => p.Feature),
                Type t when t == typeof(ComponentIndex) => Query.CurveProject<TGeometry, TOut, ComponentIndex>(key: Key, aspect: this, project: static p => p.Source),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
    internal Fin<Seq<CurveProjection>> Select(Seq<CurveProjection> curves) =>
        (this, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<CurveProjection>()),
            (AtCase { Value: int index }, int count) when index < 0 || index >= count => Fin.Fail<Seq<CurveProjection>>(Key.InvalidInput()),
            (AtCase { Value: int index }, _) => Fin.Succ(Seq(curves[index])),
            (AtCase, _) => Fin.Succ(Seq(curves[0])),
            _ => Fin.Succ(curves),
        };
    internal CurveSelector ToSelector(Topology topology) => this switch {
        AllCase => new(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }),
        SegmentsCase => new(Feature: CurveFeature.Segment),
        BoundaryCase => new(Feature: CurveFeature.Boundary),
        NakedOuterCase => new(Feature: CurveFeature.NakedOuter),
        NakedInnerCase => new(Feature: CurveFeature.NakedInner),
        InteriorCase => new(Feature: CurveFeature.Interior),
        NonManifoldCase => new(Feature: CurveFeature.NonManifold),
        OuterLoopCase => new(Feature: CurveFeature.OuterLoop),
        InnerLoopCase => new(Feature: CurveFeature.InnerLoop),
        SubCurvesCase => new(Feature: CurveFeature.SubCurve),
        IsoCase iso => new(Feature: CurveFeature.Iso, Normalized: Some(iso.Normalized), Iso: Some(iso.Direction)),
        SilhouetteCase s => new(Feature: CurveFeature.Silhouette, Direction: Optional(s.Direction)),
        DraftCase d => new(Feature: CurveFeature.Draft, Direction: Optional(d.Direction), Angle: Optional(d.Angle)),
        AtCase at => new(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }, Index: Optional(at.Value)),
        _ => new(Feature: CurveFeature.Input),
    };
}
[Union]
public partial record Meshes : IAspect {
    public sealed record ValidityBundleCase : Meshes; public sealed record StatsBundleCase : Meshes; public sealed record DefectsBundleCase : Meshes;
    public sealed record FaceQualityCase(MeshFaceMetric Metric) : Meshes; public sealed record AtFaceCase(int? Value) : Meshes;
    public static Meshes ValidityBundle => new ValidityBundleCase(); public static Meshes StatsBundle => new StatsBundleCase(); public static Meshes DefectsBundle => new DefectsBundleCase();
    public static Meshes FaceQuality(MeshFaceMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshFaceMetric.AspectRatio);
    public static Meshes AtFace(int? index = null) => new AtFaceCase(Value: index);
    private static readonly Op ValidityBundleKey = Op.Of(name: "MeshValidityBundle");
    private static readonly Op StatsBundleKey = Op.Of(name: "MeshStatsBundle");
    private static readonly Op DefectsBundleKey = Op.Of(name: "MeshDefectsBundle");
    private static readonly Op FaceMetricKey = Op.Of(name: nameof(MeshFaceMetric));
    private static readonly Op AtFaceKey = Op.Of(name: "MeshAtFace");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        validityBundleCase: static _ => Query.MeshLift<TGeometry, TOut, bool>(key: ValidityBundleKey, source: Query.MeshValidityBundle),
        statsBundleCase: static _ => Query.MeshLift<TGeometry, TOut, int>(key: StatsBundleKey, source: Query.MeshStatsBundle),
        defectsBundleCase: static _ => Query.MeshLift<TGeometry, TOut, int>(key: DefectsBundleKey, source: Query.MeshDefectsBundle),
        faceQualityCase: static fq => Query.MeshLift<TGeometry, TOut, MeshFaceSample>(key: FaceMetricKey, source: Query.MeshFaceMetric(metric: fq.Metric)),
        atFaceCase: static at => Query.MeshLift<TGeometry, TOut, MeshFaceProjection>(key: AtFaceKey, source: Query.MeshAtFace(index: at.Value)));
}
[Union]
public partial record PointSampling : IAspect {
    public sealed record Quadrants : PointSampling; public sealed record EdgeMidpoints : PointSampling; public sealed record Vertices : PointSampling; public sealed record ControlPoints : PointSampling;
    private static readonly Op QuadrantsKey = Op.Of(name: nameof(Quadrants));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        quadrants: static _ => typeof(TOut) == typeof(Point3d)
            ? Query.Cast<TGeometry, TOut>(key: QuadrantsKey, query: Query<TGeometry, Point3d>.Build(
                key: QuadrantsKey, requirement: Requirement.CurveLength, state: QuadrantsKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from result in (geometry switch {
                        Curve curve when curve.IsValid => Query.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value),
                        Polyline polyline when polyline.IsValid => Query.Bracket(factory: polyline.ToPolylineCurve, body: (Curve curve) => Query.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Line line when line.IsValid => Query.Bracket(factory: () => new LineCurve(line: line), body: (Curve curve) => Query.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Circle circle when circle.IsValid => Query.Bracket(factory: circle.ToNurbsCurve, body: (Curve curve) => Query.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Arc arc when arc.IsValid => Query.Bracket(factory: arc.ToNurbsCurve, body: (Curve curve) => Query.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    }).ToEff()
                    select result))
            : QuadrantsKey.Unsupported<TGeometry, TOut>(),
        edgeMidpoints: static _ => typeof(TOut) == typeof(Point3d) && Query.Supports(geometry: typeof(TGeometry), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)])
            ? Query.Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => Query.One(key: op, value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => Query.Many(key: op, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => Query.Many(key: op, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Box box => Query.Many(key: op, values: box.BoundingBox.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    _ => from runtime in Env.EnvAsks
                         from kind in ((object)geometry).Kind(ctx: runtime.Context).ToEff()
                         from curves in kind.Curves(value: geometry, selector: new CurveSelector(Feature: CurveFeature.Edge), ctx: runtime.Context, op: op, cancel: runtime.Cancellation).ToEff()
                         from result in Query.Many(key: op, values: curves.Map(static projection => { using Curve c = projection.Curve; return c.PointAtNormalizedLength(length: 0.5); })).ToEff()
                         select result,
                }))
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        vertices: static _ => typeof(TOut) == typeof(Point3d) && Dispatch.SupportsVertices(source: typeof(TGeometry))
            ? Query.Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                key: VerticesKey, requiresContext: true, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                    from points in kind.Vertices(value: geometry, ctx: context, op: op).ToEff()
                    from result in Query.Many(key: op, values: points).ToEff()
                    select result))
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPoints: static _ => typeof(TOut) == typeof(Point3d) && Dispatch.SupportsControlPoints(source: typeof(TGeometry))
            ? Query.Cast<TGeometry, TOut>(key: ControlPointsKey, query: Query<TGeometry, Point3d>.Build(
                key: ControlPointsKey, requiresContext: true, state: ControlPointsKey,
                evaluator: static (op, geometry) => from context in Env.Asks
                                                    from kind in ((object)geometry).Kind(ctx: context).ToEff()
                                                    from points in kind.ControlPoints(value: geometry, op: op).ToEff()
                                                    from result in Query.Many(key: op, values: points).ToEff()
                                                    select result))
            : ControlPointsKey.Unsupported<TGeometry, TOut>());
}
[Union]
public partial record Boundaries : IAspect {
    public sealed record NakedCase : Boundaries; public sealed record OutlineCase(Plane Plane) : Boundaries; public sealed record SelfIntersectionCase : Boundaries; public sealed record AllCase : Boundaries;
    public static Boundaries Naked => new NakedCase(); public static Boundaries Outline(Plane plane) => new OutlineCase(Plane: plane);
    public static Boundaries SelfIntersection => new SelfIntersectionCase(); public static Boundaries All => new AllCase();
    private static readonly Op NakedKey = Op.Of(name: "NakedEdges");
    private static readonly Op OutlineKey = Op.Of(name: "Outlines");
    private static readonly Op SelfIntersectionKey = Op.Of(name: "SelfIntersections");
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        nakedCase: static _ => (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Curve) => Query.Curves<TGeometry, TOut>(aspect: Curves.Boundary),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Polyline) => Query.Native<TGeometry, TOut, Mesh, Polyline>(key: NakedKey, project: static mesh => Query.Many(key: NakedKey, values: mesh.GetNakedEdges()).ToEff()),
            _ => NakedKey.Unsupported<TGeometry, TOut>(),
        },
        outlineCase: static o => (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline) && o.Plane.IsValid)
            ? Query.Cast<TGeometry, TOut>(key: OutlineKey, query: Query<Mesh, Polyline>.Build(key: OutlineKey, state: (Op: OutlineKey, o.Plane), evaluator: static (state, geometry) => Query.Many(key: state.Op, values: geometry.GetOutlines(plane: state.Plane)).ToEff()))
            : (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline))
                ? Query.Cast<TGeometry, TOut>(key: OutlineKey, query: Query<Mesh, Polyline>.Reject(key: OutlineKey, fault: OutlineKey.InvalidInput()))
                : OutlineKey.Unsupported<TGeometry, TOut>(),
        selfIntersectionCase: static _ => (typeof(TGeometry) == typeof(Mesh) && typeof(TOut) == typeof(Polyline))
            ? Query.Cast<TGeometry, TOut>(key: SelfIntersectionKey, query: Query<Mesh, Polyline>.Build(
                key: SelfIntersectionKey, requirement: Requirement.Basic, state: SelfIntersectionKey,
                evaluator: static (op, geometry) => from runtime in Env.EnvAsks
                                                    from result in Query.SelfIntersectionsValue(op: op, geometry: geometry, runtime: runtime).ToEff()
                                                    select result))
            : SelfIntersectionKey.Unsupported<TGeometry, TOut>(),
        allCase: static _ => Dispatch.SupportsCurves(source: typeof(TGeometry)) && typeof(TOut) == typeof(Curve)
            ? Query.Curves<TGeometry, TOut>(aspect: Curves.All)
            : Curves.Key.Unsupported<TGeometry, TOut>());
}
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record ProfileResidual(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ToQuery<TGeometry, TPrimitive, TOut>() where TGeometry : notnull where TPrimitive : notnull =>
        (this, typeof(TGeometry).AsKind().Case, Dispatch.SupportsPair(table: Dispatch.ConformanceTable, left: typeof(TGeometry), right: typeof(TPrimitive))) switch {
            (Distance { Count: <= 0 } or Rms { Count: <= 0 } or WithinTolerance { Count: <= 0 } or ProfileResidual { Count: <= 0 } or Maximum { Count: <= 0 }, _, _) =>
                Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, Kind { Topology: Topology.Curve }, true) => Query.ConformanceProject<TGeometry, TPrimitive, TOut>(aspect: this, requirement: Requirement.CurveLength),
            (_, Kind { Topology: Topology.Surface }, true) => Query.ConformanceProject<TGeometry, TPrimitive, TOut>(aspect: this, requirement: Requirement.SurfaceEvaluation),
            _ => Key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Query {
    public static Query<TGeometry, TOut> Aspect<TAspect, TGeometry, TOut>(TAspect? aspect, [CallerMemberName] string callerMember = "")
        where TAspect : class, IAspect
        where TGeometry : notnull =>
        aspect?.ToQuery<TGeometry, TOut>() ?? Query<TGeometry, TOut>.Reject(key: Op.Of(name: callerMember), fault: Op.Of(name: callerMember).InvalidInput());
    public static Query<TGeometry, TOut> Points<TGeometry, TOut>(PointSampling sampling) where TGeometry : notnull => Aspect<PointSampling, TGeometry, TOut>(aspect: sampling);
    public static Query<TGeometry, TOut> Boundaries<TGeometry, TOut>(Boundaries aspect) where TGeometry : notnull => Aspect<Boundaries, TGeometry, TOut>(aspect: aspect);
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
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue> values) =>
        Optional(values).ToFin(key.InvalidResult()).Bind(candidates => candidates.AsIterable().ToSeq().Traverse(value => key.RequireValid(value: value)).As());
    internal static Fin<Seq<TValue>> ManyOrEmpty<TValue>(this Op key, IEnumerable<TValue>? values) =>
        Optional(values).Match(Some: candidates => key.Many(values: candidates), None: () => Fin.Succ(Seq<TValue>()));
    internal static Fin<Seq<TValue>> Solved<TValue>(this Op key, bool isSolved, TValue value) =>
        isSolved switch { true => key.One(value: value), false => Fin.Fail<Seq<TValue>>(key.InvalidResult()) };
    internal static Fin<TOut> Bracket<TResource, TOut>(Func<TResource> factory, Func<TResource, Fin<TOut>> body) where TResource : class, IDisposable {
        using TResource resource = factory();
        return body(arg: resource);
    }
    internal static Fin<Seq<TOut>> Results<TValue, TOut>(this Op key, IEnumerable<TValue> values) => typeof(TValue).Equals(typeof(TOut)) switch {
        true => Many(key: key, values: values).Map(static candidates => candidates.Map(static candidate => (TOut)(object)candidate!)),
        false => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut))),
    };
    internal static Eff<Env, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(TGeometry geometry, Op key, Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve => from runtime in Env.EnvAsks
                           from validated in runtime.Context.Validate(geometry: curve, requirement: Requirement.CurveLength, cancel: runtime.Cancellation).ToEff()
                           from parameter in (validated.NormalizedLengthParameter(s: 0.5, t: out double p, fractionalTolerance: runtime.Context.Relative.Value) switch {
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
                solved: length.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => PrincipalFromMoments(key: key,
                solved: area.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => PrincipalFromMoments(key: key,
                solved: volume.WorldCoordinatesPrincipalMomentsOfInertia(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalFromMoments(Op key, bool solved, double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
}
