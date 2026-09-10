---
name: dotnet-msbuild-evaluation
description: "Use when writing or debugging a .props, .targets, or .csproj declaration, covering MSBuild evaluation order, conditions, properties, and items."
---

# [DOTNET_MSBUILD_EVALUATION]

Evaluation phase of a `Microsoft.NET.Sdk` project, from the import chain to the file that owns each declaration.

[REFERENCES]:
- [01]-[IMPORT_CHAIN](references/import-chain.md): Every import of a `Microsoft.NET.Sdk` project in order, with the properties each file assigns
- [02]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): Root and nested `Directory.Build.*` files with project files holding what differs

## [01]-[EVALUATION_ORDER]

MSBuild evaluates a project in passes, each pass reads what the earlier passes produced:

| [INDEX] | [PASS]                 | [READS]                                                                               |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Environment variables  | Every variable with a valid property name becomes a property                          |
|  [02]   | Imports and properties | Properties in order of appearance with imports expanded in place, the last value wins |
|  [03]   | Item definitions       | Properties, and metadata of the same item definition assigned earlier                 |
|  [04]   | Items                  | Final value of every property, and items assigned earlier                             |
|  [05]   | `UsingTask`            | Properties                                                                            |
|  [06]   | Targets                | Nothing runs, the execution phase evaluates each target body in order of appearance   |

- Properties read a property assigned earlier in the expanded file, one assigned later is empty at that point
- Properties never read items, `@(Item)` text in a property stays literal until a target expands it
- Property functions on a property holding `@(Item)` text operate on the literal text
- Item, `ItemGroup`, and `Target` conditions read the final value of every property
- `%(Name)` in an item definition reads the definition's own earlier metadata, never the value an item assigns
- Inside a target, properties and items evaluate together in order of appearance

Each repository file reads what the imports before it assigned:
- `Directory.Build.props` reads no project-body value and none from `Microsoft.NET.Sdk.props` onward
- `TargetFramework`, `OutputType`, `Configuration`, and `NETCoreSdkVersion` are empty in `Directory.Build.props` unless a global property sets them
- `Directory.Build.targets` reads the project body, the SDK output paths, and every package `.targets` file
- `IsPackable`, `IsTestProject`, and `EnableDefaultItems` hold a value the project or an earlier file assigned
- Inner builds and a `-p:TargetFramework=` caller supply `TargetFramework` as a global property, the outer build leaves it empty in every file
- `obj/*.nuget.g.*` imports are absent during restore, no package `.props` or `.targets` imports in that pass

Import rules:
- MSBuild imports one file path at most once, a repeated import warns `MSB4011` and is ignored
- Wildcard imports sort by ordinal string comparison, `010-a.props` before `100-b.props` before `20-c.props`, fixed-width prefixes keep order
- Import paths evaluating to empty fail `MSB4020`
- `Microsoft.Common.props` imports `$(CustomBeforeDirectoryBuildProps)` and `$(CustomAfterDirectoryBuildProps)` around `Directory.Build.props`
- `Microsoft.Common.targets` imports `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)` around `Directory.Build.targets`
- Every custom import tests for a non-empty value
- `Sdk.props` appends `UseArtifactsOutputPath.props` to `$(CustomAfterDirectoryBuildProps)`
- `$(ImportDirectoryBuildProps)` disables the props file and `$(DirectoryBuildPropsPath)` names it, from a global property or environment variable
- `Directory.Build.props` can set `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)`

Files outside the chain:
- `global.json` selects the SDK, `rollForward: disable` requires an exact match of the listed version
- `dotnet` muxer searches for `global.json` upward from the current directory, the SDK resolver from the solution or else the project directory
- Solution builds, `.slnx` included, evaluate a generated project importing `Directory.Solution.props` first and `Directory.Solution.targets` last
- Generated solution projects import no `Directory.Build.*` file and pass no property of their own to the projects they build
- `$(MSBuildProjectFullPath).user` imports when it exists, after the project body and before `Directory.Build.targets`, in inner and outer builds
- Nearest `Directory.Build.rsp` above the project or solution directory holds default switches, one per line, for every command-line build
- `-noAutoResponse` on the command line skips `Directory.Build.rsp`, inside it fails `MSB1027`, a `dotnet` switch inside it fails `MSB1001`
- `%MSBuildThisFileDirectory%` inside `Directory.Build.rsp` expands to its folder

