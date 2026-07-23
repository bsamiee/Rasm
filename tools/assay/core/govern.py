"""Process and resource governance substrate for Assay execution.

POSIX leases and dotnet admission slots, concurrency pressure and the fan-scheduling port, psutil
telemetry and fault snapshots, and the shared stream carriers (plan, capture, drain, spill, reaping)
that local exec and remote transport both ride.
"""

from collections.abc import Callable
import contextlib
from contextvars import ContextVar
from dataclasses import dataclass
from enum import StrEnum
import fcntl
from hashlib import sha256
import os
from pathlib import Path
import signal
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING

import anyio
from anyio import to_thread  # ty mis-resolves anyio.to_thread without an explicit submodule import
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
import psutil
import structlog
from upath import UPath

from tools.assay.composition.settings import Ssh
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.aspect import ring_recent
from tools.assay.core.model import (  # beartype resolves carrier annotations at runtime under PEP 649
    Artifact,
    ArtifactKind,
    Check,
    Claim,
    Completed,
    Fault,
    Mode,
    RailStatus,
    Runner,
)


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Awaitable, Coroutine, Generator, Mapping

    from anyio.abc import ByteReceiveStream, ObjectReceiveStream, ObjectSendStream, Process
    import asyncssh

    from tools.assay.composition.settings import AssaySettings
    from tools.assay.composition.store import ArtifactStore


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.govern")

# --- [OPERATIONS] -----------------------------------------------------------------------


class LeaseScope(StrEnum):
    """Lock-root scope: REPO contends within one checkout; MACHINE contends across every invocation on the host."""

    REPO = "repo"
    MACHINE = "machine"


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


type ByteRecv = Callable[[], Coroutine[None, None, bytes | None]]

_LOCKS_OPEN_FLAGS: int = os.O_RDWR | os.O_CREAT
_LOCK_MODE: int = 0o644
_FAULT_SNAPSHOT: str = "fault.resource_snapshot"
_RING_SNAPSHOT: str = "assay.ring"
_MEM_PRESSURE_PERCENT: float = 90.0
_DOTNET_SLOT_POLL_S: float = 0.25
# A zombie or dead holder can never release its flock; both statuses are stale regardless of create-time match.
_DEAD_STATUSES: frozenset[str] = frozenset({psutil.STATUS_DEAD, psutil.STATUS_ZOMBIE})
# Foreign-process census names: the dotnet-family processes that contend for the machine across invocations.
_DOTNET_PROC_NAMES: frozenset[str] = frozenset({"dotnet", "MSBuild", "VBCSCompiler"})
# Bound at import so a patched psutil module double cannot skew the verdict comparison.
_DISK_WAIT_STATUS: str = psutil.STATUS_DISK_SLEEP
# Fixed stall thresholds make silent-child diagnostics deterministic across runs.
_STALL_AFTER_S: float = 30.0
_STALL_SAMPLE_S: float = 5.0
_RESOURCE_SAMPLE_S: float = 5.0
_STALL_CPU_PCT: float = 25.0
_STALL_CTX_RATE_HZ: float = 100.0
# POSIX-only bindings localize checker suppressions for fcntl/os/signal members.
_FLOCK, _LOCK_EX, _LOCK_NB, _LOCK_UN = fcntl.flock, fcntl.LOCK_EX, fcntl.LOCK_NB, fcntl.LOCK_UN  # ty: ignore[possibly-missing-attribute]
_GETPGID, _KILLPG, _SIGKILL = os.getpgid, os.killpg, signal.SIGKILL  # ty: ignore[possibly-missing-attribute]


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
class StalledProcess:
    """Tree-aggregated stall sample: cumulative CPU seconds, involuntary switches, root status, and process count."""

    cpu_s: float
    invol: float
    status: str
    procs: int


@dataclass(frozen=True, slots=True)
class _MemoryInfo:
    rss: float
    vms: float
    uss: float
    percent_rss: float
    num_fds: float
    num_threads: float


@dataclass(frozen=True, slots=True)
class _LoadInfo:
    # Mem and load probes degrade independently to None: a faulted source elides its keys rather than reporting a sentinel.
    mem: tuple[float, float, float] | None
    load1_percent: float | None

    def to_rows(self) -> dict[str, float]:
        mem_rows = (
            {} if self.mem is None else {"sys.mem_available_bytes": self.mem[0], "sys.mem_percent": self.mem[1], "sys.swap_percent": self.mem[2]}
        )
        load_rows = {} if self.load1_percent is None else {"sys.load1_percent": self.load1_percent}
        return {**mem_rows, **load_rows}


@dataclass(frozen=True, slots=True)
class _ChildrenInfo:
    count: float
    rss_bytes: float

    def to_rows(self) -> dict[str, float]:
        return {"proc.children": self.count, "proc.children_rss_bytes": self.rss_bytes}


