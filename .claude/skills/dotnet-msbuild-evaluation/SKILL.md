---
name: dotnet-msbuild-evaluation
description: "Use when writing or debugging a .props, .targets, or .csproj declaration, covering MSBuild evaluation passes, import chain, conditions, property functions, item operations, owning file, and -getProperty proof."
---

# [DOTNET_MSBUILD_EVALUATION]

Covers the evaluation phase of a `Microsoft.NET.Sdk` project, from the import chain to the file that owns each declaration.

- Use `dotnet-msbuild-execution` for targets, `DependsOn` chains, `Inputs` and `Outputs`, generated files, and target scope
- Use `dotnet-msbuild-antipatterns` for the review catalog of smells with `BAD` and `GOOD` pairs
- Use `dotnet-msbuild-diagnostics` for binary logs, the `binlog` MCP, failure triage, and BuildCheck runs
- Use `dotnet-msbuild-packaging` for NuGet package metadata, package `build/` folders, central package management, solution files, and CI properties
- Use `monorepo-build-infrastructure` for the `eng/` directory, task runner targets, native packaging projects, and provisioning

[REFERENCES]:
- [01]-[IMPORT_CHAIN](references/import-chain.md): Every import of a `Microsoft.NET.Sdk` project in order, with the properties each file assigns
- [02]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): Root and nested `Directory.Build.*` files with project files holding only what differs

## [01]-[EVALUATION_ORDER]

MSBuild evaluates a project in passes, and each pass reads only what the earlier passes produced:

| [INDEX] | [PASS]                 | [READS]                                                                               |
| :-----: | :--------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Environment variables  | Every variable with a valid property name becomes a property                          |
|  [02]   | Imports and properties | Properties in order of appearance with imports expanded in place, the last value wins |
|  [03]   | Item definitions       | Properties, and metadata of the same item definition assigned earlier                 |
|  [04]   | Items                  | Every property, including one assigned later in the file, and items assigned earlier  |
|  [05]   | `UsingTask`            | Properties                                                                            |
|  [06]   | Targets                | Nothing runs, the execution phase evaluates each target body in order of appearance   |

- Properties read a property assigned earlier in the expanded file, and a property assigned later is empty at that point
- Properties never read items, `@(Item)` text in a property stays literal until a target expands it, and a property function on that property operates on the literal text
- Item, `ItemGroup`, and `Target` conditions read the final value of every property
- `%(Name)` in an item definition reads the definition's own earlier metadata, never the value an item assigns
- Inside a target, properties and items evaluate together in order of appearance, and a property set there reads the items assigned earlier in that target

Each repository file reads only what the imports before it assigned:
- `Directory.Build.props` reads no project-body value, and `TargetFramework`, `OutputType`, `Configuration`, and `NETCoreSdkVersion` are empty there unless a global property supplies them
- `Directory.Build.targets` reads the project body, the SDK output paths, and every package `.targets` file, and `IsPackable`, `IsTestProject`, and `EnableDefaultItems` hold only a value the project or an earlier file assigned
- Multi-targeting inner builds and a `-p:TargetFramework=` caller supply `TargetFramework` as a global property, and the outer build leaves it empty in every file
- The `obj/*.nuget.g.*` imports are absent during restore, and no package `.props` or `.targets` file imports in that pass

```xml
<!-- Directory.Build.props: a default the project body overrides, and a classification from a reserved property -->
<PropertyGroup>
  <Stage Condition="'$(Stage)' == ''">library</Stage>
  <Role Condition="$(MSBuildProjectDirectory.StartsWith('$(RepositoryRoot)tests'))">tests</Role>
  <Role Condition="'$(Role)' == ''">library</Role>
</PropertyGroup>

<!-- Directory.Build.targets: a value derived from a project-body property -->
<PropertyGroup Condition="'$(OutputType)' == 'Exe'">
  <SelfContained>false</SelfContained>
</PropertyGroup>
```

