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
- Releases select .NET projects through `release.projects` or `release.groups` in `nx.json`, since `nx release` selects npm libraries alone by default
- In the workspace, `#analyzer` names the analyzer project, which inference builds in Release once per graph computation (`dotnet build analyzer --configuration Release -getTargetResult:Build`, about 0.5 s up to date) so its output lands where `ArtifactsPath` puts it; the published package names `dist/lib/MsbuildAnalyzer.dll` through `publishConfig.imports`

## [02]-[INFERENCE]

| [INDEX] | [SOURCE]                                             | [GIVES]                                                                                     |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `*.csproj`, `*.fsproj`, `*.vbproj`                    | One project each, named by `MSBuildProjectName`                                             |
|  [02]   | Every configuration, runtime, and framework the project states | `configurations` named like MSBuild's artifacts pivots (`debug`, `release_win-x64`) |
|  [03]   | `ProjectReference` items of every evaluation          | `static` edges from the project file, `implicit` edges where a shared file declares them    |
|  [04]   | Imports, linked items, and the `Directory.*`, `global.json`, `NuGet.config`, `.editorconfig` walk | Inputs outside the project folder                          |
|  [05]   | `OutputPath`, `IntermediateOutputPath`                | Outputs of every mode, under `ArtifactsPath` where `Directory.Build.props` sets it          |
|  [06]   | `IsTestProject`, `IsTestingPlatformApplication`, `global.json` `test.runner` | `test` in the runner's own command form                                |
|  [07]   | `IsPackable`                                          | `nx-release-publish` and version actions writing `<Version>`                                |

- The analyzer runs on the SDK `global.json` selects from the workspace root, evaluates projects in one process, and reports a project it cannot evaluate by name while keeping every other one
- Inference caches by every matched file, the plugin source, and the analyzer build; a cached graph also stays valid only while the files evaluation imported and each project's restore-written `nuget.g.props` are unchanged

## [03]-[TARGETS]

| [INDEX] | [TARGET]              | [COMMAND]                                                                                  | [WHEN]                    | [CACHE] |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------- | :------------------------ | :-----: |
|  [01]   | `restore`             | `dotnet restore <project> --no-dependencies`, batched into one run over the selection      | Every project             |   No    |
|  [02]   | `build`               | `dotnet build <project> --no-restore --no-dependencies`                                    | Every project             |   No    |
|  [03]   | `check--dotnet-build` | `dotnet build <project> --no-restore --no-dependencies -p:NonExistentFile=… -warnaserror`  | Every project             |   Yes   |
|  [04]   | `test`                | `dotnet test --project <project> --no-build`, or the VSTest positional form                | Test projects             |   Yes   |
|  [05]   | `fix--dotnet-format`  | `dotnet format <project> --no-restore`, batched into one run over the selection            | An `.editorconfig` applies |   No    |
|  [06]   | `check`, `fix`        | `nx:noop` over every `check--*` and `fix--*` target                                        | Every project             |   Yes   |
|  [07]   | `nx-release-publish`  | `dotnet pack <project> --configuration Release`, then `dotnet nuget push --skip-duplicate`  | Packable projects         |   No    |

- `build` stays uncached: MSBuild keeps incremental state the editor and direct builds share
- `check` forces `CoreCompile` through MSBuild's `NonExistentFile` input, because MSBuild prints a warning once until the file recompiles; unlike `--no-incremental` it deletes nothing and keeps the reference assembly, so a dependent building beside it reads intact outputs and recompiles nothing afterwards; it depends on `build`, so one project's two writers never run at once
- Batched tasks of one command run once over a solution written to a temporary folder outside the workspace, so no solution file sits in the tree
- Arguments after `--` reach the `dotnet` command for one run, a binlog included (`nx build <project> -- -bl`)
