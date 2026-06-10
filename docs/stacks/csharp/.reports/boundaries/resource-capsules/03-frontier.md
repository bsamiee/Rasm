# Resource Capsule Law — Frontier

[IO_WITHENV_SCOPED_RESOURCE_ENVIRONMENT]:
- `IO<A>.WithEnv(Func<EnvIO, EnvIO>)` maps the `EnvIO` threading through the computation into a new local environment. Any resources acquired inside the mapped computation are tracked in the new context and automatically released when the computation completes — whether by success, failure, or cancellation. The outer computation's resource registry is unaffected. This is the correct form when a sub-computation must own a distinct resource scope without creating an explicit `bracketIO` wrapper: it composes resource scoping as an IO transformation rather than a control-flow boundary.
- `IO<A>.WithEnvFail(Func<EnvIO, EnvIO>)` applies the same scoped environment but releases resources only on failure. On success, resources acquired in the sub-computation survive into the outer scope and participate in the outer environment's cleanup. This is the correct form for acquire-on-success patterns where the resource must outlive the acquiring computation when it succeeds but must be cleaned up if the acquisition computation itself fails mid-way.
- `localIO<M,A>(K<M,A>)` creates a local cancellation environment: a fresh linked `CancellationToken` is created for the computation; `IO.cancel` fired inside the local scope raises an exception that propagates outward, but it does not cancel the outer token. Resources acquired inside `localIO` are released on that scope's exit. This is distinct from `WithEnv`: `localIO` scopes the cancellation token, `WithEnv` maps the entire `EnvIO` including its resource registry. A computation that needs both an isolated cancel scope and an isolated resource registry uses `localIO(computation.WithEnv(...))`.

```csharp
// WithEnvFail: resource survives on success, cleaned on mid-acquire failure
static IO<Connection> AcquireConnection(ConnectionSpec spec) =>
    liftIO(() => Connection.Open(spec))
        .WithEnvFail(env => env);  // failure in Open cleans up env; success returns live conn

// localIO: isolated cancel scope — outer token unaffected by inner IO.cancel
static IO<Result> IsolatedWork(IO<Result> work) =>
    localIO(
        from r in use(work)
        select r
    );
```

[REPEATIO_RESOURCE_RELEASE_PER_ITERATION]:
- `RepeatIO`, `RepeatIO(Schedule)`, `RepeatWhileIO`, and `RepeatUntilIO` on `MonadUnliftIO` release all resources acquired in each iteration before the next iteration begins. The documented constraint: resources cannot be acquired inside a repeated computation and returned from it — the acquisition is released at the iteration boundary even when the iteration succeeds. Resources that must persist across iterations must be acquired outside the `RepeatIO` scope and passed as values into the repeated body.
- The practical consequence for polling capsules: a capsule that polls a foreign service on a schedule using `RepeatIO(Schedule.spaced(interval))` must acquire the connection outside the `RepeatIO` call, or accept that each iteration incurs full connection setup and teardown.

```csharp
// Connection acquired outside repeat — survives across all poll iterations
static IO<Unit> PollForever(Func<IO<IConn>> connFactory, IO<Unit> body) =>
    bracketIO(
        from conn in use(connFactory())
        from _    in RepeatIO(body)   // body closes over conn by value
        select unit
    );
```

[ATOMHASHMAP_SWAPKEY_OPTION_DISPATCH]:
- `AtomHashMap<K, V>.SwapKey(K, Func<Option<V>, Option<V>>)` is a four-case atomic dispatch over existence and desired state. The operation table is exact: `Some(x) → Some(y)` performs `SetItem(key, y)`; `Some(x) → None` performs `Remove(key)`; `None → Some(y)` performs `Add(key, y)`; `None → None` is a no-op. The swap lambda may be called multiple times under CAS contention; it must be side-effect-free. This is the single operation that covers create-or-update, conditional remove, and no-op existence checks — replacing any combination of separate `Add`/`Remove`/`Find` calls that would have a gap between read and mutation.
- The registry evict pattern — capturing into a local `Option<V>` variable before the lambda and disposing after the `SwapKey` call — satisfies both the side-effect-free constraint and the dispose-outside-atom law.
- `AtomHashMap<K, V>.Remove(K)` is unconditional atomic removal with no capture of the previous value. Use `SwapKey` when the removed value must be observed or disposed; use `Remove` when the prior value is irrelevant.

