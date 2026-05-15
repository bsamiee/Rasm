namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record Summary(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TOut> Operation<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (this, Analyze.CanConform(geometry: typeof(TGeometry), target: typeof(TTarget))) switch {
            (Distance { Count: <= 0 } or Rms { Count: <= 0 } or WithinTolerance { Count: <= 0 } or Summary { Count: <= 0 } or Maximum { Count: <= 0 }, _) =>
                global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true) => Analyze.ConformanceProject<TGeometry, TTarget, TOut>(aspect: this),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.Operation<TGeometry, TTarget, TOut>() ?? global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
    internal static global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TOut> ConformanceProject<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, operation: ConformancePair<TGeometry, TTarget, double>(
                count: item.Count, project: static (residuals, _) => Stat.ResidualDistances(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(values => Rasm.Analysis.Conformance.Key.Accept(values: values)))),
            (Conformance.Rms item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, operation: ConformancePair<TGeometry, TTarget, double>(
                count: item.Count, project: static (residuals, _) => Stat.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(stats => Rasm.Analysis.Conformance.Key.Accept(value: stats.Rms)))),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, operation: ConformancePair<TGeometry, TTarget, bool>(
                count: item.Count, project: static (residuals, context) => Stat.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(stats => Rasm.Analysis.Conformance.Key.Accept(value: stats.Maximum <= context.Absolute.Value)))),
            (Conformance.Summary item, Type output) when output == typeof(Stat) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, operation: ConformancePair<TGeometry, TTarget, Stat>(
                count: item.Count, project: static (residuals, context) => Stat.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key)
                    .Bind(stats => Stat.Residual(tolerance: context.Absolute.Value, stats: stats, key: Rasm.Analysis.Conformance.Key))
                    .Map(stat => Seq(stat)))),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, operation: ConformancePair<TGeometry, TTarget, ResidualSample>(
                count: item.Count, project: static (residuals, _) => Stat.MaximumResidual(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(sample => Rasm.Analysis.Conformance.Key.Accept(value: sample)))),
            _ => Rasm.Analysis.Conformance.Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    private static global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TValue> ConformancePair<TGeometry, TTarget, TValue>(int count, Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TTarget : notnull =>
        global::Rasm.Analysis.Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Rasm.Analysis.Conformance.Key, requiresContext: true,
            state: (Op: Rasm.Analysis.Conformance.Key, Count: count, Project: project),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: state.Op, requirements: static (op, kindG, _) => kindG.Topology switch {
                    Topology.Curve => Fin.Succ((A: Requirement.CurveLength, B: Requirement.None)),
                    Topology.Surface => Fin.Succ((A: Requirement.SurfaceEvaluation, B: Requirement.None)),
                    _ => Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))),
                }, cancel: runtime.Cancellation).ToEff()
                from residuals in ConformanceSamples(geometry: resolved.A, target: resolved.B, count: state.Count, context: runtime.Context, op: state.Op).ToEff()
                from result in state.Project(arg1: residuals, arg2: runtime.Context).ToEff()
                select result);
    internal static bool CanConform(Type geometry, Type target) =>
        geometry == typeof(object)
        || (typeof(Curve).IsAssignableFrom(geometry) && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc)))
        || (typeof(Surface).IsAssignableFrom(geometry) && (target == typeof(Plane) || target == typeof(Sphere)));
    internal static Fin<Seq<ResidualSample>> ConformanceSamples<TGeometry, TTarget>(TGeometry geometry, TTarget target, int count, Context context, Op op) where TGeometry : notnull where TTarget : notnull =>
        (geometry, target) switch {
            (Curve curve, Line line) => SampleCurveAgainst(curve: curve, primitive: line, count: count, context: context, op: op, distance: static (l, pt) => pt.DistanceTo(l.ClosestPoint(testPoint: pt, limitToFiniteSegment: true))),
            (Curve curve, Circle circle) => SampleCurveAgainst(curve: curve, primitive: circle, count: count, context: context, op: op, distance: static (c, pt) => pt.DistanceTo(other: c.ClosestPoint(testPoint: pt))),
            (Curve curve, Arc arc) => SampleCurveAgainst(curve: curve, primitive: arc, count: count, context: context, op: op, distance: static (a, pt) => pt.DistanceTo(other: a.ClosestPoint(testPoint: pt))),
            (Surface surface, Plane plane) => SampleSurfaceAgainst(surface: surface, primitive: plane, resolution: count, context: context, op: op, distance: static (p, pt) => Math.Abs(value: p.DistanceTo(testPoint: pt))),
            (Surface surface, Sphere sphere) => SampleSurfaceAgainst(surface: surface, primitive: sphere, resolution: count, context: context, op: op, distance: static (s, pt) => pt.DistanceTo(other: s.ClosestPoint(testPoint: pt))),
            _ => Fin.Fail<Seq<ResidualSample>>(op.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Seq<ResidualSample> Residuals<TP>(Seq<Point3d> points, TP primitive, Context context, Func<TP, Point3d, double> distance) where TP : notnull =>
        toSeq(points.AsIterable().Select((p, i) => distance(primitive, p) switch { double d => new ResidualSample(i, p, d, context.Absolute.Value, d <= context.Absolute.Value) }));
    private static Fin<Seq<ResidualSample>> SampleCurveAgainst<TP>(Curve curve, TP primitive, int count, Context context, Op op, Func<TP, Point3d, double> distance) where TP : notnull =>
        GeometryKernel.Fractions(count, op).Bind(fs => Optional(curve.NormalizedLengthParameters(s: [.. fs.AsIterable()], absoluteTolerance: context.Absolute.Value, fractionalTolerance: context.Fractional)).ToFin(op.InvalidResult()).Map(ps => Residuals(toSeq(ps).Map(curve.PointAt), primitive, context, distance)));
    private static Fin<Seq<ResidualSample>> SampleSurfaceAgainst<TP>(Surface surface, TP primitive, int resolution, Context context, Op op, Func<TP, Point3d, double> distance) where TP : notnull =>
        (Analyze.Samples(domain: surface.Domain(direction: 0), resolution: resolution, key: op),
         Analyze.Samples(domain: surface.Domain(direction: 1), resolution: resolution, key: op))
        .Apply(static (u, v) => (U: u, V: v)).As()
        .Map(samples => Residuals(samples.U.Bind(u => samples.V.Map(v => surface.PointAt(u: u, v: v))), primitive, context, distance));
}
