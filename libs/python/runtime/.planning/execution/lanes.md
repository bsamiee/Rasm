# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy.drain` is the one polymorphic bounded-drain owner: it opens a single `anyio` task group under one `CapacityLimiter` and one `move_on_after` deadline scope, admits a `Block[Admit[T]]` whose three cases — a bare `Work[T]`, a content-keyed `(ContentKey, Work[T])` cache unit, a resilience-guarded `(RetryClass, Work[T])` retried unit — fold through one `ADMIT_TABLE` row each into the same lane coroutine, and returns one parameterized `DrainReceipt[T]` carrying the collected `Block[T]` values, the threaded session cache, the typed `Block[BoundaryFault]`, and the five-column outcome tally. `StagePlan.execute` drives a multi-stage DAG over that same drain, each `graphlib.TopologicalSorter` dependency-level front running concurrently rather than serialized. `LaneSource` is the one feeder union — a `scheduled` `apscheduler` `AsyncIOScheduler`-driven fire over the complete six-member `Trigger` union, a `watched` `awatch` change batch whose `Watch` value object carries paths, filter, and the `debounce`/`step` batching axis as case data — both projecting their source event through the one `_events` `match` into one `Block[Admit[T]]` that the shared `feed` tail drains under one `@drained`-observed `drain`, yielding one observed `DrainReceipt[T]` per batch.

`drain` and `offload` are one budget with one isolation axis: a caller-supplied kernel offloads onto the `Modality` the call declares — `INTERPRETER` into per-subinterpreter execution under PEP 734, `PROCESS` into a crash-isolated worker process under the process-wide `WORKER_BAND`, `THREAD` into the worker-thread pool under the process-wide `THREAD_BAND` — every arm inside the same `move_on_after(self.deadline...)` scope, so the deadline contract is total across both hops. Cross-cutting concerns ride aspects, never inline call sites — the active OTel trace context stitches across the offload hop, content-keyed work short-circuits on a cache hit that threads forward across `StagePlan` fronts, the resilience `guard` rides each unit's `RetryClass` and the optional offload `retry`, and the `Metrics.observe(receipt, in_flight=...)` swap with the `Signals.emit(Receipt.of(owner, receipt), ...)` row ride one `drained` aspect. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive, cron solely `apscheduler`.

## [01]-[INDEX]

- [02]-[LANE]: the one `LanePolicy.drain` bounded task group, the `Admit[T]` three-arm admission ADT folded by `ADMIT_TABLE` (bare/keyed/retried), the parameterized `DrainReceipt[T]` carrying values + cache + faults + tally, the deadline-as-cancelled fault containment, the three-modality `offload` isolation axis with trace-context stitching and the two process-wide bands, the content-keyed cache short-circuit, the `guard`/`drained` cross-cutting aspects, the concurrent-front `StagePlan` DAG, the `LaneSource` `scheduled`/`watched` feeder union projected by one `_events` `match` and drained under the shared `@drained`-observed `feed` tail.

## [02]-[LANE]

