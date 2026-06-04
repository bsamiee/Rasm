# [H1][INIT]
>**Dictum:** *The package marker installs one stderr-bound structlog pipeline and one endpoint-gated OTel provider at import, behind the optional beartype claw — never on stdout, never twice; init installs, `__main__` drains.*

## [1][PURPOSE]

`tools/assay/__init__.py` is the sole package marker file (every other directory is a PEP 420 namespace package, §14). It runs exactly three import-time boundary actions and declares no domain logic: (1) the optional `ASSAY_CLAW` beartype claw gate as its **first statement** (load-bearing, not cosmetic — claw rewrites modules at import and must precede the first `import tools.assay.core.model`, since `msgspec` resolves field annotations lazily at first codec build under PEP 649/749); (2) the once-only `structlog.configure` call, renderer selected by `match log_format`, bound to `sys.stderr`, paired with a process-global `bind_contextvars(**agent_context)` of the settings `{run.id, agent.task.id}` correlation pair so **every** log line and `Envelope` correlates to its driving agent task with zero CLI flags; (3) the endpoint-gated OTel `TracerProvider` install whose `Resource` carries the same `agent_context` so every span correlates too — `set_tracer_provider` once when `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` is set, otherwise a no-op so the API's default `NoOpTracer` stands (zero egress, zero cost). The agent-correlation source is single: `AssaySettings().agent_context` folds the `run_id` (env `ASSAY_RUN_ID`) and `agent_task_id` (env `ASSAY_AGENT_TASK_ID`) fields through the pydantic boundary (never `os.environ`), validated once at the sole `AssaySettings()` this gate constructs; the configure path is the *bind* site for that pair, the engine/automation seams (`aspect.traced` / `engine._spawn`) the per-check *stamp* sites (D50). Stack §6: `structlog.configure` runs once at `__init__.py` import; the OTel provider installs once via this gate (D50, §6 "module import hook gated on `(endpoint, provider)`"); `beartype` attaches at seams only unless `ASSAY_CLAW` is set, in which case the claw is the first `__init__.py` statement (D-aspect §6). It owns **install only** — the BatchSpanProcessor drain is owned by `__main__`, which calls `get_tracer_provider().force_flush(5000)` at exit (D50). It owns no `Tool`, no `Rail`, no `Envelope` writer — those live in `composition/registry.py` and `__main__.py`.

## [2][CANONICAL_SHAPES]

This doc owns `_configure(log_format)`, `_install_tracing(endpoint)`, and the claw gate. `LogFormat` is a `StrEnum` whose members are `LogFormat.CI` (value `"ci"`) and `LogFormat.HUMAN` (value `"human"`) in `composition/settings.py` (§5, D21, mirroring `settings.md`) — **never** a `Literal` re-spelling and never a lowercase member name. Renderer is chosen by `match`, never `if` (§6: "renderer by `match log_format`"). The `logger_factory` is `WriteLoggerFactory(file=sys.stderr)` — **mandatory**; the structlog default is `stdout`, which would corrupt the sole-stdout `Envelope` contract (Invariant 1, §17). Processor chain order is fixed (dossier `assay_seam`):

| [SLOT] | [PROCESSOR] | [ROLE] |
| :----: | ----------- | ------ |
| 0 | `merge_contextvars` | First — pulls `claim`/`verb`/`run_id`/`tool`/`check.id` ContextVar state for `@logged` bind isolation. |
| 1 | `ring_processor` | After `merge_contextvars` so it sees the merged context — appends a compact `level:event` summary to the live recent-events ring and returns the `event_dict` unchanged (pure pass-through; zero-cost identity when no ring is seated). |
| 2 | `add_log_level` | Injects `level` from the method name. |
| 3 | `dict_tracebacks` | Structured `exc_info` frames (bug channel) — `msgspec`-serializable, distinct from the domain `Fault` rail. |
| 4 | `TimeStamper(fmt="iso", utc=True)` | ISO-8601 UTC stamp. |
| 5 | `renderer` | Terminal — `JSONRenderer` (`ci`) vs `ConsoleRenderer` (`human`); the **only** branch. Typed `renderer: Processor` on the `ci` arm. |

