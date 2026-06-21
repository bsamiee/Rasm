# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy.drain` is the one polymorphic bounded-drain owner: it opens a single `anyio` task group under one `CapacityLimiter` and one `move_on_after` deadline scope, admits a `Block[Admit[T]]` whose three cases — a bare `Work[T]`, a content-keyed `(ContentKey, Work[T])` cache unit, a resilience-guarded `(RetryClass, Work[T])` retried unit — fold through one `ADMIT_TABLE` row each into the same lane coroutine, and returns one parameterized `DrainReceipt[T]` carrying the collected `Block[T]` values, the threaded session cache, the typed `Block[BoundaryFault]`, and the five-column outcome tally. `StagePlan.execute` drives a multi-stage DAG over that same drain, each `graphlib.TopologicalSorter` dependency-level front running concurrently rather than serialized. `LaneSource` is the one feeder union — a `scheduled` `apscheduler` `AsyncIOScheduler`-driven fire, a `watched` `PythonFilter`-narrowed `awatch` change batch — both projecting their source event through the one `_events` `match` into one `Block[Admit[T]]` that the shared `feed` tail drains under one `@drained`-observed `drain`, yielding one observed `DrainReceipt[T]` per batch.

`drain` and `offload` are one budget: a CPU-bound kernel offloads into per-subinterpreter execution under PEP 734, inside the same `move_on_after(self.deadline...)` scope and the one shared per-lane `CapacityLimiter` the units contend, so both hops obey one slot allocator and one deadline. Cross-cutting concerns ride aspects, never inline call sites — the active OTel trace context stitches across the offload hop, content-keyed work short-circuits on a cache hit that threads forward across `StagePlan` fronts, the resilience `guard` rides each unit's `RetryClass`, and the `Metrics.observe(receipt, in_flight=...)` swap with the `Signals.emit(Receipt.of(owner, receipt), ...)` row ride one `drained` aspect. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive, cron solely `apscheduler`.

## [01]-[INDEX]

- [01]-[LANE]: the one `LanePolicy.drain` bounded task group, the `Admit[T]` three-arm admission ADT folded by `ADMIT_TABLE` (bare/keyed/retried), the parameterized `DrainReceipt[T]` carrying values + cache + faults + tally, the deadline-as-cancelled fault containment, the CPU-bound subinterpreter offload with trace-context stitching, the content-keyed cache short-circuit, the `guard`/`drained` cross-cutting aspects, the concurrent-front `StagePlan` DAG, the `LaneSource` `scheduled`/`watched` feeder union projected by one `_events` `match` and drained under the shared `@drained`-observed `feed` tail.

## [02]-[LANE]

