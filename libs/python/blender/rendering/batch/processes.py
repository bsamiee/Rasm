"""Process runs, detached launches, and the host commands of queue runs."""

import base64
from collections.abc import Mapping
from enum import auto, StrEnum
from functools import partial
from pathlib import Path
import subprocess
import sys

from expression import Nothing, Option
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Faults, Outcome
from blender.records.stores import folder_made
from blender.rendering.batch.queue import RenderQueue
from blender.rendering.faults import RenderingReason

# --- [TYPES] ----------------------------------------------------------------------------


class RunCommand(StrEnum):
    """Extension command a queue run starts, valued by its command key."""

    PREPARE = auto()
    SUPERVISE = auto()
    RENDER = auto()


class RunFile(StrEnum):
    """Run file suffix after queue key."""

    QUEUE = ".json"
    PROGRESS = ".progress.json"
    LOG = ".log"
    RENDERON = ".renderon.json"
    RENDERON_LOG = ".renderon.log"


# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True, kw_only=True):
    """Running host binary, run folder, and registered name of each run command."""

    binary: str
    folder: str
    commands: Mapping[RunCommand, str]

    def argv(self, command: RunCommand, argument: msgspec.Struct, file: Option[str] = Nothing) -> tuple[str, ...]:
        """Returns background command line opening `file` and passing `argument` as base64 JSON."""
        return (self.binary, "--background", *file.to_list(), "--command", self.commands[command], base64.b64encode(msgspec.json.encode(argument)).decode("ascii"))

    def path(self, queue: RenderQueue, file: RunFile) -> Path:
        """Returns path of queue run file."""
        return Path(self.folder, f"{queue.stem}{file}")


# --- [OPERATIONS] -----------------------------------------------------------------------


def refused(argv: tuple[str, ...], error: OSError) -> Faults:
    """Maps launch error to run fault."""
    return (Fault(reason=RenderingReason.UNSTARTED, subject=" ".join(argv), cause=error),)


def ran(argv: tuple[str, ...]) -> Outcome[None]:
    """Runs process to exit, nonzero code as fault."""
    return (
        catch(subprocess.run, exception=OSError)(argv, check=False)
        .map_error(partial(refused, argv))
        .filter_with(lambda done: done.returncode == 0, lambda done: (Fault(reason=RenderingReason.EXITED, subject=" ".join(argv), member=str(done.returncode)),))
        .map(lambda _: None)
    )


def spawned(argv: tuple[str, ...], stream: int | None = None) -> Outcome[subprocess.Popen[bytes]]:
    """Starts process, stdin and stdout on stream."""

    @catch(exception=OSError)
    def started() -> subprocess.Popen[bytes]:
        return subprocess.Popen(argv, stdin=stream, stdout=stream)

    return started().map_error(partial(refused, argv))


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

    return folder_made(log.parent).bind(lambda _: launched().map_error(partial(refused, argv)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Host", "RunCommand", "RunFile", "detached", "ran", "refused", "spawned"]
