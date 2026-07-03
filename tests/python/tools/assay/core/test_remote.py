"""Remote laws: SSH round-trips, manifest push, scope pull, prune, and offload transfer semantics.

Boundary surfaces ride the socketpair SSH double, the chrooted loopback SFTP server, and a moto S3 loopback.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextlib
import os
from pathlib import Path
import time
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
from dirty_equals import Contains
import fsspec
import msgspec
import pytest
from upath import UPath

from tests.python._testkit.env import provision, SshHost
from tests.python._testkit.spec import assert_ok

# Hypothesis resolves fixture annotations at collection time under PEP 649.
from tests.python.tools.assay.kit import AssayHarness  # noqa: TC001
from tools.assay.composition.settings import AssaySettings, PullStrategy, run_id_host_token, Ssh
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.exec import fan_out, run_check
from tools.assay.core.govern import ExecPlan, recv_ssh
from tools.assay.core.model import ArtifactKind, Check, Claim, Input, Language, Mode, receipt, Runner, Tool
import tools.assay.core.remote as remote_mod
from tools.assay.core.remote import pooled_ssh, remote_command, run_remote, ssh_outcome
from tools.assay.core.routing import Routed, Scope


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Awaitable, Iterator, Mapping

    import asyncssh
    from expression import Result
    from fsspec.spec import AbstractFileSystem

    from tests.python._testkit.env import Provisioned
    from tools.assay.composition.store import ArtifactStore
    from tools.assay.core.model import Completed, Fault

    type SshEnv = Provisioned[Awaitable[asyncssh.SSHClientConnection]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# recv_ssh is govern's ssh drain arm; the remote streaming round-trip is its only real driver.
COVERS: tuple[object, ...] = (pooled_ssh, recv_ssh, remote_command, run_remote, ssh_outcome)

_ROUTED_CHANGED = Routed(language=Language.CSHARP, scope=Scope.CHANGED)

# Scope-tree seed shared by the remote-write seeding and the landed-artifact assertions; coverage.xml lives under sarif/.
_SEED_FILES: tuple[tuple[str, bytes], ...] = (("results.sarif", b'{"runs":[]}\n'), ("coverage.xml", b"<coverage/>\nline2\n"))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _stream_tool(name: str, command: tuple[str, ...]) -> Tool:
    """Build a BUILD-mode DIRECT tool that exercises the remote streaming arm.

    Returns:
        BUILD-mode DIRECT tool over ``command``.
    """
    return Tool(name, Runner.DIRECT, command, Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)


def _remote_settings(root: Path | str, **overrides: object) -> AssaySettings:
    """Build validated SSH-target settings rooted at ``root``.

    Returns:
        Settings whose ``exec_target`` is the loopback SSH url.
    """
    return AssaySettings.model_validate({"root": UPath(root), "exec_target": "ssh://x@127.0.0.1", "exec_known_hosts": None, **overrides})


def _plan(settings: AssaySettings, *, cwd: str = "", scope: ArtifactScope | None = None, argv: tuple[str, ...] = ("echo",)) -> ExecPlan:
    """Build a minimal non-streaming ``ExecPlan`` for transfer-bracket laws.

    Returns:
        Plan carrying the settings/scope/cwd the transfer dispatches on.
    """
    return ExecPlan(
        argv=argv,
        check=Check(tool=_stream_tool("remote-plan-law", ("/bin/echo", "x"))),
        cwd=cwd,
        env={},
        settings=settings,
        scope=scope,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )


async def _git_seed(root: Path, files: Mapping[str, bytes]) -> None:
    # A minimal real git repo makes `git ls-files` the manifest source: the push test pushes exactly the tracked set.
    for rel, payload in files.items():
        (root / rel).parent.mkdir(parents=True, exist_ok=True)
        (root / rel).write_bytes(payload)
    ident = {"GIT_AUTHOR_NAME": "t", "GIT_AUTHOR_EMAIL": "t@t", "GIT_COMMITTER_NAME": "t", "GIT_COMMITTER_EMAIL": "t@t"}
    env = {**os.environ, "GIT_CONFIG_GLOBAL": "/dev/null", "GIT_CONFIG_SYSTEM": "/dev/null", **ident}  # noqa: TID251  # subprocess env clone for the test-local git seed
    for argv in (("git", "init", "-q"), ("git", "add", "-A"), ("git", "-c", "commit.gpgsign=false", "commit", "-q", "-m", "seed", "--no-verify")):
        await anyio.run_process([*argv], cwd=str(root), env=env, check=True)


# --- [PROJECTIONS]


def test_ssh_outcome_projects_status_and_signal() -> None:
    """``ssh_outcome``: integer exits pass through (no notes); a signal kill maps to 255 with the signal name."""
    rows: tuple[tuple[str, int | None, object | None, tuple[int, tuple[str, ...]]], ...] = (
        ("clean-exit", 0, None, (0, ())),
        ("nonzero-exit", 2, None, (2, ())),
        ("signal-no-name", None, None, (255, ())),
        ("signal-named", None, ("TERM", False, "", ""), (255, ("ssh.signal=TERM",))),
    )
    for label, exit_status, sig, expected in rows:
        assert ssh_outcome(exit_status, sig) == expected, f"{label}: outcome drifted"
    # _as_bytes is the sibling reply projection: bytes pass through, None empties, str encodes.
    assert (remote_mod._as_bytes(b"x"), remote_mod._as_bytes(None), remote_mod._as_bytes("s")) == (b"x", b"", b"s")


@pytest.mark.parametrize(
    "argv, cwd, env, fragments",
    [
        (("dotnet", "test"), "/work", {}, ("cd /work &&", "dotnet test")),
        (("ruff", "check", "."), "/repo root", {"PYTHONHASHSEED": "0"}, ("cd '/repo root'", "PYTHONHASHSEED=0", "ruff check .")),
        (("echo", "a b"), "/w", {"K": "v w"}, ("cd /w &&", "K='v w'", "echo 'a b'")),
    ],
)
def test_remote_command_shell_quotes_cwd_env_argv(argv: tuple[str, ...], cwd: str, env: dict[str, str], fragments: tuple[str, ...]) -> None:
    """``remote_command`` shell-quotes the cwd prefix, env exports, and every argv segment into one ``cd ... && ...`` line."""
    command = remote_command(argv, cwd=cwd, env=env)
    assert command == Contains(*fragments), f"missing fragment in {command!r}"


def test_fold_receipt_projects_exec_facts_onto_completed() -> None:
    """``_fold_receipt`` stamps the remote target URL/host, exit status, signal, and push/pull counts onto ``Completed.exec``."""
    target = Ssh(host="vps", port=22, user="root")
    done = remote_mod._fold_receipt(receipt(("dotnet", "test"), 0), target, exit_status=0, signal="", notes=("n",), pushed=3, pulled=2)
    assert done.exec is not None
    assert (done.exec.target, done.exec.host, done.exec.exit_status) == ("ssh://root@vps:22", "vps", 0)
    assert (done.exec.pushed, done.exec.pulled, done.exec.notes) == (3, 2, ("n",))


# --- [SSH_ROUND_TRIP]


@pytest.mark.anyio
async def test_run_check_remote_round_trips_through_ssh_double(assay_root: AssayHarness, ssh_env: SshEnv) -> None:
    """The non-streaming remote arm shell-quotes argv and returns the ssh double reply."""
    remote = assay_root.remote(ssh_env.url)
    check = Check(tool=Tool("remote-echo", Runner.DIRECT, ("/bin/echo", "hello"), Input.NONE, Language.CSHARP, Claim.STATIC), cwd=assay_root.root)
    # Bridge run_check's owned event loop through a thread under anyio tests.
    outcome = await anyio.to_thread.run_sync(  # ty: ignore[unresolved-attribute]
        lambda: run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)
    )
    done = assert_ok(outcome)
    assert (b"remote-ok:" in done.stdout, done.returncode) == (True, 0), f"ssh double reply missing from {done.stdout!r}"


@pytest.mark.anyio
@pytest.mark.parametrize("scoped", [False, True], ids=["non-persisted", "scoped-persists-artifact"])
async def test_run_check_remote_streaming_round_trips(
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
    ssh_env: SshEnv,
) -> None:
    """The remote streaming arm drains the ssh double reply and tail-caps the receipt; the scoped row persists the artifact."""
    remote = assay_root.remote(ssh_env.url)
    scope = assay_root.scope(Claim.STATIC) if scoped else None
    name = "remote-scoped-stream-law" if scoped else "remote-stream-law"
    check = Check(tool=_stream_tool(name, ("/bin/echo", "stream-ok")), cwd=assay_root.root)
    outcome = await anyio.to_thread.run_sync(  # ty: ignore[unresolved-attribute]
        lambda: run_check(check, settings=remote, scope=scope, routed=_ROUTED_CHANGED)
    )
    done = assert_ok(outcome)
    assert (b"/bin/echo" in done.stdout, done.returncode) == (True, 0), f"streamed remote command not in tail: {done.stdout!r}"
    match scope:
        case None:
            pass
        case _:
            artifact = next((a for a in done.artifacts if a.id.startswith(f"{name}-") and a.id.endswith("-out")), None)
            assert artifact is not None, f"scoped remote stream emitted no artifact: {done.artifacts!r}"
            assert b"remote-ok:" in scope.store.read_path(artifact.path), "persisted remote artifact lost the ssh double reply"


@pytest.mark.anyio
async def test_fan_out_remote_pools_ssh_connection(assay_root: AssayHarness, ssh_env: SshEnv) -> None:
    """``fan_out`` over a remote runner pools one ssh connection across workers and closes it on scope exit."""
    remote = assay_root.remote(ssh_env.url)
    base = Tool("remote-fan-law", Runner.DIRECT, ("/bin/echo", "hi"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.CHECK)
    checks = tuple(Check(tool=msgspec.structs.replace(base, name=f"remote-fan-{i}"), cwd=assay_root.root) for i in range(2))

    def _sync() -> tuple[Result[Completed, Fault], ...]:
        return fan_out(checks, settings=remote, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 10.0)

    results = await anyio.to_thread.run_sync(_sync)  # ty: ignore[unresolved-attribute]
    assert len(results) == 2, f"fan_out lost a remote slot: {results!r}"
    for index, result in enumerate(results):
        assert b"remote-ok:" in assert_ok(result).stdout, f"remote slot {index} missing ssh double reply"


@pytest.mark.anyio
@pytest.mark.parametrize("exc_factory", ["asyncssh", "oserror"])
async def test_pooled_ssh_logs_close_failures(exc_factory: str) -> None:
    """``_pooled_ssh`` logs close failures from ``asyncssh.Error`` and ``OSError``; a broken close never aborts sibling cleanup."""
    import asyncssh  # noqa: PLC0415  # deferred: double raises asyncssh.Error directly

    boom: BaseException = asyncssh.Error(code=1, reason="close failed") if exc_factory == "asyncssh" else OSError("socket reset on close")
    closed = [False]

    def _mark_closed() -> None:
        closed[0] = True

    async def _wait_closed() -> None:  # noqa: RUF029  # asyncssh-compatible wait_closed double
        raise boom

    conn = SimpleNamespace(close=_mark_closed, wait_closed=_wait_closed)
    # Local-target settings: the teardown's once-per-fan prune is Ssh-gated, so it skips the structural conn double here.
    settings = AssaySettings(exec_known_hosts=None)
    async with pooled_ssh(settings):
        cache = remote_mod._SSH_CACHE.get()
        assert cache is not None, "_pooled_ssh did not seed the connection cache"
        cache.conns["ssh://x@host:22"] = conn  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # structural conn double
    assert closed[0] is True, "pooled connection close was not attempted before wait_closed faulted"


@pytest.mark.anyio
async def test_probe_toolchain_faults_unsupported_on_missing_remote_tool() -> None:
    """``_probe_toolchain`` returns ``(tool, detail)`` when ``command -v`` exits non-zero, else ``None`` for a present tool."""

    def _handler(command: str) -> tuple[str, int]:
        # The probe runs `command -v <tool>`; the double reports the missing tool as exit 1, a present tool as exit 0.
        return ("", 1) if "missing-tool" in command else ("/usr/bin/git\n", 0)

    conn = await provision(SshHost(handler=_handler)).client_factory()
    try:
        absent = await remote_mod._probe_toolchain(conn, ("missing-tool", "build"))
        present = await remote_mod._probe_toolchain(conn, ("git", "ls-files"))
        absolute = await remote_mod._probe_toolchain(conn, ("/bin/echo", "x"))
    finally:
        conn.close()
        await conn.wait_closed()
    assert absent is not None, "a missing remote tool must surface a probe result"
    assert absent[0] == "missing-tool", f"missing remote tool must surface its name: {absent!r}"
    assert present is None, f"a present remote tool must probe clean: {present!r}"
    assert absolute is None, "an absolute-path command is self-locating and must skip the probe"


# --- [TRANSFER_BRACKET]


@pytest.mark.anyio
async def test_remote_transfer_pushes_manifest_then_pulls_scope_tree(  # one end-to-end transfer law: push the git manifest, run, pull the scope tree, assert receipt counts
    assay_root: AssayHarness, tmp_path: Path
) -> None:
    """``_remote_transfer`` pushes the git-tracked working tree to ``<workroot>/<run_id>`` then pulls the scope tree back.

    The push leg lands exactly the ``git ls-files`` manifest under the remote run dir; the pull leg downloads the
    tool-written scope tree to the agent-local store at the same scope-relative parts, with real byte/line counts,
    no absolute host path in ``Artifact.path``, and the receipt carrying pushed/pulled counts.
    """
    remote = _remote_settings(assay_root.root, exec_workroot="/work")
    offload = remote.offload
    assert offload is not None
    backend_root, run_id, claim = offload.backend.root, remote.run_id, Claim.STATIC.value
    manifest = {"Workspace.slnx": b"", "src/a.cs": b"class A;\n", "src/nested/b.txt": b"b\n"}
    await _git_seed(Path(str(remote.local_root)), manifest)
    remote_cwd = offload.target.remote_workroot(run_id)

    # The agent landing store is local-file; the scope roots there so `_scope_relative` yields the parts the remote tool used.
    landing: ArtifactStore = remote.store(protocol="file", root="")
    scope = ArtifactScope(store=landing, path=landing.path(claim, run_id), dotnet_flags=())
    remote_scope = tmp_path / backend_root.lstrip("/") / claim / run_id
    (remote_scope / "sarif").mkdir(parents=True)
    for name, payload in _SEED_FILES:
        (remote_scope / ("sarif" if name == "coverage.xml" else ".") / name).write_bytes(payload)

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()
    try:
        async with remote_mod._remote_transfer(conn, _plan(remote, cwd=remote_cwd, scope=scope)) as transfer:
            pass  # exec is a no-op here: the push runs on bracket entry, the pull on transfer.pull
        pulled = await transfer.pull({})
        run_dir = tmp_path / "work" / run_id
        landed_manifest = {
            rel
            for p in run_dir.rglob("*")
            if p.is_file()
            for rel in (str(p.relative_to(run_dir)).replace("\\", "/"),)
            if not rel.startswith(".artifacts/")
        }
    finally:
        conn.close()
        await conn.wait_closed()

    assert transfer.notes == (), f"a clean push must add no degrade note: {transfer.notes!r}"
    assert transfer.pushed == len(manifest), f"push count != manifest size: {transfer.pushed} != {len(manifest)}"
    assert landed_manifest == set(manifest), f"pushed tree diverged from the git manifest: {landed_manifest} != {set(manifest)}"
    assert pulled.notes == (), f"a clean sftp pull must add no degrade note: {pulled.notes!r}"
    assert pulled.count == len(_SEED_FILES), f"pull count != scope tree size: {pulled.count} != {len(_SEED_FILES)}"
    by_name = {a.path.rsplit("/", 1)[-1]: a for a in pulled.artifacts}
    assert {"results.sarif", "coverage.xml"} <= set(by_name), f"sftp pull lost a scope file: {by_name.keys()}"
    for name, expected in _SEED_FILES:
        row = by_name[name]
        assert (row.kind, row.bytes) == (ArtifactKind.SCOPE, len(expected)), f"wrong kind/size for {name}: {row!r}"
        assert f"/{claim}/{run_id}/" in f"/{row.path}/", f"scope-relative path lost for {name}: {row.path!r}"
        # The agent-local landing path is recorded, never the remote backend root that hosted the source tree.
        assert not row.path.startswith(backend_root.lstrip("/")), f"remote backend path leaked into {name}: {row.path!r}"
        assert f"{backend_root}/" not in f"/{row.path}", f"remote backend path leaked into {name}: {row.path!r}"
        assert landing.read_path(row.path) == expected, f"landed bytes diverged for {name}"
    assert by_name["coverage.xml"].lines == 2, "line count not derived from the real landed bytes"


@pytest.mark.anyio
async def test_push_repo_pipelines_nested_tree_preserving_structure(assay_root: AssayHarness, tmp_path: Path) -> None:
    """``_push_repo`` pipelines per-directory puts concurrently yet lands the exact ``git ls-files`` tree, no flattening.

    Many single-file directories plus repeated basenames stress the concurrent push: the file set must land at
    ``<abs-workroot>/<run_id>/<relpath>`` with the relative tree intact and no literal ``~`` anywhere.
    """
    remote = _remote_settings(assay_root.root, exec_workroot="/work")
    run_id = remote.run_id
    manifest = {
        "Workspace.slnx": b"slnx\n",
        "a.txt": b"root-a\n",
        ".claude/skills/one/SKILL.md": b"one\n",
        ".claude/skills/two/SKILL.md": b"two\n",
        ".claude/skills/three/SKILL.md": b"three\n",
        "docs/x/index.md": b"x\n",
        "docs/y/index.md": b"y\n",
        "docs/y/deep/leaf.md": b"leaf\n",
    }
    await _git_seed(Path(str(remote.local_root)), manifest)
    remote_cwd = remote.exec_target.remote_workroot(run_id) if isinstance(remote.exec_target, Ssh) else ""
    assert "~" not in remote_cwd, f"workroot must be absolute, not a literal tilde: {remote_cwd!r}"

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()
    try:
        pushed, notes = await remote_mod._push_repo(conn, _plan(remote, cwd=remote_cwd), tuple(sorted(manifest)))
        run_dir = tmp_path / "work" / run_id
        landed = {
            rel: (run_dir / rel).read_bytes() for p in run_dir.rglob("*") if p.is_file() for rel in (str(p.relative_to(run_dir)).replace("\\", "/"),)
        }
    finally:
        conn.close()
        await conn.wait_closed()

    assert notes == (), f"a clean concurrent push must add no failure note: {notes!r}"
    assert pushed == len(manifest), f"push count != manifest size: {pushed} != {len(manifest)}"
    assert landed == manifest, f"concurrent push flattened or dropped the nested tree: {set(landed)} != {set(manifest)}"


@pytest.mark.anyio
async def test_remote_transfer_keeps_exec_cancellable_under_deadline(
    assay_root: AssayHarness, tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A wedged remote exec inside the transfer bracket is reclaimed by the check deadline; the push leg completes shielded.

    The push shield must scope to the push leg only — never the bracketed exec — or a hung remote tool runs
    unbounded and defeats the deadline.
    """
    remote = _remote_settings(assay_root.root, exec_workroot="/work")
    offload = remote.offload
    assert offload is not None
    target = offload.target
    run_id, remote_cwd = remote.run_id, target.remote_workroot(remote.run_id)
    await _git_seed(Path(str(remote.local_root)), {"Workspace.slnx": b"", "src/a.cs": b"class A;\n"})

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()

    @contextlib.asynccontextmanager
    async def _fixed_conn(_target: object) -> AsyncIterator[object]:
        yield conn  # inject the chrooted double so the bracket pushes/pulls without a real SSH host

    async def _wedged_exec(*_args: object, **_kw: object) -> object:
        await anyio.sleep(5.0)  # a hung remote tool: longer than the deadline, must be cancelled not awaited
        raise AssertionError("wedged remote exec was awaited to completion — the deadline failed to cancel it")

    monkeypatch.setattr(remote_mod, "_ssh_connection", _fixed_conn)
    monkeypatch.setattr(remote_mod, "_remote_exec", _wedged_exec)
    monkeypatch.setattr(remote_mod, "_probe_toolchain", lambda *_a, **_k: _async_none())

    started = time.monotonic()
    try:
        with pytest.raises(TimeoutError):
            with anyio.fail_after(0.4):
                await remote_mod.run_remote(_plan(remote, cwd=remote_cwd, argv=("dotnet", "test")), target)
    finally:
        conn.close()
        await conn.wait_closed()
    elapsed = time.monotonic() - started
    assert elapsed < 2.0, f"deadline did not cancel the wedged remote exec: elapsed={elapsed:.2f}s (shield leaked over the bracketed exec)"
    landed = {str(p.relative_to(tmp_path / "work" / run_id)).replace("\\", "/") for p in (tmp_path / "work" / run_id).rglob("*") if p.is_file()}
    assert {"Workspace.slnx", "src/a.cs"} <= landed, f"push leg did not complete before the exec stalled: {landed!r}"


