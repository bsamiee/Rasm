# [DOTNET_TESTING_API_TESTING_PLATFORM]

`Microsoft.Testing.Platform` is the self-hosting test runtime every C# suite compiles into: the MSBuild package generates the entry point and self-registers extensions from `TestingPlatformBuilderHook` items, and the CrashDump/HangDump/Retry/TrxReport extensions carry the diagnostics tail. `global.json` pins `test.runner = Microsoft.Testing.Platform`, so `dotnet test` routes through MTP and the legacy VSTest target hard-errors on the pinned SDK. `Directory.Build.props` injects the stack per `IsTestProject` and scrubs the entry-point machinery from transitive project references.

## [01]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                     | [KIND]       | [CAPABILITY]                                                                       |
| :-----: | :--------------------------- | :----------- | :--------------------------------------------------------------------------------- |
|  [01]   | `TestApplication`            | host         | `CreateBuilderAsync(args, options)` / `CreateServerModeBuilderAsync`; `RunAsync()` |
|  [02]   | `ITestApplicationBuilder`    | builder      | composes host, controllers, command line, configuration, logging into `BuildAsync` |
|  [03]   | `TestApplicationOptions`     | options      | `EnableTelemetry` and configuration seed                                           |
|  [04]   | `TestingPlatformBuilderHook` | registration | GUID + `TypeFullName` rows the entry-point generator wires in, ordered by GUID     |

## [02]-[ENTRYPOINTS]

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
|  [17]   | `platformOptions:resultDirectory`                                | config  | result root behind TRX, dumps, and the relocated log       |
|  [18]   | `platformOptions__resultDirectory`                               | env     | the same option through the environment provider           |
|  [19]   | `TESTINGPLATFORM_DIAGNOSTIC_OUTPUT_DIRECTORY`                    | env     | diagnostic log root, read at bootstrap                     |

```csharp
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

## [03]-[IMPLEMENTATION_LAW]

[ENTRY_POINT]: `IsTestingPlatformApplication` (default `true` for MTP apps) drives `GenerateTestingPlatformEntryPoint` and `GenerateSelfRegisteredExtensions`; the generated `MicrosoftTestingPlatformEntryPoint` composes every `TestingPlatformBuilderHook` item then `BuildAsync`/`RunAsync`. Under the xunit adapter the entry point is the adapter's — `xunit.v3.core.mtp-v2` sets `GenerateTestingPlatformEntryPoint=false`, keeps `GenerateSelfRegisteredExtensions=true`, and generates `XunitAutoGeneratedEntryPoint`; hooks still register, the host is xunit's.

[RESULT_ROUTING]: resolution runs one order — CLI `--results-directory`, then `platformOptions:resultDirectory` from any provider, then `TestResults` under the working directory, which is the module's own folder until `dotnet test` exports `DOTNET_CLI_TEST_COMMAND_WORKING_DIRECTORY` and substitutes the shell's. `Directory.Build.targets` generates the per-assembly `testconfig.json` pinning that option to the module-adjacent `TestResults`, so TRX, dumps, and logs hold the artifacts layout from every entry point.

[DIAGNOSTIC_BOOTSTRAP]: `--diagnostic` opens its log before the configuration file loads, honoring only a CLI `--results-directory`, a CLI `--diagnostic-output-directory`, `TESTINGPLATFORM_DIAGNOSTIC_OUTPUT_DIRECTORY`, or the working-directory default; the diagnostic family and `--config-file` stay bootstrap-only, refused inside `commandLineOptions`. Any configured result directory naming another folder relocates the open log and disposes the writer under every logger holding it, killing the run on its next diagnostic write.

[TELEMETRY]: the telemetry extension rides as a transitive floor; `TESTINGPLATFORM_TELEMETRY_OPTOUT` or `DOTNET_CLI_TELEMETRY_OPTOUT` disables it, and `TestApplicationOptions.EnableTelemetry` is the in-process toggle.

[STACKING]:
- `xunit.v3.mtp-v2` (`xunit-v3.md`): the test-framework adapter whose transitive core generates the entry point.
- `coverlet.MTP` (`coverlet-mtp.md`): a sibling builder hook; the estate's `RasmCoverage` gate splices its activation through `TestingPlatformCommandLineArguments`.
- `Avalonia.Headless.XUnit` (`libs/dotnet/Rasm.AppUi/.api/api-headless.md`): rides the same host through the xunit adapter's session model.

[LOCAL_ADMISSION]:
- Four diagnostics extensions inject per `IsTestProject` with `PrivateAssets="all"`; a csproj re-wiring them is the named defect.
- Platform options travel as CLI arguments or `testconfig.json`; scattering `platformOptions` env keys across scripts re-derives what the config file owns.
- `Directory.Build.targets` generates `testconfig.json` and links it as the output copy the MSBuild package leaves alone; a project-directory source or a hand-written output copy forks the routing.
