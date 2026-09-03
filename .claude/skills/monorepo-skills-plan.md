# [MONOREPO_SKILLS_PLAN]

One skill, `monorepo-build-infrastructure`, encodes how the shared build infrastructure of a polyglot monorepo is organized, from the entry points to release. Its material is the research under `docs/research/monorepo/` and the repository as the steps left it, and the skill describes behavior the repository shows. A fresh session builds the skill from the plan in the agent sequence it records.

## [01]-[SOURCES]

Each file is a verified research report, rewritten under the clean-prose rules, with a first-line HTML comment naming its owner and its integration state.

| [INDEX] | [FILE]                    | [CONTENT]                                                    | [OWNER]                                   |
| :-----: | :------------------------ | :----------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `toolchain-installer.md`  | Installer comparison, mise file shape, environment owners    | [02], `references/installer.md`           |
|  [02]   | `nx-capabilities.md`      | Plugins, `targetDefaults`, inputs, affected, cache, sync     | [01], [03], `references/nx-targets.md`    |
|  [03]   | `pnpm-capabilities.md`    | Settings file, catalogs, lockfile, engine fields, references | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]    |
|  [04]   | `uv-capabilities.md`      | Workspaces, groups, interpreter settings, tools, cache, CI   | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]    |
|  [05]   | `eng-directories.md`      | `eng/` in 24 .NET repositories, the feature per practice     | [04]-[ENGINEERING_DIRECTORY]              |
|  [06]   | `polyglot-monorepos.md`   | 15 polyglot repositories, Nx specifics, their corrections    | [03]-[CONFIGURATION], [07]-[CORRECTIONS]  |
|  [07]   | `github-actions.md`       | `.github/` file kinds, workflow design, caching, languages   | [05]-[CI], `references/ci-workflow.md`    |
|  [08]   | `pulumi.md`               | Repository-owned program placement, adoption, state, tokens  | [04], `references/iac.md`, `pulumi` skill |
|  [09]   | `doppler.md`              | Live Doppler and 1Password state, ownership split, CI route  | `references/iac.md`, `secrets` skill      |
|  [10]   | `globaljson.md`           | Readers of `global.json`, mise behavior, per-field verdict   | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]    |
|  [11]   | `dotnet-tool-manifest.md` | Coverage merging, diagnostics, tool entries, rejected tools  | [02]-[TOOLCHAIN]                          |
|  [12]   | `mcp-coverage.md`         | MCP servers, SDK-provided commands, tool pinning             | [02]-[TOOLCHAIN]                          |

`dotnet-tool-manifest.md` and `mcp-coverage.md` describe the tool manifest the repository removed, and the skill takes from them the coverage merge, the MCP servers, and the `dotnet dnx` route alone. The Parametric_Forge inventories feed the `forge-removals-for-rasm` memory, and the research folder holds none of them.

## [02]-[OWNERSHIP]

Each skill owns one kind of fact, and a fact appears in the skill that owns it:
- Use `monorepo-build-infrastructure` for the decisions on runner, targets, pins, setting placement, `eng/`, `infra/`, CI, release, and corrections
- Use `dotnet-msbuild-packaging` for central package management, `NuGet.config`, lock files, `.slnx`, package projects, and CI build properties
- Use the `dotnet-msbuild-*` skills for the MSBuild files, targets, review, and binlogs
- Use `pulumi` for the program mechanics: resources, adoption through import, state backends, previews, and destroys
- Use `secrets` for where a secret belongs: 1Password for credentials a person uses, Doppler for runtime secrets, and how a token reaches a process
- Use `clean-prose` for every sentence

## [03]-[DECISIONS]

The skill carries each decision as a rule with the criterion an agent applies to the next case, and the reason is the sentence the rule needs:

| [INDEX] | [DECISION]                                                 | [REASON]                                                               |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | One runner, every developer and CI action is an Nx target  | A second runner splits the graph and its cache, mise `[tasks]` unused  |
|  [02]   | Each environment variable has one owner by its scope       | Manifest field, target `env`, `mise.toml` `[env]`, or secret store     |
|  [03]   | One owner per fact and one statement per version           | Restated facts drift, and the second copy is read after the first      |
|  [04]   | Versions stay in the central managers alone                | Root manifests hold every version, no tool manifest, no engine field   |
|  [05]   | `project.json` only for a directory no manifest describes  | Plugins infer the rest, the root project is the root `nx` field        |
|  [06]   | No hedge and no gate                                       | A missing dependency joins the graph, and no check is named a gate     |
|  [07]   | Coverage on every `test` run, merged once per language     | Coverage and mutation score are information with no threshold          |
|  [08]   | Tool packages run through `dotnet dnx`, newest release     | A tool manifest pins a version outside the central managers            |
|  [09]   | One restore target, .NET `build` and `format` depend on it | `--no-restore` reads the assets restore wrote, no job runs restore     |
|  [10]   | Defaults state `commands` arrays for `nx:run-commands`     | A `command` string from a default beats a project's `commands`         |
|  [11]   | `infra/` holds the repository's own resources alone        | An application's program sits under its own directory in `apps/`       |
|  [12]   | Automation API program, no project or stack file           | Inline program, file backend, passphrase provider, explicit providers  |
|  [13]   | Credentials resolve in code, ambient variable else store   | Passphrase and GitHub token from 1Password, Doppler token from its CLI |
|  [14]   | Actions use their current major tags                       | The tag moves with the action, no ratchet tool pins a commit           |
|  [15]   | Cache keys from lock file or manifest, no `restore-keys`   | A partial match is a fallback, each entry is a function of its key     |
|  [16]   | MinVer reads the version from the nearest `v` tag          | `release.yml` creates the GitHub release, no file states a version     |

Environment variables by owner:

| [INDEX] | [VARIABLE]                                | [OWNER]                               |
| :-----: | :---------------------------------------- | :------------------------------------ |
|  [01]   | Settings with a manifest spelling         | The manifest field                    |
|  [02]   | Variables one task needs                  | The target's `env`                    |
|  [03]   | Variables every tool needs before it runs | `mise.toml` `[env]`                   |
|  [04]   | Secrets                                   | `doppler run` or the `infra/` program |

`dotnet dnx` commands for packages that include `net8.0` tools alone carry `--allow-roll-forward`, and the .NET coverage merge command joins the root `coverage` target with the first .NET test project, because ReportGenerator fails on an empty glob.

## [04]-[REPOSITORY_STEPS]

Each step ran as one commit with its checks at zero warnings, and every step is done:

| [INDEX] | [STEP]          | [FILES]                                                                                                    | [STATE] |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------- | :------ |
|  [01]   | Research folder | `docs/research/monorepo/*.md`                                                                              | Done    |
|  [02]   | Toolchain       | `mise.toml`, `package.json`, `pnpm-workspace.yaml`, `pyproject.toml`, `.vscode/settings.json`              | Done    |
|  [03]   | .NET tools      | `.mcp.json`, `Directory.Build.props`, `Directory.Packages.props`, no tool manifest                         | Done    |
|  [04]   | Nx              | `tools/nx/workspace.ts`, `nx.json`, root `package.json` `nx` field, `vitest.config.ts`, `biome.json`       | Done    |
|  [05]   | Coverage        | `nx.json`, root `package.json` `nx` field, `vitest.config.ts`, `pyproject.toml`, `Directory.Build.targets` | Done    |
|  [06]   | Infrastructure  | `infra/resources.ts`, `infra/program.ts`, `infra/automation.ts`, `infra/README.md`                         | Done    |
|  [07]   | CI              | `.github/workflows/*.yml`, `.github/actions/*/action.yml`, root `package.json` `nx` field                  | Done    |
|  [08]   | Tools in use    | `eng/scripts/provision.py`, no protobuf target while `libs/contracts/` holds no `.proto` file              | Done    |
|  [09]   | Documentation   | Root, `tests/`, `apps/`, `libs/dotnet/` READMEs, `CLAUDE.md`, the clean-prose terminology table            | Done    |

