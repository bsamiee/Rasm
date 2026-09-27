# ruff: file-ignore[django-extra]
"""Applies the interface to Rhino and the packages `packages.toml` declares, then edits the files Rhino reads at launch once it quit."""

from collections.abc import Mapping
from datetime import timedelta
from pathlib import Path
import shutil
from typing import Final

import anyio
from anyio.abc import SocketAttribute, SocketStream
from anyio.streams.stapled import MultiListener
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent
import msgspec
import psutil

from interface.host import Applied, bootstrap, DEADLINE, Error, Failed, Host, Info, LAUNCH_ENVIRONMENT, LOOPBACK, Outcome, parse, plist, quit_application, Stage
from interface.rhino import stores
from interface.rhino.packages import declared, Package, upgrade
from interface.rhino.window import bands, Extent, Site

# --- [CONSTANTS] ------------------------------------------------------------------------

SCRIPT: Final = f"{__package__}.script"

# --- [MODELS] ---------------------------------------------------------------------------


class Output(msgspec.Struct, frozen=True):
    """Merged text blocks of a `run_python` result."""

    stdout: str = ""
    stderr: str = ""
    error: str = ""
    message: str = ""
    guidance: str = ""


class Listener(msgspec.Struct, frozen=True):
    """Process and port of a live listener."""

    pid: int
    port: int


class Router(msgspec.Struct, frozen=True):
    """Bundle and bundle id of the Rhino whose `yak` the environment's PATH names and the folder the router beside it reads listener announcements from."""

    bundle: Path
    identifier: str
    listeners: anyio.Path

    @classmethod
    async def resolve(cls, environ: Mapping[str, str]) -> Router | None:
        """Router of the PATH's `yak` and `rhino-mcp-router`, None while either is absent."""
        match shutil.which("yak", path=environ["PATH"]), shutil.which("rhino-mcp-router", path=environ["PATH"]):
            case str(yak), str(router):
                bundle = Path(yak).parents[3]
                return cls(bundle, (await plist(bundle, Info)).identifier, anyio.Path(Path(router).parents[1], "listeners"))
            case _:
                return None


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [LISTENER]
def running(router: Router) -> tuple[int, ...]:
    """Process id of each running Rhino, its executable in the router bundle's `Contents/MacOS`."""
    macos = router.bundle / "Contents" / "MacOS"
    return tuple(process.pid for process in psutil.process_iter(["exe"]) if (exe := process.info["exe"]) and Path(exe).parent == macos)


async def announced(router: Router, pid: int) -> Listener | None:
    """Listener on the process's lowest listening port the folder announces."""
    names = {path.name async for path in router.listeners.iterdir()}
    ports = sorted(connection.laddr.port for connection in psutil.Process(pid).net_connections("tcp") if connection.status == psutil.CONN_LISTEN)
    return next((Listener(pid, port) for port in ports if not names.isdisjoint((f"{pid}-{port}.json", f"{pid}-{port}.json.tmp"))), None)


async def connected(ready: MultiListener[SocketStream]) -> int:
    """Process id Rhino sends over the first connection the listener accepts."""
    send, receive = anyio.create_memory_object_stream[int](1)
    with send, receive:
        async with anyio.create_task_group() as group:

            async def handle(stream: SocketStream) -> None:
                async with stream:
                    send.send_nowait(int(await stream.receive()))
                group.cancel()

            await ready.serve(handle, group)
        return receive.receive_nowait()


async def launch(app: str, router: Router) -> Listener | Failed:
    """Listener of a Rhino launched in the background whose startup file sends its process id back, failed without a process id by the deadline or a listener it announces."""
    async with await anyio.create_tcp_listener(local_host=LOOPBACK) as ready, anyio.TemporaryDirectory() as folder:
        startup = anyio.Path(folder, "startup.py")
        await startup.write_text(bootstrap(SCRIPT, f"ready({LOOPBACK!r}, {ready.extra(SocketAttribute.local_port)})"))
        await anyio.run_process(["/usr/bin/open", "-g", "-b", router.identifier, "--args", f'-runscript=_NoEcho _-ScriptEditor _Run "{startup}"'], env=LAUNCH_ENVIRONMENT)
        with anyio.move_on_after(DEADLINE):
            pid = await connected(ready)
            listener = await announced(router, pid)
            return Failed(app, (f"launched Rhino pid {pid} announces no listener in {router.listeners}",)) if listener is None else listener
    return Failed(app, (f"launched Rhino sent no process id within {DEADLINE:.0f} s",))


