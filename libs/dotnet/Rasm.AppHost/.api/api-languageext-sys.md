# [RASM_API_LANGUAGEEXT_SYS]

`LanguageExt.Sys` lifts the BCL's ambient effects — console, clock, file system, directory, environment, encoding, text reader, and activity source — onto trait-constrained effect modules that read their capability from the runtime instead of the process. `Live.Runtime` binds the real BCL implementations and `Test.Runtime` binds deterministic doubles over a temp-directory sandbox, so one expression runs against either without a rewrite.

## [01]-[RUNTIME]

[RUNTIME_TYPE_SCOPE]: `LanguageExt.Sys.Live` and `LanguageExt.Sys.Test` runtime records, their environment carriers, and the `LanguageExt` activity carrier both read

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :----------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `Live.Runtime`     | record        | binds every capability to its BCL implementation       |
|  [02]   | `Live.RuntimeEnv`  | record        | disposable activity carrier the live runtime reads     |
|  [03]   | `Test.Runtime`     | record        | binds every capability to a deterministic double       |
|  [04]   | `Test.RuntimeEnv`  | record        | sandbox root, memory console, clock spec, env snapshot |
|  [05]   | `ActivityEnv`      | record        | `ActivitySource`, ambient `Activity`, parent id        |
|  [06]   | `Traits.HasSys<M>` | interface     | one constraint composing the core capability set       |

[RUNTIME_ENTRY_SCOPE]: construction, scoping, and disposal; both runtimes fix `Eff<Runtime>` as their `M` and answer `Has<Eff<Runtime>, T>` per capability trait

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------ | :------- | :--------------------------------------------- |
|  [01]   | `Live.Runtime.New() -> Runtime`             | static   | mint the live runtime on `ActivityEnv.Default` |
|  [02]   | `Test.Runtime.New() -> Runtime`             | static   | mint over a fresh temp-directory sandbox       |
|  [03]   | `Test.Runtime.New(RuntimeEnv) -> Runtime`   | static   | mint over a caller-built environment           |
|  [04]   | `Test.RuntimeEnv.New(string) -> RuntimeEnv` | static   | seed console, clock, and env at a root path    |
|  [05]   | `Test.RuntimeEnv.LocalCancel`               | property | re-seat `EnvIO` on a child cancellation source |
|  [06]   | `Test.Runtime.Dispose()`                    | instance | delete the sandbox root recursively            |
|  [07]   | `Live.RuntimeEnv.Dispose()`                 | instance | dispose the activity environment               |
|  [08]   | `ActivityEnv.Default`                       | static   | source named for the entry assembly            |
|  [09]   | `ActivityEnv.Dispose()`                     | instance | dispose source and ambient activity            |

[HAS_SYS]: `ActivitySourceIO` `ConsoleIO` `FileIO` `DirectoryIO` `TextReadIO` `TimeIO`

- `Test.Runtime.New()`: mints a GUID-named directory under the system temp path, and `Dispose` deletes that root whole.
- `Test.Runtime`: its `EncodingIO` ask answers `Live.Implementations.EncodingIO.Default`, so `RuntimeEnv.Encoding` reaches no file text operation.
- `Live.Runtime`: every capability ask outside `ActivitySourceIO` and `ActivityEnv` resolves once at static initialization.
- `Live.Runtime` and `Test.Runtime`: both implement `Local<Eff<Runtime>, ActivityEnv>`, the constraint span scoping consumes.

## [02]-[CONSOLE]

[CONSOLE_TYPE_SCOPE]: console capability, its two module arities, and the in-memory screen the test double drives

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `Traits.ConsoleIO`               | interface     | console capability contract on `IO<A>`               |
|  [02]   | `Console<M, RT>`                 | static class  | trait-generic console module                         |
|  [03]   | `Console<RT>`                    | static class  | `Eff<RT, A>` specialisation                          |
|  [04]   | `MemoryConsole`                  | class         | screen stack and keyboard queue, enumerable as lines |
|  [05]   | `Live.Implementations.ConsoleIO` | record        | `System.Console` binding                             |
|  [06]   | `Test.Implementations.ConsoleIO` | record        | `MemoryConsole` binding                              |

