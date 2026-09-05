---
name: dotnet-msbuild-packaging
description: "Use when a .csproj, Directory.Packages.props, NuGet.config, or .slnx file changes: project set, central package management, restore, package authoring, CI build properties, NU codes."
---

# [DOTNET_MSBUILD_PACKAGING]

Covers the package and project files of a repository, from the project set to the properties a CI pipeline passes.

- Use `dotnet-msbuild-evaluation` for import order, conditions, and the file that owns a property or item
- Use `dotnet-msbuild-execution` for targets, `DependsOn` chains, and the SDK hook points
- Use `dotnet-msbuild-antipatterns` for the review catalog of build-file smells
- Use `dotnet-msbuild-diagnostics` for binlog capture and queries and BuildCheck
- Use `monorepo-build-infrastructure` for the `eng/` directory, task runner targets, and the isolation of a packaging subtree from the root build files

[REFERENCES]:
- [01]-[NUGET_CODES](references/nuget-codes.md): `NU1xxx` restore codes and `NU5xxx` pack codes with the cause and the fix of each

## [01]-[PROJECT_SET]

Project files hold only what differs from the root `Directory.Build.props`: the `Sdk` attribute, `PackageReference` and `ProjectReference` items, and the properties that vary per project. Projects that restate a root value fail the review.

```xml
<!-- Library/Library.csproj: the root props own TargetFramework, Nullable, analyzers, and artifacts -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Description>Domain model for the Item aggregate</Description>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Contoso.Logging.Abstractions" />
    <ProjectReference Include="../Core/Core.csproj" />
  </ItemGroup>
</Project>
```

Files with no place in the project set:

| [INDEX] | [FILE]                                          | [REASON]                                                                       |
| :-----: | :---------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `.nuspec` beside an SDK project                 | `NuspecFile` discards every project value, pack generates the nuspec           |
|  [02]   | `AssemblyInfo.cs`                               | `GenerateAssemblyInfo` writes the attributes from `Version` and `Description`  |
|  [03]   | `packages.config`                               | SDK projects restore `PackageReference` items only                             |
|  [04]   | `app.config` in a library                       | Binding redirects belong to the executable                                     |
|  [05]   | `Directory.Build.rsp` with `-p:` switches       | Global properties no project overrides, defaults go in `Directory.Build.props` |
|  [06]   | `Directory.Solution.props` with a project value | Imported by the solution build only, no project reads the value                |
|  [07]   | Committed `.nupkg` files                        | `dotnet pack` writes them under `PackageOutputPath`, which `.gitignore` covers |
|  [08]   | `bin/` or `obj/` under the tree                 | `UseArtifactsOutput` moves both, a stray copy means a project set its own path |

`UseArtifactsOutput` or `ArtifactsPath` in the root `Directory.Build.props` gives `<ArtifactsPath>/<type>/<project>/<pivot>/`, type is `bin`, `obj`, or `publish`, and restore writes `project.assets.json`, `*.nuget.g.props`, and `*.nuget.g.targets` to `<ArtifactsPath>/obj/<project>/`. `ArtifactsPath` takes `NormalizePath`, because the SDK composes `<ArtifactsPath>\bin\<project>\` itself and the trailing slash of a `NormalizeDirectory` value yields `<ArtifactsPath>//bin/<project>/`, which `dotnet msbuild <project> -getProperty:OutputPath` shows.

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <UseArtifactsOutput>true</UseArtifactsOutput>
  <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '.artifacts', 'dotnet'))</ArtifactsPath>
</PropertyGroup>
```

## [02]-[CENTRAL_PACKAGE_MANAGEMENT]

`Directory.Packages.props` owns every version in `PackageVersion` items, and projects name packages without a version. `NuGet.props` imports the nearest `Directory.Packages.props` at or above the project directory, and a nested file imports the outer one at its top.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Contoso.Logging.Abstractions" Version="10.0.11" />
  </ItemGroup>
  <ItemGroup>
    <!-- Every project restores it with PrivateAssets=all and no compile asset, its Version sits on the element itself -->
    <GlobalPackageReference Include="Contoso.Analyzers" Version="3.0.203" />
  </ItemGroup>
</Project>
```

