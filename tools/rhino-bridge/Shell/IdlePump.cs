using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class IdlePump : IDisposable {
    private readonly ConcurrentQueue<IdleJob> jobs = new();
    private readonly EventHandler pulse;
    private volatile bool disposed;

    internal IdlePump() {
        pulse = (_, _) => DrainOne();
        RhinoApp.Idle += pulse;
    }

    private sealed class IdleJob(Action run, Action abandon) {
        private int pending = 1;

        internal void Run() {
            if (Interlocked.Exchange(ref pending, 0) == 1) {
                run();
            }
        }

        internal void Abandon() {
            if (Interlocked.Exchange(ref pending, 0) == 1) {
                abandon();
            }
        }
    }

    internal async Task<T> OnUiThreadAsync<T>(Func<T> job, CancellationToken ct) {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (RhinoApp.IsOnMainThread) {
            return await InlineAsync(job).ConfigureAwait(false);
        }
        TaskCompletionSource<T> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IdleJob queued = new(
            run: () => Invoke(job, completion),
            abandon: () => _ = completion.TrySetCanceled(ct));
        jobs.Enqueue(queued);
        await using ConfiguredAsyncDisposable cancellation = ct.Register(queued.Abandon).ConfigureAwait(false);
        return await completion.Task.ConfigureAwait(false);
    }

    public void Dispose() {
        bool alreadyDisposed = disposed;
        disposed = true;
        if (!alreadyDisposed) {
            RhinoApp.Idle -= pulse;
            while (jobs.TryDequeue(out IdleJob? job)) {
                job.Abandon();
            }
        }
    }

    private void DrainOne() {
        if (!disposed && jobs.TryDequeue(out IdleJob? job)) {
            job.Run();
        }
    }

    private static Task<T> InlineAsync<T>(Func<T> job) {
        try {
            return Task.FromResult(job());
        } catch (Exception error) when (NonFatal(error)) {
            return Task.FromException<T>(error);
        }
    }

    private static void Invoke<T>(Func<T> job, TaskCompletionSource<T> completion) {
        try {
            _ = completion.TrySetResult(job());
        } catch (Exception error) when (NonFatal(error)) {
            _ = completion.TrySetException(error);
        }
    }

    private static bool NonFatal(Exception error) =>
        error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException;
}
