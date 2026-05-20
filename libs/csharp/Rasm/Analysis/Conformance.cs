using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, SmartEnum<int>]
public sealed partial class ConformanceMetric {
    public static readonly ConformanceMetric Distance = new(key: 0, output: typeof(double), isSigned: false, isContainment: false, exactCurveDeviation: false);
    public static readonly ConformanceMetric Rms = new(key: 1, output: typeof(double), isSigned: false, isContainment: false, exactCurveDeviation: false);
    public static readonly ConformanceMetric WithinTolerance = new(key: 2, output: typeof(bool), isSigned: false, isContainment: false, exactCurveDeviation: true);
    public static readonly ConformanceMetric Summary = new(key: 3, output: typeof(Stat), isSigned: false, isContainment: false, exactCurveDeviation: false);
    public static readonly ConformanceMetric Maximum = new(key: 4, output: typeof(ResidualSample), isSigned: false, isContainment: false, exactCurveDeviation: true);
    public static readonly ConformanceMetric SignedResidual = new(key: 5, output: typeof(ResidualSample), isSigned: true, isContainment: false, exactCurveDeviation: false);
    public static readonly ConformanceMetric Containment = new(key: 6, output: typeof(ResidualSample), isSigned: true, isContainment: true, exactCurveDeviation: false);
    public static readonly ConformanceMetric Distribution = new(key: 7, output: typeof(Distribution), isSigned: false, isContainment: false, exactCurveDeviation: false);
    public Type Output { get; }
    internal bool IsSigned { get; }
    internal bool IsContainment { get; }
    internal bool ExactCurveDeviation { get; }
    internal bool AcceptsTarget(Type target, bool curveSource) =>
        target == typeof(Plane) || target == typeof(Sphere) || target == typeof(Box) || target == typeof(BoundingBox)
        || (IsContainment && (target == typeof(Brep) || target == typeof(Mesh)))
        || (Equals(SignedResidual) && typeof(Brep).IsAssignableFrom(target))
        || (curveSource && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Polyline) || GeometryKernel.CanCurveForm(type: target)))
        || GeometryKernel.CanSurfaceForm(type: target);
    internal Requirement TargetRequirement(Kind kind) =>
        IsContainment && (kind.Topology == Topology.Brep || kind.Topology == Topology.Mesh) ? Requirement.SolidTopology : Requirement.None;
    internal Fin<Seq<TOut>> Project<TOut>(Conformance request, Seq<ResidualSample> residuals, Context context) =>
        this switch {
            ConformanceMetric metric when metric.Equals(Distance) => Stat.Residuals<Seq<double>>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Distances).Bind(values => Conformance.Key.AcceptResults<double, TOut>(values: values)),
            ConformanceMetric metric when metric.Equals(Rms) => Stat.Residuals<Stat>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Conformance.Key.AcceptResults<double, TOut>(values: Seq(stat.Rms))),
            ConformanceMetric metric when metric.Equals(WithinTolerance) => Stat.Residuals<Stat>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Conformance.Key.AcceptResults<bool, TOut>(values: Seq(stat.WithinTolerance))),
            ConformanceMetric metric when metric.Equals(Summary) => Stat.Residuals<Stat>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Summary(tolerance: context.Absolute.Value)).Bind(stat => Conformance.Key.AcceptResults<Stat, TOut>(values: Seq(stat))),
            ConformanceMetric metric when metric.Equals(Maximum) => Stat.Residuals<ResidualSample>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Maximum).Bind(sample => Conformance.Key.AcceptResults<ResidualSample, TOut>(values: Seq(sample))),
            ConformanceMetric metric when metric.Equals(SignedResidual) || metric.Equals(Containment) => Conformance.Key.AcceptResults<ResidualSample, TOut>(values: residuals),
            ConformanceMetric metric when metric.Equals(Distribution) => Stat.Residuals<Distribution>(samples: residuals, key: Conformance.Key, aggregate: ResidualAggregate.Distribution(percentiles: request.Percentiles)).Bind(result => Conformance.Key.AcceptResults<Distribution, TOut>(values: Seq(result))),
            _ => Fin.Fail<Seq<TOut>>(Conformance.Key.Unsupported(geometryType: typeof(ConformanceMetric), outputType: typeof(TOut))),
        };
}

