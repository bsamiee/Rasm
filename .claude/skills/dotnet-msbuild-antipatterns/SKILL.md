---
name: dotnet-msbuild-antipatterns
description: "Use when reviewing, auditing, or fixing a .csproj, .props, or .targets file, or when a build defect traces to how the file is authored."
---

# [DOTNET_MSBUILD_ANTIPATTERNS]

Use this catalog when you review project and build files. Each entry has:
- SMELL: What to look for
- WHY: The effect on correctness, incremental build, or maintenance
- A `BAD` and `GOOD` example

## [AP-01]-[UNQUOTED_CONDITION_OPERANDS]

- SMELL: `Condition="$(Foo) == Bar"`. One side of a comparison has no single quotes.
- WHY: A `Condition` attribute is one expression string. If the property is empty, the unquoted operand is empty and MSBuild reports an error. Single quotes are required for empty values and for values with spaces.

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

RULE: Quote both sides of `==` and `!=` with single quotes.

## [AP-02]-[PROPERTY_DEFAULTS_IN_TARGETS_FILES]

- SMELL: A `<PropertyGroup>` with default values inside a `.targets` file.
- WHY: `.targets` files import after the project body. A file that reads the property before that point sees an empty value.

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

RULE: `.props` owns defaults and settings. `.targets` owns targets and derived properties.

## [AP-03]-[UNCONDITIONAL_PROPERTY_OVERRIDE_IN_MULTIPLE_SCOPES]

- SMELL: A property set without a condition in both `Directory.Build.props` and a `.csproj`. The last assignment wins without a message.
- WHY: A reader cannot tell which value the build uses without a binlog.

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

## [AP-04]-[PROPERTY_CONDITIONED_ON_TARGETFRAMEWORK_IN_PROPS_FILES]

- SMELL: `<PropertyGroup Condition="'$(TargetFramework)' == '...'">` or a property condition on `$(TargetFramework)` in `Directory.Build.props` or any `.props` file that imports before the project body.
- WHY: A single-targeting project sets `TargetFramework` in the project body, after `.props` evaluation. The condition compares an empty string and never matches. Only a multi-targeting inner build receives `TargetFramework` early, as a global property. See `dotnet-msbuild-evaluation` skill for evaluation order.

```xml
<!-- BAD: In Directory.Build.props — TargetFramework may be empty here -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- ALSO BAD: Condition on the property itself has the same problem -->
<PropertyGroup>
  <DefineConstants Condition="'$(TargetFramework)' == 'net10.0'">$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- GOOD: In Directory.Build.targets — TargetFramework is set by now -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- ALSO GOOD: In the project file itself, after the TargetFramework property -->
<!-- MyProject.csproj -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
```

[CAUTION] Only property conditions have this problem. Items and targets evaluate after every property, so `ItemGroup`, item, and `Target` conditions on `$(TargetFramework)` in `.props` files are correct.

Do not flag these patterns:

```xml
<!-- OK in Directory.Build.props — ItemGroup conditions evaluate late -->
<ItemGroup Condition="'$(TargetFramework)' == 'net472'">
  <PackageReference Include="System.Memory" />
</ItemGroup>

<!-- OK in Directory.Packages.props — PackageVersion items evaluate late -->
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
</ItemGroup>
```

## [AP-05]-[ARTIFACTSPATH_IN_A_PROJECT_FILE]

- SMELL: `<ArtifactsPath>` or `<UseArtifactsOutput>` in a project file.
- WHY: The SDK reads both properties before the project body to compute the output paths. A project-file value comes too late, and the build stops with `NETSDK1199`. Set them in `Directory.Build.props` or on the command line.

```xml
<!-- BAD: MyProject.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>
</PropertyGroup>

<!-- GOOD: Directory.Build.props -->
<PropertyGroup>
  <ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>
</PropertyGroup>
```

Exception: `ArtifactsProjectName` is the one artifacts property that belongs in the project file.

