---
name: dotnet-msbuild-execution
description: "Use when writing or ordering an MSBuild target, covering hook points, Inputs and Outputs, generated files, batching, errors, and copy to output."
---

# [DOTNET_MSBUILD_EXECUTION]

MSBuild execution phase, from target order to copy-to-output rules.

[REFERENCES]:
- [01]-[HOOK_POINTS](references/hook-points.md): Every SDK hook point with its phase and the items and properties present there
- [02]-[TASK_PARAMETERS](references/task-parameters.md): Task parameters that decide correctness and incremental behavior, with the inline task form

## [01]-[TARGET_ORDERING]

MSBuild runs each target at most once per project instance, in an order the target attributes decide. For one requested target the engine runs its `DependsOnTargets` left to right, the targets naming it in `BeforeTargets`, the body, then the targets naming it in `AfterTargets`. `InitialTargets` run before every requested target, `-target:` replaces `DefaultTargets`, the last definition of a target name wins, names compare without case.

| [INDEX] | [ATTRIBUTE]        | [OWNER]          | [USE_WHEN]                                                                |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | `DependsOnTargets` | Consuming target | Named target is your own and finishes before the consumer reads           |
|  [02]   | `BeforeTargets`    | Inserted target  | Named target is another file's and runs after the inserted one            |
|  [03]   | `AfterTargets`     | Inserted target  | Named target is another file's and produces what the inserted consumes    |

- Combine attributes on one target, `DependsOnTargets` for your own chain and `BeforeTargets` or `AfterTargets` for the SDK target
- Target `Condition` evaluates when the target is about to run, after earlier targets updated the properties and items it reads
- False `Condition` skips the body and the `DependsOnTargets` chain, targets naming the skipped target in `BeforeTargets` and `AfterTargets` run once
- Skipped targets count as not run, a later `DependsOnTargets` request runs one when its condition holds at that time
- `Inputs` and `Outputs` compare after the dependencies and `BeforeTargets` ran and before the body
- `BeforeTargets` and `AfterTargets` accept a name no target defines, the target then never runs, detailed verbosity logs the unmatched name
- `Returns` names the items a caller receives, `Outputs` names the files of the up-to-date check
- Once one target in the project declares `Returns`, a target with `Outputs` alone returns nothing, `Build` declares `Returns` in every SDK project
- Returned items are recorded when the target completes, duplicates drop unless `KeepDuplicateOutputs="true"`
- Targets appending to the item list through `AfterTargets` change nothing the caller receives
- `Label` is an identifier for tooling

```xml
<Target Name="GetStageOutputs" DependsOnTargets="CollectStageOutputs" Returns="@(StageOutput)" />
```

```bash
dotnet msbuild <project>.csproj -getTargetResult:GetStageOutputs
dotnet msbuild <project>.csproj -getTargetResult:GetStageOutputs -getItem:StageOutput
dotnet msbuild <project>.csproj -targets
```

`-targets` prints every available target name without building.

## [02]-[SDK_HOOK_POINTS]

Extend the SDK chain from a target naming an SDK target in `BeforeTargets` or `AfterTargets`, or from a `DependsOn` property a `.targets` file appends to after the SDK assigned it. `BeforeBuild`, `AfterBuild`, `BeforeCompile`, `AfterCompile`, `BeforeResolveReferences`, `AfterResolveReferences`, `BeforeClean`, `AfterClean`, `BeforePublish`, and `AfterPublish` are empty SDK targets the implicit `Sdk.targets` import defines after the project body, a same-named target in a project file loses silently.

```xml
<!-- BAD -->
<Target Name="AfterBuild">
  <Message Importance="high" Text="built" />
</Target>

<!-- GOOD -->
<Target Name="ReportBuild" AfterTargets="Build">
  <Message Importance="high" Text="built $(TargetPath)" />
</Target>

<!-- GOOD: Directory.Build.targets -->
<PropertyGroup>
  <CompileDependsOn>$(CompileDependsOn);ReportCompile</CompileDependsOn>
</PropertyGroup>
```

- `BeforeTargets="CoreCompile"` reaches design-time builds, a target attached to `Build` does not, a generated source target names `CoreCompile`
- Every hook runs in the project instance running the SDK target, an item added there is visible to every later target

## [03]-[INCREMENTAL_TARGETS]

MSBuild skips a target with every output at least as new as its inputs, a target without `Outputs` runs on every build. Every target that writes a file takes both attributes, a target that registers or verifies takes a marker file.

