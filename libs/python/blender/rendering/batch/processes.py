"""Process runs, detached launches, and the host commands of queue runs."""

from collections.abc import Mapping
from enum import auto, StrEnum
from functools import partial
from pathlib import Path
import shlex
import subprocess
import sys

from expression import Nothing, Option
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome
from blender.records.stores import argument, folder_made
from blender.rendering.faults import RenderingReason

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def ran(argv: tuple[str, ...]) -> Outcome[None]:
    """Runs process to exit, nonzero code as fault."""
    return (
        catch(subprocess.run, exception=OSError)(argv, check=False)
        .map_error(partial(Fault.of, RenderingReason.UNSTARTED, shlex.join(argv)))
        .filter_with(lambda done: done.returncode == 0, lambda done: Fault.of(RenderingReason.EXITED, shlex.join(argv), member=str(done.returncode)))
        .map(lambda _: None)
    )


def spawned(argv: tuple[str, ...], stream: int | None = None) -> Outcome[subprocess.Popen[bytes]]:
    """Starts process, stdin and stdout on stream."""

    @catch(exception=OSError)
    def started() -> subprocess.Popen[bytes]:
        return subprocess.Popen(argv, stdin=stream, stdout=stream)

    return started().map_error(partial(Fault.of, RenderingReason.UNSTARTED, shlex.join(argv)))


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

    return folder_made(log.parent).bind(lambda _: launched().map_error(partial(Fault.of, RenderingReason.UNSTARTED, shlex.join(argv))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HostCommands", "RunCommand", "RunFile", "detached", "ran", "spawned"]