@dataclass(frozen=True, slots=True)
class Measurements:
    """Single-pass process, load, and child-tree telemetry folded from one psutil oneshot and one tree walk."""

    memory: _MemoryInfo
    load: _LoadInfo
    children: _ChildrenInfo | None  # None when the recursive children walk faults; elides the two child-tree keys

    def to_resources(self) -> tuple[tuple[str, float], ...]:
        """Project the canonical resource key/value rows, eliding any child-tree or load arm that degraded.

        Returns:
            The sorted-on-consumption resource rows in their existing key spellings.
        """
        return tuple(
            {
                "mem.rss_bytes": self.memory.rss,
                "mem.vms_bytes": self.memory.vms,
                "mem.uss_bytes": self.memory.uss,
                "mem.percent_rss": self.memory.percent_rss,
                "proc.num_fds": self.memory.num_fds,
                "proc.num_threads": self.memory.num_threads,
                **(self.children.to_rows() if self.children is not None else {}),
                **self.load.to_rows(),
            }.items()
        )


@dataclass(frozen=True, slots=True)
class ExecPlan:
    """One composed execution: argv, check, cwd/env, settings, scope, and stream caps."""

    argv: tuple[str, ...]
    check: Check
    cwd: str
    env: Mapping[str, str]
    settings: AssaySettings
    scope: ArtifactScope | None
    streaming: bool
    tail_cap: int
    spill_cap: int
    chunk: int
    thread_limiter: anyio.CapacityLimiter | None

    def local_store(self) -> ArtifactStore | None:
        """Return the agent-side store a spilled stdout artifact lands in, or None without a scope."""
        # The agent-side store a spilled stdout artifact landed in; None when no scope wrote one (Captured.read resolves accordingly).
        return self.scope.store if isinstance(self.scope, ArtifactScope) else None


@dataclass(frozen=True, slots=True)
class _ConcurrencyPressure:
    slots: int
    original: int
    reduced: int
    foreign_dotnet: int
    mem_percent: float
    mem_pressure: bool
    dotnet_pressure: bool

    def slot_note(self, index: int, started: float) -> tuple[str, ...]:
        wait_ms = (time.monotonic() - started) * 1000.0
        return (
            (
                f"dotnet.slot index={index} wait_ms={wait_ms:.0f} slots={self.slots} "
                f"foreign_dotnet={self.foreign_dotnet} mem_percent={self.mem_percent:.1f} "
                f"original_concurrency={self.original} reduced_concurrency={self.reduced}"
            ),
        )


@dataclass(frozen=True, slots=True)
class Captured:
    """Drained stream payload carrier with spill state, diagnostic preview, artifact path, and size counters."""

    full: bytes = b""
    spilled: bool = False
    preview: bytes = b""
    path: str = ""
    size: int = 0
    lines: int = 0

    def read(self, store: ArtifactStore | None) -> bytes:
        """Resolve the full captured payload from memory or the spilled PROCESS artifact.

        Args:
            store: Local artifact store, consulted only on the spilled path; ``None`` when no scope wrote the artifact.

        Returns:
            The inline ``full`` bytes when not spilled, else the artifact bytes read from ``store``. A spilled artifact is
            best-effort under cancellation (the ``reap`` shield bounds the write), so a truncated read carries whatever landed.
        """
        match self.spilled:
            case False:
                return self.full
            case True:
                return store.read_path(self.path) if store is not None else b""


# Fault-time snapshots cross the anyio.run boundary; ContextVar threads the snapshot without polluting call signatures.
RESOURCE: ContextVar[tuple[tuple[str, float], ...]] = ContextVar("assay_resource", default=())

_DECODER: msgspec.json.Decoder[_LeaseOwner] = msgspec.json.Decoder(_LeaseOwner)


def _spilled(path: str, total: int, spill_cap: int) -> bool:
    # One shared spill predicate, strict greater-than: a payload retains inline at or below the cap, spills past it.
    return bool(path) and total > spill_cap


async def drain_stream(
    recv: ByteRecv, *, tail_cap: int, spill_cap: int, kind: str = "err", sink: WriteSink | None = None, path: str = "", notes: list[str] | None = None
) -> Captured:
    """Drain an async byte source to EOF, carrying the full stdout payload or a bounded stderr preview.

    Unterminated final bytes count as one logical line.

    Args:
        recv: Async read primitive returning the next chunk, or ``None`` at EOF.
        tail_cap: Maximum retained stderr preview bytes.
        spill_cap: Inline stdout ceiling; a larger payload spills to the disk sink and drops its in-memory copy.
        kind: ``"out"`` retains the full stdout payload (spilling past ``spill_cap``); any other value keeps the stderr preview window.
        sink: Optional disk sink receiving every chunk; when present a spilled stdout payload survives on disk while the bytearray stops.
        path: Artifact path recorded on the resulting capture and consulted by ``Captured.read`` on the spilled path.
        notes: Optional receipt-note sink; a sink-less stdout overflow appends one ``capture.truncated`` note here.

    Returns:
        Capture carrying the inline stdout payload or spill marker, the stderr preview, artifact path, byte size, and line count.
    """
    body, preview, total, lines, last, spilled, stopped = bytearray(), b"", 0, 0, b"", False, False
    while (read := await recv()) is not None:
        _write_sink(sink, read)
        total += len(read)
        lines += read.count(b"\n")
        last = read[-1:] or last  # an empty chunk must not clobber the newline-terminus probe
        match (kind, spilled or stopped):
            case ("out", False) if _spilled(path, total, spill_cap):
                # Crossing the cap with a disk sink: the sink keeps streaming, so drop the in-memory copy and mark spilled.
                spilled, body = True, bytearray()
            case ("out", False) if not path and total > spill_cap:
                # No disk to spill to (scope=None stream): fill to the cap, stop appending, and surface one truncation note.
                body += read
                del body[spill_cap:]
                stopped = True
                if notes is not None:
                    notes.append(f"capture.truncated kind={kind} total={total} cap={spill_cap}")
            case ("out", False):
                body += read
            case _:
                preview = (preview + read)[-tail_cap:]
    return Captured(
        full=b"" if spilled else bytes(body),
        spilled=spilled,
        preview=preview,
        path=path,
        size=total,
        lines=lines + (1 if total and last != b"\n" else 0),
    )


