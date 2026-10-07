"""Shared host operations for desktop application runs."""

from collections.abc import AsyncGenerator, Callable, Mapping, Sequence
from contextlib import asynccontextmanager
from functools import partial
import os
from pathlib import Path
import shlex
from string.templatelib import Interpolation, Template
from subprocess import CalledProcessError
import sys
from typing import Annotated, Final, override

import anyio
from AppKit import NSRunningApplication, NSWorkspace, NSWorkspaceOpenConfiguration
from CoreFoundation import CFRunLoopGetMain, CFRunLoopPerformBlock, CFRunLoopWakeUp, kCFRunLoopCommonModes
from Foundation import NSBundle, NSDictionary, NSError, NSKeyValueChangeNewKey, NSKeyValueObservingOptionInitial, NSKeyValueObservingOptionNew, NSObject, NSURL
import httpx2
import msgspec
import psutil
from pydantic import BaseModel, DirectoryPath, Field, ValidationError

from interface.report import Change, Error, Header, Line, Measurement, Skip
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Applied | Failed
type Result[T] = T | Error

# --- [CONSTANTS] ------------------------------------------------------------------------

DEADLINE: Final = 300.0
LOOPBACK: Final = "127.0.0.1"
LAUNCH_ENVIRONMENT: Final = frozendict[str, str]()
ENDED: Final = ("terminated",)
READY: Final = ("finishedLaunching", *ENDED)

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Application run context with a shared HTTP client."""

    app: str
    root: Path
    environ: Mapping[str, str]
    units: Units
    client: httpx2.AsyncClient

    @property
    def artifacts(self) -> Path:
        """Application folder for logs, reports, and built packages."""
        return self.root / ".artifacts" / self.app

    @property
    def cache(self) -> Path:
        """Application folder for packages staged across runs."""
        return self.root / ".cache" / self.app


class Bundle(msgspec.Struct, frozen=True):
    """Installed application bundle metadata."""

    path: Path
    identifier: str
    executable: Path
    version: str
    channel: str | None = None


class Home(BaseModel, frozen=True):
    """Home directory containing application preferences and data."""

    home: Annotated[DirectoryPath, Field(alias="HOME")]


class Applied(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Successful application run with reported settings changes."""

    app: str
    version: str
    folder: Path
    changes: tuple[Change, ...] = ()
    skipped: tuple[str, ...] = ()
    stderr: tuple[str, ...] = ()


class Failed(msgspec.Struct, frozen=True, tag=True, tag_field="kind"):
    """Failed application access, missing report, or reported error."""

    app: str
    errors: tuple[str, ...]
    changes: tuple[Change, ...] = ()
    stderr: tuple[str, ...] = ()


# --- [SERVICES] -------------------------------------------------------------------------


class Observer(NSObject):
    """Application observer calling `changed` on the main run loop for each true property notification."""

    @override
    def observeValueForKeyPath_ofObject_change_context_(self, path: str, instance: NSRunningApplication, change: NSDictionary, context: int | None) -> None:
        if change[NSKeyValueChangeNewKey]:
            self.changed()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [REPORT]
def parse(text: str) -> tuple[Line, ...]:
    """Rows of nonempty TSV lines, each the `Line` class its lower-cased name in the first cell names, built from the cells after it."""
    rows = {row.__name__.lower(): row for row in Line.__subclasses__()}
    return tuple(rows[kind](*cells) for kind, *cells in (line.split("\t") for line in text.splitlines() if line))


def outcome(app: str, rows: Sequence[Line]) -> Outcome:
    """Return success for reports with a header and no errors."""
    changes = tuple(row for row in rows if isinstance(row, Change))
    match [row for row in rows if isinstance(row, Header)], tuple(row.text for row in rows if isinstance(row, Error)):
        case [Header(version=version, folder=folder), *_], ():
            return Applied(app, version, Path(folder), changes, tuple(row.name for row in rows if isinstance(row, Skip)))
        case [], ():
            return Failed(app, ("Report has no header row",), changes)
        case _, errors:
            return Failed(app, errors, changes)


def printed(document: object) -> None:
    """Write indented JSON to stdout with filesystem paths as strings."""
    sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode(document, enc_hook=os.fspath), indent=1) + b"\n")


# --- [ENVIRONMENT]
def environment[T: BaseModel](model: type[T], environ: Mapping[str, str]) -> Result[T]:
    """Parse process variables into a model or return every validation error."""
    try:
        return model.model_validate(environ)
    except ValidationError as error:
        return Error("; ".join(f"{'.'.join(map(str, each['loc']))} {each['msg'].lower()}" for each in error.errors()))


