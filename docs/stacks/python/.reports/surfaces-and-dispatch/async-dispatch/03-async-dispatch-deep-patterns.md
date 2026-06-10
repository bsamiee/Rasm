# [ASYNC_DISPATCH_DEEP_PATTERNS]

Sources: anyio 4.13.0 `.venv/lib/python3.15/site-packages/anyio/` + stamina 26.1.0 `.venv/lib/python3.15/site-packages/stamina/` — verified 2026-06-09 against installed source on Python 3.15.0b1.

---

## [BLOCKING_PORTAL_AS_SYNC_TO_ASYNC_DISPATCH_BRIDGE]

[PORTAL_SHAPE]:
- `BlockingPortal` runs a task group in an event loop and exposes three call shapes from sync/worker-thread contexts: `portal.call(fn, *args)` — blocks until the coroutine (or sync callable) completes and returns the value; `portal.start_task_soon(fn, *args, name=) -> Future[T]` — fires the task and returns a `concurrent.futures.Future[T]` immediately; `portal.start_task(fn, *args, name=) -> tuple[Future[T], V]` — blocks until `task_status.started(v)` and returns `(future, v)`. [`anyio/from_thread.py` lines 318–422, anyio 4.13.0, 2026-06-09]
- `portal.call` is `cast(T, self.start_task_soon(func, *args).result())` — it blocks on the future. Cancelling the returned future via `future.cancel()` triggers a `CancelScope.cancel("the future was cancelled")` on the corresponding task's scope inside the portal's task group. A cancelled future resolves as cancelled; non-`Exception` `BaseException` subclasses fall through the portal's internal handler and re-raise in the portal's task group (not the calling thread). [`anyio/from_thread.py` lines 241–276, 334, anyio 4.13.0, 2026-06-09]
- `BlockingPortalProvider` (added 4.4) is a ref-counted shared portal: `with provider as portal:` increments a lease; the last exiting thread destroys the portal; safe to share across worker threads when all callers hold the context manager. The `_lock` is a `threading.Lock` — acquisition is synchronous and deadlock-safe as long as no caller holds it across an `async with`. [`anyio/from_thread.py` lines 442–496, anyio 4.13.0, 2026-06-09]

[DISPATCH_SURFACE_CALLING_FROM_SYNC_CONTEXT]:
- Dispatch surfaces that must be driven from synchronous code (CLI entry points, callback registrations, signal handlers) use `from_thread.run(async_fn, *args)` inside an anyio worker thread, or `portal.call(async_fn, *args)` from any thread that holds a portal reference. `from_thread.run` requires that the current thread was spawned by `to_thread.run_sync` (i.e., an anyio worker thread); calling it from an unrelated thread raises `NoEventLoopError` unless a `token=` is passed explicitly. [`anyio/from_thread.py` lines 67–92, anyio 4.13.0, 2026-06-09]
- `from_thread.run_sync(fn, *args)` schedules a sync callable on the event loop thread (not in a worker thread) and blocks until it completes. The intended use is acquiring non-thread-safe event-loop resources (e.g., setting a `RunVar`, emitting an event) from inside a worker thread. It differs from `to_thread.run_sync` in direction: `from_thread` goes worker→loop; `to_thread` goes loop→worker. [`anyio/from_thread.py` lines 95–121, anyio 4.13.0, 2026-06-09]

[PORTAL_ASYNC_CONTEXT_MANAGER_BRIDGE]:
- `portal.wrap_async_context_manager(async_cm)` converts an `async with` resource into a sync `with` resource via a dedicated task that suspends at the midpoint of the context. The `__enter__` blocks until `__aenter__` resolves; `__exit__` signals the internal event and blocks until `__aexit__` resolves. Use this pattern when a dispatch surface wraps an async resource that must be opened/closed from sync code (e.g., a connection pool lifecycle owned by a sync framework). [`anyio/from_thread.py` lines 123–174, 438, anyio 4.13.0, 2026-06-09]

