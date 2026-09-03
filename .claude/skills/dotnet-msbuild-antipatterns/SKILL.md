---
name: dotnet-msbuild-antipatterns
description: "Use when reviewing a .csproj, .props, or .targets file: MSBuild evaluation, placement, item, target, path, and build graph smells with severity, corrected form, and proof."
---

# [DOTNET_MSBUILD_ANTIPATTERNS]

Covers the review catalog for project and build files: each entry names the smell, the effect, the rule, and a `BAD` form beside its `GOOD` form, and `OK` marks a form to leave alone. Each heading carries a severity: `ERROR` names a build failure, a wrong value, or a wrong output, and `STYLE` names a form that builds and costs maintenance or time.

- `dotnet-msbuild-evaluation` owns import order, conditions, properties, items, and file placement
- `dotnet-msbuild-execution` owns targets, `DependsOnTargets`, `Inputs` and `Outputs`, `FileWrites`, and copy modes
- `dotnet-msbuild-diagnostics` owns binlog capture and query, the `-check` workflow, and shared output paths
- `dotnet-msbuild-packaging` owns NuGet package authoring, central package management, lock files, solution files, and CI properties
- `monorepo-build-infrastructure` owns the `eng/` directory, task runner targets, native packaging projects, and provisioning

[REFERENCES]:
- [01]-[WORKED_EXAMPLES](references/worked-examples.md): Full files for publish instances, framework pins, host references, layer checks, and backslash cases

Delete the output directories and run `dotnet build <solution> -check` before you read the files. `BC0101`, `BC0102`, `BC0107`, and `BC0302` report shared output paths, double writes, both framework properties, and `Exec` builds. No check reports a property override or a misplaced property, read the files for those.

[AGENT]: `msbuild-fixer` reviews one scope and returns fixes with proof, use it when the scope exceeds the context window.

## [01]-[EVALUATION]

Evaluation smells produce a wrong value before any target runs. Prove each with `dotnet msbuild <project> -getProperty:<Name>`.

### [AP-01]-[ERROR]-[UNQUOTED_CONDITION_OPERANDS]

- SMELL: `Condition="$(Foo) == net10.0"`, either side of a comparison lacks single quotes
- WHY: MSBuild parses the condition before it expands the property. Unquoted literals with `.`, `,`, or a space fail with `MSB4092`, one with `:` fails with `MSB4090`, and an unclosed quote fails with `MSB4101`. Unquoted properties expand to one token and never fail, two empty properties compare equal, and an empty value fails with `MSB4113` in a condition with no operator and with `MSB4086` in a numeric comparison.
- RULE: Quote both sides of `==` and `!=`

```xml
<!-- BAD: MSB4092, the literal splits at the dot -->
<Optimize Condition="$(TargetFramework) == net10.0">true</Optimize>
<!-- GOOD -->
<Optimize Condition="'$(TargetFramework)' == 'net10.0'">true</Optimize>
```

### [AP-02]-[ERROR]-[SIDE_EFFECTS_DURING_PROPERTY_EVALUATION]

- SMELL: Property functions that read or write the file system inside a `PropertyGroup`
- WHY: MSBuild evaluates a project on every design-time build, `-getProperty` query, and project instance, the read repeats without a build, and a file that a target writes is one build stale when evaluation reads it. MSBuild blocks a write (`WriteAllText`) with `MSB4185`, and `MSBUILDENABLEALLPROPERTYFUNCTIONS=1` enables every function.
- RULE: Read files inside a target, and a read of a file no target writes is STYLE

```xml
<!-- BAD: read on every evaluation -->
<GitHead>$([System.IO.File]::ReadAllText('$(MSBuildThisFileDirectory).git/HEAD'))</GitHead>
<!-- GOOD: read when the consumer runs -->
<Target Name="ReadGitHead" BeforeTargets="CoreCompile">
  <ReadLinesFromFile File="$(MSBuildThisFileDirectory).git/HEAD">
    <Output TaskParameter="Lines" PropertyName="GitHead" />
  </ReadLinesFromFile>
</Target>
```

