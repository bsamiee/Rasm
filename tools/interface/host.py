"""Host side every application's run shares, with the report it decodes and the outcome it returns."""

from collections.abc import Mapping, Sequence
from enum import auto, StrEnum
from pathlib import Path
import plistlib
from types import MappingProxyType
from typing import Final

import anyio
import msgspec
import psutil

from interface.report import Kind
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Line = Header | Change | Skip | Measure | Plugin | Error
type Outcome = Applied | Failed


class Stage(StrEnum):
    """Phase of a run a failure names, the launch that writes or the quit."""

    RUN = auto()
    QUIT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

DEADLINE: Final = 300.0
LOOPBACK: Final = "127.0.0.1"
INFO: Final = ("Contents", "Info.plist")
LAUNCH_ENVIRONMENT: Final = MappingProxyType[str, str]({})

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts one application's run reads, the `.mcp.json` servers left for the application to decode."""

    app: str
    folder: Path
    root: Path
    servers: msgspec.Raw
    environ: Mapping[str, str]
    units: Units

    @property
    def artifacts(self) -> Path:
        """Folder under the repository's `.artifacts/` holding each run's logs, reports, and built packages."""
        return self.root / ".artifacts" / self.folder.relative_to(self.root)

    @property
    def cache(self) -> Path:
        """Folder under the repository's `.cache/` holding the packages an application stages across runs."""
        return self.root / ".cache" / self.folder.relative_to(self.root)


class Application(msgspec.Struct, frozen=True):
    """Bundle folder and bundle id of an application."""

    path: Path
    identifier: str


class Info(msgspec.Struct, frozen=True, rename={"identifier": "CFBundleIdentifier", "executable": "CFBundleExecutable"}):
    """Bundle's `Info.plist` keys the host reads, the executable absent from a helper bundle that declares none."""

    identifier: str
    executable: str | None = None


class Header(msgspec.Struct, frozen=True, array_like=True, tag=Kind.HEADER.value):
    """Report row naming the application's version and settings folder."""

    version: str
    settings: str


class Change(msgspec.Struct, frozen=True, array_like=True, tag=Kind.CHANGE.value):
    """Report row of one value the run changed, as read before its write and as written."""

    label: str
    before: str
    after: str


class Skip(msgspec.Struct, frozen=True, array_like=True, tag=Kind.SKIP.value):
    """Report row of one absent, disabled, or empty add-on, plug-in, or library the run skipped."""

    name: str


class Measure(msgspec.Struct, frozen=True, array_like=True, tag=Kind.MEASURE.value):
    """Report row of one extent the application measured for the host's file stage."""

    name: str
    value: float


class Plugin(msgspec.Struct, frozen=True, array_like=True, tag=Kind.PLUGIN.value):
    """Report row of one installed package plug-in and the toolbar file it includes."""

    package: str
    toolbars: str | None = None


class Error(msgspec.Struct, frozen=True, array_like=True, tag=Kind.ERROR.value):
    """Report row of one failure."""

    text: str


class Applied(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run that reported no failure."""

    app: str
    version: str
    settings: str
    changes: tuple[Change, ...]
    skipped: tuple[str, ...]
    measures: dict[str, float]
    plugins: tuple[Plugin, ...] = ()
    stderr: tuple[str, ...] = ()


class Failed(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run that could not reach the application, produced no report, or reported a failure."""

    app: str
    errors: tuple[str, ...]
    changes: tuple[Change, ...] = ()
    stage: Stage = Stage.RUN
    stderr: tuple[str, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [REPORT]
def parse(app: str, text: str) -> Outcome:
    """Outcome of a report, applied when it holds a header and no error row."""
    rows = msgspec.convert([line.split("\t") for line in text.splitlines() if line], tuple[Line, ...], strict=False)
    changes = tuple(row for row in rows if isinstance(row, Change))
    match [row for row in rows if isinstance(row, Header)], tuple(row.text for row in rows if isinstance(row, Error)):
        case [Header(version, settings), *_], ():
            skipped, measures = tuple(row.name for row in rows if isinstance(row, Skip)), {row.name: row.value for row in rows if isinstance(row, Measure)}
            return Applied(app, version, settings, changes, skipped, measures, tuple(row for row in rows if isinstance(row, Plugin)))
        case _, errors:
            return Failed(app, errors, changes)


# --- [ENTRY]
def bootstrap(module: str, call: str) -> str:
    """Python source evaluating the call on the module freshly imported from its package's folder."""
    package, folder = module.partition(".")[0], str(Path(__file__).resolve().parents[1])
    return "\n".join((
        "import importlib, sys",
        "sys.dont_write_bytecode = True",
        f"for name in [name for name in sys.modules if name.partition('.')[0] == {package!r}]:\n    del sys.modules[name]",
        f"if {folder!r} not in sys.path:\n    sys.path.insert(0, {folder!r})",
        f"importlib.import_module({module!r}).{call}",
    ))


# --- [PROCESS]
async def plist[T](bundle: Path, kind: type[T]) -> T:
    """Keys of the bundle's `Info.plist` the kind names."""
    return msgspec.convert(plistlib.loads(await anyio.Path(bundle, *INFO).read_bytes()), kind)


async def application(executable: Path) -> Application | None:
    """Bundle whose `Contents/MacOS` holds the executable under its declared name, None for any other."""
    if (parents := executable.parents)[0].name != "MacOS" or parents[1].name != "Contents" or not await anyio.Path(bundle := parents[2], *INFO).exists():
        return None
    info = await plist(bundle, Info)
    return Application(bundle, info.identifier) if executable.name == info.executable else None


async def terminated(processes: Sequence[psutil.Process]) -> tuple[str, ...]:
    """Errors of terminating the processes, naming each alive at the deadline."""
    for process in processes:
        process.terminate()
    _, left = await anyio.to_thread.run_sync(psutil.wait_procs, processes, DEADLINE)
    return tuple(f"pid {process.pid} still runs after its termination" for process in left)


async def quit_application(bundle: Path, processes: Sequence[psutil.Process]) -> tuple[str, ...]:
    """Errors of quitting the application through the bundle folder the run resolved, a process alive at the deadline terminated."""
    if not processes:
        return ()
    await anyio.run_process(["/usr/bin/osascript", "-e", "ignoring application responses", "-e", f'tell application "{bundle}" to quit', "-e", "end ignoring"])
    _, alive = await anyio.to_thread.run_sync(psutil.wait_procs, processes, DEADLINE)
    return await terminated(alive)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DEADLINE",
    "LAUNCH_ENVIRONMENT",
    "LOOPBACK",
    "Applied",
    "Application",
    "Change",
    "Error",
    "Failed",
    "Host",
    "Info",
    "Outcome",
    "Plugin",
    "Stage",
    "application",
    "bootstrap",
    "parse",
    "plist",
    "quit_application",
    "terminated",
]
