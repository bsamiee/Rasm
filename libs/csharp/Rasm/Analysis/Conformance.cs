namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Conformance {
    public sealed record DistanceCase(int Count) : Conformance;
    public sealed record RmsCase(int Count) : Conformance;
    public sealed record WithinToleranceCase(int Count) : Conformance;
    public sealed record SummaryCase(int Count) : Conformance;
    public sealed record MaximumCase(int Count) : Conformance;
    public sealed record SignedResidualCase(int Count) : Conformance;
    public static Conformance Distance(int count) => new DistanceCase(Count: count);
    public static Conformance Rms(int count) => new RmsCase(Count: count);
    public static Conformance WithinTolerance(int count) => new WithinToleranceCase(Count: count);
    public static Conformance Summary(int count) => new SummaryCase(Count: count);
    public static Conformance Maximum(int count) => new MaximumCase(Count: count);
    public static Conformance SignedResidual(int count) => new SignedResidualCase(Count: count);
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    internal Type OutputType => Switch(
        distanceCase: static _ => typeof(double),
        rmsCase: static _ => typeof(double),
        withinToleranceCase: static _ => typeof(bool),
        summaryCase: static _ => typeof(Stat),
        maximumCase: static _ => typeof(ResidualSample),
        signedResidualCase: static _ => typeof(ResidualSample));
    internal int SampleCount => Switch(
        distanceCase: static d => d.Count,
        rmsCase: static r => r.Count,
        withinToleranceCase: static w => w.Count,
        summaryCase: static s => s.Count,
        maximumCase: static m => m.Count,
        signedResidualCase: static sr => sr.Count);
    public Operation<(TGeometry Geometry, TTarget Target), TOut> Operation<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (SampleCount, CanConform(typeof(TGeometry), typeof(TTarget)), typeof(TOut) == OutputType) switch {
            ( <= 0, _, _) => Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true, true) => Pair<TGeometry, TTarget, TOut>(this, SampleCount),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    internal Fin<Seq<TOut>> Project<TOut>(Seq<ResidualSample> residuals, Context context) =>
        this switch {
            DistanceCase =>
                Stat.Residuals<Seq<double>>(samples: residuals, key: Key, aggregate: ResidualAggregate.Distances).Bind(values => Key.AcceptResults<double, TOut>(values: values)),
            RmsCase =>
                Stat.Residuals<Stat>(samples: residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Key.AcceptResults<double, TOut>(values: Seq(stat.Rms))),
            WithinToleranceCase =>
                Stat.Residuals<Stat>(samples: residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Key.AcceptResults<bool, TOut>(values: Seq(stat.WithinTolerance))),
            SummaryCase =>
                Stat.Residuals<Stat>(samples: residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Key.AcceptResults<Stat, TOut>(values: Seq(stat))),
            MaximumCase =>
                Stat.Residuals<ResidualSample>(samples: residuals, key: Key, aggregate: ResidualAggregate.Maximum).Bind(sample => Key.AcceptResults<ResidualSample, TOut>(values: Seq(sample))),
            SignedResidualCase =>
                Key.AcceptResults<ResidualSample, TOut>(values: residuals),
            _ => Fin.Fail<Seq<TOut>>(Key.Unsupported(geometryType: typeof(ResidualSample), outputType: typeof(TOut))),
        };
    private static bool CanConform(Type geometry, Type target) =>
        geometry == typeof(object) || target == typeof(object)
        || (GeometryKernel.CanCurveForm(type: geometry) && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Polyline) || target == typeof(Plane) || target == typeof(Sphere) || GeometryKernel.CanCurveForm(type: target) || typeof(Surface).IsAssignableFrom(target)))
        || (typeof(Surface).IsAssignableFrom(geometry) && (target == typeof(Plane) || target == typeof(Sphere) || typeof(Surface).IsAssignableFrom(target)));
    private static Operation<(TGeometry Geometry, TTarget Target), TValue> Pair<TGeometry, TTarget, TValue>(Conformance aspect, int count) where TGeometry : notnull where TTarget : notnull =>
        Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Key, requiresContext: true,
            state: (Aspect: aspect, Count: count),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: Key, requirements: static (op, kindG, _) => kindG.Topology switch {
                    Topology.Curve => Fin.Succ((A: Requirement.CurveLength, B: Requirement.None)),
                    Topology.Surface => Fin.Succ((A: Requirement.SurfaceEvaluation, B: Requirement.None)),
                    _ => Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))),
                }, cancel: runtime.Cancellation).ToEff()
                from residuals in Samples(aspect: state.Aspect, geometry: resolved.A, target: resolved.B, count: state.Count, context: runtime.Context).ToEff()
                from result in state.Aspect.Project<TValue>(residuals: residuals, context: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<ResidualSample>> Samples<TGeometry, TTarget>(Conformance aspect, TGeometry geometry, TTarget target, int count, Context context) where TGeometry : notnull where TTarget : notnull =>
        (aspect, geometry, target) switch {
            (_, object curveLike, Line line) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, line, count, context, convert: static l => new LineCurve(l), distance: static (l, pt) => pt.DistanceTo(l.ClosestPoint(testPoint: pt, limitToFiniteSegment: true))),
            (_, object curveLike, Circle circle) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, circle, count, context, convert: static c => new ArcCurve(c), distance: static (c, pt) => pt.DistanceTo(other: c.ClosestPoint(testPoint: pt))),
            (_, object curveLike, Arc arc) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, arc, count, context, convert: static a => new ArcCurve(a), distance: static (a, pt) => pt.DistanceTo(other: a.ClosestPoint(testPoint: pt))),
            (_, object curveLike, Polyline polyline) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, polyline, count, context, convert: static p => p.ToPolylineCurve(), distance: static (p, pt) => pt.DistanceTo(other: p.ClosestPoint(testPoint: pt))),
            (_, object curveLike, Surface surface) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, surface, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (s, pt) => s.ClosestPoint(testPoint: pt, u: out double u, v: out double v) ? pt.DistanceTo(other: s.PointAt(u: u, v: v)) : double.NaN))),
            (SignedResidualCase, object curveLike, Plane plane) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, plane, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (p, pt) => p.DistanceTo(testPoint: pt)))),
            (SignedResidualCase, object curveLike, Sphere sphere) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, sphere, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (s, pt) => pt.DistanceTo(s.Center) - s.Radius))),
            (_, object curveLike, Plane plane) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, plane, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (p, pt) => Math.Abs(value: p.DistanceTo(testPoint: pt))))),
            (_, object curveLike, Sphere sphere) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, sphere, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (s, pt) => pt.DistanceTo(other: s.ClosestPoint(testPoint: pt))))),
            (_, object curveLike, object targetCurveLike) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) && GeometryKernel.CanCurveForm(type: targetCurveLike.GetType()) => CurveCurveSamples(aspect, curveLike, targetCurveLike, count, context),
            (SignedResidualCase, Surface surface, Plane plane) => SampleResiduals(surface, plane, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (p, pt) => p.DistanceTo(testPoint: pt)),
            (SignedResidualCase, Surface surface, Sphere sphere) => SampleResiduals(surface, sphere, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (s, pt) => pt.DistanceTo(s.Center) - s.Radius),
            (_, Surface surface, Plane plane) => SampleResiduals(surface, plane, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (p, pt) => Math.Abs(value: p.DistanceTo(testPoint: pt))),
            (_, Surface surface, Sphere sphere) => SampleResiduals(surface, sphere, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (s, pt) => pt.DistanceTo(other: s.ClosestPoint(testPoint: pt))),
            (_, Surface surface, Surface other) => SampleResiduals(surface, other, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (s, pt) => s.ClosestPoint(testPoint: pt, u: out double u, v: out double v) ? pt.DistanceTo(other: s.PointAt(u: u, v: v)) : double.NaN),
            _ => Fin.Fail<Seq<ResidualSample>>(Key.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Fin<Seq<ResidualSample>> CurveCurveSamples(Conformance aspect, object curveLike, object targetCurveLike, int count, Context context) =>
        GeometryKernel.CurveForm(source: curveLike, op: Key)
            .Bind(leftLease => GeometryKernel.CurveForm(source: targetCurveLike, op: Key)
                .Bind(rightLease => leftLease.Use(left => rightLease.Use(right => aspect switch {
                    WithinToleranceCase or MaximumCase =>
                        Analyze.CurveDeviationOf(left: left, right: right, context: context, op: Key)
                            .Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance))),
                    _ => SampleResiduals(left, right, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: static (c, pt) => c.ClosestPoint(testPoint: pt, t: out double t) ? pt.DistanceTo(other: c.PointAt(t: t)) : double.NaN),
                }))));
    private static Fin<Seq<ResidualSample>> CurveSamples<TPrimitive>(Conformance aspect, object curveLike, TPrimitive primitive, int count, Context context, Func<TPrimitive, Curve> convert, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        GeometryKernel.CurveForm(source: curveLike, op: Key)
            .Bind(lease => lease.Use(curve => aspect switch {
                WithinToleranceCase or MaximumCase =>
                    new Lease<Curve>.Owned(Value: convert(arg: primitive)).Use(native => Analyze.CurveDeviationOf(left: curve, right: native, context: context, op: Key)
                        .Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance)))),
                _ => SampleResiduals(curve, primitive, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: distance),
            }));
    private static Fin<Seq<ResidualSample>> SampleResiduals<TGeometry, TPrimitive>(TGeometry geometry, TPrimitive primitive, int count, Context context, Func<TGeometry, int, Context, Op, Fin<Seq<Point3d>>> sampler, Func<TPrimitive, Point3d, double> distance) where TGeometry : notnull where TPrimitive : notnull =>
        sampler(arg1: geometry, arg2: count, arg3: context, arg4: Key)
            .Map(points => points.Map((p, i) => distance(primitive, p) switch { double d => new ResidualSample(i, p, d, context.Absolute.Value, Math.Abs(d) <= context.Absolute.Value) }));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.Operation<TGeometry, TTarget, TOut>() ?? Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
}
