# Resource Capsule Law

[CAPSULE_OWNERSHIP]:
- Every native or foreign resource has exactly one capsule owner responsible for acquire, project, and dispose. Borrowed and owned lifetimes are not separate types; they are cases of one capsule surface — a closed family with a `Borrowed` case that does not dispose and an `Owned` case that does. Projections from either case leave as value copies, never as live handles. The capsule's `Use<T>(Func<R, Fin<T>>)` method is the single projection entry point; callers never receive the raw handle.
- `SafeHandle` (`System.Runtime.InteropServices.SafeHandle`) is the BCL owner for unmanaged handles: it reference-counts through `DangerousAddRef`/`DangerousRelease`, runs `ReleaseHandle` in a constrained execution region even on thread abort, and survives the GC finalizer. For any raw `IntPtr` native handle, the capsule's inner representation must derive from `SafeHandle` — not hold a bare `IntPtr` — so P/Invoke marshaling never races disposal. The `IsClosed` and `IsInvalid` predicates are the canonical validity gates; `DangerousGetHandle` is a last resort only inside a `DangerousAddRef`/`DangerousRelease` guard block.
- `CriticalHandle` (non-reference-counting sibling) is the correct base when the native API has no reference-count semantics; `SafeHandle` is not a drop-in because its ref-count can hold a handle open past the capsule's logical disposal.
- The `IDisposable` pattern on capsules: the `Dispose(bool disposing)` override disposes managed children (including the inner `SafeHandle`) only when `disposing` is `true`; it releases unmanaged state unconditionally. The `IsInvalid` override on the inner `SafeHandle` must return `true` exactly when the raw handle value is the platform-defined invalid sentinel (0 or -1 depending on the family), not based on a separate boolean flag that can desync.

[BRACKET_ACQUISITION]:
- The IO monad (`LanguageExt.IO<A>`) tracks acquired resources inside `EnvIO`'s resource registry. `use(acquire)` registers an `IDisposable`-bearing acquired value; `use(acquire, release)` registers with a custom `IO<Unit>` or `Action<A>` finalizer for non-`IDisposable` handles. Both forms register under the current `EnvIO` scope and remain live until `@using` or `bracketIO` closes the scope.
- `bracketIO(computation)` creates a local resource environment: any resource registered with `use` inside `computation` is released when `computation` completes — whether by success, failure, or cancellation. This is the primary bracket form. The three-argument form `bracketIO(acq, use, fin)` is the explicit acquire-use-release triple; it is less preferred because the `bracketIO(computation)` form composes without threading the resource through every continuation manually.
- `IO<A>.Bracket(use, fin)` and `IO<A>.BracketFail(use, fin)` are instance-level variants: `Bracket` releases on both success and failure; `BracketFail` releases only on failure, leaving the resource live on success for further composition. Neither form is compatible with ambient cancellation scope management — they are operation-local and do not interact with outer `bracketIO` scopes.
- `release<A>(value)` is an explicit early-return to the IO environment's resource registry; it runs the registered finalizer immediately and removes the registration. The value must have been registered by a prior `use` call in the same `EnvIO` scope; calling `release` on an unregistered value is a no-op, not an error.
- `useMaybe<M,A>(K<M,A>)` acquires when the monad produces a value and tracks it; if the monad produces nothing (e.g., `None` from `OptionT`), no registration is made. This is the correct form for optional resources where absence is typed, not exceptional.
- `useAsync<A>(IO<A>)` and `useAsync<M,A>(K<M,A>)` wrap `IAsyncDisposable` resources; the IO environment calls `DisposeAsync` on scope exit. These are not fallback for `IDisposable` — they are the only correct registration form for resources whose disposal is inherently asynchronous (e.g., a connection whose flush requires an async drain).

```csharp
// Capsule: Borrowed/Owned cases, single projection entry
abstract record Capsule<H>(H Handle) where H : SafeHandle
{
    public sealed record Borrowed(H Handle) : Capsule<H>(Handle);
    public sealed record Owned(H Handle)   : Capsule<H>(Handle);

    // Projection leaves as value; handle never escapes
    public Fin<T> Use<T>(Func<H, Fin<T>> project) =>
        Handle.IsInvalid || Handle.IsClosed
            ? Fin<T>.Fail(Error.New("handle invalid"))
            : project(Handle);

    // Capsule is IDisposable only for the Owned case
    public void Dispose() { if (this is Owned) Handle.Dispose(); }
}
```

