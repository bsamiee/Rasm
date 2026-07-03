"""Assay execution engine for local, remote, in-process, and leased checks.

Entry points dispatch checks, discovery probes, concurrency governance, stream capture, remote command construction, and POSIX lease recovery.
"""

from collections.abc import Callable
import contextlib
from contextvars import ContextVar
from dataclasses import dataclass, replace
from enum import StrEnum
import fcntl
from functools import cache, reduce
from hashlib import sha256
import os
from pathlib import Path, PurePosixPath
import shlex
import shutil
import signal
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING

import anyio
from anyio import to_thread  # ty mis-resolves anyio.to_thread without an explicit submodule import
from expression import Error, Ok, Result
import msgspec
from opentelemetry import propagate, trace
import psutil
import stamina
import structlog
from upath import UPath

from tools.assay.composition.settings import (  # unconditional: beartype resolves forward refs at import time
    ArtifactScope,
    AssaySettings,
    Local,
    PullStrategy,
    resolve_tilde,
    run_id_host_token,
    size_from_info,
    Ssh,
)
from tools.assay.core.aspect import (
    _CHECKED_LAYER,  # noqa: PLC2701  # co-owned internal seam: aspect and engine share this layer without a public surface
    compose,
    ring_recent,
    traced,
)
from tools.assay.core.model import (  # noqa: TC001  # beartype resolves the Tool annotation on _apphost at runtime under PEP 649
    _HOST_BOUND_CLAIMS,  # noqa: PLC2701  # shared host-bound claim set for the remote-exec reject gate
    Artifact,
    ArtifactKind,
    Check,
    Claim,
    Completed,
    ExecReceipt,
    Fault,
    Mode,
    RailStatus,
    receipt,
    Runner,
    Tool,
    ToolGroup,
)
from tools.assay.core.routing import parse_csproj, place, Routed
from tools.assay.diagnostics import AST_MATCHES


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Awaitable, Coroutine, Generator, Mapping, MutableMapping

    from anyio.abc import ByteReceiveStream, ObjectReceiveStream, ObjectSendStream, Process
    import asyncssh

    from tools.assay.composition.settings import ArtifactStore


