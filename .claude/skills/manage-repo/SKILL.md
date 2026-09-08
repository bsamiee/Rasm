---
name: manage-repo
description: "Use when changing eng/, infra/, tools/, mise.toml, nx.json, or .github/, covering dispatch, placement, targets, native chain, CI, and proof."
---

# [MANAGE_REPO]

Covers shared build infrastructure of a polyglot monorepo, from the toolchain and `eng/` to the CI entry point:
1. Read `README.md`, `mise.toml`, and `.vscode/settings.json`
2. List every MSBuild, TypeScript, and Python configuration file with one `fd` call:

```sh
fd -H '^(.*\.(csproj|slnx)|Directory\..*\.(props|targets)|NuGet\.config|\.editorconfig|global\.json|(nx|package|biome|project)\.json|pnpm-workspace\.yaml|vite(st)?\.config\.ts|tsconfig.*\.json|pyproject\.toml|uv\.lock)$'
```

3. Run `tree eng`, `tree tools`, `tree infra`, and `tree .github`
4. Read each touched file with its consumers

[REFERENCES]:
- [01]-[DOTNET](references/dotnet.md): .NET build, packaging, and test configuration
- [02]-[PYTHON](references/python.md): Python manifest, scripts, and checker configuration
- [03]-[TYPESCRIPT](references/typescript.md): TypeScript package, compiler, lint, and test tooling
- [04]-[TOOLING](references/tooling.md): Shared environment, toolchain, task runner, harness, and editor configuration
- [05]-[GITHUB](references/github.md): Workflows, actions, local runs under `.github/`

[TEMPLATES]:
- [01]-[WORKFLOW](templates/workflow.yml): Workflow of one event with a fan-in job
- [02]-[REUSABLE_WORKFLOW](templates/reusable-workflow.yml): `workflow_call` with typed inputs, matrix, and artifact transfer
- [03]-[ACTION](templates/action.yml): Composite action with inputs and step output

[AGENTS]:
- [01]-[DOTNET_MAINTAINER](../../agents/dotnet-maintainer.md): .NET build, restore, test, coverage, and packaging settings
- [02]-[PYTHON_MAINTAINER](../../agents/python-maintainer.md): Python dependency, checker, test, and coverage settings and scripts
- [03]-[TYPESCRIPT_MAINTAINER](../../agents/typescript-maintainer.md): TypeScript package, compiler, lint, test, and mutation settings, plugin code
- [04]-[TOOLING_MAINTAINER](../../agents/tooling-maintainer.md): Shared toolchain, task graph, root configuration, and agent integrations
- [05]-[GITHUB_MAINTAINER](../../agents/github-maintainer.md): GitHub workflows, actions, artifacts, publication, and local execution

## [01]-[DISPATCH]

Assign each shared configuration field to one owner. When a setting, command, package, or output changes, notify affected maintainers. Read an implemented change before updating consumers. Correct guidance that caused a reviewed defect. Standards are the language skills, `clean-prose`, and zero warnings from every checker.

## [02]-[APPROACH]

Decide infrastructure changes from root `README.md` and current tool documentation:

| [INDEX] | [PRINCIPLE]    | [CRITERION]                                                                                         |
| :-----: | :------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | Adoption       | Custom code extends the maintained action, plugin, or tool that performs a step                     |
|  [02]   | Cohesion       | Changed facts reach manifest, lock, target, inputs, cache key, workflow, and editor in one change   |
|  [03]   | Directness     | Lockfiles pin, action tags name a major, and an operation's exit code is its check                  |
|  [04]   | Explicitness   | Commands take each setting from an argument or tracked file, none from discovery, host, or a daemon |
|  [05]   | Naming         | New names match no tool command or keyword                                                          |
|  [06]   | Portability    | One definition of every file runs on every operating system, the host detected once as a parameter  |
|  [07]   | Infrastructure | Every resource, environment, and image is a program row, one unit per application and library       |
|  [08]   | Renewal        | Existing approaches are rebuilt in place when a documented capability or integration is better      |

Digest pins, release-age delays, and audit steps add a step with no decision behind it. Before adding a tool, package, action, or resource, read its full option set and maintained integrations, then select those that solve an observed need. Add a repository plugin or rule when no maintained one performs the step and capability spans tools.

Rebuilds for code quality, package integration, or tool capability need no new requirement. Capabilities found in one scope reach every file they touch in the same session. Add an `eng/`, `tools/`, or `infra/` category when an input, operation, check, or resource has no owner.

