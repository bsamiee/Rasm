"""Local queue runs: host commands and run files with their retention, process launches, another program run to its exit with its output relayed into the bounded log of its run, supervision of background Blender renders, sleep hold, and shutdown."""

import base64
from collections import deque
from collections.abc import Callable, Collection, Mapping
from contextlib import closing, ExitStack
import contextvars
from datetime import timedelta
from enum import auto, StrEnum
from functools import partial
from graphlib import TopologicalSorter
from heapq import heappop, heappush
from itertools import count, groupby
import logging
import os
from pathlib import Path
import shlex
import signal
import subprocess
import sys
import threading
from typing import Annotated, Final

from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
from opentelemetry import propagate, trace
from opentelemetry.propagators._envcarrier import EnvironmentSetter
from opentelemetry.semconv.attributes.error_attributes import ERROR_TYPE
from opentelemetry.trace import Status, StatusCode
from tenacity import retry_if_result, Retrying, stop_after_attempt
from wakepy import keep

from rasm_records import accumulated, Case, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason, RecordFile, removed
from rasm_rendering._batch.queue import Ending, observed, Progress, PROGRESS_DECODER, RENDER_QUEUE_DECODER, RenderQueue
from rasm_telemetry import relayed

# --- [TYPES] ----------------------------------------------------------------------------


class RunCommand(StrEnum):
    """Extension command a queue run starts, valued by its command key."""

    PREPARE = auto()
    SUPERVISE = auto()
    RENDER = auto()


class RunFile(StrEnum):
    """Run file suffix after the key of the plan or queue it belongs to."""

    PLAN = ".plan.json"
    REFUSED = ".refused.json"
    SENT = ".sent.json"
    QUEUE = ".json"
    PROGRESS = ".progress.json"
    LOG = ".log"
    RENDERON = ".renderon.json"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TRACER: Final = trace.get_tracer(__name__)

# --- [MODELS] ---------------------------------------------------------------------------


class HostCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Running host binary, run folder, registered name of each run command, and the handler writing the log of a run at a path."""

    binary: str
    folder: str
    commands: Mapping[RunCommand, str]
    logs: Callable[[Path], logging.Handler]

    def argv(self, command: RunCommand, record: msgspec.Struct, file: Option[str] = Nothing) -> tuple[str, ...]:
        """Returns background command line opening `file` and passing `record` as its command argument."""
        return (self.binary, "--background", *file.to_list(), "--command", self.commands[command], argument(record))

    def path(self, key: str, file: RunFile) -> Path:
        """Returns path of the run file of a plan or queue key."""
        return Path(self.folder, f"{key}{file}")

    def pruned(self, kept: Callable[[tuple[str, ...]], Collection[str]], /) -> Outcome[None]:
        """Removes every file of the runs `kept` leaves out of the run keys it receives newest first, rotated logs and lock files with them, a run as new as its newest file.

        Returns:
            Ok, `UNREADABLE` fault for a folder refusing its listing, `UNWRITABLE` for each file refusing removal.
        """
        try:
            aged = sorted(((entry.stat().st_mtime, entry.name.removeprefix(".__").partition(".")[0], Path(entry.path)) for entry in os.scandir(self.folder) if entry.is_file()), reverse=True)
        except OSError as error:
            return Error(Fault.of(Reason.UNREADABLE, self.folder, error))
        held = kept(tuple(dict.fromkeys(key for _, key, _ in aged)))
        return accumulated(removed(path) for _, key, path in aged if key not in held).map(lambda _: None)

    def run(self, key: str, argv: tuple[str, ...], /, waited: Callable[[subprocess.Popen[bytes]], Outcome[int]] = lambda child: Ok(child.wait())) -> Outcome[None]:
        """Runs another program to its exit under a run key inside one span, its output relayed line by line into the run's log for the whole run.

        The process calling this stays alive for the program, so the log's rotation bounds the output and the exit reaches a caller.

        Returns:
            Ok, `UNWRITABLE` fault for a refused log, `FAILED` for a refused launch, `FAILED` holding the exit code and last 20 output lines for a nonzero exit, or the first fault of `waited`.
        """
        tail, subject = deque[str](maxlen=20), shlex.join(argv)

        def attended(log: logging.Handler, child: subprocess.Popen[bytes]) -> Outcome[None]:
            relay = threading.Thread(target=contextvars.copy_context().run, args=(tail.extend, relayed(child.stdout or (), log, Path(argv[0]).stem)))
            relay.start()
            with child:
                answer = waited(child)
                relay.join()
            return answer.bind(
                lambda code: catch(subprocess.CompletedProcess(argv, code, "\n".join(tail)).check_returncode, exception=subprocess.CalledProcessError)().map_error(
                    partial(Fault.of, Reason.FAILED, subject)
                )
            )

        def logged(log: logging.Handler) -> Outcome[None]:
            with closing(log):
                return spawned(argv).bind(partial(attended, log))

        with _TRACER.start_as_current_span(type(self).run.__qualname__) as span:
            answer = catch(self.logs, exception=OSError)(self.path(key, RunFile.LOG)).map_error(partial(Fault.of, Reason.UNWRITABLE, key)).bind(logged)
            for fault in answer.swap().default_value(())[:1]:
                span.set_status(Status(StatusCode.ERROR, fault.message))
                span.set_attribute(ERROR_TYPE, fault.reason.name)
            return answer


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
        log = host.path(queue.stem, RunFile.LOG)
        return (
            RecordFile(path=host.path(queue.stem, RunFile.QUEUE), decoder=RENDER_QUEUE_DECODER)
            .written(queue)
            .bind(lambda path: spawned(host.argv(RunCommand.SUPERVISE, Run(queue=str(path), target=self)), detached=True))
            .map(lambda _: (Ok(str(log)),))
        )


class Run(Case, frozen=True, kw_only=True):
    """Queue path and target settings commands decode."""

    queue: str
    target: LocalMachine


class Program(Case, frozen=True, kw_only=True):
    """Command line of another program a supervisor runs under a run key, decoded by the `supervise` command."""

    key: str
    argv: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ARGUMENTS]


def argument(record: msgspec.Struct) -> str:
    """Returns the command argument holding record, base64 JSON that no shell, worker, or host argument parser rewrites."""
    return base64.b64encode(msgspec.json.encode(record)).decode("ascii")


def argued[T](decoder: msgspec.json.Decoder[T], subject: str, text: str, /) -> Outcome[T]:
    """Returns the record a command argument holds, `UNREADABLE` fault over subject for text no `argument` wrote."""
    unreadable = partial(Fault.of, Reason.UNREADABLE, subject)
    return catch(base64.b64decode, exception=ValueError)(text, validate=True).map_error(unreadable).bind(lambda data: catch(decoder.decode, exception=msgspec.DecodeError)(data).map_error(unreadable))


# --- [PROCESSES]


def ran(argv: tuple[str, ...]) -> Outcome[None]:
    """Runs a system utility to its exit, a refused launch or a nonzero exit as `FAILED` fault, the exit's cause holding its code and output."""
    try:
        subprocess.run(argv, stdin=subprocess.DEVNULL, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, check=True)
    except (OSError, subprocess.CalledProcessError) as error:
        return Error(Fault.of(Reason.FAILED, shlex.join(argv), error))
    return Ok(None)


def spawned(argv: tuple[str, ...], *, detached: bool = False) -> Outcome[subprocess.Popen[bytes]]:
    """Starts process with the current trace context in its environment, its output on a pipe its parent reads to the end, or detached in its own session.

    A detached process is a command of the host that records for itself under the trace it continues, so its standard streams hold nothing and go nowhere.

    Returns:
        Ok(process handle for `poll`), `FAILED` fault for a refused launch.
    """
    flags = subprocess.CREATE_NEW_PROCESS_GROUP | subprocess.DETACHED_PROCESS if sys.platform == "win32" and detached else 0  # ty: ignore[possibly-missing-attribute]
    environment = os.environ.copy()
    propagate.inject(environment, setter=EnvironmentSetter())

    @catch(exception=OSError)
    def started() -> subprocess.Popen[bytes]:
        return subprocess.Popen(
            argv, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL if detached else subprocess.PIPE, stderr=subprocess.STDOUT, env=environment, start_new_session=detached, creationflags=flags
        )

    return started().map_error(partial(Fault.of, Reason.FAILED, shlex.join(argv)))


