# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration: `LanePolicy.drain` is the one polymorphic bounded drain over the three-case `Admit[T]` admission union, `LanePolicy.offload` the one kernel-isolation hop over the `execution/workers#CROSSING` `Kernel` crossing, `StagePlan.execute` the concurrent-front stage DAG over that same drain, and `LaneSource` the one `scheduled`/`watched` feeder union — one budget, one receipt shape, one owner for every bounded lane the branch runs.

`drain` and `offload` share one deadline budget and one isolation axis, so the deadline contract is total across both hops, and a caller supplies its kernel — the lane never imports one. `LanePolicy.of` projects capacity and deadline from the admitted `execution/admission#CONTEXT` profile row and scopes one pulse actor across the lane lifetime, so a consumer mints neither bounds nor actor custody. Offload isolation, worker-death retry, wire, shipping, and per-offload deadline all arrive on the `Kernel` value — the workers owner answers the isolation question once, this page owns the thread and subinterpreter crossing arms and routes both process arms onto the `execution/workers#POOL` capsule. Cross-cutting concerns ride aspects, never inline call sites: the OTel trace context stitches across every crossing, content-keyed work short-circuits on a cache hit that threads forward across `StagePlan` fronts, `reliability/resilience#RESILIENCE` `guard` rides each unit's `RetryClass`, and the `Metrics.observe`/`Signals.emit_async` pair rides one `drained` aspect under the lane's own composition scope. Occupancy is the one lane fact no aspect carries: a drain reports what finished, so the live borrowed-slot level registers once at the scoped constructor and the metric export cycle samples it — the per-lane limiter under the `lane` band and the process-wide `THREAD_BAND` under a refcounted single registration the same bracket owns. Mid-operation kernel facts ride one `PulseConduit` per lane into a parent-side serialized drain folding `Hooks.fire` under the conduit's own composition `ScopeKey`, so hook taps observe long-running kernels without polling receipts and a non-default composition's registered points receive their own beats. Bare `asyncio` is never imported — `anyio` owns every concurrency primitive, and cron is solely `apscheduler`.

## [01]-[INDEX]

- [02]-[LANE]: bounded `drain` over the `Admit[T]` union, the `Kernel`-crossing `offload`, the concurrent-front `StagePlan`, the `LaneSource` feeder union under one scope-carrying `drained` aspect, and the `PulseConduit` mid-operation fact spine.

## [02]-[LANE]

