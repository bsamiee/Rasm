# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration: `LanePolicy.drain` is the one polymorphic bounded drain over the `Admit[T]` admission union, `LanePolicy.whole` the direct whole-capacity grant, `LanePolicy.offload` the one kernel-isolation hop over the `execution/workers#CROSSING` `Kernel` crossing, `StagePlan.execute` the concurrent-front stage DAG over that same drain, and `LaneSource` the one `scheduled`/`watched` feeder union — one budget, one drain shape, one owner for every bounded lane the branch runs.

`drain`, `whole`, and `offload` share one deadline budget and one isolation axis, so the deadline contract is total across every hop, and a caller supplies its kernel — the lane never imports one. `LanePolicy.of` projects capacity and deadline from the admitted `execution/admission#CONTEXT` profile row and scopes one pulse actor across the lane lifetime, so a consumer mints neither bounds nor actor custody. `LaneCapacity` is the one allocator: shared admissions claim one slot, the `whole` admission and direct `LanePolicy.whole` claim every slot and receive a `LaneGrant.width`, so a native kernel retains full single-model parallelism without multiplying an outer lane width by an inner thread count. Offload isolation, worker-death retry, wire, shipping, and per-offload deadline all arrive on the `Kernel` value — the workers owner answers the isolation question once, this page owns the thread and subinterpreter crossing arms and routes both process arms onto the `execution/workers#POOL` capsule. Cross-cutting concerns ride aspects, never inline call sites: the OTel trace context stitches across every crossing, content-keyed work short-circuits on a cache hit that threads forward across `StagePlan` fronts, `reliability/resilience#RESILIENCE` `guard` rides each unit's `RetryClass`, and every drain records its counter and writes its line at the drain site under the lane's own composition scope. Occupancy is the one lane fact no aspect carries: a drain reports what finished, so the live borrowed-slot level registers once at the scoped constructor and the metric export cycle samples it — the per-lane allocator under the `lane` band and the process-wide `THREAD_BAND`/`INTERPRETER_BAND` pair under one refcounted custody bracket. Mid-operation kernel facts ride one `PulseConduit` per lane into a parent-side serialized drain folding `Hooks.fire` under the conduit's own composition `ScopeKey`, so hook taps observe long-running kernels without polling and a non-default composition's registered points receive their own beats. Bare `asyncio` is never imported — `anyio` owns every concurrency primitive, and cron is solely `apscheduler`.

## [01]-[INDEX]

- [02]-[LANE]: bounded `drain` over the `Admit[T]` union, the `Kernel`-crossing `offload`, the concurrent-front `StagePlan`, the `LaneSource` feeder union, and the `PulseConduit` mid-operation fact spine.

## [02]-[LANE]