- Owner: `LanePolicy` — the one bounded `anyio`-task-group drain with capacity and a cancellation scope, its `limiter` property resolving the `functools.cache`-memoised `_limiter(self)` so one `CapacityLimiter` is minted per frozen-hashable lane identity and shared across the lane's `drain` and `offload` rather than re-minted per call; `Admit[T]` the closed `@tagged_union` admission shape with `bare`/`keyed`/`retried` cases so one drain discriminates a plain coroutine, a `(ContentKey, Work[T])` cache unit, and a `(RetryClass, Work[T])` resilience-guarded unit by case rather than three parallel methods; `ADMIT_TABLE` the `Map[AdmitTag, AdmitRow[object]]` of one behavior row per `Admit` tag keyed on the closed `AdmitTag` literal (not a bare `str`) — each row a `key` projection (the `Option[ContentKey]` the drain probes the session cache with, `Nothing` for the un-keyed `bare`/`retried` cases) and a `make` projection (the `Work[T]` coroutine, raw for `bare`/`keyed` and `guard(cls)`-wrapped for `retried`) — so a new admission modality is one row, not a new method; `DrainReceipt[T]` the one parameterized outcome carrying `values: Block[T]`, `cache: Map[ContentKey, T]`, the typed `faults: Block[BoundaryFault]`, and the `accepted`/`completed`/`cancelled`/`rejected`/`hit` counts — never a bare scalar tally and never a count-only return that drops the computed values; `DrainOutcome`/`DRAIN_COLUMNS` the one canonical five-column outcome taxonomy this page owns — the `DrainOutcome` literal is the bounded vocabulary and `DRAIN_COLUMNS` derives from it through `get_args` so the column set is one typed fact, and the sibling `observability/metrics#METRIC` `lane.drained` counter and `observability/receipts#RECEIPT` drained emit import this taxonomy rather than redeclaring it, so the counter outcome dimension and the receipt column set can never drift; `StagePlan` the multi-stage DAG (stage edges, per-stage `RetryClass`, partial re-run) over the same drain; `LaneSource` the closed `@tagged_union` `scheduled`/`watched` feeder union, not a separate scheduler.
- Entry: `LanePolicy.drain` is the one polymorphic bounded drain over `Block[Admit[T]]` — it opens one `anyio.create_task_group` under one `CapacityLimiter` and the `move_on_after(self.deadline.default_value(float("inf")))` scope reading the one `LanePolicy.deadline: Option[float]` budget a caller projects from the `execution/admission#CONTEXT` `Deadline` value object at lane construction, folds each `Admit` case through its `ADMIT_TABLE` row (a `keyed` unit whose `ContentKey` already carries an `Ok` in the threaded session cache short-circuits without invoking the coroutine and increments `hit`; a `retried` unit binds `reliability/resilience#RESILIENCE` `guard(cls)` around the coroutine so transient faults retry under the typed policy row before the rail resolves; a `bare` unit runs directly), sends each resolved `RuntimeRail[T]` over one memory-object stream, and returns one `DrainReceipt[T]`; `LanePolicy.offload` routes a caller-supplied CPU kernel into per-subinterpreter execution through `anyio.to_interpreter.run_sync` under the one shared per-lane `CapacityLimiter` and a `move_on_after` scope reading the same `self.deadline` budget `drain` bounds with, never importing the kernel, injecting the active OTel context into a carrier the module-level `traced_kernel` shim extracts-and-attaches so the offloaded span seeds from the calling span, lifting a `BrokenWorkerInterpreter`/deadline `TimeoutError` through the one `reliability/faults#FAULT` `async_boundary`; `StagePlan.execute` drives the stage DAG through `graphlib.TopologicalSorter` in active `prepare`/`get_ready`/`done` mode so each dependency-level front runs concurrently under one `LanePolicy.drain` over the front's flattened `Admit` units, threading each stage's `RetryClass` into the admission so the units enter as `retried` cases and threading each front's `DrainReceipt.cache` forward into the next front's drain so a `keyed` unit re-tessellated by a downstream stage replays the upstream `Ok` rather than recomputing, one `DrainReceipt` per front; `feed` threads one `@drained`-observed `policy.drain` over the `_events(source)` projector so a fed lane is observed by composition rather than yielding a bare `DrainReceipt` the caller re-threads — `_events` runs the union's source by `match` with an `assert_never` tail, a `scheduled` source registering its `Trigger` on the one `AsyncIOScheduler` whose `EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED` listener is the single fire seam pushing each `JobExecutionEvent` over a memory stream and a `watched` source iterating the `PythonFilter`-narrowed `awatch` stream, both projecting the source event through the case's `build` callable into one `Block[Admit[T]]` the shared `feed` tail drains and observes per batch, never a per-case `yield await policy.drain(build(...))` tail duplicated across the arms.
- Auto: cancellation rides the `move_on_after` scope; capacity rides one `CapacityLimiter` minted once per lane identity and shared across every `drain` and `offload` — the `LanePolicy.limiter` property reads the `functools.cache`-memoised `_limiter(self)` keyed on the frozen-hashable policy, so two policies with identical `capacity`/`deadline` share one bound and a single lane caps both its concurrent `drain` units and its `offload` subinterpreter hops on one slot allocator rather than a fresh per-call limiter that bounds nothing across the lane's lifetime; a tripped deadline is contained — the task-group drain runs inside `move_on_after` (not a bare `fail_after` that escapes as a raw `TimeoutError`/`BaseExceptionGroup`), so a deadline trip cancels the in-flight units and the receipt reports them as `cancelled = accepted - hit - len(resolved)` (the went-live cardinality minus the cardinality that reached the stream) with the partial `values`/`faults` intact, never an exception escaping a bounded lane without a receipt; the `feed` source discrimination is one `match` with an `assert_never` tail so the arm set is total over the closed `LaneSource` union; a `scheduled` fire resolves cron, interval, and one-off through the `apscheduler` `Trigger` union on the one `AsyncIOScheduler`, cron owned solely by `apscheduler` `CronTrigger`/`from_crontab` — no `croniter`, no `aiocron`, no hand-rolled `anyio.sleep` cron loop.
- Auto: each lane unit sends its full `RuntimeRail[T]` outcome over the memory-object stream rather than a pre-collapsed bool, so the typed `BoundaryFault` the `reliability/faults#FAULT` rail mints survives into the receipt — `DrainReceipt.of` splits the resolved rails with `Block.choose(rail.swap().to_option())` into the `faults` `Block[BoundaryFault]` and recovers the `Ok` values with `Block.choose(rail.to_option())` into the `values` `Block[T]`, mirroring the `faults#traversed` accumulate fold so a drained lane surfaces both which units failed and the values that succeeded; `rejected` is the cardinality of that typed `Block`, never a fault-erasing count, and `completed` is the cardinality of the recovered values, so the receipt is lossless in both directions.
- Auto: the content-keyed admission folds the `ContentKey` from `evidence/identity#IDENTITY` so a settings change misses correctly — the cache is an `expression` `Map[ContentKey, T]` threaded immutably across one session on the `DrainReceipt.cache` field, a hit reproduces the exact `Ok` value and increments `hit` distinct from `completed`, only an `Ok` outcome folds back into the `Map` (the `DrainReceipt.of` thread `pair[0].bind(lambda key: pair[1].to_option().map(lambda v: acc.add(key, v))).default_value(acc)` adds the `(key, value)` pair only when the unit both carried a `ContentKey` and resolved `Ok`) and an `Error` re-runs while its fault accumulates, and the `Map` carries the most expensive offline work (re-tessellation, IFC evaluation, scan registration) by reference; the cache is session-local in-memory, never a durable store — durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the CPU-bound offload uses `anyio.to_interpreter.run_sync(traced_kernel, carrier, kernel, *args, limiter=self.limiter)` under the same memoised per-lane `CapacityLimiter` the drain bounds with and inside a per-leg `move_on_after(self.deadline.default_value(float("inf")))` scope reading the same `self.deadline` budget, so each subinterpreter carries its own GIL under PEP 734, heavy geometry work never stalls the companion event loop or contends the main GIL with no pickle round-trip on the subinterpreter path, and a hop that overruns the lane deadline is cancelled at the scope and falls through to the explicit `raise TimeoutError` the one `async_boundary` converts (the `move_on_after` arm returns the value on a clean hop and the post-scope `raise` fires only on a silent deadline trip) — the deadline contract is total across both `drain` and `offload`, never an unbounded offload leg escaping the lane budget; the lane injects the active context into a plain `dict[str, str]` carrier through `propagate.inject` before the hop and the module-level `traced_kernel` shim runs `propagate.extract`+`context.attach` inside the worker before invoking the caller kernel and `context.detach` in a `finally`, so the W3C `traceparent` crosses the boundary on the carrier and the offloaded work parents under the calling span where the worker's `opentelemetry` module state carries the installed composite propagator — a PEP 734 subinterpreter without that install resolves the default text-map and runs the kernel unparented while the carrier still holds the parent for a span the kernel opens, the stitch best-effort across the hop rather than an unconditional reparent; the kernel is a caller-supplied `Callable` the lane never imports, and a `BrokenWorkerInterpreter` raised inside the worker crosses the one `reliability/faults#FAULT` `async_boundary` conversion rather than a bare raise — the lane offloads, it never owns the geometry/scan numeric loops the `libs/python` siblings own.
- Auto: cross-cutting concerns ride aspects, not inline call-site code — `retried` admission binds `reliability/resilience#RESILIENCE` `guard(cls)` as a per-unit retry aspect woven into the lane coroutine so retry is one admission case rather than caller boilerplate around every coroutine; the `drained` aspect (the `@drained(owner, redaction)` decorator, `functools.wraps`-preserving) wraps a drain so its `DrainReceipt` feeds the `observability/metrics#METRIC` `Metrics.observe(receipt, in_flight=receipt.cancelled)` atomic-swap — threading the went-live-but-unresolved cardinality as the live `lane.in_flight` `ObservableUpDownCounter` occupancy the metrics owner reads off the swap — and mints the `observability/receipts#RECEIPT` row through that owner's one shape-polymorphic `Receipt.of(owner, receipt)` factory (which discriminates the `DrainReceipt` input into the `drained` case), never the hand-built `Receipt(drained=...)` constructor the receipts owner names a deleted form, both signals firing in one egress so a lane is observed by composition rather than each caller re-threading the receipt to the metric spine and the log stream; the `outcome` attribute the metric counter and histogram key by is the shared `DrainOutcome` taxonomy folded from the `DrainReceipt` columns, never restamped per call.
- Auto: the watch source is the `watched` `LaneSource` case wrapping `watchfiles.awatch(*paths, watch_filter=Option.of_optional(watch_filter).default_value(PythonFilter()))` so a `None` filter lifts to `watchfiles.PythonFilter()` and only Python-source `Change` events feed the lane, the consumer matching on the `Change` enum rather than string-comparing event kinds and never running a `stat` polling loop; each change batch is projected through the case's `build` callable into one `Block[Admit[T]]` entering the same drain, the source owning the cancel scope and capacity while `watchfiles` owns the change stream.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`move_on_after`/`create_memory_object_stream`/`streams.memory.MemoryObjectSendStream`/`to_interpreter.run_sync`/`WouldBlock` the bounded-stream backpressure signal the fire seam suppresses — `BrokenWorkerInterpreter` crosses the faults owner's `async_boundary` rather than being caught here), `watchfiles` (`awatch`/`PythonFilter`/`BaseFilter`/`Change`), `apscheduler` (`schedulers.asyncio.AsyncIOScheduler` the responsive-service scheduler the `BlockingScheduler` ban requires/`triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`CronTrigger.from_crontab`/`add_listener`/`add_job`/`start`/`shutdown`/`events.JobExecutionEvent`/`events.EVENT_JOB_EXECUTED`/`EVENT_JOB_ERROR`/`EVENT_JOB_MISSED`), `opentelemetry-api` (`propagate.inject`/`propagate.extract`/`context.attach`/`context.detach` for the offload context stitch), `expression` (`Map.of_seq`/`Map.empty`/`Map.try_find`/`map[key]`/`Block.of_seq`/`Block.empty`/`Block.singleton`/`Block.map`/`Block.choose`/`Block.partition`/`Block.fold`/`Block.append`/`Nothing`/`Some`/`Ok`/`Option.of_optional`/`Option.bind`/`Option.map`/`Option.map2`/`Option.default_value`/`Result.swap`/`Result.to_option`/`tagged_union`/`case`/`tag`), `graphlib` (stdlib `TopologicalSorter` driven in active `prepare`/`get_ready`/`done` mode for concurrent same-level fronts), `functools` (`cache` memoising the per-lane-identity `CapacityLimiter`, `wraps` preserving the `drained` aspect signature), `contextlib` (stdlib `suppress(WouldBlock)` containing the bounded fire-seam buffer overflow as the scheduler's own `coalesce`/`misfire_grace_time` missed-fire policy rather than a raise into the listener), `msgspec` (`Struct` the frozen carrier for `DrainReceipt`/`AdmitRow`/`LanePolicy`/`StagePlan`; the `observability/metrics#METRIC` and `observability/receipts#RECEIPT` egress reads the five outcome counts off this `DrainReceipt` through one `getattr(receipt, column)` per imported `DRAIN_COLUMNS` literal — never a `msgspec.structs.asdict` that would allocate this owner's `values`/`cache`/`faults` containers on every egress only to drop them).
- Growth: a new lane source is one `LaneSource` case with its `build` projection plus one `_events` match arm, the shared `@drained`-observed `feed` tail draining and observing it with no new entry; a new admission modality is one `Admit` case plus one `ADMIT_TABLE` row, never a new `LanePolicy` method; a new trigger modality is one `apscheduler` `Trigger` row on a `scheduled` source; a new stage is one edge on `StagePlan`; a CPU kernel is one `offload` callable carried across the context stitch unchanged; a cached unit is one `keyed` `Admit`; a retry-guarded unit is one `retried` `Admit`; a new watch filter is one `BaseFilter` on a `watched` source; a new drain outcome dimension is one member on the `DrainOutcome` literal plus its `DrainReceipt` field, reaching the metrics counter and the receipt emit through the imported `DRAIN_COLUMNS` by that one add; zero new surface.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` the `scheduled` `LaneSource` case mints, second cron owner beside `apscheduler`, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation, a second scheduler surface, a process-pool serialization tax on a CPU kernel a subinterpreter isolates, a durable lane cache, a kernel the lane imports rather than receives, a serial stage walk where a same-level front could run concurrently, a per-front cache reset that discards the upstream `Ok` a downstream `keyed` stage could replay where the one `while order.is_active()` boundary loop rebinds each `DrainReceipt.cache` forward as immutable loop-carried state, an unbounded `offload` leg that overruns the lane deadline where the same `move_on_after` scope bounds both `drain` and `offload`, a fresh-root span on the offloaded leg where the calling span is the parent, a `croniter`/`aiocron`/hand-rolled `anyio.sleep` cron replacement beside the `apscheduler` `Trigger` union driven on the one scheduler, a per-event path-string filter where `PythonFilter` already narrows the change stream, three parallel admission methods (`run`/`cached`/`retried`) where one `drain` discriminates the admission shape by `Admit` case (the retained `offload` is the one CPU-kernel subinterpreter primitive sharing the lane budget, not a fourth admission method), four divergent return shapes where one parameterized `DrainReceipt[T]` carries values/cache/faults/tally, a count-only drain that discards the computed `Ok` values, a double `ADMIT_TABLE[unit.tag]` row resolution or a double `cache.try_find` per unit where the one `probe` operation resolves the row once and folds the cache hit into a `(Option[ContentKey], Option[T], Work[T])` triple a single `Block.partition` splits, a bare `fail_after` whose `TimeoutError` escapes a bounded lane without a receipt, a fresh `CapacityLimiter(self.capacity)` minted per `drain`/`offload` call that bounds nothing across the lane where the memoised per-identity `_limiter` is the one slot allocator, a hand-built `Receipt(drained=(owner, receipt))` constructor at the `drained` aspect where the receipts owner's shape-polymorphic `Receipt.of(owner, receipt)` discriminates the `DrainReceipt` into the `drained` case, an `observe(receipt)` that drops the `in_flight` occupancy the `lane.in_flight` `ObservableUpDownCounter` reads where the aspect threads `in_flight=receipt.cancelled`, a `send.send_nowait` fire-seam listener that raises `WouldBlock` into the `apscheduler` dispatch on a backpressured buffer where the named `_fire_seam` suppresses it as the scheduler's own missed-fire policy, a per-case `yield await policy.drain(build(...))` tail duplicated across the `feed` arms where the one `_events` projector emits the `Block[Admit[T]]` and the shared `feed` tail owns the single `@drained`-observed drain, a `feed` that yields a bare un-observed `DrainReceipt` the caller must re-thread to the metric spine where the `drained` aspect rides the shared tail, and inline call-site retry/telemetry/receipt threading where the `guard`/`drained` aspects own the cross-cut are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from contextlib import suppress
from functools import cache, wraps
from graphlib import TopologicalSorter
from os import PathLike
from typing import Final, Literal, assert_never, get_args

import anyio
from anyio import CapacityLimiter, WouldBlock, move_on_after
from anyio.streams.memory import MemoryObjectSendStream
from anyio.to_interpreter import run_sync as interpreter_run_sync
from apscheduler.events import EVENT_JOB_ERROR, EVENT_JOB_EXECUTED, EVENT_JOB_MISSED, JobExecutionEvent
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import context, propagate
from watchfiles import BaseFilter, Change, PythonFilter, awatch

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt, Redaction, Signals
from rasm.runtime.resilience import RetryClass, guard

# --- [TYPES] ----------------------------------------------------------------------------

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger
type AdmitTag = Literal["bare", "keyed", "retried"]
type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]


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
    watched: tuple[tuple[str | PathLike[str], ...], BaseFilter | None, Callable[[set[tuple[Change, str]]], Block[Admit[T]]]] = case()

# --- [CONSTANTS] ------------------------------------------------------------------------

DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)
FIRE_MASK: Final[int] = EVENT_JOB_EXECUTED | EVENT_JOB_ERROR | EVENT_JOB_MISSED
FIRE_BUFFER: Final[int] = 64

# --- [MODELS] ---------------------------------------------------------------------------


class DrainReceipt[T](Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    values: Block[T] = Block.empty()
    cache: Map[ContentKey, T] = Map.empty()
    faults: Block[BoundaryFault] = Block.empty()
    hit: int = 0

    @staticmethod
    def of[U](accepted: int, hit: int, resolved: Block[tuple[Option[ContentKey], RuntimeRail[U]]], replayed: Block[tuple[ContentKey, U]], cache: Map[ContentKey, U]) -> "DrainReceipt[U]":
        merged = resolved.append(replayed.map(lambda pair: (Some(pair[0]), Ok(pair[1]))))
        completed = resolved.choose(lambda pair: pair[1].to_option())
        faults = resolved.choose(lambda pair: pair[1].swap().to_option())
        threaded = merged.fold(lambda acc, pair: pair[0].bind(lambda key: pair[1].to_option().map(lambda v: acc.add(key, v))).default_value(acc), cache)
        return DrainReceipt(accepted=accepted, completed=len(completed), cancelled=accepted - hit - len(resolved), rejected=len(faults), values=merged.choose(lambda pair: pair[1].to_option()), cache=threaded, faults=faults, hit=hit)


class AdmitRow[T](Struct, frozen=True):
    key: Callable[[Admit[T]], Option[ContentKey]]
    make: Callable[[Admit[T]], Work[T]]

# --- [SERVICES] -------------------------------------------------------------------------


# the one bounded slot allocator per lane identity; minted once and reused across every
# `drain`/`offload` on a policy so capacity is bounded over the lane's lifetime rather than
# reset per call (the frozen `LanePolicy` is hashable, so the policy is the memo key — a
# fresh `CapacityLimiter` per call would bound nothing across the lane, the deleted form).
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

    async def offload[T](self, kernel: Callable[..., T], *args: object) -> RuntimeRail[T]:
        carrier: dict[str, str] = {}
        propagate.inject(carrier)

        async def hop() -> T:
            with move_on_after(self.deadline.default_value(float("inf"))):
                return await interpreter_run_sync(traced_kernel, carrier, kernel, *args, limiter=self.limiter)
            raise TimeoutError("offload deadline elapsed")

        return await async_boundary("offload", hop)


class StagePlan(Struct, frozen=True):
    lane: LanePolicy
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]

    async def execute[T](self, work: Callable[[str, RetryClass], Sequence[Work[T]]]) -> tuple[DrainReceipt[T], ...]:
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
            units = Block.of_seq([Admit(retried=(classes[stage], fn)) for stage in front for fn in work(stage, classes[stage])])
            receipt = await self.lane.drain(units, carried)
            order.done(*front)
            carried, collected = receipt.cache, collected.append(Block.singleton(receipt))
        return tuple(collected)

# --- [OPERATIONS] -----------------------------------------------------------------------


def probe[T](row: AdmitRow[T], unit: Admit[T], cache: Map[ContentKey, T]) -> tuple[Option[ContentKey], Option[T], Work[T]]:
    key = row.key(unit)
    return key, key.bind(cache.try_find), row.make(unit)


def traced_kernel[T](carrier: dict[str, str], kernel: Callable[..., T], *args: object) -> T:
    token = context.attach(propagate.extract(carrier))
    try:
        return kernel(*args)
    finally:
        context.detach(token)


def _fire_seam(send: MemoryObjectSendStream[JobExecutionEvent]) -> Callable[[JobExecutionEvent], None]:
    def on_fire(event: JobExecutionEvent) -> None:
        with suppress(WouldBlock):
            send.send_nowait(event)

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
        case LaneSource(tag="watched", watched=(paths, watch_filter, build)):
            async for batch in awatch(*paths, watch_filter=Option.of_optional(watch_filter).default_value(PythonFilter())):
                yield build(batch)
        case LaneSource(tag="scheduled", scheduled=(trigger, build)):
            scheduler, (send, receive) = AsyncIOScheduler(), anyio.create_memory_object_stream[JobExecutionEvent](max_buffer_size=FIRE_BUFFER)
            scheduler.add_listener(_fire_seam(send), FIRE_MASK)
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
```

## [03]-[RESEARCH]

- [ADMISSION_ADT]: reflection-confirmed on the cp315 core (`apscheduler` 3.11.2, `anyio.to_interpreter`, `concurrent.interpreters` per PEP 734, `expression` 5.6.0 `@tagged_union`/`Block`/`Map`). The former three parallel admission methods (`run`/`cached`/`retried`) collapse into one `LanePolicy.drain` over a `Block[Admit[T]]` whose `bare`/`keyed`/`retried` cases each fold through one `ADMIT_TABLE` `AdmitRow` — the `@tagged_union` discriminant the `.api/expression.md` PUBLIC_TYPES [01]/tagged-union [01]-[03] surfaces own, `Map.of_seq`/`map[key]` the row lookup collection-ops ENTRYPOINTS [06]/[07], the CPU-kernel `offload` staying a distinct subinterpreter primitive sharing the lane's limiter and deadline rather than a fourth admission case — so the cache short-circuit (the `probe` operation resolves each `ADMIT_TABLE[unit.tag]` row once into a `(Option[ContentKey], Option[T], Work[T])` triple folding the `cache.try_find` probe in a single pass, and one `Block.partition` (collection-ops ENTRYPOINTS [03]) splits the `Some`-hit triples into `replayed` `hit`s reproducing the cached `Ok` value via `Option.map2` from the rest going live — never a double row lookup or a double `try_find` per unit) and the resilience-`guard` binding (the `retried` row's `make` wrapping the coroutine in `guard(cls)`) are admission cases rather than method bodies, and the lossless `DrainReceipt[T]` carries `values: Block[T]` (both replayed and resolved oks) and the threaded `cache: Map[ContentKey, T]` rather than discarding the recovered `Ok` values. The receipt threads the typed `Block[BoundaryFault]` and the `Block[T]` values through `Block.of_seq`/`Block.choose`/`Block.fold` (collection-ops ENTRYPOINTS [01]/[03]/[04]) over each lane's `RuntimeRail[T]` outcome, splitting Oks via `Result.to_option` and faults via `Result.swap().to_option` exactly as `reliability/faults#traversed` accumulates, so a rejected unit's fault stays structurally addressable and a completed unit's value survives rather than collapsing to a scalar tally. `anyio.to_interpreter.run_sync` rides a runnable `concurrent.interpreters` on the cp315 core; were a given cp315 build to ship without it, the offload degrades to the confirmed `anyio.to_thread.run_sync` (ENTRYPOINTS [01]) path with a GIL caveat rather than blocking the leg.

