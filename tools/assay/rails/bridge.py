"""The sole live-Rhino lane: one ``bridge.lock`` lease, seven verbs, one ``VerifySummary``.

Owns the ``bridge`` claim — the lone C#-only rail driving a running ``RhinoWIP.app`` through the
in-process bridge client (``dotnet run --no-build``). The seven verbs share one exclusive
``bridge.lock`` lease so the fleet runs exactly one live-Rhino proof lane; ``build`` alone is
lease-free as it compiles the client + plugin closure without touching the live host. A non-zero
client exit is a ``Completed(FAILED)`` value, never a ``Fault`` — only a held lease, spawn failure,
or timeout rides the ``Error(Fault)`` channel. The registry opens the ``ArtifactScope`` and threads
it as the second handler argument, so ``report_dir`` is ``scope.path / "verify"``. Bridge-domain
constants (project paths, timeout, retention TTL) root at ``settings.root`` and live with this rail,
never inflated into the polyglot ``AssaySettings`` surface.
"""

from dataclasses import dataclass
from pathlib import Path
import shutil
import time
from typing import Final

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
    Report,  # noqa: TC001  # unconditional import so @checked can resolve the rail's `-> Result[Report, Fault]` return annotation under PEP 649 deferred eval
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

    ``pattern`` selects ``verify`` scenarios (direct ``*.verify.csx`` path, a directory of them, or a
    worktree glob token); lifecycle verbs ignore it. One param shape serves all seven, so a new verb
    is one ``Bind`` row, never a parallel dataclass.
    """

    pattern: str = ""


class _Scenario(msgspec.Struct, frozen=True, gc=False):
    """One folded per-``*.verify.csx`` row: the throwaway carrier ``verify`` reduces into a summary.

    ``stem``/``fault_phase``/``fault_output`` are ordering facts captured per-row because the
    order-insensitive status fold erases them, letting the fold lift them off the first non-zero-exit
    scenario without re-decoding the report dir. ``exceptions`` is summed independently of
    ``counts.failed`` — a scenario can pass its assertion yet still surface in-Rhino exceptions.
    """

    status: RailStatus
    stem: str
    exceptions: int = 0
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

    Decoded with ``forbid_unknown_fields`` *off* because the external bridge JSON carries fields
    this rail does not model; ``phases`` carries both the ``execute`` stdout the facts/captures
    regex scans and the diagnostics the exception sum reads. A malformed payload degrades to a
    ``FAILED`` row — a defect, not an operational ``Fault``.
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
    """The empty C# ``Routed`` every bridge ``Check`` carries: scenarios flow as ``Check.paths``.

    The bridge does not route a change-set — its inputs are explicitly-resolved client/plugin
    projects and ``--`` verb tails carried on ``Check.paths``. A single empty ``Routed`` satisfies
    ``run_check``'s parameter contract without engaging the routing fixpoint.
    """
    return Routed(language=Language.CSHARP, scope=Scope.CHANGED)


def _client_check(settings: AssaySettings, *args: str) -> Check:
    """Bind the bridge client ``Tool`` to a concrete invocation: ``-- <verb> <args>`` tail.

    The projected argv is ``dotnet run --no-build --project <client> --configuration <conf> --
    <args>``. The client project + configuration + post-``--`` verb tail fold into ``tool.command``
    via ``structs.replace`` (mirroring ``api.py`` ``_spawn``) — an empty ``Routed`` would never reach
    argv as ``Check.paths``, so the full invocation must ride the ``Input.NONE`` command body. The
    ``Check`` carries no scope so the client stays on its canonical ``bin/`` output: a present scope
    would splice ``--artifacts-path`` and relocate the build output the ``--no-build`` client reads.
    """
    tail = (str(settings.root / _CLIENT_PROJECT), "--configuration", settings.configuration.value, "--", *args)
    tool = msgspec.structs.replace(_CLIENT_TOOL, command=(*_CLIENT_TOOL.command, *tail))
    return Check(tool=tool, cwd=Path(str(settings.root)))  # subprocess cwd is inherently local; project UPath → Path


def _client_run(settings: AssaySettings, *args: str, timeout: float | None = None) -> Result[Completed, Fault]:
    """Drive one bridge client invocation through the Engine: a single woven spawn, one loop.

    ``scope=None`` keeps the client on its canonical ``bin/`` output. ``deadline`` defaults to the
    ``Tool.timeout`` budget unless a per-scenario ``timeout`` overrides it; the override builds an
    absolute wall-clock deadline rather than mutating the frozen ``Tool``.
    """
    check = _client_check(settings, *args)
    deadline = time.monotonic() + timeout if timeout is not None else None
    return run_check(check, settings=settings, scope=None, routed=_routed(), deadline=deadline)


# --- [GROUPS] ---------------------------------------------------------------------------


def _exceptions(result: _BridgeResult) -> int:
    """Sum the in-Rhino exception reports across the ``execute`` phase diagnostics.

    Parsed from the scenario JSON ``data.diagnostics`` block — the count is absent from the process
    exit code, and is summed independently of ``counts.failed`` so a scenario that passes its
    assertion yet still surfaces in-Rhino exceptions contributes telemetry.
    """
    return sum(len(_diagnostics(phase.data).exception_reports) for phase in result.phases if phase.phase == _EXECUTE_PHASE and phase.data is not None)


def _diagnostics(data: dict[str, object] | None) -> _BridgeDiagnostics:
    """Convert the ``data.diagnostics`` sub-object to a typed roster; absence/malformed → empty roster."""
    match data:
        case {"diagnostics": raw}:
            return _coerce_diagnostics(raw)
        case _:
            return _BridgeDiagnostics()


def _coerce_diagnostics(raw: object) -> _BridgeDiagnostics:
    """Coerce a raw diagnostics object into the typed roster; a non-conforming shape → empty roster."""
    try:
        return msgspec.convert(raw, type=_BridgeDiagnostics, strict=False)
    except msgspec.ValidationError:
        return _BridgeDiagnostics()


def _first_fault(result: _BridgeResult) -> tuple[str, str]:
    """Project the earliest failing lifecycle phase and a bounded first-diagnostic snippet.

    Phases stream in wire order (``launch``→``execute``→``check``→``cleanup``), so the first phase
    whose status severity exceeds ``OK`` is the earliest fault — the retriage entry point. Its
    diagnostic is the first non-empty captured output (stderr preferred over stdout), falling back to
    the earliest in-Rhino exception report for an ``execute`` phase that failed without a captured
    stream. A clean result projects ``("", "")``; the snippet is bounded to the model's 256-char cap.
    """
    return next(((phase.phase, _phase_diagnostic(phase)) for phase in result.phases if phase.status.severity > RailStatus.OK.severity), ("", ""))


def _phase_diagnostic(phase: _BridgePhase) -> str:
    """The earliest bounded diagnostic of one failing phase: stderr → stdout → exception roster head."""
    streamed = next((out.text for src in (_STDERR_SOURCE, _STDOUT_SOURCE) for out in phase.outputs if out.source == src and out.text), "")
    match (streamed, _diagnostics(phase.data).exception_reports):
        case ("", (head, *_)):
            return head.message[:256]
        case _:
            return streamed[:256]


def _decode_result(run: Completed, result_path: Path) -> _BridgeResult:
    """Decode the per-scenario ``--result`` JSON first, captured stdout/stderr only as fallback.

    The client streams structlog to stderr, so trusting stdout alone would conflate diagnostics
    with evidence; the result file is preferred when present. A decode miss degrades to a
    ``FAILED`` ``_BridgeResult`` — a malformed scenario payload is a defect, not a rail ``Fault``.
    """
    raw = _read_result(result_path).default_value(run.stdout or run.stderr)
    try:
        return _RESULT_DECODER.decode(raw or b"{}")
    except msgspec.DecodeError:
        return _BridgeResult(status=RailStatus.FAILED)


def _read_result(result_path: Path) -> Result[bytes, Fault]:
    """Read the per-scenario result file when present; a missing/unreadable file is ``Error`` (fall back)."""
    try:
        return Ok(result_path.read_bytes()) if result_path.is_file() else Error(Fault(("read", str(result_path)), RailStatus.SKIP))
    except OSError as exc:
        return Error(Fault(("read", str(result_path)), RailStatus.FAULTED, str(exc)[:1024]))


def _run_scenario(settings: AssaySettings, report_dir: Path, scenario: Path) -> _Scenario:
    """Drive one ``*.verify.csx`` through the bridge client, decoding the per-scenario evidence.

    Issues ``check <scenario> --result <path>`` under the per-scenario timeout, then decodes the
    result JSON. A rail-level spawn/timeout ``Fault`` projects to a ``FAILED`` row rather than
    short-circuiting the fold: a non-running client is a scenario failure, which keeps the
    comprehension total over every discovered scenario.
    """
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
    """The predecessor's three-stage discovery fold: direct → directory → worktree glob.

    A ``pattern`` resolving to a ``*.verify.csx`` file is taken verbatim; a directory is scanned for
    ``*.verify.csx`` descendants; an empty result falls back to a worktree glob. The return is the
    sorted-deduped resolved tuple ``verify`` folds; an empty tuple is the ``UNSUPPORTED``
    precondition — a valid request with no applicable path, never a ``Fault``.
    """
    root = Path(str(settings.root))  # scenario discovery globs the local worktree; UPath → Path at the boundary
    direct = _direct(root, pattern)
    return direct or _glob(root, pattern)


def _direct(root: Path, pattern: str) -> tuple[Path, ...]:
    """Resolve a direct ``*.verify.csx`` file or a directory of them; neither shape yields ``()``."""
    candidate = Path(pattern) if Path(pattern).is_absolute() else root / pattern
    match (candidate.is_file(), candidate.is_dir(), candidate.name.endswith(_SCENARIO_SUFFIX)):
        case (True, _, True):
            return (candidate.resolve(),)
        case (_, True, _):
            return tuple(sorted(p.resolve() for p in candidate.rglob(f"*{_SCENARIO_SUFFIX}") if p.is_file()))
        case _:
            return ()


def _glob(root: Path, pattern: str) -> tuple[Path, ...]:
    """The worktree glob fallback: path-shaped pattern verbatim, bare token as ``**/<token>``.

    ``Path.glob`` treats the positional as a glob, not a regex, so a bare token (``smoke``) is
    expanded to ``**/smoke`` to mirror the predecessor's fd-positional recursive scan. Only
    ``*.verify.csx`` files survive.
    """
    normalized = pattern if any(glyph in pattern for glyph in _PATH_GLYPHS) else f"**/{pattern}"
    return tuple(sorted(p.resolve() for p in root.glob(normalized) if p.is_file() and p.name.endswith(_SCENARIO_SUFFIX)))


# --- [RETENTION] ------------------------------------------------------------------------


def _expire_stale(report_dir: Path, ttl_s: float) -> None:
    """Sweep stale per-run report subdirs before launch: ``st_mtime <= cutoff``, scope-bounded.

    TTL expiry must precede launch, never follow it — expiring after a populated run would race the
    freshly-written per-scenario JSON, so the sweep runs on the *parent* before the client writes any
    artifact. Each candidate is bounded to ``is_relative_to(parent)`` so a symlinked report path cannot
    ``rmtree`` outside the scope. A missing parent or an ``OSError`` is a no-op and the launch
    proceeds: retention is best-effort housekeeping, never a rail ``Fault``.
    """
    parent = report_dir.parent
    match parent.is_dir():
        case False:
            return
        case True:
            cutoff = time.time() - ttl_s
            _ = tuple(_rmtree(child) for child in parent.iterdir() if _is_stale(child, parent, cutoff))


def _is_stale(child: Path, parent: Path, cutoff: float) -> bool:
    """A scope-bounded directory whose mtime is at or before the cutoff is stale (sweep-eligible)."""
    try:
        return child.is_dir() and child.resolve().is_relative_to(parent.resolve()) and child.stat().st_mtime <= cutoff
    except OSError:
        return False


def _rmtree(path: Path) -> Path:
    """Remove one stale report subtree (returning it for the consuming sweep); ``OSError`` is a no-op."""
    shutil.rmtree(path, ignore_errors=True)
    return path


def _ensure_dir(report_dir: Path) -> Result[None, Fault]:
    """Create the per-run report directory before the client writes any ``--result`` JSON."""
    try:
        report_dir.mkdir(parents=True, exist_ok=True)
        return Ok(None)
    except OSError as exc:
        return Error(Fault(("verify", "report-dir"), RailStatus.FAULTED, str(exc)[:1024]))


# --- [COMPOSITION] ----------------------------------------------------------------------


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Fold every routed ``*.verify.csx`` into one ``Report`` carrying a ``VerifySummary``.

    Acquires ``bridge.lock`` under ``leased`` — a busy lock short-circuits to ``Fault(BUSY)``
    without running — expires stale reports before launch, builds the client + plugin closure,
    launches Rhino, then folds each routed scenario into the summary. Discovery matching zero
    scenarios yields ``UNSUPPORTED`` (exit 3). ``verify`` is a plain function: the registry folds
    its returned ``Result`` in ``_emit``, never an ``@effect.result`` generator.
    """
    argv = ("bridge", "verify", params.pattern)
    return leased("bridge", lambda _held: _verify_locked(settings, scope, params, argv), settings=settings, run_id=settings.run_id, project="bridge")


