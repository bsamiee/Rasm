"""Run live Rhino bridge lifecycle and verification commands."""

from dataclasses import dataclass
from pathlib import Path
import re
import shutil
import time
from typing import Final, override

from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import leased, run_check
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    fold,
    Input,
    Language,
    Mode,
    receipt,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the -> Result[Report, Fault] forward-ref under PEP 649
    Runner,
    Tool,
    VerifySummary,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


# --- [CONSTANTS] ------------------------------------------------------------------------

_CLIENT_PROJECT: Final[str] = "tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
_PLUGIN_PROJECT: Final[str] = "tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
_PROTOCOL_PROJECT: Final[str] = "tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
_TESTKIT_PROJECT: Final[str] = "tests/csharp/_testkit/Rasm.TestKit.csproj"
_LIB_ROOT: Final[tuple[str, ...]] = ("tests", "csharp", "libs")
_SCENARIO_SUFFIX: Final[str] = ".verify.csx"
_SCENARIO_TIMEOUT_S: Final[float] = 180.0
_VERIFY_TTL_S: Final[float] = 300.0
_VERIFY_DIR: Final[str] = "verify"
_PATH_GLYPHS: Final[str] = "/*?["
_EXECUTE_PHASE: Final[str] = "execute"
_STDOUT_SOURCE: Final[str] = "stdout"
_STDERR_SOURCE: Final[str] = "stderr"
# Scenario evidence markers (BridgeMarker.Prefix) ride execute stdout via Scenario.Run / Capture.
_EVIDENCE_RE: Final[re.Pattern[str]] = re.compile(r"rasm\.rhino-bridge\.evidence=facts=(\{.*\})")
_CAPTURE_RE: Final[re.Pattern[str]] = re.compile(r"rasm\.rhino-bridge\.capture=(\{.*\})")


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BridgeParams(BaseParams):
    """Parameters shared by bridge verbs."""

    @override
    def _arity(self, verb: str) -> int:
        return 1 if verb == "verify" else 0

    @property
    def pattern(self) -> str:
        """Scenario discovery pattern from the positional tail; empty discovers every scenario under the workspace root."""
        return self.paths[0] if self.paths else ""


class _Scenario(msgspec.Struct, frozen=True, gc=False):
    # The status fold erases scenario order, so preserve first-fault facts beside the receipt.
    status: RailStatus
    stem: str
    exceptions: int = 0
    fault_phase: str = ""
    fault_output: str = ""
    completed: Completed = msgspec.field(default_factory=lambda: receipt((), 0))


class _BridgeFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    category: str = ""
    message: str = ""
    type: str = ""
    stack_trace: str = ""
    causes: tuple["_BridgeFault", ...] = ()  # recursive self-reference — quoted so msgspec resolves it post-creation


