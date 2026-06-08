"""Execute checks through local, remote, in-process, and leased runners."""

from collections.abc import Callable
import contextlib
from contextvars import ContextVar
from dataclasses import dataclass, replace
import fcntl
from functools import partial
import os
from pathlib import Path
import shlex
import shutil
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import anyio
from anyio import to_thread  # explicit submodule import; ty mis-resolves anyio.to_thread
from expression import Error, Ok, Result
import msgspec
from opentelemetry import propagate, trace
import psutil
import stamina
import structlog
from upath import UPath

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # unconditional for beartype forward refs
from tools.assay.core.aspect import (
    _CHECKED_LAYER,  # noqa: PLC2701  # intentional internal seam; aspect is co-owned by engine
    compose,
    traced,
)
from tools.assay.core.model import Artifact, ArtifactKind, Check, Completed, Fault, Mode, receipt, Runner
from tools.assay.core.routing import place, Routed
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, ObjectReceiveStream, ObjectSendStream, Process
    import asyncssh


# --- [TYPES] ----------------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]
type ByteRecv = Callable[[], Coroutine[None, None, bytes | None]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"
_SSH_CONNECT_TIMEOUT: float = 15.0
_SSH_SIGNAL_STATUS: int = 255
_RETRY_MIN_REMAINING: float = 0.05
_LOG = structlog.get_logger("assay.engine")
_TRACER = trace.get_tracer("assay.engine")

# POSIX-only fcntl members bind once because ty checks all platforms and cannot narrow this module to POSIX.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]
# Fault-time resource snapshots cross the anyio.run boundary through this ContextVar.
_RESOURCE: ContextVar[tuple[tuple[str, float], ...]] = ContextVar("assay_resource", default=())


# --- [MODELS] ---------------------------------------------------------------------------


class _LeaseOwner(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    resource: str
    run_id: str
    pid: int
    # (pid, create_time) survives PID reuse; a recycled pid carries a fresh create_time.
    create_time: float
    cwd: str = ""
    project: str = ""
    mode: str = "exclusive"
    started_at: float = 0.0
    target: str = ""


@dataclass(frozen=True, slots=True)
class _Held:
    owner: _LeaseOwner


@dataclass(frozen=True, slots=True)
class _SshCache:
    conns: dict[str, asyncssh.SSHClientConnection]
    lock: anyio.Lock


@dataclass(frozen=True, slots=True)
class _ExecPlan:
    argv: tuple[str, ...]
    check: Check
    cwd: str
    env: Mapping[str, str]
    settings: AssaySettings
    scope: ArtifactScope | None
    streaming: bool
    tail_cap: int
    chunk: int
    thread_limiter: anyio.CapacityLimiter | None


@dataclass(frozen=True, slots=True)
class Captured:
    """Aggregate of a drained byte stream: tail window, artifact path, and total size/line counts."""

    tail: bytes = b""
    path: str = ""
    size: int = 0
    lines: int = 0


_SSH_CACHE: ContextVar[_SshCache | None] = ContextVar("assay_ssh_cache", default=None)


@runtime_checkable
class _Nullary(Protocol):
    def __call__(self) -> float | int | str: ...


class WriteSink(Protocol):
    """Structural byte sink: a writable target receiving stream chunks during a drain."""

    def write(self, payload: bytes) -> object:
        """Write one byte chunk to the sink."""
        ...


@runtime_checkable
class _WriteContext(Protocol):
    def __enter__(self) -> WriteSink: ...

    def __exit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: object) -> object: ...


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)


# --- [OPERATIONS] -----------------------------------------------------------------------


def splice_command(
    runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str], mode: Mode
) -> tuple[str, ...]:
    """Inject scope flags into a DOTNET build-graph command before any ``--`` argument separator.

    Returns:
        The command with scope flags spliced for scoped DOTNET verbs, otherwise the command verbatim.
    """
    match (runner, command, scope):
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in scoped_verbs and mode not in {Mode.QUERY, Mode.LIST}:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *s.dotnet_flags, *command[cut:])
        case _:
            return command


def _overlay(settings: AssaySettings, scope: ArtifactScope | None) -> Mapping[str, str]:
    base: MutableMapping[str, str] = dict(os.environ)  # noqa: TID251  # spawn boundary clones the host environment
    base.update(settings.python_tool_env)
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
    # Runner prefix, scoped command body, then routed tails keep scope and input axes separate.
    tool = check.tool
    body = splice_command(tool.runner, tool.command, scope, settings.scoped_verbs, tool.mode)
    body = (*(part for group in tool.groups for part in ("--group", group)), *body) if tool.runner is Runner.UV else body
    body = ("--project", str(settings.root), *body) if tool.runner is Runner.UV and tool.stage.project else body
    tails = place(routed, tool, settings=settings)
    return (*tool.runner.prefix, *body, *(part for tail in tails for part in tail))


