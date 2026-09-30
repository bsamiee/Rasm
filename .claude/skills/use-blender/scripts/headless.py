# ty: ignore[unresolved-attribute, invalid-assignment, redundant-condition-strict, too-many-positional-arguments]
# mypy: disable-error-code="untyped-decorator, union-attr, attr-defined, call-arg, func-returns-value"
# ruff: file-ignore[subprocess-without-shell-equals-true, start-process-with-partial-path, private-member-access, import-private-name]
"""Blender processes outside the live session on a named file, as the command line on the host and as the job inside each Blender it starts."""

from collections.abc import Mapping, Sequence
import contextlib
from enum import auto, StrEnum
import filecmp
from functools import cache
import hashlib
import importlib
import io
from math import prod
import os
from pathlib import Path
import re
import shutil
import signal
import struct
import subprocess
import sys
import time
from types import ModuleType
from typing import Annotated, Final

import attrs
from cattrs.preconf.json import make_converter

if "bpy" in sys.modules:
    from _blendfile_header import BlendFileHeader, BlockHeader
    import addon_utils
    import bpy
else:
    import anyio
    from anyio.abc import SocketAttribute
    import bridge
    import cyclopts
    from cyclopts.types import ResolvedExistingFile, ResolvedFile
    import msgspec
    import psutil
    from results import artifacts, JSON, repository
    from wrapper import wrap

# --- [TYPES] ----------------------------------------------------------------------------


class Scope(StrEnum):
    """Frames a render covers when `--frames` names no frame or range."""

    CURRENT = auto()
    ALL = auto()


class Job(StrEnum):
    """Work a started Blender performs, named first after `--` on its command line."""

    RUN = auto()
    SERVE = auto()
    RENDER = auto()


class Sync(StrEnum):
    """State of the session's data against the file at `stop`."""

    UNCHANGED = auto()
    SAVED = auto()
    DIVERGED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

BRIDGE: Final = "bl_ext.blender_lab.mcp"

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Ran:
    """Code that ran to its end, with `result`, captured output, and wall seconds."""

    result: dict[str, object]
    stdout: str
    stderr: str
    seconds: float


@attrs.frozen
class Session:
    """Running session, `created` the process start time that tells its pid apart from a reused one."""

    port: int
    pid: int
    created: float
    file: Path
    scripts_blocked: str
    tempdir: Path
    log: Path
    seconds: float


@attrs.frozen
class Stopped:
    """Session ended with its record removed on the file it held, `saved` true when `stop` wrote the session's changes to it, its output in the log."""

    file: Path
    saved: bool
    log: Path


@attrs.frozen
class Rendered:
    """Requested frames under `output`, a frame pattern or one movie, with `resumed` counting image frames an earlier run of the same file wrote, `sheet` a JPEG within Read's limits of up to 12 frames labeled by number, and `devices` the Cycles devices that rendered."""

    output: str
    frames: int
    resumed: int
    sheet: Path
    engine: str
    devices: tuple[str, ...]
    camera: str | None
    scripts_blocked: str
    seconds: float


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class Raised:
    """Code that raised, `message` holding the traceback with the code's lines numbered as sent."""

    message: str
    stdout: str
    stderr: str
    seconds: float


@attrs.frozen
class Lost:
    """Session process that ended during the call, its output in the session log and `crash` the report the log names when Blender crashed."""

    session: Session
    crash: Path | None = attrs.field(default=attrs.Factory(lambda lost: crashed(lost.session.log), takes_self=True))


@attrs.frozen
class Failed:
    """Blender exit code of a process that ended without an answer, its output in the log and `crash` the report the log names when Blender crashed."""

    exit_code: int
    log: Path
    crash: Path | None = attrs.field(default=attrs.Factory(lambda failed: crashed(failed.log), takes_self=True))


@attrs.frozen
class SessionRunning:
    """Start refused while a session of the name runs, `stop` ending it first."""

    session: Session


@attrs.frozen
class NoBridge:
    """Start refused when the user's extensions hold no MCP extension, `module` the first package Blender failed to import."""

    module: str


@attrs.frozen
class Diverged:
    """Stop refused with the session running while both its data and the file on disk changed since the session last loaded or saved the file."""

    session: Session


