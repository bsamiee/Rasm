"""Render queue targets: HTTP exchanges inside one client scope, Flamenco Manager submission, B-Renderon queues, and job files."""

from collections.abc import Callable, Mapping
import datetime
from functools import partial
from http import HTTPMethod
from itertools import batched, starmap
from pathlib import Path, PurePath, PurePosixPath
import platform
import sys
from typing import Annotated, ClassVar, Final, Literal, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import httpx2
import msgspec
from scipy.cluster.hierarchy import DisjointSet

from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case
from blender.records.stores import argument, committed, files_copied, RecordFile
from blender.rendering.batch.queue import PackedFile, RenderFrames, RenderQueue
from blender.rendering.batch.supervision import detached, HostCommands, LocalMachine, ran, RunCommand, RunFile

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile

# --- [CONSTANTS] ------------------------------------------------------------------------

_PLATFORM: Final = platform.system().lower()
_LINES: Final = msgspec.json.Encoder()

# --- [MODELS] ---------------------------------------------------------------------------

# --- [EXCHANGE]


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address with socket timeout."""

    url: str
    timeout: datetime.timedelta = datetime.timedelta(seconds=10)

    def __post_init__(self) -> None:
        """Validates a timeout above zero."""
        if self.timeout <= datetime.timedelta():
            raise ValueError(self.timeout)

    def opened[T](self, operation: Callable[[httpx2.Client], Outcome[T]]) -> Outcome[T]:
        """Runs operation inside one client scope, the client closing on every path."""

        def held(url: httpx2.URL) -> Outcome[T]:
            with httpx2.Client(base_url=url, timeout=self.timeout.total_seconds()) as client:
                return operation(client)

        return catch(httpx2.URL, exception=httpx2.InvalidURL)(self.url).map_error(partial(Fault.of, Reason.FAILED, self.url)).bind(held)


# --- [FLAMENCO]


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
    """Tag naming worker group, jobs selecting it by ID."""

    id: str
    name: str


class _WorkerTagList(msgspec.Struct, frozen=True, kw_only=True):
    """Worker tags Manager holds."""

    tags: tuple[_WorkerTag, ...] = ()


class _ShamanFileSpec(msgspec.Struct, frozen=True, kw_only=True):
    """File by SHA-256, size, and checkout path."""

    sha: str
    size: int
    path: str


class _Checkout(msgspec.Struct, frozen=True, kw_only=True, rename="camel", omit_defaults=True):
    """Shaman checkout: the files it needs or the server lacks, and the folder they link into once created."""

    files: tuple[_ShamanFileSpec, ...] = ()
    checkout_path: str = ""


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
        ).map(lambda found: cls(storage=found[0], job_type=Block(found[1].job_types).filter(lambda kind: kind.name == Flamenco.job_type).try_head(), tags=found[2].tags))

    def worker_tag(self, tag: str, /) -> Outcome[str]:
        """Returns the ID of the worker tag named or identified by `tag`."""
        return (
            Block(self.tags)
            .filter(lambda held: tag in {held.name, held.id})
            .try_head()
            .map(lambda held: held.id)
            .to_result_with(lambda: Fault.of(Reason.ABSENT, type(self).__name__, member=f"worker tag {tag!r}"))
        )

    def placed(self, client: httpx2.Client, queue: RenderQueue) -> Outcome[tuple[Mapping[str, str], Option[_JobStorageInfo]]]:
        """Uploads every job's pack to Shaman when enabled, else copies it to Manager storage, each pack under its job's part folder, returning the Manager path of each blend and the Shaman checkout."""
        jobs = {job.file: (part, job) for part, job in enumerate(queue.jobs)}.values()
        entries = {PurePosixPath(str(part), placed.path).as_posix(): placed for part, job in jobs for placed in job.placements}
        folder = queue.stem

        def stored(root: str) -> Mapping[str, str]:
            return {job.file: f"{{jobs}}/{root}/{part}/{job.blend.path}" for part, job in jobs}

        def read[T](source: str, reader: Callable[[Path], T]) -> Outcome[T]:
            return catch(reader, exception=OSError)(Path(source)).map_error(partial(Fault.of, Reason.UNREADABLE, source))

        def specified(name: str, placed: PackedFile) -> Outcome[_ShamanFileSpec]:
            return read(placed.source, Path.stat).map(lambda stat: _ShamanFileSpec(sha=placed.digest.hex(), size=stat.st_size, path=name))

        def checked(specs: tuple[_ShamanFileSpec, ...]) -> Outcome[tuple[Mapping[str, str], Option[_JobStorageInfo]]]:
            def uploaded(spec: _ShamanFileSpec, placed: PackedFile) -> Outcome[httpx2.Response]:
                return read(placed.source, Path.read_bytes).bind(lambda data: sent(client, HTTPMethod.POST, f"/api/v3/shaman/files/{spec.sha}/{spec.size}", Some(data)))

            return (
                exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/requirements", _Checkout, Some(_Checkout(files=specs)))
                .map(lambda wanted: frozenset(spec.path for spec in wanted.files))
                .bind(lambda lacking: accumulated(uploaded(spec, placed) for spec, placed in zip(specs, entries.values(), strict=True) if spec.path in lacking))
                .bind(lambda _: exchange(client, HTTPMethod.POST, "/api/v3/shaman/checkout/create", _Checkout, Some(_Checkout(files=specs, checkout_path=folder))))
                .map(lambda created: (stored(created.checkout_path), Some(_JobStorageInfo(shaman_checkout_id=created.checkout_path))))
            )

        return (
            accumulated(starmap(specified, entries.items())).bind(checked)
            if self.storage.shaman_enabled
            else files_copied((Path(placed.source), Path(self.storage.location, folder, name)) for name, placed in entries.items()).map(lambda _: (stored(folder), Nothing))
        )


# --- [TARGETS]


class Flamenco(Case, frozen=True, kw_only=True):
    """Jobs submitted to Flamenco Manager."""

    script: ClassVar[str] = "rasm_render.js"
    job_type: ClassVar[str] = PurePath(script).stem.replace("_", "-")
    server: Endpoint = Endpoint(url="http://localhost:8080")
    priority: int = 50
    chunk: Annotated[int, msgspec.Meta(ge=1, le=sys.maxsize)] = 10
    paused: bool = False
    tag: str | None = None

    def submitted(self, client: httpx2.Client, queue: RenderQueue, render_command: str) -> Outcome[tuple[Outcome[str], ...]]:
        """Submits each prerequisite-joined job group as one Manager job."""

        def posted(manager: Manager, tag: Option[str], files: Mapping[str, str], storage: Option[_JobStorageInfo]) -> tuple[Outcome[str], ...]:
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
                            blendfile=files[job.file],
                            output_root=f"output_root_{part}",
                            requests=tuple(_JobRequest(argument=argument(RenderFrames(scene=job.scene, frames=frames)), steps=len(frames)) for frames in batched(job.frames.numbers, self.chunk)),
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
                            type=Flamenco.job_type,
                            type_etag=manager.job_type.map(lambda kind: kind.etag).to_optional(),
                            priority=self.priority,
                            submitter_platform=_PLATFORM,
                            settings={"jobs": tuple(task for _, task in tasks), "render_command": render_command, **{task.output_root: root for root, task in tasks}},
                            initial_status="paused" if self.paused else "queued",
                            worker_tag=tag.to_optional(),
                            storage=storage.to_optional(),
                        )
                    ),
                ).map(lambda reply: reply.id)

            return tuple(map(component, components.subsets()))

        return Manager.queried(client).bind(
            lambda manager: (
                Option
                .of_optional(self.tag)
                .map(lambda tag: manager.worker_tag(tag).map(Some))
                .default_value(Ok(Nothing))
                .bind(lambda tag: manager.placed(client, queue).map(lambda found: posted(manager, tag, *found)))
            )
        )

    def sent(self, queue: RenderQueue, host: HostCommands) -> Outcome[tuple[Outcome[str], ...]]:
        """Submits the queue through one Manager client scope with the host render command, returning each Manager job ID."""
        return self.server.opened(lambda client: self.submitted(client, queue, host.commands[RunCommand.RENDER]))


class _QueueItem(msgspec.Struct, frozen=True, kw_only=True):
    """Job as B-Renderon queue line."""

    folder: str = msgspec.field(name="ruta_blend")
    file: str = msgspec.field(name="nombre_blend")
    blender: str = msgspec.field(name="tag_blender")
    mode: str = msgspec.field(default="Animation", name="modo")
    scene: str = msgspec.field(name="escena")
    layer: str = msgspec.field(default="", name="view_layer")
    camera: str = msgspec.field(default="", name="camara")
    start: str = msgspec.field(name="inicio")
    end: str = msgspec.field(name="fin")
    step: str = ""
    arguments: str = msgspec.field(default="", name="args_extra")
    frames: str = ""
    script: str = ""
    overrides: tuple[str, ...] = msgspec.field(default=("escena",), name="propiedades_argumentar")


class Renderon(Case, frozen=True, kw_only=True):
    """Jobs written as B-Renderon queue."""

    executable: str | None = None
    blender: str = "Default"

    def sent(self, queue: RenderQueue, host: HostCommands) -> Outcome[tuple[Outcome[str], ...]]:
        """Writes one queue line per frame range, then starts B-Renderon on the lines when executable is set, returning the queue path."""
        if dependent := tuple(part for part, job in enumerate(queue.jobs) if job.prerequisites):
            return Error(Fault.of(Reason.INVALID, type(self).__name__, f"prerequisites of queue parts {', '.join(map(str, dependent))} cannot be enforced"))
        path = host.path(queue.stem, RunFile.RENDERON)
        lines = _LINES.encode_lines(
            _QueueItem(folder=str(file.parent), file=file.name, blender=self.blender, scene=job.scene, start=str(start), end=str(end))
            for job in queue.jobs
            for file in (PurePath(job.file),)
            for start, end in (((job.frames.start, job.frames.end),) if job.frames.step == 1 else tuple((frame, frame) for frame in job.frames.numbers))
        )
        return (
            committed(path, lines)
            .bind(
                lambda written: (
                    Option.of_optional(self.executable).map(lambda program: detached((program, "-sq", str(written)), host.path(queue.stem, RunFile.RENDERON_LOG))).default_value(Ok(written))
                )
            )
            .map(lambda _: (Ok(str(path)),))
        )


class JobFile(Case, frozen=True, kw_only=True):
    """Queue job file passed to submission command."""

    folder: str | None = None
    command: str | None = None

    def sent(self, queue: RenderQueue, host: HostCommands) -> Outcome[tuple[Outcome[str], ...]]:
        """Writes queue file into folder when set, then passes the file to command when set, returning the queue file path."""
        path = Option.of_optional(self.folder).map(lambda held: msgspec.structs.replace(host, folder=held)).default_value(host).path(queue.stem, RunFile.QUEUE)
        return (
            RecordFile(path=path, form=RenderQueue)
            .written(queue)
            .bind(lambda written: Option.of_optional(self.command).map(lambda program: ran((program, str(written))).map(lambda _: written)).default_value(Ok(written)))
            .map(lambda written: (Ok(str(written)),))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def sent(client: httpx2.Client, method: HTTPMethod, path: str, body: Option[msgspec.Struct | bytes] = Nothing) -> Outcome[httpx2.Response]:
    """Sends request, non-success status as fault."""
    match body.to_optional():
        case msgspec.Struct() as content:
            request = client.build_request(method, path, content=msgspec.json.encode(content), headers={"Content-Type": "application/json"})
        case bytes(content):
            request = client.build_request(method, path, content=content, headers={"Content-Type": "application/octet-stream"})
        case None:
            request = client.build_request(method, path)
    url = str(request.url)
    return (
        catch(client.send, exception=httpx2.RequestError)(request)
        .map_error(partial(Fault.of, Reason.FAILED, url))
        .filter_with(lambda response: response.is_success, lambda response: Fault.of(Reason.ANSWERED, url, member=f"HTTP {response.status_code}"))
    )


def exchange[T](client: httpx2.Client, method: HTTPMethod, path: str, reply: type[T], body: Option[msgspec.Struct | bytes] = Nothing) -> Outcome[T]:
    """Sends request and decodes JSON reply."""
    return sent(client, method, path, body).bind(
        lambda response: catch(msgspec.json.decode, exception=msgspec.DecodeError)(response.content, type=reply).map_error(partial(Fault.of, Reason.FAILED, str(response.request.url)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "Flamenco", "JobFile", "Manager", "Renderon", "Target", "exchange", "sent"]