- [DEADLINE_CONTAINMENT]: reflection-confirmed against `.api/anyio.md` cancel-scope ENTRYPOINTS [01]/[02] — the bounded drain runs inside `move_on_after(delay, shield=False)` (ENTRYPOINTS [02], silently cancels on deadline, `scope.cancelled_caught` readable) rather than `fail_after` (ENTRYPOINTS [01], raises `TimeoutError`), so a deadline trip cancels the in-flight `start_soon` children inside the structured task group (the children completing-before-block-exit invariant of [ANYIO_TOPOLOGY]) and the receipt reports them as `cancelled = accepted - hit - len(resolved)` with the partial `values`/`faults` intact — a bounded lane never escapes a raw `TimeoutError`/`BaseExceptionGroup` without a `DrainReceipt`, the `Nothing` deadline mapping to `float("inf")` so an unbounded lane drains every unit. The deadline-containment shape is settled.

- [SCHEDULER_FIRE_SEAM]: reflection-confirmed against `.api/apscheduler.md` `STACK_LAW`/`[SCHEDULER_TOPOLOGY]` — a `scheduled` `LaneSource` case registers its `Trigger` on one `schedulers.asyncio.AsyncIOScheduler` (PUBLIC_TYPES [03], asyncio-loop integration, the responsive-service scheduler the `BlockingScheduler` ban requires) running on the same loop the anyio lane owns, and `add_listener(callback, EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED)` (ENTRYPOINTS infrastructure [05], event codes [08]) is the single fire seam — the `STACK_LAW` "single seam where the OTel span / receipt fact for a run is emitted, reading `JobExecutionEvent` fields, not a wrapper around the job function" — pushing each `events.JobExecutionEvent` (PUBLIC_TYPES [05], carrying `scheduled_run_time`/`retval`/`exception`) into the drain through a memory stream. This replaces the hand-rolled `Trigger.get_next_fire_time` + `anyio.sleep` loop, the precise "hand-rolled `threading.Timer`/`asyncio.call_later` recurring loop" the `RAIL_LAW` rejects: cron, interval, and one-off all resolve through the `apscheduler` `Trigger` union on the one scheduler, `CronTrigger.from_crontab` (ENTRYPOINTS trigger [02]) the sole cron-string intake, the scheduler owning the inter-fire timing rather than a bespoke sleep. The listener is the named `_fire_seam(send)` closure, not an inline `lambda` doing a bare `send.send_nowait`: the `apscheduler` listener runs synchronously on the scheduler loop and a bounded `create_memory_object_stream(max_buffer_size=FIRE_BUFFER)` raises `anyio.WouldBlock` from `send_nowait` once a slow consumer fills the buffer, so the seam wraps the non-blocking send in `contextlib.suppress(WouldBlock)` — a backpressured overflow is the scheduler's own `coalesce`/`misfire_grace_time` missed-fire policy (`[STACK_LAW]` orthogonality) dropping the redundant fire rather than a raise propagating into `apscheduler`'s dispatch and silently breaking the listener registration. The fire-seam spellings, including the bounded-buffer backpressure containment, are settled.