async def _async_none() -> None:  # noqa: RUF029  # no-op coroutine: the _probe_toolchain mock must return an awaitable
    return None


# --- [SHARED_PULL]


@contextlib.contextmanager
def _moto_s3(monkeypatch: pytest.MonkeyPatch) -> Iterator[AbstractFileSystem]:
    # A moto-backed S3FileSystem double: ambient AWS env (creds + endpoint) is production parity — the SHARED-pull store
    # reads them off the executor env, never a settings knob.
    from moto.server import ThreadedMotoServer  # noqa: PLC0415  # loopback object-store double for the real s3fs read

    for key, value in (("AWS_ACCESS_KEY_ID", "test"), ("AWS_SECRET_ACCESS_KEY", "test"), ("AWS_DEFAULT_REGION", "us-east-1")):
        monkeypatch.setenv(key, value)
    server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False)
    server.start()
    try:
        host, port = server.get_host_and_port()
        monkeypatch.setenv("AWS_ENDPOINT_URL", f"http://{host}:{port}")
        fs = fsspec.filesystem("s3", skip_instance_cache=True)
        fs.mkdirs("bkt", exist_ok=True)  # the bucket pre-exists in production; create it once so the tool-written seed lands
        yield fs
    finally:
        server.stop()


@pytest.mark.anyio
async def test_remote_transfer_reads_shared_cloud_scope_without_byte_transfer(  # one SHARED-pull law: seed a remote-written s3 tree, read it scope-relative with zero transfer, degrade a missing tree to a note
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, socket_enabled: None
) -> None:
    """A SHARED cloud offload reads the tool-written scope tree straight from the object store with zero byte transfer.

    The agent opens the SAME universal paths the remote tool wrote, folding ``Artifact`` rows scope-relative with
    byte counts from backend metadata — no SFTP, no payload crossing the wire. An empty prefix degrades to a note.
    """
    _ = socket_enabled  # lifts the INET socket ban for the moto loopback server; the hook auto-applies the network marker
    with _moto_s3(monkeypatch) as fs:
        remote = _remote_settings(assay_root.root, artifact_backend={"protocol": "s3", "root": "bkt/runs"})
        offload = remote.offload
        assert offload is not None
        assert offload.pull_strategy is PullStrategy.SHARED
        backend_root, run_id, claim = offload.backend.root, remote.run_id, Claim.STATIC.value
        shared = remote.store(protocol="s3", root=backend_root, skip_instance_cache=True)
        scope = ArtifactScope(store=shared, path=shared.path(claim, run_id), dotnet_flags=())
        conn = await provision(SshHost()).client_factory()
        transfer = remote_mod._Transfer(conn=conn, plan=_plan(remote, cwd=offload.target.remote_workroot(run_id), scope=scope), pushed=0, notes=())
        try:
            # Absent tree first: no keys under the scope prefix degrade to a note, no artifacts, parity with the sftp degrade path.
            missing = await transfer.pull({})
            assert (missing.count, missing.artifacts) == (0, ()), f"absent shared tree must fold no artifacts: {missing!r}"
            assert missing.notes == ("remote.artifacts.degraded missing_tree",), f"absent shared tree must degrade to a note: {missing.notes!r}"

            scope_prefix = f"{backend_root}/{claim}/{run_id}"
            fs.pipe_file(f"{scope_prefix}/results.sarif", b'{"runs":[]}\n')
            fs.pipe_file(f"{scope_prefix}/sarif/coverage.xml", b"<coverage/>\nline2\n")
            pulled = await transfer.pull({})
        finally:
            conn.close()
            await conn.wait_closed()

    assert pulled.notes == (), f"a present shared tree must add no degrade note: {pulled.notes!r}"
    assert pulled.count == 2, f"pull count != shared scope tree size: {pulled.count}"
    by_name = {a.path.rsplit("/", 1)[-1]: a for a in pulled.artifacts}
    assert {"results.sarif", "coverage.xml"} == set(by_name), f"shared read lost a scope file: {by_name.keys()}"
    for name, expected in (("results.sarif", 12), ("coverage.xml", 18)):
        row = by_name[name]
        assert (row.kind, row.bytes) == (ArtifactKind.SCOPE, expected), f"wrong kind/size for {name}: {row!r}"
        # The shared scope-relative path carries <claim>/<run_id> and is a bucket-rooted key, never an absolute s3:// URL.
        assert f"/{claim}/{run_id}/" in f"/{row.path}/", f"scope-relative path lost for {name}: {row.path!r}"
        assert not row.path.startswith("s3://"), f"absolute cloud URL leaked into {name}: {row.path!r}"
        assert row.path.startswith(f"{backend_root}/"), f"shared path not rooted at the backend store: {row.path!r}"
    assert by_name["coverage.xml"].path.endswith("/sarif/coverage.xml"), "recursive walk dropped the nested scope file"


