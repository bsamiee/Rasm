# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # Products by host or interface
├── libs/                     # Published packages by language
├── tests/                    # Suites per language, at the path of the project they test
├── tools/                    # Checks and grammars other checkers lack, by the tool that runs them
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
├── Package.swift             # Swift modules, executables, and remote package catalog
├── Xcode.xcconfig            # Build settings every Xcode project inherits
├── Cargo.toml                # Rust workspace and dependency catalog
├── rust-toolchain.toml       # Rust toolchain channel and components
├── sgconfig.yml              # ast-grep rule directories and language parsing
├── .editorconfig             # Editor, formatting, and analyzer settings by file path
├── .gitattributes            # Line endings, diff drivers, and LFS filters per extension
├── .gitignore                # Generated, secret, and machine-local paths
├── .github/                  # Workflows, issue forms, and jactionlint settings
├── .cargo/                   # Cargo build settings and compiler source link
├── .config/                  # Files tools find in .config/ or read through a path setting
├── .claude/                  # Claude Code knowledge and settings
├── .mcp.json                 # Claude Code MCP servers
├── .codex/                   # Codex knowledge and settings
├── .vscode/                  # VS Code settings and recommended extensions
├── CLAUDE.md                 # Agent standards, AGENTS.md is its symlink
└── README.md
```

## [02]-[FLOW]

Thick arrows supply tools and variables, solid arrows run, read, or write, and dotted arrows name a file by path:

```mermaid
flowchart LR
    versions["global.json<br/>rust-toolchain.toml"]
    mise["mise<br/>.config/mise.toml [tools] [env]"]
    shell(["Shell and child processes"])
    claude(["Claude Code session"])
    ci(["CI job"])
    hooks["plugins/function-hooks"]

    subgraph file["File level, takes paths"]
        hk(["hk check, hk fix"])
        steps[".config/hk.pkl steps"]
    end

    subgraph project["Project level, takes projects"]
        nx(["nx #lt;verb#gt; #lt;project#gt;"])
        plugins["nx.json plugin rows<br/>apps/nx/#lt;plugin#gt;"]
        manifests["Ecosystem manifests and locks"]
        build["Ecosystem build settings"]
    end

    subgraph config["Checker configuration"]
        found["Tool's own file<br/>at its default path"]
        named[".config/#lt;checker file#gt;"]
    end

    subgraph writes["Writes"]
        cache[(".cache/#lt;tool#gt;")]
        artifacts[(".artifacts/#lt;tool#gt;")]
    end

    mise -->|installs toolchains from| versions
    mise ==>|activation| shell
    mise ==>|SessionStart hook| claude
    mise ==>|jdx/mise-action| ci
    shell -->|runs| hk & nx
    ci -->|runs| nx
    claude -->|tool call| hooks
    hooks -->|runs, direct checker and build calls rewritten| hk & nx
    hk -->|runs| steps
    nx -->|runs targets of| plugins
    plugins -->|infer projects and targets from| manifests
    nx -->|tools read| build
    steps & plugins -->|tool finds| found
    steps & build & mise -.->|names by path| named
    hk -->|writes| cache
    nx -->|writes| cache & artifacts
