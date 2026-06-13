"""Assay domain model: axes, wire structs, evidence details, and fold operations.

Defines the complete msgspec wire contract for the Assay quality-gate harness.
Includes StrEnum axes (Claim, Language, Mode, Runner, Input) that drive tool
dispatch, a frozen msgspec.Struct hierarchy (Base -> Detail -> typed Detail
subtypes) for serialized evidence, and the three primary fold operations that
collapse process outcomes into Reports, Envelopes, and CLI exit codes.

The module-level encoder (_ENCODER) is the composition root; all serialization
routes through wire_encode and validate_detail to guarantee deterministic
ordering and tag enforcement.
"""

from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
from typing import Annotated, Literal, Self

from cyclopts import Parameter
import msgspec

from tools.assay.core.status import fold as rail_fold, RailStatus, Step


# --- [TYPES] ----------------------------------------------------------------------------


class ArtifactKind(StrEnum):
    """Artifact and match namespace."""

    LOCKS = "locks"
    PROCESS = "process"
    TEST = "test"
    MUTATION = "mutation"
    RHINO = "rhino"
    SCOPE = "scope"
    CODE = "code"
    HISTORY = "history"


class Claim(StrEnum):
    """Proof claim that owns a rail."""

    STATIC = "static"
    CODE = "code"
    TEST = "test"
    BRIDGE = "bridge"
    PACKAGE = "package"
    API = "api"
    DOCS = "docs"


class Input(StrEnum):
    """Input placement axis for a tool."""

    flag: tuple[str, ...]
    scoped: bool
    FILES = "files", (), False
    INCLUDE = "include", ("--include",), False
    PROJECT = "project", (), True
    SOLUTION = "solution", (), True
    NONE = "none", (), True
    OWNED = "owned", (), True  # command owns its input placement; place() contributes a single empty tail

    def __new__(cls, value: str, flag: tuple[str, ...], scoped: bool) -> Self:  # noqa: FBT001  # enum payload binder mirrors enum field order
        """Bind multi-payload StrEnum fields; StrEnum propagates only the string value, so flag and scoped are assigned manually."""
        m = str.__new__(cls, value)
        m._value_, m.flag, m.scoped = value, flag, scoped
        return m


class Language(StrEnum):
    """Language axis for routing."""

    strategy: Literal["closure", "glob"]  # closed route discriminant; typos cannot silently route as glob
    suffixes: frozenset[str]
    CSHARP = "csharp", "closure", frozenset((".cs", ".csproj", ".props", ".targets"))
    PYTHON = "python", "glob", frozenset((".py", ".pyi"))
    TYPESCRIPT = "typescript", "glob", frozenset((".ts", ".tsx", ".cts", ".mts"))
    BASH = "bash", "glob", frozenset((".sh", ".bash"))
    SQL = "sql", "glob", frozenset((".sql",))
    DOCS = "docs", "glob", frozenset((".md", ".mmd"))

    def __new__(cls, value: str, strategy: Literal["closure", "glob"], suffixes: frozenset[str]) -> Self:
        """Bind multi-payload StrEnum fields; StrEnum propagates only the string value, so strategy and suffixes are assigned manually."""
        m = str.__new__(cls, value)
        m._value_, m.strategy, m.suffixes = value, strategy, suffixes
        return m


class Mode(StrEnum):
    """Operation mode for a catalog row."""

    stream: bool
    writes: bool
    CHECK = "check", False, False
    WRITE = "write", False, True
    RESTORE = "restore", False, False
    BUILD = "build", True, False
    RUN = "run", False, False
    LIST = "list", False, False
    MUTATION = "mutation", True, False
    CLIENT = "client", False, False
    VERIFY = "verify", True, False
    QUERY = "query", False, False
    CONTENT = "content", False, False
    STAGE = "stage", False, False
    DEPLOY = "deploy", False, False
    PUBLISH = "publish", False, False

    def __new__(cls, value: str, stream: bool, writes: bool) -> Self:  # noqa: FBT001  # enum payload binder mirrors enum field order
        """Bind multi-payload StrEnum fields; StrEnum propagates only the string value, so stream and writes are assigned manually."""
        m = str.__new__(cls, value)
        m._value_, m.stream, m.writes = value, stream, writes
        return m


class MutationLane(StrEnum):
    """Mutation runner lane for test evidence."""

    OFF = "off"
    CHANGED = "changed"
    FULL = "full"


