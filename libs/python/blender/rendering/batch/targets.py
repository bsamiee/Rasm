"""Render queue targets and sends."""

from pathlib import Path, PurePath
from typing import Annotated, Final

from expression import Error, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed, RecordFile
from blender.rendering.batch.flamenco import Flamenco
from blender.rendering.batch.processes import detached, Host, ran, RunCommand, RunFile
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.batch.supervision import LocalMachine, Run
from blender.rendering.faults import RenderingReason

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINES: Final = msgspec.json.Encoder()

# --- [MODELS] ---------------------------------------------------------------------------


class Renderon(Case, frozen=True, kw_only=True):
    """Jobs written as B-Renderon queue."""

    executable: Annotated[str | None, msgspec.Meta(description="B-Renderon program, queue file alone when absent")] = None
    blender: Annotated[str, msgspec.Meta(description="B-Renderon name of rendering Blender")] = "Default"


class JobFile(Case, frozen=True, kw_only=True):
    """Queue job file passed to submission command."""

    folder: Annotated[str | None, msgspec.Meta(description="Job file folder, extension folder when absent")] = None
    command: Annotated[str | None, msgspec.Meta(description="Program taking job file path, job file alone when absent")] = None


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


def send(target: Target, queue: RenderQueue, host: Host) -> Outcome[tuple[Outcome[str], ...]]:
    """Sends queue, returning each path or farm job ID."""

    def acknowledged(found: Outcome[Path]) -> Outcome[tuple[Outcome[str], ...]]:
        return found.map(lambda path: (Ok(str(path)),))

    def queued(path: Path) -> Outcome[Path]:
        return RecordFile(path=path, form=RenderQueue).written(queue)

    def lined(path: Path, items: tuple[QueueItem, ...]) -> Outcome[Path]:
        lines = _LINES.encode_lines(items)
        return committed(path, lambda stage: catch(stage.write_bytes, exception=OSError)(lines).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(path), cause=error),)))

    match target:
        case LocalMachine():
            return acknowledged(
                queued(host.path(queue, RunFile.QUEUE)).bind(lambda path: detached(host.argv(RunCommand.SUPERVISE, Run(queue=str(path), target=target)), host.path(queue, RunFile.LOG)))
            )
        case Flamenco(server=server):
            return server.opened(lambda client: target.submitted(client, queue, host.commands[RunCommand.RENDER]))
        case Renderon(executable=executable, blender=blender):
            if dependent := tuple(part for part, job in enumerate(queue.jobs) if job.prerequisites):
                return Error((Fault(reason=RenderingReason.PREREQUISITE, subject=type(target).__name__, member=", ".join(map(str, dependent))),))
            items = tuple(
                QueueItem(folder=str(file.parent), file=file.name, blender=blender, scene=job.scene, start=str(start), end=str(end))
                for job in queue.jobs
                for file in (PurePath(job.file),)
                for start, end in (((job.frames.start, job.frames.end),) if job.frames.step == 1 else tuple((frame, frame) for frame in job.frames.numbers))
            )
            return acknowledged(
                lined(host.path(queue, RunFile.RENDERON), items).bind(
                    lambda path: Option.of_optional(executable).map(lambda program: detached((program, "-sq", str(path)), host.path(queue, RunFile.RENDERON_LOG))).default_value(Ok(path))
                )
            )
        case JobFile(folder=folder, command=command):
            path = Option.of_optional(folder).map(lambda held: msgspec.structs.replace(host, folder=held)).default_value(host).path(queue, RunFile.QUEUE)
            return acknowledged(queued(path).bind(lambda written: Option.of_optional(command).map(lambda program: ran((program, str(written))).map(lambda _: written)).default_value(Ok(written))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JobFile", "QueueItem", "Renderon", "Target", "send"]
