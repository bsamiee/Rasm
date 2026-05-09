using Core.Domain;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
namespace Core.Runtime;

// --- [OPERATIONS] ------------------------------------------------------------------------------

internal static class RhinoMatch {
    internal static Fin<TValue> RequireValid<TValue>(this OperationKey key, TValue value) =>
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
    private static Fin<TValue> Require<TValue>(this OperationKey key, bool condition, TValue value) =>
        condition switch {
            true => Fin.Succ(value),
            false => Fin.Fail<TValue>(key.InvalidResult()),
        };
}
