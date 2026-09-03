# [RASM_WORKSPACE]

Rasm is a polyglot monorepo. Development targets macOS first, all code and tooling stay portable to Linux and Windows. Only dependencies, tools, and hosts that run on macOS are accepted.

- Language-specific code is organized beneath `libs/` and `tests/`
- Applications live in `apps/`, one directory per product, and consume internal libraries through package dependencies
- Shared build and release automation lives in `eng/`

Dependency versions centralize in root manifests: `Directory.Packages.props` for .NET, `pyproject.toml` and `uv.lock` for Python, and `pnpm-workspace.yaml` for TypeScript.

## [01]-[LAYOUT]

```text
Rasm/
├── apps/                     # One directory per application, covering plugins, services, desktop and web clients
├── docs/                     # Durable documentation
├── libs/
│   ├── contracts/
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── tests/                    # Cross-language test policy, reusable test support, and non-colocated suites
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── eng/                      # Shared build and release infrastructure
│   ├── native/               # Native packaging: version manifest directory and packaging projects per library
│   └── scripts/              # Python automation that Nx targets and CI jobs invoke
├── tools/                    # Custom tools for developing this project
│   ├── biome/                # Biome GritQL plugin rules
│   └── nx/                   # Nx plugin that infers the packaging projects, their stage and pack targets, and their edges
├── nx.json                   # Task graph, caching, and change detection across the workspace
├── NuGet.config              # NuGet sources and package source mapping, clears inherited machine and user sources
├── Directory.Build.props     # .NET build defaults, artifacts path, restore, analysis, analyzers, Rhino bundle paths
├── Directory.Build.targets   # .NET derived items, host references, and project policy checks
├── Directory.Packages.props  # .NET central package versions
├── pyproject.toml            # Python dependency groups and tool configuration
├── pnpm-workspace.yaml       # TypeScript workspace and dependency catalog
├── package.json              # Root TypeScript package metadata and development dependencies
├── tsconfig.base.json        # Base TypeScript compiler options for workspace projects
├── tsconfig.json             # TypeScript project references that drive build order
├── biome.json                # TypeScript lint and formatting rules
├── vite.config.ts            # Shared TypeScript Vite build configuration imported by app and package configs
├── vitest.config.ts          # TypeScript Vitest projects, coverage, and benchmark configuration
├── stryker.config.json       # TypeScript mutation testing
├── stryker-config.json       # .NET mutation testing
├── Workspace.slnx            # .NET solution of the library, application, and test projects
├── global.json               # .NET SDK version and test runner config (MTP)
├── .config/dotnet-tools.json # .NET local tool manifest, restored by the eng provision target
├── .editorconfig             # Analyzer severity, path-specific overrides, and BuildCheck settings
├── .gitattributes
├── .gitignore
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools and tasks route configurable caches and outputs under `.cache/` and `.artifacts/`, tool work directories that cannot be relocated are ignored and hold no durable output.

Nx defines the task graph and the build, test, lint, and generate targets.

- Targets resolve from plugin inference, then `targetDefaults` in `nx.json`, then a project's own configuration, each source overriding the one before it
- Targets running a single command name that command directly
- Steps with control flow are Python scripts under `eng/scripts/` that a target invokes
- Scripts take their dependencies from the root `pyproject.toml` groups and run under `uv run`
- `nx run eng:provision` restores the tool manifest and places vcpkg, its binary cache, and every pinned release archive under `.cache/`
- `tools/nx/native-packaging.ts` infers one project per `eng/native/*/*.csproj` with a `stage` target and a cached `pack` target
- `stage` runs `uv run python -m eng.scripts.stage <library>` after `eng:provision`, and `pack` writes the package to the `local` source in `NuGet.config`
- Binding projects with `IncludeBuildOutput` true get `pack` alone, which depends on the `stage` target of the native project of the same library
- Inferred `build` targets pass `--no-restore`, and `dotnet restore Workspace.slnx` precedes `nx affected -t build test`
- `nx graph --file=.artifacts/nx/graph.json` writes the project graph
- `ProjectReference` edges and `PackageReference` edges to packaging projects drive `nx affected`

## [03]-[QUALITY]

Checker configuration is centralized, and each language area must pass its configured checks before code is merged.

- .NET: Roslyn analyzers at `latest-all`, warnings-as-errors, code-style rules enforced during build, `.editorconfig` carries rule severity and configuration
- `Thinktecture.Runtime.Extensions.Analyzers` validates generated-type declarations and generated `Switch`/`Map` usage across every .NET project
- Python: Passes with no warnings/errors from `ruff`, `ty`, and `mypy`
- TypeScript: Passes `biome check` and compiles with `tsc --build` under strict settings
- Formatting: `dotnet format`, `ruff format`, and `biome format`
- Do not relax checker settings, repair the code or correct a demonstrably invalid rule

## [04]-[LIBRARIES]

Every `libs/` package is independently consumable and publishes a stable API.

- Packages reference sibling packages through declared package dependencies
- Every dependency edge points toward a lower-level package, the graph stays acyclic
- Packages expose capabilities: Python and TypeScript files declare explicit exports at the end
- Workflow assembly, configuration loading, and dependency wiring belong to the application
- Sibling packages align on naming, the result type, and boundary types to compose predictably

## [05]-[LANGUAGE_AREAS]

Each language area follows its own ecosystem and releases independently.

- Language areas share one design approach to boundaries, errors, and immutability
- Each area follows the idioms and standards of its language
- Each area derives module layout, naming, and API design from its language
- Each area builds and runs without another language area present

## [06]-[APPLICATIONS]

Each `apps/<name>/` is one product with its own host, lifecycle, and release.

- Each application depends on `libs/` and third-party packages
- One application spans as many languages and projects as its host demands
- Applications own the composition root, where configuration, dependency wiring, effect execution, and telemetry are implemented
- Host APIs stay inside the package named for that host or inside the application itself

## [07]-[CHANGE]

The workspace has one current structure, and every change replaces the previous one in place. Data schemas derive from their owning types, and a schema management library computes the delta between the model and the live database and applies it at startup or from a command, with no migration file or history table

- No package, namespace, route, contract, or directory has a version suffix or `v1` folder, and a changed structure keeps the name of the one it replaces
- No compatibility shim, fallback reader, or deprecation period keeps a replaced structure alive, and one commit holds the change and the removal
- No `src/` directory exists at any depth, a project's files sit at its root, and no directory exists only to add a level of nesting
