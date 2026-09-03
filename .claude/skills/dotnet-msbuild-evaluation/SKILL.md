---
name: dotnet-msbuild-evaluation
description: "Use when deciding which MSBuild file owns a property, item, condition, or import across Directory.Build.props, .targets, .csproj, or Directory.Packages.props, or why a condition never matched."
---

# [DOTNET_MSBUILD_EVALUATION]

Covers the evaluation phase: import order, conditions, properties, items, and the placement of each declaration in `.props`, `.targets`, `.csproj`, `Directory.Build.*`, and `Directory.Packages.props`.

[REFERENCES]:
- [01]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): Root, inner, and tests `Directory.Build.props` files, and settings centralized out of project files

## [01]-[EVALUATION_ORDER]

MSBuild evaluates imports and properties in one pass, in order of appearance, as if it expanded each import in place. The last assignment wins:
- For a `Microsoft.NET.Sdk` project: `Directory.Build.props` → NuGet package `.props` → `Directory.Packages.props` → .NET SDK `.props` → project file → NuGet package `.targets` → `Directory.Build.targets` → later .NET SDK `.targets`
- `Microsoft.Common.props` imports `Directory.Build.props` first and `NuGet.props` last, and `NuGet.props` imports `Directory.Packages.props`. `Microsoft.Common.targets` imports `Directory.Build.targets`. Files that read a property before its assignment read an empty value

```xml
<!-- File 1 (imported first) -->
<MyProp>value1</MyProp>                                <!-- set to value1 -->
<!-- File 2 (imported second) -->
<MyProp>value2</MyProp>                                <!-- overwritten to value2 -->
<!-- File 3 (imported third) -->
<MyProp Condition="'$(MyProp)' == ''">value3</MyProp>  <!-- not set: already value2 -->
```

[CRITICAL]: In a single-targeting project, a property condition on `$(TargetFramework)` in an early `.props` file does not match. The project file sets the property after the import. Multi-targeting inner builds receive `TargetFramework` as a global property, and the condition matches there. Callers can supply the same global property. Place `TargetFramework`-conditioned property groups in `.targets` files or the project file. `ItemGroup`, item, and `Target` conditions see the final value. Items and targets evaluate after all properties. The outer build of a multi-targeting project leaves `TargetFramework` empty, and an item condition on it is false there too.

MSBuild imports one file path at most once and reports `MSB4011` on a second attempt. Marker properties at the end of a `.props` file let its `.targets` file import it without that warning. That covers a consumer that imports the `.targets` alone:

```xml
<!-- At the end of MySDK.props -->
<PropertyGroup>
  <MySDKPropsImported>true</MySDKPropsImported>
</PropertyGroup>

<!-- At the top of MySDK.targets -->
<Import Project="MySDK.props" Condition="'$(MySDKPropsImported)' != 'true'" />
```

`Import` paths can contain a wildcard or a semicolon-separated list. MSBuild sorts wildcard matches with `StringComparer.OrdinalIgnoreCase` and imports a list in written order. The sort is ordinal. Number the files of a wildcard directory with a fixed-width prefix: `010-foundation.props` before `020-policy.props`, because `100-x` sorts before `20-y`.

## [02]-[CONDITIONS]

MSBuild parses a `Condition` attribute as one expression string. Write every condition by these rules:
- Quote both sides of `==` and `!=` with single quotes: `'$(Prop)' == ''`
- Inside a quoted `Condition` operand, property function arguments take backticks or no quotes: `` `$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))` ``. An inner `'` closes the operand and fails with `MSB4092`. Property values nest single quotes freely.
- `Exists(...)` and `HasTrailingSlash(...)` are condition functions, called without a class prefix: `Condition="!HasTrailingSlash('$(OutDir)')"`. `$([MSBuild]::HasTrailingSlash(...))` fails with `MSB4186`. No such property function exists.
- `And` binds tighter than `Or`. Mixed chains without parentheses fail with `MSB4130`. Put parentheses around each group.
- Group related properties under one `PropertyGroup` condition instead of repeating it per property

Compare target frameworks with `IsTargetFrameworkCompatible(target, candidate)`. It returns true when the candidate is compatible with the target, across identifier and version. Version comparison alone misses a different identifier. `GetTargetFrameworkIdentifier('net10.0')` yields `.NETCoreApp`, never `net`.

