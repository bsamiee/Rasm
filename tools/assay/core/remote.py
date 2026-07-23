"""Remote execution transport for Assay checks over SSH.

Pooled asyncssh connections, lane-scoped working-tree push, remote exec with drained streams, artifact
pull by offload strategy, host-scoped run-dir pruning, and the remote receipt fold.
"""

import contextlib
from contextvars import ContextVar
from dataclasses import dataclass, replace
from functools import reduce
import os
from pathlib import Path, PurePosixPath
import shlex
from typing import TYPE_CHECKING

import anyio
from anyio import to_thread  # ty mis-resolves anyio.to_thread without an explicit submodule import
import msgspec
import structlog

from tools.assay.composition.settings import PullStrategy, resolve_tilde, run_id_host_token, Ssh
from tools.assay.composition.store import ArtifactScope, size_from_info
from tools.assay.core.govern import Captured, captured_outputs, drain_pair, line_count, recv_ssh, stream_artifacts
from tools.assay.core.model import (  # ruff:ignore[typing-only-first-party-import]  # beartype resolves receipt annotations at runtime under PEP 649
    Artifact,
    ArtifactKind,
    Completed,
    ExecReceipt,
    RailStatus,
    receipt,
    Runner,
)
from tools.assay.core.routing import discover_async, parse_csproj


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Mapping

    import asyncssh

    from tools.assay.composition.settings import AssaySettings
    from tools.assay.composition.store import ArtifactStore
    from tools.assay.core.govern import ExecPlan


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.remote")

# --- [OPERATIONS] -----------------------------------------------------------------------

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


@dataclass(frozen=True, slots=True)
class _SshCache:
    conns: dict[str, asyncssh.SSHClientConnection]
    lock: anyio.Lock


_SSH_CACHE: ContextVar[_SshCache | None] = ContextVar("assay_ssh_cache", default=None)


@dataclass(frozen=True, slots=True)
class _Outcome:
    streams: dict[str, Captured]
    exit_status: int | None
    signal: object | None  # raw asyncssh exit_signal: the (name, *) tuple or None; decoded once by ssh_outcome/_signal_name
    notes: tuple[str, ...] = ()


@dataclass(frozen=True, slots=True)
class _Pulled:
    artifacts: tuple[Artifact, ...]
    count: int
    notes: tuple[str, ...]


@dataclass(frozen=True, slots=True)
class _Transfer:
    conn: asyncssh.SSHClientConnection
    plan: ExecPlan
    pushed: int
    notes: tuple[str, ...]

    async def pull(self, streams: Mapping[str, Captured]) -> _Pulled:
        """Reach the tool-written remote scope tree after the remote exec, dispatching on the offload pull strategy.

        ``TRANSFER`` downloads the tree byte-for-byte over sftp into the agent-local landing store; ``SHARED`` reads the
        same universal paths the remote tool wrote to a cloud object store directly, with zero byte transfer.

        Returns:
            Folded scope artifacts, the pulled-file count, and any degrade note.
        """
        captured = stream_artifacts(self.plan.scope, self.plan.settings, self.plan.check, streams)
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


async def _resolve_remote_plan(plan: ExecPlan, target: Ssh, conn: asyncssh.SSHClientConnection) -> ExecPlan:
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


async def run_remote(plan: ExecPlan, target: Ssh) -> Completed:
    """Run one composed plan on the Ssh target: probe, push, exec, pull, and fold the remote receipt.

    Returns:
        Completed receipt carrying the remote exit, streams, transfer notes, and ExecReceipt facts.
    """
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


def _remote_done(plan: ExecPlan, target: Ssh, transfer: _Transfer, outcome: _Outcome, pulled: _Pulled) -> Completed:
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