Program logic under `infra/`, `tools/`, `eng/scripts/`, and hooks meets the language standard and each criterion:
- Group code by operation
- Share logic repeated for one behavior
- Declare cache dependencies
- Reuse parsed inputs within an operation
- Measure behavior and performance changes before and after under the same inputs and runtime

Configuration (JSON, YAML, TOML, MSBuild XML) takes the structure its schema documents, read in full before a rebuild:
- Group repeated settings through tag or glob filters, overrides, conditioned groups, and shared defaults
- Remove values that restate defaults
- Keep settings that state a project policy
- Group differences by language, path, or role

## [03]-[PLACEMENT]

Keep entry points with the configuration that defines them:
- Project operations (`build`, `typecheck`, `test`) are one command per tag-filtered default, ordered, cached, and affected by the graph
- File-kind operations (`lint`, `format`) are one root target running `eng/scripts/quality.py`

Place additions by consumer:

| [INDEX] | [ADDITION]                           | [OWNER]                                           | [FORM]                                     |
| :-----: | :----------------------------------- | :------------------------------------------------ | :----------------------------------------- |
|  [01]   | Developer command                    | `nx.json` defaults by tag, root manifest `nx`     | One target per operation, no variant       |
|  [02]   | Automation with control flow         | `eng/scripts/`                                    | One module per operation under a target    |
|  [03]   | Build input every language consumes  | `eng/<area>/`                                     | Manifest, target, `.artifacts/` output     |
|  [04]   | Native library                       | `eng/native/<library>/` and its packaging project | Manifest pin, `stage` and `pack` targets   |
|  [05]   | Target shape repeating per directory | `tools/nx/`                                       | Plugin registered by path                  |
|  [06]   | Check the linter or compiler lacks   | `tools/<linter>/`                                 | Rule file or analyzer project it loads     |
|  [07]   | Repository, service, or environment  | `infra/`, or application program                  | Typed row, imported when it exists         |
|  [08]   | Image or runner definition           | Program row that builds it from a manifest        | No image, snapshot, or exported state file |
|  [09]   | CI step                              | `.github/`                                        | Maintained action, `run` step for the rest |
|  [10]   | Dependabot, actionlint, zizmor rule  | `.github/`, file named for the tool               | One file per tool, its schema's fields     |
|  [11]   | Editor setting                       | `.vscode/settings.json`                           | Setting keyed by language                  |
|  [12]   | Cache, download, or checkout         | `.cache/<tool>/`                                  | Relocated through tool's setting           |
|  [13]   | Build output, package, or report     | `.artifacts/<area>/`                              | Declared target output                     |

Each language area holds the binding packages that consume `eng/` output through package references.

Read shared values from the defining file:

| [INDEX] | [FACT]                     | [OWNER]                               | [READERS]                                                    |
| :-----: | :------------------------- | :------------------------------------ | :----------------------------------------------------------- |
|  [01]   | Native library version     | `eng/native/<library>/*.json`         | Packaging project version check                              |
|  [02]   | Package version            | Evaluated packaging project `Version` | Target outputs from evaluated properties or SDK output items |
|  [03]   | Local feed path            | `NuGet.config` local source           | Pack target, from the source value                           |
|  [04]   | .NET output roots          | Root `Directory.Build.props`          | Target outputs                                               |
|  [05]   | Script dependency versions | Root `pyproject.toml` and lock        | Scripts under `uv run`                                       |

## [04]-[TARGETS]

`targetDefaults` fill a declared target and create none:
- Declare a tool version a cached target reads as `{ "runtime": "<command>" }` input and environment variables as `{ "env": "<NAME>" }`
- Language named inputs hold `{ "runtime": "<command> --version" }` when the runtime comes from the toolchain `latest`
- Declare the staged tree through `dependentTasksOutputFiles`, hashed from disk under an uncached dependency
- Nx reports a cache hit with missing output dropped, every declared output must exist after a run
- Name the exact package file as output, cache restore of a glob over shared feed writes stale siblings back
- Set `parallelism: false` on every target that shares a tool root or writes a shared file
- Touched files matching a target's inputs mark the project affected

Declare staging, packaging, and consumption as dependent targets. Each Python script target runs `uv run --only-group eng python -m eng.scripts.<module>`:

| [INDEX] | [TARGET]            | [RUNS]                   | [DEPENDS_ON]                                     | [CACHE] |
| :-----: | :------------------ | :----------------------- | :----------------------------------------------- | :------ |
|  [01]   | `eng:provision`     | `eng.scripts.provision`  | Nothing                                          | `false` |
|  [02]   | `Native.Item:stage` | `eng.scripts.stage item` | `{ projects: ["eng"], target: "provision" }`     | `false` |
|  [03]   | `Native.Item:pack`  | `dotnet pack`            | `stage`                                          | `true`  |
|  [04]   | `Item:pack`         | `dotnet pack`            | `{ projects: ["Native.Item"], target: "stage" }` | `true`  |
|  [05]   | `Consumer:build`    | `dotnet build`           | `^build`, `<root>:restore`                       | `true`  |