Import rules:
- MSBuild imports one file path at most once, a repeated import of that path warns `MSB4011`, and MSBuild ignores it
- Wildcard imports sort their matches by ordinal string comparison, `010-a.props` before `100-b.props` before `20-c.props`, and a fixed-width number prefix keeps the intended order
- Import paths that evaluate to empty fail with `MSB4020`, missing files fail with `MSB4019`, `Exists()` guards an optional import, and a required import stays unguarded
- `Microsoft.Common.props` imports `$(CustomBeforeDirectoryBuildProps)` and `$(CustomAfterDirectoryBuildProps)` around `Directory.Build.props`, `Microsoft.Common.targets` imports `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)` around `Directory.Build.targets`, each import tests only for a non-empty value, and `Sdk.props` appends `UseArtifactsOutputPath.props` to `$(CustomAfterDirectoryBuildProps)`
- `$(DirectoryBuildPropsPath)` and `$(ImportDirectoryBuildProps)` name or disable the props file, `Microsoft.Common.props` reads them before any repository file, a global property or environment variable supplies them, and `Directory.Build.props` can set `$(CustomBeforeDirectoryBuildTargets)` and `$(CustomAfterDirectoryBuildTargets)`

Files outside the chain:
- `global.json` selects the SDK, the `dotnet` muxer searches upward from the current directory, the MSBuild SDK resolver from the solution directory, else the project directory, and `rollForward: disable` requires an exact match of the listed version
- `dotnet build <solution>` evaluates a generated solution project that imports `Directory.Solution.props` at its start and `Directory.Solution.targets` at its end, `.slnx` solutions import both, the solution project imports no `Directory.Build.*` file, and the projects it builds receive no solution-level property
- `Microsoft.Common.CurrentVersion.targets` and `Microsoft.Common.CrossTargeting.targets` import `$(MSBuildProjectFullPath).user` when it exists, after the project body and before `Directory.Build.targets`, for local overrides that stay out of source control
- `Directory.Build.rsp` holds default switches, one per line, for every `dotnet` and `msbuild` command-line build below it, the nearest file upward from the project or solution directory applies, `-noAutoResponse` on the command line skips it, `-noAutoResponse` inside it fails with `MSB1027`, a `dotnet` switch inside it fails with `MSB1001`, and `%MSBuildThisFileDirectory%` expands to its folder

## [02]-[CONDITIONS]

The `Condition` attribute holds one expression string that MSBuild tokenizes before it expands properties:

| [INDEX] | [FORM]                     | [RULE]                                                                               |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `'$(A)' == 'b'`, `!=`      | Case-insensitive string comparison, quote both sides, an empty side needs its quotes |
|  [02]   | `<`, `>`, `<=`, `>=`       | Decimal, `0x` hexadecimal, or `System.Version` operands, escaped as `&lt;` and `&gt;` |
|  [03]   | `Exists('path')`           | File or directory test, no wildcard expansion                                        |
|  [04]   | `HasTrailingSlash('path')` | True for a trailing `/` or `\`                                                       |
|  [05]   | `!`, `And`, `Or`, `( )`    | `And` binds tighter than `Or`, a mixed chain without parentheses warns `MSB4130`     |
|  [06]   | `$([MSBuild]::Fn(...))`    | Boolean property functions stand alone, string ones sit inside quotes                |
|  [07]   | `$(A.StartsWith('x'))`     | String instance methods on a property, the value evaluates to `True` or `False`      |

- Inside a quoted operand, quote function arguments with backticks, `'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'`, and an inner `'` fails with `MSB4092`
- `Exists` and `HasTrailingSlash` exist only in conditions, and `$([MSBuild]::HasTrailingSlash())` fails with `MSB4186`
- Unquoted literals with `.`, `-`, `:`, or a space fail with `MSB4092`, an unquoted empty property compares equal to another empty one, an empty property standing alone as the condition fails with `MSB4113`, and an empty numeric operand fails with `MSB4086`
- `'1.1' < '1.1.0'` is true under `System.Version`, the `[MSBuild]::Version*` functions compare versions by semver rules, and `IsTargetFrameworkCompatible` compares frameworks
- When every child shares a condition, put it on the `PropertyGroup` or `ItemGroup`

```xml
<!-- Directory.Build.targets: compatibility across identifier and version, then an OS test -->
<PropertyGroup Condition="$([MSBuild]::IsTargetFrameworkCompatible('$(TargetFramework)', 'netstandard2.0'))">
  <DefineConstants>$(DefineConstants);FEATURE_SPANS</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
  <DefineConstants>$(DefineConstants);HOST_MACOS</DefineConstants>
</PropertyGroup>
```

## [03]-[PROPERTIES]

