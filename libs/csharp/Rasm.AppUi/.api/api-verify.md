# [RASM_APPUI_API_VERIFY]

`Verify.XunitV3` owns snapshot approval testing for xUnit v3: every `[Fact]`/`[Theory]` calls a `Verifier` static entry minting a `SettingsTask` whose awaited build compares against the committed `.verified.` file and writes `.received.` on mismatch. Scrubbers, named-value stabilization, uniqueness keys, and custom stream/string comparers carry from the transitive `Verify` core. On the AppUi proof rail it stacks onto the headless render lane: a `UseStreamComparer` byte snapshot proves a rendered dashboard, `VerifyJson` of a settled layout or command receipt proves structure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Verify.XunitV3`
- package: `Verify.XunitV3` (MIT)
- assembly: `Verify.XunitV3` (`net10.0`)
- namespace: `VerifyXunit`
- depends: `Verify` core (the `SettingsTask`/`VerifySettings`/`Target`/scrubber surface), `xunit.v3.extensibility.core`
- rail: test

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: approval-test surfaces in `VerifyXunit`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `Verifier`          | class         | xUnit v3 `Verify*`/`Throws*` entry; mints `SettingsTask`       |
|  [02]   | `VerifyBase`        | class         | abstract instance `Verify*` mirror via `[CallerFilePath]` ctor |
|  [03]   | `VerifyChecks`      | class         | `Run()` validates verify configuration, returns `Task`         |
|  [04]   | `DanglingSnapshots` | class         | `Run()` reports orphaned `.verified.` files, `void`            |
|  [05]   | `DerivePathInfo`    | delegate      | `(sourceFile, projectDir, type, method) -> PathInfo`           |

[CORE_SETTINGS_SCOPE]: transitive `Verify` core the `SettingsTask` exposes

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :--------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `SettingsTask`   | class         | fluent per-call settings; awaited build runs compare |
|  [02]   | `VerifySettings` | class         | scrubbers, uniqueness, directory, parameters         |
|  [03]   | `Target`         | struct        | named extension+data unit for multi-file verify      |
|  [04]   | `Combination`    | class         | cartesian-input combination snapshot                 |
|  [05]   | `VerifyResult`   | class         | written-file paths the awaited task returns          |

## [03]-[ENTRYPOINTS]

[VERIFY_ENTRYPOINTS]: object, value, and typed-source snapshot on `Verifier`

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                        |
| :-----: | :---------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Verify(object?, VerifySettings?, [CallerFilePath])`              | static  | object-graph snapshot               |
|  [02]   | `Verify<T>(Task<T> / ValueTask<T> / Func<Task<T>>, ...)`          | static  | async-target snapshot               |
|  [03]   | `Verify<T>(IAsyncEnumerable<T>, ...)`                             | static  | async-stream snapshot               |
|  [04]   | `Verify(string?, ...)` / `Verify(string?, extension, ...)`        | static  | string snapshot, extension-typed    |
|  [05]   | `Verify(Stream? / FileStream?, extension?, info?, ...)`           | static  | binary-stream snapshot              |
|  [06]   | `Verify(byte[]?, extension, info?, ...)`                          | static  | byte-array snapshot, visual hash    |
|  [07]   | `VerifyJson(string? / StringBuilder? / Stream? / Task<...>, ...)` | static  | JSON snapshot, pretty + scrubbed    |
|  [08]   | `VerifyXml(string? / Stream? / Task<...>, ...)`                   | static  | XML snapshot, formatted             |
|  [09]   | `VerifyTuple(Expression<Func<ITuple>>, ...)`                      | static  | named-member tuple snapshot         |
|  [10]   | `Verify(Target / IEnumerable<Target>, ...)`                       | static  | explicit-target / multi-file verify |
|  [11]   | `Verify(object?, IEnumerable<Target>, ...)`                       | static  | object + extra named targets        |

[FILE_AND_ARCHIVE_ENTRYPOINTS]: file, directory, and zip snapshot on `Verifier`

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `VerifyFile(string / FileInfo, extension?, info?, ...)`                      | static  | single-file snapshot     |
|  [02]   | `VerifyFiles(IEnumerable<string>, fileScrubber?, ...)`                       | static  | multi-file snapshot      |
|  [03]   | `VerifyDirectory(string / DirectoryInfo, include?, pattern?, options?, ...)` | static  | directory-tree snapshot  |
|  [04]   | `VerifyZip(string / Stream / byte[], include?, includeStructure?, ...)`      | static  | archive-content snapshot |
|  [05]   | `Verify(ZipArchive, include?, includeStructure?, persistArchive?, ...)`      | static  | open-archive snapshot    |

[EXCEPTION_ENTRYPOINTS]: throw verification on `Verifier`

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]              |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------ |
|  [01]   | `Throws(Action / Func<object?>, ...)`                        | static  | sync throw snapshot       |
|  [02]   | `ThrowsTask(Func<Task> / Func<Task<T>>, ...)`                | static  | async throw snapshot      |
|  [03]   | `ThrowsValueTask(Func<ValueTask> / Func<ValueTask<T>>, ...)` | static  | value-task throw snapshot |

[CONFIGURATION_ENTRYPOINTS]: process-wide path and attachment configuration on `Verifier`

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `DerivePathInfo(DerivePathInfo)`                                | static  | global snapshot-path resolver                  |
|  [02]   | `UseProjectRelativeDirectory(string)`                           | static  | project-root-relative snapshot dir             |
|  [03]   | `UseSourceFileRelativeDirectory(string)`                        | static  | source-file-relative snapshot dir              |
|  [04]   | `AddAttachmentEvents()`                                         | static  | route received/verified into xUnit attachments |
|  [05]   | `BuildVerifier(VerifySettings, sourceFile, useUniqueDirectory)` | factory | inner-verifier build, extension only           |

