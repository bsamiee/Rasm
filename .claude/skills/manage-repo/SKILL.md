---
name: manage-repo
description: "Use when changing how the repository is structured, built, or checked, assigns each repository concern its owner and growth rule."
---

# [MANAGE_REPO]

Entries state the principles by which the repository is maintained and improved. Each entry is a rule a maintainer acts on when adding the next project, tool, plugin, or harness row. Intent is the principle of development, management, and refinement, never allowing current state specifics, explicit mentioning of functionality, or features, naming, or implementation information bleed into the skill.

[REFERENCES]:
- [01]-[NX](references/nx.md): Plugin inference, target defaults, run-commands, dependencies, inputs, affected selection
- [02]-[TOOLING](references/tooling.md): Tool rows, backends, releases, version files, install paths, environment, launchd services, TOML files
- [03]-[TYPESCRIPT](references/typescript.md): Catalog, overrides, install scripts, compiler projects, direct execution, Biome rows
- [04]-[PYTHON](references/python.md): Dependency groups, lock, workspace members, interpreter, checkers
- [05]-[DOTNET](references/dotnet.md): Central row and SDK upgrades, analyzer rows, file-based apps, library groups
- [06]-[SWIFT](references/swift.md): Project files, build settings, compiler policy, packages, checkers, CI, SwiftPM, new projects, language server
- [07]-[JAVA](references/java.md): Formatter form, PMD ruleset and release tags, clone detection, language server
- [08]-[RUST](references/rust.md): Workspace root, catalog, lints, builds, members, checkers, upgrades, lint libraries
- [09]-[INFRA](references/infra.md): Automation API, resource options, workflow syntax decisions
- [10]-[PLUGINS](references/plugins.md): Plugin folder layout, Claude Code and Codex parity, converting components

## [01]-[PLACEMENT]

- Concerns with no owner take a new file named for the tool that reads it
- Structure joins with its first consumer, a row, file, target, or value nothing reads goes
- Consumers of a row are files importing its package (`package.json` included), commands running its binary, or tools reading it
- Catalog rows, their overrides, and tool-plugin rows stay without an importer, the catalog holds packages for future projects
- Build chains and project kinds of a declared host stay while no project uses them
- Rows naming a package, binary, or server point at one a project file or tool row installs
- Root files hold policy every project shares, a row one project consumes sits in the project file
- Project kinds (host, platform, product) join a root file as a row a tag or condition selects, or a kind file passing kind values to its general form
- Root files compute per-project values (cache, output path) from the project directory their tool supplies or a project file passes
- `tools/`, `infra/`, `plugins/`, and `eng/` code reads project names, paths, and values from arguments and project declarations, and names no project
- Project files are written by hand as the file set their language's init command produces
- Project files extending a root file exist for a plugin discovering projects by file name, a `--config` tool reads the root file from any `cwd`
- Rows restating a tool's documented default go, the default comes from a schema, release notes, or installed source
- Rows that leave tool output and lock unchanged when deleted go, a row that only cancels another row's effect goes with the canceled row
- Tightening checker rows (ban, required form, lowered threshold) state policy and stay with no violating file
- Skills name the tools they drive, their scripts, and output paths under `.artifacts/`, and hold no target or project row
- Scripts join their subject's skill, a script an app, target, or workflow consumes joins the repository
- Tools gain a capability through their own configuration and extension points, another tool joins for one they lack
- Scripts and `eng/` projects do work no tool command, package feature, or target does, and hold that work alone
- Root targets unify check, format, build, test, install, and release of the repository's own code, a wrapper over one mise tool is no target
- Entry points derive their items from declarations, a configuration, script, or README line per item goes
- Arguments after `--` select an operation's subject (path, app, filter) alone
- Command prefixes shared across targets (`doppler run`) state no duplicated fact

## [02]-[CHECKS]

- Projects with a check of their own hold one `check` target, per-language check names, dry-run variants, and check scripts beside it go
- Projects a root target alone checks declare no `check`
- Checkers, writers, and type checkers run as `lint:<tool>`, `format:<tool>`, and `typecheck:<tool>`, a project `lint` depends on its `lint:*`
- `<tool>` names the program implementing the command: `dotnet format` is `dotnet-format`, `cargo fmt` is `rustfmt`, `cargo dylint` is `dylint`
- One target runs a tool's subcommands, languages, and runtimes as one cache entry, a second target stays where its own entry saves reruns
- Runtime splits stay where the tool's source applies one version per run, a tool taking per-file versions runs once
- Writers sharing files take `parallelism: false`, no target depends on another tool's target
- Checkers fail on each finding they print, a tool passing warnings takes its own switch (`--strict`, `--error-on-warnings`, a config deny row) and keeps its levels
- Inputs and cache settings checker targets share sit once in a `targetDefaults` key matching their names
- Writers take no CI format-and-diff step, each writer's check form or a build diagnostic fails on every change it writes
- Rule, checker, or file extension rows join with a tracked file they read and the tree passing them
- Checks reporting an unused dependency stay off, a dependency row joins before the code importing it
- Languages built into ast-grep take no `sgconfig.yml` row until a newer grammar release parses a form the bundled grammar fails
- Grammars ast-grep lacks or bundles behind a newer release build from a catalog package through root `grammar` into a `customLanguages` row
