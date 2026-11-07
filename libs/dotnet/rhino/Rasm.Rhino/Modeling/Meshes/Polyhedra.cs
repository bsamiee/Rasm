using MathNet.Numerics;
using Rasm.Rhino.Document.Units;
using SuperLinq;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class PointGroup {
    private static readonly Seq<Seq<int>> Orders = toSeq(Enumerable.Range(0, 4).Permutations()).Map(static order => toSeq(order)).Strict();

    public static readonly PointGroup T = new(Rotations([[1d, 0d, 0d, 0d], [0.5d, 0.5d, 0.5d, 0.5d]], Orders));
    public static readonly PointGroup Td = new(Reflected(T.Elements, new(1d, -1d, 0d)));
    public static readonly PointGroup Th = new(Reflected(T.Elements, Vector3d.XAxis));
    public static readonly PointGroup O = new(T.Elements + Rotations([[Constants.Sqrt1Over2, Constants.Sqrt1Over2, 0d, 0d]], Orders));
    public static readonly PointGroup Oh = new(Reflected(O.Elements, Vector3d.XAxis));
    public static readonly PointGroup I = new(T.Elements + Rotations(
        [[Constants.GoldenRatio / 2d, 0d, 1d / (2d * Constants.GoldenRatio), 0.5d]],
        Orders.Filter(static order => new Permutation([.. order]).ToInversions().Where(static (to, from) => to != from).Count() % 2 is 0)));
    public static readonly PointGroup Ih = new(Reflected(I.Elements, Vector3d.XAxis));

    public Seq<Transform> Elements { get; }

    private static Seq<Transform> Rotations(Seq<Seq<double>> seeds, Seq<Seq<int>> orders) =>
        (from seed in seeds
         from signs in seed.Traverse(static value => Seq(-value, value)).As()
         from order in orders
         let components = order.Map(index => signs[index]).Strict()
         where components.Find(static component => component is not 0d).Exists(static component => component > 0d)
         select new Quaternion(components[0], components[1], components[2], components[3]))
        .Distinct()
        .Map(static root => {
            _ = root.GetRotation(out Transform rotation);
            return rotation;
        }).Strict();

    private static Seq<Transform> Reflected(Seq<Transform> rotations, Vector3d normal) =>
        Transform.Mirror(Point3d.Origin, normal) switch {
            var reflection => rotations + rotations.Map(rotation => reflection * rotation).Strict(),
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