---

## [CHECK_CANCELLED_AND_COOPERATIVE_CANCELLATION_IN_WORKER_THREADS]

[POLL_BASED_CANCELLATION]:
- `anyio.from_thread.check_cancelled()` raises the backend-specific cancellation exception (`asyncio.CancelledError` or `trio.Cancelled`) when the host task's enclosing cancel scope has been cancelled. It is a sync function safe to call from inside a worker thread spawned by `to_thread.run_sync`. Calling it outside a worker thread raises `NoEventLoopError`. [`anyio/from_thread.py` lines 559–578, anyio 4.13.0, 2026-06-09]
- The canonical use is long-running CPU-bound sync handlers that cannot receive a cancellation signal passively: call `check_cancelled()` at each logical checkpoint in the loop body. Without this, a `to_thread.run_sync(fn, abandon_on_cancel=False)` host cancellation suspends the task until `fn` returns naturally. With `check_cancelled()` in the hot path, the function exits the exception and satisfies the host scope's cancellation promptly.

```python
async def dispatch_arm(payload: Payload) -> Result[Output, Err]:
    def heavy_sync(data: bytes) -> Output:
        result = initial_state(data)
        for chunk in iter_chunks(data):
            from_thread.check_cancelled()
            result = fold_step(result, chunk)
        return result

    raw = await to_thread.run_sync(heavy_sync, payload.data, abandon_on_cancel=False)
    return Ok(raw)
```

---

## [RUNVAR_FOR_EVENTLOOP_SCOPED_DISPATCH_CONTEXT]

[RUNVAR_SHAPE]:
- `anyio.lowlevel.RunVar[T](name, default=)` is scoped to the current event loop run (not the current task, not the process, not a `contextvar` copy). All tasks in one `anyio.run(...)` call share the same `RunVar` values; distinct `anyio.run(...)` calls see independent values. This is the correct primitive for per-run-scope dispatch registries, connection pools, and per-run resource handles — not `contextvars.ContextVar` (task-scoped) and not module globals (process-scoped). [`anyio/lowlevel.py` lines 124–195, anyio 4.13.0, 2026-06-09]
- `RunVar.set(value)` returns a `RunvarToken` that also implements `__enter__`/`__exit__` for temporary override: `with run_var.set(override_value):` restores the previous value on exit. `RunVar.reset(token)` restores manually; double-reset raises `ValueError("This token has already been used")`. The storage is `WeakKeyDictionary[native_token, dict[RunVar, Any]]` — the dict is garbage-collected when the event loop ends. [`anyio/lowlevel.py` lines 97–193, anyio 4.13.0, 2026-06-09]
- `RunVar.get(default=)` raises `LookupError` when no value is set and no default was provided at construction or call time. Dispatch surfaces that initialize a per-run registry use `RunVar` with a factory default to provide lazy construction:

```python
_REGISTRY: RunVar[dict[str, Handler]] = RunVar("dispatch_registry", default=None)

async def resolve_handler(key: str) -> Result[Handler, MissingHandler]:
    reg = _REGISTRY.get(default=None) or {}
    return Ok(reg[key]) if key in reg else Error(MissingHandler(key))
```

---

## [RESOURCE_GUARD_FOR_EXCLUSIVE_ASYNC_DISPATCH_RESOURCES]

[GUARD_SHAPE]:
- `anyio.ResourceGuard(action="using")` is a non-async context manager (`__enter__`/`__exit__`, no `await`) that raises `BusyResourceError` if two tasks enter the same guard concurrently. The guard tracks a `_guarded: bool` flag — there is no waiter queue; the second entrant fails immediately. `BusyResourceError.__init__(action)` formats `"Another task is already {action} this resource"`. [`anyio/_core/_synchronization.py` lines 725–757, anyio 4.13.0, 2026-06-09]
- The dispatch-surface use is mutual exclusion for non-reentrant async resources that must not be driven by two concurrent calls (e.g., a stateful connection that processes one request at a time). Wrap the guard at the dispatch boundary and convert `BusyResourceError` to `Error(ResourceBusy(...))` at the rail:

```python
_guard = ResourceGuard("sending to")

async def dispatch(payload: Payload) -> Result[Response, DispatchErr]:
    with _guard:
        raw = await connection.send(payload.bytes)
    return Ok(parse(raw))
```

- Converting `BusyResourceError` to a rail error is correct: `BusyResourceError` inherits `Exception` (not `BaseException`), so it is safe to catch at the dispatch boundary without suppressing cancellations.

---

## [SYNCHRONIZATION_PRIMITIVES_WITH_PRE_LOOP_CONSTRUCTION]

[ADAPTER_PATTERN]:
- `anyio.Event()`, `anyio.Lock(fast_acquire=False)`, `anyio.Semaphore(initial, max_value=, fast_acquire=)` all use `__new__` factory dispatch: inside an event loop, `__new__` calls `get_async_backend().create_*()` and returns the backend-specific implementation; outside an event loop, `__new__` returns an `EventAdapter`/`LockAdapter`/`SemaphoreAdapter` that lazily creates the real backend object on first use. This means dispatch-surface module-level synchronization objects can be constructed at import time without a running event loop — the adapter promotes itself on first `await`. [`anyio/_core/_synchronization.py` lines 82–113, 150–155, 373–386, anyio 4.13.0, 2026-06-09]
- `Lock(fast_acquire=True)` skips the checkpoint on uncontended acquisition — no `await checkpoint()` — reducing latency in dispatch hot paths where fairness can be relaxed. The difference is visible under Trio's strict-checkpoint rule; under asyncio it affects task starvation only under high contention.
- `Semaphore(initial_value, max_value=N)` is a bounded semaphore: `release()` raises `ValueError` if incrementing would exceed `max_value`. Use `max_value` to model a fixed-capacity dispatch slot (N inflight at most) with release guaranteeing no over-count. `Semaphore.value` reads the current count (0 means all slots taken). [`anyio/_core/_synchronization.py` lines 373–454, anyio 4.13.0, 2026-06-09]

[CONDITION_WAIT_FOR_DISPATCH_COORDINATION]:
- `anyio.Condition.wait_for(predicate)` (added 4.11.0) loops `while not predicate(): await self.wait()` and returns the truthy predicate result. This enables dispatch surfaces that block until a shared state satisfies a typed condition without polling or explicit event wiring:

```python
_cond = Condition()
_ready: dict[str, Result[T, E]] = {}

async def await_result(key: str) -> Result[T, E]:
    async with _cond:
        return await _cond.wait_for(lambda: _ready.get(key))
```

- `Condition.wait` uses a shielded `CancelScope` in its `finally` block to re-acquire the lock even after a cancellation: `with CancelScope(shield=True): await self.acquire()`. This ensures the lock is always released consistently, but means a cancelled `wait` still acquires the lock before propagating. [`anyio/_core/_synchronization.py` lines 326–347, anyio 4.13.0, 2026-06-09]

---

## [STAMINA_BACKOFF_HOOK_AND_CUSTOM_BACKOFF_CONTRACT]

[BACKOFF_HOOK_SHAPE]:
- The `on` parameter of `stamina.retry` / `retry_context` accepts a `BackoffHook: Callable[[Exception], bool | float | datetime.timedelta]`. Returning `True` retries with the normal exponential backoff; returning `False` does not retry (re-raises immediately); returning a `float` (seconds) or `timedelta` overrides the next backoff entirely — the normal exponential schedule is bypassed for that attempt. This is the correct seam for `Retry-After` HTTP header semantics. [`stamina/_core.py` lines 62–116, stamina 26.1.0, 2026-06-09]
- The custom backoff is stored on the `RetryCallState` via `setattr(retry_state, "_stamina_custom_backoff", value)` — a piggyback attribute on tenacity's internal state. In test mode (`CONFIG.testing is not None`), `_jittered_backoff_for_rcs` returns `0.0` regardless of any custom backoff, ensuring tests are not subject to Retry-After delays. [`stamina/_core.py` lines 619–634, 656–658, stamina 26.1.0, 2026-06-09]
- A backoff hook returning `None` emits a `UserWarning` (not a hard error in current version) and is treated as `False`. This is documented as becoming an error in a future version.

