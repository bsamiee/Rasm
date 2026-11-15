# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # Products by host or interface
│   ├── blender/
│   ├── cli/
│   ├── creative-cloud/       # Software for Adobe apps, by host app, and products spanning several hosts
│   ├── macos/
│   ├── mcp/
│   ├── nx/                   # Nx plugins
│   ├── rhino/                # Rhino and Grasshopper 2 plug-ins
│   ├── roslyn/               # C# analyzers and code fixes
│   ├── unreal/               # Unreal projects and plug-ins
│   └── web/
├── libs/                     # Packages by language, then category
│   ├── cpp/
│   ├── dotnet/
│   ├── python/
│   ├── rust/
│   ├── swift/
│   └── typescript/
├── tests/                    # Test support, end-to-end, and workspace suites per language
├── tools/                    # Checks and grammars other checkers lack, by the tool that runs them
│   ├── ast-grep/             # Grammars, rules, and outline support per language
│   ├── dylint/               # Rust lints reading types and resolved paths Clippy lacks
│   ├── refurb/               # Python checks reading mypy types Ruff lacks
│   └── tests/                # dylint and refurb check suites
├── assets/                   # Reusable assets no app or plugin owns
├── infra/                    # Pulumi programs for the repository, its machines, and app hosting
├── plugins/                  # Agent harness marketplace
├── docs/                     # Machine, service, workflow, and TypeScript architecture facts
├── nx.json                   # Nx plugin rows and release policy
├── package.json              # Workspace root manifest
├── pnpm-workspace.yaml       # pnpm workspace globs, catalog, and settings
├── vitest.config.ts          # Vitest projects of every TypeScript suite without its own config
├── tsconfig.json             # Solution references to every composite project, written by nx sync
├── tsconfig.base.json        # TypeScript options every package compiles and emits with
├── tsconfig.tests.json       # TypeScript program checking root vitest.config.ts
├── biome.json                # TypeScript and JSON formatting and lint
├── pyproject.toml            # uv workspace root, dependency groups, Pixi environment, and checker settings
├── Workspace.slnx            # .NET solution listing every project
├── global.json               # .NET SDK and test runner
├── Directory.Packages.props  # .NET Central Package Management versions
├── Directory.Build.props     # .NET build defaults and artifacts output
├── Directory.Build.targets   # .NET items and Rhino SDK references
├── NuGet.config              # NuGet sources
├── Package.swift             # Swift modules, executables, and package dependencies
├── Package.resolved
├── Xcode.xcconfig            # Build settings every Xcode project inherits
├── Cargo.toml                # Cargo workspace members and workspace dependencies
├── rust-toolchain.toml       # Rust toolchain channel and components
├── CMakeLists.txt            # C++ root project finding each package by its CMakeLists.txt
├── CMakePresets.json         # C++ configure, build, test, package, and workflow presets
├── vcpkg.json                # C++ library features, registry baseline, and overlay triplets
├── sgconfig.yml              # ast-grep rule directories and language parsing
├── .editorconfig             # Editor, formatting, and analyzer settings by file path
├── .gitattributes            # Line endings, diff drivers, and LFS filters per extension
├── .gitignore                # Generated, secret, and machine-local paths
├── .gitmodules               # Submodule assets/, cloned only on request
├── .github/                  # Workflows, composite actions, issue forms, and jactionlint settings
├── .cargo/                   # Cargo build settings and compiler source link
├── .config/                  # Tool configuration found there or named by path
├── .claude/                  # Claude Code knowledge and settings
├── .mcp.json                 # Claude Code MCP servers
├── .codex/                   # Codex knowledge and settings
├── .agents/                  # Skills Codex and Antigravity read, Antigravity hooks
├── .vscode/                  # VS Code settings and recommended extensions
├── CLAUDE.md                 # Agent standards, linked as AGENTS.md
├── LICENSE
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
        nx(["nx #lt;target#gt; #lt;project#gt;"])
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
    mise ==>|user SessionStart hook| claude
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
- Each ecosystem pins every version, root tooling included, in its root catalog and lock
- Targets build from the committed lock of their ecosystem
- Tool configuration sits in `.config/` where a flag, variable, or setting names its path, else at the path its tool fixes
- Checkers report every finding as an error through their own configuration
- File-level formatting and linting belong to hk for speed, project-level work to Nx plugins, and neither calls the other
- Commits run hk on their message alone, and file checks run when a shell or agent calls hk
- Nx plugins are publishable packages built on public Nx plugin APIs, serving any workspace of their ecosystem
- Libraries and plugins take paths, names, and policies from their caller and hold no repository convention
- Plugin options default to their tool's behavior, and `nx.json` plugin rows set only what departs from it
- Tool behavior comes from its source and docs at the installed version
- Plugins add only what no tool, host, package, or Nx does
- Plugins infer every project, target, and dependency from manifests and tool output, with no `project.json` or hand-written `namedInputs`
- Manifest `nx` tables state only a fact no manifest row or tool reports (a service a client calls, a file a build reads unimported, tag `os:all`)
- Plugins reference their own targets alone
- Each target name means one operation in every ecosystem, and a plugin offers it where its tool performs that operation
- Each target runs its `<target>--<tool>` targets, and one run's arguments go to the tool target (`nx run <project>:build--dotnet -- -bl`)
- Aggregate and no-op targets declare the inputs of the targets they run, as `nx affected` selects a target by its own inputs
- Tools with their own incremental state keep it, Nx caches results around it and restores an intermediate folder only with the outputs built from it
- Task inputs hold only the files a task's tool reads, and `externalDependencies` names the packages it loads, `[]` where it loads none
- Tasks of an edited executor run once with `--skip-nx-cache`, as the cache replays results the old executor produced
- Targets with output that differs by platform hash one platform input, as one remote cache serves CI and this Mac
- `.config/mise.toml` `[env]` rows move caches to `.cache/<tool>`, logs to `.artifacts/logs`, and generated output to `.artifacts/<tool>`
- `dist`, `.cloudflare/output`, and editor folders stay where their tool writes them
- Apps and tools made here keep cache, log, and runtime folders in per-user locations, each moved by one documented setting, and bound their logs
- Published apps read their identity from the host API (`Bundle.main`, `process.execPath`) and keep settings in their own preferences
- Hosts are the applications that load a project, and SDKs and packages build, check, and package host projects with no host installed
- Targets running a host exist only on a machine holding it
- Unreal Engine is both host and the only build SDK, so Unreal targets exist where the engine a `.uproject` names is installed
- C++ toolchain, presets, CI, and release stay whole for packages other projects and Unreal plug-ins import, before any package exists
- Workflows hold triggers, runner choice, and credentials, and run the `nx` commands a shell runs
- Repository code declares machines, services, and how they connect, and provider accounts, tailnet addresses, and secret values stay in their service
- CI, cache, and machine changes start from `docs/` and the documented behavior of Nx, GitHub Actions, and each service