- `Inputs` without `Outputs` fails `MSB4058`, an `Inputs` or `Outputs` expression evaluating to empty skips the target
- Transforms in `Outputs` map each output to one input, the target then runs with the stale inputs alone
- Fixed `Outputs` values compare against every input and run the whole target
- MSBuild compares timestamps alone and reads the input list of the current run, a file that left the list does not make the target stale
- `$(MSBuildAllProjects)` in `Inputs` names the project and every imported file, a change to any of them reruns the target
- Skipped targets still apply their `ItemGroup` and `PropertyGroup` children
- Skipped targets infer a task `<Output>` with a `TaskParameter` that is a task input too, an output the task computes (`CopiedFiles`) stays unset
- Every written file under `$(OutDir)` or `$(IntermediateOutputPath)` joins `@(FileWrites)`
- `IncrementalClean` deletes a file a prior build wrote and the current build did not, `Clean` deletes every recorded file
- `@(FileWritesShareable)` records a copy-local file another project can write, `IncrementalClean` keeps it outside the project directory
- `_CleanRecordFileWrites` runs inside `CoreBuild`, a target after `Build` records nothing and `Clean` leaves its file
- `@(IntermediateAssembly)` names the compiled assembly under `$(IntermediateOutputPath)`

```xml
<Target Name="RegisterTool" AfterTargets="Build" Inputs="$(MSBuildAllProjects)" Outputs="$(IntermediateOutputPath)register-tool.marker">
  <Touch Files="$(IntermediateOutputPath)register-tool.marker" AlwaysCreate="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)register-tool.marker" />
  </ItemGroup>
</Target>
```

## [04]-[GENERATED_FILES]

Globs outside a target expand during evaluation and cannot see a file a target writes later, the target that writes the file adds it to the item type consuming it. Generated files go under `$(IntermediateOutputPath)`, read inside the target, the SDK sets it after the project body. Input files read through `ReadLinesFromFile`, a property function argument does not expand `@()`.

```xml
<Target Name="GenerateBuildInfoSource" BeforeTargets="CoreCompile"
        Inputs="$(MSBuildAllProjects);@(BuildInfoInput)" Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <ReadLinesFromFile File="@(BuildInfoInput)">
    <Output TaskParameter="Lines" PropertyName="_BuildLabel" />
  </ReadLinesFromFile>
  <WriteLinesToFile File="$(IntermediateOutputPath)BuildInfo.g.cs" Overwrite="true" WriteOnlyWhenDifferent="true"
                    Lines="// &lt;auto-generated /&gt;%0Ainternal static class BuildInfo { public const string Label = &quot;$(_BuildLabel)&quot;%3B }" />
  <ItemGroup>
    <Compile Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>

<Target Name="GenerateManifestData" BeforeTargets="AssignTargetPaths"
        Inputs="$(MSBuildAllProjects);@(BuildInfoInput)" Outputs="$(IntermediateOutputPath)manifest.json">
  <WriteLinesToFile File="$(IntermediateOutputPath)manifest.json" Lines="{}" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <None Include="$(IntermediateOutputPath)manifest.json" TargetPath="data/manifest.json" CopyToOutputDirectory="PreserveNewest" />
    <FileWrites Include="$(IntermediateOutputPath)manifest.json" />
  </ItemGroup>
</Target>
```

- `AssignTargetPaths` is the last target giving `None` and `Content` a `TargetPath`
- `WriteOnlyWhenDifferent="true"` keeps the timestamp when the content is unchanged, `CoreCompile` stays up to date after a rerun of the generator
- `%0A` and `%3B` escape a newline and a semicolon inside `Lines`, `Lines` is an item list and a plain `;` splits it
- `EmitCompilerGeneratedFiles=true` writes generator output under `CompilerGeneratedFilesOutputPath`, default `$(IntermediateOutputPath)/generated`

## [05]-[TASKS]

Tasks inside a target run at execution time with the current properties and items, `ItemGroup` and `PropertyGroup` children are tasks too. A property function computes a value evaluation can produce, a built-in task does file work, `Exec` runs an external program.

