namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Conformance {
    public sealed record DistanceCase(int Count) : Conformance;
    public sealed record RmsCase(int Count) : Conformance;
    public sealed record WithinToleranceCase(int Count) : Conformance;
    public sealed record SummaryCase(int Count) : Conformance;
    public sealed record MaximumCase(int Count) : Conformance;
    // Signed surface conformance: positive outside / opposite-normal side, negative inside. Surface targets only (Plane, Sphere).
    public sealed record SignedResidualCase(int Count) : Conformance;
    public static Conformance Distance(int count) => new DistanceCase(Count: count);
    public static Conformance Rms(int count) => new RmsCase(Count: count);
    public static Conformance WithinTolerance(int count) => new WithinToleranceCase(Count: count);
    public static Conformance Summary(int count) => new SummaryCase(Count: count);
    public static Conformance Maximum(int count) => new MaximumCase(Count: count);
    public static Conformance SignedResidual(int count) => new SignedResidualCase(Count: count);
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public Operation<(TGeometry Geometry, TTarget Target), TOut> Operation<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (this, ConformanceDispatch.CanConform(typeof(TGeometry), typeof(TTarget)), typeof(TOut)) switch {
            (DistanceCase { Count: <= 0 } or RmsCase { Count: <= 0 } or WithinToleranceCase { Count: <= 0 } or SummaryCase { Count: <= 0 } or MaximumCase { Count: <= 0 } or SignedResidualCase { Count: <= 0 }, _, _) =>
                Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, false, _) => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
            (DistanceCase d, _, Type o) when o == typeof(double) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, double>(this, d.Count, static (r, _) => Stat.ResidualDistances(samples: r, key: Key).Bind(values => Key.Accept(values: values))).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            (RmsCase r, _, Type o) when o == typeof(double) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, double>(this, r.Count, static (rs, _) => Stat.FromResiduals(samples: rs, key: Key).Bind(s => Key.Accept(value: s.Rms))).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            (WithinToleranceCase w, _, Type o) when o == typeof(bool) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, bool>(this, w.Count, static (rs, c) => Stat.FromResiduals(samples: rs, key: Key).Bind(s => Key.Accept(value: s.Maximum <= c.Absolute.Value))).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            (SummaryCase s, _, Type o) when o == typeof(ConformanceSummary) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, ConformanceSummary>(this, s.Count, static (rs, c) => Stat.FromResiduals(samples: rs, key: Key).Bind(stats => Key.Accept(value: new ConformanceSummary(Distribution: stats, Tolerance: c.Absolute.Value, WithinTolerance: stats.Maximum <= c.Absolute.Value)))).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            (MaximumCase m, _, Type o) when o == typeof(ResidualSample) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, ResidualSample>(this, m.Count, static (rs, _) => Stat.MaximumResidual(samples: rs, key: Key).Bind(sample => Key.Accept(value: sample))).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            (SignedResidualCase sr, _, Type o) when o == typeof(ResidualSample) =>
                ConformanceDispatch.Pair<TGeometry, TTarget, ResidualSample>(this, sr.Count, static (rs, _) => Key.Accept(values: rs)).As<(TGeometry Geometry, TTarget Target), TOut>(key: Key),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.Operation<TGeometry, TTarget, TOut>() ?? Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
}