- Owner: `Admit[T]` discriminates a plain coroutine, a content-keyed cache unit, and a resilience-guarded unit by case, so one `drain` serves all three rather than three parallel methods, and `ADMIT_TABLE` folds each case through one behavior row — a new admission modality is one row, never a new method. `DrainReceipt[T]`/`DrainOutcome` and the `DRAIN_COLUMNS`/`DRAIN_DISPOSITIONS` pair derived from it are the one canonical drain taxonomy IMPORTED from their `observability/receipts#RECEIPT` owner: a drain receipt is local evidence, so the vocabulary lives one tier down and this page, the `observability/metrics#METRIC` counter, and the receipts emit all read that one owner. `LaneSource` is a feeder union over the one drain, never a separate scheduler surface.
- Law: mid-operation kernel facts cross `LanePolicy.pulses`, one `PulseConduit` per lane — a spawn-context manager-queue proxy every arm pickles as an ordinary kernel argument, written through `pulsed` alone, lossy by design: a full conduit or dead broker drops the pulse, so telemetry never back-pressures or faults a kernel. Every authorized drop counts onto its own `rasm.runtime.pulse.*` census row under the conduit's own composition scope — the same identity the drained aspect stamps — so the lossy paths stay measured rather than silent, two embedded conduits' drops never merge into one unlabeled series, and the drain actor cannot fault on its own accounting. One parent-side lane actor serializes the fold — a `THREAD_BAND` pump relays the proxy through `anyio.from_thread.run_sync` onto a bounded anyio stream (`send_nowait` `WouldBlock` is the authorized drop) and ONE consumer posts each fact to `Hooks.fire`, so taps observe pulses in conduit order and no worker reaches the hook registry or a live span. Conduit-member verdict: the `pebble` map-iterator streams per-chunk terminals, never mid-operation facts, and `pebble` ships no pipe member; the `expression` `MailboxProcessor` inbox reaches `asyncio` against the anyio law — the manager proxy with the anyio single-consumer drain is the ruled conduit. Folder pulse vocabularies stay folder-owned `HookPoint` rows; the spine carries only the crossing and the drain, and the scoped constructor retires the actor with one shielded, grace-bounded close token after producers stop — a live pump admits the token inside the grace window with no data evicted, a dead pump's full conduit unblocks by evicting one pulse as the authorized counted drop — so teardown always returns and no concurrent drain consumes another drain's terminus.
- Entry: `LanePolicy.of(context)` is the one scoped constructor — capacity reads the profile row's `lane_capacity`, deadline the context budget, `pulses` opens one lane-lifetime conduit actor, and `Metrics.occupied` registers the limiter's borrowed-slot read for that same lifetime — so bounds, actor custody, and occupancy visibility trace to one admitted owner. The same bracket refcounts the process-wide `THREAD_BAND` registration: that limiter bounds every thread crossing, guest, and `on_thread` hop yet owns no lifecycle, so one probe object opens its series on the first lane and retires it with the last, where a per-lane probe would sum one borrowed count once per live lane. Concurrent `drain` calls share that single consumer; only the constructor's exit closes it after all composing work has stopped. `offload` accepts a `Kernel` or a bare callable it lifts through `Kernel.of`, passes the workers-owned `admitted` gate before the span export so a refused guest module never reaches a worker or a band token; the kernel's trait row supplies isolation kind and worker-death retry, its `deadline` tightens the lane budget to whichever bound is sooner, its `wire` selects the shared-memory span export around the whole hop, and its `TERMINAL` enforcement routes the hop through the workers pebble arm regardless of trait — process isolation is the kill-capable substrate for native code, so a hung native kernel dies at wall-clock instead of outliving a cooperative cancel, while a `SANDBOXED` kernel already dies in-process at its guest epoch deadline and rides the thread arm with no pebble re-route. Cooperative `HOSTILE` kernels ride the warm loky pool, whose `submit` owns the carrier stitch, the `WORKER_BAND` bound, and the in-band worker-death retry. Each `StagePlan` stage picks its own admission case — a cacheable stage mints `keyed` units, a transient-prone stage `retried`, a plain stage `bare` — and each front's `DrainReceipt.cache` threads forward, so a `keyed` unit re-admitted downstream replays the upstream `Ok` rather than recomputing.
- Auto: a tripped deadline is contained — the drain runs inside `move_on_after`, never a bare `fail_after` whose `TimeoutError` escapes as a raw `BaseExceptionGroup` — so a deadline trip cancels in-flight units and the receipt reports them as `cancelled` with the partial `values`/`faults` intact; no exception escapes a bounded lane without a receipt. Each unit sends its full `RuntimeRail[T]` over the stream rather than a pre-collapsed bool, so the typed fault survives into the receipt and the drain is lossless in both directions. Every schedule geometry the package ships is one `Trigger` member on the one `AsyncIOScheduler` — no `croniter`, no `aiocron`, no hand-rolled sleep cron.
- Auto: session cache state is an immutable `Map[ContentKey, T]` threaded on `DrainReceipt.cache` — a hit reproduces the exact `Ok` value and counts as `hit` distinct from `completed`, only an `Ok` folds back, and an `Error` re-runs while its fault accumulates. This cache is session-local in-memory, never a durable store: durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire. The receipts-owned `Cost` brackets the whole drain window — two own-process reads landing the drain's spend envelope on `DrainReceipt.cost`, kernel-grain attribution staying the worker gate's own bracket.
- Auto: trace stitching parents per the arm's module-state reality — every crossing carries the injected carrier, the anyio arms through `traced_kernel` directly and the pooled arms through `WorkerPool.submit`'s own injection. `THREAD` shares the interpreter, so the installed composite propagator is present and the kernel parents unconditionally; the process and subinterpreter workers hold independent module state, so a worker that has not run the `observability/telemetry#TELEMETRY` install resolves the default text-map and runs unparented while the carrier still holds the parent for any span the kernel opens. Pickled arms pay the IPC hop the `THREAD` arm skips — the arm keys off the kernel's declared trait, never a pickle-versus-no-pickle guess, and a closure crosses the pickled arms by value because `Kernel.of` ships it. A worker death retries under the kernel's trait-supplied retry row or crosses the one `async_boundary` conversion, never a local catch.
- Law: every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.LANES`, and the three deadline re-stamps compose the owner's `expired` fold rather than each spelling the budget-unknown floor: `Option[float]` is the budget carrier and `BUDGET_UNKNOWN` is DECLARED once at the fault owner, so a lane that held no bound and one that measured one can never read alike. The kernel name rides the tripped-axis `cause`, since a per-kernel subject would spell a raise coordinate the roster never declares.
- Law: `driven` is the ONE dependent-front drive in the branch and every axis a consumer varies is a parameter — the front SOURCE (`Fronts`, a closed two-case admission over a dependency graph or an already-resolved labelled ladder), the carried FOLD, the optional per-front `gate`, and the warm elision `cache` the FIRST front probes, since only the entry front can elide work a prior run already keyed and every later front reads its predecessor's receipt. Its three refusals are the DRIVE's own and settle per front: a front's fault block reduces through `BoundaryFault.combine` and stops the walk, since a refused front is a dependency the next front reads through the threaded cache; a cancelled front rails the lane's declared deadline, `cancelled` counting admissions the scope killed rather than units that failed; and a registered VETO point decides between waves. A caller re-spelling any of the four drifts out of the other three, which is what one owner forecloses.
- Law: the front source is ONE input discriminant and never a sibling method — a graph admission resolves its own topological waves, a resolved admission walks the labelled fronts it was handed, and `declared` answers the unit total only where the source honestly holds one, so a progress reader never takes a graph's unknown count for a whole job.
- Law: mid-operation pulse payloads are the `observability/hooks#HOOKS` `StageMark` — position, units closed, and an `Option` total — so this conduit carries ONE mark shape for every producing lane and a folder closes its own `<Lane>Stage(StrEnum)` at the site rather than minting a mark of its own.
- Growth: a new lane source is one `LaneSource` case with one `_events` arm; a new mid-operation fact is one folder-owned `HookPoint` row written through `pulsed`, never a drain or conduit edit; a new admission modality one `Admit` case with one `ADMIT_TABLE` row; a new anyio-substrate isolation kind one workers-owned `WorkerKind` member with one `_ISOLATION` row, a new pooled kind one workers arm — every offload call site untouched either way, and a kind whose row has not landed rails a typed `config` refusal at the offload, never a lookup raise; a new trigger one `Trigger` member; a new stage one `StagePlan` edge; a new watch tuning one `Watch` field; a new drain outcome dimension one member on the receipts-owned `DrainOutcome` with its field, reaching the receipt emit through the imported `DRAIN_COLUMNS` and the metrics counter through that owner's `DRAIN_DISPOSITIONS` carve.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` the `scheduled` case mints, no second cron owner, no app lifecycle hook, no background loop without a drain receipt, and no unbounded task creation; a blocking leg outside a lane rides `on_thread`, so every plain thread hop in the branch is `THREAD_BAND`-bounded by construction, and the pooled settle hop rides the workers-owned `WORKER_BAND`. Consumer contract on the receipt is column-driven: the receipts line reads every count off `DRAIN_COLUMNS` and the metrics counter off the `DRAIN_DISPOSITIONS` carve, per column and never a full-struct `asdict` allocating the receipt's containers per export cycle.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import threading
from collections.abc import AsyncIterator, Awaitable, Callable, Iterator, Sequence
from contextlib import ExitStack, asynccontextmanager, contextmanager
from functools import cache, wraps
from graphlib import TopologicalSorter
from multiprocessing import get_context
from multiprocessing.managers import SyncManager
from os import PathLike, process_cpu_count
from queue import Empty, Full, Queue
from typing import Final, Literal, Self, assert_never