## [AP-06]-[ITEM_UPDATE_BEFORE_ITEM_INCLUDE]

- SMELL: `<Compile Update="...">` or `<None Update="...">` in `Directory.Build.props`, or any `Update` that precedes the `Include` that creates the item.
- WHY: `Update` changes metadata on items that already exist at that point in evaluation. Items evaluate in order of appearance, across all imports. The SDK default globs import after `Directory.Build.props`, so an `Update` there matches nothing and reports nothing. An `Update` after the `Include` works, in the same `ItemGroup` or a later one. `Update` is not valid inside a target.

```xml
<!-- BAD: Directory.Build.props — the default None glob does not exist yet -->
<ItemGroup>
  <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>

<!-- GOOD: the project file or Directory.Build.targets — the glob already ran -->
<ItemGroup>
  <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## [AP-07]-[SIDE_EFFECTS_DURING_PROPERTY_EVALUATION]

- SMELL: A property function that writes a file, calls the network, or changes state inside a `<PropertyGroup>`.
- WHY: MSBuild evaluates a project many times: design-time builds, `-getProperty` queries, and each project instance. The side effect runs each time, without a build.

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

## [AP-08]-[RESTATING_SDK_DEFAULTS]

- SMELL: A property set to the value that the .NET SDK already provides.
- WHY: The noise hides the real overrides. When a newer SDK changes a default, the copied value pins the old behavior.

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
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

## [AP-09]-[DUPLICATED_PROPERTIES_ACROSS_PROJECT_FILES]

- SMELL: The same `<PropertyGroup>` block in three or more project files.
- WHY: Each change must land in every file. The copies drift apart.

```xml
<!-- BAD: Repeated in every .csproj -->
<!-- ProjectA.csproj, ProjectB.csproj, ProjectC.csproj all have: -->
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>

<!-- GOOD: Define once in Directory.Build.props at the repo root -->
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

See `dotnet-msbuild-evaluation` skill for `Directory.Build.props` and `Directory.Build.targets` placement.

## [AP-10]-[REFERENCE_WITH_HINTPATH_FOR_NUGET_PACKAGES]

- SMELL: `<Reference Include="..." HintPath="..\packages\SomePackage\lib\..." />`
- WHY: This is the `packages.config` layout. It has no transitive dependencies, no version conflict resolution, and no restore. The `packages/` folder must exist before the build.

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

## [AP-11]-[REDUNDANT_PROJECTREFERENCE_TO_A_TRANSITIVE_DEPENDENCY]

- SMELL: A project references both `Core` and `Utils`, and `Core` already references `Utils`, and the project uses no `Utils` type directly.
- WHY: The .NET SDK makes project references transitive through `project.assets.json` unless `DisableTransitiveProjectReferences` is `true`. The extra edge adds coupling and obscures the dependency graph.

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

## [AP-12]-[IMPORT_WITHOUT_EXISTS_GUARD]

- SMELL: `<Import Project="some-file.props" />` for an optional file, without `Condition="Exists('...')"`.
- WHY: If the file is absent, the build stops with `MSB4019`. Guard every optional import. Do not guard a required import, because a missing required file must fail fast.

```xml
<!-- BAD -->
<Import Project="$(RepoRoot)eng\custom.props" />

<!-- GOOD: Guard optional imports -->
<Import Project="$(RepoRoot)eng\custom.props" Condition="Exists('$(RepoRoot)eng\custom.props')" />

<!-- ALSO GOOD: Sdk attribute imports don't need guards (they're required by design) -->
<Project Sdk="Microsoft.NET.Sdk">
```

Exception, NuGet package forwarders: an unguarded `<Import>` inside a package's `build/` or `buildTransitive/` folder is a package contract, and the packed layout decides whether it resolves. See `dotnet-msbuild-evaluation` skill for package `build/` folder imports.

## [AP-13]-[HARDCODED_ABSOLUTE_PATHS]

