# [H1][ASPECT]
>**Dictum:** *AOT is aspect-oriented composition — `checked ▷ logged ▷ traced ▷ retried ▷ op` — woven by `Slot` at exactly two seams, never inline.*

## [1][PURPOSE]

`core/aspect.py` owns the entire cross-cutting layer. It exposes four slot-ordered decorator factories and three weavers: `compose` builds the rail-facing `Hom → Hom` stack, `compose_spawn` lifts the engine-only `retried` `Spawn → Spawn` layer, and `assemble` folds layers into a monotonic-`Slot` chain that surfaces an `Inversion` as a decoration-time `TypeError`. No rail or engine code constructs `structlog`, `opentelemetry`, `stamina`, or `beartype` directly — they consume only these factories. Per [D1] the runtime order is fixed; per [D2] the rail seam carries **no** `retried`; per [D3] the engine seam carries **no** `logged`.

## [2][CANONICAL_SHAPES]

`Hom` is the rail homomorphism (`Result[T, Fault]`); `Spawn` is the engine coroutine on the **exception channel** (no `Result` wrapper — `stamina` retries on raised exceptions). `Layer`/`SpawnLayer` pair a `Slot` with a decorator so the fold sorts by slot. `Inversion` is the structured fault a non-monotonic fold raises.

```python
type Attrs = Mapping[str, object]
type Hook = Callable[[Exception], bool]
type Bind[**P] = Callable[P, Mapping[str, object]]
type Hom[**P, T] = Callable[P, Result[T, Fault]]                    # rail-facing
type Spawn[**P, T] = Callable[P, Coroutine[None, None, T]]          # engine spawn, exception-channeled
type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]
type SpawnLayer[**P, T] = tuple[Slot, Callable[[Spawn[P, T]], Spawn[P, T]]]

class Slot(IntEnum):
    checked = 0; logged = 1; traced = 2; retried = 3               # checked=0 outermost; fold sorts by rank
```

| [SLOT] | [FACTORY] | [LIBRARY] | [SEAM] | [EFFECT] |
| :----: | --------- | --------- | ------ | -------- |
| 0 | `checked(conf=_CONF)` | `beartype` `O1` | both | `_once(beartype(conf))`; shape boundary, `@wraps`-preserved. |
| 1 | `logged(event, keys)` | `structlog` | rail only [D3] | `bound_contextvars(**keys)` + terminal `match res` reading `getattr(done, "status", OK)` / `f.status`,`f.message`,`f.argv` [D28]. |
| 2 | `traced(span, attrs, agent=None)` | `opentelemetry` | both | one span/call; merges `attrs` + optional fleet `agent` projection, binds them normalized (`_correlate`) into contextvars + OTel baggage, then `set_status` from `Result`. `start_as_current_span` exposes the span so the engine's `_diagnose` enriches faults via `trace.get_current_span()`. |
| 3 | `retried(on, attempts, timeout)` | `stamina` | engine only [D2] | `Spawn → Spawn`; never retries `BUSY`/`TIMEOUT` (`_transient` returns `False` for `ResourceBusyError`). |

`Parser`/`Engine`/`Source` are **not** Protocols here [D25] — these are plain callables typed by the PEP 695 aliases above. The module-scope `set_on_retry_hooks([_on_retry])` wires the `stamina` hook that reads `traced`'s bound context back at the engine seam.

## [3][VALIDATED_SNIPPET]

The canonical core pattern: the `assemble` fold algebra (monotonic `Slot` short-circuit) plus the `traced` correlation seam — the load-bearing reason the engine seam carries `run_id` (and the fleet `agent_task_id`) without `logged`. `traced` merges `attrs` with an optional `agent` projection (the settings `agent_context` `{run.id, agent.task.id}` pair), normalizes the union through `_correlate` (strips the `assay.` wire prefix, folds OTel-style dots to `_` so `agent.task.id` → `agent_task_id`, drops empties), binds them into `structlog` contextvars *around* the call, mirrors that context into OTel baggage, and runs outside the `retried` loop so the module-scope `_on_retry` hook (and every child span) reads a populated `run_id` **and** `agent_task_id`. Fault enrichment is **engine-owned**: `traced`'s `start_as_current_span` exposes the live span, and the engine's `_diagnose` stamps `record_exception(exc)` + a `fault.resource_snapshot` event via `trace.get_current_span()` at each fault site — `traced` owns span lifecycle/status, no parallel exception body. `expression.Result` is matched via `Result(tag="ok", ok=…)` / `Result(error=…)` patterns (not `Ok(_)`/`Error(_)`); `ResourceBusyError` is **imported** from `core/model.py` [D40], never redefined.

