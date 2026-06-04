# [H1][ASPECT_DESIGN]
>**Dictum:** *Four slot-ordered decorators plus one `compose` carry every cross-cutting concern; rails and the engine hold zero inline trace, log, timing, or retry calls.*

Replaces the scattered `tools/quality` pattern — `rail()` binding `structlog.contextvars.bound_contextvars` + `time.perf_counter` + eight projector callables (`__main__.py` L195-L219), and ad-hoc `@beartype` (`process.py` L391) — with a single AOT decorator algebra applied at exactly two seams: **`run_check`** (per `Check`, via `compose_spawn`) and the rail runner in `composition/registry.py` (per invocation). Each decorator is a `Layer = (Slot, dec)`; `compose` / `assemble` fold layers in fixed slot order. Verified: `beartype>=0.22.2`, `structlog`, `opentelemetry-{api,sdk}`, `stamina` 26.1.

**Canonical:** [`AOT.md`](AOT.md) · [`snippets/aspect.py.md`](snippets/aspect.py.md) · [`snippets/cli.py.md`](snippets/cli.py.md) (`_RAIL_LAYERS`). Structlog/otel keys use **`claim`** (wire `Envelope.claim`), not `rail`.

---
## [1][DECORATOR_CONTRACTS]
>**Dictum:** *Three layers transform `Result` on the success/error channel and never collapse it; the fourth retries on the exception channel below the rail.*

```python
type Hom[**P, T] = Callable[P, Result[T, Fault]]            # the rail-facing shape
type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]
type Spawn[**P, T] = Callable[P, Coroutine[None, None, T]]  # raises transient; below the rail

def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]   # Slot.checked
def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]     # Slot.logged — keys project claim/verb/run_id
def traced[**P, T](*, span: str, attrs: Callable[P, Attrs]) -> Layer[P, T]  # Slot.traced
def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0) -> Layer[P, T]  # Slot.retried
```

| [ASPECT] | [WRAPS] | [LIBRARY ACTION] | [RESULT/EFFECT TRANSPARENCY] |
| -------- | ------- | ---------------- | ---------------------------- |
| `@checked` | `Hom` | `@beartype(conf=conf)` validates `P.args` and the `Result[T, Fault]` return shape once. | A `BeartypeCallHintViolation` is a *programmer* fault (contract breach), deliberately raised — not a domain `Error`. It reads the value, never rewrites `Ok`/`Error`. `conf` may set `violation_type` to keep the bug channel distinct from `Fault`. |
| `@logged` | `Hom` | `with bound_contextvars(**ctx): res = fn(...)`; `match res` to emit one `info`/`error` event; returns `res`. | Pure pass-through: binds contextvars (inherited by child tasks/spans), projects the outcome to a structured event, returns the *same* `Result`. No `try`/`except`, no `Ok↔Error` flip. |
| `@traced` | `Hom` | `with tracer.start_as_current_span(span) as s:` set attrs; `match res` → `s.set_status(Status(OK\|ERROR))`, `s.set_attribute("assay.fault.*", …)`; returns `res`. | One span per call; `Error` records fault attributes + `StatusCode.ERROR` but is returned unchanged. Exceptions are *recorded* (`record_exception`) then re-raised, never swallowed. |
| `@retried` | `Spawn` | `stamina.retry(on=on, attempts=attempts, timeout=timeout)` on the raising spawn coroutine. | Operates on the **exception** channel only. A domain `Error(Fault)` is a value, not an exception, so it flows past `stamina` untouched (never retried). A non-zero tool exit is `Ok(Completed)` and is likewise never retried. Exhaustion re-raises the last exception, lifted to `Error(Fault)` by the single boundary adapter inside `run_check` (spawn channel). |

The retry boundary is the one channel transition: `Spawn` (raises) → `@retried` (raises-if-exhausted) → marked adapter `Ok(await spawn()) except _transient → Error(Fault.spawn(...))` → `Hom`. Above that adapter every layer is `Hom → Hom`, so codomain stays `Result[T, Fault]` (decorators may *widen* `Fault` but never narrow to bare `T` or raise it).

