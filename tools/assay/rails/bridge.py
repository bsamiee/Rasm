"""Run live Rhino bridge lifecycle and verification commands."""

from collections.abc import Callable  # noqa: TC003  # beartype resolves the public bridge_lease signature at runtime under PEP 649
from dataclasses import dataclass
from pathlib import Path
import re
import shutil
import time
from typing import Final, override

from expression import Error, Ok, Result
import msgspec

# beartype resolves ArtifactScope annotation in public function signatures at runtime under PEP 649
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001
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
    Report,  # noqa: TC001  # beartype resolves Result[Report, Fault] forward-ref at runtime under PEP 649
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
# BridgeMarker-prefixed lines emitted to execute-phase stdout by Scenario.Run / Capture, not raw client stdout.
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
        """Scenario discovery pattern from the positional tail.

        An empty string discovers every scenario under the workspace root.
        """
        return self.paths[0] if self.paths else ""


class _Scenario(msgspec.Struct, frozen=True, gc=False):
    # fold() erases scenario order; first-fault fields must survive as receipt-adjacent state.
    status: RailStatus
    stem: str
    exceptions: int = 0
    fault_phase: str = ""
    fault_output: str = ""
    facts: tuple[str, ...] = ()
    captures: tuple[str, ...] = ()
    completed: Completed = msgspec.field(default_factory=lambda: receipt((), 0))


class _BridgeFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    category: str = ""
    message: str = ""
    type: str = ""
    stack_trace: str = ""
    causes: tuple["_BridgeFault", ...] = ()


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

# Input.NONE keeps the verb tail in the command body rather than Check.paths.
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
# Build runs outside any lease; the lock in _build_closure prevents concurrent restores.
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
    tail = (str(settings.root / _CLIENT_PROJECT), "--configuration", settings.configuration.value, "--", *args)
    tool = msgspec.structs.replace(_CLIENT_TOOL, command=(*_CLIENT_TOOL.command, *tail))
    return Check(tool=tool, cwd=Path(str(settings.root)))


def client_run(settings: AssaySettings, *args: str, timeout: float | None = None) -> Result[Completed, Fault]:
    """Run the rhino-bridge client with the given verb arguments.

    ``timeout`` is a wall-clock budget in seconds, mapped to the run deadline.

    Returns:
        Client completion receipt, or an operational fault.
    """
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


def _markers(result: _BridgeResult) -> tuple[tuple[str, ...], tuple[str, ...]]:
    # Evidence markers appear in the decoded execute-phase stdout from the JSON result, not in raw client stdout.
    text = next((out.text for phase in result.phases if phase.phase == _EXECUTE_PHASE for out in phase.outputs if out.source == _STDOUT_SOURCE), "")
    lines = text.splitlines()
    return (
        tuple(match.group(1) for line in lines if (match := _EVIDENCE_RE.search(line)) is not None),
        tuple(match.group(1) for line in lines if (match := _CAPTURE_RE.search(line)) is not None),
    )


def first_fault(result: _BridgeResult) -> tuple[str, str]:
    """Extract the phase label and diagnostic message for the first failing phase.

    A reply-level fault outranks phase rows; otherwise phases stream in wire order so the
    first defect is the retriage entry.

    Returns:
        Pair of (phase_label, diagnostic_message); both empty strings when the result is clean.
    """
    match result.fault:
        case _BridgeFault() as fault:
            return ("reply", _fault_message(fault))
        case _:
            return next(
                ((phase.phase, _phase_diagnostic(phase)) for phase in result.phases if phase.status.severity > RailStatus.OK.severity), ("", "")
            )


def _fault_message(fault: _BridgeFault) -> str:
    # causes is a flattened inner-exception chain; the first entry is the proximate failure.
    return next((tail.message for tail in fault.causes), fault.message)[:256]


