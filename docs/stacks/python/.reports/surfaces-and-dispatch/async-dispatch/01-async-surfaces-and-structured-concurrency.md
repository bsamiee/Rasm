# [ASYNC_DISPATCH_AND_STRUCTURED_CONCURRENCY]

[ASYNC_SURFACE_CONTRACT]:
- The async dispatch surface is `async def dispatch(...) -> Result[T, E]` (the coroutine materializes `Awaitable[Result[T, E]]`); never annotate a bare `Awaitable[T]` or a sync `Result`. `expression.@effect.result` is a SYNC generator builder only (no async variant), so async surfaces are plain `async def` returning `Result`, with exception-to-`Result` conversion at the boundary the only permitted `try/except`. [expression/effect/builder.py 5.6.0 + anyio 4.13.0, 2026-06-09]
- Registry dispatch threads async identically: the table maps keys to `Callable[..., Awaitable[Result]]`, the dispatcher is `async def` and awaits the resolved handler, and `match` arms await the arm function. Sync and async surfaces are distinct entries — a caller knows which it holds.

[ASYNC_ASPECTS]:
- Timeout lifts to a rail at the scope level: `with anyio.move_on_after(t) as scope: value = await inner(); return Error(Timeout()) if scope.cancelled_caught else Ok(value)`; `fail_after` is the raises-`TimeoutError` variant. The `Result` wrap is at the scope level, never inside `inner`.
- `anyio.CapacityLimiter(n)` bounds concurrency (`async with limiter:` or `to_thread.run_sync(..., limiter=)`); `total_tokens` is writable. `stamina.retry` async detects the coroutine at decoration, retries via `tenacity.AsyncRetrying` with `_smart_sleep` dispatching `asyncio` or `trio` via `sniffio`, and re-raises after exhaustion (the boundary converts to `Error`). [stamina/_core.py 26.1.0, anyio 4.13.0, 2026-06-09]
- structlog and OpenTelemetry context propagate into spawned tasks via anyio's per-task `contextvars` copy at spawn time; bind tracing keys BEFORE entering the task group, and treat child mutations as isolated to the child's copy.

[FAN_OUT_DISPATCH]:
- Concurrent N-arm dispatch via `anyio.create_task_group()`: `tg.start_soon(arm, item)`, then `async with tg:` awaits all; on any arm failure the group cancels the rest and raises `BaseExceptionGroup`, collected via `except* DispatchError as eg:` into one typed aggregate. [anyio asyncio backend 4.13.0, 2026-06-09]
- Each arm's `Result` accumulates into a list pre-allocated before the group; the fold from `Sequence[Result[T, E]]` to one aggregate `Result[tuple[T, ...], AggregateError]` is the rail carrier's concern, not the dispatch surface's. A per-arm `move_on_after` prevents one slow arm from blocking the group.

[OFFLOAD_SEAM]:
- `anyio.to_thread.run_sync(fn, *args, abandon_on_cancel=False, limiter=)` offloads I/O-bound sync handlers; `anyio.to_process.run_sync(fn, *args, cancellable=False, limiter=)` offloads CPU-bound ones (it pickles args and exceptions, so handlers return `Result`). If the sync handler already returns `Result`, no exception wrap is needed; `to_thread` deprecated `cancellable=` in 4.1.0 for `abandon_on_cancel=`. [anyio 4.13.0, 2026-06-09]
- `anyio.to_interpreter` (3.14+) is a third offload lane with sub-interpreter isolation; `anyio.functools.lru_cache(maxsize, ttl)` is the async-safe memoization primitive for I/O-backed registry resolution, not `functools.lru_cache`.

[OWNERSHIP_LINE]:
- The dispatch surface owns the async return contract, the timeout-to-`Result` lift, `stamina` placement and boundary conversion, the fan-out `TaskGroup` and `except*` shape, and the `to_thread` / `to_process` offload. The concurrency runtime (backend selection, `contextvars` propagation mechanics, limiter sizing, `CancelScope.shield`) and the aggregate-`Result` carrier (`sequence` / `traverse` / `AggregateError`) are separate concerns.
