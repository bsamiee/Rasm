# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy.drain` is the one polymorphic bounded-drain owner: it opens a single `anyio` task group under one `CapacityLimiter` and one `move_on_after` deadline scope, admits a `Block[Admit[T]]` whose three cases — a bare `Work[T]`, a content-keyed `(ContentKey, Work[T])` cache unit, a resilience-guarded `(RetryClass, Work[T])` retried unit — fold through one `ADMIT_TABLE` row each into the same lane coroutine, and returns one parameterized `DrainReceipt[T]` carrying the collected `Block[T]` values, the threaded session cache, the typed `Block[BoundaryFault]`, and the five-column outcome tally. `StagePlan.execute` drives a multi-stage DAG over that same drain, each `graphlib.TopologicalSorter` dependency-level front running concurrently rather than serialized. `LaneSource` is the one feeder union — a `scheduled` `apscheduler` `AsyncIOScheduler`-driven fire, a `watched` `PythonFilter`-narrowed `awatch` change batch — both projecting their source event into one `Block[Admit[T]]` that drains through one `feed` entry yielding one `DrainReceipt[T]` per batch, never a second scheduler surface, cron owned solely by `apscheduler`. CPU-bound kernels offload into per-subinterpreter execution under PEP 734 with the active OTel trace context stitched across the offload hop, content-keyed work short-circuits on a cache hit, the resilience `guard` rides each unit's `RetryClass` as a drain aspect, and the `Metrics.observe`/`Receipt.drained` egress rides one `drained` aspect — never inline at the call site. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive.

## [01]-[INDEX]

- [01]-[LANE]: the one `LanePolicy.drain` bounded task group, the `Admit[T]` three-arm admission ADT folded by `ADMIT_TABLE` (bare/keyed/retried), the parameterized `DrainReceipt[T]` carrying values + cache + faults + tally, the deadline-as-cancelled fault containment, the CPU-bound subinterpreter offload with trace-context stitching, the content-keyed cache short-circuit, the `guard`/`drained` cross-cutting aspects, the concurrent-front `StagePlan` DAG, the `LaneSource` `scheduled`/`watched` feeder union.

## [02]-[LANE]