import anyio
import anyio.from_thread
import anyio.to_interpreter
import anyio.to_thread
from anyio import BrokenResourceError, CapacityLimiter, WouldBlock, move_on_after
from anyio.streams.memory import MemoryObjectReceiveStream, MemoryObjectSendStream
from apscheduler.events import EVENT_JOB_ERROR, EVENT_JOB_EXECUTED, EVENT_JOB_MISSED, JobExecutionEvent
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.calendarinterval import CalendarIntervalTrigger
from apscheduler.triggers.combining import AndTrigger, OrTrigger
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import propagate
from watchfiles import BaseFilter, Change, PythonFilter, awatch

from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import (
    LANES_EXPORT,
    LANES_FRONT,
    LANES_INLINE,
    LANES_ISOLATION,
    LANES_OFFLOAD,
    BoundaryFault,
    Catch,
    RuntimeRail,
    async_boundary,
    boundary,
    expired,
)
from rasm.runtime.hooks import HookId, Hooks, StageMark
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Cost, DrainReceipt, Receipt, Redaction, ScopeKey, Signals
from rasm.runtime.resilience import RetryClass, guard
from rasm.runtime.workers import Enforcement, Kernel, WorkerKind, WorkerPool, admitted, exported, released, shipped, traced_kernel

# --- [TYPES] ----------------------------------------------------------------------------

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger | CalendarIntervalTrigger | AndTrigger | OrTrigger
type AdmitTag = Literal["bare", "keyed", "retried"]
type IsolationArm = Callable[..., Awaitable[object]]
type PulseFact = tuple[HookId, Struct]  # registered HookPoint member + its payload — `StageMark` for every long-fold pulse point
type Front[T] = tuple[str, Block[Admit[T]]]  # a LABELLED front: the label names the wave a gate mark and a deadline refusal both carry


@tagged_union(frozen=True)
class Admit[T]:
    tag: AdmitTag = tag()
    bare: Work[T] = case()
    keyed: tuple[ContentKey, Work[T]] = case()
    retried: tuple[RetryClass, Work[T]] = case()


@tagged_union(frozen=True)
class Fronts[T]:
    # the ONE front-source discriminant every dependent drive admits through. A caller holding a dependency GRAPH
    # hands its stage roster, its edges, and the per-stage unit projection, and the drive resolves the topological
    # waves itself; a caller holding an ALREADY-RESOLVED ladder — a CPM pass, a hand-ordered wave set — hands the
    # labelled fronts whole. Two admissions and ONE drive: a sibling method per source forks the per-front gate, the
    # fault short-circuit, and the deadline rail three ways and lets one arm silently drift out of the other's
    # guarantees, which is exactly the hand-rebuilt drive this union exists to retire.
    tag: Literal["graph", "resolved"] = tag()
    graph: tuple[tuple[tuple[str, RetryClass], ...], tuple[tuple[str, str], ...], Callable[[str, RetryClass], Sequence[Admit[T]]]] = case()
    resolved: Block[Front[T]] = case()

    @property
    def declared(self) -> Option[int]:
        # total ADMITTED units where the source can answer honestly: a resolved ladder holds every front already, so
        # it sums; a graph runs its projection per wave, so it knows none until the walk reaches one and says so
        # rather than publishing a count a progress reader would take for the whole job.
        match self:
            case Fronts(tag="resolved", resolved=fronts):
                return Some(sum(len(units) for _label, units in fronts))
            case Fronts(tag="graph"):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def walked(self) -> Iterator[Front[T]]:
        # the graph arm's `done` runs when the DRIVE resumes this generator, which is after that front's drain
        # returned, so the stateful graphlib walk stays a front source and the drive holds no second graph; loop
        # depth is fronts, never nodes. The label carries the wave's own stage names, so a gate mark and a deadline
        # refusal both name which wave they answered.
        match self:
            case Fronts(tag="resolved", resolved=fronts):
                yield from fronts
            case Fronts(tag="graph", graph=(stages, edges, work)):
                classes = dict(stages)
                order: TopologicalSorter[str] = TopologicalSorter({stage: () for stage in classes})
                for parent, child in edges:
                    order.add(child, parent)
                order.prepare()
                while order.is_active():
                    front = order.get_ready()
                    yield ",".join(front), Block.of_seq([unit for stage in front for unit in work(stage, classes[stage])])
                    order.done(*front)
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class LaneSource[T]:
    tag: Literal["scheduled", "watched"] = tag()
    scheduled: tuple[Trigger, Callable[[JobExecutionEvent], Block[Admit[T]]]] = case()
    watched: tuple["Watch", Callable[[set[tuple[Change, str]]], Block[Admit[T]]]] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

