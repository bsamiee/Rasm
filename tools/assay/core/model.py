"""Assay wire model: status algebra, routing axes, evidence details, and envelope projections.

All JSON emission routes through the module encoder so envelope order, tagged-detail validation, and exit-code projection stay one contract.
Foreign tool-output parsing lives in ``tools.assay.diagnostics``, keyed by the ``Parser`` vocabulary declared here.
"""

from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from functools import reduce
from pathlib import Path
import re
from typing import Annotated, ClassVar, Literal, Self

from cyclopts import Parameter
import msgspec


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

    def __new__(cls, value: str, flag: tuple[str, ...], scoped: bool) -> Self:  # ruff:ignore[boolean-type-hint-positional-argument]  # enum payload binder mirrors enum field order
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

    def __new__(cls, value: str, stream: bool, writes: bool) -> Self:  # ruff:ignore[boolean-type-hint-positional-argument]  # enum payload binder mirrors enum field order
        """Attach enum payload fields not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.stream, m.writes = value, stream, writes
        return m


class MutationLane(StrEnum):
    """Mutation runner lane for test evidence."""

    OFF = "off"
    CHANGED = "changed"
    FULL = "full"


class Parser(StrEnum):
    """Diagnostics family a catalog row declares for its parseable output; NONE rows contribute defect evidence only."""

    NONE = "none"
    BIOME = "biome"
    CS_CONSOLE = "cs-console"
    MYPY = "mypy"
    RUFF = "ruff"
    RUFF_FORMAT = "ruff-format"
    TSC = "tsc"
    TY = "ty"


class RailStatus(StrEnum):
    """Rail status with its wire token, exit code, severity rank, and the dominant/fold severity algebra."""

    exit_code: int  # bound by __new__; not a real descriptor
    severity: int

    SKIP = "skip", 0, 0, "skipped"
    EMPTY = "empty", 0, 1
    OK = "ok", 0, 2
    DEGRADED = "degraded", 2, 3
    CANDIDATE = "candidate", 2, 4
    UNSUPPORTED = "unsupported", 3, 5
    BUSY = "busy", 5, 6
    TIMEOUT = "timeout", 5, 7
    FAILED = "failed", 1, 8
    FAULTED = "faulted", 2, 9

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        """Bind the wire token, exit code, severity, and string aliases."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.exit_code = exit_code
        member.severity = severity
        for alias in aliases:
            member._add_value_alias_(alias)  # Python 3.13+ raises on cross-member alias collisions
        return member

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        """Project a process return code onto the nearest rail status.

        Returns:
            EMPTY for 0, BUSY for 5, TIMEOUT for 124, FAILED for everything else.
        """
        match rc:
            case 0:
                return cls.EMPTY
            case 5:
                return cls.BUSY
            case 124:
                return cls.TIMEOUT
            case _:
                return cls.FAILED

    @classmethod
    def dominant(cls, left: RailStatus, right: RailStatus) -> RailStatus:
        """Return the higher-severity status, keeping the left status on ties."""
        return left if left.severity >= right.severity else right

    @classmethod
    def fold(cls, *members: RailStatus) -> RailStatus:
        """Reduce statuses under ``dominant``, seeded at ``EMPTY``.

        Returns:
            The highest-severity status among the supplied members.
        """
        return reduce(cls.dominant, members, cls.EMPTY)


