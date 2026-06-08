"""Typed fixture DNA for the assay test suite.

The S1 ``resolve`` resolver (``tests._strategies``) registers every wire struct with bounded, encode-clean
strategies (reading ``Meta`` constraints generically, so ``from_type(Fault)`` yields non-empty bounded
messages and adding a model field over the supported JSON leaf taxonomy needs zero conftest change). The
generic value-level oracles live in ``tests._spec`` and the law-coverage gate in ``tests._aspect`` — this
module wires the assay SUT into both and owns only the assay-typed seams that do not generalize: the
polymorphic ``cli`` fixture (in-process or real subprocess under ``isolate=True``), the socket-free
``RailProbe`` host, the loopback ``SshLoopback`` network seam, the ``BridgeResult``/``YakShape``
materializers, the psutil module doubles, and the envelope/history oracles
(``read_one_envelope`` / ``assert_counts_consistent`` / ``make_history_envelope`` / ``pipe_history``).

Prefer ``resolve(X)`` over a hand-rolled strategy and the ``tests._spec`` oracles over hand-rolled
``assert x.is_ok()``.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from collections.abc import Callable  # noqa: TC003  # runtime: annotates RailProbe.install's `rail` factory return
from dataclasses import dataclass, field
import functools
import os
from pathlib import Path
import shutil
from types import SimpleNamespace
from typing import Final, get_args, override, Protocol, TYPE_CHECKING
from unittest.mock import create_autospec, MagicMock

import anyio
from expression import Error, Ok, Result  # Error/Ok/Result are runtime in the RailProbe canned-outcome builders
from hypothesis import strategies as st
import msgspec
import psutil as _psutil
import pytest
from upath import UPath

from tests._aspect import register_sut  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tests._spec import (  # noqa: PLC2701  # sibling test-internal module re-exported so assay law files import oracles from one DNA surface
    assert_error,
    assert_error_status,
    assert_none,
    assert_ok,
    assert_roundtrip,
    assert_some,
)
from tests._strategies import resolve  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tests.conftest import REPO_ROOT
from tools.assay.composition.registry import REGISTRY
from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings  # noqa: TC001
from tools.assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSource,
    ApiSurface,
    Artifact,
    Check,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    Envelope,
    envelope as wrap_envelope,
    Fault,
    fold,
    Match,
    PackageRun,
    receipt,
    Report,
    RunDelta,
    RunSnapshot,
    Stage,
    TestRun,
    Tool,
    VerifySummary,
)
from tools.assay.core.status import RailStatus
from tools.assay.rails import package as package_rail


if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Generator


# --- [TYPES] ----------------------------------------------------------------------------


class VerbRunner(Protocol):
    """In-process / subprocess CLI runner fixture surface.

    Synchronous so the in-process default (which spawns its own ``anyio`` event loop inside ``main``)
    is callable from any test; ``isolate=True`` drives the subprocess via ``anyio.run`` internally.
    Returns a ``CliResult`` carrying the decoded Envelope, exit code, and the raw stdout/stderr bytes
    so channel-separation laws can assert structlog diagnostics stay on stderr.
    """

    def __call__(self, *argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> CliResult: ...


class CpuSampler(Protocol):
    """Canned ``psutil.cpu_percent`` signature: warmup ``cpu_percent(0.1)`` and read ``cpu_percent(interval=None)``."""

    def __call__(self, interval: float | None = None) -> float: ...


class CpuDoubleInstaller(Protocol):
    """The ``cpu_double`` fixture surface: pin ``automation.engine``'s ``psutil.cpu_percent`` to a canned sampler."""

    def __call__(self, cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_UV = shutil.which("uv")

# Deterministic codec shared by the history oracles (mirrors model._ENCODER order policy).
WIRE_ENCODER = msgspec.json.Encoder(order="deterministic")

# The assay SUT package whose public surface the law-coverage gate (tests._aspect) walks.
SUT_PACKAGE: Final = "tools.assay"

# Law-less symbols the coverage gate must not demand a law for. MINIMAL by intent — exemption is not a way
# to dodge a falsifiable law. Each entry is either a structural type-alias Callable (no behavior to assert)
# or an internal __all__'d seam whose behavior is proven through the public surface that consumes it.
#   - aspect.py:  Attrs/Bind/Hom/Layer are PEP 695 `type` aliases (Callable / tuple shapes), Inversion is a
#                 decoration-time ordering Exception surfaced via TypeError (asserted through compose/assemble).
#   - engine.py:  ByteRecv is a Callable alias; _RESOURCE is the resource ContextVar seam.
#   - model.py:   InprocThunk is a Callable alias. NOTE: the simple-name `Bind` also names model.Bind (a real
#                 registry-binding model) — that model still earns its own law in S3b; the name is name-exempt
#                 here only because aspect.Bind (the alias) is genuinely law-less and the gate keys on simple-names.
#   - registry.py: Handler is a Callable alias.
#   - automation/engine.py: Fire/Worker are Callable aliases.
#   - aspect.py:  _CHECKED_LAYER/_RING are the assembled-layer + ring-buffer ContextVar seams.
_EXEMPT: Final = frozenset({
    "Attrs", "Bind", "Hom", "Layer", "Inversion",  # aspect type aliases + ordering Exception
    "ByteRecv", "_RESOURCE",                        # core/engine alias + resource ContextVar
    "InprocThunk",                                  # model Callable alias
    "Handler",                                      # registry Callable alias
    "Fire", "Worker", "ChangeBatch",               # automation/engine Callable + type aliases
    "_CHECKED_LAYER", "_RING",                      # aspect assembled-layer + ring ContextVar seams
    "_HINT_CAP", "_RESULT_CAP",                     # model int caps (no independent law; _HINT_CAP exercised via field_cap)
})  # fmt: skip

register_sut(SUT_PACKAGE, exempt=_EXEMPT)


# --- [STRATEGIES] ----------------------------------------------------------------------
# `resolve` (tests._strategies) registers a bounded, encode-clean strategy for each wire struct so
# `st.from_type(X)` round-trips through the deterministic codec. StrEnum / Detail-tag unions resolve
# natively. The module-level `_st` names below are thin `resolve(...)`-backed aliases the law files import.


rail_status_st: st.SearchStrategy[RailStatus] = resolve(RailStatus)
binds_st = st.sampled_from(REGISTRY)
completed_st: st.SearchStrategy[Completed] = resolve(Completed)
fault_st: st.SearchStrategy[Fault] = resolve(Fault)
counts_st: st.SearchStrategy[Counts] = resolve(Counts)
artifact_st: st.SearchStrategy[Artifact] = resolve(Artifact)
match_st: st.SearchStrategy[Match] = resolve(Match)
run_snapshot_st: st.SearchStrategy[RunSnapshot] = resolve(RunSnapshot)
report_st: st.SearchStrategy[Report] = resolve(Report)
# Derive the Detail variants from the AnyDetail alias itself — self-tracking, no positional coupling.
detail_st: st.SearchStrategy[AnyDetail] = st.one_of(*(resolve(v) for v in get_args(AnyDetail.__value__)))
# resolve forces Tool.thunk (Callable) and Check.cwd (Path) to None via the CustomType arm, so from_type
# sweeps the engine Runner x Input x Mode x Language cartesian with encode-clean instances.
tool_st: st.SearchStrategy[Tool] = resolve(Tool)
check_st: st.SearchStrategy[Check] = resolve(Check)
stage_st: st.SearchStrategy[Stage] = resolve(Stage)
api_resolution_st: st.SearchStrategy[ApiResolution] = resolve(ApiResolution)
api_source_st: st.SearchStrategy[ApiSource] = resolve(ApiSource)
api_surface_st: st.SearchStrategy[ApiSurface] = resolve(ApiSurface)
verify_summary_st: st.SearchStrategy[VerifySummary] = resolve(VerifySummary)
test_run_st: st.SearchStrategy[TestRun] = resolve(TestRun)
package_run_st: st.SearchStrategy[PackageRun] = resolve(PackageRun)
diagnostic_st: st.SearchStrategy[Diagnostic] = resolve(Diagnostic)
run_delta_st: st.SearchStrategy[RunDelta] = resolve(RunDelta)


@st.composite
def _envelopes(draw: st.DrawFn) -> Envelope:
    """Draw a wire-consistent Envelope from a Report or Fault via the canonical ``envelope`` projection.

    Returns:
        Envelope whose status/exit_code derive from the wrapped payload.
    """
    payload: Report | Fault = draw(st.one_of(report_st, fault_st))
    return wrap_envelope(payload, claim=draw(resolve(Claim)), verb=draw(st.text(max_size=32)), run_id=draw(st.text(max_size=32)))


envelope_st: st.SearchStrategy[Envelope] = _envelopes()
st.register_type_strategy(Envelope, envelope_st)


# --- [MODELS] --------------------------------------------------------------------------


class CliResult(msgspec.Struct, frozen=True, gc=False):
    """One CLI invocation: the decoded stdout Envelope plus exit code and raw stdout/stderr bytes."""

    envelope: Envelope
    exit_code: int
    stdout: bytes
    stderr: bytes


@dataclass(frozen=True, slots=True)
class AssayHarness:
    """Isolated tmp-tree capsule: all assay I/O roots under ``<root>/.artifacts/assay``.

    ``settings`` carries ``exec_target=""``/``exec_known_hosts=None`` for point-and-go local operation
    that mutates nothing outside ``root``. ``remote`` re-derives the same settings against an ssh
    ``exec_target`` for the network-marked laws.
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
    """Socket-free mock host: monkeypatch an engine seam, as-imported-by an owner module, to a canned outcome.

    One probe owns every canned-output seam the rail/engine/automation laws consume. The patch target is
    the OWNER module that re-binds the seam (``rails.api``, ``automation.engine``, ``core.engine`` only when
    a law drives that module directly), never the definition site, mirroring how production resolves the name.

    Seam shapes (all dispatched through the polymorphic ``install``):
      - ``"run_check"``     — sync ``(check, *, settings, scope, routed, deadline=None) -> Result[Completed, Fault]``.
      - ``"run_check_async"``— coroutine of the same ``Result`` shape (``_program_outcome`` awaits this).
      - ``"fan_out"``       — sync ``(checks, ...) -> tuple[Result[Completed, Fault], ...]`` (one payload per check).
      - ``"rail"``          — factory ``(bind, settings) -> (params) -> Envelope`` (the automation registry seam).

    Every invocation is recorded on ``calls`` as ``(member, args, kwargs)``; ``checks`` collects the first
    positional ``Check`` per call so a law can assert the routed argv without unpacking ``calls`` by hand.
    """

    calls: list[tuple[str, tuple[object, ...], dict[str, object]]] = field(default_factory=list)
    checks: list[Check] = field(default_factory=list)

    def install(
        self,
        monkeypatch: pytest.MonkeyPatch,
        owner: object,
        member: str,
        payload: Result[object, Fault] | Envelope | tuple[Result[Completed, Fault], ...],
    ) -> None:
        """Bind ``owner.member`` to a canned seam mirroring the production call shape of ``member``.

        ``run_check``/``run_check_async``/``fan_out`` accept a ``Result``/tuple payload; ``rail`` accepts a
        canned ``Envelope`` and installs the ``(bind, settings) -> (params) -> Envelope`` factory. Each call
        is appended to ``calls`` and any first-positional ``Check`` is appended to ``checks``.

        Raises:
            TypeError: When ``member`` and ``payload`` are not one of the supported seam/payload pairings.
        """

        def _record(args: tuple[object, ...]) -> None:
            self.calls.append((member, args, {}))
            self.checks.extend(c for c in args[:1] if isinstance(c, Check))

        match member, payload:
            case "run_check_async", Result() as result:

                async def _async(*args: object, **kwargs: object) -> Result[object, Fault]:  # noqa: RUF029  # async required: production seam is awaited
                    self.calls.append((member, args, kwargs))
                    self.checks.extend(c for c in args[:1] if isinstance(c, Check))
                    return result

                monkeypatch.setattr(owner, member, _async)
            case "fan_out", tuple() as results:

                def _fan(checks: tuple[Check, ...], **kwargs: object) -> tuple[Result[Completed, Fault], ...]:
                    self.calls.append((member, (checks,), kwargs))
                    self.checks.extend(checks)
                    return results

                monkeypatch.setattr(owner, member, _fan)
            case "rail", Envelope() as env:

                def _factory(bind: object, settings: object) -> Callable[[object], Envelope]:
                    self.calls.append((member, (bind, settings), {}))

                    def _run(params: object) -> Envelope:
                        self.calls.append(("rail.run", (params,), {}))
                        return env

                    return _run

                monkeypatch.setattr(owner, member, _factory)
            case _, Result() as result:

                def _sync(*args: object, **kwargs: object) -> Result[object, Fault]:
                    self.calls.append((member, args, kwargs))
                    self.checks.extend(c for c in args[:1] if isinstance(c, Check))
                    return result

                monkeypatch.setattr(owner, member, _sync)
            case _:  # pragma: no cover  # the seam/payload pairings above are exhaustive for the rail laws
                raise TypeError(f"RailProbe.install: unsupported seam/payload pairing for {member!r}: {type(payload).__name__}")

    @property
    def commands(self) -> list[tuple[str, ...]]:
        """The ``tool.command`` argv of every captured ``Check``, in invocation order.

        Returns:
            One argv tuple per recorded ``Check`` — the analogue of the local ``invoked`` capture lists.
        """
        return [tuple(c.tool.command) for c in self.checks]

    @staticmethod
    def ok(argv: tuple[str, ...] = ("rasm-bridge", "check"), status: RailStatus = RailStatus.OK) -> Result[Completed, Fault]:
        """An ``Ok`` receipt outcome for a successful seam (default ``rasm-bridge check`` argv, OK status).

        Returns:
            ``Ok(Completed)`` carrying the given argv, ``rc=0``, and status.
        """
        return Ok(receipt(argv, 0, status=status))

    @staticmethod
    def receipt(
        argv: tuple[str, ...], rc: int = 0, *, status: RailStatus | None = None, stdout: bytes = b"", stderr: bytes = b""
    ) -> Result[Completed, Fault]:
        """An ``Ok`` outcome wrapping a wire-shaped ``Completed`` so a verb's output-parsing logic runs.

        ``status`` defaults to ``RailStatus.from_returncode(rc)``; pass ``stdout`` to feed the verb a realistic
        payload (canned ilspycmd output, MSBuild ``-getProperty`` JSON, build banners, …).

        Returns:
            ``Ok(Completed)`` carrying the argv, return code, status, and captured streams.
        """
        return Ok(receipt(argv, rc, status=status, stdout=stdout, stderr=stderr))

    @staticmethod
    def error(argv: tuple[str, ...], message: str, *, status: RailStatus = RailStatus.FAULTED) -> Result[Completed, Fault]:
        """An ``Error`` outcome wrapping a ``Fault`` so a verb's error rail runs against a spawn/timeout failure.

        Returns:
            ``Error(Fault)`` carrying the argv, status, and message.
        """
        return Error(Fault(argv, status, message))


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


# --- [PSUTIL_DOUBLES] ------------------------------------------------------------------
# ``_proc`` builds a create_autospec(psutil.Process) double from keyword fields — no hand-rolled API.
# ``_make_psutil_module`` wraps N pid->proc mappings into a MagicMock(spec=psutil) module double.


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
    (``_make_psutil_module``) raises ``NoSuchProcess`` instead of returning it.

    Returns:
        A ``psutil.Process`` autospec double carrying the requested field values.
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

    Returns:
        A ``psutil`` module double with a pid-dispatching ``Process`` factory.
    """
    fake = MagicMock(spec=_psutil)
    fake.Error = _psutil.Error  # real base class so `except psutil.Error, ...` stays catchable under the double
    fake.NoSuchProcess = _psutil.NoSuchProcess
    fake.AccessDenied = _psutil.AccessDenied
    fake.cpu_count.return_value = cpu_count

    def _process_factory(pid: int | None = None) -> MagicMock:
        proc = procs.get(pid)
        match proc:
            case None:
                raise _psutil.NoSuchProcess(pid if isinstance(pid, int) else 0)
            case _ if getattr(proc, "_raise_no_such", False):
                raise _psutil.NoSuchProcess(int(proc.pid))
            case _:
                return proc

    fake.Process.side_effect = _process_factory
    return fake


# --- [ORACLES] -------------------------------------------------------------------------
# Assay-TYPED oracles that do not generalize (generic value-level oracles come from tests._spec).

_ENVELOPE_DECODER: Final = msgspec.json.Decoder(Envelope)


def read_one_envelope_from_bytes(raw: bytes) -> Envelope:
    """Assert exactly one NDJSON line on the buffer and decode it as an Envelope.

    Returns:
        The single decoded Envelope.
    """
    rows = raw.splitlines()
    assert len(rows) == 1, f"expected exactly one stdout Envelope line, got {len(rows)}: {rows!r}"
    return _ENVELOPE_DECODER.decode(rows[0])


def read_one_envelope(cap: pytest.CaptureFixture[bytes] | pytest.CaptureFixture[str]) -> Envelope:
    """Drain a capsys/capsysbinary fixture, assert one stdout Envelope line, and decode it.

    Returns:
        The single decoded Envelope (bytes or str capture).
    """
    out = cap.readouterr().out
    return read_one_envelope_from_bytes(out if isinstance(out, bytes) else out.encode())


def assert_counts_consistent(report: Report) -> None:
    """Assert the report fold invariant: ``total == ok + failed`` and ``len(results) == failed``."""
    assert report.counts.total == report.counts.ok + report.counts.failed, f"counts arithmetic broken: {report.counts}"
    assert len(report.results) == report.counts.failed, f"defect rows != failed count: {len(report.results)} != {report.counts.failed}"


def make_history_envelope(run_id: str, *, claim: Claim = Claim.STATIC, status: RailStatus = RailStatus.OK) -> Envelope:
    """Build a deterministic persistable Envelope for a run_id (history/retention laws).

    Returns:
        An Envelope carrying a single-receipt OK report stamped with ``run_id``.
    """
    report = fold(claim, "check", (receipt(("tool",), 0, status=status),))
    return msgspec.structs.replace(wrap_envelope(report, claim=claim, verb="check"), run_id=run_id)


def pipe_history(store: ArtifactStore, run_ids: tuple[str, ...]) -> None:
    """Write a canned Envelope into the history tree for each run_id (delta/retention setup)."""
    for run_id in run_ids:
        store.write_bytes(WIRE_ENCODER.encode(make_history_envelope(run_id)), "history", run_id, "envelope.json")


# --- [COMPOSITION] ---------------------------------------------------------------------


@pytest.fixture(autouse=True)
def _isolate_sut_state() -> Generator[None]:
    """Reset every SUT module-level ContextVar + structlog context per test — they leak in-process.

    The CLI/``@logged``/``@checked``/engine seams bind into ContextVars (aspect ``_RING``, automation
    ``_CPU_PRIMED``, engine ``_RESOURCE``/``_SSH_CACHE``) and structlog's bound context that persist within
    the test process; forcing each to its declared default at test start keeps every ring / governor /
    resource / history law order-independent under pytest-randomly. One shared seam — the universal isolation
    the whole suite relies on (``_WRITES`` has no default and is owned per-invocation, so it is left alone).

    Yields:
        None — the test runs with all SUT ContextVars at their defaults + a cleared structlog context.
    """
    import structlog  # noqa: PLC0415

    from tools.assay.automation.engine import _CPU_PRIMED  # noqa: PLC0415, PLC2701
    from tools.assay.core.aspect import _RING  # noqa: PLC0415, PLC2701
    from tools.assay.core.engine import _RESOURCE, _SSH_CACHE  # noqa: PLC0415, PLC2701

    structlog.contextvars.clear_contextvars()
    tok_ring = _RING.set(None)
    tok_cpu = _CPU_PRIMED.set(False)
    tok_res = _RESOURCE.set(())
    tok_ssh = _SSH_CACHE.set(None)
    yield
    _RING.reset(tok_ring)
    _CPU_PRIMED.reset(tok_cpu)
    _RESOURCE.reset(tok_res)
    _SSH_CACHE.reset(tok_ssh)
    structlog.contextvars.clear_contextvars()


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
    """Isolated fsspec ``memory://`` ArtifactStore partitioned by ``run_id`` (direct store-function seam).

    Yields:
        An ``ArtifactStore`` backed by ``fsspec.MemoryFileSystem`` rooted at a ``run_id``-keyed prefix.
    """
    store = assay_root.settings.store(protocol="memory", root=f"mem-store/{assay_root.settings.run_id}")
    yield store
    if store.exists_path(store.root):
        store.remove_path(store.root, recursive=True)


@pytest.fixture
def cli(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes], monkeypatch: pytest.MonkeyPatch) -> VerbRunner:
    """Run the assay CLI and return its single stdout Envelope, exit code, and raw stdout/stderr.

    Default (``isolate=False``): drives ``main([*argv])`` in-process under ``ASSAY_ROOT=<root>``
    (~0.001s vs ~0.65s subprocess), reading the one Envelope from ``capsysbinary`` and the returned exit
    code. ``isolate=True`` spawns the real ``uv run python -m tools.assay`` subprocess via ``anyio.run``
    for the few argv/exit-code isolation laws. Synchronous: ``main`` opens its own ``anyio`` loop, so an
    async wrapper would raise "Already running asyncio". Structlog writes to stderr; one Envelope reaches
    stdout — both are preserved on the returned ``CliResult`` for channel-separation laws.

    Returns:
        Synchronous callable ``run(*argv, isolate=False, extra_env=None) -> CliResult``.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))

    def run(*argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> CliResult:
        for key, value in (extra_env or {}).items():
            monkeypatch.setenv(key, value)
        match isolate:
            case False:
                from tools.assay import __main__ as main_mod  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                # main() force_flushes + shuts down the global trace provider before exit (correct for a one-shot
                # process). In-process that would tear down the session provider the otel_spans fixture reads, so
                # neutralize ONLY the teardown here — span recording still flows through the global tracer.
                neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
                monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
                code = main_mod.main([*argv])
                cap = capsysbinary.readouterr()
                return CliResult(envelope=read_one_envelope_from_bytes(cap.out), exit_code=code, stdout=cap.out, stderr=cap.err)
            case True:
                if _UV is None:
                    pytest.skip("uv not on PATH")
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **(extra_env or {})}  # noqa: TID251  # test boundary: clone + override env for subprocess isolation
                # CWD is the repo root (so `python -m tools.assay` resolves the package); ASSAY_ROOT isolates I/O to tmp.
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                return CliResult(
                    envelope=read_one_envelope_from_bytes(result.stdout), exit_code=result.returncode, stdout=result.stdout, stderr=result.stderr
                )

    return run


@pytest.fixture
def rail_probe() -> RailProbe:
    """Build the one canned-output rail host: ``run_check``/``run_check_async``/``fan_out``/``rail`` seams.

    Returns:
        A fresh ``RailProbe`` whose ``install`` binds a canned seam as-imported-by an owner module and whose
        ``calls``/``checks``/``commands`` capture every invocation for assertions.
    """
    return RailProbe()


def install_cpu_double(monkeypatch: pytest.MonkeyPatch, cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
    """Install a ``psutil`` module double on ``automation.engine`` whose ``cpu_percent`` is ``cpu_percent``.

    Reuses ``_make_psutil_module`` semantics (real ``Error``/``NoSuchProcess`` classes, ``cpu_count``) so the
    governor laws drive ``is_governed`` against a canned non-blocking CPU sample with no real ``psutil`` read.

    Returns:
        The installed ``psutil`` module double; ``.cpu_percent`` is the supplied callable.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    fake = _make_psutil_module({}, cpu_count=cpu_count)
    fake.cpu_percent = cpu_percent
    monkeypatch.setattr(automation_engine, "psutil", fake)
    return fake


