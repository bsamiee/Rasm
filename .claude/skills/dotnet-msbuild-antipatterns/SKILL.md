---
name: dotnet-msbuild-antipatterns
description: "Detect and fix MSBuild anti-patterns in project and build files. USE WHEN asked to review, audit, lint, clean up, or code-review a .csproj/.vbproj/.fsproj/.props/.targets/.proj (or Directory.Build.props/.targets) file, when asked 'is this project file correct?' or 'what's wrong with my build file?', or when hunting subtle build bugs caused by how a project is authored. Each anti-pattern has a symptom and a concrete BAD→GOOD fix."
---

# [DOTNET_MSBUILD_ANTIPATTERNS]

Use this catalog when scanning project files for improvements, and correcting common MSBuild anti-patterns. Each entry follows the format:
- SMELL: What to look for
- WHY: Impact on builds, maintainability, or correctness
- FIX: Concrete transformation

## [AP-01]-[EXEC_FOR_BUILTIN_TASKS]

- SMELL: `<Exec Command="mkdir ..." />`, `<Exec Command="copy ..." />`, `<Exec Command="del ..." />`
- WHY: Built-in tasks are cross-platform, support incremental build, emit structured logging, and handle errors consistently. `<Exec>` is opaque to MSBuild.

```xml
<!-- BAD -->
<Target Name="PrepareOutput">
  <Exec Command="mkdir $(OutputPath)logs" />
  <Exec Command="copy config.json $(OutputPath)" />
  <Exec Command="del $(IntermediateOutputPath)*.tmp" />
</Target>

<!-- GOOD -->
<Target Name="PrepareOutput">
  <MakeDir Directories="$(OutputPath)logs" />
  <Copy SourceFiles="config.json" DestinationFolder="$(OutputPath)" />
  <Delete Files="@(TempFiles)" />
</Target>
```

Built-in task alternatives:

| [INDEX] | [SHELL_COMMAND]    | [MSBUILD_TASK]           |
| :-----: | :----------------- | :----------------------- |
|  [01]   | `mkdir`            | `<MakeDir>`              |
|  [02]   | `copy` / `cp`      | `<Copy>`                 |
|  [03]   | `del` / `rm`       | `<Delete>`               |
|  [04]   | `move` / `mv`      | `<Move>`                 |
|  [05]   | `echo text > file` | `<WriteLinesToFile>`     |
|  [06]   | `touch`            | `<Touch>`                |
|  [07]   | `xcopy /s`         | `<Copy>` with item globs |

## [AP-02]-[UNQUOTED_CONDITION_EXPRESSIONS]

- SMELL: `Condition="$(Foo) == Bar"` — either side of a comparison is unquoted.
- WHY: If the property is empty or contains spaces/special characters, the condition evaluates incorrectly or throws a parse error. MSBuild requires single-quoted strings for reliable comparisons.

```xml
<!-- BAD -->
<PropertyGroup Condition="$(Configuration) == Release">
  <Optimize>true</Optimize>
</PropertyGroup>

<!-- GOOD -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <Optimize>true</Optimize>
</PropertyGroup>
```

RULE: Always quote both sides of `==` and `!=` comparisons with single quotes.

## [AP-03]-[HARDCODED_ABSOLUTE_PATHS]

- SMELL: Paths like `C:\tools\`, `D:\packages\`, `/usr/local/bin/` in project files.
- WHY: Breaks on other machines, CI environments, and other operating systems. Not relocatable.

```xml
<!-- BAD -->
<PropertyGroup>
  <ToolPath>C:\tools\mytool\mytool.exe</ToolPath>
</PropertyGroup>
<Import Project="C:\repos\shared\common.props" />

<!-- GOOD -->
<PropertyGroup>
  <ToolPath>$(MSBuildThisFileDirectory)tools\mytool\mytool.exe</ToolPath>
