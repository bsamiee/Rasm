# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades move every central row holding a version, prereleases included, and read the rows from `Directory.Packages.props` itself
- Rows holding a range stay
- `dotnet package update` moves rows a project references alone, range rows included, and takes prereleases only for a row on a prerelease
- `dotnet package update` fails its whole preview restore on one incompatible release
- Version edits splice the attribute text in place, `dotnet package update`, `XmlDocument`, and `ProjectRootElement` saves collapse row alignment
- Newest release is the listed, framework-compatible release ranked by numeric version, then stable, then latest publish date
- Rows move down when their current version is unlisted or incompatible with the repository target framework
- `global.json` `sdk.version` moves by hand to the newest `mise ls-remote dotnet` entry
- SDK moves enable the new SDK's analyzer rules through `latest-all`
- Analyzer rules contradicting a form the repository requires take an `.editorconfig` row
