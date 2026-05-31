"""Rhino bridge client, scenario verify, and RhinoWIP API metadata rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from pathlib import Path
import re
import shutil
import time
from typing import Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import Completed, decode_json, dotnet, dotnet_build, fd_args, fold, ProcessFault, ProjectIndex, Workspace
from tools.quality.settings import ArtifactScope, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type BridgeStatus = Literal["ok", "skipped", "unsupported", "failed", "timeout", "busy"]


# --- [MODELS] ----------------------------------------------------------------------------


class BridgeFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    category: str = ""
    message: str = ""
    type: str = ""
    stack_trace: str = ""


class BridgeGhSolution(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    document_in_play: bool = False
    object_count: int = 0
    warning_count: int | None = None
    error_count: int | None = None
    state: str = ""


class BridgeRuntimeDiagnostics(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command_window: tuple[str, ...] = ()
    exception_reports: tuple[BridgeFault, ...] = ()
    gh: BridgeGhSolution | None = None


class BridgeOutput(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    source: str = ""
    text: str = ""
    truncated: bool = False
    length: int = 0
    limit: int = 0


class BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str
    status: BridgeStatus
    duration_ms: float = 0.0
    data: dict[str, object] | None = None
    outputs: tuple[BridgeOutput, ...] = ()
    fault: BridgeFault | None = None


class BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command: str = ""
    status: BridgeStatus = "failed"
    report_path: str = ""
    phases: tuple[BridgePhase, ...] = ()
    fault: BridgeFault | None = None

    @staticmethod
    def from_process_fault(fault: ProcessFault) -> BridgeResult:
        return BridgeResult(status="timeout" if fault.returncode == 124 else "failed", fault=BridgeFault(message=fault.message))

    @property
    def exit_code(self) -> int:
        return _BRIDGE_EXIT[self.status]

    @property
    def diagnostics(self) -> BridgeRuntimeDiagnostics | None:
        def convert(phase: BridgePhase) -> BridgeRuntimeDiagnostics | None:
            match phase:
                case BridgePhase(phase="execute", data=dict() as data) if raw := data.get("diagnostics"):
                    return msgspec.convert(raw, type=BridgeRuntimeDiagnostics, strict=False)
                case _:
                    return None

        return next((diagnostics for phase in self.phases if (diagnostics := convert(phase)) is not None), None)

    @property
    def execute_stdout(self) -> tuple[str, bool]:
        return next(
            ((out.text, out.truncated) for phase in self.phases if phase.phase == "execute" for out in phase.outputs if out.source == "stdout"),
            ("", False),
        )

    @property
    def facts(self) -> tuple[dict[str, object], ...]:
        return _scan(_EVIDENCE_RE, self.execute_stdout[0])

    @property
    def captures(self) -> tuple[dict[str, object], ...]:
        return _scan(_CAPTURE_RE, self.execute_stdout[0])


class VerifySummaryCounts(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    ok: int
    failed: int
    total: int
    exception_reports: int = 0


class VerifyScenario(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    name: str
    status: BridgeStatus
    report_path: str = ""
    facts: tuple[dict[str, object], ...] = ()
    captures: tuple[dict[str, object], ...] = ()
    fault: BridgeFault | None = None
    exception_reports: tuple[BridgeFault, ...] = ()
    stdout_truncated: bool = False

    @staticmethod
    def of(result: BridgeResult) -> VerifyScenario:
        diagnostics = result.diagnostics
        return VerifyScenario(
            name=Path(result.report_path).stem or result.command or "scenario",
            status=result.status,
            report_path=result.report_path,
            facts=result.facts,
            captures=result.captures,
            fault=result.fault,
            exception_reports=diagnostics.exception_reports if diagnostics is not None else (),
            stdout_truncated=result.execute_stdout[1],
        )


class VerifyReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    summary: VerifySummaryCounts
    report_dir: str = ""
    expires_in_seconds: float = 0.0
    first_failure: VerifyScenario | None = None
    scenarios: tuple[VerifyScenario, ...] = ()

    @property
    def failed(self) -> int:
        return self.summary.failed


# --- [CONSTANTS] -----------------------------------------------------------------------

_BRIDGE_EXIT: Final[dict[BridgeStatus, int]] = {"busy": 5, "failed": 1, "ok": 0, "skipped": 0, "timeout": 5, "unsupported": 3}

# Canonical scenario evidence markers (BridgeMarker.Prefix) emitted on scenario stdout by Scenario.Run / Capture.
_EVIDENCE_RE: Final[re.Pattern[str]] = re.compile(r"rasm\.rhino-bridge\.evidence=facts=(\{.*\})")
_CAPTURE_RE: Final[re.Pattern[str]] = re.compile(r"rasm\.rhino-bridge\.capture=(\{.*\})")


# --- [OPERATIONS] ------------------------------------------------------------------------


def _decode_block(raw: str) -> dict[str, object] | None:
    value = decode_json(raw, object).default_with(lambda _: None)
    return {str(key): item for key, item in value.items()} if isinstance(value, dict) else None


def _scan(pattern: re.Pattern[str], text: str) -> tuple[dict[str, object], ...]:
    return tuple(
        block for line in text.splitlines() if (match := pattern.search(line)) is not None if (block := _decode_block(match.group(1))) is not None
    )


def _verify_discover(workspace: Workspace, root: Path, pattern: str) -> tuple[Path, ...]:
    def direct(candidate: Path) -> tuple[Path, ...]:
        match (candidate.is_file(), candidate.is_dir(), candidate.suffix):
            case (True, _, ".csx") if candidate.name.endswith(".verify.csx"):
                return (candidate.resolve(),)
            case (_, True, _):
                return workspace.paths(fd_args("csx", r"\.verify\.csx$", candidate, exclude=False), cwd=candidate, suffix=".csx")
            case _:
                return ()

    match next((rows for candidate in (Path(pattern), root / pattern) for rows in (direct(candidate),) if rows), ()):
        case ():
            # fd positional is regex, not shell glob — expand path-shaped patterns via pathlib from the worktree root.
            normalized = pattern if any(ch in pattern for ch in "/*?[") else f"**/{pattern}"
            return tuple(sorted(p.resolve() for p in root.glob(normalized) if p.is_file() and p.name.endswith(".verify.csx")))
        case rows:
            return rows


def _verify_invoke(settings: QualitySettings, scope: ArtifactScope, report_dir: Path, project: Path, scenario: Path) -> BridgeResult:
    result_path = report_dir / f"{scenario.stem.removesuffix('.verify')}.json"
    return (
        client_run(
            settings, scope, "check", str(project), str(scenario), "--result", str(result_path), check=False, timeout=settings.scenario_timeout_s
        )
        .map(
            lambda run: try_decode_bridge(result_path.read_bytes() if result_path.is_file() else (run.stdout or run.stderr)).default_with(
                BridgeResult.from_process_fault
            )
        )
        .default_with(BridgeResult.from_process_fault)
    )


def _verify_resolve(settings: QualitySettings, workspace: Workspace, index: ProjectIndex, scenario: Path) -> Result[Path, ProcessFault]:
    owner = workspace.owner(index, scenario)
    parts = scenario.resolve().relative_to(settings.root).parts
    match parts:
        case ("tests", "csharp", "libs", project, *rest) if "scenarios" in rest:
            candidate = settings.csharp_lib_project(project)
            return Ok(candidate) if candidate.is_file() else owner
        case _:
            return owner


def _verify_summary(report_dir: Path, results: tuple[BridgeResult, ...], ttl_seconds: float) -> Result[VerifyReport, ProcessFault]:
    scenarios = tuple(VerifyScenario.of(result) for result in results)
    ok = sum(scenario.status == "ok" for scenario in scenarios)
    reports = sum(len(scenario.exception_reports) for scenario in scenarios)
    payload = VerifyReport(
        summary=VerifySummaryCounts(ok=ok, failed=len(scenarios) - ok, total=len(scenarios), exception_reports=reports),
        report_dir=str(report_dir),
        expires_in_seconds=ttl_seconds,
        first_failure=next((scenario for scenario in scenarios if scenario.status != "ok"), None),
        scenarios=scenarios,
    )
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-write
    try:
        (report_dir / "summary.json").write_bytes(msgspec.json.encode(payload))
    except OSError as exc:
        return Error(ProcessFault.fail("verify", "summary", detail=str(exc)))
    return Ok(payload)


def _verify_expire(root: Path, ttl_seconds: float) -> Result[None, ProcessFault]:
    match root.exists():
        case False:
            return Ok(None)
        case True:
            cutoff = time.time() - ttl_seconds
            # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-expiry
            try:
                tuple(
                    shutil.rmtree(path)
                    for path in root.iterdir()
                    if path.is_dir() and path.resolve().is_relative_to(root.resolve()) and path.stat().st_mtime <= cutoff
                )
                return Ok(None)
            except OSError as exc:
                return Error(ProcessFault.fail("verify", "expire", detail=str(exc)))


@beartype
def build_client(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    # scoped=False keeps canonical bin/ so client_run --no-build and bridge_client_ready observe the same build output.
    return dotnet_build(settings, scope, restore=settings.bridge_client, targets=(settings.bridge_client,), serial=True, scoped=False)


@beartype
def build_scenario_kit(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    # scoped=False keeps canonical bin/ so in-Rhino ScenarioKitArtifacts resolves staged TestKit and Protocol assemblies.
    return dotnet_build(settings, scope, restore=settings.scenario_kit_project, targets=(settings.scenario_kit_project,), scoped=False)


@beartype
def client_run(
    settings: QualitySettings, scope: ArtifactScope, *args: str, check: bool = True, timeout: float | None = None
) -> Result[Completed, ProcessFault]:
    match settings.bridge_client_ready:
        case False:
            bin_dir = settings.bridge_client.parent / "bin" / settings.configuration
            return Error(
                ProcessFault.fail("bridge", "client", detail=f"Bridge client is not built under {bin_dir}; run `bridge build-bridge` first.")
            )
        case True:
            return dotnet(
                "run",
                "--no-build",
                "--project",
                str(settings.bridge_client),
                "--configuration",
                settings.configuration,
                "--",
                *args,
                scope=scope,
                scoped=False,
                check=check,
                timeout=timeout,
            )


@beartype
def client_quit(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    # Graceful quit for verify prelude and yak redeploy; fail the rail with a decoded fault when the bridge refuses.
    return client_run(settings, scope, "quit", check=False).bind(
        lambda completed: (
            try_decode_bridge(completed.stdout)
            .filter_with(
                lambda decoded: decoded.status == "ok",
                lambda decoded: ProcessFault.fail(
                    "quit", returncode=1, detail=completed.stderr or (decoded.fault.message.encode() if decoded.fault else b"quit refused")
                ),
            )
            .map(lambda _: None)
        )
    )


@beartype
def client_refresh(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    # Cold-launch a fresh Rhino on the freshly-installed plugin and confirm it answers a doctor over the bridge.
    return client_run(settings, scope, "launch").bind(lambda _: client_run(settings, scope, "doctor").map(lambda _: None))


@beartype
def run_verify(settings: QualitySettings, scope: ArtifactScope, pattern: str) -> Result[VerifyReport, ProcessFault]:
    root, report_root, report_dir, workspace = (settings.root, settings.bridge_verify_root, settings.bridge_verify_dir, Workspace(settings.root))
    scenarios = _verify_discover(workspace, root, pattern)
    match scenarios:
        case ():
            return Error(ProcessFault.fail("verify", pattern, detail=b"No *.verify.csx scenarios matched"))
        case _:
            index = workspace.index()

            def ensure_report_dir() -> Result[None, ProcessFault]:
                # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-mkdir
                try:
                    report_dir.mkdir(parents=True, exist_ok=True)
                    return Ok(None)
                except OSError as exc:
                    return Error(ProcessFault.fail("verify", "report-dir", detail=str(exc)))

            def scenario(acc: tuple[BridgeResult, ...], item: Path) -> Result[tuple[BridgeResult, ...], ProcessFault]:
                return Ok(
                    _verify_resolve(settings, workspace, index, item)
                    .map(lambda project: (*acc, _verify_invoke(settings, scope, report_dir, project, item)))
                    .default_with(lambda fault: (*acc, BridgeResult.from_process_fault(fault)))
                )

            seed: tuple[BridgeResult, ...] = ()

            return (
                _verify_expire(report_root, settings.verify_retention_seconds)
                .bind(lambda _: ensure_report_dir())
                .bind(lambda _: build_client(settings, scope))
                .bind(lambda _: build_scenario_kit(settings, scope))
                .bind(lambda _: client_run(settings, scope, "launch", check=False))
                .bind(
                    lambda _: fold(scenarios, seed, scenario).bind(lambda rows: _verify_summary(report_dir, rows, settings.verify_retention_seconds))
                )
            )


def try_decode_bridge(payload: bytes | str) -> Result[BridgeResult, ProcessFault]:
    return decode_json(payload, BridgeResult)
