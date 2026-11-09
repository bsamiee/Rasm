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
- Version actions write no dependency rows: a `<ProjectReference>` carries no version, `dotnet pack` writes the referenced project's `<Version>` as the floor, and `nx release` raises every dependent on a `static` edge by a patch when a dependency moves (`updateDependents: "always"`)
- In the workspace, `#analyzer` names the analyzer project, which inference builds in Release on a graph cache miss (`dotnet build analyzer --configuration Release -getTargetResult:Build`) so its output lands where `ArtifactsPath` puts it; the published package names `dist/lib/MsbuildAnalyzer.dll` through `publishConfig.imports`

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                             | [GIVES]                                                                                     |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `*.csproj`, `*.fsproj`, `*.vbproj`                    | One project each, named by `MSBuildProjectName`                                             |
|  [02]   | Every configuration, runtime, and framework the project states | `configurations` named like MSBuild's artifacts pivots (`debug`, `release_win-x64`) |
|  [03]   | `ProjectReference` items of every evaluation          | `static` edges from the project file, `implicit` edges where a shared file declares them    |
|  [04]   | Imports, linked items, and the `Directory.*`, `global.json`, `NuGet.config`, `.editorconfig` walk | Inputs outside the project folder                          |
|  [05]   | `OutputPath`, `IntermediateOutputPath`, and `PublishDir` where `IsPublishable` | Outputs of every mode, under `ArtifactsPath` where `Directory.Build.props` sets it |
|  [06]   | `IsTestProject`, `IsTestingPlatformApplication`, `global.json` `test.runner` | `test` in the runner's own command form                                |
|  [07]   | `IsPackable`, `IsPublishable`                         | Version actions writing `<Version>` and the `release` tag on either; `nx-release-publish` on `IsPackable` alone, since a publish folder ships through its host channel (a Rhino plug-in's Yak reads `<Version>`) |
|  [08]   | Each evaluation's `NuGetLockFilePath`, read through NuGet's `PackagesLockFileFormat` | `nuget:<id>@<version>` external nodes with the lock's content hash, `implicit` edges to direct packages, `static` edges between packages |

- The analyzer runs on the SDK `global.json` selects from the workspace root, evaluates projects in one process, and reports a project it cannot evaluate by name while keeping every other one
- Inference caches by every matched file, the plugin source, and the analyzer build; a cached graph also stays valid only while the files evaluation imported and each project's restore-written `nuget.g.props` are unchanged

## [03]-[TARGETS]

| [INDEX] | [TARGET]              | [COMMAND]                                                                                  | [WHEN]                    | [CACHE] |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- | :------------------------ | :-----: |
|  [01]   | `restore`             | `dotnet restore <project> --no-dependencies`, batched into one run over the selection      | Every project             |   No    |
|  [02]   | `build--dotnet`       | `dotnet build --no-restore --no-dependencies` in the project folder; `dotnet publish --no-restore` in the `Release` modes, the configuration `dotnet publish` selects | Every project; `publish` where `IsPublishable` |   No    |
|  [03]   | `check--dotnet-build` | `dotnet build --no-restore --no-dependencies -p:NonExistentFile=… -warnaserror`            | Every project             |   Yes   |
|  [04]   | `test`                | `dotnet test --no-build` in the project folder, the form both test runners accept          | Test projects             |   Yes   |
|  [05]   | `check--dotnet-format` | `dotnet format style --no-restore --verify-no-changes`, batched into one run over the selection, the IDE rules no build runs included | An `.editorconfig` applies |   Yes   |
|  [06]   | `fix--dotnet-format`  | `dotnet format style`, then `dotnet format analyzers`, batched into one run each over the selection | An `.editorconfig` applies |   No    |
|  [07]   | `build`, `check`, `fix` | `nx:noop` over every `<verb>--*` target, inputs `...` and the union of theirs           | A `<verb>--*` target exists | No |
|  [08]   | `nx-release-publish`  | `dotnet pack <project> --configuration Release`, then `dotnet nuget push --skip-duplicate`  | Packable projects         |   No    |

- `build--dotnet` stays uncached: MSBuild keeps incremental state the editor and direct builds share
- `check` forces `CoreCompile` through MSBuild's `NonExistentFile` input, because MSBuild prints a warning once until the file recompiles; unlike `--no-incremental` it deletes nothing and keeps the reference assembly, so a dependent building beside it reads intact outputs and recompiles nothing afterwards; it depends on `build--dotnet`, so one project's two writers never run at once
- Batched tasks run in dependency waves, each group of one command once over a solution written to a temporary folder outside the workspace, so no solution file sits in the tree; whitespace belongs to hk's `dotnet format whitespace --folder`
- A publishable project publishes in `Release`, since its distributable is the publish folder, and its project references build inside that run: a Rhino plug-in references its libraries under its own global properties, a build no other task produces
- Arguments after `--` reach the `dotnet` command when given to the tool-named target (`nx run <project>:build--dotnet -- -bl`); aggregates forward none, since Nx would pass them to every dependency's members too
