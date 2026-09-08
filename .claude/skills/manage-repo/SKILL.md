---
name: manage-repo
description: "Use when changing eng/, infra/, tools/, mise.toml, nx.json, or .github/, or orchestrating repository-wide tooling work, covering placement, targets, native packaging, toolchain, environment, infrastructure, CI, and proof."
---

# [MANAGE_REPO]

Covers the shared build infrastructure of a polyglot monorepo, from the toolchain and the `eng/` directory to the CI entry point:
1. Read `README.md`, `mise.toml`, and `.vscode/settings.json`
2. Run one `fd` call to list every MSBuild, TypeScript, and Python file:

```sh
fd -H '^(.*\.(csproj|slnx)|Directory\..*\.(props|targets)|NuGet\.config|\.editorconfig|global\.json|(nx|package|biome|project)\.json|pnpm-workspace\.yaml|vite(st)?\.config\.ts|tsconfig.*\.json|pyproject\.toml|uv\.lock)$'
```

3. Run `tree eng`, `tree tools`, `tree infra`, and `tree .github`, one call per directory, and read a touched file with its consumers
4. Use `work-execution` for the briefs, dependency order, coordinated edits, and independent review of repository-wide work

[REFERENCES]:
- [01]-[DOTNET](references/dotnet.md): .NET build, packaging, and test configuration
- [02]-[PYTHON](references/python.md): Python manifest, scripts, and checker configuration
- [03]-[TYPESCRIPT](references/typescript.md): TypeScript package, compiler, lint, and test tooling
- [04]-[TOOLING](references/tooling.md): Shared environment, toolchain, task runner, harness, and editor configuration
- [05]-[GITHUB](references/github.md): Everything under `.github/`, the workflows, the actions, and the local run

[TEMPLATES]:
- [01]-[WORKFLOW](templates/workflow.yml): Workflow of one event with its fan-in job
- [02]-[REUSABLE_WORKFLOW](templates/reusable-workflow.yml): `workflow_call` with typed inputs, a matrix, and the artifact handoff
- [03]-[ACTION](templates/action.yml): Composite action with inputs and a step output

[AGENTS]:
- [01]-[DOTNET_MAINTAINER](../../agents/dotnet-maintainer.md): .NET build, restore, test, coverage, and packaging settings
- [02]-[PYTHON_MAINTAINER](../../agents/python-maintainer.md): Python dependency, checker, test, and coverage settings, and the scripts
- [03]-[TYPESCRIPT_MAINTAINER](../../agents/typescript-maintainer.md): TypeScript package, compiler, lint, test, and mutation settings, plugin code
- [04]-[TOOLING_MAINTAINER](../../agents/tooling-maintainer.md): Shared toolchain, task graph, root configuration, and agent integrations
- [05]-[GITHUB_MAINTAINER](../../agents/github-maintainer.md): GitHub workflows, actions, artifacts, publication, and local execution

## [01]-[DISPATCH]

Assign shared configuration fields to one owner and notify affected maintainers when a setting, command, package, or output changes. Read the implemented change before updating its consumers. Correct guidance that caused a reviewed defect. The standards are the language skills, `clean-prose`, and zero warnings from every checker.

## [02]-[APPROACH]

Every infrastructure change is decided from the root `README.md` and the current documentation of the tool, applied to every file that holds a part of it, and proven by a run in the session that raised it:

| [INDEX] | [PRINCIPLE]    | [CRITERION]                                                                                                    |
| :-----: | :------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | Currency       | Each tool, action, package, and method is its newest release in the form its current documentation states      |
|  [02]   | Adoption       | Maintained actions, plugins, and tools that perform a step are adopted, and custom code extends them           |
|  [03]   | Cohesion       | Changed facts reach every reader in one change: manifest, lock, target, inputs, cache key, workflow, editor    |
|  [04]   | Directness     | Lockfiles pin, action tags name a major, and an operation runs once with its own output as the check           |
|  [05]   | Completion     | Every question resolves against the root `README.md` and the tool documentation, and the change lands proven   |
|  [06]   | Portability    | One definition of every file runs on every operating system, and the host is detected once as a parameter      |
|  [07]   | Infrastructure | Every resource, environment, and image is a program row, and each application and library is its own unit      |
|  [08]   | Terminology    | Every name is the established term of its tool, of CI/CD, or of software engineering, wherever the name exists |
|  [09]   | Renewal        | Existing approaches are rebuilt in place when a documented capability or integration is objectively better     |

