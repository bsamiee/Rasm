# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades move each `Directory.Packages.props` row holding a version, prereleases included, and an exact `[<version>]` row inside its major.minor
- Rows holding any other range stay
- Releases come from nuget.org, the one `NuGet.config` source, a per-commit build on a dnceng feed (`dotnet-tools`) is no release
- Version edits splice attribute text in place, `XmlDocument` or `ProjectRootElement` saves collapse row alignment
- Newest release is the highest listed framework-compatible release by `NuGetVersion` across base versions, a stable release above its prereleases
- Package metadata and downloads read NuGet's HTTP cache under its default age
- Rows move down when their current version is unlisted or incompatible with the repository target framework
- `global.json` `sdk.version` moves by hand to the newest `mise ls-remote dotnet` entry
- SDK moves enable the new SDK's analyzer rules through `latest-all`, rules it leaves disabled (CA1021, CA1045) take their own `.config/analyzers.globalconfig` rows

## [02]-[FILE_BASED_APPS]

C# file-based apps inherit `Directory.Build.*`, central package versions, and `.editorconfig` through a virtual project named after the entry file:
- Entry files open with `#!`, the marker Roslyn's file-based program discovery reads, included files hold none
- `dotnet build`, `dotnet restore`, and `dotnet format` take one entry file per call, a second file hands every argument to MSBuild
- `ArtifactsPath` defaults `IncludeProjectNameInArtifactsPaths` to true, placing output under `bin/<file>.cs/` and `obj/<file>.cs/`
- Solution files hold no entry file, `dotnet sln add` rejects one and `dotnet build` restores its virtual project itself
- Roslyn virtual projects (`dotnet format`, C# language server) set `IncludeProjectNameInArtifactsPaths` false before `Directory.Build.props`
- `#:property TargetFramework=$(RepoTargetFramework)` sets every tool's framework over the bundled one Roslyn sets after `Directory.Build.props`
- `dotnet msbuild` rejects a `.cs` file with `MSB4025`
- `dotnet format` missing `project.assets.json` prints `Required references did not load`, skips analysis, and exits 0
- Builds run code-style analyzers under `EnforceCodeStyleInBuild`, failing an app on formatting and style findings

## [03]-[PROJECTS]

- `dotnet` targets name their binlog from `NX_TASK_TARGET_PROJECT` and `NX_TASK_TARGET_TARGET` and declare it an output
- Projects holding `Windows/` sources take `platform:windows` and a `check` prerequisite `build:windows` compiling them for `win-x64`
- Builds for a second runtime take their own `ArtifactsPath`, their restore rewrites `project.assets.json` for that runtime

## [04]-[CHECKERS]

- `Directory.Build.rsp` enables BuildCheck
- IDE design-time builds, `dotnet format`, and Roslyn workspaces read no `Directory.Build.rsp`, a setting they need sits in `Directory.Build.props`
- File-based app builds read only `-m` and `-mt` from `Directory.Build.rsp` and reject `-check`, their virtual projects run no BuildCheck
- MSBuildWorkspace warns on a referenced project lacking a reference assembly, `ProduceReferenceAssembly` keeps `dotnet format` loads clean
- C# symbol bans sit in `tools/roslyn/BannedSymbols.txt`, `BannedSymbols.Library.txt` beside it holds bans for projects under `libs/`

## [05]-[ANALYZERS]

Roslyn analyzers sit in `tools/roslyn/Rasm.Analyzers`, code fixes in `tools/roslyn/Rasm.Analyzers.CodeFixes`:
- Analyzer projects target `netstandard2.0`, authoring packages match the Roslyn API every consuming host loads
- Analyzer projects take no workspace language packages, project usings, or transitive pinning, holding Roslyn's dependency floor for every host
- Rules resolve package types through `Compilation.GetTypeByMetadataName` over consumer references, analyzer projects reference no checked package
- Shared references add analyzer outputs and keep ordinary compile references
- Semantic formatting targets schedule analyzer builds
- Analyzer references undefine `RhinoPlugIn` and `ArtifactsPivots`, plug-in library copies share one analyzer build
- Code fix providers and `Microsoft.CodeAnalysis.CSharp.Workspaces` stay in the code fix project, RS1038 rejects Workspaces beside analyzer types
- Tests check a reviewed Verify snapshot of each rule's diagnostics and fixed source
- Diagnostics fail builds through the `.config/analyzers.globalconfig` severity floor, a descriptor's default severity sets no exemption
- Global config `none` skips running an analyzer, an `.editorconfig` `none` still runs it
- Rules a package disables by default and source generator diagnostics (Mapperly `RMG`) escape the floor and take their own rows
- Rule options (`MA0053.public_class_should_be_sealed`) widen what a rule reports

## [06]-[RHINO_HOSTS]

Installed Rhino owns the host assembly set, its target framework, and how each UI reads an icon:
- `ReadRhinoHost` reads JSON through `JsonReaderWriterFactory`, `RoslynCodeTaskFactory` compiles against `netstandard` with no `System.Text.Json`
- Packages with an assembly in the host folder leave copy-local whole, an assembly version above the one `dotnetstart.*.deps.json` declares, or the host file's where it declares none, fails the build
- `[<version>]` rows under `Label="Rhino Host"` hold packages the catalog sets above a host assembly version, and every package requiring them
- SVG masters paint with `currentColor`, `var(--accent)`, and `var(--edge)`, `BuildThemedIcons` writes each mode's values into an embedded `<IconStem>.svg` and `<IconStem>_darkmode.svg`
- `EtoAssets` selects a `_darkmode` resource by name and renders inline `fill-dark` and `stroke-dark` as their light colors
- `ToolbarFile.GetSvg` returns `<light>` for both modes, a `<dark>` element changes no toolbar icon
- `Rasm.Rui` merges each dark color into `<light>` as `fill-dark` or `stroke-dark`
- Rhino opens `<plug-in>.rui` in place beside the registered `.rhp`, an installed package's from its version folder
- `yak build` adds a `guid:` keyword for Grasshopper 2 plug-ins alone, `BuildYakPackage` sets it on every `yak spec` manifest
- Installed packages stall a hidden launch at `Preparing Plug-ins for First Use`, a harness case loads the `.rhp` through `plugin`
