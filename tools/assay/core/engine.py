"""Execute checks through local, remote, in-process, and leased runners."""

from collections.abc import Callable
import contextlib
from contextvars import ContextVar
from dataclasses import dataclass, replace
import fcntl
import os
from pathlib import Path
import shlex
import shutil
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import anyio
from anyio import to_thread  # explicit submodule import; ty mis-resolves anyio.to_thread
import asyncssh
from expression import Error, Ok, Result
import msgspec
from opentelemetry import propagate, trace
import psutil
import stamina
import structlog
from upath import UPath

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # unconditional for beartype forward refs
from tools.assay.core.aspect import checked, compose, traced
from tools.assay.core.model import Artifact, ArtifactKind, Check, Completed, Fault, Mode, receipt, Runner
from tools.assay.core.routing import place, Routed
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, ObjectReceiveStream, ObjectSendStream, Process


# --- [TYPES] ----------------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]


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


_SSH_CACHE: ContextVar[_SshCache | None] = ContextVar("assay_ssh_cache", default=None)


@runtime_checkable
class _Nullary(Protocol):
    def __call__(self) -> float | int | str: ...


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _splice(runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str], mode: Mode) -> tuple[str, ...]:
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
    body = _splice(tool.runner, tool.command, scope, settings.scoped_verbs, tool.mode)
    body = ("--project", str(settings.root), *body) if tool.runner is Runner.UV and tool.stage.project else body
    tails = place(routed, tool, settings=settings)
    return (*tool.runner.prefix, *body, *(part for tail in tails for part in tail))


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
    if isinstance(dst, ValueError):
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
    tail, _full = await _drain_capture(stream, tail_cap=tail_cap, chunk=chunk)
    return tail


async def _drain_capture(stream: ByteReceiveStream | None, *, tail_cap: int, chunk: int) -> tuple[bytes, bytes]:
    match stream:
        case None:
            return b"", b""
        case _:
            tail, chunks = b"", []
            while (read := await _next_chunk(stream, chunk=chunk)) is not None:
                chunks.append(read)
                tail = (tail + read)[-tail_cap:]
            return tail, b"".join(chunks)


async def _next_chunk(stream: ByteReceiveStream, *, chunk: int) -> bytes | None:
    try:
        return await stream.receive(chunk)
    except anyio.EndOfStream:
        return None


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


def _reap_tree(pid: int) -> None:
    try:
        root = psutil.Process(pid)
        tree = (root, *root.children(recursive=True))
        for proc in reversed(tree):
            if proc.is_running():
                proc.terminate()
        _, alive = psutil.wait_procs(tree, timeout=1.0)
        for proc in alive:
            if proc.is_running():
                proc.kill()
        if alive:
            psutil.wait_procs(alive, timeout=1.0)
    except psutil.Error, ValueError:
        return


async def _run_process_backend(plan: _ExecPlan) -> Completed:
    # exec_target selects the local hot path or the remote path; both return the same receipt shape.
    match plan.settings.exec_target:
        case "":
            match plan.streaming:
                case True:
                    proc = await anyio.open_process(list(plan.argv), cwd=plan.cwd, env=plan.env)
                    streams: dict[str, tuple[bytes, bytes]] = {}
                    try:
                        async with anyio.create_task_group() as tg:

                            async def _tee(name: str, stream: ByteReceiveStream | None) -> None:
                                streams[name] = await _drain_capture(stream, tail_cap=plan.tail_cap, chunk=plan.chunk)

                            tg.start_soon(_tee, "out", proc.stdout)
                            tg.start_soon(_tee, "err", proc.stderr)
                            await proc.wait()
                        stdout, stderr = streams.get("out", (b"", b""))[0], streams.get("err", (b"", b""))[0]
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


def _stream_artifacts(
    scope: ArtifactScope | None, settings: AssaySettings, check: Check, streams: Mapping[str, tuple[bytes, bytes]]
) -> tuple[Artifact, ...]:
    match scope:
        case None:
            return ()
        case ArtifactScope(store=store):
            return tuple(
                Artifact(
                    id=f"{check.tool.name}-{name}",
                    kind=ArtifactKind.PROCESS,
                    path=store.write_bytes(full, ArtifactKind.PROCESS.value, settings.run_id, check.tool.name, f"{name}.log"),
                    bytes=len(full),
                    lines=full.count(b"\n") + (1 if full and not full.endswith(b"\n") else 0),
                )
                for name, (_tail, full) in streams.items()
                if full
            )


