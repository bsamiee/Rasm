"""Run live Rhino bridge supervisor lifecycle and verification commands."""

from collections.abc import Callable  # noqa: TC003  # beartype resolves the public bridge_lease signature at runtime under PEP 649
from dataclasses import dataclass
from fnmatch import fnmatchcase
from pathlib import Path
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
    Completed,  # noqa: TC001  # beartype resolves Result[Completed, Fault] forward-ref at runtime under PEP 649
    Diagnostic,
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
from tools.assay.core.status import fold as status_fold, RailStatus


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUPERVISOR_PROJECT: Final[str] = "tools/rhino-bridge/Supervisor/Supervisor.csproj"
_STUB_PROJECT: Final[str] = "tools/rhino-bridge/Stub/Stub.csproj"
_SHELL_PROJECT: Final[str] = "tools/rhino-bridge/Shell/Shell.csproj"
_CARGO_PROJECT: Final[str] = "tools/rhino-bridge/Cargo/Cargo.csproj"
_CONTRACT_PROJECT: Final[str] = "tools/rhino-bridge/Contract/Contract.csproj"
_SCENARIO_PROJECTS: Final[tuple[str, ...]] = (
    "tests/csharp/libs/Rasm/Rasm.Tests.csproj",
    "tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
    "tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
)
_BUILD_PROJECTS: Final[tuple[str, ...]] = (_CONTRACT_PROJECT, _CARGO_PROJECT, _SHELL_PROJECT, _STUB_PROJECT, _SUPERVISOR_PROJECT, *_SCENARIO_PROJECTS)
_BRIDGE_REPORT_ROOT: Final[tuple[str, ...]] = (".artifacts", "assay", "bridge")
_CLOSURE_FILE: Final[str] = "bridge-closure.json"
_AGGREGATE_CLOSURE_FILE: Final[str] = "bridge-closure.assay.json"
_SCENARIO_TIMEOUT_S: Final[float] = 600.0
_VERIFY_TTL_S: Final[float] = 300.0
_PATH_GLYPHS: Final[str] = "/*?["
_ALL_TOKENS: Final[frozenset[str]] = frozenset(("", "all", "*"))
_LIFECYCLE_ALIASES: Final[dict[str, str]] = {"check": "doctor", "clean": "quit", "launch": "doctor", "refresh": "doctor"}
_TEXT_ARTIFACT_SUFFIXES: Final[frozenset[str]] = frozenset((".json", ".jsonl", ".log", ".txt"))
_BRIDGE_ARTIFACT_SUFFIXES: Final[frozenset[str]] = frozenset((".gcdump", ".json", ".jsonl", ".png"))


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BridgeParams(BaseParams):
    """Parameters shared by bridge verbs."""

    @override
    def _arity(self, verb: str) -> int:
        return 1 if verb == "verify" else 0

    @property
    def pattern(self) -> str:
        """Scenario selection token; empty selects every typed scenario corpus."""
        return self.paths[0] if self.paths else ""


@dataclass(frozen=True, slots=True)
class _ScenarioCorpus:
    theme_names: tuple[tuple[str, str], ...]
    project: str
    assembly: str
    paths: tuple[str, ...] = ()

    @property
    def themes(self) -> frozenset[str]:
        return frozenset(theme for theme, _ in self.theme_names)

    @property
    def names(self) -> frozenset[str]:
        return frozenset(f"{theme}.{name}" for theme, name in self.theme_names)


@dataclass(frozen=True, slots=True)
class _SelectionPlan:
    corpora: tuple[_ScenarioCorpus, ...]
    selection_json: str


