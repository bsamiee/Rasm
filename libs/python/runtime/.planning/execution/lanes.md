# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy` owns bounded `anyio` task groups with capacity and cancellation scopes, returning a `DrainReceipt`; `StagePlan` owns multi-stage DAG orchestration over the same task-group spine, dependency-level fronts running concurrently rather than serialized. Watch-triggered and scheduled work enter the same bounded task group through `watchfiles` (a `PythonFilter`-narrowed `awatch` change stream) and the `apscheduler` `Trigger` union, draining through the same receipt — never a second scheduler surface, cron owned solely by `apscheduler`. The one lane admits I/O-bound coroutines, CPU-bound kernels offloaded into per-subinterpreter execution under PEP 734 with the active OTel trace context stitched across the offload hop, and content-keyed work that short-circuits on a cache hit, all under one `CapacityLimiter` and one `DrainReceipt`. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive.

## [01]-[INDEX]

- [01]-[LANE]: bounded anyio task groups, the CPU-bound subinterpreter offload with trace-context stitching, the content-keyed cache short-circuit, drain receipts, the concurrent-front stage DAG, the `PythonFilter`-narrowed watch feeder, the `apscheduler` `Trigger`-fire feeder.

## [02]-[LANE]

- Owner: `LanePolicy` — bounded `anyio` task groups with capacity and cancellation scopes; `DrainReceipt` the `accepted`/`completed`/`cancelled`/`rejected`/`hit` counts plus the typed `Block[BoundaryFault]` carrying which rejected units failed and why, never a bare scalar tally; `DrainOutcome`/`DRAIN_COLUMNS` the one canonical five-column outcome taxonomy this page owns — the `DrainOutcome` literal is the bounded vocabulary and `DRAIN_COLUMNS` derives from it through `get_args` so the column set is one typed fact, and the sibling `observability/metrics#METRIC` `lane.drained` counter and `observability/receipts#RECEIPT` drained emit import this taxonomy rather than redeclaring it, so the counter outcome dimension and the receipt column set can never drift; `StagePlan` the multi-stage DAG (stage edges, per-stage retry, partial re-run) over the same task-group spine; `watchfiles` and the `apscheduler` `Trigger` union are lane sources, not a separate scheduler.
- Entry: `LanePolicy.run` opens one `anyio.create_task_group` under a `CapacityLimiter` and a `fail_after` deadline, accepting either bare `Work[T]` coroutines or `(ContentKey, Work[T])` pairs so a unit whose key already carries an `Ok` result short-circuits without invoking the coroutine, returning a `DrainReceipt`; `LanePolicy.offload` routes a caller-supplied CPU kernel into per-subinterpreter execution through `anyio.to_interpreter.run_sync` under the same `CapacityLimiter`, never importing the kernel, injecting the active OTel context into a carrier the worker-side shim extracts-and-attaches so the offloaded span seeds from the calling span rather than a fresh root, draining through the same `DrainReceipt`; `StagePlan.execute` drives the stage DAG through `graphlib.TopologicalSorter` in active `prepare`/`get_ready`/`done` mode so each dependency-level front runs concurrently under one `LanePolicy.run` over the front's flattened work, threading each stage's `RetryClass` into the work-builder so the coroutines bind the right `reliability/resilience#RESILIENCE` `guard`, one `DrainReceipt` per front; `fired` yields one tick per `apscheduler` `Trigger` fire time and `watched` yields one tick per `PythonFilter`-narrowed filesystem change, both feeding work into the same lane.
- Auto: cancellation rides `CancelScope`; capacity rides one `CapacityLimiter` per lane; the deadline rides `anyio.fail_after` reading the `Deadline` budget from `execution/admission#CONTEXT`; a cancelled task is the receipt's `accepted - len(outcomes)` delta rather than a raise; cron, interval, and one-off triggers all resolve through `Trigger.get_next_fire_time` to a fire time that enqueues into the one bounded task group and emits the one `DrainReceipt`, cron owned solely by `apscheduler` `CronTrigger`/`from_crontab` — no second cron owner.
- Auto: each lane sends the full `RuntimeRail[T]` outcome over the memory-object stream rather than a pre-collapsed bool, so the typed `BoundaryFault` the `reliability/faults#FAULT` rail mints survives into the receipt — `run` and `cached` split the resolved rails with `Block.choose(rail.swap().to_option())` into the `faults` `Block[BoundaryFault]` and recover the `Ok` values with `Block.choose(rail.to_option())`, mirroring the `faults#traversed` accumulate fold so a drained lane surfaces which units failed and why; `rejected` is the cardinality of that typed `Block`, never a fault-erasing count.
- Auto: the content-keyed admission folds the `ContentKey` from `evidence/identity#IDENTITY` so a settings change misses correctly — the cache is an `expression` `Map[ContentKey, T]` threaded immutably across one session, a hit reproduces the exact `Ok` value and increments the receipt's `hit` count distinct from `completed`, only an `Ok` outcome folds into the `Map` (`pair.to_option().map(cache.add).default_value(cache)`) and an `Error` re-runs while its fault accumulates into the receipt, and the `Map` carries the most expensive offline work (re-tessellation, IFC evaluation, scan registration) by reference; the cache is session-local in-memory, never a durable store — durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the CPU-bound offload uses `anyio.to_interpreter.run_sync(traced_kernel, carrier, kernel, *args, limiter=...)` so each subinterpreter carries its own GIL under PEP 734 and heavy geometry work never stalls the companion event loop or contends the main GIL, with no pickle round-trip on the subinterpreter path; the lane injects the active context into a plain `dict[str, str]` carrier through `propagate.inject` before the hop and the module-level `traced_kernel` shim runs `propagate.extract`+`context.attach` inside the worker before invoking the caller kernel and `context.detach` in a `finally`, so the offloaded work runs under the calling span across the process/subinterpreter boundary and a flame graph stitches the offload leg to its parent rather than rooting a fresh trace; the kernel is a caller-supplied `Callable` the lane never imports, and a `BrokenWorkerInterpreter` raised inside the worker crosses the one `reliability/faults#FAULT` `async_boundary` conversion rather than a bare raise — the lane offloads, it never owns the geometry/scan numeric loops the `libs/python` siblings own.
- Auto: the watch lane is one `watched(paths, watch_filter)` async iterator wrapping `watchfiles.awatch(*paths, watch_filter=watch_filter)` defaulting to `watchfiles.PythonFilter()` so only Python-source `Change` events feed the lane, the consumer matching on the `Change` enum rather than string-comparing event kinds and never running a `stat` polling loop; the change batch enters the same bounded task group as a `Work[T]` feeder, draining through the same `DrainReceipt`, the lane owning the cancel scope and capacity while `watchfiles` owns the change stream.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`CancelScope`/`fail_after`/`create_memory_object_stream`/`to_interpreter.run_sync`/`BrokenWorkerInterpreter`), `watchfiles` (`awatch`/`PythonFilter`/`Change`), `apscheduler` (`triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`Trigger.get_next_fire_time`/`CronTrigger.from_crontab`), `opentelemetry-api` (`propagate.inject`/`propagate.extract`/`context.attach`/`context.detach` for the offload context stitch), `expression` (`Map`/`Block`/`Option`/`Result.swap`/`to_option`), `graphlib` (stdlib `TopologicalSorter` driven in active `prepare`/`get_ready`/`done` mode for concurrent same-level fronts).
- Growth: a new lane source is one feeder into the existing task group; a new trigger modality is one `apscheduler` `Trigger` row; a new stage is one edge on `StagePlan`; a CPU kernel is one `offload` callable carried across the context stitch unchanged; a cached unit is one `(ContentKey, Work[T])` pair; a new watch filter is one `BaseFilter` argument to `watched`; a new drain outcome dimension is one member on the `DrainOutcome` literal plus its `DrainReceipt` field, reaching the metrics counter and the receipt emit through the imported `DRAIN_COLUMNS` by that one add; zero new surface.
- Boundary: no daemon scheduler beside `StagePlan`, second cron owner beside `apscheduler`, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation, a second scheduler surface, a process-pool serialization tax on a CPU kernel a subinterpreter isolates, a durable lane cache, a kernel the lane imports rather than receives, a serial stage walk where a same-level front could run concurrently, a fresh-root span on the offloaded leg where the calling span is the parent, a `croniter` or hand-rolled `anyio.sleep` cron replacement beside the `apscheduler` `Trigger` union, and a per-event path-string filter where `PythonFilter` already narrows the change stream are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from datetime import datetime, timezone
from graphlib import TopologicalSorter
from os import PathLike
from typing import Final, Literal, get_args

import anyio
from anyio import CapacityLimiter, fail_after
from anyio.streams.memory import MemoryObjectSendStream
from anyio.to_interpreter import run_sync as interpreter_run_sync
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Nothing, Option
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import context, propagate
from watchfiles import BaseFilter, Change, PythonFilter, awatch

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.resilience import RetryClass

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Keyed[T] = tuple[ContentKey, Work[T]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger
type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]

DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)


def traced_kernel[T](carrier: dict[str, str], kernel: Callable[..., T], *args: object) -> T:
    token = context.attach(propagate.extract(carrier))
    try:
        return kernel(*args)
    finally:
        context.detach(token)


class DrainReceipt(Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    faults: Block[BoundaryFault] = Block.empty()
    hit: int = 0


class LanePolicy(Struct, frozen=True):
    capacity: int
    deadline: Option[float] = Nothing

    async def run[T](self, work: Sequence[Work[T]]) -> DrainReceipt:
        limiter = CapacityLimiter(self.capacity)
        send, receive = anyio.create_memory_object_stream[RuntimeRail[T]](max_buffer_size=len(work))

        async def lane(fn: Work[T], sink: MemoryObjectSendStream[RuntimeRail[T]]) -> None:
            async with sink, limiter:
                await sink.send(await fn())

        deadline = self.deadline.default_value(0.0)
        with fail_after(deadline) if self.deadline.is_some() else anyio.CancelScope():
            async with anyio.create_task_group() as group, send:
                for fn in work:
                    group.start_soon(lane, fn, send.clone())
        rails = Block.of_seq([item async for item in receive])
        faults = rails.choose(lambda rail: rail.swap().to_option())
        completed = len(rails.choose(lambda rail: rail.to_option()))
        return DrainReceipt(
            accepted=len(work), completed=completed, cancelled=len(work) - len(rails), rejected=len(faults), faults=faults
        )

    async def cached[T](self, work: Sequence[Keyed[T]], cache: Map[ContentKey, T]) -> tuple[DrainReceipt, Map[ContentKey, T]]:
        misses = [(key, fn) for key, fn in work if key not in cache]
        hits = len(work) - len(misses)
        send, receive = anyio.create_memory_object_stream[tuple[ContentKey, RuntimeRail[T]]](max_buffer_size=len(misses) or 1)
        limiter = CapacityLimiter(self.capacity)

        async def lane(key: ContentKey, fn: Work[T], sink: MemoryObjectSendStream[tuple[ContentKey, RuntimeRail[T]]]) -> None:
            async with sink, limiter:
                await sink.send((key, await fn()))

        async with anyio.create_task_group() as group, send:
            for key, fn in misses:
                group.start_soon(lane, key, fn, send.clone())
        resolved = Block.of_seq([item async for item in receive])
        threaded = resolved.fold(lambda acc, pair: pair[1].to_option().map(lambda value: acc.add(pair[0], value)).default_value(acc), cache)
        faults = resolved.choose(lambda pair: pair[1].swap().to_option())
        completed = len(resolved) - len(faults)
        receipt = DrainReceipt(
            accepted=len(work), completed=completed, cancelled=len(misses) - len(resolved), rejected=len(faults), faults=faults, hit=hits
        )
        return receipt, threaded

    async def offload[T](self, kernel: Callable[..., T], *args: object) -> RuntimeRail[T]:
        limiter = CapacityLimiter(self.capacity)
        carrier: dict[str, str] = {}
        propagate.inject(carrier)
        return await async_boundary("offload", lambda: interpreter_run_sync(traced_kernel, carrier, kernel, *args, limiter=limiter))


class StagePlan(Struct, frozen=True):
    lane: LanePolicy
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]

    async def execute[T](self, work: Callable[[str, RetryClass], Sequence[Work[T]]]) -> tuple[DrainReceipt, ...]:
        classes = {stage: cls for stage, cls in self.stages}
        order: TopologicalSorter[str] = TopologicalSorter({stage: () for stage in classes})
        for parent, child in self.edges:
            order.add(child, parent)
        order.prepare()
        fronts: list[DrainReceipt] = []
        while order.is_active():
            front = order.get_ready()
            fronts.append(await self.lane.run([fn for stage in front for fn in work(stage, classes[stage])]))
            order.done(*front)
        return tuple(fronts)


async def fired(trigger: Trigger) -> AsyncIterator[None]:
    previous: datetime | None = None
    while (nxt := trigger.get_next_fire_time(previous, datetime.now(timezone.utc))) is not None:
        await anyio.sleep(max(0.0, nxt.timestamp() - datetime.now(timezone.utc).timestamp()))
        previous = nxt
        yield None


async def watched(*paths: str | PathLike[str], watch_filter: BaseFilter | None = None) -> AsyncIterator[set[tuple[Change, str]]]:
    async for batch in awatch(*paths, watch_filter=watch_filter if watch_filter is not None else PythonFilter()):
        yield batch
```

## [03]-[RESEARCH]

[APSCHEDULER_CATALOGUE], [INTERPRETER_OFFLOAD], and [CONTENT_LANE_CACHE] are reflection-confirmed on the cp315 core (`apscheduler` 3.11.2, `anyio.to_interpreter`, `concurrent.interpreters` per PEP 734): the `triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger` namespaces, the `Trigger.get_next_fire_time(previous_fire_time, now)` 2-argument contract returning a timezone-aware `datetime` or `None` at exhaustion, `CronTrigger.from_crontab` as the sole cron-string intake (no `aiocron`, no `croniter`, no hand-rolled `anyio.sleep` cron replacement — the `Trigger` union owns every schedule modality and `fired` uses `anyio.sleep` only for the inter-fire wait), the `anyio.to_interpreter.run_sync(func, *args, limiter=None)` subinterpreter offload (`.api/anyio.md` thread-interop ENTRYPOINTS [06]) raising `anyio.BrokenWorkerInterpreter` (error-types PUBLIC_TYPES [10]) on worker failure, and the `expression.collections.Map.add(key, value)` (collection-ops ENTRYPOINTS [02]) / `key in map` (ENTRYPOINTS [03]) cache surface are settled. The receipt threads the typed `Block[BoundaryFault]` through `Block.of_seq`/`Block.choose` (collection-ops ENTRYPOINTS) over each lane's `RuntimeRail[T]` outcome, splitting Oks via `Result.to_option` and faults via `Result.swap().to_option` exactly as `reliability/faults#traversed` accumulates, so a rejected unit's fault stays structurally addressable rather than collapsing to a scalar tally. `anyio.to_interpreter.run_sync` rides a runnable `concurrent.interpreters` on the cp315 core; were a given cp315 build to ship without it, the offload degrades to the confirmed `anyio.to_thread.run_sync` (ENTRYPOINTS [01]) path with a GIL caveat rather than blocking the leg.

- [OFFLOAD_TRACE_STITCH]: reflection-confirmed against `.api/opentelemetry-api.md` — `propagate.inject(carrier, context)` (context/propagation ENTRYPOINTS [07]) writes the active W3C `traceparent`/`tracestate` into a plain `dict[str, str]` carrier resolved through the `observability/telemetry#TELEMETRY`-installed composite `propagate.get_global_textmap` (ENTRYPOINTS [08]), and the module-level `traced_kernel` shim runs `propagate.extract(carrier, context)` (ENTRYPOINTS [06])+`context.attach` (ENTRYPOINTS [02]) inside the subinterpreter/process worker (`context.detach` ENTRYPOINTS [03] token-paired in `finally`) before invoking the caller kernel, so the offloaded span seeds from the calling span across the PEP 734 hop rather than a fresh root — the same `Context` extract-and-attach pair `observability/receipts#RECEIPT` `Signals.continue_inbound`/`Signals.attach` uses on the inbound gRPC leg (which cites the same `.api/opentelemetry-api.md` indices), realizing the `ONE_DISTRIBUTED_TRACE` offload leg. The `opentelemetry-instrumentation-grpc` owner carries only the interceptor/filter surface and delegates propagation to `.api/opentelemetry-api.md`, so the propagation members are cited against the API catalog, not the grpc instrumentor catalog. The carrier crosses the no-pickle subinterpreter boundary as a plain `dict[str, str]`, and `propagate.inject`/`extract` resolve the no-op propagator before the `observability/telemetry#TELEMETRY` install, so an un-installed profile carries an empty carrier and the kernel runs unparented — the mechanical reason this composes with, never re-installs, the propagator. Spellings settled.

- [STAGE_FRONT_CONCURRENCY]: reflection-confirmed against the stdlib `graphlib.TopologicalSorter` active interface — `prepare()`/`is_active()`/`get_ready()`/`done(*nodes)` exposes each dependency-level front as the `get_ready()` tuple, so `StagePlan.execute` runs every same-level stage's flattened work concurrently under one `LanePolicy.run` per front rather than the serial `static_order()` walk, one `DrainReceipt` per front, the front-internal capacity still bounded by the lane's one `CapacityLimiter`. Settled.

- [WATCH_FILTER]: reflection-confirmed against `.api/watchfiles.md` `[02]-[PUBLIC_TYPES]`/`[03]-[ENTRYPOINTS]` — `watchfiles.PythonFilter` (PUBLIC_TYPES [04], the Python-source change filter) narrows the `awatch(*paths, watch_filter=...)` change stream so only Python-source `Change` events feed the lane, the `BaseFilter` parameter accepting any caller filter; `watched` yields the `set[tuple[Change, str]]` batch and the consumer matches on the `Change` enum per the `.api` classification law, never a `stat` polling loop or per-event path-string filter. Settled.

No open RESEARCH seam remains on this page.
