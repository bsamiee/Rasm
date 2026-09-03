---
name: dotnet-msbuild-evaluation
description: "Use when writing a property, item, condition, import, transform, or batch in a .props, .targets, or project file, or a value evaluates unexpectedly."
---

# [DOTNET_MSBUILD_EVALUATION]

Covers the evaluation phase of a `Microsoft.NET.Sdk` project: the import chain, the evaluation passes, conditions, properties, items, and the file that owns each declaration.
- `dotnet-msbuild-execution` owns targets, `DependsOn` chains, `Inputs` and `Outputs`, generated files, and target scope
- `dotnet-msbuild-antipatterns` owns the review catalog of smells with `BAD` and `GOOD` pairs
- `dotnet-msbuild-diagnostics` owns binary logs, the `binlog` MCP, failure triage, and BuildCheck runs
- `dotnet-msbuild-packaging` owns NuGet package metadata, package `build/` folders, central package management, solution files, and CI properties

[REFERENCES]:
- [01]-[IMPORT_CHAIN](references/import-chain.md): Every import of a `Microsoft.NET.Sdk` project in order, with the properties each file assigns
- [02]-[MULTI_LEVEL_EXAMPLES](references/multi-level-examples.md): Root and nested `Directory.Build.props` files, and settings moved out of project files

## [01]-[EVALUATION_ORDER]

MSBuild evaluates a project in passes, and each pass reads only what the earlier passes produced:

| [INDEX] | [PASS]                 | [READS]                                                                                 |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Environment variables  | Every variable with a valid property name becomes a property                            |
|  [02]   | Imports and properties | Properties in order of appearance with imports expanded in place, the last value wins   |
|  [03]   | Item definitions       | Properties, and metadata of the same item definition assigned earlier                   |
|  [04]   | Items                  | Every property, including one assigned later in the file, and items assigned earlier    |
|  [05]   | `UsingTask`            | Properties                                                                              |
|  [06]   | Targets                | Nothing runs, the execution phase evaluates each target body in order of appearance     |

- A property reads a property assigned earlier in the expanded file, and a property assigned later is empty at that point
- A property never reads an item, `@(Item)` text in a property stays literal until a target expands it, and a property function on that property operates on the literal text
- An item, `ItemGroup`, or `Target` condition reads the final value of every property
- `%(Name)` in an item definition reads the definition's own earlier metadata, never the value an item assigns
- Inside a target, properties and items evaluate together in order of appearance, and a property set there reads the items assigned earlier in that target

The import chain of a `Microsoft.NET.Sdk` project, in order:

| [INDEX] | [FILE]                    | [ASSIGNS BEFORE THE NEXT FILE]                                                                  |
| :-----: | :------------------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Sdk.props`               | `UsingMicrosoftNETSdk`, then imports `Microsoft.Common.props`                                   |
|  [02]   | `Microsoft.Common.props`  | Imports `Directory.Build.props`, `UseArtifactsOutputPath.props`, `obj/*.nuget.g.props`          |
|  [03]   | `NuGet.props`             | Imports the nearest `Directory.Packages.props`                                                  |
|  [04]   | `Microsoft.NET.Sdk.props` | `Configuration`, `OutputType`, `AssemblyName`, `NETCoreSdkVersion`, `DefineConstants`, globs    |
|  [05]   | Project body              | `TargetFramework` and every project-specific value                                              |
|  [06]   | `Sdk.targets`             | `TargetFrameworkIdentifier`, `OutputPath`, `IntermediateOutputPath`, `LangVersion`              |
|  [07]   | `Microsoft.Common.targets`| `TargetFrameworkMoniker`, `TargetPath`, imports `.user`, `obj/*.nuget.g.targets`, `Directory.Build.targets` |
|  [08]   | `Microsoft.NET.Sdk.targets` | `EnableDefaultItems`, `IsPackable`, and the build targets                                     |

- `Directory.Build.props` reads no project-body value, and `TargetFramework`, `OutputType`, `Configuration`, and `NETCoreSdkVersion` are empty there unless a global property supplies them
- `Directory.Build.targets` reads the project body, the SDK output paths, and every package `.targets` file, and `IsPackable`, `IsTestProject`, and `EnableDefaultItems` hold only a value the project or an earlier file assigned
- A multi-targeting inner build and a `-p:TargetFramework=` caller supply `TargetFramework` as a global property, and the outer build leaves it empty in every file
- The `obj/*.nuget.g.*` imports are absent during restore, and `Directory.Build.targets` sees no package `.targets` in that pass

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
- MSBuild imports one file path at most once, and a repeated import of that path warns `MSB4011` and is ignored
- A wildcard import sorts its matches ordinally, `010-a.props` before `100-b.props` before `20-c.props`, and a fixed-width number prefix keeps the intended order
- An empty import path fails with `MSB4020` and a missing file fails with `MSB4019`, guard an optional import with `Exists()` and leave a required import unguarded
- `Microsoft.Common.props` imports `$(CustomBeforeDirectoryBuildProps)` and `$(CustomAfterDirectoryBuildProps)` around `Directory.Build.props`, `Microsoft.Common.targets` does the same with the `DirectoryBuildTargets` pair, each tests only for a non-empty value, and `Sdk.props` appends `UseArtifactsOutputPath.props` to the after-props list
- `$(DirectoryBuildPropsPath)` and `$(ImportDirectoryBuildProps)` name or disable the props file, `Microsoft.Common.props` reads them before any repository file, a global property or environment variable supplies them, and `Directory.Build.props` can set the `DirectoryBuildTargets` pair

Files outside the chain:
- `global.json` selects the SDK, the `dotnet` muxer searches upward from the current directory, the MSBuild SDK resolver from the solution directory, else the project directory, and `rollForward: disable` pins the listed version
- `dotnet build <solution>` evaluates a generated solution project that imports `Directory.Solution.props` at its start and `Directory.Solution.targets` at its end, `.slnx` honors both, the solution project imports no `Directory.Build.*` file, and the project builds it starts see no solution-level value
- `Microsoft.Common.CurrentVersion.targets` and the cross-targeting targets import `$(MSBuildProjectFullPath).user` when it exists, after the project body and before `Directory.Build.targets`, for local overrides that stay out of source control
- `Directory.Build.rsp` holds default MSBuild switches for every `dotnet` and `msbuild` command-line build below it, the nearest file upward from the project or solution directory applies, `-noAutoResponse` on the command line skips it, `-noAutoResponse` inside it fails with `MSB1027`, a `dotnet` switch inside it fails with `MSB1001`, and `%MSBuildThisFileDirectory%` expands to its folder

## [02]-[CONDITIONS]

A `Condition` attribute is one expression string that MSBuild tokenizes before it expands properties:

| [INDEX] | [FORM]                              | [RULE]                                                                                    |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `'$(A)' == 'b'`, `!=`               | Case-insensitive string comparison, quote both sides, an empty side needs its quotes      |
|  [02]   | `<`, `>`, `<=`, `>=`                | Decimal, `0x` hexadecimal, or `System.Version` operands, escaped as `&lt;` and `&gt;`     |
|  [03]   | `Exists('path')`                    | File or directory test, no wildcard expansion                                             |
|  [04]   | `HasTrailingSlash('path')`          | True for a trailing `/` or `\`                                                            |
|  [05]   | `!`, `And`, `Or`, `( )`             | `And` binds tighter than `Or`, a mixed chain without parentheses warns `MSB4130`          |
|  [06]   | `$([MSBuild]::Fn(...))`             | A boolean property function stands alone, a string one sits inside quotes                 |
|  [07]   | `$(A.StartsWith('x'))`              | String instance methods on a property, the value evaluates to `True` or `False`           |

- Inside a quoted operand, quote function arguments with backticks, `'$([MSBuild]::GetTargetFrameworkIdentifier(`$(TargetFramework)`))' == '.NETCoreApp'`, and an inner `'` fails with `MSB4092`
- `Exists` and `HasTrailingSlash` exist only in conditions, and `$([MSBuild]::HasTrailingSlash())` fails with `MSB4186`
- An unquoted literal with `.`, `-`, `:`, or a space fails with `MSB4092`, an unquoted empty property compares equal to another empty one, a bare empty boolean fails with `MSB4113`, and an empty numeric operand fails with `MSB4086`
- `'1.1' < '1.1.0'` is true under `System.Version`, the `[MSBuild]::Version*` functions compare versions by semver rules, and `IsTargetFrameworkCompatible` compares frameworks
- Put one condition on the `PropertyGroup` or `ItemGroup` when every child shares it

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

A property holds one string, the last assignment in evaluation order wins, and a global property wins over every assignment:
- A default carries `Condition="'$(Name)' == ''"`, in `.props` a project can override it, and in `.targets` it becomes a fallback
- A list-valued property appends through its current value, `$(DefineConstants);FEATURE_A`, and an assignment without `$(Name);` drops every earlier entry
- A global property from `-p:`, the `MSBuild` task, or an inner build cannot be reassigned by project XML, MSBuild skips the assignment without a message, and a normalized form goes into a private property
- `TreatAsLocalProperty="Name"` on the `<Project>` element makes an assignment in that file and later files win over the global value, and child projects still receive the global value
- An environment variable with a valid property name is a property, a project assignment overrides it, and a global property overrides both
- A reserved property (`MSBuildProjectName`, `MSBuildThisFileDirectory`) fails with `MSB4004` on assignment
- `_` prefixes a property private to its file, and an `_` property that an SDK file defines is never read or assigned

```xml
<PropertyGroup>
  <ToolPath>$([MSBuild]::ValueOrDefault('$(ToolPathOverride)', '$(MSBuildThisFileDirectory)tools/tool'))</ToolPath>
  <!-- ToolPath can arrive as a global property, the trailing slash lands on a private name -->
  <_ToolDir>$([MSBuild]::NormalizeDirectory('$(ToolPath)'))</_ToolDir>
  <ToolFile>$([MSBuild]::NormalizePath('$(_ToolDir)', 'tool.exe'))</ToolFile>
  <ToolRelative>$([MSBuild]::MakeRelative('$(MSBuildThisFileDirectory)', '$(ToolPath)'))</ToolRelative>
  <NoWarn>$(NoWarn);CS1591</NoWarn>
</PropertyGroup>
```

| [INDEX] | [FUNCTION]                                            | [RESULT]                                                             |
| :-----: | :---------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `[MSBuild]::NormalizeDirectory(parts...)`             | Full path with the OS separator and a trailing slash, `''` fails     |
|  [02]   | `[MSBuild]::NormalizePath(parts...)`                  | Full path with the OS separator, no trailing slash added             |
|  [03]   | `[MSBuild]::EnsureTrailingSlash(path)`                | The value with a trailing slash, `''` stays empty                    |
|  [04]   | `[MSBuild]::MakeRelative(base, path)`                 | `path` relative to the absolute directory `base`                     |
|  [05]   | `[MSBuild]::ValueOrDefault(value, default)`           | `value` unless empty, then `default`                                 |
|  [06]   | `[MSBuild]::GetDirectoryNameOfFileAbove(dir, file)`   | Nearest directory at or above `dir` holding `file`, else empty       |
|  [07]   | `[MSBuild]::GetPathOfFileAbove(file, dir)`            | Full path of the nearest `file` at or above `dir`, else empty        |
|  [08]   | `[MSBuild]::IsOSPlatform('OSX')`                      | `True` on the named `OSPlatform`, `Windows`, `Linux`, `OSX`          |
|  [09]   | `[MSBuild]::VersionGreaterThanOrEquals(a, b)`         | Semver-aware compare, `v` prefix and `-`/`+` suffix ignored, `''` fails |
|  [10]   | `[MSBuild]::IsTargetFrameworkCompatible(target, cand)`| `True` when `cand` can consume an asset built for `target`           |
|  [11]   | `[MSBuild]::GetTargetFrameworkIdentifier(tfm)`        | `.NETCoreApp` for `net10.0`, `.NETStandard` for `netstandard2.0`     |
|  [12]   | `[MSBuild]::StableStringHash(text, 'Sha256')`         | Hash stable across machines and hosts                                |
|  [13]   | `[System.IO.Path]::GetFileName(path)`                 | Any static method of the allowed `System.*` classes                  |

- The two search functions take `file` and `dir` in opposite orders, `GetDirectoryNameOfFileAbove` with reversed arguments returns empty, and `GetPathOfFileAbove` with a directory as `file` fails with `MSB4184`
- `$(MSBuildThisFileDirectory)` ends with a slash and names the folder of the file being evaluated, `$(MSBuildProjectDirectory)` names the project folder without a slash

Prove a value with `dotnet msbuild <project> -getProperty:Name`, one name prints the bare value, a comma list prints JSON, and `-p:` shows the effect of a global property.

## [04]-[ITEMS]

An item element performs one operation, in order of appearance across every import:

| [INDEX] | [ATTRIBUTE]                | [EFFECT]                                                                                |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Include`                  | Adds items with the metadata on the element, and duplicates stay outside a target       |
|  [02]   | `Exclude`                  | Subtracts from the `Include` of the same element only, beside `Update` it fails `MSB4066` |
|  [03]   | `Remove`                   | Removes matching items of that type, `Remove="@(Type)"` clears it                       |
|  [04]   | `Update`                   | Sets metadata on items that already exist at that point, unmatched specs report nothing |
|  [05]   | `MatchOnMetadata`          | `Remove="@(Other)"` matches on the named metadata, `MatchOnMetadataOptions="PathLike"`  |
|  [06]   | `KeepMetadata`, `RemoveMetadata` | In a target, filters the metadata copied from the source items, definition defaults stay |
|  [07]   | `KeepDuplicates="false"`   | In a target, skips an item with the identity and metadata of an existing item           |

- The SDK default globs run after `Directory.Build.props`, an `Update` there matches nothing, and an `Include` of a file a default glob already matches fails `Compile` with `NETSDK1022`
- Inside a target, `Update` applies its metadata to every item of the type, and `Condition="'%(Identity)' == 'name'"` on an item element selects one item
- Outside a target, an item condition reads properties and `@(Item)` lists only, `%(Custom)` fails with `MSB4191` and `%(Filename)` with `MSB4190`, and only the transform `@(Item->'%(Meta)')` is legal
- An `ItemDefinitionGroup` sets default metadata for a type, an item's own metadata wins, and `@(Item)` in a definition fails with `MSB4164`
- An `Include` path resolves against the project directory even in an imported file, and `$(MSBuildThisFileDirectory)` roots a path beside the importing file
- Every item carries `%(FullPath)`, `%(RootDir)`, `%(Filename)`, `%(Extension)`, `%(RelativeDir)`, `%(RecursiveDir)`, `%(Identity)`, and `%(DefiningProjectDirectory)`

```xml
<ItemGroup>
  <Source Include="**/*.cs" Exclude="Generated/**;Tests/**" />
  <Source Include="Generated/*.cs" Kind="generated" />
  <Source Remove="Legacy.cs" />
  <Source Update="Generated/*.cs" Owner="tool" />
  <Using Include="Microsoft.Extensions.Logging" Condition="'@(PackageReference->WithMetadataValue('Identity', 'Microsoft.Extensions.Logging'))' != ''" />
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

| [INDEX] | [EXPRESSION]                                        | [RESULT]                                                        |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `@(Item->'%(Filename)%(Extension)')`                | Transform, one string per item                                  |
|  [02]   | `@(Item->'%(Filename)', ', ')`                      | Transform joined with a separator                               |
|  [03]   | `@(Item->WithMetadataValue('Kind', 'generated'))`   | Items with that metadata value, case-insensitive                |
|  [04]   | `@(Item->AnyHaveMetadataValue('Kind', 'generated'))`| `true` or `false`, usable as a bare condition                   |
|  [05]   | `@(Item->Metadata('Kind'))`                         | The metadata values, source metadata kept                       |
|  [06]   | `@(Item->Distinct())`, `->Count()`, `->Reverse()`   | Identities without duplicates, the count, the reversed list     |
|  [07]   | `@(Item->ClearMetadata())`                          | Identities with every metadata value removed                    |
|  [08]   | `@(Item->HasMetadata('Kind'))`, `->Exists()`        | Items with that metadata name, items present on disk            |

`%()` outside a target is legal only inside a transform, and task and target batching run inside targets:
- `@(Item->'%(Meta)')` is the one `%()` form an evaluation-time item, condition, or property accepts
- See `dotnet-msbuild-execution` for task batching, target batching, `MSB4096`, and `MSB4116`

Prove items with `dotnet msbuild <project> -getItem:Type`, which prints every item with its well-known metadata as JSON, and `jq -r '.Items.Type[].Identity'` lists identities.

## [05]-[FILE_PLACEMENT]

Each declaration has one owning file, chosen by what it must read and who must override it:

| [INDEX] | [DECLARATION]                                                                 | [FILE]                              |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | Repository root paths, `ArtifactsPath`, `UseArtifactsOutput`, `BaseIntermediateOutputPath` | `Directory.Build.props` |
|  [02]   | Defaults a project overrides: `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel` | `Directory.Build.props` |
|  [03]   | Classification from `MSBuildProjectName` or `MSBuildProjectDirectory`         | `Directory.Build.props`             |
|  [04]   | Items every project gets: analyzer `PackageReference`, `Using`                | `Directory.Build.props`             |
|  [05]   | Values derived from `TargetFramework`, `OutputType`, or another body property | `Directory.Build.targets`           |
|  [06]   | `Using` conditioned on a `PackageReference`, `Update` on SDK glob items       | `Directory.Build.targets`           |
|  [07]   | Custom targets and `DependsOn` extensions                                     | `Directory.Build.targets`           |
|  [08]   | `TargetFramework`, `OutputType`, `ArtifactsPivots`, `PackageReference`, `ProjectReference` | Project file           |
|  [09]   | `ArtifactsProjectName` for `bin`, `obj`, and `publish`                        | `Directory.Build.props`             |
|  [10]   | Package versions, `PackageVersion`, `GlobalPackageReference`                  | `Directory.Packages.props`          |
|  [11]   | Analyzer severity, `build_check.*` severity and options                       | `.editorconfig`                     |
|  [12]   | Default command-line switches                                                 | `Directory.Build.rsp`               |
|  [13]   | Machine-local overrides                                                       | `$(MSBuildProjectFullPath).user`    |
|  [14]   | Properties and targets a package gives its consumers                          | The package `build/` files          |

- `ArtifactsPath` and `UseArtifactsOutput` in a project file fail with `NETSDK1199`, `BaseIntermediateOutputPath` there warns `MSB3539`, and either artifacts property alone enables the layout `artifacts/<type>/<project>/<pivot>` beside `Directory.Build.props`
- The pivot is the lowercase configuration, then `_<tfm>` for a multi-targeting project, then `_<rid>` when `RuntimeIdentifier` is set, `release_net10.0_osx-arm64`, and packages go to `artifacts/package/<configuration>`
- Give `ArtifactsPath` no trailing slash, the SDK appends its own separator, and `ArtifactsProjectName` in a project file renames `bin` and `publish` only while `obj` keeps the project name
- See `dotnet-msbuild-packaging` for `Directory.Packages.props`, nested package files, package `build/` and `buildTransitive/` layouts, and solution files

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <RepositoryRoot>$(MSBuildThisFileDirectory)</RepositoryRoot>
  <ArtifactsPath>$(RepositoryRoot).artifacts</ArtifactsPath>
</PropertyGroup>
```

MSBuild imports only the nearest `Directory.Build.props` and `Directory.Build.targets` above a project, a nested file opens with an import of the outer one, and the private property keeps nested quotes out of the condition:

```xml
<!-- tests/Directory.Build.props -->
<PropertyGroup>
  <_OuterDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))</_OuterDirectoryBuildProps>
</PropertyGroup>
<Import Project="$(_OuterDirectoryBuildProps)" Condition="'$(_OuterDirectoryBuildProps)' != ''" />
```

`Directory.Build.rsp` takes one switch per line, `#` opens a comment, and `%MSBuildThisFileDirectory%` is the only expansion:

```text
# Applied to every dotnet and msbuild command-line build below this directory
-nodeReuse:false
-bl:%MSBuildThisFileDirectory%.artifacts/logs/build-{}
```

`.editorconfig` owns analyzer severity as `dotnet_diagnostic.<ID>.severity`, BuildCheck reads it from the project directory upward, a key applies only under a section header, and `BC0201` and `BC0202` share one `AllowUninitializedPropertiesInConditions` value:

```ini
[*.csproj]
build_check.BC0201.severity=error
build_check.BC0202.severity=none
build_check.BC0201.AllowUninitializedPropertiesInConditions=false
build_check.BC0202.AllowUninitializedPropertiesInConditions=false
```

## [06]-[TROUBLESHOOTING]

| [INDEX] | [PROBLEM]                                  | [CAUSE]                                        | [FIX]                                    |
| :-----: | :----------------------------------------- | :--------------------------------------------- | :--------------------------------------- |
|  [01]   | `Directory.Build.props` is not imported    | The case differs on a case-sensitive volume    | Match the case exactly                   |
|  [02]   | A `Directory.Build.props` value is ignored | The project body or the SDK reassigns it later | Set it in `Directory.Build.targets`      |
|  [03]   | A `TargetFramework` condition never holds  | The `PropertyGroup` sits in a `.props` file    | Move it to `.targets` or the project     |
|  [04]   | A `-p:` value is not normalized            | Project XML cannot reassign a global property  | Derive a private property                |
|  [05]   | `Update` changes no metadata               | The item does not exist yet at that point      | Move the `Update` after the `Include`    |
|  [06]   | A property holds `@(...)` text             | Properties never read items                    | Read the list in a target                |
|  [07]   | `-getProperty` fails with `MSB1063`        | The argument is a solution                     | Point the query at one project file      |

- `dotnet msbuild <project> -pp:expanded.xml` writes every import inline with its file boundaries marked, and a search for the property shows each assignment in order
- `dotnet msbuild <project> -getProperty:A,B -getItem:Type` prints evaluated values without a build, `-t:Target` runs the target before the query, `-getTargetResult:Target` prints its returned items, and `-getResultOutputFile:file` writes the JSON to a file
- `dotnet msbuild <project> -p:TargetFramework=net10.0 -getProperty:Name` evaluates one inner build of a multi-targeting project
- `dotnet build <project> -v:diag` logs `Property reassignment: $(Name)="new" (previous value: "old")` with the file and line of each assignment
- `dotnet build <project> -check` runs BuildCheck, `BC0201` reports a read of a property that is never assigned and `BC0202` a read before its assignment
- See `dotnet-msbuild-diagnostics` for binary log capture and every `binlog` MCP query
