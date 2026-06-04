"""Wire/evidence spine: axis enums, the ``Base`` policy, and the bounded ``Detail`` union.

Owns every non-config ``msgspec`` struct that crosses a rail. Each axis ``StrEnum`` is at once
the Cyclopts token, the wire value, and the ``match`` key.
"""

from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
from typing import Annotated, Literal, Self

from cyclopts import Parameter
import msgspec

from tools.assay.core.status import fold as rail_fold, RailStatus  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------


class Runner(StrEnum):
    """Launch axis: ``prefix`` is the argv head every routed command is splayed onto."""

    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")
    INPROC = "inproc", ()  # in-process executor: no launcher; _guarded runs the thunk on a worker thread under the same fail_after deadline

    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self:
        """Bind the wire value as member identity and attach the launch ``prefix`` payload."""
        m = str.__new__(cls, value)
        m._value_, m.prefix = value, prefix
        return m


class Input(StrEnum):
    """Input axis: ``flag`` precedes the path tail; ``scoped`` selects project closure."""

    flag: tuple[str, ...]
    scoped: bool
    FILES = "files", (), False
    INCLUDE = "include", ("--include",), False
    PROJECT = "project", (), True
    SOLUTION = "solution", (), True
    NONE = "none", (), True

    def __new__(cls, value: str, flag: tuple[str, ...], scoped: bool) -> Self:  # noqa: FBT001  # StrEnum payload unpack
        """Bind the wire value as member identity and attach the ``flag``/``scoped`` payloads."""
        m = str.__new__(cls, value)
        m._value_, m.flag, m.scoped = value, flag, scoped
        return m


class Language(StrEnum):
    """Language axis: ``strategy`` (``closure``/``glob``) is the sole route discriminant."""

    strategy: Literal["closure", "glob"]  # the SOLE route discriminant — a closed vocabulary, not an open str (a typo would silently route glob)
    suffixes: frozenset[str]
    CSHARP = "csharp", "closure", frozenset((".cs", ".csproj", ".props", ".targets"))
    PYTHON = "python", "glob", frozenset((".py", ".pyi"))
    TYPESCRIPT = "typescript", "glob", frozenset((".ts", ".tsx", ".cts", ".mts"))
    BASH = "bash", "glob", frozenset((".sh", ".bash"))
    SQL = "sql", "glob", frozenset((".sql",))
    DOCS = "docs", "glob", frozenset((".md", ".mmd"))

    def __new__(cls, value: str, strategy: Literal["closure", "glob"], suffixes: frozenset[str]) -> Self:
        """Bind the wire value as member identity and attach the ``strategy``/``suffixes`` payloads."""
        m = str.__new__(cls, value)
        m._value_, m.strategy, m.suffixes = value, strategy, suffixes
        return m


class Mode(StrEnum):
    """Operation axis: ``stream`` enables live byte forwarding; ``writes`` flags mutation."""

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

    def __new__(cls, value: str, stream: bool, writes: bool) -> Self:  # noqa: FBT001  # StrEnum payload unpack
        """Bind the wire value as member identity and attach the ``stream``/``writes`` payloads."""
        m = str.__new__(cls, value)
        m._value_, m.stream, m.writes = value, stream, writes
        return m


class Claim(StrEnum):
    """Proof axis: the rail a verb belongs to; the wire ``claim`` discriminant."""

    STATIC = "static"
    CODE = "code"
    TEST = "test"
    BRIDGE = "bridge"
    PACKAGE = "package"
    API = "api"
    DOCS = "docs"


class ArtifactKind(StrEnum):
    """Path-lease namespace; ``Match.kind`` reuses a subset so file and match share one namespace."""

    LOCKS = "locks"
    PROCESS = "process"
    TEST = "test"
    MUTATION = "mutation"
    RHINO = "rhino"
    SCOPE = "scope"
    CODE = "code"
    HISTORY = "history"  # run-history namespace: one persisted Envelope per run_id, read back by the `delta` verb


class SourceKind(StrEnum):
    """api source provenance: typed origin of an ``ApiSurface`` symbol, never ``str``.

    One member per language family — the ``api`` rail auto-detects by key shape (C# ``ASSEMBLY``/``NUGET``
    first, then ``PYDIST`` for an installed Python distribution, then ``TSDECL`` for an npm package's
    ``.d.ts``), so the handlers ``match`` on this discriminant rather than proliferating per-language verbs.
    """

    ASSEMBLY = "assembly"
    NUGET = "nuget"
    TOOL = "tool"
    PYDIST = "pydist"  # installed Python distribution surfaced via importlib.metadata + inspect + annotationlib
    TSDECL = "tsdecl"  # npm package surfaced from its node_modules .d.ts declaration files via tree-sitter


class MutationLane(StrEnum):
    """test mutation lane: typed Stryker/``mutmut`` selection, never ``str``; a decode miss is loud (no ``UNKNOWN``)."""

    OFF = "off"
    STRYKER = "stryker"
    MUTMUT = "mutmut"


