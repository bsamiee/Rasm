# Resource Capsule Law — Edge

[UNINTERRUPTIBLE_SCOPE_AND_DISPOSAL_INDEPENDENCE]:
- `IO<A>.Uninterruptible` (and the `uninterruptible<M,A>` prelude form) wraps a computation in a local environment where cancellation-token requests are ignored. Cancellation signals arriving from an outer scope are masked for the duration of the wrapped computation. Critically, this masking does not affect the resource-release contract: any resources registered with `use` inside the uninterruptible scope are still released when that scope exits — whether the computation completes normally or faults. Masking cancellation only means the computation cannot be interrupted at a cancellation checkpoint; it does not mean resources are held indefinitely past the scope boundary.
- The design pressure: `uninterruptible` is correct for a short atomic native operation — a P/Invoke or handle-release call that must not be interrupted between acquire and the matching release. Wrapping a long-running `bracketIO` in `uninterruptible` defeats the drain-order cancellation window and is incorrect. The correct form: mark only the acquire step or the release step as uninterruptible, keeping the use body interruptible.
- `localIO<M,A>` creates an isolated cancellation environment where an inner `IO.cancel` raises an exception that propagates outward but does not cancel the outer `CancellationToken`. Resources acquired inside `localIO` are released at the `localIO` scope exit, which fires when the inner exception propagates past it. The outer token is not cancelled. The consequence: a capsule that uses `localIO` to isolate a single-operation cancel-and-retry pattern still disposes per-iteration resources via the local scope, while the outer drain loop remains live.

```csharp
// uninterruptible: wrap only the critical acquire, keep use body interruptible
static IO<IResource> SafeAcquire(Func<IResource> factory) =>
    uninterruptible(liftIO(factory));  // only the acquire step is uninterruptible

static IO<Result> UseResource(Func<IResource> factory, IO<Result> body) =>
    bracketIO(
        from r in use(SafeAcquire(factory))  // uninterruptible acquire
        from v in body                        // body remains interruptible
        select v
    );
```

[BRACKETIO_ACQUISITION_CATCH_FORM]:
- `bracketIO(acq, use, catch, fin)` is the four-argument form where `catch` is `Func<Error, IO<TResult>>` — a handler invoked when the `acq` computation raises an error. The `fin` (finalizer) runs unconditionally regardless of whether the acquisition succeeded or failed or the catch handler ran. This is the correct form when acquisition itself can partially allocate resources: the catch handler recovers from the partial failure and the finalizer cleans up whatever was registered before the failure.
- Contrast with the two-argument `bracketIO(acq, use, fin)` form, which has no acquisition-error handler: an error in `acq` propagates immediately after `fin` runs. The four-argument form allows returning a fallback value or re-raising a typed error from within the acquisition-error path rather than letting the original exception propagate.
- The `bracketIO(computation)` single-argument form — where all acquisition and use are expressed inside the `computation` monadic expression — subsumes both the two- and four-argument forms for new code. The multi-argument forms remain correct when the acquisition and finalization are logically separate from the use body.

[IO_POST_SYNCHRONIZATION_CONTEXT_AND_RESOURCE_REGISTRY]:
- `IO<A>.Post` and `MonadUnliftIO.PostIO` schedule the computation to run on the `SynchronizationContext` captured in the `EnvIO` at the start of the IO chain. The resource registry is part of `EnvIO`, not part of the synchronization context; resources registered with `use` before `.Post` are tracked in the same `EnvIO` that is threaded through after the context switch. The context switch does not create a new resource scope and does not reset the resource registry.
- The practical law: do not call `.Post` inside a `bracketIO` computation when the continuation after `.Post` acquires resources that must be released before the outer `bracketIO` exits — those resources register in the outer `bracketIO`'s `EnvIO` and are released at the outer bracket's exit, not at the `.Post` continuation's exit. If the post-synchronization-context resources must have a shorter lifetime, wrap the `.Post` call in an inner `bracketIO` that scopes the acquisition.

