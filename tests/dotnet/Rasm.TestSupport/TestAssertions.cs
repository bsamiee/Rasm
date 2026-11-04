using System.Runtime.ExceptionServices;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TestAssertions {
    extension<T>(Gen<T> gen) {
        public void ForAll(Action<T> property, string? seed = null, long iter = -1L, int time = -1, int threads = -1) {
            CancellationToken cancellation = TestContext.Current.CancellationToken;
            cancellation.ThrowIfCancellationRequested();
            try {
                gen.Sample(value => { cancellation.ThrowIfCancellationRequested(); property(value); }, seed: seed, iter: iter, time: time, threads: threads);
            } catch (CsCheckException exception) when (cancellation.IsCancellationRequested && exception.InnerException is OperationCanceledException canceled && canceled.CancellationToken == cancellation) {
                ExceptionDispatchInfo.Throw(canceled);
            }
            cancellation.ThrowIfCancellationRequested();
        }
    }

    public static void Equal(ReadOnlySpan<double> expected, ReadOnlySpan<double> actual, Tolerance tolerance,
        Func<ReadOnlySpan<double>, ReadOnlySpan<double>, Tolerance, bool> comparison) {
        if (!comparison(expected, actual, tolerance))
            Assert.Fail($"{Check.Print(expected.ToArray())} != {Check.Print(actual.ToArray())} within {tolerance}");
    }
}
