# Resource Capsule Law

[CAPSULE_OWNERSHIP]:
- Every foreign resource has exactly one capsule owner responsible for acquire, project, and dispose. Borrowed and owned lifetimes are not separate types; they are cases of one capsule surface — a closed family with a `Borrowed` case that does not dispose and an `Owned` case that does. Projections from either case leave as value copies; the raw handle never escapes the capsule into caller-controlled scope.
- `SafeHandle` (`System.Runtime.InteropServices.SafeHandle`) is the BCL owner for unmanaged handles: it reference-counts through `DangerousAddRef`/`DangerousRelease`, runs `ReleaseHandle` via the GC finalizer, and survives garbage collection. For any raw `IntPtr` native handle, the capsule's inner representation must derive from `SafeHandle` — not hold a bare `IntPtr` — so P/Invoke marshaling never races disposal. `IsClosed` and `IsInvalid` are the canonical validity gates.
- `CriticalHandle` (non-reference-counting sibling) is the correct base when the native API has no reference-count semantics and the handle never passes through a `[LibraryImport]` marshaling layer. The selection rule: if the handle ever passes as a typed `SafeHandle` parameter in a `[LibraryImport]` call, use `SafeHandle` — the marshaller's `DangerousAddRef`/`DangerousRelease` pair inside `SafeHandleMarshaller<T>` prevents handle-recycling use-after-free on interrupted invocations, which is the documented reliability guarantee that `CriticalHandle` cannot provide. If the handle is created once, stored privately in a capsule, and released only on capsule disposal with no marshaling layer, `CriticalHandle` is acceptable — the ref-count path is never exercised, and omitting it removes one lifecycle layer. The reliability cost is real: `SafeHandle`'s ref-count keeps a handle alive during in-flight marshaled calls, preventing the OS from recycling a handle value and reassigning it to a different allocation while a stale raw value is still live on another thread; `CriticalHandle`'s absence of ref-counting removes that protection, so any topology where the capsule might race teardown with a concurrent call must use `SafeHandle`.
- The `IDisposable` pattern on capsules: the `Dispose(bool disposing)` override disposes managed children (including the inner `SafeHandle`) only when `disposing` is `true`; it releases unmanaged state unconditionally. The `IsInvalid` override on the inner `SafeHandle` must return `true` exactly when the raw handle value is the platform-defined invalid sentinel (0 or -1 depending on the family), not based on a separate boolean flag that can desync from the CLR's closed state.
- `Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid` and `SafeHandleMinusOneIsInvalid` implement `IsInvalid` for the two most common OS handle sentinel conventions: `IsInvalid = (handle == IntPtr.Zero || handle == (IntPtr)(-1))` for zero-or-minus-one, `IsInvalid = (handle == (IntPtr)(-1))` for minus-one-only. Subclassing these avoids the hand-rolled `IsInvalid` override entirely. POSIX file descriptors use minus-one-is-invalid; Windows object handles predominantly use zero-or-minus-one-is-invalid. For cross-platform capsules, `SafeHandleZeroOrMinusOneIsInvalid` is the more conservative base because it treats both sentinels as invalid. Neither helper base is in `System.Runtime.InteropServices`; both are in `Microsoft.Win32.SafeHandles`, available on all supported targets without platform-conditional requirements. A capsule that extends either helper base must not override `IsInvalid`: the helper's implementation is the contract. Overriding it with a supplementary flag introduces a silent native leak — `InternalRelease` gates `ReleaseHandle` on `_ownsHandle && !IsInvalid`; an `IsInvalid` override that returns `true` (treating a still-live handle as invalid) causes `InternalRelease` to skip `ReleaseHandle` entirely and the underlying OS resource leaks without any diagnostic. `SetHandleAsInvalid()` is the correct suppression path when the native resource has already been freed by an external route: it atomically ORs the closed bit into `_state` and calls `GC.SuppressFinalize`, so the next `InternalRelease` invocation skips `ReleaseHandle` and avoids double-free — the inverse of the override trap above.
- `GCHandle.Alloc(obj, GCHandleType.Pinned)` pins a managed object at a fixed address, preventing the GC from moving it. The capsule shape: a sealed class with a single `GCHandle _handle` field, `IDisposable` implemented as `if (_handle.IsAllocated) _handle.Free()`, and `AddrOfPinnedObject()` exposed only as a `nint` projection — never as the `GCHandle` itself. `GCHandleType.Normal` handles that are never freed produce managed memory leaks with no native-resource diagnostic surfacing them; they are invisible to `SafeHandle` finalization tooling. `GCHandle` cannot be the inner representation of a `SafeHandle` because it is a struct: passing it to P/Invoke does not give the CLR the lifetime hook that `SafeHandle` provides. A capsule that must provide both a pinned managed buffer and a native handle holds them as two separately lifecycle-managed fields.

```csharp
// Capsule: Borrowed/Owned cases, projection via bounded callback; handle never escapes
abstract class Capsule<H> : IDisposable where H : SafeHandle
{
    protected readonly H Handle;
    protected Capsule(H handle) => Handle = handle;

    // Borrowed: no-op Dispose — lifetime owned by caller
    public sealed class Borrowed(H handle) : Capsule<H>(handle)
    {
        public override void Dispose() { }
    }

    // Owned: disposes handle on Dispose
    public sealed class Owned(H handle) : Capsule<H>(handle)
    {
        public override void Dispose() => Handle.Dispose();
    }

    // Handle enters a bounded callback; caller receives a value, never the handle
    public Fin<T> Use<T>(Func<H, Fin<T>> project) =>
        Handle.IsInvalid || Handle.IsClosed
            ? Fin<T>.Fail(Error.New("handle invalid"))
            : project(Handle);

    public abstract void Dispose();
}
```