[SAFEHANDLEMARSHALLER_THREE_MODES]:
- `SafeHandleMarshaller<T>` exposes three nested marshaller types for the three P/Invoke parameter directions. `ManagedToUnmanagedIn` handles `in` parameters and pass-by-value: `FromManaged(handle)` calls `DangerousAddRef`, `ToUnmanaged()` returns the raw `IntPtr`, and `Free()` calls `DangerousRelease`. The add-ref/release pair is managed internally and is not caller-visible.
- `ManagedToUnmanagedOut` handles `out IntPtr` parameters for factory-style P/Invokes that return a new handle. `ToManaged()` creates the `SafeHandle` subclass instance wrapping the raw pointer, performing the equivalent of `Marshal.InitHandle(new THandle(), rawIntPtr)` automatically. A `[LibraryImport]` method with an `out T handle` where `T : SafeHandle` uses this marshaller implicitly; there is no manual `Marshal.InitHandle` call.
- `ManagedToUnmanagedRef` handles `ref T handle` for bidirectional parameters. `OnInvoked()` is called after the native invocation and before `ToManagedFinally()`: it signals the marshaller to finalize the ref bookkeeping. A `[LibraryImport]` `ref SafeHandle` call that is interrupted between the P/Invoke and `OnInvoked` leaves the handle in an uninitialized managed wrapper; the marshaller's `Free` disposes the partial state.
- The practical rule: use `[LibraryImport]` with typed `SafeHandle`-derived parameters for all new P/Invoke surfaces. The `SafeHandleMarshaller<T>` three-mode dispatch replaces the entire `[DllImport]` + `[MarshalAs(UnmanagedType.SysInt)]` + manual `DangerousAddRef`/`DangerousRelease` pattern.

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

[MEMORYMARSHAL_CREATEFROMPINNEDARRAY_POH_SLICE]:
- `MemoryMarshal.CreateFromPinnedArray<T>(T[], int start, int length)` creates a `Memory<T>` backed by a pinned-object-heap (POH) array without allocating a `MemoryManager<T>`. The caller is responsible for ensuring the array was allocated via `GC.AllocateUninitializedArray<T>(length, pinned: true)` before calling this method; passing a non-POH array produces a `Memory<T>` whose backing reference is not pinned, and the GC may move the array between the time `Memory<T>.Span` is taken and when the span is used in a P/Invoke. The method does not validate that the array is pinned.
- The distinction from `new Memory<T>(array, start, length)`: the standard constructor requires the caller to pin before exposing to native code via `Memory<T>.Pin()`, which allocates a `MemoryHandle` with a `GCHandle`. `CreateFromPinnedArray` skips the pin step entirely because the backing storage is permanently pinned. `GC.AllocateUninitializedArray` + `MemoryMarshal.CreateFromPinnedArray` is the correct pair for high-frequency I/O buffers that need zero-copy native interop; the `GCHandle.Alloc(Pinned)` + `MemoryMarshal.CreateFromPinnedArray` combination is incorrect because `GCHandle.Alloc(Pinned)` does not place the array in the POH and the pin is temporary.
- POH allocations are never compacted. Every `GC.AllocateUninitializedArray(..., pinned: true)` that is never freed produces a permanent hole in the POH. The correct scoping: POH arrays are owned by a capsule whose `Dispose` sets the backing field to `null`, allowing the GC to collect the POH object and reclaim the slot. The `MemoryMarshal.CreateFromPinnedArray` call must be guarded by an `IsAllocated` check on the capsule's disposal state before dereferencing.

