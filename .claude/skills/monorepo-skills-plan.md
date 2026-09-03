# [MONOREPO_SKILLS_PLAN]

One skill, `monorepo-build-infrastructure`, encodes how the shared build infrastructure of a polyglot monorepo is organized: entry points, the toolchain, configuration ownership, the engineering directory, CI, and release. Its material is the research under `docs/research/monorepo/`, and the repository is corrected before the skill is written, so the skill describes behavior the repository already shows. The plan records the sources and their owners, the ownership split with the sibling skills, the repository steps with the category of mistake each corrects, the target shape of the skill, and the agent sequence.

## [01]-[SOURCES]

Each file is a verified research report, rewritten under the clean-prose rules, with a first-line HTML comment naming its owner and its integration state.

| [INDEX] | [FILE]                    | [CONTENT]                                                                    | [OWNER]                                        |
| :-----: | :------------------------ | :--------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `toolchain-installer.md`  | Installer comparison, mise file shape, environment variable owners, `mise-action` | [02]-[TOOLCHAIN], `references/installer.md`    |
|  [02]   | `nx-capabilities.md`      | Plugin inference, local plugins, `targetDefaults`, inputs, env, affected, cache, sync | [01]-[ENTRY_POINTS], [03], `references/nx-targets.md` |
|  [03]   | `pnpm-capabilities.md`    | Settings file, catalogs, lockfile, `packageManager` and `devEngines`, project references | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]         |
|  [04]   | `uv-capabilities.md`      | Workspaces, groups, interpreter settings, `uv run`, tools, cache, CI            | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]         |
|  [05]   | `eng-directories.md`      | `eng/` in 24 .NET repositories, recurring practices, the official feature per practice | [04]-[ENGINEERING_DIRECTORY]                   |
|  [06]   | `polyglot-monorepos.md`   | 15 polyglot repositories, Nx specifics, corrections their history records       | [03]-[CONFIGURATION], [07]-[CORRECTIONS]       |
|  [07]   | `github-actions.md`       | File kinds under `.github/`, workflow design, caching, each language under CI   | [05]-[CI], `references/ci-workflow.md`         |
|  [08]   | `pulumi.md`               | Placement of a repository-owned program, adoption, state, tokens, Nx targets    | [04], `references/iac.md`, the `pulumi` skill  |
|  [09]   | `doppler.md`              | Live Doppler and 1Password state, the ownership split, CI route, declarations   | `references/iac.md`, the `secrets` skill       |
|  [10]   | `globaljson.md`           | Readers of `global.json`, what mise does with it, field by field verdict        | [02]-[TOOLCHAIN], [03]-[CONFIGURATION]         |
|  [11]   | `dotnet-tool-manifest.md` | Coverage merging, diagnostics, the five manifest entries, rejected tools        | [02]-[TOOLCHAIN]                               |
|  [12]   | `mcp-coverage.md`         | The two MCP servers, SDK-provided commands, manifest pinning                    | [02]-[TOOLCHAIN]                               |

The two Parametric_Forge inventories (flake and language tools) feed the migration steps and the Forge removal memory, not the research folder.

## [02]-[OWNERSHIP]

Each skill owns one kind of fact, and a fact appears in the skill that owns it:
- `monorepo-build-infrastructure` owns the decisions: one runner, target shapes, what each manifest pins, where a setting lives, what grows in `eng/`, `infra/`, `tools/`, and `.config/`, how CI and release are shaped, and the categories of correction
- `dotnet-msbuild-packaging` owns central package management, `NuGet.config`, lock files, `.slnx`, package projects, and the MSBuild properties CI passes
- `dotnet-msbuild-evaluation`, `dotnet-msbuild-execution`, `dotnet-msbuild-antipatterns`, and `dotnet-msbuild-diagnostics` own the MSBuild files, targets, review, and binlogs
- `pulumi` owns the program mechanics: resources, adoption through import, state backends, previews, and destroys
- `secrets` owns where a secret lives: 1Password for credentials a person uses, Doppler for runtime secrets, and how a token reaches a process

`infra/` holds the repository's own resources alone: the repository settings and the Doppler project for the repository's runtime secrets. An application that provisions anything owns its Pulumi program and its Doppler project inside its own directory under `apps/`, with one stack per environment, and reads what it needs from `infra/` through stack references. Libraries own no program.

The skill names no repository, product, or person. It states rules with the category and the criterion an agent applies to the next case, and it grows a corrections row whenever a repository change corrects a category of mistake.

## [03]-[PROCESS]

