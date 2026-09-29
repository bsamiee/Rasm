# ty: ignore[unresolved-attribute, invalid-assignment]
# mypy: disable-error-code="untyped-decorator, union-attr, attr-defined"
# ruff: file-ignore[subprocess-without-shell-equals-true, django-extra, exec-builtin, blind-except, private-member-access]
"""Blender processes outside the live session on a named file, as the command line on the host and as the job inside each Blender it starts."""

from collections.abc import Mapping, Sequence
import contextlib
from enum import auto, StrEnum
import filecmp
import hashlib
import importlib
import io
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import time
import traceback
from types import ModuleType
from typing import Annotated

import attrs

if "bpy" in sys.modules:
    import addon_utils
    import bpy
    from cattrs.preconf.json import make_converter
else:
    import anyio
    from anyio.abc import SocketAttribute
    import cyclopts
    from cyclopts.types import ResolvedExistingFile, ResolvedFile
    import msgspec
    import psutil
    from results import artifacts, as_result, repository
    from wrapper import wrap

    from interface.blender.session import Done, execute, Raised as BridgeRaised
    from interface.host import LOOPBACK, terminated

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


class Status(StrEnum):
    """Status of one response in the MCP extension's execute protocol."""

    OK = auto()
    ERROR = auto()


class Sync(StrEnum):
    """State of the session's data against the file at `stop`."""

    UNCHANGED = auto()
    SAVED = auto()
    DIVERGED = auto()


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
    """Session ended with its record removed, `saved` true when `stop` wrote the session's changes to the titled file."""

    session: Session
    saved: bool


@attrs.frozen
class Rendered:
    """Files Blender names for the requested frames, `resumed` counting image frames an earlier run of the same file wrote."""

    files: tuple[Path, ...]
    engine: str
    camera: str | None
    scripts_blocked: str
    resumed: int
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
    """Session process that ended during the call, its output in the session log."""

    session: Session


@attrs.frozen
class Failed:
    """Blender exit code of a process that ended without an answer, its output in the log."""

    exit_code: int
    log: Path


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


def built(kind: type, value: object) -> object:
    """Value of the kind built from its JSON value, the hook msgspec calls for each path a record holds."""
    return kind(value)


def answered(reply: "Done | BridgeRaised", began: float) -> Ran | Raised:
    """Case of one execute reply to a request begun at the monotonic time."""
    seconds = round(time.monotonic() - began, 3)
    match reply:
        case Done(result=result, stdout=stdout, stderr=stderr):
            return Ran(msgspec.json.decode(result, type=dict[str, object]), stdout, stderr, seconds)
        case BridgeRaised(message=message, stdout=stdout, stderr=stderr):
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
    """File arguments of a job, `-Y` turning off scripts and drivers of a file outside the repository, a missing file first saved from the user's startup file."""
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
        reply = await execute(session.port, code, strict_json=strict_json)
    except anyio.IncompleteRead:
        record(name).unlink()
        return Lost(session)
    return answered(reply, began)


def run(file: "ResolvedFile") -> "Ran | Raised | Failed":
    """Run the code on stdin in a fresh factory process on the file with the user's extension wheels importable, `BLENDER_USER_EXTENSIONS` under `.artifacts/blender/` leaving the user's folder unchanged."""
    code, began, log = sys.stdin.read(), time.monotonic(), artifacts() / f"run-{os.getpid()}.log"
    process, reply = spawn(Job.RUN, (wrap(code),), ("--factory-startup", *opening(file)), log, os.environ | {"BLENDER_USER_EXTENSIONS": str(artifacts("extensions"))})
    exit_code = process.wait()
    if not reply:
        return Failed(exit_code, log)
    log.unlink()
    return answered(msgspec.json.decode(reply, type=Done | BridgeRaised), began)


async def start(file: "ResolvedFile", name: str) -> Session | SessionRunning | NoBridge | Failed:
    """Serve the MCP extension's execute protocol from a background Blender on the file under a copy of the user's preferences as the named session, answering once it listens."""
    if (session := running(name)) is not None:
        return SessionRunning(session)
    async with await anyio.create_tcp_listener(local_host=LOOPBACK, local_port=0) as listener:
        port = listener.extra(SocketAttribute.local_port)
    began, log = time.monotonic(), record(name).with_suffix(".log")
    process, reply = spawn(Job.SERVE, (LOOPBACK, str(port)), opening(file), log, os.environ)
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
    match await exchanged(name, session, f"import __main__\nresult = {{'sync': __main__.{synced.__name__}()}}\n", strict_json=True):
        case Raised() | Lost() as ended:
            return ended
        case Ran(result={"sync": Sync.DIVERGED}):
            return Diverged(session)
        case Ran(result=result):
            if await terminated([psutil.Process(session.pid)]):
                return Alive(session)
            record(name).unlink()
            return Stopped(session, result["sync"] == Sync.SAVED)