class SymbolShape(StrEnum):
    """api symbol shape: typed resolution granularity of an ``ApiSurface``, never ``str``."""

    INDEX = "index"
    NAMESPACE = "namespace"
    TYPE = "type"
    MEMBER = "member"
    SEARCH = "search"


type Parser = Callable[[Completed], AnyDetail | None]
type InprocThunk = Callable[[Check], Completed]  # Runner.INPROC: a bound sync callable the engine runs on a worker thread

# --- [ERRORS] ---------------------------------------------------------------------------


class ResourceBusyError(Exception):
    """Lease contention raised by ``exclusive_lease``; the rail seam maps it to ``RailStatus.BUSY``.

    ``aspect._transient`` returns ``False`` for it so ``@retried`` never re-attempts a held lease —
    a held resource is contention, not a transient fault.
    """


# --- [MODELS] ---------------------------------------------------------------------------


class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True):
    """Root wire policy: frozen, GC-isolated, default-omitting; inherited by all structs."""


class Detail(Base, frozen=True, forbid_unknown_fields=True, tag_field="kind"):
    """Tagged-union base; ``kind`` discriminates and ``forbid_unknown_fields`` gates extras at decode.

    Variants carry algorithm-specific evidence only — never fold-derived counts, which live on
    ``Report``.
    """


class Tool(Base, frozen=True, cache_hash=True):
    """Catalog row: a program plus its launch/route/proof discriminants; ``cache_hash`` makes it a key."""

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
    """A ``Tool`` bound to a concrete scope; ``paths`` flow as a field — ``scope``/``routed`` stay engine parameters."""

    tool: Tool
    paths: tuple[str, ...] = ()
    owner: str = ""
    solution: str = ""
    glob: str = ""
    cwd: Path | None = None


class Completed(Base, frozen=True):
    """Success receipt: a process ran; ``status`` derives from ``returncode`` via ``from_returncode``."""

    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()


class Fault(Base, frozen=True):
    """Error receipt: ``{argv, status, message}`` ONLY — no ``returncode``/``detail``/``stderr``.

    A ``Fault`` means assay could not run the check (spawn/lease/timeout/strict), not that a check
    found defects; ``--strict`` promotion constructs one directly.
    """

    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAULTED
    message: Annotated[str, msgspec.Meta(max_length=1024)] = ""


class Counts(Base, frozen=True):
    """Fold-derived rollup: lives on ``Report`` only; computed solely in ``fold`` via ``_count`` — never on a ``Detail``."""

    ok: int = 0
    failed: int = 0
    total: int = 0


class Artifact(Base, frozen=True):
    """A produced file under a path lease, namespaced by ``ArtifactKind``."""

    id: str
    kind: ArtifactKind
    path: str
    bytes: Annotated[int, msgspec.Meta(ge=0)] = 0
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0


class Match(Base, frozen=True):
    """A ranked result row; ``kind`` reuses ``ArtifactKind`` so file and match share one namespace."""

    id: str
    kind: ArtifactKind
    text: str
    line: Annotated[int, msgspec.Meta(ge=0)] = 0
    score: int = 0
    severity: str | None = None
    confidence: Annotated[int, msgspec.Meta(ge=0, le=100)] = 100


class ApiSurface(Detail, frozen=True, tag="api"):
    """api evidence: typed source provenance and symbol shape, never ``dict``/``str``."""

    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""


class VerifySummary(Detail, frozen=True, tag="verify"):
    """bridge evidence: scenario exception telemetry and the report-dir capture pointer."""

    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""
    first_fault_phase: str = ""
    first_fault_output: Annotated[str, msgspec.Meta(max_length=256)] = ""


class TestRun(Detail, frozen=True, tag="test"):
    """test evidence: raw mutation telemetry; ``killed``/``survived``/``selected`` are flat scalars, not a rollup."""

    mutation: MutationLane = MutationLane.OFF
    coverage: Annotated[float, msgspec.Meta(ge=0, le=100)] | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0


class PackageRun(Detail, frozen=True, tag="package"):
    """package evidence: stage/project/pattern/version of a staging or publish step."""

    stage: str = ""
    project: str = ""
    pattern: str = ""
    version: str = ""


class ApiResolution(Detail, frozen=True, tag="resolution"):
    """api resolution evidence: ranked ``candidates`` and a bounded ``reason`` for agent disambiguation."""

    candidates: tuple[tuple[str, int], ...] = ()
    reason: str = ""


class Diagnostic(Detail, frozen=True, tag="diagnostic"):
    """auto-observability evidence: the Envelope self-diagnoses on the Fault branch only.

    Carries the failing step, a bounded ring of recent events, elapsed wall time, and a bounded
    remediation hint so an agent retriages a faulted run off the wire without re-running. Rides
    ``Envelope.error_context`` (omit_defaults keeps it OFF the success wire); never a fold-derived count.
    """

    failing_step: str = ""
    recent_events: tuple[str, ...] = ()
    elapsed_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    hint: Annotated[str, msgspec.Meta(max_length=256)] = ""


