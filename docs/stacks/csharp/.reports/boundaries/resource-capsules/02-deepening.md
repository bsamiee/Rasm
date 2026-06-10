# Resource Capsule Law — Deepening

[FORK_RESOURCE_ISOLATION]:
- `IO<A>.Fork(Option<TimeSpan>)` creates a child execution on a thread-pool thread with a fresh, isolated `EnvIO` resource registry. Resources acquired inside the fork are tracked against that child registry and released when the forked computation ends — whether by success, failure, or timeout. Resources acquired in the parent before the fork are accessible to the forked computation but are shared: a forked computation that releases a parent-acquired resource removes it from the parent's registry, which can yield use-after-release in the parent or any sibling fork holding the same resource. Resources that must outlive the fork are acquired in the parent and passed as values — projections, not handles — into the forked computation; resources whose lifetime is bounded by the fork are acquired inside the fork.
- The `Fork` return value is a `ForkIO<A>` record with `Await` (`IO<A>`) and `Cancel` (`IO<Unit>`). `Await` re-raises any error from the forked computation into the awaiting monad; `Cancel` signals the fork's linked `CancellationToken`. Neither field disposes resources; disposal happens automatically on fork exit regardless of whether `Await` or `Cancel` was called. A pattern that awaits the fork to then dispose its resources in the parent breaks the isolation: the resources are already released by the time `Await` returns.

[RETRY_RESOURCE_RELEASE_GUARANTEE]:
- `retry`, `retryWhile`, `retryUntil`, and their `Schedule`-driven variants on `IO<A>` release any resources acquired in a failing iteration before the next iteration begins. On success, resources acquired in the succeeding iteration are tracked in the surrounding `EnvIO` scope. A `bracketIO(retry(...))` composes safely: the outer bracket releases only the surviving successful resources, not the already-released failed-iteration resources.
- The constraint on `SwapMaybe` construction (the lambda may be called multiple times during CAS retry) intersects here: if `SwapMaybe` is used inside a `retry`-wrapped `IO`, the CAS spin-retry and the `retry` operator are distinct levels. Double-release risk only arises if construction itself has side effects that register a resource and the CAS spin fires twice before the swap succeeds; the safe form acquires inside the `IO` step after the CAS confirms, never inside the CAS lambda.

[GCHANDLE_AS_CAPSULE]:
- `GCHandle.Alloc(obj, GCHandleType.Pinned)` pins a managed object at a fixed address, preventing the GC from moving it. The handle is a value type with `IsAllocated` as the validity gate; it must be freed with `GCHandle.Free()` or it leaks the pinning indefinitely, preventing compaction of that heap region. The capsule shape for `GCHandle`: a sealed class with a single `GCHandle _handle` field, `IDisposable` implemented as `if (_handle.IsAllocated) _handle.Free()`, and `AddrOfPinnedObject()` exposed only as a `nint` projection — never as the `GCHandle` itself.
- `GCHandleType.Normal` prevents collection without pinning; `GCHandleType.WeakTrackResurrection` participates in the resurrection phase; `GCHandleType.Weak` does not. `Normal` handles that are never freed produce managed memory leaks without any native-resource diagnostic surfacing them; they are invisible to standard leak-detection tooling that watches `SafeHandle` finalization.
- `GCHandle` cannot be the inner representation of a `SafeHandle` because it is a struct: passing it to P/Invoke does not give the CLR the lifetime hook that `SafeHandle` provides. The capsule must hold both: a `SafeHandle`-derived type for the native handle and a separately lifecycle-managed `GCHandle` for any managed object that the native layer holds a pointer into.

