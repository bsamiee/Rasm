using MathNet.Numerics;
using Rasm.Rhino.Document.Units;
using Rhino;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PointGroup {
    public static readonly PointGroup T = new(Closed([Turn(new(1d, 1d, 1d), 3), Turn(Vector3d.ZAxis, 2)]));
    public static readonly PointGroup Td = new(Closed([Reflection(new(1d, -1d, 0d)), Turn(new(1d, 1d, 1d), 3), Turn(Vector3d.ZAxis, 2)]));
    public static readonly PointGroup Th = new(Closed([Reflection(Vector3d.XAxis), Turn(new(1d, 1d, 1d), 3)]));
    public static readonly PointGroup O = new(Closed([Turn(Vector3d.ZAxis, 4), Turn(new(1d, 1d, 1d), 3)]));
    public static readonly PointGroup Oh = new(Closed([Reflection(Vector3d.XAxis), Turn(new(1d, 1d, 1d), 3), Reflection(new(1d, -1d, 0d))]));
    public static readonly PointGroup I = new(Closed([Turn(Vector3d.XAxis, 2), Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 1d, Constants.GoldenRatio), 5)]));
    public static readonly PointGroup Ih = new(Closed([Reflection(Vector3d.XAxis), Turn(new(1d, 1d, 1d), 3), Turn(new(0d, 1d, Constants.GoldenRatio), 5)]));

    public Seq<Transform> Elements { get; }

    private static Transform Turn(Vector3d axis, int order) => Transform.Rotation(double.Tau / order, axis, Point3d.Origin);

    private static Transform Reflection(Vector3d normal) => Transform.Mirror(Point3d.Origin, normal);

    private static Seq<Transform> Closed(Seq<Transform> generators) => Grown(generators, [Transform.Identity], [Transform.Identity]);

    private static Seq<Transform> Grown(Seq<Transform> generators, Seq<Transform> elements, Seq<Transform> frontier) =>
        (from element in frontier from generator in generators select generator * element).Fold(elements, static (closed, product) =>
            closed.Exists(known => toSeq(known.ToDoubleArray(rowDominant: true)).Zip(toSeq(product.ToDoubleArray(rowDominant: true))).ForAll(static pair => Math.Abs(pair.First - pair.Second) <= RhinoMath.SqrtEpsilon))
                ? closed
                : closed.Add(product)) switch {
                    var closed when closed.Count == elements.Count => elements,
                    var closed => Grown(generators, closed, closed.Skip(elements.Count)),
                };
}

public sealed record Hull {
    private Hull(Seq<Point3d> points, Seq<Seq<int>> facets) => (Points, Facets) = (points, facets);

    public Seq<Point3d> Points { get; }

    public Seq<Seq<int>> Facets { get; }

    public static Fin<Hull> Validated(Seq<Point3d> points, Seq<Seq<int>> facets) =>
        Products.Each(facets, (facet, index) => RhinoFault.Unless(facet.Count >= 3 && facet.ForAll(at => at >= 0 && at < points.Count), RhinoReason.Outside, facet.Count, index))
            .Map(_ => new Hull(points, facets));

    public Fin<Seq<Point3d>> Poles(Point3d centre, double tolerance) =>
        (Points.Fold(Point3d.Origin, static (sum, point) => sum + point) / Points.Count) switch {
            var inside => Products.Each(Facets, (facet, index) => facet.Map(at => Points[at]).Strict() switch {
                var corners =>
                    from fitted in RhinoFault.Unless(Plane.FitPlaneToPoints(corners, out Plane plane) is PlaneFitResult.Success, RhinoReason.Invalid, corners.Count, index).Map(_ => plane)
                    let middle = corners.Fold(Point3d.Origin, static (sum, point) => sum + point) / corners.Count
                    let outward = fitted.ZAxis * double.Sign(fitted.ZAxis * (middle - inside))
                    let distance = outward * (middle - centre)
                    from clear in RhinoFault.Unless(distance > tolerance, RhinoReason.Outside, distance, index)
                    select centre + (outward / distance),
            }),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Polyhedra {
    public static IO<(Mesh Mesh, Hull Hull)> Orbit(Tolerances tolerances, PointGroup symmetry, Seq<Point3d> seeds) =>
        from points in IO.lift(() => Conversions.Rows(Point3d.CullDuplicates(from map in symmetry.Elements from seed in seeds select map * seed, tolerances.Absolute)))
        from made in MeshConstruction.Hull(tolerances, points, HullMethod.Quick)
        from hull in IO.lift(Hull.Validated(points, made.Facets))
        select (made.Hull, hull);

    public static IO<(Mesh Mesh, Hull Hull)> Polar(Tolerances tolerances, Hull hull, Point3d centre) =>
        from poles in IO.lift(() => hull.Poles(centre, tolerances.Absolute))
        from made in MeshConstruction.Hull(tolerances, poles, HullMethod.Quick)
        from polar in IO.lift(Hull.Validated(poles, made.Facets))
        select (made.Hull, polar);
}