def recv_anyio(stream: ByteReceiveStream | None, chunk: int) -> ByteRecv:
    """Adapt an anyio byte stream to the drain pump's recv shape.

    Returns:
        Async recv yielding the next chunk, or None at EOF/closure.
    """

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


def recv_ssh(reader: asyncssh.SSHReader[bytes], chunk: int) -> ByteRecv:
    """Adapt an asyncssh reader to the drain pump's recv shape.

    Returns:
        Async recv yielding the next chunk, or None at EOF (asyncssh signals EOF with b"").
    """

    # asyncssh signals EOF with b"", not EndOfStream; map to None for pump uniformity.
    async def _recv() -> bytes | None:
        return (await reader.read(chunk)) or None

    return _recv


def touched(recv: ByteRecv, last_output: list[float]) -> ByteRecv:
    """Wrap a recv so every chunk re-arms the stall clock.

    Returns:
        Recv that stamps last_output on every chunk and EOF.
    """

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


async def drain_pair(
    plan: ExecPlan, out: ByteRecv, err: ByteRecv, wait: Callable[[], Awaitable[object]], notes: list[str] | None = None
) -> dict[str, Captured]:
    """Drain a process's out/err pair concurrently under the plan's caps, waiting the process to exit.

    Returns:
        Captures keyed "out"/"err".
    """
    streams: dict[str, Captured] = {}
    async with anyio.create_task_group() as tg:

        async def _tee(name: str, recv: ByteRecv) -> None:
            path, handle = _stream_writer(plan, name)
            match handle:
                case None:
                    streams[name] = await drain_stream(recv, tail_cap=plan.tail_cap, spill_cap=plan.spill_cap, kind=name, path=path, notes=notes)
                case _:
                    with handle as sink:
                        streams[name] = await drain_stream(
                            recv, tail_cap=plan.tail_cap, spill_cap=plan.spill_cap, kind=name, sink=sink, path=path, notes=notes
                        )

        _ = tg.start_soon(_tee, "out", out)
        _ = tg.start_soon(_tee, "err", err)
        await wait()
    return streams


async def reap(proc: Process, limiter: anyio.CapacityLimiter | None = None) -> None:
    """Terminate and wait a child process tree, shielded from cancellation."""
    # Shielded so cancellation cannot strand a child process past lease release.
    with anyio.CancelScope(shield=True):
        match proc.returncode:
            case None:
                await to_thread.run_sync(_reap_tree, proc.pid, limiter=limiter)
                # Backstop for a reap-eluding child; waitpid races degrade through _safe_call.
                _safe_call(proc.kill, None)
            case _:
                pass
        await proc.wait()


def _terminate_process_tree(tree: tuple[psutil.Process, ...], pgid: int | None) -> None:
    # SIGTERM-then-SIGKILL ladder; each phase emits one proc.reaped ledger line after its wait.
    alive = _signal_phase(tree, pgid, signal.SIGTERM)
    _ = _signal_phase(alive, pgid, _SIGKILL) if alive else ()


def _signal_phase(procs: tuple[psutil.Process, ...], pgid: int | None, sig: signal.Signals) -> tuple[psutil.Process, ...]:
    # killpg reaches re-parented grandchildren; per-process signaling covers in-tree stragglers.
    _ = _safe_call(lambda: _KILLPG(pgid, sig), None) if pgid is not None else None
    for proc in reversed(procs):
        if proc.is_running():
            proc.send_signal(sig)
    gone, alive = psutil.wait_procs(procs, timeout=1.0)
    _LOG.info("proc.reaped", signal=sig.name, killed=len(gone), survived=len(alive))
    return tuple(alive)


def _child_pgid(pid: int) -> int | None:
    # Never killpg the engine's own process group.
    return _safe_call(lambda: pgid if (pgid := _GETPGID(pid)) != _GETPGID(0) else None, None)


def _proc_tree(pid: int) -> tuple[psutil.Process, ...]:
    # Root plus the recursive child set; dotnet builds fan compiler children the root alone under-reports.
    return _safe_call(lambda: ((root := psutil.Process(pid)), *root.children(recursive=True)), ())


def _reap_tree(pid: int) -> None:
    _safe_call(lambda: _terminate_process_tree(_proc_tree(pid), _child_pgid(pid)), None)


def _tree_rss(procs: tuple[psutil.Process, ...]) -> float:
    # One owner folds resident memory across a process set; each per-proc read degrades to 0.0 through _safe_call.
    def _rss(proc: psutil.Process) -> float:
        return _safe_call(lambda: float(proc.memory_info().rss), 0.0)

    return sum(_rss(proc) for proc in procs)