- Repository work runs as sequential steps with one commit each, and each step runs the checks it touches to zero warnings before its commit
- Every step adds one row to [07] naming the category of mistake and the category of correction
- The skill is built after the repository work by fresh agents in sequence, one section each, with a commit between agents and a report of at most 12 lines
- A section is written from the research file that owns it, the integrated passage is wrapped in an `Integrated into` comment, and the file is read again before the next move
- Facts go in `SKILL.md` when leaving them out lets an agent violate a standard, and in a reference when leaving them out only slows the agent down
- Descriptions are written last through the two-reader process

## [04]-[REPOSITORY_STEPS]

| [INDEX] | [STEP]           | [FILES]                                                                                   | [STATE]  |
| :-----: | :--------------- | :---------------------------------------------------------------------------------------- | :------- |
|  [01]   | Research folder  | `docs/research/monorepo/*.md`, this plan                                                  | Done     |
|  [02]   | Toolchain        | `mise.toml`, `package.json`, `pnpm-workspace.yaml`, `pyproject.toml`, `.vscode/settings.json` | Done     |
|  [03]   | .NET tools       | `.mcp.json`, `Directory.Build.props`, `Directory.Packages.props`, no tool manifest               | Done     |
|  [04]   | Nx               | `tools/nx/workspace.ts`, `nx.json`, root `package.json` `nx` field, `vitest.config.ts`, `biome.json` | Done     |
|  [05]   | Coverage         | `nx.json`, root `package.json` `nx` field, `vitest.config.ts`, `pyproject.toml`, `Directory.Build.targets`, `tests/README.md` | Done     |
|  [06]   | Infrastructure   | `infra/resources.ts`, `infra/program.ts`, `infra/automation.ts`, `infra/README.md`        | Done     |
|  [07]   | CI               | `.github/workflows/ci.yml`, `.github/workflows/release.yml`                               | Pending  |
|  [08]   | Tools in use     | KTX2 CLI and `git lfs install` in provisioning, protobuf generation target                | Pending  |
|  [09]   | Documentation    | Root, `tests/`, `apps/`, `libs/dotnet/` READMEs, `CLAUDE.md`, the Forge removal memory    | Pending  |

