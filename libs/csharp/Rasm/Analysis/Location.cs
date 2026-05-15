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
    public sealed record PerpendicularFrameAtCase(double Parameter) : Location;
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
    public static Location PerpendicularFrameAt(double parameter) => new PerpendicularFrameAtCase(Parameter: parameter);
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
    private static readonly Op ParameterAtKey = Op.Of(name: "ParameterAt");
    private static readonly Op LengthAtKey = Op.Of(name: "LengthAt");
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        midpointCase: static _ => Analyze.MidpointAt<TGeometry, TOut>(),
        tangentCase: static _ => Analyze.TangentAtMidpoint<TGeometry, TOut>(),
        closestCase: static c => Analyze.ClosestPoint<TGeometry, TOut>(point: c.Point),
        curvatureCase: static cp => Analyze.Curvature<TGeometry, TOut>(count: cp.Count, mode: cp.Mode),
        pointAtCurveCase: static pac => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, operation: () => Analyze.CurveAt<TGeometry, Point3d>(key: PointAtKey, parameter: pac.Parameter, project: static (curve, p) => PointAtKey.Accept(value: curve.PointAt(t: p)))),
        pointAtLengthCase: static pal => Analyze.Located<TGeometry, TOut, Curve, Point3d>(
            key: PointAtLengthKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
                key: PointAtLengthKey, requirement: Requirement.CurveLength, state: (Key: PointAtLengthKey, Distance: pal.Length),
                evaluator: static (state, geometry) => geometry switch {
                    Curve curve => from context in Env.Asks
                                   from result in (curve.LengthParameter(segmentLength: state.Distance, t: out double parameter, fractionalTolerance: context.Fractional) switch {
                                       true => state.Key.Accept(value: curve.PointAt(t: parameter)),
                                       false => Fin.Fail<Seq<Point3d>>(state.Key.InvalidResult()),
                                   }).ToEff()
                                   select result,
                    _ => Fin.Fail<Seq<Point3d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))).ToEff(),
                })),
        frameAtCurveCase: static fac => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, operation: () => Analyze.CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
        perpendicularFrameAtCase: static pfa => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, operation: () => Analyze.CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
        curvatureAtCurveCase: static cac => Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, operation: () => Analyze.CurveAt<TGeometry, Vector3d>(key: CurvatureAtKey, parameter: cac.Parameter, project: static (curve, p) => CurvatureAtKey.Accept(value: curve.CurvatureAt(t: p)))),
        derivativeAtCase: static da => da.Count < 0
            ? global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: DerivativeAtKey, fault: DerivativeAtKey.InvalidInput())
            : Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, operation: () => Analyze.CurveAt<TGeometry, Vector3d>(key: DerivativeAtKey, parameter: da.Parameter, project: (curve, p) => DerivativeAtKey.Accept(values: curve.DerivativeAt(t: p, derivativeCount: da.Count)))),
        divideByCountCase: static dbc => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, operation: () => Analyze.DividePoly<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        divideByLengthCase: static dbl => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, operation: () => Analyze.DividePoly<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        orientationCase: static o => Analyze.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) => geometry switch {
                Curve curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame)).ToEff(),
                _ => Fin.Fail<Seq<CurveOrientation>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
            })),
        containsCase: static cnt => Analyze.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, PointContainment>.Build(
            key: ContainsKey, requiresContext: true, state: (Key: ContainsKey, Probe: cnt.Point, Frame: cnt.Plane),
            evaluator: static (state, geometry) => geometry switch {
                Curve curve => from context in Env.Asks
                               from result in (curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                                   PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                                   PointContainment containment => state.Key.Accept(value: containment),
                               }).ToEff()
                               select result,
                _ => Fin.Fail<Seq<PointContainment>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(PointContainment))).ToEff(),
            })),
        pointAtSurfaceCase: static pas => Analyze.Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Point3d>(key: PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => PointAtKey.Accept(value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
        frameAtSurfaceCase: static fas => Analyze.Located<TGeometry, TOut, Surface, Plane>(key: FrameAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Plane>(
            key: FrameAtKey, uv: fas.Uv, project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                true => FrameAtKey.Accept(value: frame),
                false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
            })),
        normalAtCase: static na => Analyze.Located<TGeometry, TOut, Surface, Vector3d>(key: NormalAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Vector3d>(
            key: NormalAtKey, uv: na.Uv, project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                Vector3d normal when normal.IsValid && !normal.IsTiny() => NormalAtKey.Accept(value: normal),
                _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
            })),
        curvatureAtSurfaceCase: static cas => Analyze.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, operation: () => Analyze.SurfaceUv<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
        shortPathCase: static sp => Analyze.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, operation: () => Analyze.ShortPath<TGeometry>(start: sp.Start, end: sp.End)),
        parameterAtCase: static pat => Analyze.ParameterAt<TGeometry, TOut>(key: ParameterAtKey, probe: pat.Probe),
        lengthAtCase: static lat => Analyze.Located<TGeometry, TOut, Curve, double>(key: LengthAtKey, operation: () => Analyze.CurveAt<TGeometry, double>(key: LengthAtKey, parameter: lat.Parameter, project: static (curve, t) => LengthAtKey.Accept(value: curve.GetLength(subdomain: new Interval(curve.Domain.T0, t))))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Location<TGeometry, TOut>(Location aspect) where TGeometry : notnull => Aspect<Location, TGeometry, TOut>(aspect: aspect);
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<global::Rasm.Analysis.Operation<TGeometry, TValue>> operation) where TGeometry : notnull =>
        (typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue)
            ? Cast<TGeometry, TOut>(key: key, operation: operation())
            : key.Unsupported<TGeometry, TOut>();
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> MidpointAt<TGeometry, TOut>() where TGeometry : notnull =>
        AtMidpoint<TGeometry, TOut, Point3d>(key: Op.Of(name: "Midpoint"), line: static line => line.PointAt(t: 0.5), curve: static (curve, parameter) => curve.PointAt(t: parameter));
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> TangentAtMidpoint<TGeometry, TOut>() where TGeometry : notnull =>
        AtMidpoint<TGeometry, TOut, Vector3d>(key: Op.Of(name: "Tangent"), line: static line => line.UnitTangent, curve: static (curve, parameter) => curve.TangentAt(t: parameter));
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> AtMidpoint<TGeometry, TOut, TValue>(Op key, Func<Line, TValue> line, Func<Curve, double, TValue> curve) where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<Line, TValue>.Build(
                key: key, state: (Key: key, Project: line), evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff())),
        (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<Polyline, TValue>.Build(
                key: key, state: (Key: key, Project: curve), evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key).Bind(lease => lease.Use(polyCurve => polyCurve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                    true => state.Key.Accept(value: state.Project(arg1: polyCurve, arg2: parameter)),
                    false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                })).ToEff())),
        (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, TValue>.Build(
                key: key, requirement: Requirement.CurveLength, state: (Key: key, Project: curve), evaluator: static (state, geometry) => CurveAtNormalized<TGeometry, TValue>(geometry: geometry, key: state.Key, project: state.Project))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Curvature<TGeometry, TOut>(int count, CurvatureMode mode) where TGeometry : notnull {
        Op key = Op.Of(name: "CurvatureAt");
        return (count, mode, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => CurvatureOperation<TGeometry, TOut, Curve, Vector3d>(key: key, requirement: Requirement.CurveLength, count: count, project: static (op, curve, sampleCount, context) => CurveCurvatures(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Stat) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, metric: ScalarMetric.Magnitude, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureMode.ScalarCase { Metric: var active }, Type geometry, Type output) when active.Equals(ScalarMetric.Magnitude) && typeof(Curve).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(Stat)) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, metric: active, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) => CurvatureOperation<TGeometry, TOut, Surface, SurfaceCurvature>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context)),
            (_, CurvatureMode.VectorCase, Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Stat) => CurvatureOperation<TGeometry, TOut, Surface, Stat>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => SurfaceStats(key: op, curvatures: curvatures))),
            (_, CurvatureMode.ScalarCase { Metric: var active }, Type geometry, Type output) when (active.Equals(ScalarMetric.Gaussian) || active.Equals(ScalarMetric.Mean)) && typeof(Surface).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(Stat)) => ScalarCurvature<TGeometry, TOut, Surface>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, metric: active, project: (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => BorrowCurvatures(curvatures: curvatures, project: owned => SurfaceScalars(key: op, curvatures: owned, metric: active)))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static global::Rasm.Analysis.Operation<TGeometry, TOut> ScalarCurvature<TGeometry, TOut, TNative>(Op key, Requirement requirement, int count, ScalarMetric metric, Func<Op, TNative, int, Context, Fin<Seq<double>>> project) where TGeometry : notnull where TNative : notnull => typeof(TOut) switch {
        Type output when output == typeof(double) => CurvatureOperation<TGeometry, TOut, TNative, double>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => op.Accept(values: values))),
        Type output when output == typeof(Stat) => CurvatureOperation<TGeometry, TOut, TNative, Stat>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => Stat.Curvature(values: values, metric: metric, key: op).Map(static stat => Seq(stat)))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    private static global::Rasm.Analysis.Operation<TGeometry, TOut> CurvatureOperation<TGeometry, TOut, TNative, TValue>(Op key, Requirement requirement, int count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, (Op Key, int Count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> Project)>(
            key: key, state: (Key: key, Count: count, Project: project), requirement: requirement, requiresContext: true,
            project: static (state, native) => from context in Env.Asks
                                               from result in state.Project(arg1: state.Key, arg2: native, arg3: state.Count, arg4: context).ToEff()
                                               select result);
    private static Fin<Seq<double>> SurfaceScalars(Op key, Seq<SurfaceCurvature> curvatures, ScalarMetric metric) =>
        metric switch {
            ScalarMetric active when active.Equals(ScalarMetric.Gaussian) => Fin.Succ(toSeq(curvatures.AsIterable().Select(static curvature => curvature.Gaussian).ToArray())),
            ScalarMetric active when active.Equals(ScalarMetric.Mean) => Fin.Succ(toSeq(curvatures.AsIterable().Select(static curvature => curvature.Mean).ToArray())),
            _ => Fin.Fail<Seq<double>>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Stat>> SurfaceStats(Op key, Seq<SurfaceCurvature> curvatures) =>
        BorrowCurvatures(curvatures: curvatures, project: owned =>
            (SurfaceScalars(key: key, curvatures: owned, metric: ScalarMetric.Gaussian).Bind(values => Stat.Curvature(values: values, metric: ScalarMetric.Gaussian, key: key)),
             SurfaceScalars(key: key, curvatures: owned, metric: ScalarMetric.Mean).Bind(values => Stat.Curvature(values: values, metric: ScalarMetric.Mean, key: key)))
            .Apply(static (gaussian, mean) => Seq(gaussian, mean)).As());
    private static Fin<T> BorrowCurvatures<T>(Seq<SurfaceCurvature> curvatures, Func<Seq<SurfaceCurvature>, Fin<T>> project) {
        Fin<T> result = project(arg: curvatures);
        _ = curvatures.Iter(static curvature => curvature.Dispose());
        return result;
    }
    private static Fin<Seq<Vector3d>> CurveCurvatures(Op key, Curve curve, int count, Context model) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: model, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudes(Op key, Curve curve, int count, Context model) =>
        CurveCurvatures(key: key, curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Op key, Surface surface, int resolution, Context model) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, resolution: resolution, context: model, key: key)
            .Bind(samples => samples.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult()))
            .As());
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> ParameterAt<TGeometry, TOut>(Op key, Point3d probe) where TGeometry : notnull =>
        probe.IsValid switch {
            false => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => (typeof(TGeometry), typeof(TOut)) switch {
                (Type geometry, Type output) when (typeof(Curve).IsAssignableFrom(c: geometry) || geometry == typeof(object) || geometry == typeof(GeometryBase)) && output == typeof(double) =>
                    Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, double>.Build(
                        key: key, state: (Key: key, Probe: probe),
                        evaluator: static (state, geometry) => geometry switch {
                            Curve curve => (curve.ClosestPoint(testPoint: state.Probe, t: out double parameter) switch {
                                true => state.Key.Accept(value: parameter),
                                false => Fin.Fail<Seq<double>>(state.Key.InvalidResult()),
                            }).ToEff(),
                            _ => Fin.Fail<Seq<double>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))).ToEff(),
                        })),
                (Type geometry, Type output) when (typeof(Surface).IsAssignableFrom(c: geometry) || geometry == typeof(object) || geometry == typeof(GeometryBase)) && output == typeof(Point2d) =>
                    Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, Point2d>.Build(
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
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> ClosestPoint<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid switch {
            false => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => global::Rasm.Analysis.Operation<TGeometry, TOut>.Build(
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
    internal static global::Rasm.Analysis.Operation<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(key: key, parameter: parameter, project: (curve, t) => perpendicular switch {
            true => curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame) ? key.Accept(value: perpendicularFrame) : Fin.Fail<Seq<Plane>>(key.InvalidResult()),
            false => curve.FrameAt(t: t, plane: out Plane frame) ? key.Accept(value: frame) : Fin.Fail<Seq<Plane>>(key.InvalidResult()),
        });
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> CurveAt<TGeometry, TOut>(Op key, double parameter, Func<Curve, double, Fin<Seq<TOut>>> project, Requirement? requirement = null) where TGeometry : notnull =>
        Native<TGeometry, TOut, Curve, TOut, (Op Key, double Parameter, Func<Curve, double, Fin<Seq<TOut>>> Project)>(
            key: key, requirement: requirement ?? Requirement.Basic, state: (Key: key, Parameter: parameter, Project: project),
            project: static (state, curve) => (curve.Domain.IncludesParameter(t: state.Parameter) switch {
                true => state.Project(arg1: curve, arg2: state.Parameter),
                false => Fin.Fail<Seq<TOut>>(state.Key.InvalidInput()),
            }).ToEff());
    internal static global::Rasm.Analysis.Operation<TGeometry, Point3d> DividePoly<TGeometry>(Op key, Requirement? requirement, Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        global::Rasm.Analysis.Operation<TGeometry, Point3d>.Build(
            key: key, requirement: requirement, state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => (geometry switch {
                Curve curve => state.Divide(arg: curve)
                    .ToFin(state.Key.InvalidResult())
                    .Bind(points => state.Key.Accept(values: points)),
                _ => Fin.Fail<Seq<Point3d>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
            }).ToEff());
    internal static global::Rasm.Analysis.Operation<TGeometry, Curve> ShortPath<TGeometry>(Point2d start, Point2d end) where TGeometry : notnull {
        Op key = Op.Of();
        return global::Rasm.Analysis.Operation<TGeometry, Curve>.Build(
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
    }
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> SurfaceUv<TGeometry, TOut>(Op key, Point2d uv, Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Native<TGeometry, TOut, Surface, TOut, (Op Key, Point2d Uv, Func<Surface, Point2d, Fin<Seq<TOut>>> Project)>(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Uv: uv, Project: project),
            project: static (state, surface) => from context in Env.Asks
                                                from parameter in GeometryKernel.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key).ToEff()
                                                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                                                select result);
    internal static Eff<Env, Seq<TOut>> CurveAtNormalized<TGeometry, TOut>(TGeometry geometry, Op key, Func<Curve, double, TOut> project) where TGeometry : notnull =>
        geometry switch {
            Curve curve => from runtime in Env.EnvAsks
                           from parameter in (curve.NormalizedLengthParameter(s: 0.5, t: out double p, fractionalTolerance: runtime.Context.Fractional) switch {
                               true => Fin.Succ(p),
                               false => Fin.Fail<double>(key.InvalidResult()),
                           }).ToEff()
                           from result in key.Accept(value: project(arg1: curve, arg2: parameter)).ToEff()
                           select result,
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: geometry.GetType(), outputType: typeof(TOut))).ToEff(),
        };
}
