# [H1][AUTOMATION_ENGINE]
>**Dictum:** *One `anyio` loop hosts watch and schedule over a shared stop; each fire folds one `Action` into one `Envelope` on the NDJSON stream.*

## [1][PURPOSE]

`automation/engine.py` is the runtime spine of the automation arm. It hosts a `Watch` (`watchfiles.awatch`) loop and a `Schedule` (`aiocron.crontab`) tasklet concurrently under **one** `anyio` task group sharing one `anyio.Event` stop, and on every fire runs the bound `Action` through the same `core/engine` + `composition/registry` rails the CLI uses, emitting **one** `Envelope` per fire as newline-delimited JSON via the engine's own `_emit` writer. It consumes `REGISTRY`/`rail` and `run_check`; it is **not** a `Claim` and never extends them. `drive` is the single public surface; `Trigger`/`Action` decode is owned by `automation/model.py` and treated as given here. A top-level `Debounce` action is the fourth `Action` case: under a looping trigger it coalesces a fire storm into one delayed `inner` fire per `window_ms` quiescence window (an `anyio.move_on_after` re-armable timer), the second backpressure layer beyond the per-drive `CapacityLimiter(1)`.

## [2][CANONICAL_SHAPES]

`drive` is the only entry; the per-trigger loops, the fire fold, and the writer are private. Verbatim contracts:

```python
type Fire = Callable[[], Coroutine[None, None, None]]  # closes over action, settings, limiter, governor

def _emit(line: Envelope) -> None: ...                 # engine-owned NDJSON writer; cached module-local _ENCODER
def _governed(threshold: float | None) -> bool: ...    # None disables; else float(cpu_percent(0.1)) >= threshold*100
async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None: ...
async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None: ...   # ignore_patterns → DefaultFilter(ignore_entity_patterns=…)
async def _schedule(spec: str, fire: Fire, stop: anyio.Event) -> None: ...  # aiocron has no native stop; waits then cron.stop()
async def _emit_leaf(leaf: Action, settings: AssaySettings,
                     limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus: ...
async def _sequence(leaves: tuple[Action, ...], settings: AssaySettings,
                    limiter: anyio.CapacityLimiter, cpu_threshold: float | None,
                    folded: RailStatus = RailStatus.EMPTY) -> RailStatus: ...  # join-fold, halt on policy
def _fire(action: Action, settings: AssaySettings, *,
          limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> Fire: ...  # builds the Action→Envelope closure
async def _quiesce(recv: MemoryObjectReceiveStream[None], window_ms: float) -> None: ...  # move_on_after re-arm; recurses until the window settles
def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Fire]: ...   # (signal, worker) coalescer over a size-1 channel
def _armed(action: Action, settings: AssaySettings, *,
           limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Fire | None]: ...  # Debounce → (signal, worker); else (fire, None)
```

`drive` discriminates the `Trigger` union (`Watch(paths, filter, ignore_patterns, debounce, cpu_threshold) | Schedule(cron, cpu_threshold) | Manual`); `Action` (`Rail | Program | Sequence | Debounce`) is folded by `_fire`. A `Rail` reuses the registry runner `rail(bind)(params)` (which writes its own line); a `Program` runs through `core/engine.run_check` and the engine emits the folded envelope; a `Sequence` recurses into the `join`-fold; a `Debounce` is a coalescing wrapper interpreted by `_armed`/`_debounce` at the trigger seam (and degenerates to a single inner fire under `Manual` / as a `Sequence` leaf). Library binding (verified, dossiers):