# --- [PRUNE]


def test_stale_remote_runs_keeps_newest_per_host_namespace() -> None:
    """``_stale_remote_runs`` prunes only this host's surplus run dirs, oldest-first, never another host's namespace.

    The run-id host token partitions a shared workroot: foreign-token rows are inert regardless of age, and within
    this host the oldest ``len-keep`` dirs by ``(mtime, run_id)`` are returned. ``keep >= own`` selects nothing.
    """
    mine, theirs, absent = "aaaaaaaa", "bbbbbbbb", "cccccccc"
    rows = (
        (f"2026-01-01T00-00-00.0-{mine}-100", 100.0),
        (f"2026-01-02T00-00-00.0-{mine}-101", 200.0),
        (f"2026-01-03T00-00-00.0-{mine}-102", 300.0),
        (f"2026-01-01T00-00-00.0-{theirs}-999", 50.0),  # foreign host: never selected
        ("custom-no-token", 10.0),  # tokenless id: filtered out by every host token
    )
    assert run_id_host_token(rows[0][0]) == mine, "host token must round-trip out of the canonical run id"
    keep1 = remote_mod._stale_remote_runs(rows, token=mine, keep=1)
    assert keep1 == (rows[0][0], rows[1][0]), f"keep=1 must drop this host's two oldest, newest-first retained: {keep1!r}"
    assert all(theirs not in run_id and "custom" not in run_id for run_id in keep1), "a foreign or tokenless run leaked into the prune set"
    assert remote_mod._stale_remote_runs(rows, token=mine, keep=3) == (), "keep>=own count prunes nothing"
    assert remote_mod._stale_remote_runs(rows, token=absent, keep=0) == (), "an absent host token owns no runs to prune"