```csharp
// GCHandle capsule: pinned managed buffer; address projected as nint, handle never escapes
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

    public static PinnedBuffer Rent(int size) => new(new byte[size]);

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

[SAFEHANDLE_PROJECTION_DISCIPLINE]:
- `SafeHandle.DangerousGetHandle()` returns the raw `IntPtr` even when the handle has been marked invalid via `SetHandleAsInvalid`. Callers must guard with `DangerousAddRef(ref bool success)` + `DangerousRelease()` in a `try/finally`. `DangerousAddRef` never returns with `success = false` — a closed or invalidated handle throws `ObjectDisposedException` instead — so `success` stays `false` only when the call threw before the increment landed, and the `finally { if (success) ... }` guard is the gate for exactly that path. A capsule that omits the `success` guard and unconditionally calls `DangerousRelease` decrements a count that was never incremented, corrupting the ref-count and triggering a premature `ReleaseHandle`. The capsule's `Use<T>` entry-point `IsInvalid`/`IsClosed` pre-check is a fast-path optimization only; `DangerousAddRef`'s own closed check is the true gate. This pattern is required only when the raw handle must pass to a non-`[LibraryImport]` path; for `[LibraryImport]`-marshaled parameters, `SafeHandleMarshaller<T>.ManagedToUnmanagedIn.FromManaged` performs `DangerousAddRef` with the same guard and `Free` performs the matching `DangerousRelease`.

```csharp
bool success = false;
try
{
    handle.DangerousAddRef(ref success);   // closed handle throws ObjectDisposedException; success=true on return
    nint raw = handle.DangerousGetHandle();
    return NativeOp(raw);
}
finally
{
    if (success) handle.DangerousRelease();
}
```

- `SafeHandle.SetHandleAsInvalid()` marks the handle as no longer in use without triggering `ReleaseHandle`. Because `InternalRelease` gates `ReleaseHandle` on `!IsInvalid`, marking the handle invalid via `SetHandleAsInvalid` is the correct path when the native resource has already been freed externally and a subsequent `Dispose` or finalizer invocation must not double-free; `Dispose(true)` or the finalizer will still execute their bookkeeping path but `ReleaseHandle` is skipped.
- `SafeHandleMarshaller<T>` exposes three nested marshaller types covering all P/Invoke parameter directions. `ManagedToUnmanagedIn` handles `in` parameters and pass-by-value: `FromManaged(handle)` calls `DangerousAddRef`, `ToUnmanaged()` returns the raw `IntPtr`, and `Free()` calls `DangerousRelease` — the add-ref/release pair is managed internally and is not caller-visible. `ManagedToUnmanagedOut` handles `out IntPtr` parameters for factory-style P/Invokes: `ToManaged()` creates the `SafeHandle` subclass instance wrapping the raw pointer, performing the equivalent of `Marshal.InitHandle(new THandle(), rawIntPtr)` automatically — a `[LibraryImport]` method with `out T handle` where `T : SafeHandle` uses this marshaller implicitly and no manual `Marshal.InitHandle` call is needed. `ManagedToUnmanagedRef` handles `ref T handle` bidirectional parameters; `OnInvoked()` must be called after the native invocation and before `ToManagedFinally()` to finalize ref bookkeeping — an invocation interrupted between the P/Invoke and `OnInvoked` leaves the handle in an uninitialized managed wrapper whose `Free` disposes the partial state. The practical rule: use `[LibraryImport]` with typed `SafeHandle`-derived parameters for all new P/Invoke surfaces; the three-mode dispatch replaces the entire `[DllImport]` + `[MarshalAs(UnmanagedType.SysInt)]` + manual `DangerousAddRef`/`DangerousRelease` pattern.

```csharp
// LibraryImport with SafeHandle out-param: marshaller handles Marshal.InitHandle
[LibraryImport("native.dylib", EntryPoint = "create_context")]
private static partial void CreateContext(out ContextHandle handle);

// LibraryImport with SafeHandle in-param: DangerousAddRef/Release handled by marshaller
[LibraryImport("native.dylib", EntryPoint = "use_context")]
private static partial int UseContext(ContextHandle handle);

sealed class ContextHandle : SafeHandle
{
    public ContextHandle() : base(IntPtr.Zero, ownsHandle: true) { }
    public override bool IsInvalid => handle == IntPtr.Zero;
    protected override bool ReleaseHandle()
    {
        DestroyContext(handle);
        return true;
    }
    [LibraryImport("native.dylib", EntryPoint = "destroy_context")]
    private static partial void DestroyContext(IntPtr h);
}
```

- `Marshal.InitHandle(SafeHandle safeHandle, IntPtr handle)` calls `safeHandle.SetHandle(handle)` — `SetHandle` is a `protected` method on `SafeHandle`, inaccessible to marshallers or out-of-assembly code; `InitHandle` is the public surface that bridges into it. It is the correct path when constructing a `SafeHandle` outside a `[LibraryImport]` boundary where the subclass constructor must pass `IntPtr.Zero` to `base` (starting in the invalid state), then publish the real handle only after the P/Invoke succeeds: call `Marshal.InitHandle(this, actualHandle)` before exposing the instance. For `[LibraryImport]` with `out T handle where T : SafeHandle`, `SafeHandleMarshaller<T>.ManagedToUnmanagedOut.ToManaged()` calls the equivalent automatically and no manual `InitHandle` is needed.

[NATIVEMEMORY_CAPSULE_INTERNALS]:
- `NativeMemory.Alloc(byteCount)` and `NativeMemory.AlignedAlloc(byteCount, alignment)` allocate unmanaged heap memory via the platform CRT allocator. The capsule derives from `SafeHandle` with `invalidHandleValue: IntPtr.Zero`, overrides `IsInvalid` as `handle == IntPtr.Zero`, and calls `NativeMemory.Free(handle.ToPointer())` or `NativeMemory.AlignedFree(handle.ToPointer())` in `ReleaseHandle` respectively. `NativeMemory.Free` paired with `NativeMemory.AlignedAlloc` is undefined behavior in the underlying CRT; the correct free must match the allocation variant.
- `GC.AllocateUninitializedArray<T>(length, pinned: true)` allocates a pinned managed array in the pinned object heap (POH). POH arrays are permanently pinned without incurring GC pressure from scatter-gather movement. For high-frequency native I/O buffers that must be pinned, this avoids the `GCHandle.Alloc(Pinned)` + `GCHandle.Free()` lifecycle on each buffer operation. The tradeoff: POH allocations are never compacted, so fragmentation is permanent; use only for long-lived or fixed-size buffers.
- For security-sensitive native memory, prefer zero-on-release via `NativeMemory.Clear(ptr, size)` in `ReleaseHandle` before `NativeMemory.Free`, not zero-on-allocate — callers that observe initial zero state rely on an allocation-site invariant that has no enforcement point.
- `MemoryMarshal.CreateFromPinnedArray<T>(T[], int start, int length)` creates a `Memory<T>` backed by a POH array without allocating a `MemoryManager<T>`. The array must have been allocated via `GC.AllocateUninitializedArray<T>(length, pinned: true)`; passing a non-POH array produces a `Memory<T>` whose backing is not permanently pinned, and the GC may move the array between `Memory<T>.Span` being taken and its use in a P/Invoke — the method does not validate that the array is pinned. The distinction between `GCHandle.Alloc(Pinned)` and POH: a `GCHandle` pin holds the array at a fixed address for the lifetime of the handle (not merely "temporarily"), but the address is valid only while the handle exists and `GCHandle.Free()` has not been called; the POH pins permanently without a handle lifecycle, so `MemoryMarshal.CreateFromPinnedArray` is the correct pair for POH-allocated arrays, while `GCHandle.Alloc(Pinned)` + `GCHandle.AddrOfPinnedObject()` is correct for handle-scoped pinning — the two are distinct lifetime shapes, not equivalent. Every `GC.AllocateUninitializedArray(..., pinned: true)` that is never freed produces a permanent hole in the POH; the owning capsule's `Dispose` sets the backing field to `null` to allow GC collection of the POH slot, and the `CreateFromPinnedArray` call must be guarded by the capsule's disposal state before dereferencing.

```csharp
// NativeMemory capsule: aligned allocation, zero-on-release, span projection
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
        NativeMemory.Clear(handle.ToPointer(), _byteCount);
        NativeMemory.AlignedFree(handle.ToPointer());
        return true;
    }

    // Projection via bounded callback — Span<byte> is a live pointer into native memory and
    // must not escape the capsule; callers receive T, never the span itself.
    public Fin<T> Use<T>(Func<Span<byte>, T> project)
    {
        if (IsInvalid || IsClosed)
            return Fin<T>.Fail(Error.New("handle invalid"));
        unsafe { return project(new Span<byte>(handle.ToPointer(), (int)_byteCount)); }
    }
}
```

- `Memory<T>.Pin()` returns a `MemoryHandle`, an `IDisposable` struct. `MemoryHandle.Pointer` returns a `void*` to the start of the pinned memory. Disposing the `MemoryHandle` unpins the memory — either releasing the `GCHandle` for heap-allocated arrays or calling `IPinnable.Unpin()` for `MemoryManager<T>`-backed memory. A `MemoryHandle` not disposed keeps backing memory pinned indefinitely: for ordinary heap arrays this blocks GC compaction; for `MemoryManager<T>`-backed memory the behavior is implementation-defined. The correct model is a two-phase capsule: `Memory<T>` is the logical extent, `MemoryHandle` is the physical address lock. The physical address is only valid inside the `MemoryHandle`'s scope; any code that retains the `void*` past `MemoryHandle.Dispose` dereferences a dangling pointer.

```csharp
// MemoryHandle two-phase capsule: logical extent vs physical address lock
unsafe delegate Fin<TResult> NativeOp<TResult>(void* ptr, int length);