[CONSOLE_ENTRY_SCOPE]: `Console<M, RT>` under `M : MonadIO<M>, Fallible<Error, M>` and `RT : Has<M, ConsoleIO>`; `Console<RT>` mirrors each member as `Eff<RT, A>`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Console.readKey -> K<M, ConsoleKeyInfo>`                | static   | one key press                     |
|  [02]   | `Console.readKeys -> ProducerT<ConsoleKeyInfo, M, Unit>` | static   | key stream                        |
|  [03]   | `Console.read -> K<M, int>`                              | static   | one character code                |
|  [04]   | `Console.reads -> ProducerT<int, M, Unit>`               | static   | character-code stream             |
|  [05]   | `Console.readLine -> K<M, string>`                       | static   | one line                          |
|  [06]   | `Console.readLines -> ProducerT<string, M, Unit>`        | static   | line stream                       |
|  [07]   | `Console.write(string)`                                  | static   | append to the current line        |
|  [08]   | `Console.write(char)`                                    | static   | append one character              |
|  [09]   | `Console.writeLine(string)`                              | static   | write a whole line                |
|  [10]   | `Console.writeLine2(string) -> K<M, string>`             | static   | write a line and return it        |
|  [11]   | `Console.writeEmptyLine`                                 | static   | break the current line            |
|  [12]   | `Console.clear`                                          | static   | clear the screen                  |
|  [13]   | `Console.colour -> K<M, ConsoleColor>`                   | static   | read the foreground colour        |
|  [14]   | `Console.bgColour -> K<M, ConsoleColor>`                 | static   | read the background colour        |
|  [15]   | `Console.setColour(ConsoleColor)`                        | static   | set the foreground colour         |
|  [16]   | `Console.setBgColour(ConsoleColor)`                      | static   | set the background colour         |
|  [17]   | `Console.resetColour()`                                  | static   | restore both colours              |
|  [18]   | `MemoryConsole()`                                        | ctor     | mint an empty screen              |
|  [19]   | `MemoryConsole.WriteKey(ConsoleKeyInfo)`                 | instance | queue one key                     |
|  [20]   | `MemoryConsole.WriteKeyChar(char)`                       | instance | queue one character as a key      |
|  [21]   | `MemoryConsole.WriteKeyString(string)`                   | instance | queue every character of a string |
|  [22]   | `MemoryConsole.WriteKeyLine(string)`                     | instance | queue a string and a newline key  |
|  [23]   | `MemoryConsole.Commit()`                                 | instance | cycle the keyboard queue          |
|  [24]   | `MemoryConsole.GetEnumerator()`                          | instance | screen lines in write order       |

[CONSOLE_IO]: `BgColor` `Color` `Clear()` `ReadKey()` `Read()` `ReadLine()` `WriteLine()` `WriteLine(string)` `Write(string)` `SetBgColor` `SetColor` `ResetColor`

- `Console.readKey`, `Console.read`, `Console.readLine`: fail with `Errors.EndOfStream` wherever the trait member answers `None`, which is why this module alone carries the `Fallible<Error, M>` constraint.
- `MemoryConsole.Commit`: re-enqueues every key it dequeues, so the keyboard queue never drains.
- `Console<RT>`: spells the foreground read `color` and exposes `resetColour` as a property, where the trait-generic module spells `colour` and `resetColour()`.

## [03]-[FILE]

[FILE_TYPE_SCOPE]: file capability, its two module arities, and the two bindings

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :---------------------------- | :------------ | :---------------------------------- |
|  [01]   | `Traits.FileIO`               | interface     | file capability contract on `IO<A>` |
|  [02]   | `File<M, RT>`                 | class         | trait-generic file module           |
|  [03]   | `File<RT>`                    | class         | `Eff<RT, A>` specialisation         |
|  [04]   | `Live.Implementations.FileIO` | record        | `System.IO.File` binding            |
|  [05]   | `Test.Implementations.FileIO` | record        | root-rewritten sandbox binding      |

[FILE_ENTRY_SCOPE]: `File<M, RT>` under `RT : Has<M, FileIO>, Has<M, EncodingIO>`; every text member resolves its `Encoding` through `Enc<M, RT>.encoding`

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `File.copy(string, string, bool)`                                       | static  | copy with an overwrite flag    |
|  [02]   | `File.move(string, string, bool)`                                       | static  | move, overwrite flag optional  |
|  [03]   | `File.delete(string)`                                                   | static  | delete one path                |
|  [04]   | `File.exists(string) -> K<M, bool>`                                     | static  | test one path                  |
|  [05]   | `File.readAllText(string) -> K<M, string>`                              | static  | whole-file text read           |
|  [06]   | `File.readAllLines(string) -> K<M, Seq<string>>`                        | static  | whole-file line read           |
|  [07]   | `File.readAllBytes(string) -> K<M, byte[]>`                             | static  | whole-file byte read           |
|  [08]   | `File.writeAllText(string, string)`                                     | static  | replace file text              |
|  [09]   | `File.writeAllLines(string, Seq<string>)`                               | static  | replace file lines             |
|  [10]   | `File.writeAllBytes(string, byte[])`                                    | static  | replace file bytes             |
|  [11]   | `File.appendAllLines(string, IEnumerable<string>)`                      | static  | append lines                   |
|  [12]   | `File.createText(string) -> K<M, TextWriter>`                           | static  | writer over a truncated file   |
|  [13]   | `File.appendText(string) -> K<M, TextWriter>`                           | static  | writer positioned at the end   |
|  [14]   | `File.openText(string) -> ProducerT<TextReader, M, Unit>`               | static  | scoped reader handle           |
|  [15]   | `File.openRead(string) -> ProducerT<Stream, M, Unit>`                   | static  | scoped read stream             |
|  [16]   | `File.open(string, FileMode, FileAccess) -> ProducerT<Stream, M, Unit>` | static  | scoped stream, access optional |
|  [17]   | `File.openWrite(string) -> ProducerT<Stream, M, Unit>`                  | static  | scoped write stream            |

[FILE_IO]: `Copy` `Move` `AppendAllLines` `ReadAllLines` `ReadAllText` `ReadAllBytes` `WriteAllText` `WriteAllLines` `WriteAllBytes` `Delete` `Exists` `OpenText` `CreateText` `AppendText` `OpenRead` `Open` `OpenWrite`

- `Traits.FileIO`: every text member takes a trailing `Encoding` the module supplies, and `OpenText`, `CreateText`, `AppendText`, `OpenRead`, `Open`, and `OpenWrite` hand back raw BCL handles.
- `File.openText`, `File.openRead`, `File.open`, `File.openWrite`: acquire through `Prelude.use` and yield exactly one handle, so the downstream consumer runs inside the disposal scope.
- `File.createText`, `File.appendText`: hand back an undisposed `TextWriter`, so the caller owns its release.
- `File<RT>`: `move` lands on the trait-generic `File<M, RT>` alone, and the stream members mirror as `Producer<RT, …, Unit>`.
- `Test.Implementations.FileIO`: rewrites each path under `root` with `:` replaced by `_drive`, and `Move` passes both paths through unrewritten.

## [04]-[DIRECTORY]

[DIRECTORY_TYPE_SCOPE]: directory capability, its two module arities, and the two bindings

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Traits.DirectoryIO`               | interface     | directory capability contract on `IO<A>` |
|  [02]   | `Directory<M, RT>`                 | class         | trait-generic directory module           |
|  [03]   | `Directory<RT>`                    | class         | `Eff<RT, A>` specialisation              |
|  [04]   | `Live.Implementations.DirectoryIO` | record        | `System.IO.Directory` binding            |
|  [05]   | `Test.Implementations.DirectoryIO` | record        | root-rewritten sandbox binding           |