```

## [03]-[PRINCIPLES]

- Each fact sits once in the file its tool reads, and every other use derives it
- Nx plugins infer every project, target, and dependency from manifests and tool output, with no `project.json`, `targetDefaults`, or `namedInputs`
- Manifest `nx` tables state only a path no manifest row or tool reports (a file a build reads unimported)
- Plugin rows of `nx.json` share the options under `pluginsConfig` `@rasm/nx-devkit`, and a row's own options win
- Verbs name one operation in every ecosystem, and a project answers a verb only where one of its tools performs that operation
- Verbs aggregate `<verb>--<tool>` targets, and one run's arguments go to the tool-named target (`nx run <project>:build--dotnet -- -bl`)
- File-level formatting and linting belong to hk, project-level checks to the Nx plugins, and neither calls the other
- Checkers report every finding as an error through their own configuration
- Checker configuration named by path sits in `.config/`, since tools run through hk and Nx alone, never directly
- Libraries are published packages: they take paths, names, and policies from their caller and hold no repository convention
- Caches sit under `.cache/<tool>` and generated output under `.artifacts/<tool>`, `dist` and editor-shared folders stay where their tool puts them
- Published apps read their identity from the host API (`Bundle.main`, `process.execPath`) and keep settings in their own preferences
- Apps and tools made here default cache, log, and runtime folders to per-user locations, one documented setting moves each, each bounds its logs
- `.config/mise.toml` `[env]` rows move the folders: caches to `.cache/<tool>`, logs under `.artifacts/logs` in a file or folder per tool
- Tool behavior comes from its source and docs at the installed version, a plugin never restates what a tool, host, package, or Nx already does
- Tools with their own incremental state keep it, Nx caches results around it and restores an intermediate folder only with the outputs built from it
- CI selects projects by tags: `os:<platform>` where a toolchain or host limits build platforms, `host:<technology>` where a build runs a host no runner holds

## [04]-[TASKS]

`nx <verb> <project>` runs one project, `nx affected -t <verb>` runs changed work, and `--files=<path>` limits it to one change's reach:
- `build` compiles or packages a project into its declared outputs
- `check` reports findings of checkers reading the project model and changes no source file
- `fix` applies fixes from checkers reading the project model
- `test` runs a project's test runners
- `serve` runs a project's development servers until the run ends, where its config holds the ready signal its Nx plugin exports
- `preview` builds a project, then serves its build output as `serve` does
- `install` puts a local build into its host or starts local services
- `deploy` applies a project's infrastructure stacks, `--stack=<name>` one of them
- `upgrade` moves an ecosystem root's dependency requirements and locks to newer releases through its own tool
- `clean` removes the outputs a project's targets declare and runs its tool's clean

Other tasks run through their own commands:
- `git submodule update --init --checkout assets` fills `assets/` before a build that reads a shared asset
- `nx build @rasm/nx-apple-darwin-arm64 -c release` builds the core `@rasm/nx-apple` reads Xcode projects through, on a Mac after a clone
- `hk check <paths>` and `hk fix <paths>` run every file-level checker and writer over the named paths
- Playwright `webServer` rows with `reuseExistingServer` and command `nx run <project>:<target>` start that target and wait for its address
- Workflow `release.yml` runs `nx release --skip-publish`, then per platform a `-c release` build of the versioned projects and `nx release publish`
- `mise bootstrap` registers the pitchfork supervisor as a login agent, on a Mac after a clone
- `nx install @rasm/infra` starts local services of `infra/compose.yaml`, only `-c store` adds or updates the store service test suites read
- `mise daemons start services` applies a changed `infra/compose.yaml` and waits until every service is healthy
- `mise daemons stop docker` stops the container engine until a session enters, `mise daemons start docker` starts it and waits until Docker answers
- `nx deploy @rasm/infra` applies the repository and deployed-host stacks
- `mise lock --bump` moves tools and `pinact run --update` moves workflow action pins to their newest release SHA
- `nx migrate latest`, then `nx migrate --run-migrations`, applies a new Nx release's migrations
- `nx sync` writes each executor's `schema.json` and `executors.json` description from its declared options schema, `nx sync:check` fails on a stale one
- `pixi run --locked uv sync` builds Python distributions against Pixi libraries
- `pixi run --locked python` supplies the library search path for `magic` and `weasyprint`

## [05]-[DELIVERY]

Packaging turns a build into the one file its host or store takes, and every channel delivers that file:
- Hosts and ecosystems decide package format (`.yak`, `.nupkg`, wheel, `.dmg`)
- Plugin technology decides a host's package and install form (UXP `.ccx`, CEP `.zxp`), never its store
- Installs serve development alone, replacing a previous build where its host loads it and skipping packaging where the host allows
- Releases version and tag a project and attach its packaged files to its GitHub release, the direct-download channel
- Release assets hold native files unwrapped, and a folder product (Unreal plugin, `.app`) goes in a `.zip` or `.dmg`
- Publishing pushes the same packaged file to its host's or ecosystem's official store
- GitHub releases are a primary channel for host plugins and apps, for libraries the registry is canonical and release assets optional

## [06]-[OWNERS]

| [INDEX] | [CONCERN]                      | [OWNER]                                                                                                |
| :-----: | :----------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | Tool binary                    | `.config/mise.toml` `[tools]`, exact versions in `.config/mise.lock`                                   |
|  [02]   | Process variable               | `.config/mise.toml` `[env]`, platform-only values in `.config/mise.<os>.toml`                          |
|  [03]   | Toolchain version              | `global.json` .NET, `rust-toolchain.toml` Rust, uv root `requires-python` Python, `xcode-select` Swift |
|  [04]   | Package version                | `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row   |
|  [05]   | .NET tool package              | `.config/mise.toml` `[tools]` `dotnet:<id>` row at `latest`, its command called by name                |
|  [06]   | Task graph                     | `nx.json` plugin rows, inference in `apps/nx/<plugin>`                                                 |
|  [07]   | Checker configuration          | Tool's own file, in `.config/` where a path names it, `pyproject.toml` `[tool.*]` for Python tools     |
|  [08]   | File-level check               | `.config/hk.pkl` step                                                                                  |
|  [09]   | Secret                         | Doppler `rasm` project and 1Password, read through the `secrets` skill                                 |
|  [10]   | Resource or repository setting | Typed row of an `infra/` program, applied by `nx deploy`                                               |
|  [11]   | Service definition             | `infra/compose.yaml`                                                                                   |
|  [12]   | Xcode build setting            | `Xcode.xcconfig`, per-product rows in the `.xcodeproj` target                                          |
|  [13]   | Swift package version          | Root `Package.swift` `dependencies` row, `.xcodeproj` `remote` row for a package only an app imports   |
|  [14]   | Rust package version           | `Cargo.toml` `[workspace.dependencies]` row                                                            |
|  [15]   | Agent harness plugin           | `plugins/<name>`                                                                                       |
|  [16]   | Native libraries for sdists    | Pixi `default` environment in `pyproject.toml`, exact builds in `pixi.lock`                            |
|  [17]   | Container runtime              | Mise clients, Colima on macOS                                                                          |
|  [18]   | Agent browser options          | `.config/mise.toml` `[env]` `PLAYWRIGHT_MCP_*` rows, `PLAYWRIGHT_CLI_SESSION` per agent session        |
|  [19]   | Reusable asset                 | `assets/<kind>/<name>/` in `bsamiee/Rasm-Assets`, with native relative dependencies                    |
|  [20]   | Backup target                  | Doppler `rasm/deploy` `BACKUP_*` rows, absent `BACKUP_KIND` runs no backup                             |
|  [21]   | Local daemon                   | `[daemons]` row in `.config/mise.<os>.toml`, started with the first shell or agent session             |

- Tool rows stay at `latest` where their backend lists releases, a `version_regex` over release tags excludes a development build `latest` resolves

## [07]-[STRUCTURE]

- Projects hold no `src/` directory
- Libraries sit at `libs/<language>/<category>/<package>/`, categories name the concern spanning their packages and siblings that field anticipates
- New categories start from the package being made, its sibling packages outside it, and the higher-order concern they share
- Packages are named by the term practitioners use for that concern
- Projects a host loads target the runtime and libraries their host bundles, and build output holds none of them
- Each uv root resolves one Python range: projects on a host's Python get their own root, and a package shared across roots declares the lower bound
- Swift modules are root `Package.swift` targets, each in its own folder under `libs/swift/<category>/`, executables under `apps/macos/`
- SQL schemas reconcile the live database to owning types, with no migration, history table, or versioned file or folder
- Reconciles drop or retype only what owning types declare retired or retyped