- SMELL: Paths such as `C:\tools\`, `D:\packages\`, or `/usr/local/bin/` in project files.
- WHY: The path does not exist on other machines, in CI, or on other operating systems.
- See `dotnet-msbuild-evaluation` skill for path normalization.

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

## [AP-14]-[BACKSLASHES_IN_PATHS]

- SMELL: Backslash separators in `.props` or `.targets` files that run cross-platform.
- WHY: The evaluator converts `\` to `/` on Unix-like systems before it resolves a path. The conversion is a heuristic: it applies only when the string looks like a path and its first segment exists on disk. A string that does not pass through the evaluator gets no conversion.

[ERROR] when the string bypasses the evaluator:
- A raw shell string inside `<Exec Command="...\tools\foo.exe ..." />`. On Unix, `Exec` runs the command with `sh`, which reads `\` as an escape.
- A backslash path inside a CDATA block, in a source file that `<WriteLinesToFile>` writes, or in a value for a non-MSBuild consumer such as a script, response file, or environment variable.
- A path that a custom task passes to OS file APIs without MSBuild path utilities.

[STYLE] when the string passes through the evaluator: `<Import Project="...">`, a path property that a built-in task such as `<Copy>`, `<MakeDir>`, or `<Delete>` consumes, or an item `Include` or `Exclude` glob. `$(MSBuildThisFileDirectory)` ends with the separator of the current operating system, so `$(MSBuildThisFileDirectory)tools/mytool` works on all of them. Forward slashes are the convention in new code. Do not flag an existing backslash import as [ERROR].

```xml
<!-- [ERROR]: \ in raw shell string breaks on Linux/macOS -->
<Exec Command="$(MSBuildThisFileDirectory)tools\release\sign.exe $(OutputPath)" />

<!-- [STYLE]: \ in Import is normalized on Unix, but / is nicer -->
<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />

<!-- [RECOMMENDED] in new code -->
<Import Project="$(MSBuildThisFileDirectory)../../build/common.props" />
```

## [AP-15]-[MONOLITHIC_TARGETS]

- SMELL: One `<Target>` of 50 or more lines that does unrelated work.
- WHY: MSBuild cannot skip one step of the target with an incremental build. A failure is hard to locate. The target name says nothing.

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

## [AP-16]-[CUSTOM_TARGETS_MISSING_INPUTS_AND_OUTPUTS]

- SMELL: `<Target Name="MyTarget" BeforeTargets="Build">` that writes files, with no `Inputs` and `Outputs` attributes.
- WHY: MSBuild skips a target only when every output is newer than its inputs. Without both attributes, the target runs on every build, and a no-op build becomes slow.

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

- `Inputs` lists `$(MSBuildProjectFile)` plus every source file that drives generation.
- `Outputs` lives under `$(IntermediateOutputPath)`, so the generated file lands in `obj/`.
- The `FileWrites` item makes sure that `Clean` deletes the generated file.
- The `Compile` item inside the target adds the file to compilation without a glob at evaluation time.

See `dotnet-msbuild-execution` skill for `Inputs`, `Outputs`, `FileWrites`, and up-to-date checks.

## [AP-17]-[EXEC_FOR_BUILTIN_TASKS]

- SMELL: `<Exec Command="mkdir ..." />`, `<Exec Command="copy ..." />`, `<Exec Command="del ..." />`
- WHY: `Exec` runs `cmd.exe` on Windows and `sh` on other systems, so the command is not portable. Built-in tasks run on every operating system, log each file, report errors in one format, and `Copy` can skip unchanged files.

```xml
<!-- BAD -->
<Target Name="PrepareOutput">
  <Exec Command="mkdir $(OutputPath)logs" />
  <Exec Command="copy config.json $(OutputPath)" />
  <Exec Command="del $(IntermediateOutputPath)*.tmp" />
</Target>