def _stall_sample(pid: int) -> StalledProcess:
    # Tree-aggregated: dotnet builds fan out compiler children, so the root process alone under-reports activity.
    def _cpu(proc: psutil.Process) -> float:
        def _read() -> float:
            times = proc.cpu_times()
            return float(times.user) + float(times.system)

        return _safe_call(_read, 0.0)

    def _invol(proc: psutil.Process) -> float:
        return _safe_call(lambda: float(proc.num_ctx_switches().involuntary), 0.0)

    tree = _proc_tree(pid)
    return StalledProcess(
        cpu_s=sum(_cpu(proc) for proc in tree),
        invol=sum(_invol(proc) for proc in tree),
        status=_safe_call(tree[0].status, "") if tree else "",
        procs=len(tree),
    )


def _stall_verdict(first: StalledProcess, second: StalledProcess) -> str:
    # Silent-child triage reports the resource consumed across the sample window.
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


async def stall_monitor(pid: int, last_output: list[float], notes: list[str]) -> None:
    """Diagnose one silent-child stall after the threshold and append a bounded note."""
    # Diagnose once after the silence threshold so receipts carry at most one bounded stall note.
    while not notes:
        await anyio.sleep(_STALL_SAMPLE_S)
        match time.monotonic() - last_output[0] >= _STALL_AFTER_S:
            case False:
                pass
            case True:
                first = await to_thread.run_sync(_stall_sample, pid, abandon_on_cancel=True)
                await anyio.sleep(_STALL_SAMPLE_S)
                second = await to_thread.run_sync(_stall_sample, pid, abandon_on_cancel=True)
                silent_s = time.monotonic() - last_output[0]
                verdict = _stall_verdict(first, second)
                notes.append(f"proc.stall silent={silent_s:.0f}s {verdict}"[:256])
                _LOG.warning("proc.stall", pid=pid, silent_s=round(silent_s, 1), verdict=verdict, procs=second.procs)
                trace.get_current_span().add_event("proc.stall", attributes={"proc.pid": pid, "proc.silent_s": silent_s, "proc.verdict": verdict})


def resource_sample(pid: int, last_output: float) -> tuple[tuple[str, float], ...]:
    """Sample the process tree's resource rows for one telemetry tick.

    Returns:
        Sorted-on-construction resource key/value rows for the tree.
    """

    def _name(proc: psutil.Process) -> str:
        return _safe_call(proc.name, "")

    tree = _proc_tree(pid)
    children = tree[1:]
    names = tuple(_name(proc) for proc in tree)
    dotnet = sum(1 for name in names if name in _DOTNET_PROC_NAMES)
    csc = sum(1 for name in names if name.lower() in {"csc", "csc.exe", "vbc", "vbc.exe"})
    sample = {
        "proc.pid": float(pid),
        "proc.children": float(len(children)),
        "proc.children_rss_bytes": _tree_rss(children),
        "proc.tree_rss_bytes": _tree_rss(tree),
        "proc.dotnet.count": float(dotnet),
        "proc.csc.count": float(csc),
        "proc.last_output_age_s": max(0.0, time.monotonic() - last_output),
        **_load_info().to_rows(),
    }
    return tuple(sample.items())


async def resource_monitor(pid: int, last_output: list[float], samples: list[tuple[tuple[str, float], ...]], tool: str) -> None:
    """Append one resource sample per tick until cancelled.

    The first tick runs shielded, so a child that exits before the thread hop completes still
    lands exactly one sampled row and its ``resource.sample`` event — the receipt telemetry
    contract is at-least-one, never best-effort racing the process lifetime.
    """

    async def _tick(*, shield: bool) -> None:
        with anyio.CancelScope(shield=shield):
            sample = await to_thread.run_sync(resource_sample, pid, last_output[0], abandon_on_cancel=not shield)
            samples.append(sample)
            _LOG.info("resource.sample", tool=tool, pid=pid, **dict(sample))

    await _tick(shield=True)
    while True:
        await anyio.sleep(_RESOURCE_SAMPLE_S)
        await _tick(shield=False)


def max_resources(samples: tuple[tuple[tuple[str, float], ...], ...]) -> tuple[tuple[str, float], ...]:
    """Fold per-tick samples to the per-key maximum rows.

    Returns:
        Sorted (key, max value) rows across all samples.
    """
    folded: dict[str, float] = {}
    for sample in samples:
        for key, value in sample:
            folded[key] = max(folded.get(key, value), value)
    return tuple(sorted(folded.items()))


def stream_artifacts(scope: ArtifactScope | None, settings: AssaySettings, check: Check, streams: Mapping[str, Captured]) -> tuple[Artifact, ...]:
    """Project drained stream captures into PROCESS artifact rows under a scope.

    Returns:
        One artifact row per non-empty captured stream; empty without a scope.
    """
    _ = settings
    match scope:
        case None:
            return ()
        case ArtifactScope():
            return tuple(
                Artifact(
                    id=f"{check.tool.name}-{Path(captured.path).parent.name}-{name}",
                    kind=ArtifactKind.PROCESS,
                    path=captured.path,
                    bytes=captured.size,
                    lines=captured.lines,
                )
                for name, captured in streams.items()
                if captured.path and captured.size
            )


def captured_outputs(plan: ExecPlan, stdout: bytes, stderr: bytes) -> dict[str, Captured]:
    """Capture non-streamed stdout/stderr payloads under the plan's spill policy.

    Returns:
        Captures keyed "out"/"err" for non-empty payloads.
    """
    return {name: _capture_payload(plan, name, payload) for name, payload in (("out", stdout), ("err", stderr)) if payload}