FIRE_MASK: Final[int] = EVENT_JOB_EXECUTED | EVENT_JOB_ERROR | EVENT_JOB_MISSED
FIRE_BUFFER: Final[int] = 64
PULSE_BUFFER: Final[int] = 256  # bounds the conduit proxy and the drain stream alike; overflow is the authorized lossy drop
CLOSE_GRACE_S: Final[float] = 2.0  # bounded close window: a live pump admits the control token well inside it; expiry proves the pump dead

# process-wide thread band: bounds every THREAD-kind crossing and every `on_thread` hop; its process-pool counterpart
# `WORKER_BAND` lives with the pool owner at execution/workers#POOL, and consumers arrive as ledger `python:` rows,
# never as sibling-minted limiters beside these bands.
THREAD_BAND: Final[CapacityLimiter] = CapacityLimiter(2 * (process_cpu_count() or 4))

# the two raise surfaces this page fences, each named once. Span export reaches the shared-memory allocator and the
# buffer protocol, so its classes enumerate. A kernel body does NOT: it is caller-supplied work whose raise surface no
# runtime can roster, and an unclassified raise crossing an offload loses the whole lane's receipt, so the guest fences
# carry the ONE catch-all this producer plane is allowed and every other fence on the page names its provider set.
_EXPORT_RAISES: Final[Catch] = (OSError, BufferError, TypeError, ValueError)
_GUEST_RAISES: Final[Catch] = Exception

# --- [MODELS] ---------------------------------------------------------------------------


class AdmitRow[T](Struct, frozen=True):
    key: Callable[[Admit[T]], Option[ContentKey]]
    make: Callable[[Admit[T]], Work[T]]


class Watch(Struct, frozen=True):
    # filter and the debounce/step batching axis are case DATA, so a consumer tunes batching without a new source case.
    paths: tuple[str | PathLike[str], ...]
    filter: BaseFilter | None = None
    debounce: int = 1600
    step: int = 50

    @staticmethod
    def facts(batch: set[tuple[Change, str]]) -> Block[tuple[str, str]]:
        # watch-fact receipt form is the lowercase `raw_str()` member name — never `str(change)` or the IntEnum value.
        return Block.of_seq(sorted((change.raw_str(), path) for change, path in batch))


# --- [SERVICES] -------------------------------------------------------------------------


@cache
def _pulse_manager() -> SyncManager:
    # one spawn-context broker per interpreter: every lane conduit's proxy rides this manager process, and spawn pins
    # crossing semantics exactly as the worker pools do — never a platform-defaulted fork.
    return get_context("spawn").Manager()


# --- [THREAD_BAND_CUSTODY]

# `THREAD_BAND` is ONE process-wide limiter, so exactly one probe may ever hold its `thread` series: a per-lane
# registration sums the SAME borrowed count once per live lane and publishes a saturation no limiter is carrying —
# the defect the pool's single `worker` registration already forecloses across its arms. Custody therefore refcounts
# across concurrent lanes: the first lane opens the series, the last one out retires it, and a band nobody bounds
# publishes no point. The registration takes no `scope` because the limiter is process-global — its level belongs to
# the default sink rather than to whichever composition happened to open the first lane.
_BAND_GATE: Final[threading.Lock] = threading.Lock()
_BAND_PROBES: Final[ExitStack] = ExitStack()
_BAND_HOLDERS: Final[list[object]] = []


def _thread_occupancy() -> int:
    # the one probe OBJECT this band ever registers: `occupied` keys registration on identity, so a module-level read
    # is what keeps the single registration single no matter how many lanes open under it.
    return THREAD_BAND.borrowed_tokens


@contextmanager
def _thread_band() -> Iterator[None]:
    # lane-entry refcount under a plain thread lock, not an anyio primitive: a sibling daemon's own event loop opens
    # lanes against this same process-global band, so the gate has to hold across loops.
    token = object()
    with _BAND_GATE:
        _BAND_HOLDERS.append(token)
        if len(_BAND_HOLDERS) == 1:
            _BAND_PROBES.enter_context(Metrics.occupied(_thread_occupancy, band="thread"))
    try:
        yield
    finally:
        with _BAND_GATE:
            _BAND_HOLDERS.remove(token)
            if not _BAND_HOLDERS:
                _BAND_PROBES.close()  # the stack empties rather than dies; the next first lane re-enters it