| [INDEX] | [PROPERTY]                               | [DEFAULT] | [MEANING]                                                              |
| :-----: | :--------------------------------------- | :-------- | :--------------------------------------------------------------------- |
|  [01]   | `CentralPackageVersionOverrideEnabled`   | `true`    | `false` rejects `VersionOverride`, per `Directory.Packages.props` file |
|  [02]   | `CentralPackageTransitivePinningEnabled` | `false`   | Every `PackageVersion` pins that package when it is transitive         |
|  [03]   | `CentralPackageFloatingVersionsEnabled`  | `false`   | `true` permits `1.*`, and restore then depends on the feed state       |
|  [04]   | `ManagePackageVersionsCentrally=false`   |           | In a nested `Directory.Packages.props`, removes a tree from CPM        |

Transitive pinning is right when a lower transitive version is a defect `Directory.Packages.props` must correct, because every pinned package then restores at the `PackageVersion` version and a pin below a dependency's own floor fails restore. `PackageVersion Update` in a nested file changes one version for one tree.

| [INDEX] | [METADATA]             | [EFFECT]                                                                                 |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `VersionOverride`      | The project restores another version, the `PackageVersion` item serves every other one   |
|  [02]   | `PrivateAssets`        | Assets consumed here and withheld from consumers, default `contentfiles;analyzers;build` |
|  [03]   | `IncludeAssets`        | Assets the project consumes, default `all`                                               |
|  [04]   | `ExcludeAssets`        | Assets the project skips, default `none`                                                 |
|  [05]   | `GeneratePathProperty` | Defines `$(PkgSome_Package)` for `Some.Package` as the package directory                 |
|  [06]   | `Aliases`              | C# extern alias for the package assemblies when two packages share a namespace           |
|  [07]   | `NoWarn`               | Suppresses a restore code for the one reference                                          |
|  [08]   | `Condition`            | Adds the reference under a property, `'$(ProjectRole)' == 'tests'`                       |

| [INDEX] | [ASSET_CLASS]         | [PACKAGE_FOLDER]             | [MEANING]                                                           |
| :-----: | :-------------------- | :--------------------------- | :------------------------------------------------------------------ |
|  [01]   | `compile`             | `ref/<tfm>/` or `lib/<tfm>/` | Assemblies the compiler references                                  |
|  [02]   | `runtime`             | `lib/` and `runtimes/`       | Assemblies copied to the output directory                           |
|  [03]   | `native`              | `runtimes/<rid>/native/`     | Native libraries copied to the output directory                     |
|  [04]   | `contentFiles`        | `contentFiles/`              | Files added to the consuming project as items                       |
|  [05]   | `build`               | `build/`                     | `.props` and `.targets` imported by the direct consumer             |
|  [06]   | `buildTransitive`     | `buildTransitive/`           | `.props` and `.targets` imported by every consumer down the graph   |
|  [07]   | `buildMultitargeting` | `buildMultitargeting/`       | `.props` and `.targets` imported by the outer multi-targeting build |
|  [08]   | `analyzers`           | `analyzers/`                 | Roslyn analyzers and source generators                              |

`Directory.Build.targets` evaluates after the project's own properties and `PackageReference` items, and a reference that depends on a project property belongs there:

```xml
<!-- Directory.Build.targets: ProjectRole derives from the project directory earlier in the same file -->
<ItemGroup>
  <PackageReference Include="Contoso.Testing.TrxReport" Condition="'$(ProjectRole)' == 'tests'" />
</ItemGroup>
```

| [INDEX] | [COMMAND]                                                     | [EFFECT]                                                              |
| :-----: | :------------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `dotnet package add <id> --project <csproj>`                  | Adds the reference, under CPM the version goes to `PackageVersion`    |
|  [02]   | `dotnet package add <id>@<version> --project <csproj>`        | The same with a pinned version, `--prerelease` accepts a prerelease   |
|  [03]   | `dotnet package list --project <csproj> --include-transitive` | Requested and resolved versions, `--outdated` compares with the feeds |
|  [04]   | `dotnet package remove <id> --project <csproj>`               | Removes the reference, the `PackageVersion` item stays                |
|  [05]   | `dotnet package search <term> --source <path or url>`         | Feed search, a folder source takes an absolute path only              |
|  [06]   | `dotnet package update`                                       | Updates references in a project or solution, stable versions only     |
|  [07]   | `dotnet add package`                                          | Accepted alias, `dotnet package add` is the current form              |

