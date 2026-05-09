using System.Linq;
using System.Threading;
using Core;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [OPERATIONS] ------------------------------------------------------------------------------

public static partial class Query {
    private delegate Fin<Seq<TOut>> PairOutput<TLeft, TRight, TOut>(
        TLeft left,
        TRight right,
        GeometryContext context) where TLeft : notnull where TRight : notnull;
    private delegate bool CurvePointIntersection<TLeft, TRight>(
        TLeft left,
        TRight right,
        GeometryContext context,
        out Curve[] curves,
        out Point3d[] points);
    public static Query<(TA A, TB B), TOut> Intersect<TA, TB, TOut>() where TA : notnull where TB : notnull =>
        Aspect<(TA A, TB B), TOut, Unit>(
            aspect: unit,
            key: IntersectKey,
            dispatch: static (Unit _) => (typeof(TA), typeof(TB), typeof(TOut)) switch {
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Curve, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.Basic,
                        intersect: static (Curve left, Curve right, GeometryContext context) => Intersection.CurveCurve(
                            curveA: left,
                            curveB: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Plane) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Plane, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.None,
                        intersect: static (Curve left, Plane right, GeometryContext context) => Intersection.CurvePlane(
                            curve: left,
                            plane: right,
                            tolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && b == typeof(Line) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Line, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.None,
                        intersect: static (Curve left, Line right, GeometryContext context) => Intersection.CurveLine(
                            curve: left,
                            line: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Curve, Brep, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.Basic,
                        acceptPartialResults: true,
                        intersect: static (Curve left, Brep right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.CurveBrep(
                                curve: left,
                                brep: right,
                                tolerance: context.Absolute.Value,
                                overlapCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(BrepFace).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Curve, BrepFace, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (Curve left, BrepFace right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.CurveBrepFace(
                                curve: left,
                                face: right,
                                tolerance: context.Absolute.Value,
                                overlapCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Events(output: output) =>
                    PairEvents<TA, TB, Curve, Surface, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.SurfaceEvaluation,
                        intersect: static (Curve left, Surface right, GeometryContext context) => Intersection.CurveSurface(
                            curve: left,
                            surface: right,
                            tolerance: context.Absolute.Value,
                            overlapTolerance: context.Absolute.Value)),
                (Type a, Type b, Type output) when typeof(Surface).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Surface, Surface, TOut>(
                        a: GeometryRequirement.SurfaceEvaluation,
                        b: GeometryRequirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (Surface left, Surface right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.SurfaceSurface(
                                surfaceA: left,
                                surfaceB: right,
                                tolerance: context.Absolute.Value,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && b == typeof(Plane) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Plane, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.None,
                        acceptPartialResults: false,
                        intersect: static (Brep left, Plane right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.BrepPlane(
                                brep: left,
                                plane: right,
                                tolerance: context.Absolute.Value,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Surface).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Surface, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.SurfaceEvaluation,
                        acceptPartialResults: false,
                        intersect: static (Brep left, Surface right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.BrepSurface(
                                brep: left,
                                surface: right,
                                tolerance: context.Absolute.Value,
                                joinCurves: true,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Brep).IsAssignableFrom(c: a) && typeof(Brep).IsAssignableFrom(c: b) && Curves(output: output) =>
                    PairCurvePoint<TA, TB, Brep, Brep, TOut>(
                        a: GeometryRequirement.Basic,
                        b: GeometryRequirement.Basic,
                        acceptPartialResults: false,
                        intersect: static (Brep left, Brep right, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                            Intersection.BrepBrep(
                                brepA: left,
                                brepB: right,
                                tolerance: context.Absolute.Value,
                                joinCurves: true,
                                intersectionCurves: out curves,
                                intersectionPoints: out points)),
                (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && b == typeof(Plane) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) =>
                    PairPolylines<TA, TB, Mesh, Plane, TOut>(
                        a: GeometryRequirement.MeshCheck,
                        b: GeometryRequirement.None,
                        intersect: static (Mesh left, Plane right, GeometryContext context) => {
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
                        a: GeometryRequirement.MeshCheck,
                        b: GeometryRequirement.None,
                        output: static (Mesh left, Line right, GeometryContext _) => IntersectKey.IntersectionOutput<TOut>(
                            points: Intersection.MeshLineSorted(
                                mesh: left,
                                line: right,
                                faceIds: out int[] _))),
                (Type a, Type b, Type output) when typeof(Mesh).IsAssignableFrom(c: a) && typeof(Mesh).IsAssignableFrom(c: b) && (output == typeof(Polyline) || output == typeof(IntersectionKind)) =>
                    PairPolylines<TA, TB, Mesh, Mesh, TOut>(
                        a: GeometryRequirement.MeshCheck,
                        b: GeometryRequirement.MeshCheck,
                        intersect: static (Mesh left, Mesh right, GeometryContext context) => {
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
    public static Query<(TA A, TB B), TOut> Deviation<TA, TB, TOut>(Deviation aspect) where TA : notnull where TB : notnull =>
        Aspect<(TA A, TB B), TOut, Deviation>(
            aspect: aspect,
            key: DeviationKey,
            dispatch: static (Deviation candidate) => (candidate.Kind, typeof(TA), typeof(TB), typeof(TOut)) switch {
                (DeviationKind.None, _, _, _) => Query<(TA A, TB B), TOut>.Reject(
                    key: DeviationKey,
                    fault: DeviationKey.InvalidInput()),
                (DeviationKind.Curve, Type a, Type b, Type output) when typeof(Curve).IsAssignableFrom(c: a) && typeof(Curve).IsAssignableFrom(c: b) && output == typeof(CurveDeviation) =>
                    Pair<TA, TB, Curve, Curve, TOut>(
                        key: DeviationKey,
                        a: GeometryRequirement.CurveLength,
                        b: GeometryRequirement.CurveLength,
                        output: static (Curve left, Curve right, GeometryContext context) => CurveDeviationValue<TOut>(left: left, right: right, context: context)),
                _ => null,
            });
    private static Fin<Seq<TOut>> CurveDeviationValue<TOut>(Curve left, Curve right, GeometryContext context) =>
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
                    true => new CurveDeviation(
                        MinimumDistance: minimumDistance,
                        MinimumA: left.PointAt(t: minimumA),
                        MinimumB: right.PointAt(t: minimumB),
                        MaximumDistance: maximumDistance,
                        MaximumA: left.PointAt(t: maximumA),
                        MaximumB: right.PointAt(t: maximumB),
                        Tolerance: context.Absolute.Value,
                        WithinTolerance: maximumDistance <= context.Absolute.Value) switch {
                            CurveDeviation deviation when deviation.MinimumDistance >= 0.0
                                && deviation.MaximumDistance >= deviation.MinimumDistance
                                && RhinoMath.IsValidDouble(x: deviation.MinimumDistance)
                                && RhinoMath.IsValidDouble(x: deviation.MaximumDistance)
                                && deviation.MinimumA.IsValid
                                && deviation.MinimumB.IsValid
                                && deviation.MaximumA.IsValid
                                && deviation.MaximumB.IsValid =>
                                Fin.Succ(Seq(deviation))
                                    .Bind(static (Seq<CurveDeviation> values) => DeviationKey.Retype<CurveDeviation, TOut>(values: values)),
                            _ => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                        },
                    false => Fin.Fail<Seq<TOut>>(DeviationKey.InvalidResult()),
                };
    private static Query<(TA A, TB B), TOut> Pair<TA, TB, TLeft, TRight, TOut>(
        OperationKey key,
        GeometryRequirement a,
        GeometryRequirement b,
        PairOutput<TLeft, TRight, TOut> output) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Query<(TA A, TB B), TOut>.Build(
            key: key,
            requiresContext: true,
            state: (Key: key, A: a, B: b, Output: output),
            evaluator: static ((OperationKey Key, GeometryRequirement A, GeometryRequirement B, PairOutput<TLeft, TRight, TOut> Output) state, (TA A, TB B) geometry) =>
                from rt in Analyze.Asks
                from validated in rt.Context.ValidateOperands(
                        geometry: geometry,
                        a: state.A,
                        b: state.B)
                    .ToEff()
                from result in PairOutputValue(
                        state: state,
                        geometry: validated,
                        context: rt.Context)
                    .ToEff()
                select result);
    private static Fin<Seq<TOut>> PairOutputValue<TA, TB, TLeft, TRight, TOut>(
        (OperationKey Key, GeometryRequirement A, GeometryRequirement B, PairOutput<TLeft, TRight, TOut> Output) state,
        (TA A, TB B) geometry,
        GeometryContext context) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        (geometry.A, geometry.B) switch {
            (TLeft left, TRight right) => state.Output(
                left: left,
                right: right,
                context: context),
            _ => Fin.Fail<Seq<TOut>>(state.Key.Unsupported(
                geometryType: typeof((TA A, TB B)),
                outputType: typeof(TOut))),
        };
    private static Query<(TA A, TB B), TOut> PairEvents<TA, TB, TLeft, TRight, TOut>(
        GeometryRequirement a,
        GeometryRequirement b,
        Func<TLeft, TRight, GeometryContext, CurveIntersections?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (TLeft left, TRight right, GeometryContext context) => {
                using CurveIntersections? intersections = intersect(
                    arg1: left,
                    arg2: right,
                    arg3: context);
                return IntersectKey.IntersectionOutput<TOut>(intersections: intersections);
            });
    private static Query<(TA A, TB B), TOut> PairCurvePoint<TA, TB, TLeft, TRight, TOut>(
        GeometryRequirement a,
        GeometryRequirement b,
        bool acceptPartialResults,
        CurvePointIntersection<TLeft, TRight> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (TLeft left, TRight right, GeometryContext context) => intersect(
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
        GeometryRequirement a,
        GeometryRequirement b,
        Func<TLeft, TRight, GeometryContext, IEnumerable<Polyline>?> intersect) where TA : notnull where TB : notnull where TLeft : notnull where TRight : notnull =>
        Pair<TA, TB, TLeft, TRight, TOut>(
            key: IntersectKey,
            a: a,
            b: b,
            output: (TLeft left, TRight right, GeometryContext context) => IntersectKey.IntersectionOutput<TOut>(
                polylines: intersect(
                    arg1: left,
                    arg2: right,
                    arg3: context)));
    private static bool Events(Type output) =>
        output == typeof(IntersectionEvent) || output == typeof(Point3d) || output == typeof(IntersectionKind);
    private static bool Curves(Type output) =>
        output == typeof(Curve) || output == typeof(Point3d) || output == typeof(IntersectionKind);
}