## [02]-[CONDITIONS]

`Condition` attributes hold one expression string MSBuild tokenizes before it expands properties:

| [INDEX] | [FORM]                     | [RULE]                                                                               |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `'$(A)' == 'b'`, `!=`      | Case-insensitive string comparison, quote both sides, an empty side needs its quotes |
|  [02]   | `<`, `>`, `<=`, `>=`       | Decimal, `0x` hexadecimal, or `System.Version` operands, escaped as `&lt;` and `&gt;` |
|  [03]   | `Exists('path')`           | File or directory test, no wildcard expansion                                        |
|  [04]   | `HasTrailingSlash('path')` | True for a trailing `/` or `\`                                                       |
|  [05]   | `!`, `And`, `Or`, `( )`    | `And` binds tighter than `Or`, a mixed chain without parentheses warns `MSB4130`     |
|  [06]   | `$([MSBuild]::Fn(...))`    | Boolean property functions stand alone, string ones sit inside quotes                |
|  [07]   | `$(A.StartsWith('x'))`     | String instance methods on a property, the value evaluates to `True` or `False`      |

- Inside a quoted operand, backticks quote function arguments and an inner `'` fails `MSB4092`
- `'` alone quotes a top-level operand, `&apos;` inside a single-quoted attribute evaluates
- `Exists` and `HasTrailingSlash` exist in conditions alone, `$([MSBuild]::HasTrailingSlash())` fails `MSB4186`
- `'1.1' < '1.1.0'` is true under `System.Version`, the `[MSBuild]::Version*` functions compare by semver rules
- When every child shares a condition, put it on the `PropertyGroup` or `ItemGroup`

```xml
<!-- Directory.Build.targets -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'netstandard2.0'))">
  <DefineConstants>$(DefineConstants);FEATURE_SPANS</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'">
  <DefineConstants>$(DefineConstants);HOST_CORE</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
  <DefineConstants>$(DefineConstants);HOST_MACOS</DefineConstants>
</PropertyGroup>
```

## [03]-[PROPERTIES]

Properties hold one string, the last assignment in evaluation order wins, a global property wins over every assignment:
- List-valued properties append through their current value, `$(DefineConstants);FEATURE_A`, an assignment without it drops every earlier entry
- MSBuild skips a global property assignment and logs `The "Name" property is a global property, and cannot be modified` at diagnostic verbosity
- `TreatAsLocalProperty="Name"` on `<Project>` lets that file and later files override the global value, child projects receive the global value
- Project assignments override an environment variable's property, a global property overrides both
- Reserved properties (`MSBuildProjectName`, `MSBuildThisFileDirectory`) fail `MSB4004` on assignment
- `_` prefixes a property private to its file

```xml
<PropertyGroup>
  <ToolPath>$([MSBuild]::ValueOrDefault('$(ToolPathOverride)', '$(MSBuildThisFileDirectory)tools/tool'))</ToolPath>
  <_ToolDir>$([MSBuild]::NormalizeDirectory('$(ToolPath)'))</_ToolDir>
  <ToolFile>$([MSBuild]::NormalizePath('$(_ToolDir)', 'tool.exe'))</ToolFile>
  <ToolRelative>$([MSBuild]::MakeRelative('$(MSBuildThisFileDirectory)', '$(ToolPath)'))</ToolRelative>
  <NoWarn>$(NoWarn);NU1603</NoWarn>
</PropertyGroup>
```

