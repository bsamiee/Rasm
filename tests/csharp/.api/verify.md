# [verify] — the snapshot verb and hygiene walk over generator, contract, and report shapes

`Verify.XunitV3` binds the snapshot verb to the xunit.v3 discovery model: `Verifier.Verify(...)` compares a rendered target against a committed `*.verified.*` file and fails on drift, with the received counterpart written for review. `Verify.DiffPlex` replaces the failure diff with a DiffPlex-rendered compare, armed once per test assembly from a `[ModuleInitializer]`. The estate snapshots generator emissions and runs the whole-tree snapshot-hygiene walk as an explicit-only architecture case.

## [01]-[PACKAGE_SURFACE]

- package: `Verify.XunitV3` `31.20.0` / `Verify.DiffPlex` `3.3.0`
- license: `MIT`
- namespace: `VerifyXunit` (entry statics), `VerifyTests` (settings, scrubbers, DiffPlex extension)
- asset: `Verify.XunitV3.dll` + transitive `Verify.dll` (`net10.0`); `Verify.DiffPlex.dll` over `DiffPlex`
- rail: evidence — snapshot equality with typed scrubbing, path routing, and repo hygiene checks

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                             | [KIND]        | [CAPABILITY]                                                                                          |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Verifier`                           | static entry  | the `Verify*` overload family; every call returns `SettingsTask`                                      |
|  [02]   | `SettingsTask`                       | fluent task   | awaitable (`TaskAwaiter<VerifyResult>`) with per-call setting verbs                                   |
|  [03]   | `VerifySettings`                     | settings      | instance form of the same verbs; `UseFileName` excludes `UseTypeName`/`UseMethodName`/`UseParameters` |
|  [04]   | `VerifierSettings`                   | static config | process-wide scrubbers, serialization, `DerivePathInfo` path routing                                  |
|  [05]   | `VerifyChecks` / `DanglingSnapshots` | hygiene       | whole-tree snapshot hygiene walk; untracked-snapshot gate on CI                                       |
|  [06]   | `VerifyDiffPlex` / `OutputType`      | diff renderer | `Initialize(OutputType)` once per assembly; `Full`/`Compact`/`Minimal`                                |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                               | [KIND]    | [CAPABILITY]                                                                                          |
| :-----: | :---------------------------------------------------------------------- | :-------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Verifier.Verify(string?, VerifySettings?)`                             | snapshot  | string target; `object?`, `Task<T>`, `Stream`, `byte[]` forms mirror                                  |
|  [02]   | `Verifier.VerifyJson` / `VerifyXml` / `VerifyTuple`                     | snapshot  | syntax-aware targets                                                                                  |
|  [03]   | `Verifier.VerifyFile` / `VerifyFiles` / `VerifyDirectory` / `VerifyZip` | snapshot  | file-system and archive targets with include filters and scrubbers                                    |
|  [04]   | `SettingsTask.UseDirectory/UseFileName/UseParameters/UniqueFor*`        | routing   | snapshot path and discriminant control                                                                |
|  [05]   | `SettingsTask.ScrubLines*/ScrubInlineGuids/AddScrubber/IgnoreMember<T>` | scrubbing | deterministic snapshot normalization                                                                  |
|  [06]   | `SettingsTask.AutoVerify(bool includeBuildServer, bool throwException)` | control   | accept-current mode; `DisableDiff()` suppresses the diff tool                                         |
|  [07]   | `VerifyChecks.Run()`                                                    | hygiene   | gitignore, csproj snapshot-nesting, editorconfig, gitattributes checks; throws `VerifyCheckException` |
|  [08]   | `VerifyDiffPlex.Initialize(OutputType)` / `UseDiffPlex(...)`            | diff      | global arm (second call throws) and per-verification override                                         |

```csharp contract
public static partial class Verifier {
    public static SettingsTask Verify(string? target, VerifySettings? settings = null,
        [CallerFilePath] string sourceFile = "");
}
public partial class SettingsTask {
    public TaskAwaiter<VerifyResult> GetAwaiter();
    public SettingsTask UseDirectory(string directory);
    public SettingsTask UseFileName(string fileName);
    public SettingsTask AutoVerify(bool includeBuildServer = true, bool throwException = false);
}
public static class VerifyDiffPlex {
    public static void Initialize(OutputType outputType);
}
```

## [04]-[IMPLEMENTATION_LAW]

[NAMING]: the snapshot path is `{Directory}/{TestClassName}.{TestMethodName}_{Parameters}_{UniqueFor1..X}.verified.{extension}`; `UseFileName` collapses to `{FileName}_{UniqueFor}.verified.{extension}`. Received files are the `*.received.*` mirror. Snapshot bytes open with the UTF-8 BOM the hygiene walk's editorconfig contract demands — a `[*.{received,verified}.{<exts>}]` section per the snapshot extensions found in-tree (`txt` here) carrying `charset = utf-8-bom`, `end_of_line = lf`, `insert_final_newline = false`, `trim_trailing_whitespace = false`.

[HYGIENE]: `VerifyChecks.Run()` resolves the calling assembly's solution directory and walks the WHOLE tree — gitignore coverage for received files, wrongly nested `<None Update>` snapshot rows in csprojs, the editorconfig section, and gitattributes rows per snapshot extension. The estate carries it as `[Fact(Explicit = true)]`, invoked with `-- --explicit only`. `DanglingSnapshots.Run()` fails a build-server run on `*.verified.*` files no executed test tracked and stays inert locally.

[ATTACHMENT]: no base class — `Verify` binds to the ambient test through `TestContext`; test-assembly registration lives in one `[ModuleInitializer]` per assembly, which is also where `VerifierSettings` global scrubbers belong. `VerifyDiffPlex.Initialize` throws on a second call, so each test assembly arms it exactly once.

[STACKING]:
- `xunit.v3` (`xunit-v3.md`): depends on `xunit.v3.extensibility.core`; the snapshot verb rides v3 discovery and `Explicit` routing.
- `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` (`analyzer-testing.md`): generator emissions render to a string and snapshot through `Verifier.Verify`, bypassing the analyzer harness for generator specs.
- `Verify.DiffPlex`: replaces the default failure output; `OutputType.Compact` is the initialize default.

[LOCAL_ADMISSION]:
- Snapshot registration is one `[ModuleInitializer]` per test assembly; scattered `VerifierSettings` mutation inside test bodies is the named defect.
- The hygiene walk runs explicit-only; wiring it into default runs makes every suite a whole-tree scan.

[RAIL_LAW]:
- Package: `Verify.XunitV3` + `Verify.DiffPlex`
- Owns: snapshot equality, snapshot path/scrub policy, diff rendering, and snapshot repo hygiene for C# specs.
- Accept: string/object/file/archive targets rendered deterministically; explicit-only hygiene walks; `AutoVerify` only in deliberate local acceptance.
- Reject: a second snapshot engine, hand-rolled golden-file comparison, or committed `*.received.*` files.
