namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return Dispatch.Supports(CapTag.Intersect, typeof(TA), typeof(TB), unordered: true) switch {
            true => Query<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.ValidatePair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation).ToEff()
                                                from result in Dispatch.Resolve<IntersectionResult, (Context, Op, CancellationToken, IProgress<double>?)>(CapTag.Intersect, resolved.A, resolved.B, (runtime.Context, op, runtime.Cancellation, runtime.Progress), op, unordered: true).ToEff()
                                                from typed in IntersectionResultRole.Project<TOut>(result: result, key: op).ToEff()
                                                select typed),
            false => key.Unsupported<(TA A, TB B), TOut>(),
        };
    }
    public static Query<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (typeof(Curve).IsAssignableFrom(c: typeof(TA)) && typeof(Curve).IsAssignableFrom(c: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
            ? Query<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.ValidatePair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.CurveLength, B: Requirement.CurveLength)), cancel: runtime.Cancellation).ToEff()
                                                from result in DeviationProject<TOut>(op: op, left: (Curve)(object)resolved.A, right: (Curve)(object)resolved.B, context: runtime.Context).ToEff()
                                                select result)
            : key.Unsupported<(TA A, TB B), TOut>();
    }
    // BOUNDARY ADAPTER — Rhino emits 6 out parameters; collapse into one CurveDeviation value object.
    private static Fin<Seq<TOut>> DeviationProject<TOut>(Op op, Curve left, Curve right, Context context) =>
        Curve.GetDistancesBetweenCurves(curveA: left, curveB: right, tolerance: context.Absolute.Value, maxDistance: out double maxDist, maxDistanceParameterA: out double maxA, maxDistanceParameterB: out double maxB, minDistance: out double minDist, minDistanceParameterA: out double minA, minDistanceParameterB: out double minB) switch {
            true => (op.RequireValid(value: minDist), op.RequireValid(value: maxDist), op.RequireValid(value: left.PointAt(t: minA)), op.RequireValid(value: right.PointAt(t: minB)), op.RequireValid(value: left.PointAt(t: maxA)), op.RequireValid(value: right.PointAt(t: maxB)))
                .Apply((minD, maxD, mA, mB, xA, xB) => new CurveDeviation(MinimumDistance: minD, MinimumA: mA, MinimumB: mB, MaximumDistance: maxD, MaximumA: xA, MaximumB: xB, Tolerance: context.Absolute.Value, WithinTolerance: maxD <= context.Absolute.Value))
                .As()
                .Bind(deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                    (true, true) => op.Results<CurveDeviation, TOut>(values: Seq(deviation)),
                    _ => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
                }),
            false => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
        };
}

// --- [COMPOSITION] ------------------------------------------------------------------------
internal static class IntersectionResultRole {
    internal static Fin<Seq<TOut>> Project<TOut>(this IntersectionResult result, Op key) => result.Switch(
        state: key,
        curves: static (k, c) => ProjectUniform<Curve, TOut>(key: k, caseType: typeof(IntersectionResult.Curves), values: c.Values, tag: IntersectionKind.Curve),
        lines: static (k, l) => ProjectUniform<Line, TOut>(key: k, caseType: typeof(IntersectionResult.Lines), values: l.Values, tag: IntersectionKind.Curve),
        circles: static (k, c) => ProjectUniform<Circle, TOut>(key: k, caseType: typeof(IntersectionResult.Circles), values: c.Values, tag: IntersectionKind.Curve),
        points: static (k, p) => ProjectUniform<Point3d, TOut>(key: k, caseType: typeof(IntersectionResult.Points), values: p.Values, tag: IntersectionKind.Point),
        intervals: static (k, i) => ProjectUniform<Interval, TOut>(key: k, caseType: typeof(IntersectionResult.Intervals), values: i.Values, tag: IntersectionKind.Overlap),
        polylines: static (k, p) => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => k.Results<Polyline, TOut>(values: p.Values.Map(static x => x.Curve)),
            Type t when t == typeof(IntersectionKind) => k.Results<IntersectionKind, TOut>(values: p.Values.Map(static x => x.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(IntersectionResult.Polylines), outputType: typeof(TOut))),
        },
        hits: static (k, h) => typeof(TOut) switch {
            Type t when t == typeof(IntersectionHit) => k.Results<IntersectionHit, TOut>(values: h.Values),
            Type t when t == typeof(Curve) => k.Results<Curve, TOut>(values: h.Values.Bind(static value => value.Curves)),
            Type t when t == typeof(Point3d) => k.Results<Point3d, TOut>(values: h.Values.Bind(static value => value.Points)),
            Type t when t == typeof(Interval) => k.Results<Interval, TOut>(values: h.Values.Bind(static value => value.Intervals)),
            Type t when t == typeof(IntersectionKind) => k.Results<IntersectionKind, TOut>(values: h.Values.Map(static value => value.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(IntersectionResult.Hits), outputType: typeof(TOut))),
        });
    private static Fin<Seq<TOut>> ProjectUniform<TNative, TOut>(Op key, Type caseType, Seq<TNative> values, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.Results<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: values.Map(_ => tag)),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(IntersectionResult.Hits), outputType: typeof(TOut))),
    };
}
