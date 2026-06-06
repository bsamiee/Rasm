"""Run live Rhino bridge lifecycle and verification commands."""

from dataclasses import dataclass
from pathlib import Path
import shutil
import time
from typing import Final, override

from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import leased, run_check
from tools.assay.core.model import (
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
_SCENARIO_SUFFIX: Final[str] = ".verify.csx"
_SCENARIO_TIMEOUT_S: Final[float] = 180.0
_VERIFY_TTL_S: Final[float] = 300.0
_VERIFY_DIR: Final[str] = "verify"
_PATH_GLYPHS: Final[str] = "/*?["
_EXECUTE_PHASE: Final[str] = "execute"
_STDOUT_SOURCE: Final[str] = "stdout"
_STDERR_SOURCE: Final[str] = "stderr"


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BridgeParams(BaseParams):
    """Parameters shared by bridge verbs."""

    pattern: str = ""

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


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


class _BridgeDiagnostics(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command_window: tuple[str, ...] = ()
    exception_reports: tuple[_BridgeFault, ...] = ()


class _BridgeOutput(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    source: str = ""
    text: str = ""
    truncated: bool = False


class _BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str
    status: RailStatus = RailStatus.FAILED
    data: dict[str, object] | None = None
    outputs: tuple[_BridgeOutput, ...] = ()


class _BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    command: str = ""
    status: RailStatus = RailStatus.FAILED
    report_path: str = ""
    phases: tuple[_BridgePhase, ...] = ()


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


def _first_fault(result: _BridgeResult) -> tuple[str, str]:
    # Phases stream in wire order, so the first defect is the retriage entry.
    return next(((phase.phase, _phase_diagnostic(phase)) for phase in result.phases if phase.status.severity > RailStatus.OK.severity), ("", ""))


def _phase_diagnostic(phase: _BridgePhase) -> str:
    streamed = next((out.text for src in (_STDERR_SOURCE, _STDOUT_SOURCE) for out in phase.outputs if out.source == src and out.text), "")
    match (streamed, _diagnostics(phase.data).exception_reports):
        case ("", (head, *_)):
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


def _run_scenario(settings: AssaySettings, report_dir: Path, scenario: Path) -> _Scenario:
    # Spawn/timeout Faults become FAILED scenario rows so every discovered scenario stays represented.
    stem = scenario.name.removesuffix(_SCENARIO_SUFFIX) or scenario.stem
    result_path = report_dir / f"{stem}.json"
    run = _client_run(settings, "check", str(scenario), "--result", str(result_path), timeout=_SCENARIO_TIMEOUT_S)
    match run:
        case Result(tag="ok", ok=done):
            res = _decode_result(done, result_path)
            phase, output = _first_fault(res)
            return _Scenario(status=res.status, stem=stem, exceptions=_exceptions(res), fault_phase=phase, fault_output=output, completed=done)
        case Result(error=fault):
            return _Scenario(
                status=RailStatus.FAILED,
                stem=stem,
                fault_phase="launch",
                fault_output=(fault.message or " ".join(fault.argv))[:256],
                completed=receipt(fault.argv, 1, status=RailStatus.FAILED),
            )


def _discover(settings: AssaySettings, pattern: str) -> tuple[Path, ...]:
    # Discover by direct file, directory scan, then worktree glob; empty is UNSUPPORTED, not Faulted.
    root = Path(str(settings.root))  # scenario discovery globs the local worktree; UPath -> Path at the boundary
    direct = _direct(root, pattern)
    return direct or _glob(root, pattern)


def _direct(root: Path, pattern: str) -> tuple[Path, ...]:
    candidate = Path(pattern) if Path(pattern).is_absolute() else root / pattern
    match (candidate.is_file(), candidate.is_dir(), candidate.name.endswith(_SCENARIO_SUFFIX)):
        case (True, _, True):
            return (candidate.resolve(),)
        case (_, True, _):
            return tuple(sorted(p.resolve() for p in candidate.rglob(f"*{_SCENARIO_SUFFIX}") if p.is_file()))
        case _:
            return ()


def _glob(root: Path, pattern: str) -> tuple[Path, ...]:
    # Bare tokens expand to recursive worktree globs.
    normalized = pattern if any(glyph in pattern for glyph in _PATH_GLYPHS) else f"**/{pattern}"
    return tuple(sorted(p.resolve() for p in root.glob(normalized) if p.is_file() and p.name.endswith(_SCENARIO_SUFFIX)))


def _expire_stale(report_dir: Path, ttl_s: float) -> None:
    # Expire before launch so housekeeping cannot race freshly written scenario JSON.
    parent = report_dir.parent
    match parent.is_dir():
        case False:
            return
        case True:
            cutoff = time.time() - ttl_s
            _ = tuple(_rmtree(child) for child in parent.iterdir() if _is_stale(child, parent, cutoff))


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
    # Build, report-dir setup, and launch must complete before scenario discovery and execution.
    report_dir = Path(scope.path) / _VERIFY_DIR
    _expire_stale(report_dir, _VERIFY_TTL_S)
    prelude = _ensure_dir(report_dir).bind(lambda _: _build_closure(settings)).bind(lambda _: _affirm(_client_run(settings, "launch")))
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
            return Ok(fold(Claim.BRIDGE, "verify", tuple(row.completed for row in rows), detail=summary))


def _build_closure(settings: AssaySettings) -> Result[None, Fault]:
    # One stable build tree keeps the live host and --no-build client on the same output.
    scope = ArtifactScope.build(settings, "bridge")
    projects = (_PROTOCOL_PROJECT, _PLUGIN_PROJECT, _CLIENT_PROJECT)
    check = Check(tool=_BUILD_TOOL, cwd=Path(str(settings.root)))
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=tuple(str(settings.root / p) for p in projects))
    return _affirm(run_check(check, settings=settings, scope=scope, routed=routed, deadline=None))


def _affirm(outcome: Result[Completed, Fault]) -> Result[None, Fault]:
    # Non-zero build/launch exits mean verify could not run, so promote them to Fault.
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(None)
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
    return _build_closure(settings).map(lambda _: fold(Claim.BRIDGE, "build", (receipt(("bridge", "build"), 0),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "build", "check", "clean", "doctor", "launch", "quit", "verify"]
