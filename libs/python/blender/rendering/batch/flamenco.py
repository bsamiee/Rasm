"""Render queue submission to Flamenco Manager."""

import base64
from collections.abc import Callable
from enum import StrEnum
from http import HTTPMethod
from itertools import batched, starmap
from pathlib import Path, PurePath
import platform
import shutil
import sys
from typing import Annotated, ClassVar, Final

from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import httpx2
import msgspec
from scipy.cluster.hierarchy import DisjointSet

from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated, Presentation
from blender.rendering.batch.exchange import Endpoint, exchange, sent
from blender.rendering.batch.queue import Job, RenderFrames, RenderQueue
from blender.rendering.faults import Cause, RenderingFaults, RenderingReadFault, RenderingResult, RenderingSendFault, RenderingWriteFault

# --- [TYPES] ----------------------------------------------------------------------------


class ShamanFileStatus(StrEnum):
    """File state on Shaman server."""

    UNKNOWN = "unknown"
    UPLOADING = "uploading"
    STORED = "stored"


class JobStatus(StrEnum):
    """Status a submitted job starts in."""

    QUEUED = "queued"
    PAUSED = "paused"


# --- [CONSTANTS] ------------------------------------------------------------------------

PLATFORM: Final = platform.system().lower()

# --- [MODELS] ---------------------------------------------------------------------------


class Flamenco(Case, frozen=True, kw_only=True, tag=3):
    """Jobs submitted to Flamenco Manager."""

    script: ClassVar[str] = "rasm_render.js"
    kind: ClassVar[str] = PurePath(script).stem.replace("_", "-")
    server: Annotated[Endpoint, msgspec.Meta(title="Manager", description="Flamenco Manager address")] = Endpoint(url="http://localhost:8080")
    priority: Annotated[int, msgspec.Meta(description="Order among queued jobs"), Presentation(soft=Some((1.0, 100.0)))] = 50
    chunk: Annotated[int, msgspec.Meta(ge=1, le=sys.maxsize, title="Chunk Size", description="Frames each task renders"), Presentation(soft=Some((1.0, 4096.0)))] = 10
    paused: Annotated[bool, msgspec.Meta(title="Submit Paused", description="Jobs wait for start from Manager")] = False
    tag: Annotated[Gated[str], msgspec.Meta(title="Worker Tag", description="Workers taking jobs, every worker when off")] = Gated(enabled=False, value="")


class SharedStorageLocation(msgspec.Struct, frozen=True, kw_only=True):
    """Platform job storage path with Shaman state."""

    location: str
    shaman_enabled: bool = msgspec.field(name="shamanEnabled")


class AvailableJobType(msgspec.Struct, frozen=True, kw_only=True):
    """Job type Manager runs."""

    name: str
    etag: str


class AvailableJobTypes(msgspec.Struct, frozen=True, kw_only=True):
    """Job types Manager runs."""

    job_types: tuple[AvailableJobType, ...]


class WorkerTag(msgspec.Struct, frozen=True, kw_only=True):
    """Tag naming worker group."""

    name: str
    id: str | None = None
    description: str | None = None


class WorkerTagList(msgspec.Struct, frozen=True, kw_only=True):
    """Worker tags Manager holds."""

    tags: tuple[WorkerTag, ...] = ()


class Manager(msgspec.Struct, frozen=True, kw_only=True):
    """Manager state submission reads."""

    storage: SharedStorageLocation
    job_type: Option[AvailableJobType]
    tags: tuple[WorkerTag, ...]


class ShamanFileSpec(msgspec.Struct, frozen=True, kw_only=True):
    """File by SHA-256, size, and checkout path."""

    sha: str
    size: int
    path: str


class ShamanFileSpecWithStatus(ShamanFileSpec, frozen=True, kw_only=True):
    """File with Shaman server state."""

    status: ShamanFileStatus


class ShamanRequirementsRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Files a checkout needs."""

    files: tuple[ShamanFileSpec, ...]


class ShamanRequirementsResponse(msgspec.Struct, frozen=True, kw_only=True):
    """Requested files Shaman server lacks."""

    files: tuple[ShamanFileSpecWithStatus, ...]


class ShamanCheckout(msgspec.Struct, frozen=True, kw_only=True):
    """Files linked into checkout folder."""

    files: tuple[ShamanFileSpec, ...]
    checkout_path: str = msgspec.field(name="checkoutPath")


class ShamanCheckoutResult(msgspec.Struct, frozen=True, kw_only=True):
    """Checkout folder Manager created."""

    checkout_path: str = msgspec.field(name="checkoutPath")


class JobStorageInfo(msgspec.Struct, frozen=True, kw_only=True):
    """Shaman checkout holding job files."""

    shaman_checkout_id: str


class JobRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Opaque render command argument with step count."""

    argument: str
    steps: int


class JobTasks(msgspec.Struct, frozen=True, kw_only=True):
    """Frame requests of one blend file with prerequisite parts."""

    blendfile: str
    output_root: str
    requests: tuple[JobRequest, ...]
    prerequisites: tuple[int, ...]


class SubmittedJob(msgspec.Struct, frozen=True, kw_only=True):
    """Job posted to Manager."""

    name: str
    type: str
    priority: int
    submitter_platform: str
    settings: dict[str, str | tuple[JobTasks, ...]]
    initial_status: JobStatus
    type_etag: str | None = None
    worker_tag: str | None = None
    storage: JobStorageInfo | None = None


class ManagerJob(msgspec.Struct, frozen=True, kw_only=True):
    """Submitted job ID."""

    id: str


class ManagerFiles(msgspec.Struct, frozen=True, kw_only=True):
    """Manager path per source file with Shaman checkout."""

    files: dict[str, str]
    storage: Option[JobStorageInfo]


# --- [OPERATIONS] -----------------------------------------------------------------------


def queried(client: httpx2.Client) -> RenderingResult[Manager]:
    """Reads Manager state."""

    def built(found: tuple[tuple[SharedStorageLocation, AvailableJobTypes], WorkerTagList]) -> Manager:
        (storage, types), tags = found
        return Manager(storage=storage, job_type=Option.of_optional(next((type_ for type_ in types.job_types if type_.name == Flamenco.kind), None)), tags=tags.tags)

    return paired(
        paired(
            exchange(client, HTTPMethod.GET, f"/api/v3/configuration/shared-storage/users/{PLATFORM}", SharedStorageLocation), exchange(client, HTTPMethod.GET, "/api/v3/jobs/types", AvailableJobTypes)
        ),
        exchange(client, HTTPMethod.GET, "/api/v3/worker-mgt/tags", WorkerTagList),
    ).map(built)