[PERIODICTIMER_CAPSULE_LIFETIME]:
- `PeriodicTimer(TimeSpan, TimeProvider)` accepts an injected `TimeProvider`, making it the testable clock-driven capsule for polling loops. The timer is `IDisposable`; calling `Dispose` signals the timer to stop: the current `WaitForNextTickAsync` call returns `false` (not a `OperationCanceledException`), and all subsequent calls return `false` immediately. A polling loop that checks the return value of `WaitForNextTickAsync` instead of catching `OperationCanceledException` produces the correct drain-time termination behavior.
- The cancellation behavior is asymmetric: passing a `CancellationToken` to `WaitForNextTickAsync` and cancelling it throws `OperationCanceledException` and affects only that single wait, while the underlying timer continues firing. `Dispose` stops the timer entirely and causes all future and current waits to return `false` cleanly. The capsule law: `Dispose` is the shutdown mechanism; `CancellationToken` is the per-wait interrupt. A design that calls `Dispose` on the timer via the cancellation callback conflates the two and will suppress future `false`-return detection if the timer is in a pool or reused.

```csharp
// PeriodicTimer capsule: TimeProvider-injected, drain via Dispose → false return
sealed class PollingCapsule : IAsyncDisposable
{
    readonly PeriodicTimer _timer;

    PollingCapsule(TimeSpan interval, TimeProvider clock) =>
        _timer = new PeriodicTimer(interval, clock);

    public static PollingCapsule Create(TimeSpan interval, TimeProvider clock) =>
        new(interval, clock);

    public async IO<Unit> RunAsync(IO<Unit> body) =>
        liftIO(async env =>
        {
            // WaitForNextTickAsync returns false on Dispose, not exception
            while (await _timer.WaitForNextTickAsync(env.Token))
                await body.RunAsync(env);
        });

    public ValueTask DisposeAsync()
    {
        _timer.Dispose();
        return ValueTask.CompletedTask;
    }
}
```

[SAFEHANDLE_HELPER_BASE_SELECTION]:
- `Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid` and `SafeHandleMinusOneIsInvalid` are BCL-provided convenience bases that implement `IsInvalid` for the two most common OS handle sentinel conventions: `IsInvalid = (handle == IntPtr.Zero || handle == (IntPtr)(-1))` for zero-or-minus-one, `IsInvalid = (handle == (IntPtr)(-1))` for minus-one-only. Subclassing these avoids the hand-rolled `IsInvalid` override entirely. The selection rule: POSIX file descriptors use minus-one-is-invalid (`-1` is `INVALID_HANDLE_VALUE` equivalent); Windows object handles predominantly use zero-or-minus-one-is-invalid. For cross-platform capsules that must work on both POSIX and Windows, `SafeHandleZeroOrMinusOneIsInvalid` is the more conservative base because it treats both sentinels as invalid.
- Neither helper base is in the `System.Runtime.InteropServices` namespace; both are in `Microsoft.Win32.SafeHandles`. For cross-platform code this namespace is available on all .NET 10 targets, not only Windows. Referencing them is not a platform-conditional requirement.

[MEMORY_PIN_AND_MEMORYHANDLE_CAPSULE]:
- `Memory<T>.Pin()` returns a `MemoryHandle`, which is an `IDisposable` struct. The `MemoryHandle.Pointer` property returns a `void*` to the start of the pinned memory. Disposing the `MemoryHandle` unpins the memory — either releasing the `GCHandle` for heap-allocated arrays or calling `IPinnable.Unpin()` for `MemoryManager<T>`-backed memory. A `MemoryHandle` that is not disposed keeps the backing memory pinned indefinitely: for ordinary heap arrays, this blocks GC compaction; for `MemoryManager<T>`-backed memory, the behavior is implementation-defined.
- The capsule law for `MemoryHandle`: the handle is not a handle to the data — it is a scoped lock on the physical address. The correct model is a two-phase capsule: `Memory<T>` is the logical extent, `MemoryHandle` is the physical address lock. The physical address is only valid inside the `MemoryHandle`'s scope. Any code that retains the `void*` past the `MemoryHandle.Dispose` call dereferences a dangling pointer.

