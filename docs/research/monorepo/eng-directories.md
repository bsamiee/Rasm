<!-- Source for monorepo-build-infrastructure [04]-[ENGINEERING_DIRECTORY], nothing integrated yet -->
# [ENG_DIRECTORIES]

## [00]-[RASM_BASELINE]

The Rasm working tree on 2026-09-03 holds under `eng/`:
- `eng/project.json` — one Nx project named `eng` with a single `provision` target, `cache: false`, `parallelism: false`
- The `provision` target runs `uv run python -m eng.scripts.provision`
- `eng/scripts/provision.py` — provisioning script, runs `dotnet tool restore` and places vcpkg at a pinned commit under `.cache/vcpkg`
- The same script places EnergyPlus 25.2.0 and the DuckDB extension and sqlite-vec archives
- `eng/scripts/stage.py` — native staging script, one entry point for every library, writes staged trees under `.artifacts/native/<library>/stage`
- `eng/scripts/gen_gmsh_bindings.py` — code generation for the Gmsh managed bindings, called by `stage.py`
- The generated bindings are written to the staged `managed/` directory and not committed
- `eng/native/<library>/` — one version manifest per native library
- The manifests are `vcpkg.json` (blosc2, ffmpeg, gmsh, lcms2, z3), `extensions.json` (duckdbextensions), `source.json` (emgucv)
- The sqlitevec manifest is `loadable.json`
- `eng/native/Rasm.Native.*/`, `eng/native/Rasm.Gmsh`, `eng/native/Rasm.Z3` — one packaging `.csproj` per library
- `eng/native/Directory.Build.props` — isolates the packaging projects from the root build files
- The same file sets `ArtifactsPath` under `.artifacts/native/msbuild`, `TargetFramework` netstandard2.0, `IncludeBuildOutput` false
- The same file sets `EnableDefaultItems` false and `DeterministicTimestamp`
- `eng/native/Directory.Build.targets` — packs `$(StageRoot)runtimes/**` and `contentFiles/**`, with four guard targets
- The guard targets are `EnsureStagedNativeLibraries`, `EnsureStagedManagedSources`, `EnsureManifestVersionMatch`, `EnsureCentralPackageVersionMatch`
- `eng/native/Directory.Packages.props` — sets `ManagePackageVersionsCentrally` false for the packaging subtree
- `eng/native/_._` — empty placeholder packed to `lib/$(TargetFramework)/`

Root files hold concerns other repositories put in `eng/`: `Directory.Build.props` (artifacts path, analyzer package references, `TreatWarningsAsErrors`, `RestorePackagesWithLockFile`, `ContinuousIntegrationBuild`), `Directory.Build.targets` (171 lines: project-role policy errors RASM0001 to RASM0005, global usings by package presence, Rhino host references, native asset repair targets), `Directory.Packages.props`, `global.json` (SDK 10.0.400, `rollForward` disable, `test.runner` Microsoft.Testing.Platform), `.config/dotnet-tools.json` (dotnet-stryker 4.16.0), `Workspace.slnx`, `stryker-config.json` (names `Workspace.slnx`), `.mcp.json` (`dotnet dnx RoslynCodeLens.Mcp Workspace.slnx`), and `nx.json`. Every root file the `README.md` layout lists is present in the working tree, `Workspace.slnx`, `tsconfig.json`, `tsconfig.base.json`, `vite.config.ts`, and `vitest.config.ts` included.

Rasm has no version-computation file, no `eng/` MSBuild props or targets shared with the main tree, no test-infrastructure directory under `eng/`, and no CI helper scripts under `eng/`. No file sets `Version`, `VersionPrefix`, or `PackageVersion`, and RASM0002 rejects a project that sets one. The packaging projects under `eng/native/` match their `Version` against the library manifest and against `Directory.Packages.props`.

## [01]-[METHOD]

Directory listings come from the GitHub MCP `get_file_contents` on each repository's default branch on 2026-09-03, one level at a time. Activity is the date of the newest commit on the default branch, read with `list_commits` on the same date. No archived repository, no repository without a 2026 commit, and no template repository is in the survey. Signing, scanning, and compliance files appear in the listings and are left out of the tables.

## [02]-[MICROSOFT_REPOSITORIES]

Newest commit on the default branch: runtime 2026-09-03, sdk 2026-09-03, msbuild 2026-09-02, roslyn 2026-09-03, aspire 2026-09-03, efcore 2026-09-02, maui 2026-09-03, yarp 2026-08-11.

### [02.1]-[DOTNET_RUNTIME]

`eng/` at <https://github.com/dotnet/runtime/tree/main/eng> holds 64 files and 9 directories:
- Version pinning — `Version.Details.xml`, `Version.Details.props`, `Versions.props` (Arcade, darc, Maestro)
- Shared MSBuild props — `Build.props`, `Subsets.props`, `RuntimeIdentifier.props`, `OSArch.props` (MSBuild, Arcade SDK)
- Shared MSBuild targets — `SubsetValidation.targets`, `Analyzers.targets`, `references.targets`, `resources.targets`, `resolveContract.targets`
- Further shared targets — `packaging.targets`, `versioning.targets`, `targetingpacks.targets`, `liveBuilds.targets`, `outerBuild.targets`
- Further shared targets — `BeforeTargetFrameworkInference.targets`, `codeOptimization.targets`
- Analyzer configuration — `CodeAnalysis.src.globalconfig`, `CodeAnalysis.test.globalconfig`, `analyzers/` (Roslyn `.globalconfig`)
- Native and third-party builds — `native/`, `wasm/`, `AcquireEmscriptenSdk.proj`, `AcquireWasiSdk.targets` (CMake, bash, MSBuild)
- Further native builds — `coredistools.targets`, `DiaSymReaderNative.targets`, `docker/`
- Code generation — `generators.targets`, `generatorProjects.targets`, `intellisense.targets` (MSBuild, Roslyn source generators)
- Further code generation — `ILLink.Substitutions.Resources.template`, `MultiTargetRoslynComponent.targets.template`
- Further code generation — `regenerate-third-party-notices.proj`
- Test infrastructure — `testing/` with 53 entries (xunit, XHarness, Helix)
- `testing/` entries — `tests.props`, `tests.targets`, per-platform `tests.android.targets`, `tests.browser.targets`, `tests.wasm.targets`
- Further `testing/` entries — `RunnerTemplate.sh`, `coverage.targets`, `runsettings.targets`, `.runsettings`, `xunit/`, `performance/`
- Packaging and publishing — `packaging.targets`, `Publishing.props` (Arcade SDK)
- CI helpers — `build.sh`, `build.ps1`, `configure-toolset.sh`/`.ps1`, `pipelines/`, `restore/`, `formatting/` (bash, PowerShell, Azure Pipelines)
- Further CI helpers — `collect_vsinfo.ps1`, `extract-for-crossdac.ps1`
- Solution generation — `slngen.targets`, `slngen.template.proj` (Microsoft.VisualStudio.SlnGen)
- Shared Arcade files — `common/`, copied from dotnet/arcade by automation