</PropertyGroup>
<Import Project="$(RepoRoot)eng\common.props" />
```

Preferred path properties:

| [INDEX] | [PROPERTY]                                       | [MEANING]                                     |
| :-----: | :----------------------------------------------- | :-------------------------------------------- |
|  [01]   | `$(MSBuildThisFileDirectory)`                    | Directory of the current .props/.targets file |
|  [02]   | `$(MSBuildProjectDirectory)`                     | Directory of the .csproj                      |
|  [03]   | `$([MSBuild]::GetDirectoryNameOfFileAbove(...))` | Walk up to find a marker file                 |
|  [04]   | `$([MSBuild]::NormalizePath(...))`               | Combine and normalize path segments           |

## [AP-04]-[RESTATING_SDK_DEFAULTS]

- SMELL: Properties set to values that the .NET SDK already provides by default.
- WHY: Adds noise, hides intentional overrides, and makes it harder to identify what's actually customized. When defaults change in newer SDKs, the redundant properties may silently pin old behavior.

```xml
<!-- BAD: All of these are already the default -->
<PropertyGroup>
  <OutputType>Library</OutputType>
  <EnableDefaultItems>true</EnableDefaultItems>
  <EnableDefaultCompileItems>true</EnableDefaultCompileItems>
  <RootNamespace>MyLib</RootNamespace>       <!-- matches project name -->
  <AssemblyName>MyLib</AssemblyName>         <!-- matches project name -->
  <AppendTargetFrameworkToOutputPath>true</AppendTargetFrameworkToOutputPath>
</PropertyGroup>

<!-- GOOD: Only non-default values -->
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

## [AP-05]-[REFERENCE_WITH_HINTPATH_FOR_NUGET_PACKAGES]

- SMELL: `<Reference Include="..." HintPath="..\packages\SomePackage\lib\..." />`
- WHY: This is the legacy `packages.config` pattern. It doesn't support transitive dependencies, version conflict resolution, or automatic restore. The `packages/` folder must be committed or restored separately.

```xml
<!-- BAD -->
<ItemGroup>
  <Reference Include="Newtonsoft.Json">
    <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\netstandard2.0\Newtonsoft.Json.dll</HintPath>
  </Reference>
</ItemGroup>

<!-- GOOD -->
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

## [AP-06]-[COPY_PASTED_PROPERTIES_ACROSS_MULTIPLE_CSPROJ_FILES]

- SMELL: The same `<PropertyGroup>` block appears in 3+ project files.
- WHY: Maintenance burden — a change must be made in every file. Inconsistencies creep in over time.

```xml
<!-- BAD: Repeated in every .csproj -->
<!-- ProjectA.csproj, ProjectB.csproj, ProjectC.csproj all have: -->
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>

<!-- GOOD: Define once in Directory.Build.props at the repo/src root -->
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

See `directory-build-organization` skill for full guidance on structuring `Directory.Build.props` / `Directory.Build.targets`.

## [AP-07]-[MONOLITHIC_TARGETS]

- SMELL: A single `<Target>` with 50+ lines doing multiple unrelated things.
- WHY: Can't skip individual steps via incremental build, hard to debug, hard to extend, and the target name becomes meaningless.

```xml
<!-- BAD -->
<Target Name="PrepareRelease" BeforeTargets="Build">
  <WriteLinesToFile File="version.txt" Lines="$(Version)" Overwrite="true" />
  <Copy SourceFiles="LICENSE" DestinationFolder="$(OutputPath)" />
  <Exec Command="signtool sign /f cert.pfx $(OutputPath)*.dll" />
  <MakeDir Directories="$(OutputPath)docs" />
  <Copy SourceFiles="@(DocFiles)" DestinationFolder="$(OutputPath)docs" />
  <!-- ... 30 more lines ... -->
</Target>

<!-- GOOD: Single-responsibility targets -->
<Target Name="WriteVersionFile" BeforeTargets="CoreCompile"
        Inputs="$(MSBuildProjectFile)" Outputs="$(IntermediateOutputPath)version.txt">
  <WriteLinesToFile File="$(IntermediateOutputPath)version.txt" Lines="$(Version)" Overwrite="true" />
</Target>

<Target Name="CopyLicense" AfterTargets="Build">
  <Copy SourceFiles="LICENSE" DestinationFolder="$(OutputPath)" SkipUnchangedFiles="true" />
</Target>

<Target Name="SignAssemblies" AfterTargets="Build" DependsOnTargets="CopyLicense"
        Condition="'$(SignAssemblies)' == 'true'">
  <Exec Command="signtool sign /f cert.pfx %(AssemblyFiles.Identity)" />
</Target>
```