```csharp
// MemoryHandle two-phase capsule: logical extent vs physical address lock
static Fin<TResult> WithPinnedAddress<T, TResult>(
    Memory<T>                  memory,
    Func<unsafe void*, int, Fin<TResult>> nativeOp)
{
    using var handle = memory.Pin();            // physical address lock
    unsafe
    {
        return nativeOp(handle.Pointer, memory.Length);
    }
}   // handle.Dispose() here — address lock released, Pointer invalid past this point
```

[MEMORYMANGER_PIN_UNPIN_SYMMETRY]:
- `MemoryManager<T>` is the extensibility point for `Memory<T>` with custom backing stores — native allocations, memory-mapped regions, GPU-resident buffers. The abstract methods `Pin(int elementIndex)` and `Unpin()` are the capsule's physical-address lock pair: `Pin` must increment an internal pin count and return a `MemoryHandle` that calls `Unpin` on disposal; `Unpin` decrements the count and releases the physical lock when the count reaches zero. The implementation must be thread-safe: multiple concurrent callers may `Pin` the same `MemoryManager` at overlapping element indices.
- `MemoryManager<T>` implements `IDisposable` via `System.IDisposable.Dispose()` (explicit interface). The explicit interface implementation means ordinary `using` statements do not call `Dispose` unless the variable is typed as `IDisposable` — a common trap when working with a `MemoryManager<T>` subclass reference. The capsule design: dispose the `MemoryManager` only after all `MemoryHandle` instances derived from it have been disposed. Disposing a `MemoryManager` while outstanding `MemoryHandle`s exist (and the pin count is non-zero) is undefined behavior at the native memory level; the `MemoryHandle.Pointer` values held by callers become dangling.
- `MemoryMarshal.TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T>, out TManager, out int start, out int length)` extracts the underlying `MemoryManager<T>` from a `Memory<T>` slice, returning the manager, the offset into it, and the count. This is the correct recovery path when a component receives a `Memory<T>` slice and needs to participate in the pin-count lifecycle — it recovers the manager and calls `Pin`/`Unpin` directly rather than through the `Memory<T>` surface.

[BOUNDED_CHANNEL_ITEMDROPPED_AS_DISPOSAL_SEAM]:
- `Channel.CreateBounded<T>(BoundedChannelOptions, Action<T> itemDropped)` accepts a callback that fires synchronously when an item is dropped due to a full channel under `DropOldest`, `DropNewest`, or `DropWrite` modes. For channels carrying `IDisposable` items, this callback is the only disposal seam: without it, dropped items are silently discarded without disposal. The channel's own `Complete` and drain do not dispose items remaining in the buffer; the reader must drain and dispose, or the `itemDropped` callback must dispose the items that never reach the reader.
- The `itemDropped` callback fires on the writer's thread during the `WriteAsync` or `TryWrite` call that triggers the drop. It must not block, must not throw, and must not call `WriteAsync` or `TryWrite` on the same channel (re-entrant drop). The law: keep the callback to at most one `Dispose()` call or a non-blocking enqueue to a separate disposal worker.
- For `BoundedChannelFullMode.DropWrite` (the written item is dropped, not an existing one), `itemDropped` receives the item that was just written and rejected — it was never in the channel's buffer. This is the only mode where `itemDropped` is guaranteed to fire before the channel contains or has processed the item.

```csharp
// BoundedChannel with resource-bearing items — itemDropped is the disposal seam
static Channel<IResource> BoundedResourceChannel(int capacity) =>
    Channel.CreateBounded<IResource>(
        new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.DropOldest },
        itemDropped: r => r.Dispose());   // dropped items disposed here; drained items disposed by reader
```