[NATIVEMEMORY_CAPSULE_INTERNALS]:
- `NativeMemory.Alloc(byteCount)` and `NativeMemory.AlignedAlloc(byteCount, alignment)` allocate unmanaged heap memory via the platform CRT allocator. The capsule for `NativeMemory` allocations derives from `SafeHandle` with `invalidHandleValue: IntPtr.Zero`, overrides `IsInvalid` as `handle == IntPtr.Zero`, and calls `NativeMemory.Free(handle.ToPointer())` in `ReleaseHandle`. `NativeMemory.AlignedFree` must be paired with `NativeMemory.AlignedAlloc`; calling `NativeMemory.Free` on an aligned allocation is undefined behavior in the underlying CRT.
- `GC.AllocateUninitializedArray<T>(length, pinned: true)` allocates a pinned managed array in the pinned object heap (POH). POH arrays are permanently pinned without incurring GC pressure from scatter-gather movement. For high-frequency native I/O buffers that must be pinned, this is the correct allocation path: it avoids the `GCHandle.Alloc(Pinned)` + `GCHandle.Free()` lifecycle on each buffer operation and does not risk forgetting to release the pin. The tradeoff: POH allocations are never compacted, so fragmentation is permanent; use only for long-lived or fixed-size buffers.
- `NativeMemory.AllocZeroed` zeroes on allocation; for security-sensitive memory, prefer zero-on-release via `NativeMemory.Clear(ptr, size)` in `ReleaseHandle` before `NativeMemory.Free`, not zero-on-allocate — callers that observe initial zero state rely on an allocation-site invariant that has no enforcement point.

[MARSHAL_INITHANDLE_CONSTRUCTION_DISCIPLINE]:
- `Marshal.InitHandle(SafeHandle instance, IntPtr handle)` initializes the `handle` field of a newly created `SafeHandle` subclass without calling `SetHandle`. It is the correct path when constructing a `SafeHandle` whose raw handle value comes from a factory that cannot be inlined into the constructor (e.g., a handle returned via an `out` parameter of a `[LibraryImport]` call). The incorrect alternative — passing `IntPtr.Zero` to the `SafeHandle` constructor and then calling `SetHandle(IntPtr)` — introduces a window between construction and `SetHandle` where the handle is marked invalid and could be disposed prematurely by a racing finalizer thread.
- `SafeHandle`'s protected constructor taking `(IntPtr invalidHandleValue, bool ownsHandle)` initializes the `handle` field to `invalidHandleValue` and sets the ref-count to 1 if `ownsHandle` is true. When the handle is provided by a P/Invoke `out IntPtr` parameter, the subclass constructor must accept `IntPtr.Zero` as the base constructor argument and call `Marshal.InitHandle(this, actualHandle)` after the P/Invoke returns successfully.

[OBJECTPOOL_POLICY_AND_RETURN_CONTRACT]:
- `ObjectPool<T>.Return(obj)` does not automatically reset `obj` to a clean state. If the policy type implements `IResettable`, `DefaultObjectPool<T>` calls `TryReset()` before pooling; if `TryReset()` returns `false`, the object is discarded rather than pooled. A pooled mutable object whose state bleeds across rentals creates a silent correctness bug: the next `Get()` caller receives stale accumulated state. Every pooled object either implements `IResettable.TryReset()` returning `true` after full reset, or holds no state that matters across rental boundaries.
- `LeakTrackingObjectPool<T>` wraps any `ObjectPool<T>` and attaches a stack-trace `WeakReference` to each rented object via a `ConditionalWeakTable`. In debug builds, finalization of a leased object that was not returned triggers an error. It is a diagnostic wrapper, not a production surface; it adds allocation pressure per rental and must be controlled by build configuration.
- `IPooledObjectPolicy<T>.Return(T obj)` returning `false` discards the object: the pool drops it without `Dispose`. Objects with `IDisposable` state returned to a pool that calls a policy returning `false` must be disposed by the policy, not by the pool's internal plumbing.

[CHANNEL_AS_DRAIN_SEAM]:
- `ChannelWriter<T>.Complete(Exception?)` signals that no more items will be written. `ChannelReader<T>.Completion` is a `Task` that transitions to completed only after `Complete` has been called and all pending reads have been satisfied — it is the canonical await point for confirming the channel has been fully drained. `Completion` does not fire after `Complete` while unread items remain in the buffer; it fires when the buffer is also empty.
- `BoundedChannelOptions.AllowSynchronousContinuations = false` (the default) guarantees that `WriteAsync` completions are dispatched asynchronously, preventing the writer from re-entering the reader's continuation inline and potentially deadlocking on a bounded channel that blocks writers when full. Setting it to `true` can improve throughput on hot single-producer/single-consumer paths but requires verifying that no reader holds a lock the writer also acquires.
- `Channel.CreateUnboundedPrioritized<T>(UnboundedPrioritizedChannelOptions<T>)` accepts an `IComparer<T>` for priority ordering; `TryRead` returns the highest-priority item. For drain-order where high-severity items must flush before low-severity ones, this is the BCL-native prioritized drain primitive. The `IComparer<T>` contract specifies total order; partial orders produce non-deterministic drain sequences.

