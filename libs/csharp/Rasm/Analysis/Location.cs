namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record Locator {
    public sealed record CurveParameter(double T) : Locator; public sealed record SurfaceParameter(Point2d Uv) : Locator; public sealed record ArcLength(double Distance) : Locator;
    public sealed record ClosestTo(Point3d Probe) : Locator; public sealed record NormalizedMid : Locator; public sealed record PerpendicularParameters(Seq<double> Ts) : Locator;
}

[Union]
public partial record Division {
    public sealed record ByCount(int Count) : Division; public sealed record ByLength(double Length) : Division;
}

[Union]
public partial record RegionQuery {
    public sealed record Contains(Point3d Probe, Plane Frame) : RegionQuery; public sealed record ShortPath(Point2d Start, Point2d End) : RegionQuery;
    public sealed record LengthAt(double Parameter) : RegionQuery;
}

[Union]
internal partial record CurvatureAggregation {
    public sealed record SamplesCase : CurvatureAggregation;
    public sealed record ExtremaCase(ExtremumDirection Direction) : CurvatureAggregation;
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Location : IAspect {
    public sealed record PointAtCase(Locator At) : Location; public sealed record FrameAtCase(Locator At) : Location; public sealed record CurvatureAtCase(Locator At) : Location;
    public sealed record CurvatureSamplesCase(int Count, CurvatureMode Mode) : Location; public sealed record CurvatureExtremaCase(int Count, CurvatureMode Mode, ExtremumDirection Direction) : Location; public sealed record DerivativeAtCase(Locator At, int Order) : Location;
    public sealed record ParameterAtCase(Point3d Probe) : Location;
    public sealed record DivideCase(Division By) : Location; public sealed record OrientationCase(Plane Plane) : Location; public sealed record RegionQueryCase(RegionQuery Query) : Location;
    public static Location Midpoint => new PointAtCase(At: new Locator.NormalizedMid());
    public static Location Tangent => new FrameAtCase(At: new Locator.NormalizedMid());
    public static Location Closest(Point3d point) => new PointAtCase(At: new Locator.ClosestTo(Probe: point));
    public static Location ParameterAt(Point3d point) => new ParameterAtCase(Probe: point);
    public static Location PointAtCurve(double parameter) => new PointAtCase(At: new Locator.CurveParameter(T: parameter));
    public static Location PointAtSurface(Point2d uv) => new PointAtCase(At: new Locator.SurfaceParameter(Uv: uv));
    public static Location PointAtLength(double length) => new PointAtCase(At: new Locator.ArcLength(Distance: length));
    public static Location FrameAtCurve(double parameter) => new FrameAtCase(At: new Locator.CurveParameter(T: parameter));
    public static Location FrameAtSurface(Point2d uv) => new FrameAtCase(At: new Locator.SurfaceParameter(Uv: uv));
    public static Location PerpendicularFrameAt(params double[] parameters) => new FrameAtCase(At: new Locator.PerpendicularParameters(Ts: toSeq(parameters)));
    public static Location NormalAt(Point2d uv) => new FrameAtCase(At: new Locator.SurfaceParameter(Uv: uv));
    public static Location CurvatureAtCurve(double parameter) => new CurvatureAtCase(At: new Locator.CurveParameter(T: parameter));
    public static Location CurvatureAtSurface(Point2d uv) => new CurvatureAtCase(At: new Locator.SurfaceParameter(Uv: uv));
    public static Location Curvature(int count, CurvatureMode mode) => new CurvatureSamplesCase(Count: count, Mode: mode);
    public static Location CurvatureExtrema(int count, CurvatureMode mode, ExtremumDirection direction) => new CurvatureExtremaCase(Count: count, Mode: mode, Direction: direction);
    public static Location DerivativeAt(double parameter, int count) => new DerivativeAtCase(At: new Locator.CurveParameter(T: parameter), Order: count);
    public static Location DivideByCount(int count) => new DivideCase(By: new Division.ByCount(Count: count));
    public static Location DivideByLength(double length) => new DivideCase(By: new Division.ByLength(Length: length));
    public static Location Orientation(Plane plane) => new OrientationCase(Plane: plane);
    public static Location Contains(Point3d point, Plane plane) => new RegionQueryCase(Query: new RegionQuery.Contains(Probe: point, Frame: plane));
    public static Location ShortPath(Point2d start, Point2d end) => new RegionQueryCase(Query: new RegionQuery.ShortPath(Start: start, End: end));
    public static Location LengthAt(double parameter) => new RegionQueryCase(Query: new RegionQuery.LengthAt(Parameter: parameter));
    internal static readonly Op MidpointKey = Op.Of(name: "Midpoint"); internal static readonly Op TangentKey = Op.Of(name: "Tangent"); internal static readonly Op PointAtKey = Op.Of(name: "PointAt"); internal static readonly Op PointAtLengthKey = Op.Of(name: "PointAtLength");
    internal static readonly Op ClosestKey = Op.Of(name: "Closest"); internal static readonly Op ParameterAtKey = Op.Of(name: "ParameterAt");
    internal static readonly Op FrameAtKey = Op.Of(name: "FrameAt"); internal static readonly Op PerpendicularFrameAtKey = Op.Of(name: "PerpendicularFrameAt"); internal static readonly Op CurvatureAtKey = Op.Of(name: "CurvatureAt"); internal static readonly Op DerivativeAtKey = Op.Of(name: "DerivativeAt");
    internal static readonly Op DivideByCountKey = Op.Of(name: "DivideByCount"); internal static readonly Op DivideByLengthKey = Op.Of(name: "DivideByLength"); internal static readonly Op OrientationKey = Op.Of(name: "Orientation"); internal static readonly Op ContainsKey = Op.Of(name: "Contains");
    internal static readonly Op NormalAtKey = Op.Of(name: "NormalAt"); internal static readonly Op ShortPathKey = Op.Of(name: "ShortPath"); internal static readonly Op LengthAtKey = Op.Of(name: "LengthAt");
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        pointAtCase: static p => p.At.Switch<Operation<TGeometry, TOut>>(
            curveParameter: static cp => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Point3d>(key: PointAtKey, locator: cp, project: static (curve, t, _) => PointAtKey.Accept(value: curve.PointAt(t: t)))),
            surfaceParameter: static sp => Analyze.Located<TGeometry, TOut, Surface, Point3d>(key: PointAtKey, operation: () => Analyze.SurfaceUvOp<TGeometry, Point3d>(key: PointAtKey, uv: sp.Uv, project: static (surface, p) => PointAtKey.Accept(value: surface.PointAt(u: p.X, v: p.Y)))),
            arcLength: static al => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: PointAtLengthKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Point3d>(key: PointAtLengthKey, locator: al, project: static (curve, t, _) => PointAtLengthKey.Accept(value: curve.PointAt(t: t)), requirement: Requirement.CurveLength)),
            closestTo: static ct => Analyze.ClosestOp<TGeometry, TOut>(key: ClosestKey, target: ct.Probe, canProject: static t => ClosestHit.CanProjectTo(output: t), project: static (hit, key) => hit.Project<TOut>(key: key)),
            normalizedMid: static mid => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: MidpointKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Point3d>(key: MidpointKey, locator: mid, project: static (curve, parameter, _) => MidpointKey.Accept(value: curve.PointAt(t: parameter)), requirement: Requirement.CurveLength)),
            perpendicularParameters: static _ => PointAtKey.Unsupported<TGeometry, TOut>()),
        frameAtCase: static f => f.At.Switch<Operation<TGeometry, TOut>>(
            curveParameter: static cp => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: FrameAtKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Plane>(key: FrameAtKey, locator: cp, project: static (curve, t, _) => curve.FrameAt(t: t, plane: out Plane frame) ? FrameAtKey.Accept(value: frame) : Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()))),
            surfaceParameter: static sp => typeof(TOut) == typeof(Vector3d)
                ? Analyze.Located<TGeometry, TOut, Surface, Vector3d>(key: NormalAtKey, operation: () => Analyze.SurfaceUvOp<TGeometry, Vector3d>(key: NormalAtKey, uv: sp.Uv, project: static (surface, p) => GeometryKernel.NormalAt(surface: surface, uv: p, key: NormalAtKey).Bind(normal => NormalAtKey.Accept(value: normal))))
                : Analyze.Located<TGeometry, TOut, Surface, Plane>(key: FrameAtKey, operation: () => Analyze.SurfaceUvOp<TGeometry, Plane>(key: FrameAtKey, uv: sp.Uv, project: static (surface, p) => GeometryKernel.FrameAt(surface: surface, uv: p, key: FrameAtKey).Bind(frame => FrameAtKey.Accept(value: frame)))),
            arcLength: static _ => FrameAtKey.Unsupported<TGeometry, TOut>(),
            closestTo: static _ => FrameAtKey.Unsupported<TGeometry, TOut>(),
            normalizedMid: static mid => Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: TangentKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Vector3d>(key: TangentKey, locator: mid, project: static (curve, parameter, _) => TangentKey.Accept(value: curve.TangentAt(t: parameter)), requirement: Requirement.CurveLength)),
            perpendicularParameters: static ps => Analyze.Located<TGeometry, TOut, Curve, Plane>(key: PerpendicularFrameAtKey, operation: () => Analyze.PerpendicularFrameOp<TGeometry>(key: PerpendicularFrameAtKey, parameters: ps.Ts))),
        curvatureAtCase: static c => c.At.Switch<Operation<TGeometry, TOut>>(
            curveParameter: static cp => Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: CurvatureAtKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Vector3d>(key: CurvatureAtKey, locator: cp, project: static (curve, t, _) => CurvatureAtKey.Accept(value: curve.CurvatureAt(t: t)))),
            surfaceParameter: static sp => Analyze.Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: CurvatureAtKey, operation: () => Analyze.SurfaceUvOp<TGeometry, SurfaceCurvature>(key: CurvatureAtKey, uv: sp.Uv, project: static (surface, p) => Optional(surface.CurvatureAt(u: p.X, v: p.Y)).ToFin(CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
            arcLength: static _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
            closestTo: static _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
            normalizedMid: static _ => CurvatureAtKey.Unsupported<TGeometry, TOut>(),
            perpendicularParameters: static _ => CurvatureAtKey.Unsupported<TGeometry, TOut>()),
        curvatureSamplesCase: static cs => Analyze.CurvatureOp<TGeometry, TOut>(count: cs.Count, mode: cs.Mode, agg: new CurvatureAggregation.SamplesCase()),
        curvatureExtremaCase: static ce => Analyze.CurvatureOp<TGeometry, TOut>(count: ce.Count, mode: ce.Mode, agg: new CurvatureAggregation.ExtremaCase(Direction: ce.Direction)),
        derivativeAtCase: static d => (d.Order >= 0 && d.At is Locator.CurveParameter cp)
            ? Analyze.Located<TGeometry, TOut, Curve, Vector3d>(key: DerivativeAtKey, operation: () => Analyze.CurveLocatedOp<TGeometry, Vector3d>(key: DerivativeAtKey, locator: cp, project: (curve, p, _) => DerivativeAtKey.Accept(values: curve.DerivativeAt(t: p, derivativeCount: d.Order))))
            : Analysis.Operation<TGeometry, TOut>.Reject(key: DerivativeAtKey, fault: DerivativeAtKey.InvalidInput()),
        parameterAtCase: static p => Analyze.ClosestOp<TGeometry, TOut>(key: ParameterAtKey, target: p.Probe, canProject: static t => ClosestHit.CanProjectTo(output: t, parameterMode: true), project: static (hit, key) => hit.Project<TOut>(key: key, parameterMode: true)),
        divideCase: static d => d.By.Switch<Operation<TGeometry, TOut>>(
            byCount: static bc => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, operation: () => Analyze.DividePolyOp<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: bc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
            byLength: static bl => Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, operation: () => Analyze.DividePolyOp<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: bl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None }))),
        orientationCase: static o => Analyze.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, operation: () => Analysis.Operation<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key).Bind(lease => lease.Use(curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame)))).ToEff())),
        regionQueryCase: static r => r.Query.Switch<Operation<TGeometry, TOut>>(
            contains: static cnt => Analyze.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, operation: () => Analysis.Operation<TGeometry, PointContainment>.Build(
                key: ContainsKey, requiresContext: true, state: (Key: ContainsKey, cnt.Probe, cnt.Frame),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                        .Bind(lease => lease.Use(curve => curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                            PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                            PointContainment containment => state.Key.Accept(value: containment),
                        })).ToEff()
                    select result)),
            shortPath: static sp => Analyze.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, operation: () => Analyze.ShortPathOp<TGeometry>(key: ShortPathKey, start: sp.Start, end: sp.End)),
            lengthAt: static la => Analyze.Located<TGeometry, TOut, Curve, double>(key: LengthAtKey, operation: () => Analyze.CurveLocatedOp<TGeometry, double>(key: LengthAtKey, locator: new Locator.CurveParameter(T: la.Parameter), project: static (curve, t, context) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(t0: curve.Domain.T0, t1: t)) switch {
                double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => LengthAtKey.Accept(value: length),
                _ => Fin.Fail<Seq<double>>(LengthAtKey.InvalidResult()),
            }, requirement: Requirement.CurveLength))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Location<TGeometry, TOut>(Location aspect) where TGeometry : notnull => Aspect<Location, TGeometry, TOut>(aspect: aspect);
    internal static Operation<TGeometry, TOut> ClosestOp<TGeometry, TOut>(Op key, Point3d target, Func<Type, bool> canProject, Func<ClosestHit, Op, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        (target.IsValid, canProject(arg: typeof(TOut)), GeometryKernel.CanClosest(type: typeof(TGeometry))) switch {
            (false, _, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (true, true, true) => Operation<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: target, Project: project),
                evaluator: static (state, geometry) =>
                    from hit in GeometryKernel.ClosestOf(geometry: geometry, target: state.Target, key: state.Key).ToEff()
                    from result in state.Project(arg1: hit, arg2: state.Key).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    internal static Operation<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<Operation<TGeometry, TValue>> operation) where TGeometry : notnull =>
        (((typeof(TNative) == typeof(Curve) && GeometryKernel.CanCurveForm(type: typeof(TGeometry)))
            || (typeof(TNative) == typeof(Surface) && GeometryKernel.CanSurfaceForm(type: typeof(TGeometry)))
            || typeof(TNative).IsAssignableFrom(c: typeof(TGeometry))
            || typeof(TGeometry) == typeof(object)
            || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue))
            ? operation().As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    internal static Operation<TGeometry, TOut> CurveLocatedOp<TGeometry, TOut>(Op key, Locator locator, Func<Curve, double, Context, Fin<Seq<TOut>>> project, Requirement? requirement = null) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: requirement ?? Requirement.Basic, state: (Key: key, Locator: locator, Project: project),
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => (state.Locator switch {
                        Locator.CurveParameter { T: double parameter } => curve.Domain.IncludesParameter(t: parameter) ? Fin.Succ(parameter) : Fin.Fail<double>(state.Key.InvalidInput()),
                        Locator.NormalizedMid => curve.NormalizedLengthParameter(s: 0.5, t: out double parameter, fractionalTolerance: runtime.Context.Fractional) ? Fin.Succ(parameter) : Fin.Fail<double>(state.Key.InvalidResult()),
                        Locator.ArcLength { Distance: double distance } => curve.LengthParameter(segmentLength: distance, t: out double parameter, fractionalTolerance: runtime.Context.Fractional) ? Fin.Succ(parameter) : Fin.Fail<double>(state.Key.InvalidResult()),
                        _ => Fin.Fail<double>(state.Key.InvalidInput()),
                    }).Bind(parameter => state.Project(arg1: curve, arg2: parameter, arg3: runtime.Context)))).ToEff()
                select result);
    internal static Operation<TGeometry, TOut> SurfaceUvOp<TGeometry, TOut>(Op key, Point2d uv, Func<Surface, Point2d, Fin<Seq<TOut>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Uv: uv, Project: project),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in GeometryKernel.SurfaceForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(surface => GeometryKernel.SurfaceUv(surface: surface, uv: state.Uv, context: context, key: state.Key)
                        .Bind(parameter => state.Project(arg1: surface, arg2: parameter)))).ToEff()
                select result);
    internal static Operation<TGeometry, Plane> PerpendicularFrameOp<TGeometry>(Op key, Seq<double> parameters) where TGeometry : notnull =>
        Operation<TGeometry, Plane>.Build(
            key: key, requirement: Requirement.CurveLength, state: (Key: key, Parameters: parameters),
            evaluator: static (state, geometry) =>
                GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => Optional(curve.GetPerpendicularFrames(toSeq(state.Parameters.AsIterable().OrderBy(t => t).Distinct()).AsIterable()))
                        .ToFin(state.Key.InvalidResult())
                        .Bind(planes => state.Key.Accept(values: planes))))
                    .ToEff());
    internal static Operation<TGeometry, Point3d> DividePolyOp<TGeometry>(Op key, Requirement? requirement, Func<Curve, Option<Point3d[]>> divide) where TGeometry : notnull =>
        Operation<TGeometry, Point3d>.Build(
            key: key, requirement: requirement, state: (Key: key, Divide: divide),
            evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key)
                .Bind(lease => lease.Use(curve => state.Divide(arg: curve).ToFin(state.Key.InvalidResult()).Bind(points => state.Key.Accept(values: points)))).ToEff());
    internal static Operation<TGeometry, Curve> ShortPathOp<TGeometry>(Op key, Point2d start, Point2d end) where TGeometry : notnull =>
        Operation<TGeometry, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Start: start, End: end),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in GeometryKernel.SurfaceForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(surface =>
                        from uvStart in GeometryKernel.SurfaceUv(surface: surface, uv: state.Start, context: context, key: state.Key)
                        from uvEnd in GeometryKernel.SurfaceUv(surface: surface, uv: state.End, context: context, key: state.Key)
                        from path in Optional(surface.ShortPath(start: uvStart, end: uvEnd, tolerance: context.Absolute.Value)).ToFin(state.Key.InvalidResult())
                        select Seq(path))).ToEff()
                select result);
    internal static Operation<TGeometry, TOut> CurvatureOp<TGeometry, TOut>(int count, CurvatureMode mode, CurvatureAggregation agg) where TGeometry : notnull {
        Op key = Op.Of(name: agg is CurvatureAggregation.ExtremaCase ? "CurvatureExtrema" : "CurvatureAt");
        return (count, mode, agg, typeof(TGeometry), typeof(TOut)) switch {
            ( <= 0, _, _, _, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, CurvatureMode.VectorCase, CurvatureAggregation.SamplesCase, Type g, Type o) when GeometryKernel.CanCurveForm(type: g) && o == typeof(Vector3d) =>
                CurveCurvatureSamplesOp<TGeometry, TOut, Vector3d>(key: key, count: count, project: CurveCurvaturesOf),
            (_, CurvatureMode m, CurvatureAggregation.SamplesCase, Type g, Type o) when (m is CurvatureMode.VectorCase || (m is CurvatureMode.ScalarCase sc && sc.Metric.Equals(ScalarMetric.Magnitude))) && GeometryKernel.CanCurveForm(type: g) && o == typeof(Stat) =>
                CurveCurvatureSamplesOp<TGeometry, TOut, Stat>(key: key, count: count, project: static (op, curve, n, ctx) =>
                    CurveMagnitudesOf(key: op, curve: curve, count: n, model: ctx).Bind(values => Stat.Of(values: values, key: op, context: StatContext.Metric(metric: ScalarMetric.Magnitude)).Map(static stat => Seq(stat)))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, CurvatureAggregation.SamplesCase, Type g, Type o) when metric.Equals(ScalarMetric.Magnitude) && GeometryKernel.CanCurveForm(type: g) && o == typeof(double) =>
                CurveCurvatureSamplesOp<TGeometry, TOut, double>(key: key, count: count, project: static (op, curve, n, ctx) =>
                    CurveMagnitudesOf(key: op, curve: curve, count: n, model: ctx).Bind(values => op.Accept(values: values))),
            (_, CurvatureMode.VectorCase, CurvatureAggregation.SamplesCase, Type g, Type o) when GeometryKernel.CanSurfaceForm(type: g) && o == typeof(SurfaceCurvature) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, SurfaceCurvature>(key: key, count: count, project: SurfaceCurvaturesOf),
            (_, CurvatureMode.VectorCase, CurvatureAggregation.SamplesCase, Type g, Type o) when GeometryKernel.CanSurfaceForm(type: g) && o == typeof(Stat) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, Stat>(key: key, count: count, project: static (op, surface, n, ctx) =>
                    SurfaceCurvaturesOf(key: op, surface: surface, count: n, model: ctx).Bind(curvatures => SurfaceStatsOf(key: op, curvatures: curvatures))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, CurvatureAggregation.SamplesCase, Type g, Type o) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && GeometryKernel.CanSurfaceForm(type: g) && o == typeof(double) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, double>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvaturesOf(key: op, surface: surface, count: n, model: ctx).Bind(curvatures =>
                        BorrowCurvaturesOf(curvatures: curvatures, project: owned => SurfaceScalarsOf(key: op, curvatures: owned, metric: metric).Bind(values => op.Accept(values: values))))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, CurvatureAggregation.SamplesCase, Type g, Type o) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && GeometryKernel.CanSurfaceForm(type: g) && o == typeof(Stat) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, Stat>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvaturesOf(key: op, surface: surface, count: n, model: ctx).Bind(curvatures =>
                        BorrowCurvaturesOf(curvatures: curvatures, project: owned => SurfaceScalarsOf(key: op, curvatures: owned, metric: metric).Bind(values => Stat.Of(values: values, key: op, context: StatContext.Metric(metric: metric)).Map(stat => Seq(stat)))))),
            (_, CurvatureMode m, CurvatureAggregation.ExtremaCase ex, Type g, Type o) when (m is CurvatureMode.VectorCase || (m is CurvatureMode.ScalarCase sc && sc.Metric.Equals(ScalarMetric.Magnitude))) && GeometryKernel.CanCurveForm(type: g) && o == typeof(Point3d) =>
                CurveCurvatureSamplesOp<TGeometry, TOut, Point3d>(key: key, count: count, project: (op, curve, n, ctx) =>
                    CurveCurvatureSamples(op: op, curve: curve, count: n, ctx: ctx).Bind(samples => op.Accept(values: Stat.Extrema(items: samples, projection: static s => s.Curvature, tolerance: 0.0, direction: ex.Direction).Map(static h => h.Point)))),
            (_, CurvatureMode m, CurvatureAggregation.ExtremaCase ex, Type g, Type o) when (m is CurvatureMode.VectorCase || (m is CurvatureMode.ScalarCase sc && sc.Metric.Equals(ScalarMetric.Magnitude))) && GeometryKernel.CanCurveForm(type: g) && o == typeof(double) =>
                CurveCurvatureSamplesOp<TGeometry, TOut, double>(key: key, count: count, project: (op, curve, n, ctx) =>
                    CurveCurvatureSamples(op: op, curve: curve, count: n, ctx: ctx).Bind(samples => op.Accept(values: Stat.Extrema(items: samples, projection: static s => s.Curvature, tolerance: 0.0, direction: ex.Direction).Map(static h => h.Curvature)))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, CurvatureAggregation.ExtremaCase ex, Type g, Type o) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && GeometryKernel.CanSurfaceForm(type: g) && o == typeof(Point3d) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, Point3d>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvatureSamples(op: op, surface: surface, count: n, ctx: ctx, metric: metric).Bind(samples => op.Accept(values: Stat.Extrema(items: samples, projection: static s => s.Curvature, tolerance: 0.0, direction: ex.Direction).Map(static h => h.Point)))),
            (_, CurvatureMode.ScalarCase { Metric: var metric }, CurvatureAggregation.ExtremaCase ex, Type g, Type o) when (metric.Equals(ScalarMetric.Gaussian) || metric.Equals(ScalarMetric.Mean)) && GeometryKernel.CanSurfaceForm(type: g) && o == typeof(double) =>
                SurfaceCurvatureSamplesOp<TGeometry, TOut, double>(key: key, count: count, project: (op, surface, n, ctx) =>
                    SurfaceCurvatureSamples(op: op, surface: surface, count: n, ctx: ctx, metric: metric).Bind(samples => op.Accept(values: Stat.Extrema(items: samples, projection: static s => s.Curvature, tolerance: 0.0, direction: ex.Direction).Map(static h => h.Curvature)))),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    }
    private static Operation<TGeometry, TOut> CurveCurvatureSamplesOp<TGeometry, TOut, TValue>(Op key, int count, Func<Op, Curve, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, requirement: Requirement.CurveLength, state: (Key: key, Count: count, Project: project),
            evaluator: static (state, geometry) => from context in Env.Asks
                                                   from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                                                       .Bind(lease => lease.Use(curve => state.Project(arg1: state.Key, arg2: curve, arg3: state.Count, arg4: context))).ToEff()
                                                   select result).As<TGeometry, TOut>(key: key);
    private static Operation<TGeometry, TOut> SurfaceCurvatureSamplesOp<TGeometry, TOut, TValue>(Op key, int count, Func<Op, Surface, int, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Count: count, Project: project), requirement: Requirement.SurfaceEvaluation,
            evaluator: static (state, geometry) => from context in Env.Asks
                                                   from result in GeometryKernel.SurfaceForm(source: geometry, op: state.Key)
                                                       .Bind(lease => lease.Use(surface => state.Project(arg1: state.Key, arg2: surface, arg3: state.Count, arg4: context))).ToEff()
                                                   select result).As<TGeometry, TOut>(key: key);
    private static Fin<Seq<double>> SurfaceScalarsOf(Op key, Seq<SurfaceCurvature> curvatures, ScalarMetric metric) =>
        metric switch {
            ScalarMetric active when active.Equals(ScalarMetric.Gaussian) => Fin.Succ(curvatures.Map(static c => c.Gaussian)),
            ScalarMetric active when active.Equals(ScalarMetric.Mean) => Fin.Succ(curvatures.Map(static c => c.Mean)),
            _ => Fin.Fail<Seq<double>>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(double))),
        };
    private static Fin<Seq<Stat>> SurfaceStatsOf(Op key, Seq<SurfaceCurvature> curvatures) =>
        BorrowCurvaturesOf(curvatures: curvatures, project: owned =>
            (SurfaceScalarsOf(key: key, curvatures: owned, metric: ScalarMetric.Gaussian).Bind(values => Stat.Of(values: values, key: key, context: StatContext.Metric(metric: ScalarMetric.Gaussian))),
             SurfaceScalarsOf(key: key, curvatures: owned, metric: ScalarMetric.Mean).Bind(values => Stat.Of(values: values, key: key, context: StatContext.Metric(metric: ScalarMetric.Mean))))
            .Apply(static (gaussian, mean) => Seq(gaussian, mean)).As());
    private static Fin<T> BorrowCurvaturesOf<T>(Seq<SurfaceCurvature> curvatures, Func<Seq<SurfaceCurvature>, Fin<T>> project) {
        Fin<T> result = project(arg: curvatures);
        _ = curvatures.Iter(static curvature => curvature.Dispose());
        return result;
    }
    private static Fin<Seq<Vector3d>> CurveCurvaturesOf(Op key, Curve curve, int count, Context model) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: model, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudesOf(Op key, Curve curve, int count, Context model) =>
        CurveCurvaturesOf(key: key, curve: curve, count: count, model: model).Map(static vectors => vectors.Map(static v => v.Length));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvaturesOf(Op key, Surface surface, int count, Context model) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, count: count, context: model, key: key)
            .Bind(samples => samples.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult())).As());
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CurvatureSample(Point3d Point, double Curvature);
    private static Fin<Seq<CurvatureSample>> CurveCurvatureSamples(Op op, Curve curve, int count, Context ctx) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: ctx, key: op)
            .Map(parameters => parameters.Map(t => new CurvatureSample(Point: curve.PointAt(t: t), Curvature: curve.CurvatureAt(t: t).Length)));
    private static Fin<Seq<CurvatureSample>> SurfaceCurvatureSamples(Op op, Surface surface, int count, Context ctx, ScalarMetric metric) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, count: count, context: ctx, key: op)
            .Bind(uvs => uvs.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(op.InvalidResult())
                .Map(curvature => new Lease<SurfaceCurvature>.Owned(Value: curvature)
                    .Use(c => new CurvatureSample(Point: surface.PointAt(u: uv.X, v: uv.Y), Curvature: metric.Equals(ScalarMetric.Gaussian) ? c.Gaussian : c.Mean)))).As());
}