`eng/common/README.md` states the rule for that directory: "The files in this directory are shared by all Arcade repos and managed by automation. If you need to make changes to these files, open an issue or submit a pull request to https://github.com/dotnet/arcade first." (<https://github.com/dotnet/runtime/blob/main/eng/common/README.md>)

`eng/common/` holds `dotnet-install.sh`/`.ps1`, `tools.sh`/`.ps1`, `darc-init.sh`/`.ps1`, `init-tools-native.sh`/`.ps1`, `helixpublish.proj`, `post-build/`, `templates/`, `templates-official/`, `core-templates/`, `cross/`, `vmr-sync.sh`, and `SetupNugetSources.sh`. `darc-init`, `helixpublish.proj`, `post-build/`, `internal/`, `templates-official/`, `vmr-sync.sh`, and `internal-feed-operations.sh` are Microsoft-internal: darc and Maestro dependency flow, Helix test distribution, the internal package feeds, and the VMR. `dotnet-install.sh` is the public installer script.

### [02.2]-[DOTNET_SDK]

`eng/` at <https://github.com/dotnet/sdk/tree/main/eng>:
- Version pinning — `Version.Details.xml`, `Version.Details.props`, `Versions.props`, `ManualVersions.props`, `Packages.props`, `dependabot/`
- Shared MSBuild — `Build.props`, `MSBuildTaskAuthoringAnalyzer.props`, `SourcePackage.editorconfig` and two framework-specific siblings
- Toolset acquisition — `restore-toolset.sh`/`.ps1`, `configure-toolset.sh`/`.ps1`, `dotnetup-shared.sh`/`.ps1`
- Further toolset acquisition — `enable-preview-sdks.ps1`, `sdk-tools.sh`/`.ps1`
- Local dogfooding — `dogfood.sh`, `dogfood.ps1`, `dogfood.cmd`
- Vendored source tracking — `vendored-files.json`, `vendored-files.md`
- Test infrastructure — `test-configuration.json`, `BuildConfiguration/`
- CI helpers — `build.sh`, `build.ps1`, `pipelines/`, `Badge.proj`, `version_badge.svg`, `Get-BranchMirrorStatus.ps1`
- Further CI helpers — `gather-otel.sh`/`.ps1`, `print-full-msbuild-path.ps1`
- Publishing — `Publishing.props`

### [02.3]-[DOTNET_MSBUILD]

`eng/` at <https://github.com/dotnet/msbuild/tree/main/eng>:
- Version pinning — `Version.Details.xml`, `Version.Details.props`, `Versions.props`, `Tools.props`, `dependabot/`
- Shared MSBuild — `Build.props`, `packaging.targets`, `BootStrapMsBuild.props`, `BootStrapMsBuild.targets`
- Analyzer configuration — `Common.globalconfig`, `Common.Test.globalconfig`, `config/`
- Test infrastructure — `process-coverage.sh`/`.ps1`, `restore-dotnet-coverage.sh`/`.ps1`
- CI helpers — `build.sh`, `build.ps1`, `restore-toolset.sh`/`.ps1`
- Publishing — `Publishing.props`

`eng/Versions.props` holds `VersionPrefix`, `PreReleaseVersionLabel`, `AssemblyVersion`, `PackageValidationBaselineVersion`, named version properties for its dependencies, and a target `OverrideArcadeFileVersion` that rewrites `FileVersion` because Arcade cannot express a fixed `AssemblyVersion` beside a varying `FileVersion` (<https://github.com/dotnet/msbuild/blob/main/eng/Versions.props>).

`eng/Build.props` replaces Arcade's globbed `ProjectToBuild` item with one entry, `MSBuild.slnx`, because Arcade otherwise builds all three solutions at once and hits locked-file errors (<https://github.com/dotnet/msbuild/blob/main/eng/Build.props>).

### [02.4]-[DOTNET_ROSLYN]

`eng/` at <https://github.com/dotnet/roslyn/tree/main/eng>:
- Version pinning — `Version.Details.xml`, `Version.Details.props`, `Versions.props`, `Packages.props`, `InternalTools.props`
- Shared MSBuild — `Build.props`, `targets/`, `config/`, `eng.sln`
- Code generation — `generate-compiler-code.cs`, `generate-compiler-code.cmd`, `generate-vssdk-versions.csx`, `ensure-sources-synced.cs`
- Build correctness checks — `test-build-correctness.ps1`, `test-determinism.ps1`, `test-rebuild.ps1`
- Repository policy checks — `todo-check.ps1`, `validate-code-formatting.ps1`, `validate-rules-missing-documentation.ps1`
- Further policy checks — `validate-benchmarks.ps1`, `validate-roslyn-sdk-samples.ps1`
- Bootstrap — `make-bootstrap.ps1`, `make-bootstrap.cmd`, `build-utils.ps1`
- Test infrastructure — `prepare-tests.sh`/`.ps1`
- CI helpers — `build.sh`, `build.ps1`, `cibuild.sh`, `pipelines/`, `evaluate-changed-paths.sh`, `setup-pr-validation.ps1`
- Further CI helpers — `publish-assets.ps1`, `isolated/`

`eng/generate-compiler-code.cs` and `eng/ensure-sources-synced.cs` are file-based C# programs, run without a project, which the .NET 10 SDK supports.

### [02.5]-[DOTNET_ASPIRE]

`eng/` at <https://github.com/dotnet/aspire/tree/main/eng>:
- Version pinning — `Version.Details.xml`, `Versions.props`
- Shared MSBuild — `Build.props`, `AfterSolutionBuild.targets`, `ReplaceText.targets`, `NullablePolyfill.targets`, `OuterPreBuild.proj`
- Test infrastructure — `Testing.props`, `Testing.targets`, `testing/`, `Xunit3/`, `test-configuration.json`, `test-retry-patterns.json`
- Further test infrastructure — `CodeCoverage.config`, `OuterloopTestRunsheetBuilder/`, `QuarantinedTestRunsheetBuilder/`
- Further test infrastructure — `TestEnumerationRunsheetBuilder/`, `SpecializedTestRunsheetBuilderBase.targets`
- Packaging and distribution — `Bundle.proj`, `clipack/`, `dashboardpack/`, `dcppack/`, `homebrew/`, `nix/`, `winget/`
- Further packaging — `generate-catalog.ps1`, `find-missing-packages.sh`
- CI helpers — `build.sh`, `build.ps1`, `restore-toolset.sh`/`.ps1`, `pipelines/`, `github-ci/`, `scripts/`

Aspire is the one repository in the survey with `homebrew/`, `nix/`, and `winget/` directories under `eng/`, and the one with a `test-retry-patterns.json` and three separate runsheet-builder directories.

### [02.6]-[DOTNET_EFCORE]

`eng/` at <https://github.com/dotnet/efcore/tree/main/eng> is the smallest Microsoft `eng/` in the survey: `Version.Details.xml`, `Version.Details.props`, `Versions.props`, `Publishing.props`, `common/`, `Tools/`, `testing/`, `helix.proj`, `efcore.coverage.xml`, `aggregate-azdo-tests.ps1`. It holds no `build.sh`, no `build.ps1`, and no `Build.props`.

### [02.7]-[DOTNET_MAUI]

`eng/` at <https://github.com/dotnet/maui/tree/main/eng>:
- Version pinning — `Version.Details.xml`, `Versions.props`, `Versions.targets`, `NuGetVersions.targets`, `Tools.props`
- Shared MSBuild — `Build.props`, `Environment.Build.props`, `SourceLink.Build.props`, `ReplaceText.targets`, `optimizationData.targets`
- API policy — `BannedApis.targets`, `BannedSymbols.txt`
- Assembly merging — `ILRepack.targets`, `ILRepack.exe`
- Project set — `Microsoft.Maui.Packages.slnf`, `Microsoft.Maui.Packages-mac.slnf`, `Microsoft.Maui.Samples.slnf`
- Native and third-party builds — `AndroidX.targets`, `ingest-maven-deps.sh`, `init.gradle`, `provisioning/`
- Test infrastructure — `helix.proj`, `helix_xharness.proj`, `devices/`, `test-configuration.json`
- CI helpers — `build.sh`, `build.ps1`, `configure-toolset.sh`/`.ps1`, `pipelines/`, `automation/`, `scripts/`, `cake/`

maui is the one repository in the survey that keeps an `eng/cake/` directory beside Arcade, and the one that checks a prebuilt executable into `eng/` (`ILRepack.exe`).

### [02.8]-[DOTNET_YARP]

`eng/` at <https://github.com/dotnet/yarp/tree/main/eng> holds `Build.props`, `Versions.props`, `Version.Details.xml`, `Publishing.props`, `CodeAnalysis.src.globalconfig`, `CodeAnalysis.test.globalconfig`, `common/`, `yarpapppack/`. The root, not `eng/`, holds `TFMs.props`, `activate.sh`, `build.sh`, `pack.sh`, `restore.sh`, `test.sh`, and `YARP.slnx`. The newest commit on `main`, 2026-08-11, is an automated Arcade dependency update from dotnet-maestro.

## [03]-[NON_MICROSOFT_REPOSITORIES]

### [03.1]-[APP_VNEXT_POLLY]

`eng/` at <https://github.com/App-vNext/Polly/tree/main/eng>, newest commit 2026-08-28, has no `common/`, no `Version.Details.xml`, and no Arcade import, and every file in it was written for this repository:
- Shared MSBuild by project role — `Common.props`, `Common.targets`, `Library.targets`, `Test.targets`, `Benchmark.targets` (MSBuild)
- Further shared MSBuild — `Analyzers.targets`
- Version computation — `MinVer` package reference in `eng/Common.props` with `MinVerMinimumMajorMinor` 8.7 (MinVer)
- Analyzer configuration — `analyzers/` (Roslyn)
- Mutation testing — `stryker-config.json` (Stryker.NET)
- Release automation — `bump-version.ps1`, `update-changelog.ps1`, `update-baselines.ps1` (PowerShell)
- Build driver — `cake.tool` 6.2.0 pinned in `.config/dotnet-tools.json` (Cake as a file-based C# app)

The whole root `Directory.Build.props` is one import, `<Import Project="$(MsBuildThisFileDirectory)eng/Common.props" />`, with four properties: `ManagePackageVersionsCentrally`, `UseArtifactsOutput`, and a CI-conditioned `ContinuousIntegrationBuild` and `Deterministic` (<https://github.com/App-vNext/Polly/blob/main/Directory.Build.props>).

The root `Directory.Build.targets` imports `eng/Common.targets` and then selects the role file by property: `<Import Project="$(MsBuildThisFileDirectory)eng/$(ProjectType).targets" Condition="$(ProjectType) != ''" />`. A project sets `<ProjectType>Library</ProjectType>` or `Test` or `Benchmark` and picks up that file (<https://github.com/App-vNext/Polly/blob/main/Directory.Build.targets>).

`eng/Library.targets` holds package metadata, `EnablePackageValidation` with `PackageValidationBaselineVersion` 8.5.2, `PublishRepositoryUrl`/`IncludeSymbols`/`SymbolPackageFormat`, `EmbedUntrackedSources`, `ChecksumAlgorithm` SHA256, a `Microsoft.CodeAnalysis.PublicApiAnalyzers` reference with `PublicAPI.Shipped.txt`/`PublicAPI.Unshipped.txt` as `AdditionalFiles`, and a target `SetNuGetPackageOutputs` that writes `package-names` and `package-version` to `$(GITHUB_OUTPUT)` after `Pack` (<https://github.com/App-vNext/Polly/blob/main/eng/Library.targets>).

The build driver is Cake, and not as a script engine: `build.ps1` runs `dotnet tool restore` and then `dotnet $Script -- "--target=$Target" ...` with `$Script` defaulting to `cake.cs`, and the Cake build is a .NET 10 file-based app (<https://github.com/App-vNext/Polly/blob/main/build.ps1>).

`eng/Test.targets` supplies the test package set (xunit, Shouldly, NSubstitute, coverlet.msbuild, ReportGenerator, GitHubActionsTestLogger, JunitXml.TestLogger), global `Using` items for the test role, coverlet properties, and a `GenerateCoverageReports` target that appends a markdown coverage summary to `$(GITHUB_STEP_SUMMARY)` through an inline `RoslynCodeTaskFactory` task (<https://github.com/App-vNext/Polly/blob/main/eng/Test.targets>).

`.config/dotnet-tools.json` pins nine tools, among them `cake.tool`, `docfx`, `dotnet-stryker` 4.16.0, `markdownsnippets.tool`, and two package validators.

### [03.2]-[OPENTELEMETRY_DOTNET]

`build/` at <https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/build>, newest commit 2026-09-02:
- Shared MSBuild — `Common.props`, `Common.targets`, `Common.prod.props`, `Common.nonprod.props`
- Target framework sets — eight `TargetFrameworks*` properties in `Common.props`
- Framework bounds — `NetFrameworkMinimumSupportedVersion` and `NetFrameworkSupportedVersions`
- The eight properties — `TargetFrameworksForLibraries`, `TargetFrameworksForLibrariesExtended`, `TargetFrameworksForPrometheusAspNetCore`
- Further properties — `TargetFrameworksRequiringSystemTextJsonDirectReference`, `TargetFrameworksForAspNetCoreTests`
- Further properties — `TargetFrameworksForAotCompatibilityTests`
- Further properties — `TargetFrameworksForDocs`, `TargetFrameworksForTests`
- Analyzer configuration — `OpenTelemetry.prod.ruleset`, `OpenTelemetry.prod.loose.ruleset`, `OpenTelemetry.test.ruleset`, `stylecop.json`
- Further analyzer configuration — `BannedSymbols.txt`, `GlobalAttrExclusions.txt`
- Traversal projects — `OpenTelemetry.proj`, `UnstableCoreLibraries.proj`
- Test infrastructure — `CodeCoverage.runsettings`, `xunit.runner.json`, `scripts/tests/`
- Further test infrastructure — `docker-compose.net8.0.yml`, `docker-compose.net9.0.yml`, `docker-compose.net10.0.yml`
- Release automation — `scripts/prepare-release.psm1`, `scripts/post-release.psm1`, `scripts/update-changelogs.ps1`
- Further release automation — `scripts/report-unreleased-changes.ps1`, `RELEASING.md`
- Public API — `scripts/finalize-publicapi.ps1`
- Correctness checks — `scripts/test-aot-compatibility.ps1`, `scripts/test-threadSafety.ps1`, `scripts/sanitycheck.py`
- Documentation — `docfx.cmd`

`build/Common.props` names one property per target framework set rather than repeating framework lists in projects, and declares `AnalysisLevel` `latest-All`, `EnforceCodeStyleInBuild`, and `TreatWarningsAsErrors` under Release alone (<https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/build/Common.props>).

### [03.3]-[AVALONIAUI_AVALONIA]

<https://github.com/AvaloniaUI/Avalonia/tree/master/build>, newest commit 2026-09-02, splits the concerns into `build/` and `nukebuild/`.

`build/` holds MSBuild fragments imported by projects, each named for one concern: `Base.props`, `TargetFrameworks.props`, `NullableEnable.props`, `TrimmingEnable.props`, `NetAnalyzers.props`, `DevAnalyzers.props`, `SourceLink.props`, `SourceGenerators.props`, `EmbedXaml.props`, `CoreLibraries.props`, `ReferenceCoreLibraries.props`, `ExternalConsumers.props`, `SampleApp.props`, `SkiaSharp.props`, `HarfBuzzSharp.props`, `MicroCOM.props`, `XUnit.props`, `UnitTests.NetCore.targets`, `UnitTests.NetFX.props`, `AnalyzerProject.targets`, `BuildTargets.targets`, `DevSingleProject.targets`, `SharedVersion.props`, `AvaloniaPublicKey.props`, `ApiCompatAttributeExcludeList.txt`, `xunit.runner.mono.json`.

`nukebuild/` holds the build program: `_build.csproj`, `Build.cs`, `BuildParameters.cs`, `RefAssemblyGenerator.cs`, `ApiDiffHelper.cs`, `XamlCompilationVerifier.cs`, `BuildTasksPatcher.cs`, `numerge.json`. `.nuke/` at the root marks the Nuke root, and `Avalonia.slnx` and `Directory.Packages.props` sit at the root.

### [03.4]-[JASPERFX_MARTEN]

`build/` at <https://github.com/JasperFx/marten/tree/master/build>, newest commit 2026-09-02, is a Nuke program: `build.csproj` referencing `Nuke.Common`, `Npgsql`, `JasperFx`, and `Bobcat.Supervisor`, with `build.cs`, `Configuration.cs`, `SupervisedTests.cs`, `RetryLedger.cs`, and its own `Directory.Build.props` and `Directory.Build.targets` (<https://github.com/JasperFx/marten/blob/master/build/build.csproj>).

`build/Directory.Build.props` is an empty `<Project>` with the comment "This file prevents unintended imports of unrelated MSBuild files" and the parent import left commented out. It is the same device as Rasm's `eng/native/Directory.Build.props` (<https://github.com/JasperFx/marten/blob/master/build/Directory.Build.props>).

`SupervisedTests.cs` and `RetryLedger.cs` appear in no other repository in the survey. The root holds `Directory.Packages.props` and a `.config/` directory.

### [03.5]-[QUARTZNET]

`build/` at <https://github.com/quartznet/quartznet/tree/main/build>, newest commit 2026-09-03, is a build program split by concern into partial classes of one `Build` class, driven by the `Fallout` packages (`Fallout.Common.CI.GitHubActions`, `Fallout.Components`) with `.fallout/` at the repository root: `Build.cs`, `Build.CI.GitHubActions.cs`, `Build.Publish.cs`, `Build.Docs.cs`, `Build.Docs.Snippets.cs`, `Build.Docs.LogEvents.cs`, `Build.DatabaseSchema.cs`, `Build.DatabaseMigrations.cs`, `Build.DatabaseMigrations.Scripts.cs`, `_build.csproj`, and its own `Directory.Build.props` and `Directory.Build.targets`. `tools/NuGet/` holds packaging inputs, and the root holds a `.config/` directory, `Directory.Packages.props`, and `Quartz.slnx`.

`Build.CI.GitHubActions.cs` declares every workflow as an attribute: `[GitHubActions(...)]`, `[DatabaseGitHubActions(...)]`, and `[DatabaseIntegrationGitHubActions(...)]` on `public partial class Build` name each workflow, its runner images, its triggers and path filters, its invoked targets, its timeout, and its permissions, and two attribute subclasses in the same file define presets for the per-database workflows (<https://github.com/quartznet/quartznet/blob/main/build/Build.CI.GitHubActions.cs>).

### [03.6]-[DOTNET_SILK_NET]

`build/` at <https://github.com/dotnet/Silk.NET/tree/main/build>, newest commit 2026-08-02, sits in the dotnet organization with no Arcade, no `common/`, and no `Version.Details.xml`:
- Build program — `nuke/` with `Silk.NET.NUKE.csproj`, `Build.cs`, `Build.Core.cs`, `Build.Generation.cs`, `Build.NuGet.cs`, `Build.Packaging.cs`
- Further build program files — `Build.PublicApi.cs`, `Build.SolutionGenerator.cs`, `Build.Website.cs`, `Build.Support.cs`, `Build.ReviewHelpers.cs`
- Shared MSBuild — `props/common.props`, `props/bindings.props`
- Code generation input — `csharp_typemap.json`, `gl_typemap.json`, `dx_typemap.json`, `khronos_typemap.json`, `comments/`, `include/`
- Generator cache — `cache/` with 31 gzipped JSON files, one per API (`vulkan.json.gz`, `gl.json.gz`, `d3d12.json.gz`, `webgpu.json.gz`, and 27 more)
- The generator cache is checked into the repository
- Native builds — `cmake/`, `submodules/`, `Install-WindowsSDK.ps1`
- Platform checks — `utilities/android_probe.proj`, `utilities/AndroidManifest.xml`

The checked-in generator cache and the typemap files are the committed inputs of a binding generator, next to the generator target that reads them, the same role `eng/native/<library>/*.json` plays for `eng/scripts/`.

### [03.7]-[UNOPLATFORM_UNO]

`build/` at <https://github.com/unoplatform/uno/tree/master/build>, newest commit 2026-09-03:
- Build program — `Uno.UI.Build.csproj`, `build.sln`
- CI — `ci/`
- API compatibility — `PackageDiffIgnore.xml`, `run-api-sync-tool.cmd`
- SDK acquisition — `Install-Tizen.ps1`, `Install-WindowsSdkISO.ps1`
- Packaging — `nuget/`, `filters/`
- Documentation — `run-doc-generation.cmd`
- Version computation — `version.json` at the repository root

`version.json` at the root is the Nerdbank.GitVersioning manifest, `version` `7.0-dev.{height}`, and the root holds a `.config/` directory (<https://github.com/unoplatform/uno/blob/master/version.json>).

### [03.8]-[ELASTIC_ELASTICSEARCH_NET]

`build/` at <https://github.com/elastic/elasticsearch-net/tree/main/build>, newest commit 2026-08-31, holds an F# build program: `build/scripts/scripts.fsproj` with `Targets.fs`, `Building.fs`, `Testing.fs`, `Benchmarking.fs`, `Documentation.fs`, `Versioning.fs`, `ReleaseNotes.fs`, `Tooling.fs`, `Paths.fs`, `Commandline.fs`, `ReposTooling.fs`, `XmlDocPatcher.fs`, with `build/profiling/`, `build/keys/`, and `strip-bom.sh`. The tool manifest sits at the repository root as `dotnet-tools.json` rather than under `.config/`, and pins four tools: `assembly-rewriter`, `assembly-differ`, `nupkg-validator`, `release-notes` (<https://github.com/elastic/elasticsearch-net/blob/main/dotnet-tools.json>).

### [03.9]-[XUNIT]

`tools/` at <https://github.com/xunit/xunit/tree/main/tools>, newest commit 2026-09-01, holds `tools/builder/`, a C# console program. `version.json` at the root is the Nerdbank.GitVersioning manifest, `version` `4.0.1-pre.{height}` (<https://github.com/xunit/xunit/blob/main/version.json>). `.config/` holds the dotnet tool manifest, `docfx/` holds the documentation build, and `xunit.slnx` is the solution.

### [03.10]-[COMMUNITYTOOLKIT_DOTNET]

`build/` at <https://github.com/CommunityToolkit/dotnet/tree/main/build>, newest commit 2026-03-24, the least active repository accepted, holds `Community.Toolkit.Common.props`, `Community.Toolkit.Common.targets`, and a package icon. `version.json` at the root is the Nerdbank.GitVersioning manifest, `.runsettings` sits at the root, and `dotnet.slnx` is the solution.

### [03.11]-[MESSAGEPACK_CSHARP]

`tools/` at <https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/master/tools>, newest commit 2026-09-03, holds seven PowerShell scripts that acquire and check the toolchain: `Install-DotNetSdk.ps1`, `Check-DotNetSdk.ps1`, `Check-DotNetRuntime.ps1`, `DotNetSdkVersion.ps1`, `Get-TempToolsPath.ps1`, `Set-EnvVars.ps1`, `Install-NuGetCredProvider.ps1`, driven by root `init.ps1`/`init.cmd`. The root holds `Directory.Build.rsp`, an MSBuild response file, `Directory.Packages.props`, and a `.config/` directory.

### [03.12]-[SERILOG]

<https://github.com/serilog/serilog>, newest commit 2026-07-31, has no `eng/`, `build/`, or `tools/` directory, the smallest arrangement in the survey. `Directory.Version.props` at the root holds the version, and `Build.ps1` at the root is the whole build.

### [03.13]-[DOTNET_ORLEANS]

<https://github.com/dotnet/orleans>, newest commit 2026-09-03, has no `eng/`. `build.ps1`, `common.ps1`, `Build.cmd`, `Test.cmd`, `TestAll.cmd`, `Parallel-Tests.ps1` sit at the root, with `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `Orleans.slnx`, and `.config/`.

### [03.14]-[ORCHARDCMS_ORCHARDCORE]

<https://github.com/OrchardCMS/OrchardCore>, newest commit 2026-09-02: `.scripts/` holds two tool projects (`assets-manager/`, `bloom/`), `tools/` holds repository tooling, and the TypeScript build sits at the root (`build.config.mjs`, `package.json`, `yarn.lock`, `eslint.config.mjs`, `tsconfig.json`). It is the one polyglot arrangement in the survey, and it keeps the JavaScript build at the root rather than under a build directory. `OrchardCore.slnx` and `Directory.Packages.props` sit at the root.

### [03.15]-[SIXLABORS_IMAGESHARP]

<https://github.com/SixLabors/ImageSharp>, newest commit 2026-08-24, the migration of the solution to `ImageSharp.slnx`. `shared-infrastructure` is a git submodule at the root holding the shared MSBuild files, and `ci-build.ps1`, `ci-test.ps1`, `ci-pack.ps1` sit at the root with `SixLabors.ImageSharp.props`. The shared build files are versioned in a separate repository and pinned here by submodule commit.

## [04]-[RECURRING_PRACTICES]

Counts are out of the 23 repositories surveyed: 8 Microsoft-run (runtime, sdk, msbuild, roslyn, aspire, efcore, maui, yarp) and 15 others (Polly, opentelemetry-dotnet, Avalonia, marten, quartznet, Silk.NET, uno, elasticsearch-net, xunit, CommunityToolkit/dotnet, MessagePack-CSharp, serilog, orleans, OrchardCore, ImageSharp). A count covers the repositories where the listing shows the entry:
- 15 of 23 — `build.sh` with `build.ps1` or `build.cmd` at the repository root or in the build directory, one entry point per shell
- 14 of 23 — a dedicated directory holding shared `.props`/`.targets` imported by product projects, separate from the root `Directory.Build.*`
- 11 — a `.slnx` solution: Avalonia, opentelemetry-dotnet, Polly, quartznet, orleans, yarp, OrchardCore, ImageSharp, xunit, CommunityToolkit, msbuild
- 10 of 23 — test infrastructure in the build directory: shared test `.props`/`.targets`, a `.runsettings`, a coverage or runner configuration
- 8 of 23 — Arcade: `eng/common/`, `eng/Version.Details.xml`, `eng/Versions.props`, every one of the 8 Microsoft-run, none of the other 15
- 8 of 23 — analyzer or style configuration in the build directory: `.globalconfig`, `.ruleset`, `stylecop.json`, `BannedSymbols.txt`
- 8 of 16 roots listed — central package management, a root `Directory.Packages.props`
- The central package management repositories — Avalonia, opentelemetry-dotnet, Polly, marten, orleans, OrchardCore, quartznet, MessagePack-CSharp
- 8 of 23 — public API tracking: an ApiCompat exclusion list, `PublicAPI.Shipped.txt`, a package-diff ignore file, a banned-API target
- `PackageValidationBaselineVersion` counts as public API tracking
- 7 of 23 — a compiled build program (a `.csproj` or `.fsproj`) in place of shell scripts
- The compiled build program repositories — Avalonia, marten, quartznet, Silk.NET, uno, xunit, elasticsearch-net
- 6 of 23 — pipeline or workflow definitions inside the build directory: runtime, sdk, roslyn, aspire, maui, uno
- 6 of 23 — code generation inputs or targets in the build directory: runtime, roslyn, Silk.NET, uno, maui, aspire
- 6 of 23 — release automation in the build directory (version bump, changelog, release notes, catalog)
- The release automation repositories — Polly, opentelemetry-dotnet, elasticsearch-net, aspire, quartznet, Silk.NET
- 6 of 23 — toolchain acquisition scripts (SDK, native tools) in the build directory: runtime, sdk, msbuild, aspire, maui, MessagePack-CSharp
- 4 of 23 — native or third-party library build scripting in the build directory: runtime, maui, Silk.NET, uno
- 4 — a `dotnet tool` manifest pinning repository tooling: Polly (`.config/dotnet-tools.json`, read), elasticsearch-net (root file, read)
- quartznet and xunit count by a `.config/` directory present, and a `.config/` directory is present in marten, uno, orleans, yarp, MessagePack-CSharp
- 3 of 23 — Nerdbank.GitVersioning `version.json` at the root: xunit, CommunityToolkit/dotnet, uno
- 2 of 23 — the build directory's own project isolated from the root build files by an empty `Directory.Build.props`: marten, quartznet
- 1 of 23 — MinVer: Polly, declared in `eng/Common.props`

The version-computation approaches divide the survey with no overlap: Arcade's `eng/Versions.props` with `Version.Details.xml` (8), Nerdbank.GitVersioning's `version.json` (3), and MinVer (1). The rest hold the version in an ordinary MSBuild property file (serilog's root `Directory.Version.props`, Avalonia's `build/SharedVersion.props`).

## [05]-[UNIQUE_PRACTICES]

- dotnet/runtime `eng/Subsets.props` and `eng/SubsetValidation.targets` let a build name a subset of the repository rather than building all of it
- dotnet/runtime `eng/slngen.targets` and `eng/slngen.template.proj` synthesize the solution by SlnGen from a template project rather than checking it in
- dotnet/sdk `eng/dogfood.sh`, `eng/dogfood.ps1`, and `eng/dogfood.cmd` open a shell that uses the SDK the repository just built
- dotnet/sdk `eng/vendored-files.json` and `eng/vendored-files.md` track source files copied in from other repositories for re-syncing
- dotnet/msbuild `eng/BootStrapMsBuild.props` and `eng/BootStrapMsBuild.targets` carry the file's own comment: "Construct a location of MSBuild bootstrap folder - to be used for deployment and for tests relying on bootstrapped MSBuild"
- dotnet/roslyn `eng/test-determinism.ps1`, `eng/test-rebuild.ps1`, and `eng/make-bootstrap.ps1` check that two builds of the compiler produce identical output, and build the compiler with itself
- dotnet/aspire `eng/homebrew/`, `eng/nix/`, and `eng/winget/` package the Aspire CLI for Homebrew, Nix, and winget beside the NuGet packages
- dotnet/aspire `eng/OuterloopTestRunsheetBuilder/`, `eng/QuarantinedTestRunsheetBuilder/`, and `eng/TestEnumerationRunsheetBuilder/` are three C# projects that build CI test runsheets, one of them for quarantined tests, with `eng/test-retry-patterns.json` as a retry-pattern file
- dotnet/maui `eng/init.gradle` and `eng/ingest-maven-deps.sh` put a Gradle init script and a Maven dependency ingest next to the .NET build
- dotnet/maui `eng/cake/` sits next to `eng/common`, and `eng/ILRepack.exe` is a checked-in executable
- dotnet/Silk.NET `build/cache/*.json.gz` and `build/*_typemap.json` are 31 committed gzipped API descriptions and four typemap files read by the `Build.Generation.cs` target
- AvaloniaUI/Avalonia `nukebuild/XamlCompilationVerifier.cs`, `nukebuild/RefAssemblyGenerator.cs`, and `build/numerge.json` make the build program verify XAML compilation and generate reference assemblies
- JasperFx/marten `build/SupervisedTests.cs` and `build/RetryLedger.cs` make the build program reference `Npgsql` and `Bobcat.Supervisor` and keep a retry ledger for the test run
- quartznet/quartznet `build/Build.CI.GitHubActions.cs` declares workflow triggers, runner images, invoked targets, timeouts, and permissions as attributes on `partial class Build`
- quartznet/quartznet `build/Build.DatabaseSchema.cs` and `build/Build.DatabaseMigrations.cs` are build targets named for the database schema and migration scripts, next to a `database/` directory at the repository root
- open-telemetry/opentelemetry-dotnet `build/docker-compose.net8.0.yml`, `.net9.0.yml`, and `.net10.0.yml` are one Docker Compose file per target framework, in the build directory
- elastic/elasticsearch-net `build/scripts/*.fs` is a whole build program in F#: `Targets.fs`, `Building.fs`, `Testing.fs`, `Versioning.fs`, and eight more
- SixLabors/ImageSharp `shared-infrastructure` git submodule versions the shared MSBuild files in a separate repository, pinned by submodule commit
- MessagePack-CSharp `Directory.Build.rsp` is an MSBuild response file, which supplies default command-line switches to builds started in that directory
- App-vNext/Polly `eng/$(ProjectType).targets` role file: a project sets `ProjectType` to `Library`, `Test`, or `Benchmark` and the root `Directory.Build.targets` imports the matching file

## [06]-[OFFICIAL_FEATURES]

The SDK, MSBuild, or NuGet provides a feature for each recurring concern, and a hand-rolled script in a `build/` directory is recognizable as one.

| [CONCERN] | [FEATURE] | [DOCUMENTATION] |
| :-- | :-- | :-- |
| Output layout under one directory | `UseArtifactsOutput` / `ArtifactsPath` in `Directory.Build.props`, `dotnet new buildprops --use-artifacts`, `--artifacts-path` on `dotnet build` | https://learn.microsoft.com/dotnet/core/sdk/artifacts-output |
| One version per package across the repository | Central package management: `Directory.Packages.props`, `ManagePackageVersionsCentrally`, `PackageVersion`, `GlobalPackageReference`, `CentralPackageTransitivePinningEnabled`, `VersionOverride` | https://learn.microsoft.com/nuget/consume-packages/central-package-management |
| Breaking-change and applicability checks on a package | Package validation: `EnablePackageValidation`, `PackageValidationBaselineVersion`, `CompatibilitySuppressions.xml`, the validators (baseline, compatible runtime, compatible framework) | https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview |
| ApiCompat outside packaging | Assembly validation, and `Microsoft.DotNet.ApiCompat.Tool` as a global tool | https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview |
| Source and commit information in symbols and packages | Source Link, in the SDK since .NET 8 and on by default for GitHub, Azure Repos, GitLab, Bitbucket, `PublishRepositoryUrl`, `EmbedUntrackedSources` | https://learn.microsoft.com/dotnet/core/compatibility/sdk/8.0/source-link and https://github.com/dotnet/sourcelink#readme |
| Reproducible builds on CI | `ContinuousIntegrationBuild`, `Deterministic`, `--artifacts-path` cascaded across `dotnet` commands | https://github.com/dotnet/sourcelink/blob/main/docs/README.md |
| Build-script defect checks | MSBuild BuildCheck, `dotnet build -check`, rules BC0101-BC0302 configured in `.editorconfig`, BC0101 shared output path and BC0102 double writes are the rules a monorepo hits first | https://learn.microsoft.com/dotnet/core/tools/buildcheck-rules/ and https://github.com/dotnet/msbuild/blob/main/documentation/specs/BuildCheck/Codes.md |
| Pinning the tools a repository builds with | Local tool manifest `.config/dotnet-tools.json`, `dotnet new tool-manifest`, `dotnet tool restore`, `dnx <tool>` | https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use |
| Pinning the SDK | `global.json` with `sdk.version` and `rollForward` | https://learn.microsoft.com/dotnet/core/tools/global-json |
| The project set | `.slnx`, an XML solution, `dotnet sln migrate` converts, `dotnet new sln` defaults to `.slnx` in .NET 10, `.slnf` solution filters still apply and must be repointed at the `.slnx` | https://learn.microsoft.com/dotnet/core/tools/dotnet-sln and https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/ |
| A small build step written in C# | File-based apps: `dotnet run file.cs`, with `#:package`, `#:project`, `#:property`, `#:sdk`, `#:include` directives and a Unix shebang | https://learn.microsoft.com/dotnet/core/sdk/file-based-apps |
| Analyzer severity and configuration | `.editorconfig` and `.globalconfig` analyzer configuration files | https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files |
| Locked restore | `RestorePackagesWithLockFile`, `packages.lock.json`, `RestoreLockedMode` | https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files |
| Restricting which source serves which package | `packageSourceMapping` in `NuGet.config` | https://learn.microsoft.com/nuget/consume-packages/package-source-mapping |
| Running tests | Microsoft.Testing.Platform, selected by `test.runner` in `global.json` | https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro |

Practices in the survey that the SDK does not provide, and that stay hand-written wherever they appear: version computation from git history (Nerdbank.GitVersioning, MinVer, or Arcade), native toolchain acquisition, code generation from external API descriptions, release-note and changelog assembly, CI job partitioning, and the build driver itself.

Rasm uses the official feature for `ArtifactsPath` in `Directory.Build.props`, `Directory.Packages.props`, `RestorePackagesWithLockFile` with `RestoreLockedMode` under CI, `ContinuousIntegrationBuild` under CI, `PublishRepositoryUrl` with `IncludeSymbols` and `SymbolPackageFormat` snupkg, `global.json` with `rollForward` disable and the Microsoft.Testing.Platform runner, `.config/dotnet-tools.json`, `Workspace.slnx`, `packageSourceMapping` in `NuGet.config`, and `.editorconfig` for analyzer severity and BuildCheck.

## [07]-[CONCERN_PRACTICE_TOOL_REPOSITORIES]

Each item names the concern, the recurring practice, the tool, and the repositories:
- Version pinning by `eng/Versions.props` with `eng/Version.Details.xml`, updated by automated pull request (Arcade, darc, Maestro): runtime, sdk, msbuild, roslyn, aspire, efcore, maui, yarp
- Version pinning by `version.json` at the repository root (Nerdbank.GitVersioning): xunit, CommunityToolkit/dotnet, uno
- Version pinning by a `PackageReference` and `MinVerMinimumMajorMinor` in the shared props (MinVer): Polly
- Version pinning by a plain MSBuild property file (MSBuild): serilog (`Directory.Version.props`), Avalonia (`build/SharedVersion.props`)
- Version pinning by one `Directory.Packages.props` at the root (NuGet central package management): Avalonia, opentelemetry-dotnet, Polly, marten, orleans, OrchardCore, quartznet, MessagePack-CSharp
- Version pinning by `.config/dotnet-tools.json` restored before the build (dotnet local tools): Polly, quartznet, xunit, and elasticsearch-net at the root
- Shared MSBuild in a build directory of `.props`/`.targets` imported by product projects (MSBuild): runtime, sdk, msbuild, roslyn, aspire, efcore, maui, yarp, Polly, opentelemetry-dotnet, Avalonia, Silk.NET, CommunityToolkit, ImageSharp
- Shared MSBuild as one file per project role, selected by an MSBuild property (MSBuild): Polly (`eng/$(ProjectType).targets`)
- Shared MSBuild with target framework sets as named properties (MSBuild): opentelemetry-dotnet (eight `TargetFrameworks*` properties), yarp (`TFMs.props`: `LatestDevTFM`, `ReleaseTFMs`, `TestTFMs`), Avalonia (`build/TargetFrameworks.props`: `AvsCurrentTargetFramework` and platform variants)
- Shared MSBuild with the build directory's own project isolated by an empty `Directory.Build.props` (MSBuild): marten, quartznet
- Analyzer configuration as `.globalconfig`, `.ruleset`, `stylecop.json`, or a banned-symbols list in the build directory (Roslyn analyzers): runtime, msbuild, yarp, roslyn, sdk, opentelemetry-dotnet, maui, Polly
- Native and third-party builds as CMake toolchain files, submodules, SDK installers under the build directory (CMake, bash, PowerShell): runtime, maui, Silk.NET, uno
- Code generation as generator targets and committed generator inputs in the build directory (MSBuild, C#, Roslyn): runtime, roslyn, Silk.NET, uno, maui, aspire
- Test infrastructure as shared test `.props`/`.targets`, `.runsettings`, coverage and runner configuration in the build directory (xunit, coverlet, ReportGenerator, Helix, XHarness): runtime, aspire, efcore, maui, msbuild, sdk, Polly, opentelemetry-dotnet, Avalonia, uno
- Packaging and publishing with `Publishing.props` next to the version files (Arcade SDK): runtime, sdk, msbuild, roslyn, aspire, efcore, maui, yarp
- Packaging and publishing with package metadata and package validation in a shared library `.targets` (.NET SDK pack, package validation): Polly, Avalonia, CommunityToolkit
- Packaging and publishing with distribution beyond NuGet (Homebrew, Nix, winget) (shell, PowerShell): aspire
- CI helpers as a `build.sh` with `build.ps1`/`build.cmd` pair (bash, PowerShell): runtime, sdk, msbuild, roslyn, aspire, maui, yarp, Avalonia, marten, quartznet, Silk.NET, elasticsearch-net, Polly, orleans, xunit
- CI helpers as a compiled build program the entry script invokes (Nuke in Avalonia, marten, Silk.NET, Fallout in quartznet, plain console in xunit and uno, F# in elasticsearch-net): Avalonia, marten, quartznet, Silk.NET, uno, xunit, elasticsearch-net
- CI helpers with Cake as the build driver (Cake, as a file-based C# app in Polly): Polly (`cake.tool` in the tool manifest, root `cake.cs` run by `dotnet cake.cs`), maui (`eng/cake/`)
- CI helpers as pipeline or workflow YAML inside the build directory (Azure Pipelines, GitHub Actions): runtime, sdk, roslyn, aspire, maui, uno
- CI helpers as workflows generated from the build program (Fallout, Nuke): quartznet
- Documentation generation by a docfx entry point or target (docfx): opentelemetry-dotnet (`build/docfx.cmd`), xunit (`docfx/`), elasticsearch-net (`docfx/`), uno (`run-doc-generation.cmd`)
- Public API as exclusion lists, shipped/unshipped API files, package diff ignores (ApiCompat, Microsoft.CodeAnalysis.PublicApiAnalyzers): runtime, Avalonia, Polly, opentelemetry-dotnet, Silk.NET, uno, maui, msbuild
- Release automation as version bump, changelog, release-note scripts in the build directory (PowerShell, F#, C#): Polly, opentelemetry-dotnet, elasticsearch-net, aspire, quartznet, Silk.NET
- Source of truth for tool versions in the tool manifest, with `global.json` for the SDK (dotnet local tools, `global.json`): Polly, quartznet, xunit, elasticsearch-net, and every repository with `global.json`

## [08]-[ADOPTION_VERDICTS]

Verdicts apply the plan's decisions of 2026-09-03:
- Build directory of `.props`/`.targets` imported by product projects, one file per concern — Adopt, 14 of 23 repositories, no Microsoft dependency, and Rasm does this under `eng/native/` and not for the main tree
- One shared file per project role, selected by an MSBuild property — Adopt, Polly's `eng/$(ProjectType).targets`, and Rasm computes `RasmRole` in `Directory.Build.targets` and branches on it
- The build directory's own project isolated by an empty `Directory.Build.props` — Adopted, marten and quartznet do it, and Rasm's `eng/native/Directory.Build.props` is the same device
- Target framework sets as named properties in the shared props — Adopt, opentelemetry-dotnet, yarp, Avalonia, no Microsoft dependency
- A `dotnet tool` manifest as the source of truth for repository tooling — Adopted, contents decided, the manifest holds `binlogtool`, `dotnet-reportgenerator-globaltool`, `dotnet-stryker`, `RoslynCodeLens.Mcp`, and `Microsoft.AITools.BinlogMcp`, with `rollForward` on the two net8 tools run through `dotnet tool run`
- Package validation with a baseline version, in the shared library targets — Not adopted, no Rasm package publishes to a registry, and no released baseline exists
- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` — Not adopted, nothing publishes
- Version computation from git history (Nerdbank.GitVersioning or MinVer) — Open, round six, the repository gets releases and tags on GitHub and the build reads its version from the tag, and the tool that reads it is the open item
- A compiled build program in place of shell scripts — Not adopted, Nx is the task runner and `eng/scripts/` holds the steps with control flow
- Committed generator inputs next to the generator — Adopted, Silk.NET's `build/cache/` and typemaps, and Rasm's `eng/native/<library>/*.json` is the same shape
- Release automation scripts (changelog, version bump, release notes) in the build directory — Not adopted, releases are GitHub releases and tags, and the code, contracts, and schema carry no version
- A shared test `.targets` holding the test package set and coverage configuration — Adopt, Polly's `eng/Test.targets`, Rasm attaches `xunit.analyzers` by role in `Directory.Build.targets` and nothing else, and coverage merges into one report per language with no threshold
- `build.sh` with `build.ps1` entry-point pair — Skip, 15 of 23 have it, and its job in Rasm is done by Nx targets and `uv run`
- Arcade (`eng/common/`, `Version.Details.xml`, `darc`, `Maestro`) — Microsoft-tied, requires Maestro subscriptions and the internal build service, 8 of 8 users are Microsoft-run, 0 of 15 others
- Helix (`helix.proj`, `helixpublish.proj`, XHarness runner templates) — Microsoft-tied, requires the Helix test distribution service
- VMR synchronization (`vmr-sync.sh`), source-build properties (`DotNetBuildSourceOnly`) — Microsoft-tied, meaningful inside the .NET product build alone
- Internal feed setup (`SetupNugetSources.sh`, `internal-feed-operations.sh`), `darc-init` — Microsoft-tied, requires internal Azure DevOps feeds
- IBC and optimization data (`UsingToolIbcOptimization`, `eng/optimizationData.targets`) — Microsoft-tied, requires Microsoft-collected profile data
- CI job runsheet builders and quarantine pipelines — Microsoft-tied in practice, aspire alone, sized for a suite that needs partitioning across many CI machines
- `Publishing.props` and the Arcade publishing pipeline — Microsoft-tied, depends on the Arcade SDK and the publishing infrastructure

## [09]-[SETTLED_AND_OPEN]

Settled by the plan:
- No `libs/` package publishes to a registry, the code, contracts, and schema carry no version, and package validation, public API tracking, and changelog scripts are out of scope
- The repository gets releases and tags on GitHub, and the build reads its version from the tag
- `.config/dotnet-tools.json` holds `binlogtool`, `dotnet-reportgenerator-globaltool`, `dotnet-stryker`, `RoslynCodeLens.Mcp`, and `Microsoft.AITools.BinlogMcp`, and every Forge-installed .NET tool leaves
- CI workflow files live under `.github/` (workflows, composite actions, templates, `CODEOWNERS`) with GitHub Actions as the host, and no `eng/pipelines/` directory is created
- Nx is the task runner, `eng/scripts/` holds the steps with control flow, and no build driver program or `build.sh` pair is added
- `dotnet test` results land under `.artifacts/dotnet` through `ArtifactsPath` and the Microsoft.Testing.Platform defaults, and coverage merges into one Cobertura report with no threshold

Open, the answer changes the design:
1. Which tool reads the git tag into the build version (round six, release versioning tool)
2. Whether the main-tree policy in the 171-line root `Directory.Build.targets` splits into one `eng/<Role>.targets` per `RasmRole` value on the Polly shape, so that `eng/` holds shared MSBuild for the main tree beside `eng/native/`
