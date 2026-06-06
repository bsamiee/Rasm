"""Execute checks through local, remote, in-process, and leased runners."""

import contextlib
from contextvars import ContextVar
from dataclasses import dataclass
import fcntl
import os
from pathlib import Path
import shlex
import shutil
import time
from typing import TYPE_CHECKING
from urllib.parse import urlsplit

import anyio
from anyio import to_thread  # explicit submodule import; ty mis-resolves anyio.to_thread
import asyncssh
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
import psutil
import structlog
from upath import UPath

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # unconditional for beartype forward refs
from tools.assay.core.aspect import checked, compose, compose_spawn, retried, traced
from tools.assay.core.model import ArtifactKind, Check, Completed, Fault, Mode, receipt, ResourceBusyError, Runner
from tools.assay.core.routing import place, Routed
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, Process


# --- [TYPES] ----------------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"
_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)
_LOG = structlog.get_logger("assay.engine")
_TRACER = trace.get_tracer("assay.engine")

# POSIX-only fcntl members bind once because ty checks all platforms and cannot narrow this module to POSIX.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]
# Fault-time resource snapshots cross the anyio.run boundary through this ContextVar.
_RESOURCE: ContextVar[tuple[tuple[str, float], ...]] = ContextVar("assay_resource", default=())


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
    work = root / stage.root
    shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True, exist_ok=True)
    for rel in stage.inputs:
        src = root / rel
        dst = work / rel
        if not src.exists():
            return Error(Fault((check.tool.name, "stage", rel), message=f"missing stage input: {rel}"))
        if src.is_dir():
            shutil.copytree(src, dst)
        else:
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, dst)
    return Ok(msgspec.structs.replace(check, cwd=work))


async def _drain(stream: ByteReceiveStream | None, *, tail_cap: int, chunk: int) -> bytes:
    match stream:
        case None:
            return b""
        case _:

            async def _fold(acc: bytes) -> bytes:
                read = await _next_chunk(stream, chunk=chunk)
                match read:
                    case None:
                        return acc
                    case _:
                        return await _fold((acc + read)[-tail_cap:])

            return await _fold(b"")


async def _next_chunk(stream: ByteReceiveStream, *, chunk: int) -> bytes | None:
    try:
        return await stream.receive(chunk)
    except anyio.EndOfStream:
        return None


async def _reap(proc: Process) -> None:
    # Shield kill-and-wait so cancellation cannot strand a child past lease release.
    with anyio.CancelScope(shield=True):
        match proc.returncode:
            case None:
                proc.kill()
            case _:
                pass
        await proc.wait()


async def _run_process_backend(
    argv: tuple[str, ...], *, cwd: str, env: Mapping[str, str], settings: AssaySettings, streaming: bool, tail_cap: int, chunk: int
) -> Completed:
    # exec_target selects the local hot path or the remote path; both return the same receipt shape.
    match settings.exec_target:
        case "":
            match streaming:
                case True:
                    proc = await anyio.open_process(list(argv), cwd=cwd, env=env)
                    tails: dict[str, bytes] = {}
                    try:
                        async with anyio.create_task_group() as tg:

                            async def _tee(name: str, stream: ByteReceiveStream | None) -> None:
                                tails[name] = await _drain(stream, tail_cap=tail_cap, chunk=chunk)

                            tg.start_soon(_tee, "out", proc.stdout)
                            tg.start_soon(_tee, "err", proc.stderr)
                            await proc.wait()
                        return receipt(argv, proc.returncode or 0, stdout=tails.get("out", b""), stderr=tails.get("err", b""))
                    finally:
                        await _reap(proc)
                case False:
                    done = await anyio.run_process(list(argv), cwd=cwd, env=env, check=False)
                    return receipt(argv, done.returncode, stdout=done.stdout, stderr=done.stderr)
        case target:
            return await _run_remote(argv, target, cwd=cwd, env=env, settings=settings, streaming=streaming, tail_cap=tail_cap, chunk=chunk)


# Bounds the SSH connect + login so a routable-but-unreachable exec_target (TCP black-hole) cannot deadlock the
# rail: catalog tools carry no timeout, so the outer anyio.fail_after(None) is a no-op for remote spawns.
_SSH_CONNECT_TIMEOUT: float = 15.0


async def _run_remote(
    argv: tuple[str, ...], target: str, *, cwd: str, env: Mapping[str, str], settings: AssaySettings, streaming: bool, tail_cap: int, chunk: int
) -> Completed:
    # encoding=None keeps remote stdout/stderr as bytes, matching local Completed receipts.
    parts = urlsplit(target)
    port = parts.port if parts.port is not None else ()
    command = _remote_command(argv, cwd=cwd, env=env)
    async with asyncssh.connect(
        parts.hostname or "",
        port,
        username=parts.username or (),
        known_hosts=settings.exec_known_hosts,
        connect_timeout=_SSH_CONNECT_TIMEOUT,
        login_timeout=_SSH_CONNECT_TIMEOUT,
    ) as conn:
        match streaming:
            case True:
                proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
                tails: dict[str, bytes] = {}
                try:
                    async with anyio.create_task_group() as tg:

                        async def _tee(name: str, reader: asyncssh.SSHReader[bytes]) -> None:
                            tails[name] = await _drain_reader(reader, tail_cap=tail_cap, chunk=chunk)

                        tg.start_soon(_tee, "out", proc.stdout)
                        tg.start_soon(_tee, "err", proc.stderr)
                        await proc.wait()
                    return receipt(argv, proc.exit_status or 0, stdout=tails.get("out", b""), stderr=tails.get("err", b""))
                finally:
                    with anyio.CancelScope(shield=True):
                        proc.close()
                        await proc.wait_closed()
            case False:
                done = await conn.run(command, encoding=None, check=False)
                return receipt(argv, done.exit_status or 0, stdout=_as_bytes(done.stdout), stderr=_as_bytes(done.stderr))


