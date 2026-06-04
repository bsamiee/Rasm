"""The polyglot program table: one dense ``Tool`` row per program, one ``select`` fold.

Owns ``TOOLS`` (the C#/Python/TypeScript/Bash/SQL/Docs row set), the by-reference parsers, and the
deterministic ``select(claim, language)`` slice. Variance rides the five axis enums
(``Runner``/``Input``/``Language``/``Mode``/``Claim``); the body is data, never control flow. Parsers
attach as the literal ``Callable[[Completed], AnyDetail | None]`` identity, so a renamed decoder is an
import-time type error, never a silent miss; ``parser=None`` rows ("ran, exit code N") fold via
``RailStatus.from_returncode``.
"""

from typing import TYPE_CHECKING

import msgspec

from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    ApiSurface,
    Claim,
    Input,
    Language,
    Mode,
    Runner,
    SourceKind,
    SymbolShape,
    TestRun,
    Tool,
    VerifySummary,
)


if TYPE_CHECKING:
    from tools.assay.core.model import AnyDetail, Completed  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


class _Finding(msgspec.Struct, frozen=True, gc=False):
    """One py-analyzer diagnostic (``parse_findings``)."""

    rule_id: str
    message: str = ""
    path: str = ""
    line: int = 0


class _Surface(msgspec.Struct, frozen=True, gc=False):
    """The ``ilspycmd`` surface roster (``parse_surface``): source provenance + symbol shape evidence."""

    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""


class _TestTelemetry(msgspec.Struct, frozen=True, gc=False):
    """The ``dotnet test --list-tests`` JSON (``parse_tests``): flat mutation/coverage scalars."""

    mutation: str = "off"
    coverage: float | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0


class _ShellMessage(msgspec.Struct, frozen=True, gc=False):
    """One ShellCheck JSON1 ``comments`` row (``parse_shellcheck``)."""

    code: int = 0
    message: str = ""
    file: str = ""
    line: int = 0
    level: str = ""


class _Shellcheck(msgspec.Struct, frozen=True, gc=False):
    """The ShellCheck JSON1 envelope (``-f json1``): a ``comments`` array of ``_ShellMessage`` rows."""

    comments: tuple[_ShellMessage, ...] = ()


class _Point(msgspec.Struct, frozen=True, gc=False):
    """One ast-grep ``range.start``/``range.end`` endpoint: zero-based ``{line, column}`` (``byteOffset`` tolerated)."""

    line: int = 0
    column: int = 0


class _Range(msgspec.Struct, frozen=True, gc=False):
    """The ast-grep match ``range``: ``start``/``end`` points; ``byteOffset`` and other keys ride through as unknowns."""

    start: _Point = msgspec.field(default_factory=_Point)
    end: _Point = msgspec.field(default_factory=_Point)


class AstMatch(msgspec.Struct, frozen=True, gc=False):
    """One ast-grep ``run --json=compact`` row: the ``code`` search/rewrite evidence the rail folds into ``Match``.

    No ``forbid_unknown_fields`` — ast-grep emits ``charCount``/``language``/``metaVariables`` this rail
    does not model, and rewrite mode adds ``replacement``; both shapes ride one struct, extras ignored.
    """

    text: str = ""
    file: str = ""
    lines: str = ""
    replacement: str = ""
    range: _Range = msgspec.field(default_factory=_Range)


class Capture(msgspec.Struct, frozen=True, gc=False):
    """One tree-sitter query capture the ``Runner.INPROC`` thunk emits and the ``code query`` fold reads back."""

    name: str = ""
    text: str = ""
    file: str = ""
    line: int = 0


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDINGS = msgspec.json.Decoder(tuple[_Finding, ...])
_SURFACE = msgspec.json.Decoder(_Surface)
_VERIFY = msgspec.json.Decoder(VerifySummary)
_TESTS = msgspec.json.Decoder(_TestTelemetry)
_SHELLCHECK = msgspec.json.Decoder(_Shellcheck)
AST_MATCHES = msgspec.json.Decoder(tuple[AstMatch, ...])  # code search/rewrite: ast-grep run --json=compact array
CAPTURES = msgspec.json.Decoder(tuple[Capture, ...])  # code query: the INPROC thunk's capture array
CAPTURE_ENCODER = msgspec.json.Encoder()  # the INPROC tree-sitter thunk encodes captures onto Completed.stdout