def _capture_payload(plan: ExecPlan, name: str, payload: bytes) -> Captured:
    path = ""
    if plan.check.tool.claim is Claim.PROVISION:
        return Captured(full=payload, preview=payload[-plan.tail_cap :], size=len(payload), lines=line_count(payload))
    match plan.scope:
        case ArtifactScope(store=store):
            path = store.write_bytes(payload, *_process_parts(plan, name))
        case None:
            pass
    # scope=None retains full inline regardless of size (one in-flight subprocess bounds it); a scoped payload past the cap spills to the artifact.
    return Captured(
        full=payload if not _spilled(path, len(payload), plan.spill_cap) else b"",
        spilled=_spilled(path, len(payload), plan.spill_cap),
        preview=payload[-plan.tail_cap :],
        path=path,
        size=len(payload),
        lines=line_count(payload),
    )


def line_count(payload: bytes) -> int:
    """Count logical lines.

    Returns:
        Newline count, plus one when the payload ends unterminated.
    """
    return payload.count(b"\n") + (1 if payload and not payload.endswith(b"\n") else 0)


def _process_parts(plan: ExecPlan, name: str) -> tuple[str, str, str, str, str]:
    digest = sha256(b"\0".join(part.encode(errors="surrogateescape") for part in plan.argv)).hexdigest()[:16]
    return ArtifactKind.PROCESS.value, plan.settings.run_id, plan.check.tool.name, digest, f"{name}.log"


def _stream_writer(plan: ExecPlan, name: str) -> tuple[str, _WriteContext | None]:
    if plan.check.tool.claim is Claim.PROVISION:
        return "", None
    match plan.scope:
        case None:
            return "", None
        case ArtifactScope(store=store):
            path, handle = store.open_write(*_process_parts(plan, name))
            match handle:
                case _WriteContext() as writer:
                    return path, writer
                case _:
                    raise TypeError(f"artifact backend returned non-context writer: {type(handle).__name__}")


def _safe_call[T](fn: Callable[[], T], default: T) -> T:
    # NotImplementedError is psutil's off-POSIX signal for per-process probes such as num_fds.
    try:
        return fn()
    except psutil.Error, TypeError, ValueError, AttributeError, OSError, NotImplementedError:
        return default


def _load_info() -> _LoadInfo:
    # Memory and load probes degrade independently: each arm yields None (elided keys) when its own source calls fail.
    def _mem() -> tuple[float, float, float]:
        mem, swap = psutil.virtual_memory(), psutil.swap_memory()
        return (
            _safe_call(lambda: float(mem.available), -1.0),
            _safe_call(lambda: float(mem.percent), -1.0),
            _safe_call(lambda: float(swap.percent), -1.0),
        )

    def _load() -> float:
        load1 = os.getloadavg()[0]  # ty: ignore[possibly-missing-attribute]  # POSIX-only; absence degrades through _safe_call
        return load1 * 100.0 / max(float(psutil.cpu_count(logical=True) or 1), 1.0)

    return _LoadInfo(mem=_safe_call(_mem, None), load1_percent=_safe_call(_load, None))


def measure() -> Measurements:
    """Fold one psutil oneshot plus a child-tree walk into the unified Measurements owner.

    Returns:
        Measurements over this process, its children, and system load.
    """
    # One oneshot batches self-process syscalls; one recursive children walk and one load read feed the unified owner.
    proc = psutil.Process()

    def _walk() -> _ChildrenInfo:
        kids = tuple(proc.children(recursive=True))
        return _ChildrenInfo(count=float(len(kids)), rss_bytes=_tree_rss(kids))

    with proc.oneshot():
        info = proc.memory_info()
        memory = _MemoryInfo(
            rss=float(info.rss),
            vms=_safe_call(lambda: float(info.vms), -1.0),
            uss=_safe_call(lambda: float(proc.memory_full_info().uss), -1.0),
            percent_rss=_safe_call(lambda: float(info.rss) * 100.0 / max(float(psutil.virtual_memory().total), 1.0), -1.0),
            num_fds=_safe_call(lambda: float(proc.num_fds()), -1.0),  # ty: ignore[possibly-missing-attribute]  # POSIX-only probe
            num_threads=_safe_call(lambda: float(proc.num_threads()) if isinstance(proc.num_threads, _Nullary) else -1.0, -1.0),
        )
        children = _safe_call(_walk, None)
    return Measurements(memory=memory, load=_load_info(), children=children)


def diagnose(exc: BaseException) -> None:
    """Snapshot resources and the event ring onto the active span for a fault."""
    snap = dict(measure().to_resources())
    RESOURCE.set(tuple(snap.items()))
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=snap)
    span.add_event(_RING_SNAPSHOT, attributes={"events": ring_recent()})


def remaining(deadline: float | None) -> float | None:
    """Project the time left before an absolute monotonic deadline.

    Returns:
        Seconds remaining (floored at 1ms), or None when unbounded.
    """
    return max(0.001, deadline - time.monotonic()) if deadline is not None else None


def _total(slot: Result[Completed, Fault] | None) -> Result[Completed, Fault]:
    match slot:
        case None:
            return Error(Fault((), status=RailStatus.TIMEOUT, message="deadline exceeded"))
        case Result() as r:
            return r


