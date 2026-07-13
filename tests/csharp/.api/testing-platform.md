# [testing-platform] — the MTP execution host, entry-point generation, and diagnostics extensions

`Microsoft.Testing.Platform` is the self-hosting test runtime every C# suite compiles into: the MSBuild package generates the entry point and self-registers extensions from `TestingPlatformBuilderHook` items, and the CrashDump/HangDump/Retry/TrxReport extensions carry the diagnostics tail. `global.json` pins `test.runner = Microsoft.Testing.Platform`, so `dotnet test` routes through MTP and the legacy VSTest target hard-errors on the pinned SDK. `Directory.Build.props` injects the stack per `IsTestProject` and scrubs the entry-point machinery from transitive project references.

## [01]-[PACKAGE_SURFACE]

- package: `Microsoft.Testing.Platform` `2.2.3` / `Microsoft.Testing.Platform.MSBuild` `2.2.3` / `Microsoft.Testing.Extensions.CrashDump` `2.2.3` / `Microsoft.Testing.Extensions.HangDump` `2.2.3` / `Microsoft.Testing.Extensions.Retry` `2.2.3` / `Microsoft.Testing.Extensions.TrxReport` `2.2.3`; transitive floor `Microsoft.Testing.Extensions.Telemetry` `2.2.3`
- license: `MIT` (Retry carries a license file)
- namespace: `Microsoft.Testing.Platform.Builder` (builder API); extensions register through generated code, not consumer calls
- asset: `lib/net8.0|net9.0|netstandard2.0` per package; MSBuild package ships the task under `_MSBuildTasks/netstandard2.0/`
- rail: evidence — process hosting, entry-point generation, dump capture, retry, and TRX reporting for every suite

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                     | [KIND]       | [CAPABILITY]                                                                       |
| :-----: | :--------------------------- | :----------- | :--------------------------------------------------------------------------------- |
|  [01]   | `TestApplication`            | host         | `CreateBuilderAsync(args, options)` / `CreateServerModeBuilderAsync`; `RunAsync()` |
|  [02]   | `ITestApplicationBuilder`    | builder      | composes host, controllers, command line, configuration, logging into `BuildAsync` |
|  [03]   | `TestApplicationOptions`     | options      | `EnableTelemetry` and configuration seed                                           |
|  [04]   | `TestingPlatformBuilderHook` | registration | GUID + `TypeFullName` rows the entry-point generator wires in, ordered by GUID     |

## [03]-[ENTRYPOINTS]

Every dump and retry sub-flag demands its master switch; `--retry-failed-tests-max-percentage` and `--retry-failed-tests-max-tests` are mutually exclusive.

| [INDEX] | [SURFACE]                                                        | [KIND]  | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------------- | :------ | :--------------------------------------------------------- |
|  [01]   | `--results-directory`                                            | CLI     | TRX and dump output root                                   |
|  [02]   | `--report-trx`                                                   | CLI     | emit a TRX report                                          |
|  [03]   | `--report-trx-filename`                                          | CLI     | TRX filename; needs `--report-trx`, forbids `--list-tests` |
|  [04]   | `--crashdump`                                                    | CLI     | crash dump master switch                                   |
|  [05]   | `--crashdump-type` / `--crashdump-filename`                      | CLI     | crash dump type and filename                               |
|  [06]   | `--hangdump`                                                     | CLI     | hang dump master switch                                    |
|  [07]   | `--hangdump-type` / `--hangdump-timeout` / `--hangdump-filename` | CLI     | hang dump type, detection timeout, and filename            |
|  [08]   | `--retry-failed-tests <n>`                                       | CLI     | in-process retry count                                     |
|  [09]   | `--retry-failed-tests-max-percentage`                            | CLI     | retry ceiling by failure percentage                        |
|  [10]   | `--retry-failed-tests-max-tests`                                 | CLI     | retry ceiling by failure count                             |
|  [11]   | `--filter` / `--list-tests`                                      | CLI     | selection expression and list-without-run                  |
|  [12]   | `--minimum-expected-tests` / `--maximum-failed-tests`            | CLI     | discovered-count floor and fail-fast ceiling               |
|  [13]   | `--diagnostic*` / `--timeout`                                    | CLI     | platform diagnostics family and global run timeout         |
|  [14]   | `--ignore-exit-code` / `--no-banner` / `--no-progress`           | CLI     | force zero exit, suppress banner and progress              |
|  [15]   | `TestingPlatformCommandLineArguments`                            | MSBuild | verbatim argument splice; the coverage gate rides it       |
|  [16]   | `testconfig.json` -> `$(AssemblyName).testconfig.json`           | config  | file-borne platform options; copied beside the executable  |

