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
    public sealed record ContainmentCase(int Count) : Conformance;
    public sealed record DistributionCase(int Count, Seq<double> Percentiles) : Conformance;
    public static Conformance Distance(int count) => new DistanceCase(Count: count);
    public static Conformance Rms(int count) => new RmsCase(Count: count);
    public static Conformance WithinTolerance(int count) => new WithinToleranceCase(Count: count);
    public static Conformance Summary(int count) => new SummaryCase(Count: count);
    public static Conformance Maximum(int count) => new MaximumCase(Count: count);
    public static Conformance SignedResidual(int count) => new SignedResidualCase(Count: count);
    public static Conformance Containment(int count) => new ContainmentCase(Count: count);
    public static Conformance Distribution(int count, params double[] percentiles) => new DistributionCase(Count: count, Percentiles: toSeq(percentiles));
    internal static readonly Op Key = Op.Of(name: nameof(Conformance));
    internal Type OutputType => Switch(
        distanceCase: static _ => typeof(double),
        rmsCase: static _ => typeof(double),
        withinToleranceCase: static _ => typeof(bool),
        summaryCase: static _ => typeof(Stat),
        maximumCase: static _ => typeof(ResidualSample),
        signedResidualCase: static _ => typeof(ResidualSample),
        containmentCase: static _ => typeof(ResidualSample),
        distributionCase: static _ => typeof(Distribution));
    internal int SampleCount => Switch(
        distanceCase: static d => d.Count,
        rmsCase: static r => r.Count,
        withinToleranceCase: static w => w.Count,
        summaryCase: static s => s.Count,
        maximumCase: static m => m.Count,
        signedResidualCase: static sr => sr.Count,
        containmentCase: static c => c.Count,
        distributionCase: static d => d.Count);
    public Operation<(TGeometry Geometry, TTarget Target), TOut> Operation<TGeometry, TTarget, TOut>() where TGeometry : notnull where TTarget : notnull =>
        (SampleCount, CanConform(aspect: this, geometry: typeof(TGeometry), target: typeof(TTarget)), typeof(TOut) == OutputType) switch {
            ( <= 0, _, _) => Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Key, fault: Key.InvalidInput()),
            (_, true, true) => Pair<TGeometry, TTarget, TOut>(this, SampleCount),
            _ => Key.Unsupported<(TGeometry Geometry, TTarget Target), TOut>(),
        };
    internal Fin<Seq<TOut>> Project<TOut>(Seq<ResidualSample> residuals, Context context) =>
        Switch(
            state: (Residuals: residuals, Context: context),
            distanceCase: static (state, _) => Stat.Residuals<Seq<double>>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Distances).Bind(values => Key.AcceptResults<double, TOut>(values: values)),
            rmsCase: static (state, _) => Stat.Residuals<Stat>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: state.Context.Absolute.Value)).Bind(stat => Key.AcceptResults<double, TOut>(values: Seq(stat.Rms))),
            withinToleranceCase: static (state, _) => Stat.Residuals<Stat>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: state.Context.Absolute.Value)).Bind(stat => Key.AcceptResults<bool, TOut>(values: Seq(stat.WithinTolerance))),
            summaryCase: static (state, _) => Stat.Residuals<Stat>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Summary(tolerance: state.Context.Absolute.Value)).Bind(stat => Key.AcceptResults<Stat, TOut>(values: Seq(stat))),
            maximumCase: static (state, _) => Stat.Residuals<ResidualSample>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Maximum).Bind(sample => Key.AcceptResults<ResidualSample, TOut>(values: Seq(sample))),
            signedResidualCase: static (state, _) => Key.AcceptResults<ResidualSample, TOut>(values: state.Residuals),
            containmentCase: static (state, _) => Key.AcceptResults<ResidualSample, TOut>(values: state.Residuals),
            distributionCase: static (state, distribution) => Stat.Residuals<Distribution>(samples: state.Residuals, key: Key, aggregate: ResidualAggregate.Distribution(percentiles: distribution.Percentiles)).Bind(result => Key.AcceptResults<Distribution, TOut>(values: Seq(result))));
    private static bool CanConform(Conformance aspect, Type geometry, Type target) =>
        geometry == typeof(object) || target == typeof(object)
        || (GeometryKernel.CanCurveForm(type: geometry) && AcceptedTarget(aspect: aspect, target: target, curveSource: true))
        || (GeometryKernel.CanSurfaceForm(type: geometry) && AcceptedTarget(aspect: aspect, target: target, curveSource: false));
    private static bool AcceptedTarget(Conformance aspect, Type target, bool curveSource) =>
        target == typeof(Plane) || target == typeof(Sphere) || target == typeof(Box) || target == typeof(BoundingBox)
        || (aspect is ContainmentCase && (target == typeof(Brep) || target == typeof(Mesh)))
        || (curveSource && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Polyline) || GeometryKernel.CanCurveForm(type: target)))
        || GeometryKernel.CanSurfaceForm(type: target);
    private static Operation<(TGeometry Geometry, TTarget Target), TValue> Pair<TGeometry, TTarget, TValue>(Conformance aspect, int count) where TGeometry : notnull where TTarget : notnull =>
        Operation<(TGeometry Geometry, TTarget Target), TValue>.Build(
            key: Key, requiresContext: true,
            state: (Aspect: aspect, Count: count),
            evaluator: static (state, pair) =>
                from runtime in Env.EnvAsks
                from resolved in runtime.Context.Pair(a: pair.Geometry, b: pair.Target, op: Key, requirements: static (op, kindG, _) =>
                    (kindG.Topology == Topology.Curve || kindG.Topology == Topology.Surface)
                        ? Fin.Succ((A: Requirement.ForKind(kind: kindG), B: Requirement.None))
                        : Fin.Fail<(Requirement A, Requirement B)>(op.Unsupported(geometryType: kindG.Type, outputType: typeof(ResidualSample))), cancel: runtime.Cancellation).ToEff()
                from residuals in Samples(aspect: state.Aspect, geometry: resolved.A, target: resolved.B, count: state.Count, context: runtime.Context).ToEff()
                from result in state.Aspect.Project<TValue>(residuals: residuals, context: runtime.Context).ToEff()
                select result);
    private static Fin<Seq<ResidualSample>> Samples<TGeometry, TTarget>(Conformance aspect, TGeometry geometry, TTarget target, int count, Context context) where TGeometry : notnull where TTarget : notnull =>
        (geometry, target) switch {
            (object curveLike, object targetCurveLike) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) && GeometryKernel.CanCurveForm(type: targetCurveLike.GetType()) => CurveCurveSamples(aspect, curveLike, targetCurveLike, count, context),
            (object curveLike, _) when GeometryKernel.CanCurveForm(type: curveLike.GetType()) =>
                GeometryKernel.CurveForm(source: curveLike, op: Key).Bind(lease => lease.Use(curve =>
                    SampleResiduals(curve, target, count, context, sampler: GeometryKernel.SamplePoints, distance: (t, pt) => DistanceFor(aspect: aspect, target: t, point: pt, tolerance: context.Absolute.Value)))),
            (object surfaceLike, _) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) =>
                GeometryKernel.SurfaceForm(source: surfaceLike, op: Key).Bind(lease => lease.Use(surface =>
                    SampleResiduals(surface, target, count, context, GeometryKernel.SamplePoints, distance: (t, pt) => DistanceFor(aspect: aspect, target: t, point: pt, tolerance: context.Absolute.Value)))),
            _ => Fin.Fail<Seq<ResidualSample>>(Key.Unsupported(typeof(TGeometry), typeof(ResidualSample))),
        };
    private static Fin<Seq<ResidualSample>> CurveCurveSamples(Conformance aspect, object curveLike, object targetCurveLike, int count, Context context) =>
        GeometryKernel.CurveForm(source: curveLike, op: Key)
            .Bind(leftLease => GeometryKernel.CurveForm(source: targetCurveLike, op: Key)
                .Bind(rightLease => leftLease.Use(left => rightLease.Use(right => aspect switch {
                    WithinToleranceCase or MaximumCase =>
                        Analyze.CurveDeviationOf(left: left, right: right, context: context, op: Key)
                            .Map(static d => Seq(new ResidualSample(Index: 0, Location: d.MaximumA, Distance: d.MaximumDistance, Tolerance: d.Tolerance, WithinTolerance: d.WithinTolerance))),
                    _ => SampleResiduals(left, right, count, context, sampler: GeometryKernel.SamplePoints, distance: static (c, pt) => c.ClosestPoint(testPoint: pt, t: out double t) ? Fin.Succ(pt.DistanceTo(c.PointAt(t: t))) : Fin.Fail<double>(Key.InvalidResult())),
                }))));
    private static Fin<Seq<ResidualSample>> SampleResiduals<TGeometry, TPrimitive>(TGeometry geometry, TPrimitive primitive, int count, Context context, Func<TGeometry, int, Context, Op, Fin<Seq<Point3d>>> sampler, Func<TPrimitive, Point3d, Fin<double>> distance) where TGeometry : notnull where TPrimitive : notnull =>
        sampler(arg1: geometry, arg2: count, arg3: context, arg4: Key)
            .Bind(points => points.Map((p, i) => distance(arg1: primitive, arg2: p).Map(d => new ResidualSample(i, p, d, context.Absolute.Value, Math.Abs(d) <= context.Absolute.Value))).TraverseM(identity).As());
    // Sign convention: positive=outside, negative=inside via Rhino's outward-normal axis. NaN propagates failure through Stat.Of.AllFinite.
    private static Fin<double> DistanceFor(Conformance aspect, object target, Point3d point, double tolerance) =>
        (aspect, target) switch {
            (SignedResidualCase or ContainmentCase, Plane plane) => Fin.Succ(plane.DistanceTo(testPoint: point)),
            (SignedResidualCase or ContainmentCase, Sphere sphere) => Fin.Succ(point.DistanceTo(sphere.Center) - sphere.Radius),
            (SignedResidualCase or ContainmentCase, Box box) => Fin.Succ((box.Contains(point, false) ? -1.0 : 1.0) * point.DistanceTo(box.ClosestPoint(point, false))),
            (SignedResidualCase or ContainmentCase, BoundingBox bbox) => Fin.Succ((bbox.Contains(point) ? -1.0 : 1.0) * point.DistanceTo(bbox.ClosestPoint(point, false))),
            (ContainmentCase, Brep brep) => brep.ClosestPoint(point, out Point3d brepClosest, out _, out _, out _, 0.0, out Vector3d brepNormal) ? Fin.Succ(brep.IsSolid switch { true => (brep.IsPointInside(point, tolerance, false) ? -1.0 : 1.0) * point.DistanceTo(brepClosest), false => (point - brepClosest) * brepNormal }) : Fin.Fail<double>(Key.InvalidResult()),
            (ContainmentCase, Mesh mesh) => mesh.ClosestPoint(point, out Point3d meshClosest, out Vector3d meshNormal, 0.0) >= 0 ? Fin.Succ(mesh.IsSolid switch { true => (mesh.IsPointInside(point, tolerance, false) ? -1.0 : 1.0) * point.DistanceTo(meshClosest), false => (point - meshClosest) * meshNormal }) : Fin.Fail<double>(Key.InvalidResult()),
            (object surfaceLike, _) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) => GeometryKernel.SurfaceForm(source: surfaceLike, op: Key).Bind(lease => lease.Use(surface => GeometryKernel.ClosestOf(geometry: surface, target: point, key: Key).Bind(hit => hit.Distance.ToFin(Fail: Key.InvalidResult())))),
            _ => GeometryKernel.ClosestOf(geometry: target, target: point, key: Key).Bind(hit => hit.Distance.ToFin(Fail: Key.InvalidResult())),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TGeometry Geometry, TTarget Target), TOut> Conformance<TGeometry, TTarget, TOut>(Conformance aspect) where TGeometry : notnull where TTarget : notnull =>
        aspect?.Operation<TGeometry, TTarget, TOut>() ?? Operation<(TGeometry Geometry, TTarget Target), TOut>.Reject(key: Op.Of(), fault: Op.Of().InvalidInput());
}
