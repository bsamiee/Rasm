---
name: monorepo-build-infrastructure
description: "Use when changing eng/, infra/, tools/, mise.toml, nx.json, or .github/, or orchestrating repository-wide tooling work: placement, targets, native chain, toolchain, environment, infrastructure, CI, proof."
---

# [MONOREPO_BUILD_INFRASTRUCTURE]

Covers the shared build infrastructure of a polyglot monorepo, from the toolchain and the `eng/` directory to the CI entry point.
1. Read `README.md`, `mise.toml`, and `.vscode/settings.json`
2. Run one `fd` call to list every MSBuild, TypeScript, and Python file:

```sh
fd -H '^(.*\.(csproj|slnx)|Directory\..*\.(props|targets)|NuGet\.config|\.editorconfig|global\.json|(nx|package|biome|project)\.json|pnpm-workspace\.yaml|vite(st)?\.config\.ts|tsconfig.*\.json|pyproject\.toml|uv\.lock)$'
```

3. Run `tree eng`, `tree tools`, `tree infra`, and `tree .github`, one call per directory

The listings locate the files, and a file is read when a change touches it.

[REFERENCES]: category information a change in that category reads:
- [01]-[DOTNET](references/dotnet.md): .NET build, packaging, and test configuration, and the discipline over every .NET file type
- [02]-[PYTHON](references/python.md): Python manifest, scripts, and checker configuration, and the discipline over every Python file type
- [03]-[TYPESCRIPT](references/typescript.md): TypeScript package, compiler, lint, and test tooling, and the discipline over every file type
- [04]-[TOOLING](references/tooling.md): Tool-general configuration: the environment, the toolchain, the task runner, the harness and editor
- [05]-[INFRASTRUCTURE](references/infrastructure.md): Infrastructure program in any language: resources, stacks, tokens, secret store rows
- [06]-[GITHUB](references/github.md): Everything under `.github/`: the workflows, the actions, and the local run

[AGENTS]: Each agent owns one category, reads its reference, decides from the documentation and the tool, and proves its result by a run:
- [01]-[DOTNET_MAINTAINER](../../agents/dotnet-maintainer.md): .NET build, restore, test, coverage, and packaging settings
- [02]-[PYTHON_MAINTAINER](../../agents/python-maintainer.md): Python dependency, checker, test, and coverage settings, and the scripts
- [03]-[TYPESCRIPT_MAINTAINER](../../agents/typescript-maintainer.md): TypeScript package, compiler, lint, test, and mutation settings, plugin code

## [01]-[DISPATCH]

Repository-wide work runs as one collective pass that ends aligned, with `main` as the orchestrator:
- Dispatch the maintainers, and a general-purpose agent over the tooling, infrastructure, and github categories, each fresh
- Each brief names the scope, the direction, the standards, the proof, the messaging rule, and the references the scope reads
- The standards are the language skills, `clean-prose`, and zero warnings from every checker
- Brief each agent to message `main` with a finding outside its scope, related or tangential, and a smell in any file
- Brief each agent to message an active agent directly with a change that agent adjusts to or integrates
- Act on each finding as it arrives: relay it to the agent that holds the scope, make the surgical change, or dispatch a focused agent
- An agent's list of peers holds the agents alive when it started, and a finding for an agent started later goes through `main`
- Read each changed file as it lands, and judge agents converging on one approach against their scopes, the shared approach is a finding
- Take a large proposal for a second opinion before it starts, and an approved proposal joins the scope of every agent it touches
- Hold no work back for a later pass, and defer, store, or hedge nothing
- After each agent returns, dispatch a fresh agent over the same scope as an adversarial pass from the current state, writing its changes
- The pass attacks each tool chosen, absent, replaceable, misplaced, weakly used, or hand-written where a maintained tool exists
- Where the scope holds program logic the pass attacks its quality and capability, and where it holds configuration, the full option set

Implement each improvement to the skill, a reference, or an agent file that a run or an agent identifies in place: delete, reframe, or correct.

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