`dotnet dnx dotnet-outdated-tool --yes -- --upgrade --pre-release Always --no-restore <solution>` rewrites each `PackageVersion` item a solution project references to its newest release in place, prereleases included, and without `--no-restore` the tool runs `dotnet add package`, which reformats the whole file.

## [03]-[RESTORE]

Restore resolves every direct reference to its exact `PackageVersion` and every transitive package to the lowest version the graph accepts, or to its `PackageVersion` under transitive pinning. The resolved graph is a function of `Directory.Packages.props`, the project files, and the sources, and no NuGet lock file or lock-file restore setting exists.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="local" value=".artifacts/nuget" />
  </packageSources>
  <!-- Every id restores from one named source, the longest matching prefix wins and * is the default -->
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="local">
      <package pattern="Contoso.Native.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

- `<clear />` drops every source a user or machine `NuGet.config` adds, and `<disabledPackageSources><clear /></disabledPackageSources>` drops inherited disables
- Package source mapping pins each id to a source, CPM requires a mapping when two or more HTTP sources exist, and a folder source does not count
- Mapping is skipped for an id present in the global packages folder, and a repository that needs the pin sets `globalPackagesFolder` in the `<config>` section or `NUGET_PACKAGES` in the pipeline
- `globalPackagesFolder` applies to `PackageReference`, `repositoryPath` applies to `packages.config` only, and `NUGET_PACKAGES` overrides both
- `RestoreSources` replaces the configured sources for one restore, `RestoreAdditionalProjectSources` adds to them, and `RestoreIgnoreFailedSources` turns an unreachable source into a warning
- `RestoreUseStaticGraphEvaluation` in `Directory.Build.props` applies to a project restore only, and a solution restore reads it from `-p:` or from `Directory.Solution.props`
- `NuGetAudit=false` skips the vulnerability fetch, because restore contacts the audit sources only while auditing is enabled
- `RestoreEnablePackagePruning` is on for `net10.0`, and restore then drops the packages the framework supplies from the graph

| [INDEX] | [COMMAND]                                | [EFFECT]                                                                      |
| :-----: | :--------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `dotnet restore --force`                 | Resolves again as if `project.assets.json` were deleted, keeps the HTTP cache |
|  [02]   | `dotnet restore --runtime <rid>`         | Restores the runtime-specific assets a publish for that RID needs             |
|  [03]   | `dotnet restore -p:Name=Value`           | Global property for the restore evaluation                                    |
|  [04]   | `dotnet nuget locals all --list`         | Paths of `global-packages`, `http-cache`, `temp`, and `plugins-cache`         |
|  [05]   | `dotnet nuget locals http-cache --clear` | Drops the 30 minute feed cache after a package republish                      |

## [04]-[PACKAGE_AUTHORING]

`dotnet pack` reads every value from the project: a package project sets `Version`, `Description`, and `PackageLicenseExpression`, and the `Directory.Build.props` of the packaging directory owns the shared layout. `IsPackable=false` in the root props keeps every other project out of `dotnet pack`.

```xml
<!-- packaging/Directory.Build.props: asset-only packages, the tree stops root inheritance -->
<Project>
  <PropertyGroup>
    <RepositoryRoot>$([MSBuild]::NormalizeDirectory('$(MSBuildThisFileDirectory)', '..'))</RepositoryRoot>
    <UseArtifactsOutput>true</UseArtifactsOutput>
    <ArtifactsPath>$([MSBuild]::NormalizePath('$(RepositoryRoot)', '.artifacts', 'packaging'))</ArtifactsPath>
    <PackageOutputPath>$([MSBuild]::NormalizeDirectory('$(RepositoryRoot)', '.artifacts', 'nuget'))</PackageOutputPath>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <EnableDefaultItems>false</EnableDefaultItems>
    <StageRoot>$([MSBuild]::NormalizeDirectory('$(RepositoryRoot)', '.artifacts', 'stage', '$(MSBuildProjectName)'))</StageRoot>
    <!-- Unix seconds or RFC 3339, every zip entry takes the time and the nupkg bytes repeat -->
    <DeterministicTimestamp>1735689600</DeterministicTimestamp>
  </PropertyGroup>
  <ItemGroup>
    <None Include="$(StageRoot)runtimes/**" Pack="true" PackagePath="runtimes/" />
    <None Include="$(StageRoot)contentFiles/**" Pack="true" PackagePath="contentFiles/any/any/" PackageCopyToOutput="true" />
    <None Include="$(MSBuildProjectDirectory)/buildTransitive/**" Pack="true" PackagePath="buildTransitive/" />
    <None Include="$(MSBuildThisFileDirectory)_._" Pack="true" PackagePath="lib/$(TargetFramework)/" />
  </ItemGroup>
</Project>
```

