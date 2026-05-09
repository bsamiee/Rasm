using Rhino.Geometry;
using Thinktecture;
namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

[Union]
public partial record RhinoGeometry {
    public sealed record Native(GeometryBase Geometry) : RhinoGeometry;
    public sealed record Box(Rhino.Geometry.Box Value) : RhinoGeometry;
    public sealed record BoundingBox(Rhino.Geometry.BoundingBox Value) : RhinoGeometry;
    public sealed record Line(Rhino.Geometry.Line Value) : RhinoGeometry;
    public sealed record Polyline(Rhino.Geometry.Polyline Value) : RhinoGeometry;
    public sealed record Plane(Rhino.Geometry.Plane Value) : RhinoGeometry;
    public sealed record Sphere(Rhino.Geometry.Sphere Value) : RhinoGeometry;
    public sealed record Cylinder(Rhino.Geometry.Cylinder Value) : RhinoGeometry;
    public sealed record Cone(Rhino.Geometry.Cone Value) : RhinoGeometry;
    public sealed record Torus(Rhino.Geometry.Torus Value) : RhinoGeometry;
    public sealed record Circle(Rhino.Geometry.Circle Value) : RhinoGeometry;
    public sealed record Arc(Rhino.Geometry.Arc Value) : RhinoGeometry;
    public Type ClrType =>
        this switch {
            Native native => native.Geometry.GetType(),
            Box => typeof(Rhino.Geometry.Box),
            BoundingBox => typeof(Rhino.Geometry.BoundingBox),
            Line => typeof(Rhino.Geometry.Line),
            Polyline => typeof(Rhino.Geometry.Polyline),
            Plane => typeof(Rhino.Geometry.Plane),
            Sphere => typeof(Rhino.Geometry.Sphere),
            Cylinder => typeof(Rhino.Geometry.Cylinder),
            Cone => typeof(Rhino.Geometry.Cone),
            Torus => typeof(Rhino.Geometry.Torus),
            Circle => typeof(Rhino.Geometry.Circle),
            Arc => typeof(Rhino.Geometry.Arc),
            _ => typeof(GeometryBase),
        };
    internal Fin<RhinoGeometry> Validate(OperationKey key) =>
        this switch {
            Native native when native.Geometry.IsValid => Fin.Succ(this),
            Box box when box.Value.IsValid => Fin.Succ(this),
            BoundingBox bbox when bbox.Value.IsValid => Fin.Succ(this),
            Line line when line.Value.IsValid => Fin.Succ(this),
            Polyline polyline when polyline.Value.IsValid => Fin.Succ(this),
            Plane plane when plane.Value.IsValid => Fin.Succ(this),
            Sphere sphere when sphere.Value.IsValid => Fin.Succ(this),
            Cylinder cylinder when cylinder.Value.IsValid => Fin.Succ(this),
            Cone cone when cone.Value.IsValid => Fin.Succ(this),
            Torus torus when torus.Value.IsValid => Fin.Succ(this),
            Circle circle when circle.Value.IsValid => Fin.Succ(this),
            Arc arc when arc.Value.IsValid => Fin.Succ(this),
            _ => Fin.Fail<RhinoGeometry>(error: key.InvalidInput()),
        };
    public static Seq<Type> Items { get; } = Seq(
        typeof(GeometryBase),
        typeof(Rhino.Geometry.Box),
        typeof(Rhino.Geometry.BoundingBox),
        typeof(Rhino.Geometry.Line),
        typeof(Rhino.Geometry.Polyline),
        typeof(Rhino.Geometry.Plane),
        typeof(Rhino.Geometry.Sphere),
        typeof(Rhino.Geometry.Cylinder),
        typeof(Rhino.Geometry.Cone),
        typeof(Rhino.Geometry.Torus),
        typeof(Rhino.Geometry.Circle),
        typeof(Rhino.Geometry.Arc));
}