### [AP-03]-[ERROR]-[GLOBAL_PROPERTY_REASSIGNED_IN_PROJECT_XML]

- SMELL: Project XML assigns a property that the command line, a `-p:` switch in `Directory.Build.rsp`, or an `<MSBuild>` task can supply, for example to add a trailing slash
- WHY: Global properties are read-only during evaluation, the assignment applies when nothing supplies the property and is skipped when something does, and `-p:ToolRoot=/usr/tool` leaves `$(ToolRoot)bin/` as `/usr/toolbin/`
- RULE: Derive the normalized value into a `_` prefixed property and read that, or declare `TreatAsLocalProperty` on the `Project` element when the file must win over the command line

```xml
<!-- BAD: skipped under -p:ToolRoot=/usr/tool -->
<ToolRoot>$([MSBuild]::NormalizeDirectory('$(ToolRoot)'))</ToolRoot>
<!-- GOOD -->
<_ToolDir>$([MSBuild]::NormalizeDirectory('$(ToolRoot)'))</_ToolDir>
<ToolBin>$(_ToolDir)bin/</ToolBin>
```

## [02]-[PLACEMENT]

Placement smells put a setting in a file that reads it too early, too late, or twice. Prove each with `-getProperty` from one project, and with `-pp:` when the assignment source is in question.

### [AP-04]-[ERROR]-[PROPERTY_DEFAULTS_IN_TARGETS_FILES]

- SMELL: Only a `.targets` file sets a default that a `.props` file or the project body reads
- WHY: `.targets` files import after the project body, and every earlier reader sees an empty value
- RULE: `.props` owns overridable defaults, `.targets` owns targets and values derived from properties the SDK sets later

```xml
<!-- BAD: custom.targets, $(ToolVersion) is empty in every .props reader -->
<ToolVersion>2.0</ToolVersion>
<!-- GOOD: custom.props -->
<ToolVersion Condition="'$(ToolVersion)' == ''">2.0</ToolVersion>
```

### [AP-05]-[STYLE]-[UNCONDITIONAL_PROPERTY_OVERRIDE_IN_MULTIPLE_SCOPES]

- SMELL: `Directory.Build.props` and a `.csproj` both assign one property without a condition, and the last assignment wins with no message
- WHY: Readers cannot tell which value the build uses without `-getProperty`
- RULE: The `.props` default carries `Condition="'$(Name)' == ''"`, and the project assigns only another value. `OutputPath` and `IntermediateOutputPath` never take a default this way, an assigned value drops the `Configuration` segment, and `BaseOutputPath` or `ArtifactsPath` sets the root.

```xml
<!-- BAD: Directory.Build.props, the project assigns false and nothing says which wins -->
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<!-- GOOD: Directory.Build.props -->
<GenerateDocumentationFile Condition="'$(GenerateDocumentationFile)' == ''">true</GenerateDocumentationFile>
```

### [AP-06]-[ERROR]-[PROPS_CONDITION_ON_A_LATER_VALUE]

- SMELL: `PropertyGroup` or property conditions in `Directory.Build.props` or any `.props` file on `$(TargetFramework)`, `$(OutputType)`, an SDK-computed path, or a property the project body sets
- WHY: The project body sets `TargetFramework` and its own properties after the `.props` import, the SDK `.targets` compute `OutputPath`, `IntermediateOutputPath`, `TargetPath`, and `TargetFrameworkMoniker`, and the condition compares an empty string and never matches. Multi-targeting inner builds receive `TargetFramework` as a global property and match, and the outer build leaves it empty.
- RULE: Condition these properties in `Directory.Build.targets` or after the assignment in the project file, key a `.props` condition the SDK needs early on `$(MSBuildProjectName)` or the project directory, and never flag an `ItemGroup`, item, `PackageVersion`, or `Target` condition, which evaluates after every property