@attrs.frozen
class Alive:
    """Session process running past the deadline after its termination."""

    session: Session


@attrs.frozen
class NoSession:
    """Call or stop with no running session behind the record."""

    record: Path


# --- [OPERATIONS] -----------------------------------------------------------------------


def record(name: str) -> Path:
    """Record of the named session that `start` writes and later commands read."""
    return artifacts("session") / f"{name}.json"


def running(name: str) -> Session | None:
    """Named session the record holds while the process it started runs."""
    try:
        session = msgspec.json.decode(record(name).read_bytes(), type=Session, dec_hook=built)
        alive = psutil.Process(session.pid).create_time() == session.created
    except (FileNotFoundError, psutil.NoSuchProcess):
        return None
    return session if alive else None


def crashed(log: Path) -> Path | None:
    """Crash report Blender names in the log as it dies, `None` for a process that exited without crashing."""
    return Path(written[1]) if (written := re.search(r"^Writing: (.+\.crash\.txt)$", log.read_text(encoding="utf-8", errors="replace"), re.MULTILINE)) else None


def built(kind: type, value: object) -> object:
    """Value of the kind built from its JSON value, the hook msgspec calls for each path a record holds."""
    return kind(value)


def answered(reply: "bridge.Done | bridge.Raised", began: float) -> Ran | Raised:
    """Case of one execute reply to a request begun at the monotonic time."""
    seconds = round(time.monotonic() - began, 3)
    match reply:
        case bridge.Done(result=result, stdout=stdout, stderr=stderr):
            return Ran(result, stdout, stderr, seconds)
        case bridge.Raised(message=message, stdout=stdout, stderr=stderr):
            return Raised(message, stdout, stderr, seconds)


def span(text: str) -> Scope | tuple[int, int]:
    """`--frames` text as one frame, a range as `1..24`, or a scope, `ValueError` for any other text."""
    match text.partition(".."):
        case (Scope.CURRENT | Scope.ALL) as scope, "", "":
            return Scope(scope)
        case frame, "", "":
            return ((number := int(frame)), number)
        case first, _, last if (start := int(first)) <= (end := int(last)):
            return start, end
        case _:
            raise ValueError(f"Expected a frame, a range as 1..24, current, or all, got {text!r}")


def opening(file: Path) -> tuple[str, ...]:
    """File arguments of a job, `-Y` turning off scripts and drivers of a file outside the repository, a missing file first saved from the startup file the process loads."""
    trust, path = () if file.is_relative_to(repository()) else ("-Y",), str(file)
    return (*trust, path) if file.exists() else (*trust, "--python-expr", f"import bpy; bpy.ops.wm.read_homefile(); bpy.ops.wm.save_as_mainfile(filepath={path!r})")


def spawn(job: Job, arguments: tuple[str, ...], options: tuple[str, ...], log: Path, env: Mapping[str, str]) -> tuple[subprocess.Popen[bytes], bytes]:
    """Blender that `BLENDER_PATH` in the environment names, started with the options on the job and its arguments after `--`, its output in the log, and the answer it writes to its pipe, empty when it ends without one."""
    read, write = os.pipe()
    command = (env["BLENDER_PATH"], "--background", "--python-exit-code", "1", *options, "--python", __file__, "--", job, str(write), *arguments)
    with log.open("wb") as sink:
        process = subprocess.Popen(command, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, pass_fds=(write,), start_new_session=job is Job.SERVE, env=env)
    os.close(write)
    with os.fdopen(read, "rb") as pipe:
        return process, pipe.read()


async def exchanged(name: str, session: Session, code: str, *, strict_json: bool) -> Ran | Raised | Lost:
    """Case of the named session's reply to the code, `Lost` with the record removed when the process ends before it answers."""
    began = time.monotonic()
    try:
        reply = await bridge.execute(session.port, code, strict_json=strict_json)
    except anyio.IncompleteRead:
        record(name).unlink()
        return Lost(session)
    return answered(reply, began)


