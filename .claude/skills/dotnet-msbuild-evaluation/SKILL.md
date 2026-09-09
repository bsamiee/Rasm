---
name: dotnet-msbuild-evaluation
description: "Use when writing or debugging a .props, .targets, or .csproj declaration, covering MSBuild evaluation order, conditions, properties, and items."
---

# [DOTNET_MSBUILD_EVALUATION]

The evaluation phase of a `Microsoft.NET.Sdk` project, from the import chain to the file that owns each declaration.

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
|  [04]   | Items                  | Every property, one assigned later in the file included, and items assigned earlier   |
|  [05]   | `UsingTask`            | Properties                                                                            |
|  [06]   | Targets                | Nothing runs, the execution phase evaluates each target body in order of appearance   |

- A property reads a property assigned earlier in the expanded file, a property assigned later is empty at that point
- A property never reads items, `@(Item)` text in a property stays literal until a target expands it
- A property function on a property holding `@(Item)` text operates on the literal text, `$(X.Split())` over an item list splits the joined string
- Item, `ItemGroup`, and `Target` conditions read the final value of every property
- `%(Name)` in an item definition reads the definition's own earlier metadata, never the value an item assigns
- Inside a target, properties and items evaluate together in order of appearance

Each repository file reads what the imports before it assigned:
- `Directory.Build.props` reads no project-body value, `TargetFramework`, `OutputType`, `Configuration`, and `NETCoreSdkVersion` are empty there unless a global property sets them
- `Directory.Build.targets` reads the project body, the SDK output paths, and every package `.targets` file
- `IsPackable`, `IsTestProject`, and `EnableDefaultItems` hold a value the project or an earlier file assigned
- A multi-targeting inner build and a `-p:TargetFramework=` caller supply `TargetFramework` as a global property, the outer build leaves it empty in every file
- The `obj/*.nuget.g.*` imports are absent during restore, no package `.props` or `.targets` imports in that pass

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <Stage Condition="'$(Stage)' == ''">library</Stage>
  <Role Condition="$(MSBuildProjectDirectory.StartsWith('$(RepositoryRoot)tests'))">tests</Role>
  <Role Condition="'$(Role)' == ''">library</Role>
</PropertyGroup>

<!-- Directory.Build.targets -->
<PropertyGroup Condition="'$(OutputType)' == 'Exe'">
  <SelfContained>false</SelfContained>
</PropertyGroup>
```

Import rules:
- MSBuild imports one file path at most once, a repeated import warns `MSB4011` and is ignored
- Wildcard imports sort by ordinal string comparison, `010-a.props` before `100-b.props` before `20-c.props`, fixed-width number prefixes keep the order
- An import path evaluating to empty fails `MSB4020`, a missing file fails `MSB4019`
- `Exists()` guards an optional import, a required import stays unguarded
- `Microsoft.Common.props` imports `$(CustomBeforeDirectoryBuildProps)` and `$(CustomAfterDirectoryBuildProps)` around `Directory.Build.props`
- `Microsoft.Common.targets` imports `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)` around `Directory.Build.targets`
- Every custom import tests for a non-empty value
- `Sdk.props` appends `UseArtifactsOutputPath.props` to `$(CustomAfterDirectoryBuildProps)`
- `$(DirectoryBuildPropsPath)` and `$(ImportDirectoryBuildProps)` name or disable the props file, `Microsoft.Common.props` reads both before any repository file, a global property or environment variable supplies them
- `Directory.Build.props` can set `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)`

Files outside the chain:
- `global.json` selects the SDK, `rollForward: disable` requires an exact match of the listed version
- The `dotnet` muxer searches for `global.json` upward from the current directory, the MSBuild SDK resolver upward from the solution directory, else the project directory
- `dotnet build <solution>` evaluates a generated solution project, it imports `Directory.Solution.props` at its start and `Directory.Solution.targets` at its end, `.slnx` included, and no `Directory.Build.*` file
- The projects a solution build starts receive no solution-level property
- `Microsoft.Common.CurrentVersion.targets` and `Microsoft.Common.CrossTargeting.targets` import `$(MSBuildProjectFullPath).user` when it exists, after the project body and before `Directory.Build.targets`
- `Directory.Build.rsp` holds default switches, one per line, for every `dotnet` and `msbuild` command-line build below it, the nearest one upward from the project or solution directory applies, `-noAutoResponse` on the command line skips it
- `-noAutoResponse` inside `Directory.Build.rsp` fails `MSB1027`, a `dotnet` switch inside it fails `MSB1001`, `%MSBuildThisFileDirectory%` inside it expands to its folder

## [02]-[CONDITIONS]

The `Condition` attribute holds one expression string MSBuild tokenizes before it expands properties:

| [INDEX] | [FORM]                     | [RULE]                                                                               |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `'$(A)' == 'b'`, `!=`      | Case-insensitive string comparison, quote both sides, an empty side needs its quotes |
|  [02]   | `<`, `>`, `<=`, `>=`       | Decimal, `0x` hexadecimal, or `System.Version` operands, escaped as `&lt;` and `&gt;` |
|  [03]   | `Exists('path')`           | File or directory test, no wildcard expansion                                        |
|  [04]   | `HasTrailingSlash('path')` | True for a trailing `/` or `\`                                                       |
|  [05]   | `!`, `And`, `Or`, `( )`    | `And` binds tighter than `Or`, a mixed chain without parentheses warns `MSB4130`     |
|  [06]   | `$([MSBuild]::Fn(...))`    | Boolean property functions stand alone, string ones sit inside quotes                |
|  [07]   | `$(A.StartsWith('x'))`     | String instance methods on a property, the value evaluates to `True` or `False`      |

- Inside a quoted operand, quote function arguments with backticks, `'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'`, an inner `'` fails `MSB4092`
- `'` alone quotes a top-level operand, `"`, `&quot;`, and backticks fail `MSB4090`, `&apos;` inside a single-quoted attribute evaluates
- `Exists` and `HasTrailingSlash` exist in conditions alone, `$([MSBuild]::HasTrailingSlash())` fails `MSB4186`
- An unquoted literal with `.`, `-`, `:`, or a space fails `MSB4092`, an unquoted empty property compares equal to another empty one
- An empty property standing alone as the condition fails `MSB4113`, an empty numeric operand fails `MSB4086`
- `'1.1' < '1.1.0'` is true under `System.Version`, the `[MSBuild]::Version*` functions compare by semver rules
- `IsTargetFrameworkCompatible` compares frameworks
- When every child shares a condition, put it on the `PropertyGroup` or `ItemGroup`