## [05]-[CORRECTIONS]

Each row names a category of mistake beside the category of correction, in the order of the skill sections, and the skill's last section carries the rows without repository names. Files created per directory while an owner exists include a root `project.json`, a `project.json` beside a manifest, and a per-project tool config, and restated facts include a version in two fields, a guard around a missing dependency, a default copied into every config, and a list stated four times.

| [INDEX] | [SECTION] | [MISTAKE]                                                 | [CORRECTION]                                                     |
| :-----: | :-------: | :-------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   |   [01]    | Checks as raw tool commands, one target variant per tool  | One target per kind of work, filled by tag from `targetDefaults` |
|  [02]   |   [01]    | Target variants as separate targets                       | `check` and `write` configurations on one target                 |
|  [03]   |   [01]    | Second runner beside the task graph                       | One runner, every developer and CI action a target               |
|  [04]   |   [01]    | Raw tool commands as workflow steps and in documentation  | Workflow steps and documentation name the target                 |
|  [05]   |   [02]    | Runtimes from a machine profile outside the repository    | One installer file pinning exact runtimes                        |
|  [06]   |   [02]    | Versions restated as ranges, guards, and engine fields    | One statement per version in the manifest that owns it           |
|  [07]   |   [02]    | Tool versions pinned outside the central managers         | Tool packages run through `dotnet dnx` at the newest release     |
|  [08]   |   [02]    | Tools a machine profile installs for one repository       | Provisioning from a pinned, digest-verified release in `.cache/` |
|  [09]   |   [02]    | Git filters a profile writes into a read-only user config | The repository `.git/config` holds the filters it requires       |
|  [10]   |   [03]    | Configuration files per directory while an owner exists   | Plugin inference, the manifest `nx` field, the root config       |
|  [11]   |   [03]    | Restated facts and hedges left behind by a step           | One owner per fact, dependency in the graph, no copied default   |
|  [12]   |   [03]    | Root project rehashing root files, a discovery twice      | Root project with explicit inputs alone, one discovery per tool  |
|  [13]   |   [03]    | Coverage thresholds as gates, per-project reports alone   | Information merged once per language by one root target          |
|  [14]   |   [04]    | Repository settings and secrets project held by hand      | One `infra/` program per repository declaring its own resources  |
|  [15]   |   [04]    | IaC as project files a CLI reads                          | Automation API program owning its stack, credentials in code     |
|  [16]   |   [04]    | Application resources in the repository program           | Program under the app directory, one stack per environment       |
|  [17]   |   [05]    | Per-language setup actions restating manifest pins        | One installer action reading the manifests                       |
|  [18]   |   [05]    | One workflow per tool                                     | One workflow per concern                                         |
|  [19]   |   [05]    | Caches keyed on nothing, partial-match fallbacks          | Caches keyed on the lock file or manifest, no `restore-keys`     |
|  [20]   |   [05]    | Action references pinned per commit by a ratchet tool     | Current major tag per action                                     |
|  [21]   |   [06]    | Versions stated in project files                          | MinVer from the `v` tag, `release.yml` makes the GitHub release  |

## [06]-[SKILL_SHAPE]

`SKILL.md` holds 300 to 350 lines in 7 sections in dependency order, in instruction voice, with decision tables and rule lists, and one-line pointers to the sibling skills in the intro. The current `SKILL.md` sections on the directory map, task graph, native packaging, provisioning, isolation, CI entry, and anti-patterns fold into the new sections and references, and every fact of theirs stays.

| [INDEX] | [SECTION]             | [CONTENT]                                                                                                    |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | ENTRY_POINTS          | One runner, target shapes (inferred, by tag, root aggregate), configurations over variants, scripts          |
|  [02]   | TOOLCHAIN             | The installer and its pins, the manifests and what each owns, one statement per version, env owners          |
|  [03]   | CONFIGURATION         | Root config owns settings, per-language inputs, caches and outputs, no adjacent files, one owner per fact    |
|  [04]   | ENGINEERING_DIRECTORY | What grows in `eng/`, `infra/`, and `tools/`, the native pipeline, provisioning, isolation                   |
|  [05]   | CI                    | Each tool under CI, caching rules and their hidden problems, `.github/` file kinds, one workflow per concern |
|  [06]   | RELEASE               | Tags, MinVer, GitHub releases, no versioned code                                                             |
|  [07]   | CORRECTIONS           | Category of mistake beside the category of correction, from the plan                                         |

