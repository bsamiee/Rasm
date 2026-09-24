# ty: ignore[unresolved-import]
# mypy: disable-error-code="arg-type, import-not-found, untyped-decorator"
# ruff: file-ignore[subprocess-without-shell-equals-true, django-extra, exec-builtin]
# /// script
# requires-python = ">=3.13"
# dependencies = ["anyio", "attrs", "cyclopts", "msgspec", "psutil"]
# [tool.uv]
# prerelease = "allow"
# ///
"""Blender outside the live session on a named file, this file running both the command line on the host and the job inside each Blender it starts."""

from collections.abc import Mapping, Sequence
import contextlib
from enum import auto, StrEnum
import hashlib
import importlib
import io
import os
from pathlib import Path
import re
import runpy
import shutil
import signal
import subprocess
import sys
import time
import traceback
from typing import Annotated, Any, Self

import attrs

if "bpy" in sys.modules:
    import bpy
    from cattrs.preconf.json import make_converter
    from cycles.properties import CyclesPreferences
    import numpy as np
else:
    import anyio
    from anyio.abc import SocketAttribute
    import cyclopts
    from cyclopts.types import ResolvedExistingFile
    import msgspec
    import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Ran | Session | Stopped | Rendered | Raised | Lost | Incomplete | Failed | SessionRunning | Diverged | NoSession
type Frames = Scope | tuple[int, int]


class Scope(StrEnum):
    """Frames a render covers when `--frames` names no frame or range."""

    CURRENT = auto()
    ALL = auto()


class Job(StrEnum):
    """Work each Blender this file starts performs, named after `--` on its command line."""

    CONFIG = auto()
    RUN = auto()
    SERVE = auto()
    RENDER = auto()


class Status(StrEnum):
    """Status of one response in the add-on's execute protocol."""

    OK = auto()
    ERROR = auto()


class Ending(StrEnum):
    """End of a stopped session, a quit on SIGTERM or a kill past the deadline."""

    QUIT = auto()
    KILLED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

LOOPBACK = "localhost"
DEADLINE = 5
IDLE = "result = {}\n"
SAVE = "import __main__\nresult = __main__.saved()\n"
RANGE = ".."

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Ran:
    """Code that ran to its end, with `result`, captured output, and wall seconds."""

    where: str
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
    """Session `stop` ended after saving the titled file when the session changed data, its temporary folder removed by Blender on a quit or here on a kill."""

    session: Session
    ending: Ending


@attrs.frozen
class Frame:
    """Rendered frame at the path Blender names for it, `mean` and `deviation` `None` for a video or a missing file."""

    frame: int
    path: Path
    written: bool
    mean: float | None
    deviation: float | None


@attrs.frozen
class Rendered:
    """Every requested frame on disk with pixels that vary, `resumed` counting image frames an earlier run of the same file wrote."""

    frames: tuple[Frame, ...]
    engine: str
    camera: str | None
    scripts_blocked: str
    resumed: int
    seconds: float
    log: Path


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class Raised:
    """Code that raised, `message` holding the traceback with `<agent>` lines numbered as sent."""

    where: str
    message: str
    stdout: str
    stderr: str
    seconds: float


@attrs.frozen
class Lost:
    """Session process that ended during the call, a crash or a kill, with Blender's error lines from its log."""

    session: Session
    errors: tuple[str, ...]


@attrs.frozen
class Incomplete:
    """Render that finished with missing frames or frames flat within one 8-bit level, the flat ones removed for the next run."""

    missing: tuple[int, ...]
    blank: tuple[int, ...]
    frames: tuple[Frame, ...]
    log: Path


@attrs.frozen
class Failed:
    """Blender exit code with its error lines and the exception line of this file's traceback, the whole output in the log."""

    exit_code: int
    errors: tuple[str, ...]
    log: Path


@attrs.frozen
class SessionRunning:
    """Start refused while a session of that name runs, `stop` ends it first."""

    session: Session


@attrs.frozen
class Diverged:
    """Stop refused with the session running while both its data and the file on disk changed since the session last read or wrote the file."""

    session: Session


@attrs.frozen
class NoSession:
    """Call or stop with no running session behind the record."""

    record: Path


# --- [SERVICES] -------------------------------------------------------------------------


