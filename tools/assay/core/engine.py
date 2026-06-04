"""The sole process executor: one weave folds any ``Check`` into ``Result[Completed, Fault]``.

Public surface is two sync façades (``run_check``, ``fan_out``), each calling ``anyio.run`` once
over the ``compose_spawn(retried()) ▷ compose(checked, traced)`` weave on ``_guarded``. A non-zero
exit is an ``Ok(Completed)`` value, never a ``Fault``; only spawn failure / timeout / lease
contention take the ``Error`` channel. The ``exclusive_lease`` / ``leased`` seam steals a stale
lock and raises ``ResourceBusyError`` → ``RailStatus.BUSY`` only on a *live* holder.
"""

import contextlib
from dataclasses import dataclass
import fcntl
import os
import shlex
import time
from typing import TYPE_CHECKING
from urllib.parse import urlsplit

import anyio
from anyio import to_thread  # explicit submodule import: ty mis-resolves the anyio.to_thread attribute
import asyncssh
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
import psutil
import structlog
from upath import UPath  # .path yields the LOCAL filesystem string for child cwd

from tools.assay.composition.settings import (  # unconditional for @checked beartype: resolves _guarded forward-refs at runtime
    ArtifactScope,
    AssaySettings,
)
from tools.assay.core.aspect import checked, compose, compose_spawn, retried, traced
from tools.assay.core.model import (
    ArtifactKind,
    Check,  # unconditional: @checked beartype resolves the _guarded(check: Check) forward-ref at call time
    Completed,  # unconditional: @checked beartype resolves the -> Result[Completed, Fault] forward-ref
    Fault,
    receipt,
    ResourceBusyError,
    Runner,
)
from tools.assay.core.routing import place, Routed  # Routed unconditional: @checked resolves the _guarded(routed: Routed) forward-ref
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, Process

    from tools.assay.core.aspect import Spawn


# --- [TYPES] ----------------------------------------------------------------------------

# The shared woven spawn: checked ▷ traced over the retried-wrapped async ``_guarded``.
type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]


# --- [MODELS] ---------------------------------------------------------------------------


class _LeaseOwner(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    """The fcntl-locked owner block: msgspec-JSON wire identity of a held resource lease."""

    resource: str
    run_id: str
    pid: int
    # (pid, create_time) survives PID reuse: a recycled pid carries a fresh create_time, so _stale
    # reads the holder as dead and the lock is stolen rather than stranded.
    create_time: float
    cwd: str = ""
    project: str = ""
    mode: str = "exclusive"
    started_at: float = 0.0
    target: str = ""  # settings.exec_target stamped at acquire: "" local, ssh://… remote — a fleet sees WHERE a holder ran


@dataclass(frozen=True, slots=True)
class _Held:
    """The value ``leased`` yields inside its ``with`` block: the resolved owner block."""

    owner: _LeaseOwner


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"
_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)
_LOG = structlog.get_logger("assay.engine")
_TRACER = trace.get_tracer("assay.engine")  # D50: the bounded fan-out parent span the per-tool child spans nest under

# POSIX-only fcntl members bound at one boundary: ty's python-platform="all" flags every member as
# possibly-missing on the Windows leg, and ty narrows neither sys.platform nor `match`, so the floor is
# one irreducible suppression carried by the single tuple-bind, never a per-member spread.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _splice(runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str]) -> tuple[str, ...]:
    """Inject dotnet ``--artifacts-path`` flags as ONE ``match`` arm.

    Only a ``Runner.DOTNET`` build-graph verb (``settings.scoped_verbs``) under a present ``scope``
    receives the artifact flags, spliced *before* any ``--`` argument boundary so the scope reaches
    MSBuild while the post-``--`` tail (test-runner args, msbuild props) stays untouched. Tool-driver
    verbs (``format``, ``tool``) are absent from ``scoped_verbs``, so artifact flags never reach a
    verb that rejects them. Every other shape passes ``command`` verbatim — the single arm is the
    whole scope axis, no per-verb branching.
    """
    match (runner, command, scope):
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in scoped_verbs:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *s.dotnet_flags, *command[cut:])
        case _:
            return command


