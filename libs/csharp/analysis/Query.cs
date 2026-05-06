using System.Globalization;
using System.Linq;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;

namespace Analysis;

// --- [ALGEBRA] ---------------------------------------------------------------------------------

public sealed class Query<TGeometry, TOut> where TGeometry : notnull {
    private Query(
        OperationKey key,
        GeometryRequirement requirement,
        bool requiresContext,
        Fin<Unit> ready,
        Func<TGeometry, Fin<GeometryContext>, Fin<Seq<TOut>>> evaluator) {
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
    private Func<TGeometry, Fin<GeometryContext>, Fin<Seq<TOut>>> Evaluator { get; }
    internal Fin<Seq<TOut>> Apply(TGeometry geometry, Fin<GeometryContext> context) =>
        Evaluator(arg1: geometry, arg2: context);

    internal static Query<TGeometry, TOut> Build(
        OperationKey key,
        Func<TGeometry, Fin<GeometryContext>, Fin<Seq<TOut>>> evaluator,
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
        Func<TState, TGeometry, Fin<GeometryContext>, Fin<Seq<TOut>>> evaluator,
        GeometryRequirement requirement = default,
        bool requiresContext = false) =>
        new(
            key: key,
            requirement: requirement,
            requiresContext: requiresContext,
            ready: Fin.Succ(unit),
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) =>
                evaluator(arg1: state, arg2: geometry, arg3: context));

    internal static Query<TGeometry, TOut> Reject(OperationKey key, Error fault) =>
        new(
            key: key,
            requirement: GeometryRequirement.None,
            requiresContext: false,
            ready: Fin.Fail<Unit>(error: fault),
            evaluator: (TGeometry _, Fin<GeometryContext> _) =>
                Fin.Fail<Seq<TOut>>(error: fault));
}

public static partial class Query {
    internal delegate bool ContextPrimitive<TSource, TValue>(
        TSource geometry,
        GeometryContext context,
        out TValue value) where TSource : GeometryBase;

    internal delegate bool PurePrimitive<TSource, TValue>(
        TSource geometry,
        out TValue value) where TSource : GeometryBase;

    internal delegate Fin<Seq<TValue>> ClosestCase<TSource, TValue>(
        Point3d target,
        TSource geometry) where TSource : notnull;

    internal static readonly OperationKey
        MidpointKey = new(name: nameof(Midpoint)), BoundsKey = new(name: nameof(Bounds)), OrientedBoundsKey = new(name: nameof(OrientedBounds)),
        TransformedBoundsKey = new(name: nameof(TransformedBounds)), BoundsCenterKey = new(name: nameof(BoundsCenter)), BoundsCornersKey = new(name: nameof(BoundsCorners)),
        BoxEdgesKey = new(name: nameof(BoxEdges)), BoxAreaKey = new(name: nameof(BoxArea)), BoxVolumeKey = new(name: nameof(BoxVolume)),
        LengthKey = new(name: nameof(Length)), TangentKey = new(name: nameof(Tangent)), ClosestKey = new(name: nameof(Closest)),
        DomainKey = new(name: nameof(Domain)), PointAtKey = new(name: nameof(PointAt)), PointAtLengthKey = new(name: nameof(PointAtLength)),
        FrameAtKey = new(name: nameof(FrameAt)), PerpendicularFrameAtKey = new(name: nameof(PerpendicularFrameAt)), NormalAtKey = new(name: nameof(NormalAt)),
        CurvatureAtKey = new(name: nameof(CurvatureAt)), DerivativeAtKey = new(name: nameof(DerivativeAt)), DivideByCountKey = new(name: nameof(DivideByCount)),
        DivideByLengthKey = new(name: nameof(DivideByLength)), OrientationKey = new(name: nameof(Orientation)), ContainsKey = new(name: nameof(Contains)),
        SegmentsKey = new(name: nameof(Segments)), EdgesKey = new(name: nameof(Edges)), NakedEdgesKey = new(name: nameof(NakedEdges)),
        OutlinesKey = new(name: nameof(Outlines)), IsoKey = new(name: nameof(Iso)), PrimitiveKey = new(name: nameof(Primitive)),
        ShortPathKey = new(name: nameof(ShortPath)), SolidOrientationKey = new(name: nameof(SolidOrientation)), IsPointInsideKey = new(name: nameof(IsPointInside)),
        VerticesKey = new(name: nameof(Vertices)), ComponentsKey = new(name: "Components"), IsManifoldKey = new(name: nameof(IsManifold)),
        NakedPointStatusKey = new(name: nameof(NakedPointStatus)), SelfIntersectionsKey = new(name: nameof(SelfIntersections)), IntersectKey = new(name: nameof(Intersect)),
        ScopeKey = new(name: nameof(Analyze.Scope));

