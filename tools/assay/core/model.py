"""Assay wire model, routing axes, evidence details, and report folds.

All JSON emission routes through the module encoder so envelope order, tagged-detail validation, and exit-code projection stay one contract.
"""

from collections import Counter
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from functools import cache
from pathlib import Path
import re
import shlex
from typing import Annotated, Final, Literal, Self
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
    PROVISION = "provision"


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
        """Attach enum payload fields not represented by the StrEnum value."""
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
        """Attach enum payload fields not represented by the StrEnum value."""
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
        """Attach enum payload fields not represented by the StrEnum value."""
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
        """Attach enum payload fields not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m


class ToolGroup(StrEnum):
    """Tool policy group: uv dependency groups inject ``uv run --group``; assay tags drive status/eligibility."""

    uv: bool  # genuine uv dependency group when True, assay policy tag when False

    MUTATION = "mutation", True
    RUN_DEFAULT = "run-default", False
    REQUIRES_COVERAGE = "requires-coverage", False
    REQUIRES_BENCHMARK = "requires-benchmark", False
    EMPTY_ON_EXIT1 = "empty-on-exit1", False

    def __new__(cls, value: str, uv: bool) -> Self:  # noqa: FBT001  # enum payload binder mirrors enum field order
        """Attach the uv-dependency-group flag not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.uv = value, uv
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


class SarifStatus(StrEnum):
    """Per-build SARIF evidence class.

    The ``absent:*`` reasons distinguish a warm incremental skip (no recompile, no analyzer pass) from a clean
    analyzer pass, so an agent never reads an incremental "no SARIF" as "no diagnostics".
    """

    PRODUCED = "produced"
    INCREMENTAL = "absent:incremental"
    NO_BUILD = "absent:no-build"
    BUILD_FAILED = "absent:build-failed"

    def token(self, results: int) -> str:
        """Render this class as a wire token, qualifying ``produced`` with its result count.

        Returns:
            ``produced:N`` for a produced SARIF, else the bare absent-reason token.
        """
        return f"{self.value}:{results}" if self is SarifStatus.PRODUCED else self.value


type InprocThunk = Callable[[Check], Completed]

# --- [CONSTANTS] ------------------------------------------------------------------------

_RESULT_CAP: int = 1000
_DEFECT_TAIL: int = 4096
# SARIF 2.1 result levels -> assay severities; analyzer notes (e.g. CSP0903) surface as info-grade evidence.
_SARIF_SEVERITY: dict[str, str] = {"error": "error", "warning": "warning", "note": "info", "none": "info"}
_DIAGNOSTIC_SEVERITY_RANK: dict[str, int] = {"error": 0, "warning": 1, "info": 2, "failed": 3}
_PROCESS_BACKED_OK_CLAIMS: tuple[Claim, ...] = (Claim.STATIC, Claim.TEST, Claim.PACKAGE, Claim.BRIDGE, Claim.PROVISION)
# host-bound claims cannot run off-host; remote execution rejects them before argv composition.
_HOST_BOUND_CLAIMS: frozenset[Claim] = frozenset((Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION))
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
# Forward-slash-normalized, lowercased path fragments and suffix that mark generated/build-output rows as evidence-only.
_GENERATED_MARKERS: Final[tuple[str, ...]] = ("/obj/", "/.artifacts/assay/")
_GENERATED_SUFFIX: Final[str] = ".g.cs"

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
    groups: tuple[ToolGroup, ...] = ()
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
    thunk: InprocThunk | None = None
    stage: Stage = Stage()
    env: tuple[tuple[str, str], ...] = ()

    def uv_groups(self) -> tuple[ToolGroup, ...]:
        """Return the groups that name genuine uv dependency groups for ``uv run --group`` injection.

        Returns:
            The subset of ``groups`` flagged as uv dependency groups; assay policy tags are excluded.
        """
        return tuple(group for group in self.groups if group.uv)


