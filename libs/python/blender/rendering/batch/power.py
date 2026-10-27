"""Sleep hold while queue renders and shutdown after it."""

from collections.abc import Callable
import ctypes
from functools import partial
import os
from pathlib import Path
import shlex
import signal
import subprocess
import sys

from expression import Ok
from expression.extra.result import catch
import msgspec

from blender.records.faults import Fault, Outcome, paired
from blender.rendering.batch.processes import ran, spawned
from blender.rendering.batch.progress import Progress
from blender.rendering.batch.supervision import stop
from blender.rendering.faults import RenderingReason

if sys.platform == "win32":
    from ctypes import get_last_error, WinDLL, WinError, wintypes  # ty: ignore[possibly-missing-import]

# --- [MODELS] ---------------------------------------------------------------------------


class PowerHold(msgspec.Struct, frozen=True, kw_only=True):
    """Sleep and shutdown settings."""

    awake: bool = True
    shutdown: bool = False

    def held[T](self, operation: Callable[[], Outcome[T]]) -> Outcome[T]:
        """Runs operation with sleep inhibited."""
        if not self.awake:
            return operation()
        if sys.platform == "win32":
            continuous, system = 0x80000000, 0x00000001

            def inhibited(library: ctypes.CDLL) -> Outcome[T]:
                state = library.SetThreadExecutionState
                state.argtypes = (wintypes.DWORD,)
                state.restype = wintypes.DWORD

                def changed(flags: int) -> Outcome[int]:
                    return Ok(state(flags)).filter_with(bool, lambda _: Fault.of(RenderingReason.SYSTEM, f"{state.__name__}({flags})", WinError(get_last_error())))

                return changed(continuous | system).bind(lambda previous: paired(operation(), changed(previous)).map(lambda pair: pair[0]))

            return catch(WinDLL, exception=OSError)("kernel32", use_last_error=True).map_error(partial(Fault.of, RenderingReason.SYSTEM, f"{WinDLL.__name__}(kernel32)")).bind(inhibited)

        pid = str(os.getpid())
        argv = (
            ("caffeinate", "-i", "-w", pid)
            if sys.platform == "darwin"
            else ("systemd-inhibit", "--what=idle:sleep", "--mode=block", f"--who={Path(sys.executable).name}", "--why=Render queue", "tail", f"--pid={pid}", "-f", "/dev/null")
        )

        def released(holder: subprocess.Popen[bytes]) -> Outcome[None]:
            unstarted = partial(Fault.of, RenderingReason.UNSTARTED, shlex.join(argv))
            return (
                paired(catch(holder.terminate, exception=OSError)().map_error(unstarted), catch(holder.wait, exception=OSError)().map_error(unstarted))
                .filter_with(lambda pair: pair[1] in {0, -signal.SIGTERM}, lambda pair: Fault.of(RenderingReason.EXITED, shlex.join(argv), member=str(pair[1])))
                .map(lambda _: None)
            )

        return spawned(argv, subprocess.DEVNULL).bind(lambda holder: paired(operation(), released(holder)).map(lambda pair: pair[0]))


# --- [OPERATIONS] -----------------------------------------------------------------------


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

__all__ = ["PowerHold", "cancelled", "shut"]
