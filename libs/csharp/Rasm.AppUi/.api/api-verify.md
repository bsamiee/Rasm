# [RASM_APPUI_API_VERIFY]

`Verify.XunitV3` supplies snapshot approval testing for xUnit v3: `Verifier` is the static `Verify*`/`Throws*` entry that every `[Fact]`/`[Theory]` calls, `VerifyBase` is the instance base for test classes, and `VerifyChecks`/`DanglingSnapshots` are the configuration and stale-file gates. Every `Verify*` returns a `SettingsTask` whose awaited build runs the comparison and writes `.received.`/`.verified.` files; the fluent `SettingsTask`/`VerifySettings` chain (scrubbers, named-value stabilization, uniqueness keys, custom stream/string comparers) carries from the core `Verify` package. The AppUi proof rail composes this onto the headless render lane: an `SKImage`/`RenderReceipt` byte snapshot under a `UseStreamComparer` byte-diff plus a `UniqueForTargetFramework` key is the per-named-dashboard visual proof, and a `VerifyJson` of a settled command-deck or layout receipt is the structural proof.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Verify.XunitV3`
- package: `Verify.XunitV3` (version `31.20.0`, MIT)
- assembly: `Verify.XunitV3` (TFM `net10.0`; the package also ships `net11.0`, the `net10.0` asset binds the workspace floor)
- transitive: `Verify` core (the `SettingsTask`/`VerifySettings`/`Target`/scrubber surface) and `xunit.v3.extensibility.core`
- namespace: `VerifyXunit`
- asset: runtime library + `Verify.XunitV3.props` (module-init wiring of the xUnit v3 attachment context)
- rail: test

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: approval-test surfaces — 5 types in `VerifyXunit`
- rail: test

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                |
| :-----: | :------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `Verifier`          | static entry    | xUnit v3 `Verify*`/`Throws*` entry; mints `SettingsTask` |
|  [02]   | `VerifyBase`        | test base class | instance `Verify*` mirror via `[CallerFilePath]` ctor   |
|  [03]   | `VerifyChecks`      | self-check gate | `Run()` validates verify configuration (async)       |
|  [04]   | `DanglingSnapshots` | stale-file gate | `Run()` reports orphaned `.verified.` files          |
|  [05]   | `DerivePathInfo`    | path delegate   | `(sourceFile, projectDir, type, method) -> PathInfo`  |

[CORE_SETTINGS_SCOPE]: transitive `Verify` fluent surface the `SettingsTask` exposes
- rail: test

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :--------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `SettingsTask`   | awaitable builder | fluent per-call settings; awaited build runs compare |
|  [02]   | `VerifySettings` | settings record   | scrubbers, uniqueness, directory, parameters    |
|  [03]   | `Target`         | snapshot target   | named extension+data unit for multi-file verify  |
|  [04]   | `Combination`    | matrix runner     | cartesian-input combination snapshot            |
|  [05]   | `VerifyResult`   | result receipt    | written-file paths the awaited task returns      |

## [03]-[ENTRYPOINTS]

[VERIFY_ENTRYPOINTS]: object, value, and typed-source snapshot verify on `Verifier`
- rail: test

| [INDEX] | [SURFACE]                                                            | [SURFACE_ROOT] | [RAIL]                              |
| :-----: | :------------------------------------------------------------------ | :------------- | :---------------------------------- |
|  [01]   | `Verify(object?, VerifySettings?, [CallerFilePath])`                | `Verifier`     | object-graph snapshot               |
|  [02]   | `Verify<T>(Task<T> / ValueTask<T> / Func<Task<T>>, ...)`            | `Verifier`     | async-target snapshot               |
|  [03]   | `Verify<T>(IAsyncEnumerable<T>, ...)`                               | `Verifier`     | async-stream snapshot               |
|  [04]   | `Verify(string?, ...)` / `Verify(string?, extension, ...)`         | `Verifier`     | string snapshot, extension-typed    |
|  [05]   | `Verify(Stream? / FileStream?, extension?, info?, ...)`            | `Verifier`     | binary-stream snapshot              |
|  [06]   | `Verify(byte[]?, extension, info?, ...)`                            | `Verifier`     | byte-array snapshot (visual hash)   |
|  [07]   | `VerifyJson(string? / StringBuilder? / Stream? / Task<...>, ...)`  | `Verifier`     | JSON snapshot, pretty + scrubbed    |
|  [08]   | `VerifyXml(string? / Stream? / Task<...>, ...)`                    | `Verifier`     | XML snapshot, formatted             |
|  [09]   | `VerifyTuple(Expression<Func<ITuple>>, ...)`                       | `Verifier`     | named-member tuple snapshot         |
|  [10]   | `Verify(Target / IEnumerable<Target>, ...)`                        | `Verifier`     | explicit-target / multi-file verify |
|  [11]   | `Verify(object?, IEnumerable<Target>, ...)`                        | `Verifier`     | object + extra named targets        |

[FILE_AND_ARCHIVE_ENTRYPOINTS]: file, directory, and zip snapshot verify on `Verifier`
- rail: test

| [INDEX] | [SURFACE]                                                                   | [SURFACE_ROOT] | [RAIL]                       |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `VerifyFile(string / FileInfo, extension?, info?, ...)`                     | `Verifier`     | single-file snapshot         |
|  [02]   | `VerifyFiles(IEnumerable<string>, fileScrubber?, ...)`                      | `Verifier`     | multi-file snapshot          |
|  [03]   | `VerifyDirectory(string / DirectoryInfo, include?, pattern?, options?, ...)`| `Verifier`     | directory-tree snapshot      |
|  [04]   | `VerifyZip(string / Stream / byte[], include?, includeStructure?, ...)`     | `Verifier`     | archive-content snapshot     |
|  [05]   | `Verify(ZipArchive, include?, includeStructure?, persistArchive?, ...)`     | `Verifier`     | open-archive snapshot        |

[EXCEPTION_ENTRYPOINTS]: throw verification on `Verifier`
- rail: test

| [INDEX] | [SURFACE]                                          | [SURFACE_ROOT] | [RAIL]                          |
| :-----: | :------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Throws(Action / Func<object?>, ...)`              | `Verifier`     | sync throw snapshot             |
|  [02]   | `ThrowsTask(Func<Task> / Func<Task<T>>, ...)`      | `Verifier`     | async throw snapshot            |
|  [03]   | `ThrowsValueTask(Func<ValueTask> / Func<ValueTask<T>>, ...)` | `Verifier` | value-task throw snapshot   |

