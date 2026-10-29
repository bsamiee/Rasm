using Rasm.Rhino.Document.Units;
using Rasm.Spatial;

namespace Rasm.Rhino.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fields {
    // --- [LEAVES]
    public static Fin<ScalarField.Moved> Solid(Box box) =>
        RhinoFault.Unless(box.IsValid, RhinoReason.Invalid, box).Bind(_ =>
            ScalarField.Moved.Of(new ScalarField.Box(new(box.X.Min, box.Y.Min, box.Z.Min), new(box.X.Max, box.Y.Max, box.Z.Max)), Transform.PlaneToPlane(Plane.WorldXY, box.Plane).ToMatrix()));

    public static ScalarField Distance(GeometryBase source) =>
        new ScalarField.Measured(point => Unsigned(source, point.ToPoint3d()), source.GetBoundingBox(accurate: true).ToBounds());

    public static ScalarField Signed(Tolerances tolerances, Interior region) =>
        region.Switch(
            solid: static solid => (Boundary: solid.Brep, Plane: Option<Plane>.None),
            closed: static closed => (Boundary: closed.Mesh, Plane: Option<Plane>.None),
            planar: static planar => (Boundary: (GeometryBase)planar.Curve, Plane: Some(planar.Plane))) switch {
                var (boundary, plane) => new ScalarField.Measured(
                    point => point.ToPoint3d() switch {
                        var at => (region.Contains(tolerances, at), Unsigned(boundary, plane.Map(held => held.ClosestPoint(at)).IfNone(at))) switch {
                            (PointContainment.Inside, var distance) => -distance,
                            (PointContainment.Coincident, _) => 0d,
                            (PointContainment.Outside, var distance) => distance,
                            (PointContainment.Unset, _) => double.NaN,
                        },
                    },
                    region is Interior.Planar ? None : Some(boundary.GetBoundingBox(accurate: true).ToBounds())),
            };

    private static double Unsigned(GeometryBase source, Point3d point) =>
        Proximity.Nearest(source, point, None).Match(Succ: found => found.Map(at => point.DistanceTo(at.Point)).IfNone(double.NaN), Fail: static _ => double.NaN);

    // --- [MESHING]
    public static IO<Mesh> Isosurface(ScalarField field, int resolution, int rootFindingSteps, Option<BoundingBox> box = default) =>
        from grid in IO.lift(() =>
            from found in Conversions.Required(box.Map(given => (Box: given, Resolution: resolution))
                | field.Bounds.Map(bounds => (Box: bounds.Inflated(bounds.ToBoundingBox().Diagonal.MinimumCoordinate / resolution).ToBoundingBox(), Resolution: resolution + 2)))
            from valid in RhinoFault.Unless(found.Box.IsValid && found.Box.IsDegenerate(0d) is 0, RhinoReason.Invalid, found.Box)
            select found)
        from failed in IO.lift(static () => Atom(value: false))
        from mesh in Products.Of(() => Mesh.CreateFromIsosurface(point => {
            double distance = field.Distance(point.ToVector3D());
            if (!double.IsFinite(distance)) _ = failed.Swap(static _ => true);
            return distance;
        }, grid.Box, grid.Resolution, rootFindingSteps))
        from finite in IO.lift(() => RhinoFault.Unless(!failed.Value, RhinoReason.Invalid, failed.Value))
        select mesh;
}
