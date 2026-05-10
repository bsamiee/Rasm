using System.Collections.Generic;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------

internal readonly record struct Op {
    internal Op(string name) =>
        Name = name;
    internal string Name { get; }
}

[Union]
internal abstract partial record OpResult<TValue> {
    internal sealed record One(TValue Value) : OpResult<TValue>;
    internal sealed record Many(Seq<TValue> Values) : OpResult<TValue>;
    internal sealed record SolvedSuccess(TValue Value) : OpResult<TValue>;
    internal sealed record SolvedFailure(TValue Witness) : OpResult<TValue>;
    internal static OpResult<TValue> Solved(bool isSolved, TValue value) =>
        isSolved switch {
            true => new SolvedSuccess(Value: value),
            false => new SolvedFailure(Witness: value),
        };
    internal Fin<Seq<TValue>> Reduce(Op key) =>
        Switch<Op, Fin<Seq<TValue>>>(
            state: key,
            one: static (k, single) => k.RequireValid(value: single.Value)
                .Map(static candidate => Seq(candidate)),
            many: static (k, multi) => multi.Values.Fold(
                    initialState: (Operation: k, Result: Fin.Succ(Seq<TValue>())),
                    f: static (current, candidate) => (
                        current.Operation,
                        Result: (current.Result, current.Operation.RequireValid(value: candidate))
                            .Apply(static (previous, next) => next.Cons(previous))
                            .As()))
                .Result
                .Map(static values => values.Rev()),
            solvedSuccess: static (k, solved) => new One(Value: solved.Value).Reduce(key: k),
            solvedFailure: static (k, _) => Fin.Fail<Seq<TValue>>(k.InvalidResult()));
}

// --- [ERRORS] --------------------------------------------------------------------------

internal static class OpFault {
    internal static Error MissingOperation() =>
        Error.New(message: "Geometry operation requires a query.");
    internal static Error MissingContext(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' requires a model context.");
    internal static Error InvalidInput(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' received invalid Rhino input.");
    internal static Error InvalidResult(this Op key) =>
        Error.New(message: $"Geometry operation '{key.Name}' produced no valid Rhino result.");
    internal static Error Unsupported(this Op key, Type geometryType, Type outputType) =>
        Error.New(message: $"Geometry operation '{key.Name}' does not support geometry '{geometryType.Name}' with output '{outputType.Name}'.");
    internal static Error ComputationFailed(string label) =>
        Error.New(message: $"Rhino {label} computation failed.");
    internal static Error ComputationUnsupported(string label, Type geometryType) =>
        Error.New(message: $"Rhino {label} computation does not support geometry '{geometryType.Name}'.");
    internal static Error PrimitiveNoEdges(this Op key, string primitive) =>
        Error.New(message: $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no edges.");
    internal static Error PrimitiveNoVertices(this Op key, string primitive) =>
        Error.New(message: $"Geometry operation '{key.Name}' rejects '{primitive}' primitive: no vertices.");
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class ValidationLifts {
    internal static Eff<Context, T> ToEff<T>(this Validation<Error, T> validation) =>
        validation.ToFin();
}