def argv_for(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
    """Project the exact argv the engine would execute for a check.

    Returns:
        Command argv after runner, scope, stage, and routed inputs are applied.
    """
    return _argv(check, routed, settings=settings, scope=scope)


async def discover_async(argv: tuple[str, ...], *, root: UPath | Path | str, limit_s: float) -> Result[bytes, Fault]:
    """Run an engine-owned read-only discovery command.

    Returns:
        Captured stdout, or a typed discovery fault.
    """
    try:
        with anyio.fail_after(limit_s):
            done = await anyio.run_process(list(argv), cwd=str(root), check=False)
    except TimeoutError:
        return Error(Fault(argv, RailStatus.TIMEOUT, f"timeout after {limit_s:g}s"))
    except OSError as exc:
        return Error(Fault(argv, RailStatus.FAULTED, str(exc)[:1024]))
    match done.returncode:
        case 0:
            return Ok(done.stdout)
        case _:
            tail = (done.stderr or done.stdout or b"").decode(errors="replace").strip()[:1024]
            return Error(Fault(argv, RailStatus.FAULTED, tail))


def discover(argv: tuple[str, ...], *, root: UPath | Path | str, timeout: float) -> Result[bytes, Fault]:
    """Run an engine-owned read-only discovery command from synchronous rails.

    Returns:
        Captured stdout, or a typed discovery fault.
    """

    async def _run() -> Result[bytes, Fault]:
        return await discover_async(argv, root=root, limit_s=timeout)

    return anyio.run(_run)


def _materialize(check: Check, settings: AssaySettings) -> Result[Check, Fault]:
    stage = check.tool.stage
    if not stage.root:
        return Ok(check)
    if settings.exec_target:
        return Error(Fault((check.tool.name, "stage"), status=RailStatus.UNSUPPORTED, message="staged tools require local execution"))
    root = Path(str(settings.root)).resolve()
    work = _contained(root, stage.root)
    if isinstance(work, ValueError):
        return Error(Fault((check.tool.name, "stage"), message=str(work)))
    shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True, exist_ok=True)
    for rel in stage.inputs:
        if (fault := _copy_stage_input(check, root, work, rel)) is not None:
            return Error(fault)
    return Ok(msgspec.structs.replace(check, cwd=work))


def _copy_stage_input(check: Check, root: Path, work: Path, rel: str) -> Fault | None:
    src = _contained(root, rel)
    if isinstance(src, ValueError):
        return Fault((check.tool.name, "stage", rel), message=str(src))
    dst = _contained(work, rel)
    if isinstance(dst, ValueError):  # pragma: no cover  # defensive: src passed identical containment on the same rel; dst cannot escape work
        return Fault((check.tool.name, "stage", rel), message=str(dst))
    if not src.exists():
        return Fault((check.tool.name, "stage", rel), message=f"missing stage input: {rel}")
    if src.is_dir():
        shutil.copytree(src, dst)
    else:
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)
    return None


def _contained(root: Path, rel: str) -> Path | ValueError:
    text = rel.replace("\\", "/")
    parts = text.split("/")
    match text.startswith("/") or text in {"", ".", ".."} or "\x00" in text or any(p in {"", ".", ".."} for p in parts):
        case True:
            return ValueError(f"unsafe stage path: {rel!r}")
        case False:
            target = (root / text).resolve()
            return target if target.is_relative_to(root) else ValueError(f"stage path escaped root: {rel!r}")


async def _drain(stream: ByteReceiveStream | None, *, tail_cap: int, chunk: int) -> bytes:
    captured = await drain_stream(_recv_anyio(stream, chunk), tail_cap=tail_cap)
    return captured.tail


async def drain_stream(recv: ByteRecv, *, tail_cap: int, sink: WriteSink | None = None, path: str = "") -> Captured:
    """Pump an async byte source to EOF, tee-ing to an optional sink and retaining a bounded tail.

    Args:
        recv: Async read primitive returning the next chunk, or ``None`` at EOF.
        tail_cap: Maximum number of trailing bytes retained in the captured tail.
        sink: Optional byte sink receiving every chunk as it is read.
        path: Artifact path recorded on the resulting capture.

    Returns:
        Capture of the tail window, artifact path, total byte size, and line count.
    """
    tail, total, lines, last = b"", 0, 0, b""
    # why: an async byte pump is irreducibly imperative — the read primitive is a side-effecting coroutine driven to EOF.
    while (read := await recv()) is not None:
        _write_sink(sink, read)
        tail = (tail + read)[-tail_cap:]
        total += len(read)
        lines += read.count(b"\n")
        last = read[-1:] or last  # track last CONTENT byte: an empty chunk must not clobber the newline-terminus probe
    return Captured(tail=tail, path=path, size=total, lines=lines + (1 if total and last != b"\n" else 0))


def _recv_anyio(stream: ByteReceiveStream | None, chunk: int) -> ByteRecv:
    async def _recv() -> bytes | None:
        match stream:
            case None:
                return None
            case _:
                try:
                    return await stream.receive(chunk)
                except anyio.EndOfStream, anyio.ClosedResourceError:
                    return None

    return _recv


