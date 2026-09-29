"""Host side every application's run shares: the facts it reads, the bundle and processes it drives, the report it decodes, and the outcome it returns."""

from collections.abc import AsyncIterator, Callable, Mapping, Sequence
from contextlib import asynccontextmanager
from pathlib import Path
import plistlib
from string.templatelib import Interpolation, Template
import sys
from typing import Final

import anyio
import httpx
import msgspec
import psutil

from interface.report import Kind
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Line = Header | Change | Skip | Measurement | Error

# --- [CONSTANTS] ------------------------------------------------------------------------

DEADLINE: Final = 300.0
LOOPBACK: Final = "127.0.0.1"
LAUNCH_ENVIRONMENT: Final = frozendict[str, str]()

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts one application's run reads and the HTTP client every run shares."""

    app: str
    root: Path
    environ: Mapping[str, str]
    units: Units
    client: httpx.AsyncClient

    @property
    def artifacts(self) -> Path:
        """Folder under the repository's `.artifacts/` holding the application's logs, reports, and built packages."""
        return self.root / ".artifacts" / self.app

    @property
    def cache(self) -> Path:
        """Folder under the repository's `.cache/` holding the packages the application stages across runs."""
        return self.root / ".cache" / self.app


class Bundle(msgspec.Struct, frozen=True, rename={"identifier": "CFBundleIdentifier", "name": "CFBundleName", "version": "CFBundleShortVersionString", "channel": "RELEASECHANNEL"}):
    """Application bundle by its folder, main executable, and the `Info.plist` keys the host reads, the release channel absent from a bundle that declares none."""

    path: Path
    identifier: str
    executable: Path
    name: str
    version: str
    channel: str | None = None


class Header(msgspec.Struct, frozen=True, array_like=True, tag=Kind.HEADER.value):
    """Report row naming the application's version and the store folder its changes land in."""

    version: str
    folder: str


class Change(msgspec.Struct, frozen=True, array_like=True, tag=Kind.CHANGE.value):
    """Report row of one value the run changed, as read before its write and as written."""

    label: str
    before: str
    after: str


class Skip(msgspec.Struct, frozen=True, array_like=True, tag=Kind.SKIP.value):
    """Report row of one absent, disabled, or empty add-on, plug-in, or library the run skipped."""

    name: str


class Measurement(msgspec.Struct, frozen=True, array_like=True, tag=Kind.MEASUREMENT.value):
    """Report row holding the JSON record the application measured for the host's file stage."""

    record: str


class Error(msgspec.Struct, frozen=True, array_like=True, tag=Kind.ERROR.value):
    """Report row of one failure, and the value a host step returns for an expected failure."""

    text: str


