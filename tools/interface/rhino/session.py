"""Rhino's transport and lifecycle: the Rhino the run drives, its document listeners, and `script` calls through `run_python`."""

from datetime import timedelta
from pathlib import Path
import re
from string.templatelib import Template
from typing import Annotated, Final

import anyio
from anyio.abc import SocketAttribute, SocketStream
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent
import msgspec
import psutil
from pydantic import DirectoryPath, Field

from interface.host import bootstrap, Bundle, bundle, DEADLINE, environment, Error, Home, Host, launch, Line, LOOPBACK, Measurement, parse

# --- [CONSTANTS] ------------------------------------------------------------------------

SCRIPT: Final = "interface.rhino.script"

# --- [MODELS] ---------------------------------------------------------------------------


class RhinoEnvironment(Home, frozen=True):
    """Variables a Rhino run reads beside the home folder: the Rhino bundle."""

    rhino_path: Annotated[DirectoryPath, Field(alias="RHINO_PATH")]


class Rhino(msgspec.Struct, frozen=True):
    """Rhino bundle, its `yak`, its application data folder, and the package folder and installed version by yak id `yak list` reports."""

    bundle: Bundle
    yak: Path
    data: Path
    directory: Path
    installed: frozendict[str, str]

    @property
    def release(self) -> tuple[int, int]:
        """Major and minor release of the bundle's version."""
        major, minor = map(int, self.bundle.version.split(".")[:2])
        return major, minor

    @classmethod
    async def resolve(cls, host: Host) -> Rhino | Error:
        """Rhino of the `RHINO_PATH` bundle, or the environment variables it refused, or the `yak list` output naming no package folder."""
        if isinstance(variables := environment(RhinoEnvironment, host.environ), Error):
            return variables
        application = await bundle(variables.rhino_path)
        yak = application.path / "Contents" / "Resources" / "bin" / "yak"
        listing = (await anyio.run_process([yak, "list"])).stdout.decode()
        match re.search(r"^Package directory: (.+)$", listing, re.MULTILINE):
            case re.Match() as found:
                data = variables.home.joinpath("Library", "Application Support", "McNeel", "Rhinoceros")
                return cls(application, yak, data, Path(found[1]), frozendict(re.findall(r"^(.+) \(([^()]+)\)$", listing, re.MULTILINE)))
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
    """Lowest port the process listens on that one listing of its announcement folder names, the folder under the process's `RHINO_MCP_HOME` or else Rhino's application data, or the process listening on none."""
    listeners = Path(process.info["environ"].get("RHINO_MCP_HOME") or rhino.data, "ai", "listeners")
    names = {path.name async for path in anyio.Path(listeners).iterdir()}
    ports = sorted(connection.laddr.port for connection in process.net_connections("tcp") if connection.status == psutil.CONN_LISTEN)
    return next(
        (port for port in ports if not names.isdisjoint((f"{process.pid}-{port}.json", f"{process.pid}-{port}.json.tmp"))),
        Error(f"Rhino pid {process.pid} listens on no port announced in {listeners}, its documents stay unread"),
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
    send, receive = anyio.create_memory_object_stream[bytes](1)

    async def forwarded(stream: SocketStream) -> None:
        """Forward the bytes the startup file sends over its connection."""
        async with stream:
            await send.send(await stream.receive())

    async with await anyio.create_tcp_listener(local_host=LOOPBACK) as listener, anyio.TemporaryDirectory() as folder, send, receive:
        startup = anyio.Path(folder, "startup.py")
        await startup.write_text(bootstrap(SCRIPT, t"ready({LOOPBACK}, {listener.extra(SocketAttribute.local_port)})"))
        await launch(rhino.bundle, f'-runscript=_NoEcho _-ScriptEditor _Run "{startup}"', environment={"RHINO_MCP_AUTOSTART_PORT": "1"})
        with anyio.move_on_after(DEADLINE):
            async with anyio.create_task_group() as group:
                serving = group.start_soon(listener.serve, forwarded)
                sent = await receive.receive()
                serving.cancel()
                return int(sent)
    return Error(f"Launched Rhino sent no listener port within {DEADLINE:.0f} s")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rhino", "announced", "launched", "released", "reported", "run"]