[AWAITANY_CANCEL_ON_FIRST_SUCCESS]:
- `awaitAny(ForkIO<A>[])` cancels all remaining forks on the first successful result and returns immediately. Errors are collected; if all forks fail, the accumulated error sequence is returned. Any resources acquired inside a fork that is cancelled by `awaitAny` are released by that fork's `EnvIO` scope exit, just as if the fork had been cancelled by an explicit `ForkIO.Cancel`. The caller does not need to track or cancel losing forks.
- `awaitAll(ForkIO<A>[])` awaits all forks and returns a `Seq<A>`. It does not cancel any fork on failure of a sibling. If one fork fails, `awaitAll` continues awaiting the rest and collects all results and errors. Resources in each fork are released independently on that fork's completion.
- For competitive acquisition patterns (e.g., acquiring from multiple candidate endpoints, returning whichever connects first): `awaitAny` is the correct combinator. The acquired resource from the winning fork is in the winner's `EnvIO` scope, not the caller's; the caller must `use` the projected value from `awaitAny` to transfer ownership into the outer scope.

```csharp
// Competitive connection: first ready wins, rest auto-disposed via fork exit
static IO<ISession> FirstAvailable(Seq<ConnectionSpec> specs) =>
    bracketIO(
        from winner in awaitAny(specs.Map(spec =>
            liftIO(() => Session.Connect(spec)).Fork(Some(TimeSpan.FromSeconds(5)))))
        from session in use(Pure(winner))
        select session
    );
```

[CRITICALHANDLE_REFCOUNT_DISTINCTION]:
- `SafeHandle` maintains a reference count that starts at 1 (when `ownsHandle` is true). `ReleaseHandle` is called only when the count reaches 0. A `SafeHandle` subclass used for a resource with no native reference-count semantics will hold the handle open if any code calls `DangerousAddRef` without a matching `DangerousRelease`, which can occur when a P/Invoke marshaling path is interrupted by a thread abort between `DangerousAddRef` and the native call's `DangerousRelease`.
- `CriticalHandle` has no reference count: `ReleaseHandle` fires on the single `Dispose` or finalizer path. The correct base for a handle whose native lifetime is exclusively owned by the capsule and never shared with a P/Invoke marshaling layer that performs `DangerousAddRef`. For `[LibraryImport]`-marshaled handles using `SafeHandleMarshaller<T>`, the marshaller performs `DangerousAddRef` internally — this is why `SafeHandle` is correct for marshaled handles. For handles that never pass through `[LibraryImport]` marshaling (constructed from a P/Invoke `out IntPtr` parameter, stored in a capsule, and released only on capsule disposal), `CriticalHandle` avoids the spurious ref-count layer.
- The capsule selection rule: if the handle ever passes as a typed `SafeHandle` parameter in a `[LibraryImport]` call, use `SafeHandle`; the marshaller needs the ref-count hook. If the handle is created once, stored in a capsule, and only released by the capsule, use `CriticalHandle`.

[OBJECTPOOL_DISPOSABLE_REJECTION_DISCIPLINE]:
- `IPooledObjectPolicy<T>.Return(T obj)` returning `false` causes `DefaultObjectPool<T>` to discard the object without disposing it. The pool's `Return` method is not `IDisposable`-aware: it does not call `Dispose` on a rejected `IDisposable` object. An `IPooledObjectPolicy<T>` for a pooled type that carries `IDisposable` state must call `obj.Dispose()` inside `Return` before returning `false`. Failing to dispose in the policy means every rejected rental leaks.
- `DefaultObjectPool<T>` with a type that implements `IResettable` calls `TryReset()` inside `ReturnCore`; `TryReset()` returning `false` is treated identically to a policy `Return` returning `false` — the object is discarded, not pooled, and not disposed by the pool. An `IResettable.TryReset()` implementation that encounters a degraded internal state must dispose in-band before returning `false`.
- `LeakTrackingObjectPool<T>` uses `ConditionalWeakTable` allocation overhead per rental in `RELEASE` builds without providing any diagnostic benefit. The correct integration: reference `LeakTrackingObjectPool<T>` only through an `ObjectPool<T>` interface and swap it out via `ObjectPoolProvider` in `Debug` builds.

