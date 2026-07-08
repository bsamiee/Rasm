# [PY_RUNTIME_API_STAMINA]

`stamina` is the production retry layer over tenacity: a `@retry` decorator and a `retry_context` iterator that both share one keyword schema, four reusable retrying-caller objects with exception/backoff-hook binding (`.on(...)`), exponential backoff with jitter and a wait cap, a custom-backoff hook discriminator (`on=` may be a callable returning a per-error delay overriding the schedule), an `Attempt` per-iteration context exposing `num`/`next_wait`, a context-manager-capable on-retry hook surface with structlog/stdlib-logging/Prometheus emitters, and active/test toggles. It is the single resilience owner; no hand-rolled retry loops, manual `sleep` backoff, or second retry implementation survive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stamina`
- package: `stamina`
- version: `26.1.0`
- license: MIT
- import: `stamina`
- owner: `runtime`
- rail: resilience
- depends-on: `tenacity` (scheduling engine; stamina owns the typed policy face over it)
- namespaces: `stamina` (callers + `retry`/`retry_context` + active/test toggles), `stamina.instrumentation` (hook protocol/factory/data + shipped emitters + register/read entries), `stamina.typing` (type-only re-export of `RetryDetails`/`RetryHook` for annotating hooks without importing the instrumentation subpackage)
- capability: retry decorator + inline retry-context iterator sharing one keyword schema, reusable sync/async retrying callers with exception/backoff-hook binding, exponential backoff with jitter and cap, custom per-error backoff hook, context-manager-capable on-retry instrumentation hooks (structlog/logging/Prometheus), active/test toggles
- version-note: `stamina.__version__` resolves lazily via `importlib.metadata.version("stamina")` through module `__getattr__`; no eager attribute.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: retry-caller family
- rail: resilience
- `RetryingCaller`/`AsyncRetryingCaller` subclass `BaseRetryingCaller` (which freezes the policy keyword set into a `**`-passable dict); the two `Bound*` callers are NOT subclasses — each wraps a parent caller plus a frozen `on` target. Sync/async is the runtime axis, bound/unbound is the exception-binding axis. `.on(exc_or_hook)` returns the matching `Bound*` caller (a separate class, not a `functools.partial`, so the `(callable, *args, **kw)` signature stays precisely typed). Both bound and unbound callers are reusable: each `__call__` opens a fresh `retry_context` internally.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                              |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `RetryingCaller`           | caller        | reusable sync caller; `__call__(on, fn, /, *a, **kw) -> T`          |
|  [02]   | `AsyncRetryingCaller`      | caller        | reusable async caller; `__call__(on, afn, /, *a, **kw) -> T`        |
|  [03]   | `BoundRetryingCaller`      | caller        | sync caller pre-bound to an `on` target via `RetryingCaller.on`     |
|  [04]   | `BoundAsyncRetryingCaller` | caller        | async caller pre-bound via `AsyncRetryingCaller.on`                 |
|  [05]   | `Attempt`                  | attempt       | per-iteration context yielded by `retry_context`; `num`/`next_wait` |

[PUBLIC_TYPE_SCOPE]: retry-target discriminator
- rail: resilience
- The two aliases below are SHAPES, not importable symbols — they live in `stamina._core` (private) and are re-exported nowhere public (`stamina.typing` carries only `RetryDetails`/`RetryHook`); a consumer spells the union as a local `type` alias.
- The `on=` parameter is a discriminated union, not just an exception type: a single error class, a tuple of classes, OR a backoff hook. The hook receives the raised `Exception` and returns `bool | float | timedelta` — `True`/`False` decides whether to retry, a `float`/`timedelta` both decides yes AND overrides the computed exponential backoff for that attempt (the `Retry-After`-header pattern). A custom backoff bypasses the `wait_*` machinery entirely.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [DEFINITION]                                                    |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `ExcOrBackoffHook` | type alias    | `type[Exception] \| tuple[type[Exception], ...] \| BackoffHook` |
|  [02]   | `BackoffHook`      | type alias    | `Callable[[Exception], bool \| float \| timedelta]`             |

[PUBLIC_TYPE_SCOPE]: instrumentation family (`stamina.instrumentation`)
- rail: resilience
- `RetryHook` is a `Protocol`: a callable `(details: RetryDetails) -> None | AbstractContextManager[None]`. Returning a context manager makes the hook span the scheduled wait — entered when the retry is scheduled, exited right before the retry runs (the OTel-span-around-the-wait pattern). `RetryHookFactory` wraps a `Callable[[], RetryHook]` so hook construction is deferred to the first scheduled retry; the three shipped emitters are `RetryHookFactory` instances, not classes.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [RAIL]                                                            |
| :-----: | :-------------------------------------- | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `instrumentation.RetryHook`             | `Protocol`         | `(RetryDetails) -> None \| AbstractContextManager[None]`          |
|  [02]   | `instrumentation.RetryHookFactory`      | frozen dataclass   | wraps `hook_factory: Callable[[], RetryHook]` for lazy build      |
|  [03]   | `instrumentation.RetryDetails`          | frozen dataclass   | per-retry payload passed to every hook                            |
|  [04]   | `instrumentation.StructlogOnRetryHook`  | `RetryHookFactory` | structlog `stamina` logger warning emitter (default if installed) |
|  [05]   | `instrumentation.LoggingOnRetryHook`    | `RetryHookFactory` | stdlib-`logging` retry emitter                                    |
|  [06]   | `instrumentation.PrometheusOnRetryHook` | `RetryHookFactory` | Prometheus `stamina_retries_total` counter increment              |

[PUBLIC_TYPE_SCOPE]: `RetryDetails` fields
- rail: resilience
- The single fact carrier every hook reads; map these straight onto a receipt/span without re-deriving.

| [INDEX] | [FIELD]         | [TYPE]               | [MEANING]                                             |
| :-----: | :-------------- | :------------------- | :---------------------------------------------------- |
|  [01]   | `name`          | `str`                | qualified name of the retried callable                |
|  [02]   | `args`          | `tuple[object, ...]` | positional args passed to the callable                |
|  [03]   | `kwargs`        | `dict[str, object]`  | keyword args passed to the callable                   |
|  [04]   | `retry_num`     | `int`                | retry attempt number; starts at 1 after first failure |
|  [05]   | `wait_for`      | `float`              | seconds stamina will wait before the next attempt     |
|  [06]   | `waited_so_far` | `float`              | cumulative seconds waited across prior attempts       |
|  [07]   | `caused_by`     | `Exception`          | the exception that triggered this retry               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: retry operations
- rail: resilience
- `retry` and `retry_context` share one keyword schema: `on` (required), `attempts=10`, `timeout=45.0`, `wait_initial=0.1`, `wait_max=5.0`, `wait_jitter=1.0`, `wait_exp_base=2`. `timeout`/`wait_*` accept `float | timedelta`. Backoff for attempt `n` is `min(wait_max, wait_initial * wait_exp_base**(n-1) + uniform(0, wait_jitter))`. `attempts=None` and `timeout=None` lift the respective stop condition (at least one must bound the loop). `retry` auto-detects sync/async/generator/async-generator wrapped callables; `retry_context` yields `Attempt` and is consumed by both `for`/`with` and `async for`/`with`.

| [INDEX] | [SURFACE]                                                                                                        | [ENTRY_FAMILY] | [RAIL]                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `retry(*, on, attempts=10, timeout=45.0, wait_initial=0.1, wait_max=5.0, wait_jitter=1.0, wait_exp_base=2)`      | decorator      | wrap sync/async/(async)gen callable on `on`               |
|  [02]   | `retry_context(on, attempts=10, timeout=45.0, wait_initial=0.1, wait_max=5.0, wait_jitter=1.0, wait_exp_base=2)` | iterator       | retry inline block; yields `Attempt` per iteration        |
|  [03]   | `RetryingCaller(attempts=..., timeout=..., wait_*=...)`                                                          | caller build   | construct reusable sync caller (policy frozen)            |
|  [04]   | `AsyncRetryingCaller(attempts=..., timeout=..., wait_*=...)`                                                     | caller build   | construct reusable async caller                           |
|  [05]   | `RetryingCaller.on(on)` / `AsyncRetryingCaller.on(on)`                                                           | binding        | bind the `on` target -> `Bound*` caller                   |
|  [06]   | `Attempt.num` / `Attempt.next_wait`                                                                              | introspection  | current attempt number; jitter-less next-wait lower bound |
|  [07]   | `is_active` / `set_active(bool)`                                                                                 | toggle         | enable/disable retrying process-globally                  |
|  [08]   | `is_testing` / `set_testing(bool, *, attempts=1)`                                                                | toggle         | deterministic test mode (collapse backoff, cap attempts)  |

[ENTRYPOINT_SCOPE]: instrumentation operations (`stamina.instrumentation`)
- rail: resilience
- Hooks are set once, process-globally. `set_on_retry_hooks(None)` restores the lazy default set; `set_on_retry_hooks(())` (empty iterable) deactivates instrumentation entirely. The default set is computed by availability: `PrometheusOnRetryHook` is added when `prometheus_client` imports (additive), and `StructlogOnRetryHook` XOR `LoggingOnRetryHook` is added (structlog if importable, else the stdlib-logging fallback) — the two logging emitters are mutually exclusive, never both. Pass `RetryHook` callables and/or `RetryHookFactory` instances; factories construct on the first scheduled retry. `get_on_retry_hooks()` returns the finalized `tuple[RetryHook, ...]` with any factories already executed.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]  | [RAIL]                                                |
| :-----: | :------------------------------------------ | :-------------- | :---------------------------------------------------- |
|  [01]   | `instrumentation.set_on_retry_hooks(hooks)` | instrumentation | register hooks/factories (`None` restores default)    |
|  [02]   | `instrumentation.get_on_retry_hooks()`      | instrumentation | read active `tuple[RetryHook, ...]`                   |
|  [03]   | `instrumentation.get_prometheus_counter()`  | instrumentation | the `Counter \| None` backing `PrometheusOnRetryHook` |

## [04]-[IMPLEMENTATION_LAW]

[RESILIENCE_TOPOLOGY]:
- retry law: every transient-failure boundary call is wrapped with `@retry(on=..., attempts=..., timeout=...)` or a `retry_context` block; explicit retry loops with manual `sleep` are deleted.
- schema law: `retry` and `retry_context` are the same policy expressed as decorator vs iterator; choose by call shape (whole-callable vs inline block), never re-implement one in terms of a hand-rolled version of the other.
- caller law: a fixed-policy retry target reused across many call sites is a `RetryingCaller`/`AsyncRetryingCaller` built once; `.on(exc)` pre-binds the exception set so the call site passes only the callable and arguments.
- backoff law: exponential backoff with jitter and a wait cap is the default schedule expressed through `wait_initial`/`wait_max`/`wait_jitter`/`wait_exp_base`; the schedule is a parameter set, never a hand-coded delay computation.
- custom-backoff law: server-directed delays (`Retry-After`) ride the `on=` backoff-hook discriminator returning a `float`/`timedelta`, not a parallel sleep path; a hook returning `False` makes the error non-retryable inline.
- selectivity law: `on` names the exact retryable exception set or hook; non-transient faults are not retried and surface immediately as `Error(BoundaryFault(...))`.
- bound law: each loop must be bounded — at least one of `attempts`/`timeout` is non-`None`; unbounded retrying is rejected.

[LOCAL_ADMISSION]:
- The lane and transport surfaces compose `retry`/`retry_context` around fallible I/O; the runtime owns no second retry implementation. The async path auto-detects the running event loop via `sniffio` (`asyncio` or `trio`) and sleeps with the matching primitive, so an `AsyncRetryingCaller`/async `retry_context` rides whichever async runtime the transport already uses — no loop is named at the call site.
- instrumentation stacks with the observability siblings as one registration: `set_on_retry_hooks((StructlogOnRetryHook, PrometheusOnRetryHook))` routes the on-retry signal into the structlog/OTel receipt surface (`.api/structlog.md`) and the Prometheus counter in one call; a `RetryHook` returning an OTel span context manager (`opentelemetry` `.api`) wraps the scheduled wait so each retry is a child span. Per-call retry logging is never duplicated.
- `RetryDetails` maps field-for-field onto a receipt fact stream — `caused_by`/`retry_num`/`wait_for`/`waited_so_far` are recorded slots, not re-derived; the receipt owner consumes them, stamina emits them.
- deterministic specs call `set_testing(True)` to collapse backoff and cap attempts; production code never branches on test mode.

[RAIL_LAW]:
- Package: `stamina`
- Owns: retry policy, exponential-backoff scheduling with jitter/cap, custom per-error backoff, reusable retrying callers with exception binding, and context-manager-capable on-retry instrumentation
- Accept: `@retry`/`retry_context` boundaries sharing the keyword schema, reusable `*RetryingCaller` with `.on(...)` binding, `ExcOrBackoffHook` targets, registered `RetryHook`/`RetryHookFactory` hooks, `set_testing` in specs
- Reject: manual retry loops, hand-coded backoff delays, blanket exception retrying, unbounded loops (`attempts` and `timeout` both `None`), duplicated retry logging, a second retry or backoff implementation
