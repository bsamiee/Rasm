# [H1][CLI_SNIPPET]
>**Dictum:** *One REGISTRY fold builds the tree; `_RAIL_LAYERS` wraps handlers; `_emit` is the sole stdout writer; leaves return `Envelope`.*

Wave 3 spine (`CRITIQUE-AOT-SNIPPETS.md` §2.iii). Verified: `cyclopts==4.16.1`, `structlog==25.5.0`, Python `>=3.14`. Structlog AOT at import via `tools/assay/__init__.py` (`aot-structlog.md` §1); pre-rail parse errors skip Envelope (`main.md` §5).

```python
# tools/assay/composition/registry.py
from __future__ import annotations
import sys, time, msgspec
from collections.abc import Callable
from dataclasses import dataclass
from functools import reduce, wraps
from itertools import groupby
from operator import attrgetter
from typing import Annotated, Final
from cyclopts import App, Parameter
from effect import result
from expression import Result
from tools.assay.core.aspect import checked, compose, logged, traced
from tools.assay.core.model import Bind, Claim, Envelope, Fault, Report
from tools.assay.composition.settings import AssaySettings, ArtifactScope

# core/model.py: Envelope.__cyclopts_returncode__() -> exit_code; resolve_returncode reads protocol

@dataclass(frozen=True, slots=True)
class StaticParams:
    paths: tuple[str, ...] = ()
    strict: bool = False

type Handler[**P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]
type Params = StaticParams  # per-verb frozen @dataclass; TID251 bans NamedTuple

_RAIL_LAYERS: Final = (checked(), logged(event="rail"), traced(span="assay.rail", attrs=lambda _s, _sc, p: {"strict": p.strict}))

def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    ms = (time.perf_counter() - started) * 1000.0
    envelope = outcome.map(
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
    sys.stdout.buffer.write(msgspec.json.encode(envelope) + b"\n")  # sole stdout writer
    return envelope

def rail(bind: Bind) -> Callable[[Params], Envelope]:
    handler = compose(*_RAIL_LAYERS)(bind.handler)

    @result.result[Envelope, Fault]()
    def run(params: Params) -> Envelope:
        settings, started = AssaySettings(), time.perf_counter()
        with ArtifactScope.open(settings, bind.claim) as scope:
            outcome = handler(settings, scope, params)
        return _emit(bind, settings, started, outcome)

    run.__name__ = bind.verb
    return run

def _leaf(bind: Bind) -> Callable[..., Envelope]:
    runner = rail(bind)
    ann = Annotated[bind.params, Parameter(name="*")]

    @wraps(runner)
    def command(params: ann = bind.params()) -> Envelope:
        return runner(params)

    command.__annotations__ = {"params": ann, "return": Envelope}
    return command

def build_app(registry: tuple[Bind, ...]) -> App:
    _PARAMETER = Parameter(show_default=False)
    root = App(name="assay", help="Rasm polyglot quality operator.", default_parameter=_PARAMETER, result_action="return_value")
    keyed = sorted(registry, key=lambda b: b.claim.value)
    subs = tuple(
        reduce(lambda app, row: app.command(_leaf(row), name=row.verb, help=row.help), tuple(rows), App(name=claim.value))
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    return reduce(lambda app, sub: app.command(sub), subs, root)

# tools/assay/__main__.py
from cyclopts import resolve_returncode
from tools.assay.composition.registry import REGISTRY, build_app

app = build_app(REGISTRY)

@app.meta.default
def meta(*tokens: str) -> int:
    result_obj = app(tokens, result_action="return_value", backend="asyncio")
    return match result_obj:
        case None: 0
        case env: resolve_returncode(env)

def main(argv: list[str] | None = None) -> int:
    return meta(*([] if argv is None else argv))

if __name__ == "__main__":
    raise SystemExit(main())
```

## [1][CONTRACT_TEST]
```python
# tests/tools/assay/test_cyclopts_contract.py
from cyclopts import resolve_returncode
from tools.assay.composition.registry import REGISTRY, build_app, _leaf
from tools.assay.core.model import Claim, Envelope, RailStatus

app = build_app(REGISTRY)

def test_registry_tree_matches_rows() -> None:
    names = {c.name for c in app._commands.values()}  # claim sub-apps
    assert names == {b.claim.value for b in REGISTRY}
    assert all(_leaf(b).__annotations__["return"] is Envelope for b in REGISTRY)

def test_help_returns_none_exit_zero() -> None:
    assert app(["--help"], result_action="return_value") is None
    assert resolve_returncode(None) == 0

def test_envelope_returncode_protocol() -> None:
    env = Envelope(claim=Claim.STATIC, verb="plan", status=RailStatus.FAILED, exit_code=1)
    assert resolve_returncode(env) == 1
    assert env.__cyclopts_returncode__() == 1

def test_flatten_params_default() -> None:
    leaf = _leaf(next(b for b in REGISTRY if b.claim == Claim.STATIC and b.verb == "plan"))
    assert leaf.__annotations__["params"].__metadata__[0].name == "*"
```

## [2][FURTHER_CONSIDERATION]
- **`resolve_returncode(int)`** returns `default=0` — leaves return `Envelope` only (`research-cyclopts-api.md` §1).
- **`cache_logger_on_first_use`:** env `ASSAY_LOG_LEVEL` before first `get_logger` (`aot-structlog.md` §6).
- **Watch NDJSON:** one Envelope line per fold (`registry.md` §6.2); `_emit` remains sole stdout writer (`CRITIQUE-CONCURRENCY.md` §5).