@pytest.mark.anyio
async def test_remote_prune_sweeps_only_this_hosts_stale_run_dirs(assay_root: AssayHarness, tmp_path: Path) -> None:
    """``_remote_prune`` removes this host's orphaned ``<workroot>/<run_id>`` dirs over SFTP, sparing another host's runs."""
    remote = _remote_settings(assay_root.root, exec_workroot="/work", artifact_retention=1)
    token = remote.host_run_token
    assert token, "the default run id must embed a host token for the namespace filter"
    workdir = tmp_path / "work"
    # Three of this host's runs (only the newest survives at retention=1), plus one foreign-token run that must persist.
    mine_old = f"2026-01-01T00-00-00.0-{token}-1"
    mine_mid = f"2026-01-02T00-00-00.0-{token}-2"
    mine_new = remote.run_id  # the current run: newest, always retained
    theirs = "2026-06-01T00-00-00.0-deadbeef-9"
    for run_id, payload in ((mine_old, b"old"), (mine_mid, b"mid"), (mine_new, b"new"), (theirs, b"foreign")):
        (workdir / run_id / "src").mkdir(parents=True)
        (workdir / run_id / "src" / "f.cs").write_bytes(payload)

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()
    try:
        notes = await remote_mod._remote_prune(conn, remote)
        survivors = {p.name for p in workdir.iterdir() if p.is_dir()}
    finally:
        conn.close()
        await conn.wait_closed()

    assert notes == ("remote.prune.removed runs=2",), f"prune note must report exactly this host's two removed runs: {notes!r}"
    assert survivors == {mine_new, theirs}, f"prune must keep the newest own run and the foreign run, drop the rest: {survivors!r}"


