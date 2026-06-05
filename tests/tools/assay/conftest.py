"""Typed fixture surface for assay bedrock tests.

Session-scoped OTel provider + per-test span exporter, structlog capture, fsspec memory store,
deterministic psutil doubles, SSH loopback seam, subprocess verb_cell, and module-scope Hypothesis
strategies for all 8 assay enums.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass, field
import datetime as dt
import importlib.util
import os
from pathlib import Path
import shutil
from types import SimpleNamespace
from typing import override, Protocol, TYPE_CHECKING
from unittest.mock import create_autospec, MagicMock

import anyio
from anyio.abc import ByteReceiveStream
import asyncssh
from expression import Ok, Result  # noqa: TC002  # Result used as runtime annotation in diff() closure
from hypothesis import strategies as st
from hypothesis.strategies import binary, builds, floats, integers, register_type_strategy, sampled_from
import msgspec
import psutil as _psutil
import pytest
import time_machine
from upath import UPath

from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings  # noqa: TC001
from tools.assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSurface,
    ArtifactKind,
    Claim,
    Completed,
    Diagnostic,
    Envelope,
    envelope as wrap_envelope,
    Fault,
    Input,
    Language,
    Mode,
    MutationLane,
    PackageRun,
    receipt,
    RunDelta,
    Runner,
    SourceKind,
    SymbolShape,
    TestRun,
    VerifySummary,
)
from tools.assay.core.status import RailStatus
from tools.assay.rails import bridge as bridge_rail, package as package_rail
from tools.quality.rails import package as quality_package
from tools.quality.settings import ArtifactScope as QualityScope, QualitySettings


if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Awaitable, Callable, Generator, Mapping

    from structlog.types import EventDict

    from tools.assay.core.model import Report


# --- [CONSTANTS] -----------------------------------------------------------------------

# Host-gated skipif markers — computed once at collection so xdist workers share the result.
_DOTNET = shutil.which("dotnet")
_YAK = shutil.which("yak")
_ILSPYCMD = shutil.which("ilspycmd")
_UV = shutil.which("uv")
_RHINOWIP = next(
    (p for p in ("/Applications/RhinoWIP.app", "/Applications/Rhino WIP.app", "/Applications/Rhino 8 WIP.app") if Path(p).is_dir()), None
)
_TREE_SITTER_PY: bool = importlib.util.find_spec("tree_sitter_python") is not None


# --- [TYPES] ----------------------------------------------------------------------------


class VerbRunner(Protocol):
    """Async subprocess runner fixture surface."""

    def __call__(self, *argv: str, extra_env: dict[str, str] | None = None) -> Awaitable[Envelope]: ...


skip_no_dotnet = pytest.mark.skipif(_DOTNET is None, reason="dotnet not on PATH")
skip_no_rhino = pytest.mark.skipif(_RHINOWIP is None, reason="RhinoWIP.app not installed")
skip_no_yak = pytest.mark.skipif(_YAK is None, reason="yak not on PATH")
skip_no_ilspycmd = pytest.mark.skipif(_ILSPYCMD is None, reason="ilspycmd not on PATH")
skip_no_tree_sitter_py = pytest.mark.skipif(not _TREE_SITTER_PY, reason="tree-sitter-python grammar absent")


# --- [GENERATORS] -----------------------------------------------------------------------
# Register all 8 assay enums so from_type(X) resolves in any @given after module import.

register_type_strategy(RailStatus, sampled_from(list(RailStatus)))
register_type_strategy(Claim, sampled_from(list(Claim)))
register_type_strategy(Language, sampled_from(list(Language)))
register_type_strategy(Runner, sampled_from(list(Runner)))
register_type_strategy(Input, sampled_from(list(Input)))
register_type_strategy(Mode, sampled_from(list(Mode)))
register_type_strategy(ArtifactKind, sampled_from(list(ArtifactKind)))
register_type_strategy(MutationLane, sampled_from(list(MutationLane)))
register_type_strategy(SourceKind, sampled_from(list(SourceKind)))
register_type_strategy(SymbolShape, sampled_from(list(SymbolShape)))

# Module-scope composite strategies — built once, reused across all test modules.
rail_status_st: st.SearchStrategy[RailStatus] = sampled_from(list(RailStatus))

completed_st: st.SearchStrategy[Completed] = builds(
    Completed,
    argv=st.tuples(st.text(max_size=32)),
    returncode=integers(min_value=0, max_value=255),
    stdout=binary(max_size=256),
    stderr=binary(max_size=256),
    status=rail_status_st,
    duration_ms=floats(min_value=0.0, max_value=10_000.0, allow_nan=False),
)

fault_st: st.SearchStrategy[Fault] = builds(
    Fault,
    argv=st.tuples(st.text(max_size=32)),
    status=sampled_from([RailStatus.FAULTED, RailStatus.BUSY, RailStatus.TIMEOUT]),
    message=st.text(max_size=256),
)

# AnyDetail union: iterate the 7 concrete Detail variants with equal weight per arm (1/8 each).
_DETAIL_VARIANTS: tuple[type, ...] = (ApiSurface, VerifySummary, TestRun, PackageRun, ApiResolution, Diagnostic, RunDelta)


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class AssayHarness:
    """Isolated tmp-tree capsule: all assay I/O roots under ``<root>/.artifacts/assay``.

    ``settings`` carries ``exec_target=""``/``exec_known_hosts=None`` for point-and-go local
    operation that mutates nothing outside ``root``. ``remote`` re-derives the same settings against
    an ssh ``exec_target`` for the single network-marked law.
    """

    root: Path
    settings: AssaySettings

    def write(self, rel: str | Path, text: str = "") -> Path:
        path = self.root / Path(rel)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        return path

    def scope(self, claim: Claim = Claim.PACKAGE) -> ArtifactScope:
        return ArtifactScope.open(self.settings, claim)

    def remote(self, exec_target: str) -> AssaySettings:
        return self.settings.model_copy(update={"exec_target": exec_target, "exec_known_hosts": None})

    @staticmethod
    def envelope_of(payload: Report | Fault, *, claim: Claim, verb: str) -> Envelope:
        return wrap_envelope(payload, claim=claim, verb=verb)


@dataclass(frozen=True, slots=True)
class RailProbe:
    """Socket-free mock host: monkeypatch a rail-module member to a canned receipt.

    Patch target is the OWNER rail module, never ``core.engine``, mirroring production binding.
    """

    calls: list[tuple[str, tuple[object, ...], dict[str, object]]] = field(default_factory=list)

    def install(self, monkeypatch: pytest.MonkeyPatch, owner: object, member: str, payload: Result[object, Fault]) -> None:
        def replacement(*args: object, **kwargs: object) -> Result[object, Fault]:
            self.calls.append((member, args, kwargs))
            return payload

        monkeypatch.setattr(owner, member, replacement)

    @staticmethod
    def ok(argv: tuple[str, ...] = ("rasm-bridge", "check"), status: RailStatus = RailStatus.OK) -> Result[Completed, Fault]:
        return Ok(receipt(argv, 0, status=status))


@dataclass(frozen=True, slots=True)
class SshLoopback:
    """A live loopback asyncssh server bound via ``asyncssh.listen``.

    ``exec_target`` includes a username because asyncssh ``saslprep`` rejects ``None`` at connect.
    """

    port: int

    @property
    def exec_target(self) -> str:
        return f"ssh://x@127.0.0.1:{self.port}"


@dataclass(frozen=True, slots=True)
class AbDelta:
    """The A/B delta: both decoded operator outputs plus the field-name correspondence."""

    assay: Envelope
    quality: dict[str, object]
    mapping: Mapping[str, str]


@dataclass(frozen=True, slots=True)
class BridgeResult:
    """``_BridgeResult`` JSON writer: one valid camelCase payload plus three adversarial variants."""

    directory: Path

    def valid(self, command: str = "scenario.verify.csx") -> Path:
        payload = {
            "command": command,
            "status": "ok",
            "phases": [
                {
                    "phase": "execute",
                    "status": "ok",
                    "data": {"diagnostics": {"commandWindow": [], "exceptionReports": []}},
                    "outputs": [{"source": "stdout", "text": "rasm.rhino-bridge.evidence=facts={}"}],
                }
            ],
        }
        return self._write("valid.json", msgspec.json.encode(payload))

    def malformed(self) -> Path:
        return self._write("malformed.json", b"{not json")

    def partial(self) -> Path:
        return self._write("partial.json", b"{}")

    def missing(self) -> Path:
        return self.directory / "absent.json"

    def _write(self, name: str, raw: bytes) -> Path:
        path = self.directory / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(raw)
        return path


@dataclass(frozen=True, slots=True)
class YakShape:
    """Fake-yak + fake-msbuild materializer."""

    slug: str = "rasm-bridge"
    project: Path = Path("apps/bridge/plugin.csproj")
    assembly_name: str = "Rasm"
    target_ext: str = ".rhp"
    target_framework: str = "net10.0"
    package_pattern: str = "rasm-rh9_1-mac.yak"

    def props(self, meta: package_rail.YakMeta) -> dict[str, str]:
        return {
            "AssemblyName": meta.assembly_name,
            "MSBuildProjectDirectory": str(meta.project_dir),
            "TargetDir": str(meta.target_dir),
            "TargetExt": meta.target_ext,
            "TargetFramework": meta.target_framework,
            "YakManifestDirectory": str(meta.manifest_dir),
            "YakPackageDirectory": str(meta.package_dir),
            "YakPackagePattern": meta.package_pattern,
            "YakPackageSlug": self.slug,
            "YakPath": str(meta.yak_path),
            "YakPlatform": meta.yak_platform,
            "YakPushSource": meta.yak_push_source,
        }

    def materialize(self, harness: AssayHarness) -> package_rail.YakMeta:
        yak = harness.write("yak", "#!/bin/sh\nexit 0\n")
        yak.chmod(0o755)
        project = harness.write(self.project, f"<Project><PropertyGroup><YakPackageSlug>{self.slug}</YakPackageSlug></PropertyGroup></Project>")
        target = project.parent / "bin" / harness.settings.configuration.value / self.target_framework
        target.mkdir(parents=True, exist_ok=True)
        harness.write(project.parent.relative_to(harness.root) / "manifest.yml", f"name: {self.slug}\n")
        harness.write(target.relative_to(harness.root) / f"{self.assembly_name}{self.target_ext}")
        harness.write(target.relative_to(harness.root) / f"{self.assembly_name}.dll")
        return package_rail.YakMeta(
            project=str(project.relative_to(harness.root)),
            manifest_dir=project.parent,
            target_dir=target,
            assembly_name=self.assembly_name,
            target_ext=self.target_ext,
            yak_path=yak,
            yak_platform="mac",
            yak_push_source="",
            package_dir=project.parent / "yak",
            package_pattern=self.package_pattern,
            target_framework=self.target_framework,
            project_dir=project.parent,
        )


# --- [PSUTIL DOUBLES] ------------------------------------------------------------------
# ``_proc`` builds a create_autospec(psutil.Process) double from keyword fields — no hand-rolled API.
# ``_make_psutil_module`` wraps N pid→proc mappings into a MagicMock(spec=psutil) module double.


def _proc(
    *,
    pid: int = 12345,
    rss: int = 4096,
    fds: int = 8,
    cpu: float = 0.0,
    running: bool = True,
    create_time: float = 1_700_000_000.0,
    raise_no_such: bool = False,
) -> MagicMock:
    """Return a ``create_autospec(psutil.Process)`` double with the given field values.

    ``raise_no_such=True`` marks the double as a dead-process sentinel: the module-level factory
    (``_make_psutil_module``) will raise ``NoSuchProcess`` instead of returning it.
    """
    p: MagicMock = create_autospec(_psutil.Process, instance=True)
    p.pid = pid
    p.memory_info.return_value = SimpleNamespace(rss=rss)
    p.num_fds.return_value = fds
    p.cpu_percent.return_value = cpu
    p.is_running.return_value = running
    p.create_time.return_value = create_time
    p._raise_no_such = raise_no_such  # private sentinel flag; consumed by _make_psutil_module
    return p


def _make_psutil_module(procs: dict[int | None, MagicMock], *, cpu_count: int = 4) -> MagicMock:
    """Return a ``MagicMock(spec=psutil)`` module double that dispatches ``Process(pid)`` via ``procs``.

    ``procs[None]`` is the self-process (``Process()`` / ``Process(os.getpid())``); integer keys are
    specific pids. An unregistered pid causes ``NoSuchProcess`` — no silent fallback.
    ``raise_no_such=True`` on a double causes the factory to raise ``NoSuchProcess(pid)`` on lookup.
    """
    fake = MagicMock(spec=_psutil)
    fake.NoSuchProcess = _psutil.NoSuchProcess
    fake.AccessDenied = _psutil.AccessDenied
    fake.cpu_count.return_value = cpu_count

    def _process_factory(pid: int | None = None) -> MagicMock:
        proc = procs.get(pid)
        if proc is None:
            raise _psutil.NoSuchProcess(pid if isinstance(pid, int) else 0)
        if getattr(proc, "_raise_no_such", False):
            raise _psutil.NoSuchProcess(int(proc.pid))
        return proc

    fake.Process.side_effect = _process_factory
    return fake


# --- [COMPOSITION] ---------------------------------------------------------------------


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Build isolated tmp-tree settings with redirected artifact I/O.

    Returns:
        Assay harness rooted under the pytest temporary directory.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), exec_target="", exec_known_hosts=None)
    return AssayHarness(tmp_path, settings)


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """Isolated fsspec ``memory://`` ArtifactStore partitioned by ``run_id``.

    Teardown uses ``store.fs.rm(root, recursive=True)`` (fsspec-native, lock-safe) to delete only this
    test's namespace prefix, preserving isolation for concurrent xdist workers.

    Yields:
        An ``ArtifactStore`` backed by ``fsspec.MemoryFileSystem`` rooted at ``run_id``-keyed prefix.
    """
    store = assay_root.settings.store(protocol="memory")
    yield store
    if store.fs.exists(f"/{store.root}"):
        store.fs.rm(f"/{store.root}", recursive=True)


@pytest.fixture
def frozen_clock() -> Generator[None]:
    """Pin the wall clock for deterministic run_id and started_at oracle tests.

    Yields:
        None while time is frozen at 2026-01-01T00:00:00 UTC.
    """
    with time_machine.travel(dt.datetime(2026, 1, 1, tzinfo=dt.UTC), tick=False):
        yield


@pytest.fixture
def envelope() -> Callable[[pytest.CaptureFixture[str]], Envelope]:
    """Decode the single stdout Envelope line the CLI ``_emit`` writes.

    Returns:
        Callable that reads captured stdout and decodes one Envelope.
    """

    def decode(capsys: pytest.CaptureFixture[str]) -> Envelope:
        captured = capsys.readouterr()
        rows = captured.out.splitlines()
        assert len(rows) == 1, f"expected exactly one stdout Envelope line, got {len(rows)}"
        return msgspec.json.decode(rows[0].encode(), type=Envelope)

    return decode


@pytest.fixture
def rail_probe() -> RailProbe:
    """Build a socket-free rail host with canned receipts.

    Returns:
        Probe that installs fake ``run_check`` and ``fan_out`` responses.
    """
    return RailProbe()


@pytest.fixture
async def ssh_loopback(socket_enabled: None) -> AsyncGenerator[SshLoopback]:
    """Live loopback asyncssh server (network-marked): yields the ssh ``exec_target`` capsule.

    Uses ``asyncssh.listen`` as an async context manager — no daemon threads, no manual teardown,
    no ResourceWarning under ``filterwarnings=["error"]``.

    Yields:
        ``SshLoopback`` carrying the bound port and ``ssh://x@127.0.0.1:<port>`` ``exec_target``.
    """
    _ = socket_enabled

    class _Server(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            _ = username
            return False

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029
        # Echo a fixed token PLUS the received command so callers can assert the command transited.
        # Format: "remote-ok:<command>\n" — both connectivity and command-fidelity laws can assert.
        command = process.command or ""
        process.stdout.write(f"remote-ok:{command}\n")
        process.exit(0)

    key = asyncssh.generate_private_key("ssh-ed25519")
    server = await asyncssh.listen("127.0.0.1", 0, server_host_keys=[key], server_factory=_Server, process_factory=_handler)
    async with server:
        yield SshLoopback(port=server.get_port())


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """Build bridge-result fixtures for defensive-decode laws.

    Returns:
        Bridge result writer rooted under the pytest temporary directory.
    """
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Build the fake yak and fake MSBuild materializer.

    Returns:
        Package-shape materializer for yak rail tests.
    """
    return YakShape()


