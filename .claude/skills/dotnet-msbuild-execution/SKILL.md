---
name: dotnet-msbuild-execution
description: "Use when writing an MSBuild target, covering SDK hook points, incremental Inputs and Outputs, generated files, tasks, batching, errors, multi-targeting and publish scope, and copy to output."
---

# [DOTNET_MSBUILD_EXECUTION]

Covers the execution phase, from target order to the copy-to-output rules.

- Use `dotnet-msbuild-evaluation` for properties, items, conditions, transforms, and batching outside targets, and the placement of each declaration
- Use `dotnet-msbuild-antipatterns` for the review catalog of smells with `BAD` and `GOOD` pairs
- Use `dotnet-msbuild-diagnostics` for binlog capture and queries, failure triage, performance, double builds, and BuildCheck
- Use `dotnet-msbuild-packaging` for `Pack`, `GenerateNuspec` content, central package management, solution files, and CI properties
- Use `monorepo-build-infrastructure` for the `eng/` directory, task runner targets, native packaging projects, and provisioning

[REFERENCES]:
- [01]-[HOOK_POINTS](references/hook-points.md): Every SDK hook point with its phase and the items and properties present there
- [02]-[TASK_PARAMETERS](references/task-parameters.md): Task parameters that decide correctness and incremental behavior, with the inline task form

## [01]-[TARGET_ORDERING]

MSBuild runs each target at most once per project instance, in an order that the target attributes decide and never the declaration order. For one requested target the engine runs, in order, its `DependsOnTargets` from left to right, the targets in `BeforeTargets` that name it, the target body, and the targets in `AfterTargets` that name it. `InitialTargets` run before every requested target, `-target:` replaces `DefaultTargets`, and the last definition of a target name wins.

| [INDEX] | [ATTRIBUTE]        | [OWNER]              | [USE_WHEN]                                                                 |
| :-----: | :----------------- | :------------------- | :------------------------------------------------------------------------- |
|  [01]   | `DependsOnTargets` | The consuming target | The named target is your own and must finish before the consumer reads it  |
|  [02]   | `BeforeTargets`    | The inserted target  | The named target is another file's and the inserted one must run before it |
|  [03]   | `AfterTargets`     | The inserted target  | The named target is another file's and produces what the inserted consumes |

- Combine them on one target: `DependsOnTargets` for your own chain and one of `BeforeTargets` or `AfterTargets` for the SDK target
- MSBuild evaluates a target `Condition` when the target is about to run, after every earlier target updated the properties and items it reads
- Targets with a false `Condition` skip the body and the `DependsOnTargets` chain, and the `BeforeTargets` and `AfterTargets` that name the skipped target still run
- Skipped targets count as not run, a later `DependsOnTargets` request runs one when its condition is true at that time, and its `BeforeTargets` and `AfterTargets` do not run again
- MSBuild compares `Inputs` and `Outputs` after the dependencies and `BeforeTargets` ran and before the body, and the check never changes the order
- `BeforeTargets` and `AfterTargets` accept a name no target defines, the target then never runs, and detailed verbosity logs the unmatched name
- `Returns` names the items a caller receives, `Outputs` names the files of the up-to-date check, and once one target in the project declares `Returns` a target with only `Outputs` returns nothing, which holds in every SDK project because `Build` declares `Returns`
- MSBuild records the returned items when the target completes, drops duplicate items unless `KeepDuplicateOutputs="true"`, and a target that appends to the item list through `AfterTargets` changes nothing the caller receives
- `Label` is an identifier for tooling and never changes execution

```xml
<!-- Query target, the MSBuild task reads its items through TargetOutputs -->
<Target Name="GetStageOutputs" DependsOnTargets="CollectStageOutputs" Returns="@(StageOutput)" />
```

```bash
dotnet msbuild Library.csproj -getTargetResult:GetStageOutputs
dotnet msbuild Library.csproj -getTargetResult:GetStageOutputs -getItem:StageOutput
dotnet msbuild Library.csproj -targets                                                # lists every target the project defines
```

## [02]-[SDK_HOOK_POINTS]

