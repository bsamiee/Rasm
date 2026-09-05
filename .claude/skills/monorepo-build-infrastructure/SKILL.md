---
name: monorepo-build-infrastructure
description: "Use when changing eng/, infra/, tools/, mise.toml, nx.json, or .github/, or orchestrating repository-wide tooling work: placement, task graph, native packaging, environment, infrastructure, CI, corrections."
---

# [MONOREPO_BUILD_INFRASTRUCTURE]

Covers the shared build infrastructure of a polyglot monorepo, from the toolchain and the `eng/` directory to the CI entry point.
1. Read `README.md`, `mise.toml`, and `.vscode/settings.json`
2. Run one `fd` call to list every MSBuild, TypeScript, and Python file:

```sh
fd -H '^(.*\.(csproj|slnx)|Directory\..*\.(props|targets)|NuGet\.config|\.editorconfig|global\.json|(nx|package|biome|project)\.json|pnpm-workspace\.yaml|vite(st)?\.config\.ts|tsconfig.*\.json|pyproject\.toml|uv\.lock)$'
```

3. Run `tree eng` → `tree tools` → `tree infra` → `tree .github`, one call per directory

The listings locate the files, and a file is read when a change touches it. Every listed file is in scope, and a change to one file updates every file that reads or supplies its facts.

[AGENTS]: Each agent owns one scope, decides from the documentation and the tool itself, and proves its result by a run:
- [01]-[DOTNET_MAINTAINER](../../agents/dotnet-maintainer.md): .NET build, restore, test, coverage, and packaging settings
- [02]-[PYTHON_MAINTAINER](../../agents/python-maintainer.md): Python dependency groups, uv, checker, pytest, and coverage settings, and the scripts
- [03]-[TYPESCRIPT_MAINTAINER](../../agents/typescript-maintainer.md): Nx, pnpm, Biome, tsc, and Vitest settings, and the infrastructure program
- [04]-[DOMAIN_RESEARCHER](../../agents/domain-researcher.md): Domain of a skill, an agent file, a tool, or an integration gathered into its archive

For a fresh session, a request to orchestrate, or repository-wide work, dispatch the three language maintainers and a general-purpose agent over `mise.toml`, `.github/`, the editor and agent harness settings, the git attributes, and every tool outside one language. Each brief names the scope, the direction, the standards (the language skills, `clean-prose`, zero warnings from every checker), the proof, and the messaging rule. That agent reads the sections that follow for understanding, not as its procedure, and delegates as it needs. Every brief carries the messaging rule: relay each finding to the agent with the scope it touches as the finding arrives, and each agent messages that agent directly, because an improvement in one scope opens one in another, a change in one scope needs alignment in another, and an agent reading its own files finds facts another scope needs. No scope boundary holds a finding back.

As each agent returns, dispatch a fresh agent over the same full scope as an adversarial cold pass. The pass takes the current state as the base and attacks every decision and approach in it: a tool chosen or absent, a tool used outside its place, a hand-written step where a maintained tool exists, a weak use of a good tool, and a tool with a better replacement. Where the scope holds program logic (`infra/`, `tools/`, `.github/`, the automation scripts), the pass attacks code quality and capability as well as selection and integration, and where the scope is configuration (`mise.toml`), the pass reads the full option set for what is overlooked or underused. The pass writes its changes like the agent before it.

Act as the bus and the orchestrator between the agents, handle surgical work directly, and never hold work for a collision or a block between agents, because the work is one collective pass that ends aligned. Every smell found anywhere in the session, a mistake in a code file included, rolls into the scope, because it bears on the health of the repository, and a focused agent takes it in the same session.

## [01]-[APPROACH]

Every infrastructure change is decided from the root `README.md` and the current documentation of the tool, applied to every file that holds a part of it, and proven by a run in the session that raised it:

| [INDEX] | [PRINCIPLE]    | [CRITERION]                                                                                                    |
| :-----: | :------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | Currency       | Each tool, action, package, and method is its newest release in the form its current documentation states      |
|  [02]   | Adoption       | A maintained action, plugin, or tool that performs a step is adopted, and custom code extends it               |
|  [03]   | Cohesion       | A changed fact reaches every reader in one change: manifest, lock, target, inputs, cache key, workflow, editor |
|  [04]   | Directness     | Lockfiles pin, action tags name a major, and an operation runs once with its own output as the check           |
|  [05]   | Completion     | Every question resolves against the root `README.md` and the tool documentation, and the change lands proven   |
|  [06]   | Portability    | One definition of every file runs on every operating system, and the host is detected once as a parameter      |
|  [07]   | Infrastructure | Every resource, environment, and image is a program row, and each application and library is its own unit      |
|  [08]   | Terminology    | Every name is the established term of its tool, of CI/CD, or of software engineering, wherever the name exists |
|  [09]   | Renewal        | An existing approach is rebuilt in place when a documented capability or integration is objectively better     |

