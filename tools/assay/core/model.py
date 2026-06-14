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

from collections import Counter
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
import re
import shlex
from typing import Annotated, Literal, Self
from urllib.parse import unquote, urlparse

from cyclopts import Parameter
import msgspec
import structlog

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
    CSHARP = "csharp", "closure", frozenset((".cs", ".csproj", ".props", ".targets", ".slnx"))
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
_DEFECT_TAIL: int = 4096
# SARIF 2.1 result levels -> assay severities; analyzer notes (e.g. CSP0903) surface as info-grade evidence.
_SARIF_SEVERITY: dict[str, str] = {"error": "error", "warning": "warning", "note": "info", "none": "info"}
_DIAGNOSTIC_SEVERITY_RANK: dict[str, int] = {"error": 0, "warning": 1, "info": 2, "failed": 3}
_PROCESS_BACKED_OK_CLAIMS: tuple[Claim, ...] = (Claim.STATIC, Claim.TEST, Claim.PACKAGE, Claim.BRIDGE)
_ANSI_ESCAPE = re.compile(r"\x1b\[[0-9;]*m")
_HEADER_DIAGNOSTIC = re.compile(r"^(?P<severity>error|warning|warn|info|note)(?:\[(?P<rule>[^\]]+)])?:\s*(?P<message>.+)$", re.IGNORECASE)
_ARROW_LOCATION = re.compile(r"^\s*-->\s*(?P<path>.+?):(?P<line>\d+):(?P<column>\d+)$")
_MYPY_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?):(?P<line>\d+)(?::(?P<column>\d+))?:\s*(?P<severity>error|warning|note):\s*"
    r"(?P<message>.*?)(?:\s+\[(?P<rule>[a-z0-9_.-]+)])?$",
    re.IGNORECASE,
)
_TSC_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?)(?:\((?P<line1>\d+),(?P<column1>\d+)\):?|:(?P<line2>\d+):(?P<column2>\d+)\s+-)\s*"
    r"(?P<severity>error|warning)\s+(?P<rule>TS\d+):\s*(?P<message>.+)$",
    re.IGNORECASE,
)
_BIOME_TEXT_DIAGNOSTIC = re.compile(
    r"^(?P<path>.+?):(?P<line>\d+):(?P<column>\d+)\s+(?P<rule>(?:lint|assist|parse|format)[/\w.-]*)\s*(?P<message>.*)$", re.IGNORECASE
)
_FORMAT_DIAGNOSTIC = re.compile(r"^(?:Would reformat:\s*(?P<path>.+)|(?P<path2>.+?)\s+would be reformatted)$", re.IGNORECASE)
_CS_DIAGNOSTIC = re.compile(
    r"^(?P<path>.*?\.(?:cs|csproj|props|targets|slnx))\((?P<line>\d+),(?P<column>\d+)\):\s*"
    r"(?P<severity>error|warning|info)\s+(?P<rule>[A-Z][A-Z0-9]*\d+):\s*(?P<message>.*?)(?:\s+\[(?P<project>[^\]]+)\])?$",
    re.IGNORECASE,
)

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
    resources: tuple[tuple[str, float], ...] = ()


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
    text: Annotated[str, msgspec.Meta(max_length=4096)]
    line: Annotated[int, msgspec.Meta(ge=0)] = 0
    score: int = 0
    severity: str | None = None
    confidence: Annotated[int, msgspec.Meta(ge=0, le=100)] = 100
    count: Annotated[int, msgspec.Meta(ge=0)] = 0
    column: Annotated[int, msgspec.Meta(ge=0)] = 0
    path: Annotated[str, msgspec.Meta(max_length=1024)] = ""
    project: Annotated[str, msgspec.Meta(max_length=1024)] = ""
    message: Annotated[str, msgspec.Meta(max_length=4096)] = ""


# SARIF wire subset stays off the shared Base policy: SARIF documents carry tool/version/schema
# fields the assay wire contract does not model, so unknown fields must pass, not fault.
class _SarifMessage(msgspec.Struct, frozen=True, gc=False):
    text: str = ""