Properties hold one string each, the last assignment in evaluation order wins, and a global property wins over every assignment:
- Defaults have `Condition="'$(Name)' == ''"`, a project overrides one in `.props`, and one in `.targets` applies only when the project assigned nothing
- List-valued properties append through their current value, `$(DefineConstants);FEATURE_A`, and an assignment without `$(Name);` drops every earlier entry
- Project XML cannot reassign a global property from `-p:`, the `MSBuild` task, or an inner build, MSBuild skips the assignment and logs `The "Name" property is a global property, and cannot be modified` at diagnostic verbosity, and a normalized form goes into a private property
- `TreatAsLocalProperty="Name"` on the `<Project>` element makes an assignment in that file and later files win over the global value, and child projects still receive the global value
- Environment variables with a valid property name are properties, a project assignment overrides them, and a global property overrides both
- Reserved properties (`MSBuildProjectName`, `MSBuildThisFileDirectory`) fail with `MSB4004` on assignment
- `_` prefixes a property private to its file, SDK `_` properties included

```xml
<PropertyGroup>
  <ToolPath>$([MSBuild]::ValueOrDefault('$(ToolPathOverride)', '$(MSBuildThisFileDirectory)tools/tool'))</ToolPath>
  <!-- ToolPath can be a global property, the normalized form goes into a private property -->
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
|  [03]   | `[MSBuild]::EnsureTrailingSlash(path)`                 | The value with a trailing slash, `''` stays empty                       |
|  [04]   | `[MSBuild]::MakeRelative(base, path)`                  | `path` relative to the absolute directory `base`                        |
|  [05]   | `[MSBuild]::ValueOrDefault(value, default)`            | `value` unless empty, then `default`                                    |
|  [06]   | `[MSBuild]::GetDirectoryNameOfFileAbove(dir, file)`    | Nearest directory at or above `dir` holding `file`, else empty          |
|  [07]   | `[MSBuild]::GetPathOfFileAbove(file, dir)`             | Full path of the nearest `file` at or above `dir`, else empty           |
|  [08]   | `[MSBuild]::IsOSPlatform('OSX')`                       | `True` on the named `OSPlatform`, `Windows`, `Linux`, `OSX`             |
|  [09]   | `[MSBuild]::VersionGreaterThanOrEquals(a, b)`          | Semver-aware compare, `v` prefix and `-`/`+` suffix ignored, `''` fails |
|  [10]   | `[MSBuild]::IsTargetFrameworkCompatible(target, cand)` | `True` when `cand` can consume an asset built for `target`              |
|  [11]   | `[MSBuild]::GetTargetFrameworkIdentifier(tfm)`         | `.NETCoreApp` for `net10.0`, `.NETStandard` for `netstandard2.0`        |
|  [12]   | `[MSBuild]::StableStringHash(text, 'Sha256')`          | Hash stable across machines and hosts                                   |
|  [13]   | `[System.IO.Path]::GetFileName(path)`                  | Any static method of the allowed `System.*` classes                     |

- `GetDirectoryNameOfFileAbove(dir, file)` and `GetPathOfFileAbove(file, dir)` take their arguments in opposite orders, the reversed order returns empty from `GetDirectoryNameOfFileAbove` and fails with `MSB4184` in `GetPathOfFileAbove`
- `$(MSBuildThisFileDirectory)` ends with a slash and names the folder of the file being evaluated, `$(MSBuildProjectDirectory)` names the project folder without a slash

Prove a value without a build with `dotnet msbuild <project> -getProperty:Name`, one name prints the value, a comma list prints JSON, and `-p:` shows the effect of a global property.

## [04]-[ITEMS]

Item elements perform one operation each, in order of appearance across every import:

| [INDEX] | [ATTRIBUTE]                      | [EFFECT]                                                                                  |
| :-----: | :------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `Include`                        | Adds items with the metadata on the element, and duplicates stay outside a target         |
|  [02]   | `Exclude`                        | Subtracts from the `Include` of the same element only, beside `Update` it fails `MSB4066` |
|  [03]   | `Remove`                         | Removes matching items of that type, `Remove="@(Type)"` clears it                         |
|  [04]   | `Update`                         | Sets metadata on items that already exist at that point, unmatched specs report nothing   |
|  [05]   | `MatchOnMetadata`                | `Remove="@(Other)"` matches on the named metadata, `MatchOnMetadataOptions="PathLike"`    |
|  [06]   | `KeepMetadata`, `RemoveMetadata` | In a target, filters the metadata copied from the source items, definition defaults stay  |
|  [07]   | `KeepDuplicates="false"`         | In a target, skips an item with the identity and metadata of an existing item             |

- The SDK default globs run after `Directory.Build.props`, an `Update` there matches nothing, and an `Include` of a file a default glob already matches fails `Compile` with `NETSDK1022`
- Inside a target, `Update` applies its metadata to every item of the type, and `Condition="'%(Identity)' == 'name'"` on an item element selects one item
- Outside a target, an item condition reads properties and `@(Item)` lists only, `%(Custom)` fails with `MSB4191` and `%(Filename)` with `MSB4190`, and the transform `@(Item->'%(Meta)')` is the one `%()` form an evaluation-time item, condition, or property accepts
- `ItemDefinitionGroup` sets default metadata for a type, an item's own metadata wins, and `@(Item)` in a definition fails with `MSB4164`
- `Include` paths resolve against the project directory even in an imported file, and a path prefixed with `$(MSBuildThisFileDirectory)` resolves beside the importing file
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
<!-- Inside a target: Condition selects one item, KeepMetadata filters the copied metadata -->
<ItemGroup>
  <Asset Condition="'%(Identity)' == 'config.json'" Copy="PreserveNewest" />
  <Copied Include="@(Asset)" KeepMetadata="Copy" />
</ItemGroup>
```