static Fin<TResult> WithPinnedAddress<T, TResult>(
    Memory<T>             memory,
    NativeOp<TResult>     nativeOp)
{
    using var handle = memory.Pin();            // physical address lock
    unsafe
    {
        return nativeOp(handle.Pointer, memory.Length);
    }
}   // handle.Dispose() here — address lock released, Pointer invalid past this point
```

- `MemoryManager<T>` is the extensibility point for `Memory<T>` with custom backing stores. The abstract methods `Pin(int elementIndex)` and `Unpin()` are the physical-address lock pair: `Pin` must increment an internal pin count and return a `MemoryHandle` that calls `Unpin` on disposal; `Unpin` decrements the count and releases the physical lock when the count reaches zero. The implementation must be thread-safe: multiple concurrent callers may `Pin` the same `MemoryManager` at overlapping element indices. `MemoryManager<T>` implements `IDisposable` via explicit interface (`System.IDisposable.Dispose()`): `using` statements bind correctly through the interface and call `Dispose` on scope exit regardless, but a direct `.Dispose()` call on a variable typed as the concrete subclass does not compile — the caller must cast to `IDisposable` explicitly or rely on `using`. Disposing a `MemoryManager` while outstanding `MemoryHandle` instances exist (pin count non-zero) is undefined behavior at the native memory level; dispose only after all derived handles have been disposed. `MemoryMarshal.TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T>, out TManager, out int start, out int length)` extracts the underlying `MemoryManager<T>` from a `Memory<T>` slice — the correct recovery path when a component receives a `Memory<T>` and needs to participate in the pin-count lifecycle by calling `Pin`/`Unpin` directly.

[BRACKET_ACQUISITION]:
- The `IO<A>` monad tracks acquired resources inside `EnvIO`'s resource registry. `use(acquire)` registers an `IDisposable`-bearing value; `use(acquire, release)` registers with a custom `IO<Unit>` or `Action<A>` finalizer for non-`IDisposable` handles. Both forms register under the current `EnvIO` scope and remain live until `bracketIO` closes the scope.
- `bracketIO(computation)` creates a local resource environment: any resource registered with `use` inside `computation` is released when `computation` completes — whether by success, failure, or cancellation. This is the primary bracket form. The three-argument form `bracketIO(acq, use, fin)` is the explicit acquire-use-release triple; it is less preferred because the `bracketIO(computation)` form composes without threading the resource through every continuation manually.
- `IO<A>.Bracket` and `IO<A>.BracketFail` are instance-level scope operators: `Bracket` releases resources on both success and failure; `BracketFail` releases only on failure, leaving resources live on success for further composition. The multi-argument instance forms `Bracket<TUse, TFin>(Func<A, IO<TUse>>, Func<A, IO<TFin>>)` and `Bracket<TUse, TFin>(Func<A, IO<TUse>>, Func<Error, IO<TUse>>, Func<A, IO<TFin>>)` supply explicit use and finalize delegates. None of these forms interact with outer `bracketIO` scopes — they are operation-local.
- `release<A>(value)` runs the registered finalizer immediately and removes the registration from the `EnvIO` scope. The value must have been registered by a prior `use` call in the same scope; calling `release` on an unregistered value is a no-op.
- `useMaybe<M,A>(K<M,A>)` acquires when the monad produces a value and tracks it; if the monad produces nothing (`None` from `OptionT`), no registration is made. This is the correct form for optional resources where absence is typed, not exceptional.
- `useAsync<A>(IO<A>)` and `useAsync<M,A>(K<M,A>)` wrap `IAsyncDisposable` resources; the IO environment calls `DisposeAsync` on scope exit. These are not fallback for `IDisposable` — they are the only correct registration form for resources whose disposal is inherently asynchronous (e.g., a connection whose flush requires an async drain).
- `IO<A>.WithEnv(Func<EnvIO, EnvIO>)` maps the `EnvIO` threading through the computation into a new local environment. Resources acquired inside the mapped computation are tracked in the new context and released when the computation completes — whether by success, failure, or cancellation — without affecting the outer computation's resource registry. `IO<A>.WithEnvFail(Func<EnvIO, EnvIO>)` applies the same scoped environment but releases resources only on failure; on success, resources acquired in the sub-computation survive into the outer scope and participate in the outer environment's cleanup. `WithEnvFail` is the correct form for acquire-on-success patterns where a resource must outlive the acquiring computation on success but must be cleaned up if the computation fails mid-way. Both `WithEnv` and `WithEnvFail` are `protected` instance methods on `IO<A>`; they are accessible only inside a subclass of `IO<A>`, not from standalone static functions. `localIO<M,A>(K<M,A>)` creates a local cancellation environment backed by `EnvIO.LocalCancel` — a fresh linked `CancellationToken` with the outer resource registry unchanged — where `IO.cancel` fired inside the local scope raises an exception that propagates outward without cancelling the outer token. `localIO` scopes only the cancellation token; the resource registry is shared with the outer `EnvIO` (resources registered with `use` inside `localIO` are tracked in the outer scope and released when that outer scope exits, not at `localIO` scope exit). `IO<A>.Local()` is not that form: it derives `EnvIO.New(env.Resources, env.Token, null, env.SyncContext)` — the resource registry and token are shared and only the cancellation-source slot is fresh; resource isolation comes from the bracketed acquire-use-release forms, never from `Local()`.

```csharp
// WithEnvFail is protected on IO<A>; the acquire-on-success pattern is expressed via
// bracketIO with BracketFail — release on failure, survive on success.
// IO.lift(Func<A>) is the sync lift; liftIO only accepts Task-based delegates.
static IO<IConn> AcquireConnection(ConnSpec spec) =>
    bracketIO(
        from conn in use(IO.lift<IConn>(() => Connection.Open(spec)),
                         c => IO.lift(() => { c.Dispose(); return unit; }))
        select conn
    ).BracketFail();   // outer BracketFail: resource released on failure, survives on success

// localIO: isolated cancel scope — outer token unaffected by inner IO.cancel
// Resources registered with use inside localIO are tracked in the OUTER scope.
static IO<TResult> IsolatedCancel<TResult>(IO<TResult> work) =>
    localIO(work);   // cancellation isolated; resources tracked in outer scope

// IO<A>.Local(): fresh cancellation-source slot ONLY — Resources and Token are shared with the outer env.
// Resource isolation comes from the bracketed acquire-use-release forms, not from Local().
static IO<TResult> FreshCancelSource<TResult>(IO<TResult> work) =>
    work.Local();    // cancellation-source slot refreshed; registry and token shared