[DISPATCH_ARM_WITH_RETRY_AFTER_HEADER]:
```python
def retry_after_hook(exc: Exception) -> bool | float:
    header = getattr(exc, "retry_after", None)
    return float(header) if header is not None else isinstance(exc, TransientError)

async def fetch_arm(key: Key) -> Result[Payload, FetchError]:
    async for attempt in stamina.retry_context(on=retry_after_hook, attempts=5):
        with attempt:
            raw = await transport.get(key)
    return Ok(parse(raw))
```

---

## [STAMINA_RETRY_HOOK_AS_SPAN_CONTEXT_MANAGER]

[HOOK_CONTRACT]:
- `RetryHook: (details: RetryDetails) -> None | AbstractContextManager[None]`. When a hook returns a context manager, stamina calls `cm.__enter__()` before the retry attempt and `cm.__exit__(None, None, None)` when the `_RetryContextIterator` exits (success or exhaustion), via the `_cms_to_exit` list. This enables span-scoped instrumentation: the hook opens an OTel span on the first retry and closes it when retrying ends. [`stamina/_core.py` lines 566–568, 684–708, stamina 26.1.0, 2026-06-09]
- `RetryHookFactory(hook_factory: Callable[[], RetryHook])` defers construction until the first scheduled retry; factories are called at most once (per `CONFIG.on_retry` property access after a reset). Use `RetryHookFactory` when hook initialization imports a heavy library (e.g., prometheus_client) that should not be imported at module load. [`stamina/instrumentation/_data.py` lines 91–103; `stamina/instrumentation/_hooks.py` lines 15–28, stamina 26.1.0, 2026-06-09]
- The built-in structlog hook (`StructlogOnRetryHook`) logs `stamina.retry_scheduled` at `WARNING` level with fields: `callable`, `args` (repr-stringified), `kwargs`, `retry_num`, `caused_by` (repr), `wait_for` (rounded to 2 places), `waited_so_far`. These field names are part of the public structlog event contract. [`stamina/instrumentation/_structlog.py` lines 10–35, stamina 26.1.0, 2026-06-09]
- Global retry hooks are replaced or reset via `stamina.instrumentation.set_on_retry_hooks(hooks | None)`. Passing `None` resets to defaults (prometheus if available, then structlog if available, else stdlib logging). Passing `()` (empty iterable) silences all instrumentation — the correct form for tests that assert on retry counts without log noise.

---

## [STAMINA_SET_TESTING_CAP_SEMANTICS_AND_CM_USAGE]

[TESTING_API]:
- `stamina.set_testing(testing=True, attempts=1, cap=False)` returns `_RestoreTestingCM` which also implements `__enter__`/`__exit__`, making it usable as a context manager for scoped test mode. In 26.1.0 this is the documented pattern: `with stamina.set_testing(True):` (no assignment needed); the old `stamina.set_active(False)` form still works but only disables retries, not backoff timing. [`stamina/_config.py` lines 158–195, stamina 26.1.0, 2026-06-09]
- `cap=True` means: the test-mode attempt count is `min(testing_attempts, production_attempts)` — it caps the production value rather than replacing it. Use `cap=True` when tests want to verify that low-attempt configurations (e.g., `attempts=2`) are genuinely short while not extending high-attempt configurations beyond the cap. `cap=False` (default) always uses `testing_attempts` regardless of what the decorated function specifies. [`stamina/_config.py` lines 32–45, stamina 26.1.0, 2026-06-09]
- In test mode, `_compute_backoff` returns `0.0` unconditionally, and any custom backoff from a backoff hook is also zeroed. The `_LazyNoAsyncRetry` null object (used when `CONFIG.is_active` is `False`) wraps `tenacity.AsyncRetrying(reraise=True, stop=stop_after_attempt(1), sleep=_smart_sleep)` — it still awaits `_smart_sleep(0)`, so the async event loop still yields once per attempt path even with retrying disabled. [`stamina/_core.py` lines 431–447, stamina 26.1.0, 2026-06-09]

