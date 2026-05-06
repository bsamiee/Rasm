using System.Threading;
using Core.Domain;
using FsCheck;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.FSharp.Core;
using Rhino.Geometry;
using Xunit;
using static LanguageExt.Prelude;
using FsCheckArb = FsCheck.Fluent.Arb;
using FsCheckGen = FsCheck.Fluent.Gen;
using FsCheckProp = FsCheck.FSharp.Prop;

namespace Core.Tests.Domain;

// --- [LAWS] ------------------------------------------------------------------------------------

public sealed class OperationSpec {
    private static readonly Config Law = Config.QuickThrowOnFailure.WithMaxTest(256);

    [Fact]
    public void RejectsMissingOperation() {
        Fin<Operation<int, int>> result = Operation.Create<int, int>(
            operation: null!);

        Assert.True(
            condition: result.IsFail,
            userMessage: result.Match(
                Succ: static (Operation<int, int> _) => "Missing operation unexpectedly succeeded.",
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void RejectsMissingContext() {
        Fin<Operation<GeometryBase, int>> result = Operation
            .Create<GeometryBase, int>(
                operation: static (GeometryBase _) => Fin.Succ(Seq(1)))
            .Bind(static (Operation<GeometryBase, int> program) =>
                program.WithValidation(context: null!));

        Assert.True(
            condition: result.IsFail,
            userMessage: result.Match(
                Succ: static (Operation<GeometryBase, int> _) => "Missing context unexpectedly succeeded.",
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void RejectsMissingValidationProgram() {
        Operation<GeometryBase, int> program = null!;
        Fin<Operation<GeometryBase, int>> result = program.WithValidation(
            context: null!);

        Assert.True(
            condition: result.IsFail,
            userMessage: result.Match(
                Succ: static (Operation<GeometryBase, int> _) => "Missing program unexpectedly succeeded.",
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void ExecutesEmptyInput() {
        Validation<Error, Seq<int>> result = Operation.Create<int, int>(
                operation: static (int value) => Fin.Succ(Seq(value)))
            .Bind(static (Operation<int, int> operation) =>
                operation.Execute(
                        executionMode: Operation.Mode.Accumulate,
                        input: [])
                    .ToFin())
            .ToValidation();

        Assert.Empty(collection: result.Match(
            Succ: static (Seq<int> output) => output,
            Fail: static (Error fault) => throw new Xunit.Sdk.XunitException(fault.Message)));
    }

    [Fact]
    public void AccumulatePreservesOrder() =>
        Check.One(
            name: nameof(AccumulatePreservesOrder),
            config: Law,
            property:
            FsCheckProp.ForAll(
                FsCheckArb.From(FsCheckGen.ArrayOf(FsCheckGen.Choose(-10_000, 10_000))),
                FSharpFunc<int[], bool>.FromConverter(static (int[] values) =>
                    Operation.Create<int, int>(
                            operation: static (int value) => Fin.Succ(Seq(value)))
                        .Bind((Operation<int, int> operation) =>
                            operation.Execute(
                                    executionMode: Operation.Mode.Accumulate,
                                    input: values)
                                .ToFin())
                        .Match(
                            Succ: (Seq<int> output) =>
                                output.ToArray() is int[] actual
                                && actual.Length == values.Length
                                && actual.SequenceEqual(values),
                            Fail: static (Error _) => false))));

    [Fact]
    public void FailFastPreservesOrderWhenAllInputsSucceed() =>
        Check.One(
            name: nameof(FailFastPreservesOrderWhenAllInputsSucceed),
            config: Law,
            property:
            FsCheckProp.ForAll(
                FsCheckArb.From(FsCheckGen.ArrayOf(FsCheckGen.Choose(-10_000, 10_000))),
                FSharpFunc<int[], bool>.FromConverter(static (int[] values) =>
                    Operation.Create<int, int>(
                            operation: static (int value) => Fin.Succ(Seq(value)))
                        .Bind((Operation<int, int> operation) =>
                            operation.Execute(
                                    executionMode: Operation.Mode.FailFast,
                                    input: values)
                                .ToFin())
                        .Match(
                            Succ: (Seq<int> output) =>
                                output.ToArray() is int[] actual
                                && actual.Length == values.Length
                                && actual.SequenceEqual(values),
                            Fail: static (Error _) => false))));

    [Fact]
    public void AccumulatesFaults() =>
        Check.One(
            name: nameof(AccumulatesFaults),
            config: Law,
            property:
            FsCheckProp.ForAll(
                FsCheckArb.From(FsCheckGen.Choose(1, 10_000)),
                FSharpFunc<int, bool>.FromConverter(static (int candidate) =>
                    Operation.Create<int, int>(
                            operation: static (int value) =>
                                value switch {
                                    < 0 => Fin.Fail<Seq<int>>(Error.New(code: value, message: "negative input")),
                                    _ => Fin.Succ(Seq(value)),
                                })
                        .Bind((Operation<int, int> operation) =>
                            operation.Execute(
                                    executionMode: Operation.Mode.Accumulate,
                                    input: [0, -candidate, 1, -(candidate + 1)])
                                .ToFin())
                        .Match(
                            Succ: static (Seq<int> _) => false,
                            Fail: (Error error) =>
                                error.Count == 2
                                && error.HasCode(code: -candidate)
                                && error.HasCode(code: -(candidate + 1))))));

    [Fact]
    public void AccumulateInvokesEveryInput() =>
        Check.One(
            name: nameof(AccumulateInvokesEveryInput),
            config: Law,
            property:
            FsCheckProp.ForAll(
                FsCheckArb.From(FsCheckGen.Choose(1, 10_000)),
                FSharpFunc<int, bool>.FromConverter((int candidate) => {
                    int calls = 0;
                    return Operation.Create<int, int>(
                            operation: (int value) => {
                                _ = Interlocked.Increment(location: ref calls);
                                return value switch {
                                    < 0 => Fin.Fail<Seq<int>>(Error.New(code: value, message: "negative input")),
                                    _ => Fin.Succ(Seq(value)),
                                };
                            })
                        .Bind((Operation<int, int> operation) =>
                            operation.Execute(
                                    executionMode: Operation.Mode.Accumulate,
                                    input: [0, -candidate, 1, -(candidate + 1)])
                                .ToFin())
                        .Match(
                            Succ: static (Seq<int> _) => false,
                            Fail: (Error error) =>
                                calls == 4
                                && error.Count == 2
                                && error.HasCode(code: -candidate)
                                && error.HasCode(code: -(candidate + 1)));
                })));

    [Fact]
    public void FailFastStopsAtFirstFailure() =>
        Check.One(
            name: nameof(FailFastStopsAtFirstFailure),
            config: Law,
            property:
            FsCheckProp.ForAll(
                FsCheckArb.From(FsCheckGen.Choose(1, 10_000)),
                FSharpFunc<int, bool>.FromConverter((int candidate) => {
                    int calls = 0;
                    return Operation.Create<int, int>(
                            operation: (int value) => {
                                _ = Interlocked.Increment(location: ref calls);
                                return value switch {
                                    < 0 => Fin.Fail<Seq<int>>(Error.New(code: value, message: "negative input")),
                                    _ => Fin.Succ(Seq(value)),
                                };
                            })
                        .Bind((Operation<int, int> operation) =>
                            operation.Execute(
                                    executionMode: Operation.Mode.FailFast,
                                    input: [0, candidate, -candidate, -(candidate + 1)])
                                .ToFin())
                        .Match(
                            Succ: static (Seq<int> _) => false,
                            Fail: (Error error) =>
                                calls == 3
                                && error.Count == 1
                                && error.HasCode(code: -candidate)
                                && !error.HasCode(code: -(candidate + 1)));
                })));
}
