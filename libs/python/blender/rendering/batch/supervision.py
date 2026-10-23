"""Local queue supervision in background Blender processes."""

import base64
import datetime
from enum import StrEnum
from fractions import Fraction
from graphlib import TopologicalSorter
from heapq import heappop, heappush
from itertools import groupby
import os
from pathlib import Path
import signal
import subprocess
import sys
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
from tenacity import retry, retry_if_result, stop_after_attempt

from blender.imaging.faults import accumulated, scoped
from blender.imaging.parameters import Case, Presentation
from blender.rendering.batch.processes import ran, refused, spawned
from blender.rendering.batch.progress import Ending, Heartbeat, observed, Progress
from blender.rendering.batch.queue import RenderQueue, revised, store
from blender.rendering.faults import Call, RenderingFaults, RenderingResult, RenderingSystemFault, RenderingWaitFault, RenderingWriteFault

if sys.platform == "win32":
    import _winapi
else:
    from os import killpg  # ty: ignore[possibly-missing-import]

# --- [TYPES] ----------------------------------------------------------------------------


class RunFile(StrEnum):
    """Run file suffix after queue key."""

    QUEUE = ".json"
    PROGRESS = ".progress.json"
    LOG = ".log"
    RENDERON = ".renderon.json"
    RENDERON_LOG = ".renderon.log"


# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True, kw_only=True):
    """Running host binary, extension command names, and folder."""

    binary: str
    supervise_command: str
    render_command: str
    folder: str

    def path(self, queue: RenderQueue, file: RunFile) -> Path:
        """Returns path of queue run file."""
        return Path(self.folder, f"{queue.stem}{file}")


class LocalMachine(Case, frozen=True, kw_only=True, tag=4):
    """Background Blender process per run of job frames."""

    title = "This Machine"
    retries: Annotated[int, msgspec.Meta(ge=0, description="Restarts of crashed render from remaining frames"), Presentation(soft=Some((0.0, 20.0)))] = 3
    sampled: Annotated[bool, msgspec.Meta(title="Sample First Frames", description="Render first two frames of each ready job before others")] = True

    def schedule(self, queue: RenderQueue) -> tuple[tuple[int, int], ...]:
        """Orders frames by prerequisites, sampled frames, job, then frame."""
        graph = TopologicalSorter[tuple[int, int]]()
        priorities: dict[tuple[int, int], tuple[bool, int, int]] = {}
        lead = 2 if self.sampled else 0
        for part, job in enumerate(queue.jobs):
            frames = job.frames.numbers
            for offset, frame in enumerate(frames):
                step = part, frame
                predecessors = ((part, frames[offset - 1]),) if offset else tuple((before, queue.jobs[before].frames.numbers[-1]) for before in job.prerequisites)
                graph.add(step, *predecessors)
                priorities[step] = offset >= lead, part, offset
        graph.prepare()
        ready: list[tuple[tuple[bool, int, int], tuple[int, int]]] = []
        steps: list[tuple[int, int]] = []
        while graph.is_active():
            for step in graph.get_ready():
                heappush(ready, (priorities[step], step))
            _, step = heappop(ready)
            steps.append(step)
            graph.done(step)
        return tuple(steps)


class Run(msgspec.Struct, frozen=True, kw_only=True, tag=1):
    """Queue path and target settings commands decode."""

    queue: str
    target: LocalMachine


# --- [OPERATIONS] -----------------------------------------------------------------------