Research the operation before adding a tool, package, action, or resource:
- Resolve API and configuration facts through `search-context7`, known pages through `search-tavily`, and repository source through the `github` MCP
- Read the full option set and the maintained integrations, then select those that solve an observed need
- Prefer the language, adopted package, or maintained plugin over handwritten equivalents
- Resolve newest releases, including prereleases, through the owning package manager and runtime configuration
- Add a plugin or rule of the repository's own when no maintained one performs the step and the capability spans tools

A rebuild for code quality, for the integration of a package, or for a capability the tool offers needs no new requirement, and a capability found in one scope reaches every file it touches in the same session. Add a workflow when a repository event or schedule requires it. Add an `eng/`, `tools/`, or `infra/` category when an input, operation, check, or resource has no suitable owner. Unused options and plugin catalogs do not justify additions alone.

Guards, retries, fallbacks, digest pins, release-age delays, and audit steps add a step with no decision behind it, and the exit code of the operation is the check.

Program logic under `infra/`, `tools/`, `eng/scripts/`, and the hooks meets the language standard and these criteria:
- Use package types directly and remove redundant types, schemas, and classes while preserving domain constraints
- Group code by operation, and share repeated logic when it expresses the same behavior
- Use the package API for the operation (Effect runtime, Nx devkit) instead of recreating its behavior
- Declare cache dependencies and reuse parsed inputs within an operation instead of repeating reads or builds
- Measure behavior and performance changes before and after under the same inputs and runtime

Configuration in any format (JSON, YAML, TOML, MSBuild XML) takes the structure its schema documents, read in full before the rebuild:
- Group repeated settings through tag or glob filters, overrides, conditioned groups, and shared defaults
- Remove values that only restate defaults, and retain settings that establish a required project policy
- State each fact once, and group differences by language, path, or role

## [03]-[PLACEMENT]

Keep entry points and guidance with the configuration that defines them:
- Expose developer operations as Nx targets, with shared defaults in `nx.json` and root declarations in the manifest
- Select language builds and checks through project filters, and group shared operations in their root target (`rules`, `upgrade`)
- Use established target and file names, with repository prefixes only where the ecosystem requires them
- Command and tool guidance has one owner, and every other document states its purpose and points there in one line

Place additions by their consumer:

| [INDEX] | [ADDITION]                           | [OWNER]                                           | [FORM]                                     |
| :-----: | :----------------------------------- | :------------------------------------------------ | :----------------------------------------- |
|  [01]   | Developer command                    | `nx.json` defaults by tag, the root manifest `nx` | One target per operation, no variant       |
|  [02]   | Automation with control flow         | `eng/scripts/`                                    | One module per operation under a target    |
|  [03]   | Build input every language consumes  | `eng/<area>/`                                     | Manifest, target, `.artifacts/` output     |
|  [04]   | Native library                       | `eng/native/<library>/` and its packaging project | Manifest pin, `stage` and `pack` targets   |
|  [05]   | Target shape repeating per directory | `tools/nx/`                                       | Plugin registered by path                  |
|  [06]   | Check the linter or compiler lacks   | `tools/<linter>/`                                 | Rule file or analyzer project it loads     |
|  [07]   | Runtime or binary a process runs     | `mise.toml` `[tools]`                             | `latest`                                   |
|  [08]   | Package a config or editor reads     | Language manager catalog and lock                 | Catalog entry, `upgrade` moves it          |
|  [09]   | Repository, service, or environment  | `infra/`, or the application's own program        | Typed row, imported when it exists         |
|  [10]   | Image or runner definition           | Program row that builds it from a manifest        | No image, snapshot, or exported state file |
|  [11]   | CI step                              | `.github/`                                        | Maintained action, `run` step for the rest |
|  [12]   | Dependabot, actionlint, zizmor rule  | `.github/`, the file named for the tool           | One file per tool, its schema's fields     |
|  [13]   | Editor setting                       | `.vscode/settings.json`                           | Setting keyed by language                  |
|  [14]   | Cache, download, or checkout         | `.cache/<tool>/`                                  | Relocated through the tool's own setting   |
|  [15]   | Build output, package, or report     | `.artifacts/<area>/`                              | Declared target output                     |

