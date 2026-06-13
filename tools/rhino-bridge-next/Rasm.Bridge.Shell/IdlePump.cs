using System.Collections.Concurrent;
using Rhino;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the D-1 UI-marshal kernel. ALL host mutation marshals through here; one job per
// RhinoApp.Idle pulse keeps the host responsive between bridge frames. Failure is captured per
// job and surfaces as the awaiting caller's typed fault data — raw InvokeOnUiThread is forbidden
// (it routes throws through HostUtils.ExceptionReport silently). The cancellation token abandons
// the WAIT only: a queued job always runs on the host thread once dequeued (host law — UI work is
// never aborted mid-flight).
internal sealed class IdlePump : IDisposable {
    private readonly ConcurrentQueue<IdleJob> jobs = new();
    private readonly EventHandler pulse;
    private volatile bool disposed;

    internal IdlePump() {
        pulse = (_, _) => DrainOne();
        RhinoApp.Idle += pulse;
    }

    private sealed record IdleJob(Action Run, Action Abandon);

    internal Task<T> OnUiThreadAsync<T>(Func<T> job, CancellationToken ct) {
        ObjectDisposedException.ThrowIf(condition: disposed, instance: this);
        if (RhinoApp.IsOnMainThread) {
            return InlineAsync(job: job);
        }
        TaskCompletionSource<T> completion = new(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
        jobs.Enqueue(item: new IdleJob(
            Run: () => Invoke(job: job, completion: completion),
            Abandon: () => _ = completion.TrySetCanceled()));
        return completion.Task.WaitAsync(cancellationToken: ct);
    }

    public void Dispose() {
        bool alreadyDisposed = disposed;
        disposed = true;
        if (!alreadyDisposed) {
            RhinoApp.Idle -= pulse;
            while (jobs.TryDequeue(result: out IdleJob? job)) {
                job.Abandon();
            }
        }
    }

    private void DrainOne() {
        if (!disposed && jobs.TryDequeue(result: out IdleJob? job)) {
            job.Run();
        }
    }

    private static Task<T> InlineAsync<T>(Func<T> job) {
        // BOUNDARY ADAPTER — callers already on the host thread (Closing-driven work) run inline;
        // queueing here would deadlock the frame against its own pump.
        try {
            return Task.FromResult(result: job());
        } catch (Exception error) when (NonFatal(error: error)) {
            return Task.FromException<T>(exception: error);
        }
    }

    private static void Invoke<T>(Func<T> job, TaskCompletionSource<T> completion) {
        // BOUNDARY ADAPTER — per-job capture: the failure becomes the awaiting caller's exception,
        // never a swallowed host report.
        try {
            _ = completion.TrySetResult(result: job());
        } catch (Exception error) when (NonFatal(error: error)) {
            _ = completion.TrySetException(exception: error);
        }
    }

    private static bool NonFatal(Exception error) =>
        error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException;
}