def _remote_command(argv: tuple[str, ...], *, cwd: str, env: Mapping[str, str]) -> str:
    # cwd and env ride the remote shell command because sshd may reject AcceptEnv; quote every segment.
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def _as_bytes(data: bytes | str | None) -> bytes:
    match data:
        case bytes():
            return data
        case None:
            return b""
        case _:
            return data.encode()


async def _drain_reader(reader: asyncssh.SSHReader[bytes], *, tail_cap: int, chunk: int) -> bytes:
    # Remote analogue of _drain; an empty read is EOF.
    async def _fold(acc: bytes) -> bytes:
        read = await reader.read(chunk)
        match read:
            case b"":
                return acc
            case _:
                return await _fold((acc + read)[-tail_cap:])

    return await _fold(b"")


def _snapshot() -> dict[str, float]:
    # psutil oneshot batches resource facts into the Diagnostic wire shape.
    proc = psutil.Process()
    with proc.oneshot():
        return {"mem.rss_bytes": float(proc.memory_info().rss), "cpu.percent": proc.cpu_percent(), "proc.num_fds": float(_num_fds(proc))}


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
    prepared = _materialize(check, settings)
    if prepared.is_error():
        return Error(prepared.error)
    check = prepared.ok
    argv = _argv(check, routed, settings=settings, scope=scope)
    budget = deadline - time.monotonic() if deadline is not None else check.tool.timeout
    cwd = str(UPath(check.cwd or settings.root).path)
    env = _overlay(settings, scope)
    trace.get_current_span().set_attribute("exec.target", settings.exec_target)
    started = time.monotonic()
    try:
        with anyio.fail_after(budget):
            match check.tool.runner:
                case Runner.INPROC:
                    done = await _inproc(check)
                case _:
                    done = await _run_process_backend(
                        argv,
                        cwd=cwd,
                        env=env,
                        settings=settings,
                        streaming=check.tool.mode.stream,
                        tail_cap=settings.stream_tail_bytes,
                        chunk=settings.stream_chunk_bytes,
                    )
        return Ok(msgspec.structs.replace(done, duration_ms=(time.monotonic() - started) * 1000.0))
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


async def _inproc(check: Check) -> Completed:
    # INPROC thunks run under the same deadline and child span as subprocess tools.
    match check.tool.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC tool carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check)
            except Exception as exc:  # noqa: BLE001  # INPROC resilience boundary: any thunk fault -> FAILED receipt, never an uncaught raise across the fan
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    # Retry correlation binds at the engine seam; the engine intentionally has no logged layer.
    span = traced(
        span=check.tool.name,
        attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name},
        agent=lambda *_a, **_k: settings.agent_context,
    )
    # mypy binds the no-arg retried() factory to Never; ty specializes it correctly at the _guarded apply.
    layered: _Woven = compose_spawn(retried())(_guarded)  # type: ignore[arg-type, assignment]  # mypy-only: retried() loses the ParamSpec through the runtime aspect boundary
    # compose is Hom-typed, so threading async _Woven through it needs one checker suppression.
    weave: Callable[[_Woven], _Woven] = compose(checked(), span)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return weave(layered)


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
    spawn = _spawn

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        limiter = anyio.CapacityLimiter(_governed(settings))
        slots: list[Result[Completed, Fault] | None] = [None] * len(checks)

        async def _into(i: int, check: Check) -> None:
            async with limiter:
                slots[i] = await spawn(check, settings)(check, settings, scope, routed, deadline)

        with _TRACER.start_as_current_span("assay.fan_out") as parent:
            parent.set_attribute("assay.checks_total", len(checks))
            with anyio.move_on_after(deadline - time.monotonic() if deadline is not None else None):
                async with anyio.create_task_group() as tg:
                    for i, check in enumerate(checks):
                        tg.start_soon(_into, i, check)
        return tuple(_total(slot) for slot in slots)

    return anyio.run(_run)


def _total(slot: Result[Completed, Fault] | None) -> Result[Completed, Fault]:
    match slot:
        case None:
            return Error(Fault((), status=RailStatus.TIMEOUT, message="deadline exceeded"))
        case Result() as r:
            return r


def _governed(settings: AssaySettings) -> int:
    return min(settings.max_checks, psutil.cpu_count(logical=True) or settings.max_checks)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _stale(owner: _LeaseOwner, tolerance: float) -> bool:
    # Match pid plus create_time within the drift band so PID reuse does not preserve stale locks.
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():
            return not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except psutil.NoSuchProcess, psutil.AccessDenied, ValueError:
        # ValueError covers psutil.Process(pid<=0) from a corrupt/adversarial owner block: an unresolvable pid is dead and stealable.
        return True


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
    # POSIX write(2) may short-write; recursion writes the remaining tail at the advanced fd offset.
    written = os.write(fd, payload)
    match written >= len(payload):
        case True:
            return
        case False:
            _write_all(fd, payload[written:])


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

__all__ = ["ResourceBusyError", "_RESOURCE", "exclusive_lease", "fan_out", "leased", "run_check"]
