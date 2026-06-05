"""The sole live-Rhino lane: one ``bridge.lock`` lease, seven verbs, one ``VerifySummary``.

The seven verbs serialize on one exclusive ``bridge.lock`` so the fleet runs exactly one live-Rhino
proof lane; ``build`` alone is lease-free. A non-zero client exit is a ``Completed(FAILED)`` value,
never a ``Fault`` — only a held lease, spawn failure, or timeout rides the ``Error(Fault)`` channel.
"""

from dataclasses import dataclass
from pathlib import Path
import shutil
import time
from typing import Final, override

from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import leased, run_check  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
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
from tools.assay.core.routing import Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BridgeParams(BaseParams):
    """The ``bridge`` verb params: ``BaseParams`` plus the sole ``pattern`` scenario selector.

    Zero-positional contract: ``verify`` reads its ``--pattern`` keyword and the lifecycle verbs read
    nothing positional, so a bare token is a typo — ``bound`` folds it to the canonical ``parse`` Fault.
    """

    pattern: str = ""  # selects verify scenarios (path / dir / worktree glob); lifecycle verbs ignore it

    @override
    def _arity(self, verb: str) -> int:
        """Zero positional slots: ``bridge`` is keyword-only (``--pattern``), so the base ``bound`` folds any token to ``parse``."""
        _ = verb
        return 0


class _Scenario(msgspec.Struct, frozen=True, gc=False):
    """One folded per-``*.verify.csx`` row: the throwaway carrier ``verify`` reduces into a summary."""

    # stem/fault_phase/fault_output are ordering facts the order-insensitive status fold erases, so
    # the fold lifts them off the first non-zero-exit scenario without re-decoding the report dir.
    status: RailStatus
    stem: str
    exceptions: int = 0  # summed independently of counts.failed: a pass can still surface in-Rhino exceptions
    fault_phase: str = ""
    fault_output: str = ""
    completed: Completed = msgspec.field(default_factory=lambda: receipt((), 0))


class _BridgeFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    """One in-Rhino exception report (``BridgeResult.exceptionReports``): camel-cased C# JSON."""

    category: str = ""
    message: str = ""
    type: str = ""
    stack_trace: str = ""