```xml
<!-- packaging/Contoso.Native.Item/Contoso.Native.Item.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Version>1.0.0</Version>
    <Description>Item shared library staged per runtime identifier</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

- `lib/<tfm>/_._` marks the framework the package supports, the pack targets emit the matching dependency group, and `SuppressDependenciesWhenPacking` stays off
- Native libraries go under `runtimes/<rid>/native/`, the SDK copies the matching RID directory and flattens it on publish, and `contentFiles` has no RID selection
- `contentFiles/any/any/` with `PackageCopyToOutput="true"` writes `copyToOutput="true"` to the nuspec, for a data file the runtime opens by path
- `build/<PackageId>.props` and `.targets` reach the direct consumer, `buildTransitive/` reaches every consumer down the graph, and a file that sets a property sets it under a condition the consumer can override
- `PackagePath` names the folder in the package, `Pack="true"` on `None` includes the item, and `Pack="false"` on `Content` excludes it
- `DeterministicTimestamp` defaults to `SOURCE_DATE_EPOCH` when that variable is set and to the wall clock otherwise

| [INDEX] | [PROPERTY]                                      | [EFFECT]                                                                          |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `PackageId`, `Version`                          | Id defaults to `AssemblyName`, prefix and suffix properties compose `Version`     |
|  [02]   | `PackageOutputPath`                             | Directory of the `.nupkg`, defaults to `<ArtifactsPath>/package/<configuration>/` |
|  [03]   | `IncludeBuildOutput=false`                      | No assembly in `lib/`, for a package of assets only                               |
|  [04]   | `PackageReadmeFile`                             | Path inside the package of a Markdown file the project packs with `Pack="true"`   |
|  [05]   | `PackageLicenseExpression`                      | SPDX expression, `PackageLicenseFile` is the alternative for a packed file        |
|  [06]   | `IncludeSymbols`, `SymbolPackageFormat`         | `snupkg` writes the portable PDBs beside the `.nupkg`                             |
|  [07]   | `PublishRepositoryUrl`, `EmbedUntrackedSources` | SourceLink writes the repository URL and embeds generated sources                 |
|  [08]   | `PackAsTool`, `ToolCommandName`                 | Packs an executable as a `dotnet tool`, the SDK imports the tool pack targets     |
|  [09]   | `DevelopmentDependency`                         | Build-time dependency, consumers exclude its compile assets                       |
|  [10]   | `PackageType`                                   | Semicolon list of package types, `Dependency` is the default                      |
|  [11]   | `NoPackageAnalysis`                             | Skips the `NU5xxx` analysis, right only for a layout the rules cannot describe    |
|  [12]   | `NuspecFile`                                    | Packs a hand-written nuspec and ignores the project, for a non-SDK package only   |

`GenerateNuspec` runs after `Build` and after `_GetPackageFiles` collects the `Pack="true"` items, a validation target uses `BeforeTargets="GenerateNuspec"`, and a target that adds files sets `TargetsForTfmSpecificContentInPackage` and returns `TfmSpecificPackageFile` items with `PackagePath` metadata, or `TargetsForTfmSpecificBuildOutput` for files in `lib/`.

| [INDEX] | [COMMAND]                                         | [EFFECT]                                                              |
| :-----: | :------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `dotnet pack`                                     | Release is the default configuration of `pack`                        |
|  [02]   | `dotnet pack --no-build`                          | Packs the existing build output, implies `--no-restore`               |
|  [03]   | `dotnet pack -o <dir>`                            | Overrides `PackageOutputPath` for one run                             |
|  [04]   | `dotnet pack -p:PackageVersion=1.2.0`             | Version for one run, `--version-suffix` sets `VersionSuffix` alone    |
|  [05]   | `dotnet pack -p:DeterministicTimestamp=<seconds>` | Timestamp for one run, the first build's value reproduces the package |

## [05]-[SOLUTION_FILES]

`.slnx` is the current format and the default of `dotnet new sln`, the solution folders match the repository layout, and a project stays out of the solution when a task runner builds it on its own, as a packaging project does.

```xml
<Solution>
  <Folder Name="/apps/">
    <Project Path="apps/Tool/Tool.csproj" />
  </Folder>
  <Folder Name="/libs/">
    <Project Path="libs/Library/Library.csproj" />
  </Folder>
