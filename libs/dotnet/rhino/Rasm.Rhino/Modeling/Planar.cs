using System.Globalization;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class PlanarRegion {
    public Curve Boundary { get; }
    public Seq<Curve> Holes { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Curve boundary, ref Seq<Curve> holes) =>
        validationError = boundary.IsClosed && holes.ForAll(static hole => hole.IsClosed) ? null : new RhinoFault("planar region needs a closed boundary and closed holes");
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class PlanarGeometry {
    public Seq<Point3d> Points { get; }
    public Seq<Curve> Curves { get; }
    public Seq<PlanarRegion> Regions { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Seq<Point3d> points, ref Seq<Curve> curves, ref Seq<PlanarRegion> regions) =>
        validationError = points.IsEmpty && curves.IsEmpty && regions.IsEmpty ? new RhinoFault("planar geometry needs a point, curve or region") : null;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ConcaveCriterion {
    public sealed record MaximumEdgeLength(double Length) : ConcaveCriterion;

    public sealed record MaximumEdgeLengthRatio(double Ratio) : ConcaveCriterion;

    public sealed record Alpha(double Radius) : ConcaveCriterion;
}

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class ConcaveTarget {
    public ConcaveCriterion Criterion { get; }
    public bool HolesAllowed { get; }

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref ConcaveCriterion criterion, ref bool holesAllowed) =>
        validationError = criterion.Switch(
            maximumEdgeLength: static of => double.IsFinite(of.Length) && of.Length >= 0,
            maximumEdgeLengthRatio: static of => of.Ratio is >= 0 and <= 1,
            alpha: static of => double.IsFinite(of.Radius) && of.Radius >= 0)
            ? null
            : new RhinoFault("concave hull target needs an edge length or alpha radius at or above 0, or an edge length ratio in [0, 1]");
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PlanarEnclosure {
    public sealed record Rectangle(Rectangle3d Bounds) : PlanarEnclosure;

    public sealed record Segment(Line Line) : PlanarEnclosure;

    public sealed record Point(Point3d Location) : PlanarEnclosure;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Planar {
    // --- [CIRCLES]
    public static IO<(Circle Circle, Point3d RadiusPoint)> MaximumInscribedCircle(Tolerances tolerances, Plane plane, IterableNE<PlanarRegion> regions) =>
        Analyzed(plane, scratch => Polygonal(tolerances, plane, toSeq(regions), scratch), polygonal =>
            Radius(plane, NetTopologySuite.Algorithm.Construct.MaximumInscribedCircle.GetRadiusLine(polygonal, tolerances.Absolute)));

    public static IO<(Circle Circle, Point3d RadiusPoint)> LargestEmptyCircle(Tolerances tolerances, Plane plane, PlanarGeometry obstacles, Seq<PlanarRegion> boundary) =>
        Analyzed(
            plane,
            scratch =>
                from held in Collection(tolerances, plane, obstacles, scratch)
                from within in Polygonal(tolerances, plane, boundary, scratch)
                select (Obstacles: held, Boundary: within),
            input => Radius(plane, NetTopologySuite.Algorithm.Construct.LargestEmptyCircle.GetRadiusLine(input.Obstacles, input.Boundary, tolerances.Absolute)));

    // --- [ENCLOSURES]
    public static IO<PlanarEnclosure> MinimumAreaRectangle(Tolerances tolerances, Plane plane, PlanarGeometry input) =>
        Analyzed(plane, scratch => Collection(tolerances, plane, input, scratch), geometry =>
            Enclosure(plane, NetTopologySuite.Algorithm.MinimumAreaRectangle.GetMinimumRectangle(geometry), nameof(NetTopologySuite.Algorithm.MinimumAreaRectangle.GetMinimumRectangle)));

    public static IO<(Line Diameter, Line SupportingSegment, PlanarEnclosure Rectangle)> Width(Tolerances tolerances, Plane plane, PlanarGeometry input) =>
        Analyzed(plane, scratch => Collection(tolerances, plane, input, scratch), geometry => new MinimumDiameter(geometry) switch {
            var diameter => Enclosure(plane, diameter.GetMinimumRectangle(), nameof(MinimumDiameter.GetMinimumRectangle))
                .Map(rectangle => (Diameter: Segment(plane, diameter.Diameter), SupportingSegment: Segment(plane, diameter.SupportingSegment), Rectangle: rectangle)),
        });

    // --- [HULLS]
    public static IO<(Polyline Boundary, Seq<Polyline> Holes)> ConcaveHull(Tolerances tolerances, Plane plane, Seq<Point3d> points, ConcaveTarget target) =>
        Analyzed(plane, _ => IO.lift(() => Callbacks.Each(points, (point, index) => Local(tolerances, plane, point, Subject("point", index)))), coordinates =>
            Region(plane, target.Criterion.Switch(
                (Points: Factory.CreateMultiPointFromCoords([.. coordinates]), target.HolesAllowed),
                maximumEdgeLength: static (at, of) => (Hull: NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLength(at.Points, of.Length, at.HolesAllowed),
                    Member: nameof(NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLength)),
                maximumEdgeLengthRatio: static (at, of) => (Hull: NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLengthRatio(at.Points, of.Ratio, at.HolesAllowed),
                    Member: nameof(NetTopologySuite.Algorithm.Hull.ConcaveHull.ConcaveHullByLengthRatio)),
                alpha: static (at, of) => (Hull: NetTopologySuite.Algorithm.Hull.ConcaveHull.AlphaShape(at.Points, of.Radius, at.HolesAllowed),
                    Member: nameof(NetTopologySuite.Algorithm.Hull.ConcaveHull.AlphaShape)))));

    // --- [DIAGRAMS]
    public static IO<Seq<(Point3d Site, Polyline Cell)>> VoronoiDiagram(Tolerances tolerances, Rectangle3d frame, Seq<Point3d> sites) =>
        from envelope in IO.lift(() => frame.IsValid && frame.Width > 0 && frame.Height > 0
            ? Fin.Succ(new Envelope(frame.X.Min, frame.X.Max, frame.Y.Min, frame.Y.Max))
            : Error.New(ErrorCodes.ValidationFailed, $"{nameof(VoronoiDiagram)} frame encloses no area"))
        from coordinates in IO.lift(() => Callbacks.Each(sites, (site, index) => Local(tolerances, frame.Plane, site, Subject("site", index)).Bind(at =>
            envelope.Covers(at) ? Fin.Succ(at) : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"site {index} lies outside the {nameof(VoronoiDiagram)} frame")))))
        from diagram in IO.lift(() => {
            NetTopologySuite.Triangulate.VoronoiDiagramBuilder builder = new() { ClipEnvelope = envelope, Tolerance = tolerances.Absolute };
            builder.SetSites([.. coordinates]);
            return builder.GetDiagram(Factory);
        })
        from cells in IO.lift(() => Callbacks.Each(toSeq(diagram.Geometries), (cell, index) => cell switch {
            Polygon { UserData: Coordinate site } polygon => Fin.Succ((Site: World(frame.Plane, site), Cell: Ring(frame.Plane, polygon.Shell))),
            _ => new Refused(nameof(NetTopologySuite.Triangulate.VoronoiDiagramBuilder.GetDiagram), $"returned {cell.GeometryType}", index),
        }))
        select cells;

    // --- [BOUNDARY]
    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance.CreateGeometryFactory();

    private static IO<T> Analyzed<TInput, T>(Plane plane, Func<Action<IDisposable>, IO<TInput>> read, Func<TInput, Fin<T>> analysis) =>
        from _ in IO.lift(() => plane.IsValid ? Fin.Succ(unit) : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{plane} is no valid plane")))
        from answer in DisposalOps.Within((Action<IDisposable> scratch) => read(scratch).Bind(input => IO.lift(() => analysis(input))), DisposalOps.Release)
        select answer;

    private static IO<GeometryCollection> Collection(Tolerances tolerances, Plane plane, PlanarGeometry geometry, Action<IDisposable> scratch) =>
        from points in IO.lift(() => Callbacks.Each(geometry.Points, (point, index) => Local(tolerances, plane, point, Subject("point", index)).Map(static at => Factory.CreatePoint(at))))
        from curves in Callbacks.Each(geometry.Curves.Map((curve, index) => Vertices(tolerances, plane, curve, Subject("curve", index), scratch).Map(static vertices => Factory.CreateLineString([.. vertices]))))
        from regions in Callbacks.Each(geometry.Regions.Map((region, index) => Shape(tolerances, plane, region, index, scratch)))
        select Factory.CreateGeometryCollection([.. points, .. curves, .. regions]);

    private static IO<MultiPolygon> Polygonal(Tolerances tolerances, Plane plane, Seq<PlanarRegion> regions, Action<IDisposable> scratch) =>
        Callbacks.Each(regions.Map((region, index) => Shape(tolerances, plane, region, index, scratch))).Map(static shapes => Factory.CreateMultiPolygon([.. shapes]));

    private static IO<Polygon> Shape(Tolerances tolerances, Plane plane, PlanarRegion region, int index, Action<IDisposable> scratch) =>
        from shell in Vertices(tolerances, plane, region.Boundary, Subject("region", index), scratch)
        from holes in Callbacks.Each(region.Holes.Map((hole, at) => Vertices(tolerances, plane, hole, string.Create(CultureInfo.InvariantCulture, $"region {index} hole {at}"), scratch)))
        select Factory.CreatePolygon(Factory.CreateLinearRing([.. shell]), [.. holes.Map(static hole => Factory.CreateLinearRing([.. hole]))]);

    private static IO<Seq<Coordinate>> Vertices(Tolerances tolerances, Plane plane, Curve curve, string subject, Action<IDisposable> scratch) =>
        from lines in Copies.Acquire(nameof(Curve.ToPolyline), _ => Answer.One(curve.ToPolyline(tolerances.Absolute, tolerances.Angle, 0, 0)), scratch)
        from vertices in IO.lift(() => Callbacks.Each(toSeq(lines.ToPolyline()), (vertex, at) => Local(tolerances, plane, vertex, string.Create(CultureInfo.InvariantCulture, $"{subject} vertex {at}"))))
        from ring in IO.lift(() => curve.IsClosed switch {
            true when vertices.Count < 4 => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{subject} closes in {vertices.Count} vertices at {tolerances.Absolute}")),
            true => Fin.Succ(vertices.Init.Add(vertices[0].Copy())),
            false => vertices,
        })
        select ring;

    private static Fin<Coordinate> Local(Tolerances tolerances, Plane plane, Point3d point, string subject) =>
        plane.RemapToPlaneSpace(point, out Point3d local) && Math.Abs(local.Z) <= tolerances.Absolute
            ? new Coordinate(local.X, local.Y)
            : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{subject} lies {local.Z} off {plane}"));

    private static string Subject(string kind, int index) => string.Create(CultureInfo.InvariantCulture, $"{kind} {index}");

    private static Point3d World(Plane plane, Coordinate at) => plane.PointAt(at.X, at.Y);

    private static Line Segment(Plane plane, LineString segment) => new(World(plane, segment.GetCoordinateN(0)), World(plane, segment.GetCoordinateN(1)));

    private static Polyline Ring(Plane plane, LineString ring) => [.. ring.Coordinates.Select(at => World(plane, at))];

    private static Fin<(Circle Circle, Point3d RadiusPoint)> Radius(Plane plane, LineString radius) =>
        Segment(plane, radius) switch {
            var line => (Circle: new Circle(new Plane(line.From, plane.XAxis, plane.YAxis), line.Length), RadiusPoint: line.To),
        };

    private static Fin<(Polyline Boundary, Seq<Polyline> Holes)> Region(Plane plane, (Geometry Hull, string Member) answer) =>
        answer.Hull switch {
            Polygon { IsEmpty: false } polygon => (Boundary: Ring(plane, polygon.Shell), Holes: toSeq(polygon.Holes).Map(hole => Ring(plane, hole)).Strict()),
            var hull => new Refused(answer.Member, hull.IsEmpty ? Refused.Absent : $"returned {hull.GeometryType}"),
        };

    private static Fin<PlanarEnclosure> Enclosure(Plane plane, Geometry enclosure, string member) =>
        enclosure switch {
            Polygon { IsEmpty: false } rectangle => toSeq(rectangle.Shell.Coordinates).Map(at => World(plane, at)) switch {
                var corners => new Plane(corners[0], corners[1] - corners[0], Vector3d.CrossProduct(plane.ZAxis, corners[1] - corners[0])) switch {
                    var frame => (frame.YAxis * (corners[3] - corners[0])) switch {
                        var height => new PlanarEnclosure.Rectangle(new Rectangle3d(frame, new Interval(0, corners[0].DistanceTo(corners[1])), new Interval(Math.Min(0, height), Math.Max(0, height)))),
                    },
                },
            },
            LineString { NumPoints: 2 } segment => new PlanarEnclosure.Segment(Segment(plane, segment)),
            NetTopologySuite.Geometries.Point { IsEmpty: false } point => new PlanarEnclosure.Point(World(plane, point.Coordinate)),
            _ => new Refused(member, enclosure.IsEmpty ? Refused.Absent : $"returned {enclosure.GeometryType}"),
        };
}