Research precedes a new tool, action, package, method, or provisioned resource: its newest release, its current documentation, its full option set, and the maintained actions and plugins around it, settled before the first line is written. One fact resolves through `search-context7`, `search-tavily`, or the `github` MCP, and a subject with capabilities that decide the design goes to `domain-researcher`, which gathers the documentation, the repository, the field use, and its own binary probes into the archive of the subject's owner, and a change to a subject with an archive starts from its findings files. Language and tool capabilities come first, a maintained plugin or extension second, and custom code exists where neither performs the step. Guards, retries, fallbacks, digest pins, release-age delays, and audit steps add a step with no decision behind it, and the operation's exit code and `git diff --exit-code` are the checks.

Decide every question in the session from the root `README.md`, the memory notes, the repository as it is, and the tool documentation, and rebuild an existing form when a documented capability, a package integration, or a configuration is objectively better, tooling replacement included. A rebuild for code quality, for the proper integration of a package, or for a capability the tool offers needs no new requirement, and a capability found in one scope reaches every agent it touches in the same session, so the change lands everywhere at once. Rename a coined or invented name wherever it exists, in files, directories, configuration keys and paths, targets, functions, identifiers, comments, and the messages code emits, through the tool that updates every reference, and report a name another system resolves as a coupling.

Each pass proposes what its scope lacks, judged against the capability the tool documents and the needs of a large polyglot monorepo of unrelated applications and libraries:
- A capability the tool offers and the repository leaves unused, read from its full option set and its plugin and extension catalog
- A workflow beyond build and release when a schedule or an event serves the repository, placed on the runner or the host that owns the trigger
- A file or directory the tool or the host reads that the repository lacks, declared through a program row where a provider owns it
- A plugin or rule of the repository's own when no maintained one performs the step and the capability spans tools
- A new kind under `eng/`, `tools/`, or `infra/` when a build input, a check, or a resource has no owner

A large proposal goes to `main` for a second opinion before the work starts, and an approved proposal joins the scope of every agent it touches.

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

Proof of a change is a run traced from the entry point to its last output:
- Run the target, and a cached target proves its inputs by a hit after an unrelated edit and a miss after a related one
- Run `nx graph --file=<path>` and `nx show projects --affected --files=<file>` after an edge changes
- Run the root `lint` target for `actionlint` and the root `workflow` target for the Linux jobs under act after a workflow or action changes
- Run `mise env` after a `mise.toml` change, and every checker to zero warnings after any change
- Follow the change through every target, output, cache entry, and workflow step it touches, and read each output
- Fix a wrong output, a missing output, a leftover file, a process that outlives its run, or a step that passes with no effect at its cause
- Read `git log -p <file>` over each document or configuration a pass rebuilt, the adversarial cold pass included, before the change lands
- Restore each criterion, capability, command flag, and purpose statement an earlier revision stated and the rebuild dropped or loosened

Guidance placement:
- `nx.json` and the root manifest `nx` field are the one entry point, and every developer command is a target
- `mise.toml` holds the toolchain, its settings, the process environment, and each mise capability of the machine setup, and Nx owns every task
- No entry point runs every language, and the project filter selects one
- Names in targets, files, and prose carry no repository prefix beyond what the ecosystem requires
- Command and tool guidance has one owner, and every other document states its purpose and points there in one line

## [02]-[PLACEMENT]

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
|  [08]   | Package a config or editor reads     | The language manager catalog and lock             | Catalog entry, `upgrade` moves it          |
|  [09]   | Repository, service, or environment  | `infra/`, or the application's own program        | Typed row, imported when it exists         |
|  [10]   | Image or runner definition           | A program row that builds it from a manifest      | No image, snapshot, or exported state file |
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

## [03]-[TASK_GRAPH]

Nx infers targets from the files a plugin globs, and a `project.json` exists only where no plugin recognizes a file. The .NET plugin globs every project file and every ancestor `Directory.Build.*` and `Directory.Packages.props`:

| [INDEX] | [PLUGIN_FACT]                            | [CONSEQUENCE]                                                                      |
| :-----: | :--------------------------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `build` runs `dotnet build --no-restore` | `build` depends on the root `restore` target, the one restore of the solution      |
|  [02]   | `build` depends on `^build`              | `ProjectReference` edges order builds, `--no-dependencies` skips referenced ones   |
|  [03]   | Outputs derive from `ArtifactsPath`      | One `ArtifactsPath` under `.artifacts/` makes every output cacheable               |
|  [04]   | `pack` outputs the `PackageOutputPath`   | One output over a shared feed caches every sibling, `pack: false` replaces it      |
|  [05]   | `exclude` is a registration property     | Packaging subtrees leave the inferred graph without `project.json`                 |
|  [06]   | Directory files are per-target inputs    | Dotnet `build`, `test`, and `format` entries name `global.json` and `NuGet.config` |

`targetDefaults` fill a target a project declares and create none, a root target exists when the root `package.json` `nx` field declares it, and the local plugin emits the empty `lint`, `format`, `typecheck`, and `check` targets the tag-filtered defaults fill:
- Exclude the root project (`!<root>`) from a filtered entry when the root target lists `commands`, because a default `command` replaces the list
- `"..."` in a filtered entry's `inputs` spreads the inputs the plugin inferred
- `sharedGlobals` names `nx.json`, `mise.toml`, and `tools/nx/*.ts`, and a plugin edit marks every project affected
- Language named inputs hold `{ "runtime": "<command> --version" }` when the runtime comes from the toolchain at `latest`
- Touched files matching a project's target inputs mark it affected, a `workspace:` dependency makes a project edge, `catalog:` makes none
- Every declared output exists after a run, because Nx drops a missing output silently and still reports a hit
- The `typecheck` output is the `outDir` under `.cache/typescript/out/<root>`, which holds the build info beside the declarations
- Vitest `test` targets hold a `benchmark` configuration running `vitest bench`, the one producer of `bench/<project>.json`
- `pluginsConfig["@nx/js"].analyzeSourceFiles: false` keeps `package.json` edges, the native `typescript` binary lacks the analyzer's compiler API

Register one local plugin under `tools/` by path for a target shape that repeats per directory, it globs the per-directory file and returns the targets:

```json
{
    "plugins": [
        { "plugin": "@nx/dotnet", "exclude": ["eng/native/**"], "options": { "pack": false, "restore": false } },
        { "plugin": "./tools/nx/<plugin>.ts" }
    ]
}
```

```ts
export const createNodes: CreateNodes = [
    "{**/*.csproj,{apps,libs,tests}/**/tsconfig.json,{libs/python,apps/*}/*/__init__.py}",
    async (projectFiles, _options, context) =>
        projectFiles.map((file) => [file, { projects: { [dirname(file)]: projectFor(context.workspaceRoot, file) } }]),
];
```

Export `createDependencies` beside `createNodes`, and each `PackageReference` to a packaging project then becomes a static edge that marks the consumer affected. Python packages carry no manifest, `__init__.py` one level under `libs/python/` or an application marks the project, the plugin names it by the last segment of its root, and the publish script builds it from a generated manifest.

Nx runs each plugin in an isolated worker and loads a `.ts` plugin and the release version actions through Node type stripping, the swc loader fails under the native `typescript` compiler, and `NX_PREFER_NODE_STRIP_TYPES` stays unset. CommonJS default imports in a loaded module arrive as the module object under the native loader and as the class under swc, and the interop expression in `tools/nx/typescript-version-actions.ts` handles both. The root `tsconfig.json` includes the plugin files for `tsc --build` and the linter.

The stage, pack, and consume chain as targets, with the script targets under the dependency group of the scripts alone:

| [INDEX] | [TARGET]            | [COMMAND]                                            | [DEPENDS_ON]                                     | [CACHE] |
| :-----: | :------------------ | :--------------------------------------------------- | :----------------------------------------------- | :-----: |
|  [01]   | `eng:provision`     | `uv run python -m eng.scripts.provision`             | Nothing                                          | `false` |
|  [02]   | `Native.Item:stage` | `uv run python -m eng.scripts.stage item`            | `{ projects: ["eng"], target: "provision" }`     | `false` |
|  [03]   | `Native.Item:pack`  | `dotnet pack eng/native/Native.Item --output <feed>` | `stage`                                          | `true`  |
|  [04]   | `Item:pack`         | `dotnet pack eng/native/Item --output <feed>`        | `{ projects: ["Native.Item"], target: "stage" }` | `true`  |
|  [05]   | `Consumer:build`    | `dotnet build --no-restore --no-dependencies`        | `^build`, `<root>:restore`                       | `true`  |

Root project targets, declared in the root `package.json` `nx` field:

| [INDEX] | [TARGET]  | [PURPOSE]                                                                                          | [CACHE] |
| :-----: | :-------- | :------------------------------------------------------------------------------------------------- | :-----: |
|  [01]   | `restore` | `dotnet restore <solution>`, the one restore the .NET `build`, `format`, and publish defaults need | `true`  |
|  [02]   | `upgrade` | `uv lock --upgrade`, `pnpm update --latest --recursive`, and dotnet-outdated under `dotnet dnx`    | `false` |