async def _run_remote(plan: _ExecPlan, target: str) -> Completed:
    # encoding=None keeps remote stdout/stderr as bytes, matching local Completed receipts.
    command = _remote_command(plan.argv, cwd=plan.cwd, env=plan.env)
    async with _ssh_connection(target, plan.settings) as conn:
        match plan.streaming:
            case True:
                proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
                streams: dict[str, tuple[bytes, bytes]] = {}
                try:
                    async with anyio.create_task_group() as tg:

                        async def _tee(name: str, reader: asyncssh.SSHReader[bytes]) -> None:
                            streams[name] = await _drain_reader(reader, tail_cap=plan.tail_cap, chunk=plan.chunk)

                        tg.start_soon(_tee, "out", proc.stdout)
                        tg.start_soon(_tee, "err", proc.stderr)
                        await proc.wait()
                    stdout, stderr = streams.get("out", (b"", b""))[0], streams.get("err", (b"", b""))[0]
                    return receipt(
                        plan.argv,
                        _ssh_status(proc.exit_status, getattr(proc, "exit_signal", None)),
                        stdout=stdout,
                        stderr=stderr,
                        artifacts=_stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                    )
                finally:
                    with anyio.CancelScope(shield=True):
                        proc.close()
                        await proc.wait_closed()
            case False:
                done = await conn.run(command, encoding=None, check=False)
                return receipt(
                    plan.argv,
                    _ssh_status(done.exit_status, getattr(done, "exit_signal", None)),
                    stdout=_as_bytes(done.stdout),
                    stderr=_as_bytes(done.stderr),
                )


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
    parts = urlsplit(target)
    return await asyncssh.connect(
        parts.hostname or "",
        parts.port,
        username=parts.username,
        known_hosts=settings.exec_known_hosts,
        connect_timeout=_SSH_CONNECT_TIMEOUT,
        login_timeout=_SSH_CONNECT_TIMEOUT,
    )


def _remote_command(argv: tuple[str, ...], *, cwd: str, env: Mapping[str, str]) -> str:
    # cwd and env ride the remote shell command because sshd may reject AcceptEnv; quote every segment.
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def _ssh_status(status: int | None, signal: object | None) -> int:
    _ = signal
    return status if status is not None else _SSH_SIGNAL_STATUS


def _as_bytes(data: bytes | str | None) -> bytes:
    match data:
        case bytes():
            return data
        case None:
            return b""
        case _:
            return data.encode()


async def _drain_reader(reader: asyncssh.SSHReader[bytes], *, tail_cap: int, chunk: int) -> tuple[bytes, bytes]:
    # Remote analogue of _drain_capture; an empty read is EOF.
    tail, chunks = b"", []
    while (read := await reader.read(chunk)) != b"":
        chunks.append(read)
        tail = (tail + read)[-tail_cap:]
    return tail, b"".join(chunks)


def _snapshot() -> dict[str, float]:
    # psutil oneshot batches resource facts into the Diagnostic wire shape.
    proc = psutil.Process()
    with proc.oneshot():
        info = proc.memory_info()
        full = _memory_full(proc)
        base = {
            "mem.rss_bytes": float(info.rss),
            "mem.vms_bytes": _float_attr(info, "vms"),
            "mem.uss_bytes": _float_attr(full, "uss"),
            "mem.percent_rss": _rss_percent(info),
            "cpu.percent": _float_call(proc.cpu_percent),
            "proc.num_fds": float(_num_fds(proc)),
            "proc.num_threads": _float_call(proc.num_threads),
        }
    return {**base, **_memory_pressure(), **_load_pressure(), **_children(proc)}


def _memory_full(proc: psutil.Process) -> object:
    try:
        return proc.memory_full_info()
    except psutil.Error, AttributeError:
        return object()


def _rss_percent(info: object) -> float:
    try:
        total = psutil.virtual_memory().total
        return _float_attr(info, "rss") * 100.0 / max(float(total), 1.0)
    except psutil.Error, TypeError, ValueError, AttributeError:
        return -1.0


def _float_attr(obj: object, name: str) -> float:
    try:
        return float(getattr(obj, name))
    except TypeError, ValueError, AttributeError:
        return -1.0


def _float_call(fn: object) -> float:
    try:
        match fn:
            case _Nullary() as call:
                return float(call())
            case _:
                return -1.0
    except psutil.Error, TypeError, ValueError, AttributeError:
        return -1.0