## [AP-08]-[CUSTOM_TARGETS_MISSING_INPUTS_AND_OUTPUTS]

Custom targets must specify `Inputs` and `Outputs` attributes so MSBuild can skip them when up-to-date. Without both attributes, the target runs on every build.

- SMELL: `<Target Name="MyTarget" BeforeTargets="Build">` with no `Inputs` / `Outputs` attributes.
- WHY: The target runs on every build, even when nothing changed. This defeats incremental build and slows down no-op builds.

```xml
<!-- BAD: Runs every time -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile">
  <WriteLinesToFile File="$(IntermediateOutputPath)BuildInfo.g.cs"
                    Lines="// Generated at $(Version)" Overwrite="true" />
</Target>

<!-- GOOD: Skipped when up-to-date -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile"
        Inputs="$(MSBuildProjectFile)" Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <WriteLinesToFile File="$(IntermediateOutputPath)BuildInfo.g.cs"
                    Lines="// Generated at $(Version)" Overwrite="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
    <Compile Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>
```

Key points:
- `Inputs` should include `$(MSBuildProjectFile)` plus any source files that drive generation
- `Outputs` should use `$(IntermediateOutputPath)` so generated files go in `obj/` and are managed by MSBuild
- `FileWrites` registration ensures `dotnet clean` removes the generated file
- `Compile` inclusion adds the generated file to compilation without requiring it at evaluation time

See `incremental-build` skill for deep guidance on Inputs/Outputs, FileWrites, and up-to-date checks.

## [AP-09]-[SETTING_DEFAULTS_IN_TARGETS_INSTEAD_OF_PROPS]

- SMELL: `<PropertyGroup>` with default values inside a `.targets` file.
- WHY: `.targets` files are imported late (after project files). By the time they set defaults, other `.targets` files may have already used the empty/undefined value. `.props` files are imported early and are the correct place for defaults.

```xml
<!-- BAD: custom.targets -->
<PropertyGroup>
  <MyToolVersion>2.0</MyToolVersion>
</PropertyGroup>
<Target Name="RunMyTool">
  <Exec Command="mytool --version $(MyToolVersion)" />
</Target>

<!-- GOOD: Split into .props (defaults) + .targets (logic) -->
<!-- custom.props (imported early) -->
<PropertyGroup>
  <MyToolVersion Condition="'$(MyToolVersion)' == ''">2.0</MyToolVersion>
</PropertyGroup>

<!-- custom.targets (imported late) -->
<Target Name="RunMyTool">
  <Exec Command="mytool --version $(MyToolVersion)" />
</Target>
```

RULE: `.props` = defaults and settings (evaluated early). `.targets` = build logic and targets (evaluated late).

## [AP-10]-[IMPORT_WITHOUT_EXISTS_GUARD]

- SMELL: `<Import Project="some-file.props" />` without a `Condition="Exists('...')"` check.
- WHY: If the file doesn't exist (not yet created, wrong path, deleted), the build fails with a confusing error. Optional imports should always be guarded.

```xml
<!-- BAD -->
<Import Project="$(RepoRoot)eng\custom.props" />

<!-- GOOD: Guard optional imports -->
<Import Project="$(RepoRoot)eng\custom.props" Condition="Exists('$(RepoRoot)eng\custom.props')" />

<!-- ALSO GOOD: Sdk attribute imports don't need guards (they're required by design) -->
<Project Sdk="Microsoft.NET.Sdk">
```

Exception — required imports: Imports that are required for the build to work correctly should fail fast — don't guard those. Guard imports that are optional or environment-specific (e.g., local developer overrides, CI-specific settings).

