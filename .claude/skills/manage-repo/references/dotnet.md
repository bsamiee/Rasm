# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades move each `Directory.Packages.props` row holding a version, prereleases included, a row holding a range stays
- `dotnet package update` moves rows a project references alone, range rows included, and takes prereleases only for a row on a prerelease
- `dotnet package update` fails its whole preview restore on one incompatible release
- Version edits splice the attribute text in place, `dotnet package update` and `XmlDocument` or `ProjectRootElement` saves collapse row alignment
- Newest release is the listed, framework-compatible release ranked by numeric version, then stable, then latest publish date
- Rows move down when their current version is unlisted or incompatible with the repository target framework
- `global.json` `sdk.version` moves by hand to the newest `mise ls-remote dotnet` entry
- SDK moves enable the new SDK's analyzer rules through `latest-all`

## [02]-[FILE_BASED_APPS]

C# file-based apps inherit `Directory.Build.props`, `Directory.Build.targets`, central package versions, and `.editorconfig` through a virtual project named after the entry file:
- Entry files open with `#!`, the marker Roslyn's file-based program discovery reads, included files hold none
- `dotnet build`, `dotnet restore`, and `dotnet format` take one entry file per call, a second file hands every argument to MSBuild
- `ArtifactsPath` defaults `IncludeProjectNameInArtifactsPaths` to true, placing output under `bin/<file>.cs/` and `obj/<file>.cs/`
- Solution files hold no entry file, `dotnet sln add` rejects one and `dotnet build` restores its virtual project itself
- Roslyn virtual projects (`dotnet format`, C# language server) set `IncludeProjectNameInArtifactsPaths` false before `Directory.Build.props` and SDK's bundled framework after it
- `#:property` directives follow the bundled framework, so `#:property TargetFramework=$(RepoTargetFramework)` holds every tool at the repository framework
- `dotnet msbuild` rejects a `.cs` file with `MSB4025`
- `dotnet format` missing `project.assets.json` prints `Required references did not load`, skips analysis, and exits 0
- Builds run code-style analyzers under `EnforceCodeStyleInBuild`, failing an app on formatting and style findings

## [03]-[PROJECTS]

- Use `README.md` for the group a library joins
- Shared source no library owns is one internal file each consumer compiles through its own `Compile` item, a root item reaches projects that never use it
- `dotnet` targets name their binlog from `NX_TASK_TARGET_PROJECT` and `NX_TASK_TARGET_TARGET`
- Targets declare their binlog an output

## [04]-[CHECKERS]

- `Directory.Build.rsp` turns on BuildCheck
- IDE design-time builds, `dotnet format`, and Roslyn workspaces read no `Directory.Build.rsp`, a setting they need sits in `Directory.Build.props`
- File-based app builds read only `-m` and `-mt` from `Directory.Build.rsp` and reject `-check` as an argument, their virtual projects run no BuildCheck
- C# API bans sit in `tools/ast-grep/rules/csharp` alone, a `BannedSymbols.txt` list splits their policy
