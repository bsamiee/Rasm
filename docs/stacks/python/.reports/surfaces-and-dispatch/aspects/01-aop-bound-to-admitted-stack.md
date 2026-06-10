# [AOP_ASPECTS_ON_ADMITTED_STACK]

[ASPECT_TO_LIBRARY]:
- Bind each cross-cutting concern to its admitted library, never a hand-roll: retry and backoff -> `stamina.retry`; retry telemetry -> `stamina.instrumentation.StructlogOnRetryHook` + `set_on_retry_hooks`; tracing -> `opentelemetry.trace.get_tracer` + `start_as_current_span`; structured log -> `structlog.get_logger` + `contextvars.bind_contextvars`; runtime contract -> `beartype` + `BeartypeConf`; boundary validation -> `pydantic.validate_call`; memoization -> `functools.cache` / `lru_cache`; timeout and scope -> `anyio.fail_after` / `move_on_after`; concurrency bound -> `anyio.CapacityLimiter`; registration -> a typed row; policy and capability -> a `frozendict` table. [stamina 26.1.0, structlog 25.5, opentelemetry 1.41, beartype 0.23.0, anyio 4.13.0, pydantic 2.13.4, installed, 2026-06-09]
- The distinguishing law: a real aspect changes what executes, what is recorded, or what the domain receives; a marker or erased wrapper that only mutates metadata earns no aspect status.

[VERIFIED_LIB_APIS]:
- `@stamina.retry(on=ExcOrBackoffHook, attempts=10, timeout=45.0, wait_initial=, wait_max=, wait_jitter=, wait_exp_base=)` works sync and async (it detects `iscoroutinefunction` at decoration) and preserves `P` and the return type; `on` may return a float to OVERRIDE the backoff (e.g. a `Retry-After` value); `RetryingCaller` / `AsyncRetryingCaller` are the imperative forms; `set_testing(True, attempts=1)` disables backoff for tests; after exhaustion the last exception RE-RAISES (`reraise=True`). [stamina/_core.py 26.1.0, 2026-06-09]
- `functools.wraps` in 3.15 adds `__annotate__` (PEP 649) and `__type_params__` (PEP 695) to the default `assigned`; an aspect that overrides `assigned` with a pre-3.13 tuple silently drops generic metadata and breaks `get_type_hints` and beartype on the wrapped callable — never override the default. [functools 3.15.0b1, 2026-06-09]
- `anyio.fail_after(delay)` and `move_on_after(delay)` are SYNC context managers used inside `async def`; `fail_after` raises stdlib `TimeoutError`, `move_on_after` exits silently and exposes `scope.cancelled_caught`. [anyio 4.13.0, 2026-06-09]
- The OpenTelemetry-to-structlog bridge needs no extra package: bind `trace_id` and `span_id` from `trace.get_current_span().get_span_context()` into `bind_contextvars` inside the observability aspect.

[COMPOSITION_LAW]:
- Definition-time materialization: every heavy object (tracer, bound logger, cache, pydantic schema, beartype check, retry policy) is built in the decorator-factory body, never per call; the inner callable is a thin projection over resolved state.
- Rails-not-raises: the aspect catches the library's native exception and lifts it — `stamina` exhaustion to `Error(RetryExhausted)`, `BeartypeCallHintViolation` to `Error(ContractViolation)`, `ValidationError` to `Error(ValidationFailure)`, `TimeoutError` to `Error(Timeout)` — so domain logic only sees `Result`.
- Deterministic stack order, innermost to outermost: `@beartype` (contract on typed values) -> cache (pure interior) -> observability (span and log) -> retry -> `@validate_call` (boundary coercion); registration is innermost when present.
- Cache-outside-the-rail hazard: a `@cache` decorating a `Result`-returning function caches `Error` permanently; `@cache` must wrap the pure `T`-returning interior and the rail is applied outside it.
- Two to four ad-hoc wrappers (retry-only, span-only, timeout-only, full) sharing construction collapse into ONE parameterized aspect factory keyed by a `Concern` discriminant (never a `str`), returning `Result`.

[OPEN_GAP]:
- `beartype` and `pydantic.validate_call` model orthogonal concerns (pure contract versus coercion) and must not be stacked on the same function; `StructlogOnRetryHook` lazily binds its logger at first retry, so structlog must be configured before `set_on_retry_hooks`.