Exception — NuGet package forwarders: `.props`/`.targets` files inside a NuGet package's per-TFM `build/` or `buildTransitive/` folder routinely import a sibling file under `buildTransitive/<tfm>/…` without an `Exists()` guard. These are a package contract: the target file is guaranteed to be present in the restored package, even if it doesn't appear in the source tree at that relative path. The package layout is typically produced by:
- A custom `.nuspec` with per-TFM `<file>` entries — e.g. `<file src="buildTransitive\common\MyAdapter.props" target="buildTransitive\net8.0\MyAdapter.props" />` — that copy files from a single source folder (such as `buildTransitive/common/`) into per-TFM subfolders at pack time, or
- `<None Update="...">` / `<Content Include="...">` items in the `.csproj` with a per-TFM `<PackagePath>` (e.g. `<PackagePath>buildTransitive/net8.0/</PackagePath>`), declared once per target TFM, or SDK conventions (e.g. `IncludeBuildOutput`, `BuildOutputTargetFolder`) that place built outputs under `build/<tfm>/`.

Before flagging an unguarded `<Import>` inside a `build/` or `buildTransitive/` folder, resolve it against the packed layout — read every `*.nuspec` in the project directory and its immediate parent directory (shared nuspecs are common in mono-repos; do not walk further up), and any `<PackagePath>` metadata on `<None>`/`<Content>` items in the `.csproj`. Only flag if the target path is missing from both the source tree and the projected package layout. The `dotnet-msbuild/extension-points` skill — Source tree vs packed layout — documents the full cross-check procedure.

Forwarding `buildTransitive/` → `build/`: forward through the sibling `build/*.props` / `build/*.targets` file (not directly to `buildMultiTargeting/`); when `build/` is per-TFM (`build/<tfm>/`), include the TFM segment derived from the file's own folder (not `$(TargetFramework)`), or transitive consumers hit `MSB4019`. See the `extension-points` skill — Forwarding chain — for the rule and derivation expression.

## [AP-11]-[BACKSLASHES_IN_PATHS]

- SMELL: Backslash path separators in `.props`/`.targets` files meant to run cross-platform.
- Note: `$(MSBuildThisFileDirectory)` already ends with a platform-appropriate separator, so `$(MSBuildThisFileDirectory)tools/mytool` works on both platforms.

Where this is a real bug [ERROR] — paths that MSBuild does not route through its path normalizer:
- Raw shell strings inside `<Exec Command="...\tools\foo.exe ..." />` — passed verbatim to `bash`/`sh` on Unix, which treats `\` as an escape.
- Backslash-delimited paths inside CDATA blocks, embedded in source files written by `<WriteLinesToFile>`, or constructed for non-MSBuild consumers (custom scripts, response files, environment variables).
- Paths handed to custom tasks that call OS file APIs directly without going through MSBuild path utilities.

Where this is only a style preference [STYLE] — paths that go through MSBuild's evaluator (`<Import Project="...">`, file-path properties consumed by built-in tasks like `<Copy>`/`<MakeDir>`/`<Delete>`, item `Include=`/`Exclude=` globs):

MSBuild's evaluator normalizes `\` → `/` on Unix-like systems before resolving the path. See `FileUtilities.MaybeAdjustFilePath` and `ConvertToUnixSlashes` in [`microsoft/msbuild` `src/Framework/FileUtilities.cs`](https://github.com/dotnet/msbuild/blob/main/src/Framework/FileUtilities.cs). So `<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />` resolves correctly on Linux/macOS today. Forward slashes are still preferred for consistency, but the import will not break and existing backslash-style imports should not be flagged as [ERROR].

```xml
<!-- [ERROR]: \ in raw shell string breaks on Linux/macOS -->
<Exec Command="$(MSBuildThisFileDirectory)tools\release\sign.exe $(OutputPath)" />

<!-- [STYLE]: \ in Import is normalized on Unix, but / is nicer -->
<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />

<!-- [RECOMMENDED] in new code -->
<Import Project="$(MSBuildThisFileDirectory)../../build/common.props" />
```

Verification rule: Before flagging a backslash path as [ERROR], ask "does this string flow through MSBuild's evaluator, or is it handed verbatim to a non-MSBuild consumer?" Only the second case is a correctness defect.

## [AP-12]-[UNCONDITIONAL_PROPERTY_OVERRIDE_IN_MULTIPLE_SCOPES]

- SMELL: A property set unconditionally in both `Directory.Build.props` and a `.csproj` — last write wins silently.
- WHY: Hard to trace which value is actually used. Makes the build fragile and confusing for anyone reading the project files.

```xml
<!-- BAD: Directory.Build.props sets it, csproj silently overrides -->
<!-- Directory.Build.props -->
<PropertyGroup>
  <OutputPath>bin\custom\</OutputPath>
