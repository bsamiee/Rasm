# ast-grep-ignore: no-json-codec, no-socket-module, no-stdlib-record
# mypy: disable-error-code="arg-type"
# ruff: file-ignore[subprocess-without-shell-equals-true]
# /// script
# requires-python = ">=3.13"
# dependencies = ["cyclopts", "msgspec", "psutil"]
# [tool.uv]
# prerelease = "allow"
# ///
"""Blender outside the live session on a named file, this file running both the command line on the host and the job inside each Blender it starts."""

from collections.abc import Mapping, Sequence
import contextlib
from dataclasses import asdict, dataclass
from enum import auto, StrEnum
import hashlib
import importlib
import io
import json
import os
from pathlib import Path
import re
import runpy
import shutil
import signal
import socket
import subprocess
import sys
import time
import traceback
from typing import Annotated, Any, cast, Self

if "bpy" in sys.modules:
    import bpy
    import numpy as np
else:
    import cyclopts
    from cyclopts.types import ResolvedExistingFile
    import msgspec
    import psutil

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Ran | Session | Stopped | Rendered | Raised | Lost | Incomplete | Failed | SessionRunning | NoSession
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
    """How a stopped session ended: it quit on its own, or it stayed busy past the deadline and was killed."""

    QUIT = auto()
    KILLED = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

LOOPBACK = "localhost"

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Ran:
    """Code that ran to its end, with `result`, captured output, and wall seconds."""

    where: str
    result: dict[str, object]
    stdout: str
    stderr: str
    seconds: float


@dataclass(frozen=True, slots=True)
class Session:
    """Running session: its port, process and that process's start time, the file it opened, the first script or driver Blender blocked, its temporary folder, and its log."""

    port: int
    pid: int
    created: float
    file: Path
    scripts_blocked: str
    tempdir: Path
    log: Path
    seconds: float


@dataclass(frozen=True, slots=True)
class Stopped:
    """Session the stop ended, quit with its temporary folder removed by Blender or killed with it removed here."""

    session: Session
    ending: Ending


@dataclass(frozen=True, slots=True)
class Frame:
    """Frame number, the path Blender names for it, whether it is on disk, and its pixel mean and deviation, `None` for a video or a missing file."""

    frame: int
    path: Path
    written: bool
    mean: float | None
    deviation: float | None


@dataclass(frozen=True, slots=True)
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


@dataclass(frozen=True, slots=True)
class Raised:
    """Code that raised, `message` holding the traceback with `<agent>` lines numbered as sent."""

    where: str
    message: str
    stdout: str
    stderr: str
    seconds: float


@dataclass(frozen=True, slots=True)
class Lost:
    """Session process that ended during the call, a crash or a kill, with Blender's error lines from its log."""

    session: Session
    errors: tuple[str, ...]


@dataclass(frozen=True, slots=True)
class Incomplete:
    """Render that finished with frames it did not write, or wrote flat within one 8-bit level and removed so a rerun renders them again."""

    missing: tuple[int, ...]
    blank: tuple[int, ...]
    frames: tuple[Frame, ...]
    log: Path


@dataclass(frozen=True, slots=True)
class Failed:
    """Blender exit code with its error lines and the exception line of this file's traceback, the whole output in the log."""

    exit_code: int
    errors: tuple[str, ...]
    log: Path


@dataclass(frozen=True, slots=True)
class SessionRunning:
    """Start refused while a session runs, `stop` ends it first."""

    session: Session


