---
name: dotnet-msbuild-packaging
description: "Use when a .csproj, Directory.Packages.props, NuGet.config, or .slnx changes, covering central versions, restore, packing, and NU codes."
---

# [DOTNET_MSBUILD_PACKAGING]

Package and project files of a repository, from project set to CI build properties.

[REFERENCES]:
- [01]-[NUGET_CODES](references/nuget-codes.md): `NU1xxx` restore and `NU5xxx` pack causes and corrections

## [01]-[PROJECT_SET]

Project files hold what differs from root `Directory.Build.props`: `Sdk` attribute, `PackageReference` and `ProjectReference` items, and properties that vary per project.

```xml
<!-- Library/Library.csproj -->
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
|  [03]   | `packages.config`                               | SDK projects restore `PackageReference` items                                  |
|  [04]   | `app.config` in a library                       | Binding redirects belong to the executable                                     |
|  [05]   | `Directory.Build.rsp` with `-p:` switches       | Global properties no project overrides, defaults go in `Directory.Build.props` |
|  [06]   | `Directory.Solution.props` with a project value | Imported by the solution build alone, no project reads the value               |
|  [07]   | Committed `.nupkg` files                        | `dotnet pack` writes them under `PackageOutputPath`, which `.gitignore` covers |
|  [08]   | `bin/` or `obj/` under the tree                 | `UseArtifactsOutput` moves both, a stray copy means a project set its own path |

`UseArtifactsOutput` or `ArtifactsPath` in root `Directory.Build.props` places every output under `<ArtifactsPath>/<type>/<project>/<pivot>/`:
- Type is `bin`, `obj`, `publish`, or `package`, `package/<configuration>/` omits the project name
- Restore writes `project.assets.json`, `*.nuget.g.props`, and `*.nuget.g.targets` under `obj/<project>/`
- `ArtifactsPath` takes `NormalizePath`, SDK composes `<ArtifactsPath>\bin\<project>\` itself and a `NormalizeDirectory` value doubles the separator
- `dotnet msbuild <project> -getProperty:OutputPath` shows the composed path

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <UseArtifactsOutput>true</UseArtifactsOutput>
  <ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', '.artifacts', 'dotnet'))</ArtifactsPath>
</PropertyGroup>
```

## [02]-[CENTRAL_PACKAGE_MANAGEMENT]

`Directory.Packages.props` owns every version in `PackageVersion` items, projects name packages without a version. `NuGet.props` imports the nearest `Directory.Packages.props` at or above the project directory, a nested file imports the outer one at its top.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Contoso.Logging.Abstractions" Version="10.0.11" />
  </ItemGroup>
  <ItemGroup>
    <GlobalPackageReference Include="Contoso.Analyzers" Version="3.0.203" />
  </ItemGroup>
</Project>
```

- `GlobalPackageReference` restores in every project with `PrivateAssets=all` and no compile asset, its `Version` sits on the element

| [INDEX] | [PROPERTY]                               | [DEFAULT] | [MEANING]                                                              |
| :-----: | :--------------------------------------- | :-------- | :--------------------------------------------------------------------- |
|  [01]   | `CentralPackageVersionOverrideEnabled`   | `true`    | `false` rejects `VersionOverride`, per `Directory.Packages.props` file |
|  [02]   | `CentralPackageTransitivePinningEnabled` | `false`   | Every `PackageVersion` pins that package when it is transitive         |
|  [03]   | `CentralPackageFloatingVersionsEnabled`  | `false`   | `true` permits `1.*`, restore then depends on the feed state           |
|  [04]   | `ManagePackageVersionsCentrally=false`   |           | In a nested `Directory.Packages.props`, removes a tree from CPM        |

Transitive pinning restores every transitive package with a `PackageVersion` item at that version, a pin below a dependency's floor fails restore with `NU1109`. `dotnet pack` promotes pinned transitive packages to explicit nuspec dependencies. `PackageVersion Update` in a nested file changes one version for one tree.

| [INDEX] | [METADATA]             | [EFFECT]                                                                                 |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `VersionOverride`      | Project restores another version, the `PackageVersion` item serves every other one       |
|  [02]   | `PrivateAssets`        | Assets consumed here and withheld from consumers, default `contentfiles;analyzers;build` |
|  [03]   | `IncludeAssets`        | Assets the project consumes, default `all`                                               |
|  [04]   | `ExcludeAssets`        | Assets the project skips, default `none`                                                 |
|  [05]   | `GeneratePathProperty` | Defines `$(PkgSome_Package)` for `Some.Package` as the package directory                 |
|  [06]   | `Aliases`              | C# extern alias for the package assemblies when two packages share a namespace           |
|  [07]   | `NoWarn`               | Suppresses a restore code for the one reference                                          |
|  [08]   | `Condition`            | Adds the reference under a condition, pack maps a `TargetFramework` condition alone      |

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

`Directory.Build.targets` evaluates after the project's own properties and `PackageReference` items, a reference depending on a project property belongs there:

```xml
<!-- Directory.Build.targets -->
<ItemGroup>
  <PackageReference Include="Contoso.Testing.TrxReport" Condition="'$(ProjectRole)' == 'tests'" />