def run(file: "ResolvedFile") -> "Ran | Raised | Failed":
    """Run the code on stdin through the MCP extension's execute in a fresh factory process on the file with the user's extension wheels importable, `BLENDER_USER_EXTENSIONS`, `BLENDER_USER_CONFIG`, and `BLENDER_USER_SCRIPTS` under `.artifacts/blender/` in place of the user's folders."""
    code, began, log = sys.stdin.read(), time.monotonic(), artifacts() / f"run-{os.getpid()}.log"
    redirected = {"BLENDER_USER_EXTENSIONS": str(artifacts("extensions")), "BLENDER_USER_CONFIG": str(artifacts("config")), "BLENDER_USER_SCRIPTS": str(artifacts("scripts"))}
    process, reply = spawn(Job.RUN, (wrap(code),), ("--factory-startup", *opening(file)), log, os.environ | redirected)
    exit_code = process.wait()
    if not reply:
        return Failed(exit_code, log)
    log.unlink()
    return answered(msgspec.json.decode(reply, type=bridge.Done | bridge.Raised), began)


async def start(file: "ResolvedFile", name: str) -> Session | SessionRunning | NoBridge | Failed:
    """Serve the MCP extension's execute protocol from a background Blender on the file under a copy of the user's preferences as the named session, answering once it listens."""
    if (session := running(name)) is not None:
        return SessionRunning(session)
    async with await anyio.create_tcp_listener(local_host=bridge.LOOPBACK, local_port=0) as listener:
        port = listener.extra(SocketAttribute.local_port)
    began, log = time.monotonic(), record(name).with_suffix(".log")
    process, reply = spawn(Job.SERVE, (bridge.LOOPBACK, str(port)), opening(file), log, os.environ)
    if not reply:
        return Failed(process.wait(), log)
    if isinstance(listening := msgspec.json.decode(reply, type=tuple[Path, str] | str, dec_hook=built), str):
        return NoBridge(listening)
    tempdir, blocked = listening
    session = Session(port, process.pid, psutil.Process(process.pid).create_time(), file, blocked, tempdir, log, round(time.monotonic() - began, 2))
    record(name).write_bytes(msgspec.json.encode(session, enc_hook=os.fspath))
    return session


async def call(name: str) -> Ran | Raised | Lost | NoSession:
    """Run the code on stdin in the named session, `bpy.data` kept from earlier calls."""
    if (session := running(name)) is None:
        return NoSession(record(name))
    return await exchanged(name, session, wrap(sys.stdin.read()), strict_json=False)


async def stop(name: str) -> Stopped | Diverged | Raised | Alive | Lost | NoSession:
    """Save the session's changes to its titled file once any running call returns, then end the session, `Raised` with the session running when the save raises."""
    if (session := running(name)) is None:
        return NoSession(record(name))
    match await exchanged(name, session, f"import __main__, bpy\nresult = {{'sync': __main__.{synced.__name__}(), 'file': bpy.data.filepath}}\n", strict_json=True):
        case Raised() | Lost() as ended:
            return ended
        case Ran(result={"sync": Sync.DIVERGED}):
            return Diverged(session)
        case Ran(result=result):
            try:
                (process := psutil.Process(session.pid)).terminate()
            except psutil.NoSuchProcess:
                alive: list[psutil.Process] = []
            else:
                _, alive = psutil.wait_procs((process,), timeout=300)
            if alive:
                return Alive(session)
            record(name).unlink()
            return Stopped(Path(str(result["file"])), result["sync"] == Sync.SAVED, session.log)


def render(file: "ResolvedExistingFile", *, frames: "Annotated[str, cyclopts.Parameter(validator=lambda _type, text: span(text))]" = Scope.CURRENT) -> Rendered | Failed:
    """Render the current frame, one frame, a range as `1..24`, or `all` of the scene range under the user's preferences into `.artifacts/blender/renders/<stem>/`, resuming frames an earlier run of the unchanged file wrote and clearing the folder for a changed one."""
    with file.open("rb") as handle:
        current = hashlib.file_digest(handle, "sha256").hexdigest()
    out = artifacts("renders", file.stem)
    log, stamp = out / f"{file.stem}.log", out / f"{current}.sha256"
    if not stamp.exists():
        shutil.rmtree(out)
        out.mkdir()
        stamp.touch()
    process, reply = spawn(Job.RENDER, (str(out / f"{file.stem}_####"), frames), opening(file), log, os.environ)
    exit_code = process.wait()
    return msgspec.json.decode(reply, type=Rendered, dec_hook=built) if reply else Failed(exit_code, log)


