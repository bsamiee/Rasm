---
name: dotnet-msbuild-execution
description: "Use when adding or ordering an MSBuild target, extending a DependsOn chain, making a target incremental, copying items to output, or registering generated files."
---

# [DOTNET_MSBUILD_EXECUTION]

Covers the execution phase: dependency chains, target ordering, `Returns` and `Outputs`, output copies, incremental targets, generated files, and target scope.

## [01]-[DEPENDENCY_CHAINS]

The standard `Build` chain delegates work through a `DependsOn` property. It has before, core, and after targets:

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

- The implicit SDK import follows the project body. The SDK definition of a target replaces a same-named definition in the project body. A `BeforeBuild` or `AfterBuild` target in a `.csproj` never runs, and MSBuild logs `Overriding target` for it at diagnostic verbosity only.
- Extend imported targets with `BeforeTargets` or `AfterTargets`.

`CoreBuild` delegates to its `DependsOn` property and declares the `OnError` elements:

```xml
<Target Name="CoreBuild" DependsOnTargets="$(CoreBuildDependsOn)">
  <OnError ExecuteTargets="_TimeStampAfterCompile;PostBuildEvent"
           Condition="'$(RunPostBuildEvent)' == 'Always' or '$(RunPostBuildEvent)' == 'OnOutputUpdated'" />
  <OnError ExecuteTargets="_CleanRecordFileWrites" />
</Target>
```

- `OnError` runs when the target itself fails, and when a target in its `DependsOnTargets` chain fails and stops the build. Each `OnError` element has its own `Condition`.
- `OnError` elements must be the final child elements of the target. A later child fails with `MSB4038`.
- `ContinueOnError` on a task selects the failure mode. `ErrorAndStop`, the default, fails the target. `ErrorAndContinue` keeps the errors and continues. `WarnAndContinue` logs them as warnings and continues.

## [02]-[CHAIN_EXTENSION]

Extend a `DependsOn` property after its owner assigns the base value. Preserve its current value:

```xml
<!-- Directory.Build.targets, or a .targets file imported after the SDK targets -->
<PropertyGroup>
  <CompileDependsOn>$(CompileDependsOn);MyPostCompileTarget</CompileDependsOn>
</PropertyGroup>
```

The SDK assigns `CompileDependsOn` after the project body, and the same assignment in a `.csproj` is lost. An assignment that omits `$(CompileDependsOn)` removes the existing chain.

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

`BeforeTargets` and `AfterTargets` accept a name that no target defines. The target then never runs. MSBuild logs the unmatched name at diagnostic verbosity only.

A false `Condition` on a target skips its body and its `DependsOnTargets` targets. Targets that name it in `BeforeTargets` or `AfterTargets` still run. A later `DependsOnTargets` request runs the skipped target when the condition is true at that time.

## [04]-[RETURNS_AND_OUTPUTS]

`Returns` specifies the values that an `<MSBuild>` task receives from the target. `Outputs` drives the timestamp-based up-to-date check.

A target can set both attributes:
- Once one target in the project declares `Returns`, a target with only `Outputs` returns nothing. The SDK `Build` target declares `Returns`, and this holds in every SDK project.
- Declare `Returns` on every target that a caller queries.

```xml
<Target Name="GetMyFeatureOutput" Returns="@(MyFeatureOutput)" />

<!-- The caller reads the returned values through TargetOutputs -->
<MSBuild Projects="@(ProjectReference)" Targets="GetMyFeatureOutput">
  <Output TaskParameter="TargetOutputs" ItemName="CollectedFeatureOutput" />
</MSBuild>
```

`TargetOutputs` contains returns from the named targets only, never from their dependencies. Each item has `MSBuildSourceProjectFile` and `MSBuildSourceTargetName` metadata. `dotnet msbuild <project> -t:<target> -getTargetResult:<target>` prints the returned items as JSON, which tests a query target without a caller.

## [05]-[COPY_TO_OUTPUT_DIRECTORY]