Extend the SDK chain from a target that names an SDK target in `BeforeTargets` or `AfterTargets`, or from a `DependsOn` property that a `.targets` file appends to after the SDK assigned it. `BeforeBuild`, `AfterBuild`, `BeforeCompile`, `AfterCompile`, `BeforeResolveReferences`, `AfterResolveReferences`, `BeforeClean`, `AfterClean`, `BeforePublish`, and `AfterPublish` are empty SDK targets that the implicit `Sdk.targets` import defines after the project body, attach to them with `AfterTargets`, and a same-named target in a project file loses without a message.

```xml
<!-- BAD: the SDK import redefines AfterBuild after the project body, the target never runs -->
<Target Name="AfterBuild">
  <Message Importance="high" Text="built" />
</Target>

<!-- GOOD: the target owns its name and attaches to the SDK target -->
<Target Name="ReportBuild" AfterTargets="Build">
  <Message Importance="high" Text="built $(TargetPath)" />
</Target>

<!-- GOOD: Directory.Build.targets, the SDK assigned CompileDependsOn before the import and the append keeps the chain -->
<PropertyGroup>
  <CompileDependsOn>$(CompileDependsOn);ReportCompile</CompileDependsOn>
</PropertyGroup>
```

- `BeforeTargets="CoreCompile"` reaches design-time builds and `BeforeTargets="BeforeCompile"` does not, and a generated source target names `CoreCompile`
- Every hook runs in the project instance that runs the SDK target, and an item added there is visible to every later target without an `<MSBuild>` call
- Use `references/hook-points.md` for the full hook table per phase

## [03]-[INCREMENTAL_TARGETS]

MSBuild skips a target with every output at least as new as its inputs, and a target without `Outputs` runs on every build. Give every target that writes a file both attributes, and give a target that only registers or verifies something a marker file.

- `Inputs` without `Outputs` fails with `MSB4058`, and an `Inputs` or `Outputs` expression that evaluates to empty skips the target
- Transforms in `Outputs` map each output to one input, MSBuild then runs the target with only the stale inputs, and a fixed `Outputs` value compares against every input and runs the whole target
- MSBuild compares timestamps only, reads only the input list of the current run, and a file that left the list does not make the target stale
- Put `$(MSBuildAllProjects)` in `Inputs`, it names the project and every imported file, and a change in `Directory.Build.targets` or the `.targets` file that holds the generator then reruns it
- Skipped targets still apply their `ItemGroup` and `PropertyGroup` children and infer a task `<Output>` with a `TaskParameter` that is also a task input, and an output the task computes (`CopiedFiles`) stays unset
- Add every written file to `@(FileWrites)` under `$(OutDir)` or `$(IntermediateOutputPath)`, `IncrementalClean` deletes a file that a prior build wrote and the current build did not, and `Clean` deletes every recorded file
- `@(FileWritesShareable)` records a copy that another project can also write (a copy-local reference), and `IncrementalClean` keeps it when the file is outside the project directory
- `_CleanRecordFileWrites` runs inside `CoreBuild`, a target after `Build` records nothing, and `Clean` leaves its file
- `@(IntermediateAssembly)` names the compiled assembly under `$(IntermediateOutputPath)` when a target needs the compile output as an input

```xml
<!-- The marker under IntermediateOutputPath makes a target without a real output incremental -->
<Target Name="RegisterTool" AfterTargets="Build" Inputs="$(MSBuildAllProjects)" Outputs="$(IntermediateOutputPath)register-tool.marker">
  <Touch Files="$(IntermediateOutputPath)register-tool.marker" AlwaysCreate="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)register-tool.marker" />
  </ItemGroup>
</Target>
```

## [04]-[GENERATED_FILES]

Globs outside a target expand during evaluation and cannot see a file that a target writes later, and the target that writes the file adds it to the item type that consumes it. Write generated files under `$(IntermediateOutputPath)`, read that property inside the target because the SDK sets it after the project body, and read an input file through `ReadLinesFromFile` because a property function argument does not expand `@()`.