```xml
<!-- Directory.Build.targets -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'netstandard2.0'))">
  <DefineConstants>$(DefineConstants);FEATURE_SPANS</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
  <DefineConstants>$(DefineConstants);HOST_MACOS</DefineConstants>
</PropertyGroup>
```

## [03]-[PROPERTIES]

A property holds one string, the last assignment in evaluation order wins, a global property wins over every assignment:
- A default takes `Condition="'$(Name)' == ''"`, a project overrides one in `.props`, one in `.targets` applies when the project assigned nothing
- A list-valued property appends through its current value, `$(DefineConstants);FEATURE_A`, an assignment without `$(Name);` drops every earlier entry
- Project XML cannot reassign a global property from `-p:`, the `MSBuild` task, or an inner build, MSBuild skips the assignment and logs `The "Name" property is a global property, and cannot be modified` at diagnostic verbosity, a normalized form goes into a private property
- `TreatAsLocalProperty="Name"` on `<Project>` makes an assignment in that file and later files win over the global value, child projects still receive the global value
- An environment variable with a valid property name is a property, a project assignment overrides it, a global property overrides both
- A reserved property (`MSBuildProjectName`, `MSBuildThisFileDirectory`) fails `MSB4004` on assignment
- `_` prefixes a property private to its file

```xml
<PropertyGroup>
  <ToolPath>$([MSBuild]::ValueOrDefault('$(ToolPathOverride)', '$(MSBuildThisFileDirectory)tools/tool'))</ToolPath>
  <_ToolDir>$([MSBuild]::NormalizeDirectory('$(ToolPath)'))</_ToolDir>
  <ToolFile>$([MSBuild]::NormalizePath('$(_ToolDir)', 'tool.exe'))</ToolFile>
  <ToolRelative>$([MSBuild]::MakeRelative('$(MSBuildThisFileDirectory)', '$(ToolPath)'))</ToolRelative>
  <NoWarn>$(NoWarn);CS1591</NoWarn>
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
|  [09]   | `[MSBuild]::IsOsUnixLike()`                            | `True` on Linux or OSX, spelled with a lowercase `s`                    |
|  [10]   | `[MSBuild]::VersionGreaterThanOrEquals(a, b)`          | Semver-aware compare, `v` prefix and `-`/`+` suffix ignored, `''` fails |
|  [11]   | `[MSBuild]::IsTargetFrameworkCompatible(target, cand)` | `True` when `cand` can consume an asset built for `target`              |
|  [12]   | `[MSBuild]::GetTargetFrameworkIdentifier(tfm)`         | `.NETCoreApp` for `net10.0`, `.NETStandard` for `netstandard2.0`        |
|  [13]   | `[MSBuild]::StableStringHash(text, 'Sha256')`          | Hash stable across machines and hosts                                   |
|  [14]   | `[System.IO.Path]::GetFileName(path)`                  | Any static method of the allowed `System.*` classes                     |

- `GetDirectoryNameOfFileAbove(dir, file)` and `GetPathOfFileAbove(file, dir)` take their arguments in opposite orders, the reversed order returns empty from the first and fails `MSB4184` in the second
- `$(MSBuildThisFileDirectory)` ends with a slash and names the folder of the file under evaluation, `$(MSBuildProjectDirectory)` names the project folder without a slash

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

- The SDK default globs run after `Directory.Build.props`, an `Update` there matches nothing
- `Include` of a file a default glob matches fails `Compile` with `NETSDK1022`
- Inside a target, `Update` applies its metadata to every item of the type, `Condition="'%(Identity)' == 'name'"` on an item element selects one item
- Outside a target, an item condition reads properties and `@(Item)` lists, `%(Custom)` fails `MSB4191` and `%(Filename)` fails `MSB4190`
- The transform `@(Item->'%(Meta)')` is the one `%()` form an evaluation-time item, condition, or property accepts
- `ItemDefinitionGroup` sets default metadata for a type, an item's own metadata wins, `@(Item)` in a definition fails `MSB4164`
- `Include` paths resolve against the project directory in an imported file too, a `$(MSBuildThisFileDirectory)` prefix resolves beside the importing file
- Every item has `%(FullPath)`, `%(RootDir)`, `%(Filename)`, `%(Extension)`, `%(RelativeDir)`, `%(RecursiveDir)`, `%(Identity)`, and `%(DefiningProjectDirectory)`

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
|  [01]   | Repository root paths, `ArtifactsPath`, `UseArtifactsOutput`, `BaseIntermediateOutputPath`           | `Directory.Build.props`          |
|  [02]   | Defaults a project overrides: `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel` | `Directory.Build.props`          |
|  [03]   | Classification from `MSBuildProjectName` or `MSBuildProjectDirectory`                                | `Directory.Build.props`          |
|  [04]   | Items every project gets: analyzer `PackageReference`, `Using`                                       | `Directory.Build.props`          |
|  [05]   | Values derived from `TargetFramework`, `OutputType`, or another body property                        | `Directory.Build.targets`        |
|  [06]   | `Using` conditioned on a `PackageReference`, `Update` on SDK glob items                              | `Directory.Build.targets`        |
|  [07]   | Custom targets and `DependsOn` extensions                                                            | `Directory.Build.targets`        |
|  [08]   | `TargetFramework`, `OutputType`, `ArtifactsPivots`, `PackageReference`, `ProjectReference`           | Project file                     |
|  [09]   | `ArtifactsProjectName` for `bin`, `obj`, and `publish`                                               | `Directory.Build.props`          |
|  [10]   | Package versions, `PackageVersion`, `GlobalPackageReference`                                         | `Directory.Packages.props`       |
|  [11]   | Analyzer severity, `build_check.*` severity and options                                              | `.editorconfig`                  |
|  [12]   | Machine-local overrides                                                                              | `$(MSBuildProjectFullPath).user` |
|  [13]   | Properties and targets a package gives its consumers                                                 | Package `build/` files           |

- `ArtifactsProjectName` in a project file renames `bin` and `publish` while `obj` keeps the project name
- MSBuild imports the nearest `Directory.Build.props` and `Directory.Build.targets` above a project, a nested file opens with an import of the outer one, `references/multi-level-examples.md` holds the form

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM]                                | [CAUSE]                                        | [FIX]                                 |
| :-----: | :--------------------------------------- | :--------------------------------------------- | :------------------------------------ |
|  [01]   | `Directory.Build.props` is not imported  | Case differs on a case-sensitive volume        | Match the case exactly                |
|  [02]   | `Directory.Build.props` value is ignored | Project body or the SDK reassigns it later     | Set it in `Directory.Build.targets`   |
|  [03]   | `TargetFramework` condition never holds  | The `PropertyGroup` sits in a `.props` file    | Move it to `.targets` or the project  |
|  [04]   | `-p:` value is not normalized            | Project XML cannot reassign a global property  | Derive a private property             |
|  [05]   | `Update` changes no metadata             | Item does not exist yet at that point          | Move the `Update` after the `Include` |
|  [06]   | Property holds `@(...)` text             | Properties never read items                    | Read the list in a target             |
|  [07]   | `-getProperty` fails with `MSB1063`      | Argument is a solution                         | Point the query at one project file   |

- `dotnet msbuild <project> -p:TargetFramework=net10.0 -getProperty:Name` evaluates one inner build of a multi-targeting project
