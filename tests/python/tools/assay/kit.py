"""Assay test payloads: typed strategies, seam doubles, harnesses, and wire oracles."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable, Generator  # noqa: TC003  # runtime: Protocol and msgspec fields resolve these annotations
from pathlib import Path
from types import SimpleNamespace
from typing import Final, override, Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

from expression import Error, Ok  # Error/Ok are runtime in the RailProbe canned-outcome builders
from hypothesis import strategies as st
import msgspec
import psutil as _psutil
from upath import UPath

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
)
from tests.python._testkit.strategies import resolve
from tools.assay.composition.registry import REGISTRY
from tools.assay.composition.settings import AssaySettings, Ssh
from tools.assay.composition.store import ArtifactScope
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
    ExecReceipt,
    Fault,
    Match,
    PackageRun,
    ProvisionRun,
    RailStatus,
    receipt,
    Report,
    RunDelta,
    RunSnapshot,
    Stage,
    StaticRun,
    TestRun,
    Tool,
    VerifySummary,
)
from tools.assay.diagnostics import fold
from tools.assay.rails import package as package_rail


if TYPE_CHECKING:
    from expression import Result
    import pytest

    from tests.python._testkit.seams import Shape as _Shape
    from tools.assay.composition.store import ArtifactStore


# --- [TYPES] ----------------------------------------------------------------------------


class VerbRunner(Protocol):
    """Synchronous CLI fixture that returns decoded wire output plus raw channels."""

    def __call__(
        self, *argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None, executor: SeamExecutor | None = None
    ) -> CliResult: ...


class CpuSampler(Protocol):
    """Canned ``psutil.cpu_percent`` callable for warmup and non-blocking reads."""

    def __call__(self, interval: float | None = None) -> float: ...


class CpuDoubleInstaller(Protocol):
    """Fixture surface that installs a canned psutil module on ``automation.engine``."""

    def __call__(self, cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

# Mirrors model._ENCODER order so oracle bytes are byte-identical to production wire output.
WIRE_ENCODER = msgspec.json.Encoder(order="deterministic")

# --- [TABLES]
# Typed aliases keep ``@given(X_st)`` overload resolution; lazy strategy attributes do not.


rail_status_st: st.SearchStrategy[RailStatus] = resolve(RailStatus)
completed_st: st.SearchStrategy[Completed] = resolve(Completed)
exec_receipt_st: st.SearchStrategy[ExecReceipt] = resolve(ExecReceipt)
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
provision_run_st: st.SearchStrategy[ProvisionRun] = resolve(ProvisionRun)
diagnostic_st: st.SearchStrategy[Diagnostic] = resolve(Diagnostic)
run_delta_st: st.SearchStrategy[RunDelta] = resolve(RunDelta)
static_run_st: st.SearchStrategy[StaticRun] = resolve(StaticRun)

# Registry rows and Envelope composition do not map to a single ``resolve(T)`` strategy.
binds_st = st.sampled_from(REGISTRY)
detail_st: st.SearchStrategy[AnyDetail] = resolve(AnyDetail)


@st.composite
def _envelopes(draw: st.DrawFn) -> Envelope:
    """Draw a wire-consistent Envelope through the production projection.

    Returns:
        Envelope whose status and exit code derive from a Report or Fault payload.
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


def assay_settings(root: Path) -> AssaySettings:
    """Build tmp-rooted settings with local execution and no known-hosts constraint.

    Returns:
        Settings rooted at ``root`` with remote execution disabled.
    """
    (root / "Workspace.slnx").write_text("", encoding="utf-8")
    # Isolate the machine-wide dotnet slot lock root under the tmp tree so tests never contend on the real ~/.rasm pool.
    return AssaySettings(root=UPath(root), exec_known_hosts=None, machine_lock_root=root / ".rasm" / "locks")