```xml
<!-- Generated source: CoreCompile sees the Compile item in every build and in design-time builds -->
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

<!-- Generated data file: AssignTargetPaths is the last target that gives None and Content a TargetPath -->
<Target Name="GenerateManifestData" BeforeTargets="AssignTargetPaths"
        Inputs="$(MSBuildAllProjects);@(BuildInfoInput)" Outputs="$(IntermediateOutputPath)manifest.json">
  <WriteLinesToFile File="$(IntermediateOutputPath)manifest.json" Lines="{}" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <None Include="$(IntermediateOutputPath)manifest.json" TargetPath="data/manifest.json" CopyToOutputDirectory="PreserveNewest" />
    <FileWrites Include="$(IntermediateOutputPath)manifest.json" />
  </ItemGroup>
</Target>
```

- `WriteOnlyWhenDifferent="true"` keeps the timestamp when the content is unchanged, and `CoreCompile` stays up to date after a rerun of the generator
- `%0A` and `%3B` escape a newline and a semicolon inside `Lines`, because `Lines` is an item list and a plain `;` splits it
- Roslyn source generator output belongs to `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath`, which defaults to `$(IntermediateOutputPath)generated/`

## [05]-[TASKS]

Tasks inside a target run at execution time with the current properties and items, and `ItemGroup` and `PropertyGroup` children are tasks too. Prefer a property function for a value that evaluation can compute, a built-in task for file work, and `Exec` only for an external program.

- `Exec` runs `sh` on macOS and Linux and `cmd.exe` on Windows, a command with shell syntax gets an `IsOSPlatform` condition on the target, `Exec` never skips by itself, and the target that holds it gets `Inputs` and `Outputs`
- `Exec` fails on a non-zero exit code and on a line in the standard error and warning format, `IgnoreExitCode="true"` keeps the first, `IgnoreStandardErrorWarningFormat="true"` keeps the second, and the target reads `ExitCode` to decide
- `ConsoleToMSBuild="true"` fills `ConsoleOutput` with every output line, `EchoOff="true"` keeps the expanded command out of the log, `StandardOutputImportance="low"` hides tool output at normal verbosity, and `EnvironmentVariables` and `WorkingDirectory` replace a shell prefix
- `Copy` with `SkipUnchangedFiles="true"` compares size and timestamp, `UseHardlinksIfPossible` links instead of copying, and `Retries` hides a file race
- `Warning` and `Error` take `Code` and `File`, a repository diagnostic gets a code that `MSBuildWarningsAsMessages`, `NoWarn`, and `-warnaserror` can name, and `Message` takes `Importance`
- `CallTarget` runs the named target in a new scope, a property or item it sets is invisible to the caller, only `TargetOutputs` comes back, and `DependsOnTargets` runs it in the same scope
- The `MSBuild` task creates a new project instance for every new global property set, `Properties` on a call to a project the build already builds creates a second build of it, and `RemoveProperties` strips a global property the callee never reads
- `SkipNonexistentProjects="true"` skips a missing project file, `SkipNonexistentTargets="true"` skips a project that lacks the target, `BuildInParallel` follows `$(BuildInParallel)`, and `StopOnFirstFailure` applies only to a serial call
- Inline tasks with `RoslynCodeTaskFactory` replace a property function only when the computation loops over files, reads many files, or calls a .NET API outside the property function allowlist

```xml
<!-- Exec that captures a value, the exit code decides and the tool output stays out of the normal log -->
<Exec Command="dotnet --version" ConsoleToMSBuild="true" IgnoreExitCode="true" EchoOff="true"
      WorkingDirectory="$(MSBuildProjectDirectory)" EnvironmentVariables="DOTNET_NOLOGO=1;DOTNET_CLI_TELEMETRY_OPTOUT=1"
      StandardOutputImportance="low">
  <Output TaskParameter="ConsoleOutput" PropertyName="ToolVersion" />
  <Output TaskParameter="ExitCode" PropertyName="ToolExitCode" />
</Exec>
<Error Text="dotnet --version exited with $(ToolExitCode)" Condition="'$(ToolExitCode)' != '0'" />

<!-- The MSBuild task queries a referenced project, the referenced project owns the Returns -->
<MSBuild Projects="@(ProjectReference)" Targets="GetSchemaFiles" BuildInParallel="$(BuildInParallel)"
         RemoveProperties="RuntimeIdentifier" SkipNonexistentTargets="true">
  <Output TaskParameter="TargetOutputs" ItemName="_CollectedSchema" />
</MSBuild>
```

