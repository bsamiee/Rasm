using CsCheck;
using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SessionTests {

    // --- [THREADS]

    [Fact]
    public async Task OnUiThreadMovesWorkFromTheTestThreadOntoRhinoMainThread() {
        Assert.False(RhinoApp.IsOnMainThread);
        Assert.True(await RhinoHost.RunAsync(IO.lift(static () => RhinoApp.IsOnMainThread).OnUiThread()));
    }

    [Fact]
    public async Task NestedOnUiThreadStaysOnTheMainThread() =>
        Assert.True(await RhinoHost.RunAsync(IO.lift(static () => RhinoApp.IsOnMainThread).OnUiThread().OnUiThread()));

    [Fact]
    public Task ConcurrentCallersEachRunOnTheMainThread() =>
        Gen.Int[2, 16].SampleAsync(static async callers =>
            Assert.All(
                await Task.WhenAll(Enumerable.Range(0, callers).Select(static _ => Task.Run(static () => RhinoHost.RunAsync(IO.lift(static () => RhinoApp.IsOnMainThread).OnUiThread())))),
                Assert.True));

    // --- [FAILURES]

    [Fact]
    public async Task ExceptionOnTheMainThreadReturnsAsTheRunFailure() {
        InvalidOperationException thrown = new(nameof(ExceptionOnTheMainThreadReturnsAsTheRunFailure));
        Assert.Same(thrown, (await Expect<Error>.FailAsync(IO.lift(Unit () => throw thrown).OnUiThread())).ToException());
    }
}
