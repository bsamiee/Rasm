"""Process runs, detached launches, and the host commands of queue runs."""

import base64
from collections.abc import Callable, Mapping
from enum import auto, StrEnum
from pathlib import Path
import subprocess
import sys

from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.rendering.batch.queue import RenderQueue
from blender.rendering.faults import RenderingFaults, RenderingResult, RenderingRunFault

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


def refused(argv: tuple[str, ...]) -> Callable[[OSError | int], RenderingFaults]:
    """Maps launch error or exit code to run fault."""
    return lambda error: (RenderingRunFault(request=argv, error=error),)


def ran(argv: tuple[str, ...]) -> RenderingResult[None]:
    """Runs process to exit, nonzero code as fault."""
    return catch(lambda: subprocess.run(argv, check=False).returncode, exception=OSError)().map_error(refused(argv)).bind(lambda code: Ok(None) if code == 0 else Error(refused(argv)(code)))


def spawned(argv: tuple[str, ...], stream: int | None = None) -> RenderingResult[subprocess.Popen[bytes]]:
    """Starts process, stdin and stdout on stream."""
    return catch(lambda: subprocess.Popen(argv, stdin=stream, stdout=stream), exception=OSError)().map_error(refused(argv))


def detached(argv: tuple[str, ...], log: Path) -> RenderingResult[Path]:
    """Starts process in own session with output appended to log."""

    def launched() -> Path:
        log.parent.mkdir(parents=True, exist_ok=True)
        with log.open("ab") as sink:
            if sys.platform == "win32":
                subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NEW_PROCESS_GROUP | subprocess.DETACHED_PROCESS)  # ty: ignore[possibly-missing-attribute]
            else:
                subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, start_new_session=True)
        return log

    return catch(launched, exception=OSError)().map_error(refused(argv))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Host", "RunCommand", "RunFile", "detached", "ran", "refused", "spawned"]