| [ROLE] | [CALL] | [SOURCE] |
| ------ | ------ | -------- |
| watch | `awatch(*paths, watch_filter=DefaultFilter(ignore_entity_patterns=spec.ignore_patterns) if spec.ignore_patterns else _FILTERS.get(spec.filter, DefaultFilter()), debounce=spec.debounce, stop_event=stop)` → `AsyncGenerator[set[FileChange], None]` | `lib-watchfiles` |
| schedule | `aiocron.crontab(spec, func=fire, start=False) -> Cron`; launched via `tg.start_soon(cron.start)`; stopped via `cron.stop()` + `cancel_scope.cancel()` | `lib-scheduler` |
| loop | `anyio.run(...)` once at the sync edge; `create_task_group()`; shared `stop = anyio.Event()`; per-drive `anyio.CapacityLimiter(1)` re-entrancy guard | `lib-anyio` |
| debounce | `send, recv = anyio.create_memory_object_stream[None](1)`; `signal` notifies non-blocking (size-1 buffer = the coalescing point); `worker` re-arms `anyio.move_on_after(window_ms/1000)` around `recv.receive()` per signal | `lib-anyio` |
| governor | optional `Watch/Schedule.cpu_threshold: float\|None`; `float(psutil.cpu_percent(0.1)) >= threshold*100.0` before a fire → emit `Completed{SKIP}` and skip, never wait | `lib-psutil` |

`stop` and the `CapacityLimiter(1)` are constructed in `drive` *before* the match, so a single `stop.set()` collapses either trigger and every leaf of a `Sequence` shares one re-entrancy token. `Manual` bypasses both loops: `drive` awaits `fire()` once and returns. No inline `break`/`while`/`for`: `awatch` is an `async for` generator, `cron.start` is loop-resident, `_sequence` is a recursive head/tail `match`-fold, and the debounce `worker`/`_quiesce` re-arm via recursion + statement-`match` on `scope.cancelled_caught` (no inline `while`). The limiter serializes fires so a fire slower than its trigger cadence queues rather than re-entering a leased `Action` into spurious `BUSY` (which `@retried` will not absorb). `Watch.ignore_patterns` is an entity-name-regex tuple folded into a runtime `DefaultFilter(ignore_entity_patterns=…)` (only `DefaultFilter` exposes the ctor kwarg; `BaseFilter` is the annotation/value type, ctor-arg-free) — the wire stays a string tag + glob tuple, no `watchfiles` subclass leaks onto it.

## [3][VALIDATED_SNIPPET]

The core pattern: one fire = one `Action` → one `Envelope` per leaf → one NDJSON line; `_sequence` folds by module-scope `join` and halts on a definitive defect or any `Fault` leaf.