- Owner: `LanePolicy` — the one bounded `anyio`-task-group drain with capacity and a cancellation scope, its `limiter` property resolving the `functools.cache`-memoised `_limiter(self)` so one `CapacityLimiter` is minted per frozen-hashable lane identity and shared across the lane's `drain` and `offload` rather than re-minted per call; `Admit[T]` the closed `@tagged_union` admission shape with `bare`/`keyed`/`retried` cases so one drain discriminates a plain coroutine, a `(ContentKey, Work[T])` cache unit, and a `(RetryClass, Work[T])` resilience-guarded unit by case rather than three parallel methods; `ADMIT_TABLE` the `Map[str, AdmitRow]` of one behavior row per `Admit` tag — each row a `key` projection (the `Option[ContentKey]` the drain probes the session cache with, `Nothing` for the un-keyed `bare`/`retried` cases) and a `make` projection (the `Work[T]` coroutine, raw for `bare`/`keyed` and `guard(cls)`-wrapped for `retried`) — so a new admission modality is one row, not a new method; `DrainReceipt[T]` the one parameterized outcome carrying `values: Block[T]`, `cache: Map[ContentKey, T]`, the typed `faults: Block[BoundaryFault]`, and the `accepted`/`completed`/`cancelled`/`rejected`/`hit` counts — never a bare scalar tally and never a count-only return that drops the computed values; `DrainOutcome`/`DRAIN_COLUMNS` the one canonical five-column outcome taxonomy this page owns — the `DrainOutcome` literal is the bounded vocabulary and `DRAIN_COLUMNS` derives from it through `get_args` so the column set is one typed fact, and the sibling `observability/metrics#METRIC` `lane.drained` counter and `observability/receipts#RECEIPT` drained emit import this taxonomy rather than redeclaring it, so the counter outcome dimension and the receipt column set can never drift; `StagePlan` the multi-stage DAG (stage edges, per-stage `RetryClass`, partial re-run) over the same drain; `LaneSource` the closed `@tagged_union` `scheduled`/`watched` feeder union, not a separate scheduler.
- Entry: `LanePolicy.drain` is the one polymorphic bounded drain over `Block[Admit[T]]` — it opens one `anyio.create_task_group` under one `CapacityLimiter` and the `move_on_after` deadline scope read from `execution/admission#CONTEXT`, folds each `Admit` case through its `ADMIT_TABLE` row (a `keyed` unit whose `ContentKey` already carries an `Ok` in the threaded session cache short-circuits without invoking the coroutine and increments `hit`; a `retried` unit binds `reliability/resilience#RESILIENCE` `guard(cls)` around the coroutine so transient faults retry under the typed policy row before the rail resolves; a `bare` unit runs directly), sends each resolved `RuntimeRail[T]` over one memory-object stream, and returns one `DrainReceipt[T]`; `LanePolicy.offload` routes a caller-supplied CPU kernel into per-subinterpreter execution through `anyio.to_interpreter.run_sync` under the same `CapacityLimiter`, never importing the kernel, injecting the active OTel context into a carrier the module-level `traced_kernel` shim extracts-and-attaches so the offloaded span seeds from the calling span, lifting a `BrokenWorkerInterpreter` through the one `reliability/faults#FAULT` `async_boundary`; `StagePlan.execute` drives the stage DAG through `graphlib.TopologicalSorter` in active `prepare`/`get_ready`/`done` mode so each dependency-level front runs concurrently under one `LanePolicy.drain` over the front's flattened `Admit` units, threading each stage's `RetryClass` into the admission so the units enter as `retried` cases, one `DrainReceipt` per front; `feed` runs the union's source by `match` with an `assert_never` tail — a `scheduled` source registers its `Trigger` on the one `AsyncIOScheduler` whose `EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED` listener is the single fire seam pushing each `JobExecutionEvent` over a memory stream, a `watched` source iterates the `PythonFilter`-narrowed `awatch` stream — both projecting the source event through the case's `build` callable into one `Block[Admit[T]]` drained per batch.
- Auto: cancellation rides the `move_on_after` scope; capacity rides one `CapacityLimiter` minted once per lane identity and shared across every `drain` and `offload` — the `LanePolicy.limiter` property reads the `functools.cache`-memoised `_limiter(self)` keyed on the frozen-hashable policy, so two policies with identical `capacity`/`deadline` share one bound and a single lane caps both its concurrent `drain` units and its `offload` subinterpreter hops on one slot allocator rather than a fresh per-call limiter that bounds nothing across the lane's lifetime; a tripped deadline is contained — the task-group drain runs inside `move_on_after` (not a bare `fail_after` that escapes as a raw `TimeoutError`/`BaseExceptionGroup`), so a deadline trip cancels the in-flight units and the receipt reports them as `cancelled = accepted - hit - len(resolved)` (the went-live cardinality minus the cardinality that reached the stream) with the partial `values`/`faults` intact, never an exception escaping a bounded lane without a receipt; the `feed` source discrimination is one `match` with an `assert_never` tail so the arm set is total over the closed `LaneSource` union; a `scheduled` fire resolves cron, interval, and one-off through the `apscheduler` `Trigger` union on the one `AsyncIOScheduler`, cron owned solely by `apscheduler` `CronTrigger`/`from_crontab` — no `croniter`, no `aiocron`, no hand-rolled `anyio.sleep` cron loop.
- Auto: each lane unit sends its full `RuntimeRail[T]` outcome over the memory-object stream rather than a pre-collapsed bool, so the typed `BoundaryFault` the `reliability/faults#FAULT` rail mints survives into the receipt — `DrainReceipt.of` splits the resolved rails with `Block.choose(rail.swap().to_option())` into the `faults` `Block[BoundaryFault]` and recovers the `Ok` values with `Block.choose(rail.to_option())` into the `values` `Block[T]`, mirroring the `faults#traversed` accumulate fold so a drained lane surfaces both which units failed and the values that succeeded; `rejected` is the cardinality of that typed `Block`, never a fault-erasing count, and `completed` is the cardinality of the recovered values, so the receipt is lossless in both directions.
- Auto: the content-keyed admission folds the `ContentKey` from `evidence/identity#IDENTITY` so a settings change misses correctly — the cache is an `expression` `Map[ContentKey, T]` threaded immutably across one session on the `DrainReceipt.cache` field, a hit reproduces the exact `Ok` value and increments `hit` distinct from `completed`, only an `Ok` outcome folds back into the `Map` (`pair.to_option().map(cache.add).default_value(cache)`) and an `Error` re-runs while its fault accumulates, and the `Map` carries the most expensive offline work (re-tessellation, IFC evaluation, scan registration) by reference; the cache is session-local in-memory, never a durable store — durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the CPU-bound offload uses `anyio.to_interpreter.run_sync(traced_kernel, carrier, kernel, *args, limiter=self.limiter)` under the same memoised per-lane `CapacityLimiter` the drain bounds with, so each subinterpreter carries its own GIL under PEP 734 and heavy geometry work never stalls the companion event loop or contends the main GIL, with no pickle round-trip on the subinterpreter path; the lane injects the active context into a plain `dict[str, str]` carrier through `propagate.inject` before the hop and the module-level `traced_kernel` shim runs `propagate.extract`+`context.attach` inside the worker before invoking the caller kernel and `context.detach` in a `finally`, so the offloaded work runs under the calling span across the process/subinterpreter boundary and a flame graph stitches the offload leg to its parent rather than rooting a fresh trace; the kernel is a caller-supplied `Callable` the lane never imports, and a `BrokenWorkerInterpreter` raised inside the worker crosses the one `reliability/faults#FAULT` `async_boundary` conversion rather than a bare raise — the lane offloads, it never owns the geometry/scan numeric loops the `libs/python` siblings own.
- Auto: cross-cutting concerns ride aspects, not inline call-site code — `retried` admission binds `reliability/resilience#RESILIENCE` `guard(cls)` as a per-unit retry aspect woven into the lane coroutine so retry is one admission case rather than caller boilerplate around every coroutine; the `drained` aspect (the `@drained(owner, redaction)` decorator, `functools.wraps`-preserving) wraps a drain so its `DrainReceipt` feeds the `observability/metrics#METRIC` `Metrics.observe` atomic-swap and emits the `observability/receipts#RECEIPT` `Receipt(drained=...)` row in one egress, so a lane is observed by composition rather than each caller re-threading the receipt to the metric spine and the log stream; the `outcome` attribute the metric counter and histogram key by is the shared `DrainOutcome` taxonomy folded from the `DrainReceipt` columns, never restamped per call.
- Auto: the watch source is the `watched` `LaneSource` case wrapping `watchfiles.awatch(*paths, watch_filter=watch_filter)` defaulting to `watchfiles.PythonFilter()` so only Python-source `Change` events feed the lane, the consumer matching on the `Change` enum rather than string-comparing event kinds and never running a `stat` polling loop; each change batch is projected through the case's `build` callable into one `Block[Admit[T]]` entering the same drain, the source owning the cancel scope and capacity while `watchfiles` owns the change stream.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`CancelScope`/`move_on_after`/`fail_after`/`create_memory_object_stream`/`to_interpreter.run_sync`/`BrokenWorkerInterpreter`), `watchfiles` (`awatch`/`PythonFilter`/`Change`), `apscheduler` (`schedulers.asyncio.AsyncIOScheduler`/`executors.asyncio.AsyncIOExecutor`/`triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`CronTrigger.from_crontab`/`add_listener`/`events.JobExecutionEvent`/`events.EVENT_JOB_EXECUTED`/`EVENT_JOB_ERROR`/`EVENT_JOB_MISSED`), `opentelemetry-api` (`propagate.inject`/`propagate.extract`/`context.attach`/`context.detach` for the offload context stitch), `expression` (`Map`/`Block`/`Option`/`tagged_union`/`case`/`tag`/`Result.swap`/`to_option`), `graphlib` (stdlib `TopologicalSorter` driven in active `prepare`/`get_ready`/`done` mode for concurrent same-level fronts), `functools` (`cache` memoising the per-lane-identity `CapacityLimiter`, `wraps` preserving the `drained` aspect signature), `msgspec` (`Struct`/`structs.asdict` for the receipt fold the metric/receipt egress reads).
- Growth: a new lane source is one `LaneSource` case with its `build` projection plus one `feed` match arm; a new admission modality is one `Admit` case plus one `ADMIT_TABLE` row, never a new `LanePolicy` method; a new trigger modality is one `apscheduler` `Trigger` row on a `scheduled` source; a new stage is one edge on `StagePlan`; a CPU kernel is one `offload` callable carried across the context stitch unchanged; a cached unit is one `keyed` `Admit`; a retry-guarded unit is one `retried` `Admit`; a new watch filter is one `BaseFilter` on a `watched` source; a new drain outcome dimension is one member on the `DrainOutcome` literal plus its `DrainReceipt` field, reaching the metrics counter and the receipt emit through the imported `DRAIN_COLUMNS` by that one add; zero new surface.
- Boundary: no daemon scheduler beside the one `AsyncIOScheduler` `StagePlan` shares, second cron owner beside `apscheduler`, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation, a second scheduler surface, a process-pool serialization tax on a CPU kernel a subinterpreter isolates, a durable lane cache, a kernel the lane imports rather than receives, a serial stage walk where a same-level front could run concurrently, a fresh-root span on the offloaded leg where the calling span is the parent, a `croniter`/`aiocron`/hand-rolled `anyio.sleep` cron replacement beside the `apscheduler` `Trigger` union driven on the one scheduler, a per-event path-string filter where `PythonFilter` already narrows the change stream, three parallel lane methods (`run`/`cached`/`offload`) where one `drain` discriminates the admission shape, four divergent return shapes where one parameterized `DrainReceipt[T]` carries values/cache/faults/tally, a count-only drain that discards the computed `Ok` values, a bare `fail_after` whose `TimeoutError` escapes a bounded lane without a receipt, a fresh `CapacityLimiter(self.capacity)` minted per `drain`/`offload` call that bounds nothing across the lane where the memoised per-identity `_limiter` is the one slot allocator, and inline call-site retry/telemetry/receipt threading where the `guard`/`drained` aspects own the cross-cut are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from functools import cache, wraps
from graphlib import TopologicalSorter
from os import PathLike
from typing import Final, Literal, assert_never, get_args