# --- [SUPERVISION]


def supervise(target: LocalMachine, queue: RenderQueue, host: HostCommands, interval: timedelta, /) -> Outcome[Progress]:
    """Renders remaining frames one job group per process through `HostCommands.run`, touching the progress record every `interval` and restarting a render that exits with a code other than 1."""
    records = RecordFile(path=host.path(queue.stem, RunFile.PROGRESS), decoder=PROGRESS_DECODER)
    run = Run(queue=str(host.path(queue.stem, RunFile.QUEUE)), target=target)
    schedule = target.schedule(queue)
    restarted = Retrying(
        stop=stop_after_attempt(target.retries + 1),
        retry=retry_if_result(lambda answer: any(isinstance(fault.cause, subprocess.CalledProcessError) and fault.cause.returncode != 1 for fault in answer.swap().default_value(()))),
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
        return next(answer for _ in count() for answer in polled(child).to_list())

    def rendered(progress: Progress) -> Outcome[Progress]:
        while True:
            remaining = frozenset(progress.remaining(queue))
            if not (pending := (progress.pending or next((tuple(group) for _, group in groupby((step for step in schedule if step in remaining), lambda step: step[0])), ()))):
                return Ok(progress)
            argv = host.argv(RunCommand.RENDER, run, Some(queue.jobs[pending[0][0]].file))
            match (
                records
                .written(msgspec.structs.replace(progress, pending=pending))
                .bind(lambda _: restarted(lambda: host.run(queue.stem, argv, waited)))
                .bind(lambda _: records.revised(lambda held: msgspec.structs.replace(held, pending=())))
            ):
                case Result(tag="ok", ok=held):
                    progress = held
                case Result(error=faults):
                    return Error(faults)

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
    return catch(os.kill, exception=OSError)(-progress.process, signal.SIGTERM).map_error(partial(Fault.of, Reason.FAILED, f"{os.kill.__name__}(-{progress.process})"))


def cleared(queue: RenderQueue, host: HostCommands) -> Outcome[None]:
    """Deletes every run file of a finished run's queue key: its queue, progress, and log with the log's rotated files and lock."""
    return host.pruned(lambda keys: frozenset(keys) - {queue.stem})


# --- [POWER]


def awake[T](operation: Callable[[], Outcome[T]], /) -> Outcome[T]:
    """Runs operation while wakepy's `keep.running` mode holds off system sleep, the display free to sleep, a mode failing to activate running nothing."""
    with ExitStack() as stack:
        mode = stack.enter_context(keep.running(on_fail="pass"))
        unheld = partial(Fault.of, Reason.FAILED, str(mode.result.method or mode.result.mode_name))
        answer = operation() if mode.active else Error(unheld(mode.result.get_failure_text(style="inline")))
        release = stack.pop_all().close
    return paired(answer, catch(release, exception=RuntimeError)().map_error(unheld)).map(lambda pair: pair[0])


def shutdown() -> Outcome[None]:
    """Schedules machine shutdown in one minute."""
    return ran(
        ("shutdown", "/s", "/t", "60")
        if sys.platform == "win32"
        else ("osascript", "-e", "delay 60", "-e", 'tell application "System Events" to shut down')
        if sys.platform == "darwin"
        else ("shutdown", "-h", "+1")
    )


def shutdown_cancelled(progress: Progress) -> Outcome[None]:
    """Cancels scheduled shutdown."""
    return ran(("shutdown", "/a")) if sys.platform == "win32" else stop(progress) if sys.platform == "darwin" else ran(("shutdown", "-c"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HostCommands", "LocalMachine", "Program", "Run", "RunCommand", "RunFile", "argued", "argument", "awake", "cleared", "ran", "shutdown", "shutdown_cancelled", "spawned", "stop", "supervise"]
