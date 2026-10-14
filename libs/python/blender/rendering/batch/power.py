"""Machine held awake while a queue renders and the optional shutdown after it, per platform."""

from collections.abc import Callable
import ctypes
import os
from pathlib import Path
import signal
import subprocess
import sys
from typing import Annotated

from expression import Error, Ok
from expression.extra.result import catch
import msgspec

from blender.imaging.faults import paired
from blender.rendering.batch.processes import ran
from blender.rendering.batch.progress import Progress
from blender.rendering.batch.supervision import stop
from blender.rendering.faults import Call, RenderingResult, RenderingRunFault, RenderingSystemFault

if sys.platform == "win32":
    from ctypes import get_last_error, WinDLL, WinError, wintypes  # ty: ignore[possibly-missing-import]

# --- [MODELS] ---------------------------------------------------------------------------


class PowerHold(msgspec.Struct, frozen=True, kw_only=True):
    """Keep-awake and shutdown policy of a queue run."""

    awake: Annotated[bool, msgspec.Meta(title="Keep Awake", description="Hold the machine out of sleep while the queue renders")] = True
    shutdown: Annotated[bool, msgspec.Meta(title="Shut Down", description="Shut the machine down one minute after the queue finishes")] = False

    def held[T](self, operation: Callable[[], RenderingResult[T]]) -> RenderingResult[T]:
        """Runs an operation with sleep inhibited and reports acquisition and release failures."""
        if not self.awake:
            return operation()
        if sys.platform == "win32":
            continuous, system = 0x80000000, 0x00000001

            def inhibited(library: ctypes.CDLL) -> RenderingResult[T]:
                state = library.SetThreadExecutionState
                state.argtypes = (wintypes.DWORD,)
                state.restype = wintypes.DWORD

                def changed(flags: int) -> RenderingResult[int]:
                    previous: int = state(flags)
                    return Ok(previous) if previous else Error((RenderingSystemFault(request=(Call.EXECUTION_STATE, flags), error=WinError(get_last_error())),))

                def restored(previous: int) -> RenderingResult[T]:
                    try:
                        answer = operation()
                    finally:
                        released = changed(previous)
                    return paired(answer, released).bind(lambda _: answer)

                return changed(continuous | system).bind(restored)

            try:
                library = WinDLL("kernel32", use_last_error=True)
            except OSError as error:
                return Error((RenderingSystemFault(request=(Call.EXECUTION_STATE, continuous | system), error=error),))
            return inhibited(library)

        pid = str(os.getpid())
        argv = (
            ("caffeinate", "-i", "-w", pid)
            if sys.platform == "darwin"
            else ("systemd-inhibit", "--what=idle:sleep", "--mode=block", f"--who={Path(sys.executable).name}", "--why=Rendering a queue", "tail", f"--pid={pid}", "-f", "/dev/null")
        )

        def scoped(holder: subprocess.Popen[bytes]) -> RenderingResult[T]:
            try:
                answer = operation()
            finally:
                terminated: RenderingResult[None] = catch(holder.terminate, exception=OSError)().map_error(lambda error: (RenderingRunFault(request=argv, error=error),))
                waited: RenderingResult[int] = catch(holder.wait, exception=OSError)().map_error(lambda error: (RenderingRunFault(request=argv, error=error),))
                released = paired(terminated, waited).bind(lambda pair: Ok(None) if pair[1] in {0, -signal.SIGTERM} else Error((RenderingRunFault(request=argv, error=pair[1]),)))
            return paired(answer, released).bind(lambda _: answer)

        try:
            holder = subprocess.Popen(argv, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL)
        except OSError as error:
            return Error((RenderingRunFault(request=argv, error=error),))
        return scoped(holder)


# --- [OPERATIONS] -----------------------------------------------------------------------


def shut() -> RenderingResult[None]:
    """Schedules the machine's shutdown one minute out."""
    return ran(
        ("shutdown", "/s", "/t", "60")
        if sys.platform == "win32"
        else ("osascript", "-e", "delay 60", "-e", 'tell application "System Events" to shut down')
        if sys.platform == "darwin"
        else ("shutdown", "-h", "+1")
    )


def cancelled(progress: Progress) -> RenderingResult[None]:
    """Cancels a scheduled shutdown."""
    return ran(("shutdown", "/a")) if sys.platform == "win32" else stop(progress) if sys.platform == "darwin" else ran(("shutdown", "-c"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PowerHold", "cancelled", "shut"]