@pytest.fixture
def ab_diff(assay_root: AssayHarness) -> Callable[[Claim, str], AbDelta]:
    """Run a read-only ``assay`` rail and the matching ``tools.quality`` rail; decode both + the field delta.

    Fault-transparent: asserts that the quality rail is ``is_ok()`` before decoding — silently
    degrading to empty-dict on fault was the prior bug.

    Returns:
        Callable that compares an assay Envelope with the quality rail payload.
    """
    mapping = {"rail": "claim", "data": "report", "evidence": "error_context"}

    def diff(claim: Claim, verb: str) -> AbDelta:
        scope = assay_root.scope(claim)
        outcome: Result[Report, Fault] = package_rail.list(assay_root.settings, scope, package_rail.PackageParams())
        payload = outcome.ok if outcome.is_ok() else outcome.error
        assay_env = wrap_envelope(payload, claim=claim, verb=verb)
        quality_settings = QualitySettings(root=assay_root.root, rhino_app=None, run_id="ab-diff")
        quality_scope = QualityScope(root=assay_root.root, rail=claim.value, scope_path=assay_root.root, dotnet_env={})
        quality_payload = quality_package.package_list_payload(quality_settings, quality_scope)
        assert quality_payload.is_ok(), f"ab_diff: quality rail faulted: {quality_payload}"
        quality_decoded = msgspec.json.decode(quality_payload.ok, type=dict[str, object])
        return AbDelta(assay=assay_env, quality=quality_decoded, mapping=mapping)

    return diff