- [OFFLOAD_TRACE_STITCH]: reflection-confirmed against `.api/opentelemetry-api.md`. The inject side runs in the main interpreter: `propagate.inject(carrier)` (context/propagation ENTRYPOINTS [12]) writes the active W3C `traceparent`/`tracestate` into a plain `dict[str, str]` carrier resolved through the `observability/telemetry#TELEMETRY`-installed composite `propagate.get_global_textmap` (ENTRYPOINTS [13]) before the hop, so the W3C parent is the one cross-boundary fact the no-pickle PEP 734 carrier moves. The module-level `traced_kernel` shim runs `propagate.extract(carrier)` (ENTRYPOINTS [11])+`context.attach` (ENTRYPOINTS [02]) inside the worker (`context.detach` ENTRYPOINTS [03] token-paired in `finally`) before invoking the kernel. The parent attaches only where the worker's `opentelemetry` module state carries the composite propagator: a PEP 734 subinterpreter holds independent module globals, so a worker that has not run the `observability/telemetry#TELEMETRY` install resolves the default `propagate` text-map and `extract` yields an empty `Context`, leaving the kernel unparented while the `traceparent` survives on the carrier for any span the kernel itself opens. The stitch is therefore best-effort across the offload hop — the inject side is sound, the extract side parents only an install-carrying worker — and never re-installs the propagator. The same `propagate.extract`+`context.attach` pair backs the `observability/receipts#RECEIPT` `Signals.continue_inbound`/`Signals.attach` inbound gRPC leg, where the in-process receiver already carries the installed propagator, so that leg parents unconditionally and this offload leg parents on the install gate. Realizes the `ONE_DISTRIBUTED_TRACE` offload contribution. Spellings settled.

