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

## [02]-[CHECKS]

- Per-language target names, preview or dry-run variants, and proof scripts are second checks, one run of the project's `check` target proves a change
- Writers prove through `git diff --exit-code` after their target

[REFERENCES]:
- [01]-[NX](references/nx.md): Plugin inference, target defaults, run-commands, dependencies, inputs, affected selection
- [02]-[TYPESCRIPT](references/typescript.md): Catalog, overrides, patches, composite compiler projects
- [03]-[PYTHON](references/python.md): Dependency groups, lock, environments, interpreter, automation packages
- [04]-[DOTNET](references/dotnet.md): MSBuild skills, project file contents, central row upgrades
- [05]-[TOOLING](references/tooling.md): Tool rows, release settings, version files, environment templates
- [06]-[INFRA](references/infra.md): Automation API, resource options, workflow syntax decisions
