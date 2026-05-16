namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Location : IAspect {
    public sealed record MidpointCase : Location;
    public sealed record TangentCase : Location;
    public sealed record ClosestCase(Point3d Point) : Location;
    public sealed record PointAtCurveCase(double Parameter) : Location;
    public sealed record PointAtSurfaceCase(Point2d Uv) : Location;
    public sealed record PointAtLengthCase(double Length) : Location;
    public sealed record FrameAtCurveCase(double Parameter) : Location;
    public sealed record FrameAtSurfaceCase(Point2d Uv) : Location;
    public sealed record PerpendicularFrameAtCase(Seq<double> Parameters) : Location;
    public sealed record NormalAtCase(Point2d Uv) : Location;
    public sealed record CurvatureAtCurveCase(double Parameter) : Location;
    public sealed record CurvatureAtSurfaceCase(Point2d Uv) : Location;
    public sealed record CurvatureCase(int Count, CurvatureMode Mode) : Location;
    public sealed record DerivativeAtCase(double Parameter, int Count) : Location;
    public sealed record DivideByCountCase(int Count) : Location;
    public sealed record DivideByLengthCase(double Length) : Location;
    public sealed record OrientationCase(Plane Plane) : Location;
    public sealed record ContainsCase(Point3d Point, Plane Plane) : Location;
    public sealed record ShortPathCase(Point2d Start, Point2d End) : Location;
    public sealed record ParameterAtCase(Point3d Probe) : Location;
    public sealed record LengthAtCase(double Parameter) : Location;
    public static Location Midpoint => new MidpointCase();
    public static Location Tangent => new TangentCase();
    public static Location Closest(Point3d point) => new ClosestCase(Point: point);
    public static Location PointAtCurve(double parameter) => new PointAtCurveCase(Parameter: parameter);
    public static Location PointAtSurface(Point2d uv) => new PointAtSurfaceCase(Uv: uv);
    public static Location PointAtLength(double length) => new PointAtLengthCase(Length: length);
    public static Location FrameAtCurve(double parameter) => new FrameAtCurveCase(Parameter: parameter);
    public static Location FrameAtSurface(Point2d uv) => new FrameAtSurfaceCase(Uv: uv);
    public static Location PerpendicularFrameAt(params double[] parameters) => new PerpendicularFrameAtCase(Parameters: toSeq(parameters));
    public static Location NormalAt(Point2d uv) => new NormalAtCase(Uv: uv);
    public static Location CurvatureAtCurve(double parameter) => new CurvatureAtCurveCase(Parameter: parameter);
    public static Location CurvatureAtSurface(Point2d uv) => new CurvatureAtSurfaceCase(Uv: uv);
    public static Location Curvature(int count, CurvatureMode mode) => new CurvatureCase(Count: count, Mode: mode);
    public static Location DerivativeAt(double parameter, int count) => new DerivativeAtCase(Parameter: parameter, Count: count);
    public static Location DivideByCount(int count) => new DivideByCountCase(Count: count);
    public static Location DivideByLength(double length) => new DivideByLengthCase(Length: length);
    public static Location Orientation(Plane plane) => new OrientationCase(Plane: plane);
    public static Location Contains(Point3d point, Plane plane) => new ContainsCase(Point: point, Plane: plane);
    public static Location ShortPath(Point2d start, Point2d end) => new ShortPathCase(Start: start, End: end);
    public static Location ParameterAt(Point3d probe) => new ParameterAtCase(Probe: probe);
    public static Location LengthAt(double parameter) => new LengthAtCase(Parameter: parameter);
    internal static readonly Op MidpointKey = Op.Of(name: "Midpoint");
    internal static readonly Op TangentKey = Op.Of(name: "Tangent");
    internal static readonly Op PointAtKey = Op.Of(name: "PointAt");
    internal static readonly Op PointAtLengthKey = Op.Of(name: "PointAtLength");
    internal static readonly Op FrameAtKey = Op.Of(name: "FrameAt");
    internal static readonly Op PerpendicularFrameAtKey = Op.Of(name: "PerpendicularFrameAt");
    internal static readonly Op CurvatureAtKey = Op.Of(name: "CurvatureAt");
    internal static readonly Op DerivativeAtKey = Op.Of(name: "DerivativeAt");
    internal static readonly Op DivideByCountKey = Op.Of(name: "DivideByCount");
    internal static readonly Op DivideByLengthKey = Op.Of(name: "DivideByLength");
    internal static readonly Op OrientationKey = Op.Of(name: "Orientation");
    internal static readonly Op ContainsKey = Op.Of(name: "Contains");
    internal static readonly Op NormalAtKey = Op.Of(name: "NormalAt");
    internal static readonly Op ShortPathKey = Op.Of(name: "ShortPath");
    internal static readonly Op ParameterAtKey = Op.Of(name: "ParameterAt");
    internal static readonly Op LengthAtKey = Op.Of(name: "LengthAt");
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        midpointCase: static _ => LocationDispatch.AtMidpoint<TGeometry, TOut, Point3d>(key: MidpointKey, project: static (curve, parameter) => curve.PointAt(t: parameter)),
        tangentCase: static _ => LocationDispatch.AtMidpoint<TGeometry, TOut, Vector3d>(key: TangentKey, project: static (curve, parameter) => curve.TangentAt(t: parameter)),
        closestCase: static c => LocationDispatch.ClosestPoint<TGeometry, TOut>(point: c.Point),
        curvatureCase: static cp => LocationDispatch.Curvature<TGeometry, TOut>(count: cp.Count, mode: cp.Mode),
        pointAtCurveCase: static pac => LocationDispatch.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, operation: () => LocationDispatch.CurveAt<TGeometry, Point3d>(key: PointAtKey, parameter: pac.Parameter, project: static (curve, p) => PointAtKey.Accept(value: curve.PointAt(t: p)))),
        pointAtLengthCase: static pal => LocationDispatch.Located<TGeometry, TOut, Curve, Point3d>(
            key: PointAtLengthKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: PointAtLengthKey, requirement: Requirement.CurveLength, state: (Key: PointAtLengthKey, Distance: pal.Length),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                        .Bind(lease => lease.Use(curve => curve.LengthParameter(segmentLength: state.Distance, t: out double parameter, fractionalTolerance: context.Fractional) switch {
                            true => state.Key.Accept(value: curve.PointAt(t: parameter)),
                            false => Fin.Fail<Seq<Point3d>>(state.Key.InvalidResult()),
                        })).ToEff()
                    select result)),
        frameAtCurveCase: static fac => LocationDispatch.Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, operation: () =>
            LocationDispatch.CurveAt<TGeometry, Plane>(key: FrameAtKey, parameter: fac.Parameter, project: static (curve, t) =>
                curve.FrameAt(t: t, plane: out Plane frame) ? FrameAtKey.Accept(value: frame) : Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()))),
        perpendicularFrameAtCase: static pfa => LocationDispatch.Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, operation: () =>
            global::Rasm.Analysis.Operation<TGeometry, Plane>.Build(
                key: PerpendicularFrameAtKey, requirement: Requirement.CurveLength, state: (Key: PerpendicularFrameAtKey, pfa.Parameters),
                evaluator: static (state, geometry) =>
                    GeometryKernel.CurveForm(source: geometry, op: state.Key)
                        .Bind(lease => lease.Use(curve => Optional(curve.GetPerpendicularFrames(toSeq(state.Parameters.AsIterable().OrderBy(t => t).Distinct()).AsIterable()))
                            .ToFin(state.Key.InvalidResult())
                            .Bind(planes => state.Key.Accept(values: planes))))
                        .ToEff())),
        curvatureAtCurveCase: static cac => LocationDispatch.Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, operation: () => LocationDispatch.CurveAt<TGeometry, Vector3d>(key: CurvatureAtKey, parameter: cac.Parameter, project: static (curve, p) => CurvatureAtKey.Accept(value: curve.CurvatureAt(t: p)))),
        derivativeAtCase: static da => da.Count < 0
            ? global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: DerivativeAtKey, fault: DerivativeAtKey.InvalidInput())
            : LocationDispatch.Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, operation: () => LocationDispatch.CurveAt<TGeometry, Vector3d>(key: DerivativeAtKey, parameter: da.Parameter, project: (curve, p) => DerivativeAtKey.Accept(values: curve.DerivativeAt(t: p, derivativeCount: da.Count)))),
        divideByCountCase: static dbc => LocationDispatch.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, operation: () => LocationDispatch.DividePoly<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        divideByLengthCase: static dbl => LocationDispatch.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, operation: () => LocationDispatch.DividePoly<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        orientationCase: static o => LocationDispatch.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) =>
                GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame))))
                    .ToEff())),
        containsCase: static cnt => LocationDispatch.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, PointContainment>.Build(
            key: ContainsKey, requiresContext: true, state: (Key: ContainsKey, Probe: cnt.Point, Frame: cnt.Plane),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                        PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                        PointContainment containment => state.Key.Accept(value: containment),
                    })).ToEff()
                select result)),
        pointAtSurfaceCase: static pas => LocationDispatch.Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, operation: () => LocationDispatch.SurfaceUv<TGeometry, Point3d>(key: PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => PointAtKey.Accept(value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
        frameAtSurfaceCase: static fas => LocationDispatch.Located<TGeometry, TOut, Surface, Plane>(key: FrameAtKey, operation: () => LocationDispatch.SurfaceUv<TGeometry, Plane>(
            key: FrameAtKey, uv: fas.Uv, project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                true => FrameAtKey.Accept(value: frame),
                false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
            })),
        normalAtCase: static na => LocationDispatch.Located<TGeometry, TOut, Surface, Vector3d>(key: NormalAtKey, operation: () => LocationDispatch.SurfaceUv<TGeometry, Vector3d>(
            key: NormalAtKey, uv: na.Uv, project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                Vector3d normal when normal.IsValid && !normal.IsTiny() => NormalAtKey.Accept(value: normal),
                _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
            })),
        curvatureAtSurfaceCase: static cas => LocationDispatch.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, operation: () => LocationDispatch.SurfaceUv<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
        shortPathCase: static sp => LocationDispatch.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, operation: () => LocationDispatch.ShortPath<TGeometry>(key: ShortPathKey, start: sp.Start, end: sp.End)),
        parameterAtCase: static pat => LocationDispatch.ParameterAt<TGeometry, TOut>(key: ParameterAtKey, probe: pat.Probe),
        lengthAtCase: static lat => LocationDispatch.Located<TGeometry, TOut, Curve, double>(key: LengthAtKey, operation: () => LocationDispatch.CurveAt<TGeometry, double>(key: LengthAtKey, parameter: lat.Parameter, project: static (curve, t) => curve.GetLength(subdomain: new Interval(curve.Domain.T0, t)) switch {
            double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => LengthAtKey.Accept(value: length),
            _ => Fin.Fail<Seq<double>>(LengthAtKey.InvalidResult()),
        })));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Location<TGeometry, TOut>(Location aspect) where TGeometry : notnull => Aspect<Location, TGeometry, TOut>(aspect: aspect);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
