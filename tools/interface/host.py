"""Host side every application shares: the facts a run reads, the report rows in-application code writes, the outcome a run returns, and the bundle lookup and quit.

A report is tab-separated rows whose first cell names the row's kind.
"""

from collections.abc import Mapping, Sequence
from pathlib import Path
import plistlib
from typing import Final

import anyio
import msgspec
import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type Row = Header | Change | Skip | Measure | Error
type Outcome = Applied | Failed

# --- [CONSTANTS] ------------------------------------------------------------------------

DEADLINE: Final = 300.0
LOOPBACK: Final = "127.0.0.1"

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts one application's run reads, the `.mcp.json` servers left for the application to decode."""

    app: str
    folder: Path
    artifacts: Path
    servers: msgspec.Raw
    environ: Mapping[str, str]


class Application(msgspec.Struct, frozen=True):
    """Bundle folder and bundle id of an application."""

    path: Path
    identifier: str


class Info(msgspec.Struct, frozen=True, rename={"identifier": "CFBundleIdentifier", "executable": "CFBundleExecutable"}):
    """The bundle's `Info.plist` keys the host reads."""

    identifier: str
    executable: str | None = None


class Header(msgspec.Struct, frozen=True, array_like=True, tag="app"):
    """Report row naming the application's version and settings folder."""

    version: str
    settings: str


class Change(msgspec.Struct, frozen=True, array_like=True, tag="change"):
    """Report row of one value the run changed, read before and after its write."""

    label: str
    before: str
    after: str


class Skip(msgspec.Struct, frozen=True, array_like=True, tag="skip"):
    """Report row of one optional add-on, plug-in, or library the run skipped because it is absent, disabled, or empty."""

    name: str


class Measure(msgspec.Struct, frozen=True, array_like=True, tag="measure"):
    """Report row of one extent the application measured for the host's file stage."""

    name: str
    value: float


class Error(msgspec.Struct, frozen=True, array_like=True, tag="error"):
    """Report row of one failure or value that did not hold."""

    text: str


class Applied(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run whose every value holds, with the values it changed, the optional components it skipped, and the extents it measured by name."""

    app: str
    version: str
    settings: str
    changes: tuple[Change, ...]
    skipped: tuple[str, ...]
    measures: dict[str, float]


class Failed(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run that could not reach the application, produced no report, or reported values that did not hold."""

    app: str
    errors: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [REPORT]
def parse(app: str, text: str) -> Outcome:
    """Outcome of a report: applied with its changes and measures when it holds one header and no error row, failed with the errors otherwise."""
    rows = msgspec.convert([line.split("\t") for line in text.splitlines() if line], tuple[Row, ...], strict=False)
    match [row for row in rows if isinstance(row, Header)], tuple(row.text for row in rows if isinstance(row, Error)):
        case [Header(version, settings)], ():
            return Applied(
                app,
                version,
                settings,
                tuple(row for row in rows if isinstance(row, Change)),
                tuple(row.name for row in rows if isinstance(row, Skip)),
                {row.name: row.value for row in rows if isinstance(row, Measure)},
            )
        case _, (_, *_) as errors:
            return Failed(app, errors)
        case headers, _:
            return Failed(app, (f"report holds {len(headers)} app rows and needs one",))


# --- [PROCESS]
async def application(executable: Path) -> Application | None:
    """The bundle whose main executable, `Contents/MacOS/<CFBundleExecutable>` or the bundle's name when the key is absent, is the executable, None for any other."""
    contents = executable.parent.parent
    plist = anyio.Path(contents, "Info.plist")
    match msgspec.convert(plistlib.loads(await plist.read_bytes()), Info) if await plist.exists() else None:
        case Info(identifier, main) if executable == contents / "MacOS" / (main or contents.parent.stem):
            return Application(contents.parent, identifier)
        case _:
            return None


async def quit_application(identifier: str, processes: Sequence[psutil.Process]) -> tuple[str, ...]:
    """Errors of quitting the application through its bundle id, a process alive at the deadline terminated."""
    if not processes:
        return ()
    await anyio.run_process(["/usr/bin/osascript", "-e", "ignoring application responses", "-e", f'tell application id "{identifier}" to quit', "-e", "end ignoring"])
    _, alive = await anyio.to_thread.run_sync(psutil.wait_procs, processes, DEADLINE)
    for process in alive:
        process.terminate()
    _, left = await anyio.to_thread.run_sync(psutil.wait_procs, alive, DEADLINE)
    return tuple(f"pid {process.pid} still runs after its termination" for process in left)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DEADLINE", "LOOPBACK", "Applied", "Application", "Change", "Error", "Failed", "Host", "Outcome", "application", "parse", "quit_application"]
