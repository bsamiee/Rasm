# [DOTNET]

MSBuild directory files and central package versions own every shared .NET declaration.

## [01]-[DECLARATIONS]

- Use `dotnet-msbuild-evaluation` for the file and order a declaration takes
- Use `dotnet-msbuild-packaging` for central versions, restore, `NuGet.config`, and `.slnx`
- Project files hold references and the properties tree position cannot derive

## [02]-[UPGRADES]

- `dotnet-outdated` moves the central rows the analyzed project references and no other, `--no-restore` writes them in place of `dotnet add package`
