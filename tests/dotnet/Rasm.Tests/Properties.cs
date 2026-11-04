using NSubstitute;
using Rasm.TestSupport;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Properties {
    // --- [CANCELLATION]
    [Fact]
    public void CancellationBeforeSampling() =>
        InContext(static (cancellation, token) => {
            Action<int> property = Substitute.For<Action<int>>();
            cancellation.Cancel();
            OperationCanceledException error = Assert.Throws<OperationCanceledException>(() => Gen.Const(0).ForAll(property, iter: 1, threads: 1));
            Assert.Equal(token, error.CancellationToken);
            property.DidNotReceiveWithAnyArgs()(default);
        });

    [Fact]
    public void CancellationAfterSuccess() =>
        InContext(static (cancellation, token) => {
            Action<int> completed = Substitute.For<Action<int>>();
            OperationCanceledException error = Assert.Throws<OperationCanceledException>(() => Gen.Const(0).ForAll(value => {
                completed(value);
                cancellation.Cancel();
            }, iter: 1, threads: 1));
            Assert.Equal(token, error.CancellationToken);
            completed.Received(1)(0);
        });

    [Fact]
    public void MatchingCancellationKeepsException() =>
        InContext(static (cancellation, token) => {
            OperationCanceledException expected = new(token);
            OperationCanceledException actual = Assert.Throws<OperationCanceledException>(() => Gen.Const(0).ForAll(_ => {
                cancellation.Cancel();
                throw expected;
            }, iter: 1, threads: 1));
            Assert.Same(expected, actual);
        });

    [Fact]
    public void OtherCancellationKeepsCounterexample() =>
        InContext(static (cancellation, _) => {
            using CancellationTokenSource other = new();
            OperationCanceledException expected = new(other.Token);
            CsCheckException actual = Assert.Throws<CsCheckException>(() => Gen.Const(0).ForAll(_ => {
                cancellation.Cancel();
                throw expected;
            }, iter: 1, threads: 1));
            Assert.Same(expected, actual.InnerException);
        });

    [Fact]
    public void FailuresKeepCounterexamples() =>
        InContext(static (cancellation, _) => {
            (int First, int Second) value = (17, -23);
            string seed = PCG.ThreadPCG.ToString();
            InvalidOperationException expected = new("rejected sample");
            CsCheckException actual = Assert.Throws<CsCheckException>(() => Gen.Const(value).ForAll(_ => {
                cancellation.Cancel();
                throw expected;
            }, seed: seed, iter: 1, threads: 1));
            Assert.Same(expected, actual.InnerException);
            Assert.Contains(seed, actual.Message, StringComparison.Ordinal);
            Assert.Contains(Check.Print(value), actual.Message, StringComparison.Ordinal);
        });

    // --- [CONTEXT]
    private static void InContext(Action<CancellationTokenSource, CancellationToken> assertions) {
        using CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        ExecutionContext.Run(Assert.IsType<ExecutionContext>(ExecutionContext.Capture()), _ => {
            TestContext parent = Assert.IsType<TestContext>(TestContext.Current);
            Assert.NotNull(parent.Test);
            TestContext.SetForTest(parent.Test, TestEngineStatus.Running, cancellation.Token);
            using TestContext context = Assert.IsType<TestContext>(TestContext.Current);
            assertions(cancellation, context.CancellationToken);
        }, state: null);
    }
}