</Solution>
```

| [INDEX] | [COMMAND]                                                     | [EFFECT]                                                         |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `dotnet new sln -n Product`                                   | Empty `.slnx`, `--format sln` writes the old format              |
|  [02]   | `dotnet sln Product.slnx add <csproj> --solution-folder libs` | Adds it and its references under a folder, `--in-root` skips one |
|  [03]   | `dotnet sln Product.slnx remove <csproj>`                     | Removes the entry, an empty folder stays                         |
|  [04]   | `dotnet sln Product.slnx list`                                | Project paths                                                    |
|  [05]   | `dotnet sln Product.sln migrate`                              | Writes `Product.slnx` beside the `.sln`                          |

| [INDEX] | [ELEMENT]         | [PURPOSE]                                                                                  |
| :-----: | :---------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `Solution`        | Root, optional `Description` and `Version` attributes                                      |
|  [02]   | `Folder`          | Solution folder by `Name`, holds `Project` and `File` children                             |
|  [03]   | `Project`         | `Path` relative to the solution, optional `Type` and `DisplayName`, `BuildDependency` rows |
|  [04]   | `Configurations`  | Holds `BuildType` and `Platform` rows, absent when Debug, Release, and Any CPU apply       |
|  [05]   | `Build`, `Deploy` | Per-project `Solution` and `Project` configuration mapping under `Project`                 |
|  [06]   | `Properties`      | Named `Property` rows, `Scope` is `PostLoad`                                               |

- MSBuild builds a solution as a generated project that imports `Directory.Solution.props` and `Directory.Solution.targets` and never `Directory.Build.props`
- `before.<name>.sln.targets` and `after.<name>.sln.targets` beside the file run for a `.slnx` build, the `.slnx.targets` spellings do not run, and the file name keeps `.sln.targets`
- `.slnf` filters name a `.slnx` in `path`, and `dotnet build Filter.slnf` builds the listed projects and their references
- `dotnet build Product.slnx -graph` builds the project graph from references, and `dotnet build Library/Library.csproj` builds one project and its references
- `-getProperty` and `-pp` reject a solution argument and take one project

## [06]-[CI_BUILD_PROPERTIES]

Every CI property sits in one `PropertyGroup` in the root `Directory.Build.props` under a condition on a property the pipeline passes with `-p:CI=true` or exports as the `CI` environment variable, and the switches sit on the pipeline command lines.

```xml
<!-- Directory.Build.props -->
<PropertyGroup Label="Continuous integration" Condition="'$(CI)' == 'true'">
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <MSBuildTreatWarningsAsErrors>true</MSBuildTreatWarningsAsErrors>
</PropertyGroup>
```

- `ContinuousIntegrationBuild` turns on `DeterministicSourcePaths`, and `Deterministic` is the SDK default
- `TreatWarningsAsErrors` covers compiler and NuGet warnings, `MSBuildTreatWarningsAsErrors` covers warnings MSBuild tasks log, and `-warnaserror` on the command line covers both with `-warnnotaserror:<code>` as the exception
- `SatelliteResourceLanguages=en` keeps only the named satellite assemblies, `GenerateDocumentationFile=true` writes the XML file and turns on `CS1591`, and both belong to the unconditioned group
- `EnableWindowsTargeting` stays unset on a non-Windows runner, and a Windows target framework then fails with `NETSDK1100` instead of downloading packs
- `global.json` `rollForward: disable` pins the SDK on the runner, and `DOTNET_ROLL_FORWARD` governs the runtime an application host selects and not the SDK
- `-check` runs BuildCheck on the pipeline build, use `dotnet-msbuild-diagnostics` for the checks

```bash
dotnet restore Product.slnx -p:CI=true
dotnet build Product.slnx --no-restore -p:CI=true -warnaserror -tl:off -nodeReuse:false -bl:.artifacts/logs/build-{}
dotnet test --solution Product.slnx --no-build --report-trx --results-directory .artifacts/test-results
```

| [INDEX] | [SWITCH]                        | [EFFECT]                                                                               |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `--no-restore`, `--no-build`    | `test --no-build` implies `--no-restore`, restore once and build once per pipeline     |
|  [02]   | `-tl:off`                       | Console logger output a log file keeps, `auto` picks the terminal logger on a terminal |
|  [03]   | `-nologo`                       | Passed by `dotnet build`, needed on `dotnet msbuild`                                   |
|  [04]   | `-clp:Summary;ErrorsOnly`       | Console logger parameters, `-v:m` sets the verbosity                                   |
|  [05]   | `-m`, `-maxCpuCount`            | One node per processor, `dotnet build` passes it                                       |
|  [06]   | `-nodeReuse:false`              | Worker nodes exit with the build, an idle node otherwise lives 15 minutes              |
|  [07]   | `-p:UseSharedCompilation=false` | Compiles in process, the Roslyn server otherwise lives 10 minutes after the last build |
|  [08]   | `-bl:<dir>/build-{}`            | Binary log per invocation, kept as a failure artifact                                  |

| [INDEX] | [VARIABLE]                             | [EFFECT]                                                                  |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `DOTNET_NOLOGO=1`                      | No first-run banner                                                       |
|  [02]   | `DOTNET_CLI_TELEMETRY_OPTOUT=1`        | No telemetry                                                              |
|  [03]   | `DOTNET_GENERATE_ASPNET_CERTIFICATE=0` | No development certificate on first run                                   |
|  [04]   | `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`    | No `PATH` edit on first run                                               |
|  [05]   | `NUGET_PACKAGES=<runner cache dir>`    | Global packages folder the runner cache restores between jobs             |
|  [06]   | `DOTNET_CLI_HOME=<dir>`                | Location of first-run sentinels, workload data, and local tools           |
|  [07]   | `MSBUILDDISABLENODEREUSE=1`            | `-nodeReuse:false` for every MSBuild process, including one a tool starts |

`global.json` `test.runner: Microsoft.Testing.Platform` makes `dotnet test` run every test project as an MTP application and reject a VSTest project, `--report-trx` needs the `Microsoft.Testing.Extensions.TrxReport` package in each test project, `--project` and `--solution` exclude each other, and the exit code is `0` for success, `2` for a failed test, `8` for zero tests, `9` for fewer tests than `--minimum-expected-tests`, and `5` for an invalid command line.

- `dotnet test --no-build` runs `ComputeRunArguments` per test project and starts the app host
- `TestingPlatformCommandLineArguments` reaches the test run through `RunArguments`
- `TestingPlatformDotnetTestSupport` belongs to the VSTest mode, and the .NET 10 runner fails when a project sets it
- Extension options (`--report-trx`, `--coverage`, `--crashdump`) fail with exit code `5` in a project without the package that provides them
- coverlet.MTP names each report by a timestamp under `--results-directory`, and a target before `ComputeRunArguments` empties it
- MinVer runs one `dotnet` and three `git` processes per project per build and skips restore and design-time builds
- A role condition on the MinVer `PackageReference` spares the projects that never publish

## [07]-[ANTIPATTERNS]

Packaging smells and the form that replaces each:

| [INDEX] | [SMELL]                                                     | [CORRECT_FORM]                                                            |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `Reference` with a `HintPath` into `~/.nuget`               | `PackageReference` with `GeneratePathProperty` when a path is needed      |
|  [02]   | `PackageVersion` with `1.*`                                 | The exact version, `dotnet package update` moves it                       |
|  [03]   | `VersionOverride` in more than one project                  | One `PackageVersion` item, or a nested file with `PackageVersion Update`  |
|  [04]   | `PackageReference` with `Version` under CPM                 | `PackageVersion` in `Directory.Packages.props`                            |
|  [05]   | `NuGet.config` without `<clear />`                          | `<clear />` first, then the named sources and their mappings              |
|  [06]   | NuGet lock file or lock-file restore setting               | Deleted, exact central versions make restore repeat without a lock file   |
|  [07]   | `GeneratePackageOnBuild` in a library                       | `dotnet pack` from the pipeline, a build then writes no package           |
|  [08]   | Packed `build/` props setting a property unconditionally    | `Condition="'$(Name)' == ''"`, the consumer keeps its own value           |
|  [09]   | Native libraries under `contentFiles`                       | `runtimes/<rid>/native/`, the only layout with RID selection              |
|  [10]   | `PackageReference` to a framework-provided package          | Removed, the framework supplies the package                               |
|  [11]   | `SuppressDependenciesWhenPacking` with a `lib/<tfm>/` entry | The dependency group stays                                                |