class Runner(StrEnum):
    """Launch axis for a tool."""

    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "--silent", "exec")  # --silent keeps the pnpm reporter off stdout, whose JSON belongs to the child tool
    INPROC = "inproc", ()

    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self:
        """Attach enum payload fields not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m


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


class SourceKind(StrEnum):
    """Source provenance for API evidence."""

    ASSEMBLY = "assembly"
    NUGET = "nuget"
    TOOL = "tool"
    PYDIST = "pydist"
    TSDECL = "tsdecl"


class Step(StrEnum):
    """Fault-step taxonomy whose declaration order drives prefix classification.

    ``scan=True`` members may appear as ``{step}:`` message prefixes; status-derived members stay classification-only.
    """

    scan: bool  # bound by __new__; True for the prefix-scan roster, False for status-derived classifications

    STRICT = "strict", True
    VALIDATION = "validation", True
    CONFIG = "config", True
    DISPATCH = "dispatch", True
    PARSE = "parse", True
    SPAWN = "spawn", True
    TIMEOUT = "timeout", False
    LEASE_BUSY = "lease_busy", False
    DEFECTS = "defects", False

    def __new__(cls, value: str, scan: bool) -> Self:  # ruff:ignore[boolean-type-hint-positional-argument]  # positional enum-member payload, not a boolean knob
        """Bind the wire token and the prefix-scan roster flag."""
        member = str.__new__(cls, value)
        member._value_ = value
        member.scan = scan
        return member


class SymbolShape(StrEnum):
    """API symbol resolution shape."""

    INDEX = "index"
    NAMESPACE = "namespace"
    TYPE = "type"
    MEMBER = "member"
    SEARCH = "search"


class ToolGroup(StrEnum):
    """Tool policy group: uv dependency groups inject ``uv run --group``; assay tags drive status/eligibility."""

    uv: bool  # genuine uv dependency group when True, assay policy tag when False

    MUTATION = "mutation", True
    RUN_DEFAULT = "run-default", False
    REQUIRES_COVERAGE = "requires-coverage", False
    REQUIRES_BENCHMARK = "requires-benchmark", False
    EMPTY_ON_EXIT1 = "empty-on-exit1", False

    def __new__(cls, value: str, uv: bool) -> Self:  # ruff:ignore[boolean-type-hint-positional-argument]  # enum payload binder mirrors enum field order
        """Attach the uv-dependency-group flag not represented by the StrEnum value."""
        m = str.__new__(cls, value)
        m._value_, m.uv = value, uv
        return m


type InprocThunk = Callable[[Check], Completed]

# --- [CONSTANTS] ------------------------------------------------------------------------

RESULT_CAP: int = 1000
# host-bound claims cannot run off-host; remote execution rejects them before argv composition.
HOST_BOUND_CLAIMS: frozenset[Claim] = frozenset((Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION))
# Catalog command holes: `{name}` substitutes a ToolArgs string field inside a token; `{name*}` is a whole-token
# tuple splice. A token whose referenced string value is empty drops whole, so optional flags vanish cleanly.
_HOLE: re.Pattern[str] = re.compile(r"\{([a-z_]+)(\*)?\}")

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


class ToolArgs(Base, frozen=True, cache_hash=True):
    """Typed splice values filling the catalog command holes for one check.

    Each field names one hole; string fields substitute inside tokens (``{fqn}``), tuple fields splice whole
    tokens (``{targets*}``). Rails never edit ``Tool.command`` — they declare values here and ``fill`` weaves them.
    """

    argv: tuple[str, ...] = ()
    assembly: str = ""
    binary: str = ""
    config: str = ""
    configuration: str = ""
    filter: tuple[str, ...] = ()
    flags: tuple[str, ...] = ()
    fqn: str = ""
    globs: tuple[str, ...] = ()
    input: str = ""
    language: str = ""
    langversion: tuple[str, ...] = ()
    max_children: str = ""
    max_cpu: str = ""
    output: str = ""
    pattern: str = ""
    platform: str = ""
    project: str = ""
    props: tuple[str, ...] = ()
    refs: tuple[str, ...] = ()
    sarif_dir: str = ""
    scope: tuple[str, ...] = ()
    sink: str = ""
    sink_dir: str = ""
    solution: str = ""
    target: str = ""
    targets: tuple[str, ...] = ()
    verb: str = ""
    version: str = ""

    def fill(self, command: tuple[str, ...]) -> tuple[str, ...]:
        """Weave typed values into a command template.

        Returns:
            The command with every hole resolved; empty-valued string holes drop their whole token.

        Raises:
            ValueError: When a hole names a field of the wrong kind (tuple embedded, or string under ``*``).
        """
        return tuple(part for token in command for part in self._tokens(token))

    def _tokens(self, token: str) -> tuple[str, ...]:
        splat = _HOLE.fullmatch(token)
        if splat is not None and splat.group(2) == "*":
            value = getattr(self, splat.group(1))
            if not isinstance(value, tuple):
                raise ValueError(f"splice hole {token!r} requires a tuple field")
            return value
        holes = tuple(_HOLE.finditer(token))
        if not holes:
            return (token,)
        values = {found.group(1): getattr(self, found.group(1)) for found in holes}
        if any(not isinstance(value, str) for value in values.values()):
            raise ValueError(f"embedded hole in {token!r} requires string fields")
        if any(not value for value in values.values()):
            return ()
        return (_HOLE.sub(lambda found: str(values[found.group(1)]), token),)


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
    stage: Stage = Stage()
    env: tuple[tuple[str, str], ...] = ()
    # (returncode, output marker) the tool emits for "nothing to do"; a b"" marker keys on the returncode alone.
    empty_signature: tuple[int, bytes] | None = None
    # Diagnostics family for the row's parseable output; the engine stamps it onto each receipt for the report fold.
    parser: Parser = Parser.NONE
    # Row-owned PROJECT placement flag (e.g. ("--project",)); empty means the bare project token.
    input_flag: tuple[str, ...] = ()

    def uv_groups(self) -> tuple[ToolGroup, ...]:
        """Return the groups that name genuine uv dependency groups for ``uv run --group`` injection.

        Returns:
            The subset of ``groups`` flagged as uv dependency groups; assay policy tags are excluded.
        """
        return tuple(group for group in self.groups if group.uv)


class Check(Base, frozen=True, cache_hash=True):
    """Tool bound to a concrete input scope.

    ``tail=None`` defers placement until argv composition and requires the route to resolve to at most one tail.
    ``args`` carries the typed splice values for the row's command holes; ``thunk`` carries the per-invocation
    INPROC callable; ``timeout`` overrides the row timeout without minting a sibling row.
    """

    tool: Tool
    paths: tuple[str, ...] = ()
    cwd: Path | None = None
    tail: tuple[str, ...] | None = None
    args: ToolArgs = ToolArgs()
    thunk: InprocThunk | None = None
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None


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
    # Stamped from the tool row by the engine at receipt time; keys the diagnostics fold without argv sniffing.
    parser: Parser = Parser.NONE
    # Stamped from Check.args at receipt time; the SARIF fold reads this typed field, never re-parses argv.
    sarif_dir: str = ""


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
    # Consumer-bound target framework chosen by the oracle's TFM policy; empty for non-NuGet sources.
    tfm: str = ""


class ApiSurface(Detail, frozen=True, tag="api"):
    """API surface detail.

    ``accessibility``/``kind``/``arity``/``owner``/``reflection`` are the member-truth band: derived from the
    decompiled signature, the INPROC kind capture, and the metadata reflection map, so a verification consumer
    reads them typed instead of re-parsing the preview.
    """

    source: ApiSource = ApiSource()
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""
    member: str = ""
    accessibility: str = ""
    member_kind: str = ""
    arity: Annotated[int, msgspec.Meta(ge=0)] = 0
    owner: str = ""
    reflection: tuple[str, ...] = ()
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
    generated_artifacts: tuple[tuple[str, str, str], ...] = ()
    resource_inventory: tuple[tuple[str, int], ...] = ()
    facts: tuple[tuple[str, str], ...] = ()
    summary: tuple[tuple[str, int], ...] = ()
    extension_summary: tuple[tuple[str, str], ...] = ()
    tool_summary: tuple[tuple[str, str], ...] = ()
    plan_summary: tuple[tuple[str, str], ...] = ()
    tool_surfaces: tuple[tuple[str, str, str], ...] = ()
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

    def __cyclopts_returncode__(self) -> int:  # ruff:ignore[bad-dunder-method-name]  # Cyclopts protocol hook: supplies process exit code
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
HINT_CAP: int = field_cap(Diagnostic, "hint", default=1 << 62)
_SURPLUS_TOKEN_CAP: int = HINT_CAP - 76


@Parameter(name="*")
@dataclass(frozen=True, slots=True)
class BaseParams:
    """Shared CLI params base.

    ``SLOTS`` maps verb names to positional-slot usage text; the ``""`` key is the claim default the registry
    renders when a verb declares no override. Each params dataclass owns its verbs' slot grammar.
    """

    SLOTS: ClassVar[dict[str, str]] = {"": "[PATHS]..."}

    paths: Annotated[
        tuple[str, ...],
        Parameter(name="paths", help="Positional tokens: paths plus the verb's leading slots (pattern, symbol, key, token); surplus tokens fault."),
    ] = ()

    def _arity(self, verb: str) -> int | None:  # ruff:ignore[no-self-use]  # polymorphic dispatch point: package/bridge override on self's type to declare 0
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
                return Fault((), RailStatus.FAULTED, body[:HINT_CAP] + ("…" if len(body) > HINT_CAP else ""))
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
    "Parser",
    "ProvisionRun",
    "RailStatus",
    "Report",
    "RunDelta",
    "RunSnapshot",
    "Runner",
    "SarifStatus",
    "SourceKind",
    "Stage",
    "StaticRun",
    "Step",
    "SymbolShape",
    "TestRun",
    "Tool",
    "ToolArgs",
    "ToolGroup",
    "VerifySummary",
    "HINT_CAP",
    "HOST_BOUND_CLAIMS",
    "RESULT_CAP",
    "envelope",
    "field_cap",
    "language_choice",
    "receipt",
    "validate_detail",
    "wire_encode",
    "wire_safe",
]