def _recv_ssh(reader: asyncssh.SSHReader[bytes], chunk: int) -> ByteRecv:
    # asyncssh read returns b"" only at EOF; project the empty terminator to the pump's None sentinel.

    async def _recv() -> bytes | None:
        return (await reader.read(chunk)) or None

    return _recv


def _write_sink(sink: WriteSink | None, payload: bytes) -> None:
    match sink:
        case None:
            pass
        case _:
            sink.write(payload)


async def _reap(proc: Process, limiter: anyio.CapacityLimiter | None = None) -> None:
    # Shield kill-and-wait so cancellation cannot strand a child past lease release.
    with anyio.CancelScope(shield=True):
        match proc.returncode:
            case None:
                await to_thread.run_sync(_reap_tree, proc.pid, limiter=limiter)
                proc.kill()
            case _:
                pass
        await proc.wait()


def _terminate_process_tree(tree: tuple[psutil.Process, ...]) -> None:
    for proc in reversed(tree):
        if proc.is_running():
            proc.terminate()
    _, alive = psutil.wait_procs(tree, timeout=1.0)
    for proc in alive:
        if proc.is_running():
            proc.kill()
    if alive:
        psutil.wait_procs(alive, timeout=1.0)


def _reap_tree(pid: int) -> None:
    try:
        root = psutil.Process(pid)
        _terminate_process_tree((root, *root.children(recursive=True)))
    except psutil.Error, ValueError:
        return


async def _run_process_backend(plan: _ExecPlan) -> Completed:
    # exec_target selects the local hot path or the remote path; both return the same receipt shape.
    match plan.settings.exec_target:
        case "":
            match plan.streaming:
                case True:
                    proc = await anyio.open_process(list(plan.argv), cwd=plan.cwd, env=plan.env)
                    streams: dict[str, Captured] = {}
                    try:
                        async with anyio.create_task_group() as tg:

                            async def _tee(name: str, stream: ByteReceiveStream | None) -> None:
                                path, handle = _stream_writer(plan, name)
                                if handle is None:
                                    streams[name] = await drain_stream(_recv_anyio(stream, plan.chunk), tail_cap=plan.tail_cap, path=path)
                                    return
                                with handle as sink:
                                    streams[name] = await drain_stream(_recv_anyio(stream, plan.chunk), tail_cap=plan.tail_cap, sink=sink, path=path)

                            tg.start_soon(_tee, "out", proc.stdout)
                            tg.start_soon(_tee, "err", proc.stderr)
                            await proc.wait()
                        stdout, stderr = streams.get("out", Captured()).tail, streams.get("err", Captured()).tail
                        return receipt(
                            plan.argv,
                            proc.returncode or 0,
                            stdout=stdout,
                            stderr=stderr,
                            artifacts=_stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                        )
                    finally:
                        await _reap(proc, plan.thread_limiter)
                case False:
                    done = await anyio.run_process(list(plan.argv), cwd=plan.cwd, env=plan.env, check=False)
                    return receipt(plan.argv, done.returncode, stdout=done.stdout, stderr=done.stderr)
        case target:
            return await _run_remote(replace(plan, env=plan.settings.remote_env(dict(plan.env))), target)


def _stream_artifacts(scope: ArtifactScope | None, settings: AssaySettings, check: Check, streams: Mapping[str, Captured]) -> tuple[Artifact, ...]:
    _ = settings
    match scope:
        case None:
            return ()
        case ArtifactScope():
            return tuple(
                Artifact(id=f"{check.tool.name}-{name}", kind=ArtifactKind.PROCESS, path=captured.path, bytes=captured.size, lines=captured.lines)
                for name, captured in streams.items()
                if captured.path and captured.size
            )


def _stream_writer(plan: _ExecPlan, name: str) -> tuple[str, _WriteContext | None]:
    match plan.scope:
        case None:
            return "", None
        case ArtifactScope(store=store):
            path, handle = store.open_write(ArtifactKind.PROCESS.value, plan.settings.run_id, plan.check.tool.name, f"{name}.log")
            match handle:
                case _WriteContext() as writer:
                    return path, writer
                case _:
                    raise TypeError(f"artifact backend returned non-context writer: {type(handle).__name__}")


