# [PY_RUNTIME_RESOURCES_LANES]

Resource roots, transport resources, and bounded structured-concurrency lanes. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with safe relative resolution; `TransportResource` is the one tagged union over HTTP/SSH/Speckle acquisition; `LanePolicy` owns bounded `anyio` task groups with drain receipts, and `StagePlan` owns multi-stage DAG orchestration over the same task-group spine. No daemon scheduler, app lifecycle hook, or product store-root derivation crosses this page.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | RESOURCE  | resource roots, references, transport resources                |
|   [2]   | LANE      | bounded anyio task groups, drain receipts, the stage DAG       |

## [2]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP, `asyncssh` SSH/SFTP, and `specklepy` Speckle stream send/receive (a remote-AEC transport row, never a durable store).
- Cases: `TransportResource` cases `Http(url, retry_class)` · `Ssh(host, port, retry_class)` · `Speckle(stream_id, token)` — each matched by `match`/`case`, each binding a `Retry` row from `rails-resilience#RESILIENCE`.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection; `TransportResource.acquire` returns a `RuntimeRail[bytes]` over the bound retry policy.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `universal-pathlib` (`UPath`), `httpx` (`AsyncClient`), `asyncssh` (`connect`/`start_sftp_client`), `specklepy`.
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves; a new transport is one `TransportResource` case binding a `Retry` row; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, or companion-control transport; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, and a Speckle durable-store treatment are the deleted forms; Speckle is transport + bundle shapes only.

```python signature
from typing import Literal, Self

from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct
from upath import UPath

from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail
from rasm.runtime.rails_resilience import Retry as Retry
from rasm.runtime.rails_resilience import RetryClass


class ResourceRef(Struct, frozen=True):
    scheme: str
    root: str
    relative: str
    owner: str

    @property
    def path(self) -> UPath:
        return UPath(self.root, protocol=self.scheme) / self.relative


class ResourceRoot(Struct, frozen=True):
    scheme: str
    root: str
    owner: str

    @classmethod
    def admit(cls, uri: str, owner: str) -> Self:
        path = UPath(uri)
        return cls(scheme=path.protocol or "file", root=str(path), owner=owner)

    def child(self, relative: str) -> "RuntimeRail[ResourceRef]":
        normalized = UPath(self.root) / relative
        return (
            Ok(ResourceRef(self.scheme, self.root, relative, self.owner))
            if str(normalized).startswith(self.root)
            else Error(BoundaryFault.Resource("traversal", relative))
        )


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh", "speckle"] = tag()
    http: tuple[str, RetryClass] = case()
    ssh: tuple[str, int, RetryClass] = case()
    speckle: tuple[str, str] = case()

    @staticmethod
    def Http(url: str, retry_class: RetryClass) -> "TransportResource":
        return TransportResource(http=(url, retry_class))

    @staticmethod
    def Ssh(host: str, port: int, retry_class: RetryClass) -> "TransportResource":
        return TransportResource(ssh=(host, port, retry_class))

    @staticmethod
    def Speckle(stream_id: str, token: str) -> "TransportResource":
        return TransportResource(speckle=(stream_id, token))
```

## [3]-[LANE]

- Owner: `LanePolicy` — bounded `anyio` task groups with capacity and cancellation scopes; `DrainReceipt` the accepted/completed/cancelled/rejected counts; `StagePlan` the multi-stage DAG (stage edges, per-stage retry, partial re-run) over the same task-group spine; `watchfiles`/`aiocron` are lane sources, not a separate scheduler.
- Entry: `LanePolicy.run` opens one `anyio.create_task_group` under a `CapacityLimiter` and a `fail_after` deadline, returning a `DrainReceipt`; `StagePlan.execute` walks the stage DAG over the same lane spine; the file-watch and cron sources feed work into the lane through `watchfiles.awatch` and `aiocron.crontab`.
- Auto: cancellation rides `CancelScope`; capacity rides `CapacityLimiter`; the deadline rides `anyio.fail_after` reading the `Deadline` budget from `RuntimeContext`; a rejected admission increments the receipt rather than raising.
- Packages: `anyio` (`create_task_group`/`CapacityLimiter`/`CancelScope`/`fail_after`/`to_thread.run_sync`/`to_process.run_sync`), `watchfiles` (`awatch`), `aiocron` (`crontab`).
- Growth: a new lane source is one feeder into the existing task group; a new stage is one edge on `StagePlan`; zero new surface.
- Boundary: no daemon scheduler, app lifecycle hook, service health contribution, or background loop without a drain receipt; unbounded task creation and a second scheduler surface beside `StagePlan` are the deleted forms; bare `asyncio` is never imported — `anyio` owns every concurrency primitive.

```python signature
from collections.abc import Awaitable, Callable, Sequence

import anyio
from anyio import CapacityLimiter, fail_after
from anyio.streams.memory import MemoryObjectSendStream
from expression import Nothing, Option
from msgspec import Struct

from rasm.runtime.rails_resilience import RetryClass, RuntimeRail


class DrainReceipt(Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int


class LanePolicy(Struct, frozen=True):
    capacity: int
    deadline: Option[float] = Nothing

    async def run[T](self, work: Sequence[Callable[[], Awaitable[RuntimeRail[T]]]]) -> DrainReceipt:
        limiter = CapacityLimiter(self.capacity)
        send, receive = anyio.create_memory_object_stream[bool](max_buffer_size=len(work))

        async def lane(fn: Callable[[], Awaitable[RuntimeRail[T]]], sink: MemoryObjectSendStream[bool]) -> None:
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
    stages: tuple[tuple[str, RetryClass], ...]
    edges: tuple[tuple[str, str], ...]
```

## [4]-[RESEARCH]

- [SPECKLE_TRANSPORT]: the `specklepy` stream send/receive client surface (`SpeckleClient`, `operations.send`/`receive`) backing the `TransportResource.speckle` case is verified against `specklepy>=3.2.8`; the exact operation spellings confirm against `.api/api-specklepy.md` at fence transcription.