class _SarifRegion(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    start_line: int = 0
    start_column: int = 0


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


class _PyAnalyzerDiagnostic(msgspec.Struct, frozen=True, gc=False):
    rule_id: str = ""
    severity: str = ""
    path: str = ""
    line: int = 0
    column: int = 0
    title: str = ""
    message: str = ""


class _BiomePoint(msgspec.Struct, frozen=True, gc=False):
    line: int = 0
    column: int = 0


class _BiomeLocation(msgspec.Struct, frozen=True, gc=False):
    path: str = ""
    start: _BiomePoint = msgspec.field(default_factory=_BiomePoint)


class _BiomeDiagnostic(msgspec.Struct, frozen=True, gc=False):
    severity: str = ""
    message: str = ""
    category: str = ""
    location: _BiomeLocation = msgspec.field(default_factory=_BiomeLocation)


class _BiomeReport(msgspec.Struct, frozen=True, gc=False):
    diagnostics: tuple[_BiomeDiagnostic, ...] = ()


@dataclass(frozen=True, slots=True)
class _TextPolicy:
    pattern: re.Pattern[str]
    tools: frozenset[str]
    rule: str = ""
    severity: str = ""
    message: str = ""
    rule_groups: tuple[str, ...] = ("rule",)
    severity_groups: tuple[str, ...] = ("severity",)
    path_groups: tuple[str, ...] = ("path",)
    line_groups: tuple[str, ...] = ("line",)
    column_groups: tuple[str, ...] = ("column",)
    message_groups: tuple[str, ...] = ("message",)

    def match(self, tool: str, line: str) -> Match | None:
        return (
            _diagnostic_match(
                tool,
                self.value(found, self.rule, self.rule_groups, tool),
                self.value(found, self.severity, self.severity_groups, "error"),
                self.value(found, "", self.path_groups, ""),
                self.value(found, "", self.line_groups, "0"),
                self.value(found, "", self.column_groups, "0"),
                self.value(found, self.message, self.message_groups, ""),
            )
            if (not self.tools or tool in self.tools) and (found := self.pattern.match(line)) is not None
            else None
        )

    @staticmethod
    def value(found: re.Match[str], literal: str, groups: tuple[str, ...], fallback: str) -> str:
        return literal or next((value for group in groups if (value := found.groupdict().get(group))), fallback)


@dataclass(frozen=True, slots=True)
class _PayloadPolicy:
    accepts: Callable[[tuple[str, ...]], bool]
    parse: Callable[[str], tuple[Match, ...]]


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


class StaticRun(Detail, frozen=True, tag="static"):
    """Static route, check, skip, artifact, and resource detail."""

    targets: tuple[tuple[str, str], ...] = ()
    routes: tuple[tuple[str, ...], ...] = ()
    planned: tuple[tuple[str, str, str], ...] = ()
    skipped: tuple[tuple[str, str, str], ...] = ()
    phases: tuple[str, ...] = ()
    resources: tuple[tuple[str, float], ...] = ()
    artifacts: tuple[str, ...] = ()


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


type AnyDetail = ApiSource | ApiSurface | VerifySummary | TestRun | StaticRun | PackageRun | ApiResolution | Diagnostic | RunDelta


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


def language_choice(verb: str, *, csharp: bool = False, python: bool = False, typescript: bool = False) -> Language | Fault | None:
    """Project mutually-exclusive CLI language flags into the internal language axis.

    Returns:
        Selected language, None when unrestricted, or a parse Fault when flags conflict.
    """
    selected = tuple(
        language for language, active in ((Language.CSHARP, csharp), (Language.PYTHON, python), (Language.TYPESCRIPT, typescript)) if active
    )
    match selected:
        case ():
            return None
        case (language,):
            return language
        case _:
            flags = ", ".join(f"--{language.value}" for language in selected)
            return Fault((), RailStatus.FAULTED, f"{Step.PARSE}: {verb}: choose one language flag, got {flags}")


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


def _sarif_match(result: _SarifResult) -> Match:
    uri = next((loc.physical_location.artifact_location.uri for loc in result.locations), "")
    line = next((loc.physical_location.region.start_line for loc in result.locations), 0)
    column = next((loc.physical_location.region.start_column for loc in result.locations), 0)
    parsed = urlparse(uri)
    path = unquote(parsed.path if parsed.scheme == "file" else uri)
    message = result.message.text.strip()
    return Match(
        id=result.rule_id.lower(),
        kind=ArtifactKind.CODE,
        text=f"{path}({line},{column}): {message}"[:_MATCH_TEXT_CAP],
        line=line,
        column=column,
        score=column,
        severity=_SARIF_SEVERITY.get(result.level, "warning"),
        path=path,
        message=message,
    )


def _sarif_rows(sarif_dir: str | None) -> tuple[Match, ...]:
    files = sorted(Path(sarif_dir).glob("*.sarif")) if sarif_dir else ()
    return tuple(_sarif_match(result) for path in files for run in _sarif_log(path).runs for result in run.results)


def _csharp_rows(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    rows = tuple(
        Match(
            id=found.group("rule").lower(),
            kind=ArtifactKind.CODE,
            text=(
                f"{found.group('path')}({found.group('line')},{found.group('column')}): {found.group('message').strip()}"
                + (f" [project={project}]" if (project := (found.group("project") or "").strip()) else "")
            )[:_MATCH_TEXT_CAP],
            line=int(found.group("line")),
            column=int(found.group("column")),
            score=int(found.group("column")),
            severity=found.group("severity").lower(),
            path=found.group("path"),
            project=project,
            message=found.group("message").strip(),
        )
        for done in outcomes
        for line in (done.stdout + b"\n" + done.stderr).decode(errors="replace").splitlines()
        for found in (_CS_DIAGNOSTIC.search(line.strip()),)
        if found is not None
    )
    if rows:
        _LOG.info(
            "diagnostic.discovered",
            count=len(rows),
            source=sum(1 for row in rows if not _generated(row)),
            generated=sum(1 for row in rows if _generated(row)),
        )
    return rows


def _rows_of(done: Completed) -> tuple[Match, ...]:
    payload = (done.stdout + b"\n" + done.stderr).decode(errors="replace")
    return next((policy.parse(payload) for policy in _PAYLOAD_POLICIES if policy.accepts(done.argv)), ())


def _text_rows(tool: str, payload: str) -> tuple[Match, ...]:
    rows: list[Match] = []
    pending: tuple[str, str, str] | None = None
    for raw in payload.splitlines():
        line = _ANSI_ESCAPE.sub("", raw).strip()
        if not line:
            continue
        row = next((match for policy in _TEXT_POLICIES if (match := policy.match(tool, line)) is not None), None)
        if row is not None:
            rows.append(row)
            continue
        if (found := _HEADER_DIAGNOSTIC.match(line)) is not None:
            pending = (_severity(found.group("severity")), found.group("rule") or tool, found.group("message").strip())
            continue
        if pending is not None and (found := _ARROW_LOCATION.match(line)) is not None:
            severity, rule, message = pending
            rows.append(_diagnostic_match(tool, rule, severity, found.group("path"), found.group("line"), found.group("column"), message))
            pending = None
    return tuple(rows)


def _py_analyzer_rows(payload: str) -> tuple[Match, ...]:
    try:
        diagnostics = _PY_ANALYZER_LOG.decode(payload.encode())
    except msgspec.MsgspecError:
        return ()
    return tuple(
        _diagnostic_match("py-analyzer", row.rule_id, row.severity, row.path, str(row.line), str(row.column), row.message or row.title)
        for row in diagnostics
    )


def _biome_rows(payload: str) -> tuple[Match, ...]:
    json_payload = _json_object(payload)
    if json_payload:
        try:
            decoded = _BIOME_LOG.decode(json_payload.encode())
        except msgspec.MsgspecError:
            decoded = _BiomeReport()
        rows = tuple(
            _diagnostic_match(
                "biome",
                row.category or "biome",
                row.severity,
                row.location.path,
                str(row.location.start.line),
                str(row.location.start.column),
                row.message,
            )
            for row in decoded.diagnostics
        )
        if rows:
            return rows
    return _text_rows(tool="biome", payload=payload)


def _json_object(payload: str) -> str:
    start = payload.find("{")
    end = payload.rfind("}") + 1
    if start < 0 or end <= start:
        return ""
    try:
        candidate = payload[start:end]
        _ = _BIOME_LOG.decode(candidate.encode())
    except msgspec.MsgspecError:
        return ""
    return candidate


def _severity(raw: str) -> str:
    return {"warn": "warning", "warning": "warning", "note": "info", "info": "info", "error": "error"}.get(raw.lower(), "error")


def _diagnostic_match(tool: str, rule: str, severity: str, path: str, line: str, column: str, message: str) -> Match:
    line_number = int(line)
    column_number = int(column)
    rule_id = rule.lower()
    return Match(
        id=f"{tool}:{rule_id}",
        kind=ArtifactKind.CODE,
        text=f"{tool}: {path}:{line_number}:{column_number}: {rule_id}: {message}"[:_MATCH_TEXT_CAP],
        line=line_number,
        column=column_number,
        score=column_number,
        severity=_severity(severity),
        path=path,
        project=tool,
        message=message,
    )


_TEXT_POLICIES: tuple[_TextPolicy, ...] = (
    _TextPolicy(_MYPY_DIAGNOSTIC, frozenset(("mypy",))),
    _TextPolicy(_TSC_DIAGNOSTIC, frozenset(("tsc",)), line_groups=("line1", "line2"), column_groups=("column1", "column2")),
    _TextPolicy(_BIOME_TEXT_DIAGNOSTIC, frozenset(("biome",)), severity="error"),
    _TextPolicy(
        _FORMAT_DIAGNOSTIC,
        frozenset(("ruff-format",)),
        rule="format",
        severity="error",
        message="file would be reformatted",
        path_groups=("path", "path2"),
    ),
)

_PAYLOAD_POLICIES: tuple[_PayloadPolicy, ...] = (
    _PayloadPolicy(
        lambda argv: any(arg == "ruff" and index + 1 < len(argv) and argv[index + 1] == "format" for index, arg in enumerate(argv)),
        lambda payload: _text_rows("ruff-format", payload),
    ),
    _PayloadPolicy(lambda argv: "ruff" in argv, lambda payload: _text_rows("ruff", payload)),
    _PayloadPolicy(lambda argv: "ty" in argv, lambda payload: _text_rows("ty", payload)),
    _PayloadPolicy(lambda argv: "mypy" in argv, lambda payload: _text_rows("mypy", payload)),
    _PayloadPolicy(lambda argv: "tools.py_analyzer" in argv, _py_analyzer_rows),
    _PayloadPolicy(lambda argv: "biome" in argv, _biome_rows),
    _PayloadPolicy(lambda argv: "tsc" in argv, lambda payload: _text_rows("tsc", payload)),
)


def _generated(row: Match) -> bool:
    path = (row.path or row.text.split(": ", 1)[0]).replace("\\", "/").lower()
    return "/obj/" in path or "/.artifacts/assay/" in path or path.endswith(".g.cs")


def _diagnostic_body(row: Match) -> str:
    return row.message or (row.text.split(": ", 1)[1] if ": " in row.text else row.text)


def _dedupe(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    seen: set[tuple[str, str | None, str, int, int, str]] = set()
    out: list[Match] = []
    for row in rows:
        path = row.path.replace("\\", "/").lower()
        key = (row.id, row.severity, path, row.line, row.column, row.message or row.text)
        if key not in seen:
            seen.add(key)
            out.append(row)
    return tuple(out)


def _group_generated(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    grouped: dict[tuple[str, str | None, str, str], int] = {}
    for row in rows:
        key = (row.id, row.severity, row.project.replace("\\", "/").lower(), _diagnostic_body(row))
        grouped[key] = grouped.get(key, 0) + 1
    return tuple(
        Match(
            id=rule,
            kind=ArtifactKind.PROCESS,
            text=f"generated diagnostics grouped count={count}: {body}"[:_MATCH_TEXT_CAP],
            severity=severity,
            project=project,
            message=body,
            count=count,
        )
        for (rule, severity, project, body), count in sorted(
            grouped.items(), key=lambda item: (_DIAGNOSTIC_SEVERITY_RANK.get(item[0][1] or "", 9), item[0])
        )
    )


def _rank(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    return tuple(
        sorted(rows, key=lambda row: (_DIAGNOSTIC_SEVERITY_RANK.get(row.severity or "", 9), row.id, row.path, row.line, row.column, row.text))
    )


def _result_rows(claim: Claim, outcomes: tuple[Completed, ...], defects: tuple[Match, ...], sarif_dir: str | None) -> tuple[Match, ...]:
    sarif = _sarif_rows(sarif_dir)
    diagnostics = (*tuple(row for done in outcomes for row in _rows_of(done)), *_csharp_rows(outcomes), *sarif)
    if claim is not Claim.STATIC:
        return (*defects, *sarif)
    source = _rank(_dedupe(tuple(row for row in diagnostics if not _generated(row))))
    generated = _group_generated(tuple(row for row in diagnostics if _generated(row)))
    return (*source, *generated, *defects)


def _diagnostic_notes(rows: tuple[Match, ...]) -> tuple[str, ...]:
    weights = tuple((m, m.count or 1) for m in rows)
    rule_counts: Counter[str] = Counter()
    for row, count in weights:
        rule_counts[row.id] += count
    summary = (
        f"diagnostics: total={sum(count for _, count in weights)} "
        f"source={sum(count for row, count in weights if row.kind is ArtifactKind.CODE)} "
        f"generated={sum(count for row, count in weights if row.kind is ArtifactKind.PROCESS and row.count)} "
        f"error={sum(count for row, count in weights if row.severity == 'error')} "
        f"warning={sum(count for row, count in weights if row.severity == 'warning')} "
        f"info={sum(count for row, count in weights if row.severity == 'info')}"
    )
    return (
        summary,
        "diagnostics.rules: " + " ".join(f"{rule}={count}" for rule, count in sorted(rule_counts.items(), key=lambda item: (-item[1], item[0]))[:16]),
    )


def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *, detail: AnyDetail | None = None, sarif_dir: str | None = None) -> Report:
    """Fold outcomes into a Report: aggregate status, ok/fail counts, defect tail rows, and notes.

    ``sarif_dir`` decodes any ``*.sarif`` documents under the run scope into per-diagnostic
    evidence rows (id, severity, line, text). Static source diagnostics participate in the verdict:
    source error rows fail, non-error rows promote a ran-empty receipt to ok, generated diagnostics
    remain evidence, and absent or empty directories fold silently to no rows.

    Returns:
        Report carrying folded status, ok/fail counts, defect and SARIF evidence rows, collected artifacts, notes, and optional detail.
    """
    pairs = tuple(map(_count, outcomes))
    ok_n = sum(ok for ok, _ in pairs)
    fail_n = sum(failed for _, failed in pairs)
    defects = tuple(
        Match(
            id=shlex.join(o.argv) if o.argv else claim.value,
            kind=ArtifactKind.PROCESS,
            text=(o.stderr or o.stdout)[-_DEFECT_TAIL:].decode(errors="replace").strip(),
            severity="failed",
        )
        for o, p in zip(outcomes, pairs, strict=True)
        if p == (0, 1)
    )
    results = _result_rows(claim, outcomes, defects, sarif_dir)
    diagnostic_rows = tuple(m for m in results if m.severity in _SARIF_SEVERITY.values() and m.id)
    source_error = any(row.kind is ArtifactKind.CODE and row.severity == "error" for row in diagnostic_rows)
    folded_status = rail_fold(*(o.status for o in outcomes))
    status = (
        RailStatus.FAILED
        if claim is Claim.STATIC and source_error
        else RailStatus.OK
        if claim in _PROCESS_BACKED_OK_CLAIMS and folded_status is RailStatus.EMPTY and bool(outcomes) and not defects
        else folded_status
    )
    return Report(
        claim,
        verb,
        status,
        Counts(ok_n, fail_n, ok_n + fail_n),
        results=results,
        artifacts=tuple(artifact for o in outcomes for artifact in o.artifacts),
        notes=(
            *tuple(n for o in outcomes for n in o.notes),
            *((_diagnostic_notes(diagnostic_rows)) if claim is Claim.STATIC and diagnostic_rows else ()),
        ),
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
_PY_ANALYZER_LOG: msgspec.json.Decoder[tuple[_PyAnalyzerDiagnostic, ...]] = msgspec.json.Decoder(tuple[_PyAnalyzerDiagnostic, ...])
_BIOME_LOG: msgspec.json.Decoder[_BiomeReport] = msgspec.json.Decoder(_BiomeReport)
_LOG = structlog.get_logger("assay.model")

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
    "StaticRun",
    "SymbolShape",
    "TestRun",
    "Tool",
    "VerifySummary",
    "_HINT_CAP",
    "_RESULT_CAP",
    "envelope",
    "field_cap",
    "fold",
    "language_choice",
    "receipt",
    "validate_detail",
    "wire_encode",
    "wire_safe",
]
