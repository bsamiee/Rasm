using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Core;
using Core.Domain;
using Core.Runtime;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [ALGEBRA] ---------------------------------------------------------------------------------

public sealed class Query<TGeometry, TOut> where TGeometry : notnull {
    private Query(
        OperationKey key,
        GeometryRequirement requirement,
        bool requiresContext,
        Fin<Unit> ready,
        Func<TGeometry, Eff<AnalysisRuntime, Seq<TOut>>> evaluator) {
        Key = key;
        Requirement = requirement;
        RequiresContext = requiresContext || requirement != GeometryRequirement.None;
        Ready = ready;
        Evaluator = evaluator;
    }
    internal OperationKey Key { get; }
    internal GeometryRequirement Requirement { get; }
    internal bool RequiresContext { get; }
    internal Fin<Unit> Ready { get; }
    private Func<TGeometry, Eff<AnalysisRuntime, Seq<TOut>>> Evaluator { get; }
    public Eff<AnalysisRuntime, Seq<TOut>> Apply(TGeometry geometry) =>
        Evaluator(arg: geometry);
    internal static Query<TGeometry, TOut> Build(
        OperationKey key,
        Func<TGeometry, Eff<AnalysisRuntime, Seq<TOut>>> evaluator,
        GeometryRequirement requirement = default,
        bool requiresContext = false) =>
        new(
            key: key,
            requirement: requirement,
            requiresContext: requiresContext,
            ready: Fin.Succ(unit),
            evaluator: evaluator);
    internal static Query<TGeometry, TOut> Build<TState>(
        OperationKey key,
        TState state,
        Func<TState, TGeometry, Eff<AnalysisRuntime, Seq<TOut>>> evaluator,
        GeometryRequirement requirement = default,
        bool requiresContext = false) =>
        new(
            key: key,
            requirement: requirement,
            requiresContext: requiresContext,
            ready: Fin.Succ(unit),
            evaluator: (TGeometry geometry) =>
                evaluator(arg1: state, arg2: geometry));
    internal static Query<TGeometry, TOut> Reject(OperationKey key, Error fault) =>
        new(
            key: key,
            requirement: GeometryRequirement.None,
            requiresContext: false,
            ready: Fin.Fail<Unit>(error: fault),
            evaluator: (TGeometry _) =>
                Eff<AnalysisRuntime, Seq<TOut>>.Fail(error: fault));
}
public enum MassKind { None = 0, Length = 1, Area = 2, Volume = 3 }
public enum CurvatureScalar { None = 0, Magnitude = 1, Gaussian = 2, Mean = 3 }
public enum MeshCheckCount { None = 0, DegenerateFaces = 1, DisjointMeshes = 2, DuplicateFaces = 3, ExtremelyShortEdges = 4, InvalidNgons = 5, NakedEdges = 6, NonManifoldEdges = 7, NonUnitVectorNormals = 8, RandomFaceNormals = 9, SelfIntersectingPairs = 10, UnusedVertices = 11, VertexFaceNormalsDiffer = 12, ZeroLengthNormals = 13 }
public enum ConformanceResidual { None = 0, Distance = 1, Rms = 2, WithinTolerance = 3, Profile = 4, Maximum = 5 }
public enum MeshFaceMetric { None = 0, AspectRatio = 1 }
public enum DeviationKind { None = 0, Curve = 1 }
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
public readonly record struct SpatialHit(int Id);
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpatialPair(int A, int B);
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
internal enum TopologyKind { Boundary, EdgeMidpoints, Adjacency, NonManifold }
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
[SuppressMessage(category: "Naming", checkId: "CA1716:Identifiers should not match keywords", Justification = "Measure.Error is a domain-scoped union variant unambiguous within Measure namespace.")]
[SuppressMessage(category: "Naming", checkId: "CA1724:Type names should not match namespaces", Justification = "Measure.Principal is a domain-scoped union variant unambiguous within Measure namespace.")]
public partial record Measure {
    public sealed record Length : Measure;
    public sealed record Area : Measure;
    public sealed record Volume : Measure;
    public sealed record SpatialMidpoint : Measure;
    public sealed record Centroid(MassKind Mass) : Measure;
    public sealed record Error(MassKind Mass) : Measure;
    public sealed record CentroidError(MassKind Mass) : Measure;
    public sealed record Radii(MassKind Mass) : Measure;
    public sealed record Principal(MassKind Mass) : Measure;
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
[StructLayout(LayoutKind.Auto)]
public readonly record struct Topology {
    private Topology(TopologyKind kind) =>
        Kind = kind;
    internal readonly TopologyKind Kind;
    public static Topology Boundary => new(kind: TopologyKind.Boundary); public static Topology EdgeMidpoints => new(kind: TopologyKind.EdgeMidpoints); public static Topology Adjacency => new(kind: TopologyKind.Adjacency); public static Topology NonManifold => new(kind: TopologyKind.NonManifold);
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
[StructLayout(LayoutKind.Auto)]
public readonly record struct Deviation {
    private Deviation(DeviationKind kind) =>
        Kind = kind;
    internal readonly DeviationKind Kind;
    public static Deviation Curve => new(kind: DeviationKind.Curve);
}
public static partial class Query {
    internal delegate bool PrimitiveCase<TSource, TValue>(
        TSource geometry,
        GeometryContext context,
        out TValue value) where TSource : GeometryBase;
    internal delegate Fin<Seq<TValue>> ClosestCase<TSource, TValue>(
        Point3d target,
        TSource geometry) where TSource : notnull;
    internal static readonly OperationKey
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
        ConformanceKey = new(name: nameof(Conformance)), DeviationKey = new(name: nameof(Deviation)), SpatialIndexKey = new(name: nameof(SpatialIndex)),
        TopologyKey = new(name: nameof(Topology)), ScopeKey = new(name: nameof(Analyze.Scope)),
        KindKey = new(name: nameof(Kind)),
        UniqueCornersKey = new(name: "UniqueCorners"),
        WorldCardinalPointsKey = new(name: "WorldCardinalPoints");
    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this OperationKey key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(
            key: key,
            fault: key.Unsupported(
                geometryType: typeof(TGeometry),
                outputType: typeof(TOut)));
    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(
        OperationKey key,
        object query) where TGeometry : notnull =>
        query.GetType().Equals(typeof(Query<TGeometry, TOut>)) switch {
            true => (Query<TGeometry, TOut>)query,
            false => Query<TGeometry, TOut>.Reject(
                key: key,
                fault: key.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TOut))),
        };
    internal static Fin<Seq<TValue>> One<TValue>(this OperationKey key, TValue value) =>
        GeometryResult.One(key: key, value: value);
    internal static Fin<Seq<TValue>> Many<TValue>(this OperationKey key, IEnumerable<TValue>? values) =>
        GeometryResult.Many(key: key, values: values);
    internal static Fin<Seq<TOut>> IntersectionOutput<TOut>(
        this OperationKey key,
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
                    .Bind(static (CurveIntersections events) => events)
                    .Where(static (IntersectionEvent intersection) => intersection.IsPoint)
                    .Select(static (IntersectionEvent intersection) => intersection.PointA)),
            Type output when output == typeof(IntersectionEvent) =>
                key.CastResults<IntersectionEvent, TOut>(values: Optional(intersections)
                    .ToSeq()
                    .Bind(static (CurveIntersections events) => events)),
            Type output when output == typeof(Polyline) =>
                key.CastResults<Polyline, TOut>(values: polylines),
            Type output when output == typeof(IntersectionKind) =>
                key.CastResults<IntersectionKind, TOut>(values: IntersectionKinds(
                    curves: curves,
                    points: points,
                    polylines: polylines,
                    intersections: intersections)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(
                geometryType: typeof(void),
                outputType: typeof(TOut))),
        };
    private static IEnumerable<IntersectionKind> IntersectionKinds(
        IEnumerable<Curve>? curves,
        IEnumerable<Point3d>? points,
        IEnumerable<Polyline>? polylines,
        CurveIntersections? intersections) =>
        Optional(intersections)
            .ToSeq()
            .Bind(static (CurveIntersections events) => events)
            .Select(static (IntersectionEvent intersection) => intersection switch {
                IntersectionEvent candidate when candidate.IsOverlap => IntersectionKind.Overlap,
                IntersectionEvent candidate when candidate.IsPoint => IntersectionKind.Point,
                _ => IntersectionKind.Unknown,
            })
            .Concat(second: Optional(curves)
                .ToSeq()
                .Bind(static (IEnumerable<Curve> values) => values)
                .Select(static (Curve _) => IntersectionKind.Overlap))
            .Concat(second: Optional(points)
                .ToSeq()
                .Bind(static (IEnumerable<Point3d> values) => values)
                .Select(static (Point3d _) => IntersectionKind.Point))
            .Concat(second: Optional(polylines)
                .ToSeq()
                .Bind(static (IEnumerable<Polyline> values) => values)
                .Select(static (Polyline _) => IntersectionKind.Overlap));
    private static Fin<Seq<TOut>> CastResults<TValue, TOut>(
        this OperationKey key,
        IEnumerable<TValue>? values) =>
        Many(key: key, values: values)
            .Bind((Seq<TValue> candidates) => key.Retype<TValue, TOut>(values: candidates));
    internal static Fin<Seq<TOut>> Retype<TValue, TOut>(this OperationKey key, Seq<TValue> values) =>
        typeof(TValue).Equals(typeof(TOut)) switch {
            true => Fin.Succ(values.Map(static (TValue candidate) => (TOut)(object)candidate!)),
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
            evaluator: static (PrimitiveCase<TSource, TValue> extract, TGeometry geometry) =>
                geometry switch {
                    TSource source =>
                        from rt in Analyze.Asks
                        from validated in rt.Context.Validate(geometry: source, requirement: GeometryRequirement.Basic).ToEff()
                        from result in PrimitiveExtract(
                                key: PrimitiveKey,
                                extract: extract,
                                geometry: validated,
                                context: rt.Context)
                            .ToEff()
                        select result,
                    _ => Eff<AnalysisRuntime, Seq<TValue>>.Fail(error: PrimitiveKey.Unsupported(
                        geometryType: typeof(TGeometry),
                        outputType: typeof(TValue))),
                }));
    private static Fin<Seq<TValue>> PrimitiveExtract<TSource, TValue>(
        OperationKey key,
        PrimitiveCase<TSource, TValue> extract,
        TSource geometry,
        GeometryContext context) where TSource : GeometryBase =>
        extract(
            geometry: geometry,
            context: context,
            value: out TValue value) switch {
                bool solved => key.Solved(
                    solved: solved,
                    value: value),
            };
    internal static Query<TGeometry, TOut> ClosestMatch<TGeometry, TOut, TSource, TValue>(
        Point3d point,
        ClosestCase<TSource, TValue> project) where TGeometry : notnull where TSource : notnull =>
        Cast<TGeometry, TOut>(key: ClosestKey, query: Query<TGeometry, TValue>.Build(
            key: ClosestKey,
            state: (Point: point, Project: project),
            evaluator: static ((Point3d Point, ClosestCase<TSource, TValue> Project) state, TGeometry geometry) =>
                geometry switch {
                    TSource source => state.Project(
                            target: state.Point,
                            geometry: source)
                        .ToEff(),
                    _ => Eff<AnalysisRuntime, Seq<TValue>>.Fail(error: ClosestKey.Unsupported(
                        geometryType: typeof(TGeometry),
                        outputType: typeof(TValue))),
                }));
    internal static Fin<TOut> CurveAtNormalizedValue<TOut>(
        Curve curve,
        GeometryContext context,
        OperationKey key,
        Func<Curve, double, TOut> project) =>
        curve.NormalizedLengthParameter(
            s: 0.5,
            t: out double parameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => Fin.Succ(project(arg1: curve, arg2: parameter)),
                false => Fin.Fail<TOut>(key.InvalidResult()),
            };
    internal static Eff<AnalysisRuntime, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(
        TGeometry geometry,
        OperationKey key,
        Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve =>
                from rt in Analyze.Asks
                from validated in rt.Context.Validate(geometry: curve, requirement: GeometryRequirement.CurveLength).ToEff()
                from value in CurveAtNormalizedValue(
                        curve: validated,
                        context: rt.Context,
                        key: key,
                        project: project)
                    .ToEff()
                from result in One(key: key, value: value).ToEff()
                select result,
            _ => Eff<AnalysisRuntime, Seq<TOut>>.Fail(error: key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(TOut))),
        };
    internal static Query<TGeometry, TOut> LengthMass<TGeometry, TOut>(
        string name,
        Func<OperationKey, LengthMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = true,
        bool productMoments = false) where TGeometry : notnull =>
        Mass<TGeometry, LengthMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.CurveLength,
            compute: static (TGeometry geometry, bool second, bool product) =>
                (geometry switch {
                    Curve curve => Optional(LengthMassProperties.Compute(
                            curve: curve,
                            length: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product))
                        .ToFin(MassFault.Failed(label: nameof(LengthMassProperties))),
                    _ => Fin.Fail<LengthMassProperties>(MassFault.Unsupported(
                        label: nameof(LengthMassProperties),
                        geometryType: geometry.GetType())),
                }).ToEff(),
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);
    internal static Query<TGeometry, TOut> AreaMass<TGeometry, TOut>(
        string name,
        Func<OperationKey, AreaMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = false,
        bool productMoments = false) where TGeometry : notnull =>
        Mass<TGeometry, AreaMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.AreaMass,
            compute: static (TGeometry geometry, bool second, bool product) =>
                from rt in Analyze.Asks
                from props in (geometry switch {
                    Curve curve => AreaMassProperties.Compute(
                            closedPlanarCurve: curve,
                            planarTolerance: rt.Context.Absolute.Value)
                        .Mass(label: nameof(AreaMassProperties)),
                    Mesh mesh => AreaMassProperties.Compute(
                            mesh: mesh,
                            area: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product)
                        .Mass(label: nameof(AreaMassProperties)),
                    Brep brep => AreaMassProperties.Compute(
                            brep: brep,
                            area: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product,
                            relativeTolerance: rt.Context.Relative.Value,
                            absoluteTolerance: rt.Context.Absolute.Value)
                        .Mass(label: nameof(AreaMassProperties)),
                    Surface surface => AreaMassProperties.Compute(
                            surface: surface,
                            area: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product)
                        .Mass(label: nameof(AreaMassProperties)),
                    _ => Fin.Fail<AreaMassProperties>(MassFault.Unsupported(
                        label: nameof(AreaMassProperties),
                        geometryType: geometry.GetType())),
                }).ToEff()
                select props,
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);
    internal static Query<TGeometry, TOut> VolumeMass<TGeometry, TOut>(
        string name,
        Func<OperationKey, VolumeMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = false,
        bool productMoments = false) where TGeometry : notnull =>
        Mass<TGeometry, VolumeMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.VolumeMass,
            compute: static (TGeometry geometry, bool second, bool product) =>
                from rt in Analyze.Asks
                from props in (geometry switch {
                    Mesh mesh => VolumeMassProperties.Compute(
                            mesh: mesh,
                            volume: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product)
                        .Mass(label: nameof(VolumeMassProperties)),
                    Brep brep => VolumeMassProperties.Compute(
                            brep: brep,
                            volume: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product,
                            relativeTolerance: rt.Context.Relative.Value,
                            absoluteTolerance: rt.Context.Absolute.Value)
                        .Mass(label: nameof(VolumeMassProperties)),
                    Surface surface => VolumeMassProperties.Compute(
                            surface: surface,
                            volume: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product)
                        .Mass(label: nameof(VolumeMassProperties)),
                    _ => Fin.Fail<VolumeMassProperties>(MassFault.Unsupported(
                        label: nameof(VolumeMassProperties),
                        geometryType: geometry.GetType())),
                }).ToEff()
                select props,
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);
    private static Query<TGeometry, TOut> Mass<TGeometry, TMass, TOut>(
        string name,
        GeometryRequirement requirement,
        Func<TGeometry, bool, bool, Eff<AnalysisRuntime, TMass>> compute,
        Func<OperationKey, TMass, Fin<Seq<TOut>>> project,
        bool secondMoments,
        bool productMoments) where TGeometry : notnull where TMass : class, IDisposable {
        OperationKey key = new(name: name);
        return Query<TGeometry, TOut>.Build(
            key: key,
            requirement: requirement,
            evaluator: (TGeometry geometry) =>
                from mass in compute(
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
        OperationKey key,
        TMass mass,
        Func<OperationKey, TMass, Fin<Seq<TOut>>> project) where TMass : class, IDisposable {
        using TMass disposable = mass;
        return project(arg1: key, arg2: disposable);
    }
    private static Fin<TMass> Mass<TMass>(this TMass? mass, string label) where TMass : class, IDisposable =>
        Optional(mass)
            .ToFin(MassFault.Failed(label: label));
    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(
        this OperationKey key,
        bool solved,
        double x, Vector3d xAxis, double y, Vector3d yAxis, double z, Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq((Moment: x, Axis: xAxis), (Moment: y, Axis: yAxis), (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };
    private static class MassFault {
        internal static Error Failed(string label) =>
            Error.New(message: string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Rhino {label} computation failed."));
        internal static Error Unsupported(string label, Type geometryType) =>
            Error.New(message: string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Rhino {label} computation does not support geometry '{geometryType.Name}'."));
    }
}
