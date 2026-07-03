"""Run live Rhino bridge supervisor lifecycle and scenario verification rails."""

from collections.abc import Callable  # beartype resolves the public bridge_lease signature at runtime under PEP 649
from dataclasses import dataclass
import hashlib
from pathlib import Path, PurePosixPath
import time
from typing import ClassVar, Final, Literal, override

from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # beartype resolves public rail annotations at runtime
from tools.assay.composition.store import ArtifactScope  # beartype resolves public rail annotations at runtime
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.govern import leased
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    BridgeLifecycle,
    Check,
    Claim,
    Completed,  # noqa: TC001  # beartype resolves Result[Completed, Fault] forward-ref at runtime under PEP 649
    Diagnostic,
    Fault,
    Language,
    Match,
    Mode,
    RailStatus,
    receipt,
    Report,  # noqa: TC001  # beartype resolves Result[Report, Fault] forward-ref at runtime under PEP 649
    Tool,
    ToolArgs,
    VerifySummary,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.diagnostics import fold


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUPERVISOR_PROJECT: Final[str] = "tools/rhino-bridge/Supervisor/Supervisor.csproj"
_STUB_PROJECT: Final[str] = "tools/rhino-bridge/Stub/Stub.csproj"
_SHELL_PROJECT: Final[str] = "tools/rhino-bridge/Shell/Shell.csproj"
_CARGO_PROJECT: Final[str] = "tools/rhino-bridge/Cargo/Cargo.csproj"
_CONTRACT_PROJECT: Final[str] = "tools/rhino-bridge/Contract/Contract.csproj"
# Single scenario home: one project, one assembly, one reference root; the in-host shell owns per-scenario resolution.
_SCENARIO_PROJECT: Final[str] = "tests/csharp/scenarios/Rasm.Scenarios.csproj"
_SCENARIO_ASSEMBLY: Final[str] = "Rasm.Scenarios.dll"
_REFERENCE_ROOT: Final[str] = "tests/csharp/scenarios/_references"
_BUILD_PROJECTS: Final[tuple[str, ...]] = (_CONTRACT_PROJECT, _CARGO_PROJECT, _SHELL_PROJECT, _STUB_PROJECT, _SUPERVISOR_PROJECT, _SCENARIO_PROJECT)
_CLOSURE_FILE: Final[str] = "bridge-closure.json"
_AGGREGATE_CLOSURE_FILE: Final[str] = "bridge-closure.assay.json"
_CERTIFICATE_FILE: Final[str] = "bridge-certificate.json"
_SCENARIO_TIMEOUT_S: Final[float] = 600.0
_ALL_TOKENS: Final[frozenset[str]] = frozenset(("", "all", "*"))
_EMPTY_CORPUS_NOTE: Final[str] = f"bridge.corpus=empty: scenario corpus empty under {Path(_SCENARIO_PROJECT).parent.as_posix()}; nothing to verify"
_TEXT_ARTIFACT_SUFFIXES: Final[frozenset[str]] = frozenset((".json", ".jsonl", ".log", ".txt"))
_SHELL_SOURCE_DIRS: Final[tuple[str, ...]] = ("Shell", "Contract", "Cargo")
# Mirrors the supervisor's BundleInfo.RhinoLineMajor anchor; the installed-plugin probe derives its packages segment from it.
RHINO_LINE_MAJOR: Final[int] = 9
_INSTALLED_PLUGIN_GLOB: Final[str] = (
    f"Library/Application Support/McNeel/Rhinoceros/packages/{RHINO_LINE_MAJOR}.0/rasm-bridge/*/Rasm.Bridge.Shell.dll"
)
_FRESHNESS_STALE: Final[str] = (
    "bridge.freshness=stale: shell source newer than the installed plugin; run `assay package publish --slug rasm-bridge --version <v>`"
)
_FRESHNESS_ABSENT: Final[str] = (
    "bridge.freshness=absent: rasm-bridge plugin not installed; run `assay package publish --slug rasm-bridge --version <v>`"
)
# Not-yet-certified reference lane: unpromoted-only problem rows degrade, mirroring the supervisor's exit-2 fold; mixed rows stay faulted.
_REFERENCE_UNPROMOTED_PREFIX: Final[str] = "reference.unpromoted:"
type _CountRow[T] = tuple[str, Callable[[T], int]]


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class BridgeParams(BaseParams):
    """Parameters shared by bridge verbs."""

    SLOTS: ClassVar[dict[str, str]] = {"": "", "verify": "[PATTERN]"}

    evidence: Literal["verify", "author"] = "verify"

    @override
    def _arity(self, verb: str) -> int:
        return 1 if verb == "verify" else 0

    @property
    def pattern(self) -> str:
        """Scenario selection token; empty selects every typed scenario corpus."""
        return self.paths[0] if self.paths else ""