```

- `IO<A>.Timeout(TimeSpan)` applies a wall-clock limit whose cancellation fires the `EnvIO`'s `CancellationToken`; all resources registered with `use` inside the timed computation are released by the `EnvIO` scope exit just as on normal or failure exit — a `bracketIO` computation wrapped with `.Timeout(...)` composes safely. Timeout uses cooperative cancellation: a computation blocked on a non-cancellable synchronous call will not be interrupted, and the timeout fires while the computation thread continues executing until the blocking call returns; any native call inside a `bracketIO` used with `.Timeout` must either accept a `CancellationToken` or be wrapped in a fork so the timeout can abandon the blocking thread while resource cleanup occurs in the fork's own scope. `MonadUnliftIO<M>.TimeoutIO<A>(K<M, A>, TimeSpan)` exposes the same guarantee at the trait level, allowing timeout to compose across `Eff`, `IO`, and any custom effect monad implementing `MonadUnliftIO`; calling `IO<A>.Timeout` directly requires downcasting from the monad.
- `IO<A>.Uninterruptible` (and the `uninterruptible<M,A>` prelude form) wraps a computation in a local environment where cancellation-token requests are ignored. Resource-release behavior is unchanged: any resource registered with `use` inside the uninterruptible scope is still released when that scope exits, whether the computation completes normally or faults. Masking cancellation means the computation cannot be interrupted at a cancellation checkpoint; it does not mean resources are held indefinitely past the scope boundary. The correct scope for `uninterruptible` is a short atomic native operation — a P/Invoke or handle-release call that must not be interrupted between acquire and the matching release. Wrapping a long-running `bracketIO` in `uninterruptible` defeats the drain-order cancellation window; mark only the acquire step or the release step as uninterruptible, keeping the use body interruptible. A per-iteration cancel-and-retry pattern pairs `localIO` (isolated cancel scope, per the `WithEnv` bullet above) around each attempt with `uninterruptible` on the acquire step only; the local scope disposes per-iteration resources while the outer drain loop remains live.

```csharp
// uninterruptible: wrap only the critical acquire step; body remains interruptible
// IO.lift(Func<A>) is the sync lift; factory returns IResource (not IDisposable here)
static IO<IResource> SafeAcquire(Func<IResource> factory) =>
    uninterruptible(IO.lift<IResource>(factory));

static IO<Result> UseResource(Func<IResource> factory, IO<Result> body) =>
    bracketIO(
        from r in use(SafeAcquire(factory), _ => IO.lift<Unit>(() => unit))  // custom release for non-IDisposable
        from v in body
        select v
    );
```

- The four-argument form `bracketIO(acq, use, catch, fin)` adds a `Catch` parameter — `Func<Error, IO<TResult>>` — that is invoked when the `acq` computation raises an error. The `fin` (finalizer) runs unconditionally regardless of whether acquisition succeeded, failed, or the catch handler ran. This is the correct form when acquisition itself can partially allocate resources: the catch handler recovers from the partial failure and the finalizer cleans up whatever was registered before the failure. The two-argument form `bracketIO(acq, use, fin)` has no acquisition-error handler: an error in `acq` propagates immediately after `fin` runs. The `bracketIO(computation)` single-argument form — where all acquisition and use are expressed inside the monadic expression — subsumes both for new code; the multi-argument forms remain correct when acquisition and finalization are logically separate from the use body.
- `IO<A>.Post` and `MonadUnliftIO.PostIO` schedule the computation to run on the `SynchronizationContext` captured in the `EnvIO` at the start of the IO chain. The resource registry is part of `EnvIO`, not part of the synchronization context; resources registered with `use` before `.Post` are tracked in the same `EnvIO` threaded through after the context switch — the context switch does not create a new resource scope and does not reset the registry. Do not call `.Post` inside a `bracketIO` computation when the continuation after `.Post` acquires resources that must be released before the outer `bracketIO` exits: those resources register in the outer `bracketIO`'s `EnvIO` and are released at the outer bracket's exit, not at the `.Post` continuation's exit. If post-synchronization-context resources must have a shorter lifetime, wrap the `.Post` call in an inner `bracketIO` that scopes the acquisition.

[FORK_RESOURCE_ISOLATION]:
- `IO<A>.Fork(Option<TimeSpan>)` creates a child execution on a thread-pool thread with a fresh, isolated `EnvIO` resource registry. Resources acquired inside the fork are tracked against that child registry and released when the forked computation ends — whether by success, failure, or timeout. Resources acquired in the parent before the fork are accessible to the forked computation but are shared: a forked computation that releases a parent-acquired resource removes it from the parent's registry, which can yield use-after-release in the parent or any sibling fork holding the same resource. Resources that must outlive the fork are acquired in the parent and passed as values — projections, not handles — into the forked computation; resources whose lifetime is bounded by the fork are acquired inside the fork.
- The `Fork` return value is a `ForkIO<A>` record with `Await` (`IO<A>`) and `Cancel` (`IO<Unit>`). `Await` re-raises any error from the forked computation into the awaiting monad; `Cancel` signals the fork's linked `CancellationToken`. Neither field disposes resources; disposal happens automatically on fork exit regardless of whether `Await` or `Cancel` was called. A pattern that awaits the fork to then dispose its resources in the parent breaks the isolation: the resources are already released by the time `Await` returns.
- `awaitAny` settles on the first completion, success or fault — the implementation is `Task.WhenAny` and rethrows a first-completed fault; its XML comment promises error accumulation the body does not perform. Over pre-launched `ForkIO` handles it cancels only its own local await scope, never the forks themselves — losers run to completion and their lifetimes must be bounded explicitly. For competitive acquisition patterns (acquiring from multiple candidate endpoints, returning whichever connects first), guard each candidate so a fast terminal failure cannot defeat a slower success before reaching for `awaitAny`. The critical constraint: every fork's resources are released when that fork exits — including the winner's. The returned value is a projected copy (a value), not a live resource handle from the fork's scope. Acquiring a session inside a fork and projecting a handle out of `awaitAny` returns a handle to an already-disposed resource; the connection must be represented as a projection (an identifier, a connection-spec token, a pre-extracted value) that lets the outer scope re-acquire or reconstruct it. `awaitAll(ForkIO<A>[])` awaits all forks and returns a `Seq<A>`; it does not cancel any fork on sibling failure, continues awaiting the rest, and releases resources in each fork independently on that fork's completion.

```csharp
// Competitive endpoint probe: first responding spec wins; session opened in outer scope
// IO.lift(Func<A>) is the sync lift; liftIO accepts only Task-based delegates.
// Fork resources are disposed when each fork exits; awaitAny returns a projected value (the spec),
// never a live session handle. The session is opened in the outer bracketIO scope.
static IO<ISession> FirstAvailable(Seq<ConnSpec> specs) =>
    bracketIO(
        from winningSpec in awaitAny(specs.Map(spec =>
            bracketIO(
                from _ in IO.lift<Unit>(() => { spec.Probe(); return unit; })
                select spec
            ).Fork(Some(TimeSpan.FromSeconds(5)))))
        from session in use(IO.lift<ISession>(() => Session.Connect(winningSpec)),
                            s => IO.lift<Unit>(() => { s.Dispose(); return unit; }))
        select session
    );