[CONFIGURATION_ENTRYPOINTS]: process-wide path and attachment configuration on `Verifier`
- rail: test

| [INDEX] | [SURFACE]                              | [SURFACE_ROOT] | [RAIL]                                  |
| :-----: | :------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `DerivePathInfo(DerivePathInfo)`       | `Verifier`     | global snapshot-path resolver           |
|  [02]   | `UseProjectRelativeDirectory(string)`  | `Verifier`     | project-root-relative snapshot dir      |
|  [03]   | `UseSourceFileRelativeDirectory(string)`| `Verifier`    | source-file-relative snapshot dir       |
|  [04]   | `AddAttachmentEvents()`                | `Verifier`     | route received/verified into xUnit attachments |
|  [05]   | `BuildVerifier(VerifySettings, sourceFile, useUniqueDirectory)` | `Verifier` | inner-verifier build (extension only) |

[SETTINGS_CHAIN]: fluent per-call settings off the returned `SettingsTask`
- rail: test

| [INDEX] | [SURFACE]                                                                  | [SURFACE_ROOT]   | [RAIL]                          |
| :-----: | :------------------------------------------------------------------------- | :--------------- | :------------------------------ |
|  [01]   | `UseStreamComparer(StreamCompare, extensions?)`                            | `SettingsTask`   | custom byte-diff (visual hash)  |
|  [02]   | `UseStringComparer(StringCompare, extensions?)`                           | `SettingsTask`   | custom text-diff                |
|  [03]   | `UniqueForRuntime()` / `UniqueForTargetFramework[AndVersion]()`           | `SettingsTask`   | CI-stable per-host snapshot key |
|  [04]   | `UniqueForAssemblyConfiguration()`                                        | `SettingsTask`   | debug/release-split snapshot    |
|  [05]   | `AddNamedGuid` / `AddNamedDateTime` / `AddNamedTime` / `AddNamedDate`     | `SettingsTask`   | named-value scrub stabilization |
|  [06]   | `OnVerifyMismatch` / `OnFirstVerify` / `OnVerify(before, after)`          | `SettingsTask`   | mismatch / first-run hook       |
|  [07]   | `AddExtraSettings(Action<JsonSerializerSettings>)`                        | `SettingsTask`   | object-graph serialization tune |
|  [08]   | `AppendValue` / `AppendValues`                                            | `SettingsTask`   | extra named context in snapshot |
|  [09]   | `DisableDiff()`                                                           | `SettingsTask`   | suppress diff-tool launch (CI)  |
|  [10]   | `UseDirectory` / `UseFileName` / `UseParameters` / `AddScrubber` / `ScrubLinesContaining` / `IgnoreMember` / `AutoVerify` | `VerifySettings` | placement, scrub, member, auto-accept |