```xml
<!-- Directory.Build.targets: true when the target can consume a netstandard2.0 asset -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'netstandard2.0'))">
  <DefineConstants>$(DefineConstants);FEATURE_SPANS</DefineConstants>
  <UsePolyfills>true</UsePolyfills>
</PropertyGroup>

<!-- Directory.Build.targets: identifier only, when the version does not matter -->
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'">
  <IsAotCompatible>true</IsAotCompatible>
</PropertyGroup>

<!-- OS detection -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <DefineConstants>$(DefineConstants);WINDOWS_BUILD</DefineConstants>
</PropertyGroup>
```

## [03]-[PROPERTIES]

Set a property only when it is still empty. An earlier import then keeps its value. In `.props` the condition creates a default the project can override. In `.targets` it creates a fallback. Global properties from the command line win over every assignment. `TreatAsLocalProperty="Name;Other"` on the `<Project>` element makes those global properties local, and an assignment in that file overrides the command line. `Directory.Build.props` can have the attribute.

```xml
<PropertyGroup>
  <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
  <Platform Condition="'$(Platform)' == ''">AnyCPU</Platform>
  <BuildInParallel Condition="'$(BuildInParallel)' == ''">true</BuildInParallel>

  <!-- ValueOrDefault resolves a primary source with a fallback in one expression -->
  <ToolPath>$([MSBuild]::ValueOrDefault('$(MyToolPathOverride)', '$(MSBuildThisFileDirectory)tools/mytool'))</ToolPath>
</PropertyGroup>
```

Append to a list-valued property through its current value. An assignment that omits `$(PropertyName);` drops every prior entry:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
  <NoWarn>$(NoWarn);NU5131;IDE0005</NoWarn>
</PropertyGroup>
```

### [03.1]-[PATH_NORMALIZATION]

```xml
<PropertyGroup>
  <!-- Directory properties carry a trailing slash -->
  <OutDir>$([MSBuild]::EnsureTrailingSlash('$(OutDir)'))</OutDir>
  <TargetRefPath>$([MSBuild]::NormalizePath('$(TargetDir)', 'ref', '$(TargetFileName)'))</TargetRefPath>
  <TargetRefDir>$([MSBuild]::NormalizeDirectory('$(TargetDir)', 'ref'))</TargetRefDir>
</PropertyGroup>
```

| [INDEX] | [EXPRESSION]                                           | [PURPOSE]                                                   |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `$([MSBuild]::NormalizePath(...))`                     | Combined and normalized file path                           |
|  [02]   | `$([MSBuild]::NormalizeDirectory(...))`                | Combined and normalized path, with a trailing slash         |
|  [03]   | `$([MSBuild]::EnsureTrailingSlash(...))`               | The value with a trailing slash, an empty value stays empty |
|  [04]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(dir, file))` | Nearest directory at or above `dir` with `file`, or empty   |
|  [05]   | `$([MSBuild]::GetPathOfFileAbove(file, dir))`          | Full path of the nearest `file` at or above `dir`, or empty |
|  [06]   | `$(MSBuildThisFileDirectory)`                          | Directory of the current file, with a trailing slash        |

The two search functions take the same two values in opposite orders. Reversed calls return an empty string and no error.

## [04]-[ITEMS]

`Exclude` pairs only with `Include` in the same element. `Exclude` beside `Update` or `Remove` fails the build with `MSB4066`.

| [INDEX] | [OPERATION] | [PURPOSE]                         | [USE_WHEN]                           |
| :-----: | :---------- | :-------------------------------- | :----------------------------------- |
|  [01]   | `Include`   | Add items to the item type        | Create items with metadata           |
|  [02]   | `Remove`    | Remove items that match a pattern | Exclude files or clear an item type  |
|  [03]   | `Update`    | Set metadata on existing items    | Set metadata without a new `Include` |

```xml
<ItemGroup>
  <!-- Include with metadata -->
  <Compile Include="Generated/*.cs">
    <AutoGen>true</AutoGen>
  </Compile>

  <!-- Exclude: set subtraction on Include -->
  <Compile Include="**/*.cs" Exclude="Generated/**;Tests/**" />

  <!-- Remove specific items -->
  <Reference Remove="$(AdditionalExplicitAssemblyReferences)" />

  <!-- Set subtraction: prior minus current -->
  <_CleanOrphanFileWrites Include="@(_CleanPriorFileWrites)"
      Exclude="@(_CleanCurrentFileWrites)" />

  <!-- Clear an entire group -->
  <_Temporary Remove="@(_Temporary)" />

  <!-- Update metadata on existing items -->
  <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Conditions apply to the whole `ItemGroup` or to one item. Outside a target, an item `Condition` reads properties and `@(Item)` lists only. `%()` references there fail the build with `MSB4190` for built-in metadata or `MSB4191` for custom metadata. Only the transform `@(Item->'%(Meta)')` is legal outside a target. `Update` selects items only during evaluation. Inside a target, MSBuild ignores the `Update` value and writes the metadata on every item of that type. Select there with `Condition="'%(Identity)' == '...'"`.

```xml
<!-- Condition on the ItemGroup -->
<ItemGroup Condition="'$(NetCoreBuild)' == 'true'">
  <PackageReference Include="System.IO.Pipelines" />