[DIRECTORY_ENTRY_SCOPE]: `Directory<M, RT>` under `RT : Has<M, DirectoryIO>`; `Directory<RT>` mirrors each member as `Eff<RT, A>`

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Directory.create(string) -> K<M, DirectoryInfo>`                    | static  | create one directory                |
|  [02]   | `Directory.delete(string, bool)`                                     | static  | delete, recursive by default        |
|  [03]   | `Directory.exists(string) -> K<M, bool>`                             | static  | test one path                       |
|  [04]   | `Directory.move(string, string)`                                     | static  | move a directory                    |
|  [05]   | `Directory.getParent(string) -> K<M, Option<DirectoryInfo>>`         | static  | parent as an option                 |
|  [06]   | `Directory.getRoot(string) -> K<M, string>`                          | static  | volume root of a path               |
|  [07]   | `Directory.current -> K<M, string>`                                  | static  | process working directory           |
|  [08]   | `Directory.setCurrent(string)`                                       | static  | set the working directory           |
|  [09]   | `Directory.logicalDrives -> K<M, Seq<string>>`                       | static  | mounted drive roster                |
|  [10]   | `Directory.enumerateDirectories(string, string, SearchOption)`       | static  | child directories, filters optional |
|  [11]   | `Directory.enumerateFiles(string, string, SearchOption)`             | static  | child files, filters optional       |
|  [12]   | `Directory.enumerateFileSystemEntries(string, string, SearchOption)` | static  | every child entry, filters optional |

[TIMESTAMP_READS]: `(string) -> K<M, DateTime>` — `getCreationTime` `getCreationTimeUtc` `getLastWriteTime` `getLastWriteTimeUtc` `getLastAccessTime` `getLastAccessTimeUtc`

[TIMESTAMP_WRITES]: `(string, DateTime) -> K<M, Unit>` — `setCreationTime` `setCreationTimeUtc` `setLastWriteTime` `setLastWriteTimeUtc` `setLastAccessTime` `setLastAccessTimeUtc`

- `Directory.enumerateDirectories`, `Directory.enumerateFiles`, `Directory.enumerateFileSystemEntries`: each realises its whole result into a `Seq<string>` before the effect completes.
- `Traits.DirectoryIO`: mirrors the module in `PascalCase` and spells the three positional members `GetCurrentDirectory`, `SetCurrentDirectory`, and `GetDirectoryRoot`.
- `Test.Implementations.DirectoryIO`: rewrites each path under `root` with `:` replaced by `_drive`, and `GetCurrentDirectory` answers the live process directory.

## [05]-[TEXT_STREAM]

[TEXT_STREAM_TYPE_SCOPE]: text-reader capability, the two pipe modules over `TextReader` and `Stream`, and the two bindings

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `Traits.TextReadIO`               | interface     | text-reader capability contract on `IO<A>` |
|  [02]   | `TextRead<M, RT>`                 | static class  | trait-generic reader pipes                 |
|  [03]   | `TextRead<RT>`                    | static class  | `Pipe<RT, …>` specialisation               |
|  [04]   | `Stream<M>`                       | static class  | chunked byte pipe, runtime-free            |
|  [05]   | `Live.Implementations.TextReadIO` | record        | async `TextReader` binding                 |
|  [06]   | `Test.Implementations.TextReadIO` | struct        | same async binding, value-shaped           |

[TEXT_STREAM_ENTRY_SCOPE]: `TextRead<M, RT>` under `RT : Has<M, TextReadIO>` and `Stream<M>` under `M : MonadIO<M>` alone; each pipe awaits its handle upstream and yields decoded units downstream

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                  |
| :-----: | :--------------------------------------------------------------------- | :------ | :---------------------------- |
|  [01]   | `TextRead.readLine -> PipeT<TextReader, string, M, Unit>`              | static  | line-at-a-time stream         |
|  [02]   | `TextRead.readChar -> PipeT<TextReader, char, M, Unit>`                | static  | character-at-a-time stream    |
|  [03]   | `TextRead.readToEnd -> PipeT<TextReader, string, M, Unit>`             | static  | one yield of the whole reader |
|  [04]   | `TextRead.readChars(int) -> PipeT<TextReader, SeqLoan<char>, M, Unit>` | static  | pooled character chunks       |
|  [05]   | `TextRead.read(int) -> PipeT<TextReader, string, M, Unit>`             | static  | fixed-width string chunks     |
|  [06]   | `TextRead.close(TextReader) -> K<M, Unit>`                             | static  | close through the capability  |
|  [07]   | `Stream.read(int) -> PipeT<Stream, SeqLoan<byte>, M, Unit>`            | static  | pooled byte chunks            |

[TEXT_READ_IO]: `ReadLine(TextReader)` `ReadToEnd(TextReader)` `Read(TextReader, Memory<char>)` `Close(TextReader)`

- `TextRead.readLine`, `readChar`, `readToEnd`, `readChars`, `read`: drive the awaited `TextReader` through its own async BCL members, and `close` is the one member routing through `TextReadIO`.
- `Stream.read`: rents from `ArrayPool<byte>.Shared` and yields a `SeqLoan<byte>` owning the rented buffer, so a chunk held past its yield reads returned memory.
- `TextRead.readChars`: rents a fresh `ArrayPool<char>` buffer per iteration and breaks on a negative read count, which the BCL never returns.
- `TextRead.read`: loops while the read count is at or above zero and yields the whole rented buffer, so a chunk carries the pool's slack past the decoded prefix.
- `Test.Implementations.TextReadIO`: forwards every member to `Live.Implementations.TextReadIO.Default`, and both expose a `Default` static field.

## [06]-[TIME]

[TIME_TYPE_SCOPE]: clock capability, its two module arities, the schedule-driven test clock, and its specification record

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [CAPABILITY]                           |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------- |
|  [01]   | `Traits.TimeIO`                     | interface       | clock capability contract on `IO<A>`   |
|  [02]   | `Time<M, RT>`                       | static class    | trait-generic clock module             |
|  [03]   | `Time<RT>`                          | static class    | `Eff<RT, A>` specialisation            |
|  [04]   | `Test.Implementations.TestTimeSpec` | record          | `Schedule` and start instant pair      |
|  [05]   | `Live.Implementations.TimeIO`       | readonly struct | `DateTime` and `Task.Delay` binding    |
|  [06]   | `Test.Implementations.TimeIO`       | class           | schedule-advanced clock over an `Atom` |

[TIME_ENTRY_SCOPE]: `Time<M, RT>` under `RT : Has<M, TimeIO>` beside the test clock's own construction; `Time<RT>` mirrors each member as `Eff<RT, A>`

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Time.now -> K<M, DateTime>`                  | static   | local instant                     |
|  [02]   | `Time.nowUTC -> K<M, DateTime>`               | static   | UTC instant                       |
|  [03]   | `Time.today -> K<M, DateTime>`                | static   | local date at midnight            |
|  [04]   | `Time.sleepUntil(DateTime)`                   | static   | wait to an absolute instant       |
|  [05]   | `Time.sleepFor(TimeSpan)`                     | static   | wait a relative span              |
|  [06]   | `TestTimeSpec.FixedFromSpecified(DateTime)`   | static   | frozen clock at a given start     |
|  [07]   | `TestTimeSpec.FixedFromNow(DateTime)`         | static   | frozen clock at process now       |
|  [08]   | `TestTimeSpec.RunningFromSpecified(DateTime)` | static   | one-millisecond tick from a start |
|  [09]   | `TestTimeSpec.RunningFromNow()`               | static   | one-millisecond tick from now     |
|  [10]   | `Test.Implementations.TimeIO(TestTimeSpec)`   | ctor     | seat the clock on a specification |
|  [11]   | `Test.Implementations.TimeIO.Dispose()`       | instance | release the schedule enumerator   |