class LanePolicy(Struct, frozen=True):
    # `limiter` is lane-LIFETIME state exactly as `pulses` is, so it mints at the scoped constructor and rides the
    # value — never a `@cache` keyed on this struct. That memo pinned the whole policy, its conduit, and its manager
    # queue proxy for the process's life: `of`'s `finally` retired the actor while the cache still held the proxy, so
    # every lane a daemon opened leaked one limiter and one live broker handle with no eviction and no drain row. It
    # also made hashability load-bearing on a struct carrying a proxy object, which is a constraint the field deletes.
    capacity: int
    pulses: "PulseConduit"
    limiter: CapacityLimiter
    deadline: Option[float] = Nothing

    @classmethod
    @asynccontextmanager
    async def of(cls, context: RuntimeContext, *, scope: ScopeKey = DEFAULT_SCOPE) -> AsyncIterator[Self]:
        # one scoped lane constructor: capacity, deadline, the single lane-lifetime pulse actor, and the occupancy
        # registration derive together; `scope` is the composition identity the conduit binds so drained pulses fire
        # on the registering scope. `occupied` hands the metrics owner this lane's borrowed-slot read for exactly the
        # lane's lifetime under the `lane` band, so `rasm.band.in_flight` samples live lane concurrency on the export
        # cycle instead of replaying a finished drain's remainder, concurrent lanes under one composition sum into
        # that band alone, and a retired lane leaves none. Naming the band is what keeps a saturated lane readable
        # beside a saturated worker pool rather than folded into one number. The lane bracket is also where the
        # branch's WIDEST bound gets its series: `THREAD_BAND` bounds every THREAD-kind crossing, every guest, and
        # every `on_thread` hop, yet owns no lifecycle of its own, so its custody refcounts here — one registration
        # for the one limiter, opened by the first lane and retired by the last.
        capacity = context.policy.lane_capacity
        policy = cls(capacity=capacity, pulses=PulseConduit.opened(scope), limiter=CapacityLimiter(capacity), deadline=context.budget)
        with _thread_band(), Metrics.occupied(lambda: policy.limiter.borrowed_tokens, band="lane", scope=scope):
            async with anyio.create_task_group() as actors:
                actors.start_soon(policy.pulses.drain)
                try:
                    yield policy
                finally:
                    await policy.pulses.close()

    @property
    def scope(self) -> ScopeKey:
        # `scope` reads the lane's composition identity, held once on the conduit the scoped constructor opened.
        # Metric observation and receipt emission in the drained aspect both land under it, so an embedded
        # composition's lane evidence stays partitioned instead of merging into the default sink.
        return self.pulses.scope

    async def drain[T](self, units: Block[Admit[T]], cache: Map[ContentKey, T] = Map.empty()) -> DrainReceipt[T]:
        limiter = self.limiter
        opened = Cost.own()  # drain-window envelope: two own-process reads bracket the whole drain, never a sampling loop
        send, receive = anyio.create_memory_object_stream[tuple[Option[ContentKey], RuntimeRail[T]]](max_buffer_size=len(units) or 1)
        probed = units.map(lambda unit: probe(ADMIT_TABLE[unit.tag], unit, cache))
        hits, live = probed.partition(lambda p: p[1].is_some())
        replayed = hits.choose(lambda p: p[0].map2(p[1], lambda key, value: (key, value)))

        async def lane(key: Option[ContentKey], fn: Work[T], sink: MemoryObjectSendStream[tuple[Option[ContentKey], RuntimeRail[T]]]) -> None:
            async with sink, limiter:
                await sink.send((key, await fn()))

        with move_on_after(self.deadline.default_value(float("inf"))):
            async with anyio.create_task_group() as group, send:
                for key, _, fn in live:
                    group.start_soon(lane, key, fn, send.clone())
        # every eagerly-minted clone is adopted by exactly one child whose `async with sink` closes it even on the cancellation unwind,
        # so the post-scope drain reaches `EndOfStream` — the deadline-trip case sends the buffered partial, not a hang.
        resolved = Block.of_seq([item async for item in receive])
        return DrainReceipt.of(len(units), len(replayed), resolved, replayed, cache, cost=Cost.spent(Cost.own(), opened))

    async def driven[T, A](
        self,
        fronts: Fronts[T],
        seed: A,
        fold: Callable[[A, DrainReceipt[T]], RuntimeRail[A]],
        *,
        gate: Option[HookId] = Nothing,
        cache: Map[ContentKey, T] = Map.empty(),
    ) -> RuntimeRail[A]:
        # THE dependent-front drive for the whole branch: each front drains under THIS lane with the prior front's
        # `DrainReceipt.cache` threaded forward, and every axis a consumer varies is a parameter rather than a
        # re-spelled loop — where fronts come from (`Fronts`), what accumulates beside the cache (`fold`), and
        # whether a composition gets a say between waves (`gate`). Three refusals are the drive's OWN and settle
        # per front through `_settled`, so no caller re-derives them: a front's fault block, the lane deadline, and
        # the registered gate. One sequential `for` is the async-front exemption — each front reads the preceding
        # drain's cache, and neither `anyio` nor `expression` exposes a dependent async fold.
        # the FIRST front probes the seed, never an empty cache: a warm caller's prior run already keyed work this
        # drive would otherwise recompute, and only the entry front can elide it.
        carried: Map[ContentKey, T] = cache
        held: RuntimeRail[A] = Ok(seed)
        closed, total = 0, fronts.declared
        for label, units in fronts.walked():  # Exemption: dependent fronts — the next front is a function of the prior drain's cache
            match held:
                case Result(tag="error"):
                    return held
                case Result(tag="ok", ok=live):
                    receipt = await self.drain(units, carried)
                    closed, carried = closed + len(units), receipt.cache
                    held = self._settled(label, receipt, gate, StageMark(stage=label, done=closed, total=total)).bind(
                        lambda settled: fold(live, settled)
                    )
                case _ as unreachable:
                    assert_never(unreachable)
        return held

    def _settled[T](self, label: str, receipt: DrainReceipt[T], gate: Option[HookId], mark: StageMark) -> RuntimeRail[DrainReceipt[T]]:
        # the three refusals a front can answer, in the order their evidence settles. The fault block reads FIRST
        # because a front that refused is a dependency the NEXT front reads through the threaded cache — draining
        # past it computes on evidence that never landed — and it reduces through `BoundaryFault.combine`, so the
        # caller receives the whole wave's refusal rather than its first member. `cancelled` counts admissions the
        # deadline scope KILLED rather than units that failed, so a tripped budget rails as the deadline it is over
        # the lane's own declared bound. The gate fires LAST and only where a composition registered one: a VETO
        # point between waves is the one arm a subscriber decides, and an absent gate costs a single branch.
        if not receipt.faults.is_empty():
            return Error(receipt.faults.reduce(BoundaryFault.combine))
        if receipt.cancelled:
            return Error(expired(LANES_FRONT, self.deadline, f"front-cancelled:{label}"))
        return gate.map(lambda point: Hooks.fire(point, mark, scope=self.scope)).default_value(Ok(mark)).map(lambda _passed: receipt)

    async def offload[T](self, work: "Kernel[T] | Callable[..., T]", *args: object) -> RuntimeRail[T]:
        kernel = work if isinstance(work, Kernel) else Kernel.of(work)
        if (gated := admitted(kernel)).is_some():
            # host-side crossing admission precedes the span export and every arm, so a guest module the host refuses
            # never reaches a worker, a store, or a band token; every other shipping form answers Nothing at one branch.
            return Error(gated.value)
        budget = _tighter(self.deadline, kernel.deadline)
        match boundary(LANES_EXPORT, lambda: exported(kernel.wire, args), catch=_EXPORT_RAISES):
            # span allocation is fenced: an export raise (ENOSPC, an unmappable dtype) rails instead of escaping the offload.
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(crossed, blocks)):
                try:
                    return await self._crossed(kernel, budget, crossed)
                finally:
                    released(blocks)
            case _ as unreachable:
                assert_never(unreachable)

    async def _crossed[T](self, kernel: "Kernel[T]", budget: Option[float], crossed: tuple[object, ...]) -> RuntimeRail[T]:
        match (kernel.enforcement, kernel.row.kind):
            case (Enforcement.TERMINAL, _):
                # terminal enforcement forces the pebble arm regardless of trait — process isolation is the one kill-capable
                # substrate — and the tightened budget rides schedule(timeout=), so no outer scope doubles the bound; the kill's
                # TimeoutError classifies with the budget-unknown 0.0 floor, so the re-stamp restores the real tightened bound.
                pool = WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.TERMINAL)
                return (await pool.submit(replace(kernel, deadline=budget), *crossed)).map_error(
                    lambda fault: expired(LANES_OFFLOAD, budget, f"terminal-kill:{kernel.name}") if fault.tag == "deadline" else fault
                )
            case (_, Option(tag="none")):
                # INLINE trait: a sub-quantum body runs on the loop with no crossing and no band.
                return boundary(LANES_INLINE, lambda: shipped(kernel, *crossed), catch=_GUEST_RAISES)
            case (_, Option(tag="some", some=WorkerKind.PROCESS)):
                # cooperative process crossing rides the warm loky pool — carrier stitch, WORKER_BAND, and the in-band
                # worker-death retry live in submit; a tripped budget abandons the settle and the fault carries the real budget.
                pool = WorkerPool.acquire(WorkerKind.PROCESS)
                with move_on_after(budget.default_value(float("inf"))):
                    return await pool.submit(kernel, *crossed)
                return Error(expired(LANES_OFFLOAD, budget, f"cooperative-abandon:{kernel.name}"))
            case (_, Option(tag="some", some=kind)) if (row := _ISOLATION.try_find(kind)).is_some():
                carrier: dict[str, str] = {}
                propagate.inject(carrier)
                arm, band = row.value
                limiter = band.default_value(self.limiter)

                async def run() -> T:
                    return await arm(traced_kernel, carrier, kernel, *crossed, limiter=limiter)

                # trait-supplied `guard(cls)` leg retries a transient worker cold-start crash BEFORE `async_boundary`
                # converts the terminal raise; a `Nothing` retry runs bare, and a tripped budget rails with the real bound.
                with move_on_after(budget.default_value(float("inf"))):
                    return await async_boundary(
                        LANES_OFFLOAD, lambda: kernel.retry.map(lambda cls: guard(cls)(run)).default_with(run), catch=_GUEST_RAISES
                    )
                return Error(expired(LANES_OFFLOAD, budget, f"anyio-arm-cancel:{kernel.name}"))
            case (_, Option(tag="some", some=kind)):
                # loud witness for a kind without a crossing arm: a new WorkerKind lands as one _ISOLATION row or one
                # pool arm, and until it does every offload of it rails this typed refusal instead of a KeyError.
                return Error(LANES_ISOLATION.raised(kernel.name, kind.value))
            case _ as unreachable:
                assert_never(unreachable)


