namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Points : IAspect {
    public sealed record Quadrants : Points; public sealed record EdgeMidpoints : Points; public sealed record Vertices : Points; public sealed record ControlPoints : Points;
    private static readonly Op QuadrantsKey = Op.Of(name: nameof(Quadrants));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        quadrants: static _ => typeof(TOut) == typeof(Point3d)
            ? Analyze.Cast<TGeometry, TOut>(key: QuadrantsKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: QuadrantsKey, requirement: Requirement.CurveLength, state: QuadrantsKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from result in (geometry switch {
                        Curve curve when curve.IsValid => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value),
                        object value => GeometryKernel.CurveForm(source: value, op: op).Bind(lease => lease.Use(curve => Analyze.ExtractCardinals(op: op, curve: curve, tolerance: context.Absolute.Value))),
                    }).ToEff()
                    select result))
            : QuadrantsKey.Unsupported<TGeometry, TOut>(),
        edgeMidpoints: static _ => typeof(TOut) == typeof(Point3d) && (typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase) || typeof(TGeometry) == typeof(Line) || typeof(TGeometry) == typeof(Polyline) || typeof(TGeometry) == typeof(BoundingBox) || typeof(TGeometry) == typeof(Box) || GeometryKernel.CanProjectCurves(type: typeof(TGeometry), feature: Some(CurveFeature.Edge)))
            ? Analyze.Cast<TGeometry, TOut>(key: EdgeMidpointsKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => op.Accept(value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => op.Accept(values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => op.Accept(values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Box box => op.Accept(values: box.BoundingBox.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    _ => from runtime in Env.EnvAsks
                         from curves in Analyze.CurveProjections(geometry: geometry, selector: new Curves.Selector(Feature: CurveFeature.Edge), context: runtime.Context, op: op, cancel: runtime.Cancellation).ToEff()
                         from result in TopologyProjection.Project(all: curves, chosen: curves, project: values => op.Accept(values: values.Choose(static projection => projection.As<Curve>().Map(static c => new Lease<Curve>.Borrowed(Value: c).Use(static owned => owned.PointAtNormalizedLength(length: 0.5)))))).ToEff()
                         select result,
                }))
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        vertices: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadVertices(type: typeof(TGeometry))
            ? Analyze.Cast<TGeometry, TOut>(key: VerticesKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: VerticesKey, requiresContext: true, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from points in Analyze.VerticesOf(geometry: geometry, context: context, op: op).ToEff()
                    from result in op.Accept(values: points).ToEff()
                    select result))
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPoints: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadControlPoints(type: typeof(TGeometry))
            ? Analyze.Cast<TGeometry, TOut>(key: ControlPointsKey, operation: global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: ControlPointsKey, requiresContext: true, state: ControlPointsKey,
                evaluator: static (op, geometry) => from points in Analyze.ControlPointsOf(geometry: geometry, op: op).ToEff()
                                                    from result in op.Accept(values: points).ToEff()
                                                    select result))
            : ControlPointsKey.Unsupported<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Points<TGeometry, TOut>(Points sampling) where TGeometry : notnull => Aspect<Points, TGeometry, TOut>(aspect: sampling);
    internal static Fin<Seq<Point3d>> VerticesOf<TGeometry>(TGeometry geometry, Context context, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Point3d point => Fin.Succ(Seq(point)),
            Point point => Fin.Succ(Seq(point.Location)),
            Line line => Fin.Succ(Seq(line.From, line.To)),
            Polyline polyline => Fin.Succ(toSeq(polyline)),
            BoundingBox box => Fin.Succ(toSeq(box.GetCorners())),
            Box box => Fin.Succ(toSeq(box.GetCorners())),
            Curve curve => Fin.Succ(curve.TryGetPolyline(polyline: out Polyline poly) ? toSeq(poly) : Seq(curve.PointAtStart, curve.PointAtEnd)),
            Brep brep => Fin.Succ(toSeq(brep.DuplicateVertices())),
            Mesh mesh => Fin.Succ(toSeq(mesh.Vertices.ToPoint3dArray())),
            PointCloud cloud => Fin.Succ(toSeq(cloud.GetPoints())),
            SubD subd => Fin.Succ(toSeq(LanguageExt.List.unfold((SubDVertex?)subd.Vertices.First, static v => v switch { SubDVertex sv => Some((sv.ControlNetPoint, (SubDVertex?)sv.Next)), _ => None }))),
            GeometryBase { HasBrepForm: true } native => GeometryKernel.BrepForm(source: native, op: op).Bind(lease => lease.Use(brep => VerticesOf(geometry: brep, context: context, op: op))),
            Surface surface => (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
                (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(surface.PointAt(u: u.T0, v: v.T0), surface.PointAt(u: u.T1, v: v.T0), surface.PointAt(u: u.T1, v: v.T1), surface.PointAt(u: u.T0, v: v.T1))),
                _ => Fin.Fail<Seq<Point3d>>(op.InvalidResult()),
            },
            _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(g.GetType(), typeof(Point3d))),
        });
    internal static Fin<Seq<Point3d>> ControlPointsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => curve is NurbsCurve nc
                ? Fin.Succ(toSeq(Enumerable.Range(0, nc.Points.Count).Select(i => nc.Points[i].Location)))
                : Optional(curve.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static c => new Lease<NurbsCurve>.Owned(Value: c).Use(static d => toSeq(Enumerable.Range(0, d.Points.Count).Select(i => d.Points[i].Location).ToArray()))),
            Surface surface => surface is NurbsSurface ns
                ? Fin.Succ(toSeq(Enumerable.Range(0, ns.Points.CountU).SelectMany(u => Enumerable.Range(0, ns.Points.CountV).Select(v => ns.Points.GetControlPoint(u, v).Location))))
                : Optional(surface.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => new Lease<NurbsSurface>.Owned(Value: s).Use(static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray()))),
            Brep brep => toSeq(brep.Faces).TraverseM(f => Optional(f.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => new Lease<NurbsSurface>.Owned(Value: s).Use(static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray())))).As().Map(static nested => nested.Bind(static points => points)),
            _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(g.GetType(), typeof(Point3d))),
        });
    internal static Fin<Seq<Point3d>> ExtractCardinals(Op op, Curve curve, double tolerance) =>
        Seq((Direction: Vector3d.XAxis, Maximize: false), (Direction: Vector3d.XAxis, Maximize: true), (Direction: Vector3d.YAxis, Maximize: false), (Direction: Vector3d.YAxis, Maximize: true), (Direction: Vector3d.ZAxis, Maximize: false), (Direction: Vector3d.ZAxis, Maximize: true))
            .Take(curve.IsPlanar(tolerance: tolerance) switch { true => 4, false => 6 })
            .TraverseM(state => Stats.Maxima(
                    items: toSeq(curve.ExtremeParameters(direction: state.Direction)).Map(curve.PointAt),
                    projection: p => (Vector3d)p * (state.Maximize switch { true => state.Direction, false => -state.Direction }),
                    tolerance: 0.0)
                .Head.ToFin(op.InvalidResult()))
            .As();
}