---
## [2][STACK_ORDER_AND_COMPOSE]
>**Dictum:** *`checked ▷ logged ▷ traced ▷ retried ▷ operation`, outermost to innermost; `compose` validates monotonic slots and folds innermost-first.*

```python
class Slot(IntEnum):
    checked = 0; logged = 1; traced = 2; retried = 3   # outer → inner

def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    ordered = sorted(layers, key=lambda layer: layer[0], reverse=True)   # slot 3 applied first ⇒ innermost
    return lambda fn: reduce(lambda acc, layer: layer[1](acc), ordered, fn)
```

`reverse=True` applies the highest slot (`retried`) closest to the operation and the lowest (`checked`) last, so the runtime call order is exactly `checked → logged → traced → retried → fn`. A reused-slot duplicate is allowed (e.g. two trace enrichers); a *regression* (`retried` outside `traced`) is impossible because the input is slot-keyed and sorted, not positional. Each `dec` carries a `frozenset[int]` of `id(dec)` on the wrapper for the idempotency guard, so double-`compose` is a no-op rather than a doubled span/bind.

| [BOUNDARY] | [JUSTIFICATION] |
| ---------- | --------------- |
| `checked` outermost | Validate the *public* arguments once and fail fast on a contract breach before any span, bind, or retry cost is paid; correction relative to the `engine.md` draft (`traced ▷ … ▷ checked`), which put `@checked` innermost and thus re-ran beartype on every retry attempt. |
| `logged` ▷ `traced` | `bound_contextvars(claim, verb, run_id)` must be live *before* the span opens so the span and every nested log/attempt inherit the keys; `merge_contextvars` then injects them and the active `trace_id`/`span_id` into each event. |
| `traced` ▷ `retried` | **One** span per `Check`/rail must enclose **all** attempts; retries are span *events*/attributes (`retry.attempts`), not N sibling spans. `@trace @retry` spans the whole retry loop; the reverse would emit a span per attempt. |
| `retried` innermost | Retry must re-execute only the spawn — never the typecheck, bind, or span setup. `stamina`'s total `timeout` thus bounds spawn attempts + backoff alone. |

---
## [3][SEAM_MAP]
>**Dictum:** *The rail runner owns context + the parent trace; `run_check` owns the per-`Check` span + retry. Neither rail bodies nor `Tool` rows reference an aspect.*

| [SEAM] | [COMPOSE CALL] | [SLOTS] | [WHY THESE] |
| ------ | -------------- | ------- | ----------- |
| Rail runner (`composition/registry.py`, per invocation) | `compose(checked(), logged(event="rail", keys=_rail_keys), traced(span="assay.rail", attrs=_rail_attrs))(handler)` | checked, logged, traced | **No** `@retried`. Parent trace; see [`snippets/cli.py.md`](snippets/cli.py.md). |
| `run_check` (`core/engine.py`, per `Check`) | `compose_spawn(retried())` then `compose(checked(), traced(span="assay.check", attrs=_check_attrs))` on lifted `Hom` | checked, traced, retried | Child span under rail parent; retry on spawn only (`AOT.md` §3). |

**structlog keys** (rail seam, inherited downward): `claim`, `verb`, `run_id` (+ `status`, `exit_code` on terminal event). **stderr only** via `PrintLoggerFactory(file=sys.stderr)`; stdout is `_emit` only (`registry.md` §3).

**otel:** parent `assay.<claim>.<verb>` → child `assay.check.<tool>`; attrs `assay.claim`, `assay.verb`, `assay.run_id`, `assay.tool`, `assay.runner`, `assay.language`; outcome `assay.returncode`, `assay.status`, `retry.attempts` when retried.

---
## [4][RETRY_SEMANTICS]
>**Dictum:** *Retry a transient spawn or restore probe; never a held lease, a cancellation, or a legitimate tool failure.*

```python
def _transient(exc: Exception) -> bool:
    # busy lease and cancellation are excluded; transient spawn/probe faults retry.
    return isinstance(exc, ConnectionError | TimeoutError | BrokenPipeError | OSError) and not isinstance(exc, ResourceBusyError)
```

