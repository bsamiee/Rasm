---
name: dotnet-msbuild-antipatterns
description: "Use when reviewing, auditing, or fixing a .csproj, .props, or .targets file, or when a build defect traces to how the file is authored."
---

# [DOTNET_MSBUILD_ANTIPATTERNS]

Use this catalog when you review project and build files. Each entry has:
- SMELL: What to look for
- WHY: The effect on correctness, incremental build, or maintenance
- RULE, when one line settles the placement
- A `BAD` and a `GOOD` example. An `OK` example marks a form to leave alone.

Delete the output directories and run `dotnet build <solution> -check -m:1` before you read the files. `BC0101` and `BC0102` report shared output paths and double writes. The checks report no property override. Read the files for those. The `dotnet-msbuild-diagnostics` skill owns the check workflow.

## [AP-01]-[UNQUOTED_CONDITION_OPERANDS]

- SMELL: `Condition="$(Foo) == net10.0"`. One side of a comparison has no single quotes.
- WHY: MSBuild tokenizes the condition before it expands a property. An unquoted literal that contains `.`, `-`, `:`, or a space is not one token, and the build fails with `MSB4092` or `MSB4090`. An unquoted property expands to one string and never fails. An empty value then evaluates to `false` with no error, and two empty properties compare equal. An empty value still fails a bare boolean condition with `MSB4113` and a numeric comparison with `MSB4086`.

```xml
<!-- BAD: MSB4092, the literal breaks into two tokens -->
<PropertyGroup Condition="$(TargetFramework) == net10.0">
  <Optimize>true</Optimize>
</PropertyGroup>

<!-- GOOD -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <Optimize>true</Optimize>
</PropertyGroup>
```

## [AP-02]-[PROPERTY_DEFAULTS_IN_TARGETS_FILES]

- SMELL: A default needed by earlier imports or project evaluation exists only in a `.targets` file.
- WHY: `.targets` files import after the project body. A file that reads the property before that point sees an empty value.
- RULE: `.props` owns overridable defaults needed during evaluation. `.targets` owns targets, fallbacks, and values derived from later properties.

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

## [AP-03]-[UNCONDITIONAL_PROPERTY_OVERRIDE_IN_MULTIPLE_SCOPES]

- SMELL: A property set without a condition in both `Directory.Build.props` and a `.csproj`. The last assignment wins without a message.
- WHY: A reader cannot tell which value the build uses without a binlog.

```xml
<!-- BAD: Directory.Build.props sets it, csproj silently overrides -->
<!-- Directory.Build.props -->
<PropertyGroup>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
<!-- MyProject.csproj -->
<PropertyGroup>
  <LangVersion>preview</LangVersion>
</PropertyGroup>

<!-- GOOD: Use a condition so overrides are intentional -->
<!-- Directory.Build.props -->
<PropertyGroup>
  <LangVersion Condition="'$(LangVersion)' == ''">latest</LangVersion>
</PropertyGroup>
<!-- MyProject.csproj can now intentionally override or leave the default -->
```

Never default `OutputPath` or `IntermediateOutputPath` this way. An assigned value replaces the SDK path and drops the `Configuration` segment. Set `BaseOutputPath` or `ArtifactsPath` instead.

## [AP-04]-[PROPERTY_CONDITIONED_ON_TARGETFRAMEWORK_IN_PROPS_FILES]

- SMELL: `<PropertyGroup Condition="'$(TargetFramework)' == '...'">` or a property condition on `$(TargetFramework)` in `Directory.Build.props` or any `.props` file that imports before the project body.
- WHY: A single-targeting project sets `TargetFramework` in the project body, after `.props` evaluation. The condition compares an empty string and does not match. A multi-targeting inner build receives `TargetFramework` early as a global property. A caller can also supply that global property. See `dotnet-msbuild-evaluation` skill for evaluation order.

