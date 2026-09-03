---
name: monorepo-build-infrastructure
description: "Use when changing eng/, nx.json, or an Nx target: directory map, plugin inference and caching, native manifest pins and staging, provisioning, Directory.Build.props isolation, CI entry."
---

# [MONOREPO_BUILD_INFRASTRUCTURE]

Covers the shared build infrastructure of a polyglot monorepo, from the `eng/` directory to the CI entry point.

- Use `dotnet-msbuild-packaging` for NuGet package projects, central package management, `NuGet.config`, the solution file, lock files, and CI build properties
- Use `dotnet-msbuild-evaluation` for the placement of properties, items, conditions, and imports across `.props`, `.targets`, and project files
- Use `dotnet-msbuild-execution` for targets, `DependsOn` chains, incremental `Inputs` and `Outputs`, and generated files
- Use `dotnet-msbuild-antipatterns` for the review catalog for `.props`, `.targets`, and project files
- Use `dotnet-msbuild-diagnostics` for binlog capture and every build failure or timing investigation

## [01]-[DIRECTORY_MAP]

`eng/` holds what every language area and application consumes at build time:

| [INDEX] | [PATH]                         | [HOLDS]                                                                      |
| :-----: | :----------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `eng/native/<library>/`        | Version manifest and pins of one native library                              |
|  [02]   | `eng/native/<Package>/`        | One packaging project per package id                                         |
|  [03]   | `eng/native/Directory.Build.*` | Properties and targets every packaging project shares                        |
|  [04]   | `eng/scripts/`                 | Target-invoked automation with control flow, one module per operation        |
|  [05]   | `eng/project.json`             | Targets with no owning project, provisioning included                        |
|  [06]   | `tools/<runner>/`              | Task runner plugins and rule files the runner loads from a path              |
|  [07]   | `.cache/<tool>/`               | Relocatable caches, downloads, checkouts, and binary caches                  |
|  [08]   | `.artifacts/<area>/`           | Build outputs, staged trees, packages, graph exports, and reports            |

Composition roots belong to `apps/<name>/`, library code to `libs/`, tool configuration to the root manifests, and every binary to the pipeline, which rebuilds it from a pinned manifest. `libs/` packages consume `eng/` output through package references.

Each fact has one owning file, and every other file reads it from there:

| [INDEX] | [FACT]                     | [OWNER]                        | [READERS]                                      |
| :-----: | :------------------------- | :----------------------------- | :--------------------------------------------- |
|  [01]   | Native library version     | `eng/native/<library>/*.json`  | Packaging project version check                |
|  [02]   | Package version            | Packaging project `Version`    | Target outputs, from the element               |
|  [03]   | Local feed path            | `NuGet.config` local source    | Pack target, from the source value             |
|  [04]   | Output and cache roots     | Root `Directory.Build.props`   | Target outputs                                 |
|  [05]   | Script dependency versions | Root `pyproject.toml` and lock | Scripts under `uv run`                         |

## [02]-[TASK_GRAPH]

Nx infers targets from the files a plugin globs, and a `project.json` exists only where no plugin recognizes a file. The .NET plugin globs every project file and every ancestor `Directory.Build.*` and `Directory.Packages.props`:

| [INDEX] | [PLUGIN_FACT]                            | [CONSEQUENCE]                                                                    |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `build` runs `dotnet build --no-restore` | `dotnet restore <solution>` precedes `nx affected -t build test`                 |
|  [02]   | `build` depends on `^build`              | `ProjectReference` edges order builds, `--no-dependencies` skips referenced ones |
|  [03]   | Outputs derive from `ArtifactsPath`      | One `ArtifactsPath` under `.artifacts/` makes every output cacheable             |
|  [04]   | `pack` outputs the `PackageOutputPath`   | One output over a shared feed caches every sibling, `pack: false` replaces it    |
|  [05]   | `exclude` is a registration property     | Packaging subtrees leave the inferred graph without `project.json`               |
|  [06]   | Directory files are per-target inputs    | Dotnet `build` and `format` entries name `global.json` and `NuGet.config`        |

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
    "**/{*.csproj,package.json,pyproject.toml}",
    async (projectFiles, _options, context) =>
        projectFiles.map((file) => [file, { projects: { [dirname(file)]: projectFor(context.workspaceRoot, file) } }]),
];
```

Export `createDependencies` beside `createNodes`, and each `PackageReference` to a packaging project then becomes a static edge that marks the consumer affected. Nx transpiles a `.ts` plugin at load, and the root `tsconfig.json` includes the file for `tsc --build` and the linter.

The stage, pack, and consume chain as targets:

| [INDEX] | [TARGET]            | [COMMAND]                                            | [DEPENDS_ON]                                     | [CACHE] |
| :-----: | :------------------ | :--------------------------------------------------- | :----------------------------------------------- | :-----: |
|  [01]   | `eng:provision`     | `uv run python -m eng.scripts.provision`             | Nothing                                          | `false` |
|  [02]   | `Native.Item:stage` | `uv run python -m eng.scripts.stage item`            | `{ projects: ["eng"], target: "provision" }`     | `false` |
|  [03]   | `Native.Item:pack`  | `dotnet pack eng/native/Native.Item --output <feed>` | `stage`                                          | `true`  |
|  [04]   | `Item:pack`         | `dotnet pack eng/native/Item --output <feed>`        | `{ projects: ["Native.Item"], target: "stage" }` | `true`  |
|  [05]   | `Consumer:build`    | `dotnet build --no-restore --no-dependencies`        | `^build`                                         | `true`  |

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
- Name the exact package file as the output, because a cache restore rewrites each declared output and a glob over a shared feed writes stale sibling packages back
- Declare a tool version or environment variable a cached target reads as a `{ "runtime": "<command>" }` or `{ "env": "<NAME>" }` input
- Write a cross-project dependency in the object form, `{ "projects": ["<project>"], "target": "<target>" }`
- Extra arguments forward to the command, `nx run Native.Item:stage --rid linux-x64` reaches the script as `--rid=linux-x64`
- The runner reads the exit code alone, and a script reports failure by exiting nonzero
- Local feeds need no `nx release` configuration

`nx affected` is correct when every edge exists in the graph: `ProjectReference` edges from the .NET plugin, `PackageReference` edges to packaging projects from the local plugin, and `implicitDependencies` from a managed binding to its native package. `nx graph --file=<path>` writes the graph, and `nx show projects --affected --files=<manifest>` proves an edge by listing the packaging project, its binding, and its consumers.

## [03]-[NATIVE_PACKAGING]

Each native library has one manifest directory as its single version pin, and the packaging project checks its `Version` against that pin before `GenerateNuspec`:

| [INDEX] | [SOURCE]              | [MANIFEST]                           | [PIN]                                                           |
| :-----: | :-------------------- | :----------------------------------- | :-------------------------------------------------------------- |
|  [01]   | vcpkg port            | `vcpkg.json` with `builtin-baseline` | Baseline port version, `version-string` equals it               |
|  [02]   | Release archive       | `<kind>.json` with a digest per rid  | Version plus SHA-256 per file                                   |
|  [03]   | Source checkout       | `source.json` with a commit          | Commit, the wrapper version follows it                          |
|  [04]   | Registry-locked asset | Manifest plus `CentralPackageId`     | Version equals the central package version                      |

Stage the layout NuGet's runtime graph reads, `dotnet pack` includes the tree without renaming:

```text
.artifacts/native/<library>/stage/
├── runtimes/<rid>/native/<file>      # Shared libraries and loadable extensions, one directory per runtime identifier
├── contentFiles/<path>               # Data trees a runtime loads by path, packed with copyToOutput
└── managed/*.cs                      # Generated binding sources a managed packaging project compiles
```

Staging rules:
- Stage every library from one script module, a lookup table maps the library name to its staging function, and the shared operations (download, digest check, extract, vcpkg install, copy) exist once
- Run vcpkg with `--x-manifest-root` and `--x-install-root` under `.artifacts/` and `VCPKG_DEFAULT_BINARY_CACHE` under `.cache/`
- Check the baseline port version against the manifest `version-string` before any download
- On macOS, rewrite every install name in a shared library closure to `@loader_path` and sign it ad hoc, and the set loads from its own directory
- Take the runtime identifier as an argument with the host as default, each CI host stages its own rid, and one job packs the collected trees
- Key the output of a long compile by commit under `.cache/`, and a repeat run copies it

The asset-only package holds `runtimes/`, `contentFiles/`, and a `lib/<tfm>/_._` placeholder with `IncludeBuildOutput` false, and a pinned `DeterministicTimestamp` makes the package bytes a function of content and version:
- Use `dotnet-msbuild-packaging` for the package layout

Give a library with a generated binding a managed packaging project, `Item` beside `Native.Item`, that compiles `stage/managed/*.cs` with `IncludeBuildOutput` true, shares the manifest version, and depends on the native `stage` target. Separate package ids keep the native assets loadable from any binding and the binding free of platform-specific content.

The local feed is a folder source in `NuGet.config` under `.artifacts/`, and package source mapping pins every workspace id to that source and every other id to the registry:
- Use `dotnet-msbuild-packaging` for the source list and its mapping

Reference the native package beside the binding, and an `Error` task in the root `Directory.Build.targets` fails a project that references one without the other, because the binding package holds no native asset.

## [04]-[PROVISIONING]

Every `stage` target depends on `eng:provision`, the package manager install of the runner precedes it on a fresh clone, and the README states the commands:

| [INDEX] | [TOOL]         | [MANIFEST]                     | [PLACEMENT]                         | [IDEMPOTENCE]                                |
| :-----: | :------------- | :----------------------------- | :---------------------------------- | :------------------------------------------- |
|  [01]   | Python scripts | `pyproject.toml` and `uv.lock` | `.venv/`                            | `uv run` syncs before every invocation       |
|  [02]   | vcpkg          | Commit in the script           | `.cache/vcpkg/`, archives beside it | Fetch and checkout only on a HEAD mismatch   |
|  [03]   | Host tools     | Port name in the script        | `.cache/<tool>-hosttools/`          | Skip when the executable exists              |
|  [04]   | Release files  | Manifest digest per rid        | `.cache/<name>/<version>/`          | Skip when the digest-verified file exists    |

Provisioning rules:
- Verify a pinned digest on every download, unlink the file on a mismatch, and give a partial download a temporary name
- Pin every checkout to a commit, fetch with depth one, and update HEAD only when it differs
- Find the repository root as the nearest ancestor directory holding the root lock file
- Take every tool a package manager can pin from that manager, and download the rest
- `uv run` syncs the groups `default-groups` under `[tool.uv]` lists, and a dependency in another group needs `--group` on the target command
- Set `cache: false` and `parallelism: false` on the target, because provisioning mutates shared directories

## [05]-[ISOLATION]

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

## [06]-[CI_ENTRY]

CI runs the task graph through the runner, and the pipeline file holds the commands alone:

| [INDEX] | [STEP]                                     | [REASON]                                                                |
| :-----: | :----------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `nrwl/nx-set-shas`                         | Sets the base and head commits `nx affected` compares                   |
|  [02]   | `dotnet restore <solution>`                | Inferred `build` targets pass `--no-restore`, and lock mode holds on CI |
|  [03]   | `nx sync:check`                            | Fails before any task when a sync generator has pending changes         |
|  [04]   | `nx affected -t build test --parallel=<n>` | One command runs every affected target in graph order                   |
|  [05]   | `nx run-many -t pack -p tag:<tag>`         | Packs every packaging project on the host that staged its rid           |
|  [06]   | `CI=true` in the environment               | Nx computes the graph in process, no daemon, `NX_DAEMON` overrides      |

CI rules:
- `defaultBase` in `nx.json` names the base branch, and a pull request compares against it
- `neverConnectToCloud: true` keeps the cache local, and `cacheDirectory` sits under `.cache/`
- Pass `parallel` as a command flag on CI, the workspace default of 3 serves a developer machine
- Task sync generators run in dry-run mode on CI and fail the task when the workspace is out of sync, and `disabledTaskSyncGenerators` in `nx.json` removes a generator for a file the workspace maintains by hand
- The CI host caches each package folder keyed on its lock file hash, `packages.lock.json` for the NuGet global packages folder, `uv.lock` for the uv cache, and `pnpm-lock.yaml` for the pnpm store
- The pipeline file and the README's CI jobs appear together, once the repository has a CI host
- Use `dotnet-msbuild-packaging` for the MSBuild properties and switches the pipeline passes

## [07]-[ANTI_PATTERNS]

Smells and the form that replaces each:

| [INDEX] | [SMELL]                                                       | [CORRECT_FORM]                                                          |
| :-----: | :------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | `stage-<x>` and `pack-<x>` target pairs per library           | The local plugin infers `stage` and `pack` from each project file       |
|  [02]   | Committed `.nupkg`, `.dylib`, or `.so` files                  | Manifest pin, staging target, and an ignored `.artifacts/` feed         |
|  [03]   | One script per library with its own download code             | One script module with a lookup table and shared operations             |
|  [04]   | `cache: false` on a target with pure outputs                  | `cache: true` with inputs, `dependentTasksOutputFiles`, exact outputs   |
|  [05]   | Outputs under a project directory or `dist/`                  | Every output under `.artifacts/<area>/`, every cache under `.cache/`    |
|  [06]   | Target commands that run `nx run` or `pnpm nx`                | `dependsOn` in the object form naming the project and target            |
|  [07]   | README step lists for machine setup                           | `nx run eng:provision`, and the README names the command                |
|  [08]   | Machine paths in a script, project, or manifest               | The root lock file, `$(MSBuildThisFileDirectory)`, or `{workspaceRoot}` |
|  [09]   | `*.nupkg` output globs over a shared feed folder              | The exact `<Id>.<Version>.nupkg` output per project                     |
|  [10]   | Packaging projects inside the solution file                   | The project excluded from the solution and the plugin, packed by target |
|  [11]   | Subtree `Directory.Build.props` files that repeat root values | `Import` through `GetPathOfFileAbove` and the overrides alone           |
|  [12]   | Manifest versions copied into a script constant               | The script reads the manifest, the project checks `Version` against it  |
|  [13]   | Stage targets that run concurrently with another              | `parallelism: false` on every target sharing a tool root                |
|  [14]   | `ArtifactsPath` from `NormalizeDirectory`                     | `NormalizePath`, the SDK appends the separator itself                   |
