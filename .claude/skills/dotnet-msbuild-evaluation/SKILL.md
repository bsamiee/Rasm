---
name: dotnet-msbuild-evaluation
description: "Use when deciding which MSBuild file owns a property or item, where a property, item, or condition belongs across Directory.Build.props, .targets, .csproj, or Directory.Packages.props, or why a condition never matched."
---

# [DOTNET_MSBUILD_EVALUATION]

Owns everything MSBuild resolves before a target runs: import order, conditions, properties, items, and the placement of each declaration in `.props`, `.targets`, `.csproj`, `Directory.Build.*`, or `Directory.Packages.props`.

[REFERENCES]:
- [01]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): repo, `libs/`, `tests/` `Directory.Build.props`; centralization before/after

## [01]-[EVALUATION_ORDER]

MSBuild evaluates properties top-to-bottom across the whole import chain; the last assignment wins:
- `Directory.Build.props` → SDK `.props` → `YourProject.csproj` → SDK `.targets` → NuGet package `.targets` → `Directory.Build.targets`

```xml
<!-- File 1 (imported first) -->
<MyProp>value1</MyProp>                                <!-- set to value1 -->
<!-- File 2 (imported second) -->
<MyProp>value2</MyProp>                                <!-- overwritten to value2 -->
<!-- File 3 (imported third) -->
<MyProp Condition="'$(MyProp)' == ''">value3</MyProp>  <!-- NOT set — already value2 -->
```

`Directory.Build.props` is imported early inside `Microsoft.Common.props`, so it sets defaults and reads only what the SDK has already defined; the project overrides any value set there. `Directory.Build.targets` is imported after the project body and after every NuGet package's `.targets`, so it is the last workspace-owned window that overrides package build logic; a property assigned there cannot be re-assigned by the project.

[CRITICAL]: Property conditions on `$(TargetFramework)` in `.props` files silently fail for single-targeting projects — the property is empty during `.props` evaluation and is set in the project body. Place `TargetFramework`-conditioned property groups in `.targets` files or the project file itself. `ItemGroup`, item, and `Target` conditions are not affected.

## [02]-[CONDITIONS]

The `Condition` attribute runs a separate parser from the property body; quoting rules differ between the two.

- Quote both sides of `==` and `!=` with single quotes: `'$(Prop)' == ''`.
- Nested function arguments in a `Condition` take backticks or no quotes, `` `$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))` ``; an inner `'` closes the operand and raises `MSB4092`. A property body nests single quotes freely.
- `Exists(...)` and `HasTrailingSlash(...)` are condition functions, called bare: `Condition="!HasTrailingSlash('$(OutDir)')"`. Spelling either as `$([MSBuild]::HasTrailingSlash(...))` raises `MSB4186`.
- Group related properties under one `PropertyGroup` condition instead of repeating it per property.

`IsTargetFrameworkCompatible` is the default TFM test: it spans identifier and version in one call, where a hand-parsed version misses frameworks that differ in identifier. `GetTargetFrameworkIdentifier` returns the moniker identifier — `net10.0` yields `.NETCoreApp`, never `net`.

```xml
<!-- Compatibility spans identifier and version -->
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

Set a property only when it is still empty, so every earlier import and the command line keep the override. In `.props` the guard creates an overridable default; in `.targets` it creates a fallback. An unguarded assignment overwrites every earlier value.

```xml
<PropertyGroup>
  <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
  <Platform Condition="'$(Platform)' == ''">AnyCPU</Platform>
  <BuildInParallel Condition="'$(BuildInParallel)' == ''">true</BuildInParallel>

  <!-- ValueOrDefault resolves a primary source with a fallback in one expression -->
  <ToolPath>$([MSBuild]::ValueOrDefault('$(MyToolPathOverride)', '$(MSBuildThisFileDirectory)tools/mytool'))</ToolPath>
</PropertyGroup>
```

List-valued properties append through the existing value; an assignment without `$(Self);` drops every prior entry:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
  <NoWarn>$(NoWarn);NU5131;IDE0005</NoWarn>
</PropertyGroup>
```

Mark a file imported so a `.targets` file can pull its `.props` sibling exactly once:

```xml
<!-- At the end of MySDK.props -->
<PropertyGroup>
  <MySDKPropsImported>true</MySDKPropsImported>
</PropertyGroup>

<!-- At the top of MySDK.targets -->
<Import Project="MySDK.props" Condition="'$(MySDKPropsImported)' != 'true'" />
```

### [03.1]-[PATH_NORMALIZATION]-[TRAILING_SLASHES]

