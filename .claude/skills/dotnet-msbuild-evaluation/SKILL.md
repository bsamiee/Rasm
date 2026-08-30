---
name: dotnet-msbuild-evaluation
description: "Use when deciding which MSBuild file owns a property, item, condition, or import across Directory.Build.props, .targets, .csproj, or Directory.Packages.props, or why a condition never matched."
---

# [DOTNET_MSBUILD_EVALUATION]

Covers the evaluation phase: import order, conditions, properties, items, and the placement of each declaration in `.props`, `.targets`, `.csproj`, `Directory.Build.*`, or `Directory.Packages.props`.

[REFERENCES]:
- [01]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): Inner, outer, and `tests/`, `Directory.Build.props` files, before/after of settings moved out of project files

## [01]-[EVALUATION_ORDER]

MSBuild evaluates imports and properties in one pass, in order of appearance, as if each import were expanded in place. The last assignment wins:
- `Directory.Build.props` → NuGet package `.props` → SDK `.props` → project file → SDK `.targets` → NuGet package `.targets` → `Directory.Build.targets`

```xml
<!-- File 1 (imported first) -->
<MyProp>value1</MyProp>                                <!-- set to value1 -->
<!-- File 2 (imported second) -->
<MyProp>value2</MyProp>                                <!-- overwritten to value2 -->
<!-- File 3 (imported third) -->
<MyProp Condition="'$(MyProp)' == ''">value3</MyProp>  <!-- not set: already value2 -->
```

`Directory.Build.props` is imported early in `Microsoft.Common.props`. It sets defaults, a property defined later evaluates to empty inside it, and the project file overrides any value it sets. `Directory.Build.targets` is imported from `Microsoft.Common.targets` after the project file and after every NuGet package `.targets` file. A property assigned there overrides the project file and package build logic.

[CRITICAL]: A property condition on `$(TargetFramework)` in a `.props` file never matches for a single-targeting project. The project file sets the property after the import, so it is empty during the import. Place `TargetFramework`-conditioned property groups in `.targets` files or the project file. `ItemGroup`, item, and `Target` conditions see the final value, because items and targets evaluate after all properties.

## [02]-[CONDITIONS]

A `Condition` attribute is parsed as one expression string, so its quoting rules differ from a property value.

- Quote both sides of `==` and `!=` with single quotes: `'$(Prop)' == ''`.
- Inside a quoted `Condition` operand, property function arguments take backticks or no quotes: `` `$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))` ``. An inner `'` closes the operand and raises `MSB4092`. A property value nests single quotes freely.
- `Exists(...)` and `HasTrailingSlash(...)` are condition functions, called without a class prefix: `Condition="!HasTrailingSlash('$(OutDir)')"`. `$([MSBuild]::HasTrailingSlash(...))` raises `MSB4186` because no such property function exists.
- Group related properties under one `PropertyGroup` condition instead of repeating it per property.

Compare target frameworks with `IsTargetFrameworkCompatible(target, candidate)`. It returns true when the candidate (second argument) is compatible with the target (first argument), across identifier and version. A parsed version number misses frameworks with a different identifier. `GetTargetFrameworkIdentifier` returns the `TargetFrameworkIdentifier`: `net10.0` yields `.NETCoreApp`, never `net`.

```xml
<!-- True when the project can consume a net472 asset: net472 and later .NET Framework -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'net472'))">
  <UseFrozenVersions>true</UseFrozenVersions>
</PropertyGroup>

<!-- Identifier only, when the version does not matter -->
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'">
  <NetCoreBuild>true</NetCoreBuild>
</PropertyGroup>

<!-- One condition covers the whole group -->
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETFramework'">
  <DefineConstants>$(DefineConstants);FEATURE_APARTMENT_STATE</DefineConstants>
  <DefineConstants>$(DefineConstants);FEATURE_APM</DefineConstants>
  <FeatureAppDomain>true</FeatureAppDomain>
</PropertyGroup>

<!-- OS detection -->
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <DefineConstants>$(DefineConstants);TEST_ISWINDOWS</DefineConstants>
</PropertyGroup>
```

## [03]-[PROPERTIES]

Set a property only when it is still empty, so an earlier import keeps its value. A global property from the command line is never overwritten by a project assignment. In `.props` the condition creates a default the project can override. In `.targets` it creates a fallback. An unconditioned assignment overwrites every earlier value.

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

Set a marker property at the end of a `.props` file so its `.targets` file imports it at most once:

