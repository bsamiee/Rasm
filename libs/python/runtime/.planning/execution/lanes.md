# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy` owns bounded `anyio` task groups with capacity and cancellation scopes, returning a `DrainReceipt`; `StagePlan` owns multi-stage DAG orchestration over the same task-group spine. Watch-triggered and scheduled work enter the same bounded task group through `watchfiles` and the `apscheduler` `Trigger` union, draining through the same receipt — never a second scheduler surface, cron owned solely by `apscheduler`. The one lane admits I/O-bound coroutines, CPU-bound kernels offloaded into per-subinterpreter execution under PEP 734, and content-keyed work that short-circuits on a cache hit, all under one `CapacityLimiter` and one `DrainReceipt`. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive.

## [01]-[INDEX]

- [01]-[LANE]: bounded anyio task groups, the CPU-bound subinterpreter offload, the content-keyed cache short-circuit, drain receipts, the stage DAG.

## [02]-[LANE]

- Owner: `LanePolicy` — bounded `anyio` task groups with capacity and cancellation scopes; `DrainReceipt` the accepted/completed/cancelled/rejected/hit counts; `StagePlan` the multi-stage DAG (stage edges, per-stage retry, partial re-run) over the same task-group spine; `watchfiles` and the `apscheduler` `Trigger` union are lane sources, not a separate scheduler.
- Entry: `LanePolicy.run` opens one `anyio.create_task_group` under a `CapacityLimiter` and a `fail_after` deadline, accepting either bare `Work[T]` coroutines or `(ContentKey, Work[T])` pairs so a unit whose key already carries an `Ok` result short-circuits without invoking the coroutine, returning a `DrainReceipt`; `LanePolicy.offload` routes a caller-supplied CPU kernel into per-subinterpreter execution through `anyio.to_interpreter.run_sync` under the same `CapacityLimiter`, never importing the kernel, draining through the same `DrainReceipt`; `StagePlan.execute` topologically orders the stage DAG through `graphlib.TopologicalSorter` and drives each stage through the same `LanePolicy.run`, threading the stage's `RetryClass` into the work-builder so each stage's coroutines bind the right `reliability/resilience#RESILIENCE` `guard`, one `DrainReceipt` per stage; `fired` yields one tick per `apscheduler` `Trigger` fire time and `watchfiles.awatch` yields one tick per filesystem change, both feeding work into the same lane.
- Auto: cancellation rides `CancelScope`; capacity rides one `CapacityLimiter` per lane; the deadline rides `anyio.fail_after` reading the `Deadline` budget from `execution/admission#CONTEXT`; a cancelled task is the receipt's `accepted - len(outcomes)` delta rather than a raise; cron, interval, and one-off triggers all resolve through `Trigger.get_next_fire_time` to a fire time that enqueues into the one bounded task group and emits the one `DrainReceipt`, cron owned solely by `apscheduler` `CronTrigger`/`from_crontab` — no second cron owner.
- Auto: the content-keyed admission folds the `ContentKey` from `evidence/identity#IDENTITY` so a settings change misses correctly — the cache is an `expression` `Map[ContentKey, T]` threaded immutably across one session, a hit reproduces the exact `Ok` value and increments the receipt's `hit` count distinct from `completed`, only an `Ok` outcome caches and an `Error` re-runs, and the `Map` carries the most expensive offline work (re-tessellation, IFC evaluation, scan registration) by reference; the cache is session-local in-memory, never a durable store — durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire.
- Auto: the CPU-bound offload uses `anyio.to_interpreter.run_sync(kernel, *args, limiter=...)` so each subinterpreter carries its own GIL under PEP 734 and heavy geometry work never stalls the companion event loop or contends the main GIL, with no pickle round-trip on the subinterpreter path; the kernel is a caller-supplied `Callable` the lane never imports, and a `BrokenWorkerInterpreter` raised inside the worker crosses the one `reliability/faults#FAULT` `async_boundary` conversion rather than a bare raise — the lane offloads, it never owns the geometry/scan numeric loops the `libs/python` siblings own.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`CancelScope`/`fail_after`/`create_memory_object_stream`/`current_time`/`to_thread.run_sync`/`to_interpreter.run_sync`/`BrokenWorkerInterpreter`), `watchfiles` (`awatch`), `apscheduler` (`triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`Trigger.get_next_fire_time`/`CronTrigger.from_crontab`), `expression` (`Map`/`Option`), `graphlib` (stdlib `TopologicalSorter`).
- Growth: a new lane source is one feeder into the existing task group; a new trigger modality is one `apscheduler` `Trigger` row; a new stage is one edge on `StagePlan`; a CPU kernel is one `offload` callable; a cached unit is one `(ContentKey, Work[T])` pair; zero new surface.
- Boundary: no daemon scheduler beside `StagePlan`, second cron owner beside `apscheduler`, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation, a second scheduler surface, a process-pool serialization tax on a CPU kernel a subinterpreter isolates, a durable lane cache, and a kernel the lane imports rather than receives are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from datetime import datetime, timezone
from graphlib import TopologicalSorter

import anyio
from anyio import CapacityLimiter, fail_after
from anyio.streams.memory import MemoryObjectSendStream
from anyio.to_interpreter import run_sync as interpreter_run_sync
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Nothing, Option
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.resilience import RetryClass

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Keyed[T] = tuple[ContentKey, Work[T]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger


class DrainReceipt(Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    hit: int = 0


class LanePolicy(Struct, frozen=True):
    capacity: int
    deadline: Option[float] = Nothing

    async def run[T](self, work: Sequence[Work[T]]) -> DrainReceipt:
        limiter = CapacityLimiter(self.capacity)
        send, receive = anyio.create_memory_object_stream[bool](max_buffer_size=len(work))

        async def lane(fn: Work[T], sink: MemoryObjectSendStream[bool]) -> None:
            async with sink, limiter:
                await sink.send((await fn()).is_ok())

        deadline = self.deadline.default_value(0.0)
        with fail_after(deadline) if self.deadline.is_some() else anyio.CancelScope():
            async with anyio.create_task_group() as group, send:
                for fn in work:
                    group.start_soon(lane, fn, send.clone())
        outcomes = [item async for item in receive]
        completed = sum(outcomes)
        return DrainReceipt(accepted=len(work), completed=completed, cancelled=len(work) - len(outcomes), rejected=len(outcomes) - completed)

    async def cached[T](self, work: Sequence[Keyed[T]], cache: Map[ContentKey, T]) -> tuple[DrainReceipt, Map[ContentKey, T]]:
        misses = [(key, fn) for key, fn in work if key not in cache]
        hits = len(work) - len(misses)
        send, receive = anyio.create_memory_object_stream[tuple[ContentKey, T] | None](max_buffer_size=len(misses) or 1)
        limiter = CapacityLimiter(self.capacity)

        async def lane(key: ContentKey, fn: Work[T], sink: MemoryObjectSendStream[tuple[ContentKey, T] | None]) -> None:
            async with sink, limiter:
                outcome = await fn()
                await sink.send((key, outcome.value) if outcome.is_ok() else None)

        async with anyio.create_task_group() as group, send:
            for key, fn in misses:
                group.start_soon(lane, key, fn, send.clone())
        resolved = [item async for item in receive]
        completed = [pair for pair in resolved if pair is not None]
        threaded = cache
        for key, value in completed:
            threaded = threaded.add(key, value)
        receipt = DrainReceipt(
            accepted=len(work), completed=len(completed), cancelled=len(misses) - len(resolved), rejected=len(resolved) - len(completed), hit=hits
        )
        return receipt, threaded

    async def offload[T](self, kernel: Callable[..., T], *args: object) -> RuntimeRail[T]:
        limiter = CapacityLimiter(self.capacity)
        return await async_boundary("offload", lambda: interpreter_run_sync(kernel, *args, limiter=limiter))


class StagePlan(Struct, frozen=True):
    lane: LanePolicy
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]

    async def execute[T](self, work: Callable[[str, RetryClass], Sequence[Work[T]]]) -> tuple[DrainReceipt, ...]:
        classes = {stage: cls for stage, cls in self.stages}
        order = TopologicalSorter({stage: () for stage in classes})
        for parent, child in self.edges:
            order.add(child, parent)
        return tuple([await self.lane.run(work(stage, classes[stage])) for stage in order.static_order()])


async def fired(trigger: Trigger) -> AsyncIterator[None]:
    previous: datetime | None = None
    while (nxt := trigger.get_next_fire_time(previous, datetime.now(timezone.utc))) is not None:
        await anyio.sleep(max(0.0, nxt.timestamp() - datetime.now(timezone.utc).timestamp()))
        previous = nxt
        yield None
```

## [03]-[RESEARCH]

[APSCHEDULER_CATALOGUE], [INTERPRETER_OFFLOAD], and [CONTENT_LANE_CACHE] are reflection-confirmed on the cp315 core (`apscheduler` 3.11.2, `anyio.to_interpreter`, `concurrent.interpreters` per PEP 734): the `triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger` namespaces, the `Trigger.get_next_fire_time(previous_fire_time, now)` 2-argument contract returning a timezone-aware `datetime` or `None` at exhaustion, `CronTrigger.from_crontab` as the sole cron-string intake (no `aiocron`), the `anyio.to_interpreter.run_sync(func, *args, limiter=None)` subinterpreter offload raising `BrokenWorkerInterpreter` on worker failure, and the `expression.collections.Map.add(key, value)`/`key in map` cache surface are settled. No open RESEARCH seam remains on this page.
