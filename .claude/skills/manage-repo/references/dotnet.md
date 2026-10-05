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