import anyio
from anyio import CapacityLimiter, move_on_after
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
type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]


@tagged_union(frozen=True)
class Admit[T]:
    tag: Literal["bare", "keyed", "retried"] = tag()
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
        admitted = units.map(lambda unit: (ADMIT_TABLE[unit.tag].key(unit), ADMIT_TABLE[unit.tag].make(unit)))
        replayed = admitted.choose(lambda row: row[0].bind(lambda key: cache.try_find(key).map(lambda value: (key, value))))
        live = admitted.choose(lambda row: Nothing if row[0].bind(cache.try_find).is_some() else Some(row))

        async def lane(key: Option[ContentKey], fn: Work[T], sink: MemoryObjectSendStream[tuple[Option[ContentKey], RuntimeRail[T]]]) -> None:
            async with sink, limiter:
                await sink.send((key, await fn()))

        with move_on_after(self.deadline.default_value(float("inf"))):
            async with anyio.create_task_group() as group, send:
                for key, fn in live:
                    group.start_soon(lane, key, fn, send.clone())
        resolved = Block.of_seq([item async for item in receive])
        return DrainReceipt.of(len(units), len(replayed), resolved, replayed, cache)

    async def offload[T](self, kernel: Callable[..., T], *args: object) -> RuntimeRail[T]:
        carrier: dict[str, str] = {}
        propagate.inject(carrier)
        return await async_boundary("offload", lambda: interpreter_run_sync(traced_kernel, carrier, kernel, *args, limiter=self.limiter))


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
        fronts: list[DrainReceipt[T]] = []
        while order.is_active():
            front = order.get_ready()
            units = Block.of_seq([Admit(retried=(classes[stage], fn)) for stage in front for fn in work(stage, classes[stage])])
            fronts.append(await self.lane.drain(units))
            order.done(*front)
        return tuple(fronts)