def placed(client: httpx2.Client, manager: Manager, queue: RenderQueue) -> RenderingResult[ManagerFiles]:
    """Uploads blend files to Shaman when enabled, else copies them to Manager storage."""
    local = {PurePath(str(part), PurePath(job.file).name).as_posix(): job for part, job in {job.file: (part, job) for part, job in enumerate(queue.jobs)}.values()}
    files = local.items()
    folder = queue.stem

    def copied(name: str, job: Job) -> RenderingResult[tuple[str, str]]:
        relative = PurePath(folder, name)
        target = Path(manager.storage.location, relative)

        def copy() -> tuple[str, str]:
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(job.file, target)
            return job.file, f"{{jobs}}/{relative.as_posix()}"

        return catch(copy, exception=OSError)().map_error(lambda error: (RenderingWriteFault(request=target, error=error),))

    def read[T](source: str, reader: Callable[[Path], T]) -> RenderingResult[T]:
        return catch(lambda: reader(Path(source)), exception=OSError)().map_error(
            lambda error: (RenderingSendFault(request=source, error=424) if isinstance(error, FileNotFoundError) else RenderingReadFault(request=source, error=error),)
        )

    def specified(name: str, job: Job) -> RenderingResult[ShamanFileSpec]:
        return read(job.file, lambda path: path.stat().st_size).map(lambda size: ShamanFileSpec(sha=job.digest.hex(), size=size, path=name))

    def checked(specs: tuple[ShamanFileSpec, ...]) -> RenderingResult[ManagerFiles]:
        def uploaded(spec: ShamanFileSpecWithStatus) -> RenderingResult[httpx2.Response]:
            def unlisted() -> RenderingFaults:
                return (RenderingSendFault(request=spec.path, error=Cause.UNREADABLE),)

            return (
                Option
                .of_optional(local.get(spec.path))
                .to_result_with(unlisted)
                .bind(lambda job: read(job.file, Path.read_bytes))
                .bind(lambda data: sent(client, HTTPMethod.POST, f"/api/v3/shaman/files/{spec.sha}/{spec.size}", Some(data)))
            )

        return (
            exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/requirements", ShamanRequirementsResponse, Some(ShamanRequirementsRequest(files=specs)))
            .bind(lambda wanted: accumulated(uploaded(spec) for spec in wanted.files))
            .bind(lambda _: exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/create", ShamanCheckoutResult, Some(ShamanCheckout(files=specs, checkout_path=folder))))
            .map(
                lambda created: ManagerFiles(
                    files={job.file: f"{{jobs}}/{created.checkout_path}/{name}" for name, job in files}, storage=Some(JobStorageInfo(shaman_checkout_id=created.checkout_path))
                )
            )
        )

    return (
        accumulated(starmap(specified, files)).bind(checked)
        if manager.storage.shaman_enabled
        else accumulated(starmap(copied, files)).map(lambda pairs: ManagerFiles(files=dict(pairs), storage=Nothing))
    )


def submitted(client: httpx2.Client, target: Flamenco, queue: RenderQueue, render_command: str) -> RenderingResult[tuple[RenderingResult[str], ...]]:
    """Submits each prerequisite-joined job group as one Manager job."""

    def posted(stored: ManagerFiles, job_type: Option[AvailableJobType]) -> RenderingResult[tuple[RenderingResult[str], ...]]:
        components = DisjointSet(range(len(queue.jobs)))
        for part, job in enumerate(queue.jobs):
            for prerequisite in job.prerequisites:
                components.merge(part, prerequisite)

        def component(parts: set[int]) -> RenderingResult[str]:
            jobs = tuple((part, job) for part, job in enumerate(queue.jobs) if part in parts)
            positions = {part: index for index, (part, _) in enumerate(jobs)}
            tasks = tuple(
                (
                    job.output_root,
                    JobTasks(
                        blendfile=stored.files[job.file],
                        output_root=f"output_root_{part}",
                        requests=tuple(
                            JobRequest(argument=base64.b64encode(msgspec.json.encode(RenderFrames(scene=job.scene, frames=frames))).decode("ascii"), steps=len(frames))
                            for frames in batched(job.frames.numbers, target.chunk)
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
                ManagerJob,
                Some(
                    SubmittedJob(
                        name="; ".join(job.title for _, job in jobs),
                        type=Flamenco.kind,
                        type_etag=job_type.map(lambda kind: kind.etag).to_optional(),
                        priority=target.priority,
                        submitter_platform=PLATFORM,
                        settings={"jobs": tuple(task for _, task in tasks), "render_command": render_command, **{task.output_root: root for root, task in tasks}},
                        initial_status=JobStatus.PAUSED if target.paused else JobStatus.QUEUED,
                        worker_tag=target.tag.value if target.tag.enabled else None,
                        storage=stored.storage.to_optional(),
                    )
                ),
            ).map(lambda reply: reply.id)

        return Ok(tuple(map(component, components.subsets())))

    return queried(client).bind(lambda manager: placed(client, manager, queue).bind(lambda stored: posted(stored, manager.job_type)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AvailableJobType",
    "AvailableJobTypes",
    "Flamenco",
    "JobRequest",
    "JobStatus",
    "JobStorageInfo",
    "JobTasks",
    "Manager",
    "ManagerFiles",
    "ManagerJob",
    "ShamanCheckout",
    "ShamanCheckoutResult",
    "ShamanFileSpec",
    "ShamanFileSpecWithStatus",
    "ShamanFileStatus",
    "ShamanRequirementsRequest",
    "ShamanRequirementsResponse",
    "SharedStorageLocation",
    "SubmittedJob",
    "WorkerTag",
    "WorkerTagList",
    "placed",
    "queried",
    "submitted",
]