[RATELIMITLEASE_AS_CAPSULE]:
- `RateLimiter.AttemptAcquire(int)` returns a `RateLimitLease` synchronously. `RateLimitLease.IsAcquired` distinguishes a successful from a failed lease; both forms are `IDisposable` and must be disposed. Disposing a failed lease is a no-op for most implementations but must not be skipped: a custom limiter that returns a stateful failed lease (e.g., one that records denial metrics on disposal) would silently lose that record if callers guard disposal on `IsAcquired`. Every `RateLimitLease`, success or failure, is disposed exactly once.
- `RateLimiter.AcquireAsync` with cancellation: if cancellation fires while a request is queued, the returned lease has `IsAcquired = false` and is already in a final state; calling `Dispose` on it is safe and required. A pattern that skips disposal when `OperationCanceledException` propagates produces a lease object that was allocated but never disposed.
- `PartitionedRateLimiter<TResource>.CreateChained(limiters)` acquires from all inner limiters in order; if any inner limiter fails, the successful leases from prior limiters are released before returning the failed outer lease. This cascading release happens inside the implementation. The caller still disposes the returned outer lease; the outer lease's `Dispose` is a no-op for already-released inner leases and completes any remaining bookkeeping.

[PIPE_BUFFER_OWNERSHIP_AND_ADVANCE]:
- `PipeWriter.GetMemory(sizeHint)` returns a `Memory<byte>` backed by a buffer segment managed by the `Pipe`'s `MemoryPool<byte>`. The caller writes into this memory and then calls `PipeWriter.Advance(int bytesWritten)` to commit exactly the bytes written — not the full segment size. Calling `Advance` with a value larger than the actual bytes written produces undefined behavior: the reader will see garbage bytes in the overcommitted region. The `PipeWriter` does not validate that `bytesWritten <= GetMemory(...).Length`; the invariant is caller-enforced.
- After `PipeReader.ReadAsync`, the returned `ReadResult.Buffer` is a `ReadOnlySequence<byte>` spanning one or more segments from the pipe's pool. The caller must call `PipeReader.AdvanceTo(consumed, examined)` before calling `ReadAsync` again. Setting `examined` further than `consumed` signals the pipe to wait for more data before returning a new result even if unconsumed data remains — this is the back-pressure signal for partial-parse scenarios.
- `PipeReader.Complete(Exception?)` signals the writer that the reader is done. After `Complete`, any buffered data in the pipe is released back to the `MemoryPool`. The capsule law for `Pipe`: writer-complete fires before reader-complete in the producer-consumer teardown; reversing the order produces behavior equivalent to `ChannelClosedException`, surfaced as `PipeReader.ReadAsync` returning `IsCompleted = true` without having read all buffered data.

[LOCK_SCOPE_DISPOSAL_ORDERING_CONSTRAINT]:
- `Lock.EnterScope()` returns `Lock.Scope`, a `ref struct`. The `ref struct` constraint means the compiler forbids the scope from being stored in a heap field, captured in a lambda, or returned from a method — all of which would allow it to outlive the stack frame or cross an `await`. A `Lock.Scope.Dispose()` call that throws `SynchronizationLockException` means the current thread does not hold the lock at the point of disposal, which can occur if a `using var scope = _lock.EnterScope()` block incorrectly re-enters the lock on a different thread via an unexpected synchronous continuation. The diagnostic: `Lock.IsHeldByCurrentThread` returns `true` only from the thread that entered; a `false` at the disposal site means a continuation routing bug.
- `Lock.TryEnter(TimeSpan)` returns `bool` without a `Scope`; callers must call `Lock.Exit()` explicitly on success, in a `try/finally`. There is no `TryEnterScope` overload.