def _overlay(scope: ArtifactScope | None) -> Mapping[str, str]:
    """Build the per-spawn ``env`` overlay: inherited environment plus scope isolation vars.

    Never mutates the host environment: clones the read-only parent ``os.environ`` mapping into a
    fresh dict at this marked spawn boundary, then folds the ``ArtifactScope.dotnet_env`` isolation
    overlay (``DOTNET_CLI_HOME`` + ``MSBUILDDISABLENODEREUSE``) on top when a scope is present.
    ``PATH`` and the rest of the host environment ride through unchanged so the resolved launcher
    (``uv``/``dotnet``/``pnpm``) is still found; the overlay only *adds* scope keys, never strips them.
    """
    base: MutableMapping[str, str] = dict(os.environ)  # noqa: TID251  # marked spawn boundary: clone (never mutate) the host launch environment
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
    # prefix ▷ spliced-body ▷ routed-tails: _splice is the scope axis, routing.place the input axis;
    # tails flatten in order so a fan-of-N INCLUDE/PROJECT lays its groups end to end after the body.
    tool = check.tool
    body = _splice(tool.runner, tool.command, scope, settings.scoped_verbs)
    tails = place(routed, tool, settings=settings)
    return (*tool.runner.prefix, *body, *(part for tail in tails for part in tail))


async def _drain(stream: ByteReceiveStream | None, *, tail_cap: int, chunk: int) -> bytes:
    """Tee a process byte stream to a bounded tail (streaming ``Mode.stream`` rows).

    Folds successive ``receive`` chunks into a rolling bytestring capped at ``tail_cap``
    (``settings.stream_tail_bytes``) so a chatty child (dotnet restore, Stryker) cannot exhaust
    memory. The fold recurses on the ``EndOfStream`` sentinel rather than looping — the no-``while``
    rule makes recursion the loop vehicle. A ``None`` stream (the child inherited the parent fd)
    short-circuits to empty bytes.
    """
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
    try:  # EndOfStream → None is the recursion sentinel
        return await stream.receive(chunk)
    except anyio.EndOfStream:
        return None


async def _reap(proc: Process) -> None:
    # Shield kill-and-wait so a deadline/parent cancellation never strands an orphan child: the PID
    # is reaped before the lease releases. Idempotent — an already-exited child skips the kill.
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
    # The one subprocess seam: exec_target dispatches LOCAL vs transparent REMOTE-EXEC. The case ""
    # arm is the local hot path, preserved BYTE-IDENTICAL with zero remote overhead; both arms seed
    # the SAME receipt.
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


async def _run_remote(
    argv: tuple[str, ...], target: str, *, cwd: str, env: Mapping[str, str], settings: AssaySettings, streaming: bool, tail_cap: int, chunk: int
) -> Completed:
    # REMOTE-EXEC arm: encoding=None yields raw bytes so Completed's shape is identical to the local
    # arm; exec_known_hosts=None disables the host-key check for ephemeral CI fleet nodes.
    parts = urlsplit(target)
    port = parts.port if parts.port is not None else ()
    command = _remote_command(argv, cwd=cwd, env=env)
    async with asyncssh.connect(parts.hostname or "", port, username=parts.username, known_hosts=settings.exec_known_hosts) as conn:
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
    # asyncssh exposes no cwd/reliable-env channel, so both ride the command line: inlined KEY=value
    # assignments survive an sshd AcceptEnv whitelist that conn.run(env=…) would not. Quoting is
    # mandatory — a space/metacharacter must not break the remote shell parse.
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
    # Remote analogue of _drain: roll a bounded tail capped at tail_cap; an empty read is EOF.
    async def _fold(acc: bytes) -> bytes:
        read = await reader.read(chunk)
        match read:
            case b"":
                return acc
            case _:
                return await _fold((acc + read)[-tail_cap:])

    return await _fold(b"")


def _snapshot() -> dict[str, int | float]:
    # One psutil oneshot batching rss/cpu/fd; OTel-style keys so fault.resource_snapshot reads
    # uniformly across faults.
    proc = psutil.Process()
    with proc.oneshot():
        return {"mem.rss_bytes": proc.memory_info().rss, "cpu.percent": proc.cpu_percent(), "proc.num_fds": _num_fds(proc)}


def _num_fds(proc: psutil.Process) -> int:
    try:  # -1 on platforms/permissions that withhold num_fds — never masks the fault it annotates
        return int(proc.num_fds())  # ty: ignore[possibly-missing-attribute]  # POSIX-only psutil member; the AttributeError arm catches its absence
    except psutil.AccessDenied, NotImplementedError, AttributeError:
        return -1


