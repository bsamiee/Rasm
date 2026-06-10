"""Execution engine for the assay toolchain.

Owns the full check execution surface: local subprocess, SSH remote, in-process thunk,
and POSIX flock lease runners. Entry points are run_check, run_check_async, and fan_out
for single and concurrent check dispatch; governed_concurrency folds CPU, DOTNET, and
mutation ceilings into one concurrency limit. discover and discover_async serve
read-only discovery commands outside the check lifecycle. exclusive_lease and leased
provide the non-blocking POSIX lease protocol used by the quality gate bus.
"""

from collections.abc import Callable
import contextlib
from contextvars import ContextVar
from dataclasses import dataclass, replace
import fcntl
from functools import cache
import os
from pathlib import Path
import shlex
import shutil
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING
from urllib.parse import urlsplit

import anyio
from anyio import to_thread  # ty mis-resolves anyio.to_thread without an explicit submodule import
from expression import Error, Ok, Result
import msgspec
from opentelemetry import propagate, trace
import psutil
import stamina
import structlog
from upath import UPath

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # unconditional: beartype resolves forward refs at import time
from tools.assay.core.aspect import (
    _CHECKED_LAYER,  # noqa: PLC2701  # co-owned internal seam: aspect and engine share this layer without a public surface
    compose,
    ring_recent,
    traced,
)
from tools.assay.core.model import (  # noqa: TC001  # beartype resolves the Tool annotation on _apphost at runtime under PEP 649
    Artifact,
    ArtifactKind,
    Check,
    Completed,
    Fault,
    Mode,
    receipt,
    Runner,
    Tool,
)
from tools.assay.core.routing import place, Routed
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Awaitable, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, ObjectReceiveStream, ObjectSendStream, Process
    import asyncssh


# --- [TYPES] ----------------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]
type ByteRecv = Callable[[], Coroutine[None, None, bytes | None]]


class WriteSink(Protocol):
    """Structural byte sink: a writable target receiving stream chunks during a drain."""

    def write(self, payload: bytes) -> object:
        """Receive one raw byte chunk; return value is ignored by the drain pump."""
        ...


@runtime_checkable
class _Nullary(Protocol):
    def __call__(self) -> float | int | str: ...


@runtime_checkable
class _WriteContext(Protocol):
    def __enter__(self) -> WriteSink: ...

    def __exit__(self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: object) -> object: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"
_RING_SNAPSHOT: str = "assay.ring"
_MEM_PRESSURE_PERCENT: float = 90.0
_SSH_CONNECT_TIMEOUT: float = 15.0
_SSH_SIGNAL_STATUS: int = 255
_DOTNET_PROBE_TIMEOUT_S: float = 15.0
_DOTNET_SHARED_RUNTIME: tuple[str, str] = ("shared", "Microsoft.NETCore.App")
_RETRY_MIN_REMAINING: float = 0.05
# A zombie or dead holder can never release its flock; both statuses are stale regardless of create-time match.
_DEAD_STATUSES: frozenset[str] = frozenset({psutil.STATUS_DEAD, psutil.STATUS_ZOMBIE})
# Bound at import so a patched psutil module double cannot skew the verdict comparison.
_DISK_WAIT_STATUS: str = psutil.STATUS_DISK_SLEEP
# Fixed stall thresholds: a silent child is diagnosed once after 30s over a 5s sample window; predictability beats adaptive tuning.
_STALL_AFTER_S: float = 30.0
_STALL_SAMPLE_S: float = 5.0
_STALL_CPU_PCT: float = 25.0  # of one core across the sample window
_STALL_CTX_RATE_HZ: float = 100.0  # involuntary context switches per second across the tree

# ty cannot narrow this module to POSIX, so bind the POSIX-only members once to avoid repeated attribute ignores.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]


# --- [MODELS] ---------------------------------------------------------------------------


