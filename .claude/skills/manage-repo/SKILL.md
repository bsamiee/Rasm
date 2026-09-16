---
name: manage-repo
description: "Use when adding or changing a manifest, target, tool, workflow, or infra row, covering owners, growth, targets, and checks."
---

# [MANAGE_REPO]

Covers manifests, targets, tools, workflows, and infra rows of a polyglot monorepo.

## [01]-[PLACEMENT]

- Use `README.md` for the owner of each concern
- Concerns with no owner take a new file named for the tool that reads it
- Structure joins with its first consumer, a target, workflow, row, or configuration with no consumer is removed
- Root files hold policy every project shares, a row one project consumes sits in the project's manifest
- Rows restating a tool's documented default go, the default comes from a schema, release notes, or installed source
- `check` of a row's consumers proves the removal, a source file importing `package.json` for `version` is a consumer
- Skills couple to no project, a skill names the tools it drives and its own scripts, a target, manifest row, or repository path stays out
- Scripts join the skill whose subject they serve, a script an app, target, or workflow consumes joins the repository
- Root targets unify check, format, build, test, install, and release of the repository's own code, a wrapper over one tool mise supplies is no target

## [02]-[CHECKS]

- Per-language target names, preview or dry-run variants, and proof scripts are second checks, one run of the project's `check` target proves a change
- Writers prove through `git diff --exit-code` after their target
- Rule or checker row joins with the tree passing it, its first run over the tree is its proof

[REFERENCES]:
- [01]-[NX](references/nx.md): Plugin inference, target defaults, run-commands, dependencies, inputs, affected selection
- [02]-[TYPESCRIPT](references/typescript.md): Catalog, overrides, patches, composite compiler projects, direct execution
- [03]-[PYTHON](references/python.md): Dependency groups, lock, environments, interpreter, automation packages
- [04]-[DOTNET](references/dotnet.md): MSBuild skills, project file contents, central row upgrades
- [05]-[TOOLING](references/tooling.md): Tool rows, release settings, version files, environment templates
- [06]-[INFRA](references/infra.md): Automation API, resource options, workflow syntax decisions
- [07]-[JAVA](references/java.md): Manifest, formatter form, tool rows that wait for the first project
- [08]-[SWIFT](references/swift.md): Project file rows, build setting defaults, schemes, format configuration
