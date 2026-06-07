"""Define Assay tool catalog rows and rail wire decoders."""

import msgspec

from tools.assay.core.model import Claim, Input, Language, Mode, Runner, Stage, Tool


# --- [CONSTANTS] ------------------------------------------------------------------------

BENCHMARK_STORAGE_URI = "file://.artifacts/python/benchmarks"


# --- [MODELS] ---------------------------------------------------------------------------


class _Point(msgspec.Struct, frozen=True, gc=False):
    line: int = 0
    column: int = 0


class _Range(msgspec.Struct, frozen=True, gc=False):
    start: _Point = msgspec.field(default_factory=_Point)
    end: _Point = msgspec.field(default_factory=_Point)


class AstMatch(msgspec.Struct, frozen=True, gc=False):
    """Ast-grep compact JSON match."""

    text: str = ""
    file: str = ""
    lines: str = ""
    replacement: str = ""
    range: _Range = msgspec.field(default_factory=_Range)


class Capture(msgspec.Struct, frozen=True, gc=False):
    """Tree-sitter capture emitted by an in-process query."""

    name: str = ""
    text: str = ""
    file: str = ""
    line: int = 0
    column: int = 0
    end_line: int = 0
    end_column: int = 0
    start_byte: int = 0
    end_byte: int = 0
    pattern: int = 0
    ordinal: int = 0
    parse_error: bool = False
    truncated: bool = False


class _RgText(msgspec.Struct, frozen=True, gc=False):
    text: str = ""


class _RgData(msgspec.Struct, frozen=True, gc=False):
    path: _RgText = msgspec.field(default_factory=_RgText)
    lines: _RgText = msgspec.field(default_factory=_RgText)
    line_number: int = 0


class RgEvent(msgspec.Struct, frozen=True, gc=False):
    """Ripgrep NDJSON event with `type` decoded as `kind`."""

    kind: str = msgspec.field(default="", name="type")
    data: _RgData = msgspec.field(default_factory=_RgData)


# --- [TABLES] ---------------------------------------------------------------------------