Item functions and transforms return a new list and are legal wherever `@()` is:

| [INDEX] | [EXPRESSION]                                         | [RESULT]                                                    |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `@(Item->'%(Filename)%(Extension)')`                 | Transform, one string per item                              |
|  [02]   | `@(Item->'%(Filename)', ', ')`                       | Transform joined with a separator                           |
|  [03]   | `@(Item->WithMetadataValue('Kind', 'generated'))`    | Items with that metadata value, case-insensitive            |
|  [04]   | `@(Item->AnyHaveMetadataValue('Kind', 'generated'))` | `true` or `false`, usable alone as a condition              |
|  [05]   | `@(Item->Metadata('Kind'))`                          | The metadata values, source metadata kept                   |
|  [06]   | `@(Item->Distinct())`, `->Count()`, `->Reverse()`    | Identities without duplicates, the count, the reversed list |
|  [07]   | `@(Item->ClearMetadata())`                           | Identities with every metadata value removed                |
|  [08]   | `@(Item->HasMetadata('Kind'))`, `->Exists()`         | Items with that metadata name, items present on disk        |

- Use `dotnet-msbuild-execution` for task and target batching inside targets

Prove items with `dotnet msbuild <project> -getItem:Type`, which prints every item with its well-known metadata as JSON, and `jq -r '.Items.Type[].Identity'` lists the identities.

## [05]-[FILE_PLACEMENT]

Each declaration has one owning file, chosen by what it must read and who must override it:

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
|  [13]   | Properties and targets a package gives its consumers                                                 | The package `build/` files       |

- `ArtifactsProjectName` in a project file renames `bin` and `publish` only while `obj` keeps the project name
- Use `dotnet-msbuild-packaging` for the artifacts layout, the package files, and solution files
- Use `dotnet-msbuild-diagnostics` for the `build_check.*` keys in `.editorconfig`

MSBuild imports only the nearest `Directory.Build.props` and `Directory.Build.targets` above a project, a nested file opens with an import of the outer one, and the private property keeps nested quotes out of the condition:

```xml
<!-- tests/Directory.Build.props -->
<PropertyGroup>
  <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
</PropertyGroup>
<Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />
```

## [06]-[TROUBLESHOOTING]

Each symptom has one cause and one fix:

| [INDEX] | [PROBLEM]                                | [CAUSE]                                        | [FIX]                                 |
| :-----: | :--------------------------------------- | :--------------------------------------------- | :------------------------------------ |
|  [01]   | `Directory.Build.props` is not imported  | The case differs on a case-sensitive volume    | Match the case exactly                |
|  [02]   | `Directory.Build.props` value is ignored | The project body or the SDK reassigns it later | Set it in `Directory.Build.targets`   |
|  [03]   | `TargetFramework` condition never holds  | The `PropertyGroup` sits in a `.props` file    | Move it to `.targets` or the project  |
|  [04]   | `-p:` value is not normalized            | Project XML cannot reassign a global property  | Derive a private property             |
|  [05]   | `Update` changes no metadata             | The item does not exist yet at that point      | Move the `Update` after the `Include` |
|  [06]   | Property holds `@(...)` text             | Properties never read items                    | Read the list in a target             |
|  [07]   | `-getProperty` fails with `MSB1063`      | The argument is a solution                     | Point the query at one project file   |

- `dotnet msbuild <project> -p:TargetFramework=net10.0 -getProperty:Name` evaluates one inner build of a multi-targeting project
- Use `dotnet-msbuild-diagnostics` for the switches and `binlog` MCP queries that prove an evaluation