# --- [SOURCE]
def literal(value: object) -> str:
    """Encode a value as JSON source text."""
    return msgspec.json.encode(value).decode()


def rendered(template: Template, spelling: Callable[[object], str] = literal) -> str:
    """Render interpolations through `spelling`, with `!s` values formatted as text."""

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
    """Build Python source for a fresh module call from `tools` and `folders` using host bytecode cache settings."""
    roots = tuple(folder.resolve() for folder in (Path(__file__).parents[1], *folders))
    tops = sorted({path.stem for root in roots for path in root.iterdir() if path.suffix == ".py" or path.is_dir()})
    return rendered(
        t"import importlib, sys\n"
        t"sys.pycache_prefix = {sys.pycache_prefix}\n"
        t"for name in [name for name in sys.modules if name.partition('.')[0] in {tops}]:\n"
        t"    del sys.modules[name]\n"
        t"sys.path[:0] = {tuple(map(str, roots))}\n"
        t"importlib.import_module({module})." + call,
        repr,
    )


# --- [BUNDLE]
async def bundle(path: Path) -> Bundle:
    """Bundle at the folder as Foundation resolves it, its `pyobjc_unicode` text made exact `str`, a missing executable or version failing validation."""
    if (native := await anyio.to_thread.run_sync(NSBundle.bundleWithPath_, str(path))) is None:
        raise FileNotFoundError(f"No bundle at {path}")
    info = native.objectForInfoDictionaryKey_
    fields = {
        "path": native.bundlePath(),
        "identifier": native.bundleIdentifier(),
        "executable": native.executablePath(),
        "version": info("CFBundleShortVersionString"),
        "channel": info("RELEASECHANNEL"),
    }
    return msgspec.convert({key: str(value) if isinstance(value, str) else value for key, value in fields.items()}, Bundle, dec_hook=lambda kind, value: kind(value))


async def located(identifier: str) -> tuple[Bundle, ...]:
    """Find bundles by identifier in Spotlight result order."""
    return await anyio.gather(*(bundle(Path(found)) for found in (await anyio.run_process(["/usr/bin/mdfind", f"kMDItemCFBundleIdentifier == '{identifier}'"])).stdout.decode().splitlines()))


# --- [PROCESS]
def running(application: Bundle) -> tuple[psutil.Process, ...]:
    """Find processes by bundle executable with executable, command-line, and environment information."""
    return tuple(process for process in psutil.process_iter(["exe", "cmdline", "environ"]) if process.info["exe"] == str(application.executable))


async def executed(command: tuple[str, ...], environment: Mapping[str, str]) -> Result[bytes]:
    """Run a command with supplied variables and return stdout or an error with exit code and stderr."""
    try:
        return (await anyio.run_process(command, env=environment)).stdout
    except CalledProcessError as error:
        return Error(f"`{shlex.join(command)}` exited with code {error.returncode}: {error.stderr.decode().strip()}")


def performed(block: Callable[[], None]) -> None:
    """Schedule a block in main run loop's common modes and wake the loop."""
    CFRunLoopPerformBlock(CFRunLoopGetMain(), kCFRunLoopCommonModes, block)
    CFRunLoopWakeUp(CFRunLoopGetMain())


async def reached(instance: NSRunningApplication, keys: tuple[str, ...]) -> None:
    """Wait for one observed key to read true, the observer removed on the main run loop on return or cancellation."""
    done, observer = anyio.Event(), Observer.new()
    observer.changed = partial(anyio.from_thread.run_sync, done.set, token=anyio.lowlevel.current_token())

    def observe() -> None:
        for key in keys:
            instance.addObserver_forKeyPath_options_context_(observer, key, NSKeyValueObservingOptionInitial | NSKeyValueObservingOptionNew, None)

    def unobserve() -> None:
        for key in keys:
            instance.removeObserver_forKeyPath_(observer, key)

    performed(observe)
    try:
        await done.wait()
    finally:
        performed(unobserve)


async def registered(processes: Sequence[psutil.Process]) -> tuple[NSRunningApplication, ...]:
    """Wait up to `DEADLINE` for registered process instances to finish launching or terminate."""
    instances = tuple(filter(None, (NSRunningApplication.runningApplicationWithProcessIdentifier_(process.pid) for process in processes)))
    with anyio.fail_after(DEADLINE):
        await anyio.gather(*(reached(instance, READY) for instance in instances))
    return instances


