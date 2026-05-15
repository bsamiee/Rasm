namespace Rasm.Analysis;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return CanIntersect(left: typeof(TA), right: typeof(TB), unordered: true) switch {
            true => global::Rasm.Analysis.Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation).ToEff()
                                                from result in IntersectionOf(left: resolved.A, right: resolved.B, context: runtime.Context, op: op, progress: runtime.Progress, unordered: true, cancel: runtime.Cancellation).ToEff()
                                                from typed in IntersectionResultRole.Project<TOut>(result: result, key: op).ToEff()
                                                select typed),
            false => key.Unsupported<(TA A, TB B), TOut>(),
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
    internal static bool CanIntersect(Type left, Type right, bool unordered = false) =>
        left == typeof(object) || right == typeof(object) || CanIntersectOrdered(left: left, right: right) || (unordered && CanIntersectOrdered(left: right, right: left));
    internal static bool CanDeviation(Type left, Type right) =>
        typeof(Curve).IsAssignableFrom(left) && typeof(Curve).IsAssignableFrom(right);
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
                .Match(
                    Succ: Fin.Succ,
                    Fail: error => unordered ? IntersectOrdered(left: pair.R, right: pair.L, context: context, op: op, cancel: cancel, progress: progress) : Fin.Fail<IntersectionResult>(error)));
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
            (Curve a, Curve b) => new Lease<CurveIntersections>.Owned(Value: Intersection.CurveCurve(a, b, context.Absolute.Value, context.Absolute.Value)).Use(hits => cancel.IsCancellationRequested ? Fin.Fail<IntersectionResult>(new Fault.Cancelled()) : EventHits(hits: hits, op: op, source: a)),
            (Curve a, Plane b) => CurvePlane(a: a, b: b, context: context, op: op),
            (Curve a, Line b) => CurveLine(a: a, b: b, context: context, op: op),
            (Curve a, BrepFace b) => SolvedHits(solved: Intersection.CurveBrepFace(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel),
            (Curve a, Brep b) => SolvedHits(solved: Intersection.CurveBrep(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel),
            (Curve a, Surface b) => CurveSurface(a: a, b: b, context: context, op: op),
            (Surface a, Surface b) => SolvedHits(solved: Intersection.SurfaceSurface(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Plane b) => SolvedHits(solved: Intersection.BrepPlane(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Surface b) => SolvedHits(solved: Intersection.BrepSurface(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Brep b) => SolvedHits(solved: Intersection.BrepBrep(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
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
    private static Fin<IntersectionResult> EventHits(CurveIntersections? hits, Op op, Curve? source = null, Option<Line> finiteLine = default, double tolerance = 0.0) =>
        Optional(hits).ToFin(op.InvalidResult()).Map(native => (IntersectionResult)new IntersectionResult.Hits(toSeq(native.AsIterable().SelectMany(h => h switch {
            { IsPoint: true } when finiteLine.Map(l => OnFiniteLine(line: l, point: h.PointB, tolerance: tolerance)).IfNone(true) => Seq(IntersectionHit.At(h.PointA)),
            { IsOverlap: true } => (finiteLine.Case switch {
                Line => SegmentInterval(h.OverlapB).Head.Map(cb => (A: new Interval(h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T0)), h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T1))), B: cb)),
                _ => Some((A: h.OverlapA, B: h.OverlapB)),
            }).Map(o => Optional(source).Map(c => IntersectionHit.Overlap(c.PointAt(o.A.T0), c.PointAt(o.A.T1), o.A, o.B, Optional(c.Trim(o.A))))
                .IfNone(IntersectionHit.Overlap(h.PointA, h.PointA2, o.A, o.B))).ToSeq(),
            _ => Seq<IntersectionHit>(),
        }))));
    private static Fin<IntersectionResult> SolvedHits(bool solved, Curve[]? curves, Point3d[]? points, IntersectionKind kind, Op op, CancellationToken cancel) =>
        (Curves: toSeq(curves ?? []), Points: toSeq(points ?? [])) switch {
            (Seq<Curve> cs, Seq<Point3d> ps) => (solved || !cs.IsEmpty || !ps.IsEmpty, cancel.IsCancellationRequested) switch {
                (_, true) => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
                (true, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(cs.Map(c => IntersectionHit.Along(c, kind)) + ps.Map(static p => IntersectionHit.At(p)))),
                _ => Fin.Fail<IntersectionResult>(op.InvalidResult()),
            },
        };
    private static Fin<IntersectionResult> CurvePlane(Curve a, Plane b, Context context, Op op) {
        using CurveIntersections? hits = Intersection.CurvePlane(a, b, context.Absolute.Value);
        return EventHits(hits: hits, op: op, source: a);
    }
    private static Fin<IntersectionResult> CurveLine(Curve a, Line b, Context context, Op op) {
        using CurveIntersections? hits = Intersection.CurveLine(a, b, context.Absolute.Value, context.Absolute.Value);
        return EventHits(hits: hits, op: op, source: a, finiteLine: Some(b), tolerance: context.Absolute.Value);
    }
    private static Fin<IntersectionResult> CurveSurface(Curve a, Surface b, Context context, Op op) {
        using CurveIntersections? hits = Intersection.CurveSurface(a, b, context.Absolute.Value, context.Absolute.Value);
        return EventHits(hits: hits, op: op, source: a);
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

internal static class IntersectionResultRole {
    internal static Fin<Seq<TOut>> Project<TOut>(this IntersectionResult result, Op key) => result.Switch(
        state: key,
        curves: static (k, c) => ProjectUniform<Curve, TOut>(key: k, caseType: typeof(IntersectionResult.Curves), values: c.Values, tag: IntersectionKind.Curve),
        lines: static (k, l) => ProjectUniform<Line, TOut>(key: k, caseType: typeof(IntersectionResult.Lines), values: l.Values, tag: IntersectionKind.Curve),
        circles: static (k, c) => ProjectUniform<Circle, TOut>(key: k, caseType: typeof(IntersectionResult.Circles), values: c.Values, tag: IntersectionKind.Curve),
        points: static (k, p) => ProjectUniform<Point3d, TOut>(key: k, caseType: typeof(IntersectionResult.Points), values: p.Values, tag: IntersectionKind.Point),
        intervals: static (k, i) => ProjectUniform<Interval, TOut>(key: k, caseType: typeof(IntersectionResult.Intervals), values: i.Values, tag: IntersectionKind.Overlap),
        polylines: static (k, p) => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => k.AcceptResults<Polyline, TOut>(values: p.Values.Map(static x => x.Curve)),
            Type t when t == typeof(IntersectionKind) => k.AcceptResults<IntersectionKind, TOut>(values: p.Values.Map(static x => x.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(IntersectionResult.Polylines), outputType: typeof(TOut))),
        },
        hits: static (k, h) => ProjectHits<TOut>(key: k, hits: h.Values));
    private static Fin<Seq<TOut>> ProjectHits<TOut>(Op key, Seq<IntersectionHit> hits) => typeof(TOut) switch {
        Type t when t == typeof(IntersectionHit) => key.AcceptResults<IntersectionHit, TOut>(values: hits),
        Type t when t == typeof(Curve) => key.AcceptResults<Curve, TOut>(values: hits.Bind(static value => value.Curves)),
        Type t when t == typeof(Point3d) => DropHitCurves(hits: hits, result: key.AcceptResults<Point3d, TOut>(values: hits.Bind(static value => value.Points))),
        Type t when t == typeof(Interval) => DropHitCurves(hits: hits, result: key.AcceptResults<Interval, TOut>(values: hits.Bind(static value => value.Intervals))),
        Type t when t == typeof(IntersectionKind) => DropHitCurves(hits: hits, result: key.AcceptResults<IntersectionKind, TOut>(values: hits.Map(static value => value.Kind))),
        _ => DropHitCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(IntersectionResult.Hits), outputType: typeof(TOut)))),
    };
    private static Fin<Seq<TOut>> DropHitCurves<TOut>(Seq<IntersectionHit> hits, Fin<Seq<TOut>> result) {
        _ = hits.Iter(static value => value.Dispose());
        return result;
    }
    private static Fin<Seq<TOut>> ProjectUniform<TNative, TOut>(Op key, Type caseType, Seq<TNative> values, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.AcceptResults<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.AcceptResults<IntersectionKind, TOut>(values: toSeq(Enumerable.Repeat(element: tag, count: values.Count))),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: caseType, outputType: typeof(TOut))),
    };
}