`CopyToOutputDirectory` controls build-output copies. `CopyToPublishDirectory` controls publish-output copies. An item with `CopyToOutputDirectory` set and `CopyToPublishDirectory` unset publishes with the same mode, which the `DefaultCopyToPublishDirectoryMetadata` target applies. These items reach the output of every referencing project and the publish directory. A `Copy` task in a custom target reaches neither. Both metadata values accept the same four modes:

| [INDEX] | [MODE]           | [BEHAVIOR]                                                               |
| :-----: | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `Never`          | Does not copy the item                                                   |
|  [02]   | `PreserveNewest` | Copies when the destination is missing or the source is newer            |
|  [03]   | `Always`         | Copies on every build                                                    |
|  [04]   | `IfDifferent`    | Copies when the destination is missing, or its size or timestamp differs |

An unset `CopyToOutputDirectory` value also copies nothing.

```xml
<ItemGroup>
  <None Update="settings.ini" CopyToOutputDirectory="PreserveNewest" />
  <None Update="testdata/seed.db" CopyToOutputDirectory="IfDifferent" />
  <None Include="../../shared/**" LinkBase="assets/" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

DESTINATION:
- The copy targets write each item to `$(OutDir)%(TargetPath)`. `AssignTargetPath` reads `TargetPath` first, then `Link`, then the path relative to the project directory. An item outside that directory with neither value copies under its file name alone.
- The SDK sets `Link` to `%(LinkBase)%(RecursiveDir)%(Filename)%(Extension)` for an item outside the project directory. A recursive glob keeps its tree, and `LinkBase` places that tree under one output folder.

ALWAYS:
- `Always` copies on every build. `dotnet build -check` reports `BC0106` for every `Always` item.
- `SkipUnchangedFilesOnCopyAlways` defaults to `false`. Set it to apply the `IfDifferent` test to every `Always` item:

```xml
<PropertyGroup>
  <SkipUnchangedFilesOnCopyAlways>true</SkipUnchangedFilesOnCopyAlways>
</PropertyGroup>
```

IFDIFFERENT:
- The task compares the file size and last-write timestamp. It does not compare file contents. Equal size and timestamp skip the copy.
- When an application or a test changes the destination, use this mode. The next build then restores the source version.

EXECUTION:
- The three copy targets in the table add each destination to `@(FileWrites)`.
- `GetCopyToOutputDirectoryItems` collects this project's items and the items of transitively referenced projects. It tests each item for `Always`, `PreserveNewest`, or `IfDifferent`.

| [INDEX] | [MODE]           | [TARGET]                                           | [MECHANISM]                      |
| :-----: | :--------------- | :------------------------------------------------- | :------------------------------- |
|  [01]   | `PreserveNewest` | `_CopyOutOfDateSourceItemsToOutputDirectory`       | Target `Inputs` and `Outputs`    |
|  [02]   | `Always`         | `_CopyOutOfDateSourceItemsToOutputDirectoryAlways` | `Copy` task                      |
|  [03]   | `IfDifferent`    | `_CopyDifferingSourceItemsToOutputDirectory`       | `Copy` with `SkipUnchangedFiles` |

## [06]-[INCREMENTAL_TARGETS]

MSBuild skips the target when every output is up-to-date.

- `Inputs` names the files that drive a target. `Outputs` names the files that the target produces.
- MSBuild compares timestamps. It does not compare file contents.
- A target that declares `Inputs` without `Outputs` fails the build with `MSB4058`.
- An `Inputs` or `Outputs` expression that evaluates to empty skips the target. The log gives the reason.
- A transform in `Outputs` maps each output to one input. MSBuild then rebuilds the stale items only, and the target sees only those inputs.
- A discrete output compares against every declared input. MSBuild then runs the whole target.
- MSBuild reads the input list of the current run only. A file that left the list does not make the target stale.

```xml
<!-- One output per input, so a stale input rebuilds only its own output -->
<Target Name="Transform"
        Inputs="@(TransformFiles)"
        Outputs="@(TransformFiles->'$(IntermediateOutputPath)%(Filename).g.cs')">
  <!-- @(TransformFiles) holds only the stale items here -->