</PropertyGroup>
<!-- MyProject.csproj -->
<PropertyGroup>
  <OutputPath>bin\other\</OutputPath>
</PropertyGroup>

<!-- GOOD: Use a condition so overrides are intentional -->
<!-- Directory.Build.props -->
<PropertyGroup>
  <OutputPath Condition="'$(OutputPath)' == ''">bin\custom\</OutputPath>
</PropertyGroup>
<!-- MyProject.csproj can now intentionally override or leave the default -->
```

## [AP-13]-[USING_EXEC_FOR_STRING_PATH_OPERATIONS]

- SMELL: `<Exec Command="echo $(Var) | sed ..." />` or `<Exec Command="powershell -c ..." />` for simple string manipulation.
- WHY: Shell-dependent, not cross-platform, slower than property functions, and the result is hard to capture back into MSBuild properties.

```xml
<!-- BAD -->
<Target Name="GetCleanVersion">
  <Exec Command="echo $(Version) | sed 's/-preview//'" ConsoleToMSBuildProperty="CleanVersion" />
</Target>

<!-- GOOD: Property function -->
<PropertyGroup>
  <CleanVersion>$(Version.Replace('-preview', ''))</CleanVersion>
  <HasPrerelease>$(Version.Contains('-'))</HasPrerelease>
  <LowerName>$(AssemblyName.ToLowerInvariant())</LowerName>
</PropertyGroup>

<!-- GOOD: Path operations -->
<PropertyGroup>
  <NormalizedOutput>$([MSBuild]::NormalizeDirectory($(OutputPath)))</NormalizedOutput>
  <ToolPath>$([System.IO.Path]::Combine($(MSBuildThisFileDirectory), 'tools', 'mytool.exe'))</ToolPath>
</PropertyGroup>
```

## [AP-14]-[MIXING_INCLUDE_AND_UPDATE_FOR_THE_SAME_ITEM_TYPE_IN_ONE_ITEMGROUP]

- SMELL: Same `<ItemGroup>` has both `<Compile Include="...">` and `<Compile Update="...">`.
- WHY: `Update` acts on items already in the set. If `Include` hasn't been processed yet (evaluation order), `Update` may not find the item. Separating them avoids subtle ordering bugs.

```xml
<!-- BAD -->
<ItemGroup>
  <Compile Include="Generated\Extra.cs" />
  <Compile Update="Generated\Extra.cs" CopyToOutputDirectory="Always" />
</ItemGroup>

<!-- GOOD -->
<ItemGroup>
  <Compile Include="Generated\Extra.cs" />
</ItemGroup>
<ItemGroup>
  <Compile Update="Generated\Extra.cs" CopyToOutputDirectory="Always" />
</ItemGroup>
```

## [AP-15]-[REDUNDANT_PROJECTREFERENCE_TO_TRANSITIVELY_REFERENCED_PROJECTS]

- SMELL: A project references both `Core` and `Utils`, but `Core` already depends on `Utils`.
- WHY: Adds unnecessary coupling, makes the dependency graph harder to understand, and can cause ordering issues in large builds. MSBuild resolves transitive references automatically.
- Caveat: If you need to use types from `Utils` directly (not just transitively), the explicit reference is appropriate. But verify whether the direct dependency is actually needed.

```xml
<!-- BAD -->
<ItemGroup>
  <ProjectReference Include="..\Core\Core.csproj" />
  <ProjectReference Include="..\Utils\Utils.csproj" />  <!-- Core already references Utils -->
</ItemGroup>

<!-- GOOD: Only direct dependencies -->
<ItemGroup>
  <ProjectReference Include="..\Core\Core.csproj" />
</ItemGroup>
```

## [AP-16]-[SIDE_EFFECTS_DURING_PROPERTY_EVALUATION]

- SMELL: Property functions that write files, make network calls, or modify state during `<PropertyGroup>` evaluation.
- WHY: Property evaluation happens during the evaluation phase, which can run multiple times (e.g., during design-time builds in Visual Studio). Side effects are unpredictable and can corrupt state.

```xml
<!-- BAD: File write during evaluation -->
<PropertyGroup>
  <Timestamp>$([System.IO.File]::WriteAllText('stamp.txt', 'built'))</Timestamp>
