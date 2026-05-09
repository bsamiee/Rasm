using System.Collections.Generic;
using System.Globalization;
using Core.Runtime;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Core.Domain;

// --- [MODELS] ----------------------------------------------------------------------------------

internal readonly record struct OperationKey {
    internal OperationKey(string name) =>
        Name = name;
    internal string Name { get; }
}

[Union]
internal abstract partial record OperationOutcome<TValue> {
    internal sealed record One(TValue Value) : OperationOutcome<TValue>;
    internal sealed record Many(Seq<TValue> Values) : OperationOutcome<TValue>;
    internal sealed record SolvedSuccess(TValue Value) : OperationOutcome<TValue>;
    internal sealed record SolvedFailure(TValue Witness) : OperationOutcome<TValue>;
    internal static OperationOutcome<TValue> Solved(bool isSolved, TValue value) =>
        isSolved switch {
            true => new SolvedSuccess(Value: value),
            false => new SolvedFailure(Witness: value),
        };
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
    internal static Fin<Seq<TValue>> Result<TValue>(this OperationKey key, OperationOutcome<TValue> outcome) =>
        outcome switch {
            OperationOutcome<TValue>.One single => key.RequireValid(value: single.Value)
                .Map(static (TValue candidate) => Seq(candidate)),
            OperationOutcome<TValue>.Many multi => multi.Values.Fold(
                    initialState: (Operation: key, Result: Fin.Succ(Seq<TValue>())),
                    f: static ((OperationKey Operation, Fin<Seq<TValue>> Result) current, TValue candidate) => (
                        current.Operation,
                        Result: (current.Result, current.Operation.RequireValid(value: candidate))
                            .Apply(static (Seq<TValue> previous, TValue next) => next.Cons(previous))
                            .As()))
                .Result
                .Map(static (Seq<TValue> values) => values.Rev()),
            OperationOutcome<TValue>.SolvedSuccess solved => key.Result(outcome: new OperationOutcome<TValue>.One(Value: solved.Value)),
            OperationOutcome<TValue>.SolvedFailure => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
            _ => Fin.Fail<Seq<TValue>>(key.InvalidResult()),
        };
}
