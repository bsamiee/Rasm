# [H1][ASSAY_REGISTRY]
>**Dictum:** *One table drives the CLI tree and the dispatch; one runner folds every rail; the Report self-describes.*

`composition/registry.py` is the composition root. It imports `rails/*`, `composition/catalog.py`, `composition/settings.py`, and `core/aspect.py`, owns the `(Claim, verb) -> handler` table, and exposes the single `rail` runner plus the tree builder consumed by `__main__`. It replaces `tools/quality/__main__.py:195` `rail[T]()` with its eight projector callables and the legacy re-encode ladder.

**Canonical:** [`snippets/cli.py.md`](snippets/cli.py.md) (authoritative runner/tree) · [`AOT.md`](AOT.md) §3 · [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) · [`main.md`](main.md). No `bind_check`, no `Scope` type alias, no `Engine.run`.

---
## [1][REGISTRY_SHAPE]
>**Dictum:** *A rail is a data row; the table is the single source for both tree and dispatch.*

```python
type Handler[**P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]

class Bind(msgspec.Struct, frozen=True, gc=False):
    claim: Claim                 # StrEnum axis: sub-App name + dispatch key + wire value
    verb: str                    # leaf command name under the claim
    handler: Handler             # returns a self-describing Report (or Fault)
    params: type[Params]         # cyclopts-flattened arg struct for this leaf
    help: str = ""

REGISTRY: Final[tuple[Bind, ...]] = (
    Bind(Claim.STATIC,  "fix",    static_rail.fix,    StaticParams,  "Format, style, analyzer autofix."),
    Bind(Claim.STATIC,  "build",  static_rail.build,  StaticParams,  "Routed restore + build + analyzers."),
    Bind(Claim.STATIC,  "plan",   static_rail.plan,   StaticParams,  "Print owners, triggers, closure."),
    Bind(Claim.TEST,    "run",    test_rail.run,      TestParams,    "Unit, coverage, mutation fold."),
    Bind(Claim.TEST,    "list",   test_rail.list,     TestParams,    "Enumerate runnable cases."),
    Bind(Claim.BRIDGE,  "verify", bridge_rail.verify, BridgeParams,  "Live RhinoWIP scenario fold."),
    Bind(Claim.API,     "resolve",api_rail.resolve,   ApiParams,     "Host/NuGet metadata surface."),
    Bind(Claim.DOCS,    "check",  docs_rail.check,     DocsParams,    "Markdown + Mermaid validation."),
)
```

The same `REGISTRY` is folded twice with no second source of truth.

- **Tree:** group rows by `claim` into one `App(name=claim)` each; register every leaf via the factory in §6; mount each claim App on the root App. Single `for bind in REGISTRY` loop replaces the hand-wired `static`/`test_app`/`bridge`/`api` Apps and the `partial()` registration loops at `__main__.py:469-476`.
- **Dispatch:** Cyclopts routes parsed argv to the generated leaf function, which constructs `Params` and calls `rail(bind)(params)`. No dispatch `match` exists; the table *is* the dispatch.

---
## [2][ONE_RAIL_RUNNER]
>**Dictum:** *The runner never projects status; it reads it off the Report.*

```python
_RAIL_LAYERS: Final = (checked(), logged(event="rail"), traced(span="assay.rail", attrs=_rail_attrs))

def rail(bind: Bind) -> Callable[[Params], Envelope]:
    handler = compose(*_RAIL_LAYERS)(bind.handler)   # checked ▷ logged ▷ traced; NO retried at rail seam

    def run(params: Params) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        with ArtifactScope.open(settings, bind.claim) as scope:
            outcome = handler(settings, scope, params)        # Result[Report, Fault]
        return _emit(bind, settings, started, outcome)

    return run


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    duration_ms = (time.perf_counter() - started) * 1000.0
    envelope = outcome.map(
        lambda report: Envelope(
            claim=bind.claim, verb=bind.verb, run_id=settings.run_id, duration_ms=duration_ms,
            status=report.status, exit_code=report.status.exit_code, report=report, notes=report.notes,
        )
    ).default_with(
        lambda fault: Envelope(
            claim=bind.claim, verb=bind.verb, run_id=settings.run_id, duration_ms=duration_ms,
            status=fault.status, exit_code=fault.status.exit_code, error=fault,
        )
    )
    sys.stdout.buffer.write(msgspec.json.encode(envelope) + b"\n")  # sole stdout writer
    return envelope
```

**Zero per-rail projectors.** On `Ok`, `status`, `exit_code`, `data`, and `notes` are all read from one `Report`; on `Error`, all are read from one `Fault`. Both carry a `RailStatus`. The eight `Callable` parameters of the legacy `rail[T]` and the four `emit_success` callbacks (`data`/`status`/`exit_code`/`notes`) collapse into field access. There is exactly one `time` capture, one scope open, one `Envelope` constructor per branch.

---
## [3][ENVELOPE_EMISSION]
>**Dictum:** *Exactly one JSON line on stdout; everything else is stderr; the exit code is a RailStatus payload.*