# --- [OPERATIONS] -----------------------------------------------------------------------


def traced_kernel[T](carrier: dict[str, str], kernel: Callable[..., T], *args: object) -> T:
    token = context.attach(propagate.extract(carrier))
    try:
        return kernel(*args)
    finally:
        context.detach(token)


def drained[**P, T](owner: str, redaction: Redaction) -> Callable[[Callable[P, Awaitable[DrainReceipt[T]]]], Callable[P, Awaitable[DrainReceipt[T]]]]:
    def aspect(fn: Callable[P, Awaitable[DrainReceipt[T]]]) -> Callable[P, Awaitable[DrainReceipt[T]]]:
        @wraps(fn)
        async def observed(*args: P.args, **kwargs: P.kwargs) -> DrainReceipt[T]:
            receipt = await fn(*args, **kwargs)
            Metrics.observe(receipt)
            Signals.emit(Receipt(drained=(owner, receipt)), redaction)
            return receipt
        return observed
    return aspect


async def feed[T](policy: LanePolicy, source: LaneSource[T]) -> AsyncIterator[DrainReceipt[T]]:
    match source:
        case LaneSource(tag="watched", watched=(paths, watch_filter, build)):
            async for batch in awatch(*paths, watch_filter=watch_filter if watch_filter is not None else PythonFilter()):
                yield await policy.drain(build(batch))
        case LaneSource(tag="scheduled", scheduled=(trigger, build)):
            scheduler, send, receive = AsyncIOScheduler(), *anyio.create_memory_object_stream[JobExecutionEvent](max_buffer_size=64)
            scheduler.add_listener(lambda event: send.send_nowait(event), FIRE_MASK)
            scheduler.add_job(lambda: None, trigger=trigger)
            scheduler.start()
            try:
                async for event in receive:
                    yield await policy.drain(build(event))
            finally:
                scheduler.shutdown(wait=False)
        case _ as unreachable:
            assert_never(unreachable)

