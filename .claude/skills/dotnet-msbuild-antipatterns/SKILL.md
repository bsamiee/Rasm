---
name: dotnet-msbuild-antipatterns
description: "Use when reviewing a .csproj, .props, or .targets for MSBuild smells, covering evaluation, placement, items, targets, paths, and build graph."
---

# [DOTNET_MSBUILD_ANTIPATTERNS]

Review catalog for project and build files, each entry names the smell, the failure, the rule, and a `BAD` form beside its `GOOD` form:
- `OK` marks a form to leave
- `ERROR` names a build failure, a wrong value, or a wrong output
- `STYLE` names a form that builds and costs maintenance or time

Entries naming a `BC` code hold their proof in the `-check` build, every other entry in its section's command over the files.

Use `dotnet-msbuild-diagnostics` for the BuildCheck baseline.

Use `dotnet-msbuild-evaluation` for the evaluation rule behind an entry.

[REFERENCES]:
- [01]-[WORKED_EXAMPLES](references/worked-examples.md): Corrections that span more than one element

## [01]-[EVALUATION]

Prove each with `dotnet msbuild <project> -getProperty:<Name>`.

### [01.1]-[AP-01]-[ERROR]-[UNQUOTED_CONDITION_OPERANDS]

- SMELL: `Condition="$(Foo) == net10.0"`, a comparison side without single quotes
- WHY:
  - `MSB4092` on a literal with `.`, `-`, `,`, or a space, `MSB4090` on `:`, `"`, or a backtick, `MSB4101` on an unclosed quote
  - Two unquoted empty properties compare equal, an unquoted property expands to one token and never fails
  - An empty value fails `MSB4113` in a condition with no operator and `MSB4086` in a numeric comparison
- RULE: Quote both sides of `==` and `!=`

```xml
<!-- BAD -->
<Optimize Condition="$(TargetFramework) == net10.0">true</Optimize>
<!-- GOOD -->
<Optimize Condition="'$(TargetFramework)' == 'net10.0'">true</Optimize>
```

### [01.2]-[AP-02]-[ERROR]-[SIDE_EFFECTS_DURING_PROPERTY_EVALUATION]

- SMELL: Property functions that read or write the file system inside a `PropertyGroup`
- WHY:
  - Evaluation repeats on every design-time build and `-getProperty` query
  - A file a target writes is one build stale at evaluation
  - A write fails `MSB4185`, `MSBUILDENABLEALLPROPERTYFUNCTIONS=1` enables every property function
- RULE: Read files inside a target, a read of a file no target writes is STYLE

```xml
<!-- BAD -->
<GitHead>$([System.IO.File]::ReadAllText('$(MSBuildThisFileDirectory).git/HEAD'))</GitHead>
<!-- GOOD -->
<Target Name="ReadGitHead" BeforeTargets="CoreCompile">
  <ReadLinesFromFile File="$(MSBuildThisFileDirectory).git/HEAD">
    <Output TaskParameter="Lines" PropertyName="GitHead" />
  </ReadLinesFromFile>
</Target>
```

### [01.3]-[AP-03]-[ERROR]-[GLOBAL_PROPERTY_REASSIGNED_IN_PROJECT_XML]

- SMELL: Project XML assigns a property the command line, `Directory.Build.rsp`, or an `<MSBuild>` task can supply
- WHY:
  - A global property is read-only in evaluation, the assignment is skipped under `-p:`
  - `$(ToolRoot)bin/` reads `/usr/toolbin/` under `-p:ToolRoot=/usr/tool`
- RULE: Derive the normalized value into a `_` property, or declare `TreatAsLocalProperty` on `Project` when the file wins over the command line

```xml
<!-- BAD -->
<ToolRoot>$([MSBuild]::NormalizeDirectory('$(ToolRoot)'))</ToolRoot>
<!-- GOOD -->
<_ToolDir>$([MSBuild]::NormalizeDirectory('$(ToolRoot)'))</_ToolDir>
<ToolBin>$(_ToolDir)bin/</ToolBin>
```

## [02]-[PLACEMENT]