[TWO_PHASE_PROBE_THEN_CONSTRUCT]:
- Probe and construction must be atomic from the caller's perspective: a probe that succeeds followed by construction must be a single indivisible acquire from the calling code's view. A gap between probe and construction where another caller can observe "probed but not yet constructed" is a protocol violation that yields stale-probe authorization of construction after the underlying resource has been reacquired or changed.
- The correct shape: `Atom<Option<TSession>>` (or `Atom<Option<Guid>>` for token-gated singletons) where the `SwapMaybe` call is the atomic gate. `SwapMaybe(current => current.IsNone ? Some(construct()) : current)` is one CAS operation — probe (is `None`) and construction (produce `Some`) are the same lambda, executed under spin-lock. If the atom already has a value, `SwapMaybe` returns `None` without calling `construct()`.

```csharp
// Atomic probe-then-construct: one SwapMaybe, no observable gap
static readonly Atom<Option<Session>> _session = Atom<Option<Session>>(None);

static Fin<Session> AcquireOnce(SessionConfig cfg) =>
    _session.SwapMaybe(current =>
        current.IsSome ? current : Some(Session.Open(cfg)))
    .ToFin(Error.New("session already acquired"));
```

- `Atom<A>.SwapMaybe(Func<A, Option<A>>)` returns the new value if the swap occurred, or the unchanged current value if `None` was returned. The function `f` must be side-effect-free because it may be called multiple times during CAS retry. Construction inside `SwapMaybe` must therefore be idempotent: if called multiple times, all but one result must be safe to discard (or the construction itself is deferred until after the CAS, which then requires a two-step that reintroduces the gap).
- When construction is not idempotent, the correct form is: CAS a sentinel (e.g., a `Guid` token representing "being acquired") first, then construct and update to the final value, then release the sentinel on failure. This requires `Atom<Option<Guid>>` as the token gate with `SwapMaybe(None → Some(newGuid))` as the exclusive entry and a `Swap(Some(_) → None)` on failure.

[REGISTRY_OWNED_SESSION_DISPOSAL]:
- The registry that constructs a session owns its disposal. Call sites receive a session reference (or a value projected from it); they never call `Dispose` directly. A call site that calls `Dispose` or `Close` on a registry-managed session breaks the registry's invariant and may dispose a session concurrently used by another call site.
- Cache keys must be structurally complete: content hash plus full configuration. Partial keys (e.g., path-only, or path plus version but not flags) produce stale-hit false positives — the cached session's full configuration differs from the request's, producing silent behavioral divergence not visible until a field that was excluded from the key changes.
- The registry is `AtomHashMap<CacheKey, Session>` (or `Atom<HashMap<CacheKey, Session>>`). Disposal of a removed session must happen outside the atom's swap lambda because the lambda must be side-effect-free. The correct form: remove from the atom atomically, capture the removed value, then dispose it after the atom update completes.

```csharp
// Registry-owned disposal: remove atomically, then dispose outside atom
sealed class SessionRegistry : IDisposable
{
    readonly AtomHashMap<CacheKey, ISession> _sessions = AtomHashMap<CacheKey, ISession>();

    public ISession GetOrCreate(CacheKey key, Func<ISession> factory) =>
        _sessions.AddOrUpdate(key, factory, (_, existing) => existing);

    public Unit Evict(CacheKey key)
    {
        var removed = Option<ISession>.None;
        _sessions.SwapKey(key, current =>
        {
            removed = current;
            return Option<ISession>.None;
        });
        return removed.IfSome(s => s.Dispose());
    }

    public void Dispose() =>
        _sessions.ToSeq()
                 .Iter(pair => pair.Value.Dispose());
}
```

- Ephemeral per-call sessions are forbidden. A call site that constructs a session, uses it, and disposes it inline defeats caching, pays initialization cost on every call, and risks multiple concurrent initializations racing toward the same underlying resource. All session management routes through the registry; the registry decides whether an existing session satisfies the key.