Cache a target when its outputs are a function of declared inputs alone:

| [INDEX] | [TARGET]    | [INPUTS]                                                        | [OUTPUTS]                               | [VERDICT] |
| :-----: | :---------- | :-------------------------------------------------------------- | :-------------------------------------- | :-------- |
|  [01]   | `provision` | Network, host toolchain                                         | `.cache/<tool>/`                        | No cache  |
|  [02]   | `stage`     | Network, vcpkg toolchain, host compiler                         | `.artifacts/native/<library>/stage`     | No cache  |
|  [03]   | `pack`      | Project dir, `Directory.Build.*`, manifest dir, staged tree     | `<feed>/<Id>.<Version>.nupkg`, bin, obj | Cache     |
|  [04]   | `build`     | Sources, `Directory.Build.*`, `.editorconfig`, `^build` outputs | `ArtifactsPath` bin and obj per project | Cache     |

## [05]-[NATIVE_CHAIN]

Each native library has one manifest directory as version pin:

| [INDEX] | [SOURCE]              | [MANIFEST]                           | [PIN]                                             |
| :-----: | :-------------------- | :----------------------------------- | :------------------------------------------------ |
|  [01]   | vcpkg port            | `vcpkg.json` with `builtin-baseline` | Baseline port version, `version-string` equals it |
|  [02]   | Release archive       | `<kind>.json` with digest per rid    | Version with SHA-256 per file                     |
|  [03]   | Source checkout       | `source.json` with commit            | Commit, wrapper version follows it                |
|  [04]   | Registry-locked asset | Manifest with `CentralPackageId`     | Version equals central package version            |

Stage the layout NuGet's runtime graph reads, `dotnet pack` includes the tree without renaming:

```text
.artifacts/native/<library>/stage/
├── runtimes/<rid>/native/<file>      # Shared libraries and loadable extensions, one directory per runtime identifier
├── contentFiles/<path>               # Data trees a runtime loads by path, packed with copyToOutput
└── managed/*.cs                      # Generated binding sources a managed packaging project compiles
```

Dependencies run one way from `eng:provision` through `stage` and `pack` to feed. Consumers reach packages through feed alone.

## [06]-[ENVIRONMENT]

Every value a process reads has one owner, chosen by reader and secrecy:

| [INDEX] | [VALUE]                                | [OWNER]                                              | [READER]                                |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | Secret                                 | Secret store config of the owning environment        | `doppler run` around target             |
|  [02]   | Secret a workflow consumes             | Service token and Actions secret rows in the program | `secrets.<NAME>` on step                |
|  [03]   | Value a workflow consumes, not secret  | Actions variable row in the program                  | `vars.<NAME>` on step                   |
|  [04]   | Tool setting with a manifest field     | Manifest the tool reads by directory walk            | Tool                                    |
|  [05]   | Process setting with no manifest field | `mise.toml` `[env]`                                  | Targets, scripts, agent shell, CI steps |
|  [06]   | Process setting of one platform        | `mise.unix.toml` `[env]`                             | Same, on that platform                  |
|  [07]   | Value one target reads                 | Target `env` option beside the command               | That command                            |
|  [08]   | Path one script or program computes    | Script or program, beside other paths                | Same                                    |
|  [09]   | Output of a workflow step              | `env:` on the consuming step, from step outputs      | That step                               |

## [07]-[INFRASTRUCTURE]

Every resource outside the repository tree is a typed row a program reads by key:

| [INDEX] | [OWNER]                  | [HOLDS]                                                            |
| :-----: | :----------------------- | :----------------------------------------------------------------- |
|  [01]   | `infra/`                 | Repository settings, secret store, tokens, CI secrets              |
|  [02]   | `apps/<name>/<program>/` | Resources of one application, one stack per environment            |
|  [03]   | Pipeline                 | Every image and binary, rebuilt from a pinned manifest on each run |