class _LeaseOwner(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    resource: str
    run_id: str
    pid: int
    create_time: float  # (pid, create_time) pair survives PID reuse; a recycled pid carries a fresh create_time
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
class _StallSample:
    cpu_s: float
    invol: float
    status: str
    procs: int


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
# Fault-time snapshots cross the anyio.run boundary; ContextVar threads the snapshot without polluting call signatures.
_RESOURCE: ContextVar[tuple[tuple[str, float], ...]] = ContextVar("assay_resource", default=())
# Fan batches share one to_thread limiter so the governed cap binds the whole batch rather than each check separately.
_FAN_LIMITER: ContextVar[anyio.CapacityLimiter | None] = ContextVar("assay_fan_limiter", default=None)


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.engine")
_TRACER = trace.get_tracer("assay.engine")


# --- [TABLES] ---------------------------------------------------------------------------

_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)


# --- [OPERATIONS] -----------------------------------------------------------------------


def splice_command(
    runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str], mode: Mode
) -> tuple[str, ...]:
    """Inject scope flags into a DOTNET build-graph command before any ``--`` argument separator.

    Scope injection is skipped when the runner is not DOTNET, the verb is not in scoped_verbs,
    scope is absent, or mode is QUERY or LIST.

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
    base: MutableMapping[str, str] = dict(os.environ)  # noqa: TID251  # clones the host environment at the spawn boundary
    base.update(settings.python_tool_env)
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> tuple[str, ...]:
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


@cache
def _dotnet_root() -> str | None:
    # nix exports a DOTNET_ROOT wrapper lacking shared/Microsoft.NETCore.App; resolve the real runtime root once per
    # process (the `dotnet --list-runtimes` probe is process-stable); tests reset via cache_clear.
    def valid(path: str) -> str | None:
        root = Path(path).expanduser()
        return str(root) if path and Path(root, *_DOTNET_SHARED_RUNTIME).is_dir() else None

    def runtime_root(line: str) -> str | None:
        match line.rsplit("[", maxsplit=1):
            case [_, raw] if raw.endswith("]"):
                return valid(str(Path(raw[:-1]).parent.parent))
            case _:
                return None

    def from_env() -> str | None:
        return valid(os.environ.get("DOTNET_ROOT", ""))  # noqa: TID251  # the host override is the probe's first-precedence source

    def from_runtimes() -> str | None:
        listed = discover(("dotnet", "--list-runtimes"), root=Path.cwd(), timeout=_DOTNET_PROBE_TIMEOUT_S)
        lines = listed.map(lambda out: out.decode(errors="replace").splitlines()).default_value([])
        return next((root for line in lines for root in (runtime_root(line),) if root is not None), None)

    def from_muxer() -> str | None:
        muxer = shutil.which("dotnet")
        return valid(str(Path(muxer).resolve().parent)) if muxer else None

    return next((root for probe in (from_env, from_runtimes, from_muxer) for root in (probe(),) if root is not None), None)


def _apphost(tool: Tool, env: Mapping[str, str]) -> Mapping[str, str]:
    # `dotnet tool run` spawns apphost-deployed tools (ilspycmd, stryker) that resolve the runtime from DOTNET_ROOT;
    # SDK verbs self-locate via the muxer, so only ("tool", "run") heads receive the resolved-root overlay.
    match (tool.runner, tool.command[:2]):
        case (Runner.DOTNET, ("tool", "run")):
            match _dotnet_root():
                case str(root):
                    return {**env, "DOTNET_ROOT": root, "DOTNET_MULTILEVEL_LOOKUP": "0"}
                case None:
                    return {key: value for key, value in env.items() if key != "DOTNET_ROOT"}
        case _:
            return env


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
    if isinstance(dst, ValueError):  # pragma: no cover  # src passed identical containment on the same rel; dst cannot escape work
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


async def drain_stream(recv: ByteRecv, *, tail_cap: int, sink: WriteSink | None = None, path: str = "") -> Captured:
    """Pump an async byte source to EOF, tee-ing to an optional sink and retaining a bounded tail.

    Line count includes a synthetic final line when the stream ends without a trailing newline,
    so the count matches the number of logical lines rather than raw newline occurrences.

    Args:
        recv: Async read primitive returning the next chunk, or ``None`` at EOF.
        tail_cap: Maximum number of trailing bytes retained in the captured tail.
        sink: Optional byte sink receiving every chunk as it is read.
        path: Artifact path recorded on the resulting capture.

    Returns:
        Capture of the tail window, artifact path, total byte size, and line count.
    """
    tail, total, lines, last = b"", 0, 0, b""
    while (read := await recv()) is not None:
        _write_sink(sink, read)
        tail = (tail + read)[-tail_cap:]
        total += len(read)
        lines += read.count(b"\n")
        last = read[-1:] or last  # an empty chunk must not clobber the newline-terminus probe
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
    # asyncssh signals EOF with b"", not EndOfStream; map to None for pump uniformity.
    async def _recv() -> bytes | None:
        return (await reader.read(chunk)) or None

    return _recv


def _touched(recv: ByteRecv, last_output: list[float]) -> ByteRecv:
    # Every received chunk (and EOF) re-arms the stall clock; silence is measured from the last byte, not from spawn.
    async def _recv() -> bytes | None:
        chunk = await recv()
        last_output[0] = time.monotonic()
        return chunk

    return _recv


def _write_sink(sink: WriteSink | None, payload: bytes) -> None:
    match sink:
        case None:
            pass
        case _:
            sink.write(payload)


async def _drain_pair(plan: _ExecPlan, out: ByteRecv, err: ByteRecv, wait: Callable[[], Awaitable[object]]) -> dict[str, Captured]:
    # Shared local/remote streaming pump: tee both channels into bounded tails (+ optional artifact sinks), then await child exit inside the group.
    streams: dict[str, Captured] = {}
    async with anyio.create_task_group() as tg:

        async def _tee(name: str, recv: ByteRecv) -> None:
            path, handle = _stream_writer(plan, name)
            match handle:
                case None:
                    streams[name] = await drain_stream(recv, tail_cap=plan.tail_cap, path=path)
                case _:
                    with handle as sink:
                        streams[name] = await drain_stream(recv, tail_cap=plan.tail_cap, sink=sink, path=path)

        tg.start_soon(_tee, "out", out)
        tg.start_soon(_tee, "err", err)
        await wait()
    return streams


async def _reap(proc: Process, limiter: anyio.CapacityLimiter | None = None) -> None:
    # Shielded so cancellation cannot strand a child process past lease release.
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


def _stall_sample(pid: int) -> _StallSample:
    # Tree-aggregated: dotnet builds fan out compiler children, so the root process alone under-reports activity.
    def _tree() -> tuple[psutil.Process, ...]:
        root = psutil.Process(pid)
        return (root, *root.children(recursive=True))

    def _cpu(proc: psutil.Process) -> float:
        def _read() -> float:
            times = proc.cpu_times()
            return float(times.user) + float(times.system)

        return _safe_call(_read, 0.0)

    def _invol(proc: psutil.Process) -> float:
        return _safe_call(lambda: float(proc.num_ctx_switches().involuntary), 0.0)

    tree: tuple[psutil.Process, ...] = _safe_call(_tree, ())
    return _StallSample(
        cpu_s=sum(_cpu(proc) for proc in tree),
        invol=sum(_invol(proc) for proc in tree),
        status=_safe_call(tree[0].status, "") if tree else "",
        procs=len(tree),
    )


def _stall_verdict(first: _StallSample, second: _StallSample) -> str:
    # Triage for agents watching a silent child: which resource the process tree actually consumed across the sample window.
    cpu_pct = max(0.0, second.cpu_s - first.cpu_s) * 100.0 / max(_STALL_SAMPLE_S, 1e-6)
    invol_rate = max(0.0, second.invol - first.invol) / max(_STALL_SAMPLE_S, 1e-6)
    match (cpu_pct, second.status, invol_rate):
        case (pct, _, _) if pct >= _STALL_CPU_PCT:
            return f"cpu-bound ({pct:.0f}% of one core, {second.procs} procs)"
        case (_, status, _) if status == _DISK_WAIT_STATUS:
            return "disk-wait"
        case (_, _, rate) if rate >= _STALL_CTX_RATE_HZ:
            return "scheduler-contention"
        case _:
            return "io-or-lock-wait"


async def _stall_monitor(pid: int, last_output: list[float], notes: list[str]) -> None:
    # Armed only after _STALL_AFTER_S of child silence; diagnoses once and exits, so the receipt carries at most one bounded note.
    while not notes:
        await anyio.sleep(_STALL_SAMPLE_S)
        match time.monotonic() - last_output[0] >= _STALL_AFTER_S:
            case False:
                pass
            case True:
                first = await to_thread.run_sync(_stall_sample, pid)
                await anyio.sleep(_STALL_SAMPLE_S)
                second = await to_thread.run_sync(_stall_sample, pid)
                silent_s = time.monotonic() - last_output[0]
                verdict = _stall_verdict(first, second)
                notes.append(f"proc.stall silent={silent_s:.0f}s {verdict}"[:256])
                _LOG.warning("proc.stall", pid=pid, silent_s=round(silent_s, 1), verdict=verdict, procs=second.procs)
                trace.get_current_span().add_event("proc.stall", attributes={"proc.pid": pid, "proc.silent_s": silent_s, "proc.verdict": verdict})


async def _run_process_backend(plan: _ExecPlan) -> Completed:
    match plan.settings.exec_target:
        case "":
            match plan.streaming:
                case True:
                    proc = await anyio.open_process(list(plan.argv), cwd=plan.cwd, env=plan.env)
                    try:
                        last_output, stall = [time.monotonic()], list[str]()
                        async with anyio.create_task_group() as tg:
                            tg.start_soon(_stall_monitor, proc.pid, last_output, stall)
                            streams = await _drain_pair(
                                plan,
                                _touched(_recv_anyio(proc.stdout, plan.chunk), last_output),
                                _touched(_recv_anyio(proc.stderr, plan.chunk), last_output),
                                proc.wait,
                            )
                            tg.cancel_scope.cancel()
                        return receipt(
                            plan.argv,
                            proc.returncode or 0,
                            stdout=streams.get("out", Captured()).tail,
                            stderr=streams.get("err", Captured()).tail,
                            notes=tuple(stall),
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
    import asyncssh  # noqa: PLC0415  # lazy: ~83ms cold-start cost; defer past import time

    command = remote_command(plan.argv, cwd=plan.cwd, env=plan.env)
    async with _ssh_connection(target, plan.settings) as conn:
        match plan.streaming:
            case True:
                # No stall telemetry on this branch: psutil cannot inspect remote pids across the SSH boundary.
                proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
                try:
                    streams = await _drain_pair(plan, _recv_ssh(proc.stdout, plan.chunk), _recv_ssh(proc.stderr, plan.chunk), proc.wait)
                    code, notes = ssh_outcome(proc.exit_status, getattr(proc, "exit_signal", None))
                    return receipt(
                        plan.argv,
                        code,
                        stdout=streams.get("out", Captured()).tail,
                        stderr=streams.get("err", Captured()).tail,
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
                if cached is None or cached.is_closed():
                    cached = pooled.conns[target] = await _connect_once(target, settings)
            yield cached


@contextlib.asynccontextmanager
async def _connect(target: str, settings: AssaySettings) -> AsyncIterator[asyncssh.SSHClientConnection]:
    conn = await _connect_once(target, settings)
    try:
        yield conn
    finally:
        conn.close()
        await conn.wait_closed()


async def _connect_once(target: str, settings: AssaySettings) -> asyncssh.SSHClientConnection:
    import asyncssh  # noqa: PLC0415  # lazy: ~83ms cold-start cost; defer past import time

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
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def ssh_outcome(status: int | None, signal: object | None) -> tuple[int, tuple[str, ...]]:
    """Resolve a remote exit status and optional signal into a numeric code plus receipt notes.

    Returns:
        The integer exit code (or synthetic 255 for a signalled kill) and any signal-name notes.
    """
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
    # NotImplementedError is psutil's off-POSIX signal for per-process probes such as num_fds.
    try:
        return fn()
    except psutil.Error, TypeError, ValueError, AttributeError, OSError, NotImplementedError:
        return default


def _snapshot() -> dict[str, float]:
    # oneshot batches proc syscalls; _children is called separately to avoid recursive oneshot nesting.
    proc = psutil.Process()
    with proc.oneshot():
        info = proc.memory_info()
        base = {
            "mem.rss_bytes": float(info.rss),
            "mem.vms_bytes": _safe_call(lambda: float(info.vms), -1.0),
            "mem.uss_bytes": _safe_call(lambda: float(proc.memory_full_info().uss), -1.0),
            "mem.percent_rss": _safe_call(lambda: float(info.rss) * 100.0 / max(float(psutil.virtual_memory().total), 1.0), -1.0),
            "cpu.percent": _safe_call(lambda: float(proc.cpu_percent()) if isinstance(proc.cpu_percent, _Nullary) else -1.0, -1.0),
            "proc.num_fds": _safe_call(lambda: float(proc.num_fds()), -1.0),  # ty: ignore[possibly-missing-attribute]  # POSIX-only probe
            "proc.num_threads": _safe_call(lambda: float(proc.num_threads()) if isinstance(proc.num_threads, _Nullary) else -1.0, -1.0),
        }
    return {**base, **_sys_pressure()}


def _sys_pressure() -> dict[str, float]:
    # Memory and load probes degrade independently: each arm contributes its keys only when its own source calls succeed.
    def _mem() -> dict[str, float]:
        mem, swap = psutil.virtual_memory(), psutil.swap_memory()
        return {
            "sys.mem_available_bytes": _safe_call(lambda: float(mem.available), -1.0),
            "sys.mem_percent": _safe_call(lambda: float(mem.percent), -1.0),
            "sys.swap_percent": _safe_call(lambda: float(swap.percent), -1.0),
        }

    def _load() -> dict[str, float]:
        load1 = os.getloadavg()[0]  # ty: ignore[possibly-missing-attribute]  # POSIX-only; absence degrades through _safe_call
        return {"sys.load1_percent": load1 * 100.0 / max(float(psutil.cpu_count(logical=True) or 1), 1.0)}

    return {**_safe_call(_mem, {}), **_safe_call(_load, {})}


def _children(proc: psutil.Process) -> dict[str, float]:
    def _rss(child: psutil.Process) -> float:
        return _safe_call(lambda: float(child.memory_info().rss), 0.0)

    try:
        kids = tuple(proc.children(recursive=True))
        return {"proc.children": float(len(kids)), "proc.children_rss_bytes": float(sum(_rss(child) for child in kids))}
    except psutil.Error, TypeError, ValueError, AttributeError:
        return {}


def _diagnose(exc: BaseException) -> None:
    # Children walk on the fault path only; keeps _snapshot free of recursive oneshot nesting.
    snap = {**_snapshot(), **_children(psutil.Process())}
    _RESOURCE.set(tuple(snap.items()))
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=snap)
    span.add_event(_RING_SNAPSHOT, attributes={"events": ring_recent()})


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    import asyncssh  # noqa: PLC0415  # lazy: must bind before the try frame whose except evaluates asyncssh.Error (not an OSError subclass)

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
        # The probe behind _apphost is sync + cached; the thread hop keeps the first `dotnet --list-runtimes` off the loop.
        env = await to_thread.run_sync(_apphost, check.tool, _overlay(settings, scope))
        propagate.inject(env)
        trace.get_current_span().set_attribute("exec.target", settings.exec_target)
        done = await _execute_retrying(
            check,
            settings,
            scope,
            argv=argv,
            cwd=cwd,
            env=env,
            thread_limiter=_FAN_LIMITER.get() or anyio.CapacityLimiter(governed_concurrency(settings, (check,))),
            deadline=bound,
            attempts=attempts,
        )
        return Ok(msgspec.structs.replace(done, duration_ms=(time.monotonic() - t0) * 1000.0))
    except (TimeoutError, FileNotFoundError, OSError, ValueError, asyncssh.Error) as exc:
        # Spawn-fault fold: deadline → TIMEOUT; missing capability (never retried) → UNSUPPORTED; NUL-in-argv / transport → FAULTED.
        _diagnose(exc)
        match exc:
            case TimeoutError():
                return Error(Fault(argv, status=RailStatus.TIMEOUT, message=_stamped("deadline exceeded")))
            case FileNotFoundError():
                return Error(Fault(argv, status=RailStatus.UNSUPPORTED, message=_stamped(str(exc))))
            case _:
                return Error(Fault(argv, status=RailStatus.FAULTED, message=_stamped(str(exc))))


async def _execute_retrying(  # noqa: PLR0913  # all params are load-bearing across the retry body; no grouping reduces the count
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
    # list[int] cell carries attempt count across stamina's re-raise boundary for fault stamping in _guarded.
    done: Completed | None = None
    async for attempt in stamina.retry_context(on=retry_predicate(check, deadline), attempts=3, timeout=_retry_timeout(deadline)):
        attempts[0] = attempt.num
        with attempt:
            with anyio.fail_after(_remaining(deadline)):
                done = await _execute(check, settings, scope, argv=argv, cwd=cwd, env=env, thread_limiter=thread_limiter)
    match done:
        case None:  # pragma: no cover  # stamina always re-raises on exhaustion; None only if that contract breaks
            raise RuntimeError("stamina exhausted without raising — invariant violated")
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
    import asyncssh  # noqa: PLC0415  # lazy: classify's match arm references asyncssh.Error; must bind before the closure captures it

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
    match check.tool.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC tool carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check, limiter=limiter)
            except Exception as exc:  # noqa: BLE001  # INPROC resilience: any thunk fault becomes a FAILED receipt; never propagates across the fan
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    # compose wraps _guarded at call time so each invocation carries a fresh span; omitting the logged layer is intentional.
    # compose is Hom-typed; no variance-safe overload for async _Woven — suppression is load-bearing.
    span = traced(
        span=check.tool.name,
        attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name},
        agent=lambda *_a, **_k: settings.agent_context,
    )
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

    ``deadline`` is an absolute ``time.monotonic()`` ceiling shared across all checks; ``None``
    means no shared budget, deferring to each check's per-tool timeout. Checks that expire
    before dispatch yield a TIMEOUT fault in their result slot.

    Returns:
        One completed-or-fault result per input check.
    """

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        limit = governed_concurrency(settings, checks)
        results: dict[int, Result[Completed, Fault]] = {}
        token = _FAN_LIMITER.set(anyio.CapacityLimiter(limit))
        try:
            with _TRACER.start_as_current_span("assay.fan_out") as parent:
                parent.set_attribute("assay.checks_total", len(checks))
                parent.set_attribute("assay.checks_concurrency", limit)
                async with _pooled_ssh():
                    results.update(await _fan_schedule(checks, settings=settings, scope=scope, routed=routed, deadline=deadline, limit=limit))
        finally:
            _FAN_LIMITER.reset(token)
        return tuple(_total(results.get(i)) for i in range(len(checks)))

    return anyio.run(_run)