@pytest.fixture
def fake_psutil() -> MagicMock:
    """Default psutil module double: self-proc on ``None``, dead-sentinel at pid 99999, cpu_count=4.

    Tests override via ``fake.Process.side_effect = _process_factory`` or build fresh with
    ``_make_psutil_module``. Inject via ``monkeypatch.setattr(engine_mod, "psutil", fake_psutil)``.

    Returns:
        psutil module double with default self-process and dead-process sentinels.
    """
    default_self = _proc()
    return _make_psutil_module({None: default_self, 99999: _proc(pid=99999)})


@pytest.fixture
def verb_cell(assay_root: AssayHarness) -> VerbRunner:
    """Run assay as a real subprocess, decode the single stdout Envelope line.

    Returns an async ``run(*argv)`` coroutine so the caller can ``await`` it directly without
    ``anyio.run`` nesting that raises ``RuntimeError`` in async test contexts.

    Returns:
        Async callable ``run(*argv, extra_env=None) -> Envelope``.
    """
    match _UV:
        case None:
            pytest.skip("uv not on PATH")
        case _:
            pass

    async def run(*argv: str, extra_env: dict[str, str] | None = None) -> Envelope:
        env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **(extra_env or {})}  # noqa: TID251  # test boundary: must clone + override env for subprocess isolation
        result = await anyio.run_process(["uv", "run", "python", "-m", "tools.assay", *argv], env=env, cwd=str(assay_root.root), check=False)
        rows = result.stdout.splitlines()
        assert len(rows) == 1, f"expected 1 Envelope line, got {len(rows)}: {result.stdout[:300]!r}"
        return msgspec.json.decode(rows[0], type=Envelope)

    return run