```python
from operator import itemgetter
from opentelemetry import baggage, context, trace
from structlog.contextvars import bound_contextvars, get_contextvars
from expression import Ok, Result
from expression.collections import block

def assemble[**P, T](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    seed: Result[tuple[Slot, int, Hom[P, T]], Inversion] = Ok((Slot.checked, 0, fn))
    return layers.fold(
        lambda acc, lyr: acc.bind(
            lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2]))).filter_with(
                lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))),
        seed,
    ).map(itemgetter(2))

def _correlate(projected: Attrs) -> dict[str, str]:                 # strip assay., dots→_, drop empties
    return {key.removeprefix("assay.").replace(".", "_"): str(val) for key, val in projected.items() if str(val)}

def traced[**P, T](*, span: str, attrs: Callable[P, Attrs], agent: Callable[P, Attrs] | None = None) -> Layer[P, T]:
    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            projected = {**attrs(*a, **k), **(agent(*a, **k) if agent is not None else {})}
            with bound_contextvars(**_correlate(projected)):              # run_id + agent_task_id into contextvars
                ctx = block.of_seq(tuple(get_contextvars().items())).fold(
                    lambda c, kv: baggage.set_baggage(kv[0], kv[1], context=c), context.get_current())
                token = context.attach(ctx)
                with _TRACER.start_as_current_span(span) as s:           # current span; engine _diagnose enriches faults here
                    s.set_attributes({key: str(val) for key, val in projected.items()})
                    res = fn(*a, **k)
                    match res:
                        case Result(tag="ok", ok=done):
                            s.set_attribute("assay.status", str(getattr(done, "status", RailStatus.OK)))
                            s.set_status(Status(StatusCode.OK))
                        case Result(error=f):
                            s.set_attributes({"assay.status": f.status, "assay.message": f.message[:_ATTR_CAP]})
                            s.set_status(Status(StatusCode.ERROR, f.status))
                    context.detach(token)
                    return res
        return woven
    return (Slot.traced, dec)
```

`assemble` seeds at `Slot.checked` (not `Slot(-1)`) so the first real layer is trivially monotonic, threads `(slot, depth, woven)` through the method-form `Block.fold`, and projects via `itemgetter(2)`. `compose` matches `Result(tag="ok", ok=woven)` and raises `TypeError(inv)` on `Result(error=inv)` with an `assert_never` exhaustiveness guard. The header (`_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)`, `_TRACER`, `_LOG`, `_ATTR_CAP=256`, `_TERMINAL: dict[bool, Callable[..., object]] = {True: _LOG.error, False: _LOG.info}`, `_RING: ContextVar[deque[str] | None]`) precedes `[OPERATIONS]`; `_TERMINAL` keys on the severity predicate (`f.status.severity >= FAILED.severity`), never a removed `returncode` field [D28]. `logged` reads `getattr(done, "status", RailStatus.OK)` (no `.exit_code`). `_RING` plus `ring_processor`/`ring_recent` are the auto-observability seam ([5.1]) — a structlog `Processor`, not a slot.

## [4][SEAMS]

| [CONSUMER] | [IMPORTS] | [USE] |
| ---------- | --------- | ----- |
| `core/model.py` | — provides — | `Fault{argv,status,message}` [D28], `ResourceBusyError`. |
| `core/status.py` | — provides — | `RailStatus.severity` read by `logged`'s terminal dispatch (`>= FAILED.severity`); `RailStatus` is a `StrEnum`. |
| `composition/registry.py` | `compose`, `checked`, `logged`, `traced` | Rail seam = `compose(*_RAIL_LAYERS)(_narrow(bind.handler))` where `_RAIL_LAYERS = (checked(), logged(event="rail", keys=_correlate), traced(span="assay.rail", attrs=_correlate, agent=_agent))` — **no** `retried` [D2]. `_agent` is kept distinct from `_correlate` so the dotted `agent_context` keys flow ONLY through `traced`'s normalizer, never into `logged`'s raw `bound_contextvars`. `_narrow` casts the bound `Handler` so `checked`'s beartype forward-ref resolution succeeds under PEP 649. |
| `core/engine.py` | `compose_spawn`, `compose`, `checked`, `traced`, `retried` | Engine seam = `compose_spawn(retried())(_guarded)` on the spawn, then `compose(checked(), traced(...))` on the lifted `Hom` — **no** `logged` [D3]. `traced(span=tool.name, attrs=…, agent=λ: settings.agent_context)`'s bound contextvars carry `run_id` **and** `agent_task_id` into the `_on_retry` hook; `_diagnose` enriches faults on the same `traced` span via `trace.get_current_span()`. |
| `__init__.py` | — gates — | `structlog.configure(... WriteLoggerFactory(file=sys.stderr))` once; endpoint-gated `set_tracer_provider`; `beartype_this_package` claw only when `ASSAY_CLAW` is set (first statement, pre-import). |

