namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record Summary(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public Query<(TGeometry Geometry, TTarget Target), TOut> ToQuery<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (this, Dispatch.Supports(CapTag.Conformance, typeof(TGeometry), typeof(TTarget))) switch {
            (Distance { Count: <= 0 } or Rms { Count: <= 0 } or WithinTolerance { Count: <= 0 } or Summary { Count: <= 0 } or Maximum { Count: <= 0 }, _) =>
                Query<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true) => Analyze.ConformanceProject<TGeometry, TTarget, TOut>(aspect: this),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.ToQuery<TGeometry, TTarget, TOut>() ?? Query<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
    internal static Query<(TGeometry Geometry, TTarget Target), TOut> ConformanceProject<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TTarget, double>(
                count: item.Count, project: static (residuals, _) => Stats.ResidualDistances(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(values => Rasm.Analysis.Conformance.Key.Many(values: values)))),
            (Conformance.Rms item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TTarget, double>(
                count: item.Count, project: static (residuals, _) => Stats.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(stats => Rasm.Analysis.Conformance.Key.One(value: stats.Rms)))),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TTarget, bool>(
                count: item.Count, project: static (residuals, context) => Stats.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(stats => Rasm.Analysis.Conformance.Key.One(value: stats.Maximum <= context.Absolute.Value)))),
            (Conformance.Summary item, Type output) when output == typeof(StatProfile) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TTarget, StatProfile>(
                count: item.Count, project: static (residuals, context) => Stats.FromResiduals(samples: residuals, key: Rasm.Analysis.Conformance.Key)
                    .Bind(stats => StatProfile.Residual(tolerance: context.Absolute.Value, stats: stats, key: Rasm.Analysis.Conformance.Key))
                    .Bind(profile => Rasm.Analysis.Conformance.Key.One(value: profile)))),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) => Cast<(TGeometry Geometry, TTarget Target), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TTarget, ResidualSample>(
                count: item.Count, project: static (residuals, _) => Stats.MaximumResidual(samples: residuals, key: Rasm.Analysis.Conformance.Key).Bind(sample => Rasm.Analysis.Conformance.Key.One(value: sample)))),
            _ => Rasm.Analysis.Conformance.Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    private static Query<(TGeometry Geometry, TTarget Target), TValue> ConformancePair<TGeometry, TTarget, TValue>(int count, Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TTarget : notnull =>
        Query<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Rasm.Analysis.Conformance.Key, requiresContext: true,
            state: (Op: Rasm.Analysis.Conformance.Key, Count: count, Project: project),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: state.Op, requirements: static (op, kindG, _) => kindG.Topology switch {
                    Topology.Curve => Fin.Succ((A: Requirement.CurveLength, B: Requirement.None)),
                    Topology.Surface => Fin.Succ((A: Requirement.SurfaceEvaluation, B: Requirement.None)),
                    _ => Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))),
                }, cancel: runtime.Cancellation).ToEff()
                from residuals in Dispatch.Resolve<Seq<ResidualSample>, (int, Context, Op)>(CapTag.Conformance, resolved.A, resolved.B, (state.Count, runtime.Context, state.Op), state.Op).ToEff()
                from result in state.Project(arg1: residuals, arg2: runtime.Context).ToEff()
                select result);
}
