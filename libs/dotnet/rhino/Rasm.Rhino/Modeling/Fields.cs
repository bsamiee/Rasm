using Rasm.RhinoCommon.Document.Units;
using Rasm.Spatial;

namespace Rasm.RhinoCommon.Modeling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fields {
    // --- [LEAVES]
    public static Fin<ScalarField.Moved> Solid(Box box) =>
        RhinoFault.Unless(box.IsValid).Bind(_ =>
            ScalarField.Moved.Of(new ScalarField.Box(new(box.X.Min, box.Y.Min, box.Z.Min), new(box.X.Max, box.Y.Max, box.Z.Max)), Transform.PlaneToPlane(Plane.WorldXY, box.Plane).ToMatrix()));

    public static ScalarField Distance(Proximity source) =>
        new ScalarField.Measured(compose(static (Vector3D<double> point) => point.ToPoint3d(), Unsigned(source)), source.Switch(
            curve: static curve => (GeometryBase)curve.Geometry,
            surface: static surface => surface.Geometry,
            face: static face => face.Geometry,
            brep: static brep => brep.Geometry,
            mesh: static mesh => mesh.Geometry,
            subD: static subd => subd.Geometry,
            cloud: static cloud => cloud.Geometry).GetBoundingBox(accurate: true).ToBounds());

    public static ScalarField Signed(Tolerances tolerances, Interior region) =>
        Boundary(region) switch {
            var boundary => new ScalarField.Measured(
                point => point.ToPoint3d() switch { var at => region.Contains(tolerances, at) switch { PointContainment.Inside => -boundary(at), PointContainment.Coincident => 0d, PointContainment.Outside => boundary(at), PointContainment.Unset => double.NaN } },
                region.Switch(
                    solid: static solid => Some(solid.Brep.GetBoundingBox(accurate: true).ToBounds()),
                    closed: static closed => Some(closed.Mesh.GetBoundingBox(accurate: true).ToBounds()),
                    planar: static _ => Option<Bounds>.None)),
        };

    private static Func<Point3d, double> Unsigned(Proximity source) => point => source.Nearest([point], None).Head.Bind(static found => found).Map(point.DistanceTo).IfNone(double.NaN);

    private static Func<Point3d, double> Boundary(Interior region) => region.Switch(
        solid: static solid => Unsigned(new Proximity.Brep(solid.Brep)),
        closed: static closed => Unsigned(new Proximity.Mesh(closed.Mesh)),
        planar: static planar => (Unsigned(new Proximity.Curve(planar.Curve)), planar.Plane) switch { var (inPlane, plane) => point => inPlane(plane.ClosestPoint(point)) });

    // --- [MESHING]
    public static IO<Mesh> Isosurface(ScalarField field, int resolution, int rootFindingSteps, Option<BoundingBox> box = default) =>
        from grid in IO.lift(() => Conversions.Found(box.Map(given => (Box: given, Resolution: resolution))
                | field.Bounds.Map(bounds => (Box: bounds.Inflated(bounds.ToBoundingBox().Diagonal.MinimumCoordinate / resolution).ToBoundingBox(), Resolution: resolution + 2)))
            .Bind(static grid => RhinoFault.Unless(grid.Box.IsValid && grid.Box.IsDegenerate(0d) is 0).Map(_ => grid)))
        from mesh in Products.Made(Mesh.CreateFromIsosurface(point => field.Distance(point.ToVector3D()), grid.Box, grid.Resolution, rootFindingSteps))
        select mesh;
}
