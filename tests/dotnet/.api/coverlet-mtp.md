# [DOTNET_TESTING_API_COVERLET_MTP]

`coverlet.MTP` is the Microsoft.Testing.Platform flavor of coverlet: a builder-hook extension that IL-rewrites system-under-test assemblies ahead of load (Mono.Cecil, sequence-point hit recording) and reports on process exit. It is configured exclusively through MTP command-line options and config files — the `coverlet.msbuild` `Coverlet*` MSBuild property family is inert under this flavor. Estate activation runs through the `RasmCoverage=true` gate in `Directory.Build.targets`, which splices the `--coverlet` tail into `TestingPlatformCommandLineArguments` per test executable.

## [01]-[PUBLIC_TYPES]

`TestingPlatformBuilderHook` wires registration GUID `6C751FC6-00AA-43AD-8265-79C3FED21943` into the generated entry point; the remaining types are extension-host-internal.

| [INDEX] | [SYMBOL]                                                  | [KIND]         | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `TestingPlatformBuilderHook`                              | MSBuild item   | registration item spliced into the generated entry point    |
|  [02]   | `CoverletExtension` / `CoverletExtensionProvider`         | MTP extension  | controller lifetime: instrument pre-start, report post-exit |
|  [03]   | `CoverletMTPSettings`                                     | config         | resolved settings shape; parsing and providers internal     |
|  [04]   | `CoverletTestSessionHandler` / `CoverletInProcessHandler` | test-host side | in-process hit flush on session end                         |

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                       | [KIND] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------- | :----- | :---------------------------------------------------------------------- |
|  [01]   | `--coverlet`                                    | CLI    | activation switch; idle without it                                      |
|  [02]   | `--coverlet-output-format <fmt>`                | CLI    | repeatable; `json`, `cobertura` (seed), `lcov`, `opencover`, `teamcity` |
|  [03]   | `--coverlet-include`                            | CLI    | assembly/type filter globs, comma-separated                             |
|  [04]   | `--coverlet-include-directory`                  | CLI    | instrumented-directory filter                                           |
|  [05]   | `--coverlet-exclude`                            | CLI    | assembly/type exclusion globs                                           |
|  [06]   | `--coverlet-exclude-by-file`                    | CLI    | source-file exclusion globs                                             |
|  [07]   | `--coverlet-exclude-by-attribute`               | CLI    | attribute-name exclusion                                                |
|  [08]   | `--coverlet-does-not-return-attribute`          | CLI    | unreachable-branch attribute markers                                    |
|  [09]   | `--coverlet-exclude-assemblies-without-sources` | CLI    | sourceless-assembly policy                                              |
|  [10]   | `--coverlet-file-prefix`                        | CLI    | report filename prefix in the results directory                         |
|  [11]   | `--coverlet-include-test-assembly`              | CLI    | accepted but non-functional; the controller cannot self-instrument      |
|  [12]   | `--coverlet-single-hit`                         | CLI    | record first hit only, mirrors the core knob                            |
|  [13]   | `--coverlet-skip-auto-props`                    | CLI    | skip auto-property instrumentation                                      |
|  [14]   | `testconfig.json` `platformOptions.Coverlet`    | config | file-borne config, camelCase keys; precedence per `[PRECEDENCE]` below  |

- [14]-[PRECEDENCE]: CLI over `[app].testconfig.json` over `testconfig.json` over `coverlet.mtp.appsettings.json` over defaults.

## [03]-[IMPLEMENTATION_LAW]

[ACTIVATION]: `Directory.Build.targets` owns the one activation seam — `RasmCoverage=true` on any `IsTestProject` splices `--coverlet --coverlet-output-format $(RasmCoverageFormat) --coverlet-include "[Rasm*]*,[Csp.*]*" --coverlet-exclude "[*Tests]*,[*TestKit]*" --coverlet-file-prefix $(MSBuildProjectName)` into `TestingPlatformCommandLineArguments`; `RasmCoverageFormat` defaults to `cobertura`, and the report lands beside the run's `--results-directory` as `<prefix>.coverage.<format>.<stamp>.<ext>`.

[DEAD_KNOBS]: the shipped build assets read no `Coverlet*` MSBuild property — `CoverletOutputFormat`, `CoverletInclude`, `CoverletExclude`, `CoverletOutput`, and the rest of the `coverlet.msbuild` family configure nothing here. There is no output-directory option; only the filename prefix and the results directory route placement. In CLI-only mode default excludes (`[coverlet.*]*`, `[xunit.*]*`, `[Microsoft.Testing.*]*`, test-host families) and default exclude-attributes auto-merge; a config file switches to authoritative mode where no defaults inject.

[ARCHITECTURE]: two processes — the controller instruments target assemblies on disk before test-host start and reads hits after exit; the test host flushes coverage data through the in-process session handler. Threshold validation and runtime report merging are not part of this flavor; merging routes through external report tooling.

[STACKING]:
- `Microsoft.Testing.Platform` (`testing-platform.md`): registers as a builder hook through the well-known GUID; `GenerateSelfRegisteredExtensions` wires it into the generated entry point.
- `xunit.v3.mtp-v2` (`xunit-v3.md`): supplies the test host the extension instruments; both compose under one MTP entry point per test executable.

[LOCAL_ADMISSION]:
- Coverage activates through the `RasmCoverage` gate only; default test runs stay uninstrumented.
- `Coverlet*` MSBuild property rows anywhere in the estate are dead configuration and are deleted on sight.
