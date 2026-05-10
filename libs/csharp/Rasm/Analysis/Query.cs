using System.Linq;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Rasm.Analysis;

// --- [TYPES] ---------------------------------------------------------------------------

public sealed record Query<TGeometry, TOut> where TGeometry : notnull {
    internal Op Key { get; }
    internal Func<TGeometry, Eff<Context, Seq<TOut>>> Effect { get; }
    internal bool RequiresContext { get; }
    internal Option<Error> PreflightFault { get; }
    internal Query(
        Op key,
        Func<TGeometry, Eff<Context, Seq<TOut>>> effect,
        bool requiresContext = false,
        Option<Error> preflightFault = default) {
        Key = key;
        Effect = effect;
        RequiresContext = requiresContext;
        PreflightFault = preflightFault;
    }
    public Eff<Context, Seq<TOut>> Apply(TGeometry geometry) =>
        Effect(arg: geometry);
    internal static Query<TGeometry, TOut> Build(
        Op key,
        Func<TGeometry, Eff<Context, Seq<TOut>>> evaluator,
        Requirement? requirement = null,
        bool requiresContext = false) =>
        new(
            key: key,
            requiresContext: requiresContext,
            effect: (requirement, requiresContext) switch {
                (null or Requirement.NoneRequirement, _) => evaluator,
                (Requirement r, _) => geometry => geometry switch {
                    GeometryBase native =>
                        from ctx in Analyze.Asks
                        from _ in ctx.Validate(geometry: native, requirement: r).ToEff()
                        from result in evaluator(arg: geometry)
                        select result,
                    _ => evaluator(arg: geometry),
                },
            });
    internal static Query<TGeometry, TOut> Build<TState>(
        Op key,
        TState state,
        Func<TState, TGeometry, Eff<Context, Seq<TOut>>> evaluator,
        Requirement? requirement = null,
        bool requiresContext = false) =>
        Build(
            key: key,
            evaluator: geometry => evaluator(arg1: state, arg2: geometry),
            requirement: requirement,
            requiresContext: requiresContext);
    internal static Query<TGeometry, TOut> Reject(Op key, Error fault) =>
        new(
            key: key,
            preflightFault: Some(fault),
            effect: _ => Fin.Fail<Seq<TOut>>(fault).ToEff());
}
public enum MassKind { None = 0, Length = 1, Area = 2, Volume = 3 }
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
public enum MeshCheckCount { None = 0, DegenerateFaces = 1, DisjointMeshes = 2, DuplicateFaces = 3, ExtremelyShortEdges = 4, InvalidNgons = 5, NakedEdges = 6, NonManifoldEdges = 7, NonUnitVectorNormals = 8, RandomFaceNormals = 9, SelfIntersectingPairs = 10, UnusedVertices = 11, VertexFaceNormalsDiffer = 12, ZeroLengthNormals = 13 }
public enum ConformanceResidual { None = 0, Distance = 1, Rms = 2, WithinTolerance = 3, Profile = 4, Maximum = 5 }
public enum MeshFaceMetric { None = 0, AspectRatio = 1 }
public enum GeometryKind {
    Unknown = 0, Curve = 1, Polyline = 2, Mesh = 3, SubD = 4, Surface = 5,
    BrepGeneral = 10, BrepBox = 11, BrepSphere = 12, BrepCylinder = 13, BrepCone = 14, BrepTorus = 15, BrepPlane = 16,
    Line = 20, Sphere = 21, Box = 22, BoundingBox = 23, Cylinder = 24, Cone = 25, Torus = 26, Plane = 27,
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct CurvatureProfile(
    CurvatureScalar Scalar,
    int Count,
    double Minimum,
    double Maximum,
    double Mean,
    double Variance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct ResidualProfile(
    int Count,
    double Minimum,
    double Maximum,
    double Mean,
    double Variance,
    double Rms,
    double Tolerance,
    bool WithinTolerance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct ResidualSample(
    int Index,
    Point3d Location,
    double Distance,
    double Tolerance,
    bool WithinTolerance);
[StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceSample(int Face, double Value);
[StructLayout(LayoutKind.Auto)]
public readonly record struct Hit(int Id);
[StructLayout(LayoutKind.Auto)]
public readonly record struct Couple(int A, int B);
[StructLayout(LayoutKind.Auto)]
public readonly record struct CurveDeviation(
    double MinimumDistance,
    Point3d MinimumA,
    Point3d MinimumB,
    double MaximumDistance,
    Point3d MaximumA,
    Point3d MaximumB,
    double Tolerance,
    bool WithinTolerance);
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2 }
public enum Topology { Boundary, EdgeMidpoints, Adjacency, NonManifold }
[Union]
public partial record Bounds {
    public sealed record Box : Bounds;
    public sealed record Oriented(Plane Plane) : Bounds;
    public sealed record Transformed(Transform Transform) : Bounds;
    public sealed record Center : Bounds;
    public sealed record Corners : Bounds;
    public sealed record Edges : Bounds;
    public sealed record Area : Bounds;
    public sealed record Volume : Bounds;
}
[Union]
public partial record Measure {
    public sealed record Length : Measure;
    public sealed record Area : Measure;
    public sealed record Volume : Measure;
    public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure;
    public sealed record MassError(MassKind Mass) : Measure;
    public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure;
    public sealed record PrincipalAxes(MassKind Mass) : Measure;
}
[Union]
public partial record Location {
    public sealed record Midpoint : Location;
    public sealed record Tangent : Location;
    public sealed record Closest(Point3d Point) : Location;
    public sealed record PointAtCurve(double Parameter) : Location;
    public sealed record PointAtSurface(Point2d Uv) : Location;
    public sealed record PointAtLength(double Length) : Location;
    public sealed record FrameAtCurve(double Parameter) : Location;
    public sealed record FrameAtSurface(Point2d Uv) : Location;
    public sealed record PerpendicularFrameAt(double Parameter) : Location;
    public sealed record NormalAt(Point2d Uv) : Location;
    public sealed record CurvatureAtCurve(double Parameter) : Location;
    public sealed record CurvatureAtSurface(Point2d Uv) : Location;
    public sealed record CurvatureProfile(int Count, CurvatureScalar Scalar) : Location;
    public sealed record DerivativeAt(double Parameter, int Count) : Location;
    public sealed record DivideByCount(int Count) : Location;
    public sealed record DivideByLength(double Length) : Location;
    public sealed record Orientation(Plane Plane) : Location;
    public sealed record Contains(Point3d Point, Plane Plane) : Location;
    public sealed record ShortPath(Point2d Start, Point2d End) : Location;
}
[SmartEnum<int>]
public sealed partial class FaceSelector {
    public static readonly FaceSelector All = new(key: 0);
    public static readonly FaceSelector Top = new(key: 1);
    public static readonly FaceSelector Bottom = new(key: 2);
    public static readonly FaceSelector At = new(key: 3);
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Faces(FaceSelector Selector, Option<int> Index) {
    public static Faces All => new(Selector: FaceSelector.All, Index: None);
    public static Faces Top => new(Selector: FaceSelector.Top, Index: None);
    public static Faces Bottom => new(Selector: FaceSelector.Bottom, Index: None);
    public static Faces At(int? index = null) => new(Selector: FaceSelector.At, Index: Optional(value: index));
}
[SmartEnum<int>]
public sealed partial class CurveSelector {
    public static readonly CurveSelector All = new(key: 0);
    public static readonly CurveSelector Boundary = new(key: 1);
    public static readonly CurveSelector IsoU = new(key: 2);
    public static readonly CurveSelector IsoV = new(key: 3);
    public static readonly CurveSelector At = new(key: 4);
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Curves(CurveSelector Selector, Option<int> Index) {
    public static Curves All => new(Selector: CurveSelector.All, Index: None);
    public static Curves Boundary => new(Selector: CurveSelector.Boundary, Index: None);
    public static Curves IsoU => new(Selector: CurveSelector.IsoU, Index: None);
    public static Curves IsoV => new(Selector: CurveSelector.IsoV, Index: None);
    public static Curves At(int? index = null) => new(Selector: CurveSelector.At, Index: Optional(value: index));
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct Conformance {
    private Conformance(ConformanceResidual residual, int count) {
        Residual = residual;
        Count = count;
    }
    internal readonly ConformanceResidual Residual; internal readonly int Count;
    public static Conformance Distance(int count) => new(residual: ConformanceResidual.Distance, count: count); public static Conformance Rms(int count) => new(residual: ConformanceResidual.Rms, count: count); public static Conformance WithinTolerance(int count) => new(residual: ConformanceResidual.WithinTolerance, count: count); public static Conformance Profile(int count) => new(residual: ConformanceResidual.Profile, count: count); public static Conformance Maximum(int count) => new(residual: ConformanceResidual.Maximum, count: count);
}
public static partial class Query {
    internal delegate bool PrimitiveCase<TSource, TValue>(
        TSource geometry,
        Context context,
        out TValue value) where TSource : GeometryBase;
    internal delegate Fin<Seq<TValue>> ClosestCase<TSource, TValue>(
        Point3d target,
        TSource geometry) where TSource : notnull;
    internal static readonly Op
        MidpointKey = new(name: "Midpoint"), BoundsKey = new(name: nameof(Bounds)), OrientedBoundsKey = new(name: "OrientedBounds"),
        TransformedBoundsKey = new(name: "TransformedBounds"), BoundsCenterKey = new(name: "BoundsCenter"), BoundsCornersKey = new(name: "BoundsCorners"),
        BoxEdgesKey = new(name: "BoxEdges"), BoxAreaKey = new(name: "BoxArea"), BoxVolumeKey = new(name: "BoxVolume"), MeasureKey = new(name: nameof(Measure)),
        LengthKey = new(name: "Length"), TangentKey = new(name: "Tangent"), ClosestKey = new(name: "Closest"),
        DomainKey = new(name: nameof(Domain)), PointAtKey = new(name: "PointAt"), PointAtLengthKey = new(name: "PointAtLength"),
        FrameAtKey = new(name: "FrameAt"), PerpendicularFrameAtKey = new(name: "PerpendicularFrameAt"), NormalAtKey = new(name: "NormalAt"),
        CurvatureAtKey = new(name: "CurvatureAt"), DerivativeAtKey = new(name: "DerivativeAt"), DivideByCountKey = new(name: "DivideByCount"),
        DivideByLengthKey = new(name: "DivideByLength"), OrientationKey = new(name: "Orientation"), ContainsKey = new(name: "Contains"),
        SegmentsKey = new(name: nameof(Segments)), EdgesKey = new(name: nameof(Edges)), NakedEdgesKey = new(name: nameof(NakedEdges)),
        EdgeMidpointsKey = new(name: "EdgeMidpoints"), SpatialMidpointKey = new(name: "SpatialMidpoint"),
        OutlinesKey = new(name: nameof(Outlines)), IsoKey = new(name: nameof(Iso)), PrimitiveKey = new(name: "Primitive"),
        ShortPathKey = new(name: "ShortPath"), SolidOrientationKey = new(name: nameof(SolidOrientation)), IsPointInsideKey = new(name: nameof(IsPointInside)),
        VerticesKey = new(name: nameof(Vertices)), ComponentsKey = new(name: "Components"), IsManifoldKey = new(name: nameof(IsManifold)),
        NakedPointStatusKey = new(name: nameof(NakedPointStatus)), MeshCheckKey = new(name: nameof(MeshCheck)), MeshCheckCountKey = new(name: "MeshCheckCount"), MeshFaceMetricKey = new(name: nameof(MeshFaceMetric)), SelfIntersectionsKey = new(name: nameof(SelfIntersections)), IntersectKey = new(name: nameof(Intersect)),
        ConformanceKey = new(name: nameof(Conformance)), DeviationKey = new(name: nameof(Deviation)), TreeKey = new(name: nameof(Tree)),
        TopologyKey = new(name: nameof(Topology)), ScopeKey = new(name: nameof(Analyze.Scope)),
        KindKey = new(name: nameof(Kind)),
        UniqueCornersKey = new(name: "UniqueCorners"),
        WorldCardinalPointsKey = new(name: "WorldCardinalPoints"),
        FacesKey = new(name: nameof(Faces)),
        CurvesKey = new(name: nameof(Curves));
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this Op key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(
            key: key,
            fault: key.Unsupported(
                geometryType: typeof(TGeometry),
                outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Aspect<TGeometry, TOut, TAspect>(
        TAspect aspect,
        Op key,
        Func<TAspect, Query<TGeometry, TOut>?> dispatch) where TGeometry : notnull where TAspect : notnull =>
        Optional(dispatch(arg: aspect))
            .IfNone(() => key.Unsupported<TGeometry, TOut>());
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(
        Op key,
        object query) where TGeometry : notnull =>
        query switch {
            Query<TGeometry, TOut> typed => typed,
            _ => Query<TGeometry, TOut>.Reject(
                key: key,
                fault: key.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TOut))),
        };
    internal static Fin<Seq<TValue>> One<TValue>(this Op key, TValue value) =>
        new OpResult<TValue>.One(Value: value).Reduce(key: key);
    internal static Fin<Seq<TValue>> Many<TValue>(this Op key, IEnumerable<TValue>? values) =>
        new OpResult<TValue>.Many(Values: Optional(values).ToSeq().Bind(static v => v.AsIterable().ToSeq())).Reduce(key: key);
    internal static Fin<Seq<TOut>> IntersectionOutput<TOut>(
        this Op key,
        IEnumerable<Curve>? curves = null,
        IEnumerable<Point3d>? points = null,
        IEnumerable<Polyline>? polylines = null,
        CurveIntersections? intersections = null) =>
        typeof(TOut) switch {
            Type output when output == typeof(Curve) =>
                key.CastResults<Curve, TOut>(values: curves),
            Type output when output == typeof(Point3d) =>
                key.CastResults<Point3d, TOut>(values: points ?? Optional(intersections)
                    .ToSeq()
                    .Bind(static events => events)
                    .Where(static intersection => intersection.IsPoint)
                    .Select(static intersection => intersection.PointA)),
            Type output when output == typeof(IntersectionEvent) =>
                key.CastResults<IntersectionEvent, TOut>(values: Optional(intersections)
                    .ToSeq()
                    .Bind(static events => events)),
            Type output when output == typeof(Polyline) =>
                key.CastResults<Polyline, TOut>(values: polylines),
            Type output when output == typeof(IntersectionKind) =>
                key.CastResults<IntersectionKind, TOut>(values: Optional(intersections)
                    .ToSeq()
                    .Bind(static events => events)
                    .Select(static intersection => intersection switch {
                        IntersectionEvent candidate when candidate.IsOverlap => IntersectionKind.Overlap,
                        IntersectionEvent candidate when candidate.IsPoint => IntersectionKind.Point,
                        _ => IntersectionKind.Unknown,
                    })
                    .Concat(second: Optional(curves)
                        .ToSeq()
                        .Bind(static values => values)
                        .Select(static _ => IntersectionKind.Overlap))
                    .Concat(second: Optional(points)
                        .ToSeq()
                        .Bind(static values => values)
                        .Select(static _ => IntersectionKind.Point))
                    .Concat(second: Optional(polylines)
                        .ToSeq()
                        .Bind(static values => values)
                        .Select(static _ => IntersectionKind.Overlap))),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(
                geometryType: typeof(void),
                outputType: typeof(TOut))),
        };
    private static Fin<Seq<TOut>> CastResults<TValue, TOut>(
        this Op key,
        IEnumerable<TValue>? values) =>
        Many(key: key, values: values)
            .Bind(candidates => key.Retype<TValue, TOut>(values: candidates));
    internal static Fin<Seq<TOut>> Retype<TValue, TOut>(this Op key, Seq<TValue> values) =>
        typeof(TValue).Equals(typeof(TOut)) switch {
            true => Fin.Succ(values.Map(static candidate => (TOut)(object)candidate!)),
            false => Fin.Fail<Seq<TOut>>(key.Unsupported(
                geometryType: typeof(void),
                outputType: typeof(TOut))),
        };
    internal static Query<TGeometry, TOut> PrimitiveMatch<TGeometry, TOut, TSource, TValue>(
        PrimitiveCase<TSource, TValue> project) where TGeometry : notnull where TSource : GeometryBase =>
        Cast<TGeometry, TOut>(key: PrimitiveKey, query: Query<TGeometry, TValue>.Build(
            key: PrimitiveKey,
            requiresContext: true,
            state: project,
            evaluator: static (extract, geometry) =>
                geometry switch {
                    TSource source =>
                        from ctx in Analyze.Asks
                        from validated in ctx.Validate(geometry: source, requirement: Requirement.Basic).ToEff()
                        from result in PrimitiveExtract(
                                key: PrimitiveKey,
                                extract: extract,
                                geometry: validated,
                                context: ctx)
                            .ToEff()
                        select result,
                    _ => Fin.Fail<Seq<TValue>>(PrimitiveKey.Unsupported(
                        geometryType: typeof(TGeometry),
                        outputType: typeof(TValue))).ToEff(),
                }));
    private static Fin<Seq<TValue>> PrimitiveExtract<TSource, TValue>(
        Op key,
        PrimitiveCase<TSource, TValue> extract,
        TSource geometry,
        Context context) where TSource : GeometryBase =>
        extract(
            geometry: geometry,
            context: context,
            value: out TValue value) switch {
                bool solved => OpResult<TValue>.Solved(isSolved: solved, value: value).Reduce(key: key),
            };
    internal static Query<TGeometry, TOut> ClosestMatch<TGeometry, TOut, TSource, TValue>(
        Point3d point,
        ClosestCase<TSource, TValue> project) where TGeometry : notnull where TSource : notnull =>
        Cast<TGeometry, TOut>(key: ClosestKey, query: Query<TGeometry, TValue>.Build(
            key: ClosestKey,
            state: (Point: point, Project: project),
            evaluator: static (state, geometry) =>
                geometry switch {
                    TSource source => state.Project(
                            target: state.Point,
                            geometry: source)
                        .ToEff(),
                    _ => Fin.Fail<Seq<TValue>>(ClosestKey.Unsupported(
                        geometryType: typeof(TGeometry),
                        outputType: typeof(TValue))).ToEff(),
                }));
    internal static Fin<TOut> CurveAtNormalizedValue<TOut>(
        Curve curve,
        Context context,
        Op key,
        Func<Curve, double, TOut> project) =>
        curve.NormalizedLengthParameter(
            s: 0.5,
            t: out double parameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => Fin.Succ(project(arg1: curve, arg2: parameter)),
                false => Fin.Fail<TOut>(key.InvalidResult()),
            };
    internal static Eff<Context, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(
        TGeometry geometry,
        Op key,
        Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve =>
                from ctx in Analyze.Asks
                from validated in ctx.Validate(geometry: curve, requirement: Requirement.CurveLength).ToEff()
                from value in CurveAtNormalizedValue(
                        curve: validated,
                        context: ctx,
                        key: key,
                        project: project)
                    .ToEff()
                from result in One(key: key, value: value).ToEff()
                select result,
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))).ToEff(),
        };
    internal static readonly Func<object, bool, bool, Eff<Context, LengthMassProperties>> ComputeLength =
        static (geometry, second, product) =>
            (geometry switch {
                Curve curve => Optional(LengthMassProperties.Compute(
                        curve: curve,
                        length: true,
                        firstMoments: true,
                        secondMoments: second,
                        productMoments: product))
                    .ToFin(OpFault.ComputationFailed(label: nameof(LengthMassProperties))),
                _ => Fin.Fail<LengthMassProperties>(OpFault.ComputationUnsupported(
                    label: nameof(LengthMassProperties),
                    geometryType: geometry.GetType())),
            }).ToEff();
    internal static readonly Func<object, bool, bool, Eff<Context, AreaMassProperties>> ComputeArea =
        static (geometry, second, product) =>
            from ctx in Analyze.Asks
            from props in Optional(geometry switch {
                Curve curve => AreaMassProperties.Compute(
                    closedPlanarCurve: curve,
                    planarTolerance: ctx.Absolute.Value),
                Mesh mesh => AreaMassProperties.Compute(
                    mesh: mesh,
                    area: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product),
                Brep brep => AreaMassProperties.Compute(
                    brep: brep,
                    area: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product,
                    relativeTolerance: ctx.Relative.Value,
                    absoluteTolerance: ctx.Absolute.Value),
                Surface surface => AreaMassProperties.Compute(
                    surface: surface,
                    area: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product),
                _ => null,
            }).ToFin(geometry switch {
                Curve or Mesh or Brep or Surface => OpFault.ComputationFailed(label: nameof(AreaMassProperties)),
                _ => OpFault.ComputationUnsupported(label: nameof(AreaMassProperties), geometryType: geometry.GetType()),
            }).ToEff()
            select props;
    internal static readonly Func<object, bool, bool, Eff<Context, VolumeMassProperties>> ComputeVolume =
        static (geometry, second, product) =>
            from ctx in Analyze.Asks
            from props in Optional(geometry switch {
                Mesh mesh => VolumeMassProperties.Compute(
                    mesh: mesh,
                    volume: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product),
                Brep brep => VolumeMassProperties.Compute(
                    brep: brep,
                    volume: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product,
                    relativeTolerance: ctx.Relative.Value,
                    absoluteTolerance: ctx.Absolute.Value),
                Surface surface => VolumeMassProperties.Compute(
                    surface: surface,
                    volume: true,
                    firstMoments: true,
                    secondMoments: second,
                    productMoments: product),
                _ => null,
            }).ToFin(geometry switch {
                Mesh or Brep or Surface => OpFault.ComputationFailed(label: nameof(VolumeMassProperties)),
                _ => OpFault.ComputationUnsupported(label: nameof(VolumeMassProperties), geometryType: geometry.GetType()),
            }).ToEff()
            select props;
    internal static Query<TGeometry, TOut> Mass<TGeometry, TMass, TOut>(
        string name,
        Requirement requirement,
        Func<object, bool, bool, Eff<Context, TMass>> compute,
        Func<Op, TMass, Fin<Seq<TOut>>> project,
        bool secondMoments = false,
        bool productMoments = false) where TGeometry : notnull where TMass : class, IDisposable {
        Op key = new(name: name);
        return Query<TGeometry, TOut>.Build(
            key: key,
            requirement: requirement,
            requiresContext: true,
            evaluator: geometry => from mass in compute(
                    arg1: geometry,
                    arg2: secondMoments,
                    arg3: productMoments)
                                   from values in DisposeAndProject(
                                           key: key,
                                           mass: mass,
                                           project: project)
                                       .ToEff()
                                   select values);
    }
    private static Fin<Seq<TOut>> DisposeAndProject<TMass, TOut>(
        Op key,
        TMass mass,
        Func<Op, TMass, Fin<Seq<TOut>>> project) where TMass : class, IDisposable {
        using TMass disposable = mass;
        return project(arg1: key, arg2: disposable);
    }
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal<TMass>(
        this Op key,
        TMass mass) where TMass : class =>
        mass switch {
            LengthMassProperties length => key.PrincipalFromMoments(
                solved: length.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            AreaMassProperties area => key.PrincipalFromMoments(
                solved: area.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            VolumeMassProperties volume => key.PrincipalFromMoments(
                solved: volume.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            _ => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> PrincipalFromMoments(
        this Op key,
        bool solved,
        double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
}
