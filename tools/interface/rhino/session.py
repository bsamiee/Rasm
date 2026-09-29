"""Rhino's transport and lifecycle: the Rhino the run drives, its document listeners, and `script` calls through `run_python`."""

from datetime import timedelta
from pathlib import Path
import re
import socket
from string.templatelib import Template
from typing import Final

import anyio
from anyio.abc import SocketListener
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent
import msgspec
import psutil

from interface.host import bootstrap, Bundle, bundle, DEADLINE, Error, Host, launch, Line, LOOPBACK, Measurement, parse

# --- [CONSTANTS] ------------------------------------------------------------------------

SCRIPT: Final = "interface.rhino.script"

# --- [MODELS] ---------------------------------------------------------------------------


class Rhino(msgspec.Struct, frozen=True):
    """Rhino bundle, its `yak`, the folder its document listeners announce in, and the package folder and installed version by yak id `yak list` reports."""

    bundle: Bundle
    yak: Path
    listeners: Path
    directory: Path
    installed: frozendict[str, str]

    @property
    def release(self) -> tuple[int, int]:
        """Major and minor release of the bundle's version."""
        major, minor = map(int, self.bundle.version.split(".")[:2])
        return major, minor

    @classmethod
    async def resolve(cls, host: Host) -> Rhino | Error:
        """Rhino of the `RHINO_PATH` bundle, or the `yak list` output naming no package folder."""
        application = await bundle(Path(host.environ["RHINO_PATH"]))
        yak = application.path / "Contents" / "Resources" / "bin" / "yak"
        listing = (await anyio.run_process([yak, "list"])).stdout.decode()
        match re.search(r"^Package directory: (.+)$", listing, re.MULTILINE):
            case re.Match() as found:
                listeners = Path(host.environ["HOME"], "Library", "Application Support", "McNeel", "Rhinoceros", "ai", "listeners")
                return cls(application, yak, listeners, Path(found[1]), frozendict(re.findall(r"^(.+) \(([^()]+)\)$", listing, re.MULTILINE)))
            case None:
                return Error(f"{yak} list printed no package directory")


class Output(msgspec.Struct, frozen=True):
    """Merged text blocks of a `run_python` result."""

    stdout: str = ""
    stderr: str = ""
    message: str = ""


# --- [OPERATIONS] -----------------------------------------------------------------------


async def run(port: int, call: Template) -> tuple[tuple[Line, ...], tuple[str, ...]]:
    """Report rows one `script` call through the port's listener printed, the message of its raise as an error row, and its error output lines."""
    async with streamable_http_client(f"http://{LOOPBACK}:{port}/") as (read, write, _), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        result = await session.call_tool("run_python", {"script": bootstrap(SCRIPT, call)})
    output = msgspec.convert({key: value for block in result.content if isinstance(block, TextContent) for key, value in msgspec.json.decode(block.text, type=dict[str, str]).items()}, Output)
    return (*parse(output.stdout), *((Error(output.message),) if result.isError else ())), tuple(output.stderr.splitlines())


async def announced(rhino: Rhino, process: psutil.Process) -> int | Error:
    """Lowest port the process listens on that one listing of the announcement folder names, or the process listening on none."""
    names = {path.name async for path in anyio.Path(rhino.listeners).iterdir()}
    ports = sorted(connection.laddr.port for connection in process.net_connections("tcp") if connection.status == psutil.CONN_LISTEN)
    return next(
        (port for port in ports if not names.isdisjoint((f"{process.pid}-{port}.json", f"{process.pid}-{port}.json.tmp"))),
        Error(f"Rhino pid {process.pid} listens on no port announced in {rhino.listeners}, its documents stay unread"),
    )


async def reported(port: int) -> tuple[tuple[str, ...], tuple[str, ...]]:
    """Titled document paths the `documents` call through the port's listener reported, and the errors naming each titled document with unsaved edits."""
    rows, _ = await run(port, t"documents()")
    paths = tuple(path for row in rows if isinstance(row, Measurement) for path in msgspec.json.decode(row.record, type=tuple[str, ...]))
    return paths, tuple(row.text for row in rows if isinstance(row, Error))


async def released(port: int) -> tuple[str, ...]:
    """Errors of the `release` call through the port's listener marking every untitled document unmodified."""
    rows, _ = await run(port, t"release()")
    return tuple(row.text for row in rows if isinstance(row, Error))


async def launched(rhino: Rhino) -> int | Error:
    """Listener port a Rhino launched in the background sends from its startup file once the first document's listener announced, or the deadline passing first."""
    server = socket.create_server((LOOPBACK, 0))
    async with await SocketListener.from_socket(server) as listener, anyio.TemporaryDirectory() as folder:
        startup = anyio.Path(folder, "startup.py")
        await startup.write_text(bootstrap(SCRIPT, t"ready({LOOPBACK}, {server.getsockname()[1]})"))
        await launch(rhino.bundle, f'-runscript=_NoEcho _-ScriptEditor _Run "{startup}"', environment={"RHINO_MCP_AUTOSTART_PORT": "1"})
        with anyio.move_on_after(DEADLINE):
            async with await listener.accept() as stream:
                return int(await stream.receive())
    return Error(f"Launched Rhino sent no listener port within {DEADLINE:.0f} s")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rhino", "announced", "launched", "released", "reported", "run"]