`upgrade` moves every language's dependency set to its newest release, prereleases included, and runs with `parallelism: false` because every command writes a shared file:
- Use `dotnet-msbuild-packaging` for the dotnet-outdated command line

Cache a target when its outputs are a function of its declared inputs alone:

| [INDEX] | [TARGET]    | [INPUTS]                                                        | [OUTPUTS]                               | [VERDICT] |
| :-----: | :---------- | :-------------------------------------------------------------- | :-------------------------------------- | :-------: |
|  [01]   | `provision` | Network, host toolchain                                         | `.cache/<tool>/`                        | No cache  |
|  [02]   | `stage`     | Network, vcpkg toolchain, host compiler                         | `.artifacts/native/<library>/stage`     | No cache  |
|  [03]   | `pack`      | Project dir, `Directory.Build.*`, manifest dir, staged tree     | `<feed>/<Id>.<Version>.nupkg`, bin, obj |   Cache   |
|  [04]   | `build`     | Sources, `Directory.Build.*`, `.editorconfig`, `^build` outputs | `ArtifactsPath` bin and obj per project |   Cache   |

Declare the staged tree through `dependentTasksOutputFiles`, Nx hashes the `stage` outputs from disk even for an uncached dependency, and the ignored `.artifacts/` tree stays out of the workspace file map:

```json
{
    "pack": {
        "command": "dotnet pack eng/native/Native.Item --configuration Release --output .artifacts/nuget --nologo",
        "cache": true,
        "dependsOn": ["stage"],
        "inputs": ["{projectRoot}/**/*", "{workspaceRoot}/eng/native/item/**/*", { "dependentTasksOutputFiles": "**/*" }],
        "outputs": ["{workspaceRoot}/.artifacts/nuget/Native.Item.1.2.3.nupkg"]
    }
}
```

Target rules:
- Set `parallelism: false` on `stage` targets, vcpkg instances share one root and one download directory
- Name the exact package file as the output, a cache restore rewrites each declared output and a glob over a shared feed writes stale siblings back
- Declare a tool version or environment variable a cached target reads as a `{ "runtime": "<command>" }` or `{ "env": "<NAME>" }` input
- Write a cross-project dependency in the object form, `{ "projects": ["<project>"], "target": "<target>" }`
- Extra arguments forward to the command, `nx run Native.Item:stage --rid linux-x64` reaches the script as `--rid=linux-x64`
- The runner reads the exit code alone, and a script reports failure by exiting nonzero
- Local feeds need no `nx release` configuration

The `release` field of `nx.json` versions every project from its git tag:
- First releases need no `--first-release`: version actions answer `0.0.0`, `fallbackCurrentVersionResolver: "disk"`, `automaticFromRef: true`
- `releaseTag.pattern` takes `{projectName}`, `{version}`, and `{releaseGroupName}`, and a fixed group takes one tag per group
- `createRelease` needs `git.push: true`
- Projects with no manifest take a `VersionActions` subclass with `validManifestFilenames` null, `tools/nx/version-actions.ts`, as `versionActions`
- The typescript group's `versionActions` is `tools/nx/typescript-version-actions.ts`, the JS actions with a `0.0.0` fallback
- `manifestRootsToUpdate` of the typescript group names the built manifest under `dist` alone
- One dispatch workflow runs `nx release --skip-publish` then `nx release publish`, because a push of many tags raises no event
- The local plugin emits the publish target for languages with no public manifest, and native packages push with `--skip-duplicate`

`nx affected` is correct when every edge exists in the graph: `ProjectReference` edges from the .NET plugin, `PackageReference` edges to packaging projects from the local plugin, and `implicitDependencies` from a managed binding to its native package. `nx graph --file=<path>` writes the graph, and `nx show projects --affected --files=<manifest>` proves an edge by listing the packaging project, its binding, and its consumers.

## [04]-[NATIVE_PACKAGING]

Each native library has one manifest directory as its single version pin, and the packaging project checks its `Version` against that pin before `GenerateNuspec`:

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

Staging rules:
- Stage every library from one script module, a lookup table maps each library to its staging function, and the shared operations exist once
- Run vcpkg with `--x-manifest-root` and `--x-install-root` under `.artifacts/` and `VCPKG_DEFAULT_BINARY_CACHE` under `.cache/`
- Pin one `builtin-baseline` in every `vcpkg.json`, provisioning fails on a second one, and staging checks the port version against `version-string`
- On macOS, rewrite every install name in a shared library closure to `@loader_path` and sign it ad hoc, and the set loads from its own directory
- Take the runtime identifier as an argument with the host as default, each CI host stages its own rid, and one job packs the collected trees
- Key the output of a long compile by commit under `.cache/`, and a repeat run copies it

