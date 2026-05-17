using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

[Union]
internal partial record IntersectionResult {
    public sealed record Lines(Seq<Line> Values) : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values) : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values) : IntersectionResult;
    public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult;
    public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
    internal static readonly IntersectionResult LinesShape = new Lines(Seq<Line>());
    internal static readonly IntersectionResult PointsShape = new Points(Seq<Point3d>());
    internal static readonly IntersectionResult IntervalsShape = new Intervals(Seq<Interval>());
    internal static readonly IntersectionResult PolylinesShape = new Polylines(Seq<(Polyline Curve, IntersectionKind Kind)>());
    internal static readonly IntersectionResult HitsShape = new Hits(Seq<IntersectionHit>());
    private static readonly Seq<IntersectionResult> AllShapes = Seq(LinesShape, PointsShape, IntervalsShape, PolylinesShape, HitsShape);
    internal static bool Supports(Type left, Type right, Type output, bool unordered = false) =>
        (left == typeof(object) || right == typeof(object))
            ? AllShapes.Exists(result => result.CanProject(output: output))
            : Analyze.IntersectionShape(left: left, right: right, output: output, unordered: unordered).IsSome;
    internal bool CanProject(Type output) => Switch(
        state: output,
        lines: static (o, _) => o == typeof(Line) || o == typeof(IntersectionKind),
        points: static (o, _) => o == typeof(Point3d) || o == typeof(IntersectionKind),
        intervals: static (o, _) => o == typeof(Interval) || o == typeof(IntersectionKind),
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
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        PairOp<TA, TB, TOut>(
            key: Op.Of(),
            supported: IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), unordered: true),
            compute: static (a, b, ctx, op, p, ct) => IntersectionOf(left: a, right: b, context: ctx, op: op, progress: p, unordered: true, cancel: ct));
    public static Operation<(TA A, TB B), TOut> Classify<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        PairOp<TA, TB, TOut>(
            key: Op.Of(),
            supported: IntersectionResult.Supports(left: typeof(TA), right: typeof(TB), output: typeof(TOut), unordered: true)
                       && (typeof(TOut) == typeof(IntersectionHit) || typeof(TOut) == typeof(IntersectionTangency)),
            compute: static (a, b, ctx, op, p, ct) => ClassifiedIntersectionOf(left: a, right: b, context: ctx, op: op, progress: p, cancel: ct));
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
    private static Operation<(TA A, TB B), TOut> PairOp<TA, TB, TOut>(Op key, bool supported, Func<TA, TB, Context, Op, IProgress<double>?, CancellationToken, Fin<IntersectionResult>> compute) where TA : notnull where TB : notnull =>
        supported switch {
            true => Operation<(TA A, TB B), TOut>.Build(
                key: key, requiresContext: true, state: (Key: key, Compute: compute),
                evaluator: static (state, pair) =>
                    from runtime in Env.EnvAsks
                    from resolved in ((pair.A, pair.B) switch {
                        (RayQuery ray, GeometryBase geometry) =>
                            (state.Key.AcceptValue(value: ray), Requirement.Basic.Apply(context: runtime.Context, value: geometry, cancel: runtime.Cancellation).ToFin())
                                .Apply(static (query, target) => (A: (TA)(object)query, B: (TB)(object)target)).As(),
                        (GeometryBase geometry, RayQuery ray) =>
                            (Requirement.Basic.Apply(context: runtime.Context, value: geometry, cancel: runtime.Cancellation).ToFin(), state.Key.AcceptValue(value: ray))
                                .Apply(static (target, query) => (A: (TA)(object)target, B: (TB)(object)query)).As(),
                        _ => runtime.Context.Pair(a: pair.A, b: pair.B, op: state.Key, requirements: static (_, _, _) => Fin.Succ((A: Requirement.Basic, B: Requirement.Basic)), cancel: runtime.Cancellation)
                            .ToFin()
                            .Map(static pair => (pair.A, pair.B)),
                    }).ToEff()
                    from result in state.Compute(resolved.A, resolved.B, runtime.Context, state.Key, runtime.Progress, runtime.Cancellation).ToEff()
                    from typed in result.Project<TOut>(key: state.Key).ToEff()
                    select typed),
            false => key.Unsupported<(TA A, TB B), TOut>(),
        };
    private readonly record struct IntersectionCase(
        Func<Type, Type, bool> Supports,
        IntersectionResult Shape,
        Func<object, object, Context, Op, CancellationToken, IProgress<double>?, Option<Fin<IntersectionResult>>> Compute) {
        internal bool CanProject(Type left, Type right, Type output) => Supports(arg1: left, arg2: right) && Shape.CanProject(output: output);
        internal Option<Fin<IntersectionResult>> TryCompute(object left, object right, Context context, Op op, CancellationToken cancel, IProgress<double>? progress) =>
            Supports(arg1: left.GetType(), arg2: right.GetType()) ? Compute(arg1: left, arg2: right, arg3: context, arg4: op, arg5: cancel, arg6: progress) : Option<Fin<IntersectionResult>>.None;
        internal static IntersectionCase Pair<TL, TR>(IntersectionResult shape, Func<TL, TR, Context, Op, CancellationToken, IProgress<double>?, Fin<IntersectionResult>> compute) where TL : notnull where TR : notnull =>
            new(
                Supports: static (l, r) => typeof(TL).IsAssignableFrom(l) && typeof(TR).IsAssignableFrom(r),
                Shape: shape,
                Compute: (left, right, context, op, cancel, progress) => (left, right) switch {
                    (TL a, TR b) => Some(compute(arg1: a, arg2: b, arg3: context, arg4: op, arg5: cancel, arg6: progress)),
                    _ => Option<Fin<IntersectionResult>>.None,
                });
    }
    internal static Option<IntersectionResult> IntersectionShape(Type left, Type right, Type output, bool unordered) =>
        IntersectionShapeOrdered(left: left, right: right, output: output) | (unordered ? IntersectionShapeOrdered(left: right, right: left, output: output) : Option<IntersectionResult>.None);
    private static Option<IntersectionResult> IntersectionShapeOrdered(Type left, Type right, Type output) =>
        IntersectionCases.Find(predicate: c => c.CanProject(left: left, right: right, output: output)).Map(static c => c.Shape);
    private static readonly Seq<IntersectionCase> IntersectionCases = Seq(
        IntersectionCase.Pair<Line, Line>(IntersectionResult.PointsShape, static (a, b, context, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineLine(a, b, out double ta, out double _, context.Absolute.Value, true) ? Seq(a.PointAt(ta)) : Seq<Point3d>()))),
        IntersectionCase.Pair<Line, Plane>(IntersectionResult.PointsShape, static (a, b, _, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LinePlane(a, b, out double t) && t is >= 0.0 and <= 1.0 ? Seq(a.PointAt(t)) : Seq<Point3d>()))),
        IntersectionCase.Pair<Plane, Plane>(IntersectionResult.LinesShape, static (a, b, _, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Lines(Intersection.PlanePlane(a, b, out Line line) ? Seq(line) : Seq<Line>()))),
        IntersectionCase.Pair<Line, Circle>(IntersectionResult.PointsShape, static (a, b, _, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineCircle(a, b, out double t1, out Point3d p1, out double t2, out Point3d p2) switch {
                LineCircleIntersection.Single when t1 is >= 0.0 and <= 1.0 => Seq(p1),
                LineCircleIntersection.Multiple => Seq((T: t1, Point: p1), (T: t2, Point: p2)).Where(static p => p.T is >= 0.0 and <= 1.0).Map(static p => p.Point),
                _ => Seq<Point3d>(),
            }))),
        IntersectionCase.Pair<Line, Sphere>(IntersectionResult.PointsShape, static (a, b, context, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.LineSphere(a, b, out Point3d p1, out Point3d p2) switch {
                LineSphereIntersection.Single when OnFiniteLine(line: a, point: p1, tolerance: context.Absolute.Value) => Seq(p1),
                LineSphereIntersection.Multiple => Seq(p1, p2).Where(p => OnFiniteLine(line: a, point: p, tolerance: context.Absolute.Value)),
                _ => Seq<Point3d>(),
            }))),
        IntersectionCase.Pair<Line, BoundingBox>(IntersectionResult.IntervalsShape, static (a, b, context, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, context.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>()))),
        IntersectionCase.Pair<Line, Box>(IntersectionResult.IntervalsShape, static (a, b, context, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Intersection.LineBox(a, b, context.Absolute.Value, out Interval iv) ? SegmentInterval(iv) : Seq<Interval>()))),
        IntersectionCase.Pair<Line, Curve>(IntersectionResult.HitsShape, static (a, b, context, op, _, _) =>
            CurveAgainst(a: b, b: a, context: context, op: op, intersect: static (c, l, t) => Intersection.CurveLine(c, l, t, t), finiteLine: Some(a))),
        IntersectionCase.Pair<Curve, Curve>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            new Lease<CurveIntersections>.Owned(Value: Intersection.CurveCurve(a, b, context.Absolute.Value, context.Absolute.Value)).Use(hits => cancel.IsCancellationRequested ? Fin.Fail<IntersectionResult>(new Fault.Cancelled()) : HitsFromEvents(hits: hits, op: op, source: a))),
        IntersectionCase.Pair<Curve, Plane>(IntersectionResult.HitsShape, static (a, b, context, op, _, _) =>
            CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, p, t) => Intersection.CurvePlane(c, p, t))),
        IntersectionCase.Pair<Curve, Line>(IntersectionResult.HitsShape, static (a, b, context, op, _, _) =>
            CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, l, t) => Intersection.CurveLine(c, l, t, t), finiteLine: Some(b))),
        IntersectionCase.Pair<Curve, BrepFace>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.CurveBrepFace(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel)),
        IntersectionCase.Pair<Curve, Brep>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.CurveBrep(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Overlap, op: op, cancel: cancel, partial: true)),
        IntersectionCase.Pair<Curve, Surface>(IntersectionResult.HitsShape, static (a, b, context, op, _, _) =>
            CurveAgainst(a: a, b: b, context: context, op: op, intersect: static (c, s, t) => Intersection.CurveSurface(c, s, t, t))),
        IntersectionCase.Pair<Surface, Surface>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.SurfaceSurface(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel)),
        IntersectionCase.Pair<Brep, Plane>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.BrepPlane(a, b, context.Absolute.Value, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel)),
        IntersectionCase.Pair<Brep, Surface>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.BrepSurface(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel)),
        IntersectionCase.Pair<Brep, Brep>(IntersectionResult.HitsShape, static (a, b, context, op, cancel, _) =>
            HitsFromSolved(solved: Intersection.BrepBrep(a, b, context.Absolute.Value, true, out Curve[] cs, out Point3d[] ps), curves: cs, points: ps, kind: IntersectionKind.Curve, op: op, cancel: cancel)),
        IntersectionCase.Pair<Mesh, Line>(IntersectionResult.PointsShape, static (a, b, _, _, _, _) =>
            Fin.Succ((IntersectionResult)new IntersectionResult.Points(toSeq(Intersection.MeshLineSorted(a, b, out int[] _) ?? [])))),
        IntersectionCase.Pair<Mesh, Plane>(IntersectionResult.PolylinesShape, static (a, b, context, _, _, _) =>
            new Lease<MeshIntersectionCache>.Owned(Value: new MeshIntersectionCache()).Use(cache =>
                Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(Intersection.MeshPlane(a, cache, b, context.MeshIntersectionTolerance, true)).ToSeq().Bind(static h => h)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)))))),
        IntersectionCase.Pair<Mesh, Mesh>(IntersectionResult.PolylinesShape, static (a, b, context, op, cancel, progress) =>
            new Lease<TextLog>.Owned(Value: new TextLog()).Use(log =>
                Intersection.MeshMesh([a, b], context.MeshIntersectionTolerance, out Polyline[] ints, true, out Polyline[] olap, false, out Mesh _, log, cancel, progress) switch {
                    true => Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(toSeq(Optional(ints).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)) + toSeq(Optional(olap).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Overlap)))),
                    false when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
                    false => Fin.Fail<IntersectionResult>(op.InvalidResult()),
                })),
        IntersectionCase.Pair<RayQuery, Mesh>(IntersectionResult.PointsShape, static (a, b, _, op, _, _) =>
            (a.IsValid, a.MaxReflections) switch {
                (true, 1) =>
                    Fin.Succ((IntersectionResult)new IntersectionResult.Points(Intersection.MeshRay(b, a.Ray) switch {
                        double t when double.IsFinite(t) && t >= 0.0 => Seq(a.Ray.PointAt(t: t)),
                        _ => Seq<Point3d>(),
                    })),
                (true, _) => Fin.Fail<IntersectionResult>(op.Unsupported(typeof(Mesh), typeof(IntersectionResult))),
                _ => Fin.Fail<IntersectionResult>(op.InvalidInput()),
            }),
        new IntersectionCase(
            Supports: static (l, r) => l == typeof(RayQuery) && (typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || GeometryKernel.CanCoerce(source: r, target: typeof(Brep))),
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, context, op, _, _) => (left, right) switch {
                (RayQuery a, Surface b) => Some(RayShoot(query: a, geometry: b, op: op)),
                (RayQuery a, Brep b) => Some(a.MaxReflections == 1 ? RayBrep(query: a, brep: b, context: context, op: op) : Fin.Fail<IntersectionResult>(a.IsValid ? op.Unsupported(typeof(Brep), typeof(IntersectionResult)) : op.InvalidInput())),
                (RayQuery a, GeometryBase { HasBrepForm: true } b) => Some(a.IsValid ? GeometryKernel.BrepForm(source: b, op: op).Bind(lease => lease.Use(brep => a.MaxReflections == 1 ? RayBrep(query: a, brep: brep, context: context, op: op) : Fin.Fail<IntersectionResult>(op.Unsupported(typeof(Brep), typeof(IntersectionResult))))) : Fin.Fail<IntersectionResult>(op.InvalidInput())),
                (RayQuery, GeometryBase) => Some(Fin.Fail<IntersectionResult>(op.Unsupported(geometryType: right.GetType(), outputType: typeof(IntersectionResult)))),
                _ => Option<Fin<IntersectionResult>>.None,
            }),
        new IntersectionCase(
            Supports: static (l, r) => GeometryKernel.CanCurveForm(type: l) && (GeometryKernel.CanCurveForm(type: r) || r == typeof(Plane) || r == typeof(Line) || typeof(Surface).IsAssignableFrom(r) || typeof(Brep).IsAssignableFrom(r) || typeof(BrepFace).IsAssignableFrom(r)),
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, context, op, cancel, progress) => left is not Curve && GeometryKernel.CanCurveForm(type: left.GetType())
                ? Some(GeometryKernel.CurveForm(source: left, op: op).Bind(lease => lease.Use(curve => IntersectOrdered(left: curve, right: right, context: context, op: op, cancel: cancel, progress: progress))))
                : Option<Fin<IntersectionResult>>.None),
        new IntersectionCase(
            Supports: static (l, r) => (GeometryKernel.CanCurveForm(type: l) || l == typeof(Plane) || l == typeof(Line) || typeof(Surface).IsAssignableFrom(l) || typeof(Brep).IsAssignableFrom(l) || typeof(BrepFace).IsAssignableFrom(l)) && GeometryKernel.CanCurveForm(type: r),
            Shape: IntersectionResult.HitsShape,
            Compute: static (left, right, context, op, cancel, progress) => right is not Curve && GeometryKernel.CanCurveForm(type: right.GetType())
                ? Some(GeometryKernel.CurveForm(source: right, op: op).Bind(lease => lease.Use(curve => IntersectOrdered(left: left, right: curve, context: context, op: op, cancel: cancel, progress: progress))))
                : Option<Fin<IntersectionResult>>.None));
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
            (_, Mesh mesh) => new Lease<TextLog>.Owned(Value: new TextLog()).Use(log =>
                mesh.GetSelfIntersections(tolerance: context.MeshIntersectionTolerance, perforations: out Polyline[] perforations, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] overlaps, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: log, cancel: cancel, progress: progress) switch {
                    true => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(
                        toSeq(Optional(perforations).ToSeq().Bind(static p => p)).Map(static p => IntersectionHit.Along(curve: p.ToNurbsCurve(), kind: IntersectionKind.Curve))
                        + toSeq(Optional(overlaps).ToSeq().Bind(static p => p)).Map(static p => IntersectionHit.Along(curve: p.ToNurbsCurve(), kind: IntersectionKind.Overlap)))),
                    false when cancel.IsCancellationRequested => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
                    false => Fin.Fail<IntersectionResult>(op.InvalidResult()),
                }),
            _ => Fin.Fail<IntersectionResult>(op.Unsupported(g.GetType(), typeof(IntersectionResult))),
        });
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
        cancel.IsCancellationRequested switch {
            true => Fin.Fail<IntersectionResult>(new Fault.Cancelled()),
            false => IntersectionCases.Choose(c => c.TryCompute(left: left, right: right, context: context, op: op, cancel: cancel, progress: progress)).Head.ToFin(op.Unsupported(left.GetType(), right.GetType())).Bind(static result => result),
        };
    private static Fin<IntersectionResult> RayShoot(RayQuery query, GeometryBase geometry, Op op) =>
        query.IsValid switch {
            true => Fin.Succ((IntersectionResult)new IntersectionResult.Hits(toSeq(Intersection.RayShoot(Seq(geometry).AsIterable(), query.Ray, query.MaxReflections) ?? []).Map(static e => IntersectionHit.At(e.Point)))),
            false => Fin.Fail<IntersectionResult>(op.InvalidInput()),
        };
    private static Fin<IntersectionResult> RayBrep(RayQuery query, Brep brep, Context context, Op op) {
        BoundingBox box = brep.GetBoundingBox(accurate: true);
        using LineCurve ray = new(line: new Line(
            start: query.Ray.Position,
            direction: query.Ray.Direction,
            length: query.Ray.Position.DistanceTo(other: box.Center) + box.Diagonal.Length));
        return (query.IsValid, box) switch {
            (true, { IsValid: true }) => HitsFromSolved(
                solved: Intersection.CurveBrep(ray, brep, context.Absolute.Value, out Curve[] cs, out Point3d[] ps),
                curves: cs,
                points: [.. ps.Where(p => (p - query.Ray.Position) * query.Ray.Direction >= 0.0)],
                kind: IntersectionKind.Overlap,
                op: op,
                cancel: default,
                partial: true),
            (true, _) => Fin.Fail<IntersectionResult>(op.InvalidResult()),
            _ => Fin.Fail<IntersectionResult>(op.InvalidInput()),
        };
    }
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