```xml
<!-- BAD: In Directory.Build.props — TargetFramework is empty here -->
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

[CAUTION] Only property conditions have this problem. Items and targets evaluate after every property. `ItemGroup`, item, and `Target` conditions on `$(TargetFramework)` in `.props` files are correct. The same rule covers `OutputPath`, `IntermediateOutputPath`, `TargetPath`, and `TargetFrameworkMoniker`, which the SDK `.targets` compute.

Do NOT flag these patterns:

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
- WHY: The SDK reads both properties before the project body to compute the output paths. A project-file value comes too late, and the build fails with `NETSDK1199`. Set them in `Directory.Build.props` or on the command line.

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

Only `ArtifactsPath` and `UseArtifactsOutput` fail with `NETSDK1199`. Flag them in a project file even when the build is green. A value that follows `Directory.Build.props` passes the check and still splits bin from obj. The SDK reads `ArtifactsProjectName`, `ArtifactsPivots`, and the output-name properties after the project body. A project file can set them. `BaseIntermediateOutputPath` in a project file fails the same way as `ArtifactsPath`, with `MSB3539`.

## [AP-06]-[ITEM_UPDATE_BEFORE_ITEM_INCLUDE]

- SMELL: An `Update` in `Directory.Build.props`, any `Update` before the `Include` that creates the item, or an `Include` for a file the default glob already matches.
- WHY: `Update` changes metadata on items that already exist at that point in evaluation. Items evaluate in order of appearance, across all imports. The SDK default globs import after `Directory.Build.props`. An `Update` there matches nothing and reports nothing. An `Update` after the `Include` works, in the same `ItemGroup` or a later one. The inverse defect is an `Include` for a file the default glob already matches. `Compile` then fails with `NETSDK1022`. `None` silently contains two items, one without the metadata.

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

- SMELL: A property function that reads the file system inside a `<PropertyGroup>`.
- WHY: MSBuild evaluates a project many times: design-time builds, `-getProperty` queries, and each project instance. The read runs each time, without a build, and a stale file gives a stale value. MSBuild blocks a write such as `WriteAllText` with `MSB4185`, and `MSBUILDENABLEALLPROPERTYFUNCTIONS=1` removes that block for every function.

```xml
<!-- BAD: file read during evaluation -->
<PropertyGroup>
  <GitHead>$([System.IO.File]::ReadAllText('$(MSBuildThisFileDirectory).git/HEAD'))</GitHead>
</PropertyGroup>

<!-- GOOD: file access belongs in a target -->
<Target Name="ReadGitHead" BeforeTargets="CoreCompile">
  <ReadLinesFromFile File="$(MSBuildThisFileDirectory).git/HEAD">
    <Output TaskParameter="Lines" PropertyName="GitHead" />
  </ReadLinesFromFile>
</Target>
```

## [AP-08]-[RESTATING_SDK_DEFAULTS]

- SMELL: A property set to the .NET SDK default value.
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

## [AP-09]-[REFERENCE_WITH_HINTPATH_FOR_NUGET_PACKAGES]

- SMELL: `<Reference Include="..." HintPath="..\packages\SomePackage\lib\..." />`
- WHY: A `Reference` with `HintPath` restores nothing. An SDK-style project ignores `packages.config`. Nothing populates the folder, and the build reports `MSB3245`. `PackageReference` restores the package and resolves transitive dependencies and version conflicts.

```xml
<!-- BAD -->
<ItemGroup>
  <Reference Include="Newtonsoft.Json">
    <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\netstandard2.0\Newtonsoft.Json.dll</HintPath>
  </Reference>
</ItemGroup>

<!-- GOOD: the project declares the reference, Directory.Packages.props owns the version -->
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" />
</ItemGroup>
```

## [AP-10]-[REDUNDANT_PROJECTREFERENCE_TO_A_TRANSITIVE_DEPENDENCY]

- SMELL: A project references both `Core` and `Utils`, `Core` already references `Utils`, and the project uses no `Utils` type directly.
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

## [AP-11]-[IMPORT_WITHOUT_EXISTS_GUARD]

- SMELL: `<Import Project="some-file.props" />` for an optional file, without `Condition="Exists('...')"`.
- WHY: If the file is absent, the build fails with `MSB4019`. Guard every optional import. Do not guard a required import. A missing required file must fail fast.

```xml
<!-- BAD -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props" />

<!-- GOOD: Guard optional imports -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props"
        Condition="Exists('$(MSBuildThisFileDirectory)eng/custom.props')" />

<!-- ALSO GOOD: an Sdk attribute import needs no guard, the SDK is required -->
<Project Sdk="Microsoft.NET.Sdk">
```

Exception: an unguarded `<Import>` inside a package's `build/` or `buildTransitive/` folder is a package contract. The packed layout decides whether it resolves. See `dotnet-msbuild-evaluation` skill for package `build/` folder imports.

## [AP-12]-[HARDCODED_ABSOLUTE_PATHS]

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
  <ToolPath>$(MSBuildThisFileDirectory)tools/mytool/mytool</ToolPath>
</PropertyGroup>
<Import Project="$(MSBuildThisFileDirectory)eng/common.props" />
```

