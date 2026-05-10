using System.Linq;
using System.Threading;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Rasm.Analysis;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class Query {
    private delegate bool CurvePointIntersection<TLeft, TRight>(
        TLeft left,
        TRight right,
        Context context,
        out Curve[] curves,
        out Point3d[] points);
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        Aspect<(TA A, TB B), TOut, Unit>(
            aspect: unit,
            key: IntersectKey,
            dispatch: static _ => (typeof(TA), typeof(TB), typeof(TOut)) switch {
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Curve, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.Basic,
                        intersect: static (left, right, context) => Intersection.CurveCurve(
                            curveA: left,
                            curveB: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Plane) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Plane, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.None,
                        intersect: static (left, right, context) => Intersection.CurvePlane(
                            curve: left,
                            plane: right,
                            tolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Line) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Line, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.None,
                        intersect: static (left, right, context) => Intersection.CurveLine(
                            curve: left,
                            line: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
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
                    PairEvents<TA, TB, Curve, Surface, TOut>(
                        a: Requirement.Basic,
                        b: Requirement.SurfaceEvaluation,
                        intersect: static (left, right, context) => Intersection.CurveSurface(
                            curve: left,
                            surface: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
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
                        intersect: static (left, right, context) => {
                            using MeshIntersectionCache cache = new();
                            return Intersection.MeshPlane(
                                mesh: left,
                                cache: cache,
                                plane: right,
                                tolerance: context.MeshIntersectionTolerance);
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
                        intersect: static (left, right, context) => {
                            using TextLog textLog = new();
                            return Intersection.MeshMesh(
                                meshes: [left, right],
                                tolerance: context.MeshIntersectionTolerance,
                                intersections: out Polyline[] intersections,
                                overlapsPolylines: true,
                                overlapsPolylinesResult: out Polyline[] overlaps,
                                overlapsMesh: false,
                                overlapsMeshResult: out Mesh _,
                                textLog: textLog,
                                cancel: CancellationToken.None,
                                progress: null!) switch {
                                    true => intersections.Concat(second: overlaps),
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
                    output: static (left, right, context) => CurveDeviationValue<TOut>(left: left, right: right, context: context)),
            _ => DeviationKey.Unsupported<(TA A, TB B), TOut>(),
        };
    private static Fin<Seq<TOut>> CurveDeviationValue<TOut>(Curve left, Curve right, Context context) =>
        Curve.GetDistancesBetweenCurves(
                curveA: left,
                curveB: right,
                tolerance: context.Absolute.Value,
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
                            Tolerance: context.Absolute.Value,
                            WithinTolerance: maxDist <= context.Absolute.Value))
                    .As()
                    .Bind(static deviation => (deviation.MinimumDistance >= 0.0, deviation.MaximumDistance >= deviation.MinimumDistance) switch {
                        (true, true) => DeviationKey.Retype<CurveDeviation, TOut>(values: Seq(deviation)),
                        _ => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                    }),
                    false => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                };
    private static Query<(TA A, TB B), TOut> Pair<TA, TB, TLeft, TRight, TOut>(
        Op key,
        Requirement a,
        Requirement b,
        Func<TLeft, TRight, Context, Fin<Seq<TOut>>> output) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Query<(TA A, TB B), TOut>.Build(
            key: key,
            requiresContext: true,
            state: (Key: key, A: a, B: b, Output: output),
            evaluator: static (state, geometry) =>
                from ctx in Analyze.Asks
                from validated in ctx.Validate(
                        shape: new Pair<TA, TB>.Both(
                            A: geometry.A,
                            B: geometry.B,
                            RequirementA: state.A,
                            RequirementB: state.B))
                    .ToEff()
                from result in PairOutputValue(
                        state: state,
                        geometry: validated,
                        context: ctx)
                    .ToEff()
                select result);
    private static Fin<Seq<TOut>> PairOutputValue<TA, TB, TLeft, TRight, TOut>(
        (Op Key, Requirement A, Requirement B, Func<TLeft, TRight, Context, Fin<Seq<TOut>>> Output) state,
        (TA A, TB B) geometry,
        Context context) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        (geometry.A, geometry.B) switch {
            (TLeft left, TRight right) => state.Output(
                arg1: left,
                arg2: right,
                arg3: context),
            _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(
                geometryType: typeof((TA A, TB B)),
                outputType: typeof(TOut))),
        };
    private static Query<(TA A, TB B), TOut> PairEvents<TA, TB, TLeft, TRight, TOut>(
        Requirement a,
        Requirement b,
        Func<TLeft, TRight, Context, CurveIntersections?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, context) => {
                using CurveIntersections? intersections = intersect(
                    arg1: left,
                    arg2: right,
                    arg3: context);
                return IntersectKey.IntersectionOutput<TOut>(intersections: intersections);
            });
    private static Query<(TA A, TB B), TOut> PairCurvePoint<TA, TB, TLeft, TRight, TOut>(
        Requirement a,
        Requirement b,
        bool acceptPartialResults,
        CurvePointIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, context) => intersect(
                left: left,
                right: right,
                context: context,
                curves: out Curve[] curves,
                points: out Point3d[] points) switch {
                    true => IntersectKey.IntersectionOutput<TOut>(
                        curves: curves,
                        points: points),
                    false when acceptPartialResults && (curves.Length > 0 || points.Length > 0) =>
                        IntersectKey.IntersectionOutput<TOut>(
                            curves: curves,
                            points: points),
                    false => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
                });
    private static Query<(TA A, TB B), TOut> PairPolylines<TA, TB, TLeft, TRight, TOut>(
        Requirement a,
        Requirement b,
        Func<TLeft, TRight, Context, IEnumerable<Polyline>?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (left, right, context) => IntersectKey.IntersectionOutput<TOut>(
                polylines: intersect(
                    arg1: left,
                    arg2: right,
                    arg3: context)));
    private static readonly System.Collections.Generic.HashSet<Type> EventOutputs =
        [typeof(IntersectionEvent), typeof(Point3d), typeof(IntersectionKind)];
    private static readonly System.Collections.Generic.HashSet<Type> CurveOutputs =
        [typeof(Curve), typeof(Point3d), typeof(IntersectionKind)];
    private static bool Events(Type output) =>
        EventOutputs.Contains(item: output);
    private static bool Curves(Type output) =>
        CurveOutputs.Contains(item: output);
}