def _memory_pressure() -> dict[str, float]:
    try:
        mem = psutil.virtual_memory()
        swap = psutil.swap_memory()
        return {
            "sys.mem_available_bytes": _float_attr(mem, "available"),
            "sys.mem_percent": _float_attr(mem, "percent"),
            "sys.swap_percent": _float_attr(swap, "percent"),
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


def _children(proc: psutil.Process) -> dict[str, float]:
    try:
        kids = tuple(proc.children(recursive=True))
        return {"proc.children": float(len(kids)), "proc.children_rss_bytes": float(sum(_child_rss(child) for child in kids))}
    except psutil.Error, TypeError, ValueError, AttributeError:
        return {}


def _child_rss(proc: psutil.Process) -> int:
    try:
        return int(getattr(proc.memory_info(), "rss", 0))
    except psutil.Error, TypeError, ValueError, AttributeError:
        return 0


def _num_fds(proc: psutil.Process) -> int:
    try:
        return int(proc.num_fds())  # ty: ignore[possibly-missing-attribute]  # POSIX-only psutil member; the AttributeError arm catches its absence
    except psutil.AccessDenied, NotImplementedError, AttributeError:
        return -1


def _diagnose(exc: BaseException) -> None:
    # Seed span and ContextVar resource context before each Fault is built.
    snap = _snapshot()
    _RESOURCE.set(tuple(snap.items()))
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=snap)


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    match _materialize(check, settings):
        case Result(tag="ok", ok=prepared):
            check = prepared
        case Result(error=fault):
            return Error(fault)
    argv = _argv(check, routed, settings=settings, scope=scope)
    absolute_deadline = deadline if deadline is not None else (time.monotonic() + check.tool.timeout if check.tool.timeout is not None else None)
    started = time.monotonic()
    try:
        cwd = str(UPath(check.cwd or settings.local_root).path)
        env = _overlay(settings, scope)
        propagate.inject(env)
        trace.get_current_span().set_attribute("exec.target", settings.exec_target)
        thread_limiter = anyio.CapacityLimiter(_governed(settings, (check,)))
        done: Completed | None = None
        async for attempt in stamina.retry_context(on=_retry_on(check, absolute_deadline), attempts=3, timeout=_retry_timeout(absolute_deadline)):
            with attempt:
                with anyio.fail_after(_remaining(absolute_deadline)):
                    done = await _execute(check, settings, scope, argv=argv, cwd=cwd, env=env, thread_limiter=thread_limiter)
        return (
            Ok(msgspec.structs.replace(done, duration_ms=(time.monotonic() - started) * 1000.0))
            if done is not None
            else Error(Fault(argv, status=RailStatus.TIMEOUT, message="deadline exceeded"))
        )
    except TimeoutError as exc:
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.TIMEOUT, message="deadline exceeded"))
    except FileNotFoundError as exc:
        # Absent host binary is a capability gap, not a fault: create_subprocess_exec raises FileNotFoundError at spawn.
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.UNSUPPORTED, message=str(exc)))
    except (OSError, ValueError, asyncssh.Error) as exc:
        # ValueError covers bare NUL-in-argv from create_subprocess_exec; it is not an OSError so it must be named explicitly.
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.FAULTED, message=str(exc)))


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


def _retry_on(check: Check, deadline: float | None) -> Callable[[BaseException], bool]:
    def within_budget() -> bool:
        remaining = _remaining(deadline)
        return remaining is None or remaining > _RETRY_MIN_REMAINING

    def classify(exc: BaseException) -> bool:
        match exc:
            case FileNotFoundError() | ValueError() | TimeoutError():
                return False
            case asyncssh.Error() | ConnectionError() | BrokenPipeError():
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
    weave: Callable[[_Woven], _Woven] = compose(checked(), span)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return weave(_guarded)


