"""Render queue targets and sends."""

from pathlib import Path, PurePath
from typing import Annotated, Final

from expression import Error, Ok
from expression.extra.result import catch
import msgspec

from blender.records.faults import FileFault
from blender.records.fields import Case, Gated
from blender.records.stores import committed, RecordFile
from blender.rendering.batch.flamenco import Flamenco
from blender.rendering.batch.processes import detached, Host, ran, RunCommand, RunFile
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.batch.supervision import LocalMachine, Run
from blender.rendering.faults import RenderingDependencyFault, RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINES: Final = msgspec.json.Encoder()

# --- [MODELS] ---------------------------------------------------------------------------


class Renderon(Case, frozen=True, kw_only=True):
    """Jobs written as B-Renderon queue."""

    title = "B-Renderon"
    executable: Annotated[Gated[str], msgspec.Meta(description="B-Renderon program, queue file alone when off")] = Gated(enabled=False, value="")
    blender: Annotated[str, msgspec.Meta(title="Blender Tag", description="B-Renderon name of rendering Blender")] = "Default"


class JobFile(Case, frozen=True, kw_only=True):
    """Queue job file passed to submission command."""

    folder: Annotated[Gated[str], msgspec.Meta(description="Job file folder, extension folder when off")] = Gated(enabled=False, value="")
    command: Annotated[Gated[str], msgspec.Meta(description="Program taking job file path, job file alone when off")] = Gated(enabled=False, value="")


class QueueItem(msgspec.Struct, frozen=True, kw_only=True):
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def send(target: Target, queue: RenderQueue, host: Host) -> RenderingResult[tuple[RenderingResult[str], ...]]:
    """Sends queue, returning each path or farm job ID."""

    def acknowledged(found: RenderingResult[Path]) -> RenderingResult[tuple[RenderingResult[str], ...]]:
        return found.map(lambda path: (Ok(str(path)),))

    def queued(path: Path) -> RenderingResult[Path]:
        return RecordFile(path=path, form=RenderQueue).written(queue)

    def lined(path: Path, items: tuple[QueueItem, ...]) -> RenderingResult[Path]:
        lines = _LINES.encode_lines(items)
        return committed(path, lambda stage: catch(stage.write_bytes, exception=OSError)(lines).map_error(lambda error: (FileFault(access="write", request=path, error=error),)))

    match target:
        case LocalMachine():
            return acknowledged(
                queued(host.path(queue, RunFile.QUEUE)).bind(lambda path: detached(host.argv(RunCommand.SUPERVISE, Run(queue=str(path), target=target)), host.path(queue, RunFile.LOG)))
            )
        case Flamenco(server=server):
            return server.opened(lambda client: target.submitted(client, queue, host.commands[RunCommand.RENDER]))
        case Renderon(executable=executable, blender=blender):
            if dependent := tuple(part for part, job in enumerate(queue.jobs) if job.prerequisites):
                return Error((RenderingDependencyFault(request=type(target), error=dependent),))
            items = tuple(
                QueueItem(folder=str(file.parent), file=file.name, blender=blender, scene=job.scene, start=str(start), end=str(end))
                for job in queue.jobs
                for file in (PurePath(job.file),)
                for start, end in (((job.frames.start, job.frames.end),) if job.frames.step == 1 else tuple((frame, frame) for frame in job.frames.numbers))
            )
            return acknowledged(
                lined(host.path(queue, RunFile.RENDERON), items).bind(
                    lambda path: detached((executable.value, "-sq", str(path)), host.path(queue, RunFile.RENDERON_LOG)) if executable.enabled else Ok(path)
                )
            )
        case JobFile(folder=folder, command=command):
            path = (msgspec.structs.replace(host, folder=folder.value) if folder.enabled else host).path(queue, RunFile.QUEUE)
            return acknowledged(queued(path).bind(lambda written: ran((command.value, str(written))).map(lambda _: written) if command.enabled else Ok(written)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JobFile", "QueueItem", "Renderon", "Target", "send"]
