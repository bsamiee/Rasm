"""Applies the interface to Blender: builds the extension package, quits its windows, runs `script.py` in a new instance that installs the package, writes the report, and quits, and reopens Blender."""

from pathlib import Path

import anyio
from anyio.streams.buffered import BufferedByteReceiveStream
from host import Application, application, DEADLINE, Failed, Host, LOOPBACK, Outcome, parse, quit_application
import msgspec
import psutil

# --- [MODELS] ---------------------------------------------------------------------------


class Environment(msgspec.Struct, frozen=True, rename="upper"):
    """Variables of the server row naming the executable and the bridge port."""

    blender_path: str
    blender_mcp_port: int


class Server(msgspec.Struct, frozen=True):
    """The Blender server row."""

    env: Environment


class Servers(msgspec.Struct, frozen=True):
    """The server rows Blender's run reads."""

    blender: Server


class Done(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    """The bridge's answer to an execute request whose code returned."""


class Raised(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """The bridge's answer to an execute request whose code raised, with its traceback."""

    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def windows(executable: Path) -> list[psutil.Process]:
    """Every running Blender of the executable outside background mode."""
    return [process for process in psutil.process_iter(["exe", "cmdline"]) if process.info["exe"] == str(executable) and {"-b", "--background"}.isdisjoint(process.info["cmdline"])]


def expression(host: Host, name: str, arguments: str) -> str:
    """Python that turns off bytecode writes, puts the interface folder on `sys.path`, and calls one function of `script.py` with the arguments."""
    return f"import pathlib, runpy, sys; sys.dont_write_bytecode = True; sys.path.insert(0, {str(host.folder)!r}); runpy.run_path({str(host.folder / host.app / 'script.py')!r})[{name!r}]({arguments})"


async def documents(host: Host, port: int, processes: list[psutil.Process]) -> tuple[str, ...]:
    """Errors of the script's document step run through the bridge of a window listening on the port, none when no window listens."""
    if not any(connection.laddr.port == port and connection.status == psutil.CONN_LISTEN for process in processes for connection in process.net_connections("tcp")):
        return ()
    request = msgspec.json.encode({"type": "execute", "code": f"{expression(host, 'close', '')}\nresult = {{}}", "strict_json": True})
    async with await anyio.connect_tcp(LOOPBACK, port) as stream:
        await stream.send(request + b"\0")
        reply = msgspec.json.decode(await BufferedByteReceiveStream(stream).receive_until(b"\0", 1 << 24), type=Done | Raised)
    return () if isinstance(reply, Done) else tuple(reply.message.splitlines())


async def relaunch(host: Host, executable: Path, port: int, blender: Application) -> Outcome:
    """Build the extension package, quit every Blender window, run the script in a new background instance until it exits or the deadline quits it, reopen Blender behind the frontmost application, and read the report."""
    package = anyio.Path(host.artifacts, f"{host.app}-extension.zip")
    await anyio.run_process([
        str(executable),
        "--background",
        "--factory-startup",
        "--command",
        "extension",
        "build",
        "--source-dir",
        str(host.folder / host.app / "extension"),
        "--output-filepath",
        str(package),
    ])
    if errors := await documents(host, port, running := windows(executable)) or await quit_application(blender.identifier, running):
        return Failed(host.app, errors)
    report, log, stderr = (anyio.Path(host.artifacts, f"{host.app}.{suffix}") for suffix in ("tsv", "log", "err"))
    for path in (report, log, stderr):
        await path.unlink(missing_ok=True)
    with anyio.move_on_after(DEADLINE) as waited:
        await anyio.run_process([
            "/usr/bin/open",
            "-n",
            "-g",
            "-W",
            "-a",
            str(blender.path),
            "--stdout",
            str(log),
            "--stderr",
            str(stderr),
            "--args",
            "--no-window-focus",
            "--enable-event-simulate",
            "--python-expr",
            expression(host, "run", f"pathlib.Path({str(report)!r}), pathlib.Path({str(package)!r}), {port}"),
        ])
    stuck = await quit_application(blender.identifier, windows(executable))
    await anyio.run_process(["/usr/bin/open", "-n", "-g", "-a", str(blender.path), "--args", "--no-window-focus"])
    if not await report.exists():
        ending = f"Blender ran past the {DEADLINE:.0f} s deadline and was quit" if waited.cancelled_caught else "Blender exited on its own"
        return Failed(host.app, (f"{ending} without writing {report}, its stderr in {stderr}", *stuck, *(await log.read_text(encoding="utf-8")).splitlines()[-3:]))
    return Failed(host.app, stuck) if stuck else parse(host.app, await report.read_text(encoding="utf-8"))


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> Outcome:
    """Blender's outcome, failed when its executable is no application bundle's main executable."""
    environment = msgspec.json.decode(host.servers, type=Servers, strict=False).blender.env
    executable = Path(environment.blender_path)
    match await application(executable):
        case None:
            return Failed(host.app, (f"{executable} is no application bundle's main executable",))
        case blender:
            return await relaunch(host, executable, environment.blender_mcp_port, blender)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
