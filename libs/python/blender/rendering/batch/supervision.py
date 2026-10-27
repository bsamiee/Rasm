"""Local queue supervision in background Blender processes."""

from graphlib import TopologicalSorter
from heapq import heappop, heappush
from itertools import count, groupby
import os
from pathlib import Path
import signal
import subprocess
import sys
from typing import Annotated

from expression import Error, Ok, Option, Some
from expression.extra.result import catch
import msgspec
from tenacity import Future, retry_if_result, RetryCallState, Retrying, stop_after_attempt

from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason
from blender.records.fields import Case
from blender.records.stores import RecordFile
from blender.rendering.batch.processes import detached, Host, ran, RunCommand, RunFile, spawned
from blender.rendering.batch.progress import Ending, Heartbeat, observed, Progress
from blender.rendering.batch.queue import Job, RenderQueue
from blender.rendering.faults import RenderingReason

if sys.platform != "win32":
    from os import killpg  # ty: ignore[possibly-missing-import]

# --- [MODELS] ---------------------------------------------------------------------------


class LocalMachine(Case, frozen=True, kw_only=True):
    """Background Blender process per run of job frames."""

    retries: Annotated[int, msgspec.Meta(ge=0)] = 3
    sampled: bool = True

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

    def sent(self, queue: RenderQueue, host: Host) -> Outcome[Path]:
        """Writes queue file, then starts detached supervisor over the file, returning log path."""
        return (
            RecordFile(path=host.path(queue, RunFile.QUEUE), form=RenderQueue)
            .written(queue)
            .bind(lambda path: detached(host.argv(RunCommand.SUPERVISE, Run(queue=str(path), target=self)), host.path(queue, RunFile.LOG)))
        )


class Run(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Queue path and target settings commands decode."""

    queue: str
    target: LocalMachine


# --- [OPERATIONS] -----------------------------------------------------------------------


def supervise(target: LocalMachine, queue: RenderQueue, host: Host, heartbeat: Heartbeat) -> Outcome[Progress]:
    """Renders remaining frames one job group per process, restarting crashed renders."""
    records = RecordFile(path=host.path(queue, RunFile.PROGRESS), form=Progress)
    run = Run(queue=str(host.path(queue, RunFile.QUEUE)), target=target)
    schedule = target.schedule(queue)

    def exhausted(state: RetryCallState) -> object:
        match state.outcome:
            case Future() as last:
                return last.result()
            case None:
                raise RuntimeError(f"{state} ended with no attempt outcome")

    restarted = Retrying(
        stop=stop_after_attempt(target.retries + 1), retry=retry_if_result(lambda answer: answer.map(lambda code: code not in {0, 1}).default_value(False)), retry_error_callback=exhausted
    )

    def polled(child: subprocess.Popen[bytes]) -> Option[Outcome[int]]:
        return (
            catch(child.wait, exception=subprocess.TimeoutExpired)(timeout=heartbeat.interval)
            .to_option()
            .map(Ok)
            .or_else_with(
                lambda: catch(records.path.touch, exception=OSError)().swap().to_option().map(lambda error: Error((Fault(reason=RecordReason.UNWRITABLE, subject=str(records.path), cause=error),)))
            )
        )

    def waited(child: subprocess.Popen[bytes]) -> Outcome[int]:
        with child:
            return next(answer for _ in count() for answer in polled(child).to_list())

    def rendered(progress: Progress) -> Outcome[Progress]:
        remaining = frozenset(progress.remaining(queue))
        if not (pending := (progress.pending or next((tuple(group) for _, group in groupby((step for step in schedule if step in remaining), lambda step: step[0])), ()))):
            return Ok(progress)
        job = queue.jobs[pending[0][0]]
        argv = host.argv(RunCommand.RENDER, run, Some(job.file))
        return records.written(msgspec.structs.replace(progress, pending=pending)).bind(lambda _: restarted(lambda: spawned(argv).bind(waited))).bind(lambda code: confirmed(job, code)).bind(rendered)

    def confirmed(job: Job, code: int) -> Outcome[Progress]:
        return (
            Ok(code)
            .filter_with(lambda held: held == 0, lambda held: (Fault(reason=RenderingReason.EXITED, subject=job.title, member=str(held)),))
            .bind(lambda _: records.revised(lambda held: msgspec.structs.replace(held, pending=())))
        )

    def ended(ending: Ending) -> Outcome[Progress]:
        return records.revised(lambda held: msgspec.structs.replace(held, ending=ending))

    def failed(faults: Faults) -> Outcome[Progress]:
        return Error((*faults, *ended(Ending.FAILED).swap().default_value(())))

    def completed(progress: Progress) -> Outcome[Progress]:
        return records.written(progress).bind(lambda _: rendered(progress).bind(lambda _: ended(Ending.SUCCEEDED)).or_else_with(failed))

    return observed(records.path).map(lambda found: Progress.resumed(found, queue, os.getpid())).bind(completed)


def stop(progress: Progress) -> Outcome[None]:
    """Stops supervisor and its renders."""
    if sys.platform == "win32":
        return ran(("taskkill", "/T", "/F", "/PID", str(progress.process)))
    return catch(killpg, exception=OSError)(progress.process, signal.SIGTERM).map_error(
        lambda error: (Fault(reason=RenderingReason.SYSTEM, subject=f"{killpg.__name__}({progress.process})", cause=error),)
    )


def cleared(queue: RenderQueue, host: Host) -> Outcome[None]:
    """Deletes queue, progress, and log files of finished run."""

    def removed(file: RunFile) -> Outcome[None]:
        path = host.path(queue, file)
        return catch(path.unlink, exception=OSError)(missing_ok=True).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(path), cause=error),))

    return accumulated(map(removed, (RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LocalMachine", "Run", "cleared", "stop", "supervise"]