- [CROSS_CUT_ASPECTS]: reflection-confirmed against `.api/expression.md`, the sibling `reliability/resilience#RESILIENCE`, `observability/metrics#METRIC`, and `observability/receipts#RECEIPT` owners — the `retried` admission case binds `resilience#guard(cls)` (the `functools.cache`-memoised public entry minting the reusable `stamina.BoundAsyncRetryingCaller` once per `RetryClass`, no thin `_caller` forwarder) around the unit coroutine so retry rides one `stamina` policy row as a drain aspect rather than a `stamina.retry_context` block hand-opened at every call site, and the `@drained(owner, redaction)` decorator (`functools.wraps`-preserving) folds the post-drain `DrainReceipt` into the `Metrics.observe(receipt, in_flight=receipt.cancelled)` atomic-swap snapshot and the `Signals.emit(Receipt.of(owner, receipt), redaction)` log row in one egress — `Receipt.of` discriminating the `DrainReceipt` into the receipts owner's `drained` case carrying `(owner, DrainReceipt)` rather than the call site hand-building `Receipt(drained=...)`, the `Metrics.observe` reading the same five `DRAIN_COLUMNS` through one per-column `getattr(receipt, column)` off the typed struct (the `_drain_fold` shape both egress siblings hold — never a full-struct `msgspec.structs.asdict` that allocates the receipt's `values`/`cache`/`faults` containers on every export-cycle callback only to drop them), so telemetry and receipt egress are composed onto the drain rather than re-threaded by each caller, the cross-cutting retry/telemetry/receipt concerns owned as aspects per the AOP collapse. `feed` wraps `policy.drain` in the same `drained(owner, redaction)` aspect once and threads it over the `_events(source)` projector, so a `scheduled`/`watched` source is observed by composition through the one shared tail — neither source re-implements the metric/receipt egress, and the `_events`/`feed` split keeps the source `match` (the cancel-scope-and-capacity setup the lane owns) orthogonal to the observed-drain tail. Settled.

