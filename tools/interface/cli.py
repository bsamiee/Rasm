"""Apply desktop interfaces or upgrade staged packages for every application or one named application, reporting outcomes as JSON."""

from collections.abc import Awaitable, Callable, Iterable
from functools import partial
from importlib import import_module
from itertools import starmap
import os
from pathlib import Path
import signal
import subprocess
import traceback
from typing import Annotated, Final

import anyio
from CoreFoundation import CFRunLoopGetMain, CFRunLoopRun, CFRunLoopStop
import cyclopts
import httpx2
from mcp import MCPError
import msgspec
import psutil
from PyObjCTools import MachSignals

from interface.host import Applied, Failed, Host, Outcome, performed, printed
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Entry = Callable[[Host], Awaitable[tuple[Outcome, ...]]]
type ExitStatus = bool | int

# --- [CONSTANTS] ------------------------------------------------------------------------

FOLDER: Final = Path(__file__).resolve().parent

# --- [OPERATIONS] -----------------------------------------------------------------------


async def applied(entries: Iterable[tuple[str, Entry]], units: Units) -> bool:
    """Return application success after releasing resources and scheduling the main run loop to stop."""

    def leaves(group: BaseExceptionGroup[BaseException]) -> tuple[BaseException, ...]:
        return tuple(leaf for error in group.exceptions for leaf in (leaves(error) if isinstance(error, BaseExceptionGroup) else (error,)))

    async def outcomes(name: str, entry: Entry) -> tuple[Outcome, ...]:
        try:
            results = await entry(Host(name, FOLDER.parents[1], os.environ, units, client))
        except* (OSError, subprocess.CalledProcessError, psutil.Error, msgspec.MsgspecError, MCPError, httpx2.HTTPError) as group:
            errors = tuple(line.strip() for line in traceback.format_exception_only(group, show_group=True))
            stderr = tuple(line for error in leaves(group) if isinstance(error, subprocess.CalledProcessError) for line in error.stderr.decode().splitlines())
            results = (Failed(name, errors, stderr=stderr),)
        return results

    try:
        async with httpx2.AsyncClient(follow_redirects=True) as client:
            results = tuple(result for outcome in await anyio.gather(*starmap(outcomes, entries)) for result in outcome)
        printed(results)
        return all(isinstance(each, Applied) for each in results)
    finally:
        performed(lambda: CFRunLoopStop(CFRunLoopGetMain()))


def run(entries: Iterable[tuple[str, Entry]], *, units: Annotated[Units, cyclopts.Parameter(accepts_keys=False, n_tokens=1)] = Units.IMPERIAL) -> ExitStatus:
    """Return entry success or interrupt exit code while the main run loop services applications."""
    with anyio.from_thread.start_blocking_portal() as portal:
        future = portal.start_task_soon(applied, entries, units)
        MachSignals.signal(signal.SIGINT, lambda _: future.cancel())
        CFRunLoopRun()
    return 130 if future.cancelled() else future.result()


# --- [COMPOSITION] ----------------------------------------------------------------------

app = cyclopts.App(help=__doc__)
upgrade = cyclopts.App(name="upgrade", help="Upgrade staged application packages to their newest published builds.")
for command, entries in (
    (app, tuple((path.parent.name, (module := import_module(f"{__package__}.{path.parent.name}.apply")).apply, module.__doc__) for path in sorted(FOLDER.glob("*/apply.py")))),
    (upgrade, tuple((path.parent.name, entry := import_module(f"{__package__}.{path.parent.name}.packages").upgrade, entry.__doc__) for path in sorted(FOLDER.glob("*/packages.py")))),
):
    command.default(partial(run, tuple((name, entry) for name, entry, _ in entries)))
    for name, entry, description in entries:
        command.command(partial(run, ((name, entry),)), name=name, help=description)
app.command(upgrade)
if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