class StagePlan(Struct, frozen=True):
    lane: LanePolicy
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]

    async def execute[T](
        self,
        work: Callable[[str, RetryClass], Sequence[Admit[T]]],
        *,
        gate: Option[HookId] = Nothing,
        cache: Map[ContentKey, T] = Map.empty(),
    ) -> RuntimeRail[Block[DrainReceipt[T]]]:
        # a `graph` admission and a total fold: the plan carries the dependency edges and the drive resolves the
        # waves, so this entry is one `Fronts` construction rather than a driver of its own. The rail is the RETURN
        # now, because the drive refuses a faulted, cancelled, or vetoed front — collapsing that to a bare tuple
        # would launder exactly the refusal the three arms exist to surface. Both drive keywords pass THROUGH rather
        # than defaulting here, so the graph entry warm-starts and gates exactly as a resolved-ladder caller does.
        return await self.lane.driven(
            Fronts(graph=(self.stages, self.edges, work)),
            Block.empty(),
            lambda held, receipt: Ok(held.append(Block.singleton(receipt))),
            gate=gate,
            cache=cache,
        )


class PulseConduit(Struct, frozen=True):
    # one conduit per lane: the spawn-context manager proxy pickles into every crossing arm as an ordinary kernel
    # argument — THREAD, INTERPRETER, and both process arms share one worker-side spelling, so the spine carries no
    # per-arm conduit and no offload signature changes; None is the close signal because broker round trips
    # preserve its identity where a module sentinel would not. `scope` binds the owning composition parent-side, so
    # each pulse fires on the ScopeKey its points registered under — a worker never carries scope.
    tap: Queue[PulseFact | None]
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    def opened(cls, scope: ScopeKey = DEFAULT_SCOPE) -> Self:
        return cls(tap=_pulse_manager().Queue(maxsize=PULSE_BUFFER), scope=scope)

    async def close(self) -> None:
        # composition-side retire is shielded and BOUNDED: producers have stopped under the scoped-owner contract, so a
        # live pump admits the control token inside the grace window while data is never evicted for it; grace expiry
        # proves the pump dead — its full conduit never drains — so the terminal arm evicts one pulse as the authorized
        # counted drop and lands the token non-blocking. Teardown always returns; no shielded await parks forever.
        def retired() -> None:
            try:
                self.tap.put(None, timeout=CLOSE_GRACE_S)
            except Full:
                try:
                    self.tap.get_nowait()
                    Metrics.record({"rasm.runtime.pulse.dropped": 1.0}, domain="runtime", kind="close", scope=self.scope)
                    self.tap.put_nowait(None)
                except (Empty, Full):
                    pass

        try:
            with anyio.CancelScope(shield=True):
                await anyio.to_thread.run_sync(retired, abandon_on_cancel=False)
        except (OSError, EOFError):
            pass

    async def drain(self) -> None:
        # parent-side serialized pulse actor: ONE consumer folds every pulse into Hooks.fire, so hook taps observe
        # pulses in conduit order and no worker kernel reaches the registry or a live span; the anyio single-consumer
        # drain is the ruled stand-in for the asyncio-bound expression MailboxProcessor the serialized-agent law rejects.
        send, receive = anyio.create_memory_object_stream[PulseFact](max_buffer_size=PULSE_BUFFER)

        def pumped() -> None:
            while True:  # Exemption: blocking manager-relay kernel — the platform-forced pump seam between the process conduit and the loop.
                match self.tap.get():
                    case None:
                        return
                    case fact:
                        try:
                            anyio.from_thread.run_sync(send.send_nowait, fact)
                        except WouldBlock:  # authorized lossy drop: telemetry never back-pressures the conduit
                            Metrics.record({"rasm.runtime.pulse.dropped": 1.0}, domain="runtime", kind=fact[0].value, scope=self.scope)
                        except BrokenResourceError:  # drain consumer gone: the pump retires itself
                            return

        async with anyio.create_task_group() as group:
            group.start_soon(_pulse_fold, receive, self.scope)
            await on_thread(pumped)  # LanePolicy.close releases the broker read before the composing task group exits
            send.close()  # loop-side close ends the consumer's fold once the pump has returned on the close token