```xml
<!-- At the end of MySDK.props -->
<PropertyGroup>
  <MySDKPropsImported>true</MySDKPropsImported>
</PropertyGroup>

<!-- At the top of MySDK.targets -->
<Import Project="MySDK.props" Condition="'$(MySDKPropsImported)' != 'true'" />
```

### [03.1]-[PATH_NORMALIZATION]

```xml
<PropertyGroup>
  <!-- Directory properties carry a trailing slash -->
  <OutDir Condition="'$(OutDir)' != ''">$([MSBuild]::EnsureTrailingSlash('$(OutDir)'))</OutDir>

  <!-- Combine and normalize to a full path -->
  <TargetRefPath>$([MSBuild]::NormalizePath('$(TargetDir)', 'ref', '$(TargetFileName)'))</TargetRefPath>
  <TargetRefDir>$([MSBuild]::NormalizeDirectory('$(TargetDir)', 'ref'))</TargetRefDir>
  <MSBuildProjectExtensionsPath>$([MSBuild]::NormalizeDirectory('$(MSBuildProjectDirectory)', '$(MSBuildProjectExtensionsPath)'))</MSBuildProjectExtensionsPath>
</PropertyGroup>
```

| [INDEX] | [FUNCTION]                                       | [PURPOSE]                                                      |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `$([MSBuild]::NormalizePath(...))`               | Combine and normalize a file path                              |
|  [02]   | `$([MSBuild]::NormalizeDirectory(...))`          | Combine, normalize, and force a trailing slash                 |
|  [03]   | `$([MSBuild]::EnsureTrailingSlash(...))`         | Append a trailing slash. An empty value stays empty            |
|  [04]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(...))` | Nearest directory at or above that contains the file, or empty |
|  [05]   | `$(MSBuildThisFileDirectory)`                    | Directory of the current file, with a trailing slash           |

## [04]-[ITEMS]

| [INDEX] | [OPERATION] | [PURPOSE]                         | [USE_WHEN]                          |
| :-----: | :---------- | :-------------------------------- | :---------------------------------- |
|  [01]   | `Include`   | Add items to the item type        | Create items with metadata          |
|  [02]   | `Remove`    | Remove items that match a pattern | Exclude files or clear an item type |
|  [03]   | `Update`    | Set metadata on existing items    | Metadata without a new `Include`    |

`Update` does not add items. `Exclude` applies only to the `Include` attribute in the same element, never to `Update` or `Remove`.

```xml
<ItemGroup>
  <!-- Include with metadata -->
  <Compile Include="Generated\*.cs">
    <AutoGen>true</AutoGen>
  </Compile>

  <!-- Exclude: set subtraction on Include -->
  <Compile Include="**\*.cs" Exclude="Generated\**;Tests\**" />

  <!-- Remove specific items -->
  <Reference Remove="$(AdditionalExplicitAssemblyReferences)" />

  <!-- Set subtraction: prior minus current -->
  <_CleanOrphanFileWrites Include="@(_CleanPriorFileWrites)"
      Exclude="@(_CleanCurrentFileWrites)" />

  <!-- Clear an entire group -->
  <_Temporary Remove="@(_Temporary)" />

  <!-- Update metadata on existing items -->
  <EmbeddedResource Update="@(EmbeddedResource)"
      Condition="'%(NuGetPackageId)' == 'Microsoft.CodeAnalysis.Collections'">
    <GenerateSource>true</GenerateSource>
    <ClassName>Microsoft.CodeAnalysis.Collections.SR</ClassName>
  </EmbeddedResource>
</ItemGroup>
```

A condition applies to the whole `ItemGroup` or to one item:

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

<!-- Tool and analyzer packages never flow to consumers -->
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" PrivateAssets="all" />
  <PackageReference Include="StyleCop.Analyzers" PrivateAssets="all" />
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

When `%(Metadata)` appears in a target's `Inputs`, `Outputs`, or `Condition`, MSBuild runs the target once per unique metadata value (target batching). When it appears in a task parameter or task `Condition`, MSBuild runs the task once per unique value (task batching).

- An item list with no item that carries the referenced metadata is passed whole into every batch.
- `%()` on two item types in one expression batches each item type separately. In each batch the other item type's metadata is empty, so no batch sees both values. Batch on one item type and pass the other value as a property.

```xml
<!-- Target-level batching: runs once per unique Culture value -->
<Target Name="GenerateSatelliteAssemblies"
    Inputs="$(MSBuildAllProjects);@(_SatelliteAssemblyResourceInputs)"
    Outputs="$(IntermediateOutputPath)%(Culture)\$(TargetName).resources.dll">
