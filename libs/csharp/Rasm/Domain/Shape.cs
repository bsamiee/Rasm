using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------

[Union]
public partial record Shape {
    public sealed record Native(GeometryBase Geometry) : Shape;
    public sealed record Point(Rhino.Geometry.Point3d Value) : Shape;
    public sealed record Box(Rhino.Geometry.Box Value) : Shape;
    public sealed record BoundingBox(Rhino.Geometry.BoundingBox Value) : Shape;
    public sealed record Line(Rhino.Geometry.Line Value) : Shape;
    public sealed record Polyline(Rhino.Geometry.Polyline Value) : Shape;
    public sealed record Plane(Rhino.Geometry.Plane Value) : Shape;
    public sealed record Sphere(Rhino.Geometry.Sphere Value) : Shape;
    public sealed record Cylinder(Rhino.Geometry.Cylinder Value) : Shape;
    public sealed record Cone(Rhino.Geometry.Cone Value) : Shape;
    public sealed record Torus(Rhino.Geometry.Torus Value) : Shape;
    public sealed record Circle(Rhino.Geometry.Circle Value) : Shape;
    public sealed record Arc(Rhino.Geometry.Arc Value) : Shape;
    public Type ClrType =>
        Switch(
            native: static n => n.Geometry.GetType(),
            point: static _ => typeof(Rhino.Geometry.Point3d),
            box: static _ => typeof(Rhino.Geometry.Box),
            boundingBox: static _ => typeof(Rhino.Geometry.BoundingBox),
            line: static _ => typeof(Rhino.Geometry.Line),
            polyline: static _ => typeof(Rhino.Geometry.Polyline),
            plane: static _ => typeof(Rhino.Geometry.Plane),
            sphere: static _ => typeof(Rhino.Geometry.Sphere),
            cylinder: static _ => typeof(Rhino.Geometry.Cylinder),
            cone: static _ => typeof(Rhino.Geometry.Cone),
            torus: static _ => typeof(Rhino.Geometry.Torus),
            circle: static _ => typeof(Rhino.Geometry.Circle),
            arc: static _ => typeof(Rhino.Geometry.Arc));
    internal Fin<Shape> Validate(Op key) =>
        Switch(
            state: (Self: this, Key: key),
            native: static (s, n) => n.Geometry.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            point: static (s, point) => point.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            box: static (s, b) => b.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            boundingBox: static (s, bbox) => bbox.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            line: static (s, line) => line.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            polyline: static (s, polyline) => polyline.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            plane: static (s, plane) => plane.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            sphere: static (s, sphere) => sphere.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            cylinder: static (s, cylinder) => cylinder.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            cone: static (s, cone) => cone.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            torus: static (s, torus) => torus.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            circle: static (s, circle) => circle.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()),
            arc: static (s, arc) => arc.Value.IsValid ? Fin.Succ(s.Self) : Fin.Fail<Shape>(s.Key.InvalidInput()));
    public static Seq<Type> Items { get; } = Seq(
        typeof(GeometryBase),
        typeof(Rhino.Geometry.Point3d),
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
            native: static n => n.Geometry,
            point: static point => point.Value,
            box: static b => b.Value,
            boundingBox: static bbox => bbox.Value,
            line: static line => line.Value,
            polyline: static polyline => polyline.Value,
            plane: static plane => plane.Value,
            sphere: static sphere => sphere.Value,
            cylinder: static cylinder => cylinder.Value,
            cone: static cone => cone.Value,
            torus: static torus => torus.Value,
            circle: static circle => circle.Value,
            arc: static arc => arc.Value);
    public static Option<Shape> From(object value) =>
        value switch {
            GeometryBase geometry => Some<Shape>(new Native(Geometry: geometry)),
            Rhino.Geometry.Point3d point => Some<Shape>(new Point(Value: point)),
            Rhino.Geometry.Box box => Some<Shape>(new Box(Value: box)),
            Rhino.Geometry.BoundingBox bbox => Some<Shape>(new BoundingBox(Value: bbox)),
            Rhino.Geometry.Line line => Some<Shape>(new Line(Value: line)),
            Rhino.Geometry.Polyline polyline => Some<Shape>(new Polyline(Value: polyline)),
            Rhino.Geometry.Plane plane => Some<Shape>(new Plane(Value: plane)),
            Rhino.Geometry.Sphere sphere => Some<Shape>(new Sphere(Value: sphere)),
            Rhino.Geometry.Cylinder cylinder => Some<Shape>(new Cylinder(Value: cylinder)),
            Rhino.Geometry.Cone cone => Some<Shape>(new Cone(Value: cone)),
            Rhino.Geometry.Torus torus => Some<Shape>(new Torus(Value: torus)),
            Rhino.Geometry.Circle circle => Some<Shape>(new Circle(Value: circle)),
            Rhino.Geometry.Arc arc => Some<Shape>(new Arc(Value: arc)),
            _ => None,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Validity {
    internal static Fin<TValue> RequireValid<TValue>(this Op key, TValue value) =>
        value switch {
            Point2d point => key.Require(condition: point.IsValid, value: value),
            Point3d point => key.Require(condition: point.IsValid, value: value),
            Vector3d vector => key.Require(condition: vector.IsValid, value: value),
            Plane plane => key.Require(condition: plane.IsValid, value: value),
            BoundingBox box => key.Require(condition: box.IsValid, value: value),
            Box box => key.Require(condition: box.IsValid, value: value),
            Sphere sphere => key.Require(condition: sphere.IsValid, value: value),
            Cylinder cylinder => key.Require(condition: cylinder.IsValid, value: value),
            Cone cone => key.Require(condition: cone.IsValid, value: value),
            Torus torus => key.Require(condition: torus.IsValid, value: value),
            Arc arc => key.Require(condition: arc.IsValid, value: value),
            Circle circle => key.Require(condition: circle.IsValid, value: value),
            Ellipse ellipse => key.Require(condition: ellipse.IsValid, value: value),
            Rectangle3d rectangle => key.Require(condition: rectangle.IsValid, value: value),
            Interval interval => key.Require(condition: interval.IsValid, value: value),
            Line line => key.Require(condition: line.IsValid, value: value),
            Polyline polyline => key.Require(condition: polyline.IsValid, value: value),
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
    private static Fin<TValue> Require<TValue>(this Op key, bool condition, TValue value) =>
        condition switch {
            true => Fin.Succ(value),
            false => Fin.Fail<TValue>(key.InvalidResult()),
        };
}