`wrapper_class=make_filtering_bound_logger(_INFO)` (`_INFO = 20`, INFO) gates levels at build time — disabled levels compile to `return None`, not runtime checks. `cache_logger_on_first_use=True` caches the bound logger after the first `get_logger`; no call site constructs one. `_configure(log_format, agent_context)` is idempotent under the `match structlog.is_configured()` gate so a re-import is a no-op; its trailing `bind_contextvars(**agent_context)` seats the `{run.id, agent.task.id}` pair into the process-global ContextVar slot `merge_contextvars` (slot 0) drains, so every log line — including the engine/lease/automation seams `logged` never scopes — carries the correlation. Three module constants back the install path: `_INFO`, `_DRAIN_MS` (the `BatchSpanProcessor` schedule delay, paired with `__main__`'s `force_flush`), and `_SERVICE = {"service.name": "assay"}` (the base span-identity mapping). `_install_tracing(endpoint, agent_context)` builds the actual span `Resource` in **one** `Resource.create(_SERVICE | agent_context)` — a `Resource.create(...).merge(Resource.create(agent_context))` would collapse `service.name` to `unknown_service` (the merge's empty-default wins), so the dict-union (single `create`) is mandatory and keeps `_SERVICE` the sole base enrichment point passed to `TracerProvider(resource=...)`.

## [3][VALIDATED_SNIPPET]

```python
# First statement: ASSAY_CLAW amplifies beartype to every submodule + PEP 526 assignments.
# Table-dispatch keeps `if` out of the boundary; must run BEFORE the first transitive
# `import tools.assay.core.model` (claw rewrites at import; msgspec resolves lazily).
{"1": lambda: beartype_this_package(
    conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None),
)}.get(os.environ.get("ASSAY_CLAW", ""), lambda: None)()

# ... (post-claw imports: msgspec, structlog (+ bind_contextvars), opentelemetry, AssaySettings/LogFormat, aspect.ring_processor)

_INFO: int = 20                                                    # filtering floor; below -> `return None`
_DRAIN_MS: int = 5000                                              # BSP schedule delay; __main__ owns force_flush
_SERVICE: dict[str, str] = {"service.name": "assay"}              # base span-identity mapping; agent_context extends it
_ENDPOINT_ENV: str = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"          # only network gate this package opens


def _configure(log_format: LogFormat, agent_context: dict[str, str]) -> None:
    match log_format:
        case LogFormat.CI:
            renderer: Processor = JSONRenderer(serializer=lambda v, **_k: msgspec.json.encode(v).decode())
        case LogFormat.HUMAN:
            renderer = ConsoleRenderer(colors=True)
    structlog.configure(
        processors=[
            merge_contextvars,                       # FIRST: ContextVar isolation for @logged bind + agent-context seed
            ring_processor,                          # AFTER merge_contextvars: appends level:event to the recent-events ring WITH context (pass-through)
            add_log_level,
            dict_tracebacks,                         # bug channel, NOT the domain Fault rail
            TimeStamper(fmt="iso", utc=True),
            renderer,                                # terminal; JSON(ci)|Console(human) is the ONLY branch
        ],
        wrapper_class=make_filtering_bound_logger(_INFO),
        logger_factory=WriteLoggerFactory(file=sys.stderr),           # MANDATORY: default sink is stdout -> Envelope corruption
        cache_logger_on_first_use=True,
    )
    bind_contextvars(**agent_context)                             # process-global {run.id, agent.task.id}: every log correlates


def _install_tracing(endpoint: str, agent_context: dict[str, str]) -> None:
    provider = TracerProvider(resource=Resource.create(_SERVICE | agent_context))  # one create; merge would null service.name
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint), schedule_delay_millis=_DRAIN_MS))
    set_tracer_provider(provider)                                  # global; @traced reads it at the two seams (D50)


# Statement-form `match` is the sole boundary dispatch (D27): no `if`.
_SETTINGS = AssaySettings()                                       # validate once; agent_context is pydantic-read, never os.environ

match structlog.is_configured():
    case False:
        _configure(_SETTINGS.log_format, _SETTINGS.agent_context)
    case True:
        pass

match os.environ.get(_ENDPOINT_ENV, ""):
    case "":
        pass
    case endpoint:
        _install_tracing(endpoint, _SETTINGS.agent_context)
```

