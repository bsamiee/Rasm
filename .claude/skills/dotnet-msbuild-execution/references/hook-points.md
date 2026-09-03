# [HOOK_POINTS]

Every row names an SDK target that a custom target attaches to with `BeforeTargets` or `AfterTargets`, the phase it runs in, and the items and properties that exist at that point. The .NET SDK 10 chains read, in execution order: `Restore`, then `Build` = `BeforeBuild`, `CoreBuild`, `AfterBuild`, and `CoreBuild` = `BuildOnlySettings`, `PrepareForBuild`, `PreBuildEvent`, `ResolveReferences`, `PrepareResources`, `ResolveKeySource`, `Compile`, `ExportWindowsMDFile`, `UnmanagedUnregistration`, `GenerateSerializationAssemblies`, `CreateSatelliteAssemblies`, `GenerateManifests`, `GetTargetPath`, `PrepareForRun`, `UnmanagedRegistration`, `IncrementalClean`, `PostBuildEvent`, then `Publish` and `Pack` on top of `Build`.

## [01]-[RESTORE]

Restore is a separate MSBuild invocation under `-restore`, with `MSBuildIsRestoring=true` and `ExcludeRestorePackageImports=true` as global properties, and package `build/` imports are absent there.

| [INDEX] | [HOOK]                                        | [RUNS]                                  | [AVAILABLE]                                 |
| :-----: | :-------------------------------------------- | :-------------------------------------- | :------------------------------------------ |
|  [01]   | `AfterTargets="CollectPackageReferences"`     | Restore, and again in the build         | `@(PackageReference)`, `MSBuildIsRestoring` |
|  [02]   | `BeforeTargets="_GenerateRestoreProjectSpec"` | Before the project spec joins the graph | `$(RestoreProjectStyle)`                    |
|  [03]   | `AfterTargets="Restore"`                      | After the assets and `.nuget.g.*` files | `$(ProjectAssetsFile)`                      |

- `_GetRestoreProjectStyle` also runs `CollectPackageReferences` in the build, and a hook there reads `$(MSBuildIsRestoring)` to tell the phases apart

## [02]-[BUILD]

| [INDEX] | [HOOK]                                        | [RUNS]                                 | [AVAILABLE]                                 |
| :-----: | :-------------------------------------------- | :------------------------------------- | :------------------------------------------ |
|  [01]   | `BeforeTargets="PrepareForBuild"`             | Before the output directories exist    | Every item, `BuildingProject=true`          |
|  [02]   | `AfterTargets="PrepareForBuild"`              | After the output directories exist     | `$(OutDir)`, `$(IntermediateOutputPath)`    |
|  [03]   | `AfterTargets="ResolvePackageAssets"`         | After the assets file is read          | `@(RuntimeCopyLocalItems)`, native items    |
|  [04]   | `AfterTargets="ResolveProjectReferences"`     | After every `ProjectReference` built   | `@(_ResolvedProjectReferencePaths)`         |
|  [05]   | `AfterTargets="ResolveAssemblyReferences"`    | After the reference closure            | `@(ReferencePath)`, copy-local paths        |
|  [06]   | `AfterTargets="ResolveReferences"`            | After `AfterResolveReferences`         | The complete reference set                  |
|  [07]   | `BeforeTargets="AssignTargetPaths"`           | Before `TargetPath` is assigned        | Last point to add a copied or embedded item |
|  [08]   | `AfterTargets="AssignTargetPaths"`            | After `TargetPath` is assigned         | `@(ContentWithTargetPath)`                  |
|  [09]   | `BeforeTargets="CoreCompile"`                 | Before the compiler, design-time too   | `@(Compile)`, reference assemblies          |
|  [10]   | `AfterTargets="CoreCompile"`                  | After the compiler, unless up to date  | `@(IntermediateAssembly)`                   |
|  [11]   | `AfterTargets="Compile"`                      | After `AfterCompile`                   | The intermediate assembly                   |
|  [12]   | `BeforeTargets="GenerateBuildDependencyFile"` | Before `deps.json` is written          | The resolved copy-local items               |
|  [13]   | `AfterTargets="CopyFilesToOutputDirectory"`   | After every copy into `$(OutDir)`      | `$(TargetPath)`, `@(FileWrites)` recorded   |
|  [14]   | `BeforeTargets="IncrementalClean"`            | Before `PostBuildEvent`                | `@(FileWrites)` complete                    |
|  [15]   | `AfterTargets="Build"`                        | After `AfterBuild`                     | `$(TargetPath)`, `@(InnerOutput)` outer     |
|  [16]   | `BeforeTargets="CoreClean"`                   | `Clean` and `Rebuild`, before deletion | `@(FileWrites)` of the prior build          |

