using Rasm.Vectors;
using LocationAspect = Rasm.Analysis.Location;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record Locator {
    public sealed record CurveParameter(double T) : Locator;
    public sealed record ArcLength(double Distance) : Locator;
    public sealed record NormalizedMid : Locator;
    public sealed record SurfaceParameter(Point2d Uv) : Locator;
    public sealed record ClosestTo(Point3d Probe) : Locator;
    public sealed record PerpendicularParameters(Seq<double> Ts) : Locator;
}

[SkipUnionOps]
[Union]
public partial record LocationValue {
    public sealed record PointCase : LocationValue;
    public sealed record FrameCase : LocationValue;
    public sealed record NormalCase : LocationValue;
    public sealed record TangentCase : LocationValue;
    public sealed record CurvatureCase : LocationValue;
    public sealed record DerivativeCase(int Order) : LocationValue;
    public sealed record ParameterCase : LocationValue;
    public sealed record LengthCase : LocationValue;
    public static LocationValue Point => new PointCase();
    public static LocationValue Frame => new FrameCase();
    public static LocationValue Normal => new NormalCase();
    public static LocationValue Tangent => new TangentCase();
    public static LocationValue Curvature => new CurvatureCase();
    public static LocationValue Derivative(int order) => new DerivativeCase(Order: order);
    public static LocationValue Parameter => new ParameterCase();
    public static LocationValue Length => new LengthCase();
}

[SkipUnionOps]
[Union]
public partial record Division {
    public sealed record ByCount(int Count) : Division;
    public sealed record ByLength(double Length) : Division;
}

[SkipUnionOps]
[Union]
internal partial record CurvatureAggregation {
    public sealed record SamplesCase : CurvatureAggregation;
    public sealed record ExtremaCase(ExtremumDirection Direction) : CurvatureAggregation;
}

