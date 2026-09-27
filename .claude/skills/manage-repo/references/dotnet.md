# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[UPGRADES]

- Upgrades move central rows the analyzed project references and no other
- Catalog projects take `.proj`, reference every central row, and stay out of the solution, no plugin infers a project from them
- Rows holding a range stay
- Version edits splice the attribute text in place, `XmlDocument` and `ProjectRootElement` saves collapse row alignment
- Newest release is the listed, framework-compatible version with highest numeric version, then stable, then latest publish date
- Rows move down when their current version is unlisted or incompatible with the analyzed project's framework
- `global.json` moves by hand, `sdk.version` to the newest `mise ls-remote dotnet` entry and `msbuild-sdks` rows to their newest NuGet version
- SDK moves enable the new SDK's analyzer rules through `latest-all`
- Analyzer rules contradicting a form the repository requires take an `.editorconfig` row