</PropertyGroup>

<!-- GOOD: Side effects belong in targets -->
<Target Name="WriteTimestamp" BeforeTargets="Build">
  <WriteLinesToFile File="stamp.txt" Lines="built" Overwrite="true" />
</Target>
```

## [AP-17]-[PLATFORM_SPECIFIC_EXEC_WITHOUT_OS_CONDITION]

- SMELL: `<Exec Command="chmod +x ..." />` or `<Exec Command="cmd /c ..." />` without an OS condition.
- WHY: Fails on the wrong platform. If the project is cross-platform, guard platform-specific commands.

```xml
<!-- BAD: Fails on Windows -->
<Target Name="MakeExecutable" AfterTargets="Build">
  <Exec Command="chmod +x $(OutputPath)mytool" />
</Target>

<!-- GOOD: OS-guarded -->
<Target Name="MakeExecutable" AfterTargets="Build"
        Condition="!$([MSBuild]::IsOSPlatform('Windows'))">
  <Exec Command="chmod +x $(OutputPath)mytool" />
</Target>
```

## [AP-18]-[PROPERTY_CONDITIONED_ON_TARGETFRAMEWORK_IN_PROPS_FILES]

- SMELL: `<PropertyGroup Condition="'$(TargetFramework)' == '...'">` or `<Property Condition="'$(TargetFramework)' == '...'">` in `Directory.Build.props` or any `.props` file imported before the project body.
- WHY: `$(TargetFramework)` is NOT reliably available in `Directory.Build.props` or any `.props` file imported before the project body. It is only set that early for multi-targeting projects, which receive `TargetFramework` as a global property from the outer build. Single-targeting projects (using singular `<TargetFramework>`) set it in the project body, which is evaluated after `.props`. This means property conditions on `$(TargetFramework)` in `.props` files silently fail for single-targeting projects — the condition never matches because the property is empty. This applies to both `<PropertyGroup Condition="...">` and individual `<Property Condition="...">` elements.

For a detailed explanation of MSBuild's evaluation and execution phases, see [Build process overview](https://learn.microsoft.com/en-us/visualstudio/msbuild/build-process-overview).

```xml
<!-- BAD: In Directory.Build.props — TargetFramework may be empty here -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- ALSO BAD: Condition on the property itself has the same problem -->
<PropertyGroup>
  <DefineConstants Condition="'$(TargetFramework)' == 'net8.0'">$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- GOOD: In Directory.Build.targets — TargetFramework is always available -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- ALSO GOOD: In the project file itself -->
<!-- MyProject.csproj -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
```

[CAUTION] Item and Target conditions are NOT affected. This restriction applies ONLY to property conditions (`<PropertyGroup Condition="...">` and `<Property Condition="...">`). Item conditions (`<ItemGroup Condition="...">`) and Target conditions in `.props` files are SAFE because items and targets evaluate after all properties (including those set in the project body) have been evaluated. This includes `PackageVersion` items in `Directory.Packages.props`, `PackageReference` items in `Directory.Build.props`, and any other item types.

Do NOT flag the following patterns — they are correct:

```xml
<!-- OK in Directory.Build.props — ItemGroup conditions evaluate late -->
<ItemGroup Condition="'$(TargetFramework)' == 'net472'">
  <PackageReference Include="System.Memory" />
</ItemGroup>

<!-- OK in Directory.Packages.props — PackageVersion items evaluate late -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
</ItemGroup>

<!-- OK — Individual item conditions also evaluate late -->
<ItemGroup>
  <PackageReference Include="System.Memory" Condition="'$(TargetFramework)' == 'net472'" />
