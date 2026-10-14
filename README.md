# [RASM]

Rasm is a polyglot monorepo with macOS-first development and portable code and tooling for Linux and Windows.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # One directory per host application
│   ├── blender/
│   ├── creative-cloud/
│   ├── macos/
│   ├── rhino/
│   └── web/
├── libs/                     # Packages, one directory per language
│   ├── dotnet/
│   ├── python/
│   ├── rust/
│   └── typescript/
├── tests/                    # Shared test support per language and suites outside libs/
├── eng/
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── infra/                    # Pulumi program declaring repository resources
├── tools/
│   ├── ast-grep/             # Outlines, rules, and utilities per language
│   ├── bridge/               # mcp-bridge binary serving each launchd agent's stdio MCP server over Streamable HTTP
│   ├── dylint/               # Rust lint libraries, one crate per lint
│   └── nx/                   # Nx plugin inferring a project from each project file
├── plugins/                  # Agent harness marketplace, one directory per plugin
├── mise.toml                 # Tool binaries and process environment
├── global.json               # .NET SDK versions
├── nx.json                   # Task graph
├── package.json              # Catalog rows except tool plugins, root Nx targets
├── pnpm-workspace.yaml       # TypeScript workspace globs and dependency catalog
├── pyproject.toml            # Python wheel project, dependency groups, and tool tables
├── Directory.Packages.props  # .NET central package versions
├── Directory.Build.props     # .NET build defaults and project classification by tree position
├── Directory.Build.targets   # .NET items, host package references, and build targets
├── Workspace.slnx            # .NET solution
├── NuGet.config              # NuGet source and package folder
├── Xcode.xcconfig            # Build settings every Xcode project inherits at project level
├── tsconfig.base.json        # Compiler options every TypeScript project extends
├── tsconfig.json             # Root TypeScript project over files outside every package
├── vitest.config.ts          # Test and coverage options every project config imports
├── vite.config.ts            # Bundling options every project config imports
├── biome.json                # TypeScript and JSON formatting and lint
├── pmd.xml                   # Java lint rules
├── sgconfig.yml              # ast-grep rule directories and language parsing
├── rust-toolchain.toml       # Rust toolchain channel and components rustup and mise read
├── rustfmt.toml              # Rust format rules
├── clippy.toml               # Clippy lint options Cargo.toml lint tables lack
├── Cargo.toml                # Rust workspace members, dependency catalog, lints, and profiles
├── .editorconfig             # Editor settings and .NET analyzer severity
├── .gitattributes            # Line endings, diff drivers, and LFS filters per extension
├── .swift-format             # Swift lint and format rules
├── .swiftlint.yml            # Swift lint rules swift-format lacks
├── .lldbinit                 # LLDB MCP server start every Xcode scheme's Run loads
├── .yamllint.yaml, .yamlfmt  # YAML lint and format
├── .cargo/                   # Cargo build directories, warning policy, compiler flags, and unstable Cargo flags
├── .github/                  # Continuous integration and repository workflows
├── .claude/                  # Agent harness knowledge and settings
├── .mcp.json                 # Agent harness MCP servers
├── .codex/                   # Codex harness knowledge and settings
├── .vscode/                  # Editor settings and recommended extensions
├── CLAUDE.md                 # Agent standards, AGENTS.md is its symlink
└── README.md
```

## [02]-[FLOW]

```mermaid
flowchart LR
    subgraph toolchain ["Toolchain"]
        direction TB
        mise_tools["mise.toml [tools], global.json, rust-toolchain.toml"] --> binaries["Tool binaries"]
        mise_env["mise.toml [env]"] --> processes["Every process"]
        xcode["xcode-select"] --> apple_tools["Xcode toolchain and macOS SDK"]
        brew["Homebrew formula ghidra"] --> ghidra_tool["Ghidra install"]
    end

    subgraph dependencies ["Dependencies"]
        direction TB
        catalog_ts["pnpm-workspace.yaml catalog"] --> lock_ts["pnpm-lock.yaml"]
        catalog_py["pyproject.toml dependencies and groups"] --> lock_py["uv.lock, .venv/bin on PATH"]
        catalog_rust["Cargo.toml [workspace.dependencies]"] --> lock_rust["Cargo.lock"]
        catalog_net["Directory.Packages.props"] --> restore["rasm:restore"]
        catalog_net --> eng_net["eng/dotnet"] --> upgrade["rasm:upgrade"]
        catalog_swift[".xcodeproj package requirements"] --> lock_swift["Package.resolved"]
    end

    subgraph taskgraph ["Task graph"]
        direction TB
        plugins["nx.json plugins"] --> projects["Project per project file: language and host tags, targets"]
        target_defaults["nx.json targetDefaults by language and host tag"] --> bodies["Target body per language and host"]
        root_nx["package.json nx"] --> root_targets["Root targets rasm:*"]
    end

    subgraph commands ["Commands"]
        direction TB
        lint["nx run rasm:lint"] --> checkers["One cached target per portable checker"]
        format_tree["nx run rasm:format"] --> writers["Every portable writer, then .NET fixes"]
        check_all["nx run-many -t check"] --> project_check["Build, typecheck, test, or lint per project"]
        check_affected["nx affected -t check"] --> project_check
        ci["ci.yml"] --> setup["setup action"] --> ci_steps["rasm:check, affected check per host runner"]
    end

    toolchain --> dependencies --> taskgraph --> commands