def supervise(target: LocalMachine, queue: RenderQueue, host: Host, heartbeat: Heartbeat) -> RenderingResult[Progress]:
    """Renders remaining frames, restarting crashed renders."""
    milliseconds_per_second = 1000
    if sys.platform == "win32":
        infinite = _winapi.INFINITE  # ty: ignore[possibly-missing-attribute]
        if Fraction(heartbeat.interval) * milliseconds_per_second >= infinite or heartbeat.interval * milliseconds_per_second >= infinite:
            return Error((RenderingWaitFault(request=heartbeat.interval, error=infinite),))
    records = host.path(queue, RunFile.PROGRESS)
    run = base64.b64encode(msgspec.json.encode(Run(queue=str(host.path(queue, RunFile.QUEUE)), target=target))).decode("ascii")

    def crashed(answer: RenderingResult[int]) -> bool:
        return answer.map(lambda code: code not in {0, 1}).default_value(False)

    def rendered(progress: Progress) -> RenderingResult[None]:
        schedule = target.schedule(queue)
        while True:
            remaining = frozenset(progress.remaining(queue))
            if not (pending := (progress.pending or next((tuple(group) for _, group in groupby((step for step in schedule if step in remaining), lambda step: step[0])), ()))):
                return Ok(None)
            argv = host.binary, "--background", queue.jobs[pending[0][0]].file, "--command", host.render_command, run
            recorded = msgspec.structs.replace(progress, pending=pending)

            def waited(child: subprocess.Popen[bytes]) -> RenderingResult[int]:
                while True:
                    try:
                        return Ok(child.wait(timeout=heartbeat.interval))
                    except subprocess.TimeoutExpired:
                        try:
                            records.touch()
                        except OSError as error:
                            return Error((RenderingWriteFault(request=records, error=error),))
                    except OSError as error:
                        return Error(refused(argv)(error))

            @retry(stop=stop_after_attempt(target.retries + 1), retry=retry_if_result(crashed), retry_error_callback=lambda state: state.outcome.result())  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            def launched(_: Path) -> RenderingResult[int]:
                return scoped(spawned(argv), waited, lambda child: catch(child.wait, exception=OSError)().map_error(refused(argv)))

            def confirmed(code: int) -> RenderingResult[Progress]:
                return revised(records, Progress, lambda held: msgspec.structs.replace(held, pending=())) if code == 0 else Error(refused(argv)(code))

            match store(records, recorded).bind(launched).bind(confirmed):
                case Result(tag="ok", ok=progress):
                    continue
                case Result(error=faults):
                    return Error(faults)

    def ended(ending: Ending) -> RenderingResult[Progress]:
        return revised(records, Progress, lambda held: msgspec.structs.replace(held, ending=ending))

    def completed(progress: Progress) -> RenderingResult[Progress]:
        def failed(faults: RenderingFaults) -> RenderingResult[Progress]:
            return Error((faults[0], *faults[1:], *ended(Ending.FAILED).swap().default_value(())))

        return rendered(progress).bind(lambda _: ended(Ending.SUCCEEDED)).or_else_with(failed)

    def resumed(found: Option[tuple[Progress, datetime.datetime]]) -> RenderingResult[Progress]:
        match found.to_optional():
            case (previous, _):
                held: Option[Progress] = Some(previous)
            case None:
                held = Nothing
        progress = Progress.begun(held, queue, os.getpid())
        return store(records, progress).bind(lambda _: completed(progress))

    return observed(records).bind(resumed)


def stop(progress: Progress) -> RenderingResult[None]:
    """Stops supervisor and its renders."""
    if sys.platform == "win32":
        return ran(("taskkill", "/T", "/F", "/PID", str(progress.process)))
    return catch(lambda: killpg(progress.process, signal.SIGTERM), exception=OSError)().map_error(lambda error: (RenderingSystemFault(request=(Call.KILL_GROUP, progress.process), error=error),))


def cleared(queue: RenderQueue, host: Host) -> RenderingResult[None]:
    """Deletes queue, progress, and log files of finished run."""

    def removed(file: RunFile) -> RenderingResult[None]:
        path = host.path(queue, file)
        return catch(lambda: path.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (RenderingWriteFault(request=path, error=error),))

    return accumulated(map(removed, (RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Host", "LocalMachine", "Run", "RunFile", "cleared", "stop", "supervise"]
