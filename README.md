# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.
Root manifests hold every dependency version, one file owns each concern, every developer command is one Nx target calling one tool.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # One directory per application, projects in every language it needs
├── libs/                     # Independently consumable packages, one directory per language
├── tests/                    # Shared test support per language and suites outside libs/
├── eng/dotnet/               # Project referencing every central package row, the `upgrade` mover reads it
├── infra/                    # Pulumi program holding the repository settings and the secret store
├── tools/
│   ├── ast-grep/             # Outlines and rules per language
│   └── nx/                   # Nx plugin inferring a Python project per pyproject.toml
├── mise.toml                 # Tool binaries and the process environment
├── global.json               # .NET SDK version
├── nx.json                   # Task graph, named inputs, per-language target defaults, plugins
├── package.json              # Root package, development dependencies, root Nx targets
├── pnpm-workspace.yaml       # TypeScript workspace globs and the dependency catalog
├── pyproject.toml            # Python dependency groups and every Python tool table
├── Directory.Packages.props  # .NET central package versions
├── Directory.Build.props     # .NET build defaults and project classification by tree position
├── Directory.Build.targets   # .NET items, host references, and policy targets
├── NuGet.config              # NuGet source and package folder
├── Workspace.slnx            # .NET solution
├── tsconfig.base.json        # Compiler options every TypeScript project extends
├── tsconfig.json             # Root TypeScript project over configuration, tooling, and infrastructure
├── vitest.config.ts          # Vitest configuration each project config imports
├── biome.json                # TypeScript and JSON formatting and lint
├── sgconfig.yml              # ast-grep rules, parsers, and embedded languages
├── .editorconfig             # Editor settings and .NET analyzer severity
├── .yamllint.yaml            # YAML lint beside yamlfmt
├── .github/                  # ci.yml, codeql.yml, dependabot.yml, and the setup action
├── .claude/                  # Skills, agents, settings, and the plugin marketplace
├── .mcp.json                 # MCP servers of the agent harness
├── CLAUDE.md                 # Agent standards, AGENTS.md is its symlink
└── README.md
```

## [02]-[FLOW]

```text
mise.toml [tools] ───▶ node pnpm uv dotnet(global.json) ast-grep tree-sitter typos yamllint yamlfmt actionlint ...
mise.toml [env] ─────▶ every process: shell, Claude Bash through the settings hooks, MCP servers through mise exec

pnpm-workspace.yaml catalog ──▶ package.json ──▶ pnpm-lock.yaml
pyproject.toml groups ────────▶ uv.lock, .venv/bin on PATH
Directory.Packages.props ─────▶ eng/dotnet/Rasm.Catalog ──▶ rasm:restore

nx.json plugins ──▶ @nx/dotnet (csproj)  @nx/vitest (vitest.config.ts)  tools/nx/workspace.ts (pyproject.toml)
nx.json targetDefaults by tag:language:* ──▶ typecheck  test  check per project
package.json nx ──▶ rasm: restore  lint  format  typecheck  check  upgrade  browsers  skills  grammar  rules  rewrite  outline  up  refresh

nx run rasm:lint ──▶ typos  ast-grep scan  biome check  ruff check  yamllint  actionlint, one process each over the tree
nx run rasm:format ─▶ the writers of the same tools, then dotnet format
nx run-many -t check ──▶ typecheck and test of every project, nx affected -t check the projects a change touches
ci.yml ──▶ setup action ──▶ nx run rasm:check, nx affected -t check, nx run rasm:format with git diff --exit-code
```

## [03]-[TASKS]

- A target is one tool call, arguments on the command, configuration in the tool's own file
- `nx run rasm:lint` runs every checker, `nx run rasm:format` every writer, `nx run rasm:check` lint and root typecheck
- `nx run-many -t check` runs every project, `nx run <project>:<target>` one, `nx affected -t check` the changed ones
- `nx run rasm:upgrade` moves every catalog to its newest release, `--configuration <language>` one catalog
- `nx run rasm:rewrite -- --filter='^<id>$' <path>` applies one rule's fix across a path
- `targetDefaults` filtered by `tag:language:*` hold the per-language body of `typecheck` and `test`
- Root targets hold operations with no owning project, plugins infer every other target from a manifest
- Inputs name the files a tool reads and its version as `runtime`, outputs name the files it writes
- Caches and outputs sit under `.cache/` and `.artifacts/`, each tool relocated through its own setting
- `package.json` holds dependencies and root targets, `nx.json` the graph, a project's `tsconfig.json` extends the base, no `project.json`
- One file owns each concern, a mini config, wrapper, or alias beside an owner is wrong structure, corrected at the owner

## [04]-[OWNERS]

| [INDEX] | [CONCERN]                 | [OWNER]                                                                               |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | Tool binary               | `mise.toml` `[tools]` at `latest`, prereleases included                               |
|  [02]   | Process variable          | `mise.toml` `[env]`                                                                   |
|  [03]   | SDK version               | `global.json`                                                                         |
|  [04]   | Package version           | `pnpm-workspace.yaml` catalog, `pyproject.toml` group, `Directory.Packages.props` row |
|  [05]   | .NET tool package         | `dotnet dnx <id>` on the command                                                      |
|  [06]   | Task graph                | `nx.json`, root `package.json` `nx`                                                   |
|  [07]   | Checker configuration     | The tool's own file, `pyproject.toml` `[tool.*]` for every Python tool                |
|  [08]   | Secret                    | Doppler, read through `doppler run` around the command                                |
|  [09]   | Resource and repository setting | Typed row of the program under `infra/`, applied by `nx run rasm:up`            |
|  [10]   | Machine tool no target runs | The machine profile                                                                 |

Each package row carries a one-line purpose comment. Every other file holds section dividers alone. A fact is stated once in its owning file, another file names the owner.

## [05]-[QUALITY]

- .NET: Roslyn analyzers at `latest-all`, warnings as errors, code style enforced in build, severity in `.editorconfig`
- Python: `ruff`, `ty`, and `mypy` at zero findings
- TypeScript: `biome check` at zero findings, `tsc --build` under strict options
- Tree: `typos`, `yamllint`, `actionlint`, and the ast-grep rule families
- Writers: `dotnet format`, `ruff format`, Biome, yamlfmt, `typos --write-changes`
- A failing check is fixed in the code or in the rule, severity stays as configured

## [06]-[HARNESS]

- `.claude/settings.json` holds the allow list, one entry per tool or server, and the `mise env` hooks
- `.mcp.json` runs each stdio server under `mise exec`
- The `function-hooks` plugin refuses destructive git commands, shell waits, and a second config file beside its owner
- Skills hold knowledge of one subject, agents hold one role with its procedure and gate, memory holds facts no file covers
- Path-specific guidance sits in `.claude/rules/` with a `paths` glob
- `nx run rasm:browsers` installs the Chromium build the Playwright commands launch

## [07]-[STRUCTURE]

- Every `libs/` package is independently consumable, references siblings through declared dependencies, and points down an acyclic graph
- Each `apps/<name>/` is one product with its own host, spanning as many languages and projects as the host requires
- Rhino 9 and Grasshopper 2 on macOS are the hosts, `Directory.Build.*` classify projects and bind the host assemblies
- A project is its manifest at the project root: `.csproj` listed in `Workspace.slnx`, `package.json` beside `tsconfig.json`, `pyproject.toml`
- Python and TypeScript files declare their exports at the end
- A change replaces structure in place, one commit holds the change and the removal, the new structure keeps its predecessor's name
- Schema libraries apply the delta from the owning types to the live database, with no migration file or history table