```

## [03]-[TASKS]

- Targets call one tool, arguments on the command, configuration in the tool's own file
- `nx run rasm:check` runs every `lint:<checker>` and `typecheck:<checker>` root target, `nx run rasm:lint:<checker>` one checker
- `nx run rasm:<target> -- <paths>` runs a Ruff, ast-grep, Biome, ty, or mypy target over named paths
- `nx run <project>:<target>` runs one target of one project
- `nx run <project>:build -- <switch>` forwards MSBuild switches to a .NET build
- `--skip-nx-cache` runs a build in place of an Nx cache replay
- `nx run <project>:install` builds or packs a project, then installs its product into its host
- `nx run <project>:pack` writes a project's product as the package its host installs
- `nx run rasm:upgrade` moves catalogs, Swift package locks, the Rust nightly date, and tool binaries to newest builds
- `nx run rasm:clean` removes declared outputs and tool residue, prunes unused installations and caches, and recovers stale macOS processes
- `nx run rasm:rewrite -- --filter='^<id>$' <path>` applies one rule's fix across a path
- `nx run rasm:outline -- <path>` lists a path's declarations, `--items` selects local, exported, imported, or all items, `--view` the depth
- Workspace plugin names each project's tags and targets by project file, Python edges by import, and Rust edges by catalog path row, `@nx/dotnet` and `@nx/vitest` infer theirs
- File-based app directories are one project each, named by path, `build`, `format`, and `lint` run one `<target>:<file>` target per entry file
- `nx run rasm:format` runs every app's `format` and `nx run rasm:lint:dotnet-format` every app's `lint`
- `nx run rasm:format:files -- <paths>` runs each portable writer over the named paths it formats
- Project `vite.config.ts` infers `build` and continuous `serve` for the Vite development server, both run from that project
- Project `uxp.config.ts` configures inferred Vite `build` and infers `install` into each host the manifest names
- Tools one host supplies join a project's target, root targets hold commands no project owns
- Inputs name the files a tool reads and its version as `runtime`, outputs name the files it writes
- Caches and outputs sit under root `.cache/` and `.artifacts/`, each tool relocated through one setting every run reads, or its skill states why not

## [04]-[OWNERS]

| [INDEX] | [CONCERN]                      | [OWNER]                                                                                              |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | Tool binary                    | `mise.toml` `[tools]` at `latest`, prereleases included                                              |
|  [02]   | Process variable               | `mise.toml` `[env]`                                                                                  |
|  [03]   | SDK version                    | `global.json` for .NET, `xcode-select` for Swift, `rust-toolchain.toml` for Rust                     |
|  [04]   | Package version                | `pnpm-workspace.yaml` catalog, `pyproject.toml` `[project]` or group, `Directory.Packages.props` row |
|  [05]   | .NET tool package              | `dotnet dnx <id>` on the command                                                                     |
|  [06]   | Task graph                     | `nx.json`, root `package.json` `nx`                                                                  |
|  [07]   | Checker configuration          | Tool's own file, `pyproject.toml` `[tool.*]` for every Python tool                                   |
|  [08]   | Secret                         | Doppler, through consuming commands with `doppler run`                                               |
|  [09]   | Resource or repository setting | Typed row of the program under `infra/`, applied by `nx run rasm:infra:up`                           |
|  [10]   | Tool with no consumer          | Machine setup                                                                                        |
|  [11]   | Ghidra install                 | Homebrew formula `ghidra`, path named in `mise.toml` `[env]`                                         |
|  [12]   | Xcode build setting            | `Xcode.xcconfig`, per-product rows in the `.xcodeproj` target                                        |
|  [13]   | Swift package version          | `.xcodeproj` package requirement                                                                     |
|  [14]   | Rust package version           | `Cargo.toml` `[workspace.dependencies]` row                                                          |
|  [15]   | Agent harness plugin           | `plugins/<name>`                                                                                     |
|  [16]   | Local MCP service              | `mise.toml` launchd agent row, applied by `mise bootstrap macos launchd-agents apply`                |
|  [17]   | MCP tool a skill replaces      | `mcp-bridge --hide <glob>` on server's `mise.toml` launchd agent row                                 |
|  [18]   | Python runtime version         | Nested `requires-python` at the root of the folder holding host code                                 |

- Tool rows name a release where `latest` resolves a development build
- Tool consumers are targets, MCP rows, skills, `.gitattributes` filters, and CLAUDE.md `[CLI_TOOLING]` rows
- Facts sit once in their owning file, other files name the owner

## [05]-[QUALITY]

- Failing checks are fixed in the code or the rule, severity stays as configured
- .NET: `dotnet build` and `dotnet format --verify-no-changes` at zero findings
- .NET `format` applies whitespace, style, and analyzer fixes
- Python: `ruff`, `ty`, and `mypy` at zero findings
- TypeScript: `biome check` at zero findings, `tsc` under strict options
- Rust: `cargo clippy`, `cargo dylint`, `cargo fmt --check`, and `cargo shear` at zero findings
- Swift: warnings as errors, strict memory safety, every supported upcoming feature, `swift-format lint --strict` and `swiftlint lint` at zero findings
- Java: `google-java-format --aosp` and `pmd check` at zero findings
- Checker decisions: PMD `AvoidAccessibilityAlteration` skips `use-ghidra` `Headers.run`, Ghidra's `PreProcessor` takes a `DefineTable` through package-private field `defs` alone
- Tree: `yamllint`, `yamlfmt -lint`, `actionlint` with `shellcheck` over workflow run steps, and ast-grep rule families
- Writers: `ruff format`, Biome, yamlfmt, rustfmt, `google-java-format`, `swiftlint lint --fix` then `swift-format`

## [06]-[STRUCTURE]

- `eng/<language>/` owns engineering workflows over repository projects and artifacts
- `tools/` provides capabilities for development tools and applications
- Apps group by host under `apps/<host>/`, with one folder per product holding its sources and assets
- Product assets sit in the folder their build reads: `Assets/` for .NET, `Resources/` for Xcode, `icons/` for Blender, `public/` for Vite
- Libraries group under `libs/<language>/<group>/` by domain or by host
- Rust library crates sit at `libs/rust/<crate>/`, named for their domain with no prefix
- Host groups hold every project built against the host's API
- Task graph takes the host list from the `apps/<host>/` folders and tags a project by the host name in its path, .NET build takes Rhino hosts from the `rhino/` path folder
- Python packages build into one `rasm` wheel
- First-party Python imports use `from <import root>.<module> import <member>` for private Blender extension relocation
- `Grasshopper2` package references add Grasshopper 2 global usings
- Installed Rhino supplies host assemblies at runtime, build output holds none
- Project files define projects, never `project.json`
- Project files are `.csproj`, `.cs` files opening with `#!`, `package.json` with `tsconfig.json`, `py.typed`, `blender_manifest.toml`, member `Cargo.toml`, and `.xcodeproj`
- `Workspace.slnx` lists every project `.csproj`
- `.xcodeproj` basenames name the Nx project, its scheme, and its product
- Projects hold no `src/` directory and no folder with one file, folders group by domain per language
- Packages, namespaces, routes, contracts, and directories carry no version suffix or `v1` folder
- Schema libraries apply the delta from owning types to the live database, with no migration file or history table
