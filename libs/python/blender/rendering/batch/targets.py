"""Render queue targets and sends."""

from pathlib import PurePath
from typing import Final

from expression import Error, Ok, Option
import msgspec

from blender.records.faults import Fault, Outcome
from blender.records.fields import Case
from blender.records.stores import committed, RecordFile
from blender.rendering.batch.flamenco import Flamenco
from blender.rendering.batch.processes import detached, HostCommands, ran, RunFile
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.batch.supervision import LocalMachine
from blender.rendering.faults import RenderingReason

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINES: Final = msgspec.json.Encoder()

# --- [MODELS] ---------------------------------------------------------------------------


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
            return Error(Fault.of(RenderingReason.PREREQUISITE, type(self).__name__, member=", ".join(map(str, dependent))))
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JobFile", "Renderon", "Target"]