Decisions the steps recorded:
- The root `Directory.Build.targets` stays one file, because its conditions select by host token and role and a split by role would add files without removing a condition
- The dylib repair in `Directory.Build.targets` retargets a NuGet package's library at copy time and the repair in `eng/scripts/stage.py` relinks a closure the workspace builds, two inputs with two owners
- No tool manifest exists: `dotnet dnx` runs Stryker, ReportGenerator, and the two MCP servers at the newest release, and `binlogtool` stays out because the two commands the binlog MCP lacks, `redact` and `stats`, serve a binlog that leaves the machine or exceeds the MCP threshold, and neither exists
- The root project lives in the `nx` field of the root `package.json`, a `project.json` exists only for a directory no manifest describes (`eng/`, `tests/python/`), and `tools/nx` belongs to the root project, whose `lint` and `format` commands name `tools` and whose `tsconfig.json` includes its files
- The workspace plugin names no project it did not create alone: a `.csproj` or `package.json` node carries tags and empty check targets and takes its name from the plugin that reads the manifest, a `package.json` counts only with a `tsconfig.json` beside it, and a `pyproject.toml` node is named by its root path with `-` for `/`
- The dotnet `format` target discovers the one project file in its `cwd` and depends on `rasm-workspace:restore`, because `--no-restore` reads the assets restore wrote
- The root `check` depends on the root `lint`, `format`, and `typecheck` beside every other project's `check`, the root `typecheck` builds the root `tsconfig.json` alone (the root config files, `tools/nx`, `infra`, and the support `vitest.config.ts`) into `.cache/typescript/out/root`, the root holds no `references` and no implicit dependency, and `^typecheck` orders the project typechecks
- The per-language named inputs take the root files out of `sharedGlobals`, which keeps `nx.json` and `mise.toml`, and the `typescript` input carries the root `vite.config.ts` and `vitest.config.ts` that every Vitest project imports
- Every `vitest.config.ts` is a complete standalone configuration from `createVitestConfig`, the results, benchmark, and coverage paths split by directory name, and the root configuration serves the mutation runner and the `--merge-reports` merge, with `projects` mirroring the `pnpm-workspace.yaml` packages globs
- `Rasm.Policy.Analyzers` declares no `AnalyzerReleases.*.md` items, the `Microsoft.CodeAnalysis.Analyzers` build assets add them, and the duplicate made `dotnet format` report a workspace warning
- Coverage and mutation score are information: no threshold in `vitest.config.ts`, `pyproject.toml`, `stryker.config.json`, `stryker-config.json`, or the merge, and the root `coverage` target merges once per language with `cache: false`
- Every `test` run collects coverage with no `coverage` configuration: Python through `--cov={projectRoot}` in the `test` command, TypeScript through `coverage.enabled` in `createVitestConfig`, and .NET through `TestingPlatformCommandLineArguments` under the `RasmRole` `tests` condition in `Directory.Build.targets`, and `benchmark` stays a Python `test` configuration
- The root `coverage` target depends on every other project's `test` target and runs the Python and TypeScript merge commands without shell guards, and ReportGenerator exits 1 on an empty glob, so the commit that adds the first .NET test project adds the `dotnet dnx dotnet-reportgenerator-globaltool` merge command and the `.artifacts/dotnet/coverage` output
- pytest-cov combines its own parallel files and saves them to the base data file, so the Python `test` entry sets `COVERAGE_FILE` to `.artifacts/python/coverage/data/.coverage.{projectName}` and names that file as its output, `[tool.coverage.run] data_file` names `.coverage` in the same directory, and the root `coverage combine` merges the per-project files beside it into the report data file
- `--allow-roll-forward` stays on `dotnet dnx dotnet-stryker` and the future `dotnet dnx dotnet-reportgenerator-globaltool` command, both packages include `net8.0` tools alone and the workspace runtime is .NET 10
- The `act`, `actionlint`, `ratchet`, `git-lfs`, and `pulumi` pins and `PULUMI_HOME` stay ahead of the steps that run them, and `min_version` left `mise.toml`
- `biome.json` parses as strict JSON and Biome offers no shared include for plugins, so each plugin entry and the override state the same include list
- `minimumReleaseAgeExclude` in `pnpm-workspace.yaml` exempts the packages published inside the 1440-minute `minimumReleaseAge` pnpm applies by default, and the block goes once every listed release is older than that
- Vitest `--merge-reports` resolves each blob by project name and recomputes the file id from that project's root, and the root `vitest.config.ts` declares `projects`, drops the blob reporter, and owns the merged `lcovonly` and `json` paths
- Every Vitest project run writes its blob to `.artifacts/typescript/test-results/.vitest-reports/<name>.json`, a path outside the inferred outputs, and the merge reads every blob in that directory
- `mutmut` leaves `tests/README.md`, it is not installed and no `[tool.mutmut]` table exists, the Python `benchmark` configuration exits 5 while no benchmark test exists, and `@nx/vitest` infers no `bench` target, and TypeScript benchmarks stay on the benchmark include glob
- The `infra/` program runs on the Automation API: `LocalWorkspace.createOrSelectStack` with an inline program, project settings and the `file://${XDG_STATE_HOME:-$HOME/.local/state}/rasm-infra` backend in code, the passphrase secrets provider, explicit `github.Provider` and `doppler.Provider` resources, and no `Pulumi.yaml`, stack file, `package.json`, or `tsconfig.json` under `infra/`, the root `tsconfig.json` includes the files and the root `lint`, `format`, and `typecheck` cover them
- The `infra/` credentials resolve in `automation.ts`: `PULUMI_CONFIG_PASSPHRASE` from the ambient variable else `op read op://Tokens/PULUMI_RASM_INFRA/password`, `DOPPLER_TOKEN` from the ambient variable else `doppler configure get token --plain`, and `GITHUB_TOKEN` from ambient `GITHUB_TOKEN` or `GH_TOKEN` else `op read op://Tokens/GITHUB_TOKEN/token`, each passed as a provider input or the engine environment, with no write to a repository file
- The `preview`, `up`, and `refresh` targets sit on the root project uncached with `parallelism: false`, `up` prints the plan and asks at the prompt with no auto-approval, and `preview:expect-no-changes` is the drift gate (`preview --refresh --expect-no-changes`)
- Secret values are `doppler secrets set` writes against a declared config with no `doppler.Secret` row, the CI token reaches GitHub as a `github.ActionsSecret` row fed by the `doppler.ServiceToken` key, and `BUF_TOKEN` left 1Password once Doppler held the identical value

## [05]-[SKILL_SHAPE]

`SKILL.md` holds 300 to 350 lines in 7 sections in dependency order, in instruction voice, with decision tables and rule lists, and one-line pointers to the sibling skills in the intro.

| [INDEX] | [SECTION]             | [CONTENT]                                                                                          |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | ENTRY_POINTS          | One runner, target shapes (inferred, by tag, root aggregate), configurations over variants, scripts |
|  [02]   | TOOLCHAIN             | The installer and what it pins, the manifests and what each owns, one statement per version, env placement |
|  [03]   | CONFIGURATION         | Root config owns settings, per-language inputs, caches and outputs, no adjacent files, one owner per fact |
|  [04]   | ENGINEERING_DIRECTORY | What grows in `eng/`, `infra/`, `tools/`, `.config/`, and what each holds                          |
|  [05]   | CI                    | Each tool under CI, caching rules and their hidden problems, `.github/` file kinds, one workflow per concern |
|  [06]   | RELEASE               | Tags, MinVer, GitHub releases, no versioned code                                                   |
|  [07]   | CORRECTIONS           | Category of mistake beside the category of correction, from [07] below                              |

