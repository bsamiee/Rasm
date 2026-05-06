using System.Collections.Generic;
using System.Globalization;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;

namespace Core.Domain;

// --- [SERVICES] --------------------------------------------------------------------------------

internal abstract class Operation<TIn, TOut> where TIn : notnull {
    internal abstract Fin<Seq<TOut>> Apply(TIn input);

    internal virtual Validation<Error, Seq<TOut>> Execute(
        params ReadOnlySpan<TIn> input) =>
        Execute(
            operation: this,
            input: input,
            start: 0,
            length: input.Length);

    private static Validation<Error, Seq<TOut>> Execute(
        Operation<TIn, TOut> operation,
        ReadOnlySpan<TIn> input,
        int start,
        int length) =>
        length switch {
            0 => Fin.Succ(Seq<TOut>()).ToValidation(),
            1 => operation.Apply(input: input[start]).ToValidation(),
            _ => (
                Execute(
                    operation: operation,
                    input: input,
                    start: start,
                    length: length / 2),
                Execute(
                    operation: operation,
                    input: input,
                    start: start + (length / 2),
                    length: length - (length / 2))
            ).Apply(static (Seq<TOut> left, Seq<TOut> right) => left + right)
            .As(),
        };

}

internal readonly record struct OperationKey {
    internal OperationKey(string name) =>
        Name = name;

    internal string Name { get; }
}

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

internal static class GeometryResult {
    internal static Fin<Seq<TValue>> One<TValue>(this OperationKey key, TValue value) =>
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
            SurfaceCurvature curvature => Optional(curvature)
                .ToFin(key.InvalidResult())
                .Map(static (SurfaceCurvature candidate) => Seq((TValue)(object)candidate)),
            MeshPoint meshPoint => key.Require(condition: meshPoint.Point.IsValid, value: value),
            ComponentIndex component => key.Require(condition: component.ComponentIndexType != ComponentIndexType.InvalidType && component.Index >= 0, value: value),
            IntersectionEvent intersection => key.Require(condition: (
                intersection.IsPoint || intersection.IsOverlap,
                intersection.PointA.IsValid,
                intersection.PointB.IsValid) == (true, true, true), value: value),
            ValueTuple<double, Vector3d> principal => key.Require(condition: RhinoMath.IsValidDouble(x: principal.Item1) && principal.Item2.IsValid, value: value),
            double scalar => key.Require(condition: RhinoMath.IsValidDouble(x: scalar), value: value),
            bool bit => Fin.Succ(Seq((TValue)(object)bit)),
            int count => Fin.Succ(Seq((TValue)(object)count)),
            Enum choice => Fin.Succ(Seq((TValue)(object)choice)),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };

    internal static Fin<Seq<TValue>> Many<TValue>(this OperationKey key, IEnumerable<TValue>? values) =>
        (Fin.Succ(key), Optional(values).ToFin(key.InvalidResult()))
            .Apply(static (OperationKey operation, IEnumerable<TValue> candidates) => (
                Key: operation,
                Candidates: candidates))
            .As()
            .Bind(static ((OperationKey Key, IEnumerable<TValue> Candidates) state) => state.Candidates.Aggregate(
                seed: (
                    state.Key,
                    Result: Fin.Succ(Seq<TValue>())),
                func: static (
                    (OperationKey Key, Fin<Seq<TValue>> Result) current,
                    TValue candidate) => (
                    current.Key,
                    (current.Result, current.Key.One(value: candidate))
                        .Apply(static (Seq<TValue> previous, Seq<TValue> next) => previous + next)
                        .As())).Result);

    private static Fin<Seq<TValue>> Require<TValue>(this OperationKey key, bool condition, TValue value) =>
        condition switch {
            true => Fin.Succ(Seq(value)),
            false => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
}