file static class LocationDispatch {
    public static Operation<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<Operation<TGeometry, TValue>> operation) where TGeometry : notnull =>
        ((typeof(TNative) == typeof(Curve) && GeometryKernel.CanCurveForm(type: typeof(TGeometry))) || typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue)
            ? Analyze.Cast<TGeometry, TOut>(key: key, operation: operation())
            : key.Unsupported<TGeometry, TOut>();
    public static Operation<TGeometry, TOut> AtMidpoint<TGeometry, TOut, TValue>(Op key, Func<Curve, double, TValue> project) where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when GeometryKernel.CanCurveForm(type: geometry) && output == typeof(TValue) => Analyze.Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, TValue>.Build(
            key: key, requirement: Requirement.CurveLength, requiresContext: true, state: (Key: key, Project: project),
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from curve in GeometryKernel.CurveForm(source: geometry, op: state.Key).ToEff()
                from result in curve.Use(native => native.NormalizedLengthParameter(s: 0.5, t: out double parameter, fractionalTolerance: runtime.Context.Fractional) switch {
                    true => state.Key.Accept(value: state.Project(arg1: native, arg2: parameter)),
                    false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                }).ToEff()
                select result)),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    public static Operation<TGeometry, TOut> Curvature<TGeometry, TOut>(int count, CurvatureMode mode) where TGeometry : notnull {
        Op key = Op.Of(name: "CurvatureAt");
        return (count, mode, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when GeometryKernel.CanCurveForm(type: geometry) && output == typeof(Vector3d) =>
                CurveCurvatureSamples<TGeometry, TOut, Vector3d>(key: key, count: count, project: CurveCurvatures),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when GeometryKernel.CanCurveForm(type: geometry) && output == typeof(Stat) =>
                CurveCurvatureSamples<TGeometry, TOut, Stat>(key: key, count: count, project: static (op, curve, n, ctx) =>
                    CurveMagnitudes(key: op, curve: curve, count: n, model: ctx).Bind(values =>
                        Stat.Curvature(values: values, metric: ScalarMetric.Magnitude, key: op).Map(static stat => Seq(stat)))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, Type geometry, Type output) when metric.Equals(ScalarMetric.Magnitude) && GeometryKernel.CanCurveForm(type: geometry) && output == typeof(double) =>
                CurveCurvatureSamples<TGeometry, TOut, double>(key: key, count: count, project: static (op, curve, n, ctx) =>
                    CurveMagnitudes(key: op, curve: curve, count: n, model: ctx).Bind(values => op.Accept(values: values))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, Type geometry, Type output) when metric.Equals(ScalarMetric.Magnitude) && GeometryKernel.CanCurveForm(type: geometry) && output == typeof(Stat) =>
                CurveCurvatureSamples<TGeometry, TOut, Stat>(key: key, count: count, project: (op, curve, n, ctx) =>
                    CurveMagnitudes(key: op, curve: curve, count: n, model: ctx).Bind(values =>
                        Stat.Curvature(values: values, metric: metric, key: op).Map(stat => Seq(stat)))),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) =>
                SurfaceCurvatureSamples<TGeometry, TOut, SurfaceCurvature>(key: key, count: count, project: SurfaceCurvatures),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Stat) =>
                SurfaceCurvatureSamples<TGeometry, TOut, Stat>(key: key, count: count, project: static (op, surface, n, ctx) =>
                    SurfaceCurvatures(key: op, surface: surface, resolution: n, model: ctx).Bind(curvatures =>
                        SurfaceStats(key: op, curvatures: curvatures))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, Type geometry, Type output) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                SurfaceCurvatureSamples<TGeometry, TOut, double>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvatures(key: op, surface: surface, resolution: n, model: ctx).Bind(curvatures =>
                        BorrowCurvatures(curvatures: curvatures, project: owned =>
                            SurfaceScalars(key: op, curvatures: owned, metric: metric).Bind(values => op.Accept(values: values))))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, Type geometry, Type output) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Stat) =>
                SurfaceCurvatureSamples<TGeometry, TOut, Stat>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvatures(key: op, surface: surface, resolution: n, model: ctx).Bind(curvatures =>
                        BorrowCurvatures(curvatures: curvatures, project: owned =>
                            SurfaceScalars(key: op, curvatures: owned, metric: metric).Bind(values =>
                                Stat.Curvature(values: values, metric: metric, key: op).Map(stat => Seq(stat)))))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    public static Operation<TGeometry, TOut> CurveCurvatureSamples<TGeometry, TOut, TValue>(Op key, int count, Func<Op, Curve, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Analyze.Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, TValue>.Build(
            key: key, requirement: Requirement.CurveLength, requiresContext: true, state: (Key: key, Count: count, Project: project),
            evaluator: static (state, geometry) => from context in Env.Asks
                                                   from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                                                       .Bind(lease => lease.Use(curve => state.Project(arg1: state.Key, arg2: curve, arg3: state.Count, arg4: context))).ToEff()
                                                   select result));
    public static Operation<TGeometry, TOut> SurfaceCurvatureSamples<TGeometry, TOut, TValue>(Op key, int count, Func<Op, Surface, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Analyze.Native<TGeometry, TOut, Surface, TValue, (Op Key, int Count, Func<Op, Surface, int, Context, Fin<Seq<TValue>>> Project)>(
            key: key, state: (Key: key, Count: count, Project: project), requirement: Requirement.SurfaceEvaluation, requiresContext: true,
            project: static (state, native) => from context in Env.Asks
                                               from result in state.Project(arg1: state.Key, arg2: native, arg3: state.Count, arg4: context).ToEff()
                                               select result);
    public static Operation<TGeometry, TOut> ClosestPoint<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid switch {
            false => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => Operation<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: point),
                evaluator: static (state, geometry) =>
                    from hit in GeometryKernel.ClosestOf(geometry: geometry, target: state.Target, key: state.Key).ToEff()
                    from result in (typeof(TOut) switch {
                        Type t when t == typeof(Point3d) => state.Key.AcceptResults<Point3d, TOut>(values: Seq(hit.Point)),
                        Type t when t == typeof(double) => state.Key.AcceptResults<double, TOut>(values: Seq(hit.Distance.IfNone(state.Target.DistanceTo(other: hit.Point)))),
                        Type t when t == typeof(Vector3d) => hit.Normal.ToFin(Fail: state.Key.InvalidResult()).Bind(n => state.Key.AcceptResults<Vector3d, TOut>(values: Seq(n))),
                        Type t when t == typeof(ComponentIndex) => hit.Component.ToFin(Fail: state.Key.InvalidResult()).Bind(c => state.Key.AcceptResults<ComponentIndex, TOut>(values: Seq(c))),
                        Type t when t == typeof(MeshPoint) => hit.MeshPoint.ToFin(Fail: state.Key.InvalidResult()).Bind(mp => state.Key.AcceptResults<MeshPoint, TOut>(values: Seq(mp))),
                        _ => Fin.Fail<Seq<TOut>>(error: state.Key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                    }).ToEff()
                    select result),
        };
    }
    public static Operation<TGeometry, TOut> ParameterAt<TGeometry, TOut>(Op key, Point3d probe) where TGeometry : notnull =>
        probe.IsValid switch {
            false => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when GeometryKernel.CanCurveForm(type: geometry) && output == typeof(double) =>
                    Analyze.Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, double>.Build(
                        key: key, state: (Key: key, Probe: probe),
                        evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key)
                            .Bind(lease => lease.Use(curve => curve.ClosestPoint(testPoint: state.Probe, t: out double parameter) switch {
                                true => state.Key.Accept(value: parameter),
                                false => Fin.Fail<Seq<double>>(state.Key.InvalidResult()),
                            })).ToEff())),
                (Type geometry, Type output) when (typeof(Surface).IsAssignableFrom(c: geometry) || geometry == typeof(object) || geometry == typeof(GeometryBase)) && output == typeof(Point2d) =>
                    Analyze.Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, Point2d>.Build(
                        key: key, state: (Key: key, Probe: probe),
                        evaluator: static (state, geometry) => geometry switch {
                            Surface surface => (surface.ClosestPoint(testPoint: state.Probe, u: out double u, v: out double v) switch {
                                true => state.Key.Accept(value: new Point2d(x: u, y: v)),
                                false => Fin.Fail<Seq<Point2d>>(state.Key.InvalidResult()),
                            }).ToEff(),
                            _ => Fin.Fail<Seq<Point2d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point2d))).ToEff(),
                        })),
                _ => key.Unsupported<TGeometry, TOut>(),
            },
        };
    public static Operation<TGeometry, TOut> CurveAt<TGeometry, TOut>(Op key, double parameter, Func<Curve, double, Fin<Seq<TOut>>> project, Requirement? requirement = null) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: requirement ?? Requirement.Basic, state: (Key: key, Parameter: parameter, Project: project),
            evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key)
                .Bind(lease => lease.Use(curve => curve.Domain.IncludesParameter(t: state.Parameter) switch {
                    true => state.Project(arg1: curve, arg2: state.Parameter),
                    false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
                })).ToEff());
    public static Operation<TGeometry, Point3d> DividePoly<TGeometry>(Op key, Requirement? requirement, Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Operation<TGeometry, Point3d>.Build(
            key: key, requirement: requirement, state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key)
                .Bind(lease => lease.Use(curve => state.Divide(arg: curve)
                    .ToFin(state.Key.InvalidResult())
                    .Bind(points => state.Key.Accept(values: points)))).ToEff());
    public static Operation<TGeometry, Curve> ShortPath<TGeometry>(Op key, Point2d start, Point2d end) where TGeometry : notnull =>
        Operation<TGeometry, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation,
            state: (Key: key, Start: start, End: end),
            evaluator: static (state, geometry) => geometry switch {
                Surface surface => from context in Env.Asks
                                   from uvStart in GeometryKernel.SurfaceUv(surface: surface, uv: state.Start, context: context, key: state.Key).ToEff()
                                   from uvEnd in GeometryKernel.SurfaceUv(surface: surface, uv: state.End, context: context, key: state.Key).ToEff()
                                   from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: context.Absolute.Value))
                                       .ToFin(state.Key.InvalidResult())
                                       .ToEff()
                                   select Seq(path),
                _ => Fin.Fail<Seq<Curve>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Curve))).ToEff(),
            });
    public static Operation<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(Op key, Point2d uv, Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Analyze.Native<TGeometry, TOut, Surface, TOut, (Op Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project)>(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Uv: uv, Project: project),
            project: static (state, surface) => from context in Env.Asks
                                                from parameter in GeometryKernel.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key).ToEff()
                                                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                                                select result);
    public static Fin<Seq<double>> SurfaceScalars(Op key, Seq<SurfaceCurvature> curvatures, ScalarMetric metric) =>
        metric switch {
            ScalarMetric active when active.Equals(ScalarMetric.Gaussian) => Fin.Succ(curvatures.Map(static c => c.Gaussian)),
            ScalarMetric active when active.Equals(ScalarMetric.Mean) => Fin.Succ(curvatures.Map(static c => c.Mean)),
            _ => Fin.Fail<Seq<double>>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    public static Fin<Seq<Stat>> SurfaceStats(Op key, Seq<SurfaceCurvature> curvatures) =>
        BorrowCurvatures(curvatures: curvatures, project: owned =>
            (SurfaceScalars(key: key, curvatures: owned, metric: ScalarMetric.Gaussian).Bind(values => Stat.Curvature(values: values, metric: ScalarMetric.Gaussian, key: key)),
             SurfaceScalars(key: key, curvatures: owned, metric: ScalarMetric.Mean).Bind(values => Stat.Curvature(values: values, metric: ScalarMetric.Mean, key: key)))
            .Apply(static (gaussian, mean) => Seq(gaussian, mean)).As());
    public static Fin<T> BorrowCurvatures<T>(Seq<SurfaceCurvature> curvatures, Func<Seq<SurfaceCurvature>, Fin<T>> project) {
        Fin<T> result = project(arg: curvatures);
        _ = curvatures.Iter(static curvature => curvature.Dispose());
        return result;
    }
    public static Fin<Seq<Vector3d>> CurveCurvatures(Op key, Curve curve, int count, Context model) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: model, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    public static Fin<Seq<double>> CurveMagnitudes(Op key, Curve curve, int count, Context model) =>
        CurveCurvatures(key: key, curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    public static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Op key, Surface surface, int resolution, Context model) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, resolution: resolution, context: model, key: key)
            .Bind(samples => samples.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult())).As());
}
