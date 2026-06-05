"""Define assay axes, wire structs, evidence details, and report folds."""

from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
from typing import Annotated, get_args, Literal, Self

from cyclopts import Parameter
import msgspec

from tools.assay.core.status import fold as rail_fold, RailStatus


# --- [TYPES] ----------------------------------------------------------------------------


class Runner(StrEnum):
    """Launch axis for a tool.

    Attributes:
        prefix: Argument prefix prepended before the tool command.

    """

    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")
    INPROC = "inproc", ()  # in-process executor: no launcher; _guarded runs the thunk on a worker thread under the same fail_after deadline

    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self:  # noqa: D102  # enum payload binder
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m


class Input(StrEnum):
    """Input placement axis for a tool.

    Attributes:
        flag: Flag inserted before path arguments.
        scoped: Whether the input is project or solution scoped.

    """

    flag: tuple[str, ...]
    scoped: bool
    FILES = "files", (), False
    INCLUDE = "include", ("--include",), False
    PROJECT = "project", (), True
    SOLUTION = "solution", (), True
    NONE = "none", (), True

    def __new__(cls, value: str, flag: tuple[str, ...], scoped: bool) -> Self:  # noqa: D102, FBT001  # enum payload binder
        m = str.__new__(cls, value)
        m._value_, m.flag, m.scoped = value, flag, scoped
        return m


class Language(StrEnum):
    """Language axis for routing.

    Attributes:
        strategy: Routing strategy for the language.
        suffixes: File suffixes owned by the language.

    """

    strategy: Literal["closure", "glob"]  # the SOLE route discriminant — a closed vocabulary, not an open str (a typo would silently route glob)
    suffixes: frozenset[str]
    CSHARP = "csharp", "closure", frozenset((".cs", ".csproj", ".props", ".targets"))
    PYTHON = "python", "glob", frozenset((".py", ".pyi"))
    TYPESCRIPT = "typescript", "glob", frozenset((".ts", ".tsx", ".cts", ".mts"))
    BASH = "bash", "glob", frozenset((".sh", ".bash"))
    SQL = "sql", "glob", frozenset((".sql",))
    DOCS = "docs", "glob", frozenset((".md", ".mmd"))

    def __new__(cls, value: str, strategy: Literal["closure", "glob"], suffixes: frozenset[str]) -> Self:  # noqa: D102  # enum payload binder
        m = str.__new__(cls, value)
        m._value_, m.strategy, m.suffixes = value, strategy, suffixes
        return m


class Mode(StrEnum):
    """Operation mode for a catalog row.

    Attributes:
        stream: Whether child process streams are tailed live.
        writes: Whether the row may mutate files.

    """

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
    CONTENT = "content", False, False  # ripgrep grammar-blind content search: one self-walk, no per-language fan
    STAGE = "stage", False, False
    DEPLOY = "deploy", False, False
    PUBLISH = "publish", False, False

    def __new__(cls, value: str, stream: bool, writes: bool) -> Self:  # noqa: D102, FBT001  # enum payload binder
        m = str.__new__(cls, value)
        m._value_, m.stream, m.writes = value, stream, writes
        return m


class Claim(StrEnum):
    """Proof claim that owns a rail."""

    STATIC = "static"
    CODE = "code"
    TEST = "test"
    BRIDGE = "bridge"
    PACKAGE = "package"
    API = "api"
    DOCS = "docs"


class ArtifactKind(StrEnum):
    """Artifact and match namespace."""

    LOCKS = "locks"
    PROCESS = "process"
    TEST = "test"
    MUTATION = "mutation"
    RHINO = "rhino"
    SCOPE = "scope"
    CODE = "code"
    HISTORY = "history"  # run-history namespace: one persisted Envelope per run_id, read back by the `delta` verb