</ItemGroup>

<!-- Condition on individual items -->
<ItemGroup>
  <PackageReference Include="System.IO.Pipelines"
      Condition="'$(NetCoreBuild)' == 'true'" />
</ItemGroup>
```

### [04.1]-[TRANSFORMS]

The transform `@(Item->'expression')` applies the expression to each item and returns a new item list:

```xml
<!-- Transform file paths to destinations -->
<Copy SourceFiles="@(IntermediateAssembly)"
    DestinationFiles="@(IntermediateAssembly->'$(OutDir)%(Filename)%(Extension)')"/>

<!-- Transform with separator for display -->
<Message Text="Files: @(Compile->'%(Filename)', ', ')" />
```

### [04.2]-[BATCHING]

When `%(Metadata)` appears in a target's `Inputs` or `Outputs`, MSBuild runs the target once per unique metadata value (target batching). When it appears in a task parameter or task `Condition`, MSBuild runs the task once per unique value (task batching). `%()` references in a target `Condition` fail the build with `MSB4116`.

- Unqualified `%(Metadata)` fails the build with `MSB4096` when any referenced item list contains one item without that metadata. Qualify it as `%(List.Metadata)`.
- Qualified `%(List.Metadata)` batches on that list alone. MSBuild passes every other referenced list whole into each batch.
- `%()` on two item types in one expression batches each item type separately. In each batch the other item type's metadata is empty, and no batch sees both values. Batch on one item type and pass the other value as a property.

```xml
<!-- Target-level batching: runs once per unique Culture value -->
<Target Name="GenerateSatelliteAssemblies"
    Inputs="$(MSBuildAllProjects);@(_SatelliteAssemblyResourceInputs)"
    Outputs="$(IntermediateOutputPath)%(Culture)/$(TargetName).resources.dll">
</Target>

<!-- BAD: batches on two item types -->
<Exec Command="process %(Source.Identity) with %(Config.Identity)" />

<!-- GOOD: batch on one item type -->
<Exec Command="process %(Source.Identity) with $(ConfigFile)" />
```

## [05]-[FILE_PLACEMENT]

| [INDEX] | [PROPS]                                       | [TARGETS]                                  |
| :-----: | :-------------------------------------------- | :----------------------------------------- |
|  [01]   | Property defaults that a project can override | Custom targets, including post-build steps |
|  [02]   | Common items                                  | Properties that read SDK-defined values    |

### [05.1]-[DIRECTORY_BUILD_PROPS]

- Put here: language settings, assembly and package metadata, warning settings, and code analysis settings
- Never put here: a project-specific `TargetFramework` or a project-specific `PackageReference`

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <Company>Contoso</Company>
    <Authors>Contoso Engineering</Authors>
  </PropertyGroup>
</Project>
```

Set `<UseArtifactsOutput>true</UseArtifactsOutput>` here. The SDK then puts `artifacts/` beside this file. Set `ArtifactsPath` only to move that folder, and either property alone enables the layout. Project files that set one of them without an earlier value fail with `NETSDK1199`. Set `ArtifactsProjectName` and `ArtifactsPivots` here too.

The layout is `artifacts/<type>/<project>/<pivot>`, where type is `bin`, `obj`, or `publish`. Packages go to `artifacts/package/<configuration>` without a project segment. The pivot is the lowercase configuration, then `_<tfm>` for a multi-targeting project, then `_<rid>` when `RuntimeIdentifier` is set: `debug`, `debug_net10.0`, `release_osx-arm64`. `ArtifactsProjectName` and `ArtifactsPivots` rename the two segments.

Detect test projects by project name. MSBuild sets the reserved property `$(MSBuildProjectName)` before this import:

```xml
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Tests')) OR $(MSBuildProjectName.EndsWith('.UnitTests'))">
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

### [05.2]-[DIRECTORY_BUILD_TARGETS]

`OutputPath`, `OutputType`, and `TargetFramework` have their final values here. `IsPackable` and `IsTestProject` get their SDK defaults after this file. Property conditions on them read only a value the project set.

```xml
<Project>
  <Target Name="ValidateProjectSettings" BeforeTargets="Build">
    <Error Text="Libraries must target net10.0"
           Condition="'$(OutputType)' == 'Library' AND '$(TargetFramework)' != 'net10.0'" />
  </Target>