[SkipUnionOps]
[Union]
public partial record Location : IAspect {
    public sealed record AtCase(Locator Locator, LocationValue Value) : LocationAspect;
    public sealed record CurvatureSamplesCase(int Count, CurvatureMode Mode) : LocationAspect;
    public sealed record CurvatureExtremaCase(int Count, CurvatureMode Mode, ExtremumDirection Direction) : LocationAspect;
    public sealed record DivideCase(Division By) : LocationAspect;
    public sealed record OrientationCase(Plane Plane) : LocationAspect;
    public sealed record ContainsCase(Point3d Probe, Plane Frame) : LocationAspect;
    public sealed record ShortPathCase(Point2d Start, Point2d End) : LocationAspect;
    internal static readonly Op TangentKey = Op.Of(name: "Tangent");
    internal static readonly Op PointAtKey = Op.Of(name: "PointAt");
    internal static readonly Op ClosestKey = Op.Of(name: "Closest");
    internal static readonly Op ParameterAtKey = Op.Of(name: "ParameterAt");
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
    internal static readonly Op LengthAtKey = Op.Of(name: "LengthAt");
    public static LocationAspect At(Locator at, LocationValue value) => new AtCase(Locator: at, Value: value);
    public static LocationAspect Curvature(int count, CurvatureMode mode) => new CurvatureSamplesCase(Count: count, Mode: mode);
    public static LocationAspect CurvatureExtrema(int count, CurvatureMode mode, ExtremumDirection direction) => new CurvatureExtremaCase(Count: count, Mode: mode, Direction: direction);
    public static LocationAspect DivideByCount(int count) => new DivideCase(By: new Division.ByCount(Count: count));
    public static LocationAspect DivideByLength(double length) => new DivideCase(By: new Division.ByLength(Length: length));
    public static LocationAspect Orientation(Plane plane) => new OrientationCase(Plane: plane);
    public static LocationAspect Contains(Point3d point, Plane plane) => new ContainsCase(Probe: point, Frame: plane);
    public static LocationAspect ShortPath(Point2d start, Point2d end) => new ShortPathCase(Start: start, End: end);
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        atCase: static at => Analyze.LocatedValue<TGeometry, TOut>(locator: at.Locator, value: at.Value),
        curvatureSamplesCase: static cs => Analyze.CurvatureOp<TGeometry, TOut>(count: cs.Count, mode: cs.Mode, agg: new CurvatureAggregation.SamplesCase()),
        curvatureExtremaCase: static ce => Analyze.CurvatureOp<TGeometry, TOut>(count: ce.Count, mode: ce.Mode, agg: new CurvatureAggregation.ExtremaCase(Direction: ce.Direction)),
        divideCase: static d => d.By.Switch(
            byCount: static bc => bc.Count <= 0
                ? Analysis.Operation<TGeometry, TOut>.Reject(key: DivideByCountKey, fault: DivideByCountKey.InvalidInput())
                : Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByCountKey, operation: () => Analyze.DividePolyOp<TGeometry>(key: DivideByCountKey, requirement: null, divide: curve => curve.DivideByCount(segmentCount: bc.Count, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None })),
            byLength: static bl => !RhinoMath.IsValidDouble(x: bl.Length) || bl.Length <= RhinoMath.ZeroTolerance
                ? Analysis.Operation<TGeometry, TOut>.Reject(key: DivideByLengthKey, fault: DivideByLengthKey.InvalidInput())
                : Analyze.Located<TGeometry, TOut, Curve, Point3d>(key: DivideByLengthKey, operation: () => Analyze.DividePolyOp<TGeometry>(key: DivideByLengthKey, requirement: Requirement.CurveLength, divide: curve => curve.DivideByLength(segmentLength: bl.Length, includeEnds: true, points: out Point3d[] points) switch { double[] => Optional(points), _ => Option<Point3d[]>.None }))),
        orientationCase: static o => Analyze.Located<TGeometry, TOut, Curve, CurveOrientation>(key: OrientationKey, operation: () => Analysis.Operation<TGeometry, CurveOrientation>.Build(
            key: OrientationKey, state: (Key: OrientationKey, Frame: o.Plane),
            evaluator: static (state, geometry) => GeometryKernel.CurveForm(source: geometry, op: state.Key).Bind(lease => lease.Use(curve => state.Key.Accept(value: curve.ClosedCurveOrientation(plane: state.Frame)))).ToEff())),
        containsCase: static cnt => Analyze.Located<TGeometry, TOut, Curve, PointContainment>(key: ContainsKey, operation: () => Analysis.Operation<TGeometry, PointContainment>.Build(
            key: ContainsKey, requiresContext: true, state: (Key: ContainsKey, cnt.Probe, cnt.Frame),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => curve.Contains(testPoint: state.Probe, plane: state.Frame, tolerance: context.Absolute.Value) switch {
                        PointContainment.Unset => Fin.Fail<Seq<PointContainment>>(state.Key.InvalidResult()),
                        PointContainment containment => state.Key.Accept(value: containment),
                    })).ToEff()
                select result)),
        shortPathCase: static sp => Analyze.Located<TGeometry, TOut, Surface, Curve>(key: ShortPathKey, operation: () => Analyze.ShortPathOp<TGeometry>(key: ShortPathKey, start: sp.Start, end: sp.End)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Location<TGeometry, TOut>(LocationAspect aspect) where TGeometry : notnull => Aspect<LocationAspect, TGeometry, TOut>(aspect: aspect);
    internal static Operation<TGeometry, TOut> Located<TGeometry, TOut, TNative, TValue>(Op key, Func<Operation<TGeometry, TValue>> operation) where TGeometry : notnull =>
        (((typeof(TNative) == typeof(Curve) && GeometryKernel.CanCurveForm(type: typeof(TGeometry)))
            || (typeof(TNative) == typeof(Surface) && GeometryKernel.CanSurfaceForm(type: typeof(TGeometry)))
            || typeof(TNative).IsAssignableFrom(c: typeof(TGeometry))
            || typeof(TGeometry) == typeof(object)
            || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(TValue))
            ? operation().As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    internal static Operation<TGeometry, TOut> LocatedValue<TGeometry, TOut>(Locator locator, LocationValue value) where TGeometry : notnull =>
        (value, locator) switch {
            (LocationValue.PointCase, Locator.CurveParameter or Locator.ArcLength or Locator.NormalizedMid) =>
                Located<TGeometry, TOut, Curve, Point3d>(key: LocationAspect.PointAtKey, operation: () => CurveLocatedOp<TGeometry, Point3d>(key: LocationAspect.PointAtKey, locator: locator, project: static (curve, t, _) => LocationAspect.PointAtKey.Accept(value: curve.PointAt(t: t)))),
            (LocationValue.PointCase, Locator.SurfaceParameter sp) =>
                Located<TGeometry, TOut, Surface, Point3d>(key: LocationAspect.PointAtKey, operation: () => SurfaceUvOp<TGeometry, Point3d>(key: LocationAspect.PointAtKey, uv: sp.Uv, project: static (surface, p) => LocationAspect.PointAtKey.Accept(value: surface.PointAt(u: p.X, v: p.Y)))),
            (LocationValue.PointCase, Locator.ClosestTo ct) =>
                ClosestOp<TGeometry, TOut>(key: LocationAspect.ClosestKey, target: ct.Probe, projection: SupportProjection.Closest),
            (LocationValue.FrameCase, Locator.CurveParameter or Locator.ArcLength or Locator.NormalizedMid) =>
                Located<TGeometry, TOut, Curve, Plane>(key: LocationAspect.FrameAtKey, operation: () => CurveLocatedOp<TGeometry, Plane>(key: LocationAspect.FrameAtKey, locator: locator, project: static (curve, t, context) =>
                    VectorIntent.Curve(source: curve, parameter: t, mode: CurveProjection.Frame, key: LocationAspect.FrameAtKey).Bind(intent => intent.Project<Plane>(context: context, key: LocationAspect.FrameAtKey)).Bind(plane => LocationAspect.FrameAtKey.Accept(value: plane)))),
            (LocationValue.FrameCase, Locator.SurfaceParameter sp) =>
                Located<TGeometry, TOut, Surface, Plane>(key: LocationAspect.FrameAtKey, operation: () => SurfaceUvOp<TGeometry, Plane>(key: LocationAspect.FrameAtKey, uv: sp.Uv, project: static (surface, p) => GeometryKernel.FrameAt(surface: surface, uv: p, key: LocationAspect.FrameAtKey).Bind(frame => LocationAspect.FrameAtKey.Accept(value: frame)))),
            (LocationValue.FrameCase, Locator.ClosestTo ct) =>
                ClosestOp<TGeometry, TOut>(key: LocationAspect.FrameAtKey, target: ct.Probe, projection: SupportProjection.Frame),
            (LocationValue.FrameCase, Locator.PerpendicularParameters ps) =>
                Located<TGeometry, TOut, Curve, Plane>(key: LocationAspect.PerpendicularFrameAtKey, operation: () => PerpendicularFrameOp<TGeometry>(key: LocationAspect.PerpendicularFrameAtKey, parameters: ps.Ts)),
            (LocationValue.NormalCase, Locator.SurfaceParameter sp) =>
                Located<TGeometry, TOut, Surface, Vector3d>(key: LocationAspect.NormalAtKey, operation: () => SurfaceUvOp<TGeometry, Vector3d>(key: LocationAspect.NormalAtKey, uv: sp.Uv, project: static (surface, p) => GeometryKernel.NormalAt(surface: surface, uv: p, key: LocationAspect.NormalAtKey).Bind(normal => LocationAspect.NormalAtKey.Accept(value: normal)))),
            (LocationValue.NormalCase, Locator.ClosestTo ct) =>
                ClosestOp<TGeometry, TOut>(key: LocationAspect.NormalAtKey, target: ct.Probe, projection: SupportProjection.Normal),
            (LocationValue.TangentCase, Locator.CurveParameter or Locator.ArcLength or Locator.NormalizedMid) =>
                Located<TGeometry, TOut, Curve, Vector3d>(key: LocationAspect.TangentKey, operation: () => CurveLocatedOp<TGeometry, Vector3d>(key: LocationAspect.TangentKey, locator: locator, project: static (curve, parameter, context) =>
                    VectorIntent.Curve(source: curve, parameter: parameter, mode: CurveProjection.Tangent, key: LocationAspect.TangentKey).Bind(intent => intent.Project<Vector3d>(context: context, key: LocationAspect.TangentKey)).Bind(tangent => LocationAspect.TangentKey.Accept(value: tangent)))),
            (LocationValue.CurvatureCase, Locator.CurveParameter or Locator.ArcLength or Locator.NormalizedMid) =>
                Located<TGeometry, TOut, Curve, Vector3d>(key: LocationAspect.CurvatureAtKey, operation: () => CurveLocatedOp<TGeometry, Vector3d>(key: LocationAspect.CurvatureAtKey, locator: locator, project: static (curve, t, context) =>
                    VectorIntent.Curve(source: curve, parameter: t, mode: CurveProjection.Curvature, key: LocationAspect.CurvatureAtKey).Bind(intent => intent.Project<Vector3d>(context: context, key: LocationAspect.CurvatureAtKey)).Bind(curvature => LocationAspect.CurvatureAtKey.Accept(value: curvature)))),
            (LocationValue.CurvatureCase, Locator.SurfaceParameter sp) =>
                Located<TGeometry, TOut, Surface, SurfaceCurvature>(key: LocationAspect.CurvatureAtKey, operation: () => SurfaceUvOp<TGeometry, SurfaceCurvature>(key: LocationAspect.CurvatureAtKey, uv: sp.Uv, project: static (surface, p) => Optional(surface.CurvatureAt(u: p.X, v: p.Y)).ToFin(LocationAspect.CurvatureAtKey.InvalidResult()).Map(static curvature => Seq(curvature)))),
            (LocationValue.DerivativeCase { Order: >= 0 } derivative, Locator.CurveParameter or Locator.ArcLength or Locator.NormalizedMid) =>
                Located<TGeometry, TOut, Curve, Vector3d>(key: LocationAspect.DerivativeAtKey, operation: () => CurveLocatedOp<TGeometry, Vector3d>(key: LocationAspect.DerivativeAtKey, locator: locator, project: (curve, p, _) => Optional(curve.DerivativeAt(t: p, derivativeCount: derivative.Order)).Filter(derivatives => derivative.Order < derivatives.Length).ToFin(LocationAspect.DerivativeAtKey.InvalidResult()).Bind(derivatives => LocationAspect.DerivativeAtKey.Accept(value: derivatives[derivative.Order])))),
            (LocationValue.DerivativeCase, _) =>
                Operation<TGeometry, TOut>.Reject(key: LocationAspect.DerivativeAtKey, fault: LocationAspect.DerivativeAtKey.InvalidInput()),
            (LocationValue.ParameterCase, Locator.ClosestTo ct) =>
                ClosestOp<TGeometry, TOut>(key: LocationAspect.ParameterAtKey, target: ct.Probe, projection: SupportProjection.Parameter),
            (LocationValue.LengthCase, Locator.CurveParameter) =>
                Located<TGeometry, TOut, Curve, double>(key: LocationAspect.LengthAtKey, operation: () => CurveLocatedOp<TGeometry, double>(key: LocationAspect.LengthAtKey, locator: locator, project: static (curve, t, context) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(t0: curve.Domain.T0, t1: t)) switch {
                    double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => LocationAspect.LengthAtKey.Accept(value: length),
                    _ => Fin.Fail<Seq<double>>(LocationAspect.LengthAtKey.InvalidResult()),
                }, requirement: Requirement.CurveLength)),
            (LocationValue.PointCase, _) => LocationAspect.PointAtKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.FrameCase, _) => LocationAspect.FrameAtKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.NormalCase, _) => LocationAspect.NormalAtKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.TangentCase, _) => LocationAspect.TangentKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.CurvatureCase, _) => LocationAspect.CurvatureAtKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.ParameterCase, _) => LocationAspect.ParameterAtKey.Unsupported<TGeometry, TOut>(),
            (LocationValue.LengthCase, _) => LocationAspect.LengthAtKey.Unsupported<TGeometry, TOut>(),
            _ => LocationAspect.PointAtKey.Unsupported<TGeometry, TOut>(),
        };
    internal static Operation<TGeometry, TOut> CurveLocatedOp<TGeometry, TOut>(Op key, Locator locator, Func<Curve, double, Context, Fin<Seq<TOut>>> project, Requirement? requirement = null) where TGeometry : notnull =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: requirement ?? (locator switch { Locator.ArcLength or Locator.NormalizedMid => Requirement.CurveLength, _ => Requirement.Basic }), state: (Key: key, Locator: locator, Project: project),
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from result in GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => (state.Locator switch {
                        Locator.CurveParameter { T: double parameter } => guard(curve.Domain.IncludesParameter(t: parameter), state.Key.InvalidInput()).ToFin().Map(_ => parameter),
                        Locator.NormalizedMid => guard(curve.NormalizedLengthParameter(s: 0.5, t: out double parameter, fractionalTolerance: runtime.Context.Fractional), state.Key.InvalidResult()).ToFin().Map(_ => parameter),
                        Locator.ArcLength { Distance: double distance } => guard(curve.LengthParameter(segmentLength: distance, t: out double parameter, fractionalTolerance: runtime.Context.Fractional), state.Key.InvalidResult()).ToFin().Map(_ => parameter),
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
    internal static Operation<TGeometry, TOut> ClosestOp<TGeometry, TOut>(Op key, Point3d target, SupportProjection projection) where TGeometry : notnull =>
        (target.IsValid, GeometryKernel.CanClosest(type: typeof(TGeometry))) switch {
            (false, _) => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            (true, true) => Operation<TGeometry, TOut>.Build(
                key: key, state: (Key: key, Target: target, Projection: projection),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from space in SupportSpace.Of(value: geometry, key: state.Key).ToEff()
                    from intent in VectorIntent.Support(space: space, sample: state.Target, projection: state.Projection, key: state.Key).ToEff()
                    from result in intent.Project<TOut>(context: context, key: state.Key).Map(value => Seq(value)).ToEff()
                    select result),
            _ => key.Unsupported<TGeometry, TOut>(),
        };
    internal static Operation<TGeometry, Plane> PerpendicularFrameOp<TGeometry>(Op key, Seq<double> parameters) where TGeometry : notnull =>
        Operation<TGeometry, Plane>.Build(
            key: key, requirement: Requirement.CurveLength, state: (Key: key, Parameters: parameters),
            evaluator: static (state, geometry) =>
                GeometryKernel.CurveForm(source: geometry, op: state.Key)
                    .Bind(lease => lease.Use(curve => Optional(curve.GetPerpendicularFrames(toSeq(state.Parameters.AsIterable().Order().Distinct()).AsIterable()))
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
        return count switch {
            <= 0 => Operation<TGeometry, TOut>.Reject(key: key, fault: key.InvalidInput()),
            _ => (typeof(TGeometry), typeof(TOut), mode, agg) switch {
                (Type geometry, Type output, CurvatureMode.VectorCase, CurvatureAggregation.SamplesCase) when GeometryKernel.CanCurveForm(type: geometry) && output == typeof(Vector3d) =>
                    CurvatureNativeOp<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: GeometryKernel.CurveForm, project: static (op, curve, sampleCount, context) => CurveCurvaturesOf(key: op, curve: curve, count: sampleCount, model: context).Bind(values => op.AcceptResults<Vector3d, TOut>(values: values))),
                (Type geometry, Type output, CurvatureMode curveMode, CurvatureAggregation.SamplesCase) when GeometryKernel.CanCurveForm(type: geometry) && curveMode.IsCurveMagnitude && output == typeof(double) =>
                    CurvatureNativeOp<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: GeometryKernel.CurveForm, project: static (op, curve, sampleCount, context) => CurveMagnitudesOf(key: op, curve: curve, count: sampleCount, model: context).Bind(values => op.AcceptResults<double, TOut>(values: values))),
                (Type geometry, Type output, CurvatureMode curveMode, CurvatureAggregation.SamplesCase) when GeometryKernel.CanCurveForm(type: geometry) && curveMode.IsCurveMagnitude && output == typeof(Stat) =>
                    CurvatureNativeOp<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: GeometryKernel.CurveForm, project: static (op, curve, sampleCount, context) => CurveMagnitudesOf(key: op, curve: curve, count: sampleCount, model: context)
                        .Bind(values => Stat.Of(values: values, key: op, context: StatContext.Metric(metric: ScalarMetric.Magnitude)))
                        .Bind(stat => op.AcceptResults<Stat, TOut>(values: Seq(stat)))),
                (Type geometry, Type output, CurvatureMode curveMode, CurvatureAggregation.ExtremaCase extrema) when GeometryKernel.CanCurveForm(type: geometry) && curveMode.IsCurveMagnitude && output == typeof(Point3d) =>
                    CurvatureNativeOp<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: GeometryKernel.CurveForm, project: (op, curve, sampleCount, context) => CurveCurvatureSamples(op: op, curve: curve, count: sampleCount, ctx: context)
                        .Bind(samples => op.AcceptResults<Point3d, TOut>(values: Stat.Extrema(items: samples, projection: static sample => sample.Curvature, tolerance: 0.0, direction: extrema.Direction).Map(static hit => hit.Point)))),
                (Type geometry, Type output, CurvatureMode curveMode, CurvatureAggregation.ExtremaCase extrema) when GeometryKernel.CanCurveForm(type: geometry) && curveMode.IsCurveMagnitude && output == typeof(double) =>
                    CurvatureNativeOp<TGeometry, TOut, Curve>(key: key, count: count, requirement: Requirement.CurveLength, native: GeometryKernel.CurveForm, project: (op, curve, sampleCount, context) => CurveCurvatureSamples(op: op, curve: curve, count: sampleCount, ctx: context)
                        .Bind(samples => op.AcceptResults<double, TOut>(values: Stat.Extrema(items: samples, projection: static sample => sample.Curvature, tolerance: 0.0, direction: extrema.Direction).Map(static hit => hit.Curvature)))),
                (Type geometry, Type output, CurvatureMode.VectorCase, CurvatureAggregation.SamplesCase) when GeometryKernel.CanSurfaceForm(type: geometry) && output == typeof(SurfaceCurvature) =>
                    CurvatureNativeOp<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: GeometryKernel.SurfaceForm, project: static (op, surface, sampleCount, context) => SurfaceCurvaturesOf(key: op, surface: surface, count: sampleCount, model: context).Bind(values => op.AcceptResults<SurfaceCurvature, TOut>(values: values))),
                (Type geometry, Type output, CurvatureMode surfaceMode, CurvatureAggregation.SamplesCase) when GeometryKernel.CanSurfaceForm(type: geometry) && !surfaceMode.SurfaceMetrics.IsEmpty && output == typeof(Stat) =>
                    CurvatureNativeOp<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: GeometryKernel.SurfaceForm, project: (op, surface, sampleCount, context) => SurfaceCurvaturesOf(key: op, surface: surface, count: sampleCount, model: context)
                        .Bind(curvatures => BorrowCurvaturesOf(curvatures: curvatures, project: owned => SurfaceStatsOf(key: op, curvatures: owned, mode: surfaceMode).Bind(stats => op.AcceptResults<Stat, TOut>(values: stats))))),
                (Type geometry, Type output, CurvatureMode.ScalarCase { Metric: ScalarMetric metric }, CurvatureAggregation.SamplesCase) when GeometryKernel.CanSurfaceForm(type: geometry) && metric.IsSurface && output == typeof(double) =>
                    CurvatureNativeOp<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: GeometryKernel.SurfaceForm, project: (op, surface, sampleCount, context) => SurfaceCurvaturesOf(key: op, surface: surface, count: sampleCount, model: context)
                        .Bind(curvatures => BorrowCurvaturesOf(curvatures: curvatures, project: owned => SurfaceScalarsOf(key: op, curvatures: owned, metric: metric).Bind(values => op.AcceptResults<double, TOut>(values: values))))),
                (Type geometry, Type output, CurvatureMode.ScalarCase { Metric: ScalarMetric metric }, CurvatureAggregation.ExtremaCase extrema) when GeometryKernel.CanSurfaceForm(type: geometry) && metric.IsSurface && output == typeof(Point3d) =>
                    CurvatureNativeOp<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: GeometryKernel.SurfaceForm, project: (op, surface, sampleCount, context) => SurfaceCurvatureSamples(op: op, surface: surface, count: sampleCount, ctx: context, metric: metric)
                        .Bind(samples => op.AcceptResults<Point3d, TOut>(values: Stat.Extrema(items: samples, projection: static sample => sample.Curvature, tolerance: 0.0, direction: extrema.Direction).Map(static hit => hit.Point)))),
                (Type geometry, Type output, CurvatureMode.ScalarCase { Metric: ScalarMetric metric }, CurvatureAggregation.ExtremaCase extrema) when GeometryKernel.CanSurfaceForm(type: geometry) && metric.IsSurface && output == typeof(double) =>
                    CurvatureNativeOp<TGeometry, TOut, Surface>(key: key, count: count, requirement: Requirement.SurfaceEvaluation, native: GeometryKernel.SurfaceForm, project: (op, surface, sampleCount, context) => SurfaceCurvatureSamples(op: op, surface: surface, count: sampleCount, ctx: context, metric: metric)
                        .Bind(samples => op.AcceptResults<double, TOut>(values: Stat.Extrema(items: samples, projection: static sample => sample.Curvature, tolerance: 0.0, direction: extrema.Direction).Map(static hit => hit.Curvature)))),
                _ => key.Unsupported<TGeometry, TOut>(),
            },
        };
    }
    private static Operation<TGeometry, TOut> CurvatureNativeOp<TGeometry, TOut, TNative>(
        Op key,
        int count,
        Requirement requirement,
        Func<object?, Op, Fin<Lease<TNative>>> native,
        Func<Op, TNative, int, Context, Fin<Seq<TOut>>> project)
        where TGeometry : notnull
        where TNative : class, IDisposable =>
        Operation<TGeometry, TOut>.Build(
            key: key, requirement: requirement, state: (Key: key, Count: count, Native: native, Project: project),
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from result in state.Native(arg1: geometry, arg2: state.Key)
                    .Bind(lease => lease.Use((State: state, Context: context), static (s, native) => s.State.Project(arg1: s.State.Key, arg2: native, arg3: s.State.Count, arg4: s.Context))).ToEff()
                select result);
    private static Fin<Seq<Vector3d>> CurveCurvaturesOf(Op key, Curve curve, int count, Context model) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: model, key: key)
            .Bind(parameters => key.Accept(values: parameters.Map(parameter => curve.CurvatureAt(t: parameter))));
    private static Fin<Seq<double>> CurveMagnitudesOf(Op key, Curve curve, int count, Context model) =>
        CurveCurvaturesOf(key: key, curve: curve, count: count, model: model).Bind(vectors => vectors.TraverseM(vector => ScalarMetric.Magnitude.Of(value: vector, key: key)).As());
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CurvatureSample(Point3d Point, double Curvature);
    private static Fin<Seq<CurvatureSample>> CurveCurvatureSamples(Op op, Curve curve, int count, Context ctx) =>
        GeometryKernel.CurveSampleParameters(curve: curve, count: count, context: ctx, key: op)
            .Map(parameters => parameters.Map(t => new CurvatureSample(Point: curve.PointAt(t: t), Curvature: curve.CurvatureAt(t: t).Length)));
    private static Fin<Seq<SurfaceCurvature>> SurfaceCurvaturesOf(Op key, Surface surface, int count, Context model) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, count: count, context: model, key: key)
            .Bind(samples => samples.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult())).As());
    private static Fin<Seq<double>> SurfaceScalarsOf(Op key, Seq<SurfaceCurvature> curvatures, ScalarMetric metric) =>
        curvatures.TraverseM(curvature => metric.Of(value: curvature, key: key)).As();
    private static Fin<Seq<Stat>> SurfaceStatsOf(Op key, Seq<SurfaceCurvature> curvatures, CurvatureMode mode) =>
        mode.SurfaceMetrics.TraverseM(metric =>
            SurfaceScalarsOf(key: key, curvatures: curvatures, metric: metric)
                .Bind(values => Stat.Of(values: values, key: key, context: StatContext.Metric(metric: metric)))).As();
    private static Fin<T> BorrowCurvaturesOf<T>(Seq<SurfaceCurvature> curvatures, Func<Seq<SurfaceCurvature>, Fin<T>> project) {
        Fin<T> result = project(arg: curvatures);
        _ = curvatures.Iter(static curvature => curvature.Dispose());
        return result;
    }
    private static Fin<Seq<CurvatureSample>> SurfaceCurvatureSamples(Op op, Surface surface, int count, Context ctx, ScalarMetric metric) =>
        GeometryKernel.SurfaceSampleUv(surface: surface, count: count, context: ctx, key: op)
            .Bind(uvs => uvs.TraverseM(uv => Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(op.InvalidResult())
                .Bind(curvature => new Lease<SurfaceCurvature>.Owned(Value: curvature)
                    .Use(c => metric.Of(value: c, key: op).Map(value => new CurvatureSample(Point: c.Point, Curvature: value))))).As());
}
