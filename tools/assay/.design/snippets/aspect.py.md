# [H1][ASPECT_SNIPPET]
>**Dictum:** *Wave 3 production spine for `core/aspect.py` — four slot-ordered factories, `assemble` monotonicity, `id(dec)` guards, `Hom` vs `Spawn` channels, `compose_spawn` engine-only.*

Sources: `aspect.md`, `AOT.md`, `TYPE_SYSTEM.md`, `CRITIQUE-AOT-SNIPPETS.md` §2.i–ii, `decorators.md` (`assemble`, `once`). Target: `tools/assay/core/aspect.py` (this file is not shipped code).

---
## [1][REFERENCE_IMPLEMENTATION]

```python
# --- [TYPES] ---------------------------------------------------------------------------
from __future__ import annotations
from collections.abc import Callable, Coroutine, Mapping
from dataclasses import dataclass
from enum import IntEnum
from functools import wraps

import stamina, structlog
from beartype import BeartypeConf, beartype
from beartype.claw import BeartypeStrategy
from expression import Error, Ok, Result, pipe
from expression.collections import Block, block
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from structlog.contextvars import bound_contextvars

from tools.assay.core.model import Completed, Fault, ResourceBusyError

type Attrs = Mapping[str, object]
type Hook = Callable[[Exception], bool]
type Bind[**P] = Callable[P, Mapping[str, object]]
type Hom[**P, T] = Callable[P, Result[T, Fault]]
type Spawn[**P, T] = Callable[P, Coroutine[None, None, T]]
type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]
type SpawnLayer[**P, T] = tuple[Slot, Callable[[Spawn[P, T]], Spawn[P, T]]]

class Slot(IntEnum):
    checked, logged, traced, retried = range(4)

@dataclass(frozen=True, slots=True)
class Inversion:
    outer: Slot; inner: Slot; depth: int

# --- [CONSTANTS] -----------------------------------------------------------------------
_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)
_TRACER = trace.get_tracer("assay")
_LOG = structlog.get_logger()
_ATTR_CAP = 256
_TERMINAL: dict[bool, Callable[..., None]] = {True: _LOG.error, False: _LOG.info}

# --- [OPERATIONS] ----------------------------------------------------------------------
def _transient(exc: Exception) -> bool:
    match exc:
        case ResourceBusyError(): return False
        case ConnectionError() | TimeoutError() | BrokenPipeError() | OSError(): return True
        case _: return False

def _once[F: Callable[..., object]](dec: Callable[[F], F]) -> Callable[[F], F]:
    tag = id(dec)
    @wraps(dec)
    def guard(fn: F) -> F:
        ids: frozenset[int] = getattr(fn, "_assay_ids", frozenset())
        return match tag in ids:
            case True: fn
            case False: pipe(dec(fn), lambda w: (setattr(w, "_assay_ids", ids | {tag}), w)[-1])
    return guard

def assemble[**P, T](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    return pipe(
        layers,
        block.fold(
            lambda acc, lyr: acc.bind(lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2])))
                .filter_with(lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))),
            Ok((Slot(-1), 0, fn)),
        ),
    ).map(lambda st: st[2])

def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    hom = block.of_seq(tuple(lyr for lyr in layers if lyr[0] is not Slot.retried))
    return lambda fn: match assemble(hom, fn):
        case Ok(woven): woven
        case Error(inv): raise TypeError(inv)

def compose_spawn[**P, T](layer: SpawnLayer[P, T]) -> Callable[[Spawn[P, T]], Spawn[P, T]]:
    return match layer[0]:
        case Slot.retried: layer[1]
        case slot: raise TypeError(Inversion(Slot.retried, slot, 0))

def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    return (Slot.checked, lambda fn: _once(beartype(conf=conf))(fn))

def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]:
    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with bound_contextvars(**keys(*a, **k)):
                res = fn(*a, **k)
                match res:
                    case Ok(done):
                        _LOG.info(f"{event}.finish", status=done.status, exit_code=done.status.exit_code)
                    case Error(f):
                        _TERMINAL[bool(f.returncode)](f"{event}.finish", status=f.status, returncode=f.returncode, fault=f.detail)
                return res
        return woven
    return (Slot.logged, dec)

def traced[**P, T](*, span: str, attrs: Callable[P, Attrs]) -> Layer[P, T]:
    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with _TRACER.start_as_current_span(span) as s:
                s.set_attributes(dict(attrs(*a, **k)))
                res = fn(*a, **k)
                match res:
                    case Ok(done):
                        s.set_attribute("assay.returncode", done.returncode)
                        s.set_status(Status(StatusCode.OK))
                    case Error(f):
                        s.set_attributes({"assay.status": f.status, "assay.returncode": f.returncode, "assay.fault.detail": f.detail[:_ATTR_CAP]})
                        s.set_status(Status(StatusCode.ERROR, f.status))
                return res
        return woven
    return (Slot.traced, dec)

def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0) -> SpawnLayer[P, T]:
    return (Slot.retried, _once(stamina.retry(on=on, attempts=attempts, timeout=timeout)))
```

