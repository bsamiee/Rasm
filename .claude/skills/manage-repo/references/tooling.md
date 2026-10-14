# [TOOLING]

mise owns tool binaries, the process environment, and local services.

## [01]-[TOOLS]

- Processes inherit mise's activated environment from their launch shell and call tools by name
- `[tools]` rows name a registry short name or a backend (`github:<owner>/<repo>`, `pypi:<package>`, `npm:<package>`)
- `http:<tool>` rows install a release source archive for a tool with no registry entry and no release asset
- `http:` rows reading a releases API set `version_order = "semver"`, mise takes the last entry of a listing ordered newest first
- `prereleases = true` includes prereleases in `latest`, `minimum_release_age = "0s"` removes the 24h delay on a new release
- `registry_floating = true` reads current mise and aqua registries, snapshots baked into the mise binary answer when no cached copy loads
- `idiomatic_version_file_enable_tools` names the tools with a version file mise reads, `global.json` for `dotnet`, `rust-toolchain.toml` for `rust`
- `dotnet.isolated = true` installs each SDK under its own root and `dotnet` host, macOS kills a shared-root host a new SDK overwrote
- Isolated roots hold the SDK's runtime alone, `DOTNET_ROLL_FORWARD = "Major"` runs an older-major tool on it
- `MSBuildLocator` skips an SDK newer than its host runtime, a tool loading MSBuild rolls forward to the SDK's runtime
- `MSBUILDDISABLENODEREUSE = "1"` ends MSBuild worker nodes with each command and starts no MSBuild server
- `NuGet.config` `globalPackagesFolder` relocates packages for runs with repository working directories, including editor restores
- HTTP cache has no NuGet config key and keeps its `mise.toml` row
- Editor settings name a mise install by its `latest` link and a Homebrew install by its `opt` link, a server reading mise's environment takes neither
- `mise upgrade` moves each `latest` row and leaves an exact row, `mise upgrade --bump <tool>` rewrites one exact row and keeps its options, a `pypi:` row from a GitHub repository with no release stays at the branch HEAD it installed

## [02]-[ENVIRONMENT]

- `{{config_root}}` renders the directory of `mise.toml`, `_.path` prepends directories to PATH
- `tools = true` on a row renders `tools.<name>.version` and `tools.<name>.path` after tool resolution
- Rows render in file order, later rows read a shared value from one earlier `[env]` row as `{{ env.<NAME> }}`
- Homebrew paths name the formula's `opt` link
- `exec(command=...)` templates fail every mise call on a host without their command on PATH
- `[env]` holds values shells and repository programs read, a value one local service alone reads sits on its launchd row

## [03]-[SERVICES]

Launchd agents run each local MCP server once as a Streamable HTTP service on a loopback port, harness rows hold the service URL:
- Ports a repository program also reads sit once in `[vars]`, rendered into the launchd row and a `<NAME>_MCP_URL` `[env]` row
- Harnesses read no `[env]` value into MCP config
- `[bootstrap.macos.launchd.agents]` rows run `{{ mise_bin }}` with `args` opening `exec --` and `working_directory` at `{{config_root}}`
- `keep_alive` restarts an exited process, and launchd starts a `KeepAlive` agent at login with no `RunAtLoad`
- Row `environment` tables render into plist `EnvironmentVariables`
- Row values render no `exec(command=...)` template
- Programs on no PATH mise builds take an absolute path in `args`, `args` expand no `~`
- Agents inherit the GUI domain environment, and `exec --` adds repository `[env]` and tool paths
- `mise bootstrap macos launchd-agents apply` writes and loads `~/Library/LaunchAgents/dev.mise.<name>.plist`, launchd discards an agent's output
- `mise bootstrap macos launchd-agents status` lists each agent's state, `launchctl print gui/$UID/dev.mise.<name>` its pid and last exit code
- Servers speaking stdio alone, or exposing a tool a skill replaces, run over stdio behind `mcp-bridge`
- `--hide <glob>` before `mcp-bridge`'s `--` leaves each server tool it matches out of the endpoint, repeated per glob
- Servers starting one process per connection, or approving the connecting process by its code signature, keep a stdio row