Prove each with `-getProperty` from one project, and with `-pp:` when the assignment source is in question.

### [02.1]-[AP-04]-[ERROR]-[PROPERTY_DEFAULTS_IN_TARGETS_FILES]

- SMELL: A `.targets` file alone sets a default a `.props` file or the project body reads
- WHY: `.targets` imports after the project body, every earlier reader sees an empty value
- RULE: `.props` owns overridable defaults, `.targets` owns targets and values derived from properties the SDK sets later

```xml
<!-- BAD: custom.targets -->
<ToolVersion>2.0</ToolVersion>
<!-- GOOD: custom.props -->
<ToolVersion Condition="'$(ToolVersion)' == ''">2.0</ToolVersion>
```

### [02.2]-[AP-05]-[STYLE]-[UNCONDITIONAL_PROPERTY_OVERRIDE_IN_MULTIPLE_SCOPES]

- SMELL: `Directory.Build.props` and a `.csproj` assign one property without a condition, the last assignment wins silently
- RULE:
  - The `.props` default takes `Condition="'$(Name)' == ''"`, the project assigns another value
  - `OutputPath` and `IntermediateOutputPath` take no default in that form, an assigned value drops the `Configuration` segment
  - `BaseOutputPath` or `ArtifactsPath` sets the root

```xml
<!-- BAD: Directory.Build.props -->
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<!-- GOOD: Directory.Build.props -->
<GenerateDocumentationFile Condition="'$(GenerateDocumentationFile)' == ''">true</GenerateDocumentationFile>
```

### [02.3]-[AP-06]-[ERROR]-[PROPS_CONDITION_ON_A_LATER_VALUE]

- SMELL: A `.props` condition on `$(TargetFramework)`, `$(OutputType)`, an SDK-computed path, or a property the project body sets
- WHY:
  - `.props` imports before the body and the SDK `.targets`, the condition compares an empty string
  - A multi-targeting inner build receives `TargetFramework` as a global property and matches, the outer build leaves it empty
- RULE:
  - Condition in `Directory.Build.targets` or after the assignment in the project
  - Key an early `.props` condition on `$(MSBuildProjectName)` or the project directory
  - An item, `PackageVersion`, or `Target` condition is OK

```xml
<!-- BAD: Directory.Build.props -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
<!-- GOOD: Directory.Build.targets -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
<!-- OK: Directory.Packages.props -->
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageVersion Include="Some.Package" Version="10.0.0" />
</ItemGroup>
```

### [02.4]-[AP-07]-[ERROR]-[ARTIFACTSPATH_IN_A_PROJECT_FILE]

- SMELL: `ArtifactsPath`, `UseArtifactsOutput`, `ArtifactsProjectName`, or `BaseIntermediateOutputPath` in a project file
- WHY:
  - The SDK reads them before the project body, `ArtifactsPath` and `UseArtifactsOutput` fail `NETSDK1199`
  - `BaseIntermediateOutputPath` warns `MSB3539` after restore used the default
  - `ArtifactsProjectName` reaches `bin/` and `publish/` while `obj/` keeps the project name
- RULE:
  - Set the artifacts properties in `Directory.Build.props` or on the command line
  - `ArtifactsPivots` and the output name properties stay project-level
  - `ArtifactsPath` ends without a separator, the SDK adds one before each segment it appends

```xml
<!-- BAD: MyProject.csproj -->
<ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>
<!-- GOOD: Directory.Build.props -->
<ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', 'artifacts'))</ArtifactsPath>
```

### [02.5]-[AP-08]-[STYLE]-[RESTATING_AN_SDK_DEFAULT_OR_A_ROOT_VALUE]

- SMELL:
  - An SDK default in a project, `OutputType=Library`, `EnableDefaultItems=true`, `RootNamespace` equal to the project name
  - `LangVersion` at the framework default, or a `Directory.Build.props` value repeated in a project
- WHY:
  - The copy hides the real override and keeps the old value after an SDK or root change
  - `LangVersion` defaults to `14.0` under `net10.0` and `7.3` under `netstandard2.0`