- [STAGE_FRONT_CONCURRENCY]: reflection-confirmed against the stdlib `graphlib.TopologicalSorter` active interface — `prepare()`/`is_active()`/`get_ready()`/`done(*nodes)` exposes each dependency-level front as the `get_ready()` tuple, so `StagePlan.execute` runs every same-level stage's flattened work concurrently under one `LanePolicy.drain` per front (the units entering as `retried` `Admit` cases carrying the stage's `RetryClass`) rather than the serial `static_order()` walk, one `DrainReceipt` per front, the front-internal capacity still bounded by the lane's one `CapacityLimiter`. The DAG walk is the one `while order.is_active()` boundary loop over that stateful `graphlib` driver (the sanctioned interop form for a `prepare`/`get_ready`/`done` protocol, depth in fronts not nodes so no async-recursion stack growth), rebinding `(carried, collected)` as immutable `Map`/`Block` loop-carried state — never a mutated accumulator — and threading each front's `DrainReceipt.cache` forward as the next front's `drain` cache argument, so a `keyed` unit a downstream stage re-admits replays the upstream `Ok` from the threaded `Map[ContentKey, T]` rather than recomputing — the content cache spans the whole DAG, not one front. Settled.

- [WATCH_FILTER]: reflection-confirmed against `.api/watchfiles.md` `[02]-[PUBLIC_TYPES]`/`[03]-[ENTRYPOINTS]` — `watchfiles.PythonFilter` (PUBLIC_TYPES [03], the Python-source change filter) narrows the `awatch(*paths, watch_filter=...)` (ENTRYPOINTS [02]) change stream so only Python-source `Change` events feed the lane, the `BaseFilter` parameter accepting any caller filter; the `watched` `LaneSource` case iterates the `set[tuple[Change, str]]` batch and the consumer matches on the `Change` enum per the `.api` classification law, never a `stat` polling loop or per-event path-string filter. Settled.

No open RESEARCH seam remains on this page.
