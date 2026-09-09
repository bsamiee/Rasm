---
name: manage-repo
description: "Use when adding or changing a manifest, target, tool, workflow, or infra row, covering owners, tools, checks, growth, gates, removal."
---

# [MANAGE_REPO]

Tooling is a set of owner files, one per concern, each read by the tool that documents it. Changes land at their concern's owner.

## [01]-[OWNER]

- One file owns each concern: binaries, environment, SDK version, package versions, task graph, checker configuration, resources
- Concerns join their owner file
- New concerns with no owner take a new file
- Second file beside an owner: a fact placed in the wrong file, move the fact to the owner and delete the file
- Mini configs, platform variants, ignore files, per-directory project files, and alias files are that second file
- Facts appear once, at their owner
- Other files name the owner
- Paths, project names, counts, and session observations in skills, agents, rules, and comments restate the tree, delete them
- Readers open the file for a path, name, or count

## [02]-[TOOL]

- Tools run through their own command and read their own configuration file
- Configuration files hold policy, commands hold arguments
- Wrappers exist when they add a fact the tool lacks: a domain type, a boundary conversion, or a composed policy
- Scripts that map arguments to a tool call, select files a tool selects, or sequence calls a graph orders are wrappers, delete them
- Tool runs at environment load, binary patches, and backends past the registry are workarounds, replace them with the documented form
- Tools take the form their current documentation states
- Guards, fallbacks, retries, and comments around a defect hold the defect in place, fix it at the owner or delete the line

## [03]-[CHECK]

- Checks run over the files a change touched
- CI runs checks over the whole tree
- Checkers join target `lint` directly, writers join target `format` directly
- Each tool runs as one process over the tree
- Checks, rules, and tests stay read-only during a code change
- Changes to a check, rule, or test are their own task
- Checks that exist to make a bad form pass go with the form they protect
- Rules a configured checker already reports are duplicates, delete the rule and select the checker's code in the checker configuration
- Rules with tooling as their only subject (target commands, workflow steps, hook code) check the scaffold, defer them to the first product file
- Tree-wide runs proving a one-file change, repeated runs, and polls on a running command are waits, replace them with the exit code

## [04]-[GROWTH]

- Release, publish, changelog, versioning, tag rules, and registry login join with the first library release
- Coverage merge and mutation join with the second project of a language
- Config factories, build policies, and host bindings join with their first consumer
- Structure added ahead of its consumer is deleted, history returns it when the consumer arrives

## [05]-[GATE]

- Audits, scans, digest pins, signed checkouts, attestations, approval environments, cooldowns, and preview steps join when a product needs them
- Preview, dry-run, and check variants of a target are duplicates, delete them, the real target with `git diff --exit-code` proves the same

## [06]-[REMOVAL]

- Removals delete the thing, every mention of it, and every consumer's dependence on it in one change
- Consumers adjust to the absence
- Nothing stands in for removed content
- Replaced structures keep their predecessor's name
- One commit holds the change and the removal

[REFERENCES]:
- [01]-[NX](references/nx.md): Task graph, inference, target defaults, root targets, inputs, scoping
- [02]-[TYPESCRIPT](references/typescript.md): Workspace manifest, catalog, root manifest, compiler projects, patches
- [03]-[PYTHON](references/python.md): One project file, lock, environment, packages
- [04]-[DOTNET](references/dotnet.md): Build props and targets, central versions, SDK pin, feeds
- [05]-[TOOLING](references/tooling.md): Tool manager, environment, checkers and writers, harness
- [06]-[INFRA](references/infra.md): Typed program, workflows, CI, secrets, security timing