```xml
<!-- BAD: Directory.Build.props, $(TargetFramework) is empty here -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
<!-- GOOD: Directory.Build.targets, the body already ran -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
<!-- OK: Directory.Packages.props, items read the final value -->
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageVersion Include="Some.Package" Version="10.0.0" />
</ItemGroup>
```

### [AP-07]-[ERROR]-[ARTIFACTSPATH_IN_A_PROJECT_FILE]

- SMELL: `ArtifactsPath`, `UseArtifactsOutput`, `ArtifactsProjectName`, or `BaseIntermediateOutputPath` in a project file
- WHY: The SDK reads these before the project body. `ArtifactsPath` and `UseArtifactsOutput` fail with `NETSDK1199`, `BaseIntermediateOutputPath` warns with `MSB3539` after restore used the default, and `ArtifactsProjectName` renames `bin/` alone and leaves `obj/` under the project name. The SDK reads `ArtifactsPivots` and the output name properties after the body, and a project file can set them.
- RULE: Set the artifacts properties in `Directory.Build.props` or on the command line

```xml
<!-- BAD: MyProject.csproj, NETSDK1199 -->
<ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>
<!-- GOOD: Directory.Build.props, no trailing slash because the SDK appends the segment -->
<ArtifactsPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', 'artifacts'))</ArtifactsPath>
```

### [AP-08]-[STYLE]-[RESTATING_AN_SDK_DEFAULT_OR_A_ROOT_VALUE]

