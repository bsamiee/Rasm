using Rasm.Spatial;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Meshes;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Polyhedra {
    public static IO<(Mesh Mesh, Hull Hull)> Orbit(Tolerances tolerances, PointGroup symmetry, Seq<Point3d> seeds, Action<IDisposable> retain) =>
        from points in IO.lift(() => Conversions.Rows(Point3d.CullDuplicates(from map in symmetry.Elements.Map(static element => element.ToTransform()) from seed in seeds select map * seed, tolerances.Absolute)))
        from made in MeshConstruction.Hull(tolerances, new HullSource.Quick(points), retain)
        from hull in IO.lift(Hull.Validated(points.Map(static point => point.ToVector3D()), made.Facets))
        select (made.Hull, hull);

    public static IO<(Mesh Mesh, Hull Hull)> Polar(Tolerances tolerances, Hull hull, Point3d centre, Action<IDisposable> retain) =>
        from poles in IO.lift(() => hull.Poles(centre.ToVector3D(), tolerances.Absolute))
        from made in MeshConstruction.Hull(tolerances, new HullSource.Quick(poles.Map(static pole => pole.ToPoint3d())), retain)
        from polar in IO.lift(Hull.Validated(poles, made.Facets))
        select (made.Hull, polar);
}
