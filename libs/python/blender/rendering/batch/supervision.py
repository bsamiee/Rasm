"""Local supervision of a render queue in background Blender processes, its stop, and its files."""

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

from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Presentation
from blender.rendering.batch.processes import ran
from blender.rendering.batch.progress import Ending, Heartbeat, observed, Progress
from blender.rendering.batch.queue import load, RenderQueue, store
from blender.rendering.faults import Call, RenderingFaults, RenderingResult, RenderingRunFault, RenderingSystemFault, RenderingWaitFault, RenderingWriteFault

if sys.platform == "win32":
    import _winapi
else:
    from os import killpg  # ty: ignore[possibly-missing-import]

# --- [TYPES] ----------------------------------------------------------------------------


class RunFile(StrEnum):
    """File one queue's run writes, by its suffix after the queue key."""

    QUEUE = ".json"
    PROGRESS = ".progress.json"
    LOG = ".log"
    RENDERON = ".renderon.json"
    RENDERON_LOG = ".renderon.log"


# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True, kw_only=True):
    """Blender binary, the extension's supervise and render command names, and its folder, read from the running host."""

    binary: str
    supervise_command: str
    render_command: str
    folder: str

    def path(self, queue: RenderQueue, file: RunFile) -> Path:
        """Answers the path of one of a queue's run files."""
        return Path(self.folder, f"{queue.stem}{file}")


class LocalMachine(Case, frozen=True, kw_only=True, tag=4):
    """Background Blender processes on this machine, one per run of a job's frames."""

    title = "This Machine"
    retries: Annotated[int, msgspec.Meta(ge=0, description="Restarts of a crashed render from its remaining frames"), Presentation(soft=Some((0.0, 20.0)))] = 3
    sampled: Annotated[bool, msgspec.Meta(title="Sample First Frames", description="Render the first two frames of each ready job before remaining ready work")] = True

    def schedule(self, queue: RenderQueue) -> tuple[tuple[int, int], ...]:
        """Orders ready frames by sampled priority after their prerequisites."""
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
    """Job file and target settings the supervise and render commands decode."""

    queue: str
    target: LocalMachine


# --- [OPERATIONS] -----------------------------------------------------------------------


def supervise(target: LocalMachine, queue: RenderQueue, host: Host, heartbeat: Heartbeat) -> RenderingResult[Progress]:
    """Renders a queue's remaining frames in background Blender processes."""
    milliseconds_per_second = 1000
    if sys.platform == "win32":
        infinite = _winapi.INFINITE  # ty: ignore[possibly-missing-attribute]
        if Fraction(heartbeat.interval) * milliseconds_per_second >= infinite or heartbeat.interval * milliseconds_per_second >= infinite:
            return Error((RenderingWaitFault(request=heartbeat.interval, error=infinite),))
    records = host.path(queue, RunFile.PROGRESS)
    run = base64.b64encode(msgspec.json.encode(Run(queue=str(host.path(queue, RunFile.QUEUE)), target=target))).decode("ascii")

    def waited(child: subprocess.Popen[bytes], argv: tuple[str, ...]) -> RenderingResult[int]:
        def finished() -> RenderingResult[int]:
            while True:
                try:
                    return Ok(child.wait(timeout=heartbeat.interval))
                except subprocess.TimeoutExpired:
                    try:
                        records.touch()
                    except OSError as error:
                        return Error((RenderingWriteFault(request=records, error=error),))
                except OSError as error:
                    return Error((RenderingRunFault(request=argv, error=error),))

        try:
            answer = finished()
        finally:
            reaped: RenderingResult[int] = catch(child.wait, exception=OSError)().map_error(lambda error: (RenderingRunFault(request=argv, error=error),))
        return paired(answer, reaped).bind(lambda _: answer)

    def crashed(answer: RenderingResult[int]) -> bool:
        return answer.map(lambda code: code not in {0, 1}).default_value(False)

    def rendered(progress: Progress) -> RenderingResult[None]:
        schedule = target.schedule(queue)
        while True:
            remaining = frozenset(progress.remaining(queue))
            if not (pending := (progress.pending or next((tuple(group) for _, group in groupby((step for step in schedule if step in remaining), lambda step: step[0])), ()))):
                return Ok(None)
            argv = host.binary, "--background", queue.jobs[pending[0][0]].file, "--command", host.render_command, run
            admitted = msgspec.structs.replace(progress, pending=pending)

            @retry(stop=stop_after_attempt(target.retries + 1), retry=retry_if_result(crashed), retry_error_callback=lambda state: state.outcome.result())  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            def launched(_: Path) -> RenderingResult[int]:
                try:
                    child: subprocess.Popen[bytes] = subprocess.Popen(argv)
                except OSError as error:
                    return Error((RenderingRunFault(request=argv, error=error),))
                return waited(child, argv)

            def confirmed(code: int) -> RenderingResult[tuple[int, Progress]]:
                return (
                    load(records, Progress)
                    .map(lambda held: msgspec.structs.replace(held, pending=()) if code == 0 else held)
                    .bind(lambda held: store(records, msgspec.json.encode(held, order="deterministic")).map(lambda _: (code, held)) if code == 0 else Ok((code, held)))
                )

            match store(records, msgspec.json.encode(admitted, order="deterministic")).bind(launched).bind(confirmed):
                case Result(tag="ok", ok=(0, progress)):
                    continue
                case Result(tag="ok", ok=(code, _)):
                    return Error((RenderingRunFault(request=argv, error=code),))
                case Result(error=faults):
                    return Error(faults)

    def ended(ending: Ending) -> RenderingResult[Progress]:
        return (
            load(records, Progress)
            .map(lambda held: msgspec.structs.replace(held, ending=ending))
            .bind(lambda held: store(records, msgspec.json.encode(held, order="deterministic")).map(lambda _: held))
        )

    def completed(progress: Progress) -> RenderingResult[Progress]:
        def failed(faults: RenderingFaults) -> RenderingResult[Progress]:
            answer = Result[Progress, RenderingFaults](error=faults)
            return paired(answer, ended(Ending.FAILED)).bind(lambda _: answer)

        return rendered(progress).bind(lambda _: ended(Ending.SUCCEEDED)).or_else_with(failed)

    def resumed(found: Option[tuple[Progress, datetime.datetime]]) -> RenderingResult[Progress]:
        match found.to_optional():
            case (previous, _):
                held: Option[Progress] = Some(previous)
            case None:
                held = Nothing
        progress = Progress.begun(held, queue, os.getpid())
        return store(records, msgspec.json.encode(progress, order="deterministic")).bind(lambda _: completed(progress))

    return observed(records).bind(resumed)


def stop(progress: Progress) -> RenderingResult[None]:
    """Ends a run's supervisor and its renders."""
    if sys.platform == "win32":
        stopped = ran(("taskkill", "/T", "/F", "/PID", str(progress.process)))
    else:
        stopped = catch(lambda: killpg(progress.process, signal.SIGTERM), exception=OSError)().map_error(
            lambda error: (RenderingSystemFault(request=(Call.KILL_GROUP, progress.process), error=error),)
        )
    return stopped


def cleared(queue: RenderQueue, host: Host) -> RenderingResult[None]:
    """Deletes the files of a finished run."""

    def removed(file: RunFile) -> RenderingResult[None]:
        path = host.path(queue, file)
        return catch(lambda: path.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (RenderingWriteFault(request=path, error=error),))

    return accumulated(map(removed, (RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Host", "LocalMachine", "Run", "RunFile", "cleared", "stop", "supervise"]
