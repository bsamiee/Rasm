using Rasm.Geometry;
using Rasm.Rhino.Document.Units;
using Rhino;

namespace Rasm.Rhino.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Fields {

    // --- [LEAVES]

    public static Fin<ScalarField.Moved> Solid(Box box) =>
        from valid in RhinoFault.Unless(box.IsValid, RhinoReason.Invalid, box)
        from extent in Bounds.Validated(new(box.X.Min, box.Y.Min, box.Z.Min), new(box.X.Max, box.Y.Max, box.Z.Max))
        from map in Transform.PlaneToPlane(Plane.WorldXY, box.Plane).ToAffine()
        from moved in ScalarField.Moved.Of(new ScalarField.Box(extent), map, RhinoMath.ZeroTolerance)
        select moved;

    public static Fin<ScalarField> Distance(GeometryBase source) =>
        source.GetBoundingBox(accurate: true).ToBounds().Map(ScalarField (bounds) => new ScalarField.Measured(point => Unsigned(source, point.ToPoint3d()), bounds));

    public static Fin<ScalarField> Signed(Tolerances tolerances, Interior region) =>
        region.Switch(
            solid: static solid => (Boundary: solid.Brep, Plane: Option<Plane>.None),
            closed: static closed => (Boundary: closed.Mesh, Plane: Option<Plane>.None),
            planar: static planar => (Boundary: (GeometryBase)planar.Curve, Plane: Some(planar.Plane))) switch {
                var (boundary, plane) => (region is Interior.Planar ? None : Some(boundary)).Traverse(static held => held.GetBoundingBox(accurate: true).ToBounds()).As().Map(ScalarField (bounds) => new ScalarField.Measured(
                    point => point.ToPoint3d() switch {
                        var at => (region.Contains(tolerances, at), Unsigned(boundary, plane.Map(held => held.ClosestPoint(at)).IfNone(at))) switch {
                            (PointContainment.Inside, var distance) => -distance,
                            (PointContainment.Coincident, _) => 0d,
                            (PointContainment.Outside, var distance) => distance,
                            (PointContainment.Unset, _) => double.NaN,
                        },
                    },
                    bounds)),
            };

    private static double Unsigned(GeometryBase source, Point3d point) =>
        Proximity.Nearest(source, point, None).Match(Succ: found => found.Map(at => point.DistanceTo(at.Point)).IfNone(double.NaN), Fail: static _ => double.NaN);

    // --- [MESHING]

    public static IO<Mesh> Isosurface(ScalarField field, int resolution, int rootFindingSteps, Option<BoundingBox> box = default) =>
        from grid in IO.lift(() =>
            from found in Conversions.Required(box.Map(given => (Box: given, Resolution: resolution))
                | field.Bounds.Bind(bounds => bounds.Inflated(bounds.ToBoundingBox().Diagonal.MinimumCoordinate / resolution).ToOption()).Map(inflated => (Box: inflated.ToBoundingBox(), Resolution: resolution + 2)))
            from valid in RhinoFault.Unless(found.Box.IsValid && found.Box.IsDegenerate(0d) is 0, RhinoReason.Invalid, found.Box)
            select found)
        from failed in IO.lift(static () => Atom(value: false))
        from mesh in Products.Of(() => Mesh.CreateFromIsosurface(point => {
            double distance = field.Distance(point.ToVector3D());
            if (!double.IsFinite(distance)) _ = failed.Swap(static _ => true);
            return distance;
        }, grid.Box, grid.Resolution, rootFindingSteps))
        from finite in IO.lift(RhinoFault.Unless(!failed.Value, RhinoReason.Invalid, failed.Value))
        select mesh;
}
