"""Assay suite wiring: SUT registration, the typed strategy registry, and the engine-backed seam doubles.

The reusable seam MECHANISM now lives in the project-agnostic ``tests.python._testkit._seams`` engine (recording monkeypatch
probe, psutil double, loopback capsule, payload-variant writer, tmp-root harness, NDJSON oracle). This module
owns only the assay PAYLOAD: the SUT registration, the explicit ``<snake>_st`` strategy globals (each a typed
``resolve(Pascal)`` so ``@given(X_st)`` resolves the overload), the bespoke ``binds_st``/``detail_st``/
``envelope_st`` strategies, and the thin assay specializations of each engine abstraction —
``RailProbe(SeamProbe[Check])`` + the Result builders, the ``_proc``/``_make_psutil_module``/
``install_cpu_double`` partials, ``SshLoopback`` over ``loopback_server``, ``BridgeResult`` over
``VariantWriter[str]``, ``AssayHarness`` over ``TmpRoot[AssaySettings]``, ``YakShape`` over ``TmpRoot.write``,
and the envelope/history oracles over ``NdjsonOracle``.

Prefer ``resolve(X)`` over a hand-rolled strategy and the ``tests.python._testkit._spec`` oracles over hand-rolled
``assert x.is_ok()``.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import (  # noqa: TC003  # runtime: Callable is a CpuSampler annotation; Generator is the RailProbe.project msgspec field type
    Callable,
    Generator,
)
import functools
from itertools import starmap
import operator
import os
from pathlib import Path
import shutil
from types import SimpleNamespace
from typing import Final, override, Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

import anyio
from expression import Error, Ok  # Error/Ok are runtime in the RailProbe canned-outcome builders
from hypothesis import strategies as st
import msgspec
import psutil as _psutil
import pytest
from upath import UPath

from tests.python._testkit._aspect import register_sut  # sibling test-internal module; `_`-named by S1 design
from tests.python._testkit._seams import (  # sibling seam engine; `_`-named by S1 design
    Async as _Async,
    autospec_proc,
    Factory as _Factory,
    FanOut as _FanOut,
    install_module_attr,
    loopback_server,
    NdjsonOracle,
    psutil_module_double,
    SeamProbe,
    Sync,
    TmpRoot,
    VariantWriter,
)
from tests.python._testkit._spec import (  # sibling test-internal module re-exported so assay law files import oracles from one DNA surface
    assert_error,
    assert_error_status,
    assert_none,
    assert_ok,
    assert_roundtrip,
    assert_some,
)
from tests.python._testkit._strategies import resolve  # sibling test-internal module; `_`-named by S1 design
from tests.python._testkit.runtime import REPO_ROOT
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
    from collections.abc import AsyncGenerator, Awaitable, Mapping
    from contextvars import ContextVar

    from expression import Result

    from tests.python._testkit._seams import Shape as _Shape
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

_UV = shutil.which("uv")

# Mirrors model._ENCODER order so oracle bytes are byte-identical to production wire output.
WIRE_ENCODER = msgspec.json.Encoder(order="deterministic")

# The assay SUT package whose public surface the law-coverage gate (tests.python._testkit._aspect) walks.
SUT_PACKAGE: Final = "tools.assay"

# Callable/type aliases and ContextVar seams with no independently testable behavior; kept minimal.
# `Bind` exempts aspect.Bind (alias) only; model.Bind (a real binding) still earns a law in S3b.
_EXEMPT: Final = frozenset({
    "Attrs", "Bind", "Hom", "Layer", "Inversion",   # aspect: type aliases + decoration-time TypeError
    "ByteRecv", "_RESOURCE",                        # engine: Callable alias + resource ContextVar
    "InprocThunk",                                  # model: Callable alias
    "Handler",                                      # registry: Callable alias
    "Fire", "Worker", "ChangeBatch",                # automation/engine: Callable + type aliases
    "_CHECKED_LAYER", "_RING",                      # aspect: ContextVar seams (assembled layer + ring buffer)
    "_HINT_CAP", "_RESULT_CAP",                     # model: int caps; _HINT_CAP exercised via field_cap
})  # fmt: skip

register_sut(SUT_PACKAGE, exempt=_EXEMPT)


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


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(autouse=True)
def _isolate_sut_state() -> Generator[None]:
    """Reset every SUT module-level ContextVar + structlog context per test — they leak in-process.

    The CLI/``@logged``/``@checked``/engine seams bind into ContextVars (aspect ``_RING``, automation
    ``_CPU_PRIMED``, engine ``_RESOURCE``/``_SSH_CACHE``) and structlog's bound context that persist within
    the test process; forcing each to its declared default at test start keeps every ring / governor /
    resource / history law order-independent under pytest-randomly. One shared seam — the universal isolation
    the whole suite relies on (``_WRITES`` has no default and is owned per-invocation, so it is left alone).
    """
    import structlog  # noqa: PLC0415

    from tools.assay.automation.engine import _CPU_PRIMED  # noqa: PLC0415
    from tools.assay.core.aspect import _RING  # noqa: PLC0415
    from tools.assay.core.engine import _RESOURCE, _SSH_CACHE  # noqa: PLC0415

    def _arm[T](var: ContextVar[T], *, default: T) -> Callable[[], None]:
        token = var.set(default)
        return lambda: var.reset(token)

    structlog.contextvars.clear_contextvars()
    undo = (_arm(_RING, default=None), _arm(_CPU_PRIMED, default=False), _arm(_RESOURCE, default=()), _arm(_SSH_CACHE, default=None))
    yield
    [reset() for reset in undo]
    structlog.contextvars.clear_contextvars()


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Isolated AssayHarness rooted at tmp_path with exec_target="" and a stub Workspace.slnx.

    Returns:
        AssayHarness capsule wrapping the isolated tmp settings tree.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), exec_target="", exec_known_hosts=None)
    return AssayHarness(root=tmp_path, settings=settings)


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """In-memory ArtifactStore scoped to this test's run_id; removes its root on teardown.

    Yields:
        ArtifactStore backed by the memory protocol under a per-run-id root.
    """
    store = assay_root.settings.store(protocol="memory", root=f"mem-store/{assay_root.settings.run_id}")
    yield store
    store.remove_path(store.root, recursive=True) if store.exists_path(store.root) else None


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
        list(starmap(monkeypatch.setenv, (extra_env or {}).items()))
        match isolate:
            case False:
                from tools.assay import __main__ as main_mod  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                # main() shuts down the global tracer provider; in-process that tears down the session provider otel_spans reads.
                # Neutralize teardown while keeping recording.
                neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
                monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
                code = main_mod.main([*argv])
                cap = capsysbinary.readouterr()
                return CliResult(envelope=read_one_envelope_from_bytes(cap.out), exit_code=code, stdout=cap.out, stderr=cap.err)
            case True:
                if _UV is None:
                    pytest.skip("uv not on PATH")
                # Subprocess coverage credit: when the parent session runs under `coverage run` (COVERAGE_RUN set),
                # the child measures via site-packages' a1_coverage.pth + the parallel-mode subprocess ini.
                cov_env = {"COVERAGE_PROCESS_START": str(REPO_ROOT / ".config" / "coverage-subprocess.ini")} if "COVERAGE_RUN" in os.environ else {}  # noqa: TID251
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **cov_env, **(extra_env or {})}  # noqa: TID251  # subprocess env clone
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                return CliResult(
                    envelope=read_one_envelope_from_bytes(result.stdout), exit_code=result.returncode, stdout=result.stdout, stderr=result.stderr
                )

    return run


@pytest.fixture
def rail_probe() -> RailProbe:
    """Fresh RailProbe for canned-seam installation and call capture in a single test.

    Returns:
        Empty RailProbe with no recorded calls or captured checks.
    """
    return RailProbe()


@pytest.fixture
def cpu_double(monkeypatch: pytest.MonkeyPatch) -> CpuDoubleInstaller:
    """CpuDoubleInstaller bound to this test's monkeypatch; call it to pin automation.engine's psutil.

    Returns:
        Callable that installs a psutil module double with a canned cpu_percent sampler.
    """

    def install(cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
        return install_cpu_double(monkeypatch, cpu_percent, cpu_count=cpu_count)

    return install


@pytest.fixture
def captured_emits(monkeypatch: pytest.MonkeyPatch) -> list[Envelope]:
    """Redirect ``automation.engine._emit`` to a recording ``SeamProbe`` sink and yield its capture list.

    The engine's ``_emit(envelope)`` is patched to a ``Sync(None)`` seam whose ``project`` captures the sole
    positional Envelope (``args[:1]``) per call, so ``captured`` is a live ``list[Envelope]`` matching the HEAD
    semantics — a ``drive`` law asserts the exact emitted sequence without parsing NDJSON off ``capsysbinary``.

    Returns:
        The live ``list[Envelope]`` collecting each ``_emit`` call's Envelope, in emission order.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    probe: SeamProbe[Envelope] = SeamProbe(project=operator.itemgetter(slice(1)))
    probe.install(monkeypatch, automation_engine, "_emit", Sync(None))
    return probe.captured