def run_check(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Run one check under a single event loop.

    Returns:
        Completed receipt, or a fault when spawn, lease, or timeout handling fails.
    """
    # fan_out awaits _spawn directly to avoid nested anyio.run.
    return anyio.run(_spawn(check, settings), check, settings, scope, routed, deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Run checks concurrently and preserve input order.

    Returns:
        One completed-or-fault result per input check.
    """

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        limit = _governed(settings, checks)
        results: dict[int, Result[Completed, Fault]] = {}
        with _TRACER.start_as_current_span("assay.fan_out") as parent:
            parent.set_attribute("assay.checks_total", len(checks))
            parent.set_attribute("assay.checks_concurrency", limit)
            async with _pooled_ssh():
                with anyio.move_on_after(deadline - time.monotonic() if deadline is not None else None):
                    results.update(await _fan_schedule(checks, settings=settings, scope=scope, routed=routed, deadline=deadline, limit=limit))
        return tuple(_total(results.get(i)) for i in range(len(checks)))

    return anyio.run(_run)


async def _fan_schedule(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None, limit: int
) -> dict[int, Result[Completed, Fault]]:
    send, recv = anyio.create_memory_object_stream[tuple[int, Check]](limit)
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
            await send.send((i, await _spawn(check, settings)(check, settings, scope, routed, deadline)))


@contextlib.asynccontextmanager
async def _pooled_ssh() -> AsyncIterator[None]:
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


def _governed(settings: AssaySettings, checks: tuple[Check, ...] = ()) -> int:
    usable = _usable_cpu(settings.max_checks)
    runner_cap = settings.dotnet_max_cpu if any(c.tool.runner is Runner.DOTNET for c in checks) else settings.max_checks
    mode_cap = settings.mutation_max_cpu if any(c.tool.mode is Mode.MUTATION for c in checks) else runner_cap
    return max(1, min(settings.max_checks, mode_cap, usable))


def _usable_cpu(default: int) -> int:
    try:
        return len(psutil.Process().cpu_affinity())  # type: ignore[attr-defined]  # ty: ignore[possibly-missing-attribute]  # Linux/cgroup-aware when present
    except psutil.Error, AttributeError, NotImplementedError:
        return psutil.cpu_count(logical=True) or default


# --- [COMPOSITION] ----------------------------------------------------------------------


def _stale(owner: _LeaseOwner, tolerance: float) -> bool:
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


def _claim(fd: int, resource: str, *, run_id: str, tolerance: float, target: str) -> _LeaseOwner | None:
    # Non-blocking flock wins cleanly, steals stale/corrupt holders, and maps live holders to BUSY.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id, target=target)
    except BlockingIOError:
        held = os.read(fd, 4096)
        # b"" UNDER a contended flock can only be a live holder mid-write/mid-release (a dead holder dropped its flock,
        # so the success path above would have won). Map it to BUSY without a steal log; only present-but-corrupt or
        # present-but-stale blocks fall through to the decode + _stale + steal path.
        match held:
            case b"":
                _stamp_holder(None, run_id=run_id)
                return None
            case _:
                owner = _decode_owner(held)
                _stamp_holder(owner, run_id=run_id)
                match owner is not None and not _stale(owner, tolerance):
                    case True:
                        return None
                    case False:
                        _LOG.warning("lease.steal", resource=resource, run_id=run_id, lost=_holder(owner))
                        return _steal(fd, resource, run_id=run_id, target=target, owner=owner)


def _steal(fd: int, resource: str, *, run_id: str, target: str, owner: _LeaseOwner | None) -> _LeaseOwner | None:
    # A lost TOCTOU race yields BUSY, not FAULTED.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
    except BlockingIOError:
        return None
    won = _write_owner(fd, resource, run_id=run_id, target=target)
    _LOG.info("lease.stolen", resource=resource, run_id=run_id, lost=_holder(owner), won=_holder(won))
    return won


def _holder(owner: _LeaseOwner | None) -> dict[str, object]:
    match owner:
        case None:
            return {}
        case _:
            return {"pid": owner.pid, "create_time": owner.create_time, "run_id": owner.run_id}


def _stamp_holder(owner: _LeaseOwner | None, *, run_id: str) -> None:
    span = trace.get_current_span()
    span.set_attributes({"assay.run_id": run_id, **{f"holder.{k}": str(v) for k, v in _holder(owner).items()}})


def _decode_owner(held: bytes) -> _LeaseOwner | None:
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


def _write_owner(fd: int, resource: str, *, run_id: str, target: str) -> _LeaseOwner:
    # target records whether the holder ran locally or through ssh://.
    proc = psutil.Process(os.getpid())
    with proc.oneshot():
        block = _LeaseOwner(resource=resource, run_id=run_id, pid=proc.pid, create_time=proc.create_time(), started_at=time.time(), target=target)
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
    path.parent.mkdir(parents=True, exist_ok=True)
    fd = os.open(str(path), _LOCKS_OPEN_FLAGS, _LOCK_MODE)
    owner: _LeaseOwner | None = None
    try:
        owner = _claim(fd, resource, run_id=run_id, tolerance=settings.lease_drift_tolerance, target=settings.exec_target)
        match owner:
            case None:
                yield Error(Fault((), status=RailStatus.BUSY, message=f"{resource} held by a live process"))
            case _:
                stamped = msgspec.structs.replace(owner, run_id=run_id, cwd=str(settings.root), project=project, mode=mode)
                _write_block(fd, stamped)
                yield Ok(_Held(stamped))
    finally:
        match owner:
            case None:
                os.close(fd)
            case _:
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

__all__ = ["_RESOURCE", "exclusive_lease", "fan_out", "leased", "run_check"]
