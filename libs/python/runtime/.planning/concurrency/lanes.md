# [PY_RUNTIME_LANES]

Bounded structured-concurrency lanes and stage orchestration. `LanePolicy` owns bounded `anyio` task groups with capacity and cancellation scopes, returning a `DrainReceipt`; `StagePlan` owns multi-stage DAG orchestration over the same task-group spine. Watch-triggered and scheduled work enter the same bounded task group through `watchfiles` and the `apscheduler` `Trigger` union, draining through the same receipt — never a second scheduler surface. Bare `asyncio` is never imported; `anyio` owns every concurrency primitive.

## [1]-[INDEX]

One cluster: `[2]-[LANE]` — bounded anyio task groups, drain receipts, the stage DAG.

## [2]-[LANE]

- Owner: `LanePolicy` — bounded `anyio` task groups with capacity and cancellation scopes; `DrainReceipt` the accepted/completed/cancelled/rejected counts; `StagePlan` the multi-stage DAG (stage edges, per-stage retry, partial re-run) over the same task-group spine; `watchfiles` and the `apscheduler` `Trigger` union are lane sources, not a separate scheduler.
- Entry: `LanePolicy.run` opens one `anyio.create_task_group` under a `CapacityLimiter` and a `fail_after` deadline, returning a `DrainReceipt`; `StagePlan.execute` topologically orders the stage DAG through `graphlib.TopologicalSorter` and drives each stage through the same `LanePolicy.run`, threading the stage's `RetryClass` into the work-builder so each stage's coroutines bind the right `reliability/resilience#RESILIENCE` `guard`, one `DrainReceipt` per stage; `fired` yields one tick per `apscheduler` `Trigger` fire time and `watchfiles.awatch` yields one tick per filesystem change, both feeding work into the same lane.
- Auto: cancellation rides `CancelScope`; capacity rides one `CapacityLimiter` per lane; the deadline rides `anyio.fail_after` reading the `Deadline` budget from `context/admission#CONTEXT`; a cancelled task is the receipt's `len(work) - len(outcomes)` delta rather than a raise; cron, interval, and one-off triggers all resolve through `Trigger.get_next_fire_time` to a fire time that enqueues into the one bounded task group and emits the one `DrainReceipt`.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`CancelScope`/`fail_after`/`create_memory_object_stream`/`current_time`/`to_thread.run_sync`/`to_process.run_sync`), `watchfiles` (`awatch`), `apscheduler` (`triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger`/`Trigger.get_next_fire_time`), `graphlib` (stdlib `TopologicalSorter`).
- Growth: a new lane source is one feeder into the existing task group; a new trigger modality is one `apscheduler` `Trigger` row; a new stage is one edge on `StagePlan`; zero new surface.
- Boundary: no daemon scheduler beside `StagePlan`, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation and a second scheduler surface are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Sequence
from graphlib import TopologicalSorter

import anyio
from anyio import CapacityLimiter, fail_after
from anyio.streams.memory import MemoryObjectSendStream
from apscheduler.triggers.cron import CronTrigger
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.interval import IntervalTrigger
from expression import Nothing, Option
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.resilience import RetryClass

type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]
type Trigger = CronTrigger | IntervalTrigger | DateTrigger


class DrainReceipt(Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int


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
    while (nxt := trigger.get_next_fire_time(None, anyio.current_time())) is not None:
        await anyio.sleep(max(0.0, nxt.timestamp() - anyio.current_time()))
        yield None
```

## [3]-[RESEARCH]

- [APSCHEDULER_CATALOGUE]: `apscheduler` 3.x is manifest-declared and resolves on the cp315 core but carries no `.api/` catalogue; the `triggers.cron.CronTrigger`/`triggers.interval.IntervalTrigger`/`triggers.date.DateTrigger` namespaces and the `Trigger.get_next_fire_time(previous_fire_time, now)` 2-argument contract returning a timezone-aware `datetime` or `None` at exhaustion confirm against the `apscheduler` catalogue at capture; the `fired` feeder reads exactly that contract.