- `GetTargetPath`, `GetTargetFrameworks`, `GetNativeManifest`, and `GetCopyToOutputDirectoryItems` are the `ProjectReference` protocol targets a referencing project calls, and they run without `Build`
- Design-time builds run `ResolveAssemblyReferences`, `CoreCompile`, and the protocol targets with `DesignTimeBuild=true`

## [03]-[PUBLISH]

`Publish` = `_PublishBuildAlternative` (`Build` unless `NoBuild=true`), `PrepareForPublish`, `ComputeAndCopyFilesToPublishDirectory`, `PublishItemsOutputGroup`, and `ComputeAndCopyFilesToPublishDirectory` = `ComputeFilesToPublish`, `CopyFilesToPublishDirectory`.

| [INDEX] | [HOOK]                                              | [RUNS]                              | [AVAILABLE]                                |
| :-----: | :-------------------------------------------------- | :---------------------------------- | :----------------------------------------- |
|  [01]   | `AfterTargets="PrepareForPublish"`                  | After the publish options check     | `$(PublishDir)`, `$(_IsPublishing)`        |
|  [02]   | `BeforeTargets="ComputeResolvedFilesToPublishList"` | Before the list is computed         | `@(ReferenceCopyLocalPaths)`               |
|  [03]   | `AfterTargets="ComputeFilesToPublish"`              | After the list, before the copy     | `@(ResolvedFileToPublish)`, `RelativePath` |
|  [04]   | `AfterTargets="CopyFilesToPublishDirectory"`        | After every copy to `$(PublishDir)` | The publish directory                      |
|  [05]   | `AfterTargets="Publish"`                            | After `PublishItemsOutputGroup`     | `@(PublishItemsOutputGroupOutputs)`        |

- `_ResolveCopyLocalAssetsForPublish` fills `_ResolvedCopyLocalPublishAssets` from `@(_ResolvedCopyLocalBuildAssets)` with `CopyToPublishDirectory` not `false`, and a native item added to `@(NativeCopyLocalItems)` publishes without a second hook

## [04]-[PACK]

`Pack` = `$(BeforePack)`, `_GetRestoreProjectStyle`, `_IntermediatePack`, `GenerateNuspec`, and `GenerateNuspec` depends on `Build` unless `NoBuild=true` or `GeneratePackageOnBuild=true`, then on `_LoadPackInputItems`, `_GetTargetFrameworksOutput`, `_WalkEachTargetPerFramework`, `_GetPackageFiles`.

| [INDEX] | [HOOK]                           | [RUNS]                                             | [AVAILABLE]                                  |
| :-----: | :------------------------------- | :------------------------------------------------- | :------------------------------------------- |
|  [01]   | `BeforeTargets="GenerateNuspec"` | After `Build`, before the nuspec and package exist | `@(_PackageFiles)`, `$(PackageVersion)`      |
|  [02]   | `AfterTargets="Pack"`            | After the `.nupkg` exists                          | `@(NuGetPackOutput)`, `$(PackageOutputPath)` |

- `GenerateNuspec` runs once in the outer build of a multi-targeting project and calls the inner builds through the `MSBuild` task with `TargetFramework` as `AdditionalProperties`
- Use `dotnet-msbuild-packaging` for the package layout