- Owner: `LanePolicy` — the one bounded `anyio`-task-group drain with capacity and a cancellation scope, its `limiter` property resolving the `functools.cache`-memoised `_limiter(self)` so one `CapacityLimiter` is minted per frozen-hashable lane identity and shared across the lane's `drain` and interpreter-modality `offload` rather than re-minted per call; `Admit[T]` the closed `@tagged_union` admission shape with `bare`/`keyed`/`retried` cases so one drain discriminates a plain coroutine, a `(ContentKey, Work[T])` cache unit, and a `(RetryClass, Work[T])` resilience-guarded unit by case rather than three parallel methods; `ADMIT_TABLE` the `Map[AdmitTag, AdmitRow[object]]` of one behavior row per `Admit` tag keyed on the closed `AdmitTag` literal — each row a `key` projection (the `Option[ContentKey]` the drain probes the session cache with, `Nothing` for the un-keyed `bare`/`retried` cases) and a `make` projection (the `Work[T]` coroutine, raw for `bare`/`keyed` and `guard(cls)`-wrapped for `retried`) — so a new admission modality is one row, not a new method; `Modality` the closed three-member isolation vocabulary `offload` dispatches on through the `_ISOLATION` row table; `DrainReceipt[T]`/`DrainOutcome`/`DRAIN_COLUMNS` the one canonical drain taxonomy imported from its `observability/receipts#RECEIPT` owner (a drain receipt IS local evidence, so the vocabulary, the `get_args`-derived column tuple, and the `DrainReceipt.of` fold live one tier down and this page, the `observability/metrics#METRIC` counter, and the receipts drained emit all read the one owner); `StagePlan` the multi-stage DAG (stage edges, per-stage `RetryClass`, partial re-run) over the same drain; `LaneSource` the closed `@tagged_union` `scheduled`/`watched` feeder union with `Watch` the watched-case config value object, not a separate scheduler.
- Entry: `LanePolicy.drain` is the one polymorphic bounded drain over `Block[Admit[T]]` — it opens one `anyio.create_task_group` under one `CapacityLimiter` and the `move_on_after(self.deadline.default_value(float("inf")))` scope reading the one `LanePolicy.deadline: Option[float]` budget a caller projects from the `execution/admission#CONTEXT` `Deadline` value object at lane construction, folds each `Admit` case through its `ADMIT_TABLE` row (a `keyed` unit whose `ContentKey` already carries an `Ok` in the threaded session cache short-circuits without invoking the coroutine and increments `hit`; a `retried` unit binds `reliability/resilience#RESILIENCE` `guard(cls)` around the coroutine so transient faults retry under the typed policy row before the rail resolves; a `bare` unit runs directly), sends each resolved `RuntimeRail[T]` over one memory-object stream, and returns one `DrainReceipt[T]`; `LanePolicy.offload` routes a caller-supplied kernel onto its declared isolation modality — the `_ISOLATION` `Map[Modality, (arm, band)]` row resolves `INTERPRETER` to `anyio.to_interpreter.run_sync` under the per-lane memoised limiter, `PROCESS` to `anyio.to_process.run_sync` under the process-wide `WORKER_BAND`, and `THREAD` to `anyio.to_thread.run_sync` under the process-wide `THREAD_BAND` — inside a `move_on_after` scope reading the same `self.deadline` budget `drain` bounds with, never importing the kernel, injecting the active OTel context into a carrier the module-level `traced_kernel` shim extracts-and-attaches, and accepting an optional keyword-only `retry: RetryClass | None` that wraps the isolation leg in `guard(cls)` so a transient `BrokenWorkerInterpreter`/`BrokenWorkerProcess` cold-start crash retries under one stamina policy row BEFORE the conversion — the retry riding the offload leg as a lane aspect so a content-keyed source stays `keyed` for the cache yet still retries a transient hop, never a per-caller retry loop — then lifting a budget-exhausted retry or deadline `TimeoutError` through the one `reliability/faults#FAULT` `async_boundary`; `StagePlan.execute` drives the stage DAG through `graphlib.TopologicalSorter` in active `prepare`/`get_ready`/`done` mode so each dependency-level front runs concurrently under one `LanePolicy.drain` over the front's flattened `Admit` units `work(stage, cls)` emits — a cacheable stage minting `keyed` units, a transient-prone stage `retried` under the stage's own `RetryClass`, a plain stage `bare`, the admission case the stage's data rather than a forced uniform wrap — threading each front's `DrainReceipt.cache` forward into the next front's drain so a `keyed` unit re-admitted downstream replays the upstream `Ok` rather than recomputing, one `DrainReceipt` per front; `feed` threads one `@drained`-observed `policy.drain` over the `_events(source)` projector so a fed lane is observed by composition rather than yielding a bare `DrainReceipt` the caller re-threads — `_events` runs the union's source by `match` with an `assert_never` tail, a `scheduled` source registering its `Trigger` on the one `AsyncIOScheduler` whose `EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED` listener is the single fire seam pushing each `JobExecutionEvent` over a memory stream and a `watched` source iterating the `awatch` stream under its `Watch` row's filter and `debounce`/`step` batching, both projecting the source event through the case's `build` callable into one `Block[Admit[T]]` the shared `feed` tail drains and observes per batch, never a per-case `yield await policy.drain(build(...))` tail duplicated across the arms.
- Auto: cancellation rides the `move_on_after` scope; capacity is per-arm ownership — the per-lane memoised `_limiter` bounds `drain` units and `INTERPRETER` hops on one slot allocator over the lane's lifetime, while `WORKER_BAND` and `THREAD_BAND` are the two process-wide bands every lane's `PROCESS`/`THREAD` hops contend, so concurrent native workers never oversubscribe the host against each package's own internal thread pool regardless of how many lanes exist; a tripped deadline is contained — the task-group drain runs inside `move_on_after` (not a bare `fail_after` that escapes as a raw `TimeoutError`/`BaseExceptionGroup`), so a deadline trip cancels the in-flight units and the receipt reports them as `cancelled = accepted - hit - len(resolved)` with the partial `values`/`faults` intact, never an exception escaping a bounded lane without a receipt; the `feed` source discrimination is one `match` with an `assert_never` tail so the arm set is total over the closed `LaneSource` union; a `scheduled` fire resolves through the COMPLETE `apscheduler` trigger union — cron (`CronTrigger`/`from_crontab`), fixed interval (`IntervalTrigger`), one-off (`DateTrigger`), calendar-unit (`CalendarIntervalTrigger`), and the combining gates (`AndTrigger`/`OrTrigger`) — on the one `AsyncIOScheduler`, so every schedule geometry the package ships is one `Trigger` member, no `croniter`, no `aiocron`, no hand-rolled `anyio.sleep` cron loop.
- Auto: each lane unit sends its full `RuntimeRail[T]` outcome over the memory-object stream rather than a pre-collapsed bool, so the typed `BoundaryFault` the `reliability/faults#FAULT` rail mints survives into the receipt — `DrainReceipt.of` splits the resolved rails with `Block.choose(rail.swap().to_option())` into the `faults` `Block[BoundaryFault]` and recovers the `Ok` values with `Block.choose(rail.to_option())` into the `values` `Block[T]`, mirroring the `faults#traversed` accumulate fold so a drained lane surfaces both which units failed and the values that succeeded; `rejected` is the cardinality of that typed `Block`, never a fault-erasing count, and `completed` is the cardinality of the recovered values, so the receipt is lossless in both directions.
- Auto: the content-keyed admission folds the `ContentKey` from `evidence/identity#IDENTITY` so a settings change misses correctly — the cache is an `expression` `Map[ContentKey, T]` threaded immutably across one session on the `DrainReceipt.cache` field, a hit reproduces the exact `Ok` value and increments `hit` distinct from `completed`, only an `Ok` outcome folds back into the `Map` and an `Error` re-runs while its fault accumulates, and the `Map` carries the most expensive offline work by reference; the cache is session-local in-memory, never a durable store — durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the offload trace stitch is one carrier across every modality — the lane injects the active context into a plain `dict[str, str]` through `propagate.inject` before the hop and the module-level `traced_kernel` shim composes the `observability/receipts#RECEIPT` pair inside the worker — `Signals.continue_inbound(carrier)` the pure extract, `Signals.attach` the token-paired scope around exactly the caller kernel — so the W3C `traceparent` crosses on the carrier whichever arm runs and the inline attach/detach dance is never re-spelled beside the owning pair. The parent attaches per the arm's module-state reality: the `THREAD` arm shares the interpreter, so the installed composite propagator is present and the kernel parents unconditionally; the `PROCESS` and `INTERPRETER` arms hold independent module state, so a worker that has not run the `observability/telemetry#TELEMETRY` install resolves the default text-map and runs the kernel unparented while the carrier still holds the parent for any span the kernel opens — best-effort across those hops, never a propagator re-install. A worker death (`BrokenWorkerProcess` from the process arm, `BrokenWorkerInterpreter` from the interpreter arm) either retries under the caller's optional `retry` row or crosses the one `async_boundary` conversion, and the `PROCESS` arm pays the pickle IPC hop the `INTERPRETER` arm skips — the arm is keyed by the isolation the kernel needs (a process-global native flag, a GIL-hostile extension, a blocking syscall), never by a pickle-versus-no-pickle guess.
- Auto: the watch source is the `watched` `LaneSource` case carrying one `Watch` value object — paths, the `BaseFilter | None` a `None` lifts to `PythonFilter()`, and the `debounce`/`step` batching axis as CASE DATA the `awatch` call threads, never baked call-site defaults a consumer cannot tune — and a watch fact serializes through `Watch.facts`, the `(Change.raw_str(), path)` projection whose lowercase member name is the receipt form a `build` callable emits, never `str(change)`, the integer value, or a re-derived kind string; the consumer matches on the `Change` enum and never runs a `stat` polling loop, the source owning the cancel scope and capacity while `watchfiles` owns the change stream.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`move_on_after`/`create_memory_object_stream`/`streams.memory.MemoryObjectSendStream`/`to_interpreter.run_sync`/`to_process.run_sync`/`to_thread.run_sync` the three isolation arms/`WouldBlock`/`BrokenResourceError` the bounded fire-seam stream's two distinct dispositions, the authorized missed-fire drop and the receiver-gone listener retirement — `BrokenWorkerInterpreter`/`BrokenWorkerProcess` cross the faults owner's `async_boundary` rather than being caught here), `watchfiles` (`awatch(watch_filter=, debounce=, step=)`/`PythonFilter`/`BaseFilter`/`Change`/`Change.raw_str`), `apscheduler` (`schedulers.asyncio.AsyncIOScheduler`/`triggers.cron.CronTrigger`/`CronTrigger.from_crontab`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`triggers.calendarinterval.CalendarIntervalTrigger`/`triggers.combining.AndTrigger`/`OrTrigger`/`add_listener`/`remove_listener`/`add_job`/`start`/`shutdown`/`events.JobExecutionEvent`/`EVENT_JOB_EXECUTED`/`EVENT_JOB_ERROR`/`EVENT_JOB_MISSED`), `opentelemetry-api` (`propagate.inject` the loop-side carrier fill; the worker side composes the receipts `continue_inbound`/`attach` pair), `expression` (`Map`/`Block` persistent owners, `Option`/`Result` combinators, `tagged_union`/`case`/`tag`), `graphlib` (stdlib `TopologicalSorter` driven in active `prepare`/`get_ready`/`done` mode for concurrent same-level fronts), `functools` (`cache` memoising the per-lane-identity `CapacityLimiter`, `wraps` preserving the `drained` aspect signature), `msgspec` (`Struct` the frozen carrier for `AdmitRow`/`LanePolicy`/`StagePlan`/`Watch`; the metrics and receipts egress reads the five outcome counts off the imported `DRAIN_COLUMNS` through one `getattr(receipt, column)` per column — never a full-struct `asdict` allocating the receipt's containers per export cycle).
- Growth: a new lane source is one `LaneSource` case with its `build` projection plus one `_events` match arm, the shared `@drained`-observed `feed` tail draining and observing it with no new entry; a new admission modality is one `Admit` case plus one `ADMIT_TABLE` row, never a new `LanePolicy` method; a new isolation modality is one `Modality` member plus one `_ISOLATION` row binding its arm and band — every offload call site untouched; a new trigger modality is one member the six-row `Trigger` union already closes over; a new stage is one edge on `StagePlan`; a transient-prone offload kernel passes one optional `retry=RetryClass.<row>` so the `guard(cls)` leg retries a worker cold-start crash without changing the admission shape; a new watch tuning is one `Watch` field the case data already carries; a new drain outcome dimension is one member on the receipts-owned `DrainOutcome` literal plus its `DrainReceipt` field, reaching the metrics counter and the receipt emit through the imported `DRAIN_COLUMNS` by that one add; zero new surface.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` the `scheduled` case mints, no second cron owner beside `apscheduler`, no app lifecycle hook, no background loop without a drain receipt, and no unbounded task creation. The deleted forms: a kernel the lane imports rather than receives; a serial stage walk where a same-level front runs concurrently; a per-front cache reset where the one `while order.is_active()` boundary loop rebinds each `DrainReceipt.cache` forward as immutable loop-carried state; an unbounded `offload` leg escaping the lane deadline where the same `move_on_after` scope bounds both hops; a fresh-root span on the offloaded leg where the calling span is the parent; a `modality: str` knob or a `to_process`/`to_thread` sibling method family where the one `Modality` value keys the `_ISOLATION` row; a per-consumer `CapacityLimiter` minted beside the two process-wide bands where `WORKER_BAND`/`THREAD_BAND` are the one native-worker and one thread bound (a sibling folder's local `CapacityLimiter(n)` over native work is the collapse target, arriving as a ledger `python:` row when it lands); a `croniter`/`aiocron`/hand-rolled sleep cron beside the six-member `Trigger` union; a per-event path-string filter where the `Watch` row's filter narrows the stream; baked `debounce`/`step` defaults where the `Watch` case data carries them; three parallel admission methods where one `drain` discriminates by `Admit` case; four divergent return shapes where one parameterized `DrainReceipt[T]` carries values/cache/faults/tally; a count-only drain that discards the computed `Ok` values; a double `ADMIT_TABLE[unit.tag]` resolution or double `cache.try_find` per unit where the one `probe` fold resolves row and hit into a triple one `Block.partition` splits; a bare `fail_after` whose `TimeoutError` escapes a bounded lane without a receipt; a fresh `CapacityLimiter(self.capacity)` per `drain`/`offload` call where the memoised per-identity `_limiter` is the lane allocator; a hand-built `Receipt(drained=...)` constructor where the receipts owner's shape-polymorphic `Receipt.of(owner, receipt)` discriminates; an `observe(receipt)` that drops the `in_flight` occupancy where the aspect threads `in_flight=receipt.cancelled`; a fire-seam listener raising into the `apscheduler` dispatch — or one collapsed arm eating both stream signals — where `_fire_seam` routes `WouldBlock` as the scheduler's own `coalesce`/`misfire_grace_time` missed-fire drop and `BrokenResourceError` as the listener's own `remove_listener` retirement; a per-case drain tail duplicated across `feed` arms where the one `_events` projector emits and the shared tail observes; and inline call-site retry/telemetry/receipt threading where the `guard`/`drained` aspects own the cross-cut. Bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

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

# the two process-wide isolation bands: WORKER_BAND bounds every PROCESS-modality native worker
# across all lanes so concurrent subprocess crossings never oversubscribe the host against each
# package's own internal thread pool; THREAD_BAND bounds every THREAD-modality offload hop
# (blocking or GIL-releasing kernels). Both are sized to the schedulable CPU count once;
# consumers arrive as ledger `python:` rows, never as sibling-minted limiters beside these bands.
WORKER_BAND: Final[CapacityLimiter] = CapacityLimiter(process_cpu_count() or 4)
THREAD_BAND: Final[CapacityLimiter] = CapacityLimiter(2 * (process_cpu_count() or 4))

# --- [MODELS] ---------------------------------------------------------------------------


class AdmitRow[T](Struct, frozen=True):
    key: Callable[[Admit[T]], Option[ContentKey]]
    make: Callable[[Admit[T]], Work[T]]


class Watch(Struct, frozen=True):
    # the watched-case config value object: filter and the debounce/step batching axis are case
    # DATA the `awatch` call threads, so a consumer tunes batching without a new source case.
    paths: tuple[str | PathLike[str], ...]
    filter: BaseFilter | None = None
    debounce: int = 1600
    step: int = 50

    @staticmethod
    def facts(batch: set[tuple[Change, str]]) -> Block[tuple[str, str]]:
        # the watch-fact receipt form: the lowercase `raw_str()` member name, never `str(change)`
        # or the IntEnum value — the serialization a `build` callable emits onto its receipts.
        return Block.of_seq(sorted((change.raw_str(), path) for change, path in batch))


# --- [SERVICES] -------------------------------------------------------------------------


# the one bounded slot allocator per lane identity; minted once and reused across every
# `drain` and INTERPRETER `offload` on a policy (the frozen `LanePolicy` is hashable, so the
# policy is the memo key — a fresh `CapacityLimiter` per call would bound nothing).
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
        # every eagerly-minted clone is adopted by exactly one child whose `async with sink`
        # closes it even on the cancellation unwind, so the post-scope drain reaches
        # `EndOfStream` (the deadline-trip case sends the buffered partial, not a hang).
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

        # an optional `retry` row wraps the isolation leg in `resilience#guard(cls)` so a transient
        # `BrokenWorkerInterpreter`/`BrokenWorkerProcess` cold-start crash retries under one stamina
        # policy row BEFORE `async_boundary` converts the terminal raise; a `None` retry runs bare.
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
        # the stateful graphlib driver (`prepare`/`is_active`/`get_ready`/`done`) is the one
        # boundary loop; `carried`/`collected` stay immutable `Map`/`Block` rebound per front,
        # never a mutated accumulator, and the loop depth is fronts not nodes (no recursion).
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
    # worker-side half of the offload stitch: composes the receipts pair — the pure extract, the
    # token-paired attach scope — never an inline attach/detach dance re-spelled beside it.
    with Signals.attach(Signals.continue_inbound(carrier)):
        return kernel(*args)


def _fire_seam(scheduler: AsyncIOScheduler, send: MemoryObjectSendStream[JobExecutionEvent]) -> Callable[[JobExecutionEvent], None]:
    def on_fire(event: JobExecutionEvent) -> None:
        # the bounded stream's two failure signals are distinct dispositions, never one collapsed
        # arm: WouldBlock is the authorized missed-fire drop (the scheduler's own coalesce policy),
        # BrokenResourceError means the feed consumer is gone, so the listener retires itself.
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