---

## [ASYNC_RETRYING_CALLER_AS_REUSABLE_DISPATCH_COMPONENT]

[CALLER_SHAPE]:
- `stamina.AsyncRetryingCaller(attempts, timeout, wait_initial, wait_max, wait_jitter, wait_exp_base)` is a callable object that stores retry config and creates a new `retry_context` iterator on each invocation — instances are safe to share across dispatch arms. `await caller(on=SomeError, fn, *args, **kw)` retries `fn`. `caller.on(SomeError)` returns `BoundAsyncRetryingCaller` which pre-binds the exception type: `await bound(fn, *args, **kw)`. [`stamina/_core.py` lines 356–428, stamina 26.1.0, 2026-06-09]
- The dispatch pattern for polymorphic arms sharing one retry policy: construct one `AsyncRetryingCaller` at module level with the policy, bind it to the error type, and thread `bound` into each arm:

```python
_caller = stamina.AsyncRetryingCaller(attempts=5, timeout=30.0, wait_max=3.0)
_retry = _caller.on(TransientError)

async def arm_a(payload: A) -> Result[R, E]:
    raw = await _retry(transport.fetch_a, payload)
    return Ok(parse_a(raw))

async def arm_b(payload: B) -> Result[R, E]:
    raw = await _retry(transport.fetch_b, payload)
    return Ok(parse_b(raw))
```

- `RetryingCaller` (sync) and `AsyncRetryingCaller` share `BaseRetryingCaller.__repr__` which lists all config keys alphabetically — useful for diagnostic logging of the active retry policy on dispatch startup.

---

## [MEMORY_STREAM_BACKPRESSURE_FAILURE_MODES_AND_BROKEN_RECEIVE_END]

[BROKEN_RESOURCE_CONTRACT]:
- `MemoryObjectSendStream.send_nowait` raises `BrokenResourceError` (not `WouldBlock`) when there are no open receive channels — all receive-end clones have been closed. This is distinct from `WouldBlock` (buffer full, receivers exist but are not ready). Calling `send_nowait` after `recv_stream.close()` always raises `BrokenResourceError`; calling it with a full buffer and live receivers raises `WouldBlock`. A dispatch arm that produces to a closed consumer should convert `BrokenResourceError` to `Error(ConsumerClosed(...))` at the boundary. [`anyio/streams/memory.py` lines 205–232, anyio 4.13.0, 2026-06-09]
- When the last receive-stream clone closes (either via `aclose()` or `close()`), the implementation iterates `self._state.waiting_receivers`, clears the dict, and sets all pending receive events — causing waiting senders to wake with their send event still in the dict, after which they raise `BrokenResourceError from None`. This means `await send_stream.send(item)` can raise `BrokenResourceError` mid-await if the receive end closes while a sender is blocked. [`anyio/streams/memory.py` lines 149–163, 255–262, anyio 4.13.0, 2026-06-09]
- Symmetrically, when the last send-stream clone closes, waiting receivers get their `receive_event` set and then reach `receiver.item` — which was never assigned — and raise `EndOfStream from None`. The `async for item in recv_stream:` pattern handles this automatically because `EndOfStream` is the sentinel that terminates `ObjectReceiveStream.__aiter__`. [`anyio/streams/memory.py` lines 113–132, anyio 4.13.0, 2026-06-09]
- `MemoryObjectStreamStatistics` (a `NamedTuple`) exposes `current_buffer_used`, `max_buffer_size`, `open_send_streams`, `open_receive_streams`, `tasks_waiting_send`, `tasks_waiting_receive`. `recv_stream.statistics()` is the diagnostic surface for backpressure audits without entering the stream. [`anyio/streams/memory.py` lines 30–40, anyio 4.13.0, 2026-06-09]