async def _fan_schedule(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None, limit: int
) -> dict[int, Result[Completed, Fault]]:
    # Buffer capacity equals the check count so the producer never blocks behind a stalled worker.
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
            slot: Result[Completed, Fault]
            try:
                slot = await run_check_async(check, settings=settings, scope=scope, routed=routed, deadline=deadline)
            except Exception as exc:  # noqa: BLE001  # fan resilience: one escaped check must not cancel sibling workers
                slot = Error(Fault((check.tool.name,), status=RailStatus.FAULTED, message=f"{type(exc).__name__}: {exc}"[:1024]))
            await send.send((i, slot))


@contextlib.asynccontextmanager
async def _pooled_ssh() -> AsyncIterator[None]:
    import asyncssh  # noqa: PLC0415  # lazy: the finally except evaluates asyncssh.Error; must bind before the try frame

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

    Memory-only backpressure: at or above ``_MEM_PRESSURE_PERCENT`` system RAM the folded limit is
    halved (floor 1) and a ``concurrency.backpressure`` event is emitted.

    Returns:
        The concurrency limit, at least 1, bounded by ``max_checks``, the mode cap, and ``settings.cpu_count``.
    """
    runner_cap = settings.dotnet_max_cpu if any(c.tool.runner is Runner.DOTNET for c in checks) else settings.max_checks
    # Mixed DOTNET+MUTATION batches must honour both ceilings; intersect so neither shadows the other.
    mode_cap = min(runner_cap, settings.mutation_max_cpu) if any(c.tool.mode is Mode.MUTATION for c in checks) else runner_cap
    limit = max(1, min(settings.max_checks, mode_cap, settings.cpu_count))
    # Load-average backpressure rejected: macOS load1 counts uninterruptible waits, far too noisy a halving signal.
    mem_percent = _safe_call(lambda: float(psutil.virtual_memory().percent), 0.0)
    match mem_percent >= _MEM_PRESSURE_PERCENT:
        case True:
            halved = max(1, limit // 2)
            _LOG.warning("concurrency.backpressure", sys_mem_percent=mem_percent, limit=limit, backpressure_limit=halved)
            return halved
        case False:
            return limit


# --- [COMPOSITION] ----------------------------------------------------------------------


def is_lease_stale(owner: _LeaseOwner, tolerance: float) -> bool:
    """Decide whether a lease holder is dead, zombie, or PID-reused and therefore stealable.

    Returns:
        True when the holder process is gone, zombie/dead, not running, or drifted past ``tolerance``; False when it is live and matching.
    """
    # (pid, create_time) within the drift band guards against PID reuse presenting as a live holder.
    try:
        proc = psutil.Process(owner.pid)
        with proc.oneshot():
            return proc.status() in _DEAD_STATUSES or not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance)
    except psutil.NoSuchProcess, ValueError:
        # ValueError covers psutil.Process(pid<=0) from a corrupt owner block; treat as dead.
        return True
    except psutil.AccessDenied:
        # AccessDenied means the OS still owns the pid; fall back to pid_exists rather than stealing a live lease.
        return not psutil.pid_exists(owner.pid)


def _claim(
    fd: int, resource: str, *, run_id: str, tolerance: float, target: str, cwd: str = "", project: str = "", mode: str = "exclusive"
) -> _LeaseOwner | None:
    # Non-blocking flock: live contention returns None (BUSY); stale or corrupt owner falls through to steal.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id, target=target, cwd=cwd, project=project, mode=mode)
    except BlockingIOError:
        held = os.read(fd, 4096)
        # b"" under a live flock means the holder is mid-write; a dead holder would have released flock and the success path above would have won.
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
    # TOCTOU: the holder may have revived between is_lease_stale and this flock; a second BlockingIOError yields BUSY.
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
    match held:
        case b"":
            return None
        case _:
            try:
                return _DECODER.decode(held)
            except msgspec.DecodeError:
                return None


def _write_all(fd: int, payload: bytes) -> None:
    # POSIX write(2) permits short-writes; advance the view until the full payload is consumed.
    view = memoryview(payload)
    while view:
        view = view[os.write(fd, view) :]


def _write_block(fd: int, block: _LeaseOwner) -> _LeaseOwner:
    os.ftruncate(fd, 0)
    os.lseek(fd, 0, os.SEEK_SET)
    _write_all(fd, msgspec.json.encode(block))
    return block


def _write_owner(fd: int, resource: str, *, run_id: str, target: str, cwd: str = "", project: str = "", mode: str = "exclusive") -> _LeaseOwner:
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
    # fcntl.flock requires a local fd; remote artifact backends cannot host lease files.
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
