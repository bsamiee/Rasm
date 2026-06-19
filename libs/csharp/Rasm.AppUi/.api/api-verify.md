# [RASM_APPUI_API_VERIFY]

`Verify.XunitV3` supplies snapshot-based approval testing through `Verifier` (static entry point for xUnit v3 tests) and `VerifyBase` (base class for test classes), with `VerifyChecks` for configuration self-checks and `DanglingSnapshots` for stale file detection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Verify.XunitV3`
- package: `Verify.XunitV3`
- assembly: `Verify.XunitV3`
- namespace: `VerifyXunit`
- asset: runtime library
- rail: test

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: approval test surfaces
- rail: test

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                        |
| :-----: | :------------------ | :-------------- | :---------------------------- |
|  [01]   | `Verifier`          | static entry    | xUnit v3 verify entry point   |
|  [02]   | `VerifyBase`        | test base class | instance verify base          |
|  [03]   | `DerivePathInfo`    | path delegate   | custom snapshot path resolver |
|  [04]   | `VerifyChecks`      | self-check      | configuration verification    |
|  [05]   | `DanglingSnapshots` | stale detection | orphaned snapshot finder      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: static verify operations on Verifier
- rail: test

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT] | [RAIL]                   |
| :-----: | :-------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `Verify(object?, IEnumerable<Target>, ...)`   | `Verifier`     | multi-target verify      |
|  [02]   | `Verify(IEnumerable<Target>, ...)`            | `Verifier`     | target collection verify |
|  [03]   | `Verify(Target, ...)`                         | `Verifier`     | single target verify     |
|  [04]   | `VerifyFile(string, ...)`                     | `Verifier`     | file snapshot verify     |
|  [05]   | `VerifyFile(FileInfo, ...)`                   | `Verifier`     | FileInfo verify          |
|  [06]   | `VerifyFiles(IEnumerable<string>, ...)`       | `Verifier`     | multi-file verify        |
|  [07]   | `VerifyDirectory(string, ...)`                | `Verifier`     | directory verify         |
|  [08]   | `VerifyDirectory(DirectoryInfo, ...)`         | `Verifier`     | directory verify         |
|  [09]   | `VerifyJson(StringBuilder?, ...)`             | `Verifier`     | JSON snapshot verify     |
|  [10]   | `VerifyZip(string, ...)`                      | `Verifier`     | ZIP archive verify       |
|  [11]   | `VerifyZip(Stream, ...)`                      | `Verifier`     | stream ZIP verify        |
|  [12]   | `Combination(bool?, ...)`                     | `Verifier`     | combination test runner  |
|  [13]   | `DerivePathInfo(DerivePathInfo)`              | `Verifier`     | snapshot path override   |
|  [14]   | `UseProjectRelativeDirectory(string)`         | `Verifier`     | project-relative paths   |
|  [15]   | `BuildVerifier(VerifySettings, string, bool)` | `Verifier`     | inner verifier build     |

## [04]-[IMPLEMENTATION_LAW]

[VERIFY_TOPOLOGY]:
- namespace: `VerifyXunit`; 6 types across 1 namespace; assembly is `Verify.XunitV3`
- `Verifier` is a static class; all `Verify*` methods return `SettingsTask` for fluent settings chaining
- `VerifyBase` is abstract; test classes inherit it to access instance `Verify*` variants on `this`
- `DerivePathInfo` is a delegate type for customizing `.verified.` file placement

[LOCAL_ADMISSION]:
- Snapshot files live beside the test source under `[MethodName].verified.[ext]` by default; `DerivePathInfo` or `UseProjectRelativeDirectory` redirect placement for CI environments.
- `VerifyChecks.Run()` validates the verify configuration on first test execution; include it in suite setup.
- `DanglingSnapshots` detects `.verified.` files with no corresponding test; run during CI cleanup passes.
- xUnit v3 integration wires through `[CallerFilePath]` on `sourceFile` parameters; do not pass literal paths.

[RAIL_LAW]:
- Package: `Verify.XunitV3`
- Owns: snapshot approval testing for xUnit v3 test suites
- Accept: `Verifier.Verify*` for static entry and `VerifyBase` subclass for instance entry
- Reject: constructing `InnerVerifier` directly outside framework extension points