- Owner: `Admit[T]` discriminates a plain coroutine, a content-keyed cache unit, a resilience-guarded unit, and a whole-capacity unit by case, so one `drain` serves every admission geometry rather than parallel methods, and `ADMIT_TABLE` folds each case through one behavior row. `LaneWork` is the private shared-versus-whole execution fold the table returns; `LaneGrant` is the public width fact only whole work receives. `Drained[T]`/`DrainOutcome` and the `DRAIN_COLUMNS`/`DRAIN_DISPOSITIONS` pair derived from it are the one canonical drain taxonomy IMPORTED from their `observability/observe#OBSERVE` owner: the vocabulary lives one tier down, so this page, the `observability/metrics#METRIC` counter, and the drained line all read that one owner. `LaneSource` is a feeder union over the one drain, never a separate scheduler surface.
- Law: mid-operation kernel facts cross `LanePolicy.pulses`, one `PulseConduit` per lane — a spawn-context manager-queue proxy every arm pickles as an ordinary kernel argument, written through `pulsed` alone, lossy by design: a full conduit or dead broker drops the pulse, so telemetry never back-pressures or faults a kernel. Every authorized drop counts onto its own `rasm.runtime.pulse.*` census row under the conduit's own composition scope — the same identity the drained line stamps — so the lossy paths stay measured rather than silent, two embedded conduits' drops never merge into one unlabeled series, and the drain actor cannot fault on its own accounting. One parent-side lane actor serializes the fold — a `THREAD_BAND` pump relays the proxy through `anyio.from_thread.run_sync` onto a bounded anyio stream (`send_nowait` `WouldBlock` is the authorized drop) and ONE consumer posts each fact to `Hooks.fire`, so taps observe pulses in conduit order and no worker reaches the hook registry or a live span. Conduit-member verdict: the `pebble` map-iterator streams per-chunk terminals, never mid-operation facts, and `pebble` ships no pipe member; the `expression` `MailboxProcessor` inbox reaches `asyncio` against the anyio law — the manager proxy with the anyio single-consumer drain is the ruled conduit. Folder pulse vocabularies stay folder-owned `HookPoint` rows; the spine carries only the crossing and the drain, and the scoped constructor retires the actor with one shielded, grace-bounded close token after producers stop — a live pump admits the token inside the grace window with no data evicted, a dead pump's full conduit unblocks by evicting one pulse as the authorized counted drop — so teardown always returns and no concurrent drain consumes another drain's terminus.
- Entry: `LanePolicy.of(context)` is the one scoped constructor — capacity reads the profile row's `lane_capacity`, deadline the context budget, `pulses` opens one lane-lifetime conduit actor, and `Metrics.occupied` registers the allocator's borrowed-slot read for that same lifetime — so bounds, actor custody, and occupancy visibility trace to one admitted owner. `LaneCapacity.claim` serializes acquisition, not work: a whole claimant waits for existing shared holders while the gate prevents new ones cutting in, then the one AnyIO `CapacityLimiter` carries the full lease; no second limiter, consumer lock, or copied width exists. The same bracket refcounts the process-wide `THREAD_BAND` and `INTERPRETER_BAND` registrations: each crossing rides its own named runtime band and never re-acquires the lane allocator a surrounding drain already holds. Concurrent `drain` and `whole` calls share the allocator and conduit; only the constructor's exit closes the actor after all composing work has stopped. `offload` accepts a `Kernel` or a bare callable it lifts through `Kernel.of`, passes the workers-owned `admitted` gate before the span export so a refused guest module never reaches a worker or a band token; the kernel's trait row supplies isolation kind and worker-death retry, its `deadline` tightens the lane budget to whichever bound is sooner, its `wire` selects the shared-memory span export around the whole hop, and its `TERMINAL` enforcement routes the hop through the workers pebble arm regardless of trait — process isolation is the kill-capable substrate for native code, so a hung native kernel dies at wall-clock instead of outliving a cooperative cancel, while a `SANDBOXED` kernel already dies in-process at its guest epoch deadline and rides the thread arm with no pebble re-route. Cooperative `HOSTILE` kernels ride the warm loky pool, whose `submit` owns the carrier stitch, the `WORKER_BAND` bound, and the in-band worker-death retry. Each `StagePlan` stage picks its own admission case — a cacheable stage mints `keyed` units, a transient-prone stage `retried`, a plain stage `bare`, and a native stage `whole` — and each front's `Drained.cache` threads forward, so a keyed whole unit re-admitted downstream replays the upstream `Ok` rather than recomputing.
- Auto: a tripped deadline is contained — the drain runs inside `move_on_after`, never a bare `fail_after` whose `TimeoutError` escapes as a raw `BaseExceptionGroup` — so a deadline trip cancels in-flight units and `Drained` reports them as `cancelled` with the partial `values`/`faults` intact; no exception escapes a bounded lane without a `Drained`. Each unit sends its full `RuntimeResult[T]` over the stream rather than a pre-collapsed bool, so the typed fault survives into `Drained` and the drain is lossless in both directions. Every drain records its `rasm.lane.drained` partition through `Metrics.observe` and writes one `lane.drained` line off `Drained.facts` at the drain site, so a front, a fed batch, and a direct caller all leave the same evidence with no aspect a caller can omit. Every schedule geometry the package ships is one `Trigger` member on the one `AsyncIOScheduler` — no `croniter`, no `aiocron`, no hand-rolled sleep cron.
- Auto: session cache state is an immutable `Map[ContentKey, T]` threaded on `Drained.cache` — a hit reproduces the exact `Ok` value and counts as `hit` distinct from `completed`, only an `Ok` folds back, and an `Error` re-runs while its fault accumulates. This cache is session-local in-memory, never a durable store: durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire. The observe-owned `Cost` brackets the whole drain window — two own-process reads landing the drain's spend envelope on `Drained.cost`, kernel-grain attribution staying the worker gate's own bracket.
- Auto: trace stitching parents per the arm's module-state reality — every crossing carries the injected carrier, the anyio arms through `traced_kernel` directly and the pooled arms through `WorkerPool.submit`'s own injection. `THREAD` shares the interpreter, so the installed composite propagator is present and the kernel parents unconditionally; the process and subinterpreter workers hold independent module state, so a worker that has not run the `observability/telemetry#TELEMETRY` install resolves the default text-map and runs unparented while the carrier still holds the parent for any span the kernel opens. Pickled arms pay the IPC hop the `THREAD` arm skips — the arm keys off the kernel's declared trait, never a pickle-versus-no-pickle guess, and a closure crosses the pickled arms by value because `Kernel.of` ships it. A worker death retries under the kernel's trait-supplied retry row or crosses the one `async_boundary` conversion, never a local catch.
- Law: every refusal resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.LANES`, and the three deadline re-stamps compose the owner's `expired` fold rather than each spelling the budget-unknown floor: `Option[float]` is the budget carrier and `BUDGET_UNKNOWN` is DECLARED once at the fault owner, so a lane that held no bound and one that measured one can never read alike. The kernel name rides the tripped-axis `cause`, since a per-kernel subject would spell a raise coordinate the roster never declares.
- Law: `driven` is the ONE dependent-front drive in the branch and every axis a consumer varies is a parameter — the front SOURCE (`Fronts`, a closed two-case admission over a dependency graph or an already-resolved labelled ladder), the carried FOLD, the optional per-front `gate`, and the warm elision `cache` the FIRST front probes, since only the entry front can elide work a prior run already keyed and every later front reads its predecessor's `Drained`. Its three refusals are the DRIVE's own and settle per front: a front's fault block reduces through `BoundaryFault.combine` and stops the walk, since a refused front is a dependency the next front reads through the threaded cache; a cancelled front returns the lane's declared deadline, `cancelled` counting admissions the scope killed rather than units that failed; and a registered VETO point decides between waves. A caller re-spelling any of the four drifts out of the other three, which is what one owner forecloses.
- Law: the front source is ONE input discriminant and never a sibling method — a graph admission resolves its own topological waves, a resolved admission walks the labelled fronts it was handed, and `declared` answers the unit total only where the source honestly holds one, so a progress reader never takes a graph's unknown count for a whole job.
- Law: mid-operation pulse payloads are the `observability/hooks#HOOKS` `StageMark` — position, units closed, and an `Option` total — so this conduit carries ONE mark shape for every producing lane and a folder closes its own `<Lane>Stage(StrEnum)` at the site rather than minting a mark of its own.
- Growth: a new lane source is one `LaneSource` case with one `_events` arm; a new mid-operation fact is one folder-owned `HookPoint` row written through `pulsed`, never a drain or conduit edit; a new admission modality one `Admit` case with one `ADMIT_TABLE` row; a new anyio-substrate isolation kind one workers-owned `WorkerKind` member with one `_ISOLATION` row, a new pooled kind one workers arm — every offload call site untouched either way, and a kind whose row has not landed returns a typed `config` refusal at the offload, never a lookup raise; a new trigger one `Trigger` member; a new stage one `StagePlan` edge; a new watch tuning one `Watch` field; a new drain outcome dimension one member on the observe-owned `DrainOutcome` with its field, reaching the drained line through the imported `DRAIN_COLUMNS` and the metrics counter through that owner's `DRAIN_DISPOSITIONS` carve.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` the `scheduled` case mints, no second cron owner, no app lifecycle hook, no background loop without a `Drained`, and no unbounded task creation. A scheduling model may read `LanePolicy.slots.total` as the outer concurrency geometry it estimates; an executing native kernel never does — whole work receives `LaneGrant.width`, shared work receives no width, and a literal thread count or folder-local limiter is the rejected capacity twin. A blocking leg outside a lane rides `on_thread`, so every plain thread hop in the branch is `THREAD_BAND`-bounded by construction, and the pooled settle hop rides the workers-owned `WORKER_BAND`. Consumer contract on `Drained` is column-driven: the line reads every count off `DRAIN_COLUMNS` and the metrics counter off the `DRAIN_DISPOSITIONS` carve, per column and never a full-struct `asdict` allocating the containers per export cycle.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import threading
from collections.abc import AsyncIterator, Awaitable, Callable, Iterator, Sequence
from contextlib import ExitStack, asynccontextmanager, contextmanager
from functools import cache
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
from anyio import BrokenResourceError, CapacityLimiter, Lock, WouldBlock, move_on_after
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
    RuntimeResult,
    async_boundary,
    boundary,
    expired,
)
from rasm.runtime.hooks import HookId, Hooks, StageMark
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.observe import DEFAULT_SCOPE, Cost, Drained, ScopeKey, logger
from rasm.runtime.resilience import RetryClass, guard
from rasm.runtime.workers import Enforcement, Kernel, WorkerKind, WorkerPool, admitted, exported, released, shipped, traced_kernel