@dataclass(frozen=True, slots=True)
class NoSession:
    """Call or stop with no running session behind the record."""

    record: Path


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Host:
    """Blender binary from the `blender` row of `.mcp.json`, the repository root, and the artifact directory every command writes under."""

    blender: Path
    root: Path
    directory: Path

    @classmethod
    def locate(cls) -> Self:
        """Host of the repository holding this file, its nearest `.git` ancestor."""
        root = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists())
        blender = msgspec.json.decode((root / ".mcp.json").read_bytes())["mcpServers"]["blender"]["env"]["BLENDER_PATH"]
        directory = root / ".artifacts" / "blender"
        directory.mkdir(parents=True, exist_ok=True)
        return cls(Path(blender), root, directory)

    @property
    def record(self) -> Path:
        """Session record `start` writes and every later command reads, in a folder of its own so no snapshot or capture name reaches it."""
        return self.directory / "session" / "record.json"

    def running(self) -> Session | None:
        """Session the record names while the process it started lives."""
        if not self.record.is_file():
            return None
        session = decode(self.record.read_bytes(), Session)
        try:
            alive = psutil.Process(session.pid).create_time() == session.created
        except psutil.NoSuchProcess:
            return None
        return session if alive else None

    def opening(self, file: Path) -> tuple[str, ...]:
        """File argument of a session or render, `-Y` before a file from outside the repository so its scripts and drivers stay off."""
        return (str(file),) if file.is_relative_to(self.root) else ("-Y", str(file))

    def spawn(self, job: Job, payload: object, arguments: tuple[str, ...], log: Path, env: Mapping[str, str]) -> tuple[subprocess.Popen[bytes], bytes]:
        """Blender started on the job with its output in the log, and the answer it writes to its pipe, empty when it ends without one."""
        read, write = os.pipe()
        command = (str(self.blender), "--background", *arguments, "--python-exit-code", "1", "--python", __file__, "--", job, str(write), msgspec.json.encode(payload, enc_hook=os.fspath).decode())
        with log.open("wb") as sink:
            process = subprocess.Popen(command, stdin=subprocess.DEVNULL, stdout=sink, stderr=subprocess.STDOUT, pass_fds=(write,), start_new_session=job is Job.SERVE, env=env)
        os.close(write)
        with os.fdopen(read, "rb") as pipe:
            return process, pipe.read()

    def configured(self, name: str) -> dict[str, str]:
        """Environment whose config directory is a fresh copy of the user's under `<name>-config`, so preference and add-on writes leave the user's folder untouched."""
        process, reply = self.spawn(Job.CONFIG, None, ("--factory-startup",), self.directory / "config.log", os.environ)
        process.wait()
        if (config := self.directory / f"{name}-config").exists():
            shutil.rmtree(config)
        shutil.copytree(decode(reply, Path), config)
        return os.environ | {"BLENDER_USER_CONFIG": str(config)}

    def run(self, file: "ResolvedExistingFile") -> Ran | Raised | Failed:
        """Run the code on stdin in a fresh factory process on the file, embedded scripts off whatever the file's origin."""
        began, source, log = time.monotonic(), self.directory / "run.py", self.directory / "run.log"
        source.write_text(wrapped(sys.stdin.read()), encoding="utf-8")
        process, reply = self.spawn(Job.RUN, source, ("--factory-startup", str(file)), log, os.environ)
        exit_code = process.wait()
        return answered(str(file), decode(reply, dict[str, Any]), time.monotonic() - began) if reply else Failed(exit_code, errors(log), log)

    def start(self, file: "ResolvedExistingFile") -> Session | SessionRunning | Failed:
        """Serve the add-on's execute protocol from a background Blender on the file under the user's preferences, answering once it listens."""
        if (session := self.running()) is not None:
            return SessionRunning(session)
        with socket.socket() as probe:
            probe.bind((LOOPBACK, 0))
            port = probe.getsockname()[1]
        began, log = time.monotonic(), self.directory / "session.log"
        process, reply = self.spawn(Job.SERVE, port, self.opening(file), log, self.configured("session"))
        if not reply:
            return Failed(process.wait(), errors(log), log)
        tempdir, blocked = decode(reply, tuple[Path, str])
        session = Session(port, process.pid, psutil.Process(process.pid).create_time(), file, blocked, tempdir, log, round(time.monotonic() - began, 2))
        self.record.parent.mkdir(exist_ok=True)
        self.record.write_bytes(msgspec.json.encode(session, enc_hook=os.fspath))
        return session

    def call(self) -> Ran | Raised | Lost | NoSession:
        """Run the code on stdin in the session, `bpy.data` kept from earlier calls."""
        if (session := self.running()) is None:
            return NoSession(self.record)
        began = time.monotonic()
        with socket.create_connection((LOOPBACK, session.port)) as connection, connection.makefile("rb") as stream:
            connection.sendall(msgspec.json.encode({"type": "execute", "code": wrapped(sys.stdin.read()), "strict_json": False}) + b"\0")
            reply = stream.read().partition(b"\0")[0]
        if not reply:
            self.record.unlink(missing_ok=True)
            shutil.rmtree(session.tempdir, ignore_errors=True)
            return Lost(session, errors(session.log))
        return answered(f"session on port {session.port}", decode(reply, dict[str, Any]), time.monotonic() - began)

    def stop(self) -> Stopped | NoSession:
        """End the session with SIGTERM so Blender quits and removes its temporary folder, killing a session still busy five seconds later."""
        if (session := self.running()) is None:
            return NoSession(self.record)
        self.record.unlink()
        process = psutil.Process(session.pid)
        process.terminate()
        try:
            process.wait(timeout=5)
        except psutil.TimeoutExpired:
            process.kill()
            process.wait()
            shutil.rmtree(session.tempdir, ignore_errors=True)
            return Stopped(session, Ending.KILLED)
        return Stopped(session, Ending.QUIT)

    def render(self, file: "ResolvedExistingFile", *, frames: "Annotated[Frames, cyclopts.Parameter(converter=span, n_tokens=1)]" = Scope.CURRENT) -> Rendered | Incomplete | Failed:
        """Render the current frame, one frame, a range as `1..24`, or `all` of the scene range under the user's preferences, resuming frames an earlier run of the unchanged file wrote."""
        out, log = self.directory / file.stem, self.directory / f"{file.stem}.log"
        stamp = out / "blend.sha256"
        with file.open("rb") as blend:
            digest = hashlib.file_digest(blend, "sha256").hexdigest()
        if out.exists() and not (stamp.is_file() and stamp.read_text(encoding="utf-8") == digest):
            shutil.rmtree(out)
        out.mkdir(parents=True, exist_ok=True)
        stamp.write_text(digest, encoding="utf-8")
        process, reply = self.spawn(Job.RENDER, (out / f"{file.stem}_####", frames, log), self.opening(file), log, self.configured("render"))
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
    """Case of one add-on execute response, which leaves out an empty stdout or stderr."""
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


