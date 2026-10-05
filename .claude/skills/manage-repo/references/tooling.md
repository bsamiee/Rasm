# [TOOLING]

mise owns tool binaries and the process environment.

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- MCP stdio rows in `.mcp.json` and `.codex/config.toml` start project tools through `mise exec --`, a client's launch environment can lack `[env]` and `_.path`
- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pypi:<package>`, `npm:<package>`)
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- `idiomatic_version_file_enable_tools` names the tools with a version file mise reads, `global.json` for `dotnet`
- `dotnet.isolated = true` installs each SDK under its own root and `dotnet` host, macOS kills a shared-root host a new SDK overwrote
- Isolated roots hold the SDK's runtime alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- `MSBUILDDISABLENODEREUSE = "1"` ends MSBuild worker nodes with each command and starts no MSBuild server
- `NUGET_PACKAGES` relocates the package folder for every run, `NuGet.config` reaches runs with a working directory inside the repository
- Editor settings name a mise install by its `latest` link and a Homebrew install by its `opt` link, a server reading mise's environment takes neither
- `mise upgrade` moves each `latest` row, a `pypi:` row from a GitHub repository with no release stays at the branch HEAD it installed

## [02]-[ENVIRONMENT]

- `{{config_root}}` renders the directory of `mise.toml`, `_.path` prepends directories to PATH
- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution
- Rows render in file order, later rows read a shared value from one earlier `[env]` row as `{{ env.<NAME> }}`
- Homebrew paths render `exec(command='brew --prefix <formula>')` under `os() == 'macos'`, a host without `brew` on PATH fails the whole template
