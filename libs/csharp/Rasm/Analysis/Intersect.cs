namespace Rasm.Analysis;

public static partial class Query {
    private delegate bool CurvePointIntersection<TLeft, TRight>(TLeft left, TRight right, Context context, out Curve[] curves, out Point3d[] points);
    private delegate IEnumerable<(Polyline Polyline, IntersectionKind Kind)>? PolylineIntersection<TLeft, TRight>(TLeft left, TRight right, Analyze.Runtime runtime);
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        (typeof(TA), typeof(TB), typeof(TOut)) switch {
            (Type a, Type b, Type output) when ((a == typeof(Line) && b == typeof(Plane)) || (a == typeof(Plane) && b == typeof(Line))) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) => a == typeof(Line)
                    ? Pair<TA, TB, Line, Plane, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => LinePlane<TOut>(line: left, plane: right))
                    : Pair<TA, TB, Plane, Line, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => LinePlane<TOut>(line: right, plane: left)),
            (Type a, Type b, Type output) when a == typeof(Plane) && b == typeof(Plane) && (output == typeof(Line) || output == typeof(IntersectionKind)) => Pair<TA, TB, Plane, Plane, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => new IntersectionResult.Lines(Values: Intersection.PlanePlane(planeA: left, planeB: right, intersectionLine: out Line line) ? Seq(line) : Seq<Line>()).Project<TOut>(key: IntersectKey)),
            (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Circle) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) => Pair<TA, TB, Line, Circle, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => new IntersectionResult.Points(Values: Intersection.LineCircle(line: left, circle: right, t1: out double _, point1: out Point3d a, t2: out double _, point2: out Point3d b) switch {
                LineCircleIntersection.Single => Seq(a),
                LineCircleIntersection.Multiple => Seq(a, b),
                _ => Seq<Point3d>(),
            }).Project<TOut>(key: IntersectKey)),
            (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Sphere) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) => Pair<TA, TB, Line, Sphere, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => new IntersectionResult.Points(Values: Intersection.LineSphere(line: left, sphere: right, intersectionPoint1: out Point3d a, intersectionPoint2: out Point3d b) switch {
                LineSphereIntersection.Single => Seq(a),
                LineSphereIntersection.Multiple => Seq(a, b),
                _ => Seq<Point3d>(),
            }).Project<TOut>(key: IntersectKey)),
            (Type a, Type b, Type output) when a == typeof(Line) && (b == typeof(BoundingBox) || b == typeof(Box)) && (output == typeof(Interval) || output == typeof(IntersectionKind)) => b == typeof(BoundingBox)
                    ? Pair<TA, TB, Line, BoundingBox, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, runtime) => LineBox<TOut, BoundingBox>(line: left, box: right, runtime: runtime))
                    : Pair<TA, TB, Line, Box, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, runtime) => LineBox<TOut, Box>(line: left, box: right, runtime: runtime)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && Events(output: output) => PairEvents<TA, TB, Curve, Curve, TOut>(a: Requirement.Basic, b: Requirement.Basic, intersect: static (left, right, context) => Intersection.CurveCurve(curveA: left, curveB: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Plane) && Events(output: output) => PairEvents<TA, TB, Curve, Plane, TOut>(a: Requirement.Basic, b: Requirement.None, intersect: static (left, right, context) => Intersection.CurvePlane(curve: left, plane: right, tolerance: context.Absolute.Value)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Line) && Events(output: output) => PairEvents<TA, TB, Curve, Line, TOut>(a: Requirement.Basic, b: Requirement.None, intersect: static (left, right, context) => Intersection.CurveLine(curve: left, line: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) => PairCurvePoint<TA, TB, Curve, Brep, TOut>(
                    a: Requirement.Basic, b: Requirement.Basic, acceptPartialResults: true, intersect: static (left, right, context, out curves, out points) => Intersection.CurveBrep(curve: left, brep: right, tolerance: context.Absolute.Value, overlapCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(BrepFace).IsAssignableFrom(c: b) && Curves(output: output) => PairCurvePoint<TA, TB, Curve, BrepFace, TOut>(
                    a: Requirement.Basic, b: Requirement.SurfaceEvaluation, acceptPartialResults: false, intersect: static (left, right, context, out curves, out points) => Intersection.CurveBrepFace(curve: left, face: right, tolerance: context.Absolute.Value, overlapCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Events(output: output) => PairEvents<TA, TB, Curve, Surface, TOut>(a: Requirement.Basic, b: Requirement.SurfaceEvaluation, intersect: static (left, right, context) => Intersection.CurveSurface(curve: left, surface: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
            (Type a, Type b, Type output) when typeof(Surface).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) => PairCurvePoint<TA, TB, Surface, Surface, TOut>(
                    a: Requirement.SurfaceEvaluation, b: Requirement.SurfaceEvaluation, acceptPartialResults: false, intersect: static (left, right, context, out curves, out points) => Intersection.SurfaceSurface(surfaceA: left, surfaceB: right, tolerance: context.Absolute.Value, intersectionCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && b == typeof(Plane) && Curves(output: output) => PairCurvePoint<TA, TB, Brep, Plane, TOut>(
                    a: Requirement.Basic, b: Requirement.None, acceptPartialResults: false, intersect: static (left, right, context, out curves, out points) => Intersection.BrepPlane(brep: left, plane: right, tolerance: context.Absolute.Value, intersectionCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) => PairCurvePoint<TA, TB, Brep, Surface, TOut>(
                    a: Requirement.Basic, b: Requirement.SurfaceEvaluation, acceptPartialResults: false, intersect: static (left, right, context, out curves, out points) => Intersection.BrepSurface(brep: left, surface: right, tolerance: context.Absolute.Value, joinCurves: true, intersectionCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) => PairCurvePoint<TA, TB, Brep, Brep, TOut>(
                    a: Requirement.Basic, b: Requirement.Basic, acceptPartialResults: false, intersect: static (left, right, context, out curves, out points) => Intersection.BrepBrep(brepA: left, brepB: right, tolerance: context.Absolute.Value, joinCurves: true, intersectionCurves: out curves, intersectionPoints: out points)),
            (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && b == typeof(Plane) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) => PairPolylines<TA, TB, Mesh, Plane, TOut>(
                    a: Requirement.MeshCheck, b: Requirement.None, intersect: (left, right, runtime) => {
                        using MeshIntersectionCache cache = new();
                        return Optional(Intersection.MeshPlane(
                                mesh: left, cache: cache, plane: right, tolerance: runtime.Context.MeshIntersectionTolerance, overlaps: true))
                            .ToSeq()
                            .Bind(static values => values)
                            .Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Unknown));
                    }),
            (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && b == typeof(Line) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) => Pair<TA, TB, Mesh, Line, TOut>(
                    key: IntersectKey, a: Requirement.MeshCheck, b: Requirement.None, output: static (left, right, _) => new IntersectionResult.Points(Values: toSeq(Intersection.MeshLineSorted(mesh: left, line: right, faceIds: out int[] _) ?? [])).Project<TOut>(key: IntersectKey)),
            (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && typeof(Mesh).IsAssignableFrom(c: b) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) => PairPolylines<TA, TB, Mesh, Mesh, TOut>(
                    a: Requirement.MeshCheck, b: Requirement.MeshCheck, intersect: (left, right, runtime) => {
                        using TextLog textLog = new();
                        return Intersection.MeshMesh(
                            meshes: [left, right], tolerance: runtime.Context.MeshIntersectionTolerance, intersections: out Polyline[] intersections, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] overlaps, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: textLog, cancel: runtime.Cancellation, progress: runtime.Progress) switch {
                                true => Optional(intersections)
                                    .ToSeq()
                                    .Bind(static values => values)
                                    .Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Curve))
                                    .Concat(second: Optional(overlaps).ToSeq().Bind(static values => values).Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Overlap))),
                                false => null,
                            };
                    }),
            _ => IntersectKey.Unsupported<(TA A, TB B), TOut>(),
        };
    public static Query<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull => (typeof(TA), typeof(TB), typeof(TOut)) switch {
        (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && output == typeof(CurveDeviation) => Pair<TA, TB, Curve, Curve, TOut>(
                    key: DeviationKey, a: Requirement.CurveLength, b: Requirement.CurveLength, output: static (left, right, runtime) => Curve.GetDistancesBetweenCurves(
                            curveA: left, curveB: right, tolerance: runtime.Context.Absolute.Value, maxDistance: out double maximumDistance, maxDistanceParameterA: out double maximumA, maxDistanceParameterB: out double maximumB, minDistance: out double minimumDistance, minDistanceParameterA: out double minimumA, minDistanceParameterB: out double minimumB) switch {
                                true => (
                                    DeviationKey.RequireValid(value: minimumDistance), DeviationKey.RequireValid(value: maximumDistance), DeviationKey.RequireValid(value: left.PointAt(t: minimumA)), DeviationKey.RequireValid(value: right.PointAt(t: minimumB)), DeviationKey.RequireValid(value: left.PointAt(t: maximumA)), DeviationKey.RequireValid(value: right.PointAt(t: maximumB))
                                ).Apply((minDist, maxDist, minA, minB, maxA, maxB) => new CurveDeviation(
                                        MinimumDistance: minDist, MinimumA: minA, MinimumB: minB, MaximumDistance: maxDist, MaximumA: maxA, MaximumB: maxB, Tolerance: runtime.Context.Absolute.Value, WithinTolerance: maxDist <= runtime.Context.Absolute.Value))
                                .As()
                                .Bind(static deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                                    (true, true) => DeviationKey.Results<CurveDeviation, TOut>(values: Seq(deviation)),
                                    _ => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                                }),
                                false => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                            }),
        _ => DeviationKey.Unsupported<(TA A, TB B), TOut>(),
    };
    private static Query<(TA A, TB B), TOut> Pair<TA, TB, TLeft, TRight, TOut>(Op key, Requirement a, Requirement b, Func<TLeft, TRight, Analyze.Runtime, Fin<Seq<TOut>>> output) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Query<(TA A, TB B), TOut>.Build(
            key: key,
            requiresContext: true,
            state: (Key: key, A: a, B: b, Output: output),
            evaluator: static (state, geometry) => from runtime in Analyze.RuntimeAsks
                                                   from validated in runtime.Context.ValidatePair(a: geometry.A, b: geometry.B, requirementA: state.A, requirementB: state.B).ToEff()
                                                   from result in ((validated.A, validated.B) switch {
                                                       (TLeft left, TRight right) => state.Output(arg1: left, arg2: right, arg3: runtime),
                                                       _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(geometryType: typeof((TA A, TB B)), outputType: typeof(TOut))),
                                                   }).ToEff()
                                                   select result);
    private static Fin<Seq<TOut>> LinePlane<TOut>(Line line, Plane plane) =>
        new IntersectionResult.Points(Values: Intersection.LinePlane(line: line, plane: plane, lineParameter: out double parameter) ? Seq(line.PointAt(t: parameter)) : Seq<Point3d>()).Project<TOut>(key: IntersectKey);
    private static Fin<Seq<TOut>> LineBox<TOut, TBox>(Line line, TBox box, Analyze.Runtime runtime) =>
        box switch {
            BoundingBox bounds => new IntersectionResult.Intervals(Values: Intersection.LineBox(line: line, box: bounds, tolerance: runtime.Context.Absolute.Value, lineParameters: out Interval interval) ? Seq(interval) : Seq<Interval>()).Project<TOut>(key: IntersectKey),
            Box oriented => new IntersectionResult.Intervals(Values: Intersection.LineBox(line: line, box: oriented, tolerance: runtime.Context.Absolute.Value, lineParameters: out Interval interval) ? Seq(interval) : Seq<Interval>()).Project<TOut>(key: IntersectKey),
            _ => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidInput()),
        };
    private static Query<(TA A, TB B), TOut> PairEvents<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, Func<TLeft, TRight, Context, CurveIntersections?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => {
                using CurveIntersections? intersections = intersect(arg1: left, arg2: right, arg3: runtime.Context);
                return IntersectionResultRole.FromEvents(intersections: intersections).Project<TOut>(key: IntersectKey);
            });
    private static Query<(TA A, TB B), TOut> PairCurvePoint<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, bool acceptPartialResults, CurvePointIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => intersect(left: left, right: right, context: runtime.Context, curves: out Curve[] curves, points: out Point3d[] points) switch {
                bool solved when solved || (acceptPartialResults && ((curves?.Length ?? 0) > 0 || (points?.Length ?? 0) > 0)) =>
                    new IntersectionResult.Mixed(CurveValues: toSeq(curves ?? []), PointValues: toSeq(points ?? [])).Project<TOut>(key: IntersectKey),
                _ => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
            });
    private static Query<(TA A, TB B), TOut> PairPolylines<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, PolylineIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => intersect(left: left, right: right, runtime: runtime) switch {
                IEnumerable<(Polyline Polyline, IntersectionKind Kind)> polylines => new IntersectionResult.Polylines(Values: toSeq(polylines.Select(static value => value.Polyline)), Kinds: toSeq(polylines.Select(static value => value.Kind))).Project<TOut>(key: IntersectKey),
                _ when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<TOut>>(new OpFault.Cancelled()),
                _ => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
            });
    private static bool Events(Type output) => output == typeof(IntersectionEvent) || output == typeof(Point3d) || output == typeof(IntersectionKind);
    private static bool Curves(Type output) => output == typeof(Curve) || output == typeof(Point3d) || output == typeof(IntersectionKind);
}