Composition roots belong to `apps/<name>/`, library code to `libs/`, tool configuration to the root manifests, and every binary to the pipeline, which rebuilds it from a pinned manifest. Each language area holds the binding packages that consume `eng/` output through package references, and a new language area takes the same shape.

Read shared values from their defining file:

| [INDEX] | [FACT]                     | [OWNER]                               | [READERS]                                                    |
| :-----: | :------------------------- | :------------------------------------ | :----------------------------------------------------------- |
|  [01]   | Native library version     | `eng/native/<library>/*.json`         | Packaging project version check                              |
|  [02]   | Package version            | Evaluated packaging project `Version` | Target outputs from evaluated properties or SDK output items |
|  [03]   | Local feed path            | `NuGet.config` local source           | Pack target, from the source value                           |
|  [04]   | .NET output roots          | Root `Directory.Build.props`          | Target outputs                                               |
|  [05]   | Script dependency versions | Root `pyproject.toml` and lock        | Scripts under `uv run`                                       |
|  [06]   | Runtime and binary set     | `mise.toml` `[tools]`                 | mise on a machine, the setup action on CI                    |

## [04]-[TARGETS]

Nx infers targets from the files a plugin globs, and a `project.json` exists only where no plugin recognizes a file. `targetDefaults` fill a target a project declares and create none, and a root target exists when the root manifest `nx` field declares it. Target rules:
- Register one local plugin under `tools/` by path for a target shape that repeats per directory
- Declare a tool version or environment variable a cached target reads as a `{ "runtime": "<command>" }` or `{ "env": "<NAME>" }` input
- Language named inputs hold `{ "runtime": "<command> --version" }` when the runtime comes from the toolchain at `latest`
- Write a cross-project dependency in the object form, `{ "projects": ["<project>"], "target": "<target>" }`
- Declare the staged tree through `dependentTasksOutputFiles`, and Nx hashes it from disk even for an uncached dependency
- Every declared output exists after a run, because Nx drops a missing output silently and still reports a hit
- Name the exact package file as the output, a cache restore rewrites each declared output and a glob over a shared feed writes stale siblings back
- Set `parallelism: false` on every target that shares a tool root or writes a shared file
- The runner reads the exit code alone, and a script reports failure by exiting nonzero
- Touched files matching a target's inputs mark the project affected, and `nx affected` is correct when every edge exists in the graph

Declare staging, packaging, and consumption as dependent targets. Each Python script target runs `uv run --only-group eng python -m eng.scripts.<module>`:

| [INDEX] | [TARGET]            | [RUNS]                   | [DEPENDS_ON]                                     | [CACHE] |
| :-----: | :------------------ | :----------------------- | :----------------------------------------------- | :------ |
|  [01]   | `eng:provision`     | `eng.scripts.provision`  | Nothing                                          | `false` |
|  [02]   | `Native.Item:stage` | `eng.scripts.stage item` | `{ projects: ["eng"], target: "provision" }`     | `false` |
|  [03]   | `Native.Item:pack`  | `dotnet pack`            | `stage`                                          | `true`  |
|  [04]   | `Item:pack`         | `dotnet pack`            | `{ projects: ["Native.Item"], target: "stage" }` | `true`  |
|  [05]   | `Consumer:build`    | `dotnet build`           | `^build`, `<root>:restore`                       | `true`  |