def _slot_census(settings: AssaySettings, slots: int) -> tuple[int, int]:
    """Tally cross-invocation contention from the machine slot-lock blocks.

    Telemetry-only and dirty-read tolerant: the sibling lock blocks are read without
    flock, so under heavy cross-invocation churn a mid-write block decodes to ``None``
    and undercounts the foreign-invocation total by one. This is a best-effort
    queue-position count for the human ``dotnet.slot queued`` note and is NEVER an
    admission gate; promoting either tally into an admission decision is rejected.

    Returns:
        Distinct foreign ``run_id`` count holding slots, and the foreign
        dotnet-family process count.
    """
    run_ids: set[str] = set()
    for index in range(slots):
        path = UPath(settings.machine_lock_root / f"dotnet-slot-{index}.lock")
        try:
            held = path.read_bytes() if path.exists() else b""
        except OSError:
            held = b""
        owner = decode_lease_owner(held)
        if owner is not None:
            run_ids.add(owner.run_id)
    run_ids.discard(settings.run_id)
    return len(run_ids), _foreign_dotnet_count()


@contextlib.asynccontextmanager
async def dotnet_slot(  # closed resource-state ladder + machine-pool census; static rail work only surfaces its receipts
    check: Check, settings: AssaySettings, deadline: float | None
) -> AsyncIterator[Result[tuple[str, ...], Fault]]:
    """Hold one machine-wide dotnet admission slot for a DOTNET check, or time out at the deadline.

    Yields:
        Ok with slot notes while the lease is held; Error(TIMEOUT) when the deadline expires waiting.
    """
    if check.tool.runner is not Runner.DOTNET:
        yield Ok(())
        return
    started = time.monotonic()
    waited = False
    while True:
        pressure = _concurrency_pressure(settings, (check,))
        contended = 0
        for index in range(pressure.slots):
            stack = contextlib.ExitStack()
            held = stack.enter_context(
                exclusive_lease(
                    f"dotnet-slot-{index}", settings.run_id, settings=settings, project=check.tool.name, mode="dotnet-slot", scope=LeaseScope.MACHINE
                )
            )
            match held:
                case Result(tag="ok"):
                    note = pressure.slot_note(index, started)
                    if waited:
                        behind, census = _slot_census(settings, pressure.slots)
                        note = (*note, f"dotnet.slot queued {time.monotonic() - started:.1f}s behind {behind} invocations / census {census}")
                    try:
                        yield Ok(note)
                    finally:
                        stack.close()
                    return
                case _:
                    stack.close()
                    contended += 1
        # Every slot contended: emit the once-per-window slot-wait event and let the next acquire carry the queue-position note.
        waited = True
        wait_ms = (time.monotonic() - started) * 1000.0
        _LOG.info("dotnet.slot_wait", contended=contended, slots=pressure.slots, wait_ms=round(wait_ms, 1))
        trace.get_current_span().add_event(
            "dotnet.slot_wait", attributes={"slot.contended": contended, "slot.slots": pressure.slots, "slot.wait_ms": wait_ms}
        )
        left = remaining(deadline)
        if left is not None and left <= _DOTNET_SLOT_POLL_S:
            yield Error(Fault((check.tool.name,), status=RailStatus.TIMEOUT, message="dotnet slot wait deadline exceeded"))
            return
        await anyio.sleep(min(_DOTNET_SLOT_POLL_S, left or _DOTNET_SLOT_POLL_S))


