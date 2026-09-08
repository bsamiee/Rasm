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
├── tools/                    # Tools built for checks
│   ├── ast-grep/             # Structural outlines, rules, rewrites, utilities, and tests per language
│   ├── dotnet/               # Roslyn analyzers for executables and plugin hosts
│   └── nx/                   # Nx plugin for language tags and packaging projects
├── mise.toml                 # Toolchain, resolution settings, and process environment
├── mise.unix.toml            # Process environment on Linux and macOS
├── nx.json                   # Task graph, caching, and change detection
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
├── .github/                  # Workflows and shared composite actions
├── .vscode/                  # Editor settings
├── .claude/                  # Plugin marketplace of skills, agents, and hooks
├── .mcp.json                 # MCP servers for the agent harness
├── .miserc.toml              # mise settings read before config discovery
├── .editorconfig             # Editor and analyzer settings per path
├── .yamllint.yaml            # YAML lint rules beside the formatter
├── .gitattributes
├── .gitignore
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools and tasks route configurable caches and outputs under `.cache/` and `.artifacts/`.

Nx runs every developer command as a target configured through `nx.json` and the root `package.json` `nx` field:
- `nx run rasm:lint <scope>...` and `nx run rasm:format <scope>...` run checkers or writers of every file kind in scope
- Scope tokens are kind words (`dotnet`, `python`, `typescript`, `shell`, `yaml`, `sql`) or paths, an empty scope is the tree
- `nx run-many -t <target> -p tag:language:<language>` runs one target across one language, `nx run <project>:<target>` one project
- `check` composes `lint` and `typecheck` at the root with `typecheck` and `test` per project through `dependsOn`
- `format` applies lint fixes and formatting, then reports remaining findings
- `upgrade` takes a language configuration
- Targets name the files their commands read as `inputs`, each tool version as a `runtime` input, and the files they write as `outputs`
- `nx affected -t <target> --files=<path>[,<path>]` runs a target over the files' owning projects and their dependents
- `nx run rasm:rewrite -- --filter='^<id>$' --error=<id> <path>` applies one rewrite rule's fixes across the path
- Root targets hold operations with no owning project, plugins infer every other target from manifests and packaging projects
- Repository settings and secrets are infrastructure code under `infra/`, applied through a root target and read from the secret store at run time

## [03]-[TOOLING]

Tools join through the manager of their kind, at the newest release with prereleases:

| [INDEX] | [KIND]                                                      | [OWNER]                                                              |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | Package code imports, or a Python or Node tool with a table | Language package manager with its lock, catalog, or central versions |
|  [02]   | Standalone binary a target, script, workflow, or agent runs | `mise.toml` `[tools]` at `latest` under the registry backend         |
|  [03]   | .NET tool package a target runs                             | `dotnet dnx <id>` on the command                                     |
|  [04]   | Native library a binding builds against                     | `eng/native/<library>/` manifest, provisioned and packaged by target |
|  [05]   | Runtime or SDK a host binds to one version                  | `global.json`, or a `mise.toml` pin with its reason                  |
|  [06]   | Tool no target, script, workflow, or agent runs             | Machine profile                                                      |

- Configuration sits in `pyproject.toml` `[tool.*]`, `.editorconfig`, or the shared root file the tool documents
- Settings a tool reads from no central file sit on its command
- Tools with no command form for their rules keep their own file beside their consumers
- Checkers join `lint` and writers join `format`
- Agents reach capabilities through the MCP server that covers them, the CLI form stays for the target that runs it

## [04]-[QUALITY]

Checker configuration is centralized, each language area passes its checks before a merge.

- .NET: Roslyn analyzers at `latest-all` with warnings as errors and code-style rules enforced during build
- `.editorconfig` holds .NET rule severity and configuration
- `Thinktecture.Runtime.Extensions.Analyzers` validates generated-type declarations and generated `Switch`/`Map` usage in every .NET project
- Python: `ruff`, `ty`, and `mypy` pass with zero warnings
- TypeScript: `biome check` passes and `tsc --build` compiles under strict settings
- SQL: `sqlfluff lint` passes under the Postgres dialect
- Spelling and YAML: `typos` and `yamllint` pass over the tree
- Formatting: `dotnet format`, `ruff format`, Biome, shfmt, yamlfmt, pg-formatter, and `typos --write-changes`
- YAML formatting and lint skip ast-grep snapshots and the pnpm lockfile
- Coverage and mutation score are reported, no threshold gates a merge
- Fix a failing check in code, or in an invalid rule, and leave checker severity as configured

## [05]-[HARNESS]

Hooks enforce behavior observable by engine events, and settings, skills, and prose govern the rest:

- Settings hold harness configuration, prose holds judgment
- Skills explain approach, agents execute steps and commands with an acceptance check, and memory records facts no file covers
- Put path-specific judgment in `.claude/rules/` with a `paths` glob and enforce its mechanical requirements with hooks
- `.claude/settings.json` holds the allow list with one entry per tool or server, no deny glob, and `mise env` hooks for the agent shell
- Memory uses one format and one fact per file
- Skills, agents, and hooks belong to plugins under the `.claude/plugins/` marketplace, new ones join the plugin of their subject
- Subjects with no owning plugin take a new directory at the marketplace root, a manifest entry, and an `enabledPlugins` line with the `@rasm` suffix
- Name rule families after the package they read and enforce them at `lint`

## [06]-[LIBRARIES]

Every `libs/` package is independently consumable and publishes a stable API.

- Packages reference siblings through declared dependencies
- Every dependency points to a lower-level package, the graph stays acyclic
- Python and TypeScript files declare their exports at the end
- Sibling packages share naming, result type, and boundary types

## [07]-[LANGUAGE_AREAS]

Each language area follows its ecosystem's conventions and releases independently.

- Language areas share one design approach to boundaries, errors, and immutability
- Each area derives module layout, naming, and API design from its language
- Each area builds and runs without another present

## [08]-[APPLICATIONS]

Each `apps/<name>/` is one product with its own host, lifecycle, and release.

- Each application depends on `libs/` and third-party packages
- One application spans as many languages and projects as its host requires
- Applications assemble workflows and compose configuration loading, dependencies, effect execution, and telemetry
- Host APIs stay inside the package named for that host or the application

## [09]-[CHANGE]

Changes replace structure in place, releases run per project from git tags through one dispatch workflow.

- Schema libraries apply the delta from the owning types to the live database at startup or from a command, with no migration file or history table
- Each project or fixed group gets a `<name>@<version>` tag, build tools read the version from the tag, and registries take trusted publishing
- No file states a version
- No package, namespace, route, contract, or directory has a version suffix or `v1` folder, a changed structure keeps its predecessor's name
- No compatibility shim, fallback reader, or deprecation period keeps a replaced structure alive, one commit holds the change and the removal
- Project files sit at the project root, with no `src/` directory at any depth and no directory that only adds a level of nesting