@attrs.frozen
class Host:
    """Blender binary, repository root, and the artifact directory every command writes under."""

    blender: Path
    root: Path
    directory: Path

    @classmethod
    def locate(cls) -> Self:
        """Host of the nearest `.git` ancestor of this file, the binary from the `blender` row of `.mcp.json`."""
        root = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists())
        blender = msgspec.json.decode((root / ".mcp.json").read_bytes())["mcpServers"]["blender"]["env"]["BLENDER_PATH"]
        directory = root / ".artifacts" / "blender"
        directory.mkdir(parents=True, exist_ok=True)
        return cls(Path(blender), root, directory)

    def record(self, name: str) -> Path:
        """Record of the named session that `start` writes and later commands read, in its own folder apart from snapshot and capture names."""
        return self.directory / "session" / f"{name}.json"

    def running(self, name: str) -> Session | None:
        """Named session the record holds while the process it started lives."""
        if not (record := self.record(name)).is_file():
            return None
        session = decode(record.read_bytes(), Session)
        try:
            alive = psutil.Process(session.pid).create_time() == session.created
        except psutil.NoSuchProcess:
            return None
        return session if alive else None

    def opening(self, file: Path) -> tuple[str, ...]:
        """File argument of a session or render, with `-Y` turning off scripts and drivers of a file from outside the repository."""
        return (str(file),) if file.is_relative_to(self.root) else ("-Y", str(file))

    def spawn(self, job: Job, payload: tuple[str, ...], arguments: tuple[str, ...], log: Path, env: Mapping[str, str]) -> tuple[subprocess.Popen[bytes], bytes]:
        """Blender started on the job with its payload after `--` and its output in the log, and the answer it writes to its pipe, empty when it ends without one."""
        read, write = os.pipe()
        command = (str(self.blender), "--background", *arguments, "--python-exit-code", "1", "--python", __file__, "--", job, str(write), *payload)
        with log.open("wb") as sink:
            process = subprocess.Popen(command, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, pass_fds=(write,), start_new_session=job is Job.SERVE, env=env)
        os.close(write)
        with os.fdopen(read, "rb") as pipe:
            return process, pipe.read()

    def configured(self, name: str) -> dict[str, str]:
        """Environment pointing Blender at a fresh copy of the user's config directory under `<name>-config` for preference and add-on writes."""
        process, reply = self.spawn(Job.CONFIG, (), ("--factory-startup",), self.directory / f"{name}-config.log", os.environ)
        process.wait()
        if (config := self.directory / f"{name}-config").exists():
            shutil.rmtree(config)
        shutil.copytree(decode(reply, Path), config)
        return os.environ | {"BLENDER_USER_CONFIG": str(config)}

    def run(self, file: "ResolvedExistingFile") -> Ran | Raised | Failed:
        """Run the code on stdin in a fresh factory process on the file, embedded scripts off whatever the file's origin, the log kept when Blender ends without an answer."""
        began, log = time.monotonic(), self.directory / f"run-{os.getpid()}.log"
        process, reply = self.spawn(Job.RUN, (wrapped(sys.stdin.read()),), ("--factory-startup", str(file)), log, os.environ)
        exit_code = process.wait()
        if not reply:
            return Failed(exit_code, errors(log), log)
        log.unlink()
        return answered(str(file), decode(reply, dict[str, Any]), time.monotonic() - began)

    async def start(self, file: "ResolvedExistingFile", name: str) -> Session | SessionRunning | Failed:
        """Serve the add-on's execute protocol from a background Blender on the file under the user's preferences as the named session, answering once it listens."""
        if (session := self.running(name)) is not None:
            return SessionRunning(session)
        async with await anyio.create_tcp_listener(local_host=LOOPBACK, local_port=0) as probe:
            port = probe.extra(SocketAttribute.local_port)
        record = self.record(name)
        record.parent.mkdir(exist_ok=True)
        began, log = time.monotonic(), record.with_suffix(".log")
        process, reply = self.spawn(Job.SERVE, (str(port),), self.opening(file), log, self.configured(f"session-{name}"))
        if not reply:
            return Failed(process.wait(), errors(log), log)
        tempdir, blocked = decode(reply, tuple[Path, str])
        session = Session(port, process.pid, psutil.Process(process.pid).create_time(), file, blocked, tempdir, log, round(time.monotonic() - began, 2))
        record.write_bytes(msgspec.json.encode(session, enc_hook=os.fspath))
        return session

    async def call(self, name: str) -> Ran | Raised | Lost | NoSession:
        """Run the code on stdin in the named session, `bpy.data` kept from earlier calls."""
        if (session := self.running(name)) is None:
            return NoSession(self.record(name))
        began = time.monotonic()
        if not (reply := await exchange(session.port, wrapped(sys.stdin.read()))):
            self.record(name).unlink(missing_ok=True)
            shutil.rmtree(session.tempdir, ignore_errors=True)
            return Lost(session, errors(session.log))
        return answered(f"session on port {session.port}", decode(reply, dict[str, Any]), time.monotonic() - began)

    async def stop(self, name: str) -> Stopped | Diverged | Raised | NoSession:
        """Save the titled file when the named session changed data, then end the session through SIGTERM or a kill past the deadline, `Raised` with the session running when the save raises."""
        if (session := self.running(name)) is None:
            return NoSession(self.record(name))
        began, idle = time.monotonic(), b""
        with anyio.move_on_after(DEADLINE):
            idle = await exchange(session.port, IDLE)
        if idle and (reply := await exchange(session.port, SAVE)):
            match answered(f"session on port {session.port}", decode(reply, dict[str, Any]), time.monotonic() - began):
                case Raised() as raised:
                    return raised
                case Ran(result={"diverged": True}):
                    return Diverged(session)
                case Ran():
                    pass
        self.record(name).unlink()
        process = psutil.Process(session.pid)
        process.terminate()
        try:
            process.wait(timeout=DEADLINE)
        except psutil.TimeoutExpired:
            process.kill()
            process.wait()
            shutil.rmtree(session.tempdir, ignore_errors=True)
            return Stopped(session, Ending.KILLED)
        return Stopped(session, Ending.QUIT)

    def render(
        self, file: "ResolvedExistingFile", *, frames: "Annotated[Frames, cyclopts.Parameter(converter=lambda _hint, tokens: span(tokens[0].value), n_tokens=1)]" = Scope.CURRENT
    ) -> Rendered | Incomplete | Failed:
        """Render the current frame, one frame, a range as `1..24`, or `all` of the scene range under the user's preferences, resuming frames an earlier run of the unchanged file wrote."""
        out, log = self.directory / file.stem, self.directory / f"{file.stem}.log"
        stamp, current = out / "blend.sha256", digest(file)
        if out.exists() and not (stamp.is_file() and stamp.read_text(encoding="utf-8") == current):
            shutil.rmtree(out)
        out.mkdir(parents=True, exist_ok=True)
        stamp.write_text(current, encoding="utf-8")
        scope = frames if isinstance(frames, Scope) else RANGE.join(map(str, frames))
        process, reply = self.spawn(Job.RENDER, (str(out / f"{file.stem}_####"), scope, str(log)), self.opening(file), log, self.configured("render"))
        if process.wait() or not reply:
            return Failed(process.returncode, errors(log), log)
        result = decode(reply, Rendered)
        missing = tuple(shot.frame for shot in result.frames if not shot.written)
        blank = tuple(shot for shot in result.frames if shot.deviation is not None and shot.deviation < 1 / 255)
        for shot in blank:
            shot.path.unlink()
        return Incomplete(missing, tuple(shot.frame for shot in blank), result.frames, log) if missing or blank else result