- `Exec` runs `sh` on macOS and Linux and `cmd.exe` on Windows, a command with shell syntax gets an `IsOSPlatform` condition on the target
- `Exec` never skips by itself, the target holding it takes `Inputs` and `Outputs`
- `Exec` fails on a non-zero exit code and on a line in the standard error and warning format
- `IgnoreExitCode="true"` ignores the exit code, `IgnoreStandardErrorWarningFormat="true"` ignores format lines, the target reads `ExitCode`
- `ConsoleToMSBuild="true"` fills `ConsoleOutput` with every output line, `EchoOff="true"` keeps the expanded command out of the log
- `StandardOutputImportance="low"` hides tool output at normal verbosity, `EnvironmentVariables` and `WorkingDirectory` replace a shell prefix
- `<Output>` fills a property or an item list, never item metadata
- `Copy` with `SkipUnchangedFiles="true"` compares size and timestamp, `UseHardlinksIfPossible` links in place of copying, `Retries` hides a file race
- Boolean parameters read `true`, `on`, `yes`, `!false`, `!off`, and `!no` as true
- Task parameters are attributes, a child element (`<Delete><Files>stage/*.tmp</Files></Delete>`) fails `MSB4067`
- `Warning` and `Error` take `Code` and `File`, `Message` takes `Importance`
- Custom diagnostics take a `Code` that `MSBuildWarningsAsMessages`, `NoWarn`, and `-warnaserror` can name
- `CallTarget` runs the named target in a new scope, `TargetOutputs` alone comes back and a property or item it sets stays invisible to the caller
- `DependsOnTargets` runs the named target in the caller's scope
- `MSBuild` task calls create a new project instance for every new global property set
- `Properties` on a call to a project the build already builds creates a second build of it
- `RemoveProperties` strips a global property the callee never reads, on a self-call it changes the instance key as `Properties` does
- `SkipNonexistentProjects="true"` skips a missing project file, `SkipNonexistentTargets="true"` skips a project lacking the target
- `BuildInParallel` follows `$(BuildInParallel)`, `StopOnFirstFailure` applies to a serial call alone, `BuildInParallel` ignores it
- Inline tasks with `RoslynCodeTaskFactory` replace a property function for a loop over files or a .NET API outside the property function allowlist

```xml
<Exec Command="dotnet --version" ConsoleToMSBuild="true" IgnoreExitCode="true" EchoOff="true"
      WorkingDirectory="$(MSBuildProjectDirectory)" EnvironmentVariables="DOTNET_NOLOGO=1;DOTNET_CLI_TELEMETRY_OPTOUT=1"
      StandardOutputImportance="low">
  <Output TaskParameter="ConsoleOutput" PropertyName="ToolVersion" />
  <Output TaskParameter="ExitCode" PropertyName="ToolExitCode" />
</Exec>
<Error Text="dotnet --version exited with $(ToolExitCode)" Condition="'$(ToolExitCode)' != '0'" />

<MSBuild Projects="@(ProjectReference)" Targets="GetSchemaFiles" BuildInParallel="$(BuildInParallel)"
         RemoveProperties="RuntimeIdentifier" SkipNonexistentTargets="true">
  <Output TaskParameter="TargetOutputs" ItemName="_CollectedSchema" />
</MSBuild>
```

- `TargetOutputs` holds the returns of the named targets, never of their dependencies
- Each `TargetOutputs` item has `MSBuildSourceProjectFile` and `MSBuildSourceTargetName` metadata

## [06]-[BATCHING_IN_TARGETS]

`%(Metadata)` in a task attribute runs the task once per distinct metadata value with the matching items, `%(Metadata)` in the target `Outputs` runs the whole target once per value. `%()` in a target `Condition` fails `MSB4116`.

- Task batching leaves the target running once
- `PropertyGroup` lines that batch finish every batch before the next line reads the property, the property holds the last batch value
- Target batching gives each batch its own copy of the properties and items, a batched `ItemGroup` runs once per target batch
- Two item types in one expression batch separately, each batch sees the other type empty, batch on one type and pass the other as a property
- `;`-delimited properties with leading and trailing separators test membership through `Contains(';%(Item.Meta);')`, batching over the item

```xml
<PropertyGroup>
  <AllowedGroups>;images;data;</AllowedGroups>
</PropertyGroup>
<ItemGroup>
  <AllowedAsset Include="@(Asset)" Condition="$(AllowedGroups.Contains(';%(Asset.Group);'))" />
</ItemGroup>
<Error Text="Asset '%(Asset.Identity)' group '%(Asset.Group)' is not allowed" Condition="!$(AllowedGroups.Contains(';%(Asset.Group);'))" />
```

## [07]-[ERRORS_AND_RESULTS]

Failed tasks stop their target and the build unless `ContinueOnError` says otherwise, `OnError` elements run their targets after the failure, warning properties and switches decide which warnings fail the build.

| [INDEX] | [CONTINUEONERROR]  | [EFFECT]                                              |
| :-----: | :----------------- | :---------------------------------------------------- |
|  [01]   | `ErrorAndStop`     | Default, error stops target and build, `OnError` runs |
|  [02]   | `ErrorAndContinue` | Error logged, next task runs, build fails at end      |
|  [03]   | `WarnAndContinue`  | Errors become warnings, next task runs                |