def _phase_diagnostic(phase: _BridgePhase) -> str:
    # Scenario markers stream to stdout; every other phase (compile, load) uses stderr as the primary diagnostic stream.
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
    # The result file is authoritative; client stderr carries structlog infrastructure logs, not scenario JSON.
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
    root = Path(str(settings.root))
    parts = scenario.resolve().relative_to(root.resolve()).parts
    match parts:
        case ("tests", "csharp", "libs", project, *_):
            candidate = root / Path(*_LIB_ROOT) / project / f"{project}.Tests.csproj"
            return candidate if candidate.is_file() else root / _TESTKIT_PROJECT
        case _:
            return root / _TESTKIT_PROJECT


def _client_ready(settings: AssaySettings) -> Result[None, str]:
    # --no-build skips restore/compile; a missing client DLL must surface as an operational fault, not a scenario defect.
    bin_dir = Path(str(settings.root)) / Path(_CLIENT_PROJECT).parent / "bin" / settings.configuration.value
    if any(bin_dir.glob("*/Rasm.RhinoBridge.Client.dll")):
        return Ok(None)
    return Error(f"bridge client is not built under {bin_dir}; run `bridge build` first")


def _run_scenario(settings: AssaySettings, report_dir: Path, scenario: Path) -> _Scenario:
    # Launch and timeout faults become FAILED rows so every discovered scenario appears in the fold.
    stem = scenario.name.removesuffix(_SCENARIO_SUFFIX) or scenario.stem
    result_path = report_dir / f"{stem}.json"
    project = _resolve_project(settings, scenario)
    ready = _client_ready(settings)
    match ready.is_error():
        case True:
            reason = ready.error
            return _Scenario(
                status=RailStatus.FAILED,
                stem=stem,
                fault_phase="client",
                fault_output=reason[:256],
                completed=receipt((stem, "client"), 1, status=RailStatus.FAILED, notes=(reason,)),
            )
        case False:
            return _run_decoded(settings, project, scenario, result_path, stem)


def _run_decoded(settings: AssaySettings, project: Path, scenario: Path, result_path: Path, stem: str) -> _Scenario:
    run = client_run(settings, "check", str(project), str(scenario), "--result", str(result_path), timeout=_SCENARIO_TIMEOUT_S)
    match run:
        case Result(tag="ok", ok=done):
            res = _decode_result(done, result_path)
            phase, output = first_fault(res)
            facts, captures = _markers(res)
            return _Scenario(
                status=res.status,
                stem=stem,
                exceptions=_exceptions(res),
                fault_phase=phase,
                fault_output=output,
                facts=facts,
                captures=captures,
                completed=done,
            )
        case Result(error=fault):
            return _Scenario(
                status=RailStatus.FAILED,
                stem=stem,
                fault_phase="launch",
                fault_output=(fault.message or " ".join(fault.argv))[:256],
                completed=receipt(fault.argv, 1, status=RailStatus.FAILED),
            )


def _discover(settings: AssaySettings, pattern: str) -> tuple[Path, ...]:
    # Empty pattern returns () (UNSUPPORTED), not a Fault.
    root = Path(str(settings.root))
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
    # Bare name tokens (no glob glyphs) expand to **/<token> so callers can pass a stem without path context.
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
    # Sweeps the claim root (parent of the run-scoped dir) by mtime; skips run_id to avoid racing in-flight scenario JSON.
    claim_root = report_dir.parent.parent
    match claim_root.is_dir():
        case False:
            return
        case True:
            cutoff = time.time() - ttl_s
            _ = tuple(_rmtree(child) for child in claim_root.iterdir() if child.name != run_id and _is_stale(child, claim_root, cutoff))


def _is_stale(child: Path, parent: Path, cutoff: float) -> bool:
    try:
        # resolve() + is_relative_to() prevents symlink escapes from the claim root.
        return child.is_dir() and child.resolve().is_relative_to(parent.resolve()) and child.stat().st_mtime <= cutoff
    except OSError:
        return False


def _rmtree(path: Path) -> Path:
    # Accepted TOCTOU: _expire_stale runs under bridge_lease and skips the live run_id, so a dir re-created
    # between _is_stale and this rmtree belongs to a serialized later run; fd-anchored deletion is rejected.
    shutil.rmtree(path, ignore_errors=True)
    return path


