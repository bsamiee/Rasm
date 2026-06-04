# [H1][AOT_STRUCTLOG]
>**Dictum:** *One AOT `configure()` at package import, one `@logged` slot reading the `Result`; diagnostics own stderr, the Envelope owns stdout, and a new field is a bind, never a call site.*

Verified against `structlog==25.5.0`, Python `>=3.14`, `expression` v5.6+. APIs confirmed present: `configure`, `make_filtering_bound_logger(min_level: int|str)`, `PrintLoggerFactory(file=)`, `wrap_logger`, `get_logger`; processors `merge_contextvars`, `add_log_level`, `TimeStamper`, `dict_tracebacks`, `JSONRenderer`, `CallsiteParameterAdder`; `contextvars.{merge,bind,bound,clear}_contextvars`. Replaces the legacy inline `rail()` site (`tools/quality/__main__.py` L209-L217: `bound_contextvars` + `perf_counter` + `log.info`) and supersedes the `main.md` draft that placed `configure()` inside `main()`.

---
## [1][AOT_CONFIGURATION]
>**Dictum:** *Install the processor chain once at import; select renderer by settings; never build a logger per call.*

`structlog.configure` is pure processor-chain installation — no I/O, no write-once SDK singleton — so it is safe at package import, unlike the OpenTelemetry `Tracer/LoggerProvider` bootstrap, which stays in the `main()` imperative shell (`observability.md`: "never initialize telemetry providers at import time"). `tools/assay/__init__.py` (the sole package marker) calls `_configure(AssaySettings())` exactly once, guarded by `structlog.is_configured()`. `cache_logger_on_first_use=True` caches the bound logger after the first `get_logger`; no call site ever constructs one.

```python
# tools/assay/__init__.py  (runs once at import)
def _configure(cfg: AssaySettings) -> None:
    ci = cfg.log_format == "ci"                                   # ASSAY_LOG_FORMAT; default !sys.stderr.isatty()
    renderer = (
        structlog.processors.JSONRenderer(serializer=lambda v, **k: msgspec.json.encode(v).decode())
        if ci else structlog.dev.ConsoleRenderer(colors=True)
    )
    structlog.configure(
        processors=(
            structlog.contextvars.merge_contextvars,             # rail/verb/run_id/tool/check.id + any future bind
            structlog.processors.add_log_level,
            structlog.processors.TimeStamper(fmt="iso", utc=True),
            structlog.processors.dict_tracebacks,                # structured exc_info ONLY (bug channel), not domain Fault
            _inject_trace_identifiers,                           # trace_id/span_id from active otel span (co-seam with @traced)
            renderer,                                            # terminal; JSON (CI) vs Console (human) is the only branch
        ),
        wrapper_class=structlog.make_filtering_bound_logger(cfg.log_level),   # disabled levels compile to `return None`
        logger_factory=structlog.PrintLoggerFactory(file=sys.stderr),        # the single, pinned sink
        cache_logger_on_first_use=True,
    )
```

The renderer is the lone settings-driven branch; the chain, sink, and wrapper are fixed. `dict_tracebacks` serializes only a genuine `exc_info` (a programmer fault, e.g. a `BeartypeCallHintViolation` from `@checked`) into a structured `exception` field — it never touches a domain `Fault`, which travels as ordinary structured keys (§2).

---
## [2][LOGGED_FACTORY]
>**Dictum:** *Bind the run keys, emit start/finish from `match res`, return the same `Result` — never collapse `Error` into an exception.*

`@logged` is `Layer = (Slot.logged, dec)` with PEP 695 `**P`, `Concatenate`-free, `@wraps`-preserved, and the `id(dec)` idempotency guard from `decorators.md`. It binds `rail/verb/run_id/check.id` through `bound_contextvars` (auto-`Token.reset` on exit), reads `expression.Result`, and projects one terminal event. No `try`/`except`; `Ok`↔`Error` is never flipped.

```python
type Hom[**P, T] = Callable[P, Result[T, Fault]]
type Bind[**P]   = Callable[P, Mapping[str, object]]

def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]:        # Slot.logged = 1
    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def wrapper(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with structlog.contextvars.bound_contextvars(**keys(*a, **k)):
                log = structlog.get_logger()
                log.debug(f"{event}.start")                              # debug: free under make_filtering_bound_logger(INFO)
                res = fn(*a, **k)
                match res:
                    case Ok(report):
                        log.info(f"{event}.finish", status=report.status, exit_code=report.status.exit_code, **report.counts)
                    case Error(fault):                                   # status drives level (§6); fault is data, not raised
                        (log.error if fault.status.exit_code else log.info)(
                            f"{event}.finish", status=fault.status, exit_code=fault.status.exit_code,
                            returncode=fault.returncode, fault=fault.detail,
                        )
                return res
        return wrapper
    return (Slot.logged, dec)
```

**Stack position.** `checked(0) ▷ logged(1) ▷ traced(2) ▷ retried(3) ▷ fn`; runtime order `checked → logged → traced → retried → fn`. `logged` sits *outside* `traced` so `bound_contextvars` is live before the span opens — the span and every nested log/attempt inherit the keys, and `_inject_trace_identifiers` then folds the active `trace_id`/`span_id` into each event (`aspect.md` §2). Seam applications: rail runner `keys=_run_ctx` (`rail`,`verb`,`run_id`); per-`Check` `keys=_check_ctx` (`tool`,`check.id`) — optional/debug, since the otel span already carries the receipt.