def render(file: "ResolvedExistingFile", *, frames: "Annotated[str, cyclopts.Parameter(validator=lambda _type, text: span(text))]" = Scope.CURRENT) -> Rendered | Failed:
    """Render the current frame, one frame, a range as `1..24`, or `all` of the scene range under the user's preferences, resuming frames an earlier run of the unchanged file wrote."""
    with file.open("rb") as handle:
        current = hashlib.file_digest(handle, "sha256").hexdigest()
    out = artifacts("render", file.stem)
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
        channel.write(make_converter().dumps(value, default=str))


def respond(pipe: int, code: str) -> None:
    """Answer with the execute protocol's response to the code, its `result` dict or its traceback."""
    out, err = io.StringIO(), io.StringIO()
    namespace: dict[str, object] = {"result": {}}
    try:
        with contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
            exec(code, namespace)
    except Exception:
        response: dict[str, object] = {"status": Status.ERROR, "message": traceback.format_exc()}
    else:
        match namespace["result"]:
            case dict() as result:
                response = {"status": Status.OK, "result": result}
            case other:
                response = {"status": Status.ERROR, "message": f"The `result` variable must be a dict, not {type(other).__name__}"}
    answer(pipe, {**response, "stdout": out.getvalue(), "stderr": err.getvalue()})


def reference() -> Path:
    """Copy of the session's data as the file last loaded or saved them, the state `stop` compares against, written after the process's first save since add-on `save_pre` handlers change the pointers that save stores."""
    return Path(bpy.app.tempdir) / "reference.blend"


def copied(path: Path) -> Path:
    """Path holding a copy of the session's data, the session's file and modified flag unchanged, its `Info` report kept out of the call's stdout."""
    with contextlib.redirect_stdout(io.StringIO()):
        bpy.ops.wm.save_as_mainfile(filepath=str(path), copy=True)
    return path


def synced() -> Sync:
    """Session data saved to the titled file when they differ from the reference, `DIVERGED` when the file on disk changed after the reference was written."""
    if not bpy.data.filepath or filecmp.cmp(copied(Path(bpy.app.tempdir) / "current.blend"), reference(), shallow=False):
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
    """Frames of the scene rendered to the output pattern without overwriting earlier ones, at the paths Blender names for them."""
    scene = bpy.context.scene
    began, settings = time.monotonic(), scene.render
    settings.filepath, settings.use_overwrite, settings.use_placeholder = str(output), False, False
    match frames:
        case (first, last):
            scene.frame_start, scene.frame_end = first, last
        case Scope.CURRENT:
            scene.frame_start = scene.frame_end = scene.frame_current
        case Scope.ALL:
            pass
    wanted = range(scene.frame_start, scene.frame_end + 1, scene.frame_step)
    paths = tuple(dict.fromkeys(Path(settings.frame_path(frame=number)) for number in wanted))
    resumed = 0 if settings.image_settings.media_type == "VIDEO" else sum(path.exists() for path in paths)
    bpy.ops.render.render(animation=True)
    return Rendered(paths, settings.engine, scene.camera.name if scene.camera else None, bpy.app.autoexec_fail_message, resumed, round(time.monotonic() - began, 2))


# --- [COMPOSITION] ----------------------------------------------------------------------


def perform(job: Job, pipe: int, arguments: Sequence[str]) -> None:
    """Perform the job inside Blender on its arguments with the Cycles devices of the preferences listed, answering on the pipe."""
    bpy.context.preferences.addons["cycles"].preferences.refresh_devices()
    match job:
        case Job.RUN:
            addon_utils._initialize_extensions_site_packages(extensions_directory=str(Path(bpy.utils.resource_path("USER"), "extensions")))
            respond(pipe, *arguments)
        case Job.SERVE:
            host, port = arguments
            try:
                server = importlib.import_module("bl_ext.blender_lab.mcp.mcp_to_blender_server")
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
        sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode(as_result(outcome), enc_hook=os.fspath), indent=1) + b"\n")
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
    "Status",
    "Stopped",
    "Sync",
    "answer",
    "answered",
    "built",
    "call",
    "copied",
    "exchanged",
    "main",
    "opening",
    "perform",
    "record",
    "reference",
    "render",
    "rendered",
    "report",
    "respond",
    "run",
    "running",
    "serve",
    "spawn",
    "span",
    "start",
    "stop",
    "synced",
]