[OPERATION_SCOPED_DATA_CONTEXT]:
- Data-access contexts (connections, transactions, unit-of-work scopes) are acquired at the start of the operation that needs them, disposed at the operation boundary, and never captured across `await` continuations or stored as fields. The `await` point is a suspension that can resume on a different thread; a context captured before `await` and accessed after may violate thread-affinity or connection-state invariants of the underlying provider.
- "Ambient context" patterns (a static `AsyncLocal<TContext>` or a singleton injected into a service) make context lifetime invisible: any code in the call tree may consume it without making a visible dependency, and the context may be disposed before consumers in forked work items observe it. The explicit alternative: inject `Func<IO<TContext>>` as a factory and call it at the top of each operation.
- The operation-scope model maps directly to `bracketIO`: acquire the context inside the computation, use it via `use`, and let `bracketIO` dispose it on exit.

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
- `ArrayPool<T>.Shared.Return(array, clearArray: false)` is the preferred form for non-sensitive data; `clearArray: true` zeros the buffer before pooling, adding overhead but preventing data leakage between unrelated renters. For credential or keying material, `clearArray: true` is required.
- `MemoryPool<T>.Rent(minBufferSize)` returns `IMemoryOwner<T>`, which carries `IDisposable`; callers must dispose the owner to return the memory to the pool. The `Memory<T>` slice `owner.Memory` is the usable surface; the owner reference itself must not be shared across the lifetime seam.
- The pool vocabulary (`ArrayPool`, `MemoryPool`, `IMemoryOwner`, rented array sizes) must not appear on any public method signature. Public signatures express logical extent through `ReadOnlySpan<T>`, `ReadOnlyMemory<T>`, or a domain value type that internally slices into a rented buffer. Callers that see `IMemoryOwner<T>` in a return type become coupled to return timing and cannot be composed freely.
- Buffers return at phase boundaries, not at the end of processing. A pipeline stage that rents, processes, and then emits into a channel must return its input buffer before the write to the next stage, not after. Retaining a buffer across a phase transition risks holding it past the next renter's fill.

[MAINTENANCE_CONNECTION_QUARANTINE]:
- Connections opened for maintenance operations (backup, integrity check, compaction, vacuum, schema migration) acquire exclusively within the lifecycle state that permits them. No maintenance connection persists across a lifecycle transition; the transition itself is the signal to abort and quarantine.
- If a maintenance connection is interrupted mid-way through a sequence of configuration directives (e.g., pragma settings, journal mode changes), the partial state of those directives is undefined. The connection is quarantined: closed without further writes, never reused, and the operation emits a failure receipt rather than silently continuing with an ambiguous connection state.
- Quarantine does not imply the operation is retried. Retry policy lives at the lifecycle state machine level, which decides whether re-entering the maintenance state with a fresh connection is safe given the previous partial application. The capsule does not encode retry.

[CHILD_BEFORE_CONTAINER_DISPOSAL]:
- Child surfaces close and emit completion before the container that holds them begins disposal. Inversion — disposing the container while children still hold references to container resources — corrupts handles still alive in the child and produces access violations or use-after-free in native handle teardown.
- The canonical ordering in a container capsule: (1) signal children to stop accepting new work; (2) await child drain completions; (3) dispose child handles; (4) dispose container handle. Steps 3 and 4 are sequential, not concurrent.
- `System.Threading.Lock.EnterScope()` returns a `Scope` ref-struct that exits the lock on `Dispose`. For disposal ordering critical sections, `Lock` + `EnterScope` is the correct mechanism rather than `Monitor`/`lock` keyword, because `Scope` is a ref-struct whose `Dispose` cannot be deferred across `await`.

[GLOBAL_DRAIN_ORDER]:
- Drain order is strict: (1) fence new work from entering; (2) signal cancellation via the root `CancellationTokenSource`; (3) drain compute — await all in-flight operations to complete or cancel; (4) flush persistence — ensure all committed writes reach durable storage; (5) complete UI — let the UI thread process its final frame and release host surfaces; (6) dispose handles — release native resources in child-before-container order.
- Each step's deadline is a cancellation window derived from `TimeProvider`, not a `Thread.Sleep` or `Task.Delay(hardcoded)`. `CancellationTokenSource(TimeSpan delay, TimeProvider provider)` creates a time-limited cancellation scope that is testable via `FakeTimeProvider` without wall-clock coupling.
- `CancellationTokenSource.CancelAsync()` is the async-friendly cancellation signal; it returns a `Task` that completes after all registered callbacks have been invoked. Using `Cancel()` synchronously can block the calling thread if callbacks are long-running; `CancelAsync()` defers them.
- `CancellationTokenSource.CreateLinkedTokenSource(ReadOnlySpan<CancellationToken>)` (the span overload added in .NET 10) links N parent tokens into a single child; the child cancels when any parent does. This is the correct form for composing the global drain token with per-operation timeout tokens without creating a cascade of linked sources.

