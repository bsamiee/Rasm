namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)] public readonly record struct ResidualProfile(int Count, double Minimum, double Maximum, double Mean, double Variance, double Rms, double Tolerance, bool WithinTolerance);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Conformance {
    public sealed record Distance(int Count) : Conformance; public sealed record Rms(int Count) : Conformance; public sealed record WithinTolerance(int Count) : Conformance; public sealed record ProfileResidual(int Count) : Conformance; public sealed record Maximum(int Count) : Conformance;
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ToQuery<TGeometry, TPrimitive, TOut>() where TGeometry : notnull where TPrimitive : notnull =>
        (this, Dispatch.SupportsPair(table: Dispatch.ConformanceTable, left: typeof(TGeometry), right: typeof(TPrimitive))) switch {
            (Distance { Count: <= 0 } or Rms { Count: <= 0 } or WithinTolerance { Count: <= 0 } or ProfileResidual { Count: <= 0 } or Maximum { Count: <= 0 }, _) =>
                Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true) => Analyze.ConformanceProject<TGeometry, TPrimitive, TOut>(aspect: this),
            _ => Key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> Conformance<TGeometry, TPrimitive, TOut>(Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        aspect?.ToQuery<TGeometry, TPrimitive, TOut>() ?? Query<(TGeometry Geometry, TPrimitive Primitive), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
    internal static Query<(TGeometry Geometry, TPrimitive Primitive), TOut> ConformanceProject<TGeometry, TPrimitive, TOut>(Conformance aspect) where TGeometry : notnull where TPrimitive : notnull =>
        (aspect, typeof(TOut)) switch {
            (Conformance.Distance item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, double>(
                count: item.Count, project: static (residuals, _) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Many(key: Rasm.Analysis.Conformance.Key, values: values)))),
            (Conformance.Rms item, Type output) when output == typeof(double) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, double>(
                count: item.Count, project: static (residuals, _) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key)).Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: stats.Rms)))),
            (Conformance.WithinTolerance item, Type output) when output == typeof(bool) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, bool>(
                count: item.Count, project: static (residuals, context) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals).Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key)).Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: stats.Maximum <= context.Absolute.Value)))),
            (Conformance.ProfileResidual item, Type output) when output == typeof(ResidualProfile) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, ResidualProfile>(
                count: item.Count, project: static (residuals, context) => ResidualDistances(key: Rasm.Analysis.Conformance.Key, samples: residuals)
                    .Bind(values => Stats.From(values: values, key: Rasm.Analysis.Conformance.Key))
                    .Bind(stats => One(key: Rasm.Analysis.Conformance.Key, value: new ResidualProfile(Count: stats.Count, Minimum: stats.Minimum, Maximum: stats.Maximum, Mean: stats.Mean, Variance: stats.Variance, Rms: stats.Rms, Tolerance: context.Absolute.Value, WithinTolerance: stats.Maximum <= context.Absolute.Value))))),
            (Conformance.Maximum item, Type output) when output == typeof(ResidualSample) => Cast<(TGeometry Geometry, TPrimitive Primitive), TOut>(key: Rasm.Analysis.Conformance.Key, query: ConformancePair<TGeometry, TPrimitive, ResidualSample>(
                count: item.Count, project: static (residuals, _) => residuals
                    .TraverseM(static sample => sample switch {
                        { Distance: double d, Location.IsValid: true } when d >= 0.0 && RhinoMath.IsValidDouble(x: d) => Fin.Succ(sample),
                        _ => Fin.Fail<ResidualSample>(Rasm.Analysis.Conformance.Key.InvalidResult()),
                    })
                    .As()
                    .Bind(values => values.Maxima(projection: static sample => sample.Distance, tolerance: 0.0).Head.ToFin(Rasm.Analysis.Conformance.Key.InvalidResult()).Map(static best => Seq(best))))),
            _ => Rasm.Analysis.Conformance.Key.Unsupported<(TGeometry Geometry, TPrimitive Primitive), TOut>(),
        };
    private static Query<(TGeometry Geometry, TPrimitive Primitive), TValue> ConformancePair<TGeometry, TPrimitive, TValue>(int count, Func<Seq<ResidualSample>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull where TPrimitive : notnull =>
        Query<(TGeometry Geometry, TPrimitive Primitive), TValue>.Build(
            key: Rasm.Analysis.Conformance.Key, requiresContext: true,
            state: (Op: Rasm.Analysis.Conformance.Key, Count: count, Project: project),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from candidate in runtime.Context.ValidatePair(a: pair.Geometry, b: pair.Primitive, requirementA: Requirement.None, requirementB: Requirement.None, cancel: runtime.Cancellation).ToEff()
                from kindG in ((object)candidate.A).Kind(context: runtime.Context).ToEff()
                from kindP in ((object)candidate.B).Kind(context: runtime.Context).ToEff()
                from requirement in (kindG.Topology switch {
                    Topology.Curve => Fin.Succ(Requirement.CurveLength),
                    Topology.Surface => Fin.Succ(Requirement.SurfaceEvaluation),
                    _ => Fin.Fail<Requirement>(state.Op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))),
                }).ToEff()
                from validated in runtime.Context.ValidatePair(a: candidate.A, b: candidate.B, requirementA: requirement, requirementB: Requirement.None, cancel: runtime.Cancellation).ToEff()
                from residuals in kindG.Conformance(kindP: kindP, geometry: validated.A, primitive: validated.B, count: state.Count, context: runtime.Context, op: state.Op).ToEff()
                from result in state.Project(arg1: residuals, arg2: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<double>> ResidualDistances(Op key, Seq<ResidualSample> samples) =>
        samples.TraverseM(sample => sample switch {
            { Distance: double distance, Location.IsValid: true } when distance >= 0.0 && RhinoMath.IsValidDouble(x: distance) => Fin.Succ(distance),
            _ => Fin.Fail<double>(key.InvalidResult()),
        }).As();
}
