"""Process runs to their exit and detached launches."""

from collections.abc import Callable
from pathlib import Path
import subprocess
import sys

from expression import Error, Ok
from expression.extra.result import catch

from blender.rendering.faults import RenderingFaults, RenderingResult, RenderingRunFault

# --- [OPERATIONS] -----------------------------------------------------------------------


def refused(argv: tuple[str, ...]) -> Callable[[OSError | int], RenderingFaults]:
    """Maps a launch error or exit code of a process to its run fault."""
    return lambda error: (RenderingRunFault(request=argv, error=error),)


def ran(argv: tuple[str, ...]) -> RenderingResult[None]:
    """Runs a process to its exit."""
    return catch(lambda: subprocess.run(argv, check=False).returncode, exception=OSError)().map_error(refused(argv)).bind(lambda code: Ok(None) if code == 0 else Error(refused(argv)(code)))


def spawned(argv: tuple[str, ...], stream: int | None = None) -> RenderingResult[subprocess.Popen[bytes]]:
    """Starts a process, its input and output on a stream when given."""
    return catch(lambda: subprocess.Popen(argv, stdin=stream, stdout=stream), exception=OSError)().map_error(refused(argv))


def detached(argv: tuple[str, ...], log: Path) -> RenderingResult[Path]:
    """Starts a process in its own session with its output appended to a log."""

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

__all__ = ["detached", "ran", "refused", "spawned"]