async def _run_remote(plan: _ExecPlan, target: str) -> Completed:
    import asyncssh  # noqa: PLC0415  # lazy: asyncssh costs ~83ms cold start; defer past import-time

    # encoding=None keeps remote stdout/stderr as bytes, matching local Completed receipts.
    command = remote_command(plan.argv, cwd=plan.cwd, env=plan.env)
    async with _ssh_connection(target, plan.settings) as conn:
        match plan.streaming:
            case True:
                proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
                streams: dict[str, Captured] = {}
                try:
                    async with anyio.create_task_group() as tg:

                        async def _tee(name: str, reader: asyncssh.SSHReader[bytes]) -> None:
                            path, handle = _stream_writer(plan, name)
                            if handle is None:
                                streams[name] = await drain_stream(_recv_ssh(reader, plan.chunk), tail_cap=plan.tail_cap, path=path)
                                return
                            with handle as sink:
                                streams[name] = await drain_stream(_recv_ssh(reader, plan.chunk), tail_cap=plan.tail_cap, sink=sink, path=path)

                        tg.start_soon(_tee, "out", proc.stdout)
                        tg.start_soon(_tee, "err", proc.stderr)
                        await proc.wait()
                    stdout, stderr = streams.get("out", Captured()).tail, streams.get("err", Captured()).tail
                    code, notes = ssh_outcome(proc.exit_status, getattr(proc, "exit_signal", None))
                    return receipt(
                        plan.argv,
                        code,
                        stdout=stdout,
                        stderr=stderr,
                        notes=notes,
                        artifacts=_stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                    )
                finally:
                    with anyio.CancelScope(shield=True):
                        proc.close()
                        await proc.wait_closed()
            case False:
                done = await conn.run(command, encoding=None, check=False)
                code, notes = ssh_outcome(done.exit_status, getattr(done, "exit_signal", None))
                return receipt(plan.argv, code, stdout=_as_bytes(done.stdout), stderr=_as_bytes(done.stderr), notes=notes)


@contextlib.asynccontextmanager
async def _ssh_connection(target: str, settings: AssaySettings) -> AsyncIterator[asyncssh.SSHClientConnection]:
    cache = _SSH_CACHE.get()
    match cache:
        case None:
            async with _connect(target, settings) as conn:
                yield conn
        case _SshCache() as pooled:
            async with pooled.lock:
                cached = pooled.conns.get(target)
                if cached is None or bool(getattr(cached, "is_closed", lambda: False)()):
                    conn = await _connect_once(target, settings)
                    pooled.conns[target] = conn
                else:
                    conn = cached
            yield conn


@contextlib.asynccontextmanager
async def _connect(target: str, settings: AssaySettings) -> AsyncIterator[asyncssh.SSHClientConnection]:
    conn = await _connect_once(target, settings)
    try:
        yield conn
    finally:
        conn.close()
        await conn.wait_closed()


async def _connect_once(target: str, settings: AssaySettings) -> asyncssh.SSHClientConnection:
    import asyncssh  # noqa: PLC0415  # lazy: asyncssh costs ~83ms cold start; defer past import-time

    parts = urlsplit(target)
    return await asyncssh.connect(
        parts.hostname or "",
        parts.port,
        username=parts.username,
        known_hosts=settings.exec_known_hosts,
        connect_timeout=_SSH_CONNECT_TIMEOUT,
        login_timeout=_SSH_CONNECT_TIMEOUT,
    )


def remote_command(argv: tuple[str, ...], *, cwd: str, env: Mapping[str, str]) -> str:
    """Build a single shell-quoted remote command line carrying the cwd, env exports, and argv.

    Returns:
        A ``cd <cwd> && <exports> <argv>`` line with every segment shell-quoted.
    """
    # cwd and env ride the remote shell command because sshd may reject AcceptEnv; quote every segment.
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def ssh_outcome(status: int | None, signal: object | None) -> tuple[int, tuple[str, ...]]:
    """Resolve a remote exit status and optional signal into a numeric code plus receipt notes.

    Returns:
        The integer exit code (or synthetic 255 for a signalled kill) and any signal-name notes.
    """
    # A signalled remote exit has no numeric status; surface the signal name as receipt evidence beside the synthetic 255.
    match status, signal:
        case int() as code, _:
            return code, ()
        case None, (str() as name, *_):
            return _SSH_SIGNAL_STATUS, (f"ssh.signal={name}",)
        case _:
            return _SSH_SIGNAL_STATUS, ()


def _as_bytes(data: bytes | str | None) -> bytes:
    match data:
        case bytes():
            return data
        case None:
            return b""
        case _:
            return data.encode()


def _safe_call[T](fn: Callable[[], T], default: T) -> T:
    try:
        return fn()
    except psutil.Error, TypeError, ValueError, AttributeError, OSError:
        return default


def _snapshot() -> dict[str, float]:
    # psutil oneshot batches resource facts into the Diagnostic wire shape.
    proc = psutil.Process()
    with proc.oneshot():
        info = proc.memory_info()
        base = {
            "mem.rss_bytes": float(info.rss),
            "mem.vms_bytes": _safe_call(lambda: float(info.vms), -1.0),
            "mem.uss_bytes": _safe_call(lambda: float(proc.memory_full_info().uss), -1.0),
            "mem.percent_rss": _safe_call(lambda: float(info.rss) * 100.0 / max(float(psutil.virtual_memory().total), 1.0), -1.0),
            "cpu.percent": _safe_call(lambda: float(proc.cpu_percent()) if isinstance(proc.cpu_percent, _Nullary) else -1.0, -1.0),
            "proc.num_fds": float(_num_fds(proc)),
            "proc.num_threads": _safe_call(lambda: float(proc.num_threads()) if isinstance(proc.num_threads, _Nullary) else -1.0, -1.0),
        }
    return {**base, **_memory_pressure(), **_load_pressure()}


