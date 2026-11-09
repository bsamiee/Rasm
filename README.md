# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/
│   ├── blender/
│   ├── cli/
│   ├── creative-cloud/
│   ├── macos/
│   ├── nx/
│   ├── rhino/
│   ├── roslyn/
│   └── web/
├── libs/
│   ├── dotnet/
│   ├── python/
│   ├── rust/
│   ├── swift/
│   └── typescript/
├── tests/                    # Suites per language, at the path of the project they test
├── tools/
│   ├── ast-grep/             # Tree-sitter grammars and outline catalogs ast-grep lacks
│   ├── dylint/               # Rust lints reading types and resolved paths Clippy lacks
│   ├── mcp-proxy/            # rasm-mcp-proxy binary serving a stdio MCP server over Streamable HTTP
│   └── refurb/               # Python checks reading mypy types Ruff lacks
├── assets/                   # Reusable assets no one app or plugin owns, each app or plugin keeps its own assets in its folder
├── infra/                    # Pulumi programs for repository and hosting resources, Compose services
├── plugins/                  # Agent harness marketplace
├── docs/                     # Research sheets, ignored under docs/research
├── nx.json                   # One row per Nx plugin and the release policy
├── package.json              # Workspace root package
├── pnpm-workspace.yaml       # TypeScript workspace globs and dependency catalog
├── tsconfig.base.json        # TypeScript options every package compiles and emits with
├── tsconfig.web.json         # TypeScript options for bundled browser code
├── biome.json                # TypeScript and JSON formatting and lint
├── pyproject.toml            # Python workspace root, catalog groups, and checker settings
├── Workspace.slnx            # .NET solution listing every project
├── global.json               # .NET SDK and test runner
├── Directory.Packages.props  # .NET central package versions
├── Directory.Build.props     # .NET build defaults and artifacts output
├── Directory.Build.targets   # .NET items and Rhino host targets
├── NuGet.config              # NuGet sources
├── .globalconfig             # .NET analyzer settings for the whole compilation
├── Package.swift             # Swift modules, executables, and remote package catalog
├── Xcode.xcconfig            # Build settings every Xcode project inherits
├── Cargo.toml                # Rust workspace and dependency catalog
├── rust-toolchain.toml       # Rust toolchain channel and components
├── sgconfig.yml              # ast-grep rule directories and language parsing
├── .editorconfig             # Editor, formatting, and analyzer settings by file path
├── .gitattributes            # Line endings, diff drivers, and LFS filters per extension
├── .gitignore                # Generated, secret, and machine-local paths
├── .cargo/                   # Cargo build settings and compiler source link
├── .config/                  # Files tools find in .config/ or read through a path setting
├── .claude/                  # Claude Code knowledge and settings
├── .mcp.json                 # Claude Code MCP servers
├── .codex/                   # Codex knowledge and settings
├── .vscode/                  # VS Code settings and recommended extensions
├── CLAUDE.md                 # Agent standards, AGENTS.md is its symlink
└── README.md
```

## [02]-[PRINCIPLES]

- Each fact sits once in the file its tool reads, and every other use derives it
- Nx plugins infer every project and target from each ecosystem's own manifests and tool output, with no `project.json`, `targetDefaults`, or `namedInputs`
- Every project answers `build`, `check`, `fix`, and `test`; `install` puts a local build into its host, `deploy` applies an infrastructure stack, and `upgrade` moves an ecosystem's dependencies
- Verbs aggregate `<verb>--<tool>` targets, and one run's arguments go to the tool-named target (`nx run <project>:build--dotnet -- -bl`)
- File-level formatting and linting belong to hk, project-level checks to the Nx plugins, and neither calls the other
- Checkers report every finding as an error through their own configuration
- Libraries are published packages: they take paths, names, and policies from their caller and hold no repository convention
- Caches sit under `.cache/<tool>` and generated output under `.artifacts/<tool>`; a package's `dist` and a folder a tool shares with its editor stay where the tool puts them

## [03]-[TASKS]

- `git submodule update --init --checkout assets` fills `assets/` for font builds, Darkroom builds, and publishing
- `nx <verb> <project>` runs one project, `nx affected -t <verb>` runs changed work, and `--files=<path>` limits it to one change's reach
- `hk check <paths>` and `hk fix <paths>` run every file-level checker and writer over the named paths
- `nx release` versions, tags, writes GitHub releases, and publishes each selected project
- `mise lock --bump` moves tools, `nx migrate` moves Nx
- `pixi run --locked uv sync` builds Python distributions against Pixi libraries
- `pixi run --locked python` supplies the library search path for `magic` and `weasyprint`

## [04]-[DELIVERY]

Products reach a person through separate levels:
- Repository tooling serves development alone, installing builds over a host's previous install and running local services from the checkout
- Each product's versioned tag and GitHub release mark one release and hold a packaged product's files as an alternative install path
- Each host's install and update channel receives published products

## [05]-[OWNERS]

| [INDEX] | [CONCERN]                      | [OWNER]                                                                                              |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | Tool binary                    | `.config/mise.toml` `[tools]`, exact versions in `.config/mise.lock`                                 |
|  [02]   | Process variable               | `.config/mise.toml` `[env]`, platform-only values in `.config/mise.<os>.toml`                        |
|  [03]   | SDK version                    | `global.json` for .NET, `xcode-select` for Swift, `rust-toolchain.toml` for Rust                     |
|  [04]   | Package version                | `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row |
|  [05]   | .NET tool package              | `.config/mise.toml` `[tools]` `dotnet:<id>` row at `latest`, its command called by name              |
|  [06]   | Task graph                     | `nx.json` plugin rows, inference in `apps/nx/<plugin>`                                               |
|  [07]   | Checker configuration          | Tool's own file, `pyproject.toml` `[tool.*]` for every Python tool                                   |
|  [08]   | File-level check               | `.config/hk.pkl` step                                                                                |
|  [09]   | Secret                         | Doppler `rasm` project and 1Password, read through the `secrets` skill                               |
|  [10]   | Resource or repository setting | Typed row of an `infra/` program, applied by `nx deploy`                                             |
|  [11]   | Service definition             | `infra/compose.yaml`                                                                                 |
|  [12]   | Xcode build setting            | `Xcode.xcconfig`, per-product rows in the `.xcodeproj` target                                        |
|  [13]   | Swift package version          | Root `Package.swift` `dependencies` row, `.xcodeproj` `remote` row for a package only an app imports |
|  [14]   | Rust package version           | `Cargo.toml` `[workspace.dependencies]` row                                                          |
|  [15]   | Agent harness plugin           | `plugins/<name>`                                                                                     |
|  [16]   | Persistent local process       | Mise `[daemons]` row, platform-only rows in `.config/mise.<os>.toml`                                 |
|  [17]   | Python runtime                 | Mise defaults to 3.15, Blender roots pin 3.13                                                        |
|  [18]   | Native libraries for sdists    | Pixi `default` environment in `pyproject.toml`, exact builds in `pixi.lock`                          |
|  [19]   | Container runtime              | Mise clients, a `[daemons]` `docker` row per `.config/mise.<os>.toml`, Colima on macOS               |
|  [20]   | Agent browser options          | `.config/mise.toml` `[env]` `PLAYWRIGHT_MCP_*` rows                                                  |
|  [21]   | Reusable asset                 | `assets/<kind>/<name>/` in `bsamiee/Rasm-Assets`, with native relative dependencies                  |

- Tool rows stay at `latest`, a `version_regex` over release tags excludes a development build `latest` resolves

## [06]-[STRUCTURE]

- Libraries sit at `libs/<language>/<category>/<package>/`, categories name the concern spanning their packages and every sibling that field anticipates
- New categories start from the package being made, its sibling packages outside it, and the higher-order concern they share
- Packages are named by the term practitioners use for that concern
- Installed Rhino supplies host assemblies at runtime, build output holds none
- Swift modules are root `Package.swift` targets, each in its own folder under `libs/swift/<category>/`, executables under `apps/macos/`
- Projects hold no `src/` directory
- SQL schemas reconcile the live database to owning types, with no migration, history table, or versioned file or folder
- Reconciles drop or retype only what owning types declare retired or retyped