# --- [COMPOSITION] ----------------------------------------------------------------------

ADMIT_TABLE: Final[Map[str, AdmitRow]] = Map.of_seq([
    ("bare", AdmitRow(key=lambda _: Nothing, make=lambda unit: unit.bare)),
    ("keyed", AdmitRow(key=lambda unit: Some(unit.keyed[0]), make=lambda unit: unit.keyed[1])),
    ("retried", AdmitRow(key=lambda _: Nothing, make=lambda unit: lambda: guard(unit.retried[0])(unit.retried[1]))),
])
```

## [03]-[RESEARCH]

[ADMISSION_ADT], [BOUNDED_DRAIN], [INTERPRETER_OFFLOAD], and [CONTENT_LANE_CACHE] are reflection-confirmed on the cp315 core (`apscheduler` 3.11.2, `anyio.to_interpreter`, `concurrent.interpreters` per PEP 734, `expression` 5.6.0 `@tagged_union`/`Block`/`Map`): the former three parallel lane methods (`run`/`cached`/`offload`) collapse into one `LanePolicy.drain` over a `Block[Admit[T]]` whose `bare`/`keyed`/`retried` cases each fold through one `ADMIT_TABLE` `AdmitRow` (the `@tagged_union` discriminant the `.api/expression.md` PUBLIC_TYPES [01]/tagged-union [01]-[03] surfaces own, `Map.of_seq`/`map[key]` the row lookup collection-ops ENTRYPOINTS [06]/[07]), so the cache short-circuit (`drain` partitions `keyed` units whose `cache.try_find(key)` is `Some` into `replayed` `hit`s reproducing the cached `Ok` value, the rest going live) and the resilience-`guard` binding (the `retried` row's `make` wrapping the coroutine in `guard(cls)`) are admission cases rather than method bodies, and the lossless `DrainReceipt[T]` carries `values: Block[T]` (both replayed and resolved oks) and the threaded `cache: Map[ContentKey, T]` rather than discarding the recovered `Ok` values. The receipt threads the typed `Block[BoundaryFault]` and the `Block[T]` values through `Block.of_seq`/`Block.choose`/`Block.fold` (collection-ops ENTRYPOINTS [01]/[03]/[04]) over each lane's `RuntimeRail[T]` outcome, splitting Oks via `Result.to_option` and faults via `Result.swap().to_option` exactly as `reliability/faults#traversed` accumulates, so a rejected unit's fault stays structurally addressable and a completed unit's value survives rather than collapsing to a scalar tally. `anyio.to_interpreter.run_sync` rides a runnable `concurrent.interpreters` on the cp315 core; were a given cp315 build to ship without it, the offload degrades to the confirmed `anyio.to_thread.run_sync` (ENTRYPOINTS [01]) path with a GIL caveat rather than blocking the leg.