def _ensure_dir(report_dir: Path) -> Result[None, Fault]:
    try:
        report_dir.mkdir(parents=True, exist_ok=True)
        return Ok(None)
    except OSError as exc:
        return Error(Fault(("verify", "report-dir"), RailStatus.FAULTED, str(exc)[:1024]))


# --- [COMPOSITION] ----------------------------------------------------------------------


def bridge_lease[T](settings: AssaySettings, action: Callable[[], Result[T, Fault]]) -> Result[T, Fault]:
    """Serialize an action through the process-global Rhino bridge lease.

    One RhinoWIP.app exists per machine, so every client, verify, and package lifecycle
    command acquires the single ``bridge`` resource before touching the live host.

    Returns:
        Action result when the lease is held, or a busy/fault result otherwise.
    """
    return leased("bridge", lambda _held: action(), settings=settings, run_id=settings.run_id, project="bridge")


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Verify bridge scenarios under the live Rhino lease.

    Returns:
        Verification report, or a lease/build/launch fault.
    """
    argv = ("bridge", "verify", params.pattern)
    return bridge_lease(settings, lambda: _verify_locked(settings, scope, params, argv))


def _verify_locked(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...]) -> Result[Report, Fault]:
    # dir creation, build, and launch are sequenced as a faulting prelude; their receipts are discarded on success.
    report_dir = Path(scope.path) / _VERIFY_DIR
    _expire_stale(report_dir, settings.run_id, _VERIFY_TTL_S)
    prelude = _ensure_dir(report_dir).bind(lambda _: _build_closure(settings)).bind(lambda _: _faulted(client_run(settings, "launch")))
    match prelude:
        case Result(tag="ok"):
            return _fold_scenarios(settings, report_dir, _discover(settings, params.pattern), argv)
        case Result(error=fault):
            return Error(fault)


def _fold_scenarios(settings: AssaySettings, report_dir: Path, scenarios: tuple[Path, ...], argv: tuple[str, ...]) -> Result[Report, Fault]:
    # VerifySummary preserves first-failure evidence that fold() erases when it merges statuses order-insensitively.
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
                facts=tuple((row.stem, text) for row in rows for text in row.facts),
                captures=tuple((row.stem, text) for row in rows for text in row.captures),
            )
            report = fold(Claim.BRIDGE, "verify", tuple(row.completed for row in rows), detail=summary)
            return Ok(msgspec.structs.replace(report, artifacts=(*report.artifacts, *_scenario_artifacts(report_dir))))


def _build_closure(settings: AssaySettings) -> Result[Completed, Fault]:
    # Protocol -> plugin -> client -> testkit order ensures the isolated resolver finds Scenario.Run / FactBag before scenario execution.
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
    # Non-zero exits from build or launch mean the closure is unusable; promote to Fault rather than propagating as a defect row.
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(done)
        case Result(tag="ok", ok=done):
            # FAILED in the success channel signals a build/launch defect; FAULTED marks it as an infrastructure failure.
            status = RailStatus.FAULTED if done.status is RailStatus.FAILED else done.status
            return Error(Fault(done.argv, status, (done.stderr or done.stdout or b"")[:1024].decode(errors="replace")))
        case Result(error=fault):
            return Error(fault)


def _lifecycle(settings: AssaySettings, verb: str, *args: str) -> Result[Report, Fault]:
    return bridge_lease(settings, lambda: client_run(settings, verb, *args).map(lambda done: fold(Claim.BRIDGE, verb, (done,))))


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


def quit(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:  # noqa: A001
    """Terminate the bridge host under the live Rhino lease.

    Named to mirror the CLI token; shadows the Python builtin intentionally.

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

__all__ = ["BridgeParams", "bridge_lease", "build", "check", "clean", "client_run", "doctor", "first_fault", "launch", "quit", "verify"]