---

## [CANCEL_SCOPE_CANCEL_REASON_AND_TASKGROUP_SCOPE_ACCESS]

[CANCEL_WITH_REASON]:
- `CancelScope.cancel(reason: str | None = None)` attaches a human-readable reason string to the cancellation. The `reason` is accessible via `scope.cancel_called` (bool) but the reason string itself is not exposed on the public `CancelScope` API — it is propagated into the backend's internal representation for debugging. `tg.cancel_scope.cancel("backpressure limit reached")` is the correct form for a dispatch surface that cancels remaining arms on a backpressure condition. [`anyio/_core/_tasks.py` lines 35–42, anyio 4.13.0, 2026-06-09]
- `TaskGroup.cancel_scope` is a public attribute of type `CancelScope`. Cancelling the group's scope (`tg.cancel_scope.cancel()`) is the correct way to abort all remaining arms from within the group after one arm's `Result` already confirms that further work is unnecessary (early-exit fan-out). This is distinct from raising inside an arm task — raising triggers `BaseExceptionGroup` propagation; cancelling the scope triggers orderly structured-concurrency cancellation. [`anyio/abc/_tasks.py` lines 42–56, anyio 4.13.0, 2026-06-09]

[EARLY_EXIT_FAN_OUT]:
```python
results: list[Result[T, E] | None] = [None] * len(arms)

async def arm_task(idx: int, arm: Arm, tg: TaskGroup) -> None:
    results[idx] = r = await arm.execute()
    match r:
        case Ok(value) if is_definitive(value):
            tg.cancel_scope.cancel("definitive result found")

async with anyio.create_task_group() as tg:
    for idx, arm in enumerate(arms):
        tg.start_soon(arm_task, idx, arm, tg)
```

---

[SELF_GRADE]:
- Richness: 9.5 — nine distinct sub-topics absent from waves 1-2: `BlockingPortal`/`BlockingPortalProvider`, `from_thread.check_cancelled`, `RunVar`, `ResourceGuard`, adapter-pattern pre-loop construction + bounded `Semaphore` + `Condition.wait_for`, backoff hook `float`/`timedelta` return, `RetryHook` returning `AbstractContextManager`, `set_testing(cap=)` semantics, `AsyncRetryingCaller`/`BoundAsyncRetryingCaller` as reusable components, `BrokenResourceError`/`EndOfStream` failure-mode taxonomy, `MemoryObjectStreamStatistics`, `cancel(reason=)` + early-exit fan-out via `tg.cancel_scope`.
- Veracity: 9.5 — every claim traced to a specific file + line range in installed source; no training-data assertions; version and date cited at document header; `check_cancelled` behavior, `BrokenResourceError` wake mechanism, `cap=` distinction, and `RetryHook` CM lifecycle all verified against installed source.
- Density: 9.4 — code blocks used only where the shape is non-obvious; table avoided where prose is denser; no explanatory prose for patterns the reader can derive from the contract.
- Advancement: 9.4 — `BlockingPortalProvider` ref-counting, `RunVar` vs `ContextVar` scoping, `ResourceGuard` exclusive-access boundary, `Condition.wait_for` predicate dispatch, backoff hook custom-float return and test-mode zeroing, `RetryHook` span-CM lifecycle with `_cms_to_exit`, `cap=True` semantics, `BoundAsyncRetryingCaller` as module-level shared retry policy, `BrokenResourceError` mid-await wake path, and early-exit fan-out via `tg.cancel_scope.cancel(reason)` are each niche, source-verified, and absent from prior waves.