def _verify_locked(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...]) -> Result[Report, Fault]:
    """The leased ``verify`` body: TTL → build → launch → discover → fold, on one rail.

    Sequenced through the ``Result`` rail so any prelude ``Fault`` (build/launch failure)
    short-circuits before discovery: the stale-report sweep and a fresh client must precede
    scenario execution. A zero-scenario discovery returns ``UNSUPPORTED`` rather than an empty fold.
    """
    report_dir = Path(scope.path) / _VERIFY_DIR
    _expire_stale(report_dir, _VERIFY_TTL_S)
    prelude = _ensure_dir(report_dir).bind(lambda _: _build_closure(settings)).bind(lambda _: _affirm(_client_run(settings, "launch")))
    match prelude:
        case Result(tag="ok"):
            return _fold_scenarios(settings, report_dir, _discover(settings, params.pattern), argv)
        case Result(error=fault):
            return Error(fault)


def _fold_scenarios(settings: AssaySettings, report_dir: Path, scenarios: tuple[Path, ...], argv: tuple[str, ...]) -> Result[Report, Fault]:
    """Reduce discovered scenarios into one ``Report``: ``UNSUPPORTED`` on the empty discovery.

    Counts and overall status derive in ``model.fold`` from the per-scenario ``Completed``
    receipts, so the ``VerifySummary`` carries only the non-derivable bridge facts
    (``exceptions``/``report_dir``/``first_failure``/``first_fault_phase``/``first_fault_output``).
    The earliest non-zero-exit row is lifted once in discovery order — before the order-insensitive
    status fold erases it — and projects the failure identity plus its failing phase + diagnostic
    snippet so an agent retriages from the summary without opening the report dir.
    """
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
    """Compile the protocol + plugin + client closure (lease-free): a non-zero exit short-circuits.

    Builds the three bridge projects under one stable per-closure ``ArtifactScope.build`` tree (warm
    MSBuild/analyzer cache) so the live host loads freshly-built plugin assemblies and the
    ``--no-build`` client observes the same output. A failed compile promotes to a ``Fault`` at this
    seam — a broken plugin must abort ``verify`` before launch rather than drive a stale host.
    """
    scope = ArtifactScope.build(settings, "bridge")
    projects = (_PROTOCOL_PROJECT, _PLUGIN_PROJECT, _CLIENT_PROJECT)
    check = Check(tool=_BUILD_TOOL, owner="bridge", cwd=Path(str(settings.root)))  # subprocess cwd is inherently local
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=tuple(str(settings.root / p) for p in projects))
    return _affirm(run_check(check, settings=settings, scope=scope, routed=routed, deadline=None))