async def launch(application: Bundle, *arguments: str, files: Sequence[str] = (), environment: Mapping[str, str] = LAUNCH_ENVIRONMENT) -> NSRunningApplication:
    """Open files within `DEADLINE` in a launched background instance with supplied arguments and environment only for new processes."""
    exited = (instance for instance in NSRunningApplication.runningApplicationsWithBundleIdentifier_(application.identifier) if not psutil.pid_exists(instance.processIdentifier()))
    configuration = NSWorkspaceOpenConfiguration.configuration()
    configuration.setActivates_(False)
    configuration.setPromptsUserIfNeeded_(False)
    configuration.setAllowsRunningApplicationSubstitution_(False)
    configuration.setArguments_(arguments)
    configuration.setEnvironment_(dict(environment))
    sent, received = anyio.create_memory_object_stream[NSRunningApplication | NSError](1)
    token = anyio.lowlevel.current_token()

    def opened(instance: NSRunningApplication | None, error: NSError | None) -> None:
        anyio.from_thread.run_sync(sent.send_nowait, instance if error is None else error, token=token)

    workspace, url = NSWorkspace.sharedWorkspace(), NSURL.fileURLWithPath_(str(application.path))
    with sent, received, anyio.fail_after(DEADLINE):
        await anyio.gather(*(reached(instance, ENDED) for instance in exited))
        if files:
            workspace.openURLs_withApplicationAtURL_configuration_completionHandler_([NSURL.fileURLWithPath_(file) for file in files], url, configuration, opened)
        else:
            workspace.openApplicationAtURL_configuration_completionHandler_(url, configuration, opened)
        match await received.receive():
            case NSError() as error:
                raise OSError(f"Opening {application.path} failed with {error.localizedDescription()} ({error.code()})")
            case instance:
                await reached(instance, READY)
    if instance.isTerminated():
        raise ChildProcessError(f"{application.path} terminated during launch")
    return instance


async def terminated(processes: Sequence[psutil.Process]) -> tuple[Error, ...]:
    """Terminate surviving processes and return errors for processes alive past `DEADLINE`."""

    def signaled(process: psutil.Process) -> bool:
        try:
            process.terminate()
        except psutil.NoSuchProcess:
            return False
        return True

    _, alive = await anyio.to_thread.run_sync(psutil.wait_procs, [process for process in processes if signaled(process)], DEADLINE)
    return tuple(Error(f"Process {process.pid} did not terminate before the deadline") for process in alive)


async def quitted(instances: Sequence[NSRunningApplication]) -> tuple[Error, ...]:
    """Quit application instances, force overdue quits, and return remaining termination errors."""

    async def survived(instance: NSRunningApplication) -> Error | None:
        for request in (instance.terminate, instance.forceTerminate):
            request()
            with anyio.move_on_after(DEADLINE):
                await reached(instance, ENDED)
                return None
        return Error(f"Process {instance.processIdentifier()} did not terminate after quit and forced quit")

    return tuple(filter(None, await anyio.gather(*map(survived, instances))))


@asynccontextmanager
async def reopened(application: Bundle, discovered: Sequence[NSRunningApplication], *files: str, arguments: Sequence[str] = ()) -> AsyncGenerator[None]:
    """Reopen files on scope exit, including cancellation, when discovery found a running instance."""
    try:
        yield
    finally:
        if discovered:
            with anyio.CancelScope(shield=True):
                await launch(application, *arguments, files=files)


# --- [NETWORK]
async def downloaded(client: httpx2.AsyncClient, url: str, target: Path) -> Result[Path]:
    """Target in a folder the caller owns holding the body the address serves, or the HTTP error."""
    await anyio.Path(target.parent).mkdir(parents=True, exist_ok=True)
    try:
        async with client.stream("GET", url) as response, await anyio.open_file(target, "wb") as sink:
            async for chunk in response.raise_for_status().aiter_bytes():
                await sink.write(chunk)
    except httpx2.HTTPError as error:
        return Error(f"GET {url} failed with {error!r}")
    return target


async def fetched[T](client: httpx2.AsyncClient, url: str, decode: Callable[[bytes], T]) -> Result[T]:
    """Fetch a body through the decoder or return an HTTP error."""
    try:
        return decode((await client.get(url)).raise_for_status().content)
    except httpx2.HTTPError as error:
        return Error(f"GET {url} failed with {error!r}")


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
    "Home",
    "Host",
    "Line",
    "Measurement",
    "Outcome",
    "Result",
    "Skip",
    "bootstrap",
    "bundle",
    "downloaded",
    "environment",
    "executed",
    "fetched",
    "launch",
    "literal",
    "located",
    "outcome",
    "parse",
    "performed",
    "printed",
    "quitted",
    "registered",
    "rendered",
    "reopened",
    "running",
    "terminated",
]
