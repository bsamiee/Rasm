# [EVALUATION_AND_INCREMENTALITY]

Use the evaluation workflow for evaluation cost. Use the incrementality workflow when a later build executes unexpected work.

## [01]-[EVALUATION]

MSBuild evaluates a project before it executes targets. A project evaluation has these passes:

1. Environment variables
2. Imports and properties
3. Item definitions
4. Items
5. `UsingTask` elements
6. Targets

### [01.1]-[BINLOG_DIAGNOSIS]

1. Run `binlog_overview` for the total duration and build status.
2. Run `binlog_evaluations` to identify costly project evaluations.
3. Run `binlog_evaluation_global_properties` for each suspect evaluation.
4. Run `binlog_evaluation_properties` when an evaluated property value is relevant.
5. Run `binlog_imports` for the import chain.
6. Run `binlog_items` to examine evaluated item counts and contents.
7. Run `binlog_search_files` for glob and property-function declarations.
8. Run `binlog_preprocess` only when the complete imported source is necessary.

GLOBS:
- Broad recursive globs can enumerate large directory trees.
- For SDK default items, add large excluded directories to `DefaultItemExcludes`.
- For a custom `Include`, put `Exclude` on the same item element.
- Disable a default item type only when the project supplies every required item of that type.
- Change a glob only when its evaluated items and measured cost identify it as a problem.

IMPORTS:
- Change an import only when measured evaluation cost identifies it.
- Preserve the required property, item, task, and target order.

MULTIPLE EVALUATIONS:
- Each unique project and global-property combination can require a separate evaluation.
- Outer builds, inner builds, and restore can create expected evaluations.
- Investigate only unexpected differences in global-property sets.
- Remove an evaluation only when its project instance is not required for the requested build.

PROPERTY FUNCTIONS:
- A property function in an evaluated property or item expression runs during evaluation.
- Change a file-system query or costly expression only when measured evaluation cost identifies it.
- Keep evaluation expressions deterministic and free of side effects.

## [02]-[INCREMENTALITY]

TARGET INPUTS AND OUTPUTS:
- A file-producing target declares both `Inputs` and `Outputs`.
- A transform in `Outputs` maps each output to its corresponding input.
- A discrete output compares against all declared inputs.
- An output is current when its timestamp is equal to or newer than its mapped inputs.
- MSBuild can run a partial incremental build when only some mapped outputs are stale.
- A requested target without `Outputs` runs each time that MSBuild schedules it.
- MSBuild uses timestamps, not content hashes, for target incrementality.

```xml
<!-- Map each output to its corresponding input. -->
<Target Name="Transform"
        Inputs="@(TransformFiles)"
        Outputs="@(TransformFiles->'$(IntermediateOutputPath)%(Filename).out')">
  <WriteLinesToFile File="$(IntermediateOutputPath)%(TransformFiles.Filename).out"
                    Lines="%(TransformFiles.Identity)"
                    Overwrite="true"
                    WriteOnlyWhenDifferent="true" />
</Target>
```

FILE  WRITES:
- `FileWrites` records generated files for clean bookkeeping. It does not participate in a target timestamp check.
- Add each generated file to `FileWrites` inside its producing target.
- An intrinsic `ItemGroup` preserves the item during output inference when MSBuild skips the target.

### [02.1]-[BINLOG_DIAGNOSIS]

```bash
dotnet build -bl:prime-incremental-{} # establish the outputs
dotnet build -bl:incremental-{}       # capture the no-change build
```

Analyze the second binlog:
1. Run `binlog_overview`.
2. Run `binlog_incremental_analysis` for target decisions, triggering files, and `IncrementalClean` deletions.
3. Run `binlog_project_target_times` for each suspect project.
4. Keep targets with `skipped: false`.
5. Run `binlog_target_reasons` for each unresolved target.
6. Run `binlog_expensive_targets` to prioritize costly rebuilt targets.
7. Run `binlog_preprocess` only when the imported target declaration is necessary.

`Building target "X" completely` and its reason identify the stale input or missing output. A target without an up-to-date reason or a `Skipped:` line has no declared `Inputs` and `Outputs`. See `dotnet-msbuild-antipatterns`, `AP-16`.

### [02.2]-[COMMON_DEFECTS]

1. A file-producing target omits `Inputs` and `Outputs`.
2. An output path contains a timestamp, build number, or random value.
3. The target writes a file that `Outputs` does not declare.
4. The declared inputs omit a file that affects the output.
5. A changed property alters a declared input or output path.
6. A removed glob input leaves an old output because MSBuild sees only the current input list.
7. A task rewrites unchanged output content and changes its timestamp.

### [02.3]-[CUSTOM_TARGET]

- `Inputs` names the project file and each source file that affects generation.
- `Outputs` uses one stable intermediate path.
- `BeforeTargets="CoreCompile"` makes the generated file available to the compiler.
- `WriteOnlyWhenDifferent="true"` preserves the output timestamp when content does not change.
- `FileWrites` records the generated file for clean operations.
- `Compile` adds the generated file during target execution and output inference.

```xml
<Target Name="GenerateConfig"
        Inputs="$(MSBuildProjectFile);@(ConfigInput)"
        Outputs="$(IntermediateOutputPath)config.generated.cs"
        BeforeTargets="CoreCompile">
  <WriteLinesToFile File="$(IntermediateOutputPath)config.generated.cs"
                    Lines="@(GeneratedLines)"
                    Overwrite="true"
                    WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)config.generated.cs" />
    <Compile Include="$(IntermediateOutputPath)config.generated.cs" />
  </ItemGroup>
</Target>
```
