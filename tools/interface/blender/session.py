"""Blender's session: the windowed Blender released through its bridge and quit, the interface run in a scripted instance, and the stored shelves edited before the released file reopens."""

import re

import anyio
from anyio.streams.buffered import BufferedByteReceiveStream
import msgspec
import psutil

from interface.blender import stores
from interface.blender.packages import Manifest
from interface.blender.rows import Launch, Listed, Local
from interface.host import Applied, bootstrap, Bundle, DEADLINE, Error, Failed, Host, LAUNCH_ENVIRONMENT, Line, literal, located, LOOPBACK, outcome, parse, quitted, reopened, running, Skip, terminated

# --- [MODELS] ---------------------------------------------------------------------------


class Held(msgspec.Struct, frozen=True):
    """File the windowed Blender holds once released, empty when untitled, and whether it holds unsaved edits."""

    filepath: str
    is_dirty: bool


class Done(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    """Bridge answer to a request that ran, with its `result` variable as JSON and the output it printed."""

    result: msgspec.Raw
    stdout: str = ""
    stderr: str = ""


class Raised(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """Bridge answer to a request that raised, with its traceback and the output it printed."""

    message: str
    stdout: str = ""
    stderr: str = ""


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BRIDGE]
async def execute(port: int, code: str, *, strict_json: bool) -> Done | Raised:
    """Answer of the Blender Lab bridge on the port to the code, `strict_json` refusing a `result` JSON cannot hold."""
    async with await anyio.connect_tcp(LOOPBACK, port) as stream:
        await stream.send(msgspec.json.encode({"type": "execute", "code": code, "strict_json": strict_json}) + b"\0")
        return msgspec.json.decode(await BufferedByteReceiveStream(stream).receive_until(b"\0", 1 << 24), type=Done | Raised)


# --- [PROCESSES]
def windowed(bundle: Bundle) -> tuple[psutil.Process, ...]:
    """Every Blender of the bundle a person runs, none in background or command mode."""
    return tuple(process for process in running(bundle) if {"-b", "--background", "-c", "--command"}.isdisjoint(process.info["cmdline"]))


async def closed(port: int, processes: tuple[psutil.Process, ...]) -> tuple[str, ...] | Error:
    """Titled file the one windowed Blender holds for the reopen once its bridge discards an untitled session, or why it stays open."""
    match processes:
        case ():
            return ()
        case (process,):
            release = "import bpy\nif not bpy.data.filepath:\n    bpy.ops.wm.read_homefile()\nresult = {'filepath': bpy.data.filepath, 'is_dirty': bpy.data.is_dirty}"
            try:
                reply = await execute(port, release, strict_json=True)
            except OSError:
                return Error(f"Blender pid {process.pid} runs with no bridge listening on port {port}, so its file stays unread")
            match reply:
                case Raised(message=message):
                    return Error(f"the release call through the Blender bridge on port {port} raised {message.rstrip()}")
                case Done(result=result):
                    match msgspec.json.decode(result, type=Held):
                        case Held(filepath=""):
                            return ()
                        case Held(filepath=path, is_dirty=True):
                            return Error(f"Blender stays open because {path} holds unsaved edits")
                        case Held(filepath=path):
                            return (path,)
        case _:
            return Error(f"{len(processes)} windowed Blender instances run and the bridge on port {port} reaches one of them")


async def ran(bundle: Bundle, launch: Launch) -> tuple[Line, ...]:
    """Report rows the scripted Blender wrote, or the error naming its crash log or output files when it wrote no report, or the deadline it ran past; a cancelled or deadlined wait terminates it."""
    report = anyio.Path(launch.report)
    log, err = report.with_suffix(".log"), report.with_suffix(".err")
    await report.parent.mkdir(parents=True, exist_ok=True)
    for path in (report, log, err):
        await path.unlink(missing_ok=True)
    call = t"start({literal(launch)})"
    arguments = ("--no-window-focus", "--online-mode", "--enable-event-simulate", "--python-exit-code", "1", "--python-expr", bootstrap("interface.blender.script", call))

    def scripted() -> list[psutil.Process]:
        return [process for process in running(bundle) if any(str(report) in argument for argument in process.info["cmdline"])]

    try:
        with anyio.move_on_after(DEADLINE) as waited:
            await anyio.run_process(["/usr/bin/open", "-n", "-g", "-W", "-a", str(bundle.path), "--stdout", str(log), "--stderr", str(err), "--args", *arguments], env=LAUNCH_ENVIRONMENT)
    except anyio.get_cancelled_exc_class():
        with anyio.CancelScope(shield=True):
            await terminated(scripted())
        raise
    if waited.cancelled_caught:
        return (Error(f"the scripted Blender ran past the {DEADLINE:.0f} s deadline, its output in {log} and {err}"), *map(Error, await terminated(scripted())))
    try:
        return parse(await report.read_text(encoding="utf-8"))
    except FileNotFoundError:
        match re.search(r"^Writing: (?P<crash>.+\.crash\.txt)$", await log.read_text(encoding="utf-8"), re.MULTILINE):
            case re.Match() as written:
                return (Error(f"the scripted Blender crashed before writing {report}, its crash log at {written['crash']}"),)
            case None:
                return (Error(f"the scripted Blender exited without writing {report}, its output in {log} and {err}"),)


# --- [SESSION]
async def session(host: Host, bundle: Bundle, manifest: Manifest, essentials: frozenset[str], packages: tuple[Local | Listed, ...]) -> tuple[Line, ...]:
    """Report rows of the interface run in a scripted Blender and the stored shelves edited once it quit, the windowed Blender released, quit, and reopened on its titled file, a companion bundle not installed a skip row."""
    companions = ("com.microsoft.VSCode", "org.inkscape.Inkscape")
    code, inkscapes = await anyio.gather(*(located(identifier) for identifier in companions))
    port = int(host.environ["BLENDER_MCP_PORT"])
    launch = Launch(
        report=str(host.artifacts / f"{host.app}.tsv"),
        renders=str(host.artifacts / "renders"),
        units=host.units.name,
        extension=manifest.id,
        port=port,
        packages=packages,
        editor=next((str(each.path / "Contents" / "Resources" / "app" / "bin" / "code") for each in code), None),
        inkscape=next((str(each.executable) for each in inkscapes), None),
    )
    absent = tuple(Skip(identifier) for identifier, bundles in zip(companions, (code, inkscapes), strict=True) if not bundles)
    discovered = windowed(bundle)
    if isinstance(files := await closed(port, discovered), Error):
        return (files,)
    if errors := await quitted(bundle, discovered):
        return tuple(map(Error, errors))
    async with reopened(bundle, discovered, *files, arguments=("--no-window-focus",)):
        reported = await ran(bundle, launch)
        match outcome(host.app, reported):
            case Applied(folder=folder):
                return (*reported, *absent, *await anyio.to_thread.run_sync(stores.edit, folder / "userpref.blend", manifest, essentials))
            case Failed():
                return reported


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Done", "Raised", "execute", "session"]