The asset-only package holds `runtimes/`, `contentFiles/`, and a `lib/<tfm>/_._` placeholder with `IncludeBuildOutput` false, and a pinned `DeterministicTimestamp` makes the package bytes a function of content and version:
- Use `dotnet-msbuild-packaging` for the package layout

Give a library with a generated binding a managed packaging project, `Item` beside `Native.Item`, that compiles `stage/managed/*.cs` with `IncludeBuildOutput` true, shares the manifest version, and depends on the native `stage` target. Separate package ids keep the native assets loadable from any binding and the binding free of platform-specific content.

The local feed is a folder source in `NuGet.config` under `.artifacts/`, and package source mapping pins every workspace id to that source and every other id to the registry:
- Use `dotnet-msbuild-packaging` for the source list and its mapping

Reference the native package beside the binding, and an `Error` task in the root `Directory.Build.targets` fails a project that references one without the other, because the binding package holds no native asset.

## [05]-[PROVISIONING]

Every `stage` target depends on `eng:provision`, and the toolchain install precedes it on a fresh clone:

| [INDEX] | [TOOL]         | [MANIFEST]                               | [PLACEMENT]                  | [IDEMPOTENCE]                              |
| :-----: | :------------- | :--------------------------------------- | :--------------------------- | :----------------------------------------- |
|  [01]   | Python scripts | `pyproject.toml` and `uv.lock`           | `.venv/`                     | `uv run` syncs before every invocation     |
|  [02]   | vcpkg          | `builtin-baseline` in every `vcpkg.json` | `.cache/vcpkg/` and archives | Fetch and checkout only on a HEAD mismatch |
|  [03]   | Host tools     | Port name in the script                  | `.cache/<tool>-hosttools/`   | Skip when the executable exists            |
|  [04]   | Release files  | Manifest digest per rid                  | `.cache/<name>/<version>/`   | Skip when the digest-verified file exists  |

Provisioning rules:
- Verify a pinned digest on every download, unlink the file on a mismatch, and give a partial download a temporary name
- Pin every checkout to a commit, fetch with depth one, and update HEAD only when it differs
- Find the repository root as the nearest ancestor directory holding the root lock file
- Take every tool a package manager can pin from the manager, and download the rest
- `uv run` syncs the groups `default-groups` lists, `all` syncs every root group, and `--only-group <group>` on a target command syncs one group
- Set `cache: false` and `parallelism: false` on the target, because provisioning mutates shared directories

## [06]-[TOOLCHAIN]

`mise.toml` owns the machine setup, the toolchain at `latest` with the language lock files as the only pins, the resolution settings, the process environment, and each further mise capability that serves the setup, while tasks and entry points stay with Nx:

| [INDEX] | [KIND]                                                 | [OWNER]                               | [EXAMPLES]                             |
| :-----: | :----------------------------------------------------- | :------------------------------------ | :------------------------------------- |
|  [01]   | Binary a target, script, workflow, or agent shell runs | `[tools]` at `latest`                 | node, pnpm, uv, act, doppler, buf      |
|  [02]   | Package code, a config, a plugin, or an editor reads   | The package manager and its lock      | `@biomejs/biome`, `typescript`, `ruff` |
|  [03]   | .NET tool package a target runs                        | `dotnet dnx <tool>` on the command    | dotnet-stryker, dotnet-outdated-tool   |
|  [04]   | Host SDK a plugin host binds to one version            | `global.json`, `rollForward` disabled | The .NET SDK                           |
|  [05]   | Machine tooling the workspace never invokes            | The machine profile                   | `gh`, `jq`, `git`, `docker`            |

Toolchain rules:
- `[settings]` holds `prereleases = true` and `minimum_release_age = "0s"`, because mise delays a new release 24h by default
- `idiomatic_version_file_enable_tools = ["dotnet"]` reads the SDK version from `global.json`, and no `[tools]` row names it
- Interpreters with no stable release keep an exact pin, `latest` and a major.minor prefix resolve the `-dev` build under prereleases
- `_.path = "./node_modules/.bin"` and `python.uv_venv_auto = "source"` put the pnpm and uv lock copies on `PATH`
- `NX_WORKSPACE_DATA_DIRECTORY` relocates the Nx graph database under `.cache/nx/`, `nx.json` has no field for it

## [07]-[ENVIRONMENT]

