using System.Diagnostics;
using Core.Composition;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static LanguageExt.Prelude;

namespace Core.Tests.Composition;

// --- [COMPOSITION] -----------------------------------------------------------------------------

[CollectionDefinition(nameof(ActivitySourceScope))]
public sealed class ActivitySourceScope;

[Collection(nameof(ActivitySourceScope))]
public sealed class RegistrationSpec {
    [Fact]
    public void RegistersFactory() {
        using ServiceProvider provider = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();

        OperationFactory factory = provider.GetRequiredService<OperationFactory>();
        Fin<Operation<int, int>> result = factory.Create<int, int>(
            operation: static (int value) => Fin.Succ(Seq(value)));

        Assert.True(
            condition: result.IsSucc,
            userMessage: result.Match(
                Succ: static (Operation<int, int> _) => string.Empty,
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void RejectsMissingFactoryOperation() {
        using ServiceProvider provider = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();

        OperationFactory factory = provider.GetRequiredService<OperationFactory>();
        Fin<Operation<int, int>> result = factory.Create<int, int>(
            operation: null!);

        Assert.True(
            condition: result.IsFail,
            userMessage: result.Match(
                Succ: static (Operation<int, int> _) => "Missing operation unexpectedly resolved.",
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void ExecutesWithoutTraceListener() {
        using ServiceProvider provider = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();

        Fin<Validation<Error, Seq<int>>> result = provider.GetRequiredService<OperationFactory>()
            .Create<int, int>(operation: static (int value) => Fin.Succ(Seq(value)))
            .Map(static (Operation<int, int> operation) =>
                operation.Execute(
                    executionMode: Operation.Mode.FailFast,
                    input: [1, 2]));

        Assert.True(
            condition: result.Bind(static (Validation<Error, Seq<int>> output) => output.ToFin()).IsSucc,
            userMessage: result.Match(
                Succ: static (Validation<Error, Seq<int>> output) => output.ToFin().Match(
                    Succ: static (Seq<int> _) => string.Empty,
                    Fail: static (Error fault) => fault.Message),
                Fail: static (Error fault) => fault.Message));
    }

    [Fact]
    public void EmitsTraceTags() {
        using ServiceProvider provider = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();
        ActivitySource source = provider.GetRequiredService<ActivitySource>();
        Activity? stopped = default;
        using ActivityListener listener = new() {
            ShouldListenTo = static (ActivitySource candidate) => StringComparer.Ordinal.Equals(
                x: candidate.Name,
                y: "Rasm.Core"),
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = (Activity activity) => stopped = activity,
        };
        ActivitySource.AddActivityListener(listener: listener);

        Fin<Validation<Error, Seq<int>>> result = provider.GetRequiredService<OperationFactory>()
            .Create<int, int>(operation: static (int value) => Fin.Succ(Seq(value)))
            .Map(static (Operation<int, int> operation) =>
                operation.Execute(
                    executionMode: Operation.Mode.Accumulate,
                    input: [1, 2]));

        Assert.True(
            condition: result.Bind(static (Validation<Error, Seq<int>> output) => output.ToFin()).IsSucc,
            userMessage: result.Match(
                Succ: static (Validation<Error, Seq<int>> output) => output.ToFin().Match(
                    Succ: static (Seq<int> _) => string.Empty,
                    Fail: static (Error fault) => fault.Message),
                Fail: static (Error fault) => fault.Message));
        Activity activity = Assert.IsType<Activity>(@object: stopped);
        Assert.Equal(expected: "rasm.operation", actual: activity.OperationName);
        Assert.Equal(expected: "rasm.operation", actual: activity.GetTagItem(key: "rasm.operation"));
        Assert.Equal(expected: typeof(int).FullName, actual: activity.GetTagItem(key: "rasm.input.type"));
        Assert.Equal(expected: typeof(int).FullName, actual: activity.GetTagItem(key: "rasm.output.type"));
        Assert.Equal(expected: 2, actual: activity.GetTagItem(key: "rasm.input.count"));
        Assert.Equal(expected: "Accumulate", actual: activity.GetTagItem(key: "rasm.execution"));
        Assert.Equal(expected: true, actual: activity.GetTagItem(key: "rasm.success"));
    }

    [Fact]
    public void EmitsTraceFailureTags() {
        using ServiceProvider provider = new ServiceCollection()
            .AddCore()
            .BuildServiceProvider();
        ActivitySource source = provider.GetRequiredService<ActivitySource>();
        Activity? stopped = default;
        using ActivityListener listener = new() {
            ShouldListenTo = static (ActivitySource candidate) => StringComparer.Ordinal.Equals(
                x: candidate.Name,
                y: "Rasm.Core"),
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = (Activity activity) => stopped = activity,
        };
        ActivitySource.AddActivityListener(listener: listener);

        Fin<Validation<Error, Seq<int>>> result = provider.GetRequiredService<OperationFactory>()
            .Create<int, int>(operation: static (int _) => Fin.Fail<Seq<int>>(Error.New(code: 42, message: "boom")))
            .Map(static (Operation<int, int> operation) =>
                operation.Execute(
                    executionMode: Operation.Mode.FailFast,
                    input: [1]));

        Assert.True(
            condition: result.Match(
                Succ: static (Validation<Error, Seq<int>> output) => output.ToFin().IsFail,
                Fail: static (Error _) => false),
            userMessage: result.Match(
                Succ: static (Validation<Error, Seq<int>> output) => output.ToFin().Match(
                    Succ: static (Seq<int> _) => "Failing operation unexpectedly succeeded.",
                    Fail: static (Error _) => string.Empty),
                Fail: static (Error fault) => fault.Message));
        Activity activity = Assert.IsType<Activity>(@object: stopped);
        Assert.Equal(expected: "FailFast", actual: activity.GetTagItem(key: "rasm.execution"));
        Assert.Equal(expected: false, actual: activity.GetTagItem(key: "rasm.success"));
        Assert.Equal(expected: 42, actual: activity.GetTagItem(key: "rasm.error.code"));
        Assert.Equal(expected: "boom", actual: activity.GetTagItem(key: "rasm.error.message"));
        Assert.Equal(expected: ActivityStatusCode.Error, actual: activity.Status);
    }
}