- **Busy lease — never retried.** The non-blocking `flock` lease maps `BlockingIOError → ResourceBusyError` (`process.py` L260-L307) and the engine surfaces it as `Error(Fault, status=BUSY, returncode=5)` — a **Result**, not an exception, so it never reaches `stamina`. The predicate additionally excludes `ResourceBusyError` defensively. Retrying a busy lease would hammer or block; both are prohibited.
- **Transient spawn/probe — retried.** A failed `open_process`/`run_process` exec (`OSError` `EAGAIN`/`ENOMEM`), a bridge client connecting to a not-yet-ready RhinoWIP socket (`ConnectionError`), or a restore probe `TimeoutError` raise; `stamina` retries up to `attempts=3` within `timeout=30.0` s with exponential backoff + jitter.
- **anyio interaction.** `stamina`'s `timeout` is the **total** wall-clock budget over attempts + backoff (outer); the per-attempt `anyio.fail_after` deadline inside the spawn is **inner** (`@deadline outer @retry inner` ⇒ total budget, per `decorators.md`). A *tool-runtime* timeout (a legitimately slow analyzer) is converted to `Error(Fault, status=TIMEOUT, returncode=124)` as a Result and is **not** retried; only a *spawn/probe* hang raises and is. Cancellation (`anyio.get_cancelled_exc_class()`) is a `BaseException`, never passed to the `Exception`-typed predicate, so it propagates inward intact — never caught, never retried.

---
## [5][EXTENSION]
>**Dictum:** *A new cross-cutting library is one new `Layer`; rails and the engine are untouched.*

Adding e.g. a RED-metrics exporter: define `metered(*, meter) -> Layer[P, T]` in `aspect.py`, assign it a `Slot` (e.g. `metered = 2` co-resident with `traced`, or a new ordinal), and add it to the relevant seam's `compose(...)` call. Because the only two seams are `run_check` and the rail runner, and rails/`Tool` rows never name an aspect, no rail body, no handler, and no catalog row changes. The decorator projects counters/histograms from the same `Result` outcome (`match res`) the other layers already inspect — fused projection, not a new pipeline pass.

---
## [6][TYPE_DISCIPLINE_AND_OPEN_DECISIONS]
>**Dictum:** *PEP 695 `**P` keeps the stack transparent to `ty`; codomain widening is explicit; the operation never disappears behind `Any`.*

- Every decorator is `Callable[[Hom[P, T]], Hom[P, T]]` (or `[Spawn] → [Spawn]` for `@retried`) with PEP 695 `**P`/`type` aliases, `Concatenate` where a layer injects a parameter, and `@wraps` so `inspect.unwrap` and `__type_params__` survive the chain. `stamina.retry` already preserves the decorated signature; `beartype` is signature-transparent. No `Any`, no `cast`, no `Callable[..., Any]` erasure — `ty` sees `P` and `Result[T, Fault]` through all four layers.
- Idempotency guard via `id(dec)` in a `frozenset[int]` on the wrapper (structural identity, not call-counting) survives reimport and concurrent decoration.

**Open decisions.** (1) **Sampling** — a CLI invocation is short-lived; default `ParentBased(AlwaysOn)` so every rail trace is complete, with an `AssaySettings.trace_sampler` escape if export volume matters. (2) **Log-level routing** — `@logged` emits `info` on `Ok`, `error` on `Error`; both to stderr. Confirm whether `Error` with `status ∈ {SKIP, EMPTY}` should downgrade to `info` to avoid false-alarm error lines. (3) **beartype `violation_type`** — keep the default raising `BeartypeCallHintViolation` (bug ≠ domain `Fault`), or set `violation_type` to a typed bug exception caught once at `__main__` and rendered as an exit-2 Envelope. (4) **`@checked` strategy** — `BeartypeStrategy.O1` (constant-time, default) is correct for the `Check`/`Report` shapes; revisit only if a deep `tuple[...]` field forces `Ologn`.
