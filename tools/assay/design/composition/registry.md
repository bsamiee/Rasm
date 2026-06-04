# [H1][ASSAY_REGISTRY]
>**Dictum:** *One `Bind` table drives the CLI tree and the dispatch; one `rail` runner folds every claim; `_emit` is the sole stdout writer; the `Report` self-describes.*

## [1][PURPOSE]

`composition/registry.py` is the composition root (stage 10, `ARCHITECTURE.md` §14). It imports the six rail modules, `composition/catalog.py`, `composition/settings.py`, and `core/aspect.py`; owns the full `(Claim, verb) -> handler` `REGISTRY` per §13; exposes the single `rail(bind)` runner, the lone stdout writer `_emit`, and the `groupby(claim)` tree builder `build_app`. It supersedes `tools/quality/__main__.py:195` `rail[T]()` and its `data`/`evidence` re-encode ladder (D10). `self-test` attaches to the **root** `App`, outside the claim tree (D38), and folds a `REGISTRY`+`TOOLS` census (every row routable via `select()`, every parser callable on an empty `Completed`) so day-one catalog rot is caught at preflight. `_emit` carries a process-static one-Envelope guard (`_WRITES`): the first write rides stdout, any later write is a wiring defect returned as a `FAULTED` `Envelope` to stderr (runtime guard for Invariant 1). The runner stack carries **no** `@retried` — a rail is a `Hom`, not a `Spawn` (D2).