class _BridgeDiagnostics(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command_window: tuple[str, ...] = ()
    exception_reports: tuple[_BridgeFault, ...] = ()


class _BridgeOutput(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    source: str = ""
    text: str = ""
    truncated: bool = False
    length: int = 0
    limit: int = 0


class _BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str
    status: RailStatus = RailStatus.FAILED
    duration_ms: int = 0
    data: dict[str, object] | None = None
    outputs: tuple[_BridgeOutput, ...] = ()
    fault: _BridgeFault | None = None


class _BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command: str = ""
    status: RailStatus = RailStatus.FAILED
    report_path: str = ""
    phases: tuple[_BridgePhase, ...] = ()
    fault: _BridgeFault | None = None


# --- [TABLES] ---------------------------------------------------------------------------

_RESULT_DECODER: Final[msgspec.json.Decoder[_BridgeResult]] = msgspec.json.Decoder(_BridgeResult, strict=False)

# The client command is completed with project, configuration, and verb tail through structs.replace.
_CLIENT_TOOL: Final[Tool] = Tool(
    name="rasm-bridge",
    runner=Runner.DOTNET,
    command=("run", "--no-build", "--project"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.BRIDGE,
    mode=Mode.VERIFY,
    timeout=_SCENARIO_TIMEOUT_S,
)
# Lease-free compile check for the protocol, plugin, and client closure.
_BUILD_TOOL: Final[Tool] = Tool(
    name="rasm-bridge-build",
    runner=Runner.DOTNET,
    command=("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"),
    input=Input.PROJECT,
    language=Language.CSHARP,
    claim=Claim.BRIDGE,
    mode=Mode.BUILD,
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _routed() -> Routed:
    return Routed(language=Language.CSHARP, scope=Scope.CHANGED)


def _client_check(settings: AssaySettings, *args: str) -> Check:
    # Input.NONE keeps the full client invocation in the command body and out of Check.paths.
    tail = (str(settings.root / _CLIENT_PROJECT), "--configuration", settings.configuration.value, "--", *args)
    tool = msgspec.structs.replace(_CLIENT_TOOL, command=(*_CLIENT_TOOL.command, *tail))
    return Check(tool=tool, cwd=Path(str(settings.root)))


def _client_run(settings: AssaySettings, *args: str, timeout: float | None = None) -> Result[Completed, Fault]:
    check = _client_check(settings, *args)
    deadline = time.monotonic() + timeout if timeout is not None else None
    return run_check(check, settings=settings, scope=None, routed=_routed(), deadline=deadline)


def _exceptions(result: _BridgeResult) -> int:
    return sum(len(_diagnostics(phase.data).exception_reports) for phase in result.phases if phase.phase == _EXECUTE_PHASE and phase.data is not None)


def _diagnostics(data: dict[str, object] | None) -> _BridgeDiagnostics:
    match data:
        case {"diagnostics": raw}:
            return _coerce_diagnostics(raw)
        case _:
            return _BridgeDiagnostics()


def _coerce_diagnostics(raw: object) -> _BridgeDiagnostics:
    try:
        return msgspec.convert(raw, type=_BridgeDiagnostics, strict=False)
    except msgspec.ValidationError:
        return _BridgeDiagnostics()


def _with_facts(done: Completed, result: _BridgeResult) -> Completed:
    # Markers ride the execute phase's decoded stdout BridgeOutput, not raw client stdout;
    # surface them as receipt notes (full VerifyScenario decoding lands in Wave E).
    text = next(
        (out.text for phase in result.phases if phase.phase == _EXECUTE_PHASE for out in phase.outputs if out.source == _STDOUT_SOURCE),
        "",
    )
    facts = tuple(
        f"{label}:{match.group(1)}"
        for label, pattern in (("facts", _EVIDENCE_RE), ("capture", _CAPTURE_RE))
        for line in text.splitlines()
        if (match := pattern.search(line)) is not None
    )
    return done if not facts else msgspec.structs.replace(done, notes=(*done.notes, *facts))


def _first_fault(result: _BridgeResult) -> tuple[str, str]:
    # A reply-level fault outranks phase rows; otherwise phases stream in wire order, so the first defect is the retriage entry.
    match result.fault:
        case _BridgeFault() as fault:
            return ("reply", _fault_message(fault))
        case _:
            return next(
                ((phase.phase, _phase_diagnostic(phase)) for phase in result.phases if phase.status.severity > RailStatus.OK.severity),
                ("", ""),
            )


def _fault_message(fault: _BridgeFault) -> str:
    # Causes are a flattened inner-exception chain; the first carries the proximate failure when present.
    return next((tail.message for tail in fault.causes), fault.message)[:256]


def _phase_diagnostic(phase: _BridgePhase) -> str:
    # Execute markers stream to stdout, so the execute phase prefers stdout; every other phase keeps stderr-first.
    order = (_STDOUT_SOURCE, _STDERR_SOURCE) if phase.phase == _EXECUTE_PHASE else (_STDERR_SOURCE, _STDOUT_SOURCE)
    streamed = next((out.text for src in order for out in phase.outputs if out.source == src and out.text), "")
    match (phase.fault, streamed, _diagnostics(phase.data).exception_reports):
        case (_BridgeFault() as fault, "", _):
            return _fault_message(fault)
        case (_, "", (head, *_)):
            return head.message[:256]
        case _:
            return streamed[:256]


def _decode_result(run: Completed, result_path: Path) -> _BridgeResult:
    # Prefer the result file because client stderr carries structlog diagnostics, not evidence.
    raw = _read_result(result_path).default_value(run.stdout or run.stderr)
    try:
        return _RESULT_DECODER.decode(raw or b"{}")
    except msgspec.DecodeError:
        return _BridgeResult(status=RailStatus.FAILED)


def _read_result(result_path: Path) -> Result[bytes, Fault]:
    try:
        return Ok(result_path.read_bytes()) if result_path.is_file() else Error(Fault(("read", str(result_path)), RailStatus.SKIP))
    except OSError as exc:
        return Error(Fault(("read", str(result_path)), RailStatus.FAULTED, str(exc)[:1024]))


def _resolve_project(settings: AssaySettings, scenario: Path) -> Path:
    # ClientVerb routes on extension: a .csproj positional[0] -> Project closure that supplies the scenario's compile surface.
    # tests/csharp/libs/<project>/.../scenarios/X.verify.csx owns <project>.Tests.csproj; bare scenarios fall back to the TestKit closure.
    root = Path(str(settings.root))
    parts = scenario.resolve().relative_to(root.resolve()).parts
    match parts:
        case ("tests", "csharp", "libs", project, *_):
            candidate = root / Path(*_LIB_ROOT) / project / f"{project}.Tests.csproj"
            return candidate if candidate.is_file() else root / _TESTKIT_PROJECT
        case _:
            return root / _TESTKIT_PROJECT


def _client_ready(settings: AssaySettings) -> Result[None, str]:
    # --no-build client invocations need a prior build closure; a missing client DLL is an operational fault, not a scenario defect.
    bin_dir = Path(str(settings.root)) / Path(_CLIENT_PROJECT).parent / "bin" / settings.configuration.value
    if any(bin_dir.glob("*/Rasm.RhinoBridge.Client.dll")):
        return Ok(None)
    return Error(f"bridge client is not built under {bin_dir}; run `bridge build` first")


def _run_scenario(settings: AssaySettings, report_dir: Path, scenario: Path) -> _Scenario:
    # Spawn/timeout Faults become FAILED scenario rows so every discovered scenario stays represented.
    stem = scenario.name.removesuffix(_SCENARIO_SUFFIX) or scenario.stem
    result_path = report_dir / f"{stem}.json"
    project = _resolve_project(settings, scenario)
    match _client_ready(settings):
        case Result(error=reason):
            return _Scenario(
                status=RailStatus.FAILED, stem=stem, fault_phase="client", fault_output=reason[:256],
                completed=receipt((stem, "client"), 1, status=RailStatus.FAILED, notes=(reason,)),
            )
        case _:
            return _run_decoded(settings, project, scenario, result_path, stem)


def _run_decoded(settings: AssaySettings, project: Path, scenario: Path, result_path: Path, stem: str) -> _Scenario:
    run = _client_run(settings, "check", str(project), str(scenario), "--result", str(result_path), timeout=_SCENARIO_TIMEOUT_S)
    match run:
        case Result(tag="ok", ok=done):
            res = _decode_result(done, result_path)
            phase, output = _first_fault(res)
            return _Scenario(
                status=res.status, stem=stem, exceptions=_exceptions(res), fault_phase=phase, fault_output=output,
                completed=_with_facts(done, res),
            )
        case Result(error=fault):
            return _Scenario(
                status=RailStatus.FAILED, stem=stem, fault_phase="launch",
                fault_output=(fault.message or " ".join(fault.argv))[:256],
                completed=receipt(fault.argv, 1, status=RailStatus.FAILED),
            )


def _discover(settings: AssaySettings, pattern: str) -> tuple[Path, ...]:
    # Discover by direct file, directory scan, then worktree glob; empty is UNSUPPORTED, not Faulted.
    root = Path(str(settings.root))  # scenario discovery globs the local worktree; UPath -> Path at the boundary
    direct = _direct(root, pattern)
    return direct or _glob(root, pattern)


def _direct(root: Path, pattern: str) -> tuple[Path, ...]:
    base = root.resolve()
    candidate = (Path(pattern) if Path(pattern).is_absolute() else root / pattern).resolve()
    if not candidate.is_relative_to(base):
        return ()
    match (candidate.is_file(), candidate.is_dir(), candidate.name.endswith(_SCENARIO_SUFFIX)):
        case (True, _, True):
            return (candidate.resolve(),)
        case (_, True, _):
            return tuple(sorted(p.resolve() for p in candidate.rglob(f"*{_SCENARIO_SUFFIX}") if p.is_file()))
        case _:
            return ()


def _glob(root: Path, pattern: str) -> tuple[Path, ...]:
    # Bare tokens expand to recursive worktree globs.
    base = root.resolve()
    normalized = pattern if any(glyph in pattern for glyph in _PATH_GLYPHS) else f"**/{pattern}"
    return tuple(
        sorted(
            resolved
            for p in root.glob(normalized)
            for resolved in (p.resolve(),)
            if resolved.is_relative_to(base) and p.is_file() and p.name.endswith(_SCENARIO_SUFFIX)
        )
    )


def _expire_stale(report_dir: Path, run_id: str, ttl_s: float) -> None:
    # The per-run scope (report_dir.parent) is unique, so housekeeping must sweep the shared claim root by mtime.
    # Exclude the live run so expiry before launch cannot race this run's freshly written scenario JSON.
    claim_root = report_dir.parent.parent
    match claim_root.is_dir():
        case False:
            return
        case True:
            cutoff = time.time() - ttl_s
            _ = tuple(_rmtree(child) for child in claim_root.iterdir() if child.name != run_id and _is_stale(child, claim_root, cutoff))


def _is_stale(child: Path, parent: Path, cutoff: float) -> bool:
    try:
        # is_relative_to bounds the sweep against symlink escapes.
        return child.is_dir() and child.resolve().is_relative_to(parent.resolve()) and child.stat().st_mtime <= cutoff
    except OSError:
        return False


def _rmtree(path: Path) -> Path:
    shutil.rmtree(path, ignore_errors=True)
    return path


def _ensure_dir(report_dir: Path) -> Result[None, Fault]:
    try:
        report_dir.mkdir(parents=True, exist_ok=True)
        return Ok(None)
    except OSError as exc:
        return Error(Fault(("verify", "report-dir"), RailStatus.FAULTED, str(exc)[:1024]))


# --- [COMPOSITION] ----------------------------------------------------------------------


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Verify bridge scenarios under the live Rhino lease.

    Returns:
        Verification report, or a lease/build/launch fault.
    """
    argv = ("bridge", "verify", params.pattern)
    return leased("bridge", lambda _held: _verify_locked(settings, scope, params, argv), settings=settings, run_id=settings.run_id, project="bridge")


def _verify_locked(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...]) -> Result[Report, Fault]:
    # Report-dir setup, the fault-gated build, then launch must each clear before scenario discovery; the prelude receipts are discarded.
    report_dir = Path(scope.path) / _VERIFY_DIR
    _expire_stale(report_dir, settings.run_id, _VERIFY_TTL_S)
    prelude = _ensure_dir(report_dir).bind(lambda _: _build_closure(settings)).bind(lambda _: _faulted(_client_run(settings, "launch")))
    match prelude:
        case Result(tag="ok"):
            return _fold_scenarios(settings, report_dir, _discover(settings, params.pattern), argv)
        case Result(error=fault):
            return Error(fault)


def _fold_scenarios(settings: AssaySettings, report_dir: Path, scenarios: tuple[Path, ...], argv: tuple[str, ...]) -> Result[Report, Fault]:
    # VerifySummary carries only bridge facts the status/count fold cannot derive.
    # Lift the first failing scenario before order-insensitive folding erases it.
    match scenarios:
        case ():
            return Ok(fold(Claim.BRIDGE, "verify", (receipt(argv, 3, status=RailStatus.UNSUPPORTED),)))
        case _:
            rows = tuple(_run_scenario(settings, report_dir, scenario) for scenario in scenarios)
            first = next((row for row in rows if row.status.exit_code != 0), None)
            summary = VerifySummary(
                exceptions=sum(row.exceptions for row in rows),
                report_dir=str(report_dir),
                first_failure=first.stem if first is not None else "",
                first_fault_phase=first.fault_phase if first is not None else "",
                first_fault_output=first.fault_output if first is not None else "",
            )
            report = fold(Claim.BRIDGE, "verify", tuple(row.completed for row in rows), detail=summary)
            return Ok(msgspec.structs.replace(report, artifacts=(*report.artifacts, *_scenario_artifacts(report_dir))))


def _build_closure(settings: AssaySettings) -> Result[Completed, Fault]:
    # One stable build tree keeps the live host and --no-build client on the same output.
    # Order protocol -> plugin -> client -> testkit so the isolated resolver finds Scenario.Run / FactBag at scenario time.
    def locked(_held: object) -> Result[Completed, Fault]:
        scope = ArtifactScope.build(settings, "bridge")
        projects = (_PROTOCOL_PROJECT, _PLUGIN_PROJECT, _CLIENT_PROJECT, _TESTKIT_PROJECT)
        check = Check(tool=_BUILD_TOOL, cwd=Path(str(settings.root)))
        routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=tuple(str(settings.root / p) for p in projects))
        return _faulted(run_check(check, settings=settings, scope=scope, routed=routed, deadline=None))

    return leased(f"build-bridge-{settings.configuration.value}", locked, settings=settings, run_id=settings.run_id, project="bridge")


def _scenario_artifacts(report_dir: Path) -> tuple[Artifact, ...]:
    return tuple(
        Artifact(
            id=path.stem, kind=ArtifactKind.RHINO, path=str(path), bytes=path.stat().st_size, lines=len(path.read_text(errors="replace").splitlines())
        )
        for path in sorted(report_dir.glob("*.json"))
        if path.is_file()
    )


def _faulted(outcome: Result[Completed, Fault]) -> Result[Completed, Fault]:
    # Non-zero build/launch exits mean the closure is unusable; promote them to Fault while preserving the receipt on success.
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(done)
        case Result(tag="ok", ok=done):
            # FAILED is success-channel defect evidence; build/launch failure is an operational Fault.
            status = RailStatus.FAULTED if done.status is RailStatus.FAILED else done.status
            return Error(Fault(done.argv, status, (done.stderr or done.stdout or b"")[:1024].decode(errors="replace")))
        case Result(error=fault):
            return Error(fault)


def _lifecycle(settings: AssaySettings, verb: str, *args: str) -> Result[Report, Fault]:
    # Lifecycle verbs differ only by client subcommand and operands under the same bridge.lock lease.
    return leased(
        "bridge",
        lambda _held: _client_run(settings, verb, *args).map(lambda done: fold(Claim.BRIDGE, verb, (done,))),
        settings=settings,
        run_id=settings.run_id,
        project="bridge",
    )


def doctor(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Run the bridge host health probe.

    Returns:
        Bridge lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "doctor")


def launch(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Launch the bridge host under the live Rhino lease.

    Returns:
        Bridge lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "launch")


def quit(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:  # noqa: A001  # registry verb name; the rail surface mirrors the CLI token, never the builtin
    """Terminate the bridge host under the live Rhino lease.

    Returns:
        Bridge lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "quit")


def check(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Probe bridge host liveness.

    Returns:
        Bridge lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "check")


def clean(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Clean bridge crash and autosave state.

    Returns:
        Bridge lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "clean")


def build(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Build the bridge plugin and client closure.

    Returns:
        Bridge build report or build fault.
    """
    _ = (scope, params)
    return _build_closure(settings).map(lambda done: fold(Claim.BRIDGE, "build", (done,)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "build", "check", "clean", "doctor", "launch", "quit", "verify"]