</ItemGroup>
```

## [AP-19]-[MSBUILD_FORKING_WITH_PATH_NEUTRAL_GLOBAL_PROPERTIES]

- SMELL: A target uses the `<MSBuild>` task to build or publish a project, passing extra `Properties` that don't change that project's output path.
- WHY: An MSBuild project instance is identified by its path plus its global properties. Passing an extra global property creates a distinct instance of the target project — `(project, {_IsPublishing=true})` — that still resolves to the same `OutputPath`/`IntermediateOutputPath` as the instance the solution/graph already builds, `(project, {})`. That project is then built twice, and in a parallel/graph build the two instances can write the same files concurrently (PDBs, `*.sourcelink` and other NativeAOT intermediates, `project.assets.json`), producing `The process cannot access the file because it is being used by another process` or intermittent file-lock failures. This applies whether the offending `<MSBuild>` call is in the target project itself or in some other project in the same build. Use the `check-bin-obj-clash` skill to confirm two evaluations of that project differ only by a path-neutral property while sharing an output path.

Two common shapes:

```xml
<!-- (a) the SAME project re-invokes itself (publish-on-build) -->
<MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />

<!-- (b) a consumer invokes Build/Publish on ANOTHER project it consumes (e.g. a test or layout project publishing a tool) -->
<MSBuild Projects="..\tool\tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
```

```xml
<!-- BAD (a): forks a second instance (path + {_IsPublishing=true}) that shares this project's bin/obj -->
<Target Name="PublishOnBuild" AfterTargets="Build">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>
```

```xml
<!-- GOOD (a): set the flag as a normal (non-global) property and run the target in the SAME instance -->
<PropertyGroup>
  <!-- Capture whether the entry point already invoked publish (it sets _IsPublishing as a global prop). -->
  <_PublishWasInvokedDirectly Condition="'$(_IsPublishing)' == 'true'">true</_PublishWasInvokedDirectly>
  <_IsPublishing>true</_IsPublishing>
</PropertyGroup>

<Target Name="PublishOnBuild"
        AfterTargets="Build"
        DependsOnTargets="Publish"
        Condition="'$(_PublishWasInvokedDirectly)' != 'true'" />
```

For (a), the static property keeps everything in one instance (one output path, nothing to race); running `Publish` via `DependsOnTargets` (or `CallTarget`) reuses that instance instead of forking. The `_PublishWasInvokedDirectly` guard breaks the target cycle when publish is the entry point (e.g. `dotnet publish`, which sets `_IsPublishing=true` as a global property and would otherwise re-trigger `PublishOnBuild`).

```xml
<!-- BAD (b): the consumer forks a publish instance of the tool that races the tool's own build in the graph -->
<MSBuild Projects="..\tool\tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />

<!-- GOOD (b): apply the (a) fix in tool.csproj so the tool publishes its OWN build; reference it only to sequence it, never to re-publish -->
<ItemGroup>
  <ProjectReference Include="..\tool\tool.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
<!-- The consumer then reads the tool's publish dir; it does not invoke Publish on tool. -->
```

For (b), the consumer must not fork the producer with path-neutral global properties. Let the producer publish itself (one instance), reference it only to sequence the build, and read its output.

When extra global properties ARE fine: only when the output path encodes the discriminator (`RuntimeIdentifier`, `TargetFramework`, `Configuration`, `Platform`) so each instance writes to a distinct directory. If you must invoke a project with a path-neutral property, give that build its own `BaseIntermediateOutputPath`/output path so it can't collide.

## [AP-20]-[SETTARGETFRAMEWORK_METADATA_ON_A_PROJECTREFERENCE_TO_A_NON-MULTI-TARGETING_PROJECT]

- SMELL: A `<ProjectReference>` carries `SetTargetFramework="TargetFramework=net8.0"` (or similar) metadata, the referenced project is single-targeting (uses singular `<TargetFramework>`, not `<TargetFrameworks>`), and the injected TFM equals the TFM the project already targets.
- WHY: `SetTargetFramework` injects `TargetFramework` as a global property on the referenced project's build. That mechanism exists so a consumer can pick one specific TFM of a multi-targeting project — different TFM values produce different output paths, so each build is distinct and safe.

```xml
<!-- BAD: Tool.csproj single-targets net8.0 and we inject that SAME net8.0 — redundant AND harmful -->
<ItemGroup>
  <ProjectReference Include="..\Tool\Tool.csproj" SetTargetFramework="TargetFramework=net8.0" />