class _ReferenceRoot(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    assembly: str = ""
    theme: str = ""
    path: str = ""


class _HostFingerprint(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    bundle_version: str = ""
    rhino_common_version: str = ""
    grasshopper2_version: str = ""
    runtime_version: str = ""


class _CapabilityEntry(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    key: str = ""
    outcome: str = ""  # PhaseStatus key token: ok / skipped / unsupported / failed / timeout / busy
    receipt: str = ""


class _ClosureManifest(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    assemblies: tuple[str, ...] = ()
    scenario_assemblies: tuple[str, ...] = ()
    host_plugins: tuple[str, ...] = ()
    built_against: _HostFingerprint = msgspec.field(default_factory=_HostFingerprint)
    reference_roots: tuple[_ReferenceRoot, ...] = ()


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
    scenario_status: RailStatus | None = None
    duration_ms: float = 0.0
    fault: _SessionFault | None = None
    reference_results: tuple[object, ...] | None = None
    first_scenario_failure: str | None = None


class _SessionEvidence(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    kind: str = msgspec.field(default="", name="$type")
    stamp: _EventStamp = msgspec.field(default_factory=_EventStamp)
    key: str = ""
    value: object = None
    path: str = ""
    width: int = 0
    height: int = 0
    on_failure: bool = False


class _ArtifactHash(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    algorithm: str = ""
    value: str = ""


class _ArtifactRef(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    id: str = ""
    role: str = ""
    relative_path: str = ""
    media_type: str = ""
    bytes: int = 0
    hash: _ArtifactHash = msgspec.field(default_factory=_ArtifactHash)
    retention: str = ""
    scenario: str = ""
    on_failure: bool = False


class _EvidenceCounts(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    facts: int = 0
    assertions: int = 0
    references: int = 0
    reference_matches: int = 0
    reference_failures: int = 0
    captures: int = 0
    artifacts: int = 0
    object_manifests: int = 0
    geometry_manifests: int = 0
    viewport_manifests: int = 0
    gh2_canvas_manifests: int = 0
    scratch_manifests: int = 0


class _ScenarioCounts(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    total: int = 0
    ok: int = 0
    failed: int = 0
    skipped: int = 0
    unsupported: int = 0
    timeout: int = 0
    busy: int = 0
    degraded: int = 0


class _PhaseReceipt(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    phase: str = ""
    status: RailStatus = RailStatus.EMPTY


class _SpoolSummary(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    durable_events: int = 0
    relayed_events: int = 0
    last_sequence: int = 0
    diverged: bool = False
    failures: int = 0


class _ReferenceEvidenceResult(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    scenario: str = ""
    name: dict[str, object] | None = None
    admission: str = ""
    matched: bool = False
    reference_path: str = ""
    detail: str = ""


class _EvidenceCertificate(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    run_id: str = ""
    scenario: str = ""
    counts: _EvidenceCounts = msgspec.field(default_factory=_EvidenceCounts)
    artifacts: tuple[_ArtifactRef, ...] = ()
    references: tuple[_ReferenceEvidenceResult, ...] = ()


@dataclass(frozen=True, slots=True)
class _EvidenceProjection:
    freshness: str
    certificate: _EvidenceCertificate | None
    certificate_problems: tuple[str, ...]
    reference_problems: tuple[str, ...]
    status: RailStatus


class _SessionEnvelope(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, rename="camel"):
    run_id: str = ""
    verb: str = ""
    status: RailStatus = RailStatus.FAILED
    scenario_status: RailStatus = RailStatus.EMPTY
    session_status: RailStatus = RailStatus.EMPTY
    duration_ms: float = 0.0
    report_dir: str = ""
    host: _HostFingerprint = msgspec.field(default_factory=_HostFingerprint)
    capabilities: tuple[_CapabilityEntry, ...] = ()
    scenarios: tuple[_SessionScenario, ...] = ()
    evidence: tuple[_SessionEvidence, ...] = ()
    first_failure: str = ""
    first_scenario_failure: str = ""
    first_session_fault: str = ""
    first_fault_phase: str | None = None
    fault: _SessionFault | None = None
    phase_receipts: tuple[_PhaseReceipt, ...] = ()
    certificate_path: str = ""
    artifact_refs: tuple[_ArtifactRef, ...] = ()
    evidence_counts: _EvidenceCounts = msgspec.field(default_factory=_EvidenceCounts)
    scenario_counts: _ScenarioCounts = msgspec.field(default_factory=_ScenarioCounts)
    spool: _SpoolSummary = msgspec.field(default_factory=_SpoolSummary)


# --- [TABLES] ---------------------------------------------------------------------------

_ENVELOPE_DECODER: Final[msgspec.json.Decoder[_SessionEnvelope]] = msgspec.json.Decoder(_SessionEnvelope, strict=False)
_CLOSURE_DECODER: Final[msgspec.json.Decoder[_ClosureManifest]] = msgspec.json.Decoder(_ClosureManifest, strict=False)
_CERTIFICATE_DECODER: Final[msgspec.json.Decoder[_EvidenceCertificate]] = msgspec.json.Decoder(_EvidenceCertificate, strict=False)
_SCENARIO_COUNT_ROWS: Final[tuple[_CountRow[_ScenarioCounts], ...]] = (
    ("total", lambda counts: counts.total),
    ("ok", lambda counts: counts.ok),
    ("failed", lambda counts: counts.failed),
    ("skipped", lambda counts: counts.skipped),
    ("unsupported", lambda counts: counts.unsupported),
    ("timeout", lambda counts: counts.timeout),
    ("busy", lambda counts: counts.busy),
    ("degraded", lambda counts: counts.degraded),
)
_EVIDENCE_COUNT_ROWS: Final[tuple[_CountRow[_EvidenceCounts], ...]] = (
    ("facts", lambda counts: counts.facts),
    ("assertions", lambda counts: counts.assertions),
    ("references", lambda counts: counts.references),
    ("captures", lambda counts: counts.captures),
    ("artifacts", lambda counts: counts.artifacts),
    ("objectManifests", lambda counts: counts.object_manifests),
    ("geometryManifests", lambda counts: counts.geometry_manifests),
    ("viewportManifests", lambda counts: counts.viewport_manifests),
    ("gh2CanvasManifests", lambda counts: counts.gh2_canvas_manifests),
    ("scratchManifests", lambda counts: counts.scratch_manifests),
)

_SUPERVISOR_BINARY: Final[str] = "Rasm.Bridge.Supervisor"


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bridge_row(mode: Mode) -> Result[Tool, Fault]:
    match next((t for t in select(Claim.BRIDGE, Language.CSHARP) if t.mode is mode), None):
        case Tool() as tool:
            return Ok(tool)
        case _:
            return Error(Fault(("bridge", mode.value), message=f"no Claim.BRIDGE catalog row for mode {mode.value}"))


def _routed() -> Routed:
    return Routed(language=Language.CSHARP, scope=Scope.CHANGED)


def _pivot_output(scope_path: object, project: str, configuration: str) -> Path | None:
    # dotnet artifacts pivots are `<config>` or `<config>_<rid>`; the newest matching pivot is the bound output.
    root = Path(str(scope_path)) / "bin" / project
    return next(iter(sorted(root.glob(f"{configuration.lower()}*"), key=lambda p: p.stat().st_mtime, reverse=True)), None)


def _supervisor_binary(settings: AssaySettings) -> Path | None:
    # dotnet run recomposes artifacts-path output locations by SDK heuristics that drift per release;
    # the built apphost under the stable bridge scope is the one spawn target that cannot drift.
    pivot = _pivot_output(ArtifactScope.build(settings, "bridge").path, "Supervisor", settings.configuration.value)
    binary = pivot / _SUPERVISOR_BINARY if pivot is not None else None
    return binary if binary is not None and binary.is_file() else None


def _client_check(settings: AssaySettings, *args: str) -> Result[Check, Fault]:
    verb = args[0] if args else "status"
    match _supervisor_binary(settings):
        case None:
            return Error(
                Fault(
                    ("bridge", "supervisor"),
                    status=RailStatus.FAULTED,
                    message="supervisor binary absent under the bridge build scope; run `uv run python -m tools.assay bridge build` first",
                )
            )
        case binary:
            args_slice = ToolArgs(binary=str(binary), verb=verb, argv=args[1:])
            return _bridge_row(Mode.VERIFY).map(lambda tool: Check(tool=tool, cwd=Path(str(settings.root)), args=args_slice))


def client_run(settings: AssaySettings, *args: str, executor: Executor, timeout: float | None = None) -> Result[Completed, Fault]:
    """Run the bridge supervisor with verb arguments and an optional wall-clock deadline.

    Returns:
        Supervisor completion or operational fault.
    """
    deadline = time.monotonic() + timeout if timeout is not None else None
    scope = ArtifactScope.build(settings, "bridge")
    return (
        _client_check(settings, *args)
        .bind(lambda check: executor.run(check, settings=settings, scope=scope, routed=_routed(), deadline=deadline))
        .map(_completed_from_stdout)
    )


def first_fault(envelope: _SessionEnvelope) -> tuple[str, str]:
    """Extract the first supervisor fault phase and bounded message.

    Returns:
        Fault phase and bounded text, or empty strings when no fault exists.
    """
    message = envelope.first_session_fault or envelope.first_failure
    return ((envelope.first_fault_phase or "session"), message[:256]) if message else ("", "")


def _decode_envelope(run: Completed) -> _SessionEnvelope:
    raw = run.stdout.strip()
    failures: list[str] = []
    if raw:
        try:
            return _ENVELOPE_DECODER.decode(raw)
        except msgspec.MsgspecError as exc:
            failures.append(f"stdout: {str(exc)[:120]}")
    paths = tuple(
        Path(artifact.path) for artifact in run.artifacts if artifact.kind is ArtifactKind.PROCESS and Path(artifact.path).name == "out.log"
    )
    for path in paths:
        try:
            payload = path.read_bytes().strip()
            if payload:
                return _ENVELOPE_DECODER.decode(payload)
            failures.append(f"{path}: empty")
        except (OSError, msgspec.MsgspecError) as exc:
            failures.append(f"{path}: {str(exc)[:120]}")
    text = (run.stderr or run.stdout).decode(errors="replace").strip()
    return _SessionEnvelope(
        status=RailStatus.FAILED, first_failure=text[:256] or "; ".join(failures)[:256] or "supervisor emitted no SessionEnvelope"
    )


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


def _selection(pattern: str) -> str:
    """Project the verify pattern into the in-host selection payload.

    Pass-through by design: the token set routes whole — any dotted token (a full scenario name or dotted
    glob) selects the ``names`` arm for every token, otherwise all tokens are themes — and the in-host
    shell owns name/glob resolution with its own typed zero-match fault; no local roster validation.

    Returns:
        Selection JSON: ``all`` for empty/``all``/``*`` token sets, else one ``names`` or ``themes`` payload.
    """
    tokens = tuple(dict.fromkeys(part.strip() for part in pattern.split(",") if part.strip()))
    if frozenset(tokens) <= _ALL_TOKENS:
        return '{"$type":"all"}'
    kind = "names" if any("." in token for token in tokens) else "themes"
    return msgspec.json.encode({"$type": kind, kind: list(tokens)}).decode()


def _corpus_sources(settings: AssaySettings) -> tuple[Path, ...]:
    """Roster the scenario sources under the single scenario home.

    Returns:
        Every non-generated ``*.cs`` under the scenarios project directory; empty means nothing to verify.
    """
    root = settings.root / PurePosixPath(_SCENARIO_PROJECT).parent.as_posix()
    try:
        return tuple(sorted(Path(str(path)) for path in root.rglob("*.cs") if not {"bin", "obj"} & set(Path(str(path)).parts)))
    except OSError:
        return ()


def _scenario_closure(scope: ArtifactScope) -> Result[tuple[Path, _ClosureManifest], Fault]:
    # One scenario assembly, one manifest: the first closure naming Rasm.Scenarios.dll is the bound manifest.
    root = Path(scope.path)
    try:
        manifests = tuple(sorted(root.rglob(_CLOSURE_FILE)))
    except OSError as exc:
        return Error(Fault(("bridge", "closure-index"), RailStatus.FAULTED, str(exc)[:1024]))
    for path in manifests:
        decoded = _read_closure(path)
        if _SCENARIO_ASSEMBLY in {Path(assembly).name for assembly in decoded.assemblies}:
            return Ok((path, decoded))
    return Error(Fault(("bridge", "closure-index"), RailStatus.FAULTED, f"missing bridge closure: {_SCENARIO_ASSEMBLY}"))


def _read_closure(path: Path) -> _ClosureManifest:
    try:
        return _CLOSURE_DECODER.decode(path.read_bytes())
    except OSError, msgspec.MsgspecError:
        return _ClosureManifest()


def _aggregate_closure(settings: AssaySettings, scope: ArtifactScope, closure: tuple[Path, _ClosureManifest]) -> Result[Path, Fault]:
    path, manifest = closure
    target = Path(scope.ensure()) / _AGGREGATE_CLOSURE_FILE
    cargo_fallback = Path(scope.path) / "bin" / "Cargo" / settings.configuration.value.lower()
    cargo_root = _pivot_output(scope.path, "Cargo", settings.configuration.value) or cargo_fallback
    cargo_assemblies = tuple(sorted(cargo_root.glob("*.dll")))
    if not cargo_assemblies:
        return Error(Fault(("bridge", "closure-aggregate"), RailStatus.FAULTED, f"missing cargo output assemblies under {cargo_root}"))
    payload = _ClosureManifest(
        assemblies=tuple(
            sorted(
                {
                    str((path.parent / assembly).resolve()) if not Path(assembly).is_absolute() else str(Path(assembly).resolve())
                    for assembly in manifest.assemblies
                }
                | {str(dll.resolve()) for dll in cargo_assemblies}
            )
        ),
        scenario_assemblies=(_SCENARIO_ASSEMBLY,),
        host_plugins=tuple(sorted(manifest.host_plugins)),
        built_against=manifest.built_against,
        reference_roots=(_ReferenceRoot(assembly=_SCENARIO_ASSEMBLY, path=str((settings.root / _REFERENCE_ROOT).resolve())),),
    )
    try:
        target.write_bytes(msgspec.json.encode(payload))
        return Ok(target)
    except OSError as exc:
        return Error(Fault(("bridge", "closure-aggregate"), RailStatus.FAULTED, str(exc)[:1024]))


def _scenario_artifacts(report_dir: Path) -> tuple[Artifact, ...]:
    certificate, problems = _read_certificate_path(report_dir / _CERTIFICATE_FILE)
    if certificate is None or problems:
        return ()
    return tuple(
        Artifact(
            id=ref.id,
            kind=ArtifactKind.RHINO,
            path=str(path),
            bytes=ref.bytes or _size(path),
            lines=_lines(path) if path.suffix in _TEXT_ARTIFACT_SUFFIXES else 0,
        )
        for ref in certificate.artifacts
        for path, problem in (_admit_artifact(report_dir, ref),)
        if path is not None and not problem
    )


def _certificate_path(envelope: _SessionEnvelope) -> Path | None:
    if envelope.certificate_path:
        path = Path(envelope.certificate_path)
        return path if path.is_absolute() or not envelope.report_dir else Path(envelope.report_dir) / path
    return Path(envelope.report_dir) / _CERTIFICATE_FILE if envelope.report_dir else None


def _read_certificate(envelope: _SessionEnvelope) -> tuple[_EvidenceCertificate | None, tuple[str, ...]]:
    path = _certificate_path(envelope)
    if path is None:
        return None, ("certificate.missing",)
    certificate, problems = _read_certificate_path(path)
    if certificate is None:
        return None, problems
    report_dir = Path(envelope.report_dir) if envelope.report_dir else path.parent
    artifact_problems = tuple(problem for ref in certificate.artifacts if (problem := _admit_artifact(report_dir, ref)[1]))
    return certificate, (*problems, *artifact_problems)


def _read_certificate_path(path: Path) -> tuple[_EvidenceCertificate | None, tuple[str, ...]]:
    try:
        return _CERTIFICATE_DECODER.decode(path.read_bytes()), ()
    except FileNotFoundError:
        return None, ("certificate.missing",)
    except (OSError, msgspec.MsgspecError) as exc:
        return None, (f"certificate.decode:{str(exc)[:160]}",)


def _admit_artifact(report_dir: Path, ref: _ArtifactRef) -> tuple[Path | None, str]:
    path, label, problem = _artifact_path(report_dir, ref)
    if problem or path is None:
        return path, problem
    if not path.is_file():
        return path, f"artifact.missing:{label}"
    algorithm = ref.hash.algorithm.lower()
    expected = ref.hash.value.lower()
    if algorithm != "sha256" or len(expected) != 64 or any(char not in "0123456789abcdef" for char in expected):
        return path, f"artifact.hash.invalid:{label}"
    try:
        size = path.stat().st_size
        with path.open("rb") as payload:
            actual = hashlib.file_digest(payload, "sha256").hexdigest()
    except OSError as exc:
        return path, f"artifact.read:{label}:{str(exc)[:80]}"
    if ref.bytes and size != ref.bytes:
        return path, f"artifact.bytes:{label}"
    return (path, "") if actual == expected else (path, f"artifact.hash.mismatch:{label}")


def _artifact_path(report_dir: Path, ref: _ArtifactRef) -> tuple[Path | None, str, str]:
    label = ref.id or ref.relative_path
    if not ref.relative_path:
        return None, label, "artifact.path.invalid"
    relative = PurePosixPath(ref.relative_path)
    if relative.is_absolute() or ".." in relative.parts:
        return None, label, f"artifact.path.invalid:{label}"
    root = report_dir.resolve()
    path = root.joinpath(*relative.parts).resolve()
    try:
        path.relative_to(root)
    except ValueError:
        return None, label, f"artifact.path.invalid:{label}"
    return path, label, ""


def _reference_problems(certificate: _EvidenceCertificate | None) -> tuple[str, ...]:
    if certificate is None:
        return ("certificate.missing",)
    if not certificate.references:
        return ("reference.missing",)
    return tuple(
        f"reference.{row.admission or 'unknown'}:{row.scenario or row.detail}"
        for row in certificate.references
        if not (row.matched and row.admission == "matched")
    )


def _reference_count_rows(certificate: _EvidenceCertificate | None) -> tuple[tuple[str, int], ...]:
    if certificate is None:
        return (("total", 0), ("matched", 0), ("failed", 0))
    matched = sum(1 for row in certificate.references if row.matched and row.admission == "matched")
    return (("total", len(certificate.references)), ("matched", matched), ("failed", len(certificate.references) - matched))


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
    return tuple(Artifact(id=path.stem, kind=ArtifactKind.PROCESS, path=str(path), bytes=_size(path), lines=_lines(path)) for path in paths)


def _build_project(settings: AssaySettings, scope: ArtifactScope, project: str, executor: Executor) -> Result[Completed, Fault]:
    args = ToolArgs(configuration=settings.configuration.value, project=str(settings.root / project))
    return _bridge_row(Mode.BUILD).bind(
        lambda tool: executor.run(
            Check(tool=tool, cwd=Path(str(settings.root)), args=args), settings=settings, scope=scope, routed=_routed(), deadline=None
        )
    )


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


def _build_closure(settings: AssaySettings, executor: Executor) -> Result[Completed, Fault]:
    def locked(_held: object) -> Result[Completed, Fault]:
        scope = ArtifactScope.build(settings, "bridge")
        rows: list[Completed] = []
        for project in _BUILD_PROJECTS:
            match _build_project(settings, scope, project, executor):
                case Result(tag="ok", ok=done):
                    row = msgspec.structs.replace(done, status=RailStatus.OK) if done.status is RailStatus.EMPTY and done.returncode == 0 else done
                    rows.append(row)
                    if not _build_ready(row):
                        break
                case Result(error=fault):
                    return Error(fault)
        status = RailStatus.fold(*(row.status for row in rows))
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
                artifacts=(*tuple(artifact for row in rows for artifact in row.artifacts), *_sarif_artifacts(scope)),
            )
        )

    return leased(f"build-bridge-{settings.configuration.value}", locked, settings=settings, run_id=settings.run_id, project="bridge")


# --- [COMPOSITION] ----------------------------------------------------------------------


def bridge_lease[T](settings: AssaySettings, action: Callable[[], Result[T, Fault]]) -> Result[T, Fault]:
    """Serialize live-host work through the process-global Rhino bridge lease.

    Returns:
        Action result, busy state, or lease fault.
    """
    return leased("bridge", lambda _held: action(), settings=settings, run_id=settings.run_id, project="bridge")


def verify(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, executor: Executor) -> Result[Report, Fault]:
    """Verify typed bridge scenarios under the live host lease.

    An empty scenario corpus short-circuits to a typed UNSUPPORTED receipt before any build or host launch.

    Returns:
        Verification report or setup/session fault.
    """
    argv = ("bridge", "verify", params.pattern, f"--evidence={params.evidence}")
    if not _corpus_sources(settings):
        empty = receipt(argv, RailStatus.UNSUPPORTED.exit_code, status=RailStatus.UNSUPPORTED, notes=(_EMPTY_CORPUS_NOTE,))
        return Ok(fold(Claim.BRIDGE, "verify", (empty,), promote_empty=True))
    return bridge_lease(settings, lambda: _verify_locked(settings, scope, params, argv, executor))


def _verify_locked(
    settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, argv: tuple[str, ...], executor: Executor
) -> Result[Report, Fault]:
    _ = scope
    build_scope = ArtifactScope.build(settings, "bridge")
    prelude = (
        _build_closure(settings, executor)
        .bind(
            lambda built: (
                _scenario_closure(build_scope)
                if _build_ready(built)
                else Error(Fault(built.argv, built.status, _first_diagnostic((built,)) or "bridge build failed"))
            )
        )
        .bind(lambda closure: _aggregate_closure(settings, build_scope, closure))
    )
    match prelude:
        case Result(tag="ok", ok=closure):
            return _fold_session(settings, _selection(params.pattern), closure, argv, evidence=params.evidence, executor=executor)
        case Result(error=fault):
            return Error(fault)


def _fold_session(
    settings: AssaySettings, selection: str, closure: Path, argv: tuple[str, ...], *, evidence: Literal["verify", "author"], executor: Executor
) -> Result[Report, Fault]:
    outcome = client_run(settings, "verify", selection, str(closure), evidence, executor=executor, timeout=_SCENARIO_TIMEOUT_S)
    match outcome:
        case Result(tag="ok", ok=done):
            envelope = _decode_envelope(done)
            projection = _evidence_projection(settings=settings, envelope=envelope, done=done.status, evidence=evidence)
            projected_done = _project_done(done=done, argv=argv, evidence=evidence, projection=projection)
            report = fold(Claim.BRIDGE, "verify", (projected_done,), detail=_verify_summary(envelope, projection), promote_empty=True)
            return Ok(msgspec.structs.replace(report, results=(*report.results, *_session_rows(envelope))))
        case Result(error=fault):
            return Error(fault)


def _evidence_projection(
    *, settings: AssaySettings, envelope: _SessionEnvelope, done: RailStatus, evidence: Literal["verify", "author"]
) -> _EvidenceProjection:
    certificate, certificate_problems = _read_certificate(envelope)
    reference_problems = _reference_problems(certificate)
    return _EvidenceProjection(
        freshness=_freshness(settings),
        certificate=certificate,
        certificate_problems=certificate_problems,
        reference_problems=reference_problems,
        status=_evidence_status(
            evidence=evidence, done=done, certificate_ok=certificate is not None and not certificate_problems, reference_problems=reference_problems
        ),
    )


def _project_done(*, done: Completed, argv: tuple[str, ...], evidence: Literal["verify", "author"], projection: _EvidenceProjection) -> Completed:
    certificate_ok = projection.certificate is not None and not projection.certificate_problems
    return msgspec.structs.replace(
        done,
        argv=argv,
        status=projection.status,
        notes=(
            *done.notes,
            *_freshness_note(projection.freshness),
            *(("bridge.evidence=author:candidate",) if evidence == "author" else ()),
            *(tuple(f"bridge.certificate={problem}" for problem in projection.certificate_problems) if evidence == "verify" else ()),
            *(tuple(f"bridge.reference={problem}" for problem in projection.reference_problems) if evidence == "verify" and certificate_ok else ()),
        ),
    )


def _verify_summary(envelope: _SessionEnvelope, projection: _EvidenceProjection) -> VerifySummary:
    phase, output = first_fault(envelope)
    return VerifySummary(
        exceptions=sum(1 for row in envelope.scenarios if row.fault is not None),
        scenario_status=envelope.scenario_status.value,
        session_status=envelope.session_status.value,
        report_dir=envelope.report_dir,
        freshness=projection.freshness,
        first_failure=envelope.first_failure,
        first_scenario_failure=envelope.first_scenario_failure,
        first_session_fault=envelope.first_session_fault,
        first_fault_phase=phase,
        first_fault_output=output,
        scenario_counts=_count_rows(envelope.scenario_counts, _SCENARIO_COUNT_ROWS),
        evidence_counts=_count_rows(envelope.evidence_counts, _EVIDENCE_COUNT_ROWS),
        reference_counts=_reference_count_rows(projection.certificate),
        artifact_count=len(projection.certificate.artifacts) if projection.certificate is not None else envelope.evidence_counts.artifacts,
        capture_count=envelope.evidence_counts.captures,
        manifest_count=_manifest_count(envelope.evidence_counts),
        certificate_path=envelope.certificate_path,
        phase_status=tuple((phase.phase, phase.status.value) for phase in envelope.phase_receipts),
        facts=tuple(_fact_row(evt) for evt in envelope.evidence if evt.kind == "fact"),
        captures=tuple(_capture_row(evt) for evt in envelope.evidence if evt.kind == "capture"),
    )


def _manifest_count(counts: _EvidenceCounts) -> int:
    return counts.object_manifests + counts.geometry_manifests + counts.viewport_manifests + counts.gh2_canvas_manifests + counts.scratch_manifests


def _evidence_status(
    *, evidence: Literal["verify", "author"], done: RailStatus, certificate_ok: bool, reference_problems: tuple[str, ...]
) -> RailStatus:
    # Unpromoted-only reference problems are honest not-yet-certified evidence: DEGRADED (exit 2), never OK, never FAULTED.
    # Any promoted-corpus problem (missing/mismatch/not-reviewed) faults, alone or mixed with unpromoted rows.
    if done.severity > RailStatus.OK.severity:
        return done
    if evidence == "author":
        return RailStatus.CANDIDATE
    if not certificate_ok:
        return RailStatus.FAULTED
    if not reference_problems:
        return RailStatus.OK
    return RailStatus.DEGRADED if all(problem.startswith(_REFERENCE_UNPROMOTED_PREFIX) for problem in reference_problems) else RailStatus.FAULTED


def _count_rows[T](counts: T, rows: tuple[_CountRow[T], ...]) -> tuple[tuple[str, int], ...]:
    return tuple((label, read(counts)) for label, read in rows)


def _session_rows(envelope: _SessionEnvelope) -> tuple[Match, ...]:
    session = Match(
        id="bridge.session",
        kind=ArtifactKind.RHINO,
        text=msgspec.json.encode({
            "status": envelope.status.value,
            "scenarioStatus": envelope.scenario_status.value,
            "sessionStatus": envelope.session_status.value,
            "certificatePath": envelope.certificate_path,
            "firstScenarioFailure": envelope.first_scenario_failure,
            "firstSessionFault": envelope.first_session_fault,
        }).decode(),
        severity=None if envelope.status.severity <= RailStatus.OK.severity else "failed",
    )
    scenarios = tuple(_scenario_row(row) for row in envelope.scenarios)
    return (session, *scenarios)


def _scenario_row(row: _SessionScenario) -> Match:
    status = row.scenario_status or row.status
    return Match(
        id=row.scenario or "bridge.scenario",
        kind=ArtifactKind.RHINO,
        text=msgspec.json.encode({
            "status": row.status.value,
            "scenarioStatus": status.value,
            "durationMs": row.duration_ms,
            "firstScenarioFailure": row.first_scenario_failure or "",
        }).decode(),
        severity=None if status.severity <= RailStatus.OK.severity else "failed",
    )


def _fact_row(evt: _SessionEvidence) -> tuple[str, str]:
    return (evt.stamp.scenario or evt.key, msgspec.json.encode({"key": evt.key, "value": evt.value}).decode())


def _capture_row(evt: _SessionEvidence) -> tuple[str, str]:
    return (
        evt.stamp.scenario or Path(evt.path).stem,
        msgspec.json.encode({"path": evt.path, "width": evt.width, "height": evt.height, "onFailure": evt.on_failure}).decode(),
    )


def _freshness(settings: AssaySettings) -> str:
    """Return installed shell freshness as report data, not a rail gate."""
    try:
        sources = [path for subdir in _SHELL_SOURCE_DIRS for path in (settings.root / "tools" / "rhino-bridge" / subdir).rglob("*.cs")]
        installed = list(Path.home().glob(_INSTALLED_PLUGIN_GLOB))
        source_mtime = max((path.stat().st_mtime for path in sources), default=0.0)
        plugin_mtime = max((path.stat().st_mtime for path in installed), default=0.0)
    except OSError:
        return "unknown"
    if not sources:
        return "unknown"
    if not installed:
        return "absent"
    return "stale" if source_mtime > plugin_mtime else "fresh"


def _freshness_note(state: str) -> tuple[str, ...]:
    return (_FRESHNESS_STALE,) if state == "stale" else (_FRESHNESS_ABSENT,) if state == "absent" else ()


def _host_rows(host: _HostFingerprint) -> tuple[tuple[str, str], ...]:
    return tuple(
        (axis, version)
        for axis, version in (
            ("bundle", host.bundle_version),
            ("rhinoCommon", host.rhino_common_version),
            ("grasshopper2", host.grasshopper2_version),
            ("runtime", host.runtime_version),
        )
        if version
    )


def _lifecycle(settings: AssaySettings, verb: str, *args: str, executor: Executor) -> Result[Report, Fault]:
    def _fold_lifecycle(done: Completed) -> Report:
        envelope = _decode_envelope(done)
        phase, output = first_fault(envelope)
        freshness = _freshness(settings) if verb == "status" else ""
        return fold(
            Claim.BRIDGE,
            verb,
            (msgspec.structs.replace(done, notes=(*done.notes, *_freshness_note(freshness))),),
            promote_empty=True,
            detail=BridgeLifecycle(
                verb=verb,
                report_dir=envelope.report_dir,
                freshness=freshness,
                host=_host_rows(envelope.host),
                capabilities=tuple((cap.key, cap.outcome, cap.receipt) for cap in envelope.capabilities),
                first_fault_phase=phase,
                first_fault_output=output,
            ),
        )

    return bridge_lease(settings, lambda: client_run(settings, verb, *args, executor=executor).map(_fold_lifecycle))


def status(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, executor: Executor) -> Result[Report, Fault]:
    """Run the bridge host health probe.

    Returns:
        Lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "status", executor=executor)


def quit(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, executor: Executor) -> Result[Report, Fault]:  # noqa: A001
    """Terminate the bridge host under the live host lease.

    Returns:
        Lifecycle report or operational fault.
    """
    _ = (scope, params)
    return _lifecycle(settings, "quit", executor=executor)


def build(settings: AssaySettings, scope: ArtifactScope, params: BridgeParams, executor: Executor) -> Result[Report, Fault]:
    """Build the bridge supervisor, shell, stub, cargo, and typed scenario closures.

    Returns:
        Build report or build fault.
    """
    _ = (scope, params)
    return _build_closure(settings, executor).map(
        lambda done: fold(
            Claim.BRIDGE,
            "build",
            (done,),
            detail=_build_detail(done),
            sarif_dir=str(ArtifactScope.build(settings, "bridge").path),
            promote_empty=True,
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BridgeParams", "bridge_lease", "build", "client_run", "first_fault", "quit", "status", "verify"]
