"""Host side every application's run shares: the facts and environment it reads, the bundle, processes, and instances it drives, the report it decodes, and the outcome it returns."""

from collections.abc import AsyncGenerator, Callable, Mapping, Sequence
from contextlib import asynccontextmanager
from functools import partial
from pathlib import Path
import plistlib
import shlex
from string.templatelib import Interpolation, Template
from subprocess import CalledProcessError
import sys
from typing import Annotated, Final, override

import anyio
from AppKit import NSRunningApplication, NSWorkspace, NSWorkspaceOpenConfiguration
from CoreFoundation import CFRunLoopGetMain, CFRunLoopPerformBlock, CFRunLoopWakeUp, kCFRunLoopCommonModes
from Foundation import NSDictionary, NSError, NSKeyValueChangeNewKey, NSKeyValueObservingOptionInitial, NSKeyValueObservingOptionNew, NSObject, NSURL
import httpx2
import msgspec
import psutil
from pydantic import BaseModel, DirectoryPath, Field, ValidationError

from interface.report import Kind
from interface.units import Units

# --- [TYPES] ----------------------------------------------------------------------------

type Line = Header | Change | Skip | Measurement | Error

# --- [CONSTANTS] ------------------------------------------------------------------------

DEADLINE: Final = 300.0
LOOPBACK: Final = "127.0.0.1"
LAUNCH_ENVIRONMENT: Final = frozendict[str, str]()
ENDED: Final = ("terminated",)
READY: Final = ("finishedLaunching", *ENDED)

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts one application's run reads and the HTTP client every run shares."""

    app: str
    root: Path
    environ: Mapping[str, str]
    units: Units
    client: httpx2.AsyncClient

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


class Home(BaseModel, frozen=True):
    """Variables every desktop application's run reads: the home folder holding the application's preferences and data."""

    home: Annotated[DirectoryPath, Field(alias="HOME")]


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


# --- [SERVICES] -------------------------------------------------------------------------


class Observer(NSObject):
    """Key-value observer of an application instance calling its `changed` once an observed key reads true, on the main run loop that updates the instance."""

    @override
    def observeValueForKeyPath_ofObject_change_context_(self, path: str, instance: NSRunningApplication, change: NSDictionary, context: int | None) -> None:
        if change[NSKeyValueChangeNewKey]:
            self.changed()


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


# --- [ENVIRONMENT]
def environment[T: BaseModel](model: type[T], environ: Mapping[str, str]) -> T | Error:
    """Model of the variables an application's run reads, validated once over the process environment, or the error naming every variable it refused."""
    try:
        return model.model_validate(environ)
    except ValidationError as error:
        return Error("; ".join(f"{'.'.join(map(str, each['loc']))} {each['msg'].lower()}" for each in error.errors()))


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
        t"sys.path[:0] = {tuple(map(str, roots))}\n"
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
    """Every process running the bundle's main executable, its `info` holding the executable, command line, and environment read in one snapshot."""
    return tuple(process for process in psutil.process_iter(["exe", "cmdline", "environ"]) if process.info["exe"] == str(application.executable))


async def executed(command: tuple[str, ...], environment: Mapping[str, str]) -> bytes | Error:
    """Output of the command run in the environment, or the error naming its exit code and error output."""
    try:
        return (await anyio.run_process(command, env=environment)).stdout
    except CalledProcessError as error:
        return Error(f"`{shlex.join(command)}` exited with code {error.returncode}: {error.stderr.decode().strip()}")


def performed(block: Callable[[], None]) -> None:
    """Queue the block on the main run loop in its common modes and wake the loop to run it."""
    CFRunLoopPerformBlock(CFRunLoopGetMain(), kCFRunLoopCommonModes, block)
    CFRunLoopWakeUp(CFRunLoopGetMain())


async def reached(instance: NSRunningApplication, keys: tuple[str, ...]) -> bool:
    """Whether one of the instance's keys read true before the deadline, each key observed on the main run loop that updates the instance."""
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
        with anyio.move_on_after(DEADLINE):
            await done.wait()
    finally:
        performed(unobserve)
    return done.is_set()