Repository program declares store project, configs, tokens, repository settings, secrets, and variables:
- Store holds one project, a config per environment, branch config named `<environment>_<suffix>` for repository automation
- Runtime secrets enter branch config once, from stdin through `doppler secrets set`
- Service token rows name config and access level
- Actions secret rows hold the token key under the name workflows read
- Repository row holds merge, branch, and feature settings, `protect` refuses deletion, `archiveOnDestroy` archives on destroy
- Adopt an existing resource through import, declared as row's resource option
- `up --import` adopts project, environments, branch configs, repository, creates tokens and secrets
- Providers read credentials from environment alone
- Repository provider detects owner from the token
- Actions variable rows read values from environment under the variable name through `Config` at entry boundary
- Variables enter config once through `doppler secrets set <NAME>`
- Unset names fail the run naming them
- Ruleset rows target default branch (`~DEFAULT_BRANCH`) and release tags (`refs/tags/*@*`), with `excludes: []` beside `includes`
- Branch ruleset requires fan-in status check and names admin role (`RepositoryRole`, `actorId` 5) as bypass actor
- Environment rows hold deployment branch policy, one policy naming the branch jobs deploy from
- Actions permissions row selects GitHub-owned actions and the explicit pattern list alone, an allow list in place of digest pins
- Vulnerability alerts, secret scanning, and push protection are rows
- Public repositories take no `advancedSecurity` block
- Every table is `as const satisfies` the provider args type
- Settings under a disabled merge method leave the row
- Import ids are `<repository>:<id>` for ruleset, `<repository>:<environment>` for environment, `<repository>:<name>` for variable
- `@pulumiverse/doppler` lags upstream, with no GitHub integration, change-request policy, or rotated secret
- Neither provider models trusted publishers, nuget.org, PyPI, and npm publishers stay account settings

Program dependencies sit in the root catalog and manifest. Root `up` and `refresh` run the program entry under `doppler run --project <project> --config <config>`, with the resource change summary as proof:
- Entry runs the stack through the Automation API on Pulumi Cloud with `PULUMI_ACCESS_TOKEN` and service secrets from the environment
- Plugins sit under `.cache/pulumi/`
- Inline project without `Pulumi.yaml` runs the `pulumi` CLI through the entry alone
- Each run prints the operation output and its resource-change JSON
- Failed selects and operations print the diagnostic

Application programs share nothing by position. Applications consume another's output through a published package or declared output.

## [08]-[CI]

CI runs the task graph as one job per language. Workflow files hold commands alone.

## [09]-[PROOF]

Proof of a change is a run in the session that raised it, traced from the entry point to its last output against a baseline recorded before the first edit:
- Run the target, a cached target proves its inputs by a hit after an unrelated edit and a miss after a related one
- Run `nx graph --file=<path>` and `nx show projects --affected --files=<manifest>` after an edge changes
- Graph output holds the static edge from each consumer to its packaging project beside `ProjectReference` edges
- Affected listing for a manifest names the packaging project, binding, and consumers
- Compare `git diff | shasum` before and after a rewriting target, equal hashes prove it rewrote nothing
- Read each output of every target, cache entry, and workflow step the change touches
- Fix a wrong or missing output, leftover file, process that outlives its run, or step that passes with no effect at its cause
- Read `git log -p <file>` over each rebuilt document or configuration before committing
- Restore each criterion, capability, command flag, and purpose statement a rebuild dropped or loosened
- Delete probe projects and generated outputs after reading the result

## [10]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                 | [CORRECT_FORM]                                                        |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `stage-<x>` and `pack-<x>` target pairs per library     | Local plugin infers `stage` and `pack` from each project file         |
|  [02]   | Committed `.nupkg`, `.dylib`, or `.so` files            | Manifest pin, staging target, and an ignored `.artifacts/` feed       |
|  [03]   | `cache: false` on a target with pure outputs            | `cache: true` with inputs, `dependentTasksOutputFiles`, exact outputs |
|  [04]   | Configurable outputs outside the shared roots           | `.artifacts/<area>/` outputs and `.cache/` caches                     |
|  [05]   | Target commands that run `nx run` or `pnpm nx`          | `dependsOn` as `{ "projects": ["<project>"], "target": "<target>" }`  |
|  [06]   | README step lists for machine setup                     | Provisioning target documented in the root README                     |
|  [07]   | Machine paths in a script, project, or manifest         | Root lock file, `$(MSBuildThisFileDirectory)`, or `{workspaceRoot}`   |
|  [08]   | Preview, check, or dry-run variants of a target         | One target, `git diff --exit-code` in CI after the rewriting targets  |
|  [09]   | Configuration files per directory where an owner exists | Plugin inference, manifest `nx` field, or root config                 |
|  [10]   | `scripts` in a manifest beside the targets              | Target, the one entry the graph orders and caches                     |
|  [11]   | Repeated language build commands in a target            | Project filters, with shared operations in root targets               |
