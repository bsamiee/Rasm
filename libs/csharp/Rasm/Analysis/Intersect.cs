namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
internal partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult; public sealed record Lines(Seq<Line> Values) : IntersectionResult; public sealed record Circles(Seq<Circle> Values) : IntersectionResult; public sealed record Points(Seq<Point3d> Values) : IntersectionResult; public sealed record Intervals(Seq<Interval> Values) : IntersectionResult; public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult; public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
    internal Fin<Seq<TOut>> Project<TOut>(Op key) => Switch(
        state: key,
        curves: static (k, c) => UniformAs<Curve, TOut>(values: c.Values, key: k, caseType: typeof(Curves), tag: IntersectionKind.Curve),
        lines: static (k, l) => UniformAs<Line, TOut>(values: l.Values, key: k, caseType: typeof(Lines), tag: IntersectionKind.Curve),
        circles: static (k, c) => UniformAs<Circle, TOut>(values: c.Values, key: k, caseType: typeof(Circles), tag: IntersectionKind.Curve),
        points: static (k, p) => UniformAs<Point3d, TOut>(values: p.Values, key: k, caseType: typeof(Points), tag: IntersectionKind.Point),
        intervals: static (k, i) => UniformAs<Interval, TOut>(values: i.Values, key: k, caseType: typeof(Intervals), tag: IntersectionKind.Overlap),
        polylines: static (k, p) => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => k.AcceptResults<Polyline, TOut>(values: p.Values.Map(static x => x.Curve)),
            Type t when t == typeof(IntersectionKind) => k.AcceptResults<IntersectionKind, TOut>(values: p.Values.Map(static x => x.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(Polylines), outputType: typeof(TOut))),
        },
        hits: static (k, h) => HitsAs<TOut>(hits: h.Values, key: k));
    private static Fin<Seq<TOut>> HitsAs<TOut>(Seq<IntersectionHit> hits, Op key) => typeof(TOut) switch {
        Type t when t == typeof(IntersectionHit) => key.AcceptResults<IntersectionHit, TOut>(values: hits),
        Type t when t == typeof(Curve) => key.AcceptResults<Curve, TOut>(values: hits.Bind(static value => value.Curves)),
        Type t when t == typeof(Point3d) => DropHitCurves(hits: hits, result: key.AcceptResults<Point3d, TOut>(values: hits.Bind(static value => value.Points))),
        Type t when t == typeof(Interval) => DropHitCurves(hits: hits, result: key.AcceptResults<Interval, TOut>(values: hits.Bind(static value => value.Intervals))),
        Type t when t == typeof(IntersectionKind) => DropHitCurves(hits: hits, result: key.AcceptResults<IntersectionKind, TOut>(values: hits.Map(static value => value.Kind))),
        _ => DropHitCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(Hits), outputType: typeof(TOut)))),
    };
    private static Fin<Seq<TOut>> DropHitCurves<TOut>(Seq<IntersectionHit> hits, Fin<Seq<TOut>> result) {
        _ = hits.Iter(static value => value.Dispose());
        return result;
    }
    private static Fin<Seq<TOut>> UniformAs<TNative, TOut>(Seq<TNative> values, Op key, Type caseType, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.AcceptResults<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.AcceptResults<IntersectionKind, TOut>(values: toSeq(Enumerable.Repeat(element: tag, count: values.Count))),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: caseType, outputType: typeof(TOut))),
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (Supports(left: typeof(TA), right: typeof(TB), unordered: true), Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), unordered: true)) switch {
            (true, true) => global::Rasm.Analysis.Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation).ToEff()
                                                from result in IntersectionOf(left: resolved.A, right: resolved.B, context: runtime.Context, op: op, progress: runtime.Progress, unordered: true, cancel: runtime.Cancellation).ToEff()
                                                from typed in result.Project<TOut>(key: op).ToEff()
                                                select typed),
            _ => key.Unsupported<(TA A, TB B), TOut>(),
        };
    }
    public static global::Rasm.Analysis.Operation<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (CanDeviation(left: typeof(TA), right: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
            ? global::Rasm.Analysis.Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.CurveLength, B: Requirement.CurveLength)), cancel: runtime.Cancellation).ToEff()
                                                from deviation in DeviationOf(left: resolved.A, right: resolved.B, context: runtime.Context, op: op).ToEff()
                                                from result in op.AcceptResults<CurveDeviation, TOut>(values: Seq(deviation)).ToEff()
                                                select result)
            : key.Unsupported<(TA A, TB B), TOut>();
    }
    internal static bool Supports(Type left, Type right, Type? output = null, bool unordered = false) =>
        (left == typeof(object) || right == typeof(object), output) switch {
            (true, null) => true,
            (true, Type o) => AnyIntersectionOutput(output: o),
            (false, null) => CanIntersectOrdered(left: left, right: right) || (unordered && CanIntersectOrdered(left: right, right: left)),
            (false, Type o) => CanProjectIntersectionOrdered(left: left, right: right, output: o) || (unordered && CanProjectIntersectionOrdered(left: right, right: left, output: o)),
        };
    internal static bool CanDeviation(Type left, Type right) =>
        typeof(Curve).IsAssignableFrom(left) && typeof(Curve).IsAssignableFrom(right);
    private static bool CanProjectIntersectionOrdered(Type left, Type right, Type output) =>
        (left, right, output) switch {
            (Type l, Type r, Type o) when CanIntersectOrdered(left: l, right: r) && o == typeof(IntersectionKind) => true,
            (Type l, Type r, Type o) when l == typeof(Line) && (r == typeof(Plane) || r == typeof(Circle) || r == typeof(Sphere)) => o == typeof(Point3d),
            (Type l, Type r, Type o) when l == typeof(Mesh) && r == typeof(Line) => o == typeof(Point3d),
            (Type l, Type r, Type o) when l == typeof(Plane) && r == typeof(Plane) => o == typeof(Line),
            (Type l, Type r, Type o) when l == typeof(Line) && (r == typeof(BoundingBox) || r == typeof(Box)) => o == typeof(Interval),
            (Type l, Type r, Type o) when l == typeof(Mesh) && (r == typeof(Plane) || typeof(Mesh).IsAssignableFrom(r)) => o == typeof(Polyline),
            (Type l, Type r, Type o) when typeof(Curve).IsAssignableFrom(l) && (typeof(Curve).IsAssignableFrom(r) || r == typeof(Plane) || r == typeof(Line) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || typeof(BrepFace).IsAssignableFrom(r)) => HitsOutput(output: o),
            (Type l, Type r, Type o) when typeof(Surface).IsAssignableFrom(l) && typeof(Surface).IsAssignableFrom(r) => HitsOutput(output: o),
            (Type l, Type r, Type o) when typeof(Brep).IsAssignableFrom(l) && (r == typeof(Plane) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r)) => HitsOutput(output: o),
            _ => false,
        };
    private static bool HitsOutput(Type output) =>
        output == typeof(IntersectionHit) || output == typeof(Curve) || output == typeof(Point3d) || output == typeof(Interval);
    private static bool AnyIntersectionOutput(Type output) =>
        HitsOutput(output: output) || output == typeof(IntersectionKind) || output == typeof(Line) || output == typeof(Polyline);
    private static bool CanIntersectOrdered(Type left, Type right) =>
        (left, right) switch {
            (Type l, Type r) when l == typeof(Line) && r == typeof(Plane) => true,
            (Type l, Type r) when l == typeof(Plane) && r == typeof(Plane) => true,
            (Type l, Type r) when l == typeof(Line) && (r == typeof(Circle) || r == typeof(Sphere) || r == typeof(BoundingBox) || r == typeof(Box)) => true,
            (Type l, Type r) when typeof(Curve).IsAssignableFrom(l) && (typeof(Curve).IsAssignableFrom(r) || r == typeof(Plane) || r == typeof(Line) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || typeof(BrepFace).IsAssignableFrom(r)) => true,
            (Type l, Type r) when typeof(Surface).IsAssignableFrom(l) && typeof(Surface).IsAssignableFrom(r) => true,
            (Type l, Type r) when typeof(Brep).IsAssignableFrom(l) && (r == typeof(Plane) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r)) => true,
            (Type l, Type r) when typeof(Mesh).IsAssignableFrom(l) && (r == typeof(Line) || r == typeof(Plane) || typeof(Mesh).IsAssignableFrom(r)) => true,
            _ => false,
        };
    internal static Fin<IntersectionResult> IntersectionOf<TL, TR>(TL left, TR right, Context context, Op op, IProgress<double>? progress, bool unordered, CancellationToken cancel) where TL : notnull where TR : notnull =>
        (Optional(left).ToFin(op.InvalidInput()), Optional(right).ToFin(op.InvalidInput())).Apply((l, r) => (L: (object)l, R: (object)r)).As()
            .Bind(pair => IntersectOrdered(left: pair.L, right: pair.R, context: context, op: op, cancel: cancel, progress: progress)
                .BindFail(error => (unordered, error) switch {
                    (true, Fault.Unsupported) => IntersectOrdered(left: pair.R, right: pair.L, context: context, op: op, cancel: cancel, progress: progress),
                    _ => Fin.Fail<IntersectionResult>(error),
                }));
    internal static Fin<CurveDeviation> DeviationOf<TL, TR>(TL left, TR right, Context context, Op op) where TL : notnull where TR : notnull =>
        (left, right) switch {
            (Curve a, Curve b) => CurveDeviationOf(left: a, right: b, context: context, op: op),
            _ => Fin.Fail<CurveDeviation>(op.Unsupported(typeof(TL), typeof(TR))),
        };
    private static Fin<IntersectionResult> IntersectOrdered(object left, object right, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) =>
        (left, right) switch {
            (Line a, Plane b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LinePlane(a, b, out double t) && t is >= 0.0 and <= 1.0 ? Seq(a.PointAt(t)) : Seq<Point3d>())),
            (Plane a, Plane b) => Fin.Succ((IntersectionResult)new IntersectionResult.Lines(Intersection.PlanePlane(a, b, out Line line) ? Seq(line) : Seq<Line>())),
            (Line a, Circle b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineCircle(a, b, out double t1, out Point3d p1, out double t2, out Point3d p2) switch {
                LineCircleIntersection.Single when t1 is >= 0.0 and <= 1.0 => Seq(p1),
                LineCircleIntersection.Multiple => Seq((T: t1, Point: p1), (T: t2, Point: p2)).Where(static p => p.T is >= 0.0 and <= 1.0).Map(static p => p.Point),
                _ => Seq<Point3d>(),
            })),
            (Line a, Sphere b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineSphere(a, b, out Point3d p1, out Point3d p2) switch {
                LineSphereIntersection.Single when OnFiniteLine(line: a, point: p1, tolerance: context.Absolute.Value) => Seq(p1),
                LineSphereIntersection.Multiple => Seq(p1, p2).Where(p => OnFiniteLine(line: a, point: p, tolerance: context.Absolute.Value)),
                _ => Seq<Point3d>(),
            })),
            (Line a, BoundingBox b) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, context.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>())),
            (Line a, Box b) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, context.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>())),
            (Curve a, Curve b) when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            (Curve a, Curve b) => new Lease<CurveIntersections>.Owned(Value: Intersection.CurveCurve(a, b, context.Absolute.Value, context.Absolute.Value)).Use(hits => cancel.IsCancellationRequested ? Fin.Fail<IntersectionResult>(new Fault.Cancelled()) : HitsFromEvents(hits: hits, op: op, source: a)),
            (Curve a, Plane b) => CurveAgainst<Plane>(a: a, b: b, context: context, op: op, intersect: static (c, p, t) => Intersection.CurvePlane(c, p, t)),
            (Curve a, Line b) => CurveAgainst<Line>(a: a, b: b, context: context, op: op, intersect: static (c, l, t) => Intersection.CurveLine(c, l, t, t), finiteLine: Some(b)),
            (Curve a, BrepFace b) => HitsFromSolved(solved: Intersection.CurveBrepFace(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel),
            (Curve a, Brep b) => HitsFromSolved(solved: Intersection.CurveBrep(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel, partial: true),
            (Curve a, Surface b) => CurveAgainst<Surface>(a: a, b: b, context: context, op: op, intersect: static (c, s, t) => Intersection.CurveSurface(c, s, t, t)),
            (Surface a, Surface b) => HitsFromSolved(solved: Intersection.SurfaceSurface(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Plane b) => HitsFromSolved(solved: Intersection.BrepPlane(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Surface b) => HitsFromSolved(solved: Intersection.BrepSurface(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Brep b) => HitsFromSolved(solved: Intersection.BrepBrep(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Mesh a, Line b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(toSeq(Intersection.MeshLineSorted(a, b, out int[] _) ?? []))),
            (Mesh a, Plane b) => MeshPlane(mesh: a, plane: b, context: context),
            (Mesh a, Mesh b) => MeshMesh(left: a, right: b, context: context, op: op, cancel: cancel, progress: progress),
            _ => Fin.Fail<IntersectionResult>(op.Unsupported(left.GetType(), right.GetType())),
        };
    private static bool OnFiniteLine(Line line, Point3d point, double tolerance) => point.IsValid && point.DistanceTo(other: line.ClosestPoint(testPoint: point, limitToFiniteSegment: true)) <= tolerance;
    private static Seq<Interval> SegmentInterval(Interval interval) =>
        (Math.Min(interval.T0, interval.T1), Math.Max(interval.T0, interval.T1)) switch {
            (double min, double max) when Math.Max(min, 0.0) <= Math.Min(max, 1.0) => Seq(new Interval(
                interval.T0 <= interval.T1 ? Math.Max(min, 0.0) : Math.Min(max, 1.0),
                interval.T0 <= interval.T1 ? Math.Min(max, 1.0) : Math.Max(min, 0.0))),
            _ => Seq<Interval>(),
        };
    private static Fin<IntersectionResult> HitsFromEvents(CurveIntersections? hits, Op op, Curve? source = null, Option<Line> finiteLine = default, double tolerance = 0.0) =>
        Optional(hits).ToFin(op.InvalidResult()).Map(native => (IntersectionResult)new IntersectionResult.Hits(toSeq(native.AsIterable().SelectMany(h => h switch {
            { IsPoint: true } when finiteLine.Map(l => OnFiniteLine(line: l, point: h.PointB, tolerance: tolerance)).IfNone(true) => Seq(IntersectionHit.At(h.PointA)),
            { IsOverlap: true } => (finiteLine.Case switch {
                Line => SegmentInterval(h.OverlapB).Head.Map(cb => (A: new Interval(h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T0)), h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T1))), B: cb)),
                _ => Some((A: h.OverlapA, B: h.OverlapB)),
            }).Map(o => Optional(source).Map(c => IntersectionHit.Overlap(c.PointAt(o.A.T0), c.PointAt(o.A.T1), o.A, o.B, Optional(c.Trim(o.A))))
                .IfNone(IntersectionHit.Overlap(h.PointA, h.PointA2, o.A, o.B))).ToSeq(),
            _ => Seq<IntersectionHit>(),
        }))));
    private static Fin<IntersectionResult> HitsFromSolved(bool solved, Curve[]? curves, Point3d[]? points, IntersectionKind kind, Op op, CancellationToken cancel, bool partial = false) =>
        (Curves: toSeq(curves ?? []), Points: toSeq(points ?? [])) switch {
            (Seq<Curve> cs, Seq<Point3d> ps) => (solved || (partial && (!cs.IsEmpty || !ps.IsEmpty)), cancel.IsCancellationRequested) switch {
                (_, true) => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
                (true, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(cs.Map(c => IntersectionHit.Along(c, kind)) + ps.Map(static p => IntersectionHit.At(p)))),
                _ => Fin.Fail<IntersectionResult>(op.InvalidResult()),
            },
        };
    private static Fin<IntersectionResult> CurveAgainst<TRight>(Curve a, TRight b, Context context, Op op, Func<Curve, TRight, double, CurveIntersections?> intersect, Option<Line> finiteLine = default) {
        using CurveIntersections? hits = intersect(arg1: a, arg2: b, arg3: context.Absolute.Value);
        return HitsFromEvents(hits: hits, op: op, source: a, finiteLine: finiteLine, tolerance: finiteLine.IsSome ? context.Absolute.Value : 0.0);
    }
    private static Fin<IntersectionResult> MeshPlane(Mesh mesh, Plane plane, Context context) {
        using MeshIntersectionCache cache = new();
        Polyline[]? polylines = Intersection.MeshPlane(mesh, cache, plane, context.MeshIntersectionTolerance, true);
        return Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(polylines).ToSeq().Bind(static h => h)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve))));
    }
    private static Fin<IntersectionResult> MeshMesh(Mesh left, Mesh right, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) {
        using TextLog log = new();
        return Intersection.MeshMesh([left, right], context.MeshIntersectionTolerance, out Polyline[] ints, true, out Polyline[] olap, false, out Mesh _, log, cancel, progress) switch {
            true => Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(ints).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)) + toSeq(Optional(olap).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Overlap)))),
            false when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            false => Fin.Fail<IntersectionResult>(op.InvalidResult()),
        };
    }
    private static Fin<CurveDeviation> CurveDeviationOf(Curve left, Curve right, Context context, Op op) =>
        Curve.GetDistancesBetweenCurves(curveA: left, curveB: right, tolerance: context.Absolute.Value, maxDistance: out double maxDist, maxDistanceParameterA: out double maxA, maxDistanceParameterB: out double maxB, minDistance: out double minDist, minDistanceParameterA: out double minA, minDistanceParameterB: out double minB) switch {
            true => (op.AcceptValue(value: minDist), op.AcceptValue(value: maxDist), op.AcceptValue(value: left.PointAt(t: minA)), op.AcceptValue(value: right.PointAt(t: minB)), op.AcceptValue(value: left.PointAt(t: maxA)), op.AcceptValue(value: right.PointAt(t: maxB)))
                .Apply((minD, maxD, mA, mB, xA, xB) => new CurveDeviation(MinimumDistance: minD, MinimumA: mA, MinimumB: mB, MaximumDistance: maxD, MaximumA: xA, MaximumB: xB, Tolerance: context.Absolute.Value, WithinTolerance: maxD <= context.Absolute.Value))
                .As()
                .Bind(deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                    (true, true) => Fin.Succ(deviation),
                    _ => Fin.Fail<CurveDeviation>(op.InvalidResult()),
                }),
            false => Fin.Fail<CurveDeviation>(op.InvalidResult()),
        };
}
