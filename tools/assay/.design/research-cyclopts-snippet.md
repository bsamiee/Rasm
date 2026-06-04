# [H1][CYCLOPTS_SNIPPET]
>**Dictum:** *One REGISTRY fold builds the tree; one Envelope owns stdout and exit; structlog never touches stdout.*

Singular copy-paste contract for `tools/assay`. Verified: `cyclopts==4.16.1`, `structlog==25.5.0`, Python `>=3.14` — `App.__call__(result_action=)`, `resolve_returncode`, `Parameter(name="*")` + frozen `@dataclass` default `= Params()`, `Envelope.__cyclopts_returncode__`, `@app.meta.default`, `PrintLoggerFactory(file=stderr)`. Canonical field names: `claim` / `report` (not `rail` / `data`). Replaces `tools/quality/__main__.py` hand-wiring and projector ladder.

---
## [1][INTEGRATION_SNIPPET]
>**Dictum:** *Runnable-shaped; ≤180 lines in this file; implement without editing `__main__` per new verb.*

```python
# tools/assay/__init__.py — structlog AOT at import; stderr-only (aot-structlog.md)
from __future__ import annotations
import sys, structlog, msgspec
from tools.assay.composition.settings import AssaySettings

def _configure(cfg: AssaySettings) -> None:
    if structlog.is_configured():
        return
    ci = cfg.log_format == "ci"
    renderer = (
        structlog.processors.JSONRenderer(serializer=lambda v, **k: msgspec.json.encode(v).decode())
        if ci else structlog.dev.ConsoleRenderer(colors=True)
    )
    structlog.configure(
        processors=(
            structlog.contextvars.merge_contextvars,
            structlog.processors.add_log_level,
            structlog.processors.TimeStamper(fmt="iso", utc=True),
            structlog.processors.dict_tracebacks,
            renderer,
        ),
        wrapper_class=structlog.make_filtering_bound_logger(cfg.log_level),
        logger_factory=structlog.PrintLoggerFactory(file=sys.stderr),
        cache_logger_on_first_use=True,
    )

_configure(AssaySettings())

# tools/assay/core/model.py — wire surface (excerpt)
class Envelope(msgspec.Struct, frozen=True, gc=False, kw_only=True, omit_defaults=False):
    schema_version: int = 1
    claim: Claim; verb: str; status: RailStatus; exit_code: int = 0
    report: Report | None = None; error: Fault | None = None
    run_id: str = ""; duration_ms: float = 0.0; notes: tuple[str, ...] = ()
    def __cyclopts_returncode__(self) -> int:
        return self.exit_code  # resolve_returncode reads this; never return bare int from leaves

# tools/assay/composition/registry.py
from collections.abc import Callable
from dataclasses import dataclass
from functools import wraps
import time
from typing import Annotated, Final
from cyclopts import Parameter
from expression import Result
from tools.assay.core.aspect import checked, compose, logged, traced
from tools.assay.core.model import Claim, Envelope, Fault, Report
from tools.assay.composition.settings import AssaySettings, ArtifactScope

@dataclass(frozen=True, slots=True)  # TID251: never NamedTuple for CLI params
class StaticParams:
    paths: tuple[str, ...] = ()
    strict: bool = False

type Handler[**P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]

class Bind(msgspec.Struct, frozen=True, gc=False):
    claim: Claim; verb: str; handler: Handler[StaticParams]; params: type[StaticParams]; help: str = ""

REGISTRY: Final[tuple[Bind, ...]] = (
    Bind(Claim.STATIC, "plan", static_rail.plan, StaticParams, "Owners, triggers, closure."),
    Bind(Claim.STATIC, "build", static_rail.build, StaticParams, "Routed restore + build + analyzers."),
)

def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    ms = (time.perf_counter() - started) * 1000.0
    return outcome.map(
        lambda report: Envelope(
            claim=bind.claim, verb=bind.verb, run_id=settings.run_id, duration_ms=ms,
            status=report.status, exit_code=report.status.exit_code, report=report, notes=report.notes,
        )
    ).default_with(
        lambda fault: Envelope(
            claim=bind.claim, verb=bind.verb, run_id=settings.run_id, duration_ms=ms,
            status=fault.status, exit_code=fault.status.exit_code, error=fault,
        )
    )

def rail(bind: Bind) -> Callable[[StaticParams], Envelope]:
    stack = compose(
        checked(),
        logged(event="rail", keys=lambda s, _sc, _p: {"claim": bind.claim, "verb": bind.verb, "run_id": s.run_id}),
        traced(span=f"assay.{bind.claim}.{bind.verb}", attrs=lambda _s, _sc, p: {"strict": p.strict}),
    )
    handler = stack(bind.handler)  # no retried at rail seam (AUDIT §3.i)

    @wraps(handler)
    def run(params: StaticParams) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        with ArtifactScope.open(settings, bind.claim) as scope:
            outcome = handler(settings, scope, params)  # boundary: catch bugs → Fault → still _emit once
        envelope = _emit(bind, settings, started, outcome)
        sys.stdout.buffer.write(msgspec.json.encode(envelope) + b"\n")  # sole stdout writer
        return envelope

    return run

def _adapt(bind: Bind) -> Callable[..., Envelope]:
    runner = rail(bind)
    annotated = Annotated[bind.params, Parameter(name="*")]
    def command(params: annotated = bind.params()) -> Envelope:
        return runner(params)
    command.__annotations__ = {"params": annotated, "return": Envelope}
    command.__name__ = bind.verb
    return command

# tools/assay/__main__.py — four seams only
from cyclopts import App, Parameter, resolve_returncode
from tools.assay.composition.registry import REGISTRY, _adapt
from tools.assay import _configure
from tools.assay.composition.settings import AssaySettings

_PARAMETER = Parameter(show_default=False)
app = App(name="assay", help="Rasm polyglot quality operator.", default_parameter=_PARAMETER, result_action="return_value")

subs: dict[Claim, App] = {}
for row in REGISTRY:
    subs.setdefault(row.claim, App(name=row.claim.value)).command(_adapt(row), name=row.verb, help=row.help)
for sub in subs.values():
    app.command(sub)

@app.meta.default
def meta(*tokens: str) -> int:
    _configure(AssaySettings())
    result = app(tokens, result_action="return_value", backend="asyncio")
    return 0 if result is None else resolve_returncode(result)

def main(argv: list[str] | None = None) -> int:
    return meta(*([] if argv is None else argv))

if __name__ == "__main__":
    raise SystemExit(main())
```