// --- [COMPOSITION] ------------------------------------------------------------------------
file static class ConformanceDispatch {
    public static bool CanConform(Type geometry, Type target) =>
        geometry == typeof(object) || target == typeof(object)
        || (GeometryKernel.CanCurveForm(type: geometry) && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc)))
        || (typeof(Surface).IsAssignableFrom(geometry) && (target == typeof(Plane) || target == typeof(Sphere)));
    public static Operation<(TGeometry Geometry, TTarget Target), TValue> Pair<TGeometry, TTarget, TValue>(Conformance aspect, int count, Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TTarget : notnull =>
        Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Conformance.Key, requiresContext: true,
            state: (Aspect: aspect, Count: count, Project: project),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: Conformance.Key, requirements: static (op, kindG, _) => kindG.Topology switch {
                    Topology.Curve => Fin.Succ((A: Requirement.CurveLength, B: Requirement.None)),
                    Topology.Surface => Fin.Succ((A: Requirement.SurfaceEvaluation, B: Requirement.None)),
                    _ => Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))),
                }, cancel: runtime.Cancellation).ToEff()
                from residuals in Samples(aspect: state.Aspect, geometry: resolved.A, target: resolved.B, count: state.Count, context: runtime.Context).ToEff()
                from result in state.Project(arg1: residuals, arg2: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<ResidualSample>> Samples<TGeometry, TTarget>(Conformance aspect, TGeometry geometry, TTarget target, int count, Context context) where TGeometry : notnull where TTarget : notnull =>
        (aspect, geometry, target) switch {
            (_, object curveLike, Line line) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, line, count, context, convert: static l => new LineCurve(l), distance: static (l, pt) => pt.DistanceTo(l.ClosestPoint(testPoint: pt, limitToFiniteSegment: true))),
            (_, object curveLike, Circle circle) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, circle, count, context, convert: static c => new ArcCurve(c), distance: static (c, pt) => pt.DistanceTo(other: c.ClosestPoint(testPoint: pt))),
            (_, object curveLike, Arc arc) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => CurveSamples(aspect, curveLike, arc, count, context, convert: static a => new ArcCurve(a), distance: static (a, pt) => pt.DistanceTo(other: a.ClosestPoint(testPoint: pt))),
            (Conformance.SignedResidualCase, Surface surface, Plane plane) => SampleResiduals(surface, plane, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (p, pt) => p.DistanceTo(testPoint: pt)),
            (Conformance.SignedResidualCase, Surface surface, Sphere sphere) => SampleResiduals(surface, sphere, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (s, pt) => pt.DistanceTo(s.Center) - s.Radius),
            (_, Surface surface, Plane plane) => SampleResiduals(surface, plane, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (p, pt) => Math.Abs(value: p.DistanceTo(testPoint: pt))),
            (_, Surface surface, Sphere sphere) => SampleResiduals(surface, sphere, count, context, GeometryKernel.SurfaceSamplePoints, distance: static (s, pt) => pt.DistanceTo(other: s.ClosestPoint(testPoint: pt))),
            _ => Fin.Fail<Seq<ResidualSample>>(Conformance.Key.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Fin<Seq<ResidualSample>> CurveSamples<TPrimitive>(Conformance aspect, object curveLike, TPrimitive primitive, int count, Context context, Func<TPrimitive, Curve> convert, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        GeometryKernel.CurveForm(source: curveLike, op: Conformance.Key)
            .Bind(lease => lease.Use(curve => aspect switch {
                Conformance.WithinToleranceCase or Conformance.MaximumCase =>
                    new Lease<Curve>.Owned(Value: convert(arg: primitive)).Use(native => Analyze.CurveDeviationOf(left: curve, right: native, context: context, op: Conformance.Key)
                        .Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance)))),
                _ => SampleResiduals(curve, primitive, count, context, sampler: static (c, n, ctx, key) => GeometryKernel.CurveSampleParameters(curve: c, count: n, context: ctx, key: key).Map(parameters => parameters.Map(c.PointAt)), distance: distance),
            }));
    private static Fin<Seq<ResidualSample>> SampleResiduals<TGeometry, TPrimitive>(TGeometry geometry, TPrimitive primitive, int count, Context context, Func<TGeometry, int, Context, Op, Fin<Seq<Point3d>>> sampler, Func<TPrimitive, Point3d, double> distance) where TGeometry : notnull where TPrimitive : notnull =>
        sampler(arg1: geometry, arg2: count, arg3: context, arg4: Conformance.Key)
            .Map(points => points.Map((p, i) => distance(primitive, p) switch { double d => new ResidualSample(i, p, d, context.Absolute.Value, d <= context.Absolute.Value) }));
}