# --- [TYPES] ----------------------------------------------------------------------------

class LaneGrant(Struct, frozen=True):
    width: int


type Work[T] = Callable[[], Awaitable[RuntimeResult[T]]]
type GrantedWork[T] = Callable[[LaneGrant], Awaitable[RuntimeResult[T]]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger | CalendarIntervalTrigger | AndTrigger | OrTrigger
type AdmitTag = Literal["bare", "keyed", "retried", "whole"]
type IsolationArm = Callable[..., Awaitable[object]]
type PulseFact = tuple[HookId, Struct]
type Front[T] = tuple[str, Block[Admit[T]]]


@tagged_union(frozen=True)
class Admit[T]:
    tag: AdmitTag = tag()
    bare: Work[T] = case()
    keyed: tuple[ContentKey, Work[T]] = case()
    retried: tuple[RetryClass, Work[T]] = case()
    whole: tuple[Option[ContentKey], GrantedWork[T]] = case()


@tagged_union(frozen=True)
class LaneWork[T]:
    tag: Literal["shared", "whole"] = tag()
    shared: Work[T] = case()
    whole: GrantedWork[T] = case()


@tagged_union(frozen=True)
class Fronts[T]:
    tag: Literal["graph", "resolved"] = tag()
    graph: tuple[tuple[tuple[str, RetryClass], ...], tuple[tuple[str, str], ...], Callable[[str, RetryClass], Sequence[Admit[T]]]] = case()
    resolved: Block[Front[T]] = case()

    @property
    def declared(self) -> Option[int]:
        match self:
            case Fronts(tag="resolved", resolved=fronts):
                return Some(sum(len(units) for _label, units in fronts))
            case Fronts(tag="graph"):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def walked(self) -> Iterator[Front[T]]:
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
PULSE_BUFFER: Final[int] = 256
CLOSE_GRACE_S: Final[float] = 2.0

THREAD_BAND: Final[CapacityLimiter] = CapacityLimiter(2 * (process_cpu_count() or 4))
INTERPRETER_BAND: Final[CapacityLimiter] = CapacityLimiter(process_cpu_count() or 4)

_EXPORT_RAISES: Final[Catch] = (OSError, BufferError, TypeError, ValueError)
_GUEST_RAISES: Final[Catch] = Exception

# --- [MODELS] ---------------------------------------------------------------------------


class AdmitRow[T](Struct, frozen=True):
    key: Callable[[Admit[T]], Option[ContentKey]]
    make: Callable[[Admit[T]], LaneWork[T]]


class LaneCapacity(Struct, frozen=True):
    total: int
    limiter: CapacityLimiter
    gate: Lock

    @classmethod
    def of(cls, total: int) -> Self:
        return cls(total=total, limiter=CapacityLimiter(total), gate=Lock())

    @property
    def borrowed(self) -> int:
        return self.limiter.borrowed_tokens

    @asynccontextmanager
    async def claim(self, width: int) -> AsyncIterator[LaneGrant]:
        borrowers = tuple(object() for _ in range(width))
        held: list[object] = []
        try:
            async with self.gate:
                for borrower in borrowers:
                    await self.limiter.acquire_on_behalf_of(borrower)
                    held.append(borrower)
            yield LaneGrant(width=width)
        finally:
            for borrower in reversed(held):
                self.limiter.release_on_behalf_of(borrower)


class Watch(Struct, frozen=True):
    paths: tuple[str | PathLike[str], ...]
    filter: BaseFilter | None = None
    debounce: int = 1600
    step: int = 50

    @staticmethod
    def facts(batch: set[tuple[Change, str]]) -> Block[tuple[str, str]]:
        return Block.of_seq(sorted((change.raw_str(), path) for change, path in batch))


# --- [SERVICES] -------------------------------------------------------------------------


@cache
def _pulse_manager() -> SyncManager:
    return get_context("spawn").Manager()


# --- [CROSSING_BAND_CUSTODY]

_BAND_GATE: Final[threading.Lock] = threading.Lock()
_BAND_PROBES: Final[ExitStack] = ExitStack()
_BAND_HOLDERS: Final[list[object]] = []


def _thread_occupancy() -> int:
    return THREAD_BAND.borrowed_tokens


def _interpreter_occupancy() -> int:
    return INTERPRETER_BAND.borrowed_tokens


@contextmanager
def _crossing_bands() -> Iterator[None]:
    token = object()
    with _BAND_GATE:
        _BAND_HOLDERS.append(token)
        if len(_BAND_HOLDERS) == 1:
            _BAND_PROBES.enter_context(Metrics.occupied(_thread_occupancy, band="thread"))
            _BAND_PROBES.enter_context(Metrics.occupied(_interpreter_occupancy, band="interpreter"))
    try:
        yield
    finally:
        with _BAND_GATE:
            _BAND_HOLDERS.remove(token)
            if not _BAND_HOLDERS:
                _BAND_PROBES.close()


class LanePolicy(Struct, frozen=True):
    pulses: "PulseConduit"
    slots: LaneCapacity
    deadline: Option[float] = Nothing

    @classmethod
    @asynccontextmanager
    async def of(cls, context: RuntimeContext, *, scope: ScopeKey = DEFAULT_SCOPE) -> AsyncIterator[Self]:
        capacity = context.policy.lane_capacity
        policy = cls(pulses=PulseConduit.opened(scope), slots=LaneCapacity.of(capacity), deadline=context.budget)
        with _crossing_bands(), Metrics.occupied(lambda: policy.slots.borrowed, band="lane", scope=scope):
            async with anyio.create_task_group() as actors:
                actors.start_soon(policy.pulses.drain)
                try:
                    yield policy
                finally:
                    await policy.pulses.close()

    @property
    def scope(self) -> ScopeKey:
        return self.pulses.scope

    async def drain[T](self, units: Block[Admit[T]], cache: Map[ContentKey, T] = Map.empty()) -> Drained[T]:
        opened = Cost.own()
        send, receive = anyio.create_memory_object_stream[tuple[Option[ContentKey], RuntimeResult[T]]](max_buffer_size=len(units) or 1)
        probed = units.map(lambda unit: probe(ADMIT_TABLE[unit.tag], unit, cache))
        hits, live = probed.partition(lambda p: p[1].is_some())
        replayed = hits.choose(lambda p: p[0].map2(p[1], lambda key, value: (key, value)))

        async def lane(
            key: Option[ContentKey], work: LaneWork[T], sink: MemoryObjectSendStream[tuple[Option[ContentKey], RuntimeResult[T]]]
        ) -> None:
            async with sink:
                match work:
                    case LaneWork(tag="shared", shared=fn):
                        async with self.slots.claim(1):
                            await sink.send((key, await fn()))
                    case LaneWork(tag="whole", whole=fn):
                        async with self.slots.claim(self.slots.total) as grant:
                            await sink.send((key, await fn(grant)))
                    case _ as unreachable:
                        assert_never(unreachable)

        with move_on_after(self.deadline.default_value(float("inf"))):
            async with anyio.create_task_group() as group, send:
                for key, _, fn in live:
                    group.start_soon(lane, key, fn, send.clone())
        resolved = Block.of_seq([item async for item in receive])
        drained = Drained.of(len(units), len(replayed), resolved, replayed, cache, cost=Cost.spent(Cost.own(), opened))
        Metrics.observe(drained, scope=self.scope)
        logger(self.scope).info("lane.drained", **drained.facts())
        return drained

    async def driven[T, A](
        self,
        fronts: Fronts[T],
        seed: A,
        fold: Callable[[A, Drained[T]], RuntimeResult[A]],
        *,
        gate: Option[HookId] = Nothing,
        cache: Map[ContentKey, T] = Map.empty(),
    ) -> RuntimeResult[A]:
        carried: Map[ContentKey, T] = cache
        held: RuntimeResult[A] = Ok(seed)
        closed, total = 0, fronts.declared
        for label, units in fronts.walked():
            match held:
                case Result(tag="error"):
                    return held
                case Result(tag="ok", ok=live):
                    drained = await self.drain(units, carried)
                    closed, carried = closed + len(units), drained.cache
                    held = self._settled(label, drained, gate, StageMark(stage=label, done=closed, total=total)).bind(
                        lambda settled: fold(live, settled)
                    )
                case _ as unreachable:
                    assert_never(unreachable)
        return held

    def _settled[T](self, label: str, drained: Drained[T], gate: Option[HookId], mark: StageMark) -> RuntimeResult[Drained[T]]:
        if not drained.faults.is_empty():
            return Error(drained.faults.reduce(BoundaryFault.combine))
        if drained.cancelled:
            return Error(expired(LANES_FRONT, self.deadline, f"front-cancelled:{label}"))
        return gate.map(lambda point: Hooks.fire(point, mark, scope=self.scope)).default_value(Ok(mark)).map(lambda _passed: drained)

    async def whole[T](self, work: GrantedWork[T]) -> RuntimeResult[T]:
        with move_on_after(self.deadline.default_value(float("inf"))):
            async with self.slots.claim(self.slots.total) as grant:
                return await work(grant)
        return Error(expired(LANES_OFFLOAD, self.deadline, "whole-lane-cancel"))

    async def offload[T](self, work: "Kernel[T] | Callable[..., T]", *args: object) -> RuntimeResult[T]:
        kernel = work if isinstance(work, Kernel) else Kernel.of(work)
        if (gated := admitted(kernel)).is_some():
            return Error(gated.value)
        budget = _tighter(self.deadline, kernel.deadline)
        match boundary(LANES_EXPORT, lambda: exported(kernel.wire, args), catch=_EXPORT_RAISES):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(crossed, blocks)):
                try:
                    return await self._crossed(kernel, budget, crossed)
                finally:
                    released(blocks)
            case _ as unreachable:
                assert_never(unreachable)

    async def _crossed[T](self, kernel: "Kernel[T]", budget: Option[float], crossed: tuple[object, ...]) -> RuntimeResult[T]:
        match (kernel.enforcement, kernel.row.kind):
            case (Enforcement.TERMINAL, _):
                pool = WorkerPool.acquire(WorkerKind.PROCESS, Enforcement.TERMINAL)
                return (await pool.submit(replace(kernel, deadline=budget), *crossed)).map_error(
                    lambda fault: expired(LANES_OFFLOAD, budget, f"terminal-kill:{kernel.name}") if fault.tag == "deadline" else fault
                )
            case (_, Option(tag="none")):
                return boundary(LANES_INLINE, lambda: shipped(kernel, *crossed), catch=_GUEST_RAISES)
            case (_, Option(tag="some", some=WorkerKind.PROCESS)):
                pool = WorkerPool.acquire(WorkerKind.PROCESS)
                with move_on_after(budget.default_value(float("inf"))):
                    return await pool.submit(kernel, *crossed)
                return Error(expired(LANES_OFFLOAD, budget, f"cooperative-abandon:{kernel.name}"))
            case (_, Option(tag="some", some=kind)) if (row := _ISOLATION.try_find(kind)).is_some():
                carrier: dict[str, str] = {}
                propagate.inject(carrier)
                arm, limiter = row.value

                async def run() -> T:
                    return await arm(traced_kernel, carrier, kernel, *crossed, limiter=limiter)

                with move_on_after(budget.default_value(float("inf"))):
                    return await async_boundary(
                        LANES_OFFLOAD, lambda: kernel.retry.map(lambda cls: guard(cls)(run)).default_with(run), catch=_GUEST_RAISES
                    )
                return Error(expired(LANES_OFFLOAD, budget, f"anyio-arm-cancel:{kernel.name}"))
            case (_, Option(tag="some", some=kind)):
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
    ) -> RuntimeResult[Block[Drained[T]]]:
        return await self.lane.driven(
            Fronts(graph=(self.stages, self.edges, work)),
            Block.empty(),
            lambda held, drained: Ok(held.append(Block.singleton(drained))),
            gate=gate,
            cache=cache,
        )