# --- [OPERATIONS] -----------------------------------------------------------------------


def decode[T](data: bytes, kind: type[T]) -> T:
    """Value of the kind from JSON, each path built from its string."""
    return msgspec.json.decode(data, type=kind, dec_hook=lambda hint, value: hint(value))


def wrapped(code: str) -> str:
    """Agent code inside the hook's wrapper, code the wrapper declines running as sent."""
    return runpy.run_path(str(Path(__file__).with_name("wrapper.py")))["wrap"](code) or code


def answered(where: str, response: dict[str, object], seconds: float) -> Ran | Raised:
    """Case of one add-on execute response, with stdout or stderr absent when empty."""
    stdout, stderr = str(response.get("stdout", "")), str(response.get("stderr", ""))
    match response:
        case {"status": Status.OK, "result": dict() as result}:
            return Ran(where, result, stdout, stderr, round(seconds, 3))
        case _:
            return Raised(where, str(response.get("message", "")), stdout, stderr, round(seconds, 3))


def errors(log: Path) -> tuple[str, ...]:
    """Blender's report errors, argument errors, and crash report in the log, then the exception line of each traceback through this file."""
    text = log.read_text(encoding="utf-8", errors="replace")
    reports = re.findall(r"^.*\| ERROR .*$|^Error: .*$|^Writing: .*crash\.txt$", text, re.MULTILINE)
    raised = re.findall(rf'^Traceback \(most recent call last\):\n(?:[ \t].*\n)*?[ \t]+File "{re.escape(__file__)}".*\n(?:[ \t].*\n)*(\S.*)$', text, re.MULTILINE)
    return tuple(line.strip() for line in (*reports, *raised))