def _diagnose(exc: BaseException) -> None:
    # Called BEFORE every Fault is built so a Fault auto-carries resource context. Span events (not
    # stdout) keep the one-Envelope wire clean; a non-recording span absorbs both calls as no-ops.
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=_snapshot())


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    """The inner async spawn and the SOLE ``try/except`` boundary of the executor.

    The ``traced`` layer wraps this with a ``tool.name`` child span; ``retried`` wraps the spawn for
    transient-OSError recovery. A ``fail_after`` budget elapse becomes ``Fault(TIMEOUT)``; an
    ``OSError`` (spawn failure after ``retried`` exhausts) becomes ``Fault(FAULTED)``. Both Fault
    paths run ``_diagnose`` first so the Fault auto-carries a resource snapshot. Every other outcome —
    including a non-zero process exit — rides the ``Ok`` channel as a ``Completed`` (via ``receipt`` +
    ``from_returncode``), so ``_into`` never raises and the fan-out exits clean.
    """
    argv = _argv(check, routed, settings=settings, scope=scope)
    budget = deadline - time.monotonic() if deadline is not None else check.tool.timeout
    cwd = str(UPath(check.cwd or settings.root).path)  # LOCAL path: the child always roots in the real local FS regardless of settings.root protocol
    env = _overlay(scope)
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
    except (
        OSError,
        asyncssh.Error,
    ) as exc:  # asyncssh.Error descends from Exception (not OSError): remote-exec failures must take the Fault rail, not escape the group
        _diagnose(exc)
        return Error(Fault(argv, status=RailStatus.FAULTED, message=str(exc)))


async def _inproc(check: Check) -> Completed:
    # Runner.INPROC: run the Tool's bound thunk on a worker thread under the caller's fail_after, so an
    # in-process parse rides the same deadline/token/child-span as a subprocess. A thunk-less row is a
    # wiring defect surfaced as a non-zero Completed (FAILED), never a raise across the spawn seam.
    match check.tool.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC tool carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check)
            except Exception as exc:  # noqa: BLE001  # INPROC resilience boundary: any thunk fault → FAILED receipt, never an uncaught raise across the fan
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    # compose_spawn(retried()) ▷ compose(checked, traced) over _guarded: retry correlation is bound
    # here at the engine seam (run_id span attr), never in the forbidden-on-the-engine logged layer.
    span = traced(
        span=check.tool.name,
        attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name},
        agent=lambda *_a, **_k: settings.agent_context,  # {run.id, agent.task.id}: agent_task_id into _on_retry + child spans
    )
    # mypy eagerly binds ``retried()``'s **P/T to Never (a no-arg generic factory offers no argument to
    # solve from), so the woven spawn types as ``(*Never) -> Coroutine[None, None, Never]``; the two-step
    # form is ty-clean (ty specializes at the ``_guarded`` apply) but mypy cannot, so this lone [arg-type]
    # is the floor — an explicit ``compose_spawn(layer, fn)`` only relocates it (ty then rejects instead).
    layered: Spawn[..., Result[Completed, Fault]] = compose_spawn(retried())(_guarded)  # type: ignore[arg-type]  # mypy-only: retried() infers Never; _guarded binds **P/T at the apply
    # ``compose`` is Hom-typed (sync ``Result`` return); threading the async ``_Woven`` through it is the one
    # irreducible re-scope — the same @wraps-induced Hom view ``logged``/``traced`` carry — because ``Hom``'s
    # ``Result[T, Fault]`` return can never unify with ``_Woven``'s ``Coroutine``. One suppression per checker.
    weave: Callable[[_Woven], _Woven] = compose(checked(), span)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return weave(layered)


