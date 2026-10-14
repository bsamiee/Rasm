"""Process runs to their exit and detached launches."""

from pathlib import Path
import subprocess
import sys

from expression import Error, Ok
from expression.extra.result import catch

from blender.rendering.faults import RenderingResult, RenderingRunFault

# --- [OPERATIONS] -----------------------------------------------------------------------


def ran(argv: tuple[str, ...]) -> RenderingResult[None]:
    """Runs a process to its exit."""
    return (
        catch(lambda: subprocess.run(argv, check=False).returncode, exception=OSError)()
        .map_error(lambda error: (RenderingRunFault(request=argv, error=error),))
        .bind(lambda code: Ok(None) if code == 0 else Error((RenderingRunFault(request=argv, error=code),)))
    )


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

    return catch(launched, exception=OSError)().map_error(lambda error: (RenderingRunFault(request=argv, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["detached", "ran"]
