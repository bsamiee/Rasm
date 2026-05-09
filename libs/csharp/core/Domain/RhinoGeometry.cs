using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
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
    public object Inner =>
        this switch {
            Native native => native.Geometry,
            Box box => box.Value,
            BoundingBox bbox => bbox.Value,
            Line line => line.Value,
            Polyline polyline => polyline.Value,
            Plane plane => plane.Value,
            Sphere sphere => sphere.Value,
            Cylinder cylinder => cylinder.Value,
            Cone cone => cone.Value,
            Torus torus => torus.Value,
            Circle circle => circle.Value,
            Arc arc => arc.Value,
            _ => this,
        };
    public static Option<RhinoGeometry> From(object value) =>
        value switch {
            GeometryBase geometry => Some<RhinoGeometry>(new Native(Geometry: geometry)),
            Rhino.Geometry.Box box => Some<RhinoGeometry>(new Box(Value: box)),
            Rhino.Geometry.BoundingBox bbox => Some<RhinoGeometry>(new BoundingBox(Value: bbox)),
            Rhino.Geometry.Line line => Some<RhinoGeometry>(new Line(Value: line)),
            Rhino.Geometry.Polyline polyline => Some<RhinoGeometry>(new Polyline(Value: polyline)),
            Rhino.Geometry.Plane plane => Some<RhinoGeometry>(new Plane(Value: plane)),
            Rhino.Geometry.Sphere sphere => Some<RhinoGeometry>(new Sphere(Value: sphere)),
            Rhino.Geometry.Cylinder cylinder => Some<RhinoGeometry>(new Cylinder(Value: cylinder)),
            Rhino.Geometry.Cone cone => Some<RhinoGeometry>(new Cone(Value: cone)),
            Rhino.Geometry.Torus torus => Some<RhinoGeometry>(new Torus(Value: torus)),
            Rhino.Geometry.Circle circle => Some<RhinoGeometry>(new Circle(Value: circle)),
            Rhino.Geometry.Arc arc => Some<RhinoGeometry>(new Arc(Value: arc)),
            _ => None,
        };
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

internal static class RhinoValidity {
    internal static Fin<TValue> RequireValid<TValue>(this OperationKey key, TValue value) =>
        value switch {
            Point2d point => key.Require(condition: point.IsValid, value: value),
            Point3d point => key.Require(condition: point.IsValid, value: value),
            Vector3d vector => key.Require(condition: vector.IsValid, value: value),
            Rhino.Geometry.Plane plane => key.Require(condition: plane.IsValid, value: value),
            Rhino.Geometry.BoundingBox box => key.Require(condition: box.IsValid, value: value),
            Rhino.Geometry.Box box => key.Require(condition: box.IsValid, value: value),
            Rhino.Geometry.Sphere sphere => key.Require(condition: sphere.IsValid, value: value),
            Rhino.Geometry.Cylinder cylinder => key.Require(condition: cylinder.IsValid, value: value),
            Rhino.Geometry.Cone cone => key.Require(condition: cone.IsValid, value: value),
            Rhino.Geometry.Torus torus => key.Require(condition: torus.IsValid, value: value),
            Rhino.Geometry.Arc arc => key.Require(condition: arc.IsValid, value: value),
            Rhino.Geometry.Circle circle => key.Require(condition: circle.IsValid, value: value),
            Ellipse ellipse => key.Require(condition: ellipse.IsValid, value: value),
            Rectangle3d rectangle => key.Require(condition: rectangle.IsValid, value: value),
            Interval interval => key.Require(condition: interval.IsValid, value: value),
            Rhino.Geometry.Line line => key.Require(condition: line.IsValid, value: value),
            Rhino.Geometry.Polyline polyline => key.Require(condition: polyline.IsValid, value: value),
            GeometryBase geometry => key.Require(condition: geometry.IsValid, value: value),
            SurfaceCurvature => Fin.Succ(value),
            MeshCheckParameters => Fin.Succ(value),
            MeshPoint meshPoint => key.Require(condition: meshPoint.Point.IsValid, value: value),
            ComponentIndex component => key.Require(condition: component.ComponentIndexType != ComponentIndexType.InvalidType && component.Index >= 0, value: value),
            IntersectionEvent intersection => key.Require(condition:
                (intersection.IsPoint || intersection.IsOverlap)
                && intersection.PointA.IsValid
                && intersection.PointB.IsValid, value: value),
            ValueTuple<double, Vector3d> principal => key.Require(condition: RhinoMath.IsValidDouble(x: principal.Item1) && principal.Item2.IsValid, value: value),
            double scalar => key.Require(condition: RhinoMath.IsValidDouble(x: scalar), value: value),
            bool or int or Enum => Fin.Succ(value),
            _ => Fin.Fail<TValue>(key.InvalidResult()),
        };
    private static Fin<TValue> Require<TValue>(this OperationKey key, bool condition, TValue value) =>
        condition switch {
            true => Fin.Succ(value),
            false => Fin.Fail<TValue>(key.InvalidResult()),
        };
}
