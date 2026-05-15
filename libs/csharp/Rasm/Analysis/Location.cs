namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Location : IAspect {
    public sealed record Midpoint : Location; public sealed record Tangent : Location; public sealed record Closest(Point3d Point) : Location;
    public sealed record PointAtCurve(double Parameter) : Location; public sealed record PointAtSurface(Point2d Uv) : Location; public sealed record PointAtLength(double Length) : Location;
    public sealed record FrameAtCurve(double Parameter) : Location; public sealed record FrameAtSurface(Point2d Uv) : Location; public sealed record PerpendicularFrameAt(double Parameter) : Location;
    public sealed record NormalAt(Point2d Uv) : Location; public sealed record CurvatureAtCurve(double Parameter) : Location; public sealed record CurvatureAtSurface(Point2d Uv) : Location;
    public sealed record Curvature(int Count, StatKind Kind) : Location; public sealed record DerivativeAt(double Parameter, int Count) : Location;
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
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch<global::Rasm.Analysis.Operation<TGeometry, TOut>>(
        midpoint: static _ => Analyze.Mid<TGeometry, TOut>(),
        tangent: static _ => Analyze.TangentAtMiddle<TGeometry, TOut>(),
        closest: static c => Analyze.Closest<TGeometry, TOut>(point: c.Point),
        curvature: static cp => Analyze.Curvature<TGeometry, TOut>(count: cp.Count, kind: cp.Kind),
        pointAtCurve: static pac => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, operation: () => Analyze.CurveAt<TGeometry, Point3d>(key: PointAtKey, parameter: pac.Parameter, project: static (curve, p) => PointAtKey.Accept(value: curve.PointAt(t: p)))),
        pointAtLength: static pal => Analyze.Located<TGeometry, TOut, Curve, Point3d>(
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
        frameAtCurve: static fac => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, operation: () => Analyze.CurveFrame<TGeometry>(key: FrameAtKey, parameter: fac.Parameter, perpendicular: false)),
        perpendicularFrameAt: static pfa => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, operation: () => Analyze.CurveFrame<TGeometry>(key: PerpendicularFrameAtKey, parameter: pfa.Parameter, perpendicular: true)),
        curvatureAtCurve: static cac => Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, operation: () => Analyze.CurveAt<TGeometry, Vector3d>(key: CurvatureAtKey, parameter: cac.Parameter, project: static (curve, p) => CurvatureAtKey.Accept(value: curve.CurvatureAt(t: p)))),
        derivativeAt: static da => da.Count < 0
            ? global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: DerivativeAtKey, fault: DerivativeAtKey.InvalidInput())
            : Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, operation: () => Analyze.CurveAt<TGeometry, Vector3d>(key: DerivativeAtKey, parameter: da.Parameter, project: (curve, p) => DerivativeAtKey.Accept(values: curve.DerivativeAt(t: p, derivativeCount: da.Count)))),
        divideByCount: static dbc => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, operation: () => Analyze.DividePoly<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: dbc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        divideByLength: static dbl => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, operation: () => Analyze.DividePoly<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: dbl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
        orientation: static o => Analyze.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) => geometry switch {
                Curve curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame)).ToEff(),
                _ => Fin.Fail<Seq<CurveOrientation>>(state.Key.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(CurveOrientation))).ToEff(),
            })),
        contains: static cnt => Analyze.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, operation: () => global::Rasm.Analysis.Operation<TGeometry, PointContainment>.Build(
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
        pointAtSurface: static pas => Analyze.Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Point3d>(key: PointAtKey, uv: pas.Uv, project: static (geometry, parameter) => PointAtKey.Accept(value: geometry.PointAt(u: parameter.X, v: parameter.Y)))),
        frameAtSurface: static fas => Analyze.Located<TGeometry, TOut, Surface, Plane>(key: FrameAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Plane>(
            key: FrameAtKey, uv: fas.Uv, project: static (geometry, parameter) => geometry.FrameAt(u: parameter.X, v: parameter.Y, frame: out Plane frame) switch {
                true => FrameAtKey.Accept(value: frame),
                false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
            })),
        normalAt: static na => Analyze.Located<TGeometry, TOut, Surface, Vector3d>(key: NormalAtKey, operation: () => Analyze.SurfaceUv<TGeometry, Vector3d>(
            key: NormalAtKey, uv: na.Uv, project: static (geometry, parameter) => geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                Vector3d normal when normal.IsValid && !normal.IsTiny() => NormalAtKey.Accept(value: normal),
                _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
            })),
        curvatureAtSurface: static cas => Analyze.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, operation: () => Analyze.SurfaceUv<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: cas.Uv, project: static (geometry, parameter) => Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
        shortPath: static sp => Analyze.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, operation: () => Analyze.ShortPath<TGeometry>(start: sp.Start, end: sp.End)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Location<TGeometry, TOut>(Location aspect) where TGeometry : notnull => Aspect<Location, TGeometry, TOut>(aspect: aspect);
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<global::Rasm.Analysis.Operation<TGeometry, TValue>> operation) where TGeometry : notnull =>
        (typeof(TNative).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue)
            ? Cast<TGeometry, TOut>(key: key, operation: operation())
            : key.Unsupported<TGeometry, TOut>();
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Mid<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Point3d>(key: Op.Of(name: "Midpoint"), line: static line => line.PointAt(t: 0.5), curve: static (curve, parameter) => curve.PointAt(t: parameter));
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> TangentAtMiddle<TGeometry, TOut>() where TGeometry : notnull =>
        Middle<TGeometry, TOut, Vector3d>(key: Op.Of(name: "Tangent"), line: static line => line.UnitTangent, curve: static (curve, parameter) => curve.TangentAt(t: parameter));
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Middle<TGeometry, TOut, TValue>(Op key, Func<Line, TValue> line, Func<Curve, double, TValue> curve) where TGeometry : notnull => (typeof(TGeometry), typeof(TOut)) switch {
        (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<Line, TValue>.Build(
                key: key, state: (Key: key, Project: line), evaluator: static (state, geometry) => state.Key.AcceptValue(value: geometry).Bind(validated => state.Key.Accept(value: state.Project(arg: validated))).ToEff())),
        (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<Polyline, TValue>.Build(
                key: key, state: (Key: key, Project: curve), evaluator: static (state, geometry) => Bracket(factory: geometry.ToPolylineCurve, body: polyCurve => polyCurve.NormalizedLengthParameter(s: 0.5, t: out double parameter) switch {
                    true => state.Key.Accept(value: state.Project(arg1: polyCurve, arg2: parameter)),
                    false => Fin.Fail<Seq<TValue>>(state.Key.InvalidResult()),
                }).ToEff())),
        (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(TValue) => Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, TValue>.Build(
                key: key, requirement: Requirement.CurveLength, state: (Key: key, Project: curve), evaluator: static (state, geometry) => CurveAtNormalized<TGeometry, TValue>(geometry: geometry, key: state.Key, project: state.Project))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Curvature<TGeometry, TOut>(int count, StatKind kind) where TGeometry : notnull {
        Op key = Op.Of(name: "CurvatureAt");
        return (count, kind, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _) => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, StatKind active, Type geometry, Type output) when active.Equals(StatKind.Curvature) && typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) => CurvatureOperation<TGeometry, TOut, Curve, Vector3d>(key: key, requirement: Requirement.CurveLength, count: count, project: static (op, curve, sampleCount, context) => CurveCurvatures(key: op, curve: curve, count: sampleCount, model: context)),
            (_, StatKind active, Type geometry, Type output) when active.Equals(StatKind.Curvature) && typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Stat) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, kind: StatKind.Magnitude, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, StatKind active, Type geometry, Type output) when active.Equals(StatKind.Magnitude) && typeof(Curve).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(Stat)) => ScalarCurvature<TGeometry, TOut, Curve>(key: key, requirement: Requirement.CurveLength, count: count, kind: active, project: static (op, curve, sampleCount, context) => CurveMagnitudes(key: op, curve: curve, count: sampleCount, model: context)),
            (_, StatKind active, Type geometry, Type output) when active.Equals(StatKind.Curvature) && typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(SurfaceCurvature) => CurvatureOperation<TGeometry, TOut, Surface, SurfaceCurvature>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context)),
            (_, StatKind active, Type geometry, Type output) when active.Equals(StatKind.Curvature) && typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Stat) => CurvatureOperation<TGeometry, TOut, Surface, Stat>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, project: static (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => SurfaceStats(key: op, curvatures: curvatures))),
            (_, StatKind active, Type geometry, Type output) when (active.Equals(StatKind.Gaussian) || active.Equals(StatKind.Mean)) && typeof(Surface).IsAssignableFrom(c: geometry) && (output == typeof(double) || output == typeof(Stat)) => ScalarCurvature<TGeometry, TOut, Surface>(key: key, requirement: Requirement.SurfaceEvaluation, count: count, kind: active, project: (op, surface, sampleCount, context) => SurfaceCurvatures(key: op, surface: surface, resolution: sampleCount, model: context).Bind(curvatures => BorrowCurvatures(curvatures: curvatures, project: owned => SurfaceScalars(key: op, curvatures: owned, kind: active)))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static global::Rasm.Analysis.Operation<TGeometry, TOut> ScalarCurvature<TGeometry, TOut, TNative>(Op key, Requirement requirement, int count, StatKind kind, Func<Op, TNative, int, Context, Fin<Seq<double>>> project) where TGeometry : notnull where TNative : notnull => typeof(TOut) switch {
        Type output when output == typeof(double) => CurvatureOperation<TGeometry, TOut, TNative, double>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => op.Accept(values: values))),
        Type output when output == typeof(Stat) => CurvatureOperation<TGeometry, TOut, TNative, Stat>(key: key, requirement: requirement, count: count, project: (op, native, sampleCount, context) => project(arg1: op, arg2: native, arg3: sampleCount, arg4: context).Bind(values => Stat.Curvature(values: values, kind: kind, key: op).Map(static stat => Seq(stat)))),
        _ => key.Unsupported<TGeometry, TOut>(),
    };
    private static global::Rasm.Analysis.Operation<TGeometry, TOut> CurvatureOperation<TGeometry, TOut, TNative, TValue>(Op key, Requirement requirement, int count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TNative : notnull =>
        Native<TGeometry, TOut, TNative, TValue, (Op Key, int Count, Func<Op, TNative, int, Context, Fin<Seq<TValue>>> Project)>(
            key: key, state: (Key: key, Count: count, Project: project), requirement: requirement, requiresContext: true,
            project: static (state, native) => from context in Env.Asks
                                               from result in state.Project(arg1: state.Key, arg2: native, arg3: state.Count, arg4: context).ToEff()
                                               select result);
    private static Fin<Seq<double>> SurfaceScalars(Op key, Seq<SurfaceCurvature> curvatures, StatKind kind) =>
        kind switch {
            StatKind active when active.Equals(StatKind.Gaussian) => Fin.Succ(toSeq(curvatures.AsIterable().Select(static curvature => curvature.Gaussian).ToArray())),
            StatKind active when active.Equals(StatKind.Mean) => Fin.Succ(toSeq(curvatures.AsIterable().Select(static curvature => curvature.Mean).ToArray())),
            _ => Fin.Fail<Seq<double>>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Stat>> SurfaceStats(Op key, Seq<SurfaceCurvature> curvatures) =>
        BorrowCurvatures(curvatures: curvatures, project: owned =>
            (SurfaceScalars(key: key, curvatures: owned, kind: StatKind.Gaussian).Bind(values => Stat.Curvature(values: values, kind: StatKind.Gaussian, key: key)),
             SurfaceScalars(key: key, curvatures: owned, kind: StatKind.Mean).Bind(values => Stat.Curvature(values: values, kind: StatKind.Mean, key: key)))
            .Apply(static (gaussian, mean) => Seq(gaussian, mean)).As());
    private static Fin<T> BorrowCurvatures<T>(Seq<SurfaceCurvature> curvatures, Func<Seq<SurfaceCurvature>, Fin<T>> project) {
        Fin<T> result = project(arg: curvatures);
        _ = curvatures.Iter(static curvature => curvature.Dispose());
        return result;
    }
    private static Fin<Seq<Vector3d>> CurveCurvatures(Op key, Curve curve, int count, Context model) =>
        CurveSamples(curve: curve, count: count, model: model, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudes(Op key, Curve curve, int count, Context model) =>
        CurveCurvatures(key: key, curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvatures(Op key, Surface surface, int resolution, Context model) =>
        (Samples(domain: surface.Domain(direction: 0), resolution: resolution, key: key),
         Samples(domain: surface.Domain(direction: 1), resolution: resolution, key: key))
        .Apply(static (u, v) => (U: u, V: v)).As()
        .Bind(samples => samples.U
            .Bind(u => samples.V.Map(v => new Point2d(x: u, y: v)))
            .TraverseM(uv => Uv(surface: surface, uv: uv, context: model, key: key)
                .Bind(parameter => Optional(surface.CurvatureAt(u: parameter.X, v: parameter.Y)).ToFin(key.InvalidResult())))
            .As());
    private static Fin<Seq<double>> CurveSamples(Curve curve, int count, Context model, Op key) =>
        GeometryKernel.Fractions(count: count, op: key)
                .Bind(fractions => Optional(curve.NormalizedLengthParameters(s: [.. fractions.AsIterable()], absoluteTolerance: model.Absolute.Value, fractionalTolerance: model.Fractional))
                .ToFin(key.InvalidResult())
                .Map(static parameters => toSeq(parameters)));
    internal static Fin<Seq<double>> Samples(Interval domain, int resolution, Op key) =>
        domain.IsValid switch {
            true => GeometryKernel.Fractions(count: resolution, op: key).Map(fractions => fractions.Map(f => domain.ParameterAt(f))),
            false => Fin.Fail<Seq<double>>(key.InvalidInput()),
        };
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid switch {
            false => global::Rasm.Analysis.Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            true => global::Rasm.Analysis.Operation<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: point), requiresContext: true,
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from hit in ClosestOf(geometry: geometry, target: state.Target, context: context, op: state.Key).ToEff()
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
    internal static Fin<ClosestHit> ClosestOf<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : notnull =>
        (Optional(geometry).ToFin(op.InvalidInput()), target.IsValid) switch {
            (_, false) => Fin.Fail<ClosestHit>(op.InvalidInput()),
            (Fin<TGeometry> source, true) => source.Bind(g => g switch {
                Line line => Fin.Succ(new ClosestHit(Point: line.ClosestPoint(testPoint: target, limitToFiniteSegment: true), Distance: Some(target.DistanceTo(other: line.ClosestPoint(testPoint: target, limitToFiniteSegment: true))), Normal: None, Component: None, MeshPoint: None)),
                Polyline polyline => Fin.Succ(new ClosestHit(Point: polyline.ClosestPoint(testPoint: target), Distance: Some(target.DistanceTo(other: polyline.ClosestPoint(testPoint: target))), Normal: None, Component: None, MeshPoint: None)),
                Curve curve => curve.ClosestPoint(testPoint: target, t: out double parameter) ? Fin.Succ(new ClosestHit(Point: curve.PointAt(t: parameter), Distance: Some(target.DistanceTo(other: curve.PointAt(t: parameter))), Normal: None, Component: None, MeshPoint: None)) : Fin.Fail<ClosestHit>(op.InvalidInput()),
                Surface surface => surface.ClosestPoint(testPoint: target, u: out double u, v: out double v) ? Fin.Succ(new ClosestHit(Point: surface.PointAt(u: u, v: v), Distance: Some(target.DistanceTo(other: surface.PointAt(u: u, v: v))), Normal: Some(surface.NormalAt(u: u, v: v)), Component: None, MeshPoint: None)) : Fin.Fail<ClosestHit>(op.InvalidInput()),
                Brep brep => brep.ClosestPoint(target, out Point3d point, out ComponentIndex component, out double _, out double _, 0.0, out Vector3d normal) ? Fin.Succ(new ClosestHit(Point: point, Distance: Some(target.DistanceTo(other: point)), Normal: Some(normal), Component: Some(component), MeshPoint: None)) : Fin.Fail<ClosestHit>(op.InvalidInput()),
                Mesh mesh => Optional(mesh.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0)).ToFin(op.InvalidResult()).Map(meshPoint => new ClosestHit(Point: meshPoint.Point, Distance: Some(target.DistanceTo(other: meshPoint.Point)), Normal: Some(mesh.NormalAt(meshPoint: meshPoint)), Component: None, MeshPoint: Some(meshPoint))),
                _ => Fin.Fail<ClosestHit>(op.Unsupported(g.GetType(), typeof(ClosestHit))),
            }),
        };
    internal static global::Rasm.Analysis.Operation<TGeometry, Plane> CurveFrame<TGeometry>(Op key, double parameter, bool perpendicular) where TGeometry : notnull =>
        CurveAt<TGeometry, Plane>(key: key, parameter: parameter, project: (curve, t) => perpendicular switch {
            true => key.AcceptSolved(isSolved: curve.PerpendicularFrameAt(t: t, plane: out Plane perpendicularFrame), value: perpendicularFrame),
            false => key.AcceptSolved(isSolved: curve.FrameAt(t: t, plane: out Plane frame), value: frame),
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
                                   from uvStart in Uv(surface: surface, uv: state.Start, context: context, key: state.Key).ToEff()
                                   from uvEnd in Uv(surface: surface, uv: state.End, context: context, key: state.Key).ToEff()
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
                                                from parameter in Uv(surface: surface, uv: state.Uv, context: context, key: state.Key).ToEff()
                                                from result in state.Project(arg1: surface, arg2: parameter).ToEff()
                                                select result);
    private static Fin<Point2d> Uv(Surface surface, Point2d uv, Context context, Op key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y) && (surface is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
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