## [5][EXTENSIBILITY]

A fifth concern (e.g. `metered = 4`) is one new `Slot` member plus one `Hom → Hom` factory returning `(Slot.metered, dec)`; `assemble`'s `block.fold` already enforces its monotonic position with zero shape change.

### [5.1][OBSERVABILITY-RING]

Auto-observability is a **structlog `Processor`, NOT a `Slot`/`Layer`** — it rides the configured processor chain (`__init__._configure`), never the 4-slot weave, so it adds **no** aspect slot and does not perturb `checked ▷ logged ▷ traced ▷ retried` ordering. Two module-scope symbols carry it:

- `_RING: ContextVar[deque[str] | None] = ContextVar("assay_ring", default=None)` — an invocation-scoped recent-events ring. `default=None` means the processor is a zero-cost identity outside an invocation.
- `ring_processor(logger, method_name, event_dict) -> EventDict` — the mandated structlog `Processor` signature (`logger` unused per contract). When `(ring := _RING.get()) is not None` it appends a compact bounded summary `f"{event_dict.get('log_level', method_name)}:{event_dict.get('event', '')}"` to `ring` **in place** and returns `event_dict` **UNCHANGED** (pure pass-through, never mutated). `ring_recent() -> tuple[str, ...]` snapshots the live ring (empty when none is seated).

The load-bearing invariant: `registry.rail` **sets the `deque` into `_RING` exactly ONCE per invocation** (a bounded `deque(maxlen=…)`), `ring_processor` **appends by-reference** as every log line flows the chain, and `registry._emit` **reads it back** at envelope time. Because the deque is seated once and mutated by-reference (not re-`set` per log), the recent-events window **survives the `anyio.run` boundary** — the engine spawn runs under a copied context, but the same deque object is shared, so events logged inside `anyio.run` land in the ring the rail thread later reads. This is correlation context, never an event emission, so it is orthogonal to the engine-seam no-`logged` rule [D3] exactly as `traced`'s contextvar bind is.

## [6][CONSIDERATIONS]

- `assemble` threads `(Slot, depth, fn)`, short-circuiting via `acc.bind` + `filter_with`; the `Slot.checked` seed makes the first real layer trivially monotonic and lets `Inversion(st[0], lyr[0], st[1])` report exact outer/inner slots — cheaper than a post-hoc sort that would silently repair caller error.
- `_once` keys on `id(dec)`, so two `compose(checked(...))` calls produce **distinct** tags; idempotency holds only for the *same* factory instance. Wire the four factory results once at module scope in `registry`/`engine` rather than re-invoking `checked()` per call, or the guard cannot dedupe.
- `set_status(Status(StatusCode.ERROR, f.status))` passes the `RailStatus` where a `str` description is expected; because it is a `StrEnum` the value serializes as the wire token across OTel and structlog with no projection — relying on `__str__`, not `repr`.
- Retry correlation is bound in `traced`, **not** `logged` — `logged`'s `bound_contextvars` is rail-only [D3], so the engine seam's `run_id` *and* `agent_task_id` must come from `traced`'s context+baggage bind, which sits outside the `retried` loop (`traced(2) ▷ retried(3)`) so the `stamina` `_on_retry` hook reads both. The `agent` projection (`settings.agent_context` `{run.id, agent.task.id}`) is merged with `attrs` before `_correlate` normalizes the union; an unset `agent_task_id` drops out so absent agent context binds nothing. Binding a context-var is *context*, never an event emission, so this does not violate the engine seam's no-`logged` rule.
- `_correlate(projected)` is the single normalizer both the contextvar bind and the baggage fold consume: it strips the `assay.` wire prefix **and** folds OTel-style dots to `_` (so the dotted `agent.task.id` becomes the valid `agent_task_id` kwarg `bound_contextvars` requires) and drops blank values. Span attributes keep their native wire form (`assay.*`/dotted) via the separate `set_attributes` projection — only the structlog/baggage identifiers are normalized.
- Fault enrichment is engine-owned, not a `traced` arm: `_guarded`/`exclusive_lease` hold the in-hand `BaseException`, so `_diagnose(exc)` calls `trace.get_current_span().record_exception(exc)` + a `fault.resource_snapshot` event on the span `traced` opened. `traced`'s error arm maps only `set_status`/`set_attributes` — adding a second exception event in `traced` would double-emit the fault, so the seam exposes the current span (`start_as_current_span`) and lets the engine own the `record_exception` + resource event.
