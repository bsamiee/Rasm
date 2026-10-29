"""Local queue runs: host commands and run files, process launches, supervision of background Blender renders, sleep hold, and shutdown."""

from collections.abc import Callable, Mapping
import ctypes
from datetime import timedelta
from enum import auto, StrEnum
from functools import partial
from graphlib import TopologicalSorter
from heapq import heappop, heappush
from itertools import count, groupby
import os
from pathlib import Path
import shlex
import signal
import subprocess
import sys
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
from tenacity import retry_if_result, Retrying, stop_after_attempt

from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.fields import Case
from blender.records.stores import argument, folder_made, RecordFile, removed
from blender.rendering.batch.queue import Ending, Job, observed, Progress, RenderQueue

if sys.platform == "win32":
    from ctypes import get_last_error, WinDLL, WinError, wintypes  # ty: ignore[possibly-missing-import]
else:
    from os import killpg  # ty: ignore[possibly-missing-import]

# --- [TYPES] ----------------------------------------------------------------------------


class RunCommand(StrEnum):
    """Extension command a queue run starts, valued by its command key."""

    PREPARE = auto()
    SUPERVISE = auto()
    RENDER = auto()


class RunFile(StrEnum):
    """Run file suffix after the key of the plan or queue it belongs to."""

    PLAN = ".plan.json"
    PLAN_LOG = ".plan.log"
    SENT = ".sent.json"
    QUEUE = ".json"
    PROGRESS = ".progress.json"
    LOG = ".log"
    RENDERON = ".renderon.json"
    RENDERON_LOG = ".renderon.log"


# --- [MODELS] ---------------------------------------------------------------------------


class HostCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Running host binary, run folder, and registered name of each run command."""

    binary: str
    folder: str
    commands: Mapping[RunCommand, str]

    def argv(self, command: RunCommand, record: msgspec.Struct, file: Option[str] = Nothing) -> tuple[str, ...]:
        """Returns background command line opening `file` and passing `record` as its command argument."""
        return (self.binary, "--background", *file.to_list(), "--command", self.commands[command], argument(record))

    def path(self, key: str, file: RunFile) -> Path:
        """Returns path of the run file of a plan or queue key."""
        return Path(self.folder, f"{key}{file}")


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

    def sent(self, queue: RenderQueue, host: HostCommands) -> Outcome[tuple[Outcome[str], ...]]:
        """Writes queue file, then starts detached supervisor over the file, returning log path."""
        return (
            RecordFile(path=host.path(queue.stem, RunFile.QUEUE), form=RenderQueue)
            .written(queue)
            .bind(lambda path: detached(host.argv(RunCommand.SUPERVISE, Run(queue=str(path), target=self)), host.path(queue.stem, RunFile.LOG)))
            .map(lambda log: (Ok(str(log)),))
        )


class Run(Case, frozen=True, kw_only=True):
    """Queue path and target settings commands decode."""

    queue: str
    target: LocalMachine


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSES]


def ran(argv: tuple[str, ...]) -> Outcome[None]:
    """Runs process to exit, nonzero code as fault."""
    return (
        catch(subprocess.run, exception=OSError)(argv, check=False)
        .map_error(partial(Fault.of, Reason.FAILED, shlex.join(argv)))
        .filter_with(lambda done: done.returncode == 0, lambda done: Fault.of(Reason.ANSWERED, shlex.join(argv), member=f"exit code {done.returncode}"))
        .map(lambda _: None)
    )


def spawned(argv: tuple[str, ...], stream: int | None = None) -> Outcome[subprocess.Popen[bytes]]:
    """Starts process, stdin and stdout on stream."""

    @catch(exception=OSError)
    def started() -> subprocess.Popen[bytes]:
        return subprocess.Popen(argv, stdin=stream, stdout=stream)

    return started().map_error(partial(Fault.of, Reason.FAILED, shlex.join(argv)))


def detached(argv: tuple[str, ...], log: Path) -> Outcome[Path]:
    """Starts process in own session with output appended to log."""

    @catch(exception=OSError)
    def launched() -> Path:
        with log.open("ab") as sink:
            if sys.platform == "win32":
                subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NEW_PROCESS_GROUP | subprocess.DETACHED_PROCESS)  # ty: ignore[possibly-missing-attribute]
            else:
                subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, start_new_session=True)
        return log

    return folder_made(log.parent).bind(lambda _: launched().map_error(partial(Fault.of, Reason.FAILED, shlex.join(argv))))


# --- [SUPERVISION]


def supervise(target: LocalMachine, queue: RenderQueue, host: HostCommands, interval: timedelta, /) -> Outcome[Progress]:
    """Renders remaining frames one job group per process, touching the progress record every `interval` and restarting crashed renders."""
    records = RecordFile(path=host.path(queue.stem, RunFile.PROGRESS), form=Progress)
    run = Run(queue=str(host.path(queue.stem, RunFile.QUEUE)), target=target)
    schedule = target.schedule(queue)
    restarted = Retrying(
        stop=stop_after_attempt(target.retries + 1),
        retry=retry_if_result(lambda answer: answer.map(lambda code: code not in {0, 1}).default_value(False)),
        retry_error_callback=lambda state: state.outcome and state.outcome.result(),
    )

    def polled(child: subprocess.Popen[bytes]) -> Option[Outcome[int]]:
        return (
            catch(child.wait, exception=subprocess.TimeoutExpired)(timeout=interval.total_seconds())
            .to_option()
            .map(Ok)
            .or_else_with(lambda: catch(records.path.touch, exception=OSError)().swap().to_option().map(lambda error: Error(Fault.of(Reason.UNWRITABLE, str(records.path), error))))
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
            .filter_with(lambda held: held == 0, lambda held: Fault.of(Reason.ANSWERED, job.title, member=f"exit code {held}"))
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
    return catch(killpg, exception=OSError)(progress.process, signal.SIGTERM).map_error(partial(Fault.of, Reason.FAILED, f"{killpg.__name__}({progress.process})"))


def cleared(queue: RenderQueue, host: HostCommands) -> Outcome[None]:
    """Deletes queue, progress, and log files of finished run."""
    return accumulated(removed(host.path(queue.stem, file)) for file in (RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG)).map(lambda _: None)


# --- [POWER]


def awake[T](operation: Callable[[], Outcome[T]], /) -> Outcome[T]:
    """Runs operation with sleep inhibited."""
    if sys.platform == "win32":
        continuous, system = 0x80000000, 0x00000001

        def inhibited(library: ctypes.CDLL) -> Outcome[T]:
            state = library.SetThreadExecutionState
            state.argtypes = (wintypes.DWORD,)
            state.restype = wintypes.DWORD

            def changed(flags: int) -> Outcome[int]:
                return Ok(state(flags)).filter_with(bool, lambda _: Fault.of(Reason.FAILED, f"{state.__name__}({flags})", WinError(get_last_error())))

            return changed(continuous | system).bind(lambda previous: paired(operation(), changed(previous)).map(lambda pair: pair[0]))

        return catch(WinDLL, exception=OSError)("kernel32", use_last_error=True).map_error(partial(Fault.of, Reason.FAILED, f"{WinDLL.__name__}(kernel32)")).bind(inhibited)

    pid = str(os.getpid())
    argv = (
        ("caffeinate", "-i", "-w", pid)
        if sys.platform == "darwin"
        else ("systemd-inhibit", "--what=idle:sleep", "--mode=block", f"--who={Path(sys.executable).name}", "--why=Render queue", "tail", f"--pid={pid}", "-f", "/dev/null")
    )

    def released(holder: subprocess.Popen[bytes]) -> Outcome[None]:
        unheld = partial(Fault.of, Reason.FAILED, shlex.join(argv))
        return (
            paired(catch(holder.terminate, exception=OSError)().map_error(unheld), catch(holder.wait, exception=OSError)().map_error(unheld))
            .filter_with(lambda pair: pair[1] in {0, -signal.SIGTERM}, lambda pair: Fault.of(Reason.ANSWERED, shlex.join(argv), member=f"exit code {pair[1]}"))
            .map(lambda _: None)
        )

    return spawned(argv, subprocess.DEVNULL).bind(lambda holder: paired(operation(), released(holder)).map(lambda pair: pair[0]))


def shut() -> Outcome[None]:
    """Schedules machine shutdown in one minute."""
    return ran(
        ("shutdown", "/s", "/t", "60")
        if sys.platform == "win32"
        else ("osascript", "-e", "delay 60", "-e", 'tell application "System Events" to shut down')
        if sys.platform == "darwin"
        else ("shutdown", "-h", "+1")
    )


def cancelled(progress: Progress) -> Outcome[None]:
    """Cancels scheduled shutdown."""
    return ran(("shutdown", "/a")) if sys.platform == "win32" else stop(progress) if sys.platform == "darwin" else ran(("shutdown", "-c"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HostCommands", "LocalMachine", "Run", "RunCommand", "RunFile", "awake", "cancelled", "cleared", "detached", "ran", "shut", "spawned", "stop", "supervise"]
