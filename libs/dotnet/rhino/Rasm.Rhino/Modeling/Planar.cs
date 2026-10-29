using System.Runtime.CompilerServices;
using NetTopologySuite;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Construct;
using NetTopologySuite.Algorithm.Hull;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlanarRegion {
    private PlanarRegion(Curve boundary, Seq<Curve> holes) => (Boundary, Holes) = (boundary, holes);

    public Curve Boundary { get; }

    public Seq<Curve> Holes { get; }

    public static Fin<PlanarRegion> Validated(Curve boundary, Seq<Curve> holes) =>
        (RhinoFault.Unless(boundary.IsClosed, RhinoReason.Invalid, boundary.IsClosed), Products.Each(holes, static (hole, index) => RhinoFault.Unless(hole.IsClosed, RhinoReason.Invalid, hole.IsClosed, index)))
            .Apply((_, _) => new PlanarRegion(boundary, holes)).As();
}

public sealed record PlanarGeometry {
    private PlanarGeometry(Seq<Point3d> points, Seq<Curve> curves, Seq<PlanarRegion> regions) => (Points, Curves, Regions) = (points, curves, regions);

    public Seq<Point3d> Points { get; }

    public Seq<Curve> Curves { get; }

    public Seq<PlanarRegion> Regions { get; }