@pytest.fixture
async def ssh_loopback(socket_enabled: None) -> AsyncGenerator[SshLoopback]:
    """Live loopback asyncssh server (network-marked): yields the ssh ``exec_target`` capsule.

    Wraps ``asyncssh.listen`` in the engine's ``loopback_server`` async-context lifecycle — no daemon threads,
    no manual teardown, no ResourceWarning under ``filterwarnings=["error"]``. ``asyncssh`` is imported lazily
    so the ~194ms import tax is paid only on network runs.

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

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029  # no await; asyncssh drives the handler synchronously
        command = process.command or ""
        process.stdout.write(f"remote-ok:{command}\n")
        process.exit(0)

    key = asyncssh.generate_private_key("ssh-ed25519")

    def _listen() -> Awaitable[asyncssh.SSHAcceptor]:
        # asyncssh.listen returns an _ACMWrapper (Awaitable, not Coroutine); SSHAcceptor is its awaited type.
        return asyncssh.listen("127.0.0.1", 0, server_host_keys=[key], server_factory=_Server, process_factory=_handler)

    def _port(server: asyncssh.SSHAcceptor) -> int:
        return server.get_port()

    # ty cannot resolve loopback_server's S typevar through @asynccontextmanager; runtime is sound.
    async with loopback_server(_listen, _port) as lb:  # ty: ignore[invalid-argument-type]
        yield SshLoopback(port=lb.port)


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """BridgeResult writer rooted at tmp_path/verify for adversarial bridge-parse laws.

    Returns:
        BridgeResult capsule whose variant methods write into tmp_path/verify.
    """
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Default YakShape materializer for package-rail laws.

    Returns:
        YakShape with default slug, project path, and assembly/framework settings.
    """
    return YakShape()