- RULE:
  - A project holds values that differ from the SDK default and the root file
  - `LangVersion` appears where the framework default is below what the sources need

```xml
<!-- BAD: MyProject.csproj -->
<OutputType>Library</OutputType>
<Nullable>enable</Nullable>
<!-- GOOD: netstandard2.0 analyzer project -->
<LangVersion>latest</LangVersion>
```

### [02.6]-[AP-09]-[STYLE]-[NOWARN_IN_A_PROJECT_FILE]

- SMELL: `NoWarn` with a compiler or analyzer code in a project, or `NoWarn` assigned without `$(NoWarn);`
- WHY:
  - `.editorconfig` owns compiler and analyzer severity per path, the form is `dotnet_diagnostic.CS1591.severity = none` under `[*.cs]`
  - `NU*` and `MSB*` codes take `NoWarn`, an assignment without `$(NoWarn);` drops the SDK default `1701;1702`
- RULE:
  - Compiler and analyzer codes go to `.editorconfig`, restore and MSBuild codes go to `NoWarn` in `Directory.Build.props`
  - Every `NoWarn` assignment starts with `$(NoWarn);`

```xml
<!-- BAD: MyProject.csproj -->
<NoWarn>CS1591</NoWarn>
<!-- GOOD: Directory.Build.props -->
<NoWarn>$(NoWarn);NU1603</NoWarn>
```

### [02.7]-[AP-10]-[ERROR]-[RSP_OR_SOLUTION_PROPS_FOR_A_PROJECT_SETTING]

- SMELL: `-p:Name=Value` in `Directory.Build.rsp`, or a project value in `Directory.Solution.props`
- WHY:
  - A `-p:` switch in the response file is a global property no project overrides
  - `Directory.Solution.props` imports into the solution project alone, its properties reach no project under any entry point
- RULE:
  - Project settings go in `Directory.Build.props`, `Directory.Build.rsp` holds command-line switches
  - `Directory.Solution.props` holds solution project settings

```xml
<!-- BAD: Directory.Build.rsp -->
-p:Configuration=Release
<!-- GOOD: Directory.Build.props -->
<Configuration Condition="'$(Configuration)' == ''">Release</Configuration>
```

## [03]-[ITEMS_AND_REFERENCES]

Prove each with `dotnet msbuild <project> -getItem:<Type>`.

### [03.1]-[AP-11]-[ERROR]-[ITEM_UPDATE_OR_REMOVE_BEFORE_THE_SDK_INCLUDE]

- SMELL: `Update` or `Remove` in `Directory.Build.props`, an `Update` matching no item, or an `Include` of a file the default glob matches
- WHY:
  - `Update` and `Remove` act on items that exist at that point, the SDK globs import after `Directory.Build.props`
  - The element matches nothing and reports nothing
  - A duplicate `Compile` fails `NETSDK1022`, a full-path duplicate warns `CS2002`, a duplicate `None` copies twice silently
- RULE: `Update` and `Remove` go in the project or `Directory.Build.targets`, an exclusion goes to `DefaultItemExcludes` in `.props`

```xml
<!-- BAD: Directory.Build.props -->
<None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
<Compile Remove="Generated/**" />
<!-- GOOD: Directory.Build.props -->
<DefaultItemExcludes>$(DefaultItemExcludes);Generated/**</DefaultItemExcludes>
<!-- GOOD: Directory.Build.targets -->
<None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
```

### [03.2]-[AP-12]-[ERROR]-[REFERENCE_WITH_HINTPATH_FOR_A_PACKAGE_OR_PROJECT]

- SMELL: `Reference` with a `HintPath` into `packages/` or another project's `bin/`, or a host-supplied assembly without `Private="false"`
- WHY:
  - Nothing restores `packages/` for an SDK project, `MSB3245`
  - A `Reference` to a project output loses the build order, `BC0104`
  - A host assembly without `Private="false"` copies into the output and the host loads that copy
- RULE:
  - Packages are `PackageReference`, projects are `ProjectReference`
  - A host assembly is a `Reference` with `HintPath` from a property and `Private="false"`