</ItemGroup>
```

| [INDEX] | [COMMAND]                                                     | [EFFECT]                                                              |
| :-----: | :------------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `dotnet package add <id> --project <csproj>`                  | Adds the reference, under CPM the version goes to `PackageVersion`    |
|  [02]   | `dotnet package add <id>@<version> --project <csproj>`        | Same with a pinned version, `--prerelease` accepts a prerelease       |
|  [03]   | `dotnet package list --project <csproj> --include-transitive` | Requested and resolved versions, `--outdated` compares with the feeds |
|  [04]   | `dotnet package remove <id> --project <csproj>`               | Removes the reference, the `PackageVersion` item stays                |
|  [05]   | `dotnet package search <term> --source <url>`                 | Feed search, `--exact-match` lists every version of one id            |
|  [06]   | `dotnet package update [<id>@<version>] --project <csproj>`   | Newest version, `--vulnerable` narrows, no prerelease switch          |
|  [07]   | `dotnet add package`                                          | Verb-first spelling of `dotnet package add`                           |

`dotnet dnx dotnet-outdated-tool -- --upgrade --pre-release Always --no-restore <project>` moves every referenced row to its newest release, prereleases included:
- `dnx` downloads the tool package to the NuGet cache and runs it, arguments after `--` reach the tool
- dotnet-outdated restores the project and reads `project.assets.json`, a project that fails restore reports no row
- `--no-restore` makes the tool write each `PackageVersion` row under CPM itself, without it `dotnet add package` runs per row with a restore each
- Rows with an exact range (`[x.y.z]`) stay, the candidate satisfies the row's range
- Candidate versions have a dependency group the project's target framework accepts, a release for a later framework alone moves no row

Catalog projects reference every central row, the tool then reads rows no other project references:

```xml
<!-- Catalog/Catalog.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="@(PackageVersion->ClearMetadata())" Exclude="@(PackageReference)" />
  </ItemGroup>
</Project>
```

- `ClearMetadata` drops `Version`, a `PackageReference` with `Version` under CPM fails `NU1008`
- `Exclude` skips the rows root `Directory.Build.props` already references, a duplicate reports `NU1504`
- `RestoreEnablePackagePruning=false` keeps a framework-supplied row from `NU1510`
- `IsTestingPlatformApplication=false` keeps `Microsoft.Testing.Platform.MSBuild` from treating a project with test packages as a test application

## [03]-[RESTORE]

Restore resolves every direct reference to its exact `PackageVersion` and every transitive package to the lowest version the graph accepts, or to its `PackageVersion` under transitive pinning. Resolved graph is a function of `Directory.Packages.props`, the project files, and the sources.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="contoso" value="https://pkgs.contoso.example/nuget/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="contoso">
      <package pattern="Contoso.*" />
    </packageSource>
  </packageSourceMapping>
  <config>
    <add key="globalPackagesFolder" value="packages" />
  </config>
</configuration>
```

- `<clear />` drops every source a user or machine `NuGet.config` adds
- `<disabledPackageSources><clear /></disabledPackageSources>` drops inherited disables
- `packageSourceMapping` routes every id, transitive ones included, to the source whose pattern matches
- Exact id patterns beat a prefix, a longer prefix beats a shorter one, `*` is the default
- CPM reports `NU1507` with more than one HTTP source and no mapping
- Mapping is skipped for an id present in the global packages folder
- `globalPackagesFolder` in the `<config>` section or `NUGET_PACKAGES` in the pipeline gives a repository its own folder
- `globalPackagesFolder` applies to `PackageReference`, `repositoryPath` applies to `packages.config`, `NUGET_PACKAGES` overrides both
- `RestoreSources` replaces the configured sources for one restore, `RestoreAdditionalProjectSources` adds to them
- `RestoreIgnoreFailedSources` turns an unreachable source into a warning
- `RestoreUseStaticGraphEvaluation` in `Directory.Build.props` applies to a project restore
- Solution restores read `RestoreUseStaticGraphEvaluation` from `-p:` or from `Directory.Solution.props`
- `NuGetAudit=false` skips the vulnerability fetch
- `RestoreEnablePackagePruning` is on for every framework of a project targeting `net10.0` or later
- Pruning drops framework-supplied packages from the graph
- Pruned direct references get `PrivateAssets=all` and `IncludeAssets=none`, `NU1510` asks for removal when every framework prunes the reference

