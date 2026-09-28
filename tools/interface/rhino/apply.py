# ruff: file-ignore[django-extra]
"""Applies the interface to Rhino and the packages `packages.toml` declares, then edits the files Rhino reads at launch once it quit."""

from datetime import timedelta
from pathlib import Path
import shutil
from string.templatelib import Template
from typing import Final

import anyio
from anyio.abc import SocketAttribute, SocketStream
from anyio.streams.stapled import MultiListener
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent
import msgspec
import psutil

from interface.host import Applied, bootstrap, Bundle, bundle, DEADLINE, Error, Failed, Host, joined, launch, LOOPBACK, Measurement, outcome, parse, quitted, reopened, running
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


class Measured(msgspec.Struct, frozen=True):
    """Record a configured Rhino reports: each measure by name, and each installed package plug-in's package id with the toolbar file it includes."""

    measures: dict[str, float]
    plugins: tuple[tuple[str, str | None], ...]


class Router(msgspec.Struct, frozen=True):
    """Rhino bundle the `rhino-mcp-platform` server row names and the folder the router on the environment's PATH reads listener announcements from."""

    bundle: Bundle
    listeners: anyio.Path

    @classmethod
    async def resolve(cls, host: Host) -> Router | None:
        """Router of the server row's `RHINO_PATH` bundle and the PATH's `rhino-mcp-router`, None while PATH names no router."""
        match shutil.which("rhino-mcp-router", path=host.environ["PATH"]):
            case str(router):
                return cls(await bundle(Path(host.server("rhino-mcp-platform").env["RHINO_PATH"])), anyio.Path(Path(router).parents[1], "listeners"))
            case _:
                return None


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [LISTENER]
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


async def launched(app: str, router: Router) -> Listener | Failed:
    """Listener of a Rhino launched in the background whose startup file sends its process id back, failed without a process id by the deadline or a listener it announces."""
    async with await anyio.create_tcp_listener(local_host=LOOPBACK) as ready, anyio.TemporaryDirectory() as folder:
        startup = anyio.Path(folder, "startup.py")
        await startup.write_text(bootstrap(SCRIPT, t"ready({LOOPBACK}, {ready.extra(SocketAttribute.local_port)})"))
        await launch(router.bundle, f'-runscript=_NoEcho _-ScriptEditor _Run "{startup}"')
        with anyio.move_on_after(DEADLINE):
            pid = await connected(ready)
            listener = await announced(router, pid)
            return Failed(app, (f"launched Rhino pid {pid} announces no listener in {router.listeners}",)) if listener is None else listener
    return Failed(app, (f"launched Rhino sent no process id within {DEADLINE:.0f} s",))


async def run(app: str, listener: Listener, call: Template) -> tuple[Applied | Failed, tuple[str, ...]]:
    """Outcome of one `script.py` call through the listener, a raise's text joined to its errors, and the measured records the call printed."""
    async with streamable_http_client(f"http://{LOOPBACK}:{listener.port}/") as (read, write, _), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        result = await session.call_tool("run_python", {"script": bootstrap(SCRIPT, call)})
    output = msgspec.convert({key: value for block in result.content if isinstance(block, TextContent) for key, value in msgspec.json.decode(block.text, type=dict[str, str]).items()}, Output)
    raised = tuple(line for text in (output.error, output.message, output.guidance, output.stderr) for line in text.splitlines())
    rows = parse(output.stdout)
    records = tuple(row.record for row in rows if isinstance(row, Measurement))
    match outcome(app, rows), result.isError:
        case reported, False:
            return msgspec.structs.replace(reported, stderr=tuple(output.stderr.splitlines())), records
        case Failed() as failed, True:
            return msgspec.structs.replace(failed, errors=(*failed.errors, *raised)), records
        case Applied(changes=changes), True:
            return Failed(app, raised, changes), records


async def released(app: str, listener: Listener) -> tuple[str, ...]:
    """Errors naming each titled Rhino or Grasshopper 2 document the listener's Rhino holds with unsaved edits, the untitled ones released."""
    match await run(app, listener, t"release()"):
        case Failed(errors=errors), _:
            return errors
        case Applied(), _:
            return ()


async def quit_running(app: str, router: Router) -> tuple[str, ...]:
    """Errors of quitting every running Rhino, none quit while one announces no listener or holds a titled document with unsaved edits."""
    held = {process: await announced(router, process.pid) for process in running(router.bundle)}
    match tuple([
        error
        for process, listener in held.items()
        for error in (await released(app, listener) if listener is not None else (f"Rhino pid {process.pid} announces no listener in {router.listeners} and its documents stay unread",))
    ]):
        case ():
            return await quitted(router.bundle, tuple(held))
        case errors:
            return errors


# --- [STAGES]
async def edit(host: Host, router: Router, configured: Applied, stage: stores.Stage) -> Applied | Failed:
    """Settings outcome with the changes of the files edited once Rhino quit, failed with an element the files lack and every change already made."""
    if errors := await quit_running(host.app, router):
        return joined(configured, errors)
    match await anyio.to_thread.run_sync(stores.edit, stage):
        case Error(text=text):
            return Failed(host.app, (text,), configured.changes)
        case changes:
            return msgspec.structs.replace(configured, changes=(*configured.changes, *changes))


async def configure(host: Host, router: Router, packages: tuple[Package, ...]) -> Applied | Failed:
    """Outcome of the settings and declared packages applied inside a fresh Rhino, then of the file edit once it quit."""
    match await launched(host.app, router):
        case Failed() as failed:
            return failed
        case listener:
            match await run(host.app, listener, t"main(__rhino_doc__, {tuple(package.id for package in packages)})"):
                case Applied() as configured, records:
                    (record,) = records
                    measured = msgspec.json.decode(record, type=Measured)
                    measures = {member: measured.measures[member] for member in (*Site, *Extent)}
                    columns = {name: value for name, value in measured.measures.items() if name not in measures}
                    stage = stores.Stage(configured.folder, router.bundle.path, measures, bands(measures), columns, measured.plugins, packages)
                    return await edit(host, router, configured, stage)
                case Failed() as failed, _:
                    return failed


async def converge(host: Host, router: Router, packages: tuple[Package, ...]) -> Applied | Failed:
    """Outcome of a fresh Rhino's configuration, Rhino reopened when one ran at discovery and quit when none did, the quit's errors joined and the changes kept."""
    await router.listeners.mkdir(parents=True, exist_ok=True)
    discovered = running(router.bundle)
    if errors := await quit_running(host.app, router):
        return Failed(host.app, errors)
    async with reopened(router.bundle, discovered):
        configured = await configure(host, router, packages)
    return configured if discovered else joined(configured, await quit_running(host.app, router))


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed, ...]:
    """Rhino's one outcome over the packages `packages.toml` declares, failed when PATH names no `rhino-mcp-router`."""
    match await Router.resolve(host):
        case None:
            return (Failed(host.app, ("PATH names no rhino-mcp-router beside its listeners folder",)),)
        case router:
            return (await converge(host, router, await declared()),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply", "upgrade"]