</Target>
```

## [07]-[INCLUDING_GENERATED_FILES]

A glob outside a target expands during evaluation. A glob inside a target expands when that target runs. An evaluation-time glob cannot include a file that target execution creates later. Add each generated file to the item type that consumes it.

- Write generated files under `$(IntermediateOutputPath)`. The property contains the configured intermediate directory, including any artifacts layout.
- Read `$(IntermediateOutputPath)` inside the target. A `PropertyGroup` in the project body reads it before the SDK sets it, and `-check` reports `BC0202`.
- Put `$(MSBuildAllProjects)` in `Inputs`. It names the newest file among the project and every file it imports. A change to `Directory.Build.props` or to the `.targets` file that contains the generator then rebuilds the output. `$(MSBuildProjectFullPath)` names the project alone.
- Add each generated file to `@(FileWrites)`.
- A skipped target still applies its `ItemGroup` and `PropertyGroup` children. A `Compile` or `FileWrites` item added there survives the skip. An item set through a task `<Output>` element does not.
- `IncrementalClean` deletes files that a prior build wrote and this build did not write. `Clean` deletes every recorded file. Both delete only under `$(OutDir)` or `$(IntermediateOutputPath)`.

## [07.1]-[GENERATED_SOURCE]

Use `BeforeTargets="CoreCompile"`, never `BeforeCompile`. A design-time build calls `CoreCompile` alone. Add the generated file to `@(Compile)` in the same target:

```xml
<Target Name="GenerateSource"
        BeforeTargets="CoreCompile"
        Inputs="$(MSBuildAllProjects)"
        Outputs="$(IntermediateOutputPath)Generated/MyGeneratedFile.cs">
  <MakeDir Directories="$(IntermediateOutputPath)Generated/" />
  <WriteLinesToFile File="$(IntermediateOutputPath)Generated/MyGeneratedFile.cs"
                    Lines="// Generated file." Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <Compile Include="$(IntermediateOutputPath)Generated/MyGeneratedFile.cs" />
    <FileWrites Include="$(IntermediateOutputPath)Generated/MyGeneratedFile.cs" />
  </ItemGroup>
</Target>
```

## [07.2]-[GENERATED_OUTPUT_ITEM]

Add generated `None` or `Content` items before `AssignTargetPaths`. This target assigns `TargetPath` to these item types, and a later item has none.

```xml
<Target Name="GenerateData"
        BeforeTargets="AssignTargetPaths"
        Inputs="$(MSBuildAllProjects)"
        Outputs="$(IntermediateOutputPath)Generated/data.json">
  <MakeDir Directories="$(IntermediateOutputPath)Generated/" />
  <WriteLinesToFile File="$(IntermediateOutputPath)Generated/data.json"
                    Lines="{}" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <None Include="$(IntermediateOutputPath)Generated/data.json"
          TargetPath="Generated/data.json"
          CopyToOutputDirectory="PreserveNewest" />
    <FileWrites Include="$(IntermediateOutputPath)Generated/data.json" />
  </ItemGroup>
</Target>
```

## [08]-[TARGET_SCOPE]

A target that extends `Build` in a multi-targeting project runs once per target framework and once for the outer build. `dotnet publish` passes `_IsPublishing=true` as a global property. `dotnet build` and `dotnet pack` do not.

| [INDEX] | [SCOPE]          | [CONDITION]                    |
| :-----: | :--------------- | :----------------------------- |
|  [01]   | Inner build only | `'$(TargetFramework)' != ''`   |
|  [02]   | Outer build only | `'$(TargetFramework)' == ''`   |
|  [03]   | Publish only     | `'$(_IsPublishing)' == 'true'` |

Per-project work belongs in the outer build. Per-assembly work belongs in the inner build.
