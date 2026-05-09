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
        Switch<Type>(
            native: static (Native n) => n.Geometry.GetType(),
            box: static (Box _) => typeof(Rhino.Geometry.Box),
            boundingBox: static (BoundingBox _) => typeof(Rhino.Geometry.BoundingBox),
            line: static (Line _) => typeof(Rhino.Geometry.Line),
            polyline: static (Polyline _) => typeof(Rhino.Geometry.Polyline),
            plane: static (Plane _) => typeof(Rhino.Geometry.Plane),
            sphere: static (Sphere _) => typeof(Rhino.Geometry.Sphere),
            cylinder: static (Cylinder _) => typeof(Rhino.Geometry.Cylinder),
            cone: static (Cone _) => typeof(Rhino.Geometry.Cone),
            torus: static (Torus _) => typeof(Rhino.Geometry.Torus),
            circle: static (Circle _) => typeof(Rhino.Geometry.Circle),
            arc: static (Arc _) => typeof(Rhino.Geometry.Arc));
    internal Fin<RhinoGeometry> Validate(OperationKey key) =>
        Switch<(RhinoGeometry Self, OperationKey Key), Fin<RhinoGeometry>>(
            state: (Self: this, Key: key),
            native: static ((RhinoGeometry Self, OperationKey Key) s, Native n) => n.Geometry.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            box: static ((RhinoGeometry Self, OperationKey Key) s, Box b) => b.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            boundingBox: static ((RhinoGeometry Self, OperationKey Key) s, BoundingBox bbox) => bbox.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            line: static ((RhinoGeometry Self, OperationKey Key) s, Line line) => line.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            polyline: static ((RhinoGeometry Self, OperationKey Key) s, Polyline polyline) => polyline.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            plane: static ((RhinoGeometry Self, OperationKey Key) s, Plane plane) => plane.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            sphere: static ((RhinoGeometry Self, OperationKey Key) s, Sphere sphere) => sphere.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            cylinder: static ((RhinoGeometry Self, OperationKey Key) s, Cylinder cylinder) => cylinder.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            cone: static ((RhinoGeometry Self, OperationKey Key) s, Cone cone) => cone.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            torus: static ((RhinoGeometry Self, OperationKey Key) s, Torus torus) => torus.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            circle: static ((RhinoGeometry Self, OperationKey Key) s, Circle circle) => circle.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()),
            arc: static ((RhinoGeometry Self, OperationKey Key) s, Arc arc) => arc.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<RhinoGeometry>(s.Key.InvalidInput()));
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
        Switch<object>(
            native: static (Native n) => n.Geometry,
            box: static (Box b) => b.Value,
            boundingBox: static (BoundingBox bbox) => bbox.Value,
            line: static (Line line) => line.Value,
            polyline: static (Polyline polyline) => polyline.Value,
            plane: static (Plane plane) => plane.Value,
            sphere: static (Sphere sphere) => sphere.Value,
            cylinder: static (Cylinder cylinder) => cylinder.Value,
            cone: static (Cone cone) => cone.Value,
            torus: static (Torus torus) => torus.Value,
            circle: static (Circle circle) => circle.Value,
            arc: static (Arc arc) => arc.Value);
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