| [INDEX] | [FUNCTION]                                             | [RESULT]                                                                |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `[MSBuild]::NormalizeDirectory(parts...)`              | Full path with the OS separator and a trailing slash, `''` fails        |
|  [02]   | `[MSBuild]::NormalizePath(parts...)`                   | Full path with the OS separator, no trailing slash added                |
|  [03]   | `[MSBuild]::EnsureTrailingSlash(path)`                 | Value with a trailing slash, `''` stays empty                           |
|  [04]   | `[MSBuild]::MakeRelative(base, path)`                  | `path` relative to the absolute directory `base`                        |
|  [05]   | `[MSBuild]::ValueOrDefault(value, default)`            | `value` unless empty, then `default`                                    |
|  [06]   | `[MSBuild]::GetDirectoryNameOfFileAbove(dir, file)`    | Nearest directory at or above `dir` holding `file`, else empty          |
|  [07]   | `[MSBuild]::GetPathOfFileAbove(file, dir)`             | Full path of the nearest `file` at or above `dir`, else empty           |
|  [08]   | `[MSBuild]::IsOSPlatform('OSX')`                       | `True` on the named `OSPlatform`, `Windows`, `Linux`, `OSX`             |
|  [09]   | `[MSBuild]::IsOSUnixLike()`                            | `True` on Linux or OSX                                                  |
|  [10]   | `[MSBuild]::VersionGreaterThanOrEquals(a, b)`          | Semver-aware compare, `v` prefix and `-`/`+` suffix ignored, `''` fails |
|  [11]   | `[MSBuild]::IsTargetFrameworkCompatible(target, cand)` | `True` when `target` can consume an asset built for `cand`              |
|  [12]   | `[MSBuild]::GetTargetFrameworkIdentifier(tfm)`         | `.NETCoreApp` for `net10.0`, `.NETStandard` for `netstandard2.0`        |
|  [13]   | `[MSBuild]::StableStringHash(text, 'Sha256')`          | Hash stable across machines and hosts                                   |
|  [14]   | `[System.IO.Path]::GetFileName(path)`                  | Any static method of the allowed `System.*` classes                     |

- Reversed arguments return empty from `GetDirectoryNameOfFileAbove(dir, file)` and fail `MSB4184` in `GetPathOfFileAbove(file, dir)`
- `$(MSBuildThisFileDirectory)` names the evaluating file's folder with a trailing slash, `$(MSBuildProjectDirectory)` the project folder without one

Prove a value without a build with `dotnet msbuild <project> -getProperty:Name`, one name prints the value, a comma list prints JSON, `-p:` shows the effect of a global property.

## [04]-[ITEMS]

Item elements perform one operation each, in order of appearance across every import:

| [INDEX] | [ATTRIBUTE]                      | [EFFECT]                                                                                  |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Include`                        | Adds items with the metadata on the element, duplicates stay outside a target             |
|  [02]   | `Exclude`                        | Subtracts from the `Include` of the same element, beside `Update` it fails `MSB4066`      |
|  [03]   | `Remove`                         | Removes matching items of that type, `Remove="@(Type)"` clears it                         |
|  [04]   | `Update`                         | Sets metadata on items existing at that point, an unmatched spec reports nothing          |
|  [05]   | `MatchOnMetadata`                | `Remove="@(Other)"` matches on the named metadata, `MatchOnMetadataOptions="PathLike"`    |
|  [06]   | `KeepMetadata`, `RemoveMetadata` | In a target, filters the metadata copied from the source items, definition defaults stay  |
|  [07]   | `KeepDuplicates="false"`         | In a target, skips an item with the identity and metadata of an existing item             |

- Inside a target, `Update` applies its metadata to every item of the type, `Condition="'%(Identity)' == 'name'"` on an item element selects one item
- Outside a target, an item condition reads properties and `@(Item)` lists, `%(Custom)` fails `MSB4191` and `%(Filename)` fails `MSB4190`
- Transform `@(Item->'%(Meta)')` is the one `%()` form an evaluation-time item, condition, or property accepts
- `ItemDefinitionGroup` sets default metadata for a type, an item's own metadata wins, `@(Item)` in a definition fails `MSB4164`
- `Include` paths in an imported file resolve against the project directory, a `$(MSBuildThisFileDirectory)` prefix resolves beside the importing file
- Every item has metadata `FullPath`, `RootDir`, `Filename`, `Extension`, `RelativeDir`, `RecursiveDir`, `Identity`, and `DefiningProjectDirectory`

```xml
<ItemGroup>
  <Source Include="**/*.cs" Exclude="Generated/**;Tests/**" />
  <Source Include="Generated/*.cs" Kind="generated" />
  <Source Remove="Legacy.cs" />
  <Source Update="Generated/*.cs" Owner="tool" />