# --- [OPERATIONS] -----------------------------------------------------------------------


def probe[T](row: AdmitRow[T], unit: Admit[T], cache: Map[ContentKey, T]) -> tuple[Option[ContentKey], Option[T], Work[T]]:
    key = row.key(unit)
    return key, key.bind(cache.try_find), row.make(unit)


def _tighter(lane: Option[float], unit: Option[float]) -> Option[float]:
    # deadline fold: whichever of the lane budget and the per-offload budget is sooner bounds the hop; one absent side defers.
    return lane.map(lambda held: unit.map(lambda own: min(held, own)).default_value(held)).or_else(unit)


def pulsed(tap: Queue[PulseFact | None], point_id: HookId, payload: Struct) -> None:
    # worker-side pulse write — a kernel's WHOLE reach into observability: lossy by design, a full conduit or dead
    # broker drops the pulse, so telemetry never back-pressures or faults a kernel mid-operation; the payload struct
    # is the folder-owned HookPoint vocabulary, pickled whole across the proxy. `point_id` is a ROSTER member and never
    # a literal — the manager proxy pickles an enum member by identity, so the id arrives worker-side as its member.
    try:
        tap.put_nowait((point_id, payload))
    except (Full, OSError, EOFError):  # Exemption: fire-and-forget conduit — every refusal is the authorized drop
        pass


async def _pulse_fold(receive: MemoryObjectReceiveStream[PulseFact], scope: ScopeKey) -> None:
    # Serialized consumer relies on Hooks.fire's boundary fence to isolate a raising tap, and an unregistered point id
    # rails there — counted here as producer drift, never a silent drop and never a drain fault. Each fire carries the
    # conduit's composition scope, so a non-default composition's registered points receive their own beats.
    async for point_id, payload in receive:
        if Hooks.fire(point_id, payload, scope=scope).is_error():
            Metrics.record({"rasm.runtime.pulse.rejected": 1.0}, domain="runtime", kind=point_id.value, scope=scope)


