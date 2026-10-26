using System.Globalization;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PointGroup {
    private static readonly Transform HalfTurnZ = Transform.Rotation(double.Pi, Vector3d.ZAxis, Point3d.Origin);
    private static readonly Transform ThirdTurn = Transform.Rotation(double.Tau / 3, new Vector3d(1, 1, 1), Point3d.Origin);
    private static readonly Transform FifthTurn = Transform.Rotation(double.Tau / 5, new Vector3d(0, 1, (1 + Math.Sqrt(5)) / 2), Point3d.Origin);
    private static readonly Transform MirrorX = Transform.Mirror(Point3d.Origin, Vector3d.XAxis);
    private static readonly Transform MirrorDiagonal = Transform.Mirror(Point3d.Origin, new Vector3d(1, -1, 0));

    public static readonly PointGroup T = new([ThirdTurn, HalfTurnZ]);
    public static readonly PointGroup Td = new([MirrorDiagonal, ThirdTurn, HalfTurnZ]);
    public static readonly PointGroup Th = new([MirrorX, ThirdTurn]);
    public static readonly PointGroup O = new([Transform.Rotation(double.Pi / 2, Vector3d.ZAxis, Point3d.Origin), ThirdTurn]);
    public static readonly PointGroup Oh = new([MirrorX, ThirdTurn, MirrorDiagonal]);
    public static readonly PointGroup I = new([Transform.Rotation(double.Pi, Vector3d.XAxis, Point3d.Origin), ThirdTurn, FifthTurn]);
    public static readonly PointGroup Ih = new([MirrorX, ThirdTurn, FifthTurn]);

    public Seq<Transform> Generators { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Polyhedra {
    // --- [CONSTRUCTION]
    public static IO<(Mesh Hull, Seq<Point3d> Points, Seq<Seq<int>> Facets)> Orbit(Tolerances tolerances, PointGroup symmetry, Seq<Point3d> seeds, Action<IDisposable> retain) =>
        from points in IO.lift(() => Closure(tolerances, symmetry, Conversions.Rows(Point3d.CullDuplicates(seeds, tolerances.Absolute))))
        from hull in MeshConstruction.Hull(tolerances, new HullSource.Quick(points), retain)
        select (hull.Hull, Points: points, hull.Facets);

    public static IO<(Mesh Hull, Seq<Point3d> Points, Seq<Seq<int>> Facets)> Polar(Tolerances tolerances, (Seq<Point3d> Points, Seq<Seq<int>> Facets) hull, Point3d centre, Action<IDisposable> retain) =>
        from poles in IO.lift(() => (hull.Points.Fold(Point3d.Origin, static (sum, point) => sum + point) / hull.Points.Count) switch {
            var inside => Callbacks.Each(hull.Facets, (facet, index) => Pole(tolerances, centre, inside, facet.Map(at => hull.Points[at]), index)),
        })
        from polar in MeshConstruction.Hull(tolerances, new HullSource.Quick(poles), retain)
        select (polar.Hull, Points: poles, polar.Facets);

    // --- [ORBITS]
    private static Seq<Point3d> Closure(Tolerances tolerances, PointGroup symmetry, Seq<Point3d> points) =>
        Conversions.Rows(Point3d.CullDuplicates(points + points.Bind(point => symmetry.Generators.Map(generator => generator * point)), tolerances.Absolute)) switch {
            var grown when grown.Count == points.Count => points,
            var grown => Closure(tolerances, symmetry, grown),
        };

    // --- [POLES]
    private static Fin<Point3d> Pole(Tolerances tolerances, Point3d centre, Point3d inside, Seq<Point3d> facet, int index) =>
        Plane.FitPlaneToPoints(facet, out Plane plane) is PlaneFitResult.Success
            ? (plane.Normal * Math.Sign(plane.Normal * (plane.Origin - inside))) switch {
                var outward => (outward * (plane.Origin - centre)) switch {
                    var distance when distance > tolerances.Absolute => centre + (outward / distance),
                    _ => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{nameof(Polar)} centre {centre} lies on or outside the plane of facet {index}")),
                },
            }
            : new Refused(nameof(Plane.FitPlaneToPoints), Refused.Declined, index);
}