## [04]-[IMPLEMENTATION_LAW]

[VERIFY_TOPOLOGY]:
- `VerifyXunit` carries exactly 5 types; `SettingsTask`, `VerifySettings`, `Target`, `Combination`, and the scrubber surface arrive from the transitive `Verify` core, so a test composes `Verifier.Verify*(...)` plus the core fluent chain in one expression.
- Every `Verifier.Verify*`/`Throws*` returns a `SettingsTask`; the test `await`s it, the awaited build runs the comparison against the `.verified.` file and writes a `.received.` on mismatch, and the fluent settings methods are called inline before the await (`await Verify(x).ScrubLinesContaining("ts").UniqueForTargetFramework()`).
- `VerifyBase` is abstract; a test class inherits it to call instance `Verify*` on `this`, and its constructor captures `[CallerFilePath]` once so every instance call resolves snapshot placement from the class source file. The static `Verifier` entry is the default; the base class is for fixtures that hold shared settings.
- `DerivePathInfo` is the `(sourceFile, projectDirectory, type, method) -> PathInfo` delegate; `Verifier.DerivePathInfo` or `UseProjectRelativeDirectory`/`UseSourceFileRelativeDirectory` redirect `.verified.` placement so snapshots live in a committed directory rather than beside transient build output.

[STACKING]:
- xUnit v3 wires through `[CallerFilePath]` on every `sourceFile` parameter; the test never passes a literal path, and `Verify.XunitV3.props` initializes the attachment context at module load so `AddAttachmentEvents()` can route received/verified files into the xUnit v3 attachment stream. The `assay test --csharp` lane (`Mode.RUN`) executes these as ordinary MTP tests; a snapshot mismatch surfaces as a `Completed(FAILED)` test result, not a rail fault.
- Visual proof stacks the byte-snapshot path onto the headless render lane: an offscreen `SKImage` encoded to a PNG `byte[]` (or the `RenderReceipt` frame bytes) flows through `Verify(bytes, "png", ...)` under a `UseStreamComparer` that hashes rather than byte-equals, and `UniqueForTargetFramework()` partitions the snapshot per host so a Skia raster-version drift does not cross-fail; this is the per-named-dashboard render-hash proof the Charts and Render design pages reference.
- Structural proof stacks `VerifyJson` onto a settled receipt: a frozen `CommandDeck`, a `DockLayout` blob, or a `TableViewState` serializes through `VerifyJson` with `ScrubLinesContaining`/`AddNamedGuid` stabilizing volatile ids and timestamps, so a deck or layout regression is a one-line snapshot diff. `AddExtraSettings` tunes the Argon object-graph serializer when the receipt carries types Verify does not format by default.
- Suite hygiene stacks the two gates into setup/teardown: `await VerifyChecks.Run()` validates the verify configuration once (it is async — `await` it), and `DanglingSnapshots.Run()` reports `.verified.` files with no owning test during a CI cleanup pass; neither is a per-test call.

[LOCAL_ADMISSION]:
- Snapshot files default to `[ClassName].[MethodName].verified.[ext]` beside the test source; `UseProjectRelativeDirectory`/`UseSourceFileRelativeDirectory`/`DerivePathInfo` redirect placement for a committed snapshot directory under CI.
- `Throws*` is the canonical typed-error proof — a domain rail that returns `Fin`/`Validation` proves its failure shape through `Throws` only at the boundary where an exception is the contract; inside ROP code the receipt is verified via `VerifyJson` of the failure value, never a thrown exception.
- `Combination` runs a cartesian input matrix into one snapshot, collapsing N near-identical `[Theory]` cases into one combination block.

[RAIL_LAW]:
- Package: `Verify.XunitV3`
- Owns: snapshot approval testing for xUnit v3 suites — object, value, JSON/XML, file/directory/zip, byte/stream, and throw snapshots, plus the fluent scrub/uniqueness/comparer settings chain.
- Accept: `Verifier.Verify*`/`Throws*` static entry or a `VerifyBase` subclass; the fluent `SettingsTask`/`VerifySettings` chain for scrubbing, CI-stable uniqueness, and custom comparers; `VerifyChecks`/`DanglingSnapshots` as suite gates.
- Reject: constructing `InnerVerifier` outside `BuildVerifier`; literal `sourceFile` arguments that bypass `[CallerFilePath]`; a hand-rolled string-equality assertion over serialized output when `VerifyJson` + scrubbers already own the comparison; a parallel image-diff harness when `UseStreamComparer` carries the visual-hash compare.
