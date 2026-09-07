# [RASM]

Rasm is a polyglot monorepo. Development targets macOS first, and all code and tooling stay portable to Linux and Windows. Dependencies, tools, and hosts must run on macOS. The root manifests hold every dependency version.

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
│   ├── ast-grep/             # Structural rules, on-demand rewrites, utilities, and rule tests per language
│   ├── dotnet/               # Roslyn analyzers executables and plugin hosts reference
│   └── nx/                   # Nx plugin for language tags and packaging projects
├── mise.toml                 # Toolchain, its resolution settings, and the process environment
├── nx.json                   # Task graph, caching, and change detection across the workspace
├── NuGet.config              # NuGet sources and package source mapping
├── Directory.Build.props     # .NET build defaults every project imports
├── Directory.Build.targets   # .NET items and policy checks every project imports
├── Directory.Packages.props  # .NET central package versions
├── pyproject.toml            # Python dependency groups and tool configuration
├── pnpm-workspace.yaml       # TypeScript workspace and dependency catalog
├── package.json              # Root package with development dependencies and root Nx targets
├── tsconfig.base.json        # Base TypeScript compiler options for workspace projects
├── tsconfig.json             # Root TypeScript project over the config files, tools/nx, and infra
├── biome.json                # TypeScript and JSON lint and formatting rules
├── sgconfig.yml              # ast-grep rule, utility, and test directories
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
├── .editorconfig             # Editor and analyzer settings per path
├── .shellcheckrc             # Shellcheck shell and check set
├── .gitattributes
├── .gitignore
├── CLAUDE.md                 # Agent standards
├── AGENTS.md                 # Symlink to CLAUDE.md
├── README.md
└── LICENSE                   # MIT license
```

## [02]-[TASKS]

[REQUIRED]: Tools and tasks route configurable caches and outputs under `.cache/` and `.artifacts/`. Work directories a tool cannot relocate are ignored and hold no durable output.

Nx is the task runner, `nx.json` and the root `package.json` `nx` field are the one entry point, and every developer command is a target:
- `nx run-many -t <target> -p tag:language:<language>` runs one target across one language, and `nx run <project>:<target>` runs one project
- `check` depends on `lint`, `format`, `typecheck`, and `test`, and the rewriting targets fix what their tool can fix and fail on the rest
- `nx run rasm:rewrite --id=<id> --paths=<paths>` applies one rewrite rule across the paths, re-run until it applies nothing
- Root targets hold the operations with no owning project, and plugins infer every other target from the manifests and the packaging projects
- Repository settings and secrets are infrastructure code under `infra/`, applied through a root target and read from the secret store at run time
- `mise.toml` owns the machine setup, every tool at its newest release, and the language lock files are the only pins

## [03]-[QUALITY]

Checker configuration is centralized, and each language area must pass its configured checks before a merge.

- .NET: Roslyn analyzers at `latest-all` with warnings as errors and code-style rules enforced during build
- `.editorconfig` holds .NET rule severity and configuration
- `Thinktecture.Runtime.Extensions.Analyzers` validates generated-type declarations and generated `Switch`/`Map` usage across every .NET project
- Python: `ruff`, `ty`, and `mypy` pass with zero warnings
- TypeScript: `biome check` passes and `tsc --build` compiles under strict settings
- Formatting: `dotnet format`, `ruff format`, and `biome format`
- Coverage and mutation score are reported, and no threshold gates a merge
- Fix a failing check in code, or in the rule when the rule is demonstrably invalid, and leave checker severity as configured

## [04]-[HARNESS]

Every agent behavior an engine event can observe is a hook, every other behavior has one owning file, and a weakness in either becomes a finding the automation lands.

- Behavior an event observes is a row in the event's table, or an arm of its event file when no table states it, each with its spec and its form
- Policies are pure decisions folded in table order, event files adapt them to the engine, and the store is the one state, one key per row
- Behavior no event observes is a setting when the harness reads it, a skill when an agent follows it, and prose when it is judgment
- Judgment that holds only under a `paths` glob is a `.claude/rules/` file, and a hook takes the mechanical half of the same fact
- `.claude/settings.json` holds the allow list, one entry per tool or server and no deny glob, and the `mise env` hook that feeds the agent shell
- Skills hold understanding and approach, agents are their workers with steps, commands, and a gate, and memory holds what no file owns
- Memory holds one fact per file under one shape, and the editor creates, narrows, merges, deletes, and indexes it
- Every skill, agent, and hook belongs to a plugin under the one marketplace `.claude/plugins/`, and a new one joins the plugin of its subject
- Subjects with no owning plugin take a new directory at the marketplace root, one manifest entry, and one `enabledPlugins` line under `@rasm`
- Rule families named for the package they read gate the code at `lint`, and a fix proven across the code becomes a rule with its siblings and test
- Rules and code widen each other until neither yields a move, the family scans, a hit lands as a fix, and a move beyond the rules derives a rule
- `nx run rasm:harness` regenerates the declarations every hook is typed against, and each regenerate is read for a capability a plugin hand-rolls
- Hooks are proven by their own decision in a transcript and debug file, `-p` for a call and an interactive session for a timer
- Findings come from every source an event observes, and the editor lands a batch of them or a due guidance part by one `manage-repo-guidance` move

## [05]-[LIBRARIES]

Every `libs/` package is independently consumable and publishes a stable API.

- Packages reference sibling packages through declared package dependencies
- Every dependency points to a lower-level package, and the dependency graph stays acyclic
- Python and TypeScript files declare their exports at the end
- Workflow assembly, configuration loading, and dependency composition belong to the application
- Sibling packages share naming, result type, and boundary types

## [06]-[LANGUAGE_AREAS]

Each language area follows its ecosystem's conventions and releases independently.

- Language areas share one design approach to boundaries, errors, and immutability
- Each area derives module layout, naming, and API design from its language
- Each area builds and runs without another language area present

## [07]-[APPLICATIONS]

Each `apps/<name>/` is one product with its own host, lifecycle, and release.

- Each application depends on `libs/` and third-party packages
- One application spans as many languages and projects as its host requires
- Applications hold the composition root for configuration, dependencies, effect execution, and telemetry
- Host APIs stay inside the package named for that host or inside the application

## [08]-[CHANGE]

Changes replace structure in place, and releases run per project from git tags through one dispatch workflow.

- Schema libraries apply the delta from the owning types to the live database at startup or from a command, with no migration file or history table
- Each project or fixed group gets a `<name>@<version>` tag, build tools read the version from the tag, and registries take trusted publishing
- No file states a version
- No package, namespace, route, contract, or directory has a version suffix or `v1` folder, and a changed structure keeps its predecessor's name
- No compatibility shim, fallback reader, or deprecation period keeps a replaced structure alive, and one commit holds the change and the removal
- No `src/` directory exists at any depth, a project's files sit at its root, and no directory exists only to add a level of nesting