def _affirm(outcome: Result[Completed, Fault]) -> Result[None, Fault]:
    """Promote a non-zero build/launch exit to a ``Fault``: an ``EMPTY``/``OK`` exit passes through.

    A ``Completed`` at or below ``OK`` severity is a clean run (``None``); a higher severity is promoted
    to an ``Error(Fault)`` carrying the argv so the prelude rail short-circuits. A ``FAILED`` exit maps to
    ``FAULTED`` — the ``Fault`` contract reserves ``FAILED`` for the success channel (a check ran and found
    defects), whereas a failed build/launch means assay could not run the verify (exit 2); a process that
    itself exited ``TIMEOUT``/``BUSY`` keeps that status. A rail-level ``Fault`` passes through unchanged.
    """
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(None)
        case Result(tag="ok", ok=done):
            status = RailStatus.FAULTED if done.status is RailStatus.FAILED else done.status
            return Error(Fault(done.argv, status, (done.stderr or done.stdout or b"")[:1024].decode(errors="replace")))
        case Result(error=fault):
            return Error(fault)


def _lifecycle(settings: AssaySettings, verb: str, *args: str) -> Result[Report, Fault]:
    """The one parameterized fold over ``_client_run`` for every lifecycle verb.

    ``doctor``/``launch``/``quit``/``check``/``clean`` differ only by the client subcommand and its
    operands, so they collapse to one body under one ``bridge.lock`` lease, folding into a single
    ``Report`` (no ``Detail``). A new lifecycle verb is one more delegating handler plus a ``Bind``
    row — never a new module or a new lock.
    """
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
    """Compile the ``rasm-bridge`` plugin + client closure (lease-free).

    The sole lease-free verb — it never drives the live host, so it does not serialize on
    ``bridge.lock``. A clean compile folds to an ``EMPTY``/``OK`` ``Report``; a broken one promotes
    to a ``Fault`` via ``_affirm`` so the verb's exit code carries the analyzer/compile proof.
    """
    _ = (scope, params)
    return _build_closure(settings).map(lambda _: fold(Claim.BRIDGE, "build", (receipt(("bridge", "build"), 0),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "build", "check", "clean", "doctor", "launch", "quit", "verify"]