## [04]-[TASKS]

hk lints and formats files; Nx runs project tasks

`nx <target> <project>` runs one project, `nx affected -t <target>` runs changed work, and `--files=<path>` limits it to one change's reach:
- `codegen` writes source code a project's tools derive from its schemas and configuration
- `build` compiles or packages a project into its declared outputs
- `pack` writes the files a release holds where they differ from the build
- `attach` uploads the files a project's packaging targets wrote to the GitHub release of its version
- `check` reports findings of checkers reading the project model
- `fix` applies fixes from checkers reading the project model
- `test` runs a project's test runners
- `serve` runs a project's development servers until the run ends, and starts the Compose services it holds, which stay up
- `preview` builds a project, then serves its build output as `serve` does
- `install` puts a local build into its host or starts local services
- `deploy` applies a project's infrastructure stacks, and `-c release` those of a released version
- `upgrade` moves an ecosystem root's dependency requirements and locks to newer releases through its own tool
- `clean` removes the outputs a project's targets declare and runs its tool's clean

Other tasks run through their own commands:
- `hk check <paths>` and `hk fix <paths>` run every file-level checker and writer over the named paths
- `mise bootstrap` after a clone installs mise tools and pnpm packages and registers pitchfork as a Mac login agent
- `nx build @rasm/nx-apple-darwin-arm64` builds the core `@rasm/nx-apple` reads Xcode projects through, on a Mac after a clone
- `mise deps` syncs pnpm packages and each uv root from its lock where its inputs changed, the workspace root through Pixi libraries, and `mise exec` and `mise run` sync first
- `pixi run --locked python` supplies the library search path for `magic` and `weasyprint`
- `nx sync` writes executor schemas and tsconfig `references`, task runs in a terminal apply it, and runs in other shells stop until it runs
- `nx migrate latest`, then `nx migrate --run-migrations`, applies a new Nx release's migrations
- `@rasm/nx-playwright` infers `test--playwright` per Playwright config, which starts the `nx run` task of each `webServer` row with `reuseExistingServer` and waits until it is ready
- `mise daemons stop docker` stops the container engine until a session enters, `mise daemons start docker` starts it and waits until Docker answers
- `nx install @rasm/infra` runs the proxy, sign-in, and object storage containers of role `Apps` on the local Docker engine, under `rasm.localhost`
- `nx deploy @rasm/infra` applies the repository and machine stacks
- `nx run <project>:deploy--pulumi --stack=<name>` applies one stack, and `--operation=preview` prints the changes an apply makes
- `nx release --skip-publish`, `nx run-many -t attach -c release`, then `nx release publish` run a release
- `doppler run --config release --command 'nx pack <app>'` packages a Mac app with the release credentials

