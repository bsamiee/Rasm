using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Crossing<T> {
    public sealed record Disjoint : Crossing<T>;

    public sealed record One(T At) : Crossing<T>;

    public sealed record Two(T First, T Second) : Crossing<T>;

    public sealed record Coincident : Crossing<T>;
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SphereCrossing {
    public sealed record Disjoint : SphereCrossing;

    public sealed record Tangent(Point3d At) : SphereCrossing;

    public sealed record Circular(Circle Circle) : SphereCrossing;

    public sealed record Coincident : SphereCrossing;
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveCrossing<TAt, TSpan> {
    public sealed record Hit(double A, TAt B, Point3d OnA, Point3d OnB) : CurveCrossing<TAt, TSpan>;

    public sealed record Overlap(Interval A, TSpan B) : CurveCrossing<TAt, TSpan>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Sets<T> {
    public sealed record Within(Seq<T> Items) : Sets<T>;

    public sealed record Between(Seq<T> A, Seq<T> B) : Sets<T>;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurvesAndPoints {
    public sealed record BrepBrep(Brep A, Brep B, bool JoinCurves) : CurvesAndPoints;

    public sealed record BrepSurface(Brep Brep, Surface Surface, bool JoinCurves) : CurvesAndPoints;

    public sealed record SurfaceSurface(Surface A, Surface B) : CurvesAndPoints;

    public sealed record CurveBrepFace(Curve Curve, BrepFace Face) : CurvesAndPoints;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record MeshPoints {
    public sealed record MeshLineSorted(Mesh Mesh, Line Line) : MeshPoints;

    public sealed record MeshPolylineSorted(Mesh Mesh, PolylineCurve Polyline) : MeshPoints;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ProjectionTargets {
    public sealed record OnMeshes(Seq<Mesh> Meshes, Vector3d Direction) : ProjectionTargets;

    public sealed record OnBreps(Seq<Brep> Breps, Vector3d Direction) : ProjectionTargets;

    public sealed record OnPlane(Plane Plane, Option<Vector3d> Along) : ProjectionTargets {
        public Fin<Transform> Projection =>
            Along.Match(Some: along => Transform.ProjectAlong(Plane, along), None: () => Transform.PlanarProjection(Plane)) switch {
                var xform => RhinoFault.Unless(!xform.IsIdentity).Map(_ => xform),
            };
    }
}

[Union<BoundingBox, Sphere>(SkipToString = true, SkipEqualityComparison = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SearchRegion;

public sealed record MeshCrossings(Seq<Polyline> Polylines, Seq<Polyline> Overlaps, Option<Mesh> OverlapMesh, Option<string> Log);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Intersections {
    // --- [ANALYTIC]
    public static Option<(double A, double B)> LineLine(Line lineA, Line lineB) =>
        Conversions.Found(Intersection.LineLine(lineA, lineB, out double a, out double b), (A: a, B: b));

    public static Option<(double A, double B)> LineLine(Tolerances tolerances, Line lineA, Line lineB, bool finiteSegments) =>
        Conversions.Found(Intersection.LineLine(lineA, lineB, out double a, out double b, tolerances.Absolute, finiteSegments), (A: a, B: b));

    public static Option<double> LinePlane(Line line, Plane plane) =>
        Conversions.Found(Intersection.LinePlane(line, plane, out double t), t);

    public static Option<Line> PlanePlane(Plane planeA, Plane planeB) =>
        Conversions.Found(Intersection.PlanePlane(planeA, planeB, out Line line), line);

    public static Option<Point3d> PlanePlanePlane(Plane planeA, Plane planeB, Plane planeC) =>
        Conversions.Found(Intersection.PlanePlanePlane(planeA, planeB, planeC, out Point3d point), point);

    public static Option<Interval> LineBox(Tolerances tolerances, Line line, Box box) =>
        Conversions.Found(Intersection.LineBox(line, box, tolerances.Absolute, out Interval span), span);

    public static Option<Polyline> PlaneBoundingBox(Plane plane, BoundingBox box) =>
        Conversions.Found(Intersection.PlaneBoundingBox(plane, box, out Polyline polyline), polyline);

    public static Crossing<(double T, Point3d Point)> LineCircle(Line line, Circle circle) =>
        Intersection.LineCircle(line, circle, out double t1, out Point3d point1, out double t2, out Point3d point2) switch {
            LineCircleIntersection.None => new Crossing<(double T, Point3d Point)>.Disjoint(),
            LineCircleIntersection.Single => new Crossing<(double T, Point3d Point)>.One((t1, point1)),
            LineCircleIntersection.Multiple => new Crossing<(double T, Point3d Point)>.Two((t1, point1), (t2, point2)),
        };

    public static Crossing<Point3d> LineSphere(Line line, Sphere sphere) =>
        Intersection.LineSphere(line, sphere, out Point3d first, out Point3d second) switch {
            LineSphereIntersection.None => new Crossing<Point3d>.Disjoint(),
            LineSphereIntersection.Single => new Crossing<Point3d>.One(first),
            LineSphereIntersection.Multiple => new Crossing<Point3d>.Two(first, second),
        };

    public static Crossing<Point3d> LineCylinder(Line line, Cylinder cylinder) =>
        Intersection.LineCylinder(line, cylinder, out Point3d first, out Point3d second) switch {
            LineCylinderIntersection.None => new Crossing<Point3d>.Disjoint(),
            LineCylinderIntersection.Single => new Crossing<Point3d>.One(first),
            LineCylinderIntersection.Multiple => new Crossing<Point3d>.Two(first, second),
            LineCylinderIntersection.Overlap => new Crossing<Point3d>.Coincident(),
        };

    public static Crossing<Point3d> ArcArc(Arc arcA, Arc arcB) =>
        Intersection.ArcArc(arcA, arcB, out Point3d first, out Point3d second) switch {
            ArcArcIntersection.None => new Crossing<Point3d>.Disjoint(),
            ArcArcIntersection.Single => new Crossing<Point3d>.One(first),
            ArcArcIntersection.Multiple => new Crossing<Point3d>.Two(first, second),
            ArcArcIntersection.Overlap => new Crossing<Point3d>.Coincident(),
        };

    public static Crossing<Point3d> CircleCircle(Circle circleA, Circle circleB) =>
        Intersection.CircleCircle(circleA, circleB, out Point3d first, out Point3d second) switch {
            CircleCircleIntersection.None => new Crossing<Point3d>.Disjoint(),
            CircleCircleIntersection.Single => new Crossing<Point3d>.One(first),
            CircleCircleIntersection.Multiple => new Crossing<Point3d>.Two(first, second),
            CircleCircleIntersection.Overlap => new Crossing<Point3d>.Coincident(),
        };

    public static Crossing<double> PlaneCircle(Plane plane, Circle circle) =>
        Intersection.PlaneCircle(plane, circle, out double first, out double second) switch {
            PlaneCircleIntersection.None or PlaneCircleIntersection.Parallel => new Crossing<double>.Disjoint(),
            PlaneCircleIntersection.Tangent => new Crossing<double>.One(first),
            PlaneCircleIntersection.Secant => new Crossing<double>.Two(first, second),
            PlaneCircleIntersection.Coincident => new Crossing<double>.Coincident(),
        };

    public static SphereCrossing PlaneSphere(Plane plane, Sphere sphere) =>
        Intersection.PlaneSphere(plane, sphere, out Circle circle) switch {
            PlaneSphereIntersection.None => new SphereCrossing.Disjoint(),
            PlaneSphereIntersection.Point => new SphereCrossing.Tangent(circle.Center),
            PlaneSphereIntersection.Circle => new SphereCrossing.Circular(circle),
        };

    public static SphereCrossing SphereSphere(Sphere sphereA, Sphere sphereB) =>
        Intersection.SphereSphere(sphereA, sphereB, out Circle circle) switch {
            SphereSphereIntersection.None => new SphereCrossing.Disjoint(),
            SphereSphereIntersection.Point => new SphereCrossing.Tangent(circle.Center),
            SphereSphereIntersection.Circle => new SphereCrossing.Circular(circle),
            SphereSphereIntersection.Overlap => new SphereCrossing.Coincident(),
        };

    // --- [CURVE_EVENTS]
    public static IO<(Seq<CurveCrossing<double, Interval>> Crossings, Seq<int> Invalid, string Log)> CurveCurve(Tolerances tolerances, Curve curveA, Curve curveB, double overlap) =>
        Validated(() => (Intersection.CurveCurveValidate(curveA, curveB, tolerances.Absolute, overlap, out int[] invalid, out TextLog log), invalid, log), OnCurve, OverlapOnCurve);

    public static IO<Seq<CurveCrossing<double, Interval>>> CurveSelf(Tolerances tolerances, Curve curve) =>
        Crossings(() => Intersection.CurveSelf(curve, tolerances.Absolute), OnCurve, OverlapOnCurve);

    public static IO<Seq<CurveCrossing<double, Interval>>> CurveLine(Tolerances tolerances, Curve curve, Line line, double overlap) =>
        Crossings(() => Intersection.CurveLine(curve, line, tolerances.Absolute, overlap), OnCurve, OverlapOnCurve);

    public static IO<(Seq<CurveCrossing<Point2d, (Interval U, Interval V)>> Crossings, Seq<int> Invalid, string Log)> CurveSurface(Tolerances tolerances, Curve curve, Surface surface, Option<Interval> domain, double overlap) =>
        Validated(() => domain.Match(
                Some: span => (Intersection.CurveSurfaceValidate(curve, span, surface, tolerances.Absolute, overlap, out int[] invalid, out TextLog log), invalid, log),
                None: () => (Intersection.CurveSurfaceValidate(curve, surface, tolerances.Absolute, overlap, out int[] invalid, out TextLog log), invalid, log)),
            static crossing => {
                crossing.SurfacePointParameter(out double u, out double v);
                return new Point2d(u, v);
            },
            static crossing => {
                crossing.SurfaceOverlapParameter(out Interval u, out Interval v);
                return (U: u, V: v);
            });

    public static IO<Seq<CurveCrossing<Unit, Unit>>> CurvePlane(Tolerances tolerances, Curve curve, Plane plane) =>
        Crossings(() => Intersection.CurvePlane(curve, plane, tolerances.Absolute), static _ => unit, static _ => unit);

    private static double OnCurve(IntersectionEvent crossing) => crossing.ParameterB;

    private static Interval OverlapOnCurve(IntersectionEvent crossing) => crossing.OverlapB;

    private static IO<(Seq<CurveCrossing<TAt, TSpan>> Crossings, Seq<int> Invalid, string Log)> Validated<TAt, TSpan>(
        Func<(CurveIntersections? Events, int[] Invalid, TextLog Log)> host, Func<IntersectionEvent, TAt> at, Func<IntersectionEvent, TSpan> span) =>
        (from answer in IO.lift(host)
         from log in use(() => answer.Log)
         from crossings in Crossings(() => answer.Events, at, span)
         select (Crossings: crossings, Invalid: toSeq(answer.Invalid), Log: log.ToString())).Bracket();

    private static IO<Seq<CurveCrossing<TAt, TSpan>>> Crossings<TAt, TSpan>(Func<CurveIntersections?> host, Func<IntersectionEvent, TAt> at, Func<IntersectionEvent, TSpan> span) =>
        (from events in IO.lift(host)
         from held in Products.Held(events)
         select Conversions.Rows(events)
             .Map<CurveCrossing<TAt, TSpan>>(crossing => crossing.IsPoint
                 ? new CurveCrossing<TAt, TSpan>.Hit(crossing.ParameterA, at(crossing), crossing.PointA, crossing.PointB)
                 : new CurveCrossing<TAt, TSpan>.Overlap(crossing.OverlapA, span(crossing)))
             .Strict()).Bracket();

    // --- [SURFACES]
    public static IO<(Seq<Curve> Curves, Seq<Point3d> Points)> CurvesAndPoints(Tolerances tolerances, CurvesAndPoints operands) =>
        operands.Switch(
            tolerances.Absolute,
            brepBrep: static (absolute, pair) => Traced(Intersection.BrepBrep(pair.A, pair.B, absolute, pair.JoinCurves, out Curve[] curves, out Point3d[] points), curves, points),
            brepSurface: static (absolute, pair) => Traced(Intersection.BrepSurface(pair.Brep, pair.Surface, absolute, pair.JoinCurves, out Curve[] curves, out Point3d[] points), curves, points),
            surfaceSurface: static (absolute, pair) => Traced(Intersection.SurfaceSurface(pair.A, pair.B, absolute, out Curve[] curves, out Point3d[] points), curves, points),
            curveBrepFace: static (absolute, pair) => Traced(Intersection.CurveBrepFace(pair.Curve, pair.Face, absolute, out Curve[] curves, out Point3d[] points), curves, points));

    public static IO<(Seq<Curve> Overlaps, Seq<CurveSample> Hits)> CurveBrep(Tolerances tolerances, Curve curve, Brep brep) =>
        Intersection.CurveBrep(curve, brep, tolerances.Absolute, out Curve[] overlaps, out Point3d[] points, out double[] parameters) switch {
            var accepted =>
                from held in Products.Made(overlaps)
                from traced in IO.lift(() => RhinoFault.Unless(accepted))
                select (Overlaps: held, Hits: CurveSample.Of(parameters, points)),
        };

    private static IO<(Seq<Curve> Curves, Seq<Point3d> Points)> Traced(bool accepted, Curve[] curves, Point3d[] points, [CallerArgumentExpression(nameof(accepted))] string call = "") =>
        from held in Products.Made(curves, call)
        from traced in IO.lift(() => RhinoFault.Unless(accepted, call: call))
        select (Curves: held, Points: toSeq(points));

    // --- [MESHES]
    public static IO<MeshCrossings> MeshMesh(Tolerances tolerances, Sets<Mesh> meshes, bool overlaps, bool overlapMesh, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from crossed in meshes.Switch(
            (Tolerance: tolerances.MeshIntersection, Overlaps: overlaps, OverlapMesh: overlapMesh, Progress: progress.ValueUnsafe(), Token: token),
            within: static (at, within) => Crossed(log => (
                Intersection.MeshMesh(within.Items, at.Tolerance, out Polyline[] polylines, at.Overlaps, out Polyline[] overlapped, at.OverlapMesh, out Mesh merged, log, at.Token, at.Progress),
                polylines, overlapped, merged)),
            between: static (at, between) => Crossed(log => (
                Intersection.MeshMeshTwoSets(between.A, between.B, at.Tolerance, out Polyline[] polylines, at.Overlaps, out Polyline[] overlapped, at.OverlapMesh, out Mesh merged, log, at.Token, at.Progress),
                polylines, overlapped, merged)))
        select crossed;

    public static IO<Seq<IntersectingMeshPair>> Predicate(Tolerances tolerances, Sets<Mesh> meshes, bool fast) =>
        from token in cancelToken
        from pairs in IO.lift(() => Operands(meshes) switch {
            var (a, b) => Predicated(log => (
                Intersection.MeshMeshPredicate(a, b, tolerances.MeshIntersection, fast, out IntersectingMeshPair[] found, log, token), toSeq(found)), token),
        })
        select pairs;

    public static IO<bool> Intersects(Tolerances tolerances, Sets<Mesh> meshes, bool fast) =>
        from token in cancelToken
        from any in IO.lift(() => Operands(meshes) switch {
            var (a, b) => Predicated(log => Intersection.MeshMeshPredicate(a, b, tolerances.MeshIntersection, fast, log, token) switch {
                var found => (found, found),
            }, token),
        })
        select any;

    public static IO<Option<(double T, Seq<int> Faces)>> MeshRay(Mesh mesh, Ray3d ray) =>
        IO.lift(() => Some((T: Intersection.MeshRay(mesh, ray, out int[] faces), Faces: faces))
            .Filter(static hit => hit.T >= 0.0)
            .Map(static hit => (hit.T, Faces: Conversions.Rows(hit.Faces))));

    public static IO<Seq<Option<double>>> MeshRays(Mesh mesh, Seq<Ray3d> rays, bool multithreaded) =>
        IO.lift(() => Conversions.Rows(Intersection.MeshRays(mesh, rays, multithreaded)).Map(static t => Some(t).Filter(static at => at >= 0.0)).Strict());

    public static IO<Seq<(Point3d Point, int Face)>> MeshPoints(MeshPoints crossing) =>
        IO.lift(() => crossing.Switch(
            meshLineSorted: static at => (Points: Intersection.MeshLineSorted(at.Mesh, at.Line, out int[] faces), Faces: faces),
            meshPolylineSorted: static at => (Points: Intersection.MeshPolylineSorted(at.Mesh, at.Polyline, out int[] faces), Faces: faces)) switch {
                var (points, faces) => toSeq(points).Zip(toSeq(faces), static (point, face) => (Point: point, Face: face)).Strict(),
            });

    private static IO<MeshCrossings> Crossed(Func<TextLog, (bool Accepted, Polyline[] Polylines, Polyline[] Overlaps, Mesh? OverlapMesh)> host, [CallerArgumentExpression(nameof(host))] string call = "") =>
        from log in use(static () => new TextLog())
        from crossed in IO.lift(() => host(log))
        from mesh in Products.Held(crossed.OverlapMesh, call)
        let text = log.ToString()
        from accepted in IO.lift(() => crossed.Accepted ? Fin.Succ(unit) : new RhinoFault(call, RhinoReason.Declined, Inner: Conversions.Present(text).Map(static report => Error.New(report))))
        select new MeshCrossings(Conversions.Rows(crossed.Polylines), Conversions.Rows(crossed.Overlaps), mesh, Conversions.Present(text));

    private static (IEnumerable<Mesh> A, IEnumerable<Mesh>? B) Operands(Sets<Mesh> meshes) =>
        meshes.Switch<(IEnumerable<Mesh> A, IEnumerable<Mesh>? B)>(
            within: static within => (within.Items, null),
            between: static between => (between.A, between.B));

    private static Fin<T> Predicated<T>(Func<TextLog, (bool Any, T Value)> host, CancellationToken token) {
        using TextLog log = new();
        (bool any, T value) = host(log);
        return token.IsCancellationRequested ? Errors.Cancelled
            : any ? value
            : Conversions.Unreported(log.ToString()).Map(_ => value);
    }

    // --- [RAYS_AND_PROJECTIONS]
    public static IO<Seq<(Point3d Point, int Geometry, Option<int> Face)>> RayShoot(Seq<GeometryBase> geometry, Ray3d ray, int reflections, bool honorTrims) =>
        IO.lift(() => Conversions.Rows(Intersection.RayShoot(geometry, ray, reflections, honorTrims))
            .Map(static hit => (hit.Point, Geometry: hit.GeometryIndex, Face: Conversions.Present(hit.BrepFaceIndex)))
            .Strict());

    public static IO<Seq<(int Source, Point3d Point)>> Project(Tolerances tolerances, ProjectionTargets targets, Seq<Point3d> points) =>
        IO.lift(() => targets.Switch(
            (Points: points, tolerances.Absolute),
            onMeshes: static (at, target) => Conversions.Found(Intersection.ProjectPointsToMeshesEx(target.Meshes, at.Points, target.Direction, at.Absolute, out int[] sources))
                .Map(projected => toSeq(sources).Zip(toSeq(projected), static (source, point) => (Source: source, Point: point)).Strict()),
            onBreps: static (at, target) => Conversions.Found(Intersection.ProjectPointsToBrepsEx(target.Breps, at.Points, target.Direction, at.Absolute, out int[] sources))
                .Map(projected => toSeq(sources).Zip(toSeq(projected), static (source, point) => (Source: source, Point: point)).Strict()),
            onPlane: static (at, target) => target.Projection.Map(xform => at.Points.Map((point, index) => (Source: index, Point: xform * point)).Strict())));

    public static IO<Seq<(Curve Curve, Option<int> Source, Option<int> Target)>> Project(Tolerances tolerances, ProjectionTargets targets, Seq<Curve> curves, bool loose) =>
        targets.Switch(
            (Curves: curves, tolerances.Absolute, Loose: loose),
            onMeshes: static (at, target) => Products.Made(Curve.ProjectToMesh(at.Curves, target.Meshes, target.Direction, at.Absolute, at.Loose))
                .Map(static projected => projected.Map(static curve => (Curve: curve, Source: Option<int>.None, Target: Option<int>.None)).Strict()),
            onBreps: static (at, target) => Products.Made(Curve.ProjectToBrep(at.Curves, target.Breps, target.Direction, at.Absolute, at.Loose, out int[]? sources, out int[]? targets))
                .Map(projected => projected.Zip(Conversions.Rows(sources).Zip(Conversions.Rows(targets)),
                    static (curve, row) => (Curve: curve, Source: Some(row.First), Target: Some(row.Second))).Strict()),
            onPlane: static (at, target) =>
                from xform in IO.lift(() => target.Projection)
                from projected in at.Curves.Map(static (curve, index) => (Curve: curve, Index: index)).TraverseM(row =>
                    from copy in Copies.Duplicate(row.Curve)
                    from moved in Transformations.Transformed(copy, xform)
                    select (Curve: moved, Source: Some(row.Index), Target: Option<int>.None)).As()
                select projected);

    // --- [PULLS]
    public static IO<Curve> PullToMesh(Tolerances tolerances, Curve curve, Mesh mesh, bool loose) =>
        Products.Made(curve.PullToMesh(mesh, tolerances.Absolute, loose));

    public static IO<Seq<Curve>> PullToBrepFace(Tolerances tolerances, Curve curve, BrepFace face, bool loose) =>
        Products.Made(Curve.PullToBrepFace(curve, face, tolerances.Absolute, loose));

    public static IO<PolylineCurve> PullCurve(Tolerances tolerances, Mesh mesh, Curve curve) =>
        Products.Made(mesh.PullCurve(curve, tolerances.Absolute));

    // --- [OVERLAPS]
    public static IO<Seq<(int A, int B)>> SearchOverlaps(Sets<BoundingBox> boxes, double tolerance) =>
        boxes.Switch(
            tolerance,
            within: static (distance, within) => Planted(within.Items, tree => IO.lift(() =>
                Collected(found => tree.SearchOverlaps(distance, found, tag: null), static found => (A: found.Id, B: found.IdB)) switch {
                    var (searched, pairs) => RhinoFault.Unless(searched).Map(_ => pairs),
                })),
            between: static (distance, between) => Planted(between.A, treeA => Planted(between.B, treeB => IO.lift(() =>
                Collected(found => RTree.SearchOverlaps(treeA, treeB, distance, found), static found => (A: found.Id, B: found.IdB)) switch {
                    var (searched, pairs) => RhinoFault.Unless(searched).Map(_ => pairs),
                }))));

    public static IO<Seq<Seq<int>>> Search(Seq<BoundingBox> items, Seq<SearchRegion> queries) =>
        Planted(items, tree => IO.lift(() => Callbacks.Each(queries, (query, index) => query.Switch(
            tree,
            boundingBox: static (held, box) => Collected(found => held.Search(box, found), static found => found.Id),
            sphere: static (held, sphere) => Collected(found => held.Search(sphere, found), static found => found.Id)) switch {
                var (searched, hits) => RhinoFault.Unless(searched, index).Map(_ => hits),
            })));

    public static IO<Seq<(int Object, bool Entire, Seq<ComponentIndex> Removed, Option<Curve> Replacement)>> GetOverlaps(
        Tolerances tolerances, Seq<GeometryBase> objects, Plane plane, bool wholeCurves, Seq<GeometryBase> locked, bool lockedExclusive) =>
        GeometryBase.GetOverlaps(objects, plane, tolerances.Absolute, wholeCurves, locked, lockedExclusive, out bool[] entire, out ComponentIndex[][] removed, out Curve[] replacements) switch {
            var indices => IO.lift(() => Conversions.Found(indices)).Bind(found => Callbacks.Each(toSeq(found).Map((index, at) => Products.Held(replacements[at])
                .Map(replacement => (Object: index, Entire: entire[at], Removed: toSeq(removed[at]), Replacement: replacement))).Strict())),
        };

    private static IO<T> Planted<T>(Seq<BoundingBox> boxes, Func<RTree, IO<T>> body) =>
        (from tree in use(static () => new RTree())
         from planted in IO.lift(() => Callbacks.Each(boxes, (box, index) => RhinoFault.Unless(tree.Insert(box, index), index)))
         from found in body(tree)
         select found).Bracket();

    private static (bool Searched, Seq<T> Rows) Collected<T>(Func<EventHandler<RTreeEventArgs>, bool> search, Func<RTreeEventArgs, T> row) {
        List<T> rows = [];
        bool searched = search((_, found) => rows.Add(row(found)));
        return (searched, toSeq(rows).Strict());
    }

    // --- [CLASHES]
    public static IO<Seq<Option<MeshClash>>> Clashes(Seq<Mesh> setA, Seq<Mesh> setB, double distance, int maxEvents) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, maxEvents)).Map(static clash => Optional(clash.MeshA).Map(_ => clash)).Strict());

    public static IO<Seq<Option<MeshInterference>>> Interferences(Seq<RhinoObject> setA, Seq<RhinoObject> setB, double distance, MeshType meshType, MeshingParameters parameters) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, meshType, parameters)).Map(static found => Conversions.Present(found.IndexA).Map(_ => found)).Strict());

    public static IO<Seq<Mesh>> FindDetail(RhinoObject objectA, RhinoObject objectB, double distance, MeshType meshType, MeshingParameters parameters) =>
        Products.Made(MeshClash.FindDetail(objectA, objectB, distance, meshType, parameters));
}