class Applied(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run that reported no failure, with the store folder its changes landed in."""

    app: str
    version: str
    folder: Path
    changes: tuple[Change, ...] = ()
    skipped: tuple[str, ...] = ()
    stderr: tuple[str, ...] = ()


class Failed(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Run that could not reach the application, produced no report, or reported a failure."""

    app: str
    errors: tuple[str, ...]
    changes: tuple[Change, ...] = ()
    stderr: tuple[str, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [REPORT]
def parse(text: str) -> tuple[Line, ...]:
    """Rows of a report, one per line, the first tab-separated cell naming the row's kind."""
    return msgspec.convert([line.split("\t") for line in text.splitlines() if line], tuple[Line, ...])


def outcome(app: str, rows: Sequence[Line]) -> Applied | Failed:
    """Outcome of a report's rows, applied when they hold a header and no error."""
    changes = tuple(row for row in rows if isinstance(row, Change))
    match [row for row in rows if isinstance(row, Header)], tuple(row.text for row in rows if isinstance(row, Error)):
        case [Header(version=version, folder=folder), *_], ():
            return Applied(app, version, Path(folder), changes, tuple(row.name for row in rows if isinstance(row, Skip)))
        case [], ():
            return Failed(app, ("the report holds no header row",), changes)
        case _, errors:
            return Failed(app, errors, changes)


# --- [SOURCE]
def literal(value: object) -> str:
    """Value as JSON text, a literal in AppleScript and JavaScript source."""
    return msgspec.json.encode(value).decode()


def rendered(template: Template, spelling: Callable[[object], str] = literal) -> str:
    """Source text of the template, each interpolation's value spelled by `spelling`, or as its formatted text under the `!s` conversion."""

    def spelled(part: str | Interpolation[object]) -> str:
        match part:
            case str():
                return part
            case Interpolation(conversion="s"):
                return format(part.value, part.format_spec)
            case _:
                return spelling(part.value)

    return "".join(map(spelled, template))


def bootstrap(module: str, call: Template, *folders: Path) -> str:
    """Python source evaluating the call on the module imported from the `tools` folder and `folders`, after evicting every module their Python files and directories name, bytecode written under the host's cache prefix."""
    roots = tuple(folder.resolve() for folder in (Path(__file__).parents[1], *folders))
    tops = sorted({path.stem for root in roots for path in root.iterdir() if path.suffix == ".py" or path.is_dir()})
    return rendered(
        t"import importlib, sys\n"
        t"sys.pycache_prefix = {sys.pycache_prefix}\n"
        t"for name in [name for name in sys.modules if name.partition('.')[0] in {tops}]:\n"
        t"    del sys.modules[name]\n"
        t"sys.path[:0] = [root for root in {tuple(map(str, roots))} if root not in sys.path]\n"
        t"importlib.import_module({module})." + call,
        repr,
    )


# --- [BUNDLE]
async def bundle(path: Path) -> Bundle:
    """Bundle at the folder from its `Info.plist`, the executable and name taken from the folder where the plist names none, as CoreFoundation resolves them."""
    info = plistlib.loads(await anyio.Path(path, "Contents", "Info.plist").read_bytes())
    return msgspec.convert({"CFBundleName": path.stem, **info, "path": path, "executable": path.joinpath("Contents", "MacOS", info.get("CFBundleExecutable", path.stem))}, Bundle)


async def located(identifier: str) -> tuple[Bundle, ...]:
    """Every bundle Spotlight finds by the bundle id, in its order."""
    return await anyio.gather(*(bundle(Path(found)) for found in (await anyio.run_process(["/usr/bin/mdfind", f"kMDItemCFBundleIdentifier == '{identifier}'"])).stdout.decode().splitlines()))


# --- [PROCESS]
def running(application: Bundle) -> tuple[psutil.Process, ...]:
    """Every process running the bundle's main executable, its `info` holding the executable and command line read in one snapshot."""
    return tuple(process for process in psutil.process_iter(["exe", "cmdline"]) if process.info["exe"] == str(application.executable))


async def launch(application: Bundle, *arguments: str, files: Sequence[str] = (), environment: Mapping[str, str] = frozendict()) -> None:
    """Start a new background instance of the bundle on the files with the arguments, its environment the given variables and none of the host's."""
    variables = (part for name, value in environment.items() for part in ("--env", f"{name}={value}"))
    await anyio.run_process(["/usr/bin/open", "-n", "-g", "-a", str(application.path), *variables, *files, *(("--args", *arguments) if arguments else ())], env=LAUNCH_ENVIRONMENT)


async def terminated(processes: Sequence[psutil.Process]) -> tuple[str, ...]:
    """Errors naming each process alive at the deadline after its termination, a process gone before the signal skipped."""

    def signaled(process: psutil.Process) -> bool:
        try:
            process.terminate()
        except psutil.NoSuchProcess:
            return False
        return True

    _, alive = await anyio.to_thread.run_sync(psutil.wait_procs, [process for process in processes if signaled(process)], DEADLINE)
    return tuple(f"pid {process.pid} runs past its termination" for process in alive)


async def quitted(application: Bundle, processes: Sequence[psutil.Process]) -> tuple[str, ...]:
    """Errors of quitting the bundle's processes, the quit sent without awaiting its reply and each process alive at the deadline terminated."""
    if not processes:
        return ()
    script = rendered(t"tell application id {application.identifier} to quit")
    await anyio.run_process(["/usr/bin/osascript", "-e", "ignoring application responses", "-e", script, "-e", "end ignoring"])
    _, alive = await anyio.to_thread.run_sync(psutil.wait_procs, processes, DEADLINE)
    return await terminated(alive)


@asynccontextmanager
async def reopened(application: Bundle, discovered: Sequence[psutil.Process], *files: str, arguments: Sequence[str] = ()) -> AsyncIterator[None]:
    """Scope that launches the bundle on the files at its exit, cancellation included, when an instance ran at discovery."""
    try:
        yield
    finally:
        if discovered:
            with anyio.CancelScope(shield=True):
                await launch(application, *arguments, files=files)


# --- [NETWORK]
async def downloaded(client: httpx.AsyncClient, url: str, target: Path) -> Path | Error:
    """Target holding the address's response body, streamed into a part file beside it and moved into place, or the failed request."""
    part = anyio.Path(target.with_name(f"{target.name}.part"))
    await part.parent.mkdir(parents=True, exist_ok=True)
    try:
        async with client.stream("GET", url) as response, await anyio.open_file(part, "wb") as sink:
            response.raise_for_status()
            async for chunk in response.aiter_bytes():
                await sink.write(chunk)
    except httpx.HTTPError as error:
        await part.unlink(missing_ok=True)
        return Error(f"GET {url} failed with {error!r}")
    return Path(await part.replace(target))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DEADLINE",
    "LAUNCH_ENVIRONMENT",
    "LOOPBACK",
    "Applied",
    "Bundle",
    "Change",
    "Error",
    "Failed",
    "Header",
    "Host",
    "Line",
    "Measurement",
    "Skip",
    "bootstrap",
    "bundle",
    "downloaded",
    "launch",
    "literal",
    "located",
    "outcome",
    "parse",
    "quitted",
    "rendered",
    "reopened",
    "running",
    "terminated",
]
