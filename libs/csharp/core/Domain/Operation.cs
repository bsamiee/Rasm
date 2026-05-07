using System.Globalization;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

internal readonly record struct OperationKey {
    internal OperationKey(string name) =>
        Name = name;
    internal string Name { get; }
}

// --- [ERRORS] ----------------------------------------------------------------------------------

internal static class OperationFault {
    internal static Error MissingOperation() =>
        Error.New(message: "Geometry operation requires a query.");
    internal static Error MissingContext(this OperationKey key) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' requires a model context."));
    internal static Error InvalidInput(this OperationKey key) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' received invalid Rhino input."));
    internal static Error InvalidResult(this OperationKey key) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' produced no valid Rhino result."));
    internal static Error Unsupported(this OperationKey key, Type geometryType, Type outputType) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' does not support geometry '{geometryType.Name}' with output '{outputType.Name}'."));
}

internal static class SemanticFault {
    internal static Error PrimitiveNoEdges(this OperationKey key, string primitive) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no edges."));
    internal static Error PrimitiveNoVertices(this OperationKey key, string primitive) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no vertices."));
}

// --- [OPERATIONS] ------------------------------------------------------------------------------

internal static class GeometryResult {
    internal static Fin<Seq<TValue>> One<TValue>(this OperationKey key, TValue value) =>
        key.Value(value: value)
            .Map(static (TValue candidate) => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this OperationKey key, System.Collections.Generic.IEnumerable<TValue>? values) =>
        (Fin.Succ(key), Optional(values).ToFin(key.InvalidResult()))
            .Apply(static (OperationKey operation, System.Collections.Generic.IEnumerable<TValue> candidates) =>
                candidates.Aggregate(
                        seed: (Operation: operation, Result: Fin.Succ(Seq<TValue>())),
                        func: static ((OperationKey Operation, Fin<Seq<TValue>> Result) current, TValue candidate) => (
                            current.Operation,
                            Result: (current.Result, current.Operation.Value(value: candidate))
                                .Apply(static (Seq<TValue> previous, TValue next) => next.Cons(previous))
                                .As()))
                    .Result)
            .As()
            .Bind(static (Fin<Seq<TValue>> result) => result.Map(static (Seq<TValue> values) => values.Rev()));
    internal static Fin<Seq<TValue>> Solved<TValue>(this OperationKey key, bool solved, TValue value) =>
        solved switch {
            true => key.One(value: value),
            false => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
    private static Fin<TValue> Value<TValue>(this OperationKey key, TValue value) =>
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
            IntersectionEvent intersection => key.Require(condition: (
                intersection.IsPoint || intersection.IsOverlap,
                intersection.PointA.IsValid,
                intersection.PointB.IsValid) == (true, true, true), value: value),
            ValueTuple<double, Vector3d> principal => key.Require(condition: RhinoMath.IsValidDouble(x: principal.Item1) && principal.Item2.IsValid, value: value),
            double scalar => key.Require(condition: RhinoMath.IsValidDouble(x: scalar), value: value),
            bool => Fin.Succ(value),
            int => Fin.Succ(value),
            Enum => Fin.Succ(value),
            _ => Fin.Fail<TValue>(key.InvalidResult()),
        };
    private static Fin<TValue> Require<TValue>(this OperationKey key, bool condition, TValue value) =>
        condition switch {
            true => Fin.Succ(value),
            false => Fin.Fail<TValue>(key.InvalidResult()),
        };
}
