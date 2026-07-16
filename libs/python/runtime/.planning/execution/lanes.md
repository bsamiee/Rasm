# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration: `LanePolicy.drain` is the one polymorphic bounded drain over the three-case `Admit[T]` admission union, `LanePolicy.offload` the one kernel-isolation hop over the closed `Modality` axis, `StagePlan.execute` the concurrent-front stage DAG over that same drain, and `LaneSource` the one `scheduled`/`watched` feeder union — one budget, one receipt shape, one owner for every bounded lane the branch runs.

`drain` and `offload` share one deadline budget and one isolation axis, so the deadline contract is total across both hops, and a caller supplies its kernel — the lane never imports one. Cross-cutting concerns ride aspects, never inline call sites: the OTel trace context stitches across the offload hop, content-keyed work short-circuits on a cache hit that threads forward across `StagePlan` fronts, `reliability/resilience#RESILIENCE` `guard` rides each unit's `RetryClass`, and the `Metrics.observe`/`Signals.emit` pair rides one `drained` aspect. Bare `asyncio` is never imported — `anyio` owns every concurrency primitive, and cron is solely `apscheduler`.

## [01]-[INDEX]

- [01]-[LANE]: the bounded `drain` over the `Admit[T]` union, the three-modality `offload`, the concurrent-front `StagePlan`, and the `LaneSource` feeder union under one `drained` aspect.

## [02]-[LANE]