Rename a coined name wherever it exists through the tool that updates every reference, and report a name another system resolves as a coupling.

Research settles a change before its first line is written, and each fact resolves at its source:
- Research precedes a new tool, action, package, method, or resource: newest release, documentation, full option set, maintained actions and plugins
- One fact resolves through `search-context7`, `search-tavily`, or the `github` MCP, and a subject that decides a design gets Opus gatherers
- Language and tool capabilities come first, a maintained plugin second, and custom code where neither performs the step
- Guards, retries, fallbacks, digest pins, release-age delays, and audit steps add a step with no decision behind it
- The exit code of the operation and `git diff --exit-code` are the checks

Decide every question in the session from the root `README.md`, the repository as it is, and the tool documentation, and rebuild an existing form when a documented capability, a package integration, or a configuration is objectively better, tooling replacement included. A rebuild for code quality, for the proper integration of a package, or for a capability the tool offers needs no new requirement, and a capability found in one scope reaches every agent it touches in the same session.

Each pass proposes what its scope lacks, judged against the capability the tool documents and the needs of a large polyglot monorepo of unrelated applications and libraries:
- A capability the tool offers and the repository leaves unused, read from its full option set and its plugin and extension catalog
- A workflow beyond build and release when a schedule or an event serves the repository, placed on the runner or the host that owns the trigger
- A file or directory the tool or the host reads that the repository lacks, declared through a program row where a provider owns it
- A plugin or rule of the repository's own when no maintained one performs the step and the capability spans tools
- A new kind under `eng/`, `tools/`, or `infra/` when a build input, a check, or a resource has no owner

Program logic under `infra/`, `tools/`, `eng/scripts/`, and the hooks meets the language standard and these criteria:
- Fewer types, schemas, and classes, the package's own types hold the data
- One module per operation, and repeated logic collapses into one function
- The package's capability performs the step (the Effect runtime, the Nx devkit, the standard library) in place of hand-written logic
- Every cache link is declared and every input is processed once, a second read or a second build of one input is a defect
- Behavior and performance are measured before and after under the same controls

Configuration in any format (JSON, YAML, TOML, MSBuild XML) takes the structure its schema documents, read in full before the rebuild:
- Repeated flat entries collapse into the grouping the format offers: a tag or glob filter, an override block, a conditioned group, a shared default
- A value equal to the tool's default leaves the file, and every option kept has a reason
- One entry states each fact, and the entries that vary by language, path, or role vary in one keyed block

## [03]-[PLACEMENT]

Entry points and guidance have one owner each:
- `nx.json` and the root manifest `nx` field are the one entry point, and every developer command is a target
- No entry point runs every language, and the project filter selects one
- Names in targets, files, and prose carry no repository prefix beyond what the ecosystem requires
- Command and tool guidance has one owner, and every other document states its purpose and points there in one line

Each kind of addition has one owner, chosen by what consumes it, and a directory grows by kind:

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
|  [12]   | Editor setting                       | `.vscode/settings.json`                           | Setting keyed by language                  |
|  [13]   | Cache, download, or checkout         | `.cache/<tool>/`                                  | Relocated through the tool's own setting   |
|  [14]   | Build output, package, or report     | `.artifacts/<area>/`                              | Declared target output                     |

Composition roots belong to `apps/<name>/`, library code to `libs/`, tool configuration to the root manifests, and every binary to the pipeline, which rebuilds it from a pinned manifest. Each language area holds the binding packages that consume `eng/` output through package references, and a new language area takes the same shape.

Each fact has one owning file, and every other file reads it from there:

| [INDEX] | [FACT]                     | [OWNER]                        | [READERS]                                 |
| :-----: | :------------------------- | :----------------------------- | :---------------------------------------- |
|  [01]   | Native library version     | `eng/native/<library>/*.json`  | Packaging project version check           |
|  [02]   | Package version            | Packaging project `Version`    | Target outputs, from the element          |
|  [03]   | Local feed path            | `NuGet.config` local source    | Pack target, from the source value        |
|  [04]   | Output and cache roots     | Root `Directory.Build.props`   | Target outputs                            |
|  [05]   | Script dependency versions | Root `pyproject.toml` and lock | Scripts under `uv run`                    |
|  [06]   | Runtime and binary set     | `mise.toml` `[tools]`          | mise on a machine, the setup action on CI |

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

