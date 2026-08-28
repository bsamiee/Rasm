# [RASM_WORKSPACE]

Rasm is a polyglot monorepo. Development targets macOS first and every surface stays portable to Linux and Windows. A dependency, tool, or host that cannot run on macOS stays out.

All primary folders are branched by language, `libs/`, `apps/`, `tests/`, and `eng/` contains the automation for all languages, but is NOT organized by language. Reusable libraries live in `libs/`. Applications live in `apps/`, one directory per product, each consuming a library exactly as it consumes any third-party package.

All languages are CPM, centralized package management, versions ONLY live in repo root, nowhere else, and they carry package identity and dependency edges.

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
├── tests/                    # Centralized test suites
│   ├── dotnet/
│   ├── python/
│   └── typescript/
├── eng/                      # Build and release infrastructure every language branch shares
│   └── scripts/              # Python automation that Nx targets and CI jobs invoke
├── tools/                    # Custom tools for developing this project
│   └── biome/                # Python automation that Nx targets and CI jobs invoke
├── nx.json                   # Task graph, caching, and change detection across every branch
├── Directory.Build.props     # C# configuration
├── Directory.Build.targets   # C# items and references resolved after each project declares its packages
├── Directory.Packages.props  # CPM - C#
├── pyproject.toml            # CPM - Python, and rule set tuning for all Python tools
├── pnpm-workspace.yaml       # CPM - TypeScript
├── package.json              # TypeScript catalog dependencies EVERY project extends
├── tsconfig.base.json        # TypeScript compiler options EVERY project extends
├── tsconfig.json             # TypeScript project references that drive build order
├── biome.json                # TypeScript lint and formatting rules
├── vite.config.ts            # TypeScript Shared Vite build configuration that app and package configs import
├── vitest.config.ts          # TypeScript Vitest projects, coverage, and benchmark configuration
├── stryker.config.json       # TypeScript mutation testing
├── stryker-config.json       # C# mutation testing
├── Workspace.slnx            # C# solution listing every C# project
├── NuGet.config              # NuGet feed with package source mapping and audit source
├── global.json               # .NET SDK version and test runner config (MTP)
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── .editorconfig             # C# analyzer severity rules (200+)
├── .gitattributes
├── .gitignore
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools/tasks MUST have outputs configured for caches and outputs to land under `.cache/` and `.artifacts/`, NEVER as root level litter.

Nx owns the task graph, and build, test, lint, and generate targets.

- Targets resolve from plugin inference, then `targetDefaults` in `nx.json`, then a project's own configuration, each source overriding the one before it
- Targets running a single command name that command directly
- Steps carrying real logic land as Python scripts under `eng/scripts/`, which a target invokes
- Scripts declare their own dependencies inline and run under `uv run`, no shared environment gates them

## [03]-[QUALITY]

Every checker runs at its strictest available setting, and each branch passes all of its checkers before code lands.
- Dotnet: Roslyn analyzers at `latest-all`, warnings-as-errors, code-style rules enforced during build, `.editorconfig` carries rule severity and configuration.
- Python: Passes with no warnings/errors from `ruff`, `ty`, and `mypy`.
- TypeScript: Compiles under `biome`, and `tsc` with `strict`
- Formatting: `dotnet format`, `ruff format`, and `biome`
- Strictness moves in one direction, a relaxed setting is a defect to repair rather than a decision to keep

## [04]-[LIBRARIES]

Every `libs/` package stands alone, publishing an API for a consumer it never meets.

- Packages import a sibling the way they import any third-party dependency
- Every dependency edge points toward a lower-level package, the graph stays acyclic
- Packages expose capability. Explicit exports at the END of the file is required for Python and Typescript
- Workflow assembly, configuration loading, and dependency wiring belong to the application
- Siblings align on naming, failure handling, and boundary shape to compose predictably

## [05]-[LANGUAGE_BRANCHES]

Each language branch develops on its own terms and ships without the others.

- Branches share one design approach to boundaries, failure, and immutability
- Each branch follows the idioms and standards of its own language
- Each branch derives its module layout, naming, and API shape from that language alone
- Each branch builds and runs with no other branch present
- Each branch expresses that approach in its own idiom

## [06]-[APPLICATIONS]

Each `apps/<name>/` is one product with its own host, lifecycle, and release.

- Each application depends on `libs/` and third-party packages
- One application spans as many languages and projects as its host demands
- Applications own the composition root, where configuration, dependency wiring, effect execution, and telemetry land
- Host APIs stay inside the package named for that host or inside the application itself