    public static Fin<PlanarGeometry> Validated(Seq<Point3d> points, Seq<Curve> curves, Seq<PlanarRegion> regions) =>
        (points.Count + curves.Count + regions.Count) switch {
            var count => RhinoFault.Unless(count > 0, RhinoReason.Missing, count).Map(_ => new PlanarGeometry(points, curves, regions)),
        };
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ConcaveCriterion {
    private ConcaveCriterion(bool valid) => Valid = valid;

    public bool Valid { get; }

    public sealed record MaximumEdgeLength(double Length) : ConcaveCriterion(double.IsFinite(Length) && Length >= 0d);

    public sealed record MaximumEdgeLengthRatio(double Ratio) : ConcaveCriterion(Ratio is >= 0d and <= 1d);

    public sealed record Alpha(double Radius) : ConcaveCriterion(double.IsFinite(Radius) && Radius >= 0d);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PlanarEnclosure {
    public sealed record Rectangle(Rectangle3d Bounds) : PlanarEnclosure;

    public sealed record Segment(Line Line) : PlanarEnclosure;

    public sealed record Single(Point3d Location) : PlanarEnclosure;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Planar {
    // --- [CIRCLES]
    public static IO<(Circle Circle, Point3d RadiusPoint)> InscribedCircle(Tolerances tolerances, Plane plane, IterableNE<PlanarRegion> regions) =>
        Analyzed(plane, () => Polygonal(tolerances, plane, toSeq(regions)), polygonal => Radius(plane, MaximumInscribedCircle.GetRadiusLine(polygonal, tolerances.Absolute)));

    public static IO<(Circle Circle, Point3d RadiusPoint)> EmptyCircle(Tolerances tolerances, Plane plane, PlanarGeometry obstacles, Seq<PlanarRegion> boundary) =>
        Analyzed(
            plane,
            () =>
                from held in Collection(tolerances, plane, obstacles)
                from within in Polygonal(tolerances, plane, boundary)
                select (Obstacles: held, Boundary: within),
            input => Radius(plane, LargestEmptyCircle.GetRadiusLine(input.Obstacles, input.Boundary, tolerances.Absolute)));

    // --- [ENCLOSURES]
    public static IO<PlanarEnclosure> EnclosingRectangle(Tolerances tolerances, Plane plane, PlanarGeometry input) =>
        Analyzed(plane, () => Collection(tolerances, plane, input), geometry => Enclosure(plane, MinimumAreaRectangle.GetMinimumRectangle(geometry)));

    public static IO<(Line Diameter, Line SupportingSegment, PlanarEnclosure Rectangle)> Width(Tolerances tolerances, Plane plane, PlanarGeometry input) =>
        Analyzed(plane, () => Collection(tolerances, plane, input), geometry => new MinimumDiameter(geometry) switch {
            var diameter => Enclosure(plane, diameter.GetMinimumRectangle())
                .Map(rectangle => (Diameter: Segment(plane, diameter.Diameter), SupportingSegment: Segment(plane, diameter.SupportingSegment), Rectangle: rectangle)),
        });

    // --- [HULLS]
    public static IO<(Polyline Boundary, Seq<Polyline> Holes)> ConcaveBoundary(Tolerances tolerances, Plane plane, Seq<Point3d> points, ConcaveCriterion criterion, bool holesAllowed) =>
        Analyzed(
            plane,
            () => IO.lift(() => RhinoFault.Unless(criterion.Valid, RhinoReason.Outside, criterion).Bind(_ => Products.Each(points, (point, index) => Local(tolerances, plane, point, index)))),
            coordinates => criterion.Switch(
                (Plane: plane, Points: Factory.CreateMultiPointFromCoords([.. coordinates]), HolesAllowed: holesAllowed),
                maximumEdgeLength: static (at, of) => Region(at.Plane, ConcaveHull.ConcaveHullByLength(at.Points, of.Length, at.HolesAllowed)),
                maximumEdgeLengthRatio: static (at, of) => Region(at.Plane, ConcaveHull.ConcaveHullByLengthRatio(at.Points, of.Ratio, at.HolesAllowed)),
                alpha: static (at, of) => Region(at.Plane, ConcaveHull.AlphaShape(at.Points, of.Radius, at.HolesAllowed))));

    // --- [DIAGRAMS]
    public static IO<Seq<(Point3d Site, Polyline Cell)>> Voronoi(Tolerances tolerances, Rectangle3d frame, Seq<Point3d> sites) =>
        from envelope in IO.lift(() => RhinoFault.Unless(frame.IsValid && frame.Width > 0 && frame.Height > 0, RhinoReason.Invalid, frame).Map(_ => new Envelope(frame.X.Min, frame.X.Max, frame.Y.Min, frame.Y.Max)))
        from coordinates in IO.lift(() => Products.Each(sites, (site, index) =>
            from at in Local(tolerances, frame.Plane, site, index)
            from covered in RhinoFault.Unless(envelope.Covers(at), RhinoReason.Outside, site, index)
            select at))
        from diagram in IO.lift(() => {
            VoronoiDiagramBuilder builder = new() { ClipEnvelope = envelope, Tolerance = tolerances.Absolute };
            builder.SetSites([.. coordinates]);
            return builder.GetDiagram(Factory);
        })
        from cells in IO.lift(() => Products.Each(toSeq(diagram.Geometries), (cell, index) => cell switch {
            Polygon { UserData: Coordinate site } polygon => Fin.Succ((Site: World(frame.Plane, site), Cell: Ring(frame.Plane, polygon.Shell))),
            _ => new RhinoFault(cell.GeometryType, RhinoReason.Mismatched, index),
        }))
        select cells;

    // --- [BOUNDARY]
    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance.CreateGeometryFactory();

    private static IO<T> Analyzed<TInput, T>(Plane plane, Func<IO<TInput>> read, Func<TInput, Fin<T>> analysis) =>
        from valid in IO.lift(() => RhinoFault.Unless(plane.IsValid, RhinoReason.Invalid, plane))
        from answer in read().Bind(input => IO.lift(() => analysis(input))).Bracket()
        select answer;

    private static IO<GeometryCollection> Collection(Tolerances tolerances, Plane plane, PlanarGeometry geometry) =>
        from points in IO.lift(() => Products.Each(geometry.Points, (point, index) => Local(tolerances, plane, point, index).Map(static at => (Geometry)Factory.CreatePoint(at))))
        from curves in Products.Each(geometry.Curves.Map((curve, index) => Vertices(tolerances, plane, curve, index).Map(static vertices => (Geometry)Factory.CreateLineString([.. vertices]))))
        from regions in Products.Each(geometry.Regions.Map((region, index) => Shape(tolerances, plane, region, index).Map(static shape => (Geometry)shape)))
        select Factory.CreateGeometryCollection([.. points, .. curves, .. regions]);

    private static IO<MultiPolygon> Polygonal(Tolerances tolerances, Plane plane, Seq<PlanarRegion> regions) =>
        Products.Each(regions.Map((region, index) => Shape(tolerances, plane, region, index))).Map(static shapes => Factory.CreateMultiPolygon([.. shapes]));

    private static IO<Polygon> Shape(Tolerances tolerances, Plane plane, PlanarRegion region, int index) =>
        from shell in Vertices(tolerances, plane, region.Boundary, index)
        from holes in Products.Each(region.Holes.Map(hole => Vertices(tolerances, plane, hole, index)))
        select Factory.CreatePolygon(Factory.CreateLinearRing([.. shell]), [.. holes.Map(static hole => Factory.CreateLinearRing([.. hole]))]);

    private static IO<Seq<Coordinate>> Vertices(Tolerances tolerances, Plane plane, Curve curve, int index) =>
        from lines in Products.Of(() => curve.ToPolyline(tolerances.Absolute, tolerances.Angle, 0, 0))
        from vertices in IO.lift(() => Products.Each(toSeq(lines.ToPolyline()), (vertex, _) => Local(tolerances, plane, vertex, index)))
        from ring in IO.lift(() => curve.IsClosed switch {
            true => RhinoFault.Unless(vertices.Count >= 4, RhinoReason.Outside, vertices.Count, index).Map(_ => vertices.Init.Add(vertices[0].Copy())),
            false => vertices,
        })
        select ring;

    private static Fin<Coordinate> Local(Tolerances tolerances, Plane plane, Point3d point, int index) =>
        from mapped in RhinoFault.Unless(plane.RemapToPlaneSpace(point, out Point3d local), RhinoReason.Declined, point, index).Map(_ => local)
        from flat in RhinoFault.Unless(Math.Abs(mapped.Z) <= tolerances.Absolute, RhinoReason.Outside, mapped.Z, index)
        select new Coordinate(mapped.X, mapped.Y);

    private static Point3d World(Plane plane, Coordinate at) => plane.PointAt(at.X, at.Y);

    private static Line Segment(Plane plane, LineString segment) => new(World(plane, segment.GetCoordinateN(0)), World(plane, segment.GetCoordinateN(1)));

    private static Polyline Ring(Plane plane, LineString ring) => [.. ring.Coordinates.Select(at => World(plane, at))];

    private static Fin<(Circle Circle, Point3d RadiusPoint)> Radius(Plane plane, LineString radius) =>
        Segment(plane, radius) switch {
            var line => (Circle: new Circle(new Plane(line.From, plane.XAxis, plane.YAxis), line.Length), RadiusPoint: line.To),
        };

    private static Fin<(Polyline Boundary, Seq<Polyline> Holes)> Region(Plane plane, Geometry hull, [CallerArgumentExpression(nameof(hull))] string call = "") =>
        hull switch {
            Polygon { IsEmpty: false } polygon => (Boundary: Ring(plane, polygon.Shell), Holes: toSeq(polygon.Holes).Map(hole => Ring(plane, hole)).Strict()),
            _ => new RhinoFault(call, hull.IsEmpty ? RhinoReason.Missing : RhinoReason.Mismatched),
        };

    private static Fin<PlanarEnclosure> Enclosure(Plane plane, Geometry enclosure, [CallerArgumentExpression(nameof(enclosure))] string call = "") =>
        enclosure switch {
            Polygon { IsEmpty: false } rectangle => toSeq(rectangle.Shell.Coordinates).Map(at => World(plane, at)) switch {
                var corners => new Plane(corners[0], corners[1] - corners[0], Vector3d.CrossProduct(plane.ZAxis, corners[1] - corners[0])) switch {
                    var frame => (frame.YAxis * (corners[3] - corners[0])) switch {
                        var height => new PlanarEnclosure.Rectangle(new Rectangle3d(frame, new Interval(0, corners[0].DistanceTo(corners[1])), new Interval(Math.Min(0, height), Math.Max(0, height)))),
                    },
                },
            },
            LineString { NumPoints: 2 } segment => new PlanarEnclosure.Segment(Segment(plane, segment)),
            NetTopologySuite.Geometries.Point { IsEmpty: false } point => new PlanarEnclosure.Single(World(plane, point.Coordinate)),
            _ => new RhinoFault(call, enclosure.IsEmpty ? RhinoReason.Missing : RhinoReason.Mismatched),
        };
}
