using Foundation.CSharp.Analyzers.Contracts;
using Rasm.Vectors;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[BoundaryAdapter, SmartEnum<int>]
public sealed partial class SpreadAspect {
    public static readonly SpreadAspect Frame = new(key: 0, output: typeof(Plane));
    public static readonly SpreadAspect PrincipalFrame = new(key: 1, output: typeof(Plane));
    public static readonly SpreadAspect Distribution = new(key: 2, output: typeof(Stat));
    public static readonly SpreadAspect Collinear = new(key: 3, output: typeof(bool));
    public static readonly SpreadAspect Coplanar = new(key: 4, output: typeof(bool));
    public Type Output { get; }
}

[SkipUnionOps]
[Union]
public partial record Points : IAspect {
    public sealed record ExtremaCase(Option<Seq<Vector3d>> Directions) : Points;
    public sealed record EdgeMidpointsCase : Points;
    public sealed record VerticesCase : Points;
    public sealed record ControlPointsCase : Points;
    public sealed record SpreadCase(SpreadAspect Aspect) : Points;
    public static Points Quadrants => new ExtremaCase(Directions: Option<Seq<Vector3d>>.None);
    public static Points Extrema(Seq<Vector3d> directions) => new ExtremaCase(Directions: Some(value: directions));
    public static Points EdgeMidpoints => new EdgeMidpointsCase();
    public static Points Vertices => new VerticesCase();
    public static Points ControlPoints => new ControlPointsCase();
    public static Points Spread(SpreadAspect aspect) => new SpreadCase(Aspect: aspect);
    private static readonly Op ExtremaKey = Op.Of(name: nameof(Extrema));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    private static readonly Op SpreadKey = Op.Of(name: nameof(Spread));
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        extremaCase: static c => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanCurveForm(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: ExtremaKey, requirement: Requirement.Basic, state: (Key: ExtremaKey, c.Directions),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from lease in GeometryKernel.CurveForm(source: geometry, op: state.Key).ToEff()
                    from points in lease.Use(curve => curve.IsValid switch {
                        false => Fin.Fail<Seq<Point3d>>(state.Key.InvalidInput()),
                        true => DirectionsFor(custom: state.Directions, planar: curve.IsPlanar(tolerance: context.Absolute.Value), context: context, key: state.Key)
                            .Bind(directions => directions.TraverseM(direction => Stat.Extrema(
                                    items: toSeq(curve.ExtremeParameters(direction: direction)).Map(curve.PointAt),
                                    projection: p => (Vector3d)p * direction,
                                    tolerance: 0.0,
                                    direction: ExtremumDirection.Maximum)
                                .Head.ToFin(state.Key.InvalidResult()))
                            .As()),
                    }).ToEff()
                    select points).As<TGeometry, TOut>(key: ExtremaKey)
            : ExtremaKey.Unsupported<TGeometry, TOut>(),
        edgeMidpointsCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.Can(type: typeof(TGeometry), predicate: static k => k.CanReadEdges)
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => Analyze.CurveProject<TGeometry, Point3d, Point3d>(
                    key: op,
                    aspect: Curves.All,
                    project: static (projection, _, _) => Fin.Succ(projection.As<Curve>().Map(static curve => new Lease<Curve>.Borrowed(Value: curve).Use(static owned => owned.PointAtNormalizedLength(length: 0.5)))))
                    .Apply(geometry: Seq(geometry))).As<TGeometry, TOut>(key: EdgeMidpointsKey)
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        verticesCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.CanReadVertices(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: VerticesKey, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from points in GeometryKernel.VerticesOf(source: geometry, key: op).ToEff()
                    from result in op.Accept(values: points).ToEff()
                    select result).As<TGeometry, TOut>(key: VerticesKey)
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPointsCase: static _ => typeof(TOut) == typeof(Point3d) && GeometryKernel.Can(type: typeof(TGeometry), predicate: static k => k.CanReadControlPoints)
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: ControlPointsKey, state: ControlPointsKey,
                evaluator: static (op, geometry) => from points in Analyze.ControlPointsOf(geometry: geometry, op: op).ToEff()
                                                    from result in op.Accept(values: points).ToEff()
                                                    select result).As<TGeometry, TOut>(key: ControlPointsKey)
            : ControlPointsKey.Unsupported<TGeometry, TOut>(),
        spreadCase: static s => Optional(s.Aspect).Match(
            Some: aspect => aspect.Output == typeof(TOut) && GeometryKernel.CanReadVertices(type: typeof(TGeometry))
                ? Analysis.Operation<TGeometry, TOut>.Build(
                    key: SpreadKey, requiresContext: true, state: (Key: SpreadKey, Aspect: aspect),
                    evaluator: static (state, geometry) =>
                        from context in Env.Asks
                        from points in GeometryKernel.VerticesOf(source: geometry, key: state.Key).ToEff()
                        from result in Analyze.SpreadProject<TOut>(aspect: state.Aspect, points: points, geometry: geometry, context: context, op: state.Key).ToEff()
                        select result)
                : SpreadKey.Unsupported<TGeometry, TOut>(),
            None: () => Analysis.Operation<TGeometry, TOut>.Reject(key: SpreadKey, fault: SpreadKey.InvalidInput())));
    private static Fin<Seq<Vector3d>> DirectionsFor(Option<Seq<Vector3d>> custom, bool planar, Context context, Op key) =>
        VectorIntent.Axes(values: custom, planar: planar).Project<Seq<Vector3d>>(context: context, key: key);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Points<TGeometry, TOut>(Points sampling) where TGeometry : notnull => Aspect<Points, TGeometry, TOut>(aspect: sampling);
    internal static Fin<Seq<Point3d>> ControlPointsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => curve is NurbsCurve nc
                ? Fin.Succ(toSeq(Enumerable.Range(0, nc.Points.Count).Select(i => nc.Points[i].Location)))
                : Optional(curve.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static c => new Lease<NurbsCurve>.Owned(Value: c).Use(static d => toSeq(Enumerable.Range(0, d.Points.Count).Select(i => d.Points[i].Location).ToArray()))),
            Surface surface => surface is NurbsSurface ns
                ? Fin.Succ(toSeq(Enumerable.Range(0, ns.Points.CountU).SelectMany(u => Enumerable.Range(0, ns.Points.CountV).Select(v => ns.Points.GetControlPoint(u, v).Location))))
                : Optional(surface.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => new Lease<NurbsSurface>.Owned(Value: s).Use(static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray()))),
            Brep brep => toSeq(brep.Faces).TraverseM(f => Optional(f.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static s => new Lease<NurbsSurface>.Owned(Value: s).Use(static d => toSeq(Enumerable.Range(0, d.Points.CountU).SelectMany(u => Enumerable.Range(0, d.Points.CountV).Select(v => d.Points.GetControlPoint(u, v).Location)).ToArray())))).As().Map(static nested => nested.Bind(static points => points)),
            object surfaceLike when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) => GeometryKernel.SurfaceForm(source: surfaceLike, op: op).Bind(lease => lease.Use(surface => ControlPointsOf(geometry: surface, op: op))),
            _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(g.GetType(), typeof(Point3d))),
        });
    internal static Fin<Seq<TOut>> SpreadProject<TOut>(SpreadAspect aspect, Seq<Point3d> points, object geometry, Context context, Op op) =>
        aspect.Equals(SpreadAspect.Distribution)
            ? CentroidOf(geometry: geometry, context: context, op: op).Bind(centroid => Stat.Of(values: points.Map(point => point.DistanceTo(other: centroid)), key: op)).Bind(stat => op.AcceptResults<Stat, TOut>(values: Seq(stat)))
            : (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane fit, maximumDeviation: out double dev), fit.IsValid) switch {
                (PlaneFitResult.Success, true) => aspect switch {
                    SpreadAspect a when a.Equals(SpreadAspect.Frame) => op.AcceptResults<Plane, TOut>(values: Seq(fit)),
                    SpreadAspect a when a.Equals(SpreadAspect.PrincipalFrame) => OrientedFrame(fit: fit, points: points, context: context, op: op).Bind(plane => op.AcceptResults<Plane, TOut>(values: Seq(plane))),
                    SpreadAspect a when a.Equals(SpreadAspect.Coplanar) => op.AcceptResults<bool, TOut>(values: Seq(dev <= context.Absolute.Value)),
                    SpreadAspect a when a.Equals(SpreadAspect.Collinear) => MinorSpread(fit: fit, points: points, context: context, op: op).Bind(spread => op.AcceptResults<bool, TOut>(values: Seq(spread <= context.Absolute.Value))),
                    _ => Fin.Fail<Seq<TOut>>(op.Unsupported(geometryType: typeof(SpreadAspect), outputType: typeof(TOut))),
                },
                _ when (aspect.Equals(SpreadAspect.Coplanar) || aspect.Equals(SpreadAspect.Collinear)) && points.Count <= 2 => op.AcceptResults<bool, TOut>(values: Seq(value: true)),
                _ => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
            };
    private static Fin<Plane> OrientedFrame(Plane fit, Seq<Point3d> points, Context context, Op op) =>
        from angle in PrincipalAngle(points: points, fit: fit, context: context, op: op)
        from xAxis in VectorIntent.Direction(value: (fit.XAxis * Math.Cos(d: angle)) + (fit.YAxis * Math.Sin(a: angle))).Project<Vector3d>(context: context, key: op)
        from yAxis in VectorIntent.Direction(value: Vector3d.CrossProduct(a: fit.ZAxis, b: xAxis)).Project<Vector3d>(context: context, key: op)
        from plane in op.AcceptValue(value: new Plane(origin: fit.Origin, xDirection: xAxis, yDirection: yAxis))
        select plane;
    private static Fin<double> MinorSpread(Plane fit, Seq<Point3d> points, Context context, Op op) =>
        from angle in PrincipalAngle(points: points, fit: fit, context: context, op: op)
        from spread in points.TraverseM(point => VectorIntent.Components(anchor: fit.Origin, value: point - fit.Origin, frame: fit)
                .Project<(double X, double Y)>(context: context, key: op)
            .Map(components => Math.Abs(value: (components.X * -Math.Sin(a: angle)) + (components.Y * Math.Cos(d: angle))))
            .BindFail(static _ => Fin.Succ(0.0))).As()
        select spread.Fold(initialState: 0.0, f: Math.Max);
    // Principal angle of the 2D point cloud in `fit` plane coordinates. Domain owns raw
    // covariance moments; Vectors owns the 2x2 symmetric eigen decomposition, where the
    // first eigenvector encodes the principal axis directly via atan2.
    private static Fin<double> PrincipalAngle(Seq<Point3d> points, Plane fit, Context context, Op op) {
        Seq<Arr<double>> planar = points.Map(point =>
            VectorIntent.Components(anchor: fit.Origin, value: point - fit.Origin, frame: fit)
                .Project<(double X, double Y)>(context: context, key: op)
                .Match(
                    Succ: v => new Arr<double>([v.X, v.Y]),
                    Fail: _ => new Arr<double>([0.0, 0.0])));
        return SampleMoment.Of(rows: planar, dimension: 2, key: op)
            .Bind(moment => SymmetricMatrix.Of(dim: Vectors.Dimension.Create(value: moment.Dimension), upper: moment.UpperCovariance, key: op))
            .Bind(covariance => covariance.DecomposeEigen(key: op))
            .Map(static eigen => Math.Atan2(y: eigen[0].Eigenvector[1], x: eigen[0].Eigenvector[0]));
    }
}
