# [SOURCE_GENERATOR_CONFIGURATION]

The source generator reads project-level MSBuild properties. They control diagnostics logging, the generated JetBrains annotation, and a debugging counter. No property changes the signature of a generated member. `GenerateJetBrainsAnnotations` removes a generated type, and `Counter` changes the text of every emitted file. They apply to every generated type in the project: smart enums, value objects, and unions.

## [01]-[MSBUILD_PROPERTIES]

The `Thinktecture.Runtime.Extensions` package ships a props file that declares one `CompilerVisibleProperty` item per property. MSBuild forwards each item to the compiler as an analyzer config global option named `build_property.<PropertyName>`. A global analyzer configuration file sets the same option directly with a `build_property.` key and needs no MSBuild property. Several generators run in one compilation. Each reads the options and installs its own logger. A missing or unparsable value yields the default.

Every property name below carries the prefix `ThinktectureRuntimeExtensions_SourceGenerator_`.

| [INDEX] | [PROPERTY]                     | [VALUES]                                                                      | [DEFAULT] |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------- | :-------- |
|  [01]   | `LogFilePath`                  | a file or folder path, trimmed                                                | no log    |
|  [02]   | `LogFilePathMustBeUnique`      | `true` or `false`                                                             | `true`    |
|  [03]   | `LogLevel`                     | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`, case-insensitive | `Warning` |
|  [04]   | `LogMessageInitialBufferSize`  | an integer of at least 100                                                    | `100`     |
|  [05]   | `GenerateJetBrainsAnnotations` | `disable`, `disabled`, `false`, or `0` turn it off, case-insensitive          | on        |
|  [06]   | `Counter`                      | `enable`, `enabled`, `true`, or `1` turn it on, case-insensitive              | off       |

A blank `LogFilePath` disables file logging. `LogFilePath` gates the other logging properties. Without it the generator reads no log level, no uniqueness flag, and no buffer size. A buffer size below the minimum falls back to the minimum. `LogMessageInitialBufferSize` presizes the in-memory queue of pending messages inside one sink and changes no output. Every other string leaves the annotation on and the counter off.

The properties live in a `PropertyGroup` of the project file, or in a shared build props file for a whole tree. A `-p:` argument on the command line sets them for one build.

```xml
<PropertyGroup>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogFilePath>$(MSBuildProjectDirectory)/logs/generator.txt</ThinktectureRuntimeExtensions_SourceGenerator_LogFilePath>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogLevel>Information</ThinktectureRuntimeExtensions_SourceGenerator_LogLevel>
  <ThinktectureRuntimeExtensions_SourceGenerator_LogFilePathMustBeUnique>false</ThinktectureRuntimeExtensions_SourceGenerator_LogFilePathMustBeUnique>
</PropertyGroup>
```

## [02]-[LOG_FILE_RESOLUTION]

An existing file supplies its folder, name, and extension. An existing folder supplies only the folder, and the name defaults to `ThinktectureRuntimeExtensions_logs` with the extension `.log`. A missing file inside an existing folder supplies its folder, name, and extension. A path whose folder does not exist produces no log file, and the generator records one self-log line for that path.

With `LogFilePathMustBeUnique` at `true`, the file name becomes `<name>_<yyyyMMdd>_<HHmmss>_<guid><extension>` from the UTC clock, and each compiler process names a new file. With `false`, the file name is `<name><extension>`, and every process appends to one file. Every generator that names one path shares one sink, so one compiler process writes one file and interleaves its generators. The sink appends to an existing file, so one compiler process reuses one file across builds.

The generator opens the file on the first message it writes, not when the compilation starts. The level filters the generator's own entries, not build diagnostics. The generator writes one `Warning` entry, `Code generator '<name>' didn't emit any code for '<namespace>.<type>'.` At the default level `Warning`, a compilation that emits code for every type creates no file.

## [03]-[LOG_LEVELS]

The enum order is `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`. Only `Information`, `Warning`, and `Error` create a file logger. `Trace`, `Debug`, and `None` fall back to a logger that writes errors to the self-log alone. A file logger writes every message at or above its level.

Each line carries a local timestamp, the level, the generator name, and the message. At `Information` a clean build reports which serialization code generators participate. The list mirrors the referenced serialization packages, so a project without `Thinktecture.Runtime.Extensions.MessagePack` shows no MessagePack line. The value object, smart enum, and object factory generators each log the list, so every referenced serialization package appears once under each of those names.

```text
[yyyy-MM-dd HH:mm:ss:fff | Information] [SmartEnumSourceGenerator] Code generator for MessagePack will participate in code generation
```

## [04]-[SELF_LOG]

The generator keeps a second file named `ThinktectureRuntimeExtensionsSourceGenerator.log` in the temp folder of the process that hosts the compiler. It appends a line when the log folder does not exist, when the logger throws, and for every `Error` message regardless of the file logger. The compiler server keeps its own temp folder across builds. Setting `UseSharedCompilation` to `false` starts the compiler from the build, and the self-log follows the build's temp folder. The self-log stops writing after its first own failure.

## [05]-[JETBRAINS_ANNOTATIONS]

Every generated `Switch` and `SwitchPartially` method marks each delegate parameter with `[global::JetBrains.Annotations.InstantHandleAttribute]`. The attribute tells a code analysis engine that the delegate runs only while the method is on the stack. The generator wraps those methods in `#pragma warning disable CS0436` and `restore` because a project can hold its own copy of the attribute.

The annotation generator adds one file, `Thinktecture.Annotations.g.cs`, with an `internal sealed class InstantHandleAttribute` in the namespace `JetBrains.Annotations`. It skips the file when the compilation declares a class `JetBrains.Annotations.InstantHandleAttribute` in source. It also skips the file when a metadata reference contains the module `JetBrains.Annotations.dll`, or when the property turns the generation off.

The core runtime assembly carries its own internal copy of the attribute. Turning the generation off in a project without the `JetBrains.Annotations` package binds the generated `Switch` methods to that internal copy. The compilation then fails with `CS0122` on every delegate parameter. Where the generator already skips the file, the property is redundant, and everywhere else it breaks the build.

## [06]-[COUNTER]

With the counter on, every file a code generator emits starts with the line `// COUNTER: <n>`. The number `n` is a process-wide running number padded to eight characters. The generated JetBrains attribute file carries no header. The number increments once per code-generation attempt, before the code exists, so a generator that emits nothing consumes a number. A number that rises between two inspections shows that the generator ran again, and the highest number marks the last file of the process. It serves diagnostics only.

## [07]-[DESIGN_RULES]

- Keep the generator properties out of the committed project file. Pass them with `-p:` for a single diagnostic build, or keep them in a local, ignored props file.
- Set `LogLevel` to `Information` when the goal is to see the generator run.
- Point `LogFilePath` at a folder that exists before the build. The generator never creates folders.
- Set `LogFilePathMustBeUnique` to `false` to collect every compiler process in one file. Keep the default `true` to read one process alone.
- Delete the log file before a measurement.
- Leave `GenerateJetBrainsAnnotations` unset. Turning it off without an accessible attribute fails the compilation with `CS0122` on every delegate parameter.
- Use `Counter` only to detect regeneration. Turn it off before the generated files are compared or committed, because every run changes the header.