[PIPE_RESET_AND_REUSE]:
- `Pipe.Reset()` returns a `Pipe` to its initial state, releasing all internal buffer segments back to the `MemoryPool<byte>`. It may only be called when both the `PipeReader` and `PipeWriter` are in a completed state. The law for pipe reuse: complete writer, drain reader to `Completion`, then `Reset`. A capsule that reuses a `Pipe` across operation cycles must enforce this ordering explicitly, because `Pipe.Reset` does not check caller state before releasing buffers.
- `PipeOptions.PauseWriterThreshold` and `PipeOptions.ResumeWriterThreshold` control back-pressure. A capsule that allocates a `Pipe` for an internal protocol buffer must set these thresholds to bound memory usage; default `PipeOptions` sets both to 0, which disables back-pressure and allows unbounded growth if a reader stalls.
- `PipeOptions.UseSynchronousCompletion` enables synchronous continuations on the pipe's internal scheduler. The `Pipe.Writer.WriteAsync` completion may run the reader's awaiter inline on the writer's thread. For capsules that process pipe data under a `Lock`, this option must not be enabled.

[IO_TIMEOUT_RESOURCE_GUARANTEE]:
- `IO<A>.Timeout(TimeSpan)` applies a wall-clock limit to the computation. When the timeout fires, the `EnvIO`'s `CancellationToken` is cancelled, the computation exits with a timeout exception, and all resources registered with `use` inside the timed computation are released by the `EnvIO` scope exit. A `bracketIO` computation wrapped with `.Timeout(...)` composes safely: the `bracketIO` cleanup runs on timeout exit just as on normal exit.
- `IO<A>.Timeout` uses cooperative cancellation. A computation blocked on a non-cancellable synchronous call will not be interrupted by the timeout: the timeout fires, the token is cancelled, but the computation thread continues executing until the P/Invoke returns. Any native call inside a `bracketIO` used with `.Timeout` must itself accept and observe a `CancellationToken`, or must be wrapped in a fork so the timeout can abandon the blocking thread while resource cleanup occurs in the fork's own scope.
- `MonadUnliftIO<M>.TimeoutIO<A>(K<M, A>, TimeSpan)` exposes the same guarantee at the trait level, allowing timeout to compose across `Eff`, `IO`, and any custom effect monad implementing `MonadUnliftIO`. This is the correct surface for timeout in polymorphic effect pipelines; calling `IO<A>.Timeout` directly requires downcasting from the monad to `IO`.

[SAFEHANDLE_DANGEROUSADDREF_REF_PARAMETER_CORRECT_FORM]:
- `SafeHandle.DangerousAddRef(out bool success)` has a required contract: `success` is `false` when the handle is already marked invalid or closed at the time of the call. The caller must check `success` before using the raw handle and must not call `DangerousRelease` if `success` is `false`.

```csharp
bool success = false;
try
{
    handle.DangerousAddRef(ref success);
    if (!success) return Fin<T>.Fail(Error.New("handle closed"));
    nint raw = handle.DangerousGetHandle();
    return NativeOp(raw);
}
finally
{
    if (success) handle.DangerousRelease();
}
```

- A failed `DangerousAddRef` (`success == false`) means the handle has been marked closed by `Dispose` on another thread between the capsule's `IsInvalid`/`IsClosed` pre-check and the `DangerousAddRef` call. The pre-check at the capsule's `Use<T>` entry point is a fast-path optimization only; `DangerousAddRef`'s own closed check is the true gate. A capsule that omits the `success` guard and unconditionally calls `DangerousRelease` decrements a count that was never incremented, corrupting the ref-count and triggering a premature `ReleaseHandle`.
- This pattern is required only inside `liftIO` or `use` when the raw handle must be passed to a non-`[LibraryImport]` path. For `[LibraryImport]`-marshaled parameters, `SafeHandleMarshaller<T>.ManagedToUnmanagedIn.FromManaged` performs `DangerousAddRef` with the same guard and `Free` performs the matching `DangerousRelease`; the caller is entirely insulated.
