---
name: manage-repo
description: "Use when adding or changing a project file, target, tool, workflow, infra row, or desktop application interface, covering owners, growth, targets, and checks."
---

# [MANAGE_REPO]

[REFERENCES]:
- [01]-[NX](references/nx.md): Plugin inference, target defaults, run-commands, dependencies, inputs, affected selection
- [02]-[TOOLING](references/tooling.md): Tool rows, release settings, version files, install paths, environment templates
- [03]-[TYPESCRIPT](references/typescript.md): Catalog, overrides, install scripts, composite compiler projects, direct execution, Biome rows
- [04]-[PYTHON](references/python.md): Dependency groups, lock, workspace members, interpreter, checkers
- [05]-[DOTNET](references/dotnet.md): Central row and SDK upgrades, catalog project, analyzer rows
- [06]-[SWIFT](references/swift.md): Project files, root build settings, compiler policy, packages, checkers, CI, SwiftPM libraries, new projects
- [07]-[JAVA](references/java.md): Formatter form, PMD and ast-grep rows, language server
- [08]-[INFRA](references/infra.md): Automation API, resource options, workflow syntax decisions
- [09]-[APPLICATIONS](references/applications.md): Interface standard every configured desktop application follows

## [01]-[PLACEMENT]

- Use `README.md` for each concern's owner
- Concerns with no owner take a new file named for the tool that reads it
- Structure joins with its first consumer, a row, file, target, or value nothing reads goes
- Consumers of a row are files importing its package, commands running its binary, or tools reading it, a file importing `package.json` included
- Catalog rows, their overrides, and tool-plugin rows stay without an importer, the catalog holds packages for future projects
- Build chains and project kinds of a declared host (UXP, ExtendScript) stay while no project uses them
- `plan/` holds plans and research for future projects and stays read-only
- Rows naming a package, binary, or server point at one a project file or tool row installs, a missing install record joins its owning file
- Root files hold policy every project shares, a row one project consumes sits in the project file
- Project files are written by hand as the file set their language's init command produces
- Project files extending a root file exist for a plugin discovering projects by file name, a `--config` tool reads the root file from any `cwd`
- Rows restating a tool's documented default go, the default comes from a schema, release notes, or installed source
- Rows that leave tool output and lock unchanged when deleted go, a row that only cancels another row's effect goes with the canceled row
- Relaxing checker rows (ignore, allowlist, suppression, raised threshold) stay while a file violates the rule without them
- Tightening checker rows (ban, required form, lowered threshold) state policy and stay with no violating file
- Suppressions one file needs sit at its top, one every importer of a package needs (missing stubs) sits in the tool's table
- Skills name the tools they drive, their scripts, and output paths under `.artifacts/`, and hold no target or project row
- Scripts join their subject's skill, a script an app, target, or workflow consumes joins the repository
- Scripts answer a question no single tool call answers, a script that forwards one tool call goes
- Root targets unify check, format, build, test, install, and release of the repository's own code, a wrapper over one tool mise supplies is no target
- One operation of one tool over one file set runs in one target, a second target or entry running it again goes
- Each operation a tool exposes as a subcommand takes one target, the unit Nx lists, orders through `dependsOn`, and hashes by its own inputs
- Entry points derive their items from declarations, a configuration, script, or README line per item goes
- Arguments after `--` select an operation's subject (path, app, filter) and never the operation
- Command prefixes shared across targets (`doppler run`) state no duplicated fact

## [02]-[CHECKS]

- Projects with a check of their own hold one `check` target, per-language check names, dry-run variants, and check scripts beside it go
- Projects a root target alone checks declare no `check`
- Writers take no CI format-and-diff step, each writer's check form or a build diagnostic fails on every change it writes
- Rule, checker, or file extension rows join with a tracked file they read and the tree passing them
- Languages built into ast-grep take no `sgconfig.yml` row