- **stdout:** `_emit` writes `msgspec.json.encode(envelope) + b"\n"` once. `Envelope.report` carries the typed `Report` struct directly — no `data`/`evidence`/`quality_payload` re-encode ladder, and no `Completed`-to-dict shimming.
- **stderr:** all engine bytes (`Completed.stdout/stderr`), `structlog` diagnostics, and `Fault.message` are emitted by the `@logged` aspect and `core/engine.py`, never by the runner. The runner touches stdout only.
- **exit code:** `envelope.exit_code` is `report.status.exit_code` or `fault.status.exit_code` and nothing else. `RailStatus` (`core/status.py`) is the lone status algebra: each member carries its `exit_code` via `__new__` payload (`OK->0`, `SKIP/EMPTY->0`, `FAILED->1`, `UNSUPPORTED->3`, `BUSY`/`TIMEOUT->5`). No `STRICT_FAILED` member unless added to `status.md` with a pinned exit payload. `--strict` is a `Params` flag the handler reads to promote vacuous outcomes to `FAILED` inside the fold, not a runner projector.

---
## [4][ASPECT_SEAMS]
>**Dictum:** *Two seams only: the engine wraps a Check, the runner wraps a rail.*

`core/aspect.py` exposes `checked` (beartype boundary), `traced` (otel span), `retried` (stamina), `logged` (structlog bind), and `compose`. They attach at two places and nowhere else.

| [SEAM]        | [WHO]                | [SPAN]                       | [RETRY]                       | [LOG BIND]                     |
| ------------- | -------------------- | ---------------------------- | ----------------------------- | ------------------------------ |
| Rail runner   | `registry.rail`      | parent `assay.<claim>.<verb>`| **none** — rail is not `Spawn` | `claim`, `verb`, `run_id`      |
| `run_check`   | `core/engine.py`     | child span per `Check`       | per process spawn / restore   | (span attrs only; no `@logged`) |

The parent otel span is the rail; each `Check` span nests under it, so a trace renders the rail as the root and every program as a child — the rail tree *is* the trace (`ARCHITECTURE.md` §9). `structlog.contextvars` binds `claim`/`verb`/`run_id` at the rail seam and inherits into engine spans, which add `tool`/`argv`. Rail stack: `checked ▷ logged ▷ traced ▷ handler`. Engine stack: `checked ▷ traced ▷ retried ▷ spawn` (`aspect.md` §2–§3).

---
## [5][ADDING_A_RAIL]
>**Dictum:** *One row, one handler, no runner edit.*

Hypothetical new rail (illustrative — **`Claim.LINT` is not in `model.md`** until added):

```python
# rails/lint.py — use _check_from + fan_out + model.fold (snippets/model-status.py.md)
def lint(settings: AssaySettings, scope: ArtifactScope, params: LintParams) -> Result[Report, Fault]:
    return route(params.language, params.paths).bind(
        lambda routed: fan_out(tuple(_check_from(t, routed) for t in select(Claim.STATIC, params.language)), settings=settings)
    ).map(lambda outcomes: fold(Claim.STATIC, "lint", outcomes))
```

One new `Bind(Claim.STATIC, "lint", ...)` row if promoted; `rail`, `_emit`, and `Envelope` unchanged.

---
## [6][OPEN_DECISIONS]
>**Dictum:** *Name the seams that are not yet settled.*

1. **Verb params into handlers.** Preferred: each `Bind.params` is a Cyclopts-bindable struct (dataclass/`msgspec`/pydantic), and one factory registers leaves by injecting the annotation Cyclopts introspects:

```python
def _leaf(bind: Bind) -> Callable[..., Envelope]:
    runner = rail(bind)
    ann = Annotated[bind.params, Parameter(name="*")]
    def command(params: ann = bind.params()) -> Envelope:
        return runner(params)
    command.__annotations__ = {"params": ann, "return": Envelope}
    command.__name__ = bind.verb
    return command  # registered via claim_app.command(name=bind.verb)(_leaf(bind))
```

   Cyclopts flattens the struct fields into options and reconstructs the instance, so types survive end to end. Alternative (rejected): a positional `*tokens: str` re-parsed inside the handler — loses static types and re-creates stringly routing. **Pinned:** `Params` is a frozen `@dataclass` per verb (`TID251` bans `NamedTuple`); Cyclopts introspects dataclass fields natively (`main.md` §4).

2. **Watch mode.** One row `Bind(Claim.WATCH, "watch", watch_rail.watch, WatchParams, ...)` whose handler loops `watchfiles.watch(roots)` and re-invokes the *target* `rail(target_bind)` per change set. Open: this emits one Envelope per cycle, which strains Invariant 1 ("exactly one Envelope to stdout"). Resolution candidates: (a) define watch output as a newline-delimited Envelope *stream*, one per fold; (b) emit a single terminal aggregate Envelope on exit with per-cycle detail. Leaning (a), since each fold already self-describes and downstream consumers read line-by-line.

3. **Self-test / settings injection.** `rail` constructs `AssaySettings()` per invocation; verbs needing overrides (e.g. `--target`) pass them through `Params`, and the handler does `settings.model_copy(update=...)` rather than the runner branching, keeping the runner verb-agnostic.