class _HostFingerprint(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    bundle_version: str = ""
    rhino_common_version: str = ""
    grasshopper2_version: str = ""
    runtime_version: str = ""


class _ClosureManifest(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    assemblies: tuple[str, ...] = ()
    host_plugins: tuple[str, ...] = ()
    built_against: _HostFingerprint = msgspec.field(default_factory=_HostFingerprint)


class _EventStamp(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    scenario: str | None = None


class _SessionFault(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    kind: str = msgspec.field(default="", name="$type")
    detail: str = ""
    prescription: str = ""
    capability: str = ""
    probe_receipt: str = ""
    failing_check: str = ""
    scenario: str = ""
    elapsed_ms: float = 0.0
    silent_for_ms: float = 0.0


class _SessionScenario(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    scenario: str = ""
    status: RailStatus = RailStatus.SKIP
    duration_ms: float = 0.0
    fault: _SessionFault | None = None


class _SessionEvidence(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    kind: str = msgspec.field(default="", name="$type")
    stamp: _EventStamp = msgspec.field(default_factory=_EventStamp)
    key: str = ""
    value: object = None
    path: str = ""
    width: int = 0
    height: int = 0
    on_failure: bool = False


class _SessionEnvelope(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    run_id: str = ""
    verb: str = ""
    status: RailStatus = RailStatus.FAILED
    duration_ms: float = 0.0
    report_dir: str = ""
    scenarios: tuple[_SessionScenario, ...] = ()
    evidence: tuple[_SessionEvidence, ...] = ()
    first_failure: str = ""
    first_fault_phase: str | None = None
    fault: _SessionFault | None = None


# --- [TABLES] ---------------------------------------------------------------------------

_SCENARIO_CORPORA: Final[tuple[_ScenarioCorpus, ...]] = (
    _ScenarioCorpus(
        project="tests/csharp/libs/Rasm/Rasm.Tests.csproj",
        assembly="Rasm.Tests.dll",
        paths=(
            "tests/csharp/libs/Rasm/Analysis",
            "tests/csharp/libs/Rasm/Analysis/Scenarios",
            "tests/csharp/libs/Rasm/Vectors",
            "tests/csharp/libs/Rasm/Vectors/Scenarios",
        ),
        theme_names=(
            ("analysis", "NativeRail"),
            ("vectors", "CloudShapes"),
            ("vectors", "CloudNeighborhood"),
            ("vectors", "CloudHull"),
            ("vectors", "FieldSdfIsosurface"),
            ("vectors", "AtomsFrame"),
            ("vectors", "SpaceProjection"),
            ("vectors", "SampleDworkContinuous"),
            ("vectors", "SpectralDec"),
            ("vectors", "SpectralDescriptor"),
            ("vectors", "SpectralEdgeConnection"),
        ),
    ),
    _ScenarioCorpus(
        project="tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
        assembly="Rasm.Rhino.Tests.dll",
        paths=(
            "tests/csharp/libs/Rasm.Rhino/Blocks",
            "tests/csharp/libs/Rasm.Rhino/Blocks/Scenarios",
            "tests/csharp/libs/Rasm.Rhino/Camera",
            "tests/csharp/libs/Rasm.Rhino/Camera/Scenarios",
            "tests/csharp/libs/Rasm.Rhino/Exchange",
            "tests/csharp/libs/Rasm.Rhino/Exchange/Scenarios",
            "tests/csharp/libs/Rasm.Rhino/UI",
            "tests/csharp/libs/Rasm.Rhino/UI/Scenarios",
        ),
        theme_names=(
            ("blocks", "CoreRail"),
            ("blocks", "Stats"),
            ("blocks", "GraphPlan"),
            ("blocks", "Author"),
            ("blocks", "Bounds"),
            ("blocks", "WriteAttributes"),
            ("blocks", "ArchiveClosure"),
            ("blocks", "ArchiveValidateBroken"),
            ("blocks", "NativeAdd"),
            ("blocks", "PlacementReference"),
            ("camera", "NamedViewRail"),
            ("exchange", "Exchange"),
            ("ui", "MotionOverlay"),
            ("ui", "Paint"),
            ("ui", "ProjectionHud"),
        ),
    ),
    _ScenarioCorpus(
        project="tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
        assembly="Rasm.Grasshopper.Tests.dll",
        paths=(
            "tests/csharp/libs/Rasm.Grasshopper/UI",
            "tests/csharp/libs/Rasm.Grasshopper/UI/Scenarios",
        ),
        theme_names=(("gh-ui", "MotionLayout"),),
    ),
)
_ENVELOPE_DECODER: Final[msgspec.json.Decoder[_SessionEnvelope]] = msgspec.json.Decoder(_SessionEnvelope, strict=False)
_CLOSURE_DECODER: Final[msgspec.json.Decoder[_ClosureManifest]] = msgspec.json.Decoder(_ClosureManifest, strict=False)

_SUPERVISOR_TOOL: Final[Tool] = Tool(
    name="rasm-bridge",
    runner=Runner.DOTNET,
    command=("run", "--no-build", "--project"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.BRIDGE,
    mode=Mode.VERIFY,
    timeout=_SCENARIO_TIMEOUT_S,
)
_BUILD_TOOL: Final[Tool] = Tool(
    name="rasm-bridge-build",
    runner=Runner.DOTNET,
    command=("build", "-tl:off", "-v:quiet", "/clp:ErrorsOnly"),
    input=Input.OWNED,
    language=Language.CSHARP,
    claim=Claim.BRIDGE,
    mode=Mode.BUILD,
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _routed() -> Routed:
    return Routed(language=Language.CSHARP, scope=Scope.CHANGED)


def _client_check(settings: AssaySettings, *args: str) -> Check:
    verb = _LIFECYCLE_ALIASES.get(args[0], args[0]) if args else "doctor"
    tail = (str(settings.root / _SUPERVISOR_PROJECT), "--configuration", settings.configuration.value, "--", verb, *args[1:])
    return Check(tool=msgspec.structs.replace(_SUPERVISOR_TOOL, command=(*_SUPERVISOR_TOOL.command, *tail)), cwd=Path(str(settings.root)))


def client_run(settings: AssaySettings, *args: str, timeout: float | None = None) -> Result[Completed, Fault]:
    """Run the rhino-bridge supervisor with the given verb arguments.

    ``timeout`` is a wall-clock budget in seconds, mapped to the run deadline.

    Returns:
        Supervisor completion receipt, or an operational fault.
    """
    check = _client_check(settings, *args)
    deadline = time.monotonic() + timeout if timeout is not None else None
    scope = ArtifactScope.build(settings, "bridge")
    return run_check(check, settings=settings, scope=scope, routed=_routed(), deadline=deadline).map(_completed_from_stdout)


def first_fault(envelope: _SessionEnvelope) -> tuple[str, str]:
    """Extract first supervisor fault phase and message from a SessionEnvelope.

    Returns:
        Fault phase plus bounded message, or empty strings when the session succeeded.
    """
    return ((envelope.first_fault_phase or "session"), envelope.first_failure[:256]) if envelope.first_failure else ("", "")


def _decode_envelope(run: Completed) -> _SessionEnvelope:
    raw = run.stdout.strip()
    try:
        return _ENVELOPE_DECODER.decode(raw or b"{}")
    except msgspec.MsgspecError:
        for path in tuple(Path(artifact.path) for artifact in run.artifacts if artifact.kind is ArtifactKind.PROCESS and Path(artifact.path).name == "out.log"):
            try:
                return _ENVELOPE_DECODER.decode(path.read_bytes().strip() or b"{}")
            except OSError, msgspec.MsgspecError:
                pass
        text = (run.stderr or run.stdout).decode(errors="replace").strip()
        return _SessionEnvelope(status=RailStatus.FAILED, first_failure=text[:256] or "supervisor emitted no SessionEnvelope")


def _completed_from_stdout(run: Completed) -> Completed:
    envelope = _decode_envelope(run)
    report_artifacts = _scenario_artifacts(Path(envelope.report_dir)) if envelope.report_dir else ()
    return msgspec.structs.replace(
        run,
        status=envelope.status,
        notes=(*run.notes, *(("bridge.reportDir=" + envelope.report_dir,) if envelope.report_dir else ())),
        artifacts=(*run.artifacts, *report_artifacts),
    )


def _faulted(outcome: Result[Completed, Fault]) -> Result[Completed, Fault]:
    match outcome:
        case Result(tag="ok", ok=done) if done.status.severity <= RailStatus.OK.severity:
            return Ok(done)
        case Result(tag="ok", ok=done):
            status = RailStatus.FAULTED if done.status is RailStatus.FAILED else done.status
            return Error(Fault(done.argv, status, (done.stderr or done.stdout or b"")[:1024].decode(errors="replace")))
        case Result(error=fault):
            return Error(fault)


def _theme_tokens(pattern: str) -> tuple[str, ...]:
    return tuple(part.strip() for part in pattern.split(",") if part.strip())


def _plan(pattern: str) -> Result[_SelectionPlan, Fault]:
    tokens = _theme_tokens(pattern)
    if pattern.strip() in _ALL_TOKENS or not tokens:
        return Ok(_SelectionPlan(corpora=_SCENARIO_CORPORA, selection_json='{"$type":"all"}'))
    all_themes = frozenset(theme for corpus in _SCENARIO_CORPORA for theme in corpus.themes)
    token_set = frozenset(tokens)
    if token_set <= all_themes:
        return Ok(
            _SelectionPlan(
                corpora=tuple(corpus for corpus in _SCENARIO_CORPORA if corpus.themes & token_set),
                selection_json=_selection("themes", "themes", tokens),
            )
        )
    path_corpora = tuple(corpus for corpus in _SCENARIO_CORPORA if any(_matches_corpus(token, corpus) for token in tokens))
    resolved = tuple(
        name for token in tokens if (name := _scenario_name(token)) in frozenset(name for corpus in _SCENARIO_CORPORA for name in corpus.names)
    )
    matches = tuple(name for corpus in _SCENARIO_CORPORA for name in corpus.names if any(_matches(token, name) for token in tokens))
    selected = tuple(
        dict.fromkeys((
            *resolved,
            *matches,
            *((f"{theme}.{name}" for corpus in path_corpora for theme, name in corpus.theme_names) if resolved or matches else ()),
        ))
    )
    corpora = tuple(corpus for corpus in _SCENARIO_CORPORA if corpus.names & frozenset(selected) or corpus in path_corpora)
    if selected and corpora:
        return Ok(_SelectionPlan(corpora=corpora, selection_json=_selection("names", "names", selected)))
    if corpora:
        selected_themes = tuple(theme for corpus in corpora for theme, _ in corpus.theme_names)
        return Ok(_SelectionPlan(corpora=corpora, selection_json=_selection("themes", "themes", selected_themes)))
    return Error(Fault(("bridge", "verify", pattern), RailStatus.UNSUPPORTED, f"no typed bridge scenario matched pattern: {pattern!r}"))


def _matches(token: str, value: str) -> bool:
    bare = value.rsplit(".", 1)[-1]
    return token in {value, bare} or (any(glyph in token for glyph in _PATH_GLYPHS) and fnmatchcase(value, token))


def _matches_corpus(token: str, corpus: _ScenarioCorpus) -> bool:
    parent = Path(corpus.project).parent.as_posix()
    paths = (corpus.project, parent, *corpus.paths)
    return token in paths or (any(glyph in token for glyph in _PATH_GLYPHS) and any(fnmatchcase(path, token) for path in paths))


def _scenario_name(token: str) -> str:
    return token if "." in token else next((name for corpus in _SCENARIO_CORPORA for name in corpus.names if name.endswith("." + token)), token)


def _selection(kind: str, field: str, values: tuple[str, ...]) -> str:
    return msgspec.json.encode({"$type": kind, field: list(dict.fromkeys(values))}).decode()


def _closure_index(scope: ArtifactScope) -> Result[dict[str, tuple[Path, _ClosureManifest]], Fault]:
    root = Path(scope.path)
    try:
        manifests = tuple(sorted(root.rglob(_CLOSURE_FILE)))
    except OSError as exc:
        return Error(Fault(("bridge", "closure-index"), RailStatus.FAULTED, str(exc)[:1024]))
    rows: dict[str, tuple[Path, _ClosureManifest]] = {}
    for path in manifests:
        decoded = _read_closure(path)
        for corpus in _SCENARIO_CORPORA:
            if corpus.assembly in {Path(assembly).name for assembly in decoded.assemblies}:
                rows[corpus.assembly] = (path, decoded)
    return Ok(rows)


def _read_closure(path: Path) -> _ClosureManifest:
    try:
        return _CLOSURE_DECODER.decode(path.read_bytes())
    except OSError, msgspec.MsgspecError:
        return _ClosureManifest()


def _aggregate_closure(settings: AssaySettings, scope: ArtifactScope, plan: _SelectionPlan, index: dict[str, tuple[Path, _ClosureManifest]]) -> Result[Path, Fault]:
    missing = tuple(corpus.assembly for corpus in plan.corpora if corpus.assembly not in index)
    if missing:
        return Error(Fault(("bridge", "closure-index"), RailStatus.FAULTED, f"missing bridge closure(s): {', '.join(missing)}"))
    try:
        target = Path(scope.ensure()) / _AGGREGATE_CLOSURE_FILE
        selected = tuple(index[corpus.assembly] for corpus in plan.corpora)
        cargo_root = Path(scope.path) / "bin" / "Cargo" / settings.configuration.value.lower()
        cargo_assemblies = tuple(sorted(cargo_root.glob("*.dll")))
        if not cargo_assemblies:
            return Error(Fault(("bridge", "closure-aggregate"), RailStatus.FAULTED, f"missing cargo output assemblies under {cargo_root}"))
        payload = _ClosureManifest(
            assemblies=tuple(
                sorted({
                    str((path.parent / assembly).resolve()) if not Path(assembly).is_absolute() else str(Path(assembly).resolve())
                    for path, closure in selected
                    for assembly in closure.assemblies
                } | {str(path.resolve()) for path in cargo_assemblies})
            ),
            host_plugins=tuple(sorted({plugin for _, closure in selected for plugin in closure.host_plugins})),
            built_against=next((closure.built_against for _, closure in selected if closure.built_against != _HostFingerprint()), _HostFingerprint()),
        )
        target.write_bytes(msgspec.json.encode(payload))
        return Ok(target)
    except OSError as exc:
        return Error(Fault(("bridge", "closure-aggregate"), RailStatus.FAULTED, str(exc)[:1024]))


def _expire_stale(report_root: Path, ttl_s: float) -> None:
    try:
        if not report_root.is_dir():
            return
        cutoff = time.time() - ttl_s
        _ = tuple(shutil.rmtree(child, ignore_errors=True) for child in report_root.iterdir() if child.is_dir() and child.stat().st_mtime <= cutoff)
    except OSError:
        return


def _scenario_artifacts(report_dir: Path) -> tuple[Artifact, ...]:
    try:
        paths = tuple(sorted(path for path in report_dir.rglob("*") if path.is_file() and path.suffix in _BRIDGE_ARTIFACT_SUFFIXES))
    except OSError:
        return ()
    return tuple(
        Artifact(
            id=path.stem,
            kind=ArtifactKind.RHINO,
            path=str(path),
            bytes=_size(path),
            lines=_lines(path) if path.suffix in _TEXT_ARTIFACT_SUFFIXES else 0,
        )
        for path in paths
    )


def _size(path: Path) -> int:
    try:
        return path.stat().st_size
    except OSError:
        return 0


def _lines(path: Path) -> int:
    try:
        return len(path.read_text(encoding="utf-8", errors="replace").splitlines())
    except OSError, UnicodeError:
        return 0


def _sarif_artifacts(scope: ArtifactScope) -> tuple[Artifact, ...]:
    try:
        paths = tuple(sorted(Path(scope.path).rglob("*.sarif")))
    except OSError:
        return ()
    return tuple(
        Artifact(
            id=path.stem,
            kind=ArtifactKind.PROCESS,
            path=str(path),
            bytes=_size(path),
            lines=_lines(path),
        )
        for path in paths
    )


def _build_project(settings: AssaySettings, scope: ArtifactScope, project: str) -> Result[Completed, Fault]:
    tool = msgspec.structs.replace(
        _BUILD_TOOL,
        command=(*_BUILD_TOOL.command, "--configuration", settings.configuration.value, str(settings.root / project)),
    )
    return run_check(Check(tool=tool, cwd=Path(str(settings.root))), settings=settings, scope=scope, routed=_routed(), deadline=None)


def _first_diagnostic(rows: tuple[Completed, ...]) -> str:
    return next(
        (
            text[:256]
            for row in rows
            for text in (row.stdout + b"\n" + row.stderr).decode(errors="replace").splitlines()
            if text.strip() and ("): error " in text or ": error " in text or " error " in text)
        ),
        "",
    )


def _build_detail(done: Completed) -> Diagnostic | None:
    first = _first_diagnostic((done,))
    return Diagnostic(failing_step="bridge.build", recent_events=(first,), hint=first) if first else None


def _build_ready(done: Completed) -> bool:
    return done.status.severity <= RailStatus.OK.severity


def _build_closure(settings: AssaySettings) -> Result[Completed, Fault]:
    def locked(_held: object) -> Result[Completed, Fault]:
        scope = ArtifactScope.build(settings, "bridge")
        rows: list[Completed] = []
        for project in _BUILD_PROJECTS:
            match _build_project(settings, scope, project):
                case Result(tag="ok", ok=done):
                    rows.append(done)
                    if not _build_ready(done):
                        break
                case Result(error=fault):
                    return Error(fault)
        status = status_fold(*(row.status for row in rows))
        return Ok(
            receipt(
                ("rasm-bridge-build",),
                status.exit_code,
                stdout=b"\n".join(row.stdout for row in rows if row.stdout),
                stderr=b"\n".join(row.stderr for row in rows if row.stderr),
                status=status,
                duration_ms=sum(row.duration_ms for row in rows),
                notes=(
                    *tuple(note for row in rows for note in row.notes),
                    *tuple(f"build.{Path(project).name}={row.status.value}" for project, row in zip(_BUILD_PROJECTS, rows, strict=False)),
                    *((f"bridge.firstDiagnostic={first}",) if (first := _first_diagnostic(tuple(rows))) else ()),
                ),
                artifacts=(
                    *tuple(artifact for row in rows for artifact in row.artifacts),
                    *_sarif_artifacts(scope),
                ),
            )
        )

    return leased(f"build-bridge-{settings.configuration.value}", locked, settings=settings, run_id=settings.run_id, project="bridge")


# --- [COMPOSITION] ----------------------------------------------------------------------


def bridge_lease[T](settings: AssaySettings, action: Callable[[], Result[T, Fault]]) -> Result[T, Fault]:
    """Serialize an action through the process-global Rhino bridge lease.

    One RhinoWIP.app exists per machine, so every supervisor, verify, and package
    lifecycle command acquires the single ``bridge`` resource before touching the host.

    Returns:
        Action result when the lease is held, or a busy/fault result otherwise.
    """
    return leased("bridge", lambda _held: action(), settings=settings, run_id=settings.run_id, project="bridge")


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams) -> Result[Report, Fault]:
    """Verify typed bridge scenarios under the live Rhino lease.

    Returns:
        Verification report, or a lease/build/launch fault.
    """
    argv = ("bridge", "verify", params.pattern)
    return bridge_lease(settings, lambda: _verify_locked(settings, scope, params, argv))


def _verify_locked(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...]) -> Result[Report, Fault]:
    _ = scope
    build_scope = ArtifactScope.build(settings, "bridge")
    _expire_stale(Path(str(settings.root)).joinpath(*_BRIDGE_REPORT_ROOT), _VERIFY_TTL_S)
    prelude = (
        _build_closure(settings)
        .bind(lambda built: _plan(params.pattern) if _build_ready(built) else Error(Fault(built.argv, built.status, _first_diagnostic((built,)) or "bridge build failed")))
        .bind(
            lambda plan: _closure_index(build_scope).bind(
                lambda index: _aggregate_closure(settings, build_scope, plan, index).map(lambda closure: (plan, closure))
            )
        )
    )
    match prelude:
        case Result(tag="ok", ok=(plan, closure)):
            return _fold_session(settings, plan, closure, argv)
        case Result(error=fault):
            return Error(fault)


def _fold_session(settings: AssaySettings, plan: _SelectionPlan, closure: Path, argv: tuple[str, ...]) -> Result[Report, Fault]:
    outcome = client_run(settings, "verify", plan.selection_json, str(closure), timeout=_SCENARIO_TIMEOUT_S)
    match outcome:
        case Result(tag="ok", ok=done):
            envelope = _decode_envelope(done)
            phase, output = first_fault(envelope)
            summary = VerifySummary(
                exceptions=sum(1 for row in envelope.scenarios if row.fault is not None),
                report_dir=envelope.report_dir,
                first_failure=next((row.scenario for row in envelope.scenarios if row.status.exit_code != 0), ""),
                first_fault_phase=phase,
                first_fault_output=output,
                facts=tuple(_fact_row(evt) for evt in envelope.evidence if evt.kind == "fact"),
                captures=tuple(_capture_row(evt) for evt in envelope.evidence if evt.kind == "capture"),
            )
            return Ok(fold(Claim.BRIDGE, "verify", (msgspec.structs.replace(done, argv=argv),), detail=summary))
        case Result(error=fault):
            return Error(fault)


def _fact_row(evt: _SessionEvidence) -> tuple[str, str]:
    return (evt.stamp.scenario or evt.key, msgspec.json.encode({"key": evt.key, "value": evt.value}).decode())


def _capture_row(evt: _SessionEvidence) -> tuple[str, str]:
    return (
        evt.stamp.scenario or Path(evt.path).stem,
        msgspec.json.encode({"path": evt.path, "width": evt.width, "height": evt.height, "onFailure": evt.on_failure}).decode(),
    )


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
    """Build the bridge supervisor, shell, stub, cargo, and typed scenario closures.

    Returns:
        Bridge build report or build fault.
    """
    _ = (scope, params)
    return _build_closure(settings).map(
        lambda done: fold(
            Claim.BRIDGE,
            "build",
            (done,),
            detail=_build_detail(done),
            sarif_dir=str(ArtifactScope.build(settings, "bridge").path),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "bridge_lease", "build", "check", "clean", "client_run", "doctor", "first_fault", "launch", "quit", "verify"]
