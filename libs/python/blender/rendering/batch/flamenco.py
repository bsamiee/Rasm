"""Render queue submission to Flamenco Manager."""

import base64
from collections.abc import Callable, Mapping
from functools import partial
from http import HTTPMethod
from itertools import batched, starmap
from pathlib import Path, PurePath, PurePosixPath
import platform
import sys
from typing import Annotated, ClassVar, Final, Literal, Self

from expression import Nothing, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import httpx2
import msgspec
from scipy.cluster.hierarchy import DisjointSet

from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.stores import files_copied
from blender.rendering.batch.exchange import Endpoint, exchange, sent
from blender.rendering.batch.queue import Placed, RenderFrames, RenderQueue

# --- [CONSTANTS] ------------------------------------------------------------------------

_PLATFORM: Final = platform.system().lower()

# --- [MODELS] ---------------------------------------------------------------------------


class _SharedStorageLocation(msgspec.Struct, frozen=True, kw_only=True, rename="camel"):
    """Platform job storage path with Shaman state."""

    location: str
    shaman_enabled: bool


class _AvailableJobType(msgspec.Struct, frozen=True, kw_only=True):
    """Job type Manager runs."""

    name: str
    etag: str


class _AvailableJobTypes(msgspec.Struct, frozen=True, kw_only=True):
    """Job types Manager runs."""

    job_types: tuple[_AvailableJobType, ...]


class _WorkerTag(msgspec.Struct, frozen=True, kw_only=True):
    """Tag naming worker group."""

    name: str
    id: str | None = None
    description: str | None = None


class _WorkerTagList(msgspec.Struct, frozen=True, kw_only=True):
    """Worker tags Manager holds."""

    tags: tuple[_WorkerTag, ...] = ()


class _ShamanFileSpec(msgspec.Struct, frozen=True, kw_only=True):
    """File by SHA-256, size, and checkout path."""

    sha: str
    size: int
    path: str


class _ShamanRequirements(msgspec.Struct, frozen=True, kw_only=True):
    """Files a checkout needs, or the requested files Shaman server lacks."""

    files: tuple[_ShamanFileSpec, ...]


class _ShamanCheckout(msgspec.Struct, frozen=True, kw_only=True, rename="camel"):
    """Files linked into checkout folder."""

    files: tuple[_ShamanFileSpec, ...]
    checkout_path: str


class _ShamanCheckoutResult(msgspec.Struct, frozen=True, kw_only=True, rename="camel"):
    """Checkout folder Manager created."""

    checkout_path: str


class _JobStorageInfo(msgspec.Struct, frozen=True, kw_only=True):
    """Shaman checkout holding job files."""

    shaman_checkout_id: str


