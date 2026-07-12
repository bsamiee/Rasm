using System.Collections.Concurrent;
using Rhino;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] ------------------------------------------------------------------------

// Ownership: host UI marshal. One job per RhinoApp.Idle pulse keeps the host responsive; failures
// return to the awaiting caller, and cancellation abandons only the wait, never queued UI work.
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
        // BOUNDARY ADAPTER: host-thread callers run inline to avoid self-pump deadlock.
        try {
            return Task.FromResult(result: job());
        } catch (Exception error) when (NonFatal(error: error)) {
            return Task.FromException<T>(exception: error);
        }
    }

    private static void Invoke<T>(Func<T> job, TaskCompletionSource<T> completion) {
        // BOUNDARY ADAPTER: job failures become awaiting caller exceptions.
        try {
            _ = completion.TrySetResult(result: job());
        } catch (Exception error) when (NonFatal(error: error)) {
            _ = completion.TrySetException(exception: error);
        }
    }

    private static bool NonFatal(Exception error) =>
        error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException;
}