def digest(path: Path) -> str:
    """SHA-256 hex digest of the file."""
    with path.open("rb") as handle:
        return hashlib.file_digest(handle, "sha256").hexdigest()


def span(text: str) -> Frames:
    """`--frames` text as one frame, a range as `1..24`, or a scope, `ValueError` for any other text."""
    match text.partition(RANGE):
        case (Scope.CURRENT | Scope.ALL) as scope, "", "":
            return Scope(scope)
        case frame, "", "":
            return ((number := int(frame)), number)
        case first, _, last if int(first) <= int(last):
            return (int(first), int(last))
        case _:
            raise ValueError(f"{text!r} is not a frame, a range as 1..24, current, or all")


async def exchange(port: int, code: str) -> bytes:
    """Reply of the session's execute protocol to the code, empty when the session ends before it answers."""
    async with await anyio.connect_tcp(LOOPBACK, port) as stream:
        await stream.send(msgspec.json.encode({"type": "execute", "code": code, "strict_json": False}) + b"\0")
        return b"".join([chunk async for chunk in stream]).partition(b"\0")[0]


# --- [BLENDER] --------------------------------------------------------------------------


def answer(pipe: int, value: object) -> None:
    """Write the value as JSON to the host's pipe and close it to end the host's read."""
    with os.fdopen(pipe, "w", encoding="utf-8") as channel:
        channel.write(make_converter().dumps(value, default=str))


def devices() -> None:
    """Refresh the Cycles device list for renders on the compute device the user's preferences select."""
    if (preferences := bpy.context.preferences) is None:
        raise RuntimeError("context.preferences is None")
    match preferences.addons["cycles"].preferences:
        case CyclesPreferences() as cycles:
            cycles.refresh_devices()
        case unregistered:
            raise RuntimeError(f"Cycles preferences are {unregistered!r}, the add-on failed to register")


def execute(pipe: int, code: str) -> None:
    """Answer with the add-on's response to the code, its `result` dict or its traceback, re-raising the traceback's exception after the answer."""
    out, err = io.StringIO(), io.StringIO()
    namespace: dict[str, object] = {}

    def respond(**fields: object) -> None:
        answer(pipe, {**fields, "stdout": out.getvalue(), "stderr": err.getvalue()})

    try:
        with contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
            exec(compile(code, "<run>", "exec"), namespace)
    except Exception:
        respond(status=Status.ERROR, message=traceback.format_exc())
        raise
    match namespace.get("result", {}):
        case dict() as result:
            respond(status=Status.OK, result=result)
        case other:
            respond(status=Status.ERROR, message=f"The `result` variable must be a dict, not {type(other).__name__}")


def state() -> Path:
    """Reference copy of the session's data in Blender's temporary folder, replaced by the titled file itself after each load or save of it, its mtime the last sync with disk."""
    return Path(bpy.app.tempdir) / "state.blend"


def saved() -> dict[str, object]:
    """Titled file saved when the session's data differs from the reference copy, `diverged` when the file on disk changed after the copy, nothing saved otherwise."""
    reference, current = state(), Path(bpy.app.tempdir) / "current.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(current), copy=True, relative_remap=False)
    if not bpy.data.filepath or digest(current) == digest(reference):
        return {"saved": []}
    if Path(bpy.data.filepath).stat().st_mtime_ns > reference.stat().st_mtime_ns:
        return {"diverged": True}
    return {"saved": sorted(bpy.ops.wm.save_mainfile())}


def serve(pipe: int, port: int) -> None:
    """Answer with the temporary folder and the first blocked script once the add-on listens on the port, then serve its execute requests until SIGTERM."""
    signal.signal(signal.SIGTERM, signal.default_int_handler)
    devices()

    @bpy.app.handlers.persistent
    def synced(path: str, _: object) -> None:
        if path == bpy.data.filepath:
            shutil.copyfile(path, state())

    bpy.app.handlers.save_post.append(synced)
    bpy.app.handlers.load_post.append(synced)
    bpy.ops.wm.save_as_mainfile(filepath=str(state()), copy=True, relative_remap=False)
    server = next(module for name, module in sys.modules.items() if name.endswith(".mcp_to_blender_server"))
    blocking = importlib.import_module(".execute_blocking", server.__package__)
    server.start(LOOPBACK, port)
    answer(pipe, (bpy.app.tempdir, bpy.app.autoexec_fail_message))
    try:
        blocking.run()
    finally:
        server.stop()