---
## [3][CHANNEL_DISCIPLINE]
>**Dictum:** *stderr is the only logger sink; `_emit` is the only stdout writer; a stray emission fails loudly.*

`PrintLoggerFactory(file=sys.stderr)` pins every event — `ConsoleRenderer` and `JSONRenderer` both write through that one `PrintLogger` to stderr. The sole `sys.stdout.buffer` writer is `registry._emit` (`msgspec.json.encode(envelope) + b"\n"`, `registry.md` §2). Engine subprocess bytes (`Completed.stdout/stderr`) and `Fault.message` route through `@logged`/`core/engine.py`, never the runner. Three layers prevent a stray `print`/log on stdout:

| [GUARD] | [MECHANISM] |
| ------- | ----------- |
| Lint ban | Ruff `T201`/`T203` (`flake8-print`) rejects `print`/`pprint` repo-wide; the only sanctioned stdout write is `_emit`, carrying a boundary-exemption comment. |
| Runtime sentinel | The rail runner swaps `sys.stdout` for a `_StdoutGuard` whose `write` raises unless invoked by `_emit`'s token; restored on scope exit. Any rogue write surfaces as an exit-1 `Fault`, not silent stdout pollution. |
| Invariant test | A property/golden test asserts captured stdout is exactly one decodable Envelope line and stderr holds the diagnostics (Invariant 1). |

---
## [4][CONTEXT_FANOUT]
>**Dictum:** *Bind before spawn; PEP 567 copy semantics give each Check its own correct, non-leaking binding.*

`anyio` `task_group.start_soon` runs each child under `contextvars.copy_context()` (PEP 567), so the `bound_contextvars(rail,verb,run_id)` set at the rail seam is *snapshotted* into every concurrent `Check` task before fan-out. `merge_contextvars` reads the task-local copy, so parallel checks never cross-contaminate. The engine seam then `bind_contextvars(tool=…, check_id=…)` *inside* each task; because the copy is per-task, a child mutation is invisible to siblings and to the parent.

```python
async with anyio.create_task_group() as tg:                 # rail keys already bound -> snapshotted per child
    for check in checks:
        tg.start_soon(_run_logged, check, scope)            # each task: own ctx copy; engine binds tool/check_id locally
```

**Ordering law:** a bind issued *after* `start_soon` is not seen by already-spawned tasks. Therefore run-scope keys bind at the outer `@logged` (pre-fan-out) and per-`Check` keys bind at the inner engine seam. Cancellation propagates as `BaseException` and is never caught by `@logged` (no `try`/`except`), preserving structured-concurrency teardown.

---
## [5][FIELD_AS_BIND]
>**Dictum:** *A new structured field is a key in the bind mapping; the call sites stay frozen.*

Because the finish event is fold-projected from the `Result` and `merge_contextvars` injects every context-local key into *all* events, a new field is added by extending the seam's `keys` mapping — not by adding a `log.info(...)`. Example: surfacing `language` per Check is one bind, after which start/finish and every nested span-event carry it automatically.

```python
def _check_ctx(check: Check, _scope: Scope) -> Mapping[str, object]:
    return {"tool": check.tool.name, "check_id": check.id, "language": check.tool.language}   # +1 key == +1 field everywhere
```

The chain processors (`add_log_level`, `TimeStamper`, `_inject_trace_identifiers`) are the same pattern: field injection by configuration, zero new call sites. New fields thus land as data (a bind key or a processor), preserving the "growth lands in data, not code paths" invariant.

---
## [6][VERDICT_AND_OPEN]
>**Dictum:** *Decorator + contextvars over inline logging; name the three unsettled knobs.*

**Verdict — adopt.** The legacy `rail()` inlines `bound_contextvars` + `perf_counter` + `log.info` at one site and would have to duplicate it at the engine seam. `@logged` collapses both into one slot-ordered `Layer`, applied at exactly two `compose(...)` seams, with zero log calls in rail bodies or `Tool` rows. Outcome projection via `match res` keeps the `Error`/exception boundary intact (`observability.md`: project from `Result`, never catch). The decorator+contextvars shape is strictly denser, type-transparent through `**P`, and correct under fan-out — it is the recommended construction over any inline logging.

**Open decisions.**
1. **Level routing.** `@logged` emits `error` only when `fault.status.exit_code != 0`; `status ∈ {SKIP, EMPTY}` (exit 0) downgrades to `info` to avoid false-alarm error lines. Confirm the full `RailStatus → level` table lives in one projection, not scattered conditionals.
2. **Sampling.** A CLI invocation is short-lived ⇒ no event sampling; per-`Check` `start` stays `debug`. Trace sampling defaults `ParentBased(AlwaysOn)` with an `AssaySettings.trace_sampler` escape (`aspect.md` §6).
3. **Disabled-level perf + level mutability.** `make_filtering_bound_logger(min_level)` compiles sub-threshold methods to `return None`, so debug `start` events are free at `INFO`. Caveat: `cache_logger_on_first_use=True` freezes the wrapper at import-time `cfg.log_level`; a `--verbose` meta-flag must `configure()` once *before* first `get_logger`, or accept env-only `ASSAY_LOG_LEVEL`. Decide between an agent-ergonomic flag (one early reconfigure in the meta-app) and pure-env config.