class Runner(StrEnum):
    """Launch axis for a tool."""

    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")
    INPROC = "inproc", ()

    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self:
        """Bind enum payloads; StrEnum only propagates the string value, so extra fields must be assigned manually."""
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m


class SourceKind(StrEnum):
    """Source provenance for API evidence."""

    ASSEMBLY = "assembly"
    NUGET = "nuget"
    TOOL = "tool"
    PYDIST = "pydist"
    TSDECL = "tsdecl"


class SymbolShape(StrEnum):
    """API symbol resolution shape."""

    INDEX = "index"
    NAMESPACE = "namespace"
    TYPE = "type"
    MEMBER = "member"
    SEARCH = "search"


type InprocThunk = Callable[[Check], Completed]

# --- [CONSTANTS] ------------------------------------------------------------------------

_RESULT_CAP: int = 1000
_DEFECT_TAIL: int = 400
# SARIF 2.1 result levels -> assay severities; analyzer notes (e.g. CSP0903) surface as info-grade evidence.
_SARIF_SEVERITY: dict[str, str] = {"error": "error", "warning": "warning", "note": "info", "none": "info"}

# --- [MODELS] ---------------------------------------------------------------------------


class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True, forbid_unknown_fields=True):
    """Shared msgspec wire policy: frozen, gc-free, no unknown fields, omit defaults."""


class Detail(Base, frozen=True, forbid_unknown_fields=True, tag_field="kind"):
    """Tagged-union base for algorithm-specific evidence."""


class Stage(Base, frozen=True, cache_hash=True):
    """Artifact-backed worktree projection for tools with root-writing behavior."""

    root: str = ""
    inputs: tuple[str, ...] = ()
    project: bool = False


class Tool(Base, frozen=True, cache_hash=True):
    """Catalog row describing one executable or in-process program."""

    name: str
    runner: Runner
    command: tuple[str, ...]
    input: Input
    language: Language
    claim: Claim
    mode: Mode = Mode.CHECK
    groups: tuple[str, ...] = ()
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
    thunk: InprocThunk | None = None
    stage: Stage = Stage()
    env: tuple[tuple[str, str], ...] = ()


class Check(Base, frozen=True, cache_hash=True):
    """Tool bound to a concrete input scope.

    ``tail`` pins one ``place()``-projected argument tail to this check; ``None`` defers
    placement to argv composition, which requires the routed placement to be single-tail.
    """

    tool: Tool
    paths: tuple[str, ...] = ()
    cwd: Path | None = None
    tail: tuple[str, ...] | None = None


class Artifact(Base, frozen=True):
    """Produced artifact record."""

    id: str
    kind: ArtifactKind
    path: str
    bytes: Annotated[int, msgspec.Meta(ge=0)] = 0
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0


class Completed(Base, frozen=True):
    """Receipt for a process or in-process tool that ran."""

    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()
    artifacts: tuple[Artifact, ...] = ()


class Counts(Base, frozen=True):
    """Fold-derived report counts."""

    ok: int = 0
    failed: int = 0
    total: int = 0


class Fault(Base, frozen=True):
    """Operational failure that prevented Assay from running a check."""

    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAULTED
    message: Annotated[str, msgspec.Meta(max_length=1024)] = ""


class Match(Base, frozen=True):
    """Ranked evidence row."""

    id: str
    kind: ArtifactKind
    text: Annotated[str, msgspec.Meta(max_length=400)]
    line: Annotated[int, msgspec.Meta(ge=0)] = 0
    score: int = 0
    severity: str | None = None
    confidence: Annotated[int, msgspec.Meta(ge=0, le=100)] = 100


# SARIF wire subset stays off the shared Base policy: SARIF documents carry tool/version/schema
# fields the assay wire contract does not model, so unknown fields must pass, not fault.
class _SarifMessage(msgspec.Struct, frozen=True, gc=False):
    text: str = ""