</Project>
```

Conditions on a property that the SDK or the project file sets later:

```xml
<PropertyGroup Condition="'$(OutputType)' == 'Exe'">
  <SelfContained>false</SelfContained>
</PropertyGroup>

<PropertyGroup Condition="'$(OutputType)' == 'Library' AND '$(IsTestProject)' != 'true'">
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### [05.3]-[DIRECTORY_PACKAGES_PROPS]

Central Package Management declares every NuGet package version in `PackageVersion` items. `NuGet.props` imports the nearest `Directory.Packages.props` at or above the project directory and stops there. `DirectoryPackagesPropsPath` names another file, and `ImportDirectoryPackagesProps=false` disables the import. Nested files replace the root file and drop `ManagePackageVersionsCentrally`, and restore fails with `NU1015`. Import the outer file at the top of the inner one. See `references/multi-level-examples.md`.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <!-- Pins transitive versions too. Off by default -->
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="10.0.0" />
    <PackageVersion Include="xunit.v3" Version="4.0.0" />
  </ItemGroup>

  <!-- Override one version for one framework -->
  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
    <PackageVersion Update="Microsoft.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Every project gets the package -->
    <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
  </ItemGroup>
</Project>
```

| [INDEX] | [ESCAPE]                                     | [PLACE]                          | [EFFECT]                                   |
| :-----: | :------------------------------------------- | :------------------------------- | :----------------------------------------- |
|  [01]   | `VersionOverride` on a `PackageReference`    | Project file                     | One project takes a different version      |
|  [02]   | `CentralPackageVersionOverrideEnabled=false` | `Directory.Packages.props`       | `VersionOverride` then fails with `NU1013` |
|  [03]   | `ManagePackageVersionsCentrally=false`       | Project file                     | One project leaves CPM                     |
|  [04]   | `PackageVersion Update`                      | Inner `Directory.Packages.props` | One version overrides the imported parent  |

`Version` on a `PackageReference` fails restore with `NU1008`. `PackageReference` without a `PackageVersion` fails with `NU1010`. `GlobalPackageReference` adds no compile-time reference. It fits analyzers, source generators, and build extensions only.

### [05.4]-[DIRECTORY_BUILD_RSP]

Contains default MSBuild switches for command-line builds. Every `dotnet` command that runs MSBuild applies it, as does `msbuild`. Visual Studio does not.

- MSBuild searches upward from the directory of the project or solution argument and applies the first file it finds. Without an argument, the search starts in the current directory. Nested files do not merge.
- Command-line switches override response-file switches. `dotnet build` and `dotnet msbuild` pass `-maxcpucount` and `-verbosity` themselves, and `dotnet build` also passes `-consoleLoggerParameters`, `-nologo`, and `-restore`. Those lines have no effect.
- The file accepts MSBuild switches only. `dotnet` switches (`--configuration`) fail the build, and MSBuild names the response file. Write `-p:Configuration=Release` instead.
- Lines are comments only when `#` is the first non-blank character. One line can contain more than one switch.
- MSBuild expands `%NAME%` environment variables on every operating system, never `$NAME`. `%MSBuildThisFileDirectory%` expands to the folder of the response file, with a trailing slash.
- `-noAutoResponse` on the command line skips the file for one invocation. `-noAutoResponse` inside the file fails with `MSB1027`, and an unknown switch fails with `MSB1001` before the build starts.

```
# Every command-line build in this repository, including builds that another tool starts
-nodeReuse:false
-bl:%MSBuildThisFileDirectory%.artifacts/logs/build-{}
```

### [05.5]-[MULTILEVEL_DIRECTORY_BUILD_FILES]

MSBuild imports only the first `Directory.Build.props` (or `.targets`) it finds when it searches upward from the project directory. To merge levels, import the outer file at the top of the inner file. `Directory.Packages.props` follows the same rule. `Directory.Build.rsp` also follows it, and no import merges response files. Read `references/multi-level-examples.md` for full files.

Holding the outer path in a property keeps nested quotes out of the `Condition`. Without the guard, a file with nothing above it fails with `MSB4020`. The import path is empty.

```xml
<Project>
  <PropertyGroup>
    <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />

  <!-- Inner-level overrides go here -->
</Project>
```

