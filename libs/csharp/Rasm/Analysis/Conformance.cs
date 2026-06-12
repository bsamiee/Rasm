using Foundation.CSharp.Analyzers.Contracts;
using Rasm.Vectors;

namespace Rasm.Analysis;

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class ConformanceMetric {
    public static readonly ConformanceMetric Distance = new(key: 0, output: typeof(double), isSigned: false, isContainment: false, exactCurveDeviation: false, projection: static (residuals, _, _, key) => Analyze.ConformanceResidualDistances(samples: residuals, key: key).Map(static values => values.Map(static value => (object)value)));
    public static readonly ConformanceMetric Rms = new(key: 1, output: typeof(double), isSigned: false, isContainment: false, exactCurveDeviation: false, projection: static (residuals, _, context, key) => Analyze.ConformanceResidualSummary(samples: residuals, tolerance: context.Absolute.Value, key: key).Map(static stat => Seq((object)stat.Rms)));
    public static readonly ConformanceMetric WithinTolerance = new(key: 2, output: typeof(bool), isSigned: false, isContainment: false, exactCurveDeviation: true, projection: static (residuals, _, context, key) => Analyze.ConformanceResidualSummary(samples: residuals, tolerance: context.Absolute.Value, key: key).Map(static stat => Seq((object)stat.WithinTolerance)));
    public static readonly ConformanceMetric Summary = new(key: 3, output: typeof(Stat), isSigned: false, isContainment: false, exactCurveDeviation: false, projection: static (residuals, _, context, key) => Analyze.ConformanceResidualSummary(samples: residuals, tolerance: context.Absolute.Value, key: key).Map(static stat => Seq((object)stat)));
    public static readonly ConformanceMetric Maximum = new(key: 4, output: typeof(ResidualSample), isSigned: false, isContainment: false, exactCurveDeviation: true, projection: static (residuals, _, _, key) => Analyze.ConformanceResidualMaximum(samples: residuals, key: key).Map(static sample => Seq((object)sample)));
    public static readonly ConformanceMetric SignedResidual = new(key: 5, output: typeof(ResidualSample), isSigned: true, isContainment: false, exactCurveDeviation: false, projection: static (residuals, _, _, _) => Fin.Succ(residuals.Map(static sample => (object)sample)));
    public static readonly ConformanceMetric Containment = new(key: 6, output: typeof(ResidualSample), isSigned: true, isContainment: true, exactCurveDeviation: false, projection: static (residuals, _, _, _) => Fin.Succ(residuals.Map(static sample => (object)sample)));
    public static readonly ConformanceMetric Distribution = new(key: 7, output: typeof(Distribution), isSigned: false, isContainment: false, exactCurveDeviation: false, projection: static (residuals, percentiles, _, key) => Analyze.ConformanceResidualDistribution(samples: residuals, percentiles: percentiles, key: key).Map(static result => Seq((object)result)));
    internal delegate Fin<Seq<object>> ConformanceProjection(Seq<ResidualSample> residuals, Seq<double> percentiles, Context context, Op key);
    public Type Output { get; }
    internal bool IsSigned { get; }
    internal bool IsContainment { get; }
    internal bool ExactCurveDeviation { get; }
    internal ConformanceProjection Projection { get; }
    internal bool AcceptsTarget(Type target, bool curveSource) =>
        (IsContainment && (target == typeof(Brep) || target == typeof(Mesh)))
        || (IsSigned && !IsContainment && GeometryKernel.CanSignedDistance(type: target))
        || (!IsSigned && !IsContainment && (GeometryKernel.CanClosest(type: target)
            || (curveSource && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Polyline) || GeometryKernel.CanCurveForm(type: target)))));
    internal Requirement TargetRequirement(Kind kind) => IsContainment && (kind.Topology == Topology.Brep || kind.Topology == Topology.Mesh) ? Requirement.SolidTopology : Requirement.None;
    internal Fin<Seq<TOut>> Project<TOut>(Seq<ResidualSample> residuals, Seq<double> percentiles, Context context, Op key) =>
        Output == typeof(TOut)
            ? Projection(residuals: residuals, percentiles: percentiles, context: context, key: key).Bind(values => new AnalysisOutput<TOut>(key).Objects(values: values, sourceType: Output))
            : Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(ConformanceMetric), outputType: typeof(TOut)));
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance) {
    public bool IsValid =>
        Index >= 0 && Location.IsValid && RhinoMath.IsValidDouble(Distance) && RhinoMath.IsValidDouble(Tolerance) && Tolerance >= 0.0
        && WithinTolerance == (Math.Abs(Distance) <= Tolerance);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<(TGeometry Geometry, TTarget Target), TOut> RelationConformance<TGeometry, TTarget, TOut>(ConformanceMetric? metric, int count, Seq<double> percentiles, Op key) where TGeometry : notnull where TTarget : notnull =>
        (metric, count) switch {
            (null, _) => Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: key, fault: key.InvalidInput()),
            (_, <= 0) => Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: key, fault: key.InvalidInput()),
            (ConformanceMetric active, _) when CanConform(metric: active, geometry: typeof(TGeometry), target: typeof(TTarget)) && typeof(TOut) == active.Output =>
                ConformancePair<TGeometry, TTarget, TOut>(metric: active, count: count, percentiles: percentiles, key: key),
            _ => key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    internal static Fin<Seq<double>> ConformanceResidualDistances(Seq<ResidualSample> samples, Op key) =>
        ValidateResiduals(samples: samples, key: key).Map(static validated => validated.Map(static sample => sample.Distance));
    internal static Fin<Stat> ConformanceResidualSummary(Seq<ResidualSample> samples, double tolerance, Op key) =>
        ConformanceResidualDistances(samples: samples, key: key)
            .Bind(distances => Stat.Of(values: distances, key: key))
            .Bind(stat => AnalysisAcceptance.AcceptValue(key: key, value: stat with { Context = StatContext.Tolerance(tolerance: tolerance, minimum: stat.Minimum, maximum: stat.Maximum) }));
    internal static Fin<ResidualSample> ConformanceResidualMaximum(Seq<ResidualSample> samples, Op key) =>
        ValidateResiduals(samples: samples, key: key)
            .Bind(validated => Stat.Extrema(items: validated, projection: static sample => sample.Distance, tolerance: 0.0, direction: ExtremumDirection.Maximum).Head.ToFin(key.InvalidResult()))
            .Bind(sample => AnalysisAcceptance.AcceptValue(key: key, value: sample));
    internal static Fin<Distribution> ConformanceResidualDistribution(Seq<ResidualSample> samples, Seq<double> percentiles, Op key) =>
        ConformanceResidualDistances(samples: samples, key: key)
            .Bind(distances => Distribution.Of(values: distances, percentiles: percentiles, key: key));
    private static Fin<Seq<ResidualSample>> ValidateResiduals(Seq<ResidualSample> samples, Op key) =>
        samples.TraverseM(sample => AnalysisAcceptance.AcceptValue(key: key, value: sample)).As();
    private static bool CanConform(ConformanceMetric metric, Type geometry, Type target) =>
        geometry == typeof(object) || target == typeof(object)
        || (GeometryKernel.CanCurveForm(type: geometry) && metric.AcceptsTarget(target: target, curveSource: true))
        || (GeometryKernel.CanSurfaceForm(type: geometry) && metric.AcceptsTarget(target: target, curveSource: false));
    private static Fin<double> ConformanceDistanceFor(ConformanceMetric metric, object target, Point3d point, Context context, Op key) =>
        from space in SupportSpace.Of(value: target, key: key)
        let projection = metric.IsContainment ? SupportProjection.ContainmentDistance : metric.IsSigned ? SupportProjection.SignedDistance : SupportProjection.Distance
        from intent in VectorIntent.Support(space: space, sample: point, projection: projection, key: key)
        from distance in intent.Project<double>(context: context, key: key)
        select distance;
    private static Fin<Seq<ResidualSample>> ConformanceSampleResiduals<TGeometry, TPrimitive>(TGeometry geometry, TPrimitive primitive, int count, Context context, Op key, Func<TGeometry, int, Context, Op, Fin<Seq<Point3d>>> sampler, Func<TPrimitive, Point3d, Context, Fin<double>> distance) where TGeometry : notnull where TPrimitive : notnull =>
        sampler(arg1: geometry, arg2: count, arg3: context, arg4: key)
            .Bind(points => points.Map((p, i) => distance(arg1: primitive, arg2: p, arg3: context).Map(d => new ResidualSample(i, p, d, context.Absolute.Value, Math.Abs(d) <= context.Absolute.Value))).TraverseM(identity).As());
    private static Fin<Seq<ResidualSample>> ConformanceCurveCurveSamples(ConformanceMetric metric, int count, object curveLike, object targetCurveLike, Context context, Op key) =>
        GeometryKernel.CurveForm(source: curveLike, op: key)
            .Bind(leftLease => GeometryKernel.CurveForm(source: targetCurveLike, op: key)
                .Bind(rightLease => leftLease.Use(left => rightLease.Use(right => metric.ExactCurveDeviation switch {
                    true => CurveDeviationOf(left: left, right: right, context: context, op: key).Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance))),
                    false => ConformanceSampleResiduals(left, right, count, context, key, sampler: GeometryKernel.SamplePoints, distance: (c, pt, model) => ConformanceDistanceFor(metric: metric, target: c, point: pt, context: model, key: key)),
                }))));
    private static Fin<Seq<ResidualSample>> ConformanceSamples<TGeometry, TTarget>(ConformanceMetric metric, int count, TGeometry geometry, TTarget target, Context context, Op key) where TGeometry : notnull where TTarget : notnull =>
        (geometry, target) switch {
            (object curveLike, object targetCurveLike) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) && GeometryKernel.CanCurveForm(type: targetCurveLike.GetType()) => ConformanceCurveCurveSamples(metric, count, curveLike, targetCurveLike, context, key),
            (object curveLike, _) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) => GeometryKernel.CurveForm(source: curveLike, op: key).Bind(lease => lease.Use(curve => ConformanceSampleResiduals(curve, target, count, context, key, sampler: GeometryKernel.SamplePoints, distance: (t, pt, model) => ConformanceDistanceFor(metric: metric, target: t, point: pt, context: model, key: key)))),
            (object surfaceLike, _) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) => GeometryKernel.SurfaceForm(source: surfaceLike, op: key).Bind(lease => lease.Use(surface => ConformanceSampleResiduals(surface, target, count, context, key, GeometryKernel.SamplePoints, distance: (t, pt, model) => ConformanceDistanceFor(metric: metric, target: t, point: pt, context: model, key: key)))),
            _ => Fin.Fail<Seq<ResidualSample>>(key.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Operation<(TGeometry Geometry, TTarget Target), TValue> ConformancePair<TGeometry, TTarget, TValue>(ConformanceMetric metric, int count, Seq<double> percentiles, Op key) where TGeometry : notnull where TTarget : notnull =>
        Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: key, requiresContext: true,
            state: (Metric: metric, Count: count, Percentiles: percentiles, Key: key),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: state.Key, requirements: (op, kindG, kindT) =>
                    guard(kindG.Topology == Topology.Curve || kindG.Topology == Topology.Surface, op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))).ToFin()
                        .Map(_ => (A: Requirement.ForKind(kind: kindG), B: state.Metric.TargetRequirement(kind: kindT))), cancel: runtime.Cancellation).ToEff()
                from residuals in ConformanceSamples(metric: state.Metric, count: state.Count, geometry: resolved.A, target: resolved.B, context: runtime.Context, key: state.Key).ToEff()
                from result in state.Metric.Project<TValue>(residuals: residuals, percentiles: state.Percentiles, context: runtime.Context, key: state.Key).ToEff()
                select result);
}
