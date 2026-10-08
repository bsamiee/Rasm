using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rhino.DocObjects;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ReflectionCount : System.Numerics.IMinMaxValue<ReflectionCount> {
    public static ReflectionCount MinValue { get; } = new(1);
    public static ReflectionCount MaxValue { get; } = new(1000);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record Crossing<T> {
    public Option<Two> Secant =>
        Switch(
            disjoint: static _ => Option<Two>.None,
            one: static _ => Option<Two>.None,
            two: static two => Some(two),
            coincident: static _ => Option<Two>.None);

    public sealed record Disjoint : Crossing<T>;

    public sealed record One(T At) : Crossing<T>;

    public sealed record Two(T First, T Second) : Crossing<T>;

    public sealed record Coincident : Crossing<T>;
}

[Union]
public abstract partial record SphereCrossing {
    public sealed record Disjoint : SphereCrossing;

    public sealed record Tangent(Point3d At) : SphereCrossing;

    public sealed record Circular(Circle Circle) : SphereCrossing;

    public sealed record Coincident : SphereCrossing;
}

[Union]
public abstract partial record CurveCrossing<TAt, TSpan> {
    public sealed record Hit(double A, TAt B, Point3d OnA, Point3d OnB) : CurveCrossing<TAt, TSpan>;

    public sealed record Overlap(Interval A, TSpan B) : CurveCrossing<TAt, TSpan>;
}

[Union]
public abstract partial record ProjectionTargets {
    public sealed record OnMeshes(Seq<Mesh> Meshes) : ProjectionTargets;

    public sealed record OnBreps(Seq<Brep> Breps) : ProjectionTargets;
}

public sealed record MeshCrossings(Seq<Polyline> Polylines, Seq<Polyline> Overlaps, Option<Mesh> OverlapMesh);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Intersections {
    // --- [ANALYTIC]
    public static Option<(double A, double B)> LineLine(Line lineA, Line lineB) =>
        Callbacks.Found(Intersection.LineLine(lineA, lineB, out double a, out double b), (A: a, B: b));

    public static Option<double> LinePlane(Line line, Plane plane) =>
        Callbacks.Found(Intersection.LinePlane(line, plane, out double t), t);

    public static Option<Line> PlanePlane(Plane planeA, Plane planeB) =>
        Callbacks.Found(Intersection.PlanePlane(planeA, planeB, out Line line), line);

    public static Option<Point3d> PlanePlanePlane(Plane planeA, Plane planeB, Plane planeC) =>
        Callbacks.Found(Intersection.PlanePlanePlane(planeA, planeB, planeC, out Point3d point), point);

    public static Option<Interval> LineBox(Line line, Box box, Tolerances tolerances) =>
        Callbacks.Found(Intersection.LineBox(line, box, tolerances.Absolute, out Interval span), span);

    public static Option<Polyline> PlaneBoundingBox(Plane plane, BoundingBox box) =>
        Callbacks.Found(Intersection.PlaneBoundingBox(plane, box, out Polyline polyline), polyline);

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
    public static IO<Seq<CurveCrossing<double, Interval>>> CurveCrossings(Func<CurveIntersections?> host) =>
        Crossings(host, static crossing => crossing.ParameterB, static crossing => crossing.OverlapB);

    public static IO<Seq<CurveCrossing<Point2d, (Interval U, Interval V)>>> SurfaceCrossings(Func<CurveIntersections?> host) =>
        Crossings(
            host,
            static crossing => {
                crossing.SurfacePointParameter(out double u, out double v);
                return new Point2d(u, v);
            },
            static crossing => {
                crossing.SurfaceOverlapParameter(out Interval u, out Interval v);
                return (U: u, V: v);
            });

    public static IO<Seq<CurveCrossing<Unit, Unit>>> PlaneCrossings(Func<CurveIntersections?> host) =>
        Crossings(host, static _ => unit, static _ => unit);

    private static IO<Seq<CurveCrossing<TAt, TSpan>>> Crossings<TAt, TSpan>(Func<CurveIntersections?> host, Func<IntersectionEvent, TAt> at, Func<IntersectionEvent, TSpan> span) =>
        IO.lift(() => {
            using CurveIntersections? events = host();
            return Conversions.Rows(events)
                .Map<CurveCrossing<TAt, TSpan>>(crossing => crossing.IsPoint
                    ? new CurveCrossing<TAt, TSpan>.Hit(crossing.ParameterA, at(crossing), crossing.PointA, crossing.PointB)
                    : new CurveCrossing<TAt, TSpan>.Overlap(crossing.OverlapA, span(crossing)))
                .Strict();
        });

    // --- [CURVES_AND_POINTS]
    public static IO<(Seq<Curve> Curves, Seq<Point3d> Points)> Acquire(Func<(bool Accepted, Curve[] Curves, Point3d[] Points)> host, string member) =>
        from answer in IO.lift(host)
        from curves in Copies.Acquire(() => Refused.Unless(answer.Accepted, answer.Curves, member), member)
        select (curves, toSeq(answer.Points));

    // --- [MESHES]
    public static IO<MeshCrossings> MeshMesh(Seq<Mesh> meshes, Tolerances tolerances, bool overlaps, bool overlapMesh, Option<IProgress<double>> progress) =>
        cancelToken.Bind(token => IO.lift(() => Meshed(
            Intersection.MeshMesh(meshes, tolerances.MeshIntersection, out Polyline[] polylines, overlaps, out Polyline[] overlapped, overlapMesh, out Mesh merged, textLog: null, token, progress.ValueUnsafe()),
            polylines,
            overlapped,
            merged,
            token,
            nameof(Intersection.MeshMesh))));

    public static IO<MeshCrossings> MeshMeshTwoSets(Seq<Mesh> setA, Seq<Mesh> setB, Tolerances tolerances, bool overlaps, bool overlapMesh, Option<IProgress<double>> progress) =>
        cancelToken.Bind(token => IO.lift(() => Meshed(
            Intersection.MeshMeshTwoSets(setA, setB, tolerances.MeshIntersection, out Polyline[] polylines, overlaps, out Polyline[] overlapped, overlapMesh, out Mesh merged, textLog: null, token, progress.ValueUnsafe()),
            polylines,
            overlapped,
            merged,
            token,
            nameof(Intersection.MeshMeshTwoSets))));

    public static IO<Option<(double T, Seq<int> Faces)>> MeshRay(Mesh mesh, Ray3d ray) =>
        IO.lift(() => Some((T: Intersection.MeshRay(mesh, ray, out int[] faces), Faces: faces))
            .Filter(static hit => hit.T >= 0.0)
            .Map(static hit => (hit.T, Faces: Conversions.Rows(hit.Faces))));

    public static IO<Seq<Option<double>>> MeshRays(Mesh mesh, Seq<Ray3d> rays) =>
        IO.lift(() => Conversions.Rows(Intersection.MeshRays(mesh, rays)).Map(static t => Some(t).Filter(static at => at >= 0.0)).Strict());

    public static IO<Seq<(Point3d Point, int Face)>> MeshLine(Mesh mesh, Line line) =>
        IO.lift(() => Conversions.Rows(Intersection.MeshLine(mesh, line, out int[] faces)).Zip(Conversions.Rows(faces), static (point, face) => (Point: point, Face: face)).Strict());

    public static IO<Seq<(Point3d Point, int Face)>> MeshPolyline(Mesh mesh, PolylineCurve polyline) =>
        IO.lift(() => Conversions.Rows(Intersection.MeshPolyline(mesh, polyline, out int[] faces)).Zip(Conversions.Rows(faces), static (point, face) => (Point: point, Face: face)).Strict());

    private static Fin<MeshCrossings> Meshed(bool done, Polyline[]? polylines, Polyline[]? overlaps, Mesh? merged, CancellationToken token, string member) =>
        done
            ? new MeshCrossings(Conversions.Rows(polylines), Conversions.Rows(overlaps), Optional(merged))
            : token.IsCancellationRequested ? Errors.Cancelled : new Refused(member);

    // --- [RAYS_AND_PROJECTIONS]
    public static IO<Seq<(Point3d Point, int Geometry, Option<int> Face)>> RayShoot(Seq<GeometryBase> geometry, Ray3d ray, ReflectionCount reflections, bool honorTrims) =>
        IO.lift(() => Conversions.Rows(Intersection.RayShoot(geometry, ray, reflections, honorTrims))
            .Map(static hit => (hit.Point, Geometry: hit.GeometryIndex, Face: Conversions.Present(hit.BrepFaceIndex)))
            .Strict());

    public static IO<Seq<(int Source, Point3d Point)>> Project(ProjectionTargets targets, Seq<Point3d> points, Vector3d direction, Tolerances tolerances) =>
        IO.lift(() => targets.Switch(
            (Points: points, Direction: direction, tolerances.Absolute),
            onMeshes: static (state, on) => Missing.Unless(Intersection.ProjectPointsToMeshesEx(on.Meshes, state.Points, state.Direction, state.Absolute, out int[] sources), nameof(Intersection.ProjectPointsToMeshesEx))
                .Map(projected => toSeq(sources).Zip(toSeq(projected), static (source, point) => (Source: source, Point: point))),
            onBreps: static (state, on) => Missing.Unless(Intersection.ProjectPointsToBrepsEx(on.Breps, state.Points, state.Direction, state.Absolute, out int[] sources), nameof(Intersection.ProjectPointsToBrepsEx))
                .Map(projected => toSeq(sources).Zip(toSeq(projected), static (source, point) => (Source: source, Point: point)))));

    // --- [CLASHES]
    public static IO<Seq<MeshClash>> Clashes(Seq<Mesh> setA, Seq<Mesh> setB, double distance, int maxEvents) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, maxEvents)).Filter(static clash => Conversions.Present(clash.ClashPoint).IsSome).Strict());

    public static IO<Seq<MeshInterference>> Interferences(Seq<RhinoObject> setA, Seq<RhinoObject> setB, double distance, MeshType meshType, MeshingParameters parameters) =>
        IO.lift(() => toSeq(MeshClash.Search(setA, setB, distance, meshType, parameters)).Filter(static found => Conversions.Present(found.IndexA).IsSome).Strict());
}