`$(DirectoryBuildPropsPath)` and `$(DirectoryBuildTargetsPath)` name the file to import. `$(ImportDirectoryBuildProps)` and `$(ImportDirectoryBuildTargets)` default to `true`, and `false` disables the import. Set the `.props` properties before `Microsoft.Common.props` imports `Directory.Build.props`. In an SDK-style project, use a global property, an environment variable, or `Directory.Build.rsp`. `Directory.Build.props` or the project file can set the `.targets` pair.

`Microsoft.Common.props` imports `$(CustomBeforeDirectoryBuildProps)` and `$(CustomAfterDirectoryBuildProps)` around `Directory.Build.props`. `Microsoft.Common.targets` imports `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)` around `Directory.Build.targets`. These four imports test only for a non-empty value, and a missing file fails the build with `MSB4019`. Relative values resolve against the SDK directory of the common file. Give a rooted path. The outer build of a multi-targeting project imports `Directory.Build.targets` from `Microsoft.Common.CrossTargeting.targets`, which imports neither `.targets` extension point.

`CustomBeforeMicrosoftCommonTargets` and `CustomAfterMicrosoftCommonTargets` belong to machine and tool scope, and their default paths do not exist in a .NET SDK layout. `dotnet watch` and `dotnet user-secrets` pass `CustomAfterMicrosoftCommonTargets` as a global property, paired with `CustomAfterMicrosoftCommonCrossTargetingTargets`. That adds a targets file to a project they do not own. Import a repository file from `Directory.Build.props` instead.

### [05.6]-[PACKAGE_BUILD_FOLDER_IMPORTS]

NuGet imports the applicable `.props` and `.targets` files from each package under `build/`, `buildTransitive/`, or `buildMultiTargeting/`. When the folder has TFM subfolders, NuGet selects the nearest compatible TFM folder for the consuming project. That folder can differ from the consumer's own TFM. `buildTransitive/<tfm>/` files that forward to `build/$(TargetFramework)/` can then name a folder the package lacks, and the consumer fails with `MSB4019`. Derive the TFM segment from the file's own folder and forward through the sibling `build/` file:

```xml
<!-- buildTransitive/net8.0/MyPackage.props -->
<PropertyGroup>
  <_MyPackageTfm>$([System.IO.Path]::GetFileName($(MSBuildThisFileDirectory.TrimEnd('\/'))))</_MyPackageTfm>
</PropertyGroup>
<Import Project="$(MSBuildThisFileDirectory)../../build/$(_MyPackageTfm)/MyPackage.props" />
```

The forwarding import needs no `Exists()` guard. The package layout is the contract. Restore already guards its own generated import of the package file with `Exists()`. The pack step places a file at the `target` of a `.nuspec` `<file>` entry. It also places a file at the `PackagePath` of a `<None>` or `<Content>` item with `Pack="true"`. Before you flag such an import, read every `*.nuspec` in the project directory and its parent directory, and read every `PackagePath` in the `.csproj`. The import is broken only when that package layout lacks the target path.

Restore writes `<project>.nuget.g.props` and `<project>.nuget.g.targets` under `$(MSBuildProjectExtensionsPath)`, which defaults to `obj/`. Those two files contain the imports of the package `build/` files. `Microsoft.Common.props` and `Microsoft.Common.targets` collect them through a wildcard import that `ImportProjectExtensionProps` and `ImportProjectExtensionTargets` gate. Both properties default to `false` during restore and `true` in every other build. Restore evaluations in a binary log show neither file. Restore rewrites this directory. No repository file belongs there.

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM]                                | [CAUSE]                                          | [FIX]                               |
| :-----: | :--------------------------------------- | :----------------------------------------------- | :---------------------------------- |
|  [01]   | `Directory.Build.props` is not imported  | The case differs on a case-sensitive volume      | Match the case exactly              |
|  [02]   | `Directory.Build.props` value is ignored | The project file reassigns it after the import   | Set it in `Directory.Build.targets` |
|  [03]   | SDK property empty in `.props`           | The SDK sets it after the `.props` import        | Read it in `.targets`               |
|  [04]   | Restore fails with `NU1015`              | Nested `Directory.Packages.props` hides the root | Import the outer file first         |
|  [05]   | `.rsp` switch has no effect              | The command line already has that switch         | Remove the switch                   |

[DIAGNOSIS]: `dotnet msbuild -pp:output.xml MyProject.csproj` writes every import inline, with file boundaries marked, and shows where each property is assigned. `dotnet msbuild -getProperty:Name MyProject.csproj` prints the evaluated value without a build, and `-getItem:Name` does the same for items. Point every query at one project file. Solution arguments fail with `MSB1063`.
