# [coverlet-mtp] — CLI-configured coverage instrumentation inside every MTP test executable

`coverlet.MTP` is the Microsoft.Testing.Platform flavor of coverlet: a builder-hook extension that IL-rewrites system-under-test assemblies ahead of load (Mono.Cecil, sequence-point hit recording) and reports on process exit. It is configured exclusively through MTP command-line options and config files — the `coverlet.msbuild` `Coverlet*` MSBuild property family is inert under this flavor. The estate activates it through the `RasmCoverage=true` gate in `Directory.Build.props`, which splices the `--coverlet` tail into `TestingPlatformCommandLineArguments` per test executable.

## [01]-[PACKAGE_SURFACE]

- package: `coverlet.MTP` `10.0.1`
- license: `MIT`
- namespace: `Coverlet.MTP` (extension host types; no consumer-facing managed API)
- asset: `lib/net10.0/coverlet.MTP.dll` + `coverlet.core.dll` (Mono.Cecil instrumentation stack); `buildMultiTargeting` props register the `TestingPlatformBuilderHook`
- rail: evidence — line/branch coverage collection and report emission for MTP test hosts

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                  | [KIND]         | [CAPABILITY]                                                                                   |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `TestingPlatformBuilderHook`                              | MSBuild item   | registers extension GUID `6C751FC6-00AA-43AD-8265-79C3FED21943` into the generated entry point |
|  [02]   | `CoverletExtension` / `CoverletExtensionProvider`         | MTP extension  | controller-process lifetime handler: instrument before start, report after exit                |
|  [03]   | `CoverletMTPSettings`                                     | config         | the resolved settings shape; parsing and option providers stay internal                        |
|  [04]   | `CoverletTestSessionHandler` / `CoverletInProcessHandler` | test-host side | in-process hit flush on session end                                                            |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                                                    | [KIND] | [CAPABILITY]                                                                                                                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------- | :----- | :------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `--coverlet`                                                                                                                 | CLI    | the activation switch; without it the extension stays idle                                                                                        |
|  [02]   | `--coverlet-output-format <fmt>`                                                                                             | CLI    | repeatable; `json`, `cobertura` (the settings seed), `lcov`, `opencover`, `teamcity`                                                              |
|  [03]   | `--coverlet-include` / `--coverlet-include-directory` / `--coverlet-exclude` / `--coverlet-exclude-by-file`                  | CLI    | assembly/type filter globs plus file and directory filters, comma-separated                                                                       |
|  [04]   | `--coverlet-exclude-by-attribute` / `--coverlet-does-not-return-attribute` / `--coverlet-exclude-assemblies-without-sources` | CLI    | attribute exclusion, unreachable-branch attributes, sourceless-assembly policy                                                                    |
|  [05]   | `--coverlet-file-prefix`                                                                                                     | CLI    | report filename prefix; the report lands in the results directory                                                                                 |
|  [06]   | `--coverlet-include-test-assembly` / `--coverlet-single-hit` / `--coverlet-skip-auto-props`                                  | CLI    | include-test-assembly is accepted but non-functional (the controller cannot self-instrument); single-hit and auto-prop skip mirror the core knobs |
|  [07]   | `testconfig.json` `platformOptions.Coverlet` (camelCase keys)                                                                | config | file-borne configuration; precedence CLI > `[app].testconfig.json` > `testconfig.json` > `coverlet.mtp.appsettings.json` > defaults               |

## [04]-[IMPLEMENTATION_LAW]

[ACTIVATION]: `Directory.Build.props` owns the one activation seam — `RasmCoverage=true` on any `IsTestProject` splices `--coverlet --coverlet-output-format $(RasmCoverageFormat) --coverlet-include "[Rasm*]*,[Csp.*]*" --coverlet-exclude "[*Tests]*,[*TestKit]*" --coverlet-file-prefix $(MSBuildProjectName)` into `TestingPlatformCommandLineArguments`; `RasmCoverageFormat` defaults to `cobertura`. The report lands beside the run's `--results-directory` as `<prefix>.coverage.<format>.<stamp>.<ext>`.

[DEAD_KNOBS]: the shipped build assets read no `Coverlet*` MSBuild property — `CoverletOutputFormat`, `CoverletInclude`, `CoverletExclude`, `CoverletOutput`, and the rest of the `coverlet.msbuild` family configure nothing here. There is no output-directory option; only the filename prefix and the results directory route placement. In CLI-only mode default excludes (`[coverlet.*]*`, `[xunit.*]*`, `[Microsoft.Testing.*]*`, test-host families) and default exclude-attributes auto-merge; a config file switches to authoritative mode where no defaults inject.

[ARCHITECTURE]: two processes — the controller instruments target assemblies on disk before test-host start and reads hits after exit; the test host flushes coverage data through the in-process session handler. Threshold validation and runtime report merging are not part of this flavor; merging routes through external report tooling.

[STACKING]:
- `Microsoft.Testing.Platform` (`testing-platform.md`): registers as a builder hook through the well-known GUID; `GenerateSelfRegisteredExtensions` wires it into the generated entry point.
- `xunit.v3.mtp-v2` (`xunit-v3.md`): supplies the test host the extension instruments; both compose under one MTP entry point per test executable.

[LOCAL_ADMISSION]:
- Coverage activates through the `RasmCoverage` gate only; default test runs stay uninstrumented.
- A `Coverlet*` MSBuild property row anywhere in the estate is dead configuration and is deleted on sight.

[RAIL_LAW]:
- Package: `coverlet.MTP`
- Owns: coverage instrumentation and report emission for every MTP-hosted C# suite.
- Accept: `RasmCoverage=true` runs with format/filter policy carried by the central splice; config-file authoritative mode when a coverage campaign needs per-key control.
- Reject: `coverlet.msbuild`/`coverlet.collector` siblings, `Coverlet*` property blocks, or per-csproj coverage wiring.