def _memory_pressure() -> dict[str, float]:
    try:
        mem = psutil.virtual_memory()
        swap = psutil.swap_memory()
        return {
            "sys.mem_available_bytes": _safe_call(lambda: float(mem.available), -1.0),
            "sys.mem_percent": _safe_call(lambda: float(mem.percent), -1.0),
            "sys.swap_percent": _safe_call(lambda: float(swap.percent), -1.0),
        }
    except psutil.Error, TypeError, ValueError, AttributeError:
        return {}


def _load_pressure() -> dict[str, float]:
    try:
        getloadavg = getattr(os, "getloadavg", None)
        if getloadavg is None:
            return {}
        load1, _load5, _load15 = getloadavg()
        return {"sys.load1_percent": load1 * 100.0 / max(float(psutil.cpu_count(logical=True) or 1), 1.0)}
    except OSError, AttributeError, TypeError, ValueError:
        return {}


def _child_rss(child: psutil.Process) -> int:
    return int(getattr(child.memory_info(), "rss", 0))


def _children(proc: psutil.Process) -> dict[str, float]:
    try:
        kids = tuple(proc.children(recursive=True))
        return {"proc.children": float(len(kids)), "proc.children_rss_bytes": float(sum(_safe_call(partial(_child_rss, child), 0) for child in kids))}
    except psutil.Error, TypeError, ValueError, AttributeError:
        return {}


def _num_fds(proc: psutil.Process) -> int:
    try:
        return int(proc.num_fds())  # ty: ignore[possibly-missing-attribute]  # POSIX-only psutil member; the AttributeError arm catches its absence
    except psutil.AccessDenied, NotImplementedError, AttributeError:
        return -1


def _diagnose(exc: BaseException) -> None:
    # Seed span and ContextVar resource context before each Fault is built.
    # The children walk runs only on the real spawn-fault path, not on every _snapshot caller.
    snap = {**_snapshot(), **_children(psutil.Process())}
    _RESOURCE.set(tuple(snap.items()))
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=snap)


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    import asyncssh  # noqa: PLC0415  # lazy import MUST bind before the try frame whose except evaluates asyncssh.Error (not OSError-derived)

    match _materialize(check, settings):
        case Result(tag="ok", ok=prepared):
            check = prepared
        case Result(error=fault):
            return Error(fault)
    argv = _argv(check, routed, settings=settings, scope=scope)
    bound = deadline if deadline is not None else (time.monotonic() + check.tool.timeout if check.tool.timeout is not None else None)
    t0 = time.monotonic()
    attempts = [1]

    def _stamped(message: str) -> str:
        return f"{message} [attempts={attempts[0]}]" if attempts[0] > 1 else message

    try:
        cwd = str(UPath(check.cwd or settings.local_root).path)
        env = _overlay(settings, scope)
        propagate.inject(env)
        trace.get_current_span().set_attribute("exec.target", settings.exec_target)
        done = await _execute_retrying(
            check,
            settings,
            scope,
            argv=argv,
            cwd=cwd,
            env=env,
            thread_limiter=anyio.CapacityLimiter(governed_concurrency(settings, (check,))),
            deadline=bound,
            attempts=attempts,
        )
        return Ok(msgspec.structs.replace(done, duration_ms=(time.monotonic() - t0) * 1000.0))
    except TimeoutError as exc:
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.TIMEOUT, message=_stamped("deadline exceeded")))
    except FileNotFoundError as exc:
        # Absent host binary is a capability gap, not a fault: create_subprocess_exec raises FileNotFoundError at spawn.
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.UNSUPPORTED, message=_stamped(str(exc))))
    except (OSError, ValueError, asyncssh.Error) as exc:
        # Parentheses are PEP 758-required when binding with ``as``; ValueError covers bare NUL-in-argv from create_subprocess_exec.
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.FAULTED, message=_stamped(str(exc))))


async def _execute_retrying(  # noqa: PLR0913  # retry loop: check/settings/scope/argv/cwd/env/limiter/deadline/attempts are all load-bearing across the retry body
    check: Check,
    settings: AssaySettings,
    scope: ArtifactScope | None,
    *,
    argv: tuple[str, ...],
    cwd: str,
    env: Mapping[str, str],
    thread_limiter: anyio.CapacityLimiter,
    deadline: float | None,
    attempts: list[int],
) -> Completed:
    # The cell carries the live attempt count across stamina's exhaustion re-raise so _guarded can stamp the surfaced fault.
    # stamina re-raises on exhaustion and anyio.fail_after raises TimeoutError on deadline — normal return always has done set.
    done: Completed | None = None
    async for attempt in stamina.retry_context(on=retry_predicate(check, deadline), attempts=3, timeout=_retry_timeout(deadline)):
        attempts[0] = attempt.num
        with attempt:
            with anyio.fail_after(_remaining(deadline)):
                done = await _execute(check, settings, scope, argv=argv, cwd=cwd, env=env, thread_limiter=thread_limiter)
    match done:
        case None:  # pragma: no cover  # invariant guard: stamina re-raises on exhaustion, so done is always set on normal return
            raise RuntimeError("stamina exhausted without raising — invariant violated")  # unreachable; stamina re-raises
        case Completed() as result:
            return msgspec.structs.replace(result, notes=(*result.notes, f"retry attempts={attempts[0]}")) if attempts[0] > 1 else result


