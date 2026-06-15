"""Catalog Assay tool rows and structured-output decoders.

`TOOLS` owns runnable analysis surfaces; `select` is the public query shape. Decoder
constants parse ast-grep, tree-sitter, and ripgrep output into stable wire models.
"""

import msgspec

from tools.assay.core.model import Claim, Input, Language, Mode, Runner, Stage, Tool, ToolGroup


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
    """ast-grep JSON match row."""

    text: str = ""
    file: str = ""
    lines: str = ""
    replacement: str = ""
    range: _Range = msgspec.field(default_factory=_Range)


class Capture(msgspec.Struct, frozen=True, gc=False):
    """Tree-sitter capture row emitted by in-process queries."""

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
    """Ripgrep NDJSON event with wire `type` projected to `kind`."""

    kind: str = msgspec.field(default="", name="type")
    data: _RgData = msgspec.field(default_factory=_RgData)


# --- [TABLES] ---------------------------------------------------------------------------

AST_MATCHES = msgspec.json.Decoder(tuple[AstMatch, ...])
CAPTURES = msgspec.json.Decoder(tuple[Capture, ...])
CAPTURE_ENCODER = msgspec.json.Encoder()
RG_EVENT = msgspec.json.Decoder(RgEvent)

DIRECT, MODULE, UV, DOTNET, PNPM, INPROC = Runner.DIRECT, Runner.MODULE, Runner.UV, Runner.DOTNET, Runner.PNPM, Runner.INPROC
FILES, INCLUDE, PROJECT, SOLUTION, NONE, OWNED = (Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.NONE, Input.OWNED)
PY, TS, CS, BASH, SQL, DOCS = (Language.PYTHON, Language.TYPESCRIPT, Language.CSHARP, Language.BASH, Language.SQL, Language.DOCS)

