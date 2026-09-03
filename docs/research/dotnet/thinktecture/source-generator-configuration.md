<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
# [SOURCE_GENERATOR_CONFIGURATION]

The source generator reads project-level MSBuild properties. They control diagnostics logging, the generated JetBrains annotation, and a debugging counter, and they apply to every generated type in the project. The generator keeps a second file named `ThinktectureRuntimeExtensionsSourceGenerator.log` in the temp folder of the process that hosts the compiler. Setting `UseSharedCompilation` to `false` starts the compiler from the build, and the self-log follows the build's temp folder.

## [01]-[MSBUILD_PROPERTIES]

`Thinktecture.Runtime.Extensions` provides a props file that declares one `CompilerVisibleProperty` item per property. MSBuild forwards each item to the compiler as an analyzer config global option named `build_property.<PropertyName>`. Global analyzer configuration files set the same option directly with a `build_property.` key and need no MSBuild property.

Every property name below carries the prefix `ThinktectureRuntimeExtensions_SourceGenerator_`.

| [INDEX] | [PROPERTY]                     | [VALUES]                                                                      | [DEFAULT] |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------- | :-------- |
|  [01]   | `LogFilePath`                  | File or folder path, trimmed                                                  | No log    |
|  [02]   | `LogFilePathMustBeUnique`      | `true` or `false`                                                             | `true`    |
|  [03]   | `LogLevel`                     | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`, case-insensitive | `Warning` |
|  [04]   | `LogMessageInitialBufferSize`  | Integer of at least 100                                                       | `100`     |
|  [05]   | `GenerateJetBrainsAnnotations` | `disable`, `disabled`, `false`, or `0` turn it off, case-insensitive          | On        |
|  [06]   | `Counter`                      | `enable`, `enabled`, `true`, or `1` turn it on, case-insensitive              | Off       |

- Blank `LogFilePath` disables file logging, and `LogFilePath` gates the other logging properties: without it the generator reads no log level, no uniqueness flag, and no buffer size
- `LogMessageInitialBufferSize` presizes the in-memory queue of pending messages inside one sink and changes no output
- Every other string leaves the annotation on and the counter off
- The properties live in a `PropertyGroup` of the project file or in a shared build props file for a whole tree

```xml
<PropertyGroup>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogFilePath>$(MSBuildProjectDirectory)/logs/generator.txt</ThinktectureRuntimeExtensions_SourceGenerator_LogFilePath>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogLevel>Information</ThinktectureRuntimeExtensions_SourceGenerator_LogLevel>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogFilePathMustBeUnique>false</ThinktectureRuntimeExtensions_SourceGenerator_LogFilePathMustBeUnique>
</PropertyGroup>
```

## [02]-[LOG_FILE_RESOLUTION]

Existing files supply their folder, name, and extension. Existing folders supply the folder, and the name defaults to `ThinktectureRuntimeExtensions_logs` with the extension `.log`. Every generator that names one path shares one sink: one compiler process writes one file, interleaves its generators, and appends to an existing file. The generator never creates folders, the folder must exist before the build.

- With `LogFilePathMustBeUnique` at `true`, the file name becomes `<name>_<yyyyMMdd>_<HHmmss>_<guid><extension>` from the UTC clock, and each compiler process names a new file
- With `false`, the file name is `<name><extension>`, and every process appends to one file
- The generator opens the file on the first message it writes, not when the compilation starts
- The level filters the generator's own entries, not build diagnostics
- The generator writes one `Warning` entry: `Code generator '<name>' didn't emit any code for '<namespace>.<type>'.`

## [03]-[LOG_LEVELS]

The enum order is `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`. Each line carries a local timestamp, the level, the generator name, and the message.

- Only `Information`, `Warning`, and `Error` create a file logger
- `Trace`, `Debug`, and `None` fall back to a logger that writes errors to the self-log alone
- At `Information` a clean build reports which serialization code generators participate

```text
[yyyy-MM-dd HH:mm:ss:fff | Information] [SmartEnumSourceGenerator] Code generator for MessagePack will participate in code generation
```

## [04]-[JETBRAINS_ANNOTATIONS]

Every generated `Switch` and `SwitchPartially` method marks each delegate parameter with `[global::JetBrains.Annotations.InstantHandleAttribute]`. The attribute tells a code analysis engine that the delegate runs only while the method is on the stack.

- The annotation generator adds one file, `Thinktecture.Annotations.g.cs`, with an `internal sealed class InstantHandleAttribute` in the namespace `JetBrains.Annotations`
- It skips the file when the compilation declares a class `JetBrains.Annotations.InstantHandleAttribute` in source
- It also skips the file when a metadata reference contains the module `JetBrains.Annotations.dll`, or when the property turns the generation off

The core runtime assembly carries its own internal copy of the attribute. Turning the generation off in a project without the `JetBrains.Annotations` package binds the generated `Switch` methods to that internal copy. Where the generator already skips the file, the property is redundant, everywhere else it fails the compilation with `CS0122` on every delegate parameter.

## [05]-[COUNTER]

With the counter on, every file a code generator emits starts with the line `// COUNTER: <n>`.

- The number `n` is a process-wide running number padded to eight characters
- The generated JetBrains attribute file carries no header
- The number increments once per code-generation attempt, before the code exists
- Numbers rising between two inspections show the generator ran again, and the highest number marks the last file of the process

## [06]-[DESIGN_RULES]

- Keep the generator properties out of the committed project file. Pass them with `-p:` for one diagnostic build, or keep them in a local, ignored props file.
- Set `LogLevel` to `Information` to see the generator run
- Point `LogFilePath` at a folder that exists before the build
- Set `LogFilePathMustBeUnique` to `false` to collect every compiler process in one file, keep the default `true` to read one process alone
- Delete the log file before a measurement
- Leave `GenerateJetBrainsAnnotations` unset
- Use `Counter` only to detect regeneration, and turn it off before generated files are compared or committed, because every run changes the header
-->
