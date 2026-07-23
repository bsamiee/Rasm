# [PY_RUNTIME_API_STAMINA]

`stamina` mints the production retry layer over `tenacity`: a `@retry` decorator and a `retry_context` iterator share one keyword policy schema, reusable sync/async callers bind the retryable exception set through `.on(...)`, exponential backoff with jitter and a wait cap schedules every attempt, and a context-manager-capable on-retry hook surface feeds the observability rail. It is the resilience rail's sole owner — every transient-failure boundary folds through it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stamina`
- package: `stamina` (MIT)
- module: `stamina`
- namespaces: `stamina` (callers, `retry`/`retry_context`, active/test toggles), `stamina.instrumentation` (hook protocol/factory/data, shipped emitters, register/read), `stamina.typing` (type-only `RetryDetails`/`RetryHook` re-export)
- rail: resilience
- depends: `tenacity` (scheduling engine)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: retry-caller family
- `RetryingCaller`/`AsyncRetryingCaller` subclass `BaseRetryingCaller`; sync/async is the runtime axis, bound/unbound the exception-binding axis. `.on(exc_or_hook)` returns a `Bound*` caller — a distinct class, never a `functools.partial`, so the `(callable, *args, **kw)` signature stays precisely typed. Every caller is reusable: each `__call__` opens a fresh `retry_context`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `RetryingCaller`           | caller        | reusable sync caller; `__call__(on, fn, /, *a, **kw) -> T`          |
|  [02]   | `AsyncRetryingCaller`      | caller        | reusable async caller; `__call__(on, afn, /, *a, **kw) -> T`        |
|  [03]   | `BoundRetryingCaller`      | caller        | sync caller pre-bound to an `on` target via `RetryingCaller.on`     |
|  [04]   | `BoundAsyncRetryingCaller` | caller        | async caller pre-bound via `AsyncRetryingCaller.on`                 |
|  [05]   | `Attempt`                  | attempt       | per-iteration context yielded by `retry_context`; `num`/`next_wait` |

[PUBLIC_TYPE_SCOPE]: retry-target discriminator
- `on=` discriminates a single exception class, a tuple of classes, or a backoff hook: the hook receives the raised `Exception` and returns `bool | float | timedelta`, where `True`/`False` decides retry and a `float`/`timedelta` both decides yes and overrides the computed backoff for that attempt (the `Retry-After` pattern), bypassing the `wait_*` machinery. `ExcOrBackoffHook`/`BackoffHook` are shapes, not importable symbols — they live in private `stamina._core`, so a consumer spells the union as a local `type` alias.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `ExcOrBackoffHook` | type alias    | `type[Exception] \| tuple[type[Exception], ...] \| BackoffHook` |
|  [02]   | `BackoffHook`      | type alias    | `Callable[[Exception], bool \| float \| timedelta]`             |

[PUBLIC_TYPE_SCOPE]: instrumentation family (`stamina.instrumentation`)
- `RetryHook` is a `Protocol` callable `(RetryDetails) -> None | AbstractContextManager[None]`; a returned context manager spans the scheduled wait, entered when the retry is scheduled and exited right before it runs. `RetryHookFactory` wraps a `Callable[[], RetryHook]` so hook construction defers to the first scheduled retry; the three shipped emitters are `RetryHookFactory` instances.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------- | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `instrumentation.RetryHook`             | `Protocol`         | `(RetryDetails) -> None \| AbstractContextManager[None]`          |
|  [02]   | `instrumentation.RetryHookFactory`      | frozen dataclass   | wraps `hook_factory: Callable[[], RetryHook]` for lazy build      |
|  [03]   | `instrumentation.RetryDetails`          | frozen dataclass   | per-retry payload passed to every hook                            |
|  [04]   | `instrumentation.StructlogOnRetryHook`  | `RetryHookFactory` | structlog `stamina` logger warning emitter (default if installed) |
|  [05]   | `instrumentation.LoggingOnRetryHook`    | `RetryHookFactory` | stdlib-`logging` retry emitter                                    |
|  [06]   | `instrumentation.PrometheusOnRetryHook` | `RetryHookFactory` | Prometheus `stamina_retries_total` counter increment              |

[PUBLIC_TYPE_SCOPE]: `RetryDetails` fields
- Every hook reads one `RetryDetails`; each field maps onto a receipt slot un-re-derived.