TOOLS: tuple[Tool, ...] = (
    # --- [PYTHON]
    Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), OWNED, PY, Claim.STATIC),
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
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error", "tests/ast-grep/fail", "--no-color"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("py-analyzer", MODULE, ("tools.py_analyzer", "check", "--format", "json"), NONE, PY, Claim.STATIC),
    Tool("pytest", UV, ("pytest", "-m", "not benchmark"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN, groups=(ToolGroup.RUN_DEFAULT,)),
    Tool("pytest", UV, ("pytest", "--collect-only", "-q"), INCLUDE, PY, Claim.TEST, mode=Mode.LIST, groups=(ToolGroup.RUN_DEFAULT,)),
    Tool(
        "pytest-benchmark",
        UV,
        ("pytest", "-m", "benchmark", "--benchmark-only", "--benchmark-autosave", f"--benchmark-storage={BENCHMARK_STORAGE_URI}"),
        INCLUDE,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_BENCHMARK,),
    ),
    Tool(
        "coverage",
        UV,
        ("coverage", "run", "-m", "pytest", "-m", "not benchmark"),
        INCLUDE,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "coverage-json",
        UV,
        ("coverage", "json", "-o", ".artifacts/python/coverage/coverage.json"),
        NONE,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "coverage-xml",
        UV,
        ("coverage", "xml", "-o", ".artifacts/python/coverage/coverage.xml"),
        NONE,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "coverage-lcov",
        UV,
        ("coverage", "lcov", "-o", ".artifacts/python/coverage/coverage.lcov"),
        NONE,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "mutmut",
        UV,
        ("mutmut", "run"),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.MUTATION,
        groups=(ToolGroup.MUTATION,),
        timeout=3600.0,
        stage=Stage(
            root=".artifacts/python/mutmut/work",
            inputs=("pyproject.toml", ".gitignore", ".config/coverage-mutmut.ini", "tools/assay", "tests/python"),
            project=True,
        ),
        # The staged rcfile supplies relative_files=false; otherwise mutmut's covered-lines pass aborts before mutation.
        env=(("COVERAGE_RCFILE", ".config/coverage-mutmut.ini"),),
    ),
    # --- [TYPESCRIPT]
    Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), PROJECT, TS, Claim.STATIC, mode=Mode.BUILD),
    Tool("biome", PNPM, ("biome", "ci", "--files-ignore-unknown=true", "--colors=off", "--reporter=json"), NONE, TS, Claim.STATIC),
    Tool("biome", PNPM, ("biome", "check", "--write", "--files-ignore-unknown=true"), FILES, TS, Claim.STATIC, mode=Mode.WRITE),
    Tool("ast-grep-ts", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error"), FILES, TS, Claim.STATIC),
    Tool(
        "ast-grep-ts",
        PNPM,
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error", "tests/ast-grep/fail", "--no-color"),
        FILES,
        TS,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("vitest", PNPM, ("vitest", "run"), NONE, TS, Claim.TEST, mode=Mode.RUN),
    # --- [CSHARP]
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error", "--verify-no-changes"), INCLUDE, CS, Claim.STATIC),
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error"), INCLUDE, CS, Claim.STATIC, mode=Mode.WRITE),
    Tool("dotnet-restore", DOTNET, ("restore", "--locked-mode"), PROJECT, CS, Claim.STATIC, mode=Mode.RESTORE),
    # ArtifactScope supplies --artifacts-path; the static rail adds CspSarifDir for SARIF evidence rows.
    # fold() consumes SARIF as report detail, never as an exit-code substitute.
    Tool("dotnet-build", DOTNET, ("build", "--no-restore", "-tl:off", "-v:quiet", "/clp:ErrorsOnly"), PROJECT, CS, Claim.STATIC, mode=Mode.BUILD),
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
    # INPROC API thunks emit Capture rows, matching tree-sitter query output.
    Tool("py-api", INPROC, ("py-api", "surface"), NONE, PY, Claim.API, mode=Mode.QUERY),
    Tool("py-api", INPROC, ("py-api", "member"), NONE, PY, Claim.API, mode=Mode.LIST),
    Tool("ts-api", INPROC, ("ts-api", "surface"), NONE, TS, Claim.API, mode=Mode.QUERY),
    Tool("ts-api", INPROC, ("ts-api", "member"), NONE, TS, Claim.API, mode=Mode.LIST),
    Tool("yak", DIRECT, ("yak", "build"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    # --- [BASH]
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-d"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-w"), FILES, BASH, Claim.STATIC, mode=Mode.WRITE),
    # --- [SQL]
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("sqlfluff", UV, ("sqlfluff", "fix", "--dialect", "postgres"), FILES, SQL, Claim.STATIC, mode=Mode.WRITE),
    Tool("squawk", UV, ("squawk",), FILES, SQL, Claim.STATIC),
    # --- [DOCS]
    Tool("mmdc", PNPM, ("mmdc", "-q"), OWNED, DOCS, Claim.DOCS),
    # --- [CODE]
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, PY, Claim.CODE, groups=(ToolGroup.EMPTY_ON_EXIT1,)),
    Tool("ast-grep", PNPM, ("ast-grep", "run"), NONE, TS, Claim.CODE, groups=(ToolGroup.EMPTY_ON_EXIT1,)),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, PY, Claim.CODE, mode=Mode.QUERY),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, TS, Claim.CODE, mode=Mode.QUERY),
    # ripgrep self-walks the tree; the PY tag is census-only because rail globs narrow files at invocation.
    Tool(
        "ripgrep", DIRECT, ("rg", "--json", "-U", "--multiline-dotall", "-P", "--hidden", "--glob", "!.git"), NONE, PY, Claim.CODE, mode=Mode.CONTENT
    ),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Return deterministic catalog rows for one claim and optional language."""
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AST_MATCHES", "CAPTURES", "CAPTURE_ENCODER", "RG_EVENT", "Capture", "TOOLS", "select"]