</Target>

<!-- Task-level batching -->
<Copy SourceFiles="@(_SourceItems)"
    DestinationFiles="@(_SourceItems->'$(OutDir)%(TargetPath)')">
</Copy>

<!-- Item condition on the item's own metadata (no batching outside a target) -->
<ItemGroup>
  <_ResxOutput Include="@(EmbeddedResource->'%(OutputResource)')"
      Condition="'%(EmbeddedResource.WithCulture)' == 'false'" />
</ItemGroup>

<!-- BAD: batches on two item types, each batch has one side empty -->
<Exec Command="process %(Source.Identity) with %(Config.Identity)" />

<!-- GOOD: batch on one item type, pass the other as a property -->
<Exec Command="process %(Source.Identity) with $(ConfigFile)" />
```

### [04.3]-[GENERATED_FILE_ITEMS]

- Add every file that a target writes to `@(FileWrites)`, so that `IncrementalClean` and `Clean` delete it.
- Write generated files under `$(IntermediateOutputPath)`, never the source directory. A generated file in the source tree lands in version control and in the default `Compile` glob.

The `dotnet-msbuild-execution` skill owns the target that writes the file and adds it to `@(FileWrites)`.

## [05]-[FILE_PLACEMENT]

| [INDEX] | [PROPS]                          | [TARGETS]                                     |
| :-----: | :------------------------------- | :-------------------------------------------- |
|  [01]   | Property defaults                | Custom targets                                |
|  [02]   | Common items                     | Properties that read SDK-defined values       |
|  [03]   | Properties projects can override | Post-build steps                              |
|  [04]   | Assembly and package metadata    | Conditions on final property values           |
|  [05]   | Shared `PackageReference` items  | Targets that depend on SDK-defined properties |

### [05.1]-[DIRECTORY_BUILD_PROPS]

Never put here: a project-specific `TargetFramework`, a project-specific `PackageReference`, a target, or a property that reads an SDK-defined value. SDK-defined values are empty during this import.

Set `<ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>` here, never in a project file. `UseArtifactsOutput` follows the same rule. The SDK then writes `artifacts/<type>/<project>/<pivot>`, where type is `bin`, `obj`, `publish`, or `package`. A project-file assignment raises `NETSDK1199`.

Put here: language settings, assembly and package metadata, warning settings, and code analysis settings.

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

Detect test projects by project name. `$(MSBuildProjectName)` is a reserved property and is set before this import:

```xml
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Tests')) OR $(MSBuildProjectName.EndsWith('.UnitTests'))">
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

### [05.2]-[DIRECTORY_BUILD_TARGETS]

Put here: custom targets, properties that read SDK-defined values, and validation targets.

```xml
<Project>
  <Target Name="ValidateProjectSettings" BeforeTargets="Build">
    <Error Text="Libraries must target net10.0"
           Condition="'$(OutputType)' == 'Library' AND '$(TargetFramework)' != 'net10.0'" />
  </Target>

  <PropertyGroup>
    <!-- OutputPath is computed in the SDK targets, so this assignment works only in .targets -->
    <DocumentationFile Condition="'$(IsPackable)' == 'true'">$(OutputPath)$(AssemblyName).xml</DocumentationFile>
  </PropertyGroup>
</Project>
```

Conditions on a property that the SDK or the project file sets after `Directory.Build.props`, such as `OutputType`:

```xml
<PropertyGroup Condition="'$(OutputType)' == 'Exe'">
  <SelfContained>false</SelfContained>
</PropertyGroup>

<PropertyGroup Condition="'$(OutputType)' == 'Library' AND '$(IsTestProject)' != 'true'">
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

A target that runs after `Pack` makes sure that the package exists:

```xml
<Target Name="ValidatePackageOutput" AfterTargets="Pack"
        Condition="'$(IsPackable)' == 'true'">
  <Error Text="Package was not created at $(PackageOutputPath)$(PackageId).$(PackageVersion).nupkg"
         Condition="!Exists('$(PackageOutputPath)$(PackageId).$(PackageVersion).nupkg')" />
</Target>
```

### [05.3]-[DIRECTORY_PACKAGES_PROPS]

Central Package Management holds every NuGet package version in `Directory.Packages.props` at the repository root. Enable it there:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="10.0.0" />
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="xunit" Version="2.9.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <!-- GlobalPackageReference adds the package to every project with PrivateAssets=all -->
    <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
  </ItemGroup>
</Project>
```

[REFERENCE]: https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management

### [05.4]-[DIRECTORY_BUILD_RSP]

