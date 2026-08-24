"""Catalog Assay tool rows: the one total table of every spawned surface.

`TOOLS` owns every runnable analysis, build, probe, provisioning, packaging, and program surface; `select`
is the public query shape and `launch` is the single launcher-prefix speller. Command templates carry typed
holes (``{name}`` / ``{name*}``) that `ToolArgs.fill` weaves from each check's typed splice values; rails
never edit ``Tool.command``. Rows producing parseable output declare their diagnostics family through
`Tool.parser`; the wire decoders live in `assay.diagnostics`.
"""

from assay.composition.store import PY_ARTIFACT_ROOTS, PY_COVERAGE_FILES
from assay.core.model import Claim, Input, Language, Mode, Parser, Runner, Stage, Tool, ToolGroup


# --- [CONSTANTS] ------------------------------------------------------------------------

BENCHMARK_STORAGE_URI = f"file://{PY_ARTIFACT_ROOTS['benchmarks']}"
PROBE_TIMEOUT_S: float = 8.0
# buf's rule-violation exit; every other non-zero buf exit is a tool failure the lane reads as FAULTED.
BUF_DEFECT_EXIT: int = 100
JSONSCHEMA_PLUGIN: str = "protoc-gen-jsonschema"
JSONSCHEMA_TEMPLATE: str = (
    '{"version":"v2","plugins":[{"local":"'
    + JSONSCHEMA_PLUGIN
    + '","out":".","opt":["target=json-strict-bundle"],"strategy":"all","include_imports":false}]}'
)
_CONTRACTS_TIMEOUT_S: float = 120.0
_CONTRACTS_GENERATE_TIMEOUT_S: float = 300.0
_BUF_ENV: tuple[tuple[str, str], ...] = (("BUF_CACHE_DIR", ".cache/buf"),)
_PROVISION_TIMEOUT_S: float = 120.0
_PROVISION_WRITE_TIMEOUT_S: float = 300.0
_SCENARIO_TIMEOUT_S: float = 600.0
_MUTATION_TIMEOUT_S: float = 3600.0
_PYTHON_ABI_PROBE: str = "import sys, sysconfig; print(sys.implementation.cache_tag, sysconfig.get_config_var('Py_GIL_DISABLED') or 0)"
_ONNXRUNTIME_LIB_PROBE: str = 'test -n "${ONNXRUNTIME_LIB:-}" && test -e "$ONNXRUNTIME_LIB" && printf "present:%s\\n" "${ONNXRUNTIME_LIB##*/}"'

# --- [TABLES] ---------------------------------------------------------------------------

DIRECT, UV, DOTNET, PNPM, INPROC = Runner.DIRECT, Runner.UV, Runner.DOTNET, Runner.PNPM, Runner.INPROC
FILES, INCLUDE, PROJECT, SOLUTION, NONE, OWNED = (Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.NONE, Input.OWNED)
PY, TS, CS, BASH, SQL, DOCS, PROTO = (
    Language.PYTHON,
    Language.TYPESCRIPT,
    Language.DOTNET,
    Language.BASH,
    Language.SQL,
    Language.DOCS,
    Language.PROTO,
)