- `TargetOutputs` holds only the returns of the named targets, never of their dependencies, and each item has `MSBuildSourceProjectFile` and `MSBuildSourceTargetName` metadata
- Use `references/task-parameters.md` for the parameters per task and the inline task form

## [06]-[BATCHING_IN_TARGETS]

`%(Metadata)` in a task attribute runs the task once per distinct metadata value with the matching items, and `%(Metadata)` in the target `Outputs` runs the whole target once per value. `%()` references in a target `Condition` fail with `MSB4116`.

- Task batching leaves the target running once, a `PropertyGroup` line that batches finishes every batch before the next line reads the property, and the property holds the last batch value
- Target batching gives each batch its own copy of the properties and items, and a batched `ItemGroup` runs once per target batch
- Two item types in one expression batch separately, each batch sees the other type empty, batch on one type, and pass the other as a property
- The `;`-delimited property with a leading and trailing separator tests set membership through `Contains(';%(Item.Meta);')`, which batches over the item and reads the whole list

```xml
<!-- One Include per item that passes the membership test, one Error per item that fails it -->
<PropertyGroup>
  <AllowedGroups>;images;data;</AllowedGroups>
</PropertyGroup>
<ItemGroup>
  <AllowedAsset Include="@(Asset)" Condition="$(AllowedGroups.Contains(';%(Asset.Group);'))" />
</ItemGroup>
<Error Text="Asset '%(Asset.Identity)' group '%(Asset.Group)' is not allowed" Condition="!$(AllowedGroups.Contains(';%(Asset.Group);'))" />
```

## [07]-[ERRORS_AND_RESULTS]

Failed tasks stop their target and the build unless `ContinueOnError` says otherwise, `OnError` elements run their targets after the failure, and the warning properties and switches decide which warnings fail the build.

| [INDEX] | [CONTINUEONERROR]  | [EFFECT]                                                            |
| :-----: | :----------------- | :------------------------------------------------------------------ |
|  [01]   | `ErrorAndStop`     | Default, the error stops the target and the build, `OnError` runs   |
|  [02]   | `ErrorAndContinue` | The error is logged, the next task runs, the build fails at the end |
|  [03]   | `WarnAndContinue`  | The errors become warnings and the next task runs                   |

- `OnError ExecuteTargets` runs when a task of the target fails with `ErrorAndStop`, and when a target in its `DependsOnTargets` chain fails
- `OnError` elements come last in the target or the build fails with `MSB4038`, each has its own `Condition`, and they run in order
- `-warnaserror` promotes every warning to an error and the target keeps running as if it were a warning, `-warnaserror:CODE` promotes a list, `-warnnotaserror:CODE` exempts a list under `-warnaserror`, and `-warnasmessage:CODE` demotes a list
- `MSBuildTreatWarningsAsErrors`, `MSBuildWarningsAsErrors`, `MSBuildWarningsNotAsErrors`, and `MSBuildWarningsAsMessages` are the same controls as project properties, `WarningsAsErrors`, `WarningsNotAsErrors`, and `NoWarn` feed the last three, and every one is evaluated per project
- `MSBuildWarningsNotAsErrors` exempts a code from `MSBuildTreatWarningsAsErrors` and from `-warnaserror` in that project

```xml
<Target Name="Stage" DependsOnTargets="Prepare">
  <Exec Command="tool --optional-step" ContinueOnError="WarnAndContinue" />
  <Error Code="TOOL0001" File="tool.config" Text="tool.config lacks a stage entry" Condition="'$(StageEntry)' == ''" />
  <OnError ExecuteTargets="Cleanup" Condition="'$(KeepStageOutput)' != 'true'" />
  <OnError ExecuteTargets="Report" />
</Target>
```

The command line proves what a target returns and controls the whole build:

| [INDEX] | [SWITCH]                                   | [EFFECT]                                                                     |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `-target:Name` or `-t:Name`                | Runs the named targets in place of `DefaultTargets`                          |
|  [02]   | `-getTargetResult:Name`                    | Runs the target and prints `Result` and the returned `Items` as JSON         |
|  [03]   | `-t:Name -getProperty:` or `-getItem:`     | Prints the value after the target ran instead of after evaluation            |
|  [04]   | `-restore` with `-restoreProperty:N=V`     | Runs `Restore` first, the property applies to restore and never to the build |
|  [05]   | `-tl:on`                                   | Terminal logger, `-tl:off -v:n` shows target and task messages               |
|  [06]   | `-graph`                                   | Builds a static graph, references build before the projects that use them    |
|  [07]   | `-graph -isolate`                          | Fails with `MSB4252` on an `MSBuild` call the graph did not predict          |
|  [08]   | `-isolate -outputResultsCache:file`        | Serializes the built target results, `-inputResultsCaches:file` reuses them  |

- Graph builds predict the targets of every reference through `ProjectReferenceTargets`, `Build` maps to `GetTargetFrameworks`, the default target, `GetNativeManifest`, and `GetCopyToOutputDirectoryItems`, and a custom target that a project calls on its references joins the protocol through an item
- The results cache holds the results of the targets that were built, the producing build names every protocol target in `-target:`, and the consumer under `-isolate` then builds without evaluating the reference

```xml
<!-- Build on the project also calls GetSchemaFiles on every ProjectReference, a graph build schedules it and an isolated build finds it in the cache -->
<ItemGroup>
  <ProjectReferenceTargets Include="Build" Targets="GetSchemaFiles" />
</ItemGroup>
```

- Use `dotnet-msbuild-diagnostics` for the binlog queries that show why a target ran, was skipped, or built twice

## [08]-[MULTI_TARGETING_AND_PUBLISH]

Multi-targeting projects build once as the outer build, `DispatchToInnerBuilds` runs one inner build per `TargetFrameworks` entry through the `MSBuild` task with `TargetFramework` as a global property, and a target attached to `Build` runs in the outer build and in every inner build. `dotnet publish` passes `_IsPublishing=true` as a global property, `dotnet build` and `dotnet pack` do not, and the SDK sets it when it packs a tool.

| [INDEX] | [SCOPE]                 | [CONDITION]                            | [NOTE]                                                       |
| :-----: | :---------------------- | :------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Inner build only        | `'$(TargetFramework)' != ''`           | Per-assembly work, `OutDir` and `IntermediateOutputPath` set |
|  [02]   | Outer build only        | `'$(IsCrossTargetingBuild)' == 'true'` | Per-project work, no compile output, `TargetFrameworks` set  |
|  [03]   | Publish only            | `'$(_IsPublishing)' == 'true'`         | The SDK sets it and changes the output path on it            |
|  [04]   | Not a design-time build | `'$(DesignTimeBuild)' != 'true'`       | IDE loads, hooks that write files or fail the build test it  |
|  [05]   | `CoreBuild` only        | `'$(BuildingProject)' == 'true'`       | `BuildOnlySettings` sets it, false in `GetTargetPath` calls  |

- `IsPublishable=false` turns `Publish` into a no-op
- `ComputeFilesToPublish` fills `@(ResolvedFileToPublish)` with `RelativePath` and `CopyToPublishDirectory` metadata, `CopyFilesToPublishDirectory` copies each item to `$(PublishDir)%(RelativePath)`, and `PublishItemsOutputGroup` returns the list to a caller
- `dotnet publish` on a multi-targeting project fails with `NETSDK1129` without `-f`, and a publish hook runs in an inner build only
- `RuntimeIdentifier` appends `_<rid>` to the pivot under the artifacts layout and adds a `<rid>/` directory under the default layout, and `AppendRuntimeIdentifierToOutputPath=false` removes only the default-layout directory
- Targets that edit a file in `$(OutDir)` after `CopyFilesToOutputDirectory` run again after `Publish`, because publish copies each `ResolvedFileToPublish` item from its source and never from `$(OutDir)`

```xml
<!-- Generated file that publish copies, RelativePath places it under PublishDir -->
<Target Name="AddNoticesToPublish" AfterTargets="ComputeFilesToPublish">
  <WriteLinesToFile File="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" Lines="@(Notice)" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <ResolvedFileToPublish Include="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" RelativePath="legal/THIRD-PARTY-NOTICES.txt" CopyToPublishDirectory="PreserveNewest" />
    <FileWrites Include="$(IntermediateOutputPath)THIRD-PARTY-NOTICES.txt" />
  </ItemGroup>
</Target>
```

