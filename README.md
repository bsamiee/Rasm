# [RASM_WORKSPACE]

Rasm is a polyglot monorepo. Development targets macOS first, and all code and tooling remain portable to Linux and Windows. Rasm accepts only dependencies, tools, and hosts that run on macOS.

- Language-specific code is organized beneath `libs/` and `tests/`.
- Applications live in `apps/`, one directory per product, and consume internal libraries through package dependencies.
- Shared build and release automation lives in `eng/`.

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
│   └── scripts/              # Python automation that Nx targets and CI jobs invoke
├── tools/                    # Custom tools for developing this project
│   └── biome/                # Python automation that Nx targets and CI jobs invoke
├── nx.json                   # Task graph, caching, and change detection across the workspace
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
├── Workspace.slnx            # .NET solution listing every project
├── global.json               # .NET SDK version and test runner config (MTP)
├── NuGet.config              # NuGet feed, clears inherited machine and user sources
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── .editorconfig             # Analyzer severity, path-specific overrides, and BuildCheck settings
├── .gitattributes
├── .gitignore
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools and tasks route configurable caches and outputs under `.cache/` and `.artifacts/`; tool-specific root work directories that cannot be relocated are explicitly ignored and contain no durable output.

Nx defines the task graph and the build, test, lint, and generate targets.

- Targets resolve from plugin inference, then `targetDefaults` in `nx.json`, then a project's own configuration, each source overriding the one before it
- Targets running a single command name that command directly
- Steps containing control flow are implemented as Python scripts under `eng/scripts/`, which a target invokes
- Scripts declare their dependencies inline and run under `uv run` without a shared Python environment

## [03]-[QUALITY]

Checker configuration is centralized, and each language area must pass its configured checks before code is merged.

- .NET: Roslyn analyzers at `latest-all`, warnings-as-errors, code-style rules enforced during build, `.editorconfig` carries rule severity and configuration.
- `Thinktecture.Runtime.Extensions.Analyzers` validates generated-type declarations and generated `Switch`/`Map` usage across every .NET project.
- Python: Passes with no warnings/errors from `ruff`, `ty`, and `mypy`.
- TypeScript: Passes `biome check` and compiles with `tsc --build` under strict settings.
- Formatting: `dotnet format`, `ruff format`, and `biome format`
- Do not relax checker settings; repair the code or correct a demonstrably invalid rule

## [04]-[LIBRARIES]

Every `libs/` package is independently consumable and publishes a stable API.

- Packages reference sibling packages through declared package dependencies
- Every dependency edge points toward a lower-level package, the graph stays acyclic
- Packages expose capabilities. Python and TypeScript files declare explicit exports at the end.
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