async def _execute(
    check: Check,
    settings: AssaySettings,
    scope: ArtifactScope | None,
    *,
    argv: tuple[str, ...],
    cwd: str,
    env: Mapping[str, str],
    thread_limiter: anyio.CapacityLimiter,
) -> Completed:
    match check.tool.runner:
        case Runner.INPROC:
            return await _inproc(check, limiter=thread_limiter)
        case _:
            return await _run_process_backend(
                _ExecPlan(
                    argv=argv,
                    check=check,
                    cwd=cwd,
                    env=env,
                    settings=settings,
                    scope=scope,
                    streaming=check.tool.mode.stream,
                    tail_cap=settings.stream_tail_bytes,
                    chunk=settings.stream_chunk_bytes,
                    thread_limiter=thread_limiter,
                )
            )


def _remaining(deadline: float | None) -> float | None:
    return max(0.001, deadline - time.monotonic()) if deadline is not None else None


def _retry_timeout(deadline: float | None) -> float:
    return min(30.0, _remaining(deadline) or 30.0)


def retry_predicate(check: Check, deadline: float | None) -> Callable[[BaseException], bool]:
    """Build a stamina retry predicate that retries transport/spawn faults within the remaining budget.

    Returns:
        A predicate that retries connection/OS faults on non-direct runners while budget remains, never spawn/value/timeout faults.
    """
    import asyncssh  # noqa: PLC0415  # lazy: classify's match arm references asyncssh.Error; bind for the closure

    def within_budget() -> bool:
        remaining = _remaining(deadline)
        return remaining is None or remaining > _RETRY_MIN_REMAINING

    def classify(exc: BaseException) -> bool:
        match exc:
            case FileNotFoundError() | ValueError() | TimeoutError():
                return False
            case asyncssh.Error() | ConnectionError():
                return within_budget()
            case OSError():
                return check.tool.runner is not Runner.DIRECT and within_budget()
            case _:
                return False

    return classify


async def _inproc(check: Check, limiter: anyio.CapacityLimiter | None = None) -> Completed:
    # INPROC thunks run under the same deadline and child span as subprocess tools.
    match check.tool.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC tool carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check, limiter=limiter)
            except Exception as exc:  # noqa: BLE001  # INPROC resilience boundary: any thunk fault -> FAILED receipt, never an uncaught raise across the fan
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    # Retry correlation binds at the engine seam; the engine intentionally has no logged layer.
    span = traced(
        span=check.tool.name,
        attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name},
        agent=lambda *_a, **_k: settings.agent_context,
    )
    # compose is Hom-typed, so threading async _Woven through it needs one checker suppression.
    weave: Callable[[_Woven], _Woven] = compose(_CHECKED_LAYER, span)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return weave(_guarded)