class Check(Base, frozen=True, cache_hash=True):
    """Tool bound to a concrete input scope.

    ``tail=None`` defers placement until argv composition and requires the route to resolve to at most one tail.
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


class ExecReceipt(Base, frozen=True):
    """Remote-execution facts for an offloaded check.

    A dedicated carrier field on Completed/Report/Envelope, paralleling Envelope.error_context: it never
    rides the Report.detail slot, so the rail's domain detail and the remote evidence stay disjoint. Local
    execution leaves the carrier ``None``; only the Ssh case projects a receipt.
    """

    target: str = ""
    host: str = ""
    exit_status: int | None = None
    signal: str = ""
    pushed: Annotated[int, msgspec.Meta(ge=0)] = 0
    pulled: Annotated[int, msgspec.Meta(ge=0)] = 0
    notes: tuple[str, ...] = ()

    @classmethod
    def merge(cls, receipts: tuple[ExecReceipt, ...]) -> ExecReceipt | None:
        """Fold the per-outcome remote receipts of a multi-check fold into one host receipt.

        A fan-out over one ``exec_target`` yields one receipt per check, all to the same host; the merge sums the
        push/pull counts, concatenates notes, and keeps the host identity, so a multi-outcome remote fold surfaces
        every leg's transfer evidence instead of only the first outcome's.

        Returns:
            The folded receipt, or ``None`` when no outcome carried one (a local run).
        """
        if not receipts:
            return None
        if len(receipts) == 1:
            return receipts[0]
        head, last_status = receipts[0], next((r.exit_status for r in reversed(receipts) if r.exit_status is not None), None)
        return cls(
            target=head.target,
            host=head.host,
            exit_status=last_status,
            signal=next((r.signal for r in receipts if r.signal), ""),
            pushed=sum(r.pushed for r in receipts),
            pulled=sum(r.pulled for r in receipts),
            notes=tuple(n for r in receipts for n in r.notes),
        )


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
    exec: ExecReceipt | None = None


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


# --- [SARIF] ----------------------------------------------------------------------------
# SARIF carries tool/schema fields outside the assay wire contract; unknown fields must pass.
# Eight structs model distinct SARIF 2.1 schema levels (log -> run -> result -> location -> region); they are not parallel
# shapes for one concept, so they stay separate owners under one navigation label.


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


class _SarifSuppression(msgspec.Struct, frozen=True, gc=False):
    kind: str = ""


class _SarifResult(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    rule_id: str = ""
    level: str = ""
    message: _SarifMessage = msgspec.field(default_factory=_SarifMessage)
    locations: tuple[_SarifLocation, ...] = ()
    suppressions: tuple[_SarifSuppression, ...] = ()  # non-empty => suppressed in-source; the build honors it, so the rail must too


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


class ProvisionRun(Detail, frozen=True, tag="provision"):
    """Forge provisioning evidence projected from forge-provision JSON."""

    verb: str = ""
    json: bool = False
    schema_version: int = 0
    ok: bool = True
    warnings: tuple[str, ...] = ()
    error: tuple[tuple[str, str], ...] = ()
    auth_mode: str = ""
    auth_risk: str = ""
    port_policy: tuple[tuple[str, str], ...] = ()
    provision_scope: tuple[tuple[str, str], ...] = ()
    local_service_topology: tuple[tuple[str, str, str, str, str, str, str], ...] = ()
    service_roles: tuple[tuple[str, str], ...] = ()
    resource_counts: tuple[tuple[str, int], ...] = ()
    facts: tuple[tuple[str, str], ...] = ()
    summary: tuple[tuple[str, int], ...] = ()
    services: tuple[tuple[str, str, str, str, str], ...] = ()
    service_connections: tuple[tuple[str, str, str, str, str, str, str, str, str], ...] = ()
    ports: tuple[tuple[str, str, str, str, str, str, str], ...] = ()
    extensions: tuple[tuple[str, str, str, str, str, str], ...] = ()
    extension_catalog: tuple[tuple[str, str, str, str, str, str, str, str], ...] = ()
    extension_metadata: tuple[tuple[str, str, str, str, str, str, str, str, str, str, str, str], ...] = ()
    extension_requirements: tuple[tuple[str, str, str, str, str, str, str], ...] = ()
    tool_surface_extensions: tuple[tuple[str, str, str, str, str, str, str, str, str, str, str, str, str], ...] = ()
    doctor: tuple[tuple[str, str], ...] = ()
    local_probes: tuple[tuple[str, str], ...] = ()
    local_probe_values: tuple[tuple[str, str, str], ...] = ()


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
    drift: tuple[tuple[str, str, str], ...] = ()  # (host-fact key, before, after) for changed cross-session host facts


class StaticRun(Detail, frozen=True, tag="static"):
    """Static route, check, skip, artifact, SARIF-status, and resource detail."""

    targets: tuple[tuple[str, str], ...] = ()
    routes: tuple[tuple[str, ...], ...] = ()
    planned: tuple[tuple[str, str, str], ...] = ()
    skipped: tuple[tuple[str, str, str], ...] = ()
    phases: tuple[str, ...] = ()
    resources: tuple[tuple[str, float], ...] = ()
    artifacts: tuple[str, ...] = ()
    # (csproj-stem, SarifStatus token) per C# build outcome; absent:* keeps a warm-incremental skip distinct from a clean pass.
    sarif_status: tuple[tuple[str, str], ...] = ()


class TestRun(Detail, frozen=True, tag="test"):
    """Test run detail."""

    mutation: MutationLane = MutationLane.OFF
    coverage: Annotated[float, msgspec.Meta(ge=0, le=100)] | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0
    project_counts: tuple[tuple[str, int], ...] = ()
    discovery_counts: tuple[tuple[str, int], ...] = ()


class VerifySummary(Detail, frozen=True, tag="verify"):
    """Bridge verification summary detail.

    ``facts`` and ``captures`` carry decoded scenario evidence so consumers avoid note scans.
    """

    exceptions: int = 0
    scenario_status: str = ""
    session_status: str = ""
    report_dir: str = ""
    freshness: str = ""
    first_failure: str = ""
    first_scenario_failure: str = ""
    first_session_fault: str = ""
    first_fault_phase: str = ""
    first_fault_output: Annotated[str, msgspec.Meta(max_length=256)] = ""
    scenario_counts: tuple[tuple[str, int], ...] = ()
    evidence_counts: tuple[tuple[str, int], ...] = ()
    reference_counts: tuple[tuple[str, int], ...] = ()
    artifact_count: int = 0
    capture_count: int = 0
    manifest_count: int = 0
    certificate_path: str = ""
    phase_status: tuple[tuple[str, str], ...] = ()
    facts: tuple[tuple[str, str], ...] = ()
    captures: tuple[tuple[str, str], ...] = ()


class BridgeLifecycle(Detail, frozen=True, tag="bridge"):
    """Bridge lifecycle host and capability projection.

    Non-verify lifecycle verbs expose host fingerprints and capability admission rows without requiring the bridge artifact.
    """

    verb: str = ""
    report_dir: str = ""
    freshness: str = ""
    host: tuple[tuple[str, str], ...] = ()
    capabilities: tuple[tuple[str, str, str], ...] = ()
    first_fault_phase: str = ""
    first_fault_output: Annotated[str, msgspec.Meta(max_length=256)] = ""


type AnyDetail = (
    ApiSource | ApiSurface | VerifySummary | BridgeLifecycle | TestRun | StaticRun | PackageRun | ProvisionRun | ApiResolution | Diagnostic | RunDelta
)


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
    exec: ExecReceipt | None = None


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
    exec: ExecReceipt | None = None
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
    def surplus(verb: str, tokens: tuple[str, ...], *, flags: tuple[str, ...] = (), arity: int | None = None) -> Fault:
        """Return a parse fault describing surplus positional tokens.

        ``flags``/``arity`` append a flag-routing tail naming the exact flags that own the slots, so a verb whose
        positionals are all flag-backed reports which flags the surplus tokens should have used; the full body then
        clips at the hint cap. Without them the bare token list clips at the surplus-token cap.
        """
        joined = " ".join(tokens)
        match (flags, arity):
            case ((_, *_), int()):
                use = f"; {verb} accepts at most {arity} positional(s) — use flags: {' '.join(flags)}"
                body = f"{Step.PARSE}: {verb}: unexpected positional(s): {joined}{use}"
                return Fault((), RailStatus.FAULTED, body[:_HINT_CAP] + ("…" if len(body) > _HINT_CAP else ""))
            case _:
                clipped = joined[:_SURPLUS_TOKEN_CAP] + ("…" if len(joined) > _SURPLUS_TOKEN_CAP else "")
                return Fault((), RailStatus.FAULTED, f"{Step.PARSE}: {verb}: unexpected positional(s): {clipped}")


def language_choice(verb: str, *, csharp: bool = False, python: bool = False, typescript: bool = False) -> Language | Fault | None:
    """Project mutually-exclusive CLI language flags into the internal language axis.

    Returns:
        Selected language, ``None`` when unrestricted, or a parse fault when flags conflict.
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
    """Round-trip detail through the tagged-union codec.

    Returns:
        Validated detail, or ``None`` when absent.
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


@cache
def _sarif_decode(path: Path, stat_key: tuple[int, int]) -> _SarifLog:
    # SARIF rows are evidence only: unreadable or malformed documents fold to the empty log, never a fault.
    _ = stat_key  # content fingerprint participates in the cache key, not the read
    try:
        return _SARIF_LOG.decode(path.read_bytes())
    except OSError, msgspec.MsgspecError:
        return _SarifLog()


def _sarif_log(path: Path) -> _SarifLog:
    # Decode once per (path, mtime, size): rows/status/results read the same build-written document inside one fold, so
    # the fingerprint collapses those redundant decodes; a rebuild that rewrites the document in a long-lived process
    # (the automation daemon re-fires one stable build closure) shifts the fingerprint, so the stale log never sticks.
    try:
        info = path.stat()
    except OSError:
        return _SarifLog()
    return _sarif_decode(path, (info.st_mtime_ns, info.st_size))


def _code_match(rule_id: str, severity: str, path: str, line: int, column: int, message: str, *, text: str, project: str = "") -> Match:
    # One source-diagnostic projection: SARIF, C# console, and text/JSON tool rows differ only in id/text/severity/project
    # derivation; the Match skeleton (CODE kind, score=column, capped text) is one owner.
    return Match(
        id=rule_id,
        kind=ArtifactKind.CODE,
        text=text[:_MATCH_TEXT_CAP],
        line=line,
        column=column,
        score=column,
        severity=severity,
        path=path,
        project=project,
        message=message,
    )


def _sarif_match(result: _SarifResult) -> Match:
    uri = next((loc.physical_location.artifact_location.uri for loc in result.locations), "")
    line = next((loc.physical_location.region.start_line for loc in result.locations), 0)
    column = next((loc.physical_location.region.start_column for loc in result.locations), 0)
    parsed = urlparse(uri)
    path = unquote(parsed.path if parsed.scheme == "file" else uri)
    message = result.message.text.strip()
    return _code_match(
        result.rule_id.lower(), _SARIF_SEVERITY.get(result.level, "warning"), path, line, column, message, text=f"{path}({line},{column}): {message}"
    )


def _argv_sarif_dir(argv: tuple[str, ...], fallback: Path) -> Path:
    return next((Path(token.split("=", 1)[1]) for token in argv if token.startswith("-p:CspSarifDir=")), fallback)


def _sarif_files(base: Path, argv: tuple[str, ...], stem: str, *, slnx: bool) -> tuple[Path, ...]:
    active = _argv_sarif_dir(argv, base)
    match (slnx, bool(stem)):
        case (True, _):
            return tuple(sorted(active.glob("*.sarif")))
        case (False, True):
            return (active / f"{stem}.sarif",)
        case _:
            return tuple(sorted(active.glob("*.sarif")))


def _sarif_rows(sarif_dir: str | None, outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    if not sarif_dir:
        return ()
    base = Path(sarif_dir)
    builds = tuple(done for done in outcomes if "dotnet" in done.argv and "build" in done.argv)
    files = (
        tuple(
            path
            for done in builds
            for stem, slnx in (_build_targets(done.argv) or (("", True),))
            for path in _sarif_files(base, done.argv, stem, slnx=slnx)
        )
        if builds
        else tuple(sorted(base.glob("*.sarif")))
    )
    return tuple(
        _sarif_match(result) for path in files if path.is_file() for run in _sarif_log(path).runs for result in run.results if not result.suppressions
    )


def _build_targets(argv: tuple[str, ...]) -> tuple[tuple[str, bool], ...]:
    # The analyzer drops one ``$(MSBuildProjectName).sarif`` per built project; a .csproj argv names one stem keyed to
    # its own file, a .slnx argv names the whole solution and keys against every SARIF the directory holds.
    return tuple((Path(token).stem, token.endswith(".slnx")) for token in argv if token.endswith((".csproj", ".slnx")))


def _sarif_status(outcomes: tuple[Completed, ...], sarif_dir: str | None) -> tuple[tuple[str, str], ...]:
    base = Path(sarif_dir) if sarif_dir else None
    return tuple(
        (
            stem,
            _classify_sarif(done.status, base, done.argv, stem, slnx=slnx).token(_sarif_results(base, done.argv, stem, slnx=slnx)),
        )
        for done in outcomes
        if "dotnet" in done.argv and "build" in done.argv
        for stem, slnx in (_build_targets(done.argv) or (("", False),))
    )


def _classify_sarif(status: RailStatus, base: Path | None, argv: tuple[str, ...], stem: str, *, slnx: bool) -> SarifStatus:
    produced = base is not None and any(path.is_file() for path in _sarif_files(base, argv, stem, slnx=slnx))
    match (produced, status):
        case (True, _):
            return SarifStatus.PRODUCED
        case (False, RailStatus.SKIP):
            return SarifStatus.NO_BUILD
        case (False, RailStatus.OK | RailStatus.EMPTY):
            return SarifStatus.INCREMENTAL
        case _:
            return SarifStatus.BUILD_FAILED


def _sarif_results(base: Path | None, argv: tuple[str, ...], stem: str, *, slnx: bool) -> int:
    if base is None:
        return 0
    files = _sarif_files(base, argv, stem, slnx=slnx)
    return sum(1 for path in files if path.is_file() for run in _sarif_log(path).runs for result in run.results if not result.suppressions)


def _csharp_rows(outcomes: tuple[Completed, ...]) -> tuple[Match, ...]:
    rows = tuple(
        _code_match(
            found.group("rule").lower(),
            found.group("severity").lower(),
            found.group("path"),
            int(found.group("line")),
            int(found.group("column")),
            message := found.group("message").strip(),
            text=f"{found.group('path')}({found.group('line')},{found.group('column')}): {message}"
            + (f" [project={project}]" if (project := (found.group("project") or "").strip()) else ""),
            project=project,
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


def _json_object[T](payload: str, decoder: msgspec.json.Decoder[T]) -> str:
    start = payload.find("{")
    end = payload.rfind("}") + 1
    if start < 0 or end <= start:
        return ""
    try:
        candidate = payload[start:end]
        _ = decoder.decode(candidate.encode())
    except msgspec.MsgspecError:
        return ""
    return candidate


def _json_rows[T](
    payload: str, *, decoder: msgspec.json.Decoder[T], project: str, rows: Callable[[T], tuple[Match, ...]], embedded: bool = False
) -> tuple[Match, ...]:
    # One JSON-diagnostic projection: a tool whose stdout is a bare diagnostic document decodes the whole payload; a tool
    # that frames its JSON inside log chatter (``embedded``) carves the object span first, then folds to text rows when the
    # decode yields nothing. The decoder, project label, and per-schema row map are the only axes that vary.
    span = _json_object(payload, decoder) if embedded else payload
    if not span:
        return _text_rows(tool=project, payload=payload)
    try:
        decoded = decoder.decode(span.encode())
    except msgspec.MsgspecError:
        return _text_rows(tool=project, payload=payload) if embedded else ()
    projected = rows(decoded)
    return projected if projected or not embedded else _text_rows(tool=project, payload=payload)


def _severity(raw: str) -> str:
    return {"warn": "warning", "warning": "warning", "note": "info", "info": "info", "error": "error"}.get(raw.lower(), "error")


def _diagnostic_match(tool: str, rule: str, severity: str, path: str, line: str, column: str, message: str) -> Match:
    line_number, column_number, rule_id = int(line), int(column), rule.lower()
    return _code_match(
        f"{tool}:{rule_id}",
        _severity(severity),
        path,
        line_number,
        column_number,
        message,
        text=f"{tool}: {path}:{line_number}:{column_number}: {rule_id}: {message}",
        project=tool,
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
    _PayloadPolicy(
        lambda argv: "tools.py_analyzer" in argv,
        lambda payload: _json_rows(
            payload,
            decoder=_PY_ANALYZER_LOG,
            project="py-analyzer",
            rows=lambda diagnostics: tuple(
                _diagnostic_match("py-analyzer", row.rule_id, row.severity, row.path, str(row.line), str(row.column), row.message or row.title)
                for row in diagnostics
            ),
        ),
    ),
    _PayloadPolicy(
        lambda argv: "biome" in argv,
        lambda payload: _json_rows(
            payload,
            decoder=_BIOME_LOG,
            project="biome",
            embedded=True,
            rows=lambda report: tuple(
                _diagnostic_match(
                    "biome",
                    row.category or "biome",
                    row.severity,
                    row.location.path,
                    str(row.location.start.line),
                    str(row.location.start.column),
                    row.message,
                )
                for row in report.diagnostics
            ),
        ),
    ),
    _PayloadPolicy(lambda argv: "tsc" in argv, lambda payload: _text_rows("tsc", payload)),
)


def _generated(row: Match) -> bool:
    path = (row.path or row.text.split(": ", 1)[0]).replace("\\", "/").lower()
    return any(marker in path for marker in _GENERATED_MARKERS) or path.endswith(_GENERATED_SUFFIX)


def _norm_path(path: str) -> str:
    # Cross-channel dedup canonical form: the console emits a cwd-relative path while the SARIF uri is absolute, so the
    # same diagnostic keys differently unless both anchor on the assay cwd (the repo root every dotnet build runs from).
    return Path(path.replace("\\", "/")).absolute().as_posix().lower() if path else ""


def _dedupe(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    seen: set[tuple[str, str | None, str, int, int, str]] = set()
    out: list[Match] = []
    for row in rows:
        key = (row.id, row.severity, _norm_path(row.path), row.line, row.column, row.message or row.text)
        if key not in seen:
            seen.add(key)
            out.append(row)
    return tuple(out)


def _group_generated(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    grouped: dict[tuple[str, str | None, str, str], int] = {}
    for row in rows:
        body = row.message or (row.text.split(": ", 1)[1] if ": " in row.text else row.text)
        key = (row.id, row.severity, row.project.replace("\\", "/").lower(), body)
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
    sarif = _sarif_rows(sarif_dir, outcomes)
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


def fold(
    claim: Claim,
    verb: str,
    outcomes: tuple[Completed, ...],
    *,
    detail: AnyDetail | None = None,
    sarif_dir: str | None = None,
    promote_empty: bool = False,
) -> Report:
    """Fold process outcomes and evidence into a rail report.

    Static source error rows fail the report; generated diagnostics remain evidence-only, and absent SARIF folds to no rows.
    ``promote_empty`` lets a process-backed rail (eligible claim, ran cleanly, no defects) report OK for a folded-empty
    run; without it a clean no-op stays EMPTY so non-rail folds reusing the claim keep their natural absence.

    Returns:
        Report carrying status, counts, artifacts, evidence rows, notes, and optional detail.
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
        if promote_empty and claim in _PROCESS_BACKED_OK_CLAIMS and folded_status is RailStatus.EMPTY and bool(outcomes) and not defects
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
        # Remote-execution facts ride the dedicated carrier: a multi-check fan-out over one host folds every leg's transfer counts.
        exec=ExecReceipt.merge(tuple(o.exec for o in outcomes if o.exec is not None)),
    )


def envelope(payload: Report | Fault, *, claim: Claim, verb: str, run_id: str = "", error_context: Diagnostic | None = None) -> Envelope:
    """Return an envelope with exit code derived from report or fault status."""
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
    """Replace lone surrogates so msgspec can UTF-8 encode argv-derived text.

    Returns:
        Wire-encodable text.
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
    "BridgeLifecycle",
    "Check",
    "Claim",
    "Completed",
    "Counts",
    "Detail",
    "Diagnostic",
    "Envelope",
    "ExecReceipt",
    "Fault",
    "InprocThunk",
    "Input",
    "Language",
    "Match",
    "Mode",
    "MutationLane",
    "PackageRun",
    "ProvisionRun",
    "Report",
    "RunDelta",
    "RunSnapshot",
    "Runner",
    "SarifStatus",
    "SourceKind",
    "Stage",
    "StaticRun",
    "SymbolShape",
    "_sarif_status",
    "TestRun",
    "Tool",
    "ToolGroup",
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
