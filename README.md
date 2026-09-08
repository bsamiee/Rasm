# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows. Dependencies, tools, and hosts must run on macOS. Root manifests hold every dependency version.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # One directory per application
├── docs/                     # Durable documentation
├── libs/
│   ├── contracts/
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── tests/                    # Test policy, shared test support, and suites outside libs/
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── eng/                      # Shared automation and native packaging
│   ├── native/               # Version manifests and packaging projects per native library
│   └── scripts/              # Python automation that Nx targets invoke
├── infra/                    # Pulumi program for repository settings and the Doppler project
├── tools/                    # Tools the repository builds for its checks
│   ├── ast-grep/             # Structural outlines, rules, rewrites, utilities, and tests per language
│   ├── dotnet/               # Roslyn analyzers for executables and plugin hosts
│   └── nx/                   # Nx plugin for language tags and packaging projects
├── mise.toml                 # Toolchain, its resolution settings, and the process environment
├── mise.unix.toml            # Process environment on Linux and macOS
├── nx.json                   # Task graph, caching, and change detection across the workspace
├── NuGet.config              # NuGet sources and package source mapping
├── Directory.Build.props     # .NET build defaults every project imports
├── Directory.Build.targets   # .NET items and policy checks every project imports
├── Directory.Packages.props  # .NET central package versions
├── pyproject.toml            # Python dependency groups and tool configuration
├── pnpm-workspace.yaml       # TypeScript workspace and dependency catalog
├── package.json              # Root package with development dependencies and root Nx targets
├── tsconfig.base.json        # Base TypeScript compiler options for workspace projects
├── tsconfig.json             # Root TypeScript project over configuration, tooling, and infrastructure
├── biome.json                # TypeScript formatting and TypeScript/JSON lint rules
├── sgconfig.yml              # ast-grep rules, parsers, and embedded languages
├── vite.config.ts            # Vite configuration that app and library configs import
├── vitest.config.ts          # Vitest configuration each project config imports
├── stryker.config.json       # TypeScript mutation testing
├── stryker-config.json       # .NET mutation testing
├── Workspace.slnx            # .NET solution of library, application, and test projects
├── global.json               # .NET SDK version and test runner
├── .github/                  # Workflows with their shared composite actions
├── .vscode/                  # Editor settings
├── .claude/                  # Plugin marketplace with the skills, agents, and hooks, rules by path glob, local scratch
├── .mcp.json                 # MCP servers for the agent harness
├── .miserc.toml              # mise settings read before config discovery
├── .editorconfig             # Editor and analyzer settings per path
├── .shellcheckrc             # Shellcheck shell and check set
├── .yamlfmt.yaml             # YAML formatting settings
├── .yamllint.yaml            # YAML lint rules beside the formatter
├── .gitattributes
├── .gitignore
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools and tasks route configurable caches and outputs under `.cache/` and `.artifacts/`. Work directories a tool cannot relocate are ignored and hold no durable output.

Nx runs every developer command as a target configured through `nx.json` and the root `package.json` `nx` field:
- `nx run-many -t <target> -p tag:language:<language>` runs one target across one language, and `nx run <project>:<target>` runs one project
- `check` runs `lint`, `typecheck`, and `test` without changing source files
- `format` applies automatic lint corrections and formatting and reports remaining findings
- Root `lint`, `format`, and `check` take the directories named after `--` and run over the tree without them, and `upgrade` takes a language configuration
- No root target splits per path or per tool, and none of the four runs another
- `nx run rasm:rewrite -- --filter='^<id>$' --error=<id> <path>` applies the fixes of one rewrite rule across the path
- Root targets hold the operations with no owning project, and plugins infer every other target from the manifests and the packaging projects
- Repository settings and secrets are infrastructure code under `infra/`, applied through a root target and read from the secret store at run time

## [03]-[TOOLING]

Every tool joins the repository through the manager that owns its kind, at its newest release with prereleases, and states its configuration in the file its consumers read:

| [INDEX] | [KIND]                                                      | [OWNER]                                                                 |
| :-----: | :---------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | Package code imports, or a Python or Node tool with a table | Language package manager and its lock, the catalog, or central versions |
|  [02]   | Standalone binary a target, script, workflow, or agent runs | `mise.toml` `[tools]` at `latest` under the registry backend            |
|  [03]   | .NET tool package a target runs                             | `dotnet dnx <id>` on the command                                        |
|  [04]   | Native library a binding builds against                     | `eng/native/<library>/` manifest, provisioned and packaged by target    |
|  [05]   | Runtime or SDK a host binds to one version                  | `global.json`, or a `mise.toml` pin with its reason                     |
|  [06]   | Tool no target, script, workflow, or agent runs             | Machine profile                                                         |