async def registered(processes: Sequence[psutil.Process]) -> tuple[NSRunningApplication, ...]:
    """Instances Launch Services lists for the processes once each finished launching or ended, a process it lists none for skipped."""
    instances = tuple(filter(None, (NSRunningApplication.runningApplicationWithProcessIdentifier_(process.pid) for process in processes)))
    await anyio.gather(*(reached(instance, READY) for instance in instances))
    return instances


async def launch(application: Bundle, *arguments: str, files: Sequence[str] = (), environment: Mapping[str, str] = frozendict()) -> NSRunningApplication:
    """Instance of the bundle open on the files once it finished launching in the background, the live instance Launch Services lists or a new one with the arguments and only the given environment, opened after Launch Services drops every instance whose process exited."""
    exited = (instance for instance in NSRunningApplication.runningApplicationsWithBundleIdentifier_(application.identifier) if not psutil.pid_exists(instance.processIdentifier()))
    await anyio.gather(*(reached(instance, ENDED) for instance in exited))
    configuration = NSWorkspaceOpenConfiguration.configuration()
    configuration.setActivates_(False)
    configuration.setPromptsUserIfNeeded_(False)
    configuration.setAllowsRunningApplicationSubstitution_(False)
    configuration.setArguments_(arguments)
    configuration.setEnvironment_(dict(environment))
    token = anyio.lowlevel.current_token()
    send, receive = anyio.create_memory_object_stream[NSRunningApplication | NSError](1)

    def opened(instance: NSRunningApplication | None, error: NSError | None) -> None:
        anyio.from_thread.run_sync(send.send_nowait, error if instance is None else instance, token=token)

    workspace, url = NSWorkspace.sharedWorkspace(), NSURL.fileURLWithPath_(str(application.path))
    if files:
        workspace.openURLs_withApplicationAtURL_configuration_completionHandler_([NSURL.fileURLWithPath_(file) for file in files], url, configuration, opened)
    else:
        workspace.openApplicationAtURL_configuration_completionHandler_(url, configuration, opened)
    with send, receive:
        match await receive.receive():
            case NSError() as error:
                raise OSError(f"opening {application.path} failed with {error.localizedDescription()} ({error.code()})")
            case instance:
                await reached(instance, READY)
                return instance


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


async def quitted(instances: Sequence[NSRunningApplication]) -> tuple[str, ...]:
    """Errors naming each instance Launch Services still lists at the deadline after its quit and after its forced quit."""

    async def errors(instance: NSRunningApplication) -> tuple[str, ...]:
        for request in (instance.terminate, instance.forceTerminate):
            request()
            if await reached(instance, ENDED):
                return ()
        return (f"pid {instance.processIdentifier()} runs past its quit and forced quit",)

    return tuple(error for found in await anyio.gather(*map(errors, instances)) for error in found)


@asynccontextmanager
async def reopened(application: Bundle, discovered: Sequence[NSRunningApplication], *files: str, arguments: Sequence[str] = ()) -> AsyncGenerator[None]:
    """Scope that opens the files in the bundle's instance at its exit, cancellation included, when an instance ran at discovery."""
    try:
        yield
    finally:
        if discovered:
            with anyio.CancelScope(shield=True):
                await launch(application, *arguments, files=files)


# --- [NETWORK]
async def downloaded(client: httpx2.AsyncClient, url: str, target: Path) -> Path | Error:
    """Target holding the address's response body, streamed into a part file beside it and moved into place, or the failed request."""
    part = anyio.Path(target.with_name(f"{target.name}.part"))
    await part.parent.mkdir(parents=True, exist_ok=True)
    try:
        async with client.stream("GET", url) as response, await anyio.open_file(part, "wb") as sink:
            response.raise_for_status()
            async for chunk in response.aiter_bytes():
                await sink.write(chunk)
    except httpx2.HTTPError as error:
        await part.unlink(missing_ok=True)
        return Error(f"GET {url} failed with {error!r}")
    return Path(await part.replace(target))


async def fetched[T](client: httpx2.AsyncClient, url: str, kind: type[T]) -> T | Error:
    """JSON body at the address decoded as the type, or the failed request."""
    try:
        return msgspec.json.decode((await client.get(url)).raise_for_status().content, type=kind)
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
    "quitted",
    "registered",
    "rendered",
    "reopened",
    "running",
    "terminated",
]
