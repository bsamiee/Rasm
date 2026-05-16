namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
public enum SpreadAspect { Frame, PrincipalFrame, Distribution, Collinear, Coplanar }

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Points : IAspect {
    public sealed record QuadrantsCase : Points; public sealed record EdgeMidpointsCase : Points; public sealed record VerticesCase : Points; public sealed record ControlPointsCase : Points;
    public sealed record SpreadCase(SpreadAspect Aspect) : Points;
    public static Points Quadrants => new QuadrantsCase();
    public static Points EdgeMidpoints => new EdgeMidpointsCase();
    public static Points Vertices => new VerticesCase();
    public static Points ControlPoints => new ControlPointsCase();
    public static Points Spread(SpreadAspect aspect) => new SpreadCase(Aspect: aspect);
    private static readonly Op QuadrantsKey = Op.Of(name: nameof(Quadrants));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    private static readonly Op SpreadKey = Op.Of(name: nameof(Spread));
    private static readonly Seq<(Vector3d Direction, bool Maximize)> QuadrantProbes = Seq(
        (Vector3d.XAxis, false), (Vector3d.XAxis, true), (Vector3d.YAxis, false), (Vector3d.YAxis, true), (Vector3d.ZAxis, false), (Vector3d.ZAxis, true));
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        quadrantsCase: static _ => typeof(TOut) == typeof(Point3d)
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: QuadrantsKey, requirement: Requirement.Basic, requiresContext: true, state: QuadrantsKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from lease in GeometryKernel.CurveForm(source: geometry, op: op).ToEff()
                    from points in lease.Use(curve => curve.IsValid switch {
                        false => Fin.Fail<Seq<Point3d>>(op.InvalidInput()),
                        true => QuadrantProbes
                            .Take(curve.IsPlanar(tolerance: context.Absolute.Value) switch { true => 4, false => 6 })
                            .TraverseM(state => Stat.Extrema(
                                    items: toSeq(curve.ExtremeParameters(direction: state.Direction)).Map(curve.PointAt),
                                    projection: p => (Vector3d)p * (state.Maximize switch { true => state.Direction, false => -state.Direction }),
                                    tolerance: 0.0,
                                    direction: ExtremumDirection.Maximum)
                                .Head.ToFin(op.InvalidResult()))
                            .As(),
                    }).ToEff()
                    select points).As<TGeometry, TOut>(key: QuadrantsKey)
            : QuadrantsKey.Unsupported<TGeometry, TOut>(),
        edgeMidpointsCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadEdges(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => geometry switch {
                    Line line => op.Accept(value: line.PointAt(t: 0.5)).ToEff(),
                    Polyline polyline => op.Accept(values: polyline.GetSegments().Select(static segment => segment.PointAt(t: 0.5))).ToEff(),
                    BoundingBox box => op.Accept(values: box.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    Box box => op.Accept(values: box.BoundingBox.GetEdges().Select(static edge => edge.PointAt(t: 0.5))).ToEff(),
                    _ => from runtime in Env.EnvAsks
                         from kind in ((object)geometry).KindOf(context: runtime.Context).ToEff()
                         from curves in (kind.Topology switch {
                             Topology.Brep or Topology.Mesh or Topology.SubD => Analyze.CurveProjections(geometry: geometry, aspect: Curves.All, feature: CurveFeature.Edge, context: runtime.Context, op: op, cancel: runtime.Cancellation),
                             _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(geometryType: geometry.GetType(), outputType: typeof(Curve))),
                         }).ToEff()
                         from result in TopologyProjection.Project(all: curves, chosen: curves, project: values => op.Accept(values: values.Choose(static projection => projection.As<Curve>().Map(static c => new Lease<Curve>.Borrowed(Value: c).Use(static owned => owned.PointAtNormalizedLength(length: 0.5)))))).ToEff()
                         select result,
                }).As<TGeometry, TOut>(key: EdgeMidpointsKey)
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        verticesCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadVertices(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: VerticesKey, requiresContext: true, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from context in Env.Asks
                    from points in Analyze.VerticesOf(geometry: geometry, context: context, op: op).ToEff()
                    from result in op.Accept(values: points).ToEff()
                    select result).As<TGeometry, TOut>(key: VerticesKey)
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPointsCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadControlPoints(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: ControlPointsKey, state: ControlPointsKey,
                evaluator: static (op, geometry) => from points in Analyze.ControlPointsOf(geometry: geometry, op: op).ToEff()
                                                    from result in op.Accept(values: points).ToEff()
                                                    select result).As<TGeometry, TOut>(key: ControlPointsKey)
            : ControlPointsKey.Unsupported<TGeometry, TOut>(),
        spreadCase: static s => ((s.Aspect is SpreadAspect.Frame or SpreadAspect.PrincipalFrame && typeof(TOut) == typeof(Plane))
                || (s.Aspect == SpreadAspect.Distribution && typeof(TOut) == typeof(Stat))
                || (s.Aspect is SpreadAspect.Collinear or SpreadAspect.Coplanar && typeof(TOut) == typeof(bool)))
            && GeometryKernel.CanReadVertices(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, TOut>.Build(
                key: SpreadKey, requiresContext: true, state: (Key: SpreadKey, s.Aspect),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from points in Analyze.VerticesOf(geometry: geometry, context: context, op: state.Key).ToEff()
                    from result in Analyze.SpreadProject<TOut>(aspect: state.Aspect, points: points, geometry: geometry, context: context, op: state.Key).ToEff()
                    select result)
            : SpreadKey.Unsupported<TGeometry, TOut>());
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Points<TGeometry, TOut>(Points sampling) where TGeometry : notnull => Aspect<Points, TGeometry, TOut>(aspect: sampling);
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
    internal static Fin<Seq<TOut>> SpreadProject<TOut>(SpreadAspect aspect, Seq<Point3d> points, object geometry, Context context, Op op) =>
        aspect == SpreadAspect.Distribution
            ? CentroidOf(geometry: geometry, context: context, op: op).Bind(centroid => Stat.Of(values: points.Map(point => point.DistanceTo(other: centroid)), key: op)).Bind(stat => op.AcceptResults<Stat, TOut>(values: Seq(stat)))
            : (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane fit, maximumDeviation: out double dev), fit.IsValid) switch {
                (PlaneFitResult.Success, true) => aspect switch {
                    SpreadAspect.Frame => op.AcceptResults<Plane, TOut>(values: Seq(fit)),
                    SpreadAspect.PrincipalFrame => OrientedFrame(fit: fit, points: points).ToFin(op.InvalidResult()).Bind(plane => op.AcceptResults<Plane, TOut>(values: Seq(plane))),
                    SpreadAspect.Coplanar => op.AcceptResults<bool, TOut>(values: Seq(dev <= context.Absolute.Value)),
                    SpreadAspect.Collinear => op.AcceptResults<bool, TOut>(values: Seq(MinorSpread(fit: fit, points: points) <= context.Absolute.Value)),
                    _ => Fin.Fail<Seq<TOut>>(op.Unsupported(geometryType: typeof(SpreadAspect), outputType: typeof(TOut))),
                },
                (PlaneFitResult.Inconclusive, _) when aspect is SpreadAspect.Coplanar or SpreadAspect.Collinear => op.AcceptResults<bool, TOut>(values: Seq(true)),
                _ when aspect is SpreadAspect.Coplanar or SpreadAspect.Collinear && points.Count <= 2 => op.AcceptResults<bool, TOut>(values: Seq(true)),
                _ => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
            };
    private static Option<Plane> OrientedFrame(Plane fit, Seq<Point3d> points) {
        double angle = PrincipalAngle(points: points, fit: fit);
        Vector3d xAxis = (fit.XAxis * Math.Cos(d: angle)) + (fit.YAxis * Math.Sin(a: angle));
        return new Plane(origin: fit.Origin, xDirection: xAxis, yDirection: Vector3d.CrossProduct(a: fit.ZAxis, b: xAxis)) is { IsValid: true } principal ? Some(principal) : Option<Plane>.None;
    }
    private static double MinorSpread(Plane fit, Seq<Point3d> points) {
        double angle = PrincipalAngle(points: points, fit: fit);
        return points.Map(point => Math.Abs(value: ((point - fit.Origin) * fit.XAxis * -Math.Sin(a: angle)) + ((point - fit.Origin) * fit.YAxis * Math.Cos(d: angle)))).Fold(initialState: 0.0, f: Math.Max);
    }
    private static double PrincipalAngle(Seq<Point3d> points, Plane fit) =>
        points.Fold(initialState: (Sxx: 0.0, Sxy: 0.0, Syy: 0.0), f: (state, point) => ((point - fit.Origin) * fit.XAxis, (point - fit.Origin) * fit.YAxis) switch {
            (double x, double y) => (state.Sxx + (x * x), state.Sxy + (x * y), state.Syy + (y * y)),
        }) switch { var c => 0.5 * Math.Atan2(y: 2.0 * c.Sxy, x: c.Sxx - c.Syy) };
}
