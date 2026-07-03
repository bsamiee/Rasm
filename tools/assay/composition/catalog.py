"""Catalog Assay tool rows: the one total table of every spawned surface.

`TOOLS` owns every runnable analysis, build, probe, provisioning, packaging, and program surface; `select`
is the public query shape and `launch` is the single launcher-prefix speller. Command templates carry typed
holes (``{name}`` / ``{name*}``) that `ToolArgs.fill` weaves from each check's typed splice values; rails
never edit ``Tool.command``. Rows producing parseable output declare their diagnostics family through
`Tool.parser`; the wire decoders live in `tools.assay.diagnostics`.
"""

from tools.assay.composition.store import CS_ARTIFACT_ROOTS, PY_ARTIFACT_ROOTS, PY_COVERAGE_FILES
from tools.assay.core.model import Claim, Input, Language, Mode, Parser, Runner, Stage, Tool, ToolGroup


# --- [CONSTANTS] ------------------------------------------------------------------------

BENCHMARK_STORAGE_URI = f"file://{PY_ARTIFACT_ROOTS['benchmarks']}"
PROBE_TIMEOUT_S: float = 8.0
_PROVISION_TIMEOUT_S: float = 120.0
_PROVISION_WRITE_TIMEOUT_S: float = 300.0
_SCENARIO_TIMEOUT_S: float = 600.0
_MUTATION_TIMEOUT_S: float = 3600.0
_PYTHON_ABI_PROBE: str = "import sys, sysconfig; print(sys.implementation.cache_tag, sysconfig.get_config_var('Py_GIL_DISABLED') or 0)"
_ONNXRUNTIME_LIB_PROBE: str = 'test -n "${ONNXRUNTIME_LIB:-}" && test -e "$ONNXRUNTIME_LIB" && printf "present:%s\\n" "${ONNXRUNTIME_LIB##*/}"'

# --- [TABLES] ---------------------------------------------------------------------------

DIRECT, MODULE, UV, DOTNET, PNPM, INPROC = Runner.DIRECT, Runner.MODULE, Runner.UV, Runner.DOTNET, Runner.PNPM, Runner.INPROC
FILES, INCLUDE, PROJECT, SOLUTION, NONE, OWNED = (Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.NONE, Input.OWNED)
PY, TS, CS, BASH, SQL, DOCS = (Language.PYTHON, Language.TYPESCRIPT, Language.CSHARP, Language.BASH, Language.SQL, Language.DOCS)