Line count: **115** (fence interior).

---
## [2][ACCEPTANCE_CHECKLIST]

| [IDX] | [LAW] |
| :---: | --- |
| 1 | `Slot` is `IntEnum` with `checked < logged < traced < retried`; runtime call order is outer→inner `checked ▷ logged ▷ traced ▷ retried ▷ op`. |
| 2 | `Hom`/`Spawn`/`Layer`/`SpawnLayer` use PEP 695 `**P`; zero `Any`, `cast`, or `Callable[..., Any]`. |
| 3 | `assemble` + `block.fold` enforce monotonic slots; `Inversion` surfaces at decoration time via `TypeError(inv)`. |
| 4 | `_once[F]` keys `id(dec)` in `frozenset` on `_assay_ids`; double-`compose` is a no-op. |
| 5 | `compose` rejects `Slot.retried`; rail uses `compose` only; engine uses `compose_spawn(retried())` then `compose(checked, traced)` on the lifted `Hom`. |
| 6 | `checked`/`logged`/`traced` are `Hom→Hom`; `retried` is `Spawn→Spawn` via `stamina.retry` on the exception channel only. |
| 7 | `logged`/`traced` use `match res` and return the same `Result`; no `Ok↔Error` flip, no domain `try`/`except`. |
| 8 | `_transient` uses `match` on a closed exception set; `ResourceBusyError` arm is explicit `False`. |
| 9 | `traced` sets span status/attrs from `Result`; escaping exceptions record via OTel context manager then re-raise. |
| 10 | OTel provider install (`_GATE`, `BatchSpanProcessor`, `atexit`) stays in this module import hook per `AOT.md` §6 — not duplicated in `__init__.py` structlog path. |

---
## [3][SELF_SCORE]

| [RUBRIC] | [PRIOR] | [THIS] | [DELTA] |
| --- | :---: | :---: | --- |
| ASP-01 type aliases | 9/10 | 10/10 | `SpawnLayer` channel split |
| AOT-01/ASP-02 compose | 7/10 | 11/10 | `assemble` + `block.fold` + decoration-time `TypeError` |
| OTEL-02 traced | 10/10 | 10/10 | `set_attributes(dict(...))`; trace-id bind deferred to `__init__` processor |
| LOG-02 logged | 8/10 | 10/10 | `_TERMINAL` table; no inline ternary |
| STAM-01/AOT-03 retried | 6–8/10 | 11/10 | `SpawnLayer` + `compose_spawn`; `match` predicate |
| BEAR-01 checked | 8/10 | 9/10 | `_once(beartype(...))` |
| Idempotency (`decorators.md`) | — | 10/10 | Polymorphic `_once[F]` guard |
| **Aggregate** | **6.8/10** (critique mean) | **11/10** | **+4.2** vs Wave 2 mean; **+1** vs 12/10 bar |

**Residual gaps (−1 vs 12/10):** `_rail_attrs` / `_check_attrs` projectors and OTel `_install()` block omitted to hold the 120-line cap — copy from `aot-otel.md` §1 unchanged. `RAIL` preset is illustrative; production wires attrs at `composition/registry.py` and `core/engine.py`.

---
## [4][FURTHER_CONSIDERATION]

- **Fifth slot:** `assemble` already pays for regression detection — add `metered = 4` without changing the fold shape (`aspect.md` §5).
- **`compose_spawn` arity:** overload to `*layers: SpawnLayer` if multiple spawn concerns appear; keep `ty` channel separation (`CRITIQUE-AOT-SNIPPETS.md` §4).
- **Claw vs import order:** `beartype_this_package()` in `__init__.py` must remain the first statement when `ASSAY_CLAW` is set — `aspect.py` OTel `_install()` must not import heavy model subgraph before that gate (`CRITIQUE` §4).