Every value a process reads has one owner, chosen by who reads it and whether it is secret:

| [INDEX] | [VALUE]                                | [OWNER]                                              | [READER]                                |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :-------------------------------------- |
|  [01]   | Secret                                 | Secret store config of the owning environment        | `doppler run` around the target         |
|  [02]   | Secret a workflow consumes             | Service token and Actions secret rows in the program | `secrets.<NAME>` on the step            |
|  [03]   | Value a workflow consumes, not secret  | Actions variable row in the program                  | `vars.<NAME>` on the step               |
|  [04]   | Tool setting with a manifest field     | The manifest the tool reads by directory walk        | The tool                                |
|  [05]   | Process setting with no manifest field | `mise.toml` `[env]`                                  | Targets, scripts, agent shell, CI steps |
|  [06]   | Value one target reads                 | The target's `env` option beside its command         | That command                            |
|  [07]   | Path one script or program computes    | The script or program beside its other paths         | Itself                                  |
|  [08]   | Output of a workflow step              | `env:` on the consuming step from the step outputs   | That step                               |

Environment rules:
- `[env]` reaches targets through the shell hook or the shims, the agent shell through the session hook, and CI steps through the setup action
- The secret store holds one project, a config per environment, and a branch config for repository automation, set from stdin
- `doppler run --project <project> --config <config> -- <command>` injects the config into the process without a shell on every operating system
- Nothing the store injects is masked in workflow logs, and a secret reaches a log through no step
- Registries take trusted publishing with `id-token: write` on the job, keyed on the workflow file name, and no registry token is stored

## [08]-[INFRASTRUCTURE]

Every resource outside the repository tree is a typed row in a program, and the program models the monorepo as it is, many unrelated applications and libraries with their own lifecycles:

| [INDEX] | [OWNER]                  | [HOLDS]                                                                        |
| :-----: | :----------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `infra/`                 | The repository's own resources: settings, the secret store, tokens, CI secrets |
|  [02]   | `apps/<name>/<program>/` | The resources of one application, one stack per environment                    |
|  [03]   | The pipeline             | Every image and binary, rebuilt from a pinned manifest on each run             |

Infrastructure rules:
- Declare the store project, configs, and tokens and the repository settings, secrets, and variables as typed rows the program reads by key
- Adopt a resource that exists through import in place of creating it, and the row declares the adoption
- Read every credential through the default provider of its package from the environment alone, and the program passes no token
- `up` applies the rows and `refresh` reads the live state back, and the summary of resource changes is the proof
- Take each provider and each provisioned runtime, image, and service at its newest release, and pin nothing outside the lockfile
- Share nothing between application programs by position, an application consumes another's output through a published package or a declared output
- Use `pulumi` for the program mechanics: resources, adoption through import, state backends, and destroys
- Use `secrets` for where a secret belongs and how a token reaches a process

## [09]-[ISOLATION]

Take a packaging subtree out of the root `Directory.Build.props` chain when the root enforces rules the packaging projects break: `Version` in the project file, `IsPackable` false, analyzer references under central package management, and an `ArtifactsPath` for the language area:

| [INDEX] | [FORM] | [FILE]                                   | [WHEN]                                                  |
| :-----: | :----- | :--------------------------------------- | :------------------------------------------------------ |
|  [01]   | Chain  | `Import` through `GetPathOfFileAbove`    | The subtree needs the root defaults and overrides a few |
|  [02]   | Stop   | `Directory.Build.props` with no `Import` | The subtree breaks root rules and shares no default     |

```xml
<!-- Chain: the root file evaluates first and the subtree overrides after it -->
<Project>
    <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />
    <PropertyGroup>
        <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '..', '.artifacts', 'packaging'))</ArtifactsPath>
    </PropertyGroup>
</Project>
```

Pair the stop form with a minimal `Directory.Build.targets` and a `Directory.Packages.props` that sets `ManagePackageVersionsCentrally` to `false`, because MSBuild finds every directory file in the subtree and searches no further up. `dotnet msbuild <project> -getProperty:<RootProperty>` proves the choice: the chain returns the root value and the stop returns empty.

Packaging projects stay out of the solution file and out of the inferred graph:
- The solution lists the projects a developer builds and tests, and a packaging project builds through its own target
- The .NET plugin registration excludes the subtree, and the local plugin infers it with its own target names
- Consumers reach a package through the feed alone

Checks that keep the graph acyclic:
- The root `Directory.Build.targets` gives each project role an allowed set of reference roles and fails a reference outside it
- Every edge in the `nx graph --file` output points from a consumer to a packaging project
- The staging script imports the provisioning module, and the provisioning module imports no script