`import msgspec` backs the `ci` renderer's `serializer=` (the `JSONRenderer` default `json.dumps` cannot encode `msgspec` structs / `datetime`, so the lambda routes through `msgspec.json.encode`). `ring_processor` (from `core/aspect.py`) is inserted immediately after `merge_contextvars` and before `add_log_level` so it captures every event with merged context into the live recent-events ring (a pure pass-through that returns the `event_dict` unchanged, zero-cost when no ring is seated). `match` is statement form (D27): each arm binds `renderer`, then `configure` consumes it — `renderer: Processor` is annotated on the first arm only. The sole `_SETTINGS = AssaySettings()` is validated once at the boundary and feeds both gates: `_SETTINGS.log_format` selects the renderer and `_SETTINGS.agent_context` (`{run.id, agent.task.id}`) is the correlation pair bound for logs and folded into the span resource — so there is exactly one config read and one correlation source, never a second `BaseSettings` or an `os.environ` peek for the agent tags. The two trailing `match` blocks are the import-time boundary, not domain control flow: the `is_configured()` gate makes a re-import a no-op (configure-once) and its arm runs `_configure(log_format, agent_context)` whose terminal `bind_contextvars(**agent_context)` seats the pair process-globally; the `_ENDPOINT_ENV` gate installs the `TracerProvider` exactly when the endpoint is non-empty and falls through to the `NoOpTracer` otherwise. `_install_tracing(endpoint, agent_context)` performs **install only**: it builds a `TracerProvider(resource=Resource.create(_SERVICE | agent_context))` (one `create`, never a `.merge` — the merge collapses `service.name` to `unknown_service`), attaches a `BatchSpanProcessor(OTLPSpanExporter(endpoint=...), schedule_delay_millis=_DRAIN_MS)`, and `set_tracer_provider`s it as the process-global (`add_span_processor` returns `None`, so the build is three statements, never a chained one-liner); it never flushes. Because the correlation rides the provider `Resource`, every span the provider emits — parent `fan_out`, child, and lease — carries `{run.id, agent.task.id}` beside the per-seam `traced` attrs, with zero CLI flags. Draining the `BatchSpanProcessor` queue is `__main__`'s sole responsibility: `get_tracer_provider().force_flush(5000)` at exit before return (D50), so any span the two `@traced` seams (§6) emit egresses before the process ends. The exporter's `requests.Session` egress is the only point this package touches the network, and only behind the endpoint gate.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| ---------- | :---------: | ---------- |
| `composition/settings.py` | imports | `AssaySettings().log_format -> LogFormat` (D21) **and** `AssaySettings().agent_context -> {run.id, agent.task.id}` (the fleet pair from `ASSAY_RUN_ID`/`ASSAY_AGENT_TASK_ID`); the sole `AssaySettings()` here is the only config read at import. |
| `core/aspect.py` `logged(event, keys)` | downstream | Consumes the configured pipeline via `structlog.get_logger()`; `merge_contextvars` (slot 0) surfaces both `bound_contextvars(**keys)` and the process-global `bind_contextvars(**agent_context)` this gate seeds, so the agent pair rides every log even outside a `logged` scope. |
| `core/aspect.py` `traced(span, attrs)` | downstream | Reads the globally-installed provider via `opentelemetry.trace.get_tracer(...)`; the provider `Resource` already carries `agent_context`, so every span correlates without per-seam plumbing. When the endpoint gate is unset, `_install_tracing` never fires and the API returns the `NoOpTracer`, so `@traced` is inert without a branch at the seam (§6, D50). This doc installs the provider; it never opens a span. |
| `core/model.py` | ordering | Claw gate **must precede** any transitive `import core.model` so module-rewrite timing vs lazy `msgspec` annotation resolution holds. |
| `__main__.py` | sibling | A `BeartypeViolation` escaping the seam stack is caught at the `__main__` boundary and rendered as an exit-2 `Envelope(error=Fault(...))` (§6) — the bug channel stays distinct from the domain `Fault` rail. `__main__` also owns the trace drain: `get_tracer_provider().force_flush(5000)` at exit empties the `BatchSpanProcessor` queue this doc installed (D50); init installs, `__main__` flushes. |

## [5][EXTENSIBILITY]