<!-- GOOD -->
<Target Name="PrepareOutput">
  <ItemGroup>
    <TempFiles Include="$(IntermediateOutputPath)*.tmp" />
  </ItemGroup>
  <MakeDir Directories="$(OutputPath)logs" />
  <Copy SourceFiles="config.json" DestinationFolder="$(OutputPath)" />
  <Delete Files="@(TempFiles)" />
</Target>
```

| [INDEX] | [SHELL_COMMAND]    | [MSBUILD_TASK]           |
| :-----: | :----------------- | :----------------------- |
|  [01]   | `mkdir`            | `<MakeDir>`              |
|  [02]   | `copy` / `cp`      | `<Copy>`                 |
|  [03]   | `del` / `rm`       | `<Delete>`               |
|  [04]   | `move` / `mv`      | `<Move>`                 |
|  [05]   | `echo text > file` | `<WriteLinesToFile>`     |
|  [06]   | `touch`            | `<Touch>`                |
|  [07]   | `xcopy /s`         | `<Copy>` with item globs |

## [AP-18]-[EXEC_FOR_STRING_AND_PATH_OPERATIONS]

- SMELL: `<Exec Command="echo $(Var) | sed ..." />` or `<Exec Command="powershell -c ..." />` for string or path manipulation.
- WHY: The command depends on the shell, fails on other operating systems, and runs a process for a value that a property function computes at evaluation time. The result comes back only through `ConsoleToMSBuild` and the `ConsoleOutput` output item.

```xml
<!-- BAD -->
<Target Name="GetCleanVersion">
  <Exec Command="echo $(Version) | sed 's/-preview//'" ConsoleToMSBuild="true">
    <Output TaskParameter="ConsoleOutput" PropertyName="CleanVersion" />
  </Exec>
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

## [AP-19]-[PLATFORM_SPECIFIC_EXEC_WITHOUT_OS_CONDITION]

- SMELL: `<Exec Command="chmod +x ..." />` or `<Exec Command="cmd /c ..." />` without an OS condition.
- WHY: The command fails on the other operating system. If the project builds on more than one operating system, guard each platform-specific command.

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

## [AP-20]-[DUPLICATE_PROJECT_INSTANCE_WITH_SHARED_OUTPUT_PATH]

- SMELL: A target calls the `<MSBuild>` task on a project with extra `Properties` that do not change that project's output path.
- WHY: MSBuild creates one project instance for a project path and a unique set of global properties. The extra property creates a second instance, `(project, {_IsPublishing=true})`, with the same `OutputPath` and `IntermediateOutputPath` as the instance the solution or graph already builds, `(project, {})`. The project builds twice. In a parallel build the two instances write the same output files and fail with `The process cannot access the file because it is being used by another process`, or with an intermittent file-lock failure. The `<MSBuild>` call can live in the target project or in any other project in the same build.

```xml
<!-- BAD (a): forks a second instance (path + {_IsPublishing=true}) that shares this project's bin/obj -->
<Target Name="PublishOnBuild" AfterTargets="Build">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>
```

```xml
<!-- GOOD (a): set the flag as a normal (non-global) property and run the target in the SAME instance -->
<PropertyGroup>
  <_PublishWasInvokedDirectly Condition="'$(_IsPublishing)' == 'true'">true</_PublishWasInvokedDirectly>
  <_IsPublishing>true</_IsPublishing>
</PropertyGroup>

<Target Name="PublishOnBuild"
        AfterTargets="Build"
        DependsOnTargets="Publish"
        Condition="'$(_PublishWasInvokedDirectly)' != 'true'" />
```

For (a), the normal property keeps one instance, one output path, and nothing to race. `DependsOnTargets="Publish"` (or `CallTarget`) runs `Publish` in that instance instead of a second one. The `_PublishWasInvokedDirectly` guard breaks the target cycle when `dotnet publish` is the entry point, because `dotnet publish` sets `_IsPublishing=true` as a global property.