```csharp
// Clock-derived cancellation windows — TimeProvider injected, not hard-coded
static async ValueTask DrainAsync(TimeProvider clock, CancellationToken host)
{
    using var drainCts = CancellationTokenSource.CreateLinkedTokenSource(
        [host, new CancellationTokenSource(TimeSpan.FromSeconds(30), clock).Token]);

    // Step 2: fence, then signal
    _fence.Set();            // no new work enters
    await _drainCts.CancelAsync();

    // Step 3: compute drain with bounded deadline
    await _computeWorker.DrainAsync(drainCts.Token);

    // Step 4: flush
    await _persistLayer.FlushAsync(drainCts.Token);

    // Step 5 + 6: UI and handles — synchronous from here, host controls order
}
```

[LOCK_SCOPE_AS_DISPOSABLE]:
- `System.Threading.Lock` (introduced in .NET 9, stable on .NET 10) is the typed replacement for `object` + `lock` keyword. Its `EnterScope()` method returns `Lock.Scope`, a `ref struct` with `Dispose()` that exits the lock. The `ref struct` constraint prevents it from being boxed, stored in fields, or captured across `await` — which is the correctness constraint: lock scopes must not span asynchronous suspensions.
- A disposal-ordering guard that must hold a lock across synchronous disposal of children uses `Lock.EnterScope()` + a `using` declaration. The `using` declaration's scope ends at the block exit; because `Lock.Scope` is a `ref struct`, the compiler prevents any usage pattern that would escape it across an `await`.
- `Lock.TryEnter(TimeSpan)` attempts entry with a timeout, returning `bool`; this is the correct form for disposal under bounded deadline — if the lock is not obtained within the drain window, the caller must escalate rather than block indefinitely.

[IDISPOSABLE_ASYNCDISPOSABLE_SELECTION]:
- Implement `IAsyncDisposable` when the disposal path must flush asynchronous operations (e.g., flush a `Channel`, drain a background worker, commit a pending async write). Implement `IDisposable` when disposal is synchronous. Implementing both on the same type without delegating one to the other creates two independent disposal paths that callers cannot rely on being equivalent.
- When a type implements both, `await using` calls `DisposeAsync`; `using` calls `Dispose`. If `DisposeAsync` is the canonical disposal, `Dispose` should call `DisposeAsync().AsTask().GetAwaiter().GetResult()` only as a last resort for sync-only callers — and the type's documentation must state this. The preferred design: implement only the interface that matches the actual disposal path.
- `ConfiguredAsyncDisposable` (`IAsyncDisposable.ConfigureAwait(false)`) suppresses context capture on the `DisposeAsync` continuation; for capsules disposed inside library-internal pipelines, `ConfigureAwait(false)` on `DisposeAsync` is the standard form to avoid deadlock on contexts that require specific thread affinity.

[SAFEHANDLE_PROJECTION_DISCIPLINE]:
- `SafeHandle.DangerousGetHandle()` returns the raw `IntPtr` even when the handle has been marked invalid via `SetHandleAsInvalid`. The `Dangerous` prefix is the contract: callers must guard with `DangerousAddRef(out bool success)` + `DangerousRelease()` in a `try/finally` to prevent disposal racing the use of the raw value. Any code that calls `DangerousGetHandle()` outside this guard is unsound.
- `SafeHandle.SetHandleAsInvalid()` marks the handle as no longer in use without running the finalizer. Use this when the underlying resource has already been freed by another path and running `ReleaseHandle` would double-free. It does not trigger `ReleaseHandle`; `Dispose(true)` or the finalizer would still be called but `ReleaseHandle` must then return `true` without performing I/O.
- `SafeHandleMarshaller<T>` is the source-generator marshaller for P/Invoke in LibraryImport scenarios. It manages `DangerousAddRef`/`DangerousRelease` internally for `in` parameters. Manual `[MarshalAs]` on `SafeHandle` subclasses in `DllImport` is the legacy path; `LibraryImport` + `SafeHandleMarshaller<T>` is the current surface.
