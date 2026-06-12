"""Assay test library payload: typed strategy registry, engine-backed seam doubles, and wire oracles.

The reusable seam MECHANISM lives in the project-agnostic ``tests.python._testkit.seams`` engine (recording
monkeypatch probe, psutil double, loopback capsule, payload-variant writer, tmp-root harness, NDJSON oracle).
This module owns the assay PAYLOAD that test modules import directly: the explicit ``<snake>_st`` strategy
globals (each a typed ``resolve(Pascal)`` so ``@given(X_st)`` resolves the overload), the bespoke ``binds_st``/
``detail_st``/``envelope_st`` strategies, and the thin assay specializations of each engine abstraction —
``RailProbe(SeamProbe[Check])`` + the Result builders, the ``_proc``/``_make_psutil_module``/
``install_cpu_double`` partials, ``SshLoopback`` over ``loopback_server``, ``BridgeResult`` over
``VariantWriter[str]``, ``AssayHarness`` over ``TmpRoot[AssaySettings]``, ``YakShape`` over ``TmpRoot.write``,
and the envelope/history oracles over ``NdjsonOracle``. The sibling ``conftest.py`` owns only pytest wiring:
SUT registration, fixtures, and the ``log_processors`` override.

Prefer ``resolve(X)`` over a hand-rolled strategy and the ``tests.python._testkit.spec`` oracles over
hand-rolled ``assert x.is_ok()``.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import (  # noqa: TC003  # runtime: Callable is a CpuSampler annotation; Generator is the RailProbe.project msgspec field type
    Callable,
    Generator,
)
from pathlib import Path
from types import SimpleNamespace
from typing import Final, override, Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

from expression import Error, Ok  # Error/Ok are runtime in the RailProbe canned-outcome builders
from hypothesis import strategies as st
import msgspec
import psutil as _psutil

from tests.python._testkit.seams import (
    Async as _Async,
    autospec_proc,
    Factory as _Factory,
    FanOut as _FanOut,
    install_module_attr,
    NdjsonOracle,
    psutil_module_double,
    SeamProbe,
    Sync,
    TmpRoot,
    VariantWriter,
)
from tests.python._testkit.strategies import resolve
from tools.assay.composition.registry import REGISTRY
from tools.assay.composition.settings import ArtifactScope, AssaySettings
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
    from collections.abc import Mapping

    from expression import Result
    import pytest

    from tests.python._testkit.seams import Shape as _Shape
    from tools.assay.composition.settings import ArtifactStore


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


# --- [CONSTANTS] ------------------------------------------------------------------------

# Mirrors model._ENCODER order so oracle bytes are byte-identical to production wire output.
WIRE_ENCODER = msgspec.json.Encoder(order="deterministic")

# --- [STRATEGIES] -----------------------------------------------------------------------
# Typed `resolve(Pascal)` aliases so `@given(X_st)` resolves the Hypothesis overload statically;
# lazy `__getattr__` attributes return untyped `SearchStrategy` and defeat overload resolution.


rail_status_st: st.SearchStrategy[RailStatus] = resolve(RailStatus)
completed_st: st.SearchStrategy[Completed] = resolve(Completed)
fault_st: st.SearchStrategy[Fault] = resolve(Fault)
counts_st: st.SearchStrategy[Counts] = resolve(Counts)
artifact_st: st.SearchStrategy[Artifact] = resolve(Artifact)
match_st: st.SearchStrategy[Match] = resolve(Match)
run_snapshot_st: st.SearchStrategy[RunSnapshot] = resolve(RunSnapshot)
report_st: st.SearchStrategy[Report] = resolve(Report)
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

# `binds_st` samples the live registry; `envelope_st` composes via the canonical projection — neither maps to a single `resolve(T)`.
binds_st = st.sampled_from(REGISTRY)
detail_st: st.SearchStrategy[AnyDetail] = resolve(AnyDetail)


@st.composite
def _envelopes(draw: st.DrawFn) -> Envelope:
    """Draw a wire-consistent Envelope from a Report or Fault via the canonical ``envelope`` projection.

    Resolves its payload strategies directly (not via the module `report_st`/`fault_st` globals) so the
    draw is self-contained.

    Returns:
        Envelope whose status/exit_code derive from the wrapped payload.
    """
    payload: Report | Fault = draw(st.one_of(resolve(Report), resolve(Fault)))
    return wrap_envelope(payload, claim=draw(resolve(Claim)), verb=draw(st.text(max_size=32)), run_id=draw(st.text(max_size=32)))


envelope_st: st.SearchStrategy[Envelope] = _envelopes()
st.register_type_strategy(Envelope, envelope_st)

# --- [MODELS] ---------------------------------------------------------------------------


def _pick_check(args: tuple[object, ...]) -> Generator[Check]:
    return (a for a in args[:1] if isinstance(a, Check))


class CliResult(msgspec.Struct, frozen=True, gc=False):
    """CLI invocation result carrying the decoded Envelope, exit code, and raw channel bytes."""

    envelope: Envelope
    exit_code: int
    stdout: bytes
    stderr: bytes


class AssayHarness(TmpRoot[AssaySettings], frozen=True, gc=False):
    """Isolated tmp-tree capsule over ``TmpRoot`` whose ``exec_target=""`` settings mutate nothing outside ``root``."""

    def scope(self, claim: Claim = Claim.PACKAGE) -> ArtifactScope:
        """Open an ArtifactScope rooted at this harness's settings.

        Returns:
            ArtifactScope bound to this harness's root and the given claim.
        """
        return ArtifactScope.open(self.settings, claim)

    def remote(self, exec_target: str) -> AssaySettings:
        """Return a copy of settings with exec_target set and exec_known_hosts cleared for SSH law fixtures.

        Returns:
            AssaySettings copy with the given exec_target and no known-hosts constraint.
        """
        return self.settings.model_copy(update={"exec_target": exec_target, "exec_known_hosts": None})

    @staticmethod
    def envelope_of(payload: Report | Fault, *, claim: Claim, verb: str) -> Envelope:
        """Wrap payload in an Envelope via the canonical projection; run_id is auto-generated.

        Returns:
            Envelope whose status and exit_code derive from the payload type.
        """
        return wrap_envelope(payload, claim=claim, verb=verb)


class RailProbe(SeamProbe[Check], frozen=True, gc=False):
    """Socket-free canned-seam host over ``SeamProbe`` — projects the first positional ``Check`` per call.

    The patch target is the OWNER module that re-binds the seam (``rails.api``/``automation.engine``/
    ``core.engine``), never the definition site, mirroring how production resolves the name. ``install``
    promotes the member name to a ``Shape`` variant (``run_check_async`` → awaited; ``rail`` → the
    ``(bind, settings) -> (params) -> Envelope`` factory recording a ``rail.run`` call-layer; a tuple-of-Results
    payload → fan-out; everything else → sync) — this member→shape map is documented ASSAY POLICY, not an engine
    catch-all.
    """

    project: Callable[[tuple[object, ...]], Generator[Check]] = _pick_check

    @override
    def install(  # ty: ignore[invalid-method-override]  # the 4-arg member-string shim narrows the engine's Shape param to a canned assay payload
        self,
        monkeypatch: pytest.MonkeyPatch,
        owner: object,
        member: str,
        payload: Result[object, Fault] | Envelope | tuple[Result[Completed, Fault], ...],
    ) -> None:
        """Bind ``owner.member`` to a canned seam mirroring the production call shape of ``member``.

        ``run_check``/``run_check_async`` accept a ``Result`` payload; ``rail`` accepts a canned ``Envelope``
        and installs the ``(bind, settings) -> (params) -> Envelope`` factory; a tuple-of-Results payload binds a
        fan-out seam so a future fan-out member is not silently doubled as ``Sync``. Each call is appended to
        ``calls`` and any first-positional ``Check`` to ``captured`` (read as ``checks``).
        """
        match member:
            case "run_check_async":
                shape: _Shape[object] = _Async[object](value=payload)
            case "rail":
                shape = _Factory[object](value=payload, inner_label="rail.run")
            case _:
                shape = _FanOut[object](values=payload) if isinstance(payload, tuple) else Sync[object](value=payload)
        super().install(monkeypatch, owner, member, shape)

    @property
    def checks(self) -> list[Check]:
        return self.captured

    @property
    def commands(self) -> list[tuple[str, ...]]:
        return [tuple(c.tool.command) for c in self.captured]

    @staticmethod
    def ok(argv: tuple[str, ...] = ("rasm-bridge", "check"), status: RailStatus = RailStatus.OK) -> Result[Completed, Fault]:
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
        return Error(Fault(argv, status, message))


class SshLoopback(msgspec.Struct, frozen=True, gc=False):
    """A live loopback asyncssh server's bound port + ssh ``exec_target`` projection.

    ``exec_target`` includes a username because asyncssh ``saslprep`` rejects ``None`` at connect.
    """

    port: int

    @property
    def exec_target(self) -> str:
        """SSH exec_target URI with an explicit username required by asyncssh saslprep."""
        return f"ssh://x@127.0.0.1:{self.port}"


class BridgeResult(msgspec.Struct, frozen=True, gc=False):
    """``_BridgeResult`` JSON writer over ``VariantWriter`` — one valid payload plus three adversarial variants."""

    directory: Path

    def valid(self, command: str = "scenario.verify.csx") -> Path:
        """Write a well-formed bridge result JSON and return its path.

        Returns:
            Path to the written valid.json file.
        """
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
        return self._writer({"valid": "valid.json"}, {"valid": payload}).path("valid")

    def malformed(self) -> Path:
        """Write a bridge result containing invalid JSON and return its path.

        Returns:
            Path to the written malformed.json file.
        """
        return self._writer({"malformed": "malformed.json"}, {"malformed": b"{not json"}).path("malformed")

    def partial(self) -> Path:
        """Write a bridge result with a valid-JSON but structurally incomplete payload and return its path.

        Returns:
            Path to the written partial.json file.
        """
        return self._writer({"partial": "partial.json"}, {"partial": b"{}"}).path("partial")

    def missing(self) -> Path:
        """Return a path for a bridge result whose backing file is absent (never written).

        Returns:
            Path that maps to absent.json, which is not created on disk.
        """
        return self._writer({"missing": "absent.json"}, {}, absent=frozenset({"missing"})).path("missing")

    def _writer(
        self, names: Mapping[str, str], payloads: Mapping[str, bytes | object], *, absent: frozenset[str] = frozenset()
    ) -> VariantWriter[str]:
        return VariantWriter(directory=self.directory, names=names, payloads=payloads, absent=absent)


class YakShape(msgspec.Struct, frozen=True, gc=False):
    """Fake-yak + fake-msbuild materializer built on the harness ``TmpRoot.write`` primitive."""

    slug: str = "rasm-bridge"
    project: Path = Path("apps/bridge/plugin.csproj")
    assembly_name: str = "Rasm"
    target_ext: str = ".rhp"
    target_framework: str = "net10.0"
    package_pattern: str = "rasm-rh9_1-mac.yak"

    def props(self, meta: package_rail.YakMeta) -> dict[str, str]:
        """Return the MSBuild/yak property dict corresponding to a materialized YakMeta."""
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
        """Write a fake yak binary, project file, manifest, and assembly tree under harness.root.

        Returns:
            YakMeta with paths pointing into the harness tmp-tree.
        """
        yak = harness.write("yak", "#!/bin/sh\nexit 0\n", mode=0o755)
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


# --- [PSUTIL_DOUBLES] -------------------------------------------------------------------


def _proc(
    *,
    pid: int = 12345,
    rss: int = 4096,
    fds: int = 8,
    cpu: float = 0.0,
    running: bool = True,
    status: str = _psutil.STATUS_RUNNING,
    create_time: float = 1_700_000_000.0,
    raise_no_such: bool = False,
) -> MagicMock:
    """Return a ``create_autospec(psutil.Process)`` double with the given field values.

    ``status`` feeds ``Process.status()`` (default ``STATUS_RUNNING``) so staleness laws can model
    ``STATUS_ZOMBIE``/``STATUS_DEAD`` processes. ``raise_no_such=True`` marks the double as a
    dead-process sentinel: the module-level factory (``_make_psutil_module``) raises ``NoSuchProcess``
    instead of returning it.
    """
    return autospec_proc(
        _psutil.Process,
        fields={"pid": pid},
        methods={
            "memory_info": SimpleNamespace(rss=rss),
            "num_fds": fds,
            "cpu_percent": cpu,
            "is_running": running,
            "status": status,
            "create_time": create_time,
        },
        dead=raise_no_such,
    )


def _make_psutil_module(procs: dict[int | None, MagicMock], *, cpu_count: int = 4) -> MagicMock:
    """Return a ``MagicMock(spec=psutil)`` module double that dispatches ``Process(pid)`` via ``procs``.

    ``procs[None]`` is the self-process (``Process()`` / ``Process(os.getpid())``); integer keys are
    specific pids. An unregistered pid causes ``NoSuchProcess`` — no silent fallback.
    ``raise_no_such=True`` on a double causes the factory to raise ``NoSuchProcess(pid)`` on lookup.
    """
    return psutil_module_double(
        _psutil,
        procs,
        not_found=lambda pid: _psutil.NoSuchProcess(int(pid) if isinstance(pid, int) else 0),
        extra={
            "Error": _psutil.Error,
            "NoSuchProcess": _psutil.NoSuchProcess,
            "AccessDenied": _psutil.AccessDenied,
            "cpu_count": MagicMock(return_value=cpu_count),
        },
    )


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
    return install_module_attr(monkeypatch, automation_engine, "psutil", fake)


# --- [ORACLES] --------------------------------------------------------------------------

_ENV_ORACLE: Final = NdjsonOracle(msgspec.json.Decoder(Envelope))
read_one_envelope_from_bytes = _ENV_ORACLE.one
read_one_envelope = _ENV_ORACLE.from_capture


def assert_counts_consistent(report: Report) -> None:
    """Assert the report fold invariant: ``total == ok + failed`` and ``len(results) == failed``."""
    assert report.counts.total == report.counts.ok + report.counts.failed, f"counts arithmetic broken: {report.counts}"
    assert len(report.results) == report.counts.failed, f"defect rows != failed count: {len(report.results)} != {report.counts.failed}"


def make_history_envelope(run_id: str, *, claim: Claim = Claim.STATIC, status: RailStatus = RailStatus.OK) -> Envelope:
    """Build a canned history Envelope with the given run_id, claim, and status for delta/retention fixtures.

    Returns:
        Envelope with run_id replaced to the given value and status/exit_code derived from the payload.
    """
    report = fold(claim, "check", (receipt(("tool",), 0, status=status),))
    return msgspec.structs.replace(wrap_envelope(report, claim=claim, verb="check"), run_id=run_id)


def pipe_history(store: ArtifactStore, run_ids: tuple[str, ...]) -> None:
    """Write a canned Envelope into the history tree for each run_id (delta/retention setup)."""
    [store.write_bytes(WIRE_ENCODER.encode(make_history_envelope(run_id)), "history", run_id, "envelope.json") for run_id in run_ids]