[OBJECTDISPOSEDEXCEPTION_THROWIF_CANONICAL_GUARD]:
- `ObjectDisposedException.ThrowIf(bool condition, object instance)` and `ObjectDisposedException.ThrowIf(bool condition, Type type)` are the expression-form disposal guards. They inline into a single `if (_disposed) throw` pattern that the JIT can optimize and that produces a correctly attributed exception message naming the type. The `object instance` overload uses `instance.GetType().FullName`; the `Type type` overload uses `type.FullName`. For a capsule whose disposal state is tracked by `Interlocked.Exchange(ref int, 1)` returning the prior value, the guard form is `ObjectDisposedException.ThrowIf(Interlocked.Exchange(ref _disposed, 0) == 0, this)` — though this conflates the check-and-mark idiom; the correct pattern is a two-field check-then-mark or a single `volatile int _disposed` read followed by a non-interlocked throw.
- The idiomatic capsule disposal guard: `_disposed` is a `volatile int`; `Dispose` uses `Interlocked.CompareExchange(ref _disposed, 1, 0)` to ensure exactly-once disposal; projection methods use `ObjectDisposedException.ThrowIf(_disposed == 1, this)` as the fast read-only gate. The two are not interchangeable: `Interlocked.CompareExchange` in the dispose path guarantees at-most-once teardown; the `ThrowIf` in the use path is a non-atomic read that is correct only because disposal is terminal (once disposed, `_disposed` is permanently 1 and reads of 1 are always correct). A type that can be "un-disposed" or "reset" after disposal cannot use this pattern.

```csharp
// Canonical disposal guard using ObjectDisposedException.ThrowIf
sealed class BoundedCapsule : IDisposable
{
    volatile int _disposed;
    readonly SafeHandle _handle;

    BoundedCapsule(SafeHandle handle) => _handle = handle;

    public Fin<T> Use<T>(Func<SafeHandle, Fin<T>> project)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        return _handle.IsInvalid || _handle.IsClosed
            ? Fin<T>.Fail(Error.New("handle invalid"))
            : project(_handle);
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        _handle.Dispose();
    }
}
```

[SAFEHANDLEHELPER_BASE_CROSSPLATFORM_ISVALID_INVARIANT]:
- A capsule that extends `SafeHandleZeroOrMinusOneIsInvalid` or `SafeHandleMinusOneIsInvalid` must not override `IsInvalid`. The helper base's `IsInvalid` implementation is the contract; overriding it to add a supplementary flag introduces a desync: `SafeHandle.ReleaseHandle` fires when the CLR's ref-count reaches zero and `IsClosed` becomes true — it does not re-check `IsInvalid` before calling `ReleaseHandle`. A capsule that sets a private `bool _logicallyInvalid` and overrides `IsInvalid` to return `_logicallyInvalid || handle == IntPtr.Zero` cannot prevent `ReleaseHandle` from running if the handle was once valid and the ref-count drained. The only valid path to suppress `ReleaseHandle` is `SetHandleAsInvalid()`, which marks the handle as permanently closed before the finalizer path.

[MEMORYMAPPEDVIEW_ACCESSOR_DISPOSAL_ORDERING]:
- `MemoryMappedFile.CreateViewAccessor` and `CreateViewStream` produce child views over the same mapped region. Each view holds an internal `SafeMemoryMappedViewHandle`; the underlying OS mapping is released when all views have been closed and the `MemoryMappedFile` is disposed. The BCL enforces no child-before-parent disposal ordering: `MemoryMappedFile.Dispose` marks its internal handle as invalid but does not force all views to flush or close — views that outlive the file object retain access to the mapped memory through their own `SafeMemoryMappedViewHandle`. The capsule law for memory-mapped resources: the file is the container, views are children, and the correct disposal order is (1) flush and dispose all views, (2) dispose the file. Reversing the order produces no immediate error (the OS reference count keeps the mapping live until all handles close) but violates the child-before-container principle and can cause silent data loss on writeable views that are flushed after the container is marked disposed.