```xml
<!-- BAD -->
<Reference Include="Newtonsoft.Json" HintPath="../packages/Newtonsoft.Json.13.0.3/lib/netstandard2.0/Newtonsoft.Json.dll" />
<!-- GOOD -->
<PackageReference Include="Newtonsoft.Json" />
<!-- OK -->
<Reference Include="HostCore" HintPath="$(HostAssemblyDir)HostCore.dll" Private="false" />
```

### [03.3]-[AP-13]-[STYLE]-[REDUNDANT_PROJECTREFERENCE_TO_A_TRANSITIVE_DEPENDENCY]

- SMELL: `Core` references `Utils`, the project references both and names no `Utils` type
- WHY: Project references are transitive through `project.assets.json` unless `DisableTransitiveProjectReferences` is `true`
- RULE: Reference the projects with types the sources name

```xml
<!-- BAD -->
<ProjectReference Include="../Core/Core.csproj" />
<ProjectReference Include="../Utils/Utils.csproj" />
<!-- GOOD -->
<ProjectReference Include="../Core/Core.csproj" />
```

### [03.4]-[AP-14]-[ERROR]-[IMPORT_WITHOUT_EXISTS_GUARD]

- SMELL: `<Import Project="...">` of an optional file without `Condition="Exists('...')"`
- WHY:
  - A missing file fails `MSB4019`
  - An unguarded import inside a package `build/` or `buildTransitive/` folder is the package contract
- RULE: Guard optional imports, a required import stays unguarded

```xml
<!-- BAD -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props" />
<!-- GOOD -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props" Condition="Exists('$(MSBuildThisFileDirectory)eng/custom.props')" />
<!-- OK -->
<Project Sdk="Microsoft.NET.Sdk">
```

### [03.5]-[AP-15]-[ERROR]-[ASSEMBLYINFO_WITH_GENERATEASSEMBLYINFO]

- SMELL: `AssemblyInfo.cs` with an attribute the SDK generates while `GenerateAssemblyInfo` is `true`
- WHY: The compiler sees the attribute twice, `CS0579`
- RULE: Delete the file, set the values as properties, add an attribute the SDK lacks as an `AssemblyAttribute` item

```xml
<!-- BAD: Properties/AssemblyInfo.cs holds [assembly: AssemblyTitle("Library")] -->
<!-- GOOD -->
<AssemblyTitle>Library</AssemblyTitle>
```

### [03.6]-[AP-16]-[ERROR]-[TARGETFRAMEWORK_PLURAL_SINGULAR_CONFUSION]

- SMELL: A list in `TargetFramework`, both properties in one project, or one value in `TargetFrameworks`
- WHY:
  - A list in `TargetFramework` fails `NETSDK1046`, both together take the singular and report `BC0107`
  - One value in `TargetFrameworks` runs an outer and an inner build and adds `_net10.0` to the pivot, STYLE
- RULE: One framework goes in `TargetFramework`, a list in `TargetFrameworks`, the project sets one

```xml
<!-- BAD -->
<TargetFramework>net10.0;netstandard2.0</TargetFramework>
<!-- GOOD -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
```

### [03.7]-[AP-17]-[ERROR]-[PROJECTREFERENCE_CYCLE_OR_UPWARD_EDGE]

- SMELL: Two projects referencing each other, or a library referencing an application, tool, or test project
- WHY:
  - A cycle fails restore with `MSB4006` naming `_GenerateRestoreProjectPathWalk`
  - An upward edge builds with no MSBuild check, STYLE until a validation target errors on it
- RULE:
  - Every edge points to a lower layer
  - A validation target before `PrepareForBuild` errors on a `ProjectReference` outside the layer root

```xml
<!-- BAD: libs/Library/Library.csproj -->
<ProjectReference Include="../../apps/Application/Application.csproj" />
<!-- GOOD: Directory.Build.targets -->
<Error Condition="'@(_UpwardReference)' != ''" Text="Project '$(MSBuildProjectName)' references outside its layer: @(_UpwardReference, ', ')" />
```

