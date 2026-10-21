# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades move each `Directory.Packages.props` row holding a version, prereleases included, a row holding a range stays
- Releases come from nuget.org, the one `NuGet.config` source, a per-commit build on a dnceng feed (`dotnet-tools`) is no release
- Version edits splice attribute text in place, `XmlDocument` or `ProjectRootElement` saves collapse row alignment
- Newest release is the highest listed framework-compatible release by `NuGetVersion` across base versions, a stable release above its prereleases
- Prereleases of one base version rank by `NuGetVersion` where the first differing label is numeric on both sides, else by feed publish time
- Package metadata and downloads read NuGet's HTTP cache under its default age
- Rows move down when their current version is unlisted or incompatible with the repository target framework
- `global.json` `sdk.version` moves by hand to the newest `mise ls-remote dotnet` entry
- SDK moves enable the new SDK's analyzer rules through `latest-all`, rules it leaves off (CA1021, CA1045) take their own `.editorconfig` rows

## [02]-[FILE_BASED_APPS]

C# file-based apps inherit `Directory.Build.*`, central package versions, and `.editorconfig` through a virtual project named after the entry file:
- Entry files open with `#!`, the marker Roslyn's file-based program discovery reads, included files hold none
- `dotnet build`, `dotnet restore`, and `dotnet format` take one entry file per call, a second file hands every argument to MSBuild
- `ArtifactsPath` defaults `IncludeProjectNameInArtifactsPaths` to true, placing output under `bin/<file>.cs/` and `obj/<file>.cs/`
- Solution files hold no entry file, `dotnet sln add` rejects one and `dotnet build` restores its virtual project itself
- Roslyn virtual projects (`dotnet format`, C# language server) set `IncludeProjectNameInArtifactsPaths` false before `Directory.Build.props`
- Roslyn virtual projects set the SDK's bundled framework after `Directory.Build.props`
- `#:property` directives follow the bundled framework, `#:property TargetFramework=$(RepoTargetFramework)` sets every tool's framework
- `dotnet msbuild` rejects a `.cs` file with `MSB4025`
- `dotnet format` missing `project.assets.json` prints `Required references did not load`, skips analysis, and exits 0
- Builds run code-style analyzers under `EnforceCodeStyleInBuild`, failing an app on formatting and style findings

## [03]-[PROJECTS]

- `dotnet` targets name their binlog from `NX_TASK_TARGET_PROJECT` and `NX_TASK_TARGET_TARGET` and declare it an output
- MTP tasks pass one result root, SDK per-module layout places outputs beneath it

## [04]-[CHECKERS]

- `Directory.Build.rsp` turns on BuildCheck
- IDE design-time builds, `dotnet format`, and Roslyn workspaces read no `Directory.Build.rsp`, a setting they need sits in `Directory.Build.props`
- File-based app builds read only `-m` and `-mt` from `Directory.Build.rsp` and reject `-check`, their virtual projects run no BuildCheck
- MSBuildWorkspace warns on a referenced project lacking a reference assembly, `ProduceReferenceAssembly` keeps `dotnet format` loads clean
- C# symbol bans sit in `tools/roslyn/BannedSymbols.txt`, `BannedSymbols.Library.txt` beside it holds bans for projects under `libs/`

## [05]-[ANALYZERS]

Roslyn analyzers sit in `tools/roslyn/Rasm.Analyzers`, code fixes in `tools/roslyn/Rasm.Analyzers.CodeFixes`:
- Analyzer projects target `netstandard2.0`, authoring packages match the Roslyn API every consuming host loads
- Analyzer projects take no workspace language packages, project usings, or transitive pinning, holding Roslyn's dependency floor for every host
- Rules resolve package types through `Compilation.GetTypeByMetadataName` over consumer references, analyzer projects reference no checked package
- Shared references add analyzer outputs while retaining existing ordinary compile references
- Semantic formatting targets schedule analyzer builds
- Analyzer references undefine `RhinoPlugIn` and `ArtifactsPivots`, plug-in library copies share one analyzer build
- Code fix providers and `Microsoft.CodeAnalysis.CSharp.Workspaces` stay in the code fix project, RS1038 rejects Workspaces beside analyzer types
- New rules join `Rasm.Analyzers.Tests` as a `Samples/<RULE>/` folder of `.txt` samples with `{|ID:...|}` markup and no harness edit
- Tests check a reviewed Verify snapshot of each rule's diagnostics and fixed source
- Diagnostics fail builds through the `.editorconfig` severity floor, a descriptor's default severity sets no exemption
- Source generator diagnostics (Mapperly `RMG`) escape the floor and take their own rows

Diagnostic ids take four digits by domain:
- `RASM1xxx` library rules (LanguageExt, Thinktecture, Mapperly)
- `RASM2xxx` host and build rules (RhinoCommon, Grasshopper, MSBuild)
- `RASM3xxx` language and BCL rules
