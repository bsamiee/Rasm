# [TASK_PARAMETERS]

Parameters per built-in task that decide correctness and incremental behavior. Every task accepts `Condition` and `ContinueOnError`, every `ToolTask` (`Exec`, `Csc`) accepts `EnvironmentVariables`, `EchoOff`, `StandardOutputImportance`, `StandardErrorImportance`, `LogStandardErrorAsError`, `Timeout`, and the `ExitCode` output.

## [01]-[FILE_TASKS]

- `Copy` (`SourceFiles`, `DestinationFiles` or `DestinationFolder`, `SkipUnchangedFiles`, `UseHardlinksIfPossible`, `Retries`, output `CopiedFiles`)
- `Copy` `DestinationFiles` maps one to one with `SourceFiles`, `CopiedFiles` includes the skipped files
- `Copy` `Retries` with `RetryDelayMilliseconds` repeats a failed copy
- `Copy` `OverwriteReadOnlyFiles` clears the attribute, `UseSymboliclinksIfPossible` links
- `MakeDir` (`Directories`) creates every missing level, `Copy` with `DestinationFolder` creates the folder itself
- `Delete` (`Files`), `RemoveDir` (`Directories`), `Delete` never fails on a missing file
- `WriteLinesToFile` (`File`, `Lines`, `Overwrite`, `WriteOnlyWhenDifferent`, `Encoding`)
- `ReadLinesFromFile` (`File`, output `Lines`), one item per line, an `<Output>` to a `PropertyName` joins the lines with `;`
- `Touch` (`Files`, `AlwaysCreate`, `ForceTouch`, `Time`, output `TouchedFiles`), `AlwaysCreate="true"` writes a marker file
- `Hash` (`ItemsToHash`, `IgnoreCase`, output `HashResult`) hashes the item specs, `GetFileHash` hashes content
- `ConvertToAbsolutePath` (`Paths`, output `AbsolutePaths`) resolves against the project directory
- `ZipDirectory` (`SourceDirectory`, `DestinationFile`, `Overwrite`) fails on an existing file unless `Overwrite="true"`
- `Unzip` (`SourceFiles`, `DestinationFolder`, `SkipUnchangedFiles`, `OverwriteReadOnlyFiles`), `SkipUnchangedFiles` defaults to `true`
- `DownloadFile` (`SourceUrl`, `DestinationFolder`, `DestinationFileName`, `SkipUnchangedFiles`, `Retries`, output `DownloadedFile`)
- `DownloadFile` `SkipUnchangedFiles` defaults to `true` and needs a last-modified header from the server
- `GetReferenceAssemblyPaths` (`TargetFrameworkMoniker`, `RootPath`, output `ReferenceAssemblyPaths`) resolves .NET Framework reference assemblies
- `PrepareForBuild` runs `GetReferenceAssemblyPaths` in every build, a second call in a project changes nothing
- `Delete` and `Touch` take items in `Files`, a wildcard there is a literal path, an item `Include` expands it

## [02]-[CONTROL_TASKS]

- `Exec` (`Command`, `WorkingDirectory`, `ConsoleToMSBuild`, `IgnoreExitCode`, `IgnoreStandardErrorWarningFormat`, output `ConsoleOutput`, `ExitCode`)
- `Exec` `CustomErrorRegularExpression` and `CustomWarningRegularExpression` add patterns to the standard error and warning format
- `Exec` `ExitCode` is `-1` when the tool exited 0 and the task logged an error
- `Message` (`Text`, `Importance`, `Code`, `File`), `high` shows at minimal verbosity, `low` at detailed
- `Warning` (`Text`, `Code`, `File`, `HelpLink`)
- `Error` (`Text`, `Code`, `File`, `HelpLink`) stops the target, `ContinueOnError` on the `Error` element downgrades it
- `MSBuild` (`Projects`, `Targets`, `Properties`, `RemoveProperties`, `BuildInParallel`, `StopOnFirstFailure`, output `TargetOutputs`)
- `MSBuild` `SkipNonexistentProjects` and `SkipNonexistentTargets` skip a missing project or target
- `MSBuild` `RebaseOutputs` rebases returned relative paths to the caller
- `MSBuild` `RunEachTargetSeparately` calls each target on its own, a failed target then stops none of the following targets
- `Properties` and `AdditionalProperties` metadata on a `Projects` item override or extend the `Properties` parameter
- `CallTarget` (`Targets`, `RunEachTargetSeparately`, `UseResultsCache`, output `TargetOutputs`)
- `CreateProperty` (`Value`, output `Value` or `ValueSetByTask`), `ValueSetByTask` is set when the target ran, never through output inference

## [03]-[INLINE_TASK]

`RoslynCodeTaskFactory` compiles the `Code` element in memory, `Type="Fragment"` supplies the body of `Execute` with every `ParameterGroup` entry as a property.

```xml
<UsingTask TaskName="CountLines" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">
  <ParameterGroup>
    <Files ParameterType="Microsoft.Build.Framework.ITaskItem[]" Required="true" />
    <Total ParameterType="System.Int32" Output="true" />
  </ParameterGroup>
  <Task>
    <Using Namespace="System.IO" />
    <Using Namespace="System.Linq" />
    <Code Type="Fragment" Language="cs">
      <![CDATA[
        Total = Files.Sum(file => File.ReadAllLines(file.ItemSpec).Length);
      ]]>
    </Code>
  </Task>
</UsingTask>

<Target Name="CountStagedLines">
  <CountLines Files="@(Staged)">
    <Output TaskParameter="Total" PropertyName="LineTotal" />
  </CountLines>
</Target>
```

- `Type="Class"` takes a whole `ITask` class and infers the parameters from it, `Source` on `Code` reads the class from a file
- Compiled tasks load into the build node, a throw fails the build with the exception in the log