AST_MATCHES = msgspec.json.Decoder(tuple[AstMatch, ...])
CAPTURES = msgspec.json.Decoder(tuple[Capture, ...])
CAPTURE_ENCODER = msgspec.json.Encoder()
RG_EVENT = msgspec.json.Decoder(RgEvent)

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
    Tool("ty", UV, ("ty", "check", "--no-progress"), FILES, PY, Claim.STATIC),
    Tool("mypy", UV, ("mypy", "--no-error-summary", "--hide-error-context", "--no-pretty"), FILES, PY, Claim.STATIC),
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
    Tool("py-analyzer", MODULE, ("tools.py_analyzer", "check", "--format", "json"), NONE, PY, Claim.STATIC),
    Tool("pytest", UV, ("pytest", "-m", "not benchmark"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("pytest", UV, ("pytest", "--collect-only", "-q"), INCLUDE, PY, Claim.TEST, mode=Mode.LIST),
    Tool(
        "pytest-benchmark",
        UV,
        ("pytest", "-m", "benchmark", "--benchmark-only", "--benchmark-autosave", f"--benchmark-storage={BENCHMARK_STORAGE_URI}"),
        INCLUDE,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
    ),
    Tool("coverage", UV, ("coverage", "run", "-m", "pytest", "-m", "not benchmark"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("coverage-json", UV, ("coverage", "json", "-o", ".artifacts/python/coverage/coverage.json"), NONE, PY, Claim.TEST, mode=Mode.CLIENT),
    Tool("coverage-xml", UV, ("coverage", "xml", "-o", ".artifacts/python/coverage/coverage.xml"), NONE, PY, Claim.TEST, mode=Mode.CLIENT),
    Tool("coverage-lcov", UV, ("coverage", "lcov", "-o", ".artifacts/python/coverage/coverage.lcov"), NONE, PY, Claim.TEST, mode=Mode.CLIENT),
    Tool("coverage-report", UV, ("coverage", "report", "--format=total"), NONE, PY, Claim.TEST, mode=Mode.CLIENT),
    Tool(
        "mutmut",
        UV,
        ("mutmut", "run"),
        NONE,
        PY,
        Claim.TEST,
        mode=Mode.MUTATION,
        groups=("mutation",),
        stage=Stage(
            root=".artifacts/python/mutmut/work",
            inputs=("pyproject.toml", ".gitignore", "tools/assay", "tests/conftest.py", "tests/tools/assay"),
            project=True,
        ),
    ),
    # -- TypeScript --------------------------------------------------------------------------
    Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), PROJECT, TS, Claim.STATIC, mode=Mode.BUILD),
    Tool("biome", PNPM, ("biome", "ci", "--files-ignore-unknown=true"), NONE, TS, Claim.STATIC),
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
    Tool("dotnet-build", DOTNET, ("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"), PROJECT, CS, Claim.STATIC, mode=Mode.BUILD),
    Tool("dotnet-test", DOTNET, ("test", "--minimum-expected-tests", "1"), PROJECT, CS, Claim.TEST, mode=Mode.RUN),
    Tool("dotnet-test", DOTNET, ("test", "--list-tests"), PROJECT, CS, Claim.TEST, mode=Mode.LIST),
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
    Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--", "verify"), PROJECT, CS, Claim.BRIDGE, mode=Mode.VERIFY),
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "-l", "cisde"), NONE, CS, Claim.API, mode=Mode.QUERY),
    # Python/TypeScript API thunks emit Capture arrays like tree-sitter, not ApiSurface JSON.
    Tool("py-api", INPROC, ("py-api", "surface"), NONE, PY, Claim.API, mode=Mode.QUERY),
    Tool("py-api", INPROC, ("py-api", "member"), NONE, PY, Claim.API, mode=Mode.LIST),
    Tool("ts-api", INPROC, ("ts-api", "surface"), NONE, TS, Claim.API, mode=Mode.QUERY),
    Tool("ts-api", INPROC, ("ts-api", "member"), NONE, TS, Claim.API, mode=Mode.LIST),
    Tool("yak", DIRECT, ("yak", "build"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    # -- Bash (configured when the tools are available on the executing host) ----------------
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-d"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-w"), FILES, BASH, Claim.STATIC, mode=Mode.WRITE),
    # -- SQL (configured when the tools are available on the executing host) -----------------
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("sqlfluff", UV, ("sqlfluff", "fix", "--dialect", "postgres"), FILES, SQL, Claim.STATIC, mode=Mode.WRITE),
    Tool("squawk", UV, ("squawk",), FILES, SQL, Claim.STATIC),
    # -- Docs --------------------------------------------------------------------------------
    Tool("mmdc", PNPM, ("mmdc", "-a", ".artifacts/mermaid", "-q"), NONE, DOCS, Claim.DOCS),
    # -- Code (`ast-grep run` self-walks; tree-sitter queries receive routed files) ----------
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, mode=Mode.WRITE),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, mode=Mode.WRITE),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, PY, Claim.CODE, mode=Mode.QUERY),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, TS, Claim.CODE, mode=Mode.QUERY),
    # Ripgrep self-walks once; the Python tag is census-only and --language refines through rail globs.
    Tool(
        "ripgrep", DIRECT, ("rg", "--json", "-U", "--multiline-dotall", "-P", "--hidden", "--glob", "!.git"), NONE, PY, Claim.CODE, mode=Mode.CONTENT
    ),
)


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Select catalog rows for a claim and optional language.

    Returns:
        Sorted tool rows matching the requested claim and language.
    """
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AST_MATCHES", "CAPTURES", "CAPTURE_ENCODER", "RG_EVENT", "Capture", "TOOLS", "select"]