## [04]-[TARGETS]

Prove each with a build at `-v:n`, and a repeated build for the incremental case.

### [04.1]-[AP-18]-[STYLE]-[CUSTOM_TARGETS_MISSING_INPUTS_AND_OUTPUTS]

- SMELL: A target writing files without `Inputs` and `Outputs`, or a target combining unrelated work
- WHY: A target without both attributes runs on every build, one stale input reruns every step of a large target
- RULE:
  - Group outputs sharing a dependency and regeneration step
  - `Inputs` lists every driving file, `Outputs` lists every written file, the written file joins `@(FileWrites)` inside the target

```xml
<!-- BAD -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile">
  <Copy SourceFiles="BuildInfo.cs.in" DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
</Target>
<!-- GOOD -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile" Inputs="BuildInfo.cs.in" Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <Copy SourceFiles="BuildInfo.cs.in" DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>
```

### [04.2]-[AP-19]-[ERROR]-[CALLTARGET_FOR_A_DEPENDENCY]

- SMELL: `<CallTarget Targets="Compute" />` followed by a task reading a value the called target sets
- WHY: Values a `CallTarget` target creates reach the caller after it finishes, the next task reads empty
- RULE: Name the dependency in `DependsOnTargets`

```xml
<!-- BAD -->
<Target Name="Report">
  <CallTarget Targets="Compute" />
  <Message Text="$(Computed)" />
</Target>
<!-- GOOD -->
<Target Name="Report" DependsOnTargets="Compute">
  <Message Text="$(Computed)" />
</Target>
```

### [04.3]-[AP-20]-[ERROR]-[COPY_TASK_FOR_AN_OUTPUT_ITEM]

- SMELL: `Copy` placing a content file in `$(OutDir)`, or a target named `AfterBuild` or `BeforeBuild` in a `.csproj` or `.props` file
- WHY:
  - The SDK import follows the project body and replaces a same-named target, `AfterBuild` in a `.csproj` or `.props` file never runs
  - A `Copy` in a custom target reaches the project's own output, a referencing project and `dotnet publish` receive nothing
- RULE: Output files are `None` or `Content` items with `CopyToOutputDirectory`, a custom step attaches with `AfterTargets`

```xml
<!-- BAD -->
<Target Name="AfterBuild">
  <Copy SourceFiles="settings.ini" DestinationFolder="$(OutDir)" />
</Target>
<!-- GOOD -->
<None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
```

### [04.4]-[AP-21]-[ERROR]-[WRITELINESTOFILE_WITHOUT_OVERWRITE]

- SMELL: `WriteLinesToFile` without `Overwrite="true"`
- WHY: The task appends, the file grows by one copy of `Lines` per build
- RULE: Set `Overwrite="true"` and `WriteOnlyWhenDifferent="true"`, the timestamp then holds for the next incremental check

```xml
<!-- BAD -->
<WriteLinesToFile File="$(IntermediateOutputPath)version.txt" Lines="$(Version)" />
<!-- GOOD -->
<WriteLinesToFile File="$(IntermediateOutputPath)version.txt" Lines="$(Version)" Overwrite="true" WriteOnlyWhenDifferent="true" />
```

### [04.5]-[AP-22]-[ERROR]-[DESTINATIONFOLDER_FLATTENS_A_TREE]

- SMELL: `Copy` with `DestinationFolder` over a recursive glob
- WHY: Every file lands in one directory, two files with one name overwrite each other
- RULE: Transform the items with `%(RecursiveDir)` into `DestinationFiles`

```xml
<!-- BAD -->
<Copy SourceFiles="@(Asset)" DestinationFolder="$(OutDir)assets/" />
<!-- GOOD -->
<Copy SourceFiles="@(Asset)" DestinationFiles="@(Asset->'$(OutDir)assets/%(RecursiveDir)%(Filename)%(Extension)')" />
```

## [05]-[EXECUTION_AND_PATHS]

Prove each with a build on the current host, the `Exec` command line reads at `-v:n`.

### [05.1]-[AP-23]-[STYLE]-[HARDCODED_ABSOLUTE_PATHS]

