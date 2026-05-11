namespace Rasm.Analysis;

public static partial class Query {
    private delegate bool CurvePointIntersection<TLeft, TRight>(TLeft left, TRight right, Context context, out Curve[] curves, out Point3d[] points);
    private delegate IEnumerable<(Polyline Polyline, IntersectionKind Kind)>? PolylineIntersection<TLeft, TRight>(TLeft left, TRight right, Analyze.Runtime runtime);
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        Aspect<(TA A, TB B), TOut, Unit>(
            aspect: unit,
            key: IntersectKey,
            dispatch: static _ => (typeof(TA), typeof(TB), typeof(TOut)) switch {
                (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Plane) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Line, Plane, TOut>(
                        key: IntersectKey,
                        a: Requirement.None,
                        b: Requirement.None,
                        output: static (left, right, _) => LinePlane<TOut>(line: left, plane: right)),
                (Type a, Type b, Type output) when a == typeof(Plane) && b == typeof(Line) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Plane, Line, TOut>(
                        key: IntersectKey,
                        a: Requirement.None,
                        b: Requirement.None,
                        output: static (left, right, _) => LinePlane<TOut>(line: right, plane: left)),
                (Type a, Type b, Type output) when a == typeof(Plane) && b == typeof(Plane) && (output == typeof(Line) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Plane, Plane, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => IntersectKey.IntersectionOutput<TOut>(lines: Intersection.PlanePlane(planeA: left, planeB: right, intersectionLine: out Line line) ? [line] : [])),
                (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Circle) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Line, Circle, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => Intersection.LineCircle(line: left, circle: right, t1: out double _, point1: out Point3d a, t2: out double _, point2: out Point3d b) switch {
                        LineCircleIntersection.Single => IntersectKey.IntersectionOutput<TOut>(points: [a]),
                        LineCircleIntersection.Multiple => IntersectKey.IntersectionOutput<TOut>(points: [a, b]),
                        _ => IntersectKey.IntersectionOutput<TOut>(points: []),
                    }),
                (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Sphere) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Line, Sphere, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, _) => Intersection.LineSphere(line: left, sphere: right, intersectionPoint1: out Point3d a, intersectionPoint2: out Point3d b) switch {
                        LineSphereIntersection.Single => IntersectKey.IntersectionOutput<TOut>(points: [a]),
                        LineSphereIntersection.Multiple => IntersectKey.IntersectionOutput<TOut>(points: [a, b]),
                        _ => IntersectKey.IntersectionOutput<TOut>(points: []),
                    }),
                (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(BoundingBox) && (output == typeof(Interval) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Line, BoundingBox, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, runtime) => IntersectKey.IntersectionOutput<TOut>(intervals: Intersection.LineBox(line: left, box: right, tolerance: runtime.Context.Absolute.Value, lineParameters: out Interval interval) ? [interval] : [])),
                (Type a, Type b, Type output) when a == typeof(Line) && b == typeof(Box) && (output == typeof(Interval) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Line, Box, TOut>(key: IntersectKey, a: Requirement.None, b: Requirement.None, output: static (left, right, runtime) => IntersectKey.IntersectionOutput<TOut>(intervals: Intersection.LineBox(line: left, box: right, tolerance: runtime.Context.Absolute.Value, lineParameters: out Interval interval) ? [interval] : [])),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Curve, TOut>(a: Requirement.Basic, b: Requirement.Basic, intersect: static (left, right, context) => Intersection.CurveCurve(curveA: left, curveB: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Plane) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Plane, TOut>(a: Requirement.Basic, b: Requirement.None, intersect: static (left, right, context) => Intersection.CurvePlane(curve: left, plane: right, tolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Line) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Line, TOut>(a: Requirement.Basic, b: Requirement.None, intersect: static (left, right, context) => Intersection.CurveLine(curve: left, line: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Curve, Brep, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.Basic,
                        acceptPartialResults: true,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.CurveBrep(
                                curve: left,
                                brep: right,
                                tolerance: context.Absolute.Value,
                                overlapCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(BrepFace).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Curve, BrepFace, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.CurveBrepFace(
                                curve: left,
                                face: right,
                                tolerance: context.Absolute.Value,
                                overlapCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Surface, TOut>(a: Requirement.Basic, b: Requirement.SurfaceEvaluation, intersect: static (left, right, context) => Intersection.CurveSurface(curve: left, surface: right, tolerance: context.Absolute.Value, overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Surface).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Surface, Surface, TOut>(
                        a: Requirement.SurfaceEvaluation,
                        b: Requirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.SurfaceSurface(
                                surfaceA: left,
                                surfaceB: right,
                                tolerance: context.Absolute.Value,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && b == typeof(Plane) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Plane, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.None,
                        acceptPartialResults: false,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.BrepPlane(
                                brep: left,
                                plane: right,
                                tolerance: context.Absolute.Value,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Surface, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.BrepSurface(
                                brep: left,
                                surface: right,
                                tolerance: context.Absolute.Value,
                                joinCurves: true,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Brep, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.Basic,
                        acceptPartialResults: false,
                        intersect: static (left, right, context, out curves, out points) =>
                            Intersection.BrepBrep(
                                brepA: left,
                                brepB: right,
                                tolerance: context.Absolute.Value,
                                joinCurves: true,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && b == typeof(Plane) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) =>
                    PairPolylines<TA, TB, Mesh, Plane, TOut>(
                        a: Requirement.MeshCheck,
                        b: Requirement.None,
                        intersect: (left, right, runtime) => {
                            using MeshIntersectionCache cache = new();
                            return Optional(Intersection.MeshPlane(
                                    mesh: left,
                                    cache: cache,
                                    plane: right,
                                    tolerance: runtime.Context.MeshIntersectionTolerance,
                                    overlaps: true))
                                .ToSeq()
                                .Bind(static values => values)
                                .Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Unknown));
                        }),
                (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && b == typeof(Line) && (output == typeof(Point3d) || output == typeof(IntersectionKind)) =>
                    Pair<TA, TB, Mesh, Line, TOut>(
                        key: IntersectKey,
                        a: Requirement.MeshCheck,
                        b: Requirement.None,
                        output: static (left, right, _) => IntersectKey.IntersectionOutput<TOut>(
                            points: Intersection.MeshLineSorted(
                                mesh: left,
                                line: right,
                                faceIds: out int[] _))),
                (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && typeof(Mesh).IsAssignableFrom(c: b) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) =>
                    PairPolylines<TA, TB, Mesh, Mesh, TOut>(
                        a: Requirement.MeshCheck,
                        b: Requirement.MeshCheck,
                        intersect: (left, right, runtime) => {
                            using TextLog textLog = new();
                            return Intersection.MeshMesh(
                                meshes: [left, right],
                                tolerance: runtime.Context.MeshIntersectionTolerance,
                                intersections: out Polyline[] intersections,
                                overlapsPolylines: true,
                                overlapsPolylinesResult: out Polyline[] overlaps,
                                overlapsMesh: false,
                                overlapsMeshResult: out Mesh _,
                                textLog: textLog,
                                cancel: runtime.Cancellation,
                                progress: runtime.Progress) switch {
                                    true => Optional(intersections)
                                        .ToSeq()
                                        .Bind(static values => values)
                                        .Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Curve))
                                        .Concat(second: Optional(overlaps).ToSeq().Bind(static values => values).Select(static polyline => (Polyline: polyline, Kind: IntersectionKind.Overlap))),
                                    false => null,
                                };
                        }),
                _ => null,
            });
    public static Query<(TA A, TB B), TOut> Deviation<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        (typeof(TA), typeof(TB), typeof(TOut)) switch {
            (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && output == typeof(CurveDeviation) =>
                    Pair<TA, TB, Curve, Curve, TOut>(
                        key: DeviationKey,
                        a: Requirement.CurveLength,
                        b: Requirement.CurveLength,
                            output: static (left, right, runtime) => Curve.GetDistancesBetweenCurves(
                                curveA: left,
                                curveB: right,
                                tolerance: runtime.Context.Absolute.Value,
                                maxDistance: out double maximumDistance,
                                maxDistanceParameterA: out double maximumA,
                                maxDistanceParameterB: out double maximumB,
                                minDistance: out double minimumDistance,
                                minDistanceParameterA: out double minimumA,
                                minDistanceParameterB: out double minimumB) switch {
                                    true => (
                                        DeviationKey.RequireValid(value: minimumDistance),
                                        DeviationKey.RequireValid(value: maximumDistance),
                                        DeviationKey.RequireValid(value: left.PointAt(t: minimumA)),
                                        DeviationKey.RequireValid(value: right.PointAt(t: minimumB)),
                                        DeviationKey.RequireValid(value: left.PointAt(t: maximumA)),
                                        DeviationKey.RequireValid(value: right.PointAt(t: maximumB))
                                    ).Apply((minDist, maxDist, minA, minB, maxA, maxB) =>
                                        new CurveDeviation(
                                            MinimumDistance: minDist,
                                            MinimumA: minA,
                                            MinimumB: minB,
                                            MaximumDistance: maxDist,
                                            MaximumA: maxA,
                                            MaximumB: maxB,
                                            Tolerance: runtime.Context.Absolute.Value,
                                            WithinTolerance: maxDist <= runtime.Context.Absolute.Value))
                                    .As()
                                    .Bind(static deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                                        (true, true) => DeviationKey.Retype<CurveDeviation, TOut>(values: Seq(deviation)),
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
            evaluator: static (state, geometry) =>
                from runtime in Analyze.RuntimeAsks
                from validated in runtime.Context.ValidatePair(a: geometry.A, b: geometry.B, requirementA: state.A, requirementB: state.B).ToEff()
                from result in ((validated.A, validated.B) switch {
                    (TLeft left, TRight right) => state.Output(arg1: left, arg2: right, arg3: runtime),
                    _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(geometryType: typeof((TA A, TB B)), outputType: typeof(TOut))),
                }).ToEff()
                select result);
    private static Fin<Seq<TOut>> LinePlane<TOut>(Line line, Plane plane) =>
        IntersectKey.IntersectionOutput<TOut>(points: Intersection.LinePlane(line: line, plane: plane, lineParameter: out double parameter) ? [line.PointAt(t: parameter)] : []);
    private static Query<(TA A, TB B), TOut> PairEvents<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, Func<TLeft, TRight, Context, CurveIntersections?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => {
                using CurveIntersections? intersections = intersect(arg1: left, arg2: right, arg3: runtime.Context);
                return IntersectKey.IntersectionOutput<TOut>(intersections: intersections);
            });
    private static Query<(TA A, TB B), TOut> PairCurvePoint<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, bool acceptPartialResults, CurvePointIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => intersect(left: left, right: right, context: runtime.Context, curves: out Curve[] curves, points: out Point3d[] points) switch {
                true => IntersectKey.IntersectionOutput<TOut>(curves: curves, points: points),
                false when acceptPartialResults && (Optional(curves).Map(static values => values.Length).IfNone(0) > 0 || Optional(points).Map(static values => values.Length).IfNone(0) > 0) => IntersectKey.IntersectionOutput<TOut>(curves: curves, points: points),
                false => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
            });
    private static Query<(TA A, TB B), TOut> PairPolylines<TA, TB, TLeft, TRight, TOut>(Requirement a, Requirement b, PolylineIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, runtime) => intersect(left: left, right: right, runtime: runtime) switch {
                IEnumerable<(Polyline Polyline, IntersectionKind Kind)> polylines => IntersectKey.IntersectionOutput<TOut>(polylines: polylines.Select(static value => value.Polyline), kinds: polylines.Select(static value => value.Kind)),
                _ when runtime.Cancellation.IsCancellationRequested => Fin.Fail<Seq<TOut>>(OpFault.Cancelled()),
                _ => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
            });
    private static bool Events(Type output) => output == typeof(IntersectionEvent) || output == typeof(Point3d) || output == typeof(IntersectionKind);
    private static bool Curves(Type output) => output == typeof(Curve) || output == typeof(Point3d) || output == typeof(IntersectionKind);
}