- [DEADLINE_CONTAINMENT]: reflection-confirmed against `.api/anyio.md` cancel-scope ENTRYPOINTS [01]/[02] — the bounded drain runs inside `move_on_after(delay, shield=False)` (ENTRYPOINTS [02], silently cancels on deadline, `scope.cancelled_caught` readable) rather than `fail_after` (ENTRYPOINTS [01], raises `TimeoutError`), so a deadline trip cancels the in-flight `start_soon` children inside the structured task group (the children completing-before-block-exit invariant of [ANYIO_TOPOLOGY]) and the receipt reports them as `cancelled = accepted - hit - len(resolved)` with the partial `values`/`faults` intact — a bounded lane never escapes a raw `TimeoutError`/`BaseExceptionGroup` without a `DrainReceipt`, the `Nothing` deadline mapping to `float("inf")` so an unbounded lane drains every unit. The deadline-containment shape is settled.

- [SCHEDULER_FIRE_SEAM]: reflection-confirmed against `.api/apscheduler.md` `STACK_LAW`/`[SCHEDULER_TOPOLOGY]` — a `scheduled` `LaneSource` case registers its `Trigger` on one `schedulers.asyncio.AsyncIOScheduler` (PUBLIC_TYPES [03], asyncio-loop integration, the responsive-service scheduler the `BlockingScheduler` ban requires) running on the same loop the anyio lane owns, and `add_listener(callback, EVENT_JOB_EXECUTED|EVENT_JOB_ERROR|EVENT_JOB_MISSED)` (ENTRYPOINTS infrastructure [05], event codes [08]) is the single fire seam — the `STACK_LAW` "single seam where the OTel span / receipt fact for a run is emitted, reading `JobExecutionEvent` fields, not a wrapper around the job function" — pushing each `events.JobExecutionEvent` (PUBLIC_TYPES [05], carrying `scheduled_run_time`/`retval`/`exception`) into the drain through a memory stream. This replaces the hand-rolled `Trigger.get_next_fire_time` + `anyio.sleep` loop, the precise "hand-rolled `threading.Timer`/`asyncio.call_later` recurring loop" the `RAIL_LAW` rejects: cron, interval, and one-off all resolve through the `apscheduler` `Trigger` union on the one scheduler, `CronTrigger.from_crontab` (ENTRYPOINTS trigger [02]) the sole cron-string intake, the scheduler owning the inter-fire timing rather than a bespoke sleep. The fire-seam spellings are settled.