```xml
<PropertyGroup>
  <!-- Directory properties carry a trailing slash -->
  <OutDir Condition="'$(OutDir)' != ''">$([MSBuild]::EnsureTrailingSlash('$(OutDir)'))</OutDir>

  <!-- Combine and normalize; the result is absolute whether or not the last segment is rooted -->
  <TargetRefPath>$([MSBuild]::NormalizePath('$(TargetDir)', 'ref', '$(TargetFileName)'))</TargetRefPath>
  <TargetRefDir>$([MSBuild]::NormalizeDirectory('$(TargetDir)', 'ref'))</TargetRefDir>
  <MSBuildProjectExtensionsPath>$([MSBuild]::NormalizeDirectory('$(MSBuildProjectDirectory)', '$(MSBuildProjectExtensionsPath)'))</MSBuildProjectExtensionsPath>
</PropertyGroup>
```

| [INDEX] | [FUNCTION]                                       | [PURPOSE]                                       |
| :-----: | :----------------------------------------------- | :---------------------------------------------- |
|  [01]   | `$([MSBuild]::NormalizePath(...))`               | Combine and normalize a file path               |
|  [02]   | `$([MSBuild]::NormalizeDirectory(...))`          | Combine, normalize, and force a trailing slash  |
|  [03]   | `$([MSBuild]::EnsureTrailingSlash(...))`         | Append a trailing slash; leaves empty untouched |
|  [04]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(...))` | Walk up directory tree                          |
|  [05]   | `$(MSBuildThisFileDirectory)`                    | Directory of current file, slash-terminated     |

## [04]-[ITEMS]

| [INDEX] | [OPERATION] | [PURPOSE]                         | [USE_WHEN]                                 |
| :-----: | :---------- | :-------------------------------- | :----------------------------------------- |
|  [01]   | `Include`   | Add new items to the group        | Creating items with identity + metadata    |
|  [02]   | `Remove`    | Remove items matching a pattern   | Excluding files or clearing a group        |
|  [03]   | `Update`    | Modify metadata on existing items | Adding/changing metadata without re-adding |

`Update` does not add items — it only modifies items already in the group. `Exclude` only works on `Include` — it cannot be used with `Update` or `Remove`.

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

Conditional inclusion, on the group or on the item:

```xml
<!-- Condition on ItemGroup — all or nothing -->
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

### [04.1]-[TRANSFORMS]-[ITEM_TO_EXPRESSION]

`@(Item->'expression')` creates a new item list by applying an expression to each item:

```xml
<!-- Transform file paths to destinations -->
<Copy SourceFiles="@(IntermediateAssembly)"
    DestinationFiles="@(IntermediateAssembly->'$(OutDir)%(Filename)%(Extension)')"/>

<!-- Transform with separator for display -->
<Message Text="Files: @(Compile->'%(Filename)', ', ')" />
```

### [04.2]-[BATCHING]-[METADATA]

When `%(Metadata)` appears in target attributes or task parameters, MSBuild batches execution per unique metadata value.

- `%(Metadata)` in `Condition` or `Outputs` → target batches per unique value.
- `%(Metadata)` in task parameters → task batches per unique value.
- `%()` from two different item groups in one expression creates an O(N×M) cross-product; reference one group via batching and the other via a property.

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

<!-- Per-item filtering with Condition -->
<ItemGroup>
  <_ResxOutput Include="@(EmbeddedResource->'%(OutputResource)')"
      Condition="'%(EmbeddedResource.WithCulture)' == 'false'" />
</ItemGroup>

<!-- BAD: Cross-product of @(Source) × @(Config) -->
<Exec Command="process %(Source.Identity) with %(Config.Identity)" />

<!-- GOOD: Reference one group via batching, the other via property -->
<Exec Command="process %(Source.Identity) with $(ConfigFile)" />
```

### [04.3]-[GENERATED_FILE_ITEMS]

- Every file created during a target must be added to `@(FileWrites)` for `dotnet clean` support.
- Write generated files to `$(IntermediateOutputPath)` (obj/), not the source directory. Source-tree generation pollutes version control and can cause duplicate compilation via globs.

See `dotnet-msbuild-execution` skill for the target that generates and registers the file.

## [05]-[FILE_PLACEMENT]

| [INDEX] | [USE_PROPS_FOR]                  | [USE_TARGETS_FOR]                             |
| :-----: | :------------------------------- | :-------------------------------------------- |
|  [01]   | Setting property defaults        | Custom build targets                          |
|  [02]   | Common item definitions          | Late-bound property overrides                 |
|  [03]   | Properties projects can override | Post-build steps                              |
|  [04]   | Assembly/package metadata        | Conditional logic on final values             |
|  [05]   | Analyzer PackageReferences       | Targets that depend on SDK-defined properties |

RULE: Properties and items go in `.props`. Custom targets and late-bound logic go in `.targets`.

### [05.1]-[DIRECTORY_BUILD_PROPS]

Do NOT put here: project-specific TFMs, project-specific PackageReferences, targets/build logic, or properties depending on SDK-defined values (not available during `.props` evaluation).

Set `<ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>` here to produce project-name-separated `bin/`, `obj/`, and `publish/` directories under a single `artifacts/` folder, avoiding bin/obj clashes by default.

Good candidates: language settings, assembly/package metadata, build warnings, code analysis, common analyzers.

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

Conditional settings by project type, detect test projects by naming convention in `Directory.Build.props`:

```xml
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Tests')) OR $(MSBuildProjectName.EndsWith('.UnitTests'))">
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