- `OnError ExecuteTargets` runs when a task of the target fails with `ErrorAndStop`, and when a target in its `DependsOnTargets` chain fails
- `OnError` elements come last in the target or the build fails `MSB4038`, each has its own `Condition`, they run in order
- `-warnaserror` promotes every warning to an error, the target keeps running as for a warning
- `-warnaserror:CODE` promotes a list, `-warnnotaserror:CODE` exempts a list under `-warnaserror`, `-warnasmessage:CODE` demotes a list
- `MSBuildTreatWarningsAsErrors`, `MSBuildWarningsAsErrors`, `MSBuildWarningsNotAsErrors`, and `MSBuildWarningsAsMessages` are the property forms
- `WarningsAsErrors`, `WarningsNotAsErrors`, and `NoWarn` feed the last three per project
- `MSBuildWarningsNotAsErrors` exempts a code from `MSBuildTreatWarningsAsErrors` and from `-warnaserror` in that project

```xml
<Target Name="Stage" DependsOnTargets="Prepare">
  <Exec Command="tool --optional-step" ContinueOnError="WarnAndContinue" />
  <Error Code="TOOL0001" File="tool.config" Text="tool.config lacks a stage entry" Condition="'$(StageEntry)' == ''" />
  <OnError ExecuteTargets="Cleanup" Condition="'$(KeepStageOutput)' != 'true'" />
  <OnError ExecuteTargets="Report" />
</Target>
```

Command line switches prove what a target returns and control the whole build:

| [INDEX] | [SWITCH]                               | [EFFECT]                                                                   |
| :-----: | :------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `-target:Name` or `-t:Name`            | Runs named targets in place of `DefaultTargets`                            |
|  [02]   | `-getTargetResult:Name`                | Runs target, prints `Result` and returned `Items` as JSON                  |
|  [03]   | `-t:Name -getProperty:` or `-getItem:` | Prints value after target ran                                              |
|  [04]   | `-tl:on`                               | Terminal logger, `-v:n` shows target and task messages on console logger   |
|  [05]   | `-isolate -outputResultsCache:file`    | Serializes built target results, `-inputResultsCaches:file` reuses them    |

- Graph builds predict the targets of every reference through `ProjectReferenceTargets`, a custom target called on references joins through an item
- `Build` maps to `GetTargetFrameworks`, the default target, `GetNativeManifest`, and `GetCopyToOutputDirectoryItems`
- Results caches hold built target results, the producing build names every protocol target in `-target:`
- Consumers under `-isolate -inputResultsCaches` build without evaluating the reference
- Use `dotnet-msbuild-diagnostics` for the static graph and isolation switches

```xml
<ItemGroup>
  <ProjectReferenceTargets Include="Build" Targets="GetSchemaFiles" />
</ItemGroup>
```

## [08]-[MULTI_TARGETING_AND_PUBLISH]

Multi-targeting projects build once as the outer build, `DispatchToInnerBuilds` runs one inner build per `TargetFrameworks` entry through the `MSBuild` task with `TargetFramework` as a global property, a target attached to `Build` runs in the outer build and in every inner build. `dotnet publish` passes `_IsPublishing=true` as a global property, `dotnet build` and `dotnet pack` do not, the SDK sets it when it packs a tool.

| [INDEX] | [SCOPE]                 | [CONDITION]                            | [NOTE]                                                          |
| :-----: | :---------------------- | :------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Inner build alone       | `'$(TargetFramework)' != ''`           | Per-assembly work, `OutDir` and `IntermediateOutputPath` set    |
|  [02]   | Outer build alone       | `'$(IsCrossTargetingBuild)' == 'true'` | Per-project work, no compile output, `TargetFrameworks` set     |
|  [03]   | Publish alone           | `'$(_IsPublishing)' == 'true'`         | SDK sets it and changes the output path                         |
|  [04]   | Not a design-time build | `'$(DesignTimeBuild)' != 'true'`       | IDE loads, hooks that write files or fail the build test it     |
|  [05]   | `CoreBuild` alone       | `'$(BuildingProject)' == 'true'`       | `BuildOnlySettings` sets it, false in `GetTargetPath` calls     |