Holds default command-line switches for every command-line build under its directory, one switch per line. `msbuild.exe` and `dotnet build` apply it. Visual Studio does not. Pass `-noAutoResponse` to skip it for one invocation.

```
-maxcpucount
-nodeReuse:false
-consoleLoggerParameters:Summary;ForceNoAlign
-warnAsMessage:MSB3277
```

### [05.5]-[MULTILEVEL_DIRECTORY_BUILD_FILES]

MSBuild imports only the first `Directory.Build.props` (or `.targets`) it finds when it searches upward from the project directory. To merge levels, import the outer file at the top of the inner file.

`$(DirectoryBuildPropsPath)` and `$(DirectoryBuildTargetsPath)` name the file to import. `$(ImportDirectoryBuildProps)` and `$(ImportDirectoryBuildTargets)` default to `true`, and `false` disables the import. `Microsoft.Common.props` reads the `.props` pair before any project content, so only a global property, an environment variable, or `Directory.Build.rsp` can set it. `Directory.Build.props` or the project file can set the `.targets` pair.

```xml
<Project>
  <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))"
         Condition="Exists('$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))')" />

  <!-- Inner-level overrides go here -->
</Project>
```

Example layout:

```
repo/
  Directory.Build.props          ← repo-wide (language settings, package metadata)
  Directory.Build.targets        ← repo-wide targets
  Directory.Packages.props       ← central package versions
  libs/dotnet/
    Directory.Build.props        ← library-specific (imports repo-level, sets IsPackable=true)
  tests/dotnet/
    Directory.Build.props        ← test-specific (imports repo-level, sets IsPackable=false, adds test packages)
```

### [05.6]-[PACKAGE_BUILD_FOLDER_IMPORTS]

NuGet imports one `.props` and one `.targets` file per package from `build/`, `buildTransitive/`, or `buildMultiTargeting/`. When the folder has TFM subfolders, NuGet selects the nearest compatible TFM folder for the consuming project, not the folder named by the consumer's own TFM. As a result, a `buildTransitive/<tfm>/` file that forwards to `build/$(TargetFramework)/` can name a folder that the package does not ship, and the consumer stops with `MSB4019`. Derive the TFM segment from the file's own folder and forward through the sibling `build/` file:

```xml
<!-- buildTransitive/net8.0/MyPackage.props -->
<PropertyGroup>
  <_MyPackageTfm>$([System.IO.Path]::GetFileName($(MSBuildThisFileDirectory.TrimEnd('\/'))))</_MyPackageTfm>
</PropertyGroup>
<Import Project="$(MSBuildThisFileDirectory)../../build/$(_MyPackageTfm)/MyPackage.props" />
```

The import needs no `Exists()` guard. The packed layout, not the source tree, is the contract. The pack step produces the per-TFM folders from a `.nuspec` `<file>` entry with a per-TFM `target`, or from `<None>` or `<Content>` items with a per-TFM `<PackagePath>`. Before you judge such an import, read every `*.nuspec` in the project directory and its parent directory, and read every `<PackagePath>` in the `.csproj`. The import is broken only when the target path is absent from both the source tree and that packed layout.

[REFERENCE]: https://learn.microsoft.com/en-us/nuget/concepts/msbuild-props-and-targets

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM]                                             | [CAUSE]                                             | [FIX]                                           |
| :-----: | :---------------------------------------------------- | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Directory.Build.props` is not imported               | Filename casing differs, Linux is case-sensitive    | Match the casing exactly                        |
|  [02]   | `Directory.Build.props` value is ignored              | The project file reassigns it after the import      | Set it in `Directory.Build.targets`             |
|  [03]   | Outer `Directory.Build.props` is skipped              | MSBuild imports the first file found upward         | Inner file imports it with `GetPathOfFileAbove` |
|  [04]   | An SDK-defined property is empty in `.props`          | The SDK sets it after the `.props` import           | Read it in `.targets`                           |
|  [05]   | `Directory.Packages.props` is not found               | No file of that name in project dir or a parent dir | Create it at the repo root                      |
|  [06]   | `$(TargetFramework)` condition never matches `.props` | Single-target projects set it in the project file   | Move it to `.targets`                           |

[DIAGNOSIS]: `dotnet msbuild -pp:output.xml MyProject.csproj` writes every import inline, with file boundaries marked, which shows where each property is assigned. `dotnet msbuild -getProperty:Name MyProject.csproj` prints the evaluated value without a build. `-getItem:Name` and `-getTargetResult:Name` do the same for items and target outputs.