# --- [TYPES] ----------------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]
type ByteRecv = Callable[[], Coroutine[None, None, bytes | None]]


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
class Executor(Protocol):
    """Execution port rails spawn checks through; the registry weave threads the bound instance into every handler."""

    def run(
        self, check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> Result[Completed, Fault]:
        """Run one check to a completed receipt or an operational fault."""
        ...

    def fan(
        self, checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> tuple[Result[Completed, Fault], ...]:
        """Run checks concurrently, preserving input order, one result slot per check."""
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
# Keepalive failure tears down wedged SSH transports so the retry rail can re-establish before the check deadline.
_SSH_KEEPALIVE_INTERVAL_S: float = 15.0
_SSH_KEEPALIVE_COUNT_MAX: int = 3
_SSH_SIGNAL_STATUS: int = 255
# Explicit opt-out POLICY_VALUE: an exec_known_hosts of this token maps to asyncssh known_hosts=None (host-key verification
# off). The empty/unset path keeps the ~/.ssh/known_hosts default (settings env_ignore_empty), so this is the only disable route.
_KNOWN_HOSTS_INSECURE: str = "insecure"
# Repo push + artifact pull bracket the remote exec; a shielded ceiling stops a large transfer from reclassifying a completed run as TIMEOUT.
# The budget floor and per-file scale are operator-owned settings (transfer_budget_s / transfer_per_file_s); this constant is the
# manifest-discovery limit only, where no settings instance is in scope yet. _transfer_budget folds (floor, file_count * per-file).
_TRANSFER_BUDGET_S: float = 120.0
# git ls-files is the set-algebraic source universe the lane manifest scopes: .git/.cache/.artifacts/bin/obj/node_modules/.venv are
# gitignored, so they never cross; the build-closure derivation then narrows the universe to the lane's transitive closure.
_PUSH_MANIFEST_ARGV: tuple[str, ...] = ("git", "ls-files", "-z")
# Push throttle (per-directory put concurrency over the one channel, and per-put request pipelining) is operator-owned on
# AssaySettings.sftp_push_concurrency / sftp_max_requests so a throttled or low-MaxSessions server can tune it down.
# Root build-config files always cross with a C# closure: a transitive ProjectReference set still resolves versions, props,
# and SDK against these repo-root anchors, so the lane manifest keeps them regardless of which project dir a closure spans.
_CSHARP_CONFIG_FILES: frozenset[str] = frozenset((
    ".config/dotnet-tools.json",
    ".editorconfig",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "Workspace.slnx",
    "global.json",
    "nuget.config",
))
# A Python lane pushes the package source, the test corpus, and the dependency/config anchors; nothing else in the tree is on
# the import or test path. The prefixes are repo-relative dir roots; the files are repo-root anchors that resolve the env.
_PYTHON_MANIFEST_PREFIXES: tuple[str, ...] = ("tools/", "tests/python/", "libs/python/", "src/")
_PYTHON_CONFIG_FILES: frozenset[str] = frozenset((".python-version", "pyproject.toml", "uv.lock"))
# SFTP v3 directory file-type discriminant (FILEXFER_TYPE_DIRECTORY); the remote prune sweeps only run dirs, never stray files.
_SFTP_DIR_TYPE: int = 2
_DOTNET_PROBE_TIMEOUT_S: float = 15.0
_DOTNET_SHARED_RUNTIME: tuple[str, str] = ("shared", "Microsoft.NETCore.App")
_DOTNET_SLOT_POLL_S: float = 0.25
_RETRY_MIN_REMAINING: float = 0.05
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
class _ExecPlan:
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
        # The agent-side store a spilled stdout artifact landed in; None when no scope wrote one (Captured.read resolves accordingly).
        return self.scope.store if isinstance(self.scope, ArtifactScope) else None


@dataclass(frozen=True, slots=True)
class _PreparedExec:
    check: Check
    argv: tuple[str, ...]
    cwd: str
    env: Mapping[str, str]
    bound: float | None
    thread_limiter: anyio.CapacityLimiter


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
            best-effort under cancellation (the ``_reap`` shield bounds the write), so a truncated read carries whatever landed.
        """
        match self.spilled:
            case False:
                return self.full
            case True:
                return store.read_path(self.path) if store is not None else b""


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
    """Inject scope flags into eligible DOTNET build-graph commands.

    Returns:
        Command with scope flags inserted before ``--``, or the original command when ineligible.
    """
    match (runner, command, scope):
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in scoped_verbs and mode not in {Mode.QUERY, Mode.LIST}:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *_dotnet_scope_flags(command=command, scope=s), *command[cut:])
        case _:
            return command


def _dotnet_scope_flags(command: tuple[str, ...], scope: ArtifactScope) -> tuple[str, ...]:
    match _project_scope(command):
        case "":
            return scope.dotnet_flags
        case segment:
            return tuple(
                f"{scope.path.rstrip('/')}/dotnet/{segment}" if prior == "--artifacts-path" else current
                for prior, current in zip(("", *scope.dotnet_flags[:-1]), scope.dotnet_flags, strict=True)
            )


def _project_scope(command: tuple[str, ...]) -> str:
    try:
        project = command[command.index("--project") + 1]
    except ValueError, IndexError:
        return ""
    stem = PurePosixPath(project.replace("\\", "/")).with_suffix("").as_posix().replace("/", "__")
    return "".join(ch if ch.isalnum() or ch in "-._" else "_" for ch in stem).strip("._")


def _overlay(tool: Tool, settings: AssaySettings, scope: ArtifactScope | None) -> Mapping[str, str]:
    base: MutableMapping[str, str] = dict(os.environ)  # noqa: TID251  # clones the host environment at the spawn boundary
    base.update(settings.python_tool_env)
    base.update(tool.env)
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> Result[tuple[str, ...], Fault]:
    tool = check.tool
    body = tool.command
    body = (*(part for group in tool.uv_groups() for part in ("--group", group)), *body) if tool.runner is Runner.UV else body
    body = ("--project", str(settings.root), *body) if tool.runner is Runner.UV and tool.stage.project else body
    body = ("--locked", *body) if tool.runner is Runner.UV else body
    prefix = ("uv", "run", "--locked", "python", "-m") if tool.runner is Runner.MODULE else tool.runner.prefix
    # A staged dotnet tool (Stryker) runs from an empty .artifacts cwd; absolute --solution/--output anchor it to the real
    # tree and keep its sandbox + reports under .artifacts. _materialize forces cwd to the same staged work root.
    body = (
        (*body, "--solution", str(settings.solution), "--output", str(Path(str(settings.root)).resolve() / tool.stage.root))
        if tool.runner is Runner.DOTNET and tool.stage.project
        else body
    )

    def scoped(pinned: tuple[str, ...]) -> tuple[str, ...]:
        return splice_command(tool.runner, (*body, *pinned), scope, settings.scoped_verbs, tool.mode)

    match check.tail:
        case tuple() as pinned:
            return Ok((*prefix, *scoped(pinned)))
        case None:
            tails = place(routed, tool, settings=settings)
            match tails:
                case () | (_,):
                    return Ok((*prefix, *scoped(tuple(part for tail in tails for part in tail))))
                case _:
                    return Error(Fault((tool.name,), message=f"incoherent closure: {len(tails)} tails for one check"))


def argv_for(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> Result[tuple[str, ...], Fault]:
    """Project the exact argv the engine would execute for a check.

    Returns:
        Command argv, or a fault when an unpinned check resolves to multiple tails.
    """
    return _argv(check, routed, settings=settings, scope=scope)


async def discover_async(argv: tuple[str, ...], *, root: UPath | Path | str, limit_s: float) -> Result[bytes, Fault]:
    """Run an engine-owned read-only discovery command.

    Returns:
        Captured stdout, or a typed discovery fault.
    """
    try:
        with anyio.fail_after(limit_s):
            done = await anyio.run_process(list(argv), cwd=str(root), check=False, start_new_session=True)
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
    # Nix DOTNET_ROOT can point at a wrapper; resolve the real shared-runtime root once per process.
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
    # Apphost-deployed dotnet tools need DOTNET_ROOT; SDK verbs self-locate through the muxer.
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
    # The per-part scan rejects empty, self, and parent parts without a standalone path disjunct.
    match text.startswith("/") or "\x00" in text or any(p in {"", ".", ".."} for p in parts):
        case True:
            return ValueError(f"unsafe stage path: {rel!r}")
        case False:
            target = (root / text).resolve()
            return target if target.is_relative_to(root) else ValueError(f"stage path escaped root: {rel!r}")


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


async def _drain_pair(
    plan: _ExecPlan, out: ByteRecv, err: ByteRecv, wait: Callable[[], Awaitable[object]], notes: list[str] | None = None
) -> dict[str, Captured]:
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


async def _reap(proc: Process, limiter: anyio.CapacityLimiter | None = None) -> None:
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


async def _stall_monitor(pid: int, last_output: list[float], notes: list[str]) -> None:
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


def _resource_sample(pid: int, last_output: float) -> tuple[tuple[str, float], ...]:
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


async def _resource_monitor(pid: int, last_output: list[float], samples: list[tuple[tuple[str, float], ...]], tool: str) -> None:
    while True:
        sample = await to_thread.run_sync(_resource_sample, pid, last_output[0], abandon_on_cancel=True)
        samples.append(sample)
        _LOG.info("resource.sample", tool=tool, pid=pid, **dict(sample))
        await anyio.sleep(_RESOURCE_SAMPLE_S)


def _max_resources(samples: tuple[tuple[tuple[str, float], ...], ...]) -> tuple[tuple[str, float], ...]:
    folded: dict[str, float] = {}
    for sample in samples:
        for key, value in sample:
            folded[key] = max(folded.get(key, value), value)
    return tuple(sorted(folded.items()))


async def _run_process_backend(plan: _ExecPlan) -> Completed:  # noqa: PLR0914  # closed local/remote backend branches keep telemetry state local
    started = time.monotonic()
    _LOG.info(
        "process.start", tool=plan.check.tool.name, argv=plan.argv, cwd=plan.cwd, streaming=plan.streaming, remote=bool(plan.settings.exec_target)
    )
    match plan.settings.exec_target:
        case Local():
            match plan.streaming:
                case True:
                    proc = await anyio.open_process(list(plan.argv), cwd=plan.cwd, env=plan.env, start_new_session=True)
                    try:
                        last_output, stall, samples = [time.monotonic()], list[str](), list[tuple[tuple[str, float], ...]]()
                        async with anyio.create_task_group() as tg:
                            _ = tg.start_soon(_stall_monitor, proc.pid, last_output, stall)
                            _ = tg.start_soon(_resource_monitor, proc.pid, last_output, samples, plan.check.tool.name)
                            streams = await _drain_pair(
                                plan,
                                _touched(_recv_anyio(proc.stdout, plan.chunk), last_output),
                                _touched(_recv_anyio(proc.stderr, plan.chunk), last_output),
                                proc.wait,
                                stall,
                            )
                            tg.cancel_scope.cancel()
                        resources = _max_resources(tuple(samples) or (_resource_sample(proc.pid, last_output[0]),))
                        duration_ms = (time.monotonic() - started) * 1000.0
                        _LOG.info(
                            "process.end",
                            tool=plan.check.tool.name,
                            argv=plan.argv,
                            returncode=proc.returncode or 0,
                            duration_ms=round(duration_ms, 1),
                            **dict(resources),
                        )
                        return msgspec.structs.replace(
                            receipt(
                                plan.argv,
                                proc.returncode or 0,
                                stdout=streams.get("out", Captured()).read(plan.local_store()),
                                stderr=streams.get("err", Captured()).preview,
                                notes=tuple(stall),
                                artifacts=_stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                            ),
                            resources=(*resources, ("process.duration_ms", duration_ms)),
                        )
                    finally:
                        await _reap(proc, plan.thread_limiter)
                case False:
                    done = await anyio.run_process(list(plan.argv), cwd=plan.cwd, env=plan.env, check=False, start_new_session=True)
                    streams = _captured_outputs(plan, done.stdout, done.stderr)
                    # One measurement owner feeds every receipt: the non-streaming receipt now carries proc.children like the streaming path.
                    resources = tuple(sorted(_measure().to_resources()))
                    duration_ms = (time.monotonic() - started) * 1000.0
                    _LOG.info(
                        "process.end",
                        tool=plan.check.tool.name,
                        argv=plan.argv,
                        returncode=done.returncode,
                        duration_ms=round(duration_ms, 1),
                        **dict(resources),
                    )
                    return msgspec.structs.replace(
                        receipt(
                            plan.argv,
                            done.returncode,
                            stdout=streams.get("out", Captured()).read(plan.local_store()),
                            stderr=streams.get("err", Captured()).preview,
                            artifacts=_stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                        ),
                        resources=(*resources, ("process.duration_ms", duration_ms)),
                    )
        case Ssh() as target:
            # The remote env (allowlist + injected toolchain PATH) is built inside _run_remote once the connection resolves the home.
            remote_done = await _run_remote(plan, target)
            _LOG.info(
                "process.end",
                tool=plan.check.tool.name,
                argv=plan.argv,
                returncode=remote_done.returncode,
                duration_ms=round((time.monotonic() - started) * 1000.0, 1),
                remote=True,
            )
            return remote_done


def _stream_artifacts(scope: ArtifactScope | None, settings: AssaySettings, check: Check, streams: Mapping[str, Captured]) -> tuple[Artifact, ...]:
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


def _captured_outputs(plan: _ExecPlan, stdout: bytes, stderr: bytes) -> dict[str, Captured]:
    return {name: _capture_payload(plan, name, payload) for name, payload in (("out", stdout), ("err", stderr)) if payload}


def _capture_payload(plan: _ExecPlan, name: str, payload: bytes) -> Captured:
    path = ""
    if plan.check.tool.claim is Claim.PROVISION:
        return Captured(full=payload, preview=payload[-plan.tail_cap :], size=len(payload), lines=_line_count(payload))
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
        lines=_line_count(payload),
    )


def _line_count(payload: bytes) -> int:
    return payload.count(b"\n") + (1 if payload and not payload.endswith(b"\n") else 0)


def _process_parts(plan: _ExecPlan, name: str) -> tuple[str, str, str, str, str]:
    digest = sha256(b"\0".join(part.encode(errors="surrogateescape") for part in plan.argv)).hexdigest()[:16]
    return ArtifactKind.PROCESS.value, plan.settings.run_id, plan.check.tool.name, digest, f"{name}.log"


def _stream_writer(plan: _ExecPlan, name: str) -> tuple[str, _WriteContext | None]:
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


@dataclass(frozen=True, slots=True)
class _Outcome:
    streams: dict[str, Captured]
    exit_status: int | None
    signal: object | None  # raw asyncssh exit_signal: the (name, *) tuple or None; decoded once by ssh_outcome/_signal_name
    notes: tuple[str, ...] = ()


def _remap_scope_path(token: str, *, local_root: str, remote_root: str) -> str:
    # One derived rule remaps every build-scope path the local store seeded into argv (CspSarifDir, --artifacts-path,
    # any future scope flag) from its macOS-absolute form to the remote workroot, so a remote Linux build never sees a
    # host-absolute path (CS0016). The flag/value frame is preserved: a `prop=<abs>` token rebinds only the value tail,
    # a bare `<abs>` token rebinds whole, and a token carrying no local-root path passes through untouched.
    prefix, sep, value = token.rpartition("=")
    target = value if sep else token
    rebased = f"{remote_root}/{target[len(local_root) + 1 :]}" if target.startswith(f"{local_root}/") else target
    return f"{prefix}{sep}{rebased}" if sep else rebased


def _remote_scope_argv(argv: tuple[str, ...], *, local_root: str, remote_root: str) -> tuple[str, ...]:
    """Rewrite every host-absolute build-scope path in argv to the remote workroot before remote argv composition.

    Returns:
        The argv with ``CspSarifDir``/``--artifacts-path``/scope paths rebased ``<local_root>/X -> <remote_root>/X``.
    """
    return tuple(_remap_scope_path(token, local_root=local_root, remote_root=remote_root) for token in argv)


async def _resolve_remote_plan(plan: _ExecPlan, target: Ssh, conn: asyncssh.SSHClientConnection) -> _ExecPlan:
    # Resolve the remote ``~`` once (sftp.realpath('.') canonicalizes the SFTP login dir to the absolute home; a chroot returns '/')
    # so the SFTP push, the offload backend root, the exec ``cd``, and the injected toolchain PATH all share one absolute workroot.
    async with await conn.start_sftp_client() as sftp:
        home = str(await sftp.realpath("."))
    settings = plan.settings.model_copy(update={"exec_target": target.resolve_home(home)})
    row_env = frozenset(key for key, _ in plan.check.tool.env)
    env = settings.remote_env(dict(plan.env), home=home, forward=row_env)
    target_resolved = settings.exec_target if isinstance(settings.exec_target, Ssh) else target
    remote_root = target_resolved.remote_workroot(settings.run_id)
    argv = _remote_scope_argv(plan.argv, local_root=str(plan.settings.local_root), remote_root=remote_root)
    return replace(plan, argv=argv, cwd=remote_root, env=env, settings=settings)


async def _run_remote(plan: _ExecPlan, target: Ssh) -> Completed:
    async with _ssh_connection(target) as conn:
        plan = await _resolve_remote_plan(plan, target, conn)
        resolved = plan.settings.exec_target if isinstance(plan.settings.exec_target, Ssh) else target
        # Pre-flight probe under the SAME injected PATH the exec uses, so a tool on the injected PATH never falsely reads as UNSUPPORTED.
        match await _probe_toolchain(conn, plan.argv, path=plan.env.get("PATH", "")):
            case (str() as missing, str() as detail):
                miss = receipt(plan.argv, RailStatus.UNSUPPORTED.exit_code, status=RailStatus.UNSUPPORTED, stderr=detail.encode()[:1024])
                return _fold_receipt(miss, resolved, exit_status=None, signal="", notes=(f"remote.toolchain.missing tool={missing}",))
            case _:
                pass
        # One shielded bracket owns push (before exec) and pull (after exec) on the same pooled connection.
        async with _remote_transfer(conn, plan) as transfer:
            outcome = await _remote_exec(conn, plan, remote_command(plan.argv, cwd=plan.cwd, env=plan.env))
            pulled = await transfer.pull(outcome.streams)
    return _remote_done(plan, resolved, transfer, outcome, pulled)


def _remote_done(plan: _ExecPlan, target: Ssh, transfer: _Transfer, outcome: _Outcome, pulled: _Pulled) -> Completed:
    """Fold the remote exec outcome, transfer counts, and pull artifacts into one local ``Completed`` with an ``ExecReceipt``.

    Returns:
        The completed receipt carrying the resolved exit code, signal/transfer notes, pulled artifacts, and exec facts.
    """
    code, signal_notes = ssh_outcome(outcome.exit_status, outcome.signal)
    notes = (*transfer.notes, *pulled.notes)
    # The spilled stdout artifact is written agent-side to the LOCAL store during drain, distinct from the pulled SCOPE tree.
    done = receipt(
        plan.argv,
        code,
        stdout=outcome.streams.get("out", Captured()).read(plan.local_store()),
        stderr=outcome.streams.get("err", Captured()).preview,
        notes=(*signal_notes, *outcome.notes, *notes),
        artifacts=pulled.artifacts,
    )
    return _fold_receipt(
        done, target, exit_status=outcome.exit_status, signal=_signal_name(outcome.signal), notes=notes, pushed=transfer.pushed, pulled=pulled.count
    )


async def _remote_exec(conn: asyncssh.SSHClientConnection, plan: _ExecPlan, command: str) -> _Outcome:
    import asyncssh  # noqa: PLC0415  # lazy: ~83ms cold-start cost; defer past import time

    # The raw asyncssh exit_signal ((name, *) tuple or None) rides _Outcome verbatim; ssh_outcome/_signal_name own the decode.
    match plan.streaming:
        case True:
            # No stall telemetry on this branch: psutil cannot inspect remote pids across the SSH boundary.
            proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
            try:
                drain_notes = list[str]()
                streams = await _drain_pair(plan, _recv_ssh(proc.stdout, plan.chunk), _recv_ssh(proc.stderr, plan.chunk), proc.wait, drain_notes)
                return _Outcome(streams, proc.exit_status, getattr(proc, "exit_signal", None), tuple(drain_notes))
            finally:
                with anyio.CancelScope(shield=True):
                    proc.close()
                    await proc.wait_closed()
        case False:
            run = await conn.run(command, encoding=None, check=False)
            streams = _captured_outputs(plan, _as_bytes(run.stdout), _as_bytes(run.stderr))
            return _Outcome(streams, run.exit_status, getattr(run, "exit_signal", None))


@dataclass(frozen=True, slots=True)
class _Pulled:
    artifacts: tuple[Artifact, ...]
    count: int
    notes: tuple[str, ...]


@dataclass(frozen=True, slots=True)
class _Transfer:
    conn: asyncssh.SSHClientConnection
    plan: _ExecPlan
    pushed: int
    notes: tuple[str, ...]

    async def pull(self, streams: Mapping[str, Captured]) -> _Pulled:
        """Reach the tool-written remote scope tree after the remote exec, dispatching on the offload pull strategy.

        ``TRANSFER`` downloads the tree byte-for-byte over sftp into the agent-local landing store; ``SHARED`` reads the
        same universal paths the remote tool wrote to a cloud object store directly, with zero byte transfer.

        Returns:
            Folded scope artifacts, the pulled-file count, and any degrade note.
        """
        captured = _stream_artifacts(self.plan.scope, self.plan.settings, self.plan.check, streams)
        if not isinstance(self.plan.scope, ArtifactScope):
            return _Pulled(captured, 0, ())
        scope: ArtifactScope = self.plan.scope
        budget = _transfer_budget(self.plan.settings)
        try:
            # Shield the post-exit read so a completed run lands its artifacts even under deadline cancellation; the budget still bounds it.
            with anyio.CancelScope(shield=True), anyio.fail_after(budget):
                landed = await self._reach_scope(scope)
        except TimeoutError:
            return _Pulled(captured, 0, (f"remote.artifacts.degraded budget_s={budget:g}",))
        if landed is None:
            return _Pulled(captured, 0, ("remote.artifacts.degraded missing_tree",))
        return _Pulled((*captured, *landed), len(landed), ())

    async def _reach_scope(self, scope: ArtifactScope) -> tuple[Artifact, ...] | None:
        # One dispatch owns every reach modality: sftp byte-download, shared zero-transfer read, or no admitted backend.
        match self.plan.settings.offload.pull_strategy if self.plan.settings.offload is not None else PullStrategy.NONE:
            case PullStrategy.TRANSFER:
                return await _sftp_pull_scope(self.conn, self.plan, scope)
            case PullStrategy.SHARED:
                return await to_thread.run_sync(_shared_read_scope, self.plan, scope)
            case _:
                return ()


@contextlib.asynccontextmanager
async def _remote_transfer(conn: asyncssh.SSHClientConnection, plan: _ExecPlan) -> AsyncIterator[_Transfer]:
    """Bracket the remote exec with the push leg and prepare the pull leg on one pooled connection.

    The repo working tree is pushed up front so the remote tool sees it at ``<workroot>/<run_id>``; the pull leg
    runs after the body downloads the tool-written scope tree. Push and pull each own a shield + budget so a large
    transfer degrades to a receipt note rather than reclassifying a completed run as TIMEOUT; the ``yield`` stays
    outside the push shield so the bracketed remote exec remains cancellable by the check deadline.

    Yields:
        A transfer handle carrying the pushed-file count, push notes, and the post-exec ``pull`` coroutine.
    """
    manifest = await _push_manifest(plan)
    budget = _transfer_budget(plan.settings, len(manifest))
    # Retention prune is hoisted to the fan-level pooled-ssh teardown (once per fan), so the push leg no longer pays the
    # per-check scandir: the current run dir always survives the sweep because it is the newest under the workroot.
    try:
        with anyio.CancelScope(shield=True), anyio.fail_after(budget):
            pushed, push_notes = await _push_repo(conn, plan, manifest)
    except TimeoutError:
        pushed, push_notes = 0, (f"remote.push.degraded budget_s={budget:g} files={len(manifest)}",)
    yield _Transfer(conn=conn, plan=plan, pushed=pushed, notes=push_notes)


def _transfer_budget(settings: AssaySettings, file_count: int = 0) -> float:
    # Floor covers the pull leg and small manifests; the per-file term scales the push so a 1000+ file tree does not degrade mid-push.
    return max(settings.transfer_budget_s, file_count * settings.transfer_per_file_s)


async def _push_repo(conn: asyncssh.SSHClientConnection, plan: _ExecPlan, manifest: tuple[str, ...]) -> tuple[int, tuple[str, ...]]:
    """Push the lane-scoped build closure to the remote run dir, pipelining per-directory puts over one SFTP channel.

    The manifest is the lane-scoped build closure (``_lane_manifest``) over the ``git ls-files`` universe, so gitignored
    roots never cross and only the closure's files do. Each repo-relative directory group is one ``put(list, remotedir)``
    preserving the exact tree (``<abs-workroot>/<run_id>/<relpath>``, never a literal ``~``); the groups run concurrently
    under an operator-tuned capacity limiter so asyncssh multiplexes each open/write/close on the single channel,
    overlapping round-trip latency instead of serializing single-file directories. Per-file ``put`` failures fold into notes.

    A channel-open or run-root ``makedirs`` failure cannot proceed coherently, so it short-circuits to a single
    ``remote.push.dir_failed`` note (degrade-to-note, never a FAULTED crash); a per-directory ``makedirs`` failure folds the
    same note and drops only that subtree, leaving sibling directories to push. Per-file ``put`` failures fold into notes too.

    Returns:
        The pushed-file count and any per-file/per-dir failure or empty-manifest notes.
    """
    import asyncssh  # noqa: PLC0415  # lazy: bind asyncssh.Error before the except evaluates it; defer the ~83ms cold-start

    local_root = Path(str(plan.settings.local_root))
    remote_root = plan.cwd.rstrip("/")
    limiter = anyio.CapacityLimiter(plan.settings.sftp_push_concurrency)
    max_requests = plan.settings.sftp_max_requests
    # One failure stream owns both fault kinds as (note, dropped) facts: the dropped weight stays a typed int the pushed
    # count folds directly, so the count is never re-parsed out of its own formatted note string.
    failures: list[tuple[str, int]] = []

    def _dir_failed(parent: str, dropped: int, exc: BaseException) -> tuple[str, int]:
        return (f"remote.push.dir_failed dir={parent or '.'!r} {type(exc).__name__}: {str(exc)[:120]}", dropped)

    async def _push_dir(sftp: asyncssh.SFTPClient, parent: str, names: tuple[str, ...]) -> None:
        async with limiter:
            remote_dir = "/".join((remote_root, *parent.split("/"))) if parent else remote_root
            # makedirs and the put both fault per-directory: a raise from either (channel-level error, or a source-stat
            # FileNotFoundError that asyncssh raises before the transfer rather than routing through error_handler) drops
            # only this subtree, leaving sibling directories to push. error_handler still folds remote-side transfer errors
            # per file (weight 1); a put raise aborts before any transfer, so it carries the whole undelivered group weight.
            locals_in_dir = [str(local_root / (f"{parent}/{name}" if parent else name)) for name in names]
            handler = lambda exc: failures.append((f"remote.push.failed {type(exc).__name__}: {str(exc)[:120]}", 1))  # noqa: E731
            try:
                await sftp.makedirs(remote_dir, exist_ok=True)
                await sftp.put(locals_in_dir, remote_dir, max_requests=max_requests, error_handler=handler)
            except (OSError, asyncssh.Error) as exc:
                failures.append(_dir_failed(parent, len(names), exc))

    async def _drive() -> None:
        async with await conn.start_sftp_client() as sftp:
            # The run root is created unconditionally so the remote `cd <workroot>/<run_id>` is valid even for an empty manifest.
            await sftp.makedirs(remote_root, exist_ok=True)
            async with anyio.create_task_group() as tg:
                for parent, names in _grouped_by_parent(manifest).items():
                    _ = tg.start_soon(_push_dir, sftp, parent, names)

    aborted: tuple[tuple[str, int], ...] = ()
    try:
        await _drive()
    except* (OSError, asyncssh.Error) as group:
        # Channel-open or run-root makedirs failed (no subtree pushed): bind one whole-manifest degrade fact, return after the except* block.
        aborted = tuple(_dir_failed("", len(manifest), exc) for exc in group.exceptions)
    final = aborted or tuple(failures)
    pushed = max(0, len(manifest) - sum(dropped for _, dropped in final))
    return pushed, tuple(note for note, _ in final) or (() if manifest else ("remote.push.empty",))


async def _push_manifest(plan: _ExecPlan) -> tuple[str, ...]:
    # git ls-files -z is NUL-delimited so paths with spaces/newlines survive; the agent-local root is the manifest source.
    # The full git universe is then lane-scoped to the build closure so a remote run pushes the closure, never the whole tree.
    listed = await discover_async(_PUSH_MANIFEST_ARGV, root=plan.settings.local_root, limit_s=_TRANSFER_BUDGET_S)
    universe = listed.map(lambda out: tuple(p for p in out.decode(errors="replace").split("\x00") if p)).default_value(())
    return _lane_manifest(plan, universe)


def _lane_manifest(plan: _ExecPlan, universe: tuple[str, ...]) -> tuple[str, ...]:
    # One dispatch on the lane's runner scopes the universe to the build closure: a C# closure is the transitive
    # ProjectReference set plus root build config; a Python lane is package + tests + config; every other lane keeps the
    # full universe (it carries no project graph to scope against). Naive subtree-scope is rejected: cross-project refs
    # would be dropped, so the C# arm walks ProjectReference transitively rather than trusting directory containment alone.
    match plan.check.tool.runner:
        case Runner.DOTNET:
            return _csharp_manifest(plan, universe)
        case Runner.UV | Runner.MODULE | Runner.PNPM:
            return _python_manifest(universe)
        case _:
            return universe


def _csharp_manifest(plan: _ExecPlan, universe: tuple[str, ...]) -> tuple[str, ...]:
    seeds = _csharp_seeds(plan)
    if not seeds:
        return universe
    local_root = Path(str(plan.settings.local_root))
    projects = frozenset(p for p in universe if p.endswith(".csproj"))
    closure = _project_closure(seeds, projects, local_root)
    dirs = tuple(f"{rel.rpartition('/')[0]}/" if "/" in rel else "" for rel in closure)
    return tuple(
        rel for rel in universe if rel in _CSHARP_CONFIG_FILES or any(prefix and rel.startswith(prefix) for prefix in dirs) or rel in closure
    )


def _csharp_seeds(plan: _ExecPlan) -> frozenset[str]:
    # Seeds are the closure roots: the .csproj project tokens the composed build argv carries. The project tail is bound by
    # `place(routed, ...)` at argv composition for a `--project`/closure route, so it lands in `plan.argv`, never in
    # `tool.command` or `check.paths` (those stay empty for a project route). Reading the composed argv keeps the seed in one
    # source regardless of whether the project arrived as `check.tail`, an unpinned `place()` tail, or a routed file token.
    # Absolute argv tokens are rebased to repo-relative against the agent-local root so they key into the git universe.
    local_root = str(plan.settings.local_root)
    return frozenset(
        rel
        for token in plan.argv
        if token.endswith(".csproj")
        for rel in (token[len(local_root) + 1 :] if token.startswith(f"{local_root}/") else token,)
        if rel and not rel.startswith("/")
    )


def _project_closure(seeds: frozenset[str], projects: frozenset[str], local_root: Path) -> frozenset[str]:
    # Forward-dependency fixed-point over the ProjectReference graph: the build of a seed needs every project the seed
    # transitively references, so each pass folds in the references of the current members. A cross-directory reference
    # (libs/A -> libs/B) survives because the walk follows the edge, not directory containment; subtree-scope would drop it.
    # Complete in at most len(projects) passes since each pass adds at least one node or terminates.
    graph = {rel: _csproj_refs(rel, local_root) & projects for rel in projects}
    seeded = seeds & projects
    return reduce(
        lambda current, _: current | frozenset(ref for member in current for ref in graph.get(member, frozenset())), range(len(graph)), seeded
    )


def _csproj_refs(rel: str, local_root: Path) -> frozenset[str]:
    # An unreadable or malformed project becomes an isolated graph node; the closure derivation never faults on one bad file.
    parent = PurePosixPath(rel).parent
    try:
        raw = (local_root / rel).read_bytes()
    except OSError:
        return frozenset()
    normalized = (os.path.normpath(str(parent / inc.replace("\\", "/"))) for inc in parse_csproj(raw, "ProjectReference", "Include"))
    return frozenset(PurePosixPath(ref).as_posix() for ref in normalized)


def _python_manifest(universe: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(rel for rel in universe if rel in _PYTHON_CONFIG_FILES or any(rel.startswith(prefix) for prefix in _PYTHON_MANIFEST_PREFIXES))


def _grouped_by_parent(manifest: tuple[str, ...]) -> dict[str, tuple[str, ...]]:
    # Group by repo-relative parent so one put call per directory preserves tree structure: basenames within a dir are unique.
    groups: dict[str, list[str]] = {}
    for rel in manifest:
        parent, _, name = rel.rpartition("/")
        groups.setdefault(parent, []).append(name)
    return {parent: tuple(names) for parent, names in groups.items()}


def _stale_remote_runs(rows: tuple[tuple[str, float], ...], *, token: str, keep: int) -> tuple[str, ...]:
    """Select this host's surplus remote run dirs to prune, mirroring the local retention rank without cross-host deletes.

    Rows are ``(run_id, mtime)`` directory entries directly under ``<workroot>``. Only run ids carrying this run's host
    token survive the filter, so a shared workroot keeps every other host's namespace intact; the survivors rank
    oldest-first by ``(mtime, run_id)`` exactly as the local ``_sorted_run_ids`` does, and all but the newest ``keep`` prune.

    Returns:
        Run-dir basenames to remove, oldest-first, scoped to this host's run-id namespace.
    """
    # Rank oldest-first on (mtime, run_id): the mtime-keyed tuple reorder sorts without a lambda; run_id breaks an mtime tie.
    own = sorted((mtime, run_id) for run_id, mtime in rows if run_id_host_token(run_id) == token)
    return tuple(run_id for _, run_id in own[: max(0, len(own) - keep)])


async def _remote_prune(conn: asyncssh.SSHClientConnection, settings: AssaySettings) -> tuple[str, ...]:
    """Sweep this host's stale ``<workroot>/<run_id>`` dirs on the remote over the pooled SFTP connection, once per fan.

    The remote workroot accumulates one git-tracked source closure per offloaded run; the local backend prunes itself
    but the remote orphans. This bounded sweep is hoisted to the fan-level pooled-ssh teardown so it runs once for the
    whole fan rather than once per check: it lists ``<workroot>`` a single time, ranks this host's own run dirs by the
    same ``(mtime, run_id)`` order the local retention uses, and ``rmtree``s all but ``artifact_retention`` newest — never
    another host's runs, because the run-id host token partitions a shared workroot into disjoint per-host namespaces.

    Returns:
        Pruned-run notes, or empty when nothing on this host's namespace exceeds the retention bound.
    """
    import asyncssh  # noqa: PLC0415  # lazy: bind asyncssh.Error before the except evaluates it; defer the ~83ms cold-start past import time

    target = settings.exec_target
    if not isinstance(target, Ssh):  # pragma: no cover  # the pooled-ssh teardown only prunes under an Ssh target
        return ()
    token, keep = settings.host_run_token, settings.artifact_retention
    try:
        stale = await _sweep_remote_runs(conn, target.workroot.rstrip("/"), token=token, keep=keep)
    except (OSError, asyncssh.Error) as exc:
        return (f"remote.prune.degraded {type(exc).__name__}: {str(exc)[:120]}",)
    return (f"remote.prune.removed runs={len(stale)}",) if stale else ()


async def _sweep_remote_runs(conn: asyncssh.SSHClientConnection, workroot: str, *, token: str, keep: int) -> tuple[str, ...]:
    """List ``<workroot>`` once, select this host's stale run dirs, and ``rmtree`` them over the pooled SFTP connection.

    The ``~`` in the configured workroot is resolved against the connection's realpath on this same prune client, so the
    sweep folds its own home resolution rather than opening a separate realpath channel.

    Returns:
        The run-dir basenames removed, oldest-first within this host's namespace.
    """
    rows: list[tuple[str, float]] = []
    async with await conn.start_sftp_client() as sftp:
        workroot = resolve_tilde(workroot, str(await sftp.realpath(".")))
        async for entry in sftp.scandir(workroot):
            name = str(entry.filename)
            if name not in {".", ".."} and entry.attrs.type == _SFTP_DIR_TYPE:
                rows.append((name, float(getattr(entry.attrs, "mtime", None) or 0.0)))
        stale = _stale_remote_runs(tuple(rows), token=token, keep=keep)
        for run_id in stale:
            await sftp.rmtree(f"{workroot}/{run_id}", ignore_errors=True)
    return stale


async def _probe_toolchain(conn: asyncssh.SSHClientConnection, argv: tuple[str, ...], *, path: str = "") -> tuple[str, str] | None:
    """Probe the remote PATH for the runner's leading tool before committing to the exec.

    The probe runs under the SAME injected ``PATH`` the exec exports, so probe and exec agree: a tool reachable only on the
    injected toolchain PATH (uv at ``~/.local/bin``, dotnet at ``/usr/local/dotnet``) never falsely reads as ``UNSUPPORTED``.

    Returns:
        ``(tool, detail)`` when the tool is absent, else ``None`` when present or unprobeable.
    """
    match argv:
        case (str() as tool, *_) if tool and "/" not in tool:
            export = f"PATH={shlex.quote(path)} " if path else ""
            probe = await conn.run(f"{export}command -v {shlex.quote(tool)}", encoding="utf-8", check=False)
            return None if probe.exit_status == 0 else (tool, f"remote toolchain missing: {tool!r} not on PATH")
        case _:
            # An absolute-path or empty leading token is self-locating; the exec surfaces its own ENOENT.
            return None


def _fold_receipt(
    done: Completed, target: Ssh, *, exit_status: int | None, signal: str, notes: tuple[str, ...] = (), pushed: int = 0, pulled: int = 0
) -> Completed:
    """Project the remote-execution facts onto the receipt's dedicated ``exec`` carrier.

    Returns:
        The receipt with an ``ExecReceipt`` stamping the target URL, host, exit status, signal, and transfer counts.
    """
    return msgspec.structs.replace(
        done, exec=ExecReceipt(target=target.url, host=target.host, exit_status=exit_status, signal=signal, pushed=pushed, pulled=pulled, notes=notes)
    )


def _scope_relative(scope: ArtifactScope) -> tuple[str, ...]:
    # Scope parts are root-down by construction: stripping the store root yields parts that agree across local and remote.
    tail = scope.path[len(scope.store.root) + 1 :] if scope.path.startswith(f"{scope.store.root}/") else scope.path
    return tuple(part for part in tail.split("/") if part)


async def _sftp_pull_scope(conn: asyncssh.SSHClientConnection, plan: _ExecPlan, scope: ArtifactScope) -> tuple[Artifact, ...]:
    # The agent landing store is always local-file: an SFTP execution backend cannot be read in-process, so artifacts come down to disk.
    # Offload pins the sftp backend under the run dir, so the backend root is the type's projection, never an inline re-derivation.
    offload = plan.settings.offload
    if offload is None:  # pragma: no cover  # the remote arm only reaches here under an Ssh target, which always derives an Offload
        return ()
    rel = _scope_relative(scope)
    landing = plan.settings.store(protocol="file", root="")
    remote_dir = "/".join((offload.backend.root.rstrip("/"), *rel))
    local_dir = Path(landing.ensure(*rel))
    async with await conn.start_sftp_client() as sftp:
        if not await sftp.exists(remote_dir):
            return ()
        # localpath is the parent so asyncssh recreates the run-id dir from the remote basename, agreeing with `rel`.
        await sftp.get(remote_dir, str(local_dir.parent), recurse=True, preserve=False)
    return await to_thread.run_sync(_landed_scope_artifacts, landing, local_dir, rel, plan.check.tool.name)


def _landed_scope_artifacts(landing: ArtifactStore, local_dir: Path, rel: tuple[str, ...], tool: str) -> tuple[Artifact, ...]:
    # Sync file-tree read of the freshly-landed scope: byte/line counts are real, paths are agent-local and scope-relative.
    return tuple(
        Artifact(
            id=f"{tool}-scope-{file.name}",
            kind=ArtifactKind.SCOPE,
            path=landing.path(*rel, *file.relative_to(local_dir).parts),
            bytes=file.stat().st_size,
            lines=_line_count(file.read_bytes()),
        )
        for file in sorted(local_dir.rglob("*"))
        if file.is_file()
    )


def _shared_read_scope(plan: _ExecPlan, scope: ArtifactScope) -> tuple[Artifact, ...] | None:
    """Fold scope artifacts straight from the shared cloud store the remote tool wrote, with zero byte transfer.

    The SHARED offload backend (s3/gs/gcs) is the same universal store the executor wrote scope artifacts to. The agent
    opens it directly, walks the scope-relative tree, and stamps ``Artifact`` rows at the shared scope-relative paths —
    byte counts come from backend metadata, no payload crosses the wire.

    Returns:
        Folded shared scope artifacts, or ``None`` when the remote tree is absent so the caller degrades to a note.
    """
    offload = plan.settings.offload
    if offload is None:  # pragma: no cover  # the remote arm only reaches here under an Ssh target, which always derives an Offload
        return None
    rel = _scope_relative(scope)
    store = plan.settings.store(protocol=offload.backend.protocol, root=offload.backend.root)
    tool = plan.check.tool.name
    # detail=True yields (path, info) rows; the isinstance guard narrows walk's union and skips directory markers.
    rows = tuple(row for row in store.walk(*rel, recursive=True, detail=True) if isinstance(row, tuple) and isinstance(row[1], dict))
    # An object store has no empty directories: a prefix with no keys is the absent-tree signal, folded to a note by the caller.
    artifacts = tuple(
        Artifact(id=f"{tool}-scope-{path.rsplit('/', 1)[-1]}", kind=ArtifactKind.SCOPE, path=path, bytes=size_from_info(info))
        for path, info in rows
        if info.get("type", "file") != "directory"
    )
    return artifacts or None


@contextlib.asynccontextmanager
async def _ssh_connection(target: Ssh) -> AsyncIterator[asyncssh.SSHClientConnection]:
    cache = _SSH_CACHE.get()
    match cache:
        case None:
            async with _connect(target) as conn:
                yield conn
        case _SshCache() as pooled:
            async with pooled.lock:
                cached = pooled.conns.get(target.url)
                if cached is None or cached.is_closed():
                    cached = pooled.conns[target.url] = await _connect_once(target)
            yield cached


@contextlib.asynccontextmanager
async def _connect(target: Ssh) -> AsyncIterator[asyncssh.SSHClientConnection]:
    conn = await _connect_once(target)
    try:
        yield conn
    finally:
        conn.close()
        await conn.wait_closed()


async def _connect_once(target: Ssh) -> asyncssh.SSHClientConnection:
    import asyncssh  # noqa: PLC0415  # lazy: ~83ms cold-start cost; defer past import time

    # The Ssh value object owns host/port/username/known_hosts; the engine owns the timeout/keepalive policy and the explicit
    # insecure opt-out: an `insecure` known_hosts token rebinds to asyncssh known_hosts=None and warns once before connecting.
    return await asyncssh.connect(
        **{**target.connect_kwargs, **_insecure_host_key(target.connect_kwargs.get("known_hosts"))},
        connect_timeout=_SSH_CONNECT_TIMEOUT,
        login_timeout=_SSH_CONNECT_TIMEOUT,
        keepalive_interval=_SSH_KEEPALIVE_INTERVAL_S,
        keepalive_count_max=_SSH_KEEPALIVE_COUNT_MAX,
    )


def _insecure_host_key(known_hosts: object) -> Mapping[str, None]:
    # Only the explicit `insecure` token disables host-key verification (-> known_hosts=None); every other value passes through.
    match known_hosts:
        case str() as token if token == _KNOWN_HOSTS_INSECURE:
            _LOG.warning("ssh.host_key_verification_disabled")
            return {"known_hosts": None}
        case _:
            return {}


def remote_command(argv: tuple[str, ...], *, cwd: str, env: Mapping[str, str]) -> str:
    """Build a shell-quoted remote command line.

    Returns:
        A ``cd <cwd> && <exports> <argv>`` line with every segment shell-quoted.
    """
    exports = tuple(f"{shlex.quote(k)}={shlex.quote(v)}" for k, v in env.items())
    body = " ".join((*exports, *(shlex.quote(part) for part in argv)))
    return f"cd {shlex.quote(cwd)} && {body}"


def _signal_name(exit_signal: object | None) -> str:
    # One owner decodes asyncssh's signalled-kill fact: a (name, *) tuple yields the receipt-bearing name, else the empty string.
    match exit_signal:
        case (str() as name, *_):
            return name
        case _:
            return ""


def ssh_outcome(status: int | None, signal: object | None) -> tuple[int, tuple[str, ...]]:
    """Resolve a remote exit status and optional signal into a numeric code plus receipt notes.

    Returns:
        The integer exit code (or synthetic 255 for a signalled kill) and any signal-name notes.
    """
    if isinstance(status, int):
        return status, ()
    name = _signal_name(signal)
    return _SSH_SIGNAL_STATUS, (f"ssh.signal={name}",) if name else ()


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


def _measure() -> Measurements:
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


def _diagnose(exc: BaseException) -> None:
    snap = dict(_measure().to_resources())
    _RESOURCE.set(tuple(snap.items()))
    span = trace.get_current_span()
    span.record_exception(exc)
    span.add_event(_FAULT_SNAPSHOT, attributes=snap)
    span.add_event(_RING_SNAPSHOT, attributes={"events": ring_recent()})


def apply_row_status(tool: Tool, done: Completed) -> Completed:
    """Apply a tool row's status policy to a process receipt.

    An ``empty-on-exit1`` row whose returncode-1 stdout decodes as the no-match document maps to ``EMPTY``
    (the tool signals "no match" through exit 1); non-document stdout on exit 1 stays a tool fault (FAILED).
    A row carrying an ``empty_signature`` maps its (returncode, marker) nothing-to-do receipt to ``EMPTY`` —
    a runner with no eligible work (pytest exit 5, vitest "No test files found") is an empty scope, never a defect.

    Returns:
        The receipt with the row-driven status applied, or unchanged when no policy matches.
    """
    empty = (ToolGroup.EMPTY_ON_EXIT1 in tool.groups and done.returncode == 1 and _is_match_document(done.stdout)) or (
        tool.empty_signature is not None and done.returncode == tool.empty_signature[0] and tool.empty_signature[1] in done.stdout + done.stderr
    )
    return msgspec.structs.replace(done, status=RailStatus.EMPTY) if empty else done


def _is_match_document(raw: bytes) -> bool:
    # The typed match-array decoder rejects a valid-JSON non-array on exit 1, keeping it a tool fault.
    try:
        AST_MATCHES.decode(raw or b"[]")
    except msgspec.MsgspecError:
        return False
    return True


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    import asyncssh  # noqa: PLC0415  # lazy: must bind before the try frame whose except evaluates asyncssh.Error (not an OSError subclass)

    t0 = time.monotonic()
    attempts = [1]
    argv: tuple[str, ...] = (check.tool.name,)

    try:
        match await _prepare_exec(check, settings, scope, routed, deadline):
            case Result(tag="ok", ok=prepared):
                argv = prepared.argv
                # The parser stamp carries the tool row's diagnostics family onto the receipt for the report fold.
                return (await _run_prepared(prepared, settings, scope, attempts)).map(
                    lambda done: apply_row_status(
                        check.tool, msgspec.structs.replace(done, duration_ms=(time.monotonic() - t0) * 1000.0, parser=check.tool.parser)
                    )
                )
            case Result(error=fault):
                return Error(fault)
    except (TimeoutError, FileNotFoundError, ValueError, OSError) as exc:
        return Error(_spawn_fault(argv, exc, attempts[0]))
    except asyncssh.Error as exc:
        return Error(_spawn_fault(argv, exc, attempts[0]))


def _exec_cwd(check: Check, settings: AssaySettings) -> str:
    # Local keeps the staging worktree or anchored root; the Ssh case projects the remote run dir the push tree lands under.
    match settings.exec_target:
        case Ssh() as target:
            return target.remote_workroot(settings.run_id)
        case _:
            return str(UPath(check.cwd or settings.local_root).path)


async def _prepare_exec(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[_PreparedExec, Fault]:
    # Staging rmtree/copytree are filesystem-bound; the thread hop keeps them off the event loop.
    match await to_thread.run_sync(_materialize, check, settings):
        case Result(tag="ok", ok=prepared):
            check = prepared
        case Result(error=fault):
            return Error(fault)
    if check.tool.claim in _HOST_BOUND_CLAIMS and settings.exec_target:
        return Error(
            Fault((check.tool.name, check.tool.claim.value), status=RailStatus.UNSUPPORTED, message="host-bound tools require local execution")
        )
    match _argv(check, routed, settings=settings, scope=scope):
        case Result(tag="ok", ok=argv):
            pass
        case Result(error=fault):
            return Error(fault)
    bound = deadline if deadline is not None else (time.monotonic() + check.tool.timeout if check.tool.timeout is not None else None)
    # The remote cwd is the run dir the push lands under; an agent-local absolute path would not exist on the remote host.
    cwd = _exec_cwd(check, settings)
    env = await to_thread.run_sync(_apphost, check.tool, _overlay(check.tool, settings, scope), abandon_on_cancel=True)
    propagate.inject(env)
    trace.get_current_span().set_attribute("exec.target", settings.exec_target.url if isinstance(settings.exec_target, Ssh) else "")
    return Ok(
        _PreparedExec(
            check=check,
            argv=argv,
            cwd=cwd,
            env=env,
            bound=bound,
            thread_limiter=_FAN_LIMITER.get() or anyio.CapacityLimiter(governed_concurrency(settings, (check,))),
        )
    )


async def _run_prepared(
    prepared: _PreparedExec, settings: AssaySettings, scope: ArtifactScope | None, attempts: list[int]
) -> Result[Completed, Fault]:
    async with _dotnet_slot(prepared.check, settings, prepared.bound) as slot:
        match slot:
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok", ok=slot_notes):
                done = await _execute_retrying(
                    prepared.check,
                    settings,
                    scope,
                    argv=prepared.argv,
                    cwd=prepared.cwd,
                    env=prepared.env,
                    thread_limiter=prepared.thread_limiter,
                    deadline=prepared.bound,
                    attempts=attempts,
                )
                return Ok(msgspec.structs.replace(done, notes=(*slot_notes, *done.notes)) if slot_notes else done)
            case never:  # pragma: no cover
                return Error(Fault(prepared.argv, status=RailStatus.FAULTED, message=str(never)))


def _spawn_fault(argv: tuple[str, ...], exc: BaseException, attempts: int) -> Fault:
    # Spawn-fault status is derived from the boundary class, not message text.
    _diagnose(exc)
    message = "deadline exceeded" if isinstance(exc, TimeoutError) else str(exc)
    stamped = f"{message} [attempts={attempts}]" if attempts > 1 else message
    match exc:
        case TimeoutError():
            return Fault(argv, status=RailStatus.TIMEOUT, message=stamped)
        case FileNotFoundError():
            return Fault(argv, status=RailStatus.UNSUPPORTED, message=stamped)
        case _:
            return Fault(argv, status=RailStatus.FAULTED, message=stamped)


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
    # Name the retry context so stamina telemetry attributes attempts to the tool, not "<context block>".
    retrying = stamina.retry_context(on=retry_predicate(check, deadline), attempts=3, timeout=_retry_timeout(deadline))
    async for attempt in retrying.with_name(check.tool.name, (), {}):
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
                    spill_cap=settings.capture_spill_bytes,
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
    # Compose per invocation for a fresh span; no variance-safe overload exists for async _Woven.
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


@dataclass(frozen=True, slots=True)
class EngineExecutor:
    """Production Executor over the engine spawn rails; the registry weave constructs the one bound instance."""

    def run(  # noqa: PLR6301  # port method: the instance IS the capability rails receive
        self, check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> Result[Completed, Fault]:
        """Run one check under a single event loop.

        Returns:
            Completed receipt, or a fault when spawn, lease, or timeout handling fails.
        """
        return run_check(check, settings=settings, scope=scope, routed=routed, deadline=deadline)

    def fan(  # noqa: PLR6301  # port method: the instance IS the capability rails receive
        self, checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> tuple[Result[Completed, Fault], ...]:
        """Run checks concurrently and preserve input order.

        Returns:
            One completed-or-fault result per input check.
        """
        return fan_out(checks, settings=settings, scope=scope, routed=routed, deadline=deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Run checks concurrently and preserve input order.

    ``deadline`` is a shared absolute ``time.monotonic()`` ceiling; expired checks yield timeout faults in their slots.

    Returns:
        One completed-or-fault result per input check.
    """

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        _FOREIGN_DOTNET_MEMO.clear()  # fresh dotnet census per fan; the TTL memo only spans one fan's slot-poll window
        limit = governed_concurrency(settings, checks)
        results: dict[int, Result[Completed, Fault]] = {}
        token = _FAN_LIMITER.set(anyio.CapacityLimiter(limit))
        try:
            with _TRACER.start_as_current_span("assay.fan_out") as parent:
                parent.set_attribute("assay.checks_total", len(checks))
                parent.set_attribute("assay.checks_concurrency", limit)
                async with _pooled_ssh(settings):
                    results.update(await _fan_schedule(checks, settings=settings, scope=scope, routed=routed, deadline=deadline, limit=limit))
        finally:
            _FAN_LIMITER.reset(token)
        return tuple(_total(results.get(i)) for i in range(len(checks)))

    return anyio.run(_run)


async def _fan_schedule(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None, limit: int
) -> dict[int, Result[Completed, Fault]]:
    _LOG.info("fan.schedule", total=len(checks), concurrency=limit)
    trace.get_current_span().add_event("fan.schedule", attributes={"fan.total": len(checks), "fan.concurrency": limit})
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
            _ = tg.start_soon(_fan_worker, work_recv, result_send, settings, scope, routed, deadline)
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
async def _pooled_ssh(settings: AssaySettings) -> AsyncIterator[None]:
    import asyncssh  # noqa: PLC0415  # lazy: the finally except evaluates asyncssh.Error; must bind before the try frame

    cache = _SshCache({}, anyio.Lock())
    token = _SSH_CACHE.set(cache)
    try:
        yield None
    finally:
        _SSH_CACHE.reset(token)
        # Once-per-fan remote retention sweep, before the connections close, bounding the remote workroot's run-dir pile.
        await _fan_prune(cache, settings)
        for conn in cache.conns.values():
            conn.close()
        for conn in cache.conns.values():
            try:
                await conn.wait_closed()
            except (OSError, asyncssh.Error) as exc:
                _LOG.warning("ssh.close_failed", error=str(exc)[:200])


async def _fan_prune(cache: _SshCache, settings: AssaySettings) -> None:
    # Every cached connection ran offloaded checks under this fan's exec target, so one shielded prune per pooled
    # connection sweeps this host's stale run dirs exactly once for the whole fan rather than once per check.
    if not isinstance(settings.exec_target, Ssh):
        return
    for conn in cache.conns.values():
        with anyio.CancelScope(shield=True):
            notes = await _remote_prune(conn, settings)
        if notes:
            _LOG.info("remote.prune", run_id=settings.run_id, notes=notes)


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
async def _dotnet_slot(  # noqa: PLR0912, PLR0914, PLR0915  # closed resource-state ladder + machine-pool census; static rail work only surfaces its receipts
    check: Check, settings: AssaySettings, deadline: float | None
) -> AsyncIterator[Result[tuple[str, ...], Fault]]:
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
        remaining = _remaining(deadline)
        if remaining is not None and remaining <= _DOTNET_SLOT_POLL_S:
            yield Error(Fault((check.tool.name,), status=RailStatus.TIMEOUT, message="dotnet slot wait deadline exceeded"))
            return
        await anyio.sleep(min(_DOTNET_SLOT_POLL_S, remaining or _DOTNET_SLOT_POLL_S))


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


# --- [COMPOSITION] ----------------------------------------------------------------------


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
        _diagnose(exc)
        return Error(Fault((), status=RailStatus.FAULTED, message=str(exc)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "_RESOURCE",
    "ByteRecv",
    "Captured",
    "EngineExecutor",
    "Executor",
    "Measurements",
    "StalledProcess",
    "WriteSink",
    "apply_row_status",
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
    "proc_dead",
    "remote_command",
    "retry_predicate",
    "run_check",
    "run_check_async",
    "splice_command",
    "ssh_outcome",
]