```

[RETRY_RESOURCE_RELEASE_GUARANTEE]:
- `retry`, `retryWhile`, `retryUntil`, and their `Schedule`-driven variants on `IO<A>` release any resources acquired in a failing iteration before the next iteration begins. On success, resources acquired in the succeeding iteration are tracked in the surrounding `EnvIO` scope. A `bracketIO(retry(...))` composes safely: the outer bracket releases only the surviving successful resources, not the already-released failed-iteration resources.
- The CAS spin-retry inside `Atom<A>.SwapMaybe` and the `retry` operator are distinct retry levels. `SwapMaybe`'s lambda may be called multiple times during CAS spin; a lambda that acquires a resource and registers it with `use` inside the CAS spin would produce duplicate registrations — only the final swap-winner's registration survives, but prior registrations could already point to disposed resources. The safe form: perform the CAS to confirm the swap, then acquire and register the resource in a subsequent `IO` step after the atom has settled.
- `IO<A>.Repeat`, `IO<A>.Repeat(Schedule)`, `IO<A>.RepeatWhile`, and `IO<A>.RepeatUntil` (instance forms) and the trait-level `RepeatIO`/`RepeatWhileIO`/`RepeatUntilIO` on `MonadUnliftIO` release all resources acquired in each iteration before the next iteration begins — including on a successful iteration. Resources cannot be acquired inside a repeated computation and returned from it; the acquisition is released at the iteration boundary even when the iteration succeeds. Resources that must persist across iterations must be acquired outside the repeat scope and passed as values into the repeated body. For polling capsules using `IO<A>.Repeat(Schedule.spaced(interval))`: acquire the connection in a surrounding `bracketIO` outside the `Repeat` call, or accept that each iteration incurs full connection setup and teardown.

```csharp
// Connection acquired outside repeat — survives across all poll iterations
static IO<Unit> PollForever(Func<IO<IConn>> connFactory, IO<Unit> body) =>
    bracketIO(
        from conn in use(connFactory())
        from _    in body.Repeat()   // instance Repeat; body closes over conn by value
        select unit
    );
```

[PERIODICTIMER_CAPSULE_LIFETIME]:
- `PeriodicTimer(TimeSpan, TimeProvider)` accepts an injected `TimeProvider`, making it the testable clock-driven capsule for polling loops. The timer is `IDisposable`; calling `Dispose` signals the timer to stop: the current `WaitForNextTickAsync` call returns `false` (not an `OperationCanceledException`), and all subsequent calls return `false` immediately. A polling loop that checks the return value of `WaitForNextTickAsync` instead of catching `OperationCanceledException` produces the correct drain-time termination behavior.
- The cancellation behavior is asymmetric: passing a `CancellationToken` to `WaitForNextTickAsync` and cancelling it throws `OperationCanceledException` and affects only that single wait, while the underlying timer continues firing. `Dispose` stops the timer entirely and causes all future and current waits to return `false` cleanly. `Dispose` is the shutdown mechanism; `CancellationToken` is the per-wait interrupt. A design that calls `Dispose` on the timer via the cancellation callback conflates the two and suppresses future `false`-return detection if the timer is in a pool or reused.

```csharp
// PeriodicTimer capsule: TimeProvider-injected, drain via Dispose → false return
sealed class PollingCapsule : IAsyncDisposable
{
    readonly PeriodicTimer _timer;

    PollingCapsule(TimeSpan interval, TimeProvider clock) =>
        _timer = new PeriodicTimer(interval, clock);

    public static PollingCapsule Create(TimeSpan interval, TimeProvider clock) =>
        new(interval, clock);

