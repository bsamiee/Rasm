# [H1][ASSAY_AOT]
>**Dictum:** *Decorators carry cross-cutting behavior; registration stays data. Four aspects, two seams, one slot-ordered compose.*

Consolidates `aot-architecture.md`, `aot-beartype.md`, `aot-structlog.md`, `aot-otel.md`, `aot-stamina.md`. Shard files retained until Wave 5.

---
## [1][DOCTRINE]

| [CONSTRUCT] | [VERDICT] |
| --- | --- |
| Aspect decorators (`checked`, `logged`, `traced`, `retried`) | **ADOPT** — two seams only |
| Registration decorators (`@tool`, `@rail`, `@parser`) | **REJECT** — `TOOLS`/`REGISTRY` tuples |
| God `@aspect` / metaclass auto-register | **REJECT** |

Facts (claim, verb, runner prefix, parser ref) are row fields. Behavior (span, bind, retry, typecheck) is aspects.

---
## [2][STACK_ORDER_DEFINITIVE]

**Outer → inner:** `checked ▷ logged ▷ traced ▷ retried ▷ operation`

| [SLOT] | [CHANNEL] | [WHY] |
| --- | --- | --- |
| `checked` (0) | bug — raises violation | Fail fast before span/bind/retry cost |
| `logged` (1) | `Result` pass-through | `bound_contextvars` before span opens |
| `traced` (2) | `Result` + record exception | One span encloses all retry attempts |
| `retried` (3) | exception only | Re-executes spawn only |

```python
class Slot(IntEnum):
    checked = 0; logged = 1; traced = 2; retried = 3

def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    ordered = sorted(layers, key=lambda l: l[0], reverse=True)
    return lambda fn: reduce(lambda acc, layer: layer[1](acc), ordered, fn)
```

---
## [3][SEAM_MAP]

| [SEAM] | [COMPOSE] | [NO] |
| --- | --- | --- |
| Rail runner | `checked()`, `logged(event="rail")`, `traced(span=..., attrs=_rail_attrs)` | `retried` |
| `Engine.run_check` | `checked()`, `traced(span=..., attrs=_check_attrs)`, `retried()` | `logged` (optional debug) |

Constant layer tuples — not `logged(bind)` per handler (`registry.md` §2).

**structlog keys:** `claim`, `verb`, `run_id` (+ outcome on terminal event). **stderr only** via `PrintLoggerFactory(file=sys.stderr)`.

**otel:** parent `assay.<claim>.<verb>` → child `assay.check.<tool>`; `retry.attempts` on span when retried.

---
## [4][BEARTYPE]

```python
_CONF: Final = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)

def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    return (Slot.checked, lambda fn: beartype(conf=conf)(fn))
```

- Production: `@checked` at seams only; **not** `beartype_this_package()` claw (import cost + over-checks inner loops).
- Claw optional as CI amplifier.
- Violation = programmer bug (exit 2), not domain `Fault`.

---
## [5][STRUCTLOG]

- `structlog.configure` once at package import (`tools/assay/__init__.py`), guarded by `is_configured()`.
- Processor chain: `merge_contextvars` → level → ISO timestamp → `dict_tracebacks` → trace id inject → JSON (CI) or Console (human).
- `@logged`: `with bound_contextvars(...): match res` → one event; returns same `Result`.

---
## [6][OPENTELEMETRY]

- Provider installed at `aspect.py` import **only when** `OTEL_EXPORTER_OTLP_ENDPOINT` set; else `NonRecordingSpan` (zero cost).
- `BatchSpanProcessor` + `atexit.shutdown` for short CLI.
- `@traced`: `start_as_current_span`; set attrs from `P`; `Status` from `match res`; never swallow `Fault`.

---
## [7][STAMINA]

```python
def _transient(exc: Exception) -> bool:
    return isinstance(exc, (ConnectionError, TimeoutError, BrokenPipeError, OSError)) and not isinstance(exc, ResourceBusyError)

def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0) -> Layer[P, T]:
    return (Slot.retried, stamina.retry(on=on, attempts=attempts, timeout=timeout))
```

- **Never retry:** `Error(Fault)` (Result), non-zero exit (`Ok(Completed)`), busy lease, cancellation (`BaseException`).
- Single `Spawn → Result` adapter inside `run_check` (marked boundary exemption).
- `timeout` = outer budget; `anyio.fail_after` = inner per attempt.

---
## [8][AOT_RULES]

1. Decorate aspects only at two seams.
2. `TOOLS`/`REGISTRY` are literal frozen tuples.
3. No god decorator; four factories max.
4. Never decorate to hide a constant.
5. `compose` once per seam; idempotency via `id(dec)` frozenset.
6. PEP 695 `**P` + `@wraps`; zero `Any` through stack.

---
## [OPEN_DECISIONS]

1. OTel sampling default `AlwaysOn` for short CLI.
2. `@logged` downgrade `SKIP`/`EMPTY` to `info`.
3. `beartype` violation → Envelope vs traceback at `__main__`.
4. Fifth aspect slot → consider full `assemble` validation.