| [INDEX] | [COMMAND]                                | [EFFECT]                                                                      |
| :-----: | :--------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `dotnet restore --force`                 | Resolves again as if `project.assets.json` were deleted, keeps the HTTP cache |
|  [02]   | `dotnet restore --runtime <rid>`         | Restores the runtime-specific assets a publish for that RID needs             |
|  [03]   | `dotnet restore -p:Name=Value`           | Global property for the restore evaluation                                    |
|  [04]   | `dotnet nuget locals all --list`         | Paths of `global-packages`, `http-cache`, `temp`, and `plugins-cache`         |
|  [05]   | `dotnet nuget locals http-cache --clear` | Drops the 30 minute feed cache after a package republish                      |

## [04]-[PACKAGE_AUTHORING]

`dotnet pack` reads every value from the project, a package project sets `Version`, `Description`, and `PackageLicenseExpression`, the `Directory.Build.props` of a packaging directory owns the shared layout. `IsPackable=false` in the root props keeps every other project out of `dotnet pack`.

```xml
<!-- packaging/Directory.Build.props -->
<Project>
  <PropertyGroup>
    <RepositoryRoot>$([MSBuild]::NormalizeDirectory('$(MSBuildThisFileDirectory)', '..'))</RepositoryRoot>
    <UseArtifactsOutput>true</UseArtifactsOutput>
    <ArtifactsPath>$([MSBuild]::NormalizePath('$(RepositoryRoot)', '.artifacts', 'packaging'))</ArtifactsPath>
    <PackageOutputPath>$([MSBuild]::NormalizeDirectory('$(RepositoryRoot)', '.artifacts', 'nuget'))</PackageOutputPath>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <EnableDefaultItems>false</EnableDefaultItems>
    <DeterministicTimestamp>1735689600</DeterministicTimestamp>
  </PropertyGroup>
  <ItemGroup>
    <None Include="$(MSBuildProjectDirectory)/runtimes/**" Pack="true" PackagePath="runtimes/" />
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
    <Description>Item shared library per runtime identifier</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

- Nested `Directory.Build.props` files without the root import stop root inheritance for the tree below
- `lib/<tfm>/_._` marks the framework the package supports, the pack targets emit the matching dependency group
- Native libraries go under `runtimes/<rid>/native/`, the SDK copies the matching RID directory and flattens it on publish
- Managed assemblies per RID go under `runtimes/<rid>/lib/<tfm>/` with an AnyCPU compile assembly under `ref/<tfm>/`
- NuGet takes compile assets from `ref/` over `lib/` and runtime assets from `runtimes/` over `lib/`
- `contentFiles/any/any/` with `PackageCopyToOutput="true"` writes `copyToOutput="true"` to the nuspec, for a data file the runtime opens by path
- `build/<PackageId>.props` and `.targets` reach the direct consumer, `buildTransitive/` reaches every consumer down the graph
- Packed `.props` files set properties under a condition the consumer can override
- `PackagePath` names the folder in the package, `Pack="true"` on `None` includes the item, `Pack="false"` on `Content` excludes it
- `Deterministic` gives every zip entry the `DeterministicTimestamp` time, RFC 3339 or Unix seconds
- `SOURCE_DATE_EPOCH` fills `DeterministicTimestamp` when unset, the wall clock otherwise

| [INDEX] | [PROPERTY]                                      | [EFFECT]                                                                          |
| :-----: | :---------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `PackageId`, `Version`                          | Id defaults to `AssemblyName`, prefix and suffix properties compose `Version`     |
|  [02]   | `PackageOutputPath`                             | Directory of the `.nupkg` files                                                   |
|  [03]   | `IncludeBuildOutput=false`                      | No assembly in `lib/`, for a package of assets                                    |
|  [04]   | `PackageReadmeFile`                             | Path inside the package of a Markdown file the project packs with `Pack="true"`   |
|  [05]   | `PackageLicenseExpression`                      | SPDX expression, `PackageLicenseFile` is the alternative for a packed file        |
|  [06]   | `IncludeSymbols`, `SymbolPackageFormat`         | `snupkg` writes the portable PDBs beside the `.nupkg`                             |
|  [07]   | `PublishRepositoryUrl`, `EmbedUntrackedSources` | SourceLink writes the repository URL and embeds generated sources                 |
|  [08]   | `PackAsTool`, `ToolCommandName`                 | Packs an executable as a `dotnet tool`, the SDK imports the tool pack targets     |
|  [09]   | `DevelopmentDependency`                         | Build-time dependency, consumers exclude its compile assets                       |
|  [10]   | `PackageType`                                   | Semicolon list of package types, `Dependency` is the default                      |
|  [11]   | `NoPackageAnalysis`                             | Skips the `NU5xxx` analysis, for a layout the rules cannot describe               |
|  [12]   | `NuspecFile`                                    | Packs a hand-written nuspec and ignores the project, for a non-SDK package        |

`GenerateNuspec` runs after `Build` and after `_GetPackageFiles` collects the `Pack="true"` items, a validation target takes `BeforeTargets="GenerateNuspec"`, a target that adds files sets `TargetsForTfmSpecificContentInPackage` and returns `TfmSpecificPackageFile` items with `PackagePath` metadata, or `TargetsForTfmSpecificBuildOutput` for files in `lib/`.

| [INDEX] | [COMMAND]                                         | [EFFECT]                                                              |
| :-----: | :------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `dotnet pack`                                     | Release is the default configuration of `pack`                        |
|  [02]   | `dotnet pack --no-build`                          | Packs the existing build output, implies `--no-restore`               |
|  [03]   | `dotnet pack -o <dir>`                            | Overrides `PackageOutputPath` for one run                             |
|  [04]   | `dotnet pack -p:PackageVersion=1.2.0`             | Version for one run, `--version-suffix` sets `VersionSuffix` alone    |
|  [05]   | `dotnet pack -p:DeterministicTimestamp=<seconds>` | Timestamp for one run, the first build's value reproduces the package |
|  [06]   | `dotnet pack -getItem:NuGetPackOutput`            | Prints the package paths as JSON                                      |

## [05]-[SOLUTION_FILES]

`.slnx` is the default of `dotnet new sln`, solution folders match the repository layout, a project a task runner builds on its own stays out of the solution.

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
|  [03]   | `dotnet sln Product.slnx remove <csproj>`                     | Removes the entry, a project name without extension works too    |
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

- MSBuild builds a solution as a generated project importing `Directory.Solution.props` and `Directory.Solution.targets`
- Generated solution project skips `Directory.Build.props`
- `.slnx` builds import `before.<name>.sln.targets` and `after.<name>.sln.targets` beside the file, the name keeps `.sln` for both formats
- `.slnf` filters name a `.slnx` in `path`, `dotnet build Filter.slnf` builds the listed projects and their references
- `dotnet build Library/Library.csproj` builds one project and its references

## [06]-[CI_BUILD_PROPERTIES]

Every CI property sits in one `PropertyGroup` in root `Directory.Build.props` under a condition on a property the pipeline passes with `-p:CI=true` or exports as the `CI` environment variable, switches sit on the pipeline command lines.

```xml
<!-- Directory.Build.props -->
<PropertyGroup Label="Continuous integration" Condition="'$(CI)' == 'true'">
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <MSBuildTreatWarningsAsErrors>true</MSBuildTreatWarningsAsErrors>
</PropertyGroup>
```

- `ContinuousIntegrationBuild` turns on `DeterministicSourcePaths`
- `TreatWarningsAsErrors` covers compiler and NuGet warnings, `MSBuildTreatWarningsAsErrors` covers warnings MSBuild tasks and BuildCheck log
- `SatelliteResourceLanguages=en` keeps the named satellite assemblies, `GenerateDocumentationFile=true` writes the XML file and turns on `CS1591`
- Satellite and documentation properties belong to the unconditioned group
- `EnableWindowsTargeting` stays unset on a non-Windows runner, a Windows target framework then fails `NETSDK1100`
- `global.json` `rollForward: disable` pins the SDK on the runner, `DOTNET_ROLL_FORWARD` governs the runtime an application host selects
- Use `dotnet-msbuild-diagnostics` for BuildCheck on the pipeline build

```bash
dotnet restore Product.slnx -p:CI=true
dotnet build Product.slnx --no-restore -p:CI=true -warnaserror -nodeReuse:false -bl:.artifacts/logs/build-{}.binlog
dotnet test --solution Product.slnx --no-build --report-trx --results-directory .artifacts/test-results
```

| [INDEX] | [SWITCH]                        | [EFFECT]                                                                               |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `--no-restore`, `--no-build`    | `test --no-build` implies `--no-restore`, restore once and build once per pipeline     |
|  [02]   | `-clp:Summary;ErrorsOnly`       | Console logger parameters, `-v:m` sets the verbosity                                   |
|  [03]   | `-m`, `-maxCpuCount`            | One node per processor, `dotnet build` passes it                                       |
|  [04]   | `-nodeReuse:false`              | Worker nodes exit with the build, an idle node otherwise stays for the next build      |
|  [05]   | `-p:UseSharedCompilation=false` | Compiles in process, the Roslyn server otherwise waits 10 minutes after the last build |

| [INDEX] | [VARIABLE]                             | [EFFECT]                                                                  |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `DOTNET_NOLOGO=1`                      | No first-run banner                                                       |
|  [02]   | `DOTNET_CLI_TELEMETRY_OPTOUT=1`        | No telemetry                                                              |
|  [03]   | `DOTNET_GENERATE_ASPNET_CERTIFICATE=0` | No development certificate on first run                                   |
|  [04]   | `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`    | No `PATH` edit on first run                                               |
|  [05]   | `NUGET_PACKAGES=<runner cache dir>`    | Global packages folder the runner cache restores between jobs             |
|  [06]   | `DOTNET_CLI_HOME=<dir>`                | Location of first-run sentinels, workload data, and local tools           |
|  [07]   | `MSBUILDDISABLENODEREUSE=1`            | `-nodeReuse:false` for every MSBuild process, one a tool starts included  |

`global.json` `test.runner: Microsoft.Testing.Platform` makes `dotnet test` run every test project as an MTP application and reject a VSTest project, `--report-trx` needs the `Microsoft.Testing.Extensions.TrxReport` package in each test project, `--project` and `--solution` exclude each other, the exit code is `0` for success, `2` for a failed test, `8` for zero tests, `9` for fewer tests than `--minimum-expected-tests`, and `5` for an invalid command line.

- `dotnet test --no-build` runs `ComputeRunArguments` per test project and starts the app host
- `TestingPlatformCommandLineArguments` reaches the test run through `RunArguments`
- `TestingPlatformDotnetTestSupport` belongs to the VSTest mode, the .NET 10 runner fails when a project sets it
- Extension options (`--report-trx`, `--coverage`, `--crashdump`) fail with exit code `5` in a project without the providing package
- coverlet.MTP names each report by a timestamp under `--results-directory`

## [07]-[ANTIPATTERNS]

| [INDEX] | [SMELL]                                                     | [CORRECT_FORM]                                                            |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `Reference` with a `HintPath` into `~/.nuget`               | `PackageReference` with `GeneratePathProperty` when a path is needed      |
|  [02]   | `PackageVersion` with `1.*`                                 | Exact version, `dotnet-outdated` moves it                                 |
|  [03]   | `VersionOverride` in more than one project                  | One `PackageVersion` item, or a nested file with `PackageVersion Update`  |
|  [04]   | `PackageReference` with `Version` under CPM                 | `PackageVersion` in `Directory.Packages.props`                            |
|  [05]   | `NuGet.config` without `<clear />`                          | `<clear />` first, then the named sources and their mappings              |
|  [06]   | NuGet lock file or lock-file restore setting                | Deleted, exact central versions make restore repeat without a lock file   |
|  [07]   | `GeneratePackageOnBuild` in a library                       | `dotnet pack` from the pipeline, a build then writes no package           |
|  [08]   | Packed `build/` props setting a property unconditionally    | `Condition="'$(Name)' == ''"`, the consumer keeps its own value           |
|  [09]   | Native libraries under `contentFiles`                       | `runtimes/<rid>/native/`, the one layout with RID selection               |
|  [10]   | `SuppressDependenciesWhenPacking` with a `lib/<tfm>/` entry | Dependency group stays                                                    |
