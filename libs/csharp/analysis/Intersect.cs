using System.Linq;
using System.Threading;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.FileIO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;

namespace Analysis;

// --- [INTERSECT] -------------------------------------------------------------------------------

public static partial class Query {
    private delegate bool CurvePointIntersection<TGeometry>(
        TGeometry geometry,
        GeometryContext context,
        out Curve[] curves,
        out Point3d[] points);

    public static Query<TGeometry, TOut> Intersect<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type _) when Events<TOut>() && (geometry == typeof((Curve A, Curve B))
                || geometry == typeof((Curve Curve, Plane Plane))
                || geometry == typeof((Curve Curve, Line Line))
                || geometry == typeof((Curve Curve, Surface Surface))) =>
                EventIntersect<TGeometry, TOut>(),
            (Type geometry, Type _) when Curves<TOut>() && (geometry == typeof((Curve Curve, Brep Brep))
                || geometry == typeof((Curve Curve, BrepFace Face))) =>
                CurveBrepIntersect<TGeometry, TOut>(),
            (Type geometry, Type _) when Curves<TOut>() && (geometry == typeof((Surface A, Surface B))
                || geometry == typeof((Brep Brep, Plane Plane))
                || geometry == typeof((Brep Brep, Surface Surface))
                || geometry == typeof((Brep A, Brep B))) =>
                BrepSurfaceIntersect<TGeometry, TOut>(),
            (Type geometry, Type output) when geometry == typeof((Mesh Mesh, Plane Plane))
                || geometry == typeof((Mesh Mesh, Line Line))
                || geometry == typeof((Mesh A, Mesh B)) =>
                MeshIntersect<TGeometry, TOut>(output: output),
            _ => IntersectKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> EventIntersect<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TGeometry) switch {
            Type geometry when geometry == typeof((Curve A, Curve B)) =>
                Cast<TGeometry, TOut>(query: Event<(Curve A, Curve B), TOut>(
                    validate: static (geometry, context) => Both(geometry: geometry, context: context),
                    intersect: static (geometry, context) => Intersection.CurveCurve(
                        curveA: geometry.A,
                        curveB: geometry.B,
                        tolerance: context.Absolute.Value,
                        overlapTolerance: context.Absolute.Value))),
            Type geometry when geometry == typeof((Curve Curve, Plane Plane)) =>
                Cast<TGeometry, TOut>(query: Event<(Curve Curve, Plane Plane), TOut>(
                    validate: static (geometry, context) => One(geometry: geometry, context: context),
                    intersect: static (geometry, context) => Intersection.CurvePlane(
                        curve: geometry.Curve,
                        plane: geometry.Plane,
                        tolerance: context.Absolute.Value))),
            Type geometry when geometry == typeof((Curve Curve, Line Line)) =>
                Cast<TGeometry, TOut>(query: Event<(Curve Curve, Line Line), TOut>(
                    validate: static (geometry, context) => One(geometry: geometry, context: context),
                    intersect: static (geometry, context) => Intersection.CurveLine(
                        curve: geometry.Curve,
                        line: geometry.Line,
                        tolerance: context.Absolute.Value,
                        overlapTolerance: context.Absolute.Value))),
            Type geometry when geometry == typeof((Curve Curve, Surface Surface)) =>
                Cast<TGeometry, TOut>(query: Event<(Curve Curve, Surface Surface), TOut>(
                    validate: static (geometry, context) => Both(
                        geometry: geometry,
                        context: context,
                        b: GeometryRequirement.SurfaceEvaluation),
                    intersect: static (geometry, context) => Intersection.CurveSurface(
                        curve: geometry.Curve,
                        surface: geometry.Surface,
                        tolerance: context.Absolute.Value,
                        overlapTolerance: context.Absolute.Value))),
            _ => IntersectKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> CurveBrepIntersect<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TGeometry) switch {
            Type geometry when geometry == typeof((Curve Curve, Brep Brep)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Curve Curve, Brep Brep), TOut>(
                    validate: static (geometry, context) => Both(geometry: geometry, context: context),
                    intersect: static ((Curve Curve, Brep Brep) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.CurveBrep(
                            curve: geometry.Curve,
                            brep: geometry.Brep,
                            tolerance: context.Absolute.Value,
                            overlapCurves: out curves,
                            intersectionPoints: out points))),
            Type geometry when geometry == typeof((Curve Curve, BrepFace Face)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Curve Curve, BrepFace Face), TOut>(
                    validate: static (geometry, context) => Both(
                        geometry: geometry,
                        context: context,
                        b: GeometryRequirement.SurfaceEvaluation),
                    intersect: static ((Curve Curve, BrepFace Face) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.CurveBrepFace(
                            curve: geometry.Curve,
                            face: geometry.Face,
                            tolerance: context.Absolute.Value,
                            overlapCurves: out curves,
                            intersectionPoints: out points))),
            _ => IntersectKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> BrepSurfaceIntersect<TGeometry, TOut>() where TGeometry : notnull =>
        typeof(TGeometry) switch {
            Type geometry when geometry == typeof((Surface A, Surface B)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Surface A, Surface B), TOut>(
                    validate: static (geometry, context) => Both(
                        geometry: geometry,
                        context: context,
                        a: GeometryRequirement.SurfaceEvaluation,
                        b: GeometryRequirement.SurfaceEvaluation),
                    intersect: static ((Surface A, Surface B) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.SurfaceSurface(
                            surfaceA: geometry.A,
                            surfaceB: geometry.B,
                            tolerance: context.Absolute.Value,
                            intersectionCurves: out curves,
                            intersectionPoints: out points))),
            Type geometry when geometry == typeof((Brep Brep, Plane Plane)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Brep Brep, Plane Plane), TOut>(
                    validate: static (geometry, context) => One(geometry: geometry, context: context),
                    intersect: static ((Brep Brep, Plane Plane) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.BrepPlane(
                            brep: geometry.Brep,
                            plane: geometry.Plane,
                            tolerance: context.Absolute.Value,
                            intersectionCurves: out curves,
                            intersectionPoints: out points))),
            Type geometry when geometry == typeof((Brep Brep, Surface Surface)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Brep Brep, Surface Surface), TOut>(
                    validate: static (geometry, context) => Both(
                        geometry: geometry,
                        context: context,
                        b: GeometryRequirement.SurfaceEvaluation),
                    intersect: static ((Brep Brep, Surface Surface) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.BrepSurface(
                            brep: geometry.Brep,
                            surface: geometry.Surface,
                            tolerance: context.Absolute.Value,
                            joinCurves: true,
                            intersectionCurves: out curves,
                            intersectionPoints: out points))),
            Type geometry when geometry == typeof((Brep A, Brep B)) =>
                Cast<TGeometry, TOut>(query: CurvePoint<(Brep A, Brep B), TOut>(
                    validate: static (geometry, context) => Both(geometry: geometry, context: context),
                    intersect: static ((Brep A, Brep B) geometry, GeometryContext context, out Curve[] curves, out Point3d[] points) =>
                        Intersection.BrepBrep(
                            brepA: geometry.A,
                            brepB: geometry.B,
                            tolerance: context.Absolute.Value,
                            joinCurves: true,
                            intersectionCurves: out curves,
                            intersectionPoints: out points))),
            _ => IntersectKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> MeshIntersect<TGeometry, TOut>(Type output) where TGeometry : notnull =>
        typeof(TGeometry) switch {
            Type geometry when geometry == typeof((Mesh Mesh, Plane Plane)) && output == typeof(Polyline) =>
                Cast<TGeometry, TOut>(query: PolylineIntersection<(Mesh Mesh, Plane Plane), TOut>(
                    validate: static (geometry, context) => One(
                        geometry: geometry,
                        context: context,
                        requirement: GeometryRequirement.MeshCheck),
                    intersect: static (geometry, context) => {
                        using MeshIntersectionCache cache = new();
                        return Intersection.MeshPlane(
                            mesh: geometry.Mesh,
                            cache: cache,
                            plane: geometry.Plane,
                            tolerance: context.Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient);
                    })),
            Type geometry when geometry == typeof((Mesh Mesh, Line Line)) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<(Mesh Mesh, Line Line), Point3d>.Build(
                    key: IntersectKey,
                    requiresContext: true,
                    evaluator: static ((Mesh Mesh, Line Line) geometry, Fin<GeometryContext> context) =>
                        context
                            .Bind((GeometryContext model) => One(
                                    geometry: geometry,
                                    context: model,
                                    requirement: GeometryRequirement.MeshCheck)
                                .ToFin())
                            .Bind(static ((Mesh A, Line B) state) => Many(
                                key: IntersectKey,
                                values: Intersection.MeshLineSorted(
                                    mesh: state.A,
                                    line: state.B,
                                    faceIds: out int[] _))))),
            Type geometry when geometry == typeof((Mesh A, Mesh B)) && output == typeof(Polyline) =>
                Cast<TGeometry, TOut>(query: PolylineIntersection<(Mesh A, Mesh B), TOut>(
                    validate: static (geometry, context) => Both(
                        geometry: geometry,
                        context: context,
                        a: GeometryRequirement.MeshCheck,
                        b: GeometryRequirement.MeshCheck),
                    intersect: static (geometry, context) => {
                        using TextLog textLog = new();
                        return Intersection.MeshMesh(
                            meshes: [geometry.A, geometry.B],
                            tolerance: context.Absolute.Value,
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
                    })),
            _ => IntersectKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> Event<TGeometry, TOut>(
        Func<TGeometry, GeometryContext, Validation<Error, TGeometry>> validate,
        Func<TGeometry, GeometryContext, CurveIntersections?> intersect) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: IntersectKey,
            requiresContext: true,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext model) => validate(arg1: geometry, arg2: model)
                        .ToFin()
                        .Map((TGeometry valid) => (Geometry: valid, Context: model)))
                    .Bind(((TGeometry Geometry, GeometryContext Context) state) => {
                        using CurveIntersections? intersections = intersect(arg1: state.Geometry, arg2: state.Context);
                        return IntersectKey.IntersectionOutput<TOut>(intersections: intersections);
                    }));

    private static Query<TGeometry, TOut> CurvePoint<TGeometry, TOut>(
        Func<TGeometry, GeometryContext, Validation<Error, TGeometry>> validate,
        CurvePointIntersection<TGeometry> intersect) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: IntersectKey,
            requiresContext: true,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext model) => validate(arg1: geometry, arg2: model)
                        .ToFin()
                        .Map((TGeometry valid) => (Geometry: valid, Context: model)))
                    .Bind(((TGeometry Geometry, GeometryContext Context) state) => intersect(
                        geometry: state.Geometry,
                        context: state.Context,
                        curves: out Curve[] curves,
                        points: out Point3d[] points) switch {
                            true => IntersectKey.IntersectionOutput<TOut>(
                                curves: curves,
                                points: points),
                            false => Fin.Fail<Seq<TOut>>(IntersectKey.InvalidResult()),
                        }));

    private static Query<TGeometry, TOut> PolylineIntersection<TGeometry, TOut>(
        Func<TGeometry, GeometryContext, Validation<Error, TGeometry>> validate,
        Func<TGeometry, GeometryContext, IEnumerable<Polyline>?> intersect) where TGeometry : notnull =>
        Query<TGeometry, TOut>.Build(
            key: IntersectKey,
            requiresContext: true,
            evaluator: (TGeometry geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext model) => validate(arg1: geometry, arg2: model)
                        .ToFin()
                        .Map((TGeometry valid) => (Geometry: valid, Context: model)))
                    .Bind(((TGeometry Geometry, GeometryContext Context) state) =>
                        IntersectKey.IntersectionOutput<TOut>(
                            polylines: intersect(arg1: state.Geometry, arg2: state.Context))));

    private static Validation<Error, (TA A, TB B)> Both<TA, TB>(
        (TA A, TB B) geometry,
        GeometryContext context,
        GeometryRequirement a = default,
        GeometryRequirement b = default) where TA : GeometryBase where TB : GeometryBase =>
        (
            context.Validate(geometry: geometry.A, requirement: a == default ? GeometryRequirement.Basic : a),
            context.Validate(geometry: geometry.B, requirement: b == default ? GeometryRequirement.Basic : b)
        ).Apply(static (TA first, TB second) => (A: first, B: second))
        .As();

    private static Validation<Error, (TA A, TB B)> One<TA, TB>(
        (TA A, TB B) geometry,
        GeometryContext context,
        GeometryRequirement requirement = default) where TA : GeometryBase =>
        context
            .Validate(
                geometry: geometry.A,
                requirement: requirement == default ? GeometryRequirement.Basic : requirement)
            .Map((TA first) => (first, geometry.B));

    private static bool Events<TOut>() =>
        typeof(TOut) == typeof(IntersectionEvent) || typeof(TOut) == typeof(Point3d);

    private static bool Curves<TOut>() =>
        typeof(TOut) == typeof(Curve) || typeof(TOut) == typeof(Point3d);
}
