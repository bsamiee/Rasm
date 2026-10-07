"""Blender's session: the windowed Blender released through its bridge and quit, the interface run in a scripted instance, and the stored shelves and region widths edited before the released file reopens."""

import re

import anyio
import msgspec
import psutil

from interface.blender import stores
from interface.blender.packages import Manifest
from interface.blender.rows import Install, Launch, Width
from interface.host import (
    Applied,
    bootstrap,
    Bundle,
    DEADLINE,
    Error,
    executed,
    Failed,
    Host,
    LAUNCH_ENVIRONMENT,
    Line,
    literal,
    located,
    LOOPBACK,
    Measurement,
    outcome,
    parse,
    quitted,
    registered,
    reopened,
    Result,
    running,
    Skip,
    terminated,
)

# --- [MODELS] ---------------------------------------------------------------------------


class Held(msgspec.Struct, frozen=True):
    """File the windowed Blender holds once released, empty when untitled, and whether it holds unsaved edits, the dict the bridge requires as `result`."""

    filepath: str
    is_dirty: bool


class Done(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    """Bridge answer to a request that ran, with its `result` variable held raw for the requested type."""

    result: msgspec.Raw


class Raised(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """Bridge answer to a request that raised, with its traceback."""

    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BRIDGE]
def windowed(bundle: Bundle) -> tuple[psutil.Process, ...]:
    """Every Blender of the bundle a person runs, none in background or command mode."""
    return tuple(process for process in running(bundle) if {"-b", "--background", "-c", "--command"}.isdisjoint(process.info["cmdline"]))


async def bridged[T](port: int, processes: tuple[psutil.Process, ...], code: str, kind: type[T]) -> Result[T] | None:
    """`result` of the kind the code sets in the one windowed Blender through its bridge on the port, none when no windowed Blender runs, or the error of several, the transport, the decode, or the code."""
    if not processes:
        return None
    if len(processes) != 1:
        return Error(f"{len(processes)} windowed Blender instances run and the bridge on port {port} reaches one of them")
    try:
        with anyio.fail_after(DEADLINE, reason=f"Blender bridge on port {port} exceeded {DEADLINE:.0f} s"):
            async with await anyio.connect_tcp(LOOPBACK, port) as stream:
                await stream.send(msgspec.json.encode({"type": "execute", "code": code, "strict_json": True}) + b"\0")
                reply = msgspec.json.decode(b"".join([chunk async for chunk in stream]).removesuffix(b"\0"), type=Done | Raised)
                return Error(f"Blender bridge on port {port} raised {reply.message.rstrip()}") if isinstance(reply, Raised) else msgspec.json.decode(reply.result, type=kind)
    except (OSError, anyio.BrokenResourceError, msgspec.DecodeError) as error:
        return Error(f"Blender bridge on port {port} failed: {type(error).__name__}: {error}")


async def closed(port: int, processes: tuple[psutil.Process, ...]) -> Result[tuple[str, ...]]:
    """Titled file the windowed Blender holds for the reopen once its bridge discards an untitled session, none without a windowed Blender, or why it stays open."""
    release = "import bpy\nif not bpy.data.filepath:\n    bpy.ops.wm.read_homefile()\nresult = {'filepath': bpy.data.filepath, 'is_dirty': bpy.data.is_dirty}"
    match await bridged(port, processes, release, Held):
        case None | Held(filepath=""):
            return ()
        case Held(filepath=path, is_dirty=True):
            return Error(f"Blender stays open with unsaved edits in {path}")
        case Held(filepath=path):
            return (path,)
        case Error() as failed:
            return failed


# --- [PROCESSES]
async def ran(bundle: Bundle, launch: Launch) -> tuple[Line, ...]:
    """Report rows the scripted Blender wrote, or the error naming its output files when it wrote none or ran past the deadline, a cancelled or deadlined wait terminating it."""
    report = anyio.Path(launch.report)
    log, err = report.with_suffix(".log"), report.with_suffix(".err")
    await report.parent.mkdir(parents=True, exist_ok=True)
    for path in (report, log, err):
        await path.unlink(missing_ok=True)
    script = bootstrap("interface.blender.script", t"start({literal(launch)})")
    arguments = ("--no-window-focus", "--online-mode", "--python-exit-code", "1", "--python-expr", script)

    def scripted() -> list[psutil.Process]:
        return [process for process in running(bundle) if script in process.info["cmdline"]]

    try:
        with anyio.fail_after(DEADLINE):
            launched = await executed(("/usr/bin/open", "-n", "-g", "-W", "-a", str(bundle.path), "--stdout", str(log), "--stderr", str(err), "--args", *arguments), LAUNCH_ENVIRONMENT)
    except (TimeoutError, anyio.get_cancelled_exc_class()) as error:
        with anyio.CancelScope(shield=True):
            errors = await terminated(scripted())
            if isinstance(error, TimeoutError):
                return (Error(f"Scripted Blender exceeded {DEADLINE:.0f} s. Output: {log}, {err}"), *errors)
        raise
    if isinstance(launched, Error):
        return (launched,)
    try:
        return parse(await report.read_text(encoding="utf-8"))
    except FileNotFoundError:
        match re.search(r"^Writing: (?P<crash>.+\.crash\.txt)$", await log.read_text(encoding="utf-8"), re.MULTILINE):
            case re.Match() as written:
                return (Error(f"Scripted Blender crashed before writing {report}. Crash log: {written['crash']}"),)
            case None:
                return (Error(f"Scripted Blender exited without writing {report}. Output: {log}, {err}"),)


# --- [SESSION]
async def session(host: Host, port: int, bundle: Bundle, manifest: Manifest, essentials: frozenset[str], packages: tuple[Install, ...]) -> tuple[Line, ...]:
    """Report rows of the interface run in a scripted Blender and the stored shelves and region widths edited once it quit, the windowed Blender on the bridge port released, quit, and reopened on its titled file, a companion bundle not installed a skip row."""
    companions = ("com.microsoft.VSCode", "org.inkscape.Inkscape")
    code, inkscapes = await anyio.gather(*(located(identifier) for identifier in companions))
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
    instances = await registered(discovered)
    async with reopened(bundle, instances, *files, arguments=("--no-window-focus",)):
        with anyio.CancelScope(shield=True):
            if errors := await quitted(instances):
                return errors
        reported = await ran(bundle, launch)
        match outcome(host.app, reported):
            case Applied(folder=folder):
                (record,) = (row.record for row in reported if isinstance(row, Measurement))
                return (*reported, *absent, *await anyio.to_thread.run_sync(stores.edit, folder, manifest, essentials, msgspec.json.decode(record, type=tuple[Width, ...])))
            case Failed():
                return reported


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bridged", "session", "windowed"]