Cache a target when its outputs are a function of its declared inputs alone:

| [INDEX] | [TARGET]    | [INPUTS]                                                        | [OUTPUTS]                               | [VERDICT] |
| :-----: | :---------- | :-------------------------------------------------------------- | :-------------------------------------- | :-------- |
|  [01]   | `provision` | Network, host toolchain                                         | `.cache/<tool>/`                        | No cache  |
|  [02]   | `stage`     | Network, vcpkg toolchain, host compiler                         | `.artifacts/native/<library>/stage`     | No cache  |
|  [03]   | `pack`      | Project dir, `Directory.Build.*`, manifest dir, staged tree     | `<feed>/<Id>.<Version>.nupkg`, bin, obj | Cache     |
|  [04]   | `build`     | Sources, `Directory.Build.*`, `.editorconfig`, `^build` outputs | `ArtifactsPath` bin and obj per project | Cache     |

## [05]-[NATIVE_CHAIN]

Each native library has one manifest directory as its single version pin:

| [INDEX] | [SOURCE]              | [MANIFEST]                           | [PIN]                                             |
| :-----: | :-------------------- | :----------------------------------- | :------------------------------------------------ |
|  [01]   | vcpkg port            | `vcpkg.json` with `builtin-baseline` | Baseline port version, `version-string` equals it |
|  [02]   | Release archive       | `<kind>.json` with a digest per rid  | Version plus SHA-256 per file                     |
|  [03]   | Source checkout       | `source.json` with a commit          | Commit, the wrapper version follows it            |
|  [04]   | Registry-locked asset | Manifest plus `CentralPackageId`     | Version equals the central package version        |

Stage the layout NuGet's runtime graph reads, `dotnet pack` includes the tree without renaming:

```text
.artifacts/native/<library>/stage/
├── runtimes/<rid>/native/<file>      # Shared libraries and loadable extensions, one directory per runtime identifier
├── contentFiles/<path>               # Data trees a runtime loads by path, packed with copyToOutput
└── managed/*.cs                      # Generated binding sources a managed packaging project compiles
```

The chain runs in one direction:
- Every `stage` target depends on `eng:provision`
- Consumers reach a package through the feed alone

## [06]-[TOOLCHAIN]

Configure runtimes, binary resolution, and the process environment in `mise.toml`. Use `latest` unless the host SDK or interpreter compatibility requires a constrained release. Keep the constraint with its reason in the runtime configuration and the invocation in Nx:

| [INDEX] | [KIND]                                                 | [OWNER]                               | [EXAMPLES]                             |
| :-----: | :----------------------------------------------------- | :------------------------------------ | :------------------------------------- |
|  [01]   | Binary a target, script, workflow, or agent shell runs | `[tools]` at `latest`                 | node, pnpm, uv, act, doppler, buf      |
|  [02]   | Package code, a config, a plugin, or an editor reads   | Package manager and its lock          | `@biomejs/biome`, `typescript`, `ruff` |
|  [03]   | .NET tool package a target runs                        | `dotnet dnx <tool>` on the command    | dotnet-stryker, dotnet-outdated-tool   |
|  [04]   | Host SDK a plugin host binds to one version            | `global.json`, `rollForward` disabled | .NET SDK                               |
|  [05]   | Machine tooling the workspace never invokes            | Machine profile                       | Tools no workspace command consumes    |

## [07]-[ENVIRONMENT]

Every value a process reads has one owner, chosen by who reads it and whether it is secret:

| [INDEX] | [VALUE]                                | [OWNER]                                              | [READER]                                |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | Secret                                 | Secret store config of the owning environment        | `doppler run` around the target         |
|  [02]   | Secret a workflow consumes             | Service token and Actions secret rows in the program | `secrets.<NAME>` on the step            |
|  [03]   | Value a workflow consumes, not secret  | Actions variable row in the program                  | `vars.<NAME>` on the step               |
|  [04]   | Tool setting with a manifest field     | Manifest the tool reads by directory walk            | Tool                                    |
|  [05]   | Process setting with no manifest field | `mise.toml` `[env]`                                  | Targets, scripts, agent shell, CI steps |
|  [06]   | Process setting of one platform        | `mise.unix.toml` `[env]`, loaded by `.miserc.toml`   | The same, on that platform              |
|  [07]   | Value one target reads                 | Target's `env` option beside its command             | That command                            |
|  [08]   | Path one script or program computes    | Script or program beside its other paths             | Itself                                  |
|  [09]   | Output of a workflow step              | `env:` on the consuming step from the step outputs   | That step                               |

## [08]-[INFRASTRUCTURE]

Every resource outside the repository tree is a typed row a program reads by key, one program per application with a stack per environment, and the repository's own program for its settings and its secret store:

| [INDEX] | [OWNER]                  | [HOLDS]                                                                    |
| :-----: | :----------------------- | :------------------------------------------------------------------------- |
|  [01]   | `infra/`                 | Repository's own resources: settings, the secret store, tokens, CI secrets |
|  [02]   | `apps/<name>/<program>/` | Resources of one application, one stack per environment                    |
|  [03]   | Pipeline                 | Every image and binary, rebuilt from a pinned manifest on each run         |

The repository program declares the store project, its configs, and its tokens, and the repository settings, secrets, and variables:
- The store holds one project, a config per environment, and a branch config named `<environment>_<suffix>` for repository automation
- A runtime secret enters the branch config once, from stdin through `doppler secrets set`
- Service token rows name the config and the access level, and an Actions secret row holds a token's key under the name the workflows read
- The repository row holds the merge, branch, and feature settings, `protect` refuses its deletion, and `archiveOnDestroy` archives it
- Adopt a resource that exists through import in place of creating it, and the row declares the adoption as its resource option
- `up --import` adopts the project, environments, branch configs, and repository, and tokens and secrets are created with no import
- Read every credential through the default provider of its package from the environment alone, and the program passes no token
- The repository provider detects the owner from its token
- Actions variable rows read their values from the environment under the variable's own name through `Config` at the entry boundary
- Variables enter the config once, `doppler secrets set <NAME>`, and an unset name fails the run naming it
- Ruleset rows target the default branch (`~DEFAULT_BRANCH`) and the release tags (`refs/tags/*@*`), `excludes: []` required beside `includes`
- The branch ruleset requires the fan-in status check and names the admin role (`RepositoryRole`, `actorId` 5) as the bypass actor
- Environment rows hold their deployment branch policy in the same row, one policy naming the branch the jobs deploy from
- The Actions permissions row selects GitHub-owned actions and the explicit pattern list alone, the allow list in place of digest pins
- Vulnerability alerts, secret scanning, and push protection are rows, and a public repository takes no `advancedSecurity` block
- Every table is `as const satisfies` the provider's args type, and a setting under a disabled merge method leaves the row
- Import ids are `<repository>:<id>` for a ruleset, `<repository>:<environment>` for an environment, `<repository>:<name>` for a variable
- `@pulumiverse/doppler` 0.9.0 is two years behind upstream, with no GitHub integration, change-request policy, or rotated secret
- Trusted publishers on nuget.org, PyPI, and npm stay account settings, because neither the GitHub nor the Doppler provider models them