async def on_thread[T](fn: Callable[..., T], *args: object, abandon: bool = False, **kwargs: object) -> T:
    # band-bound raw thread hop: a resilience-enveloped blocking leg outside a lane rides this arm, so THREAD_BAND
    # bounds every plain thread crossing in the branch — `guarded(cls, on_thread, fn, ...)` is the composed spelling.
    # `abandon=True` frees the band slot when an enclosing deadline trips a side-effect-free read; the abandoned
    # thread runs to completion unobserved, so a wedged network read never parks a slot past its scope.
    return await anyio.to_thread.run_sync(lambda: fn(*args, **kwargs), abandon_on_cancel=abandon, limiter=THREAD_BAND)


def _fire_seam(scheduler: AsyncIOScheduler, send: MemoryObjectSendStream[JobExecutionEvent]) -> Callable[[JobExecutionEvent], None]:
    def on_fire(event: JobExecutionEvent) -> None:
        # two distinct dispositions, never one collapsed arm: WouldBlock is the authorized missed-fire drop (the scheduler's own
        # coalesce policy), BrokenResourceError means the feed consumer is gone and the listener retires itself.
        try:
            send.send_nowait(event)
        except WouldBlock:
            pass
        except BrokenResourceError:
            scheduler.remove_listener(on_fire)

    return on_fire


def drained[**P, T](
    owner: str, redaction: Redaction, *, scope: ScopeKey = DEFAULT_SCOPE
) -> Callable[[Callable[P, Awaitable[DrainReceipt[T]]]], Callable[P, Awaitable[DrainReceipt[T]]]]:
    # Both egress legs carry the lane's composition scope: drain counts land on the scope-stamped counter and each
    # drained line resolves that composition's own bound logger, so one embedded lane's evidence never reads as
    # another's. `feed` binds it off `policy.scope`, so a caller never re-supplies an identity the lane already holds.
    # Reporting stops at what FINISHED — `cancelled` is already a column on the drain counter, and occupancy is the
    # standing `Metrics.occupied` probe the scoped constructor registered, sampled on the export cycle.
    # Emission rides the async mirror because this aspect wraps a coroutine on the running loop: the sync sink renders
    # and writes inline, so a fast feed stalls its own next drain behind every line it just produced.
    def aspect(fn: Callable[P, Awaitable[DrainReceipt[T]]]) -> Callable[P, Awaitable[DrainReceipt[T]]]:
        @wraps(fn)
        async def observed(*args: P.args, **kwargs: P.kwargs) -> DrainReceipt[T]:
            receipt = await fn(*args, **kwargs)
            Metrics.observe(receipt, scope=scope)
            await Signals.emit_async(Receipt.of(owner, receipt), redaction, scope=scope)
            return receipt

        return observed

    return aspect


async def _events[T](source: LaneSource[T]) -> AsyncIterator[Block[Admit[T]]]:
    match source:
        case LaneSource(tag="watched", watched=(watch, build)):
            narrowed = Option.of_optional(watch.filter).default_value(PythonFilter())
            async for batch in awatch(*watch.paths, watch_filter=narrowed, debounce=watch.debounce, step=watch.step):
                yield build(batch)
        case LaneSource(tag="scheduled", scheduled=(trigger, build)):
            scheduler, (send, receive) = AsyncIOScheduler(), anyio.create_memory_object_stream[JobExecutionEvent](max_buffer_size=FIRE_BUFFER)
            scheduler.add_listener(_fire_seam(scheduler, send), FIRE_MASK)
            scheduler.add_job(lambda: None, trigger=trigger)
            scheduler.start()
            try:
                async for event in receive:
                    yield build(event)
            finally:
                scheduler.shutdown(wait=False)
        case _ as unreachable:
            assert_never(unreachable)


async def feed[T](policy: LanePolicy, source: LaneSource[T], owner: str, redaction: Redaction) -> AsyncIterator[DrainReceipt[T]]:
    observed = drained(owner, redaction, scope=policy.scope)(policy.drain)
    async for batch in _events(source):
        yield await observed(batch)


# --- [COMPOSITION] ----------------------------------------------------------------------

ADMIT_TABLE: Final[Map[AdmitTag, AdmitRow[object]]] = Map.of_seq([
    ("bare", AdmitRow(key=lambda _: Nothing, make=lambda unit: unit.bare)),
    ("keyed", AdmitRow(key=lambda unit: Some(unit.keyed[0]), make=lambda unit: unit.keyed[1])),
    ("retried", AdmitRow(key=lambda _: Nothing, make=lambda unit: lambda: guard(unit.retried[0])(unit.retried[1]))),
])

# anyio isolation arms as data: one row binds each anyio-substrate `WorkerKind` to its arm and band — `Nothing` selects the
# per-lane memoised limiter, the `Some` row the thread band. PROCESS rides the workers pool capsule, DAEMON is spawned and
# supervised, never called, and REMOTE and GPU are fleet/device placement acquired on the pool arms, never trait-derived,
# so none carries a row here; WASM rides the thread band because the guest arm's own epoch deadline is its in-process kill.
# A deadline-free guest runs on `UNBOUNDED_TICKS` and parks its THREAD_BAND token for as long as it runs, and the band's own
# level series is the ONLY reading that exposes it — no arm, receipt, or fault reports a crossing that never terminates.
_ISOLATION: Final[Map[WorkerKind, tuple[IsolationArm, Option[CapacityLimiter]]]] = Map.of_seq([
    (WorkerKind.INTERPRETER, (anyio.to_interpreter.run_sync, Nothing)),
    (WorkerKind.THREAD, (anyio.to_thread.run_sync, Some(THREAD_BAND))),
    (WorkerKind.WASM, (anyio.to_thread.run_sync, Some(THREAD_BAND))),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