| [INDEX] | [FIELD]         | [TYPE]               | [MEANING]                                             |
| :-----: | :-------------- | :------------------- | :---------------------------------------------------- |
|  [01]   | `name`          | `str`                | qualified name of the retried callable                |
|  [02]   | `args`          | `tuple[object, ...]` | positional args passed to the callable                |
|  [03]   | `kwargs`        | `dict[str, object]`  | keyword args passed to the callable                   |
|  [04]   | `retry_num`     | `int`                | retry attempt number; starts at 1 after first failure |
|  [05]   | `wait_for`      | `float`              | seconds stamina waits before the next attempt         |
|  [06]   | `waited_so_far` | `float`              | cumulative seconds waited across prior attempts       |
|  [07]   | `caused_by`     | `Exception`          | the exception that triggered this retry               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: retry operations
- schema carry: `on` (required), `attempts=10`, `timeout=45.0`, `wait_initial=0.1`, `wait_max=5.0`, `wait_jitter=1.0`, `wait_exp_base=2`; `timeout`/`wait_*` accept `float | timedelta`.
- Backoff for attempt `n` is `min(wait_max, wait_initial * wait_exp_base**(n-1) + uniform(0, wait_jitter))`. `attempts=None`/`timeout=None` lift the respective stop condition, and at least one must bound the loop. `retry` auto-detects the sync/async/generator/async-generator callable; `retry_context` yields `Attempt` and drives both `for`/`with` and `async for`/`with`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]   | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------------- | :-------- | :-------------------------------------------------------- |
|  [01]   | `retry(*, on, …schema)`                                      | decorator | wrap sync/async/(async)gen callable on `on`               |
|  [02]   | `retry_context(on, …schema)`                                 | iterator  | retry inline block; yields `Attempt` per iteration        |
|  [03]   | `RetryingCaller(…schema)`                                    | ctor      | construct reusable sync caller (policy frozen)            |
|  [04]   | `AsyncRetryingCaller(…schema)`                               | ctor      | construct reusable async caller                           |
|  [05]   | `RetryingCaller.on(on)` / `AsyncRetryingCaller.on(on)`       | binding   | bind the `on` target -> `Bound*` caller                   |
|  [06]   | `Attempt.num` / `Attempt.next_wait`                          | property  | current attempt number; jitter-less next-wait lower bound |
|  [07]   | `is_active` / `set_active(bool)`                             | toggle    | enable/disable retrying process-globally                  |
|  [08]   | `is_testing` / `set_testing(bool, *, attempts=1, cap=False)` | toggle    | deterministic test mode (collapse backoff, cap attempts)  |

[ENTRYPOINT_SCOPE]: instrumentation operations (`stamina.instrumentation`)
- `set_on_retry_hooks(None)` restores the lazy default set and `set_on_retry_hooks(())` deactivates instrumentation; hooks set once, process-globally.
- Availability computes the default set — `PrometheusOnRetryHook` joins when `prometheus_client` imports, and `StructlogOnRetryHook` XOR `LoggingOnRetryHook` joins (structlog when importable, else the stdlib fallback). `set_on_retry_hooks` accepts `RetryHook` callables and/or `RetryHookFactory` instances; `get_on_retry_hooks()` returns the finalized `tuple[RetryHook, ...]` with factories already executed.

| [INDEX] | [SURFACE]                                   | [SHAPE]         | [CAPABILITY]                                          |
| :-----: | :------------------------------------------ | :-------------- | :---------------------------------------------------- |
|  [01]   | `instrumentation.set_on_retry_hooks(hooks)` | instrumentation | register hooks/factories (`None` restores default)    |
|  [02]   | `instrumentation.get_on_retry_hooks()`      | instrumentation | read active `tuple[RetryHook, ...]`                   |
|  [03]   | `instrumentation.get_prometheus_counter()`  | instrumentation | the `Counter \| None` backing `PrometheusOnRetryHook` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every transient-failure boundary folds through `@retry` (whole callable) or a `retry_context` block (inline); a hand-rolled `sleep` loop is the deleted form.
- A fixed-policy target reused across sites is one `RetryingCaller`/`AsyncRetryingCaller`; `.on(exc)` pre-binds the exception set so the call site passes only the callable and arguments.
- `on` names the exact retryable exception set or hook; a non-transient fault surfaces immediately as `Error(BoundaryFault(...))`, never retried.
- Server-directed delay rides the `on=` backoff hook returning a `float`/`timedelta`, never a parallel sleep path; a hook returning `False` makes the error non-retryable inline.

[STACKING]:
- `structlog`(`.api/structlog.md`) + `opentelemetry-api`(`.api/opentelemetry-api.md`): `set_on_retry_hooks((StructlogOnRetryHook, PrometheusOnRetryHook))` routes each on-retry signal into the structlog `stamina` bound-logger warning and the `stamina_retries_total` counter in one registration; a `RetryHook` returning an `AbstractContextManager[None]` opens a `start_as_current_span` child span around the scheduled wait, so every retry is one span carrying `RetryDetails`.
- `trio`(`.api/trio.md`) / `anyio`(`.api/anyio.md`): async `retry_context`/`AsyncRetryingCaller` sniffio-detect the running loop and sleep through the matching `trio.sleep`/`anyio.sleep` checkpoint, so an enclosing `move_on_after`/`fail_after` preempts a retry storm and a `Cancelled` cuts the wait.
- runtime receipt owner: `RetryDetails` maps field-for-field onto the receipt fact stream — `caused_by`/`retry_num`/`wait_for`/`waited_so_far` are recorded slots the receipt owner consumes, never re-derived.

[LOCAL_ADMISSION]:
- Every fallible I/O boundary in the lane and transport surfaces composes `retry`/`retry_context`; no sibling stands up a second retry loop.
- Deterministic specs call `set_testing(True)` to collapse backoff and cap attempts; production code never branches on test mode.

[RAIL_LAW]:
- Package: `stamina`
- Owns: retry policy, exponential-backoff scheduling with jitter/cap, custom per-error backoff, reusable retrying callers with exception binding, context-manager-capable on-retry instrumentation
- Accept: `@retry`/`retry_context` boundaries sharing the keyword schema, reusable `*RetryingCaller` with `.on(...)` binding, `ExcOrBackoffHook` targets, registered `RetryHook`/`RetryHookFactory` hooks, `set_testing` in specs
- Reject: manual retry loops, hand-coded backoff delays, blanket exception retrying, unbounded loops (`attempts` and `timeout` both `None`), duplicated retry logging, a second retry or backoff implementation