class SourceKind(StrEnum):
    """Source provenance for API evidence."""

    ASSEMBLY = "assembly"
    NUGET = "nuget"
    TOOL = "tool"
    PYDIST = "pydist"  # installed Python distribution surfaced via importlib.metadata + inspect + annotationlib
    TSDECL = "tsdecl"  # npm package surfaced from its node_modules .d.ts declaration files via tree-sitter


class MutationLane(StrEnum):
    """Mutation runner lane for test evidence."""

    OFF = "off"
    STRYKER = "stryker"
    MUTMUT = "mutmut"


class SymbolShape(StrEnum):
    """API symbol resolution shape."""

    INDEX = "index"
    NAMESPACE = "namespace"
    TYPE = "type"
    MEMBER = "member"
    SEARCH = "search"


type Parser = Callable[[Completed], AnyDetail | None]
type InprocThunk = Callable[[Check], Completed]  # Runner.INPROC: a bound sync callable the engine runs on a worker thread

# --- [ERRORS] ---------------------------------------------------------------------------


class ResourceBusyError(Exception):
    """Lease contention signal that is mapped to `RailStatus.BUSY`."""


# --- [MODELS] ---------------------------------------------------------------------------


class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True):
    """Base msgspec wire policy shared by assay structs."""


class Detail(Base, frozen=True, forbid_unknown_fields=True, tag_field="kind"):
    """Tagged-union base for algorithm-specific evidence."""


class Tool(Base, frozen=True, cache_hash=True):
    """Catalog row describing one executable or in-process program.

    Attributes:
        name: Program name.
        runner: Launch strategy.
        command: Program command body.
        input: Input projection strategy.
        language: Language slice for the row.
        claim: Claim that owns the row.
        mode: Operation mode.
        timeout: Optional per-check timeout in seconds.
        parser: Optional stdout parser for detail evidence.
        thunk: Optional in-process callable for `Runner.INPROC` rows.

    """

    name: str
    runner: Runner
    command: tuple[str, ...]
    input: Input
    language: Language
    claim: Claim
    mode: Mode = Mode.CHECK
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
    parser: Parser | None = None
    thunk: InprocThunk | None = None  # Runner.INPROC only: the rail splices a bound thunk via structs.replace; omit_defaults keeps it off any wire


class Check(Base, frozen=True, cache_hash=True):
    """Tool bound to a concrete input scope.

    Attributes:
        tool: Tool to execute.
        paths: File paths passed through file-based projections.
        owner: Optional owner label.
        solution: Optional solution label.
        glob: Optional glob label.
        cwd: Optional child-process working directory.

    """

    tool: Tool
    paths: tuple[str, ...] = ()
    owner: str = ""
    solution: str = ""
    glob: str = ""
    cwd: Path | None = None


class Completed(Base, frozen=True):
    """Receipt for a process or in-process tool that ran.

    Attributes:
        argv: Executed argument vector.
        returncode: Process-style return code.
        stdout: Captured stdout tail.
        stderr: Captured stderr tail.
        duration_ms: Execution duration in milliseconds.
        status: Rail status derived from the return code unless overridden.
        notes: Human-readable notes from the check.

    """

    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()


class Fault(Base, frozen=True):
    """Operational failure that prevented assay from running a check.

    Attributes:
        argv: Argument vector associated with the failure.
        status: Fault status.
        message: Bounded fault message.

    """

    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAULTED
    message: Annotated[str, msgspec.Meta(max_length=1024)] = ""


class Counts(Base, frozen=True):
    """Fold-derived report counts.

    Attributes:
        ok: Successful or non-defect receipts.
        failed: Defect receipts.
        total: Total counted receipts.

    """

    ok: int = 0
    failed: int = 0
    total: int = 0


class Artifact(Base, frozen=True):
    """Produced artifact record.

    Attributes:
        id: Stable artifact identifier.
        kind: Artifact namespace.
        path: Artifact path.
        bytes: Artifact byte count.
        lines: Artifact line count.

    """

    id: str
    kind: ArtifactKind
    path: str
    bytes: Annotated[int, msgspec.Meta(ge=0)] = 0
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0