# --- [OPERATIONS] -----------------------------------------------------------------------


def parse_findings(done: Completed) -> AnyDetail | None:
    """Decode-validate py-analyzer ``--format json`` as a catalog schema guard (yields no ``Detail``).

    ``Claim.STATIC`` is a pass/fail gate folding ``Completed.status``, so ``PYS####`` rows carry no
    ``Detail``; per-finding evidence is the ``code``/``api`` rails' job. The decode earns its place via
    ``_census``, which probes it on the empty receipt at preflight, so a JSON-shape drift fails at
    self-test rather than at a live rail.
    """
    _FINDINGS.decode(done.stdout or b"[]")
    return None


def parse_build(done: Completed) -> AnyDetail | None:
    """Surface the cs-analyzer MSBuild pass: ``CSP####`` are exit-code evidence, not JSON.

    ``dotnet build /clp:ErrorsOnly`` emits free-form ``CSP####`` lines; the count rides
    ``Report.counts`` via ``fold`` and the defect status rides ``Completed.status`` via
    ``from_returncode``, so no ``AnyDetail`` variant is constructed.
    """
    _ = done
    return None


def parse_tests(done: Completed) -> AnyDetail | None:
    """Decode ``dotnet test`` telemetry into a ``TestRun`` evidence variant.

    ``test.py`` probes this over EVERY receipt on a ``--mutation`` run, including non-JSON
    pytest/dotnet stdout; a decode miss is a defaulted ``TestRun`` (``mutation="off"``) that
    ``_is_mutation`` rejects, so the first real mutation lane still wins — not a FAULTED rail.
    """
    try:
        t = _TESTS.decode(done.stdout or b"{}")  # codec boundary: non-JSON stdout is a no-op default, not a fault
    except msgspec.DecodeError:
        return TestRun()
    return TestRun(mutation=t.mutation, coverage=t.coverage, killed=t.killed, survived=t.survived, selected=t.selected)


def parse_verify(done: Completed) -> AnyDetail | None:
    """Decode the bridge ``verify`` JSON directly into a ``VerifySummary`` evidence variant.

    Decoding straight into the ``Detail`` subclass enforces its ``max_length=256`` bound on
    ``first_fault_output`` at the msgspec C boundary (a field-for-field intermediate struct bypassed
    it) and rejects unknown fields — safe because the bridge ``verify`` JSON is Rasm-owned. The empty
    census receipt decodes to a defaulted ``VerifySummary``: the ``kind`` tag is required only for
    union discrimination, never a concrete-type decode.
    """
    return _VERIFY.decode(done.stdout or b"{}")


def parse_search(done: Completed) -> AnyDetail | None:
    """Decode-validate ast-grep ``run --json=compact`` as a catalog schema guard (yields no ``Detail``).

    ``Claim.CODE`` evidence rides ``Report.results`` as ``Match`` rows the rail builds, so this carries no
    ``Detail``; the decode earns its place via ``_census``, which probes it on the empty receipt at
    preflight so an ast-grep JSON-shape drift fails at self-test. The ``code`` rail re-decodes the same
    array to project the bounded ranked ``Match`` set.
    """
    AST_MATCHES.decode(done.stdout or b"[]")
    return None


def parse_query(done: Completed) -> AnyDetail | None:
    """Decode-validate the tree-sitter capture array the ``Runner.INPROC`` thunk emits (a schema guard, no ``Detail``).

    Mirrors ``parse_search``: ``code query`` results ride ``Report.results``; the census probes this on
    the empty receipt so a drift in the in-process capture shape fails at preflight, not at a live query.
    """
    CAPTURES.decode(done.stdout or b"[]")
    return None