```csharp signature
public static class TestApplication {
    public static Task<ITestApplicationBuilder> CreateBuilderAsync(
        string[] args, TestApplicationOptions? testApplicationOptions = null);
}
public interface ITestApplicationBuilder {
    ITestHostManager TestHost { get; }
    ITestHostControllersManager TestHostControllers { get; }
    Task<ITestApplication> BuildAsync();
}
```

## [04]-[IMPLEMENTATION_LAW]

[ENTRY_POINT]: `IsTestingPlatformApplication` (default `true` for MTP apps) drives `GenerateTestingPlatformEntryPoint` and `GenerateSelfRegisteredExtensions`; the platform-generated `MicrosoftTestingPlatformEntryPoint` composes every `TestingPlatformBuilderHook` item then `BuildAsync`/`RunAsync`. Under the xunit adapter the entry point is the adapter's: `xunit.v3.core.mtp-v2` sets `GenerateTestingPlatformEntryPoint=false`, keeps `GenerateSelfRegisteredExtensions=true`, and generates `XunitAutoGeneratedEntryPoint` instead — hooks still register, the host is xunit's. The estate's reference-isolation `ItemDefinitionGroup` removes `IsTestProject;UseMicrosoftTestingPlatformRunner;IsTestingPlatformApplication;GenerateTestingPlatformEntryPoint;GenerateSelfRegisteredExtensions` from transitive project references so production assemblies never grow entry points; `AssayTestShell=true` reclassifies a shell out of the machinery entirely.

[DOTNET_TEST]: `global.json` `test.runner = Microsoft.Testing.Platform` selects the MTP `dotnet test` experience on the pinned SDK; `_MTPBeforeVSTest` errors any VSTest-routed MTP application on SDK 10+, so the VSTest lane is unspellable here. Manual TRX evidence routes `--report-trx --results-directory .artifacts/csharp/trx/<project>`; assay-run suites route into the assay artifact scope.

[TELEMETRY]: the telemetry extension rides as a transitive floor; `TESTINGPLATFORM_TELEMETRY_OPTOUT` or `DOTNET_CLI_TELEMETRY_OPTOUT` disables it, and `TestApplicationOptions.EnableTelemetry` is the in-process toggle.

[STACKING]:
- `xunit.v3.mtp-v2` (`xunit-v3.md`): the test-framework adapter whose transitive core generates the entry point.
- `coverlet.MTP` (`coverlet-mtp.md`): a sibling builder hook; the estate's `RasmCoverage` gate splices its activation through `TestingPlatformCommandLineArguments`.
- `Avalonia.Headless.XUnit` (`libs/csharp/Rasm.AppUi/.api/api-headless.md`): rides the same host through the xunit adapter's session model.

[LOCAL_ADMISSION]:
- The four diagnostics extensions inject per `IsTestProject` with `PrivateAssets="all"`; a csproj re-wiring them is the named defect.
- Platform options travel as CLI arguments or `testconfig.json`; scattering `platformOptions` env keys across scripts re-derives what the config file owns.

[RAIL_LAW]:
- Package: `Microsoft.Testing.Platform` + `Microsoft.Testing.Platform.MSBuild` + `Microsoft.Testing.Extensions.{CrashDump, HangDump, Retry, TrxReport}`
- Owns: test process hosting, entry-point generation, extension registration order, dump/retry/TRX diagnostics, and the `dotnet test` MTP integration.
- Accept: CLI-driven run shaping; `TestingPlatformCommandLineArguments` for MSBuild-variable-driven splices; server mode for IDE discovery.
- Reject: VSTest routing, `Microsoft.NET.Test.Sdk`, per-project runner wiring, or duplicate extension registration.