## [10]-[CI_ENTRY]

CI runs the task graph through the runner, one job per language after the native workflow, and the pipeline file holds the commands alone:

| [INDEX] | [STEP]                                                     | [REASON]                                                                 |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `actions/checkout` with `fetch-depth: 0`                   | Full history for the base commit and the tag readers                     |
|  [02]   | `nrwl/nx-set-shas` in each language job                    | Exports `NX_BASE` and `NX_HEAD` for `nx affected`, no graph job exists   |
|  [03]   | The setup action                                           | Toolchain through mise, package folders restored, dependencies installed |
|  [04]   | `nx affected -t <targets> --exclude='*,!tag:language:<x>'` | One command runs the affected targets of one language in graph order     |
|  [05]   | `git diff --exit-code` after the rewriting targets         | Fails the job on a rewrite `lint` or `format` made                       |
|  [06]   | `nx run <root>:coverage --language <x>`                    | Merges one language's coverage, `--excludeTaskDependencies` skips tests  |
|  [07]   | `nx run-many -t stage` per rid, `nx run-many -t pack` once | Each matrix host stages its rid, one job packs the collected trees       |
|  [08]   | `CI=true` in the environment                               | Nx computes the graph in process, no daemon, `NX_DAEMON` overrides       |

The setup action caches each package folder keyed on the files that decide its contents:

| [INDEX] | [FOLDER]                     | [KEY]                                                                                             |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | NuGet global packages folder | `Directory.Packages.props`, `NuGet.config`, the `Directory.Build.*` files, and every project file |
|  [02]   | uv cache                     | `uv.lock`                                                                                         |
|  [03]   | pnpm store and cache         | `pnpm-lock.yaml`                                                                                  |

CI rules:
- `defaultBase` in `nx.json` names the base branch, and a pull request compares against it
- `neverConnectToCloud: true` keeps the cache local, and `cacheDirectory` sits under `.cache/`
- mise-action runs with `cache: false`, its cache key hashes the config files alone and `mise install` treats `latest` as installed
- mise-action exports the `[env]` values and `_.path` to later steps
- pnpm's `ci` setting turns frozen mode on under CI, and `pnpm install` fails on lock drift
- `uv sync --locked --only-group <group>` syncs the group a job needs and fails on lock drift
- The native cache action splits restore from save, the save steps run under `always()` with a non-empty key, and a failed stage saves what it built
- `nx sync:check` enters the pipeline when a sync generator is registered, the `@nx/js/typescript` plugin registers one, and the workspace has none
- A maintained action performs each step one exists for (checkout, toolchain, caches, artifacts, registry login), and a `run` step holds the rest
- The root `workflow` target runs the Linux jobs under act, with the image, the daemon socket, and the server paths under `.cache/` on the command
- Artifact upload and download steps carry no act guard, because the `workflow` target passes `--artifact-server-path` and act serves them
- Use `dotnet-msbuild-packaging` for the MSBuild properties and switches the pipeline passes

## [11]-[ANTI_PATTERNS]

Smells and the form that replaces each:

| [INDEX] | [SMELL]                                                       | [CORRECT_FORM]                                                          |
| :-----: | :------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | `stage-<x>` and `pack-<x>` target pairs per library           | The local plugin infers `stage` and `pack` from each project file       |
|  [02]   | Committed `.nupkg`, `.dylib`, or `.so` files                  | Manifest pin, staging target, and an ignored `.artifacts/` feed         |
|  [03]   | One script per library with its own download code             | One script module with a lookup table and shared operations             |
|  [04]   | `cache: false` on a target with pure outputs                  | `cache: true` with inputs, `dependentTasksOutputFiles`, exact outputs   |
|  [05]   | Outputs under a project directory or `dist/`                  | Every output under `.artifacts/<area>/`, every cache under `.cache/`    |
|  [06]   | Target commands that run `nx run` or `pnpm nx`                | `dependsOn` in the object form naming the project and target            |
|  [07]   | README step lists for machine setup                           | The one provisioning target, and the README points at the skill         |
|  [08]   | Machine paths in a script, project, or manifest               | The root lock file, `$(MSBuildThisFileDirectory)`, or `{workspaceRoot}` |
|  [09]   | `*.nupkg` output globs over a shared feed folder              | The exact `<Id>.<Version>.nupkg` output per project                     |
|  [10]   | Packaging projects inside the solution file                   | The project excluded from the solution and the plugin, packed by target |
|  [11]   | Subtree `Directory.Build.props` files repeating root values   | `Import` through `GetPathOfFileAbove` and the overrides alone           |
|  [12]   | Manifest versions copied into a script constant               | The script reads the manifest, the project checks `Version` against it  |
|  [13]   | Stage targets that run concurrently with another              | `parallelism: false` on every target sharing a tool root                |
|  [14]   | `ArtifactsPath` from `NormalizeDirectory`                     | `NormalizePath`, the SDK appends the separator itself                   |
|  [15]   | Preview, check, or dry-run variants of a target               | One target, `git diff --exit-code` in CI after the rewriting targets    |
|  [16]   | `!env.ACT` on upload steps with unguarded download steps      | Artifacts on under act through the artifact server path                 |
|  [17]   | `test: {}` plugin options or an option restating its default  | No option, an empty object renames and merges nothing                   |
|  [18]   | `targetDefaults` `command` over a root target with `commands` | The root excluded from the filter, its own executor declared            |
|  [19]   | Configuration files per directory where an owner exists       | Plugin inference, the manifest's `nx` field, or the root config         |
|  [20]   | `mise x <command>` or a mise task wrapping a target           | The target under the mise environment from the hook or the shims        |
|  [21]   | Versions in `mise.toml`, `mise.lock`, or a release-age delay  | `latest`, with the language lock files as the only pins                 |
|  [22]   | `[env]` rows for a setting a manifest table holds             | The manifest the tool reads by directory walk (`UV_CACHE_DIR`)          |
|  [23]   | `[env]` rows for a directory one script or program computes   | The script or program derives it beside its other paths                 |
|  [24]   | Binary-only npm packages in the catalog and `allowBuilds`     | `[tools]` rows at `latest`                                              |
|  [25]   | `scripts` in a manifest beside the targets                    | The target, the one entry the graph orders and caches                   |
|  [26]   | A second secret route copied from another repository          | One store, the variable in the environment, an error naming unset names |
|  [27]   | A tool manifest that pins a one-shot tool package             | `dotnet dnx <tool>` on the command                                      |
|  [28]   | An aggregate target that runs every language                  | The project filter on the runner's own command                          |

## [12]-[CORRECTIONS]

Attempts that failed for a reason the tool documentation states and the surface did not show:

| [INDEX] | [ATTEMPT]                                              | [REASON]                                                                       |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | mise-action with its cache on                          | The key hashes the config files, and `latest` tools froze at the cached build  |
|  [02]   | `actions/cache` alone around a long native build       | The post step saves under `success()` alone, and a failed build saved nothing  |
|  [03]   | `restore-keys` on a package cache                      | A prefix match restores an older folder, and the post step saves a new entry   |
|  [04]   | A workflow triggered by release tags                   | A push of more than three tags raises no event, one dispatch workflow instead  |
|  [05]   | `nx release --yes` with `--skip-publish`               | The two flags exclude each other, `--skip-publish` then `nx release publish`   |
|  [06]   | A `targetDefaults` entry to create a target            | Defaults fill a declared target and create none, the plugin emits the target   |
|  [07]   | A default `command` over a root target with `commands` | The default replaced the list, the root leaves the filter                      |
|  [08]   | A missing target output left declared                  | Nx drops it silently and reports a hit, every declared output exists           |
|  [09]   | The swc loader for a `.ts` plugin                      | The native `typescript` package lacks its compiler API, Node strips the types  |
|  [10]   | `latest` or `3.15` for an interpreter at a candidate   | Both resolve the `-dev` branch build under prereleases, the exact pin stays    |
|  [11]   | `[env]` rows for tool caches and program paths         | The export overrode the manifest field, the manifest and the program own them  |
|  [12]   | A machine profile exporting `DOTNET_ROOT`              | The mise install became a link into the profile store, and the SDK bump failed |
|  [13]   | A vcpkg cache directory named before it exists         | vcpkg reads the variables only for absolute existing paths and falls back      |
|  [14]   | Paths composed from `NuGetPackageRoot` under override  | The property lacks a trailing slash under `NUGET_PACKAGES`, ensure the slash   |
|  [15]   | `IsTestProject` expected from the test framework       | xunit v3 sets `IsTestingPlatformApplication` alone, the build files derive it  |
|  [16]   | A `packages.lock.json` per project with locked restore | Central versions resolve the same set, the lock added files and drift failures |
|  [17]   | An `.actrc` file in the repository                     | act reads XDG, home, and current directory files alone, flags on the target    |
|  [18]   | A `project.json` beside a manifest or at the root      | The plugin and the root manifest `nx` field already own the targets            |
|  [19]   | A configuration file per directory                     | The root config keyed by tag or glob already holds the setting                 |
|  [20]   | A `coverage combine` step before the report            | It fails on empty input, and the report commands combine the data themselves   |
