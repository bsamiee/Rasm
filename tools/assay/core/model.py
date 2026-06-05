"""Wire/evidence spine: axis enums, the ``Base`` policy, and the bounded ``Detail`` union.

Owns every non-config ``msgspec`` struct that crosses a rail; each axis ``StrEnum`` is at once
the Cyclopts token, the wire value, and the ``match`` key.
"""

from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from pathlib import Path
from typing import Annotated, get_args, Literal, Self

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
    """api source provenance: typed origin of an ``ApiSurface`` symbol, never ``str``."""

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
    """Lease contention from ``exclusive_lease``; mapped to ``RailStatus.BUSY``, never retried (held ≠ transient)."""


# --- [MODELS] ---------------------------------------------------------------------------


class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True):
    """Root wire policy: frozen, GC-isolated, default-omitting; inherited by all structs."""


class Detail(Base, frozen=True, forbid_unknown_fields=True, tag_field="kind"):
    """Tagged-union base; variants carry algorithm-specific evidence only, never fold-derived counts."""


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
    """Error receipt: assay could not run the check (spawn/lease/timeout/strict), not that a check found defects."""

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
    text: Annotated[str, msgspec.Meta(max_length=400)]
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
    member: str = ""  # resolved member name when shape=MEMBER; empty for TYPE/NAMESPACE/INDEX
    truncated: bool = False  # True when the decompile window was clipped by max_lines
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0  # total selected lines in the decompile body (0 for roster/search paths)


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
    """package evidence: full yak-distribution metadata for a staging or publish step.

    ``stage``/``project``/``pattern``/``version`` are the original four fields; the remaining
    seven carry the full ``YakMeta`` projection so ``package plan`` emits every evaluated field
    as typed machine data rather than prose ``notes``.
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
    """api resolution evidence: ranked ``candidates`` and a bounded ``reason`` for agent disambiguation."""

    candidates: tuple[tuple[str, int], ...] = ()
    reason: str = ""


class Diagnostic(Detail, frozen=True, tag="diagnostic"):
    """auto-observability evidence: rides ``Envelope.error_context`` on both the Fault and FAILED defect rail.

    ``dispatched`` is the TYPED dispatch discriminant for the parse-fault class: ``True`` when a real rail
    resolved (a surplus positional, or an unknown verb/option under a resolved sub-app), ``False`` on a bare
    unknown root token where ``Envelope.claim``/``verb`` are placeholders. An agent reads this boolean
    instead of substring-scraping ``recent_events[0]`` to know whether the wire ``claim`` is a dispatch fact.
    ``omit_defaults`` drops it on the common (non-parse) fault where it stays the ``True`` default.

    ``resource`` carries the psutil snapshot at fault/defect time as ``((key, value), ...)`` pairs reusing
    OTel-style keys (``mem.rss_bytes`` / ``cpu.percent`` / ``proc.num_fds``); empty on clean runs so
    ``omit_defaults`` keeps the success wire terse. Floats accommodate both integer byte counts and
    fractional cpu/fd values in one homogeneous slot.
    """

    failing_step: str = ""
    recent_events: tuple[str, ...] = ()
    elapsed_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    hint: Annotated[str, msgspec.Meta(max_length=256)] = ""
    dispatched: bool = True
    resource: tuple[tuple[str, float], ...] = ()


class RunSnapshot(Base, frozen=True):
    """One side of a ``delta``: a persisted run's ``(id, status, counts)`` endpoint, bundled so ``RunDelta`` pairs two."""

    id: str = ""
    status: RailStatus = RailStatus.EMPTY
    counts: Counts = Counts()


class RunDelta(Detail, frozen=True, tag="delta"):
    """``delta`` evidence: two persisted-run endpoints plus the symmetric-difference drift of their ``Match`` sets.

    ``before``/``after`` carry each run's full ``(id, status, counts)`` so count drift derives off the wire
    (``after.counts - before.counts``); ``added``/``removed`` are the result-set cardinalities keyed by ``(id, line)``.
    """

    before: RunSnapshot = RunSnapshot()
    after: RunSnapshot = RunSnapshot()
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
    """Wire root: ``omit_defaults`` keeps the success wire terse; ``schema_version`` is required so it always emits."""

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