def run_check(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Run one check under a single event loop.

    Returns:
        Completed receipt, or a fault when spawn, lease, or timeout handling fails.
    """

    async def _run() -> Result[Completed, Fault]:
        return await run_check_async(check, settings=settings, scope=scope, routed=routed, deadline=deadline)

    return anyio.run(_run)


async def run_check_async(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Run one check inside an existing event loop.

    Returns:
        Completed receipt, or a fault when spawn, lease, or timeout handling fails.
    """
    return await _spawn(check, settings)(check, settings, scope, routed, deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Run checks concurrently and preserve input order.

    Returns:
        One completed-or-fault result per input check.
    """

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        limit = governed_concurrency(settings, checks)
        results: dict[int, Result[Completed, Fault]] = {}
        with _TRACER.start_as_current_span("assay.fan_out") as parent:
            parent.set_attribute("assay.checks_total", len(checks))
            parent.set_attribute("assay.checks_concurrency", limit)
            async with _pooled_ssh():
                results.update(await _fan_schedule(checks, settings=settings, scope=scope, routed=routed, deadline=deadline, limit=limit))
        return tuple(_total(results.get(i)) for i in range(len(checks)))

    return anyio.run(_run)


async def _fan_schedule(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None, limit: int
) -> dict[int, Result[Completed, Fault]]:
    # Buffer the producer to the full check count so enqueueing never blocks behind a stalled worker; the deadline still bounds the send loop.
    send, recv = anyio.create_memory_object_stream[tuple[int, Check]](max(1, len(checks)))
    out_send, out_recv = anyio.create_memory_object_stream[tuple[int, Result[Completed, Fault]]](limit)
    results: dict[int, Result[Completed, Fault]] = {}
    async with anyio.create_task_group() as tg:
        work_recvs = tuple(recv.clone() for _ in range(limit))
        result_sends = tuple(out_send.clone() for _ in range(limit))
        await recv.aclose()
        await out_send.aclose()
        for work_recv, result_send in zip(work_recvs, result_sends, strict=True):
            tg.start_soon(_fan_worker, work_recv, result_send, settings, scope, routed, deadline)
        async with send:
            with anyio.move_on_after(_remaining(deadline)):
                for item in enumerate(checks):
                    await send.send(item)
        async with out_recv:
            async for index, result in out_recv:
                results[index] = result
                if len(results) == len(checks):
                    break
    return results


async def _fan_worker(
    recv: ObjectReceiveStream[tuple[int, Check]],
    send: ObjectSendStream[tuple[int, Result[Completed, Fault]]],
    settings: AssaySettings,
    scope: ArtifactScope | None,
    routed: Routed,
    deadline: float | None,
) -> None:
    async with recv, send:
        async for i, check in recv:
            await send.send((i, await run_check_async(check, settings=settings, scope=scope, routed=routed, deadline=deadline)))


@contextlib.asynccontextmanager
async def _pooled_ssh() -> AsyncIterator[None]:
    import asyncssh  # noqa: PLC0415  # lazy: the close-loop except evaluates asyncssh.Error; bind before the try frame

    cache = _SshCache({}, anyio.Lock())
    token = _SSH_CACHE.set(cache)
    try:
        yield None
    finally:
        _SSH_CACHE.reset(token)
        for conn in cache.conns.values():
            conn.close()
        for conn in cache.conns.values():
            try:
                await conn.wait_closed()
            except asyncssh.Error as exc:
                _LOG.warning("ssh.close_failed", error=str(exc)[:200])
            except OSError as exc:
                _LOG.warning("ssh.close_failed", error=str(exc)[:200])


def _total(slot: Result[Completed, Fault] | None) -> Result[Completed, Fault]:
    match slot:
        case None:
            return Error(Fault((), status=RailStatus.TIMEOUT, message="deadline exceeded"))
        case Result() as r:
            return r


def governed_concurrency(settings: AssaySettings, checks: tuple[Check, ...] = ()) -> int:
    """Fold the usable-CPU, DOTNET-runner, and MUTATION-mode ceilings into one concurrency floor.

    Returns:
        The concurrency limit, at least 1, bounded by ``max_checks``, the mode cap, and ``settings.cpu_count``.
    """
    runner_cap = settings.dotnet_max_cpu if any(c.tool.runner is Runner.DOTNET for c in checks) else settings.max_checks
    # Mixed DOTNET+MUTATION batches must honour BOTH ceilings — intersect, never let the mode cap shadow the runner cap.
    mode_cap = min(runner_cap, settings.mutation_max_cpu) if any(c.tool.mode is Mode.MUTATION for c in checks) else runner_cap
    return max(1, min(settings.max_checks, mode_cap, settings.cpu_count))


# --- [COMPOSITION] ----------------------------------------------------------------------


def is_lease_stale(owner: _LeaseOwner, tolerance: float) -> bool:
    """Decide whether a lease holder is dead or PID-reused and therefore stealable.

    Returns:
        True when the holder process is gone, not running, or drifted past ``tolerance``; False when it is live and matching.
    """
    # Match pid plus create_time within the drift band so PID reuse does not preserve stale locks.
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():
            return not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except psutil.NoSuchProcess, ValueError:
        # ValueError covers psutil.Process(pid<=0) from a corrupt/adversarial owner block: an unresolvable pid is dead and stealable.
        return True
    except psutil.AccessDenied:
        # AccessDenied proves a pid is still owned by the OS; do not steal and risk releasing a live holder's lease.
        return not psutil.pid_exists(owner.pid)


def _claim(
    fd: int, resource: str, *, run_id: str, tolerance: float, target: str, cwd: str = "", project: str = "", mode: str = "exclusive"
) -> _LeaseOwner | None:
    # Non-blocking flock wins cleanly, steals stale/corrupt holders, and maps live holders to BUSY.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id, target=target, cwd=cwd, project=project, mode=mode)
    except BlockingIOError:
        held = os.read(fd, 4096)
        # b"" UNDER a contended flock can only be a live holder mid-write/mid-release (a dead holder dropped its flock,
        # so the success path above would have won). Map it to BUSY without a steal log; only present-but-corrupt or
        # present-but-stale blocks fall through to the decode + is_lease_stale + steal path.
        match held:
            case b"":
                _stamp_holder(None)
                return None
            case _:
                owner = decode_lease_owner(held)
                _stamp_holder(owner)
                match owner is not None and not is_lease_stale(owner, tolerance):
                    case True:
                        return None
                    case False:
                        _LOG.warning("lease.steal", resource=resource, run_id=run_id, lost=_holder(owner))
                        return _steal(fd, resource, run_id=run_id, target=target, cwd=cwd, project=project, mode=mode, owner=owner)


def _steal(
    fd: int, resource: str, *, run_id: str, target: str, cwd: str = "", project: str = "", mode: str = "exclusive", owner: _LeaseOwner | None
) -> _LeaseOwner | None:
    # A lost TOCTOU race yields BUSY, not FAULTED.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
    except BlockingIOError:
        return None
    won = _write_owner(fd, resource, run_id=run_id, target=target, cwd=cwd, project=project, mode=mode)
    _LOG.info("lease.stolen", resource=resource, run_id=run_id, lost=_holder(owner), won=_holder(won))
    return won


def _holder(owner: _LeaseOwner | None) -> dict[str, object]:
    match owner:
        case None:
            return {}
        case _:
            return {"pid": owner.pid, "create_time": owner.create_time, "run_id": owner.run_id}


def _stamp_holder(owner: _LeaseOwner | None) -> None:
    trace.get_current_span().add_event("lease.contested", attributes={f"holder.{k}": str(v) for k, v in _holder(owner).items()})


def decode_lease_owner(held: bytes) -> _LeaseOwner | None:
    """Decode lock-file bytes into a lease owner, treating empty or corrupt content as absent.

    Returns:
        The decoded owner, or ``None`` when the bytes are empty or fail to decode.
    """
    # Empty or corrupt lock bytes represent a stealable stale holder.
    match held:
        case b"":
            return None
        case _:
            try:
                return _DECODER.decode(held)
            except msgspec.DecodeError:
                return None


def _write_all(fd: int, payload: bytes) -> None:
    # POSIX write(2) may short-write; iterate the remaining tail at the advanced fd offset.
    view = memoryview(payload)
    while view:
        view = view[os.write(fd, view) :]


def _write_block(fd: int, block: _LeaseOwner) -> _LeaseOwner:
    os.ftruncate(fd, 0)
    os.lseek(fd, 0, os.SEEK_SET)
    _write_all(fd, msgspec.json.encode(block))
    return block


def _write_owner(fd: int, resource: str, *, run_id: str, target: str, cwd: str = "", project: str = "", mode: str = "exclusive") -> _LeaseOwner:
    # target records whether the holder ran locally or through ssh://; cwd/project/mode are stamped here so the block is written once.
    proc = psutil.Process(os.getpid())
    with proc.oneshot():
        block = _LeaseOwner(
            resource=resource,
            run_id=run_id,
            pid=proc.pid,
            create_time=proc.create_time(),
            cwd=cwd,
            project=project,
            mode=mode,
            started_at=time.time(),
            target=target,
        )
    return _write_block(fd, block)


@contextlib.contextmanager
def exclusive_lease(
    resource: str, run_id: str, *, settings: AssaySettings, project: str = "", mode: str = "exclusive"
) -> Generator[Result[_Held, Fault]]:
    """Acquire a non-blocking process lease.

    Args:
        resource: Lease resource name.
        run_id: Current run identifier.
        settings: Runtime settings.
        project: Project label written into the owner block.
        mode: Lease mode label written into the owner block.

    Yields:
        Result containing the held lease or a busy fault.
    """
    path = settings.artifact(ArtifactKind.LOCKS, f"{resource}.lock")
    # fcntl.flock + os.open + mkdir are local-fs POSIX primitives; a remote artifact backend cannot host the lease.
    if path.protocol not in {"", "file"}:
        yield Error(Fault((), status=RailStatus.UNSUPPORTED, message=f"POSIX leases require a local artifact root; got {path.protocol!r} backend"))
        return
    path.parent.mkdir(parents=True, exist_ok=True)
    fd = os.open(str(path), _LOCKS_OPEN_FLAGS, _LOCK_MODE)
    owner: _LeaseOwner | None = None
    try:
        owner = _claim(
            fd,
            resource,
            run_id=run_id,
            tolerance=settings.lease_drift_tolerance,
            target=settings.exec_target,
            cwd=str(settings.root),
            project=project,
            mode=mode,
        )
        yield Ok(_Held(owner)) if owner is not None else Error(Fault((), status=RailStatus.BUSY, message=f"{resource} held by a live process"))
    finally:
        if owner is not None:
            os.ftruncate(fd, 0)
            _FLOCK(fd, _LOCK_UN)
        os.close(fd)


def leased[T](
    resource: str, action: Callable[[_Held], Result[T, Fault]], *, settings: AssaySettings, run_id: str, project: str = "", mode: str = "exclusive"
) -> Result[T, Fault]:
    """Run an action only after acquiring a lease.

    Returns:
        Action result when the lease is held, or a busy/fault result otherwise.
    """
    try:
        with exclusive_lease(resource, run_id, settings=settings, project=project, mode=mode) as held:
            match held:
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=owned):
                    return action(owned)
                case never:  # pragma: no cover
                    return Error(Fault((), status=RailStatus.FAULTED, message=str(never)))
    except OSError as exc:
        _diagnose(exc)
        return Error(Fault((), status=RailStatus.FAULTED, message=str(exc)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "_RESOURCE",
    "ByteRecv",
    "Captured",
    "WriteSink",
    "argv_for",
    "decode_lease_owner",
    "discover",
    "discover_async",
    "drain_stream",
    "exclusive_lease",
    "fan_out",
    "governed_concurrency",
    "is_lease_stale",
    "leased",
    "remote_command",
    "retry_predicate",
    "run_check",
    "run_check_async",
    "splice_command",
    "ssh_outcome",
]