Pre-rail: Cyclopts parse/validation errors → stderr + non-zero, **no** Envelope (`main.md` §5). Post-rail: exactly one JSON line on `stdout.buffer`; all diagnostics on stderr.

---
## [2][SELF_CRITIQUE]
>**Dictum:** *5/10 vs `coding-python`; the snippet is integration truth, not production density.*

| [GAP] | [WHY 5/10] | [12/10 TARGET] |
| ----- | ---------- | -------------- |
| Imperative loops | `for row in REGISTRY` / `setdefault` | `build_app(registry) -> App` via `groupby` + one expression fold |
| `subs` mutation | Clear but not algebraic | Materialize `dict[Claim, App]` from rows without side-effect loop |
| `_adapt` closure factory | Per-row factory, not `@effect.result` | Typed `rail` as `Result[Envelope, Fault]` end-to-end; aspects as `Layer[**P, T]` only |
| `match` absent in `main` | `None` + `resolve_returncode` only | Single exit: always `resolve_returncode`; leaves never return `int` |
| Settings per invoke | `AssaySettings()` inside `rail` | `Parameter(parse=False)` + `parse_args` `ignored` inject (research-cyclopts-api.md §6) |
| No contract test | Probes are ad hoc | `tests/tools/assay/test_cyclopts_contract.py`: flatten, `__cyclopts_returncode__`, help→`None` |

**Verified probes (4.16.1):** `@dataclass(frozen=True)` + `Parameter(name="*")` + `= StaticParams()` → `app(["run","--strict"], result_action="return_value")` is `42`; `resolve_returncode(Envelope(...))` reads protocol; `--help` → `None` → `0`.

```bash
uv run python -c "import cyclopts, structlog; print(cyclopts.__version__, structlog.__version__)"
```

---
## [3][FURTHER_CONSIDERATION]
- **`resolve_returncode` on `int`:** returns `default=0`, not the int — leaves must return `Envelope`, not `int` (`research-cyclopts-api.md` §1).
- **Meta `*tokens` greed:** short flags collide; globals via `ASSAY_LOG_FORMAT` / `ASSAY_STRICT` until `_normalize_top_level_flags` is pinned (`research-cyclopts-projects.md`).
- **`cache_logger_on_first_use`:** first `get_logger` freezes level; env-only `ASSAY_LOG_LEVEL` or re-`configure` before any log (`aot-structlog.md` §6).

**Paths:** `research-cyclopts-api.md`, `main.md`, `registry.md`, `model.md`, `status.md`, `AUDIT.md` §3–§4.
