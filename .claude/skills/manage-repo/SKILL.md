---
name: manage-repo
description: "Use when changing how the repository is structured, built, or checked, assigns each repository concern its owner and growth rule."
---

# [MANAGE_REPO]

Entries state rules a maintainer acts on when adding the next project, tool, plugin, or harness row. Current state, features, project names, and code implementation stay in the files that hold them.

[REFERENCES]:
- [01]-[NX](references/nx.md): Plugin inference, target defaults, run-commands, dependencies, inputs, affected selection
- [02]-[TOOLING](references/tooling.md): Tool rows, backends, releases, version files, install paths, environment, launchd services, TOML files
- [03]-[TYPESCRIPT](references/typescript.md): Catalog, overrides, install scripts, compiler projects, direct execution, Biome rows
- [04]-[PYTHON](references/python.md): Dependency groups, lock, workspace members, interpreter, checkers
- [05]-[DOTNET](references/dotnet.md): Central row and SDK upgrades, file-based apps, library groups, BuildCheck
- [06]-[SWIFT](references/swift.md): Project files, build settings, compiler policy, packages, checkers, CI, SwiftPM, new projects, language server
- [07]-[JAVA](references/java.md): Formatter form, PMD ruleset and release tags, clone detection, language server
- [08]-[RUST](references/rust.md): Workspace root, toolchain, catalog, lints, builds, members, checkers, upgrades, lint libraries
- [09]-[INFRA](references/infra.md): Automation API, resource options, workflow syntax decisions
- [10]-[PLUGINS](references/plugins.md): Plugin folder layout, Claude Code and Codex parity, MCP server rows, converting components

## [01]-[PLACEMENT]

- Concerns with no owner take a new file named for the tool that reads it
- Structure joins with its first consumer, a row, file, target, or value nothing reads goes
- Row consumers are importing files (`package.json` included), reading tools, targets, MCP rows, skills, `.gitattributes` filters, and `[CLI_TOOLING]` rows
- Catalog rows, their overrides, and tool-plugin rows stay without an importer, the catalog holds packages for future projects
- Build chains and project kinds of a declared host stay while no project uses them
- Rows naming a package, binary, or server point at one a project file or tool row installs
- Root files hold policy every project shares, a row one project consumes sits in the project file
- Project kinds (host, platform, product) join a root file as a row a tag or condition selects, or a kind file passing kind values to its general form
- Root files compute per-project values (cache, output path) from the project directory their tool supplies or a project file passes
- `tools/`, `infra/`, `plugins/`, and `eng/` code reads project names, paths, and values from arguments and project declarations, and names no project
- Project files are written by hand as the file set their language's init command produces
- Project files extending a root file exist for a plugin discovering projects by file name, a `--config` tool reads the root file from any `cwd`
- Rows leaving tool output and lock unchanged when deleted go, a documented default among them, a row canceling another goes with the canceled row
- Lists replacing a default list keep its entries, a schema, release notes, or installed source states each default
- Tightening checker rows (ban, required form, lowered threshold) state policy and stay with no violating file
- Skills name the tools they drive, their scripts, and output paths, and hold no target or project row
- Scripts join their subject's skill, a script an app, target, or workflow consumes joins the repository
- Capabilities come from tool configuration and extension points, then another tool, then a script or `eng/` project for work no command or package does
- Root targets unify check, format, build, test, install, and release of the repository's own code, a wrapper over one mise tool is no target
- Entry points derive their items from declarations, a configuration, script, or README line per item goes
- Arguments after `--` select an operation's subject (path, app, filter) or pass a one-run switch, checker policy stays in target `options.args`
- Command prefixes shared across targets (`doppler run`) state no duplicated fact

## [02]-[CHECKS]

- Projects with their own check hold one `check` target with no per-language name, dry-run variant, or check script, a root-checked project holds none
- Checkers, writers, and type checkers run as `lint:<tool>`, `format:<tool>`, and `typecheck:<tool>`, a project `lint` depends on its `lint:*`
- `<tool>` names the program implementing the command: `dotnet format` is `dotnet-format`, `cargo fmt` is `rustfmt`, `cargo dylint` is `dylint`
- One target runs a tool's subcommands, languages, and runtimes as one cache entry, a second target stays where its own entry saves reruns
- Runtime splits stay where the tool's source applies one version per run, a tool taking per-file versions runs once
- Writers sharing files take `parallelism: false`, no target depends on another tool's target
- Checkers fail on each finding they print, a tool passing warnings takes its own switch (`--strict`, `--error-on-warnings`, a config deny row)
- Checker rules join for a form no enabled checker reports, a rule contradicting a required form or another checker stays off with its reason
- Inputs and cache settings checker targets share sit once in a `targetDefaults` key matching their names
- Writers take no CI format-and-diff step, each writer's check form or a build diagnostic fails on every change it writes
- Rule, checker, or file extension rows join with a tracked file they read and the tree passing them
- Languages built into ast-grep take no `sgconfig.yml` row until a newer grammar release parses a form the bundled grammar fails
- Grammars ast-grep lacks or bundles behind a newer release build from a catalog package through root `grammar` into a `customLanguages` row