class _SarifRegion(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    start_line: int = 0


class _SarifArtifactLocation(msgspec.Struct, frozen=True, gc=False):
    uri: str = ""


class _SarifPhysicalLocation(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    artifact_location: _SarifArtifactLocation = msgspec.field(default_factory=_SarifArtifactLocation)
    region: _SarifRegion = msgspec.field(default_factory=_SarifRegion)


class _SarifLocation(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    physical_location: _SarifPhysicalLocation = msgspec.field(default_factory=_SarifPhysicalLocation)


class _SarifResult(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    rule_id: str = ""
    level: str = ""
    message: _SarifMessage = msgspec.field(default_factory=_SarifMessage)
    locations: tuple[_SarifLocation, ...] = ()


class _SarifRun(msgspec.Struct, frozen=True, gc=False):
    results: tuple[_SarifResult, ...] = ()


class _SarifLog(msgspec.Struct, frozen=True, gc=False):
    """Roslyn-shaped SARIF 2.1 subset: runs[].results[] with ruleId/level/message/locations."""

    runs: tuple[_SarifRun, ...] = ()


class ApiResolution(Detail, frozen=True, tag="resolution"):
    """API resolution miss detail."""

    candidates: tuple[tuple[str, int], ...] = ()
    reason: str = ""


class ApiSource(Detail, frozen=True, tag="api-source"):
    """Polyglot API source inventory detail."""

    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    package: str = ""
    primary_assembly: str = ""
    primary_xml: str = ""
    assemblies: tuple[str, ...] = ()
    xmls: tuple[str, ...] = ()
    assets: tuple[str, ...] = ()
    package_root: str = ""
    nuspec: str = ""
    frameworks: tuple[str, ...] = ()
    owners: tuple[str, ...] = ()
    restore: str = ""
    status: RailStatus = RailStatus.EMPTY
    selected: tuple[str, ...] = ()
    candidates: tuple[tuple[str, int], ...] = ()
    reason: str = ""


class ApiSurface(Detail, frozen=True, tag="api"):
    """API surface detail."""

    source: ApiSource = ApiSource()
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""
    member: str = ""
    truncated: bool = False
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0
    selected: Annotated[int, msgspec.Meta(ge=0)] = 0
    artifact_paths: tuple[str, ...] = ()


class Diagnostic(Detail, frozen=True, tag="diagnostic"):
    """Fault and defect diagnostic detail."""

    failing_step: str = ""
    recent_events: tuple[str, ...] = ()
    elapsed_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    hint: Annotated[str, msgspec.Meta(max_length=256)] = ""
    dispatched: bool = True
    resource: tuple[tuple[str, float], ...] = ()
    # trace_id/span_id are additive: defaults keep pre-trace payloads decodable, so schema_version stays 1.
    trace_id: str = ""
    span_id: str = ""


class PackageRun(Detail, frozen=True, tag="package"):
    """Package lifecycle detail."""

    stage: str = ""
    project: str = ""
    pattern: str = ""
    version: str = ""
    manifest_dir: str = ""
    target_dir: str = ""
    package_dir: str = ""
    target_framework: str = ""
    platform: str = ""
    push_source: str = ""
    yak_path: str = ""


class RunSnapshot(Base, frozen=True):
    """Persisted run endpoint for delta details."""

    id: str = ""
    status: RailStatus = RailStatus.EMPTY
    counts: Counts = Counts()


class RunDelta(Detail, frozen=True, tag="delta"):
    """Delta detail comparing two persisted runs."""

    before: RunSnapshot = RunSnapshot()
    after: RunSnapshot = RunSnapshot()
    added: int = 0
    removed: int = 0


class TestRun(Detail, frozen=True, tag="test"):
    """Test run detail."""

    mutation: MutationLane = MutationLane.OFF
    coverage: Annotated[float, msgspec.Meta(ge=0, le=100)] | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0


class VerifySummary(Detail, frozen=True, tag="verify"):
    """Bridge verification summary detail.

    ``facts`` and ``captures`` carry (scenario_stem, JSON_text) pairs decoded from
    execute-phase evidence markers; agents filter structurally instead of scanning notes.
    """

    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""
    first_fault_phase: str = ""
    first_fault_output: Annotated[str, msgspec.Meta(max_length=256)] = ""
    facts: tuple[tuple[str, str], ...] = ()
    captures: tuple[tuple[str, str], ...] = ()


type AnyDetail = ApiSource | ApiSurface | VerifySummary | TestRun | PackageRun | ApiResolution | Diagnostic | RunDelta


class Report(Base, frozen=True):
    """Rail report."""

    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: AnyDetail | None = None


class Envelope(Base, frozen=True, kw_only=True):
    """Top-level Assay wire Envelope."""

    # schema_version is omitted on the wire (omit_defaults) until a v2 divergence forces a discriminant.
    schema_version: Literal[1] = 1
    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    exit_code: int = 0
    run_id: str = ""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    report: Report | None = None
    error: Fault | None = None
    error_context: Diagnostic | None = None
    truncated: bool = False
    notes: tuple[str, ...] = ()

    def __cyclopts_returncode__(self) -> int:  # noqa: PLW3201  # Cyclopts protocol hook: supplies process exit code
        """Return the process exit code for the Cyclopts runtime."""
        return self.exit_code


class Bind(Base, frozen=True):
    """Registry binding from claim and verb to a handler."""

    claim: Claim
    verb: str
    handler: object
    params: type
    help: str = ""


# --- [OPERATIONS] -----------------------------------------------------------------------


def field_cap(struct: type[msgspec.Struct], field: str, *, default: int) -> int:
    """Return the msgspec `max_length` for a string field, or `default` when absent."""
    match msgspec.inspect.type_info(struct):
        case msgspec.inspect.StructType(fields=fields):
            return next(
                (
                    f.type.max_length
                    for f in fields
                    if f.name == field and isinstance(f.type, msgspec.inspect.StrType) and f.type.max_length is not None
                ),
                default,
            )
        case _:
            return default


# Reserve headroom within the hint cap so surplus-token text does not sever the diagnostic suffix framing.
_HINT_CAP: int = field_cap(Diagnostic, "hint", default=1 << 62)
_MATCH_TEXT_CAP: int = field_cap(Match, "text", default=400)
_SURPLUS_TOKEN_CAP: int = _HINT_CAP - 76


@Parameter(name="*")
@dataclass(frozen=True, slots=True)
class BaseParams:
    """Shared CLI params base."""

    paths: Annotated[
        tuple[str, ...],
        Parameter(name="paths", help="Positional tokens: paths plus the verb's leading slots (pattern, symbol, key, token); surplus tokens fault."),
    ] = ()
    # show_default stays False: cyclopts 4.16.1 help renders Enum-hinted defaults via default_val.name
    # BEFORE consulting any show_default callable (cyclopts/help/help.py Enum branch), so a None default
    # crashes --help with AttributeError regardless of the callable form. Upstream bug; revisit on bump.
    language: Annotated[Language | None, Parameter(show_default=False)] = None

    def _arity(self, verb: str) -> int | None:  # noqa: PLR6301  # polymorphic dispatch point: package/bridge override on self's type to declare 0
        _ = verb
        return None

    def bound(self, verb: str) -> Self | Fault:
        """Return validated params, or a parse fault for surplus positional tokens."""
        match self._arity(verb):
            case int(cap) if len(self.paths) > cap:
                return self.surplus(verb, self.paths[cap:])
            case _:
                return self

    @staticmethod
    def surplus(verb: str, tokens: tuple[str, ...]) -> Fault:
        """Return a parse fault describing surplus positional tokens."""
        joined = " ".join(tokens)
        clipped = joined[:_SURPLUS_TOKEN_CAP] + ("…" if len(joined) > _SURPLUS_TOKEN_CAP else "")
        return Fault((), RailStatus.FAULTED, f"{Step.PARSE}: {verb}: unexpected positional(s): {clipped}")


def receipt(
    argv: tuple[str, ...],
    rc: int,
    *,
    stdout: bytes = b"",
    stderr: bytes = b"",
    duration_ms: float = 0.0,
    status: RailStatus | None = None,
    notes: tuple[str, ...] = (),
    artifacts: tuple[Artifact, ...] = (),
) -> Completed:
    """Return a Completed receipt; derives status from `rc` when not supplied explicitly."""
    return Completed(
        argv=argv,
        returncode=rc,
        stdout=stdout,
        stderr=stderr,
        duration_ms=duration_ms,
        status=status or RailStatus.from_returncode(rc),
        notes=notes,
        artifacts=artifacts,
    )


def validate_detail(detail: AnyDetail | None) -> AnyDetail | None:
    """Round-trip detail through the tagged-union codec to enforce tag and field constraints.

    Returns:
        The decoded detail, or None when the input is None.
    """
    value: AnyDetail | None = msgspec.convert(msgspec.to_builtins(detail), AnyDetail | None)
    return value


def _count(done: Completed) -> tuple[int, int]:
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP:
            return 1, 0
        case RailStatus.FAILED:
            return 0, 1
        case _:
            return 0, 0


def _sarif_log(path: Path) -> _SarifLog:
    # SARIF rows are evidence only: unreadable or malformed documents fold to the empty log, never a fault.
    try:
        return _SARIF_LOG.decode(path.read_bytes())
    except OSError, msgspec.MsgspecError:
        return _SarifLog()


def _sarif_rows(sarif_dir: str | None) -> tuple[Match, ...]:
    files = sorted(Path(sarif_dir).glob("*.sarif")) if sarif_dir else ()
    return tuple(
        Match(
            id=result.rule_id.lower(),
            kind=ArtifactKind.PROCESS,
            text=": ".join(
                part for part in (next((loc.physical_location.artifact_location.uri for loc in result.locations), ""), result.message.text) if part
            )[:_MATCH_TEXT_CAP],
            line=next((loc.physical_location.region.start_line for loc in result.locations), 0),
            severity=_SARIF_SEVERITY.get(result.level, "warning"),
        )
        for path in files
        for run in _sarif_log(path).runs
        for result in run.results
    )


def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *, detail: AnyDetail | None = None, sarif_dir: str | None = None) -> Report:
    """Fold outcomes into a Report: aggregate status, ok/fail counts, defect tail rows, and notes.

    ``sarif_dir`` decodes any ``*.sarif`` documents under the run scope into per-diagnostic
    evidence rows (id, severity, line, text); SARIF rows never alter status, counts, or exit
    codes, and absent or empty directories fold silently to no rows.

    Returns:
        Report carrying folded status, ok/fail counts, defect and SARIF evidence rows, collected artifacts, notes, and optional detail.
    """
    pairs = tuple(map(_count, outcomes))
    ok_n, fail_n = (sum(a) for a in zip(*pairs, strict=True)) if pairs else (0, 0)
    defects = tuple(
        Match(
            id=o.argv[0] if o.argv else claim.value,
            kind=ArtifactKind.PROCESS,
            text=(o.stderr or o.stdout)[-_DEFECT_TAIL:].decode(errors="replace").strip(),
            severity="failed",
        )
        for o, p in zip(outcomes, pairs, strict=True)
        if p == (0, 1)
    )
    return Report(
        claim,
        verb,
        rail_fold(*(o.status for o in outcomes)),
        Counts(ok_n, fail_n, ok_n + fail_n),
        results=(*defects, *_sarif_rows(sarif_dir)),
        artifacts=tuple(artifact for o in outcomes for artifact in o.artifacts),
        notes=tuple(n for o in outcomes for n in o.notes),
        detail=detail,
    )


def envelope(payload: Report | Fault, *, claim: Claim, verb: str, run_id: str = "", error_context: Diagnostic | None = None) -> Envelope:
    """Return a top-level Envelope carrying either a Report or a Fault with exit-code derived from status."""
    match payload:
        case Report() as r:
            return Envelope(schema_version=1, claim=claim, verb=verb, status=r.status, exit_code=r.status.exit_code, run_id=run_id, report=r)
        case Fault() as f:
            return Envelope(
                schema_version=1,
                claim=claim,
                verb=verb,
                status=f.status,
                exit_code=f.status.exit_code,
                run_id=run_id,
                error=f,
                error_context=error_context,
            )


def wire_encode(value: object) -> bytes:
    """Return deterministic JSON bytes via the module-level ordered encoder."""
    return _ENCODER.encode(value)


def wire_safe(text: str) -> str:
    """Replace lone surrogates (PEP 383 surrogateescape from invalid-UTF-8 argv) with U+FFFD so msgspec can encode the result.

    Returns:
        A string msgspec can UTF-8 encode for the wire.
    """
    return text.encode("utf-8", "replace").decode("utf-8")


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")
_SARIF_LOG: msgspec.json.Decoder[_SarifLog] = msgspec.json.Decoder(_SarifLog)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AnyDetail",
    "ApiResolution",
    "ApiSource",
    "ApiSurface",
    "Artifact",
    "ArtifactKind",
    "Base",
    "BaseParams",
    "Bind",
    "Check",
    "Claim",
    "Completed",
    "Counts",
    "Detail",
    "Diagnostic",
    "Envelope",
    "Fault",
    "InprocThunk",
    "Input",
    "Language",
    "Match",
    "Mode",
    "MutationLane",
    "PackageRun",
    "Report",
    "RunDelta",
    "RunSnapshot",
    "Runner",
    "SourceKind",
    "Stage",
    "SymbolShape",
    "TestRun",
    "Tool",
    "VerifySummary",
    "_HINT_CAP",
    "_RESULT_CAP",
    "envelope",
    "field_cap",
    "fold",
    "receipt",
    "validate_detail",
    "wire_encode",
    "wire_safe",
]
