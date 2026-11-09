# [NX_DOTNET]

`@rasm/nx-dotnet` is an Nx 24 plugin for MSBuild projects, forked from `@nx/dotnet` 24.0.5. One MSBuild evaluation of the workspace states every project, edge, input, and output, and each target runs the `dotnet` command a developer would type.

## [01]-[USE]

```json
{ "plugins": ["@rasm/nx-dotnet"] }
```

Options take `@nx/dotnet`'s per-target form: a key names an inferred target, `false` removes it from every project and from every `dependsOn` naming it, and an object renames it through `targetName` or merges target fields over the inferred ones; a rename reaches every `dependsOn` that names the target.

```json
{ "plugin": "@rasm/nx-dotnet", "options": { "fix--dotnet-format": false, "test": { "targetName": "unit", "parallelism": false } } }
```

- Facts MSBuild's own files state are read back, never options: `ArtifactsPath` in `Directory.Build.props` places every output, `global.json` selects the SDK and the test runner, `NuGet.config` the package sources, `.editorconfig` the style rules
- Every packable or publishable project carries the `release` tag, so `nx.json` selects .NET releases with `tag:release` in `release.projects`, since `nx release` selects npm libraries alone by default
- Version actions write `<Version>` through MSBuild's own project writer (`MsbuildAnalyzer version <version>`, the project text on stdin, `ProjectRootElement` with formatting preserved), into the last unconditioned `<Version>` or the first unconditioned `<PropertyGroup>`; the registry version is the highest SemVer `dotnet package search` lists across every source
- Version actions move every `AnalyzerReleases.Unshipped.md` row into its `AnalyzerReleases.Shipped.md` under `## Release <major>.<minor>.<patch>` on a stable release (Roslyn reads the header through `System.Version`, which holds no prerelease, so prereleases leave the rows unshipped), for the pair the project lists as `AdditionalFiles` and for the pairs of every project in its reference closure that ships through it (closure stopping at a packable or publishable reference, which releases its own)
- Version actions write no dependency rows: a `<ProjectReference>` carries no version, `dotnet pack` writes the referenced project's `<Version>` as the floor, and `nx release` raises every dependent on a `static` edge by a patch when a dependency moves (`updateDependents: "always"`)
- `#analyzer` names `analyzer/dist/MsbuildAnalyzer.dll` in the workspace and in the published package alike; the analyzer project states that folder as its `Release` `OutputPath`, since npm packs only files inside the package, and `tsc` keeps the package's own `dist`; its inferred `nx build MsbuildAnalyzer -c release` refreshes what inference runs and the package's `prepare` (`dotnet build analyzer --configuration Release`) covers a fresh install; inference never builds anything and a half-saved MSBuild file in the tree cannot stop the graph

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                             | [GIVES]                                                                                     |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `*.csproj`, `*.fsproj`, `*.vbproj`                    | One project each, named by `PackageId` where `IsPackable` (the identity `nx release` tags), else `MSBuildProjectName`; a name two projects claim fails as Nx's own duplicate-name error, so a package project whose `PackageId` equals a sibling's project name renames one of them (the Roslyn template layout names the analyzer project for its role, `Rasm.Analyzers.Diagnostics`) |
|  [02]   | Every configuration, runtime, and framework the project states | `configurations` named like MSBuild's artifacts pivots (`debug`, `release_win-x64`) |
|  [03]   | `ProjectReference` items of every evaluation          | `static` edges from the project file, `implicit` edges where a shared file declares them    |
|  [04]   | Imports, linked items, and the `Directory.*`, `global.json`, `NuGet.config`, `.editorconfig` walk | Inputs outside the project folder                          |
|  [05]   | `OutputPath`, `IntermediateOutputPath`, and `PublishDir` where `IsPublishable` | `build--dotnet` outputs per configuration (`{options.outputs.<n>}`, each configuration setting `outputs`), so `-c release` names the release folders alone; the publish folder only in the mode that publishes |
|  [06]   | `IsTestProject`, `IsTestingPlatformApplication`, `global.json` `test.runner` | `test` in the runner's own command form                                |
|  [07]   | `IsPackable`, `IsPublishable`                         | Version actions writing `<Version>` and the `release` tag on either; `nx-release-publish` on `IsPackable` alone, since a publish folder ships through its host channel (a Rhino plug-in's Yak reads `<Version>`) |
|  [08]   | Each evaluation's `NuGetLockFilePath`, read through NuGet's `PackagesLockFileFormat` | `nuget:<id>@<version>` external nodes with the lock's content hash, `implicit` edges to direct packages, `static` edges between packages |

- The analyzer runs on the SDK `global.json` selects from the workspace root, evaluates projects in one process, and reports a project it cannot evaluate by name while keeping every other one
- Inference caches by every matched file, the plugin source, and the analyzer assembly's folder stamps; a cached graph also stays valid only while the files evaluation imported and each project's restore-written `nuget.g.props` are unchanged

## [03]-[TARGETS]

| [INDEX] | [TARGET]              | [COMMAND]                                                                                  | [WHEN]                    | [CACHE] |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- | :------------------------ | :-----: |
|  [01]   | `restore`             | `dotnet restore <project> --no-dependencies`, batched into one run over the selection; no `^restore`, since NuGet evaluates references and reads none of their restore output | Every project             |   No    |
|  [02]   | `build--dotnet`       | `dotnet build --no-restore --no-dependencies` in the project folder; `dotnet publish --no-restore` in the `Release` modes, the configuration `dotnet publish` selects | Every project; `publish` where `IsPublishable` |   No    |
|  [03]   | `check--dotnet-build` | `dotnet build --no-restore --no-dependencies -p:NonExistentFile=… -warnaserror`            | Every project             |   Yes   |
|  [04]   | `test`                | `dotnet test --no-build` in the project folder, the form both test runners accept; keyed also on the built `*.runtimeconfig.json`, the run configuration the .NET host reads | Test projects             |   Yes   |
|  [05]   | `check--dotnet-format` | `dotnet format style --no-restore --verify-no-changes`, batched into one run over the selection, the IDE rules no build runs included | An `.editorconfig` applies |   Yes   |
|  [06]   | `fix--dotnet-format`  | `dotnet format style`, then `dotnet format analyzers`, batched into one run each over the selection | An `.editorconfig` applies |   No    |
|  [07]   | `build`, `check`, `fix` | `nx:noop` over every `<verb>--*` target, inputs `...` and the union of theirs           | A `<verb>--*` target exists | No |
|  [08]   | `nx-release-publish`  | `dotnet pack <project> --configuration Release`, then `dotnet nuget push --skip-duplicate`, the key never on its command line: NuGet reads `NUGET_API_KEY` from the environment, then the source's `apikeys` entry in `NuGet.config`; in GitHub Actions, trusted publishing's `NuGet/login` step exchanges the job's OIDC token for a short-lived key the job exports as `NUGET_API_KEY` | Packable projects         |   No    |

- `build--dotnet` stays uncached: MSBuild keeps incremental state the editor and direct builds share
- `check` forces `CoreCompile` through MSBuild's `NonExistentFile` input, because MSBuild prints a warning once until the file recompiles; unlike `--no-incremental` it deletes nothing and keeps the reference assembly, so a dependent building beside it reads intact outputs and recompiles nothing afterwards; it depends on `build--dotnet`, so one project's two writers never run at once
- Targets the plugin's own executor runs hash the plugin's source, since Nx 24 hashes only `@nx/` executor packages: in the workspace that holds it (its manifest a tracked workspace file) through `{ "input": "default", "projects": ["@rasm/nx-dotnet"] }`, installed from npm through `{ "externalDependencies": ["@rasm/nx-dotnet"] }`, the package's external node
- Batched tasks run in dependency waves, each group of one command once over a temporary solution of the selected projects written outside the workspace, so a run touches no solution the workspace keeps; whitespace belongs to hk's `dotnet format whitespace --folder`
- A publishable project publishes in `Release`, since its distributable is the publish folder, and its project references build inside that run: a Rhino plug-in references its libraries under its own global properties, a build no other task produces
- Arguments after `--` reach the `dotnet` command when given to the tool-named target (`nx run <project>:build--dotnet -- -bl`); aggregates forward none, since Nx would pass them to every dependency's members too