[TIME_IO]: `Now` `UtcNow` `Today` `SleepUntil(DateTime)` `SleepFor(TimeSpan)`

- `Test.Implementations.TimeIO`: every `Now`, `UtcNow`, or `Today` read advances the clock by the next `Schedule` duration and throws `TimeoutException` once that schedule runs out.
- `Test.Implementations.TimeIO.SleepUntil`, `SleepFor`: delegate to the live implementation, so a scripted clock still sleeps in real time.
- `TestTimeSpec.FixedFromNow`: discards its `DateTime` argument and seats `DateTime.UtcNow`.
- `TestTimeSpec.FixedFromSpecified`: seats `Schedule.Forever` for a zero advance per read, where `RunningFromSpecified` seats `Schedule.spaced` at one millisecond.

## [07]-[ENVIRONMENT]

[ENVIRONMENT_TYPE_SCOPE]: process-environment and encoding capabilities, their module arities, and the mutable snapshot the test double drives

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `Traits.EnvironmentIO`               | interface     | process-environment contract on `IO<A>`     |
|  [02]   | `Traits.EncodingIO`                  | interface     | single-member encoding contract             |
|  [03]   | `Environment<M, RT>`                 | static class  | trait-generic environment module            |
|  [04]   | `Environment<RT>`                    | static class  | `Eff<RT, A>` specialisation                 |
|  [05]   | `Enc<M, RT>`                         | static class  | trait-generic encoding module               |
|  [06]   | `Enc<RT>`                            | static class  | `Eff<RT, A>` specialisation                 |
|  [07]   | `MemorySystemEnvironment`            | class         | mutable snapshot of every environment slot  |
|  [08]   | `Live.Implementations.EnvironmentIO` | record        | `System.Environment` binding                |
|  [09]   | `Live.Implementations.EncodingIO`    | class         | `Encoding.Default` binding                  |
|  [10]   | `Test.Implementations.EnvironmentIO` | record        | snapshot binding over three variable scopes |

