# [TOOLING]

mise owns tool binaries, the process environment, and local services.

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pypi:<package>`, `npm:<package>`)
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
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

## [03]-[SERVICES]

Launchd agents run each local MCP server once as a Streamable HTTP service on a loopback port, harness rows hold the service URL:
- `[bootstrap.macos.launchd.agents]` rows run `{{ mise_bin }}` with `args` opening `exec --` and `working_directory` at `{{config_root}}`
- `keep_alive` restarts an exited process, and launchd starts a `KeepAlive` agent at login with no `RunAtLoad`
- Agents inherit the GUI domain environment, machine setup places Homebrew on its PATH for `[env]` templates
- `mise bootstrap macos launchd-agents apply` writes and loads `~/Library/LaunchAgents/dev.mise.<name>.plist`, launchd discards an agent's output
- `mise bootstrap macos launchd-agents status` lists each agent's state, `launchctl print gui/$UID/dev.mise.<name>` its pid and last exit code
- Servers speaking stdio alone run behind `tools/bridge/bridge.ts`
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row