</ItemGroup>
```

For a single-targeting project, injecting the TFM it already targets is path-neutral: the project already resolves to `bin\<config>\net8.0\` and `obj\<config>\net8.0\` on its own, so the extra global property doesn't change the output path — it only creates a distinct MSBuild project instance `(project, {TargetFramework=net8.0})`. Meanwhile the solution/graph builds that same project as `(project, {})` with no global properties. Both instances resolve to the same `OutputPath`/`IntermediateOutputPath`, so the project is built twice and the two instances write the same files (assemblies, PDBs, `project.assets.json`, etc.). Under a parallel build this is a classic bin/obj clash — `The process cannot access the file because it is being used by another process` or intermittent, retry-flaky failures. (Injecting a different TFM changes the output path and is a legitimate override — see below.)

Note the healthy contrast: the P2P protocol itself does not inject `TargetFramework` when it sees a non-multi-targeting reference — it correctly omits the global property. `SetTargetFramework` overrides that safe default and is what reintroduces the clash. Use the `check-bin-obj-clash` skill to confirm two evaluations of the referenced project differ only by a path-neutral `TargetFramework` global property while sharing an output path.

```xml
<!-- GOOD: single-targeting reference needs no SetTargetFramework — just reference it -->
<ItemGroup>
  <ProjectReference Include="..\Tool\Tool.csproj" />
</ItemGroup>
```

When `SetTargetFramework` IS appropriate:
1. Multi-targeting reference — the referenced project is multi-targeting (`<TargetFrameworks>`) and you deliberately need to consume a specific TFM. Each TFM has its own output path, so the forked instance doesn't collide.
2. Deliberately overriding a single-targeting project's TFM to a different value — you can use `SetTargetFramework` on a single-targeting reference to build it under a TFM other than the one it declares. This is only valid when the passed-in TFM differs from what the project single-targets: because the injected `TargetFramework` then changes the output path (`obj\<config>\<different-tfm>\`), the instance no longer collides with the `(project, {})` build. It is only the redundant case — passing the same TFM the project already targets (path-neutral) — that causes the clash.

Related: referencing a framework-incompatible project. Independently of the clash above, whenever the referencing and referenced projects target incompatible frameworks (e.g. a `.NETFramework` project referencing a `.NETCoreApp` project, or vice-versa) — regardless of whether either side is single- or multi-targeting — you must set both:
- `SkipGetTargetFrameworkProperties="true"` — bypass the P2P `GetTargetFrameworkProperties` negotiation, which would otherwise fail because the frameworks aren't compatible, and
- `ReferenceOutputAssembly="false"` — because an assembly built for an incompatible framework can't be consumed as a reference; you only want to trigger/sequence the build, not reference its output.

```xml
<!-- OK: .NETFramework project builds an incompatible .NETCoreApp tool without referencing its assembly -->
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  ReferenceOutputAssembly="false" />
```

[CAUTION] Prevent the referencing project's `TargetFramework` from leaking. When `SkipGetTargetFrameworkProperties="true"` bypasses the negotiation, nothing stops the referencing project's own `TargetFramework` global property (present whenever the referencing project is being built for a specific TFM — e.g. it is multi-targeting) from flowing down into the referenced project. If it flows into a single-targeting referenced project, that project builds under the wrong TFM (and to a different, wrong output path). Guard against it one of two ways:
- set `SetTargetFramework="TargetFramework=<tfm>"` to explicitly pin the referenced build's TFM (also required for multi-targeting references), or
- for a single-targeting referenced project you want to build as-declared, set `UndefineProperties="TargetFramework"` to strip the inherited global property so the project uses its own `<TargetFramework>`.

```xml
<!-- OK: strip the referencing project's TargetFramework so the single-targeting tool builds as it declares -->
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  UndefineProperties="TargetFramework"
                  ReferenceOutputAssembly="false" />
```

Add `SetTargetFramework` on top of these only if you also need to pin the referenced build to a specific TFM (a multi-targeting project, or a single-targeting project you're overriding to a different TFM per case 2 above). Use `SetTargetFramework` or `UndefineProperties="TargetFramework"`, not both — the former sets the property, the latter removes it.