async def run(app: str, listener: Listener, call: str) -> Outcome:
    """Outcome of one `script.py` call through the listener, a raise's text joined to its errors."""
    async with streamable_http_client(f"http://{LOOPBACK}:{listener.port}/") as (read, write, _), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        result = await session.call_tool("run_python", {"script": bootstrap(SCRIPT, call)})
    output = msgspec.convert({key: value for block in result.content if isinstance(block, TextContent) for key, value in msgspec.json.decode(block.text, type=dict[str, str]).items()}, Output)
    raised = tuple(line for text in (output.error, output.message, output.guidance, output.stderr) for line in text.splitlines())
    match parse(app, output.stdout), result.isError:
        case outcome, False:
            return msgspec.structs.replace(outcome, stderr=tuple(output.stderr.splitlines()))
        case Failed() as failed, True:
            return msgspec.structs.replace(failed, errors=(*failed.errors, *raised))
        case Applied(changes=changes), True:
            return Failed(app, raised, changes)


async def released(app: str, listener: Listener) -> tuple[str, ...]:
    """Errors naming each titled Rhino or Grasshopper 2 document the listener's Rhino holds with unsaved edits, the untitled ones released."""
    match await run(app, listener, "release()"):
        case Failed(errors=errors):
            return errors
        case Applied():
            return ()


async def quit_running(app: str, router: Router) -> tuple[str, ...]:
    """Errors of quitting every running Rhino, none quit while one announces no listener or holds a titled document with unsaved edits."""
    held = {pid: await announced(router, pid) for pid in running(router)}
    match tuple([
        error
        for pid, listener in held.items()
        for error in (await released(app, listener) if listener is not None else (f"Rhino pid {pid} announces no listener in {router.listeners}. Its documents stay unread. Quit it and rerun",))
    ]):
        case ():
            return await quit_application(router.bundle, [psutil.Process(pid) for pid in held])
        case errors:
            return errors


def closed(outcome: Outcome, errors: tuple[str, ...]) -> Outcome:
    """Outcome with the errors of a Rhino quit joined, failed at the quit stage with every change kept."""
    match outcome, errors:
        case _, ():
            return outcome
        case Failed() as failed, _:
            return msgspec.structs.replace(failed, errors=(*failed.errors, *errors))
        case Applied(changes=changes, stderr=stderr), _:
            return Failed(outcome.app, errors, changes, Stage.QUIT, stderr)


# --- [STAGES]
async def edit(host: Host, router: Router, configured: Applied, stage: stores.Stage) -> Outcome:
    """Settings outcome with the changes of the files edited once Rhino quit, failed with an element the files lack and every change already made."""
    if errors := await quit_running(host.app, router):
        return closed(configured, errors)
    match await anyio.to_thread.run_sync(stores.edit, stage):
        case Error(text=text):
            return Failed(host.app, (text,), configured.changes)
        case changes:
            return msgspec.structs.replace(configured, changes=(*configured.changes, *changes))


async def configure(host: Host, router: Router, packages: tuple[Package, ...]) -> Outcome:
    """Outcome of the settings and declared packages applied inside a fresh Rhino, then of the file edit once it quit."""
    match await launch(host.app, router):
        case Failed() as failed:
            return failed
        case listener:
            match await run(host.app, listener, f"main(__rhino_doc__, {tuple(package.id for package in packages)!r})"):
                case Applied() as configured:
                    measures = {member: configured.measures[member] for member in (*Site, *Extent)}
                    columns = {name: value for name, value in configured.measures.items() if name not in measures}
                    stage = stores.Stage(Path(configured.settings), router.bundle, measures, bands(measures), columns, configured.plugins, packages)
                    return await edit(host, router, configured, stage)
                case Failed() as failed:
                    return failed


async def converge(host: Host, router: Router, packages: tuple[Package, ...]) -> Outcome:
    """Outcome of a fresh Rhino's configuration, Rhino reopened on the edited files when one ran at discovery and quit when none did, the quit's errors joined and the changes kept."""
    await router.listeners.mkdir(parents=True, exist_ok=True)
    discovered = running(router)
    if errors := await quit_running(host.app, router):
        return Failed(host.app, errors, stage=Stage.QUIT)
    outcome = await configure(host, router, packages)
    if discovered:
        await anyio.run_process(["/usr/bin/open", "-g", "-b", router.identifier], env=LAUNCH_ENVIRONMENT)
        return outcome
    return closed(outcome, await quit_running(host.app, router))


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Outcome]:
    """Rhino's one outcome over the packages `packages.toml` declares, failed when PATH names no `yak` and router pair."""
    match await Router.resolve(host.environ):
        case None:
            return (Failed(host.app, ("PATH names no yak beside a Rhino bundle and rhino-mcp-router beside its listeners folder",)),)
        case router:
            return (await converge(host, router, await declared()),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply", "upgrade"]