Use `Directory.Build.targets` for conditions on SDK-defined properties like `OutputType`:

```xml
<PropertyGroup Condition="'$(OutputType)' == 'Exe'">
  <SelfContained>false</SelfContained>
</PropertyGroup>

<PropertyGroup Condition="'$(OutputType)' == 'Library' AND '$(IsTestProject)' != 'true'">
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Post-build validation, validate that `Pack` produced the expected output:

```xml
<Target Name="ValidatePackageOutput" AfterTargets="Pack"
        Condition="'$(IsPackable)' == 'true'">
  <Error Text="Package was not created at $(PackageOutputPath)$(PackageId).$(PackageVersion).nupkg"
         Condition="!Exists('$(PackageOutputPath)$(PackageId).$(PackageVersion).nupkg')" />
</Target>
```

### [05.2]-[DIRECTORY_BUILD_TARGETS]

Good candidates: custom build targets, late-bound property overrides (values depending on SDK properties), post-build validation.

```xml
<Project>
  <Target Name="ValidateProjectSettings" BeforeTargets="Build">
    <Error Text="All libraries must target netstandard2.0 or higher"
           Condition="'$(OutputType)' == 'Library' AND '$(TargetFramework)' == 'net472'" />
  </Target>

  <PropertyGroup>
    <!-- DocumentationFile depends on OutputPath, which is set by the SDK -->
    <DocumentationFile Condition="'$(IsPackable)' == 'true'">$(OutputPath)$(AssemblyName).xml</DocumentationFile>
  </PropertyGroup>
</Project>
```

### [05.3]-[DIRECTORY_PACKAGES_PROPS]-[CENTRAL_PACKAGE_MANAGEMENT]

Central Package Management (CPM) provides a single source of truth for all NuGet package versions. Enable CPM in `Directory.Packages.props` at the repo root:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="xunit" Version="2.9.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <!-- GlobalPackageReference applies to ALL projects; analyzers belong here -->
    <GlobalPackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" />
    <GlobalPackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
  </ItemGroup>
</Project>
```

See [https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) for details.

### [05.4]-[DIRECTORY_BUILD_RSP]

Contains default MSBuild CLI arguments applied to all builds under the directory tree, one argument per line. Works with both `msbuild` and `dotnet` CLI in modern .NET versions; enforces consistent CI and local build flags.

```
/maxcpucount
/nodeReuse:false
/consoleLoggerParameters:Summary;ForceNoAlign
/warnAsMessage:MSB3277
```

### [05.5]-[MULTILEVEL_DIRECTORY_BUILD_FILES]

MSBuild only auto-imports the first `Directory.Build.props` (or `.targets`) it finds walking up from the project directory. To chain multiple levels, explicitly import the parent at the top of the inner file.

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
  Directory.Build.props          ← repo-wide (lang version, company info, analyzers)
  Directory.Build.targets        ← repo-wide targets
  Directory.Packages.props       ← central package versions
  libs/dotnet/
    Directory.Build.props        ← library-specific (imports repo-level, sets IsPackable=true)
  tests/dotnet/
    Directory.Build.props        ← test-specific (imports repo-level, sets IsPackable=false, adds test packages)
```

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM] | [CAUSE] | [FIX] |
| :-----: | :-------- | :------ | :---- |
| [01] | `Directory.Build.props` is not imported | Filename casing differs; Linux file systems are case-sensitive | Match the casing exactly |
| [02] | A `Directory.Build.props` property is ignored | The project body reassigns it after the import | Assign it in `Directory.Build.targets` |
| [03] | Outer `Directory.Build.props` is skipped | MSBuild stops at the first file found upward | Inner file imports it via `GetPathOfFileAbove` |
| [04] | SDK properties read empty in `.props` | `Microsoft.Common.props` imports it before the SDK defines them | Read them in `.targets` |
| [05] | `Directory.Packages.props` is not found | No file of that exact name in the project directory or any parent | Create it at the repo root |
| [06] | `$(TargetFramework)` condition never matches in `.props` | Single-targeting projects set it in the project body | Move it to `.targets` |

Diagnosis: `dotnet msbuild -pp:output.xml MyProject.csproj` expands all imports inline, showing exactly where each property is set and its final evaluated value.