def span(_hint: object, tokens: "Sequence[cyclopts.Token]") -> Frames:
    """`--frames` value as one frame, a range as `1..24`, or a scope, `ValueError` for any other text."""
    match tokens[0].value.partition(".."):
        case (Scope.CURRENT | Scope.ALL) as scope, "", "":
            return Scope(scope)
        case frame, "", "":
            return ((number := int(frame)), number)
        case first, "..", last if int(first) <= int(last):
            return (int(first), int(last))
        case _:
            raise ValueError(f"{tokens[0].value!r} is not a frame, a range as 1..24, current, or all")


# --- [BLENDER] --------------------------------------------------------------------------


def answer(pipe: int, value: object) -> None:
    """Write the value as JSON to the pipe the host reads, closing it so the host's read ends."""
    with os.fdopen(pipe, "w", encoding="utf-8") as channel:
        json.dump(value, channel, default=str)


def devices() -> None:
    """Refresh the Cycles device list so renders use the compute device the user's preferences select."""
    match cast("bpy.types.Preferences", bpy.context.preferences).addons["cycles"].preferences:
        case object(refresh_devices=refresh):
            refresh()


def execute(pipe: int, source: Path) -> None:
    """Answer with the add-on's response to the code file, its `result` dict or its traceback, re-raising the traceback's exception after the answer."""
    out, err = io.StringIO(), io.StringIO()

    def respond(**fields: object) -> None:
        answer(pipe, {**fields, "stdout": out.getvalue(), "stderr": err.getvalue()})

    try:
        with contextlib.redirect_stdout(out), contextlib.redirect_stderr(err):
            namespace = runpy.run_path(str(source), init_globals={"result": {}})
    except Exception:
        respond(status=Status.ERROR, message=traceback.format_exc())
        raise
    match namespace["result"]:
        case dict() as result:
            respond(status=Status.OK, result=result)
        case other:
            respond(status=Status.ERROR, message=f"The `result` variable must be a dict, not {type(other).__name__}")


def serve(pipe: int, port: int) -> None:
    """Answer with the temporary folder and the first blocked script once the add-on listens on the port, then serve its execute requests until SIGTERM."""
    signal.signal(signal.SIGTERM, signal.default_int_handler)
    devices()
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
    began, scene = time.monotonic(), cast("bpy.types.Scene", bpy.context.scene)
    settings = scene.render
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


def perform(job: Job, pipe: int, payload: object) -> None:
    """Perform the job inside Blender, answering on the pipe."""
    match job, payload:
        case Job.CONFIG, None:
            answer(pipe, bpy.utils.user_resource("CONFIG"))
        case Job.RUN, str() as source:
            execute(pipe, Path(source))
        case Job.SERVE, int() as port:
            serve(pipe, port)
        case Job.RENDER, [str() as output, [int() as first, int() as last], str() as log]:
            answer(pipe, asdict(rendered(Path(output), (first, last), Path(log))))
        case Job.RENDER, [str() as output, str() as scope, str() as log]:
            answer(pipe, asdict(rendered(Path(output), Scope(scope), Path(log))))
        case unknown:
            raise ValueError(unknown)


def report(outcome: Outcome | None) -> int:
    """Print the outcome as JSON with its case under `kind`, answering the exit code: 0 for a success or help, 1 for every other case."""
    match outcome:
        case None:
            return 0
        case _:
            sys.stdout.buffer.write(msgspec.json.format(msgspec.json.encode({"kind": type(outcome).__name__, **msgspec.to_builtins(outcome, enc_hook=os.fspath)}), indent=1) + b"\n")
            return 0 if isinstance(outcome, Ran | Session | Stopped | Rendered) else 1


def main() -> None:
    """Answer the command line with one JSON outcome and its exit code."""
    host = Host.locate()
    app = cyclopts.App(help=__doc__, result_action=(report, "sys_exit"))
    for command in (host.run, host.start, host.call, host.stop, host.render):
        app.command(command)
    app()


if __name__ == "__main__":
    match sys.argv:
        case [*_, "--", job, pipe, payload] if "bpy" in sys.modules:
            perform(Job(job), int(pipe), json.loads(payload))
        case _:
            main()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
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
    "errors",
    "execute",
    "main",
    "perform",
    "rendered",
    "report",
    "serve",
    "span",
    "wrapped",
]