The program's dependencies sit in the root catalog and manifest, the root `tsconfig.json` includes its files for the root `typecheck` target, and the root `up` and `refresh` targets run the program's entry under `doppler run --project <project> --config <config>` with the summary of resource changes as the proof:
- The entry runs the stack through the Automation API on Pulumi Cloud with `PULUMI_ACCESS_TOKEN` from the environment and service secrets
- Plugins sit under `.cache/pulumi/`
- The infrastructure targets are `up` and `refresh`, and the automation entry holds the same subcommands
- The inline project has no `Pulumi.yaml`, and the `pulumi` CLI runs through the entry alone
- Preview, dry-run, plan, and intermediate targets join none of `package.json`, `infra/`, or `eng/`
- Each run prints the operation's output and the JSON of its resource changes, and a failed select or operation prints the diagnostic
- Take each provider and each provisioned runtime, image, and service at its newest release, and pin nothing outside the lockfile

Share nothing between application programs by position, and an application consumes another's output through a published package or a declared output. One store, the variable in the environment, and an error naming the unset names replace a second secret route copied from another repository.

## [09]-[CI]

CI runs the task graph through the runner as one job per language, the job of the language with native packages as a matrix over the runtime identifiers after the native workflow, and the pipeline file holds the commands alone:
- One job per language runs `nx affected` filtered by the language tag, in graph order
- The job of the language with source builds provisions the native inputs before its dependency sync
- Each matrix host stages its rid, and one job packs the collected trees
- Maintained actions perform each step one exists for, and a `run` step holds the rest
- One fan-in job over every job is the status check the branch ruleset requires, and a skipped or cancelled job fails it

## [10]-[PROOF]

Proof of a change is a run traced from the entry point to its last output, against a baseline recorded before the first edit:
- Run the target, and a cached target proves its inputs by a hit after an unrelated edit and a miss after a related one
- Run `nx graph --file=<path>` and `nx show projects --affected --files=<manifest>` after an edge changes
- Every edge in the `nx graph --file` output points from a consumer to a packaging project
- The affected listing for a manifest names the packaging project, its binding, and its consumers
- Run every checker to zero warnings after any change
- Compare `git diff | shasum` before and after a rewriting target, equal hashes prove it rewrote nothing
- Follow the change through every target, output, cache entry, and workflow step it touches, and read each output
- Fix a wrong output, a missing output, a leftover file, a process that outlives its run, or a step that passes with no effect at its cause
- Read `git log -p <file>` over each document or configuration a pass rebuilt before the change lands
- Restore each criterion, capability, command flag, and purpose statement an earlier revision stated and the rebuild dropped or loosened
- Delete disposable probe projects and their generated outputs after reading the result

## [11]-[ANTI_PATTERNS]

Smells and the form that replaces each:

| [INDEX] | [SMELL]                                                 | [CORRECT_FORM]                                                        |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `stage-<x>` and `pack-<x>` target pairs per library     | Local plugin infers `stage` and `pack` from each project file         |
|  [02]   | Committed `.nupkg`, `.dylib`, or `.so` files            | Manifest pin, staging target, and an ignored `.artifacts/` feed       |
|  [03]   | `cache: false` on a target with pure outputs            | `cache: true` with inputs, `dependentTasksOutputFiles`, exact outputs |
|  [04]   | Configurable outputs outside the shared roots           | `.artifacts/<area>/` outputs and `.cache/` caches                     |
|  [05]   | Target commands that run `nx run` or `pnpm nx`          | `dependsOn` in the object form naming the project and target          |
|  [06]   | README step lists for machine setup                     | Provisioning target documented in the root README                     |
|  [07]   | Machine paths in a script, project, or manifest         | Root lock file, `$(MSBuildThisFileDirectory)`, or `{workspaceRoot}`   |
|  [08]   | Preview, check, or dry-run variants of a target         | One target, `git diff --exit-code` in CI after the rewriting targets  |
|  [09]   | Configuration files per directory where an owner exists | Plugin inference, the manifest's `nx` field, or the root config       |
|  [10]   | `scripts` in a manifest beside the targets              | Target, the one entry the graph orders and caches                     |
|  [11]   | Repeated language build commands in a target            | Project filters, with shared operations in root targets               |
