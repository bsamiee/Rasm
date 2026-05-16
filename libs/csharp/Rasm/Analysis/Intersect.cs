using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RayQuery(Ray3d Ray, int MaxReflections = 1) {
    public static RayQuery Of(Ray3d ray, int maxReflections = 1) => new(Ray: ray, MaxReflections: maxReflections);
}

[Union]
internal partial record IntersectionResult {
    public sealed record Lines(Seq<Line> Values) : IntersectionResult; public sealed record Points(Seq<Point3d> Values) : IntersectionResult; public sealed record Intervals(Seq<Interval> Values) : IntersectionResult; public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult; public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
    internal static bool CanProjectAny(Type output) =>
        Seq<IntersectionResult>(new Lines(Seq<Line>()), new Points(Seq<Point3d>()), new Intervals(Seq<Interval>()), new Polylines(Seq<(Polyline Curve, IntersectionKind Kind)>()), new Hits(Seq<IntersectionHit>()))
            .Exists(result => result.CanProject(output: output));
    internal static bool Supports(Type left, Type right, Type output, bool unordered = false) =>
        (left == typeof(object) || right == typeof(object))
            ? CanProjectAny(output: output)
            : ShapeOf(left: left, right: right).Map(result => result.CanProject(output: output)).IfNone(false)
              || (unordered && ShapeOf(left: right, right: left).Map(result => result.CanProject(output: output)).IfNone(false));
    internal static IntersectionResult HitsShape => new Hits(Seq<IntersectionHit>());
    internal bool CanProject(Type output) => Switch(
        state: output,
        lines: static (o, _) => UniformCanProjectTo<Line>(output: o),
        points: static (o, _) => UniformCanProjectTo<Point3d>(output: o),
        intervals: static (o, _) => UniformCanProjectTo<Interval>(output: o),
        polylines: static (o, _) => o == typeof(Polyline) || o == typeof(IntersectionKind),
        hits: static (o, _) => IntersectionHit.CanProjectTo(output: o));
    internal Fin<Seq<TOut>> Project<TOut>(Op key) => Switch(
        state: key,
        lines: static (k, l) => UniformAs<Line, TOut>(values: l.Values, key: k, caseType: typeof(Lines), tag: IntersectionKind.Curve),
        points: static (k, p) => UniformAs<Point3d, TOut>(values: p.Values, key: k, caseType: typeof(Points), tag: IntersectionKind.Point),
        intervals: static (k, i) => UniformAs<Interval, TOut>(values: i.Values, key: k, caseType: typeof(Intervals), tag: IntersectionKind.Overlap),
        polylines: static (k, p) => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => k.AcceptResults<Polyline, TOut>(values: p.Values.Map(static x => x.Curve)),
            Type t when t == typeof(IntersectionKind) => k.AcceptResults<IntersectionKind, TOut>(values: p.Values.Map(static x => x.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(Polylines), outputType: typeof(TOut))),
        },
        hits: static (k, h) => IntersectionHit.Project<TOut>(hits: h.Values, key: k));
    private static Fin<Seq<TOut>> UniformAs<TNative, TOut>(Seq<TNative> values, Op key, Type caseType, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.AcceptResults<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.AcceptResults<IntersectionKind, TOut>(values: toSeq(Enumerable.Repeat(element: tag, count: values.Count))),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: caseType, outputType: typeof(TOut))),
    };
    private static bool UniformCanProjectTo<TNative>(Type output) => output == typeof(TNative) || output == typeof(IntersectionKind);
    private static Option<IntersectionResult> ShapeOf(Type left, Type right) =>
        (left, right) switch {
            (Type l, Type r) when l == typeof(Line) && (r == typeof(Line) || r == typeof(Plane) || r == typeof(Circle) || r == typeof(Sphere)) => Some<IntersectionResult>(new Points(Seq<Point3d>())),
            (Type l, Type r) when typeof(Mesh).IsAssignableFrom(l) && r == typeof(Line) => Some<IntersectionResult>(new Points(Seq<Point3d>())),
            (Type l, Type r) when l == typeof(Plane) && r == typeof(Plane) => Some<IntersectionResult>(new Lines(Seq<Line>())),
            (Type l, Type r) when l == typeof(Line) && (r == typeof(BoundingBox) || r == typeof(Box)) => Some<IntersectionResult>(new Intervals(Seq<Interval>())),
            (Type l, Type r) when typeof(Mesh).IsAssignableFrom(l) && (r == typeof(Plane) || typeof(Mesh).IsAssignableFrom(r)) => Some<IntersectionResult>(new Polylines(Seq<(Polyline Curve, IntersectionKind Kind)>())),
            (Type l, Type r) when l == typeof(RayQuery) && typeof(Mesh).IsAssignableFrom(r) => Some<IntersectionResult>(new Points(Seq<Point3d>())),
            (Type l, Type r) when l == typeof(RayQuery) && typeof(GeometryBase).IsAssignableFrom(r) => Some<IntersectionResult>(new Hits(Seq<IntersectionHit>())),
            (Type l, Type r) when GeometryKernel.CanCurveForm(type: l) && (GeometryKernel.CanCurveForm(type: r) || r == typeof(Plane) || r == typeof(Line) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || typeof(BrepFace).IsAssignableFrom(r)) => Some<IntersectionResult>(new Hits(Seq<IntersectionHit>())),
            (Type l, Type r) when typeof(Surface).IsAssignableFrom(l) && typeof(Surface).IsAssignableFrom(r) => Some<IntersectionResult>(new Hits(Seq<IntersectionHit>())),
            (Type l, Type r) when typeof(Brep).IsAssignableFrom(l) && (r == typeof(Plane) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r)) => Some<IntersectionResult>(new Hits(Seq<IntersectionHit>())),
            _ => Option<IntersectionResult>.None,
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), unordered: true) switch {
            true => Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation).ToEff()
                                                from result in IntersectionOf(left: resolved.A, right: resolved.B, context: runtime.Context, op: op, progress: runtime.Progress, unordered: true, cancel: runtime.Cancellation).ToEff()
                                                from typed in result.Project<TOut>(key: op).ToEff()
                                                select typed),
            _ => key.Unsupported<(TA A, TB B), TOut>(),
        };
    }
    public static Operation<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (CanDeviation(left: typeof(TA), right: typeof(TB)) && typeof(TOut) == typeof(CurveDeviation))
            ? Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) => from runtime in Env.EnvAsks
                                                from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op, requirements: static (_, _, _) => Fin.Succ((A: Requirement.CurveLength, B: Requirement.CurveLength)), cancel: runtime.Cancellation).ToEff()
                                                from deviation in DeviationOf(left: resolved.A, right: resolved.B, context: runtime.Context, op: op).ToEff()
                                                from result in op.AcceptResults<CurveDeviation, TOut>(values: Seq(deviation)).ToEff()
                                                select result)
            : key.Unsupported<(TA A, TB B), TOut>();
    }
    public static Operation<TGeometry, TOut> SelfIntersect<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (CanSelfIntersect(geometry: typeof(TGeometry)) && IntersectionResult.HitsShape.CanProject(output: typeof(TOut)))
            ? Operation<TGeometry, TOut>.Build(
                key: key, requirement: Requirement.Basic, state: key,
                evaluator: static (op, geometry) => from runtime in Env.EnvAsks
                                                    from result in SelfIntersectionOf(geometry: geometry, context: runtime.Context, op: op, cancel: runtime.Cancellation, progress: runtime.Progress).ToEff()
                                                    from typed in result.Project<TOut>(key: op).ToEff()
                                                    select typed)
            : key.Unsupported<TGeometry, TOut>();
    }
    public static Operation<(TA A, TB B), TOut> Classify<TA, TB, TOut>() where TA : notnull where TB : notnull {
        Op key = Op.Of();
        return (IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), unordered: true)
                && (typeof(TOut) == typeof(IntersectionHit) || typeof(TOut) == typeof(IntersectionTangency)))
            ? Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: key,
                evaluator: static (op, pair) =>
                    from runtime in Env.EnvAsks
                    from resolved in runtime.Context.Pair(a: pair.A, b: pair.B, op: op,
                        requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)),
                        cancel: runtime.Cancellation).ToEff()
                    from result in ClassifiedIntersectionOf(left: resolved.A, right: resolved.B,
                        context: runtime.Context, op: op, progress: runtime.Progress,
                        cancel: runtime.Cancellation).ToEff()
                    from typed in result.Project<TOut>(key: op).ToEff()
                    select typed)
            : key.Unsupported<(TA A, TB B), TOut>();
    }
    internal static Fin<IntersectionResult> ClassifiedIntersectionOf<TL, TR>(TL left, TR right, Context context, Op op, IProgress<double>? progress, CancellationToken cancel) where TL : notnull where TR : notnull =>
        IntersectionOf(left: left, right: right, context: context, op: op, progress: progress, unordered: true, cancel: cancel)
            .Bind(result => (result, GeometryKernel.CanCurveForm(type: typeof(TL)) && GeometryKernel.CanCurveForm(type: typeof(TR))) switch {
                (IntersectionResult.Hits hits, true) => EnrichTangency(hits: hits.Values, left: left, right: right, context: context, op: op)
                    .Map(static enriched => (IntersectionResult)new IntersectionResult.Hits(enriched)),
                _ => Fin.Succ(result),
            });
    private static Fin<Seq<IntersectionHit>> EnrichTangency<TL, TR>(Seq<IntersectionHit> hits, TL left, TR right, Context context, Op op) where TL : notnull where TR : notnull =>
        GeometryKernel.CurveForm(source: left, op: op).Bind(leftLease =>
            GeometryKernel.CurveForm(source: right, op: op).Bind(rightLease =>
                leftLease.Use(lc => rightLease.Use(rc =>
                    Fin.Succ(hits.Map(hit => hit switch {
                        IntersectionHit.PointCase pc when pc.Tangency == IntersectionTangency.Unknown =>
                            IntersectionHit.At(point: pc.Point, tangency: TangencyAt(left: lc, right: rc, point: pc.Point, tolerance: context.Angle.Value)),
                        _ => hit,
                    }))))));
    private static IntersectionTangency TangencyAt(Curve left, Curve right, Point3d point, double tolerance) =>
        (left.ClosestPoint(testPoint: point, t: out double tl), right.ClosestPoint(testPoint: point, t: out double tr)) switch {
            (true, true) => (left.TangentAt(t: tl), right.TangentAt(t: tr)) switch {
                (Vector3d a, Vector3d b) when a.IsValid && b.IsValid && !a.IsTiny() && !b.IsTiny() =>
                    Vector3d.VectorAngle(a: a, b: b) switch {
                        double angle when angle <= tolerance || (Math.PI - angle) <= tolerance => IntersectionTangency.Tangent,
                        _ => IntersectionTangency.Transversal,
                    },
                _ => IntersectionTangency.Unknown,
            },
            _ => IntersectionTangency.Unknown,
        };
    internal static bool CanSelfIntersect(Type geometry) =>
        geometry == typeof(object) || typeof(Curve).IsAssignableFrom(c: geometry) || typeof(Mesh).IsAssignableFrom(c: geometry);
    internal static Fin<IntersectionResult> SelfIntersectionOf<TGeometry>(TGeometry geometry, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => (cancel.IsCancellationRequested, g) switch {
            (true, _) => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            (_, Curve curve) => new Lease<CurveIntersections>.Owned(Value: Intersection.CurveSelf(curve: curve, tolerance: context.Absolute.Value)).Use(hits => HitsFromEvents(hits: hits, op: op, source: curve)),
            (_, Mesh mesh) => MeshSelfIntersectionsOf(mesh: mesh, context: context, op: op, cancel: cancel, progress: progress),
            _ => Fin.Fail<IntersectionResult>(op.Unsupported(g.GetType(), typeof(IntersectionResult))),
        });
    private static Fin<IntersectionResult> MeshSelfIntersectionsOf(Mesh mesh, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) {
        using TextLog textLog = new();
        return mesh.GetSelfIntersections(tolerance: context.MeshIntersectionTolerance, perforations: out Polyline[] perforations, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] overlaps, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: textLog, cancel: cancel, progress: progress) switch {
            true => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(
                toSeq(Optional(perforations).ToSeq().Bind(static p => p)).Map(PerforationHit)
                + toSeq(Optional(overlaps).ToSeq().Bind(static p => p)).Map(OverlapHit))),
            false when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            false => Fin.Fail<IntersectionResult>(op.InvalidResult()),
        };
    }
    private static IntersectionHit PerforationHit(Polyline polyline) => IntersectionHit.Along(curve: polyline.ToNurbsCurve(), kind: IntersectionKind.Curve);
    private static IntersectionHit OverlapHit(Polyline polyline) => IntersectionHit.Along(curve: polyline.ToNurbsCurve(), kind: IntersectionKind.Overlap);
    internal static bool CanDeviation(Type left, Type right) =>
        GeometryKernel.CanCurveForm(type: left) && GeometryKernel.CanCurveForm(type: right);
    internal static Fin<IntersectionResult> IntersectionOf<TL, TR>(TL left, TR right, Context context, Op op, IProgress<double>? progress, bool unordered, CancellationToken cancel) where TL : notnull where TR : notnull =>
        (Optional(left).ToFin(op.InvalidInput()), Optional(right).ToFin(op.InvalidInput())).Apply((l, r) => (L: (object)l, R: (object)r)).As()
            .Bind(pair => IntersectOrdered(left: pair.L, right: pair.R, context: context, op: op, cancel: cancel, progress: progress)
                .BindFail(error => (unordered, error) switch {
                    (true, Fault.Unsupported) => IntersectOrdered(left: pair.R, right: pair.L, context: context, op: op, cancel: cancel, progress: progress),
                    _ => Fin.Fail<IntersectionResult>(error),
                }));
    internal static Fin<CurveDeviation> DeviationOf<TL, TR>(TL left, TR right, Context context, Op op) where TL : notnull where TR : notnull =>
        GeometryKernel.CurveForm(source: left, op: op)
            .Bind(leftLease => leftLease.Use(leftCurve => GeometryKernel.CurveForm(source: right, op: op)
                .Bind(rightLease => rightLease.Use(rightCurve => CurveDeviationOf(left: leftCurve, right: rightCurve, context: context, op: op)))));
    private static Fin<IntersectionResult> IntersectOrdered(object left, object right, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) =>
        (left, right) switch {
            _ when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            (Line a, Line b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineLine(a, b, out double ta, out double _, context.Absolute.Value, true) ? Seq(a.PointAt(ta)) : Seq<Point3d>())),
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
            (Line a, Curve b) => CurveAgainst(a: b, b: a, context: context, op: op, intersect: static (c, l, t) => Intersection.CurveLine(c, l, t, t), finiteLine: Some(a)),
            (Curve a, Curve b) => new Lease<CurveIntersections>.Owned(Value: Intersection.CurveCurve(a, b, context.Absolute.Value, context.Absolute.Value)).Use(hits => cancel.IsCancellationRequested ? Fin.Fail<IntersectionResult>(new Fault.Cancelled()) : HitsFromEvents(hits: hits, op: op, source: a)),
            (Curve a, Plane b) => CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, p, t) => Intersection.CurvePlane(c, p, t)),
            (Curve a, Line b) => CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, l, t) => Intersection.CurveLine(c, l, t, t), finiteLine: Some(b)),
            (Curve a, BrepFace b) => HitsFromSolved(solved: Intersection.CurveBrepFace(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel),
            (Curve a, Brep b) => HitsFromSolved(solved: Intersection.CurveBrep(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel, partial: true),
            (Curve a, Surface b) => CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, s, t) => Intersection.CurveSurface(c, s, t, t)),
            (Surface a, Surface b) => HitsFromSolved(solved: Intersection.SurfaceSurface(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Plane b) => HitsFromSolved(solved: Intersection.BrepPlane(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Surface b) => HitsFromSolved(solved: Intersection.BrepSurface(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Brep a, Brep b) => HitsFromSolved(solved: Intersection.BrepBrep(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel),
            (Mesh a, Line b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(toSeq(Intersection.MeshLineSorted(a, b, out int[] _) ?? []))),
            (Mesh a, Plane b) => MeshPlane(mesh: a, plane: b, context: context),
            (Mesh a, Mesh b) => MeshMesh(left: a, right: b, context: context, op: op, cancel: cancel, progress: progress),
            (RayQuery a, Mesh b) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.MeshRay(b, a.Ray) switch {
                double t when double.IsFinite(t) && t >= 0.0 => Seq(a.Ray.PointAt(t: t)),
                _ => Seq<Point3d>(),
            })),
            (RayQuery a, GeometryBase b) => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(toSeq(Intersection.RayShoot(Seq(b).AsIterable(), a.Ray, a.MaxReflections) ?? []).Map(static e => IntersectionHit.At(e.Point)))),
            (object a, object b) when a is not Curve && GeometryKernel.CanCurveForm(type: a.GetType()) =>
                GeometryKernel.CurveForm(source: a, op: op).Bind(lease => lease.Use(curve => IntersectOrdered(left: curve, right: b, context: context, op: op, cancel: cancel, progress: progress))),
            (object a, object b) when b is not Curve && GeometryKernel.CanCurveForm(type: b.GetType()) =>
                GeometryKernel.CurveForm(source: b, op: op).Bind(lease => lease.Use(curve => IntersectOrdered(left: a, right: curve, context: context, op: op, cancel: cancel, progress: progress))),
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
        hits switch {
            CurveIntersections native => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(toSeq(native.AsIterable().SelectMany(h => h switch {
                { IsPoint: true } when finiteLine.Map(l => OnFiniteLine(line: l, point: h.PointB, tolerance: tolerance)).IfNone(true) => Seq(IntersectionHit.At(h.PointA)),
                { IsOverlap: true } => (finiteLine.Case switch {
                    Line => SegmentInterval(h.OverlapB).Head.Map(cb => (A: new Interval(h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T0)), h.OverlapA.ParameterAt(h.OverlapB.NormalizedParameterAt(cb.T1))), B: cb)),
                    _ => Some((A: h.OverlapA, B: h.OverlapB)),
                }).Map(o => Optional(source).Map(c => IntersectionHit.Overlap(c.PointAt(o.A.T0), c.PointAt(o.A.T1), o.A, o.B, Optional(c.Trim(o.A))))
                    .IfNone(IntersectionHit.Overlap(h.PointA, h.PointA2, o.A, o.B))).ToSeq(),
                _ => Seq<IntersectionHit>(),
            })))),
            _ => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(Seq<IntersectionHit>())),
        };
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
    internal static Fin<CurveDeviation> CurveDeviationOf(Curve left, Curve right, Context context, Op op) =>
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