# --- [LANE_MANIFEST]


def _manifest_plan(harness: AssayHarness, tool: Tool, paths: tuple[str, ...] = ()) -> ExecPlan:
    # Minimal ExecPlan carrying just the lane discriminant (runner) and the seed tokens the manifest scoper reads.
    return ExecPlan(
        argv=tool.command,
        check=Check(tool=tool, paths=paths),
        cwd="",
        env={},
        settings=harness.settings,
        scope=None,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )


def test_lane_manifest_csharp_scopes_to_transitive_project_closure(assay_root: AssayHarness) -> None:
    """The C# lane manifest is the transitive ProjectReference closure plus root build config, not the whole git tree.

    A naive subtree scope would drop ``libs/B`` when the build seeds ``libs/A`` that references it across directories;
    the closure walk keeps B and its files while excluding the unrelated ``libs/C`` project tree entirely.
    """
    root = Path(str(assay_root.settings.local_root))
    (root / "libs/A").mkdir(parents=True)
    (root / "libs/B").mkdir(parents=True)
    (root / "libs/C").mkdir(parents=True)
    (root / "libs/A/A.csproj").write_bytes(b'<Project><ItemGroup><ProjectReference Include="../B/B.csproj"/></ItemGroup></Project>')
    (root / "libs/B/B.csproj").write_bytes(b"<Project/>")
    (root / "libs/C/C.csproj").write_bytes(b"<Project/>")
    universe = (
        "Directory.Build.props",
        "Directory.Packages.props",
        "README.md",  # repo-root non-config file: excluded (not on the closure, not a build-config anchor)
        "libs/A/A.csproj",
        "libs/A/Owner.cs",
        "libs/B/B.csproj",
        "libs/B/Dep.cs",
        "libs/C/C.csproj",  # unrelated project: excluded
        "libs/C/Other.cs",
    )
    tool = Tool("cs-build", Runner.DOTNET, ("build", str(root / "libs/A/A.csproj")), Input.OWNED, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    scoped = frozenset(remote_mod._lane_manifest(_manifest_plan(assay_root, tool), universe))
    assert frozenset({"libs/A/Owner.cs", "libs/B/Dep.cs"}) <= scoped, f"closure dropped a transitive project file: {scoped!r}"
    assert frozenset({"Directory.Build.props", "Directory.Packages.props"}) <= scoped, f"root build config must always cross: {scoped!r}"
    assert not any(p.startswith("libs/C/") for p in scoped), f"unrelated project tree must not cross: {scoped!r}"
    assert "README.md" not in scoped, f"a non-config repo-root file is off the C# closure: {scoped!r}"


@pytest.mark.parametrize(
    "label, tool, universe, expected",
    [
        (
            "python-package-tests-config",
            Tool("py-lint", Runner.UV, ("ruff", "check"), Input.NONE, Language.PYTHON, Claim.STATIC),
            ("pyproject.toml", "uv.lock", "tools/assay/core/engine.py", "tests/python/conftest.py", "libs/csharp/Rasm/Foo.cs", "docs/x.md"),
            {"pyproject.toml", "uv.lock", "tools/assay/core/engine.py", "tests/python/conftest.py"},
        ),
        (
            "unknown-lane-full-universe",
            Tool("direct", Runner.DIRECT, ("echo",), Input.NONE, Language.CSHARP, Claim.STATIC),
            ("a", "b/c", "d/e/f"),
            {"a", "b/c", "d/e/f"},
        ),
    ],
)
def test_lane_manifest_python_and_unknown_lanes(
    label: str, tool: Tool, universe: tuple[str, ...], expected: set[str], assay_root: AssayHarness
) -> None:
    """The Python lane scopes to package source + tests + config anchors; a lane with no project graph keeps the full universe."""
    _ = label
    assert set(remote_mod._lane_manifest(_manifest_plan(assay_root, tool), universe)) == expected


def test_remote_scope_argv_rebases_host_absolute_scope_paths(assay_root: AssayHarness) -> None:
    """``_remote_scope_argv`` rebases CspSarifDir and --artifacts-path host-absolute paths under the remote workroot.

    A remote Linux build never sees a macOS-absolute scope path (CS0016): the ``prop=<abs>`` value tail and the bare
    ``<abs>`` token both rebind ``<local_root>/X -> <remote_root>/X``, while flags and non-local tokens pass through.
    """
    local_root = str(assay_root.settings.local_root)
    remote_root = "/work/run-1"
    argv = (
        "dotnet",
        "build",
        f"-p:CspSarifDir={local_root}/.artifacts/assay/build/bridge/Release/sarif",
        "--artifacts-path",
        f"{local_root}/.artifacts/assay/build/bridge/Release",
        "/p:Unrelated=value",
        f"{local_root}/libs/A/A.csproj",
    )
    rebased = remote_mod._remote_scope_argv(argv, local_root=local_root, remote_root=remote_root)
    assert rebased[2] == f"-p:CspSarifDir={remote_root}/.artifacts/assay/build/bridge/Release/sarif", f"CspSarifDir not rebased: {rebased[2]!r}"
    assert rebased[4] == f"{remote_root}/.artifacts/assay/build/bridge/Release", f"--artifacts-path value not rebased: {rebased[4]!r}"
    assert rebased[:2] == ("dotnet", "build"), "leading runner/verb tokens must survive the rebase"
    assert rebased[3] == "--artifacts-path", "the --artifacts-path flag token must survive the rebase"
    assert rebased[5] == "/p:Unrelated=value", "a prop token carrying no local-root path must pass through untouched"
    assert rebased[6] == f"{remote_root}/libs/A/A.csproj", "a bare absolute project token under local_root rebases whole"