def parse_surface(done: Completed) -> AnyDetail | None:
    """Decode the ``ilspycmd`` surface roster into an ``ApiSurface`` evidence variant."""
    s = _SURFACE.decode(done.stdout or b"{}")
    return ApiSurface(
        source_kind=s.source_kind, source_id=s.source_id, version=s.version, shape=s.shape, signature=s.signature, doc=s.doc, preview=s.preview
    )


def parse_shellcheck(done: Completed) -> AnyDetail | None:
    """Decode-validate ShellCheck ``-f json1`` as a catalog schema guard (yields no ``Detail``).

    Like ``parse_findings``: ``Claim.STATIC`` is a pass/fail gate, so the ``comments`` rows (each with a
    ``level`` severity + ``code``) carry no ``Detail`` and surface no ``Match`` here — per-finding evidence
    is the ``code``/``api`` rails' job. The decode is the census schema guard: ``_census`` probes it on the
    empty receipt so a ShellCheck JSON1-shape drift fails at preflight. ``FAILED`` rides ``Completed``.
    """
    _SHELLCHECK.decode(done.stdout or b'{"comments":[]}')
    return None


# --- [TABLES] ---------------------------------------------------------------------------

DIRECT, MODULE, UV, DOTNET, PNPM, INPROC = Runner.DIRECT, Runner.MODULE, Runner.UV, Runner.DOTNET, Runner.PNPM, Runner.INPROC
FILES, INCLUDE, PROJECT, SOLUTION, NONE = (Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.NONE)
PY, TS, CS, BASH, SQL, DOCS = (Language.PYTHON, Language.TYPESCRIPT, Language.CSHARP, Language.BASH, Language.SQL, Language.DOCS)