- SMELL: Properties set to the SDK default (`OutputType=Library`, `EnableDefaultItems=true`, `RootNamespace` equal to the project name, `LangVersion` equal to the framework default), or a project file that repeats a `Directory.Build.props` value (`Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `TargetFramework`)
- WHY: The copies hide the real overrides and keep the old value after an SDK or root change. `LangVersion` defaults to `14.0` under `net10.0` and `7.3` under `netstandard2.0`, and a pin below the default holds the sources at the older version.
- RULE: Project files hold only values that differ from the SDK default and the root file, and `LangVersion` appears only where the framework default is below what the sources need

```xml
<!-- BAD: MyProject.csproj restates the default and the root file -->
<OutputType>Library</OutputType>
<Nullable>enable</Nullable>
<!-- GOOD: netstandard2.0 analyzer project, the default is 7.3 -->
<LangVersion>latest</LangVersion>
```

### [AP-09]-[STYLE]-[NOWARN_IN_A_PROJECT_FILE]

- SMELL: `NoWarn` with a compiler or analyzer code in a project file, or `NoWarn` assigned without `$(NoWarn);`
- WHY: `.editorconfig` owns the severity of compiler and analyzer diagnostics per path, and `dotnet_diagnostic.CS1591.severity = none` under `[*.cs]` removes the warning without a project edit. `NoWarn` is the only switch for `NU*` and `MSB*` codes, and an assignment without `$(NoWarn);` drops the SDK default `1701;1702`.
- RULE: Compiler and analyzer codes go to `.editorconfig`, restore and MSBuild codes go to `NoWarn` in `Directory.Build.props`, and every `NoWarn` assignment starts with `$(NoWarn);`

```xml
<!-- BAD: MyProject.csproj -->
<NoWarn>CS1591</NoWarn>
<!-- GOOD: Directory.Build.props, restore code only -->
<NoWarn>$(NoWarn);NU1603</NoWarn>
```

### [AP-10]-[ERROR]-[RSP_OR_SOLUTION_PROPS_FOR_A_PROJECT_SETTING]

- SMELL: `-p:Name=Value` in `Directory.Build.rsp`, or a property in `Directory.Solution.props` that a project reads
- WHY: `-p:` switches in the response file are global properties that no project can override, and `Directory.Solution.props` imports into the solution project alone, where its properties reach no project under any entry point
- RULE: Project settings belong in `Directory.Build.props`, `Directory.Build.rsp` holds command-line switches only, and `Directory.Solution.props` holds solution project settings only

```xml
<!-- BAD: Directory.Build.rsp turns Configuration into a global property -->
-p:Configuration=Release
<!-- GOOD: Directory.Build.props, a default the command line still overrides -->
<Configuration Condition="'$(Configuration)' == ''">Release</Configuration>
```

## [03]-[ITEMS_AND_REFERENCES]

Item and reference smells produce a missing item, a doubled item, or a copy the output must not carry. Prove each with `dotnet msbuild <project> -getItem:<Type>`.

### [AP-11]-[ERROR]-[ITEM_UPDATE_OR_REMOVE_BEFORE_THE_SDK_INCLUDE]

- SMELL: `Update` or `Remove` in `Directory.Build.props`, an `Update` with a pattern that matches no item, or an `Include` for a file the default glob already matches
- WHY: `Update` and `Remove` act on items that exist at that point, items evaluate in order across imports, the SDK globs import after `Directory.Build.props`, and the element matches nothing and reports nothing. Duplicate `Compile` items fail with `NETSDK1022`, a full-path duplicate passes that check and warns with `CS2002`, and a duplicate `None` copies twice with no message.
- RULE: `Update` and `Remove` go in the project file or `Directory.Build.targets`, an exclusion goes to `DefaultItemExcludes` in `.props`, and `EnableDefaultCompileItems=false` with a hand-written list never replaces an exclusion

```xml
<!-- BAD: Directory.Build.props, no None item exists yet -->
<None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
<Compile Remove="Generated/**" />
<!-- GOOD: Directory.Build.props excludes, Directory.Build.targets updates -->
<DefaultItemExcludes>$(DefaultItemExcludes);Generated/**</DefaultItemExcludes>
<None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
```

### [AP-12]-[ERROR]-[REFERENCE_WITH_HINTPATH_FOR_A_PACKAGE_OR_PROJECT]

- SMELL: `Reference` with a `HintPath` into a `packages/` folder or into another project's `bin/`, or a `Reference` to a host-supplied assembly without `Private="false"`
- WHY: Nothing restores a `packages/` folder for an SDK project, and the build warns with `MSB3245`. `Reference` to a project output loses the build order, which `BC0104` reports. Host-supplied assemblies without `Private="false"` copy into the output, and the host loads that copy.
- RULE: Packages are `PackageReference` items, projects are `ProjectReference` items, and a host-supplied assembly is a `Reference` with `HintPath` from a property and `Private="false"`

```xml
<!-- BAD -->
<Reference Include="Newtonsoft.Json" HintPath="../packages/Newtonsoft.Json.13.0.3/lib/netstandard2.0/Newtonsoft.Json.dll" />
<!-- GOOD: Directory.Packages.props owns the version -->
<PackageReference Include="Newtonsoft.Json" />
<!-- OK: the host loads the assembly, the output never carries it -->
<Reference Include="HostCore" HintPath="$(HostAssemblyDir)HostCore.dll" Private="false" />
```

### [AP-13]-[STYLE]-[REDUNDANT_PROJECTREFERENCE_TO_A_TRANSITIVE_DEPENDENCY]

- SMELL: `Core` references `Utils`, the project references both, and the project uses no `Utils` type
- WHY: The SDK makes project references transitive through `project.assets.json` unless `DisableTransitiveProjectReferences` is `true`, and the extra edge adds coupling and hides the graph
- RULE: Reference only the projects with types the sources name

```xml
<!-- BAD -->
<ProjectReference Include="../Core/Core.csproj" />
<ProjectReference Include="../Utils/Utils.csproj" />
<!-- GOOD -->
<ProjectReference Include="../Core/Core.csproj" />
```

### [AP-14]-[ERROR]-[IMPORT_WITHOUT_EXISTS_GUARD]

- SMELL: `<Import Project="...">` for an optional file without `Condition="Exists('...')"`
- WHY: Missing files fail with `MSB4019`, required imports stay unguarded and a missing one then fails at once, and an unguarded import inside a package `build/` or `buildTransitive/` folder is the package contract, which the package layout resolves
- RULE: Guard optional imports only

```xml
<!-- BAD -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props" />
<!-- GOOD -->
<Import Project="$(MSBuildThisFileDirectory)eng/custom.props" Condition="Exists('$(MSBuildThisFileDirectory)eng/custom.props')" />
<!-- OK: the SDK import is required -->
<Project Sdk="Microsoft.NET.Sdk">
```

### [AP-15]-[ERROR]-[ASSEMBLYINFO_WITH_GENERATEASSEMBLYINFO]

- SMELL: `AssemblyInfo.cs` with `AssemblyTitle`, `AssemblyVersion`, or another attribute the SDK generates, while `GenerateAssemblyInfo` keeps its default of `true`
- WHY: The compiler sees the attribute twice and fails with `CS0579`
- RULE: Delete the file, set the values as properties, and add an attribute the SDK lacks as an `AssemblyAttribute` item

```xml
<!-- BAD: Properties/AssemblyInfo.cs holds [assembly: AssemblyTitle("Library")] -->
<!-- GOOD: the project file -->
<AssemblyTitle>Library</AssemblyTitle>
```

### [AP-16]-[ERROR]-[TARGETFRAMEWORK_PLURAL_SINGULAR_CONFUSION]

- SMELL: `TargetFramework` with a semicolon list, both `TargetFramework` and `TargetFrameworks` in one project, or `TargetFrameworks` with one value
- WHY: Lists in `TargetFramework` fail with `NETSDK1046`, both properties together take the singular and `-check` reports `BC0107`, and one value in `TargetFrameworks` runs an outer build plus an inner build and adds `_net10.0` to the artifacts pivot, which is STYLE
- RULE: One framework goes in `TargetFramework`, a list goes in `TargetFrameworks`, and the project sets one of them

```xml
<!-- BAD: NETSDK1046 -->
<TargetFramework>net10.0;netstandard2.0</TargetFramework>
<!-- GOOD -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
```

### [AP-17]-[ERROR]-[PROJECTREFERENCE_CYCLE_OR_UPWARD_EDGE]

- SMELL: Two projects that reference each other, or a `ProjectReference` from a library to an application, a tool, or a test project
- WHY: Cycles fail restore with `MSB4006` naming `_GenerateRestoreProjectPathWalk`, and an upward edge builds with no MSBuild check and is STYLE until a validation target makes it an error
- RULE: Every edge points to a lower layer, and a target before `PrepareForBuild` compares `@(ProjectReference->'%(FullPath)')` with the layer root and errors on a match outside it

```xml
<!-- BAD: libs/Library/Library.csproj -->
<ProjectReference Include="../../apps/Application/Application.csproj" />
<!-- GOOD: Directory.Build.targets, the full target is in references/worked-examples.md -->
<Error Condition="'@(_UpwardReference)' != ''" Text="Library '$(MSBuildProjectName)' references outside libs/: @(_UpwardReference, ', ')" />
```

## [04]-[TARGETS]

Target smells run at the wrong time, run every build, or leave a value unseen. Prove each with a build at `-v:n` and a repeated build for the incremental case.

### [AP-18]-[STYLE]-[CUSTOM_TARGETS_MISSING_INPUTS_AND_OUTPUTS]

- SMELL: Targets that write files without `Inputs` and `Outputs`, or a `Target` of 50 or more lines that does unrelated work
- WHY: MSBuild skips a target as a whole and only when every output is at least as new as every input, a target without both attributes runs on every build, and one stale input reruns every unrelated step in a large target
- RULE: One target per output, `Inputs` lists every file that drives the output, `Outputs` lists every file written, and the written file joins `@(FileWrites)` from an `ItemGroup` inside the target

```xml
<!-- BAD: runs every build -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile">
  <Copy SourceFiles="BuildInfo.cs.in" DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
</Target>
<!-- GOOD: skipped when up to date -->
<Target Name="GenerateBuildInfo" BeforeTargets="CoreCompile" Inputs="BuildInfo.cs.in" Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <Copy SourceFiles="BuildInfo.cs.in" DestinationFiles="$(IntermediateOutputPath)BuildInfo.g.cs" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>
```

### [AP-19]-[ERROR]-[CALLTARGET_FOR_A_DEPENDENCY]

- SMELL: `<CallTarget Targets="Compute" />` followed by a task that reads a property or item the called target sets
- WHY: Properties and items that a `CallTarget` target creates reach the calling target only after it finishes, and the following task reads an empty value
- RULE: Name the dependency in `DependsOnTargets`

```xml
<!-- BAD: $(Computed) is empty in the Message -->
<Target Name="Report">
  <CallTarget Targets="Compute" />
  <Message Text="$(Computed)" />
</Target>
<!-- GOOD -->
<Target Name="Report" DependsOnTargets="Compute">
  <Message Text="$(Computed)" />
</Target>
```

### [AP-20]-[ERROR]-[COPY_TASK_FOR_AN_OUTPUT_ITEM]

- SMELL: `Copy` tasks in a custom target that place a content file in `$(OutDir)`, or a target named `AfterBuild` or `BeforeBuild` in a `.csproj`
- WHY: The SDK import follows the project body and replaces a same-named target, and `AfterBuild` in a `.csproj` never runs. `Copy` tasks in a custom target reach this project's output only, and a referencing project and `dotnet publish` never receive the file.
- RULE: Files that belong in the output are `None` or `Content` items with `CopyToOutputDirectory`, and a custom step extends the chain with `AfterTargets`

```xml
<!-- BAD: never runs in an SDK project -->
<Target Name="AfterBuild">
  <Copy SourceFiles="settings.ini" DestinationFolder="$(OutDir)" />
</Target>
<!-- GOOD: reaches every referencing project and the publish directory -->
<None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
```

### [AP-21]-[ERROR]-[WRITELINESTOFILE_WITHOUT_OVERWRITE]

- SMELL: `WriteLinesToFile` without `Overwrite="true"`
- WHY: The task appends by default, and the file grows by one copy of `Lines` on every build
- RULE: Set `Overwrite="true"` and `WriteOnlyWhenDifferent="true"`, which keeps the timestamp for the next incremental check

```xml
<!-- BAD: two lines after two builds -->
<WriteLinesToFile File="$(IntermediateOutputPath)version.txt" Lines="$(Version)" />
<!-- GOOD -->
<WriteLinesToFile File="$(IntermediateOutputPath)version.txt" Lines="$(Version)" Overwrite="true" WriteOnlyWhenDifferent="true" />
```

### [AP-22]-[ERROR]-[DESTINATIONFOLDER_FLATTENS_A_TREE]

- SMELL: `Copy` with `DestinationFolder` over a recursive glob
- WHY: `DestinationFolder` writes every file under one directory, two files with one name overwrite each other, and the tree is lost
- RULE: Transform the items with `%(RecursiveDir)` into `DestinationFiles`

```xml
<!-- BAD: assets/a/b/two.txt lands beside one.txt -->
<Copy SourceFiles="@(Asset)" DestinationFolder="$(OutDir)assets/" />
<!-- GOOD -->
<Copy SourceFiles="@(Asset)" DestinationFiles="@(Asset->'$(OutDir)assets/%(RecursiveDir)%(Filename)%(Extension)')" />
```

## [05]-[EXECUTION_AND_PATHS]

Execution and path smells run a process or name a path that works on one machine only. Prove each with a build on this host and read the `Exec` command line at `-v:n`.

### [AP-23]-[STYLE]-[HARDCODED_ABSOLUTE_PATHS]

- SMELL: `C:\tools\`, `D:\packages\`, or `/usr/local/bin/` in a project file
- WHY: The path exists on one machine, and a missing import or reference then fails with `MSB4019` or `MSB3245`
- RULE: Compose every path from `$(MSBuildThisFileDirectory)` or `$(MSBuildProjectDirectory)` with `NormalizeDirectory` or `NormalizePath`

```xml
<!-- BAD -->
<ToolPath>C:\tools\mytool\mytool.exe</ToolPath>
<!-- GOOD -->
<ToolPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', 'tools', 'mytool'))</ToolPath>
```

### [AP-24]-[ERROR]-[BACKSLASHES_IN_PATHS]

- SMELL: Backslash separators in a file that builds on more than one operating system
- WHY: On Unix the evaluator converts `\` to `/` only when the string looks like a path and its first segment exists on disk. An `Exec` command that starts with a program name gets no conversion, `sh` deletes the backslash, and `cat data\file.txt` reads `datafile.txt`. Backslashes in `Import`, an item glob, or a path that `Copy`, `MakeDir`, or `Delete` consumes convert, and that case is STYLE.
- RULE: Write `/` in every path, and `references/worked-examples.md` lists the other ERROR cases

```xml
<!-- BAD: sh deletes the backslash -->
<Exec Command="cat data\file.txt" />
<!-- GOOD -->
<Exec Command="cat data/file.txt" />
<!-- OK: STYLE, the import converts on Unix -->
<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />
```

### [AP-25]-[STYLE]-[EXEC_FOR_A_BUILTIN_TASK_OR_A_PROPERTY_FUNCTION]

- SMELL: `Exec` running `mkdir`, `copy`, `del`, `xcopy`, `touch`, or `echo text > file`, or `Exec` with `ConsoleToMSBuild` running `sed` or `powershell -c` for a string or path value
- WHY: `Exec` runs `cmd.exe` on Windows and `sh` elsewhere, a built-in task runs on every operating system, logs each file, and reports errors in one format, and a property function computes a string or path value at evaluation without a process
- RULE: Replace the command with its task, a string operation with a string property function, and a path operation with a path property function that takes `$(MSBuildThisFileDirectory)` as its first argument, because a relative argument resolves against the working directory of the build process

| [INDEX] | [COMMAND]             | [TASK]             |
| :-----: | :-------------------- | :----------------- |
|  [01]   | `mkdir`               | `MakeDir`          |
|  [02]   | `copy`, `cp`, `xcopy` | `Copy`             |
|  [03]   | `del`, `rm`           | `Delete`           |
|  [04]   | `move`, `mv`          | `Move`             |
|  [05]   | `echo text > file`    | `WriteLinesToFile` |
|  [06]   | `touch`               | `Touch`            |

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

### [AP-26]-[ERROR]-[PLATFORM_SPECIFIC_EXEC_WITHOUT_OS_CONDITION]

- SMELL: `<Exec Command="chmod +x ..." />` or `<Exec Command="cmd /c ..." />` without an operating system condition
- WHY: The command fails on the other operating system
- RULE: Guard the target with `$([MSBuild]::IsOSPlatform('...'))`

```xml
<!-- BAD: fails on Windows -->
<Target Name="MakeExecutable" AfterTargets="Build">
  <Exec Command="chmod +x $(OutDir)mytool" />
</Target>
<!-- GOOD -->
<Target Name="MakeExecutable" AfterTargets="Build" Condition="!$([MSBuild]::IsOSPlatform('Windows'))">
  <Exec Command="chmod +x $(OutDir)mytool" />
</Target>
```

### [AP-27]-[ERROR]-[EXEC_BUILDS_A_PROJECT]

- SMELL: `<Exec Command="dotnet build ..." />`, `dotnet pack`, `dotnet publish`, or `msbuild` inside a target
- WHY: The command starts a separate build process that the engine cannot schedule or log, global properties reach it only by hand, and `-check` reports `BC0302`
- RULE: Order a project through `ProjectReference` with `ReferenceOutputAssembly="false"`, and call a specific target through the `MSBuild` task

```xml
<!-- BAD: BC0302 -->
<Target Name="BuildTool" BeforeTargets="Build">
  <Exec Command="dotnet build ../Tool/Tool.csproj -c $(Configuration)" />
</Target>
<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" ReferenceOutputAssembly="false" />
```

## [06]-[BUILD_GRAPH]

Build graph smells build one project twice. Prove each with `binlog_evaluations` on a `-bl:{}` build, where two evaluations of one project outside restore share the output paths.

### [AP-28]-[ERROR]-[DUPLICATE_PROJECT_INSTANCE_WITH_SHARED_OUTPUT_PATH]

- SMELL: `<MSBuild>` task calls with a `Properties` value that the target project's output path does not contain (`_IsPublishing=true`)
- WHY: MSBuild creates one instance per project path and global property set, the call creates `(project, {_IsPublishing=true})` beside `(project, {})`, both run every target against one `bin/` and `obj/`, and `CoreCompile` runs twice. `-check` reports `BC0102` for the copies and `BC0202` for the read of `_IsPublishing`, and a parallel build races the writes and reports `MSB3026`.
- RULE: Run `Publish` in the same instance through `DependsOnTargets`, keep the `_IsPublishing` condition because without it `dotnet publish` fails with `MSB4006`, and never set `_IsPublishing`, which the SDK owns and which moves a plain build to the `PublishRuntimeIdentifier` output path when `true`

```xml
<!-- BAD: another instance shares bin/ and obj/ -->
<Target Name="PublishOnBuild" AfterTargets="Build" Condition="'$(_IsPublishing)' != 'true'">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>
<!-- GOOD: one instance, the consumer form and the BC0201 setting are in references/worked-examples.md -->
<Target Name="PublishOnBuild" AfterTargets="Build" DependsOnTargets="Publish" Condition="'$(_IsPublishing)' != 'true'" />
```

### [AP-29]-[ERROR]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_PROJECTREFERENCE]

- SMELL: `ProjectReference` with `SetTargetFramework="TargetFramework=net10.0"` to a project that declares `<TargetFramework>net10.0</TargetFramework>`
- WHY: The metadata passes `TargetFramework` as a global property, which creates `(project, {TargetFramework=net10.0})` beside the `(project, {})` the solution builds, and both resolve to one `bin/Debug/net10.0/` and `obj/Debug/net10.0/`. The `ProjectReference` protocol removes the global property for a single-targeting reference, and the metadata reintroduces it.
- RULE: Single-targeting references carry no `SetTargetFramework`, a multi-targeting reference or a build under another framework carries it, and an incompatible reference carries `SkipGetTargetFrameworkProperties="true"` with `UndefineProperties="TargetFramework"` or `SetTargetFramework`, never both

```xml
<!-- BAD: Tool.csproj already targets net10.0 -->
<ProjectReference Include="../Tool/Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />
<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" />
<!-- OK: incompatible frameworks, the consumer's TargetFramework never reaches the tool -->
<ProjectReference Include="../Tool/Tool.csproj" SkipGetTargetFrameworkProperties="true" UndefineProperties="TargetFramework" ReferenceOutputAssembly="false" />
```

## [07]-[PACKAGING_SMELLS]

`dotnet-msbuild-packaging` owns the rule and the fix for the packaging smells:
- `.nuspec` beside an SDK project
- `GeneratePackageOnBuild` in a library
- `Version` or `PackageVersion` set per project
- `PackageReference` with `Version` under central package management
- Nested `Directory.Packages.props` without the root import
- Lock file and `RestoreLockedMode` settings outside the root file
- `ContinuousIntegrationBuild` and other CI properties per project