class _JobRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Opaque render command argument with step count."""

    argument: str
    steps: int


class _JobTasks(msgspec.Struct, frozen=True, kw_only=True):
    """Frame requests of one blend file with prerequisite parts."""

    blendfile: str
    output_root: str
    requests: tuple[_JobRequest, ...]
    prerequisites: tuple[int, ...]


class _SubmittedJob(msgspec.Struct, frozen=True, kw_only=True):
    """Job posted to Manager."""

    name: str
    type: str
    priority: int
    submitter_platform: str
    settings: Mapping[str, str | tuple[_JobTasks, ...]]
    initial_status: Literal["queued", "paused"]
    type_etag: str | None = None
    worker_tag: str | None = None
    storage: _JobStorageInfo | None = None


class _ManagerJob(msgspec.Struct, frozen=True, kw_only=True):
    """Submitted job ID."""

    id: str


class _ManagerFiles(msgspec.Struct, frozen=True, kw_only=True):
    """Manager path per source file with Shaman checkout."""

    files: Mapping[str, str]
    storage: Option[_JobStorageInfo]


class Manager(msgspec.Struct, frozen=True, kw_only=True):
    """Manager state submission reads."""

    storage: _SharedStorageLocation
    job_type: Option[_AvailableJobType]
    tags: tuple[_WorkerTag, ...]

    @classmethod
    def queried(cls, client: httpx2.Client) -> Outcome[Self]:
        """Reads storage location, the Rasm job type, and worker tags."""
        return paired(
            exchange(client, HTTPMethod.GET, f"/api/v3/configuration/shared-storage/users/{_PLATFORM}", _SharedStorageLocation),
            exchange(client, HTTPMethod.GET, "/api/v3/jobs/types", _AvailableJobTypes),
            exchange(client, HTTPMethod.GET, "/api/v3/worker-mgt/tags", _WorkerTagList),
        ).map(lambda found: cls(storage=found[0], job_type=Block(found[1].job_types).filter(lambda kind: kind.name == Flamenco.kind).try_head(), tags=found[2].tags))

    def placed(self, client: httpx2.Client, queue: RenderQueue) -> Outcome[_ManagerFiles]:
        """Uploads every job's pack to Shaman when enabled, else copies it to Manager storage, each pack under its job's part folder."""
        jobs = {job.file: (part, job) for part, job in enumerate(queue.jobs)}.values()
        entries = {PurePosixPath(str(part), placed.path).as_posix(): placed for part, job in jobs for placed in job.placements}
        folder = queue.stem

        def stored(root: str, storage: Option[_JobStorageInfo]) -> _ManagerFiles:
            return _ManagerFiles(files={job.file: f"{{jobs}}/{root}/{part}/{job.blend.path}" for part, job in jobs}, storage=storage)

        def read[T](source: str, reader: Callable[[Path], T]) -> Outcome[T]:
            return catch(reader, exception=OSError)(Path(source)).map_error(partial(Fault.of, RecordReason.UNREADABLE, source))

        def specified(name: str, placed: Placed) -> Outcome[_ShamanFileSpec]:
            return read(placed.source, Path.stat).map(lambda stat: _ShamanFileSpec(sha=placed.digest.hex(), size=stat.st_size, path=name))

        def checked(specs: tuple[_ShamanFileSpec, ...]) -> Outcome[_ManagerFiles]:
            def uploaded(spec: _ShamanFileSpec, placed: Placed) -> Outcome[httpx2.Response]:
                return read(placed.source, Path.read_bytes).bind(lambda data: sent(client, HTTPMethod.POST, f"/api/v3/shaman/files/{spec.sha}/{spec.size}", Some(data)))

            return (
                exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/requirements", _ShamanRequirements, Some(_ShamanRequirements(files=specs)))
                .map(lambda wanted: frozenset(spec.path for spec in wanted.files))
                .bind(lambda lacking: accumulated(uploaded(spec, placed) for spec, placed in zip(specs, entries.values(), strict=True) if spec.path in lacking))
                .bind(lambda _: exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/create", _ShamanCheckoutResult, Some(_ShamanCheckout(files=specs, checkout_path=folder))))
                .map(lambda created: stored(created.checkout_path, Some(_JobStorageInfo(shaman_checkout_id=created.checkout_path))))
            )

        return (
            accumulated(starmap(specified, entries.items())).bind(checked)
            if self.storage.shaman_enabled
            else files_copied((Path(placed.source), Path(self.storage.location, folder, name)) for name, placed in entries.items()).map(lambda _: stored(folder, Nothing))
        )


class Flamenco(Case, frozen=True, kw_only=True):
    """Jobs submitted to Flamenco Manager."""

    script: ClassVar[str] = "rasm_render.js"
    kind: ClassVar[str] = PurePath(script).stem.replace("_", "-")
    server: Endpoint = Endpoint(url="http://localhost:8080")
    priority: int = 50
    chunk: Annotated[int, msgspec.Meta(ge=1, le=sys.maxsize)] = 10
    paused: bool = False
    tag: str | None = None

    def submitted(self, client: httpx2.Client, queue: RenderQueue, render_command: str) -> Outcome[tuple[Outcome[str], ...]]:
        """Submits each prerequisite-joined job group as one Manager job."""

        def posted(manager: Manager, stored: _ManagerFiles) -> tuple[Outcome[str], ...]:
            components = DisjointSet(range(len(queue.jobs)))
            for part, job in enumerate(queue.jobs):
                for prerequisite in job.prerequisites:
                    components.merge(part, prerequisite)

            def component(parts: set[int]) -> Outcome[str]:
                jobs = tuple((part, job) for part, job in enumerate(queue.jobs) if part in parts)
                positions = {part: index for index, (part, _) in enumerate(jobs)}
                tasks = tuple(
                    (
                        job.output_root,
                        _JobTasks(
                            blendfile=stored.files[job.file],
                            output_root=f"output_root_{part}",
                            requests=tuple(
                                _JobRequest(argument=base64.b64encode(msgspec.json.encode(RenderFrames(scene=job.scene, frames=frames))).decode("ascii"), steps=len(frames))
                                for frames in batched(job.frames.numbers, self.chunk)
                            ),
                            prerequisites=tuple(positions[before] for before in job.prerequisites),
                        ),
                    )
                    for part, job in jobs
                )
                return exchange(
                    client,
                    HTTPMethod.POST,
                    "/api/v3/jobs",
                    _ManagerJob,
                    Some(
                        _SubmittedJob(
                            name="; ".join(job.title for _, job in jobs),
                            type=Flamenco.kind,
                            type_etag=manager.job_type.map(lambda kind: kind.etag).to_optional(),
                            priority=self.priority,
                            submitter_platform=_PLATFORM,
                            settings={"jobs": tuple(task for _, task in tasks), "render_command": render_command, **{task.output_root: root for root, task in tasks}},
                            initial_status="paused" if self.paused else "queued",
                            worker_tag=self.tag,
                            storage=stored.storage.to_optional(),
                        )
                    ),
                ).map(lambda reply: reply.id)

            return tuple(map(component, components.subsets()))

        return Manager.queried(client).bind(lambda manager: manager.placed(client, queue).map(lambda stored: posted(manager, stored)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Flamenco", "Manager"]