TOOLS: tuple[Tool, ...] = (
    # -- Python ------------------------------------------------------------------------------
    Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), PROJECT, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check"), FILES, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check", "--fix"), FILES, PY, Claim.STATIC, mode=Mode.WRITE),
    Tool("ruff-format", UV, ("ruff", "format", "--check"), FILES, PY, Claim.STATIC),
    Tool("ruff-format", UV, ("ruff", "format"), FILES, PY, Claim.STATIC, mode=Mode.WRITE),
    Tool("ty", UV, ("ty", "check", "--no-progress"), INCLUDE, PY, Claim.STATIC),
    Tool("mypy", UV, ("mypy", "--explicit-package-bases"), INCLUDE, PY, Claim.STATIC),
    Tool("ast-grep-py", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error"), FILES, PY, Claim.STATIC),
    Tool(
        "ast-grep-py",
        PNPM,
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error", "tests/tools/ast-grep/fail", "--no-color"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("py-analyzer", MODULE, ("tools.py_analyzer", "check", "--root", ".", "--format", "json"), NONE, PY, Claim.STATIC, parser=parse_findings),
    Tool("pytest", UV, ("pytest",), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("pytest-deadfixtures", UV, ("pytest", "--dead-fixtures"), INCLUDE, PY, Claim.TEST, mode=Mode.LIST),
    Tool("pytest-benchmark", UV, ("pytest", "-m", "benchmark", "--benchmark-only", "--benchmark-autosave"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("coverage", UV, ("coverage", "run", "-m", "pytest"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("mutmut", UV, ("mutmut", "run"), PROJECT, PY, Claim.TEST, mode=Mode.MUTATION),
    # -- TypeScript --------------------------------------------------------------------------
    Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), PROJECT, TS, Claim.STATIC, mode=Mode.BUILD),
    Tool("biome", PNPM, ("biome", "ci", ".", "--files-ignore-unknown=true"), NONE, TS, Claim.STATIC),
    Tool("knip", PNPM, ("knip", "--exclude", "catalog", "--no-config-hints"), PROJECT, TS, Claim.STATIC),
    Tool("sherif", PNPM, ("sherif",), PROJECT, TS, Claim.STATIC),
    Tool("ast-grep-ts", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error"), FILES, TS, Claim.STATIC),
    Tool(
        "ast-grep-ts",
        PNPM,
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error", "tests/tools/ast-grep/fail", "--no-color"),
        FILES,
        TS,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("vitest", PNPM, ("vitest", "run"), NONE, TS, Claim.TEST, mode=Mode.RUN),
    # -- C# ----------------------------------------------------------------------------------
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error", "--verify-no-changes"), INCLUDE, CS, Claim.STATIC),
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error"), INCLUDE, CS, Claim.STATIC, mode=Mode.WRITE),
    Tool("dotnet-restore", DOTNET, ("restore", "--locked-mode"), PROJECT, CS, Claim.STATIC, mode=Mode.RESTORE),
    Tool(
        "dotnet-build",
        DOTNET,
        ("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"),
        PROJECT,
        CS,
        Claim.STATIC,
        mode=Mode.BUILD,
        parser=parse_build,
    ),
    Tool("dotnet-test", DOTNET, ("test", "--minimum-expected-tests", "1"), PROJECT, CS, Claim.TEST, mode=Mode.RUN),
    Tool("dotnet-test", DOTNET, ("test", "--list-tests"), PROJECT, CS, Claim.TEST, mode=Mode.LIST, parser=parse_tests),
    Tool(
        "dotnet-stryker",
        DOTNET,
        ("tool", "run", "dotnet-stryker", "--", "--test-runner", "mtp", "--mutation-level", "Standard"),
        PROJECT,
        CS,
        Claim.TEST,
        mode=Mode.MUTATION,
        timeout=3600.0,
    ),
    Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--", "verify"), PROJECT, CS, Claim.BRIDGE, mode=Mode.VERIFY, parser=parse_verify),
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "-l", "cisde"), NONE, CS, Claim.API, mode=Mode.QUERY, parser=parse_surface),
    Tool("yak", DIRECT, ("yak", "build"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    # -- Bash (config-pending day-one rows; linters not yet configured in-repo) --------------
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC, parser=parse_shellcheck),
    Tool("shfmt", DIRECT, ("shfmt", "-d"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-w"), FILES, BASH, Claim.STATIC, mode=Mode.WRITE),
    # -- SQL (config-pending day-one rows; linters not yet configured in-repo) ---------------
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("sqlfluff", UV, ("sqlfluff", "fix", "--dialect", "postgres"), FILES, SQL, Claim.STATIC, mode=Mode.WRITE),
    Tool("squawk", UV, ("squawk",), FILES, SQL, Claim.STATIC),
    # -- Docs --------------------------------------------------------------------------------
    Tool("mmdc", PNPM, ("mmdc", "-a", ".artifacts/mermaid", "-q"), INCLUDE, DOCS, Claim.DOCS),
    # -- Code (ast-grep `run` self-walks the target via Input.NONE+splice; tree-sitter AST query needs the file list via Runner.INPROC) --
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, mode=Mode.WRITE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, mode=Mode.WRITE, parser=parse_search),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, PY, Claim.CODE, mode=Mode.QUERY, parser=parse_query),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, TS, Claim.CODE, mode=Mode.QUERY, parser=parse_query),
)


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Filter ``TOOLS`` to one rail's slice, deterministically sorted (independent of authoring order).

    The key ``(language.value, mode.value, name, command)`` groups by language for fan-out,
    sorts the ``(name, mode)`` write/check twins adjacently, and makes execution order stable.
    ``language=None`` is the polyglot request (``static``/``test``/``docs`` fold across every
    language); a C#-only rail (``bridge``/``package``/``api``) passes ``Language.CSHARP``.
    """
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AST_MATCHES",
    "CAPTURES",
    "CAPTURE_ENCODER",
    "AstMatch",
    "Capture",
    "TOOLS",
    "parse_build",
    "parse_findings",
    "parse_query",
    "parse_search",
    "parse_shellcheck",
    "parse_surface",
    "parse_tests",
    "parse_verify",
    "select",
]