class _BridgeDiagnostics(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    """The runtime-diagnostics block of a ``BridgeResult.execute`` phase: the exception roster."""

    command_window: tuple[str, ...] = ()
    exception_reports: tuple[_BridgeFault, ...] = ()


class _BridgeOutput(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    """One captured phase output stream (stdout/stderr): the facts/captures evidence carrier."""

    source: str = ""
    text: str = ""
    truncated: bool = False


class _BridgePhase(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    """One bridge lifecycle phase row (``launch``/``execute``/…): outputs plus optional diagnostics."""

    phase: str
    status: RailStatus = RailStatus.FAILED
    data: dict[str, object] | None = None
    outputs: tuple[_BridgeOutput, ...] = ()


class _BridgeResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    """The per-scenario ``--result`` JSON the C# client streams (camel-cased wire).

    Decoded with ``forbid_unknown_fields`` off: the external bridge JSON carries fields this rail does
    not model. A malformed payload degrades to a ``FAILED`` row — a defect, not an operational ``Fault``.
    """

    command: str = ""
    status: RailStatus = RailStatus.FAILED
    report_path: str = ""
    phases: tuple[_BridgePhase, ...] = ()


# --- [CONSTANTS] ------------------------------------------------------------------------

_CLIENT_PROJECT: Final[str] = "tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
_PLUGIN_PROJECT: Final[str] = "tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj"
_PROTOCOL_PROJECT: Final[str] = "tools/rhino-bridge/protocol/Rasm.RhinoBridge.Protocol.csproj"
_SCENARIO_SUFFIX: Final[str] = ".verify.csx"
_SCENARIO_TIMEOUT_S: Final[float] = 180.0  # per-scenario client deadline (ports quality scenario_timeout_s)
_VERIFY_TTL_S: Final[float] = 300.0  # stale per-run report retention before launch (ports verify_retention_seconds)
_VERIFY_DIR: Final[str] = "verify"
_PATH_GLYPHS: Final[str] = "/*?["  # a pattern carrying any of these is path-shaped; else it is a bare token globbed as **/<token>
_EXECUTE_PHASE: Final[str] = "execute"
_STDOUT_SOURCE: Final[str] = "stdout"
_STDERR_SOURCE: Final[str] = "stderr"

_RESULT_DECODER: Final[msgspec.json.Decoder[_BridgeResult]] = msgspec.json.Decoder(_BridgeResult, strict=False)

# `dotnet run --no-build --project <client> -- <args>`: project + conf + verb tail fold into command
# via structs.replace; Input.NONE appends one empty routing tail.
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
# Lease-free compile proof for the protocol + plugin + client closure (never drives the live host).
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
    # the bridge routes no change-set; an empty Routed satisfies run_check without the routing fixpoint
    return Routed(language=Language.CSHARP, scope=Scope.CHANGED)


def _client_check(settings: AssaySettings, *args: str) -> Check:
    # the full invocation rides the Input.NONE command body: an empty Routed never reaches argv as
    # Check.paths. No scope so the --no-build client stays on canonical bin/ (a scope splices --artifacts-path).
    tail = (str(settings.root / _CLIENT_PROJECT), "--configuration", settings.configuration.value, "--", *args)
    tool = msgspec.structs.replace(_CLIENT_TOOL, command=(*_CLIENT_TOOL.command, *tail))
    return Check(tool=tool, cwd=Path(str(settings.root)))  # subprocess cwd is inherently local; project UPath → Path


def _client_run(settings: AssaySettings, *args: str, timeout: float | None = None) -> Result[Completed, Fault]:
    check = _client_check(settings, *args)
    # per-scenario timeout override builds an absolute deadline rather than mutating the frozen Tool
    deadline = time.monotonic() + timeout if timeout is not None else None
    return run_check(check, settings=settings, scope=None, routed=_routed(), deadline=deadline)


# --- [GROUPS] ---------------------------------------------------------------------------


def _exceptions(result: _BridgeResult) -> int:
    # exception count is absent from the exit code; summed independently of counts.failed as telemetry
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
        return _BridgeDiagnostics()  # a non-conforming shape → empty roster


def _first_fault(result: _BridgeResult) -> tuple[str, str]:
    # phases stream in wire order, so the first status severity > OK is the earliest fault (retriage entry)
    return next(((phase.phase, _phase_diagnostic(phase)) for phase in result.phases if phase.status.severity > RailStatus.OK.severity), ("", ""))


def _phase_diagnostic(phase: _BridgePhase) -> str:
    streamed = next((out.text for src in (_STDERR_SOURCE, _STDOUT_SOURCE) for out in phase.outputs if out.source == src and out.text), "")
    match (streamed, _diagnostics(phase.data).exception_reports):
        case ("", (head, *_)):
            return head.message[:256]
        case _:
            return streamed[:256]


def _decode_result(run: Completed, result_path: Path) -> _BridgeResult:
    # prefer the result file: the client streams structlog to stderr, so stdout alone conflates
    # diagnostics with evidence. A decode miss degrades to a FAILED row — a defect, not a rail Fault.
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
    # a rail-level spawn/timeout Fault projects to a FAILED row rather than short-circuiting the fold:
    # a non-running client is a scenario failure, keeping the comprehension total over every scenario.
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


# --- [DISCOVERY] ------------------------------------------------------------------------


def _discover(settings: AssaySettings, pattern: str) -> tuple[Path, ...]:
    # three-stage fold: direct file → directory scan → worktree glob; empty is the UNSUPPORTED
    # precondition (a valid request with no applicable path, never a Fault).
    root = Path(str(settings.root))  # scenario discovery globs the local worktree; UPath → Path at the boundary
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
    # Path.glob treats the positional as a glob, so a bare token expands to **/<token> (recursive scan)
    normalized = pattern if any(glyph in pattern for glyph in _PATH_GLYPHS) else f"**/{pattern}"
    return tuple(sorted(p.resolve() for p in root.glob(normalized) if p.is_file() and p.name.endswith(_SCENARIO_SUFFIX)))


# --- [RETENTION] ------------------------------------------------------------------------


def _expire_stale(report_dir: Path, ttl_s: float) -> None:
    # must precede launch: expiring after a populated run would race the freshly-written per-scenario
    # JSON. Best-effort housekeeping — a missing parent or OSError is a no-op, never a rail Fault.
    parent = report_dir.parent
    match parent.is_dir():
        case False:
            return
        case True:
            cutoff = time.time() - ttl_s
            _ = tuple(_rmtree(child) for child in parent.iterdir() if _is_stale(child, parent, cutoff))


def _is_stale(child: Path, parent: Path, cutoff: float) -> bool:
    try:
        # is_relative_to bounds the sweep so a symlinked report path cannot rmtree outside the scope
        return child.is_dir() and child.resolve().is_relative_to(parent.resolve()) and child.stat().st_mtime <= cutoff
    except OSError:
        return False


def _rmtree(path: Path) -> Path:
    shutil.rmtree(path, ignore_errors=True)
    return path  # returned for the consuming sweep comprehension


def _ensure_dir(report_dir: Path) -> Result[None, Fault]:
    try:
        report_dir.mkdir(parents=True, exist_ok=True)
        return Ok(None)
    except OSError as exc:
        return Error(Fault(("verify", "report-dir"), RailStatus.FAULTED, str(exc)[:1024]))


# --- [COMPOSITION] ----------------------------------------------------------------------


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Fold every routed ``*.verify.csx`` into one ``Report`` carrying a ``VerifySummary``.

    Acquires ``bridge.lock`` (a busy lock short-circuits to ``Fault(BUSY)``), expires stale reports,
    builds the closure, launches Rhino, then folds each scenario. Zero scenarios → ``UNSUPPORTED``.
    """
    argv = ("bridge", "verify", params.pattern)
    return leased("bridge", lambda _held: _verify_locked(settings, scope, params, argv), settings=settings, run_id=settings.run_id, project="bridge")


def _verify_locked(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...]) -> Result[Report, Fault]:
    # sequenced on the Result rail so a prelude Fault (build/launch) short-circuits before discovery:
    # the stale-report sweep and a fresh client must precede scenario execution.
    report_dir = Path(scope.path) / _VERIFY_DIR
    _expire_stale(report_dir, _VERIFY_TTL_S)
    prelude = _ensure_dir(report_dir).bind(lambda _: _build_closure(settings)).bind(lambda _: _affirm(_client_run(settings, "launch")))
    match prelude:
        case Result(tag="ok"):
            return _fold_scenarios(settings, report_dir, _discover(settings, params.pattern), argv)
        case Result(error=fault):
            return Error(fault)


def _fold_scenarios(settings: AssaySettings, report_dir: Path, scenarios: tuple[Path, ...], argv: tuple[str, ...]) -> Result[Report, Fault]:
    # counts/status derive in model.fold from the Completed receipts, so VerifySummary carries only the
    # non-derivable bridge facts. The earliest non-zero-exit row is lifted once in discovery order —
    # before the order-insensitive status fold erases it — so an agent retriages without opening the dir.
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
    # one stable per-closure build tree (warm cache) so the live host and --no-build client observe the
    # same output. A failed compile promotes to a Fault here — a broken plugin must abort before launch.
    scope = ArtifactScope.build(settings, "bridge")
    projects = (_PROTOCOL_PROJECT, _PLUGIN_PROJECT, _CLIENT_PROJECT)
    check = Check(tool=_BUILD_TOOL, owner="bridge", cwd=Path(str(settings.root)))  # subprocess cwd is inherently local
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=tuple(str(settings.root / p) for p in projects))
    return _affirm(run_check(check, settings=settings, scope=scope, routed=routed, deadline=None))


def _affirm(outcome: Result[Completed, Fault]) -> Result[None, Fault]:
    # promote a non-zero build/launch exit to a Fault; ≤ OK severity passes through as None.
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(None)
        case Result(tag="ok", ok=done):
            # FAILED → FAULTED: the Fault contract reserves FAILED for the success channel (a check ran
            # and found defects); a failed build/launch means assay could not run the verify (exit 2).
            status = RailStatus.FAULTED if done.status is RailStatus.FAILED else done.status
            return Error(Fault(done.argv, status, (done.stderr or done.stdout or b"")[:1024].decode(errors="replace")))
        case Result(error=fault):
            return Error(fault)


def _lifecycle(settings: AssaySettings, verb: str, *args: str) -> Result[Report, Fault]:
    # one parameterized fold for every lifecycle verb: they differ only by client subcommand + operands,
    # collapsing to one body under one bridge.lock lease, folding into a single Report (no Detail).
    return leased(
        "bridge",
        lambda _held: _client_run(settings, verb, *args).map(lambda done: fold(Claim.BRIDGE, verb, (done,))),
        settings=settings,
        run_id=settings.run_id,
        project="bridge",
    )


def doctor(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Bridge host health probe: ``dotnet run … -- doctor`` under the live-Rhino lease."""
    _ = (scope, params)
    return _lifecycle(settings, "doctor")


def launch(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Start ``RhinoWIP.app`` under the lease: ``dotnet run … -- launch``."""
    _ = (scope, params)
    return _lifecycle(settings, "launch")


def quit(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:  # noqa: A001  # registry verb name; the rail surface mirrors the CLI token, never the builtin
    """Clean Cocoa terminate of the live host: ``dotnet run … -- quit``."""
    _ = (scope, params)
    return _lifecycle(settings, "quit")


def check(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Liveness probe of the running host: ``dotnet run … -- check``."""
    _ = (scope, params)
    return _lifecycle(settings, "check")


def clean(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Clear crash markers + autosave docs: ``dotnet run … -- clean``."""
    _ = (scope, params)
    return _lifecycle(settings, "clean")


def build(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Compile the ``rasm-bridge`` plugin + client closure (the sole lease-free verb)."""
    _ = (scope, params)
    return _build_closure(settings).map(lambda _: fold(Claim.BRIDGE, "build", (receipt(("bridge", "build"), 0),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "build", "check", "clean", "doctor", "launch", "quit", "verify"]