- [OFFLOAD_TRACE_STITCH]: reflection-confirmed against `.api/opentelemetry-api.md` — `propagate.inject(carrier, context)` (context/propagation ENTRYPOINTS [12]) writes the active W3C `traceparent`/`tracestate` into a plain `dict[str, str]` carrier resolved through the `observability/telemetry#TELEMETRY`-installed composite `propagate.get_global_textmap` (ENTRYPOINTS [13]), and the module-level `traced_kernel` shim runs `propagate.extract(carrier, context)` (ENTRYPOINTS [11])+`context.attach` (ENTRYPOINTS [02]) inside the subinterpreter/process worker (`context.detach` ENTRYPOINTS [03] token-paired in `finally`) before invoking the caller kernel, so the offloaded span seeds from the calling span across the PEP 734 hop rather than a fresh root — the same `Context` extract-and-attach pair `observability/receipts#RECEIPT` `Signals.continue_inbound`/`Signals.attach` uses on the inbound gRPC leg (which cites the same `.api/opentelemetry-api.md` indices), realizing the `ONE_DISTRIBUTED_TRACE` offload leg. The carrier crosses the no-pickle subinterpreter boundary as a plain `dict[str, str]`, and `propagate.inject`/`extract` resolve the no-op propagator before the `observability/telemetry#TELEMETRY` install, so an un-installed profile carries an empty carrier and the kernel runs unparented — the mechanical reason this composes with, never re-installs, the propagator. Spellings settled.

- [CROSS_CUT_ASPECTS]: reflection-confirmed against `.api/expression.md`, the sibling `reliability/resilience#RESILIENCE`, `observability/metrics#METRIC`, and `observability/receipts#RECEIPT` owners — the `retried` admission case binds `resilience#guard(cls)` (the `Map[RetryClass, BoundAsyncRetryingCaller]` lookup resolved once into `BOUND`) around the unit coroutine so retry rides one `stamina` policy row as a drain aspect rather than a `stamina.retry_context` block hand-opened at every call site, and the `@drained(owner, redaction)` decorator (`functools.wraps`-preserving) folds the post-drain `DrainReceipt` into the `Metrics.observe` atomic-swap snapshot and the `Signals.emit(Receipt(drained=...))` log row in one egress — the `Receipt` `drained` case carrying `(owner, DrainReceipt)` per the receipts owner, the `Metrics.observe` reading the same five `DRAIN_COLUMNS` through `msgspec.structs.asdict`, so telemetry and receipt egress are composed onto the drain rather than re-threaded by each caller, the cross-cutting retry/telemetry/receipt concerns owned as aspects per the AOP collapse. Settled.

- [STAGE_FRONT_CONCURRENCY]: reflection-confirmed against the stdlib `graphlib.TopologicalSorter` active interface — `prepare()`/`is_active()`/`get_ready()`/`done(*nodes)` exposes each dependency-level front as the `get_ready()` tuple, so `StagePlan.execute` runs every same-level stage's flattened work concurrently under one `LanePolicy.drain` per front (the units entering as `retried` `Admit` cases carrying the stage's `RetryClass`) rather than the serial `static_order()` walk, one `DrainReceipt` per front, the front-internal capacity still bounded by the lane's one `CapacityLimiter`. Settled.

- [WATCH_FILTER]: reflection-confirmed against `.api/watchfiles.md` `[02]-[PUBLIC_TYPES]`/`[03]-[ENTRYPOINTS]` — `watchfiles.PythonFilter` (PUBLIC_TYPES [03], the Python-source change filter) narrows the `awatch(*paths, watch_filter=...)` (ENTRYPOINTS [02]) change stream so only Python-source `Change` events feed the lane, the `BaseFilter` parameter accepting any caller filter; the `watched` `LaneSource` case iterates the `set[tuple[Change, str]]` batch and the consumer matches on the `Change` enum per the `.api` classification law, never a `stat` polling loop or per-event path-string filter. Settled.

No open RESEARCH seam remains on this page.