- SMELL: `C:\tools\`, `D:\packages\`, or `/usr/local/bin/` in a project file
- WHY: The path exists on one machine, a missing import or reference fails `MSB4019` or `MSB3245`
- RULE: Compose every path from `$(MSBuildThisFileDirectory)` or `$(MSBuildProjectDirectory)` with `NormalizeDirectory` or `NormalizePath`

```xml
<!-- BAD -->
<ToolPath>C:\tools\mytool\mytool.exe</ToolPath>
<!-- GOOD -->
<ToolPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', 'tools', 'mytool'))</ToolPath>
```

### [05.2]-[AP-24]-[ERROR]-[BACKSLASHES_IN_PATHS]

- SMELL: Backslash separators in a file that builds on more than one operating system
- WHY:
  - On Unix the evaluator converts `\` to `/` in a value whose first segment exists on disk, an item passed to a task converts in every case
  - An `Exec` command starting with a program name reaches `sh` unconverted
- RULE: Write `/` in every path

| [INDEX] | [PLACE]                                                  | [SEVERITY] | [RESULT]                                                  |
| :-----: | :------------------------------------------------------- | :--------- | :-------------------------------------------------------- |
|  [01]   | `Exec` command                                           | ERROR      | `sh` deletes it, `cat data\file.txt` reads `datafile.txt` |
|  [02]   | `Lines` on `WriteLinesToFile`                            | ERROR      | Every item converts, the file receives `a/b` for `a\b`    |
|  [03]   | Custom task passing a path to file APIs                  | ERROR      | No conversion inside the task                             |
|  [04]   | `Import`, item glob, `Copy`, `MakeDir`, or `Delete` path | STYLE      | Converts                                                  |
|  [05]   | `$(MSBuildThisFileDirectory)tools/mytool`                | OK         | Reserved directory property ends with the host separator  |

```xml
<!-- BAD -->
<Exec Command="cat data\file.txt" />
<!-- GOOD -->
<Exec Command="cat data/file.txt" />
```

### [05.3]-[AP-25]-[STYLE]-[EXEC_FOR_A_BUILTIN_TASK_OR_A_PROPERTY_FUNCTION]

- SMELL:
  - `Exec` running `mkdir`, `copy`, `del`, `xcopy`, `touch`, or `echo text > file`
  - `Exec` with `ConsoleToMSBuild` running `sed` or `powershell -c` for a string or path value
- WHY:
  - `Exec` runs `cmd.exe` on Windows and `sh` elsewhere
  - A built-in task runs everywhere, logs each file, and reports errors in one format
  - A property function computes a value at evaluation without a process
- RULE:
  - Replace the command with its task, a string operation with a string function
  - Replace a path operation with a path function taking `$(MSBuildThisFileDirectory)` first
  - A relative argument resolves against the working directory of the build process

| [INDEX] | [COMMAND]             | [TASK]             |
| :-----: | :-------------------- | :----------------- |
|  [01]   | `mkdir`               | `MakeDir`          |
|  [02]   | `copy`, `cp`, `xcopy` | `Copy`             |
|  [03]   | `del`, `rm`           | `Delete`           |
|  [04]   | `rmdir`, `rd`         | `RemoveDir`        |
|  [05]   | `move`, `mv`          | `Move`             |
|  [06]   | `echo text > file`    | `WriteLinesToFile` |
|  [07]   | `touch`               | `Touch`            |

```xml
<!-- BAD -->
<Exec Command="mkdir $(OutDir)logs" />
<Exec Command="echo $(Version) | sed 's/-preview//'" ConsoleToMSBuild="true">
  <Output TaskParameter="ConsoleOutput" PropertyName="CleanVersion" />