| [INDEX] | [REFERENCE]                       | [CONTENT]                                                                       |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `references/native-packaging.md`  | Manifests, staging layout, packaging projects, from the current skill           |
|  [02]   | `references/provisioning.md`      | Provisioning rules, pinned archives, host tools, idempotence                    |
|  [03]   | `references/nx-targets.md`        | The worked `nx.json` shapes, local plugin, `targetDefaults` by tag              |
|  [04]   | `references/installer.md`         | The installer configuration, idiomatic files, the CI action                     |
|  [05]   | `references/ci-workflow.md`       | The job graph, caches, runners, artifacts                                       |
|  [06]   | `references/iac.md`               | The `infra/` program shape, adoption, state, tokens                             |

## [06]-[SEQUENCE]

| [INDEX] | [AGENT]                    | [SOURCES]                                        | [OUTPUT]                                           |
| :-----: | :------------------------- | :----------------------------------------------- | :------------------------------------------------- |
|  [01]   | ENTRY_POINTS, nx-targets   | nx-capabilities, polyglot-monorepos [03]         | Section [01], `references/nx-targets.md`           |
|  [02]   | TOOLCHAIN, installer       | toolchain-installer, globaljson, pnpm, uv, tool manifest, mcp | Section [02], `references/installer.md`    |
|  [03]   | CONFIGURATION              | nx, pnpm, uv, polyglot-monorepos                 | Section [03]                                       |
|  [04]   | ENGINEERING_DIRECTORY, iac | eng-directories, pulumi, doppler, current skill  | Section [04], `references/iac.md`, native-packaging, provisioning |
|  [05]   | CI, ci-workflow            | github-actions                                   | Section [05], `references/ci-workflow.md`          |
|  [06]   | RELEASE, CORRECTIONS       | tool manifest [04.2], polyglot-monorepos [04], this plan | Sections [06] and [07]                     |
|  [07]   | Consistency pass           | Every skill and reference                        | Duplicates removed, pointers aligned, open items settled |
|  [08]   | Review                     | Plan, skill, references, integrated sources      | Facts restored, narration removed, complexity carried |
|  [09]   | Description                | The finished skill and the router line           | Description chosen by two readers                  |

## [07]-[CORRECTIONS]

Each repository step corrects one category of mistake, and the skill's last section carries these rows without repository names.

| [INDEX] | [STEP] | [MISTAKE]                                                                     | [CORRECTION]                                                                    |
| :-----: | :----: | :---------------------------------------------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   |  [01]  | Source material scattered across plan files and session scratch directories   | One research folder per skill area, one file per report, integration markers   |
|  [02]   |  [02]  | Runtimes installed by a machine profile outside the repository, versions restated as ranges and guards | One installer file pinning exact runtimes, each manifest pinning its own packages, one statement per version |
|  [03]   |  [03]  | A tool installed by the machine for one repository's checks, a per-tool manifest pinning versions outside the central package managers | Tool packages run through `dotnet dnx` at the newest release from the target that needs them, versions stated only in the central package managers |
|  [04]   |  [04]  | Checks run as raw commands and per-tool target variants, root files rehashed by a root project, a discovery declared twice | One target name per kind of work filled by tag from `targetDefaults` with `check` and `write` configurations, a root project with explicit inputs alone, one discovery per tool |
|  [05]   |  [04]  | A configuration file created per directory while an existing owner holds the setting (a root `project.json`, a `project.json` beside a manifest, a per-project tool config) | The existing owner: plugin inference, the manifest's `nx` field, `tsconfig.base.json`, the root tool config, and a new file only for a directory no manifest describes |
|  [06]   |  [05]  | Coverage thresholds as gates, per-project reports with no aggregate, raw tool commands in documentation | Information reports merged once per language by one root target, `test` configurations for collection, documentation naming targets and configurations |
|  [07]   |  [06]  | A repository's settings and its secrets project held by hand or by another repository's program, IaC written as project files a CLI reads in place of a program that owns its stack | One `infra/` program per repository on the Automation API that declares its own resources as typed rows, imports the live ones, resolves its credentials in code, and runs from root targets with no project or stack file |
|  [08]   | Audit  | Restated facts and hedges left behind by a step: a version in two fields, a guard around a missing dependency, a default copied into every config, a list stated four times | One owner per fact, the dependency in the graph, the default left unstated, the list stated once |
