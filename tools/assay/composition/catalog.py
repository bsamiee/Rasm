"""The polyglot program table: one dense ``Tool`` row per program, one ``select`` fold.

Parsers attach as the literal ``Callable[[Completed], AnyDetail | None]`` identity, so a renamed
decoder is an import-time type error, never a silent miss; ``parser=None`` rows fold via
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
    """One ast-grep ``range`` endpoint: zero-based ``{line, column}`` (``byteOffset`` tolerated)."""

    line: int = 0
    column: int = 0


class _Range(msgspec.Struct, frozen=True, gc=False):
    """The ast-grep match ``range`` ``start``/``end`` points; other keys ride through as unknowns."""

    start: _Point = msgspec.field(default_factory=_Point)
    end: _Point = msgspec.field(default_factory=_Point)


class AstMatch(msgspec.Struct, frozen=True, gc=False):
    """One ast-grep ``run --json=compact`` row the ``code`` rail folds into ``Match``.

    No ``forbid_unknown_fields`` — ast-grep emits extra keys (``charCount``/``language``/
    ``metaVariables``, and ``replacement`` in rewrite mode) that ride one struct, extras ignored.
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


class _RgText(msgspec.Struct, frozen=True, gc=False):
    """One ripgrep ``path``/``lines`` payload: the UTF-8 ``text`` branch (a ``bytes`` branch rides through)."""

    text: str = ""


class _RgData(msgspec.Struct, frozen=True, gc=False):
    """The ripgrep ``--json`` event ``data`` block; other keys ride through as unknowns."""

    path: _RgText = msgspec.field(default_factory=_RgText)
    lines: _RgText = msgspec.field(default_factory=_RgText)
    line_number: int = 0


class RgEvent(msgspec.Struct, frozen=True, gc=False):
    """One ripgrep ``--json`` NDJSON event the ``code search`` rail folds into ``Match``.

    No ``forbid_unknown_fields`` — ripgrep emits ``begin``/``end``/``summary``/``context`` events that
    ride one struct; the rail keeps only ``kind == "match"`` rows. ``kind`` is renamed off the JSON
    ``type`` key (away from the ``type`` builtin).
    """

    kind: str = msgspec.field(default="", name="type")
    data: _RgData = msgspec.field(default_factory=_RgData)


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDINGS = msgspec.json.Decoder(tuple[_Finding, ...])
_SURFACE = msgspec.json.Decoder(ApiSurface)  # decode into the Detail subclass so forbid_unknown_fields gates extras
_VERIFY = msgspec.json.Decoder(VerifySummary)
_TESTS = msgspec.json.Decoder(TestRun)  # decode into the Detail subclass; off-shape lanes caught at parse_tests' codec boundary
_SHELLCHECK = msgspec.json.Decoder(_Shellcheck)
AST_MATCHES = msgspec.json.Decoder(tuple[AstMatch, ...])
CAPTURES = msgspec.json.Decoder(tuple[Capture, ...])
CAPTURE_ENCODER = msgspec.json.Encoder()  # the INPROC tree-sitter thunk encodes captures onto Completed.stdout
RG_EVENT = msgspec.json.Decoder(RgEvent)  # ripgrep --json is NDJSON, decoded one event per line


# --- [OPERATIONS] -----------------------------------------------------------------------


def parse_findings(done: Completed) -> AnyDetail | None:
    """Decode-validate py-analyzer ``--format json`` as a catalog schema guard (yields no ``Detail``)."""
    # Claim.STATIC is a pass/fail gate; the decode is the census schema guard, not a Detail source.
    _FINDINGS.decode(done.stdout or b"[]")
    return None


def parse_build(done: Completed) -> AnyDetail | None:
    """Surface the cs-analyzer MSBuild pass: ``CSP####`` are exit-code evidence, not JSON (yields no ``Detail``)."""
    _ = done
    return None


def parse_tests(done: Completed) -> AnyDetail | None:
    """Decode ``dotnet test`` telemetry into a ``TestRun`` evidence variant.

    Probed over EVERY receipt on a ``--mutation`` run, including non-JSON stdout; a decode miss is a
    defaulted ``TestRun`` (``mutation=off``) that ``_is_mutation`` rejects, so the first real mutation
    lane still wins — not a FAULTED rail.
    """
    try:
        return _TESTS.decode(done.stdout or b"{}")  # codec boundary: a non-JSON / off-shape receipt is a defaulted no-op, not a fault
    except msgspec.DecodeError:
        return TestRun()