- Owner: `Admit[T]` discriminates a plain coroutine, a content-keyed cache unit, and a resilience-guarded unit by case, so one `drain` serves all three rather than three parallel methods, and `ADMIT_TABLE` folds each case through one behavior row — a new admission modality is one row, never a new method. `DrainReceipt[T]`/`DrainOutcome`/`DRAIN_COLUMNS` are the one canonical drain taxonomy IMPORTED from their `observability/receipts#RECEIPT` owner: a drain receipt is local evidence, so the vocabulary lives one tier down and this page, the `observability/metrics#METRIC` counter, and the receipts emit all read that one owner. `LaneSource` is a feeder union over the one drain, never a separate scheduler surface.
- Entry: `drain`'s deadline budget is the `Option[float]` a caller projects from the `execution/admission#CONTEXT` `Deadline` value object at lane construction. `offload`'s optional `retry` row rides the isolation leg as a lane aspect, so a content-keyed source stays `keyed` for the cache yet still retries a transient hop — never a per-caller retry loop. A `StagePlan` stage picks its own admission case — a cacheable stage mints `keyed` units, a transient-prone stage `retried`, a plain stage `bare` — and each front's `DrainReceipt.cache` threads forward, so a `keyed` unit re-admitted downstream replays the upstream `Ok` rather than recomputing.
- Auto: a tripped deadline is contained — the drain runs inside `move_on_after`, never a bare `fail_after` whose `TimeoutError` escapes as a raw `BaseExceptionGroup` — so a deadline trip cancels in-flight units and the receipt reports them as `cancelled` with the partial `values`/`faults` intact; no exception escapes a bounded lane without a receipt. Each unit sends its full `RuntimeRail[T]` over the stream rather than a pre-collapsed bool, so the typed fault survives into the receipt and the drain is lossless in both directions. Every schedule geometry the package ships is one `Trigger` member on the one `AsyncIOScheduler` — no `croniter`, no `aiocron`, no hand-rolled sleep cron.
- Auto: the session cache is an immutable `Map[ContentKey, T]` threaded on `DrainReceipt.cache` — a hit reproduces the exact `Ok` value and counts as `hit` distinct from `completed`, only an `Ok` folds back, and an `Error` re-runs while its fault accumulates. The cache is session-local in-memory, never a durable store: durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the trace stitch parents per the arm's module-state reality — the `THREAD` arm shares the interpreter, so the installed composite propagator is present and the kernel parents unconditionally; the `PROCESS` and `INTERPRETER` arms hold independent module state, so a worker that has not run the `observability/telemetry#TELEMETRY` install resolves the default text-map and runs unparented while the carrier still holds the parent for any span the kernel opens. The `PROCESS` arm pays the pickle IPC hop the `INTERPRETER` arm skips — the arm is keyed by the isolation the kernel needs (a process-global native flag, a GIL-hostile extension, a blocking syscall), never by a pickle-versus-no-pickle guess. A worker death retries under the caller's `retry` row or crosses the one `async_boundary` conversion, never a local catch.
- Growth: a new lane source is one `LaneSource` case plus one `_events` arm; a new admission modality one `Admit` case plus one `ADMIT_TABLE` row; a new isolation modality one `Modality` member plus one `_ISOLATION` row with every offload call site untouched; a new trigger one `Trigger` member; a new stage one `StagePlan` edge; a new watch tuning one `Watch` field; a new drain outcome dimension one member on the receipts-owned `DrainOutcome` plus its field, reaching the metrics counter and the receipt emit through the imported `DRAIN_COLUMNS`.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` the `scheduled` case mints, no second cron owner, no app lifecycle hook, no background loop without a drain receipt, and no unbounded task creation. The consumer contract on the receipt is column-driven: the metrics and receipts egress read the outcome counts off `DRAIN_COLUMNS` per column, never a full-struct `asdict` allocating the receipt's containers per export cycle.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from enum import StrEnum
from functools import cache, wraps
from graphlib import TopologicalSorter
from os import PathLike, process_cpu_count
from typing import Final, Literal, assert_never

import anyio
import anyio.to_interpreter
import anyio.to_process
import anyio.to_thread
from anyio import BrokenResourceError, CapacityLimiter, WouldBlock, move_on_after
from anyio.streams.memory import MemoryObjectSendStream
from apscheduler.events import EVENT_JOB_ERROR, EVENT_JOB_EXECUTED, EVENT_JOB_MISSED, JobExecutionEvent
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.calendarinterval import CalendarIntervalTrigger
from apscheduler.triggers.combining import AndTrigger, OrTrigger
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import propagate
from watchfiles import BaseFilter, Change, PythonFilter, awatch

from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DrainReceipt, Receipt, Redaction, Signals
from rasm.runtime.resilience import RetryClass, guard

# --- [TYPES] ----------------------------------------------------------------------------

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger | CalendarIntervalTrigger | AndTrigger | OrTrigger
type AdmitTag = Literal["bare", "keyed", "retried"]
type IsolationArm = Callable[..., Awaitable[object]]


class Modality(StrEnum):
    INTERPRETER = "interpreter"
    PROCESS = "process"
    THREAD = "thread"


@tagged_union(frozen=True)
class Admit[T]:
    tag: AdmitTag = tag()
    bare: Work[T] = case()
    keyed: tuple[ContentKey, Work[T]] = case()
    retried: tuple[RetryClass, Work[T]] = case()


@tagged_union(frozen=True)
class LaneSource[T]:
    tag: Literal["scheduled", "watched"] = tag()
    scheduled: tuple[Trigger, Callable[[JobExecutionEvent], Block[Admit[T]]]] = case()
    watched: tuple["Watch", Callable[[set[tuple[Change, str]]], Block[Admit[T]]]] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

FIRE_MASK: Final[int] = EVENT_JOB_EXECUTED | EVENT_JOB_ERROR | EVENT_JOB_MISSED
FIRE_BUFFER: Final[int] = 64

# the two process-wide isolation bands, sized to the schedulable CPU count once: WORKER_BAND bounds every PROCESS-modality native
# worker across all lanes (concurrent subprocess crossings never oversubscribe the host against each package's internal thread pool),
# THREAD_BAND every THREAD-modality hop; consumers arrive as ledger `python:` rows, never as sibling-minted limiters beside these bands.
WORKER_BAND: Final[CapacityLimiter] = CapacityLimiter(process_cpu_count() or 4)
THREAD_BAND: Final[CapacityLimiter] = CapacityLimiter(2 * (process_cpu_count() or 4))

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
        # the watch-fact receipt form is the lowercase `raw_str()` member name — never `str(change)` or the IntEnum value.
        return Block.of_seq(sorted((change.raw_str(), path) for change, path in batch))


# --- [SERVICES] -------------------------------------------------------------------------


# one slot allocator per lane identity: the frozen hashable `LanePolicy` is the memo key, reused across every `drain` and INTERPRETER
# `offload` — a fresh `CapacityLimiter` per call would bound nothing.
@cache
def _limiter(policy: "LanePolicy") -> CapacityLimiter:
    return CapacityLimiter(policy.capacity)


class LanePolicy(Struct, frozen=True):
    capacity: int
    deadline: Option[float] = Nothing

    @property
    def limiter(self) -> CapacityLimiter:
        return _limiter(self)

    async def drain[T](self, units: Block[Admit[T]], cache: Map[ContentKey, T] = Map.empty()) -> DrainReceipt[T]:
        limiter = self.limiter
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
        return DrainReceipt.of(len(units), len(replayed), resolved, replayed, cache)

    async def offload[T](
        self, kernel: Callable[..., T], *args: object, modality: Modality = Modality.INTERPRETER, retry: RetryClass | None = None
    ) -> RuntimeRail[T]:
        carrier: dict[str, str] = {}
        propagate.inject(carrier)
        arm, band = _ISOLATION[modality]
        limiter = band.default_value(self.limiter)

        async def run() -> T:
            return await arm(traced_kernel, carrier, kernel, *args, limiter=limiter)

        # the `guard(cls)` leg retries a transient `BrokenWorkerInterpreter`/`BrokenWorkerProcess` cold-start crash BEFORE
        # `async_boundary` converts the terminal raise; a `None` retry runs bare.
        async def hop() -> T:
            with move_on_after(self.deadline.default_value(float("inf"))):
                return await (guard(retry)(run) if retry is not None else run())
            raise TimeoutError("offload deadline elapsed")

        return await async_boundary("offload", hop)


class StagePlan(Struct, frozen=True):
    lane: LanePolicy
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]

    async def execute[T](self, work: Callable[[str, RetryClass], Sequence[Admit[T]]]) -> tuple[DrainReceipt[T], ...]:
        classes = {stage: cls for stage, cls in self.stages}
        order: TopologicalSorter[str] = TopologicalSorter({stage: () for stage in classes})
        for parent, child in self.edges:
            order.add(child, parent)
        order.prepare()
        # the stateful graphlib driver is the one boundary loop; `carried`/`collected` stay immutable values rebound per front, never a
        # mutated accumulator, and the loop depth is fronts not nodes.
        carried: Map[ContentKey, T] = Map.empty()
        collected: Block[DrainReceipt[T]] = Block.empty()
        while order.is_active():
            front = order.get_ready()
            units = Block.of_seq([unit for stage in front for unit in work(stage, classes[stage])])
            receipt = await self.lane.drain(units, carried)
            order.done(*front)
            carried, collected = receipt.cache, collected.append(Block.singleton(receipt))
        return tuple(collected)


# --- [OPERATIONS] -----------------------------------------------------------------------


def probe[T](row: AdmitRow[T], unit: Admit[T], cache: Map[ContentKey, T]) -> tuple[Option[ContentKey], Option[T], Work[T]]:
    key = row.key(unit)
    return key, key.bind(cache.try_find), row.make(unit)


def traced_kernel[T](carrier: dict[str, str], kernel: Callable[..., T], *args: object) -> T:
    # worker-side half of the offload stitch: the receipts pair — pure extract, token-paired attach scope — never an inline attach/detach dance.
    with Signals.attach(Signals.continue_inbound(carrier)):
        return kernel(*args)


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


def drained[**P, T](owner: str, redaction: Redaction) -> Callable[[Callable[P, Awaitable[DrainReceipt[T]]]], Callable[P, Awaitable[DrainReceipt[T]]]]:
    def aspect(fn: Callable[P, Awaitable[DrainReceipt[T]]]) -> Callable[P, Awaitable[DrainReceipt[T]]]:
        @wraps(fn)
        async def observed(*args: P.args, **kwargs: P.kwargs) -> DrainReceipt[T]:
            receipt = await fn(*args, **kwargs)
            Metrics.observe(receipt, in_flight=receipt.cancelled)
            Signals.emit(Receipt.of(owner, receipt), redaction)
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
    observed = drained(owner, redaction)(policy.drain)
    async for batch in _events(source):
        yield await observed(batch)


# --- [COMPOSITION] ----------------------------------------------------------------------

ADMIT_TABLE: Final[Map[AdmitTag, AdmitRow[object]]] = Map.of_seq([
    ("bare", AdmitRow(key=lambda _: Nothing, make=lambda unit: unit.bare)),
    ("keyed", AdmitRow(key=lambda unit: Some(unit.keyed[0]), make=lambda unit: unit.keyed[1])),
    ("retried", AdmitRow(key=lambda _: Nothing, make=lambda unit: lambda: guard(unit.retried[0])(unit.retried[1]))),
])

# the isolation axis as data: one row binds each `Modality` to its `anyio` arm and its band —
# `Nothing` selects the per-lane memoised limiter, the two `Some` rows the process-wide bands.
_ISOLATION: Final[Map[Modality, tuple[IsolationArm, Option[CapacityLimiter]]]] = Map.of_seq([
    (Modality.INTERPRETER, (anyio.to_interpreter.run_sync, Nothing)),
    (Modality.PROCESS, (anyio.to_process.run_sync, Some(WORKER_BAND))),
    (Modality.THREAD, (anyio.to_thread.run_sync, Some(THREAD_BAND))),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
