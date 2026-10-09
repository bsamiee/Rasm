# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades read central rows from `Directory.Packages.props` and move each row holding a version, prereleases included
- Rows holding a range stay
- `dotnet package update` moves rows a project references alone, range rows included, and takes prereleases only for a row on a prerelease
- `dotnet package update` fails its whole preview restore on one incompatible release
- Version edits splice the attribute text in place, `dotnet package update` and `XmlDocument` or `ProjectRootElement` saves collapse row alignment
- Newest release is the listed, framework-compatible release ranked by numeric version, then stable, then latest publish date
- Rows move down when their current version is unlisted or incompatible with the repository target framework
- `global.json` `sdk.version` moves by hand to the newest `mise ls-remote dotnet` entry
- SDK moves enable the new SDK's analyzer rules through `latest-all`
- Analyzer rules contradicting a form the repository requires take an `.editorconfig` row

## [02]-[FILE_BASED_APPS]

C# file-based apps inherit `Directory.Build.props`, `Directory.Build.targets`, central package versions, and `.editorconfig` through a virtual project named after the entry file:
- Entry files open with `#!`, the marker Roslyn's file-based program discovery reads, included files hold none
- `dotnet build`, `dotnet restore`, and `dotnet format` take one entry file per call, a second file hands every argument to MSBuild
- `ArtifactsPath` with `IncludeProjectNameInArtifactsPaths` places output under `bin/<file>.cs/` and `obj/<file>.cs/`
- Solution files hold no entry file, `dotnet sln add` rejects one
- `dotnet build` restores an entry file's virtual project itself, a solution restore covers none
- Roslyn's virtual projects (`dotnet format`, the C# language server) set `IncludeProjectNameInArtifactsPaths` false before `Directory.Build.props` and SDK's bundled framework after it
- `#:property` directives follow the bundled framework, so `#:property TargetFramework=$(RepoTargetFramework)` holds every tool at the repository framework
- `dotnet msbuild` rejects a `.cs` file with `MSB4025`
- `dotnet format` missing `project.assets.json` prints `Required references did not load`, skips analysis, and exits 0
- Builds run code-style analyzers under `EnforceCodeStyleInBuild`, failing an app on formatting and style findings