def _concurrency_pressure(settings: AssaySettings, checks: tuple[Check, ...]) -> _ConcurrencyPressure:
    has_dotnet = any(c.tool.runner is Runner.DOTNET for c in checks)
    slots = max(1, min(settings.dotnet_max_cpu, settings.cpu_count))
    runner_cap = settings.dotnet_max_cpu if has_dotnet else settings.max_checks
    mode_cap = min(runner_cap, settings.mutation_max_cpu) if any(c.tool.mode is Mode.MUTATION for c in checks) else runner_cap
    original = max(1, min(settings.max_checks, mode_cap, settings.cpu_count))
    foreign = _foreign_dotnet_count() if has_dotnet else 0
    mem_percent = _safe_call(lambda: float(psutil.virtual_memory().percent), 0.0)
    mem_pressure = mem_percent >= _MEM_PRESSURE_PERCENT
    dotnet_pressure = has_dotnet and foreign >= settings.cpu_count
    reduced = max(1, original // 2) if mem_pressure or dotnet_pressure else original
    if reduced < original:
        _LOG.info("concurrency.pressure", original=original, reduced=reduced, foreign_dotnet=foreign, mem_percent=round(mem_percent, 1))
        trace.get_current_span().add_event(
            "concurrency.pressure",
            attributes={
                "pressure.original": original,
                "pressure.reduced": reduced,
                "pressure.foreign_dotnet": foreign,
                "pressure.mem_percent": mem_percent,
            },
        )
    return _ConcurrencyPressure(
        slots=slots,
        original=original,
        reduced=reduced,
        foreign_dotnet=foreign,
        mem_percent=mem_percent,
        mem_pressure=mem_pressure,
        dotnet_pressure=dotnet_pressure,
    )


def _slot_waits(notes: tuple[str, ...]) -> tuple[float, ...]:
    return tuple(
        float(part.removeprefix("wait_ms="))
        for note in notes
        if note.startswith("dotnet.slot ")
        for part in note.split()
        if part.startswith("wait_ms=") and part.removeprefix("wait_ms=").replace(".", "", 1).isdigit()
    )


def resource_projection(
    settings: AssaySettings, checks: tuple[Check, ...] = (), *, notes: tuple[str, ...] = (), receipts: tuple[Completed, ...] = ()
) -> tuple[tuple[str, float], ...]:
    """Project static resource telemetry from engine pressure, receipts, and process notes.

    Returns:
        Sorted key/value telemetry rows suitable for ``StaticRun.resources`` and fault diagnostics.
    """
    pressure = _concurrency_pressure(settings, checks)
    waits = _slot_waits(notes)
    resources = {
        "dotnet.slots": float(pressure.slots),
        "dotnet.foreign": float(pressure.foreign_dotnet),
        "memory.percent": pressure.mem_percent,
        "concurrency.original": float(pressure.original),
        "concurrency.reduced": float(pressure.reduced),
        "dotnet.pressure": float(pressure.dotnet_pressure),
        "memory.pressure": float(pressure.mem_pressure),
        "dotnet.slot_wait_ms.max": max(waits, default=0.0),
        "proc.stall.count": float(sum(1 for note in notes if note.startswith("proc.stall "))),
        "process.duration_ms.total": float(sum(done.duration_ms for done in receipts)),
        "process.duration_ms.max": max((done.duration_ms for done in receipts), default=0.0),
    }
    for key in (
        "proc.children",
        "proc.children_rss_bytes",
        "proc.tree_rss_bytes",
        "proc.dotnet.count",
        "proc.csc.count",
        "proc.last_output_age_s",
        "sys.mem_percent",
        "sys.swap_percent",
        "sys.load1_percent",
    ):
        values = tuple(value for done in receipts for name, value in done.resources if name == key)
        if values:
            resources[f"{key}.max"] = max(values)
    return tuple(sorted(resources.items()))


# (monotonic_at, count) TTL memo: the 0.25s slot poll and the pressure checks share one census within the poll window.
_FOREIGN_DOTNET_MEMO: list[tuple[float, int]] = []


def _foreign_dotnet_count() -> int:
    # Dotnet-family processes are the cross-invocation pressure unit; exclude this invocation's subtree.
    now = time.monotonic()
    match _FOREIGN_DOTNET_MEMO:
        case [(stamped, count)] if now - stamped < _DOTNET_SLOT_POLL_S:
            return count
        case _:
            own: set[int] = _safe_call(lambda: {p.pid for p in psutil.Process().children(recursive=True)} | {os.getpid()}, {os.getpid()})
            processes: list[psutil.Process] = _safe_call(lambda: list(psutil.process_iter(["name", "pid"])), [])
            count = sum(1 for proc in processes if (proc.info.get("name") or "") in _DOTNET_PROC_NAMES and proc.info.get("pid") not in own)
            _FOREIGN_DOTNET_MEMO[:] = ((now, count),)
            return count


type FanWorker = Callable[[Check], Coroutine[None, None, Result[Completed, Fault]]]


async def fan_schedule(checks: tuple[Check, ...], worker: FanWorker, *, deadline: float | None, limit: int) -> tuple[Result[Completed, Fault], ...]:
    """Schedule checks across bounded workers through the typed worker port, preserving input order.

    The worker is the execution capability (exec passes its run-check closure); govern owns only the
    scheduling algebra: bounded fan, deadline-gated feed, per-check exception containment, and the
    TIMEOUT backfill for slots the deadline starved.

    Returns:
        One completed-or-fault result per input check, in input order.
    """
    _LOG.info("fan.schedule", total=len(checks), concurrency=limit)
    trace.get_current_span().add_event("fan.schedule", attributes={"fan.total": len(checks), "fan.concurrency": limit})
    # Buffer capacity equals the check count so the producer never blocks behind a stalled worker.
    send, recv = anyio.create_memory_object_stream[tuple[int, Check]](max(1, len(checks)))
    out_send, out_recv = anyio.create_memory_object_stream[tuple[int, Result[Completed, Fault]]](limit)
    results: dict[int, Result[Completed, Fault]] = {}

    async def _worker(work: ObjectReceiveStream[tuple[int, Check]], out: ObjectSendStream[tuple[int, Result[Completed, Fault]]]) -> None:
        async with work, out:
            async for i, check in work:
                slot: Result[Completed, Fault]
                try:
                    slot = await worker(check)
                except Exception as exc:  # ruff:ignore[blind-except]  # fan resilience: one escaped check must not cancel sibling workers
                    slot = Error(Fault((check.tool.name,), status=RailStatus.FAULTED, message=f"{type(exc).__name__}: {exc}"[:1024]))
                await out.send((i, slot))

    async with anyio.create_task_group() as tg:
        work_recvs = tuple(recv.clone() for _ in range(limit))
        result_sends = tuple(out_send.clone() for _ in range(limit))
        await recv.aclose()
        await out_send.aclose()
        for work_recv, result_send in zip(work_recvs, result_sends, strict=True):
            _ = tg.start_soon(_worker, work_recv, result_send)
        async with send:
            with anyio.move_on_after(remaining(deadline)):
                for item in enumerate(checks):
                    await send.send(item)
        async with out_recv:
            async for index, result in out_recv:
                results[index] = result
                if len(results) == len(checks):
                    break
    return tuple(_total(results.get(i)) for i in range(len(checks)))


def reset_foreign_census() -> None:
    """Clear the foreign-dotnet TTL memo so a new fan takes a fresh census."""
    _FOREIGN_DOTNET_MEMO.clear()  # the memo only spans one fan's slot-poll window


def governed_concurrency(settings: AssaySettings, checks: tuple[Check, ...] = ()) -> int:
    """Fold CPU, DOTNET-runner, mutation, and pressure ceilings into one limit.

    Memory pressure or foreign dotnet-family contention halves the folded limit and emits ``concurrency.backpressure``.

    Returns:
        Concurrency limit, at least 1.
    """
    # Load-average backpressure rejected: macOS load1 counts uninterruptible waits, far too noisy a halving signal.
    pressure = _concurrency_pressure(settings, checks)
    match pressure.mem_pressure or pressure.dotnet_pressure:
        case True:
            _LOG.warning(
                "concurrency.backpressure",
                sys_mem_percent=pressure.mem_percent,
                foreign_dotnet=pressure.foreign_dotnet,
                mem_pressure=pressure.mem_pressure,
                dotnet_pressure=pressure.dotnet_pressure,
                limit=pressure.original,
                backpressure_limit=pressure.reduced,
            )
            return pressure.reduced
        case False:
            return pressure.original


def _stale(pid: int, extra: Callable[[psutil.Process], bool] = lambda _p: False) -> bool:
    # Single psutil liveness admission: gone/corrupt/dead/zombie, plus an optional per-holder staleness predicate.
    try:
        proc = psutil.Process(pid)
        with proc.oneshot():
            return proc.status() in _DEAD_STATUSES or extra(proc)
    except psutil.NoSuchProcess, ValueError:
        # ValueError covers psutil.Process(pid<=0) from a corrupt marker or owner block; treat as dead.
        return True
    except psutil.AccessDenied:
        # AccessDenied means the OS still owns the pid; defer to pid_exists rather than declaring death.
        return not psutil.pid_exists(pid)


def proc_dead(pid: int) -> bool:
    """Decide whether a pid no longer denotes a live owner.

    Returns:
        True when gone, corrupt, dead, or zombie; ``AccessDenied`` falls back to ``pid_exists``.
    """
    return _stale(pid)


def is_lease_stale(owner: _LeaseOwner, tolerance: float) -> bool:
    """Decide whether a lease holder is stealable.

    Returns:
        True when the holder is gone, zombie/dead, not running, or PID-reused beyond ``tolerance``.
    """
    # (pid, create_time) within the drift band guards against PID reuse presenting as a live holder.
    return _stale(owner.pid, lambda proc: not (proc.is_running() and abs(proc.create_time() - owner.create_time) < tolerance))


def _claim(
    fd: int, resource: str, *, run_id: str, tolerance: float, target: str, cwd: str = "", project: str = "", mode: str = "exclusive"
) -> _LeaseOwner | None:
    # Non-blocking flock: live contention returns None (BUSY); stale or corrupt owner falls through to steal.
    try:
        _FLOCK(fd, _LOCK_EX | _LOCK_NB)
        return _write_owner(fd, resource, run_id=run_id, target=target, cwd=cwd, project=project, mode=mode)
    except BlockingIOError:
        held = os.read(fd, 4096)
        # Empty bytes under a live flock means the holder is mid-write, not dead.
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
    """Decode lease-owner bytes.

    Returns:
        Owner block, or ``None`` when bytes are empty or corrupt.
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
    resource: str, run_id: str, *, settings: AssaySettings, project: str = "", mode: str = "exclusive", scope: LeaseScope = LeaseScope.REPO
) -> Generator[Result[_Held, Fault]]:
    """Acquire a non-blocking local POSIX process lease.

    Args:
        resource: Lease resource name.
        run_id: Current run identifier.
        settings: Runtime settings.
        project: Project label written into the owner block.
        mode: Lease mode label written into the owner block.
        scope: REPO roots the lock under the repo artifact store; MACHINE roots it under the per-user machine lock home.

    Yields:
        Result containing the held lease or a busy fault.
    """
    match scope:
        case LeaseScope.REPO:
            path = settings.artifact(ArtifactKind.LOCKS, f"{resource}.lock")
        case LeaseScope.MACHINE:
            path = UPath(settings.machine_lock_root / f"{resource}.lock")
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
            target=settings.exec_target.url if isinstance(settings.exec_target, Ssh) else "",
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
        diagnose(exc)
        return Error(Fault((), status=RailStatus.FAULTED, message=str(exc)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "RESOURCE",
    "ByteRecv",
    "Captured",
    "ExecPlan",
    "FanWorker",
    "LeaseScope",
    "Measurements",
    "StalledProcess",
    "WriteSink",
    "captured_outputs",
    "decode_lease_owner",
    "diagnose",
    "dotnet_slot",
    "drain_pair",
    "drain_stream",
    "exclusive_lease",
    "fan_schedule",
    "governed_concurrency",
    "is_lease_stale",
    "leased",
    "line_count",
    "max_resources",
    "measure",
    "proc_dead",
    "reap",
    "recv_anyio",
    "recv_ssh",
    "remaining",
    "reset_foreign_census",
    "resource_monitor",
    "resource_projection",
    "resource_sample",
    "stall_monitor",
    "stream_artifacts",
    "touched",
]
