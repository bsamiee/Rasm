"""Dispatch targets of a render queue as a closed union and the send over it."""

import base64
from pathlib import PurePath
from typing import Annotated

from expression import Error, Ok
import msgspec

from rasm.imaging.parameters import Case, Gated
from rasm.rendering.batch.exchange import opened
from rasm.rendering.batch.flamenco import Flamenco, submitted
from rasm.rendering.batch.processes import detached, ran
from rasm.rendering.batch.queue import RenderQueue, store
from rasm.rendering.batch.supervision import Host, LocalMachine, Run, RunFile
from rasm.rendering.faults import RenderingDependencyFault, RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Target = LocalMachine | Flamenco | Renderon | JobFile


# --- [MODELS] ---------------------------------------------------------------------------


class Renderon(Case, frozen=True, kw_only=True, tag=5):
    """Jobs written as a B-Renderon queue and opened in B-Renderon."""

    title = "B-Renderon"
    executable: Annotated[Gated[str], msgspec.Meta(description="B-Renderon program opening the queue, the queue file written alone while off")] = Gated(enabled=False, value="")
    blender: Annotated[str, msgspec.Meta(title="Blender Tag", description="B-Renderon's name for the Blender that renders")] = "Default"


class JobFile(Case, frozen=True, kw_only=True, tag=6):
    """Queue written as a job file and passed to a submission command when one is set."""

    title = "Job File"
    folder: Annotated[Gated[str], msgspec.Meta(description="Folder of the job file, the extension folder while off")] = Gated(enabled=False, value="")
    command: Annotated[Gated[str], msgspec.Meta(description="Submission program run with the job file's path as its argument, the job file written alone while off")] = Gated(enabled=False, value="")


class QueueItem(msgspec.Struct, frozen=True, kw_only=True):
    """One job as a B-Renderon queue line."""

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
    """Sends a queue to a target."""
    match target:
        case LocalMachine():
            return (
                store(host.path(queue, RunFile.QUEUE), msgspec.json.encode(queue, order="deterministic"))
                .bind(
                    lambda path: detached(
                        (host.binary, "--background", "--command", host.supervise_command, base64.b64encode(msgspec.json.encode(Run(queue=str(path), target=target))).decode("ascii")),
                        host.path(queue, RunFile.LOG),
                    )
                )
                .map(lambda log: (Ok(str(log)),))
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
            return store(host.path(queue, RunFile.RENDERON), lines).bind(
                lambda path: detached((executable.value, "-sq", str(path)), host.path(queue, RunFile.RENDERON_LOG)).map(lambda log: (Ok(str(log)),)) if executable.enabled else Ok((Ok(str(path)),))
            )
        case JobFile(folder=folder, command=command):
            path = (msgspec.structs.replace(host, folder=folder.value) if folder.enabled else host).path(queue, RunFile.QUEUE)
            return store(path, msgspec.json.encode(queue, order="deterministic")).bind(
                lambda written: ran((command.value, str(written))).map(lambda _: (Ok(str(written)),)) if command.enabled else Ok((Ok(str(written)),))
            )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["JobFile", "QueueItem", "Renderon", "Target", "send"]