class Match(Base, frozen=True):
    """Ranked evidence row.

    Attributes:
        id: Stable match identifier.
        kind: Match namespace.
        text: Bounded display text.
        line: Source line when available.
        score: Ranking score.
        severity: Optional severity label.
        confidence: Confidence percentage.

    """

    id: str
    kind: ArtifactKind
    text: Annotated[str, msgspec.Meta(max_length=400)]
    line: Annotated[int, msgspec.Meta(ge=0)] = 0
    score: int = 0
    severity: str | None = None
    confidence: Annotated[int, msgspec.Meta(ge=0, le=100)] = 100


class ApiSurface(Detail, frozen=True, tag="api"):
    """API surface detail.

    Attributes:
        source_kind: Source provenance kind.
        source_id: Source identifier.
        version: Source version.
        shape: Symbol shape returned by the query.
        signature: Selected signature text.
        doc: Selected documentation text.
        preview: Bounded preview body.
        member: Resolved member name for member queries.
        truncated: Whether the preview was clipped.
        lines: Total selected line count.

    """

    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""
    member: str = ""  # resolved member name when shape=MEMBER; empty for TYPE/NAMESPACE/INDEX
    truncated: bool = False  # True when the decompile window was clipped by max_lines
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0  # total selected lines in the decompile body (0 for roster/search paths)


class VerifySummary(Detail, frozen=True, tag="verify"):
    """Bridge verification summary detail.

    Attributes:
        exceptions: Exception count reported by Rhino.
        report_dir: Directory containing per-scenario reports.
        first_failure: First failing scenario name.
        first_fault_phase: First failing bridge phase.
        first_fault_output: Bounded diagnostic output for the first fault.

    """

    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""
    first_fault_phase: str = ""
    first_fault_output: Annotated[str, msgspec.Meta(max_length=256)] = ""


class TestRun(Detail, frozen=True, tag="test"):
    """Test run detail.

    Attributes:
        mutation: Mutation lane represented by the detail.
        coverage: Optional coverage percentage.
        killed: Killed mutant count.
        survived: Survived mutant count.
        selected: Selected test or mutant count.

    """

    mutation: MutationLane = MutationLane.OFF
    coverage: Annotated[float, msgspec.Meta(ge=0, le=100)] | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0


class PackageRun(Detail, frozen=True, tag="package"):
    """Package lifecycle detail.

    Attributes:
        stage: Staged package directory.
        project: Package project path.
        pattern: Yak package filename pattern.
        version: Package version.
        manifest_dir: Manifest directory.
        target_dir: Build output directory.
        package_dir: Committed package directory.
        target_framework: Target framework.
        platform: Yak platform.
        push_source: Yak push source.
        yak_path: Yak executable path.

    """

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


class ApiResolution(Detail, frozen=True, tag="resolution"):
    """API resolution miss detail.

    Attributes:
        candidates: Ranked candidate names and scores.
        reason: Reason the requested source or symbol did not resolve.

    """

    candidates: tuple[tuple[str, int], ...] = ()
    reason: str = ""


class Diagnostic(Detail, frozen=True, tag="diagnostic"):
    """Fault and defect diagnostic detail.

    Attributes:
        failing_step: Structural step where the fault or defect surfaced.
        recent_events: Recent log or parse events.
        elapsed_ms: Elapsed invocation time in milliseconds.
        hint: Bounded human-readable hint.
        dispatched: Whether the envelope claim and verb reflect a real dispatch.
        resource: Resource snapshot pairs captured at diagnosis time.

    """

    failing_step: str = ""
    recent_events: tuple[str, ...] = ()
    elapsed_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    hint: Annotated[str, msgspec.Meta(max_length=256)] = ""
    dispatched: bool = True
    resource: tuple[tuple[str, float], ...] = ()


