# [H1][AOT_STAMINA]
>**Dictum:** *`@retried` is the sole retry surface: a `stamina`-driven, predicate-scoped decorator that re-executes only a transient spawn or probe on the exception channel, never a busy lease, a tool verdict, or a cancellation.*

`stamina` 26.1.0 (verified upstream): `retry(*, on, attempts=10, timeout=45.0, wait_initial=0.1, wait_max=5.0, wait_jitter=1.0, wait_exp_base=2)`. `on` is `type[Exception] | tuple[type[Exception], ...] | Callable[[Exception], bool | float | timedelta]` with **no default** (explicit by design — `on=Exception` is an attractive nuisance). Backoff per attempt `n`: `min(wait_max, wait_initial * wait_exp_base**(n-1) + random(0, wait_jitter))`. `attempts` and `timeout` combine; `timeout` is the **total** wall-clock budget over attempts + backoff (`None` disables, `0` warns). **If all retries fail, the last exception is let through** — confirmed. Async uses the identical API (asyncio + Trio ⇒ anyio-native). Type hints of the wrapped callable are preserved.

---
## [1][DECORATOR_FACTORY]
>**Dictum:** *One factory parameterized by policy; the predicate, not a `try` ladder, decides retryability.*

```python
type Spawn[**P, T] = Callable[P, Coroutine[None, None, T]]   # raises transient; below the rail
_TRANSIENT: tuple[type[Exception], ...] = (ConnectionError, TimeoutError, BrokenPipeError, OSError)

def _transient(exc: Exception) -> bool:                      # on-hook: True ⇒ retry this attempt
    return isinstance(exc, _TRANSIENT) and not isinstance(exc, ResourceBusyError)

def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0,
                    wait_initial: float = 0.1, wait_max: float = 5.0, wait_jitter: float = 1.0) -> Layer[P, T]:
    dec = stamina.retry(on=on, attempts=attempts, timeout=timeout,
                        wait_initial=wait_initial, wait_max=wait_max, wait_jitter=wait_jitter)
    return (Slot.retried, dec)   # wraps Spawn (raises) below the Hom rail; @wraps-transparent, ty sees P, T
```

- **`on=_transient`** is the precise scope: `OSError` (`EAGAIN`/`ENOMEM` on `exec`), `ConnectionError` (bridge client to a not-yet-ready RhinoWIP socket), `TimeoutError`/`BrokenPipeError` (probe). `ResourceBusyError` is excluded **defensively** even though it can never arrive as an exception (see §2).
- **`timeout`** is the *outer* budget; per-attempt `anyio.fail_after` inside the spawn is *inner* (`@deadline outer ▷ @retry inner ⇒ total budget`, per `decorators.md`). A new policy is a parameter change on this one factory — `wait_jitter` widens the desync window; never a hand-rolled loop.
- **`BaseException`/cancellation excludes itself by type**: anyio's cancelled exc derives from `BaseException`, so the `Exception`-typed `on` predicate is never invoked for it; it propagates inward intact (`decorators.md`: wrappers must never catch `Cancelled`).

---
## [2][TWO_CHANNEL_RECONCILIATION]
>**Dictum:** *`stamina` lives on the exception channel; the domain lives on `Result`. Exactly one marked adapter in `run_check` bridges them, lifting an exhausted exception to `Error(Fault)`.*

The raising `Spawn` is the only thing `@retried` sees. Above it, one boundary adapter — the sole sanctioned `try/except` — converts the surviving transient into a typed `Fault`; everything above stays `Hom = Callable[P, Result[Completed, Fault]]`.

```python
async def _spawn(check: Check, settings: AssaySettings) -> Completed:   # raises _TRANSIENT on exec/probe failure
    with anyio.fail_after(check.tool.timeout):                          # inner per-attempt deadline
        return await _capture_or_stream(_argv(check), ...)             # run_process(check=False) ⇒ non-zero rc is data

_run_spawn = compose(checked(), traced(span=f"assay.check.{tool}", attrs=_check_attrs), retried())(_spawn)

async def run_check(check: Check, *, settings: AssaySettings) -> Result[Completed, Fault]:
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason="exception→Result seam for stamina exhaustion" ...
    try:
        return Ok(_classify(await _run_spawn(check, settings)))         # rc→status: 0/5/124/else as Result
    except _TRANSIENT as exc:                                           # only the exhausted transient lands here
        return Error(Fault.spawn(_argv(check), exc, status=RailStatus.FAILED))
```

| [SIGNAL] | [CHANNEL] | [RETRIED?] | [WHY] |
| -------- | --------- | :--------: | ----- |
| Transient spawn/probe (`OSError`/`ConnectionError`/`TimeoutError`) | exception | **yes** | The one class `stamina` exists for; `_transient` returns `True`. |
| Busy exclusive lease | `Result` (`Error(Fault, status=BUSY, rc=5)`) | **no** | `flock(LOCK_NB)` maps `BlockingIOError→ResourceBusyError→Fault` *before* the spawn; a value never reaches `stamina`. Retrying would hammer/block. |
| Non-zero tool exit | `Result` (`Ok(Completed)`, `from_returncode→FAILED`) | **no** | `run_process(check=False)` returns normally; a verdict is data, not an exception. |
| Tool-runtime `TIMEOUT` (rc 124) | `Result` (`Error(Fault, status=TIMEOUT)`) | **no** | A legitimately slow analyzer is a verdict; only a *spawn/probe* hang raises. |
| Cancellation | `BaseException` | **no** | Not an `Exception`; predicate never sees it; propagates to teardown (`kill` + shielded `wait`). |

