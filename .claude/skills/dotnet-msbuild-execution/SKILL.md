---
name: dotnet-msbuild-execution
description: "Use when adding or ordering an MSBuild target, extending a DependsOn chain, making a target incremental, copying items to output, or registering generated files."
---

# [DOTNET_MSBUILD_EXECUTION]

Covers the execution phase: target dependency chains, target ordering, `Returns` and `Outputs`, incremental targets, output copies, and the files that a target generates.

## [01]-[DEPENDENCY_CHAINS]

The standard `Build` chain delegates work through a dependency property. It has before, core, and after targets:

```xml
<PropertyGroup>
  <BuildDependsOn>
    BeforeBuild;
    CoreBuild;
    AfterBuild
  </BuildDependsOn>
</PropertyGroup>

<Target Name="Build"
        Condition="'$(_InvalidConfigurationWarning)' != 'true'"
        DependsOnTargets="$(BuildDependsOn)"
        Returns="@(TargetPathWithTargetPlatformMoniker)" />

<Target Name="BeforeBuild" />
<Target Name="AfterBuild" />
```

`CoreBuild` delegates to its dependency property. Its `OnError` elements run when a task failure propagates with `ContinueOnError="ErrorAndStop"` or `false`:

```xml
<Target Name="CoreBuild" DependsOnTargets="$(CoreBuildDependsOn)">
  <OnError ExecuteTargets="_TimeStampAfterCompile;PostBuildEvent"
           Condition="'$(RunPostBuildEvent)' == 'Always' or '$(RunPostBuildEvent)' == 'OnOutputUpdated'" />
  <OnError ExecuteTargets="_CleanRecordFileWrites" />
</Target>
```

- `OnError` is optional. When a target has `OnError` elements, they must be its final child elements.
- In SDK-style project files, later implicit imports can replace a project-level target definition.
- Extend imported targets with `BeforeTargets` or `AfterTargets`.

## [02]-[CHAIN_EXTENSION]

Extend a dependency property after its owner assigns the base value. Preserve its current value:

```xml
<!-- This assignment follows the base CompileDependsOn assignment. -->
<PropertyGroup>
  <CompileDependsOn>$(CompileDependsOn);MyPostCompileTarget</CompileDependsOn>
</PropertyGroup>
```

An assignment that omits `$(CompileDependsOn)` removes the existing chain. Use target injection when you do not own the chain or its import order.

## [03]-[TARGET_ORDERING]

A target runs at most once per project instance. Declaration order does not establish execution order.

| [INDEX] | [ATTRIBUTE]        | [OWNER]              | [PURPOSE]                                 |
| :-----: | :----------------- | :------------------- | :---------------------------------------- |
|  [01]   | `DependsOnTargets` | The dependent target | Runs required targets first               |
|  [02]   | `BeforeTargets`    | The inserted target  | Runs before a target that it does not own |
|  [03]   | `AfterTargets`     | The inserted target  | Runs after a target that it does not own  |

```xml
<Target Name="ValidateInputs" BeforeTargets="CoreCompile">
  <Error Text="MyInput items are required." Condition="'@(MyInput)' == ''" />
</Target>
```

## [04]-[RETURNS_AND_OUTPUTS]

`Returns` specifies the values that an `<MSBuild>` task receives from the target. `Outputs` participates in timestamp-based incremental execution.

A target can set both attributes:
- `Returns` selects returned values when present.
- If one project target declares `Returns`, only targets with `Returns` record returned values. Otherwise, `Outputs` supplies the returned values.
- Use `Returns` for query targets that do not need timestamp comparisons.
- Use paired `Inputs` and `Outputs` for incremental work.

```xml
<Target Name="GetMyFeatureOutput" Returns="@(MyFeatureOutput)" />
```

## [05]-[RESOLVE_PROJECT_REFERENCES_DURATION]

`ResolveProjectReferences` can invoke referenced-project builds. Its inclusive duration includes their execution, including yielded wait. An MSBuild node can yield while a referenced build runs. The target duration is not CPU self-time or the build critical path.

1. Use `binlog_build_graph` to identify the critical path and the project that owns each interval.
2. Use `binlog_expensive_projects` to rank exclusive project work.
3. Use `binlog_project_target_times` to examine each suspect project on the critical path.
4. Use `binlog_tasks_in_target` to inspect each slow target in that project.
5. Use `binlog_expensive_tasks` only for build-wide aggregated duration.

- Select the optimization target from critical-path work.
- A high `ResolveProjectReferences` duration alone does not identify that work.
- Use the `dotnet-msbuild-diagnostics` skill for binlog capture and comparable performance baselines.

## [06]-[COPY_TO_OUTPUT_DIRECTORY]

`CopyToOutputDirectory` controls build-output copies. `CopyToPublishDirectory` controls publish-output copies. Both metadata values accept the same four modes:

| [INDEX] | [MODE]           | [BEHAVIOR]                                                               |
| :-----: | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `Never`          | Does not copy the item                                                   |
|  [02]   | `PreserveNewest` | Copies when the destination is missing or the source is newer            |
|  [03]   | `Always`         | Runs the copy on every build unless skip-unchanged behavior is active    |
|  [04]   | `IfDifferent`    | Copies when the destination is missing, or its size or timestamp differs |