[CANCELLATION_SOURCE_DISPOSAL_RACE]:
- `CancellationTokenSource.Dispose()` is not safe to call while other threads hold the `CancellationToken` and are calling `CancellationToken.Register(...)` concurrently. The internal `WaitHandle` and timer allocated by the source are freed by `Dispose`; a concurrent `Register` that accesses the disposed `WaitHandle` throws `ObjectDisposedException`. `CancellationTokenSource` is disposed only after all token-holder scopes have been fenced — specifically after `CancelAsync()` has completed and the drain has confirmed no new registrations are possible.
- `CancellationTokenSource.TryReset()` resets the source for reuse if and only if cancellation has not been requested. Returns `false` if the source was already canceled or if a `CancelAfter` timer is pending. A source that returns `false` from `TryReset` is terminal: dispose it and allocate a fresh source. A source for which `TryReset` returns `true` retains existing `Register` callbacks; callers that assume `TryReset` clears the callback list will re-invoke stale callbacks on the next cancellation.
- The `CancellationTokenSource(TimeSpan, TimeProvider)` constructor schedules the cancellation timer through the provided `TimeProvider`. `FakeTimeProvider.Advance(TimeSpan)` advances the virtual clock and triggers the cancellation synchronously within the `Advance` call. The timer fires on the `TimeProvider`'s own scheduler, which for `FakeTimeProvider` is the thread calling `Advance`; test code that calls `Advance` while holding a lock the cancellation callback also acquires will deadlock.

```csharp
// GCHandle capsule: pinned managed buffer with tracked release
sealed class PinnedBuffer : IDisposable
{
    readonly GCHandle _pin;
    readonly byte[]   _buf;
    bool              _disposed;

    PinnedBuffer(byte[] buf)
    {
        _buf = buf;
        _pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
    }

    public static PinnedBuffer Rent(int size) =>
        new(new byte[size]);

    // Callers receive a nint address projection — never the GCHandle itself
    public Fin<nint> Address() =>
        _disposed
            ? Fin<nint>.Fail(Error.New("disposed"))
            : Fin<nint>.Succ(_pin.AddrOfPinnedObject());

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_pin.IsAllocated) _pin.Free();
    }
}
```

```csharp
// NativeMemory capsule: aligned allocation with zero-on-release
sealed unsafe class AlignedBuffer : SafeHandle
{
    readonly nuint _byteCount;
    readonly nuint _alignment;

    AlignedBuffer(nuint byteCount, nuint alignment)
        : base(invalidHandleValue: IntPtr.Zero, ownsHandle: true)
    {
        _byteCount = byteCount;
        _alignment = alignment;
        Marshal.InitHandle(this, (IntPtr)NativeMemory.AlignedAlloc(byteCount, alignment));
    }

    public static AlignedBuffer Alloc(nuint byteCount, nuint alignment) =>
        new(byteCount, alignment);

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMemory.Clear(handle.ToPointer(), _byteCount);   // zero before free
        NativeMemory.AlignedFree(handle.ToPointer());
        return true;
    }

    // Projection: span over managed-address; handle never escapes
    public Fin<Span<byte>> AsSpan() =>
        IsInvalid || IsClosed
            ? Fin<Span<byte>>.Fail(Error.New("invalid"))
            : Fin<Span<byte>>.Succ(new Span<byte>(handle.ToPointer(), (int)_byteCount));
}
```

```csharp
// Drain seam: fence → cancel → channel drain → pipe drain
static async ValueTask DrainAsync(
    Channel<WorkItem> channel,
    PipeWriter        pipeWriter,
    TimeProvider      clock,
    CancellationToken host)
{
    // Step 1: fence new work
    channel.Writer.Complete();

    // Step 2: clock-scoped linked cancellation
    await using var drainCts = new CancellationTokenSource(TimeSpan.FromSeconds(20), clock);
    using var linked = CancellationTokenSource.CreateLinkedTokenSource(
        [host, drainCts.Token]);
    await linked.CancelAsync();

    // Step 3: channel drain — Completion fires when buffer is empty
    await channel.Reader.Completion.WaitAsync(linked.Token);

    // Step 4: pipe flush — writer Complete before reader Complete
    await pipeWriter.CompleteAsync();
}
```

```csharp
// RateLimitLease capsule: always dispose, regardless of IsAcquired
static IO<Fin<TResult>> WithPermit<TResult>(
    RateLimiter limiter,
    IO<TResult> work) =>
    bracketIO(
        from lease in use(liftIO(() => limiter.AttemptAcquire(1)),
                         l => liftIO(() => { l.Dispose(); return unit; }))
        from result in lease.IsAcquired
            ? work.Map(Fin<TResult>.Succ)
            : Pure(Fin<TResult>.Fail(Error.New("rate limited")))
        select result
    );
```