class AssayHarness(TmpRoot[AssaySettings], frozen=True, gc=False):
    """Tmp-root harness whose default settings cannot mutate outside ``root``."""

    def scope(self, claim: Claim = Claim.PACKAGE) -> ArtifactScope:
        """Open an ArtifactScope rooted at this harness.

        Returns:
            Scope bound to the harness settings for ``claim``.
        """
        return ArtifactScope.open(self.settings, claim)

    def remote(self, exec_target: str) -> AssaySettings:
        """Copy settings for SSH law fixtures, binding the parsed Ssh target without a known-hosts constraint.

        Returns:
            Settings whose modal exec_target is the Ssh value object parsed from ``exec_target``.
        """
        target = Ssh.parse(exec_target).model_copy(update={"known_hosts": None})
        return self.settings.model_copy(update={"exec_target": target, "exec_known_hosts": None})

    def supervisor(self) -> Path:
        """Materialize the built supervisor binary so ``client_run`` resolves its real spawn target.

        Returns:
            The materialized binary path under the bridge build scope pivot.
        """
        pivot = f"{self.settings.configuration.value.lower()}_test-rid"
        binary = Path(str(ArtifactScope.build(self.settings, "bridge").path)) / "bin" / "Supervisor" / pivot / "Rasm.Bridge.Supervisor"
        binary.parent.mkdir(parents=True, exist_ok=True)
        binary.write_bytes(b"")
        return binary

    @staticmethod
    def envelope_of(payload: Report | Fault, *, claim: Claim, verb: str) -> Envelope:
        """Wrap a payload through the canonical Envelope projection.

        Returns:
            Envelope carrying ``payload`` under ``claim``/``verb``.
        """
        return wrap_envelope(payload, claim=claim, verb=verb)


class RailProbe(SeamProbe[Check], frozen=True, gc=False):
    """Canned assay seam host that captures the first positional ``Check`` per call."""

    project: Callable[[tuple[object, ...]], Generator[Check]] = _pick_check

    @override
    def install(  # ty: ignore[invalid-method-override]  # the 4-arg member-string shim narrows the engine's Shape param to a canned assay payload
        self,
        monkeypatch: pytest.MonkeyPatch,
        owner: object,
        member: str,
        payload: Result[object, Fault] | Envelope | tuple[Result[Completed, Fault], ...],
    ) -> None:
        """Install the canned seam shape selected by the production member contract."""
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
        # The filled command is the spawned argv body; holeless rows fill to identity.
        return [tuple(c.args.fill(c.tool.command)) for c in self.captured]

    @staticmethod
    def ok(argv: tuple[str, ...] = ("rasm-bridge", "check"), status: RailStatus = RailStatus.OK) -> Result[Completed, Fault]:
        return Ok(receipt(argv, 0, status=status))

    @staticmethod
    def receipt(
        argv: tuple[str, ...], rc: int = 0, *, status: RailStatus | None = None, stdout: bytes = b"", stderr: bytes = b""
    ) -> Result[Completed, Fault]:
        """Build an ``Ok(Completed)`` outcome that still drives verb output parsing.

        ``status`` defaults from ``rc``; ``stdout`` carries realistic payloads such as canned build output.

        Returns:
            Ok-wrapped completed receipt for the canned argv.
        """
        return Ok(receipt(argv, rc, status=status, stdout=stdout, stderr=stderr))

    @staticmethod
    def error(argv: tuple[str, ...], message: str, *, status: RailStatus = RailStatus.FAULTED) -> Result[Completed, Fault]:
        return Error(Fault(argv, status, message))