- `IsPublishable=false` turns `Publish` into a no-op
- `ComputeFilesToPublish` fills `@(ResolvedFileToPublish)` with `RelativePath` and `CopyToPublishDirectory` metadata
- `CopyFilesToPublishDirectory` copies each item to `$(PublishDir)%(RelativePath)`, `PublishItemsOutputGroup` returns the list to a caller
- `dotnet publish` on a multi-targeting project fails `NETSDK1129` without `-f`, a publish hook runs in an inner build
- `RuntimeIdentifier` appends `_<rid>` to the pivot under the artifacts layout and adds a `<rid>/` directory under the default layout
- `AppendRuntimeIdentifierToOutputPath=false` removes the default-layout directory alone
- Publish copies each `ResolvedFileToPublish` item from its source, never from `$(OutDir)`, a target editing `$(OutDir)` files runs again after `Publish`

```xml
<Target Name="AddNoticesToPublish" AfterTargets="ComputeFilesToPublish">
  <WriteLinesToFile File="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" Lines="@(Notice)" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <ResolvedFileToPublish Include="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" RelativePath="legal/THIRD-PARTY-NOTICES.txt" CopyToPublishDirectory="PreserveNewest" />
    <FileWrites Include="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" />
  </ItemGroup>
</Target>
```

## [09]-[COPY_TO_OUTPUT]

`CopyToOutputDirectory` on a `None`, `Content`, `Compile`, or `EmbeddedResource` item copies it under `$(OutDir)`, `CopyToPublishDirectory` controls the publish copy with the same values. `Copy` tasks in a custom target do neither.

| [INDEX] | [VALUE]          | [BUILD_TARGET]                                     | [BEHAVIOR]                                                     |
| :-----: | :--------------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PreserveNewest` | `_CopyOutOfDateSourceItemsToOutputDirectory`       | Copies when destination is missing or older                    |
|  [02]   | `IfDifferent`    | `_CopyDifferingSourceItemsToOutputDirectory`       | Copies when size or timestamp differs, restores an edited file |
|  [03]   | `Always`         | `_CopyOutOfDateSourceItemsToOutputDirectoryAlways` | Copies on every build                                          |
|  [04]   | `Never` or unset | None                                               | Copies nothing                                                 |

- `AssignTargetPaths` computes `TargetPath` from `TargetPath`, then `Link`, then the path relative to the project directory
- SDK sets `Link` to `%(LinkBase)%(RecursiveDir)%(Filename)%(Extension)` for a file outside the project directory
- `None` and `Content` copy the same way, `Content` joins `ContentFilesProjectOutputGroup` too
- `DefaultCopyToPublishDirectoryMetadata` copies `CopyToOutputDirectory` into an unset `CopyToPublishDirectory` after `AssignTargetPaths`
- `SkipUnchangedFilesOnCopyAlways=true` applies the `IfDifferent` test to every `Always` item
- `GetCopyToOutputDirectoryItems` returns the project's items and the items of its `ProjectReference` projects transitively
- `MSBuildCopyContentTransitively=false` limits the walk to one level
- Referencing projects copy reference items under their own `$(OutDir)%(TargetPath)`, `Private="false"` on the reference stops the copy
- `ResolvePackageAssets` emits `@(RuntimeCopyLocalItems)` for managed assets and `@(NativeCopyLocalItems)` for `runtimes/<rid>/native/` assets
- `@(RuntimeTargetsCopyLocalItems)` holds the RID-specific assets of a RID-less build
- Each copy-local item has `NuGetPackageId`, `AssetType`, `CopyLocal`, and `DestinationSubDirectory` metadata
- Items with `CopyLocal=true` join `@(ReferenceCopyLocalPaths)` when `CopyLocalLockFileAssemblies` is `true`
- `CopyLocalLockFileAssemblies` defaults to `true` with runtime output or `EnableDynamicLoading`
- `_CopyFilesMarkedCopyLocal` writes `@(ReferenceCopyLocalPaths)` to `$(OutDir)%(DestinationSubDirectory)%(Filename)%(Extension)`
- Publish copies the same assets whatever `CopyLocalLockFileAssemblies` is

```xml
<ItemGroup>
  <None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
  <None Update="testdata/seed.db" CopyToOutputDirectory="IfDifferent" />
  <Content Include="../shared/assets/**" LinkBase="content/" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="Never" />
</ItemGroup>

<Target Name="AddNativeAsset" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <NativeCopyLocalItems Include="$(MSBuildProjectDirectory)/native/libtool.dylib"
                          NuGetPackageId="Tool.Native" NuGetPackageVersion="1.0.0"
                          AssetType="native" CopyLocal="true" DestinationSubDirectory="runtimes/osx-arm64/native/" />
  </ItemGroup>
</Target>
```
