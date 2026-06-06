using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class AnalyzeGens {
    public static readonly Op Key = Op.Of(name: "analyze-spec");
    public static readonly Gen<int[]> Inputs = Gen.Int[start: -1_000, finish: 1_000].Array[1, 24];
    public static Operation<int, int> PerItem() =>
        Operation<int, int>.Build(
            key: Key,
            evaluator: static value => Fin.Succ(value: Seq(value, -value)).ToEff());
    public static Operation<int, int> Aggregate() =>
        Operation<int, int>.Build(
            key: Key,
            evaluator: static value => Fin.Succ(value: Seq(value)).ToEff(),
            aggregate: Some<Func<Seq<int>, Eff<Env, Seq<int>>>>(static values => Fin.Succ(value: Seq(Enumerable.Sum(values.AsIterable()))).ToEff()));
    public static Operation<int, int> RequiresContext() =>
        Operation<int, int>.Build(
            key: Key,
            requiresContext: true,
            evaluator: static _ => Env.Asks.Map(static context => Seq((int)Math.Round(context.Absolute.Value * 1_000.0, MidpointRounding.ToEven))).As());
    public static Operation<string, int> StringEcho() =>
        Operation<string, int>.Build(
            key: Key,
            evaluator: static value => Fin.Succ(value: Seq(value.Length)).ToEff());
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class AnalyzeExecutionLaws {
    [Fact]
    public void PerItemOperationsTraverseEveryInputAndFlattenChunks() =>
        Spec.ForAll(AnalyzeGens.Inputs, static values =>
            Spec.Valid(Analyze.In(absolute: 0.25, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters)
                .Run(operation: AnalyzeGens.PerItem(), input: values), then: actual =>
                Assert.Equal(
                    expected: [.. values.SelectMany(static value => new[] { value, -value })],
                    actual: [.. actual.AsIterable()])));

    [Fact]
    public void AggregateOperationsReceiveTheWholeBatchOnce() =>
        Spec.ForAll(AnalyzeGens.Inputs, static values =>
            Spec.Valid(Analyze.In(absolute: 0.25, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters)
                .Run(operation: AnalyzeGens.Aggregate(), input: values), then: actual =>
                Assert.Equal(expected: Seq(values.Sum()), actual: actual)));

    [Fact]
    public void ScopedContextSatisfiesContextRequiringOperations() =>
        Spec.Valid(
            Analyze.In(absolute: 0.25, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters)
                .Run(operation: AnalyzeGens.RequiresContext(), input: 1),
            then: static actual => Assert.Equal(expected: Seq(250), actual: actual));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class AnalyzeRailLaws {
    [Fact]
    public void MissingOperationAndMissingContextStayOnOperationRail() {
        Spec.Invalid(Analyze.Run<int, int>(operation: null, input: 1), then: static error =>
            Assert.Equal(expected: "Operation", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: AnalyzeGens.RequiresContext(), input: 1), then: static error =>
            Assert.Equal(expected: "Operation", actual: error.Category()));
        Spec.Invalid(Analyze.In(context: null!).Run(operation: AnalyzeGens.PerItem(), input: 1), then: static error =>
            Assert.Equal(expected: "Operation", actual: error.Category()));
    }

    [Fact]
    public void CancelledScopeStopsBeforeEvaluatorRuns() {
        using CancellationTokenSource source = new();
        source.Cancel();
        Spec.Invalid(
            Analyze.In(absolute: 0.25, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).With(cancellation: source.Token).Run(operation: AnalyzeGens.PerItem(), input: 1),
            then: static error => Assert.Equal(expected: "Cancelled", actual: error.Category()));
    }

    [Fact]
    public void RejectedOperationAndMissingGeometryFailBeforeEvaluator() {
        Spec.Invalid(Analyze.Run(operation: Operation<int, int>.Reject(key: AnalyzeGens.Key, fault: AnalyzeGens.Key.Unsupported(typeof(int), typeof(int))), input: 1),
            then: static error => Assert.Equal(expected: "Unsupported", actual: error.Category()));
        Spec.Invalid(
            Analyze.In(absolute: 0.25, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters).Run(operation: AnalyzeGens.StringEcho(), input: [null!]),
            then: static error => Assert.Equal(expected: "Geometry", actual: error.Category()));
    }
}