def field_cap(struct: type[msgspec.Struct], field: str, *, default: int) -> int:
    """The SOLE msgspec ``Meta(max_length=…)`` introspection: walk ``struct``'s ``field`` Annotated metas once.

    Every wire-cap literal (``Fault.message``, ``Diagnostic.hint``, and the surplus budget derived off it)
    reads through this one projection so the clip tracks the type — a ``None``/absent constraint folds to
    ``default``. msgspec enforces ``max_length`` on DECODE only, so a wire past the cap encodes to stdout yet
    faults on history readback; clipping to ``field_cap`` keeps every persisted run round-tripping through ``delta``.
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
    """Shared CLI-params leaf: lives here (not in a rail) so rails and registry import it without a cycle.

    ``@Parameter(name="*")`` is inherited by every concrete ``*Params`` subclass, flattening its fields
    onto the CLI without a per-verb ``Annotated`` wrapper. ``paths`` is the SINGLE positional sink
    (``kw_only`` is intentionally absent here so the inherited flatten binds the bare positional
    ``<path>``/``<key>``/``<symbol>``/``<token>`` tokens here): the cyclopts flatten disables positionals
    at the first keyword-only field, so ``paths`` is the only field a bare token can bind, and
    ``language`` (the second field) NEVER captures a positional — it is itself ``kw_only`` in spirit
    because cyclopts stops binding positionals after the first variadic, so it always reads off its
    ``--language`` keyword. Every subclass keeps ``kw_only=True`` so its per-verb discriminant fields hold
    OFF the positional rail; a verb whose contract carries positionals (e.g. ``api query <key> [symbol]``)
    overrides ``bound`` to project them off this ``paths`` sink, with its keyword default as the
    empty-slot fallback.
    """

    paths: tuple[str, ...] = ()
    language: Language | None = None

    def _arity(self, verb: str) -> int | None:  # noqa: PLR6301  # polymorphic dispatch point: package/bridge override on self's type to declare 0
        """Positional-slot contract as DATA: ``None`` = variadic path rail; ``0`` = keyword-only; ``N`` = bounded.

        The base is the variadic rail (``static``/``code``/``test``/``docs``): every token is a legal path.
        A keyword-only rail (``package``/``bridge``) declares ``0`` so ``bound`` rejects any positional with
        NO copy-pasted ``if self.paths`` body. ``api`` overrides ``bound`` itself (its per-verb slot
        PROJECTION is real logic, not a uniform reject), so it does not route through this arity gate.
        """
        _ = verb
        return None

    def bound(self, verb: str) -> Self | Fault:
        """Validate/project the positional ``paths`` sink against the verb's ``_arity`` contract.

        The single positional-arity boundary every verb crosses once at ``rail.run``. The surplus check is
        computed ONCE here off ``_arity`` data: ``None`` passes the variadic record through untouched; a
        bounded ``N`` folds any token past slot ``N`` via ``surplus`` — routed through the same ``parse``
        taxonomy ``_failing_step`` names, never a silent black hole. A verb that PROJECTS owned slots off
        ``paths`` (``api query``/``resolve``/``show``) overrides ``bound`` directly.
        """
        match self._arity(verb):
            case int(cap) if len(self.paths) > cap:
                return self.surplus(verb, self.paths[cap:])
            case _:
                return self

    @staticmethod
    def surplus(verb: str, tokens: tuple[str, ...]) -> Fault:
        """Fold unexpected/surplus positional tokens into the canonical ``parse`` Fault the boundary names.

        The joined-token segment is clipped to ``_SURPLUS_TOKEN_CAP`` so a shell-expanded glob (a bare
        ``bridge verify <glob>`` over many ``*.verify.csx`` files) keeps the whole message under
        ``Fault.message``'s 1024 cap AND the ``Diagnostic.hint`` it distills to under 256 — msgspec
        enforces ``max_length`` on DECODE only, so an unclipped message persists yet faults on history
        readback (``delta`` would silently report the run as not-found).
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
    """Build a ``Completed`` success receipt; ``status`` defaults to ``from_returncode(rc)``."""
    return Completed(argv, rc, stdout, stderr, duration_ms, status or RailStatus.from_returncode(rc), notes)


def validate_detail(detail: AnyDetail | None) -> AnyDetail | None:
    """Round-trip a parser ``Detail`` through the cached tagged-union codec so malformity fails loud at fold."""
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
    """Fold many ``Completed`` into one ``Report``: the SOLE count-derivation site.

    For each ``Completed`` whose status is FAILED (``_count(o) == (0, 1)``), a bounded ``Match`` row
    is projected from the tail of ``stderr`` (preferred) or ``stdout`` so the failing tool's actual
    output rides the typed ``results`` surface uniformly — zero difference between subprocess and
    INPROC modalities, zero ceremony at call sites.
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
    """Wrap a ``Report`` or ``Fault`` into one ``Envelope`` via statement-match; ``error_context`` rides the Fault branch only."""
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