class PulseConduit(Struct, frozen=True):
    tap: Queue[PulseFact | None]
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    def opened(cls, scope: ScopeKey = DEFAULT_SCOPE) -> Self:
        return cls(tap=_pulse_manager().Queue(maxsize=PULSE_BUFFER), scope=scope)

    async def close(self) -> None:
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
        send, receive = anyio.create_memory_object_stream[PulseFact](max_buffer_size=PULSE_BUFFER)

        def pumped() -> None:
            while True:
                match self.tap.get():
                    case None:
                        return
                    case fact:
                        try:
                            anyio.from_thread.run_sync(send.send_nowait, fact)
                        except WouldBlock:
                            Metrics.record({"rasm.runtime.pulse.dropped": 1.0}, domain="runtime", kind=fact[0].value, scope=self.scope)
                        except BrokenResourceError:
                            return

        async with anyio.create_task_group() as group:
            group.start_soon(_pulse_fold, receive, self.scope)
            await on_thread(pumped)
            send.close()


# --- [OPERATIONS] -----------------------------------------------------------------------


def probe[T](row: AdmitRow[T], unit: Admit[T], cache: Map[ContentKey, T]) -> tuple[Option[ContentKey], Option[T], LaneWork[T]]:
    key = row.key(unit)
    return key, key.bind(cache.try_find), row.make(unit)


