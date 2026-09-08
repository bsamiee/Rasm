# [DOTNET]

Configure .NET through shared directory files, central package versions, and project declarations. Nx infers the targets local commands and CI run.

## [01]-[INFERENCE]

.NET plugin globs every project file and ancestor `Directory.Build.*` and `Directory.Packages.props`. Registration options and tag-filtered defaults decide what each inferred target runs:

| [INDEX] | [PLUGIN_FACT]                                   | [CONSEQUENCE]                                                                     |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `restore: false` and `build --no-restore`       | `build` depends on root `restore`, the one solution restore                       |
|  [02]   | `build` depends on `^build`                     | `ProjectReference` edges order builds, `--no-dependencies` skips referenced ones  |
|  [03]   | Outputs derive from `ArtifactsPath`             | One `ArtifactsPath` under `.artifacts/` makes every output cacheable              |
|  [04]   | `pack: false`, `publish: false`, `clean: false` | Inferred `pack` over a shared feed caches every sibling, the publish script packs |
|  [05]   | `exclude` is a registration property            | Plugin skips packaging subtrees without `project.json`                            |
|  [06]   | Directory files are per-target inputs           | Named input `dotnet` lists them, filtered defaults add it per target              |

`nx affected` is correct when every edge exists in the graph:
- `ProjectReference` edges come from the .NET plugin
- `PackageReference` edges from a consumer to a packaging project come from the local plugin
- `implicitDependencies` from a managed binding to its native package come from the local plugin, paired by library name

Each `test` target writes a Cobertura report under `.artifacts/dotnet/coverage/<project>`. Root `coverage` merges the reports through `dotnet dnx dotnet-reportgenerator-globaltool` into Cobertura, lcov, and a markdown summary over workspace assemblies alone. `does not exist (any more)` on a referenced package source path is a collector setting to correct.

## [02]-[PACKAGING]

Each packaging project derives `Version` from the library manifest under `eng/native/<library>/`, checked before `GenerateNuspec`:
- `Version` is the manifest `version-string`
- `VersionManifestFileName` names a manifest other than `vcpkg.json`
- `Error` tasks fail a pack with no manifest version
- `Error` tasks fail a `Version` that differs from the `CentralPackageId` entry of a project locked to a central version

Asset-only packages hold `runtimes/`, `contentFiles/any/any/` with `PackageCopyToOutput`, and a `lib/<tfm>/_._` placeholder with `IncludeBuildOutput` false:
- Pinned `DeterministicTimestamp` makes package bytes a function of content and version
- `contentFiles` with copy-to-output keeps the staged tree in consumer output where a runtime-specific publish flattens `runtimes/` native assets
- `EnableDefaultItems` false keeps the project directory out of the package
- `Error` tasks fail a pack with nothing staged

Libraries with a generated binding take a managed packaging project, `Item` beside `Native.Item`, under `IncludeBuildOutput` true:
- Compiles `stage/managed/*.cs` with the support sources beside its project file
- Shares the manifest version
- Depends on the native `stage` target

Separate package ids keep native assets loadable from any binding and the binding free of platform-specific content.

`globalPackagesFolder` in `NuGet.config` places the one restore folder every client shares under `.cache/`.

`Error` tasks in root `Directory.Build.targets` fail a consumer that references the binding or native package without the other, or a binding without the companion project holding its runtime initialization.

Read `NuGetPackOutput` from the `dotnet pack -getItem:NuGetPackOutput` invocation that creates the package. Publish from its `FullPath` with the main `.nupkg` distinguished from symbol packages. Package identity, normalized version, and output path come from the SDK.

Local feeds need no `nx release` configuration. Publish script under the .NET `nx-release-publish` default pushes with `--skip-duplicate`.

## [03]-[SUBTREE]

Take a packaging subtree out of the root `Directory.Build.props` chain when the root enforces rules packaging projects break (`Version` in the project file, `IsPackable` false, analyzer references under central package management, `ArtifactsPath` for the language area):

| [INDEX] | [FORM] | [FILE]                                   | [WHEN]                                              |
| :-----: | :----- | :--------------------------------------- | :-------------------------------------------------- |
|  [01]   | Chain  | `Import` through `GetPathOfFileAbove`    | Subtree needs the root defaults and overrides a few |
|  [02]   | Stop   | `Directory.Build.props` with no `Import` | Subtree breaks root rules and shares no default     |

```xml
<!-- Chain form, the root file evaluates first and the subtree overrides after -->
<Project>
    <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />
    <PropertyGroup>
        <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '..', '.artifacts', 'packaging'))</ArtifactsPath>
    </PropertyGroup>
</Project>
```

MSBuild finds the nearest directory file and searches no further up. Stop form pairs with a subtree `Directory.Build.targets` and a `Directory.Packages.props` that sets `ManagePackageVersionsCentrally` to `false`. Native packaging subtree takes the stop form with its own `ArtifactsPath` and `PackageOutputPath` at the local feed. `dotnet msbuild <project> -getProperty:<RootProperty>` proves the choice, the chain returns the root value and the stop returns empty.

## [04]-[BUILD_FILES]

Project files state what the root directory files cannot derive from the project role and package references:
- Root `Directory.Build.targets` gives each project role an allowed set of reference roles and fails a reference outside it
- Compose a path from `NuGetPackageRoot` through `EnsureTrailingSlash`, the property lacks the slash under a `NUGET_PACKAGES` override
- Derive `IsTestProject` from `IsTestingPlatformApplication`, set by the test framework package, and the plugin infers `test` from it
- Keep no `packages.lock.json`, central versions with transitive pinning resolve the same set
- Set `ContinuousIntegrationBuild` from the `CI` variable hosted runners export

## [05]-[ANTI_PATTERNS]

| [INDEX] | [SMELL]                                                     | [CORRECT_FORM]                                                       |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | Packaging projects inside the solution file                 | Project excluded from the solution and .NET plugin, packed by target |
|  [02]   | Subtree `Directory.Build.props` files repeating root values | `Import` through `GetPathOfFileAbove` and the overrides alone        |
|  [03]   | `ArtifactsPath` from `NormalizeDirectory`                   | `NormalizePath`, the SDK appends the separator                       |
|  [04]   | Tool manifest for a package run through `dnx`               | `dotnet dnx <tool>` on the command                                   |
|  [05]   | Package path from a file name or the newest feed file       | `NuGetPackOutput` `FullPath` from the pack invocation                |

Use `dotnet-msbuild-packaging` for package layout, source mapping, the dotnet-outdated command line, and pipeline MSBuild switches.