TOOLS: tuple[Tool, ...] = (
    # --- [PYTHON]
    Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), OWNED, PY, Claim.STATIC),
    # Explicit file paths bypass the manifest's `extend-exclude` unless `--force-exclude` rides the row, so the lane honors the project's carve.
    Tool("ruff", UV, ("ruff", "check", "--force-exclude"), FILES, PY, Claim.STATIC, parser=Parser.RUFF),
    Tool("ruff", UV, ("ruff", "check", "--fix", "--force-exclude"), FILES, PY, Claim.STATIC, mode=Mode.WRITE, parser=Parser.RUFF),
    Tool("ruff-format", UV, ("ruff", "format", "--check", "--force-exclude"), FILES, PY, Claim.STATIC, parser=Parser.RUFF_FORMAT),
    Tool("ruff-format", UV, ("ruff", "format", "--force-exclude"), FILES, PY, Claim.STATIC, mode=Mode.WRITE, parser=Parser.RUFF_FORMAT),
    Tool("ty", UV, ("ty", "check", "--no-progress"), OWNED, PY, Claim.STATIC, parser=Parser.TY),
    Tool(
        "mypy",
        UV,
        ("mypy", "--cache-dir", ".cache/mypy", "--no-error-summary", "--hide-error-context", "--no-pretty"),
        OWNED,
        PY,
        Claim.STATIC,
        parser=Parser.MYPY,
    ),
    Tool("lint-imports", UV, ("lint-imports", "--cache-dir", ".cache/grimp"), OWNED, PY, Claim.STATIC),
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
        # chdir seats mutmut inside the staged package so it reads the member manifest's [tool.mutmut] and its
        # mutants/src insert makes the mutated `assay` shadow the venv editable — the src-layout injection contract.
        stage=Stage(
            root=PY_ARTIFACT_ROOTS["mutmut"],
            inputs=("pyproject.toml", ".gitignore", ".config/coverage-mutmut.ini", "tools/assay", "tests/python"),
            project=True,
            chdir="tools/assay",
        ),
        # The staged rcfile supplies relative_files=false; otherwise mutmut's covered-lines pass aborts before mutation.
        env=(("COVERAGE_RCFILE", "../../.config/coverage-mutmut.ini"),),
    ),
    # Lease-riding kill-rate gate over the staged mutmut cache; VERIFY keeps it off the MUTATION dispatch fan.
    Tool(
        "mutmut-gate",
        UV,
        ("python", "-m", "assay.rails.mutation_gate"),
        OWNED,
        PY,
        Claim.TEST,
        mode=Mode.VERIFY,
        groups=(ToolGroup.MUTATION,),
        stage=Stage(project=True),
    ),
    # --- [TYPESCRIPT]
    # The root tsconfig.json is the solution file (files: [] + references), so the lane drives the project graph
    # with --build; a `-p tsconfig.json --noEmit` form would typecheck the empty solution shell and green falsely.
    Tool("tsc", PNPM, ("tsc", "--build", "tsconfig.json", "--pretty", "false"), OWNED, TS, Claim.STATIC, mode=Mode.BUILD, parser=Parser.TSC),
    Tool(
        "biome",
        PNPM,
        ("biome", "ci", "--files-ignore-unknown=true", "--no-errors-on-unmatched", "--colors=off", "--reporter=json"),
        NONE,
        TS,
        Claim.STATIC,
        parser=Parser.BIOME,
    ),
    Tool("biome", PNPM, ("biome", "check", "--write", "--files-ignore-unknown=true"), FILES, TS, Claim.STATIC, mode=Mode.WRITE, parser=Parser.BIOME),
    Tool(
        "vitest",
        PNPM,
        ("vitest", "run"),
        NONE,
        TS,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.RUN_DEFAULT,),
        empty_signature=(1, b"No test files found"),
    ),
    # Coverage and bench each re-run vitest under their own config lane, so the default row yields when either is asked.
    Tool(
        "vitest",
        PNPM,
        ("vitest", "run", "--coverage"),
        OWNED,
        TS,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_COVERAGE,),
        empty_signature=(1, b"No test files found"),
    ),
    Tool(
        "vitest-bench",
        PNPM,
        ("vitest", "bench", "--run"),
        OWNED,
        TS,
        Claim.TEST,
        mode=Mode.RUN,
        groups=(ToolGroup.REQUIRES_BENCHMARK,),
        empty_signature=(1, b"No test files found"),
    ),
    Tool("vitest", PNPM, ("vitest", "list"), NONE, TS, Claim.TEST, mode=Mode.LIST, empty_signature=(1, b"No test files found")),
    # Root residency keeps `stryker.config.json` on auto-discovery; `{scope*}` carries the CHANGED lane's --mutate globs.
    Tool("stryker", PNPM, ("stryker", "run", "{scope*}"), OWNED, TS, Claim.TEST, mode=Mode.MUTATION, timeout=_MUTATION_TIMEOUT_S),
    # --- [DOTNET]
    # Nx owns the .NET graph alone: `@nx/dotnet` infers the node and edge set (its roster equals Workspace.slnx),
    # and nx.json refuses every inferred target whose job a claim below already owns. No row delegates through
    # `nx run-many`, because an inferred `nx:run-commands` target pins `--no-restore --no-dependencies`, a
    # project-directory cwd, and fixed `.artifacts/dotnet/{bin,obj}` outputs: it carries neither the leased
    # `--artifacts-path` closure (argv forwarding lands the build outside the declared outputs, so a cache hit
    # would restore a tree the scoped build never wrote) nor the locked restore, the per-check SARIF drop dir, or
    # the repo-root cwd the diagnostic fold keys anchor on. Nx keeps `build`/`watch`/`run` for the dev loop.
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
    # MTP contract: the SDK<->MTP handshake under `dotnet test` loses the run floor — `-- --minimum-expected-tests 1`
    # reports zero tests and exits 5 where the same argv under `dotnet run` passes the whole suite, proven live on a
    # built project — so the rows drive `dotnet run --project <csproj> -- <mtp args>`.
    # The test rail pins the full per-project tail — project, `--`, the minimum-tests floor, filter, and the opt-in
    # TRX/coverage flags — because tails append after the body and every MTP option must ride behind `--`.
    Tool("dotnet-test", DOTNET, ("run",), PROJECT, CS, Claim.TEST, mode=Mode.RUN, input_flag=("--project",)),
    Tool("dotnet-test", DOTNET, ("run",), PROJECT, CS, Claim.TEST, mode=Mode.LIST, input_flag=("--project",)),
    # Stryker.NET discovers the project under test from the cwd, so the row runs at the repo root — a staged empty
    # work root refuses with "No .csproj found" before any mutant exists (proven live). Policy rides the root
    # stryker-config.json; --output routes reports to the pre-created .artifacts root; the .stryker-tmp sandbox is
    # cwd-relative with no relocation option upstream, so .gitignore carries that one row.
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
            "{config}",
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
    # --disable-updatecheck keeps the automated loop off the network and the "not using the latest" nag out of stderr;
    # {refs*} splices -r reference-path pairs so base types outside the decompiled assembly resolve with full fidelity.
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "--disable-updatecheck", "--version"), NONE, CS, Claim.API, mode=Mode.CHECK),
    Tool(
        "ilspycmd",
        DOTNET,
        ("tool", "run", "ilspycmd", "--", "--disable-updatecheck", "-l", "cisde", "{assembly}"),
        NONE,
        CS,
        Claim.API,
        mode=Mode.QUERY,
    ),
    Tool(
        "ilspycmd",
        DOTNET,
        (
            *("tool", "run", "ilspycmd", "--", "--disable-updatecheck", "-t", "{fqn}"),
            *("--no-dead-code", "--no-dead-stores", "{langversion*}", "{refs*}", "{assembly}"),
        ),
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
    Tool(
        "validate-mermaid",
        DIRECT,
        ("uv", "run", "--no-project", ".claude/skills/mermaid-diagramming/scripts/validate_mermaid.py", "--json", "{input}"),
        OWNED,
        DOCS,
        Claim.DOCS,
    ),
    Tool(
        "prose-gate",
        DIRECT,
        ("uv", "run", "--no-project", ".claude/skills/docgen/scripts/prose_gate.py", "--json", "{input}"),
        OWNED,
        DOCS,
        Claim.DOCS,
    ),
    # INPROC planning-marker gate: card leaders/statuses/bullets and terminal RESEARCH sections over libs/ planning durables.
    Tool("planning-gate", INPROC, ("planning-gate", "check"), OWNED, DOCS, Claim.DOCS),
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
    # --- [CONTRACTS]
    # buf is the one driver, run from the repo root against the libs/contracts workspace input: lint/format are executable gate lanes
    Tool(
        "buf-lint",
        PNPM,
        ("buf", "lint", "libs/contracts", "--error-format", "json"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        timeout=_CONTRACTS_TIMEOUT_S,
        parser=Parser.BUF,
        defect_exit=BUF_DEFECT_EXIT,
        env=_BUF_ENV,
    ),
    # The estate module path alone: vendored publisher bytes are never graded, and buf format has shipped
    # non-idempotent releases, so the lane diffs and never writes.
    Tool(
        "buf-format",
        PNPM,
        ("buf", "format", "--diff", "--exit-code", "libs/contracts/proto"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        timeout=_CONTRACTS_TIMEOUT_S,
        defect_exit=BUF_DEFECT_EXIT,
        env=_BUF_ENV,
    ),
    Tool(
        "buf-module",
        PNPM,
        ("buf", "registry", "module", "info", "{input}", "--format", "json"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.QUERY,
        timeout=_CONTRACTS_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    # Publish custody: the module's default label resolves to one immutable commit before the push and again after it.
    Tool(
        "buf-baseline",
        PNPM,
        ("buf", "registry", "module", "commit", "resolve", "{input}", "--format", "json"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.QUERY,
        timeout=_CONTRACTS_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    Tool(
        "buf-build",
        PNPM,
        ("buf", "build", "libs/contracts", "-o", "{output}", "--as-file-descriptor-set"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.QUERY,
        timeout=_CONTRACTS_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    Tool(
        "buf-generate",
        PNPM,
        ("buf", "generate", "libs/contracts", "--template", "libs/contracts/buf.gen.yaml", "-o", "{output}"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.STAGE,
        timeout=_CONTRACTS_GENERATE_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    Tool(
        "buf-push",
        PNPM,
        ("buf", "push", "libs/contracts", "--exclude-unnamed", "{flags*}", "--label", "{target}"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.PUBLISH,
        timeout=_CONTRACTS_GENERATE_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    Tool(
        "buf-jsonschema",
        PNPM,
        ("buf", "generate", "{input}", "--template", JSONSCHEMA_TEMPLATE, "-o", "{output}", "--type", "{fqn}"),
        OWNED,
        PROTO,
        Claim.CONTRACTS,
        mode=Mode.STAGE,
        timeout=_CONTRACTS_GENERATE_TIMEOUT_S,
        env=_BUF_ENV,
    ),
    # INPROC gates: plugin resolution over the template's binaries, the corpus audit over manifest, schemas, disk,
    # anchors, rosters, and descriptors, and the scratch-vs-committed freshness diff.
    Tool("plugin-probe", INPROC, ("plugin-probe", "resolve"), OWNED, PROTO, Claim.CONTRACTS, mode=Mode.VERIFY),
    Tool("corpus-gate", INPROC, ("corpus-gate", "check"), OWNED, PROTO, Claim.CONTRACTS),
    Tool("freshness-gate", INPROC, ("freshness-gate", "diff"), OWNED, PROTO, Claim.CONTRACTS, mode=Mode.QUERY),
    # The writer leg validates and transactionally commits the complete staged package/schema image.
    Tool("corpus-emit", INPROC, ("corpus-emit", "write"), OWNED, PROTO, Claim.CONTRACTS, mode=Mode.WRITE),
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

__all__ = ["BUF_DEFECT_EXIT", "JSONSCHEMA_PLUGIN", "JSONSCHEMA_TEMPLATE", "PROBE_TIMEOUT_S", "TOOLS", "launch", "select"]