public sealed record Conformance {
    private Conformance(ConformanceMetric metric, int count, Seq<double> percentiles) {
        Metric = metric;
        Count = count;
        Percentiles = percentiles;
    }
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    public ConformanceMetric Metric { get; }
    public int Count { get; }
    internal Seq<double> Percentiles { get; }
    public static Conformance Distance(int count) => new(metric: ConformanceMetric.Distance, count: count, percentiles: Seq<double>());
    public static Conformance Rms(int count) => new(metric: ConformanceMetric.Rms, count: count, percentiles: Seq<double>());
    public static Conformance WithinTolerance(int count) => new(metric: ConformanceMetric.WithinTolerance, count: count, percentiles: Seq<double>());
    public static Conformance Summary(int count) => new(metric: ConformanceMetric.Summary, count: count, percentiles: Seq<double>());
    public static Conformance Maximum(int count) => new(metric: ConformanceMetric.Maximum, count: count, percentiles: Seq<double>());
    public static Conformance SignedResidual(int count) => new(metric: ConformanceMetric.SignedResidual, count: count, percentiles: Seq<double>());
    public static Conformance Containment(int count) => new(metric: ConformanceMetric.Containment, count: count, percentiles: Seq<double>());
    public static Conformance Distribution(int count, params double[] percentiles) => new(metric: ConformanceMetric.Distribution, count: count, percentiles: toSeq(percentiles));
    public Operation<(TGeometry Geometry, TTarget Target), TOut> Operation<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (Count, CanConform(aspect: this, geometry: typeof(TGeometry), target: typeof(TTarget)), typeof(TOut) == Metric.Output) switch {
            ( <= 0, _, _) => Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true, true) => Pair<TGeometry, TTarget, TOut>(this),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    internal Fin<Seq<TOut>> Project<TOut>(Seq<ResidualSample> residuals, Context context) =>
        Metric.Project<TOut>(request: this, residuals: residuals, context: context);
    private static bool CanConform(Conformance aspect, Type geometry, Type target) =>
        geometry == typeof(object) || target == typeof(object)
        || (GeometryKernel.CanCurveForm(type: geometry) && aspect.Metric.AcceptsTarget(target: target, curveSource: true))
        || (GeometryKernel.CanSurfaceForm(type: geometry) && aspect.Metric.AcceptsTarget(target: target, curveSource: false));
    private static Operation<(TGeometry Geometry, TTarget Target), TValue> Pair<TGeometry, TTarget, TValue>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Key, requiresContext: true,
            state: aspect,
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: Key, requirements: (op, kindG, kindT) =>
                    (kindG.Topology == Topology.Curve || kindG.Topology == Topology.Surface)
                        ? Fin.Succ((A: Requirement.ForKind(kind: kindG), B: state.Metric.TargetRequirement(kind: kindT)))
                        : Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))), cancel: runtime.Cancellation).ToEff()
                from residuals in Samples(aspect: state, geometry: resolved.A, target: resolved.B, context: runtime.Context).ToEff()
                from result in state.Project<TValue>(residuals: residuals, context: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<ResidualSample>> Samples<TGeometry, TTarget>(Conformance aspect, TGeometry geometry, TTarget target, Context context) where TGeometry : notnull where TTarget : notnull =>
        (geometry, target) switch {
            (object curveLike, object targetCurveLike) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) && GeometryKernel.CanCurveForm(type: targetCurveLike.GetType()) => CurveCurveSamples(aspect, curveLike, targetCurveLike, context),
            (object curveLike, _) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, target, aspect.Count, context, sampler: GeometryKernel.SamplePoints, distance: (t, pt) => DistanceFor(metric: aspect.Metric, target: t, point: pt, tolerance: context.Absolute.Value)))),
            (object surfaceLike, _) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) =>
                GeometryKernel.SurfaceForm(source: surfaceLike, op: Key).Bind(lease => lease.Use(surface =>
                    SampleResiduals(surface, target, aspect.Count, context, GeometryKernel.SamplePoints, distance: (t, pt) => DistanceFor(metric: aspect.Metric, target: t, point: pt, tolerance: context.Absolute.Value)))),
            _ => Fin.Fail<Seq<ResidualSample>>(Key.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Fin<Seq<ResidualSample>> CurveCurveSamples(Conformance aspect, object curveLike, object targetCurveLike, Context context) =>
        GeometryKernel.CurveForm(source: curveLike, op: Key)
            .Bind(leftLease => GeometryKernel.CurveForm(source: targetCurveLike, op: Key)
                .Bind(rightLease => leftLease.Use(left => rightLease.Use(right => aspect.Metric.ExactCurveDeviation switch {
                    true => Analyze.CurveDeviationOf(left: left, right: right, context: context, op: Key)
                        .Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance))),
                    false => SampleResiduals(left, right, aspect.Count, context, sampler: GeometryKernel.SamplePoints, distance: static (c, pt) => c.ClosestPoint(testPoint: pt, t: out double t) ? Fin.Succ(pt.DistanceTo(c.PointAt(t: t))) : Fin.Fail<double>(Key.InvalidResult())),
                }))));
    private static Fin<Seq<ResidualSample>> SampleResiduals<TGeometry, TPrimitive>(TGeometry geometry, TPrimitive primitive, int count, Context context, Func<TGeometry, int, Context, Op, Fin<Seq<Point3d>>> sampler, Func<TPrimitive, Point3d, Fin<double>> distance) where TGeometry : notnull where TPrimitive : notnull =>
        sampler(arg1: geometry, arg2: count, arg3: context, arg4: Key)
            .Bind(points => points.Map((p, i) => distance(arg1: primitive, arg2: p).Map(d => new ResidualSample(i, p, d, context.Absolute.Value, Math.Abs(d) <= context.Absolute.Value))).TraverseM(identity).As());
    private static Fin<double> DistanceFor(ConformanceMetric metric, object target, Point3d point, double tolerance) =>
        (metric, target) switch {
            (ConformanceMetric m, Plane plane) when m.IsSigned => Fin.Succ(plane.DistanceTo(testPoint: point)),
            (ConformanceMetric m, Sphere sphere) when m.IsSigned => Fin.Succ(point.DistanceTo(sphere.Center) - sphere.Radius),
            (ConformanceMetric m, Box box) when m.IsSigned => Fin.Succ((box.Contains(point, false) ? -1.0 : 1.0) * point.DistanceTo(box.ClosestPoint(point, false))),
            (ConformanceMetric m, BoundingBox bbox) when m.IsSigned => Fin.Succ((bbox.Contains(point) ? -1.0 : 1.0) * point.DistanceTo(bbox.ClosestPoint(point, false))),
            (ConformanceMetric m, Brep { IsSolid: true } brep) when m.IsContainment => GeometryKernel.ClosestOf(geometry: brep, target: point, key: Key)
                .Bind(hit => hit.Distance.ToFin(Fail: Key.InvalidResult()))
                .Map(distance => (brep.IsPointInside(point, tolerance, false) ? -1.0 : 1.0) * distance),
            (ConformanceMetric m, Brep brep) when m.IsSigned => GeometryKernel.ClosestOf(geometry: brep, target: point, key: Key).Bind(hit => hit.SignedDistanceFrom(sample: point, key: Key)),
            (ConformanceMetric m, Mesh mesh) when m.IsContainment => GeometryKernel.ClosestOf(geometry: mesh, target: point, key: Key)
                .Bind(hit => mesh.IsSolid switch {
                    true => hit.Distance.ToFin(Fail: Key.InvalidResult()).Map(distance => (mesh.IsPointInside(point, tolerance, false) ? -1.0 : 1.0) * distance),
                    false => hit.SignedDistanceFrom(sample: point, key: Key),
                }),
            (ConformanceMetric m, object surfaceLike) when m.IsSigned && GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) =>
                GeometryKernel.SurfaceForm(source: surfaceLike, op: Key).Bind(lease => lease.Use(surface => GeometryKernel.ClosestOf(geometry: surface, target: point, key: Key).Bind(hit => hit.SignedDistanceFrom(sample: point, key: Key)))),
            (_, object surfaceLike) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) => GeometryKernel.SurfaceForm(source: surfaceLike, op: Key).Bind(lease => lease.Use(surface => GeometryKernel.ClosestOf(geometry: surface, target: point, key: Key).Bind(hit => hit.Distance.ToFin(Fail: Key.InvalidResult())))),
            _ => GeometryKernel.ClosestOf(geometry: target, target: point, key: Key).Bind(hit => hit.Distance.ToFin(Fail: Key.InvalidResult())),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.Operation<TGeometry, TTarget, TOut>() ?? Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
}