    public IO<Unit> RunAsync(IO<Unit> body) =>
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

[TWO_PHASE_PROBE_THEN_CONSTRUCT]:
- Probe and construction must be atomic from the caller's perspective: a probe that succeeds followed by construction must be a single indivisible acquire from the calling code's view. A gap between probe and construction where another caller can observe "probed but not yet constructed" is a protocol violation that yields stale-probe authorization after the underlying resource has changed.
- For idempotent construction (constructing the same value multiple times is safe and all but one can be discarded), `Atom<Option<TSession>>` with `SwapMaybe(current => current.IsSome ? current : Some(construct()))` is atomic in one CAS — probe and construction in one lambda. For non-idempotent construction, the sentinel pattern (see snippet) separates the ownership claim from the construction: a side-effect-free `SwapMaybe` CAS on a `Guid` sentinel claims exclusive ownership, construction happens exactly once outside the CAS on the winner's path, and the result is published via a second atomic swap.

```csharp
// Atomic probe-then-construct: sentinel CAS acquires exclusive lane; construct happens outside CAS
static int                            _acquired = 0;
static readonly Atom<Option<Session>> _session  = Atom<Option<Session>>(None);

static Fin<Session> AcquireOnce(SessionConfig cfg)
{
    // Step 1: CAS sentinel — side-effect-free, at-most-one winner
    if (Interlocked.CompareExchange(ref _acquired, 1, 0) != 0)
        return Fin<Session>.Fail(Error.New("session already acquired"));

    // Step 2: won the gate — construct outside CAS, then publish
    var session = Session.Open(cfg);
    _session.Swap(_ => Some(session));
    return Fin<Session>.Succ(session);
}
```

- `Atom<A>.SwapMaybe(Func<A, Option<A>>)` may call its function multiple times during CAS retry. The lambda must be side-effect-free — construction of a non-idempotent resource inside the lambda violates the contract because only one constructed value becomes canonical while prior invocations' allocations leak. Correct shape: claim exclusive ownership with a side-effect-free CAS first, then construct exactly once outside the CAS, then publish the result.
- For an acquire-once gate, `Interlocked.CompareExchange(ref int, 1, 0)` is the idiomatic side-effect-free sentinel: exactly one caller sees the exchange return 0 (the winner), all others see 1. The winner constructs and publishes; losers return a typed failure. When the gate must be releasable (retry after failure), replace `int` with `Atom<bool>` and reset via `Atom.Swap(_ => false)` on failure — the swap lambda is side-effect-free and the construction remains outside the CAS.

[REGISTRY_OWNED_SESSION_DISPOSAL]:
- The registry that constructs a session owns its disposal. Call sites receive a value projected from the session, never the session reference itself, and never call `Dispose` or `Close` directly. A call site that calls `Dispose` on a registry-managed session breaks the registry's invariant and may dispose a session concurrently held by another call site.
- Cache keys must be structurally complete: content hash plus full configuration. Partial keys produce stale-hit false positives — the cached session's full configuration differs from the request's, producing silent behavioral divergence not visible until an excluded field changes.
- The registry uses `AtomHashMap<CacheKey, Session>`. Disposal of a removed session must happen outside the atom's swap lambda because the lambda must be side-effect-free. Remove from the atom atomically, capture the removed value, then dispose it after the atom update completes.

```csharp
// Registry-owned disposal: SwapKey for get-or-create; caller receives projected value, not session
sealed class SessionRegistry : IDisposable
{
    readonly AtomHashMap<CacheKey, ISession> _sessions = AtomHashMap<CacheKey, ISession>();

    // Callers receive a projected value, never the session reference itself
    public TValue Use<TValue>(CacheKey key, Func<ISession> factory, Func<ISession, TValue> project)
    {
        // SwapKey: None→Some creates; Some→Some is identity (no-op update)
        _sessions.SwapKey(key, current => current.IsSome ? current : Some(factory()));
        return _sessions.Find(key).Map(project)
                        .IfNone(() => throw new InvalidOperationException("session absent after acquire"));
    }

    public Unit Evict(CacheKey key)
    {
        // Find before removal; SwapKey lambda must be side-effect-free
        var snapshot = _sessions.Find(key);
        _sessions.SwapKey(key, _ => None);
        return snapshot.IfSome(s => s.Dispose());
    }

    public void Dispose() =>
        _sessions.ToSeq()
                 .Iter(pair => pair.Value.Dispose());
}
```

- `AtomHashMap<K, V>.SwapKey(K, Func<Option<V>, Option<V>>)` is a four-case atomic dispatch: `Some(x) → Some(y)` performs `SetItem(key, y)`; `Some(x) → None` performs `Remove(key)`; `None → Some(y)` performs `Add(key, y)`; `None → None` is a no-op. The swap lambda may be called multiple times under CAS contention and must be side-effect-free. This single operation covers create-or-update, conditional remove, and no-op existence checks — replacing any combination of separate `Add`/`Remove`/`Find` calls that would have a read-mutate gap. The registry evict pattern uses `Find` before `SwapKey` to snapshot the outgoing value outside the lambda, then `SwapKey(key, _ => None)` atomically removes with a side-effect-free lambda, then disposes the snapshotted value after the swap — satisfying both the side-effect-free lambda constraint and the dispose-outside-atom law. `AtomHashMap<K, V>.Remove(K)` is unconditional atomic removal with no capture of the previous value; use `SwapKey` when the removed value must be observed or disposed, `Remove` when the prior value is irrelevant.
- Ephemeral per-call sessions are forbidden. A call site that constructs a session, uses it, and disposes it inline defeats caching, pays initialization cost on every call, and risks concurrent initializations racing toward the same underlying resource. All session management routes through the registry.

[OPERATION_SCOPED_DATA_CONTEXT]:
- Data-access contexts (connections, transactions, unit-of-work scopes) are acquired at the start of the operation that needs them, disposed at the operation boundary, and never captured across `await` continuations or stored as fields. The `await` point is a suspension that can resume on a different thread; a context captured before `await` and accessed after may violate thread-affinity or connection-state invariants of the underlying provider.
- Ambient-context patterns (a static `AsyncLocal<TContext>` or a singleton injected into a service) make context lifetime invisible: any code in the call tree may consume it without a visible dependency, and the context may be disposed before consumers in forked work items observe it. The explicit alternative: inject `Func<IO<TContext>>` as a factory and call it at the top of each operation.
- The operation-scope model maps directly to `bracketIO`: acquire the context inside the computation, register it with `use`, and let `bracketIO` dispose it on exit.

```csharp
// Operation-scoped context via bracketIO — never ambient
static IO<Result> QueryItems(QuerySpec spec, Func<IO<IContext>> contextFactory) =>
    bracketIO(
        from ctx  in use(contextFactory())
        from rows in liftIO(() => ctx.Execute(spec))
        select rows.Map(Row.Project)
    );
```

[POOLED_BUFFER_DISCIPLINE]:
- `ArrayPool<T>.Shared.Rent(minimumLength)` returns an array that is at least `minimumLength` long; the actual length may be larger. Callers must track the logical extent of data separately — typically as a `Memory<T>` slice over the rented array — and never pass the full rented array to downstream consumers because the trailing bytes past the logical extent contain stale data.
- `ArrayPool<T>.Shared.Return(array, clearArray: false)` is the preferred form for non-sensitive data; `clearArray: true` zeros the buffer before pooling. For credential or keying material, `clearArray: true` is required.
- `MemoryPool<T>.Rent(minBufferSize)` returns `IMemoryOwner<T>`, which carries `IDisposable`; callers must dispose the owner to return the memory to the pool. The `Memory<T>` slice `owner.Memory` is the usable surface; the owner reference itself must not be shared across the lifetime seam.
- The pool vocabulary (`ArrayPool`, `MemoryPool`, `IMemoryOwner`, rented array sizes) must not appear on any public method signature. Public signatures express logical extent through `ReadOnlySpan<T>`, `ReadOnlyMemory<T>`, or a domain value type that internally slices into a rented buffer. Callers that see `IMemoryOwner<T>` in a return type become coupled to return timing and cannot be composed freely.
- Buffers return at phase boundaries, not at the end of processing. A pipeline stage that rents, processes, and then emits into a channel must return its input buffer before the write to the next stage, not after. Retaining a buffer across a phase transition risks holding it past the next renter's fill.
- `PipeWriter.GetMemory(sizeHint)` returns a `Memory<byte>` backed by a buffer segment managed by the `Pipe`'s `MemoryPool<byte>`. The caller writes into this memory and then calls `PipeWriter.Advance(int bytesWritten)` to commit exactly the bytes written — not the full segment size. Calling `Advance` with a value larger than the actual bytes written produces undefined behavior: the reader will see garbage bytes in the overcommitted region. The `PipeWriter` does not validate that `bytesWritten <= GetMemory(...).Length`; the invariant is caller-enforced.
- After `PipeReader.ReadAsync`, the returned `ReadResult.Buffer` is a `ReadOnlySequence<byte>` spanning one or more segments from the pipe's pool. The caller must call `PipeReader.AdvanceTo(consumed, examined)` before calling `ReadAsync` again. Setting `examined` further than `consumed` signals the pipe to wait for more data before returning a new result even if unconsumed data remains — this is the back-pressure signal for partial-parse scenarios.
- `PipeReader.Complete(Exception?)` signals the writer that the reader is done. After `Complete`, any buffered data in the pipe is released back to the `MemoryPool`. The correct producer-consumer teardown order is: writer calls `PipeWriter.Complete()` first, then the reader drains all remaining buffered data, then the reader calls `PipeReader.Complete()`. Reversing the order — reader completes before all buffered data is consumed — discards unconsumed segments immediately on `Complete`, so any subsequent `ReadAsync` sees an empty, completed pipe and the producer's written data is lost.
- `Pipe.Reset()` returns a `Pipe` to its initial state, releasing all internal buffer segments back to the `MemoryPool<byte>`. It throws `InvalidOperationException` if either the `PipeReader` or `PipeWriter` is not yet in a completed state — the caller must complete both before calling `Reset`. The correct pipe-reuse sequence is: call `PipeWriter.Complete()`, drain the reader until `ReadResult.IsCompleted`, call `PipeReader.Complete()`, then `Reset`. `PipeOptions.PauseWriterThreshold` and `PipeOptions.ResumeWriterThreshold` control back-pressure; the default `PipeOptions` constructor sentinel `-1` resolves to 65536 (pause) and 32768 (resume) — explicitly passing `0` for `pauseWriterThreshold` disables back-pressure and allows unbounded growth if a reader stalls. `PipeOptions.UseSynchronizationContext` (the actual member name) controls whether asynchronous callbacks and continuations execute on the `SynchronizationContext` captured at the point the pipe was created; when `true`, `WriteAsync` and `FlushAsync` continuations are dispatched to the captured context — this takes precedence over `ReaderScheduler`/`WriterScheduler` and must not be set to `true` for capsules that hold a `Lock` across pipe-read continuations, as it routes continuations through the captured context rather than inlining them on the writer's thread.

[OBJECTPOOL_POLICY_AND_RETURN_CONTRACT]:
- `ObjectPool<T>.Return(obj)` does not automatically reset `obj` to a clean state. If the policy type implements `IResettable`, `DefaultObjectPool<T>` calls `TryReset()` before pooling; if `TryReset()` returns `false`, the object is discarded rather than pooled. A pooled mutable object whose state bleeds across rentals creates a silent correctness bug: the next `Get()` caller receives stale accumulated state. Every pooled object either implements `IResettable.TryReset()` returning `true` after full reset, or holds no state that matters across rental boundaries.
- `LeakTrackingObjectPool<T>` wraps any `ObjectPool<T>` and attaches a stack-trace `WeakReference` to each rented object via a `ConditionalWeakTable`. In debug builds, finalization of a leased object that was not returned triggers an error. It is a diagnostic wrapper, not a production surface; it adds `ConditionalWeakTable` allocation overhead per rental in all build configurations without providing diagnostic benefit outside `Debug`. The correct integration: reference `LeakTrackingObjectPool<T>` only through an `ObjectPool<T>` interface and swap it via `ObjectPoolProvider` in `Debug` builds only.
- `IPooledObjectPolicy<T>.Return(T obj)` returning `false` discards the object; `DefaultObjectPool<T>` drops it without calling `Dispose`. Objects with `IDisposable` state returned to a pool that calls a policy returning `false` must be disposed by the policy before returning `false` — every rejected rental that does not dispose in-band leaks. `DefaultObjectPool<T>` with a type implementing `IResettable` calls `TryReset()` inside `ReturnCore`; `TryReset()` returning `false` is treated identically to a policy rejection — the object is discarded, not pooled, and not disposed by the pool — so an `IResettable.TryReset()` implementation that encounters a degraded internal state must dispose in-band before returning `false`.

[RATELIMITLEASE_AS_CAPSULE]:
- `RateLimiter.AttemptAcquire(int)` returns a `RateLimitLease` synchronously. `RateLimitLease.IsAcquired` distinguishes a successful from a failed lease; both forms are `IDisposable` and must be disposed. Disposing a failed lease is a no-op for most implementations but must not be skipped: a custom limiter that returns a stateful failed lease (e.g., one that records denial metrics on disposal) would silently lose that record if callers guard disposal on `IsAcquired`. Every `RateLimitLease`, success or failure, is disposed exactly once.
- `RateLimiter.AcquireAsync` with cancellation: if the `CancellationToken` fires while a request is queued, the `ValueTask<RateLimitLease>` throws `OperationCanceledException` — no lease is returned, so there is nothing to dispose for the cancelled call itself. The caller must not attempt to dispose a lease it never received. A `try`/`finally`-style pattern that calls `Dispose` on the lease in the `finally` block must be guarded against the OCE path where the local lease variable is still null.
- `PartitionedRateLimiter<TResource>.CreateChained(limiters)` acquires from all inner limiters in order; if any inner limiter fails, the successful leases from prior limiters are released before returning the failed outer lease. This cascading release happens inside the implementation. The caller still disposes the returned outer lease; the outer lease's `Dispose` is a no-op for already-released inner leases and completes any remaining bookkeeping.

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

[MAINTENANCE_CONNECTION_QUARANTINE]:
- Connections opened for maintenance operations (backup, integrity check, compaction, schema migration) acquire exclusively within the lifecycle state that permits them. The lifecycle state is an `Atom<LifecycleState>` (a `[SmartEnum]`-typed cell). Transition from `Running` to `Maintenance` uses `SwapMaybe(s => s == Running ? Some(Maintenance) : None)` — the lambda is side-effect-free; transition to the new state returns `Some(Maintenance)`, while any other state returns `None` causing no update. The `SwapMaybe` return value is the atom's settled value — `Running` if the gate was closed, `Maintenance` if the transition succeeded — which the caller inspects to decide whether to open a connection.
- The construction pattern: after `SwapMaybe` returns `Maintenance`, open the connection and wrap it in a `MaintenanceToken` capsule that holds the connection and a `CancellationToken` linked to the external lifecycle source. No maintenance connection persists across a lifecycle transition; the transition itself is the signal to abort.
- If a maintenance connection is interrupted mid-way through a sequence of configuration steps, the partial application state is undefined and the connection is quarantined: disposed without further writes via `IDisposable.Dispose()` immediately (not deferred), never returned to any pool, and the operation emits a typed failure receipt carrying the step index at which interruption occurred. The capsule's `Dispose` sets the atom back to `Running` only after the connection is closed, ensuring the next acquirer sees a clean state or does not acquire at all if the transition was to a non-recoverable state.
- `CancellationToken.Register(Action<object?>, object?)` on the lifecycle token fires the quarantine path synchronously from `CancelAsync()` on the lifecycle source; the callback closes the connection and emits the failure receipt. Retry policy lives at the lifecycle state machine level — the capsule does not re-enter the maintenance state after quarantine.

[CHILD_BEFORE_CONTAINER_DISPOSAL]:
- Child surfaces close and emit completion before the container that holds them begins disposal. Inversion — disposing the container while children still hold references to container resources — corrupts handles still alive in the child and produces access violations or use-after-free in native handle teardown.
- The canonical ordering in a container capsule: (1) signal children to stop accepting new work; (2) await child drain completions; (3) dispose child handles; (4) dispose container handle. Steps 3 and 4 are sequential, not concurrent.
- `System.Threading.Lock.EnterScope()` returns a `Scope` ref-struct that exits the lock on `Dispose`. For disposal ordering critical sections, `Lock` + `EnterScope` is the correct mechanism rather than `Monitor` or the `lock` keyword, because `Scope` is a ref-struct whose `Dispose` cannot be deferred across `await`.
- `MemoryMappedFile.CreateViewAccessor` and `CreateViewStream` produce child views over the same mapped region; each view holds an internal `SafeMemoryMappedViewHandle`. The BCL enforces no child-before-parent disposal ordering: `MemoryMappedFile.Dispose` marks its internal handle as invalid but does not force all views to flush or close — views that outlive the file object retain access to the mapped memory through their own `SafeMemoryMappedViewHandle`. The capsule law: flush and dispose all views first, then dispose the file. Reversing the order produces no immediate error (the OS reference count keeps the mapping live until all handles close), but violates child-before-container ordering and can cause silent data loss on writeable views that are flushed after the container is marked disposed.

[LOCK_SCOPE_AS_DISPOSABLE]:
- `System.Threading.Lock` is the typed replacement for `object` + `lock` keyword. Its `EnterScope()` method returns `Lock.Scope`, a `ref struct` with `Dispose()` that exits the lock. The `ref struct` constraint prevents boxing, field storage, or capture across `await` — which is the correctness constraint: lock scopes must not span asynchronous suspensions.
- A disposal-ordering guard that must hold a lock across synchronous disposal of children uses `Lock.EnterScope()` with a `using` declaration. The compiler prevents any usage pattern that would escape the scope across an `await`. A `Lock.Scope.Dispose()` call that throws `SynchronizationLockException` means the current thread does not hold the lock at the point of disposal — which can occur if a `using var scope = _lock.EnterScope()` block incorrectly re-enters the lock on a different thread via an unexpected synchronous continuation. The diagnostic: `Lock.IsHeldByCurrentThread` returns `true` only from the thread that entered; a `false` at the disposal site indicates a continuation routing bug.
- `Lock.TryEnter(TimeSpan)` attempts entry with a timeout, returning `bool`. There is no `TryEnterScope` overload; the `Scope` form is unconditional entry only. For disposal under bounded deadline: `if (!_lock.TryEnter(deadline)) { /* escalate */ return; } try { /* ordered dispose */ } finally { _lock.Exit(); }`.

[GLOBAL_DRAIN_ORDER]:
- Drain order is strict: (1) fence new work from entering; (2) signal cancellation via the root `CancellationTokenSource`; (3) drain compute — await all in-flight operations to complete or cancel; (4) flush persistence — ensure all committed writes reach durable storage; (5) complete UI — let the UI thread process its final frame and release host surfaces; (6) dispose handles — release native resources in child-before-container order.
- Each step's deadline is a cancellation window derived from `TimeProvider`, not `Thread.Sleep` or `Task.Delay(hardcoded)`. `CancellationTokenSource(TimeSpan delay, TimeProvider provider)` creates a time-limited cancellation scope testable via `FakeTimeProvider` without wall-clock coupling. `FakeTimeProvider.Advance(TimeSpan)` triggers the cancellation timer synchronously within the `Advance` call on the thread calling `Advance`; test code that calls `Advance` while holding a lock the cancellation callback also acquires will deadlock.
- `CancellationTokenSource.CancelAsync()` returns a `Task` that completes after all registered callbacks have been invoked. Using `Cancel()` synchronously can block the calling thread if callbacks are long-running.
- `CancellationTokenSource.CreateLinkedTokenSource(ReadOnlySpan<CancellationToken>)` links N parent tokens into a single child without creating a cascade of linked sources.
- `CancellationTokenSource.Dispose()` is not safe to call while other threads hold the `CancellationToken` and are calling `CancellationToken.Register(...)` concurrently; `Dispose` frees internal timer and state objects, and a concurrent `Register` may access freed state. A source is disposed only after all token-holder scopes have been fenced — specifically after `CancelAsync()` has completed and the drain confirms no new registrations are possible.
- `CancellationTokenSource.TryReset()` resets the source for reuse if and only if cancellation has not been requested; returns `false` if the source was already canceled or if a `CancelAfter` timer is pending. On a successful `TryReset`, all previously registered callbacks are cleared and will no longer be notified for any subsequent cancellation — the source is reset for an unrelated operation. A source for which `TryReset` returns `false` is terminal: dispose it and allocate a fresh source.
- `ChannelWriter<T>.Complete(Exception?)` signals that no more items will be written. `ChannelReader<T>.Completion` is a `Task` that transitions to completed only after `Complete` has been called and all pending reads have been satisfied — it is the canonical await point for confirming a channel has been fully drained. `Completion` does not fire after `Complete` while unread items remain in the buffer; it fires when the buffer is also empty.
- `BoundedChannelOptions.AllowSynchronousContinuations = false` (the default) guarantees that `WriteAsync` completions are dispatched asynchronously, preventing the writer from re-entering the reader's continuation inline and potentially deadlocking on a bounded channel that blocks writers when full. Setting it to `true` can improve throughput on hot single-producer/single-consumer paths but requires verifying that no reader holds a lock the writer also acquires.
- `Channel.CreateUnboundedPrioritized<T>(UnboundedPrioritizedChannelOptions<T>)` accepts an `IComparer<T>` for priority ordering; `TryRead` returns the highest-priority item. For drain-order where high-severity items must flush before low-severity ones, this is the BCL-native prioritized drain primitive. The `IComparer<T>` contract specifies total order; partial orders produce non-deterministic drain sequences.
- `Channel.CreateBounded<T>(BoundedChannelOptions, Action<T> itemDropped)` accepts a callback that fires synchronously when an item is dropped due to a full channel under `DropOldest`, `DropNewest`, or `DropWrite` modes. For channels carrying `IDisposable` items, this callback is the only disposal seam: without it, dropped items are silently discarded without disposal. The channel's own `Complete` and drain do not dispose items remaining in the buffer; the reader must drain and dispose, or the `itemDropped` callback must dispose items that never reach the reader. The callback fires on the writer's thread during the `WriteAsync` or `TryWrite` call that triggers the drop; it must not block, must not throw, and must not call `WriteAsync` or `TryWrite` on the same channel. For `BoundedChannelFullMode.DropWrite` (the written item is dropped, not an existing one), `itemDropped` receives the item just written and rejected — it was never in the channel's buffer, and `itemDropped` is guaranteed to fire before the channel contains or has processed it.

```csharp
// BoundedChannel with resource-bearing items — itemDropped is the disposal seam
static Channel<IResource> BoundedResourceChannel(int capacity) =>
    Channel.CreateBounded<IResource>(
        new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.DropOldest },
        itemDropped: r => r.Dispose());   // dropped items disposed here; drained items disposed by reader
```

```csharp
// Clock-derived cancellation windows, channel drain, and pipe teardown
static async ValueTask DrainAsync(
    Channel<WorkItem> channel,
    PipeWriter        pipeWriter,
    TimeProvider      clock,
    CancellationToken host)
{
    // Step 1: fence new work
    channel.Writer.Complete();

    // Step 2: clock-scoped drain deadline derived from TimeProvider
    using var drainCts = new CancellationTokenSource(TimeSpan.FromSeconds(20), clock);
    using var linked   = CancellationTokenSource.CreateLinkedTokenSource(
        [host, drainCts.Token]);

    // Step 3: channel drain — Completion fires when buffer is empty after Complete
    await channel.Reader.Completion.WaitAsync(linked.Token);

    // Step 4: pipe flush — writer Complete before reader Complete
    await pipeWriter.CompleteAsync();
}
```

[DISPOSAL_GUARD]:
- `ObjectDisposedException.ThrowIf(bool condition, object instance)` and `ObjectDisposedException.ThrowIf(bool condition, Type type)` are the expression-form disposal guards; the `object instance` overload uses `instance.GetType().FullName`, the `Type type` overload uses `type.FullName`. The idiomatic capsule disposal guard: `_disposed` is a `volatile int`; `Dispose` uses `Interlocked.CompareExchange(ref _disposed, 1, 0)` to ensure exactly-once disposal; projection methods use `ObjectDisposedException.ThrowIf(_disposed == 1, this)` as the fast read-only gate. `Interlocked.CompareExchange` in the dispose path guarantees at-most-once teardown; the `ThrowIf` in the use path is a non-atomic read that is correct only because disposal is terminal — once disposed, `_disposed` is permanently 1 and reads of 1 are always correct. A type that can be "un-disposed" or reset after disposal cannot use this pattern.

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

[IDISPOSABLE_ASYNCDISPOSABLE_SELECTION]:
- A type that implements both `IDisposable` and `IAsyncDisposable` with non-equivalent bodies produces an interface contract that callers cannot compose safely: `await using` selects `DisposeAsync` without reaching `Dispose`, so any code path that acquires the type as `IDisposable` skips the async flush. The only valid dual-implementation form: `Dispose()` calls `DisposeAsync().AsTask().GetAwaiter().GetResult()` as a documented sync-over-async bridge for callers that cannot await, with explicit guidance that blocking disposal on a `SynchronizationContext` that executes work on the same thread deadlocks. The deadlock surface is not limited to UI threads — `AsyncPump` patterns and custom task schedulers with single-thread affinity produce the same pathology.
- `IAsyncDisposable.ConfigureAwait(false)` returns `ConfiguredAsyncDisposable`, a value type that wraps the `DisposeAsync` call; `await using (capsule.ConfigureAwait(false))` synthesizes the pattern without requiring the capsule itself to call `ConfigureAwait` internally. This is the call-site form; a capsule's internal `DisposeAsync` should still call `ConfigureAwait(false)` on every awaited operation to prevent context capture for nested async flushes.
- When a capsule holds a mix of `IDisposable` and `IAsyncDisposable` children — e.g., a `SafeHandle` (synchronous) and a `Channel<T>` (asynchronous drain) — the parent is `IAsyncDisposable`; `DisposeAsync` drains the async child first, then disposes the synchronous child inline. The parent must not implement `IDisposable` unless the synchronous body is complete without the async drain — if the async drain is required for correctness, a synchronous `Dispose` that skips it is a partial-cleanup bug, not a valid fallback.
- `SafeHandle`-backed capsules that add an async flush layer should delegate the `SafeHandle.Dispose()` call to the end of `DisposeAsync` after the flush completes, not in a `Dispose()` override, to guarantee the handle is not released before the flush drains — a racing `Dispose()` that frees the handle while `DisposeAsync`'s flush is in progress produces a use-after-free on the native resource.