TOOLS: tuple[Tool, ...] = (
    # --- [PYTHON]
    Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), OWNED, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check"), FILES, PY, Claim.STATIC, parser=Parser.RUFF),
    Tool("ruff", UV, ("ruff", "check", "--fix"), FILES, PY, Claim.STATIC, mode=Mode.WRITE, parser=Parser.RUFF),
    Tool("ruff-format", UV, ("ruff", "format", "--check"), FILES, PY, Claim.STATIC, parser=Parser.RUFF_FORMAT),
    Tool("ruff-format", UV, ("ruff", "format"), FILES, PY, Claim.STATIC, mode=Mode.WRITE, parser=Parser.RUFF_FORMAT),
    Tool("ty", UV, ("ty", "check", "--no-progress"), OWNED, PY, Claim.STATIC, parser=Parser.TY),
    Tool("mypy", UV, ("mypy", "--no-error-summary", "--hide-error-context", "--no-pretty"), OWNED, PY, Claim.STATIC, parser=Parser.MYPY),
    Tool("lint-imports", UV, ("lint-imports", "--cache-dir", ".cache/grimp"), OWNED, PY, Claim.STATIC),
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
    Tool("py-analyzer", MODULE, ("tools.py_analyzer", "check", "--format", "json"), NONE, PY, Claim.STATIC, parser=Parser.PY_ANALYZER),
    Tool(
        "pytest",
        UV,
        ("pytest", "-m", "not benchmark"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.RUN_DEFAULT,),
        empty_signature=(5, b""),
    ),
    Tool(
        "pytest",
        UV,
        ("pytest", "--collect-only", "-q"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.LIST,
        groups=(ToolGroup.RUN_DEFAULT,),
        empty_signature=(5, b""),
    ),
    Tool(
        "pytest-benchmark",
        UV,
        ("pytest", "-m", "benchmark", "--benchmark-only", "--benchmark-autosave", f"--benchmark-storage={BENCHMARK_STORAGE_URI}"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_BENCHMARK,),
        empty_signature=(5, b""),
    ),
    Tool(
        "coverage",
        UV,
        ("coverage", "run", "-m", "pytest", "-m", "not benchmark"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
        empty_signature=(5, b""),
    ),
    # patch=["subprocess"] forces parallel suffixed data files; the STAGE combine merges them before any report row reads.
    Tool(
        "coverage-combine",
        UV,
        ("coverage", "combine"),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.STAGE,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
        empty_signature=(1, b"No data to combine"),
    ),
    Tool(
        "coverage-json",
        UV,
        ("coverage", "json", "-o", PY_COVERAGE_FILES["json"]),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "coverage-xml",
        UV,
        ("coverage", "xml", "-o", PY_COVERAGE_FILES["xml"]),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "coverage-lcov",
        UV,
        ("coverage", "lcov", "-o", PY_COVERAGE_FILES["lcov"]),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.CLIENT,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
    ),
    Tool(
        "mutmut",
        UV,
        ("mutmut", "run", "--max-children={max_children}", "{scope*}"),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.MUTATION,
        groups=(ToolGroup.MUTATION,),
        timeout=_MUTATION_TIMEOUT_S,
        stage=Stage(
            root=PY_ARTIFACT_ROOTS["mutmut"],
            inputs=("pyproject.toml", ".gitignore", ".config/coverage-mutmut.ini", "tools/assay", "tests/python"),
            project=True,
        ),
        # The staged rcfile supplies relative_files=false; otherwise mutmut's covered-lines pass aborts before mutation.
        env=(("COVERAGE_RCFILE", ".config/coverage-mutmut.ini"),),
    ),
    # Lease-riding kill-rate gate over the staged mutmut cache; VERIFY keeps it off the MUTATION dispatch fan.
    Tool(
        "mutmut-gate",
        UV,
        ("python", "-m", "tools.assay.rails.mutation_gate"),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.VERIFY,
        groups=(ToolGroup.MUTATION,),
        stage=Stage(project=True),
    ),
    # --- [TYPESCRIPT]
    Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), PROJECT, TS, Claim.STATIC, mode=Mode.BUILD, parser=Parser.TSC),
    Tool(
        "biome", PNPM, ("biome", "ci", "--files-ignore-unknown=true", "--colors=off", "--reporter=json"), NONE, TS, Claim.STATIC, parser=Parser.BIOME
    ),
    Tool("biome", PNPM, ("biome", "check", "--write", "--files-ignore-unknown=true"), FILES, TS, Claim.STATIC, mode=Mode.WRITE, parser=Parser.BIOME),
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
    Tool("vitest", PNPM, ("vitest", "run"), NONE, TS, Claim.TEST, mode=Mode.RUN, empty_signature=(1, b"No test files found")),
    # --- [CSHARP]
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error", "--verify-no-changes"), INCLUDE, CS, Claim.STATIC, parser=Parser.CS_CONSOLE),
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error"), INCLUDE, CS, Claim.STATIC, mode=Mode.WRITE, parser=Parser.CS_CONSOLE),
    Tool("dotnet-restore", DOTNET, ("restore", "--locked-mode"), PROJECT, CS, Claim.STATIC, mode=Mode.RESTORE, parser=Parser.CS_CONSOLE),
    # ArtifactScope supplies --artifacts-path; the static rail fills {max_cpu} and the per-project {sarif_dir} hole,
    # whose value is also stamped onto the receipt as the typed SARIF-fold key. fold() consumes SARIF as report
    # detail, never as an exit-code substitute.
    Tool(
        "dotnet-build",
        DOTNET,
        ("build", "--no-restore", "-tl:off", "-v:minimal", "-maxCpuCount:{max_cpu}", "-p:CspSarifDir={sarif_dir}"),
        PROJECT,
        CS,
        Claim.STATIC,
        mode=Mode.BUILD,
        parser=Parser.CS_CONSOLE,
    ),
    # Analyzer-free, SARIF-free compile probe gating the C# FIX phase; VERIFY keeps it off the BUILD phase fan.
    Tool("dotnet-probe", DOTNET, ("build", "-p:RunAnalyzers=false", "-tl:off", "-v:quiet"), PROJECT, CS, Claim.STATIC, mode=Mode.VERIFY),
    # {flags*} carries the opt-in TRX evidence tail (--report-trx --results-directory <trx-dir>) the test rail splices per project.
    Tool(
        "dotnet-test",
        DOTNET,
        ("test", "--minimum-expected-tests", "1", "{filter*}", "{flags*}"),
        PROJECT,
        CS,
        Claim.TEST,
        mode=Mode.RUN,
        input_flag=("--project",),
    ),
    Tool("dotnet-test", DOTNET, ("test", "--list-tests", "{filter*}"), PROJECT, CS, Claim.TEST, mode=Mode.LIST, input_flag=("--project",)),
    # Stryker.NET runs from the empty staged work root: the config file pins mutation policy, {solution} anchors the
    # real tree, and {output} routes reports to the pre-created report root so no sandbox litter escapes .artifacts.
    Tool(
        "dotnet-stryker",
        DOTNET,
        (
            "tool",
            "run",
            "dotnet-stryker",
            "--",
            "--test-runner",
            "mtp",
            "--mutation-level",
            "Standard",
            "--config-file",
            ".config/stryker-config.json",
            "--solution",
            "{solution}",
            "--output",
            "{output}",
            "{scope*}",
        ),
        PROJECT,
        CS,
        Claim.TEST,
        mode=Mode.MUTATION,
        timeout=_MUTATION_TIMEOUT_S,
        stage=Stage(root=CS_ARTIFACT_ROOTS["stryker"], project=True),
        input_flag=("--test-project",),
    ),
    # Live bridge supervisor: the rail fills {binary} with the built apphost and {verb}/{argv*} with the wire call.
    Tool("rasm-bridge", DIRECT, ("{binary}", "{verb}", "{argv*}"), NONE, CS, Claim.BRIDGE, mode=Mode.VERIFY, timeout=_SCENARIO_TIMEOUT_S),
    Tool(
        "rasm-bridge-build",
        DOTNET,
        ("build", "-tl:off", "-v:quiet", "/clp:ErrorsOnly", "--configuration", "{configuration}", "{project}"),
        NONE,
        CS,
        Claim.BRIDGE,
        mode=Mode.BUILD,
    ),
    # ilspy port: version probe (CHECK), type-roster listing (QUERY), and member decompile (LIST) are three total rows.
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "--version"), NONE, CS, Claim.API, mode=Mode.CHECK),
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "-l", "cisde", "{assembly}"), NONE, CS, Claim.API, mode=Mode.QUERY),
    Tool(
        "ilspycmd",
        DOTNET,
        ("tool", "run", "ilspycmd", "--", "-t", "{fqn}", "--no-dead-code", "--no-dead-stores", "{assembly}"),
        NONE,
        CS,
        Claim.API,
        mode=Mode.LIST,
    ),
    # INPROC API thunks emit Capture rows, matching tree-sitter query output.
    Tool("py-api", INPROC, ("py-api", "surface"), NONE, PY, Claim.API, mode=Mode.QUERY),
    Tool("py-api", INPROC, ("py-api", "member"), NONE, PY, Claim.API, mode=Mode.LIST),
    Tool("ts-api", INPROC, ("ts-api", "surface"), NONE, TS, Claim.API, mode=Mode.QUERY),
    Tool("ts-api", INPROC, ("ts-api", "member"), NONE, TS, Claim.API, mode=Mode.LIST),
    # --- [PACKAGE]
    Tool(
        "dotnet-msbuild",
        DOTNET,
        ("msbuild", "{project}", "-p:Configuration={configuration}", "-p:Version={version}", "-p:YakVersion={version}", "{props*}", "-nologo"),
        NONE,
        CS,
        Claim.PACKAGE,
        mode=Mode.QUERY,
    ),
    Tool(
        "dotnet-build",
        DOTNET,
        ("build", "{project}", "-c", "{configuration}", "-p:Version={version}", "-v:quiet", "/clp:ErrorsOnly"),
        NONE,
        CS,
        Claim.PACKAGE,
        mode=Mode.BUILD,
    ),
    Tool("yak", DIRECT, ("{binary}", "build", "--platform", "{platform}", "--version", "{version}"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    Tool("yak", DIRECT, ("{binary}", "install", "{target}"), NONE, CS, Claim.PACKAGE, mode=Mode.DEPLOY),
    Tool("yak", DIRECT, ("{binary}", "push", "{flags*}", "{target}"), NONE, CS, Claim.PACKAGE, mode=Mode.PUBLISH),
    # --- [BASH]
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-d"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-w"), FILES, BASH, Claim.STATIC, mode=Mode.WRITE),
    # --- [SQL]
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("sqlfluff", UV, ("sqlfluff", "fix", "--dialect", "postgres"), FILES, SQL, Claim.STATIC, mode=Mode.WRITE),
    Tool("squawk", UV, ("squawk",), FILES, SQL, Claim.STATIC),
    # --- [DOCS]
    Tool("mmdc", PNPM, ("mmdc", "-q", "-i", "{input}", "-a", "{sink_dir}", "-o", "{sink}"), OWNED, DOCS, Claim.DOCS),
    # --- [CODE]
    Tool(
        "ast-grep",
        PNPM,
        ("ast-grep", "run", "-p", "{pattern}", "-l", "{language}", "--json=compact", "--no-ignore", "hidden", "{targets*}"),
        NONE,
        PY,
        Claim.CODE,
        groups=(ToolGroup.EMPTY_ON_EXIT1,),
    ),
    Tool(
        "ast-grep",
        PNPM,
        ("ast-grep", "run", "-p", "{pattern}", "-l", "{language}", "--json=compact", "--no-ignore", "hidden", "{targets*}"),
        NONE,
        TS,
        Claim.CODE,
        groups=(ToolGroup.EMPTY_ON_EXIT1,),
    ),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, PY, Claim.CODE, mode=Mode.QUERY),
    Tool("tree-sitter", INPROC, ("tree-sitter", "query"), FILES, TS, Claim.CODE, mode=Mode.QUERY),
    # ripgrep self-walks the tree; the PY tag is census-only because rail globs narrow files at invocation.
    Tool(
        "ripgrep",
        DIRECT,
        ("rg", "--json", "-U", "--multiline-dotall", "-P", "--hidden", "--glob", "!.git", "{globs*}", "-e", "{pattern}", "--", "{targets*}"),
        NONE,
        PY,
        Claim.CODE,
        mode=Mode.CONTENT,
    ),
    # --- [PROVISION]
    Tool(
        "forge-provision", DIRECT, ("forge-provision", "{flags*}", "{verb}"), NONE, PY, Claim.PROVISION, mode=Mode.RUN, timeout=_PROVISION_TIMEOUT_S
    ),
    Tool(
        "forge-provision",
        DIRECT,
        ("forge-provision", "{flags*}", "{verb}"),
        NONE,
        PY,
        Claim.PROVISION,
        mode=Mode.WRITE,
        timeout=_PROVISION_WRITE_TIMEOUT_S,
    ),
    Tool(
        "forge-python-abi",
        DIRECT,
        ("forge-scientific-env", "python3", "-c", _PYTHON_ABI_PROBE),
        NONE,
        PY,
        Claim.PROVISION,
        mode=Mode.RUN,
        timeout=_PROVISION_TIMEOUT_S,
    ),
    Tool(
        "forge-openblas",
        DIRECT,
        ("forge-scientific-env", "pkg-config", "--modversion", "openblas"),
        NONE,
        PY,
        Claim.PROVISION,
        mode=Mode.RUN,
        timeout=_PROVISION_TIMEOUT_S,
    ),
    Tool(
        "forge-onnxruntime-lib",
        DIRECT,
        ("forge-scientific-env", "sh", "-lc", _ONNXRUNTIME_LIB_PROBE),
        NONE,
        PY,
        Claim.PROVISION,
        mode=Mode.RUN,
        timeout=_PROVISION_TIMEOUT_S,
    ),
    # --- [PROBES_AND_PROGRAMS]
    Tool("git-head", DIRECT, ("git", "rev-parse", "--short", "HEAD"), NONE, PY, Claim.STATIC, mode=Mode.QUERY, timeout=PROBE_TIMEOUT_S),
    Tool("git-dirty", DIRECT, ("git", "status", "--porcelain"), NONE, PY, Claim.STATIC, mode=Mode.QUERY, timeout=PROBE_TIMEOUT_S),
    # Health-probe template: the health rail derives each launcher probe argv from `launch()` and fills {argv*}.
    Tool("tool-probe", DIRECT, ("{argv*}",), NONE, PY, Claim.STATIC, mode=Mode.QUERY, timeout=PROBE_TIMEOUT_S),
    # Automation Program actions: arbitrary argv runs through this one total row, never an ad-hoc Tool.
    Tool("program", DIRECT, ("{argv*}",), NONE, PY, Claim.STATIC, mode=Mode.RUN),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def launch(tool: Tool) -> tuple[str, ...]:
    """Project a row's full launcher prefix: runner prefix plus uv lock and dependency-group injection.

    The one launcher speller: argv composition and health version-probes both derive from it, so the
    ``uv run --locked`` semantics are never re-spelled.

    Returns:
        Launcher tokens preceding the row's command body.
    """
    match tool.runner:
        case Runner.UV:
            return ("uv", "run", "--locked", *(part for group in tool.uv_groups() for part in ("--group", group.value)))
        case Runner.MODULE:
            return ("uv", "run", "--locked", "python", "-m")
        case _:
            return tool.runner.prefix


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Return deterministic catalog rows for one claim and optional language."""
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBE_TIMEOUT_S", "TOOLS", "launch", "select"]
