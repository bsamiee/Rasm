namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Points : IAspect {
    public sealed record Quadrants : Points; public sealed record EdgeMidpoints : Points; public sealed record Vertices : Points; public sealed record ControlPoints : Points;
    private static readonly Op QuadrantsKey = Op.Of(name: nameof(Quadrants));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull => Switch<Query<TGeometry, TOut>>(
        quadrants: static _ => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: QuadrantsKey, query: Query<TGeometry, Point3d>.Build(
                key: QuadrantsKey, requirement: Requirement.CurveLength, state: QuadrantsKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from result in (geometry switch {
                        Curve curve when curve.IsValid => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value),
                        Polyline polyline when polyline.IsValid => Analyze.Bracket(factory: polyline.ToPolylineCurve, body: (Curve curve) => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Line line when line.IsValid => Analyze.Bracket(factory: () => new LineCurve(line: line), body: (Curve curve) => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Circle circle when circle.IsValid => Analyze.Bracket(factory: circle.ToNurbsCurve, body: (Curve curve) => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        Arc arc when arc.IsValid => Analyze.Bracket(factory: arc.ToNurbsCurve, body: (Curve curve) => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value)),
                        _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    }).ToEff()
                    select result))
            : QuadrantsKey.Unsupported<TGeometry, TOut>(),
        edgeMidpoints: static _ => typeof(TOut) == typeof(Point3d) && (Analyze.Supports(geometry: typeof(TGeometry), native: [typeof(Line), typeof(Polyline), typeof(BoundingBox), typeof(Box)]) || Dispatch.Supports(CapTag.Curves, typeof(TGeometry)))
            ? Analyze.Cast<TGeometry, TOut>(key: EdgeMidpointsKey, query: Query<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => Analyze.One(key: op, value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => Analyze.Many(key: op, values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => Analyze.Many(key: op, values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Box box => Analyze.Many(key: op, values: box.BoundingBox.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    _ => from runtime in Env.EnvAsks
                         from curves in Dispatch.Resolve<Seq<TopologyProjection>, (CurveSelector, Context, Op, CancellationToken)>(CapTag.Curves, geometry, (new CurveSelector(Feature: CurveFeature.Edge), runtime.Context, op, runtime.Cancellation), op, variant: CurveFeature.Edge).ToEff()
                         from result in Analyze.Many(key: op, values: curves.Choose(static projection => projection.As<Curve>().Map(static c => Dispatch.Borrowed(c, static owned => owned.PointAtNormalizedLength(length: 0.5))))).ToEff()
                         select result,
                }))
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        vertices: static _ => typeof(TOut) == typeof(Point3d) && Dispatch.Supports(CapTag.Vertices, typeof(TGeometry))
            ? Analyze.Cast<TGeometry, TOut>(key: VerticesKey, query: Query<TGeometry, Point3d>.Build(
                key: VerticesKey, requiresContext: true, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from points in Dispatch.Resolve<Seq<Point3d>, (Context, Op)>(CapTag.Vertices, geometry, (context, op), op).ToEff()
                    from result in Analyze.Many(key: op, values: points).ToEff()
                    select result))
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPoints: static _ => typeof(TOut) == typeof(Point3d) && Dispatch.Supports(CapTag.ControlPoints, typeof(TGeometry))
            ? Analyze.Cast<TGeometry, TOut>(key: ControlPointsKey, query: Query<TGeometry, Point3d>.Build(
                key: ControlPointsKey, requiresContext: true, state: ControlPointsKey,
                evaluator: static (op, geometry) => from points in Dispatch.Resolve<Seq<Point3d>, Op>(CapTag.ControlPoints, geometry, op, op).ToEff()
                                                    from result in Analyze.Many(key: op, values: points).ToEff()
                                                    select result))
            : ControlPointsKey.Unsupported<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Points<TGeometry, TOut>(Points sampling) where TGeometry : notnull => Aspect<Points, TGeometry, TOut>(aspect: sampling);
    internal static Fin<Seq<Point3d>> ExtractCardinals(Op op, Curve curve, double tolerance) =>
        Seq((Direction: Vector3d.XAxis, Maximize: false), (Direction: Vector3d.XAxis, Maximize: true), (Direction: Vector3d.YAxis, Maximize: false), (Direction: Vector3d.YAxis, Maximize: true), (Direction: Vector3d.ZAxis, Maximize: false), (Direction: Vector3d.ZAxis, Maximize: true))
            .Take(curve.IsPlanar(tolerance: tolerance) switch { true => 4, false => 6 })
            .TraverseM(state => toSeq(curve.ExtremeParameters(direction: state.Direction)).Map(curve.PointAt)
                .Maxima(projection: p => (Vector3d)p * (state.Maximize switch { true => state.Direction, false => -state.Direction }), tolerance: 0.0)
                .Head.ToFin(op.InvalidResult()))
            .As();
}
