# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[DECLARATIONS]

- Use `dotnet-msbuild-evaluation` for the file and order a declaration takes
- Use `dotnet-msbuild-packaging` for central versions, restore, `NuGet.config`, and `.slnx`
- Project files hold references and the properties tree position cannot derive

## [02]-[UPGRADES]

- Upgrades move the central rows the analyzed project references and no other
- Catalog projects take `.proj` and stay out of the solution, no plugin infers a project from them and no restore resolves every central row
- Rows holding a range stay
- Newest release is the listed, framework-compatible version with the highest numeric version, then stable, then the latest publish date
- Rows move down when the current version is unlisted or incompatible with the analyzed project's framework
- `global.json` moves by hand, `sdk.version` to the newest `mise ls-remote dotnet` entry and `msbuild-sdks` rows to the newest NuGet version
- SDK moves prove through a build diffed against a baseline build on the old SDK, `latest-all` enables the new SDK's rules
- Analyzer rules contradicting a form the repository requires take an `.editorconfig` row, the analyzer source proves the contradiction