An unset `CopyToOutputDirectory` value also causes no build-output copy.

```xml
<ItemGroup>
  <None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
  <None Update="testdata/seed.db" CopyToOutputDirectory="IfDifferent" />
</ItemGroup>
```

IFDIFFERENT:
- `CopyToPublishDirectory="IfDifferent"` requires .NET SDK 10.0.200.
- `CopyToOutputDirectory="IfDifferent"` requires MSBuild 17.13 or .NET SDK 9.0.200.
- The task compares the file size and last-write timestamp. It does not compare file contents. Equal size and timestamp values cause the task to skip the copy.
- Use this mode when an application or test changes the destination and the next build must restore the source version.

ALWAYS:
- `Always` copies on each build by default. Its default value is `false`.
- Set `SkipUnchangedFilesOnCopyAlways` to apply the same test to all `CopyToOutputDirectory="Always"` items:

```xml
<PropertyGroup>
  <SkipUnchangedFilesOnCopyAlways>true</SkipUnchangedFilesOnCopyAlways>
</PropertyGroup>
```

EXECUTION:
- These targets add their destination items to `@(FileWrites)`.
- The project-reference collection can carry `Always`, `PreserveNewest`, and `IfDifferent` items when its child-item conditions pass. It does not collect `Never` items.

| [INDEX] | [MODE]           | [TARGET]                                           | [MECHANISM]                   |
| :-----: | :--------------- | :------------------------------------------------- | :---------------------------- |
|  [01]   | `PreserveNewest` | `_CopyOutOfDateSourceItemsToOutputDirectory`       | Target `Inputs` and `Outputs` |
|  [02]   | `Always`         | `_CopyOutOfDateSourceItemsToOutputDirectoryAlways` | `Copy` task                   |
|  [03]   | `IfDifferent`    | `_CopyDifferingSourceItemsToOutputDirectory`       | `Copy` with skip-unchanged    |

## [07]-[INCREMENTAL_TARGETS]

MSBuild skips the target when every output is up-to-date. It can partially build one-to-one input and output mappings.

- `Inputs` names the files that drive a target.
- `Outputs` names the files that the target produces.
- Keep generated outputs under `$(IntermediateOutputPath)`.

## [08]-[INCLUDING_GENERATED_FILES]

Evaluation-time globs cannot include files that target execution creates later. Add each generated file to the item type that consumes it.

- Write generated files under `$(IntermediateOutputPath)`. This property follows the configured intermediate-output layout on each operating system.
- Add each generated file to `@(FileWrites)`.
- `IncrementalClean` removes stale tracked files.
- `Clean` removes all tracked files.
- Do not hardcode `obj/` or reconstruct its configuration and framework segments.

EXECUTION TIME GLOBS:
- A glob outside a target expands during evaluation.
- A glob inside a target expands when that target runs.

## [08.1]-[GENERATED_SOURCE]

Generate source before `CoreCompile`. Add the file to `@(Compile)` in the same target:

```xml
<PropertyGroup>
  <GeneratedCodeDirectory>$(IntermediateOutputPath)Generated/</GeneratedCodeDirectory>
  <GeneratedSource>$(GeneratedCodeDirectory)MyGeneratedFile.cs</GeneratedSource>
</PropertyGroup>

<Target Name="GenerateSource"
        BeforeTargets="BeforeCompile;CoreCompile"
        Inputs="$(MSBuildProjectFullPath)"
        Outputs="$(GeneratedSource)">
  <MakeDir Directories="$(GeneratedCodeDirectory)" />
  <WriteLinesToFile File="$(GeneratedSource)" Lines="// Generated file." Overwrite="true" />
  <ItemGroup>
    <Compile Include="$(GeneratedSource)" />
    <FileWrites Include="$(GeneratedSource)" />
  </ItemGroup>
</Target>
```

`BeforeCompile` is the common-targets convention for generated `Compile` items. The `CoreCompile` hook also covers builds that invoke it directly.

## [08.2]-[GENERATED_OUTPUT_ITEM]

Add generated `None` or `Content` items before `AssignTargetPaths`. This target is the final item-transformation boundary for these item types.

```xml
<PropertyGroup>
  <GeneratedData>$(IntermediateOutputPath)Generated/data.json</GeneratedData>
</PropertyGroup>

<Target Name="GenerateData"
        BeforeTargets="AssignTargetPaths"
        Inputs="$(MSBuildProjectFullPath)"
        Outputs="$(GeneratedData)">
  <MakeDir Directories="$(IntermediateOutputPath)Generated/" />
  <WriteLinesToFile File="$(GeneratedData)" Lines="{}" Overwrite="true" />
  <ItemGroup>
    <None Include="$(GeneratedData)"
          TargetPath="Generated/data.json"
          CopyToOutputDirectory="PreserveNewest" />
    <FileWrites Include="$(GeneratedData)" />
  </ItemGroup>
</Target>
```