| [INDEX] | [REFERENCE]                      | [CONTENT]                                                             |
| :-----: | :------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `references/native-packaging.md` | Manifests, staging layout, packaging projects, from the current skill |
|  [02]   | `references/provisioning.md`     | Provisioning rules, pinned archives, host tools, idempotence          |
|  [03]   | `references/nx-targets.md`       | The worked `nx.json` shapes, local plugin, `targetDefaults` by tag    |
|  [04]   | `references/installer.md`        | The installer configuration, idiomatic files, the CI action           |
|  [05]   | `references/ci-workflow.md`      | The job graph, caches, runners, artifacts                             |
|  [06]   | `references/iac.md`              | The `infra/` program shape, adoption, state, tokens                   |

## [07]-[SEQUENCE]

Fresh agents run in sequence, one slice each, with a commit between agents and a report of at most 12 lines. Every agent reads the plan, the current skill and references, the sibling skills `dotnet-msbuild-packaging`, `pulumi`, `secrets`, and `clean-prose`, and the memories `skill-authoring-workflow`, `skill-content-standard`, `research-folder-intent`, `skill-description-process`, and `workspace-config-file-discipline` before its slice, wraps each integrated passage in an `Integrated into` comment, and reads the source again before the next move. Facts go in `SKILL.md` when leaving them out lets an agent violate a standard, and in a reference when leaving them out only slows the agent down.

| [INDEX] | [AGENT]               | [READS]                                           | [WRITES]                                              |
| :-----: | :-------------------- | :------------------------------------------------ | :---------------------------------------------------- |
|  [01]   | ENTRY_POINTS          | Sources [02], [06], `nx.json`, `package.json`     | Section [01], `references/nx-targets.md`              |
|  [02]   | TOOLCHAIN             | Sources [01], [03], [04], [10]-[12], `mise.toml`  | Section [02], `references/installer.md`               |
|  [03]   | CONFIGURATION         | Sources [02], [03], [04], [06], root config files | Section [03]                                          |
|  [04]   | ENGINEERING_DIRECTORY | Sources [05], [08], [09], current skill, `infra/` | Section [04], iac, native-packaging, provisioning     |
|  [05]   | CI                    | Source [07], `.github/`                           | Section [05], `references/ci-workflow.md`             |
|  [06]   | RELEASE, CORRECTIONS  | Source [06], the plan, `Directory.Build.props`    | Sections [06] and [07]                                |
|  [07]   | Consistency pass      | Every skill and reference                         | Duplicates removed, pointers aligned, items settled   |
|  [08]   | Review                | Plan, skill, references, integrated sources       | Facts restored, narration removed, complexity carried |
|  [09]   | Description           | The finished skill and the router line            | Description chosen by two readers                     |

The review agent is one fresh agent per file, and the description step follows the `skill-description-process` memory and ends with the `CLAUDE.md` router line updated to the description.

## [08]-[EXCLUSIONS]

The skill states rules with the category and the criterion an agent applies to the next case, and holds none of:
- Repository, product, organization, or person names, outside an identifier the ecosystem requires
- Versions, a version belongs to the manifest that pins it
- URLs, the fact stays and the citation goes
- The removed patterns: a tool manifest, a ratchet tool, machine-profile installs, mise `[tasks]`, coverage thresholds
- The removed patterns: a `project.json` per directory, `Pulumi.yaml` and stack files, `restore-keys`, raw tool commands as entry points
- Instructions already carried out in the repository, the skill describes the resulting behavior
- Paraphrased code, a target, script, or workflow is named with its purpose