class RunSnapshot(Base, frozen=True):
    """Persisted run endpoint for delta details.

    Attributes:
        id: Run identifier.
        status: Run status.
        counts: Run report counts.

    """

    id: str = ""
    status: RailStatus = RailStatus.EMPTY
    counts: Counts = Counts()


class RunDelta(Detail, frozen=True, tag="delta"):
    """Delta detail comparing two persisted runs.

    Attributes:
        before: Baseline run snapshot.
        after: Compared run snapshot.
        added: Added result count.
        removed: Removed result count.

    """

    before: RunSnapshot = RunSnapshot()
    after: RunSnapshot = RunSnapshot()
    added: int = 0
    removed: int = 0


type AnyDetail = ApiSurface | VerifySummary | TestRun | PackageRun | ApiResolution | Diagnostic | RunDelta


class Report(Base, frozen=True):
    """Rail report.

    Attributes:
        claim: Claim that produced the report.
        verb: Verb that produced the report.
        status: Folded rail status.
        counts: Fold-derived counts.
        artifacts: Produced artifacts.
        results: Ranked result rows.
        notes: Human-readable notes.
        detail: Optional algorithm-specific detail.

    """

    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: AnyDetail | None = None


class Envelope(Base, frozen=True, kw_only=True):
    """Top-level assay wire envelope.

    Attributes:
        schema_version: Required wire schema version.
        claim: Claim associated with the invocation.
        verb: Verb associated with the invocation.
        status: Envelope status.
        exit_code: Process exit code.
        run_id: Run identifier.
        duration_ms: Invocation duration in milliseconds.
        report: Success-channel report.
        error: Fault-channel error.
        error_context: Optional diagnostic detail.
        truncated: Whether inline envelope data was clipped.
        notes: Top-level human-readable notes.

    """

    schema_version: int
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

    def __cyclopts_returncode__(self) -> int:  # noqa: PLW3201  # Cyclopts protocol hook
        return self.exit_code


class Bind(Base, frozen=True):
    """Registry binding from claim and verb to a handler.

    Attributes:
        claim: Claim that owns the verb.
        verb: Verb token.
        handler: Handler callable.
        params: Params type decoded by the CLI layer.
        help: Short CLI help text.

    """

    claim: Claim
    verb: str
    handler: object
    params: type
    help: str = ""


def field_cap(struct: type[msgspec.Struct], field: str, *, default: int) -> int:
    """Read a msgspec string max-length constraint.

    Args:
        struct: msgspec struct type to inspect.
        field: Field name to inspect.
        default: Value returned when no max length is declared.

    Returns:
        Declared max length or the default.

    """
    return (
        next((m.max_length for f in msgspec.structs.fields(struct) if f.name == field for m in get_args(f.type) if isinstance(m, msgspec.Meta)), None)
        or default
    )


# The surplus-token budget reserves a 76-char margin off Diagnostic.hint's cap for the fixed framing the message and hint wrap the
# tokens in ("parse: {verb}: unexpected positional(s): " + " after {ms}ms" + a generous verb); hint's 256 cap dominates Fault.message's 1024.
_HINT_CAP: int = field_cap(Diagnostic, "hint", default=1 << 62)
_SURPLUS_TOKEN_CAP: int = _HINT_CAP - 76


@Parameter(name="*")
@dataclass(frozen=True, slots=True)
class BaseParams:
    """Shared CLI params base.

    Attributes:
        paths: Variadic positional token sink.
        language: Optional language filter.

    """

    paths: tuple[str, ...] = ()
    language: Language | None = None

    def _arity(self, verb: str) -> int | None:  # noqa: PLR6301  # polymorphic dispatch point: package/bridge override on self's type to declare 0
        _ = verb
        return None

    def bound(self, verb: str) -> Self | Fault:
        """Validate positional tokens against the verb arity.

        Args:
            verb: Verb token.

        Returns:
            The params instance or a parse fault for surplus tokens.

        """
        match self._arity(verb):
            case int(cap) if len(self.paths) > cap:
                return self.surplus(verb, self.paths[cap:])
            case _:
                return self

    @staticmethod
    def surplus(verb: str, tokens: tuple[str, ...]) -> Fault:
        """Build a parse fault for surplus positional tokens.

        Args:
            verb: Verb token.
            tokens: Surplus positional tokens.

        Returns:
            Fault describing the unexpected tokens.

        """
        joined = " ".join(tokens)
        clipped = joined[:_SURPLUS_TOKEN_CAP] + ("…" if len(joined) > _SURPLUS_TOKEN_CAP else "")
        return Fault((), RailStatus.FAULTED, f"parse: {verb}: unexpected positional(s): {clipped}")