// --- [INTERSECTION_RESULT] ---------------------------------------------------------------
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult;
    public sealed record Lines(Seq<Line> Values) : IntersectionResult;
    public sealed record Circles(Seq<Circle> Values) : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values) : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values) : IntersectionResult;
    public sealed record Polylines(Seq<Polyline> Values, Seq<IntersectionKind> Kinds) : IntersectionResult;
    public sealed record Events(Seq<IntersectionEvent> Values) : IntersectionResult;
    public sealed record Mixed(Seq<Curve> CurveValues, Seq<Point3d> PointValues) : IntersectionResult;
}

// --- [INTERSECTION_RESULT_ROLE] ----------------------------------------------------------
internal static class IntersectionResultRole {
    internal static Fin<Seq<TOut>> Project<TOut>(this IntersectionResult result, Op key) => result switch {
        IntersectionResult.Curves curves => typeof(TOut) switch {
            Type t when t == typeof(Curve) => key.Results<Curve, TOut>(values: curves.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: curves.Values.Map(static _ => IntersectionKind.Overlap)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Lines lines => typeof(TOut) switch {
            Type t when t == typeof(Line) => key.Results<Line, TOut>(values: lines.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: lines.Values.Map(static _ => IntersectionKind.Curve)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Circles circles => typeof(TOut) switch {
            Type t when t == typeof(Circle) => key.Results<Circle, TOut>(values: circles.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: circles.Values.Map(static _ => IntersectionKind.Curve)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Points points => typeof(TOut) switch {
            Type t when t == typeof(Point3d) => key.Results<Point3d, TOut>(values: points.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: points.Values.Map(static _ => IntersectionKind.Point)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Intervals intervals => typeof(TOut) switch {
            Type t when t == typeof(Interval) => key.Results<Interval, TOut>(values: intervals.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: intervals.Values.Map(static _ => IntersectionKind.Overlap)),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Polylines polylines => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => key.Results<Polyline, TOut>(values: polylines.Values),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: polylines.Kinds),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Events events => typeof(TOut) switch {
            Type t when t == typeof(IntersectionEvent) => key.Results<IntersectionEvent, TOut>(values: events.Values),
            Type t when t == typeof(Point3d) => key.Results<Point3d, TOut>(values: events.Values.Choose(static value => value.IsPoint ? Some(value.PointA) : Option<Point3d>.None)),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: events.Values.Map(static value => value switch {
                { IsOverlap: true } => IntersectionKind.Overlap,
                { IsPoint: true } => IntersectionKind.Point,
                _ => IntersectionKind.Unknown,
            })),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        IntersectionResult.Mixed mixed => typeof(TOut) switch {
            Type t when t == typeof(Curve) => key.Results<Curve, TOut>(values: mixed.CurveValues),
            Type t when t == typeof(Point3d) => key.Results<Point3d, TOut>(values: mixed.PointValues),
            Type t when t == typeof(IntersectionKind) => key.Results<IntersectionKind, TOut>(values: mixed.CurveValues.Map(static _ => IntersectionKind.Overlap).Concat(second: mixed.PointValues.Map(static _ => IntersectionKind.Point))),
            _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
        },
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(void), outputType: typeof(TOut))),
    };
    internal static IntersectionResult FromEvents(CurveIntersections? intersections) =>
        new IntersectionResult.Events(Values: toSeq(Optional(intersections).ToSeq().Bind(static events => events)));
}