- Use `dotnet-msbuild-antipatterns` for the second build that `Properties="_IsPublishing=true"` on an `MSBuild` call creates and its `DependsOnTargets="Publish"` fix

## [09]-[COPY_TO_OUTPUT]

`CopyToOutputDirectory` on a `None`, `Content`, `Compile`, or `EmbeddedResource` item copies it under `$(OutDir)`, and `CopyToPublishDirectory` controls the publish copy with the same values. `Copy` tasks in a custom target do neither.

| [INDEX] | [VALUE]          | [BUILD TARGET]                                     | [BEHAVIOR]                                                     |
| :-----: | :--------------- | :------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `PreserveNewest` | `_CopyOutOfDateSourceItemsToOutputDirectory`       | Copies when the destination is missing or older                |
|  [02]   | `IfDifferent`    | `_CopyDifferingSourceItemsToOutputDirectory`       | Copies when size or timestamp differs, restores an edited file |
|  [03]   | `Always`         | `_CopyOutOfDateSourceItemsToOutputDirectoryAlways` | Copies on every build                                          |
|  [04]   | `Never` or unset | None                                               | Copies nothing                                                 |

- `AssignTargetPaths` computes `TargetPath` from `TargetPath`, then `Link`, then the path relative to the project directory, and the SDK sets `Link` to `%(LinkBase)%(RecursiveDir)%(Filename)%(Extension)` for a file outside the project directory
- `None` and `Content` copy the same way, `Content` also joins `ContentFilesProjectOutputGroup`, and `DefaultCopyToPublishDirectoryMetadata` copies `CopyToOutputDirectory` into an unset `CopyToPublishDirectory` after `AssignTargetPaths`
- `SkipUnchangedFilesOnCopyAlways=true` applies the `IfDifferent` test to every `Always` item
- `GetCopyToOutputDirectoryItems` returns the project's items and the items of its `ProjectReference` projects transitively, `MSBuildCopyContentTransitively=false` limits that to one level, the referencing project copies them under its own `$(OutDir)%(TargetPath)`, and `Private="false"` on the reference stops that
- `ResolvePackageAssets` emits `@(RuntimeCopyLocalItems)` for managed assets, `@(NativeCopyLocalItems)` for `runtimes/<rid>/native/` assets, and `@(RuntimeTargetsCopyLocalItems)` for the RID-specific assets of a RID-less build, each with `NuGetPackageId`, `AssetType`, `CopyLocal`, and `DestinationSubDirectory` metadata
- Items with `CopyLocal=true` join `@(ReferenceCopyLocalPaths)` when `CopyLocalLockFileAssemblies` is `true`, the SDK default for a project with runtime output or `EnableDynamicLoading`, `_CopyFilesMarkedCopyLocal` writes them to `$(OutDir)%(DestinationSubDirectory)%(Filename)%(Extension)`, and publish copies the same assets whatever `CopyLocalLockFileAssemblies` is

```xml
<!-- Build and publish copies for a project file, an edited destination, and a tree outside the project -->
<ItemGroup>
  <None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
  <None Update="testdata/seed.db" CopyToOutputDirectory="IfDifferent" />
  <Content Include="../shared/assets/**" LinkBase="content/" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="Never" />
</ItemGroup>

<!-- Native asset the package layout lacks, added after ResolvePackageAssets, publish copies it and the build output copies it when CopyLocalLockFileAssemblies is true -->
<Target Name="AddNativeAsset" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <NativeCopyLocalItems Include="$(MSBuildProjectDirectory)/native/libtool.dylib"
                          NuGetPackageId="Tool.Native" NuGetPackageVersion="1.0.0"
                          AssetType="native" CopyLocal="true" DestinationSubDirectory="runtimes/osx-arm64/native/" />
  </ItemGroup>
</Target>
```

- Use `dotnet-msbuild-evaluation` for `Update` placement and the `Directory.Build.targets` import order the items depend on
