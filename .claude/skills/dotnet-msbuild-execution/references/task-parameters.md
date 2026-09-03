# [TASK_PARAMETERS]

Each built-in task a custom target uses lists the parameters that decide correctness and incremental behavior. Every task accepts `Condition` and `ContinueOnError`, and every `ToolTask` (`Exec`, `Csc`) also accepts `EnvironmentVariables`, `EchoOff`, `StandardOutputImportance`, `StandardErrorImportance`, `LogStandardErrorAsError`, `Timeout`, and the `ExitCode` output.

## [01]-[FILE_TASKS]

- `Copy` (`SourceFiles`, `DestinationFiles` or `DestinationFolder`, `SkipUnchangedFiles`, `UseHardlinksIfPossible`, `UseSymbolicLinksIfPossible`, `Retries`, `RetryDelayMilliseconds`, `OverwriteReadOnlyFiles`, output `CopiedFiles`) — `DestinationFiles` maps one to one with `SourceFiles`, and `CopiedFiles` includes the skipped files
- `MakeDir` (`Directories`) — creates every missing level, and `Copy` with `DestinationFolder` creates the folder itself
- `Delete` (`Files`), `RemoveDir` (`Directories`) — `Delete` never fails on a missing file
- `WriteLinesToFile` (`File`, `Lines`, `Overwrite`, `WriteOnlyWhenDifferent`, `Encoding`)
- `ReadLinesFromFile` (`File`, output `Lines`) — one item per line, and an `<Output>` to a `PropertyName` joins the lines with `;`
- `Touch` (`Files`, `AlwaysCreate`, `ForceTouch`, `Time`, output `TouchedFiles`) — `AlwaysCreate="true"` writes a marker file
- `Hash` (`ItemsToHash`, `IgnoreCase`, output `HashResult`) — hashes the item specs and never file contents, `GetFileHash` hashes content
- `ConvertToAbsolutePath` (`Paths`, output `AbsolutePaths`) — resolves against the project directory
- `ZipDirectory` (`SourceDirectory`, `DestinationFile`, `Overwrite`) — fails on an existing file unless `Overwrite="true"`
- `Unzip` (`SourceFiles`, `DestinationFolder`, `SkipUnchangedFiles`, `OverwriteReadOnlyFiles`) — `SkipUnchangedFiles` defaults to `true`
- `DownloadFile` (`SourceUrl`, `DestinationFolder`, `DestinationFileName`, `SkipUnchangedFiles`, `Retries`, output `DownloadedFile`) — `SkipUnchangedFiles` needs a last-modified header from the server
- `GetReferenceAssemblyPaths` (`TargetFrameworkMoniker`, `RootPath`, output `ReferenceAssemblyPaths`) — .NET Framework reference assemblies only, and `PrepareForBuild` already runs it

## [02]-[CONTROL_TASKS]

- `Exec` (`Command`, `WorkingDirectory`, `ConsoleToMSBuild`, `IgnoreExitCode`, `IgnoreStandardErrorWarningFormat`, `CustomErrorRegularExpression`, `CustomWarningRegularExpression`, output `ConsoleOutput`, output `ExitCode`) — `ExitCode` is `-1` when the tool exited 0 and the task logged an error
- `Message` (`Text`, `Importance`, `Code`, `File`) — `high` shows at minimal verbosity, `low` at detailed
- `Warning` (`Text`, `Code`, `File`, `HelpLink`)
- `Error` (`Text`, `Code`, `File`, `HelpLink`) — stops the target, and `ContinueOnError` on the `Error` element downgrades it
- `MSBuild` (`Projects`, `Targets`, `Properties`, `RemoveProperties`, `BuildInParallel`, `SkipNonexistentProjects`, `SkipNonexistentTargets`, `StopOnFirstFailure`, `RunEachTargetSeparately`, `RebaseOutputs`, output `TargetOutputs`) — `Properties` and `AdditionalProperties` metadata on a `Projects` item override or extend the parameter
- `CallTarget` (`Targets`, `RunEachTargetSeparately`, `UseResultsCache`, output `TargetOutputs`)
- `CreateProperty` (`Value`, output `Value` or `ValueSetByTask`) — `ValueSetByTask` is set only when the target ran, never through output inference

## [03]-[INLINE_TASK]

`RoslynCodeTaskFactory` compiles the `Code` element in memory, and `Type="Fragment"` supplies the body of `Execute` with every `ParameterGroup` entry as a property.

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

- `Type="Class"` takes a whole `ITask` class and infers the parameters from it, and `Source` on `Code` reads the class from a file
- The compiled task loads into the build node, and a task that throws fails the build with the exception in the log
