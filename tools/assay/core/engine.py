"""The sole process executor: one weave folds any ``Check`` into ``Result[Completed, Fault]``.

Generalizes ``tools/quality/process.py`` from a dotnet-only invoker to a ``Runner``-agnostic
executor. The public surface is exactly two sync façades — ``run_check`` and ``fan_out`` — each
calling ``anyio.run`` exactly once and weaving ``compose_spawn(retried()) ▷ compose(checked,
traced)`` on the inner ``_guarded`` spawn; ``logged`` lives on the rail, never the engine seam,
so retry correlation rides ``traced``. ``routed``/``scope``/``deadline`` flow as parameters,
never as ``Check`` fields. Faults never raise across the rail boundary: a non-zero exit is a
``Completed`` value (via ``receipt`` + ``from_returncode``), never a ``Fault``; only spawn
failure / timeout / lease contention take the ``Error`` channel, all at the single
``try/except`` boundary. The ``exclusive_lease`` / ``leased`` seam steals a stale lock
(dead-or-reused holder validated by ``psutil`` ``(pid, create_time)``) and raises
``ResourceBusyError`` → ``RailStatus.BUSY`` only on a *live* holder.

The executor carries inherent, knob-free diagnostics: at every ``Fault`` construction (``_guarded``
timeout/spawn-failure and the lease path) the current OTel span records the exception and a
``fault.resource_snapshot`` event (memory/cpu/fd counts from one ``psutil`` ``oneshot``) so a Fault
auto-carries the resource context an agent needs without flooding stdout — span events, never prints.
The lease path additionally stamps the holder ``(pid, create_time)`` + ``run_id`` as span attributes
and logs the before/after holder around a psutil stale-steal via ``structlog``. Magic numbers
(``stream_tail_bytes``/``stream_chunk_bytes``/``lease_drift_tolerance``/``scoped_verbs``) are read
from ``AssaySettings`` rather than module constants. The spawn ``cwd`` is materialized to a LOCAL
path string (``str(UPath(...).path)``): a child process roots in the real local FS, so the executor
is the local-exec boundary even when ``settings.root`` rides a ``UPath`` of any protocol.
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
from anyio import to_thread  # Runner.INPROC worker-thread executor; explicit submodule import (ty mis-resolves the anyio.to_thread attribute)
import asyncssh  # transparent REMOTE-EXEC backend: ssh:// exec_target routes the spawn over a verified connection
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
import psutil  # lease/fleet boundary; typed via the types-psutil stub (psutil ships no py.typed marker)
import structlog
from upath import UPath  # pathlib.Path drop-in: .path yields the LOCAL filesystem string for child cwd

from tools.assay.composition.settings import (  # unconditional for beartype @checked: resolves _guarded's forward-refs at runtime (PEP 649)
    ArtifactScope,
    AssaySettings,
)
from tools.assay.core.aspect import checked, compose, compose_spawn, retried, traced  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    ArtifactKind,
    Check,  # unconditional: @checked beartype resolves the _guarded(check: Check) forward-ref at call time
    Completed,  # unconditional: @checked beartype resolves the -> Result[Completed, Fault] forward-ref
    Fault,
    receipt,
    ResourceBusyError,
    Runner,
)
from tools.assay.core.routing import place, Routed  # Routed unconditional: @checked beartype resolves the _guarded(routed: Routed) forward-ref
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Callable, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, Process

    from tools.assay.core.aspect import Spawn  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------

# The shared woven spawn: the aspect stack folds checked ▷ traced over the retried-wrapped
# async ``_guarded``, yielding one async callable both façades await for a ``Result`` value.
type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]


# --- [MODELS] ---------------------------------------------------------------------------


class _LeaseOwner(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    """The fcntl-locked owner block: the wire identity of a held resource lease.

    Serialized into the lock file as ``msgspec`` JSON so a contending acquirer decodes the holder
    and validates ``(pid, create_time)`` liveness via ``_stale``. ``create_time`` (the psutil
    process-start timestamp) paired with ``pid`` survives PID reuse: a recycled pid carries a
    fresh ``create_time``, so the holder reads as stale and the lock is stolen, not stranded.
    """

    resource: str
    run_id: str
    pid: int
    create_time: float
    cwd: str = ""
    project: str = ""
    mode: str = "exclusive"
    started_at: float = 0.0
    target: str = ""  # settings.exec_target stamped at acquire: "" local, ssh://… remote — a fleet sees WHERE a lock holder ran


@dataclass(frozen=True, slots=True)
class _Held:
    """The value ``leased`` yields inside its ``with`` block: the resolved owner block."""

    owner: _LeaseOwner


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"  # OTel event name: psutil oneshot stamped at every Fault
_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)  # one-pass cached owner decode
_LOG = structlog.get_logger("assay.engine")  # lease stale-steal before/after holder rail
_TRACER = trace.get_tracer("assay.engine")  # D50: the bounded fan-out parent span the per-tool child spans nest under

# POSIX-only fcntl members bound at one boundary (ports process.py _posix_flag). ty's
# python-platform="all" flags every member as possibly-missing on the Windows leg; the lease seam is
# POSIX-only, the no-`if` rule bars sys.platform narrowing, and ty does not narrow `match`, so the
# floor is one irreducible suppression — the single tuple-bind carries it, never a per-member spread.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _splice(runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str]) -> tuple[str, ...]:
    """Inject dotnet ``--artifacts-path`` flags as ONE ``match`` arm.

    Only a ``Runner.DOTNET`` build-graph verb (``settings.scoped_verbs``) under a present ``scope``
    receives the artifact flags, spliced *before* any ``--`` argument boundary so the scope
    reaches MSBuild and the post-``--`` tail (test runner args, msbuild props) is untouched.
    Tool-driver verbs (``format``, ``tool``) never match, so artifact flags never reach a verb
    that rejects them. Every other shape passes ``command`` verbatim.
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
    (``uv``/``dotnet``/``pnpm``) is found; the overlay only *adds* scope keys, never strips them.
    """
    base: MutableMapping[str, str] = dict(os.environ)  # noqa: TID251  # marked spawn boundary: clone (never mutate) the host launch environment
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
    """Project one ``Check`` to its full argv: ``prefix ▷ spliced-body ▷ routed-tails``.

    ``_splice`` is the scope axis (dotnet flags as one arm); ``routing.place`` is the input axis
    (the sole argv-tail projector, fanned per ``Input``). The tails are flattened in order so a
    fan-of-N ``INCLUDE``/``PROJECT`` projection lays its groups end to end after the command body.
    """
    tool = check.tool
    body = _splice(tool.runner, tool.command, scope, settings.scoped_verbs)
    tails = place(routed, tool, settings=settings)
    return (*tool.runner.prefix, *body, *(part for tail in tails for part in tail))


async def _drain(stream: ByteReceiveStream | None, *, tail_cap: int, chunk: int) -> bytes:
    """Tee a process byte stream to a bounded tail (streaming ``Mode.stream`` rows).

    Folds successive ``receive`` chunks into a rolling bytestring capped at ``tail_cap``
    (``settings.stream_tail_bytes``) so a chatty child (dotnet restore, Stryker) cannot exhaust
    memory; ``chunk`` (``settings.stream_chunk_bytes``) bounds each ``receive``. The receive loop
    terminates on ``EndOfStream`` (the stream closes when the child's fd is reaped). A ``None``
    stream (the child inherited the parent fd) yields empty bytes. The recursion threads the
    accumulated tail so no mutable accumulator and no ``while`` loop is needed.
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
    """Read one bounded chunk, projecting ``EndOfStream`` to ``None`` (the recursion sentinel)."""
    try:
        return await stream.receive(chunk)
    except anyio.EndOfStream:
        return None


async def _reap(proc: Process) -> None:
    """Shielded teardown: kill then ``wait`` under ``CancelScope(shield=True)``.

    A ``fail_after`` / ``move_on_after`` deadline or a parent-group cancellation must never strand
    an orphaned dotnet/Stryker child: the shield guarantees the kill-and-wait completes even while
    the surrounding scope is cancelling, so the PID is reaped before the lease releases. Idempotent
    -- a child that already exited (``returncode is not None``) skips the ``kill`` and only awaits
    ``wait``, so there is no ``ProcessLookupError`` race to suppress.
    """
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
    """The one subprocess seam: ``match settings.exec_target`` dispatches LOCAL vs transparent REMOTE-EXEC.

    The ``case "":`` arm is the local hot path, preserved BYTE-IDENTICAL: ``anyio.open_process`` +
    dual bounded-tail tee + shielded ``_reap`` for ``streaming``, else ``anyio.run_process(check=False)``
    for buffered. When ``exec_target`` is ``""`` there is zero remote overhead — the dispatch is a
    single ``match`` arm folding to the unchanged anyio calls — ``_guarded`` calls this directly with
    ``streaming=check.tool.mode.stream``, so the engine's hot path is provably the original. The ``case target:`` arm parses
    ``ssh://[user@]host[:port]`` (``urllib.parse.urlsplit`` — the ecosystem URI parser) and runs the
    command over an ``asyncssh`` connection: buffered via ``conn.run`` (``encoding=None`` → raw
    ``bytes`` matching ``Completed.stdout``), streaming via ``conn.create_process`` teed to the same
    bounded ``_drain_reader`` tails and torn down shielded. Both arms seed the SAME ``receipt``.
    """
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
    """Run ``argv`` over ``asyncssh`` against ``ssh://[user@]host[:port]``: the REMOTE-EXEC arm.

    ``urlsplit`` projects the URI to ``(username, hostname, port)``; ``asyncssh.connect`` opens a
    verified connection (``known_hosts`` from ``settings.exec_known_hosts`` — ``None`` disables the
    host-key check for ephemeral CI fleet nodes). asyncssh has no native ``cwd``, so the command is
    a ``cd <cwd> && <argv>`` string with every token ``shlex.quote``-escaped; env is inlined into
    the command as ``KEY=value`` exports rather than passed to ``conn.run(env=…)`` because most
    ``sshd`` ``AcceptEnv`` whitelists reject arbitrary keys (the scope-isolation overlay would be
    silently dropped). ``encoding=None`` yields raw ``bytes`` so the ``Completed`` shape is identical
    to the local arm. Buffered → ``conn.run``; streaming → ``conn.create_process`` teed to bounded
    tails via ``_drain_reader`` and torn down shielded. ``exit_status`` (``None`` on a signal kill)
    folds to ``returncode`` via ``or 0`` exactly as the local streaming arm does.
    """
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
    """Project ``argv`` to one ``cd <cwd> && KEY=v … <argv>`` shell string, every token ``shlex.quote``-escaped.

    asyncssh exposes no working-directory or reliable env channel, so both ride the command line: the
    ``cd`` prefix roots the remote spawn and inlined ``KEY=value`` assignments survive an ``sshd``
    ``AcceptEnv`` whitelist that ``conn.run(env=…)`` would not. Quoting is mandatory — a path or env
    value with a space/metacharacter must not break the remote shell parse.
    """
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def _as_bytes(data: bytes | str | None) -> bytes:
    """Normalize an asyncssh output payload to ``bytes`` (``encoding=None`` yields ``bytes``; ``None`` is redirected, ``str`` the narrowing tail)."""
    match data:
        case bytes():
            return data
        case None:
            return b""
        case _:
            return data.encode()


async def _drain_reader(reader: asyncssh.SSHReader[bytes], *, tail_cap: int, chunk: int) -> bytes:
    """Tee an ``asyncssh.SSHReader`` to a bounded tail — the remote analogue of ``_drain`` (the local ``ByteReceiveStream`` tee).

    Folds successive ``read(chunk)`` results into a rolling bytestring capped at ``tail_cap`` so a
    chatty remote child cannot exhaust memory; an empty ``read`` is EOF (``SSHReader.read`` returns
    ``b""`` once the channel closes), terminating the recursion with no mutable accumulator.
    """

    async def _fold(acc: bytes) -> bytes:
        read = await reader.read(chunk)
        match read:
            case b"":
                return acc
            case _:
                return await _fold((acc + read)[-tail_cap:])

    return await _fold(b"")


def _snapshot() -> dict[str, int | float]:
    """One ``psutil`` ``oneshot`` projecting this process' live resource footprint as span-attr values.

    ``memory_info().rss`` (resident bytes), ``cpu_percent`` (since-last-call utilization), and
    ``num_fds`` (open descriptors) batch under a single cached lookup. The keys are OTel-style
    (``mem.rss_bytes``/``cpu.percent``/``proc.num_fds``) so a ``fault.resource_snapshot`` event reads
    uniformly across faults; an ``AccessDenied``/platform gap on ``num_fds`` degrades to ``-1`` rather
    than masking the fault it annotates.
    """
    proc = psutil.Process()
    with proc.oneshot():
        return {"mem.rss_bytes": proc.memory_info().rss, "cpu.percent": proc.cpu_percent(), "proc.num_fds": _num_fds(proc)}


def _num_fds(proc: psutil.Process) -> int:
    """Project ``num_fds`` to ``-1`` on the platforms/permissions that withhold it (never masks the fault)."""
    try:
        return int(proc.num_fds())  # ty: ignore[possibly-missing-attribute]  # POSIX-only psutil member; the AttributeError arm catches its absence
    except psutil.AccessDenied, NotImplementedError, AttributeError:
        return -1


def _diagnose(exc: BaseException) -> None:
    """Stamp the current OTel span with the exception + a ``fault.resource_snapshot`` event.

    Inherent, knob-free: called BEFORE every ``Fault`` is built (``_guarded`` timeout/spawn-failure
    and the lease ``OSError`` path) so a Fault auto-carries the agent-critical resource context. Uses
    span events (not stdout) so a flood of faults never pollutes the one-Envelope wire. A
    non-recording span (no exporter configured) absorbs both calls as no-ops, so the diagnostic is
    free when tracing is off.
    """
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=_snapshot())


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    """The inner async spawn the ``traced`` layer wraps with a ``tool.name`` child span.

    The SOLE ``try/except`` boundary in the executor: ``stream``/``capture`` are dispatched on
    ``Mode.stream``; a per-spawn ``fail_after`` budget (the absolute monotonic ``deadline`` minus
    now, else ``tool.timeout``) raises ``TimeoutError`` on elapse → ``Fault`` (``TIMEOUT``); an
    ``OSError`` (spawn failure after ``retried`` exhausts) → ``Fault`` (``FAULTED``). Every other
    outcome — including a non-zero process exit — rides the ``Ok`` channel as a ``Completed``, so
    ``_into`` never raises and the fan-out group always exits clean. ``_diagnose`` stamps the
    ``traced`` child span with the exception + a resource snapshot BEFORE each ``Fault`` is built.
    ``cwd`` is materialized to a LOCAL path string (``UPath(...).path``): the child process always
    roots in the real local FS, the local-exec boundary, regardless of the ``settings.root`` protocol.
    """
    argv = _argv(check, routed, settings=settings, scope=scope)
    budget = deadline - time.monotonic() if deadline is not None else check.tool.timeout
    cwd = str(UPath(check.cwd or settings.root).path)
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
    """Runner.INPROC executor: run the Tool's bound thunk on a worker thread under the caller's ``fail_after``.

    The thunk is a fully-bound sync callable the rail spliced onto the ``Tool`` via ``structs.replace``;
    ``anyio.to_thread.run_sync`` runs it off the event loop so an in-process tree-sitter parse rides the
    same per-spawn deadline, ``CapacityLimiter`` token, and ``traced`` child span as a subprocess spawn.
    A thunk-less INPROC row is a wiring defect surfaced as a non-zero ``Completed`` (the fold reads it as
    FAILED), never a raise across the spawn seam — ``abandon_on_cancel`` defaults False so a deadline
    cancels the awaiter (``TimeoutError`` → ``Fault(TIMEOUT)``) while the worker thread drains. The
    ``try/except`` is the INPROC resilience boundary: an untrusted thunk (a malformed tree-sitter query
    raising ``QueryError``, an ``importlib`` metadata-api lookup failure) folds to a FAILED ``Completed``
    carrying the exception text, NEVER an uncaught raise that escapes ``_guarded``'s spawn-fault catch and
    crashes the fan — so an agent's bad pattern is a clean FAILED row, not a traceback. ``Exception`` (not
    ``BaseException``) is caught so a ``CancelledError``/deadline still propagates to ``fail_after``.
    """
    match check.tool.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC tool carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check)
            except Exception as exc:  # noqa: BLE001  # INPROC resilience boundary: any thunk fault → FAILED receipt, never an uncaught raise across the fan
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    """The shared aspect-woven async callable: ``compose_spawn(retried()) ▷ compose(checked, traced)``.

    ``compose_spawn(retried())`` weaves the ``Spawn``-only ``retried`` layer onto ``_guarded`` (the
    engine seam, never ``logged``); ``compose(checked(), traced(...))`` lifts the ``checked`` shape
    boundary and the per-call ``traced`` child span keyed by ``tool.name`` with ``run_id`` as a span
    attribute (so retry correlation is bound here, at the engine seam, not in ``logged``). The woven
    value is one async callable returning ``Result[Completed, Fault]`` that both ``run_check`` (its
    own ``anyio.run``) and ``_into`` (awaited directly inside the one ``fan_out`` loop) consume
    verbatim, so there is never a second event loop.
    """
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
    """Execute one ``Check`` under exactly one event loop: the single-spawn sync façade.

    ``routed``/``scope``/``deadline`` arrive as parameters, never as ``Check`` fields. The woven
    ``_spawn`` runs the ``checked ▷ traced ▷ retried`` stack over ``_guarded``; a non-zero exit is
    an ``Ok(Completed)``, only a timeout / spawn failure rides ``Error(Fault)``. Calling this from
    inside a ``fan_out`` task would nest ``anyio.run`` and ``RuntimeError`` — the fan-out path
    awaits ``_spawn`` directly instead.
    """
    return anyio.run(_spawn(check, settings), check, settings, scope, routed, deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Capacity-bounded fan-out under one event loop: ordered, total-on-exit slots.

    One ``CapacityLimiter(_governed(settings))`` bounds concurrency (not task count — a task killed
    mid-flight auto-returns its token, unlike ``Semaphore``); ordered ``slots`` are pre-sized so the
    return is total and in input order. The ``start_soon`` loop is task-spawn glue carrying no fold
    state (the fold lives in ``model.fold`` downstream), so the imperative-loop ban does not bind it.
    A group-level ``move_on_after(deadline - now)`` silently cancels in-flight spawns on elapse
    (never raising, so ``__aexit__`` runs and slots stay total); any slot still ``None`` is back-filled
    with ``Fault(TIMEOUT)`` — best-effort partial results, never a stranded loop. ``_into`` cannot
    raise (``_guarded`` converts every spawn failure to a ``Result`` value), so the group exits clean.
    The task group runs inside one ``assay.fan_out`` parent span (``checks_total=N``, D50); each per-tool
    child span nests under it through anyio's context copy at ``start_soon``, so the batch causality is
    structural — an explicit OTel ``Link`` is redundant where the parent context already propagates.
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
    """Project an unfinished ``None`` slot to a ``Fault(TIMEOUT)``: keep the return tuple total."""
    match slot:
        case None:
            return Error(Fault((), status=RailStatus.TIMEOUT, message="deadline exceeded"))
        case Result() as r:
            return r


def _governed(settings: AssaySettings) -> int:
    """Fleet governor: cap ``max_checks`` by the logical CPU count to avoid oversubscription.

    ``psutil.cpu_count(logical=True)`` is the scheduling-relevant count (cores x threads); folding it
    against the configured ``max_checks`` keeps the bounded fan-out from spawning more concurrent
    children than the host can schedule, while still honoring a smaller operator-set cap.
    """
    return min(settings.max_checks, psutil.cpu_count(logical=True) or settings.max_checks)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _stale(owner: _LeaseOwner, tolerance: float) -> bool:
    """Liveness predicate: a dead-or-reused lock holder is stale and the lock may be stolen.

    ``Process.oneshot`` caches the pid lookup so ``is_running`` and ``create_time`` are one batched
    read; a holder is *live* only when the process runs AND its start timestamp matches the recorded
    one within ``tolerance`` (``settings.lease_drift_tolerance`` — the NTP-drift band; exact equality
    would mis-flag a clock-adjusted live holder). A ``NoSuchProcess`` / ``AccessDenied`` holder is
    unconditionally stale (the recorded pid is gone or unreadable).
    """
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():
            return not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except psutil.NoSuchProcess, psutil.AccessDenied:
        return True


def _claim(fd: int, resource: str, *, run_id: str, tolerance: float, target: str) -> _LeaseOwner | None:
    """Acquire the fcntl lock non-blocking, stealing a stale holder; return ``None`` only on live contention.

    A clean ``flock(LOCK_EX|LOCK_NB)`` wins immediately. A ``BlockingIOError`` decodes the held owner
    block (``_decode_owner`` projects an empty OR corrupt/partial lock to ``None``, a steal-able stale
    holder, so a holder that crashed mid-write never FAULTs a contending acquirer), stamps it as
    ``holder.*`` span attributes (``pid``/``create_time``/``run_id``), and tests liveness within
    ``tolerance``: a stale (or empty/corrupt) holder is logged then stolen with a second ``LOCK_NB``
    acquire — the before/after holder rail (``_LOG``) records who lost and who won the steal; a *live*
    holder yields ``None``, which the caller maps to ``ResourceBusyError`` (exit 5).
    ``target`` (``settings.exec_target``) is stamped onto the written owner block. Never blocks (LOCK_NB).
    """
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
    """Re-acquire a freed-stale lock and stamp ownership; a lost TOCTOU race (a second ``BlockingIOError``) yields ``None`` → BUSY, never FAULTED."""
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
    except BlockingIOError:
        return None
    won = _write_owner(fd, resource, run_id=run_id, target=target)
    _LOG.info("lease.stolen", resource=resource, run_id=run_id, lost=_holder(owner), won=_holder(won))
    return won


def _holder(owner: _LeaseOwner | None) -> dict[str, object]:
    """Project a holder block to its ``(pid, create_time, run_id)`` identity for the steal rail (``None`` → empty)."""
    match owner:
        case None:
            return {}
        case _:
            return {"pid": owner.pid, "create_time": owner.create_time, "run_id": owner.run_id}


def _stamp_holder(owner: _LeaseOwner | None, *, run_id: str) -> None:
    """Attach the contended holder ``(pid, create_time)`` + acquirer ``run_id`` to the current OTel span."""
    span = trace.get_current_span()
    span.set_attributes({"assay.run_id": run_id, **{f"holder.{k}": str(v) for k, v in _holder(owner).items()}})


def _decode_owner(held: bytes) -> _LeaseOwner | None:
    """Decode the held owner block; empty OR corrupt/partial lock bytes project to ``None`` — a steal-able stale holder.

    A truncated or non-JSON lock file (a holder that crashed mid-write, a manually corrupted lock) is NOT
    a live owner: a ``msgspec.DecodeError`` folds to ``None`` so ``_claim`` treats it as stale and steals
    it, rather than letting the decode raise across the lease seam and FAULT every contending acquirer.
    """
    match held:
        case b"":
            return None
        case _:
            try:
                return _DECODER.decode(held)
            except msgspec.DecodeError:
                return None


def _write_all(fd: int, payload: bytes) -> None:
    """Loop ``os.write`` until ``payload`` is fully written — a POSIX ``write(2)`` may short-write a large buffer.

    Recursion threads the unwritten tail; each partial write advances the fd position, so the remainder
    appends at the right offset with no ``lseek`` — the loop-free vehicle the module's folds already use.
    """
    written = os.write(fd, payload)
    match written >= len(payload):
        case True:
            return
        case False:
            _write_all(fd, payload[written:])


def _write_block(fd: int, block: _LeaseOwner) -> _LeaseOwner:
    """Truncate the lock file and write ``block`` as msgspec JSON via the looped ``os.write`` seam (short-write safe)."""
    os.ftruncate(fd, 0)
    os.lseek(fd, 0, os.SEEK_SET)
    _write_all(fd, msgspec.json.encode(block))
    return block


def _write_owner(fd: int, resource: str, *, run_id: str, target: str) -> _LeaseOwner:
    """Stamp this process as the holder: truncate the lock file and write a fresh ``_LeaseOwner`` block.

    ``target`` (``settings.exec_target``) records WHERE the holder runs ("" local, ssh://… remote) so a
    contending acquirer's steal log distinguishes a local holder from a remote one.
    """
    proc = psutil.Process(os.getpid())
    with proc.oneshot():
        block = _LeaseOwner(resource=resource, run_id=run_id, pid=proc.pid, create_time=proc.create_time(), started_at=time.time(), target=target)
    return _write_block(fd, block)


@contextlib.contextmanager
def exclusive_lease(
    resource: str, run_id: str, *, settings: AssaySettings, project: str = "", mode: str = "exclusive"
) -> Generator[Result[_Held, Fault]]:
    """Non-blocking fcntl lease with psutil staleness-steal.

    Never blocks, never raises across the rail seam. A dead-or-reused holder is stolen (``_stale``
    validates ``(pid, create_time)``); the owner block is the ``msgspec`` JSON identity written under
    the lock and truncated on release so a crashed holder leaves an empty, immediately-stealable file.
    Lock paths live under ``ArtifactKind.LOCKS`` (``build-<closure>.lock``/``mutation.lock``/
    ``bridge.lock``/``package-stage.lock``) via the settings path fold -- there is no ``*_lock`` property.

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
    """The one lease boundary every rail folds through: busy → ``Fault(BUSY)``, else run.

    Holds the ``exclusive_lease`` for the duration of ``action`` and threads its ``Result`` out
    unchanged; ``action`` is a thunk evaluated *only* under an ``Ok`` lease (the lease guards the
    resource window, the rail owns the work), receiving the resolved ``_Held`` owner. A *live*
    contention short-circuits to the lease's own ``Error(Fault(BUSY))`` (exit 5) without evaluating
    ``action``. ``OSError`` at the lock fd (the sole non-domain failure) → ``Fault(FAULTED)``.
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