def _tighter(lane: Option[float], unit: Option[float]) -> Option[float]:
    return lane.map(lambda held: unit.map(lambda own: min(held, own)).default_value(held)).or_else(unit)


def pulsed(tap: Queue[PulseFact | None], point_id: HookId, payload: Struct) -> None:
    try:
        tap.put_nowait((point_id, payload))
    except (Full, OSError, EOFError):
        pass


async def _pulse_fold(receive: MemoryObjectReceiveStream[PulseFact], scope: ScopeKey) -> None:
    async for point_id, payload in receive:
        if Hooks.fire(point_id, payload, scope=scope).is_error():
            Metrics.record({"rasm.runtime.pulse.rejected": 1.0}, domain="runtime", kind=point_id.value, scope=scope)


async def on_thread[T](fn: Callable[..., T], *args: object, abandon: bool = False, **kwargs: object) -> T:
    return await anyio.to_thread.run_sync(lambda: fn(*args, **kwargs), abandon_on_cancel=abandon, limiter=THREAD_BAND)


def _fire_listener(scheduler: AsyncIOScheduler, send: MemoryObjectSendStream[JobExecutionEvent]) -> Callable[[JobExecutionEvent], None]:
    def on_fire(event: JobExecutionEvent) -> None:
        try:
            send.send_nowait(event)
        except WouldBlock:
            pass
        except BrokenResourceError:
            scheduler.remove_listener(on_fire)

    return on_fire