# --- [JOBS] -----------------------------------------------------------------------------


def answer(pipe: int, value: object) -> None:
    """Write the value as JSON to the host's pipe and close it to end the host's read."""
    with os.fdopen(pipe, "w", encoding="utf-8") as channel:
        channel.write(ANSWERS.dumps(value, default=str))


def reference() -> Path:
    """Copy of the session's data as the file last loaded or saved them, the state `stop` compares against, written after the process's first save since add-on `save_pre` handlers change the pointers that save stores."""
    return Path(bpy.app.tempdir) / "reference.blend"


def copied(path: Path) -> Path:
    """Path holding a copy of the session's data, the session's file and modified flag unchanged, its `Info` report kept out of the call's stdout."""
    with contextlib.redirect_stdout(io.StringIO()):
        bpy.ops.wm.save_as_mainfile(filepath=str(path), copy=True)
    return path


def normalized(path: Path) -> tuple[tuple[bytes, int, int, bytes], ...]:
    """Blocks of a `.blend` copy as code, struct index, count, and body, each pointer read as the ordinal of the block it names and zero for a runtime address, each char array blank past its terminator."""
    blocks: list[tuple[BlockHeader, bytes]] = []
    with path.open("rb") as file:
        shape = BlendFileHeader(file).create_block_header_struct()
        while (header := BlockHeader(file, shape)).code != b"ENDB":
            blocks.append((header, file.read(header.size)))
    dna, ordinals = next(body for header, body in blocks if header.code == b"DNA1"), {header.addr_old: index for index, (header, _) in enumerate(blocks, 1)}

    def strings(start: int) -> tuple[list[str], int]:
        (total,) = struct.unpack_from("<i", dna, start)
        items = dna[start + 4 :].split(b"\0", total)[:total]
        return [item.decode() for item in items], (start + 4 + sum(map(len, items)) + total + 3) & ~3

    names, cursor = strings(8)
    types, cursor = strings(cursor + 4)
    sizes, cursor = struct.unpack_from(f"<{len(types)}H", dna, cursor + 4), (cursor + 4 + 2 * len(types) + 3) & ~3
    fields: list[tuple[int, tuple[tuple[int, int], ...]]] = []
    cursor += 8
    for _ in range(struct.unpack_from("<i", dna, cursor - 4)[0]):
        kind, width = struct.unpack_from("<hh", dna, cursor)
        fields.append((kind, tuple(struct.iter_unpack("<hh", dna[cursor + 4 : cursor + 4 + 4 * width]))))
        cursor += 4 + 4 * width
    indexes = {types[kind]: index for index, (kind, _) in enumerate(fields)}

    @cache
    def flat(index: int) -> tuple[int, tuple[int, ...], tuple[tuple[int, int], ...]]:
        offset, pointers, chars = 0, list[int](), list[tuple[int, int]]()
        for kind, name in ((kind, names[number]) for kind, number in fields[index][1]):
            dims = tuple(int(part.rstrip("]")) for part in name.split("[")[1:])
            items = prod(dims)
            match name[0], types[kind], indexes.get(types[kind]):
                case ("*" | "(", _, _):
                    pointers.extend(range(offset, offset + 8 * items, 8))
                    offset += 8 * items
                case (_, "char", _) if dims:
                    chars.extend((at, dims[-1]) for at in range(offset, offset + items, dims[-1]))
                    offset += items
                case (_, _, int() as inner):
                    size, inner_pointers, inner_chars = flat(inner)
                    pointers.extend(start + at for start in range(offset, offset + size * items, size) for at in inner_pointers)
                    chars.extend((start + at, width) for start in range(offset, offset + size * items, size) for at, width in inner_chars)
                    offset += size * items
                case _:
                    offset += sizes[kind] * items
        return offset, tuple(pointers), tuple(chars)

    def body(header: BlockHeader, raw: bytes) -> bytes:
        size, pointers, chars = flat(header.sdna_index)
        data = bytearray(raw)
        for base in range(0, len(data) - size + 1, size) if size else ():
            for at in pointers:
                struct.pack_into("<Q", data, base + at, ordinals.get(struct.unpack_from("<Q", data, base + at)[0], 0))
            for at, width in chars:
                if (end := data.find(0, base + at, base + at + width)) >= 0:
                    data[end : base + at + width] = bytes(base + at + width - end)
        return bytes(data)

    return tuple((header.code, header.sdna_index, header.count, body(header, raw)) for header, raw in blocks)


