"""Rhino bridge client, scenario verify, and RhinoWIP API metadata rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

from pathlib import Path
from typing import Final, Literal

from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.process import (
    Completed,
    decode_json,
    dotnet,
    dotnet_rail,
    fd_args,
    fold,
    ProcessFault,
    ProjectIndex,
    run_fold,
    Workspace,
)
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


class BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str
    status: BridgeStatus
    duration_ms: float = 0.0
    data: dict[str, object] | None = None
    fault: BridgeFault | None = None


class BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    schema: str = ""
    command: str = ""
    status: BridgeStatus = "failed"
    report_path: str = ""
    phases: tuple[BridgePhase, ...] = ()
    fault: BridgeFault | None = None

    @staticmethod
    def from_process_fault(fault: ProcessFault) -> BridgeResult:
        return BridgeResult(
            status="timeout" if fault.returncode == 124 else "failed", fault=BridgeFault(message=fault.message)
        )

    @property
    def exit_code(self) -> int:
        return _BRIDGE_EXIT[self.status]

    @property
    def diagnostics(self) -> BridgeRuntimeDiagnostics | None:
        for phase in self.phases:
            if phase.phase == "execute" and (data := phase.data) and (raw := data.get("diagnostics")):
                return msgspec.convert(raw, type=BridgeRuntimeDiagnostics, strict=False)
        return None


class VerifySummaryCounts(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    ok: int
    failed: int
    total: int
    exception_reports: int = 0


class VerifyReport(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    summary: VerifySummaryCounts
    scenarios: tuple[BridgeResult, ...] = ()

    @property
    def failed(self) -> int:
        return self.summary.failed


# --- [CONSTANTS] -----------------------------------------------------------------------

_BRIDGE_EXIT: Final[dict[BridgeStatus, int]] = {
    "busy": 5,
    "failed": 1,
    "ok": 0,
    "skipped": 0,
    "timeout": 5,
    "unsupported": 3,
}


# --- [OPERATIONS] ------------------------------------------------------------------------


def _verify_discover(workspace: Workspace, root: Path, pattern: str) -> tuple[Path, ...]:
    def direct(candidate: Path) -> tuple[Path, ...]:
        match (candidate.is_file(), candidate.is_dir(), candidate.suffix):
            case (True, _, ".csx") if candidate.name.endswith(".verify.csx"):
                return (candidate.resolve(),)
            case (_, True, _):
                return workspace.paths(
                    fd_args("csx", r"\.verify\.csx$", candidate, exclude=False), cwd=candidate, suffix=".csx"
                )
            case _:
                return ()

    match next((rows for candidate in (Path(pattern), root / pattern) for rows in (direct(candidate),) if rows), ()):
        case ():
            glob = pattern if any(ch in pattern for ch in "/*?[") else f"**/{pattern}"
            return workspace.paths(fd_args("csx", glob, root, exclude=False), suffix=".csx")
        case rows:
            return rows


def _verify_invoke(
    settings: QualitySettings, scope: ArtifactScope, report_dir: Path, project: Path, scenario: Path
) -> BridgeResult:
    result_path = report_dir / f"{scenario.stem.removesuffix('.verify')}.json"
    return (
        client_run(
            settings,
            scope,
            "check",
            str(project),
            str(scenario),
            "--result",
            str(result_path),
            check=False,
            timeout=settings.scenario_timeout_s,
        )
        .map(
            lambda run: try_decode_bridge(
                result_path.read_bytes() if result_path.is_file() else (run.stdout or run.stderr)
            ).default_with(BridgeResult.from_process_fault)
        )
        .default_with(BridgeResult.from_process_fault)
    )


def _verify_resolve(
    settings: QualitySettings, workspace: Workspace, index: ProjectIndex, scenario: Path
) -> Result[Path, ProcessFault]:
    owner = workspace.owner(index, scenario)
    parts = scenario.resolve().relative_to(settings.root).parts
    match parts:
        case ("tests", "csharp", "libs", project, *rest) if "scenarios" in rest:
            candidate = settings.csharp_lib_project(project)
            return Ok(candidate) if candidate.is_file() else owner
        case _:
            return owner


def _verify_summary(report_dir: Path, results: tuple[BridgeResult, ...]) -> Result[VerifyReport, ProcessFault]:
    ok = sum(result.status == "ok" for result in results)
    reports = sum(len(d.exception_reports) for r in results if (d := r.diagnostics) is not None)
    payload = VerifyReport(
        summary=VerifySummaryCounts(ok=ok, failed=len(results) - ok, total=len(results), exception_reports=reports),
        scenarios=results,
    )
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-write ticket=QUALITY-R6
    # expires=2026-12-31 rationale=evidence-boundary
    try:
        (report_dir / "summary.json").write_bytes(msgspec.json.encode(payload))
    except OSError as exc:
        return Error(ProcessFault.fail("verify", "summary", detail=str(exc)))
    return Ok(payload)


@beartype
def build_client(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    return dotnet_rail(settings, scope, restore=settings.bridge_client, targets=(settings.bridge_client,))


@beartype
def build_scenario_kit(settings: QualitySettings, scope: ArtifactScope) -> Result[None, ProcessFault]:
    # Default bin/ (no --artifacts-path) so the in-Rhino client's ScenarioKitArtifacts probe resolves the
    # Rasm.TestKit + transitive Rasm/Protocol assemblies it injects as scenario #r references.
    kit = str(settings.scenario_kit_project)
    return run_fold(
        scope,
        (
            ("dotnet", "restore", kit, "--locked-mode"),
            ("dotnet", "build", kit, "--configuration", settings.configuration, "--no-restore"),
        ),
    )


@beartype
def client_run(
    settings: QualitySettings, scope: ArtifactScope, *args: str, check: bool = True, timeout: float | None = None
) -> Result[Completed, ProcessFault]:
    match settings.bridge_client_ready:
        case False:
            bin_dir = settings.bridge_client.parent / "bin" / settings.configuration
            return Error(
                ProcessFault.fail(
                    "bridge",
                    "client",
                    detail=f"Bridge client is not built under {bin_dir}; run `bridge build-bridge` first.",
                )
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
                check=check,
                timeout=timeout,
            )


@beartype
def run_verify(settings: QualitySettings, scope: ArtifactScope, pattern: str) -> Result[VerifyReport, ProcessFault]:
    root, report_dir, workspace = settings.root, settings.bridge_verify_dir, Workspace(settings.root)
    scenarios = _verify_discover(workspace, root, pattern)
    match scenarios:
        case ():
            return Error(ProcessFault.fail("verify", pattern, detail=b"No *.verify.csx scenarios matched"))
        case _:
            index = workspace.index()

            def ensure_report_dir() -> Result[None, ProcessFault]:
                # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=artifact-mkdir ticket=QUALITY-R6
                # expires=2026-12-31 rationale=evidence-dir-boundary
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
                ensure_report_dir()
                .bind(lambda _: build_client(settings, scope))
                .bind(lambda _: build_scenario_kit(settings, scope))
                .bind(lambda _: client_run(settings, scope, "launch", check=False))
                .bind(lambda _: fold(scenarios, seed, scenario).bind(lambda rows: _verify_summary(report_dir, rows)))
            )


def try_decode_bridge(payload: bytes | str) -> Result[BridgeResult, ProcessFault]:
    return decode_json(payload, BridgeResult)