```xml
<!-- BAD (b): the consumer forks a publish instance of the tool that races the tool's own build in the graph -->
<MSBuild Projects="..\tool\tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />

<!-- GOOD (b): apply the (a) fix in tool.csproj so the tool publishes its OWN build; reference it only to sequence it, never to re-publish -->
<ItemGroup>
  <ProjectReference Include="..\tool\tool.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
<!-- The consumer then reads the tool's publish dir; it does not invoke Publish on tool. -->
```

Extra global properties are safe only when the output path contains the value: `RuntimeIdentifier`, `TargetFramework`, `Configuration`, `Platform`. Then each instance writes to its own directory. If a path-neutral property is unavoidable, give that build its own `BaseIntermediateOutputPath` and output path.

## [AP-21]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_PROJECTREFERENCE]

- SMELL: A `<ProjectReference>` carries `SetTargetFramework="TargetFramework=net10.0"`, the referenced project is single-targeting (`<TargetFramework>`, not `<TargetFrameworks>`), and the injected TFM equals the TFM that the project already targets.
- WHY: `SetTargetFramework` injects `TargetFramework` as a global property on the referenced build. The mechanism exists so a consumer can pick one TFM of a multi-targeting project.

```xml
<!-- BAD: Tool.csproj single-targets net10.0 and we inject that SAME net10.0 — redundant AND harmful -->
<ItemGroup>
  <ProjectReference Include="..\Tool\Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />
</ItemGroup>
```

For a single-targeting project, the TFM it already targets is path-neutral. The project already resolves to `bin\<config>\net10.0\` and `obj\<config>\net10.0\`. The global property only creates a second project instance, `(project, {TargetFramework=net10.0})`. The solution or graph builds the same project as `(project, {})`. Both instances clash on `OutputPath` and `IntermediateOutputPath`.

The ProjectReference protocol itself does not set `TargetFramework` for a single-targeting reference. It removes the global property. `SetTargetFramework` overrides that default and reintroduces the clash.

```xml
<!-- GOOD: single-targeting reference needs no SetTargetFramework — just reference it -->
<ItemGroup>
  <ProjectReference Include="..\Tool\Tool.csproj" />
</ItemGroup>
```

`SetTargetFramework` is correct in two cases:
1. The referenced project is multi-targeting and the consumer needs one specific TFM. Each TFM has its own output path.
2. The consumer builds a single-targeting project under a TFM other than the one it declares. The injected `TargetFramework` changes the output path to `obj\<config>\<different-tfm>\`, so the instance does not collide with `(project, {})`.

Related, a framework-incompatible reference: when the referencing and referenced projects target incompatible frameworks, for example `.NETFramework` and `.NETCoreApp`, set both metadata values:
- `SkipGetTargetFrameworkProperties="true"` skips the target framework negotiation, which fails for incompatible frameworks.
- `ReferenceOutputAssembly="false"` sequences the build without a reference to an assembly the consumer cannot load.

```xml
<!-- OK: .NETFramework project builds an incompatible .NETCoreApp tool without referencing its assembly -->
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  ReferenceOutputAssembly="false" />
```

[CAUTION] The negotiation step adds `UndefineProperties="TargetFramework"` for a single-targeting reference. `SkipGetTargetFrameworkProperties="true"` skips that step. Then the referencing project's own `TargetFramework` global property, present in every multi-targeting inner build, flows into the referenced project. A single-targeting referenced project then builds under the wrong TFM and the wrong output path. Guard in one of two ways:
- Set `SetTargetFramework="TargetFramework=<tfm>"` to pin the referenced build. A multi-targeting reference requires this.
- Set `UndefineProperties="TargetFramework"` to strip the inherited global property, so a single-targeting project builds as declared.

```xml
<!-- OK: strip the referencing project's TargetFramework so the single-targeting tool builds as it declares -->
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  UndefineProperties="TargetFramework"
                  ReferenceOutputAssembly="false" />
```

Use `SetTargetFramework` or `UndefineProperties="TargetFramework"`, not both.