- Configuration sits in `pyproject.toml` `[tool.*]`, `.editorconfig`, or the shared root file the tool documents
- Tools that read no shared file take a file of their own at the root, listed in the layout tree
- Checkers join `lint` and writers join `format`, each with its version in the target's runtime input
- Agents reach a capability through the MCP server that covers it, and the CLI form stays for the target that runs it

## [04]-[QUALITY]

Checker configuration is centralized, and each language area must pass its configured checks before a merge.

- .NET: Roslyn analyzers at `latest-all` with warnings as errors and code-style rules enforced during build
- `.editorconfig` holds .NET rule severity and configuration
- `Thinktecture.Runtime.Extensions.Analyzers` validates generated-type declarations and generated `Switch`/`Map` usage across every .NET project
- Python: `ruff`, `ty`, and `mypy` pass with zero warnings
- TypeScript: `biome check` passes and `tsc --build` compiles under strict settings
- SQL: `sqlfluff lint` passes under the Postgres dialect, and pg-formatter owns the layout
- Spelling and YAML: `typos` and `yamllint` pass over the tree
- Formatting: `dotnet format`, `ruff format`, Biome, shfmt, yamlfmt, pg-formatter, and `typos --write-changes`
- YAML formatting and lint cover authored files, excluding ast-grep snapshots and the pnpm lockfile to preserve their generators' format
- Coverage and mutation score are reported, and no threshold gates a merge
- Fix a failing check in code, or in the rule when the rule is demonstrably invalid, and leave checker severity as configured

## [05]-[HARNESS]

Hooks enforce behavior observable by engine events. Settings, skills, and prose govern other behavior, and automation records weaknesses as findings.

- Use settings for harness configuration, skills for agent procedures, and prose for judgment
- Put path-specific judgment in `.claude/rules/` with a `paths` glob and enforce its mechanical requirements with hooks
- `.claude/settings.json` holds the allow list with one entry per tool or server, no deny glob, and `mise env` hooks for the agent shell
- Skills explain approach, agents execute steps and commands with an acceptance check, and memory records facts no file covers
- Memory uses one format and one fact per file, and the editor creates, narrows, merges, deletes, and indexes records
- Every skill, agent, and hook belongs to a plugin under the `.claude/plugins/` marketplace, and a new one joins the plugin of its subject
- Subjects with no owning plugin take a new directory at the marketplace root, a manifest entry, and an `enabledPlugins` line with the `@rasm` suffix
- Name rule families after the package they read and enforce them at `lint`
- Findings come from every observable source, and the editor resolves a batch or a due guidance section

## [06]-[LIBRARIES]

Every `libs/` package is independently consumable and publishes a stable API.

- Packages reference sibling packages through declared package dependencies
- Every dependency points to a lower-level package, and the dependency graph stays acyclic
- Python and TypeScript files declare their exports at the end
- Sibling packages share naming, result type, and boundary types

## [07]-[LANGUAGE_AREAS]

Each language area follows its ecosystem's conventions and releases independently.

- Language areas share one design approach to boundaries, errors, and immutability
- Each area derives module layout, naming, and API design from its language
- Each area builds and runs without another language area present

## [08]-[APPLICATIONS]

Each `apps/<name>/` is one product with its own host, lifecycle, and release.

- Each application depends on `libs/` and third-party packages
- One application spans as many languages and projects as its host requires
- Applications assemble workflows and compose configuration loading, dependencies, effect execution, and telemetry
- Host APIs stay inside the package named for that host or inside the application

## [09]-[CHANGE]

Changes replace structure in place, and releases run per project from git tags through one dispatch workflow.

- Schema libraries apply the delta from the owning types to the live database at startup or from a command, with no migration file or history table
- Each project or fixed group gets a `<name>@<version>` tag, build tools read the version from the tag, and registries take trusted publishing
- No file states a version
- No package, namespace, route, contract, or directory has a version suffix or `v1` folder, and a changed structure keeps its predecessor's name
- No compatibility shim, fallback reader, or deprecation period keeps a replaced structure alive, and one commit holds the change and the removal
- No `src/` directory exists at any depth, a project's files sit at its root, and no directory exists only to add a level of nesting