def run_check(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Execute one ``Check`` under exactly one event loop: the single-spawn sync façade."""
    # Calling this inside a fan_out task would nest anyio.run and RuntimeError; the fan path awaits _spawn directly.
    return anyio.run(_spawn(check, settings), check, settings, scope, routed, deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Capacity-bounded fan-out under one event loop: ordered, total-on-exit slots.

    ``CapacityLimiter`` bounds concurrency (a mid-flight kill auto-returns its token, unlike
    ``Semaphore``); pre-sized ``slots`` keep the return total and in input order. A group-level
    ``move_on_after`` silently cancels in-flight spawns on elapse (never raises, so ``__aexit__`` runs
    and slots stay total) and any ``None`` slot back-fills to ``Fault(TIMEOUT)``. Per-tool child spans
    nest under the one ``assay.fan_out`` parent (D50) via anyio's context copy at ``start_soon``.
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
    match slot:  # an unfinished None slot → Fault(TIMEOUT) so the return tuple stays total
        case None:
            return Error(Fault((), status=RailStatus.TIMEOUT, message="deadline exceeded"))
        case Result() as r:
            return r


def _governed(settings: AssaySettings) -> int:
    # Cap max_checks by the logical CPU count (cores x threads) to avoid oversubscription.
    return min(settings.max_checks, psutil.cpu_count(logical=True) or settings.max_checks)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _stale(owner: _LeaseOwner, tolerance: float) -> bool:
    # A holder is LIVE only when running AND its start timestamp matches the recorded one within
    # tolerance (the NTP-drift band; exact equality would mis-flag a clock-adjusted live holder); a
    # NoSuchProcess/AccessDenied holder is unconditionally stale.
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():
            return not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except psutil.NoSuchProcess, psutil.AccessDenied:
        return True


def _claim(fd: int, resource: str, *, run_id: str, tolerance: float, target: str) -> _LeaseOwner | None:
    # Acquire the fcntl lock non-blocking (LOCK_NB never blocks), stealing a stale holder. A clean
    # flock wins; a BlockingIOError decodes the held owner (an empty OR corrupt/partial lock decodes to
    # None — steal-able — so a holder that crashed mid-write never FAULTs a contender), and a live
    # holder yields None which the caller maps to ResourceBusyError (exit 5).
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id, target=target)
    except BlockingIOError:
        held = os.read(fd, 4096)
        owner = _decode_owner(held)
        _stamp_holder(owner, run_id=run_id)
        match owner is not None and not _stale(owner, tolerance):
            case True:
                return None
            case False:
                _LOG.warning("lease.steal", resource=resource, run_id=run_id, lost=_holder(owner))
                return _steal(fd, resource, run_id=run_id, target=target, owner=owner)


def _steal(fd: int, resource: str, *, run_id: str, target: str, owner: _LeaseOwner | None) -> _LeaseOwner | None:
    # A lost TOCTOU race (a second BlockingIOError) yields None → BUSY, never FAULTED.
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
    # Empty OR corrupt/partial lock bytes → None (a steal-able stale holder): a DecodeError must fold
    # here, not raise across the lease seam and FAULT every contending acquirer.
    match held:
        case b"":
            return None
        case _:
            try:
                return _DECODER.decode(held)
            except msgspec.DecodeError:
                return None


def _write_all(fd: int, payload: bytes) -> None:
    # POSIX write(2) may short-write a large buffer; recursion threads the unwritten tail and each
    # partial write advances the fd position, so the remainder appends at the right offset with no lseek.
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
    # target records WHERE the holder runs ("" local, ssh://… remote) so a contender's steal log can
    # distinguish a local holder from a remote one.
    proc = psutil.Process(os.getpid())
    with proc.oneshot():
        block = _LeaseOwner(resource=resource, run_id=run_id, pid=proc.pid, create_time=proc.create_time(), started_at=time.time(), target=target)
    return _write_block(fd, block)


@contextlib.contextmanager
def exclusive_lease(
    resource: str, run_id: str, *, settings: AssaySettings, project: str = "", mode: str = "exclusive"
) -> Generator[Result[_Held, Fault]]:
    """Non-blocking fcntl lease with psutil staleness-steal: never blocks, never raises across the rail.

    A dead-or-reused holder is stolen (``_stale`` validates ``(pid, create_time)``); the owner block is
    truncated on release so a crashed holder leaves an empty, immediately-stealable file.

    Yields:
        ``Ok(_Held(owner))`` on acquisition; ``Error(Fault(BUSY))`` on *live* contention (exit 5).
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
            case None:  # never acquired the flock (live contention / claim error): release our fd only — never truncate the live holder's block
                os.close(fd)
            case _:
                os.ftruncate(fd, 0)
                _FLOCK(fd, _LOCK_UN)
                os.close(fd)


def leased[T](
    resource: str, action: Callable[[_Held], Result[T, Fault]], *, settings: AssaySettings, run_id: str, project: str = "", mode: str = "exclusive"
) -> Result[T, Fault]:
    """The one lease boundary every rail folds through: busy → ``Fault(BUSY)``, else run ``action``.

    ``action`` is evaluated *only* under an ``Ok`` lease; live contention short-circuits to
    ``Error(Fault(BUSY))`` (exit 5) without evaluating it, and an ``OSError`` at the lock fd → ``Fault(FAULTED)``.
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

__all__ = ["ResourceBusyError", "exclusive_lease", "fan_out", "leased", "run_check"]