[ENVIRONMENT_ENTRY_SCOPE]: `Environment<M, RT>` under `RT : Has<M, EnvironmentIO>` and `Enc<M, RT>` under `RT : Has<M, EncodingIO>`; both mirror as `Eff<RT, A>` on the single-parameter form

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Enc.encoding -> K<M, Encoding>`                                                        | static   | encoding every text member takes |
|  [02]   | `Environment.getEnvironmentVariable(string, EnvironmentVariableTarget)`                 | static   | one variable, target optional    |
|  [03]   | `Environment.setEnvironmentVariable(string, Option<string>, EnvironmentVariableTarget)` | static   | write or clear, target optional  |
|  [04]   | `Environment.getEnvironmentVariables(EnvironmentVariableTarget)`                        | static   | one target's whole map           |
|  [05]   | `Environment.expandEnvironmentVariables(string) -> K<M, string>`                        | static   | substitute variable tokens       |
|  [06]   | `Environment.getFolderPath(SpecialFolder, SpecialFolderOption)`                         | static   | known folder, option optional    |
|  [07]   | `Environment.exit(int)`                                                                 | static   | end the process                  |
|  [08]   | `Environment.setExitCode(int)`                                                          | static   | stage the exit code              |
|  [09]   | `Environment.failFast(Option<string>, Option<Exception>)`                               | static   | abort, exception optional        |
|  [10]   | `MemorySystemEnvironment.InitFromSystem()`                                              | static   | snapshot the live process        |
|  [11]   | `MemorySystemEnvironment.With(…)`                                                       | instance | copy with any slot replaced      |

[ENVIRONMENT_READS]: `K<M, …>` properties — `commandLine` `commandLineArgs` `currentManagedThreadId` `exitCode` `environmentVariables` `logicalDrives` `hasShutdownStarted` `is64BitOperatingSystem` `is64BitProcess` `machineName` `newLine` `osVersion` `processorCount` `stackTrace` `systemDirectory` `systemPageSize` `tickCount` `userDomainName` `userInteractive` `userName` `version` `workingSet`

- `MemorySystemEnvironment`: carries `ProcessEnvironmentVariables`, `UserEnvironmentVariables`, and `SystemEnvironmentVariables` as public `ConcurrentDictionary` fields a test mutates in place, beside settable `ExitCode` and `HasShutdownStarted`.
- `MemorySystemEnvironment.With`: takes one optional named argument per slot and reads a null argument as keep-the-current-value.
- `Test.Implementations.EnvironmentIO.Exit`, `FailFast`: call the real `System.Environment` members, so either one ends the host process.
- `Test.Implementations.EnvironmentIO.ExpandEnvironmentVariables`: throws `NotImplementedException` on execution.
- `Live.Implementations.EnvironmentIO.SetCurrentDirectory(string)`: rides the record past the `EnvironmentIO` contract, which seats directory navigation on `DirectoryIO`.
- `Live.Implementations.EncodingIO.Default`: answers `Encoding.Default` from an assignable static field.

## [08]-[TRACING]

[TRACING_TYPE_SCOPE]: activity-source capability, the diagnostic module arities, and the live binding over an `ActivityEnv`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `Traits.ActivitySourceIO`               | interface     | span-start contract on `IO<Activity?>` |
|  [02]   | `Activity<M, RT>`                       | class         | trait-generic tracing module           |
|  [03]   | `Activity<RT>`                          | class         | `Eff<RT, A>` specialisation            |
|  [04]   | `Live.Implementations.ActivitySourceIO` | record        | `ActivitySource.StartActivity` binding |

[TRACING_ENTRY_SCOPE]: `Activity<M, RT>` under `RT : Has<M, ActivitySourceIO>, Local<M, ActivityEnv>`; `Activity<RT>` mirrors every member except `currentActivity`, which stays trait-generic

| [INDEX] | [SURFACE]                                                                                      | [SHAPE] | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------------------------------- | :------ | :--------------------------- |
|  [01]   | `Activity.span<A>(string, K<M, A>)`                                                            | static  | wrap an operation in a span  |
|  [02]   | `Activity.startActivity(string, ActivityKind, HashMap, Seq, DateTimeOffset, ActivityContext?)` | static  | start one span               |
|  [03]   | `Activity.currentActivity -> K<M, Activity?>`                                                  | static  | ambient span                 |
|  [04]   | `Activity.addTag(string, string?)`                                                             | static  | tag the ambient span         |
|  [05]   | `Activity.addTag(string, object?)`                                                             | static  | tag with a boxed value       |
|  [06]   | `Activity.addBaggage(string, string?)`                                                         | static  | carry a propagated pair      |
|  [07]   | `Activity.addEvent(ActivityEvent)`                                                             | static  | record one span event        |
|  [08]   | `Activity.setTraceState(string)`                                                               | static  | write the trace-state header |

[ACTIVITY_READS]: `K<M, …>` properties on the ambient span — `traceState` `traceId` `baggage` `tags` `tagObjects` `context` `duration` `events` `id` `kind` `links` `current` `parentId` `parentSpanId` `recorded` `displayName` `operationName` `rootId` `spanId` `startTimeUTC`

[ACTIVITY_SOURCE_IO]: `StartActivity(string, ActivityKind)` `StartActivity(string, ActivityKind, ActivityContext, HashMap, Seq, DateTimeOffset)` `StartActivity(string, ActivityKind, string, HashMap, Seq, DateTimeOffset)`

- `Activity.span`: five overloads narrow to one — `(string, ActivityKind, ActivityContext, HashMap<string, object>, Seq<ActivityLink>, DateTimeOffset, K<M, A>)` — each shorter form filling `ActivityKind.Internal`, empty tags and links, and `DateTimeOffset.Now`.
- `Activity.span`: scopes the started span as ambient through `Local<M, ActivityEnv>.With` for the wrapped operation and releases it through `Prelude.use`.
- `Activity.startActivity`: throws `NullReferenceException` where the source answers null, which is what an unlistened `ActivitySource` returns.
- `Activity.startActivity`: reads the ambient span's `Context` as the parent wherever the caller passes no `ActivityContext`.
- `ACTIVITY_READS`: each answers `Option<…>` or an empty collection where no span is ambient, and `currentActivity` alone answers the nullable `Activity` directly.