</Exec>
<!-- GOOD -->
<MakeDir Directories="$(OutDir)logs" />
<CleanVersion>$(Version.Replace('-preview', ''))</CleanVersion>
```

### [05.4]-[AP-26]-[ERROR]-[PLATFORM_SPECIFIC_EXEC_WITHOUT_OS_CONDITION]

- SMELL: `<Exec Command="chmod +x ..." />` or `<Exec Command="cmd /c ..." />` without an operating system condition
- WHY: The command fails on the other operating system
- RULE: Condition the task or its target on `$([MSBuild]::IsOSPlatform('...'))`

```xml
<!-- BAD -->
<Target Name="MakeExecutable" AfterTargets="Build">
  <Exec Command="chmod +x $(OutDir)mytool" />
</Target>
<!-- GOOD -->
<Target Name="MakeExecutable" AfterTargets="Build" Condition="!$([MSBuild]::IsOSPlatform('Windows'))">
  <Exec Command="chmod +x $(OutDir)mytool" />
</Target>
```

### [05.5]-[AP-27]-[ERROR]-[EXEC_BUILDS_A_PROJECT]

- SMELL: `<Exec Command="dotnet build ..." />`, `dotnet pack`, `dotnet publish`, or `msbuild` inside a target
- WHY: The command starts a build process the engine cannot schedule or log, global properties reach it by hand, `-check` reports `BC0302`
- RULE: Order a project through `ProjectReference` with `ReferenceOutputAssembly="false"`, call a target through the `MSBuild` task

```xml
<!-- BAD -->
<Target Name="BuildTool" BeforeTargets="Build">
  <Exec Command="dotnet build ../Tool/Tool.csproj -c $(Configuration)" />
</Target>
<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" ReferenceOutputAssembly="false" />
```

## [06]-[BUILD_GRAPH]

Prove each with `binlog_evaluations` on a `-bl:{}.binlog` build, two evaluations of one project outside restore share the output paths.

### [06.1]-[AP-28]-[ERROR]-[DUPLICATE_PROJECT_INSTANCE_WITH_SHARED_OUTPUT_PATH]

- SMELL: An `<MSBuild>` call with a `Properties` value the target project's output path lacks (`_IsPublishing=true`)
- WHY:
  - One instance exists per project path and global property set, the call creates `(project, {_IsPublishing=true})` beside `(project, {})`
  - Both run every target against one `bin/` and `obj/`, `CoreCompile` runs twice
  - `-check` reports `BC0102` for the copies and `BC0202` for the `_IsPublishing` read, a parallel build can report `MSB3026`
- RULE:
  - Run `Publish` in the same instance through `DependsOnTargets`
  - Condition the target on `'$(_IsPublishing)' == ''`, no condition fails `dotnet publish` with `MSB4006`, a `!= 'true'` read reports `BC0201`
  - Leave `_IsPublishing` to the SDK, a build under `_IsPublishing=true` with `PublishRuntimeIdentifier` set moves to that runtime's output path

```xml
<!-- BAD -->
<Target Name="PublishOnBuild" AfterTargets="Build" Condition="'$(_IsPublishing)' == ''">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>
<!-- GOOD -->
<Target Name="PublishOnBuild" AfterTargets="Build" DependsOnTargets="Publish" Condition="'$(_IsPublishing)' == ''" />
```

### [06.2]-[AP-29]-[ERROR]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_PROJECTREFERENCE]

- SMELL: `ProjectReference` with `SetTargetFramework="TargetFramework=net10.0"` to a project declaring `<TargetFramework>net10.0</TargetFramework>`
- WHY:
  - The metadata passes `TargetFramework` as a global property, `(project, {TargetFramework=net10.0})` sits beside the solution's `(project, {})`
  - Both resolve to one `bin/Debug/net10.0/` and `obj/Debug/net10.0/`
  - The `ProjectReference` protocol removes the global property for a single-targeting reference, the metadata reintroduces it
- RULE:
  - A single-targeting reference takes no `SetTargetFramework`, a multi-targeting reference or a build under another framework takes it
  - An incompatible reference takes `SkipGetTargetFrameworkProperties="true"` with `UndefineProperties="TargetFramework"` or `SetTargetFramework`

```xml
<!-- BAD -->
<ProjectReference Include="../Tool/Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />
<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" />
<!-- OK -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" UndefineProperties="TargetFramework" ReferenceOutputAssembly="false" />
```