`rail(bind)`'s `run` closure **seats the invocation-scoped recent-events ring**: `token = _RING.set(deque(maxlen=16))` runs **before** the handler and is `reset` in a `finally`, so the ring is bounded to exactly one invocation. `core/aspect.ring_processor` (a structlog `Processor`, not a `Slot`) appends a compact `level:event` summary to that **same** `deque` by reference — even inside the engine's `anyio.run`, the copied context-var reference points at the identical object — so the bounded window survives the spawn boundary and is readable at envelope time. On the **Error branch only**, `_emit` calls `_distill(fault, ms)` and rides the result on `Envelope.error_context`: a `Diagnostic` carrying the recent-events ring, a `failing_step` distilled by a statement-`match` on `fault.status` (`TIMEOUT`→`timeout`, `BUSY`→`lease_busy`, `FAULTED`→`validation`/`spawn` off the ring's last event), the elapsed wall time, and a synthesized `hint`, so an agent retriages a faulted run off the wire without re-running. The success branch never distills — `error_context` stays `None` (its nested `Diagnostic` never inflates the wire).

## [2][CANONICAL_SHAPES]

This doc owns the `Handler` alias, the `_RAIL_LAYERS` stack, and the full `REGISTRY`. `Bind`/`Envelope`/`Fault`/`Report` originate in `core/model.py`; `Params` dataclasses originate in their owning rail docs; they are referenced, never re-declared.

```python
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]
```

`P` is a single `TypeVar`, never a `ParamSpec`: the third positional is one `Params` *value*, not a call signature, so `Callable[[S, A, P], R]` is the valid shape. `Bind(claim: Claim, verb: str, handler: object, params: type, help: str = "")` is a frozen `msgspec.Struct` (`core/model.md`) — `handler` is erased to `object` and `_narrow` re-narrows it to `Handler` at the seam. The rail seam stack omits `retried` (D1, D2); the layer order is `checked(0) ▷ logged(1) ▷ traced(2)`, sorted by `Slot` inside `compose`. Both `logged` and `traced` take the **shared** `_correlate` projector, so the correlation map is authored once:

```python
def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}

_RAIL_LAYERS: Final = (
    checked(),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),   # run_id + agent_task_id correlate the rail span ↔ each run_check child span
)
```

`_correlate` folds `settings.agent_context` (`{run.id, agent.task.id}`) into the one map both `logged` and `traced` bind — no separate `_agent` projector. `logged` binds the whole map into `structlog` context-vars; `traced` mirrors + namespace-normalizes it (dotted `run.id`/`agent.task.id` → `run_id`/`agent_task_id`) into OTel baggage so the parent rail span and each `run_check` child span correlate on `run_id` and the agent task id (D50, §8). An unset `agent_task_id` drops out in `traced`'s `_correlate` normalizer, so absent agent context binds nothing extra. Retry correlation closes at the **engine** seam: `traced` runs outside the engine-seam `retried` loop and binds `run_id` there even though D3 forbids `logged`, so the module-scope `stamina` `_on_retry` hook (in `core/aspect.py`) reads it back through the populated context-vars — not bound in `logged`. The full `REGISTRY` covers every `tools/quality` verb plus polyglot expansion (D31, §12, §13). `thin_rail(settings, scope, params, *, claim, verb, mode)` is **not** a `Handler` (its keyword-only `claim`/`verb`/`mode` violate the arity); the per-verb **adapter** — `static_rail.fix`, a 3-arg `(settings, scope, params) -> Result[Report, Fault]` closing over `claim=STATIC`/`verb="fix"`/`mode=WRITE` — **is** the `Handler`, and is what `REGISTRY` binds (D44). The three thin claims (`static`/`test`/`docs`) bind those adapters; the three C#-only claims (`bridge`/`package`/`api`) bind bespoke folds carrying a `Detail` variant. Per-verb `Params` (`StaticParams`/`TestParams`/`BridgeParams`/`PackageParams`/`ApiParams`/`DocsParams`) are defined in the owning `rails/<claim>.py` and imported here, never re-declared (D43); `mutation`/`benchmark`/`coverage`/`target` are `TestParams` fields, never verbs (§13).

| [CLAIM] | [VERBS] | [HANDLER FAMILY] | [DETAIL] |
| ------- | ------- | ---------------- | -------- |
| `Claim.STATIC` | `fix`, `report`, `build`, `full`, `plan` | `static_rail.*` (thin fold) | none |
| `Claim.TEST` | `run`, `list`, `coverage` | `test_rail.*` (thin fold) | `TestRun` |
| `Claim.BRIDGE` | `verify`, `doctor`, `launch`, `quit`, `check`, `clean`, `build` | `bridge_rail.*` | `VerifySummary` |
| `Claim.PACKAGE` | `stage`, `deploy`, `publish`, `list`, `plan` | `package_rail.*` | `PackageRun` |
| `Claim.API` | `doctor`, `resolve`, `query`, `show` | `api_rail.*` | `ApiSurface` |
| `Claim.DOCS` | `check` | `docs_rail.check` (thin fold) | none |
| (root) | `self-test` | `self_test` (D38) | none |

## [3][VALIDATED_SNIPPET]

The canonical core pattern, per D26/D27/D2/D10/D28: `rail` is a **plain** function (it returns, never yields — `@effect.result` wraps generators only, D26); `_emit` folds the `Result` with a **statement** `match` over the `ok`/`error` tag (D27, never `.map().default_with`); `_RAIL_LAYERS` omits `retried` (D2); both `Envelope` branches read a `RailStatus` whose `exit_code` is the single exit source (D29). `Fault` exposes `argv`/`status`/`message` only (D28) and is constructed positionally `Fault((), RailStatus.FAULTED, msg)`. `_strict` (D52) sits between `handler` and `_emit`: on `getattr(params, "strict", False)`, a folded `EMPTY`/`SKIP` `Report` is promoted to `Error(Fault(FAULTED))` via a statement `match`; every other outcome passes through untouched. `_validated` follows `_strict` in the same `_guard` thunk: it round-trips the success `Report.detail` through `core/model.validate_detail` (the cached `AnyDetail | None` codec), so a malformed `Detail` raises `msgspec.MsgspecError` at the seam and `_guard` maps it to the identical `Fault{(), FAULTED}` — a malformed detail fails loud, never reaches the wire. The cap on `Report.results`≤1000/`artifacts`≤100 lives in `core/model.fold` (D51, D46 — counts and bounds derive there). **`Report` carries no `truncated` field** — only `Envelope` does; `_emit` reads saturation off the capped `results`/`artifacts` lengths against `_RESULT_CAP`/`_ARTIFACT_CAP`, sets `Envelope.truncated`, and writes the stderr note pointing at the run's scope dir. `_emit` also reads the process-static `_WRITES = count()` one-Envelope guard (Invariant 1 at runtime): a statement `match` on `next(_WRITES)` writes stdout on rank `0` and, on any later rank, returns a `FAULTED` `Envelope` to **stderr** instead of a second stdout line. `bind.handler` is erased to `object`, so `rail` re-narrows it through `_narrow` (a `FunctionType` match, no `cast`); the docs rail's `FaultedPromotion` **and** a `validate_detail` `MsgspecError` are caught at `_guard` — the one `except` boundary — and mapped to the identical `Fault`.

```python
# tools/assay/composition/registry.py  (CORE pattern — REGISTRY rows + bespoke seams elided)
import sys, time, msgspec
from collections import deque
from collections.abc import Callable, Mapping
from functools import reduce
from itertools import count, groupby
from operator import attrgetter
from types import FunctionType
from typing import Annotated, Final
from cyclopts import App, Parameter
from expression import Error, Ok, Result
from tools.assay.core.aspect import _RING, checked, compose, logged, traced
from tools.assay.core.status import RailStatus
from tools.assay.core.model import Bind, Claim, Completed, Diagnostic, Envelope, Fault, Report, validate_detail
from tools.assay.composition.catalog import select, TOOLS
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.rails.docs import FaultedPromotion
# ... per-verb rails + their Params imported, never re-declared (D43)

type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]

_RESULT_CAP: Final = 1000   # Report.results bound the fold saturates → signals truncation to _emit
_ARTIFACT_CAP: Final = 100  # Report.artifacts bound the fold saturates
_WRITES: Final = count()    # process-static one-Envelope guard (Invariant 1): rank 0 → stdout, later ranks → stderr fault

def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}

_RAIL_LAYERS: Final = (                                             # D1/D2: checked ▷ logged ▷ traced; NO retried
    checked(),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),                   # D50: run_id + agent_task_id correlate rail span ↔ run_check children
)

REGISTRY: Final[tuple[Bind, ...]] = (                              # D31: full §13 parity; D44: binds per-verb ADAPTERS, not thin_rail
    Bind(Claim.STATIC, "fix", static_rail.fix, StaticParams, "Format, style, analyzer autofix."),
    # ... 25 rows total — see the [CANONICAL_SHAPES] verb/claim table
)

def _distill(fault: Fault, duration_ms: float) -> Diagnostic:      # auto-observability: read the invocation ring back at envelope time
    events = tuple(_RING.get() or ())                              # SAME deque ring_processor appended in place — survives anyio.run
    last = events[-1] if events else ""
    match fault.status:                                            # D27: statement match names the failing step
        case RailStatus.TIMEOUT: step = "timeout"
        case RailStatus.BUSY:    step = "lease_busy"
        case _:                  step = "validation" if "validation" in last else "spawn"  # FAULTED: codec raise logs no rail event → spawn
    hint = f"{step}: {last or fault.message} after {duration_ms:.1f}ms"
    return Diagnostic(failing_step=step, recent_events=events, elapsed_ms=duration_ms, hint=hint)

def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    ms = (time.perf_counter() - started) * 1000.0
    match outcome:                                                 # D27: statement match; D26: NOT .map/.default_with
        case Result(tag="ok", ok=report):                          # D10/D12: success channel carries report
            truncated = len(report.results) >= _RESULT_CAP or len(report.artifacts) >= _ARTIFACT_CAP
            truncated and sys.stderr.write(                        # D51: caps live in fold; _emit derives Envelope.truncated
                f"assay: {bind.claim.value} {bind.verb} output truncated; full results under {settings.run_id}\n")
            envelope = Envelope(
                claim=bind.claim, verb=bind.verb, status=report.status, exit_code=report.status.exit_code,
                run_id=settings.run_id, duration_ms=ms, report=report, truncated=truncated, notes=report.notes,
            )                                                      # success leaves error_context None — no Diagnostic on the wire
        case Result(error=fault):                                  # D10/D28: error channel carries Fault{argv,status,message}
            envelope = Envelope(
                claim=bind.claim, verb=bind.verb, status=fault.status, exit_code=fault.status.exit_code,
                run_id=settings.run_id, duration_ms=ms, error=fault, error_context=_distill(fault, ms),  # Fault branch ONLY
            )
    match next(_WRITES):                                            # Invariant 1 runtime guard: exactly one Envelope per process
        case 0:
            sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")  # sole stdout writer; cached deterministic codec
            return envelope
        case rank:                                                 # a second writer is a wiring defect → FAULTED to stderr, not stdout
            doubled = Envelope(claim=bind.claim, verb=bind.verb, status=RailStatus.FAULTED,
                               exit_code=RailStatus.FAULTED.exit_code, run_id=settings.run_id,
                               error=Fault((), RailStatus.FAULTED, f"second Envelope suppressed (write #{rank}); Invariant 1 violated"))
            sys.stderr.buffer.write(_ENCODER.encode(doubled) + b"\n")
            return doubled

def rail(bind: Bind) -> Callable[[object], Envelope]:              # D26: plain fn (no @effect.result — never yields)
    handler = compose(*_RAIL_LAYERS)(_narrow(bind.handler))        # D2: checked ▷ logged ▷ traced over the narrowed Handler
    def run(params: object) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        scope = ArtifactScope.open(settings, bind.claim)
        token = _RING.set(deque(maxlen=16))                        # seat the invocation ring BEFORE the handler; ring_processor appends in place
        try:
            outcome = _guard(lambda: _validated(_strict(handler(settings, scope, params), params)))  # D52 + validate_detail + docs FaultedPromotion seam
            return _emit(bind, settings, started, outcome)
        finally:
            _RING.reset(token)                                     # invocation-scoped: reset so the next call seats a fresh deque
    run.__name__ = bind.verb
    return run

def _narrow(handler: object) -> Handler[object]:                  # D19: Bind.handler erased to object; re-narrow at seam (no cast)
    match handler:
        case FunctionType() as fn:                                 # beartype forward-ref resolves under PEP 649; rails import the annotation
            return fn
        case _:
            raise TypeError(f"Bind.handler must be a module-level def, got {type(handler).__name__}")

def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:  # round-trip Report.detail; malformity raises MsgspecError → _guard
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)                         # None/well-formed: inert; malformed: raises into the _guard catch
            return outcome
        case _:
            return outcome

def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:  # the ONE except boundary
    try:
        return thunk()
    except (FaultedPromotion, msgspec.MsgspecError) as promoted:   # docs --strict raise OR malformed Detail → identical Fault{(), FAULTED}
        return Error(Fault((), RailStatus.FAULTED, str(promoted)))

def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:  # D52: getattr-read strict
    match outcome:                                                 # D27: statement match; only no-op folds promote
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, "strict: empty/skipped fold"))
        case _:
            return outcome

def _leaf(bind: Bind) -> Callable[..., Envelope]:
    runner = rail(bind)
    ann = Annotated[bind.params, Parameter(name="*")]              # flatten frozen dataclass fields
    def command(params: ann = bind.params()) -> Envelope:          # NO @wraps: __wrapped__→defaultless run breaks cyclopts flatten
        return runner(params)
    command.__name__ = bind.verb
    command.__doc__ = bind.help
    command.__annotations__ = {"params": ann, "return": Envelope}  # pins the flatten type + Envelope return for the returncode hook
    return command

def build_app(registry: tuple[Bind, ...]) -> App:                 # groupby(claim) → reduce-fold leaves, then claims
    root = App(name="assay", help="Rasm polyglot quality operator.",
               default_parameter=Parameter(show_default=False), result_action="return_value")
    keyed = sorted(registry, key=lambda b: b.claim.value)          # sort on StrEnum VALUE so groupby runs are contiguous (D9)
    subs = tuple(
        reduce(lambda app, row: _register(app, _leaf(row), name=row.verb, help=row.help), tuple(rows), App(name=claim.value))
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    app = reduce(_register, subs, root)
    _register(app, self_test, name="self-test")                   # D38: root command, outside the claim tree
    return app

def _register(app: App, obj, *, name=None, help="") -> App:       # App.command returns OBJ not app; this seam returns app for the fold
    match (name, help):
        case (None, _): app.command(obj)
        case (verb, ""): app.command(obj, name=verb)
        case (verb, text): app.command(obj, name=verb, help=text)
    return app

_ENCODER: Final = msgspec.json.Encoder(order="deterministic")     # D55: the sole stdout codec, cached once

def self_test(*, rhino: bool = False) -> Envelope:                # preflight; failure → FAILED (D38, §8)
    ...
```

## [4][SEAMS]

| [SEAM] | [DIRECTION] | [CONTRACT] |
| ------ | ----------- | ---------- |
| `core/model.py` | imports | `Bind`, `Claim`, `Completed`, `Diagnostic`, `Envelope`, `Fault`, `Report`, `validate_detail`; `Envelope.__cyclopts_returncode__()` and `RailStatus.exit_code` are the only exit sources (D29, D30). `fold` caps `results`≤1000/`artifacts`≤100 on the **collections** (D51); `Report` has **no** `truncated` field — `_emit` derives `Envelope.truncated` by measuring those capped lengths against `_RESULT_CAP`/`_ARTIFACT_CAP`. `Fault((), FAULTED, msg)` is positional. `validate_detail(report.detail)` round-trips the success detail through the cached `AnyDetail \| None` codec in `_validated`; malformity raises `msgspec.MsgspecError` into `_guard` → `Fault{(), FAULTED}`. `Completed((), 0)` is the degenerate receipt `self_test`'s census feeds each catalog `Parser`. `Diagnostic` is the `Detail` variant `_distill` builds on the Error branch; it rides `Envelope.error_context` (Fault branch only). |
| `composition/catalog.py` | imports | `TOOLS`, `select`; `self_test`'s `_census` asserts every `TOOLS` row is in `select(t.claim, t.language)` (routable) and every `t.parser` folds `Completed((), 0)` without raising (`_parses` probe) — the day-one catalog-rot catch. |
| `core/status.py` | imports | `RailStatus`; `_strict` promotes a folded `EMPTY`/`SKIP` to `Error(Fault((), FAULTED, ...))` (D52); members are the sole `match` discriminants. |
| `core/aspect.py` | imports | `_RING`/`checked`/`logged`/`traced`/`compose`; rail seam = `checked ▷ logged ▷ traced` (D2). `compose` raises `TypeError` on `Slot` inversion (probed by `_composes`). `_correlate` feeds **both** `logged.keys` and `traced.attrs`; retry correlation closes at the **engine** seam via `traced` + the module-scope `_on_retry` hook, never `logged` (D3, R05). `rail.run` seats `_RING.set(deque(maxlen=16))` before the handler (reset in `finally`); `aspect.ring_processor` appends `level:event` to that **same** deque by reference across `anyio.run`, and `_distill` reads `_RING.get()` back at `_emit` — the auto-observability event flow. |
| `composition/settings.py` | imports | `AssaySettings()` (no-arg, env-sourced); `scope = ArtifactScope.open(settings, claim)` is a **plain call** (not a `with` block — the runner holds the scope for the whole rail, no per-call teardown). |
| `rails/*` | imports | Each `*_rail.<verb>` adapter is the `Handler` returning `Result[Report, Fault]`; thin claims' adapters delegate to `thin_rail(..., claim, verb, mode)` (D44 — `thin_rail` itself is **not** a `Handler`), bespoke claims attach `Detail` (§13). Per-verb `Params` imported from the owning `rails/<claim>.py` (D43). `docs_rail.FaultedPromotion` is imported for the `_guard` catch. |
| `__main__.py` | exports | `REGISTRY`, `build_app`; `meta` calls `resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))` (D30). |
| `automation/engine.py` | reuses | `_rail_outcome` resolves a `Rail` action to its `REGISTRY` row and invokes `rail(bind)(params)`; that runner writes **its own** `Envelope` (the registry stays the sole rail stdout writer), so the engine does **not** re-emit — it returns the runner's `Envelope` for the fold. Engine-direct fires use the engine's own `_emit`. NDJSON is the documented one-Envelope exception (D37, §9). |

`_emit` is the **sole** stdout writer for a rail (Invariant 1); `structlog` lines and `Fault.message` egress on stderr via `@logged`. The runner constructs one `AssaySettings`, one scope, one `time` capture, and exactly one `Envelope` per branch — zero per-rail projectors (D10).

## [5][EXTENSIBILITY]

A new quality verb is one `Bind(claim, verb, handler, ParamsT, help)` row appended to `REGISTRY` — where `handler` is the per-verb **adapter** (D44) and `ParamsT` is the claim's `Params` imported from its owning `rails/<claim>.py` (D43); `rail`, `_strict`, `_emit`, `_leaf`, and `build_app` are untouched (Invariant 4). A new claim is one new sub-`App` produced automatically by the `groupby` fold once its rows exist.

## [6][CONSIDERATIONS]

- `meta` (in `__main__.py`) must call `resolve_returncode(...)` on the raw return, not branch: `resolve_returncode(None) == 0` already covers `--help`/`--version` (which return `None` under `result_action="return_value"`), so a defensive `match` is dead ceremony — D30 collapses it to one expression.
- `groupby` is correct only on contiguous keys; `build_app` pre-sorts on `b.claim.value`. Sorting on the `StrEnum` *value* (not the member) keeps the sub-`App` name and the wire/dispatch key identical (D9), so a future enum reorder never silently fragments a claim into two sub-apps.
- `self_test` is attached after the claim fold and is **not** in `REGISTRY`; the contract test `names == {b.claim.value for b in REGISTRY}` therefore checks claim sub-apps only — `self-test` lives beside them on the root, so any assertion over the full root command set must add it explicitly (D38) to avoid a false parity failure.
- `self_test`'s `_census` is the day-one catalog-rot catch: it folds `REGISTRY`+`TOOLS` and demands every catalog row be routable via `select(t.claim, t.language)` and every `t.parser` survive `Completed((), 0)` without raising (`_parses` is a marked-boundary probe, kin to `_composes`). A new `Tool` row with a stale `(claim, language)` route or a `Parser` that NREs on an empty receipt is surfaced at preflight as `FAILED`/exit 1, not at the first live rail. The census is read-only — it never spawns a process, so `self-test` stays a cheap structural assertion.
- `_emit`'s `_WRITES = count()` is the **runtime** half of Invariant 1 (the design-time half is "only `_emit` writes stdout"). A second `_emit` in one process — an engine that re-emits, a test that calls `rail(bind)(params)` twice without a fresh interpreter, a future arm that forgets the NDJSON exception is automation-only — yields a `FAULTED` `Envelope` on **stderr**, leaving stdout at exactly one line. Automation's one-Envelope-per-fire stream (D37) runs through the engine's own `_emit`, not this runner, so it is unaffected.
- `_strict` reads `strict` via `getattr`, so it is inert on any `Params` lacking the field; D43 puts the flag on api/docs (and static-family) `Params` only, and the promotion fires solely on a folded `EMPTY`/`SKIP` — a `FAILED`/`BUSY`/`TIMEOUT` fold already dominates by severity (D48), leaving `strict` a no-op there. The single concrete `Fault(status=FAULTED)` is the only place this runner authors a `Fault`; the engine fold never emits one (D12), so the strict promotion is the sole synthetic error channel in the composition root.
- The auto-observability ring is **invocation-scoped**, not process-static like `_WRITES`: `rail.run` seats a fresh `deque(maxlen=16)` per call and resets the `ContextVar` token in `finally`, so a second `rail(bind)(params)` in one process gets a clean window — no cross-invocation bleed. The load-bearing subtlety is that `ring_processor` mutates the **same** `deque` object in place: even where the handler logs from inside the engine's `anyio.run`, the spawned coroutine inherits a *copy of the context* whose `_RING` slot still points at the original deque, so `_distill`'s `_RING.get()` at `_emit` reads the events the spawn appended. A `Diagnostic` rides `error_context` on the Fault branch only; the success branch never distills, so the success wire carries no `Diagnostic` payload (D28 holds — `Fault` itself stays `{argv, status, message}`; the diagnostic is an `Envelope` field, never a `Fault` field).