---
## [3][STACK_POSITION]
>**Dictum:** *One span over all attempts; per-attempt visibility rides `stamina`'s own hook into that span and into `structlog`.*

Runtime order `checked → logged → traced → retried → _spawn` (`aspect.md` §2; `Slot.retried = 3` innermost via `compose`'s `reverse=True` sort). `@traced` **outside** `@retried` is load-bearing: `@trace @retry` emits **one** `assay.check.<tool>` span enclosing all attempts; the reverse would emit N sibling spans. Per-attempt evidence is sourced from `stamina`, not from extra layers:

- **`@logged`** emits the single terminal `info(Ok)`/`error(Error)` event for the `Check` outcome (stderr only).
- **Per-attempt** retries are logged by `stamina`'s default `StructlogOnRetryHook` (auto-active when `structlog` imports) at `warning` with `caused_by`, `retry_num`, `wait_for`, `waited_so_far` — inheriting the rail's bound `contextvars`.
- **Trace fusion**: register one `RetryHook` (or `RetryHookFactory`) via `set_on_retry_hooks` that calls `span.add_event("retry", {retry_num, wait_for, error_type})` on the current `@traced` span and increments `retry.attempts`; a `RetryHook` may *return a context manager* entered at schedule and exited before the attempt, so the backoff sleep can be a span event without a child span. `PrometheusOnRetryHook` (`stamina_retries_total{callable,retry_num,error_type}`) is auto-active when `prometheus-client` imports — RED retry metrics for free.

---
## [4][WHERE_RETRY_APPLIES]
>**Dictum:** *Retry the spawn and the probe; never the fold.*

| [SEAM] | [`@retried`?] | [JUSTIFICATION] |
| ------ | :-----------: | --------------- |
| `run_check` spawn (`open_process`/`run_process` exec) | **yes** | A failed exec is a transient OS condition (`EAGAIN`/`ENOMEM`). |
| Restore / health probe (bridge connect, `RhinoWIP` socket readiness) | **yes** | `ConnectionError`/`TimeoutError` on a not-yet-ready endpoint is the canonical retryable. |
| Rail runner (`composition/registry.py`) | **no** | A rail is a fold over many `Check` rows, not a transient spawn; retrying re-runs the whole claim. It holds context + the parent trace only (`checked, logged, traced`). |
| Held exclusive lease | **no** | Already a `BUSY` `Result` at exit 5; orchestrator-level concern, not in-process retry. |

---
## [5][NEW_RETRYABLE_OPERATION]
>**Dictum:** *A new retryable becomes a `@retried` application with a tuned policy — never an inline loop.*

1. Express the operation as a raising `Spawn[**P, T]` that lets its transient escape (no internal `except`).
2. Wrap it: `compose(..., retried(on=_transient, attempts=N, timeout=B, wait_jitter=J))(spawn)`.
3. Surface the surviving exception to `Result` at exactly one marked adapter (§2 pattern).
4. Tune via factory parameters only. Prefer `@retried` for the **exception** channel; reserve `decorators.md`'s `bounded_retry` strictly for *Result-aware* layers that must carry a typed `Exhausted[E]` rail (not the spawn). `set_testing(True, attempts=1)` (context manager) collapses retries deterministically in specs; `set_active(False)` disables globally.

---
## [6][VERDICT_AND_OPEN_DECISIONS]
>**Dictum:** *Adopt the predicate-scoped `stamina` decorator at `run_check` only; defer policy granularity to data.*

**Verdict — ADOPT.** `stamina` already owns transient-retry-with-jitter, async parity, signature preservation, and structlog/Prometheus instrumentation; `@retried` is a thin `Slot.retried` layer with `on=_transient` and a single exception→`Result` adapter. No bespoke retry loop is justified. The predicate is the entire correctness surface: it retries spawn/probe transients and structurally cannot retry `BUSY` (a `Result`), a tool verdict (a `Result`), or cancellation (a `BaseException`).

**Open decisions.** (1) **Per-`Tool` policy** — add an optional `Tool.retry: RetryPolicy | None` (frozen `msgspec` row: `attempts`, `timeout`, `wait_jitter`) so a flaky bridge row tunes independently, vs. one global default; *lean*: optional field, global default when absent (keeps catalog rows uniform). (2) **Global default** — `AssaySettings.retry_*` as the fallback the factory reads, so CI can dampen jitter centrally. (3) **Jitter bounds** — default `wait_initial=0.1`, `wait_max=5.0`, `wait_jitter=1.0`; confirm `wait_max` ≤ the spawn's inner `fail_after` so a single backoff cannot consume the per-attempt deadline. (4) **Predicate width** — whether `OSError` is too broad (it subsumes `FileNotFoundError` for a missing binary, which is *not* transient); *lean*: narrow to `ConnectionError | TimeoutError | BrokenPipeError | InterruptedError` plus `OSError` filtered on `errno in {EAGAIN, ENOMEM, ECONNREFUSED}`.