@pytest.fixture
def cpu_double(monkeypatch: pytest.MonkeyPatch) -> CpuDoubleInstaller:
    """Yield an installer that pins ``automation.engine``'s ``psutil.cpu_percent`` to a canned sample.

    Returns:
        ``install(cpu_percent, *, cpu_count=4) -> MagicMock`` — call it with the canned ``cpu_percent`` callable
        (e.g. ``lambda *_a, **_k: 91.0``) to drive the CPU governor decision deterministically.
    """

    def install(cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
        return install_cpu_double(monkeypatch, cpu_percent, cpu_count=cpu_count)

    return install


@pytest.fixture
def captured_emits(monkeypatch: pytest.MonkeyPatch) -> list[Envelope]:
    """Redirect ``automation.engine._emit`` to a capture list and yield it.

    Every Envelope the engine would write to stdout is appended instead, so a ``drive`` law asserts the exact
    emitted Envelope sequence without parsing NDJSON off ``capsysbinary``.

    Returns:
        The mutable ``list[Envelope]`` collecting each ``_emit`` call, in emission order.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    seen: list[Envelope] = []
    monkeypatch.setattr(automation_engine, "_emit", seen.append)
    return seen


@pytest.fixture
async def ssh_loopback(socket_enabled: None) -> AsyncGenerator[SshLoopback]:
    """Live loopback asyncssh server (network-marked): yields the ssh ``exec_target`` capsule.

    Uses ``asyncssh.listen`` as an async context manager — no daemon threads, no manual teardown, no
    ResourceWarning under ``filterwarnings=["error"]``. ``asyncssh`` is imported lazily here so the
    ~194ms import tax is paid only on network runs.

    Yields:
        ``SshLoopback`` carrying the bound port and ``ssh://x@127.0.0.1:<port>`` ``exec_target``.
    """
    _ = socket_enabled
    import asyncssh  # noqa: PLC0415  # lazy: keep the import off the non-network collection path

    class _Server(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            _ = username
            return False

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029
        # Echo a fixed token PLUS the received command so callers can assert the command transited.
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