## [05]-[TESTING]

Projects take tag `os:<platform>` only where their toolchain or target framework limits where they build or test:
- Rhino test projects keep their test target on Linux
- Suites sit in a `tests/` folder beside the projects they test, each named for its project
- Logic needing no host sits in projects tested on every platform
- C++ projects take one Nx configuration per CMake workflow preset (`nx test <project> -c asan`), the first preset being the default
- `nx test <project> -c coverage` writes coverage to option `coveragePath`, else each tool's own report, and plain `nx test` stays uninstrumented

## [06]-[DELIVERY]

Installs, releases, and channels each deliver a product, releases and channels from the files packaging writes:
- Installs serve development alone, replacing a previous build where its host loads it and skipping packaging where the host allows
- Releases version each release project on its own through `nx release`, from its conventional commits, and give its dependents a patch
- Release tags take the Nx independent default `{projectName}@{version}` (`Arches@1.2.0`)
- Each release tag gets its own GitHub release, holding every file its project packages
- Attach, publish targets, and the Sparkle feed act on projects tagged at the released commit, and a project without a tag there uploads nothing
- Untagged projects read their version from their manifest and their changelog from their first commit, with no `--first-release` flag
- Release projects are those a plugin tags `release`, each versioned and attached, and its registry's publish setting alone gives it a publish target
- Publish settings are `package.json` `private`, `Cargo.toml` `publish`, classifier `Private :: Do Not Upload`, `NuGet.config` `defaultPushSource`
- Channels (registries, stores, the Sparkle feed) take published projects through their own command
- Every release step stays built, and a channel lacking its credential skips with success until the credential is set
- Store portals without an upload API (Fab, Creative Cloud Marketplace) take a store-ready file from the GitHub release by hand
- Package form follows plugin technology and ecosystem (UXP `.ccx`, CEP `.zxp`), whatever channel the package reaches
- Release assets hold native files unwrapped, and a product that is a folder (`.app`, Unreal plug-in) goes in a `.zip` or `.dmg`
- C++ packages install a CPS file and an SPDX SBOM with their files, and pack that tree as a CPack archive named by project, version, and system
- Unreal plug-ins release one source archive per engine, holding their installed packages and that `EngineVersion`, the form Fab takes

## [07]-[OWNERS]

