"""Local queue supervision in background Blender processes."""

from graphlib import TopologicalSorter
from heapq import heappop, heappush
from itertools import groupby
import os
from pathlib import Path
import signal
import subprocess
import sys
from typing import Annotated

from expression import Error, Ok, Result, Some
from expression.extra.result import catch
import msgspec
from tenacity import retry_if_result, Retrying, stop_after_attempt

from blender.records.faults import accumulated, FileFault, scoped
from blender.records.fields import Case, Presentation
from blender.records.stores import RecordFile
from blender.rendering.batch.processes import Host, ran, refused, RunCommand, RunFile, spawned
from blender.rendering.batch.progress import Ending, Heartbeat, observed, Progress
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.faults import RenderingFaults, RenderingResult, RenderingSystemFault

if sys.platform != "win32":
    from os import killpg  # ty: ignore[possibly-missing-import]

# --- [MODELS] ---------------------------------------------------------------------------


class LocalMachine(Case, frozen=True, kw_only=True):
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


class Run(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Queue path and target settings commands decode."""

    queue: str
    target: LocalMachine


# --- [OPERATIONS] -----------------------------------------------------------------------


def supervise(target: LocalMachine, queue: RenderQueue, host: Host, heartbeat: Heartbeat) -> RenderingResult[Progress]:
    """Renders remaining frames one job group per process, restarting crashed renders."""
    records = RecordFile(path=host.path(queue, RunFile.PROGRESS), form=Progress)
    run = Run(queue=str(host.path(queue, RunFile.QUEUE)), target=target)
    schedule = target.schedule(queue)
    restarted = Retrying(
        stop=stop_after_attempt(target.retries + 1),
        retry=retry_if_result(lambda answer: answer.map(lambda code: code not in {0, 1}).default_value(False)),
        retry_error_callback=lambda state: state.outcome.result(),  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )

    def launched(argv: tuple[str, ...]) -> RenderingResult[int]:
        def waited(child: subprocess.Popen[bytes]) -> RenderingResult[int]:
            while True:
                try:
                    return Ok(child.wait(timeout=heartbeat.interval))
                except subprocess.TimeoutExpired:
                    try:
                        records.path.touch()
                    except OSError as error:
                        return Error((FileFault(access="write", request=records.path, error=error),))
                except OSError as error:
                    return Error(refused(argv)(error))

        return scoped(spawned(argv), waited, lambda child: catch(child.wait, exception=OSError)().map_error(refused(argv)))

    def rendered(progress: Progress) -> RenderingResult[Progress]:
        remaining = frozenset(progress.remaining(queue))
        if not (pending := (progress.pending or next((tuple(group) for _, group in groupby((step for step in schedule if step in remaining), lambda step: step[0])), ()))):
            return Ok(progress)
        argv = host.argv(RunCommand.RENDER, run, Some(queue.jobs[pending[0][0]].file))
        return (
            Result[Path, RenderingFaults]
            .bind(records.written(msgspec.structs.replace(progress, pending=pending)), lambda _: restarted(launched, argv))
            .bind(lambda code: confirmed(argv, code))
            .bind(rendered)
        )

    def confirmed(argv: tuple[str, ...], code: int) -> RenderingResult[Progress]:
        return records.revised(lambda held: msgspec.structs.replace(held, pending=())) if code == 0 else Error(refused(argv)(code))

    def ended(ending: Ending) -> RenderingResult[Progress]:
        return records.revised(lambda held: msgspec.structs.replace(held, ending=ending))

    def failed(faults: RenderingFaults) -> RenderingResult[Progress]:
        return Error((faults[0], *faults[1:], *ended(Ending.FAILED).swap().default_value(())))

    def completed(progress: Progress) -> RenderingResult[Progress]:
        return Result[Path, RenderingFaults].bind(records.written(progress), lambda _: rendered(progress).bind(lambda _: ended(Ending.SUCCEEDED)).or_else_with(failed))

    return observed(records.path).map(lambda found: Progress.resumed(found, queue, os.getpid())).bind(completed)


def stop(progress: Progress) -> RenderingResult[None]:
    """Stops supervisor and its renders."""
    if sys.platform == "win32":
        return ran(("taskkill", "/T", "/F", "/PID", str(progress.process)))
    return catch(lambda: killpg(progress.process, signal.SIGTERM), exception=OSError)().map_error(lambda error: (RenderingSystemFault(request=(killpg.__name__, progress.process), error=error),))


def cleared(queue: RenderQueue, host: Host) -> RenderingResult[None]:
    """Deletes queue, progress, and log files of finished run."""

    def removed(file: RunFile) -> RenderingResult[None]:
        path = host.path(queue, file)
        return catch(lambda: path.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (FileFault(access="write", request=path, error=error),))

    return accumulated(map(removed, (RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LocalMachine", "Run", "cleared", "stop", "supervise"]