## [AP-13]-[BACKSLASHES_IN_PATHS]

- SMELL: Backslash separators in `.props` or `.targets` files that run cross-platform.
- WHY: The evaluator converts `\` to `/` on Unix-like systems before it resolves a path. The conversion is a heuristic: it applies only when the string looks like a path and its first segment exists on disk. A string that does not pass through the evaluator gets no conversion.

[ERROR] when the heuristic does not convert, or converts a backslash that must stay:
- An `Exec` command whose first segment is not an existing directory. A command that starts with a program name, such as `cat`, `git`, or `dotnet`, is never converted. `sh` then reads each `\` as an escape and deletes it.
- A backslash that is not part of a path, such as `<Exec Command="echo a\b\c" />`. `sh` prints `abc`.
- A backslash a consumer must keep. `Lines` on `<WriteLinesToFile>` is an item list. Every item is converted, and the file receives `a/b` where it needs `a\b`.
- A path that a custom task passes to OS file APIs without MSBuild path utilities.

[STYLE] when the string passes through the evaluator. That covers `<Import Project="...">`, a path property that `<Copy>`, `<MakeDir>`, or `<Delete>` consumes, and an item `Include` or `Exclude` glob. `$(MSBuildThisFileDirectory)` ends with the separator of the current operating system, and `$(MSBuildThisFileDirectory)tools/mytool` works on all of them. Forward slashes are the convention in new code. Do not flag an existing backslash import as [ERROR].

```xml
<!-- [ERROR]: the first segment is a program, so sh deletes the backslash and cat reads "datafile.txt" -->
<Exec Command="cat data\file.txt" />

<!-- [STYLE]: \ in Import is normalized on Unix, but / is nicer -->
<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />

<!-- GOOD in new code -->
<Import Project="$(MSBuildThisFileDirectory)../../build/common.props" />
```

## [AP-14]-[MONOLITHIC_TARGETS]

- SMELL: One `<Target>` of 50 or more lines that does unrelated work.
- WHY: MSBuild cannot skip one step of the target in an incremental build. A failure is hard to locate. The target name says nothing.

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
<Target Name="CopyVersionFile" BeforeTargets="CoreCompile"
        Inputs="version.txt" Outputs="$(IntermediateOutputPath)version.txt">
  <Copy SourceFiles="version.txt" DestinationFiles="$(IntermediateOutputPath)version.txt" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)version.txt" />
  </ItemGroup>
</Target>

<Target Name="CopyLicense" AfterTargets="CopyFilesToOutputDirectory"
        Inputs="LICENSE" Outputs="$(OutputPath)LICENSE">
  <Copy SourceFiles="LICENSE" DestinationFolder="$(OutputPath)" SkipUnchangedFiles="true" />
  <ItemGroup>
    <FileWrites Include="$(OutputPath)LICENSE" />
  </ItemGroup>
</Target>
```

Register `@(FileWrites)` from an `ItemGroup` inside the target. A skipped target runs its `ItemGroup` children and no task. A task `<Output>` element then leaves the file unrecorded, and `IncrementalClean` deletes it. `_CleanRecordFileWrites` runs inside the `Build` chain. A target that runs after `Build` records nothing, and `Clean` leaves the file.

## [AP-15]-[CUSTOM_TARGETS_MISSING_INPUTS_AND_OUTPUTS]

- SMELL: `<Target Name="MyTarget" BeforeTargets="Build">` that writes files, with no `Inputs` and `Outputs` attributes.
- WHY: MSBuild skips a target when every output is the same age as or newer than its corresponding input set. Without both attributes, the target runs on every build and slows a no-op build.

```xml
<!-- BAD: Runs every time -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile">
  <Copy SourceFiles="BuildInfo.cs.in"
        DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
</Target>

<!-- GOOD: Skipped when up-to-date -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile"
        Inputs="BuildInfo.cs.in" Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <Copy SourceFiles="BuildInfo.cs.in"
        DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
    <Compile Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>
```

- `Inputs` lists every source file that drives generation.
- See `dotnet-msbuild-execution` skill for `Inputs`, `Outputs`, `FileWrites`, and up-to-date checks.

## [AP-16]-[EXEC_FOR_BUILTIN_TASKS]