class SeamExecutor(msgspec.Struct, frozen=True, gc=False):
    """Canned Executor port for rail laws: forwards each call verbatim to the lifted lane callable.

    A lane left ``None`` fails loudly when the rail reaches it, so a law canning only ``fan`` proves the rail
    never spawned a single check (and vice versa).
    """

    run_fn: Callable[..., object] | None = None
    fan_fn: Callable[..., object] | None = None

    def run(self, *args: object, **kwargs: object) -> Result[Completed, Fault]:
        """Play the canned single-check lane with the rail's exact call shape.

        Returns:
            The canned run outcome for this call.
        """
        assert self.run_fn is not None, "rail reached executor.run but this SeamExecutor cans no run lane"
        return self.run_fn(*args, **kwargs)  # type: ignore[return-value]  # ty: ignore[invalid-return-type]  # canned lane owns the Result shape

    def fan(self, *args: object, **kwargs: object) -> tuple[Result[Completed, Fault], ...]:
        """Play the canned batch lane with the rail's exact call shape.

        Returns:
            The canned per-check outcome slots for this call.
        """
        assert self.fan_fn is not None, "rail reached executor.fan but this SeamExecutor cans no fan lane"
        return self.fan_fn(*args, **kwargs)  # type: ignore[return-value]  # ty: ignore[invalid-return-type]  # canned lane owns the Result shape


class YakShape(msgspec.Struct, frozen=True, gc=False):
    """Fake yak and MSBuild tree materializer."""

    slug: str = "rasm-bridge"
    project: Path = Path("apps/bridge/plugin.csproj")
    assembly_name: str = "Rasm"
    target_ext: str = ".rhp"
    target_framework: str = "net10.0"
    package_pattern: str = "rasm-rh9_1-mac.yak"

    def props(self, meta: package_rail.YakMeta) -> dict[str, str]:
        """Project materialized YakMeta into the MSBuild/yak property map.

        Returns:
            Property map mirroring the MSBuild/yak names for ``meta``.
        """
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
        """Write the fake yak binary, project file, manifest, and assembly tree.

        Returns:
            Materialized YakMeta describing the written tree.
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


# --- [OPERATIONS] -----------------------------------------------------------------------


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
    """Build a process double with status and liveness controls.

    ``raise_no_such`` marks the double as a dead-process sentinel for the module factory.

    Returns:
        Autospec ``psutil.Process`` double with the configured fields and methods.
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
    """Build a psutil module double whose ``Process(pid)`` dispatches through ``procs``.

    ``procs[None]`` is the self-process; unregistered or dead-sentinel pids raise ``NoSuchProcess``.

    Returns:
        Module double exposing the canned ``Process`` dispatch and psutil error classes.
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
    """Install a psutil module double on ``automation.engine``.

    The double keeps real psutil error classes while replacing ``cpu_percent`` with the supplied sampler.

    Returns:
        The installed module double for further per-test configuration.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    fake = _make_psutil_module({}, cpu_count=cpu_count)
    fake.cpu_percent = cpu_percent
    return install_module_attr(monkeypatch, automation_engine, "psutil", fake)


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENV_ORACLE: Final = NdjsonOracle(msgspec.json.Decoder(Envelope))
read_one_envelope_from_bytes = _ENV_ORACLE.one
read_one_envelope = _ENV_ORACLE.from_capture


def assert_counts_consistent(report: Report) -> None:
    """Assert the report fold invariant: ``total == ok + failed`` and ``len(results) == failed``."""
    assert report.counts.total == report.counts.ok + report.counts.failed, f"counts arithmetic broken: {report.counts}"
    assert len(report.results) == report.counts.failed, f"defect rows != failed count: {len(report.results)} != {report.counts.failed}"


def make_history_envelope(run_id: str, *, claim: Claim = Claim.STATIC, status: RailStatus = RailStatus.OK) -> Envelope:
    """Build a canned history Envelope for delta and retention fixtures.

    Returns:
        Envelope stamped with ``run_id`` for history-tree fixtures.
    """
    report = fold(claim, "check", (receipt(("tool",), 0, status=status),))
    return msgspec.structs.replace(wrap_envelope(report, claim=claim, verb="check"), run_id=run_id)


def pipe_history(store: ArtifactStore, run_ids: tuple[str, ...]) -> None:
    """Write a canned Envelope into the history tree for each run_id (delta/retention setup)."""
    [store.write_bytes(WIRE_ENCODER.encode(make_history_envelope(run_id)), "history", run_id, "envelope.json") for run_id in run_ids]
