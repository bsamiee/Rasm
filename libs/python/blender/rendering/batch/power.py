"""Sleep hold while queue renders and shutdown after it."""

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

from blender.imaging.faults import paired, scoped
from blender.rendering.batch.processes import ran, refused, spawned
from blender.rendering.batch.progress import Progress
from blender.rendering.batch.supervision import stop
from blender.rendering.faults import Call, RenderingResult, RenderingSystemFault

if sys.platform == "win32":
    from ctypes import get_last_error, WinDLL, WinError, wintypes  # ty: ignore[possibly-missing-import]

# --- [MODELS] ---------------------------------------------------------------------------


class PowerHold(msgspec.Struct, frozen=True, kw_only=True):
    """Sleep and shutdown settings."""

    awake: Annotated[bool, msgspec.Meta(title="Keep Awake", description="Prevents sleep while queue renders")] = True
    shutdown: Annotated[bool, msgspec.Meta(title="Shut Down", description="Shuts down one minute after queue finishes")] = False

    def held[T](self, operation: Callable[[], RenderingResult[T]]) -> RenderingResult[T]:
        """Runs operation with sleep inhibited."""
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

                return scoped(changed(continuous | system), lambda _: operation(), changed)

            try:
                library = WinDLL("kernel32", use_last_error=True)
            except OSError as error:
                return Error((RenderingSystemFault(request=(Call.EXECUTION_STATE, continuous | system), error=error),))
            return inhibited(library)

        pid = str(os.getpid())
        argv = (
            ("caffeinate", "-i", "-w", pid)
            if sys.platform == "darwin"
            else ("systemd-inhibit", "--what=idle:sleep", "--mode=block", f"--who={Path(sys.executable).name}", "--why=Render queue", "tail", f"--pid={pid}", "-f", "/dev/null")
        )

        def released(holder: subprocess.Popen[bytes]) -> RenderingResult[None]:
            terminated: RenderingResult[None] = catch(holder.terminate, exception=OSError)().map_error(refused(argv))
            waited: RenderingResult[int] = catch(holder.wait, exception=OSError)().map_error(refused(argv))
            return paired(terminated, waited).bind(lambda pair: Ok(None) if pair[1] in {0, -signal.SIGTERM} else Error(refused(argv)(pair[1])))

        return scoped(spawned(argv, subprocess.DEVNULL), lambda _: operation(), released)


# --- [OPERATIONS] -----------------------------------------------------------------------


def shut() -> RenderingResult[None]:
    """Schedules machine shutdown in one minute."""
    return ran(
        ("shutdown", "/s", "/t", "60")
        if sys.platform == "win32"
        else ("osascript", "-e", "delay 60", "-e", 'tell application "System Events" to shut down')
        if sys.platform == "darwin"
        else ("shutdown", "-h", "+1")
    )


def cancelled(progress: Progress) -> RenderingResult[None]:
    """Cancels scheduled shutdown."""
    return ran(("shutdown", "/a")) if sys.platform == "win32" else stop(progress) if sys.platform == "darwin" else ran(("shutdown", "-c"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PowerHold", "cancelled", "shut"]
