namespace Rasm.Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------
public static partial class Query {
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (typeof(TA).AsKind().IsSome && typeof(TB).AsKind().IsSome, Dispatch.SupportsUnorderedPair(table: Dispatch.IntersectTable, left: typeof(TA), right: typeof(TB))) switch {
            (true, true) => Query<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from kA in ((object)pair.A).Kind(ctx: runtime.Context).ToEff()
                                                from kB in ((object)pair.B).Kind(ctx: runtime.Context).ToEff()
                                                from result in kA.Intersect(b: kB, valueA: pair.A, valueB: pair.B, ctx: runtime.Context, op: op, progress: runtime.Progress, cancel: runtime.Cancellation).ToEff()
                                                from typed in IntersectionResultRole.Project<TOut>(result: result, key: op).ToEff()
                                                select typed),
            _ => key.Unsupported<(TA A, TB B), TOut>(),
        };
    }
    public static Query<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (typeof(Curve).IsAssignableFrom(c: typeof(TA)) && typeof(Curve).IsAssignableFrom(c: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
            ? Query<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from validated in runtime.Context.ValidatePair(a: pair.A, b: pair.B, requirementA: Requirement.CurveLength, requirementB: Requirement.CurveLength, cancel: runtime.Cancellation).ToEff()
                                                from result in DeviationProject<TOut>(op: op, left: (Curve)(object)validated.A, right: (Curve)(object)validated.B, ctx: runtime.Context).ToEff()
                                                select result)
            : key.Unsupported<(TA A, TB B), TOut>();
    }
    // BOUNDARY ADAPTER — Rhino emits 6 out parameters; collapse into one CurveDeviation value object.
    private static Fin<Seq<TOut>> DeviationProject<TOut>(Op op, Curve left, Curve right, Context ctx) =>
        Curve.GetDistancesBetweenCurves(curveA: left, curveB: right, tolerance: ctx.Absolute.Value, maxDistance: out double maxDist, maxDistanceParameterA: out double maxA, maxDistanceParameterB: out double maxB, minDistance: out double minDist, minDistanceParameterA: out double minA, minDistanceParameterB: out double minB) switch {
            true => (op.RequireValid(value: minDist), op.RequireValid(value: maxDist), op.RequireValid(value: left.PointAt(t: minA)), op.RequireValid(value: right.PointAt(t: minB)), op.RequireValid(value: left.PointAt(t: maxA)), op.RequireValid(value: right.PointAt(t: maxB)))
                .Apply((minD, maxD, mA, mB, xA, xB) => new CurveDeviation(MinimumDistance: minD, MinimumA: mA, MinimumB: mB, MaximumDistance: maxD, MaximumA: xA, MaximumB: xB, Tolerance: ctx.Absolute.Value, WithinTolerance: maxD <= ctx.Absolute.Value))
                .As()
                .Bind(deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                    (true, true) => op.Results<CurveDeviation, TOut>(values: Seq(deviation)),
                    _ => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
                }),
            false => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
        };
}

// --- [INTERSECTION_RESULT_ROLE] ----------------------------------------------------------
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
            Type t when t == typeof(Curve) => k.Results<Curve, TOut>(values: h.Values.Choose(static value => value.Curve)),
            Type t when t == typeof(Point3d) => k.Results<Point3d, TOut>(values: h.Values.Choose(static value => value.Point)),
            Type t when t == typeof(Interval) => k.Results<Interval, TOut>(values: h.Values.Bind(static value => value.OverlapA.ToSeq() + value.OverlapB.ToSeq())),
            Type t when t == typeof(IntersectionKind) => k.Results<IntersectionKind, TOut>(values: h.Values.Map(static value => value.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(IntersectionResult.Hits), outputType: typeof(TOut))),
        });
    private static Fin<Seq<TOut>> ProjectUniform<TNative, TOut>(Op key, Type caseType, Seq<TNative> values, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.Results<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: values.Map(_ => tag)),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: caseType, outputType: typeof(TOut))),
    };
}