def parse_verify(done: Completed) -> AnyDetail | None:
    """Decode the bridge ``verify`` JSON into a ``VerifySummary`` evidence variant.

    Decoding straight into the ``Detail`` subclass enforces its ``max_length=256`` bound on
    ``first_fault_output`` at the msgspec C boundary (an intermediate struct bypassed it) and rejects
    unknown fields — safe because the bridge ``verify`` JSON is Rasm-owned.
    """
    return _VERIFY.decode(done.stdout or b"{}")


def parse_search(done: Completed) -> AnyDetail | None:
    """Decode-validate ast-grep ``run --json=compact`` as a catalog schema guard (yields no ``Detail``)."""
    # Match evidence rides Report.results; the code rail re-decodes the same array for the ranked set.
    AST_MATCHES.decode(done.stdout or b"[]")
    return None


def parse_query(done: Completed) -> AnyDetail | None:
    """Decode-validate the tree-sitter capture array the ``Runner.INPROC`` thunk emits (schema guard, no ``Detail``)."""
    CAPTURES.decode(done.stdout or b"[]")
    return None


def parse_content(done: Completed) -> AnyDetail | None:
    """Decode-validate ripgrep ``--json`` NDJSON as a catalog schema guard (yields no ``Detail``)."""
    tuple(RG_EVENT.decode(line) for line in (done.stdout or b"").splitlines() if line)  # NDJSON: one object per line, decoded line-by-line
    return None


def parse_surface(done: Completed) -> AnyDetail | None:
    """Decode the ``ilspycmd`` surface roster into an ``ApiSurface`` evidence variant.

    Decoding into the ``Detail`` subclass enforces its field bounds at the msgspec C boundary and
    rejects unknown fields (``forbid_unknown_fields``).
    """
    return _SURFACE.decode(done.stdout or b"{}")


def parse_shellcheck(done: Completed) -> AnyDetail | None:
    """Decode-validate ShellCheck ``-f json1`` as a catalog schema guard (yields no ``Detail``)."""
    # Claim.STATIC is a pass/fail gate; the decode is the census schema guard, FAILED rides Completed.
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
    # py/ts api surface+member INPROC thunks emit the SAME Capture array as the tree-sitter code rows,
    # so parse_query is the census-correct decode (NOT parse_surface's ApiSurface JSON).
    Tool("py-api", INPROC, ("py-api", "surface"), NONE, PY, Claim.API, mode=Mode.QUERY, parser=parse_query),
    Tool("py-api", INPROC, ("py-api", "member"), NONE, PY, Claim.API, mode=Mode.LIST, parser=parse_query),
    Tool("ts-api", INPROC, ("ts-api", "surface"), NONE, TS, Claim.API, mode=Mode.QUERY, parser=parse_query),
    Tool("ts-api", INPROC, ("ts-api", "member"), NONE, TS, Claim.API, mode=Mode.LIST, parser=parse_query),
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
    # -- Code (ast-grep `run` self-walks via Input.NONE+splice; tree-sitter query needs the file list via Runner.INPROC) --
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, mode=Mode.WRITE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, parser=parse_search),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, mode=Mode.WRITE, parser=parse_search),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, PY, Claim.CODE, mode=Mode.QUERY, parser=parse_query),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, TS, Claim.CODE, mode=Mode.QUERY, parser=parse_query),
    # ripgrep grammar-blind content search: ONE DIRECT self-walk, never a per-language fan — the PY
    # language tag is inert (census/membership only); --language refines via globs spliced in the rail.
    Tool(
        "ripgrep",
        DIRECT,
        ("rg", "--json", "-U", "--multiline-dotall", "-P", "--hidden", "--glob", "!.git"),
        NONE,
        PY,
        Claim.CODE,
        mode=Mode.CONTENT,
        parser=parse_content,
    ),
)


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Filter ``TOOLS`` to one rail's slice, deterministically sorted (independent of authoring order).

    The ``(language.value, mode.value, name, command)`` key groups by language for fan-out, sorts the
    write/check twins adjacently, and makes execution order stable. ``language=None`` is the polyglot
    request; a C#-only rail (``bridge``/``package``/``api``) passes ``Language.CSHARP``.
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
    "RG_EVENT",
    "AstMatch",
    "Capture",
    "RgEvent",
    "TOOLS",
    "parse_build",
    "parse_content",
    "parse_findings",
    "parse_query",
    "parse_search",
    "parse_shellcheck",
    "parse_surface",
    "parse_tests",
    "parse_verify",
    "select",
]