async def _events[T](source: LaneSource[T]) -> AsyncIterator[Block[Admit[T]]]:
    match source:
        case LaneSource(tag="watched", watched=(watch, build)):
            narrowed = Option.of_optional(watch.filter).default_value(PythonFilter())
            async for batch in awatch(*watch.paths, watch_filter=narrowed, debounce=watch.debounce, step=watch.step):
                yield build(batch)
        case LaneSource(tag="scheduled", scheduled=(trigger, build)):
            scheduler, (send, receive) = AsyncIOScheduler(), anyio.create_memory_object_stream[JobExecutionEvent](max_buffer_size=FIRE_BUFFER)
            scheduler.add_listener(_fire_listener(scheduler, send), FIRE_MASK)
            scheduler.add_job(lambda: None, trigger=trigger)
            scheduler.start()
            try:
                async for event in receive:
                    yield build(event)
            finally:
                scheduler.shutdown(wait=False)
        case _ as unreachable:
            assert_never(unreachable)


async def feed[T](policy: LanePolicy, source: LaneSource[T]) -> AsyncIterator[Drained[T]]:
    async for batch in _events(source):
        yield await policy.drain(batch)


# --- [COMPOSITION] ----------------------------------------------------------------------

ADMIT_TABLE: Final[Map[AdmitTag, AdmitRow[object]]] = Map.of_seq([
    ("bare", AdmitRow(key=lambda _: Nothing, make=lambda unit: LaneWork(shared=unit.bare))),
    ("keyed", AdmitRow(key=lambda unit: Some(unit.keyed[0]), make=lambda unit: LaneWork(shared=unit.keyed[1]))),
    (
        "retried",
        AdmitRow(key=lambda _: Nothing, make=lambda unit: LaneWork(shared=lambda: guard(unit.retried[0])(unit.retried[1]))),
    ),
    ("whole", AdmitRow(key=lambda unit: unit.whole[0], make=lambda unit: LaneWork(whole=unit.whole[1]))),
])

_ISOLATION: Final[Map[WorkerKind, tuple[IsolationArm, CapacityLimiter]]] = Map.of_seq([
    (WorkerKind.INTERPRETER, (anyio.to_interpreter.run_sync, INTERPRETER_BAND)),
    (WorkerKind.THREAD, (anyio.to_thread.run_sync, THREAD_BAND)),
    (WorkerKind.WASM, (anyio.to_thread.run_sync, THREAD_BAND)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