def synced() -> Sync:
    """Session data saved to the titled file when they differ from the reference, `DIVERGED` when the file on disk changed after the reference was written."""
    if not bpy.data.filepath or filecmp.cmp(current := copied(Path(bpy.app.tempdir) / "current.blend"), reference(), shallow=False) or normalized(current) == normalized(reference()):
        return Sync.UNCHANGED
    if Path(bpy.data.filepath).stat().st_mtime_ns > reference().stat().st_mtime_ns:
        return Sync.DIVERGED
    bpy.ops.wm.save_mainfile()
    return Sync.SAVED


def serve(pipe: int, server: ModuleType, host: str, port: int) -> None:
    """Answer with `bpy.app.tempdir` and the first blocked script once the MCP extension's server listens on the port, then serve its execute requests until SIGTERM closes it."""
    signal.signal(signal.SIGTERM, lambda _signal, _frame: server.stop())
    os.environ["BLENDER_USER_CONFIG"] = str(shutil.copytree(bpy.utils.user_resource("CONFIG"), Path(bpy.app.tempdir, "config")))

    @bpy.app.handlers.persistent
    def loaded(_path: str) -> None:
        reference().unlink(missing_ok=True)

    @bpy.app.handlers.persistent
    def saved(path: str) -> None:
        if path == bpy.data.filepath:
            copied(reference())

    bpy.app.handlers.load_post.append(loaded)
    bpy.app.handlers.save_post.append(saved)
    copied(copied(reference()))
    server.start(host, port)
    answer(pipe, (bpy.app.tempdir, bpy.app.autoexec_fail_message))
    while server.is_running():
        server.poll_blocking()
        if not reference().exists():
            copied(reference())


