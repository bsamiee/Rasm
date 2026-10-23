"""Render queue targets and sends."""

import base64
from pathlib import Path, PurePath
from typing import Annotated

from expression import Error, Ok
import msgspec

from blender.imaging.parameters import Case, Gated
from blender.rendering.batch.exchange import opened
from blender.rendering.batch.flamenco import Flamenco, submitted
from blender.rendering.batch.processes import detached, ran
from blender.rendering.batch.queue import RenderQueue, store
from blender.rendering.batch.supervision import Host, LocalMachine, Run, RunFile
from blender.rendering.faults import RenderingDependencyFault, RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile


# --- [MODELS] ---------------------------------------------------------------------------


class Renderon(Case, frozen=True, kw_only=True, tag=5):
    """Jobs written as B-Renderon queue."""

    title = "B-Renderon"
    executable: Annotated[Gated[str], msgspec.Meta(description="B-Renderon program, queue file alone when off")] = Gated(enabled=False, value="")
    blender: Annotated[str, msgspec.Meta(title="Blender Tag", description="B-Renderon name of rendering Blender")] = "Default"


class JobFile(Case, frozen=True, kw_only=True, tag=6):
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

    match target:
        case LocalMachine():
            return acknowledged(
                store(host.path(queue, RunFile.QUEUE), queue).bind(
                    lambda path: detached(
                        (host.binary, "--background", "--command", host.supervise_command, base64.b64encode(msgspec.json.encode(Run(queue=str(path), target=target))).decode("ascii")),
                        host.path(queue, RunFile.LOG),
                    )
                )
            )
        case Flamenco(server=server):
            return opened(server, lambda client: submitted(client, target, queue, host.render_command))
        case Renderon(executable=executable, blender=blender):
            if dependent := tuple(part for part, job in enumerate(queue.jobs) if job.prerequisites):
                return Error((RenderingDependencyFault(request=type(target), error=dependent),))
            lines = b"".join(
                msgspec.json.encode(QueueItem(folder=str(file.parent), file=file.name, blender=blender, scene=job.scene, start=str(start), end=str(end))) + b"\n"
                for job in queue.jobs
                for file in (PurePath(job.file),)
                for start, end in (((job.frames.start, job.frames.end),) if job.frames.step == 1 else tuple((frame, frame) for frame in job.frames.numbers))
            )
            return acknowledged(
                store(host.path(queue, RunFile.RENDERON), lines).bind(
                    lambda path: detached((executable.value, "-sq", str(path)), host.path(queue, RunFile.RENDERON_LOG)) if executable.enabled else Ok(path)
                )
            )
        case JobFile(folder=folder, command=command):
            path = (msgspec.structs.replace(host, folder=folder.value) if folder.enabled else host).path(queue, RunFile.QUEUE)
            return acknowledged(store(path, queue).bind(lambda written: ran((command.value, str(written))).map(lambda _: written) if command.enabled else Ok(written)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JobFile", "QueueItem", "Renderon", "Target", "send"]