A new renderer (e.g. logfmt) is one `LogFormat` member plus one `match` arm; a new structural processor (e.g. `CallsiteParameterAdder`) is one inserted tuple element before `renderer` — never a second `configure` call and never a per-call-site logger build. A new correlation tag (e.g. `ci.job.id`) is one more key in `AssaySettings.agent_context` (settings.md §5) — it flows into both the `bind_contextvars` log seed and the span `Resource` automatically, with no edit here. A second exporter (e.g. console-debug span output) is one more `add_span_processor` inside `_install_tracing` — never a second `set_tracer_provider`; base span identity is enriched by extending the `_SERVICE` mapping (`{"service.name": "assay"}`), which `_install_tracing` unions with `agent_context` into the sole `Resource.create(...)` passed to `TracerProvider(resource=...)`, kept here so the gate stays the sole install site.

## [6][CONSIDERATIONS]

- `WriteLoggerFactory` over `PrintLoggerFactory` is deliberate beyond the stderr binding: `WriteLogger` is the faster variant and both share `_get_lock_for_file(file)` thread-safety, so the single pinned `sys.stderr` sink is already lock-coordinated against concurrent `fan_out` checks without extra coordination — but mixing a second factory on a different `sys.stderr` handle would defeat that shared lock, so the sink stays singular.
- `make_filtering_bound_logger(_INFO)` is static at build time and cannot be re-leveled post-`configure`; if a future `--debug` flag must lower the floor, it has to feed `AssaySettings.log_level` *before* the import-time `_configure` fires (env precedes import), not mutate the wrapper afterward — the level is a config ingress decision, not a runtime toggle.
- `warning_cls_on_decorator_exception=None` makes the claw **raise** (not warn) on decoration edge cases, so the CI gate fails loud; pairing it with `BeartypeConf(is_debug=True)` once verifies the synthesized wrapper actually deep-checks `Result[Completed, Fault]` rather than collapsing it to the bare `Result` origin, since `expression`'s runtime subscription depth governs whether `Ok`'s payload is sampled at all.
- The OTLP exporter is sync `requests.Session` (dossier TRAP-3), and `BatchSpanProcessor` buffers off-thread on `schedule_delay_millis=_DRAIN_MS` (5000ms, pinned at the install site rather than left to the `OTEL_BSP_SCHEDULE_DELAY` env default); a short-lived `assay` invocation almost always exits before that timer fires, so the spans live only in the BSP queue until drained. `TracerProvider(shutdown_on_exit=True)` registers its own `atexit` flush, but it is not deterministically ordered against the Cyclopts return and runs after the interpreter starts tearing down — which is exactly why the explicit `__main__` `force_flush(5000)` before return (D50) is load-bearing, not redundant: it guarantees egress on the main thread inside the live runtime. Lowering the install floor by setting `OTEL_SDK_DISABLED=true` is a faster kill than unsetting the endpoint when a span-free run is wanted without touching the env gate.
- The endpoint gate is the only network surface this package opens; `_install_tracing` constructing `OTLPSpanExporter` does **not** itself connect — the first POST happens at flush, so a misconfigured endpoint surfaces as a `force_flush` timeout in `__main__` (returning `False`), never as an import-time failure. Keep the install path connection-free so a stale `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` never blocks `import tools.assay`.
- Agent correlation is `Resource.create(_SERVICE | agent_context)`, **never** `Resource.create(_SERVICE).merge(Resource.create(agent_context))`: `Resource.merge` lets the *other* resource's `service.name` win, and `Resource.create(agent_context)` supplies the schema default `unknown_service`, silently overwriting `assay`. The dict-union into a single `create` is the only shape that keeps `service.name` and the agent pair together — verified against opentelemetry-sdk 1.41.1.
- `agent.task.id` binds present-but-empty when `ASSAY_AGENT_TASK_ID` is unset (the field defaults to `""`), which is the correct "no agent" signal — a downstream collector filters on key presence, and an empty value never pollutes a non-fleet local run. `run.id` always populates (the `%Y-%m-%dT…-{pid}` default), so the correlation tuple is structurally total even with zero env. Binding at configure time means the pair is process-global *before* the first `fan_out` child task spawns, so even a worker that never enters a `logged`/`traced` scope (e.g. a lease steal log) still emits the correlation — the per-seam `traced` attrs (D50) are additive precision, not the correlation's only carrier.