```python
import anyio
from expression import Result
from tools.assay.automation.model import Action, Debounce, Manual, Program, Rail, Schedule, Sequence, Watch
from tools.assay.core.model import Completed, Envelope, Fault, Report, envelope, fold
from tools.assay.core.status import join, RailStatus
from watchfiles import DefaultFilter

async def _emit_leaf(leaf: Action, settings: AssaySettings,
                     limiter: anyio.CapacityLimiter, cpu_threshold: float | None) -> RailStatus:
    match _governed(cpu_threshold):
        case True:                                          # governor: emit Completed{SKIP}, never wait
            claim, verb = _label(leaf)                       # Debounce → _label(inner): the wrapper has no rail identity
            skipped = Completed(argv=(), returncode=0, status=RailStatus.SKIP, notes=(f"governed: cpu>={cpu_threshold or 0.0:.0%}",))
            return _emitted(envelope(fold(claim, verb, (skipped,)), claim=claim, verb=verb)).status
        case False:
            async with limiter:                             # per-drive CapacityLimiter(1): single-flight
                match leaf:
                    case Rail() as r:
                        return _rail_outcome(r).status       # registry writes its own line; engine returns it
                    case Program() as p:
                        return _program_envelope(p, settings).status
                    case Sequence() as s:
                        return await _sequence(s.actions, settings, limiter, cpu_threshold)
                    case Debounce(action=inner):
                        return await _emit_leaf(inner, settings, limiter, cpu_threshold)  # degenerate: no storm in a one-shot fold

async def _quiesce(recv: MemoryObjectReceiveStream[None], window_ms: float) -> None:
    with anyio.move_on_after(window_ms / 1000.0) as scope:   # re-armable timer: a fresh signal lands before it elapses
        await recv.receive()
    match scope.cancelled_caught:
        case True:                                          # window elapsed clean → the storm has settled
            return
        case False:                                         # a fresh signal re-armed the window → keep draining
            await _quiesce(recv, window_ms)

def _debounce(inner: Fire, window_ms: int, *, collapse: bool) -> tuple[Fire, Fire]:
    send, recv = anyio.create_memory_object_stream[None](1)  # size-1 buffer: a second pending signal is the coalescing no-op
    async def signal() -> None:
        match send.statistics().current_buffer_used:
            case 0:
                send.send_nowait(None)
            case _:
                return                                      # already pending → drop (coalesce)
    async def worker() -> None:
        await recv.receive()
        match collapse:                                     # leading edge fires immediately when collapse=False
            case False: await inner()
            case True:  pass
        await _quiesce(recv, window_ms)
        match collapse:                                     # trailing edge fires once the storm settles when collapse=True
            case True:  await inner()
            case False: pass
        await worker()                                      # await the next storm — recursion is the loop-free re-arm
    return signal, worker

async def _watch(spec: Watch, fire: Fire, stop: anyio.Event) -> None:
    match spec.ignore_patterns:                             # entity-name regexes refine the base vocabulary tag
        case ():
            watch_filter: BaseFilter = _FILTERS.get(spec.filter, DefaultFilter())
        case patterns:
            watch_filter = DefaultFilter(ignore_entity_patterns=patterns)  # only DefaultFilter exposes the ctor kwarg
    async for _changes in awatch(*spec.paths, watch_filter=watch_filter, debounce=spec.debounce, stop_event=stop):
        await fire()

def _armed(action: Action, settings: AssaySettings, *,
           limiter: anyio.CapacityLimiter, ceiling: float | None) -> tuple[Fire, Fire | None]:
    match action:                                           # a top-level Debounce installs the (signal, worker) coalescer
        case Debounce(action=inner, window_ms=window, collapse=coalesce):
            return _debounce(_fire(inner, settings, limiter=limiter, cpu_threshold=ceiling), window, collapse=coalesce)
        case Rail() | Program() | Sequence():
            return _fire(action, settings, limiter=limiter, cpu_threshold=ceiling), None

async def drive(trigger: Trigger, action: Action, settings: AssaySettings) -> None:
    limiter = anyio.CapacityLimiter(1)                       # per-drive: one fire in flight
    stop = anyio.Event()                                     # one stop, constructed before the match
    async def _co_resident(tg: TaskGroup, worker: Fire | None) -> None:
        match worker:
            case None:
                return
            case _:
                tg.start_soon(worker)                       # the worker is co-resident under the same tg/stop
                await stop.wait()
                tg.cancel_scope.cancel()                    # the worker blocks on its channel; cancel collapses it with the trigger
    match trigger:
        case Manual():
            await _fire(action, settings, limiter=limiter, cpu_threshold=None)()  # Debounce degenerates here
        case Watch(cpu_threshold=ceiling) as spec:
            fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
            async with anyio.create_task_group() as tg:
                tg.start_soon(_watch, spec, fire, stop)      # awatch honors stop natively
                await _co_resident(tg, worker)
        case Schedule(cron=cron_spec, cpu_threshold=ceiling):
            fire, worker = _armed(action, settings, limiter=limiter, ceiling=ceiling)
            async with anyio.create_task_group() as tg:
                tg.start_soon(_schedule, cron_spec, fire, stop)
                await _co_resident(tg, worker)
```