def rendered(output: Path, frames: Scope | tuple[int, int]) -> Rendered:
    """Frames of the scene rendered to the output pattern without overwriting earlier ones, a scene-linear frame with its display-encoded JPEG beside it, then a sheet of up to 12 evenly spaced frames beside the pattern."""
    scene, began = bpy.context.scene, time.monotonic()
    settings, cycles = scene.render, bpy.context.preferences.addons["cycles"].preferences
    image, sheet = settings.image_settings, output.with_name(f"{Path(bpy.data.filepath).stem}.jpg")
    settings.filepath, settings.use_overwrite, settings.use_placeholder, image.use_preview = str(output), False, False, image.has_linear_colorspace
    match frames:
        case (first, last):
            scene.frame_start, scene.frame_end = first, last
        case Scope.CURRENT:
            scene.frame_start = scene.frame_end = scene.frame_current
        case Scope.ALL:
            pass
    wanted, movie = range(scene.frame_start, scene.frame_end + 1, scene.frame_step), image.media_type == "VIDEO"
    paths = tuple(dict.fromkeys(Path(settings.frame_path(frame=number)) for number in wanted))
    resumed = 0 if movie else sum(path.exists() for path in paths)
    bpy.ops.render.render(animation=True)
    count = min(12, len(wanted))
    picks = sorted({round(index * (len(wanted) - 1) / max(count - 1, 1)) for index in range(count)})
    columns = next(side for side in range(1, count + 1) if side * side >= count)
    if movie:
        selected = "+".join(f"eq(n,{pick})" for pick in picks)
        subprocess.run(("ffmpeg", "-v", "error", "-i", str(paths[0]), "-vf", f"select='{selected}'", "-fps_mode", "vfr", str(Path(bpy.app.tempdir, "sheet_%02d.png"))), check=True)
    sources = (Path(bpy.app.tempdir, f"sheet_{index:02d}.png") if movie else paths[pick].with_suffix(".jpg") if image.use_preview else paths[pick] for index, pick in enumerate(picks, 1))
    labeled = (item for pick, source in zip(picks, sources, strict=True) for item in ("-label", str(wanted[pick]), str(source)))
    tiles = subprocess.run(("magick", "montage", *labeled, "-resize", f"{2000 // columns - 4}x>", "-tile", f"{columns}x", "-geometry", "+2+2", "miff:-"), capture_output=True, check=True).stdout
    subprocess.run(("magick", "-", "-resize", "2000x2000>", "-define", "jpeg:extent=500KB", str(sheet)), input=tiles, check=True)
    gpus = tuple(device.name for device in cycles.get_devices_for_type(cycles.compute_device_type) if device.use) if scene.cycles.device == "GPU" else ()
    devices = (gpus or tuple(device.name for device in cycles.get_devices_for_type("CPU"))) if settings.engine == "CYCLES" else ()
    return Rendered(
        str(paths[0]) if movie else f"{output}{settings.file_extension}",
        len(wanted),
        resumed,
        sheet,
        settings.engine,
        devices,
        scene.camera.name if scene.camera else None,
        bpy.app.autoexec_fail_message,
        round(time.monotonic() - began, 2),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

ANSWERS: Final = make_converter()


def perform(job: Job, pipe: int, arguments: Sequence[str]) -> None:
    """Perform the job inside Blender on its arguments with the Cycles devices of the preferences listed and each printed line in the log before a crash, answering on the pipe."""
    sys.stdout.reconfigure(line_buffering=True)
    bpy.context.preferences.addons["cycles"].preferences.refresh_devices()
    match job:
        case Job.RUN:
            (code,), extensions = arguments, Path(bpy.utils.resource_path("USER"), "extensions")
            addon_utils._initialize_extensions_site_packages(extensions_directory=str(extensions))
            package = sys.modules[BRIDGE] = ModuleType(BRIDGE)
            package.__path__ = [str(extensions.joinpath(*BRIDGE.split(".")[1:]))]
            answer(pipe, importlib.import_module(f"{BRIDGE}.mcp_to_blender_server")._execute_code(code, strict_json=False).response)
        case Job.SERVE:
            host, port = arguments
            try:
                server = importlib.import_module(f"{BRIDGE}.mcp_to_blender_server")
            except ModuleNotFoundError as missing:
                answer(pipe, missing.name)
            else:
                serve(pipe, server, host, int(port))
        case Job.RENDER:
            output, frames = arguments
            answer(pipe, rendered(Path(output), span(frames)))


def report(outcome: Ran | Session | Stopped | Rendered | Raised | Lost | Failed | SessionRunning | NoBridge | Diverged | Alive | NoSession | None) -> int:
    """Print the outcome as JSON with its case under `kind` and return the exit code, 0 for a success or help and 1 otherwise."""
    if outcome is not None:
        sys.stdout.write(f"{JSON.dumps(outcome, indent=1, default=os.fspath)}\n")
    return 0 if isinstance(outcome, Ran | Session | Stopped | Rendered | None) else 1


def main() -> None:
    """Answer the command line with one JSON outcome and its exit code."""
    app = cyclopts.App(help=__doc__, result_action=(report, "sys_exit"))
    for command in (run, start, call, stop, render):
        app.command(command)
    app()


if __name__ == "__main__" and "bpy" in sys.modules:
    job, pipe, *arguments = sys.argv[sys.argv.index("--") + 1 :]
    perform(Job(job), int(pipe), arguments)
elif __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ANSWERS",
    "BRIDGE",
    "Alive",
    "Diverged",
    "Failed",
    "Job",
    "Lost",
    "NoBridge",
    "NoSession",
    "Raised",
    "Ran",
    "Rendered",
    "Scope",
    "Session",
    "SessionRunning",
    "Stopped",
    "Sync",
    "answer",
    "answered",
    "built",
    "call",
    "copied",
    "crashed",
    "exchanged",
    "main",
    "opening",
    "perform",
    "record",
    "reference",
    "render",
    "rendered",
    "report",
    "run",
    "running",
    "serve",
    "spawn",
    "span",
    "start",
    "stop",
    "synced",
]