The stage, pack, and consume chain as targets, and each script target runs `uv run --only-group eng python -m eng.scripts.<module>`:

| [INDEX] | [TARGET]            | [RUNS]                      | [DEPENDS_ON]                                     | [CACHE] |
| :-----: | :------------------ | :-------------------------- | :----------------------------------------------- | :-----: |
|  [01]   | `eng:provision`     | `eng.scripts.provision`     | Nothing                                          | `false` |
|  [02]   | `Native.Item:stage` | `eng.scripts.stage item`    | `{ projects: ["eng"], target: "provision" }`     | `false` |
|  [03]   | `Native.Item:pack`  | `dotnet pack`               | `stage`                                          | `true`  |
|  [04]   | `Item:pack`         | `dotnet pack`               | `{ projects: ["Native.Item"], target: "stage" }` | `true`  |
|  [05]   | `Consumer:build`    | `dotnet build`              | `^build`, `<root>:restore`                       | `true`  |

Cache a target when its outputs are a function of its declared inputs alone:

| [INDEX] | [TARGET]    | [INPUTS]                                                        | [OUTPUTS]                               | [VERDICT] |
| :-----: | :---------- | :-------------------------------------------------------------- | :-------------------------------------- | :-------: |
|  [01]   | `provision` | Network, host toolchain                                         | `.cache/<tool>/`                        | No cache  |
|  [02]   | `stage`     | Network, vcpkg toolchain, host compiler                         | `.artifacts/native/<library>/stage`     | No cache  |
|  [03]   | `pack`      | Project dir, `Directory.Build.*`, manifest dir, staged tree     | `<feed>/<Id>.<Version>.nupkg`, bin, obj |   Cache   |
|  [04]   | `build`     | Sources, `Directory.Build.*`, `.editorconfig`, `^build` outputs | `ArtifactsPath` bin and obj per project |   Cache   |

The `release` field of `nx.json` versions every project from its git tag, and one dispatch workflow runs `nx release --skip-publish` then `nx release publish`, because a push of many tags raises no event.

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

`mise.toml` holds the machine setup, the toolchain at `latest` with the language lock files as the only pins, the resolution settings, the process environment, and each further mise capability that serves the setup, while tasks and entry points stay with Nx:

| [INDEX] | [KIND]                                                 | [OWNER]                               | [EXAMPLES]                             |
| :-----: | :----------------------------------------------------- | :------------------------------------ | :------------------------------------- |
|  [01]   | Binary a target, script, workflow, or agent shell runs | `[tools]` at `latest`                 | node, pnpm, uv, act, doppler, buf      |
|  [02]   | Package code, a config, a plugin, or an editor reads   | Package manager and its lock          | `@biomejs/biome`, `typescript`, `ruff` |
|  [03]   | .NET tool package a target runs                        | `dotnet dnx <tool>` on the command    | dotnet-stryker, dotnet-outdated-tool   |
|  [04]   | Host SDK a plugin host binds to one version            | `global.json`, `rollForward` disabled | .NET SDK                               |
|  [05]   | Machine tooling the workspace never invokes            | Machine profile                       | `gh`, `jq`, `git`, `docker`            |

## [07]-[ENVIRONMENT]

Every value a process reads has one owner, chosen by who reads it and whether it is secret:

| [INDEX] | [VALUE]                                | [OWNER]                                              | [READER]                                |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | Secret                                 | Secret store config of the owning environment        | `doppler run` around the target         |
|  [02]   | Secret a workflow consumes             | Service token and Actions secret rows in the program | `secrets.<NAME>` on the step            |
|  [03]   | Value a workflow consumes, not secret  | Actions variable row in the program                  | `vars.<NAME>` on the step               |
|  [04]   | Tool setting with a manifest field     | Manifest the tool reads by directory walk            | Tool                                    |
|  [05]   | Process setting with no manifest field | `mise.toml` `[env]`                                  | Targets, scripts, agent shell, CI steps |
|  [06]   | Value one target reads                 | Target's `env` option beside its command             | That command                            |
|  [07]   | Path one script or program computes    | Script or program beside its other paths             | Itself                                  |
|  [08]   | Output of a workflow step              | `env:` on the consuming step from the step outputs   | That step                               |

## [08]-[INFRASTRUCTURE]

Every resource outside the repository tree is a typed row in a program, and the program models the monorepo as it is, many unrelated applications and libraries with their own lifecycles:

| [INDEX] | [OWNER]                  | [HOLDS]                                                                        |
| :-----: | :----------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `infra/`                 | Repository's own resources: settings, the secret store, tokens, CI secrets     |
|  [02]   | `apps/<name>/<program>/` | Resources of one application, one stack per environment                        |
|  [03]   | Pipeline                 | Every image and binary, rebuilt from a pinned manifest on each run             |

## [09]-[CI]

CI runs the task graph through the runner as one job per language, the job of the language with native packages as a matrix over the runtime identifiers that follows the native workflow, and the pipeline file holds the commands alone:
- One job per language runs `nx affected -t <targets>` filtered by the language tag, in graph order
- `git diff --exit-code` follows the rewriting targets
- Coverage merges per language
- Each matrix host stages its rid, and one job packs the collected trees
- Maintained actions perform each step one exists for, and a `run` step holds the rest

## [10]-[PROOF]

Proof of a change is a run traced from the entry point to its last output:
- Run the target, and a cached target proves its inputs by a hit after an unrelated edit and a miss after a related one
- Run `nx graph --file=<path>` and `nx show projects --affected --files=<manifest>` after an edge changes
- Every edge in the `nx graph --file` output points from a consumer to a packaging project
- The affected listing for a manifest names the packaging project, its binding, and its consumers
- Run every checker to zero warnings after any change
- Follow the change through every target, output, cache entry, and workflow step it touches, and read each output
- Fix a wrong output, a missing output, a leftover file, a process that outlives its run, or a step that passes with no effect at its cause
- Read `git log -p <file>` over each document or configuration a pass rebuilt, the adversarial pass included, before the change lands
- Restore each criterion, capability, command flag, and purpose statement an earlier revision stated and the rebuild dropped or loosened

## [11]-[ANTI_PATTERNS]

Smells and the form that replaces each:

| [INDEX] | [SMELL]                                                       | [CORRECT_FORM]                                                          |
| :-----: | :------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | `stage-<x>` and `pack-<x>` target pairs per library           | Local plugin infers `stage` and `pack` from each project file           |
|  [02]   | Committed `.nupkg`, `.dylib`, or `.so` files                  | Manifest pin, staging target, and an ignored `.artifacts/` feed         |
|  [03]   | `cache: false` on a target with pure outputs                  | `cache: true` with inputs, `dependentTasksOutputFiles`, exact outputs   |
|  [04]   | Outputs under a project directory or `dist/`                  | Every output under `.artifacts/<area>/`, every cache under `.cache/`    |
|  [05]   | Target commands that run `nx run` or `pnpm nx`                | `dependsOn` in the object form naming the project and target            |
|  [06]   | README step lists for machine setup                           | One provisioning target, and the README points at the skill             |
|  [07]   | Machine paths in a script, project, or manifest               | Root lock file, `$(MSBuildThisFileDirectory)`, or `{workspaceRoot}`     |
|  [08]   | Preview, check, or dry-run variants of a target               | One target, `git diff --exit-code` in CI after the rewriting targets    |
|  [09]   | Configuration files per directory where an owner exists       | Plugin inference, the manifest's `nx` field, or the root config         |
|  [10]   | `scripts` in a manifest beside the targets                    | Target, the one entry the graph orders and caches                       |
|  [11]   | Aggregate target that runs every language                     | Project filter on the runner's own command                              |