    internal static Query<TGeometry, TOut> Unsupported<TGeometry, TOut>(this OperationKey key) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Reject(
            key: key,
            fault: key.Unsupported(
                geometryType: typeof(TGeometry),
                outputType: typeof(TOut)));

    internal static Query<TGeometry, TOut> Cast<TGeometry, TOut>(object query) where TGeometry : notnull =>
        (Query<TGeometry, TOut>)query;

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
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(
                geometryType: typeof(void),
                outputType: typeof(TOut))),
        };

    private static Fin<Seq<TOut>> CastResults<TValue, TOut>(
        this OperationKey key,
        IEnumerable<TValue>? values) =>
        Many(key: key, values: values)
            .Map(static (Seq<TValue> candidates) => candidates.Map(static (TValue candidate) => (TOut)(object)candidate!));

    internal static Query<TGeometry, TOut> PrimitiveContext<TGeometry, TOut, TSource, TValue>(
        ContextPrimitive<TSource, TValue> project) where TGeometry : notnull where TSource : GeometryBase =>
        Cast<TGeometry, TOut>(query: Query<TGeometry, TValue>.Build(
            key: PrimitiveKey,
            requiresContext: true,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) => geometry switch {
                TSource source => context.Bind((GeometryContext model) => project(
                    geometry: source,
                    context: model,
                    value: out TValue value) switch {
                        true => One(key: PrimitiveKey, value: value),
                        false => Fin.Fail<Seq<TValue>>(PrimitiveKey.InvalidResult()),
                    }),
                _ => Fin.Fail<Seq<TValue>>(PrimitiveKey.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TValue))),
            }));

    internal static Query<TGeometry, TOut> PrimitivePure<TGeometry, TOut, TSource, TValue>(
        PurePrimitive<TSource, TValue> project) where TGeometry : notnull where TSource : GeometryBase =>
        Cast<TGeometry, TOut>(query: Query<TGeometry, TValue>.Build(
            key: PrimitiveKey,
            evaluator: static (PurePrimitive<TSource, TValue> extract, TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                TSource source => extract(
                    geometry: source,
                    value: out TValue value) switch {
                        true => One(key: PrimitiveKey, value: value),
                        false => Fin.Fail<Seq<TValue>>(PrimitiveKey.InvalidResult()),
                    },
                _ => Fin.Fail<Seq<TValue>>(PrimitiveKey.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TValue))),
            },
            state: project));

    internal static Query<TGeometry, TOut> ClosestMatch<TGeometry, TOut, TSource, TValue>(
        Point3d point,
        ClosestCase<TSource, TValue> project) where TGeometry : notnull where TSource : notnull =>
        Cast<TGeometry, TOut>(query: Query<TGeometry, TValue>.Build(
            key: ClosestKey,
            state: (Point: point, Project: project),
            evaluator: static ((Point3d Point, ClosestCase<TSource, TValue> Project) state, TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                TSource source => state.Project(
                    target: state.Point,
                    geometry: source),
                _ => Fin.Fail<Seq<TValue>>(ClosestKey.Unsupported(
                    geometryType: typeof(TGeometry),
                    outputType: typeof(TValue))),
            }));

    internal static Query<Curve, TOut> LengthMass<TOut>(
        string name,
        Func<OperationKey, LengthMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = true,
        bool productMoments = false) =>
        Mass<Curve, LengthMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.CurveLength,
            compute: static (Curve geometry, GeometryContext _, bool second, bool product) =>
                Optional(geometry)
                    .ToFin(MassFault.Missing(label: nameof(Curve)))
                    .Bind(candidate => Optional(LengthMassProperties.Compute(
                            curve: candidate,
                            length: true,
                            firstMoments: true,
                            secondMoments: second,
                            productMoments: product))
                        .ToFin(MassFault.Failed(label: nameof(LengthMassProperties)))),
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);

    internal static Query<GeometryBase, TOut> AreaMass<TOut>(
        string name,
        Func<OperationKey, AreaMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = false,
        bool productMoments = false) =>
        Mass<GeometryBase, AreaMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.AreaMass,
            compute: static (GeometryBase geometry, GeometryContext context, bool second, bool product) =>
                Optional(geometry)
                    .ToFin(MassFault.Missing(label: nameof(GeometryBase)))
                    .Bind(candidate => candidate switch {
                        Curve curve => AreaMassProperties.Compute(
                                closedPlanarCurve: curve,
                                planarTolerance: context.Absolute.Value)
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
                                relativeTolerance: context.Relative.Value,
                                absoluteTolerance: context.Absolute.Value)
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
                            geometryType: candidate.GetType())),
                    }),
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);

    internal static Query<GeometryBase, TOut> VolumeMass<TOut>(
        string name,
        Func<OperationKey, VolumeMassProperties, Fin<Seq<TOut>>> project,
        bool secondMoments = false,
        bool productMoments = false) =>
        Mass<GeometryBase, VolumeMassProperties, TOut>(
            name: name,
            requirement: GeometryRequirement.VolumeMass,
            compute: static (GeometryBase geometry, GeometryContext context, bool second, bool product) =>
                Optional(geometry)
                    .ToFin(MassFault.Missing(label: nameof(GeometryBase)))
                    .Bind(candidate => candidate switch {
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
                                relativeTolerance: context.Relative.Value,
                                absoluteTolerance: context.Absolute.Value)
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
                            geometryType: candidate.GetType())),
                    }),
            project: project,
            secondMoments: secondMoments,
            productMoments: productMoments);

    private static Query<TGeometry, TOut> Mass<TGeometry, TMass, TOut>(
        string name,
        GeometryRequirement requirement,
        Func<TGeometry, GeometryContext, bool, bool, Fin<TMass>> compute,
        Func<OperationKey, TMass, Fin<Seq<TOut>>> project,
        bool secondMoments,
        bool productMoments) where TGeometry : notnull where TMass : class, IDisposable {
        OperationKey key = new(name: name);
        return Query<TGeometry, TOut>.Build(
            key: key,
            requirement: requirement,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext model) => compute(
                        arg1: geometry,
                        arg2: model,
                        arg3: secondMoments,
                        arg4: productMoments))
                    .Bind((TMass mass) => {
                        using TMass disposable = mass;
                        return project(arg1: key, arg2: disposable);
                    }));
    }

    private static Fin<TMass> Mass<TMass>(this TMass? mass, string label) where TMass : class, IDisposable =>
        Optional(mass)
            .ToFin(MassFault.Failed(label: label));

    internal static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(
        this OperationKey key,
        bool solved,
        double x,
        Vector3d xAxis,
        double y,
        Vector3d yAxis,
        double z,
        Vector3d zAxis) =>
        solved switch {
            true => Fin.Succ(Seq(
                (Moment: x, Axis: xAxis),
                (Moment: y, Axis: yAxis),
                (Moment: z, Axis: zAxis))),
            false => Fin.Fail<Seq<(double Moment, Vector3d Axis)>>(key.InvalidResult()),
        };

    private static class MassFault {
        internal static Error Missing(string label) =>
            Error.New(message: string.Create(
                provider: CultureInfo.InvariantCulture,
                $"Geometry mass properties require {label} input."));

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