class RunDelta(Detail, frozen=True, tag="delta"):
    """``delta`` evidence: the before/after run identity plus the status/count/result drift of two persisted Envelopes.

    Rides ``Report.detail`` for the root ``delta`` verb. ``added``/``removed`` are the symmetric-difference
    cardinalities of the two runs' ``Match`` result sets (keyed by ``(id, line)``), so an agent reads what a
    change introduced or resolved between two runs straight off the wire without re-diffing the raw reports.
    """

    before_run: str = ""
    after_run: str = ""
    before_status: RailStatus = RailStatus.EMPTY
    after_status: RailStatus = RailStatus.EMPTY
    ok_delta: int = 0
    failed_delta: int = 0
    total_delta: int = 0
    added: int = 0
    removed: int = 0


type AnyDetail = ApiSurface | VerifySummary | TestRun | PackageRun | ApiResolution | Diagnostic | RunDelta


class Report(Base, frozen=True):
    """One report crosses every rail: ``counts`` derive in ``fold``; algorithm evidence rides ``detail``."""

    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: AnyDetail | None = None


class Envelope(Base, frozen=True, kw_only=True):
    """Wire root: inherits ``Base``'s ``omit_defaults=True`` so the success wire stays terse.

    ``schema_version`` is REQUIRED (no default), so msgspec serializes it unconditionally regardless of
    ``omit_defaults`` — the wire version tag always emits while ``error``/``error_context``/``notes``/
    ``truncated`` stay OFF the success wire when defaulted (the prior ``omit_defaults=False`` override
    forced every field to emit, contradicting the discipline and the ``Diagnostic`` contract). ``claim``
    + ``verb`` already carry the dispatched command path, so no separate ``command_path`` field rides the
    wire. ``__cyclopts_returncode__`` feeds ``resolve_returncode`` so the exit code originates from a
    single field aligned to ``status.exit_code``.
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
    """Registry row: binds a verb to its handler and per-verb frozen ``@dataclass`` params."""

    claim: Claim
    verb: str
    handler: object
    params: type
    help: str = ""


@Parameter(name="*")
@dataclass(frozen=True, slots=True, kw_only=True)
class BaseParams:
    """Shared CLI-params leaf: the no-cycle home both rails and registry import.

    Lives here, not in a rail, so rails and registry import it without a cycle. Concrete
    ``StaticParams``/``TestParams``/... inherit it in their owning ``rails/<claim>.py``. The
    ``@Parameter(name="*")`` decoration rides ``__cyclopts__`` and is inherited by every concrete
    ``*Params`` subclass, so the Cyclopts leaf flattens its fields onto the CLI without a per-verb
    ``Annotated`` wrapper — the registry ``_leaf`` reads it off the resolved type, no override hack.
    """

    paths: tuple[str, ...] = ()
    language: Language | None = None


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
    """Build a ``Completed`` success receipt; ``status`` defaults to ``from_returncode(rc)``."""
    return Completed(argv, rc, stdout, stderr, duration_ms, status or RailStatus.from_returncode(rc), notes)


def validate_detail(detail: AnyDetail | None) -> AnyDetail | None:
    """Round-trip a parser ``Detail`` through the cached tagged-union codec so malformity fails loud at fold."""
    return _DETAIL_DECODER.decode(_ENCODER.encode(detail))


def _count(done: Completed) -> tuple[int, int]:
    """Sole count-derivation primitive: fold one ``Completed.status`` to an ``(ok, failed)`` pair."""
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP:
            return 1, 0
        case RailStatus.FAILED:
            return 0, 1
        case _:
            return 0, 0


def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *, detail: AnyDetail | None = None) -> Report:
    """Fold many ``Completed`` into one ``Report``: the SOLE count-derivation site."""
    pairs = tuple(map(_count, outcomes))
    ok_n, fail_n = (sum(a) for a in zip(*pairs, strict=True)) if pairs else (0, 0)
    return Report(
        claim,
        verb,
        rail_fold(*(o.status for o in outcomes)),
        Counts(ok_n, fail_n, ok_n + fail_n),
        notes=tuple(n for o in outcomes for n in o.notes),
        detail=detail,
    )


def envelope(payload: Report | Fault, *, claim: Claim, verb: str, error_context: Diagnostic | None = None) -> Envelope:
    """Wrap a ``Report`` or ``Fault`` into one ``Envelope`` via statement-match; ``error_context`` rides the Fault branch only."""
    match payload:
        case Report() as r:
            return Envelope(schema_version=1, claim=claim, verb=verb, status=r.status, exit_code=r.status.exit_code, report=r)
        case Fault() as f:
            return Envelope(
                schema_version=1, claim=claim, verb=verb, status=f.status, exit_code=f.status.exit_code, error=f, error_context=error_context
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
    "Runner",
    "SourceKind",
    "SymbolShape",
    "TestRun",
    "Tool",
    "VerifySummary",
    "envelope",
    "fold",
    "receipt",
    "validate_detail",
]