`join` is the module-scope semilattice operator from `core/status`, not a method: `RailStatus` subclasses `str` whose own `join` is `str.join`, so the algebra is lifted off the enum to avoid that collision. `__main__` enters via `anyio.run(drive, trigger, action, settings)` — the single `anyio.run` for the process. A combined `Watch + Schedule` (future) spawns both `_watch` and `_schedule` under the **same** `tg` against the **same** `stop`. A top-level `Debounce` arms the trigger via `_armed`: the loop awaits the coalescing `signal` while the `worker` runs co-resident; `_co_resident` cancels the scope on `stop` so the channel-blocked worker collapses with the trigger.

## [4][SEAMS]

| [NEIGHBOR] | [DIRECTION] | [CONTRACT] |
| ---------- | :---------: | ---------- |
| `automation/model.py` | in | `Trigger`/`Action` tagged unions; `drive` matches the cases, never re-parses tokens. The watch `filter` is a string tag; `engine` resolves it via `_FILTERS` (or a `DefaultFilter(ignore_entity_patterns=Watch.ignore_patterns)` when the tuple is populated) so the model never imports `watchfiles`. `Debounce(action, window_ms, collapse)` is the fourth `Action` case: `engine` interprets it at the trigger seam via `_armed`/`_debounce`, the data stays inert. |
| `composition/registry.py` | in | `_resolve` looks up the `Bind` row by `(claim, verb)`; `rail(bind)(params)` weaves `checked ▷ logged ▷ traced` (NO `retried` — a rail is a `Hom`) and writes its OWN `Envelope` (the registry is the sole writer for a rail). The engine returns it without re-emitting; an unbound verb or malformed `params` folds to a `FAULTED` envelope written through the engine's own `_emit`. |
| `core/engine.py` | in | `run_check` executes a `Program` and owns the `compose_spawn(retried()) ▷ compose(checked, traced)` seam; retry correlation rides `traced` (the engine seam), never `logged`. A non-zero exit rides the `Ok` channel as `Completed{FAILED}`; only spawn failure / timeout takes `Error`. |
| `composition/settings.py` | in | `ArtifactScope.open(settings, Claim.STATIC)` scopes a `Program` fire's artifacts. The CPU governor ceiling is **not** a settings knob — it is the `Watch/Schedule.cpu_threshold: float\|None` field read off the trigger spec. |
| `core/model.py` | in | `fold` is the sole count-derivation site; `envelope(payload, claim=, verb=)` projects `status = payload.status`, `exit = status.exit_code`. The engine never authors a `Fault` for an exit code. |
| `__main__.py` | out | The root `App` automation verb resolves `Trigger`+`Action`, then calls `anyio.run(drive, …)`; consumes the streamed NDJSON exit semantics. |

## [5][EXTENSIBILITY]