[SETTINGS_CHAIN]: fluent per-call settings off the returned `SettingsTask`

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `SettingsTask.UseStreamComparer(StreamCompare, extensions?)`                       | instance | custom byte comparison     |
|  [02]   | `SettingsTask.UseStringComparer(StringCompare, extensions?)`                       | instance | custom text comparison     |
|  [03]   | `SettingsTask.UniqueForRuntime` / `UniqueForTargetFramework[AndVersion]`           | instance | host-specific key          |
|  [04]   | `SettingsTask.UniqueForAssemblyConfiguration`                                      | instance | configuration-specific key |
|  [05]   | `SettingsTask.AddNamedGuid` / `AddNamedDateTime` / `AddNamedTime` / `AddNamedDate` | instance | named-value stabilization  |
|  [06]   | `SettingsTask.OnVerifyMismatch` / `OnFirstVerify` / `OnVerify`                     | instance | verification hooks         |
|  [07]   | `SettingsTask.AddExtraSettings(Action<JsonSerializerSettings>)`                    | instance | serializer configuration   |
|  [08]   | `SettingsTask.AppendValue` / `AppendValues`                                        | instance | named snapshot context     |
|  [09]   | `SettingsTask.DisableDiff`                                                         | instance | suppress diff launch       |
|  [10]   | `VerifySettings.UseDirectory` / `UseFileName`                                      | instance | snapshot placement         |
|  [11]   | `VerifySettings.UseParameters` / `UseTextForParameters`                            | instance | parameter key              |
|  [12]   | `VerifySettings.AddScrubber` / `ScrubLinesContaining`                              | instance | content scrubbing          |
|  [13]   | `VerifySettings.IgnoreMember`                                                      | instance | member omission            |
|  [14]   | `VerifySettings.AutoVerify`                                                        | instance | automatic acceptance       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `Verifier.Verify*`/`Throws*` returns a `SettingsTask`; the test threads the fluent settings inline, then `await`s it, and the awaited build compares against `.verified.` and writes `.received.` on mismatch — the `VerifyXunit` adapter and the transitive `Verify` core compose in that one expression.
- `VerifyBase` is the abstract instance mirror: a test class inherits it, its constructor captures `[CallerFilePath]` once, and every instance `Verify*` resolves placement from the class source file; the static `Verifier` entry is the default, the base class for fixtures holding shared settings.
- `DerivePathInfo`, `UseProjectRelativeDirectory`, and `UseSourceFileRelativeDirectory` redirect `.verified.` placement into a committed directory rather than beside transient build output.

[STACKING]:
- `SkiaSharp`(`.api/api-skiasharp.md`) and `Avalonia.Headless`(`.api/api-headless.md`): the headless render lane encodes an offscreen `SKImage` to a PNG `byte[]` (or the `RenderReceipt` frame bytes), which `Verify(bytes, "png", ...)` snapshots under a `UseStreamComparer` that hashes rather than byte-equals; `UniqueForTargetFramework()` partitions per host so a Skia raster drift does not cross-fail, sealing the byte-snapshot proof of a rendered dashboard.
- Structural proof folds `VerifyJson` onto a settled receipt — a `CommandDeck`, a `TableViewState`, or a frozen layout blob — with `ScrubLinesContaining`/`AddNamedGuid` stabilizing volatile ids and timestamps, so a layout regression reads as a one-line diff; `AddExtraSettings` tunes the object-graph serializer for types Verify does not format by default.
- `[CallerFilePath]` fills every `sourceFile` parameter so no test passes a literal path, and `Verify.XunitV3.props` initializes the attachment context at module load so `AddAttachmentEvents()` routes received/verified files into the xUnit v3 attachment stream; the `assay test --csharp` lane runs these as ordinary MTP tests, and a mismatch surfaces as a `Completed(FAILED)` result, not a rail fault.
- `await VerifyChecks.Run()` validates the verify configuration once and `DanglingSnapshots.Run()` reports orphaned `.verified.` files in a cleanup pass; neither is a per-test call.

[LOCAL_ADMISSION]:
- Snapshot files default to `[ClassName].[MethodName].verified.[ext]` beside the test source; `UseProjectRelativeDirectory`/`UseSourceFileRelativeDirectory`/`DerivePathInfo` redirect to a committed directory under CI.
- `Throws*` proves a failure shape only where an exception is the contract boundary; inside ROP code the receipt verifies via `VerifyJson` of the `Fin`/`Validation` failure value, never a thrown exception.
- `Combination` folds a cartesian input matrix into one snapshot, collapsing N near-identical `[Theory]` cases.

[RAIL_LAW]:
- Package: `Verify.XunitV3`
- Owns: snapshot approval testing for xUnit v3 — object, value, JSON/XML, file/directory/zip, byte/stream, and throw snapshots with the fluent scrub/uniqueness/comparer chain.
- Accept: `Verifier.Verify*`/`Throws*` or a `VerifyBase` subclass; the `SettingsTask`/`VerifySettings` chain for scrubbing, CI-stable uniqueness, and custom comparers; `VerifyChecks`/`DanglingSnapshots` as suite gates.
- Reject: constructing `InnerVerifier` outside `BuildVerifier`; a literal `sourceFile` bypassing `[CallerFilePath]`; a hand-rolled string-equality assertion when `VerifyJson` + scrubbers own the comparison; a parallel image-diff harness when `UseStreamComparer` carries the visual-hash compare.