def rendered(output: Path, frames: Frames, log: Path) -> Rendered:
    """Frames of the scene rendered to the output pattern without overwriting earlier ones, each read back from the path Blender names for it."""
    if (scene := bpy.context.scene) is None:
        raise RuntimeError("context.scene is None")
    began, settings = time.monotonic(), scene.render
    settings.filepath, settings.use_overwrite, settings.use_placeholder = str(output), False, False
    match frames:
        case (first, last):
            scene.frame_start, scene.frame_end = first, last
        case Scope.CURRENT:
            scene.frame_start = scene.frame_end = scene.frame_current
        case Scope.ALL:
            pass
    devices()
    wanted = range(scene.frame_start, scene.frame_end + 1, scene.frame_step)
    paths = tuple(Path(settings.frame_path(frame=number)) for number in wanted)
    video = settings.image_settings.media_type == "VIDEO"
    resumed = 0 if video else sum(path.exists() for path in paths)
    bpy.ops.render.render(animation=True)
    length = bpy.data.movieclips.load(str(paths[0])).frame_duration if video and paths[0].exists() else 0

    def measured(index: int, number: int, path: Path) -> Frame:
        if not path.exists():
            return Frame(number, path, written=False, mean=None, deviation=None)
        if video:
            return Frame(number, path, written=index < length, mean=None, deviation=None)
        image = bpy.data.images.load(str(path))
        pixels = np.empty(len(image.pixels), dtype=np.float32)
        image.pixels.foreach_get(pixels)
        rgb = pixels.reshape(-1, image.channels)[:, :3]
        bpy.data.images.remove(image)
        return Frame(number, path, written=True, mean=round(float(rgb.mean()), 4), deviation=round(float(rgb.std()), 4))

    shots = tuple(measured(index, number, path) for index, (number, path) in enumerate(zip(wanted, paths, strict=True)))
    return Rendered(shots, settings.engine, scene.camera.name if scene.camera else None, bpy.app.autoexec_fail_message, resumed, round(time.monotonic() - began, 2), log)


# --- [COMPOSITION] ----------------------------------------------------------------------


def perform(job: Job, pipe: int, payload: Sequence[str]) -> None:
    """Perform the job inside Blender on the payload after `--`, answering on the pipe."""
    match job, payload:
        case Job.CONFIG, []:
            answer(pipe, bpy.utils.user_resource("CONFIG"))
        case Job.RUN, [code]:
            execute(pipe, code)
        case Job.SERVE, [port]:
            serve(pipe, int(port))
        case Job.RENDER, [output, frames, log]:
            answer(pipe, rendered(Path(output), span(frames), Path(log)))
        case unknown:
            raise ValueError(unknown)


def report(outcome: Outcome | None) -> int:
    """Print the outcome as JSON with its case under `kind` and return the exit code, 0 for a success or help and 1 otherwise."""
    match outcome:
        case None:
            return 0
        case _:
            sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode({"kind": type(outcome).__name__, **attrs.asdict(outcome)}, enc_hook=os.fspath), indent=1) + b"\n")
            return 0 if isinstance(outcome, Ran | Session | Stopped | Rendered) else 1


def main() -> None:
    """Answer the command line with one JSON outcome and its exit code."""
    host = Host.locate()
    app = cyclopts.App(help=__doc__, result_action=(report, "sys_exit"))
    for command in (host.run, host.start, host.call, host.stop, host.render):
        app.command(command)
    app()


if __name__ == "__main__" and "bpy" in sys.modules:
    job, pipe, *payload = sys.argv[sys.argv.index("--") + 1 :]
    perform(Job(job), int(pipe), payload)
elif __name__ == "__main__":
    main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Diverged",
    "Ending",
    "Failed",
    "Frame",
    "Frames",
    "Host",
    "Incomplete",
    "Job",
    "Lost",
    "NoSession",
    "Outcome",
    "Raised",
    "Ran",
    "Rendered",
    "Scope",
    "Session",
    "SessionRunning",
    "Status",
    "Stopped",
    "answer",
    "answered",
    "decode",
    "devices",
    "digest",
    "errors",
    "exchange",
    "execute",
    "main",
    "perform",
    "rendered",
    "report",
    "saved",
    "serve",
    "span",
    "state",
    "wrapped",
]
