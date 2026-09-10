# [HOOK_POINTS]

Every row names an SDK target a custom target attaches to with `BeforeTargets` or `AfterTargets`, the phase it runs in, and the items and properties present there. SDK chains in execution order: `Restore`, then `Build` = `BeforeBuild`, `CoreBuild`, `AfterBuild`, and `CoreBuild` = `BuildOnlySettings`, `PrepareForBuild`, `PreBuildEvent`, `ResolveReferences`, `PrepareResources`, `ResolveKeySource`, `Compile`, `ExportWindowsMDFile`, `UnmanagedUnregistration`, `GenerateSerializationAssemblies`, `CreateSatelliteAssemblies`, `GenerateManifests`, `GetTargetPath`, `PrepareForRun`, `UnmanagedRegistration`, `IncrementalClean`, `PostBuildEvent`, then `Publish` and `Pack` on top of `Build`.

## [01]-[RESTORE]

`-restore` runs `Restore` in a separate build with `MSBuildIsRestoring=true` as a global property, the restore graph walk evaluates each project with `ExcludeRestorePackageImports=true`, package `build/` imports are absent there.

| [INDEX] | [HOOK]                                        | [RUNS]                              | [AVAILABLE]                                 |
| :-----: | :-------------------------------------------- | :---------------------------------- | :------------------------------------------ |
|  [01]   | `AfterTargets="CollectPackageReferences"`     | Restore, again in build             | `@(PackageReference)`, `MSBuildIsRestoring` |
|  [02]   | `BeforeTargets="_GenerateRestoreProjectSpec"` | Before project spec joins graph     | `$(RestoreProjectStyle)`                    |
|  [03]   | `AfterTargets="Restore"`                      | After assets and `.nuget.g.*` files | `$(ProjectAssetsFile)`                      |

- `_GetRestoreProjectStyle` runs `CollectPackageReferences` in the build, a hook there reads `$(MSBuildIsRestoring)` to tell the phases apart

## [02]-[BUILD]

| [INDEX] | [HOOK]                                        | [RUNS]                                 | [AVAILABLE]                               |
| :-----: | :-------------------------------------------- | :------------------------------------- | :---------------------------------------- |
|  [01]   | `BeforeTargets="PrepareForBuild"`             | Before output directories exist        | Every item, `BuildingProject=true`        |
|  [02]   | `AfterTargets="PrepareForBuild"`              | After output directories exist         | `$(OutDir)`, `$(IntermediateOutputPath)`  |
|  [03]   | `AfterTargets="ResolvePackageAssets"`         | After assets file is read              | `@(RuntimeCopyLocalItems)`, native items  |
|  [04]   | `AfterTargets="ResolveProjectReferences"`     | After every `ProjectReference` built   | `@(_ResolvedProjectReferencePaths)`       |
|  [05]   | `AfterTargets="ResolveAssemblyReferences"`    | After reference closure                | `@(ReferencePath)`, copy-local paths      |
|  [06]   | `AfterTargets="ResolveReferences"`            | After `AfterResolveReferences`         | Complete reference set                    |
|  [07]   | `BeforeTargets="AssignTargetPaths"`           | Before `TargetPath` is assigned        | Last point for a copied or embedded item  |
|  [08]   | `AfterTargets="AssignTargetPaths"`            | After `TargetPath` is assigned         | `@(ContentWithTargetPath)`                |
|  [09]   | `BeforeTargets="CoreCompile"`                 | Before compiler, design-time too       | `@(Compile)`, reference assemblies        |
|  [10]   | `AfterTargets="CoreCompile"`                  | After compiler, unless up to date      | `@(IntermediateAssembly)`                 |
|  [11]   | `AfterTargets="Compile"`                      | After `AfterCompile`                   | Intermediate assembly                     |
|  [12]   | `BeforeTargets="GenerateBuildDependencyFile"` | Before `deps.json` is written          | Resolved copy-local items                 |
|  [13]   | `AfterTargets="CopyFilesToOutputDirectory"`   | After every copy into `$(OutDir)`      | `$(TargetPath)`, `@(FileWrites)` recorded |
|  [14]   | `BeforeTargets="IncrementalClean"`            | Before `PostBuildEvent`                | `@(FileWrites)` complete                  |
|  [15]   | `AfterTargets="Build"`                        | After `AfterBuild`                     | `$(TargetPath)`, `@(InnerOutput)` outer   |
|  [16]   | `BeforeTargets="CoreClean"`                   | `Clean` and `Rebuild`, before deletion | `@(FileWrites)` of prior build            |

- `GetTargetPath`, `GetTargetFrameworks`, `GetNativeManifest`, and `GetCopyToOutputDirectoryItems` are the `ProjectReference` protocol targets
- Protocol targets run without `Build` when a referencing project calls them
- Design-time builds run `ResolveAssemblyReferences`, `CoreCompile`, and the protocol targets with `DesignTimeBuild=true`

## [03]-[PUBLISH]

`Publish` = `_PublishBuildAlternative` (`Build` unless `NoBuild=true`), `PrepareForPublish`, `ComputeAndCopyFilesToPublishDirectory`, `PublishItemsOutputGroup`, and `ComputeAndCopyFilesToPublishDirectory` = `ComputeFilesToPublish`, `CopyFilesToPublishDirectory`.

| [INDEX] | [HOOK]                                              | [RUNS]                              | [AVAILABLE]                                |
| :-----: | :-------------------------------------------------- | :---------------------------------- | :----------------------------------------- |
|  [01]   | `AfterTargets="PrepareForPublish"`                  | After publish options check         | `$(PublishDir)`, `$(_IsPublishing)`        |
|  [02]   | `BeforeTargets="ComputeResolvedFilesToPublishList"` | Before list is computed             | `@(ReferenceCopyLocalPaths)`               |
|  [03]   | `AfterTargets="ComputeFilesToPublish"`              | After list, before copy             | `@(ResolvedFileToPublish)`, `RelativePath` |
|  [04]   | `AfterTargets="CopyFilesToPublishDirectory"`        | After every copy to `$(PublishDir)` | Publish directory                          |
|  [05]   | `AfterTargets="Publish"`                            | After `PublishItemsOutputGroup`     | `@(PublishItemsOutputGroupOutputs)`        |

- `_ResolveCopyLocalAssetsForPublish` fills `_ResolvedCopyLocalPublishAssets` from build assets with `CopyToPublishDirectory` not `false`
- Native items added to `@(NativeCopyLocalItems)` publish without a second hook

## [04]-[PACK]

`Pack` = `$(BeforePack)`, `_GetRestoreProjectStyle`, `_IntermediatePack`, `GenerateNuspec`, and `GenerateNuspec` depends on `Build` unless `NoBuild=true` or `GeneratePackageOnBuild=true`, then on `_LoadPackInputItems`, `_GetTargetFrameworksOutput`, `_WalkEachTargetPerFramework`, `_GetPackageFiles`.

| [INDEX] | [HOOK]                           | [RUNS]                                         | [AVAILABLE]                                  |
| :-----: | :------------------------------- | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | `BeforeTargets="GenerateNuspec"` | After `Build`, before nuspec and package exist | `@(_PackageFiles)`, `$(PackageVersion)`      |
|  [02]   | `AfterTargets="Pack"`            | After `.nupkg` exists                          | `@(NuGetPackOutput)`, `$(PackageOutputPath)` |

- `GenerateNuspec` runs once in the outer build, `_WalkEachTargetPerFramework` calls each inner build with `TargetFramework` as `AdditionalProperties`