# --- [OPERATIONS] -----------------------------------------------------------------------


def receipt(
    argv: tuple[str, ...],
    rc: int,
    *,
    stdout: bytes = b"",
    stderr: bytes = b"",
    duration_ms: float = 0.0,
    status: RailStatus | None = None,
    notes: tuple[str, ...] = (),
) -> Completed:
    """Build a completed receipt.

    Args:
        argv: Executed argument vector.
        rc: Process return code.
        stdout: Captured stdout tail.
        stderr: Captured stderr tail.
        duration_ms: Execution duration in milliseconds.
        status: Optional status override.
        notes: Human-readable notes.

    Returns:
        Completed receipt.

    """
    return Completed(argv, rc, stdout, stderr, duration_ms, status or RailStatus.from_returncode(rc), notes)


def validate_detail(detail: AnyDetail | None) -> AnyDetail | None:
    """Validate detail through the tagged-union codec.

    Args:
        detail: Detail value to validate.

    Returns:
        Decoded detail value.

    """
    return _DETAIL_DECODER.decode(_ENCODER.encode(detail))


def _count(done: Completed) -> tuple[int, int]:
    # sole count-derivation primitive: one Completed.status -> (ok, failed)
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP:
            return 1, 0
        case RailStatus.FAILED:
            return 0, 1
        case _:
            return 0, 0


_RESULT_CAP: int = 1000  # Report.results saturation bound; the SOLE definition — api.py and registry.py import this, never redefine it
_DEFECT_TAIL: int = 400  # bounded tail bytes projected from stderr‖stdout for FAILED Match rows; matches Match.text max_length(400)


def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *, detail: AnyDetail | None = None) -> Report:
    """Fold completed receipts into one report.

    Args:
        claim: Claim that owns the report.
        verb: Verb that owns the report.
        outcomes: Completed receipts to fold.
        detail: Optional algorithm-specific detail.

    Returns:
        Folded report.

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
        for o in outcomes
        if _count(o) == (0, 1)
    )
    return Report(
        claim,
        verb,
        rail_fold(*(o.status for o in outcomes)),
        Counts(ok_n, fail_n, ok_n + fail_n),
        results=defects,
        notes=tuple(n for o in outcomes for n in o.notes),
        detail=detail,
    )


def envelope(payload: Report | Fault, *, claim: Claim, verb: str, run_id: str = "", error_context: Diagnostic | None = None) -> Envelope:
    """Wrap a report or fault in an envelope.

    Args:
        payload: Report or fault payload.
        claim: Claim for the envelope.
        verb: Verb for the envelope.
        run_id: Optional run identifier.
        error_context: Optional diagnostic detail for fault envelopes.

    Returns:
        Envelope representing the payload.

    """
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


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER = msgspec.json.Encoder(order="deterministic")  # deterministic order makes the api-surface cache content-addressable
_DETAIL_DECODER: msgspec.json.Decoder[AnyDetail | None] = msgspec.json.Decoder(AnyDetail | None)  # validate_detail tagged-union round-trip

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AnyDetail",
    "ApiResolution",
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
    "Parser",
    "Report",
    "ResourceBusyError",
    "RunDelta",
    "RunSnapshot",
    "Runner",
    "SourceKind",
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
]