- SMELL: `<Exec Command="mkdir ..." />`, `<Exec Command="copy ..." />`, `<Exec Command="del ..." />`
- WHY: `Exec` runs `cmd.exe` on Windows and `sh` elsewhere. The command is not portable. Built-in tasks run on every operating system, log each file, report errors in one format, and `Copy` can skip unchanged files.

```xml
<!-- BAD -->
<Target Name="PrepareOutput" AfterTargets="CopyFilesToOutputDirectory">
  <Exec Command="mkdir $(OutputPath)logs" />
  <Exec Command="copy config.json $(OutputPath)" />
  <Exec Command="del $(IntermediateOutputPath)*.tmp" />
</Target>

<!-- GOOD -->
<Target Name="PrepareOutput" AfterTargets="CopyFilesToOutputDirectory"
        Inputs="config.json" Outputs="$(OutputPath)config.json">
  <ItemGroup>
    <TempFiles Include="$(IntermediateOutputPath)*.tmp" />
  </ItemGroup>
  <MakeDir Directories="$(OutputPath)logs" />
  <Copy SourceFiles="config.json" DestinationFolder="$(OutputPath)" />
  <Delete Files="@(TempFiles)" />
  <ItemGroup>
    <FileWrites Include="$(OutputPath)config.json" />
  </ItemGroup>
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

## [AP-17]-[EXEC_FOR_STRING_AND_PATH_OPERATIONS]

- SMELL: `<Exec Command="echo $(Var) | sed ..." />` or `<Exec Command="powershell -c ..." />` for string or path manipulation.
- WHY: The command depends on the shell and fails on other operating systems. It runs a process for a value that a property function computes at evaluation time. The result returns only through `ConsoleToMSBuild` and the `ConsoleOutput` output item.

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
  <ToolDir>$([MSBuild]::NormalizeDirectory('$(MSBuildThisFileDirectory)', 'tools'))</ToolDir>
  <ToolPath>$([System.IO.Path]::Combine($(MSBuildThisFileDirectory), 'tools', 'mytool'))</ToolPath>
</PropertyGroup>
```

A path function resolves a relative argument against the working directory of the build process. Pass `$(MSBuildProjectDirectory)` or `$(MSBuildThisFileDirectory)` as the first argument.

## [AP-18]-[PLATFORM_SPECIFIC_EXEC_WITHOUT_OS_CONDITION]

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

## [AP-19]-[DUPLICATE_PROJECT_INSTANCE_WITH_SHARED_OUTPUT_PATH]

- SMELL: A target calls the `<MSBuild>` task on a project with extra `Properties` that do not change that project's output path.
- WHY: MSBuild creates one project instance per project path and global property set. The extra property creates a second instance, `(project, {_IsPublishing=true})`, with the same `OutputPath` and `IntermediateOutputPath` as the instance the solution or graph already builds, `(project, {})`. The project builds twice. Both instances run the same targets against one output directory. A green build that does the work twice is the common result. A parallel build can race those writes and report `MSB3026` copy retries or a file-lock error. The `<MSBuild>` call can appear in the target project or in any other project in the same build.

```xml
<!-- BAD (a): forks a second instance (path + {_IsPublishing=true}) that shares this project's bin/obj; the guard only stops the recursion -->
<Target Name="PublishOnBuild" AfterTargets="Build" Condition="'$(_IsPublishing)' != 'true'">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>
```

```xml
<!-- GOOD (a): read the SDK flag, keep one instance, run Publish in it -->
<Target Name="PublishOnBuild"
        AfterTargets="Build"
        DependsOnTargets="Publish"
        Condition="'$(_IsPublishing)' != 'true'" />
```

In (a), `DependsOnTargets="Publish"` runs `Publish` in the same instance, with one output path and nothing to race. The condition breaks the target cycle when `dotnet publish` is the entry point. Without it, `dotnet publish` fails with `MSB4006`. Never set `_IsPublishing` yourself. The SDK owns it, and a `true` value moves a plain build to the `PublishRuntimeIdentifier` output path. Under `dotnet build -check`, the read reports `BC0201`. Nothing sets the property in a plain build. Under `-warnaserror` that check fails the build. Set `build_check.BC0201.severity=none` in `.editorconfig`.