| [INDEX] | [CONCERN]                      | [OWNER]                                                                                                |
| :-----: | :----------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | Tool binary                    | `.config/mise.toml` `[tools]`, exact versions in `.config/mise.lock`                                   |
|  [02]   | .NET tool package              | `.config/mise.toml` `[tools]` `dotnet:<id>` row at `latest`, its command called by name                |
|  [03]   | Process variable               | `.config/mise.toml` `[env]`, a platform value through `os()` in its row                                  |
|  [04]   | Toolchain version              | `global.json` .NET, `rust-toolchain.toml` Rust, uv root `requires-python` Python, `xcode-select` Swift |
|  [05]   | Package version                | `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row   |
|  [06]   | Swift package version          | Root `Package.swift` `dependencies` row, `.xcodeproj` `remote` row for a package only an app imports   |
|  [07]   | Rust package version           | `Cargo.toml` `[workspace.dependencies]` row                                                            |
|  [08]   | C++ toolchain and packages     | `github:llvm/llvm-project` row of `.config/mise.toml`, `vcpkg.json` rows and `builtin-baseline`        |
|  [09]   | Native libraries for sdists    | Pixi `default` environment in `pyproject.toml`, exact builds in `pixi.lock`                            |
|  [10]   | Unreal Engine                  | `EngineAssociation` of a `.uproject`, found in Epic install lists                                      |
|  [11]   | Xcode build setting            | `Xcode.xcconfig`, per-product rows in the `.xcodeproj` target                                          |
|  [12]   | Task graph                     | `nx.json` plugin rows, inference in `apps/nx/<plugin>`                                                 |
|  [13]   | Checker configuration          | Tool's own file, `pyproject.toml` `[tool.*]` for Python tools                                          |
|  [14]   | File-level check               | `.config/hk.pkl` step                                                                                  |
|  [15]   | Secret                         | Doppler `rasm` project and 1Password, read through the `secrets` skill                                 |
|  [16]   | Registry token                 | Doppler `rasm/release` row named as the variable its publish target reads (`NODE_AUTH_TOKEN`)          |
|  [17]   | Mac app release format         | Doppler `rasm/release` `ASC_API_*` rows, set for a notarized `.dmg`, absent for a `.zip`               |
|  [18]   | Resource or repository setting | Typed row of an `infra/` program, applied by workflow `Deploy`                                         |
|  [19]   | Machine                        | Row of table `hosts` in `infra/platform-stack.ts`                                                      |
|  [20]   | Service definition             | Role of a `hosts` row in `infra/platform-stack.ts`, rows of an app's `hosting.config.ts`               |
|  [21]   | Container runtime              | Mise clients, Colima on macOS                                                                          |
|  [22]   | Local daemon                   | `[daemons]` row in `.config/mise.toml`, started with the first shell or agent session                  |
|  [23]   | Agent browser options          | `.config/mise.toml` `[env]` `PLAYWRIGHT_MCP_*` rows, `PLAYWRIGHT_CLI_SESSION` per agent session        |
|  [24]   | Agent harness plugin           | `plugins/<name>`                                                                                       |
|  [25]   | Reusable asset                 | `assets/<kind>/<name>/` in `bsamiee/Rasm-Assets`, with native relative dependencies                    |

- Tool rows stay at `latest` where their backend lists releases
- `version_regex` rows over release tags exclude a development build `latest` resolves

## [08]-[STRUCTURE]

- Projects hold no `src/` directory
- Libraries sit at `libs/<language>/<category>/<package>/`, categories name the concern spanning their packages and siblings that field anticipates
- New categories start from the package being made, its sibling packages outside it, and the higher-order concern they share
- Packages are named by the term practitioners use for that concern
- Projects a host loads target the runtime and libraries their host bundles, and build output holds none of them
- Each uv root resolves one Python range: projects on a host's Python get their own root, and a package shared across roots declares the lower bound
- Nested uv roots each take a `[deps.<root>]` row in `.config/mise.toml` whose `dir` names the root
- Swift modules are root `Package.swift` targets, each in its own folder under `libs/swift/<category>/`, executables under `apps/macos/`
- C++ packages each declare their own `project()`
- Unreal plug-ins sit beside the project enabling them, whose `Plugins` folder takes their installed package
- Unreal plug-ins name each C++ package they take in `.uplugin` table `nx.packages`, built with their engine's compiler before the plug-in builds
- SQL schemas reconcile the live database to owning types, with no migration, history table, or versioned file or folder
- Reconciles drop or retype only what owning types declare retired or retyped
