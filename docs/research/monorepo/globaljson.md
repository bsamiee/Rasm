<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN] and [03]-[CONFIGURATION], nothing integrated yet -->

# global.json in Rasm: who reads it, what mise does, what can move

On the machine read on 2026-09-03, `dotnet --info` reports SDK 10.0.400, MSBuild 18.9.6, and host 10.0.11, and `dotnet --list-sdks` lists 8.0.424, 9.0.317, and 10.0.400. The mise facts come from the source at tag `v2026.9.1`. Every claim applies to SDK 10.0.400 and host 10.0.11 unless stated.

## [01]-[READERS]

### [01.1]-[MUXER]

The muxer and the MSBuild project SDK resolver search for the file from different start directories, walking up ancestors (<https://learn.microsoft.com/en-us/dotnet/core/tools/global-json>). The muxer handling `dotnet` CLI commands starts from the current working directory, and the resolver starts from the directory holding the solution file, else the directory holding the project file, else the working directory.

The walk is in the host binary. `strings` on `.../dotnet-combined/share/dotnet/host/fxr/10.0.11/libhostfxr.dylib` yields `Probing path [%s] for global.json`, `Found global.json [%s]`, `Terminating global.json search at [%s]`, `Resolving SDKs with version = '%s', rollForward = '%s', allowPrerelease = %s`, `Invalid global.json is ignored for SDK resolution.`, `Ignoring SDK settings in global.json: the latest installed .NET SDK (%s prereleases) will be used`, and all nine `rollForward` tokens `disable`, `patch`, `feature`, `minor`, `major`, `latestPatch`, `latestFeature`, `latestMinor`, `latestMajor`.

Matching rules (same page): no file, or a file without `sdk.version` and without `allowPrerelease`, selects the highest installed SDK (equivalent to `latestMajor`), prereleases considered outside Visual Studio. A file with `allowPrerelease` and no `version` selects the highest installed with prereleases gated by that flag. A file with `version` and no `rollForward` uses `patch`.

`rollForward` values (same page): `patch` uses the exact version, else the latest patch level, else fails. `feature`, `minor`, and `major` each roll up one further dimension on a miss. `latestPatch`, `latestFeature`, `latestMinor`, and `latestMajor` take the highest installed at or above the floor within that dimension. `disable` requires an exact match. The page's footnote: with package lock files, set `rollForward` to `disable`, which keeps the SDK version and dependency graph in lockstep (citing dotnet/sdk#48795 and dotnet/aspnetcore#65061).

Observed: an empty directory with no global.json and three SDKs installed resolves `dotnet --version` to `10.0.400`, the highest installed. A directory holding `{"sdk":{"version":"10.0.999","rollForward":"disable"}}` exits 155 with `A compatible .NET SDK was not found. Requested SDK version: 10.0.999` and the installed list. The hard stop is what `disable` buys.

`sdk.paths` and `sdk.errorMessage` are "Available since: .NET 10 SDK" (same page). `paths` takes absolute or global.json-relative locations with `$host$`, searched in order, and works for SDK commands (`dotnet run`) alone, never for `app.exe`, `dotnet app.dll`, or `dotnet exec`. Rasm sets neither.

### [01.2]-[MSBUILD_SDK_RESOLVER]

<https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk> (last updated 2025-05-05): "The NuGet-based SDK resolver supports specifying a version in the global.json file, which allows you to control the project SDK version in one place rather than in each individual project", and "It is recommended to not specify a version in your projects if a version is specified in the global.json file." The registered resolvers are the NuGet-based one (active only when a version is given), the .NET SDK resolver that finds `Microsoft.NET.Sdk` and its siblings, and the default MSBuild-installed resolver.

The installed `.../sdk/10.0.400/SdkResolvers/` holds `Microsoft.Build.NuGetSdkResolver` and `Microsoft.NET.Sdk.WorkloadMSBuildSdkResolver`, and `Microsoft.DotNet.MSBuildSdkResolver` appears only as an assembly-reference string inside `Microsoft.DotNet.Cli.Utils.dll` and `dotnet.dll`. Rasm's `global.json` has no `msbuild-sdks` block, all 18 `.csproj` files use `Sdk="Microsoft.NET.Sdk"` with no version, and the consumer reads nothing.

### [01.3]-[VISUAL_STUDIO]

Same page: `allowPrerelease` defaults to `true` outside Visual Studio, and inside it follows the prerelease status requested. The page states that Visual Studio "only ever installs a single copy of the .NET SDK" and removes the previous one on upgrade, the standing argument for a pinned global.json on Visual Studio machines. Rasm develops on macOS, and the point is advisory.

### [01.4]-[TEST_RUNNER]

Schema (global-json page): `test.runner`, type string, "Available since: .NET 10.0 SDK", "The test runner to discover/run tests with."

The CLI reads the switch at parser construction, before any MSBuild work. `dotnet/sdk` branch `release/10.0.4xx`, `src/Cli/Microsoft.DotNet.Cli.Definitions/Commands/Test/TestCommandDefinition.cs`: `TestCommandDefinition.Create()` walks up from `Environment.CurrentDirectory` for `global.json`, returns `new VSTest()` when no file exists, deserializes only `{"test":{"runner":…}}`, returns `new VSTest()` when the name is null or equals `VSTest`, returns `new MicrosoftTestingPlatform()` when it equals `Microsoft.Testing.Platform`, and otherwise throws `CmdUnsupportedTestRunnerDescription`. Its comment, "This code path is hit exactly once during the whole life of the dotnet process", confirms it runs before MSBuild evaluation, the `dotnet test` command object is decided before any project is evaluated, and no MSBuild property can select it.

Corroborating strings in the installed `.../sdk/10.0.400/dotnet.dll`: `global.json defines test runner to be Microsoft.Testing.Platform. All projects must use that test runner.`, `The following test projects are using VSTest test runner:`, `MicrosoftTestingPlatformTestCommand`, `NETSDK1227: Unsupported ToolCommandRunner value: {0}`.

No environment escape hatch exists on 10.0.400: `strings dotnet.dll | rg -x DOTNET_TEST_RUNNER` returns nothing. On `dotnet/sdk` `main` (read 2026-09-03) the same file declares `TestRunnerEnvironmentVariableName = "DOTNET_TEST_RUNNER"` and `Create()` reads it, and the variable is post-10.0.4xx and unusable here.

The documentation names no property form. <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-mtp> (.NET 10 SDK and later): "When MTP is opted in via global.json, dotnet test expects all test projects to use MTP. It is an error if any of the test projects use VSTest." <https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test>: "To enable this mode, add the following configuration to your global.json file", and "Since this mode is specifically designed for MTP, neither `TestingPlatformDotnetTestSupport` nor the additional `--` are required."

### [01.5]-[OTHER_CONSUMERS]

`dotnet workload` reads `sdk.workloadVersion` (<https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-sets>): "If you have a workload-set version in the global.json file, the workload commands are in workload-set mode even if you haven't run the config command or used --version. The global.json file overrides those." `dotnet.dll` strings confirm: `Using workload version {0}, which was specified in the global.json file at {1}.` and `Cannot specify a particular workload version on the command line via --version or --from-history when there is already a version specified in global.json file {0}.` Rasm sets no `workloadVersion`, and `dotnet --info` reports no installed workloads and no workload sets.

`dotnet new` writes rather than reads: `dotnet new globaljson --sdk-version 8.0.302 --roll-forward latestFeature` (global-json page). `dotnet tool` has no global.json key, the local manifest is `.config/dotnet-tools.json`, which Rasm has (restored by `nx run eng:provision`, `README.md` line 49), and tool execution runs under whichever SDK the muxer picked. The SDK names the file in its own diagnostics: `NETSDK1141: Unable to resolve the .NET SDK version as specified in the global.json located at {0}.` and `NETSDK1145: … remove global.json if it specifies a certain SDK version` are in the installed `dotnet.dll`. Comments are supported (global-json page) and `TestCommandDefinition.Create()` sets `ReadCommentHandling.Skip`, and it sets `AllowDuplicateProperties = false` and `AllowTrailingCommas = false`, and a duplicate key or trailing comma makes `dotnet test` throw where the muxer tolerates it.

## [02]-[MISE]

Source: `jdx/mise` `src/plugins/core/dotnet.rs` at tag `v2026.9.1`. Parsed fields: exactly one. `_parse_idiomatic_file` deserializes `struct GlobalJson { sdk: Option<GlobalJsonSdk> }` and `struct GlobalJsonSdk { version: String }`, returns `eyre!("no sdk.version found in {}", path.display())` when the `sdk` object is absent, an empty list when `version` is empty, and otherwise `vec![sdk.version]`. `rollForward`, `allowPrerelease`, `paths`, `errorMessage`, `msbuild-sdks`, `test`, and `workloadVersion` are invisible to mise.

Registration: `registry/dotnet.toml` has `idiomatic_files = ["global.json"]`, and `docs/configuration.md` lists the dotnet row as `global.json`. Idiomatic files are off by default: `docs/configuration.md` line 556, "In mise, these are disabled by default, see <https://github.com/jdx/mise/discussions/4345> for rationale", and `settings.toml` defines `idiomatic_version_file_enable_tools` with `default = []` and the description "Specific tools to enable idiomatic version files for". mise's own `e2e/core/test_dotnet` writes a `global.json` with `sdk.version` 8.0.408, runs `mise settings set idiomatic_version_file_enable_tools dotnet`, and asserts `mise x -- dotnet --version` prints `8.0.408` with no `[tools]` entry.

Precedence: `src/config/mod.rs` `Config::load` builds the config filename list as `idiomatic_files.keys().chain(DEFAULT_CONFIG_FILENAMES)` (lines 312-316) and later files win (line 1816, "later wins, matching LOCAL_CONFIG_FILENAMES ordering"), and a `[tools] dotnet` entry in `mise.toml` overrides `global.json` in the same directory when both exist.

Where the setting belongs: `src/config/settings.rs` `IdiomaticVersionFileSettings::resolve_from(root, …)` loads settings from the config root, and `src/config/mod.rs` lines 1166-1177 resolve those settings per root before detecting an idiomatic file. `[settings]` is a `mise.toml` section (`docs/configuration.md` line 238), and `[settings] idiomatic_version_file_enable_tools = ["dotnet"]` in Rasm's `mise.toml` enables the file for the repository without a machine-level `mise settings set`.

Installing: `install_version_` downloads `dotnet-install.sh` and runs it with `--install-dir <dir> --version <exact version> --no-path`, driven by the resolved `ToolVersion`, whether that version came from `[tools]` or from `_parse_idiomatic_file`. `docs/lang/dotnet.md` shows multi-version `[tools] dotnet = ["9", { version = "8.0.14", runtime = "dotnet" }]`. `exec_env` sets `DOTNET_ROOT` (the shared root, or the isolated install path when `dotnet.isolated` is on) and `DOTNET_MULTILEVEL_LOOKUP=0`, with `DOTNET_CLI_TELEMETRY_OPTOUT` when `dotnet.cli_telemetry_optout` is set.

With more than one SDK installed in the default non-isolated mode, every version lands in one `DOTNET_ROOT`, and `dotnet --list-sdks` shows all of them (`docs/lang/dotnet.md`). mise puts one `dotnet` muxer on `PATH` and selects nothing among them, and selection among 8.0.424, 9.0.317, and 10.0.400 is the muxer's job with global.json as its only input. mise's install-time check `test_dotnet` asserts that the requested version appears in `dotnet --list-sdks` and never asserts what a plain `dotnet` resolves to.

## [03]-[FIELD_MOBILITY]

`sdk.version` moves nowhere. mise reads it from `global.json` itself for installing (`dotnet.rs` `_parse_idiomatic_file`, `install_version_`), the muxer never reads `mise.toml` for selecting, and no MSBuild file can hold it because hostfxr selects the SDK before MSBuild starts.

`sdk.rollForward` and `sdk.allowPrerelease` move nowhere, mise parses `sdk.version` alone (`dotnet.rs` `GlobalJsonSdk`), and MSBuild never reads them. `sdk.paths` and `sdk.errorMessage` move nowhere (global-json page, .NET 10 SDK), and `sdk.workloadVersion` moves nowhere (dotnet-workload-sets page).

`msbuild-sdks` never moves to `mise.toml` and moves into a project as `Sdk="Id/1.2.3"`, `<Sdk Name Version>`, or `<Import Sdk Version>` (how-to-use-project-sdk page).

`test.runner` moves nowhere, and the CLI reads it at parser construction, before evaluation (`TestCommandDefinition.cs`, release/10.0.4xx).

`Directory.Build.props` can host none of them. MSBuild evaluates it after the muxer chooses an SDK and after `dotnet test` chooses its command shape.

## [04]-[TEST_PROJECTS]

SDK 10 has two modes (unit-testing-with-dotnet-test page): VSTest mode is "the default mode for dotnet test and was the only mode available before the .NET 10 SDK", and MTP mode was "Introduced with the .NET 10 SDK" and "exclusively supports test applications built with MTP".

`TestingPlatformDotnetTestSupport` is the older mechanism belonging to VSTest mode: "you can run MTP projects in dotnet test VSTest mode by using the Microsoft.Testing.Platform.MSBuild package … enabled by setting the `TestingPlatformDotnetTestSupport` MSBuild property to true (it's false by default)". The same page: "Running MTP projects under VSTest mode is considered legacy … The support of running under this mode will be removed in MTP version 2 if run with .NET 10 SDK", and migration step 2 is "Remove `TestingPlatformDotnetTestSupport` MSBuild property, as it's no longer required." Rasm pins `Microsoft.Testing.Platform` 2.3.3 and `Microsoft.Testing.Platform.MSBuild` 2.3.3 in `Directory.Packages.props` (`Transitive Pinning` group, lines 848-849): MTP v2 on the 10 SDK, the combination where the VSTest-mode path is removed.

The `Testing` group (`Directory.Packages.props` lines 767-781) is MTP-native: `xunit.v3.mtp-v2` 4.0.0 ("Developer test framework packaged for Microsoft Testing Platform v2"), `coverlet.MTP` 10.0.1, and `Microsoft.Testing.Extensions.CrashDump`, `HangDump`, `TrxReport` 2.3.3. No root manifest names VSTest.

`tests/dotnet/support/Rasm.TestSupport.csproj` is the only project under `tests/`. It is a helper library (`CsCheck`, `LanguageExt.Core`, `Microsoft.Extensions.TimeProvider.Testing`, `Thinktecture.Runtime.Extensions`, `xunit.v3.assert`, `xunit.v3.extensibility.core`) with no `xunit.v3.mtp-v2` reference, no MTP entry point, and no test application role. `Directory.Build.targets` line 26 injects `<Using Include="Xunit">` for any project referencing `xunit.v3.mtp-v2` or `xunit.v3.assert`, where the real test applications will pick up MTP. Neither `Directory.Build.props` nor `Directory.Build.targets` sets `TestingPlatformDotnetTestSupport`, `EnableMSTestRunner`, or `IsTestProject`, consistent with MTP mode. Extension caveat: `--report-trx`, `--hangdump`, and `--coverage` each require the matching extension package referenced by every targeted test application, or MTP exits with code 5 (dotnet-test-mtp page). Every test project must use MTP once the field is set, and Rasm satisfies that and keeps satisfying it as test applications are added.

## [05]-[VERDICT]

`global.json` stays as it is:

```json
{
    "sdk": {
        "version": "10.0.400",
        "rollForward": "disable"
    },
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

Without `test.runner`, `TestCommandDefinition.Create()` takes the null-name branch and returns `new VSTest()`, `dotnet test` runs VSTest mode against MTP v2 test applications, the path removed for MTP v2 on the .NET 10 SDK, and that path needs `TestingPlatformDotnetTestSupport=true` with a `--` separator as well. No property, environment variable, or mise setting substitutes.

Without `sdk.rollForward` (keeping `version`), the default becomes `patch`, 10.0.4xx patch drift is accepted silently, and the page's lock-file footnote argues for `disable`.

Without `sdk.version` (keeping the file), `latestMajor` semantics apply. The default resolves to 10.0.400 here, and the first .NET 11 or 10.0.5xx SDK on any machine or CI image silently changes compiler, analyzers, and SDK targets.

Without the file, the highest installed SDK and VSTest mode apply, and the 10.0.400 an empty directory resolves by height alone is correct by accident of ordering.

`sdk.paths` and `sdk.errorMessage` add nothing while the SDK is on `PATH` through mise's shared `DOTNET_ROOT` with `DOTNET_MULTILEVEL_LOOKUP=0`.

mise configuration, as the plan decides: Rasm's `mise.toml` sets `[settings] idiomatic_version_file_enable_tools = ["dotnet"]` and no `dotnet` entry under `[tools]`. The version is stated once, in `global.json`. mise reads `sdk.version` from that file (`_parse_idiomatic_file` returns `vec![sdk.version]`), resolves it to `dotnet@10.0.400`, and `install_version_` runs `dotnet-install.sh --version 10.0.400`. The muxer that mise puts on `PATH` then reads the same file and selects 10.0.400 among the installed SDKs with `rollForward: disable`.

The objection that enabling the idiomatic reader "adds a second consumer that understands only `sdk.version` and errors on a file lacking it" does not apply: the file cannot lack `sdk.version`, because the muxer needs it, and a file that has it never reaches the `eyre!` branch. A `[tools] dotnet = "10.0.400"` entry states the version twice and, by the precedence in `config/mod.rs`, silently outranks `global.json` when the two differ. The source proves the plan's configuration works and does not prove the alternative necessary.
