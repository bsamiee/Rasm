using System.Globalization;
using Core.Runtime;
using LanguageExt.Common;
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
    internal static Error ComputationFailed(string label) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Rhino {label} computation failed."));
    internal static Error ComputationUnsupported(string label, Type geometryType) =>
        Error.New(message: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"Rhino {label} computation does not support geometry '{geometryType.Name}'."));
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
        key.RequireValid(value: value)
            .Map(static (TValue candidate) => Seq(candidate));
    internal static Fin<Seq<TValue>> Many<TValue>(this OperationKey key, System.Collections.Generic.IEnumerable<TValue>? values) =>
        (Fin.Succ(key), Optional(values).ToFin(key.InvalidResult()))
            .Apply(static (OperationKey operation, System.Collections.Generic.IEnumerable<TValue> candidates) =>
                candidates.Aggregate(
                        seed: (Operation: operation, Result: Fin.Succ(Seq<TValue>())),
                        func: static ((OperationKey Operation, Fin<Seq<TValue>> Result) current, TValue candidate) => (
                            current.Operation,
                            Result: (current.Result, current.Operation.RequireValid(value: candidate))
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
}