</ItemGroup>
```

```xml
<!-- Inside a target -->
<ItemGroup>
  <Asset Condition="'%(Identity)' == 'config.json'" Copy="PreserveNewest" />
  <Copied Include="@(Asset)" KeepMetadata="Copy" />
</ItemGroup>
```

Item functions and transforms return a new list wherever `@()` is legal:

| [INDEX] | [EXPRESSION]                                         | [RESULT]                                                    |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `@(Item->'%(Filename)%(Extension)')`                 | Transform, one string per item                              |
|  [02]   | `@(Item->'%(Filename)', ', ')`                       | Transform joined with a separator                           |
|  [03]   | `@(Item->WithMetadataValue('Kind', 'generated'))`    | Items with that metadata value, case-insensitive            |
|  [04]   | `@(Item->AnyHaveMetadataValue('Kind', 'generated'))` | `true` or `false`, usable alone as a condition              |
|  [05]   | `@(Item->Metadata('Kind'))`                          | Metadata values, source metadata kept                       |
|  [06]   | `@(Item->Distinct())`, `->Count()`, `->Reverse()`    | Identities without duplicates, the count, the reversed list |
|  [07]   | `@(Item->ClearMetadata())`                           | Identities with every metadata value removed                |
|  [08]   | `@(Item->HasMetadata('Kind'))`, `->Exists()`         | Items with that metadata name, items present on disk        |

Prove items with `dotnet msbuild <project> -getItem:Type`, it prints every item with its well-known metadata as JSON, `jq -r '.Items.Type[].Identity'` lists the identities.

## [05]-[FILE_PLACEMENT]

Each declaration has one owning file, chosen by what it reads and who overrides it:

| [INDEX] | [DECLARATION]                                                                                        | [FILE]                           |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | Repository root paths, `ArtifactsPath`, `UseArtifactsOutput`, `ArtifactsProjectName`                | `Directory.Build.props`          |
|  [02]   | `BaseIntermediateOutputPath`, `MSBuildProjectExtensionsPath`                                         | `Directory.Build.props`          |
|  [03]   | Defaults a project overrides: `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel` | `Directory.Build.props`          |
|  [04]   | Classification from `MSBuildProjectName` or `MSBuildProjectDirectory`                                | `Directory.Build.props`          |
|  [05]   | Analyzer `PackageReference` every project gets                                                       | `Directory.Build.props`          |
|  [06]   | Values derived from `TargetFramework`, `OutputType`, or another body property                        | `Directory.Build.targets`        |
|  [07]   | `Using` items, conditioned on `@(PackageReference)` or not, `Update` on SDK glob items               | `Directory.Build.targets`        |
|  [08]   | Custom targets and `DependsOn` extensions                                                            | `Directory.Build.targets`        |
|  [09]   | `TargetFramework`, `OutputType`, `ArtifactsPivots`, `PackageReference`, `ProjectReference`           | Project file                     |
|  [10]   | Package versions, `PackageVersion`, `GlobalPackageReference`                                         | `Directory.Packages.props`       |
|  [11]   | Analyzer severity, `build_check.*` severity and options                                              | `.editorconfig`                  |
|  [12]   | Machine-local overrides                                                                              | `$(MSBuildProjectFullPath).user` |
|  [13]   | Properties and targets a package gives its consumers                                                 | Package `build/` files           |

- MSBuild imports the nearest `Directory.Build.props` and `.targets` above a project, a nested file opens with an import of the outer one

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM]                                | [CAUSE]                                        | [FIX]                                 |
| :-----: | :--------------------------------------- | :--------------------------------------------- | :------------------------------------ |
|  [01]   | `Directory.Build.props` is not imported  | Case differs on a case-sensitive volume        | Match the case exactly                |
|  [02]   | `Directory.Build.props` value is ignored | Project body or the SDK reassigns it later     | Set it in `Directory.Build.targets`   |
|  [03]   | Property holds `@(...)` text             | Properties never read items                    | Read the list in a target             |
|  [04]   | `-getProperty` fails with `MSB1063`      | Argument is a solution                         | Point the query at one project file   |

- `dotnet msbuild <project> -p:TargetFramework=net10.0 -getProperty:Name` evaluates one inner build of a multi-targeting project