A fourth trigger (e.g. a `Webhook` meta-watch) or a fifth action (e.g. `Pipeline`) is one new tagged case in `automation/model.py` plus one `match` arm in `drive`/`_fire`/`_armed`/`_emit_leaf`/`_label` — no new module, no loop rewrite; the shared-`stop` task group already hosts arbitrary co-resident drivers, exactly as the `Debounce` `worker` does. `Debounce` itself was the first such extension: a coalescing `Action` whose interpretation lives entirely in `_armed`/`_debounce`/`_quiesce`/`_co_resident` (a `(signal, worker)` pair spawned co-resident under the trigger's `tg`/`stop`), the data inert in the model.

## [6][CONSIDERATIONS]

- [STREAM-FLUSH] NDJSON correctness demands `_emit` flush per fire, not buffered batching — a long-lived `Watch` whose stdout is piped to a consumer must surface each `Envelope` at fire time, not at process exit. The engine owns its OWN `_emit` (and module-local `_ENCODER`); it is a distinct writer from `registry._emit(bind, settings, started, outcome)`, which writes for a rail. Per-line flushing is what keeps line-framing safe under the concurrent `cron.start` task and the per-leaf emits of a `Sequence` fold.
- [DEBOUNCE-vs-FIRE-COST] `awatch(debounce=…)` coalesces at the Rust layer *before* yield, but it does **not** serialize fire execution; a fire slower than the inter-batch interval would re-enter while the prior is in flight. The per-drive `anyio.CapacityLimiter(1)` (`async with limiter` in `_emit_leaf`) is therefore **canonical, not conditional**: every fire acquires the single token, so a slow `static build` fire cannot collide with its own next batch and spuriously hit `BUSY` (which `@retried` will not absorb). The limiter is constructed once in `drive` and shared by every leaf of a `Sequence` so the whole fold stays single-flight.
- [GOVERNOR-as-SKIP-not-WAIT] When the `Watch/Schedule.cpu_threshold` ceiling trips (`float(psutil.cpu_percent(0.1)) >= threshold*100.0`), the fire emits a `Completed{status=SKIP}` Envelope (exit 0, severity 0) rather than block — automation never waits on contention; a skipped fire stays on the stream as a first-class observable, preserving the one-Envelope-per-fire invariant and giving the fleet operator a backpressure signal without stranding the loop. The 0.1s `cpu_percent` interval blocks the fire task briefly but not the `awatch`/`cron` drivers; raise it only if the sample proves too noisy for the fleet cadence.
- [SCHEDULE-STOP-ASYMMETRY] `awatch` honors the shared `stop_event` natively, but `aiocron` exposes none, so `_schedule` launches `cron.start` alongside a one-shot `stop.wait()`; when stop arrives it calls `cron.stop()` then `cancel_scope.cancel()`. This is the only place graceful shutdown is hand-stitched — the asymmetry is the reason `_schedule` is a separate coroutine rather than inline `tg.start_soon(cron.start)`.
- [DEBOUNCE-SECOND-BACKPRESSURE-LAYER] The `CapacityLimiter(1)` serializes *execution* (a slow fire cannot collide with its own next batch) but every coalesced trigger still queues one fire. A `Debounce` action adds the orthogonal *admission* layer: the size-1 `create_memory_object_stream` is the coalescing point (a second pending `signal` is a no-op via `statistics().current_buffer_used`), and the `worker` re-arms `anyio.move_on_after(window_ms/1000)` around `recv.receive()` until the window settles, collapsing an N-event storm to one `inner` fire. `collapse=True` is trailing-edge (storm → one delayed run, the default — best for re-running `static build` after a settle); `collapse=False` is leading-edge (fire immediately, suppress the trailing tail — best for instant feedback). The two layers compose: debounce thins admission *before* the limiter serializes execution. The `worker`/`_quiesce` recursion is the loop-free re-arm; the channel-blocked `worker` is collapsed by `_co_resident`'s `stop`-triggered `cancel_scope.cancel()` (it cannot observe `stop` while blocked on `receive()`), the same hand-stitch shape as `_schedule`. A `Debounce` under `Manual` (or as a `Sequence` leaf) degenerates to a single inner fire via `_fire`/`_emit_leaf` — there is no storm to coalesce in a one-shot fold.
- [IGNORE-PATTERNS-vs-FILTER-TAG] `Watch.filter` selects a *preset* `BaseFilter` (`"default"`/`"python"`) from `_FILTERS`; `Watch.ignore_patterns` overrides it with a runtime `DefaultFilter(ignore_entity_patterns=…)` when populated — the two are mutually exclusive at resolution (a non-empty `ignore_patterns` wins, so domain globs are not silently merged onto a `PythonFilter`). The kwarg is `DefaultFilter`-only: `BaseFilter.__init__` takes no args and reads its class-attribute regexes, so `BaseFilter` is the annotation/value type while `DefaultFilter` is the sole constructable surface for wire-supplied patterns. The patterns are *entity-name* regexes (matched against `path.split(os.sep)[-1]`), not full-path globs — `r"\.tmp$"`/`"node_modules"`, not `"**/build/**"`; the wire stays a string tag plus a regex tuple, no `watchfiles` subclass leaks onto it.
```