```xml
<!-- BAD (b): the consumer forks a publish instance of the tool that races the tool's own build in the graph -->
<Target Name="PublishTool" BeforeTargets="Build">
  <MSBuild Projects="../tool/tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- GOOD (b): apply the (a) fix in tool.csproj so the tool publishes its OWN build, reference it only to sequence it -->
<ItemGroup>
  <ProjectReference Include="../tool/tool.csproj"
                    ReferenceOutputAssembly="false"
                    UndefineProperties="_IsPublishing" />
</ItemGroup>
<!-- The consumer derives the tool publish dir from $(Configuration) and never invokes Publish on the tool -->
```

`UndefineProperties="_IsPublishing"` is required. `dotnet publish` on the consumer passes `_IsPublishing=true` down the reference. That value trips the tool's guard, and the tool publishes nothing.

Extra global properties are safe only when the effective `OutputPath` and `IntermediateOutputPath` contain their values. Under the artifacts layout `Platform` is never a pivot. If a path-neutral property is unavoidable, give that build its own `BaseIntermediateOutputPath` and output path. The `dotnet-msbuild-diagnostics` skill lists the properties that separate the paths.

## [AP-20]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_PROJECTREFERENCE]

- SMELL: A `<ProjectReference>` has `SetTargetFramework="TargetFramework=net10.0"`, the referenced project is single-targeting (`<TargetFramework>`, not `<TargetFrameworks>`), and the injected TFM equals the TFM the project already targets.
- WHY: `SetTargetFramework` passes `TargetFramework` as a global property to the referenced build. That creates a second instance, `(project, {TargetFramework=net10.0})`, beside the `(project, {})` that the solution builds, and both instances clash on `OutputPath` and `IntermediateOutputPath`. The metadata lets a consumer pick one TFM of a multi-targeting project.

```xml
<!-- BAD: Tool.csproj single-targets net10.0 and the metadata injects that same net10.0 -->
<ItemGroup>
  <ProjectReference Include="../Tool/Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />
</ItemGroup>
```

For a single-targeting project, the TFM it already targets is path-neutral. With SDK default paths, the project resolves to `bin/<config>/net10.0/` and `obj/<config>/net10.0/` in both instances.

The `ProjectReference` protocol does not set `TargetFramework` for a single-targeting reference. It removes the global property. `SetTargetFramework` overrides that default and reintroduces the clash.

```xml
<!-- GOOD: single-targeting reference needs no SetTargetFramework — just reference it -->
<ItemGroup>
  <ProjectReference Include="../Tool/Tool.csproj" />
</ItemGroup>
```

`SetTargetFramework` is correct in two cases:
1. The referenced project is multi-targeting and the consumer needs one specific TFM. Each TFM has its own output path.
2. The consumer builds a single-targeting project under a different TFM, and the effective paths contain that TFM. The injected value then gives the instance separate paths.

Related, a framework-incompatible reference: when the referencing and referenced projects target incompatible frameworks, for example `.NETFramework` and `.NETCoreApp`, set both metadata values:
- `SkipGetTargetFrameworkProperties="true"` skips the target framework negotiation, which fails for incompatible frameworks.
- `ReferenceOutputAssembly="false"` sequences the build without a reference to an assembly the consumer cannot load.

```xml
<!-- OK: .NETFramework project builds an incompatible .NETCoreApp tool without referencing its assembly -->
<ProjectReference Include="../Tool/Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  ReferenceOutputAssembly="false" />
```

[CAUTION] The negotiation step adds `UndefineProperties="TargetFramework"` for a single-targeting reference. `SkipGetTargetFrameworkProperties="true"` skips that step. The referencing project's own `TargetFramework` global property, present in every multi-targeting inner build, then flows into the referenced project. A single-targeting referenced project builds once per consumer framework and fails with `NETSDK1005`. That error names an assets file with no target for the inherited framework. Guard in one of two ways:
- Set `SetTargetFramework="TargetFramework=<tfm>"` to pin the referenced build. A multi-targeting reference requires this.
- Set `UndefineProperties="TargetFramework"` to strip the inherited global property. A single-targeting project then builds as declared.

```xml
<!-- OK: strip the referencing project's TargetFramework so the single-targeting tool builds as it declares -->
<ProjectReference Include="../Tool/Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  UndefineProperties="TargetFramework"
                  ReferenceOutputAssembly="false" />
```

Use `SetTargetFramework` or `UndefineProperties="TargetFramework"`, never both. `UndefineProperties` removes the property that `SetTargetFramework` sets, and a multi-targeting reference silently loses its pin.