async def _remote_exec(conn: asyncssh.SSHClientConnection, plan: ExecPlan, command: str) -> _Outcome:
    import asyncssh  # ruff:ignore[import-outside-top-level]  # lazy: ~83ms cold-start cost; defer past import time

    # The raw asyncssh exit_signal ((name, *) tuple or None) rides _Outcome verbatim; ssh_outcome/_signal_name own the decode.
    match plan.streaming:
        case True:
            # No stall telemetry on this branch: psutil cannot inspect remote pids across the SSH boundary.
            proc = await conn.create_process(command, encoding=None, stdin=asyncssh.DEVNULL)
            try:
                drain_notes = list[str]()
                streams = await drain_pair(plan, recv_ssh(proc.stdout, plan.chunk), recv_ssh(proc.stderr, plan.chunk), proc.wait, drain_notes)
                return _Outcome(streams, proc.exit_status, getattr(proc, "exit_signal", None), tuple(drain_notes))
            finally:
                with anyio.CancelScope(shield=True):
                    proc.close()
                    await proc.wait_closed()
        case False:
            run = await conn.run(command, encoding=None, check=False)
            streams = captured_outputs(plan, _as_bytes(run.stdout), _as_bytes(run.stderr))
            return _Outcome(streams, run.exit_status, getattr(run, "exit_signal", None))


@contextlib.asynccontextmanager
async def _remote_transfer(conn: asyncssh.SSHClientConnection, plan: ExecPlan) -> AsyncIterator[_Transfer]:
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


async def _push_repo(conn: asyncssh.SSHClientConnection, plan: ExecPlan, manifest: tuple[str, ...]) -> tuple[int, tuple[str, ...]]:
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
    import asyncssh  # ruff:ignore[import-outside-top-level]  # lazy: bind asyncssh.Error before the except evaluates it; defer the ~83ms cold-start

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
            handler = lambda exc: failures.append((f"remote.push.failed {type(exc).__name__}: {str(exc)[:120]}", 1))  # ruff:ignore[lambda-assignment]
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


async def _push_manifest(plan: ExecPlan) -> tuple[str, ...]:
    # git ls-files -z is NUL-delimited so paths with spaces/newlines survive; the agent-local root is the manifest source.
    # The full git universe is then lane-scoped to the build closure so a remote run pushes the closure, never the whole tree.
    listed = await discover_async(_PUSH_MANIFEST_ARGV, root=plan.settings.local_root, limit_s=_TRANSFER_BUDGET_S)
    universe = listed.map(lambda out: tuple(p for p in out.decode(errors="replace").split("\x00") if p)).default_value(())
    return _lane_manifest(plan, universe)


def _lane_manifest(plan: ExecPlan, universe: tuple[str, ...]) -> tuple[str, ...]:
    # One dispatch on the lane's runner scopes the universe to the build closure: a C# closure is the transitive
    # ProjectReference set plus root build config; a Python lane is package + tests + config; every other lane keeps the
    # full universe (it carries no project graph to scope against). Naive subtree-scope is rejected: cross-project refs
    # would be dropped, so the C# arm walks ProjectReference transitively rather than trusting directory containment alone.
    match plan.check.tool.runner:
        case Runner.DOTNET:
            return _csharp_manifest(plan, universe)
        case Runner.UV | Runner.PNPM:
            return _python_manifest(universe)
        case _:
            return universe


def _csharp_manifest(plan: ExecPlan, universe: tuple[str, ...]) -> tuple[str, ...]:
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


def _csharp_seeds(plan: ExecPlan) -> frozenset[str]:
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
    import asyncssh  # ruff:ignore[import-outside-top-level]  # lazy: bind asyncssh.Error before the except evaluates it; defer the ~83ms cold-start past import time

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


async def _sftp_pull_scope(conn: asyncssh.SSHClientConnection, plan: ExecPlan, scope: ArtifactScope) -> tuple[Artifact, ...]:
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
            lines=line_count(file.read_bytes()),
        )
        for file in sorted(local_dir.rglob("*"))
        if file.is_file()
    )


def _shared_read_scope(plan: ExecPlan, scope: ArtifactScope) -> tuple[Artifact, ...] | None:
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
    import asyncssh  # ruff:ignore[import-outside-top-level]  # lazy: ~83ms cold-start cost; defer past import time

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


@contextlib.asynccontextmanager
async def pooled_ssh(settings: AssaySettings) -> AsyncIterator[None]:
    """Pool SSH connections for one fan; teardown prunes this host's stale remote run dirs and closes the pool."""
    import asyncssh  # ruff:ignore[import-outside-top-level]  # lazy: the finally except evaluates asyncssh.Error; must bind before the try frame

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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["pooled_ssh", "remote_command", "run_remote", "ssh_outcome"]
