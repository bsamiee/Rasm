"""Blender's scripted session: the running Blender closed and quit, the interface run in a windowed session, its report decoded, and the asset shelves written once it quit."""

from pathlib import Path
import re
from typing import Final

import anyio
from anyio.streams.buffered import BufferedByteReceiveStream
import msgspec
import psutil

from interface.blender import userpref
from interface.blender.catalog import Catalog, Listed, Local
from interface.blender.packages import Manifest, packaged
from interface.host import Application, Applied, bootstrap, DEADLINE, Failed, Host, LAUNCH_ENVIRONMENT, LOOPBACK, Outcome, parse, quit_application, terminated
from interface.report import Kind, line

# --- [CONSTANTS] ------------------------------------------------------------------------

STDERR: Final = re.compile(rf"^{re.escape('Traceback (most recent call last):')}\n(?P<frames>(?:[ \t].*\n)+)(?P<raised>.+)$|^.*registration error.*$", re.MULTILINE)

# --- [MODELS] ---------------------------------------------------------------------------


class Opened(msgspec.Struct, frozen=True):
    """File the running Blender holds after the close step, empty when untitled."""

    filepath: str


class Done(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    """Bridge answer to the close request."""

    result: Opened


class Raised(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """Bridge answer to an execute request that raised, with its traceback."""

    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [PROCESSES]
def windowed(executable: Path) -> list[psutil.Process]:
    """Every Blender of the executable a person runs, none in background, command, or event simulation mode."""
    return [
        process
        for process in psutil.process_iter(["exe", "cmdline"])
        if process.info["exe"] == str(executable) and {"-b", "--background", "-c", "--command", "--enable-event-simulate"}.isdisjoint(process.info["cmdline"])
    ]


async def bundle(identifier: str) -> str | None:
    """First application bundle Spotlight finds by bundle id, None when none is installed."""
    found = (await anyio.run_process(["/usr/bin/mdfind", f"kMDItemCFBundleIdentifier == '{identifier}'"])).stdout.decode().splitlines()
    return found[0] if found else None


def reported(text: str) -> tuple[str, ...]:
    """Innermost frame and exception of each traceback and each registration error line in the session's error output."""
    return tuple(
        f"{[frame.strip() for frame in match['frames'].splitlines() if frame.lstrip().startswith('File ')][-1]}: {match['raised']}" if match["frames"] else match[0].strip()
        for match in STDERR.finditer(text)
    )


async def close(port: int, processes: list[psutil.Process]) -> Opened | tuple[str, ...]:
    """File the one running Blender holds after its bridge saves a titled file with unsaved edits and discards an untitled one, or why it cannot."""
    match processes:
        case [process] if any(connection.laddr.port == port and connection.status == psutil.CONN_LISTEN for connection in process.net_connections("tcp")):
            async with await anyio.connect_tcp(LOOPBACK, port) as stream:
                await stream.send(
                    msgspec.json.encode({
                        "type": "execute",
                        "code": "import bpy\nif bpy.data.is_dirty:\n    (bpy.ops.wm.save_mainfile if bpy.data.filepath else bpy.ops.wm.read_homefile)()\nresult = {'filepath': bpy.data.filepath}",
                        "strict_json": True,
                    })
                    + b"\0"
                )
                reply = msgspec.json.decode(await BufferedByteReceiveStream(stream).receive_until(b"\0", 1 << 24), type=Done | Raised)
            return reply.result if isinstance(reply, Done) else tuple(reply.message.splitlines())
        case [_]:
            return (f"no running Blender listens on the bridge port {port}",)
        case _:
            return (f"{len(processes)} Blender instances run and the bridge reaches one",)


async def ended(executable: Path, report: anyio.Path) -> tuple[str, ...]:
    """Errors of terminating the scripted session writing the report, naming each process alive past the deadline."""
    return await terminated([
        process for process in psutil.process_iter(["exe", "cmdline"]) if process.info["exe"] == str(executable) and any(repr(str(report)) in argument for argument in process.info["cmdline"])
    ])


# --- [SESSION]
async def session(host: Host, blender: Application, executable: Path, name: str, port: int, rows: tuple[Local | Listed, ...]) -> Outcome:
    """Outcome of the scripted Blender session from its report, partial report, or last traceback, a crash naming its native and Python frames, each application bundle it reads skipped when absent."""
    report, part, log, stderr = (anyio.Path(host.artifacts, f"{host.app}.{suffix}") for suffix in ("tsv", "part", "log", "err"))
    crash = re.compile(r"^Writing: (?P<report>.+\.crash\.txt)$", re.MULTILINE)
    backtrace = re.compile(r"^\d+\s+.+?\s+0x[0-9a-f]+ _sigtramp \+ \d+\n\d+\s+.+?\s+0x[0-9a-f]+ (?P<frame>\S+) \+ \d+$(?s:.*)^# Python backtrace\n(?P<python>(?s:.*))", re.MULTILINE)
    identifiers = ("com.microsoft.VSCode", "org.inkscape.Inkscape")
    editor, inkscape = bundles = [await bundle(identifier) for identifier in identifiers]
    for path in (report, part, log, stderr):
        await path.unlink(missing_ok=True)
    call = f"start({str(report)!r}, {name!r}, {port}, {msgspec.json.encode(rows).decode()!r}, {editor!r}, {inkscape!r})"
    with anyio.move_on_after(DEADLINE) as waited:
        await anyio.run_process(
            [
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
                "--python-exit-code",
                "1",
                "--python-expr",
                bootstrap("interface.blender.script", call),
            ],
            env=LAUNCH_ENVIRONMENT,
        )
    stuck, text, logged = await ended(executable, report) if waited.cancelled_caught else (), await stderr.read_text(encoding="utf-8"), await log.read_text(encoding="utf-8")
    crashed = crash.search(logged)
    match crashed, backtrace.search(await anyio.Path(crashed["report"]).read_text(encoding="utf-8")) if crashed else None:
        case _, re.Match() as trace:
            ending, frames = f"the session crashed in {trace['frame']}", tuple(frame.strip() for frame in trace["python"].splitlines() if frame)
        case re.Match() as written, None:
            ending, frames = f"the session crashed and Blender wrote {written['report']}", ()
        case None, None:
            ending, frames = f"the session ran past the {DEADLINE:.0f} s deadline" if waited.cancelled_caught else "the session exited before its report", ()
    if await report.exists():
        outcome = parse(host.app, await report.read_text(encoding="utf-8"))
    elif await part.exists():
        outcome = parse(host.app, "".join((await part.read_text(encoding="utf-8"), *(f"{line(Kind.ERROR, error)}\n" for error in (ending, *frames)))))
    else:
        last = [match[0].splitlines() for match in STDERR.finditer(text) if match["frames"]][-1:]
        outcome = Failed(host.app, (ending, *frames, *(entry for lines in last for entry in lines)))
    errors, absent = reported(text), tuple(identifier for identifier, found in zip(identifiers, bundles, strict=True) if found is None)
    match outcome, stuck:
        case Applied(), ():
            return msgspec.structs.replace(outcome, skipped=(*outcome.skipped, *absent), stderr=errors)
        case Applied(changes=changes), _:
            return Failed(host.app, stuck, changes, stderr=errors)
        case Failed(errors=failures, changes=changes), _:
            return Failed(host.app, (*failures, *stuck), changes, stderr=errors)


async def staged(host: Host, blender: Application, executable: Path, manifest: Manifest, port: int, rows: tuple[Local | Listed, ...], catalog: Catalog) -> Outcome:
    """Session outcome with the asset shelf catalog tabs written to its stored preferences once it quit."""
    match await session(host, blender, executable, manifest.id, port, rows):
        case Failed() as failed:
            return failed
        case Applied() as ran:
            match await anyio.to_thread.run_sync(userpref.shelved, Path(ran.settings, "userpref.blend"), Path(catalog.essentials), catalog.types, manifest.shelves):
                case userpref.Unreadable() as unreadable:
                    return Failed(host.app, (unreadable.message,), ran.changes, stderr=ran.stderr)
                case shelved:
                    return msgspec.structs.replace(ran, changes=(*ran.changes, *shelved))


async def relaunch(host: Host, executable: Path, port: int, blender: Application, declared: tuple[Local | Listed, ...], catalog: Catalog) -> Outcome:
    """Build the extension, close and quit the running Blender, run the interface, and reopen a closed Blender on its file."""
    manifest, extension = await packaged(host, executable)
    running = windowed(executable)
    match await close(port, running) if running else None:
        case tuple() as errors:
            return Failed(host.app, errors)
        case opened:
            if stuck := await quit_application(blender.path, running):
                return Failed(host.app, stuck)
            try:
                return await staged(host, blender, executable, manifest, port, (*declared, extension), catalog)
            finally:
                with anyio.CancelScope(shield=True):
                    if opened is not None:
                        await anyio.run_process(["/usr/bin/open", "-n", "-g", "-a", str(blender.path), "--args", "--no-window-focus", *filter(None, (opened.filepath,))], env=LAUNCH_ENVIRONMENT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["relaunch"]
